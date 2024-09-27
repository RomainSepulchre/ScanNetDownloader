using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace ScanNetDownloader
{
    public class AnimeSamaFrUrl: ScanWebsiteUrl
    {
        public AnimeSamaFrUrl(string url) : base(url)
        {
            this.Url = url;
            WebsiteDomain = "https://anime-sama.fr/";
            ChapterId = -1; // Fake chapter to get book info
            BookName = GetBookNameFromUrl(url);
        }

        public AnimeSamaFrUrl(string url, int chapterId) : base(url)
        {
            this.Url = url;
            WebsiteDomain = "anime-sama.fr";
            ChapterId = chapterId;
            BookName = GetBookNameFromUrl(url);
        }

        public override string GetBookNameFromUrl(string url, bool removeSpace = false)
        {
            #region Chapter and img url examples
            // Example of chapter url
            //   0    1      2           3         4     5   6
            // https://anime-sama.fr/catalogue/berserk/scan/vf/
            // https://anime-sama.fr/catalogue/20th-century-boys/scan/vf/
            // https://anime-sama.fr/catalogue/alice-in-borderland/scan/vf/
            // https://anime-sama.fr/catalogue/fairy-tail/scan/vf/
            // https://anime-sama.fr/catalogue/the-terminally-ill-young-master-of-the-baek-clan/scan/vf/

            // Example of img url
            //   0    1      2        3    4     5    6   7
            // https://anime-sama.fr/s2/scans/Berserk/2/1.jpg
            // https://anime-sama.fr/s2/scans/20th%20Century%20boys/1/1.jpg
            // https://anime-sama.fr/s2/scans/Alice%20in%20Borderland/1/1.jpg
            // https://anime-sama.fr/s2/scans/Fairy%20Tail/1/1.jpg
            // https://anime-sama.fr/s2/scans/The%20Terminally%20Ill%20Young%20Master%20of%20the%20Baek%20Clan/1/1.jpg
            #endregion

            int splitIdForChapterUrl = 4;
            int splitIdForImgUrl = 5;
            bool isImgUrl = url.Contains(Constants.ANIMESAMA_IMG_URL_MARKER); // Check if we are using a link of a chapter or an image

            string bookName = url.Split(Constants.SLASH_CHAR)[isImgUrl ? splitIdForImgUrl : splitIdForChapterUrl];
            bookName = bookName.ToTitleCase();
            bookName = bookName.Replace(Constants.DASH_CHAR.ToString(), removeSpace ? string.Empty : Constants.SPACE);
            bookName = bookName.Replace(Constants.UNDERSCORE_CHAR.ToString(), removeSpace ? string.Empty : Constants.SPACE);

            return bookName;
        }

        public override string GetChapterNumberFromUrl(string url, bool keepNumberOnly = true)
        {
            #region Chapter and img url examples
            // Example of chapter url
            //   0    1      2           3         4     5   6
            // https://anime-sama.fr/catalogue/berserk/scan/vf/
            // https://anime-sama.fr/catalogue/20th-century-boys/scan/vf/
            // https://anime-sama.fr/catalogue/alice-in-borderland/scan/vf/
            // https://anime-sama.fr/catalogue/fairy-tail/scan/vf/
            // https://anime-sama.fr/catalogue/the-terminally-ill-young-master-of-the-baek-clan/scan/vf/

            // Example of img url
            //   0    1      2        3    4     5    6   7
            // https://anime-sama.fr/s2/scans/Berserk/2/1.jpg
            // https://anime-sama.fr/s2/scans/20th%20Century%20boys/1/1.jpg
            // https://anime-sama.fr/s2/scans/Alice%20in%20Borderland/1/1.jpg
            // https://anime-sama.fr/s2/scans/Fairy%20Tail/1/1.jpg
            // https://anime-sama.fr/s2/scans/The%20Terminally%20Ill%20Young%20Master%20of%20the%20Baek%20Clan/1/1.jpg
            #endregion

            int splitIdForImgUrl = 6;
            bool isImgUrl = url.Contains(Constants.ANIMESAMA_IMG_URL_MARKER); // Check if we are using a link of a chapter or an image

            if (isImgUrl)
            {
                string chapterNumber = url.Split(Constants.SLASH_CHAR)[splitIdForImgUrl];
                if (keepNumberOnly) chapterNumber = chapterNumber.Split(Constants.DASH_CHAR)[1];

                return chapterNumber;
            }
            else
            {
                // Impossible to get chapter number from anime Sama chapter url
                Debug.WriteLine($"\nIt's impossible to get the chapter number from the anime-sama chapter url");
                return "-1";
            }
        }

        public override string GetFileExtensionFromUrl(string url)
        {
            #region Img url examples
            // Example of img url
            //   0    1      2        3    4     5    6   7
            // https://anime-sama.fr/s2/scans/Berserk/2/1.jpg
            // https://anime-sama.fr/s2/scans/20th%20Century%20boys/1/1.jpg
            // https://anime-sama.fr/s2/scans/Alice%20in%20Borderland/1/1.jpg
            // https://anime-sama.fr/s2/scans/Fairy%20Tail/1/1.jpg
            // https://anime-sama.fr/s2/scans/The%20Terminally%20Ill%20Young%20Master%20of%20the%20Baek%20Clan/1/1.jpg
            #endregion

            string fileExtension = url.Split(Constants.SLASH_CHAR)[7];
            fileExtension = "." + fileExtension.Split(Constants.POINT_CHAR)[1];

            return fileExtension;

            // Should I always return .jpg directly since I build the link myself ?
            //return Constants.JPG_EXTENSION;
        }

        protected override List<string> ParseHtmlToGetImgLinks(string htmlContent)
        {
            Debug.WriteLine($"\nPARSE HTML - {BookName}_{ChapterId} ({Url}), imgs found:");
            // V1 Recreate url since we can't get the page after js loading

            // https://anime-sama.fr/s2/scans/Berserk/2/1.jpg
            // https://anime-sama.fr/s2/scans/20th%20Century%20boys/1/1.jpg
            // https://anime-sama.fr/s2/scans/Alice%20in%20Borderland/1/1.jpg
            // https://anime-sama.fr/s2/scans/Fairy%20Tail/1/1.jpg
            // https://anime-sama.fr/s2/scans/The%20Terminally%20Ill%20Young%20Master%20of%20the%20Baek%20Clan/1/1.jpg

            // Get Book Name for download URL
            string[] splitContent = htmlContent.Split(Constants.ANIMESAMA_BOOK_NAME_START_SEPARATOR, StringSplitOptions.RemoveEmptyEntries); // Split before the meta tag with the book name
            string bookNameForUrl = splitContent[1]; // Keep the split after our separator (trim the beginning)

            splitContent = bookNameForUrl.Split(Constants.ANIMESAMA_BOOK_NAME_END_SEPARATOR, StringSplitOptions.RemoveEmptyEntries); // Split after the book name
            bookNameForUrl = splitContent[0]; // Keep the split before our separator (trim the end)

            // Test if chapter is valid by looking for first image
            int firstImgId = 1;
            string chapterUrl = $"{Constants.ANIMESAMA_IMG_URL_START}{bookNameForUrl}/{ChapterId}/";
            string firstImgUrl = $"{chapterUrl}{firstImgId}{Constants.JPG_EXTENSION}";

            if (IsUrlValid(firstImgUrl) == false)
            {
                // TODO: Add error management
                Debug.WriteLine($"Invalid chapter url ({firstImgUrl}) for chapter {ChapterId}. Check if chapter really exist, entry will not be added to the chapter list ({Url})");
                return new List<string>();
            }

            List<string> imgUrls = new List<string>();

            int pageId = 1;
            string imgUrl = $"{chapterUrl}{pageId}{Constants.JPG_EXTENSION}";
            while (IsUrlValid(imgUrl))
            {
                imgUrls.Add(imgUrl);
                pageId++;
                imgUrl = $"{chapterUrl}{pageId}{Constants.JPG_EXTENSION}";
            }

            imgUrls.Log(); // Debug log of list items
            Debug.WriteLine("\n");

            return imgUrls;
        }
    }
}
