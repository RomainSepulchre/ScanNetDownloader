using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScanNetDownloader
{
    public static class Extensions
    {
        public static string ToTitleCase(this string title)
        {
            return CultureInfo.CurrentCulture.TextInfo.ToTitleCase(title.ToLower());
        }

        public static void Log<T>(this List<T> list)
        {
            Debug.WriteLine($"\n---- {nameof(list)} ----");
            foreach (T item in list)
            {
                Debug.WriteLine($"{item}");
            }
            Debug.WriteLine($"\n---- {nameof(list)} END ----");
        }
    }
}
