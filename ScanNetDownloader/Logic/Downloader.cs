using ScanNetDownloader.View.CustomControls;
using ScanNetDownloader.Logic.Helpers;
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
        public static event EventHandler<float> UpdateDlProgressBarEvent;

        public static event EventHandler OnDownloadStartedEvent;

        public static event EventHandler OnDownloadStoppedEvent;

        public static event EventHandler<PageEventArgs> OnPageDownloadedEvent;

        public static event EventHandler<PageEventArgs> OnPageAlreadyDownloadedEvent;

        public static event EventHandler<ScanItem> OnScanDownloadedEvent;

        public static event EventHandler<ScanErrorEventArgs> OnScanDownloadErrorEvent;

        public static event EventHandler<PageErrorEventArgs> OnPageDownloadErrorEvent;

        public static event EventHandler<PageErrorEventArgs> OnPageFileSavingErrorEvent;

        public static event EventHandler OnDownloadsFinishedEvent;

        public static async void StartDownloader(List<ScanItem> scanItemsToDownload)
        {
            List<ScanData> scansToDownload = scanItemsToDownload.ToScanDataList();

            if (FileManagement.OutputDirectoryIsValid() == false)
            {
                OnDownloadStopped();
                return;
            }

            string mBoxMessage = $"The files will be downloaded in {Settings.Instance.OutputDirectory}.\n\nDo you want to start the download ?";
            string mBoxCaption = "Continue ?";
            MessageBoxResult result = MessageBox.Show(mBoxMessage, mBoxCaption, MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.No)
            {
                OnDownloadStopped();
                return;
            }

            await DownloadScans(scanItemsToDownload);

            if (CurrentSettings.OpenOutputDirectoryAfterDownload)
            {
                FileManagement.OpenRelevantFolder(scansToDownload);
            }

            OnDownloadsFinished();
        }

        static async Task DownloadScans(List<ScanItem> scanItemsToDownload)
        {
            OnDownloadStarted();

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
                    Exception tempDataEx= new Exception("Nothing can be downloaded from a temporary scan data, it should not be possible to add a temporary scan to the download list");
                    OnScanDownloadError(scanItem, tempDataEx);
                    continue;
                }

                minProgress = maxProgress;
                maxProgress = minProgress + ((((float)scanData.PagesCount) / (NumberOfImagesToDownload)) * 100);

                string url = scanData.Url;
                string bookName = scanData.BookName;
                string chapterNumber = scanData.ChapterId.ToString();

                string header = $"Download {bookName} - chapter {chapterNumber} from {url}";

                List<string> imgsToDownload = scanData.PagesUrl;
                if (imgsToDownload.Count == 0) // if list is empty skip directly to the next scan
                {
                    Exception NoImgUrlEx = new Exception("No image url has been found for this scan");
                    OnScanDownloadError(scanItem, NoImgUrlEx);
                    continue;
                } 

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
                        if (File.Exists(downloadFile) == true && File.ReadAllBytes(downloadFile).Length > 0 == true)
                        {
                            OnPageAlreadyDownloaded(scanItem, pageIndex);
                        }
                        else
                        {
                            byte[] img = await client.GetByteArrayAsync(imgUrl);
                            File.WriteAllBytes(downloadFile, img);
                            OnPageDownloaded(scanItem, pageIndex);
                        }    
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

        private static void OnDownloadStopped()
        {
            if (OnDownloadStoppedEvent != null)
            {
                OnDownloadStoppedEvent(null, EventArgs.Empty);
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

        private static void OnPageAlreadyDownloaded(ScanItem scanItem, int pageIndex)
        {
            if(OnPageAlreadyDownloadedEvent != null)
            {
                PageEventArgs args = new PageEventArgs();
                args.ScanItem = scanItem;
                args.PageIndex = pageIndex;

                OnPageAlreadyDownloadedEvent(null, args);
            }
        }

        private static void OnScanDownloaded(ScanItem scanDownloaded)
        {
            if (OnScanDownloadedEvent != null)
            {
                OnScanDownloadedEvent(null, scanDownloaded);
            }
        }

        private static void OnScanDownloadError(ScanItem scanWithError, Exception ex)
        {
            if (OnScanDownloadErrorEvent != null)
            {
                ScanErrorEventArgs args = new ScanErrorEventArgs();
                args.ScanItem = scanWithError;
                args.Exception = ex;

                OnScanDownloadErrorEvent(null, args);
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

        private static void OnDownloadsFinished()
        {
            if(OnDownloadsFinishedEvent != null)
            {
                OnDownloadsFinishedEvent(null, EventArgs.Empty);
            }
        }
        #endregion
    }

    public class ScanErrorEventArgs : EventArgs
    {
        public ScanItem ScanItem { get; set; }
        public Exception Exception { get; set; }
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
