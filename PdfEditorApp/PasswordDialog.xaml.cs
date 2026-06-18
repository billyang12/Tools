using System.Windows;
using System.Windows.Input;

namespace PdfEditorApp;

public partial class PasswordDialog : Window
{
    public string Password { get; private set; } = "";

    public PasswordDialog()
    {
        InitializeComponent();
        PasswordBox.Focus();
    }

    private void OK_Click(object sender, RoutedEventArgs e)
    {
        Password = PasswordBox.Password;
        DialogResult = true;
        Close();
    }

    private void Cancel_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
        Close();
    }

    private void PasswordBox_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
    {
        if (e.Key == Key.Enter)
        {
            Password = PasswordBox.Password;
            DialogResult = true;
            Close();
        }
    }
}
