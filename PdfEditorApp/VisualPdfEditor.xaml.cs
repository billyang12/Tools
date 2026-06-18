using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using PdfiumViewer;

namespace PdfEditorApp;

public partial class VisualPdfEditor : System.Windows.Window
{
    private PdfDocument? _pdfDocument;
    private string? _currentPdfPath;
    private string? _pdfPassword;
    private int _currentPage = 0;
    private int _totalPages = 0;
    private double _zoomLevel = 1.0;
    private PdfContentEditor _contentEditor;
    private ObservableCollection<string> _editHistory;

    // For click-to-edit
    private System.Windows.Point _mouseDownPoint;
    private bool _isMouseDown = false;
    private System.Windows.Shapes.Rectangle? _currentSelection;
    private string? _selectedImagePath;

    // For tracking edits
    private class EditOperation
    {
        public string Type { get; set; } = "";
        public int Page { get; set; }
        public double X { get; set; }
        public double Y { get; set; }
        public double Width { get; set; }
        public double Height { get; set; }
        public string? Content { get; set; }
    }

    private System.Collections.Generic.List<EditOperation> _pendingEdits;

    public VisualPdfEditor()
    {
        InitializeComponent();
        _contentEditor = new PdfContentEditor();
        _editHistory = new ObservableCollection<string>();
        _pendingEdits = new System.Collections.Generic.List<EditOperation>();
        EditHistoryList.ItemsSource = _editHistory;

        // Try to help PdfiumViewer find the native DLL
        TrySetupPdfiumPath();
    }

    private void TrySetupPdfiumPath()
    {
        try
        {
            // Get the application directory
            var appDir = System.AppDomain.CurrentDomain.BaseDirectory;
            var x64Path = System.IO.Path.Combine(appDir, "x64");
            var pdfiumDll = System.IO.Path.Combine(x64Path, "pdfium.dll");

            // Check if the DLL exists
            if (File.Exists(pdfiumDll))
            {
                // Add x64 directory to PATH so PdfiumViewer can find it
                var currentPath = Environment.GetEnvironmentVariable("PATH") ?? "";
                if (!currentPath.Contains(x64Path))
                {
                    Environment.SetEnvironmentVariable("PATH", currentPath + ";" + x64Path);
                }

                StatusText.Text = "PDF rendering libraries loaded successfully";
            }
            else
            {
                StatusText.Text = "Warning: PDF rendering DLL not found - visual editing may not work";
            }
        }
        catch
        {
            // Ignore errors in setup
        }
    }

    private void Window_Loaded(object sender, RoutedEventArgs e)
    {
        StatusText.Text = "Ready - Click 'Open PDF' to start editing";
    }

    private void OpenPdf_Click(object sender, RoutedEventArgs e)
    {
        var openDialog = new Microsoft.Win32.OpenFileDialog
        {
            Filter = "PDF files (*.pdf)|*.pdf",
            Title = "Open PDF for Visual Editing"
        };

        if (openDialog.ShowDialog() == true)
        {
            try
            {
                _currentPdfPath = openDialog.FileName;
                StatusText.Text = "Loading PDF...";

                // Dispose previous document
                _pdfDocument?.Dispose();

                // Try to load PDF
                try
                {
                    _pdfDocument = PdfDocument.Load(_currentPdfPath);
                }
                catch (Exception pdfEx)
                {
                    System.Windows.MessageBox.Show(
                        $"Could not load PDF. This might be due to:\n\n" +
                        $"1. Missing native PDF rendering library\n" +
                        $"2. Corrupted PDF file\n" +
                        $"3. Encrypted/password-protected PDF\n\n" +
                        $"Error: {pdfEx.Message}\n\n" +
                        $"Tip: Try using the basic editor instead (works without PDF rendering).",
                        "PDF Loading Error",
                        System.Windows.MessageBoxButton.OK,
                        System.Windows.MessageBoxImage.Error);
                    StatusText.Text = "Failed to load PDF - try basic editor";
                    return;
                }

                _totalPages = _pdfDocument.PageCount;
                _currentPage = 0;

                // Update UI
                TotalPagesText.Text = _totalPages.ToString();
                FileNameText.Text = System.IO.Path.GetFileName(_currentPdfPath);
                FileSizeText.Text = $"Size: {new FileInfo(_currentPdfPath).Length / 1024} KB";

                // Enable controls
                SaveButton.IsEnabled = true;
                PrevButton.IsEnabled = false;
                NextButton.IsEnabled = _totalPages > 1;

                // Clear edits
                _pendingEdits.Clear();
                _editHistory.Clear();

                // Render first page
                RenderCurrentPage();

                StatusText.Text = $"Loaded: {System.IO.Path.GetFileName(_currentPdfPath)} - {_totalPages} pages";
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show(
                    $"Error opening PDF: {ex.Message}\n\n" +
                    $"Stack trace:\n{ex.StackTrace}",
                    "Error",
                    System.Windows.MessageBoxButton.OK,
                    System.Windows.MessageBoxImage.Error);
                StatusText.Text = "Error opening PDF";
            }
        }
    }

    private void RenderCurrentPage()
    {
        if (_pdfDocument == null) return;

        try
        {
            // Render PDF page to image
            var dpi = (int)(96 * _zoomLevel);
            using var image = _pdfDocument.Render(_currentPage, dpi, dpi, false);

            // Convert to WPF BitmapImage
            using var memory = new MemoryStream();
            image.Save(memory, System.Drawing.Imaging.ImageFormat.Png);
            memory.Position = 0;

            var bitmapImage = new BitmapImage();
            bitmapImage.BeginInit();
            bitmapImage.StreamSource = memory;
            bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
            bitmapImage.EndInit();
            bitmapImage.Freeze();

            PdfImage.Source = bitmapImage;

            // Update canvas size to match image
            EditingCanvas.Width = bitmapImage.PixelWidth;
            EditingCanvas.Height = bitmapImage.PixelHeight;

            // Update page info
            CurrentPageText.Text = (_currentPage + 1).ToString();
            var pageSize = _pdfDocument.PageSizes[_currentPage];
            PageSizeText.Text = $"Page Size: {pageSize.Width:F0} x {pageSize.Height:F0}";

            // Update navigation buttons
            PrevButton.IsEnabled = _currentPage > 0;
            NextButton.IsEnabled = _currentPage < _totalPages - 1;

            // Clear selection
            ClearSelection();
        }
        catch (Exception ex)
        {
            System.Windows.MessageBox.Show($"Error rendering page: {ex.Message}", "Error",
                System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
        }
    }

    private void PreviousPage_Click(object sender, RoutedEventArgs e)
    {
        if (_currentPage > 0)
        {
            _currentPage--;
            RenderCurrentPage();
        }
    }

    private void NextPage_Click(object sender, RoutedEventArgs e)
    {
        if (_currentPage < _totalPages - 1)
        {
            _currentPage++;
            RenderCurrentPage();
        }
    }

    private void ZoomIn_Click(object sender, RoutedEventArgs e)
    {
        _zoomLevel = Math.Min(_zoomLevel + 0.25, 3.0);
        ZoomText.Text = $"{(_zoomLevel * 100):F0}%";
        RenderCurrentPage();
    }

    private void ZoomOut_Click(object sender, RoutedEventArgs e)
    {
        _zoomLevel = Math.Max(_zoomLevel - 0.25, 0.5);
        ZoomText.Text = $"{(_zoomLevel * 100):F0}%";
        RenderCurrentPage();
    }

    private void EditMode_Changed(object sender, RoutedEventArgs e)
    {
        UpdateCursorAndStatus();
    }

    private void UpdateCursorAndStatus()
    {
        if (TextModeRadio.IsChecked == true)
        {
            PdfImage.Cursor = System.Windows.Input.Cursors.IBeam;
            StatusText.Text = "Text Edit Mode - Click on text to edit or click anywhere to add new text";
        }
        else if (ImageModeRadio.IsChecked == true)
        {
            PdfImage.Cursor = System.Windows.Input.Cursors.Cross;
            StatusText.Text = "Image Mode - Click where you want to place an image";
        }
        else if (EraseModeRadio.IsChecked == true)
        {
            PdfImage.Cursor = System.Windows.Input.Cursors.No;
            StatusText.Text = "Erase Mode - Click and drag to select area to erase";
        }
        else
        {
            PdfImage.Cursor = System.Windows.Input.Cursors.Arrow;
            StatusText.Text = "Select Mode - Click and drag to select content";
        }
    }

    private void PdfImage_MouseDown(object sender, MouseButtonEventArgs e)
    {
        _mouseDownPoint = e.GetPosition(PdfImage);
        _isMouseDown = true;

        if (TextModeRadio.IsChecked == true)
        {
            // Text edit mode - prompt for text
            ShowTextInputDialog(_mouseDownPoint);
        }
        else if (ImageModeRadio.IsChecked == true)
        {
            // Image mode - add image at clicked position
            AddImageAtPosition(_mouseDownPoint);
        }
    }

    private void PdfImage_MouseMove(object sender, System.Windows.Input.MouseEventArgs e)
    {
        var pos = e.GetPosition(PdfImage);

        // Convert screen coordinates to PDF coordinates
        var pdfX = pos.X / _zoomLevel;
        var pdfY = pos.Y / _zoomLevel;

        MousePositionText.Text = $"X: {pdfX:F0}, Y: {pdfY:F0}";

        if (_isMouseDown && (EraseModeRadio.IsChecked == true || SelectModeRadio.IsChecked == true))
        {
            // Draw selection rectangle
            DrawSelectionRectangle(_mouseDownPoint, pos);
        }
    }

    private void PdfImage_MouseUp(object sender, MouseButtonEventArgs e)
    {
        if (!_isMouseDown) return;

        _isMouseDown = false;
        var endPoint = e.GetPosition(PdfImage);

        if (EraseModeRadio.IsChecked == true)
        {
            // Erase content in selected area
            EraseSelectedArea(_mouseDownPoint, endPoint);
        }
    }

    private void EditingCanvas_MouseDown(object sender, MouseButtonEventArgs e)
    {
        PdfImage_MouseDown(sender, e);
    }

    private void EditingCanvas_MouseMove(object sender, System.Windows.Input.MouseEventArgs e)
    {
        PdfImage_MouseMove(sender, e);
    }

    private void EditingCanvas_MouseUp(object sender, MouseButtonEventArgs e)
    {
        PdfImage_MouseUp(sender, e);
    }

    private void ShowTextInputDialog(System.Windows.Point position)
    {
        var dialog = new TextInputDialog();
        dialog.FontSize = FontSizeSlider.Value;

        if (dialog.ShowDialog() == true && !string.IsNullOrWhiteSpace(dialog.TextContent))
        {
            // Add text at clicked position
            var pdfX = (float)(position.X / _zoomLevel);
            var pdfY = (float)(position.Y / _zoomLevel);

            // Convert screen Y to PDF Y (PDF Y is from bottom)
            var pageSize = _pdfDocument!.PageSizes[_currentPage];
            var pdfYFromBottom = (float)pageSize.Height - pdfY;

            var edit = new EditOperation
            {
                Type = "AddText",
                Page = _currentPage + 1,
                X = pdfX,
                Y = pdfYFromBottom,
                Content = dialog.TextContent
            };

            _pendingEdits.Add(edit);
            _editHistory.Add($"Added text '{dialog.TextContent}' at ({pdfX:F0}, {pdfYFromBottom:F0})");

            // Visual feedback
            DrawTextMarker(position, dialog.TextContent);

            StatusText.Text = $"Text added - {_pendingEdits.Count} pending edit(s)";
        }
    }

    private void DrawTextMarker(System.Windows.Point position, string text)
    {
        var textBlock = new TextBlock
        {
            Text = text,
            FontSize = FontSizeSlider.Value * _zoomLevel,
            Foreground = GetCurrentTextColor(),
            Background = new SolidColorBrush(System.Windows.Media.Color.FromArgb(128, 255, 255, 0))
        };

        Canvas.SetLeft(textBlock, position.X);
        Canvas.SetTop(textBlock, position.Y);
        EditingCanvas.Children.Add(textBlock);
    }

    private System.Windows.Media.Brush GetCurrentTextColor()
    {
        if (RedTextRadio.IsChecked == true) return System.Windows.Media.Brushes.Red;
        if (BlueTextRadio.IsChecked == true) return System.Windows.Media.Brushes.Blue;
        return System.Windows.Media.Brushes.Black;
    }

    private void AddImageAtPosition(System.Windows.Point position)
    {
        if (string.IsNullOrEmpty(_selectedImagePath))
        {
            System.Windows.MessageBox.Show("Please select an image first using 'Select & Add Image' button",
                "No Image Selected", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Information);
            return;
        }

        var dialog = new AddImageDialog();
        if (dialog.ShowDialog() == true)
        {
            var pdfX = (float)(position.X / _zoomLevel);
            var pdfY = (float)(position.Y / _zoomLevel);

            var pageSize = _pdfDocument!.PageSizes[_currentPage];
            var pdfYFromBottom = (float)pageSize.Height - pdfY;

            var edit = new EditOperation
            {
                Type = "AddImage",
                Page = _currentPage + 1,
                X = pdfX,
                Y = pdfYFromBottom,
                Width = dialog.Width,
                Height = dialog.Height,
                Content = _selectedImagePath
            };

            _pendingEdits.Add(edit);
            _editHistory.Add($"Added image at ({pdfX:F0}, {pdfYFromBottom:F0})");

            // Visual feedback
            DrawImageMarker(position, dialog.Width, dialog.Height);

            StatusText.Text = $"Image added - {_pendingEdits.Count} pending edit(s)";
        }
    }

    private void DrawImageMarker(System.Windows.Point position, float width, float height)
    {
        var rect = new System.Windows.Shapes.Rectangle
        {
            Width = width * _zoomLevel,
            Height = height * _zoomLevel,
            Stroke = System.Windows.Media.Brushes.Blue,
            StrokeThickness = 2,
            StrokeDashArray = new DoubleCollection { 5, 3 },
            Fill = new SolidColorBrush(System.Windows.Media.Color.FromArgb(50, 0, 0, 255))
        };

        Canvas.SetLeft(rect, position.X);
        Canvas.SetTop(rect, position.Y);
        EditingCanvas.Children.Add(rect);
    }

    private void DrawSelectionRectangle(System.Windows.Point start, System.Windows.Point end)
    {
        if (_currentSelection != null)
        {
            EditingCanvas.Children.Remove(_currentSelection);
        }

        var x = Math.Min(start.X, end.X);
        var y = Math.Min(start.Y, end.Y);
        var width = Math.Abs(end.X - start.X);
        var height = Math.Abs(end.Y - start.Y);

        _currentSelection = new System.Windows.Shapes.Rectangle
        {
            Width = width,
            Height = height,
            Stroke = System.Windows.Media.Brushes.Red,
            StrokeThickness = 2,
            StrokeDashArray = new DoubleCollection { 3, 2 },
            Fill = new SolidColorBrush(System.Windows.Media.Color.FromArgb(30, 255, 0, 0))
        };

        Canvas.SetLeft(_currentSelection, x);
        Canvas.SetTop(_currentSelection, y);
        EditingCanvas.Children.Add(_currentSelection);

        SelectionText.Text = $"Selected: {width:F0} x {height:F0}";
    }

    private void EraseSelectedArea(System.Windows.Point start, System.Windows.Point end)
    {
        var pdfX = (float)(Math.Min(start.X, end.X) / _zoomLevel);
        var pdfY = (float)(Math.Min(start.Y, end.Y) / _zoomLevel);
        var width = (float)(Math.Abs(end.X - start.X) / _zoomLevel);
        var height = (float)(Math.Abs(end.Y - start.Y) / _zoomLevel);

        if (width < 5 || height < 5) return; // Ignore tiny selections

        var pageSize = _pdfDocument!.PageSizes[_currentPage];
        var pdfYFromBottom = (float)pageSize.Height - pdfY - height;

        var edit = new EditOperation
        {
            Type = "Erase",
            Page = _currentPage + 1,
            X = pdfX,
            Y = pdfYFromBottom,
            Width = width,
            Height = height
        };

        _pendingEdits.Add(edit);
        _editHistory.Add($"Erased area at ({pdfX:F0}, {pdfYFromBottom:F0})");

        StatusText.Text = $"Area erased - {_pendingEdits.Count} pending edit(s)";

        ClearSelection();
    }

    private void ClearSelection()
    {
        if (_currentSelection != null)
        {
            EditingCanvas.Children.Remove(_currentSelection);
            _currentSelection = null;
        }
        EditingCanvas.Children.Clear();
        SelectionText.Text = "Nothing selected";
    }

    private void AddTextBox_Click(object sender, RoutedEventArgs e)
    {
        TextModeRadio.IsChecked = true;
        System.Windows.MessageBox.Show("Click anywhere on the PDF to add text", "Add Text",
            System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Information);
    }

    private void SelectAndAddImage_Click(object sender, RoutedEventArgs e)
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
            ImageModeRadio.IsChecked = true;
            StatusText.Text = "Image selected - Click on PDF to place it";
        }
    }

    private void HighlightText_Click(object sender, RoutedEventArgs e)
    {
        System.Windows.MessageBox.Show("Select an area first using Select mode, then click this button",
            "Highlight", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Information);
    }

    private void UnderlineText_Click(object sender, RoutedEventArgs e)
    {
        System.Windows.MessageBox.Show("Select text first using Select mode, then click this button",
            "Underline", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Information);
    }

    private void AddWatermark_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new TextInputDialog { Title = "Add Watermark" };
        if (dialog.ShowDialog() == true && !string.IsNullOrWhiteSpace(dialog.TextContent))
        {
            _editHistory.Add($"Watermark '{dialog.TextContent}' will be added on save");
            System.Windows.MessageBox.Show("Watermark will be applied when you save the PDF",
                "Watermark", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Information);
        }
    }

    private void ExportPageAsImage_Click(object sender, RoutedEventArgs e)
    {
        if (string.IsNullOrEmpty(_currentPdfPath))
        {
            System.Windows.MessageBox.Show("Please open a PDF first", "No PDF",
                System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Warning);
            return;
        }

        // Create a dialog to ask for export settings
        var exportDialog = new Window
        {
            Title = "Export Page as Image",
            Width = 400,
            Height = 320,
            WindowStartupLocation = WindowStartupLocation.CenterOwner,
            Owner = this,
            ResizeMode = ResizeMode.NoResize
        };

        var mainPanel = new StackPanel { Margin = new Thickness(20) };

        // Page number
        mainPanel.Children.Add(new TextBlock { Text = "Page Number:", Margin = new Thickness(0, 5, 0, 2) });
        var pageBox = new System.Windows.Controls.TextBox { Text = (_currentPage + 1).ToString() };
        mainPanel.Children.Add(pageBox);

        // DPI
        mainPanel.Children.Add(new TextBlock { Text = "Image Quality (DPI):", Margin = new Thickness(0, 15, 0, 2) });
        var dpiCombo = new System.Windows.Controls.ComboBox { SelectedIndex = 1 };
        dpiCombo.Items.Add(new System.Windows.Controls.ComboBoxItem { Content = "72 DPI (Screen)" });
        dpiCombo.Items.Add(new System.Windows.Controls.ComboBoxItem { Content = "150 DPI (Standard)" });
        dpiCombo.Items.Add(new System.Windows.Controls.ComboBoxItem { Content = "300 DPI (High Quality)" });
        dpiCombo.Items.Add(new System.Windows.Controls.ComboBoxItem { Content = "600 DPI (Print Quality)" });
        mainPanel.Children.Add(dpiCombo);

        // Format
        mainPanel.Children.Add(new TextBlock { Text = "Format:", Margin = new Thickness(0, 15, 0, 2) });
        var formatCombo = new System.Windows.Controls.ComboBox { SelectedIndex = 0 };
        formatCombo.Items.Add(new System.Windows.Controls.ComboBoxItem { Content = "PNG" });
        formatCombo.Items.Add(new System.Windows.Controls.ComboBoxItem { Content = "JPEG" });
        formatCombo.Items.Add(new System.Windows.Controls.ComboBoxItem { Content = "BMP" });
        mainPanel.Children.Add(formatCombo);

        // Buttons
        var buttonPanel = new StackPanel
        {
            Orientation = System.Windows.Controls.Orientation.Horizontal,
            HorizontalAlignment = System.Windows.HorizontalAlignment.Right,
            Margin = new Thickness(0, 20, 0, 0)
        };

        var okButton = new System.Windows.Controls.Button
        {
            Content = "Export",
            Width = 80,
            Height = 30,
            Margin = new Thickness(5, 0, 0, 0)
        };
        okButton.Click += (s, args) => exportDialog.DialogResult = true;

        var cancelButton = new System.Windows.Controls.Button
        {
            Content = "Cancel",
            Width = 80,
            Height = 30,
            Margin = new Thickness(5, 0, 0, 0)
        };
        cancelButton.Click += (s, args) => exportDialog.Close();

        buttonPanel.Children.Add(okButton);
        buttonPanel.Children.Add(cancelButton);
        mainPanel.Children.Add(buttonPanel);

        exportDialog.Content = mainPanel;

        if (exportDialog.ShowDialog() == true)
        {
            if (!int.TryParse(pageBox.Text, out int page))
            {
                System.Windows.MessageBox.Show("Please enter a valid page number", "Invalid Input",
                    System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Warning);
                return;
            }

            try
            {
                // Get DPI from selection
                int dpi = 150;
                switch (dpiCombo.SelectedIndex)
                {
                    case 0: dpi = 72; break;
                    case 1: dpi = 150; break;
                    case 2: dpi = 300; break;
                    case 3: dpi = 600; break;
                }

                // Get format from selection
                string extension = ".png";
                string filterName = "PNG Image";
                switch (formatCombo.SelectedIndex)
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
                    _contentEditor.ExportPageAsImage(_currentPdfPath, page, saveDialog.FileName, dpi, _pdfPassword);

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
            }
        }
    }

    private void SavePdf_Click(object sender, RoutedEventArgs e)
    {
        if (_pendingEdits.Count == 0)
        {
            System.Windows.MessageBox.Show("No edits to save", "Info",
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

                // Apply each edit
                foreach (var edit in _pendingEdits)
                {
                    var tempOutput = System.IO.Path.GetTempFileName() + ".pdf";

                    switch (edit.Type)
                    {
                        case "AddText":
                            _contentEditor.AddTextAtPosition(currentPath!, tempOutput, edit.Page,
                                edit.Content!, (float)edit.X, (float)edit.Y, (float)FontSizeSlider.Value);
                            break;

                        case "AddImage":
                            _contentEditor.AddImage(currentPath!, tempOutput, edit.Page,
                                edit.Content!, (float)edit.X, (float)edit.Y, (float)edit.Width, (float)edit.Height);
                            break;

                        case "Erase":
                            _contentEditor.RemoveTextAtPosition(currentPath!, tempOutput, edit.Page,
                                (float)edit.X, (float)edit.Y, (float)edit.Width, (float)edit.Height);
                            break;
                    }

                    if (currentPath != _currentPdfPath)
                    {
                        File.Delete(currentPath);
                    }
                    currentPath = tempOutput;
                }

                // Copy final result
                File.Copy(currentPath!, outputPath, true);
                if (currentPath != _currentPdfPath)
                {
                    File.Delete(currentPath);
                }

                StatusText.Text = $"Saved: {System.IO.Path.GetFileName(outputPath)}";
                System.Windows.MessageBox.Show($"PDF saved successfully!\n\nSaved to:\n{outputPath}",
                    "Success", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Information);

                // Optionally open the saved file
                var result = System.Windows.MessageBox.Show("Would you like to open the edited PDF?",
                    "Open File", System.Windows.MessageBoxButton.YesNo, System.Windows.MessageBoxImage.Question);

                if (result == System.Windows.MessageBoxResult.Yes)
                {
                    _currentPdfPath = outputPath;
                    _pdfDocument?.Dispose();
                    _pdfDocument = PdfDocument.Load(_currentPdfPath);
                    _pendingEdits.Clear();
                    EditingCanvas.Children.Clear();
                    RenderCurrentPage();
                }
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Error saving PDF: {ex.Message}", "Error",
                    System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
            }
        }
    }

    protected override void OnClosed(EventArgs e)
    {
        _pdfDocument?.Dispose();
        base.OnClosed(e);
    }
}
