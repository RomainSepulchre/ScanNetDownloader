using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
using static System.Net.WebRequestMethods;

namespace ScanNetDownloader.View.CustomControls
{
    /// <summary>
    /// Logique d'interaction pour WebsiteUrlWithExample.xaml
    /// </summary>
    public partial class WebsiteUrlWithExample : UserControl, INotifyPropertyChanged
    {
        private string _websiteUrl;
        public string WebsiteUrl
        {
            get { return _websiteUrl; }
            set {
                _websiteUrl = value;
                OnPropertyChanged();
            }
        }

        private List<string> _exampleUrls = new List<string>();
        public List<string> ExampleUrls
        {
            get { return _exampleUrls; }
            set { _exampleUrls = value; }
        }

        private ObservableCollection<TextBlock> _exampleUrlTxtBlocks = new ObservableCollection<TextBlock>();
        public ObservableCollection<TextBlock> ExampleUrlTxtBlocks
        {
            get { return _exampleUrlTxtBlocks; }
            set { _exampleUrlTxtBlocks = value; }
        }

        private string _linkText;
        public string LinkText
        {
            get { return _linkText; }
            set {
                _linkText = value;
                OnPropertyChanged();
            }
        }

        private const string SHOW_EXAMPLES = "Show examples";
        private const string HIDE_EXAMPLES = "Hide examples";

        public event PropertyChangedEventHandler? PropertyChanged;

        private void OnPropertyChanged([CallerMemberName] string property = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(property));
        }

        public WebsiteUrlWithExample()
        {
            DataContext = this;
            InitializeComponent();

            // Example url for debug in designer
            WebsiteUrl = "https://www.example.net";
            ExampleUrls.Add("https://www.example.net/Scan/BookName/Chapter-1/1");
            ExampleUrls.Add("https://www.example.net/Scan/BookName/Chapter-1/2");

            RefreshExampleUrls();

            LinkText = SHOW_EXAMPLES;
            stPanelExamples.Visibility = Visibility.Collapsed;
        }

        public WebsiteUrlWithExample(string websiteUrl, List<string> exampleUrls)
        {
            DataContext = this;
            InitializeComponent();

            WebsiteUrl = websiteUrl;
            ExampleUrls = exampleUrls;

            RefreshExampleUrls();

            LinkText = SHOW_EXAMPLES;
            stPanelExamples.Visibility = Visibility.Collapsed;
        }

        private void Hyperlink_Click(object sender, RoutedEventArgs e)
        {
            // Show or hide
            if(stPanelExamples.Visibility == Visibility.Visible)
            {
                LinkText = SHOW_EXAMPLES;
                stPanelExamples.Visibility = Visibility.Collapsed;
            }
            else
            {
                LinkText = HIDE_EXAMPLES;
                stPanelExamples.Visibility = Visibility.Visible;
            }
        }

        public void RefreshExampleUrls()
        {
            ExampleUrlTxtBlocks.Clear();
            foreach (string example in ExampleUrls)
            {
                TextBlock txtBlock = new TextBlock();
                txtBlock.Text = $"- {example}";
                ExampleUrlTxtBlocks.Add(txtBlock);
            }
        }
    }
}
