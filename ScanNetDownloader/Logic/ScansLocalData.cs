using Newtonsoft.Json;
using ScanNetDownloader.Logic.Helpers;
using ScanNetDownloader.View;
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
        /// A list containing saved ScanData
        /// </summary>
        public List<ScanData> ScanDataList { get; set; } = new List<ScanData>();

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
                    Error.FailedToLoadScansLocalData(jsonPath, ex);

                    // TODO: Redo error management to fit with WPF version
                    string mBoxMessage = $"Error while loading scan local data, do you want to clear the data ?\nAll the previous data of the scan manager view will be lost but you will keep everything you already downloaded.";
                    string mBoxCaption = "Clear local scan data ?";
                    Debug.WriteLine($"{mBoxMessage}\n {ex}");
                    
                    YesNoWindow yesNoWindow = MsgWindow.ShowYesNoWindow(mBoxCaption, mBoxMessage, false, MsgWindow.ImageType.Warning);

                    if (yesNoWindow.Success)
                    {
                        loadedData = ClearLocalData();
                        return loadedData;
                    }
                    else // TODO: What to do in this case with WPF app ?
                    {
                        mBoxMessage = "Please make sure nothing is wrong with the data in ScansLocalData.json, if the problem persist backup your data and reset the json to it's default values.";
                        mBoxCaption = "Scan data loading error";
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
                // TODO: Error Management missing scan data json file
                //Error.MissingSettingsJson(jsonPath);
                loadedData = ClearLocalData();
                return loadedData;
            }
        }

        public static void Save(ScansLocalData newScanLocalData=null)
        {
            // If we specify newScanLocalData, they replace the instance otherwise we save our ScansLocalData instance
            if (newScanLocalData != null)
            {
                if (ReferenceEquals(Instance, newScanLocalData) == false) // Make sure we didn't provide a reference of instance as argument
                {
                    Instance = newScanLocalData;
                }
            }

            // TODO: Delete Update and do the same change done with Settings on Save()
            JsonSerializerSettings serializerSettings = new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.All,
                Formatting = Formatting.Indented
            };

            try
            {
                File.WriteAllText(Constants.SCANSLOCALDATA_JSON_PATH, JsonConvert.SerializeObject(Instance, serializerSettings));
            }
            catch (Exception ex)
            {
                Error.FailedToSaveScansLocalData(Constants.SCANSLOCALDATA_JSON_PATH, ex);
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

        public void ClearScanData()
        {
            ClearLocalData();
        }

        public void Log()
        {
            Debug.WriteLine($"LOG SCANS DATA");
            Debug.WriteLine(JsonConvert.SerializeObject(this, Formatting.Indented));
        }
    }
}
