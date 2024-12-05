using ScanNetDownloader.View.CustomControls;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using System.Windows.Documents;

namespace ScanNetDownloader.Logic.Helpers
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

        public static bool IsAnIncreasingSuite(this List<int> intList, out int breakIndex)
        {
            breakIndex = intList.Count-1;
            for (int i = 0; i < intList.Count - 1; i++) // Count-1 because we don't need to test last item
            {
                if (intList[i] + 1 != intList[i+1])
                {
                    breakIndex = i;
                    return false;
                }
            }
            return true;
        }

        public static int GetTotalOfScanPages(this List<ScanItem> scanItemList)
        {
            int pageTotal = 0;

            foreach (ScanItem scanItem in scanItemList)
            {
                pageTotal += scanItem.PagesCount;
            }

            return pageTotal;
        }

        public static int GetTotalOfScanPages(this List<ScanData> scanList)
        {
            int pageTotal = 0;

            foreach (ScanData scanData in scanList)
            {
                if (scanData.PagesUrl != null) pageTotal += scanData.PagesCount;
            }

            return pageTotal;
        }

        public static List<ScanData> GetScansToDownload(this ObservableCollection<ScanItem> scanItems)
        {
            List<ScanData> scansToDownload = new List<ScanData>();
            foreach (ScanItem item in scanItems)
            {
                if (item.IsSelectedForDownload) scansToDownload.Add(item.linkedScanData);
            }

            return scansToDownload;
        }

        public static List<ScanItem> GetScanItemsSelectedForDownload(this ObservableCollection<ScanItem> scanItems)
        {
            List<ScanItem> scanItemsToDownload = new List<ScanItem>();
            foreach (ScanItem item in scanItems)
            {
                if (item.IsSelectedForDownload) scanItemsToDownload.Add(item);
            }

            return scanItemsToDownload;
        }

        public static List<ScanData> ToScanDataList (this List<ScanItem> scanItems)
        {
            List<ScanData> scansToDownload = new List<ScanData>();
            foreach(ScanItem item in scanItems)
            {
                scansToDownload.Add(item.linkedScanData);
            }
            return scansToDownload;
        }
    }
}
