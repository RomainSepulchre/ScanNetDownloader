using Newtonsoft.Json;
using ScanNetDownloader.Logic.Helpers;
using ScanNetDownloader.View;
using System.Diagnostics;
using System.IO;

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

        private static string scansDataPath;

        public static void InitializeScansData(bool firstLaunch)
        {
#if DEBUG
            scansDataPath = Constants.DEBUG_SCANSLOCALDATA_JSON_PATH;
#else
            scansDataPath = Constants.SCANSLOCALDATA_JSON_PATH;
#endif
            Instance = LoadData(firstLaunch);
            //Instance.Log();
        }

        private static ScansLocalData LoadData(bool firstLaunch)
        {
            ScansLocalData loadedData;

            JsonSerializerSettings serializerSettings = new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.All
            };

            if (File.Exists(scansDataPath))
            {
                try
                {
                    loadedData = JsonConvert.DeserializeObject<ScansLocalData>(File.ReadAllText(scansDataPath), serializerSettings);
                    if (loadedData == null) throw new Exception($"Loaded scan local data should never be null, something wrong happened during json deserialization");
                    return loadedData;
                }
                catch (Exception ex)
                {
                    Error.FailedToLoadScansLocalData(scansDataPath, ex);

                    string mBoxMessage = $"Error while loading scan local data, do you want to clear the data ?\nAll the previous data of the scan manager view will be lost but you will keep everything you already downloaded.";
                    string mBoxCaption = "Clear local scan data ?";
                    Debug.WriteLine($"{mBoxMessage}\n {ex}");
                    
                    YesNoWindow yesNoWindow = MsgWindow.ShowYesNoWindow(true, mBoxCaption, mBoxMessage, false, MsgWindow.ImageType.Warning);

                    if (yesNoWindow.Success)
                    {
                        loadedData = ClearLocalData();
                        return loadedData;
                    }
                    else //
                         // : What to do in this case with WPF app ? Is there a better solution ?
                    {
                        mBoxMessage = "Please make sure nothing is wrong with the data in ScansLocalData.json, if the problem persist backup your data and reset the json to it's default values.";
                        mBoxCaption = "Scan data loading error";
                        Debug.WriteLine(mBoxMessage);
                        MsgWindow.ShowOkWindow(true, mBoxCaption, mBoxMessage, false, MsgWindow.ImageType.Error);

                        OpenJsonFile();
                        App.Quit();
                        return null;
                    }
                }
            }
            else // No scan data yet, create the scan data
            {
                if (!firstLaunch)
                {
                    Error.MissingScanDataJson(scansDataPath);
                }

                loadedData = ClearLocalData();
                return loadedData;
            }
        }

        public static ScanDataImportResult ImportScanData(string importPath)
        {
            JsonSerializerSettings serializerSettings = new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.All
            };

            try
            {
                ScansLocalData scanDataToImport = JsonConvert.DeserializeObject<ScansLocalData>(File.ReadAllText(importPath), serializerSettings);
                if (scanDataToImport == null) throw new Exception($"Failed to get data from provided scan data json, the data is null. Json is an empty file or something went wrong during json deserialization.");

                // Merge with current scan data
                Instance.ScanDataList.AddRange(scanDataToImport.ScanDataList);
                Save();
                return new ScanDataImportResult(true, scanDataToImport.ScanDataList.Count);
            }
            catch (Exception ex)
            {
                return new ScanDataImportResult(false, ex);
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

            JsonSerializerSettings serializerSettings = new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.All,
                Formatting = Formatting.Indented
            };

            try
            {
                File.WriteAllText(scansDataPath, JsonConvert.SerializeObject(Instance, serializerSettings));
            }
            catch (Exception ex)
            {
                Error.FailedToSaveScansLocalData(scansDataPath, ex);
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
                new Process { StartInfo = new ProcessStartInfo(scansDataPath) { UseShellExecute = true } }.Start();
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
