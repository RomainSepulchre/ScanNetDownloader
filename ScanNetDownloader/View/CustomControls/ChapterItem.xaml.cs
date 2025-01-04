using ScanNetDownloader.Logic;
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
    /// Logique d'interaction pour ChapterItem.xaml
    /// </summary>
    public partial class ChapterItem : UserControl, INotifyPropertyChanged
    {
        public int SortingId { get; set; }

        public string ChapterKey { get; private set; }

        private string _chapterText;     

        public string ChapterText
        {
            get { return _chapterText; }
            set {
                _chapterText = value;
                OnPropertyChanged();
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        public static RoutedEvent DeleteBtnPressedEvent = EventManager.RegisterRoutedEvent(nameof(DeleteChapterBtnPressed), RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(ChapterItem));

        public event RoutedEventHandler DeleteChapterBtnPressed
        {
            add { AddHandler(DeleteBtnPressedEvent, value); }
            remove { RemoveHandler(DeleteBtnPressedEvent, value); }
        }

        public ChapterItem(string chapterKey)
        {
            DataContext = this;
            InitializeComponent();

            ChapterKey = chapterKey;
            ChapterText = chapterKey;

            SortingId = int.Parse(chapterKey.Split(Constants.DASH_CHAR)[0]);
            Debug.WriteLine($"SORTING ID ={SortingId} ({ChapterKey})");
        }

        private void OnPropertyChanged([CallerMemberName] string property = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(property));
        }

        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            RaiseEvent(new RoutedEventArgs(DeleteBtnPressedEvent, this));
        }
    }
}
