using ScanNetDownloader.View.CustomControls;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace ScanNetDownloader.Logic.Helpers
{
    class GridViewSorter
    {
        #region Properties
        // COMMAND PROPERTY
        public static ICommand GetCommand(DependencyObject obj)
        {
            return (ICommand)obj.GetValue(CommandProperty);
        }

        public static void SetCommand(DependencyObject obj, ICommand value)
        {
            obj.SetValue(CommandProperty, value);
        }

        public static readonly DependencyProperty CommandProperty =
            DependencyProperty.RegisterAttached("Command", typeof(ICommand), typeof(GridViewSorter),
                new UIPropertyMetadata(null, (depObj, eventArgs) =>
                {
                    ItemsControl listView = depObj as ItemsControl;
                    if (listView != null)
                    {
                        if (!GetAutoSort(listView)) // Only change ClickHandler when AutoSort is disabled
                        {
                            // Add Column Click Handler
                            if (eventArgs.OldValue == null && eventArgs.NewValue != null)
                            {
                                listView.AddHandler(GridViewColumnHeader.ClickEvent, new RoutedEventHandler(ColumnHeader_Click));
                            }

                            // Remove Column Click Handler
                            if (eventArgs.OldValue != null && eventArgs.NewValue == null)
                            {
                                listView.RemoveHandler(GridViewColumnHeader.ClickEvent, new RoutedEventHandler(ColumnHeader_Click));
                            }
                        }
                    }
                })
            );

        // AUTO SORT PROPERTY
        public static bool GetAutoSort(DependencyObject obj)
        {
            return (bool)obj.GetValue(AutoSortProperty);
        }

        public static void SetAutoSort(DependencyObject obj, bool value)
        {
            obj.SetValue(AutoSortProperty, value);
        }

        public static readonly DependencyProperty AutoSortProperty =
            DependencyProperty.RegisterAttached("AutoSort", typeof(bool), typeof(GridViewSorter),
                new UIPropertyMetadata(false, (depObj, eventArgs) =>
                {
                    ItemsControl listView = depObj as ItemsControl;
                    if (listView != null)
                    {
                        if (GetCommand(listView) == null) // Only change click handler if no command has been set
                        {
                            bool oldValue = (bool)eventArgs.OldValue;
                            bool newValue = (bool)eventArgs.NewValue;

                            // Add Click Handler when AutoSort is set to true
                            if (!oldValue && newValue)
                            {
                                listView.AddHandler(GridViewColumnHeader.ClickEvent, new RoutedEventHandler(ColumnHeader_Click));
                            }

                            // Remove Click Handler when AutoSort is set to false
                            if (oldValue && !newValue)
                            {
                                listView.RemoveHandler(GridViewColumnHeader.ClickEvent, new RoutedEventHandler(ColumnHeader_Click));
                            }
                        }
                    }
                })
            );

        // PROPERTY NAME PROPERTY
        public static string GetPropertyName(DependencyObject obj)
        {
            return (string)obj.GetValue(PropertyNameProperty);
        }

        public static void SetPropertyName(DependencyObject obj, string value)
        {
            obj.SetValue(PropertyNameProperty, value);
        }

        public static readonly DependencyProperty PropertyNameProperty =
            DependencyProperty.RegisterAttached("PropertyName", typeof(string), typeof(GridViewSorter), new UIPropertyMetadata(null));

        #endregion

        #region Column header click event Handler
        private static void ColumnHeader_Click(object sender, RoutedEventArgs e)
        {
            GridViewColumnHeader headerClicked = e.OriginalSource as GridViewColumnHeader;
            if (headerClicked != null)
            {
                string propertyName = GetPropertyName(headerClicked.Column);
                if (!string.IsNullOrEmpty(propertyName))
                {
                    ListView listView = GetAncestor<ListView>(headerClicked);
                    if (listView != null)
                    {
                        ICommand command = GetCommand(listView);
                        if (command != null)
                        {
                            if (command.CanExecute(propertyName))
                            {
                                command.Execute(propertyName);
                            }
                        }
                        else if (GetAutoSort(listView))
                        {
                            ApplySort(listView, propertyName);
                        }
                    }
                }
            }
        }

        public static T GetAncestor<T>(DependencyObject reference) where T : DependencyObject
        {
            DependencyObject parent = VisualTreeHelper.GetParent(reference);
            while (!(parent is T)) // ? What happens if there is no parent that correspond to T ?
            {
                parent = VisualTreeHelper.GetParent(parent);
            }

            if (parent != null)
            {
                return (T)parent;
            }
            else
            {
                return null;
            }
        }

        public static void ApplySort(ListView listView, string propertyName)
        {
            ICollectionView collectionView = listView.Items;
            ListSortDirection direction = ListSortDirection.Ascending;
            bool sameSortProperty = false;

            if (collectionView.SortDescriptions.Count > 0)
            {
                SortDescription currentSort = collectionView.SortDescriptions[0];
                sameSortProperty = currentSort.PropertyName == propertyName;
                if (sameSortProperty)
                {
                    if (currentSort.Direction == ListSortDirection.Ascending)
                    {
                        direction = ListSortDirection.Descending;
                    }
                    else
                    {
                        direction = ListSortDirection.Ascending;
                    }
                }
                collectionView.SortDescriptions.Clear();
            }

            if (!string.IsNullOrEmpty(propertyName))
            {
                collectionView.SortDescriptions.Add(new SortDescription(propertyName, direction));

                // TODO: Transform this into a custom sort command instead of using AutoSort
                // Add secondary sorting depending on the property passed + Only change ScanManagerListViewSortProperty if property is from a ScanItem
                if (propertyName == nameof(ScanItem.BookName))
                {
                    Debug.WriteLine($"Sort by book name, then by ChapterId");
                    collectionView.SortDescriptions.Add(new SortDescription(nameof(ScanItem.ChapterId), ListSortDirection.Ascending));

                    if(!sameSortProperty)
                    {
                        Settings.Instance.ScanManagerListViewSortProperty = propertyName;
                        Debug.WriteLine($"Call Save");
                        Settings.Save();
                    }
                }
                else if (propertyName == nameof(ScanItem.ChapterId))
                {
                    Debug.WriteLine($"Sort by chapterID, then by BookName");
                    collectionView.SortDescriptions.Add(new SortDescription(nameof(ScanItem.BookName), ListSortDirection.Ascending));

                    if (!sameSortProperty)
                    {
                        Settings.Instance.ScanManagerListViewSortProperty = propertyName;
                        Debug.WriteLine($"Call Save");
                        Settings.Save();
                    }
                }
                else if (propertyName == nameof(ScanItem.IsSelectedForDownload) || propertyName == nameof(ScanItem.DownloadStatus) || propertyName == nameof(ScanItem.PagesCount) || propertyName == nameof(ScanItem.Website))
                {
                    Debug.WriteLine($"Sort by {propertyName}, then by BookName and then by chapterId");
                    collectionView.SortDescriptions.Add(new SortDescription(nameof(ScanItem.BookName), ListSortDirection.Ascending));
                    collectionView.SortDescriptions.Add(new SortDescription(nameof(ScanItem.ChapterId), ListSortDirection.Ascending));

                    if (!sameSortProperty)
                    {
                        Settings.Instance.ScanManagerListViewSortProperty = propertyName;
                        Debug.WriteLine($"Call Save");
                        Settings.Save();
                    }
                }
            }
        }

        #endregion
    }
}
