using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Media.Imaging;

namespace PdfEditorApp;

public partial class MainWindow : System.Windows.Window
{
    public class ThumbnailItem : INotifyPropertyChanged
    {
        private BitmapSource? _thumbnailImage;
        private bool _isLoading = true;
        private string _borderBrush = "#CCCCCC";

        public int PageNumber { get; set; }

        public BitmapSource? ThumbnailImage
        {
            get => _thumbnailImage;
            set
            {
                _thumbnailImage = value;
                OnPropertyChanged();
            }
        }

        public bool IsLoading
        {
            get => _isLoading;
            set
            {
                _isLoading = value;
                OnPropertyChanged();
            }
        }

        public string BorderBrush
        {
            get => _borderBrush;
            set
            {
                _borderBrush = value;
                OnPropertyChanged();
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    private ObservableCollection<ThumbnailItem> _thumbnails = new ObservableCollection<ThumbnailItem>();
    private PdfiumViewer.PdfDocument? _pdfDocument;
    private HashSet<int> _renderedThumbnails = new HashSet<int>();
    private bool _thumbnailsVisible = true;

    public MainWindow()
    {
        InitializeComponent();
        var viewModel = new MainViewModel();
        DataContext = viewModel;

        // Subscribe to PDF loaded event
        viewModel.PropertyChanged += ViewModel_PropertyChanged;

        ThumbnailPanel.ItemsSource = _thumbnails;
        UpdateTitle();
    }

    private void ViewModel_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(MainViewModel.CurrentPdfPath))
        {
            LoadThumbnails();
            UpdateTitle();
        }
        else if (e.PropertyName == nameof(MainViewModel.SelectedPageNumber))
        {
            UpdateSelectedThumbnail();
        }
        else if (e.PropertyName == nameof(MainViewModel.HasUnsavedChanges))
        {
            UpdateTitle();
        }
    }

    private void UpdateTitle()
    {
        var viewModel = DataContext as MainViewModel;
        if (viewModel == null)
        {
            Title = "PDF Editor Pro";
            return;
        }

        var baseTitle = "PDF Editor Pro";
        if (!string.IsNullOrEmpty(viewModel.CurrentFileName))
        {
            baseTitle += $" - {viewModel.CurrentFileName}";
            if (viewModel.HasUnsavedChanges)
            {
                baseTitle += " *";
            }
        }
        Title = baseTitle;
    }

    private void Window_Closing(object? sender, System.ComponentModel.CancelEventArgs e)
    {
        var viewModel = DataContext as MainViewModel;
        if (viewModel != null && viewModel.HasUnsavedChanges)
        {
            var result = System.Windows.MessageBox.Show(
                "You have unsaved changes. Close without saving?",
                "Unsaved Changes",
                System.Windows.MessageBoxButton.YesNo,
                System.Windows.MessageBoxImage.Warning);

            if (result != System.Windows.MessageBoxResult.Yes)
            {
                e.Cancel = true;
            }
        }
    }

    private void LoadThumbnails()
    {
        var viewModel = DataContext as MainViewModel;
        if (viewModel == null || string.IsNullOrEmpty(viewModel.CurrentPdfPath))
        {
            _thumbnails.Clear();
            _pdfDocument?.Dispose();
            _pdfDocument = null;
            _renderedThumbnails.Clear();
            return;
        }

        try
        {
            // Dispose old document and wait for file handles to release
            if (_pdfDocument != null)
            {
                _pdfDocument.Dispose();
                _pdfDocument = null;

                // Force garbage collection to ensure file handles are released
                GC.Collect();
                GC.WaitForPendingFinalizers();

                // Give file system a moment to release the handles
                System.Threading.Thread.Sleep(200);
            }
            _renderedThumbnails.Clear();

            // Load new document
            _pdfDocument = PdfiumViewer.PdfDocument.Load(viewModel.CurrentPdfPath);

            // Create thumbnail items
            _thumbnails.Clear();
            for (int i = 1; i <= _pdfDocument.PageCount; i++)
            {
                _thumbnails.Add(new ThumbnailItem
                {
                    PageNumber = i,
                    IsLoading = true
                });
            }

            // Load visible thumbnails
            RenderVisibleThumbnails();
            UpdateSelectedThumbnail();
        }
        catch (Exception ex)
        {
            System.Windows.MessageBox.Show($"Error loading thumbnails:\n\n{ex.Message}\n\nPath: {viewModel.CurrentPdfPath}", "Error",
                System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
        }
    }

    private void ThumbnailScrollViewer_ScrollChanged(object sender, System.Windows.Controls.ScrollChangedEventArgs e)
    {
        RenderVisibleThumbnails();
    }

    private void RenderVisibleThumbnails()
    {
        if (_pdfDocument == null || !_thumbnailsVisible) return;

        var scrollViewer = ThumbnailScrollViewer;
        var verticalOffset = scrollViewer.VerticalOffset;
        var viewportHeight = scrollViewer.ViewportHeight;

        // Each thumbnail is roughly 210px tall
        int thumbnailHeight = 210;
        int firstVisibleIndex = Math.Max(0, (int)(verticalOffset / thumbnailHeight) - 2);
        int lastVisibleIndex = Math.Min(_thumbnails.Count - 1, (int)((verticalOffset + viewportHeight) / thumbnailHeight) + 2);

        for (int i = firstVisibleIndex; i <= lastVisibleIndex; i++)
        {
            if (!_renderedThumbnails.Contains(i))
            {
                _renderedThumbnails.Add(i);
                RenderThumbnail(i);
            }
        }
    }

    private void RenderThumbnail(int index)
    {
        if (_pdfDocument == null || index < 0 || index >= _thumbnails.Count) return;

        var thumbnailItem = _thumbnails[index];

        System.Threading.Tasks.Task.Run(() =>
        {
            try
            {
                int dpi = 96;
                var pageSize = _pdfDocument.PageSizes[index];
                var width = (int)(pageSize.Width * dpi / 72.0);
                var height = (int)(pageSize.Height * dpi / 72.0);

                // Scale to fit width of 130px
                double scale = 130.0 / width;
                width = 130;
                height = (int)(height * scale);

                using (var image = _pdfDocument.Render(index, width, height, dpi, dpi, false))
                using (var bitmap = new System.Drawing.Bitmap(image))
                {
                    var bitmapSource = ConvertToBitmapSource(bitmap);

                    Dispatcher.Invoke(() =>
                    {
                        thumbnailItem.ThumbnailImage = bitmapSource;
                        thumbnailItem.IsLoading = false;
                    });
                }
            }
            catch
            {
                Dispatcher.Invoke(() =>
                {
                    thumbnailItem.IsLoading = false;
                });
            }
        });
    }

    private BitmapSource ConvertToBitmapSource(System.Drawing.Bitmap bitmap)
    {
        var hBitmap = bitmap.GetHbitmap();
        try
        {
            var bitmapSource = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(
                hBitmap,
                IntPtr.Zero,
                System.Windows.Int32Rect.Empty,
                BitmapSizeOptions.FromEmptyOptions());

            bitmapSource.Freeze();
            return bitmapSource;
        }
        finally
        {
            DeleteObject(hBitmap);
        }
    }

    [System.Runtime.InteropServices.DllImport("gdi32.dll")]
    private static extern bool DeleteObject(IntPtr hObject);

    private void UpdateSelectedThumbnail()
    {
        var viewModel = DataContext as MainViewModel;
        if (viewModel == null) return;

        int selectedPage = viewModel.SelectedPageNumber;

        foreach (var thumbnail in _thumbnails)
        {
            thumbnail.BorderBrush = thumbnail.PageNumber == selectedPage ? "#2196F3" : "#CCCCCC";
        }
    }

    private void Thumbnail_Click(object sender, System.Windows.Input.MouseButtonEventArgs e)
    {
        if (sender is System.Windows.FrameworkElement element && element.Tag is int pageNumber)
        {
            var viewModel = DataContext as MainViewModel;
            if (viewModel != null)
            {
                viewModel.SelectedPageNumber = pageNumber;
            }
        }
    }

    private void ToggleThumbnails_Click(object sender, System.Windows.RoutedEventArgs e)
    {
        try
        {
            // Prevent event from bubbling up
            e.Handled = true;

            _thumbnailsVisible = !_thumbnailsVisible;

            var thumbnailColumn = MainContentGrid.ColumnDefinitions[0];
            var splitterColumn = MainContentGrid.ColumnDefinitions[1];

            if (_thumbnailsVisible)
            {
                // Show thumbnails
                thumbnailColumn.Width = new System.Windows.GridLength(180, System.Windows.GridUnitType.Pixel);
                thumbnailColumn.MinWidth = 100;
                splitterColumn.Width = System.Windows.GridLength.Auto;
                ToggleThumbnailsButton.Content = "◀";
                ToggleThumbnailsButton.ToolTip = "Hide thumbnails";
                ThumbnailScrollViewer.Visibility = System.Windows.Visibility.Visible;

                // Render visible thumbnails when showing
                RenderVisibleThumbnails();
            }
            else
            {
                // Hide thumbnails but keep button visible
                thumbnailColumn.Width = new System.Windows.GridLength(35, System.Windows.GridUnitType.Pixel);
                thumbnailColumn.MinWidth = 35;
                splitterColumn.Width = new System.Windows.GridLength(0);
                ToggleThumbnailsButton.Content = "▶";
                ToggleThumbnailsButton.ToolTip = "Show thumbnails";
                ThumbnailScrollViewer.Visibility = System.Windows.Visibility.Collapsed;
            }
        }
        catch (Exception ex)
        {
            System.Windows.MessageBox.Show($"Error toggling thumbnails: {ex.Message}", "Error",
                System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
        }
    }

    protected override void OnClosed(EventArgs e)
    {
        base.OnClosed(e);
        _pdfDocument?.Dispose();
    }

    private void Exit_Click(object sender, System.Windows.RoutedEventArgs e)
    {
        System.Windows.Application.Current.Shutdown();
    }

    private void About_Click(object sender, System.Windows.RoutedEventArgs e)
    {
        System.Windows.MessageBox.Show(
            "PDF Editor Pro v2.0\n\n" +
            "A comprehensive PDF editing application built with C# .NET 10 and WPF.\n\n" +
            "Features:\n" +
            "• Edit PDF content (text and images)\n" +
            "• Replace text in PDFs\n" +
            "• Add/replace images\n" +
            "• Merge multiple PDFs\n" +
            "• Split PDFs into individual pages\n" +
            "• Rotate, delete pages\n" +
            "• Add watermarks\n" +
            "• Encrypt PDFs with password\n" +
            "• Extract text from pages\n" +
            "• Convert images to PDF\n" +
            "• Highlight and erase areas\n\n" +
            "Built with iText7 library",
            "About PDF Editor Pro",
            System.Windows.MessageBoxButton.OK,
            System.Windows.MessageBoxImage.Information);
    }

    private void ContentEditor_Click(object sender, System.Windows.RoutedEventArgs e)
    {
        // Use the simple, reliable editor that always works
        var simpleEditor = new SimplePdfEditor();
        simpleEditor.Show();
    }

    private void ExportPageAsImage_Click(object sender, System.Windows.RoutedEventArgs e)
    {
        var viewModel = DataContext as MainViewModel;
        if (viewModel == null || string.IsNullOrEmpty(viewModel.CurrentPdfPath))
        {
            System.Windows.MessageBox.Show("Please open a PDF first", "No PDF Loaded",
                System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Warning);
            return;
        }

        try
        {
            var contentEditor = new PdfContentEditor();
            var totalPages = new PdfEditorService().GetPageCount(viewModel.CurrentPdfPath);

            // Ask for page number
            var pageDialog = new PageNumberDialog
            {
                Owner = this,
                TotalPages = totalPages
            };
            pageDialog.SetMessage($"Enter page number (1-{totalPages}):");

            if (pageDialog.ShowDialog() != true)
            {
                return;
            }

            int page = pageDialog.PageNumber;

            // Ask for format and DPI
            var exportDialog = new ExportImageDialog();
            if (exportDialog.ShowDialog() != true)
            {
                return;
            }

            var saveDialog = new Microsoft.Win32.SaveFileDialog
            {
                Filter = exportDialog.GetFileFilter(),
                Title = "Save Page as Image",
                FileName = $"{System.IO.Path.GetFileNameWithoutExtension(viewModel.CurrentPdfPath)}_page{page}{exportDialog.GetExtension()}"
            };

            if (saveDialog.ShowDialog() == true)
            {
                contentEditor.ExportPageAsImage(viewModel.CurrentPdfPath, page, saveDialog.FileName, exportDialog.GetDpi());

                System.Windows.MessageBox.Show(
                    $"Page {page} exported successfully!\n\nSaved to:\n{saveDialog.FileName}",
                    "Export Successful",
                    System.Windows.MessageBoxButton.OK,
                    System.Windows.MessageBoxImage.Information);

                // Ask to open
                var result = System.Windows.MessageBox.Show("Open the exported image?", "Open Image",
                    System.Windows.MessageBoxButton.YesNo, System.Windows.MessageBoxImage.Question);

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
            System.Windows.MessageBox.Show($"Error exporting page:\n\n{ex.Message}", "Export Error",
                System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
        }
    }

    private void ExportAllPagesAsImages_Click(object sender, System.Windows.RoutedEventArgs e)
    {
        var viewModel = DataContext as MainViewModel;
        if (viewModel == null || string.IsNullOrEmpty(viewModel.CurrentPdfPath))
        {
            System.Windows.MessageBox.Show("Please open a PDF first", "No PDF Loaded",
                System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Warning);
            return;
        }

        try
        {
            var contentEditor = new PdfContentEditor();
            var pdfService = new PdfEditorService();
            var totalPages = pdfService.GetPageCount(viewModel.CurrentPdfPath);

            if (totalPages == 0)
            {
                System.Windows.MessageBox.Show("PDF has no pages", "Export Error",
                    System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Warning);
                return;
            }

            // Ask for export settings
            var exportDialog = new ExportImageDialog();
            if (exportDialog.ShowDialog() != true)
            {
                return;
            }

            // Ask for output folder
            var folderDialog = new System.Windows.Forms.FolderBrowserDialog
            {
                Description = $"Select folder to save {totalPages} image(s)",
                UseDescriptionForTitle = true
            };

            if (folderDialog.ShowDialog() != System.Windows.Forms.DialogResult.OK)
            {
                return;
            }

            var outputFolder = folderDialog.SelectedPath;
            var baseFileName = System.IO.Path.GetFileNameWithoutExtension(viewModel.CurrentPdfPath);
            var extension = exportDialog.GetExtension();
            var dpi = exportDialog.GetDpi();

            // Show progress
            var progressWindow = new System.Windows.Window
            {
                Title = "Exporting Pages",
                Width = 400,
                Height = 150,
                WindowStartupLocation = System.Windows.WindowStartupLocation.CenterOwner,
                Owner = this,
                ResizeMode = System.Windows.ResizeMode.NoResize
            };

            var progressPanel = new System.Windows.Controls.StackPanel { Margin = new System.Windows.Thickness(20) };
            var progressText = new System.Windows.Controls.TextBlock
            {
                Text = "Exporting pages...",
                FontSize = 14,
                Margin = new System.Windows.Thickness(0, 10, 0, 20)
            };
            var progressBar = new System.Windows.Controls.ProgressBar
            {
                Height = 25,
                Minimum = 0,
                Maximum = totalPages
            };

            progressPanel.Children.Add(progressText);
            progressPanel.Children.Add(progressBar);
            progressWindow.Content = progressPanel;

            progressWindow.Show();

            int successCount = 0;
            int failCount = 0;

            for (int page = 1; page <= totalPages; page++)
            {
                try
                {
                    var outputPath = System.IO.Path.Combine(outputFolder, $"{baseFileName}_page{page:D3}{extension}");
                    contentEditor.ExportPageAsImage(viewModel.CurrentPdfPath, page, outputPath, dpi);
                    successCount++;

                    progressText.Text = $"Exported page {page} of {totalPages}";
                    progressBar.Value = page;

                    // Allow UI to update
                    System.Windows.Application.Current.Dispatcher.Invoke(() => { }, System.Windows.Threading.DispatcherPriority.Background);
                }
                catch
                {
                    failCount++;
                }
            }

            progressWindow.Close();

            var message = $"Export complete!\n\n" +
                         $"Total pages: {totalPages}\n" +
                         $"Successful: {successCount}\n" +
                         $"Failed: {failCount}\n\n" +
                         $"Saved to:\n{outputFolder}";

            System.Windows.MessageBox.Show(message, "Export Complete",
                System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Information);

            // Ask to open folder
            var result = System.Windows.MessageBox.Show("Open the output folder?", "Open Folder",
                System.Windows.MessageBoxButton.YesNo, System.Windows.MessageBoxImage.Question);

            if (result == System.Windows.MessageBoxResult.Yes)
            {
                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                {
                    FileName = outputFolder,
                    UseShellExecute = true
                });
            }
        }
        catch (Exception ex)
        {
            System.Windows.MessageBox.Show($"Error exporting pages:\n\n{ex.Message}", "Export Error",
                System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
        }
    }

    private void ExportAllPagesAsOneImage_Click(object sender, System.Windows.RoutedEventArgs e)
    {
        var viewModel = DataContext as MainViewModel;
        if (viewModel == null || string.IsNullOrEmpty(viewModel.CurrentPdfPath))
        {
            System.Windows.MessageBox.Show("Please open a PDF first", "No PDF Loaded",
                System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Warning);
            return;
        }

        try
        {
            var contentEditor = new PdfContentEditor();
            var pdfService = new PdfEditorService();
            var totalPages = pdfService.GetPageCount(viewModel.CurrentPdfPath);

            if (totalPages == 0)
            {
                System.Windows.MessageBox.Show("PDF has no pages", "Export Error",
                    System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Warning);
                return;
            }

            // Ask for export settings
            var exportDialog = new ExportImageDialog();
            if (exportDialog.ShowDialog() != true)
            {
                return;
            }

            var extension = exportDialog.GetExtension();
            var dpi = exportDialog.GetDpi();

            // Show save dialog
            var saveDialog = new Microsoft.Win32.SaveFileDialog
            {
                Filter = exportDialog.GetFileFilter(),
                Title = "Save Combined Image",
                FileName = $"{System.IO.Path.GetFileNameWithoutExtension(viewModel.CurrentPdfPath)}_all_pages{extension}"
            };

            if (saveDialog.ShowDialog() != true)
            {
                return;
            }

            // Show progress window
            var progressWindow = new System.Windows.Window
            {
                Title = "Exporting All Pages as One Image",
                Width = 400,
                Height = 150,
                WindowStartupLocation = System.Windows.WindowStartupLocation.CenterOwner,
                Owner = this,
                ResizeMode = System.Windows.ResizeMode.NoResize
            };

            var progressPanel = new System.Windows.Controls.StackPanel { Margin = new System.Windows.Thickness(20) };
            var progressText = new System.Windows.Controls.TextBlock
            {
                Text = $"Combining {totalPages} page(s) into one image...",
                FontSize = 14,
                Margin = new System.Windows.Thickness(0, 10, 0, 20),
                TextWrapping = System.Windows.TextWrapping.Wrap
            };
            var progressBar = new System.Windows.Controls.ProgressBar
            {
                Height = 25,
                IsIndeterminate = true
            };

            progressPanel.Children.Add(progressText);
            progressPanel.Children.Add(progressBar);
            progressWindow.Content = progressPanel;

            progressWindow.Show();

            // Run export in background to keep UI responsive
            System.Threading.Tasks.Task.Run(() =>
            {
                try
                {
                    contentEditor.ExportAllPagesAsOneImage(viewModel.CurrentPdfPath, saveDialog.FileName, dpi);

                    // Update UI on main thread
                    Dispatcher.Invoke(() =>
                    {
                        progressWindow.Close();

                        var fileInfo = new System.IO.FileInfo(saveDialog.FileName);
                        var fileSizeMB = fileInfo.Length / (1024.0 * 1024.0);

                        System.Windows.MessageBox.Show(
                            $"All {totalPages} page(s) combined into one image!\n\n" +
                            $"Resolution: {dpi} DPI\n" +
                            $"Format: {extension.ToUpper()}\n" +
                            $"File Size: {fileSizeMB:F2} MB\n\n" +
                            $"Saved to:\n{saveDialog.FileName}",
                            "Export Successful",
                            System.Windows.MessageBoxButton.OK,
                            System.Windows.MessageBoxImage.Information);

                        // Ask to open
                        var result = System.Windows.MessageBox.Show("Open the exported image?", "Open Image",
                            System.Windows.MessageBoxButton.YesNo, System.Windows.MessageBoxImage.Question);

                        if (result == System.Windows.MessageBoxResult.Yes)
                        {
                            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                            {
                                FileName = saveDialog.FileName,
                                UseShellExecute = true
                            });
                        }
                    });
                }
                catch (Exception ex)
                {
                    // Update UI on main thread
                    Dispatcher.Invoke(() =>
                    {
                        progressWindow.Close();
                        System.Windows.MessageBox.Show($"Error exporting combined image:\n\n{ex.Message}", "Export Error",
                            System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
                    });
                }
            });
        }
        catch (Exception ex)
        {
            System.Windows.MessageBox.Show($"Error preparing export:\n\n{ex.Message}", "Export Error",
                System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
        }
    }

    private void ExportToWord_Click(object sender, System.Windows.RoutedEventArgs e)
    {
        var viewModel = DataContext as MainViewModel;
        if (viewModel == null || string.IsNullOrEmpty(viewModel.CurrentPdfPath))
        {
            System.Windows.MessageBox.Show("Please open a PDF first", "No PDF Loaded",
                System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Warning);
            return;
        }

        try
        {
            var contentEditor = new PdfContentEditor();
            var pdfService = new PdfEditorService();
            var totalPages = pdfService.GetPageCount(viewModel.CurrentPdfPath);

            if (totalPages == 0)
            {
                System.Windows.MessageBox.Show("PDF has no pages", "Export Error",
                    System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Warning);
                return;
            }

            // Ask for DPI/quality settings
            var dpiDialog = new ExportImageDialog();
            if (dpiDialog.ShowDialog() != true)
            {
                return;
            }

            int dpi = dpiDialog.GetDpi();

            // Show save dialog
            var saveDialog = new Microsoft.Win32.SaveFileDialog
            {
                Filter = "Word Document (*.docx)|*.docx",
                Title = "Save as Word Document",
                FileName = $"{System.IO.Path.GetFileNameWithoutExtension(viewModel.CurrentPdfPath)}.docx"
            };

            if (saveDialog.ShowDialog() != true)
            {
                return;
            }

            // Show progress window
            var progressWindow = new System.Windows.Window
            {
                Title = "Exporting to Word",
                Width = 400,
                Height = 150,
                WindowStartupLocation = System.Windows.WindowStartupLocation.CenterOwner,
                Owner = this,
                ResizeMode = System.Windows.ResizeMode.NoResize
            };

            var progressPanel = new System.Windows.Controls.StackPanel { Margin = new System.Windows.Thickness(20) };
            var progressText = new System.Windows.Controls.TextBlock
            {
                Text = $"Converting {totalPages} page(s) to Word document...",
                FontSize = 14,
                Margin = new System.Windows.Thickness(0, 10, 0, 20),
                TextWrapping = System.Windows.TextWrapping.Wrap
            };
            var progressBar = new System.Windows.Controls.ProgressBar
            {
                Height = 25,
                IsIndeterminate = true
            };

            progressPanel.Children.Add(progressText);
            progressPanel.Children.Add(progressBar);
            progressWindow.Content = progressPanel;

            progressWindow.Show();

            // Run export in background
            System.Threading.Tasks.Task.Run(() =>
            {
                try
                {
                    contentEditor.ExportToWord(viewModel.CurrentPdfPath, saveDialog.FileName, dpi);

                    // Update UI on main thread
                    Dispatcher.Invoke(() =>
                    {
                        progressWindow.Close();

                        var fileInfo = new System.IO.FileInfo(saveDialog.FileName);
                        var fileSizeKB = fileInfo.Length / 1024.0;

                        System.Windows.MessageBox.Show(
                            $"PDF exported to Word successfully!\n\n" +
                            $"Pages: {totalPages}\n" +
                            $"Resolution: {dpi} DPI\n" +
                            $"File Size: {fileSizeKB:F1} KB\n\n" +
                            $"Note: Each PDF page is embedded as an image to preserve exact appearance.\n\n" +
                            $"Saved to:\n{saveDialog.FileName}",
                            "Export Successful",
                            System.Windows.MessageBoxButton.OK,
                            System.Windows.MessageBoxImage.Information);

                        // Ask to open
                        var result = System.Windows.MessageBox.Show("Open the Word document?", "Open Document",
                            System.Windows.MessageBoxButton.YesNo, System.Windows.MessageBoxImage.Question);

                        if (result == System.Windows.MessageBoxResult.Yes)
                        {
                            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                            {
                                FileName = saveDialog.FileName,
                                UseShellExecute = true
                            });
                        }
                    });
                }
                catch (Exception ex)
                {
                    // Update UI on main thread
                    Dispatcher.Invoke(() =>
                    {
                        progressWindow.Close();
                        System.Windows.MessageBox.Show($"Error exporting to Word:\n\n{ex.Message}", "Export Error",
                            System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
                    });
                }
            });
        }
        catch (Exception ex)
        {
            System.Windows.MessageBox.Show($"Error preparing export:\n\n{ex.Message}", "Export Error",
                System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
        }
    }

    private void ReorderPages_Click(object sender, System.Windows.RoutedEventArgs e)
    {
        var viewModel = DataContext as MainViewModel;
        if (viewModel == null || string.IsNullOrEmpty(viewModel.CurrentPdfPath))
        {
            System.Windows.MessageBox.Show("Please open a PDF first", "No PDF Loaded",
                System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Warning);
            return;
        }

        try
        {
            var pdfService = new PdfEditorService();
            var totalPages = pdfService.GetPageCount(viewModel.CurrentPdfPath);

            if (totalPages < 2)
            {
                System.Windows.MessageBox.Show("PDF must have at least 2 pages to reorder", "Cannot Reorder",
                    System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Warning);
                return;
            }

            // Show reorder dialog with PDF path for preview
            var reorderDialog = new PageReorderDialog(viewModel.CurrentPdfPath, totalPages, null)
            {
                Owner = this
            };

            if (reorderDialog.ShowDialog() != true)
            {
                return;
            }

            // Check if order actually changed
            bool orderChanged = false;
            for (int i = 0; i < reorderDialog.NewPageOrder.Length; i++)
            {
                if (reorderDialog.NewPageOrder[i] != i + 1)
                {
                    orderChanged = true;
                    break;
                }
            }

            if (!orderChanged)
            {
                System.Windows.MessageBox.Show("Page order was not changed.", "No Changes",
                    System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Information);
                return;
            }

            // Show save dialog
            var saveDialog = new Microsoft.Win32.SaveFileDialog
            {
                Filter = "PDF Files (*.pdf)|*.pdf",
                Title = "Save Reordered PDF",
                FileName = $"{System.IO.Path.GetFileNameWithoutExtension(viewModel.CurrentPdfPath)}_reordered.pdf"
            };

            if (saveDialog.ShowDialog() != true)
            {
                return;
            }

            // Perform the reordering
            var contentEditor = new PdfContentEditor();
            contentEditor.ReorderPages(viewModel.CurrentPdfPath, saveDialog.FileName, reorderDialog.NewPageOrder);

            System.Windows.MessageBox.Show(
                $"Page reordering successful!\n\n" +
                $"Total pages: {totalPages}\n\n" +
                $"Saved to:\n{saveDialog.FileName}",
                "Reorder Complete",
                System.Windows.MessageBoxButton.OK,
                System.Windows.MessageBoxImage.Information);

            // Ask to open
            var result = System.Windows.MessageBox.Show("Open the reordered PDF?", "Open PDF",
                System.Windows.MessageBoxButton.YesNo, System.Windows.MessageBoxImage.Question);

            if (result == System.Windows.MessageBoxResult.Yes)
            {
                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                {
                    FileName = saveDialog.FileName,
                    UseShellExecute = true
                });
            }
        }
        catch (Exception ex)
        {
            System.Windows.MessageBox.Show($"Error reordering pages:\n\n{ex.Message}", "Reorder Error",
                System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
        }
    }
}