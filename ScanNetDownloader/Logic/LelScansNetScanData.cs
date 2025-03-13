using Newtonsoft.Json;
using ScanNetDownloader.Logic.Helpers;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace ScanNetDownloader.Logic
{
    class LelScansNetScanData : ScanData
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
        public LelScansNetScanData(string url, string websiteDomain, string bookName, int chapterId, bool isSelectedForDownload, List<string> pagesUrl, bool isTemporaryData)
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
        public LelScansNetScanData(string url) : base(url) // FOR TEMPORARY SCAN DATA ONLY
        {
            Url = url;
            WebsiteDomain = Constants.LELSCANS_DOMAIN_NAME;
            BookName = GetBookNameFromUrl(url);
            if (UrlContainsChapter()) ChapterId = int.Parse(GetChapterNumberFromUrl(url));
            else ChapterId = -1;
            IsSelectedForDownload = false;
            IsTemporaryData = true;
        }

        public LelScansNetScanData(string url, int chapterId) : base(url, chapterId)
        {
            if (UrlContainsChapter() && int.Parse(GetChapterNumberFromUrl(url)) == chapterId) Url = url;
            else Url = GenerateAnotherChapterUrl(chapterId, GetBookNameFromUrl(url));
            WebsiteDomain = Constants.LELSCANS_DOMAIN_NAME;
            BookName = GetBookNameFromUrl(url);
            ChapterId = chapterId;
            IsSelectedForDownload = true;
            IsTemporaryData = false;
        }

        #region Inherited functions
        public override string GetFileExtensionFromImgUrl(string url)
        {
            // https://lelscans.net/mangas/hunter-x-hunter/410/03.jpg
            // https://lelscans.net/mangas/magi/353/03.jpg?v=fr1502046766

            ThrowExceptionIfTemporaryData();

            string fileExtension = url.Split(Constants.SLASH_CHAR)[6];
            fileExtension = "." + fileExtension.Split(Constants.POINT_CHAR)[1];

            return fileExtension;
        }

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
            // https://lelscans.net/lecture-ligne-bleach.php

            // https://lelscans.net/scan-one-piece/999
            // https://lelscans.net/scan-dr-stone/232/12
            // https://lelscans.net/scan-magi/353/4

            // https://lelscans.net/mangas/hunter-x-hunter/410/03.jpg
            // https://lelscans.net/mangas/magi/353/03.jpg?v=fr1502046766
            //

            int splitIdForChapterUrl = 3;
            int splitIdForImgUrl = 4;
            bool isImgUrl = url.Contains(Constants.LELSCANS_IMG_URL_MARKER); // Check if we are using a link of a chapter or an image
            bool isHomeUrl = url.Split(Constants.SLASH_CHAR).Length == 4;
            string bookName;

            if (isHomeUrl)
            {
                bookName = url.Split(Constants.SLASH_CHAR)[3]; // get 4th split
                if (bookName.StartsWith(Constants.LELSCANS_LECTURE_PREFIX)) bookName = bookName.Remove(0, Constants.LELSCANS_LECTURE_PREFIX.Length);
                else if (bookName.StartsWith(Constants.LELSCANS_LECTURE_PREFIX_2)) bookName = bookName.Remove(0, Constants.LELSCANS_LECTURE_PREFIX_2.Length);
                bookName = bookName.Split(Constants.POINT_CHAR)[0]; // Remove .php extension
            }
            else
            {
                bookName = url.Split(Constants.SLASH_CHAR)[isImgUrl ? splitIdForImgUrl : splitIdForChapterUrl];
                if (bookName.StartsWith(Constants.LELSCANS_SCAN_PREFIX)) bookName = bookName.Remove(0, Constants.LELSCANS_SCAN_PREFIX.Length);
            }  
            
            bookName = bookName.ToTitleCase();
            bookName = bookName.Replace(Constants.DASH_CHAR.ToString(), removeSpace ? string.Empty : Constants.SPACE);
            bookName = bookName.Replace(Constants.UNDERSCORE_CHAR.ToString(), removeSpace ? string.Empty : Constants.SPACE);

            return bookName;
        }

        protected override string GetChapterNumberFromUrl(string url, bool keepNumberOnly = true)
        {
            int splitIdForChapterUrl = 4;
            int splitIdForImgUrl = 5;
            bool isImgUrl = url.Contains(Constants.LELSCANS_IMG_URL_MARKER); // Check if we are using a link of a chapter or an image

            string chapterNumber = url.Split(Constants.SLASH_CHAR)[isImgUrl ? splitIdForImgUrl : splitIdForChapterUrl];
            if (keepNumberOnly == false) chapterNumber = $"Chapter-{chapterNumber}";

            return chapterNumber;
        }

        protected override async Task<List<string>> ParseHtmlToGetImgLinks(string htmlContent)
        {
            ThrowExceptionIfTemporaryData();

            Debug.WriteLine($"\nPARSE HTML - {BookName}_{ChapterId} ({Url}), imgs found:");

            // Get link for all imgs pages
            string[] splitContent = htmlContent.Split(Constants.LELSCANS_NAV_PAGE_URL_START_SEPARATOR, StringSplitOptions.RemoveEmptyEntries); // Split before the block with all the img url
            string urlSplit = splitContent[1]; // Keep the split after our separator (trim the beginning)

            splitContent = urlSplit.Split(Constants.LELSCANS_NAV_PAGE_URL_END_SEPARATOR, StringSplitOptions.RemoveEmptyEntries); // Split after the block with all the img url
            urlSplit = splitContent[0]; // Keep the split before our separator (trim the end)

            splitContent = urlSplit.Split(Constants.LELSCANS_CLEAN_BEFORE_NAV_LINKS_SEPARATOR, StringSplitOptions.RemoveEmptyEntries); // Remove the html code that is before the first <img/>
            urlSplit = splitContent[1]; // Keep the split after our separator (trim the beginning)

            List<string> imgPageUrls = urlSplit.Split(Constants.LELSCANS_NAV_LINK_END_SEPARATOR, StringSplitOptions.RemoveEmptyEntries).ToList(); // Split each img tag in a list

            // Keep only the urls in the list
            for (int i = imgPageUrls.Count - 1; i >= 0; i--)
            {
                if (imgPageUrls[i].ToLower().Contains(Constants.HTTP_ADDRESS))
                {
                    string[] imgUrlSplit = imgPageUrls[i].Split([Constants.DOUBLE_QUOTE_CHAR, Constants.SINGLE_QUOTE_CHAR]);
                    foreach (string split in imgUrlSplit)
                    {
                        // Keep only the split containing the url
                        if (split.ToLower().Contains(Constants.HTTP_ADDRESS))
                        {
                            imgPageUrls[i] = split;
                            imgPageUrls[i] = imgPageUrls[i].Replace(" ", string.Empty); // Remove space in the string
                        }
                    }
                }
                else
                {
                    imgPageUrls.RemoveAt(i);
                }
            }

            imgPageUrls = imgPageUrls.Distinct().ToList(); // remove duplicated url caused by previous and next buttons

            imgPageUrls.Log();
            Debug.WriteLine("\n");

            // Now that we have every page url go through each page to get the img url
            List<string> imgUrls = new List<string>();
            foreach (string pageUrl in imgPageUrls)
            {
                HtmlContentResult htmlContentResult = await GetUrlHtmlContent(pageUrl);

                if(htmlContentResult.Success == false)
                {
                    // TODO: Error management here
                    // Skip url we were unable to get html from the page
                    continue;
                }
                else
                {
                    string imgPageHtml = htmlContentResult.HtmlContent;

                    string[] imgSplitContent = imgPageHtml.Split(Constants.LELSCANS_IMG_DIV_START_SEPARATOR, StringSplitOptions.RemoveEmptyEntries); // Split before the block with all the img url
                    string imgPageSplit = imgSplitContent[1]; // Keep the split after our separator (trim the beginning)

                    imgSplitContent = imgPageSplit.Split(Constants.LELSCANS_IMG_DIV_END_SEPARATOR, StringSplitOptions.RemoveEmptyEntries); // Split after the block with all the img url
                    imgPageSplit = imgSplitContent[0]; // Keep the split before our separator (trim the end)

                    imgSplitContent = imgPageSplit.Split(Constants.LELSCANS_IMG_URL_START_SEPARATOR, StringSplitOptions.RemoveEmptyEntries); // Remove the html code that is before the first <img/>
                    imgPageSplit = imgSplitContent[1]; // Keep the split after our separator (trim the beginning)

                    string imgUrl = imgPageSplit.Split(Constants.LELSCANS_NAV_LINK_END_SEPARATOR, StringSplitOptions.RemoveEmptyEntries)[0]; // Keep the split before our separator (trim the end)

                    // Clean url and add it to the list
                    if (imgUrl.ToLower().Contains(Constants.LELSCANS_IMG_URL_MARKER))
                    {
                        string[] imgUrlSplit = imgUrl.Split([Constants.DOUBLE_QUOTE_CHAR, Constants.SINGLE_QUOTE_CHAR]);
                        foreach (string split in imgUrlSplit)
                        {
                            // Keep only the split containing the url
                            if (split.ToLower().Contains(Constants.LELSCANS_IMG_URL_MARKER))
                            {
                                imgUrl = split;
                                imgUrl = imgUrl.Replace(" ", string.Empty); // Remove space in the string
                            }
                        }

                        imgUrl = imgUrl.Split('?')[0]; // Remove everything after the image extension
                        imgUrl = $"{Constants.LELSCANS_IMG_URL_ROOT}{imgUrl}";
                        imgUrls.Add(imgUrl);
                    }
                    else
                    {
                        // TODO: Error management here
                        // Skip url if we were unable to find an img url
                        continue;
                    }
                }           
            }

            imgUrls.Sort();
            imgUrls.Log();
            Debug.WriteLine("\n");

            return imgUrls;
        }
        #endregion

        public string GenerateAnotherChapterUrl(int chapterId, string urlBookName)
        {
            if (UrlContainsChapter())
            {
                string UrlStart = string.Join(Constants.SLASH_CHAR, Url.Split(Constants.SLASH_CHAR).Take(4)); // Take the Url up to book name
                return $"{UrlStart}/{chapterId}"; // No need to specify "/1" after chapter number redirection is done by website
            }
            else
            {
                string bookNameForUrl = urlBookName.ToLower();
                bookNameForUrl = bookNameForUrl.Replace(Constants.SPACE, Constants.DASH_CHAR.ToString());
                return $"{Constants.LELSCANS_IMG_URL_ROOT}/{Constants.LELSCANS_SCAN_PREFIX}{bookNameForUrl}/{chapterId}";
            }
        }

        public static async Task<UrlValidityResult> IsUrlValid(string url)
        {
            // What are the caracteristics of a valid LelScan url ?
            // TODO: Do this work? https://lelscans.net/lecture-ligne-my-hero-academia.php = url without chapter -> at least 4 splits and last split must not be empty
            //https://lelscans.net/scan-one-piece/999
            //https://lelscans.net/scan-my-hero-academia/431/4 = url with chapter -> No more than 6 splits

            UrlValidityResult result = new UrlValidityResult(url);

            string[] urlSplitAtSlash = url.Split(Constants.SLASH_CHAR);

            // TODO: I need to test short url with .php before keeping this
            if (urlSplitAtSlash.Length < 4 || (urlSplitAtSlash.Length == 4 && string.IsNullOrEmpty(urlSplitAtSlash[3]))) // Check if there is enough or too much / in the url to be a valid url
            //if (urlSplitAtSlash.Length < 5 || (urlSplitAtSlash.Length == 5 && string.IsNullOrEmpty(urlSplitAtSlash[4])))
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
    }
}
