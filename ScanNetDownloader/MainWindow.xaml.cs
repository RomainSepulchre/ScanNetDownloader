using ScanNetDownloader.ConsoleApp;
using ScanNetDownloader.View;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using ScanNetDownloader.View.CustomControls;

namespace ScanNetDownloader
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        public List<ScanWebsiteUrl> ScanWebsiteUrls { get; set; }

        private string _dlInfo;

        public string DlInfo
        {
            get { return _dlInfo; }
            set
            {
                _dlInfo = value;
                dlInfoScrollBar?.ScrollToBottom();
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
            Program.DlInfoWriteLineEvent += new EventHandler<string>(WriteDlStatusLine);
            Program.UpdateDlProgressBarEvent += new EventHandler<float>(UpdateDlProgressBar);
            Program.ScanUnselectedForDownloadEvent += new EventHandler<ScanWebsiteUrl>(ScanUnselectedForDownload);

            // Load Settings
            Program.InitializeAppSettings();

            // Initialize Window
            InitializeComponent();         

            // Clear scan list and load list from Settings
            //scanListView.Items.Clear();

            // Load ScanWebsiteUrl saved on system
            ScanWebsiteUrls = Settings.instance.ScanUrlList; // TODO: Separate Scan Data from the settings

            // Populate listView based on the saved data
            RefreshScanListView();
        }

        private void OnPropertyChanged([CallerMemberName]string property=null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(property));
        }

        #region Buttons and Routed Events
        private void btnStart_Click(object sender, RoutedEventArgs e)
        {
            MainTabs.SelectedIndex = 1; // Switch to download tab

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
            bool sameRef = ReferenceEquals(ScanWebsiteUrls, Settings.instance.ScanUrlList);
            Debug.WriteLine($"Is Settings.instance.ScanUrlList same as MainWindow.ScanWebsiteUrls ? => {sameRef}");
            Settings.instance.ScanUrlList = ScanWebsiteUrls;
            Program.SaveSettings(Settings.instance);
        }

        private void btnOpenJSon_Click(object sender, RoutedEventArgs e)
        {
            Program.OpenSettingsJsonFile();
        }

        private void ScanItem_DeleteBtnPressed(object sender, RoutedEventArgs e)
        {
            ScanItem item = sender as ScanItem;
            if (item != null)
            {
                DeleteScanItem(item);
            }
        }

        private void ScanItem_CreateCbzBtnPressed(object sender, RoutedEventArgs e)
        {
            ScanItem item = sender as ScanItem;
            if (item != null)
            {
                Debug.WriteLine($"CBZ CREATION BUTTON: Item={item.BookName}-{item.ChapterId}");
            }
        }

        private void ScanItem_StatusBtnPressed(object sender, RoutedEventArgs e)
        {
            ScanItem item = sender as ScanItem;
            if (item != null)
            {
                bool fileDownloaded = Program.AreScanFilesDownloaded(item.linkedScanWebsiteUrl);
                Debug.WriteLine($"SCAN STATUS, Downloaded ={fileDownloaded}");
                if (fileDownloaded)
                {
                    item.statusBtn.Content = "ok";
                    item.statusBtn.Background = Brushes.Green;
                }
                else
                {
                    item.statusBtn.Content = "∅";
                    item.statusBtn.Background = Brushes.Red;
                }
            }
        }

        #endregion

        #region Events
        public void WriteDlStatusLine(object sender, string lineToAdd)
        {
            DlInfo += $"{lineToAdd}\n";
        }

        public void UpdateDlProgressBar(object sender, float percentageDone)
        {
            // TODO: Add bindings ?
            dlProgressBar.Value = percentageDone;
        }

        public void ScanUnselectedForDownload(object sender, ScanWebsiteUrl unselectedScan)
        {
            // TODO: improve this, is first the best way ?
            ScanItem item = ScanListItems.First(x => x.linkedScanWebsiteUrl == unselectedScan);
            item.IsSelectedForDownload = false;
            Debug.WriteLine($"Debug Update isSelected = {unselectedScan}");
        }
        #endregion

        private void RefreshScanListView()
        {
            ScanListItems.Clear();

            foreach (var scanWebsiteUrl in ScanWebsiteUrls)
            {
                ScanItem item = new ScanItem(scanWebsiteUrl);
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

            // Save the settings // TODO: create a function for this
            Settings.instance.ScanUrlList = ScanWebsiteUrls; // TODO: Is it necessary ScanWebsiteUrls should be a reference of Settings.instance.ScanUrlList = ScanWebsiteUrls
            Program.SaveSettings(Settings.instance); // Seperate scanlist data from settings Data

            // Add item in list view
            foreach (ScanWebsiteUrl scanUrl in newScansToAdd)
            {
                ScanItem item = new ScanItem(scanUrl);
                item.DeleteBtnPressed += ScanItem_DeleteBtnPressed;
                item.CreateCbzBtnPressed += ScanItem_CreateCbzBtnPressed;
                ScanListItems.Add(item);
            }
        }

        private void DeleteScanItem(ScanItem itemToDelete)
        {
            // Remove from saved data
            ScanWebsiteUrls.Remove(itemToDelete.linkedScanWebsiteUrl);

            // Save the settings // TODO: create a function for this
            Settings.instance.ScanUrlList = ScanWebsiteUrls; // TODO: Is it necessary ScanWebsiteUrls should be a reference of Settings.instance.ScanUrlList = ScanWebsiteUrls
            Program.SaveSettings(Settings.instance); // TODO: Seperate scanlist data from settings Data

            // Remove item from list view
            ScanListItems.Remove(itemToDelete); 
        }
    }
}