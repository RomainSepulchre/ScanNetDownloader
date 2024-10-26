using System.Diagnostics;
using System.Globalization;

namespace ScanNetDownloader.ConsoleApp
{
    public static class Extensions
    {
        public static string ToTitleCase(this string title)
        {
            return CultureInfo.CurrentCulture.TextInfo.ToTitleCase(title.ToLower());
        }

        public static void Log<T>(this List<T> list)
        {
            foreach (T item in list)
            {
                Debug.WriteLine($"- {item}");
            }
        }
    }
}
