using ScanNetDownloader.Logic;
using ScanNetDownloader.Logic.Helpers;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;

namespace ScanNetDownloader.View.CustomControls
{
    /// <summary>
    /// Logique d'interaction pour ScanManagerView.xaml
    /// </summary>
    public partial class ScanManagerView : UserControl, INotifyPropertyChanged
    {
        private List<ScanData> ScanDatas => ScansLocalData.Instance.ScanDataList;

        private string SortPropertyBeforeSearch = string.Empty;
        private List<string> SearchedWords = new List<string>();
        private List<int> SearchedNumbers = new List<int>();

        private ObservableCollection<ScanItem> _scanListItems;
        public ObservableCollection<ScanItem> ScanListItems
        {
            get { return _scanListItems; }
            set
            {
                _scanListItems = value;
            }
        }

        private int _selectedCount;
        public int SelectedCount
        {
            get { return _selectedCount; }
            set {
                _selectedCount = value;
                if (_selectedCount > 0)
                {
                    if(btnStart.IsEnabled == false) btnStart.IsEnabled = true;
                }
                else
                {
                    btnStart.IsEnabled = false;
                }
            }
        }

        private string _filterSearch = "";
        public string FilterSearch
        {
            get { return _filterSearch; }
            set {
                _filterSearch = value;
                OnPropertyChanged();
            }
        }

        private List<ScanItem> searchPerfectMatchs = new List<ScanItem>();


        public static RoutedEvent StartDownloadEvent = EventManager.RegisterRoutedEvent(nameof(StartDownload), RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(ScanManagerView));
        public event RoutedEventHandler StartDownload
        {
            add { AddHandler(StartDownloadEvent, value); }
            remove { RemoveHandler(StartDownloadEvent, value); }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string property = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(property));
        }

        public ScanManagerView()
        {
            DataContext = this;
            ScanListItems = new ObservableCollection<ScanItem>();

            InitializeComponent();

            if (DesignerMode.IsInDesignerMode == false)
            {
                RefreshScanListView(); // Populate listView based on the local data
                // Note: Settings are refreshed at OptionsView Initialization

                // Sort the list view using the book name alphabetical order
                GridViewSorter.ApplySort(listVwScans, Settings.Instance.ScanManagerListViewSortProperty);
            }

            if (ScanListItems.Count == 0) stPanelNoScans.Visibility = Visibility.Visible;
            else stPanelNoScans.Visibility = Visibility.Collapsed;
        }

        #region Ui Routed Events
        private void btnStart_Click(object sender, RoutedEventArgs e)
        {
            RaiseEvent(new RoutedEventArgs(StartDownloadEvent, this));
        }

        private void btnAdd_Click(object sender, RoutedEventArgs e)
        {
            Window parentWindow = Window.GetWindow(this);

            AddScanWindow addWindow = new AddScanWindow(parentWindow);
            parentWindow.Opacity = 0.4;
            addWindow.ShowDialog();
            parentWindow.Opacity = 1;

            if (addWindow.Success)
            {
                string urlInput = addWindow.UrlInput;
                List<ScanData> newScansToAdd = addWindow.NewScanDatas;

                if (newScansToAdd == null || newScansToAdd.Count == 0)
                {
                    string mBoxMessage = $"No scan data to add for {urlInput}, make sure you used a valid url";
                    string mBoxCaption = "No scan data found";
                    MsgWindow.ShowOkWindow(mBoxCaption, mBoxMessage, false, MsgWindow.ImageType.Error);
                }
                else
                {
                    AddScanItems(newScansToAdd);
                }
            }
        }

        private void txtBoxSearch_TextInputChanged(object sender, RoutedEventArgs e)
        {
            SearchForBookOrChapter();
        }

        private void ScanItem_DeleteBtnPressed(object sender, RoutedEventArgs e)
        {
            ScanItem item = e.Source as ScanItem;
            if (item != null)
            {
                //TODO: Add an option to let the user decide this ?

                // Ask user before deleting scan item
                //string header = "Delete scan data";
                //string msg = $"Are you sure you want to delete {item.BookName} - Chapter {item.ChapterId} from the list?\n\nLocal files such as downloaded images and .CBZ archive won't be deleted.";
                //YesNoWindow yesNoWindow = MsgWindow.ShowYesNoWindow(header, msg, true, MsgWindow.ImageType.Warning);
                //if (yesNoWindow.Success) DeleteScanItem(item);

                // Delete item instantly
                DeleteScanItem(item);
            }
        }

        private void ScanItem_CreateCbzBtnPressed(object sender, RoutedEventArgs e)
        {
            ScanItem item = e.Source as ScanItem;
            if (item != null)
            {
                // Verify if cbz is created
                bool cbzCreated = FileManagement.CbzFileExist(item.linkedScanData);
                item.CbzArchiveCreated = cbzCreated;
                Debug.WriteLine($"CBZ CREATION BUTTON: Item={item.BookName}-{item.ChapterId}");

                if ((cbzCreated == false && item.DownloadStatus.OnlyImageDownloaded()) || item.DownloadStatus.MissingImages())
                {
                    string mBoxCaption = "CBZ archive creation";
                    string mBoxMessage;
                    if(item.DownloadStatus.MissingImages())
                    {
                        mBoxMessage = $"Do you want to rebuild the .cbz for {item.BookName} - Chapter {item.ChapterId} ?";
                    }
                    else
                    {
                        mBoxMessage = $"Do you want to create a .cbz for {item.BookName} - Chapter {item.ChapterId} ?";
                    }                

                    YesNoWindow yesNoWindow = MsgWindow.ShowYesNoWindow(mBoxCaption, mBoxMessage, true, MsgWindow.ImageType.Question);
                    if (yesNoWindow.Success)
                    {
                        CbzCreator.BuildCbzArchive(item);

                        ScanItem.DownloadedStatus downloadStatus = FileManagement.GetDownloadStatus(item.linkedScanData);
                        bool cbzSuccessfullyCreated = downloadStatus.IsCbzCreated();

                        item.CbzArchiveCreated = cbzSuccessfullyCreated;
                        item.DownloadStatus = downloadStatus;
                    }
                }
            }
        }

        private void ScanItem_StatusBtnPressed(object sender, RoutedEventArgs e)
        {
            ScanItem item = e.Source as ScanItem;
            if (item != null)
            {
                ScanItem.DownloadedStatus downloadStatus = FileManagement.GetDownloadStatus(item.linkedScanData);
                item.DownloadStatus = downloadStatus;
            }
        }

        private void ScanItem_IsSelectedForDownload(object sender, RoutedEventArgs e)
        {
            ScanItem item = e.Source as ScanItem;
            if (item != null)
            {
                if (item.IsSelectedForDownload == true) // was false before +1
                {
                    SelectedCount++;
                }
                else // was true before -1
                {
                    SelectedCount--;
                }
            }
        }
        #endregion

        private void AddScanItems(List<ScanData> newScansToAdd)
        {
            // TODO: Check for duplicated ScanData (Same BookName, chapter and url)

            // Add in saved data
            ScanDatas.AddRange(newScansToAdd);

            // Save the scans local data
            ScansLocalData.Save();

            // Add item in list view
            foreach (ScanData scanData in newScansToAdd)
            {
                // TODO: Add a check to prevent a double entry of the same chapter on the same website, maybe check before calling AddScanItems ?

                // TODO: ? Is this really necessary ? How could a new scan be downloaded or have a cbz archive ?
                ScanItem.DownloadedStatus downloadStatus = FileManagement.GetDownloadStatus(scanData);
                bool cbzAlreadyCreated = downloadStatus.IsCbzCreated();

                ScanItem item = new ScanItem(scanData, cbzAlreadyCreated, downloadStatus);
                item.DeleteScanBtnPressed += ScanItem_DeleteBtnPressed;
                item.CreateCbzBtnPressed += ScanItem_CreateCbzBtnPressed;
                ScanListItems.Add(item);

                SelectedCount++;
            }

            if (ScanListItems.Count != 0 && stPanelNoScans.Visibility == Visibility.Visible)
            {
                stPanelNoScans.Visibility = Visibility.Collapsed;
            }
        }

        private void DeleteScanItem(ScanItem itemToDelete)
        {
            // Remove from saved data
            ScanDatas.Remove(itemToDelete.linkedScanData);

            // Save the scans local data
            ScansLocalData.Save();

            if (itemToDelete.IsSelectedForDownload) SelectedCount--;

            // Remove item from list view
            ScanListItems.Remove(itemToDelete);   

            if (ScanListItems.Count == 0 && stPanelNoScans.Visibility != Visibility.Visible)
            {
                stPanelNoScans.Visibility = Visibility.Visible;
            }
        }

        public void ForceScanDataRefresh()
        {
            RefreshScanListView();

            if (ScanListItems.Count == 0) stPanelNoScans.Visibility = Visibility.Visible;
            else stPanelNoScans.Visibility = Visibility.Collapsed;
        }

        private void RefreshScanListView()
        {
            // TODO: Why is it so long with a lot of items ? Way to optimize this ?
            Debug.WriteLine("REFRESH SCAN LIST");
            ScanListItems.Clear();


            foreach (ScanData scanData in ScanDatas)
            {
                if (scanData.IsSelectedForDownload) SelectedCount++;

                ScanItem.DownloadedStatus downloadStatus = FileManagement.GetDownloadStatus(scanData);
                bool cbzAlreadyCreated = downloadStatus.IsCbzCreated();

                ScanItem item = new ScanItem(scanData, cbzAlreadyCreated, downloadStatus);
                item.DeleteScanBtnPressed += ScanItem_DeleteBtnPressed;
                item.CreateCbzBtnPressed += ScanItem_CreateCbzBtnPressed;
                item.StatusBtnPressed += ScanItem_StatusBtnPressed;
                item.IsSelectedModified += ScanItem_IsSelectedForDownload;
                ScanListItems.Add(item);
            }

            if(SelectedCount == 0) btnStart.IsEnabled = false;
        }

        // TODO: Clean this and place logically in code

        #region Search Filtering
        private void SearchForBookOrChapter()
        {
            // Reset perfect match
            if (searchPerfectMatchs.Count > 0)
            {
                foreach (ScanItem item in searchPerfectMatchs)
                {
                    item.IsSearchPerfectMatch = false;
                }

                searchPerfectMatchs.Clear();
            }

            if (string.IsNullOrEmpty(FilterSearch))
            {
                listVwScans.Items.Filter = null;

                // Revert sort mode
                if (string.IsNullOrEmpty(SortPropertyBeforeSearch) == false)
                {
                    GridViewSorter.ApplySort(listVwScans, SortPropertyBeforeSearch);
                    SortPropertyBeforeSearch = string.Empty;
                }

                // Hide no result message if it was displayed
                if (stPanelNoMatchingResult.Visibility == Visibility.Visible)
                {
                    stPanelNoMatchingResult.Visibility = Visibility.Collapsed;
                }
            }
            else
            {
                // Clear searched terms
                SearchedWords.Clear();
                SearchedNumbers.Clear();

                bool IsNumber = int.TryParse(FilterSearch, out int searchedInt);

                if (IsNumber)
                {
                    // Search for a chapter
                    SearchedWords.Add(FilterSearch); // also add number as word in case a book name with number was searched
                    SearchedNumbers.Add(searchedInt);

                    // Use search terms to filter view
                    listVwScans.Items.Filter = FilterBookNameAndChapterId;
                }
                else
                {
                    // Split with space then check every split for number -> no need for regex, each term separated by space should be a term of search
                    string[] searchTermsSplit = FilterSearch.Split(Constants.SPACE, StringSplitOptions.RemoveEmptyEntries);

                    if (searchTermsSplit.Count() == 1)
                    {
                        // Search for this string
                        SearchedWords.Add(searchTermsSplit[0]);

                        // Use search terms to filter view
                        listVwScans.Items.Filter = FilterBookName;
                    }
                    else // Checks every split to know if they are string or numbers
                    {
                        foreach (string searchTerm in searchTermsSplit)
                        {
                            SearchedWords.Add(searchTerm); // Add every term as word (even number in case a book name with number was searched)

                            bool termIsNumber = int.TryParse(searchTerm, out int termAsInt);
                            if (termIsNumber) SearchedNumbers.Add(termAsInt);
                            // TODO: special case
                            // numbers separated by ,
                            // Range with -
                        }

                        // Use search terms to filter view
                        listVwScans.Items.Filter = FilterBookNameAndChapterId;
                    }
                }

                // Change colums sorting
                string currentSortProperty = listVwScans.Items.SortDescriptions[0].PropertyName;
                if (string.IsNullOrEmpty(SortPropertyBeforeSearch))
                {
                    SortPropertyBeforeSearch = currentSortProperty;
                }

                if (SearchedWords.Count == SearchedNumbers.Count) // All terms are numbers order by chapter
                {
                    if (currentSortProperty != nameof(ScanItem.ChapterId))
                    {
                        GridViewSorter.ApplySort(listVwScans, nameof(ScanItem.ChapterId));
                    }
                }
                else
                {
                    if (currentSortProperty != nameof(ScanItem.BookName))
                    {
                        GridViewSorter.ApplySort(listVwScans, nameof(ScanItem.BookName));
                    }
                }

                // Display a message if no scan data match the search
                if (listVwScans.Items.Count == 0)
                {
                    stPanelNoMatchingResult.Visibility = Visibility.Visible;
                }
                else if (stPanelNoMatchingResult.Visibility == Visibility.Visible)
                {
                    stPanelNoMatchingResult.Visibility = Visibility.Collapsed;
                }
            }
        }

        private bool FilterBookNameAndChapterId(object obj)
        {
            ScanItem item = obj as ScanItem;

            // Check for perfect match
            if (SearchedWords.Count >= 2 && SearchedNumbers.Count == 1)
            {
                int chapterSearched = SearchedNumbers[0];
                string words = string.Join(" ", SearchedWords.Where(s => !s.Equals(chapterSearched.ToString())));

                bool matchingName = item.BookName.ToLower().Contains(words.ToLower());
                bool matchingChapter = item.ChapterId == chapterSearched;
                if (matchingName && matchingChapter)
                {
                    item.IsSearchPerfectMatch = true;
                    searchPerfectMatchs.Add(item);
                    return true;
                }

                return matchingName;
            }
            else
            {
                if (SearchedWords.Count == SearchedNumbers.Count)
                {
                    // Search numbers first
                    if (SearchNumberMatch(item, SearchedNumbers)) return true;
                    if (SearchWordMatch(item, SearchedWords)) return true;

                }
                else
                {
                    // Search words first
                    if (SearchWordMatch(item, SearchedWords)) return true;
                    if (SearchNumberMatch(item, SearchedNumbers)) return true;
                }

                // No matching condition 
                return false;
            }
        }

        private bool FilterBookName(object obj)
        {
            ScanItem item = obj as ScanItem;

            return SearchWordMatch(item, SearchedWords);
        }

        private bool FilterChapter(object obj)
        {
            ScanItem item = obj as ScanItem;

            return SearchNumberMatch(item, SearchedNumbers);
        }

        private bool SearchWordMatch(ScanItem item, List<string> searchedWords)
        {
            foreach (string wordSearched in searchedWords)
            {
                if (item.BookName.ToLower().Contains(wordSearched.ToLower()))
                {
                    return true;
                }
            }

            return false;
        }

        private bool SearchNumberMatch(ScanItem item, List<int> searchedNumbers)
        {
            foreach (int numberSearched in searchedNumbers)
            {
                bool matchingChapterId = item.ChapterId == numberSearched;
                if (matchingChapterId) return true;
                else
                {
                    int searchedIntDigitCount = numberSearched.CountDigits();
                    int chapterIdDigitCount = item.ChapterId.CountDigits();
                    if (numberSearched > 0 && searchedIntDigitCount < chapterIdDigitCount)
                    {
                        bool matchingFirstDigits = item.ChapterId.GetFirstDigits(searchedIntDigitCount) == numberSearched;
                        if (matchingFirstDigits)
                        {
                            return true;
                        }
                    }
                }
            }

            return false;
        } 
        #endregion
    }
}
