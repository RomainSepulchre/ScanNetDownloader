using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;

namespace ScanNetDownloader
{
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
            ChapterDoesntExist = 9
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

        public Error(string errorMessage, ErrorType errorType, Exception ex=null)
        {
            Message = errorMessage;
            Type = errorType;
            Exception = ex;
        }        

        public static List<Error> errorList = new List<Error>();

        public static void NoScansUrl(string nameOfEmptyList)
        {
            errorList.Add(new Error("No scans URL have been provided by the user", ErrorType.NoScansUrl));

            Console.ForegroundColor = ConsoleColor.DarkRed;
            Console.WriteLine($"No scans URL have been provided.");
            Console.WriteLine($"Add the URL of the scans you want to download in the list {nameOfEmptyList} in Program.cs.\n"); // TODO: Change this when json settings are finished

            Console.ResetColor();
            Console.WriteLine($"Press any key to close the app...");
            Console.ReadKey();
        }

        public static void UnknownScanWebDomain(string url)
        {
            errorList.Add(new Error($"{url} | Unknown web domain, impossible to download scan from here", ErrorType.UnknownScanWebDomain));

            Debug.WriteLine($"\nError unknown website cannot download scan from this url: {url}");

            Console.ForegroundColor = ConsoleColor.DarkRed;
            Console.WriteLine($"Unknown web domain: {url}, the possibility to download scan from this website has not been implemented yet.\n");
            Console.ResetColor();
        }

        public static void NoOutputDirectory()
        {
            errorList.Add(new Error("The output directory doesn't exist", ErrorType.NoOutputDirectory));

            Console.ForegroundColor = ConsoleColor.DarkRed;
            Console.WriteLine("\nError, impossible to find the expected download directory.\n");
            Console.ResetColor();
        }

        public static void FailedHtmlDownload(Exception ex, string htmlUrl)
        {
            errorList.Add(new Error($"{htmlUrl} | Failed to download html content", ErrorType.FailedHtmlDownload, ex));

            Debug.WriteLine($"Error while loading {htmlUrl} content: {ex}");

            Console.ForegroundColor = ConsoleColor.DarkRed;
            Console.WriteLine($"\nError while loading {htmlUrl} content, this scan won't be downloaded.");
            Console.WriteLine($"Verify you entered a correct scan url.\n");

            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"Exception: {ex}\n");

            Console.ResetColor();
            Console.WriteLine($"Press any key to continue...\n");
            Console.ReadKey(true);
        }

        public static void FailedImageDownload(Exception ex, string imgUrl)
        {
            errorList.Add(new Error($"{imgUrl} | Failed to download image", ErrorType.FailedImageDownload, ex));

            Debug.WriteLine($"Error while downloading: {ex}");

            Console.ForegroundColor = ConsoleColor.DarkRed;
            Console.WriteLine($"\nDownload failed for {imgUrl}!");
            Console.WriteLine($"Verify the image URL is working in a web browser.\n");

            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"Exception: {ex}\n");

            Console.ResetColor();
            Console.WriteLine($"Press any key to continue...\n");
            Console.ReadKey();
        }

        public static void FailedCbzCreation(Exception ex, ScanWebsiteUrl scanUrl, bool deleteImagesAfterCbzCreation)
        {
            errorList.Add(new Error($"{scanUrl.BookName}-{scanUrl.ChapterId} | Failed to create cbz archive", ErrorType.FailedCbzCreation, ex));

            Debug.WriteLine($"=> Error while creating CBZ archive for {scanUrl.BookName}-{scanUrl.ChapterId}: {ex}");

            Console.ForegroundColor = ConsoleColor.DarkRed;

            Console.WriteLine($"=> An error occured while creating the CBZ archive for {scanUrl.BookName}-{scanUrl.ChapterId}!");
            if (deleteImagesAfterCbzCreation)
            {
                Console.WriteLine($"=> The scan images won't be deleted so you can create the CBZ manually.\n");
            }
            else
            {
                Console.Write('\n');
            }

            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"=> Exception: {ex}\n");

            Console.ResetColor();
            Console.WriteLine($"=> Press any key to continue...\n");
            Console.ReadKey();
        }

        public static void FailedToReplaceEmptyCbz(Exception ex, ScanWebsiteUrl scanUrl, bool deleteImagesAfterCbzCreation)
        {
            errorList.Add(new Error($"{scanUrl.BookName}-{scanUrl.ChapterId} | Failed to replace empty cbz archive", ErrorType.FailedToReplaceEmptyCbz, ex));

            Debug.WriteLine($"=> Error while replacing an empty CBZ archive for {scanUrl.BookName}-{scanUrl.ChapterId}: {ex}");

            Console.ForegroundColor = ConsoleColor.DarkRed;

            Console.WriteLine($"=> An error occured while replacing an empty CBZ archive for {scanUrl.BookName}-{scanUrl.ChapterId}!");
            if (deleteImagesAfterCbzCreation)
            {
                Console.WriteLine($"=> The scan images won't be deleted so you can create the CBZ manually.\n");
            }
            else
            {
                Console.Write('\n');
            }

            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"=> Exception: {ex}\n");

            Console.ResetColor();
            Console.WriteLine($"=> Press any key to continue...\n");
            Console.ReadKey();
        }

        public static void FailedToParseChapterEnteredByUser(ScanWebsiteUrl scanUrl, string chapterEnteredByUser)
        {
            errorList.Add(new Error($"{scanUrl.BookName} | Error when parsing chapter {chapterEnteredByUser}", ErrorType.FailedToParseChapterEnteredByUser));

            Debug.WriteLine($"Cannot parse \"{chapterEnteredByUser}\" to int => invalid chapter number. Entry will not be added to the chapter list for {scanUrl.BookName} ({scanUrl.Url})");

            Console.ForegroundColor = ConsoleColor.DarkRed;
            Console.WriteLine($" -> Failed to parse \"{chapterEnteredByUser}\" to int, this is not a valid number. \"{chapterEnteredByUser}\" will not be added to the chapter list for {scanUrl.BookName}!");
            Console.ResetColor();
        }

        public static void ChapterDoesntExist(ScanWebsiteUrl scanUrl, string chapterUrl)
        {
            errorList.Add(new Error($"{scanUrl.BookName}-{scanUrl.ChapterId} | {chapterUrl} doesn't exist on the website", ErrorType.ChapterDoesntExist));

            Debug.WriteLine($"Invalid chapter url ({chapterUrl}) for {scanUrl.BookName}-{scanUrl.ChapterId}. Check if chapter really exist, entry will not be added to the chapter list ({scanUrl.Url})");

            Console.ForegroundColor = ConsoleColor.DarkRed;
            Console.WriteLine($"\n{scanUrl.BookName} chapter {scanUrl.ChapterId} doesn't exist on the website ({chapterUrl}). Make sure this chapter really exist.\n");

            Console.ResetColor();
            Console.WriteLine($"Press any key to continue...\n");
            Console.ReadKey();
        }
    }
}
