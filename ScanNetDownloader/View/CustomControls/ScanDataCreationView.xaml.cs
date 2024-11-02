using ScanNetDownloader.Logic;
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

namespace ScanNetDownloader.View.CustomControls
{
    /// <summary>
    /// Logique d'interaction pour ScanDataCreationView.xaml
    /// </summary>
    public partial class ScanDataCreationView : UserControl
    {
        private List<ScanData> NewScanDatas { get; set; }

        // View Events
        public static RoutedEvent FinishBtnPressedEvent = EventManager.RegisterRoutedEvent(nameof(FinishBtnPressed), RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(ScanItem));

        public event RoutedEventHandler FinishBtnPressed
        {
            add { AddHandler(FinishBtnPressedEvent, value); }
            remove { RemoveHandler(FinishBtnPressedEvent, value); }
        }


        public ScanDataCreationView()
        {
            InitializeComponent();
        }


        public async Task<List<ScanData>> CreateScanDatas(string url, List<int> chaptersSelected, ScanData tempScanData)
        {
            NewScanDatas = new List<ScanData>();

            progrBarScanDataCreation.Value = 0;

            for (int i = 0; i < chaptersSelected.Count; i++)
            {
                int chapter = chaptersSelected[i];

                // TODO: Replace txtBlock with a dedicated item
                TextBlock chapterTxtBlock = new TextBlock();
                chapterTxtBlock.TextWrapping = TextWrapping.Wrap;
                chapterTxtBlock.Text = $"Scan Data creation for {tempScanData.BookName}-{chapter} in progress...";
                listVwCreationStatus.Items.Add(chapterTxtBlock);

                ScanData newScanData = await ScanManagement.CreateNewScanData(url, chapter);
                if (newScanData != null)
                {
                    NewScanDatas.Add(newScanData);
                    chapterTxtBlock.Text = $"Scan Data creation for {tempScanData.BookName}-{chapter} successful!";
                    chapterTxtBlock.Foreground = Brushes.Green;
                }
                else
                {
                    chapterTxtBlock.Text = $"Scan Data creation for {tempScanData.BookName}-{chapter} failed!";
                    // TODO: Add reason why it failed
                    chapterTxtBlock.Foreground = Brushes.Red;
                }
                progrBarScanDataCreation.Value = ((float)(i + 1) / chaptersSelected.Count) * 100;
            }

            // Old way doing everything at once -> no progress evolution
            //NewScanDatas = await ScanManagement.CreateNewScanDatas(UrlInput, ChapterSelected);

            btnFinish.IsEnabled = true;
            return NewScanDatas;
        }

        private void btnFinish_Click(object sender, RoutedEventArgs e)
        {
            RaiseEvent(new RoutedEventArgs(FinishBtnPressedEvent, this));
        }
    }
}
