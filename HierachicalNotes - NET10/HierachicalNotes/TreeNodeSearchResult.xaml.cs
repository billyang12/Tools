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
    /// Interaction logic for TreeNodeSearchResult.xaml
    /// </summary>
    public partial class TreeNodeSearchResult : Window
    {
        public TreeNodeSearchResult()
        {
            InitializeComponent();
        }
        public delegate void ActionOnWindowClosedDelegate();
        public delegate void ActionOnSelectedDelegate(YListItem item);
        ActionOnWindowClosedDelegate? _actionOnWindowClosed = null;
        ActionOnSelectedDelegate? _actionOnSelected = null;
        public void SetNodes(List<YListItem> Nodes, ActionOnSelectedDelegate actionOnSelected, ActionOnWindowClosedDelegate actionOnWindowClosed)
        {
            _actionOnWindowClosed = actionOnWindowClosed;
            _actionOnSelected = actionOnSelected;
            lbItems.Items.Clear();
            if (Nodes == null || Nodes.Count <= 0) return;
            foreach (YListItem item in Nodes)
            {
                lbItems.Items.Add(item);
            }
        }
        private void Window_Closed(object sender, EventArgs e)
        {
            if (_actionOnWindowClosed != null)
            {
                _actionOnWindowClosed();
            }
        }

        private void lbItems_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_actionOnSelected == null) return;
            if (lbItems.SelectedIndex >= 0)
            {
                YListItem item = lbItems.Items[lbItems.SelectedIndex] as YListItem;
                if (item == null) return;
                _actionOnSelected(item);
            }
        }

        private void copyTextMenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (lbItems.SelectedIndex >= 0)
            {
                YListItem item = lbItems.Items[lbItems.SelectedIndex] as YListItem;
                if (item == null) return;
                Clipboard.SetText(item.ToString());
            }
        }
    }
}
