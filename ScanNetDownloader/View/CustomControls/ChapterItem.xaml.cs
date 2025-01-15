using ScanNetDownloader.Logic;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;

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
