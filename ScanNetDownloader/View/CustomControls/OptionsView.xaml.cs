using Microsoft.Win32;
using ScanNetDownloader.ConsoleApp;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Channels;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ScanNetDownloader.View.CustomControls
{
    /// <summary>
    /// Logique d'interaction pour OptionsView.xaml
    /// </summary>
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

        public event PropertyChangedEventHandler? PropertyChanged;

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
            OpenFolderDialog fileDialog = new OpenFolderDialog();
            fileDialog.Title = "Select download directory";
            fileDialog.Multiselect = false;

            bool? success = fileDialog.ShowDialog();

            if (success == true)
            {
                txtBoxOutputDir.Text = fileDialog.FolderName;
                // TODO: Do I save here or wait for Clicking on Save ?
                //SaveSettings();
            }
        }

        private void btnSaveOptions_Click(object sender, RoutedEventArgs e)
        {
            SaveSettings();
        }

        private void btnDbgOpenSettingsJson_Click(object sender, RoutedEventArgs e)
        {
            Settings.OpenJsonFile();
        }

        public void RefreshSettings()
        {
            Debug.WriteLine("REFRESH SETTINGS");

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
            // TODO: check if output directory is a valid directory before saving

            Settings.Instance.OutputDirectory = OutputDirectoryPath;
            Settings.Instance.OpenOutputDirectoryAfterDownload = OpenOutputDirAfterDownload;
            Settings.Instance.ErrorPauseApp = ErrorPauseApp;
            Settings.Instance.CreateCbzArchive = CreateCbzAfterDownload;
            Settings.Instance.DeleteImagesAfterCbzCreation = DeleteImagesAfterCbzCreation;          

            Settings.Update(Settings.Instance);

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

            // TODO: A bit overkilled and not ideal but fastes way for now
            Settings settings = Settings.Instance;

            if (OutputDirectoryPath != settings.OutputDirectory) return true;
            if (OpenOutputDirAfterDownload != settings.OpenOutputDirectoryAfterDownload) return true;
            if (ErrorPauseApp != settings.ErrorPauseApp) return true;
            if (CreateCbzAfterDownload != settings.CreateCbzArchive) return true;
            if (DeleteImagesAfterCbzCreation != settings.DeleteImagesAfterCbzCreation) return true;

            return false;
        }
    }
}
