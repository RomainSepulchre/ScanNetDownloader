using Microsoft.Win32;
using ScanNetDownloader.ConsoleApp;
using ScanNetDownloader.View;
using ScanNetDownloader.View.CustomControls;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;

namespace ScanNetDownloader
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    ///

    // TODO: Delete unselected button
    // TODO: Improve chapter selection visual to give a better understanding of what happening
    // TODO: Warn for invalid url as soon as possible (new function chck url validity in ScanWebsiteUrl-> url must contains at least a book name)
    // TODO: Manage weird image format from anime-same by cropping image automatically
    // TODO: Scrap a list of all the books available and create a search engine

    public partial class MainWindow : Window, INotifyPropertyChanged
    {

        private TabItem previousTabSelected = null;

        private List<ScanWebsiteUrl> ScanWebsiteUrls => ScansLocalData.Instance.ScanUrlList;

        private ObservableCollection<ScanItem> _scanListItems;

        public ObservableCollection<ScanItem> ScanListItems
        {
            get { return _scanListItems; }
            set
            {
                _scanListItems = value;
            }
        }

        private string _dlInfo;

        public string DlInfo
        {
            get { return _dlInfo; }
            set
            {
                _dlInfo = value;
                scrollVwDownloadInfo?.ScrollToBottom();
                OnPropertyChanged();
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        public MainWindow()
        {
            DataContext = this;
            _scanListItems = new ObservableCollection<ScanItem>();

            // Download Events
            Downloader.DlInfoWriteLineEvent += new EventHandler<string>(WriteDlInfoLine);
            Downloader.UpdateDlProgressBarEvent += new EventHandler<float>(UpdateDownloadProgress);
            Downloader.ScanDownloadedEvent += new EventHandler<ScanWebsiteUrl>(ScanDownloaded);

            // Load Settings
            Settings.InitializeAppSettings();

            // Load ScansLocalData
            ScansLocalData.InitializeScansData();

            // Initialize Window
            InitializeComponent();

            // Setup Ui
            RefreshScanListView(); // Populate listView based on the local data
            // Note: Settings are refreshed at OptionsView Initialization
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
                        MessageBoxResult result = MessageBox.Show("Do you want to save your options changes ?", "Save Options", MessageBoxButton.YesNo, MessageBoxImage.Question);
                        if (result == MessageBoxResult.Yes) optionsVw.SaveSettings();
                    }
                }

                if (tabCtrlNavigation.SelectedItem == tabMain)
                {
                    previousTabSelected = tabMain;
                    RefreshScanListView(); 
                }
                else if (tabCtrlNavigation.SelectedItem == tabDownload)
                {
                    previousTabSelected = tabDownload;
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

            List<ScanWebsiteUrl> scansToDownload = new List<ScanWebsiteUrl>();
            foreach (ScanItem item in _scanListItems)
            {
                if (item.IsSelectedForDownload) scansToDownload.Add(item.linkedScanWebsiteUrl);
            }

            DlInfo = ""; // Clear download info
            Downloader.StartDownloader(scansToDownload);
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
                string chapterInput = addWindow.ChapterInput;

                List<ScanWebsiteUrl> newScansToAdd = ScanManagement.CreateNewScanWebsiteUrls(urlInput, chapterInput);                

                if (newScansToAdd.Any())
                {
                    AddScanItems(newScansToAdd);
                }
                else
                {
                    MessageBox.Show($"{urlInput} not added, it was not a valid url", "Invalid URL", MessageBoxButton.OK, MessageBoxImage.Warning);
                }             
            }
        }

        private void btnDbgSave_Click(object sender, RoutedEventArgs e)
        {
            // Save the scans local data
            ScansLocalData.Update(ScanWebsiteUrls);
        }

        private void btnDbgOpenDataJson_Click(object sender, RoutedEventArgs e)
        {
            ScansLocalData.OpenJsonFile();
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
                bool cbzCreated = FileManagement.IsCbzArchiveCreated(item.linkedScanWebsiteUrl);
                item.CbzArchiveCreated = cbzCreated;
                Debug.WriteLine($"CBZ CREATION BUTTON: Item={item.BookName}-{item.ChapterId}");

                if (cbzCreated == false)
                {
                    // TODO: Propose to build cbz
                    MessageBoxResult result = MessageBox.Show($"Do you want to create a .cbz for {item.BookName} - Chapter {item.ChapterId} ?", "CBZ Archive creation", MessageBoxButton.YesNo, MessageBoxImage.Question);
                    if (result == MessageBoxResult.Yes)
                    {
                        string chapterPath = FileManagement.GetChapterDirectoryPath(item.linkedScanWebsiteUrl);
                        CbzCreator.BuildCbzArchive(item.linkedScanWebsiteUrl, chapterPath);

                        bool cbzSuccessfullyCreated = FileManagement.IsCbzArchiveCreated(item.linkedScanWebsiteUrl);
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
                bool fileDownloaded = FileManagement.AreScanFilesDownloaded(item.linkedScanWebsiteUrl);
                item.IsDownloaded = fileDownloaded;
            }
        }

        #endregion

        #region Events Handler
        public void WriteDlInfoLine(object sender, string lineToAdd)
        {
            DlInfo += $"{lineToAdd}\n";
        }

        public void UpdateDownloadProgress(object sender, float percentageDone)
        {
            // TODO: Add bindings ?
            progrBarDownload.Value = percentageDone;
        }

        public void ScanDownloaded(object sender, ScanWebsiteUrl downloadedScan) // This happens when the 
        {
            // TODO: What's best way to retrieve item ? .First() ? using index in scanUrlList ?
            //ScanItem item = ScanListItems.First(x => x.linkedScanWebsiteUrl == unselectedScan);
            ScanItem item = ScanListItems[ScanWebsiteUrls.IndexOf(downloadedScan)];
                       
            item.IsSelectedForDownload = false; // Disable download selection since we just downloaded

            bool fileSuccessfullyDownloaded = FileManagement.AreScanFilesDownloaded(downloadedScan);
            item.IsDownloaded = fileSuccessfullyDownloaded; 

            // TODO: Check if Cbz has been created

            // TODO: When do I save changes ? When Download is finished ?
        }
        #endregion

        private void RefreshScanListView()
        {
            Debug.WriteLine("REFRESH SCAN LIST");
            ScanListItems.Clear();

            foreach (var scanUrl in ScanWebsiteUrls)
            {
                bool filesAlreadyDownloaded = FileManagement.AreScanFilesDownloaded(scanUrl);
                bool cbzAlreadyCreated = FileManagement.IsCbzArchiveCreated(scanUrl);

                ScanItem item = new ScanItem(scanUrl, filesAlreadyDownloaded, cbzAlreadyCreated);
                item.DeleteBtnPressed += ScanItem_DeleteBtnPressed;
                item.CreateCbzBtnPressed += ScanItem_CreateCbzBtnPressed;
                item.StatusBtnPressed += ScanItem_StatusBtnPressed;
                ScanListItems.Add(item);
            }
        }

        private void AddScanItems(List<ScanWebsiteUrl> newScansToAdd) // TODO: Replace the refresh by a add function to prevent recreating the whole view everytime
        {
            // Add in saved data
            ScanWebsiteUrls.AddRange(newScansToAdd);

            // Save the scans local data
            ScansLocalData.Update(ScanWebsiteUrls);

            // Add item in list view
            foreach (ScanWebsiteUrl scanUrl in newScansToAdd)
            {
                // TODO: Add a check to prevent a double entry of the same chapter on the same website, maybe check before caliing AddScanItems ?
                bool filesAlreadyDownloaded = FileManagement.AreScanFilesDownloaded(scanUrl); 
                bool cbzAlreadyCreated = FileManagement.IsCbzArchiveCreated(scanUrl);

                ScanItem item = new ScanItem(scanUrl, filesAlreadyDownloaded, cbzAlreadyCreated);
                item.DeleteBtnPressed += ScanItem_DeleteBtnPressed;
                item.CreateCbzBtnPressed += ScanItem_CreateCbzBtnPressed;               
                ScanListItems.Add(item);
            }
        }

        private void DeleteScanItem(ScanItem itemToDelete)
        {
            // Remove from saved data
            ScanWebsiteUrls.Remove(itemToDelete.linkedScanWebsiteUrl);

            // Save the scans local data
            ScansLocalData.Update(ScanWebsiteUrls);

            // Remove item from list view
            ScanListItems.Remove(itemToDelete);
        }

        #region Debug Tab 
        private void btnDbg1_Click(object sender, RoutedEventArgs e)
        {
            SaveHtmlFiles();
        }

        private void btnDbg2_Click(object sender, RoutedEventArgs e)
        {
            
        }

        private void btnDbg3_Click(object sender, RoutedEventArgs e)
        {

        }

        private void btnDbg4_Click(object sender, RoutedEventArgs e)
        {

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
            Debug.WriteLine($"Html file saved, press to open folder location...");
            MessageBox.Show($"Html file saved, press ok to open folder location...", "Hmtl saved", MessageBoxButton.OK, MessageBoxImage.Information);
            Settings.Instance.OpenOutputDirectoryAfterDownload = false;
            FileManagement.OpenFolder(Settings.Instance.OutputDirectory);
        }
        #endregion
    }
}