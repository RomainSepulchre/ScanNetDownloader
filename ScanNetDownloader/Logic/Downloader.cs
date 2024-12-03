using ScanNetDownloader.View.CustomControls;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
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

        public static event EventHandler OnDownloadStartedEvent;

        public static event EventHandler<PageEventArgs> OnPageDownloadedEvent;

        public static event EventHandler<ScanItem> OnScanDownloadedEvent;

        public static event EventHandler<PageErrorEventArgs> OnPageDownloadErrorEvent;

        public static event EventHandler<PageErrorEventArgs> OnPageFileSavingErrorEvent;

        public static async void StartDownloader(List<ScanItem> scanItemsToDownload)
        {
            List<ScanData> scansToDownload = scanItemsToDownload.ToScanDataList();
            if (scanItemsToDownload.Count == 0)
            {
                // TODO: Prevent to click on start button if no scan are selected even before clicking calling this
                WriteDlInfoLine($"No scans have been selected, select at least a scan to start the download");
                return;
            }

            WriteDlInfoLine($"Here is the list of scans you are going to download:");
            foreach (ScanItem item in scanItemsToDownload)
            {
                WriteDlInfoLine($"-> {item.BookName} - {item.ChapterId} (source:{item.Url})");
            }

            WriteDlInfoLine($"The files will be downloaded in {Settings.Instance.OutputDirectory}, a folder will automatically be created for each title and chapters");
            if (FileManagement.OutputDirectoryIsValid() == false)
            {
                WriteDlInfoLine("\nDOWNLOAD STOPPED");
                return;
            }

            string mBoxMessage = "Do you to start the download ?";
            string mBoxCaption = "Continue ?";
            WriteDlInfoLine($"{mBoxMessage}");
            MessageBoxResult result = MessageBox.Show(mBoxMessage, mBoxCaption, MessageBoxButton.YesNo, MessageBoxImage.Question);

            WriteDlInfoLine($" YESNO RESULT = {result}");

            if (result == MessageBoxResult.No)
            {
                WriteDlInfoLine("DOWNLOAD STOPPED");
                return;
            }

            OnDownloadStarted();
            await DownloadScans(scanItemsToDownload);

            if (CurrentSettings.ErrorPauseApp)
            {
                mBoxMessage = "Finished, press any key to close...";
                mBoxCaption = "Finished";
                WriteDlInfoLine($"{mBoxMessage}");
                MessageBox.Show(mBoxMessage, mBoxCaption, MessageBoxButton.OK, MessageBoxImage.None);
            }
            else
            {
                Error.ShowDownloadErrors();
                mBoxMessage = "Finished with error, press any key to close...";
                mBoxCaption = "Finished";
                WriteDlInfoLine($"{mBoxMessage}");
                MessageBox.Show(mBoxMessage, mBoxCaption, MessageBoxButton.OK, MessageBoxImage.None);
            }

            if (CurrentSettings.OpenOutputDirectoryAfterDownload)
            {
                FileManagement.OpenRelevantFolder(scansToDownload);
            }
        }

        static async Task DownloadScans(List<ScanItem> scanItemsToDownload)
        {
            int NumberOfImagesToDownload = scanItemsToDownload.GetTotalOfScanPages();
            float progress = 0;
            float minProgress = 0;
            float maxProgress = 0;

            HttpClient client = HttpClientSingleton.Client;

            foreach (ScanItem scanItem in scanItemsToDownload)
            {
                ScanData scanData = scanItem.linkedScanData;
                if (scanData.IsTemporaryData)
                {
                    WriteDlInfoLine($"Nothing can be downloaded from a temporary scan data, it should not be possible to add a temporary scan to the download list");
                    continue;
                }

                minProgress = maxProgress;
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
                            byte[] img = await client.GetByteArrayAsync(imgUrl);
                            File.WriteAllBytes(downloadFile, img);
                            WriteDlInfoLine($"Sucessfully downloaded!\n");
                        }
                        OnPageDownloaded(scanItem, pageIndex);
                    }
                    catch (HttpRequestException ex)
                    {
                        OnPageDownloadError(scanItem, pageIndex, ex);
                        Error.FailedImageDownload(ex, imgUrl);
                    }
                    catch (IOException ex)
                    {
                        OnPageFileSavingError(scanItem, pageIndex, ex);
                        //TODO : Manage IO Eception
                    }

                    pageId++;
                }


                if (CurrentSettings.CreateCbzArchive)
                {
                    CbzCreator.BuildCbzArchive(scanItem, downloadPath);
                }

                // Deselect since we just downloaded it
                OnScanDownloaded(scanItem);
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

        private static void UpdateDownloadProgress(float percentageDone)
        {
            if (UpdateDlProgressBarEvent != null)
            {
                UpdateDlProgressBarEvent(null, percentageDone);
            }
        }

        private static void OnDownloadStarted()
        {
            if (OnDownloadStartedEvent != null)
            {
                OnDownloadStartedEvent(null, EventArgs.Empty);
            }
        }

        private static void OnPageDownloaded(ScanItem scanItem, int pageIndex)
        {
            if (OnPageDownloadedEvent != null)
            {
                PageEventArgs args = new PageEventArgs();
                args.ScanItem = scanItem;
                args.PageIndex = pageIndex;

                OnPageDownloadedEvent(null, args);
            }
        }

        private static void OnScanDownloaded(ScanItem scanDownloaded)
        {
            if (OnScanDownloadedEvent != null)
            {
                OnScanDownloadedEvent(null, scanDownloaded);
            }
        }

        private static void OnPageDownloadError(ScanItem scanItem, int pageIndex, Exception ex)
        {
            if (OnPageDownloadErrorEvent != null)
            {
                PageErrorEventArgs args = new PageErrorEventArgs();
                args.ScanItem = scanItem;
                args.PageIndex = pageIndex;
                args.Exception = ex;

                OnPageDownloadErrorEvent(null, args);
            }
        }

        private static void OnPageFileSavingError(ScanItem scanItem, int pageIndex, Exception ex)
        {
            if (OnPageFileSavingErrorEvent != null)
            {
                PageErrorEventArgs args = new PageErrorEventArgs();
                args.ScanItem = scanItem;
                args.PageIndex = pageIndex;
                args.Exception = ex;

                OnPageFileSavingErrorEvent(null, args);
            }
        }
        #endregion
    }

    public class PageEventArgs : EventArgs
    {
        public ScanItem ScanItem { get; set; }
        public int PageIndex { get; set; }
    }

    public class PageErrorEventArgs : EventArgs
    {
        public ScanItem ScanItem { get; set; }
        public int PageIndex { get; set; }
        public Exception Exception { get; set; }
    }
}
