using ScanNetDownloader.Logic;
using ScanNetDownloader.Logic.Helpers;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace ScanNetDownloader.View.CustomControls
{
    /// <summary>
    /// Logique d'interaction pour DuplicateItem.xaml
    /// </summary>
    public partial class DuplicateItem : UserControl, INotifyPropertyChanged
    {
        public enum DuplicateOptions
        {
            Delete = 0,
            Keep = 1,
            Replace = 2
        };

        public DuplicateOptions DuplicateOption { get; private set; }

        public ScanData duplicateScanData { get; private set; }

        public ScanData currentScanData { get; private set; }

        private double textAvailableWidth = 467;

        private string _itemLabel;
        public string ItemLabel
        {
            get { return _itemLabel; }
            set
            {
                _itemLabel = value;
                OnPropertyChanged();
            }
        }

        private string _duplicateScanInfo;
        public string DuplicateScanInfo
        {
            get { return _duplicateScanInfo; }
            set
            {
                _duplicateScanInfo = value;
                OnPropertyChanged();
            }
        }

        private string _duplicateScanUrl;
        public string DuplicateScanUrl
        {
            get { return _duplicateScanUrl; }
            set
            {
                _duplicateScanUrl = value;
                OnPropertyChanged();
            }
        }

        private string _currentScanInfo;
        public string CurrentScanInfo
        {
            get { return _currentScanInfo; }
            set
            {
                _currentScanInfo = value;
                OnPropertyChanged();
            }
        }

        private string _currentScanUrl;
        public string CurrentScanUrl
        {
            get { return _currentScanUrl; }
            set
            {
                _currentScanUrl = value;
                OnPropertyChanged();
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string property = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(property));
        }

        public DuplicateItem()
        {
            InitializeComponent();

            DataContext = this;
        }

        public DuplicateItem(DuplicatedScanData duplicate)
        {
            InitializeComponent();

            DataContext = this;

            duplicateScanData = duplicate.DuplicatedData;
            currentScanData = duplicate.CurrentData;

            DuplicateScanInfo = $"{duplicateScanData.BookName} - Chapter {duplicateScanData.ChapterId} ({duplicateScanData.PagesCount} pages)";
            DuplicateScanUrl = duplicateScanData.Url;
            

            CurrentScanInfo = $"{currentScanData.BookName} - Chapter {currentScanData.ChapterId} ({currentScanData.PagesCount} pages)"; ;
            CurrentScanUrl = currentScanData.Url;

            DuplicateOption = DuplicateOptions.Delete;
            cbDuplicateOptions.SelectedIndex = (int)DuplicateOption;

            ItemLabel = "Duplicate";
        }

        private void imgSeeCurrent_MouseEnter(object sender, MouseEventArgs e)
        {
            ItemLabel = "Current data";
            labelRect.Fill = (SolidColorBrush)FindResource(ResourcesKey.ColorBrushes.Green);
            stPanelCurrentData.Visibility = Visibility.Visible;
        }

        private void imgSeeCurrent_MouseLeave(object sender, MouseEventArgs e)
        {
            ItemLabel = "Duplicate";
            labelRect.Fill = (SolidColorBrush)FindResource(ResourcesKey.ColorBrushes.Orange);
            stPanelCurrentData.Visibility = Visibility.Hidden;
        }

        public void SetComboBoxSelection(DuplicateOptions newOption)
        {
            cbDuplicateOptions.SelectedIndex = (int)newOption;
        }

        private void cbDuplicateOptions_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            DuplicateOption = (DuplicateOptions)cbDuplicateOptions.SelectedIndex;
        }
    }
}
