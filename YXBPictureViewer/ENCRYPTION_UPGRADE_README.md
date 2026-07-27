# Encryption Upgrade Guide

## Overview

This project now supports two encryption methods:
- **Old Method (DES)**: Used for `.ypg` files - maintained for backward compatibility
- **New Method (AES-256-GCM)**: Used for `.xpg` files - modern, secure encryption

## What Changed

### New Files Created

1. **YAESEncrypt.cs** - New encryption class using AES-256-GCM
2. **FileConverter.cs** - Utility class for converting .ypg to .xpg files
3. **ENCRYPTION_UPGRADE_README.md** - This documentation file

### Modified Files

1. **Form1.cs** - Updated to support both .ypg and .xpg files
   - Added `aesKey` field for AES encryption
   - Updated `ShowPicture()` to decrypt both .ypg (DES) and .xpg (AES) files
   - Updated `EncriptFile()` to encrypt to .xpg using AES when specified
   - Updated `LoadFolderToTree()` to show both .ypg and .xpg files

## How to Use

### Step 1: Generate a New AES Key

You need to generate a proper AES-256 key. You can do this in two ways:

**Option A: Using C# code**
```csharp
string newKey = YAESEncrypt.GenerateKey();
Console.WriteLine($"Your new AES key: {newKey}");
```

**Option B: Using PowerShell (from project directory)**
```powershell
# Generate a new AES key
$key = [System.Security.Cryptography.Aes]::Create()
$key.KeySize = 256
$key.GenerateKey()
$base64Key = [Convert]::ToBase64String($key.Key)
Write-Host "Your new AES key: $base64Key"
$key.Dispose()
```

### Step 2: Update the AES Key in Form1.cs

Replace the placeholder key in `Form1.cs`:

```csharp
public static string aesKey = "YOUR_GENERATED_BASE64_KEY_HERE";
```

### Step 3: Choose Your Workflow

#### Workflow A: Keep Old Files, Create New Encrypted Files

1. Open the application
2. Select files to encrypt
3. Choose `.xpg` as the output extension
4. Your original files remain untouched
5. New `.xpg` files are created with AES encryption

#### Workflow B: Convert Existing .ypg Files to .xpg

Use the FileConverter utility:

```csharp
// Convert a single file
string desKey = "Man@QueY";  // Your old DES key
string aesKey = "YOUR_GENERATED_BASE64_KEY";  // Your new AES key
FileConverter.ConvertYPGtoXPG("path/to/file.ypg", "path/to/file.xpg", desKey, aesKey);

// Convert all files in a directory
int converted = FileConverter.ConvertAllYPGInDirectory(
    "path/to/directory", 
    desKey, 
    aesKey, 
    includeSubdirectories: true,  // Process subdirectories
    deleteOriginal: false         // Keep original .ypg files
);
Console.WriteLine($"Converted {converted} files");
```

### Step 4: Viewing Files

The application now supports both formats:
- **Old .ypg files**: Decrypted using DES with the old key
- **New .xpg files**: Decrypted using AES-256-GCM with the new key

When you check "Only YPG" in the UI, it will now show both .ypg and .xpg files.

## Security Considerations

### Why Upgrade to AES-256-GCM?

1. **Stronger Encryption**: AES-256 is much more secure than DES (56-bit key)
2. **Authentication**: AES-GCM provides authentication, preventing tampering
3. **Modern Standard**: AES is the current encryption standard recommended by security experts
4. **Future-Proof**: DES is deprecated and considered insecure

### Key Management

**IMPORTANT**: 
- Keep your AES key secure and backed up
- Do NOT hardcode keys in production code - use secure configuration
- Consider using .NET's Protected Configuration or Azure Key Vault for production
- The key in Form1.cs is for development only - replace with secure storage

### Recommendations

1. **Test First**: Convert a few files first and verify they decrypt correctly
2. **Backup**: Keep backups of your original .ypg files until you're confident
3. **Gradual Migration**: You can use both formats simultaneously during transition
4. **Delete Old Files**: Only delete .ypg files after confirming .xpg files work correctly

## Technical Details

### DES Encryption (Old - .ypg)
- Algorithm: DES (Data Encryption Standard)
- Key Size: 64 bits (56 effective bits)
- Key Format: 8-character ASCII string
- File Format: [4 bytes length][encrypted data]

### AES-256-GCM Encryption (New - .xpg)
- Algorithm: AES-256-GCM (Advanced Encryption Standard - Galois/Counter Mode)
- Key Size: 256 bits
- Key Format: Base64-encoded string (44 characters)
- File Format: [4 bytes length][12 bytes nonce][16 bytes auth tag][encrypted data]
- Benefits: Authenticated encryption, prevents tampering

## Troubleshooting

### "Failed to decrypt data" Error

**For .ypg files:**
- Verify the old DES key is correct (should be "Man@QueY" by default)
- Check if the file is corrupted

**For .xpg files:**
- Verify the AES key is correct
- Ensure the key is properly Base64-encoded
- Check if the file was encrypted with a different key

### "Key must be exactly X bytes" Error

**For DES (.ypg):**
- Key must be exactly 8 characters

**For AES (.xpg):**
- Key must be a valid Base64 string representing 32 bytes (256 bits)
- Use `YAESEncrypt.GenerateKey()` to create a valid key

## Example Code Snippets

### Generate Key and Convert Files
```csharp
// Generate a new AES key (do this once)
string newAESKey = YAESEncrypt.GenerateKey();
Console.WriteLine($"New AES Key (save this securely): {newAESKey}");

// Convert all .ypg files in a directory to .xpg
string oldDESKey = "Man@QueY";
string directory = @"C:\Your\Image\Directory";

int count = FileConverter.ConvertAllYPGInDirectory(
    directory, 
    oldDESKey, 
    newAESKey, 
    includeSubdirectories: true,
    deleteOriginal: false  // Set to true to delete .ypg after successful conversion
);

Console.WriteLine($"Successfully converted {count} files");
```

### Encrypt New Files to .xpg Format
```csharp
// In Form1.cs, when encrypting files, specify "xpg" as the extension
EncryptAllFiles(selectedPath, "jpg", "xpg", aesKey, true, false);
```

## Migration Checklist

- [ ] Generate a new AES-256 key using `YAESEncrypt.GenerateKey()`
- [ ] Store the key securely and make a backup
- [ ] Update `Form1.cs` with the new AES key
- [ ] Test the application with a sample file
- [ ] Convert a few .ypg files to .xpg for testing
- [ ] Verify the .xpg files decrypt correctly
- [ ] Gradually convert all .ypg files to .xpg
- [ ] Keep .ypg files as backup until fully migrated
- [ ] Document your key storage location

## Support

If you encounter issues:
1. Check the Debug output for detailed error messages
2. Verify your keys are correct
3. Ensure files are not corrupted
4. Test with a fresh encryption to verify the system works
