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
                linkedScanWebsiteUrl.IsSelectedForDownload = value; // TODO: when to save the value in the settings json ? Only when closing app or save everytime value is changed ?
            }
        }

        public int ItemId { get; private set; }

        public ScanWebsiteUrl linkedScanWebsiteUrl { get; private set; }

        public string BookName { get; private set; }

        public int ChapterId { get; private set; }

        public string Url { get; private set; }

        public string Website { get; private set; }

        public bool IsDownloaded { get; private set; }

        public bool cbzArchiveCreated { get; private set; }

        public event PropertyChangedEventHandler? PropertyChanged;

        public ScanItem()
        {
            DataContext = this;
            InitializeComponent();
        }

        public ScanItem(int _itemId, ScanWebsiteUrl _scanWebsiteUrl)
        {
            DataContext = this;
            
            ItemId = _itemId;
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
    }
}
