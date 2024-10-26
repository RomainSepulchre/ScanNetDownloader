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

        private static Settings CurrentSettings => Settings.Instance;

        private static string OutputDirectory => Settings.Instance.OutputDirectory;

        private static Window mainWindow = new Window();

        public static event EventHandler<string> DlInfoWriteLineEvent;

        public static event EventHandler<float> UpdateDlProgressBarEvent;

        public static event EventHandler<ScanWebsiteUrl> ScanDownloadedEvent;

        #region Download Events
        public static void WriteDlInfoLine(string lineToAdd)
        {
            Debug.WriteLine(lineToAdd);
            if (DlInfoWriteLineEvent != null)
            {
                DlInfoWriteLineEvent(null, lineToAdd); 
            }
        }

        public static void UpdateDownloadProgress(float percentageDone)
        {
            if (UpdateDlProgressBarEvent != null)
            {
                UpdateDlProgressBarEvent(null, percentageDone);
            }
        }

        public static void ScanDownloaded(ScanWebsiteUrl unselectedScan)
        {
            if (ScanDownloadedEvent != null)
            {
                ScanDownloadedEvent(null, unselectedScan);
            }
        }
        #endregion


        // TODO: Turn this in a Downloader class
        #region Downloader

        public static async void StartDownloader(List<ScanWebsiteUrl> scansToDownload, Window _mainWindow)
        {
            mainWindow = _mainWindow;

            if(scansToDownload.Count == 0)
            {
                WriteDlInfoLine($"No scans have been selected, select at least a scan to start the download");
                return;
            }

            WriteDlInfoLine($"\nHere is the list of scans you are going to download:");
            foreach (ScanWebsiteUrl item in scansToDownload)
            {
                WriteDlInfoLine($"-> {item.BookName} - {item.ChapterId} (source:{item.Url})");
            }

            WriteDlInfoLine($"\nThe files will be downloaded in {OutputDirectory}, a folder will automatically be created for each title and chapters");
            CheckOutputDirectory();

            bool isNo = WaitForYesOrNoMsgBox("\nDo you to start the download ?") == MessageBoxResult.No;
            if (isNo) return;


            // Main menu (definitive download list)
            WriteAppTitle();

            await DownloadScans(scansToDownload);

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
                OpenRelevantFolder(scansToDownload);                
            }
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
                List<string> imgsToDownload = await scanUrl.GetScanImagesUrl();
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

                // Deselect since we just downloaded it
                ScanDownloaded(scanUrl);
            }
        }
        #endregion

        // TODO: Turn this into a Scan Managament Class
        #region Scan Website Url Management
        public static List<ScanWebsiteUrl> CreateNewScanWebsiteUrls(string urlEntered, string chaptersEntered)
        {
            bool errorOccured = false;
            List<ScanWebsiteUrl> newScanWebsiteUrls = new List<ScanWebsiteUrl>();
            List<int> selectedChaptersId;

            Debug.WriteLine($"\nURL ==> {urlEntered}\n");
            switch (urlEntered)
            {
                case string s when s.Contains(Constants.SCANVF_DOMAIN_NAME):
                    //https://www.scan-vf.net/jujutsu-kaisen/chapitre-164/1 = url with chapter -> at least 5 split
                    //https://www.scan-vf.net/jujutsu-kaisen = url without chapter -> less than 5 split
                    bool chapterIsInUrl = urlEntered.Split(Constants.SLASH_CHAR).Count() > 4; // Check if the url has a chapter name (the number of split let us know if url stop at book name or not)
                    if (chapterIsInUrl)
                    {
                        ScanWebsiteUrl scanVfNetUrl = new ScanVfNetUrl(urlEntered);
                        newScanWebsiteUrls.Add(scanVfNetUrl);
                        Debug.WriteLine($"{scanVfNetUrl.BookName} - Chapter {scanVfNetUrl.ChapterId} added ({scanVfNetUrl.WebsiteDomain}).\n");
                    }
                    else
                    {
                        // Chapters to download
                        ScanWebsiteUrl temporaryScanVfNetUrl = new ScanVfNetUrl(urlEntered, false);
                        selectedChaptersId = ChapterSelection(temporaryScanVfNetUrl, chaptersEntered, ref errorOccured);
                        // Create link
                        foreach (int chapterId in selectedChaptersId)
                        {
                            string urlWithChapter = $"{urlEntered}{Constants.SCANVF_CHAPTER_IN_URL}{chapterId}"; // No need to specify "/1" after chapter number redirection is done by website
                            ScanWebsiteUrl scanVfNetUrl = new ScanVfNetUrl(urlWithChapter);
                            newScanWebsiteUrls.Add(scanVfNetUrl);
                            // TODO: we never check if chapter exist with ScanVf ? Check this before creating ScanWebsiteUrl
                            Debug.WriteLine($" -> {scanVfNetUrl.BookName} - Chapter {scanVfNetUrl.ChapterId} added ({scanVfNetUrl.WebsiteDomain}).");
                        }
                        Debug.WriteLine("");
                    }
                    break;

                case string s when s.Contains(Constants.ANIMESAMA_DOMAIN_NAME):
                    ScanWebsiteUrl temporaryAnimeSamaUrl = new AnimeSamaFrUrl(urlEntered); // Temporary obj to get book name

                    // TODO: If possible manage Book URL instead of chapter url
                    // -> check if chapter url is stable or if it changes too much

                    // Get chapters to download
                    selectedChaptersId = ChapterSelection(temporaryAnimeSamaUrl, chaptersEntered, ref errorOccured);

                    // Create link
                    foreach (int chapterId in selectedChaptersId)
                    {
                        ScanWebsiteUrl animeSamaUrl = new AnimeSamaFrUrl(urlEntered, chapterId);
                        newScanWebsiteUrls.Add(animeSamaUrl);
                        Debug.WriteLine($" -> {animeSamaUrl.BookName} - Chapter {animeSamaUrl.ChapterId} added ({animeSamaUrl.WebsiteDomain}).");
                    }
                    Debug.WriteLine("");
                    break;

                default: // Default, unknown domain name
                    errorOccured = true;
                    Error.UnknownScanWebDomain(urlEntered);
                    break;
            }

            if (errorOccured)
            {
                Debug.WriteLine($"Make sure to check the errors and press any key to continue...");
                MessageBox.Show("Make sure to check the errors and press any key to continue...", "Check errrors", MessageBoxButton.OK, MessageBoxImage.Warning);
            }

            return newScanWebsiteUrls;
        }

        static List<int> ChapterSelection(ScanWebsiteUrl scanUrl, string enteredChapters, ref bool errorOccured)
        {
            List<int> selectedChaptersId = new List<int>();
            
            if (string.IsNullOrEmpty(enteredChapters) == false)
            {
                selectedChaptersId = ParseToFindChapters(scanUrl, enteredChapters, ref errorOccured);
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

        #endregion

        // TODO: Move to Downloader or create a class for File Management
        #region Folder Management
        static void CheckOutputDirectory()
        {
            // TODO: Redo Error Manamgement to fit with WPF
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
                    // TODO: Select output dir here instead of opening Json
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
                    
                    Settings.OpenJsonFile();
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

        static void OpenRelevantFolder(List<ScanWebsiteUrl> scansToDownload)
        {
            if (scansToDownload.Count == 1) 
            {
                // One chapter downloaded, open this chapter folder
                string chapterDirectory = GetChapterDirectoryPath(scansToDownload[0]);
                OpenFolder(chapterDirectory);
            }
            else
            {
                bool moreThanOneBook = MoreThanOneBookInUrlList(scansToDownload);
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

        public static bool AreScanFilesDownloaded(ScanWebsiteUrl scanUrl)
        {
            string chapterDirPath = GetChapterDirectoryPath(scanUrl);
            if (Directory.Exists(chapterDirPath))
            {
                if(Directory.GetFiles(chapterDirPath).Any()) // TODO: Improve this to know if we have the correct amount of page, need more info in ScanWebsiteUrl
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

        // TODO: Move this in a CbzCreator Class
        #region Cbz Archive
        public static void BuildCbzArchive(ScanWebsiteUrl scanUrl, string downloadPath)
        {
            WriteDlInfoLine($"=> Create .CBZ for {scanUrl.BookName}-{scanUrl.ChapterId}...");

            string bookName = scanUrl.BookName;
            string chapterNumber = scanUrl.ChapterId.ToString();

            string folderToArchive = downloadPath;
            string cbzFilePath = Path.Combine(Directory.GetParent(downloadPath).FullName, $"{bookName}{Constants.CBZ_CHAPTER_PREFIX}{chapterNumber}{Constants.CBZ_EXTENSION}"); // TODO: Add const for { - chapter } and .cbz

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
        public static MessageBoxResult WaitForYesOrNoMsgBox(string textDisplayed)
        {
            WriteDlInfoLine(textDisplayed);
            MessageBoxResult result = MessageBox.Show(textDisplayed, "Continue ?", MessageBoxButton.YesNo, MessageBoxImage.Question);

            WriteDlInfoLine($" YESNO RESULT = {result}");
            return result;
        }

        //
        // TODO : Probably won't be needed but make sure before cleaning
        //
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
