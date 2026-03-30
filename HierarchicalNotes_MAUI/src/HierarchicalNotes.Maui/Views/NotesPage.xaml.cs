using System.Text.Json;
using CommunityToolkit.Maui.Storage;
using HierarchicalNotes.Core.Models;
using HierarchicalNotes.Core.Services;
using HierarchicalNotes.Maui.ViewModels;

namespace HierarchicalNotes.Maui.Views;

public partial class NotesPage : ContentPage
{
    private readonly MainViewModel _vm;
    private double _splitStartLeftWidth;
    private double _splitStartRightWidth;
    private const double SplitterColumnWidth = 10;
    private const double MinPaneWidth = 220;

    public NotesPage(MainViewModel vm)
    {
        InitializeComponent();
        _vm = vm;
        BindingContext = _vm;
        _vm.PropertyChanged += VmOnPropertyChanged;
        _vm.SelectionRequested += VmOnSelectionRequested;
        _vm.ContentSelectionRequested += VmOnContentSelectionRequested;
        _vm.TreeStructureChanged += VmOnTreeStructureChanged;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        UpdatePreview();
        UpdateWindowTitle();
    }

    private void VmOnTreeStructureChanged(object? sender, EventArgs e)
    {
        MainThread.BeginInvokeOnMainThread(() =>
        {
            if (_vm.SelectedNode != null && _vm.VisibleNodes.Contains(_vm.SelectedNode))
            {
                try
                {
                    NotesCollection.ScrollTo(_vm.SelectedNode);
                }
                catch
                {
                }
            }
        });
    }

    private void VmOnSelectionRequested(object? sender, NoteNodeViewModel? node)
    {
        MainThread.BeginInvokeOnMainThread(() =>
        {
            if (node != null && _vm.VisibleNodes.Contains(node))
            {
                try
                {
                    NotesCollection.ScrollTo(node);
                }
                catch
                {
                }
            }

            UpdatePreview();
        });
    }

    private void VmOnContentSelectionRequested(int start, int length)
    {
        MainThread.BeginInvokeOnMainThread(async () =>
        {
            if (!ContentEditor.IsFocused)
            {
                ContentEditor.Focus();
                await Task.Delay(30);
            }

            ContentEditor.CursorPosition = Math.Max(0, start);
            ContentEditor.SelectionLength = Math.Max(0, length);
        });
    }

    private void VmOnPropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(MainViewModel.WindowTitle))
        {
            MainThread.BeginInvokeOnMainThread(UpdateWindowTitle);
        }

        if (e.PropertyName == nameof(MainViewModel.PreviewHtml) || e.PropertyName == nameof(MainViewModel.SelectedNode))
        {
            MainThread.BeginInvokeOnMainThread(UpdatePreview);
        }
    }

    private void UpdateWindowTitle()
    {
        if (Window != null)
        {
            Window.Title = _vm.WindowTitle;
        }
    }

    private void UpdatePreview()
    {
        PreviewWebView.Source = new HtmlWebViewSource { Html = _vm.PreviewHtml };
    }

    private async void NewClicked(object sender, EventArgs e)
    {
        if (await ConfirmDiscardIfNeededAsync())
        {
            _vm.NewFile();
            UpdatePreview();
        }
    }

    private async void OpenClicked(object sender, EventArgs e)
    {
        if (!await ConfirmDiscardIfNeededAsync())
        {
            return;
        }

        var file = await FilePicker.Default.PickAsync(new PickOptions
        {
            PickerTitle = "Open Hierarchical Notes file",
            FileTypes = new FilePickerFileType(new Dictionary<DevicePlatform, IEnumerable<string>>
            {
                { DevicePlatform.WinUI, new[] { ".hnf" } },
                { DevicePlatform.iOS, new[] { ".hnf" } },
                { DevicePlatform.MacCatalyst, new[] { ".hnf" } },
                { DevicePlatform.Android, new[] { "*/*" } }
            })
        });

        if (file == null)
        {
            return;
        }

        try
        {
            var json = await File.ReadAllTextAsync(file.FullPath);
            var noteFile = JsonSerializer.Deserialize<HNoteFile>(json);
            if (noteFile == null)
            {
                await DisplayAlert("Open", "The selected file could not be read.", "OK");
                return;
            }

            if (noteFile.IsEncrypted)
            {
                var password = await PromptForPasswordAsync("File password", "Enter the file password.");
                if (string.IsNullOrEmpty(password))
                {
                    return;
                }

                try
                {
                    var decryptedJson = StringCipher.DecryptToString(noteFile.HNoteJsonStr ?? string.Empty, password);
                    var notes = JsonSerializer.Deserialize<HNoteCollection>(decryptedJson);
                    if (notes == null)
                    {
                        await DisplayAlert("Open", "Unable to decrypt the file.", "OK");
                        return;
                    }

                    _vm.SetCurrentPassword(password);
                    _vm.LoadFromNotes(notes, file.FullPath, password, true);
                }
                catch (Exception ex)
                {
                    await DisplayAlert("Open", ex.Message, "OK");
                    return;
                }
            }
            else
            {
                _vm.SetCurrentPassword(null);
                _vm.LoadFromNotes(noteFile.Notes ?? new HNoteCollection(), file.FullPath, null, false);
            }

            UpdatePreview();
        }
        catch (Exception ex)
        {
            await DisplayAlert("Open", ex.Message, "OK");
        }
    }

    private async void SaveClicked(object sender, EventArgs e)
    {
        await SaveAsync(saveAs: false);
    }

    private async void SaveAsClicked(object sender, EventArgs e)
    {
        await SaveAsync(saveAs: true);
    }

    private async void BackupClicked(object sender, EventArgs e)
    {
        await BackupAsync();
    }

    private async void PasswordClicked(object sender, EventArgs e)
    {
        await SetPasswordAsync();
    }

    private async void ExportHtmlClicked(object sender, EventArgs e)
    {
        await ExportHtmlAsync();
    }

    private async void CopyClicked(object sender, EventArgs e)
    {
        SelectContextNode(sender);
        await CopySelectedAsync();
    }

    private async void PasteClicked(object sender, EventArgs e)
    {
        SelectContextNode(sender);
        await PasteSelectedAsync(asChild: false);
    }

    private async void PasteChildClicked(object sender, EventArgs e)
    {
        SelectContextNode(sender);
        await PasteSelectedAsync(asChild: true);
    }

    private void AddNewClicked(object sender, EventArgs e) => _vm.AddNewNode();
    private void AddBeforeClicked(object sender, EventArgs e) => _vm.AddNewNodeBefore();
    private void AddAfterClicked(object sender, EventArgs e) => _vm.AddNewNodeAfter();
    private void AddChildClicked(object sender, EventArgs e) => _vm.AddNewChildNode();
    private void DuplicateClicked(object sender, EventArgs e) => _vm.DuplicateSelectedNode();
    private async void RemoveClicked(object sender, EventArgs e)
    {
        if (_vm.SelectedNode == null)
        {
            return;
        }

        var ok = await DisplayAlert("Remove", "Do you really want to remove it?", "Yes", "No");
        if (ok)
        {
            _vm.RemoveSelectedNode();
        }
    }
    private void MoveUpClicked(object sender, EventArgs e) => _vm.MoveSelectedUp();
    private void MoveDownClicked(object sender, EventArgs e) => _vm.MoveSelectedDown();
    private void IndentClicked(object sender, EventArgs e) => _vm.IndentSelected();
    private void OutdentClicked(object sender, EventArgs e) => _vm.OutdentSelected();
    private void ReverseClicked(object sender, EventArgs e) => _vm.ReverseChildrenOfSelectedNode();

    private bool SelectContextNode(object? sender)
    {
        var node = (sender as BindableObject)?.BindingContext as NoteNodeViewModel;
        if (node == null)
        {
            return false;
        }

        _vm.SelectNode(node);
        return true;
    }

    private void ContextAddBeforeClicked(object sender, EventArgs e)
    {
        if (SelectContextNode(sender))
        {
            _vm.AddNewNodeBefore();
        }
    }

    private void ContextAddAfterClicked(object sender, EventArgs e)
    {
        if (SelectContextNode(sender))
        {
            _vm.AddNewNodeAfter();
        }
    }

    private void ContextAddChildClicked(object sender, EventArgs e)
    {
        if (SelectContextNode(sender))
        {
            _vm.AddNewChildNode();
        }
    }

    private void ContextDuplicateClicked(object sender, EventArgs e)
    {
        if (SelectContextNode(sender))
        {
            _vm.DuplicateSelectedNode();
        }
    }

    private async void ContextRemoveClicked(object sender, EventArgs e)
    {
        if (SelectContextNode(sender))
        {
            await RemoveSelectedAsync();
        }
    }

    private void ContextMoveUpClicked(object sender, EventArgs e)
    {
        if (SelectContextNode(sender))
        {
            _vm.MoveSelectedUp();
        }
    }

    private void ContextMoveDownClicked(object sender, EventArgs e)
    {
        if (SelectContextNode(sender))
        {
            _vm.MoveSelectedDown();
        }
    }

    private void ContextIndentClicked(object sender, EventArgs e)
    {
        if (SelectContextNode(sender))
        {
            _vm.IndentSelected();
        }
    }

    private void ContextOutdentClicked(object sender, EventArgs e)
    {
        if (SelectContextNode(sender))
        {
            _vm.OutdentSelected();
        }
    }

    private void ContextReverseClicked(object sender, EventArgs e)
    {
        if (SelectContextNode(sender))
        {
            _vm.ReverseChildrenOfSelectedNode();
        }
    }

    private async void NodeActionsClicked(object sender, EventArgs e)
    {
        if (!SelectContextNode(sender))
        {
            return;
        }

        var action = await DisplayActionSheet(
            "Node actions",
            "Cancel",
            null,
            "Before",
            "After",
            "Child",
            "Duplicate",
            "Copy",
            "Paste",
            "Paste Child",
            "Move Up",
            "Move Down",
            "Indent",
            "Outdent",
            "Reverse Children",
            "Remove");

        switch (action)
        {
            case "Before":
                _vm.AddNewNodeBefore();
                break;
            case "After":
                _vm.AddNewNodeAfter();
                break;
            case "Child":
                _vm.AddNewChildNode();
                break;
            case "Duplicate":
                _vm.DuplicateSelectedNode();
                break;
            case "Copy":
                await CopySelectedAsync();
                break;
            case "Paste":
                await PasteSelectedAsync(asChild: false);
                break;
            case "Paste Child":
                await PasteSelectedAsync(asChild: true);
                break;
            case "Move Up":
                _vm.MoveSelectedUp();
                break;
            case "Move Down":
                _vm.MoveSelectedDown();
                break;
            case "Indent":
                _vm.IndentSelected();
                break;
            case "Outdent":
                _vm.OutdentSelected();
                break;
            case "Reverse Children":
                _vm.ReverseChildrenOfSelectedNode();
                break;
            case "Remove":
                await RemoveSelectedAsync();
                break;
        }
    }

    private void ContentSearchButtonPressed(object sender, EventArgs e)
    {
        _vm.SearchCurrentContent(ContentSearchBar.Text ?? string.Empty);
    }

    private async Task RemoveSelectedAsync()
    {
        if (_vm.SelectedNode == null)
        {
            return;
        }

        var ok = await DisplayAlert("Remove", "Do you really want to remove it?", "Yes", "No");
        if (ok)
        {
            _vm.RemoveSelectedNode();
        }
    }

    private async Task CopySelectedAsync()
    {
        var json = _vm.GetSelectedNodeJson();
        if (string.IsNullOrWhiteSpace(json))
        {
            return;
        }

        await Clipboard.Default.SetTextAsync(json);
        await DisplayAlert("Copy", "Selected note copied to clipboard.", "OK");
    }

    private async Task PasteSelectedAsync(bool asChild)
    {
        var text = await Clipboard.Default.GetTextAsync();
        if (string.IsNullOrWhiteSpace(text))
        {
            return;
        }

        var success = asChild
            ? _vm.PasteNodeAsChildFromJson(text)
            : _vm.PasteNodeFromJson(text, asChild: false);

        if (!success)
        {
            await DisplayAlert("Paste", "Clipboard does not contain a valid note.", "OK");
        }

        UpdatePreview();
    }

    private void ContentFindClicked(object sender, EventArgs e)
    {
        if (_vm.SelectedNode != null)
        {
            _vm.SelectedNode.Content = ContentEditor.Text ?? string.Empty;
        }

        _vm.SearchCurrentContent(ContentSearchBar.Text ?? string.Empty);
    }

    private async Task<bool> ConfirmDiscardIfNeededAsync()
    {
        if (!_vm.IsDirty)
        {
            return true;
        }

        var choice = await DisplayAlert("Unsaved changes", "Notes content has changed, do you want to save it?", "Save", "Discard");
        if (!choice)
        {
            return true;
        }

        return await SaveAsync(saveAs: false);
    }

    public async Task<bool> ConfirmCloseIfNeededAsync()
    {
        if (!_vm.IsDirty)
        {
            return true;
        }

        var choice = await DisplayActionSheet("Unsaved changes", "Cancel", null, "Save", "Discard");
        if (choice == "Save")
        {
            return await SaveAsync(saveAs: false);
        }

        if (choice == "Discard")
        {
            return true;
        }

        return false;
    }

    private async Task<bool> SaveAsync(bool saveAs)
    {
        try
        {
            var json = _vm.BuildCurrentFileJson();
            var fileName = string.IsNullOrWhiteSpace(_vm.CurrentFileName)
                ? $"Notes-{DateTime.Now:yyyyMMdd-HHmmss}.hnf"
                : Path.GetFileName(_vm.CurrentFileName);

            if (!saveAs && !string.IsNullOrWhiteSpace(_vm.CurrentFileName))
            {
                await File.WriteAllTextAsync(_vm.CurrentFileName!, json);
                _vm.CurrentNoteFile.SaveLatestNotesJson();
                UpdatePreview();
                await DisplayAlert("Save", "File saved.", "OK");
                return true;
            }

            using var stream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(json));
            var result = await FileSaver.Default.SaveAsync(fileName, stream, CancellationToken.None);
            if (!string.IsNullOrWhiteSpace(result.FilePath))
            {
                _vm.SetCurrentFileName(result.FilePath);
                _vm.CurrentNoteFile.SaveLatestNotesJson();
                await DisplayAlert("Save", "File saved.", "OK");
                return true;
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Save", ex.Message, "OK");
        }

        return false;
    }

    private async Task BackupAsync()
    {
        try
        {
            if (string.IsNullOrWhiteSpace(_vm.CurrentFileName))
            {
                await DisplayAlert("Backup", "Please save the file first.", "OK");
                return;
            }

            var json = _vm.BuildCurrentFileJson();
            var current = new FileInfo(_vm.CurrentFileName!);
            var backupName = $"Backup{DateTime.Now:yyyy-MM-dd-HHmmss} {current.Name}";
            var backupPath = Path.Combine(current.DirectoryName ?? FileSystem.AppDataDirectory, backupName);
            await File.WriteAllTextAsync(backupPath, json);
            await DisplayAlert("Backup", "Backup file saved.", "OK");
        }
        catch (Exception ex)
        {
            await DisplayAlert("Backup", ex.Message, "OK");
        }
    }

    private async Task ExportHtmlAsync()
    {
        try
        {
            var html = _vm.ExportHtml();
            using var stream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(html));
            var fileName = string.IsNullOrWhiteSpace(_vm.CurrentFileName)
                ? $"Notes-{DateTime.Now:yyyyMMdd-HHmmss}.html"
                : Path.GetFileNameWithoutExtension(_vm.CurrentFileName) + ".html";
            await FileSaver.Default.SaveAsync(fileName, stream, CancellationToken.None);
            await DisplayAlert("Export", "HTML exported successfully.", "OK");
        }
        catch (Exception ex)
        {
            await DisplayAlert("Export", ex.Message, "OK");
        }
    }

    private async Task SetPasswordAsync()
    {
        try
        {
            string? current = null;
            if (_vm.CurrentNoteFile.IsEncrypted)
            {
                current = await PromptForPasswordAsync("Password", "Enter the current password.");
                if (string.IsNullOrWhiteSpace(current))
                {
                    return;
                }
            }

            var newPassword = await PromptForPasswordAsync("New password", "Enter the new password.");
            var confirmPassword = await PromptForPasswordAsync("Confirm password", "Confirm the new password.");

            if (string.IsNullOrWhiteSpace(newPassword) || newPassword.Length < 8)
            {
                await DisplayAlert("Password", "Password length should be at least 8 characters.", "OK");
                return;
            }

            if (newPassword != confirmPassword)
            {
                await DisplayAlert("Password", "New password and confirmed password do not match.", "OK");
                return;
            }

            if (_vm.CurrentNoteFile.IsEncrypted && !string.IsNullOrWhiteSpace(_vm.CurrentPasswordForEncryption) && current != _vm.CurrentPasswordForEncryption)
            {
                await DisplayAlert("Password", "Old password is not correct.", "OK");
                return;
            }

            _vm.CurrentNoteFile.IsEncrypted = true;
            _vm.SetCurrentPassword(newPassword);
            await DisplayAlert("Password", "Password set successfully. Save the file to apply the change.", "OK");
        }
        catch (Exception ex)
        {
            await DisplayAlert("Password", ex.Message, "OK");
        }
    }

    private async Task<string?> PromptForPasswordAsync(string title, string message)
    {
        var navigation = Application.Current?.MainPage?.Navigation ?? Navigation;
        var tcs = new TaskCompletionSource<string?>();
        var isClosing = false;

        var entry = new Entry
        {
            IsPassword = true,
            Placeholder = "Password",
            Margin = new Thickness(0, 8, 0, 0)
        };

        var okButton = new Button { Text = "OK" };
        var cancelButton = new Button { Text = "Cancel" };

        var page = new ContentPage
        {
            Title = title,
            Content = new ScrollView
            {
                Content = new VerticalStackLayout
                {
                    Padding = new Thickness(20),
                    Spacing = 12,
                    Children =
                    {
                        new Label { Text = message, LineBreakMode = LineBreakMode.WordWrap },
                        entry,
                        new HorizontalStackLayout
                        {
                            Spacing = 12,
                            Children = { okButton, cancelButton }
                        }
                    }
                }
            }
        };

        var modalPage = new NavigationPage(page);

        async Task CloseAsync(string? value)
        {
            if (isClosing)
            {
                return;
            }

            isClosing = true;

            try
            {
                var isTopModal = navigation.ModalStack.Count > 0 && ReferenceEquals(navigation.ModalStack[^1], modalPage);
                var containsModal = navigation.ModalStack.Contains(modalPage);

                if (isTopModal || containsModal)
                {
                    await navigation.PopModalAsync();
                }
            }
            finally
            {
                tcs.TrySetResult(value);
            }
        }

        okButton.Clicked += async (_, _) => await CloseAsync(entry.Text);
        cancelButton.Clicked += async (_, _) => await CloseAsync(null);
        entry.Completed += async (_, _) => await CloseAsync(entry.Text);

        await navigation.PushModalAsync(modalPage);
        return await tcs.Task;
    }

    private void OnSplitterPanUpdated(object? sender, PanUpdatedEventArgs e)
    {
        if (MainSplitGrid.Width <= 0)
        {
            return;
        }

        if (e.StatusType == GestureStatus.Started)
        {
            var available = Math.Max(0, MainSplitGrid.Width - SplitterColumnWidth);
            _splitStartLeftWidth = TreeColumn.Width.IsAbsolute
                ? TreeColumn.Width.Value
                : available * (2d / 5d);
            _splitStartRightWidth = EditorColumn.Width.IsAbsolute
                ? EditorColumn.Width.Value
                : available - _splitStartLeftWidth;
            return;
        }

        if (e.StatusType != GestureStatus.Running)
        {
            return;
        }

        var total = Math.Max(0, MainSplitGrid.Width - SplitterColumnWidth);
        var left = _splitStartLeftWidth + e.TotalX;
        left = Math.Max(MinPaneWidth, Math.Min(total - MinPaneWidth, left));
        var right = Math.Max(MinPaneWidth, total - left);

        TreeColumn.Width = new GridLength(left, GridUnitType.Absolute);
        EditorColumn.Width = new GridLength(right, GridUnitType.Absolute);
    }
}
