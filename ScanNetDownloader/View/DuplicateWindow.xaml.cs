using ScanNetDownloader.Logic;
using ScanNetDownloader.View.CustomControls;
using System.Collections.ObjectModel;
using System.Windows;

namespace ScanNetDownloader.View
{
    /// <summary>
    /// Logique d'interaction pour DuplicateWindow.xaml
    /// </summary>
    public partial class DuplicateWindow : Window
    {
        private ObservableCollection<DuplicateItem> _duplicateItems;
        public ObservableCollection<DuplicateItem> DuplicateItems
        {
            get { return _duplicateItems; }
            set
            {
                _duplicateItems = value;
            }
        }

        public DuplicateWindow()
        {
            InitializeComponent();

            DataContext = this;
        }

        public DuplicateWindow(Window parentWindow, List<DuplicatedScanData> duplicateFound)
        {
            Owner = parentWindow;
            DuplicateItems = new ObservableCollection<DuplicateItem>();

            InitializeComponent();

            foreach (DuplicatedScanData duplicate in duplicateFound)
            {
                DuplicateItem item = new DuplicateItem(duplicate);
                DuplicateItems.Add(item);
            }

            DataContext = this;
        }


        private void btnFinish_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void btnDeleteAll_Click(object sender, RoutedEventArgs e)
        {
            foreach (DuplicateItem item in DuplicateItems)
            {
                item.SetComboBoxSelection(DuplicateItem.DuplicateOptions.Delete);
            }
        }

        private void btnKeepAll_Click(object sender, RoutedEventArgs e)
        {
            foreach (DuplicateItem item in DuplicateItems)
            {
                item.SetComboBoxSelection(DuplicateItem.DuplicateOptions.Keep);
            }
        }

        private void btnReplaceAll_Click(object sender, RoutedEventArgs e)
        {
            foreach (DuplicateItem item in DuplicateItems)
            {
                item.SetComboBoxSelection(DuplicateItem.DuplicateOptions.Replace);
            }
        }
    }
}
