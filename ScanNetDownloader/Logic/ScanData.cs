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


        [JsonConstructor] // Only for Json deserialization, apparently passed variable name ABSOLUTELY must the same as its destination value name  (ex: url-> Url, websiteDomain -> WebsiteDomain)
        public ScanData(string url, string websiteDomain, string bookName, int chapterId, bool isSelectedForDownload)
        {
            Url = url;
            WebsiteDomain = websiteDomain;
            BookName = bookName;
            ChapterId = chapterId;
            IsSelectedForDownload = isSelectedForDownload;
        }

        public ScanData(string url)
        {
            Url = url;
        }

        public async Task<List<string>> GetScanImagesUrl()
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
                    return new List<string>();
                }
            }

            return ParseHtmlToGetImgLinks(htmlContent);
        }       

        protected abstract string GetBookNameFromUrl(string url, bool removeSpace = false);

        protected abstract string GetChapterNumberFromUrl(string url, bool keepNumberOnly = true);

        public abstract string GetFileExtensionFromUrl(string url); //TODO: Can probably be improved once img url will be saved in ScanData

        public abstract bool UrlContainsChapter();

        protected abstract List<string> ParseHtmlToGetImgLinks(string htmlContent);

        public bool DoesThisChapterExist(int chapterId) // TODO: Complete this
        {
            // Get a chapter Url from ScanData using chapterId

            // Web request to see if url exist

            // return web request result
            return true;
        }

        public static bool UrlLoadCorrectly(string url, int timeout = 1000)
        {
            WebRequest webRequest = WebRequest.Create(url);
            webRequest.Method = "HEAD";
            webRequest.Timeout = timeout;

            try
            {
                WebResponse response = webRequest.GetResponse();
                response.Close();
                return true;
            }
            catch
            {
                Debug.WriteLine($"|---> Invalid url: {url}");
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
