using ScanNetDownloader.ConsoleApp;
using System;
using System.Collections.Generic;
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
                Debug.WriteLine($"IS SELECTED FOR DOWNLOAD CHANGED FOR {BookName}-{ChapterId}, new value = {value}");
                if (linkedScanWebsiteUrl != null) linkedScanWebsiteUrl.IsSelectedForDownload = value; // TODO: when to save the value in the settings json ? Only when closing app or save everytime value is changed ?
            }
        }

        public ScanWebsiteUrl linkedScanWebsiteUrl { get; private set; }

        public string BookName { get; private set; }

        public int ChapterId { get; private set; }

        public string Url { get; private set; }

        public string Website { get; private set; }

        public bool IsDownloaded { get; private set; }

        public bool cbzArchiveCreated { get; private set; }

        public event PropertyChangedEventHandler? PropertyChanged;

        public static RoutedEvent DeleteBtnPressedEvent = EventManager.RegisterRoutedEvent(nameof(DeleteBtnPressed), RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(ScanItem));

        public event RoutedEventHandler DeleteBtnPressed
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

        public ScanItem(ScanWebsiteUrl _scanWebsiteUrl)
        {
            DataContext = this;
            
            linkedScanWebsiteUrl = _scanWebsiteUrl;

            BookName = _scanWebsiteUrl.BookName;
            ChapterId = _scanWebsiteUrl.ChapterId;
            Url = _scanWebsiteUrl.Url;
            Website = _scanWebsiteUrl.WebsiteDomain;
            IsDownloaded = false;
            cbzArchiveCreated = false;
            IsSelectedForDownload = _scanWebsiteUrl.IsSelectedForDownload;

            InitializeComponent();

            bookNameLb.Content = BookName;
            chapterNumberLb.Content = ChapterId;
            websiteLb.Content = Website;
        }

        private void OnPropertyChanged([CallerMemberName] string property = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(property));
        }

        private void statusBtn_Click(object sender, RoutedEventArgs e)
        {
            RaiseEvent(new RoutedEventArgs(StatusBtnPressedEvent, this));
        }

        private void cbzCreationBtn_Click(object sender, RoutedEventArgs e)
        {
            RaiseEvent(new RoutedEventArgs(CreateCbzBtnPressedEvent, this));
        }

        private void deleteBtn_Click(object sender, RoutedEventArgs e)
        {
            RaiseEvent(new RoutedEventArgs(DeleteBtnPressedEvent, this));
        }
    }
}
