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

            if(CHAPTERS_TO_DOWNLOAD_URL.Count <= 0)
            {
                ErrorMessageNoChapterUrl();
                QuitConsole();
            }

            Console.WriteLine($"Here is the list of chapters you are going to download:");
            foreach (string url in CHAPTERS_TO_DOWNLOAD_URL)
            {
                Console.WriteLine($"- {GetBookNameFromUrl(url)}_{GetChapterNumberFromUrl(url, false)}");
            }

            Console.WriteLine($"\nThe files will be downloaded in {OUTPUT_DIRECTORY}, a folder will automatically be created for each title and chapters");
            CheckOutputDirectory();

            bool isNo = WaitForYesOrNoReadKey("\nPress Y to start or N to quit...") == ConsoleKey.N;
            if (isNo) QuitConsole();

            // DEBUG ANIME SAME
            List<string> animeSamaDebugUrl = new List<string>()
            {
                //"https://anime-sama.fr/catalogue/berserk/scan/vf/",
                //"https://anime-sama.fr/catalogue/20th-century-boys/scan/vf/",
                //"https://anime-sama.fr/catalogue/alice-in-borderland/scan/vf/",
                "https://anime-sama.fr/catalogue/fairy-tail/scan/vf/",
                "https://anime-sama.fr/catalogue/the-terminally-ill-young-master-of-the-baek-clan/scan/vf/"
            };

            //SaveHtmlFiles(animeSamaDebugUrl);

            // https://anime-sama.fr/s2/scans/Berserk/2/1.jpg
            // https://anime-sama.fr/s2/scans/20th%20Century%20boys/1/1.jpg
            // https://anime-sama.fr/s2/scans/Alice%20in%20Borderland/1/1.jpg
            // https://anime-sama.fr/s2/scans/Fairy%20Tail/1/1.jpg
            // https://anime-sama.fr/s2/scans/The%20Terminally%20Ill%20Young%20Master%20of%20the%20Baek%20Clan/1/1.jpg

            Console.WriteLine($"????????? ANIME SAMA DEBUG ?????????");

            foreach (var item in animeSamaDebugUrl)
            {
                GetImgUrlsFromHtmlContent(item);
            }
            Console.ReadKey();
            return;
            // DEBUG END
            DownloadFromChaptersUrl();

            Console.WriteLine("Finished, press any key to close...");
            Console.ReadKey();

            if (openOutputDirectoryWhenClosing)
            {
                OpenRelevantFolder();                
            }

            QuitConsole();
        }
        #endregion

        #region Downloading
        static void DownloadFromChaptersUrl()
        {
            foreach (string url in CHAPTERS_TO_DOWNLOAD_URL)
            {
                string bookName = GetBookNameFromUrl(url);
                string chapterNumber = GetChapterNumberFromUrl(url);

                string header = $"Download {bookName} - chapter {chapterNumber} from {url}";
                Console.WriteLine($"\n{AdaptativeLineOfCharForHeader(header, '*')}");
                Console.WriteLine(header);
                Console.WriteLine($"{AdaptativeLineOfCharForHeader(header, '*')}");

                List<string> imgsToDownload = GetImgUrlsFromHtmlContent(url);
                if (imgsToDownload.Count == 0) { continue; } // if list (in case of error while getting html content) is empty skip directly to the next url

                // Create output folder if necessary
                string downloadPath = CreateChapterDirectory(bookName, chapterNumber);

                int pageId = 1;
                foreach (string imgUrl in imgsToDownload)
                {
                    string fileExtension = GetFileExtensionFromUrl(imgUrl); 
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

        static bool IsUrlValid(string url, int timeout=1000)
        {
            WebRequest webRequest = WebRequest.Create(url);
            webRequest.Method = "HEAD";
            webRequest.Timeout = timeout;

            try
            {
                WebResponse response = webRequest.GetResponse();
                response.Close();
                return true;
            }
            catch
            {
                return false;
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
                    htmlContent = client.DownloadString(htmlUrl); // Save html code in a variable
                }
                catch (Exception ex)
                {
                    ErrorMessageForHtmlDownload(ex, htmlUrl);
                    return new List<string>();
                }  
            }

            switch (htmlUrl)
            {
                case string a when a.Contains(Constants.SCANVF_DOMAIN_NAME):
                    Debug.WriteLine($"\nUrl is from scan-vf.net: {htmlUrl}");
                    return ParseHtmlForScanVf(htmlContent, htmlUrl);

                case string a when a.Contains(Constants.ANIMESAMA_DOMAIN_NAME):
                    Debug.WriteLine($"\nUrl is from anime-same.fr: {htmlUrl}");
                    return ParseHtmlForAnimeSama(htmlContent, htmlUrl);

                default: // Default, unknown domain name
                    // TODO: Add Error unknown website cannot download scan from this domain
                    Debug.WriteLine($"\nError unknown website cannot download scan from this url: {htmlUrl}");
                    return new List<string>();
            }
        }

        static List<string> ParseHtmlForScanVf(string htmlContent, string htmlUrl)
        {
            string[] splitContent = htmlContent.Split(Constants.SCANVF_URL_BLOCK_START_SEPARATOR, StringSplitOptions.RemoveEmptyEntries); // Split before the block with all the img url
            string urlSplit = splitContent[1]; // Keep the split after our separator (trim the beginning)

            splitContent = urlSplit.Split(Constants.SCANVF_URL_BLOCK_END_SEPARATOR, StringSplitOptions.RemoveEmptyEntries); // Split after the block with all the img url
            urlSplit = splitContent[0]; // Keep the split before our separator (trim the end)

            splitContent = urlSplit.Split(Constants.SCANVF_CLEAN_BEFORE_IMG_TAG_SEPARATOR, StringSplitOptions.RemoveEmptyEntries); // Clean the html code that is still before the first <img/>
            urlSplit = splitContent[1]; // Keep the split after our separator (trim the beginning)

            List<string> imgUrls = urlSplit.Split(Constants.SCANVF_IMG_TAG_END_SEPARATOR, StringSplitOptions.RemoveEmptyEntries).ToList(); // Split each img tag in a list

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
                    imgUrls.RemoveAt(i);
                }
            }

            Debug.WriteLine($"\nList of images url found for {htmlUrl}");
            imgUrls.Log(); // Debug log of list items
            Debug.WriteLine("\n");

            return imgUrls;
        }

        static List<string> ParseHtmlForAnimeSama(string htmlContent, string htmlUrl)
        {
            // V1 Recreate url since we can't get the page after js loading

            // https://anime-sama.fr/s2/scans/Berserk/2/1.jpg
            // https://anime-sama.fr/s2/scans/20th%20Century%20boys/1/1.jpg
            // https://anime-sama.fr/s2/scans/Alice%20in%20Borderland/1/1.jpg
            // https://anime-sama.fr/s2/scans/Fairy%20Tail/1/1.jpg
            // https://anime-sama.fr/s2/scans/The%20Terminally%20Ill%20Young%20Master%20of%20the%20Baek%20Clan/1/1.jpg

            // info needed
            // Book name with correct casing -> <meta name="description" ...
            // Chapter Number -> ask for chapter number
            // number of page (Can deal without it)
            // img extension (Seems to be always jpg)

            // Book Name
            string[] splitContent = htmlContent.Split(Constants.ANIMESAMA_BOOK_NAME_START_SEPARATOR, StringSplitOptions.RemoveEmptyEntries); // Split before the meta tag with the book name
            string bookName = splitContent[1]; // Keep the split after our separator (trim the beginning)

            splitContent = bookName.Split(Constants.ANIMESAMA_BOOK_NAME_END_SEPARATOR, StringSplitOptions.RemoveEmptyEntries); // Split after the book name
            bookName = splitContent[0]; // Keep the split before our separator (trim the end)

            Debug.WriteLine($"\nBook Name found = \"{bookName}\"");

            // Chapter Number
            Console.WriteLine($"\nWrite the number of every chapter you want to download for {htmlUrl} separated by a ; and press enter"); // TODO: Detect range of chapter
            string lineRead = Console.ReadLine();
            Debug.WriteLine($"\nselectedChapters = {lineRead}");

            List<string> chaptersRead = lineRead.Split(Constants.SEMICOLON_CHAR).ToList();
            List<int> selectedChaptersId = new List<int>();
            List<string> selectedChapterUrls = new List<string>();


            for (int i = chaptersRead.Count - 1; i >= 0; i--)
            {
                if (int.TryParse(chaptersRead[i], out int chapterId))
                {
                    Debug.WriteLine($"Valid Chapter number Found: {chapterId} ({htmlUrl})");

                    int firstImgId = 1;
                    string chapterUrl = $"{Constants.ANIMESAMA_IMG_URL_START}{bookName}/{chapterId}/";
                    string firstImgUrl = $"{chapterUrl}{firstImgId}{Constants.JPG_EXTENSION}";

                    // Test if chapter url is valid for first image
                    if (IsUrlValid(firstImgUrl))
                    {
                        Debug.WriteLine($"Valid chapter confirmed, img url is working");
                        selectedChaptersId.Add(chapterId);
                        selectedChapterUrls.Add(chapterUrl);
                    }
                    else
                    {
                        // TODO: Add error management
                        Debug.WriteLine($"Invalid chapter url ({firstImgUrl}) for chapter {chapterId}. Check if chapter really exist, entry will not be added to the chapter list ({htmlUrl})");
                    }
                }
                else
                {
                    // TODO: Add error management
                    Debug.WriteLine($"Cannot parse \"{chaptersRead[i]}\" to int => invalid chapter number: {chapterId}, entry will not be added to the chapter list ({htmlUrl})");
                }
            }

            Debug.WriteLine($"\nChapter Id: ");
            selectedChaptersId.Log();

            Console.WriteLine($"\nList of chapter found for {htmlUrl}: ");
            foreach (int chapterId in selectedChaptersId)
            {
                Console.WriteLine($"-Chapter {chapterId} ");
            }
            Console.WriteLine($"\n Press any key to continue... "); // TODO: Replace by yes/no question
            Console.ReadKey();

            // Build Url
            Debug.WriteLine($"\nChapter url: ");
            selectedChapterUrls.Log();

            // PageNumber
            
            List<string> imgUrls = new List<string>();
            foreach (string chapterUrl in selectedChapterUrls)
            {
                int pageId = 1;
                string imgUrl = $"{chapterUrl}{pageId}{Constants.JPG_EXTENSION}";
                while(IsUrlValid(imgUrl))
                {
                    Console.WriteLine($"Valid img url found: {imgUrl}");
                    Debug.WriteLine($"Valid img url found: {imgUrl}");
                    imgUrls.Add(imgUrl);
                    pageId++;
                    imgUrl = $"{chapterUrl}{pageId}{Constants.JPG_EXTENSION}";
                }
                Console.WriteLine($"Finish to img found valid img url at page {pageId-1}\n");
                Debug.WriteLine($"Finish to img found valid img url at page {pageId-1}\n");
            }

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

            return bookName;
        }

        static string GetChapterNumberFromUrl(string url, bool keepNumberOnly=true)
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

            string chapterNumber = url.Split(Constants.SLASH_CHAR)[isImgUrl ? splitIdForImgUrl : splitIdForChapterUrl];
            if (keepNumberOnly) chapterNumber = chapterNumber.Split(Constants.DASH_CHAR)[1];            
            
            return chapterNumber;
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
            if (CHAPTERS_TO_DOWNLOAD_URL.Count == 1) 
            {
                
                // One chapter downloaded, open this chapter folder
                string chapterDirectory = GetChapterDirectoryPath(CHAPTERS_TO_DOWNLOAD_URL[0]);
                OpenFolder(chapterDirectory);
            }
            else
            {
                bool moreThanOneBook = MoreThanOneBookInUrlList(CHAPTERS_TO_DOWNLOAD_URL);
                if (moreThanOneBook) 
                {
                    // Several books downloaded, open the output folder
                    OpenFolder(OUTPUT_DIRECTORY);
                }
                else 
                {
                    
                    // Several chapters of the same book downloaded, open the book folder
                    string bookDirectory = GetBookDirectoryPath(CHAPTERS_TO_DOWNLOAD_URL[0]);
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

        static string GetBookDirectoryPath(string url)
        {
            string bookName = GetBookNameFromUrl(url);
            string bookDirectoryPath = Path.Combine(OUTPUT_DIRECTORY, $"{bookName}{Constants.SCAN_SUFFIX}");

            return bookDirectoryPath;
        }

        static string GetChapterDirectoryPath(string url)
        {
            string bookName = GetBookNameFromUrl(url);
            string chapterNumber = GetChapterNumberFromUrl(url);
            string chapterDirectoryPath = Path.Combine(OUTPUT_DIRECTORY, $"{bookName}{Constants.SCAN_CHAPTER_PATH}{chapterNumber}");

            return chapterDirectoryPath;
        }

        static bool MoreThanOneBookInUrlList(List<string> urlList)
        {
            if (urlList.Count <= 1) return false;

            string firstUrlBookName = GetBookNameFromUrl(urlList[0]);
            for (int i = 1; i < urlList.Count; i++) // Start at item 1 because we compare with item 0
            {
                string currentUrlBookName = GetBookNameFromUrl(urlList[i]);
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
