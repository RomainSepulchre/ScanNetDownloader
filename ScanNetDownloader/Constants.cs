using ScanDownloader;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScanNetDownloader
{
    public static class Constants
    {
        #region Paths
        public static readonly string USER_FOLDER_PATH = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile); // User directory path
        public static readonly string USER_DOWNLOAD_FOLDER_PATH = Path.Combine(USER_FOLDER_PATH, "Downloads"); // Default download directory path
        #endregion


        #region Split Separator Strings

        // Scan VF
        public static readonly string[] SCANVF_URL_BLOCK_START_SEPARATOR = new string[] { "<div class=\"viewer-cnt\">" };
        public static readonly string[] SCANVF_URL_BLOCK_END_SEPARATOR = new string[] { "<div id=\"ppp\" style>" };
        public static readonly string[] SCANVF_CLEAN_BEFORE_IMG_TAG_SEPARATOR = new string[] { "<div id=\"all\" style=\" display: none; \">" };
        public static readonly string[] SCANVF_IMG_TAG_END_SEPARATOR = new string[] { "/>" };

        // Anime Sama
        public static readonly string[] ANIMESAMA_BOOK_NAME_START_SEPARATOR = new string[] { "<meta name=\"description\" content=\"" };
        public static readonly string[] ANIMESAMA_BOOK_NAME_END_SEPARATOR = new string[] { " - Scans" };
        #endregion

        #region Char and String
        public static readonly char QUOTE_CHAR = '\"';
        public static readonly char SLASH_CHAR = '/';
        public static readonly char BACKSLASH_CHAR = '\\';
        public static readonly char DASH_CHAR = '-';
        public static readonly char UNDERSCORE_CHAR = '_';
        public static readonly char POINT_CHAR = '.';
        public static readonly char SEMICOLON_CHAR = ';';

        public static readonly string HTTP_ADDRESS = "https://";
        public static readonly string SPACE = " ";
        public static readonly string SCAN_SUFFIX = " Scan";
        public static readonly string CHAPTER_PREFIX = "Chapter ";
        public static readonly string SCAN_CHAPTER_PATH = $"{SCAN_SUFFIX}{BACKSLASH_CHAR}{CHAPTER_PREFIX}";

        public static readonly string SCANVF_DOMAIN_NAME = "scan-vf.net";
        public static readonly string SCANVF_IMG_URL_SPECIFICITY = "uploads";
        public static readonly string ANIMESAMA_DOMAIN_NAME = "anime-sama.fr";
        public static readonly string ANIMESAMA_IMG_URL_START = "https://anime-sama.fr/s2/scans/";
        public static readonly string ANIMESAMA_IMG_URL_SPECIFICITY = "scans";

        public static readonly string WEBP_EXTENSION = ".webp";
        public static readonly string JPG_EXTENSION = ".jpg";
        #endregion

        #region Time values
        public static readonly int ONE_SECOND_IN_MILLISECONDS = 1000;
        public static readonly int HALF_SECOND_IN_MILLISECONDS = 500;
        #endregion


    }
}
