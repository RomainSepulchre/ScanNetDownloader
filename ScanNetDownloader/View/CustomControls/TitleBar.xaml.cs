using ScanNetDownloader.Logic.Helpers;
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
    /// Logique d'interaction pour TitleBar.xaml
    /// </summary>
    public partial class TitleBar : UserControl, INotifyPropertyChanged
    {

        private string _title;
        public string Title
        {
            get { return _title; }
            set {
                _title = value;
                OnPropertyChanged();
            }
        }

        private ResizeMode resizeMode { get; set; }


        public event PropertyChangedEventHandler? PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string property = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(property));
        }

        public TitleBar()
        {
            InitializeComponent();
        }

        public void InitializeTitleBar(bool showTitleBar=true)
        {
            if(showTitleBar)
            {
                Window parentWindow = Window.GetWindow(this);
                Title = parentWindow.Title;

                resizeMode = parentWindow.ResizeMode;

                switch (resizeMode)
                {
                    case ResizeMode.NoResize:
                        panelMinimize.Visibility = Visibility.Collapsed;
                        panelMaximize.Visibility = Visibility.Collapsed;
                        break;
                    case ResizeMode.CanMinimize:
                        panelMinimize.Visibility = Visibility.Visible;
                        panelMaximize.Visibility = Visibility.Collapsed;
                        break;
                    default:
                    case ResizeMode.CanResize:
                    case ResizeMode.CanResizeWithGrip:
                        panelMinimize.Visibility = Visibility.Visible;
                        panelMaximize.Visibility = Visibility.Visible;
                        break;
                }

                switch (parentWindow.WindowState)
                {
                    case WindowState.Normal:
                        imgMaximizeBtn.Source = (BitmapImage)FindResource(ResourcesKey.BitmapImages.Maximize);
                        break;
                    case WindowState.Maximized:
                        imgMaximizeBtn.Source = (BitmapImage)FindResource(ResourcesKey.BitmapImages.InferiorLevel);
                        break;
                    case WindowState.Minimized:
                    default:
                        break;
                }
            }
            else
            {
                Visibility = Visibility.Collapsed;
            } 
        }

        private void gridBar_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
                if (e.ClickCount == 2)
                {
                    if(resizeMode == ResizeMode.CanResize || resizeMode == ResizeMode.CanResizeWithGrip)
                    {
                        AdjustWindowSize();
                    }
                }
                else
                {
                    Window.GetWindow(this).DragMove();
                }
        }

        /// <summary>
        /// Adjusts the WindowSize to correct parameters when Maximize button is clicked
        /// </summary>
        private void AdjustWindowSize()
        {
            Window parentWindow = Window.GetWindow(this);
            if (parentWindow.WindowState == WindowState.Maximized)
            {
                parentWindow.WindowState = WindowState.Normal;
                imgMaximizeBtn.Source = (BitmapImage)FindResource(ResourcesKey.BitmapImages.Maximize);
            }
            else
            {
                parentWindow.WindowState = WindowState.Maximized;
                imgMaximizeBtn.Source = (BitmapImage)FindResource(ResourcesKey.BitmapImages.InferiorLevel);
            }
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            Window parentWindow = Window.GetWindow(this);
            if(parentWindow is MainWindow)
            {
                App.Quit();
            }
            else
            {
                parentWindow.Close();
            }
        }

        private void btnMaximize_Click(object sender, RoutedEventArgs e)
        {
            AdjustWindowSize();
        }

        private void btnMinimize_Click(object sender, RoutedEventArgs e)
        {
            Window parentWindow = Window.GetWindow(this);
            parentWindow.WindowState = WindowState.Minimized;
        }
    } 
}
