using System;
using System.Collections.Generic;
using System.IO.Compression;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ScanNetDownloader.View.CustomControls;

namespace ScanNetDownloader.Logic
{
    /// <summary>
    /// Handle the creation of the CbzArchive
    /// </summary>
    class CbzCreator
    {
        public static event EventHandler<ScanItem> OnCbzCreationStartEvent;

        public static event EventHandler<ScanItem> OnCbzCreatedEvent;

        public static event EventHandler<ScanItem> OnCbzAlreadyCreatedEvent;

        public static event EventHandler<CbzErrorEventArgs> OnCbzCreationErrorEvent;

        public static void BuildCbzArchive(ScanItem scanItem, string downloadPath)
        {
            OnCbzCreationStart(scanItem);

            ScanData scanData = scanItem.linkedScanData;

            string bookName = scanData.BookName;
            string chapterNumber = scanData.ChapterId.ToString();

            string folderToArchive = downloadPath;
            string cbzFilePath = Path.Combine(Directory.GetParent(downloadPath).FullName, $"{bookName}{Constants.CBZ_CHAPTER_PREFIX}{chapterNumber}{Constants.CBZ_EXTENSION}");

            if (Directory.EnumerateFileSystemEntries(folderToArchive).Any() == false)
            {
                Exception noImgEx = new Exception($"No images downloaded for {bookName}-{chapterNumber}, CBZ creation will be skipped!");
                OnCbzCreationError(scanItem, "Error while creating new cbz archive", noImgEx);
                return;
            }

            if (File.Exists(cbzFilePath) == false)
            {   
                try
                {
                    ZipFile.CreateFromDirectory(folderToArchive, cbzFilePath);
                    OnCbzCreated(scanItem);
                }
                catch (IOException ex)
                {
                    Error.FailedCbzCreation(ex, scanData, Settings.Instance.DeleteImagesAfterCbzCreation);
                    OnCbzCreationError(scanItem, "Error while creating new cbz archive", ex);
                    return;
                }
            }
            else // a cbz file already exist
            {
                if (File.ReadAllBytes(cbzFilePath).Length > 0)
                {
                    OnCbzAlreadyCreated(scanItem);
                }
                else // Replace empty Cbz
                {
                    try
                    {
                        File.Delete(cbzFilePath);
                        ZipFile.CreateFromDirectory(folderToArchive, cbzFilePath);
                        OnCbzCreated(scanItem);
                    }
                    catch (IOException ex)
                    {
                        Error.FailedToReplaceEmptyCbz(ex, scanData, Settings.Instance.DeleteImagesAfterCbzCreation);
                        OnCbzCreationError(scanItem, "Error while replacing empty cbz archive", ex);
                        return;
                    }
                }
            }

            if (Settings.Instance.DeleteImagesAfterCbzCreation)
            {
                if (Directory.Exists(downloadPath)) Directory.Delete(downloadPath, true);
            }
        }

        #region Events
        private static void OnCbzCreationStart(ScanItem scanItem)
        {
            if (OnCbzCreationStartEvent != null)
            {
                OnCbzCreationStartEvent(null, scanItem);
            }
        }

        private static void OnCbzCreated(ScanItem scanItem)
        {
            if (OnCbzCreatedEvent != null)
            {
                OnCbzCreatedEvent(null, scanItem);
            }
        }

        private static void OnCbzAlreadyCreated(ScanItem scanItem)
        {
            if (OnCbzAlreadyCreatedEvent != null)
            {
                OnCbzAlreadyCreatedEvent(null, scanItem);
            }
        }

        private static void OnCbzCreationError(ScanItem scanItem, string errorMsg, Exception ex)
        {
            if (OnCbzCreationErrorEvent != null)
            {
                CbzErrorEventArgs args = new CbzErrorEventArgs();
                args.ScanItem = scanItem;
                args.ErrorMessage = errorMsg;
                args.Exception = ex;

                OnCbzCreationErrorEvent(null, args);
            }
        }
        #endregion
    }

    public class CbzErrorEventArgs : EventArgs
    {
        public ScanItem ScanItem { get; set; }
        public string ErrorMessage { get; set; }
        public Exception Exception { get; set; }
    }
}
