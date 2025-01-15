using Microsoft.Win32;
using ScanNetDownloader.Logic.Helpers;
using ScanNetDownloader.View;
using ScanNetDownloader.View.CustomControls;
using System.Diagnostics;
using System.IO;
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

        public static void OpenRelevantFolder(List<ScanData> scansToDownload)
        {
            if (scansToDownload.Count == 1)
            {
                // One chapter downloaded, open this chapter folder
                string chapterDirectory = GetChapterDirectoryPath(scansToDownload[0]);
                OpenFolder(chapterDirectory);
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
                    string bookDirectory = GetBookDirectoryPath(scansToDownload[0]);
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

        [Obsolete("Remove after cleaning - use Download statuts instead of this")]
        public static bool AreScanFilesDownloaded(ScanData scanData, bool cbzCreated)
        {
            if (cbzCreated) return true;

            string chapterDirPath = GetChapterDirectoryPath(scanData);
            if (Directory.Exists(chapterDirPath))
            {
                int expectedImgsCount = scanData.PagesCount;
                int filesInChapterDirCount = Directory.GetFiles(chapterDirPath).Length;

                if (filesInChapterDirCount > 0 && expectedImgsCount == filesInChapterDirCount)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                return false;
            }
        }

        public static ScanItem.DownloadedStatus GetDownloadStatus(ScanData scanData, bool cbzCreated)
        {
            string chapterDirPath = GetChapterDirectoryPath(scanData);
            if (Directory.Exists(chapterDirPath))
            {
                int expectedImgsCount = scanData.PagesCount;
                int filesInChapterDirCount = Directory.GetFiles(chapterDirPath).Length;

                if (filesInChapterDirCount > 0 && expectedImgsCount == filesInChapterDirCount)
                {
                    if (cbzCreated) return ScanItem.DownloadedStatus.Downloaded;
                    else return ScanItem.DownloadedStatus.OnlyImages;
                }
                else if (filesInChapterDirCount > 0 && filesInChapterDirCount < expectedImgsCount)
                {
                    return ScanItem.DownloadedStatus.MissingImages;
                }
                else
                {
                    if(cbzCreated) return ScanItem.DownloadedStatus.OnlyCbz;
                    else return ScanItem.DownloadedStatus.NotDownloaded;
                }
            }
            else
            {
                if (cbzCreated) return ScanItem.DownloadedStatus.OnlyCbz;
                else return ScanItem.DownloadedStatus.NotDownloaded;
            }
        }

        public static bool IsCbzArchiveCreated(ScanData scanData)
        {
            string cbzPath = GetCbzFilePath(scanData);
            return File.Exists(cbzPath) && File.ReadAllBytes(cbzPath).Length > 0;
        }

        static string GetBookDirectoryPath(ScanData scanData)
        {
            string bookName = scanData.BookName;
            string bookDirectoryPath = Path.Combine(OutputDirectory, $"{bookName}{Constants.SCAN_SUFFIX}");

            return bookDirectoryPath;
        }

        public static string GetChapterDirectoryPath(ScanData scanData)
        {
            string bookName = scanData.BookName;
            string chapterNumber = scanData.ChapterId.ToString();
            string chapterDirectoryPath = Path.Combine(OutputDirectory, $"{bookName}{Constants.SCAN_CHAPTER_PATH}{chapterNumber}");

            return chapterDirectoryPath;
        }

        static string GetCbzFilePath(ScanData scanData)
        {
            string bookName = scanData.BookName;
            string chapterNumber = scanData.ChapterId.ToString();
            string cbzFilePath = Path.Combine(GetBookDirectoryPath(scanData), $"{bookName}{Constants.CBZ_CHAPTER_PREFIX}{chapterNumber}{Constants.CBZ_EXTENSION}");

            return cbzFilePath;
        }
    }
}
