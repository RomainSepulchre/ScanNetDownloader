using Newtonsoft.Json;
using System.Diagnostics;
using System.Net.Http;

namespace ScanNetDownloader.Logic
{
    public abstract class ScanData : IComparable<ScanData>
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

        public string? LocationPath
        {
            get; set;
        }


        [JsonConstructor] // Only for Json deserialization, apparently passed variable name ABSOLUTELY must the same as its destination value name  (ex: url-> Url, websiteDomain -> WebsiteDomain)
        public ScanData(string url, string websiteDomain, string bookName, int chapterId, bool isSelectedForDownload, List<string> pagesUrl, bool isTemporaryData, string path)
        {
            Url = url;
            WebsiteDomain = websiteDomain;
            BookName = bookName;
            ChapterId = chapterId;
            IsSelectedForDownload = isSelectedForDownload;
            PagesUrl = pagesUrl;
            IsTemporaryData = isTemporaryData;
            LocationPath = path;
        }

        public ScanData(string url)
        {
            Url = url;
        }

        public ScanData(string url, int chapterId, string bookName)
        {
            Url = url;
            ChapterId = chapterId;
            BookName = bookName;
        }


        #region Abstract functions
        public abstract Task<ScanDataInitResult> InitScanData();

        public abstract bool UrlContainsChapter();

        protected abstract string GetBookNameFromUrl(string url, bool removeSpace = false);

        protected abstract string GetChapterNumberFromUrl(string url, bool keepNumberOnly = true);

        public abstract string GetFileExtensionFromImgUrl(string url);    

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

            HttpClient client = HttpClientSingleton.Client;
            try
            {
                result.HtmlContent = await client.GetStringAsync(Url);
                result.Success = true;
            }
            catch (HttpRequestException ex)
            {
                //TODO: Check ex.StatusCode to know act depending on the type of error
                Debug.WriteLine(ex);
                Error.FailedHtmlDownload(ex, Url);
                result.Success = false;
                result.StatusCode = ex.StatusCode;
                result.Exception = ex;
                result.HtmlContent = null;
            }
            return result;
        }

        protected async Task<HtmlContentResult> GetUrlHtmlContent(string url)
        {
            HtmlContentResult result = new HtmlContentResult();

            HttpClient client = HttpClientSingleton.Client;
            try
            {
                result.HtmlContent = await client.GetStringAsync(url);
                result.Success = true;
            }
            catch (HttpRequestException ex)
            {
                //TODO: Check ex.StatusCode to know act depending on the type of error
                Debug.WriteLine(ex);
                Error.FailedHtmlDownload(ex, url);
                result.Success = false;
                result.StatusCode = ex.StatusCode;
                result.Exception = ex;
                result.HtmlContent = null;
            }
            return result;
        }

        public static async Task<UrlLoadResult> UrlLoadCorrectlyAsync(string url, int timeout = 1500)
        {
            UrlLoadResult result = new UrlLoadResult(url);

            HttpClient client = HttpClientSingleton.Client;

            try
            {
                using (HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Head, url))
                {
                    using (HttpResponseMessage response = await client.SendAsync(request))
                    {
                        result.StatusCode = response.StatusCode;
                        if (response.IsSuccessStatusCode)
                        {
                            result.Success = true;   
                        }
                        else
                        {
                            result.Success = false;
                        }     
                    }
                }
            }
            catch (HttpRequestException ex)
            {
                Debug.WriteLine($"{ex} |---> Invalid url: {url}");
                result.Success = false;
                result.StatusCode = ex.StatusCode;
                result.Exception = ex;
            }
            catch (InvalidOperationException ex)
            {
                Debug.WriteLine($"{ex} |---> Invalid url: {url}");
                result.Success = false;
                result.StatusCode = null;
                result.Exception = ex;
            }
            catch (NotSupportedException ex)
            {
                Debug.WriteLine($"{ex} |---> Invalid url: {url}");
                result.Success = false;
                result.StatusCode = null;
                result.Exception = ex;
            }

            return result;
        }
        #endregion

        #region IComparable
        public int CompareTo(ScanData? other)
        {
            if(BookName != other.BookName)
            {
                return BookName.CompareTo(other.BookName);
            }
            else
            {
                return ChapterId.CompareTo(other.ChapterId);
            }
        }
        #endregion
    }
}
