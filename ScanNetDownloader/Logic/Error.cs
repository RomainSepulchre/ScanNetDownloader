using ScanNetDownloader.Logic.Helpers;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace ScanNetDownloader.Logic
{
    public class Error
    {
        public enum ErrorType
        {
            None = 0,
            UnknownScanWebDomain = 1, // ScanData Creation
            NoOutputDirectory = 2, // Download
            FailedHtmlDownload = 3, // ScanData Creation
            FailedImageDownload = 4, // Download
            FailedCbzCreation = 5, // Download
            FailedToReplaceEmptyCbz = 6, // Download
            ChapterDoesntExist = 7, // ScanData Creation
            MissingSettingsJson = 8, // Local File
            FailedToLoadSettingsJson = 9, // Local File
            FailedToSaveSettingsJson = 10, // Local File
            FailedToLoadScansLocalData = 11, // Local File
            FailedToSaveScansLocalData = 12, // Local File
            MissingScanDataJson = 13 // Local File
        }

        public DateTime Time
        {
            get; private set;
        }

        public string Message
        {
            get; private set;
        }

        public ErrorType Type
        {
            get; private set;
        }

        public Exception? Exception
        {
            get; private set;
        }

        public Error()
        {
            Time = DateTime.Now;
            errorList.Add(this);
        }

        public Error(string errorMessage, ErrorType errorType, Exception ex = null)
        {
            Message = errorMessage;
            Type = errorType;
            Exception = ex;
            Time = DateTime.Now;

            errorList.Add(this);
        }

        public static List<Error> errorList = new List<Error>();

        public static Error UnknownScanWebDomain(string url, Exception ex, bool showPopUp=false)
        {
            Error error = new Error();
            error.Message = $"{url} | Unknown web domain, impossible to download scan from here";
            error.Type = ErrorType.UnknownScanWebDomain;
            error.Exception = ex;

            string mBoxMessage = $"Unknown web domain: {url}, the possibility to download scan from this website has not been implemented yet.";
            string mBoxCaption = "Error";
            Debug.WriteLine($"{mBoxMessage}\n");
            if (showPopUp) MsgWindow.ShowOkWindow(mBoxCaption, mBoxMessage, false, MsgWindow.ImageType.Information);

            return error;
        }

        public static Error NoOutputDirectory()
        {
            Error error = new Error();
            error.Message = "The output directory doesn't exist";
            error.Type = ErrorType.NoOutputDirectory;

            Debug.WriteLine("\nError, impossible to find the expected download directory.\n");

            return error;
        }

        public static Error FailedHtmlDownload(Exception ex, string htmlUrl)
        {
            Error error = new Error();
            error.Message = $"{htmlUrl} | Failed to download html content";
            error.Type = ErrorType.FailedHtmlDownload;
            error.Exception = ex;

            string mBoxMessage = $"Error while loading {htmlUrl} content, this scan won't be downloaded : {ex}";
            string mBoxCaption = "Error";

            Debug.WriteLine($"\n{mBoxMessage}");
            Debug.WriteLine($"Verify you entered a correct scan url.\n");
            Debug.WriteLine($"Exception: {ex}\n");

            if (Settings.Instance.ErrorPauseApp)
            {
                MsgWindow.ShowOkWindow(mBoxCaption, mBoxMessage, false, MsgWindow.ImageType.Error);
            }

            return error;
        }

        public static Error FailedImageDownload(Exception ex, string imgUrl)
        {
            Error error = new Error();
            error.Message = $"{imgUrl} | Failed to download image";
            error.Type = ErrorType.FailedImageDownload;
            error.Exception = ex;

            Debug.WriteLine($"\nDownload failed for {imgUrl}!");
            Debug.WriteLine($"Verify the image URL is working in a web browser.\n");
            Debug.WriteLine($"Exception: {ex}\n");

            return error;
        }

        public static Error FailedCbzCreation(Exception ex, ScanData scanData, bool deleteImagesAfterCbzCreation)
        {
            Error error = new Error();
            error.Message = $"{scanData.BookName}-{scanData.ChapterId} | Failed to create cbz archive";
            error.Type = ErrorType.FailedCbzCreation;
            error.Exception = ex;

            Debug.WriteLine($"An error occured while creating the CBZ archive for {scanData.BookName}-{scanData.ChapterId}: {ex}");
            Debug.WriteLine($"Exception: {ex}\n");

            return error;
        }

        public static Error FailedToReplaceEmptyCbz(Exception ex, ScanData scanData, bool deleteImagesAfterCbzCreation)
        {
            Error error = new Error();
            error.Message = $"{scanData.BookName}-{scanData.ChapterId} | Failed to replace empty cbz archive";
            error.Type = ErrorType.FailedToReplaceEmptyCbz;
            error.Exception = ex;

            Debug.WriteLine($"An error occured while replacing an empty CBZ archive for {scanData.BookName}-{scanData.ChapterId}!");
            Debug.WriteLine($"=> Exception: {ex}\n");

            return error;
        }

        public static Error ChapterDoesntExist(ScanData scanData, string chapterUrl)
        {
            Error error = new Error();
            error.Message = $"{scanData.BookName}-{scanData.ChapterId} | {chapterUrl} doesn't exist on the website";
            error.Type = ErrorType.ChapterDoesntExist;

            Debug.WriteLine($"\n{scanData.BookName} chapter {scanData.ChapterId} doesn't exist on the website ({chapterUrl}). Make sure this chapter really exist.\n");

            return error;
        }

        public static Error MissingSettingsJson(string jsonPath)
        {
            Error error = new Error();
            error.Message = $"The Settings.json file ({jsonPath}) is missing";
            error.Type = ErrorType.MissingSettingsJson;
            error.Exception = Exceptions.MissingSettingsJson(jsonPath);

            string mBoxMessage = $"The Settings.json file ({jsonPath}) is missing, a new json file will be created with the default settings.";
            string mBoxCaption = "Error - Missing settings json file";
            Debug.WriteLine($"\n{mBoxMessage}\n");

            MsgWindow.ShowOkWindow(true, mBoxCaption, mBoxMessage, false, MsgWindow.ImageType.Warning);

            return error;
        }

        public static Error MissingScanDataJson(string jsonPath)
        {
            Error error = new Error();
            error.Message = $"The ScansLocalData.json file ({jsonPath}) is missing";
            error.Type = ErrorType.MissingScanDataJson;
            error.Exception = Exceptions.MissingScanDataJson(jsonPath);

            string mBoxMessage = $"The ScansLocalData.json file ({jsonPath}) is missing, a new json file without your previous scan data will be created.";
            string mBoxCaption = "Error - Missing scan data json file";
            Debug.WriteLine($"\n{mBoxMessage}\n");

            MsgWindow.ShowOkWindow(true, mBoxCaption, mBoxMessage, false, MsgWindow.ImageType.Warning);

            return error;
        }

        public static Error FailedToLoadSettingsJson(string jsonPath, Exception ex)
        {
            Error error = new Error();
            error.Message = $"Failed to load the settings json ({jsonPath}), an error happened during json deserialization";
            error.Type = ErrorType.FailedToLoadSettingsJson;
            error.Exception = ex;

            Debug.WriteLine($"\nImpossible to load the settings from the json ({jsonPath}), an error happened during json deserialization\n");
            Debug.WriteLine($"Exception: {ex}\n");

            return error;
        }

        public static Error FailedToSaveSettingsJson(string jsonPath, Exception ex)
        {
            Error error = new Error();
            error.Message = $"Failed to save the settings in the json ({jsonPath}), an error happened.";
            error.Type = ErrorType.FailedToSaveSettingsJson;
            error.Exception = ex;

            string mBoxMessage = $"Impossible to save the settings in the json ({jsonPath}), the changes you made will be reverted after an app restart.";
            string mBoxCaption = "Error";

            Debug.WriteLine($"\n{mBoxMessage}\n");
            Debug.WriteLine($"Exception: {ex}\n");

            MsgWindow.ShowOkWindow(mBoxCaption, mBoxMessage, false, MsgWindow.ImageType.Error);

            return error;
        }

        public static Error FailedToLoadScansLocalData(string jsonPath, Exception ex)
        {
            Error error = new Error();
            error.Message = $"Failed to load the scans data json ({jsonPath}), an error happened during json deserialization";
            error.Type = ErrorType.FailedToLoadScansLocalData;
            error.Exception = ex;

            Debug.WriteLine($"\nImpossible to load the scans data from the json ({jsonPath}), an error happened during json deserialization\n");
            Debug.WriteLine($"Exception: {ex}\n");

            return error;
        }

        public static Error FailedToSaveScansLocalData(string jsonPath, Exception ex)
        {
            Error error = new Error();
            error.Message = $"Failed to save the scans data in the json ({jsonPath}), an error happened.";
            error.Type = ErrorType.FailedToSaveScansLocalData;
            error.Exception = ex;

            string mBoxMessage = $"Impossible to save the scans data in the json ({jsonPath}), the changes you made will be reverted after an app restart.";
            string mBoxCaption = "Error";

            Debug.WriteLine($"\n{mBoxMessage}\n");
            Debug.WriteLine($"Exception: {ex}\n");

            MsgWindow.ShowOkWindow(mBoxCaption, mBoxMessage, false, MsgWindow.ImageType.Error);

            return error;
        }
    }
}
