using ScanNetDownloader.Logic.Helpers;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace ScanNetDownloader.View.CustomControls
{
    /// <summary>
    /// Logique d'interaction pour ScanDataCreationItem.xaml
    /// </summary>
    public partial class ScanDataCreationItem : UserControl, INotifyPropertyChanged
    {
        public string BookName { get; set; }

        public int ChapterNumber { get; set; }


        private string _scanDataCreationInfos;
        public string ScanDataCreationInfos
        {
            get { return _scanDataCreationInfos; }
            set {
                _scanDataCreationInfos = value;
                OnPropertyChanged();
            }
        }

        private string _statusMessage;
        public string StatusMessage
        {
            get { return _statusMessage; }
            set {
                _statusMessage = value;
                OnPropertyChanged();
            }
        }

        private string _errorMessage;
        public string ErrorMessage
        {
            get { return _errorMessage; }
            set {
                _errorMessage = value;
                OnPropertyChanged();
            }
        }

        private bool _showError;
        public bool ShowError
        {
            get { return _showError; }
            set {
                _showError = value;
                ShowErrorMessage(_showError);
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        private void OnPropertyChanged([CallerMemberName] string property = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(property));
        }

        public ScanDataCreationItem()
        {
            DataContext = this;
            InitializeComponent();
        }

        public ScanDataCreationItem(string bookName, int chapterNumber)
        {
            DataContext = this;
            InitializeComponent();

            BookName = bookName;
            ChapterNumber = chapterNumber;
            ScanDataCreationInfos = $"Retrieve scan data for {BookName} - {ChapterNumber}";

            StatusMessage = "In progress...";
            txtBlockStatus.Foreground = (SolidColorBrush)FindResource(ResourcesKey.ColorBrushes.DarkGrey);
            rectStatusBar.Fill = (SolidColorBrush)FindResource(ResourcesKey.ColorBrushes.MediumGrey);

            ShowError = false;
        }

        public void RetrieveDataSuccess(string msg)
        {
            StatusMessage = msg;
            SolidColorBrush successColor = (SolidColorBrush)FindResource(ResourcesKey.ColorBrushes.Green);
            txtBlockStatus.Foreground = successColor;
            rectStatusBar.Fill = successColor;
        }

        public void RetrieveDataFailed(string errorMsg)
        {
            StatusMessage = $"Failed";   
            ErrorMessage = errorMsg;
            ShowError = true;
            SolidColorBrush failedColor = (SolidColorBrush)FindResource(ResourcesKey.ColorBrushes.Red);
            txtBlockStatus.Foreground = failedColor;
            rectStatusBar.Fill = failedColor;
        }

        public void ShowErrorMessage(bool show)
        {
            if (show)
            {
                txtBlockErrorMsg.Visibility = Visibility.Visible;
            }
            else
            {
                txtBlockErrorMsg.Visibility = Visibility.Collapsed;
            }
        }
    }
}
