using Newtonsoft.Json;
using System.Diagnostics;

namespace ScanNetDownloader.Logic
{
    public class ScanVfNetScanData : ScanData
    {
        [JsonConstructor] // Only for Json deserialization, apparently passed variable name ABSOLUTELY must the same as its destination value name  (ex: url-> Url, websiteDomain -> WebsiteDomain)
        public ScanVfNetScanData(string url, string websiteDomain, string bookName, int chapterId, bool isSelectedForDownload) : base(url, websiteDomain, bookName, chapterId, isSelectedForDownload)
        {
            Url = url;
            WebsiteDomain = websiteDomain;
            BookName = bookName;
            ChapterId = chapterId;
            IsSelectedForDownload = isSelectedForDownload;
        }

        public ScanVfNetScanData(string url, bool chapterIsInUrl = true) : base(url)
        {
            Url = url;
            WebsiteDomain = "https://www.scan-vf.net/";
            BookName = GetBookNameFromUrl(url);
            if (chapterIsInUrl) ChapterId = int.Parse(GetChapterNumberFromUrl(url));
            else ChapterId = -1;
            IsSelectedForDownload = true;

        }

        public override string GetBookNameFromUrl(string url, bool removeSpace = false)
        {
            #region Chapter and img url examples
            // Example of chapter url
            //   0    1        2           3          4         5
            // https://www.scan-vf.net/one_piece/chapitre-1079/1
            // https://www.scan-vf.net/jujutsu-kaisen/chapitre-268/1
            // https://www.scan-vf.net/dragon-Ball-Super/chapitre-73/1
            // https://www.scan-vf.net/my-hero-academia/chapitre-358/1

            // Example of img url
            //   0    1      2            3      4      5        6         7          8
            // https://www.scan-vf.net/uploads/manga/kingdom/chapters/chapitre-810/01.webp
            // https://www.scan-vf.net/uploads/manga/attaque-des-titans/chapters/chapitre-139.5/01.jpg
            // https://www.scan-vf.net/uploads/manga/one-punch-man/chapters/chapitre-230/mp-01.webp
            // https://www.scan-vf.net/uploads/manga/one_piece/chapters/chapitre-1091/001.webp 
            #endregion

            int splitIdForChapterUrl = 3;
            int splitIdForImgUrl = 5;
            bool isImgUrl = url.Contains(Constants.SCANVF_IMG_URL_MARKER); // Check if we are using a link of a chapter or an image

            string bookName = url.Split(Constants.SLASH_CHAR)[isImgUrl ? splitIdForImgUrl : splitIdForChapterUrl];
            bookName = bookName.ToTitleCase();
            bookName = bookName.Replace(Constants.DASH_CHAR.ToString(), removeSpace ? string.Empty : Constants.SPACE);
            bookName = bookName.Replace(Constants.UNDERSCORE_CHAR.ToString(), removeSpace ? string.Empty : Constants.SPACE);

            return bookName;
        }

        public override string GetChapterNumberFromUrl(string url, bool keepNumberOnly = true)
        {
            #region Chapter and img url examples
            // Example of chapter url
            //   0    1        2           3          4         5
            // https://www.scan-vf.net/one_piece/chapitre-1079/1
            // https://www.scan-vf.net/jujutsu-kaisen/chapitre-268/1
            // https://www.scan-vf.net/dragon-Ball-Super/chapitre-73/1
            // https://www.scan-vf.net/my-hero-academia/chapitre-358/1

            // Example of img url
            //   0    1      2            3      4      5        6         7          8
            // https://www.scan-vf.net/uploads/manga/kingdom/chapters/chapitre-810/01.webp
            // https://www.scan-vf.net/uploads/manga/attaque-des-titans/chapters/chapitre-139.5/01.jpg
            // https://www.scan-vf.net/uploads/manga/one-punch-man/chapters/chapitre-230/mp-01.webp
            // https://www.scan-vf.net/uploads/manga/one_piece/chapters/chapitre-1091/001.webp 
            #endregion

            int splitIdForChapterUrl = 4;
            int splitIdForImgUrl = 7;
            bool isImgUrl = url.Contains(Constants.SCANVF_IMG_URL_MARKER); // Check if we are using a link of a chapter or an image

            string chapterNumber = url.Split(Constants.SLASH_CHAR)[isImgUrl ? splitIdForImgUrl : splitIdForChapterUrl];
            if (keepNumberOnly) chapterNumber = chapterNumber.Split(Constants.DASH_CHAR)[1];

            return chapterNumber;
        }

        public override string GetFileExtensionFromUrl(string url)
        {
            #region Img url examples
            // Example of img url
            //   0    1      2            3      4      5        6         7          8
            // https://www.scan-vf.net/uploads/manga/kingdom/chapters/chapitre-810/01.webp
            // https://www.scan-vf.net/uploads/manga/attaque-des-titans/chapters/chapitre-139.5/01.jpg
            // https://www.scan-vf.net/uploads/manga/one-punch-man/chapters/chapitre-230/mp-01.webp
            // https://www.scan-vf.net/uploads/manga/one_piece/chapters/chapitre-1091/001.webp 
            #endregion

            string fileExtension = url.Split(Constants.SLASH_CHAR)[8];
            fileExtension = "." + fileExtension.Split(Constants.POINT_CHAR)[1];

            return fileExtension;
        }

        protected override List<string> ParseHtmlToGetImgLinks(string htmlContent)
        {
            Debug.WriteLine($"\nPARSE HTML - {BookName}_{ChapterId} ({Url}), imgs found:");

            string[] splitContent = htmlContent.Split(Constants.SCANVF_URL_BLOCK_START_SEPARATOR, StringSplitOptions.RemoveEmptyEntries); // Split before the block with all the img url
            string urlSplit = splitContent[1]; // Keep the split after our separator (trim the beginning)

            splitContent = urlSplit.Split(Constants.SCANVF_URL_BLOCK_END_SEPARATOR, StringSplitOptions.RemoveEmptyEntries); // Split after the block with all the img url
            urlSplit = splitContent[0]; // Keep the split before our separator (trim the end)

            splitContent = urlSplit.Split(Constants.SCANVF_CLEAN_BEFORE_IMG_TAG_SEPARATOR, StringSplitOptions.RemoveEmptyEntries); // Remove the html code that is before the first <img/>
            urlSplit = splitContent[1]; // Keep the split after our separator (trim the beginning)

            List<string> imgUrls = urlSplit.Split(Constants.SCANVF_IMG_TAG_END_SEPARATOR, StringSplitOptions.RemoveEmptyEntries).ToList(); // Split each img tag in a list

            // Keep only the urls in the list
            for (int i = imgUrls.Count - 1; i >= 0; i--)
            {
                if (imgUrls[i].ToLower().Contains(Constants.HTTP_ADDRESS))
                {
                    string[] imgUrlSplit = imgUrls[i].Split([Constants.DOUBLE_QUOTE_CHAR, Constants.SINGLE_QUOTE_CHAR]);
                    foreach (string split in imgUrlSplit)
                    {
                        // Keep only the split containing the url
                        if (split.ToLower().Contains(Constants.HTTP_ADDRESS))
                        {
                            imgUrls[i] = split;
                            imgUrls[i] = imgUrls[i].Replace(" ", string.Empty); // Remove space in the string
                        }
                    }
                }
                else
                {
                    imgUrls.RemoveAt(i);
                }
            }

            imgUrls.Log(); // Debug log of list items
            Debug.WriteLine("\n");

            return imgUrls;
        }

        public static bool IsUrlValid(string url)
        {
            // What are the caracteristics of a valid scanVf url ?
            //https://www.scan-vf.net/jujutsu-kaisen/chapitre-164/1 = url with chapter -> at least 5 split
            //https://www.scan-vf.net/jujutsu-kaisen = url without chapter -> less than 5 split
            // Check if url is long enough to have a book name

            // Try to form a valid img url for chapter ? page 1 ?

            if (UrlLoadCorrectly(url) == false) // Test if we can load url
            {
                return false;
            }
            else
            {
                return true;
            }
        }
    }
}
