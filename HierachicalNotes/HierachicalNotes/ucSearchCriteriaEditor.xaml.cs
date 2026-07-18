using HierachicalNotes.Classes;
using HierachicalNotes.Interfaces;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace HierachicalNotes
{
    /// <summary>
    /// Interaction logic for ucSearchCriteriaEditor.xaml
    /// </summary>
    public partial class ucSearchCriteriaEditor : UserControl
    {
        public ucSearchCriteriaEditor()
        {
            InitializeComponent();
            tbSearchFromDate.Text = DateTime.Now.AddYears(-1).ToString("yyyy-MM-dd");
            tbSearchToDate.Text=DateTime.Now.ToString("yyyy-MM-dd");
        }
        public ISearchCriteria GetSearchCriteria()
        {
            SearchCriteria criteria = new SearchCriteria();
            ToSearchCriteria(criteria);
            return criteria;
        }
        public void ToSearchCriteria(ISearchCriteria criteria)
        {
            if (criteria is SearchCriteria)
            {
                SearchCriteria simple = criteria as SearchCriteria;
                simple.IsCaseSensitive = cbCaseSensitive.IsChecked == true;
                if (rbStrict.IsChecked == true) simple.SearchMethod = TextSearchMethodEnum.Strict;
                else if (rbAnd.IsChecked == true) simple.SearchMethod = TextSearchMethodEnum.And;
                else if (rbOr.IsChecked == true) simple.SearchMethod = TextSearchMethodEnum.Or;
                else if (rbKeywordsOnly.IsChecked == true) simple.SearchMethod = TextSearchMethodEnum.KeywordsOnly;
                else simple.SearchMethod = TextSearchMethodEnum.Strict;
                simple.Pattern = tbPattern.Text;
                simple.IsRegularExpression = cbRegularExpression.IsChecked == true;
                simple.SearchFromDateTime = SearchFromDate;
                simple.SearchToDateTime = SearchToDate;
            }
        }
        public void SetSearchCriteria(ISearchCriteria searchPattern)
        {
            SearchCriteria tmp = searchPattern as SearchCriteria;
            if (tmp == null)
            {
                throw new ArgumentException("The search criteria for this editor can only be of SimpleLineSearchCriteria");
            }
            cbCaseSensitive.IsChecked = tmp.IsCaseSensitive;
            switch (tmp.SearchMethod)
            {
                case TextSearchMethodEnum.Strict:
                    rbStrict.IsChecked = true;
                    break;
                case TextSearchMethodEnum.And:
                    rbAnd.IsChecked = true;
                    break;
                case TextSearchMethodEnum.Or:
                    rbOr.IsChecked = true;
                    break;
                case TextSearchMethodEnum.KeywordsOnly:
                    rbKeywordsOnly.IsChecked = true;
                    break;
            }
            tbPattern.Text = tmp.Pattern;
            cbRegularExpression.IsChecked = tmp.IsRegularExpression;
        }
        public bool IsSearchOnlyCurrentNode
        {
            get
            {
                return cbSearchCurrentNode.IsChecked == true;
            }
            set
            {
                cbSearchCurrentNode.IsChecked = value;
            }
        }

        public DateTime? SearchFromDate
        {
            get
            {
                DateTime? date = GetDateTimeFromUI(cbSearchFrom.IsChecked, tbSearchFromDate.Text);
                if (date.HasValue) date = new DateTime(date.Value.Year, date.Value.Month, date.Value.Day);
                return date;
            }
        }
        public DateTime? SearchToDate
        {
            get
            {
                DateTime? date = GetDateTimeFromUI(cbSearchTo.IsChecked, tbSearchToDate.Text);
                if (date.HasValue) date = new DateTime(date.Value.Year, date.Value.Month, date.Value.Day, 23, 59, 59);
                return date;
            }
        }
        private DateTime? GetDateTimeFromUI(bool? provided, string dateString)
        {
            if (provided != true) return null;
            DateTime date;
            bool success = DateTime.TryParse(dateString, out date);
            if (success)
            {
                return date;
            }
            else
            {
                return null;
            }
        }
        private void cbSearchFrom_Unchecked(object sender, RoutedEventArgs e)
        {
            tbSearchFromDate.IsEnabled = cbSearchFrom.IsChecked == true;
        }

        private void cbSearchFrom_Checked(object sender, RoutedEventArgs e)
        {
            tbSearchFromDate.IsEnabled = cbSearchFrom.IsChecked == true;
        }

        private void cbSearchTo_Checked(object sender, RoutedEventArgs e)
        {
            tbSearchToDate.IsEnabled = cbSearchTo.IsChecked == true;
        }

        private void cbSearchTo_Unchecked(object sender, RoutedEventArgs e)
        {
            tbSearchToDate.IsEnabled = cbSearchTo.IsChecked == true;
        }
    }
}
