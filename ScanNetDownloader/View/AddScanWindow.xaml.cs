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

        private void btnUrlView_Click(object sender, RoutedEventArgs e)
        {
            urlSelectionVw.Visibility = Visibility.Visible;
            chapterSelectionVw.Visibility = Visibility.Collapsed;
            scanDataCreationVw.Visibility = Visibility.Collapsed;

            btnUrlView.IsEnabled = false;
            btnUrlView.FontWeight = FontWeights.Bold;

            btnChapterView.IsEnabled = false;
            btnChapterView.FontWeight = FontWeights.Normal;

            btnScanDataCreationView.IsEnabled = false;
            btnScanDataCreationView.FontWeight = FontWeights.Normal;
        }

        private void btnChapterView_Click(object sender, RoutedEventArgs e)
        {
            urlSelectionVw.Visibility = Visibility.Collapsed;
            chapterSelectionVw.Visibility = Visibility.Visible;
            scanDataCreationVw.Visibility = Visibility.Collapsed;

            btnUrlView.IsEnabled = true;
            btnUrlView.FontWeight = FontWeights.Normal;

            btnChapterView.IsEnabled = false;
            btnChapterView.FontWeight = FontWeights.Bold;

            btnScanDataCreationView.IsEnabled = false;
            btnScanDataCreationView.FontWeight = FontWeights.Normal;
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
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

        private async void chapterSelectionVw_ChaptersConfirmed(object sender, RoutedEventArgs e)
        {
            ChapterSelected = chapterSelectionVw.ChaptersSelected;
            ChapterSelected.Log();

            chapterSelectionVw.Visibility = Visibility.Collapsed;
            scanDataCreationVw.Visibility = Visibility.Visible;

            btnUrlView.IsEnabled = false;
            btnUrlView.FontWeight = FontWeights.Normal;

            btnChapterView.IsEnabled = false;
            btnChapterView.FontWeight = FontWeights.Normal;

            btnScanDataCreationView.IsEnabled = false;
            btnScanDataCreationView.FontWeight = FontWeights.Bold;

            // TODO: Create scan data one by one to have a better visual representation of what is happening
            progrBarScanDataCreation.Value = 0;

            NewScanDatas = new List<ScanData>();
            for (int i = 0; i < ChapterSelected.Count; i++)
            {
                int chapter = ChapterSelected[i];

                // TODO: Replace txtBlock with a dedicated item
                TextBlock chapterTxtBlock = new TextBlock();
                chapterTxtBlock.TextWrapping = TextWrapping.Wrap;
                chapterTxtBlock.Text = $"Scan Data creation for {TempScanData.BookName}-{chapter} in progress...";
                listVwCreationStatus.Items.Add(chapterTxtBlock);

                ScanData newScanData = await ScanManagement.CreateNewScanData(UrlInput, chapter);
                if(newScanData != null)
                {
                    NewScanDatas.Add(newScanData);
                    chapterTxtBlock.Text = $"Scan Data creation for {TempScanData.BookName}-{chapter} successful!";
                    chapterTxtBlock.Foreground = Brushes.Green;
                }
                else
                {
                    chapterTxtBlock.Text = $"Scan Data creation for {TempScanData.BookName}-{chapter} failed!";
                    // TODO: Add reason why it failed
                    chapterTxtBlock.Foreground = Brushes.Red;
                }
                progrBarScanDataCreation.Value = ((float)(i+1) / ChapterSelected.Count) * 100;
            }
            // Old way doing everything at once -> no progress evolution
            //NewScanDatas = await ScanManagement.CreateNewScanDatas(UrlInput, ChapterSelected);

            if (NewScanDatas != null && NewScanDatas.Count > 0)
            {
                Success = true;
            }
            
            btnFinish.IsEnabled = true;
        }

        private void btnFinish_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
