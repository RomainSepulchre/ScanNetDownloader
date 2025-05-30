using ScanNetDownloader.Logic;
using ScanNetDownloader.Logic.Helpers;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

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

        public bool IsDownloaded
        {
            get { return DownloadStatusToIsDownloaded(DownloadStatus); }
        }

        public bool IsSearchPerfectMatch { get; set; }

        public enum DownloadedStatus
        {
            NotDownloaded = 0,
            FullyDownloaded = 1, // Images + Cbz 
            OnlyImagesDownloaded = 2, // Images 
            OnlyCbzDownloaded = 3, // Cbz
            MissingImages = 4
        }
        private DownloadedStatus _downloadStatus;
        public DownloadedStatus DownloadStatus
        {
            get { return _downloadStatus; }
            set {
                _downloadStatus = value;
                OnPropertyChanged();
                SetDownloadStatus(_downloadStatus);
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

        public ScanItem(ScanData _scanData, bool cbzAlreadyCreated=false, DownloadedStatus dlStatus=DownloadedStatus.NotDownloaded)
        {
            DataContext = this;
            
            InitializeComponent();

            linkedScanData = _scanData;
            BookName = _scanData.BookName;
            ChapterId = _scanData.ChapterId;
            PagesCount = _scanData.PagesCount;
            Url = _scanData.Url;
            Website = _scanData.WebsiteDomain;
            DownloadStatus = dlStatus;
            CbzArchiveCreated = cbzAlreadyCreated; 
            IsSelectedForDownload = _scanData.IsSelectedForDownload;           
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

        private void SetDownloadStatus(DownloadedStatus status)
        {
            switch (status)
            {
                case DownloadedStatus.NotDownloaded:
                case DownloadedStatus.FullyDownloaded:
                case DownloadedStatus.OnlyCbzDownloaded:
                default:
                    btnCbzCreation.IsEnabled = false;
                    break;
                case DownloadedStatus.OnlyImagesDownloaded:
                case DownloadedStatus.MissingImages:
                    btnCbzCreation.IsEnabled = true;
                    break;
            }     
        }

        private void SetCbzCreatedStatus(bool cbzCreated)
        {
            if (cbzCreated)
            {
                if(DownloadStatus == DownloadedStatus.MissingImages)
                {
                    btnCbzCreation.IsEnabled = true;
                    btnCbzCreation.Content = ".CBZ created";
                    btnCbzCreation.ToolTip = "CBZ created with missing images";
                    // TODO: Create style for this
                    btnCbzCreation.Style = (Style)FindResource(ResourcesKey.Style.ButtonWithWarning);
                    //btnCbzCreation.Foreground = (SolidColorBrush)FindResource(ResourcesKey.ColorBrushes.White);
                    //btnCbzCreation.Background = (SolidColorBrush)FindResource(ResourcesKey.ColorBrushes.Orange);
                }
                else
                {
                    if (btnCbzCreation.IsEnabled) btnCbzCreation.IsEnabled = false;
                    btnCbzCreation.Content = ".CBZ created";
                    btnCbzCreation.ClearValue(Button.ToolTipProperty);
                    btnCbzCreation.Style = (Style)FindResource(ResourcesKey.Style.ComplexButton);
                    btnCbzCreation.Foreground = (SolidColorBrush)FindResource(ResourcesKey.ColorBrushes.White);
                    btnCbzCreation.Background = (SolidColorBrush)FindResource(ResourcesKey.ColorBrushes.Green);
                }
            }
            else
            {
                btnCbzCreation.Content = "Create .CBZ";
                btnCbzCreation.Style = (Style)FindResource(ResourcesKey.Style.ComplexButton);
                btnCbzCreation.ClearValue(Button.ToolTipProperty);
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

                case DownloadedStatus.FullyDownloaded:
                case DownloadedStatus.MissingImages:
                case DownloadedStatus.OnlyCbzDownloaded:
                case DownloadedStatus.OnlyImagesDownloaded:
                    return true;
            }
        }

        #region Debug
        private void btnDebugLocation_Click(object sender, RoutedEventArgs e)
        {
            bool locIsNull = linkedScanData.LocationPath == null;
            string loc = locIsNull ? "Is null" : linkedScanData.LocationPath;
            Debug.WriteLine($"{BookName} - {ChapterId}: location = \"{loc}\"");

            //if(locIsNull)
            //{
            //    string newLoc = "I'm am the path who loc";
            //    linkedScanData.LocationPath = newLoc;
            //    Debug.WriteLine($"{BookName} - {ChapterId}: Set new location = {newLoc}");
            //    ScansLocalData.Save();
            //}
        }
        #endregion
    }
}
