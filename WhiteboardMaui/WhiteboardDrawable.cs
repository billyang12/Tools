using Microsoft.Maui.Graphics;
using System.Collections.Generic;
using System.Linq;
using System;

namespace WhiteboardMaui;

public enum Tool
{
    FreeDraw,
    Line,
    Rectangle,
    Ellipse,
    Text,
    Erase
}

public class Stroke
{
    public List<PointF> Points { get; set; } = new List<PointF>();
    public Color Color { get; set; } = Colors.Black;
    public float Width { get; set; } = 4;
    public Tool Tool { get; set; } = Tool.FreeDraw;
    public string? Text { get; set; }
    public string? FontFamily { get; set; }
    public float FontSize { get; set; } = 24;
}

public class WhiteboardDrawable : IDrawable
{
    public List<Stroke> Strokes { get; } = new List<Stroke>();
    public Color CurrentColor { get; set; } = Colors.Black;
    public Color Background { get; set; } = Colors.White;
    public float LineWidth { get; set; } = 4;
    public Tool CurrentTool { get; set; } = Tool.FreeDraw;

    // image bytes if loaded
    byte[]? imageData;
    Microsoft.Maui.Graphics.IImage? loadedImage;
    Microsoft.Maui.Graphics.RectF imageRect;

    Stroke? current;

    public void Draw(ICanvas canvas, RectF dirtyRect)
    {
        // background
        canvas.SaveState();
        canvas.FillColor = Background;
        canvas.FillRectangle(dirtyRect);
        canvas.RestoreState();

        if (imageData != null)
        {
            // For now, skip image rendering in the stubs
            // loadedImage = ... would need proper MAUI image loading
        }

        foreach (var s in Strokes)
        {
            canvas.SaveState();
            canvas.StrokeColor = s.Color;
            canvas.StrokeSize = s.Width;
            if (s.Tool == Tool.FreeDraw || s.Tool == Tool.Erase)
            {
                for (int i = 1; i < s.Points.Count; i++)
                {
                    var a = s.Points[i - 1];
                    var b = s.Points[i];
                    canvas.DrawLine(a.X, a.Y, b.X, b.Y);
                }
            }
            else if (s.Tool == Tool.Line)
            {
                if (s.Points.Count >= 2)
                {
                    var a = s.Points.First();
                    var b = s.Points.Last();
                    canvas.DrawLine(a.X, a.Y, b.X, b.Y);
                }
            }
            else if (s.Tool == Tool.Rectangle)
            {
                if (s.Points.Count >= 2)
                {
                    var a = s.Points.First();
                    var b = s.Points.Last();
                    var rect = new RectF(Math.Min(a.X, b.X), Math.Min(a.Y, b.Y), Math.Abs(b.X - a.X), Math.Abs(b.Y - a.Y));
                    canvas.DrawRectangle(rect);
                }
            }
            else if (s.Tool == Tool.Ellipse)
            {
                if (s.Points.Count >= 2)
                {
                    var a = s.Points.First();
                    var b = s.Points.Last();
                    var rect = new RectF(Math.Min(a.X, b.X), Math.Min(a.Y, b.Y), Math.Abs(b.X - a.X), Math.Abs(b.Y - a.Y));
                    canvas.DrawEllipse(rect);
                }
            }
            else if (s.Tool == Tool.Text)
            {
                if (!string.IsNullOrEmpty(s.Text) && s.Points.Count >= 1)
                {
                    var p = s.Points.First();
                    canvas.FontSize = s.FontSize;
                    // Note: FontName property varies by platform in MAUI
                    // canvas.FontName = s.FontFamily;
                    canvas.DrawString(s.Text, p.X, p.Y, HorizontalAlignment.Left);
                }
            }
            canvas.RestoreState();
        }
    }

    public void Start(PointF p)
    {
        current = new Stroke { Color = CurrentTool == Tool.Erase ? Background : CurrentColor, Width = LineWidth, Tool = CurrentTool };
        current.Points.Add(p);
        Strokes.Add(current);
    }

    public void Move(PointF p)
    {
        if (current == null) return;
        current.Points.Add(p);
    }

    public void End(PointF p)
    {
        if (current == null) return;
        current.Points.Add(p);
        current = null;
    }

    public void Clear(Color bg)
    {
        Strokes.Clear();
        imageData = null;
        loadedImage = null;
        Background = bg;
    }

    public void LoadImage(byte[] data)
    {
        imageData = data;
        loadedImage = null;
        // imageRect will be set on demand; for now, use a default size
    }

    public void LoadFromImage(byte[] data)
    {
        Clear(Background);
        LoadImage(data);
    }

    public Microsoft.Maui.Graphics.IImage? Rasterize(int width, int height)
    {
        try
        {
            // MAUI's image rasterization would be done via a platform-specific bitmap API
            // For now, return null as we can't properly render without platform implementation
            return null;
        }
        catch
        {
            return null;
        }
    }
}
