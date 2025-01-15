using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Media.Imaging;

namespace ScanNetDownloader.View
{
    /// <summary>
    /// Logique d'interaction pour InputPopUp.xaml
    /// </summary>
    public partial class InputWindow : Window, INotifyPropertyChanged
    {
        public bool Success { get; set; } = false;

        private string _header;
        public string Header
        {
            get { return _header; }
            set
            {
                _header = value;
                OnPropertyChanged();
            }
        }

        private string _message;
        public string Message
        {
            get { return _message; }
            set
            {
                _message = value;
                OnPropertyChanged();
            }
        }

        private BitmapImage _image;
        public BitmapImage Image
        {
            get { return _image; }
            set
            {
                _image = value;
                OnPropertyChanged();
            }
        }

        private bool _showImage;
        public bool ShowImage
        {
            get { return _showImage; }
            set
            {
                _showImage = value;
                OnPropertyChanged();
            }
        }

        private bool _showQuitButton;
        public bool ShowQuitButton
        {
            get { return _showQuitButton; }
            set
            {
                _showQuitButton = value;
                OnPropertyChanged();
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string property = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(property));
        }

        public string Input { get; set; }

        public InputWindow()
        {
            DataContext = this;
            InitializeComponent();
        }

        public InputWindow(Window parentWindow, string header, string msg, string inputPlaceholder="", bool allowQuit=true, BitmapImage? img=null)
        {
            Owner = parentWindow;
            
            DataContext = this;
            InitializeComponent();

            Header = header;
            Message = msg;
            ShowQuitButton = allowQuit;

            if (img != null)
            {
                Image = img;
                ShowImage = true;
            }
            else
            {
                ShowImage = false;
            }

            txtBoxInput.PlaceholderText = inputPlaceholder;
        }

        private void btnOk_Click(object sender, RoutedEventArgs e)
        {
            Input = txtBoxInput.TxtInput;
            Success = true;
            Close();
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void TextInputBox_TextInputChanged(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(txtBoxInput.TxtInput))
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
