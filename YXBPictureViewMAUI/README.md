# XPG Picture Viewer - MAUI Mobile App

Cross-platform mobile app for viewing, encrypting, and decrypting XPG image files using AES-256-GCM encryption.

## Features

✅ **View XPG Images**: Browse and view encrypted XPG files  
✅ **Encrypt Images**: Convert JPG/PNG/BMP to encrypted XPG format  
✅ **Decrypt Images**: Convert XPG files back to JPG/PNG/BMP  
✅ **AES-256-GCM**: Strong encryption compatible with desktop YXBPictureViewer  
✅ **Password Derivation**: Generate keys from memorable passwords using PBKDF2  
✅ **Cross-Platform**: Android, iOS, Windows, macOS support  

## Supported Platforms

- ✅ **Android** (API 21+)
- ✅ **iOS** (15.0+)
- ✅ **Windows** (10.0.17763+)
- ✅ **macOS Catalyst** (15.0+)

## Compatibility

**Desktop ↔ Mobile**: XPG files encrypted on desktop (Windows YXBPictureViewer) can be decrypted on mobile, and vice versa. The encryption format is identical.

## Pages

### 1. View Images
- Browse folders for XPG files
- Enter password or generate new key
- Navigate through images with Previous/Next buttons
- Real-time decryption and display

### 2. Encrypt
- Select folder containing images (JPG, PNG, GIF, BMP)
- Choose source file type
- Generate random key or derive from password
- Batch encrypt with progress indication
- Optional: delete originals after encryption

### 3. Decrypt
- Select folder containing XPG files
- Enter encryption password/key
- Choose output format (JPG, PNG, BMP)
- Batch decrypt with progress indication
- Optional: delete XPG files after decryption

## Building

### Prerequisites
- .NET 10 SDK
- Visual Studio 2022 17.13+ or Visual Studio Code with .NET MAUI extension
- Platform-specific SDKs:
  - **Android**: Android SDK 21+
  - **iOS/macOS**: Xcode 15+
  - **Windows**: Windows 10 SDK

### Build Commands

```bash
# Android
dotnet build -t:Run -f net10.0-android

# iOS
dotnet build -t:Run -f net10.0-ios

# Windows
dotnet build -t:Run -f net10.0-windows10.0.19041.0

# macOS
dotnet build -t:Run -f net10.0-maccatalyst
```

## Permissions

### Android
- READ_EXTERNAL_STORAGE
- WRITE_EXTERNAL_STORAGE
- READ_MEDIA_IMAGES
- MANAGE_EXTERNAL_STORAGE

### iOS
- Photo Library Access (prompted at runtime)

## File Format

XPG files use AES-256-GCM encryption:

```
[4 bytes: original length]
[12 bytes: nonce]
[16 bytes: authentication tag]
[encrypted data]
```

## Security

- **AES-256-GCM**: Authenticated encryption prevents tampering
- **Unique Nonce**: Each encryption uses a random nonce for security
- **PBKDF2**: Password-to-key derivation with 100,000 iterations
- **No DES**: Only modern AES encryption (no legacy DES support)

## Usage Example

### Encrypting Images on Mobile

1. Open app → Navigate to "Encrypt" page
2. Click "Generate Random" or "From Password" to create encryption key
3. Copy and save the generated key securely
4. Click "Select Folder" and choose folder with images
5. Select source file type (e.g., "jpg")
6. Click "Start Encryption"
7. Wait for completion

### Viewing XPG Files

1. Navigate to "View Images" page
2. Enter your encryption password/key
3. Click "Select Folder" and choose folder with XPG files
4. Use Previous/Next buttons to browse images
5. Images decrypt and display automatically

### Decrypting on Mobile

1. Navigate to "Decrypt" page
2. Enter encryption password/key
3. Click "Select Folder with XPG Files"
4. Choose output format (JPG recommended)
5. Click "Start Decryption"
6. Wait for completion

## Troubleshooting

### "Permission Denied" on Android
- Grant storage permissions when prompted
- For Android 11+, enable "All files access" in Settings

### "Decryption Failed"
- Verify password/key is correct (case-sensitive)
- Ensure XPG file was encrypted with same key
- Check file is not corrupted

### "No Files Found"
- Verify folder contains files with correct extension
- Check "Include subfolders" if files are in subdirectories
- Ensure app has storage permissions

## Project Structure

```
YXBPictureViewMAUI/
├── Services/
│   ├── XPGEncryption.cs      # AES-256-GCM encryption
│   └── FolderPicker.cs        # Cross-platform folder picker
├── Views/
│   ├── ViewImagesPage.xaml    # View encrypted images
│   ├── EncryptPage.xaml       # Encrypt images to XPG
│   └── DecryptPage.xaml       # Decrypt XPG to images
├── Platforms/
│   ├── Android/               # Android-specific code
│   ├── iOS/                   # iOS-specific code
│   ├── Windows/               # Windows-specific code
│   └── MacCatalyst/           # macOS-specific code
└── AppShell.xaml             # App navigation shell
```

## Differences from Desktop App

**Not Included**:
- ❌ DES encryption (YPG files)
- ❌ YPG to XPG conversion
- ❌ TreeView folder browsing
- ❌ Multiple view modes (Zoom, Stretch, etc.)

**Mobile-Specific Features**:
- ✅ Touch-friendly UI
- ✅ Simplified navigation
- ✅ Platform-native folder pickers
- ✅ Progress indication
- ✅ Mobile-optimized image display

## License

Same as YXBPictureViewer desktop application.

## Support

For issues or questions, refer to the main YXBPictureViewer desktop application repository.
