using ScanNetDownloader.View.CustomControls;
using System.IO;
using System.IO.Compression;

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

        public static void BuildCbzArchive(ScanItem scanItem)
        {
            OnCbzCreationStart(scanItem);

            ScanData scanData = scanItem.linkedScanData;

            string folderToArchive = scanData.LocationPath;
            string bookName = scanData.BookName;
            string chapterNumber = scanData.ChapterId.ToString();

            if (string.IsNullOrEmpty(folderToArchive))
            {
                Exception noLocationPathEx = new Exception($"Location path for {bookName}-{chapterNumber} is null or empty. This should only happen when the scan was never downloaded in the first place. CBZ creation will be skipped!");
                OnCbzCreationError(scanItem, "Error while creating new cbz archive", noLocationPathEx);
                return;
            }

            string cbzFolderPath = Path.Combine(folderToArchive, ".."); // Get parent
            string cbzFilePath = Path.Combine(cbzFolderPath, $"{bookName}{Constants.CBZ_CHAPTER_PREFIX}{chapterNumber}{Constants.CBZ_EXTENSION}");

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
                    string errorMsg = "Error while creating new cbz archive";
                    if (Settings.Instance.DeleteImagesAfterCbzCreation) errorMsg += ", the scan images won't be deleted so you can create the CBZ manually";
                    OnCbzCreationError(scanItem, errorMsg, ex);
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
                        string errorMsg = "Error while replacing empty cbz archive";
                        if (Settings.Instance.DeleteImagesAfterCbzCreation) errorMsg += ", the scan images won't be deleted so you can create the CBZ manually";
                        OnCbzCreationError(scanItem, errorMsg, ex);
                        return;
                    }
                }
            }

            if (Settings.Instance.DeleteImagesAfterCbzCreation)
            {
                if (Directory.Exists(folderToArchive)) Directory.Delete(folderToArchive, true);
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
