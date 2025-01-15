using ScanNetDownloader.Logic.Helpers;
using ScanNetDownloader.View.CustomControls;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Markup;
using System.Windows.Media.Imaging;

namespace ScanNetDownloader.Logic.Converters
{
    class DownloadStatusToDownloadIconConverter : IValueConverter
    {
        BitmapImage downloadedIcon = (BitmapImage)Application.Current.FindResource(ResourcesKey.BitmapImages.DownloadBlack);
        BitmapImage notDownloadedIcon = (BitmapImage)Application.Current.FindResource(ResourcesKey.BitmapImages.DownloadGrey);
        BitmapImage missingImageIcon = (BitmapImage)Application.Current.FindResource(ResourcesKey.BitmapImages.DownloadOrange);
        BitmapImage errorIcon = (BitmapImage)Application.Current.FindResource(ResourcesKey.BitmapImages.Error);

        bool partialDl = false;
        bool isOnlyCbz = false;


        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            ScanItem.DownloadedStatus dlStatus = (ScanItem.DownloadedStatus)value;

            switch (dlStatus)
            {
                case ScanItem.DownloadedStatus.NotDownloaded:               
                    return notDownloadedIcon;
                case ScanItem.DownloadedStatus.Downloaded:
                    partialDl = false;
                    return downloadedIcon;
                case ScanItem.DownloadedStatus.OnlyImages:
                    partialDl = true;
                    isOnlyCbz = false;
                    return downloadedIcon;
                case ScanItem.DownloadedStatus.OnlyCbz:
                    partialDl = true;
                    isOnlyCbz = true;
                    return downloadedIcon;
                case ScanItem.DownloadedStatus.MissingImages:          
                    return missingImageIcon;
                default:
                    return errorIcon;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            BitmapImage img = (BitmapImage)value;

            if (img == downloadedIcon)
            {
                if(partialDl == false) return ScanItem.DownloadedStatus.Downloaded;
                else
                {
                    if (isOnlyCbz == false) return ScanItem.DownloadedStatus.OnlyImages;
                    else return ScanItem.DownloadedStatus.OnlyCbz;
                }
            }
            else if (img == notDownloadedIcon)
            {
                return ScanItem.DownloadedStatus.NotDownloaded;
            }
            else if (img == missingImageIcon)
            {
                return ScanItem.DownloadedStatus.MissingImages;
            }
            else if (img == errorIcon)
            {
                return ScanItem.DownloadedStatus.NotDownloaded;
            }
            else return ScanItem.DownloadedStatus.NotDownloaded;
        }
    }
}
