using System;
using System.Collections.Generic;
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
using ScanNetDownloader.Logic;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace ScanNetDownloader.View.CustomControls
{
    /// <summary>
    /// Logique d'interaction pour ChaptersSelectionView.xaml
    /// </summary>
    public partial class ChaptersSelectionView : UserControl
    {
        private Dictionary<string, List<int>> ChaptersSelection { get; set; } = new Dictionary<string, List<int>>();

        public List<int> ChaptersSelected => GetSelectedChapters();

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
            InitializeComponent();
        }

        public void StartChapterSelection(ScanData tempScanData) // TODO: How to manage adding more chapters for Url with chapter number -> add url with chapter first remove chapter from list and then generate url for all the other added chapter
        {
            SetUrlInfo(tempScanData);

            if (tempScanData.UrlContainsChapter() )
            {
                if(ChapterAlreadyAdded(tempScanData.ChapterId) == false)
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
                txtBlockUrlInfo.Text = $"Chapter {tempScanData.ChapterId} is already specified in {tempScanData.Url} but you can add additionnal chapter";
            }
            else
            {
                txtBlockUrlInfo.Text = $"Choose chapters for {tempScanData.Url}";
            }      
        }   

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
            chapterTxtBlock.Margin = new Thickness(5,0,0,0);

            chapterSelectedPanel.Children.Add(chapterTxtBlock);

            btnConfirmChapters.IsEnabled = true;
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

        #region Buttons and Events
        private void btnAddSingleChapter_Click(object sender, RoutedEventArgs e)
        {
            bool validNumber = int.TryParse(txtBoxSingleChapter.Text, out int chapterToAdd);
            if (validNumber)
            {
                if (ChapterAlreadyAdded(chapterToAdd))
                {
                    txtBlockChapterError.Text = $"Chapter {chapterToAdd} is already added";
                    txtBoxSingleChapter.Background = Brushes.IndianRed;
                }
                else
                {
                    // Add chapter
                    string chapterKey = txtBoxSingleChapter.Text;
                    AddToChaptersSelection(chapterKey, new List<int>() { chapterToAdd });

                    // Clear txt box
                    txtBoxSingleChapter.Text = "";
                }
            }
            else // Invalid number entered
            {
                //TODO: Show error, Add text explanation
                txtBlockChapterError.Text = $"\"{txtBoxSingleChapter.Text}\" is not a valid number";
                txtBoxSingleChapter.Background = Brushes.IndianRed;
            }
        }

        private void btnAddRangeOfChapter_Click(object sender, RoutedEventArgs e)
        {
            int startChapter, endChapter;
            bool validStartChapter = int.TryParse(txtBoxStartChapter.Text, out startChapter);
            bool validEndChapter = int.TryParse(txtBoxEndChapter.Text, out endChapter);

            if (validStartChapter && validEndChapter)
            {
                if (startChapter > endChapter) (startChapter, endChapter) = (endChapter, startChapter); // invert two value to make sure start is the lower value

                if (ChaptersAlreadyAdded(startChapter, endChapter, out List<int> nonDuplicatedChapters))
                {
                    if (nonDuplicatedChapters.Count == 0)
                    {
                        txtBlockChapterError.Text = $"{startChapter}-{endChapter} all chapters in the range are already added";
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

                        txtBlockChapterError.Text = $"Some chapters were already added, only chapters {chaptersAdded} have been added";

                        // Clear txt box
                        txtBoxStartChapter.Text = "";
                        txtBoxEndChapter.Text = "";
                    }
                }
                else
                {
                    // Add chapter
                    string chapterKey = $"{startChapter}{Constants.DASH_CHAR}{endChapter}";
                    List<int> selectedChapters = Enumerable.Range(startChapter, (endChapter - startChapter) + 1).ToList();
                    AddToChaptersSelection(chapterKey, selectedChapters);

                    // Clear txt box
                    txtBoxStartChapter.Text = "";
                    txtBoxEndChapter.Text = "";
                }
            }
            else // Invalid number entered
            {
                //TODO: Show error, Add text explanation
                string errorMessage = "";
                if (validStartChapter == false)
                {
                    errorMessage += $"\"{txtBoxStartChapter.Text}\"";
                    txtBoxStartChapter.Background = Brushes.IndianRed;
                }
                if (validEndChapter == false)
                {
                    errorMessage += string.IsNullOrEmpty(errorMessage) ? $"\"{txtBoxEndChapter.Text}\"" : $", \"{txtBoxEndChapter.Text}\"";
                    txtBoxEndChapter.Background = Brushes.IndianRed;
                }
                errorMessage += " is not a valid number";
                txtBlockChapterError.Text = errorMessage;
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
        #endregion
    }
}
