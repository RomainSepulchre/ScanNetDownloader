using ScanNetDownloader.View;
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
        public static UrlValidityResult IsValidScanUrl(string url)
        {
            switch (url)
            {
                case string s when s.Contains(Constants.SCANVF_DOMAIN_NAME):
                    return ScanVfNetScanData.IsUrlValid(url);

                case string s when s.Contains(Constants.ANIMESAMA_DOMAIN_NAME):
                    return AnimeSamaFrScanData.IsUrlValid(url);

                default:
                    Error.UnknownScanWebDomain(url);
                    string invalidityReason = "This website is not compatible with ScanNetDownloader";
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
                    Error.UnknownScanWebDomain(url);
                    return null;
            }
        }

        public static async Task<ScanData> CreateNewScanData(string url, int chapterSelected)
        {
            bool errorOccured = false;
            ScanData newScanData =  null;
            bool initSuccess = false;

            switch (url)
            {
                case string s when s.Contains(Constants.SCANVF_DOMAIN_NAME):
                    ScanData scanVfNetData = new ScanVfNetScanData(url, chapterSelected);
                    initSuccess = await scanVfNetData.InitScanData();
                    if (initSuccess) newScanData = scanVfNetData;
                    break;

                case string s when s.Contains(Constants.ANIMESAMA_DOMAIN_NAME):
                    ScanData animeSamaData = new AnimeSamaFrScanData(url, chapterSelected);
                    initSuccess = await animeSamaData.InitScanData();
                    if (initSuccess) newScanData = animeSamaData;
                    break;

                default: // Default, unknown domain name
                    errorOccured = true;
                    Error.UnknownScanWebDomain(url);
                    break;
            }

            if (errorOccured)
            {
                string mBoxMessage = "Make sure to check the errors and press any key to continue...";
                string mBoxCaption = "Check errrors";
                Debug.WriteLine(mBoxMessage);
                MessageBox.Show(mBoxMessage, mBoxCaption, MessageBoxButton.OK, MessageBoxImage.Warning);
            }

            return newScanData;
        }

        public static async Task<List<ScanData>> CreateNewScanDatas(string url, List<int> chaptersSelected)
        {
            bool errorOccured = false;
            List<ScanData> newScanDatas = new List<ScanData>();

            switch (url)
            {
                case string s when s.Contains(Constants.SCANVF_DOMAIN_NAME):

                    foreach (int chapterId in chaptersSelected)
                    {
                        ScanData scanVfNetData = new ScanVfNetScanData(url, chapterId);
                        bool initSuccess = await scanVfNetData.InitScanData();
                        if (initSuccess) newScanDatas.Add(scanVfNetData);
                    }
                    break;

                case string s when s.Contains(Constants.ANIMESAMA_DOMAIN_NAME):

                    foreach(int chapterId in chaptersSelected)
                    {
                        ScanData animeSamaData = new AnimeSamaFrScanData(url, chapterId);
                        bool initSuccess = await animeSamaData.InitScanData();
                        if(initSuccess) newScanDatas.Add(animeSamaData);
                    }
                    break;

                default: // Default, unknown domain name
                    errorOccured = true;
                    Error.UnknownScanWebDomain(url);
                    break;
            }

            if (errorOccured)
            {
                string mBoxMessage = "Make sure to check the errors and press any key to continue...";
                string mBoxCaption = "Check errrors";
                Debug.WriteLine(mBoxMessage);
                MessageBox.Show(mBoxMessage, mBoxCaption, MessageBoxButton.OK, MessageBoxImage.Warning);
            }

            return newScanDatas;
        }

        // TODO: Clean obsolete functions once i'm sure nothing in them will be needed
        [Obsolete]
        public static List<ScanData> CreateNewScanData(string urlEntered, string chaptersEntered)
        {
            bool errorOccured = false;
            List<ScanData> newScanDatas = new List<ScanData>();
            List<int> selectedChaptersId;

            Debug.WriteLine($"\nURL ==> {urlEntered}\n");
            switch (urlEntered)
            {
                case string s when s.Contains(Constants.SCANVF_DOMAIN_NAME):
                    //https://www.scan-vf.net/jujutsu-kaisen/chapitre-164/1 = url with chapter -> at least 5 split
                    //https://www.scan-vf.net/jujutsu-kaisen = url without chapter -> less than 5 split
                    bool chapterIsInUrl = urlEntered.Split(Constants.SLASH_CHAR).Count() > 4; // Check if the url has a chapter name (the number of split let us know if url stop at book name or not)
                    if (chapterIsInUrl)
                    {
                        ScanData scanVfNetData = new ScanVfNetScanData(urlEntered);
                        newScanDatas.Add(scanVfNetData);
                        Debug.WriteLine($"{scanVfNetData.BookName} - Chapter {scanVfNetData.ChapterId} added ({scanVfNetData.WebsiteDomain}).\n");
                    }
                    else
                    {
                        // Chapters to download
                        ScanData temporaryScanVfNetData = new ScanVfNetScanData(urlEntered);
                        selectedChaptersId = ChapterSelection(temporaryScanVfNetData, chaptersEntered, ref errorOccured);
                        // Create link
                        foreach (int chapterId in selectedChaptersId)
                        {
                            string urlWithChapter = temporaryScanVfNetData.GenerateAnotherChapterUrl(chapterId); // TODO: Test this
                            //string urlWithChapter = $"{urlEntered}{Constants.SCANVF_CHAPTER_IN_URL}{chapterId}"; // No need to specify "/1" after chapter number redirection is done by website
                            ScanData scanVfNetData = new ScanVfNetScanData(urlWithChapter);
                            newScanDatas.Add(scanVfNetData);
                            // TODO: we never check if chapter exist with ScanVf ? Check this before creating ScanData
                            Debug.WriteLine($" -> {scanVfNetData.BookName} - Chapter {scanVfNetData.ChapterId} added ({scanVfNetData.WebsiteDomain}).");
                        }
                        Debug.WriteLine("");
                    }
                    break;

                case string s when s.Contains(Constants.ANIMESAMA_DOMAIN_NAME):
                    ScanData temporaryAnimeSamaData = new AnimeSamaFrScanData(urlEntered); // Temporary obj to get book name

                    // TODO: If possible manage Book URL instead of chapter url
                    // -> check if chapter url is stable or if it changes too much

                    // Get chapters to download
                    selectedChaptersId = ChapterSelection(temporaryAnimeSamaData, chaptersEntered, ref errorOccured);

                    // Create link
                    foreach (int chapterId in selectedChaptersId)
                    {
                        ScanData animeSamaData = new AnimeSamaFrScanData(urlEntered, chapterId);
                        newScanDatas.Add(animeSamaData);
                        Debug.WriteLine($" -> {animeSamaData.BookName} - Chapter {animeSamaData.ChapterId} added ({animeSamaData.WebsiteDomain}).");
                    }
                    Debug.WriteLine("");
                    break;

                default: // Default, unknown domain name
                    errorOccured = true;
                    Error.UnknownScanWebDomain(urlEntered);
                    break;
            }

            if (errorOccured)
            {
                string mBoxMessage = "Make sure to check the errors and press any key to continue...";
                string mBoxCaption = "Check errrors";
                Debug.WriteLine(mBoxMessage);
                MessageBox.Show(mBoxMessage, mBoxCaption, MessageBoxButton.OK, MessageBoxImage.Warning);
            }

            return newScanDatas;
        }

        [Obsolete]
        private static List<int> ChapterSelection(ScanData scanData, string enteredChapters, ref bool errorOccured)
        {
            List<int> selectedChaptersId = new List<int>();

            if (string.IsNullOrEmpty(enteredChapters) == false)
            {
                selectedChaptersId = ParseToFindChapters(scanData, enteredChapters, ref errorOccured);
                if (selectedChaptersId.Count <= 0)
                {
                    selectedChaptersId = AskUserToProvideChapters(scanData, ref errorOccured);
                }
                else
                {
                    selectedChaptersId.Log();
                }
            }
            else
            {
                selectedChaptersId = AskUserToProvideChapters(scanData, ref errorOccured);
            }
            selectedChaptersId.Sort();
            return selectedChaptersId;
        }

        // TODO : Should not be useful anymore after finishing the addScan Pop-Up
        [Obsolete]
        private static List<int> AskUserToProvideChapters(ScanData scanData, ref bool errorOccured) // TODO: maybe this could be in a class dedicated to pop up ?
        {
            MainWindow mainWindow = (MainWindow)Application.Current.MainWindow;

            Debug.WriteLine($"Select chapters for \"{scanData.BookName}\" ({scanData.Url}):");
            Debug.WriteLine($"Write a range of chapter (ex: 1-10) or the number of the chapters you want to download separated by ; (ex:4;6;8) and press enter.");

            InputPopUp inputPopUp = new InputPopUp(mainWindow, $"Select chapters for \"{scanData.BookName}\" ({scanData.Url}):");
            mainWindow.Opacity = 0.4;
            inputPopUp.ShowDialog();
            mainWindow.Opacity = 1;

            string userTxtInput = inputPopUp.Input;

            List<int> chaptersFound = ParseToFindChapters(scanData, userTxtInput, ref errorOccured);
            return chaptersFound;

        }

        [Obsolete]
        private static List<int> ParseToFindChapters(ScanData scanData, string stringToParse, ref bool errorOccured)
        {
            List<int> validChapters = new List<int>();
            List<string> chaptersEnteredByUser = stringToParse.Split(Constants.SEMICOLON_CHAR).ToList();

            for (int i = chaptersEnteredByUser.Count - 1; i >= 0; i--)
            {
                // Detect range of chapter
                string[] rangeSplitAttempt = chaptersEnteredByUser[i].Split(Constants.DASH_CHAR);
                if (rangeSplitAttempt.Count() == 2) // Range of chapter
                {
                    // Get start and end of range
                    bool startParsed = int.TryParse(rangeSplitAttempt[0], out int startRange);
                    bool endParsed = int.TryParse(rangeSplitAttempt[1], out int endRange);

                    if (startParsed == false || endParsed == false)
                    {
                        errorOccured = true;
                        Error.FailedToParseChapterEnteredByUser(scanData, chaptersEnteredByUser[i]);
                        continue;
                    }
                    else
                    {
                        if (startRange > endRange) (startRange, endRange) = (endRange, startRange); // invert

                        for (int j = startRange; j <= endRange; j++)
                        {
                            validChapters.Add(j);
                        }
                    }
                }
                else // Single chapter
                {
                    if (int.TryParse(chaptersEnteredByUser[i], out int chapterId))
                    {
                        validChapters.Add(chapterId);
                    }
                    else
                    {
                        errorOccured = true;
                        Error.FailedToParseChapterEnteredByUser(scanData, chaptersEnteredByUser[i]);
                    }
                }
            }

            return validChapters;
        }
    }
}
