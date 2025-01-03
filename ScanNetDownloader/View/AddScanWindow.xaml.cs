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

        private ScanData TempScanData { get; set; }

        public List<ScanData> NewScanDatas { get; set; }

        public List<int> ChapterSelected { get; set; }

        public bool Success { get; set; } = false;

        public AddScanWindow(Window parentWindow)
        {
            Owner = parentWindow;
            InitializeComponent();
        }

        #region Button Events
        private void btnUrlView_Click(object sender, RoutedEventArgs e)
        {
            urlSelectionVw.Visibility = Visibility.Visible;
            chapterSelectionVw.Visibility = Visibility.Collapsed;
            scanDataCreationVw.Visibility = Visibility.Collapsed;

            headerUrlView.FontWeight = FontWeights.SemiBold;
            headerUrlView.Foreground = (SolidColorBrush)FindResource("Colors.Black");
            headerFirstSeparator.FontWeight = FontWeights.SemiBold;
            headerFirstSeparator.Foreground = (SolidColorBrush)FindResource("Colors.Black");
            //btnUrlView.IsEnabled = false;
            //btnUrlView.FontWeight = FontWeights.Bold;

            headerChapterView.FontWeight = FontWeights.Light;
            headerChapterView.Foreground = (SolidColorBrush)FindResource("Colors.DarkGrey");
            headerSecondSeparator.FontWeight = FontWeights.Light;
            headerSecondSeparator.Foreground = (SolidColorBrush)FindResource("Colors.DarkGrey");
            //btnChapterView.IsEnabled = false;
            //btnChapterView.FontWeight = FontWeights.Normal;

            headerScanDataCreationView.FontWeight = FontWeights.Light;
            headerScanDataCreationView.Foreground = (SolidColorBrush)FindResource("Colors.DarkGrey");
            //btnScanDataCreationView.IsEnabled = false;
            //btnScanDataCreationView.FontWeight = FontWeights.Normal;
        }

        private void btnChapterView_Click(object sender, RoutedEventArgs e)
        {
            urlSelectionVw.Visibility = Visibility.Collapsed;
            chapterSelectionVw.Visibility = Visibility.Visible;
            scanDataCreationVw.Visibility = Visibility.Collapsed;

            headerUrlView.FontWeight = FontWeights.Normal;
            headerUrlView.Foreground = (SolidColorBrush)FindResource("Colors.Black");
            headerFirstSeparator.FontWeight = FontWeights.Normal;
            headerFirstSeparator.Foreground = (SolidColorBrush)FindResource("Colors.Black");
            //btnUrlView.IsEnabled = true;
            //btnUrlView.FontWeight = FontWeights.Normal;

            headerChapterView.FontWeight = FontWeights.SemiBold;
            headerChapterView.Foreground = (SolidColorBrush)FindResource("Colors.Black");
            headerSecondSeparator.FontWeight = FontWeights.SemiBold;
            headerSecondSeparator.Foreground = (SolidColorBrush)FindResource("Colors.Black");
            //btnChapterView.IsEnabled = false;
            //btnChapterView.FontWeight = FontWeights.Bold;

            headerScanDataCreationView.FontWeight = FontWeights.Light;
            headerScanDataCreationView.Foreground = (SolidColorBrush)FindResource("Colors.DarkGrey");
            //btnScanDataCreationView.IsEnabled = false;
            //btnScanDataCreationView.FontWeight = FontWeights.Normal;
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
        #endregion


        #region Custom view events
        private void urlSelectionVw_UrlConfirmed(object sender, RoutedEventArgs e)
        {
            UrlInput = urlSelectionVw.UrlInput;
            TempScanData = urlSelectionVw.TempScanData;

            // Check if chapter is already specified

            chapterSelectionVw.InitChapterSelection(TempScanData);

            urlSelectionVw.Visibility = Visibility.Collapsed;
            chapterSelectionVw.Visibility = Visibility.Visible;

            headerUrlView.FontWeight = FontWeights.Normal;
            headerUrlView.Foreground = (SolidColorBrush)FindResource("Colors.Black");
            headerFirstSeparator.FontWeight = FontWeights.Normal;
            headerFirstSeparator.Foreground = (SolidColorBrush)FindResource("Colors.Black");
            //btnUrlView.IsEnabled = true;
            //btnUrlView.FontWeight = FontWeights.Normal;

            headerChapterView.FontWeight = FontWeights.SemiBold;
            headerChapterView.Foreground = (SolidColorBrush)FindResource("Colors.Black");
            headerSecondSeparator.FontWeight = FontWeights.SemiBold;
            headerSecondSeparator.Foreground = (SolidColorBrush)FindResource("Colors.Black");
            //btnChapterView.IsEnabled = false;
            //btnChapterView.FontWeight = FontWeights.Bold;
        }

        private void chapterSelectionVw_BackBtnPressed(object sender, RoutedEventArgs e)
        {
            btnUrlView_Click(sender, e);
        }

        private async void chapterSelectionVw_ChaptersConfirmed(object sender, RoutedEventArgs e)
        {
            
            ChapterSelected = chapterSelectionVw.ChaptersSelected;

            chapterSelectionVw.Visibility = Visibility.Collapsed;
            scanDataCreationVw.Visibility = Visibility.Visible;

            headerUrlView.FontWeight = FontWeights.Normal;
            headerUrlView.Foreground = (SolidColorBrush)FindResource("Colors.Black");
            headerFirstSeparator.FontWeight = FontWeights.Normal;
            headerFirstSeparator.Foreground = (SolidColorBrush)FindResource("Colors.Black");
            //btnUrlView.IsEnabled = false;
            //btnUrlView.FontWeight = FontWeights.Normal;

            headerChapterView.FontWeight = FontWeights.Normal;
            headerChapterView.Foreground = (SolidColorBrush)FindResource("Colors.Black");
            headerSecondSeparator.FontWeight = FontWeights.Normal;
            headerSecondSeparator.Foreground = (SolidColorBrush)FindResource("Colors.Black");
            //btnChapterView.IsEnabled = false;
            //btnChapterView.FontWeight = FontWeights.Normal;

            headerScanDataCreationView.FontWeight = FontWeights.SemiBold;
            headerScanDataCreationView.Foreground = (SolidColorBrush)FindResource("Colors.Black");
            //btnScanDataCreationView.IsEnabled = false;
            //btnScanDataCreationView.FontWeight = FontWeights.Bold;

            // Create Scan Datas
            NewScanDatas = await scanDataCreationVw.CreateScanDatas(UrlInput, ChapterSelected, TempScanData);

            if (NewScanDatas != null && NewScanDatas.Count > 0)
            {
                Success = true;
            }
        }

        private void scanDataCreationVw_FinishBtnPressed(object sender, RoutedEventArgs e)
        {
            Close();
        }
        #endregion
    }
}
