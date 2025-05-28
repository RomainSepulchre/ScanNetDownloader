using Microsoft.Win32;
using Newtonsoft.Json;
using ScanNetDownloader.Logic;
using ScanNetDownloader.Logic.Helpers;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;

namespace ScanNetDownloader.View.CustomControls
{
    /// <summary>
    /// Logique d'interaction pour OptionsView.xaml
    /// </summary>
    /// 

    //
    // : prevent to change options while downloading
    public partial class OptionsView : UserControl, INotifyPropertyChanged
    {
        public bool OptionsChangesNotSaved { get; private set; } = false;

        private string _outputDirectoryPath;
        public string OutputDirectoryPath
        {
            get { return _outputDirectoryPath; }
            set
            {
                _outputDirectoryPath = value;
                OnPropertyChanged();
                CheckForChangedSettings();
            }
        }

        private bool _openOutputDirAfterDownload;
        public bool OpenOutputDirAfterDownload
        {
            get { return _openOutputDirAfterDownload; }
            set
            {
                _openOutputDirAfterDownload = value;
                OnPropertyChanged();
                CheckForChangedSettings();
            }
        }

        private bool _errorPauseApp;
        public bool ErrorPauseApp
        {
            get { return _errorPauseApp; }
            set
            {
                _errorPauseApp = value;
                OnPropertyChanged();
                CheckForChangedSettings();
            }
        }

        private bool _createCbzAfterDownload;
        public bool CreateCbzAfterDownload
        {
            get { return _createCbzAfterDownload; }
            set
            {
                _createCbzAfterDownload = value;
                OnPropertyChanged();
                CheckForChangedSettings();
            }
        }

        private bool _deleteImagesAfterCbzCreation;
        public bool DeleteImagesAfterCbzCreation
        {
            get { return _deleteImagesAfterCbzCreation; }
            set
            {
                _deleteImagesAfterCbzCreation = value;
                OnPropertyChanged();
                CheckForChangedSettings();
            }
        }

        private string _chooseOutputDirectoryErrorMsg;
        public string ChooseOutputDirectoryErrorMsg
        {
            get { return _chooseOutputDirectoryErrorMsg; }
            set
            {
                _chooseOutputDirectoryErrorMsg = value;
                OnPropertyChanged();
            }
        }

        private string _openOutputDirectoryErrorMsg;
        public string OpenOutputDirectoryErrorMsg
        {
            get { return _openOutputDirectoryErrorMsg; }
            set {
                _openOutputDirectoryErrorMsg = value;
                OnPropertyChanged();
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        public static RoutedEvent ClearLocalDataEvent = EventManager.RegisterRoutedEvent(nameof(ClearLocalData), RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(OptionsView));
        public event RoutedEventHandler ClearLocalData
        {
            add { AddHandler(ClearLocalDataEvent, value); }
            remove { RemoveHandler(ClearLocalDataEvent, value); }
        }

        public OptionsView()
        {
            DataContext = this;

            InitializeComponent();

            if (Settings.Instance != null) // To prevent XAML compilation error
            {
                RefreshSettings();
            }      
        }

        private void OnPropertyChanged([CallerMemberName] string property = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(property));
        }

        private void btnChooseOutputDir_Click(object sender, RoutedEventArgs e)
        {
            OpenFolderDialog folderDialog = new OpenFolderDialog();
            folderDialog.Title = "Select download directory";
            folderDialog.Multiselect = false;

            bool? success = folderDialog.ShowDialog();

            if (success == true)
            {
                OutputDirectoryPath = folderDialog.FolderName;
                HideErrorMessages();

                SaveSettings();
            }
        }

        private void btnOpenOutputDir_Click(object sender, RoutedEventArgs e)
        {
            if (Directory.Exists(Settings.Instance.OutputDirectory))
            {
                HideErrorMessages();
                FileManagement.OpenFolder(Settings.Instance.OutputDirectory);
            }
            else
            {
                OpenOutputDirectoryErrorMsg = "Directory doesn't exist, impossible to open it";
                errorAlertOpenOutputDir.Visibility = Visibility.Visible;
            }
        }

        private void btnImportScanData_Click(object sender, RoutedEventArgs e)
        {
            // Give user some informations before importing scan data
            string header = "Import scan data";
            string msg = $"Select a ScanLocalData.json file to import its data, the data will be merged to your current data.\n\nIf you want to replace your current data rather than merge, clear your scan data before doing the data importation.\n\nAre you ready to import scan data?";
            YesNoWindow yesNoWindow = MsgWindow.ShowYesNoWindow(header, msg, true, MsgWindow.ImageType.Information);
            if (yesNoWindow.Success)
            {
                OpenFileDialog fileDialog = new OpenFileDialog();
                fileDialog.Title = "Select the scan data json file";
                fileDialog.Multiselect = false;
                fileDialog.Filter = "Json files (*.json)|*.json";

                bool? success = fileDialog.ShowDialog();

                if (success == true)
                {
                    string scanDataPath = fileDialog.FileName;

                    Debug.WriteLine("IMPORT SCAN DATA PATH:");
                    Debug.WriteLine(scanDataPath);

                    ScanDataImportResult result = ScansLocalData.ImportScanData(scanDataPath);

                    string resultHeader;
                    string resultMessage;
                    MsgWindow.ImageType msgType = MsgWindow.ImageType.Information;
                    if (!result.Success)
                    {
                        resultHeader = "Failed - Are you sure this is a scan data json ?";
                        resultMessage = $"Failed to import scan data from: {scanDataPath}.\n\n{result.Exception.Message}";
                        msgType = MsgWindow.ImageType.Error;
                    }
                    else
                    {
                        resultHeader = "Scan data importation done";
                        resultMessage = $"You have successfully imported scan data from: {scanDataPath}.";
                    }
                    MsgWindow.ShowOkWindow(resultHeader, resultMessage, false, msgType);
                }
            }            
        }

        private void btnImportSettings_Click(object sender, RoutedEventArgs e)
        {
            // Give user some informations before importing settings
            string header = "Import settings";
            string msg = $"Select a Settings.json file to import its data, your current settings will be replaced by the settings from the json file.\n\nAre you ready to import settings?";
            YesNoWindow yesNoWindow = MsgWindow.ShowYesNoWindow(header, msg, true, MsgWindow.ImageType.Information);
            if (yesNoWindow.Success)
            {
                OpenFileDialog fileDialog = new OpenFileDialog();
                fileDialog.Title = "Select the settings json file";
                fileDialog.Multiselect = false;
                fileDialog.Filter = "Json files (*.json)|*.json";

                bool? success = fileDialog.ShowDialog();

                if (success == true)
                {
                    string settingsPath = fileDialog.FileName;

                    Debug.WriteLine("IMPORT SETTINGS PATH:");
                    Debug.WriteLine(settingsPath);

                    SettingsImportResult result = Settings.ImportSettings(settingsPath);

                    string resultHeader;
                    string resultMessage;
                    MsgWindow.ImageType msgType = MsgWindow.ImageType.Information;
                    if (!result.Success)
                    {
                        resultHeader = "Failed - Are you sure this is a settings json ?";
                        resultMessage = $"Failed to import settings from: {settingsPath}.\n\n{result.Exception.Message}";
                        msgType = MsgWindow.ImageType.Error;
                    }
                    else
                    {
                        resultHeader = "Settings importation done";
                        resultMessage = $"You have successfully imported settings from: {settingsPath}.";
                    }
                    MsgWindow.ShowOkWindow(resultHeader, resultMessage, false, msgType);
                }
            }
        }

        private void btnClearScanData_Click(object sender, RoutedEventArgs e)
        {
            // Ask user before deleting scan item
            string header = "Clear local scan data ?";
            string msg = $"Are you sure you want to clear all your scan data ?\n\nLocal scan data are the information displayed in the scan manager view, local files such as downloaded images and .CBZ archive won't be deleted.";
            YesNoWindow yesNoWindow = MsgWindow.ShowYesNoWindow(header, msg, true, MsgWindow.ImageType.Warning);
            if (yesNoWindow.Success)
            {
                ScansLocalData.Instance.ClearScanData();
                RaiseEvent(new RoutedEventArgs(ClearLocalDataEvent, this));
            }
        }

        private void btnSaveOptions_Click(object sender, RoutedEventArgs e)
        {
            SaveSettings();
        }

        public void RefreshSettings()
        {
            Debug.WriteLine("REFRESH SETTINGS");

            HideErrorMessages();

            Settings settings = Settings.Instance;

            OutputDirectoryPath = settings.OutputDirectory;
            CreateCbzAfterDownload = settings.CreateCbzArchive;
            DeleteImagesAfterCbzCreation = settings.DeleteImagesAfterCbzCreation;
            OpenOutputDirAfterDownload = settings.OpenOutputDirectoryAfterDownload;
            ErrorPauseApp = settings.ErrorPauseApp;

            CheckForChangedSettings(false);
        }

        public void SaveSettings()
        {
            bool saveOutpurDir = true;
            if (OutputDirectoryPath != Settings.Instance.OutputDirectory && Directory.Exists(OutputDirectoryPath) == false)
            {
                errorMsgChooseOutputDir.Visibility = Visibility.Visible;
                ChooseOutputDirectoryErrorMsg = "Directory doesn't exist, impossible to save it as a download location";
                saveOutpurDir = false;
            }
            else
            {
                HideErrorMessages();
            }

            if (saveOutpurDir) Settings.Instance.OutputDirectory = OutputDirectoryPath;
            Settings.Instance.OpenOutputDirectoryAfterDownload = OpenOutputDirAfterDownload;
            Settings.Instance.ErrorPauseApp = ErrorPauseApp;
            Settings.Instance.CreateCbzArchive = CreateCbzAfterDownload;
            Settings.Instance.DeleteImagesAfterCbzCreation = DeleteImagesAfterCbzCreation;          

            Settings.Save();

            CheckForChangedSettings(false);
        }

        /// <summary>
        /// Check and define if some options have been changed and not saved
        /// </summary>
        /// <param name="Changed_Override">If not null, override the AreSettingsChanged() check and use passed value as AreSettingsChanged() result</param>
        private void CheckForChangedSettings(bool? Changed_Override=null)
        {
            if (Changed_Override != null) OptionsChangesNotSaved = (bool)Changed_Override;
            else OptionsChangesNotSaved = AreSettingsChanged();

            btnSaveOptions.IsEnabled = OptionsChangesNotSaved;
        }

        private bool AreSettingsChanged()
        {
            if (Settings.Instance == null) return false; // To prevent XAML compilation error

            // TODO: A bit overkilled and not ideal but fastest way for now
            Settings settings = Settings.Instance;

            if (OutputDirectoryPath != settings.OutputDirectory) return true;
            if (OpenOutputDirAfterDownload != settings.OpenOutputDirectoryAfterDownload) return true;
            if (ErrorPauseApp != settings.ErrorPauseApp) return true;
            if (CreateCbzAfterDownload != settings.CreateCbzArchive) return true;
            if (DeleteImagesAfterCbzCreation != settings.DeleteImagesAfterCbzCreation) return true;

            return false;
        }

        private void HideErrorMessages()
        {
            if(errorAlertOpenOutputDir.Visibility == Visibility.Visible)
            {
                OpenOutputDirectoryErrorMsg = "";
                errorAlertOpenOutputDir.Visibility = Visibility.Collapsed;
            }

            if (errorMsgChooseOutputDir.Visibility == Visibility.Visible)
            {
                ChooseOutputDirectoryErrorMsg = "";
                errorMsgChooseOutputDir.Visibility = Visibility.Collapsed;
            }
        }
    }
}
