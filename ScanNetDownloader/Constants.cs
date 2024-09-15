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
        public static readonly string[] URL_BLOCK_START_SEPARATOR = new string[] { "<div class=\"viewer-cnt\">" };
        public static readonly string[] URL_BLOCK_END_SEPARATOR = new string[] { "<div id=\"ppp\" style>" };
        public static readonly string[] CLEAN_BEFORE_IMG_TAG_SEPARATOR = new string[] { "<div id=\"all\" style=\" display: none; \">" };
        public static readonly string[] IMG_TAG_END_SEPARATOR = new string[] { "/>" };
        #endregion

        #region Char and String
        public static readonly char QUOTE_CHAR = '\"';
        public static readonly char SLASH_CHAR = '/';
        public static readonly char DASH_CHAR = '-';
        public static readonly char UNDERSCORE_CHAR = '_';
        public static readonly char POINT_CHAR = '.';

        public static readonly string HTTP_ADDRESS = "https://";
        public static readonly string SPACE = " ";
        #endregion

        #region Time values
        public static readonly int ONE_SECOND_IN_MILLISECONDS = 1000;
        public static readonly int HALF_SECOND_IN_MILLISECONDS = 500;
        #endregion


    }
}
