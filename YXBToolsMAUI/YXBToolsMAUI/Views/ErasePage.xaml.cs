using YXBToolsMAUI.Services;
using FolderPicker = YXBToolsMAUI.Services.FolderPicker;

namespace YXBToolsMAUI.Views;

public partial class ErasePage : ContentPage
{
    private readonly IEraseService _eraseService;
    private string? _selectedPath;
    private bool _isFolder;
    private CancellationTokenSource? _cancellationTokenSource;
    private EraseMethod _selectedMethod = EraseMethod.Quick;
    private bool _includeSubfolders = false;

    public ErasePage(IEraseService eraseService)
    {
        InitializeComponent();
        _eraseService = eraseService;
        MethodPicker.SelectedIndex = 0; // Default to Quick
    }

    private void OnIncludeSubfoldersChanged(object sender, CheckedChangedEventArgs e)
    {
        _includeSubfolders = e.Value;
        SubfolderWarningLabel.IsVisible = e.Value;
        System.Diagnostics.Debug.WriteLine($"Include subfolders: {_includeSubfolders}");
    }

    private void OnMethodChanged(object sender, EventArgs e)
    {
        _selectedMethod = MethodPicker.SelectedIndex switch
        {
            0 => EraseMethod.Quick,
            1 => EraseMethod.DoD3Pass,
            2 => EraseMethod.DoD7Pass,
            3 => EraseMethod.Gutmann,
            _ => EraseMethod.Quick
        };

        MethodInfoLabel.Text = _selectedMethod switch
        {
            EraseMethod.Quick => "Quick method overwrites with zeros once. Fast and sufficient for preventing casual recovery.",
            EraseMethod.DoD3Pass => "DoD 3-Pass: Zeros → Ones → Random. Good balance of security and speed. Takes 3x longer than Quick.",
            EraseMethod.DoD7Pass => "DoD 7-Pass: Multiple passes with zeros, ones, and random data. High security. Takes 7x longer than Quick.",
            EraseMethod.Gutmann => "Gutmann 35-Pass: Maximum security with 35 different patterns. Very slow. Takes 35x longer than Quick. Designed for old hard drives.",
            _ => ""
        };

        System.Diagnostics.Debug.WriteLine($"Erase method changed to: {_selectedMethod}");
    }

    private async void OnSelectFileClicked(object sender, EventArgs e)
    {
        try
        {
            string? filePath = null;

#if WINDOWS
            var filePickerService = new YXBToolsMAUI.Platforms.Windows.FilePickerService();
            filePath = await filePickerService.PickFileAsync("Select a file to erase");
#elif ANDROID
            // Use Android native file picker to get content URI (not cache copy)
            var androidFilePicker = new YXBToolsMAUI.Platforms.Android.AndroidFilePickerService();
            filePath = await androidFilePicker.PickFileAsync();
#else
            var result = await FilePicker.Default.PickAsync(new PickOptions
            {
                PickerTitle = "Select a file to erase"
            });
            filePath = result?.FullPath;
#endif

#if ANDROID
            if (!string.IsNullOrEmpty(filePath))
            {
                System.Diagnostics.Debug.WriteLine($"OnSelectFileClicked: File picked: {filePath}");

                // For content URIs, check if writable
                if (filePath.StartsWith("content://"))
                {
                    StatusLabel.Text = "Checking file permissions...";
                    StatusLabel.TextColor = Colors.Blue;

                    bool isWritable = YXBToolsMAUI.Platforms.Android.AndroidEraseHelper.IsContentUriWritable(filePath);

                    if (!isWritable)
                    {
                        StatusLabel.Text = "File is read-only";
                        StatusLabel.TextColor = Colors.Red;
                        await DisplayAlert("Error", "The selected file is read-only and cannot be securely erased. Please select a file you have write permission for.", "OK");
                        return;
                    }

                    System.Diagnostics.Debug.WriteLine($"OnSelectFileClicked: Content URI is writable: {filePath}");
                }

                StatusLabel.Text = string.Empty;
            }
#endif

            if (!string.IsNullOrEmpty(filePath))
            {
                _selectedPath = filePath;
                _isFolder = false;
                SelectedPathLabel.Text = $"File: {Path.GetFileName(filePath)}";
                SelectedPathLabel.TextColor = Colors.Red;
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
            System.Diagnostics.Debug.WriteLine("OnSelectFolderClicked: Starting folder picker");

            var result = await FolderPicker.PickAsync();

            System.Diagnostics.Debug.WriteLine($"OnSelectFolderClicked: Folder picker returned, result is null: {result == null}");

            if (result?.Folder != null && !string.IsNullOrEmpty(result.Folder.Path))
            {
                System.Diagnostics.Debug.WriteLine($"OnSelectFolderClicked: Selected path: {result.Folder.Path}");

                _selectedPath = result.Folder.Path;
                _isFolder = true;
                SelectedPathLabel.Text = result.Folder.Path;
                SelectedPathLabel.TextColor = Colors.Red;
                StatusLabel.Text = string.Empty;

                System.Diagnostics.Debug.WriteLine("OnSelectFolderClicked: UI updated successfully");
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("OnSelectFolderClicked: No folder selected or result is null");
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Folder picker error: {ex.Message}");
            System.Diagnostics.Debug.WriteLine($"Stack trace: {ex.StackTrace}");
            System.Diagnostics.Debug.WriteLine($"Inner exception: {ex.InnerException?.Message}");
            await DisplayAlert("Error", $"Failed to select folder:\n\n{ex.Message}\n\nType: {ex.GetType().Name}", "OK");
        }
    }

    private async void OnEraseClicked(object sender, EventArgs e)
    {
        if (string.IsNullOrEmpty(_selectedPath))
        {
            await DisplayAlert("Error", "Please select a file or folder to erase", "OK");
            return;
        }

        // Check confirmation
        if (ConfirmationEntry.Text?.Trim().ToUpper() != "ERASE")
        {
            await DisplayAlert("Confirmation Required", "You must type 'ERASE' in the confirmation field to proceed.", "OK");
            return;
        }

        // Final warning
        string message;
        if (_isFolder)
        {
            if (_includeSubfolders)
            {
                message = "This will PERMANENTLY DESTROY all files in the selected folder AND ALL SUBFOLDERS by overwriting them and deleting them.\n\n⚠️ INCLUDING ALL NESTED FOLDERS! ⚠️\n\nThis CANNOT be undone!\n\nContinue?";
            }
            else
            {
                message = "This will PERMANENTLY DESTROY all files in the selected folder (not subfolders) by overwriting them and deleting them.\n\nThis CANNOT be undone!\n\nContinue?";
            }
        }
        else
        {
            message = "This will PERMANENTLY DESTROY the selected file by overwriting it and deleting it.\n\nThis CANNOT be undone!\n\nContinue?";
        }

        bool confirmed = await DisplayAlert("⚠️ FINAL WARNING ⚠️", message, "Yes, Erase Forever", "Cancel");

        if (!confirmed)
            return;

        _cancellationTokenSource = new CancellationTokenSource();

        EraseBtn.IsEnabled = false;
        SelectFileBtn.IsEnabled = false;
        SelectFolderBtn.IsEnabled = false;
        ConfirmationEntry.IsEnabled = false;
        StatusLabel.Text = "";
        ProgressFrame.IsVisible = true;
        ProgressBar.Progress = 0;
        ProgressLabel.Text = "Starting...";
        CurrentFileLabel.Text = "";

        try
        {
            bool success;

            // Pass progress reporter
            var passProgress = new Progress<(int currentPass, int totalPasses)>(update =>
            {
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    PassProgressLabel.Text = $"Pass {update.currentPass} of {update.totalPasses}";
                });
            });

            if (_isFolder)
            {
                var progress = new Progress<(int current, int total, string fileName)>(update =>
                {
                    MainThread.BeginInvokeOnMainThread(() =>
                    {
                        ProgressBar.Progress = (double)update.current / update.total;
                        ProgressLabel.Text = $"{update.current} / {update.total} files";
                        CurrentFileLabel.Text = $"Erasing: {update.fileName}";
                    });
                });

                success = await _eraseService.EraseFolderAsync(_selectedPath, _selectedMethod, _includeSubfolders, progress, passProgress, _cancellationTokenSource.Token);
            }
            else
            {
                ProgressLabel.Text = "Erasing file...";
                CurrentFileLabel.Text = Path.GetFileName(_selectedPath);

                success = await _eraseService.EraseFileAsync(_selectedPath, _selectedMethod, passProgress, _cancellationTokenSource.Token);
                ProgressBar.Progress = 1.0;
            }

            if (_cancellationTokenSource.Token.IsCancellationRequested)
            {
                StatusLabel.Text = "Erase cancelled!";
                StatusLabel.TextColor = Colors.Orange;
                await DisplayAlert("Cancelled", "Erase operation was cancelled.", "OK");
            }
            else if (success)
            {
                StatusLabel.Text = "Files erased successfully!";
                StatusLabel.TextColor = Colors.Green;
                await DisplayAlert("Success", "Files have been permanently erased!", "OK");

                _selectedPath = null;
                SelectedPathLabel.Text = "No file or folder selected";
                SelectedPathLabel.TextColor = Colors.Gray;
                ConfirmationEntry.Text = string.Empty;
            }
            else
            {
                StatusLabel.Text = "Erase failed!";
                StatusLabel.TextColor = Colors.Red;
                await DisplayAlert("Error", "Erase operation failed. Please try again.", "OK");
            }
        }
        catch (OperationCanceledException)
        {
            StatusLabel.Text = "Erase cancelled!";
            StatusLabel.TextColor = Colors.Orange;
            await DisplayAlert("Cancelled", "Erase operation was cancelled.", "OK");
        }
        catch (Exception ex)
        {
            StatusLabel.Text = "Erase error!";
            StatusLabel.TextColor = Colors.Red;
            await DisplayAlert("Error", $"An error occurred: {ex.Message}", "OK");
        }
        finally
        {
            EraseBtn.IsEnabled = true;
            SelectFileBtn.IsEnabled = true;
            SelectFolderBtn.IsEnabled = true;
            ConfirmationEntry.IsEnabled = true;
            ProgressFrame.IsVisible = false;
            ProgressBar.Progress = 0;
            _cancellationTokenSource?.Dispose();
            _cancellationTokenSource = null;
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
