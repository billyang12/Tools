# Setup Instructions for XPG Picture Viewer MAUI App

## Installation Steps

### 1. Install Required Workloads

Before building the app, install .NET MAUI workloads:

```bash
cd "C:\Bill Yang\Tools\YXBPictureViewMAUI\YXBPictureViewMAUI"
dotnet workload restore
```

Or manually install MAUI workload:

```bash
dotnet workload install maui
```

For Android specifically:

```bash
dotnet workload install maui-android
```

### 2. Build the Project

Once workloads are installed:

```bash
# For Android
dotnet build -f net10.0-android

# To run on Android emulator or device
dotnet build -t:Run -f net10.0-android
```

### 3. Verify Installation

Check installed workloads:

```bash
dotnet workload list
```

You should see:
- `maui` or `maui-android` installed
- Version should match .NET 10.0

## Files Created

### Core Services

**Services/XPGEncryption.cs**
- AES-256-GCM encryption implementation
- Compatible with desktop YXBPictureViewer
- Methods: GenerateKey(), DeriveKeyFromPassword(), EncryptFileAsync(), DecryptFileAsync()

**Services/FolderPicker.cs**
- Cross-platform folder picker
- Platform-specific implementations for Android/iOS/Windows
- Returns folder path for file operations

### UI Pages

**Views/ViewImagesPage.xaml/.cs**
- Browse and view encrypted XPG images
- Password entry with generate/derive options
- Previous/Next navigation
- Real-time image decryption and display

**Views/EncryptPage.xaml/.cs**
- Select source folder
- Choose file type (jpg, png, gif, bmp)
- Generate/derive encryption key
- Batch encrypt with progress bar
- Optional: delete originals

**Views/DecryptPage.xaml/.cs**
- Select folder with XPG files
- Enter decryption password
- Choose output format
- Batch decrypt with progress bar
- Optional: delete XPG files

### Configuration

**AppShell.xaml**
- Navigation structure with 3 main pages
- Flyout menu for easy navigation

**Platforms/Android/AndroidManifest.xml**
- Storage permissions (READ_EXTERNAL_STORAGE, WRITE_EXTERNAL_STORAGE)
- Media permissions (READ_MEDIA_IMAGES)
- Management permissions (MANAGE_EXTERNAL_STORAGE)

## Features Implemented

### ✅ View Images
- Browse folders for XPG files
- Enter password or generate new key
- Navigate with Previous/Next buttons
- Automatic decryption and display
- Loading indicator during decryption

### ✅ Encrypt Images
- Select source folder
- Support multiple image formats
- Generate random AES-256 key
- Derive key from password (PBKDF2)
- Include subfolders option
- Delete originals option (with warning)
- Progress bar with file count
- Batch processing

### ✅ Decrypt Images
- Select folder with XPG files
- Enter encryption password/key
- Choose output format (jpg/png/bmp)
- Include subfolders option
- Delete XPG files option (with warning)
- Progress bar with file count
- Batch processing

### ✅ Security Features
- AES-256-GCM authenticated encryption
- PBKDF2 password derivation (100,000 iterations)
- Unique nonce per encryption
- Base64 key storage
- Password masking in UI
- Clipboard support for keys

## Compatibility

### Desktop → Mobile
- XPG files encrypted on Windows desktop app can be opened on mobile
- Same encryption format (AES-256-GCM)
- Same file structure

### Mobile → Desktop
- XPG files encrypted on mobile can be opened on desktop app
- Compatible with YXBPictureViewer

## Known Limitations

### Android
- Folder picker defaults to Pictures directory
- Android 11+ requires "All files access" permission
- Large files (>10MB) may take longer to process

### iOS
- Limited folder access (sandboxed)
- Uses Documents directory by default
- Photo library permissions required

### General
- No DES/YPG support (AES/XPG only)
- No TreeView folder browsing
- Single view mode (AspectFit)
- No zoom controls (use pinch gesture on mobile)

## Testing

### Test Encryption
1. Open app
2. Go to "Encrypt" page
3. Click "Generate Random" to create key
4. Save the key (copy to clipboard)
5. Select folder with test images (jpg/png)
6. Click "Start Encryption"
7. Verify .xpg files created

### Test Viewing
1. Go to "View Images" page
2. Paste the encryption key
3. Select folder with .xpg files
4. Images should display
5. Use Previous/Next to browse

### Test Decryption
1. Go to "Decrypt" page
2. Enter encryption key
3. Select folder with .xpg files
4. Choose output format (jpg)
5. Click "Start Decryption"
6. Verify .jpg files created

### Test Desktop Compatibility
1. Encrypt image on desktop YXBPictureViewer
2. Transfer .xpg file to mobile device
3. Open in mobile app with same password
4. Should display correctly

## Troubleshooting

### Build Errors

**"workloads must be installed"**
```bash
dotnet workload restore
```

**"Android SDK not found"**
- Install Android SDK via Visual Studio Installer
- Or install Android Studio

**"iOS SDK not found" (macOS only)**
- Install Xcode from App Store
- Run `xcode-select --install`

### Runtime Errors

**"Permission Denied"**
- Grant storage permissions when prompted
- For Android 11+, enable "All files access"

**"Decryption Failed"**
- Verify correct password/key
- Check file is valid XPG format
- Ensure file not corrupted

**"No Files Found"**
- Check folder path is correct
- Verify files have .xpg extension
- Try including subfolders

## Next Steps

1. Install required workloads
2. Build for target platform
3. Test encryption/decryption
4. Verify desktop compatibility
5. Deploy to device/emulator

## Additional Notes

- Project uses .NET 10 (latest)
- MAUI version from SDK
- Minimum Android API 21
- Minimum iOS 15.0
- Uses platform-specific folder pickers
- Progress indication for long operations
- Error handling with user-friendly messages

## Support

For build issues, check:
1. .NET 10 SDK installed
2. MAUI workload installed
3. Platform SDKs installed (Android/Xcode)
4. Visual Studio 2022 17.13+ or VS Code with MAUI extension

For functionality issues, refer to README.md and desktop app documentation.
