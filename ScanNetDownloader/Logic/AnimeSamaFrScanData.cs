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
        public override async Task<ScanDataInitResult> InitScanData()
        {
            ThrowExceptionIfTemporaryData();
 
            HtmlContentResult htmlContentResult = await GetUrlHtmlContent();
            ScanDataInitResult result = new ScanDataInitResult(this, htmlContentResult);

            if (htmlContentResult.Success == false)
            {
                Debug.WriteLine($"ERROR WHILE DOWNLOADING HTML CONTENT");
                result.Success = false;
                result.Exception = new Exception($"Error while loading html content for {Url}, check htmlContentResult for more info");
                return result;
            }

            string htmlContent = htmlContentResult.HtmlContent;
            string chapterUrl = GetChapterUrl(htmlContent);
            UrlLoadResult chapterLoadResult = await DoesChapterExist(chapterUrl);
            bool chapterDoesntExist = !chapterLoadResult.Success;

            if (chapterDoesntExist)
            {
                Debug.WriteLine($"CHAPTER DOESNT EXIST");
                result.Success = false;
                result.Exception = new Exception($"The chapter {ChapterId} doesn't exist, {chapterUrl} does not load correctly");
                return result;
            }

            PagesUrl = await ParseHtmlToGetImgLinks(htmlContent);

            if(PagesUrl == null || PagesUrl.Count == 0)
            {
                Debug.WriteLine($"NO IMG URL FOUND");
                result.Success = false;
                result.Exception = new Exception($"No images urls were found for {BookName}-{ChapterId}");
                return result;
            }

            result.Success= true;
            return result;
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

            if (IsUrlForScanInEnglish()) bookName += Constants.ANIMESAMA_ENGLISH_BOOKNAME_SUFFIX;

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

            // Local functions to return success value of UrlLoadResult
            async Task<bool> UrlLoadSuccessfully(string urlToLoad)
            {
                UrlLoadResult urlLoadResult = await UrlLoadCorrectlyAsync(urlToLoad);
                return urlLoadResult.Success;
            }

            while (await UrlLoadSuccessfully(imgUrl))
            {
                imgUrls.Add(imgUrl);
                pageId++;
                imgUrl = $"{chapterUrl}{pageId}{Constants.JPG_EXTENSION}";
                Debug.WriteLine($"New img found: {imgUrl}");
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

            if (IsUrlForScanInEnglish() == true)
            {
                return $"{Constants.ANIMESAMA_IMG_URL_START}{DownloadUrlBookName}{Constants.ANIMESAMA_ENGLISH_IMG_SUFFIX}/{ChapterId}/";
            }
            else
            {
                return $"{Constants.ANIMESAMA_IMG_URL_START}{DownloadUrlBookName}/{ChapterId}/";
            }
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
        
        private async Task<UrlLoadResult> DoesChapterExist(string chapterUrl)
        {
            ThrowExceptionIfTemporaryData();

            int firstImgId = 1;
            string firstImgUrl = $"{chapterUrl}{firstImgId}{Constants.JPG_EXTENSION}";

            UrlLoadResult urlLoadResult = await UrlLoadCorrectlyAsync(firstImgUrl);
            if (urlLoadResult.Success == false)
            {
                Error.ChapterDoesntExist(this, firstImgUrl);
            }
            return urlLoadResult;

        }

        private bool IsUrlForScanInEnglish() // TODO: If I discover more scan language on the website, I need to have a cleaner way to do this.
        {
            string[] splitUrl = Url.Split(Constants.SLASH_CHAR);
            if (splitUrl.Length > 6) // Language split index is 6 so count must be 7 at least
            {
                return splitUrl[6].Contains(Constants.ANIMESAMA_ENGLISH_SCAN_URL_MARKER);
            }
            else
            {
                return false;
            }
        }

        public static async Task<UrlValidityResult> IsUrlValid(string url)
        {
            // What are the caracteristics of a valid anime sama url ?
            // https://anime-sama.fr/catalogue/20th-century-boys/scan/vf/ ==> 8 splits 
            // https://anime-sama.fr/catalogue/20th-century-boys ==> 5 splits and last split must not be empty
            // Check if url is long enough to have a book name

            // TODO: Manage short url ? Give Url Example when this happens

            UrlValidityResult result = new UrlValidityResult(url);

            string[] urlSplitAtSlash = url.Split(Constants.SLASH_CHAR);
            if (urlSplitAtSlash.Length < 7 || string.IsNullOrEmpty(urlSplitAtSlash[6]))// || (urlSplitAtSlash.Length == 5 && string.IsNullOrEmpty(urlSplitAtSlash[4]))) // Check if there is enough or too much / in the url to be a valid url
            {
                result.InvalidityReason = "Url is too short, informations are missing in the url";
                result.Success = false;
            }
            else if (urlSplitAtSlash.Length > 8)
            {
                result.InvalidityReason = "Url seems too long, url should stop with \"vf/\" or \"va/\"";
                result.Success = false;
            }
            else
            {
                UrlLoadResult loadResult = await UrlLoadCorrectlyAsync(url);
                if (loadResult.Success == false) // Test if we can load url
                {
                    result.InvalidityReason = "Impossible to load url, make sure the url load in a web browser";
                    result.Exception = loadResult.Exception;
                    result.Success = false;
                }
                else
                {
                    result.Success = true;
                }
            }            
            
            return result;
        }
        #endregion
    }
}
