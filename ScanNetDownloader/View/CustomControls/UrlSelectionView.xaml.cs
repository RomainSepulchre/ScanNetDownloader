using ScanNetDownloader.Logic;
using ScanNetDownloader.Logic.Helpers;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

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

        private ObservableCollection<WebsiteUrlWithExample> _websitesWithExampleUrls = new ObservableCollection<WebsiteUrlWithExample>();
        public ObservableCollection<WebsiteUrlWithExample> WebsitesWithExampleUrls
        {
            get { return _websitesWithExampleUrls; }
            set { _websitesWithExampleUrls = value; }
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

            // Setup compatible website and examples
            foreach(string website in Constants.COMPATIBLE_SCAN_WEBSITES)
            {
                WebsiteUrlWithExample newWebsiteUrl = new WebsiteUrlWithExample(website, Constants.EXAMPLE_URLS[website]);
                newWebsiteUrl.Margin = new Thickness(0, 5, 0, 2);
                WebsitesWithExampleUrls.Add(newWebsiteUrl);
            }

#if DEBUG
            // For easier debug
            //UrlInput = "https://www.scan-vf.net/jujutsu-kaisen/chapitre-18/1";
            //UrlInput = "https://anime-sama.fr/catalogue/20th-century-boys/scan-21st-century-boys/vf/";
            UrlInput = "https://lelscans.net/scan-one-piece/1117";
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

            if (UrlErrorAlertIsShown())
            {
                ClearAlertOnUrlTextBox();
            }
        }

        private async void btnNext_Click(object sender, RoutedEventArgs e)
        {
            if(string.IsNullOrEmpty(UrlInput))
            {
                ShowUrlErrorAlert("You didn't provide any url, the field is empty");
                return;

            }

            UrlValidityResult urlTestResult = await ScanManagement.IsValidScanUrl(UrlInput);
            if (urlTestResult.Success)
            {
                TempScanData = ScanManagement.CreateTemporaryScanData(UrlInput);
                if (TempScanData == null)
                {
                    ShowUrlErrorAlert("Impossible to create a ScanData object from url");
                    return;
                }

                // Reset Error
                HideUrlErrorAlert();
                RaiseEvent(new RoutedEventArgs(UrlConfirmedEvent, this));
            }
            else
            {
                ShowUrlErrorAlert(urlTestResult.InvalidityReason);
                return;
            }
        }
        #endregion

        #region Error Alert
        private void ShowUrlErrorAlert(string errorMsg)
        {
            errorAlertUrl.Visibility = Visibility.Visible;
            ErrorMessage = errorMsg;
            txtBoxUrlInput.BorderBrush = (SolidColorBrush)FindResource(ResourcesKey.ColorBrushes.Red);
            txtBoxUrlInput.BorderThickness = new Thickness(2);
        } 

        private void HideUrlErrorAlert()
        {
            errorAlertUrl.Visibility = Visibility.Collapsed;
            ClearAlertOnUrlTextBox();
        }

        private void ClearAlertOnUrlTextBox()
        {
            txtBoxUrlInput.ClearValue(Control.BorderBrushProperty);
            txtBoxUrlInput.ClearValue(Control.BorderThicknessProperty);
        }

        private bool UrlErrorAlertIsShown()
        {
            return txtBoxUrlInput.BorderThickness == new Thickness(2);
        }
        #endregion
    }
}
