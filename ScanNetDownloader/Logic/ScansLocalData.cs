using Newtonsoft.Json;
using System.Diagnostics;
using System.IO;
using System.Windows;

namespace ScanNetDownloader.Logic
{
    class ScansLocalData
    {
        private static ScansLocalData _instance;

        public static ScansLocalData Instance
        {
            get { return _instance; }
            set { _instance = value; }
        }

        /// <summary>
        /// A list containing saved ScanWebsiteUrl
        /// </summary>
        public List<ScanWebsiteUrl> ScanUrlList { get; set; } = new List<ScanWebsiteUrl>();

        public static void InitializeScansData()
        {
            Instance = LoadData(Constants.SCANSLOCALDATA_JSON_PATH);
            Instance.Log();
        }

        private static ScansLocalData LoadData(string jsonPath)
        {
            ScansLocalData loadedData;

            JsonSerializerSettings serializerSettings = new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.All
            };

            if (File.Exists(jsonPath))
            {
                try
                {
                    loadedData = JsonConvert.DeserializeObject<ScansLocalData>(File.ReadAllText(Constants.SCANSLOCALDATA_JSON_PATH), serializerSettings);
                    return loadedData;
                }
                catch (Exception ex)
                {
                    // TODO: Error Manamegement while loading scan data
                    //Error.FailedToLoadScansLocalData(jsonPath, ex);

                    // TODO: Redo error management to fit with WPF version
                    string mBoxMessage = $"Error while loading scan local data, do you want to clear the data ?";
                    string mBoxCaption = "Continue ?";
                    Debug.WriteLine($"{mBoxMessage}\n {ex}");
                    
                    MessageBoxResult result = MessageBox.Show(mBoxMessage, mBoxCaption, MessageBoxButton.YesNo, MessageBoxImage.Warning);

                    if (result == MessageBoxResult.Yes)
                    {
                        loadedData = ClearLocalData();
                        return loadedData;
                    }
                    else // TODO: What to do in this case with WPF app ?
                    {
                        mBoxMessage = "Please make sure nothing is wrong with the data in ScansLocalData.json, if the problem persist backup your data and reset the json to it's default values.";
                        mBoxCaption = "Scan data error";
                        Debug.WriteLine(mBoxMessage);
                        MessageBox.Show(mBoxMessage, mBoxCaption, MessageBoxButton.OK, MessageBoxImage.Warning);

                        OpenJsonFile();
                        App.Quit();
                        return null;
                    }
                }
            }
            else // Missing Settings.json
            {
                // TODO: Error Manamegement missing scan data json file
                //Error.MissingSettingsJson(jsonPath);
                loadedData = ClearLocalData();
                return loadedData;
            }
        }

        public static void Update(List<ScanWebsiteUrl> newScansUrlList)
        {
            Instance.ScanUrlList = newScansUrlList; // TODO: Create a function for this ? Check reference Equals -> no need to assign if ref equals + help to know when instance is replaced  ?
            Save(Instance); // Save the scans local data 
        }

        private static void Save(ScansLocalData newScanLocalData)
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

            Instance = newScanLocalData;

            try
            {
                File.WriteAllText(Constants.SCANSLOCALDATA_JSON_PATH, JsonConvert.SerializeObject(Instance, serializerSettings));
            }
            catch (Exception ex)
            {
                Error.FailedToSaveSettingsJson(Constants.SCANSLOCALDATA_JSON_PATH, ex);
            }
        }

        private static ScansLocalData ClearLocalData()
        {
            ScansLocalData defaultSettings = new ScansLocalData();
            Save(defaultSettings);
            return defaultSettings;
        }

        public static void OpenJsonFile()
        {
            if (Settings.Instance != null && Settings.Instance.AutoOpenJsonWhenNecessary)
            {
                new Process { StartInfo = new ProcessStartInfo(Constants.SCANSLOCALDATA_JSON_PATH) { UseShellExecute = true } }.Start();
            }
        }

        public void Log()
        {
            Debug.WriteLine($"LOG SCANS DATA");
            Debug.WriteLine(JsonConvert.SerializeObject(this, Formatting.Indented));
        }
    }
}
