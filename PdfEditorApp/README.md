# PDF Editor Pro

A comprehensive desktop PDF editor application built with **C# .NET 10.0** and **WPF**.

## 🎯 Key Highlight: Content Editing

**NEW!** Edit PDF content directly - modify text, add/replace images, and more with the built-in **Content Editor**!

## Features

### ✏️ Content Editing (NEW!)
- **Edit Text** - Replace existing text in PDFs
- **Add Text** - Insert new text at any position
- **Add Images** - Place images anywhere on the page
- **Replace Images** - Update existing images
- **Highlight Areas** - Add visual emphasis
- **Erase Content** - Remove unwanted text or images
- **Draw Shapes** - Add rectangles and borders

### Core Features
- **Open & View PDFs** - Load and navigate through PDF documents
- **Page Navigation** - Browse through pages with next/previous buttons
- **Extract Text** - Extract text content from any page

### Document Operations
- **Merge PDFs** - Combine multiple PDF files into one
- **Split PDF** - Split a PDF into individual page files
- **Delete Pages** - Remove specific pages from a PDF
- **Rotate Pages** - Rotate pages 90°, 180°, or 270°

### Advanced Features
- **Add Watermark** - Add custom watermark text to all pages
- **Encrypt PDF** - Password-protect PDFs with user and owner passwords (AES-256 encryption)
- **Convert Images to PDF** - Create PDFs from image files (JPG, PNG, BMP)

## Technology Stack

- **.NET 10.0** - Latest .NET framework
- **WPF** - Windows Presentation Foundation for rich UI
- **iText7 9.6.0** - PDF manipulation library
- **PDFiumSharp** - PDF rendering engine
- **MVVM Pattern** - Clean separation of concerns

## Requirements

- Windows 10/11
- .NET 10.0 Runtime or SDK

## Building the Application

```bash
cd PdfEditorApp
dotnet restore
dotnet build
dotnet run
```

## Quick Start - Content Editing

### Editing PDF Content
1. Click **Edit > Content Editor** or the **✏️ Edit Content** button
2. Open a PDF file in the Content Editor
3. Use the tools to:
   - **Replace text**: Enter find/replace terms and click Replace
   - **Add text**: Click "Add Text at Cursor" and specify position
   - **Add images**: Select an image file, then click "Add Image at Cursor"
   - **Highlight/Erase**: Use the respective buttons with area selection
4. Save your edited PDF

**See [CONTENT_EDITING_GUIDE.md](CONTENT_EDITING_GUIDE.md) for detailed instructions!**

## Usage

### Opening a PDF
1. Click **File > Open PDF** or the toolbar button
2. Select a PDF file from your computer
3. The file will load and display basic information

### Merging PDFs
1. Click **Tools > Merge PDFs** or the toolbar button
2. Select multiple PDF files (hold Ctrl to select multiple)
3. Choose output location and filename
4. Click Save

### Splitting a PDF
1. Open a PDF file first
2. Click **Tools > Split PDF**
3. Select an output folder
4. Each page will be saved as a separate PDF file

### Adding Watermark
1. Open a PDF file
2. Click the **Watermark** button
3. Enter your watermark text
4. Choose output location
5. Save the watermarked PDF

### Encrypting a PDF
1. Open a PDF file
2. Click **Tools > Encrypt PDF**
3. Enter user password (required to open)
4. Enter owner password (optional, for permissions)
5. Save the encrypted PDF

### Rotating Pages
1. Open a PDF and navigate to the page you want to rotate
2. Use the rotation buttons:
   - **Rotate 90° CW** - Clockwise rotation
   - **Rotate 90° CCW** - Counter-clockwise rotation
   - **Rotate 180°** - Half turn

### Converting Images to PDF
1. Click **Tools > Convert Images to PDF**
2. Select one or more image files
3. Choose output PDF filename
4. Images will be added as separate pages

## Project Structure

```
PdfEditorApp/
├── MainWindow.xaml          # Main UI layout
├── MainWindow.xaml.cs       # UI code-behind
├── MainViewModel.cs         # MVVM ViewModel with business logic
├── PdfEditorService.cs      # PDF manipulation service
├── RelayCommand.cs          # ICommand implementation
├── NullToVisibilityConverter.cs  # XAML converter
├── App.xaml                 # Application resources
└── PdfEditorApp.csproj      # Project configuration
```

## Key Components

### PdfEditorService
Core service class that handles all PDF operations using iText7:
- Page extraction and manipulation
- Text extraction
- Merging and splitting
- Encryption and watermarking
- Image to PDF conversion

### MainViewModel
MVVM ViewModel that manages:
- UI state and bindings
- Command implementations
- File dialog interactions
- User feedback and status messages

### RelayCommand
Generic ICommand implementation for WPF command binding in MVVM pattern.

## License

This is a demonstration project built for educational purposes.

## Notes

- iText7 is AGPL licensed for open source projects. For commercial use, a commercial license is required.
- The application uses AES-256 encryption for password protection
- PDF preview rendering requires additional implementation with PDFiumSharp
- All operations create new files rather than modifying originals

## Future Enhancements

Potential features to add:
- PDF page preview rendering
- Drag-and-drop page reordering
- Digital signatures
- Form field editing
- Annotation tools
- OCR (Optical Character Recognition)
- Batch processing
- PDF/A conversion
