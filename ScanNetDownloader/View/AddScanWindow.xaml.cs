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
    /// Logique d'interaction pour AddScanWindow.xaml
    /// </summary>
    public partial class AddScanWindow : Window
    {
        public string UrlInput { get; set; }

        public string ChapterInput { get; set; }

        public bool Success { get; set; } = false;

        public AddScanWindow(Window parentWindow)
        {
            Owner = parentWindow;
            InitializeComponent();
        }

        private void btnAdd_Click(object sender, RoutedEventArgs e)
        {
            Success = true;
            UrlInput = inputUrl.Text;
            ChapterInput = inputChapter.Text;
            Close();
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void inputUrl_TextChanged(object sender, TextChangedEventArgs e)
        {
            // TODO : THIS
            // Check if valid url

            // if valid check if chapter provided
            // -y: keep chapter input disabled, set chapterInput, add text to tell chapter is provided through url, enable ok
            // -n: enabled chapter input
            if (string.IsNullOrEmpty(inputUrl.Text) == false)
            {
                inputChapter.IsEnabled = true;
            }
        }

        private void inputChapter_TextChanged(object sender, TextChangedEventArgs e)
        {
            // TODO : THIS
            // if valid chapter, enable ok
            if (string.IsNullOrEmpty(inputChapter.Text) == false)
            {
                btnAdd.IsEnabled = true;
            }
        }

        
    }
}
