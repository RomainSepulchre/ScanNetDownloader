using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using ScanNetDownloader.Logic;
using ScanNetDownloader.View.CustomControls;
using static System.Net.Mime.MediaTypeNames;

namespace ScanNetDownloader.View
{
    /// <summary>
    /// Logique d'interaction pour AddScanWindow.xaml
    /// </summary>
    public partial class AddScanWindow : Window
    {
        public string UrlInput { get; set; }

        public ScanData TempScanData { get; set; }

        public string ChapterInput { get; set; }

        public List<int> ChapterSelected { get; set; }

        public bool Success { get; set; } = false;

        public AddScanWindow(Window parentWindow)
        {
            Owner = parentWindow;
            InitializeComponent();


        }

        private void btnUrlView_Click(object sender, RoutedEventArgs e)
        {
            urlSelectionVw.Visibility = Visibility.Visible;
            chapterSelectionVw.Visibility = Visibility.Collapsed;

            btnUrlView.IsEnabled = false;
            btnUrlView.FontWeight = FontWeights.Bold;

            btnChapterView.IsEnabled = false;
            btnChapterView.FontWeight = FontWeights.Normal;
        }

        private void btnChapterView_Click(object sender, RoutedEventArgs e)
        {
            urlSelectionVw.Visibility = Visibility.Collapsed;
            chapterSelectionVw.Visibility = Visibility.Visible;

            btnUrlView.IsEnabled = true;
            btnUrlView.FontWeight = FontWeights.Normal;

            btnChapterView.IsEnabled = false;
            btnChapterView.FontWeight = FontWeights.Bold;
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            Success = false;
            Close();
        }

        private void urlSelectionVw_UrlConfirmed(object sender, RoutedEventArgs e)
        {
            UrlInput = urlSelectionVw.UrlInput;
            TempScanData = urlSelectionVw.TempScanData;

            // Check if chapter is already specified

            chapterSelectionVw.StartChapterSelection(TempScanData);
                   
            urlSelectionVw.Visibility = Visibility.Collapsed;
            chapterSelectionVw.Visibility = Visibility.Visible;

            btnUrlView.IsEnabled = true;
            btnUrlView.FontWeight = FontWeights.Normal;

            btnChapterView.IsEnabled = false;
            btnChapterView.FontWeight = FontWeights.Bold;
        }

        private void chapterSelectionVw_BackBtnPressed(object sender, RoutedEventArgs e)
        {
            btnUrlView_Click(sender, e);
        }

        private void chapterSelectionVw_ChaptersConfirmed(object sender, RoutedEventArgs e)
        {
            ChapterInput = chapterSelectionVw.ChapterInput;
            Success = true;
            Close();
        }
    }
}
