using System.Diagnostics;
using System.Windows;

namespace ScanNetDownloader.Logic
{
    // TODO : Build WPF error system
    public class Error
    {
        public enum ErrorType
        {
            None = 0,
            NoScansUrl = 1, // TODO: Is it still usefull ?
            UnknownScanWebDomain = 2,
            NoOutputDirectory = 3,
            FailedHtmlDownload = 4,
            FailedImageDownload = 5,
            FailedCbzCreation = 6,
            FailedToReplaceEmptyCbz = 7,
            FailedToParseChapterEnteredByUser = 8,
            ChapterDoesntExist = 9,
            MissingSettingsJson = 10,
            FailedToLoadSettingsJson = 11,
            FailedToSaveSettingsJson = 12
        }

        public string Message
        {
            get; private set;
        }

        public ErrorType Type
        {
            get; private set;
        }

        public Exception Exception
        {
            get; private set;
        }

        public Error(string errorMessage, ErrorType errorType, Exception ex = null)
        {
            Message = errorMessage;
            Type = errorType;
            Exception = ex;
        }

        public static List<Error> errorList = new List<Error>();

        public static void ShowDownloadErrors()
        {
            Debug.WriteLine("Finished, some error happened during downloading:\n");

            foreach (var error in errorList)
            {
                switch (error.Type)
                {
                    case ErrorType.None:
                    case ErrorType.NoScansUrl:
                    case ErrorType.UnknownScanWebDomain:
                    case ErrorType.NoOutputDirectory:
                    case ErrorType.FailedToParseChapterEnteredByUser:
                    case ErrorType.MissingSettingsJson:
                    case ErrorType.FailedToLoadSettingsJson:
                    case ErrorType.FailedToSaveSettingsJson:
                    default:
                        break;

                    case ErrorType.FailedHtmlDownload:
                    case ErrorType.FailedImageDownload:
                    case ErrorType.FailedCbzCreation:
                    case ErrorType.FailedToReplaceEmptyCbz:
                    case ErrorType.ChapterDoesntExist:
                        Debug.WriteLine($"-{error.Type} | {error.Message}");
                        break;
                }
            }

        }

        public static void UnknownScanWebDomain(string url)
        {
            errorList.Add(new Error($"{url} | Unknown web domain, impossible to download scan from here", ErrorType.UnknownScanWebDomain));

            string mBoxMessage = $"Unknown web domain: {url}, the possibility to download scan from this website has not been implemented yet.";
            string mBoxCaption = "Error";
            Debug.WriteLine($"{mBoxMessage}\n");
            MessageBox.Show(mBoxMessage, mBoxCaption, MessageBoxButton.OK, MessageBoxImage.Information);

        }

        public static void NoOutputDirectory()
        {
            errorList.Add(new Error("The output directory doesn't exist", ErrorType.NoOutputDirectory));

            Debug.WriteLine("\nError, impossible to find the expected download directory.\n");
        }

        public static void FailedHtmlDownload(Exception ex, string htmlUrl)
        {
            errorList.Add(new Error($"{htmlUrl} | Failed to download html content", ErrorType.FailedHtmlDownload, ex));

            string mBoxMessage = $"Error while loading {htmlUrl} content, this scan won't be downloaded.";
            string mBoxCaption = "Error";

            Debug.WriteLine($"\n{mBoxMessage}");
            Debug.WriteLine($"Verify you entered a correct scan url.\n");

            Debug.WriteLine($"Exception: {ex}\n");

            if (Settings.Instance.ErrorPauseApp)
            {               
                MessageBox.Show(mBoxMessage, mBoxCaption, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public static void FailedImageDownload(Exception ex, string imgUrl)
        {
            errorList.Add(new Error($"{imgUrl} | Failed to download image", ErrorType.FailedImageDownload, ex));

            string mBoxMessage = $"Download failed for {imgUrl}!";
            string mBoxCaption = "Error";

            Debug.WriteLine($"\n{mBoxMessage}");
            Debug.WriteLine($"Verify the image URL is working in a web browser.\n");

            Debug.WriteLine($"Exception: {ex}\n");

            if (Settings.Instance.ErrorPauseApp)
            {
                
                MessageBox.Show(mBoxMessage, mBoxCaption, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public static void FailedCbzCreation(Exception ex, ScanWebsiteUrl scanUrl, bool deleteImagesAfterCbzCreation)
        {
            errorList.Add(new Error($"{scanUrl.BookName}-{scanUrl.ChapterId} | Failed to create cbz archive", ErrorType.FailedCbzCreation, ex));

            string mBoxMessage = $"=> An error occured while creating the CBZ archive for {scanUrl.BookName}-{scanUrl.ChapterId}!";
            string mBoxCaption = "Error";

            Debug.WriteLine(mBoxMessage);
            if (deleteImagesAfterCbzCreation)
            {
                Debug.WriteLine($"=> The scan images won't be deleted so you can create the CBZ manually.\n");
            }
            else
            {
                Debug.Write('\n');
            }

            Debug.WriteLine($"=> Exception: {ex}\n");

            if (Settings.Instance.ErrorPauseApp)
            { 
                MessageBox.Show(mBoxMessage, mBoxCaption, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public static void FailedToReplaceEmptyCbz(Exception ex, ScanWebsiteUrl scanUrl, bool deleteImagesAfterCbzCreation)
        {
            errorList.Add(new Error($"{scanUrl.BookName}-{scanUrl.ChapterId} | Failed to replace empty cbz archive", ErrorType.FailedToReplaceEmptyCbz, ex));

            string mBoxMessage = $"=> An error occured while replacing an empty CBZ archive for {scanUrl.BookName}-{scanUrl.ChapterId}!";
            string mBoxCaption = "Error";

            Debug.WriteLine(mBoxMessage);
            if (deleteImagesAfterCbzCreation)
            {
                Debug.WriteLine($"=> The scan images won't be deleted so you can create the CBZ manually.\n");
            }
            else
            {
                Debug.Write('\n');
            }

            Debug.WriteLine($"=> Exception: {ex}\n");

            if (Settings.Instance.ErrorPauseApp)
            {               
                MessageBox.Show(mBoxMessage, mBoxCaption, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public static void FailedToParseChapterEnteredByUser(ScanWebsiteUrl scanUrl, string chapterEnteredByUser)
        {
            errorList.Add(new Error($"{scanUrl.BookName} | Error when parsing chapter {chapterEnteredByUser}", ErrorType.FailedToParseChapterEnteredByUser));

            Debug.WriteLine($" -> Failed to parse \"{chapterEnteredByUser}\" to int, this is not a valid number. \"{chapterEnteredByUser}\" will not be added to the chapter list for {scanUrl.BookName}!");

            string mBoxMessage = $"Cannot parse \"{chapterEnteredByUser}\" to int => invalid chapter number. Entry will not be added to the chapter list for {scanUrl.BookName} ({scanUrl.Url})";
            string mBoxCaption = "Error";
            MessageBox.Show(mBoxMessage, mBoxCaption, MessageBoxButton.OK, MessageBoxImage.Information);
        }

        public static void ChapterDoesntExist(ScanWebsiteUrl scanUrl, string chapterUrl)
        {
            errorList.Add(new Error($"{scanUrl.BookName}-{scanUrl.ChapterId} | {chapterUrl} doesn't exist on the website", ErrorType.ChapterDoesntExist));

            string mBoxMessage = $"{scanUrl.BookName} chapter {scanUrl.ChapterId} doesn't exist on the website ({chapterUrl}). Make sure this chapter really exist.";
            string mBoxCaption = "Error";
            Debug.WriteLine($"\n{mBoxMessage}\n");

            if (Settings.Instance.ErrorPauseApp)
            {
                Debug.WriteLine($"Press any key to continue...\n");
                MessageBox.Show(mBoxMessage, mBoxCaption, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public static void MissingSettingsJson(string jsonPath)
        {
            errorList.Add(new Error($"The settings.json file ({jsonPath}) is missing", ErrorType.MissingSettingsJson));

            string mBoxMessage = $"The settings.json file ({jsonPath}) is missing, a new json file will be created with the default settings.";
            string mBoxCaption = "Error - missing json file";
            Debug.WriteLine($"\n{mBoxMessage}\n");

            // TODO: Proper management of error pop-up
            
            MessageBox.Show(mBoxMessage, mBoxCaption, MessageBoxButton.OK, MessageBoxImage.Warning);
        }

        public static void FailedToLoadSettingsJson(string jsonPath, Exception ex)
        {
            errorList.Add(new Error($"Failed to load the settings json ({jsonPath}), an error happened during json deserialization", ErrorType.FailedToLoadSettingsJson, ex));

            Debug.WriteLine($"\nImpossible to load the settings from the json ({jsonPath}), an error happened during json deserialization\n");
            Debug.WriteLine($"Exception: {ex}\n");

        }

        public static void FailedToSaveSettingsJson(string jsonPath, Exception ex)
        {
            errorList.Add(new Error($"Failed to save the settings in the json ({jsonPath}), an error happened.", ErrorType.FailedToSaveSettingsJson, ex));

            string mBoxMessage = $"Impossible to save the settings in the json ({jsonPath}), the changes you made will be reverted after an app restart.";
            string mBoxCaption = "Error";

            Debug.WriteLine($"\n{mBoxMessage}\n");
            Debug.WriteLine($"Exception: {ex}\n");

            MessageBox.Show(mBoxMessage, mBoxCaption, MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
}
