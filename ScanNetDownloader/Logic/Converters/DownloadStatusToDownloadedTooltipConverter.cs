using ScanNetDownloader.View.CustomControls;
using System.Globalization;
using System.Windows.Data;

namespace ScanNetDownloader.Logic.Converters
{
    class DownloadStatusToDownloadedTooltipConverter : IValueConverter
    {
        string downloadedText = "Downloaded";
        string notDownloadedText = "Not downloaded";
        string onlyImagesText = "Images downloaded, no .CBZ created";
        string onlyCbzText = ".CBZ created, images deleted";
        string missingImagesText = "Some images are missing";
        string errorText = "Unknown download status, an error happened";

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            ScanItem.DownloadedStatus dlStatus = (ScanItem.DownloadedStatus)value ;

            switch (dlStatus)
            {
                case ScanItem.DownloadedStatus.NotDownloaded:
                    return notDownloadedText;
                case ScanItem.DownloadedStatus.FullyDownloaded:
                    return downloadedText;
                case ScanItem.DownloadedStatus.OnlyImagesDownloaded:
                    return onlyImagesText;
                case ScanItem.DownloadedStatus.OnlyCbzDownloaded:
                    return onlyCbzText;
                case ScanItem.DownloadedStatus.MissingImages:
                    return missingImagesText;
                default:
                    return errorText;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string img = (string)value;

            if (img == downloadedText)
            {
                return ScanItem.DownloadedStatus.FullyDownloaded;
            }
            else if (img == notDownloadedText)
            {
                return ScanItem.DownloadedStatus.NotDownloaded;
            }
            else if (img == onlyImagesText)
            {
                return ScanItem.DownloadedStatus.OnlyImagesDownloaded;
            }
            else if(img == onlyCbzText)
            {
                return ScanItem.DownloadedStatus.OnlyCbzDownloaded;
            }
            else if (img == missingImagesText)
            {
                return ScanItem.DownloadedStatus.MissingImages;
            }
            else if (img == errorText)
            {
                return ScanItem.DownloadedStatus.NotDownloaded;
            }
            else return ScanItem.DownloadedStatus.NotDownloaded;
        }
    }
}
