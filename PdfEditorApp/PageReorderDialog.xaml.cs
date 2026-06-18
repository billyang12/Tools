using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace PdfEditorApp;

public partial class PageReorderDialog : Window
{
    public class PageItem : INotifyPropertyChanged
    {
        private int _position;
        private BitmapSource? _thumbnailImage;
        private bool _isLoading = true;

        public int Position
        {
            get => _position;
            set
            {
                _position = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(DisplayText));
            }
        }

        public int OriginalPageNumber { get; set; }
        public string Description { get; set; } = "";

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

        public string DisplayText => $"Position {Position} (Page {OriginalPageNumber})";

        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    private readonly string _pdfPath;
    private readonly string? _password;
    private readonly int _totalPages;
    private readonly HashSet<int> _renderedPages = new HashSet<int>();
    private PdfiumViewer.PdfDocument? _pdfDocument;

    public ObservableCollection<PageItem> Pages { get; set; }
    public int[] NewPageOrder { get; private set; } = Array.Empty<int>();

    public PageReorderDialog(string pdfPath, int totalPages, string? password = null)
    {
        InitializeComponent();

        _pdfPath = pdfPath;
        _totalPages = totalPages;
        _password = password;

        Pages = new ObservableCollection<PageItem>();
        for (int i = 1; i <= totalPages; i++)
        {
            Pages.Add(new PageItem
            {
                Position = i,
                OriginalPageNumber = i,
                Description = $"Page {i}",
                IsLoading = true
            });
        }

        PageListBox.ItemsSource = Pages;
        ThumbnailsPanel.ItemsSource = Pages;

        Loaded += PageReorderDialog_Loaded;
        Closing += PageReorderDialog_Closing;
    }

    private void PageReorderDialog_Loaded(object sender, RoutedEventArgs e)
    {
        try
        {
            // Load PDF document
            if (!string.IsNullOrEmpty(_password))
            {
                _pdfDocument = PdfiumViewer.PdfDocument.Load(_pdfPath, _password);
            }
            else
            {
                _pdfDocument = PdfiumViewer.PdfDocument.Load(_pdfPath);
            }

            LoadingText.Visibility = Visibility.Visible;

            // Start loading visible thumbnails
            RenderVisibleThumbnails();
        }
        catch (Exception ex)
        {
            System.Windows.MessageBox.Show($"Error loading PDF:\n\n{ex.Message}", "Error",
                System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
        }
    }

    private void PageReorderDialog_Closing(object? sender, CancelEventArgs e)
    {
        _pdfDocument?.Dispose();
    }

    private void PreviewScrollViewer_ScrollChanged(object sender, System.Windows.Controls.ScrollChangedEventArgs e)
    {
        // Load thumbnails as user scrolls
        RenderVisibleThumbnails();
    }

    private void PageListBox_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
    {
        // Could add auto-scroll to selected page in preview if desired
    }

    private void RenderVisibleThumbnails()
    {
        if (_pdfDocument == null) return;

        // Calculate which items are visible in the scroll viewer
        var scrollViewer = PreviewScrollViewer;
        var verticalOffset = scrollViewer.VerticalOffset;
        var viewportHeight = scrollViewer.ViewportHeight;

        // Rough calculation: each thumbnail is about 210 pixels tall (200 + margins)
        // and we have ~4 thumbnails per row (in a 650px wide panel)
        int thumbnailHeight = 210;
        int thumbnailsPerRow = Math.Max(1, (int)((scrollViewer.ViewportWidth - 20) / 160));

        int firstVisibleRow = Math.Max(0, (int)(verticalOffset / thumbnailHeight) - 1);
        int lastVisibleRow = (int)((verticalOffset + viewportHeight) / thumbnailHeight) + 2;

        int firstVisibleIndex = firstVisibleRow * thumbnailsPerRow;
        int lastVisibleIndex = Math.Min(Pages.Count - 1, (lastVisibleRow + 1) * thumbnailsPerRow);

        // Render visible thumbnails
        for (int i = firstVisibleIndex; i <= lastVisibleIndex && i < Pages.Count; i++)
        {
            var pageItem = Pages[i];
            if (!_renderedPages.Contains(i))
            {
                _renderedPages.Add(i);
                RenderThumbnail(pageItem, i);
            }
        }

        // Check if all thumbnails are loaded
        if (_renderedPages.Count >= Pages.Count)
        {
            LoadingText.Visibility = Visibility.Collapsed;
        }
    }

    private void RenderThumbnail(PageItem pageItem, int index)
    {
        if (_pdfDocument == null) return;

        System.Threading.Tasks.Task.Run(() =>
        {
            try
            {
                // Render at low resolution for thumbnails (96 DPI)
                int dpi = 96;
                var pageSize = _pdfDocument.PageSizes[pageItem.OriginalPageNumber - 1];
                var width = (int)(pageSize.Width * dpi / 72.0);
                var height = (int)(pageSize.Height * dpi / 72.0);

                // Scale to fit thumbnail width of 140px
                double scale = 140.0 / width;
                width = 140;
                height = (int)(height * scale);

                using (var image = _pdfDocument.Render(pageItem.OriginalPageNumber - 1, width, height, dpi, dpi, false))
                using (var bitmap = new System.Drawing.Bitmap(image))
                {
                    // Convert to WPF BitmapSource
                    var bitmapSource = ConvertToBitmapSource(bitmap);

                    Dispatcher.Invoke(() =>
                    {
                        pageItem.ThumbnailImage = bitmapSource;
                        pageItem.IsLoading = false;
                    });
                }
            }
            catch
            {
                Dispatcher.Invoke(() =>
                {
                    pageItem.IsLoading = false;
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
                Int32Rect.Empty,
                BitmapSizeOptions.FromEmptyOptions());

            bitmapSource.Freeze(); // Make it thread-safe
            return bitmapSource;
        }
        finally
        {
            DeleteObject(hBitmap);
        }
    }

    [System.Runtime.InteropServices.DllImport("gdi32.dll")]
    private static extern bool DeleteObject(IntPtr hObject);

    private void UpdatePositionNumbers()
    {
        for (int i = 0; i < Pages.Count; i++)
        {
            Pages[i].Position = i + 1;
        }
        PageListBox.Items.Refresh();
    }

    private void MoveUp_Click(object sender, RoutedEventArgs e)
    {
        var selectedIndex = PageListBox.SelectedIndex;
        if (selectedIndex > 0)
        {
            var item = Pages[selectedIndex];
            Pages.RemoveAt(selectedIndex);
            Pages.Insert(selectedIndex - 1, item);
            UpdatePositionNumbers();
            PageListBox.SelectedIndex = selectedIndex - 1;
        }
    }

    private void MoveDown_Click(object sender, RoutedEventArgs e)
    {
        var selectedIndex = PageListBox.SelectedIndex;
        if (selectedIndex >= 0 && selectedIndex < Pages.Count - 1)
        {
            var item = Pages[selectedIndex];
            Pages.RemoveAt(selectedIndex);
            Pages.Insert(selectedIndex + 1, item);
            UpdatePositionNumbers();
            PageListBox.SelectedIndex = selectedIndex + 1;
        }
    }

    private void MoveTo_Click(object sender, RoutedEventArgs e)
    {
        var selectedIndex = PageListBox.SelectedIndex;
        if (selectedIndex < 0)
        {
            System.Windows.MessageBox.Show("Please select a page first", "No Selection",
                System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Warning);
            return;
        }

        var dialog = new PageNumberDialog
        {
            Owner = this,
            TotalPages = Pages.Count
        };
        dialog.SetMessage($"Move page to position (1-{Pages.Count}):");

        if (dialog.ShowDialog() == true)
        {
            var newPosition = dialog.PageNumber - 1; // Convert to 0-based index
            if (newPosition >= 0 && newPosition < Pages.Count && newPosition != selectedIndex)
            {
                var item = Pages[selectedIndex];
                Pages.RemoveAt(selectedIndex);
                Pages.Insert(newPosition, item);
                UpdatePositionNumbers();
                PageListBox.SelectedIndex = newPosition;
            }
        }
    }

    private void Save_Click(object sender, RoutedEventArgs e)
    {
        // Build the new page order array
        NewPageOrder = new int[Pages.Count];
        for (int i = 0; i < Pages.Count; i++)
        {
            NewPageOrder[i] = Pages[i].OriginalPageNumber;
        }

        DialogResult = true;
        Close();
    }

    private void Cancel_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
        Close();
    }
}
