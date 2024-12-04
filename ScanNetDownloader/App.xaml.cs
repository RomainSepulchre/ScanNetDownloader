using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Windows;

namespace ScanNetDownloader
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public static event EventHandler OnApplicationExitEvent;

        public static void Quit()
        {
            // TODO: Weird things happening with Application.Current.Shutdown && Window.Close, the app continue to run anyway even with window closed -> Retest with App.xaml.ShutdownMode="OnMainWindowClose"
            Environment.Exit(0); 
        }

        private void Application_Exit(object sender, ExitEventArgs e)
        {
            OnApplicationExit(e);
            Debug.WriteLine($"APPLICATION EXIT");
        }

        private void OnApplicationExit(EventArgs e)
        {
            if (OnApplicationExitEvent != null)
            {
                OnApplicationExitEvent(this, e);
            }
        }
    }

}
