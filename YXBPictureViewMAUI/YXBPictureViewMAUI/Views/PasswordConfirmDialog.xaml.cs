namespace YXBPictureViewMAUI.Views;

public partial class PasswordConfirmDialog : ContentView
{
    private TaskCompletionSource<string?> _taskCompletionSource;

    public PasswordConfirmDialog()
    {
        InitializeComponent();
        _taskCompletionSource = new TaskCompletionSource<string?>();
    }

    public void SetTitle(string title)
    {
        lblTitle.Text = title;
    }

    public void SetMessage(string message)
    {
        lblMessage.Text = message;
    }

    public Task<string?> GetPasswordAsync()
    {
        // Focus the password entry when dialog appears
        txtPassword.Focus();
        return _taskCompletionSource.Task;
    }

    private async void OnOk(object? sender, EventArgs e)
    {
        string password = txtPassword.Text?.Trim() ?? string.Empty;
        string confirmPassword = txtConfirmPassword.Text?.Trim() ?? string.Empty;

        if (string.IsNullOrEmpty(password))
        {
            await Application.Current!.MainPage!.DisplayAlert("Error", "Password cannot be empty.", "OK");
            return;
        }

        if (password != confirmPassword)
        {
            await Application.Current!.MainPage!.DisplayAlert("Error", "Passwords do not match. Please try again.", "OK");
            txtPassword.Text = string.Empty;
            txtConfirmPassword.Text = string.Empty;
            txtPassword.Focus();
            return;
        }

        _taskCompletionSource.SetResult(password);
    }

    private void OnCancel(object? sender, EventArgs e)
    {
        _taskCompletionSource.SetResult(null);
    }
}
