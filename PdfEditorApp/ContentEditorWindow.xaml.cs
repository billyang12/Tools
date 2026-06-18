using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace PdfEditorApp;

public partial class ContentEditorWindow : System.Windows.Window
{
    private readonly PdfContentEditor _contentEditor;
    private readonly PdfEditorService _pdfService;
    private string? _currentPdfPath;
    private string? _selectedImagePath;
    private int _currentPage = 1;
    private int _totalPages = 0;
    private double _zoomLevel = 1.0;

    private System.Windows.Point _selectionStart;
    private bool _isSelecting = false;

    public ContentEditorWindow()
    {
        InitializeComponent();
        _contentEditor = new PdfContentEditor();
        _pdfService = new PdfEditorService();
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
            _currentPdfPath = openDialog.FileName;
            _totalPages = _pdfService.GetPageCount(_currentPdfPath);
            _currentPage = 1;

            PageNumberTextBox.Text = _currentPage.ToString();
            TotalPagesLabel.Text = $"/ {_totalPages}";
            PlaceholderText.Visibility = Visibility.Collapsed;

            StatusLabel.Text = $"Loaded: {System.IO.Path.GetFileName(_currentPdfPath)} ({_totalPages} pages)";

            LoadPageElements();
        }
    }

    private void LoadPageElements()
    {
        if (string.IsNullOrEmpty(_currentPdfPath)) return;

        try
        {
            // Load text elements
            var textElements = _contentEditor.ExtractTextElements(_currentPdfPath, _currentPage);
            TextElementsList.ItemsSource = textElements;

            // Load image elements
            var imageElements = _contentEditor.GetImageLocations(_currentPdfPath, _currentPage);
            ImageElementsList.ItemsSource = imageElements;

            StatusLabel.Text = $"Page {_currentPage}: Found {textElements.Count} text elements, {imageElements.Count} images";
        }
        catch (Exception ex)
        {
            System.Windows.MessageBox.Show($"Error loading page elements: {ex.Message}", "Error",
                System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
        }
    }

    private void GoToPage_Click(object sender, RoutedEventArgs e)
    {
        if (int.TryParse(PageNumberTextBox.Text, out int pageNum) &&
            pageNum >= 1 && pageNum <= _totalPages)
        {
            _currentPage = pageNum;
            LoadPageElements();
            StatusLabel.Text = $"Navigated to page {_currentPage}";
        }
        else
        {
            System.Windows.MessageBox.Show($"Please enter a valid page number (1-{_totalPages})",
                "Invalid Page", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Warning);
        }
    }

    private void PreviousPage_Click(object sender, RoutedEventArgs e)
    {
        if (_currentPage > 1)
        {
            _currentPage--;
            PageNumberTextBox.Text = _currentPage.ToString();
            LoadPageElements();
        }
    }

    private void NextPage_Click(object sender, RoutedEventArgs e)
    {
        if (_currentPage < _totalPages)
        {
            _currentPage++;
            PageNumberTextBox.Text = _currentPage.ToString();
            LoadPageElements();
        }
    }

    private void ReplaceText_Click(object sender, RoutedEventArgs e)
    {
        if (string.IsNullOrEmpty(_currentPdfPath) ||
            string.IsNullOrWhiteSpace(FindTextBox.Text) ||
            string.IsNullOrWhiteSpace(ReplaceTextBox.Text))
        {
            System.Windows.MessageBox.Show("Please enter text to find and replace",
                "Input Required", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Warning);
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
                _contentEditor.ReplaceText(_currentPdfPath, saveDialog.FileName, _currentPage,
                    FindTextBox.Text, ReplaceTextBox.Text, (float)FontSizeSlider.Value);

                StatusLabel.Text = $"Text replaced and saved to: {System.IO.Path.GetFileName(saveDialog.FileName)}";
                System.Windows.MessageBox.Show("Text replaced successfully!", "Success",
                    System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Information);

                // Update current PDF to the edited version
                _currentPdfPath = saveDialog.FileName;
                LoadPageElements();
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Error replacing text: {ex.Message}", "Error",
                    System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
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

        var dialog = new AddTextDialog();
        if (dialog.ShowDialog() == true && !string.IsNullOrWhiteSpace(dialog.TextContent))
        {
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
                    _contentEditor.AddTextAtPosition(_currentPdfPath, saveDialog.FileName, _currentPage,
                        dialog.TextContent, dialog.XPosition, dialog.YPosition, (float)FontSizeSlider.Value);

                    StatusLabel.Text = "Text added successfully";
                    System.Windows.MessageBox.Show("Text added successfully!", "Success",
                        System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Information);

                    _currentPdfPath = saveDialog.FileName;
                    LoadPageElements();
                }
                catch (Exception ex)
                {
                    System.Windows.MessageBox.Show($"Error adding text: {ex.Message}", "Error",
                        System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
                }
            }
        }
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
            StatusLabel.Text = $"Image selected: {SelectedImageLabel.Text}";
        }
    }

    private void AddImageAtCursor_Click(object sender, RoutedEventArgs e)
    {
        if (string.IsNullOrEmpty(_currentPdfPath) || string.IsNullOrEmpty(_selectedImagePath))
            return;

        var dialog = new AddImageDialog();
        if (dialog.ShowDialog() == true)
        {
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
                    _contentEditor.AddImage(_currentPdfPath, saveDialog.FileName, _currentPage,
                        _selectedImagePath, dialog.XPosition, dialog.YPosition,
                        dialog.Width, dialog.Height);

                    StatusLabel.Text = "Image added successfully";
                    System.Windows.MessageBox.Show("Image added successfully!", "Success",
                        System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Information);

                    _currentPdfPath = saveDialog.FileName;
                    LoadPageElements();
                }
                catch (Exception ex)
                {
                    System.Windows.MessageBox.Show($"Error adding image: {ex.Message}", "Error",
                        System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
                }
            }
        }
    }

    private void HighlightArea_Click(object sender, RoutedEventArgs e)
    {
        if (string.IsNullOrEmpty(_currentPdfPath))
            return;

        var dialog = new AreaSelectionDialog("Highlight Area");
        if (dialog.ShowDialog() == true)
        {
            var saveDialog = new Microsoft.Win32.SaveFileDialog
            {
                Filter = "PDF files (*.pdf)|*.pdf",
                Title = "Save Edited PDF",
                FileName = System.IO.Path.GetFileNameWithoutExtension(_currentPdfPath) + "_highlighted.pdf"
            };

            if (saveDialog.ShowDialog() == true)
            {
                try
                {
                    _contentEditor.HighlightArea(_currentPdfPath, saveDialog.FileName, _currentPage,
                        dialog.XPosition, dialog.YPosition, dialog.Width, dialog.Height);

                    StatusLabel.Text = "Area highlighted successfully";
                    _currentPdfPath = saveDialog.FileName;
                }
                catch (Exception ex)
                {
                    System.Windows.MessageBox.Show($"Error highlighting: {ex.Message}", "Error",
                        System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
                }
            }
        }
    }

    private void DrawRectangle_Click(object sender, RoutedEventArgs e)
    {
        if (string.IsNullOrEmpty(_currentPdfPath))
            return;

        var dialog = new AreaSelectionDialog("Draw Rectangle");
        if (dialog.ShowDialog() == true)
        {
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
                    _contentEditor.DrawRectangle(_currentPdfPath, saveDialog.FileName, _currentPage,
                        dialog.XPosition, dialog.YPosition, dialog.Width, dialog.Height);

                    StatusLabel.Text = "Rectangle drawn successfully";
                    _currentPdfPath = saveDialog.FileName;
                }
                catch (Exception ex)
                {
                    System.Windows.MessageBox.Show($"Error drawing: {ex.Message}", "Error",
                        System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
                }
            }
        }
    }

    private void EraseArea_Click(object sender, RoutedEventArgs e)
    {
        if (string.IsNullOrEmpty(_currentPdfPath))
            return;

        var dialog = new AreaSelectionDialog("Erase Area");
        if (dialog.ShowDialog() == true)
        {
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
                    _contentEditor.RemoveTextAtPosition(_currentPdfPath, saveDialog.FileName, _currentPage,
                        dialog.XPosition, dialog.YPosition, dialog.Width, dialog.Height);

                    StatusLabel.Text = "Area erased successfully";
                    _currentPdfPath = saveDialog.FileName;
                    LoadPageElements();
                }
                catch (Exception ex)
                {
                    System.Windows.MessageBox.Show($"Error erasing: {ex.Message}", "Error",
                        System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
                }
            }
        }
    }

    private void SavePdf_Click(object sender, RoutedEventArgs e)
    {
        if (string.IsNullOrEmpty(_currentPdfPath))
        {
            System.Windows.MessageBox.Show("No PDF to save", "Info",
                System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Information);
            return;
        }

        System.Windows.MessageBox.Show($"Current PDF: {_currentPdfPath}\n\nEdits are saved immediately when you perform operations.",
            "PDF Info", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Information);
    }

    private void RefreshElements_Click(object sender, RoutedEventArgs e)
    {
        LoadPageElements();
    }

    private void TextElementsList_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (TextElementsList.SelectedItem is PdfContentEditor.TextElement element)
        {
            SelectionLabel.Text = $"Selected: \"{element.Text}\"\nAt: ({element.X:F0}, {element.Y:F0})";
        }
    }

    private void Canvas_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        _isSelecting = true;
        _selectionStart = e.GetPosition(PdfCanvas);

        SelectionRectangle.Visibility = Visibility.Visible;
        Canvas.SetLeft(SelectionRectangle, _selectionStart.X);
        Canvas.SetTop(SelectionRectangle, _selectionStart.Y);
        SelectionRectangle.Width = 0;
        SelectionRectangle.Height = 0;
    }

    private void Canvas_MouseMove(object sender, System.Windows.Input.MouseEventArgs e)
    {
        var currentPos = e.GetPosition(PdfCanvas);
        PositionLabel.Text = $"X: {currentPos.X:F0}, Y: {currentPos.Y:F0}";

        if (_isSelecting && e.LeftButton == MouseButtonState.Pressed)
        {
            var width = Math.Abs(currentPos.X - _selectionStart.X);
            var height = Math.Abs(currentPos.Y - _selectionStart.Y);
            var left = Math.Min(_selectionStart.X, currentPos.X);
            var top = Math.Min(_selectionStart.Y, currentPos.Y);

            Canvas.SetLeft(SelectionRectangle, left);
            Canvas.SetTop(SelectionRectangle, top);
            SelectionRectangle.Width = width;
            SelectionRectangle.Height = height;
        }
    }

    private void Canvas_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
    {
        if (_isSelecting)
        {
            _isSelecting = false;
            var endPos = e.GetPosition(PdfCanvas);

            var width = Math.Abs(endPos.X - _selectionStart.X);
            var height = Math.Abs(endPos.Y - _selectionStart.Y);

            if (width > 5 && height > 5)
            {
                SelectionLabel.Text = $"Selection: ({_selectionStart.X:F0}, {_selectionStart.Y:F0}) - ({endPos.X:F0}, {endPos.Y:F0})\nSize: {width:F0} x {height:F0}";
            }
            else
            {
                SelectionRectangle.Visibility = Visibility.Collapsed;
            }
        }
    }

    private void ZoomIn_Click(object sender, RoutedEventArgs e)
    {
        _zoomLevel = Math.Min(_zoomLevel + 0.1, 3.0);
        ApplyZoom();
    }

    private void ZoomOut_Click(object sender, RoutedEventArgs e)
    {
        _zoomLevel = Math.Max(_zoomLevel - 0.1, 0.5);
        ApplyZoom();
    }

    private void ApplyZoom()
    {
        var transform = new ScaleTransform(_zoomLevel, _zoomLevel);
        PdfCanvas.LayoutTransform = transform;
        ZoomLabel.Text = $"{(_zoomLevel * 100):F0}%";
    }
}
