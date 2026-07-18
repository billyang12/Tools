using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace HierachicalNotes
{
    public partial class AllKeywordsDialog : Window
    {
        public AllKeywordsDialog()
        {
            InitializeComponent();
        }

        public void SetKeywords(List<string> keywords)
        {
            if (keywords == null || keywords.Count == 0)
            {
                tbKeywords.Text = "(No keywords found)";
                txtKeywordCount.Text = "Total: 0 keywords";
                return;
            }

            // Sort keywords alphabetically
            keywords.Sort(StringComparer.OrdinalIgnoreCase);

            // Display one keyword per line
            tbKeywords.Text = string.Join(Environment.NewLine, keywords);
            txtKeywordCount.Text = $"Total: {keywords.Count} keywords";
        }

        private void CopyButton_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(tbKeywords.Text))
            {
                Clipboard.SetText(tbKeywords.Text);
                MessageBox.Show("Keywords copied to clipboard!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
