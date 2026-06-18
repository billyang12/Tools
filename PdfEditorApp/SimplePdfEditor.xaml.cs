using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows;

namespace PdfEditorApp;

public partial class SimplePdfEditor : System.Windows.Window
{
    private PdfContentEditor _contentEditor;
    private PdfEditorService _pdfService;
    private string? _currentPdfPath;
    private string? _pdfPassword;
    private string? _selectedImagePath;
    private ObservableCollection<string> _editHistory;

    private class PendingEdit
    {
        public string Type { get; set; } = "";
        public int Page { get; set; }
        public float X { get; set; }
        public float Y { get; set; }
        public float Width { get; set; }
        public float Height { get; set; }
        public string? Content { get; set; }
        public string? FontName { get; set; }
        public iText.Kernel.Colors.DeviceRgb? Color { get; set; }
        public float FontSize { get; set; }
        public float Opacity { get; set; } = 1.0f;
    }

    private System.Collections.Generic.List<PendingEdit> _pendingEdits;

    public SimplePdfEditor()
    {
        InitializeComponent();
        _contentEditor = new PdfContentEditor();
        _pdfService = new PdfEditorService();
        _editHistory = new ObservableCollection<string>();
        _pendingEdits = new System.Collections.Generic.List<PendingEdit>();
        EditHistoryList.ItemsSource = _editHistory;
    }

    private void OpenPdf_Click(object sender, RoutedEventArgs e)
    {
        var openDialog = new Microsoft.Win32.OpenFileDialog
        {
            Filter = "PDF files (*.pdf)|*.pdf",
            Title = "Open PDF for Editing"
        };

        if (openDialog.ShowDialog() == true)
        {
            try
            {
                _currentPdfPath = openDialog.FileName;

                // Check if PDF is password protected
                if (_pdfService.IsPasswordProtected(_currentPdfPath))
                {
                    var passwordDialog = new PasswordDialog();
                    if (passwordDialog.ShowDialog() == true)
                    {
                        _pdfPassword = passwordDialog.Password;
                        var totalPages = _pdfService.GetPageCount(_currentPdfPath, _pdfPassword);

                        FileNameText.Text = System.IO.Path.GetFileName(_currentPdfPath);
                        PageInfoText.Text = $"PDF: {System.IO.Path.GetFileName(_currentPdfPath)} (Password Protected)\n" +
                                           $"Total Pages: {totalPages}\n" +
                                           $"File Size: {new FileInfo(_currentPdfPath).Length / 1024} KB\n\n" +
                                           $"Use the sliders on the left to position content.\n" +
                                           $"Changes are applied when you click 'Apply Changes'.";

                        SaveButton.IsEnabled = true;
                        _pendingEdits.Clear();
                        _editHistory.Clear();

                        StatusText.Text = $"Loaded: {System.IO.Path.GetFileName(_currentPdfPath)} ({totalPages} pages) - Encrypted";
                    }
                    else
                    {
                        _currentPdfPath = null;
                        _pdfPassword = null;
                        StatusText.Text = "PDF open cancelled - password required";
                    }
                }
                else
                {
                    _pdfPassword = null;
                    var totalPages = _pdfService.GetPageCount(_currentPdfPath);

                    FileNameText.Text = System.IO.Path.GetFileName(_currentPdfPath);
                    PageInfoText.Text = $"PDF: {System.IO.Path.GetFileName(_currentPdfPath)}\n" +
                                       $"Total Pages: {totalPages}\n" +
                                       $"File Size: {new FileInfo(_currentPdfPath).Length / 1024} KB\n\n" +
                                       $"Use the sliders on the left to position content.\n" +
                                       $"Changes are applied when you click 'Apply Changes'.";

                    SaveButton.IsEnabled = true;
                    _pendingEdits.Clear();
                    _editHistory.Clear();

                    StatusText.Text = $"Loaded: {System.IO.Path.GetFileName(_currentPdfPath)} ({totalPages} pages)";
                }
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Error opening PDF: {ex.Message}\n\nIf this is a password-protected PDF, the password may be incorrect.", "Error",
                    System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
                _currentPdfPath = null;
            }
        }
    }

    private void AddText_Click(object sender, RoutedEventArgs e)
    {
        if (string.IsNullOrEmpty(_currentPdfPath))
        {
            System.Windows.MessageBox.Show("Please open a PDF first", "No PDF",
                System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Warning);
            return;
        }

        if (string.IsNullOrWhiteSpace(AddTextBox.Text))
        {
            System.Windows.MessageBox.Show("Please enter text to add", "No Text",
                System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Warning);
            return;
        }

        if (!int.TryParse(AddTextPageBox.Text, out int page))
        {
            System.Windows.MessageBox.Show("Invalid page number", "Error",
                System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Warning);
            return;
        }

        // Get selected font
        string fontName = iText.IO.Font.Constants.StandardFonts.HELVETICA;
        string fontDisplayName = "Helvetica";
        switch (AddTextFontCombo.SelectedIndex)
        {
            case 0:
                fontName = iText.IO.Font.Constants.StandardFonts.HELVETICA;
                fontDisplayName = "Helvetica";
                break;
            case 1:
                fontName = iText.IO.Font.Constants.StandardFonts.HELVETICA_BOLD;
                fontDisplayName = "Helvetica-Bold";
                break;
            case 2:
                fontName = iText.IO.Font.Constants.StandardFonts.TIMES_ROMAN;
                fontDisplayName = "Times-Roman";
                break;
            case 3:
                fontName = iText.IO.Font.Constants.StandardFonts.TIMES_BOLD;
                fontDisplayName = "Times-Bold";
                break;
            case 4:
                fontName = iText.IO.Font.Constants.StandardFonts.COURIER;
                fontDisplayName = "Courier";
                break;
        }

        // Get selected color
        iText.Kernel.Colors.DeviceRgb color;
        string colorName = "Black";
        switch (AddTextColorCombo.SelectedIndex)
        {
            case 0:
                color = new iText.Kernel.Colors.DeviceRgb(0, 0, 0);
                colorName = "Black";
                break;
            case 1:
                color = new iText.Kernel.Colors.DeviceRgb(1f, 0, 0);
                colorName = "Red";
                break;
            case 2:
                color = new iText.Kernel.Colors.DeviceRgb(0, 0, 1f);
                colorName = "Blue";
                break;
            case 3:
                color = new iText.Kernel.Colors.DeviceRgb(0, 1f, 0);
                colorName = "Green";
                break;
            case 4:
                color = new iText.Kernel.Colors.DeviceRgb(0.5f, 0, 0.5f);
                colorName = "Purple";
                break;
            case 5:
                color = new iText.Kernel.Colors.DeviceRgb(1f, 1f, 1f);
                colorName = "White";
                break;
            default:
                color = new iText.Kernel.Colors.DeviceRgb(0, 0, 0);
                colorName = "Black";
                break;
        }

        var edit = new PendingEdit
        {
            Type = "AddText",
            Page = page,
            X = (float)AddTextXSlider.Value,
            Y = (float)AddTextYSlider.Value,
            Width = (float)AddTextSizeSlider.Value,
            Content = AddTextBox.Text,
            FontName = fontName,
            Color = color
        };

        _pendingEdits.Add(edit);
        _editHistory.Add($"Add Text: '{edit.Content}' at Page {page}, X:{edit.X:F0}, Y:{edit.Y:F0}, Font:{fontDisplayName}, Color:{colorName}");

        StatusText.Text = $"{_pendingEdits.Count} change(s) pending";
        AddTextBox.Clear();
    }

    private void SelectImage_Click(object sender, RoutedEventArgs e)
    {
        var openDialog = new Microsoft.Win32.OpenFileDialog
        {
            Filter = "Image files (*.jpg;*.jpeg;*.png;*.bmp)|*.jpg;*.jpeg;*.png;*.bmp",
            Title = "Select Image"
        };

        if (openDialog.ShowDialog() == true)
        {
            _selectedImagePath = openDialog.FileName;
            SelectedImageLabel.Text = System.IO.Path.GetFileName(_selectedImagePath);
            AddImageButton.IsEnabled = true;
            StatusText.Text = $"Image selected: {SelectedImageLabel.Text}";
        }
    }

    private void AddImage_Click(object sender, RoutedEventArgs e)
    {
        if (string.IsNullOrEmpty(_currentPdfPath))
        {
            System.Windows.MessageBox.Show("Please open a PDF first", "No PDF",
                System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Warning);
            return;
        }

        if (string.IsNullOrEmpty(_selectedImagePath))
        {
            System.Windows.MessageBox.Show("Please select an image first", "No Image",
                System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Warning);
            return;
        }

        if (!int.TryParse(AddImagePageBox.Text, out int page))
        {
            System.Windows.MessageBox.Show("Invalid page number", "Error",
                System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Warning);
            return;
        }

        var edit = new PendingEdit
        {
            Type = "AddImage",
            Page = page,
            X = (float)AddImageXSlider.Value,
            Y = (float)AddImageYSlider.Value,
            Width = (float)AddImageWidthSlider.Value,
            Height = (float)AddImageHeightSlider.Value,
            Content = _selectedImagePath
        };

        _pendingEdits.Add(edit);
        _editHistory.Add($"Add Image: {System.IO.Path.GetFileName(_selectedImagePath)} at Page {page}, X:{edit.X:F0}, Y:{edit.Y:F0}");

        StatusText.Text = $"{_pendingEdits.Count} change(s) pending";
    }

    private void FindText_Click(object sender, RoutedEventArgs e)
    {
        if (string.IsNullOrEmpty(_currentPdfPath))
        {
            System.Windows.MessageBox.Show("Please open a PDF first", "No PDF",
                System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Warning);
            return;
        }

        if (string.IsNullOrWhiteSpace(FindSearchBox.Text))
        {
            System.Windows.MessageBox.Show("Please enter text to search for", "Empty Search",
                System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Warning);
            return;
        }

        if (!int.TryParse(FindPageBox.Text, out int page))
        {
            System.Windows.MessageBox.Show("Please enter a valid page number", "Invalid Input",
                System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Warning);
            return;
        }

        try
        {
            StatusText.Text = "Searching for text...";
            var results = _contentEditor.FindTextLocations(_currentPdfPath, page, FindSearchBox.Text, _pdfPassword);

            if (results.Count == 0)
            {
                FindResultsText.Text = $"Text '{FindSearchBox.Text}' not found on page {page}.";
                StatusText.Text = "Text not found";
            }
            else
            {
                var resultText = new System.Text.StringBuilder();
                resultText.AppendLine($"Found {results.Count} occurrence(s) of '{FindSearchBox.Text}' on page {page}:\n");

                for (int i = 0; i < results.Count; i++)
                {
                    var result = results[i];
                    resultText.AppendLine($"=== Match #{i + 1} ===");
                    resultText.AppendLine($"Text: \"{result.Text}\"");
                    resultText.AppendLine($"Position:");
                    resultText.AppendLine($"  X: {result.X:F2}");
                    resultText.AppendLine($"  Y: {result.Y:F2}");
                    resultText.AppendLine($"Size:");
                    resultText.AppendLine($"  Width: {result.Width:F2}");
                    resultText.AppendLine($"  Height: {result.Height:F2}");
                    resultText.AppendLine($"Font:");
                    resultText.AppendLine($"  Size: {result.FontSize:F2}pt");
                    resultText.AppendLine($"  Name: {result.FontName}");
                    if (result.Color != null)
                    {
                        resultText.AppendLine($"Color:");
                        resultText.AppendLine($"  RGB: ({result.Color.GetColorValue()[0]:F2}, {result.Color.GetColorValue()[1]:F2}, {result.Color.GetColorValue()[2]:F2})");
                    }
                    resultText.AppendLine();
                }

                resultText.AppendLine("💡 TIP: Use these values in Delete Text or Add Text sections!");

                FindResultsText.Text = resultText.ToString();
                StatusText.Text = $"Found {results.Count} match(es)";
            }
        }
        catch (Exception ex)
        {
            FindResultsText.Text = $"Error searching: {ex.Message}";
            StatusText.Text = "Search error";
            System.Windows.MessageBox.Show($"Error searching for text:\n{ex.Message}", "Error",
                System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
        }
    }

    private void ReplaceText_Click(object sender, RoutedEventArgs e)
    {
        if (string.IsNullOrEmpty(_currentPdfPath))
        {
            System.Windows.MessageBox.Show("Please open a PDF first", "No PDF",
                System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Warning);
            return;
        }

        if (string.IsNullOrWhiteSpace(FindTextBox.Text) || string.IsNullOrWhiteSpace(ReplaceTextBox.Text))
        {
            System.Windows.MessageBox.Show("Please enter text to find and replace", "Missing Text",
                System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Warning);
            return;
        }

        if (!int.TryParse(ReplacePageBox.Text, out int page))
        {
            System.Windows.MessageBox.Show("Invalid page number", "Error",
                System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Warning);
            return;
        }

        try
        {
            // Find the text to get its properties
            StatusText.Text = "Finding text to replace...";
            var locations = _contentEditor.FindTextLocations(_currentPdfPath, page, FindTextBox.Text, _pdfPassword);

            if (locations.Count == 0)
            {
                System.Windows.MessageBox.Show(
                    $"Text '{FindTextBox.Text}' not found on page {page}.\n\nUse the Find Text feature to verify the text exists.",
                    "Text Not Found",
                    System.Windows.MessageBoxButton.OK,
                    System.Windows.MessageBoxImage.Warning);
                StatusText.Text = "Text not found - cannot replace";
                return;
            }

            // If multiple matches, ask user which one
            int selectedIndex = 0;
            if (locations.Count > 1)
            {
                var result = System.Windows.MessageBox.Show(
                    $"Found {locations.Count} occurrences of '{FindTextBox.Text}' on page {page}.\n\n" +
                    $"Click 'Yes' to replace the FIRST occurrence only.\n" +
                    $"Click 'No' to replace ALL occurrences.\n" +
                    $"Click 'Cancel' to abort.",
                    "Multiple Matches",
                    System.Windows.MessageBoxButton.YesNoCancel,
                    System.Windows.MessageBoxImage.Question);

                if (result == System.Windows.MessageBoxResult.Cancel)
                {
                    StatusText.Text = "Replace cancelled";
                    return;
                }

                if (result == System.Windows.MessageBoxResult.No)
                {
                    // Replace all occurrences
                    foreach (var location in locations)
                    {
                        var edit = new PendingEdit
                        {
                            Type = "ReplaceText",
                            Page = page,
                            X = location.X,
                            Y = location.Y,
                            Width = location.Width,
                            Height = location.Height,
                            Content = ReplaceTextBox.Text,
                            FontName = MapFontNameToStandard(location.FontName),
                            Color = location.Color ?? new iText.Kernel.Colors.DeviceRgb(0, 0, 0),
                            FontSize = location.FontSize
                        };

                        _pendingEdits.Add(edit);
                        _editHistory.Add($"Replace Text: '{FindTextBox.Text}' → '{ReplaceTextBox.Text}' at X:{location.X:F0}, Y:{location.Y:F0}");
                    }

                    StatusText.Text = $"{_pendingEdits.Count} change(s) pending - replacing {locations.Count} occurrence(s)";
                    FindTextBox.Clear();
                    ReplaceTextBox.Clear();
                    return;
                }
            }

            // Replace single (first) occurrence
            var loc = locations[selectedIndex];
            var singleEdit = new PendingEdit
            {
                Type = "ReplaceText",
                Page = page,
                X = loc.X,
                Y = loc.Y,
                Width = loc.Width,
                Height = loc.Height,
                Content = ReplaceTextBox.Text,
                FontName = MapFontNameToStandard(loc.FontName),
                Color = loc.Color ?? new iText.Kernel.Colors.DeviceRgb(0, 0, 0),
                FontSize = loc.FontSize
            };

            _pendingEdits.Add(singleEdit);
            _editHistory.Add($"Replace Text: '{FindTextBox.Text}' → '{ReplaceTextBox.Text}' at X:{loc.X:F0}, Y:{loc.Y:F0}");

            StatusText.Text = $"{_pendingEdits.Count} change(s) pending";
            FindTextBox.Clear();
            ReplaceTextBox.Clear();
        }
        catch (Exception ex)
        {
            System.Windows.MessageBox.Show($"Error finding text: {ex.Message}", "Error",
                System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
            StatusText.Text = "Error during replace";
        }
    }

    private string MapFontNameToStandard(string fontName)
    {
        // Map detected font names to standard PDF fonts
        if (fontName.Contains("Helvetica", StringComparison.OrdinalIgnoreCase))
        {
            if (fontName.Contains("Bold", StringComparison.OrdinalIgnoreCase))
                return iText.IO.Font.Constants.StandardFonts.HELVETICA_BOLD;
            return iText.IO.Font.Constants.StandardFonts.HELVETICA;
        }
        if (fontName.Contains("Times", StringComparison.OrdinalIgnoreCase))
        {
            if (fontName.Contains("Bold", StringComparison.OrdinalIgnoreCase))
                return iText.IO.Font.Constants.StandardFonts.TIMES_BOLD;
            return iText.IO.Font.Constants.StandardFonts.TIMES_ROMAN;
        }
        if (fontName.Contains("Courier", StringComparison.OrdinalIgnoreCase))
        {
            return iText.IO.Font.Constants.StandardFonts.COURIER;
        }

        // Default to Helvetica if unknown
        return iText.IO.Font.Constants.StandardFonts.HELVETICA;
    }

    private void DeleteText_Click(object sender, RoutedEventArgs e)
    {
        if (string.IsNullOrEmpty(_currentPdfPath))
        {
            System.Windows.MessageBox.Show("Please open a PDF first", "No PDF",
                System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Warning);
            return;
        }

        if (string.IsNullOrWhiteSpace(DeleteTextBox.Text))
        {
            System.Windows.MessageBox.Show("Please enter text to delete", "Empty Text",
                System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Warning);
            return;
        }

        if (!int.TryParse(DeletePageBox.Text, out int page))
        {
            System.Windows.MessageBox.Show("Please enter a valid page number", "Invalid Input",
                System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Warning);
            return;
        }

        var edit = new PendingEdit
        {
            Type = "DeleteText",
            Page = page,
            Content = DeleteTextBox.Text,
            X = (float)DeleteXSlider.Value,
            Y = (float)DeleteYSlider.Value,
            Width = (float)DeleteWidthSlider.Value,
            Height = (float)DeleteHeightSlider.Value
        };

        _pendingEdits.Add(edit);
        _editHistory.Add($"Delete Text: '{DeleteTextBox.Text}' on Page {page} (covered at X:{edit.X:F0}, Y:{edit.Y:F0})");

        StatusText.Text = $"{_pendingEdits.Count} change(s) pending";
        DeleteTextBox.Clear();
    }

    private void CoverArea_Click(object sender, RoutedEventArgs e)
    {
        if (string.IsNullOrEmpty(_currentPdfPath))
        {
            System.Windows.MessageBox.Show("Please open a PDF first", "No PDF",
                System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Warning);
            return;
        }

        if (!int.TryParse(CoverPageBox.Text, out int page))
        {
            System.Windows.MessageBox.Show("Please enter a valid page number", "Invalid Input",
                System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Warning);
            return;
        }

        // Get color from combo box
        iText.Kernel.Colors.DeviceRgb color;
        string colorName;
        switch (CoverColorCombo.SelectedIndex)
        {
            case 0: // White
                color = new iText.Kernel.Colors.DeviceRgb(1f, 1f, 1f);
                colorName = "White";
                break;
            case 1: // Black
                color = new iText.Kernel.Colors.DeviceRgb(0, 0, 0);
                colorName = "Black";
                break;
            case 2: // Red
                color = new iText.Kernel.Colors.DeviceRgb(1f, 0, 0);
                colorName = "Red";
                break;
            case 3: // Blue
                color = new iText.Kernel.Colors.DeviceRgb(0, 0, 1f);
                colorName = "Blue";
                break;
            case 4: // Green
                color = new iText.Kernel.Colors.DeviceRgb(0, 0.5f, 0);
                colorName = "Green";
                break;
            case 5: // Yellow
                color = new iText.Kernel.Colors.DeviceRgb(1f, 1f, 0);
                colorName = "Yellow";
                break;
            case 6: // Gray
                color = new iText.Kernel.Colors.DeviceRgb(0.5f, 0.5f, 0.5f);
                colorName = "Gray";
                break;
            case 7: // Light Gray
                color = new iText.Kernel.Colors.DeviceRgb(0.83f, 0.83f, 0.83f);
                colorName = "Light Gray";
                break;
            default:
                color = new iText.Kernel.Colors.DeviceRgb(1f, 1f, 1f);
                colorName = "White";
                break;
        }

        var edit = new PendingEdit
        {
            Type = "CoverArea",
            Page = page,
            X = (float)CoverXSlider.Value,
            Y = (float)CoverYSlider.Value,
            Width = (float)CoverWidthSlider.Value,
            Height = (float)CoverHeightSlider.Value,
            Color = color,
            Opacity = (float)CoverOpacitySlider.Value
        };

        _pendingEdits.Add(edit);
        _editHistory.Add($"Cover Area: Page {page}, {colorName} ({edit.Opacity:P0}), X:{edit.X:F0}, Y:{edit.Y:F0}, Size:{edit.Width:F0}x{edit.Height:F0}");

        StatusText.Text = $"{_pendingEdits.Count} change(s) pending";
    }

    private void ExportPageAsImage_Click(object sender, RoutedEventArgs e)
    {
        if (string.IsNullOrEmpty(_currentPdfPath))
        {
            System.Windows.MessageBox.Show("Please open a PDF first", "No PDF",
                System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Warning);
            return;
        }

        if (!int.TryParse(ExportPageBox.Text, out int page))
        {
            System.Windows.MessageBox.Show("Please enter a valid page number", "Invalid Input",
                System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Warning);
            return;
        }

        try
        {
            // Get DPI from selection
            int dpi = 150;
            switch (ExportDpiCombo.SelectedIndex)
            {
                case 0: dpi = 72; break;
                case 1: dpi = 150; break;
                case 2: dpi = 300; break;
                case 3: dpi = 600; break;
            }

            // Get format from selection
            string extension = ".png";
            string filterName = "PNG Image";
            switch (ExportFormatCombo.SelectedIndex)
            {
                case 0:
                    extension = ".png";
                    filterName = "PNG Image";
                    break;
                case 1:
                    extension = ".jpg";
                    filterName = "JPEG Image";
                    break;
                case 2:
                    extension = ".bmp";
                    filterName = "BMP Image";
                    break;
            }

            // Show save dialog
            var saveDialog = new Microsoft.Win32.SaveFileDialog
            {
                Filter = $"{filterName} (*{extension})|*{extension}",
                Title = "Save Page as Image",
                FileName = $"{System.IO.Path.GetFileNameWithoutExtension(_currentPdfPath)}_page{page}{extension}"
            };

            if (saveDialog.ShowDialog() == true)
            {
                StatusText.Text = $"Exporting page {page} as image...";

                _contentEditor.ExportPageAsImage(_currentPdfPath, page, saveDialog.FileName, dpi, _pdfPassword);

                StatusText.Text = $"Page {page} exported successfully";

                System.Windows.MessageBox.Show(
                    $"Page {page} exported successfully!\n\n" +
                    $"Resolution: {dpi} DPI\n" +
                    $"Format: {extension.ToUpper()}\n\n" +
                    $"Saved to:\n{saveDialog.FileName}",
                    "Export Successful",
                    System.Windows.MessageBoxButton.OK,
                    System.Windows.MessageBoxImage.Information);

                // Ask if user wants to open the image
                var result = System.Windows.MessageBox.Show(
                    "Would you like to open the exported image?",
                    "Open Image",
                    System.Windows.MessageBoxButton.YesNo,
                    System.Windows.MessageBoxImage.Question);

                if (result == System.Windows.MessageBoxResult.Yes)
                {
                    System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                    {
                        FileName = saveDialog.FileName,
                        UseShellExecute = true
                    });
                }
            }
        }
        catch (Exception ex)
        {
            System.Windows.MessageBox.Show($"Error exporting page:\n\n{ex.Message}", "Error",
                System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
            StatusText.Text = "Export failed";
        }
    }

    private void SavePdf_Click(object sender, RoutedEventArgs e)
    {
        if (_pendingEdits.Count == 0)
        {
            System.Windows.MessageBox.Show("No changes to apply", "Info",
                System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Information);
            return;
        }

        var saveDialog = new Microsoft.Win32.SaveFileDialog
        {
            Filter = "PDF files (*.pdf)|*.pdf",
            Title = "Save Edited PDF",
            FileName = System.IO.Path.GetFileNameWithoutExtension(_currentPdfPath) + "_edited.pdf"
        };

        if (saveDialog.ShowDialog() == true)
        {
            try
            {
                var outputPath = saveDialog.FileName;
                var currentPath = _currentPdfPath;
                var tempFiles = new List<string>();

                StatusText.Text = $"Applying {_pendingEdits.Count} changes...";

                int editNumber = 0;
                // Apply each edit sequentially
                foreach (var edit in _pendingEdits)
                {
                    // Create unique temp file name (don't create the file yet)
                    var tempOutput = System.IO.Path.Combine(System.IO.Path.GetTempPath(), $"pdfedit_{Guid.NewGuid()}.pdf");
                    editNumber++;
                    StatusText.Text = $"Applying change {editNumber} of {_pendingEdits.Count}...";

                    try
                    {
                        switch (edit.Type)
                        {
                            case "AddText":
                                _contentEditor.AddTextAtPosition(currentPath!, tempOutput, edit.Page,
                                    edit.Content!, edit.X, edit.Y, edit.Width,
                                    edit.FontName ?? iText.IO.Font.Constants.StandardFonts.HELVETICA,
                                    password: _pdfPassword,
                                    color: edit.Color);
                                break;

                            case "AddImage":
                                _contentEditor.AddImage(currentPath!, tempOutput, edit.Page,
                                    edit.Content!, edit.X, edit.Y, edit.Width, edit.Height, password: _pdfPassword);
                                break;

                            case "ReplaceText":
                                // First, cover the old text with white rectangle
                                var tempCover = System.IO.Path.Combine(System.IO.Path.GetTempPath(), $"pdfedit_{Guid.NewGuid()}.pdf");
                                _contentEditor.RemoveTextAtPosition(currentPath!, tempCover, edit.Page,
                                    edit.X, edit.Y, edit.Width, edit.Height, password: _pdfPassword);

                                // Force GC
                                GC.Collect();
                                GC.WaitForPendingFinalizers();
                                System.Threading.Thread.Sleep(100);

                                // Track temp file
                                if (currentPath != _currentPdfPath)
                                {
                                    tempFiles.Add(currentPath!);
                                }
                                currentPath = tempCover;

                                // Then add the new text at the same position with same formatting
                                _contentEditor.AddTextAtPosition(currentPath!, tempOutput, edit.Page,
                                    edit.Content!, edit.X, edit.Y, edit.FontSize,
                                    edit.FontName ?? iText.IO.Font.Constants.StandardFonts.HELVETICA,
                                    password: _pdfPassword,
                                    color: edit.Color);
                                break;

                            case "DeleteText":
                                // Cover the text area with white rectangle to "delete" it
                                _contentEditor.RemoveTextAtPosition(currentPath!, tempOutput, edit.Page,
                                    edit.X, edit.Y, edit.Width, edit.Height, password: _pdfPassword);
                                break;

                            case "CoverArea":
                                // Cover area with colored rectangle
                                _contentEditor.CoverAreaWithColor(currentPath!, tempOutput, edit.Page,
                                    edit.X, edit.Y, edit.Width, edit.Height,
                                    edit.Color ?? new iText.Kernel.Colors.DeviceRgb(1f, 1f, 1f),
                                    edit.Opacity, password: _pdfPassword);
                                break;

                            case "Erase":
                                _contentEditor.RemoveTextAtPosition(currentPath!, tempOutput, edit.Page,
                                    edit.X, edit.Y, edit.Width, edit.Height, password: _pdfPassword);
                                break;
                        }

                        // Force GC after each edit to release file handles
                        GC.Collect();
                        GC.WaitForPendingFinalizers();

                        // Small delay to ensure file is fully released before next operation
                        System.Threading.Thread.Sleep(150);

                        // Track temp files for cleanup later
                        if (currentPath != _currentPdfPath)
                        {
                            tempFiles.Add(currentPath!);
                        }

                        currentPath = tempOutput;
                    }
                    catch (Exception ex)
                    {
                        StatusText.Text = $"Error on change {editNumber}";
                        System.Windows.MessageBox.Show(
                            $"Error applying edit #{editNumber}:\n\n" +
                            $"Type: {edit.Type}\n" +
                            $"Page: {edit.Page}\n\n" +
                            $"Error: {ex.Message}\n\n" +
                            $"Full details:\n{ex.ToString()}",
                            "Error Applying Changes",
                            System.Windows.MessageBoxButton.OK,
                            System.Windows.MessageBoxImage.Error);

                        // Cleanup on error
                        foreach (var tempFile in tempFiles)
                        {
                            try { if (File.Exists(tempFile)) File.Delete(tempFile); } catch { }
                        }
                        return;
                    }
                }

                // Copy final result
                File.Copy(currentPath!, outputPath, true);

                // Force GC before cleanup
                GC.Collect();
                GC.WaitForPendingFinalizers();
                GC.Collect();

                // Give time for file handles to be released
                System.Threading.Thread.Sleep(200);

                // Clean up all temp files
                foreach (var tempFile in tempFiles)
                {
                    try
                    {
                        if (File.Exists(tempFile))
                        {
                            File.Delete(tempFile);
                        }
                    }
                    catch
                    {
                        // Ignore cleanup errors
                    }
                }

                // Clean up final temp file
                if (currentPath != _currentPdfPath && File.Exists(currentPath))
                {
                    try
                    {
                        File.Delete(currentPath);
                    }
                    catch
                    {
                        // Ignore cleanup errors
                    }
                }

                StatusText.Text = $"Successfully saved: {System.IO.Path.GetFileName(outputPath)}";

                System.Windows.MessageBox.Show(
                    $"PDF saved successfully!\n\n" +
                    $"Applied {_pendingEdits.Count} change(s)\n\n" +
                    $"Saved to:\n{outputPath}",
                    "Success",
                    System.Windows.MessageBoxButton.OK,
                    System.Windows.MessageBoxImage.Information);

                // Clear pending edits
                _pendingEdits.Clear();
                _editHistory.Clear();

                // Optionally load the new file
                var result = System.Windows.MessageBox.Show(
                    "Would you like to continue editing the saved file?",
                    "Continue Editing",
                    System.Windows.MessageBoxButton.YesNo,
                    System.Windows.MessageBoxImage.Question);

                if (result == System.Windows.MessageBoxResult.Yes)
                {
                    _currentPdfPath = outputPath;
                    FileNameText.Text = System.IO.Path.GetFileName(_currentPdfPath);
                }
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Error saving PDF: {ex.Message}\n\n{ex.StackTrace}",
                    "Error", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
            }
        }
    }
}
