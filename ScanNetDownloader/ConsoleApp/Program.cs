using Newtonsoft.Json;
using ScanNetDownloader.View;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Windows;


namespace ScanNetDownloader.ConsoleApp
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

        private static Window mainWindow = new Window();

        public static event EventHandler<string> DlInfoWriteLine;

        public static event EventHandler<float> UpdatDlProgressBar;

        #region Events
        // TODO: Check if work correctly before Cleaning
        public static void WriteDlInfoLine(string lineToAdd)
        {
            Debug.WriteLine(lineToAdd);
            if (DlInfoWriteLine != null)
            {
                DlInfoWriteLine(null, lineToAdd); 
            }
        }

        public static void UpdateDownloadProgress(float percentageDone)
        {
            if (UpdatDlProgressBar != null)
            {
                UpdatDlProgressBar(null, percentageDone);
            }
        }
        #endregion


        #region Main

        public static async void StartDownloader(Window _mainWindow)
        {
            mainWindow = _mainWindow;

            WriteDlInfoLine($"\nHere is the list of scans you are going to download:");
            foreach (ScanWebsiteUrl item in ScanWebsiteUrls)
            {
                WriteDlInfoLine($"-> {item.BookName} - {item.ChapterId} (source:{item.Url})");
            }

            WriteDlInfoLine($"\nThe files will be downloaded in {OutputDirectory}, a folder will automatically be created for each title and chapters");
            CheckOutputDirectory();

            bool isNo = WaitForYesOrNoMsgBox("\nDo you to start the download ?") == MessageBoxResult.No;
            if (isNo) QuitApp();

            // Main menu (definitive download list)
            WriteAppTitle();

            await DownloadScans(ScanWebsiteUrls);

            if (CurrentSettings.ErrorsPauseApp)
            {
                WriteDlInfoLine("Finished, press any key to close...");
                MessageBox.Show("Finished, press any key to close...", "Finished", MessageBoxButton.OK, MessageBoxImage.None);
            }
            else
            {
                Error.ShowDownloadErrors();
                WriteDlInfoLine("\nPress any key to close...");
                MessageBox.Show("Finished with error, press any key to close...", "Finished", MessageBoxButton.OK, MessageBoxImage.None);
            }           

            if (CurrentSettings.OpenOutputDirectoryWhenClosing)
            {
                OpenRelevantFolder();                
            }

            QuitApp();
        }
        #endregion

        #region Scan Website Url

        public static List<ScanWebsiteUrl> LoadSavedScanWebsiteUrl()
        {
            // Create Url obj from settings
            List<string> scansUrlToDownload = Settings.instance.ScansUrlAndCorrespondingChapters.Keys.ToList(); // TODO: Save ScanWebsiteUrl separetely from the Settings

            // TODO: Not needed anymore, kept for now but clean later
            //if (scansUrlToDownload.Count <= 0)
            //{
            //    Error.NoScansUrl(nameof(Settings.instance.ScansUrlAndCorrespondingChapters));
            //    OpenSettingsJsonFile();
            //    QuitApp();
            //}

            Debug.WriteLine($"\nCreation of the list of scan to download...\n");
            ScanWebsiteUrls = CreateListOfScanWebsiteUrl(scansUrlToDownload);
            return ScanWebsiteUrls;
        }

        public static List<ScanWebsiteUrl> CreateListOfScanWebsiteUrl(List<string> urls)
        {
            bool errorOccured = false;
            List<ScanWebsiteUrl> newScanWebsiteUrls = new List<ScanWebsiteUrl>();
            List<int> selectedChaptersId;

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
                            Debug.WriteLine($"{scanVfNetUrl.BookName} - Chapter {scanVfNetUrl.ChapterId} added ({scanVfNetUrl.WebsiteDomain}).\n");
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
                                // TODO: we neveer check if chapter exist with ScanVf ?
                                Debug.WriteLine($" -> {scanVfNetUrl.BookName} - Chapter {scanVfNetUrl.ChapterId} added ({scanVfNetUrl.WebsiteDomain}).");
                            }
                            Debug.WriteLine("");
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
                            Debug.WriteLine($" -> {animeSamaUrl.BookName} - Chapter {animeSamaUrl.ChapterId} added ({animeSamaUrl.WebsiteDomain}).");
                        }
                        Debug.WriteLine("");
                        break;

                    default: // Default, unknown domain name
                        errorOccured = true;
                        Error.UnknownScanWebDomain(url);
                        break;
                }
            }

            if (errorOccured)
            {
                Debug.WriteLine($"Make sure to check the errors and press any key to continue...");
                MessageBox.Show("Make sure to check the errors and press any key to continue...", "Check errrors", MessageBoxButton.OK, MessageBoxImage.Warning);
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
            Debug.WriteLine($"Select chapters for \"{scanUrl.BookName}\" ({scanUrl.Url}):");
            Debug.WriteLine($"Write a range of chapter (ex: 1-10) or the number of the chapters you want to download separated by ; (ex:4;6;8) and press enter.");

            InputPopUp inputPopUp = new InputPopUp(mainWindow, $"Select chapters for \"{scanUrl.BookName}\" ({scanUrl.Url}):");
            mainWindow.Opacity = 0.4;
            inputPopUp.ShowDialog();
            mainWindow.Opacity = 1;

            string userTxtInput = inputPopUp.Input;

            List<int> chaptersFound = ParseToFindChapters(scanUrl, userTxtInput, ref errorOccured);
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

        static async Task DownloadScans(List<ScanWebsiteUrl> scansToDownload)
        {
            float progress = 0;
            float minProgress = 0;
            float maxProgress = 0;

            foreach (ScanWebsiteUrl scanUrl in scansToDownload)
            {
                minProgress = maxProgress;
                int scanIndex = scansToDownload.IndexOf(scanUrl);
                maxProgress = (((float)scanIndex + 1) / (scansToDownload.Count)) * 100;
                

                string url = scanUrl.Url;
                string bookName = scanUrl.BookName;
                string chapterNumber = scanUrl.ChapterId.ToString();

                string header = $"Download {bookName} - chapter {chapterNumber} from {url}";
                WriteDlInfoLine($"\n{AdaptativeLineOfCharForHeader(header, '*')}");
                WriteDlInfoLine(header);
                WriteDlInfoLine($"{AdaptativeLineOfCharForHeader(header, '*')}");

                WriteDlInfoLine($"\nLook for images url for {bookName}-{chapterNumber} at {url}...");
                List<string> imgsToDownload = scanUrl.GetScanImagesUrl();
                if (imgsToDownload.Count == 0) { continue; } // if list is empty (in case of error while getting html content) skip directly to the next url

                // Create output folder if necessary
                string downloadPath = CreateChapterDirectory(bookName, chapterNumber);

                int pageId = 1;
                foreach (string imgUrl in imgsToDownload)
                {
                    int pageIndex = imgsToDownload.IndexOf(imgUrl);
                    float chapterCompletion = (float)pageIndex / (imgsToDownload.Count - 1);
                    progress = float.Lerp(minProgress, maxProgress, chapterCompletion);
                    UpdateDownloadProgress(progress);

                    string fileExtension = scanUrl.GetFileExtensionFromUrl(imgUrl);
                    string imgName = $"{bookName}_{chapterNumber}-{pageId.ToString("D3")}{fileExtension}";  
                    string downloadFile = Path.Combine(downloadPath, imgName);


                    using (WebClient client = new WebClient())
                    {
                        try
                        {
                            WriteDlInfoLine($"\nDownloading {imgName} from {imgUrl}");
                            WriteDlInfoLine($"...");

                            if (File.Exists(downloadFile) == true && File.ReadAllBytes(downloadFile).Length > 0 == true)
                            {
                                WriteDlInfoLine($"File already downloaded!\n");
                            }
                            else
                            {
                                await client.DownloadFileTaskAsync(new Uri(imgUrl), downloadFile);
                                WriteDlInfoLine($"Sucessfully downloaded!\n");
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
            // TODO: If no custom directory set ask if the user want to select a new one or if he's ok with the one selected

            if (Directory.Exists(OutputDirectory) == false)
            {
                Error.NoOutputDirectory();

                WriteDlInfoLine($"Do you want to create the directory \"{OutputDirectory}\" ? ");

                if (WaitForYesOrNoMsgBox($"Do you want to create the directory \"{OutputDirectory}\" ? ") == MessageBoxResult.Yes)
                {
                    Directory.CreateDirectory(OutputDirectory);
                    WriteDlInfoLine($"\"{OutputDirectory}\" sucessfully created. Ready to download!");
                }
                else
                {
                    WriteDlInfoLine("Please modify the output directory in Settings.json, it must be a valid directory.");
                    if (CurrentSettings.AutoOpenJsonWhenNecessary) // TODO: this is done several time, this could be a single function
                    {
                        WriteDlInfoLine("Press any key to open Settings.json and close the app...");
                        MessageBox.Show("Press ok to open Settings.json and close the app...", "Quit app", MessageBoxButton.OK, MessageBoxImage.Warning);
                    }
                    else
                    {
                        WriteDlInfoLine("Press any key to close the app...");
                        MessageBox.Show("The app will be closed...", "Quit app", MessageBoxButton.OK, MessageBoxImage.Warning);
                    }
                    

                    OpenSettingsJsonFile();
                    QuitApp();
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

        public static void InitializeAppSettings()
        {
            Settings.instance = LoadSettings(Constants.SETTINGS_JSON_PATH);
            Settings.instance.Log();
        }

        public static Settings LoadSettings(string jsonPath)
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

                    Debug.WriteLine($"Do you want to reset settings.json to it's default values ?");
                    if (WaitForYesOrNoMsgBox($"Do you want to reset settings.json to it's default values ?") == MessageBoxResult.Yes)
                    {
                        loadedSettings = ResetToDefaultSettings();
                        return loadedSettings;
                    }
                    else
                    {
                        Debug.WriteLine("Please make sure nothing is wrong with the value in Settings.json, if the problem persist backup your settings and reset the json to it's default values.");
                        if (CurrentSettings.AutoOpenJsonWhenNecessary)
                        {
                            Debug.WriteLine("Press any key to open Settings.json and close the app...");
                            MessageBox.Show("Press ok to open Settings.json and close the app...", "Quit app", MessageBoxButton.OK, MessageBoxImage.Warning);
                        }
                        else
                        {
                            Debug.WriteLine("Press any key close the app...");
                            MessageBox.Show("The app will be closed...", "Quit app", MessageBoxButton.OK, MessageBoxImage.Warning);
                        } 

                        OpenSettingsJsonFile();
                        QuitApp();
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

        public static void SaveSettings(Settings newSettings)
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

        public static void OpenSettingsJsonFile()
        {
            if (CurrentSettings.AutoOpenJsonWhenNecessary)
            {
                new Process { StartInfo = new ProcessStartInfo(Constants.SETTINGS_JSON_PATH) { UseShellExecute = true } }.Start();
                //Process.Start(Constants.SETTINGS_JSON_PATH);
            }    
        }
        #endregion

        #region Cbz Archive
        static void BuildCbzArchive(ScanWebsiteUrl scanUrl, string downloadPath)
        {
            WriteDlInfoLine($"=> Create .CBZ for {scanUrl.BookName}-{scanUrl.ChapterId}...");

            string bookName = scanUrl.BookName;
            string chapterNumber = scanUrl.ChapterId.ToString();

            string folderToArchive = downloadPath;
            string cbzFilePath = Path.Combine(Directory.GetParent(downloadPath).FullName, $"{bookName} - chapter {chapterNumber}.cbz");

            if (Directory.EnumerateFileSystemEntries(folderToArchive).Any() == false)
            {
                WriteDlInfoLine($"=> No images downloaded for {bookName}-{chapterNumber}, CBZ creation will be skipped!\n");
                return;
            }

            if (File.Exists(cbzFilePath) == false)
            {
                try
                {
                    ZipFile.CreateFromDirectory(folderToArchive, cbzFilePath);
                    WriteDlInfoLine($"=> {bookName}-{chapterNumber} .CBZ successfully created!\n");
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
                    WriteDlInfoLine($"=> .CBZ already created!\n");
                }
                else // Replace empty Cbz
                {
                    try
                    {
                        File.Delete(cbzFilePath);
                        ZipFile.CreateFromDirectory(folderToArchive, cbzFilePath);
                        WriteDlInfoLine($"=> {bookName}-{chapterNumber} .CBZ successfully created!\n");
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
        static MessageBoxResult WaitForYesOrNoMsgBox(string textDisplayed)
        {
            WriteDlInfoLine(textDisplayed);
            MessageBoxResult result = MessageBox.Show(textDisplayed, "Continue ?", MessageBoxButton.YesNo, MessageBoxImage.Question);

            WriteDlInfoLine($" YESNO RESULT = {result}");
            return result;
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
            WriteDlInfoLine($"\n{AdaptativeLineOfCharForHeader(appTitle, '$')}");
            WriteDlInfoLine(appTitle);
            WriteDlInfoLine($"{AdaptativeLineOfCharForHeader(appTitle, '$')}\n");
        }

        static string AdaptativeLineOfCharForHeader(string header, char charToUseForLine)
        {
            return new string(charToUseForLine, header.Length);
        }
        #endregion

        #region Quit Console
        public static void QuitApp()
        {
            Environment.Exit(0); // TODO: Weird things happening with Application.Current.Shutdown && Window.Close, the app continue to run anyway even with window closed
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

                    Debug.WriteLine($"\n {htmlFileName} downloaded...");
                }
            }
            Debug.WriteLine($"Html file saved, press to open folder location...");
            MessageBox.Show($"Html file saved, press ok to open folder location...", "Hmtl saved", MessageBoxButton.OK, MessageBoxImage.Information);
            CurrentSettings.OpenOutputDirectoryWhenClosing = false;
            OpenFolder(OutputDirectory);
        }
        #endregion
    }
}
