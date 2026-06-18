using System.Windows;

namespace PdfEditorApp;

public partial class PageNumberDialog : Window
{
    public int PageNumber { get; private set; }
    public int TotalPages { get; set; }

    public PageNumberDialog()
    {
        InitializeComponent();
        Loaded += (s, e) => PageNumberBox.Focus();
    }

    private void OK_Click(object sender, RoutedEventArgs e)
    {
        if (!int.TryParse(PageNumberBox.Text, out int page))
        {
            System.Windows.MessageBox.Show("Please enter a valid number", "Invalid Input",
                MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        if (TotalPages > 0 && (page < 1 || page > TotalPages))
        {
            System.Windows.MessageBox.Show($"Please enter a page number between 1 and {TotalPages}", "Invalid Page Number",
                MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        PageNumber = page;
        DialogResult = true;
        Close();
    }

    private void Cancel_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
        Close();
    }

    public void SetMessage(string message)
    {
        MessageText.Text = message;
    }
}
