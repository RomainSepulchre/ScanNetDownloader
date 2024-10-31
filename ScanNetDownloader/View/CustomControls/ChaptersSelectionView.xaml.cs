using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using ScanNetDownloader.Logic;

namespace ScanNetDownloader.View.CustomControls
{
    /// <summary>
    /// Logique d'interaction pour ChaptersSelectionView.xaml
    /// </summary>
    public partial class ChaptersSelectionView : UserControl
    {
        public ScanData TempScanData { get; set; }

        public string ChapterInput { get; set; }

        public List<string> ChaptersString { get; set; } = new List<string>();

        public static RoutedEvent BackBtnPressedEvent = EventManager.RegisterRoutedEvent(nameof(BackBtnPressed), RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(ScanItem));

        public event RoutedEventHandler BackBtnPressed
        {
            add { AddHandler(BackBtnPressedEvent, value); }
            remove { RemoveHandler(BackBtnPressedEvent, value); }
        }

        public static RoutedEvent ChaptersConfirmedEvent = EventManager.RegisterRoutedEvent(nameof(ChaptersConfirmed), RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(ScanItem));

        public event RoutedEventHandler ChaptersConfirmed
        {
            add { AddHandler(ChaptersConfirmedEvent, value); }
            remove { RemoveHandler(ChaptersConfirmedEvent, value); }
        }



        public ChaptersSelectionView()
        {
            InitializeComponent();
        }

        public void StartChapterSelection(ScanData tempScanData) // TODO: How to manage adding more chapters for Url with chapter number -> add url with chapter first remove chapter from list and then generate url for all the other added chapter
        {
            TempScanData = tempScanData;

            SetUrlInfo();

            if (TempScanData.UrlContainsChapter())
            {
                string chapterToAdd = TempScanData.ChapterId.ToString();

                AddChapterSelected(chapterToAdd);
                if (string.IsNullOrEmpty(ChapterInput))
                {
                    ChapterInput += chapterToAdd;
                }
                else
                {
                    ChapterInput += ";" + chapterToAdd;
                }
            }
        }

        private void SetUrlInfo()
        {
            if (TempScanData.UrlContainsChapter())
            {
                txtBlockUrlInfo.Text = $"Chapter {TempScanData.ChapterId} is already specified in {TempScanData.Url} but you can add additionnal chapter";
            }
            else
            {
                txtBlockUrlInfo.Text = $"Choose chapters for {TempScanData.Url}";
            }      
        }
   

        private void btnAddSingleChapter_Click(object sender, RoutedEventArgs e)
        {
            bool validNumber = int.TryParse(txtBoxSingleChapter.Text, out int chapterToAdd);
            if (validNumber)
            {
                // TODO: Check if chapter exist online

                // Add chapter
                string singleChapter = txtBoxSingleChapter.Text;
                ChaptersString.Add(singleChapter);
                AddChapterSelected(singleChapter);

                if (string.IsNullOrEmpty(ChapterInput))
                {
                    ChapterInput += singleChapter;
                }
                else
                {
                    ChapterInput += ";" + singleChapter;
                }

                // Clear txt box
                txtBoxSingleChapter.Text = "";
            }
            else // Invalid number entered
            {

                //TODO: Show error, Add text explanation
                txtBoxSingleChapter.Background = Brushes.IndianRed;
            }
        }

        private void btnAddRangeOfChapter_Click(object sender, RoutedEventArgs e)
        {
            int startChapter, endChapter;
            bool validStartChapter = int.TryParse(txtBoxStartChapter.Text, out startChapter);
            bool validEndChapter = int.TryParse(txtBoxEndChapter.Text, out endChapter);

            if (validStartChapter && validEndChapter)
            {
                if (startChapter > endChapter) (startChapter, endChapter) = (endChapter, startChapter); // invert two value to make sure start is the lower value

                // TODO: Check if chapter exist online
                // Add chapter
                string chapterRange = $"{startChapter}-{endChapter}";
                ChaptersString.Add(chapterRange);
                AddChapterSelected(chapterRange);
                if (string.IsNullOrEmpty(ChapterInput))
                {
                    ChapterInput += chapterRange;
                }
                else
                {
                    ChapterInput += ";" + chapterRange;
                }


                // Clear txt box
                txtBoxStartChapter.Text = "";
                txtBoxEndChapter.Text = "";
            }
            else // Invalid number entered
            {
                //TODO: Show error, Add text explanation
                if (validStartChapter == false) txtBoxStartChapter.Background = Brushes.IndianRed;
                if (validEndChapter == false) txtBoxEndChapter.Background = Brushes.IndianRed;
            }
        }

        private void AddChapterSelected(string chapter)
        {
            TextBlock chapterTxtBlock = new TextBlock();
            chapterTxtBlock.Text = chapter;
            chapterTxtBlock.Width = 50;
            chapterTxtBlock.Height = 20;
            chapterTxtBlock.Background = Brushes.Orange;
            chapterTxtBlock.VerticalAlignment = VerticalAlignment.Center;
            chapterTxtBlock.TextAlignment = TextAlignment.Center;
            chapterTxtBlock.Margin = new Thickness(10);

            chapterSelectedPanel.Children.Add(chapterTxtBlock);

            btnConfirmChapters.IsEnabled = true;
        }

        private void btnBackToUrlSelection_Click(object sender, RoutedEventArgs e)
        {
            RaiseEvent(new RoutedEventArgs(BackBtnPressedEvent, this));
        }

        private void btnConfirmChapters_Click(object sender, RoutedEventArgs e)
        {
            RaiseEvent(new RoutedEventArgs(ChaptersConfirmedEvent, this));
        }
    }
}
