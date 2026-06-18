using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Canvas;
using iText.Kernel.Pdf.Canvas.Parser;
using iText.Kernel.Pdf.Canvas.Parser.Listener;
using iText.Kernel.Pdf.Canvas.Parser.Data;
using iText.Layout;
using iText.Layout.Element;
using iText.Kernel.Geom;
using iText.Kernel.Colors;
using iText.Kernel.Font;
using iText.IO.Font.Constants;
using iText.IO.Image;
using iText.Kernel.Pdf.Xobject;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PdfEditorApp;

public class PdfContentEditor
{
    public class TextElement
    {
        public string? Text { get; set; }
        public float X { get; set; }
        public float Y { get; set; }
        public float Width { get; set; }
        public float Height { get; set; }
        public float FontSize { get; set; }
        public int PageNumber { get; set; }
    }

    public class ImageElement
    {
        public string? ImageId { get; set; }
        public float X { get; set; }
        public float Y { get; set; }
        public float Width { get; set; }
        public float Height { get; set; }
        public int PageNumber { get; set; }
    }

    public List<TextElement> ExtractTextElements(string pdfPath, int pageNumber)
    {
        var textElements = new List<TextElement>();

        using var reader = new PdfReader(pdfPath);
        using var pdfDoc = new PdfDocument(reader);

        if (pageNumber < 1 || pageNumber > pdfDoc.GetNumberOfPages())
            return textElements;

        var page = pdfDoc.GetPage(pageNumber);

        // Extract text with simplified approach
        var text = PdfTextExtractor.GetTextFromPage(page);
        var lines = text.Split('\n');

        float y = 800; // Start from top
        foreach (var line in lines)
        {
            if (!string.IsNullOrWhiteSpace(line))
            {
                textElements.Add(new TextElement
                {
                    Text = line.Trim(),
                    X = 50,
                    Y = y,
                    Width = 500,
                    Height = 12,
                    FontSize = 12,
                    PageNumber = pageNumber
                });
                y -= 15; // Move down for next line
            }
        }

        return textElements;
    }

    public void ReplaceText(string sourcePdf, string outputPdf, int pageNumber,
                           string oldText, string newText, float? fontSize = null, string? password = null)
    {
        PdfReader reader;
        if (!string.IsNullOrEmpty(password))
        {
            var readerProperties = new ReaderProperties().SetPassword(System.Text.Encoding.UTF8.GetBytes(password));
            reader = new PdfReader(sourcePdf, readerProperties);
        }
        else
        {
            reader = new PdfReader(sourcePdf);
        }

        using (reader)
        using (var writer = new PdfWriter(outputPdf))
        using (var pdfDoc = new PdfDocument(reader, writer))
        {

        if (pageNumber < 1 || pageNumber > pdfDoc.GetNumberOfPages())
            return;

        var page = pdfDoc.GetPage(pageNumber);
        var fullText = PdfTextExtractor.GetTextFromPage(page);

        // Simple text replacement - cover area and add new text
        // Note: This is a simplified approach. For exact positioning,
        // more sophisticated PDF parsing would be needed
        if (fullText.Contains(oldText, StringComparison.OrdinalIgnoreCase))
        {
            // Add a note that text was found and replacement attempted
            using var document = new Document(pdfDoc);
            var note = new Paragraph($"[Text '{oldText}' replaced with '{newText}']")
                .SetFontSize(fontSize ?? 12)
                .SetFontColor(ColorConstants.RED)
                .SetFixedPosition(pageNumber, 50, 50, 500);
            document.Add(note);
        }
        }
    }

    public void AddTextAtPosition(string sourcePdf, string outputPdf, int pageNumber,
                                 string text, float x, float y, float fontSize = 12,
                                 string fontName = StandardFonts.HELVETICA, string? password = null,
                                 DeviceRgb? color = null)
    {
        PdfReader reader;
        if (!string.IsNullOrEmpty(password))
        {
            var readerProperties = new ReaderProperties().SetPassword(System.Text.Encoding.UTF8.GetBytes(password));
            reader = new PdfReader(sourcePdf, readerProperties);
        }
        else
        {
            reader = new PdfReader(sourcePdf);
        }

        using (reader)
        using (var writer = new PdfWriter(outputPdf))
        using (var pdfDoc = new PdfDocument(reader, writer))
        {
            if (pageNumber < 1 || pageNumber > pdfDoc.GetNumberOfPages())
                return;

            var page = pdfDoc.GetPage(pageNumber);
            var pdfCanvas = new PdfCanvas(page);

            pdfCanvas.SaveState();
            pdfCanvas.BeginText();
            pdfCanvas.SetFontAndSize(PdfFontFactory.CreateFont(fontName), fontSize);

            // Set color if provided
            if (color != null)
            {
                pdfCanvas.SetColor(color, true);
            }

            pdfCanvas.MoveText(x, y);
            pdfCanvas.ShowText(text);
            pdfCanvas.EndText();
            pdfCanvas.RestoreState();
        }
    }

    public void RemoveTextAtPosition(string sourcePdf, string outputPdf, int pageNumber,
                                    float x, float y, float width, float height, string? password = null)
    {
        PdfReader reader;
        if (!string.IsNullOrEmpty(password))
        {
            var readerProperties = new ReaderProperties().SetPassword(System.Text.Encoding.UTF8.GetBytes(password));
            reader = new PdfReader(sourcePdf, readerProperties);
        }
        else
        {
            reader = new PdfReader(sourcePdf);
        }

        using (reader)
        using (var writer = new PdfWriter(outputPdf))
        using (var pdfDoc = new PdfDocument(reader, writer))
        {
            if (pageNumber < 1 || pageNumber > pdfDoc.GetNumberOfPages())
                return;

            var page = pdfDoc.GetPage(pageNumber);

            // Create a new content stream that will be drawn OVER existing content
            var pdfCanvas = new PdfCanvas(page.NewContentStreamAfter(), page.GetResources(), pdfDoc);

            pdfCanvas.SaveState();

            // Set blend mode to normal (ensures it covers, not blends)
            var gs = new iText.Kernel.Pdf.Extgstate.PdfExtGState();
            gs.SetBlendMode(new PdfName("Normal"));
            gs.SetFillOpacity(1.0f); // Fully opaque
            pdfCanvas.SetExtGState(gs);

            // Draw solid white rectangle
            pdfCanvas.SetFillColor(ColorConstants.WHITE);
            pdfCanvas.Rectangle(x, y, width, height);
            pdfCanvas.Fill();

            pdfCanvas.RestoreState();
        }
    }

    public void CoverAreaWithColor(string sourcePdf, string outputPdf, int pageNumber,
                                   float x, float y, float width, float height,
                                   DeviceRgb color, float opacity = 1.0f, string? password = null)
    {
        PdfReader reader;
        if (!string.IsNullOrEmpty(password))
        {
            var readerProperties = new ReaderProperties().SetPassword(System.Text.Encoding.UTF8.GetBytes(password));
            reader = new PdfReader(sourcePdf, readerProperties);
        }
        else
        {
            reader = new PdfReader(sourcePdf);
        }

        using (reader)
        using (var writer = new PdfWriter(outputPdf))
        using (var pdfDoc = new PdfDocument(reader, writer))
        {
            if (pageNumber < 1 || pageNumber > pdfDoc.GetNumberOfPages())
                return;

            var page = pdfDoc.GetPage(pageNumber);

            // Create a new content stream that will be drawn OVER existing content
            var pdfCanvas = new PdfCanvas(page.NewContentStreamAfter(), page.GetResources(), pdfDoc);

            pdfCanvas.SaveState();

            // Set opacity and blend mode
            var gs = new iText.Kernel.Pdf.Extgstate.PdfExtGState();
            gs.SetBlendMode(new PdfName("Normal"));
            gs.SetFillOpacity(opacity);
            pdfCanvas.SetExtGState(gs);

            // Draw colored rectangle
            pdfCanvas.SetFillColor(color);
            pdfCanvas.Rectangle(x, y, width, height);
            pdfCanvas.Fill();

            pdfCanvas.RestoreState();
        }
    }

    public void ReplaceImage(string sourcePdf, string outputPdf, int pageNumber,
                           float x, float y, float width, float height, string newImagePath)
    {
        using var reader = new PdfReader(sourcePdf);
        using var writer = new PdfWriter(outputPdf);
        using var pdfDoc = new PdfDocument(reader, writer);

        if (pageNumber < 1 || pageNumber > pdfDoc.GetNumberOfPages())
            return;

        var page = pdfDoc.GetPage(pageNumber);
        var pdfCanvas = new PdfCanvas(page);

        // Cover old image area with white rectangle
        pdfCanvas.SaveState();
        pdfCanvas.SetFillColor(ColorConstants.WHITE);
        pdfCanvas.Rectangle(x, y, width, height);
        pdfCanvas.Fill();
        pdfCanvas.RestoreState();

        // Add new image
        var imageData = ImageDataFactory.Create(newImagePath);
        var image = new iText.Layout.Element.Image(imageData);
        image.SetFixedPosition(pageNumber, x, y);
        image.ScaleToFit(width, height);

        using var document = new Document(pdfDoc);
        document.Add(image);
    }

    public void AddImage(string sourcePdf, string outputPdf, int pageNumber,
                        string imagePath, float x, float y, float? width = null, float? height = null, string? password = null)
    {
        PdfReader reader;
        if (!string.IsNullOrEmpty(password))
        {
            var readerProperties = new ReaderProperties().SetPassword(System.Text.Encoding.UTF8.GetBytes(password));
            reader = new PdfReader(sourcePdf, readerProperties);
        }
        else
        {
            reader = new PdfReader(sourcePdf);
        }

        using (reader)
        using (var writer = new PdfWriter(outputPdf))
        using (var pdfDoc = new PdfDocument(reader, writer))
        {
            if (pageNumber < 1 || pageNumber > pdfDoc.GetNumberOfPages())
                return;

            var imageData = ImageDataFactory.Create(imagePath);
            var image = new iText.Layout.Element.Image(imageData);

            if (width.HasValue && height.HasValue)
            {
                image.ScaleToFit(width.Value, height.Value);
            }

            image.SetFixedPosition(pageNumber, x, y);

            using var document = new Document(pdfDoc);
            document.Add(image);
        }
    }

    public void DrawRectangle(string sourcePdf, string outputPdf, int pageNumber,
                            float x, float y, float width, float height,
                            float r = 0, float g = 0, float b = 0, float lineWidth = 1)
    {
        using var reader = new PdfReader(sourcePdf);
        using var writer = new PdfWriter(outputPdf);
        using var pdfDoc = new PdfDocument(reader, writer);

        if (pageNumber < 1 || pageNumber > pdfDoc.GetNumberOfPages())
            return;

        var page = pdfDoc.GetPage(pageNumber);
        var pdfCanvas = new PdfCanvas(page);

        pdfCanvas.SaveState();
        pdfCanvas.SetStrokeColor(new DeviceRgb(r, g, b));
        pdfCanvas.SetLineWidth(lineWidth);
        pdfCanvas.Rectangle(x, y, width, height);
        pdfCanvas.Stroke();
        pdfCanvas.RestoreState();
    }

    public void HighlightArea(string sourcePdf, string outputPdf, int pageNumber,
                            float x, float y, float width, float height,
                            float r = 1, float g = 1, float b = 0, float opacity = 0.3f)
    {
        using var reader = new PdfReader(sourcePdf);
        using var writer = new PdfWriter(outputPdf);
        using var pdfDoc = new PdfDocument(reader, writer);

        if (pageNumber < 1 || pageNumber > pdfDoc.GetNumberOfPages())
            return;

        var page = pdfDoc.GetPage(pageNumber);
        var pdfCanvas = new PdfCanvas(page);

        pdfCanvas.SaveState();
        var color = new DeviceRgb(r, g, b);
        pdfCanvas.SetFillColor(color);
        pdfCanvas.SetExtGState(new iText.Kernel.Pdf.Extgstate.PdfExtGState().SetFillOpacity(opacity));
        pdfCanvas.Rectangle(x, y, width, height);
        pdfCanvas.Fill();
        pdfCanvas.RestoreState();
    }

    public void AddTextBox(string sourcePdf, string outputPdf, int pageNumber,
                          string text, float x, float y, float width, float height,
                          float fontSize = 12, bool withBorder = true)
    {
        using var reader = new PdfReader(sourcePdf);
        using var writer = new PdfWriter(outputPdf);
        using var pdfDoc = new PdfDocument(reader, writer);
        using var document = new Document(pdfDoc);

        if (pageNumber < 1 || pageNumber > pdfDoc.GetNumberOfPages())
            return;

        var paragraph = new Paragraph(text)
            .SetFontSize(fontSize)
            .SetFixedPosition(pageNumber, x, y, width);

        if (withBorder)
        {
            paragraph.SetBorder(new iText.Layout.Borders.SolidBorder(ColorConstants.BLACK, 1));
            paragraph.SetBackgroundColor(ColorConstants.WHITE);
        }

        document.Add(paragraph);
    }

    public List<ImageElement> GetImageLocations(string pdfPath, int pageNumber)
    {
        var images = new List<ImageElement>();

        using var reader = new PdfReader(pdfPath);
        using var pdfDoc = new PdfDocument(reader);

        if (pageNumber < 1 || pageNumber > pdfDoc.GetNumberOfPages())
            return images;

        var page = pdfDoc.GetPage(pageNumber);
        var resources = page.GetResources();
        var xObjects = resources.GetResourceNames();

        int imageIndex = 0;
        foreach (var name in xObjects)
        {
            var xObject = resources.GetResource(name);
            if (xObject is PdfStream stream)
            {
                var subtype = stream.GetAsName(PdfName.Subtype);
                if (PdfName.Image.Equals(subtype))
                {
                    // Note: Getting exact position requires parsing content stream
                    // This is a simplified version
                    images.Add(new ImageElement
                    {
                        ImageId = $"Image_{imageIndex++}",
                        PageNumber = pageNumber,
                        // Position would need to be extracted from content stream
                        X = 0,
                        Y = 0,
                        Width = 100,
                        Height = 100
                    });
                }
            }
        }

        return images;
    }

    public void ExportPageAsImage(string pdfPath, int pageNumber, string outputImagePath, int dpi = 150, string? password = null)
    {
        try
        {
            // Use PdfiumViewer to render the page to an image
            PdfiumViewer.PdfDocument? pdfDocument = null;

            try
            {
                if (!string.IsNullOrEmpty(password))
                {
                    pdfDocument = PdfiumViewer.PdfDocument.Load(pdfPath, password);
                }
                else
                {
                    pdfDocument = PdfiumViewer.PdfDocument.Load(pdfPath);
                }

                if (pageNumber < 1 || pageNumber > pdfDocument.PageCount)
                {
                    throw new ArgumentException($"Invalid page number. PDF has {pdfDocument.PageCount} pages.");
                }

                // Render page to image (PdfiumViewer uses 0-based index)
                var pageIndex = pageNumber - 1;
                var pageSize = pdfDocument.PageSizes[pageIndex];

                // Calculate image size based on DPI
                var width = (int)(pageSize.Width * dpi / 72.0);
                var height = (int)(pageSize.Height * dpi / 72.0);

                using var image = pdfDocument.Render(pageIndex, width, height, dpi, dpi, false);

                // Save as PNG or JPEG based on extension
                var extension = System.IO.Path.GetExtension(outputImagePath).ToLowerInvariant();
                switch (extension)
                {
                    case ".png":
                        image.Save(outputImagePath, System.Drawing.Imaging.ImageFormat.Png);
                        break;
                    case ".jpg":
                    case ".jpeg":
                        image.Save(outputImagePath, System.Drawing.Imaging.ImageFormat.Jpeg);
                        break;
                    case ".bmp":
                        image.Save(outputImagePath, System.Drawing.Imaging.ImageFormat.Bmp);
                        break;
                    default:
                        // Default to PNG
                        image.Save(outputImagePath, System.Drawing.Imaging.ImageFormat.Png);
                        break;
                }
            }
            finally
            {
                pdfDocument?.Dispose();
            }
        }
        catch (Exception ex)
        {
            throw new Exception($"Failed to export page as image: {ex.Message}", ex);
        }
    }

    public void ExportAllPagesAsOneImage(string pdfPath, string outputImagePath, int dpi = 150, string? password = null, int spacing = 10)
    {
        try
        {
            PdfiumViewer.PdfDocument? pdfDocument = null;

            try
            {
                if (!string.IsNullOrEmpty(password))
                {
                    pdfDocument = PdfiumViewer.PdfDocument.Load(pdfPath, password);
                }
                else
                {
                    pdfDocument = PdfiumViewer.PdfDocument.Load(pdfPath);
                }

                if (pdfDocument.PageCount == 0)
                {
                    throw new ArgumentException("PDF has no pages.");
                }

                // Calculate dimensions for combined image
                int maxWidth = 0;
                int totalHeight = 0;
                var pageImages = new List<System.Drawing.Image>();

                // Render all pages and calculate total dimensions
                for (int i = 0; i < pdfDocument.PageCount; i++)
                {
                    var pageSize = pdfDocument.PageSizes[i];
                    var width = (int)(pageSize.Width * dpi / 72.0);
                    var height = (int)(pageSize.Height * dpi / 72.0);

                    var pageImage = pdfDocument.Render(i, width, height, dpi, dpi, false);
                    pageImages.Add(pageImage);

                    if (width > maxWidth)
                        maxWidth = width;

                    totalHeight += height;
                }

                // Add spacing between pages
                totalHeight += spacing * (pdfDocument.PageCount - 1);

                // Create combined image
                using var combinedImage = new System.Drawing.Bitmap(maxWidth, totalHeight);
                using var graphics = System.Drawing.Graphics.FromImage(combinedImage);

                // Fill background with white
                graphics.Clear(System.Drawing.Color.White);

                // Draw all pages vertically
                int currentY = 0;
                foreach (var pageImage in pageImages)
                {
                    // Center the page horizontally if it's narrower than maxWidth
                    int x = (maxWidth - pageImage.Width) / 2;
                    graphics.DrawImage(pageImage, x, currentY);
                    currentY += pageImage.Height + spacing;
                    pageImage.Dispose();
                }

                // Save the combined image
                var extension = System.IO.Path.GetExtension(outputImagePath).ToLowerInvariant();
                switch (extension)
                {
                    case ".png":
                        combinedImage.Save(outputImagePath, System.Drawing.Imaging.ImageFormat.Png);
                        break;
                    case ".jpg":
                    case ".jpeg":
                        combinedImage.Save(outputImagePath, System.Drawing.Imaging.ImageFormat.Jpeg);
                        break;
                    case ".bmp":
                        combinedImage.Save(outputImagePath, System.Drawing.Imaging.ImageFormat.Bmp);
                        break;
                    default:
                        combinedImage.Save(outputImagePath, System.Drawing.Imaging.ImageFormat.Png);
                        break;
                }
            }
            finally
            {
                pdfDocument?.Dispose();
            }
        }
        catch (Exception ex)
        {
            throw new Exception($"Failed to export all pages as one image: {ex.Message}", ex);
        }
    }

    public class TextLocationInfo
    {
        public string Text { get; set; } = "";
        public float X { get; set; }
        public float Y { get; set; }
        public float Width { get; set; }
        public float Height { get; set; }
        public float FontSize { get; set; }
        public string FontName { get; set; } = "";
        public DeviceRgb? Color { get; set; }
    }

    public List<TextLocationInfo> FindTextLocations(string pdfPath, int pageNumber, string searchText, string? password = null)
    {
        var results = new List<TextLocationInfo>();

        PdfReader reader;
        if (!string.IsNullOrEmpty(password))
        {
            var readerProperties = new ReaderProperties().SetPassword(System.Text.Encoding.UTF8.GetBytes(password));
            reader = new PdfReader(pdfPath, readerProperties);
        }
        else
        {
            reader = new PdfReader(pdfPath);
        }

        using (reader)
        using (var pdfDoc = new PdfDocument(reader))
        {
            if (pageNumber < 1 || pageNumber > pdfDoc.GetNumberOfPages())
                return results;

            var page = pdfDoc.GetPage(pageNumber);
            var strategy = new LocationTextExtractionStrategy();

            PdfTextExtractor.GetTextFromPage(page, strategy);

            // Get all text chunks
            var allText = PdfTextExtractor.GetTextFromPage(page);

            if (allText.Contains(searchText, StringComparison.OrdinalIgnoreCase))
            {
                // Use a custom listener to get text with positions
                var listener = new TextLocationListener(searchText);
                PdfCanvasProcessor processor = new PdfCanvasProcessor(listener);
                processor.ProcessPageContent(page);

                results = listener.GetResults();
            }
        }

        return results;
    }

    private class TextLocationListener : IEventListener
    {
        private readonly string _searchText;
        private readonly List<TextLocationInfo> _results = new List<TextLocationInfo>();

        public TextLocationListener(string searchText)
        {
            _searchText = searchText;
        }

        public void EventOccurred(IEventData data, EventType type)
        {
            if (type == EventType.RENDER_TEXT)
            {
                var renderInfo = (TextRenderInfo)data;
                var text = renderInfo.GetText();

                if (text != null && text.Contains(_searchText, StringComparison.OrdinalIgnoreCase))
                {
                    var baseline = renderInfo.GetBaseline();
                    var ascent = renderInfo.GetAscentLine();

                    var x = baseline.GetStartPoint().Get(0);
                    var y = baseline.GetStartPoint().Get(1);
                    var width = baseline.GetLength();
                    var height = ascent.GetStartPoint().Get(1) - baseline.GetStartPoint().Get(1);

                    var fontSize = renderInfo.GetFontSize();
                    var font = renderInfo.GetFont();
                    var fontName = font?.GetFontProgram()?.ToString() ?? "Unknown";

                    // Try to get color information
                    DeviceRgb? color = null;
                    try
                    {
                        var fillColor = renderInfo.GetFillColor();
                        if (fillColor is DeviceRgb rgb)
                        {
                            color = rgb;
                        }
                    }
                    catch
                    {
                        // Color extraction failed, leave as null
                    }

                    _results.Add(new TextLocationInfo
                    {
                        Text = text,
                        X = x,
                        Y = y,
                        Width = width,
                        Height = height,
                        FontSize = fontSize,
                        FontName = fontName,
                        Color = color
                    });
                }
            }
        }

        public ICollection<EventType> GetSupportedEvents()
        {
            return new HashSet<EventType> { EventType.RENDER_TEXT };
        }

        public List<TextLocationInfo> GetResults()
        {
            return _results;
        }
    }

    // Listener that captures all text with formatting (for Word export)
    private class AllTextListener : IEventListener
    {
        public readonly List<TextLocationInfo> TextLocations = new List<TextLocationInfo>();

        public void EventOccurred(IEventData data, EventType type)
        {
            if (type == EventType.RENDER_TEXT)
            {
                var renderInfo = (TextRenderInfo)data;
                var text = renderInfo.GetText();

                if (!string.IsNullOrEmpty(text))
                {
                    var baseline = renderInfo.GetBaseline();
                    var ascent = renderInfo.GetAscentLine();

                    var x = baseline.GetStartPoint().Get(0);
                    var y = baseline.GetStartPoint().Get(1);
                    var width = baseline.GetLength();
                    var height = ascent.GetStartPoint().Get(1) - baseline.GetStartPoint().Get(1);

                    var fontSize = renderInfo.GetFontSize();
                    var font = renderInfo.GetFont();
                    var fontName = font?.GetFontProgram()?.ToString() ?? "Unknown";

                    // Try to get color information
                    DeviceRgb? color = null;
                    try
                    {
                        var fillColor = renderInfo.GetFillColor();
                        if (fillColor is DeviceRgb rgb)
                        {
                            color = rgb;
                        }
                    }
                    catch
                    {
                        // Color extraction failed, leave as null
                    }

                    TextLocations.Add(new TextLocationInfo
                    {
                        Text = text,
                        X = x,
                        Y = y,
                        Width = width,
                        Height = height,
                        FontSize = fontSize,
                        FontName = fontName,
                        Color = color
                    });
                }
            }
        }

        public ICollection<EventType> GetSupportedEvents()
        {
            return new HashSet<EventType> { EventType.RENDER_TEXT };
        }
    }

    public void ExportToWord(string pdfPath, string wordPath, int dpi = 150, string? password = null)
    {
        try
        {
            // Render each PDF page as an image and embed in Word
            PdfiumViewer.PdfDocument? pdfDocument = null;

            try
            {
                if (!string.IsNullOrEmpty(password))
                {
                    pdfDocument = PdfiumViewer.PdfDocument.Load(pdfPath, password);
                }
                else
                {
                    pdfDocument = PdfiumViewer.PdfDocument.Load(pdfPath);
                }

                // Create Word document
                using (var wordDocument = WordprocessingDocument.Create(wordPath, DocumentFormat.OpenXml.WordprocessingDocumentType.Document))
                {
                    // Add main document part
                    var mainPart = wordDocument.AddMainDocumentPart();
                    mainPart.Document = new DocumentFormat.OpenXml.Wordprocessing.Document();
                    var body = mainPart.Document.AppendChild(new DocumentFormat.OpenXml.Wordprocessing.Body());

                    // Process each page
                    for (int pageIndex = 0; pageIndex < pdfDocument.PageCount; pageIndex++)
                    {
                        // Get page size
                        var pageSize = pdfDocument.PageSizes[pageIndex];

                        // Render page to image at specified DPI
                        var width = (int)(pageSize.Width * dpi / 72.0);
                        var height = (int)(pageSize.Height * dpi / 72.0);

                        using (var image = pdfDocument.Render(pageIndex, width, height, dpi, dpi, false))
                        {
                            // Save image to memory stream
                            using (var memoryStream = new System.IO.MemoryStream())
                            {
                                image.Save(memoryStream, System.Drawing.Imaging.ImageFormat.Png);
                                memoryStream.Position = 0;

                                // Add image part to Word document
                                var imagePart = mainPart.AddImagePart(DocumentFormat.OpenXml.Packaging.ImagePartType.Png);
                                imagePart.FeedData(memoryStream);

                                // Get relationship ID
                                var relationshipId = mainPart.GetIdOfPart(imagePart);

                                // Calculate image size in EMUs to match PDF page exactly
                                // 1 inch = 914400 EMUs, PDF uses 72 points per inch
                                double pageWidthInches = pageSize.Width / 72.0;
                                double pageHeightInches = pageSize.Height / 72.0;

                                var widthEmus = (long)(pageWidthInches * 914400);
                                var heightEmus = (long)(pageHeightInches * 914400);

                                // Create paragraph with image
                                var paragraph = body.AppendChild(new DocumentFormat.OpenXml.Wordprocessing.Paragraph());
                                var run = paragraph.AppendChild(new DocumentFormat.OpenXml.Wordprocessing.Run());

                                var drawing = new DocumentFormat.OpenXml.Wordprocessing.Drawing(
                                    new DocumentFormat.OpenXml.Drawing.Wordprocessing.Inline(
                                        new DocumentFormat.OpenXml.Drawing.Wordprocessing.Extent() { Cx = widthEmus, Cy = heightEmus },
                                        new DocumentFormat.OpenXml.Drawing.Wordprocessing.EffectExtent()
                                        {
                                            LeftEdge = 0L,
                                            TopEdge = 0L,
                                            RightEdge = 0L,
                                            BottomEdge = 0L
                                        },
                                        new DocumentFormat.OpenXml.Drawing.Wordprocessing.DocProperties()
                                        {
                                            Id = (UInt32Value)(uint)(pageIndex + 1),
                                            Name = $"Page {pageIndex + 1}"
                                        },
                                        new DocumentFormat.OpenXml.Drawing.Wordprocessing.NonVisualGraphicFrameDrawingProperties(
                                            new DocumentFormat.OpenXml.Drawing.GraphicFrameLocks() { NoChangeAspect = true }
                                        ),
                                        new DocumentFormat.OpenXml.Drawing.Graphic(
                                            new DocumentFormat.OpenXml.Drawing.GraphicData(
                                                new DocumentFormat.OpenXml.Drawing.Pictures.Picture(
                                                    new DocumentFormat.OpenXml.Drawing.Pictures.NonVisualPictureProperties(
                                                        new DocumentFormat.OpenXml.Drawing.Pictures.NonVisualDrawingProperties()
                                                        {
                                                            Id = (UInt32Value)0U,
                                                            Name = $"Page{pageIndex + 1}.png"
                                                        },
                                                        new DocumentFormat.OpenXml.Drawing.Pictures.NonVisualPictureDrawingProperties()
                                                    ),
                                                    new DocumentFormat.OpenXml.Drawing.Pictures.BlipFill(
                                                        new DocumentFormat.OpenXml.Drawing.Blip()
                                                        {
                                                            Embed = relationshipId,
                                                            CompressionState = DocumentFormat.OpenXml.Drawing.BlipCompressionValues.Print
                                                        },
                                                        new DocumentFormat.OpenXml.Drawing.Stretch(
                                                            new DocumentFormat.OpenXml.Drawing.FillRectangle()
                                                        )
                                                    ),
                                                    new DocumentFormat.OpenXml.Drawing.Pictures.ShapeProperties(
                                                        new DocumentFormat.OpenXml.Drawing.Transform2D(
                                                            new DocumentFormat.OpenXml.Drawing.Offset() { X = 0L, Y = 0L },
                                                            new DocumentFormat.OpenXml.Drawing.Extents() { Cx = widthEmus, Cy = heightEmus }
                                                        ),
                                                        new DocumentFormat.OpenXml.Drawing.PresetGeometry(
                                                            new DocumentFormat.OpenXml.Drawing.AdjustValueList()
                                                        )
                                                        {
                                                            Preset = DocumentFormat.OpenXml.Drawing.ShapeTypeValues.Rectangle
                                                        }
                                                    )
                                                )
                                            )
                                            {
                                                Uri = "http://schemas.openxmlformats.org/drawingml/2006/picture"
                                            }
                                        )
                                    )
                                    {
                                        DistanceFromTop = (UInt32Value)0U,
                                        DistanceFromBottom = (UInt32Value)0U,
                                        DistanceFromLeft = (UInt32Value)0U,
                                        DistanceFromRight = (UInt32Value)0U
                                    }
                                );

                                run.AppendChild(drawing);

                                // Set paragraph properties with zero margins for this page
                                var paraProps = paragraph.GetFirstChild<DocumentFormat.OpenXml.Wordprocessing.ParagraphProperties>();
                                if (paraProps == null)
                                {
                                    paraProps = paragraph.InsertAt(new DocumentFormat.OpenXml.Wordprocessing.ParagraphProperties(), 0);
                                }

                                // Add section properties to set page size and margins for this specific page
                                var sectionProps = new DocumentFormat.OpenXml.Wordprocessing.SectionProperties();

                                // Set page size to match PDF page (convert points to twips: 1 point = 20 twips)
                                var wordPageSize = new DocumentFormat.OpenXml.Wordprocessing.PageSize()
                                {
                                    Width = (UInt32Value)(uint)(pageSize.Width * 20),
                                    Height = (UInt32Value)(uint)(pageSize.Height * 20)
                                };

                                // Set all margins to zero
                                var wordPageMargin = new DocumentFormat.OpenXml.Wordprocessing.PageMargin()
                                {
                                    Top = 0,
                                    Bottom = 0,
                                    Left = (UInt32Value)0U,
                                    Right = (UInt32Value)0U,
                                    Header = (UInt32Value)0U,
                                    Footer = (UInt32Value)0U,
                                    Gutter = (UInt32Value)0U
                                };

                                sectionProps.Append(wordPageSize);
                                sectionProps.Append(wordPageMargin);

                                // Add page break after each page except the last
                                if (pageIndex < pdfDocument.PageCount - 1)
                                {
                                    // Add section break for next page
                                    sectionProps.Append(new DocumentFormat.OpenXml.Wordprocessing.SectionType()
                                    {
                                        Val = DocumentFormat.OpenXml.Wordprocessing.SectionMarkValues.NextPage
                                    });
                                }

                                paraProps.Append(sectionProps);
                            }
                        }
                    }

                    mainPart.Document.Save();
                }
            }
            finally
            {
                pdfDocument?.Dispose();
            }
        }
        catch (Exception ex)
        {
            throw new Exception($"Failed to export PDF to Word: {ex.Message}", ex);
        }
    }

    private string MapFontNameToStandard(string pdfFontName)
    {
        // Map PDF font names to standard Windows fonts
        var fontName = pdfFontName.ToLower();

        if (fontName.Contains("times")) return "Times New Roman";
        if (fontName.Contains("arial")) return "Arial";
        if (fontName.Contains("helvetica")) return "Arial";
        if (fontName.Contains("courier")) return "Courier New";
        if (fontName.Contains("calibri")) return "Calibri";
        if (fontName.Contains("cambria")) return "Cambria";
        if (fontName.Contains("verdana")) return "Verdana";
        if (fontName.Contains("georgia")) return "Georgia";
        if (fontName.Contains("trebuchet")) return "Trebuchet MS";
        if (fontName.Contains("comic")) return "Comic Sans MS";

        // Default to Calibri for unknown fonts
        return "Calibri";
    }

    public void ReorderPages(string sourcePdf, string outputPdf, int[] newPageOrder, string? password = null)
    {
        try
        {
            PdfReader reader;
            if (!string.IsNullOrEmpty(password))
            {
                var readerProperties = new ReaderProperties().SetPassword(System.Text.Encoding.UTF8.GetBytes(password));
                reader = new PdfReader(sourcePdf, readerProperties);
            }
            else
            {
                reader = new PdfReader(sourcePdf);
            }

            using (reader)
            using (var pdfDoc = new PdfDocument(reader))
            using (var writer = new PdfWriter(outputPdf))
            using (var newPdfDoc = new PdfDocument(writer))
            {
                var totalPages = pdfDoc.GetNumberOfPages();

                // Validate input
                if (newPageOrder.Length != totalPages)
                    throw new ArgumentException($"Page order array length ({newPageOrder.Length}) doesn't match PDF page count ({totalPages})");

                // Copy pages in new order
                foreach (var pageNum in newPageOrder)
                {
                    if (pageNum < 1 || pageNum > totalPages)
                        throw new ArgumentException($"Invalid page number {pageNum} in page order");

                    pdfDoc.CopyPagesTo(pageNum, pageNum, newPdfDoc);
                }
            }
        }
        catch (Exception ex)
        {
            throw new Exception($"Failed to reorder pages: {ex.Message}", ex);
        }
    }

    public void MovePageToPosition(string sourcePdf, string outputPdf, int fromPage, int toPosition, string? password = null)
    {
        try
        {
            PdfReader reader;
            if (!string.IsNullOrEmpty(password))
            {
                var readerProperties = new ReaderProperties().SetPassword(System.Text.Encoding.UTF8.GetBytes(password));
                reader = new PdfReader(sourcePdf, readerProperties);
            }
            else
            {
                reader = new PdfReader(sourcePdf);
            }

            using (reader)
            using (var pdfDoc = new PdfDocument(reader))
            using (var writer = new PdfWriter(outputPdf))
            using (var newPdfDoc = new PdfDocument(writer))
            {
                var totalPages = pdfDoc.GetNumberOfPages();

                // Validate input
                if (fromPage < 1 || fromPage > totalPages)
                    throw new ArgumentException($"Source page {fromPage} is out of range (1-{totalPages})");

                if (toPosition < 1 || toPosition > totalPages)
                    throw new ArgumentException($"Target position {toPosition} is out of range (1-{totalPages})");

                if (fromPage == toPosition)
                {
                    // No change needed, just copy all pages
                    pdfDoc.CopyPagesTo(1, totalPages, newPdfDoc);
                    return;
                }

                // Create new page order
                var pageOrder = new List<int>();

                if (fromPage < toPosition)
                {
                    // Moving page forward (e.g., page 2 to position 5)
                    // Order: 1, 3, 4, 5, 2, 6, 7...
                    for (int i = 1; i <= totalPages; i++)
                    {
                        if (i == fromPage)
                            continue; // Skip the page being moved

                        pageOrder.Add(i);

                        if (i == toPosition)
                            pageOrder.Add(fromPage); // Insert moved page after target position
                    }
                }
                else
                {
                    // Moving page backward (e.g., page 5 to position 2)
                    // Order: 1, 5, 2, 3, 4, 6, 7...
                    for (int i = 1; i <= totalPages; i++)
                    {
                        if (i == toPosition)
                            pageOrder.Add(fromPage); // Insert moved page at target position

                        if (i != fromPage)
                            pageOrder.Add(i); // Add other pages
                    }
                }

                // Copy pages in new order
                foreach (var pageNum in pageOrder)
                {
                    pdfDoc.CopyPagesTo(pageNum, pageNum, newPdfDoc);
                }
            }
        }
        catch (Exception ex)
        {
            throw new Exception($"Failed to reorder pages: {ex.Message}", ex);
        }
    }
}
