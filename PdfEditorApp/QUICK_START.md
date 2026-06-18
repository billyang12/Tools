# Quick Start Guide - PDF Editor Pro

## Running the Application

### Option 1: Using dotnet run
```bash
cd C:\workspace\PdfEditorApp
dotnet run
```

### Option 2: Using the compiled executable
```bash
cd C:\workspace\PdfEditorApp
dotnet build
.\bin\Debug\net10.0-windows\PdfEditorApp.exe
```

## First Steps

1. **Open a PDF** - Click the "📂 Open" button or go to File > Open PDF
2. **Try basic operations**:
   - Navigate through pages using Previous/Next buttons
   - Extract text from current page
   - Rotate pages
   - Add a watermark

## Quick Feature Overview

| Feature | Location | Description |
|---------|----------|-------------|
| **Open PDF** | Toolbar / File menu | Load a PDF file |
| **Merge PDFs** | Toolbar / Tools menu | Combine multiple PDFs |
| **Split PDF** | Toolbar / Tools menu | Split into individual pages |
| **Delete Page** | Left panel | Remove current page |
| **Rotate Page** | Left panel | Rotate 90°, 180°, or 270° |
| **Watermark** | Toolbar / Left panel | Add text watermark |
| **Encrypt** | Toolbar / Tools menu | Password protect PDF |
| **Extract Text** | Left panel | Get text from current page |
| **Images→PDF** | Toolbar / Tools menu | Convert images to PDF |

## Sample Workflows

### Merge Multiple PDFs
1. Click "🔗 Merge" in toolbar
2. Select multiple PDF files (hold Ctrl)
3. Choose output filename
4. Click Save

### Protect a PDF with Password
1. Open your PDF
2. Click "🔒 Encrypt" in toolbar
3. Enter user password (required to open)
4. Enter owner password (optional)
5. Choose output filename
6. Click Save

### Extract Text from a Document
1. Open your PDF
2. Navigate to desired page
3. Click "📝 Extract Text (Current Page)"
4. View extracted text in the bottom panel

### Create PDF from Photos
1. Click "🖼️ Images→PDF" in toolbar
2. Select one or more image files
3. Choose output PDF filename
4. Click Save

## Technologies Used

- **.NET 10.0** - Latest .NET framework
- **WPF** - Modern Windows UI
- **iText7 9.6.0** - PDF manipulation
- **MVVM Pattern** - Clean architecture

## System Requirements

- Windows 10 or Windows 11
- .NET 10.0 Runtime (SDK for development)
- 2 GB RAM minimum
- 100 MB disk space

## Tips

- All operations create **new files** - originals are never modified
- Use descriptive filenames when saving edited PDFs
- For large PDFs, operations may take a few seconds
- Encrypted PDFs require the password to be opened

## Troubleshooting

**Application won't start?**
- Ensure .NET 10.0 runtime is installed
- Check Windows version compatibility

**PDF won't open?**
- Verify file is a valid PDF
- Check if PDF is encrypted/password protected

**Operation failed?**
- Ensure you have write permissions to output folder
- Check disk space availability
- Verify PDF isn't corrupted

## Support

For issues or questions, refer to the main README.md file.

---

**Built with ❤️ using C# .NET 10 and WPF**
