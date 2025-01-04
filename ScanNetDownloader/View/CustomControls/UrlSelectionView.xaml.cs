using ScanNetDownloader.Logic;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
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
    /// Logique d'interaction pour UrlSelectionView.xaml
    /// </summary>
    public partial class UrlSelectionView : UserControl, INotifyPropertyChanged
    {
        public ScanData TempScanData;

        private string _urlInput;
        public string UrlInput
        {
            get { return _urlInput; }
            set {
                _urlInput = value;
                OnPropertyChanged();
            }
        }

        private string _selectUrlInfos;
        public string SelectUrlInfos
        {
            get { return _selectUrlInfos; }
            set {
                _selectUrlInfos = value;
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


        public event PropertyChangedEventHandler? PropertyChanged;

        // View Events
        public static RoutedEvent UrlConfirmedEvent = EventManager.RegisterRoutedEvent(nameof(UrlConfirmed), RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(UrlSelectionView));      

        public event RoutedEventHandler UrlConfirmed
        {
            add { AddHandler(UrlConfirmedEvent, value); }
            remove { RemoveHandler(UrlConfirmedEvent, value); }
        }


        public UrlSelectionView()
        {
            DataContext = this;
            InitializeComponent();

            // Setup info
            SelectUrlInfos = "Enter a scan URL...\n";
            SelectUrlInfos += "\nCompatibles websites are:";
            foreach(string website in Constants.COMPATIBLE_SCAN_WEBSITES)
            {
                SelectUrlInfos += $"\n-{website} (ex: {Constants.EXAMPLE_URLS[website]})";
            }

#if DEBUG
            // For easier debug
            //UrlInput = "https://www.scan-vf.net/jujutsu-kaisen/chapitre-18/1";
            UrlInput = "https://anime-sama.fr/catalogue/20th-century-boys/scan-21st-century-boys/vf/";
#endif

            btnNext.IsEnabled = string.IsNullOrEmpty(UrlInput) == false;
        }


        private void OnPropertyChanged([CallerMemberName] string property = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(property));
        }

        #region Buttons and Ui Events

        private void txtBoxUrlInput_TextInputChanged(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(UrlInput) == false && btnNext != null)
            {
                btnNext.IsEnabled = true;
            }
            else if (btnNext != null && btnNext.IsEnabled) btnNext.IsEnabled = false;

            if (txtBoxUrlInput.BorderThickness == new Thickness(2))
            {
                txtBoxUrlInput.ClearValue(Control.BorderBrushProperty);
                txtBoxUrlInput.ClearValue(Control.BorderThicknessProperty);
            }
        }

        private async void btnNext_Click(object sender, RoutedEventArgs e)
        {
            UrlValidityResult urlTestResult = await ScanManagement.IsValidScanUrl(UrlInput);
            if (urlTestResult.Success) // TODO: Find a way to know if the url is valid for each website
            {
                Debug.WriteLine($"URL INPUT = {UrlInput}");

                TempScanData = ScanManagement.CreateTemporaryScanData(UrlInput);
                if (TempScanData == null)
                {
                    errorAlertUrl.Visibility = Visibility.Visible;
                    ErrorMessage = "Impossible to create a ScanData object from url";
                    txtBoxUrlInput.BorderBrush = (SolidColorBrush)FindResource("Colors.Red");
                    txtBoxUrlInput.BorderThickness = new Thickness(2);
                    return;
                }

                // Reset Error
                errorAlertUrl.Visibility = Visibility.Collapsed;
                txtBoxUrlInput.ClearValue(Control.BorderBrushProperty);
                txtBoxUrlInput.ClearValue(Control.BorderThicknessProperty);

                RaiseEvent(new RoutedEventArgs(UrlConfirmedEvent, this));
            }
            else
            {
                //TODO: Show error, Add text explanation
                Debug.WriteLine(urlTestResult.InvalidityReason);
                errorAlertUrl.Visibility = Visibility.Visible;
                ErrorMessage = urlTestResult.InvalidityReason;
                txtBoxUrlInput.BorderBrush = (SolidColorBrush)FindResource("Colors.Red");
                txtBoxUrlInput.BorderThickness = new Thickness(2);
                return;
            }
        }
        #endregion
    }
}
