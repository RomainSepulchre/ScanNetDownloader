using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
            // TODO: Redo Error Manamgement to fit with WPF
            // TODO: If no custom directory set ask if the user want to select a new one or if he's ok with the one selected

            if (Directory.Exists(OutputDirectory) == false)
            {
                Error.NoOutputDirectory();

                string mBoxMessage = $"\"{OutputDirectory}\" does not exist, do you want to create the directory?";
                string mBoxCaption = "Continue ?";
                Downloader.WriteDlInfoLine(mBoxMessage);
                // TODO: Create a window with CreateDirectory, Choose another directory, Cancel
                MessageBoxResult resultCreateDir = MessageBox.Show(mBoxMessage, mBoxCaption, MessageBoxButton.YesNo, MessageBoxImage.Question);

                if (resultCreateDir == MessageBoxResult.Yes)
                {
                    Directory.CreateDirectory(OutputDirectory);
                    Downloader.WriteDlInfoLine($"\"{OutputDirectory}\" sucessfully created. Ready to download!");
                    return true;
                }
                else
                {
                    mBoxMessage = $"Do you want to select another download directory ?";
                    mBoxCaption = "Select Directory ?";
                    MessageBoxResult resultSelectDir = MessageBox.Show(mBoxMessage, mBoxCaption, MessageBoxButton.YesNo, MessageBoxImage.Question);

                    if(resultSelectDir == MessageBoxResult.Yes)
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
                            Downloader.WriteDlInfoLine(mBoxMessage);
                            MessageBox.Show(mBoxMessage, mBoxCaption, MessageBoxButton.OK, MessageBoxImage.Warning);
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
                        Downloader.WriteDlInfoLine(mBoxMessage);
                        MessageBox.Show(mBoxMessage, mBoxCaption, MessageBoxButton.OK, MessageBoxImage.Warning);
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

        public static bool AreScanFilesDownloaded(ScanData scanData)
        {
            string chapterDirPath = GetChapterDirectoryPath(scanData);
            if (Directory.Exists(chapterDirPath))
            {
                if (Directory.GetFiles(chapterDirPath).Length > 0) // TODO: Improve this to know if we have the correct amount of page, need more info in ScanData
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
