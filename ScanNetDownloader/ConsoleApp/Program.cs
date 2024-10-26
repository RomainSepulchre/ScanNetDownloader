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

        public static event EventHandler<string> DlInfoWriteLineEvent;

        public static event EventHandler<float> UpdateDlProgressBarEvent;

        public static event EventHandler<ScanWebsiteUrl> ScanDownloadedEvent;

        #region Download Events
        public static void WriteDlInfoLine(string lineToAdd) // TODO: Do better than this ? Class decicated to eventsHandler that anyone can call ? Status/DlInfo class ?
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

        public static async void StartDownloader(List<ScanWebsiteUrl> scansToDownload)
        {
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

            WriteDlInfoLine($"\nThe files will be downloaded in {Settings.Instance.OutputDirectory}, a folder will automatically be created for each title and chapters");
            FileManagement.CheckOutputDirectory();

            bool isNo = WaitForYesOrNoMsgBox("\nDo you to start the download ?") == MessageBoxResult.No;
            if (isNo) return;

            await DownloadScans(scansToDownload);

            if (CurrentSettings.ErrorPauseApp)
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

            if (CurrentSettings.OpenOutputDirectoryAfterDownload)
            {
                FileManagement.OpenRelevantFolder(scansToDownload);                
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
                string downloadPath = FileManagement.CreateChapterDirectory(bookName, chapterNumber);

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
                    CbzCreator.BuildCbzArchive(scanUrl, downloadPath);
                }

                // Deselect since we just downloaded it
                ScanDownloaded(scanUrl);
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
        static string AdaptativeLineOfCharForHeader(string header, char charToUseForLine)
        {
            return new string(charToUseForLine, header.Length);
        }
        #endregion

        #region Quit Console
        public static void QuitApp()
        {
            Environment.Exit(0); // TODO: Weird things happening with Application.Current.Shutdown && Window.Close, the app continue to run anyway even with window closed -> Retest with App.xaml.ShutdownMode="OnMainWindowClose"
        }
        #endregion
    }
}
