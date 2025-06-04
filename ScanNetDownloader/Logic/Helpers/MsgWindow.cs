using ScanNetDownloader.View;
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
            Information,
            Success
        }

        public static OkWindow ShowOkWindow(string header, string message, bool allowQuit = true, ImageType windowImg = ImageType.NoImage, bool parseMessageAsInlines = false)
        {
            Window parentWindow = Application.Current.MainWindow;
            BitmapImage image = GetBitmapImage(windowImg);

            OkWindow okWindow = new OkWindow(parentWindow, header, message, allowQuit, image, parseMessageAsInlines);
            parentWindow.Opacity = 0.4;
            okWindow.ShowDialog();
            parentWindow.Opacity = 1;

            return okWindow;
        }

        public static OkWindow ShowOkWindow(Window parentWindow, string header, string message, bool allowQuit = true, ImageType windowImg = ImageType.NoImage, bool parseMessageAsInlines = false)
        {
            BitmapImage image = GetBitmapImage(windowImg);

            OkWindow okWindow = new OkWindow(parentWindow, header, message, allowQuit, image, parseMessageAsInlines);
            parentWindow.Opacity = 0.4;
            okWindow.ShowDialog();
            parentWindow.Opacity = 1;

            return okWindow;
        }

        public static OkWindow ShowOkWindow(bool noParent, string header, string message, bool allowQuit = true, ImageType windowImg = ImageType.NoImage, bool parseMessageAsInlines = false)
        {
            if (noParent)
            {
                BitmapImage image = GetBitmapImage(windowImg);

                OkWindow okWindow = new OkWindow( header, message, allowQuit, image, parseMessageAsInlines);
                okWindow.ShowDialog();

                return okWindow;
            }
            else
            {
                return ShowOkWindow(header, message, allowQuit, windowImg, parseMessageAsInlines);
            }
        }

        public static YesNoWindow ShowYesNoWindow(string header, string message, bool allowQuit = true, ImageType windowImg = ImageType.NoImage, bool parseMessageAsInlines = false)
        {
            Window parentWindow = Application.Current.MainWindow;
            BitmapImage image = GetBitmapImage(windowImg);

            YesNoWindow yesNoWindow = new YesNoWindow(parentWindow, header, message, allowQuit, image, parseMessageAsInlines);
            parentWindow.Opacity = 0.4;
            yesNoWindow.ShowDialog();
            parentWindow.Opacity = 1;

            return yesNoWindow;
        }

        public static YesNoWindow ShowYesNoWindow(Window parentWindow, string header, string message, bool allowQuit = true, ImageType windowImg = ImageType.NoImage, bool parseMessageAsInlines = false)
        {
            BitmapImage image = GetBitmapImage(windowImg);

            YesNoWindow yesNoWindow = new YesNoWindow(parentWindow, header, message, allowQuit, image, parseMessageAsInlines);
            parentWindow.Opacity = 0.4;
            yesNoWindow.ShowDialog();
            parentWindow.Opacity = 1;

            return yesNoWindow;
        }

        public static YesNoWindow ShowYesNoWindow(bool noParent, string header, string message, bool allowQuit = true, ImageType windowImg = ImageType.NoImage, bool parseMessageAsInlines = false)
        {
            if (noParent)
            {
                BitmapImage image = GetBitmapImage(windowImg);

                YesNoWindow yesNoWindow = new YesNoWindow(header, message, allowQuit, image, parseMessageAsInlines);
                yesNoWindow.ShowDialog();

                return yesNoWindow;
            }
            else
            {
                return ShowYesNoWindow(header, message, allowQuit, windowImg, parseMessageAsInlines);
            }
        }

        public static InputWindow ShowInputWindow(string header, string message, string inputPlaceholder, bool allowQuit = true, ImageType windowImg = ImageType.NoImage, bool parseMessageAsInlines = false)
        {
            Window parentWindow = Application.Current.MainWindow;
            BitmapImage image = GetBitmapImage(windowImg);

            InputWindow inputWindow = new InputWindow(parentWindow, header, message, inputPlaceholder, allowQuit, image, parseMessageAsInlines);
            parentWindow.Opacity = 0.4;
            inputWindow.ShowDialog();
            parentWindow.Opacity = 1;

            return inputWindow;
        }

        public static InputWindow ShowInputWindow(Window parentWindow, string header, string message, string inputPlaceholder, bool allowQuit = true, ImageType windowImg = ImageType.NoImage, bool parseMessageAsInlines = false)
        {
            BitmapImage image = GetBitmapImage(windowImg);

            InputWindow inputWindow = new InputWindow(parentWindow, header, message, inputPlaceholder, allowQuit, image, parseMessageAsInlines);
            parentWindow.Opacity = 0.4;
            inputWindow.ShowDialog();
            parentWindow.Opacity = 1;

            return inputWindow;
        }

        public static InputWindow ShowInputWindow(bool noParent, string header, string message, string inputPlaceholder, bool allowQuit = true, ImageType windowImg = ImageType.NoImage, bool parseMessageAsInlines = false)
        {
            if (noParent)
            {
                BitmapImage image = GetBitmapImage(windowImg);

                InputWindow inputWindow = new InputWindow(header, message, inputPlaceholder, allowQuit, image, parseMessageAsInlines);
                inputWindow.ShowDialog();

                return inputWindow;
            }
            else
            {
                return ShowInputWindow(header, message, inputPlaceholder, allowQuit, windowImg, parseMessageAsInlines);
            }
        }

        public static DuplicateWindow ShowDuplicateWindow()
        {
            Window parentWindow = Application.Current.MainWindow;

            DuplicateWindow duplicateWindow = new DuplicateWindow(parentWindow);

            parentWindow.Opacity = 0.4;
            duplicateWindow.ShowDialog();
            parentWindow.Opacity = 1;

            return duplicateWindow;
        }

        public static DuplicateWindow ShowDuplicateWindow(Window parentWindow)
        {
            DuplicateWindow duplicateWindow = new DuplicateWindow(parentWindow);

            parentWindow.Opacity = 0.4;
            duplicateWindow.ShowDialog();
            parentWindow.Opacity = 1;

            return duplicateWindow;
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

                case ImageType.Success:
                    bitmapImage = (BitmapImage)Application.Current.Resources[ResourcesKey.BitmapImages.SuccessIcon];
                    break;

            }

            return bitmapImage;
        }

    }
}
