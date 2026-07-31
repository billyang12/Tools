using YXBPictureViewMAUI.Services;

namespace YXBPictureViewMAUI.Views;

public partial class DecryptPage : ContentPage
{
    private string sourceFolderPath = string.Empty;
    private string currentPassword = string.Empty;

    public DecryptPage()
    {
        InitializeComponent();
        pickerOutputExt.SelectedIndex = 0; // Default to jpg
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
            "Random AES-256 key generated and masked.\n\nUse the 📋 button to copy it to clipboard.",
            "OK");
    }

    private async void OnDeriveKey(object? sender, EventArgs e)
    {
        // Create custom password dialog
        var passwordDialog = new PasswordPromptDialog();
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
                "Key derived from your password and masked.\n\nUse the 📋 button to copy it to clipboard.\n\nRemember this password to regenerate the same key anytime.",
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

    private async void OnDecrypt(object? sender, EventArgs e)
    {
        // Validate inputs
        if (string.IsNullOrWhiteSpace(txtPassword.Text))
        {
            await DisplayAlert("Password Required", "Please enter the password/key used for encryption.", "OK");
            return;
        }

        if (string.IsNullOrWhiteSpace(sourceFolderPath))
        {
            await DisplayAlert("Folder Required", "Please select a source folder.", "OK");
        }

        string outputExt = pickerOutputExt.SelectedItem?.ToString() ?? "jpg";
        bool includeSubfolders = chkIncludeSubfolders.IsChecked;
        bool deleteOriginal = chkDeleteOriginal.IsChecked;

        // Confirm deletion warning
        if (deleteOriginal)
        {
            bool confirm = await DisplayAlert("Warning",
                "You selected to DELETE encrypted files after decryption. Continue?",
                "Yes, Delete Encrypted Files", "No, Keep Encrypted Files");

            if (!confirm)
                return;
        }

        // Start decryption
        await DecryptFilesAsync(outputExt, includeSubfolders, deleteOriginal);
    }

    private async Task DecryptFilesAsync(string outputExt, bool includeSubfolders, bool deleteOriginal)
    {
        try
        {
            // Disable controls
            btnDecrypt.IsEnabled = false;
            frameProgress.IsVisible = true;

            // Find XPG and XPV files using FileSystemHelper (supports Android content:// URIs)
            SearchOption searchOption = includeSubfolders ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;
            var xpgFiles = FileSystemHelper.GetFiles(sourceFolderPath, "*.xpg", searchOption);
            var xpvFiles = FileSystemHelper.GetFiles(sourceFolderPath, "*.xpv", searchOption);
            var files = xpgFiles.Concat(xpvFiles).ToList();

            if (files.Count == 0)
            {
                await DisplayAlert("No Files", "No .xpg/.xpv files found in selected folder.", "OK");
                return;
            }

            lblProgress.Text = $"Decrypting {files.Count} files...";
            int processed = 0;
            int successful = 0;

            foreach (var file in files)
            {
                try
                {
                    // Generate output file path
                    string fileName = FileSystemHelper.GetFileNameWithoutExtension(file);
                    string outputFile;

                    if (sourceFolderPath.StartsWith("content://"))
                    {
                        // For Android content URIs, output file goes to app's private storage
                        string cacheDir = FileSystem.AppDataDirectory;
                        string subfolder = Path.Combine(cacheDir, "decrypted");
                        Directory.CreateDirectory(subfolder);
                        outputFile = Path.Combine(subfolder, fileName + $".{outputExt}");
                    }
                    else
                    {
                        // For regular file paths
                        outputFile = Path.Combine(
                            Path.GetDirectoryName(file) ?? string.Empty,
                            fileName + $".{outputExt}"
                        );
                    }

                    byte[]? decryptedData = await XPGEncryption.DecryptFileAsync(file, txtPassword.Text);

                    if (decryptedData != null)
                    {
                        await File.WriteAllBytesAsync(outputFile, decryptedData);
                        successful++;

                        if (deleteOriginal && !file.StartsWith("content://"))
                        {
                            // Only delete original if it's not a content:// URI
                            File.Delete(file);
                        }
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error decrypting {file}: {ex.Message}");
                }

                processed++;
                progressBar.Progress = (double)processed / files.Count;
                lblProgressDetail.Text = $"{processed} / {files.Count}";
            }

            string message = $"Successfully decrypted {successful} out of {files.Count} files.";
            if (sourceFolderPath.StartsWith("content://"))
            {
                message += $"\n\nDecrypted files saved to:\n{Path.Combine(FileSystem.AppDataDirectory, "decrypted")}";
            }

            await DisplayAlert("Decryption Complete", message, "OK");
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"Decryption failed: {ex.Message}", "OK");
        }
        finally
        {
            btnDecrypt.IsEnabled = true;
            frameProgress.IsVisible = false;
            progressBar.Progress = 0;
        }
    }
}
