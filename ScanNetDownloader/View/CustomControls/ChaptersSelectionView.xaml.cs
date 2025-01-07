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
using ScanNetDownloader.Logic.Helpers;
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

        private ObservableCollection<ChapterItem> _chapterItems;
        public ObservableCollection<ChapterItem> ChapterItems
        {
            get { return _chapterItems; }
            set { _chapterItems = value; }
        }


        private string _selectChapterInfos = "Select chapters for...";
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

        private string _errorMessageSingle;
        public string ErrorMessageSingle
        {
            get { return _errorMessageSingle; }
            set {
                _errorMessageSingle = value;
                OnPropertyChanged();
            }
        }

        private string _errorMessageRange;
        public string ErrorMessageRange
        {
            get { return _errorMessageRange; }
            set
            {
                _errorMessageRange = value;
                OnPropertyChanged();
            }
        }


        public event PropertyChangedEventHandler? PropertyChanged;

        // View Events
        public static RoutedEvent BackBtnPressedEvent = EventManager.RegisterRoutedEvent(nameof(BackBtnPressed), RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(ChaptersSelectionView));

        public event RoutedEventHandler BackBtnPressed
        {
            add { AddHandler(BackBtnPressedEvent, value); }
            remove { RemoveHandler(BackBtnPressedEvent, value); }
        }

        public static RoutedEvent ChaptersConfirmedEvent = EventManager.RegisterRoutedEvent(nameof(ChaptersConfirmed), RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(ChaptersSelectionView));

        

        public event RoutedEventHandler ChaptersConfirmed
        {
            add { AddHandler(ChaptersConfirmedEvent, value); }
            remove { RemoveHandler(ChaptersConfirmedEvent, value); }
        }

        public ChaptersSelectionView()
        {
            DataContext = this;
            ChapterItems = new ObservableCollection<ChapterItem>();
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
            SelectChapterInfos = $"Select chapters for {tempScanData.BookName}";

            // TODO: Do I need to tell the user a chapter has already been added ?
            //if (tempScanData.UrlContainsChapter())
            //{
            //    SelectChapterInfos = $"Chapter {tempScanData.ChapterId} is already specified in {tempScanData.Url} but you can add additionnal chapter";
            //}
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
                    ErrorMessageSingle = $"Chapter {chapterToAdd} is already added";
                    ShowErrorAlertOnTextBox(txtBoxSingleChapter);
                    ShowErrorAlert(true, errorAlertAddSingle);
                }
                else
                {
                    // Add chapter
                    string chapterKey = SingleChapterInput;
                    AddToChaptersSelection(chapterKey, new List<int>() { chapterToAdd });

                    // Clear txt box
                    SingleChapterInput = "";

                    // Clear potential error
                    ShowErrorAlert(false, errorAlertAddSingle);
                }
            }
            else // Invalid number entered
            {
                if(string.IsNullOrEmpty(SingleChapterInput))
                {
                    ErrorMessageSingle = $"No chapter number provided";
                }
                else
                {
                    ErrorMessageSingle = $"\"{SingleChapterInput}\" is not a valid number";
                }             
                ShowErrorAlertOnTextBox(txtBoxSingleChapter);
                ShowErrorAlert(true, errorAlertAddSingle);
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
                        ErrorMessageRange = $"{startChapter}-{endChapter} all chapters in the range are already added";
                        ShowErrorAlertOnTextBox(txtBoxEndChapter);
                        ShowErrorAlertOnTextBox(txtBoxStartChapter);
                        ShowErrorAlert(true, errorAlertAddRange);
                    }
                    else
                    {
                        nonDuplicatedChapters.Log();
                        List<string> chaptersForErrorMsg = new List<string>();

                        while (nonDuplicatedChapters.Count > 0)
                        {
                            if (nonDuplicatedChapters.Count == 1)
                            {
                                string chapterKey = nonDuplicatedChapters[0].ToString();
                                AddToChaptersSelection(chapterKey, new List<int>() { nonDuplicatedChapters[0] });
                                nonDuplicatedChapters.Remove(nonDuplicatedChapters[0]);
                                chaptersForErrorMsg.Add(chapterKey);
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
                                    chaptersForErrorMsg.Add(chapterKey);
                                }
                                else // add first item as single chapter
                                {
                                    string chapterKey = nonDuplicatedChapters.First().ToString();
                                    AddToChaptersSelection(chapterKey, new List<int>() { nonDuplicatedChapters.First() });
                                    nonDuplicatedChapters.Remove(nonDuplicatedChapters.First());
                                    chaptersForErrorMsg.Add(chapterKey);
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
                                chaptersForErrorMsg.Add(chapterKey);
                            }
                        }

                        if(chaptersForErrorMsg.Count > 0)
                        {
                            string chaptersToShowInMsg = string.Join(',', chaptersForErrorMsg);
                            ErrorMessageRange = $"Some chapters were already added, only chapters {chaptersToShowInMsg} have been added";
                            ShowErrorAlert(true, errorAlertAddRange);
                        }

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

                    // Clear potential error
                    ShowErrorAlert(false, errorAlertAddRange);

                    // Clear txt box
                    StartChapterInput = "";
                    EndChapterInput = "";
                }
            }
            else // Invalid number entered
            {                          
                string errorMessage = "";

                if(string.IsNullOrEmpty(StartChapterInput) && string.IsNullOrEmpty(EndChapterInput))
                {
                    errorMessage = "No chapter number provided";
                    ShowErrorAlertOnTextBox(txtBoxStartChapter);
                    ShowErrorAlertOnTextBox(txtBoxEndChapter);
                }
                else
                {
                    if (validStartChapter == false)
                    {
                        if (string.IsNullOrEmpty(StartChapterInput))
                        {
                            errorMessage += $"No chapter number provided";
                        }
                        else
                        {
                            errorMessage += $"\"{StartChapterInput}\" is not a valid number";
                        }
                        ShowErrorAlertOnTextBox(txtBoxStartChapter);
                    }

                    if (validEndChapter == false)
                    {
                        if (string.IsNullOrEmpty(EndChapterInput))
                        {
                            errorMessage += string.IsNullOrEmpty(errorMessage) ? $"No chapter number provided" : $" and no chapter number provided";
                        }
                        else
                        {
                            errorMessage += string.IsNullOrEmpty(errorMessage) ? $"\"{EndChapterInput}\" is not a valid number" : $" and \"{EndChapterInput}\" is not a valid number";

                        }
                        ShowErrorAlertOnTextBox(txtBoxEndChapter);
                    }
                }  

                ErrorMessageRange = errorMessage;
                ShowErrorAlert(true, errorAlertAddRange);
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

        private void txtBoxSingleChapter_TextInputChanged(object sender, RoutedEventArgs e)
        {
            if (ErrorAlertIsShown(txtBoxSingleChapter))
            {
                ClearAlertOnTextBox(txtBoxSingleChapter);
            }
        }

        private void txtBoxStartChapter_TextInputChanged(object sender, RoutedEventArgs e)
        {
            if (ErrorAlertIsShown(txtBoxStartChapter))
            {
                ClearAlertOnTextBox(txtBoxStartChapter);
            }
        }  

        private void txtBoxEndChapter_TextInputChanged(object sender, RoutedEventArgs e)
        {
            if (ErrorAlertIsShown(txtBoxEndChapter))
            {
                ClearAlertOnTextBox(txtBoxEndChapter);
            }
        }

        private void ChapterItem_DeleteChapter(object sender, RoutedEventArgs e)
        {
            ChapterItem item = e.Source as ChapterItem;
            if (item != null)
            {
                DeleteChaptersFromSelection(item);
            }   
        }
        #endregion

        #region Chapter Management
        private void AddToChaptersSelection(string chapterKey, List<int> chapterValues)
        {
            ChaptersSelection.Add(chapterKey, chapterValues);
            AddChapterItem(chapterKey);

            if (ChaptersSelection.Count > 0) btnConfirmChapters.IsEnabled = true;
        }

        private void DeleteChaptersFromSelection(ChapterItem item)
        {
            ChaptersSelection.Remove(item.ChapterKey);
            DeleteChapterItem(item);
            if (ChaptersSelection.Count == 0) btnConfirmChapters.IsEnabled = false;
        }

        private void AddChapterItem(string chapterKey)
        {
            ChapterItem chapterItem = new ChapterItem(chapterKey);
            chapterItem.Margin = new Thickness(5);
            chapterItem.DeleteChapterBtnPressed += ChapterItem_DeleteChapter;

            ChapterItems.Add(chapterItem); // Object ordered on display with a CollectionViewSource
        }

        private void DeleteChapterItem(ChapterItem item)
        {         
            ChapterItems.Remove(item);          
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


        #region Error Alters
        private void ShowErrorAlert(bool show, ErrorAlert alertToShow)
        {
            int shownHeight = 20;
            int hiddenHeight = 10;
            
            if (show)
            {
                alertToShow.Visibility = Visibility.Visible;
                alertToShow.Height = shownHeight;
            }
            else
            {
                alertToShow.Visibility = Visibility.Hidden;
                alertToShow.Height = hiddenHeight;
            }
        }

        private void ShowErrorAlertOnTextBox(TextInputBox txtBox)
        {
            txtBox.BorderBrush = (SolidColorBrush)FindResource("Colors.Red");
            txtBox.BorderThickness = new Thickness(2);
        }

        private void ClearAlertOnTextBox(TextInputBox txtBox)
        {
            txtBox.ClearValue(Control.BorderBrushProperty);
            txtBox.ClearValue(Control.BorderThicknessProperty);
        }

        private bool ErrorAlertIsShown(TextInputBox txtBox)
        {
            return txtBox.BorderThickness == new Thickness(2);
        }
        #endregion
    }
}
