namespace YXBPictureViewMAUI.Views;

public partial class PasswordPromptDialog : ContentView
{
    private TaskCompletionSource<string?> _taskCompletionSource;

    public PasswordPromptDialog()
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

    private void OnOk(object? sender, EventArgs e)
    {
        _taskCompletionSource.SetResult(txtPassword.Text);
    }

    private void OnCancel(object? sender, EventArgs e)
    {
        _taskCompletionSource.SetResult(null);
    }
}
