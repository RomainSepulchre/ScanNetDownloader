using Newtonsoft.Json;
using System.Diagnostics;
using System.Net;
using System.Windows.Navigation;

namespace ScanNetDownloader.Logic
{
    public abstract class ScanData
    {
        public string Url
        {
            get; protected set;
        }

        public string WebsiteDomain
        {
            get; protected set;
        }

        public string BookName
        {
            get; protected set;
        }

        public int ChapterId
        {
            get; protected set;
        }

        public bool IsSelectedForDownload
        {
            get; set;
        }

        public List<string> PagesUrl
        {
            get; protected set;
        }

        public int PagesCount => PagesUrl != null ? PagesUrl.Count : 0;

        public bool IsTemporaryData
        {
            get; protected set;
        }


        [JsonConstructor] // Only for Json deserialization, apparently passed variable name ABSOLUTELY must the same as its destination value name  (ex: url-> Url, websiteDomain -> WebsiteDomain)
        public ScanData(string url, string websiteDomain, string bookName, int chapterId, bool isSelectedForDownload, List<string> pagesUrl, bool isTemporaryData)
        {
            Url = url;
            WebsiteDomain = websiteDomain;
            BookName = bookName;
            ChapterId = chapterId;
            IsSelectedForDownload = isSelectedForDownload;
            PagesUrl = pagesUrl;
            IsTemporaryData = isTemporaryData;
        }

        public ScanData(string url)
        {
            Url = url;
        }

        public ScanData(string url, int chapterId)
        {
            Url = url;
            ChapterId = chapterId;
        }


        #region Abstract functions
        public abstract Task<ScanDataInitResult> InitScanData();

        public abstract bool UrlContainsChapter();

        protected abstract string GetBookNameFromUrl(string url, bool removeSpace = false);

        protected abstract string GetChapterNumberFromUrl(string url, bool keepNumberOnly = true);

        public abstract string GetFileExtensionFromImgUrl(string url); //TODO: Can probably be improved once img url will be saved in ScanData     

        protected abstract Task<List<string>> ParseHtmlToGetImgLinks(string htmlContent);
        #endregion

        #region Temporary Data Exception
        protected void ThrowExceptionIfTemporaryData()
        {
            if (IsTemporaryData)
            {
                throw new Exception($"A temporary scan data should never call this function");
            }
        } 
        #endregion

        #region Download Images
        public async Task<List<string>> GetScanImagesUrl()
        {
            ThrowExceptionIfTemporaryData();

            if (PagesUrl.Count == 0) // if no urls saved retry to get them
            {
                HtmlContentResult htmlContentResult = await GetUrlHtmlContent();
                if (htmlContentResult.Success == false)
                {
                    // TODO: Manage error here, if this is still needed after download rework
                    return new List<string>();
                }
                else
                {
                    string htmlContent = htmlContentResult.HtmlContent;
                    PagesUrl = await ParseHtmlToGetImgLinks(htmlContent);

                    return PagesUrl;
                }
            }
            else // Return saved urls
            {
                return PagesUrl;
            }
        } 
        #endregion

        #region Web Request
        protected async Task<HtmlContentResult> GetUrlHtmlContent()
        {
            HtmlContentResult result = new HtmlContentResult();

            using (WebClient client = new WebClient())
            {
                try
                {
                    result.HtmlContent = await client.DownloadStringTaskAsync(Url); // Save html code in a variable
                    result.Success = true;
                }
                catch (WebException ex)
                {
                    Error.FailedHtmlDownload(ex, Url);
                    result.Success = false;
                    result.Exception = ex;
                    result.HtmlContent = null;
                }
            }

            return result;
        }

        public static async Task<UrlLoadResult> UrlLoadCorrectlyAsync(string url, int timeout = 1500)
        {
            UrlLoadResult result = new UrlLoadResult(url);

            // TODO: Improve some give false positive, Why ? Timeout too short ? 1500 seems way better
            WebRequest webRequest = WebRequest.Create(url);
            webRequest.Method = "HEAD";
            webRequest.Timeout = timeout;

            try
            {
                WebResponse response = await webRequest.GetResponseAsync();
                response.Close();
                result.Success = true;
                return result;
            }
            catch (WebException ex)
            {
                // TODO: manage different type of Exception -> 404 means wring url but timeout may mean a valid url
                Debug.WriteLine($"|---> Invalid url: {url}\n{ex}");
                result.Success = false;
                result.Exception = ex;
                return result;
            }
        }
        #endregion
    }
}
