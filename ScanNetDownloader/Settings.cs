using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScanNetDownloader
{
    public class Settings
    {
        public static Settings instance = null;

        /// <summary>
        /// Dictionnary containing the scan url as a key and the chapter to download as value
        /// </summary>
        public Dictionary<string, string> ScansUrlAndCorrespondingChapters { get; set; }

        /// <summary>
        /// Set a custom output folder (if null or empty, we use the default user download folder) (Default=string.Empty)
        /// </summary>
        public string CustomFolderPath { get; set; } = "";

        /// <summary>
        /// Do you want to create a .cbz archive of every chapter downloaded (Default=True)
        /// </summary>
        public bool CreateCbzArchive { get; set; } = true;

        /// <summary>
        /// Do you want to keep the images downloaded once the cbz has been created (Default=False)
        /// </summary>
        public bool DeleteImagesAfterCbzCreation { get; set; } = false;

        /// <summary>
        /// Should the program automatically open the output directory when closing (Default=True)
        /// </summary>
        public bool OpenOutputDirectoryWhenClosing { get; set; } = true;

        public void Log()
        {
            Debug.WriteLine($"LOG SETTINGS");
            Debug.WriteLine(JsonConvert.SerializeObject(this, Formatting.Indented));
        }
    }
}
