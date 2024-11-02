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

        public int PagesCount => PagesUrl != null ? PagesUrl.Count : -1;

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
        public abstract Task<bool> InitScanData();

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
                string htmlContent = await GetUrlHtmlContent();
                if (htmlContent == null) return new List<string>();

                PagesUrl = await ParseHtmlToGetImgLinks(htmlContent);

                return PagesUrl;
            }
            else // Return saved urls
            {
                return PagesUrl;
            }
        } 
        #endregion

        #region Web Request
        protected async Task<string> GetUrlHtmlContent()
        {
            string htmlContent;

            using (WebClient client = new WebClient())
            {
                try
                {
                    htmlContent = await client.DownloadStringTaskAsync(Url); // Save html code in a variable
                }
                catch (WebException ex)
                {
                    Error.FailedHtmlDownload(ex, Url);
                    htmlContent = null;
                }
            }

            return htmlContent;
        }

        public static async Task<bool> UrlLoadCorrectlyAsync(string url, int timeout = 1500)
        {
            // TODO: Improve some give false positive, Why ? Timeout too short ? 1500 seems way better
            WebRequest webRequest = WebRequest.Create(url);
            webRequest.Method = "HEAD";
            webRequest.Timeout = timeout;

            try
            {
                WebResponse response = await webRequest.GetResponseAsync();
                response.Close();
                return true;
            }
            catch (WebException ex)
            {
                // TODO: manage different type of Exception -> 404 means wring url but timeout may mean a valid url
                Debug.WriteLine($"|---> Invalid url: {url}\n{ex}");
                return false;
            }
        }
        #endregion
    }

    //
    // Result objects to return more information at the end of an operation
    // TODO: Should I move this in a dedicated class ?
    //

    public class UrlValidityResult
    {
        public string UrlTested;
        public bool IsValid;
        public string InvalidityReason;

        public UrlValidityResult(string urlToTest)
        {
            UrlTested = urlToTest;
            IsValid = false;
            InvalidityReason = "";
        }

        public UrlValidityResult(string urlToTest, bool isValid, string invalidityReason)
        {
            UrlTested = urlToTest;
            IsValid = isValid;
            InvalidityReason = invalidityReason;
        }
    }
}
