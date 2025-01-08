using ScanNetDownloader.Logic;
using ScanNetDownloader.Logic.Helpers;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
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
    /// Logique d'interaction pour ScanManagerView.xaml
    /// </summary>
    public partial class ScanManagerView : UserControl
    {
        private List<ScanData> ScanDatas => ScansLocalData.Instance.ScanDataList;

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


        public static RoutedEvent StartDownloadEvent = EventManager.RegisterRoutedEvent(nameof(StartDownload), RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(ScanManagerView));

        public event RoutedEventHandler StartDownload
        {
            add { AddHandler(StartDownloadEvent, value); }
            remove { RemoveHandler(StartDownloadEvent, value); }
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

            if (addWindow.Success) // TODO: Clean this
            {
                string urlInput = addWindow.UrlInput;
                List<ScanData> newScansToAdd = addWindow.NewScanDatas;

                if (newScansToAdd == null || newScansToAdd.Count == 0)
                {
                    string mBoxMessage = $"No scan data to add for {urlInput}, make sure you used a valid url";
                    string mBoxCaption = "No scan data";
                    MessageBox.Show(mBoxMessage, mBoxCaption, MessageBoxButton.OK, MessageBoxImage.Warning);
                }
                else
                {
                    AddScanItems(newScansToAdd);
                }
            }
        }

        private void ScanItem_DeleteBtnPressed(object sender, RoutedEventArgs e)
        {
            ScanItem item = e.Source as ScanItem;
            if (item != null)
            {
                DeleteScanItem(item);
            }
        }

        private void ScanItem_CreateCbzBtnPressed(object sender, RoutedEventArgs e)
        {
            ScanItem item = e.Source as ScanItem;
            if (item != null)
            {
                // Verify if cbz is created
                bool cbzCreated = FileManagement.IsCbzArchiveCreated(item.linkedScanData);
                item.CbzArchiveCreated = cbzCreated;
                Debug.WriteLine($"CBZ CREATION BUTTON: Item={item.BookName}-{item.ChapterId}");

                if (cbzCreated == false)
                {
                    // TODO: Propose to build cbz
                    string mBoxMessage = $"Do you want to create a .cbz for {item.BookName} - Chapter {item.ChapterId} ?";
                    string mBoxCaption = "CBZ Archive creation";
                    MessageBoxResult result = MessageBox.Show(mBoxMessage, mBoxCaption, MessageBoxButton.YesNo, MessageBoxImage.Question);
                    if (result == MessageBoxResult.Yes)
                    {
                        string chapterPath = FileManagement.GetChapterDirectoryPath(item.linkedScanData);
                        CbzCreator.BuildCbzArchive(item, chapterPath);

                        bool cbzSuccessfullyCreated = FileManagement.IsCbzArchiveCreated(item.linkedScanData);
                        item.CbzArchiveCreated = cbzSuccessfullyCreated;
                    }
                }
            }
        }

        private void ScanItem_StatusBtnPressed(object sender, RoutedEventArgs e)
        {
            ScanItem item = e.Source as ScanItem;
            if (item != null)
            {
                bool fileDownloaded = FileManagement.AreScanFilesDownloaded(item.linkedScanData);
                item.IsDownloaded = fileDownloaded;
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

        private void AddScanItems(List<ScanData> newScansToAdd) // TODO: Replace the refresh by a add function to prevent recreating the whole view everytime
        {
            // TODO: Check for duplicated ScanData (Same BookName, chapter and url)

            // Add in saved data
            ScanDatas.AddRange(newScansToAdd);

            // Save the scans local data
            ScansLocalData.Save();

            // Add item in list view
            foreach (ScanData scanData in newScansToAdd)
            {
                // TODO: Add a check to prevent a double entry of the same chapter on the same website, maybe check before caliing AddScanItems ?
                bool filesAlreadyDownloaded = FileManagement.AreScanFilesDownloaded(scanData);
                bool cbzAlreadyCreated = FileManagement.IsCbzArchiveCreated(scanData);

                ScanItem item = new ScanItem(scanData, filesAlreadyDownloaded, cbzAlreadyCreated);
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

        private void RefreshScanListView()
        {
            // TODO: Why is it so long with a lot of items ? Way to optiomize this ?
            Debug.WriteLine("REFRESH SCAN LIST");
            ScanListItems.Clear();


            foreach (ScanData scanData in ScanDatas)
            {
                if (scanData.IsSelectedForDownload) SelectedCount++;

                bool filesAlreadyDownloaded = FileManagement.AreScanFilesDownloaded(scanData);
                bool cbzAlreadyCreated = FileManagement.IsCbzArchiveCreated(scanData);

                ScanItem item = new ScanItem(scanData, filesAlreadyDownloaded, cbzAlreadyCreated);
                item.DeleteScanBtnPressed += ScanItem_DeleteBtnPressed;
                item.CreateCbzBtnPressed += ScanItem_CreateCbzBtnPressed;
                item.StatusBtnPressed += ScanItem_StatusBtnPressed;
                item.IsSelectedModified += ScanItem_IsSelectedForDownload;
                ScanListItems.Add(item);
            }

            if(SelectedCount == 0) btnStart.IsEnabled = false;
        }
    }
}
