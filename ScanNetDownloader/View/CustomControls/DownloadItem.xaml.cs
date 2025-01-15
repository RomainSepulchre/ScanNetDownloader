using ScanNetDownloader.Logic.Helpers;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace ScanNetDownloader.View.CustomControls
{
    /// <summary>
    /// Logique d'interaction pour DownloadItem.xaml
    /// </summary>
    public partial class DownloadItem : UserControl, INotifyPropertyChanged
    {
        private ObservableCollection<DownloadEventItem> _downloadEventItems;

        public ObservableCollection<DownloadEventItem> DownloadEventItems
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

        public int ErrorCount { get; set; } = 0;

        public bool CbzCreationError { get; set; } = false;

        private string _itemHeader;
        public string ItemHeader
        {
            get { return _itemHeader; }
            set {
                _itemHeader = value;
                OnPropertyChanged();
            }
        }

        private string _downloadStatus;
        public string DownloadStatus
        {
            get { return _downloadStatus; }
            set {
                _downloadStatus = value;
                OnPropertyChanged();
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string property = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(property));
        }

        public DownloadItem()
        {
            DataContext = this;
            InitializeComponent();
        }

        public DownloadItem(ScanItem _scanItem)
        {
            DownloadEventItems = new ObservableCollection<DownloadEventItem>();

            InitializeComponent();

            DataContext = this;

            linkedScanItem = _scanItem;

            BookName = _scanItem.BookName;
            ChapterId = _scanItem.ChapterId;
            PagesCount = _scanItem.PagesCount;
            Url = _scanItem.Url;
            Website = _scanItem.Website;

            ItemHeader = $"{BookName} - Chapter {ChapterId} ({PagesCount} pages)";
            DownloadStatus = "Ready for download...";

            ShowDownloadDetails(false);
            SetShowDetailsBtnVisibility();
        }

        private void btnShowDetails_Click(object sender, RoutedEventArgs e)
        {
            if (downloadDetailsVw.Visibility == Visibility.Visible)
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
                downloadDetailsVw.Visibility = Visibility.Collapsed;
                Image img = (Image)btnShowDetails.Content; 
                img.Source = (BitmapImage)FindResource(ResourcesKey.BitmapImages.MoreDetailsIcon_Closed);
            }
            else
            {
                downloadDetailsVw.Visibility = Visibility.Visible;
                Image img = (Image)btnShowDetails.Content;
                img.Source = (BitmapImage)FindResource(ResourcesKey.BitmapImages.MoreDetailsIcon_Oppened);
            }
        }

        public void PrepareForDownload()
        {
            DownloadStatus = $"Waiting for download start...";
        }

        public void ScanDownloadStarted()
        {
            progrBarItemDownload.Visibility = Visibility.Visible;
            progrBarItemDownload.Value = 0;
            DownloadStatus = $"Downloading page 1...";
        }

        public void PageDownloaded(int pageNumber)
        {
            string msg = $"Page {pageNumber} successfully downloaded !";
            DownloadEventItem eventItem = new DownloadEventItem(msg, (SolidColorBrush)FindResource(ResourcesKey.ColorBrushes.Green));
            int countBeforeAdd = DownloadEventItems.Count;
            DownloadEventItems.Add(eventItem);
            if (countBeforeAdd == 0) SetShowDetailsBtnVisibility();

            int nextPageNumber = pageNumber + 1;
            progrBarItemDownload.Value = ((float)pageNumber / PagesCount) * 100;
            if (pageNumber != PagesCount)
            {
                DownloadStatus = $"Downloading page {nextPageNumber}...";
            }
        }

        public void PageAlreadyDownloaded(int pageNumber)
        {
            string msg = $"Page {pageNumber} already downloaded !";
            DownloadEventItem eventItem = new DownloadEventItem(msg, (SolidColorBrush)FindResource(ResourcesKey.ColorBrushes.Green));
            int countBeforeAdd = DownloadEventItems.Count;
            DownloadEventItems.Add(eventItem);
            if (countBeforeAdd == 0) SetShowDetailsBtnVisibility();

            int nextPageNumber = pageNumber + 1;
            progrBarItemDownload.Value = ((float)pageNumber / PagesCount) * 100;
            if (pageNumber != PagesCount)
            {
                DownloadStatus = $"Downloading page {nextPageNumber}...";
            }
        }

        public void PageDownloadError(int pageNumber, Exception ex)
        {
            ErrorCount++;
            string errorMsg = $"Error while downloading page {pageNumber}: {ex.Message}";
            DownloadEventItem eventItem = new DownloadEventItem(errorMsg, (SolidColorBrush)FindResource(ResourcesKey.ColorBrushes.Red));
            int countBeforeAdd = DownloadEventItems.Count;
            DownloadEventItems.Add(eventItem);
            if (countBeforeAdd == 0) SetShowDetailsBtnVisibility();

            int nextPageNumber = pageNumber + 1;
            progrBarItemDownload.Value = ((float)pageNumber / PagesCount) * 100;
            if (pageNumber != PagesCount)
            {
                DownloadStatus = $"Downloading page {nextPageNumber}...";
            }
        }

        public void PageFileSavingError(int pageNumber, Exception ex)
        {
            ErrorCount++;
            string errorMsg = $"Error while saving file for page {pageNumber}: {ex.Message}";
            DownloadEventItem eventItem = new DownloadEventItem(errorMsg, (SolidColorBrush)FindResource(ResourcesKey.ColorBrushes.Red));
            int countBeforeAdd = DownloadEventItems.Count;
            DownloadEventItems.Add(eventItem);
            if (countBeforeAdd == 0) SetShowDetailsBtnVisibility();

            int nextPageNumber = pageNumber + 1;
            progrBarItemDownload.Value = ((float)pageNumber / PagesCount) * 100;
            if (pageNumber != PagesCount)
            {
                DownloadStatus = $"Downloading page {nextPageNumber}...";
            }
        }

        public void ScanDownloadError(Exception ex)
        {
            gridDlStatus.Background = (SolidColorBrush)FindResource(ResourcesKey.ColorBrushes.Red);
            DownloadStatus = "An error happened while downloading the scan, see download details";

            string errorMsg = $"Error while downloading scan {BookName}-{ChapterId}: {ex.Message}";
            DownloadEventItem eventItem = new DownloadEventItem(errorMsg, (SolidColorBrush)FindResource(ResourcesKey.ColorBrushes.Red));
            int countBeforeAdd = DownloadEventItems.Count;
            DownloadEventItems.Add(eventItem);
            if (countBeforeAdd == 0) SetShowDetailsBtnVisibility();
        }

        public void ScanDownloaded()
        {
            progrBarItemDownload.Visibility = Visibility.Collapsed;

            // TODO: add the possibility to retry the download of the item when error happened
            if (ErrorCount == 0)
            {
                if (CbzCreationError)
                {
                    gridDlStatus.Background = (SolidColorBrush)FindResource(ResourcesKey.ColorBrushes.Orange);
                    DownloadStatus = "Successfully downloaded but cbz archive creation failed";
                }
                else
                {
                    gridDlStatus.Background = (SolidColorBrush)FindResource(ResourcesKey.ColorBrushes.Green);
                    DownloadStatus = "Successfully downloaded";
                }
            }
            else if (ErrorCount >= 1 && ErrorCount <= (PagesCount*0.1f)) // Less than 10% of error
            {
                gridDlStatus.Background = (SolidColorBrush)FindResource(ResourcesKey.ColorBrushes.Orange);
                if (CbzCreationError)
                {
                    DownloadStatus = "Scan downloaded but some pages download and cbz archive creation failed, see download details";
                }
                else
                {
                    DownloadStatus = "Scan downloaded but some pages download failed, see download details";
                }
            }
            else // Too many download error
            {
                gridDlStatus.Background = (SolidColorBrush)FindResource(ResourcesKey.ColorBrushes.Red);
                txtBlockDlStatus.Foreground = (SolidColorBrush)FindResource(ResourcesKey.ColorBrushes.White);
                if (CbzCreationError)
                {
                    DownloadStatus = "Errors while downloading scan pages and creating cbz archive, see download details";
                }
                else
                {
                    DownloadStatus = "Errors while downloading scan pages, see download details";
                }  
            } 
        }

        public void CbzCreationStarted()
        {
            DownloadStatus = "Creating cbz archive...";
        }

        public void CbzCreated()
        {
            string msg = $"Cbz archive for {BookName}-{ChapterId} successfully created ";
            DownloadEventItem eventItem = new DownloadEventItem(msg, (SolidColorBrush)FindResource(ResourcesKey.ColorBrushes.Green));
            int countBeforeAdd = DownloadEventItems.Count;
            DownloadEventItems.Add(eventItem);
            if (countBeforeAdd == 0) SetShowDetailsBtnVisibility();
        }

        public void CbzAlreadyCreated()
        {
            string msg = $"Cbz archive for {BookName}-{ChapterId} already created";
            DownloadEventItem eventItem = new DownloadEventItem(msg, (SolidColorBrush)FindResource(ResourcesKey.ColorBrushes.Green));
            int countBeforeAdd = DownloadEventItems.Count;
            DownloadEventItems.Add(eventItem);
            if (countBeforeAdd == 0) SetShowDetailsBtnVisibility();
        }

        public void CbzCreationFailed(string msg, Exception ex)
        {
            CbzCreationError = true;

            string errorMsg = $"{msg} for {BookName}-{ChapterId}: {ex.Message}";
            DownloadEventItem eventItem = new DownloadEventItem(errorMsg, (SolidColorBrush)FindResource(ResourcesKey.ColorBrushes.Red));
            int countBeforeAdd = DownloadEventItems.Count;
            DownloadEventItems.Add(eventItem);
            if (countBeforeAdd == 0) SetShowDetailsBtnVisibility();
        }

        public void SetShowDetailsBtnVisibility()
        {
            if(DownloadEventItems.Count > 0)
            {
                btnShowDetails.IsEnabled = true;
            }
            else
            {
                btnShowDetails.IsEnabled = false;
            }
        }
    }
}
