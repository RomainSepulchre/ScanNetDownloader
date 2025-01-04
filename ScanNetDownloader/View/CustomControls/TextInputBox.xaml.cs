using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
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
    /// Logique d'interaction pour TextInputBox.xaml
    /// </summary>
    public partial class TextInputBox : UserControl, INotifyPropertyChanged
    {
        private bool _showClearBtn;
        public bool ShowClearBtn
        {
            get { return _showClearBtn; }
            set {
                _showClearBtn = value;
                ShowClearButton(_showClearBtn);
            }
        }

        private TextAlignment _txtAlignment = TextAlignment.Left;

        public TextAlignment TxtAlignment
        {
            get { return _txtAlignment; }
            set {
                _txtAlignment = value;
                SetTextHorizontalAlignment(_txtAlignment);
            }
        }

        private void SetTextHorizontalAlignment(TextAlignment alignment)
        {
            txtInput.TextAlignment = alignment;
            txtPlaceholder.TextAlignment = alignment;
        }

        private string _placeholderTxt;
        public string PlaceholderText
        {
            get { return _placeholderTxt; }
            set {
                _placeholderTxt = value;
                OnPropertyChanged();
            }
        }

        private Thickness _txtPadding;
        public Thickness TxtPadding
        {
            get { return _txtPadding; }
            set {
                _txtPadding = value;
                SetTxtPadding(_txtPadding);
            }
        }

        public static RoutedEvent TextInputChangedEvent = EventManager.RegisterRoutedEvent(nameof(TextInputChanged), RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(TextInputBox));
        public event RoutedEventHandler TextInputChanged
        {
            add { AddHandler(TextInputChangedEvent, value); }
            remove { RemoveHandler(TextInputChangedEvent, value); }
        }



        public string TxtInput
        {
            get { return (string)GetValue(TxtInputProperty); }
            set {
                SetValue(TxtInputProperty, value);
                Debug.WriteLine($"TXT INPUT CHANGED");
            }
        }
        // Using a DependencyProperty as the backing store for TxtInput.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty TxtInputProperty = DependencyProperty.Register("TxtInput", typeof(string), typeof(TextInputBox), new PropertyMetadata(""));



        public event PropertyChangedEventHandler? PropertyChanged;

        private void OnPropertyChanged([CallerMemberName] string property = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(property));
        }

        public TextInputBox()
        {
            //DataContext = this;
            InitializeComponent();
        }

        private void SetTxtPadding(Thickness newPadding)
        {
            txtInput.Padding = newPadding;

            Thickness txtPlaceholderMargin = newPadding;
            switch (txtPlaceholder.TextAlignment)
            {
                default:
                case TextAlignment.Left:
                case TextAlignment.Justify:
                    txtPlaceholderMargin.Left += 3;
                    break;
                case TextAlignment.Right:
                    txtPlaceholderMargin.Right += 3;
                    break;
                case TextAlignment.Center:
                    break;
            }
            
            txtPlaceholder.Margin = txtPlaceholderMargin;       
        }

        private void ShowClearButton(bool show)
        {
            if (show) btnClear.Visibility = Visibility.Visible;
            else btnClear.Visibility = Visibility.Collapsed;
        }

        private void btnClear_Click(object sender, RoutedEventArgs e)
        {
            txtInput.Clear();
            txtInput.Focus();
        }

        private void txtInput_TextChanged(object sender, TextChangedEventArgs e)
        {
            Debug.WriteLine("TEXT CHANGED");
            RaiseEvent(new RoutedEventArgs(TextInputChangedEvent, this));

            if (string.IsNullOrEmpty(txtInput.Text))
            {
                txtPlaceholder.Visibility = Visibility.Visible; 
            }
            else
            {
                txtPlaceholder.Visibility = Visibility.Hidden;
            }
        }
    }
}
