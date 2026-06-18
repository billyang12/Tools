using System.Windows;

namespace PdfEditorApp;

public partial class AddTextDialog : System.Windows.Window
{
    public string TextContent => TextContentBox.Text;
    public float XPosition { get; private set; }
    public float YPosition { get; private set; }

    public AddTextDialog()
    {
        InitializeComponent();
    }

    private void OK_Click(object sender, RoutedEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(TextContentBox.Text))
        {
            System.Windows.MessageBox.Show("Please enter text", "Validation",
                System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Warning);
            return;
        }

        if (float.TryParse(XPositionBox.Text, out float x) &&
            float.TryParse(YPositionBox.Text, out float y))
        {
            XPosition = x;
            YPosition = y;
            DialogResult = true;
            Close();
        }
        else
        {
            System.Windows.MessageBox.Show("Please enter valid numeric positions", "Validation",
                System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Warning);
        }
    }

    private void Cancel_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
        Close();
    }
}
