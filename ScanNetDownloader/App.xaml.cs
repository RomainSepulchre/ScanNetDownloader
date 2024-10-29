using System.Configuration;
using System.Data;
using System.Windows;

namespace ScanNetDownloader
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public static void Quit()
        {
            // TODO: Weird things happening with Application.Current.Shutdown && Window.Close, the app continue to run anyway even with window closed -> Retest with App.xaml.ShutdownMode="OnMainWindowClose"
            Environment.Exit(0); 
        }
    }

}
