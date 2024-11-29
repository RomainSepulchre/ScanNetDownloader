using ScanNetDownloader.Logic;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
    /// Logique d'interaction pour DownloadItem.xaml
    /// </summary>
    public partial class DownloadItem : UserControl
    {
        private ObservableCollection<Border> _downloadEventItems;

        public ObservableCollection<Border> DownloadEventItems
        {
            get { return _downloadEventItems; }
            set { _downloadEventItems = value; }
        }

        public ScanItem linkedScanItem { get; private set; }

        public string BookName { get; private set; }

        public int ChapterId { get; private set; }

        public int PagesCount { get; private set; }

        public string Url { get; private set; }

        public string Website { get; private set; }

        public DownloadItem()
        {
            InitializeComponent();
        }

        public DownloadItem(ScanItem _scanItem)
        {
            DownloadEventItems = new ObservableCollection<Border>();

            InitializeComponent();

            DataContext = this;

            linkedScanItem = _scanItem;

            BookName = _scanItem.BookName;
            ChapterId = _scanItem.ChapterId;
            PagesCount = _scanItem.PagesCount;
            Url = _scanItem.Url;
            Website = _scanItem.Website;

            txtBlockItemHeader.Text = $"{BookName} - Chapter {ChapterId} ({PagesCount} pages)";

            ShowDownloadDetails(false);
            SetShowDetailsBtnVisibility();
        }

        private void btnShowDetails_Click(object sender, RoutedEventArgs e)
        {
            if (stPanelDetails.Visibility == Visibility.Visible)
            {
                ShowDownloadDetails(false);
            }
            else
            {
                ShowDownloadDetails(true);
            }
        }

        private void ShowDownloadDetails(bool showDetails)
        {
            if (!showDetails)
            {
                stPanelDetails.Visibility = Visibility.Collapsed;
                btnShowDetails.Content = "˅";
            }
            else
            {
                stPanelDetails.Visibility = Visibility.Visible;
                btnShowDetails.Content = "˃";
            }
        }

        public void PrepareForDownload()
        {
            progrBarItemDownload.Visibility = Visibility.Visible;
            progrBarItemDownload.Value = 0;
            txtBlockDlStatus.Text = $"Waiting for download start...";
        }

        public void PageDownloaded(int pageIndex)
        {
            Border pageItem = CreateNewDownloadEvent(pageIndex);
            int countBeforeAdd = DownloadEventItems.Count;
            DownloadEventItems.Add(pageItem);
            if (countBeforeAdd == 0) SetShowDetailsBtnVisibility();

            int pageNumber = pageIndex + 1;
            int nextPageNumber = pageIndex + 2;

            progrBarItemDownload.Value = ((float)pageNumber / PagesCount) * 100;
            txtBlockDlStatus.Text = $"Downloading page {nextPageNumber}...";
        }

        public void ScanDownloaded()
        {
            gridDlStatus.Background = Brushes.Green;
            txtBlockDlStatus.Text = "Downloaded";
        }

        public Border CreateNewDownloadEvent(int pageIndex)
        {
            Border newDlEventItem = new Border();

            newDlEventItem.Height = 20;
            newDlEventItem.Background = Brushes.LightGreen;
            newDlEventItem.BorderThickness = new Thickness(0, 0, 0, 1);
            newDlEventItem.BorderBrush = Brushes.Black;
            newDlEventItem.Margin = new Thickness(10,0,0,0);

            TextBlock info = new TextBlock();
            info.Text = $"Page {pageIndex} successfully downloaded !";
            info.VerticalAlignment = VerticalAlignment.Center;
            info.Margin = new Thickness(5, 0, 0, 0);

            newDlEventItem.Child = info;

            return newDlEventItem;
        }

        public void SetShowDetailsBtnVisibility()
        {
            if(DownloadEventItems.Count > 0) //&& btnShowDetails.Visibility != Visibility.Visible)
            {
                btnShowDetails.IsEnabled = true;
                //btnShowDetails.Visibility = Visibility.Visible;
            }
            else //if(btnShowDetails.Visibility != Visibility.Collapsed)
            {
                btnShowDetails.IsEnabled = false;
                //btnShowDetails.Visibility = Visibility.Collapsed;
            }
        }
    }
}
