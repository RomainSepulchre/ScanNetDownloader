using Newtonsoft.Json;
using ScanNetDownloader.Logic.Helpers;
using ScanNetDownloader.View;
using ScanNetDownloader.View.CustomControls;
using System.Diagnostics;
using System.IO;

namespace ScanNetDownloader.Logic
{
    public class Settings
    {

        private static Settings _instance;

        public static Settings Instance
        {
            get { return _instance; }
            set { _instance = value; }
        }

        /// <summary>
        /// Store a custom output folder (if null or empty, we use the default user download folder)
        /// </summary>
        private string customOutputDir { get; set; } = "";

        /// <summary>
        /// Directory where the file should be downloaded.
        /// </summary>
        public string OutputDirectory
        {
            get
            {
                if (string.IsNullOrEmpty(customOutputDir)) return Constants.USER_DOWNLOAD_FOLDER_PATH;
                else return customOutputDir;
            }
            set
            {
                customOutputDir = value;  
            }
        }

        /// <summary>
        /// Should the program automatically open the output directory when closing (Default=True)
        /// </summary>
        public bool OpenOutputDirectoryAfterDownload { get; set; } = true;

        /// <summary>
        /// Should the program pause the app and wait for an user input when an error is triggered (Default=True)
        /// A major error requiring user input bypass this and pause the app anyway
        /// </summary>
        public bool ErrorPauseApp { get; set; } = false;

        /// <summary>
        /// Do you want to create a .cbz archive of every chapter downloaded (Default=True)
        /// </summary>
        public bool CreateCbzArchive { get; set; } = true;

        /// <summary>
        /// Do you want to keep the images downloaded once the cbz has been created (Default=False)
        /// </summary>
        public bool DeleteImagesAfterCbzCreation { get; set; } = false;       

        /// <summary>
        /// Should the program automatically open Settings.json when you need to check something in it (Default=True)
        /// </summary>
        public bool AutoOpenJsonWhenNecessary { get; set; } = true; // TODO: Do we still really need this, if yes add to optionsView

        /// <summary>
        /// Last property used to sort the ScanManager ListView, to use the same property next time we open the app
        /// </summary>
        public string ScanManagerListViewSortProperty { get; set; } = nameof(ScanItem.BookName);

        private static string settingsDataPath;

        public static void InitializeAppSettings(bool firstLaunch)
        {
#if DEBUG
            settingsDataPath = Constants.DEBUG_SETTINGS_JSON_PATH;
#else
            settingsDataPath = Constants.SETTINGS_JSON_PATH;
#endif
            Instance = LoadSettings(firstLaunch);
            Instance.Log();
        }

        

        private static Settings LoadSettings(bool firstLaunch)
        {
            Settings loadedSettings;

            JsonSerializerSettings serializerSettings = new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.All
            };

            if (File.Exists(settingsDataPath))
            {
                try
                {
                    loadedSettings = JsonConvert.DeserializeObject<Settings>(File.ReadAllText(settingsDataPath), serializerSettings);
                    if (loadedSettings == null) throw new Exception($"Loaded settings should never be null, something wrong happened during json deserialization");
                    return loadedSettings;
                }
                catch (Exception ex)
                {
                    Error.FailedToLoadSettingsJson(settingsDataPath, ex);

                    // TODO: Redo error management to fit with WPF version
                    string mBoxMessage = $"Impossible to load settings. Do you want to reset settings.json to it's default values ?";
                    string mBoxCaption = "Unable to load settings";
                    Debug.WriteLine(mBoxMessage);

                    YesNoWindow yesNoWindow = MsgWindow.ShowYesNoWindow(true, mBoxCaption, mBoxMessage, false, MsgWindow.ImageType.Warning);

                    if (yesNoWindow.Success)
                    {
                        loadedSettings = ResetToDefault();
                        return loadedSettings;
                    }
                    else //TODO: What to do in this case with WPF app ? Is there a better solution ?
                    {
                        mBoxMessage = "Please make sure nothing is wrong with the data in Settings.json, if the problem persist backup your settings and reset the json to it's default values.";
                        mBoxCaption = "Settings loading error";
                        Debug.WriteLine(mBoxMessage);
                        MsgWindow.ShowOkWindow(true, mBoxCaption, mBoxMessage, false, MsgWindow.ImageType.Error);

                        OpenJsonFile();
                        App.Quit();
                        return null;
                    }
                }
            }
            else // Missing Settings.json
            {
                if (!firstLaunch)
                {
                    Error.MissingSettingsJson(settingsDataPath);
                }

                loadedSettings = ResetToDefault();
                return loadedSettings;
            }
        }

        public static SettingsImportResult ImportSettings(string importPath)
        {
            JsonSerializerSettings serializerSettings = new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.All
            };

            try
            {
                Settings settingsToImport = JsonConvert.DeserializeObject<Settings>(File.ReadAllText(importPath), serializerSettings);
                if(settingsToImport == null) throw new Exception($"Failed to get data from provided settings json, the data is null. Json is an empty file or something went wrong during json deserialization.");

                // Replace settings
                Save(settingsToImport);
                return new SettingsImportResult(true);
            }
            catch (Exception ex)
            {
                return new SettingsImportResult(false, ex);
            }
        }

        public static void Save(Settings newSettings=null)
        {
            // If we specify newSettings, they replace the instance otherwise we save our Settings instance
            if (newSettings != null)
            {
                if (ReferenceEquals(Instance, newSettings) == false) // Make sure we didn't provide a reference of instance as argument
                {
                    Instance = newSettings;
                }
            }            

            JsonSerializerSettings serializerSettings = new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.All,
                Formatting = Formatting.Indented
            };

            try
            {
                File.WriteAllText(settingsDataPath, JsonConvert.SerializeObject(Instance, serializerSettings));
            }
            catch (Exception ex)
            {
                Error.FailedToSaveSettingsJson(settingsDataPath, ex);
            }
        }

        private static Settings ResetToDefault()
        {
            Settings defaultSettings = new Settings();
            Save(defaultSettings);
            return defaultSettings;
        }        

        public static void OpenJsonFile()
        {
            if (Instance != null)
            {
                if (Instance.AutoOpenJsonWhenNecessary)
                {
                    new Process { StartInfo = new ProcessStartInfo(settingsDataPath) { UseShellExecute = true } }.Start();
                }
            }
            else
            {
                new Process { StartInfo = new ProcessStartInfo(settingsDataPath) { UseShellExecute = true } }.Start();
            }  
        }

        public void Log()
        {
            Debug.WriteLine($"LOG SETTINGS");
            Debug.WriteLine(JsonConvert.SerializeObject(this, Formatting.Indented));
        }
    }
}
