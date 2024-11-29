using Microsoft.Win32;
using ScanNetDownloader.Logic;
using ScanNetDownloader.View;
using ScanNetDownloader.View.CustomControls;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Security.Policy;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using static System.Net.Mime.MediaTypeNames;

namespace ScanNetDownloader
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    ///

    // TODO: Delete unselected button
    // TODO: Improve chapter selection visual to give a better understanding of what happening
    // TODO: Warn for invalid url as soon as possible (new function chck url validity in ScanData-> url must contains at least a book name)
    // TODO: Manage weird image format from anime-same by cropping image automatically
    // TODO: Scrap a list of all the books available and create a search engine

    public partial class MainWindow : Window, INotifyPropertyChanged
    {

        private TabItem previousTabSelected = null;

        private List<ScanData> ScanDatas => ScansLocalData.Instance.ScanDataList;

        private ObservableCollection<ScanItem> _scanListItems;

        public ObservableCollection<ScanItem> ScanListItems
        {
            get { return _scanListItems; }
            set
            {
                _scanListItems = value;
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        public MainWindow()
        {
            DataContext = this;
            ScanListItems = new ObservableCollection<ScanItem>();
            
            // Load Settings
            Settings.InitializeAppSettings();

            // Load ScansLocalData
            ScansLocalData.InitializeScansData();

            // Initialize Window
            InitializeComponent();

            // Setup Ui
            RefreshScanListView(); // Populate listView based on the local data
                                   // Note: Settings are refreshed at OptionsView Initialization

#if !DEBUG
            tabDebug.Visibility = Visibility.Collapsed;
            gridStatusBar.Visibility = Visibility.Collapsed;
            gridMainContent.RowDefinitions[1].Height = new GridLength(0);
            gridMainContent.RowDefinitions[2].Height = new GridLength(0);
#endif

        }

        private void OnPropertyChanged([CallerMemberName]string property=null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(property));
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
                        string mBoxMessage = "Do you want to save your options changes ?";
                        string mBoxCaption = "Save Options";
                        MessageBoxResult result = MessageBox.Show(mBoxMessage, mBoxCaption, MessageBoxButton.YesNo, MessageBoxImage.Question);
                        if (result == MessageBoxResult.Yes) optionsVw.SaveSettings();
                    }
                }

                if (tabCtrlNavigation.SelectedItem == tabMain)
                {
                    previousTabSelected = tabMain;
                }
                else if (tabCtrlNavigation.SelectedItem == tabDownload)
                {
                    previousTabSelected = tabDownload;

                    List<ScanItem> scanItemsToDownload = ScanListItems.GetScanItemsSelectedForDownload();
                    downloadVw.RefreshDownloadView(scanItemsToDownload);
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

        private void btnStart_Click(object sender, RoutedEventArgs e)
        {
            tabCtrlNavigation.SelectedItem = tabDownload; // Switch to download tab

            List<ScanItem> scanItemsToDownload = ScanListItems.GetScanItemsSelectedForDownload();
            downloadVw.StartDownload(scanItemsToDownload);
        }

        private void btnAdd_Click(object sender, RoutedEventArgs e)
        {
            AddScanWindow addWindow = new AddScanWindow(this);
            Opacity = 0.4;
            addWindow.ShowDialog();
            Opacity = 1;

            if(addWindow.Success) // TODO: Clean this
            {
                string urlInput = addWindow.UrlInput;
                List<ScanData> newScansToAdd = addWindow.NewScanDatas;                

                if (newScansToAdd == null || newScansToAdd.Count == 0)
                {
                    string mBoxMessage = $"No scan data to add for {urlInput}, make sure you used a valid url";
                    string mBoxCaption = "No scan data";
                    MessageBox.Show(mBoxMessage, mBoxCaption, MessageBoxButton.OK, MessageBoxImage.Warning);
                }
                else
                {
                    AddScanItems(newScansToAdd);
                }             
            }
        }

        private void btnOpenStatusBar_Click(object sender, RoutedEventArgs e)
        {
            // TODO: Open a scrollable list view that allow to see all status
        }

        private void ScanItem_DeleteBtnPressed(object sender, RoutedEventArgs e)
        {
            ScanItem item = e.Source as ScanItem;
            if (item != null)
            {
                DeleteScanItem(item);
            }
        }

        private void ScanItem_CreateCbzBtnPressed(object sender, RoutedEventArgs e)
        {
            ScanItem item = e.Source as ScanItem;
            if (item != null)
            {
                // Verify if cbz is created
                bool cbzCreated = FileManagement.IsCbzArchiveCreated(item.linkedScanData);
                item.CbzArchiveCreated = cbzCreated;
                Debug.WriteLine($"CBZ CREATION BUTTON: Item={item.BookName}-{item.ChapterId}");

                if (cbzCreated == false)
                {
                    // TODO: Propose to build cbz
                    string mBoxMessage = $"Do you want to create a .cbz for {item.BookName} - Chapter {item.ChapterId} ?";
                    string mBoxCaption = "CBZ Archive creation";
                    MessageBoxResult result = MessageBox.Show(mBoxMessage, mBoxCaption, MessageBoxButton.YesNo, MessageBoxImage.Question);
                    if (result == MessageBoxResult.Yes)
                    {
                        string chapterPath = FileManagement.GetChapterDirectoryPath(item.linkedScanData);
                        CbzCreator.BuildCbzArchive(item.linkedScanData, chapterPath);

                        bool cbzSuccessfullyCreated = FileManagement.IsCbzArchiveCreated(item.linkedScanData);
                        item.CbzArchiveCreated = cbzSuccessfullyCreated;
                    }
                }
            }
        }

        private void ScanItem_StatusBtnPressed(object sender, RoutedEventArgs e)
        {
            ScanItem item = e.Source as ScanItem;
            if (item != null)
            {
                bool fileDownloaded = FileManagement.AreScanFilesDownloaded(item.linkedScanData);
                item.IsDownloaded = fileDownloaded;
            }
        }

        #endregion

        private void RefreshScanListView()
        {
            // TODO: Why is it so long with a lot of items ? Way to optiomize this ?
            Debug.WriteLine("REFRESH SCAN LIST");
            ScanListItems.Clear();

            foreach (ScanData scanData in ScanDatas)
            {
                bool filesAlreadyDownloaded = FileManagement.AreScanFilesDownloaded(scanData);
                bool cbzAlreadyCreated = FileManagement.IsCbzArchiveCreated(scanData);

                ScanItem item = new ScanItem(scanData, filesAlreadyDownloaded, cbzAlreadyCreated);
                item.DeleteScanBtnPressed += ScanItem_DeleteBtnPressed;
                item.CreateCbzBtnPressed += ScanItem_CreateCbzBtnPressed;
                item.StatusBtnPressed += ScanItem_StatusBtnPressed;
                ScanListItems.Add(item);
            }
        }

        private void AddScanItems(List<ScanData> newScansToAdd) // TODO: Replace the refresh by a add function to prevent recreating the whole view everytime
        {

            // TODO: Check for duplicated ScanData (Same BookName, chapter and url)

            // Add in saved data
            ScanDatas.AddRange(newScansToAdd);

            // Save the scans local data
            ScansLocalData.Update(ScanDatas);

            // Add item in list view
            foreach (ScanData scanData in newScansToAdd)
            {
                // TODO: Add a check to prevent a double entry of the same chapter on the same website, maybe check before caliing AddScanItems ?
                bool filesAlreadyDownloaded = FileManagement.AreScanFilesDownloaded(scanData); 
                bool cbzAlreadyCreated = FileManagement.IsCbzArchiveCreated(scanData);

                ScanItem item = new ScanItem(scanData, filesAlreadyDownloaded, cbzAlreadyCreated);
                item.DeleteScanBtnPressed += ScanItem_DeleteBtnPressed;
                item.CreateCbzBtnPressed += ScanItem_CreateCbzBtnPressed;               
                ScanListItems.Add(item);
            }
        }

        private void DeleteScanItem(ScanItem itemToDelete)
        {
            // Remove from saved data
            ScanDatas.Remove(itemToDelete.linkedScanData);

            // Save the scans local data
            ScansLocalData.Update(ScanDatas);

            // Remove item from list view
            ScanListItems.Remove(itemToDelete);
        }

        #region Debug Tab

        private void btnDbgSave_Click(object sender, RoutedEventArgs e)
        {
            // Save the scans local data
            ScansLocalData.Update(ScanDatas);
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
                //TODO : Manage IO Eception
            }
        }

        private void btnDbg4_Click(object sender, RoutedEventArgs e)
        {
            //AnimeSamaFrScanData d = new AnimeSamaFrScanData("https://anime-sama.fr/catalogue/the-story-of-a-manga-artist-confined-by-a-strange-high-school-girl/scan/va/");
            //Debug.WriteLine("ENGLISH SCAN LINK - " + d.BookName + " -> " + d.IsUrlForScanInEnglish());
        }

        private void btnDbg5_Click(object sender, RoutedEventArgs e)
        {

        }

        private void btnDbg6_Click(object sender, RoutedEventArgs e)
        {

        }

        void SaveHtmlFiles(List<string> urlList=null)
        {
            List<string> urlToDownload;

            if (urlList == null)
            {
                InputPopUp inputPopUp = new InputPopUp(this, $"Enter the url to download HTML from:");
                Opacity = 0.4;
                inputPopUp.ShowDialog();
                Opacity = 1;

                urlToDownload = new List<string>();
                urlToDownload.Add(inputPopUp.Input);
            }
            else
            {
                urlToDownload = urlList;
            }

            foreach (string urlToDl in urlToDownload)
            {
                // Save htlm code in a file to test
                using (WebClient client = new WebClient())
                {
                    string htmlFileName = urlToDl.Remove(0, 8); // Remove "https://"
                    htmlFileName = htmlFileName.Replace('/', '_');
                    htmlFileName = htmlFileName + ".html";
                    client.DownloadFile(urlToDl, Path.Combine(Settings.Instance.OutputDirectory, htmlFileName));

                    Debug.WriteLine($"\n {htmlFileName} downloaded...");
                }
            }

            string mBoxCaption = "Hmtl saved";
            string mBoxMessage = $"Html file saved, press ok to open folder location...";
            Debug.WriteLine(mBoxMessage);
            MessageBox.Show(mBoxMessage, mBoxCaption, MessageBoxButton.OK, MessageBoxImage.Information);
            Settings.Instance.OpenOutputDirectoryAfterDownload = false;
            FileManagement.OpenFolder(Settings.Instance.OutputDirectory);
        }
        #endregion
    }
}