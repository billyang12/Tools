using System.Windows;

namespace PdfEditorApp;

public partial class TextInputDialog : System.Windows.Window
{
    public string TextContent => TextContentBox.Text;
    public new double FontSize { get; set; } = 12;

    public TextInputDialog()
    {
        InitializeComponent();
        TextContentBox.Focus();
    }

    private void OK_Click(object sender, RoutedEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(TextContentBox.Text))
        {
            System.Windows.MessageBox.Show("Please enter some text", "Validation",
                System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Warning);
            return;
        }

        DialogResult = true;
        Close();
    }

    private void Cancel_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
        Close();
    }
}
