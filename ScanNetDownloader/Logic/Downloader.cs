using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace ScanNetDownloader.Logic
{
    /// <summary>
    /// Manage 
    /// </summary>
    class Downloader
    {
        private static Settings CurrentSettings => Settings.Instance;

        // Events
        public static event EventHandler<string> DlInfoWriteLineEvent;

        public static event EventHandler<float> UpdateDlProgressBarEvent;

        public static event EventHandler<ScanData> ScanDownloadedEvent;

        public static async void StartDownloader(List<ScanData> scansToDownload)
        {
            if (scansToDownload.Count == 0)
            {
                // TODO: Prevent to click on start button if no scan are selected even before clicking calling this
                WriteDlInfoLine($"No scans have been selected, select at least a scan to start the download");
                return;
            }

            WriteDlInfoLine($"\nHere is the list of scans you are going to download:");
            foreach (ScanData item in scansToDownload)
            {
                WriteDlInfoLine($"-> {item.BookName} - {item.ChapterId} (source:{item.Url})");
            }

            WriteDlInfoLine($"\nThe files will be downloaded in {Settings.Instance.OutputDirectory}, a folder will automatically be created for each title and chapters");
            if (FileManagement.OutputDirectoryIsValid() == false)
            {
                WriteDlInfoLine("\nDOWNLOAD STOPPED");
                return;
            }

            string mBoxMessage = "Do you to start the download ?";
            string mBoxCaption = "Continue ?";
            WriteDlInfoLine($"\n{mBoxMessage}");
            MessageBoxResult result = MessageBox.Show(mBoxMessage, mBoxCaption, MessageBoxButton.YesNo, MessageBoxImage.Question);

            WriteDlInfoLine($" YESNO RESULT = {result}");

            if (result == MessageBoxResult.No)
            {
                WriteDlInfoLine("\nDOWNLOAD STOPPED");
                return;
            }

            await DownloadScans(scansToDownload);

            if (CurrentSettings.ErrorPauseApp)
            {
                mBoxMessage = "Finished, press any key to close...";
                mBoxCaption = "Finished";
                WriteDlInfoLine($"\n{mBoxMessage}");
                MessageBox.Show(mBoxMessage, mBoxCaption, MessageBoxButton.OK, MessageBoxImage.None);
            }
            else
            {
                Error.ShowDownloadErrors();
                mBoxMessage = "Finished with error, press any key to close...";
                mBoxCaption = "Finished";
                WriteDlInfoLine($"\n{mBoxMessage}");
                MessageBox.Show(mBoxMessage, mBoxCaption, MessageBoxButton.OK, MessageBoxImage.None);
            }

            if (CurrentSettings.OpenOutputDirectoryAfterDownload)
            {
                FileManagement.OpenRelevantFolder(scansToDownload);
            }
        }

        static async Task DownloadScans(List<ScanData> scansToDownload)
        {
            int NumberOfImagesToDownload = scansToDownload.GetTotalOfScanPages();
            float progress = 0;
            float minProgress = 0;
            float maxProgress = 0;

            foreach (ScanData scanData in scansToDownload)
            {
                if (scanData.IsTemporaryData)
                {
                    WriteDlInfoLine($"Nothing can be downloaded from a temporary scan data, it should not be possible to add a temporary scan to the download list");
                    continue;
                }

                minProgress = maxProgress;
                int scanIndex = scansToDownload.IndexOf(scanData);
                maxProgress = minProgress + ((((float)scanData.PagesCount) / (NumberOfImagesToDownload)) * 100);


                string url = scanData.Url;
                string bookName = scanData.BookName;
                string chapterNumber = scanData.ChapterId.ToString();

                string header = $"Download {bookName} - chapter {chapterNumber} from {url}";

                WriteDlInfoLine($"\nLook for images url for {bookName}-{chapterNumber} at {url}...");
                List<string> imgsToDownload = scanData.PagesUrl;
                if (imgsToDownload.Count == 0) { continue; } // if list is empty (in case of error while getting html content) skip directly to the next url

                // Create output folder if necessary
                string downloadPath = FileManagement.CreateChapterDirectory(bookName, chapterNumber);

                int pageId = 1;
                foreach (string imgUrl in imgsToDownload)
                {
                    int pageIndex = imgsToDownload.IndexOf(imgUrl);
                    float chapterCompletion = (float)pageIndex / (imgsToDownload.Count - 1);
                    progress = float.Lerp(minProgress, maxProgress, chapterCompletion);
                    UpdateDownloadProgress(progress);

                    string fileExtension = scanData.GetFileExtensionFromImgUrl(imgUrl);
                    string imgName = $"{bookName}_{chapterNumber}-{pageId.ToString("D3")}{fileExtension}";
                    string downloadFile = Path.Combine(downloadPath, imgName);


                    using (WebClient client = new WebClient())
                    {
                        try
                        {
                            WriteDlInfoLine($"\nDownloading {imgName} from {imgUrl}");
                            WriteDlInfoLine($"...");

                            if (File.Exists(downloadFile) == true && File.ReadAllBytes(downloadFile).Length > 0 == true)
                            {
                                WriteDlInfoLine($"File already downloaded!\n");
                            }
                            else
                            {
                                await client.DownloadFileTaskAsync(new Uri(imgUrl), downloadFile);
                                WriteDlInfoLine($"Sucessfully downloaded!\n");
                            }
                        }
                        catch (WebException ex)
                        {
                            Error.FailedImageDownload(ex, imgUrl);
                        }
                    }
                    pageId++;
                }

                if (CurrentSettings.CreateCbzArchive)
                {
                    CbzCreator.BuildCbzArchive(scanData, downloadPath);
                }

                // Deselect since we just downloaded it
                OnScanDownloaded(scanData);
            }
        }

        #region Events
        public static void WriteDlInfoLine(string lineToAdd) // TODO: Do better than this ? Class decicated to eventsHandler that anyone can call ? Status/DlInfo class ?
        {
            Debug.WriteLine(lineToAdd);
            if (DlInfoWriteLineEvent != null)
            {
                DlInfoWriteLineEvent(null, lineToAdd);
            }
        }

        public static void UpdateDownloadProgress(float percentageDone)
        {
            if (UpdateDlProgressBarEvent != null)
            {
                UpdateDlProgressBarEvent(null, percentageDone);
            }
        }

        public static void OnScanDownloaded(ScanData unselectedScan)
        {
            if (ScanDownloadedEvent != null)
            {
                ScanDownloadedEvent(null, unselectedScan);
            }
        }
        #endregion
    }
}
