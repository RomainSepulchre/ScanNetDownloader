using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;

namespace ScanDownloader
{
    internal class Program
    {
        // TODO: Scrap web page of the first chapter page to get the exact link of all webp image instead searching manually
        // TODO: Download several chapters at once
        // TODO: Output folder selection
        // TODO: Switch from console to an interface
        // TODO: Generate Cbz automatically when a chapter is fully downloaded

        //
        // TEST LINKS (with several different img naming concention)
        //
        //https://www.scan-vf.net/uploads/manga/one_piece/chapters/chapitre-1090/01.webp
        //https://www.scan-vf.net/uploads/manga/one_piece/chapters/chapitre-1091/001.webp
        //https://www.scan-vf.net/uploads/manga/one_piece/chapters/chapitre-1092/001.webp
        //https://www.scan-vf.net/uploads/manga/one_piece/chapters/chapitre-1093/001.webp
        //https://www.scan-vf.net/uploads/manga/one_piece/chapters/chapitre-1094/01.webp
        //https://www.scan-vf.net/uploads/manga/one_piece/chapters/chapitre-1095/01.webp
        //https://www.scan-vf.net/uploads/manga/one_piece/chapters/chapitre-1096/mp001.webp
        //https://www.scan-vf.net/uploads/manga/one_piece/chapters/chapitre-1097/01.webp
        //https://www.scan-vf.net/uploads/manga/one_piece/chapters/chapitre-1098/mp01.webp
        //https://www.scan-vf.net/uploads/manga/one_piece/chapters/chapitre-1098/mp01.webp


        #region Global variables
        // URL
        private static readonly string BASE_URL = "https://www.scan-vf.net/uploads/manga/one_piece/chapters/chapitre-";
        private static string PageUrlNaming => $"mp{pageId.ToString("D2")}"; //********
        private static readonly string IMG_EXTENSION = ".webp";

        // CHAPTER AND PAGE INFO
        private static int chapterId = 1098; //*******
        private static readonly int START_CHAPTER = 0;
        private static readonly int END_CHAPTER = 0;
        private static readonly int PAGE_START_ID = 1;
        private static int pageId = PAGE_START_ID;


        // OUTPUT FOLDER
        // Book folder 
        private static readonly string BOOK_FOLDER = "One Piece Scan";
        // Classic download folder path
        private static readonly string USER_FOLDER_PATH = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        private static readonly string USER_DOWNLOAD_FOLDER_PATH = Path.Combine(USER_FOLDER_PATH, "Downloads");
        // My download folder path
        private static readonly string CUSTOM_FOLDER_PATH = @"D:\Download";
        // Download directory
        private static readonly string DOWNLOAD_DIRECTORY = Path.Combine(CUSTOM_FOLDER_PATH, BOOK_FOLDER);
        // BOOK NAME
        private static readonly string BOOK_NAME = "OnePiece";

        //OUTPUT FILE
        private static string ImgName => $"{BOOK_NAME}_{chapterId}-{pageId.ToString("D3")}{IMG_EXTENSION}";
        #endregion

        #region Main
        static void Main(string[] args)
        {
            Console.WriteLine($"{BOOK_NAME} - Chapter {chapterId}\n");
            Console.WriteLine($"Download folder will be {DOWNLOAD_DIRECTORY}");

            CheckOutputDirectory();

            bool isNo = WaitForYesOrNoReadKey("Press Y to start or N to quit...") == ConsoleKey.N;
            if (isNo) return; // Quit App prematurely

            string downloadFolder = CreateChapterFolder();
            DownloadFromImgUrl(downloadFolder);

            Console.WriteLine("All page downloaded, press any key to close...");
            Console.ReadKey();
            OpenFolder(downloadFolder);
        }
        #endregion

        #region Downloading
        static void DownloadFromChapterUrl(string downloadPath)
        {
            //using(WebClient client = new WebClient())
            //{
            //    // Or you can get the file content without saving it
            //    string htmlCode = client.DownloadString("https://www.scan-vf.net/one_piece/chapitre-1091/3");
            //    client.DownloadFile("https://www.scan-vf.net/one_piece/chapitre-1091/3", Path.Combine(DOWNLOAD_DIRECTORY, "test.html"));
            //    Console.WriteLine(htmlCode);
            //    Console.ReadKey();
            //    return;
            //}
        }

        // Better solution than this can be found -> Parse html file to find img links
        // Inconsistent file naming cause missing img and prevent to download several chapter at once
        // Could try to test several naming but it would way suboptimal and tricky than parsing html to find the img links
        static void DownloadFromImgUrl(string downloadPath)
        {          
            string downloadedFile = Path.Combine(downloadPath, ImgName);
            string downloadUrl = $"{BASE_URL}{chapterId}/{PageUrlNaming}{IMG_EXTENSION}";

            int webExCounter = 0;

            while (webExCounter < 3)
            {
                using (WebClient client = new WebClient())
                {
                    try
                    {
                        Console.WriteLine($"\nDownloading {ImgName} from {downloadUrl} ...");

                        if (File.Exists(downloadedFile) == true && File.ReadAllBytes(downloadedFile).Length > 0 == true)
                        {
                            Console.WriteLine($"File already downloaded!\n");
                        }
                        else
                        {
                            client.DownloadFile(new Uri(downloadUrl), downloadedFile);
                            Console.WriteLine($"Sucessfully downloaded!\n");
                        }

                        CheckForMissingPages(webExCounter, downloadPath);
                        webExCounter = 0;
                    }
                    catch (WebException ex)
                    {
                        // TODO: Need to test several name possibility due to inconsisten file naming 
                        //Debug.WriteLine($"EXCEPTION: {ex}");
                        webExCounter++;
                        Console.WriteLine($"Download failed ! ({webExCounter}/3)\n");
                    }
                }

                if (webExCounter < 3)
                {
                    pageId++;
                    downloadedFile = Path.Combine(downloadPath, ImgName);
                    downloadUrl = $"{BASE_URL}{chapterId}/{PageUrlNaming}{IMG_EXTENSION}";
                }
            }
        }

        static void CheckForMissingPages(int exCounter, string chapterDirectory)
        {
            if (exCounter > 0)
            {
                for (int i = exCounter; i > 0; i--)
                {
                    int missingPageId = pageId - i;
                    string missingFile = Path.Combine(chapterDirectory, $"{BOOK_NAME}_{chapterId}-{missingPageId.ToString("D3")}{IMG_EXTENSION}");
                    File.Create(missingFile);
                    Console.WriteLine($" Page #{missingPageId} is missing. Press any key to continue");
                }

                Console.ReadKey();
            }
        }
        #endregion

        #region Folder Management
        static void CheckOutputDirectory()
        {
            if (Directory.Exists(DOWNLOAD_DIRECTORY) == false)
            {
                Console.WriteLine("ERROR => Impossible to find the expected download directory");
                Console.WriteLine("Press any key to close...");
                Console.ReadKey();
                return;
            }
        }

        static string CreateChapterFolder()
        {
            string chapterFolder = Path.Combine(DOWNLOAD_DIRECTORY, $"Chapter {chapterId}");

            if (Directory.Exists(chapterFolder) == false)
            {
                Directory.CreateDirectory(chapterFolder);
            }

            return chapterFolder;
        }

        static void OpenFolder(string folderPath)
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
        #endregion

        #region UserInputs
        static ConsoleKey WaitForYesOrNoReadKey(string textDisplayed)
        {
            Console.WriteLine(textDisplayed);
            ConsoleKeyInfo keyEntered = Console.ReadKey();

            while (IsEnteredKeyValid(keyEntered.Key, new ConsoleKey[] { ConsoleKey.Y, ConsoleKey.N }) == false)
            {
                Console.WriteLine(" -> Invalid Key");
                keyEntered = Console.ReadKey();
            }

            return keyEntered.Key;
        }

        static bool IsEnteredKeyValid(ConsoleKey key, ConsoleKey[] expectedKeys)
        {
            bool isValid = false;

            foreach (ConsoleKey expectedKey in expectedKeys)
            {
                if (key == expectedKey)
                {
                    isValid = true;
                    break;
                }
            }

            return isValid;
        }
        #endregion
    }
}
