using System.Windows;
using System.Windows.Controls;

namespace ScanNetDownloader.View
{
    /// <summary>
    /// Logique d'interaction pour InputPopUp.xaml
    /// </summary>
    public partial class InputPopUp : Window
    {
        public string Input { get; set; }

        public InputPopUp(Window parentWindow, string info)
        {
            Owner = parentWindow;
            
            InitializeComponent();
            lbPopUpInfo.Content = info;
        }

        private void btnOk_Click(object sender, RoutedEventArgs e)
        {
            Input = txtBoxInput.Text;
            Close();
        }

        private void txtBoxInput_TextChanged(object sender, TextChangedEventArgs e)
        {
            if(string.IsNullOrEmpty(txtBoxInput.Text))
            {
                btnOk.IsEnabled = false;
            }
            else
            {
                btnOk.IsEnabled = true;
            }
        }
    }
}
