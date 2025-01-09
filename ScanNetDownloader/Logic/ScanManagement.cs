using ScanNetDownloader.View;
using ScanNetDownloader.Logic.Helpers;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace ScanNetDownloader.Logic
{
    /// <summary>
    /// Handle the creation of ScanData and their management
    /// </summary>
    class ScanManagement
    {
        public static async Task<UrlValidityResult> IsValidScanUrl(string url)
        {
            switch (url)
            {
                case string s when s.Contains(Constants.SCANVF_DOMAIN_NAME):
                    return await ScanVfNetScanData.IsUrlValid(url);

                case string s when s.Contains(Constants.ANIMESAMA_DOMAIN_NAME):
                    return await AnimeSamaFrScanData.IsUrlValid(url);

                default:
                    NotImplementedException exception = Exceptions.UnknownWebDomain(url);
                    Error.UnknownScanWebDomain(url, exception);
                    string invalidityReason = exception.Message;
                    return new UrlValidityResult(url, false, invalidityReason);
            }            
        }

        public static ScanData CreateTemporaryScanData(string url)
        {
            switch (url)
            {
                case string s when s.Contains(Constants.SCANVF_DOMAIN_NAME):
                    return new ScanVfNetScanData(url);

                case string s when s.Contains(Constants.ANIMESAMA_DOMAIN_NAME):
                    return new AnimeSamaFrScanData(url);

                default: // Default, unknown domain name
                    NotImplementedException exception = Exceptions.UnknownWebDomain(url);
                    Error.UnknownScanWebDomain(url, exception, true); // Show pop-up because this should not happened at this point
                    return null;
            }
        }

        public static async Task<ScanDataInitResult> CreateNewScanData(string url, int chapterSelected)
        {
            switch (url)
            {
                case string s when s.Contains(Constants.SCANVF_DOMAIN_NAME):
                    ScanData scanVfNetData = new ScanVfNetScanData(url, chapterSelected);
                    return await scanVfNetData.InitScanData();

                case string s when s.Contains(Constants.ANIMESAMA_DOMAIN_NAME):
                    ScanData animeSamaData = new AnimeSamaFrScanData(url, chapterSelected);
                    return await animeSamaData.InitScanData();

                default: // Default, unknown domain name
                    NotImplementedException exception = Exceptions.UnknownWebDomain(url);
                    Error.UnknownScanWebDomain(url, exception, true);
                    ScanDataInitResult result = new ScanDataInitResult(null);
                    result.Success = false;
                    result.Exception = exception;
                    return result;
            }
        }

        public static async Task<List<ScanData>> CreateNewScanDatas(string url, List<int> chaptersSelected) // Probably useless now, kept for potential debug purpose
        {
            bool errorOccured = false;
            List<ScanData> newScanDatas = new List<ScanData>();

            switch (url)
            {
                case string s when s.Contains(Constants.SCANVF_DOMAIN_NAME):

                    foreach (int chapterId in chaptersSelected)
                    {
                        ScanData scanVfNetData = new ScanVfNetScanData(url, chapterId);
                        ScanDataInitResult initResult = await scanVfNetData.InitScanData();
                        if (initResult.Success) newScanDatas.Add(scanVfNetData);
                        //TODO: Else Error Management
                    }
                    break;

                case string s when s.Contains(Constants.ANIMESAMA_DOMAIN_NAME):

                    foreach(int chapterId in chaptersSelected)
                    {
                        ScanData animeSamaData = new AnimeSamaFrScanData(url, chapterId);
                        ScanDataInitResult initResult = await animeSamaData.InitScanData();
                        if(initResult.Success) newScanDatas.Add(animeSamaData);
                        //TODO: Else Error Management
                    }
                    break;

                default: // Default, unknown domain name
                    errorOccured = true;
                    NotImplementedException exception = Exceptions.UnknownWebDomain(url);
                    Error.UnknownScanWebDomain(url, exception, true);
                    break;
            }

            if (errorOccured)
            {
                string mBoxMessage = "Make sure to check the errors and press any key to continue...";
                string mBoxCaption = "Check errrors";
                Debug.WriteLine(mBoxMessage);
                MsgWindow.ShowOkWindow(mBoxCaption, mBoxMessage, false, MsgWindow.ImageType.Warning);
            }

            return newScanDatas;
        }
    }
}
