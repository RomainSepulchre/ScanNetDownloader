using ScanNetDownloader.Logic;
using ScanNetDownloader.Logic.Helpers;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using static ScanNetDownloader.View.CustomControls.ScanItem;

namespace ScanNetDownloader.View.CustomControls
{
    /// <summary>
    /// Logique d'interaction pour ScanItem.xaml
    /// </summary>
    public partial class ScanItem : UserControl, INotifyPropertyChanged
    {
        private bool _isSelectedForDownload;

        public bool IsSelectedForDownload
        {
            get { return _isSelectedForDownload; }
            set {
                _isSelectedForDownload = value;
                OnPropertyChanged();
                if (linkedScanData != null)
                {
                    linkedScanData.IsSelectedForDownload = value; // TODO: when to save the value in the settings json ? Only when closing app or save everytime value is changed ?
                    RaiseEvent(new RoutedEventArgs(IsSelectedModifiedEvent, this));
                }
            }
        }

        public ScanData linkedScanData { get; private set; }

        public string BookName { get; private set; }

        public int ChapterId { get; private set; }

        public int PagesCount { get; private set; }

        public string Url { get; private set; }

        public string Website { get; private set; }

        private bool _isDownloaded;
        public bool IsDownloaded // TODO: Clean this or link it to DownloadedStatus ? Is it still used ?
        {
            get { return _isDownloaded; }
            set
            {
                _isDownloaded = value;
                SetDownloadStatus(value);
                OnPropertyChanged();
            }
        }

        public enum DownloadedStatus
        {
            NotDownloaded = 0,
            Downloaded = 1,   
            OnlyImages = 2,
            OnlyCbz = 3,
            MissingImages = 4
        }
        private DownloadedStatus _downloadStatus;
        public DownloadedStatus DownloadStatus
        {
            get { return _downloadStatus; }
            set {
                _downloadStatus = value;
                OnPropertyChanged();
            }
        }


        private bool _cbzArchiveCreated;
        public bool CbzArchiveCreated
        {
            get { return _cbzArchiveCreated; }
            set
            {
                _cbzArchiveCreated = value;
                SetCbzCreatedStatus(value);
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        public static RoutedEvent DeleteBtnPressedEvent = EventManager.RegisterRoutedEvent(nameof(DeleteScanBtnPressed), RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(ScanItem));

        public event RoutedEventHandler DeleteScanBtnPressed
        {
            add { AddHandler(DeleteBtnPressedEvent, value); }
            remove { RemoveHandler(DeleteBtnPressedEvent, value); }
        }

        public static RoutedEvent CreateCbzBtnPressedEvent = EventManager.RegisterRoutedEvent(nameof(CreateCbzBtnPressed), RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(ScanItem));

        public event RoutedEventHandler CreateCbzBtnPressed
        {
            add { AddHandler(CreateCbzBtnPressedEvent, value); }
            remove { RemoveHandler(CreateCbzBtnPressedEvent, value); }
        }

        public static RoutedEvent StatusBtnPressedEvent = EventManager.RegisterRoutedEvent(nameof(StatusBtnPressed), RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(ScanItem));

        public event RoutedEventHandler StatusBtnPressed
        {
            add { AddHandler(StatusBtnPressedEvent, value); }
            remove { RemoveHandler(StatusBtnPressedEvent, value); }
        }

        public static RoutedEvent IsSelectedModifiedEvent = EventManager.RegisterRoutedEvent(nameof(IsSelectedModified), RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(ScanItem));

        public event RoutedEventHandler IsSelectedModified
        {
            add { AddHandler(IsSelectedModifiedEvent, value); }
            remove { RemoveHandler(IsSelectedModifiedEvent, value); }
        }

        public ScanItem()
        {
            DataContext = this;
            InitializeComponent();
        }

        public ScanItem(ScanData _scanData, bool fileAlreadyDownloaded=false, bool cbzAlreadyCreated=false, DownloadedStatus dlStatus=DownloadedStatus.NotDownloaded)
        {
            DataContext = this;
            
            InitializeComponent();

            linkedScanData = _scanData;
            BookName = _scanData.BookName;
            ChapterId = _scanData.ChapterId;
            PagesCount = _scanData.PagesCount;
            Url = _scanData.Url;
            Website = _scanData.WebsiteDomain;
            IsDownloaded = fileAlreadyDownloaded;
            CbzArchiveCreated = cbzAlreadyCreated;
            DownloadStatus = dlStatus;
            IsSelectedForDownload = _scanData.IsSelectedForDownload;           

            //lbBookName.Content = BookName;
            //lbChapterNumber.Content = $"{ChapterId} - {linkedScanData.PagesCount} pages";
            //lbWebsite.Content = Website;
        }

        private void OnPropertyChanged([CallerMemberName] string property = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(property));
        }

        private void btnStatus_Click(object sender, RoutedEventArgs e)
        {
            RaiseEvent(new RoutedEventArgs(StatusBtnPressedEvent, this));
        }

        private void btnCbzCreation_Click(object sender, RoutedEventArgs e)
        {
            RaiseEvent(new RoutedEventArgs(CreateCbzBtnPressedEvent, this));
        }

        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            RaiseEvent(new RoutedEventArgs(DeleteBtnPressedEvent, this));
        }

        private void SetDownloadStatus(bool fileDownloaded)
        {
            if (fileDownloaded && CbzArchiveCreated == false)
            {
                btnCbzCreation.IsEnabled = true;
            }
            else
            {
                btnCbzCreation.IsEnabled = false;
            }
        }

        private void SetCbzCreatedStatus(bool cbzCreated)
        {
            if (cbzCreated)
            {
                if(IsDownloaded) btnCbzCreation.IsEnabled = false;
                btnCbzCreation.Content = ".CBZ created";
                btnCbzCreation.Foreground = (SolidColorBrush)FindResource(ResourcesKey.ColorBrushes.White);
                btnCbzCreation.Background = (SolidColorBrush)FindResource(ResourcesKey.ColorBrushes.Green);
            }
            else
            {
                btnCbzCreation.Content = "Create .CBZ";
                btnCbzCreation.ClearValue(Button.BackgroundProperty);
            }
        }

        public static bool DownloadStatusToIsDownloaded(DownloadedStatus dlStatus)
        {
            switch (dlStatus)
            {
                case DownloadedStatus.NotDownloaded:
                default:
                    return false;

                case DownloadedStatus.Downloaded:
                case DownloadedStatus.MissingImages:
                case DownloadedStatus.OnlyCbz:
                case DownloadedStatus.OnlyImages:
                    return true;

            }
        }
    }
}
