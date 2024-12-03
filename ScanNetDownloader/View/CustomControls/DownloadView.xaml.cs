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

        private string _dlInfo;

        public string DlInfo
        {
            get { return _dlInfo; }
            set
            {
                _dlInfo = value;
                scrollVwDownloadInfo?.ScrollToBottom();
                OnPropertyChanged();
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        public DownloadView()
        {
            DownloadItems = new ObservableCollection<DownloadItem>();

            // Download Events
            Downloader.DlInfoWriteLineEvent += new EventHandler<string>(WriteDlInfoLine);
            Downloader.UpdateDlProgressBarEvent += new EventHandler<float>(UpdateDownloadProgress);
            Downloader.OnDownloadStartedEvent += new EventHandler(OnDownloadStarted);
            Downloader.OnScanDownloadedEvent += new EventHandler<ScanItem>(OnScanDownloaded);
            Downloader.OnPageDownloadedEvent += new EventHandler<PageEventArgs>(OnPageDownloaded);
            Downloader.OnPageDownloadErrorEvent += new EventHandler<PageErrorEventArgs>(OnPageDownloadError);
            Downloader.OnPageFileSavingErrorEvent += new EventHandler<PageErrorEventArgs>(OnPageFileSavingError);
            CbzCreator.OnCbzCreationStartEvent += new EventHandler<ScanItem>(OnCbzCreationStart);
            CbzCreator.OnCbzCreatedEvent += new EventHandler<ScanItem>(OnCbzCreated);
            CbzCreator.OnCbzCreationFailedErrorEvent += new EventHandler<CbzErrorEventArgs>(OnCbzCreationFailedError);

            InitializeComponent();

            DataContext = this;

#if !DEBUG
            scrollVwDownloadInfo.Visibility = Visibility.Collapsed;
            gridDlInfo.RowDefinitions[0].Height = new GridLength(0);
            gridDlInfo.RowDefinitions[1].Height = new GridLength(0);
#endif
        }

        private void OnPropertyChanged([CallerMemberName] string property = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(property));
        }

        #region Events Handler
        public void WriteDlInfoLine(object sender, string lineToAdd)
        {
            DlInfo += $"{lineToAdd}\n";
        }

        public void UpdateDownloadProgress(object sender, float percentageDone)
        {
            // TODO: Add bindings ?
            progrBarDownload.Value = percentageDone;
        }

        public void OnDownloadStarted(object sender, EventArgs args)
        {
            foreach (var item in DownloadItems)
            {
                item.PrepareForDownload();
            }
        }

        public void OnPageDownloaded(object sender, PageEventArgs args)
        {
            Debug.WriteLine($"Page {args.PageIndex} of {args.ScanItem.BookName}-{args.ScanItem.ChapterId} downloaded.");
            ScanItem scanItem = args.ScanItem;
            int pageNumber = args.PageIndex + 1;

            DownloadItem dlItem = DownloadItems.First(x => x.linkedScanItem == scanItem);

            dlItem.PageDownloaded(pageNumber);
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

        public void OnCbzCreationFailedError(object sender, CbzErrorEventArgs args)
        {
            ScanItem cbzScan = args.ScanItem;
            DownloadItem dlItem = DownloadItems.First(x => x.linkedScanItem == cbzScan);
            dlItem.CbzCreationFailed(args.ErrorMessage, args.Exception);
        }

        #endregion

        public void StartDownload(List<ScanItem> scanItemsToDownload)
        {
            DlInfo = ""; // Clear download info
            RefreshDownloadView(scanItemsToDownload);

            Downloader.StartDownloader(scanItemsToDownload);
        }

        public void RefreshDownloadView(List<ScanItem> scanItemsToDownload)
        {
            DownloadItems.Clear();
            //TextBlock txtView = new TextBlock();
            //Binding txtBinding = new Binding(nameof(DlInfo));
            //txtBinding.Source = this;
            //txtView.SetBinding(TextBlock.TextProperty, txtBinding);
            //txtView.Height = 300;
            //txtView.Background = Brushes.Aquamarine;
            //txtView.Margin = new Thickness(5);
            //stPanelDownloadInfo.Children.Add(txtView);

            foreach (var item in scanItemsToDownload)
            {
                DownloadItem downloadItem = new DownloadItem(item);
                downloadItem.Margin = new Thickness(0, 0, 0, 3);
                DownloadItems.Add(downloadItem);
            }
        }
    }
}
