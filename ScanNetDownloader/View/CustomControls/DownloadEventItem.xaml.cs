using System;
using System.Collections.Generic;
using System.ComponentModel;
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
    /// Logique d'interaction pour DownloadEventItem.xaml
    /// </summary>
    public partial class DownloadEventItem : UserControl, INotifyPropertyChanged
    {

        private string _msgText;
        public string MsgText
        {
            get { return _msgText; }
            set {
                _msgText = value;
                OnPropertyChanged();
            }
        }

        private SolidColorBrush _backgroundColor;
        public SolidColorBrush BackgroundColor
        {
            get { return _backgroundColor; }
            set {
                _backgroundColor = value;
                OnPropertyChanged();
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        public DownloadEventItem(string msg, SolidColorBrush backgroundColor)
        {
            MsgText = msg;
            BackgroundColor = backgroundColor;

            InitializeComponent();

            DataContext = this;
        }

        private void OnPropertyChanged([CallerMemberName] string property = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(property));
        }
    }
}
