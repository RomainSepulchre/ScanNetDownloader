using ScanNetDownloader.Logic.Helpers;
using ScanNetDownloader.View;
using ScanNetDownloader.View.CustomControls;
using System.Diagnostics;

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

                case string s when s.Contains(Constants.LELSCANS_DOMAIN_NAME):
                    return await LelScansNetScanData.IsUrlValid(url);

                default:
                    NotImplementedException exception = Exceptions.UnknownWebDomain(url);
                    Error.UnknownScanWebDomain(url, exception);
                    string invalidityReason = exception.Message;
                    return new UrlValidityResult(url, false, invalidityReason);
            }            
        }

        public static async Task<ScanData> CreateTemporaryScanData(string url)
        {
            switch (url)
            {
                case string s when s.Contains(Constants.SCANVF_DOMAIN_NAME):
                    return new ScanVfNetScanData(url);

                case string s when s.Contains(Constants.ANIMESAMA_DOMAIN_NAME):
                    AnimeSamaFrScanData tempAnimeSamaScanData = new AnimeSamaFrScanData(url);
                    await tempAnimeSamaScanData.GetBookNameFromHtmlContent();
                    return tempAnimeSamaScanData;

                case string s when s.Contains(Constants.LELSCANS_DOMAIN_NAME):
                    return new LelScansNetScanData(url);

                default: // Default, unknown domain name
                    NotImplementedException exception = Exceptions.UnknownWebDomain(url);
                    Error.UnknownScanWebDomain(url, exception, true); // Show pop-up because this should not happened at this point
                    return null;
            }
        }

        public static async Task<ScanDataInitResult> CreateNewScanData(string url, int chapterSelected, ScanData tempScanData)
        {
            switch (url)
            {
                case string s when s.Contains(Constants.SCANVF_DOMAIN_NAME):
                    ScanData scanVfNetData = new ScanVfNetScanData(url, chapterSelected, tempScanData.BookName);
                    return await scanVfNetData.InitScanData();

                case string s when s.Contains(Constants.ANIMESAMA_DOMAIN_NAME):
                    ScanData animeSamaData = new AnimeSamaFrScanData(url, chapterSelected, tempScanData.BookName);
                    return await animeSamaData.InitScanData();

                case string s when s.Contains(Constants.LELSCANS_DOMAIN_NAME):
                    ScanData lelScanNetData = new LelScansNetScanData(url, chapterSelected, tempScanData.BookName);
                    return await lelScanNetData.InitScanData();

                default: // Default, unknown domain name
                    NotImplementedException exception = Exceptions.UnknownWebDomain(url);
                    Error.UnknownScanWebDomain(url, exception, true);
                    ScanDataInitResult result = new ScanDataInitResult(null);
                    result.Success = false;
                    result.Exception = exception;
                    return result;
            }
        }


        // Probably useless now, kept for potential debug purpose
        public static async Task<List<ScanData>> CreateNewScanDatas(string url, List<int> chaptersSelected, ScanData tempScanData) 
        {
            bool errorOccured = false;
            List<ScanData> newScanDatas = new List<ScanData>();

            switch (url)
            {
                case string s when s.Contains(Constants.SCANVF_DOMAIN_NAME):
                    foreach (int chapterId in chaptersSelected)
                    {
                        ScanData scanVfNetData = new ScanVfNetScanData(url, chapterId, tempScanData.BookName);
                        ScanDataInitResult initResult = await scanVfNetData.InitScanData();
                        if (initResult.Success) newScanDatas.Add(scanVfNetData);
                        //TODO: Else Error Management
                    }
                    break;

                case string s when s.Contains(Constants.ANIMESAMA_DOMAIN_NAME):
                    foreach(int chapterId in chaptersSelected)
                    {
                        ScanData animeSamaData = new AnimeSamaFrScanData(url, chapterId, tempScanData.BookName);
                        ScanDataInitResult initResult = await animeSamaData.InitScanData();
                        if(initResult.Success) newScanDatas.Add(animeSamaData);
                        //TODO: Else Error Management
                    }
                    break;

                case string s when s.Contains(Constants.LELSCANS_DOMAIN_NAME):
                    foreach (int chapterId in chaptersSelected)
                    {
                        ScanData lelScanNetData = new LelScansNetScanData(url, chapterId, tempScanData.BookName);
                        ScanDataInitResult initResult = await lelScanNetData.InitScanData();
                        if (initResult.Success) newScanDatas.Add(lelScanNetData);
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

        public static bool IsADuplicate(ScanData data, List<ScanData> sortedList = null)
        {
            if(sortedList == null) // If sorted list is not specified automatically create an instance of the list and sort it
            {
                sortedList = new List<ScanData>(ScansLocalData.Instance.ScanDataList);
                sortedList.Sort();
            }

            int searchResult = sortedList.BinarySearch(data); // Binary search need to be called on sorted list

            return searchResult >= 0;
        }

        public static List<ScanData> CheckForDuplicate(List<ScanData> dataToCheck, out List<ScanData> scanToReplace)
        {
            scanToReplace = new List<ScanData>();

            // Get current scan data and sort them for binary search
            List<ScanData> sortedData = new List<ScanData>(ScansLocalData.Instance.ScanDataList);
            sortedData.Sort();

            // Check data for duplicate
            List<DuplicatedScanData> duplicateFound = ScanManagement.FindDuplicate(dataToCheck, sortedData);
            if(duplicateFound.Count > 0)
            {
                // Open window to let user choose what he wants to do
                DuplicateWindow duplicateWindow = MsgWindow.ShowDuplicateWindow(duplicateFound);

                // Process duplicate window result
                foreach (var item in duplicateWindow.DuplicateItems)
                {
                    switch (item.DuplicateOption)
                    {
                        case DuplicateItem.DuplicateOptions.Delete:
                            dataToCheck.Remove(item.duplicateScanData);
                            break;
                        case DuplicateItem.DuplicateOptions.Keep: // Don't do anything
                            break;
                        case DuplicateItem.DuplicateOptions.Replace:
                            scanToReplace.Add(item.currentScanData);
                            // TODO: Should I compare info to make sure we keep all useful info
                            // ex: location path
                            // issue if different website -> different page number (= missing or extra page) -> location path
                            // TODO: what to do if image already downloaded, should I delete them ?
                            break;
                        default:
                            Debug.WriteLine($"Unknown DuplicateOptions, nothing will be done with this duplicate");
                            break;
                    }
                }
            }

            return dataToCheck;
        }

        private static List<DuplicatedScanData> FindDuplicate(List<ScanData> dataToCheck, List<ScanData> sortedList = null)
        {
            if (sortedList == null) // If sorted list is not specified automatically create an instance of the list and sort it
            {
                sortedList = new List<ScanData>(ScansLocalData.Instance.ScanDataList);
                sortedList.Sort();
            }

            List<DuplicatedScanData> duplicateData = new List<DuplicatedScanData>();

            foreach (ScanData data in dataToCheck)
            {
                int searchResult = sortedList.BinarySearch(data); // Binary search need to be called on sorted list
                if (searchResult >= 0)
                {
                    DuplicatedScanData duplicatedData = new DuplicatedScanData(data, sortedList[searchResult]);
                    duplicateData.Add(duplicatedData);
                }
            }         
            
            return duplicateData;
        }
    }
}
