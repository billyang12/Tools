using HierachicalNotes.Classes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Xml.Linq;

namespace HierachicalNotes
{
    /// <summary>
    /// Interaction logic for ucHNoteEditor.xaml
    /// </summary>
    public partial class ucHNoteEditor : UserControl
    {
        private HNote _hNode;
        public ucHNoteEditor()
        {
            InitializeComponent();
            _hNode = new HNote();
        }
        public void SetSpellingCheck(bool checkSpelling)
        {
            tbContent.SpellCheck.IsEnabled = checkSpelling;
            tbName.SpellCheck.IsEnabled = checkSpelling;
            tbKeywords.SpellCheck.IsEnabled = checkSpelling;
        }
        public HNote Node
        {
            get
            {
                getFromUI();
                return _hNode;
            }
            set
            {
                if (_textSearchWindow != null)
                {
                    _textSearchWindow.Close();
                    _textSearchWindow = null;
                }
                setToUI(value);
            }
        }
        void getFromUI()
        {
            if (_hNode == null)
            {
                _hNode = new HNote();
            }
            else
            {
                if(_hNode.Name != tbName.Text.Trim() || _hNode.Keywords != tbKeywords.Text.Trim() || _hNode.Content != tbContent.Text.Trim() || _hNode.Password != tbPassword.Password)
                {
                    _hNode.UpdateTime = DateTime.Now;
                }
            }
            _hNode.Name = tbName.Text.Trim();
            _hNode.Keywords = tbKeywords.Text.Trim();
            _hNode.Content = tbContent.Text;
            _hNode.Password=tbPassword.Password;
        }
        void setToUI(HNote node)
        {
            clear();
            _hNode = node;
            if (node != null)
            {
                tbName.Text = node.Name;
                tbKeywords.Text = node.Keywords;
                tbContent.Text = node.Content;
                tbPassword.Password = node.Password;
                lbCreatedDate.Content = node.CreateTime.ToString("yyyy-MM-dd hh:mm:ss");
                lbModifiedDate.Content = node.UpdateTime.ToString("yyyy-MM-dd hh:mm:ss");
            }
        }
        void clear()
        {
            tbName.Text = "";
            tbKeywords.Text = "";
            tbContent.Text = "";
            tbPassword.Password = "";
            lbCreatedDate.Content = "";
            lbModifiedDate.Content = "";
        }

        private void bShowPassword_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show(tbPassword.Password, "Password");
        }

        private void bCopyPassword_Click(object sender, RoutedEventArgs e)
        {
            Clipboard.SetText(tbPassword.Password);
            MessageBox.Show("Copied to clipboard\r\n\r\nWhen close this dialog, it will be removed from clipboard");
            Clipboard.Clear();
        }

        private void tbContentCtxMenu_Opened(object sender, RoutedEventArgs e)
        {
            string ssSelectedText = tbContent.SelectedText;
            if (string.IsNullOrEmpty(ssSelectedText))
            {
                tbContentCtxMenuMakeUrl.Visibility = Visibility.Collapsed;
                tbContentCtxMenuMakeBold.Visibility = Visibility.Collapsed;
                tbContentCtxMenuHtmlEncoding.Visibility = Visibility.Collapsed;
            }
            else
            {
                tbContentCtxMenuMakeUrl.Visibility = Visibility.Visible;
                tbContentCtxMenuMakeBold.Visibility = Visibility.Visible;
                tbContentCtxMenuHtmlEncoding.Visibility = Visibility.Visible;
            }
        }

        private void tbContentCtxMenuSelectAll_Click(object sender, RoutedEventArgs e)
        {
            tbContent.SelectAll();
        }

        private void tbContentCtxMenuClear_Click(object sender, RoutedEventArgs e)
        {
            tbContent.Clear();
        }

        private void tbContentCtxMenuMakeUrl_Click(object sender, RoutedEventArgs e)
        {
            string selection = tbContent.SelectedText;
            if (string.IsNullOrEmpty(selection)) return;
            tbContent.SelectedText = $"<a href='{selection}' target='blank'>{selection}</a>";
        }

        private void tbContentCtxMenuWrapText_Checked(object sender, RoutedEventArgs e)
        {
            tbContent.TextWrapping = TextWrapping.Wrap;
        }

        private void tbContentCtxMenuWrapText_Unchecked(object sender, RoutedEventArgs e)
        {
            tbContent.TextWrapping = TextWrapping.NoWrap;
        }

        private void tbContentCtxMenuMakeBold_Click(object sender, RoutedEventArgs e)
        {
            string selection = tbContent.SelectedText;
            if (string.IsNullOrEmpty(selection)) return;
            tbContent.SelectedText = $"<strong>{selection}</strong>";
        }

        private void tbContentCtxMenuHtmlEncoding_Click(object sender, RoutedEventArgs e)
        {
            string selection = tbContent.SelectedText;
            if (string.IsNullOrEmpty(selection)) return;
            tbContent.SelectedText = HttpUtility.HtmlEncode(selection);
        }

        private void tbContentCtxMenuCopyAll_Click(object sender, RoutedEventArgs e)
        {
            Clipboard.SetText(tbContent.Text);
        }

        private void tbContentCtxMenuBiggerFont_Click(object sender, RoutedEventArgs e)
        {
            double size = tbContent.FontSize;
            tbContent.FontSize = size + 2;
        }
        private void tbContentCtxMenuSmallerFont_Click(object sender, RoutedEventArgs e)
        {
            double size = tbContent.FontSize;
            tbContent.FontSize = size - 2;
        }
        void removeSpellingItems()
        {
            int len = tbContent.ContextMenu.Items.Count;
            int index = -1;
            for (int i = 0; i < len; i++)
            {
                MenuItem? item = tbContent.ContextMenu.Items[i] as MenuItem;
                if (item != null && item.Command == ApplicationCommands.Copy)
                {
                    index = i;
                    break;
                }
            }
            for (int j = index - 1; j >= 0; j--)
            {
                tbContent.ContextMenu.Items.RemoveAt(j);
            }
        }
        void insertSpellingCheckMenuItems()
        {
            int caretIndex, cmdIndex;
            SpellingError spellingError;

            caretIndex = tbContent.CaretIndex;

            cmdIndex = 0;
            spellingError = tbContent.GetSpellingError(caretIndex);
            if (spellingError != null)
            {
                foreach (string str in spellingError.Suggestions)
                {
                    MenuItem mi = new MenuItem();
                    mi.Header = str;
                    mi.FontWeight = FontWeights.Bold;
                    mi.Command = EditingCommands.CorrectSpellingError;
                    mi.CommandParameter = str;
                    mi.CommandTarget = tbContent;
                    tbContent.ContextMenu.Items.Insert(cmdIndex, mi);
                    cmdIndex++;
                }
                Separator separatorMenuItem1 = new Separator();
                tbContent.ContextMenu.Items.Insert(cmdIndex, separatorMenuItem1);
                cmdIndex++;
                MenuItem ignoreAllMI = new MenuItem();
                ignoreAllMI.Header = "Ignore All";
                ignoreAllMI.Command = EditingCommands.IgnoreSpellingError;
                ignoreAllMI.CommandTarget = tbContent;
                tbContent.ContextMenu.Items.Insert(cmdIndex, ignoreAllMI);
                cmdIndex++;
                Separator separatorMenuItem2 = new Separator();
                tbContent.ContextMenu.Items.Insert(cmdIndex, separatorMenuItem2);
            }
        }
                    
        private void tbContentCtxMenu_ContextMenuOpening(object sender, ContextMenuEventArgs e)
        {
            removeSpellingItems();

            insertSpellingCheckMenuItems();
        }

        #region ctrl+F search
        private static TextSearchDialog? _textSearchWindow = null;

        void ShowTextSearchWindow()
        {
            if (_textSearchWindow == null)
            {
                _textSearchWindow = new TextSearchDialog();
            }
            _textSearchWindow.SetSearchDelegates(
                  ActionSearch,
                  ActionSelectText,
                  () => { _textSearchWindow = null; }
                );
            var parent = Window.GetWindow(this);
            if (parent != null)
            {
                _textSearchWindow.Owner = parent;
                _textSearchWindow.Left = parent.Left + 50;
                _textSearchWindow.Top = parent.Top + (parent.Height - _textSearchWindow.Height) / 2;
            }

            _textSearchWindow.Show();
            if (_textSearchWindow.WindowState == WindowState.Minimized)
                _textSearchWindow.WindowState = WindowState.Normal;
        }
        private void UserControl_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.F && Keyboard.Modifiers == ModifierKeys.Control)
            {
                ShowTextSearchWindow();
            }
        }
        private void tbContentCtxMenuSearch_Click(object sender, RoutedEventArgs e)
        {
            ShowTextSearchWindow();
        }
        void ActionSelectText(int start, int length)
        {
            tbContent.Select(start, length);
            Window parentWindow = Window.GetWindow(this);
            if (parentWindow != null)
            {
                parentWindow.Activate();
                parentWindow.Focus();
            }
            int selectionStart = tbContent.SelectionStart;
            tbContent.ScrollToLine(tbContent.GetLineIndexFromCharacterIndex(selectionStart));
            tbContent.Select(start, length);
        }
        List<int> ActionSearch(string searchText)
        {
            return HighlightText(tbContent, searchText);
        }

        private List<int> HighlightText(TextBox textBox, string searchText)
        {
            string text = textBox.Text;
            
            int startIndex = 0;
            int matchIndex = -1;
            List<int> result = new List<int>();
            while ((matchIndex = text.IndexOf(searchText, startIndex, System.StringComparison.OrdinalIgnoreCase)) != -1)
            {
                result.Add(matchIndex);
                // Move start index forward
                startIndex = matchIndex + searchText.Length;
            }
            return result;
        }

        #endregion

        private void tbContentCtxMenuIndentRight_Click(object sender, RoutedEventArgs e)
        {
            // Get the selected text range
            int selectionStart = tbContent.SelectionStart;
            int selectionLength = tbContent.SelectionLength;

            // Get the start and end line indices
            int startLine = tbContent.GetLineIndexFromCharacterIndex(selectionStart);
            int endLine = tbContent.GetLineIndexFromCharacterIndex(selectionStart + selectionLength);

            // Modify the text for each affected line
            for (int i = startLine; i <= endLine; i++)
            {
                // Get the character index of the start of the line
                int lineStart = tbContent.GetCharacterIndexFromLineIndex(i);
                tbContent.Text = tbContent.Text.Insert(lineStart, "\t"); // Add a tab at the start of the line
                if (i == startLine) selectionStart += "\t".Length;  // Adjust selection start
            }

            // Reapply selection
            tbContent.Select(selectionStart, selectionLength);
        }

        private void tbContentCtxMenuIndentLeft_Click(object sender, RoutedEventArgs e)
        {
            // Get the selected text range
            int selectionStart = tbContent.SelectionStart;
            int selectionLength = tbContent.SelectionLength;

            // Get the start and end line indices
            int startLine = tbContent.GetLineIndexFromCharacterIndex(selectionStart);
            int endLine = tbContent.GetLineIndexFromCharacterIndex(selectionStart + selectionLength);

            // Modify the text for each affected line
            for (int i = startLine; i <= endLine; i++)
            {
                // Get the character index of the start of the line
                int lineStart = tbContent.GetCharacterIndexFromLineIndex(i);
                if (tbContent.Text.Substring(lineStart).StartsWith("\t"))
                {
                    tbContent.Text = tbContent.Text.Remove(lineStart, 1); // Remove the tab
                    if (i == startLine) selectionStart -= 1;  // Adjust selection start
                }
            }

            // Reapply selection
            tbContent.Select(selectionStart, selectionLength);
        }
    }
}
