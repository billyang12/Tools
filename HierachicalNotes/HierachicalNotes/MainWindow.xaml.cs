using HierachicalNotes.Classes;
using HierachicalNotes.Interfaces;
using HierarchicalNotes.Core.Services;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
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
using System.Xml;
using System.Xml.Linq;
using static System.Net.Mime.MediaTypeNames;

namespace HierachicalNotes
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        TreeNodeSearchResult? _searchResultWindow = null;
        Point _lastMouseDown;
        TreeViewItem? draggedItem, _target;
        const string _frmText = "HNode Editor";
        TreeViewItem? _currSelectedNode = null;
        CurrentFileInfo _currFileInfo = new CurrentFileInfo();
        bool _enableSpellCheck = false;
        RecentFilesManager _recentFilesManager = new RecentFilesManager();
        public MainWindow()
        {
            InitializeComponent();
            _currFileInfo._mainWindow = this;
            _currFileInfo._recentFilesManager = _recentFilesManager;
            //ClearSearchResult();
            SetFileInfo();
            _currFileInfo.NewFile();
            ToTreeView();
            Closing += MainWindow_Closing;
            UpdateRecentFilesMenu();
        }

        private void MainWindow_Closing(object? sender, System.ComponentModel.CancelEventArgs e)
        {
            FromNoteEditor();
            var hNodeCollection = new HNoteCollection();
            hNodeCollection.RootNodes = FromTreeNodeCollection(treeView1.Items);
            _currFileInfo.GatherCurrentNoteFileBeforeSave(hNodeCollection);

            if (_currFileInfo.CurrentNoteFile!=null && _currFileInfo.CurrentNoteFile.IsChangedSinceLastSave())
            {
                MessageBoxResult mr = MessageBox.Show("Notes content has changed, do you want to save it?", "Notice", MessageBoxButton.YesNoCancel);
                if (mr == MessageBoxResult.Yes)
                {
                    _currFileInfo.SaveFile(hNodeCollection, false, false);
                }
                else if (mr == MessageBoxResult.Cancel)
                {
                    e.Cancel = true;
                }
            }
        }

        void SetFileInfo()
        {
            Title = _currFileInfo.GetFileTitle(_frmText);
        }
        #region TreeNode
        void ToTreeView()
        {
            treeView1.Items.Clear();
            if (_currFileInfo.CurrentNoteFile == null) return;
            if (_currFileInfo.CurrentNoteFile.Notes == null) return;
            ToTreeNodeCollectionRecursive(treeView1.Items, _currFileInfo.CurrentNoteFile.Notes.RootNodes);
        }
        void ToTreeNodeCollectionRecursive(ItemCollection nodeCollection, List<HNote>? notes)
        {
            if (nodeCollection == null || notes == null) return;
            if (notes.Count <= 0) return;
            foreach (HNote subNote in notes)
            {
                TreeViewItem subNode = new TreeViewItem();
                nodeCollection.Add(subNode);
                ToTreeNodeRecursive(subNode, subNote);
            }
        }
        void ToTreeNodeRecursive(TreeViewItem treeNode, HNote note)
        {
            ToTreeNode(treeNode, note);
            ToTreeNodeCollectionRecursive(treeNode.Items, note.SubNodes);
        }
        void ToTreeNode(TreeViewItem treeNode, HNote note)
        {
            treeNode.Tag = note;
            treeNode.Header = note.BriefName;
        }
        List<HNote> FromTreeNodeCollection(ItemCollection? treeCollection)
        {
            List<HNote> notes = new List<HNote>();
            if (treeCollection == null) return notes;
            foreach (TreeViewItem node in treeCollection)
            {
                HNote? n = node.Tag as HNote;
                if (n != null)
                {
                    notes.Add(n);
                    n.SubNodes = FromTreeNodeCollection(node.Items);
                }
            }
            return notes;
        }
        #endregion
        TreeViewItem addNewNoteToCollection(ItemCollection collection, string name, string content = "")
        {
            TreeViewItem node = new TreeViewItem();
            HNote note = new HNote();
            note.Name = name;
            note.Content = content;
            note.CreateTime = DateTime.Now;
            note.UpdateTime = DateTime.Now;
            ToTreeNode(node, note);
            collection.Add(node);
            return node;
        }
        TreeViewItem insertNewNoteToCollectionBeforeCurrNode(TreeViewItem currItem, ItemCollection collection, string name, string content = "")
        {
            TreeViewItem node = new TreeViewItem();
            HNote note = new HNote();
            note.Name = name;
            note.Content = content;
            note.CreateTime = DateTime.Now;
            note.UpdateTime = DateTime.Now;
            ToTreeNode(node, note);
            int idx = collection.IndexOf(currItem);
            if (idx < 0) idx = 0;
            collection.Insert(idx, node);
            return node;
        }
        TreeViewItem insertNewNoteToCollectionAfterCurrNode(TreeViewItem currItem, ItemCollection collection, string name, string content = "")
        {
            TreeViewItem node = new TreeViewItem();
            HNote note = new HNote();
            note.Name = name;
            note.Content = content;
            note.CreateTime = DateTime.Now;
            note.UpdateTime = DateTime.Now;
            ToTreeNode(node, note);
            int idx = collection.IndexOf(currItem);
            if (idx < 0)
            {
                collection.Add(node);
            }
            else
            {
                collection.Insert(idx + 1, node);
            }
            return node;
        }
        TreeViewItem addDuplicateNoteToCollection(ItemCollection collection, HNote SourceNote)
        {
            TreeViewItem node = new TreeViewItem();
            HNote note = new HNote();
            note.ShallowCopyFrom(SourceNote);
            ToTreeNode(node, note);
            collection.Add(node);
            return node;
        }

        void reverseOrderOfItems(ItemCollection collection)
        {
            var items = collection.Cast<object>().ToList();
            collection.Clear();
            foreach (var item in items.AsEnumerable().Reverse())
            {
                collection.Add(item);
            }
        }

        private void _addNewNote(object sender, RoutedEventArgs e)
        {
            if (treeView1.Items.Count == 0)
            {
                SetSelectedNode(addNewNoteToCollection(treeView1.Items, ""));
                return;
            }
            TreeViewItem? n = treeView1.SelectedItem as TreeViewItem;
            if (n == null || n.Parent == null)
            {
                SetSelectedNode(addNewNoteToCollection(treeView1.Items, ""));
                return;
            }
            if (n.Parent is TreeView)
            {
                SetSelectedNode(addNewNoteToCollection((n.Parent as TreeView).Items, ""));
            }
            else
            {
                SetSelectedNode(addNewNoteToCollection((n.Parent as TreeViewItem).Items, ""));
            }
        }

        private void _addNewNoteBefore(object sender, RoutedEventArgs e)
        {
            if (treeView1.Items.Count == 0)
            {
                SetSelectedNode(addNewNoteToCollection(treeView1.Items, ""));
                return;
            }
            TreeViewItem? n = treeView1.SelectedItem as TreeViewItem;
            if (n == null || n.Parent == null)
            {
                SetSelectedNode(addNewNoteToCollection(treeView1.Items, ""));
                return;
            }
            if (n.Parent is TreeView)
            {
                SetSelectedNode(insertNewNoteToCollectionBeforeCurrNode(n, (n.Parent as TreeView).Items, ""));
            }
            else
            {
                SetSelectedNode(insertNewNoteToCollectionBeforeCurrNode(n, (n.Parent as TreeViewItem).Items, ""));
            }
        }

        private void _addNewNoteAfter(object sender, RoutedEventArgs e)
        {
            if (treeView1.Items.Count == 0)
            {
                SetSelectedNode(addNewNoteToCollection(treeView1.Items, ""));
                return;
            }
            TreeViewItem? n = treeView1.SelectedItem as TreeViewItem;
            if (n == null || n.Parent == null)
            {
                SetSelectedNode(addNewNoteToCollection(treeView1.Items, ""));
                return;
            }
            if (n.Parent is TreeView)
            {
                SetSelectedNode(insertNewNoteToCollectionAfterCurrNode(n, (n.Parent as TreeView).Items, ""));
            }
            else
            {
                SetSelectedNode(insertNewNoteToCollectionAfterCurrNode(n, (n.Parent as TreeViewItem).Items, ""));
            }
        }

        private void _duplicateNote(object sender, RoutedEventArgs e)
        {
            TreeViewItem? n = treeView1.SelectedItem as TreeViewItem;
            if (n == null || n.Parent == null || n.Tag as HNote == null)
            {
                return;
            }

            if (n.Parent is TreeView)
            {
                SetSelectedNode(addDuplicateNoteToCollection((n.Parent as TreeView).Items, n.Tag as HNote));
            }
            else
            {
                SetSelectedNode(addDuplicateNoteToCollection((n.Parent as TreeViewItem).Items, n.Tag as HNote));
            }
        }

        private void _reverseOrderOfChildren(object sender, RoutedEventArgs e)
        {
            TreeViewItem? n = treeView1.SelectedItem as TreeViewItem;
            if (n == null || n.Tag as HNote == null)
            {
                return;
            }
            reverseOrderOfItems(n.Items);
        }

        private void _addNewChildNote(object sender, RoutedEventArgs e)
        {
            if (treeView1.Items.Count == 0)
            {
                SetSelectedNode(addNewNoteToCollection(treeView1.Items, ""));
                return;
            }
            TreeViewItem? n = treeView1.SelectedItem as TreeViewItem;
            if (n == null || n.Parent == null)
            {
                SetSelectedNode(addNewNoteToCollection(treeView1.Items, ""));
                return;
            }
            SetSelectedNode(addNewNoteToCollection(n.Items, ""));
        }
        private void _removeNote(object sender, RoutedEventArgs e)
        {
            if (treeView1.Items.Count == 0)
            {
                return;
            }
            TreeViewItem? n = treeView1.SelectedItem as TreeViewItem;
            if (n == null || n.Parent == null)
            {
                return;
            }
            // if(n.Items.Count > 0)
            // {
            // MessageBoxResult mr = MessageBox.Show("The note has child notes, do you really want to remove it?", "Notice", MessageBoxButton.YesNo);
            MessageBoxResult mr = MessageBox.Show("Do you really want to remove it?", "Notice", MessageBoxButton.YesNo);
            if (mr != MessageBoxResult.Yes)
                {
                    return;
                }
            // }
            if (n.Parent is TreeView)
            {
                treeView1.Items.Remove(n);
            }
            else
            {
                (n.Parent as TreeViewItem)?.Items.Remove(n);
            }
        }
        private void _clearClipboard(object sender, RoutedEventArgs e)
        {
            Clipboard.Clear();
        }

        private void _copyNote(object sender, RoutedEventArgs e)
        {
            TreeViewItem? n = treeView1.SelectedItem as TreeViewItem;
            if (n == null || n.Parent == null || n.Tag as HNote == null)
            {
                return;
            }
            HNote? tmpNote=n.Tag as HNote;
            if (tmpNote == null) return;
            tmpNote.SubNodes = FromTreeNodeCollection(n.Items);
            string json = System.Text.Json.JsonSerializer.Serialize(tmpNote);
            Clipboard.SetText(json);
        }
        private void _pasteNote(object sender, RoutedEventArgs e)
        {
            TreeViewItem? n = treeView1.SelectedItem as TreeViewItem;
            if (n == null || n.Parent == null || n.Tag as HNote == null)
            {
                return;
            }
            string json = Clipboard.GetText();
            HNote? note = JsonSerializer.Deserialize<HNote>(json);
            if (note == null) return;
            _currSelectedNode = treeView1.SelectedItem as TreeViewItem;
            ToTreeNode(n, note);
            ucHNoteEditor1.Node = note;
            ToTreeNodeCollectionRecursive(n.Items, note.SubNodes);
            ToBrowser();
        }
        private void _pasteAsChildNote(object sender, RoutedEventArgs e)
        {
            TreeViewItem? n = treeView1.SelectedItem as TreeViewItem;
            if (n == null || n.Parent == null || n.Tag as HNote == null)
            {
                return;
            }
            string json = Clipboard.GetText();
            HNote? note = JsonSerializer.Deserialize<HNote>(json);
            if (note == null) return;
            TreeViewItem newItem = new TreeViewItem();
            ToTreeNode(newItem, note);
            ToTreeNodeCollectionRecursive(newItem.Items, note.SubNodes);
            n.Items.Add(newItem);
        }
        private void _ExitMenuItem_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void _AboutMenuItem_Click(object sender, RoutedEventArgs e)
        {
            AboutDialog aboutDialog = new AboutDialog();
            aboutDialog.Owner = this;
            aboutDialog.ShowDialog();
        }

        private void _AllKeywordsMenuItem_Click(object sender, RoutedEventArgs e)
        {
            List<string> allKeywords = CollectAllKeywords();
            AllKeywordsDialog dialog = new AllKeywordsDialog();
            dialog.Owner = this;
            dialog.SetKeywords(allKeywords);
            dialog.ShowDialog();
        }

        private List<string> CollectAllKeywords()
        {
            HashSet<string> keywordSet = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            CollectKeywordsRecursive(treeView1.Items, keywordSet);
            return keywordSet.ToList();
        }

        private void CollectKeywordsRecursive(ItemCollection collection, HashSet<string> keywordSet)
        {
            if (collection == null || collection.Count <= 0) return;

            foreach (TreeViewItem node in collection)
            {
                HNote? note = node.Tag as HNote;
                if (note != null && !string.IsNullOrWhiteSpace(note.Keywords))
                {
                    // Split keywords by comma and trim whitespace
                    string[] keywords = note.Keywords.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                    foreach (string keyword in keywords)
                    {
                        string trimmedKeyword = keyword.Trim();
                        if (!string.IsNullOrEmpty(trimmedKeyword))
                        {
                            keywordSet.Add(trimmedKeyword);
                        }
                    }
                }

                // Recursively process child nodes
                CollectKeywordsRecursive(node.Items, keywordSet);
            }
        }

        private void _NewMenuItem_Click(object sender, RoutedEventArgs e)
        {
            _currFileInfo.NewFile();
            ToTreeView();
            SetFileInfo();
            //ClearSearchResult();
        }

        private void _OpenMenuItem_Click(object sender, RoutedEventArgs e)
        {
            _currFileInfo.OpenFile();

            ToTreeView();
            if (treeView1.Items.Count > 0)
            {
              SetSelectedNode((treeView1.Items[0] as TreeViewItem));
            }
            SetFileInfo();
            UpdateRecentFilesMenu();
            //ClearSearchResult();
        }

        private void _SaveMenuItem_Click(object sender, RoutedEventArgs e)
        {
            FromNoteEditor();
            var hNodeCollection = new HNoteCollection();
            hNodeCollection.RootNodes = FromTreeNodeCollection(treeView1.Items);
            _currFileInfo.SaveFile(hNodeCollection);
            SetFileInfo();
        }
        private void _SaveAsMenuItem_Click(object sender, RoutedEventArgs e)
        {
            FromNoteEditor();
            var hNodeCollection = new HNoteCollection();
            hNodeCollection.RootNodes = FromTreeNodeCollection(treeView1.Items);
            _currFileInfo.SaveFile(hNodeCollection, true);
            SetFileInfo();
        }
        private void _SaveAsBackupMenuItem_Click(object sender, RoutedEventArgs e)
        {
            FromNoteEditor();
            var hNodeCollection = new HNoteCollection();
            hNodeCollection.RootNodes = FromTreeNodeCollection(treeView1.Items);
            _currFileInfo.SaveBackupFile(hNodeCollection);
            SetFileInfo();
        }
        private void _SetPasswordMenuItem_Click(object sender, RoutedEventArgs e)
        {
            _currFileInfo.SetPassword();
        }

        private void _ExportToHTML_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog saveFileDialog1 = new SaveFileDialog();
            saveFileDialog1.Filter = "HTML files|*.html|All files|*.*";

            bool? dr = saveFileDialog1.ShowDialog();
            if (dr == true)
            {
                System.IO.File.WriteAllText(saveFileDialog1.FileName, HTMLExporter.ExportToHTML(treeView1));
                MessageBox.Show("Exported successfully");
            }
        }

        private void TreeViewItem_PreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            TreeViewItem item = FindTreeViewItemUnderMouse(e.GetPosition(treeView1));//sender as TreeViewItem;
            if (item != null)
            {
                item.IsSelected = true;
                e.Handled = true; // Optionally, mark the event as handled to prevent further processing
            }
        }
        private TreeViewItem FindTreeViewItemUnderMouse(Point position)
        {
            HitTestResult result = VisualTreeHelper.HitTest(treeView1, position);

            if (result != null)
            {
                DependencyObject current = result.VisualHit;

                // Traverse up the visual tree to find the TreeViewItem
                while (current != null && !(current is TreeViewItem))
                {
                    current = VisualTreeHelper.GetParent(current);
                }

                return current as TreeViewItem;
            }

            return null;
        }

        private void _SetAllowSpellingCheckItem_Click(object sender, RoutedEventArgs e)
        {
            //MenuItem menuItem = sender as MenuItem;
            //if (menuItem != null)
            //{
            //    // Handle the menu item click event
            //    bool isChecked = menuItem.IsChecked;
            //    menuItem.IsChecked = !isChecked;
            //    _enableSpellCheck = menuItem.IsChecked;
            //}
        }

        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            CheckBox checkBox = sender as CheckBox;
            if(checkBox != null)
            {
                _enableSpellCheck = checkBox.IsChecked == true;

            }
            // _enableSpellCheck = _CheckBoxEnablingSpelling.IsChecked == true; 
        }

        private void _CheckBoxEnablingSpelling_Unchecked(object sender, RoutedEventArgs e)
        {
            CheckBox checkBox = sender as CheckBox;
            if (checkBox != null)
            {
                _enableSpellCheck = checkBox.IsChecked == true;

            }
        }
        void FromNoteEditor()
        {
            if (_currSelectedNode != null)
            {
                ucHNoteEditor1.SetSpellingCheck(_enableSpellCheck);
                HNote note = ucHNoteEditor1.Node;
                ToTreeNode(_currSelectedNode, note);
            }
        }

        private void treeView1_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            FromNoteEditor();
            _currSelectedNode = treeView1.SelectedItem as TreeViewItem;
            if (_currSelectedNode == null)
            {
                ucHNoteEditor1.Node = null;
                return;
            }
            HNote n = (HNote)_currSelectedNode.Tag;
            ucHNoteEditor1.Node = n;
            ToBrowser();
        }
        private void TreeView_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                _lastMouseDown = e.GetPosition(treeView1);
            }

        }
        private void treeView_MouseMove(object sender, MouseEventArgs e)
        {
            try
            {
                if (e.LeftButton == MouseButtonState.Pressed)
                {
                    Point currentPosition = e.GetPosition(treeView1);


                    if ((Math.Abs(currentPosition.X - _lastMouseDown.X) > 10.0) ||
                        (Math.Abs(currentPosition.Y - _lastMouseDown.Y) > 10.0))
                    {
                        draggedItem = (TreeViewItem)treeView1.SelectedItem;
                        if (draggedItem != null)
                        {
                            DragDropEffects finalDropEffect = DragDrop.DoDragDrop(treeView1, treeView1.SelectedValue,
                                DragDropEffects.Move);
                            //Checking target is not null and item is dragging(moving)
                            if ((finalDropEffect == DragDropEffects.Move) && (_target != null))
                            {
                                // A Move drop was accepted
                                //if (!draggedItem.Header.ToString().Equals(_target.Header.ToString()))
                                //{
                                if (draggedItem != _target)
                                {
                                    MoveItem(draggedItem, _target);
                                    //CopyItem(draggedItem, _target);
                                    _target = null;
                                    draggedItem = null;
                                }

                            }
                        }
                    }
                }
            }
            catch (Exception)
            {
            }
        }
        private void treeView_DragOver(object sender, DragEventArgs e)
        {
            try
            {

                Point currentPosition = e.GetPosition(treeView1);


                if ((Math.Abs(currentPosition.X - _lastMouseDown.X) > 10.0) ||
                    (Math.Abs(currentPosition.Y - _lastMouseDown.Y) > 10.0))
                {
                    // Verify that this is a valid drop and then store the drop target
                    TreeViewItem? item = GetNearestContainer(e.OriginalSource as UIElement);
                    if (CheckDropTarget(draggedItem, item))
                    {
                        e.Effects = DragDropEffects.Move;
                    }
                    else
                    {
                        e.Effects = DragDropEffects.None;
                    }
                }
                e.Handled = true;
            }
            catch (Exception)
            {
            }
        }
        private void treeView_Drop(object sender, DragEventArgs e)
        {
            try
            {
                e.Effects = DragDropEffects.None;
                e.Handled = true;

                // Verify that this is a valid drop and then store the drop target
                TreeViewItem? TargetItem = GetNearestContainer(e.OriginalSource as UIElement);
                if (TargetItem != null && draggedItem != null)
                {
                    _target = TargetItem;
                    e.Effects = DragDropEffects.Move;

                }
            }
            catch (Exception)
            {
            }
        }
        private bool CheckDropTarget(TreeViewItem? _sourceItem, TreeViewItem? _targetItem)
        {
            //Check whether the target item is meeting your condition
            bool _isEqual = false;
            //if (!_sourceItem.Header.ToString().Equals(_targetItem.Header.ToString()))
            if (_sourceItem != _targetItem)
            {
                _isEqual = true;
            }
            return _isEqual;

        }
        private void MoveItem(TreeViewItem _sourceItem, TreeViewItem _targetItem)
        {
            // if (_sourceItem.Items.Count > 0)
            // {
            // MessageBoxResult mr = MessageBox.Show("The note has child notes, do you really want to move it?", "Notice", MessageBoxButton.YesNo);
            MessageBoxResult mr = MessageBox.Show("Do you really want to move it?", "Notice", MessageBoxButton.YesNo);
            if (mr != MessageBoxResult.Yes)
                {
                    return;
                }
            // }
            if (rbAsChild.IsChecked == true)
            {
                MoveAsChild(_sourceItem, _targetItem);
            }
            else if (rbInsertBefore.IsChecked == true)
            {
                MoveBefore(_sourceItem, _targetItem);
            }
            else
            {
                MoveAfter(_sourceItem, _targetItem);
            }
        }
        void MoveBefore(TreeViewItem _sourceItem, TreeViewItem _targetItem)
        {
            if (_sourceItem == null || _targetItem == null) return;
            if (_sourceItem.Parent == null) return;
            if (_targetItem.Parent == null) return;
            if (_sourceItem.Parent is TreeView)
            {
                treeView1.Items.Remove(_sourceItem);
            }
            else if (_sourceItem.Parent is TreeViewItem)
            {
                (_sourceItem.Parent as TreeViewItem)?.Items.Remove(_sourceItem);
            }
            if (_targetItem.Parent is TreeView)
            {
                int i = treeView1.Items.IndexOf(_targetItem);
                treeView1.Items.Insert(i, _sourceItem);
            }
            else
            {
                TreeViewItem? p = _targetItem.Parent as TreeViewItem;
                int i = p.Items.IndexOf(_targetItem);
                p.Items.Insert(i, _sourceItem);
            }
            SetSelectedNode(_sourceItem);
        }
        void MoveAfter(TreeViewItem _sourceItem, TreeViewItem _targetItem)
        {
            if (_sourceItem == null || _targetItem == null) return;
            if (_sourceItem.Parent == null) return;
            if (_targetItem.Parent == null) return;
            if (_sourceItem.Parent is TreeView)
            {
                treeView1.Items.Remove(_sourceItem);
            }
            else if (_sourceItem.Parent is TreeViewItem)
            {
                (_sourceItem.Parent as TreeViewItem)?.Items.Remove(_sourceItem);
            }
            if (_targetItem.Parent is TreeView)
            {
                int i = treeView1.Items.IndexOf(_targetItem);
                treeView1.Items.Insert(i + 1, _sourceItem);
            }
            else
            {
                TreeViewItem? p = _targetItem.Parent as TreeViewItem;
                int i = p.Items.IndexOf(_targetItem);
                p.Items.Insert(i + 1, _sourceItem);
            }
            SetSelectedNode(_sourceItem);
        }
        void MoveAsChild(TreeViewItem _sourceItem, TreeViewItem _targetItem)
        {
            if (_sourceItem == null || _targetItem == null) return;
            if (_sourceItem.Parent == null) return;
            if (_sourceItem.Parent is TreeView)
            {
                treeView1.Items.Remove(_sourceItem);
            }
            else if (_sourceItem.Parent is TreeViewItem)
            {
                (_sourceItem.Parent as TreeViewItem)?.Items.Remove(_sourceItem);
            }
            _targetItem.Items.Add(_sourceItem);
            _targetItem.ExpandSubtree();
            SetSelectedNode(_sourceItem);
        }
        private void CopyItem(TreeViewItem _sourceItem, TreeViewItem _targetItem)
        {

            //Asking user wether he want to drop the dragged TreeViewItem here or not
            if (MessageBox.Show("Would you like to drop " + _sourceItem.Header.ToString() + " into " + _targetItem.Header.ToString() + "", "", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                try
                {
                    //adding dragged TreeViewItem in target TreeViewItem
                    addChild(_sourceItem, _targetItem);

                    //finding Parent TreeViewItem of dragged TreeViewItem 
                    TreeViewItem? ParentItem = FindVisualParent<TreeViewItem>(_sourceItem);
                    // if parent is null then remove from TreeView else remove from Parent TreeViewItem
                    if (ParentItem == null)
                    {
                        treeView1.Items.Remove(_sourceItem);
                    }
                    else
                    {
                        ParentItem.Items.Remove(_sourceItem);
                    }
                }
                catch
                {

                }
            }

        }
        public void addChild(TreeViewItem _sourceItem, TreeViewItem _targetItem)
        {
            // add item in target TreeViewItem 
            TreeViewItem item1 = new TreeViewItem();
            item1.Header = _sourceItem.Header;
            _targetItem.Items.Add(item1);
            foreach (TreeViewItem item in _sourceItem.Items)
            {
                addChild(item, item1);
            }
        }
        static TObject? FindVisualParent<TObject>(UIElement child) where TObject : UIElement
        {
            if (child == null)
            {
                return null;
            }

            UIElement? parent = VisualTreeHelper.GetParent(child) as UIElement;

            while (parent != null)
            {
                TObject? found = parent as TObject;
                if (found != null)
                {
                    return found;
                }
                else
                {
                    parent = VisualTreeHelper.GetParent(parent) as UIElement;
                }
            }

            return null;
        }
        private TreeViewItem? GetNearestContainer(UIElement? element)
        {
            // Walk up the element tree to the nearest tree view item.
            TreeViewItem? container = element as TreeViewItem;
            while ((container == null) && (element != null))
            {
                element = VisualTreeHelper.GetParent(element) as UIElement;
                container = element as TreeViewItem;
            }
            return container;
        }
        void ToBrowser()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine($"<h2>{ucHNoteEditor1.Node.Name}</h2>");
            sb.AppendLine($"<pre>{ucHNoteEditor1.Node.Content}</pre>");
            browser.NavigateToString(sb.ToString());
        }
        private void TabItem_Selected(object sender, RoutedEventArgs e)
        {
            FromNoteEditor();
            ToBrowser();
        }
        #region Search related
        private void bSearch_Click(object sender, RoutedEventArgs e)
        {

            List<TreeViewItem> result = new List<TreeViewItem>();
            ISearchCriteria criteria = ucSearchCriteriaEditor1.GetSearchCriteria();
            bool isSearchOnlyCurrentNode = ucSearchCriteriaEditor1.IsSearchOnlyCurrentNode;
            if (!isSearchOnlyCurrentNode)
            {
                SearchNotes(treeView1.Items, result, criteria);
            }
            else
            {
                if (_currSelectedNode != null)
                {
                    SearchNotes(_currSelectedNode.Items, result, criteria);
                }
                else
                {
                    SearchNotes(treeView1.Items, result, criteria);
                }
            }
            if (result.Count > 0)
            {
                List<YListItem> list = new List<YListItem>();
                foreach (TreeViewItem node in result)
                {
                    YListItem yl = new YListItem();
                    yl.Text = GetNodePath(node);
                    yl.Tag = node;
                    list.Add(yl);
                }

                ShowSearchResult(list);
            }
            else
            {
                ClearSearchResult();
            }
        }
        private void bClearSearchResult_Click(object sender, RoutedEventArgs e)
        {
            ClearSearchResult();
        }
        string GetNodePath(TreeViewItem? node)
        {
            if (node == null) return "";
            string path = node?.Header==null?"":node.Header.ToString(); //getNodeName(node);
            while (node.Parent != null && node.Parent is TreeViewItem)
            {
                node = node.Parent as TreeViewItem;
                path = getNodeName(node) + "\\" + path;
            }
            return path;
        }
        string getNodeName(TreeViewItem node)
        {
            HNote n = node.Tag as HNote;
            if (n == null) return node.Header.ToString();
            return n.BriefName;
        }

        void ClearSearchResult()
        {
            if (_searchResultWindow == null) return;
            _searchResultWindow.Close();
        }
        void ShowSearchResult(List<YListItem> list)
        {
            if (_searchResultWindow == null)
            {
                _searchResultWindow = new TreeNodeSearchResult();
            }
            _searchResultWindow.SetNodes(list, SetCurrentNode, () => { _searchResultWindow = null; });
            _searchResultWindow.Show();
            if (_searchResultWindow.WindowState == WindowState.Minimized)
                _searchResultWindow.WindowState = WindowState.Normal;
            //RestoreWindow.ShowWindowCheckingMinimized(_searchResultWindow);
        }
        void SearchNotes(ItemCollection collection, List<TreeViewItem> result, ISearchCriteria criteria)
        {
            if (collection == null || collection.Count <= 0) return;
            if (result == null) return;
            if (criteria == null) return;

            // Check if we're searching keywords only
            bool isKeywordsOnly = false;
            if (criteria is SearchCriteria searchCriteria)
            {
                isKeywordsOnly = searchCriteria.SearchMethod == TextSearchMethodEnum.KeywordsOnly;
            }

            foreach (TreeViewItem node in collection)
            {
                HNote note = node.Tag as HNote;
                if (note != null)
                {
                    bool hasMatch = false;
                    if (isKeywordsOnly)
                    {
                        // Only search in keywords field
                        hasMatch = criteria.HasMatches(note.Keywords, note.UpdateTime);
                    }
                    else
                    {
                        // Search in name and content as before
                        hasMatch = criteria.HasMatches(note.Name, note.UpdateTime) || criteria.HasMatches(note.Content, note.UpdateTime);
                    }

                    if (hasMatch)
                    {
                        result.Add(node);
                    }
                }
                SearchNotes(node.Items, result, criteria);
            }
        }
        void SetCurrentNode(YListItem item)
        {
            if (item == null) return;
            TreeViewItem node = item.Tag as TreeViewItem;
            if (node == null) return;
            node.BringIntoView();
            SetSelectedNode(node);
        }
        #endregion


        private void treeView1_ContextMenuOpening(object sender, ContextMenuEventArgs e)
        {
            TreeViewItem? n = treeView1.SelectedItem as TreeViewItem;
            if (n == null || n.Parent == null || n.Tag as HNote == null)
            {
                menuCopy.Visibility = Visibility.Hidden;
                menuPaste.Visibility = Visibility.Hidden;
                menuPasteAsChild.Visibility = Visibility.Hidden;
                return;
            }
            menuCopy.Visibility = Visibility.Visible;
            string json = Clipboard.GetText();
            HNote? note = null;
            try
            {
                note = JsonSerializer.Deserialize<HNote>(json);
            }
            catch
            {
                note = null;
            }
            bool hasNoteInClipboard = note != null;

            if(hasNoteInClipboard)
            {
                menuPaste.Visibility = Visibility.Visible;
                menuPasteAsChild.Visibility = Visibility.Visible;
            }
            else
            {
                menuPaste.Visibility = Visibility.Hidden;
                menuPasteAsChild.Visibility = Visibility.Hidden;
            }
        }

        void SetSelectedNode(TreeViewItem item)
        {
            if (item.Parent is TreeViewItem)
            {
                var p = item.Parent as TreeViewItem;
                while (p != null)
                {
                    if (!p.IsExpanded)
                    {
                        p.IsExpanded = true;
                    }
                    p = p.Parent as TreeViewItem;
                }
            }
            item.IsSelected = true;
        }

        void UpdateRecentFilesMenu()
        {
            menuRecentFiles.Items.Clear();

            List<string> recentFiles = _recentFilesManager.GetRecentFiles();

            if (recentFiles.Count == 0)
            {
                MenuItem noFilesItem = new MenuItem();
                noFilesItem.Header = "(No recent files)";
                noFilesItem.IsEnabled = false;
                menuRecentFiles.Items.Add(noFilesItem);
            }
            else
            {
                int index = 1;
                foreach (string filePath in recentFiles)
                {
                    MenuItem menuItem = new MenuItem();
                    menuItem.Header = $"_{index}. {System.IO.Path.GetFileName(filePath)}";
                    menuItem.Tag = filePath;
                    menuItem.ToolTip = filePath;
                    menuItem.Click += RecentFileMenuItem_Click;
                    menuRecentFiles.Items.Add(menuItem);
                    index++;
                }

                menuRecentFiles.Items.Add(new Separator());

                MenuItem clearItem = new MenuItem();
                clearItem.Header = "Clear Recent Files";
                clearItem.Click += ClearRecentFiles_Click;
                menuRecentFiles.Items.Add(clearItem);
            }
        }

        private void RecentFileMenuItem_Click(object sender, RoutedEventArgs e)
        {
            MenuItem menuItem = sender as MenuItem;
            if (menuItem != null && menuItem.Tag is string filePath)
            {
                _currFileInfo.OpenFile(filePath);
                ToTreeView();
                if (treeView1.Items.Count > 0)
                {
                    SetSelectedNode((treeView1.Items[0] as TreeViewItem));
                }
                SetFileInfo();
                UpdateRecentFilesMenu();
            }
        }

        private void ClearRecentFiles_Click(object sender, RoutedEventArgs e)
        {
            MessageBoxResult result = MessageBox.Show("Are you sure you want to clear the recent files list?", "Confirm", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (result == MessageBoxResult.Yes)
            {
                _recentFilesManager.ClearRecentFiles();
                UpdateRecentFilesMenu();
            }
        }
    }

    //public class TreeViewLineConverter : IValueConverter
    //{
    //    public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
    //    {
    //        TreeViewItem item = (TreeViewItem)value;
    //        ItemsControl ic = ItemsControl.ItemsControlFromItemContainer(item);
    //        return ic.ItemContainerGenerator.IndexFromContainer(item) == ic.Items.Count - 1;
    //    }

    //    public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
    //    {
    //        return false;
    //    }
    //}
    class CurrentFileInfo
    {
        public Window? _mainWindow { get; set; }
        public RecentFilesManager? _recentFilesManager { get; set; }
        string? _currPassword = null;
        const string _passwordSalt = "k!k(HJe@!H^&{";
        public string? CurrentFileName { get; set; } = null;
        public HNoteFile? CurrentNoteFile { get; set; } = null;

        public string GetFileTitle(string prefix)
        {
            string title = prefix;
            if (!string.IsNullOrEmpty(CurrentFileName))
            {
                title = title + " - " + CurrentFileName;
            }
            return title;
        }
        public string CurrentPassword
        {
            set
            {
                _currPassword = value;
            }
        }
        public bool IsCorrectPassword(string? pwdTocheck)
        {
            if (string.IsNullOrEmpty(pwdTocheck)) return false;
            if (pwdTocheck == _currPassword) return true;
            else return false;
        }
        public void NewFile()
        {
            CurrentFileName = null;
            CurrentNoteFile = new HNoteFile();
            _currPassword = null;
            CurrentNoteFile.Notes = new HNoteCollection();
            HNote n = new HNote();
            n.Name = "Note 1";
            n.Content = "This is the first note";
            n.CreateTime = DateTime.Now;
            n.UpdateTime = DateTime.Now;
            n.SubNodes = new List<HNote>();
            CurrentNoteFile.Notes.RootNodes.Add(n);
            CurrentNoteFile.SaveLatestNotesJson();
        }
        public void OpenFile()
        {
            OpenFileDialog openFileDialog1 = new OpenFileDialog();
            openFileDialog1.Filter = "HierachicalNotes files|*.hnf|All files|*.*";
            openFileDialog1.FileName = CurrentFileName;
            if (!string.IsNullOrEmpty(CurrentFileName))
            {
                string? path = System.IO.Path.GetDirectoryName(CurrentFileName);
                openFileDialog1.InitialDirectory = path;
            }
            bool? dr = openFileDialog1.ShowDialog();
            if (dr == true)
            {
                OpenFile(openFileDialog1.FileName);
            }
        }

        public void OpenFile(string fileName)
        {
            if (string.IsNullOrEmpty(fileName) || !System.IO.File.Exists(fileName))
            {
                MessageBox.Show("File does not exist.");
                return;
            }

            string? json = System.IO.File.ReadAllText(fileName);
            HNoteFile tmpNoteFile = JsonSerializer.Deserialize<HNoteFile>(json);
            if (tmpNoteFile.IsEncrypted)
            {
                HierachicalNotes.PasswordDialog dlg = new HierachicalNotes.PasswordDialog();
                dlg.DialogType = HierachicalNotes.PasswordDialog.PasswordDialogType.NeedPassword;
                dlg.Owner = _mainWindow;
                bool? result = dlg.ShowDialog();
                if (result == true)
                {
                    try
                    {
                        string tmpjson = StringCipher.DecryptToString(tmpNoteFile.HNoteJsonStr, dlg.Password, _passwordSalt);
                        tmpNoteFile.Notes = JsonSerializer.Deserialize<HNoteCollection>(tmpjson);
                        CurrentNoteFile = tmpNoteFile;
                        _currPassword = dlg.Password;
                        CurrentNoteFile.SaveLatestNotesJson();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message);
                        return;
                    }
                }
                else
                {
                    return;
                }
            }
            else
            {
                CurrentNoteFile = tmpNoteFile;
                CurrentNoteFile.SaveLatestNotesJson();
            }
            CurrentFileName = fileName;
            _recentFilesManager?.AddRecentFile(fileName);
        }

        public void GatherCurrentNoteFileBeforeSave(HNoteCollection hNoteCollection)
        {
            if (CurrentNoteFile == null)
            {
                CurrentNoteFile = new HNoteFile();
            }
            if (!CurrentNoteFile.IsEncrypted)
            {
                CurrentNoteFile.Notes = hNoteCollection;
                CurrentNoteFile.HNoteJsonStr = null;
            }
            else
            {
                CurrentNoteFile.Notes = null;
                CurrentNoteFile.HNoteJsonStr = StringCipher.Encrypt(JsonSerializer.Serialize(hNoteCollection), _currPassword, _passwordSalt);
            }
        }

        string GetCurrentNoteJSONString()
        {
            JsonSerializerOptions options = new JsonSerializerOptions();
            options.WriteIndented = true;
            string json = JsonSerializer.Serialize(CurrentNoteFile, options);
            return json;

        }
        public void SaveFile(HNoteCollection hNoteCollection, bool? saveAs = false, bool needShowDialog = true)
        {
            GatherCurrentNoteFileBeforeSave(hNoteCollection);
            string json = GetCurrentNoteJSONString();

            SaveFileDialog saveFileDialog1 = new SaveFileDialog();
            saveFileDialog1.FileName = CurrentFileName;
            saveFileDialog1.Filter = "HierachicalNotes files|*.hnf|All files|*.*";
            if (saveAs == false && !string.IsNullOrEmpty(CurrentFileName))
            {
                System.IO.File.WriteAllText(CurrentFileName, json);
                if (needShowDialog)
                {
                    MessageBox.Show("File saved");
                }
                CurrentNoteFile.SaveLatestNotesJson();
                _recentFilesManager?.AddRecentFile(CurrentFileName);
            }
            else
            {
                bool? dr = saveFileDialog1.ShowDialog();
                if (dr == true)
                {
                    System.IO.File.WriteAllText(saveFileDialog1.FileName, json);
                    CurrentFileName = saveFileDialog1.FileName;
                    if (needShowDialog)
                    {
                        MessageBox.Show("File saved");
                    }
                    CurrentNoteFile.SaveLatestNotesJson();
                    _recentFilesManager?.AddRecentFile(CurrentFileName);
                }
            }
        }
        public void SaveBackupFile(HNoteCollection hNoteCollection)
        {
            GatherCurrentNoteFileBeforeSave(hNoteCollection);

            string json = GetCurrentNoteJSONString();

            if (!string.IsNullOrEmpty(CurrentFileName))
            {
                string backFilePath = System.IO.Path.GetDirectoryName(CurrentFileName);
                string backFileName = System.IO.Path.GetFileName(CurrentFileName);
                DateTime dt = DateTime.Now;
                string backName = $"Backup{dt.Year.ToString()}-{dt.Month.ToString("00")}-{dt.Day.ToString("00")}-{dt.Hour.ToString("00")}{dt.Minute.ToString("00")}{dt.Second.ToString("00")} {backFileName}";
                backName = System.IO.Path.Combine(backFilePath, backName);
                System.IO.File.WriteAllText(backName, json);
                MessageBox.Show("Backup File saved");
            }
            else
            {
                MessageBox.Show("Current notes have not been saved. \r\nPplease use 'Save' or 'Save As' to save it first");
            }
        }
        public void SetPassword()
        {
            PasswordDialog dlg = new PasswordDialog();
            dlg.Owner = _mainWindow;
            if (String.IsNullOrEmpty(_currPassword))
            {
                dlg.DialogType = PasswordDialog.PasswordDialogType.SetNewPassword;
            }
            else
            {
                dlg.DialogType = PasswordDialog.PasswordDialogType.UpdatePassword;
            }
            bool? result = dlg.ShowDialog();
            if (result == true)
            {
                if (dlg.DialogType == PasswordDialog.PasswordDialogType.UpdatePassword)
                {
                    if (dlg.OldPassword != _currPassword)
                    {
                        MessageBox.Show("Old password is not correct, you cannot change the password");
                        return;
                    }
                }
                if (CurrentNoteFile == null)
                {
                    CurrentNoteFile = new HNoteFile();
                }
                CurrentNoteFile.IsEncrypted = true;
                _currPassword = dlg.NewPassword;
                MessageBox.Show("Password set successfully\r\n\r\nNote: the file password is not changed until you save it successfully");
            }
        }
    }
}

