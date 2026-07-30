using YXBPictureViewMAUI.Services;

namespace YXBPictureViewMAUI.Views;

public partial class EncryptPage : ContentPage
{
    private string sourceFolderPath = string.Empty;
    private string currentPassword = string.Empty;

    public EncryptPage()
    {
        InitializeComponent();
        pickerSourceExt.SelectedIndex = 0; // Default to jpg
        chkIncludeSubfolders.IsChecked = true;
        chkDeleteOriginal.IsChecked = false;
    }

    private void OnPasswordChanged(object? sender, TextChangedEventArgs e)
    {
        currentPassword = e.NewTextValue ?? string.Empty;

        // Show copy button when there's a key (not empty and not a simple password)
        btnCopyKey.IsVisible = !string.IsNullOrWhiteSpace(currentPassword) && currentPassword.Length > 20;
    }

    private async void OnGenerateKey(object? sender, EventArgs e)
    {
        string newKey = XPGEncryption.GenerateKey();
        txtPassword.Text = newKey;
        txtPassword.IsPassword = true; // Keep the generated key masked
        btnCopyKey.IsVisible = true; // Show copy button

        await DisplayAlert("Key Generated",
            "Random AES-256 key generated and masked.\n\nUse the 📋 button to copy it to clipboard.\n\nSAVE THIS KEY - you need it to decrypt!",
            "OK");
    }

    private async void OnDeriveKey(object? sender, EventArgs e)
    {
        // Create custom password confirmation dialog
        var passwordDialog = new PasswordConfirmDialog();
        passwordDialog.SetTitle("Derive Key");
        passwordDialog.SetMessage("Enter your password to derive an AES key:");

        // Show dialog in overlay container (always on top)
        dialogOverlay.Content = passwordDialog;
        dialogOverlay.IsVisible = true;

        // Wait for user input
        string? password = await passwordDialog.GetPasswordAsync();

        // Hide dialog
        dialogOverlay.IsVisible = false;
        dialogOverlay.Content = null;

        if (!string.IsNullOrEmpty(password))
        {
            string derivedKey = XPGEncryption.DeriveKeyFromPassword(password);
            txtPassword.Text = derivedKey;
            txtPassword.IsPassword = true; // Keep the key masked
            btnCopyKey.IsVisible = true; // Show copy button

            await DisplayAlert("Key Derived",
                "Key derived from your password and masked.\n\nUse the 📋 button to copy it to clipboard.\n\nRemember this password to regenerate the same key anytime!",
                "OK");
        }
    }

    private async void OnCopyKey(object? sender, EventArgs e)
    {
        if (!string.IsNullOrWhiteSpace(currentPassword))
        {
            // Copy to clipboard
            await Clipboard.SetTextAsync(currentPassword);

            // Show confirmation dialog
            await DisplayAlert("Copied", "Key is copied to clipboard.", "OK");

            // Clear clipboard for security
            await Clipboard.SetTextAsync(string.Empty);
        }
    }

    private async void OnSelectSourceFolder(object? sender, EventArgs e)
    {
        try
        {
            var result = await FolderPicker.PickAsync(default);

            if (result != null && result.Folder != null)
            {
                sourceFolderPath = result.Folder.Path;
                lblSourceFolder.Text = sourceFolderPath;
                lblSourceFolder.TextColor = Colors.Black;
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"Failed to select folder: {ex.Message}", "OK");
        }
    }

    private async void OnEncrypt(object? sender, EventArgs e)
    {
        // Validate inputs
        if (string.IsNullOrWhiteSpace(txtPassword.Text))
        {
            await DisplayAlert("Password Required", "Please enter or generate a password first.", "OK");
            return;
        }

        if (string.IsNullOrWhiteSpace(sourceFolderPath))
        {
            await DisplayAlert("Folder Required", "Please select a source folder.", "OK");
            return;
        }

        string sourceExt = pickerSourceExt.SelectedItem?.ToString() ?? "jpg";
        bool includeSubfolders = chkIncludeSubfolders.IsChecked;
        bool deleteOriginal = chkDeleteOriginal.IsChecked;

        // Confirm deletion warning
        if (deleteOriginal)
        {
            bool confirm = await DisplayAlert("Warning",
                "You selected to DELETE original files. This cannot be undone! Continue?",
                "Yes, Delete Originals", "No, Keep Originals");

            if (!confirm)
                return;
        }

        // Start encryption
        await EncryptFilesAsync(sourceExt, includeSubfolders, deleteOriginal);
    }

    private async Task EncryptFilesAsync(string sourceExt, bool includeSubfolders, bool deleteOriginal)
    {
        try
        {
            // Disable controls
            btnEncrypt.IsEnabled = false;
            frameProgress.IsVisible = true;

            // Find files
            SearchOption searchOption = includeSubfolders ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;
            var files = Directory.GetFiles(sourceFolderPath, $"*.{sourceExt}", searchOption).ToList();

            if (files.Count == 0)
            {
                await DisplayAlert("No Files", $"No .{sourceExt} files found in selected folder.", "OK");
                return;
            }

            lblProgress.Text = $"Encrypting {files.Count} files...";
            int processed = 0;
            int successful = 0;

            // Determine if encrypting videos
            bool isVideoFormat = IsVideoFormat(sourceExt);
            string targetExtension = isVideoFormat ? ".xpv" : ".xpg";

            foreach (var file in files)
            {
                try
                {
                    string outputFile = Path.Combine(
                        Path.GetDirectoryName(file) ?? string.Empty,
                        Path.GetFileNameWithoutExtension(file) + targetExtension
                    );

                    bool success = await XPGEncryption.EncryptFileAsync(file, outputFile, txtPassword.Text);

                    if (success)
                    {
                        successful++;

                        if (deleteOriginal)
                        {
                            File.Delete(file);
                        }
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error encrypting {file}: {ex.Message}");
                }

                processed++;
                progressBar.Progress = (double)processed / files.Count;
                lblProgressDetail.Text = $"{processed} / {files.Count}";
            }

            await DisplayAlert("Encryption Complete",
                $"Successfully encrypted {successful} out of {files.Count} files.",
                "OK");
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"Encryption failed: {ex.Message}", "OK");
        }
        finally
        {
            btnEncrypt.IsEnabled = true;
            frameProgress.IsVisible = false;
            progressBar.Progress = 0;
        }
    }

    private bool IsVideoFormat(string extension)
    {
        extension = extension.ToLower().TrimStart('.');
        string[] videoExtensions = { "mp4", "avi", "wmv", "mov", "mkv", "mpeg", "mpg" };
        return videoExtensions.Contains(extension);
    }
}
