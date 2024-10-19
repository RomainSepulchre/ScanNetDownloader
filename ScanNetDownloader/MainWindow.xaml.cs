using ScanNetDownloader.ConsoleApp;
using ScanNetDownloader.View;
using System.Diagnostics;
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
    public partial class MainWindow : Window
    {
        public List<ScanWebsiteUrl> ScanWebsiteUrls { get; set; }

        public MainWindow()
        {
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

        private void btnStart_Click(object sender, RoutedEventArgs e)
        {
            Program.StartDownloader(this); // Run console App program
        }

        private void btnAdd_Click(object sender, RoutedEventArgs e)
        {
            AddScanWindow addWindow = new AddScanWindow(this);
            Opacity = 0.4;
            addWindow.ShowDialog();
            Opacity = 1;

            if(addWindow.Success)
            {
                string urlInput = addWindow.UrlInput;
                string chapterInput = addWindow.ChapterInput;

                // TODO: how to handle easily the creation of a new chapter

                // Save the settings
                Settings.instance.ScansUrlAndCorrespondingChapters.Add(urlInput, chapterInput);
                Program.SaveSettings(Settings.instance);

                List<string> newUrl = new List<string>();
                newUrl.Add(urlInput);
                List<ScanWebsiteUrl> newScansToAdd = Program.CreateListOfScanWebsiteUrl(newUrl);
                ScanWebsiteUrls.AddRange(newScansToAdd); // TODO: Also update variable on Program --> keep only one of the two variable



                // Refresh list on Ui
                RefreshScanListView();
            }
        }

        private void RefreshScanListView()
        {
            scanListView.Items.Clear();

            foreach (var scanWebsiteUrl in ScanWebsiteUrls)
            {
                TextBlock scanLine = new TextBlock();
                scanLine.Text = $"{scanWebsiteUrl.BookName} - Chapter {scanWebsiteUrl.ChapterId} ({scanWebsiteUrl.Url})";
                scanListView.Items.Add(scanLine);
            }
        }
    }
}