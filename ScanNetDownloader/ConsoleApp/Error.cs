using System.Diagnostics;
using System.Windows;

namespace ScanNetDownloader.ConsoleApp
{
    // TODO : Build WPF error system
    public class Error
    {
        public enum ErrorType
        {
            None = 0,
            NoScansUrl = 1,
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

        public static void NoScansUrl(string nameOfEmptyList)
        {
            errorList.Add(new Error("No scans URL have been provided by the user", ErrorType.NoScansUrl));

            Debug.WriteLine($"No scans URL have been provided.");
            Debug.WriteLine($"Open Settings.json (located next to the .exe) and add the URL of the scans you want to download in the Dictionnary \"{nameOfEmptyList}\".");
            Debug.WriteLine($"Check the README file for more info on how to add url and select chapters.\n");

            if (Settings.Instance.AutoOpenJsonWhenNecessary)
            {
                Debug.WriteLine($"No scans URL have been provided. Press any key to close the app and open json settings...");
                MessageBox.Show("Press ok to open Settings.json and close the app...", "Quit app", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            else
            {
                Debug.WriteLine($"No scans URL have been provided. Press any key to close the app...");
                MessageBox.Show("The app will be closed...", "Quit app", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        public static void UnknownScanWebDomain(string url)
        {
            errorList.Add(new Error($"{url} | Unknown web domain, impossible to download scan from here", ErrorType.UnknownScanWebDomain));

            Debug.WriteLine($"Unknown web domain: {url}, the possibility to download scan from this website has not been implemented yet.\n");
            MessageBox.Show($"Unknown web domain: {url}, the possibility to download scan from this website has not been implemented yet.\n", "Error", MessageBoxButton.OK, MessageBoxImage.Information);

        }

        public static void NoOutputDirectory()
        {
            errorList.Add(new Error("The output directory doesn't exist", ErrorType.NoOutputDirectory));

            Debug.WriteLine("\nError, impossible to find the expected download directory.\n");
        }

        public static void FailedHtmlDownload(Exception ex, string htmlUrl)
        {
            errorList.Add(new Error($"{htmlUrl} | Failed to download html content", ErrorType.FailedHtmlDownload, ex));

            Debug.WriteLine($"\nError while loading {htmlUrl} content, this scan won't be downloaded.");
            Debug.WriteLine($"Verify you entered a correct scan url.\n");

            Debug.WriteLine($"Exception: {ex}\n");

            if (Settings.Instance.ErrorsPauseApp)
            {
                Debug.WriteLine($"Press any key to continue...\n");
                MessageBox.Show($"\nError while loading {htmlUrl} content, this scan won't be downloaded.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public static void FailedImageDownload(Exception ex, string imgUrl)
        {
            errorList.Add(new Error($"{imgUrl} | Failed to download image", ErrorType.FailedImageDownload, ex));

            Debug.WriteLine($"\nDownload failed for {imgUrl}!");
            Debug.WriteLine($"Verify the image URL is working in a web browser.\n");

            Debug.WriteLine($"Exception: {ex}\n");

            if (Settings.Instance.ErrorsPauseApp)
            {  
                Debug.WriteLine($"Press any key to continue...\n");
                MessageBox.Show($"\nDownload failed for {imgUrl}!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public static void FailedCbzCreation(Exception ex, ScanWebsiteUrl scanUrl, bool deleteImagesAfterCbzCreation)
        {
            errorList.Add(new Error($"{scanUrl.BookName}-{scanUrl.ChapterId} | Failed to create cbz archive", ErrorType.FailedCbzCreation, ex));

            Debug.WriteLine($"=> An error occured while creating the CBZ archive for {scanUrl.BookName}-{scanUrl.ChapterId}!");
            if (deleteImagesAfterCbzCreation)
            {
                Debug.WriteLine($"=> The scan images won't be deleted so you can create the CBZ manually.\n");
            }
            else
            {
                Debug.Write('\n');
            }

            Debug.WriteLine($"=> Exception: {ex}\n");

            if (Settings.Instance.ErrorsPauseApp)
            {
                Debug.WriteLine($"=> Press any key to continue...\n");
                MessageBox.Show($"=> An error occured while creating the CBZ archive for {scanUrl.BookName}-{scanUrl.ChapterId}!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public static void FailedToReplaceEmptyCbz(Exception ex, ScanWebsiteUrl scanUrl, bool deleteImagesAfterCbzCreation)
        {
            errorList.Add(new Error($"{scanUrl.BookName}-{scanUrl.ChapterId} | Failed to replace empty cbz archive", ErrorType.FailedToReplaceEmptyCbz, ex));

            Debug.WriteLine($"=> An error occured while replacing an empty CBZ archive for {scanUrl.BookName}-{scanUrl.ChapterId}!");
            if (deleteImagesAfterCbzCreation)
            {
                Debug.WriteLine($"=> The scan images won't be deleted so you can create the CBZ manually.\n");
            }
            else
            {
                Debug.Write('\n');
            }

            Debug.WriteLine($"=> Exception: {ex}\n");

            if (Settings.Instance.ErrorsPauseApp)
            {
                Debug.WriteLine($"=> Press any key to continue...\n");
                MessageBox.Show($"=> An error occured while replacing an empty CBZ archive for {scanUrl.BookName}-{scanUrl.ChapterId}!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public static void FailedToParseChapterEnteredByUser(ScanWebsiteUrl scanUrl, string chapterEnteredByUser)
        {
            errorList.Add(new Error($"{scanUrl.BookName} | Error when parsing chapter {chapterEnteredByUser}", ErrorType.FailedToParseChapterEnteredByUser));

            Debug.WriteLine($"Cannot parse \"{chapterEnteredByUser}\" to int => invalid chapter number. Entry will not be added to the chapter list for {scanUrl.BookName} ({scanUrl.Url})");

            Debug.WriteLine($" -> Failed to parse \"{chapterEnteredByUser}\" to int, this is not a valid number. \"{chapterEnteredByUser}\" will not be added to the chapter list for {scanUrl.BookName}!");
            MessageBox.Show($"Cannot parse \"{chapterEnteredByUser}\" to int => invalid chapter number. Entry will not be added to the chapter list for {scanUrl.BookName} ({scanUrl.Url})", "Error", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        public static void ChapterDoesntExist(ScanWebsiteUrl scanUrl, string chapterUrl)
        {
            errorList.Add(new Error($"{scanUrl.BookName}-{scanUrl.ChapterId} | {chapterUrl} doesn't exist on the website", ErrorType.ChapterDoesntExist));

            Debug.WriteLine($"\n{scanUrl.BookName} chapter {scanUrl.ChapterId} doesn't exist on the website ({chapterUrl}). Make sure this chapter really exist.\n");

            if (Settings.Instance.ErrorsPauseApp)
            {
                Debug.WriteLine($"Press any key to continue...\n");
                MessageBox.Show($"\n{scanUrl.BookName} chapter {scanUrl.ChapterId} doesn't exist on the website ({chapterUrl}). Make sure this chapter really exist.\n", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public static void MissingSettingsJson(string jsonPath)
        {
            errorList.Add(new Error($"The settings.json file ({jsonPath}) is missing", ErrorType.MissingSettingsJson));

            Debug.WriteLine($"\nThe settings.json file ({jsonPath}) is missing, a new json file will be created with the default settings.\n");

            // TODO: Proper management of error pop-up
            string errorInfo = $"The settings.json file ({jsonPath}) is missing, a new json file will be created with the default settings.";

            MessageBox.Show(errorInfo, "Error - missing json file", MessageBoxButton.OK, MessageBoxImage.Warning);
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

            Debug.WriteLine($"\nImpossible to save the settings in the json ({jsonPath}), the changes you made will be reverted after an app restart.\n");
            Debug.WriteLine($"Exception: {ex}\n");

            Debug.WriteLine($"Press any key to continue...\n");
            MessageBox.Show($"\nImpossible to save the settings in the json ({jsonPath}), the changes you made will be reverted after an app restart.\n", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
}
