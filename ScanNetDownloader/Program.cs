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
        // TODO: Manage weird image format from anime-same by cropping image automatically
        // TODO: Select Output folder by opening explorer window 
        // TODO: Switch from console app to an interface (WPF ?)
        // TODO: Scrap a list of all the books available and create a search engine

        // KEEP FOR TEST
        // https://anime-sama.fr/catalogue/berserk/scan/vf/
        // https://anime-sama.fr/catalogue/20th-century-boys/scan/vf/
        // https://anime-sama.fr/catalogue/alice-in-borderland/scan/vf/
        // https://anime-sama.fr/catalogue/fairy-tail/scan/vf/
        // https://anime-sama.fr/catalogue/the-terminally-ill-young-master-of-the-baek-clan/scan/vf/

        #region Parameters
        /// <summary>
        /// Provide the url of the first page of any chapter you want to download
        /// </summary>
        private static readonly List<string> CHAPTERS_TO_DOWNLOAD_URL = new List<string>
        {
            "https://anime-sama.fr/catalogue/20th-century-boys/scan/vf/",
            "https://www.scan-vf.net/one_piece/chapitre-1079/1",
            "https://www.scan-vf.net/one_piece/chapitre-1120/5",
            "https://www.scan-vf.net/one_piece/chapitre-140/1",
            "https://www.scan-vf.net/one_piece/chapitre-1087/7",
            "https://anime-sama.fr/catalogue/berserk/scan/vf/",
            "https://anime-sama.fr/catalogue/alice-in-borderland/scan/vf/",
            "https://www.scan-vf.net/jujutsu-kaisen/chapitre-268/1",
            "https://www.scan-vf.net/dragon-Ball-Super/chapitre-73/4",
            "https://anime-sama.fr/catalogue/fairy-tail/scan/vf/",
            "https://www.scan-vf.net/my-hero-academia/chapitre-358/2"
        };

        private static List<ScanWebsiteUrl> scanWebsiteUrls;

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
            WriteAppTitle();

            if(CHAPTERS_TO_DOWNLOAD_URL.Count <= 0)
            {
                ErrorMessageNoChapterUrl();
                QuitConsole();
            }

            // Create list of website url
            Console.WriteLine($"\nCreation of the list of scan to download...");
            scanWebsiteUrls = CreateListOfScanWebsiteUrl(CHAPTERS_TO_DOWNLOAD_URL);

            Thread.Sleep(Constants.HALF_SECOND_IN_MILLISECONDS);
            Console.Clear();
            WriteAppTitle();

            Console.WriteLine($"\nHere is the list of chapters you are going to download:");
            foreach (ScanWebsiteUrl item in scanWebsiteUrls)
            {
                Console.WriteLine($"- {item.BookName} - {item.ChapterId} (source:{item.Url})");
            }

            Console.WriteLine($"\nThe files will be downloaded in {OUTPUT_DIRECTORY}, a folder will automatically be created for each title and chapters");
            CheckOutputDirectory();

            bool isNo = WaitForYesOrNoReadKey("\nPress Y to start or N to quit...") == ConsoleKey.N;
            if (isNo) QuitConsole();
             
            DownloadScans(scanWebsiteUrls);

            Console.WriteLine("Finished, press any key to close...");
            Console.ReadKey();

            if (openOutputDirectoryWhenClosing)
            {
                OpenRelevantFolder();                
            }

            QuitConsole();
        }
        #endregion

        static List<ScanWebsiteUrl> CreateListOfScanWebsiteUrl(List<string> urls)
        {
            List<ScanWebsiteUrl> newScanWebsiteUrls = new List<ScanWebsiteUrl>();
            Random random = new Random();
            foreach (string url in urls)
            {
                // TODO: Test Url before ? Its probably overengeenering
                switch (url)
                {
                    case string s when s.Contains(Constants.SCANVF_DOMAIN_NAME):
                        ScanWebsiteUrl scanVfNetUrl = new ScanVfNetUrl(url);
                        newScanWebsiteUrls.Add(scanVfNetUrl);
                        break;

                    case string s when s.Contains(Constants.ANIMESAMA_DOMAIN_NAME):
                        ScanWebsiteUrl temporaryAnimeSamaUrl = new AnimeSamaFrUrl(url); // Temporary obj to get book name
                        // Ask For chapter to download
                        bool debug = true;
                        List<int> selectedChaptersId;
                        
                        if (debug) // TODO: CLEAN DEBUG
                        {
                            // Automatic Chapter Assignement
                            selectedChaptersId = new List<int>();
                            for (int i = 0; i < 3; i++)
                            {
                                int randomId = random.Next(1, 100);
                                selectedChaptersId.Add(randomId);
                            }
                        }
                        else
                        {
                            Console.WriteLine($"\nSelect chapters for \"{temporaryAnimeSamaUrl.BookName}\" (source:{temporaryAnimeSamaUrl.Url})");
                            Console.WriteLine($" -> Write the number of every chapter you want to download for \"{temporaryAnimeSamaUrl.BookName}\" separated by ; and press enter");
                            string lineRead = Console.ReadLine();
                            Debug.WriteLine($"\nselectedChapters = {lineRead}");

                            // TODO: Detect range of chapters and improve chapter selection
                            List<string> chaptersEnteredByUser = lineRead.Split(Constants.SEMICOLON_CHAR).ToList();
                            selectedChaptersId = new List<int>();

                            for (int i = chaptersEnteredByUser.Count - 1; i >= 0; i--)
                            {
                                if (int.TryParse(chaptersEnteredByUser[i], out int chapterId))
                                {
                                    Debug.WriteLine($"Valid Chapter number Found: {chapterId} ({url})");
                                    selectedChaptersId.Add(chapterId);
                                }
                                else
                                {
                                    // TODO: Add error management
                                    Debug.WriteLine($"Cannot parse \"{chaptersEnteredByUser[i]}\" to int => invalid chapter number: {chapterId}, entry will not be added to the chapter list ({url})");
                                }
                            }
                        }
                        selectedChaptersId.Sort();

                        // Create link
                        foreach (int chapterId in selectedChaptersId)
                        {
                            ScanWebsiteUrl animeSamaUrl = new AnimeSamaFrUrl(url, chapterId);
                            newScanWebsiteUrls.Add(animeSamaUrl);
                        }
                        break;

                    default: // Default, unknown domain name
                             // TODO: Add Error unknown website cannot download scan from this domain
                        Debug.WriteLine($"\nError unknown website cannot download scan from this url: {url}");
                        break;
                }
            }
            return newScanWebsiteUrls;
        }

        #region Downloading
        static void DownloadScans(List<ScanWebsiteUrl> scansToDownload)
        {
            foreach (ScanWebsiteUrl scanUrl in scansToDownload)
            {
                string url = scanUrl.Url;
                string bookName = scanUrl.BookName;
                string chapterNumber = scanUrl.ChapterId.ToString();

                string header = $"Download {bookName} - chapter {chapterNumber} from {url}";
                Console.WriteLine($"\n{AdaptativeLineOfCharForHeader(header, '*')}");
                Console.WriteLine(header);
                Console.WriteLine($"{AdaptativeLineOfCharForHeader(header, '*')}");

                Console.WriteLine($"\nLook for images url for {url}...");
                List<string> imgsToDownload = scanUrl.GetScanImagesUrl();
                if (imgsToDownload.Count == 0) { continue; } // if list is empty (in case of error while getting html content) skip directly to the next url

                // Create output folder if necessary
                string downloadPath = CreateChapterDirectory(bookName, chapterNumber);

                int pageId = 1;
                foreach (string imgUrl in imgsToDownload)
                {
                    string fileExtension = scanUrl.GetFileExtensionFromUrl(imgUrl);
                    string imgName = $"{bookName}_{chapterNumber}-{pageId.ToString("D3")}{fileExtension}";  
                    string downloadFile = Path.Combine(downloadPath, imgName);

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
                    CreateCbzArchive(bookName, chapterNumber, downloadPath);
                }
            }
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

        static string CreateChapterDirectory(string bookName, string chapterNumber)
        {
            
            string chapterDirectory = Path.Combine(OUTPUT_DIRECTORY, $"{bookName}{Constants.SCAN_CHAPTER_PATH}{chapterNumber}");

            if (Directory.Exists(chapterDirectory) == false)
            {
                Directory.CreateDirectory(chapterDirectory);
            }
            return chapterDirectory;
        }

        static void OpenRelevantFolder()
        {
            if (scanWebsiteUrls.Count == 1) 
            {
                // One chapter downloaded, open this chapter folder
                string chapterDirectory = GetChapterDirectoryPath(scanWebsiteUrls[0]);
                OpenFolder(chapterDirectory);
            }
            else
            {
                bool moreThanOneBook = MoreThanOneBookInUrlList(scanWebsiteUrls);
                if (moreThanOneBook) 
                {
                    // Several books downloaded, open the output folder
                    OpenFolder(OUTPUT_DIRECTORY);
                }
                else 
                {
                    // Several chapters of the same book downloaded, open the book folder
                    string bookDirectory = GetBookDirectoryPath(scanWebsiteUrls[0]);
                    OpenFolder(bookDirectory);
                }
            }
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

        static string GetBookDirectoryPath(ScanWebsiteUrl scanUrl)
        {
            string bookName = scanUrl.BookName;
            string bookDirectoryPath = Path.Combine(OUTPUT_DIRECTORY, $"{bookName}{Constants.SCAN_SUFFIX}");

            return bookDirectoryPath;
        }

        static string GetChapterDirectoryPath(ScanWebsiteUrl scanUrl)
        {
            string bookName = scanUrl.BookName;
            string chapterNumber = scanUrl.ChapterId.ToString();
            string chapterDirectoryPath = Path.Combine(OUTPUT_DIRECTORY, $"{bookName}{Constants.SCAN_CHAPTER_PATH}{chapterNumber}");

            return chapterDirectoryPath;
        }

        static bool MoreThanOneBookInUrlList(List<ScanWebsiteUrl> urlList)
        {
            if (urlList.Count <= 1) return false;

            string firstUrlBookName = urlList[0].BookName;
            for (int i = 1; i < urlList.Count; i++) // Start at item 1 because we always compare with item 0
            {
                string currentUrlBookName = urlList[i].BookName;
                if (string.Equals(firstUrlBookName, currentUrlBookName) == false)
                {
                    return true;
                }
            }

            return false;
        }
        #endregion

        #region Cbz Archive
        static void CreateCbzArchive(string bookName, string chapterNumber, string downloadPath)
        {
            Console.WriteLine($"=> Create .CBZ for {bookName}-{chapterNumber}...");

            string folderToArchive = downloadPath;
            string cbzFilePath = Path.Combine(Directory.GetParent(downloadPath).FullName, $"{bookName}-chapter{chapterNumber}.cbz");

            if (File.Exists(cbzFilePath) == false)
            {
                try
                {
                    ZipFile.CreateFromDirectory(folderToArchive, cbzFilePath);
                    Console.WriteLine($"=> {bookName}-{chapterNumber} .CBZ successfully created!\n");
                }
                catch (Exception ex)
                {
                    ErrorMessageForCbzCreation(ex, bookName, chapterNumber);
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
                        Console.WriteLine($"=> {bookName}-{chapterNumber} .CBZ successfully created!\n");
                    }
                    catch (Exception ex)
                    {
                        ErrorMessageForCbzCreation(ex, bookName, chapterNumber);
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

        static void ErrorMessageNoChapterUrl()
        {
            ChangeConsoleColor(ConsoleColor.DarkRed);
            Console.WriteLine($"No chapter URL have been provided.");
            Console.WriteLine($"Add the URL of the chapter you want to download in the list {nameof(CHAPTERS_TO_DOWNLOAD_URL)}.\n");

            ResetConsoleColor();
            Console.WriteLine($"Press any key to close the app...");
            Console.ReadKey();
        }

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

        static void ErrorMessageForCbzCreation(Exception ex, string bookName, string chapterNumber)
        {
            Debug.WriteLine($"=> Error while creating CBZ archive for {bookName}-{chapterNumber}: {ex}");

            ChangeConsoleColor(ConsoleColor.DarkRed);

            Console.WriteLine($"=> An error occured while creating the CBZ archive for {bookName}-{chapterNumber}!");
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
        #endregion // TODO: MOVE ERRORS IN A DEDICATED CLASS

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
        static void WriteAppTitle()
        {
            string appTitle = $"$$$--- SCAN.NET DOWNLOADER ---$$$";
            Console.WriteLine($"\n{AdaptativeLineOfCharForHeader(appTitle, '$')}");
            Console.WriteLine(appTitle);
            Console.WriteLine($"{AdaptativeLineOfCharForHeader(appTitle, '$')}\n");
        }

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
