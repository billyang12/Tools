using System.Windows;

namespace PdfEditorApp;

public partial class ExportImageDialog : Window
{
    public ExportImageDialog()
    {
        InitializeComponent();
    }

    private void Export_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = true;
        Close();
    }

    private void Cancel_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
        Close();
    }

    public int GetDpi()
    {
        return DpiCombo.SelectedIndex switch
        {
            0 => 72,
            1 => 150,
            2 => 300,
            3 => 600,
            _ => 150
        };
    }

    public string GetExtension()
    {
        return FormatCombo.SelectedIndex switch
        {
            0 => ".png",
            1 => ".jpg",
            2 => ".bmp",
            _ => ".png"
        };
    }

    public string GetFileFilter()
    {
        return FormatCombo.SelectedIndex switch
        {
            0 => "PNG Image (*.png)|*.png",
            1 => "JPEG Image (*.jpg)|*.jpg",
            2 => "BMP Image (*.bmp)|*.bmp",
            _ => "PNG Image (*.png)|*.png"
        };
    }
}
