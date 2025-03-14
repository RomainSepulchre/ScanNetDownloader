using ScanNetDownloader.View.CustomControls;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;

namespace ScanNetDownloader.Logic.Helpers
{
    public static class Extensions
    {
        public static string ToTitleCase(this string title)
        {
            return CultureInfo.CurrentCulture.TextInfo.ToTitleCase(title.ToLower());
        }

        public static bool StartsWithAny(this string s, List<string> listOfStrings)
        {
            bool startWithAny = false;

            foreach (string stringToTest in listOfStrings)
            {
                if (s.StartsWith(stringToTest)) startWithAny = true;
            }

            return startWithAny;
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

        /// <summary>
        /// Count the number of digits in a given int
        /// </summary>
        /// <param name="n">int that calls the method</param>
        /// <returns></returns>
        public static int CountDigits(this int n)
        {
            // if chain is apparently faster than using log10, while loop, or ToString().Count (https://stackoverflow.com/questions/4483886/how-can-i-get-a-count-of-the-total-number-of-digits-in-a-number)
            if (n >= 0)
            {
                if (n < 10) return 1;
                if (n < 100) return 2;
                if (n < 1000) return 3;
                if (n < 10000) return 4;
                if (n < 100000) return 5;
                if (n < 1000000) return 6;
                if (n < 10000000) return 7;
                if (n < 100000000) return 8;
                if (n < 1000000000) return 9;
                return 10;
            }
            else
            {
                if (n > -10) return 2;
                if (n > -100) return 3;
                if (n > -1000) return 4;
                if (n > -10000) return 5;
                if (n > -100000) return 6;
                if (n > -1000000) return 7;
                if (n > -10000000) return 8;
                if (n > -100000000) return 9;
                if (n > -1000000000) return 10;
                return 11;
            }
        }

        /// <summary>
        /// Returns the first left digit of a given int
        /// </summary>
        /// <param name="n">int that calls the method</param>
        /// <returns>first left digit of the int</returns>
        public static int GetFirstDigit(this int n)
        {
            int firstdigit;
            if (n >= 0)
            {
                if (n < 10) firstdigit = n;
                else if (n < 100) firstdigit = n / 10;
                else if (n < 1000) firstdigit = n / 100;
                else if (n < 10000) firstdigit = n / 1000;
                else if (n < 100000) firstdigit = n / 10000;
                else if (n < 1000000) firstdigit = n / 100000;
                else if (n < 10000000) firstdigit = n / 1000000;
                else if (n < 100000000) firstdigit = n / 10000000;
                else if (n < 1000000000) firstdigit = n / 100000000;
                else firstdigit = n / 1000000000;
            }
            else
            {
                if (n < -10) firstdigit = n;
                else if (n < -100) firstdigit = n / 10;
                else if (n < -1000) firstdigit = n / 100;
                else if (n < -10000) firstdigit = n / 1000;
                else if (n < -100000) firstdigit = n / 10000;
                else if (n < -1000000) firstdigit = n / 100000;
                else if (n < -10000000) firstdigit = n / 1000000;
                else if (n < -100000000) firstdigit = n / 10000000;
                else if (n < -1000000000) firstdigit = n / 100000000;
                else firstdigit = n / 1000000000;
            }


                return firstdigit;
        }

        /// <summary>
        /// Returns the most left digits of a given int. The number of most left digits to return is defined by count.
        /// For negative int, - is kept but not considered to be part of the digit count (with a count of 2, -123  will return -12).
        /// </summary>
        /// <param name="n">int that calls the method</param>
        /// <param name="count">number of most left digits to return</param>
        /// <returns>int of count digits with the most left digits of input int</returns>
        public static int GetFirstDigits(this int n, int count)
        {
            int nDigitCount = n.CountDigits();
            if (n < 0) count += 1; // Increase count by one to take into account the - before negative int

            if (count >= nDigitCount) return n;
            else
            {
                int digitDiff = nDigitCount - count;

                return n / (int)Math.Pow(10, digitDiff);
            }
        }
    }
}
