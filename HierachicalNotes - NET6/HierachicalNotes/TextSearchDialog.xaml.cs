using HierachicalNotes.Classes;
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
using System.Windows.Shapes;

namespace HierachicalNotes
{
    /// <summary>
    /// Interaction logic for TextSearchDialog.xaml
    /// </summary>
    public partial class TextSearchDialog : Window
    {
        private List<int> foundResult = new List<int>();
        private string searchingPattern = "";
        private int currSelected = 0;
        public TextSearchDialog()
        {
            InitializeComponent();
        }
        public delegate void ActionOnWindowClosedDelegate();
        public delegate List<int> ActionOnSearchDelegate(string searchText);
        public delegate void ActionOnSelectFoundDelegate(int start, int length);
        ActionOnWindowClosedDelegate? _actionOnWindowClosed = null;
        ActionOnSearchDelegate? _actionOnSearch = null;
        ActionOnSelectFoundDelegate? _actionOnSelectFound = null;

        private void ShowCurrentStatus()
        {
            string msg = "Enter a search pattern to find matches.";
            if (!string.IsNullOrEmpty(searchingPattern) && foundResult != null)
            {
                if(foundResult.Count <= 0)
                {
                    msg = $"Total found: {foundResult.Count}";
                }
                else
                {
                    msg = $"Total found: {foundResult.Count}, current: {currSelected + 1}";
                }
                
            }
            StatusLabel.Content = msg;
            EnableButtons();

        }
        private void EnableButtons()
        {
            if (tbSearchPattern.Text.Length > 0)
            {
                SearchButton.IsEnabled = true;
            }
            else
            {
                SearchButton.IsEnabled = false;
            }
            if (searchingPattern.Length > 0)
            {
                int count = foundResult.Count;
                if (count <= 1)
                {
                    FirstButton.IsEnabled = false;
                    PreviousButton.IsEnabled = false;
                    NextButton.IsEnabled = false;
                    LastButton.IsEnabled = false;
                }
                else
                {
                    PreviousButton.IsEnabled = true;
                    NextButton.IsEnabled = true;
                    if (currSelected == 0)
                    {
                        FirstButton.IsEnabled = false;
                    }
                    else
                    {
                        FirstButton.IsEnabled = true;
                    }
                    if(currSelected == count -1)
                    {
                        LastButton.IsEnabled = false;
                    }
                    else
                    {
                        LastButton.IsEnabled = true;
                    }
                }
            }
            else
            {
                FirstButton.IsEnabled = false;
                PreviousButton.IsEnabled = false;
                NextButton.IsEnabled = false;
                LastButton.IsEnabled = false;
            }
        }
        private void Window_Closed(object sender, EventArgs e)
        {
            if (_actionOnWindowClosed != null)
            {
                _actionOnWindowClosed();
            }
        }
        private void Window_Loaded(object sender, EventArgs e)
        {
            tbSearchPattern.Focus();
            ShowCurrentStatus();
        }
        private void Window_Activated(object sender, EventArgs e)
        {
            tbSearchPattern.Focus();
        }
        private void SearchTextChanged(object sender, EventArgs e)
        {
            if (tbSearchPattern.Text.Trim().Length > 0)
            {
                SearchButton.IsEnabled = true;
            }
            else
            {
                SearchButton.IsEnabled = false;
            }
        }
        public void SetSearchDelegates(ActionOnSearchDelegate? actionOnSearch, ActionOnSelectFoundDelegate actionOnSelectFound, ActionOnWindowClosedDelegate actionOnWindowClosed)
        {
            _actionOnSearch = actionOnSearch;
            _actionOnSelectFound = actionOnSelectFound;
            _actionOnWindowClosed = actionOnWindowClosed;
        }

        private void FirstButton_Click(object sender, RoutedEventArgs e)
        {
            if(foundResult.Count > 0 && _actionOnSelectFound != null)
            {
                currSelected = 0;
                _actionOnSelectFound(foundResult[currSelected], searchingPattern.Length);
            }
            ShowCurrentStatus();
        }

        private void PreviousButton_Click(object sender, RoutedEventArgs e)
        {
            if (foundResult.Count > 0 && _actionOnSelectFound != null)
            {
                if (currSelected <= 0)
                {
                    currSelected = foundResult.Count - 1;
                }
                else
                {
                    currSelected--;
                }
                _actionOnSelectFound(foundResult[currSelected], searchingPattern.Length);
            }
            ShowCurrentStatus();
        }

        private void NextButton_Click(object sender, RoutedEventArgs e)
        {
            if (foundResult.Count > 0 && _actionOnSelectFound != null)
            {
                if (currSelected >= foundResult.Count -1)
                {
                    currSelected = 0;
                }
                else
                {
                    currSelected++;
                }
                _actionOnSelectFound(foundResult[currSelected], searchingPattern.Length);
            }
            ShowCurrentStatus();
        }

        private void LastButton_Click(object sender, RoutedEventArgs e)
        {
            if (foundResult.Count > 0 && _actionOnSelectFound != null)
            {
                currSelected = foundResult.Count - 1;
                _actionOnSelectFound(foundResult[currSelected], searchingPattern.Length);
            }
            ShowCurrentStatus();
        }

        private void SearchButton_Click(object sender, RoutedEventArgs e)
        {
            if(_actionOnSearch != null && tbSearchPattern.Text.Trim().Length > 0)
            {
                foundResult = _actionOnSearch(tbSearchPattern.Text.Trim());
                searchingPattern = tbSearchPattern.Text.Trim();
                currSelected = 0;
                if(foundResult.Count > 0 && _actionOnSelectFound != null)
                {
                    _actionOnSelectFound(foundResult[currSelected], searchingPattern.Length);
                }
            }
            ShowCurrentStatus();
        }
    }
}
