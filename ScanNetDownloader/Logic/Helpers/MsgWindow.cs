using ScanNetDownloader.View;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;

namespace ScanNetDownloader.Logic.Helpers
{
    public class MsgWindow
    {
        public enum ImageType 
        {
            NoImage,
            Error,
            Warning,
            Question,
            Information
        }

        public static OkWindow ShowOkWindow(string header, string message, bool allowQuit = true, ImageType windowImg = ImageType.NoImage)
        {
            Window parentWindow = Application.Current.MainWindow;
            BitmapImage image = GetBitmapImage(windowImg);

            OkWindow okWindow = new OkWindow(parentWindow, header, message, allowQuit, image);
            parentWindow.Opacity = 0.4;
            okWindow.ShowDialog();
            parentWindow.Opacity = 1;

            return okWindow;
        }

        public static OkWindow ShowOkWindow(Window parentWindow, string header, string message, bool allowQuit = true, ImageType windowImg = ImageType.NoImage)
        {
            BitmapImage image = GetBitmapImage(windowImg);

            OkWindow okWindow = new OkWindow(parentWindow, header, message, allowQuit, image);
            parentWindow.Opacity = 0.4;
            okWindow.ShowDialog();
            parentWindow.Opacity = 1;

            return okWindow;
        }

        public static YesNoWindow ShowYesNoWindow(string header, string message, bool allowQuit = true, ImageType windowImg = ImageType.NoImage)
        {
            Window parentWindow = Application.Current.MainWindow;
            BitmapImage image = GetBitmapImage(windowImg);

            YesNoWindow yesNoWindow = new YesNoWindow(parentWindow, header, message, allowQuit, image);
            parentWindow.Opacity = 0.4;
            yesNoWindow.ShowDialog();
            parentWindow.Opacity = 1;

            return yesNoWindow;
        }

        public static YesNoWindow ShowYesNoWindow(Window parentWindow, string header, string message, bool allowQuit = true, ImageType windowImg = ImageType.NoImage)
        {
            BitmapImage image = GetBitmapImage(windowImg);

            YesNoWindow yesNoWindow = new YesNoWindow(parentWindow, header, message, allowQuit, image);
            parentWindow.Opacity = 0.4;
            yesNoWindow.ShowDialog();
            parentWindow.Opacity = 1;

            return yesNoWindow;
        }

        public static InputWindow ShowInputWindow(string header, string message, string inputPlaceholder, bool allowQuit = true, ImageType windowImg = ImageType.NoImage)
        {
            Window parentWindow = Application.Current.MainWindow;
            BitmapImage image = GetBitmapImage(windowImg);

            InputWindow inputWindow = new InputWindow(parentWindow, header, message, inputPlaceholder, allowQuit, image);
            parentWindow.Opacity = 0.4;
            inputWindow.ShowDialog();
            parentWindow.Opacity = 1;

            return inputWindow;
        }

        public static InputWindow ShowInputWindow(Window parentWindow, string header, string message, string inputPlaceholder, bool allowQuit = true, ImageType windowImg = ImageType.NoImage)
        {
            BitmapImage image = GetBitmapImage(windowImg);

            InputWindow inputWindow = new InputWindow(parentWindow, header, message, inputPlaceholder, allowQuit, image);
            parentWindow.Opacity = 0.4;
            inputWindow.ShowDialog();
            parentWindow.Opacity = 1;

            return inputWindow;
        }

        private static BitmapImage GetBitmapImage(ImageType windowImg)
        {
            BitmapImage bitmapImage;

            switch (windowImg)
            {
                default:
                case ImageType.NoImage:
                    bitmapImage = null;
                    break;

                case ImageType.Error:
                    bitmapImage = (BitmapImage)Application.Current.Resources[ResourcesKey.BitmapImages.Error];
                    break;

                case ImageType.Warning:
                    bitmapImage = (BitmapImage)Application.Current.Resources[ResourcesKey.BitmapImages.WarningIcon];
                    break;

                case ImageType.Question:
                    bitmapImage = (BitmapImage)Application.Current.Resources[ResourcesKey.BitmapImages.Question_Black];
                    break;

                case ImageType.Information:
                    bitmapImage = (BitmapImage)Application.Current.Resources[ResourcesKey.BitmapImages.InfoIcon];
                    break;

            }

            return bitmapImage;
        }

    }
}
