using ScanNetDownloader.Logic;
using ScanNetDownloader.View.CustomControls;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
    /// Logique d'interaction pour DuplicateWindow.xaml
    /// </summary>
    public partial class DuplicateWindow : Window
    {
        //private ObservableCollection<DuplicateItem> _duplicateItems;
        //public ObservableCollection<DuplicateItem> DuplicateItems
        //{
        //    get { return _duplicateItems; }
        //    set
        //    {
        //        _duplicateItems = value;
        //    }
        //}

        public DuplicateWindow()
        {
            InitializeComponent();
        }

        public DuplicateWindow(Window parentWindow)
        {
            Owner = parentWindow;
            InitializeComponent();
        }

        public DuplicateWindow(Window parentWindow, List<ScanData> duplicateFound)
        {
            Owner = parentWindow;
            InitializeComponent();
        }


        private void btnFinish_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
