using ScanNetDownloader.View.CustomControls;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Media.Imaging;

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
                case ScanItem.DownloadedStatus.Downloaded:
                    return downloadedText;
                case ScanItem.DownloadedStatus.OnlyImages:
                    return onlyImagesText;
                case ScanItem.DownloadedStatus.OnlyCbz:
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
                return ScanItem.DownloadedStatus.Downloaded;
            }
            else if (img == notDownloadedText)
            {
                return ScanItem.DownloadedStatus.NotDownloaded;
            }
            else if (img == onlyImagesText)
            {
                return ScanItem.DownloadedStatus.OnlyImages;
            }
            else if(img == onlyCbzText)
            {
                return ScanItem.DownloadedStatus.OnlyCbz;
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
