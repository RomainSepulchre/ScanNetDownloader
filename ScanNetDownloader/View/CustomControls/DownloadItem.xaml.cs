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
using System.Windows.Interop;
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

        public DownloadItem()
        {
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

            txtBlockItemHeader.Text = $"{BookName} - Chapter {ChapterId} ({PagesCount} pages)";

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
                btnShowDetails.Content = "˅";
            }
            else
            {
                downloadDetailsVw.Visibility = Visibility.Visible;
                btnShowDetails.Content = "˃";
            }
        }

        public void PrepareForDownload()
        {
            progrBarItemDownload.Visibility = Visibility.Visible;
            progrBarItemDownload.Value = 0;
            txtBlockDlStatus.Text = $"Waiting for download start...";
        }

        public void PageDownloaded(int pageNumber)
        {
            string msg = $"Page {pageNumber} successfully downloaded !";
            DownloadEventItem pageItem = new DownloadEventItem(msg, Brushes.LightGreen);
            int countBeforeAdd = DownloadEventItems.Count;
            DownloadEventItems.Add(pageItem);
            if (countBeforeAdd == 0) SetShowDetailsBtnVisibility();

            int nextPageNumber = pageNumber + 1;

            progrBarItemDownload.Value = ((float)pageNumber / PagesCount) * 100;
            if (pageNumber != PagesCount) txtBlockDlStatus.Text = $"Downloading page {nextPageNumber}...";
        }

        public void PageAlreadyDownloaded(int pageNumber)
        {
            string msg = $"Page {pageNumber} already downloaded !";
            DownloadEventItem pageItem = new DownloadEventItem(msg, Brushes.LightGreen);
            int countBeforeAdd = DownloadEventItems.Count;
            DownloadEventItems.Add(pageItem);
            if (countBeforeAdd == 0) SetShowDetailsBtnVisibility();

            int nextPageNumber = pageNumber + 1;

            progrBarItemDownload.Value = ((float)pageNumber / PagesCount) * 100;
            if (pageNumber != PagesCount) txtBlockDlStatus.Text = $"Downloading page {nextPageNumber}...";
        }

        public void PageDownloadError(int pageNumber, Exception ex)
        {
            ErrorCount++;
            string errorMsg = $"Error while downloading page {pageNumber}: {ex.Message}";
            DownloadEventItem pageItem = new DownloadEventItem(errorMsg, Brushes.Red);
            int countBeforeAdd = DownloadEventItems.Count;
            DownloadEventItems.Add(pageItem);
            if (countBeforeAdd == 0) SetShowDetailsBtnVisibility();
        }

        public void PageFileSavingError(int pageNumber, Exception ex)
        {
            ErrorCount++;
            string errorMsg = $"Error while saving file for page {pageNumber}: {ex.Message}";
            DownloadEventItem pageItem = new DownloadEventItem(errorMsg, Brushes.Red);
            int countBeforeAdd = DownloadEventItems.Count;
            DownloadEventItems.Add(pageItem);
            if (countBeforeAdd == 0) SetShowDetailsBtnVisibility();
        }

        public void ScanDownloadError(Exception ex)
        {
            gridDlStatus.Background = Brushes.DarkRed;
            txtBlockDlStatus.Text = "An error happened while downloading the scan, see download details";

            string errorMsg = $"Error while downloading scan {BookName}-{ChapterId}: {ex.Message}";
            DownloadEventItem pageItem = new DownloadEventItem(errorMsg, Brushes.Red);
            int countBeforeAdd = DownloadEventItems.Count;
            DownloadEventItems.Add(pageItem);
            if (countBeforeAdd == 0) SetShowDetailsBtnVisibility();
        }

        public void ScanDownloaded()
        {
            // TODO: add the possibility to retry the download of the item when error happened
            if(ErrorCount == 0)
            {
                if (CbzCreationError)
                {
                    gridDlStatus.Background = Brushes.Orange;
                    txtBlockDlStatus.Text = "Successfully downloaded but cbz archive creation failed";
                }
                else
                {
                    gridDlStatus.Background = Brushes.Green;
                    txtBlockDlStatus.Text = "Successfully downloaded";
                }
            }
            else if (ErrorCount >= 1 && ErrorCount <= (PagesCount*0.1f)) // Less than 10% of error
            {
                gridDlStatus.Background = Brushes.Orange;
                if (CbzCreationError)
                {
                    txtBlockDlStatus.Text = "Scan downloaded but some pages download and cbz archive creation failed, see download details";
                }
                else
                {
                    txtBlockDlStatus.Text = "Scan downloaded but some pages download failed, see download details";
                }
            }
            else // Too many download error
            {
                gridDlStatus.Background = Brushes.DarkRed;
                if(CbzCreationError)
                {
                    txtBlockDlStatus.Text = "Errors while downloading scan pages and creating cbz archive, see download details";
                }
                else
                {
                    txtBlockDlStatus.Text = "Errors while downloading scan pages, see download details";
                }  
            } 
        }

        public void CbzCreationStarted()
        {
            txtBlockDlStatus.Text = "Creating cbz archive...";
        }

        public void CbzCreated()
        {
            string msg = $"Cbz archive for {BookName}-{ChapterId} successfully created ";
            DownloadEventItem pageItem = new DownloadEventItem(msg, Brushes.LightGreen);
            int countBeforeAdd = DownloadEventItems.Count;
            DownloadEventItems.Add(pageItem);
            if (countBeforeAdd == 0) SetShowDetailsBtnVisibility();
        }

        public void CbzAlreadyCreated()
        {
            string msg = $"Cbz archive for {BookName}-{ChapterId} already created";
            DownloadEventItem pageItem = new DownloadEventItem(msg, Brushes.LightGreen);
            int countBeforeAdd = DownloadEventItems.Count;
            DownloadEventItems.Add(pageItem);
            if (countBeforeAdd == 0) SetShowDetailsBtnVisibility();
        }

        public void CbzCreationFailed(string msg, Exception ex)
        {
            CbzCreationError = true;

            string errorMsg = $"{msg} for {BookName}-{ChapterId}: {ex.Message}";
            DownloadEventItem pageItem = new DownloadEventItem(errorMsg, Brushes.Red);
            int countBeforeAdd = DownloadEventItems.Count;
            DownloadEventItems.Add(pageItem);
            if (countBeforeAdd == 0) SetShowDetailsBtnVisibility();
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
