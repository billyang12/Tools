# MainForm - New Enhanced Picture Viewer

## Overview

**MainForm** is the new enhanced form for YXBPictureViewer with improved features and dual encryption support. Form1.cs remains intact for reference/backup purposes.

---

## What's New in MainForm

### ✅ **No Fake Login**
- Removed the fake login system from Form1
- Direct access to all features
- Real password/key management for encryption

### ✅ **TreeView File Explorer**
- Left panel shows folder hierarchy
- Displays folders and image files in tree structure
- Click any file to display it instantly
- Visual folder/file icons

### ✅ **Dual Password System**
- **YPG Password**: For old DES-encrypted .ypg files
- **XPG Password**: For new AES-256-encrypted .xpg files
- Passwords stored in textboxes for easy management
- Default YPG password pre-filled: "Man@QueY"

### ✅ **Smart File Display**
- Automatically detects file type (.ypg, .xpg, or unencrypted)
- Uses appropriate decryption method
- Shows unencrypted images directly
- Error handling with clear messages

### ✅ **Manual Path Entry**
- Type or paste folder paths directly
- Useful for network paths, external drives, deeply nested folders
- "Load Path" button to load manually entered paths

### ✅ **Enhanced Operations**

1. **Encrypt Files** - Encrypt images to YPG or XPG format
2. **Convert YPG → XPG** - Migrate old DES files to new AES format
3. **Decrypt to JPG** - Batch decrypt encrypted files

---

## Features in Detail

### 1. TreeView File Explorer

**Location**: Left panel

**Features**:
- Hierarchical folder structure
- Shows all subfolders and files
- Click to view image
- Automatically loads on folder selection

**Supported Files**:
- `.ypg` - Old DES-encrypted files
- `.xpg` - New AES-256-encrypted files
- `.jpg`, `.jpeg`, `.png`, `.gif`, `.bmp`, `.tif`, `.tiff` - Unencrypted images

### 2. Password/Key Management

**YPG Password (DES Key)**:
- 8-character string
- Default: "Man@QueY"
- Used for decrypting .ypg files
- Used for converting YPG → XPG

**XPG Password (AES-256 Key)**:
- 44-character Base64 string
- Generate using `GenerateAESKey.ps1`
- Used for decrypting .xpg files
- Used for encrypting to .xpg format

### 3. Loading Folders

**Method 1: Browse**
1. Click "Browse Folder..."
2. Select folder in dialog
3. Folder loads automatically

**Method 2: Manual Entry**
1. Type or paste path in textbox
2. Examples:
   - `C:\Pictures`
   - `Y:\USB-Hard-Drive-2\Xuebing\Data\...`
   - `\\QNAP-device\share\folder`
3. Click "Load Path"

**What Gets Loaded**:
- All folders and subfolders
- All image files and encrypted files
- Shown in hierarchical tree structure

### 4. Viewing Images

**How to View**:
1. Click any file in the tree
2. Image displays automatically in right panel
3. Status bar shows file type and decryption method

**Automatic Detection**:
- `.ypg` → Decrypts with YPG password (DES)
- `.xpg` → Decrypts with XPG password (AES-256)
- Other → Displays as unencrypted image

**Error Handling**:
- Invalid password → Clear error message
- Corrupted file → Error dialog with details
- Missing password → Prompts user to enter password

### 5. Encrypt Files Operation

**What It Does**:
- Encrypts image files to .ypg or .xpg format

**Steps**:
1. Click "Encrypt Files" button
2. Dialog appears with options:
   - **Folder**: Select folder containing files
   - **Source Extension**: Choose file type (jpg, png, etc.)
   - **Encryption Type**: Choose YPG (DES) or XPG (AES-256)
   - **Include Subfolders**: Yes/No
   - **Delete Originals**: Yes/No (CAUTION!)
3. Click "Encrypt"
4. Operation completes, shows count

**Example**:
- Encrypt all `.jpg` files in a folder to `.xpg` format
- Keep original files as backup
- Process subfolders automatically

### 6. Convert YPG → XPG Operation

**What It Does**:
- Converts old DES-encrypted .ypg files to new AES-256-encrypted .xpg files

**Requirements**:
- Both YPG and XPG passwords must be entered

**Steps**:
1. Enter passwords in textboxes
2. Click "Convert YPG → XPG" button
3. Select folder containing .ypg files
4. Choose whether to include subdirectories
5. Operation runs, shows count

**Safety**:
- Original .ypg files are kept (not deleted)
- Verify .xpg files work before deleting originals

### 7. Decrypt to JPG Operation

**What It Does**:
- Decrypts encrypted files back to regular images

**Steps**:
1. Click "Decrypt Files to JPG" button
2. Dialog appears with options:
   - **Folder**: Select folder with encrypted files
   - **Files to Decrypt**: Both YPG/XPG, YPG only, or XPG only
   - **Output Extension**: Default "jpg", can change to png, etc.
   - **Include Subfolders**: Yes/No
   - **Delete Originals**: Yes/No (CAUTION!)
3. Click "Decrypt"
4. Operation completes, shows count

**Use Cases**:
- Decrypt files for use in other applications
- Create backup copies as JPG
- Migrate away from encryption

---

## UI Layout

```
┌─────────────────────────────────────────────────────────────┐
│ Folder Path: [___________________] [Browse] [Load Path]    │
│                                                             │
│ ┌─ Passwords ────┐ ┌─ Operations ──────────────────────┐  │
│ │ YPG: [______] │ │ [Encrypt Files]                   │  │
│ │ XPG: [______] │ │ [Convert YPG → XPG]               │  │
│ └───────────────┘ │ [Decrypt Files to JPG]            │  │
│                    └───────────────────────────────────┘  │
├─────────────────────────────────────────────────────────────┤
│ ┌─ TreeView ──┐ │ ┌─ Image Display ──────────────────┐ │
│ │ Folder/     │ │ │                                   │ │
│ │  ├─ image1  │ │ │                                   │ │
│ │  ├─ image2  │ │ │         [Image Preview]           │ │
│ │  └─ Sub/    │ │ │                                   │ │
│ │     ├─ pic1 │ │ │                                   │ │
│ └─────────────┘ │ └───────────────────────────────────┘ │
├─────────────────────────────────────────────────────────────┤
│ Status: Ready                                               │
└─────────────────────────────────────────────────────────────┘
```

---

## Files Created

### Main Form Files
1. **MainForm.cs** - Main form logic
2. **MainForm.Designer.cs** - UI designer code

### Dialog Files
3. **EncryptFilesDialog.cs** - Encryption dialog logic
4. **EncryptFilesDialog.Designer.cs** - Dialog designer
5. **DecryptFilesDialog.cs** - Decryption dialog logic
6. **DecryptFilesDialog.Designer.cs** - Dialog designer

### Program Entry
7. **Program.cs** - Updated to launch MainForm instead of Form1

---

## Differences from Form1

| Feature | Form1 (Old) | MainForm (New) |
|---------|-------------|----------------|
| Login | Fake login with constant password | No login, direct access |
| File Explorer | Simple list | Hierarchical TreeView |
| Passwords | Hidden constant | Visible textboxes for both YPG/XPG |
| Path Entry | Browse only | Browse + Manual text entry |
| Encryption | YPG only | Both YPG and XPG |
| Conversion | None | YPG → XPG conversion |
| Decryption | View only | Can decrypt to JPG files |
| File Detection | Manual | Automatic based on extension |
| Error Handling | Basic | Enhanced with clear messages |

---

## Usage Examples

### Example 1: View Encrypted Images

1. Launch application
2. Enter passwords:
   - YPG: `Man@QueY`
   - XPG: `[your-generated-key]`
3. Click "Browse Folder" and select your image folder
4. Click any file in tree to view
5. Status bar shows encryption type used

### Example 2: Encrypt New Images

1. Click "Encrypt Files"
2. Browse to folder with JPG images
3. Select:
   - Source Extension: `jpg`
   - Encryption Type: `XPG (AES-256 - New Method)`
   - Include Subfolders: ✓
   - Delete Originals: ✗ (keep originals)
4. Click "Encrypt"
5. Wait for completion message

### Example 3: Migrate Old YPG to New XPG

1. Ensure both passwords are entered
2. Click "Convert YPG → XPG"
3. Select folder with .ypg files
4. Choose "Yes" to include subdirectories
5. Wait for conversion
6. Original .ypg files remain as backup
7. Test opening some .xpg files to verify
8. Delete .ypg files manually if desired

### Example 4: Decrypt Files for Other Use

1. Click "Decrypt Files to JPG"
2. Browse to folder with encrypted files
3. Select:
   - Files to Decrypt: `Both YPG and XPG`
   - Output Extension: `jpg`
   - Include Subfolders: ✓
   - Delete Originals: ✗
4. Click "Decrypt"
5. JPG files created alongside encrypted files

---

## Tips and Best Practices

### Password Management

✅ **DO**:
- Generate XPG password using `GenerateAESKey.ps1`
- Save passwords in a secure password manager
- Back up your keys securely
- Test decryption before deleting originals

❌ **DON'T**:
- Share your XPG password/key
- Delete originals without verifying conversions
- Use weak or simple keys for XPG
- Commit keys to version control

### File Operations

✅ **DO**:
- Test on a few files first
- Keep backups during conversion
- Verify encrypted files open correctly
- Use descriptive folder names

❌ **DON'T**:
- Enable "Delete Originals" without testing
- Convert all files at once without testing
- Forget to save your encryption keys
- Mix different encryption keys for same folder

### Network/External Drives

✅ **DO**:
- Use manual path entry for network drives
- Copy full path from File Explorer
- Test path accessibility first
- Be patient with network delays

❌ **DON'T**:
- Expect folder browser to always work with network paths
- Encrypt on unstable network connections
- Process thousands of files over slow connections

---

## Troubleshooting

### "Password Required" Error
**Cause**: Password textbox is empty  
**Solution**: Enter the appropriate password in the textbox at the top

### "Decryption returned empty data"
**Cause**: Wrong password or corrupted file  
**Solution**: 
- Check password is correct
- Try opening file with old Form1 to verify
- File may be corrupted - check backups

### "Folder browser error"
**Cause**: System issue with folder dialog  
**Solution**: Use manual path entry instead
- Copy path from File Explorer
- Paste in textbox
- Click "Load Path"

### Images don't display
**Cause**: Wrong file format or missing password  
**Solution**:
- Check file extension (.ypg, .xpg, or regular image)
- Ensure correct password is entered
- Check status bar for error messages

### Conversion fails
**Cause**: Missing passwords or file access issues  
**Solution**:
- Enter both YPG and XPG passwords
- Check file permissions
- Ensure enough disk space
- Close other programs accessing files

---

## Keyboard Shortcuts

Currently, no keyboard shortcuts are implemented. All operations are mouse-driven through buttons and tree clicks.

---

## Status Bar Messages

The status bar at the bottom shows:
- `Ready` - Application idle
- `Loaded: [path]` - Folder loaded successfully
- `Displayed (YPG/DES): [file]` - Showed YPG file
- `Displayed (XPG/AES): [file]` - Showed XPG file
- `Displayed (unencrypted): [file]` - Showed regular image
- `Encrypted X files` - Batch encryption result
- `Converted X files` - Conversion result
- `Decrypted X files` - Decryption result
- `Error: [message]` - Error occurred

---

## Security Notes

### Encryption Strength
- **YPG (DES)**: Weak, legacy, for backward compatibility only
- **XPG (AES-256-GCM)**: Strong, modern, recommended for new files

### Password Storage
- Passwords are stored in memory during application runtime
- Not persisted to disk
- Enter passwords each time application starts
- Consider using Windows Credential Manager for secure storage

### File Safety
- Always test operations on copies first
- Keep backups of important files
- Verify decryption before deleting originals
- Use "Include Subfolders" carefully

---

## Future Enhancements (Potential)

- [ ] Remember last used folder
- [ ] Save passwords securely (encrypted config file)
- [ ] Drag-and-drop folder loading
- [ ] Image zoom and pan controls
- [ ] Slideshow mode
- [ ] Thumbnail view
- [ ] Batch rename functionality
- [ ] Search/filter files in tree
- [ ] Context menu on tree items
- [ ] Keyboard shortcuts

---

## Technical Details

### Technology Stack
- **.NET 10.0** (or compatible)
- **Windows Forms** UI framework
- **AES-256-GCM** for XPG encryption
- **DES** for YPG backward compatibility

### Performance
- Loads folders dynamically
- Images decrypted on-demand
- Memory management with proper disposal
- Efficient for folders with hundreds of files

### Compatibility
- Windows 10/11
- .NET Framework 4.7.2+ or .NET Core 3.0+
- Works with mapped drives, network paths, external drives

---

## Getting Started

1. **Generate XPG Key** (one-time):
   ```powershell
   .\GenerateAESKey.ps1
   ```

2. **Launch Application**:
   - Run `YXBPictureViewer.exe`
   - MainForm opens automatically

3. **Enter Passwords**:
   - YPG: `Man@QueY` (or your custom DES key)
   - XPG: [paste your generated key]

4. **Load Folder**:
   - Browse or enter path manually
   - Click Load

5. **Start Viewing**:
   - Click any file in tree
   - Image displays on right

That's it! You're ready to use YXBPictureViewer with MainForm! 🎉

---

**Last Updated**: 2026-07-27  
**Version**: 1.0  
**Form1 Status**: Intact and unchanged (kept for reference)
