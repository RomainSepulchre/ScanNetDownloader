using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
using ScanNetDownloader.Logic;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace ScanNetDownloader.View.CustomControls
{
    /// <summary>
    /// Logique d'interaction pour ChaptersSelectionView.xaml
    /// </summary>
    public partial class ChaptersSelectionView : UserControl, INotifyPropertyChanged
    {
        private Dictionary<string, List<int>> ChaptersSelection { get; set; } = new Dictionary<string, List<int>>();

        public List<int> ChaptersSelected => GetSelectedChapters();

        private ObservableCollection<TextBlock> _chapterItems;
        public ObservableCollection<TextBlock> ChapterItems
        {
            get { return _chapterItems; }
            set { _chapterItems = value; }
        }



        private string _selectChapterInfos = "Choose some chapter...";
        public string SelectChapterInfos
        {
            get { return _selectChapterInfos; }
            set {
                _selectChapterInfos = value;
                OnPropertyChanged();
            }
        }

        private string _singleChapterInput;
        public string SingleChapterInput
        {
            get { return _singleChapterInput; }
            set {
                _singleChapterInput = value;
                OnPropertyChanged();
            }
        }

        private string _startChapterInput;
        public string StartChapterInput
        {
            get { return _startChapterInput; }
            set {
                _startChapterInput = value;
                OnPropertyChanged();
            }
        }

        private string _endChapterInput;
        public string EndChapterInput
        {
            get { return _endChapterInput; }
            set {
                _endChapterInput = value;
                OnPropertyChanged();
            }
        }

        private string _errorMessage;
        public string ErrorMessage
        {
            get { return _errorMessage; }
            set {
                _errorMessage = value;
                OnPropertyChanged();
            }
        }


        public event PropertyChangedEventHandler? PropertyChanged;

        // View Events
        public static RoutedEvent BackBtnPressedEvent = EventManager.RegisterRoutedEvent(nameof(BackBtnPressed), RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(ScanItem));

        public event RoutedEventHandler BackBtnPressed
        {
            add { AddHandler(BackBtnPressedEvent, value); }
            remove { RemoveHandler(BackBtnPressedEvent, value); }
        }

        public static RoutedEvent ChaptersConfirmedEvent = EventManager.RegisterRoutedEvent(nameof(ChaptersConfirmed), RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(ScanItem));

        

        public event RoutedEventHandler ChaptersConfirmed
        {
            add { AddHandler(ChaptersConfirmedEvent, value); }
            remove { RemoveHandler(ChaptersConfirmedEvent, value); }
        }

        public ChaptersSelectionView()
        {
            DataContext = this;
            ChapterItems = new ObservableCollection<TextBlock>();
            InitializeComponent();
        }


        private void OnPropertyChanged([CallerMemberName] string property = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(property));
        }

        #region View Initialization
        public void InitChapterSelection(ScanData tempScanData) // TODO: How to manage adding more chapters for Url with chapter number -> add url with chapter first remove chapter from list and then generate url for all the other added chapter
        {
            SetUrlInfo(tempScanData);

            if (tempScanData.UrlContainsChapter())
            {
                if (ChapterAlreadyAdded(tempScanData.ChapterId) == false)
                {
                    string chapterKey = tempScanData.ChapterId.ToString();
                    AddToChaptersSelection(chapterKey, new List<int>() { tempScanData.ChapterId });
                }
            }
        }

        private void SetUrlInfo(ScanData tempScanData)
        {
            if (tempScanData.UrlContainsChapter())
            {
                SelectChapterInfos = $"Chapter {tempScanData.ChapterId} is already specified in {tempScanData.Url} but you can add additionnal chapter";
            }
            else
            {
                SelectChapterInfos = $"Choose chapters for {tempScanData.Url}";
            }
        }
        #endregion

        #region Buttons and Ui events
        private void btnAddSingleChapter_Click(object sender, RoutedEventArgs e)
        {
            bool validNumber = int.TryParse(SingleChapterInput, out int chapterToAdd);
            if (validNumber)
            {
                if (ChapterAlreadyAdded(chapterToAdd))
                {
                    ErrorMessage = $"Chapter {chapterToAdd} is already added";
                    txtBoxSingleChapter.Background = Brushes.IndianRed;
                }
                else
                {
                    // Add chapter
                    string chapterKey = SingleChapterInput;
                    AddToChaptersSelection(chapterKey, new List<int>() { chapterToAdd });

                    // Clear txt box
                    SingleChapterInput = "";
                }
            }
            else // Invalid number entered
            {
                ErrorMessage = $"\"{SingleChapterInput}\" is not a valid number";
                txtBoxSingleChapter.Background = Brushes.IndianRed;
            }
        }

        private void btnAddRangeOfChapter_Click(object sender, RoutedEventArgs e)
        {
            Debug.WriteLine($"START:{StartChapterInput}, END:{EndChapterInput}");
            int startChapter, endChapter;
            bool validStartChapter = int.TryParse(StartChapterInput, out startChapter);
            bool validEndChapter = int.TryParse(EndChapterInput, out endChapter);

            if (validStartChapter && validEndChapter)
            {
                if (startChapter > endChapter) (startChapter, endChapter) = (endChapter, startChapter); // invert two value to make sure start is the lower value

                if (ChaptersAlreadyAdded(startChapter, endChapter, out List<int> nonDuplicatedChapters))
                {
                    if (nonDuplicatedChapters.Count == 0)
                    {
                        ErrorMessage = $"{startChapter}-{endChapter} all chapters in the range are already added";
                        txtBoxEndChapter.Background = Brushes.IndianRed;
                        txtBoxStartChapter.Background = Brushes.IndianRed;
                    }
                    else
                    {
                        nonDuplicatedChapters.Log();
                        string chaptersAdded = string.Join(',', nonDuplicatedChapters);

                        while (nonDuplicatedChapters.Count > 0)
                        {
                            if (nonDuplicatedChapters.Count == 1)
                            {
                                string chapterKey = nonDuplicatedChapters[0].ToString();
                                AddToChaptersSelection(chapterKey, new List<int>() { nonDuplicatedChapters[0] });
                                nonDuplicatedChapters.Remove(nonDuplicatedChapters[0]);
                            }
                            else if (nonDuplicatedChapters.IsAnIncreasingSuite(out int breakIndex) == false)
                            {
                                if (breakIndex > 0) // there is an incomplete suite at the beginning
                                {
                                    int rangeStart = nonDuplicatedChapters[0];
                                    int rangeEnd = nonDuplicatedChapters[breakIndex];

                                    List<int> selectedChapters = Enumerable.Range(rangeStart, (rangeEnd - rangeStart) + 1).ToList();

                                    string chapterKey = $"{rangeStart}{Constants.DASH_CHAR}{rangeEnd}";
                                    AddToChaptersSelection(chapterKey, selectedChapters);

                                    nonDuplicatedChapters.RemoveRange(0, breakIndex + 1);
                                }
                                else // add first item as single chapter
                                {
                                    string chapterKey = nonDuplicatedChapters.First().ToString();
                                    AddToChaptersSelection(chapterKey, new List<int>() { nonDuplicatedChapters.First() });
                                    nonDuplicatedChapters.Remove(nonDuplicatedChapters.First());
                                }
                            }
                            else // List is a suite of increasing int
                            {
                                int rangeStart = nonDuplicatedChapters.First();
                                int rangeEnd = nonDuplicatedChapters.Last();

                                List<int> selectedChapters = Enumerable.Range(rangeStart, (rangeEnd - rangeStart) + 1).ToList();

                                string chapterKey = $"{rangeStart}{Constants.DASH_CHAR}{rangeEnd}";
                                AddToChaptersSelection(chapterKey, selectedChapters);
                                nonDuplicatedChapters.Clear();
                            }
                        }

                        ErrorMessage = $"Some chapters were already added, only chapters {chaptersAdded} have been added";

                        // Clear txt box
                        StartChapterInput = "";
                        EndChapterInput = "";
                    }
                }
                else
                {
                    // Add chapter
                    string chapterKey = $"{startChapter}{Constants.DASH_CHAR}{endChapter}";
                    List<int> selectedChapters = Enumerable.Range(startChapter, (endChapter - startChapter) + 1).ToList();
                    AddToChaptersSelection(chapterKey, selectedChapters);

                    // Clear txt box
                    StartChapterInput = "";
                    EndChapterInput = "";
                }
            }
            else // Invalid number entered
            {
                string errorMessage = "";
                if (validStartChapter == false)
                {
                    errorMessage += $"\"{StartChapterInput}\"";
                    txtBoxStartChapter.Background = Brushes.IndianRed;
                }
                if (validEndChapter == false)
                {
                    errorMessage += string.IsNullOrEmpty(errorMessage) ? $"\"{EndChapterInput}\"" : $", \"{EndChapterInput}\"";
                    txtBoxEndChapter.Background = Brushes.IndianRed;
                }
                errorMessage += " is not a valid number";
                ErrorMessage = errorMessage;
            }
        }

        private void btnBackToUrlSelection_Click(object sender, RoutedEventArgs e)
        {
            RaiseEvent(new RoutedEventArgs(BackBtnPressedEvent, this));
        }

        private void btnConfirmChapters_Click(object sender, RoutedEventArgs e)
        {
            RaiseEvent(new RoutedEventArgs(ChaptersConfirmedEvent, this));
        }

        private void txtBoxSingleChapter_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (txtBoxSingleChapter.Background == Brushes.IndianRed)
                txtBoxSingleChapter.ClearValue(Control.BackgroundProperty);
        }

        private void txtBoxStartChapter_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (txtBoxStartChapter.Background == Brushes.IndianRed)
                txtBoxStartChapter.ClearValue(Control.BackgroundProperty);
        }

        private void txtBoxEndChapter_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (txtBoxEndChapter.Background == Brushes.IndianRed)
                txtBoxEndChapter.ClearValue(Control.BackgroundProperty);
        }
        #endregion

        #region Chapter Management
        private void AddToChaptersSelection(string chapterKey, List<int> chapterValues)
        {
            ChaptersSelection.Add(chapterKey, chapterValues);
            AddChapterItem(chapterKey);
        }

        private void AddChapterItem(string chapter)
        {
            //TODO: find a way to reorder the items to always have from an increasing order
            //TODO: Create a proper chapterItem with a delete button + manage deleting Item

            TextBlock chapterTxtBlock = new TextBlock();
            chapterTxtBlock.Text = chapter;
            chapterTxtBlock.Width = 50;
            chapterTxtBlock.Height = 20;
            chapterTxtBlock.Background = Brushes.Orange;
            chapterTxtBlock.VerticalAlignment = VerticalAlignment.Center;
            chapterTxtBlock.TextAlignment = TextAlignment.Center;
            chapterTxtBlock.Margin = new Thickness(5, 0, 0, 0);

            ChapterItems.Add(chapterTxtBlock);
            //chapterSelectedPanel.Children.Add(chapterTxtBlock);

            btnConfirmChapters.IsEnabled = true;
        }

        private List<int> GetSelectedChapters()
        {
            List<int> selectedChapters = new List<int>();

            foreach (List<int> chapterList in ChaptersSelection.Values.ToList())
            {
                selectedChapters.AddRange(chapterList);
            }
            selectedChapters.Sort();
            return selectedChapters;
        }

        private bool ChapterAlreadyAdded(int chapterId)
        {
            foreach (List<int> chapterAdded in ChaptersSelection.Values.ToList())
            {
                if (chapterAdded.Contains(chapterId)) return true;
            }
            return false;
        }

        private bool ChaptersAlreadyAdded(int rangeStart, int rangeEnd, out List<int> nonDuplicatedChapters)
        {
            nonDuplicatedChapters = new List<int>();
            for (int i = rangeStart; i <= rangeEnd; i++)
            {
                nonDuplicatedChapters.Add(i);
            }

            bool chapterAlreadyAdded = false;
            foreach (List<int> chapterAdded in ChaptersSelection.Values.ToList())
            {
                int firstItem = chapterAdded.First();
                int lastItem = chapterAdded.Last();

                bool duplicateInRange = (rangeStart >= firstItem && rangeStart <= lastItem) || (rangeEnd >= firstItem && rangeEnd <= lastItem) || (firstItem > rangeStart && lastItem < rangeEnd);
                if (duplicateInRange)
                {
                    chapterAlreadyAdded = true;

                    foreach (int item in chapterAdded)
                    {
                        if (nonDuplicatedChapters.Contains(item))
                        {
                            nonDuplicatedChapters.Remove(item); // Remove duplicated chapters
                        }
                    }
                }
            }
            return chapterAlreadyAdded;
        }
        #endregion
    }
}
