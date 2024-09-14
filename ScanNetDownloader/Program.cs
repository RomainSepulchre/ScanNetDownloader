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

        // SEPARATORS
        private static readonly string[] URL_BLOCK_START_SEPARATOR = new string[] { "<div class=\"viewer-cnt\">" };
        private static readonly string[] URL_BLOCK_END_SEPARATOR = new string[] { "<div id=\"ppp\" style>" };
        private static readonly string[] CLEAN_BEFORE_IMG_TAG_SEPARATOR = new string[] { "<div id=\"all\" style=\" display: none; \">" };
        private static readonly string[] IMG_TAG_END_SEPARATOR = new string[] { "/>" };
        private static readonly char QUOTE_SEPARATOR = '\"';
        private static readonly char SLASH_SEPARATOR = '/';
        private static readonly char DASH_SEPARATOR = '-';
        private static readonly char POINT_SEPARATOR = '.';
        private static readonly string HTTP_ADDRESS = "https://";

        // SETTINGS
        private static bool openOutputDirectoryWhenClosing = true;
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
            DownloadFromChapterUrl(downloadFolder);
            //DownloadFromImgUrl(downloadFolder);

            Console.WriteLine("All page downloaded, press any key to close...");
            Console.ReadKey();
            if(openOutputDirectoryWhenClosing) OpenFolder(downloadFolder);
        }
        #endregion

        #region Downloading
        // Attempt to search in the html file the links of all images store them and to have the correct img naming every time
        static void DownloadFromChapterUrl(string downloadPath)
        {

            //////////////////////////////////////////////////////////////////////////////////
            // Download html file to check code consistency
            List<string> urlsToTest = new List<string>
            {
                "https://www.scan-vf.net/one_piece/chapitre-1091/3",
                "https://www.scan-vf.net/one_piece/chapitre-1079/1",
                "https://www.scan-vf.net/one_piece/chapitre-1120/1",
                "https://www.scan-vf.net/one_piece/chapitre-1094/1",
                "https://www.scan-vf.net/one_piece/chapitre-140/1",
                "https://www.scan-vf.net/one_piece/chapitre-1087/7"
            };

            List<string> imgUrlsToTest = new List<string>
            {
                "https://www.scan-vf.net/uploads/manga/kingdom/chapters/chapitre-810/01.webp",
                "https://www.scan-vf.net/uploads/manga/attaque-des-titans/chapters/chapitre-139.5/01.jpg",
                "https://www.scan-vf.net/uploads/manga/one-punch-man/chapters/chapitre-230/mp-01.webp",
                "https://www.scan-vf.net/uploads/manga/one_piece/chapters/chapitre-1091/001.webp",
            };
            // Debug
            if (true) 
            {
                //SaveHtmlFiles(urlsToTest);

                foreach (string item in imgUrlsToTest)
                {
                    string _bookName = GetBookNameFromUrl(item);
                    string _chapterId = GetChapterIdFromUrl(item);
                    string _fileExtension = GetFileExtensionFromUrl(item);
                    string _imgName = $"{_bookName}_{_chapterId}-{pageId.ToString("D3")}{_fileExtension}";

                    Console.WriteLine($"\nIMG NAME => {_imgName}");
                }
                return; // DEBUG
            }
            ////////////////////////////////////////////////////////////////////////////







            foreach (string url in urlsToTest)
            {
                List<string> imgsToDownload = GetImgUrlsFromHtmlContent(url);

                int pageId = 1;
                foreach (string imgUrl in imgsToDownload)
                {
                    string bookName = GetBookNameFromUrl(imgUrl);
                    string chapterId = GetChapterIdFromUrl(imgUrl);
                    string fileExtension = GetFileExtensionFromUrl(imgUrl);
                    string imgName = $"{bookName}_{chapterId}-{pageId.ToString("D3")}{fileExtension}";
                    Debug.WriteLine($"IMG NAME => {imgName}");
                    
                    string downloadedFile = Path.Combine(downloadPath, imgName);

                    using (WebClient client = new WebClient())
                    {
                        try
                        {
                            Console.WriteLine($"\nDownloading {ImgName} from {imgUrl} ...");

                            if (File.Exists(downloadedFile) == true && File.ReadAllBytes(downloadedFile).Length > 0 == true)
                            {
                                Console.WriteLine($"File already downloaded!\n");
                            }
                            else
                            {
                                client.DownloadFile(new Uri(imgUrl), downloadedFile);
                                Console.WriteLine($"Sucessfully downloaded!\n");
                            }

                        }
                        catch (WebException ex)
                        {
                            Debug.WriteLine($"EXCEPTION: {ex}");
                            Console.WriteLine($"Download failed for {imgUrl} ! Exception:{ex}\n");
                        }
                    }

                    pageId++;
                }
            }
            

        }

        static List<string> GetImgUrlsFromHtmlContent(string htmlUrl)
        {
            string htmlContent;
            using (WebClient client = new WebClient())
            {
                htmlContent = client.DownloadString(htmlUrl); // Save html code in a variable
            }

            // Start parsing
            // Separators
            string[] splitContent = htmlContent.Split(URL_BLOCK_START_SEPARATOR, StringSplitOptions.RemoveEmptyEntries); // Split before the block with all the img url
            string urlSplit = splitContent[1]; // Keep the split after our separator (trim the beginning)

            splitContent = urlSplit.Split(URL_BLOCK_END_SEPARATOR, StringSplitOptions.RemoveEmptyEntries); // Split after the block with all the img url
            urlSplit = splitContent[0]; // Keep the split before our separator (trim the end)

            splitContent = urlSplit.Split(CLEAN_BEFORE_IMG_TAG_SEPARATOR, StringSplitOptions.RemoveEmptyEntries); // Clean the html code that is still before the first <img/>
            urlSplit = splitContent[1]; // Keep the split after our separator (trim the beginning)

            Debug.WriteLine($"---- URL SPLIT ---- \n {urlSplit} \n ---- URL SPLIT END ----");

            List<string> imgUrls = urlSplit.Split(IMG_TAG_END_SEPARATOR, StringSplitOptions.RemoveEmptyEntries).ToList(); // Split each img tag in a list

            // Keep only the urls in the list
            for (int i = imgUrls.Count - 1; i >= 0; i--)
            {
                if (imgUrls[i].ToLower().Contains(HTTP_ADDRESS))
                {
                    string[] imgUrlSplit = imgUrls[i].Split(QUOTE_SEPARATOR);
                    foreach (string split in imgUrlSplit)
                    {
                        // Keep only the split containing the url
                        if (split.ToLower().Contains(HTTP_ADDRESS))
                        {
                            imgUrls[i] = split;
                            imgUrls[i] = imgUrls[i].Replace(" ", string.Empty); // Remove space in the string
                        }
                    }
                }
                else
                {
                    Debug.WriteLine($"\nRemove Line #{i} \"{imgUrls[i]}\" from url list");
                    imgUrls.RemoveAt(i);
                }
            }

            // DEBUG.WRITE LIST ITEMS
            LogListItems(imgUrls);

            return imgUrls;
        }


        

        static string GetBookNameFromUrl(string url)
        {
            // Example of url
            //   0    1      2            3      4      5        6         7          8
            // https://www.scan-vf.net/uploads/manga/kingdom/chapters/chapitre-810/01.webp
            // https://www.scan-vf.net/uploads/manga/attaque-des-titans/chapters/chapitre-139.5/01.jpg
            // https://www.scan-vf.net/uploads/manga/one-punch-man/chapters/chapitre-230/mp-01.webp
            // https://www.scan-vf.net/uploads/manga/one_piece/chapters/chapitre-1091/001.webp

            string bookName = url.Split(SLASH_SEPARATOR)[5];

            //TODO: Clean name format
            Debug.WriteLine($"GetBookNameFromUrl -> {bookName}");
            return bookName;
        }

        static string GetChapterIdFromUrl(string url)
        {
            // Example of url
            //   0    1      2            3      4      5        6         7          8
            // https://www.scan-vf.net/uploads/manga/kingdom/chapters/chapitre-810/01.webp
            // https://www.scan-vf.net/uploads/manga/attaque-des-titans/chapters/chapitre-139.5/01.jpg
            // https://www.scan-vf.net/uploads/manga/one-punch-man/chapters/chapitre-230/mp-01.webp
            // https://www.scan-vf.net/uploads/manga/one_piece/chapters/chapitre-1091/001.webp

            string chapterId = url.Split(SLASH_SEPARATOR)[7];

            //TODO: Clean chapter id format
            Debug.WriteLine($"GetChapterIdFromUrl -> {chapterId}");
            return chapterId;
        }

        static string GetFileExtensionFromUrl(string url)
        {
            // Example of url
            //   0    1      2            3      4      5        6         7          8
            // https://www.scan-vf.net/uploads/manga/kingdom/chapters/chapitre-810/01.webp
            // https://www.scan-vf.net/uploads/manga/attaque-des-titans/chapters/chapitre-139.5/01.jpg
            // https://www.scan-vf.net/uploads/manga/one-punch-man/chapters/chapitre-230/mp-01.webp
            // https://www.scan-vf.net/uploads/manga/one_piece/chapters/chapitre-1091/001.webp

            string fileExtension = url.Split(SLASH_SEPARATOR)[8];
            fileExtension = "." + fileExtension.Split(POINT_SEPARATOR)[1];

            Debug.WriteLine($"GetFileExtensionFromUrl -> {fileExtension}");
            return fileExtension;
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

        #region Debug
        static void SaveHtmlFiles(List<string> urlList)
        {
            foreach (string urlToDl in urlList)
            {
                // Save htlm code in a file to test
                using (WebClient client = new WebClient())
                {
                    string htmlFileName = urlToDl.Remove(0, 8); // Remove "https://"
                    htmlFileName = htmlFileName.Replace('/', '_');
                    htmlFileName = htmlFileName + ".html";
                    client.DownloadFile(urlToDl, Path.Combine(DOWNLOAD_DIRECTORY, htmlFileName));

                    Console.WriteLine($"\n {htmlFileName} downloaded...");
                }
            }
            Console.WriteLine($"Html file saved, press to open folder location...");
            Console.ReadKey();
            openOutputDirectoryWhenClosing = false;
            OpenFolder(DOWNLOAD_DIRECTORY);
        }

        static void LogListItems<T>(List<T> list)
        {
            Debug.WriteLine($"\n---- {nameof(list)} ----");
            foreach (T imgUrl in list)
            {
                Debug.WriteLine($"{imgUrl}");
            }
            Debug.WriteLine($"\n---- {nameof(list)} END ----");
        }
        #endregion
    }
}
