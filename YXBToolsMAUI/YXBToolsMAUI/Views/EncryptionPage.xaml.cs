using YXBToolsMAUI.Services;
using FolderPicker = YXBToolsMAUI.Services.FolderPicker;

namespace YXBToolsMAUI.Views;

public partial class EncryptionPage : ContentPage
{
    private readonly IEncryptionService _encryptionService;
    private string? _selectedSourcePath;
    private string? _selectedOutputPath;
    private bool _isFolder;
    private CancellationTokenSource? _cancellationTokenSource;

    public EncryptionPage(IEncryptionService encryptionService)
    {
        InitializeComponent();
        _encryptionService = encryptionService;
    }

    private async void OnSelectFileClicked(object sender, EventArgs e)
    {
        try
        {
            string? filePath = null;

#if WINDOWS
            var filePickerService = new YXBToolsMAUI.Platforms.Windows.FilePickerService();
            filePath = await filePickerService.PickFileAsync("Select a file to encrypt");
#else
            var result = await FilePicker.Default.PickAsync(new PickOptions
            {
                PickerTitle = "Select a file to encrypt"
            });
            filePath = result?.FullPath;

#if ANDROID
            if (!string.IsNullOrEmpty(filePath))
            {
                StatusLabel.Text = "Preparing file...";
                StatusLabel.TextColor = Colors.Blue;

                System.Diagnostics.Debug.WriteLine($"OnSelectFileClicked: File picked: {filePath}");

                var copiedPath = await YXBToolsMAUI.Platforms.Android.AndroidFileHandler.CopyFileToWorkingDirectoryAsync(filePath);

                if (string.IsNullOrEmpty(copiedPath))
                {
                    StatusLabel.Text = "Failed to prepare file";
                    StatusLabel.TextColor = Colors.Red;
                    await DisplayAlert("Error", "Failed to prepare the selected file. Please try again.", "OK");
                    return;
                }

                filePath = copiedPath;
                System.Diagnostics.Debug.WriteLine($"OnSelectFileClicked: File copied to: {filePath}");
                StatusLabel.Text = string.Empty;
            }
#endif
#endif

            if (!string.IsNullOrEmpty(filePath))
            {
                _selectedSourcePath = filePath;
                _isFolder = false;
                SelectedPathLabel.Text = $"File: {Path.GetFileName(filePath)}";
                SelectedPathLabel.TextColor = Colors.Green;
                StatusLabel.Text = string.Empty;
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("OnSelectFileClicked: No file selected");
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"File picker error: {ex.Message}");
            System.Diagnostics.Debug.WriteLine($"Stack trace: {ex.StackTrace}");
            await DisplayAlert("Error", $"Failed to select file:\n\n{ex.Message}\n\nType: {ex.GetType().Name}", "OK");
        }
    }

    private async void OnSelectFolderClicked(object sender, EventArgs e)
    {
        try
        {
            var result = await FolderPicker.PickAsync();

            if (result?.Folder != null && !string.IsNullOrEmpty(result.Folder.Path))
            {
                _selectedSourcePath = result.Folder.Path;
                _isFolder = true;
                SelectedPathLabel.Text = result.Folder.Path;
                SelectedPathLabel.TextColor = Colors.Green;
                StatusLabel.Text = string.Empty;
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Folder picker error: {ex.Message}");
            System.Diagnostics.Debug.WriteLine($"Stack trace: {ex.StackTrace}");
            await DisplayAlert("Error", $"Failed to select folder:\n\n{ex.Message}\n\nType: {ex.GetType().Name}", "OK");
        }
    }

    private async void OnSelectOutputClicked(object sender, EventArgs e)
    {
        try
        {
            System.Diagnostics.Debug.WriteLine("OnSelectOutputClicked: Starting folder picker");

            var result = await FolderPicker.PickAsync();

            System.Diagnostics.Debug.WriteLine($"OnSelectOutputClicked: Folder picker returned, result is null: {result == null}");

            if (result?.Folder != null && !string.IsNullOrEmpty(result.Folder.Path))
            {
                System.Diagnostics.Debug.WriteLine($"OnSelectOutputClicked: Selected path: {result.Folder.Path}");

                _selectedOutputPath = result.Folder.Path;
                OutputPathLabel.Text = _selectedOutputPath;
                OutputPathLabel.TextColor = Colors.Green;

                System.Diagnostics.Debug.WriteLine("OnSelectOutputClicked: UI updated successfully");
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("OnSelectOutputClicked: No folder selected or result is null");
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Output folder picker error: {ex.Message}");
            System.Diagnostics.Debug.WriteLine($"Stack trace: {ex.StackTrace}");
            System.Diagnostics.Debug.WriteLine($"Inner exception: {ex.InnerException?.Message}");
            await DisplayAlert("Error", $"Failed to select output folder:\n\n{ex.Message}\n\nType: {ex.GetType().Name}", "OK");
        }
    }

    private void OnShowPasswordChanged(object sender, CheckedChangedEventArgs e)
    {
        PasswordEntry.IsPassword = !e.Value;
        ConfirmPasswordEntry.IsPassword = !e.Value;
    }

    private async void OnEncryptClicked(object sender, EventArgs e)
    {
        if (string.IsNullOrEmpty(_selectedSourcePath))
        {
            await DisplayAlert("Error", "Please select a file or folder to encrypt", "OK");
            return;
        }

        if (string.IsNullOrEmpty(PasswordEntry.Text))
        {
            await DisplayAlert("Error", "Please enter a password", "OK");
            return;
        }

        if (PasswordEntry.Text != ConfirmPasswordEntry.Text)
        {
            await DisplayAlert("Error", "Passwords do not match", "OK");
            return;
        }

        if (string.IsNullOrEmpty(_selectedOutputPath))
        {
            await DisplayAlert("Error", "Please select an output folder", "OK");
            return;
        }

        System.Diagnostics.Debug.WriteLine($"=== ENCRYPTION DEBUG ===");
        System.Diagnostics.Debug.WriteLine($"Source: {_selectedSourcePath}");
        System.Diagnostics.Debug.WriteLine($"Output: {_selectedOutputPath}");
        System.Diagnostics.Debug.WriteLine($"IsFolder: {_isFolder}");

#if ANDROID
        bool isSourceContentUri = _selectedSourcePath.StartsWith("content://");
        bool isOutputContentUri = _selectedOutputPath.StartsWith("content://");

        // On Android, actualOutputPath is used for non-content URIs only
        string actualOutputPath = _selectedOutputPath;

        // Only check file/directory existence for non-content URIs
        if (!isSourceContentUri)
        {
            System.Diagnostics.Debug.WriteLine($"Source exists (file): {File.Exists(_selectedSourcePath)}");
            System.Diagnostics.Debug.WriteLine($"Source exists (folder): {Directory.Exists(_selectedSourcePath)}");
        }
        if (!isOutputContentUri)
        {
            System.Diagnostics.Debug.WriteLine($"Output exists: {Directory.Exists(_selectedOutputPath)}");
        }
#else
        System.Diagnostics.Debug.WriteLine($"Source exists (file): {File.Exists(_selectedSourcePath)}");
        System.Diagnostics.Debug.WriteLine($"Source exists (folder): {Directory.Exists(_selectedSourcePath)}");
        System.Diagnostics.Debug.WriteLine($"Output exists: {Directory.Exists(_selectedOutputPath)}");
        string actualOutputPath = _selectedOutputPath;
#endif
        System.Diagnostics.Debug.WriteLine($"======================");

        _cancellationTokenSource = new CancellationTokenSource();

        EncryptBtn.IsEnabled = false;
        StatusLabel.Text = "";
        ProgressFrame.IsVisible = true;
        ProgressBar.Progress = 0;
        ProgressLabel.Text = "Starting...";
        CurrentFileLabel.Text = "";

        try
        {
            bool success;

            if (_isFolder)
            {
#if ANDROID
                if (isSourceContentUri)
                {
                    // Enumerate files from content URI and encrypt each one
                    var fileList = YXBToolsMAUI.Platforms.Android.AndroidDocumentHelper.ListFilesInFolder(_selectedSourcePath);

                    if (fileList.Count == 0)
                    {
                        await DisplayAlert("No Files", "No files found in the selected folder.", "OK");
                        return;
                    }

                    int successCount = 0;
                    int totalFiles = fileList.Count;

                    for (int i = 0; i < fileList.Count; i++)
                    {
                        if (_cancellationTokenSource.Token.IsCancellationRequested)
                            break;

                        var (fileUri, fileName) = fileList[i];

                        MainThread.BeginInvokeOnMainThread(() =>
                        {
                            ProgressBar.Progress = (double)(i + 1) / totalFiles;
                            ProgressLabel.Text = $"{i + 1} / {totalFiles} files";
                            CurrentFileLabel.Text = $"Encrypting: {fileName}";
                        });

                        // Copy file from content URI to temp
                        var fileData = await YXBToolsMAUI.Platforms.Android.AndroidDocumentHelper.ReadFileFromUri(fileUri);
                        if (fileData == null) continue;

                        var tempInputPath = Path.Combine(FileSystem.CacheDirectory, fileName);
                        await File.WriteAllBytesAsync(tempInputPath, fileData);

                        // Encrypt to temp output
                        var outputFileName = fileName + ".yxbenc";
                        var tempOutputPath = Path.Combine(FileSystem.CacheDirectory, outputFileName);

                        bool encryptSuccess = await _encryptionService.EncryptFileAsync(tempInputPath, tempOutputPath, PasswordEntry.Text, _cancellationTokenSource.Token);

                        if (encryptSuccess)
                        {
                            // Write to output folder (content URI or regular path)
                            if (isOutputContentUri)
                            {
                                var encryptedData = await File.ReadAllBytesAsync(tempOutputPath);
                                bool writeSuccess = await YXBToolsMAUI.Platforms.Android.AndroidDocumentHelper.WriteFileToDocumentUri(
                                    _selectedOutputPath, outputFileName, encryptedData);

                                if (writeSuccess) successCount++;
                            }
                            else
                            {
                                var finalOutputPath = Path.Combine(actualOutputPath, outputFileName);
                                File.Copy(tempOutputPath, finalOutputPath, true);
                                successCount++;
                            }
                        }

                        // Clean up temp files
                        if (File.Exists(tempInputPath)) File.Delete(tempInputPath);
                        if (File.Exists(tempOutputPath)) File.Delete(tempOutputPath);
                    }

                    success = successCount > 0;

                    if (successCount < totalFiles)
                    {
                        await DisplayAlert("Partial Success", $"Encrypted {successCount} out of {totalFiles} files.", "OK");
                    }
                }
                else
                {
                    var progress = new Progress<(int current, int total, string fileName)>(update =>
                    {
                        MainThread.BeginInvokeOnMainThread(() =>
                        {
                            ProgressBar.Progress = (double)update.current / update.total;
                            ProgressLabel.Text = $"{update.current} / {update.total} files";
                            CurrentFileLabel.Text = $"Encrypting: {update.fileName}";
                        });
                    });

                    success = await _encryptionService.EncryptFolderAsync(_selectedSourcePath, actualOutputPath, PasswordEntry.Text, progress, _cancellationTokenSource.Token);
                }
#else
                var progress = new Progress<(int current, int total, string fileName)>(update =>
                {
                    MainThread.BeginInvokeOnMainThread(() =>
                    {
                        ProgressBar.Progress = (double)update.current / update.total;
                        ProgressLabel.Text = $"{update.current} / {update.total} files";
                        CurrentFileLabel.Text = $"Encrypting: {update.fileName}";
                    });
                });

                success = await _encryptionService.EncryptFolderAsync(_selectedSourcePath, actualOutputPath, PasswordEntry.Text, progress, _cancellationTokenSource.Token);
#endif
            }
            else
            {
                ProgressLabel.Text = "Encrypting file...";
                CurrentFileLabel.Text = Path.GetFileName(_selectedSourcePath);

                var outputFileName = Path.GetFileName(_selectedSourcePath) + ".yxbenc";

#if ANDROID
                if (isOutputContentUri)
                {
                    // Encrypt to temp file first, then copy to content URI
                    var tempOutputPath = Path.Combine(FileSystem.CacheDirectory, outputFileName);
                    System.Diagnostics.Debug.WriteLine($"Encrypting to temp file: {tempOutputPath}");

                    success = await _encryptionService.EncryptFileAsync(_selectedSourcePath, tempOutputPath, PasswordEntry.Text, _cancellationTokenSource.Token);

                    if (success)
                    {
                        System.Diagnostics.Debug.WriteLine($"Writing to content URI: {_selectedOutputPath}");
                        ProgressLabel.Text = "Writing to selected folder...";

                        // Read encrypted file and write to document URI
                        var encryptedData = await File.ReadAllBytesAsync(tempOutputPath);
                        bool writeSuccess = await YXBToolsMAUI.Platforms.Android.AndroidDocumentHelper.WriteFileToDocumentUri(
                            _selectedOutputPath, outputFileName, encryptedData);

                        if (!writeSuccess)
                        {
                            success = false;
                            await DisplayAlert("Error", "Failed to write file to selected folder. File saved to Downloads instead.", "OK");
                            // Copy to Downloads as fallback
                            var downloadsPath = global::Android.OS.Environment.GetExternalStoragePublicDirectory(global::Android.OS.Environment.DirectoryDownloads)?.AbsolutePath;
                            if (!string.IsNullOrEmpty(downloadsPath))
                            {
                                var fallbackPath = Path.Combine(downloadsPath, outputFileName);
                                File.Copy(tempOutputPath, fallbackPath, true);
                            }
                        }

                        // Clean up temp file
                        File.Delete(tempOutputPath);
                    }
                }
                else
                {
                    var outputPath = Path.Combine(actualOutputPath, outputFileName);
                    success = await _encryptionService.EncryptFileAsync(_selectedSourcePath, outputPath, PasswordEntry.Text, _cancellationTokenSource.Token);
                }
#else
                var outputPath = Path.Combine(actualOutputPath, outputFileName);
                success = await _encryptionService.EncryptFileAsync(_selectedSourcePath, outputPath, PasswordEntry.Text, _cancellationTokenSource.Token);
#endif
                ProgressBar.Progress = 1.0;
            }

            if (_cancellationTokenSource.Token.IsCancellationRequested)
            {
                StatusLabel.Text = "Encryption cancelled!";
                StatusLabel.TextColor = Colors.Orange;
                await DisplayAlert("Cancelled", "Encryption was cancelled.", "OK");
            }
            else if (success)
            {
                StatusLabel.Text = "Encryption completed successfully!";
                StatusLabel.TextColor = Colors.Green;
                await DisplayAlert("Success", "Encryption completed successfully!", "OK");

                _selectedSourcePath = null;
                _selectedOutputPath = null;
                SelectedPathLabel.Text = "No file or folder selected";
                SelectedPathLabel.TextColor = Colors.Gray;
                OutputPathLabel.Text = "No output folder selected";
                OutputPathLabel.TextColor = Colors.Gray;
                PasswordEntry.Text = string.Empty;
                ConfirmPasswordEntry.Text = string.Empty;
            }
            else
            {
                StatusLabel.Text = "Encryption failed!";
                StatusLabel.TextColor = Colors.Red;
                await DisplayAlert("Error", "Encryption failed. Please try again.", "OK");
            }
        }
        catch (OperationCanceledException)
        {
            StatusLabel.Text = "Encryption cancelled!";
            StatusLabel.TextColor = Colors.Orange;
            await DisplayAlert("Cancelled", "Encryption was cancelled.", "OK");
        }
        catch (Exception ex)
        {
            StatusLabel.Text = "Encryption error!";
            StatusLabel.TextColor = Colors.Red;
            await DisplayAlert("Error", $"An error occurred: {ex.Message}", "OK");
        }
        finally
        {
            EncryptBtn.IsEnabled = true;
            ProgressFrame.IsVisible = false;
            ProgressBar.Progress = 0;
            _cancellationTokenSource?.Dispose();
            _cancellationTokenSource = null;

#if ANDROID
            // SECURITY: Securely erase temp files to prevent data recovery
            await YXBToolsMAUI.Platforms.Android.AndroidFileHandler.SecureCleanupTempDirectoryAsync();
#endif
        }
    }

    private void OnCancelClicked(object sender, EventArgs e)
    {
        _cancellationTokenSource?.Cancel();
        CancelBtn.IsEnabled = false;
        StatusLabel.Text = "Cancelling...";
        StatusLabel.TextColor = Colors.Orange;
    }
}
