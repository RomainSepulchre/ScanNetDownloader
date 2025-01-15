using Newtonsoft.Json;
using ScanNetDownloader.Logic.Helpers;
using ScanNetDownloader.View;
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
        public bool ErrorPauseApp { get; set; } = true;

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

        public static void InitializeAppSettings()
        {
            Instance = LoadSettings(Constants.SETTINGS_JSON_PATH);
            Instance.Log();
        }

        private static Settings LoadSettings(string jsonPath)
        {
            Settings loadedSettings;

            JsonSerializerSettings serializerSettings = new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.All
            };

            if (File.Exists(jsonPath))
            {
                try
                {
                    loadedSettings = JsonConvert.DeserializeObject<Settings>(File.ReadAllText(Constants.SETTINGS_JSON_PATH), serializerSettings);
                    return loadedSettings;
                }
                catch (Exception ex)
                {
                    Error.FailedToLoadSettingsJson(jsonPath, ex);

                    // TODO: Redo error management to fit with WPF version
                    string mBoxMessage = $"No settings found. Do you want to reset settings.json to it's default values ?";
                    string mBoxCaption = "No settings found";
                    Debug.WriteLine(mBoxMessage);

                    YesNoWindow yesNoWindow = MsgWindow.ShowYesNoWindow(mBoxCaption, mBoxMessage, false, MsgWindow.ImageType.Warning);

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
                        MsgWindow.ShowOkWindow(mBoxCaption, mBoxMessage, false, MsgWindow.ImageType.Error);

                        OpenJsonFile();
                        App.Quit();
                        return null;
                    }
                }
            }
            else // Missing Settings.json
            {
                Error.MissingSettingsJson(jsonPath);
                loadedSettings = ResetToDefault();
                return loadedSettings;
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
                File.WriteAllText(Constants.SETTINGS_JSON_PATH, JsonConvert.SerializeObject(Instance, serializerSettings));
            }
            catch (Exception ex)
            {
                Error.FailedToSaveSettingsJson(Constants.SETTINGS_JSON_PATH, ex);
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
                    new Process { StartInfo = new ProcessStartInfo(Constants.SETTINGS_JSON_PATH) { UseShellExecute = true } }.Start();
                }
            }
            else
            {
                new Process { StartInfo = new ProcessStartInfo(Constants.SETTINGS_JSON_PATH) { UseShellExecute = true } }.Start();
            }  
        }

        public void Log()
        {
            Debug.WriteLine($"LOG SETTINGS");
            Debug.WriteLine(JsonConvert.SerializeObject(this, Formatting.Indented));
        }
    }
}
