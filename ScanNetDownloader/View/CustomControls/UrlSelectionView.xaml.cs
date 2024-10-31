using ScanNetDownloader.Logic;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
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
    public partial class UrlSelectionView : UserControl
    {
        public ScanData TempScanData;

        public string UrlInput { get; set; }

        public static RoutedEvent UrlConfirmedEvent = EventManager.RegisterRoutedEvent(nameof(UrlConfirmed), RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(ScanItem));

        public event RoutedEventHandler UrlConfirmed
        {
            add { AddHandler(UrlConfirmedEvent, value); }
            remove { RemoveHandler(UrlConfirmedEvent, value); }
        }

        public UrlSelectionView()
        {
            InitializeComponent();

            // Setup info
            txtBlockInfos.Text = "Enter a scan URL...";
            txtBlockInfos.Text += "\nCompatibles websites are:";
            foreach(string website in Constants.COMPATIBLE_SCAN_WEBSITES)
            {
                txtBlockInfos.Text += $"\n-{website}";
            }
        }

        private void txtBoxUrlInput_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (string.IsNullOrEmpty(txtBoxUrlInput.Text) == false)
            {
                btnNext.IsEnabled = true;
            }
        }

        private void btnNext_Click(object sender, RoutedEventArgs e)
        {
            string userInput = txtBoxUrlInput.Text;

            UrlValidityResult urlTestResult = ScanManagement.IsValidScanUrl(userInput);
            if (urlTestResult.IsValid) // TODO: Find a way to know if the url is valid for each website
            {
                UrlInput = txtBoxUrlInput.Text; // TODO: binding for this

                TempScanData = ScanManagement.CreateTemporaryScanData(UrlInput);
                if(TempScanData == null)
                {
                    lbUrlError.Content = "Impossible to create a ScanData object from url";
                    txtBoxUrlInput.Background = Brushes.IndianRed;
                    return;
                }

                // Reset Error
                lbUrlError.Content = ""; // TODO: binding for this 
                txtBoxUrlInput.ClearValue(Control.BackgroundProperty);

                RaiseEvent(new RoutedEventArgs(UrlConfirmedEvent, this));
            }
            else
            {
                //TODO: Show error, Add text explanation
                Debug.WriteLine(urlTestResult.InvalidityReason);
                lbUrlError.Content = urlTestResult.InvalidityReason;
                txtBoxUrlInput.Background = Brushes.IndianRed;
            }
        }
    }
}
