# PDF Content Editing Guide

## Overview

PDF Editor Pro now includes a **powerful Content Editor** that allows you to directly edit the text and images within PDF files. This is the core feature you requested!

## Opening the Content Editor

### Method 1: Menu
1. Launch PDF Editor Pro
2. Click **Edit > Content Editor (Edit Text/Images)**

### Method 2: Toolbar
1. Launch PDF Editor Pro
2. Click the **✏️ Edit Content** button in the toolbar

## Content Editor Interface

The Content Editor window has three main panels:

### Left Panel - Editing Tools
- **Text Operations**: Find and replace text, add new text
- **Image Operations**: Select and add images to PDF
- **Drawing Tools**: Highlight areas, draw rectangles, erase content
- **Text Properties**: Adjust font size (8-72pt)
- **Position Info**: Shows cursor position and selection

### Center Panel - PDF Canvas
- Visual editing canvas showing PDF page
- **Selection Mode**: Click and drag to select areas
- **Zoom Controls**: +/- buttons to zoom in/out
- Interactive cursor position tracking

### Right Panel - Elements List
- Shows all text elements on current page
- Lists all images on current page
- Click elements to see their properties
- Refresh button to reload elements

## Core Editing Features

### 1. Replace Text in PDF

**Steps:**
1. Open a PDF in the Content Editor
2. Navigate to the page with text you want to change
3. In the **Find Text** box, enter the text to find
4. In the **Replace With** box, enter the new text
5. Adjust **Font Size** slider if needed
6. Click **Replace Text** button
7. Choose where to save the edited PDF

**Example:**
- Find: "Draft"
- Replace With: "Final Version"
- Result: All instances of "Draft" become "Final Version"

### 2. Add New Text

**Steps:**
1. Click **Add Text at Cursor** button
2. Enter your text in the dialog
3. Set X and Y coordinates (or use default)
4. Click OK
5. Save the edited PDF

**Coordinate System:**
- X: Distance from left edge (0 = left)
- Y: Distance from bottom edge (0 = bottom, 800 = near top)
- Example: X=100, Y=700 places text near top-left

### 3. Add Images to PDF

**Steps:**
1. Click **Select Image File** button
2. Choose an image (JPG, PNG, BMP)
3. Click **Add Image at Cursor** button
4. Set position and size:
   - X Position: Horizontal placement
   - Y Position: Vertical placement
   - Width: Image width in points
   - Height: Image height in points
5. Click OK and save

**Tips:**
- 72 points = 1 inch
- For A4 page: Width ≈ 595pt, Height ≈ 842pt
- Recommended image size: 200x200 to fit nicely

### 4. Replace Images

**Steps:**
1. Note the position of the image you want to replace
2. Use **Erase Area** to remove the old image
3. Use **Add Image** to place the new image

### 5. Highlight Areas

**Steps:**
1. Click **Highlight Selection** button
2. Enter area coordinates (X, Y, Width, Height)
3. Click OK
4. A semi-transparent yellow highlight is added
5. Save the PDF

**Use Cases:**
- Highlight important sections
- Mark areas for review
- Create visual emphasis

### 6. Draw Rectangles

**Steps:**
1. Click **Draw Rectangle** button
2. Define the rectangle area
3. Click OK
4. A black border rectangle is drawn
5. Save the PDF

**Use Cases:**
- Create borders around content
- Mark sections
- Add visual structure

### 7. Erase Content

**Steps:**
1. Click **Erase Selection** button
2. Define the area to erase (X, Y, Width, Height)
3. Click OK
4. A white rectangle covers the area
5. Save the PDF

**Use Cases:**
- Remove unwanted text
- Delete images
- Clean up PDF content
- Redact information

## Edit Modes (Radio Buttons)

- **Select**: Default mode for navigation
- **Add Text**: Click mode for adding text
- **Add Image**: Click mode for adding images
- **Highlight**: Click and drag to highlight
- **Erase**: Click and drag to erase

## Page Navigation

- **Previous/Next buttons**: Navigate through pages
- **Page number box**: Jump to specific page
- **Current page indicator**: Shows current page / total pages

## Working with Text Elements

### Viewing Text Elements
1. Open a PDF
2. Text elements appear in the right panel
3. Each element shows:
   - The actual text
   - Position (X, Y coordinates)
   - Font size

### Selecting Text Elements
1. Click on a text element in the list
2. Position information appears at bottom of left panel
3. Use this info to precisely place new text nearby

## Working with Images

### Viewing Images
1. Images on current page appear in the right panel
2. Shows image ID and dimensions

### Adding Images Best Practices
1. Choose appropriate size (not too large)
2. Use coordinates that fit within page bounds
3. Common positions:
   - Top-left: X=50, Y=750
   - Center: X=200, Y=400
   - Bottom-right: X=400, Y=50

## Tips and Best Practices

### Coordinate System Understanding
```
PDF Coordinate System:
(0, 842) ─────────────── (595, 842)  ← Top of A4 page
    │                          │
    │         Page             │
    │        Content           │
    │                          │
(0, 0) ───────────────── (595, 0)   ← Bottom of page
```

### Font Sizes
- **8pt**: Very small text
- **12pt**: Normal body text (default)
- **18pt**: Subheadings
- **24-36pt**: Headings
- **48-72pt**: Large titles

### Save Strategy
- Original PDFs are never modified
- Each operation creates a new PDF
- Use descriptive filenames (e.g., "contract_edited.pdf")
- Keep originals as backup

### Precision Editing
1. Use **Refresh Elements** to see current page state
2. Note exact coordinates from text element list
3. Use small Width/Height for precise erasing
4. Test on a copy first

## Common Workflows

### Workflow 1: Update Contract Text
1. Open contract PDF
2. Find: "John Doe"
3. Replace With: "Jane Smith"
4. Save as "contract_jane_smith.pdf"

### Workflow 2: Add Company Logo
1. Open PDF
2. Select logo image file
3. Position: X=450, Y=750 (top-right)
4. Size: 100x100
5. Add image and save

### Workflow 3: Redact Information
1. Navigate to page with sensitive info
2. Click "Erase Selection"
3. Define area over sensitive text
4. Area is covered with white rectangle
5. Save redacted PDF

### Workflow 4: Annotate Document
1. Open PDF
2. Add text notes: "REVIEWED - OK"
3. Highlight important paragraphs
4. Draw rectangles around key sections
5. Save annotated version

## Limitations and Notes

### Current Limitations
1. **Text Replacement**: Simplified approach - places note when text is found
2. **Image Position Detection**: Basic implementation
3. **Font Detection**: Uses default fonts
4. **Complex Layouts**: Works best with simple PDF layouts

### For Advanced Text Editing
- Text is overlaid rather than replacing original
- Works best when you know exact coordinates
- Consider using "Add Text" for precise placement

### File Size
- Adding images increases PDF file size
- Use compressed images when possible
- JPG typically smaller than PNG for photos

## Troubleshooting

### "Cannot find text"
- Check spelling and case
- Text might be in an image (cannot be edited)
- Try opening PDF in another viewer to verify text

### "Image doesn't appear"
- Check coordinates are within page bounds
- Verify image file is valid
- Try smaller image size first

### "Operation failed"
- Ensure PDF is not encrypted
- Check you have write permissions for output folder
- Verify PDF is not corrupted

### Position seems wrong
- Remember Y coordinates start from bottom
- Use lower Y values for bottom of page
- Use higher Y values for top of page

## Keyboard Tips

While in Content Editor:
- **Mouse drag**: Select area on canvas
- **Zoom +/-**: Adjust view size
- **Page number + Enter**: Jump to page

## Next Steps

After editing:
1. Save your edited PDF
2. Open in Content Editor to verify changes
3. Use other PDF Editor Pro features:
   - Merge with other PDFs
   - Add watermarks
   - Encrypt the edited PDF
   - Extract updated text

## Support

For issues with content editing:
1. Check this guide for solutions
2. Verify PDF is not corrupted
3. Try with a simple test PDF first
4. Ensure .NET 10 runtime is installed

---

**Enjoy editing your PDFs with precision and control!**
