using ScanNetDownloader.Logic.Helpers;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media.Imaging;

namespace ScanNetDownloader.Logic.Converters
{
    class BoolToDownloadedIconConverter : IValueConverter
    {
        BitmapImage downloadedImg = (BitmapImage)Application.Current.FindResource(ResourcesKey.BitmapImages.DownloadBlack);
        BitmapImage notDownloadedImg = (BitmapImage)Application.Current.FindResource(ResourcesKey.BitmapImages.DownloadGrey);

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool downloaded = System.Convert.ToBoolean(value);
            if (downloaded)
            {
                return downloadedImg;
            }
            else
            {
                return notDownloadedImg;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value as BitmapImage == downloadedImg)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
