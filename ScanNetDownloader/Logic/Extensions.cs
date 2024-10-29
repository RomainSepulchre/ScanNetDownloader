using System.Diagnostics;
using System.Globalization;

namespace ScanNetDownloader.Logic
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

        public static bool MoreThanOneBookInList(this List<ScanData> scanDataList)
        {
            if (scanDataList.Count <= 1) return false;

            string firstUrlBookName = scanDataList[0].BookName;
            for (int i = 1; i < scanDataList.Count; i++) // Start at item 1 because we always compare with item 0
            {
                string currentUrlBookName = scanDataList[i].BookName;
                if (string.Equals(firstUrlBookName, currentUrlBookName) == false)
                {
                    return true;
                }
            }

            return false;
        }
    }
}
