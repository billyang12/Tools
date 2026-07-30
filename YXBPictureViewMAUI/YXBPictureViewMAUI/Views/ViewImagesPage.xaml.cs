using YXBPictureViewMAUI.Services;
using CommunityToolkit.Maui.Views;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace YXBPictureViewMAUI.Views;

public class FileItem : INotifyPropertyChanged
{
    public string FilePath { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public int Index { get; set; }

    private Color _textColor = Colors.Black;
    public Color TextColor
    {
        get => _textColor;
        set
        {
            _textColor = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(TextColor)));
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;
}

public partial class ViewImagesPage : ContentPage
{
    private string currentPassword = string.Empty;
    private string currentFolderPath = string.Empty;
    private List<string> mediaFiles = new List<string>();
    private ObservableCollection<FileItem> fileItems = new ObservableCollection<FileItem>();
    private int currentMediaIndex = -1;
    private string? currentTempVideoFile = null;
    private bool isSettingsPanelExpanded = true;
    private bool isFileListExpanded = true;

    public ViewImagesPage()
    {
        InitializeComponent();
        fileListView.ItemsSource = fileItems;

        // Add event handlers for video player
        mediaPlayer.MediaOpened += (s, e) => System.Diagnostics.Debug.WriteLine("MediaPlayer: MediaOpened event fired");
        mediaPlayer.MediaFailed += (s, e) => System.Diagnostics.Debug.WriteLine($"MediaPlayer: MediaFailed - {e.ErrorMessage}");
        mediaPlayer.MediaEnded += (s, e) => System.Diagnostics.Debug.WriteLine("MediaPlayer: MediaEnded event fired");
        mediaPlayer.StateChanged += (s, e) => System.Diagnostics.Debug.WriteLine($"MediaPlayer: State changed to {mediaPlayer.CurrentState}");
    }

    private void OnToggleSettingsPanel(object? sender, EventArgs e)
    {
        isSettingsPanelExpanded = !isSettingsPanelExpanded;
        settingsContent.IsVisible = isSettingsPanelExpanded;
        toggleIcon.Text = isSettingsPanelExpanded ? "▼" : "▶";
    }

    private void OnToggleFileList(object? sender, EventArgs e)
    {
        isFileListExpanded = !isFileListExpanded;
        fileListPanel.IsVisible = isFileListExpanded;
        fileListToggleIcon.Text = isFileListExpanded ? "▶" : "◀";

        // Adjust grid column widths
        // Column 0 = main content, Column 1 = toggle button (35px), Column 2 = file list
        if (isFileListExpanded)
        {
            // Show file list: file list takes 250px
            mainGrid.ColumnDefinitions[2].Width = new GridLength(250, GridUnitType.Absolute);
        }
        else
        {
            // Hide file list: file list column becomes 0
            mainGrid.ColumnDefinitions[2].Width = new GridLength(0, GridUnitType.Absolute);
        }
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

    private async void OnSelectFolder(object? sender, EventArgs e)
    {
        try
        {
            var result = await FolderPicker.PickAsync(default);

            if (result != null && result.Folder != null)
            {
                currentFolderPath = result.Folder.Path;
                System.Diagnostics.Debug.WriteLine($"Selected folder path: {currentFolderPath}");

                // Show the path to help with debugging
                if (currentFolderPath.Length > 50)
                {
                    lblImageInfo.Text = $"Loading from: ...{currentFolderPath.Substring(currentFolderPath.Length - 50)}";
                }
                else
                {
                    lblImageInfo.Text = $"Loading from: {currentFolderPath}";
                }

                await LoadXPGFilesAsync();
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("Folder picker returned null");
                await DisplayAlert("Info", "No folder selected", "OK");
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Folder picker error: {ex}");
            await DisplayAlert("Error", $"Failed to select folder: {ex.Message}", "OK");
        }
    }

    private async Task LoadXPGFilesAsync()
    {
        try
        {
            mediaFiles.Clear();
            fileItems.Clear();
            currentMediaIndex = -1;

            System.Diagnostics.Debug.WriteLine($"Loading files from: {currentFolderPath}");

            // Load all supported media file types
            var supportedExtensions = new[] {
                "*.xpg", "*.xpv",  // Encrypted
                "*.jpg", "*.jpeg", "*.png", "*.gif", "*.bmp", "*.webp",  // Images
                "*.mp4", "*.avi", "*.mov", "*.wmv", "*.mkv", "*.mpeg", "*.mpg"  // Videos
            };

            var allFiles = new List<string>();
            foreach (var pattern in supportedExtensions)
            {
                try
                {
                    var files = FileSystemHelper.GetFiles(currentFolderPath, pattern, SearchOption.AllDirectories);
                    allFiles.AddRange(files);
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error loading {pattern}: {ex.Message}");
                }
            }

            System.Diagnostics.Debug.WriteLine($"Found {allFiles.Count} total media files");

            mediaFiles = allFiles.OrderBy(f => FileSystemHelper.GetFileName(f)).ToList();

            // Populate file list
            for (int i = 0; i < mediaFiles.Count; i++)
            {
                fileItems.Add(new FileItem
                {
                    FilePath = mediaFiles[i],
                    DisplayName = FileSystemHelper.GetFileName(mediaFiles[i]),
                    Index = i
                });
            }

            if (mediaFiles.Count > 0)
            {
                System.Diagnostics.Debug.WriteLine($"Total media files: {mediaFiles.Count}, first file: {mediaFiles[0]}");
                currentMediaIndex = 0;
                UpdateFileListSelection();
                await LoadCurrentMediaAsync();
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("No media files found");
                lblImageInfo.Text = "No media files found in folder";
                imgDisplay.Source = null;
                mediaPlayer.IsVisible = false;
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"LoadXPGFilesAsync error: {ex}");
            await DisplayAlert("Error", $"Failed to load files: {ex.Message}", "OK");
        }
    }

    private async Task LoadCurrentMediaAsync()
    {
        if (currentMediaIndex < 0 || currentMediaIndex >= mediaFiles.Count)
            return;

        try
        {
            loadingIndicator.IsRunning = true;
            imgDisplay.Source = null;
            mediaPlayer.Source = null;

            string filePath = mediaFiles[currentMediaIndex];
            string extension = FileSystemHelper.GetExtension(filePath).ToLower();
            string fileName = FileSystemHelper.GetFileName(filePath);

            // Determine if this is an encrypted file
            bool isEncrypted = extension == ".xpg" || extension == ".xpv";

            // Check if password is required
            if (isEncrypted && string.IsNullOrWhiteSpace(currentPassword))
            {
                lblImageInfo.Text = $"{currentMediaIndex + 1} / {mediaFiles.Count}: {fileName} (Password required)";
                await DisplayAlert("Password Required", "Please enter your XPG password to view encrypted files.", "OK");
                loadingIndicator.IsRunning = false;
                return;
            }

            // Determine if this is a video file
            bool isVideo = extension == ".xpv" || extension == ".mp4" || extension == ".avi" ||
                          extension == ".mov" || extension == ".wmv" || extension == ".mkv" ||
                          extension == ".mpeg" || extension == ".mpg";

            // For large encrypted video files, warn the user
            if (extension == ".xpv")
            {
                long fileSize = await Task.Run(() => GetFileSize(filePath));
                double fileSizeMB = fileSize / 1024.0 / 1024.0;

                if (fileSizeMB > 50)
                {
                    bool proceed = await DisplayAlert("Large Video File",
                        $"This video is {fileSizeMB:F1}MB and may take several minutes to decrypt.\n\n" +
                        $"Continue?",
                        "Yes", "Cancel");

                    if (!proceed)
                    {
                        loadingIndicator.IsRunning = false;
                        return;
                    }
                }
            }

            byte[]? mediaData = null;

            if (isEncrypted)
            {
                // Encrypted file - decrypt it
                lblImageInfo.Text = $"{currentMediaIndex + 1} / {mediaFiles.Count}: Decrypting {fileName}...";

                // Move decryption to background thread to avoid UI freezing
                mediaData = await Task.Run(async () =>
                {
                    return await XPGEncryption.DecryptFileAsync(filePath, currentPassword);
                });

                if (mediaData == null)
                {
                    await DisplayAlert("Decryption Failed",
                        $"Failed to decrypt '{fileName}'.\n\nPossible causes:\n" +
                        $"- Wrong password/key\n" +
                        $"- File was encrypted with different key\n" +
                        $"- File is corrupted", "OK");
                    loadingIndicator.IsRunning = false;
                    return;
                }

                System.Diagnostics.Debug.WriteLine($"Successfully decrypted {mediaData.Length} bytes");
            }
            else
            {
                // Unencrypted file - read it directly
                lblImageInfo.Text = $"{currentMediaIndex + 1} / {mediaFiles.Count}: Loading {fileName}...";

                mediaData = await Task.Run(async () =>
                {
                    return await FileSystemHelper.ReadAllBytesAsync(filePath);
                });

                if (mediaData == null)
                {
                    await DisplayAlert("Error", $"Failed to read file '{fileName}'.", "OK");
                    loadingIndicator.IsRunning = false;
                    return;
                }

                System.Diagnostics.Debug.WriteLine($"Successfully loaded {mediaData.Length} bytes");
            }

            lblImageInfo.Text = $"{currentMediaIndex + 1} / {mediaFiles.Count}: {fileName}";

            // Display/play the media
            if (isVideo)
            {
                // Video file - save to temp and play
                CleanupTempVideoFile();

                currentTempVideoFile = Path.Combine(FileSystem.CacheDirectory,
                    Guid.NewGuid().ToString() + ".mp4");

                System.Diagnostics.Debug.WriteLine($"Writing {mediaData.Length} bytes to: {currentTempVideoFile}");
                await File.WriteAllBytesAsync(currentTempVideoFile, mediaData);

                // Show video player, hide image viewer
                imgDisplay.IsVisible = false;
                mediaPlayer.IsVisible = true;

                // Reset and configure the media player
                mediaPlayer.ShouldAutoPlay = true;
                mediaPlayer.ShouldShowPlaybackControls = true;

                // Set source and play
                System.Diagnostics.Debug.WriteLine($"Setting video source to: {currentTempVideoFile}");
                mediaPlayer.Source = MediaSource.FromFile(currentTempVideoFile);

                await Task.Delay(100);
                System.Diagnostics.Debug.WriteLine($"Video player state - IsVisible: {mediaPlayer.IsVisible}, Source: {mediaPlayer.Source}");
            }
            else
            {
                // Image file - display
                var stream = new MemoryStream(mediaData);

                // Show image viewer, hide video player
                mediaPlayer.IsVisible = false;
                imgDisplay.IsVisible = true;
                imgDisplay.Source = ImageSource.FromStream(() => stream);
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"LoadCurrentMediaAsync error: {ex}");
            await DisplayAlert("Error", $"Failed to load media: {ex.Message}", "OK");
        }
        finally
        {
            loadingIndicator.IsRunning = false;
        }
    }

    private void CleanupTempVideoFile()
    {
        if (!string.IsNullOrEmpty(currentTempVideoFile) && File.Exists(currentTempVideoFile))
        {
            try
            {
                // Securely delete: overwrite with random data before deletion
                SecureDeleteFile(currentTempVideoFile);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error deleting temp video file: {ex.Message}");
            }
        }
        currentTempVideoFile = null;
    }

    private void SecureDeleteFile(string filePath)
    {
        try
        {
            if (string.IsNullOrEmpty(filePath) || !File.Exists(filePath))
                return;

            // For very large files, just delete directly (overwriting can cause issues)
            var fileInfo = new FileInfo(filePath);
            long fileSize = fileInfo.Length;

            // Only do secure overwrite for files < 100MB
            if (fileSize < 100 * 1024 * 1024)
            {
                // Overwrite with random data (makes recovery harder)
                using (var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Write))
                {
                    byte[] randomData = new byte[Math.Min(fileSize, 1024 * 1024)]; // 1MB chunks
                    var random = new Random();

                    for (long written = 0; written < fileSize; written += randomData.Length)
                    {
                        int bytesToWrite = (int)Math.Min(randomData.Length, fileSize - written);
                        random.NextBytes(randomData);
                        fileStream.Write(randomData, 0, bytesToWrite);
                    }
                    fileStream.Flush();
                }
            }

            // Delete the file
            File.Delete(filePath);
            System.Diagnostics.Debug.WriteLine($"Securely deleted temp file: {filePath}");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Secure delete failed, attempting regular delete: {ex.Message}");
            try
            {
                if (!string.IsNullOrEmpty(filePath) && File.Exists(filePath))
                {
                    File.Delete(filePath);
                }
            }
            catch { }
        }
    }

    private void UpdateFileListSelection()
    {
        // Reset all colors
        foreach (var item in fileItems)
        {
            item.TextColor = Colors.Black;
        }

        // Highlight current item
        if (currentMediaIndex >= 0 && currentMediaIndex < fileItems.Count)
        {
            fileItems[currentMediaIndex].TextColor = Colors.Blue;
            fileListView.ScrollTo(fileItems[currentMediaIndex], position: ScrollToPosition.MakeVisible, animate: true);
        }
    }

    private async void OnFileSelected(object? sender, SelectionChangedEventArgs e)
    {
        if (e.CurrentSelection.Count > 0 && e.CurrentSelection[0] is FileItem selectedItem)
        {
            currentMediaIndex = selectedItem.Index;
            UpdateFileListSelection();
            await LoadCurrentMediaAsync();
        }
    }

    private async void OnFileItemTapped(object? sender, EventArgs e)
    {
        if (sender is Grid grid && grid.BindingContext is FileItem fileItem)
        {
            currentMediaIndex = fileItem.Index;
            UpdateFileListSelection();
            await LoadCurrentMediaAsync();
        }
    }

    private async void OnPreviousImage(object? sender, EventArgs e)
    {
        if (mediaFiles.Count == 0) return;

        currentMediaIndex--;
        if (currentMediaIndex < 0)
            currentMediaIndex = mediaFiles.Count - 1;

        UpdateFileListSelection();
        await LoadCurrentMediaAsync();
    }

    private async void OnNextImage(object? sender, EventArgs e)
    {
        if (mediaFiles.Count == 0) return;

        currentMediaIndex++;
        if (currentMediaIndex >= mediaFiles.Count)
            currentMediaIndex = 0;

        UpdateFileListSelection();
        await LoadCurrentMediaAsync();
    }

    private long GetFileSize(string path)
    {
#if ANDROID
        if (path.StartsWith("content://"))
        {
            try
            {
                var uri = Android.Net.Uri.Parse(path);
                var context = Platform.CurrentActivity;
                if (context?.ContentResolver != null && uri != null)
                {
                    var docFile = AndroidX.DocumentFile.Provider.DocumentFile.FromSingleUri(context, uri);
                    return docFile?.Length() ?? 0;
                }
            }
            catch
            {
                return 0;
            }
        }
#endif
        try
        {
            return new FileInfo(path).Length;
        }
        catch
        {
            return 0;
        }
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        // Clean up any leftover temp files from previous sessions (run in background)
        Task.Run(() => CleanupAllTempVideoFiles());
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        // Immediately clean up when leaving the page
        CleanupTempVideoFile();
    }

    private void CleanupAllTempVideoFiles()
    {
        try
        {
            // Clean up all .mp4 files in cache directory (our temp videos)
            var cacheDir = FileSystem.CacheDirectory;
            var tempFiles = Directory.GetFiles(cacheDir, "*.mp4");

            foreach (var file in tempFiles)
            {
                try
                {
                    SecureDeleteFile(file);
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Failed to clean up temp file {file}: {ex.Message}");
                }
            }

            System.Diagnostics.Debug.WriteLine($"Cleaned up {tempFiles.Length} temp video files");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error during temp file cleanup: {ex.Message}");
        }
    }
}
