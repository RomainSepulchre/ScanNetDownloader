using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScanNetDownloader
{
    public class ScanVfNet : ScanWebsiteUrl
    {
        public ScanVfNet(string url) : base(url)
        {
            this.url = url;
            WebsiteName = "scan-vf.net";
        }

        public override string GetBookNameFromUrl(string url, bool removeSpace = false)
        {
            throw new NotImplementedException();
        }

        public override string GetChapterNumberFromUrl(string url, bool keepNumberOnly = true)
        {
            throw new NotImplementedException();
        }

        public override string GetFileExtensionFromUrl(string url)
        {
            throw new NotImplementedException();
        }

        public override string ParseHtmlToGetImgLinks(string httmlContent, string htmlUrl)
        {
            throw new NotImplementedException();
        }
    }
}
