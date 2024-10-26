using Microsoft.Win32;
using ScanNetDownloader.ConsoleApp;
using ScanNetDownloader.View;
using ScanNetDownloader.View.CustomControls;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
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

    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        private List<ScanWebsiteUrl> ScanWebsiteUrls => ScansLocalData.Instance.ScanUrlList;

        private bool OptionsChanged => OptionsModifiedButNotSaved(); // TODO: Bind btnSave.IsEnable to this but hoe to do it with settings anything ?

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
            _scanListItems = new ObservableCollection<ScanItem>();

            // Events
            Program.DlInfoWriteLineEvent += new EventHandler<string>(WriteDlInfoLine);
            Program.UpdateDlProgressBarEvent += new EventHandler<float>(UpdateDownloadProgress);
            Program.ScanDownloadedEvent += new EventHandler<ScanWebsiteUrl>(ScanDownloaded);

            // Load Settings
            Settings.InitializeAppSettings();

            // Load ScansLocalData
            ScansLocalData.InitializeScansData();

            // Initialize Window
            InitializeComponent();         

            // Clear scan list and load list from Settings
            //scanListView.Items.Clear();

            // Populate listView based on the local data
            RefreshScanListView();
            RefreshSettings();
        }

        private void OnPropertyChanged([CallerMemberName]string property=null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(property));
        }

        #region Ui Routed Events
        
        // TODO: Is there a way to know when we leave options tab
        //private void tabOptions_LostFocus(object sender, RoutedEventArgs e)
        //{
        //    if (OptionsChanged)
        //    {
        //        MessageBoxResult result = MessageBox.Show("Do you want to save your options changes ?", "Save Options", MessageBoxButton.YesNo, MessageBoxImage.Question);
        //        if (result == MessageBoxResult.Yes) SaveSettings();
        //    }
        //}

        private void tabCtrlNavigation_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ReferenceEquals(e.OriginalSource, tabCtrlNavigation))
            {
                if (tabCtrlNavigation.SelectedItem == tabMain)
                {
                    RefreshScanListView();
                }
                else if (tabCtrlNavigation.SelectedItem == tabDownload)
                {

                }
                else if (tabCtrlNavigation.SelectedItem == tabOptions)
                {
                    RefreshSettings();
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

            Program.StartDownloader(scansToDownload, this); // Run console App program
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

                List<ScanWebsiteUrl> newScansToAdd = Program.CreateNewScanWebsiteUrls(urlInput, chapterInput);                

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

        private void btnChooseOutputDir_Click(object sender, RoutedEventArgs e)
        {
            OpenFolderDialog fileDialog = new OpenFolderDialog();
            fileDialog.Title = "Select download directory";
            fileDialog.Multiselect = false;

            bool? success = fileDialog.ShowDialog();

            if (success == true)
            {
                txtBoxOutputDir.Text = fileDialog.FolderName;
                SaveSettings();
            }
        }

        private void btnSaveOptions_Click(object sender, RoutedEventArgs e)
        {
            SaveSettings();
        }

        private void btnDbgOpenSettingsJson_Click(object sender, RoutedEventArgs e)
        {
            Settings.OpenJsonFile();
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
                bool cbzCreated = Program.IsCbzArchiveCreated(item.linkedScanWebsiteUrl);
                item.CbzArchiveCreated = cbzCreated;
                Debug.WriteLine($"CBZ CREATION BUTTON: Item={item.BookName}-{item.ChapterId}");

                if (cbzCreated == false)
                {
                    // TODO: Propose to build cbz
                    MessageBoxResult result = MessageBox.Show($"Do you want to create a .cbz for {item.BookName} - Chapter {item.ChapterId} ?", "CBZ Archive creation", MessageBoxButton.YesNo, MessageBoxImage.Question);
                    if (result == MessageBoxResult.Yes)
                    {
                        string chapterPath = Program.GetChapterDirectoryPath(item.linkedScanWebsiteUrl);
                        Program.BuildCbzArchive(item.linkedScanWebsiteUrl, chapterPath);

                        bool cbzSuccessfullyCreated = Program.IsCbzArchiveCreated(item.linkedScanWebsiteUrl);
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
                bool fileDownloaded = Program.AreScanFilesDownloaded(item.linkedScanWebsiteUrl);
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

            bool fileSuccessfullyDownloaded = Program.AreScanFilesDownloaded(downloadedScan);
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
                bool filesAlreadyDownloaded = Program.AreScanFilesDownloaded(scanUrl);
                bool cbzAlreadyCreated = Program.IsCbzArchiveCreated(scanUrl);

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
                bool filesAlreadyDownloaded = Program.AreScanFilesDownloaded(scanUrl); 
                bool cbzAlreadyCreated = Program.IsCbzArchiveCreated(scanUrl);

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

        private void RefreshSettings()
        {
            Debug.WriteLine("REFRESH SETTINGS");

            Settings settings = Settings.Instance;
            // TODO: Create bindings for ui elements
            txtBoxOutputDir.Text = settings.OutputDirectory;
            chBoxCreateCbz.IsChecked = settings.CreateCbzArchive;
            chBoxDeleteImageAfterCbz.IsChecked = settings.DeleteImagesAfterCbzCreation;
            chBoxOpenOutputDir.IsChecked = settings.OpenOutputDirectoryWhenClosing;
            chBoxErrorPauseApp.IsChecked = settings.ErrorsPauseApp;
        }

        private void SaveSettings()
        {
            // TODO: check if output directory is a valid directory before saving

            Settings.Instance.OutputDirectory = txtBoxOutputDir.Text;
            Settings.Instance.CreateCbzArchive = chBoxCreateCbz.IsChecked ?? Settings.Instance.CreateCbzArchive;
            Settings.Instance.DeleteImagesAfterCbzCreation = chBoxDeleteImageAfterCbz.IsChecked ?? Settings.Instance.DeleteImagesAfterCbzCreation;
            Settings.Instance.OpenOutputDirectoryWhenClosing = chBoxOpenOutputDir.IsChecked ?? Settings.Instance.OpenOutputDirectoryWhenClosing;
            Settings.Instance.ErrorsPauseApp = chBoxErrorPauseApp.IsChecked ?? Settings.Instance.ErrorsPauseApp;

            Settings.Update(Settings.Instance);
        }

        private bool OptionsModifiedButNotSaved()
        {
            Settings settings = Settings.Instance;
            if (txtBoxOutputDir.Text != settings.OutputDirectory) return true;
            if (chBoxCreateCbz.IsChecked != settings.CreateCbzArchive) return true;
            if (chBoxDeleteImageAfterCbz.IsChecked != settings.DeleteImagesAfterCbzCreation) return true;
            if (chBoxOpenOutputDir.IsChecked != settings.OpenOutputDirectoryWhenClosing) return true;
            if (chBoxErrorPauseApp.IsChecked != settings.ErrorsPauseApp) return true;

            return false;
        }
    }
}