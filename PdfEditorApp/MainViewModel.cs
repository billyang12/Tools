using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace PdfEditorApp;

public class MainViewModel : INotifyPropertyChanged
{
    private readonly PdfEditorService _pdfService;
    private string? _currentPdfPath;
    private string? _originalPdfPath; // Original file path
    private string? _workingPdfPath; // Temporary working copy
    private string? _statusMessage;
    private int _selectedPageNumber = 1;
    private int _totalPages;
    private string? _extractedText;
    private bool _hasUnsavedChanges;

    public MainViewModel()
    {
        _pdfService = new PdfEditorService();

        OpenCommand = new RelayCommand(_ => OpenPdf());
        SaveAsCommand = new RelayCommand(_ => SaveAs(), _ => !string.IsNullOrEmpty(CurrentPdfPath));
        MergePdfsCommand = new RelayCommand(_ => MergePdfs());
        SplitPdfCommand = new RelayCommand(_ => SplitPdf(), _ => !string.IsNullOrEmpty(CurrentPdfPath));
        DeletePageCommand = new RelayCommand(_ => DeletePage(), _ => !string.IsNullOrEmpty(CurrentPdfPath));
        RotatePageCommand = new RelayCommand(param => RotatePage(param), _ => !string.IsNullOrEmpty(CurrentPdfPath));
        AddWatermarkCommand = new RelayCommand(_ => AddWatermark(), _ => !string.IsNullOrEmpty(CurrentPdfPath));
        ExtractTextCommand = new RelayCommand(_ => ExtractText(), _ => !string.IsNullOrEmpty(CurrentPdfPath));
        EncryptPdfCommand = new RelayCommand(_ => EncryptPdf(), _ => !string.IsNullOrEmpty(CurrentPdfPath));
        ConvertImagesToPdfCommand = new RelayCommand(_ => ConvertImagesToPdf());
        NextPageCommand = new RelayCommand(_ => NextPage(), _ => SelectedPageNumber < TotalPages);
        PreviousPageCommand = new RelayCommand(_ => PreviousPage(), _ => SelectedPageNumber > 1);
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    public System.Windows.Input.ICommand OpenCommand { get; }
    public System.Windows.Input.ICommand SaveAsCommand { get; }
    public System.Windows.Input.ICommand MergePdfsCommand { get; }
    public System.Windows.Input.ICommand SplitPdfCommand { get; }
    public System.Windows.Input.ICommand DeletePageCommand { get; }
    public System.Windows.Input.ICommand RotatePageCommand { get; }
    public System.Windows.Input.ICommand AddWatermarkCommand { get; }
    public System.Windows.Input.ICommand ExtractTextCommand { get; }
    public System.Windows.Input.ICommand EncryptPdfCommand { get; }
    public System.Windows.Input.ICommand ConvertImagesToPdfCommand { get; }
    public System.Windows.Input.ICommand NextPageCommand { get; }
    public System.Windows.Input.ICommand PreviousPageCommand { get; }

    public string? CurrentPdfPath
    {
        get => _currentPdfPath;
        set
        {
            _currentPdfPath = value;
            _originalPdfPath = value;
            _hasUnsavedChanges = false;
            OnPropertyChanged();
            OnPropertyChanged(nameof(CurrentFileName));
            OnPropertyChanged(nameof(HasUnsavedChanges));
        }
    }

    public bool HasUnsavedChanges
    {
        get => _hasUnsavedChanges;
        private set
        {
            _hasUnsavedChanges = value;
            OnPropertyChanged();
        }
    }

    public string CurrentFileName => string.IsNullOrEmpty(CurrentPdfPath)
        ? "No PDF loaded"
        : System.IO.Path.GetFileName(CurrentPdfPath);

    public string? StatusMessage
    {
        get => _statusMessage;
        set
        {
            _statusMessage = value;
            OnPropertyChanged();
        }
    }

    public int SelectedPageNumber
    {
        get => _selectedPageNumber;
        set
        {
            _selectedPageNumber = value;
            OnPropertyChanged();
        }
    }

    public int TotalPages
    {
        get => _totalPages;
        set
        {
            _totalPages = value;
            OnPropertyChanged();
        }
    }

    public string? ExtractedText
    {
        get => _extractedText;
        set
        {
            _extractedText = value;
            OnPropertyChanged();
        }
    }

    private void OpenPdf()
    {
        // Check for unsaved changes
        if (HasUnsavedChanges)
        {
            var result = System.Windows.MessageBox.Show(
                "You have unsaved changes. Opening a new file will discard them.\n\nContinue?",
                "Unsaved Changes",
                System.Windows.MessageBoxButton.YesNo,
                System.Windows.MessageBoxImage.Warning);

            if (result != System.Windows.MessageBoxResult.Yes)
                return;
        }

        var openFileDialog = new Microsoft.Win32.OpenFileDialog
        {
            Filter = "PDF files (*.pdf)|*.pdf|All files (*.*)|*.*",
            Title = "Open PDF File"
        };

        if (openFileDialog.ShowDialog() == true)
        {
            try
            {
                // Clean up working file if exists
                CleanupWorkingFile();

                CurrentPdfPath = openFileDialog.FileName;
                TotalPages = _pdfService.GetPageCount(CurrentPdfPath);
                SelectedPageNumber = 1;
                StatusMessage = $"Loaded: {CurrentFileName} ({TotalPages} pages)";
                ExtractedText = null;
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error loading PDF: {ex.Message}";
                System.Windows.MessageBox.Show(
                    $"Error loading PDF file:\n\n{ex.Message}\n\nTry a different PDF file.",
                    "Error Loading PDF",
                    System.Windows.MessageBoxButton.OK,
                    System.Windows.MessageBoxImage.Error);
                CurrentPdfPath = null;
                TotalPages = 0;
            }
        }
    }

    private void CleanupWorkingFile()
    {
        if (!string.IsNullOrEmpty(_workingPdfPath) && System.IO.File.Exists(_workingPdfPath))
        {
            try
            {
                System.IO.File.Delete(_workingPdfPath);
            }
            catch
            {
                // Ignore cleanup errors
            }
        }
        _workingPdfPath = null;
        HasUnsavedChanges = false;
    }

    private void SaveAs()
    {
        if (string.IsNullOrEmpty(CurrentPdfPath)) return;

        var saveFileDialog = new Microsoft.Win32.SaveFileDialog
        {
            Filter = "PDF files (*.pdf)|*.pdf",
            Title = HasUnsavedChanges ? "Save Modified PDF As" : "Save PDF As",
            FileName = System.IO.Path.GetFileNameWithoutExtension(_originalPdfPath ?? CurrentPdfPath) +
                      (HasUnsavedChanges ? "_edited.pdf" : ".pdf")
        };

        if (saveFileDialog.ShowDialog() == true)
        {
            try
            {
                System.IO.File.Copy(CurrentPdfPath, saveFileDialog.FileName, true);
                StatusMessage = $"Saved to: {saveFileDialog.FileName}";

                // If there were changes, clean up working file and reset
                if (HasUnsavedChanges && !string.IsNullOrEmpty(_workingPdfPath))
                {
                    HasUnsavedChanges = false;
                    System.Windows.MessageBox.Show($"File saved successfully!\n\nSaved to:\n{saveFileDialog.FileName}",
                        "Save Complete", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Information);
                }
            }
            catch (System.Exception ex)
            {
                System.Windows.MessageBox.Show($"Error saving file: {ex.Message}", "Error",
                    System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
            }
        }
    }

    private void MergePdfs()
    {
        var openFileDialog = new Microsoft.Win32.OpenFileDialog
        {
            Filter = "PDF files (*.pdf)|*.pdf",
            Title = "Select PDF files to merge",
            Multiselect = true
        };

        if (openFileDialog.ShowDialog() == true && openFileDialog.FileNames.Length > 0)
        {
            var saveFileDialog = new Microsoft.Win32.SaveFileDialog
            {
                Filter = "PDF files (*.pdf)|*.pdf",
                Title = "Save Merged PDF",
                FileName = "merged.pdf"
            };

            if (saveFileDialog.ShowDialog() == true)
            {
                try
                {
                    _pdfService.MergePdfs(System.Linq.Enumerable.ToList(openFileDialog.FileNames), saveFileDialog.FileName);
                    StatusMessage = $"Merged {openFileDialog.FileNames.Length} PDFs successfully";
                    System.Windows.MessageBox.Show("PDFs merged successfully!", "Success",
                        System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Information);
                }
                catch (System.Exception ex)
                {
                    System.Windows.MessageBox.Show($"Error merging PDFs: {ex.Message}", "Error",
                        System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
                }
            }
        }
    }

    private void SplitPdf()
    {
        if (string.IsNullOrEmpty(CurrentPdfPath)) return;

        var dialog = new System.Windows.Forms.FolderBrowserDialog
        {
            Description = "Select output folder for split pages"
        };

        if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
        {
            try
            {
                var allPages = System.Linq.Enumerable.ToList(System.Linq.Enumerable.Range(1, TotalPages));
                _pdfService.SplitPdf(CurrentPdfPath, dialog.SelectedPath, allPages);
                StatusMessage = $"Split into {TotalPages} separate PDFs";
                System.Windows.MessageBox.Show($"PDF split into {TotalPages} files successfully!", "Success",
                    System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Information);
            }
            catch (System.Exception ex)
            {
                System.Windows.MessageBox.Show($"Error splitting PDF: {ex.Message}", "Error",
                    System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
            }
        }
    }

    private void DeletePage()
    {
        if (string.IsNullOrEmpty(CurrentPdfPath)) return;

        var result = System.Windows.MessageBox.Show(
            $"Delete page {SelectedPageNumber}?\n\nChanges will not be saved until you use 'Save As'.",
            "Confirm Delete",
            System.Windows.MessageBoxButton.YesNo,
            System.Windows.MessageBoxImage.Question);

        if (result == System.Windows.MessageBoxResult.Yes)
        {
            try
            {
                // Create working copy if this is the first modification
                if (!HasUnsavedChanges)
                {
                    _workingPdfPath = System.IO.Path.Combine(
                        System.IO.Path.GetTempPath(),
                        $"PdfEditor_Working_{Guid.NewGuid()}.pdf");
                    System.IO.File.Copy(CurrentPdfPath, _workingPdfPath, true);
                    _currentPdfPath = _workingPdfPath;
                }

                // Create new temp file for this operation
                var tempOutput = System.IO.Path.Combine(
                    System.IO.Path.GetTempPath(),
                    $"PdfEditor_Temp_{Guid.NewGuid()}.pdf");

                // Delete from working copy
                _pdfService.DeletePages(_currentPdfPath, tempOutput, new[] { SelectedPageNumber }.ToList());

                // Wait a moment for file system to release handles
                System.Threading.Thread.Sleep(100);

                // Replace working copy
                var oldWorkingPath = _currentPdfPath;
                _currentPdfPath = tempOutput;
                _workingPdfPath = tempOutput;

                // Try to delete old file (if it fails, it will be cleaned up later)
                try { System.IO.File.Delete(oldWorkingPath); } catch { }

                // Update page count and selection
                TotalPages--;
                if (SelectedPageNumber > TotalPages)
                    SelectedPageNumber = TotalPages;

                HasUnsavedChanges = true;
                StatusMessage = $"Page deleted (not saved)";

                // Notify that PDF changed so thumbnails reload
                OnPropertyChanged(nameof(CurrentPdfPath));
            }
            catch (System.Exception ex)
            {
                System.Windows.MessageBox.Show($"Error deleting page: {ex.Message}", "Error",
                    System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
            }
        }
    }

    private void RotatePage(object? parameter)
    {
        if (string.IsNullOrEmpty(CurrentPdfPath) || parameter is not string rotationStr) return;

        if (!int.TryParse(rotationStr, out int rotation)) return;

        try
        {
            // Create working copy if this is the first modification
            if (!HasUnsavedChanges)
            {
                _workingPdfPath = System.IO.Path.Combine(
                    System.IO.Path.GetTempPath(),
                    $"PdfEditor_Working_{Guid.NewGuid()}.pdf");
                System.IO.File.Copy(CurrentPdfPath, _workingPdfPath, true);
                _currentPdfPath = _workingPdfPath;
            }

            // Create new temp file for this operation
            var tempOutput = System.IO.Path.Combine(
                System.IO.Path.GetTempPath(),
                $"PdfEditor_Temp_{Guid.NewGuid()}.pdf");

            // Rotate in the working copy
            _pdfService.RotatePages(_currentPdfPath, tempOutput, new[] { SelectedPageNumber }.ToList(), rotation);

            // Wait a moment for file system to release handles
            System.Threading.Thread.Sleep(100);

            // Replace working copy
            var oldWorkingPath = _currentPdfPath;
            _currentPdfPath = tempOutput;
            _workingPdfPath = tempOutput;

            // Try to delete old file (if it fails, it will be cleaned up later)
            try { System.IO.File.Delete(oldWorkingPath); } catch { }

            HasUnsavedChanges = true;
            StatusMessage = $"Page {SelectedPageNumber} rotated {rotation}° (not saved)";

            // Notify that PDF changed so thumbnails reload
            OnPropertyChanged(nameof(CurrentPdfPath));
        }
        catch (System.Exception ex)
        {
            System.Windows.MessageBox.Show($"Error rotating page: {ex.Message}", "Error",
                System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
        }
    }

    private void AddWatermark()
    {
        if (string.IsNullOrEmpty(CurrentPdfPath)) return;

        var inputDialog = new System.Windows.Window
        {
            Title = "Add Watermark",
            Width = 300,
            Height = 150,
            WindowStartupLocation = System.Windows.WindowStartupLocation.CenterScreen
        };

        var textBox = new System.Windows.Controls.TextBox
        {
            Margin = new System.Windows.Thickness(10),
            Text = "CONFIDENTIAL"
        };

        var button = new System.Windows.Controls.Button
        {
            Content = "OK",
            Margin = new System.Windows.Thickness(10),
            Width = 80,
            HorizontalAlignment = System.Windows.HorizontalAlignment.Center
        };

        var stackPanel = new System.Windows.Controls.StackPanel();
        stackPanel.Children.Add(new System.Windows.Controls.TextBlock
        {
            Text = "Enter watermark text:",
            Margin = new System.Windows.Thickness(10, 10, 10, 0)
        });
        stackPanel.Children.Add(textBox);
        stackPanel.Children.Add(button);

        inputDialog.Content = stackPanel;
        button.Click += (s, e) => inputDialog.DialogResult = true;

        if (inputDialog.ShowDialog() == true && !string.IsNullOrWhiteSpace(textBox.Text))
        {
            var saveFileDialog = new Microsoft.Win32.SaveFileDialog
            {
                Filter = "PDF files (*.pdf)|*.pdf",
                Title = "Save PDF with watermark",
                FileName = System.IO.Path.GetFileNameWithoutExtension(CurrentPdfPath) + "_watermarked.pdf"
            };

            if (saveFileDialog.ShowDialog() == true)
            {
                try
                {
                    _pdfService.AddWatermark(CurrentPdfPath, saveFileDialog.FileName, textBox.Text);
                    StatusMessage = "Watermark added successfully";
                    System.Windows.MessageBox.Show("Watermark added successfully!", "Success",
                        System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Information);
                }
                catch (System.Exception ex)
                {
                    System.Windows.MessageBox.Show($"Error adding watermark: {ex.Message}", "Error",
                        System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
                }
            }
        }
    }

    private void ExtractText()
    {
        if (string.IsNullOrEmpty(CurrentPdfPath)) return;

        try
        {
            ExtractedText = _pdfService.ExtractTextFromPage(CurrentPdfPath, SelectedPageNumber);
            StatusMessage = $"Extracted text from page {SelectedPageNumber}";
        }
        catch (System.Exception ex)
        {
            System.Windows.MessageBox.Show($"Error extracting text: {ex.Message}", "Error",
                System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
        }
    }

    private void EncryptPdf()
    {
        if (string.IsNullOrEmpty(CurrentPdfPath)) return;

        var inputDialog = new System.Windows.Window
        {
            Title = "Encrypt PDF",
            Width = 350,
            Height = 200,
            WindowStartupLocation = System.Windows.WindowStartupLocation.CenterScreen
        };

        var userPasswordBox = new System.Windows.Controls.PasswordBox { Margin = new System.Windows.Thickness(10, 5, 10, 5) };
        var ownerPasswordBox = new System.Windows.Controls.PasswordBox { Margin = new System.Windows.Thickness(10, 5, 10, 5) };

        var button = new System.Windows.Controls.Button
        {
            Content = "Encrypt",
            Margin = new System.Windows.Thickness(10),
            Width = 80,
            HorizontalAlignment = System.Windows.HorizontalAlignment.Center
        };

        var stackPanel = new System.Windows.Controls.StackPanel();
        stackPanel.Children.Add(new System.Windows.Controls.TextBlock { Text = "User Password:", Margin = new System.Windows.Thickness(10, 10, 10, 0) });
        stackPanel.Children.Add(userPasswordBox);
        stackPanel.Children.Add(new System.Windows.Controls.TextBlock { Text = "Owner Password:", Margin = new System.Windows.Thickness(10, 10, 10, 0) });
        stackPanel.Children.Add(ownerPasswordBox);
        stackPanel.Children.Add(button);

        inputDialog.Content = stackPanel;
        button.Click += (s, e) => inputDialog.DialogResult = true;

        if (inputDialog.ShowDialog() == true && !string.IsNullOrWhiteSpace(userPasswordBox.Password))
        {
            var saveFileDialog = new Microsoft.Win32.SaveFileDialog
            {
                Filter = "PDF files (*.pdf)|*.pdf",
                Title = "Save encrypted PDF",
                FileName = System.IO.Path.GetFileNameWithoutExtension(CurrentPdfPath) + "_encrypted.pdf"
            };

            if (saveFileDialog.ShowDialog() == true)
            {
                try
                {
                    _pdfService.EncryptPdf(CurrentPdfPath, saveFileDialog.FileName, userPasswordBox.Password, ownerPasswordBox.Password);
                    StatusMessage = "PDF encrypted successfully";
                    System.Windows.MessageBox.Show("PDF encrypted successfully!", "Success",
                        System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Information);
                }
                catch (System.Exception ex)
                {
                    System.Windows.MessageBox.Show($"Error encrypting PDF: {ex.Message}", "Error",
                        System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
                }
            }
        }
    }

    private void ConvertImagesToPdf()
    {
        var openFileDialog = new Microsoft.Win32.OpenFileDialog
        {
            Filter = "Image files (*.jpg;*.jpeg;*.png;*.bmp)|*.jpg;*.jpeg;*.png;*.bmp",
            Title = "Select images to convert",
            Multiselect = true
        };

        if (openFileDialog.ShowDialog() == true && openFileDialog.FileNames.Length > 0)
        {
            var saveFileDialog = new Microsoft.Win32.SaveFileDialog
            {
                Filter = "PDF files (*.pdf)|*.pdf",
                Title = "Save PDF",
                FileName = "images.pdf"
            };

            if (saveFileDialog.ShowDialog() == true)
            {
                try
                {
                    _pdfService.ConvertImagesToPdf(System.Linq.Enumerable.ToList(openFileDialog.FileNames), saveFileDialog.FileName);
                    StatusMessage = $"Converted {openFileDialog.FileNames.Length} images to PDF";
                    System.Windows.MessageBox.Show("Images converted to PDF successfully!", "Success",
                        System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Information);
                }
                catch (System.Exception ex)
                {
                    System.Windows.MessageBox.Show($"Error converting images: {ex.Message}", "Error",
                        System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
                }
            }
        }
    }

    private void NextPage()
    {
        if (SelectedPageNumber < TotalPages)
        {
            SelectedPageNumber++;
            StatusMessage = $"Page {SelectedPageNumber} of {TotalPages}";
        }
    }

    private void PreviousPage()
    {
        if (SelectedPageNumber > 1)
        {
            SelectedPageNumber--;
            StatusMessage = $"Page {SelectedPageNumber} of {TotalPages}";
        }
    }

    protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
