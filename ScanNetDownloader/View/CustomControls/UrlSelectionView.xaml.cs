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
            if (ScanManagement.IsValidScanUrl(userInput, out string invalidityReason)) // TODO: Find a way to know if the url is valid for each website
            {
                UrlInput = txtBoxUrlInput.Text;

                RaiseEvent(new RoutedEventArgs(UrlConfirmedEvent, this));
            }
            else
            {
                //TODO: Show error, Add text explanation
                Debug.WriteLine(invalidityReason);
                txtBoxUrlInput.Background = Brushes.IndianRed;
            }
        }
    }
}
