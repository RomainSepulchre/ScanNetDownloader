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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ScanNetDownloader.View.CustomControls
{
    /// <summary>
    /// Logique d'interaction pour DuplicateItem.xaml
    /// </summary>
    public partial class DuplicateItem : UserControl
    {
        public DuplicateItem()
        {
            InitializeComponent();
        }

        private void imgSeeCurrent_MouseEnter(object sender, MouseEventArgs e)
        {
            stPanelCurrentData.Visibility = Visibility.Visible;
        }

        private void imgSeeCurrent_MouseLeave(object sender, MouseEventArgs e)
        {
            stPanelCurrentData.Visibility = Visibility.Collapsed;
        }
    }
}
