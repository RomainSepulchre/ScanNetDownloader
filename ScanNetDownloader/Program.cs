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
using System.Threading;


namespace ScanDownloader
{
    internal class Program
    {

        // TODO: Replace chapter list by link of book main page, ask a range of chapter to download and generate myself the chapter links instead of having to enter manually each chapter links
        //--> handle out of range chapter if user enter chapter not available yet
        //--> One chapter url was .../episode-1101/... instead of .../chapitre-1101/...
        // TODO: Save User Settings in a Json file
        // TODO: Select Output folder by opening explorer window 
        // TODO: Switch from console app to an interface (WPF ?)
        // TODO: Scrap a list of all the books available and create a search engine

        #region Parameters
        /// <summary>
        /// Provide the url of the first page of any chapter you want to download
        /// </summary>
        private static readonly List<string> CHAPTERS_TO_DOWNLOAD_URL = new List<string>
        {
            "https://www.scan-vf.net/one_piece/chapitre-1079/1",
            "https://www.scan-vf.net/one_piece/chapitre-1120/5",
            "https://www.scan-vf.net/one_piece/chapitre-140/1",
            "https://www.scan-vf.net/one_piece/chapitre-1087/7",
            "https://www.scan-vf.net/jujutsu-kaisen/chapitre-268/1",
            "https://www.scan-vf.net/dragon-Ball-Super/chapitre-73/4",
            "https://www.scan-vf.net/my-hero-academia/chapitre-358/2"
        };

        /// <summary>
        /// Set a custom output folder (if null or empty, we use the default user download folder) (Default=string.Empty)
        /// </summary>
        private static readonly string CUSTOM_FOLDER_PATH = @"D:\Download\ScanNetDownloader\Dev";

        /// <summary>
        /// The output directory used by the program
        /// </summary>
        private static readonly string OUTPUT_DIRECTORY = string.IsNullOrEmpty(CUSTOM_FOLDER_PATH) ? Constants.USER_DOWNLOAD_FOLDER_PATH : CUSTOM_FOLDER_PATH;

        /// <summary>
        /// Do you want to create a .cbz archive of every chapter downloaded (Default=True)
        /// </summary>
        private static readonly bool createCbzArchive = true;

        /// <summary>
        /// Do you want to keep the images downloaded once the cbz has been created (Default=False)
        /// </summary>
        private static readonly bool deleteImagesAfterCbzCreation = false;

        /// <summary>
        /// Should the program automatically open the output directory when closing (Default=True)
        /// </summary>
        private static bool openOutputDirectoryWhenClosing = true;
        #endregion

        #region Main
        static void Main(string[] args)
        {
            string appTitle = $"$$$--- SCAN.NET DOWNLOADER ---$$$";
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
            if (isNo) QuitConsole(); 

            DownloadFromChaptersUrl();

            Console.WriteLine("Finished, press any key to close...");
            Console.ReadKey();

            // TODO: improve this to open the more relevant folder
            // (1 chapter -> open the chapter folder, Several chapter of the same book -> Open Book folder, Several Book Open Output directory)
            if (openOutputDirectoryWhenClosing) OpenFolder(OUTPUT_DIRECTORY);

            QuitConsole();
        }
        #endregion

        #region Downloading
        static void DownloadFromChaptersUrl()
        {
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
                            ErrorMessageForImgDownload(ex, imgUrl);
                        }
                    }
                    pageId++;
                }

                if (createCbzArchive)
                {
                    CreateCbzArchive(bookName, chapterId, downloadPath);
                }
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
                    ErrorMessageForHtmlDownload(ex, htmlUrl);
                    return new List<string>();
                }  
            }

            string[] splitContent = htmlContent.Split(Constants.URL_BLOCK_START_SEPARATOR, StringSplitOptions.RemoveEmptyEntries); // Split before the block with all the img url
            string urlSplit = splitContent[1]; // Keep the split after our separator (trim the beginning)

            splitContent = urlSplit.Split(Constants.URL_BLOCK_END_SEPARATOR, StringSplitOptions.RemoveEmptyEntries); // Split after the block with all the img url
            urlSplit = splitContent[0]; // Keep the split before our separator (trim the end)

            splitContent = urlSplit.Split(Constants.CLEAN_BEFORE_IMG_TAG_SEPARATOR, StringSplitOptions.RemoveEmptyEntries); // Clean the html code that is still before the first <img/>
            urlSplit = splitContent[1]; // Keep the split after our separator (trim the beginning)

            Debug.WriteLine($"---- URL SPLIT ---- \n {urlSplit} \n ---- URL SPLIT END ----");

            List<string> imgUrls = urlSplit.Split(Constants.IMG_TAG_END_SEPARATOR, StringSplitOptions.RemoveEmptyEntries).ToList(); // Split each img tag in a list

            // Keep only the urls in the list
            for (int i = imgUrls.Count - 1; i >= 0; i--)
            {
                if (imgUrls[i].ToLower().Contains(Constants.HTTP_ADDRESS))
                {
                    string[] imgUrlSplit = imgUrls[i].Split(Constants.QUOTE_CHAR);
                    foreach (string split in imgUrlSplit)
                    {
                        // Keep only the split containing the url
                        if (split.ToLower().Contains(Constants.HTTP_ADDRESS))
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

            imgUrls.Log(); // Debug log of list items

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

            string bookName = url.Split(Constants.SLASH_CHAR)[isImgUrl ? splitIdForImgUrl : splitIdForChapterUrl];
            bookName = bookName.ToTitleCase();
            bookName = bookName.Replace(Constants.DASH_CHAR.ToString(), removeSpace ? string.Empty : Constants.SPACE);
            bookName = bookName.Replace(Constants.UNDERSCORE_CHAR.ToString(), removeSpace ? string.Empty : Constants.SPACE);

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

            string chapterId = url.Split(Constants.SLASH_CHAR)[isImgUrl ? splitIdForImgUrl : splitIdForChapterUrl];
            if (keepNumberOnly) chapterId = chapterId.Split(Constants.DASH_CHAR)[1];            
            
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

            string fileExtension = url.Split(Constants.SLASH_CHAR)[8];
            fileExtension = "." + fileExtension.Split(Constants.POINT_CHAR)[1];

            Debug.WriteLine($"GetFileExtensionFromUrl -> {fileExtension}");
            return fileExtension;
        }
        #endregion

        #region Folder Management
        static void CheckOutputDirectory()
        {
            if (Directory.Exists(OUTPUT_DIRECTORY) == false)
            {
                ErrorMessageNoOutputDirectory();

                Console.WriteLine($"Do you want to create the directory \"{OUTPUT_DIRECTORY}\" ? ");

                if (WaitForYesOrNoReadKey($"Press Y (yes) or N (no)...") == ConsoleKey.Y)
                {
                    Directory.CreateDirectory(OUTPUT_DIRECTORY);
                    Console.WriteLine($"\"{OUTPUT_DIRECTORY}\" sucessfully created. Ready to download!");
                }
                else
                {
                    Console.WriteLine("Please modify the output directory setting with a valid directory.");
                    Console.WriteLine("Press any key to close the app...");
                    Console.ReadKey();

                    QuitConsole();
                }
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
        #endregion

        #region Cbz Archive
        static void CreateCbzArchive(string bookName, string chapterId, string downloadPath)
        {
            Console.WriteLine($"=> Create .CBZ for {bookName}-{chapterId}...");

            string folderToArchive = downloadPath;
            string cbzFilePath = Path.Combine(Directory.GetParent(downloadPath).FullName, $"{bookName}-chapter{chapterId}.cbz");

            if (File.Exists(cbzFilePath) == false)
            {
                try
                {
                    ZipFile.CreateFromDirectory(folderToArchive, cbzFilePath);
                    Console.WriteLine($"=> {bookName}-{chapterId} .CBZ successfully created!\n");
                }
                catch (Exception ex)
                {
                    ErrorMessageForCbzCreation(ex, bookName, chapterId);
                    return;
                }
            }
            else
            {
                if (File.ReadAllBytes(cbzFilePath).Length > 0)
                {
                    Console.WriteLine($"=> .CBZ already created!\n");
                }
                else // Replace empty Cbz
                {
                    try
                    {
                        File.Delete(cbzFilePath);
                        ZipFile.CreateFromDirectory(folderToArchive, cbzFilePath);
                        Console.WriteLine($"=> {bookName}-{chapterId} .CBZ successfully created!\n");
                    }
                    catch (Exception ex)
                    {
                        ErrorMessageForCbzCreation(ex, bookName, chapterId);
                        return;
                    }
                }
            }

            if (deleteImagesAfterCbzCreation)
            {
                if (Directory.Exists(downloadPath)) Directory.Delete(downloadPath, true);
            }
        }
        #endregion

        #region Error Messages

        static void ErrorMessageNoOutputDirectory()
        {
            ChangeConsoleColor(ConsoleColor.DarkRed);
            Console.WriteLine("\nError, impossible to find the expected download directory.\n");
            ResetConsoleColor();
        }

        static void ErrorMessageForHtmlDownload(Exception ex, string htmlUrl)
        {
            Debug.WriteLine($"Error while loading {htmlUrl} content: {ex}");

            ChangeConsoleColor(ConsoleColor.DarkRed);
            Console.WriteLine($"\nError while loading {htmlUrl} content, this chapter won't be downloaded.");
            Console.WriteLine($"Verify you entered a correct chapter url.\n");

            ChangeConsoleColor(ConsoleColor.Red);
            Console.WriteLine($"Exception: {ex}\n");

            ResetConsoleColor();
            Console.WriteLine($"Press any key to continue...\n");
            Console.ReadKey(true);
        }

        static void ErrorMessageForImgDownload(Exception ex, string imgUrl)
        {
            Debug.WriteLine($"Error while downloading: {ex}");

            ChangeConsoleColor(ConsoleColor.DarkRed);
            Console.WriteLine($"\nDownload failed for {imgUrl}! Exception:{ex}");
            Console.WriteLine($"Verify the image URL is working in a web browser.\n");

            ChangeConsoleColor(ConsoleColor.Red);
            Console.WriteLine($"Exception: {ex}\n");

            ResetConsoleColor();
            Console.WriteLine($"Press any key to continue...\n");
            Console.ReadKey();
        }

        static void ErrorMessageForCbzCreation(Exception ex, string bookName, string chapterId)
        {
            Debug.WriteLine($"=> Error while creating CBZ archive for {bookName}-{chapterId}: {ex}");

            ChangeConsoleColor(ConsoleColor.DarkRed);

            Console.WriteLine($"=> An error occured while creating the CBZ archive for {bookName}-{chapterId}!");
            if (deleteImagesAfterCbzCreation)
            {
                Console.WriteLine($"=> The scan images won't be deleted so you can create the CBZ manually.\n");
            }
            else
            {
                Console.Write('\n');
            }

            ChangeConsoleColor(ConsoleColor.Red);
            Console.WriteLine($"=> Exception: {ex}\n");

            ResetConsoleColor();
            Console.WriteLine($"=> Press any key to continue...\n");
            Console.ReadKey();
        }
        #endregion

        #region UserInputs
        static ConsoleKey WaitForYesOrNoReadKey(string textDisplayed)
        {
            Console.WriteLine(textDisplayed);
            ConsoleKeyInfo keyEntered = Console.ReadKey();

            while (IsEnteredKeyValid(keyEntered.Key, new ConsoleKey[] { ConsoleKey.Y, ConsoleKey.N }) == false)
            {
                Console.WriteLine(" -> Invalid Key, please press Y (yes) or N (no)");
                keyEntered = Console.ReadKey();
            }
            Console.WriteLine("\n");
            Thread.Sleep(Constants.HALF_SECOND_IN_MILLISECONDS); // Wait one second to make the user has the time to remove his finger from the key
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

        #region Visual

        static void ChangeConsoleColor(ConsoleColor textColor, ConsoleColor backgroundColor=ConsoleColor.Black)
        {
            Console.ForegroundColor = textColor;
            Console.BackgroundColor = backgroundColor;
        }

        static void ResetConsoleColor()
        {
            Console.ResetColor();
        }

        static string AdaptativeLineOfCharForHeader(string header, char charToUseForLine)
        {
            return new string(charToUseForLine, header.Length);
        }
        #endregion

        #region Quit Console
        static void QuitConsole()
        {
            Environment.Exit(0);
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
        #endregion
    }
}
