using System;
using System.Collections.Generic;
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
using System.Windows.Shapes;

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
            popUpInfo.Content = info;
        }

        private void btnOk_Click(object sender, RoutedEventArgs e)
        {
            Input = txtInput.Text;
            Close();
        }

        private void txtInput_TextChanged(object sender, TextChangedEventArgs e)
        {
            if(string.IsNullOrEmpty(txtInput.Text))
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
