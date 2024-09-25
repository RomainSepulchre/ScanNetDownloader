using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScanNetDownloader
{
    public abstract class ScanWebsiteUrl
    {
        public string url;

        public string WebsiteName
        {
             get; protected set;
        }

        public ScanWebsiteUrl(string url)
        {
            this.url = url;
        }

        public abstract string ParseHtmlToGetImgLinks(string httmlContent, string htmlUrl);

        public abstract string GetBookNameFromUrl(string url, bool removeSpace=false);

        public abstract string GetChapterNumberFromUrl(string url, bool keepNumberOnly=true);

        public abstract string GetFileExtensionFromUrl(string url);

    }
}
