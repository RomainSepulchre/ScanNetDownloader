using ScanNetDownloader;
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
using System.IO.Compression;


namespace ScanDownloader
{
    internal class Program
    {

        // TODO: Replace chapter list by link of book main page, ask a range of chapter to download and generate myself the chapter links instead of having to enter manually each chapter links
        //--> handle out of range chapter if user enter chapter not available yet
        //--> One chapter url was .../episode-1101/... instead of .../chapitre-1101/...
        // TODO: Generate Cbz automatically when a chapter is fully downloaded
        // TODO: Select Output folder by opening explorer window 
        // TODO: Switch from console app to an interface
        // TODO: Scrap a list of all the books available and create a search engine

        #region Parameters
        /// <summary>
        /// Provide the url of the first page of any chapter you want to download
        /// </summary>
        private static readonly List<string> CHAPTERS_TO_DOWNLOAD_URL = new List<string>
        {
            "https://www.scan-vf.net/one_piece/chapitre-1090/1",
            "https://www.scan-vf.net/one_piece/chapitre-1091/1",
            "https://www.scan-vf.net/one_piece/chapitre-1092/1",
            "https://www.scan-vf.net/one_piece/chapitre-1093/1",
            "https://www.scan-vf.net/one_piece/chapitre-1094/1",
            "https://www.scan-vf.net/one_piece/chapitre-1095/1",
            "https://www.scan-vf.net/one_piece/chapitre-1096/1",
            "https://www.scan-vf.net/one_piece/chapitre-1097/1",
            "https://www.scan-vf.net/one_piece/chapitre-1098/1",
            "https://www.scan-vf.net/one_piece/chapitre-1099/1",
            "https://www.scan-vf.net/one_piece/chapitre-1100/1",
            "https://www.scan-vf.net/one_piece/episode-1101/1",
            "https://www.scan-vf.net/one_piece/chapitre-1102/1",
            "https://www.scan-vf.net/one_piece/chapitre-1103/1",
            "https://www.scan-vf.net/one_piece/chapitre-1104/1",
            "https://www.scan-vf.net/one_piece/chapitre-1105/1",
            "https://www.scan-vf.net/one_piece/chapitre-1106/1",
            "https://www.scan-vf.net/one_piece/chapitre-1107/1",
            "https://www.scan-vf.net/one_piece/chapitre-1108/1",
            "https://www.scan-vf.net/one_piece/chapitre-1109/1",
            "https://www.scan-vf.net/one_piece/chapitre-1110/1",
            "https://www.scan-vf.net/one_piece/chapitre-1111/1",
            "https://www.scan-vf.net/one_piece/chapitre-1112/1",
            "https://www.scan-vf.net/one_piece/chapitre-1113/1",
            "https://www.scan-vf.net/one_piece/chapitre-1114/1",
            "https://www.scan-vf.net/one_piece/chapitre-1115/1",
            "https://www.scan-vf.net/one_piece/chapitre-1116/1",
            "https://www.scan-vf.net/one_piece/chapitre-1117/1",
            "https://www.scan-vf.net/one_piece/chapitre-1118/1",
            "https://www.scan-vf.net/one_piece/chapitre-1119/1",
            "https://www.scan-vf.net/one_piece/chapitre-1120/1",
            "https://www.scan-vf.net/one_piece/chapitre-1121/1",
            "https://www.scan-vf.net/one_piece/chapitre-1122/1",
            "https://www.scan-vf.net/one_piece/chapitre-1123/1",
            "https://www.scan-vf.net/one_piece/chapitre-1124/1",
            "https://www.scan-vf.net/one_piece/chapitre-1125/1",
            "https://www.scan-vf.net/one_piece/chapitre-1126/1"


            //"https://www.scan-vf.net/one_piece/chapitre-1091/3",
            //"https://www.scan-vf.net/one_piece/chapitre-1079/1",
            //"https://www.scan-vf.net/one_piece/chapitre-1120/5",
            //"https://www.scan-vf.net/one_piece/chapitre-1094/1",
            //"https://www.scan-vf.net/one_piece/chapitre-140/1",
            //"https://www.scan-vf.net/one_piece/chapitre-1087/7",
            //"https://www.scan-vf.net/jujutsu-kaisen/chapitre-268/1",
            //"https://www.scan-vf.net/dragon-Ball-Super/chapitre-73/4",
            //"https://www.scan-vf.net/my-hero-academia/chapitre-358/2"
        };

        /// <summary>
        /// Set a custom output folder (if null or empty, we use the default user download folder)
        /// </summary>
        private static readonly string CUSTOM_FOLDER_PATH = @"D:\Download\ScanNetDownloader";

        /// <summary>
        /// Do you want to create a .cbz archive of every chapter downloaded
        /// </summary>
        private static readonly bool createCbzArchive = true;

        /// <summary>
        /// Should the program automatically open the output directory when closing
        /// </summary>
        private static bool openOutputDirectoryWhenClosing = true;
        #endregion

        // TODO: clean this shit
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

        
        // Download directory
        private static readonly string OUTPUT_DIRECTORY = string.IsNullOrEmpty(CUSTOM_FOLDER_PATH) ? USER_DOWNLOAD_FOLDER_PATH : CUSTOM_FOLDER_PATH;
        // BOOK NAME
        private static readonly string BOOK_NAME = "OnePiece";

        //OUTPUT FILE
        private static string ImgName => $"{BOOK_NAME}_{chapterId}-{pageId.ToString("D3")}{IMG_EXTENSION}";

        // CHAR AND SEPARATORS
        private static readonly string[] URL_BLOCK_START_SEPARATOR = new string[] { "<div class=\"viewer-cnt\">" };
        private static readonly string[] URL_BLOCK_END_SEPARATOR = new string[] { "<div id=\"ppp\" style>" };
        private static readonly string[] CLEAN_BEFORE_IMG_TAG_SEPARATOR = new string[] { "<div id=\"all\" style=\" display: none; \">" };
        private static readonly string[] IMG_TAG_END_SEPARATOR = new string[] { "/>" };
        private static readonly char QUOTE_CHAR = '\"';
        private static readonly char SLASH_CHAR = '/';
        private static readonly char DASH_CHAR = '-';
        private static readonly char UNDERSCORE_CHAR = '_';
        private static readonly char POINT_CHAR = '.';
        private static readonly string HTTP_ADDRESS = "https://";
        private static readonly string SPACE = " ";
        #endregion

        #region Main
        static void Main(string[] args)
        {
            string appTitle = $"$--- SCAN.NET DOWNLOADER ---$";
            Console.WriteLine($"\n{AdaptativeLineOfCharForHeader(appTitle, '$')}");
            Console.WriteLine(appTitle);
            Console.WriteLine($"{AdaptativeLineOfCharForHeader(appTitle, '$')}\n");

            Console.WriteLine($"Here is the list of chapters you are going to download:");
            foreach (string url in CHAPTERS_TO_DOWNLOAD_URL)
            {
                Console.WriteLine($"- {GetBookNameFromUrl(url)}_{GetChapterIdFromUrl(url, false)}");
            }

            Console.WriteLine($"\nThe files will be downloaded in {OUTPUT_DIRECTORY}, a folder will automatically be created for each title and chapters");
            CheckOutputDirectory();

            bool isNo = WaitForYesOrNoReadKey("\nPress Y to start or N to quit...") == ConsoleKey.N;
            if (isNo) return; // Quit App prematurely

            DownloadFromChaptersUrl();
            //DownloadFromImgUrl(downloadFolder);

            Console.WriteLine("Downloaded, press any key to close...");
            Console.ReadKey();

            // TODO: improve this to open the more relevant folder
            // (1 chapter -> open the chapter folder, Several chapter of the same book -> Open Book folder, Several Book Open Output directory)
            if (openOutputDirectoryWhenClosing) OpenFolder(OUTPUT_DIRECTORY); 
        }
        #endregion

        #region Downloading
        // Attempt to search in the html file the links of all images store them and to have the correct img naming every time
        static void DownloadFromChaptersUrl()
        {
            //////////////////////////////////////////////////////////////////////////////////
            // Download html file to check code consistency
            //List<string> imgUrlsToTest = new List<string>
            //{
            //    "https://www.scan-vf.net/uploads/manga/kingdom/chapters/chapitre-810/01.webp",
            //    "https://www.scan-vf.net/uploads/manga/attaque-des-titans/chapters/chapitre-139.5/01.jpg",
            //    "https://www.scan-vf.net/uploads/manga/one-punch-man/chapters/chapitre-230/mp-01.webp",
            //    "https://www.scan-vf.net/uploads/manga/one_piece/chapters/chapitre-1091/001.webp",
            //};
            ////////////////////////////////////////////////////////////////////////////

            foreach (string url in CHAPTERS_TO_DOWNLOAD_URL)
            {
                string bookName = GetBookNameFromUrl(url);
                string chapterId = GetChapterIdFromUrl(url);

                string header = $"Download {bookName} - chapter {chapterId} from {url}";
                Console.WriteLine($"\n{AdaptativeLineOfCharForHeader(header, '*')}");
                Console.WriteLine(header);
                Console.WriteLine($"{AdaptativeLineOfCharForHeader(header, '*')}");

                List<string> imgsToDownload = GetImgUrlsFromHtmlContent(url);
                if (imgsToDownload.Count == 0) { continue; } // if list (in case of error while getting html content) is empty skip directly to the next url

                // Create output folder if necessary
                string downloadPath = CreateChapterDirectory(bookName, chapterId);
                Debug.Write($"Download Path: {downloadPath}");

                int pageId = 1;
                foreach (string imgUrl in imgsToDownload)
                {
                    string fileExtension = GetFileExtensionFromUrl(imgUrl);
                    
                    string imgName = $"{bookName}_{chapterId}-{pageId.ToString("D3")}{fileExtension}";
                    Debug.WriteLine($"Image Name: {imgName}");
                    
                    string downloadFile = Path.Combine(downloadPath, imgName);
                    Debug.WriteLine($"Download File => {downloadFile}");

                    using (WebClient client = new WebClient())
                    {
                        try
                        {
                            Console.WriteLine($"\nDownloading {imgName} from {imgUrl}");
                            Console.WriteLine($"...");

                            if (File.Exists(downloadFile) == true && File.ReadAllBytes(downloadFile).Length > 0 == true)
                            {
                                Console.WriteLine($"File already downloaded!\n");
                            }
                            else
                            {
                                client.DownloadFile(new Uri(imgUrl), downloadFile);
                                Console.WriteLine($"Sucessfully downloaded!\n");
                            }
                        }
                        catch (WebException ex)
                        {
                            Debug.WriteLine($"Error while downloading: {ex}");
                            Console.WriteLine($"Download failed for {imgUrl} ! Exception:{ex}");
                            Console.WriteLine($"Press any key to continue...\n");
                            Console.ReadKey();
                        }
                    }
                    pageId++;
                }
                // Create cbz here
                if (createCbzArchive) // TODO: Put this in a dedicated function
                {
                    Console.WriteLine($"=> Create .CBZ for {bookName}-{chapterId}...");
                    string folderToArchive = downloadPath;
                    string cbzFilePath = Path.Combine(Directory.GetParent(downloadPath).FullName, $"{bookName}-chapter{chapterId}.cbz");
                    if (File.Exists(cbzFilePath) == false)
                    {
                        ZipFile.CreateFromDirectory(folderToArchive, cbzFilePath);
                        Console.WriteLine($"=> {bookName}-{chapterId} .CBZ successfully created!\n");
                    }
                    else
                    {
                        if (File.ReadAllBytes(cbzFilePath).Length > 0)
                        {
                            Console.WriteLine($"=> .CBZ already created!\n");
                        }
                        else
                        {
                            File.Delete(cbzFilePath);
                            ZipFile.CreateFromDirectory(folderToArchive, cbzFilePath);
                            Console.WriteLine($"=> {bookName}-{chapterId} .CBZ successfully created!\n");
                        }
                    }
                }
            }
        }

        // TODO : Remove this once other solution is functionnal
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
                        Debug.WriteLine($"EXCEPTION: {ex}");
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

        #region String Parsing
        static List<string> GetImgUrlsFromHtmlContent(string htmlUrl)
        {
            string htmlContent;
            using (WebClient client = new WebClient())
            {
                try
                {
                    Debug.WriteLine($"Loading {htmlUrl} content...");
                    htmlContent = client.DownloadString(htmlUrl); // Save html code in a variable
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Error while loading {htmlUrl} content. Ex: {ex}");
                    Console.WriteLine($"Error while loading {htmlUrl} content, this chapter won't be downloaded. Ex: {ex}");
                    Console.WriteLine($"Verify you entered a correct chapter url\n");
                    Console.WriteLine($"Ex: {ex}\n");
                    Console.WriteLine($"Press any key when you're ready to continue");
                    Console.ReadKey(true);
                    return new List<string>();
                }
                
            }

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
                    string[] imgUrlSplit = imgUrls[i].Split(QUOTE_CHAR);
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

        static string GetBookNameFromUrl(string url, bool removeSpace=false)
        {
            #region Chapter and img url examples
            // Example of chapter url
            //   0    1        2           3          4         5
            // https://www.scan-vf.net/one_piece/chapitre-1079/1
            // https://www.scan-vf.net/jujutsu-kaisen/chapitre-268/1
            // https://www.scan-vf.net/dragon-Ball-Super/chapitre-73/1
            // https://www.scan-vf.net/my-hero-academia/chapitre-358/1

            // Example of img url
            //   0    1      2            3      4      5        6         7          8
            // https://www.scan-vf.net/uploads/manga/kingdom/chapters/chapitre-810/01.webp
            // https://www.scan-vf.net/uploads/manga/attaque-des-titans/chapters/chapitre-139.5/01.jpg
            // https://www.scan-vf.net/uploads/manga/one-punch-man/chapters/chapitre-230/mp-01.webp
            // https://www.scan-vf.net/uploads/manga/one_piece/chapters/chapitre-1091/001.webp 
            #endregion

            int splitIdForChapterUrl = 3;
            int splitIdForImgUrl = 5;
            bool isImgUrl = url.Contains("uploads"); // Check if we are using a link of a chapter or an image

            string bookName = url.Split(SLASH_CHAR)[isImgUrl ? splitIdForImgUrl : splitIdForChapterUrl];
            bookName = bookName.ToTitleCase();
            bookName = bookName.Replace(DASH_CHAR.ToString(), removeSpace ? string.Empty : SPACE);
            bookName = bookName.Replace(UNDERSCORE_CHAR.ToString(), removeSpace ? string.Empty : SPACE);

            Debug.WriteLine($"GetBookNameFromUrl ({(isImgUrl ? "IMG URL" : "CHAPTER URL")}) -> {bookName}");
            return bookName;
        }

        static string GetChapterIdFromUrl(string url, bool keepNumberOnly=true)
        {
            #region Chapter and img url examples
            // Example of chapter url
            //   0    1        2           3          4         5
            // https://www.scan-vf.net/one_piece/chapitre-1079/1
            // https://www.scan-vf.net/jujutsu-kaisen/chapitre-268/1
            // https://www.scan-vf.net/dragon-Ball-Super/chapitre-73/1
            // https://www.scan-vf.net/my-hero-academia/chapitre-358/1

            // Example of img url
            //   0    1      2            3      4      5        6         7          8
            // https://www.scan-vf.net/uploads/manga/kingdom/chapters/chapitre-810/01.webp
            // https://www.scan-vf.net/uploads/manga/attaque-des-titans/chapters/chapitre-139.5/01.jpg
            // https://www.scan-vf.net/uploads/manga/one-punch-man/chapters/chapitre-230/mp-01.webp
            // https://www.scan-vf.net/uploads/manga/one_piece/chapters/chapitre-1091/001.webp 
            #endregion

            int splitIdForChapterUrl = 4;
            int splitIdForImgUrl = 7;
            bool isImgUrl = url.Contains("uploads"); // Check if we are using a link of a chapter or an image

            string chapterId = url.Split(SLASH_CHAR)[isImgUrl ? splitIdForImgUrl : splitIdForChapterUrl];
            if (keepNumberOnly) chapterId = chapterId.Split(DASH_CHAR)[1];            
            
            Debug.WriteLine($"GetChapterIdFromUrl ({(isImgUrl ? "IMG URL" : "CHAPTER URL")}) -> {chapterId}");
            return chapterId;
        }

        static string GetFileExtensionFromUrl(string url)
        {
            #region Chapter and img url examples
            // Example of chapter url
            //   0    1        2           3          4         5
            // https://www.scan-vf.net/one_piece/chapitre-1079/1
            // https://www.scan-vf.net/jujutsu-kaisen/chapitre-268/1
            // https://www.scan-vf.net/dragon-Ball-Super/chapitre-73/1
            // https://www.scan-vf.net/my-hero-academia/chapitre-358/1

            // Example of img url
            //   0    1      2            3      4      5        6         7          8
            // https://www.scan-vf.net/uploads/manga/kingdom/chapters/chapitre-810/01.webp
            // https://www.scan-vf.net/uploads/manga/attaque-des-titans/chapters/chapitre-139.5/01.jpg
            // https://www.scan-vf.net/uploads/manga/one-punch-man/chapters/chapitre-230/mp-01.webp
            // https://www.scan-vf.net/uploads/manga/one_piece/chapters/chapitre-1091/001.webp 
            #endregion

            string fileExtension = url.Split(SLASH_CHAR)[8];
            fileExtension = "." + fileExtension.Split(POINT_CHAR)[1];

            Debug.WriteLine($"GetFileExtensionFromUrl -> {fileExtension}");
            return fileExtension;
        }
        #endregion

        #region Folder Management
        static void CheckOutputDirectory()
        {
            if (Directory.Exists(OUTPUT_DIRECTORY) == false)
            {
                Console.WriteLine("ERROR => Impossible to find the expected download directory");
                Console.WriteLine("Press any key to close..."); // TODO: Ask to create the folder the instead of closing 
                Console.ReadKey();
                return;
            }
        }

        static string CreateChapterDirectory(string bookName, string chapterId)
        {
            string chapterDirectory = Path.Combine(OUTPUT_DIRECTORY, $"{bookName} Scan/Chapter {chapterId}");

            if (Directory.Exists(chapterDirectory) == false)
            {
                Directory.CreateDirectory(chapterDirectory);
                Debug.WriteLine($"Directory created: {chapterDirectory}");
            }
            return chapterDirectory;
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

        static string AdaptativeLineOfCharForHeader(string header, char charToUseForLine)
        {
            return new string(charToUseForLine, header.Length);
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

            Console.WriteLine("\n");
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
                    client.DownloadFile(urlToDl, Path.Combine(OUTPUT_DIRECTORY, htmlFileName));

                    Console.WriteLine($"\n {htmlFileName} downloaded...");
                }
            }
            Console.WriteLine($"Html file saved, press to open folder location...");
            Console.ReadKey();
            openOutputDirectoryWhenClosing = false;
            OpenFolder(OUTPUT_DIRECTORY);
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
