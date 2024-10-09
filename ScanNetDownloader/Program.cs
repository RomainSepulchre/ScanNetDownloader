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
using Newtonsoft.Json;


namespace ScanDownloader
{
    internal class Program
    {
        // TODO: Improve menu (draw schematics of the menu and make everything doable directly in app with simple console interface (ex: adding url and chapter, changing parameter, ... ))
        // TODO: Improve chapter selection visual to give a better understanding of what happening
        // TODO: Warn for invalid url as soon as possible (new function chck url validity in ScanWebsiteUrl-> url must contains at least a book name)
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

        /// <summary>
        /// Kept as back for dev test
        /// </summary>
        private static List<string> SCANS_TO_DOWNLOAD_URL = new List<string>
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

        private static readonly Dictionary<string, string> DEFAULT_SCANS_LIST = new Dictionary<string, string>
        {
            {"https://www.scan-vf.net/one_piece/chapitre-1/1",""},
            {"https://anime-sama.fr/catalogue/berserk/scan/vf/","1-3;4;5"}
        };

        private static List<ScanWebsiteUrl> ScanWebsiteUrls;

        private static Settings CurrentSettings => Settings.instance;

        private static string OutputDirectory => string.IsNullOrEmpty(CurrentSettings.CustomFolderPath) ? Constants.USER_DOWNLOAD_FOLDER_PATH : CurrentSettings.CustomFolderPath;

        #region Main
        static void Main(string[] args)
        {
            Console.SetWindowSize(150, 40);

            WriteAppTitle();

            // Load settings
            Settings.instance = LoadSettings(Constants.SETTINGS_JSON_PATH);
            CurrentSettings.Log();

            List<string> scansUrlToDownload = CurrentSettings.ScansUrlAndCorrespondingChapters.Keys.ToList();
            if(scansUrlToDownload.Count <= 0)
            {
                Error.NoScansUrl(nameof(CurrentSettings.ScansUrlAndCorrespondingChapters));
                OpenSettingsJsonFile();
                QuitConsole();
            }

            // Create list of website url
            Console.Clear();
            WriteAppTitle();
            Console.WriteLine($"\nCreation of the list of scan to download...\n");
            ScanWebsiteUrls = CreateListOfScanWebsiteUrl(scansUrlToDownload);
            Thread.Sleep(Constants.HALF_SECOND_IN_MILLISECONDS);

            // Main menu (definitive download list)
            Console.Clear();
            WriteAppTitle();

            Console.WriteLine($"\nHere is the list of scans you are going to download:");
            foreach (ScanWebsiteUrl item in ScanWebsiteUrls)
            {
                Console.WriteLine($"-> {item.BookName} - {item.ChapterId} (source:{item.Url})");
            }

            Console.WriteLine($"\nThe files will be downloaded in {OutputDirectory}, a folder will automatically be created for each title and chapters");
            CheckOutputDirectory();

            bool isNo = WaitForYesOrNoReadKey("\nPress Y to start or N to quit...") == ConsoleKey.N;
            if (isNo) QuitConsole();

            // Main menu (definitive download list)
            Console.Clear();
            WriteAppTitle();

            DownloadScans(ScanWebsiteUrls);

            if (CurrentSettings.ErrorsPauseApp)
            {
                Console.WriteLine("Finished, press any key to close...");
                Console.ReadKey();
            }
            else
            {
                Error.ShowDownloadErrors();
                Console.WriteLine("\nPress any key to close...");
                Console.ReadKey();
            }           

            if (CurrentSettings.OpenOutputDirectoryWhenClosing)
            {
                OpenRelevantFolder();                
            }

            QuitConsole();
        }
        #endregion

        #region Scan Website Url
        static List<ScanWebsiteUrl> CreateListOfScanWebsiteUrl(List<string> urls)
        {
            bool errorOccured = false;
            List<ScanWebsiteUrl> newScanWebsiteUrls = new List<ScanWebsiteUrl>();
            List<int> selectedChaptersId;
            Random random = new Random();
            foreach (string url in urls)
            {
                Debug.WriteLine($"\nURL ==> {url}\n");
                switch (url)
                {
                    case string s when s.Contains(Constants.SCANVF_DOMAIN_NAME):
                        //https://www.scan-vf.net/jujutsu-kaisen/chapitre-164/1 = url with chapter -> at least 5 split
                        //https://www.scan-vf.net/jujutsu-kaisen = url without chapter -> less than 5 split
                        bool chapterIsInUrl = url.Split(Constants.SLASH_CHAR).Count() > 4 ; // Check if the url has a chapter name (the number of split let us know if url stop at book name or not)
                        if (chapterIsInUrl)
                        {
                            ScanWebsiteUrl scanVfNetUrl = new ScanVfNetUrl(url);
                            newScanWebsiteUrls.Add(scanVfNetUrl);
                            Console.WriteLine($"{scanVfNetUrl.BookName} - Chapter {scanVfNetUrl.ChapterId} added ({scanVfNetUrl.WebsiteDomain}).\n");
                        }
                        else
                        {
                            // Chapters to download
                            ScanWebsiteUrl temporaryScanVfNetUrl = new ScanVfNetUrl(url, false);
                            selectedChaptersId = ChapterSelection(temporaryScanVfNetUrl, ref errorOccured);
                            // Create link
                            foreach (int chapterId in selectedChaptersId)
                            {
                                string urlWithChapter = $"{url}{Constants.SCANVF_CHAPTER_IN_URL}{chapterId}"; // No need to specify "/1" after chapter number redirection is done by website
                                ScanWebsiteUrl scanVfNetUrl = new ScanVfNetUrl(urlWithChapter);
                                newScanWebsiteUrls.Add(scanVfNetUrl);
                                Console.WriteLine($" -> {scanVfNetUrl.BookName} - Chapter {scanVfNetUrl.ChapterId} added ({scanVfNetUrl.WebsiteDomain}).");
                            }
                            Console.WriteLine("");
                        }                        
                        break;

                    case string s when s.Contains(Constants.ANIMESAMA_DOMAIN_NAME):
                        ScanWebsiteUrl temporaryAnimeSamaUrl = new AnimeSamaFrUrl(url); // Temporary obj to get book name

                        // TODO: If possible manage Book URL instead of chapter url
                        // -> check if chapter url is stable or if it changes too much

                        // Get chapters to download
                        selectedChaptersId = ChapterSelection(temporaryAnimeSamaUrl, ref errorOccured);                       

                        // Create link
                        foreach (int chapterId in selectedChaptersId)
                        {
                            ScanWebsiteUrl animeSamaUrl = new AnimeSamaFrUrl(url, chapterId);
                            newScanWebsiteUrls.Add(animeSamaUrl);
                            Console.WriteLine($" -> {animeSamaUrl.BookName} - Chapter {animeSamaUrl.ChapterId} added ({animeSamaUrl.WebsiteDomain}).");
                        }
                        Console.WriteLine("");
                        break;

                    default: // Default, unknown domain name
                        errorOccured = true;
                        Error.UnknownScanWebDomain(url);
                        break;
                }
            }

            if (errorOccured)
            {
                Console.WriteLine($"Make sure to check the errors and press any key to continue...");
                Console.ReadKey();
            }

            return newScanWebsiteUrls;
        }

        static List<int> ChapterSelection(ScanWebsiteUrl scanUrl, ref bool errorOccured)
        {
            List<int> selectedChaptersId = new List<int>();
            string chaptersSelectedInSettings = CurrentSettings.ScansUrlAndCorrespondingChapters[scanUrl.Url];
            if (string.IsNullOrEmpty(chaptersSelectedInSettings) == false)
            {
                selectedChaptersId = ParseToFindChapters(scanUrl, chaptersSelectedInSettings, ref errorOccured);
                if (selectedChaptersId.Count <= 0)
                {
                    selectedChaptersId = AskUserToProvideChapters(scanUrl, ref errorOccured);
                }
                else
                {
                    selectedChaptersId.Log();
                }
            }
            else
            {
                selectedChaptersId = AskUserToProvideChapters(scanUrl, ref errorOccured);
            }
            selectedChaptersId.Sort();
            return selectedChaptersId;
        }

        static List<int> AskUserToProvideChapters(ScanWebsiteUrl scanUrl, ref bool errorOccured)
        {
            Console.WriteLine($"Select chapters for \"{scanUrl.BookName}\" ({scanUrl.Url}):");
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.WriteLine($"Write a range of chapter (ex: 1-10) or the number of the chapters you want to download separated by ; (ex:4;6;8) and press enter.");
            Console.ResetColor();
            string lineRead = Console.ReadLine();

            List<int> chaptersFound = ParseToFindChapters(scanUrl, lineRead, ref errorOccured);
            return chaptersFound;

        }

        static List<int> ParseToFindChapters(ScanWebsiteUrl scanUrl, string stringToParse, ref bool errorOccured)
        {
            List<int> validChapters = new List<int>();
            List<string> chaptersEnteredByUser = stringToParse.Split(Constants.SEMICOLON_CHAR).ToList();

            for (int i = chaptersEnteredByUser.Count - 1; i >= 0; i--)
            {
                // Detect range of chapter
                string[] rangeSplitAttempt = chaptersEnteredByUser[i].Split(Constants.DASH_CHAR);
                if (rangeSplitAttempt.Count() == 2) // Range of chapter
                {
                    // Get start and end of range
                    bool startParsed = int.TryParse(rangeSplitAttempt[0], out int startRange);
                    bool endParsed = int.TryParse(rangeSplitAttempt[1], out int endRange);

                    if (startParsed == false || endParsed == false)
                    {
                        errorOccured = true;
                        Error.FailedToParseChapterEnteredByUser(scanUrl, chaptersEnteredByUser[i]);
                        continue;
                    }
                    else
                    {
                        if (startRange > endRange) (startRange, endRange) = (endRange, startRange); // invert

                        for (int j = startRange; j <= endRange; j++)
                        {
                            validChapters.Add(j);
                        }
                    }
                }
                else // Single chapter
                {
                    if (int.TryParse(chaptersEnteredByUser[i], out int chapterId))
                    {
                        validChapters.Add(chapterId);
                    }
                    else
                    {
                        errorOccured = true;
                        Error.FailedToParseChapterEnteredByUser(scanUrl, chaptersEnteredByUser[i]);
                    }
                }
            }

            return validChapters;
        }

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

                Console.WriteLine($"\nLook for images url for {bookName}-{chapterNumber} at {url}...");
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
                            Error.FailedImageDownload(ex, imgUrl);
                        }
                    }
                    pageId++;
                }

                if (CurrentSettings.CreateCbzArchive)
                {
                    BuildCbzArchive(scanUrl, downloadPath);
                }
            }
        }
        #endregion

        #region Folder Management
        static void CheckOutputDirectory()
        {
            if (Directory.Exists(OutputDirectory) == false)
            {
                Error.NoOutputDirectory();

                Console.WriteLine($"Do you want to create the directory \"{OutputDirectory}\" ? ");

                if (WaitForYesOrNoReadKey($"Press Y (yes) or N (no)...") == ConsoleKey.Y)
                {
                    Directory.CreateDirectory(OutputDirectory);
                    Console.WriteLine($"\"{OutputDirectory}\" sucessfully created. Ready to download!");
                }
                else
                {
                    Console.WriteLine("Please modify the output directory in Settings.json, it must be a valid directory.");
                    if(CurrentSettings.AutoOpenJsonWhenNecessary) Console.WriteLine("Press any key to open Settings.json and close the app...");
                    else Console.WriteLine("Press any key to close the app...");
                    Console.ReadKey();

                    OpenSettingsJsonFile();
                    QuitConsole();
                }
            }
        }

        static string CreateChapterDirectory(string bookName, string chapterNumber)
        {
            
            string chapterDirectory = Path.Combine(OutputDirectory, $"{bookName}{Constants.SCAN_CHAPTER_PATH}{chapterNumber}");

            if (Directory.Exists(chapterDirectory) == false)
            {
                Directory.CreateDirectory(chapterDirectory);
            }
            return chapterDirectory;
        }

        static void OpenRelevantFolder()
        {
            if (ScanWebsiteUrls.Count == 1) 
            {
                // One chapter downloaded, open this chapter folder
                string chapterDirectory = GetChapterDirectoryPath(ScanWebsiteUrls[0]);
                OpenFolder(chapterDirectory);
            }
            else
            {
                bool moreThanOneBook = MoreThanOneBookInUrlList(ScanWebsiteUrls);
                if (moreThanOneBook) 
                {
                    // Several books downloaded, open the output folder
                    OpenFolder(OutputDirectory);
                }
                else 
                {
                    // Several chapters of the same book downloaded, open the book folder
                    string bookDirectory = GetBookDirectoryPath(ScanWebsiteUrls[0]);
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
            string bookDirectoryPath = Path.Combine(OutputDirectory, $"{bookName}{Constants.SCAN_SUFFIX}");

            return bookDirectoryPath;
        }

        static string GetChapterDirectoryPath(ScanWebsiteUrl scanUrl)
        {
            string bookName = scanUrl.BookName;
            string chapterNumber = scanUrl.ChapterId.ToString();
            string chapterDirectoryPath = Path.Combine(OutputDirectory, $"{bookName}{Constants.SCAN_CHAPTER_PATH}{chapterNumber}");

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

        #region Settings
        static Settings LoadSettings(string jsonPath)
        {
            Settings loadedSettings;
            if (File.Exists(jsonPath))
            {
                try
                {
                    loadedSettings = JsonConvert.DeserializeObject<Settings>(File.ReadAllText(Constants.SETTINGS_JSON_PATH));
                    return loadedSettings;
                }
                catch (Exception ex)
                {
                    Error.FailedToLoadSettingsJson(jsonPath, ex);

                    Console.WriteLine($"Do you want to reset settings.json to it's default values ? ");
                    if (WaitForYesOrNoReadKey($"Press Y (yes) or N (no)...") == ConsoleKey.Y)
                    {
                        loadedSettings = ResetToDefaultSettings();
                        return loadedSettings;
                    }
                    else
                    {
                        Console.WriteLine("Please make sure nothing is wrong with the value in Settings.json, if the problem persist backup your settings and reset the json to it's default values.");
                        if (CurrentSettings.AutoOpenJsonWhenNecessary) Console.WriteLine("Press any key to open Settings.json and close the app...");
                        else Console.WriteLine("Press any key close the app...");
                        Console.ReadKey();

                        OpenSettingsJsonFile();
                        QuitConsole();
                        return null;
                    }
                }
            }
            else // Missing Settings.json
            {
                Error.MissingSettingsJson(jsonPath);
                loadedSettings = ResetToDefaultSettings();
                return loadedSettings;
            }
        }

        static Settings ResetToDefaultSettings()
        {
            Settings defaultSettings = new Settings() // Use default Settings
            {
                ScansUrlAndCorrespondingChapters = DEFAULT_SCANS_LIST
                // Other settings already have default value asssigned
            };
            SaveSettings(defaultSettings);
            return defaultSettings;
        }

        static void SaveSettings(Settings newSettings)
        {
            Settings.instance = newSettings;
            try
            {
                File.WriteAllText(Constants.SETTINGS_JSON_PATH, JsonConvert.SerializeObject(newSettings, Formatting.Indented));
            }
            catch (Exception ex)
            {
                Error.FailedToSaveSettingsJson(Constants.SETTINGS_JSON_PATH, ex);
            }  
        }

        static void OpenSettingsJsonFile()
        {
            if (CurrentSettings.AutoOpenJsonWhenNecessary)
            {
                Process.Start(Constants.SETTINGS_JSON_PATH);
            }    
        }
        #endregion

        #region Cbz Archive
        static void BuildCbzArchive(ScanWebsiteUrl scanUrl, string downloadPath)
        {
            Console.WriteLine($"=> Create .CBZ for {scanUrl.BookName}-{scanUrl.ChapterId}...");

            string bookName = scanUrl.BookName;
            string chapterNumber = scanUrl.ChapterId.ToString();

            string folderToArchive = downloadPath;
            string cbzFilePath = Path.Combine(Directory.GetParent(downloadPath).FullName, $"{bookName} - chapter {chapterNumber}.cbz");

            if (Directory.EnumerateFileSystemEntries(folderToArchive).Any() == false)
            {
                Console.WriteLine($"=> No images downloaded for {bookName}-{chapterNumber}, CBZ creation will be skipped!\n");
                return;
            }

            if (File.Exists(cbzFilePath) == false)
            {
                try
                {
                    ZipFile.CreateFromDirectory(folderToArchive, cbzFilePath);
                    Console.WriteLine($"=> {bookName}-{chapterNumber} .CBZ successfully created!\n");
                }
                catch (IOException ex)
                {
                    Error.FailedCbzCreation(ex, scanUrl, CurrentSettings.DeleteImagesAfterCbzCreation);
                    return;
                }
            }
            else // a cbz file already exist
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
                    catch (IOException ex)
                    {
                        Error.FailedToReplaceEmptyCbz(ex, scanUrl, CurrentSettings.DeleteImagesAfterCbzCreation);
                        return;
                    }
                }
            }

            if (CurrentSettings.DeleteImagesAfterCbzCreation)
            {
                if (Directory.Exists(downloadPath)) Directory.Delete(downloadPath, true);
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

        #region Visual and Ui
        static void WriteAppTitle()
        {
            string appTitle = $"$$$--- SCAN.NET DOWNLOADER ---$$$";
            Console.WriteLine($"\n{AdaptativeLineOfCharForHeader(appTitle, '$')}");
            Console.WriteLine(appTitle);
            Console.WriteLine($"{AdaptativeLineOfCharForHeader(appTitle, '$')}\n");
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
                    client.DownloadFile(urlToDl, Path.Combine(OutputDirectory, htmlFileName));

                    Console.WriteLine($"\n {htmlFileName} downloaded...");
                }
            }
            Console.WriteLine($"Html file saved, press to open folder location...");
            Console.ReadKey();
            CurrentSettings.OpenOutputDirectoryWhenClosing = false;
            OpenFolder(OutputDirectory);
        }
        #endregion
    }
}
