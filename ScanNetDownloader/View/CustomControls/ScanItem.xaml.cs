using ScanNetDownloader.Logic;
using System.ComponentModel;
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
                if (linkedScanData != null) linkedScanData.IsSelectedForDownload = value; // TODO: when to save the value in the settings json ? Only when closing app or save everytime value is changed ?
            }
        }

        public ScanData linkedScanData { get; private set; }

        public string BookName { get; private set; }

        public int ChapterId { get; private set; }

        public int PagesCount { get; private set; }

        public string Url { get; private set; }

        public string Website { get; private set; }

        private bool _isDownloaded;
        public bool IsDownloaded
        {
            get { return _isDownloaded; }
            set
            {
                _isDownloaded = value;
                SetDownloadStatus(value);
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

        public ScanItem()
        {
            DataContext = this;
            InitializeComponent();
        }

        public ScanItem(ScanData _scanData, bool fileAlreadyDownloaded=false, bool cbzAlreadyCreated=false)
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
            if (fileDownloaded)
            {
                btnStatus.Content = "ok";
                btnStatus.Background = Brushes.Green;
                btnCbzCreation.IsEnabled = true;
            }
            else
            {
                btnStatus.Content = "∅";
                btnStatus.Background = Brushes.Red;
                btnCbzCreation.IsEnabled = false;
            }
        }

        private void SetCbzCreatedStatus(bool cbzCreated)
        {
            if (cbzCreated)
            {
                btnCbzCreation.Background = Brushes.Green;
            }
            else
            {
                btnCbzCreation.ClearValue(Button.BackgroundProperty);
            }
        }
    }
}
