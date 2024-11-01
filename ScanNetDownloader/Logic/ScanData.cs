using Newtonsoft.Json;
using System.Diagnostics;
using System.Net;

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

        public async Task<List<string>> GetScanImagesUrl()
        {
            string htmlContent = await GetUrlHtmlContent();
            if (htmlContent == null) return new List<string>();

            return await ParseHtmlToGetImgLinks(htmlContent);
        }
        
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

        public abstract Task<bool> InitScanData();

        protected abstract string GetBookNameFromUrl(string url, bool removeSpace = false);

        protected abstract string GetChapterNumberFromUrl(string url, bool keepNumberOnly = true);

        public abstract string GetFileExtensionFromUrl(string url); //TODO: Can probably be improved once img url will be saved in ScanData

        public abstract bool UrlContainsChapter();

        protected abstract Task<List<string>> ParseHtmlToGetImgLinks(string htmlContent);

        [Obsolete("Create a private function in ScanVfNet and delete other occurence of this" )] // TODO: Clean this
        public abstract string GenerateAnotherChapterUrl(int chapterId);

        [Obsolete] // TODO: Clean this
        public abstract bool DoesThisChapterExist(int chapterId); 


        public async Task<bool> UrlLoadCorrectlyAsync(string url, int timeout = 1500)
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

        public static bool UrlLoadCorrectly(string url, int timeout = 1500)
        {
            // TODO: Improve some give false positive, Why ? Timeout too short ? 1500 seems way better
            WebRequest webRequest = WebRequest.Create(url);
            webRequest.Method = "HEAD";
            webRequest.Timeout = timeout;

            try
            {
                WebResponse response = webRequest.GetResponse();
                response.Close();
                return true;
            }
            catch(WebException ex)
            {
                // TODO: manage different type of Exception -> 404 means wring url but timeout may mean a valid url
                Debug.WriteLine($"|---> Invalid url: {url}\n{ex}");
                return false;
            }
        }
    }

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
