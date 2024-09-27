using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace ScanNetDownloader
{
    public abstract class ScanWebsiteUrl
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

        public ScanWebsiteUrl(string url)
        {
            this.Url = url;
        }

        public List<string> GetScanImagesUrl()
        {
            string htmlContent;
            using (WebClient client = new WebClient())
            {
                try
                {
                    htmlContent = client.DownloadString(Url); // Save html code in a variable
                }
                catch (WebException ex)
                {
                    Error.FailedHtmlDownload(ex, Url);
                    return new List<string>();
                }
            }

            return ParseHtmlToGetImgLinks(htmlContent);
        }

        protected abstract List<string> ParseHtmlToGetImgLinks(string htmlContent);

        public abstract string GetBookNameFromUrl(string url, bool removeSpace=false);

        public abstract string GetChapterNumberFromUrl(string url, bool keepNumberOnly=true);

        public abstract string GetFileExtensionFromUrl(string url);

        public static bool IsUrlValid(string url, int timeout = 1000)
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
}
