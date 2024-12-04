using ScanNetDownloader.Logic;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
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
    /// Logique d'interaction pour DownloadView.xaml
    /// </summary>
    public partial class DownloadView : UserControl, INotifyPropertyChanged
    {
        private ObservableCollection<DownloadItem> _downloadItems;

        public ObservableCollection<DownloadItem> DownloadItems
        {
            get { return _downloadItems; }
            set { _downloadItems = value; }
        }

        private float _downloadProgress;
        public float DownloadProgress // 0 to 100
        {
            get { return _downloadProgress; }
            set {
                _downloadProgress = value;
                OnPropertyChanged();
            }
        }

        private string _downloadLabelTxt;
        public string DownloadLabelTxt
        {
            get { return _downloadLabelTxt; }
            set {
                _downloadLabelTxt = value;
                OnPropertyChanged();
            }
        }


        public bool IsDownloading { get; private set; } = false;


        public event PropertyChangedEventHandler? PropertyChanged;

        public DownloadView()
        {
            DownloadItems = new ObservableCollection<DownloadItem>();

            // Download Events
            Downloader.UpdateDlProgressBarEvent += new EventHandler<float>(UpdateDownloadProgress);
            Downloader.OnDownloadStartedEvent += new EventHandler(OnDownloadStarted);
            Downloader.OnDownloadStoppedEvent += new EventHandler(OnDownloadStopped);
            Downloader.OnScanDownloadedEvent += new EventHandler<ScanItem>(OnScanDownloaded);
            Downloader.OnScanDownloadErrorEvent += new EventHandler<ScanErrorEventArgs>(OnScanDownloadError);
            Downloader.OnPageDownloadedEvent += new EventHandler<PageEventArgs>(OnPageDownloaded);
            Downloader.OnPageAlreadyDownloadedEvent += new EventHandler<PageEventArgs>(OnPageAlreadyDownloaded);
            Downloader.OnPageDownloadErrorEvent += new EventHandler<PageErrorEventArgs>(OnPageDownloadError);
            Downloader.OnPageFileSavingErrorEvent += new EventHandler<PageErrorEventArgs>(OnPageFileSavingError);
            Downloader.OnDownloadsFinishedEvent += new EventHandler(OnDownloadsFinished);
            CbzCreator.OnCbzCreationStartEvent += new EventHandler<ScanItem>(OnCbzCreationStart);
            CbzCreator.OnCbzCreatedEvent += new EventHandler<ScanItem>(OnCbzCreated);
            CbzCreator.OnCbzAlreadyCreatedEvent += new EventHandler<ScanItem>(OnCbzAlreadyCreated);
            CbzCreator.OnCbzCreationErrorEvent += new EventHandler<CbzErrorEventArgs>(OnCbzCreationError);


            InitializeComponent();

            DataContext = this;
        }

        private void OnPropertyChanged([CallerMemberName] string property = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(property));
        }

        public static RoutedEvent OnDownloadCompletedEvent = EventManager.RegisterRoutedEvent(nameof(OnDownloadCompleted), RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(ScanItem));

        public event RoutedEventHandler OnDownloadCompleted
        {
            add { AddHandler(OnDownloadCompletedEvent, value); }
            remove { RemoveHandler(OnDownloadCompletedEvent, value); }
        }

        public void StartDownload(List<ScanItem> scanItemsToDownload)
        {
            if (IsDownloading == false)
            {
                RefreshDownloadView(scanItemsToDownload);
                Downloader.StartDownloader(scanItemsToDownload);
            }
        }

        public void RefreshDownloadView(List<ScanItem> scanItemsToDownload)
        {
            if (IsDownloading == false)
            {
                DownloadItems.Clear();

                if (scanItemsToDownload.Count == 0)
                {
                    txtBlockNoScanSelected.Visibility = Visibility.Visible;
                    // TODO: Disable Start button
                }
                else
                {
                    txtBlockNoScanSelected.Visibility = Visibility.Collapsed;
                    // TODO: Enable Start button

                    foreach (var item in scanItemsToDownload)
                    {
                        DownloadItem downloadItem = new DownloadItem(item);
                        downloadItem.Margin = new Thickness(0, 0, 0, 3);
                        DownloadItems.Add(downloadItem);
                    }
                }

                DownloadProgress = 0;
                DownloadLabelTxt = "Ready...";
            }
        }

        #region Events Handler
        public void UpdateDownloadProgress(object sender, float percentageDone)
        {
            DownloadProgress = percentageDone;
        }

        public void OnDownloadStarted(object sender, EventArgs args)
        {
            IsDownloading = true;
            DownloadLabelTxt = "Downloading...";

            foreach (var item in DownloadItems)
            {
                item.PrepareForDownload();
            }
        }

        public void OnDownloadStopped(object sender, EventArgs args)
        {
            // TODO: Reset view to after download has been stopped
            Debug.WriteLine("DOWNLOAD STOPPED");
            IsDownloading = false;
            DownloadProgress = 0;
            DownloadLabelTxt = "Download stopped";
        }

        public void OnPageDownloaded(object sender, PageEventArgs args)
        {
            Debug.WriteLine($"Page {args.PageIndex} of {args.ScanItem.BookName}-{args.ScanItem.ChapterId} downloaded.");
            ScanItem scanItem = args.ScanItem;
            int pageNumber = args.PageIndex + 1;

            DownloadItem dlItem = DownloadItems.First(x => x.linkedScanItem == scanItem);

            dlItem.PageDownloaded(pageNumber);
        }

        public void OnPageAlreadyDownloaded(object sender, PageEventArgs args)
        {
            Debug.WriteLine($"Page {args.PageIndex} of {args.ScanItem.BookName}-{args.ScanItem.ChapterId} already downloaded.");

            ScanItem scanItem = args.ScanItem;
            int pageNumber = args.PageIndex + 1;

            DownloadItem dlItem = DownloadItems.First(x => x.linkedScanItem == scanItem);

            dlItem.PageAlreadyDownloaded(pageNumber);
        }

        public void OnScanDownloaded(object sender, ScanItem downloadedScan) // This happens when the Scan download finish
        {
            downloadedScan.IsSelectedForDownload = false; // Disable download selection since we just downloaded

            bool fileSuccessfullyDownloaded = FileManagement.AreScanFilesDownloaded(downloadedScan.linkedScanData);
            bool cbzCreated = FileManagement.IsCbzArchiveCreated(downloadedScan.linkedScanData);
            downloadedScan.IsDownloaded = fileSuccessfullyDownloaded;
            downloadedScan.CbzArchiveCreated = cbzCreated;

            DownloadItem dlItem = DownloadItems.First(x => x.linkedScanItem == downloadedScan);
            dlItem.ScanDownloaded();
        }

        public void OnScanDownloadError(object sender, ScanErrorEventArgs args)
        {
            ScanItem scanWithError = args.ScanItem;
            DownloadItem dlItem = DownloadItems.First(x => x.linkedScanItem == scanWithError);
            dlItem.ScanDownloadError(args.Exception);    
        }

        public void OnPageDownloadError(object sender, PageErrorEventArgs args)
        {
            Debug.WriteLine($"Error while downloading page {args.PageIndex} of {args.ScanItem.BookName}-{args.ScanItem.ChapterId}");
            ScanItem scanItem = args.ScanItem;
            int pageNumber = args.PageIndex + 1;

            DownloadItem dlItem = DownloadItems.First(x => x.linkedScanItem == scanItem);

            dlItem.PageDownloadError(pageNumber, args.Exception);
        }

        public void OnPageFileSavingError(object sender, PageErrorEventArgs args)
        {
            Debug.WriteLine($"Error while saving file for page {args.PageIndex} of {args.ScanItem.BookName}-{args.ScanItem.ChapterId}");
            ScanItem scanItem = args.ScanItem;
            int pageNumber = args.PageIndex + 1;

            DownloadItem dlItem = DownloadItems.First(x => x.linkedScanItem == scanItem);

            dlItem.PageFileSavingError(pageNumber, args.Exception);
        }

        public void OnDownloadsFinished(object sender, EventArgs args)
        {
            IsDownloading = false;
            DownloadLabelTxt = "Download finished";
            RaiseEvent(new RoutedEventArgs(OnDownloadCompletedEvent, this));
        }

        public void OnCbzCreationStart(object sender, ScanItem cbzScan)
        {
            DownloadItem dlItem = DownloadItems.First(x => x.linkedScanItem == cbzScan);
            dlItem.CbzCreationStarted();
        }

        public void OnCbzCreated(object sender, ScanItem cbzScan)
        {
            DownloadItem dlItem = DownloadItems.First(x => x.linkedScanItem == cbzScan);
            dlItem.CbzCreated();
        }

        public void OnCbzAlreadyCreated(object sender, ScanItem cbzScan)
        {
            DownloadItem dlItem = DownloadItems.First(x => x.linkedScanItem == cbzScan);
            dlItem.CbzAlreadyCreated();
        }

        public void OnCbzCreationError(object sender, CbzErrorEventArgs args)
        {
            ScanItem cbzScan = args.ScanItem;
            DownloadItem dlItem = DownloadItems.First(x => x.linkedScanItem == cbzScan);
            dlItem.CbzCreationFailed(args.ErrorMessage, args.Exception);
        }

        #endregion

        
    }
}
