using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Controls;
using System.Windows.Media;

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

        private SolidColorBrush _circleColor;
        public SolidColorBrush CircleColor
        {
            get { return _circleColor; }
            set {
                _circleColor = value;
                OnPropertyChanged();
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        public DownloadEventItem(string msg, SolidColorBrush backgroundColor)
        {
            MsgText = msg;
            CircleColor = backgroundColor;

            InitializeComponent();

            DataContext = this;
        }

        private void OnPropertyChanged([CallerMemberName] string property = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(property));
        }
    }
}
