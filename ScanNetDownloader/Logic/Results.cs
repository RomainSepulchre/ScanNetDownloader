using System.Net;

namespace ScanNetDownloader.Logic
{
    public class UrlValidityResult
    {
        public string UrlTested;
        public bool Success = false;
        public string InvalidityReason;
        public Exception Exception = null;

        public UrlValidityResult(string urlToTest)
        {
            UrlTested = urlToTest;
            Success = false;
            InvalidityReason = "";
        }

        public UrlValidityResult(string urlToTest, bool isValid, string invalidityReason)
        {
            UrlTested = urlToTest;
            Success = isValid;
            InvalidityReason = invalidityReason;
        }
    }

    public class UrlLoadResult
    {
        public string UrlToLoad;
        public bool Success = false;
        public Exception Exception = null;
        public HttpStatusCode? StatusCode; //
                                           // : Default Status code ?

        public UrlLoadResult(string urlToLoad)
        {
            UrlToLoad = urlToLoad;
            Success = false;
        }
    }

    public class HtmlContentResult
    {
        public bool Success = false;
        public string HtmlContent = null;
        public Exception Exception = null;
        public HttpStatusCode? StatusCode;

        public HtmlContentResult()
        {
        }
    }

    public class ScanDataInitResult
    {
        public bool Success = false;
        public ScanData NewScanData;
        public Exception Exception = null;
        public HtmlContentResult HtmlContentResult = null;

        public ScanDataInitResult(ScanData _newScanData)
        {
            NewScanData = _newScanData;
        }

        public ScanDataInitResult(ScanData _newScanData, HtmlContentResult _htmlContentResult)
        {
            NewScanData = _newScanData;
            HtmlContentResult = _htmlContentResult;
        }
    }

    public class ScanDataImportResult
    {
        public bool Success = false;
        public Exception Exception = null;

        public ScanDataImportResult(bool success, Exception ex=null)
        {
            Success = success;
            Exception = ex;
        }
    }

    public class SettingsImportResult
    {
        public bool Success = false;
        public Exception Exception = null;

        public SettingsImportResult(bool success, Exception ex = null)
        {
            Success = success;
            Exception = ex;
        }
    }
}
