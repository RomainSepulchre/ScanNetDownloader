using Microsoft.Win32;
using ScanNetDownloader.Logic.Helpers;
using ScanNetDownloader.View;
using ScanNetDownloader.View.CustomControls;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows;

namespace ScanNetDownloader.Logic
{
    /// <summary>
    /// Manage everything related to local files and folders
    /// </summary>
    class FileManagement
    {

        private static string OutputDirectory => Settings.Instance.OutputDirectory;

        public static bool OutputDirectoryIsValid()
        {
            // TODO: If no custom directory set ask if the user want to select a new one or if he's ok with the one selected

            if (Directory.Exists(OutputDirectory) == false)
            {
                Error.NoOutputDirectory();

                string mBoxMessage = $"\"{OutputDirectory}\" does not exist, do you want to create the directory?";
                string mBoxCaption = "Create the directory ?";
                // TODO: Create a window with CreateDirectory, Choose another directory, Cancel
                YesNoWindow yesNoWindow_CreateDir = MsgWindow.ShowYesNoWindow(mBoxCaption, mBoxMessage, false, MsgWindow.ImageType.Question);

                if (yesNoWindow_CreateDir.Success)
                {
                    Directory.CreateDirectory(OutputDirectory);
                    return true;
                }
                else
                {
                    mBoxMessage = $"Do you want to select another download directory ?";
                    mBoxCaption = "Select directory ?";
                    YesNoWindow yesNoWindow_SelectDir = MsgWindow.ShowYesNoWindow(mBoxCaption, mBoxMessage, false, MsgWindow.ImageType.Question);

                    if(yesNoWindow_SelectDir.Success)
                    {
                        OpenFolderDialog fileDialog = new OpenFolderDialog();
                        fileDialog.Title = "Select download directory";
                        fileDialog.Multiselect = false;

                        bool? success = fileDialog.ShowDialog();

                        if (success == true)
                        {
                            Settings.Instance.OutputDirectory = fileDialog.FolderName;
                            Settings.Save();

                            return true;
                        }
                        else
                        {
                            mBoxMessage = "Please modify the download directory in the Settings, it must be a valid directory.";
                            mBoxCaption = "Invalid download directory";
                            MsgWindow.ShowOkWindow(mBoxCaption, mBoxMessage, false, MsgWindow.ImageType.Warning);
                            // TODO: Find a cleaner way to do that
                            MainWindow mainWindow = Application.Current.MainWindow as MainWindow;
                            mainWindow.tabCtrlNavigation.SelectedItem = mainWindow.tabOptions;
                            return false;
                        }   
                    }
                    else
                    {
                        mBoxMessage = "Please modify the download directory in the Settings, it must be a valid directory.";
                        mBoxCaption = "Invalid download directory";
                        MsgWindow.ShowOkWindow(mBoxCaption, mBoxMessage, false, MsgWindow.ImageType.Warning);
                        // TODO: Find a cleaner way to do that
                        MainWindow mainWindow = Application.Current.MainWindow as MainWindow;
                        mainWindow.tabCtrlNavigation.SelectedItem = mainWindow.tabOptions;
                        return false;
                    }                   
                }
            }
            else
            {
                return true;
            }     
        }

        public static string CreateChapterDirectory(string bookName, string chapterNumber)
        {
            string chapterDirectory = Path.Combine(OutputDirectory, $"{bookName}{Constants.SCAN_CHAPTER_PATH}{chapterNumber}");

            if (Directory.Exists(chapterDirectory) == false)
            {
                Directory.CreateDirectory(chapterDirectory);
            }
            return chapterDirectory;
        }

        public static void OpenRelevantFolderAfterDownload(List<ScanData> scansToDownload)
        {
            // Called at the end of a download, should I make a isNull check on LocationPath ? it should always be set during download.
            if (scansToDownload.Count == 1)
            {
                // One chapter downloaded, open this chapter folder
                OpenFolder(scansToDownload[0].LocationPath);
            }
            else
            {
                bool moreThanOneBook = scansToDownload.MoreThanOneBookInList();
                if (moreThanOneBook)
                {
                    // Several books downloaded, open the output folder
                    OpenFolder(OutputDirectory);
                }
                else
                {
                    // Several chapters of the same book downloaded, open the book folder
                    string bookDirectory = Path.Combine(scansToDownload[0].LocationPath, "..");
                    OpenFolder(bookDirectory);
                }
            }
        }

        public static void OpenFolder(string folderPath)
        {
            if (Directory.Exists(folderPath))
            {
                ProcessStartInfo startInfo = new ProcessStartInfo
                {
                    Arguments = folderPath,
                    FileName = "explorer.exe",
                };

                Process.Start(startInfo);
            }
        }

        public static ScanItem.DownloadedStatus GetDownloadStatus(ScanData scanData)
        {
            if(scanData.LocationPath != null)
            {
                if(scanData.LocationPath != string.Empty)
                {
                    bool cbzCreated = TryToFindCbzFromLocationPath(scanData, out string cbzPathTried);
                    if (Directory.Exists(scanData.LocationPath))
                    {
                        return AnalyzeLocationPath(scanData.LocationPath, scanData.PagesCount, cbzCreated);
                    }
                    else
                    {
                        if (cbzCreated) return ScanItem.DownloadedStatus.OnlyCbzDownloaded;
                        else return ScanItem.DownloadedStatus.NotDownloaded;
                    }
                }
                else
                {
                    // Path was never set, images cannot have been download             
                    return ScanItem.DownloadedStatus.NotDownloaded; // TODO: Add new state NeverDownloaded ?
                }
            }
            else // Retro-compatibility: Try to find path from current download location
            {
                // Path is null, scan data was created before LocationPath introduction   
                bool cbzCreated = TryToFindCbzFromOutputDir(scanData, out string cbzPathTried);

                if (TryFindDirectoryFromOutputDir(scanData, out string pathFromOutputTried))
                {
                    // Location Path Found
                    scanData.LocationPath = pathFromOutputTried;
                    return AnalyzeLocationPath(scanData.LocationPath, scanData.PagesCount, cbzCreated);
                }
                else if (cbzCreated)
                {
                    // Cbz file found so location path found, save the path from output dir
                    scanData.LocationPath = pathFromOutputTried;
                    return ScanItem.DownloadedStatus.OnlyCbzDownloaded;  
                }
                else
                {
                    // Keep location path null if we didn't find anything in case user change download path later
                    return ScanItem.DownloadedStatus.NotDownloaded;
                }
            }       
        }

        private static ScanItem.DownloadedStatus AnalyzeLocationPath(string scanPath, int expectedImgsCount, bool cbzCreated)
        {
            int filesInChapterDirCount = Directory.GetFiles(scanPath).Length;

            // Are all images downloaded ?
            if (filesInChapterDirCount > 0 && filesInChapterDirCount >= expectedImgsCount) // if more image than expected just consider image are downloaded
            {
                // All images downloaded
                if (cbzCreated) return ScanItem.DownloadedStatus.FullyDownloaded;
                else return ScanItem.DownloadedStatus.OnlyImagesDownloaded;
            }
            else if (filesInChapterDirCount > 0 && filesInChapterDirCount < expectedImgsCount)
            {
                // Some images are missing
                return ScanItem.DownloadedStatus.MissingImages;
            }
            else
            {
                // No Images (but maybe a Cbz)
                if (cbzCreated) return ScanItem.DownloadedStatus.OnlyCbzDownloaded;
                else return ScanItem.DownloadedStatus.NotDownloaded;
            }
        }

        private static bool TryFindDirectoryFromOutputDir(ScanData scanData, out string pathTried)
        {
            // Build path from output directory
            string bookName = scanData.BookName;
            string chapterNumber = scanData.ChapterId.ToString();
            string chapterDirectoryPath = Path.Combine(OutputDirectory, $"{bookName}{Constants.SCAN_CHAPTER_PATH}{chapterNumber}");
            pathTried = chapterDirectoryPath;

            if (Directory.Exists(chapterDirectoryPath)) return true;
            else return false;
        }

        public static bool CbzFileExist(ScanData scanData)
        {
            if (scanData.LocationPath != null)
            {
                if(scanData.LocationPath != string.Empty)
                {
                    return TryToFindCbzFromLocationPath(scanData, out string cbzPathTried);
                }
                else
                {
                    // LocationPath never set
                    return false;
                }
            }
            else
            {
                // Retro-compatibility: LocationPath is null
                return TryToFindCbzFromOutputDir(scanData, out string cbzPathTried);
            }
        }

        private static bool TryToFindCbzFromOutputDir(ScanData scanData, out string cbzPathTried)
        {
            cbzPathTried = string.Empty;

            string bookName = scanData.BookName;
            string chapterNumber = scanData.ChapterId.ToString();
            string bookDirectoryPath = Path.Combine(OutputDirectory, $"{bookName}{Constants.SCAN_SUFFIX}");
            string cbzFilePath = Path.Combine(bookDirectoryPath, $"{bookName}{Constants.CBZ_CHAPTER_PREFIX}{chapterNumber}{Constants.CBZ_EXTENSION}");

            cbzPathTried = cbzFilePath;

            if (File.Exists(cbzFilePath) && File.ReadAllBytes(cbzFilePath).Length > 0) return true;
            else return false;
        }

        private static bool TryToFindCbzFromLocationPath(ScanData scanData, out string cbzPathTried)
        {
            // Ex:
            // Location path =  D:\Download\ScanNetDownloader\One Piece Scan\Chapter 1
            // Cbz Path      =  D:\Download\ScanNetDownloader\One Piece Scan\One Piece - chapter 1.cbz

            string bookName = scanData.BookName;
            string chapterNumber = scanData.ChapterId.ToString();
            string cbzFileName = $"{bookName}{Constants.CBZ_CHAPTER_PREFIX}{chapterNumber}{Constants.CBZ_EXTENSION}";

            string cbzParentDir = Path.Combine(scanData.LocationPath, ".."); // Get parent directory
            string cbzFilePath = Path.Combine (cbzParentDir, cbzFileName);

            cbzPathTried = cbzFilePath;

            if(File.Exists(cbzFilePath) && File.ReadAllBytes(cbzFilePath).Length > 0) return true;
            else return false;
        }

        static Guid folderDownloads = new Guid("374DE290-123F-4565-9164-39C4925E467B");
        [DllImport("shell32.dll", CharSet = CharSet.Auto)]
        private static extern int SHGetKnownFolderPath(ref Guid id, int flags, IntPtr token, out IntPtr path);

        public static string GetUserDownloadsFolder()
        {
            if (Environment.OSVersion.Version.Major < 6)
                return Environment.GetFolderPath(Environment.SpecialFolder.Desktop);// return desktop folder instead

            IntPtr pathPtr = IntPtr.Zero;
            try
            {
                SHGetKnownFolderPath(ref folderDownloads, 0, IntPtr.Zero, out pathPtr);
                return Marshal.PtrToStringUni(pathPtr);
            }
            finally
            {
                Marshal.FreeCoTaskMem(pathPtr);
            }
        }
    }
}
