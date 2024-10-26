using ScanNetDownloader.View;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace ScanNetDownloader.ConsoleApp
{
    class ScanManagement
    {
        public static List<ScanWebsiteUrl> CreateNewScanWebsiteUrls(string urlEntered, string chaptersEntered)
        {
            bool errorOccured = false;
            List<ScanWebsiteUrl> newScanWebsiteUrls = new List<ScanWebsiteUrl>();
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
                        ScanWebsiteUrl scanVfNetUrl = new ScanVfNetUrl(urlEntered);
                        newScanWebsiteUrls.Add(scanVfNetUrl);
                        Debug.WriteLine($"{scanVfNetUrl.BookName} - Chapter {scanVfNetUrl.ChapterId} added ({scanVfNetUrl.WebsiteDomain}).\n");
                    }
                    else
                    {
                        // Chapters to download
                        ScanWebsiteUrl temporaryScanVfNetUrl = new ScanVfNetUrl(urlEntered, false);
                        selectedChaptersId = ChapterSelection(temporaryScanVfNetUrl, chaptersEntered, ref errorOccured);
                        // Create link
                        foreach (int chapterId in selectedChaptersId)
                        {
                            string urlWithChapter = $"{urlEntered}{Constants.SCANVF_CHAPTER_IN_URL}{chapterId}"; // No need to specify "/1" after chapter number redirection is done by website
                            ScanWebsiteUrl scanVfNetUrl = new ScanVfNetUrl(urlWithChapter);
                            newScanWebsiteUrls.Add(scanVfNetUrl);
                            // TODO: we never check if chapter exist with ScanVf ? Check this before creating ScanWebsiteUrl
                            Debug.WriteLine($" -> {scanVfNetUrl.BookName} - Chapter {scanVfNetUrl.ChapterId} added ({scanVfNetUrl.WebsiteDomain}).");
                        }
                        Debug.WriteLine("");
                    }
                    break;

                case string s when s.Contains(Constants.ANIMESAMA_DOMAIN_NAME):
                    ScanWebsiteUrl temporaryAnimeSamaUrl = new AnimeSamaFrUrl(urlEntered); // Temporary obj to get book name

                    // TODO: If possible manage Book URL instead of chapter url
                    // -> check if chapter url is stable or if it changes too much

                    // Get chapters to download
                    selectedChaptersId = ChapterSelection(temporaryAnimeSamaUrl, chaptersEntered, ref errorOccured);

                    // Create link
                    foreach (int chapterId in selectedChaptersId)
                    {
                        ScanWebsiteUrl animeSamaUrl = new AnimeSamaFrUrl(urlEntered, chapterId);
                        newScanWebsiteUrls.Add(animeSamaUrl);
                        Debug.WriteLine($" -> {animeSamaUrl.BookName} - Chapter {animeSamaUrl.ChapterId} added ({animeSamaUrl.WebsiteDomain}).");
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
                Debug.WriteLine($"Make sure to check the errors and press any key to continue...");
                MessageBox.Show("Make sure to check the errors and press any key to continue...", "Check errrors", MessageBoxButton.OK, MessageBoxImage.Warning);
            }

            return newScanWebsiteUrls;
        }

        private static List<int> ChapterSelection(ScanWebsiteUrl scanUrl, string enteredChapters, ref bool errorOccured)
        {
            List<int> selectedChaptersId = new List<int>();

            if (string.IsNullOrEmpty(enteredChapters) == false)
            {
                selectedChaptersId = ParseToFindChapters(scanUrl, enteredChapters, ref errorOccured);
                if (selectedChaptersId.Count <= 0)
                {
                    selectedChaptersId = AskUserToProvideChapters(scanUrl, ref errorOccured);
                }
                else
                {
                    selectedChaptersId.Log();
                }
            }
            else
            {
                selectedChaptersId = AskUserToProvideChapters(scanUrl, ref errorOccured);
            }
            selectedChaptersId.Sort();
            return selectedChaptersId;
        }

        // TODO : Should not be useful anymore after finishing the addScan Pop Up
        private static List<int> AskUserToProvideChapters(ScanWebsiteUrl scanUrl, ref bool errorOccured) // TODO: maybe this could be in a class dedicated to pop up ?
        {
            MainWindow mainWindow = (MainWindow)Application.Current.MainWindow;

            Debug.WriteLine($"Select chapters for \"{scanUrl.BookName}\" ({scanUrl.Url}):");
            Debug.WriteLine($"Write a range of chapter (ex: 1-10) or the number of the chapters you want to download separated by ; (ex:4;6;8) and press enter.");

            InputPopUp inputPopUp = new InputPopUp(mainWindow, $"Select chapters for \"{scanUrl.BookName}\" ({scanUrl.Url}):");
            mainWindow.Opacity = 0.4;
            inputPopUp.ShowDialog();
            mainWindow.Opacity = 1;

            string userTxtInput = inputPopUp.Input;

            List<int> chaptersFound = ParseToFindChapters(scanUrl, userTxtInput, ref errorOccured);
            return chaptersFound;

        }

        private static List<int> ParseToFindChapters(ScanWebsiteUrl scanUrl, string stringToParse, ref bool errorOccured)
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
                        Error.FailedToParseChapterEnteredByUser(scanUrl, chaptersEnteredByUser[i]);
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
                        Error.FailedToParseChapterEnteredByUser(scanUrl, chaptersEnteredByUser[i]);
                    }
                }
            }

            return validChapters;
        }
    }
}
