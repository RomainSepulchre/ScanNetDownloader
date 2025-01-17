using ScanNetDownloader.View.CustomControls;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace ScanNetDownloader.Logic.Converters
{
    class DownloadStatusToEllipseVisibilityConverter : IValueConverter
    {
        bool onlyCbz = false;
        bool downloaded = false;
        bool missingImage = false;

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            ScanItem.DownloadedStatus dlStatus = (ScanItem.DownloadedStatus)value;

            switch (dlStatus)
            {
                case ScanItem.DownloadedStatus.NotDownloaded:
                    downloaded = false;
                    missingImage = false;
                    return Visibility.Collapsed;
                case ScanItem.DownloadedStatus.FullyDownloaded:
                    downloaded = true;
                    missingImage = false;
                    return Visibility.Collapsed;
                case ScanItem.DownloadedStatus.MissingImages:
                    downloaded = false;
                    missingImage = true;
                    return Visibility.Collapsed;
                default:
                    return Visibility.Collapsed;
                
                case ScanItem.DownloadedStatus.OnlyImagesDownloaded:
                    onlyCbz = false;
                    return Visibility.Visible;
                case ScanItem.DownloadedStatus.OnlyCbzDownloaded:
                    onlyCbz = true;
                    return Visibility.Visible;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            Visibility visibility = (Visibility)value;

            if (visibility == Visibility.Visible)
            {
                if (onlyCbz == false) return ScanItem.DownloadedStatus.OnlyImagesDownloaded;
                else return ScanItem.DownloadedStatus.OnlyCbzDownloaded;
            }
            else if (visibility == Visibility.Collapsed)
            {
                if(downloaded) return ScanItem.DownloadedStatus.FullyDownloaded;
                else if(missingImage) return ScanItem.DownloadedStatus.MissingImages;
                else return ScanItem.DownloadedStatus.NotDownloaded;
            }
            else
            {
                return ScanItem.DownloadedStatus.NotDownloaded;
            }
        }
    }
}
