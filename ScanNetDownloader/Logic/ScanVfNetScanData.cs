using Newtonsoft.Json;
using System.Diagnostics;
using ScanNetDownloader.Logic.Helpers;
using System.Net.Http;

namespace ScanNetDownloader.Logic
{
    public class ScanVfNetScanData : ScanData
    {
        /// <summary>
        /// This constructor only purpose is for Json deserialization.
        /// Apparently passed variable name ABSOLUTELY must be the same as its destination value name  (ex: url-> Url, websiteDomain -> WebsiteDomain).
        /// </summary>
        /// <param name="url"></param>
        /// <param name="websiteDomain"></param>
        /// <param name="bookName"></param>
        /// <param name="chapterId"></param>
        /// <param name="isSelectedForDownload"></param>
        /// <param name="pagesUrl"></param>
        /// <param name="isTemporaryData"></param>
        [JsonConstructor]
        public ScanVfNetScanData(string url, string websiteDomain, string bookName, int chapterId, bool isSelectedForDownload, List<string> pagesUrl, bool isTemporaryData)
            : base(url, websiteDomain, bookName, chapterId, isSelectedForDownload, pagesUrl, isTemporaryData)
        {
            Url = url;
            WebsiteDomain = websiteDomain;
            BookName = bookName;
            ChapterId = chapterId;
            IsSelectedForDownload = isSelectedForDownload;
            PagesUrl = pagesUrl;
            IsTemporaryData = isTemporaryData;
        }

        /// <summary>
        /// This construtor should only be used to create a temporary scan data and get url info from it
        /// </summary>
        /// <param name="url"></param>
        public ScanVfNetScanData(string url) : base(url) // FOR TEMPORARY SCAN DATA ONLY
        {
            Url = url;
            WebsiteDomain = Constants.SCANVF_DOMAIN_NAME;
            BookName = GetBookNameFromUrl(url);
            if (UrlContainsChapter()) ChapterId = int.Parse(GetChapterNumberFromUrl(url));
            else ChapterId = -1;
            IsSelectedForDownload = false;
            IsTemporaryData = true;
        }

        public ScanVfNetScanData(string url, int chapterId) : base(url, chapterId)
        {
            if (UrlContainsChapter() && int.Parse(GetChapterNumberFromUrl(url)) == chapterId) Url = url;
            else Url = GenerateAnotherChapterUrl(chapterId);
            WebsiteDomain = Constants.SCANVF_DOMAIN_NAME;
            BookName = GetBookNameFromUrl(url);
            ChapterId = chapterId;
            IsSelectedForDownload = true;
            IsTemporaryData = false;
        }


        #region Inherited functions
        public override async Task<ScanDataInitResult> InitScanData()
        {
            ThrowExceptionIfTemporaryData();

            HtmlContentResult htmlContentResult = await GetUrlHtmlContent();
            ScanDataInitResult result = new ScanDataInitResult(this, htmlContentResult);

            bool chapterDoesntExist = htmlContentResult.Success == false;

            if (chapterDoesntExist)
            {
                Error.ChapterDoesntExist(this, Url);
                Debug.WriteLine($"ERROR WHILE DOWNLOADING HTML CONTENT -> CHAPTER DOESNT EXIST");
                result.Success = false;
                result.Exception = new Exception($"The chapter {ChapterId} doesn't exist, {Url} does not load and we are unable to get its html content");
                return result;
            }

            string htmlContent = htmlContentResult.HtmlContent;
            PagesUrl = await ParseHtmlToGetImgLinks(htmlContent);

            if (PagesUrl == null || PagesUrl.Count == 0)
            {
                Debug.WriteLine($"NO IMG URL FOUND");
                result.Success = false;
                result.Exception = new Exception($"No images urls were found for {BookName}-{ChapterId}");
                return result;
            }

            result.Success = true;
            return result;
        }

        public override bool UrlContainsChapter()
        {
            string[] urlSplits = Url.Split(Constants.SLASH_CHAR); // Check if the url has a chapter number (the number of split let us know if url stop at book name or not)
            bool chapterIsInUrl = urlSplits.Length > 4; //
                                                        //
                                                        // : Should I also check && !string.IsNullOrEmpty(urlSplits[4]); to make sure Chapter split is not an empty split
            return chapterIsInUrl;
        }

        protected override string GetBookNameFromUrl(string url, bool removeSpace = false)
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

        protected override string GetChapterNumberFromUrl(string url, bool keepNumberOnly = true)
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

        public override string GetFileExtensionFromImgUrl(string url)
        {
            #region Img url examples
            // Example of img url
            //   0    1      2            3      4      5        6         7          8
            // https://www.scan-vf.net/uploads/manga/kingdom/chapters/chapitre-810/01.webp
            // https://www.scan-vf.net/uploads/manga/attaque-des-titans/chapters/chapitre-139.5/01.jpg
            // https://www.scan-vf.net/uploads/manga/one-punch-man/chapters/chapitre-230/mp-01.webp
            // https://www.scan-vf.net/uploads/manga/one_piece/chapters/chapitre-1091/001.webp 
            #endregion

            ThrowExceptionIfTemporaryData();

            string fileExtension = url.Split(Constants.SLASH_CHAR)[8];
            fileExtension = "." + fileExtension.Split(Constants.POINT_CHAR)[1];

            return fileExtension;
        }

        protected override async Task<List<string>> ParseHtmlToGetImgLinks(string htmlContent)
        {
            ThrowExceptionIfTemporaryData();

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

            imgUrls.Log();
            Debug.WriteLine("\n");

            return imgUrls;
        }
        #endregion

        #region ScanVfNet own functions
        public string GenerateAnotherChapterUrl(int chapterId)
        {
            if (UrlContainsChapter())
            {
                string UrlStart = string.Join(Constants.SLASH_CHAR, Url.Split(Constants.SLASH_CHAR).Take(4)); // Take the Url up to book name
                return $"{UrlStart}{Constants.SCANVF_CHAPTER_IN_URL}{chapterId}"; // No need to specify "/1" after chapter number redirection is done by website
            }
            else
            {
                return $"{Url}{Constants.SCANVF_CHAPTER_IN_URL}{chapterId}";
            }
        }

        public static async Task<UrlValidityResult> IsUrlValid(string url)
        {
            // What are the caracteristics of a valid scanVf url ?
            //https://www.scan-vf.net/jujutsu-kaisen = url without chapter -> at least 4 splits and last split must not be empty
            //https://www.scan-vf.net/jujutsu-kaisen/chapitre-164/1 = url with chapter -> No more than 6 splits

            UrlValidityResult result = new UrlValidityResult(url);

            string[] urlSplitAtSlash = url.Split(Constants.SLASH_CHAR);
            if (urlSplitAtSlash.Length < 4 || (urlSplitAtSlash.Length == 4 && string.IsNullOrEmpty(urlSplitAtSlash[3]))) // Check if there is enough or too much / in the url to be a valid url
            {
                result.InvalidityReason = "Url is too short, book name is probably missing in the url";
                result.Success = false;
            }
            else if (urlSplitAtSlash.Length > 6)
            {
                result.InvalidityReason = "Url seems too long, url should stop with page id";
                result.Success = false;
            }
            else
            {
                UrlLoadResult loadResult = await UrlLoadCorrectlyAsync(url);
                if (loadResult.Success == false)
                {
                    if (loadResult.Exception is HttpRequestException)
                    {
                        result.InvalidityReason = "Impossible to load url, make sure the url load in a web browser";
                        result.Exception = loadResult.Exception;
                        result.Success = false;
                    }
                    else if (loadResult.Exception is InvalidOperationException || loadResult.Exception is NotSupportedException)
                    {                  
                        result.InvalidityReason = "This does not seem to be an url, usually the url should start with \"https://\"";
                        result.Exception = loadResult.Exception;
                        result.Success = false;
                    }    
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
