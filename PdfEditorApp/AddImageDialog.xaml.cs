using System.Windows;

namespace PdfEditorApp;

public partial class AddImageDialog : System.Windows.Window
{
    public float XPosition { get; private set; }
    public float YPosition { get; private set; }
    public new float Width { get; private set; }
    public new float Height { get; private set; }

    public AddImageDialog()
    {
        InitializeComponent();
    }

    private void OK_Click(object sender, RoutedEventArgs e)
    {
        if (float.TryParse(XPositionBox.Text, out float x) &&
            float.TryParse(YPositionBox.Text, out float y) &&
            float.TryParse(WidthBox.Text, out float w) &&
            float.TryParse(HeightBox.Text, out float h))
        {
            XPosition = x;
            YPosition = y;
            Width = w;
            Height = h;
            DialogResult = true;
            Close();
        }
        else
        {
            System.Windows.MessageBox.Show("Please enter valid numeric values", "Validation",
                System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Warning);
        }
    }

    private void Cancel_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
        Close();
    }
}
