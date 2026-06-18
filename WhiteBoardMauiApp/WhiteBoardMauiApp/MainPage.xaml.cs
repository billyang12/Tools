using System.Text.Json;

namespace WhiteBoardMauiApp;

public partial class MainPage : ContentPage
{
    private readonly WhiteboardDrawable _drawable = new();
    private readonly Dictionary<string, Color> _colors = new()
    {
        ["Black"] = Colors.Black,
        ["Blue"] = Colors.Blue,
        ["Red"] = Colors.Red,
        ["Green"] = Colors.Green,
        ["Orange"] = Colors.Orange,
        ["Purple"] = Colors.Purple,
        ["Gray"] = Colors.Gray,
        ["White"] = Colors.White,
        ["Transparent"] = Colors.Transparent
    };

    private WhiteboardTool _activeTool = WhiteboardTool.Pen;
    private Color _selectedDrawColor = Colors.Black;
    private Color _selectedBackgroundColor = Colors.White;
    private float _lineWidth = 4;
    private float _eraserSize = 20;
    private StrokeElement? _activeStroke;

    public MainPage()
    {
        InitializeComponent();

        CanvasView.Drawable = _drawable;
        InitializePickers();
        UpdateSizeLabels();
    }

    private void InitializePickers()
    {
        ToolPicker.ItemsSource = Enum.GetNames<WhiteboardTool>().ToList();
        ToolPicker.SelectedIndex = 0;

        var colorNames = _colors.Keys.ToList();
        DrawColorPicker.ItemsSource = colorNames;
        DrawColorPicker.SelectedItem = "Black";

        BackgroundColorPicker.ItemsSource = colorNames;
        BackgroundColorPicker.SelectedItem = "White";
    }

    private void OnToolChanged(object? sender, EventArgs e)
    {
        if (ToolPicker.SelectedItem is string toolName && Enum.TryParse(toolName, out WhiteboardTool tool))
        {
            _activeTool = tool;
            if (_activeTool != WhiteboardTool.Eraser)
            {
                _drawable.EraserPreviewCenter = null;
                CanvasView.Invalidate();
            }
        }
    }

    private async void OnExitClicked(object? sender, EventArgs e)
    {
        var shouldExit = await DisplayAlert("Exit", "Close the app now?", "Yes", "No");
        if (!shouldExit)
        {
            return;
        }

#if ANDROID
        Microsoft.Maui.ApplicationModel.Platform.CurrentActivity?.FinishAffinity();
#elif IOS
        await DisplayAlert("Exit", "iOS does not allow apps to close programmatically.", "OK");
#else
        Application.Current?.Quit();
#endif
    }

    private async void OnAboutClicked(object? sender, EventArgs e)
    {
        var version = AppInfo.Current.VersionString;
        await DisplayAlert("About", $"Whiteboard\nVersion: {version}\nDeveloper: Bill Yang", "OK");
    }

    private void OnDrawColorChanged(object? sender, EventArgs e)
    {
        if (DrawColorPicker.SelectedItem is string colorName && _colors.TryGetValue(colorName, out var color))
        {
            _selectedDrawColor = color;
        }
    }

    private void OnBackgroundColorChanged(object? sender, EventArgs e)
    {
        if (BackgroundColorPicker.SelectedItem is string colorName && _colors.TryGetValue(colorName, out var color))
        {
            _selectedBackgroundColor = color;
        }
    }

    private void OnLineWidthChanged(object? sender, ValueChangedEventArgs e)
    {
        _lineWidth = (float)e.NewValue;
        UpdateSizeLabels();
    }

    private void OnEraserSizeChanged(object? sender, ValueChangedEventArgs e)
    {
        _eraserSize = (float)e.NewValue;
        _drawable.EraserPreviewSize = _eraserSize;
        UpdateSizeLabels();
        CanvasView.Invalidate();
    }

    private void UpdateSizeLabels()
    {
        LineWidthLabel.Text = _lineWidth.ToString("0");
        EraserSizeLabel.Text = _eraserSize.ToString("0");
    }

    private void OnSetBackgroundClicked(object? sender, EventArgs e)
    {
        _drawable.HasBackgroundImage = false;
        BackgroundImageView.Source = null;
        _drawable.BackgroundColor = _selectedBackgroundColor;
        CanvasView.Invalidate();
    }

    private void OnToggleToolsClicked(object? sender, EventArgs e)
    {
        ToolsPanel.IsVisible = !ToolsPanel.IsVisible;
        if (sender is ToolbarItem toolbarItem)
        {
            toolbarItem.Text = ToolsPanel.IsVisible ? "Hide Tools" : "Show Tools";
        }
    }

    private void OnClearClicked(object? sender, EventArgs e)
    {
        _drawable.BackgroundColor = _selectedBackgroundColor;
        _drawable.HasBackgroundImage = false;
        _drawable.FillRegions.Clear();
        _drawable.Strokes.Clear();
        _drawable.Shapes.Clear();
        _drawable.PreviewShape = null;
        BackgroundImageView.Source = null;
        CanvasView.Invalidate();
    }

    private void OnCanvasStartInteraction(object? sender, TouchEventArgs e)
    {
        var point = e.Touches.FirstOrDefault();

        if (_activeTool == WhiteboardTool.Fill)
        {
            if (_drawable.TryFillAtPoint(point, _selectedDrawColor))
            {
                CanvasView.Invalidate();
            }

            return;
        }

        if (_activeTool is WhiteboardTool.Pen or WhiteboardTool.Eraser)
        {
            var stroke = new StrokeElement
            {
                Color = _selectedDrawColor,
                IsEraser = _activeTool == WhiteboardTool.Eraser,
                Thickness = _activeTool == WhiteboardTool.Eraser ? _eraserSize : _lineWidth
            };
            stroke.Points.Add(point);
            _drawable.Strokes.Add(stroke);
            _activeStroke = stroke;

            if (_activeTool == WhiteboardTool.Eraser)
            {
                _drawable.EraserPreviewCenter = point;
                _drawable.EraserPreviewSize = _eraserSize;
            }
        }
        else
        {
            _drawable.PreviewShape = new ShapeElement
            {
                Start = point,
                End = point,
                ShapeType = _activeTool,
                Color = _selectedDrawColor,
                Thickness = _lineWidth
            };
        }

        CanvasView.Invalidate();
    }

    private void OnCanvasDragInteraction(object? sender, TouchEventArgs e)
    {
        if (_activeTool == WhiteboardTool.Fill)
        {
            return;
        }

        var point = e.Touches.FirstOrDefault();

        if (_activeStroke is not null)
        {
            _activeStroke.Points.Add(point);

            if (_activeStroke.IsEraser)
            {
                _drawable.EraserPreviewCenter = point;
                _drawable.EraserPreviewSize = _activeStroke.Thickness;
            }
        }

        if (_drawable.PreviewShape is not null)
        {
            _drawable.PreviewShape.End = point;
        }

        CanvasView.Invalidate();
    }

    private void OnCanvasEndInteraction(object? sender, TouchEventArgs e)
    {
        if (_activeTool == WhiteboardTool.Fill)
        {
            return;
        }

        var point = e.Touches.FirstOrDefault();

        if (_activeStroke is not null)
        {
            _activeStroke.Points.Add(point);

             if (_activeStroke.IsEraser)
            {
                _drawable.EraserPreviewCenter = point;
                _drawable.EraserPreviewSize = _activeStroke.Thickness;
            }

            _activeStroke = null;
        }

        if (_drawable.PreviewShape is not null)
        {
            _drawable.PreviewShape.End = point;
            _drawable.Shapes.Add(_drawable.PreviewShape);
            _drawable.PreviewShape = null;
        }

        if (_activeTool == WhiteboardTool.Eraser)
        {
            _drawable.EraserPreviewCenter = null;
        }

        CanvasView.Invalidate();
    }

    private async void OnSaveClicked(object? sender, EventArgs e)
    {
        var formatChoice = await DisplayActionSheet("Save format", "Cancel", null, "PNG", "JPEG");
        if (formatChoice is not "PNG" and not "JPEG")
        {
            return;
        }

        if (formatChoice == "JPEG" && _drawable.BackgroundColor.Alpha <= 0f && !_drawable.HasBackgroundImage)
        {
            await DisplayAlert("Save format", "Transparent background is only supported by PNG. Please choose PNG.", "OK");
            return;
        }

        var defaultFileName = formatChoice == "JPEG" ? "drawing.jpg" : "drawing.png";
        var fileName = await DisplayPromptAsync("Save Whiteboard", "Enter file name", "Save", "Cancel", initialValue: defaultFileName);
        if (string.IsNullOrWhiteSpace(fileName))
        {
            return;
        }

        var extension = formatChoice == "JPEG" ? ".jpg" : ".png";
        if (!fileName.EndsWith(extension, StringComparison.OrdinalIgnoreCase))
        {
            fileName += extension;
        }

        var filePath = Path.Combine(FileSystem.Current.AppDataDirectory, fileName);
        var screenshot = await CanvasContainer.CaptureAsync();

        if (screenshot is null)
        {
            await DisplayAlert("Save Failed", "Unable to capture canvas image.", "OK");
            return;
        }

        await using var source = await screenshot.OpenReadAsync();
        await using var target = File.Create(filePath);
        await source.CopyToAsync(target);
        await DisplayAlert("Saved", $"Saved to:\n{filePath}", "OK");
    }

    private async void OnOpenClicked(object? sender, EventArgs e)
    {
        try
        {
            var directory = FileSystem.Current.AppDataDirectory;
            var files = Directory
                .EnumerateFiles(directory)
                .Where(path =>
                {
                    var ext = Path.GetExtension(path);
                    return ext.Equals(".png", StringComparison.OrdinalIgnoreCase) ||
                           ext.Equals(".jpg", StringComparison.OrdinalIgnoreCase) ||
                           ext.Equals(".jpeg", StringComparison.OrdinalIgnoreCase) ||
                           ext.Equals(".wbd", StringComparison.OrdinalIgnoreCase) ||
                           ext.Equals(".json", StringComparison.OrdinalIgnoreCase);
                })
                .OrderByDescending(File.GetLastWriteTime)
                .ToList();

            if (files.Count == 0)
            {
                await DisplayAlert("Open", $"No supported files found in:\n{directory}", "OK");
                return;
            }

            var fileNames = files.Select(Path.GetFileName).ToArray();
            var selectedName = await DisplayActionSheet("Open file", "Cancel", null, fileNames);
            if (string.IsNullOrWhiteSpace(selectedName) || selectedName == "Cancel")
            {
                return;
            }

            var selectedPath = files.First(path => string.Equals(Path.GetFileName(path), selectedName, StringComparison.OrdinalIgnoreCase));
            var extension = Path.GetExtension(selectedPath);
            if (extension.Equals(".png", StringComparison.OrdinalIgnoreCase) ||
                extension.Equals(".jpg", StringComparison.OrdinalIgnoreCase) ||
                extension.Equals(".jpeg", StringComparison.OrdinalIgnoreCase))
            {
                await using var imageStream = File.OpenRead(selectedPath);
                await using var memory = new MemoryStream();
                await imageStream.CopyToAsync(memory);
                var bytes = memory.ToArray();
                BackgroundImageView.Source = ImageSource.FromStream(() => new MemoryStream(bytes));

                _drawable.HasBackgroundImage = true;
                _drawable.FillRegions.Clear();
                _drawable.Strokes.Clear();
                _drawable.Shapes.Clear();
                _drawable.PreviewShape = null;
                CanvasView.Invalidate();
                return;
            }

            await using var stream = File.OpenRead(selectedPath);
            using var reader = new StreamReader(stream);
            var json = await reader.ReadToEndAsync();
            var document = JsonSerializer.Deserialize<DrawingDocument>(json);

            if (document is null)
            {
                await DisplayAlert("Open Failed", "The selected file is empty or invalid.", "OK");
                return;
            }

            document.ApplyTo(_drawable);
            BackgroundImageView.Source = null;
            _drawable.HasBackgroundImage = false;
            _selectedBackgroundColor = _drawable.BackgroundColor;
            CanvasView.Invalidate();
        }
        catch (Exception ex)
        {
            await DisplayAlert("Open Failed", ex.Message, "OK");
        }
    }
}

public enum WhiteboardTool
{
    Pen,
    Eraser,
    Line,
    Rectangle,
    Ellipse,
    Fill
}

public sealed class WhiteboardDrawable : IDrawable
{
    public Color BackgroundColor { get; set; } = Colors.White;
    public bool HasBackgroundImage { get; set; }
    public RectF LastDirtyRect { get; private set; } = new(0, 0, 1, 1);
    public List<FillRegion> FillRegions { get; } = [];
    public List<StrokeElement> Strokes { get; } = [];
    public List<ShapeElement> Shapes { get; } = [];
    public ShapeElement? PreviewShape { get; set; }
    public PointF? EraserPreviewCenter { get; set; }
    public float EraserPreviewSize { get; set; } = 20;

    public void Draw(ICanvas canvas, RectF dirtyRect)
    {
        LastDirtyRect = dirtyRect;

        if (!HasBackgroundImage)
        {
            canvas.FillColor = BackgroundColor;
            canvas.FillRectangle(dirtyRect);
        }

        foreach (var fillRegion in FillRegions)
        {
            DrawFillRegion(canvas, fillRegion);
        }

        foreach (var shape in Shapes)
        {
            DrawShape(canvas, shape);
        }

        if (PreviewShape is not null)
        {
            DrawShape(canvas, PreviewShape);
        }

        foreach (var stroke in Strokes)
        {
            DrawStroke(canvas, stroke);
        }

        DrawEraserPreview(canvas);
    }

    private void DrawStroke(ICanvas canvas, StrokeElement stroke)
    {
        if (stroke.IsEraser)
        {
            DrawEraserStroke(canvas, stroke);
            return;
        }

        canvas.StrokeColor = stroke.IsEraser ? BackgroundColor : stroke.Color;
        canvas.StrokeSize = stroke.Thickness;

        if (stroke.Points.Count == 1)
        {
            var p = stroke.Points[0];
            canvas.FillColor = stroke.IsEraser ? BackgroundColor : stroke.Color;
            canvas.FillCircle(p.X, p.Y, stroke.Thickness / 2f);
            return;
        }

        for (var i = 1; i < stroke.Points.Count; i++)
        {
            var p1 = stroke.Points[i - 1];
            var p2 = stroke.Points[i];
            canvas.DrawLine(p1.X, p1.Y, p2.X, p2.Y);
        }
    }

    private void DrawEraserStroke(ICanvas canvas, StrokeElement stroke)
    {
        canvas.FillColor = BackgroundColor;

        if (stroke.Points.Count == 0)
        {
            return;
        }

        DrawEraserSquare(canvas, stroke.Points[0], stroke.Thickness);

        for (var i = 1; i < stroke.Points.Count; i++)
        {
            var p1 = stroke.Points[i - 1];
            var p2 = stroke.Points[i];
            var distance = MathF.Sqrt(MathF.Pow(p2.X - p1.X, 2) + MathF.Pow(p2.Y - p1.Y, 2));
            var step = Math.Max(1f, stroke.Thickness / 3f);
            var steps = Math.Max(1, (int)(distance / step));

            for (var s = 0; s <= steps; s++)
            {
                var t = (float)s / steps;
                var x = p1.X + ((p2.X - p1.X) * t);
                var y = p1.Y + ((p2.Y - p1.Y) * t);
                DrawEraserSquare(canvas, new PointF(x, y), stroke.Thickness);
            }
        }
    }

    private static void DrawEraserSquare(ICanvas canvas, PointF point, float size)
    {
        var half = size / 2f;
        canvas.FillRectangle(point.X - half, point.Y - half, size, size);
    }

    private void DrawEraserPreview(ICanvas canvas)
    {
        if (EraserPreviewCenter is null)
        {
            return;
        }

        var center = EraserPreviewCenter.Value;
        var half = EraserPreviewSize / 2f;
        canvas.FillColor = new Color(1f, 1f, 1f, 0.15f);
        canvas.FillRectangle(center.X - half, center.Y - half, EraserPreviewSize, EraserPreviewSize);

        canvas.StrokeColor = Colors.Black;
        canvas.StrokeSize = 1;
        canvas.DrawRectangle(center.X - half, center.Y - half, EraserPreviewSize, EraserPreviewSize);
    }

    private static void DrawShape(ICanvas canvas, ShapeElement shape)
    {
        if (shape.ShapeType == WhiteboardTool.Line)
        {
            canvas.StrokeColor = shape.Color;
            canvas.StrokeSize = shape.Thickness;
            canvas.DrawLine(shape.Start.X, shape.Start.Y, shape.End.X, shape.End.Y);
            return;
        }

        var rect = GetRect(shape.Start, shape.End);

        if (shape.FillColor is not null)
        {
            canvas.FillColor = shape.FillColor;
            if (shape.ShapeType == WhiteboardTool.Rectangle)
            {
                canvas.FillRectangle(rect);
            }
            else if (shape.ShapeType == WhiteboardTool.Ellipse)
            {
                canvas.FillEllipse(rect);
            }
        }

        canvas.StrokeColor = shape.Color;
        canvas.StrokeSize = shape.Thickness;

        if (shape.ShapeType == WhiteboardTool.Rectangle)
        {
            canvas.DrawRectangle(rect);
        }
        else if (shape.ShapeType == WhiteboardTool.Ellipse)
        {
            canvas.DrawEllipse(rect);
        }
    }

    private static RectF GetRect(PointF start, PointF end)
    {
        var x = Math.Min(start.X, end.X);
        var y = Math.Min(start.Y, end.Y);
        var w = Math.Abs(end.X - start.X);
        var h = Math.Abs(end.Y - start.Y);
        return new RectF(x, y, w, h);
    }

    public bool TryFillAtPoint(PointF point, Color color)
    {
        if (TryFloodFillRegion(point, color))
        {
            return true;
        }

        if (TryFillFromConnectedBoundary(point, color))
        {
            return true;
        }

        for (var i = Shapes.Count - 1; i >= 0; i--)
        {
            var shape = Shapes[i];
            if (shape.ShapeType == WhiteboardTool.Line)
            {
                continue;
            }

            if (shape.Contains(point))
            {
                shape.FillColor = color;
                return true;
            }
        }

        return false;
    }

    private bool TryFloodFillRegion(PointF startPoint, Color color)
    {
        const float cellSize = 1f;

        var width = Math.Max(1, (int)MathF.Ceiling(LastDirtyRect.Width / cellSize));
        var height = Math.Max(1, (int)MathF.Ceiling(LastDirtyRect.Height / cellSize));
        var maxCells = Math.Min(width * height, 2_000_000);
        var startX = (int)((startPoint.X - LastDirtyRect.Left) / cellSize);
        var startY = (int)((startPoint.Y - LastDirtyRect.Top) / cellSize);

        if (startX < 0 || startY < 0 || startX >= width || startY >= height)
        {
            return false;
        }

        if (IsBoundaryPoint(startPoint, 0f))
        {
            return false;
        }

        var visited = new bool[width, height];
        var queue = new Queue<(int X, int Y)>();
        var cells = new List<PointF>();
        var touchesCanvasEdge = false;

        queue.Enqueue((startX, startY));

        while (queue.Count > 0)
        {
            var (x, y) = queue.Dequeue();
            if (x < 0 || y < 0 || x >= width || y >= height)
            {
                return false;
            }

            if (visited[x, y])
            {
                continue;
            }

            visited[x, y] = true;
            var worldPoint = new PointF(
                LastDirtyRect.Left + ((x + 0.5f) * cellSize),
                LastDirtyRect.Top + ((y + 0.5f) * cellSize));

            if (IsBoundaryPoint(worldPoint, 0f))
            {
                continue;
            }

            cells.Add(worldPoint);
            if (cells.Count > maxCells)
            {
                return false;
            }

            if (x == 0 || y == 0 || x == width - 1 || y == height - 1)
            {
                return false;
            }

            queue.Enqueue((x + 1, y));
            queue.Enqueue((x - 1, y));
            queue.Enqueue((x, y + 1));
            queue.Enqueue((x, y - 1));
        }

        if (touchesCanvasEdge || cells.Count == 0)
        {
            return false;
        }

        FillRegions.Add(new FillRegion
        {
            Color = color,
            Points = cells,
            IsRaster = true,
            CellSize = cellSize
        });

        return true;
    }

    private bool TryFillFromConnectedBoundary(PointF point, Color color)
    {
        var pieces = BuildBoundaryPieces();
        if (pieces.Count < 2)
        {
            return false;
        }

        var tolerance = Math.Max(10f, pieces.Max(p => p.Thickness) * 1.5f);
        for (var i = 0; i < pieces.Count; i++)
        {
            if (TryBuildClosedPolygon(pieces, i, false, tolerance, out var polygon) && IsPointInPolygon(point, polygon))
            {
                FillRegions.Add(new FillRegion { Color = color, Points = polygon });
                return true;
            }

            if (TryBuildClosedPolygon(pieces, i, true, tolerance, out polygon) && IsPointInPolygon(point, polygon))
            {
                FillRegions.Add(new FillRegion { Color = color, Points = polygon });
                return true;
            }
        }

        return false;
    }

    private List<BoundaryPiece> BuildBoundaryPieces()
    {
        var pieces = new List<BoundaryPiece>();

        foreach (var shape in Shapes)
        {
            if (shape.ShapeType == WhiteboardTool.Line)
            {
                pieces.Add(new BoundaryPiece([shape.Start, shape.End], shape.Thickness));
            }
        }

        return pieces;
    }

    private static bool TryBuildClosedPolygon(List<BoundaryPiece> pieces, int startIndex, bool reverseStart, float tolerance, out List<PointF> polygon)
    {
        var startPiece = pieces[startIndex];
        List<PointF> startPoints;
        if (reverseStart)
        {
            startPoints = [.. startPiece.Points.AsEnumerable().Reverse()];
        }
        else
        {
            startPoints = [.. startPiece.Points];
        }
        var startPoint = startPoints[0];
        var currentPoint = startPoints[^1];

        var used = new HashSet<int> { startIndex };
        polygon = [.. startPoints];
        var toleranceSquared = tolerance * tolerance;

        while (used.Count <= pieces.Count)
        {
            if (DistanceSquared(currentPoint, startPoint) <= toleranceSquared && polygon.Count >= 3)
            {
                polygon[^1] = startPoint;
                polygon.RemoveAt(polygon.Count - 1);
                return true;
            }

            var foundNext = false;
            for (var i = 0; i < pieces.Count; i++)
            {
                if (used.Contains(i))
                {
                    continue;
                }

                var candidate = pieces[i];
                var candidateStart = candidate.Points[0];
                var candidateEnd = candidate.Points[^1];

                if (DistanceSquared(currentPoint, candidateStart) <= toleranceSquared)
                {
                    used.Add(i);
                    AppendPoints(polygon, candidate.Points);
                    currentPoint = polygon[^1];
                    foundNext = true;
                    break;
                }

                if (DistanceSquared(currentPoint, candidateEnd) <= toleranceSquared)
                {
                    used.Add(i);
                    AppendPoints(polygon, [.. candidate.Points.AsEnumerable().Reverse()]);
                    currentPoint = polygon[^1];
                    foundNext = true;
                    break;
                }
            }

            if (!foundNext)
            {
                break;
            }
        }

        polygon = [];
        return false;
    }

    private static void AppendPoints(List<PointF> polygon, IReadOnlyList<PointF> nextPoints)
    {
        for (var i = 1; i < nextPoints.Count; i++)
        {
            polygon.Add(nextPoints[i]);
        }
    }

    private static float DistanceSquared(PointF a, PointF b)
    {
        var dx = a.X - b.X;
        var dy = a.Y - b.Y;
        return (dx * dx) + (dy * dy);
    }

    private bool IsBoundaryPoint(PointF point, float padding)
    {
        foreach (var stroke in Strokes)
        {
            if (stroke.IsEraser)
            {
                continue;
            }

            if (IsNearStroke(stroke, point, padding))
            {
                return true;
            }
        }

        foreach (var shape in Shapes)
        {
            if (shape.ShapeType == WhiteboardTool.Line && IsNearLine(shape.Start, shape.End, shape.Thickness, point, padding))
            {
                return true;
            }

            if (shape.ShapeType == WhiteboardTool.Rectangle && IsNearRectangleBoundary(shape, point, padding))
            {
                return true;
            }

            if (shape.ShapeType == WhiteboardTool.Ellipse && IsNearEllipseBoundary(shape, point, padding))
            {
                return true;
            }
        }

        return false;
    }

    private static bool IsNearRectangleBoundary(ShapeElement shape, PointF point, float padding)
    {
        var rect = GetRect(shape.Start, shape.End);
        if (rect.Width <= 0 || rect.Height <= 0)
        {
            return false;
        }

        var halfStroke = shape.Thickness * 0.5f;
        var outer = new RectF(
            rect.X - halfStroke - padding,
            rect.Y - halfStroke - padding,
            rect.Width + (2 * (halfStroke + padding)),
            rect.Height + (2 * (halfStroke + padding)));

        if (!outer.Contains(point))
        {
            return false;
        }

        var innerWidth = Math.Max(0f, rect.Width - (2 * (halfStroke + padding)));
        var innerHeight = Math.Max(0f, rect.Height - (2 * (halfStroke + padding)));
        var inner = new RectF(
            rect.X + halfStroke + padding,
            rect.Y + halfStroke + padding,
            innerWidth,
            innerHeight);

        return !inner.Contains(point);
    }

    private static bool IsNearEllipseBoundary(ShapeElement shape, PointF point, float padding)
    {
        var rect = GetRect(shape.Start, shape.End);
        if (rect.Width <= 0 || rect.Height <= 0)
        {
            return false;
        }

        var centerX = rect.X + (rect.Width / 2f);
        var centerY = rect.Y + (rect.Height / 2f);
        var rx = rect.Width / 2f;
        var ry = rect.Height / 2f;

        var px = point.X - centerX;
        var py = point.Y - centerY;

        if (rx <= 0 || ry <= 0)
        {
            return false;
        }

        var halfStroke = shape.Thickness * 0.5f;
        var outerRx = rx + halfStroke + padding;
        var outerRy = ry + halfStroke + padding;
        var innerRx = Math.Max(0.001f, rx - halfStroke - padding);
        var innerRy = Math.Max(0.001f, ry - halfStroke - padding);

        var outerEquation = ((px * px) / (outerRx * outerRx)) + ((py * py) / (outerRy * outerRy));
        var innerEquation = ((px * px) / (innerRx * innerRx)) + ((py * py) / (innerRy * innerRy));

        return outerEquation <= 1f && innerEquation >= 1f;
    }

    private static bool IsNearStroke(StrokeElement stroke, PointF point, float padding)
    {
        if (stroke.Points.Count == 0)
        {
            return false;
        }

        if (stroke.Points.Count == 1)
        {
            return DistanceSquared(stroke.Points[0], point) <= MathF.Pow((stroke.Thickness * 0.5f) + padding, 2);
        }

        for (var i = 1; i < stroke.Points.Count; i++)
        {
            if (IsNearLine(stroke.Points[i - 1], stroke.Points[i], stroke.Thickness, point, padding))
            {
                return true;
            }
        }

        return false;
    }

    private static bool IsNearLine(PointF a, PointF b, float thickness, PointF point, float padding)
    {
        var radius = (thickness * 0.5f) + padding;
        return DistanceToSegmentSquared(point, a, b) <= radius * radius;
    }

    private static float DistanceToSegmentSquared(PointF p, PointF a, PointF b)
    {
        var abX = b.X - a.X;
        var abY = b.Y - a.Y;
        var lengthSq = (abX * abX) + (abY * abY);

        if (lengthSq <= float.Epsilon)
        {
            return DistanceSquared(p, a);
        }

        var t = ((p.X - a.X) * abX + (p.Y - a.Y) * abY) / lengthSq;
        t = Math.Clamp(t, 0f, 1f);
        var nearest = new PointF(a.X + (abX * t), a.Y + (abY * t));
        return DistanceSquared(p, nearest);
    }

    private static void DrawFillRegion(ICanvas canvas, FillRegion region)
    {
        if (region.IsRaster)
        {
            const float overlap = 0.5f;
            var half = region.CellSize / 2f;
            canvas.FillColor = region.Color;
            foreach (var point in region.Points)
            {
                canvas.FillRectangle(
                    point.X - half - overlap,
                    point.Y - half - overlap,
                    region.CellSize + (2 * overlap),
                    region.CellSize + (2 * overlap));
            }

            return;
        }

        if (region.Points.Count < 3)
        {
            return;
        }

        var path = new PathF();
        path.MoveTo(region.Points[0].X, region.Points[0].Y);
        for (var i = 1; i < region.Points.Count; i++)
        {
            path.LineTo(region.Points[i].X, region.Points[i].Y);
        }

        path.Close();
        canvas.FillColor = region.Color;
        canvas.FillPath(path);
    }


    private static bool IsPointInPolygon(PointF point, List<PointF> polygon)
    {
        var inside = false;
        var j = polygon.Count - 1;

        for (var i = 0; i < polygon.Count; i++)
        {
            var xi = polygon[i].X;
            var yi = polygon[i].Y;
            var xj = polygon[j].X;
            var yj = polygon[j].Y;

            var intersects = ((yi > point.Y) != (yj > point.Y)) &&
                             (point.X < (xj - xi) * (point.Y - yi) / ((yj - yi) + float.Epsilon) + xi);
            if (intersects)
            {
                inside = !inside;
            }

            j = i;
        }

        return inside;
    }
}

public sealed class BoundaryPiece
{
    public BoundaryPiece(List<PointF> points, float thickness)
    {
        Points = points;
        Thickness = thickness;
    }

    public List<PointF> Points { get; }
    public float Thickness { get; }
}

public sealed class FillRegion
{
    public List<PointF> Points { get; set; } = [];
    public Color Color { get; set; } = Colors.Transparent;
    public bool IsRaster { get; set; }
    public float CellSize { get; set; } = 3f;
}

public sealed class StrokeElement
{
    public List<PointF> Points { get; } = [];
    public Color Color { get; set; } = Colors.Black;
    public float Thickness { get; set; } = 4;
    public bool IsEraser { get; set; }
}

public sealed class ShapeElement
{
    public PointF Start { get; set; }
    public PointF End { get; set; }
    public WhiteboardTool ShapeType { get; set; } = WhiteboardTool.Rectangle;
    public Color Color { get; set; } = Colors.Black;
    public float Thickness { get; set; } = 4;
    public Color? FillColor { get; set; }

    public bool Contains(PointF point)
    {
        var rect = new RectF(
            Math.Min(Start.X, End.X),
            Math.Min(Start.Y, End.Y),
            Math.Abs(End.X - Start.X),
            Math.Abs(End.Y - Start.Y));

        if (ShapeType == WhiteboardTool.Rectangle)
        {
            return rect.Contains(point);
        }

        if (ShapeType == WhiteboardTool.Ellipse)
        {
            if (rect.Width <= 0 || rect.Height <= 0)
            {
                return false;
            }

            var centerX = rect.X + (rect.Width / 2f);
            var centerY = rect.Y + (rect.Height / 2f);
            var normalizedX = (point.X - centerX) / (rect.Width / 2f);
            var normalizedY = (point.Y - centerY) / (rect.Height / 2f);
            return (normalizedX * normalizedX) + (normalizedY * normalizedY) <= 1f;
        }

        return false;
    }
}

public sealed class DrawingDocument
{
    public string BackgroundColor { get; set; } = "#FFFFFFFF";
    public List<FillRegionDocument> FillRegions { get; set; } = [];
    public List<StrokeDocument> Strokes { get; set; } = [];
    public List<ShapeDocument> Shapes { get; set; } = [];

    public static DrawingDocument FromDrawable(WhiteboardDrawable drawable)
    {
        return new DrawingDocument
        {
            BackgroundColor = ToHex(drawable.BackgroundColor),
            FillRegions = drawable.FillRegions.Select(f => new FillRegionDocument
            {
                Color = ToHex(f.Color),
                IsRaster = f.IsRaster,
                CellSize = f.CellSize,
                Points = f.Points.Select(p => new PointDocument { X = p.X, Y = p.Y }).ToList()
            }).ToList(),
            Strokes = drawable.Strokes.Select(s => new StrokeDocument
            {
                Color = ToHex(s.Color),
                Thickness = s.Thickness,
                IsEraser = s.IsEraser,
                Points = s.Points.Select(p => new PointDocument { X = p.X, Y = p.Y }).ToList()
            }).ToList(),
            Shapes = drawable.Shapes.Select(s => new ShapeDocument
            {
                StartX = s.Start.X,
                StartY = s.Start.Y,
                EndX = s.End.X,
                EndY = s.End.Y,
                ShapeType = s.ShapeType,
                Color = ToHex(s.Color),
                Thickness = s.Thickness,
                FillColor = s.FillColor is null ? null : ToHex(s.FillColor)
            }).ToList()
        };
    }

    public void ApplyTo(WhiteboardDrawable drawable)
    {
        drawable.BackgroundColor = Color.FromArgb(BackgroundColor);
        drawable.FillRegions.Clear();
        drawable.Strokes.Clear();
        drawable.Shapes.Clear();
        drawable.PreviewShape = null;

        foreach (var fillRegion in FillRegions)
        {
            drawable.FillRegions.Add(new FillRegion
            {
                Color = Color.FromArgb(fillRegion.Color),
                IsRaster = fillRegion.IsRaster,
                CellSize = fillRegion.CellSize <= 0f ? 3f : fillRegion.CellSize,
                Points = fillRegion.Points.Select(p => new PointF(p.X, p.Y)).ToList()
            });
        }

        foreach (var stroke in Strokes)
        {
            var newStroke = new StrokeElement
            {
                Color = Color.FromArgb(stroke.Color),
                Thickness = stroke.Thickness,
                IsEraser = stroke.IsEraser
            };

            foreach (var point in stroke.Points)
            {
                newStroke.Points.Add(new PointF(point.X, point.Y));
            }

            drawable.Strokes.Add(newStroke);
        }

        foreach (var shape in Shapes)
        {
            drawable.Shapes.Add(new ShapeElement
            {
                Start = new PointF(shape.StartX, shape.StartY),
                End = new PointF(shape.EndX, shape.EndY),
                ShapeType = shape.ShapeType,
                Color = Color.FromArgb(shape.Color),
                Thickness = shape.Thickness,
                FillColor = string.IsNullOrWhiteSpace(shape.FillColor) ? null : Color.FromArgb(shape.FillColor)
            });
        }
    }

    private static string ToHex(Color color)
    {
        var a = (byte)(color.Alpha * 255);
        var r = (byte)(color.Red * 255);
        var g = (byte)(color.Green * 255);
        var b = (byte)(color.Blue * 255);
        return $"#{a:X2}{r:X2}{g:X2}{b:X2}";
    }
}

public sealed class StrokeDocument
{
    public string Color { get; set; } = "#FF000000";
    public float Thickness { get; set; }
    public bool IsEraser { get; set; }
    public List<PointDocument> Points { get; set; } = [];
}

public sealed class FillRegionDocument
{
    public string Color { get; set; } = "#FF000000";
    public bool IsRaster { get; set; }
    public float CellSize { get; set; } = 3f;
    public List<PointDocument> Points { get; set; } = [];
}

public sealed class ShapeDocument
{
    public float StartX { get; set; }
    public float StartY { get; set; }
    public float EndX { get; set; }
    public float EndY { get; set; }
    public WhiteboardTool ShapeType { get; set; }
    public string Color { get; set; } = "#FF000000";
    public float Thickness { get; set; }
    public string? FillColor { get; set; }
}

public sealed class PointDocument
{
    public float X { get; set; }
    public float Y { get; set; }
}
