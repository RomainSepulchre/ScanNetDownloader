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
using System.Runtime.CompilerServices;
using System.Security.Policy;
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
            Downloader.ScanDownloadedEvent += new EventHandler<ScanData>(ScanDownloaded);

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
                        string mBoxMessage = "Do you want to save your options changes ?";
                        string mBoxCaption = "Save Options";
                        MessageBoxResult result = MessageBox.Show(mBoxMessage, mBoxCaption, MessageBoxButton.YesNo, MessageBoxImage.Question);
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

            List<ScanData> scansToDownload = new List<ScanData>();
            foreach (ScanItem item in ScanListItems)
            {
                if (item.IsSelectedForDownload) scansToDownload.Add(item.linkedScanData);
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

        private void btnDbgSave_Click(object sender, RoutedEventArgs e)
        {
            // Save the scans local data
            ScansLocalData.Update(ScanDatas);
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

        public void ScanDownloaded(object sender, ScanData downloadedScan) // This happens when the 
        {
            // TODO: What's best way to retrieve item ? .First() ? using index in scanDataList ?
            //ScanItem item = ScanListItems.First(x => x.linkedScanData == downloadedScan);
            ScanItem item = ScanListItems[ScanDatas.IndexOf(downloadedScan)];
                       
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

            foreach (ScanData scanData in ScanDatas)
            {
                bool filesAlreadyDownloaded = FileManagement.AreScanFilesDownloaded(scanData);
                bool cbzAlreadyCreated = FileManagement.IsCbzArchiveCreated(scanData);

                ScanItem item = new ScanItem(scanData, filesAlreadyDownloaded, cbzAlreadyCreated);
                item.DeleteBtnPressed += ScanItem_DeleteBtnPressed;
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
                item.DeleteBtnPressed += ScanItem_DeleteBtnPressed;
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