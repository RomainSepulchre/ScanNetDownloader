using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace ScanNetDownloader.ConsoleApp
{
    class FileManagement
    {

        private static string OutputDirectory => Settings.Instance.OutputDirectory;

        public static void CheckOutputDirectory()
        {
            // TODO: Redo Error Manamgement to fit with WPF
            // TODO: If no custom directory set ask if the user want to select a new one or if he's ok with the one selected

            if (Directory.Exists(OutputDirectory) == false)
            {
                Error.NoOutputDirectory();

                Program.WriteDlInfoLine($"Do you want to create the directory \"{OutputDirectory}\" ? ");

                if (Program.WaitForYesOrNoMsgBox($"Do you want to create the directory \"{OutputDirectory}\" ? ") == MessageBoxResult.Yes)
                {
                    Directory.CreateDirectory(OutputDirectory);
                    Program.WriteDlInfoLine($"\"{OutputDirectory}\" sucessfully created. Ready to download!");
                }
                else
                {
                    // TODO: Select output dir here instead of opening Json
                    Program.WriteDlInfoLine("Please modify the output directory in Settings.json, it must be a valid directory.");
                    if (Settings.Instance.AutoOpenJsonWhenNecessary) // TODO: this is done several time, this could be a single function
                    {
                        Program.WriteDlInfoLine("Press any key to open Settings.json and close the app...");
                        MessageBox.Show("Press ok to open Settings.json and close the app...", "Quit app", MessageBoxButton.OK, MessageBoxImage.Warning);
                    }
                    else
                    {
                        Program.WriteDlInfoLine("Press any key to close the app...");
                        MessageBox.Show("The app will be closed...", "Quit app", MessageBoxButton.OK, MessageBoxImage.Warning);
                    }

                    Settings.OpenJsonFile();
                    Program.QuitApp();
                }
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

        public static void OpenRelevantFolder(List<ScanWebsiteUrl> scansToDownload)
        {
            if (scansToDownload.Count == 1)
            {
                // One chapter downloaded, open this chapter folder
                string chapterDirectory = GetChapterDirectoryPath(scansToDownload[0]);
                OpenFolder(chapterDirectory);
            }
            else
            {
                bool moreThanOneBook = ScanManagement.MoreThanOneBookInUrlList(scansToDownload);
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

        public static bool AreScanFilesDownloaded(ScanWebsiteUrl scanUrl)
        {
            string chapterDirPath = GetChapterDirectoryPath(scanUrl);
            if (Directory.Exists(chapterDirPath))
            {
                if (Directory.GetFiles(chapterDirPath).Any()) // TODO: Improve this to know if we have the correct amount of page, need more info in ScanWebsiteUrl
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

        public static bool IsCbzArchiveCreated(ScanWebsiteUrl scanUrl)
        {
            string cbzPath = GetCbzFilePath(scanUrl);
            return File.Exists(cbzPath) && File.ReadAllBytes(cbzPath).Length > 0;
        }

        static string GetBookDirectoryPath(ScanWebsiteUrl scanUrl)
        {
            string bookName = scanUrl.BookName;
            string bookDirectoryPath = Path.Combine(OutputDirectory, $"{bookName}{Constants.SCAN_SUFFIX}");

            return bookDirectoryPath;
        }

        public static string GetChapterDirectoryPath(ScanWebsiteUrl scanUrl)
        {
            string bookName = scanUrl.BookName;
            string chapterNumber = scanUrl.ChapterId.ToString();
            string chapterDirectoryPath = Path.Combine(OutputDirectory, $"{bookName}{Constants.SCAN_CHAPTER_PATH}{chapterNumber}");

            return chapterDirectoryPath;
        }

        static string GetCbzFilePath(ScanWebsiteUrl scanUrl)
        {
            string bookName = scanUrl.BookName;
            string chapterNumber = scanUrl.ChapterId.ToString();
            string cbzFilePath = Path.Combine(GetBookDirectoryPath(scanUrl), $"{bookName}{Constants.CBZ_CHAPTER_PREFIX}{chapterNumber}{Constants.CBZ_EXTENSION}");

            return cbzFilePath;
        }
    }
}
