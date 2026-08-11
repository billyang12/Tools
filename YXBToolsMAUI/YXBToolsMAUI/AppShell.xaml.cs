namespace YXBToolsMAUI
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();
        }

        private async void OnClearClipboardClicked(object sender, EventArgs e)
        {
            try
            {
                // Clear the clipboard
                await Clipboard.Default.SetTextAsync(string.Empty);

                // Show confirmation
                await DisplayAlert("Success", "Clipboard cleared successfully!", "OK");

                System.Diagnostics.Debug.WriteLine("Clipboard cleared");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error clearing clipboard: {ex.Message}");
                await DisplayAlert("Error", $"Failed to clear clipboard: {ex.Message}", "OK");
            }
        }

        private void OnExitClicked(object sender, EventArgs e)
        {
            Application.Current?.Quit();
        }
    }
}
