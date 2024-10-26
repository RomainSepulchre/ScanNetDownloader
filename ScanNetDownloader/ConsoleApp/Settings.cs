using Newtonsoft.Json;
using System.Diagnostics;
using System.IO;
using System.Windows;

namespace ScanNetDownloader.ConsoleApp
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

        /// <summary>
        /// Should the program pause the app and wait for an user input when an error is triggered (Default=True)
        /// A major error requiring user input bypass this and pause the app anyway
        /// </summary>
        public bool ErrorsPauseApp { get; set; } = true;

        /// <summary>
        /// Should the program automatically open Settings.json when you need to check something in it (Default=True)
        /// </summary>
        public bool AutoOpenJsonWhenNecessary { get; set; } = true;

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
                    Debug.WriteLine($"Do you want to reset settings.json to it's default values ?");
                    if (Program.WaitForYesOrNoMsgBox($"Do you want to reset settings.json to it's default values ?") == MessageBoxResult.Yes)
                    {
                        loadedSettings = ResetToDefault();
                        return loadedSettings;
                    }
                    else // What to do in this case with WPF app ?
                    {
                        Debug.WriteLine("Please make sure nothing is wrong with the value in Settings.json, if the problem persist backup your settings and reset the json to it's default values.");

                        if (Instance != null && Instance.AutoOpenJsonWhenNecessary)
                        {
                            Debug.WriteLine("Press any key to open Settings.json and close the app...");
                            MessageBox.Show("Press ok to open Settings.json and close the app...", "Quit app", MessageBoxButton.OK, MessageBoxImage.Warning);
                        }
                        else
                        {
                            Debug.WriteLine("Press any key close the app...");
                            MessageBox.Show("The app will be closed...", "Quit app", MessageBoxButton.OK, MessageBoxImage.Warning);
                        }

                        OpenJsonFile();
                        Program.QuitApp();
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

        public static void Update(Settings newSettings)
        {
            Instance = newSettings;
            Save(Instance);
        }

        private static void Save(Settings newSettings)
        {
            JsonSerializerSettings serializerSettings = new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.All,
                Formatting = Formatting.Indented
            };

            // TODO: Should I replace only if reference is different so I know when I replace the initial instance ?
            //if(ReferenceEquals(Instance, newSettings) == false) 
            //{
            //    Instance = newSettings;
            //}

            Instance = newSettings;

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
            if (Instance != null && Instance.AutoOpenJsonWhenNecessary)
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
