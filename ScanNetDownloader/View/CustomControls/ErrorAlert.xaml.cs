using System.Windows;
using System.Windows.Controls;

namespace ScanNetDownloader.View.CustomControls
{
    /// <summary>
    /// Logique d'interaction pour ErrorAlert.xaml
    /// </summary>
    public partial class ErrorAlert : UserControl
    {

        public string ErrorMsg
        {
            get { return (string)GetValue(ErrorMsgProperty); }
            set { SetValue(ErrorMsgProperty, value); }
        }
        public static readonly DependencyProperty ErrorMsgProperty = DependencyProperty.Register(nameof(ErrorMsg), typeof(string), typeof(ErrorAlert), new PropertyMetadata("Error message"));


        public ErrorAlert()
        {
            InitializeComponent();
        }
    }
}
