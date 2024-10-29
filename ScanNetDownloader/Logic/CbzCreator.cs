using System;
using System.Collections.Generic;
using System.IO.Compression;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScanNetDownloader.Logic
{
    /// <summary>
    /// Handle the creation of the CbzArchive
    /// </summary>
    class CbzCreator
    {
        public static void BuildCbzArchive(ScanData scanData, string downloadPath)
        {
            Downloader.WriteDlInfoLine($"=> Create .CBZ for {scanData.BookName}-{scanData.ChapterId}...");

            string bookName = scanData.BookName;
            string chapterNumber = scanData.ChapterId.ToString();

            string folderToArchive = downloadPath;
            string cbzFilePath = Path.Combine(Directory.GetParent(downloadPath).FullName, $"{bookName}{Constants.CBZ_CHAPTER_PREFIX}{chapterNumber}{Constants.CBZ_EXTENSION}");

            if (Directory.EnumerateFileSystemEntries(folderToArchive).Any() == false)
            {
                Downloader.WriteDlInfoLine($"=> No images downloaded for {bookName}-{chapterNumber}, CBZ creation will be skipped!\n");
                return;
            }

            if (File.Exists(cbzFilePath) == false)
            {
                try
                {
                    ZipFile.CreateFromDirectory(folderToArchive, cbzFilePath);
                    Downloader.WriteDlInfoLine($"=> {bookName}-{chapterNumber} .CBZ successfully created!\n");
                }
                catch (IOException ex)
                {
                    Error.FailedCbzCreation(ex, scanData, Settings.Instance.DeleteImagesAfterCbzCreation);
                    return;
                }
            }
            else // a cbz file already exist
            {
                if (File.ReadAllBytes(cbzFilePath).Length > 0)
                {
                    Downloader.WriteDlInfoLine($"=> .CBZ already created!\n");
                }
                else // Replace empty Cbz
                {
                    try
                    {
                        File.Delete(cbzFilePath);
                        ZipFile.CreateFromDirectory(folderToArchive, cbzFilePath);
                        Downloader.WriteDlInfoLine($"=> {bookName}-{chapterNumber} .CBZ successfully created!\n");
                    }
                    catch (IOException ex)
                    {
                        Error.FailedToReplaceEmptyCbz(ex, scanData, Settings.Instance.DeleteImagesAfterCbzCreation);
                        return;
                    }
                }
            }

            if (Settings.Instance.DeleteImagesAfterCbzCreation)
            {
                if (Directory.Exists(downloadPath)) Directory.Delete(downloadPath, true);
            }
        }
    }
}
