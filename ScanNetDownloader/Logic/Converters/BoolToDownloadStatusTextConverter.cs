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
    class BoolToDownloadStatusTextConverter : IValueConverter
    {
        string downloadedText = "Downloaded";
        string notDownloadedText = "Not downloaded";

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool downloaded = System.Convert.ToBoolean(value);
            if (downloaded)
            {
                return downloadedText;
            }
            else
            {
                return notDownloadedText;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value as string == downloadedText)
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
