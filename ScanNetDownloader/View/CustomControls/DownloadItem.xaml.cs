using ScanNetDownloader.Logic;
using System;
using System.Collections.Generic;
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
            InitializeComponent();

            linkedScanItem = _scanItem;

            BookName = _scanItem.BookName;
            ChapterId = _scanItem.ChapterId;
            PagesCount = _scanItem.PagesCount;
            Url = _scanItem.Url;
            Website = _scanItem.Website;

            txtBlockItemHeader.Text = $"{BookName} - Chapter {ChapterId} ({PagesCount} pages)";

            ShowDownloadDetails(false);
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
    }
}
