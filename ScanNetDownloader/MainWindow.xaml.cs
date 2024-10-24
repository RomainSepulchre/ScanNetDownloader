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
                dlInfoScrollBar.ScrollToBottom();
                OnPropertyChanged();
            }
        }

        private ObservableCollection<TextBlock> _scanListItems;

        public ObservableCollection<TextBlock> ScanListItems
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
            _scanListItems = new ObservableCollection<TextBlock>();

            // Events
            Program.DlInfoWriteLine += new EventHandler<string>(WriteDlStatusLine);
            Program.UpdatDlProgressBar += new EventHandler<float>(UpdateDlProgressBar);

            // Load Settings
            Program.InitializeAppSettings();

            // Initialize Window
            InitializeComponent();         

            // Clear scan list and load list from Settings
            //scanListView.Items.Clear();

            // Load ScanWebsiteUrl saved on system
            ScanWebsiteUrls = Program.LoadSavedScanWebsiteUrl();

            // Populate listView based on the saved data
            RefreshScanListView();
        }

        private void OnPropertyChanged([CallerMemberName]string property=null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(property));
        }

        #region Buttons
        private void btnStart_Click(object sender, RoutedEventArgs e)
        {
            MainTabs.SelectedIndex = 1; // Switch to download tab
            Program.StartDownloader(this); // Run console App program
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

                List<string> newUrl = new List<string>();
                newUrl.Add(urlInput);
                List<ScanWebsiteUrl> newScansToAdd = Program.CreateListOfScanWebsiteUrl(newUrl);                

                if (newScansToAdd.Any())
                {
                    ScanWebsiteUrls.AddRange(newScansToAdd); // TODO: Also update variable on Program --> keep only one of the two variable

                    // Save the settings
                    Settings.instance.ScansUrlAndCorrespondingChapters.Add(urlInput, chapterInput);
                    Program.SaveSettings(Settings.instance);
                }
                else
                {
                    MessageBox.Show($"{urlInput} not added, it was not a valid url", "Invalid URL", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
                
                // Refresh list on Ui
                RefreshScanListView();
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
        #endregion

        private void RefreshScanListView()
        {
            ScanListItems.Clear();

            foreach (var scanWebsiteUrl in ScanWebsiteUrls)
            {
                TextBlock scanLine = new TextBlock();
                scanLine.Text = $"{scanWebsiteUrl.BookName} - Chapter {scanWebsiteUrl.ChapterId} ({scanWebsiteUrl.Url})";
                ScanListItems.Add(scanLine);
            }
        }
    }
}