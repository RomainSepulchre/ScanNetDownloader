using ScanNetDownloader.Logic;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
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
    public partial class ScanDataCreationView : UserControl, INotifyPropertyChanged
    {
        private List<ScanData> NewScanDatas { get; set; }

        private string _progressStatus;
        public string ProgressStatus
        {
            get { return _progressStatus; }
            set {
                _progressStatus = value;
                OnPropertyChanged();
            }
        }

        // View Events
        public static RoutedEvent FinishBtnPressedEvent = EventManager.RegisterRoutedEvent(nameof(FinishBtnPressed), RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(ScanDataCreationView));

        public event RoutedEventHandler FinishBtnPressed
        {
            add { AddHandler(FinishBtnPressedEvent, value); }
            remove { RemoveHandler(FinishBtnPressedEvent, value); }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        private void OnPropertyChanged([CallerMemberName] string property = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(property));
        }

        public ScanDataCreationView()
        {
            DataContext = this;
            InitializeComponent();
        }


        public async Task<List<ScanData>> CreateScanDatas(string url, List<int> chaptersSelected, ScanData tempScanData)
        {
            NewScanDatas = new List<ScanData>();

            progrBarScanDataCreation.Value = 0;

            for (int i = 0; i < chaptersSelected.Count; i++)
            {
                int chapter = chaptersSelected[i];
                ProgressStatus = $"{tempScanData.BookName} - {chapter} ({i}/{chaptersSelected.Count})";

                ScanDataCreationItem scanDataCreationItem = new ScanDataCreationItem(tempScanData.BookName, chapter);
                //listVwCreationStatus.Items.Add(scanDataCreationItem);
                listVwCreationStatus.Children.Add(scanDataCreationItem);

                ScanDataInitResult newScanDataResult = await ScanManagement.CreateNewScanData(url, chapter);

                if (newScanDataResult.Success)
                {
                    NewScanDatas.Add(newScanDataResult.NewScanData);
                    scanDataCreationItem.RetrieveDataSuccess($"Success - {newScanDataResult.NewScanData.PagesCount} pages found");
                }
                else
                {
                    scanDataCreationItem.RetrieveDataFailed(newScanDataResult.Exception.Message);
                }
                progrBarScanDataCreation.Value = ((float)(i + 1) / chaptersSelected.Count) * 100;
            }
            ProgressStatus = $"Done ({chaptersSelected.Count}/{chaptersSelected.Count})";

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
