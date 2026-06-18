using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Canvas.Parser;
using iText.Kernel.Pdf.Canvas.Parser.Listener;
using iText.Layout;
using iText.Layout.Element;
using iText.Kernel.Geom;
using iText.Kernel.Pdf.Xobject;
using iText.IO.Image;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace PdfEditorApp;

public class PdfEditorService
{
    public class PdfPageInfo
    {
        public int PageNumber { get; set; }
        public string? PreviewImagePath { get; set; }
        public float Width { get; set; }
        public float Height { get; set; }
    }

    public List<PdfPageInfo> GetPdfPages(string pdfPath)
    {
        var pages = new List<PdfPageInfo>();

        using var pdfReader = new PdfReader(pdfPath);
        using var pdfDoc = new PdfDocument(pdfReader);

        for (int i = 1; i <= pdfDoc.GetNumberOfPages(); i++)
        {
            var page = pdfDoc.GetPage(i);
            var pageSize = page.GetPageSize();

            pages.Add(new PdfPageInfo
            {
                PageNumber = i,
                Width = pageSize.GetWidth(),
                Height = pageSize.GetHeight()
            });
        }

        return pages;
    }

    public string ExtractTextFromPage(string pdfPath, int pageNumber)
    {
        using var pdfReader = new PdfReader(pdfPath);
        using var pdfDoc = new PdfDocument(pdfReader);

        if (pageNumber < 1 || pageNumber > pdfDoc.GetNumberOfPages())
            return string.Empty;

        var page = pdfDoc.GetPage(pageNumber);
        var strategy = new LocationTextExtractionStrategy();
        return PdfTextExtractor.GetTextFromPage(page, strategy);
    }

    public void MergePdfs(List<string> sourcePdfs, string outputPath)
    {
        using var pdfWriter = new PdfWriter(outputPath);
        using var pdfDoc = new PdfDocument(pdfWriter);

        foreach (var sourcePdf in sourcePdfs)
        {
            using var sourceReader = new PdfReader(sourcePdf);
            using var sourceDoc = new PdfDocument(sourceReader);

            sourceDoc.CopyPagesTo(1, sourceDoc.GetNumberOfPages(), pdfDoc);
        }
    }

    public void SplitPdf(string sourcePdf, string outputFolder, List<int> pageNumbers)
    {
        using var pdfReader = new PdfReader(sourcePdf);
        using var sourceDoc = new PdfDocument(pdfReader);

        foreach (var pageNum in pageNumbers)
        {
            if (pageNum < 1 || pageNum > sourceDoc.GetNumberOfPages())
                continue;

            var outputPath = System.IO.Path.Combine(outputFolder, $"page_{pageNum}.pdf");
            using var writer = new PdfWriter(outputPath);
            using var newDoc = new PdfDocument(writer);

            sourceDoc.CopyPagesTo(pageNum, pageNum, newDoc);
        }
    }

    public void DeletePages(string sourcePdf, string outputPdf, List<int> pageNumbers)
    {
        using var reader = new PdfReader(sourcePdf);
        using var writer = new PdfWriter(outputPdf);
        using var pdfDoc = new PdfDocument(reader, writer);

        // Sort in descending order to avoid index shifting issues
        var sortedPages = pageNumbers.OrderByDescending(p => p).ToList();

        foreach (var pageNum in sortedPages)
        {
            if (pageNum >= 1 && pageNum <= pdfDoc.GetNumberOfPages())
            {
                pdfDoc.RemovePage(pageNum);
            }
        }
    }

    public void RotatePages(string sourcePdf, string outputPdf, List<int> pageNumbers, int rotation)
    {
        using var reader = new PdfReader(sourcePdf);
        using var writer = new PdfWriter(outputPdf);
        using var pdfDoc = new PdfDocument(reader, writer);

        foreach (var pageNum in pageNumbers)
        {
            if (pageNum >= 1 && pageNum <= pdfDoc.GetNumberOfPages())
            {
                var page = pdfDoc.GetPage(pageNum);
                var currentRotation = page.GetRotation();
                page.SetRotation((currentRotation + rotation) % 360);
            }
        }
    }

    public void AddWatermark(string sourcePdf, string outputPdf, string watermarkText)
    {
        using var reader = new PdfReader(sourcePdf);
        using var writer = new PdfWriter(outputPdf);
        using var pdfDoc = new PdfDocument(reader, writer);
        using var document = new Document(pdfDoc);

        for (int i = 1; i <= pdfDoc.GetNumberOfPages(); i++)
        {
            var page = pdfDoc.GetPage(i);
            var pageSize = page.GetPageSize();

            var watermark = new Paragraph(watermarkText)
                .SetFontSize(60)
                .SetFontColor(iText.Kernel.Colors.ColorConstants.LIGHT_GRAY)
                .SetOpacity(0.3f);

            document.ShowTextAligned(watermark,
                pageSize.GetWidth() / 2,
                pageSize.GetHeight() / 2,
                i,
                iText.Layout.Properties.TextAlignment.CENTER,
                iText.Layout.Properties.VerticalAlignment.MIDDLE,
                45);
        }
    }

    public void AddTextToPdf(string sourcePdf, string outputPdf, string text, float x, float y, int pageNumber)
    {
        using var reader = new PdfReader(sourcePdf);
        using var writer = new PdfWriter(outputPdf);
        using var pdfDoc = new PdfDocument(reader, writer);
        using var document = new Document(pdfDoc);

        if (pageNumber >= 1 && pageNumber <= pdfDoc.GetNumberOfPages())
        {
            var paragraph = new Paragraph(text).SetFontSize(12);
            document.ShowTextAligned(paragraph, x, y, pageNumber,
                iText.Layout.Properties.TextAlignment.LEFT,
                iText.Layout.Properties.VerticalAlignment.TOP, 0);
        }
    }

    public void ConvertImagesToPdf(List<string> imagePaths, string outputPdf)
    {
        using var writer = new PdfWriter(outputPdf);
        using var pdfDoc = new PdfDocument(writer);
        using var document = new Document(pdfDoc);

        foreach (var imagePath in imagePaths)
        {
            var imageData = ImageDataFactory.Create(imagePath);
            var image = new iText.Layout.Element.Image(imageData);

            // Scale image to fit page
            var pageSize = PageSize.A4;
            image.ScaleToFit(pageSize.GetWidth() - 72, pageSize.GetHeight() - 72);

            document.Add(image);
            document.Add(new AreaBreak(iText.Layout.Properties.AreaBreakType.NEXT_PAGE));
        }
    }

    public void EncryptPdf(string sourcePdf, string outputPdf, string userPassword, string ownerPassword)
    {
        using var reader = new PdfReader(sourcePdf);
        using var writer = new PdfWriter(outputPdf,
            new WriterProperties()
                .SetStandardEncryption(
                    System.Text.Encoding.UTF8.GetBytes(userPassword),
                    System.Text.Encoding.UTF8.GetBytes(ownerPassword),
                    EncryptionConstants.ALLOW_PRINTING,
                    EncryptionConstants.ENCRYPTION_AES_256));
        using var pdfDoc = new PdfDocument(reader, writer);
    }

    public int GetPageCount(string pdfPath, string? password = null)
    {
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
            return pdfDoc.GetNumberOfPages();
        }
    }

    public bool IsPasswordProtected(string pdfPath)
    {
        try
        {
            using var reader = new PdfReader(pdfPath);
            using var pdfDoc = new PdfDocument(reader);
            return false; // If we can open it without password, it's not protected
        }
        catch (iText.Kernel.Exceptions.BadPasswordException)
        {
            return true; // Password required
        }
        catch (iText.Kernel.Exceptions.PdfException ex) when (ex.Message.Contains("password") || ex.Message.Contains("encryption") || ex.Message.Contains("Bad user password"))
        {
            return true; // Password required
        }
        catch (Exception ex) when (ex.Message.Contains("password") || ex.Message.Contains("encryption"))
        {
            return true; // Password required
        }
        catch
        {
            return false; // Other error, not password-related
        }
    }
}
