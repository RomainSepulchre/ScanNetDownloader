using ScanNetDownloader.Logic;
using ScanNetDownloader.Logic.Helpers;
using ScanNetDownloader.View;
using ScanNetDownloader.View.CustomControls;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;

namespace ScanNetDownloader
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    ///

    // TODO: New scan manager button to multi-delete + multi-select/unselect (with shift/ctrl multi selection)
    // TODO: Warn for invalid url as soon as possible (new function chck url validity in ScanData-> url must contains at least a book name)
    // TODO: Manage weird image format from anime-same by cropping image automatically
    // TODO: Scrap a list of all the books available and create a search engine

    public partial class MainWindow : Window
    {

        private TabItem previousTabSelected = null;

        public MainWindow()
        {
            DataContext = this;

            // Handle First launch
            bool firstLaunch = IsFirstLaunch();

            // Load Settings
            Settings.InitializeAppSettings(firstLaunch);

            // Load ScansLocalData
            ScansLocalData.InitializeScansData(firstLaunch);

            App.OnApplicationExitEvent += new EventHandler(OnApplicationExit);

            // Initialize Window
            InitializeComponent();

            titleBar.InitializeTitleBar();

            versionNumber.Content = Assembly.GetExecutingAssembly().GetName().Version.ToString();

#if !DEBUG
            tabDebug.Visibility = Visibility.Collapsed;       
#endif
        }

        private bool IsFirstLaunch()
        {
#if DEBUG
            string dataPath = Constants.DEBUG_DATA_FOLDER_PATH;
#else
            string dataPath = Constants.DATA_FOLDER_PATH;
#endif
            bool firstLaunch = !Directory.Exists(dataPath);
            if (firstLaunch)
            {
                Directory.CreateDirectory(dataPath);
            }
            return firstLaunch;
        }

        #region Ui Routed Events    

        private void tabCtrlNavigation_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ReferenceEquals(e.OriginalSource, tabCtrlNavigation)) // Only if event come from the TabControl and not an element inside
            {
                if (previousTabSelected != null)
                {
                    if (previousTabSelected == tabOptions && optionsVw.OptionsChangesNotSaved)
                    {
                        AskToSaveSettings();
                    }

                    if (tabCtrlNavigation.SelectedItem == tabOptions && previousTabSelected == tabDownload && downloadVw.IsDownloading)
                    {
                        tabCtrlNavigation.SelectedItem = tabDownload;

                        string header = "Download in progress...";
                        string msg = "Please wait for the end of the download or stop the download to access the options tab.";
                        MsgWindow.ShowOkWindow(header, msg, false, MsgWindow.ImageType.Warning);
                    }
                }        

                if (tabCtrlNavigation.SelectedItem == tabScanManager)
                {
                    previousTabSelected = tabScanManager;
                }
                else if (tabCtrlNavigation.SelectedItem == tabDownload)
                {
                    bool refreshUi = true;
                    if (previousTabSelected == tabDownload) refreshUi = false;

                    previousTabSelected = tabDownload;
                    
                    if(refreshUi)
                    {
                        List<ScanItem> scanItemsToDownload = scanManagerVw.ScanListItems.GetScanItemsSelectedForDownload();
                        downloadVw.RefreshDownloadView(scanItemsToDownload);
                    }
                }
                else if (tabCtrlNavigation.SelectedItem == tabOptions)
                {
                    previousTabSelected = tabOptions;
                    optionsVw.RefreshSettings();
                }
                else if (tabCtrlNavigation.SelectedItem == tabDebug)
                {
                    previousTabSelected = tabDebug;
                }
            }
        }
        
        private void scanManagerVw_StartDownload(object sender, RoutedEventArgs e)
        {
            tabCtrlNavigation.SelectedItem = tabDownload; // Switch to download tab
            List<ScanItem> scanItemsToDownload = scanManagerVw.ScanListItems.GetScanItemsSelectedForDownload();
            downloadVw.StartDownloadFromMainView(scanItemsToDownload);
        }

        private void downloadVw_OnDownloadCompleted(object sender, RoutedEventArgs e)
        {
            // Save the scans local data
            ScansLocalData.Save();
        }

        private void optionsVw_RefreshScanData(object sender, RoutedEventArgs e)
        {
            scanManagerVw.ForceScanDataRefresh();
        }

        private void OnApplicationExit(object sender, EventArgs e)
        {
            // Save the scans local data
            ScansLocalData.Save();

            if(tabCtrlNavigation.SelectedItem == tabOptions && optionsVw.OptionsChangesNotSaved)
            {
                AskToSaveSettings();
            }
        }
        #endregion

        private void AskToSaveSettings()
        {
            string mBoxMessage = "Do you want to save your settings changes ?";
            string mBoxCaption = "Save settings ?";
            YesNoWindow yesNoWindow = MsgWindow.ShowYesNoWindow(mBoxCaption, mBoxMessage, true, MsgWindow.ImageType.Question);
            if (yesNoWindow.Success) optionsVw.SaveSettings();
        }

        #region Debug Tab

        private void btnDbgSave_Click(object sender, RoutedEventArgs e)
        {
            // Save the scans local data
            ScansLocalData.Save();
        }

        private void btnDbgOpenDataJson_Click(object sender, RoutedEventArgs e)
        {
            ScansLocalData.OpenJsonFile();
        }

        private void btnDbgOpenSettingsJson_Click(object sender, RoutedEventArgs e)
        {
            Settings.OpenJsonFile();
        }

        private void btnDbg1_Click(object sender, RoutedEventArgs e)
        {
            SaveHtmlFiles();
        }

        private async void btnDbg2_Click(object sender, RoutedEventArgs e)
        {
            //ScanVfNetScanData data = new ScanVfNetScanData("https://www.scan-vf.net/jujutsu-kaisen/chapitre-20/1");
            //HtmlContentResult r = await data.GetUrlHtmlContent();
            //Debug.WriteLine(r.HtmlContent);

            string url1 = "https://www.scan-vf.net/jujutsu-kaisen/chapitre-20/1"; // Host exist but url don't exist (noEx, false)
            string url2 = "https://www.scan-vf.net/jujutsu-kaisen/chapitre-21/1"; // URL -> OK (noEx, true)

            //url = "https://www.frt.grt"; // -> Host doesn't exist (Exception)
            UrlLoadResult r = await ScanData.UrlLoadCorrectlyAsync(url1);
            string exMessage = r.Exception != null ? r.Exception.Message : "NoException";
            Debug.WriteLine($"Success={r.Success}, Ex={exMessage}");

            r = await ScanData.UrlLoadCorrectlyAsync(url2);
            exMessage = r.Exception != null ? r.Exception.Message : "NoException";
            Debug.WriteLine($"Success={r.Success}, Ex={exMessage}");
        }

        private async void btnDbg3_Click(object sender, RoutedEventArgs e)
        {
            string downloadFile = @"D:\Download\ScanNetDownloader\Dev\Test\testImg.png";
            string imgUrl = "https://www.scan-vf.net/uploads/manga/jujutsu-kaisen/chapters/chapitre-21/01.png";

            HttpClient client = HttpClientSingleton.Client;
            try
            {
                Debug.WriteLine($"\nDownloading TEST IMG from {imgUrl}");
                Debug.WriteLine($"...");

                if (File.Exists(downloadFile) == true && File.ReadAllBytes(downloadFile).Length > 0 == true)
                {
                    Debug.WriteLine($"File already downloaded!\n");
                }
                else
                {
                    //await client.DownloadFileTaskAsync(new Uri(imgUrl), downloadFile);
                    byte[] img = await client.GetByteArrayAsync(imgUrl);
                    File.WriteAllBytes(downloadFile, img);
                    Debug.WriteLine($"Sucessfully downloaded!\n");
                }
            }
            catch (HttpRequestException ex)
            {
                Error.FailedImageDownload(ex, imgUrl);
            }
            catch (IOException ex)
            {
                Debug.WriteLine(ex);
            }
        }

        private void btnDbg4_Click(object sender, RoutedEventArgs e)
        {
            //AnimeSamaFrScanData d = new AnimeSamaFrScanData("https://anime-sama.fr/catalogue/the-story-of-a-manga-artist-confined-by-a-strange-high-school-girl/scan/va/");
            //Debug.WriteLine("ENGLISH SCAN LINK - " + d.BookName + " -> " + d.IsUrlForScanInEnglish());
        }

        private void btnDbg5_Click(object sender, RoutedEventArgs e)
        {
            Window parentWindow = this;

            string header = "Test for yes no window";
            string msg = "<Bold>Nothing will happen to the directory</Bold> <Italic>C:\\Users\\aRandomUserName\\IncredibleDirectoryName.</Italic><LineBreak/><LineBreak/>Do accept that nothing will happen to this directory ?";

            YesNoWindow ynWindow = MsgWindow.ShowYesNoWindow(this, header, msg, true, MsgWindow.ImageType.Question, true);

            if (ynWindow.Success)
            {
                Debug.WriteLine("YESNOWINDOW --> YES");
            }
            else
            {
                Debug.WriteLine("YESNOWINDOW --> NO OR CLOSED");
            }
        }

        private void btnDbg6_Click(object sender, RoutedEventArgs e)
        {
            string header = "Ok window";
            string msg = "<Bold>Do you acknowledge something?</Bold> <Italic>It can be anything,</Italic> just acknowledge it!";
            OkWindow okWindow = MsgWindow.ShowOkWindow(this, header, msg, false, MsgWindow.ImageType.Warning, true);

            if (okWindow.Success)
            {
                Debug.WriteLine("OK WINDOW --> YES");
            }
            else
            {
                Debug.WriteLine("OK WINDOW --> NO OR CLOSED");
            }
        }

        void SaveHtmlFiles(List<string> urlList=null)
        {
            List<string> urlToDownload;

            if (urlList == null)
            {
                urlToDownload = new List<string>();

                string header = "Download html file";
                string msg = $"Enter the url from which you want to download HTML";
                string inputPlaceholder = "Enter url here...";

                InputWindow inputWindow = MsgWindow.ShowInputWindow(this, header, msg, inputPlaceholder);
  
                if (inputWindow.Success && string.IsNullOrEmpty(inputWindow.Input) == false)
                {    
                    urlToDownload.Add(inputWindow.Input);
                }
            }
            else
            {
                urlToDownload = urlList;
            }

            if (urlToDownload.Count == 0) return;

            foreach (string urlToDl in urlToDownload)
            {
                // Save htlm code in a file to test
                using (WebClient client = new WebClient())
                {
                    string htmlFileName="";
                    if (urlToDl.StartsWith("http")) htmlFileName = urlToDl.Remove(0, 8); // Remove "https://"
                    htmlFileName = htmlFileName.Replace('/', '_');
                    htmlFileName = htmlFileName + ".html";
                    client.DownloadFile(urlToDl, Path.Combine(Settings.Instance.OutputDirectory, htmlFileName));

                    Debug.WriteLine($"\n {htmlFileName} downloaded...");
                }
            }

            string windowHeader = "Hmtl saved";
            string windowMsg = $"Html file saved, press ok to open folder location...";

            MsgWindow.ShowOkWindow(this, windowHeader, windowMsg, false, MsgWindow.ImageType.Information);

            FileManagement.OpenFolder(Settings.Instance.OutputDirectory);
        }

        private void btnDbg7_Click(object sender, RoutedEventArgs e)
        {
            string fileToOpen = FileManagement.GetUserDownloadsFolder();
            Debug.WriteLine($"DOWNLOAD FOLDER PATH: {fileToOpen}");
            FileManagement.OpenFolder(fileToOpen);
        }

        private void btnDbg8_Click(object sender, RoutedEventArgs e)
        {
            Debug.WriteLine($"AppData path: {Constants.APPDATA_PATH}, exist:{Directory.Exists(Constants.APPDATA_PATH)}");
            Debug.WriteLine($"Data folder path: {Constants.DATA_FOLDER_PATH}, exist:{Directory.Exists(Constants.DATA_FOLDER_PATH)}");
            Debug.WriteLine($"ScanData file path: {Constants.SCANSLOCALDATA_JSON_PATH}, exist:{File.Exists(Constants.SCANSLOCALDATA_JSON_PATH)}");
            Debug.WriteLine($"Settings file path: {Constants.SETTINGS_JSON_PATH}, exist:{File.Exists(Constants.SETTINGS_JSON_PATH)}");
            Debug.WriteLine($"DEV Data folder path: {Constants.DEBUG_DATA_FOLDER_PATH}, exist:{Directory.Exists(Constants.DEBUG_DATA_FOLDER_PATH)}");
            Debug.WriteLine($"DEV ScanData file path: {Constants.DEBUG_SCANSLOCALDATA_JSON_PATH}, exist:{File.Exists(Constants.DEBUG_SCANSLOCALDATA_JSON_PATH)}");
            Debug.WriteLine($"DEV Settings file path: {Constants.DEBUG_SETTINGS_JSON_PATH}, exist:{File.Exists(Constants.DEBUG_SETTINGS_JSON_PATH)}");
        }

        #endregion


    }
}