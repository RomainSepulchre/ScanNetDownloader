using Newtonsoft.Json;
using System.Diagnostics;
using System.Net.Http;

namespace ScanNetDownloader.Logic
{
    // TODO: How to manage scan in english for chapter and image url generation
    public class AnimeSamaFrScanData: ScanData
    {
        public string DownloadUrlBookName { get; private set; }

        /// <summary>
        /// This constructor only purpose is for Json deserialization.
        /// Apparently passed variable name ABSOLUTELY must the same as its destination value name  (ex: url-> Url, websiteDomain -> WebsiteDomain).
        /// </summary>
        /// <param name="url"></param>
        /// <param name="websiteDomain"></param>
        /// <param name="bookName"></param>
        /// <param name="chapterId"></param>
        /// <param name="isSelectedForDownload"></param>
        /// <param name="pagesUrl"></param>
        /// <param name="isTemporaryData"></param>
        /// <param name="downloadUrlBookName"></param>
        [JsonConstructor]
        public AnimeSamaFrScanData(string url, string websiteDomain, string bookName, int chapterId, bool isSelectedForDownload, List<string> pagesUrl, bool isTemporaryData, string downloadUrlBookName)
            : base(url, websiteDomain, bookName, chapterId, isSelectedForDownload, pagesUrl, isTemporaryData)
        {
            Url = url;
            WebsiteDomain = websiteDomain;
            BookName = bookName;
            ChapterId = chapterId;
            IsSelectedForDownload = isSelectedForDownload;
            PagesUrl = pagesUrl;
            IsTemporaryData = isTemporaryData;
            DownloadUrlBookName = downloadUrlBookName;
        }

        /// <summary>
        /// This construtor should only be used to create a temporary scan data and get url info from it
        /// </summary>
        /// <param name="url"></param>
        public AnimeSamaFrScanData(string url) : base(url) // FOR TEMPORARY SCAN DATA ONLY
        {
            this.Url = url;
            WebsiteDomain = Constants.ANIMESAMA_DOMAIN_NAME;
            ChapterId = -1; // Fake chapter to get book info
            BookName = GetBookNameFromUrl(url);
            IsSelectedForDownload = false;
            IsTemporaryData = true;
        }

        public AnimeSamaFrScanData(string url, int chapterId) : base(url, chapterId)
        {
            this.Url = url;
            WebsiteDomain = Constants.ANIMESAMA_DOMAIN_NAME;
            ChapterId = chapterId;
            BookName = GetBookNameFromUrl(url);
            IsSelectedForDownload = true;
            IsTemporaryData = false;
        }


        #region Inherited functions
        public override async Task<bool> InitScanData()
        {
            ThrowExceptionIfTemporaryData();

            string htmlContent = await GetUrlHtmlContent();

            if (htmlContent == null)
            {
                Debug.WriteLine($"ERROR WHILE DOWNLOADING HTML CONTENT");
                return false;
            }

            string chapterUrl = GetChapterUrl(htmlContent);
            bool chapterDoesntExist = !await DoesChapterExist(chapterUrl);

            if (chapterDoesntExist)
            {
                Debug.WriteLine($"CHAPTER DOESNT EXIST");
                return false;
            }

            PagesUrl = await ParseHtmlToGetImgLinks(htmlContent);

            if(PagesUrl == null || PagesUrl.Count == 0)
            {
                Debug.WriteLine($"NO IMG URL FOUND");
                return false;
            }

            return true;
        }

        public override bool UrlContainsChapter()
        {
            return false; // Anime Sama scan url never contains chapter number
        }

        protected override string GetBookNameFromUrl(string url, bool removeSpace = false)
        {
            #region Chapter and img url examples
            // Example of chapter url
            //   0    1      2           3         4     5   6
            // https://anime-sama.fr/catalogue/berserk/scan/vf/
            // https://anime-sama.fr/catalogue/20th-century-boys/scan/vf/
            // https://anime-sama.fr/catalogue/alice-in-borderland/scan/vf/
            // https://anime-sama.fr/catalogue/fairy-tail/scan/vf/
            // https://anime-sama.fr/catalogue/the-terminally-ill-young-master-of-the-baek-clan/scan/vf/

            // Example of img url
            //   0    1      2        3    4     5    6   7
            // https://anime-sama.fr/s2/scans/Berserk/2/1.jpg
            // https://anime-sama.fr/s2/scans/20th%20Century%20boys/1/1.jpg
            // https://anime-sama.fr/s2/scans/Alice%20in%20Borderland/1/1.jpg
            // https://anime-sama.fr/s2/scans/Fairy%20Tail/1/1.jpg
            // https://anime-sama.fr/s2/scans/The%20Terminally%20Ill%20Young%20Master%20of%20the%20Baek%20Clan/1/1.jpg
            #endregion

            int splitIdForChapterUrl = 4;
            int splitIdForImgUrl = 5;
            bool isImgUrl = url.Contains(Constants.ANIMESAMA_IMG_URL_MARKER); // Check if we are using a link of a chapter or an image

            string bookName = url.Split(Constants.SLASH_CHAR)[isImgUrl ? splitIdForImgUrl : splitIdForChapterUrl];
            bookName = bookName.ToTitleCase();
            bookName = bookName.Replace(Constants.DASH_CHAR.ToString(), removeSpace ? string.Empty : Constants.SPACE);
            bookName = bookName.Replace(Constants.UNDERSCORE_CHAR.ToString(), removeSpace ? string.Empty : Constants.SPACE);

            return bookName;
        }

        protected override string GetChapterNumberFromUrl(string url, bool keepNumberOnly = true)
        {
            #region Chapter and img url examples
            // Example of chapter url
            //   0    1      2           3         4     5   6
            // https://anime-sama.fr/catalogue/berserk/scan/vf/
            // https://anime-sama.fr/catalogue/20th-century-boys/scan/vf/
            // https://anime-sama.fr/catalogue/alice-in-borderland/scan/vf/
            // https://anime-sama.fr/catalogue/fairy-tail/scan/vf/
            // https://anime-sama.fr/catalogue/the-terminally-ill-young-master-of-the-baek-clan/scan/vf/

            // Example of img url
            //   0    1      2        3    4     5    6   7
            // https://anime-sama.fr/s2/scans/Berserk/2/1.jpg
            // https://anime-sama.fr/s2/scans/20th%20Century%20boys/1/1.jpg
            // https://anime-sama.fr/s2/scans/Alice%20in%20Borderland/1/1.jpg
            // https://anime-sama.fr/s2/scans/Fairy%20Tail/1/1.jpg
            // https://anime-sama.fr/s2/scans/The%20Terminally%20Ill%20Young%20Master%20of%20the%20Baek%20Clan/1/1.jpg
            #endregion

            int splitIdForImgUrl = 6;
            bool isImgUrl = url.Contains(Constants.ANIMESAMA_IMG_URL_MARKER); // Check if we are using a link of a chapter or an image

            if (isImgUrl)
            {
                string chapterNumber = url.Split(Constants.SLASH_CHAR)[splitIdForImgUrl];
                if (keepNumberOnly) chapterNumber = chapterNumber.Split(Constants.DASH_CHAR)[1];

                return chapterNumber;
            }
            else
            {
                // Impossible to get chapter number from anime Sama chapter url
                Debug.WriteLine($"\nIt's impossible to get the chapter number from the anime-sama chapter url");
                return "-1";
            }
        }

        public override string GetFileExtensionFromImgUrl(string url)
        {
            #region Img url examples
            // Example of img url
            //   0    1      2        3    4     5    6   7
            // https://anime-sama.fr/s2/scans/Berserk/2/1.jpg
            // https://anime-sama.fr/s2/scans/20th%20Century%20boys/1/1.jpg
            // https://anime-sama.fr/s2/scans/Alice%20in%20Borderland/1/1.jpg
            // https://anime-sama.fr/s2/scans/Fairy%20Tail/1/1.jpg
            // https://anime-sama.fr/s2/scans/The%20Terminally%20Ill%20Young%20Master%20of%20the%20Baek%20Clan/1/1.jpg
            #endregion

            ThrowExceptionIfTemporaryData();

            string fileExtension = url.Split(Constants.SLASH_CHAR)[7];
            fileExtension = "." + fileExtension.Split(Constants.POINT_CHAR)[1];

            return fileExtension;

            // Should I always return .jpg directly since I build the link myself ?
            //return Constants.JPG_EXTENSION;
        }        

        protected override async Task<List<string>> ParseHtmlToGetImgLinks(string htmlContent)
        {
            ThrowExceptionIfTemporaryData();

            Debug.WriteLine($"\nPARSE HTML - {BookName}_{ChapterId} ({Url}), imgs found:");
            // V1 Recreate url since we can't get the page after js loading

            // https://anime-sama.fr/s2/scans/Berserk/2/1.jpg
            // https://anime-sama.fr/s2/scans/20th%20Century%20boys/1/1.jpg
            // https://anime-sama.fr/s2/scans/Alice%20in%20Borderland/1/1.jpg
            // https://anime-sama.fr/s2/scans/Fairy%20Tail/1/1.jpg
            // https://anime-sama.fr/s2/scans/The%20Terminally%20Ill%20Young%20Master%20of%20the%20Baek%20Clan/1/1.jpg

            // TODO: Manage Scan in english-> VF (normal url)/VA (add: " Anglais" after image name) <- check if reliable
            string chapterUrl = GetChapterUrl(htmlContent);

            List<string> imgUrls = new List<string>();

            int pageId = 1;
            string imgUrl = $"{chapterUrl}{pageId}{Constants.JPG_EXTENSION}";
            while (await UrlLoadCorrectlyAsync(imgUrl))
            {
                imgUrls.Add(imgUrl);
                pageId++;
                imgUrl = $"{chapterUrl}{pageId}{Constants.JPG_EXTENSION}";
            }

            imgUrls.Log(); // Debug log of list items
            Debug.WriteLine("\n");

            return imgUrls;
        }
        #endregion

        #region AnimaSama own functions
        private string GetChapterUrl(string htmlContent)
        {
            ThrowExceptionIfTemporaryData();

            if (string.IsNullOrEmpty(DownloadUrlBookName))
            {
                DownloadUrlBookName = ParseToFindBookNameForImgUrl(htmlContent);
            }

            return $"{Constants.ANIMESAMA_IMG_URL_START}{DownloadUrlBookName}/{ChapterId}/";
        }

        private string ParseToFindBookNameForImgUrl(string htmlContent)
        {
            ThrowExceptionIfTemporaryData();

            // Get Book Name for download URL
            string[] splitContent = htmlContent.Split(Constants.ANIMESAMA_BOOK_NAME_START_SEPARATOR, StringSplitOptions.RemoveEmptyEntries); // Split before the meta tag with the book name
            string bookNameForUrl = splitContent[1]; // Keep the split after our separator (trim the beginning)

            splitContent = bookNameForUrl.Split(Constants.ANIMESAMA_BOOK_NAME_END_SEPARATOR, StringSplitOptions.RemoveEmptyEntries); // Split after the book name
            bookNameForUrl = splitContent[0]; // Keep the split before our separator (trim the end)

            return bookNameForUrl;
        }
        
        private async Task<bool> DoesChapterExist(string chapterUrl)
        {
            ThrowExceptionIfTemporaryData();

            int firstImgId = 1;
            string firstImgUrl = $"{chapterUrl}{firstImgId}{Constants.JPG_EXTENSION}";

            if (await UrlLoadCorrectlyAsync(firstImgUrl))
            {
                return true;
            }
            else
            {
                Error.ChapterDoesntExist(this, firstImgUrl);
                return false;
            }
        }

        public static async Task<UrlValidityResult> IsUrlValid(string url)
        {
            // What are the caracteristics of a valid anime sama url ?
            // https://anime-sama.fr/catalogue/20th-century-boys/scan/vf/ ==> 8 splits 
            // https://anime-sama.fr/catalogue/20th-century-boys ==> 5 splits and last split must not be empty
            // Check if url is long enough to have a book name

            // TODO: Not sure about allowing short url for this website

            UrlValidityResult result = new UrlValidityResult(url);

            string[] urlSplitAtSlash = url.Split(Constants.SLASH_CHAR);
            if (urlSplitAtSlash.Length < 5 || (urlSplitAtSlash.Length == 5 && string.IsNullOrEmpty(urlSplitAtSlash[4]))) // Check if there is enough or too much / in the url to be a valid url
            {
                result.InvalidityReason = "Url is too short, book name is probably missing in the url";
                result.IsValid = false;
            }
            else if (urlSplitAtSlash.Length > 8)
            {
                result.InvalidityReason = "Url seems too long, url should stop with \"vf/\" or \"va/\"";
                result.IsValid = false;
            }
            else
            {
                if (await UrlLoadCorrectlyAsync(url) == false) // Test if we can load url
                {
                    result.InvalidityReason = "Impossible to load url, make sure the url load in a web browser";
                    result.IsValid = false;
                }
                else
                {
                    result.IsValid = true;
                }
            }            
            
            return result;
        }
        #endregion
    }
}
