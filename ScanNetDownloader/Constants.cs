using ScanDownloader;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScanNetDownloader
{
    public class Constants
    {
        #region Paths
        public static readonly string BASE_DIRECTORY_PATH = AppDomain.CurrentDomain.BaseDirectory;
        public const string SETTINGS_JSON_FILENAME = "Settings.json";
        public static readonly string SETTINGS_JSON_PATH = BASE_DIRECTORY_PATH + SETTINGS_JSON_FILENAME;
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
        public const char QUOTE_CHAR = '\"';
        public const char SLASH_CHAR = '/';
        public const char BACKSLASH_CHAR = '\\';
        public const char DASH_CHAR = '-';
        public const char UNDERSCORE_CHAR = '_';
        public const char POINT_CHAR = '.';
        public const char SEMICOLON_CHAR = ';';

        public const string HTTP_ADDRESS = "https://";
        public const string SPACE = " ";
        public const string SCAN_SUFFIX = " Scan";
        public const string CHAPTER_PREFIX = "Chapter ";
        public static readonly string SCAN_CHAPTER_PATH = $"{SCAN_SUFFIX}{BACKSLASH_CHAR}{CHAPTER_PREFIX}";

        public const string SCANVF_DOMAIN_NAME = "scan-vf.net";
        public const string SCANVF_IMG_URL_MARKER = "uploads";
        public const string SCANVF_CHAPTER_IN_URL = "/chapitre-";
        public const string ANIMESAMA_DOMAIN_NAME = "anime-sama.fr";
        public const string ANIMESAMA_IMG_URL_START = "https://anime-sama.fr/s2/scans/";
        public const string ANIMESAMA_IMG_URL_MARKER = "scans";

        public const string WEBP_EXTENSION = ".webp";
        public const string JPG_EXTENSION = ".jpg";

        
        #endregion

        #region Time values
        public const int ONE_SECOND_IN_MILLISECONDS = 1000;
        public const int HALF_SECOND_IN_MILLISECONDS = 500;
        #endregion


    }
}
