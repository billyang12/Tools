# Building Signed Release APK for XPG Picture Viewer

## Prerequisites

1. **JDK installed** (comes with Android SDK)
2. **Keystore file created** (one-time setup)

## Step 1: Create Keystore (One-Time Setup)

Open Command Prompt and run:

```bash
keytool -genkey -v -keystore YXBPictureViewer.keystore -alias xpgviewer -keyalg RSA -keysize 2048 -validity 10000
```

You will be prompted for:
- **Keystore password**: (choose a strong password and SAVE IT!)
- **Key password**: (can be same as keystore password or different - SAVE IT!)
- **Your name**: Bill Yang
- **Organizational unit**: (optional, press Enter)
- **Organization**: (optional, press Enter)  
- **City**: (your city)
- **State**: (your state)
- **Country code**: (e.g., US, CN, etc.)

**IMPORTANT**: 
- Store the keystore file safely: `C:\Bill Yang\Keys\YXBPictureViewer.keystore`
- **NEVER lose this file or passwords** - you need them for ALL future updates
- If you lose the keystore, you cannot update your app on Google Play Store

## Step 2: Verify Project Configuration

The project file `.csproj` has been configured with:
- Keystore location: `C:\Bill Yang\Keys\YXBPictureViewer.keystore`
- Key alias: `xpgviewer`

Passwords are left empty in the project file for security (you'll enter them during build).

## Step 3: Build Release APK

### Option A: Using Visual Studio

1. **Set configuration to Release**:
   - At the top toolbar, change from "Debug" to "Release"

2. **Select Android framework**:
   - Change from "Any CPU" to specific Android target

3. **Build the APK**:
   - Right-click on the project → **Publish** → **Android** → **Ad Hoc**
   - OR use menu: **Build** → **Archive for Publishing**
   
4. **Sign the APK**:
   - Visual Studio will prompt for:
     - Keystore password
     - Key password
   - Enter the passwords you created in Step 1

5. **Save the APK**:
   - The signed APK will be saved to:
     `bin\Release\net10.0-android\publish\`

### Option B: Using Command Line

1. Open Command Prompt in the project directory:
   ```bash
   cd "C:\Bill Yang\Tools\YXBPictureViewMAUI\YXBPictureViewMAUI"
   ```

2. Build and publish with signing:
   ```bash
   dotnet publish -f net10.0-android -c Release /p:AndroidSigningKeyStore="C:\Bill Yang\Keys\YXBPictureViewer.keystore" /p:AndroidSigningKeyAlias=xpgviewer /p:AndroidSigningKeyPass=YOUR_KEY_PASSWORD /p:AndroidSigningStorePass=YOUR_STORE_PASSWORD
   ```

   Replace `YOUR_KEY_PASSWORD` and `YOUR_STORE_PASSWORD` with your actual passwords.

3. Find the signed APK at:
   ```
   bin\Release\net10.0-android\publish\com.yxb.xpgpictureviewer-Signed.apk
   ```

## Step 4: Test the APK

Before distributing, test the signed APK:

```bash
adb install -r "bin\Release\net10.0-android\publish\com.yxb.xpgpictureviewer-Signed.apk"
```

## Step 5: Distribute

Your signed APK is ready for distribution:

- **Google Play Store**: Upload the APK/AAB through Google Play Console
- **Direct Distribution**: Share the APK file directly with users
- **Website Download**: Host the APK on your website

## Building AAB for Google Play Store

Google Play Store prefers AAB (Android App Bundle) format:

```bash
dotnet publish -f net10.0-android -c Release /p:AndroidPackageFormat=aab /p:AndroidSigningKeyStore="C:\Bill Yang\Keys\YXBPictureViewer.keystore" /p:AndroidSigningKeyAlias=xpgviewer /p:AndroidSigningKeyPass=YOUR_KEY_PASSWORD /p:AndroidSigningStorePass=YOUR_STORE_PASSWORD
```

The AAB file will be at: `bin\Release\net10.0-android\publish\com.yxb.xpgpictureviewer-Signed.aab`

## Version Updates

When releasing updates:

1. Update version in `YXBPictureViewMAUI.csproj`:
   ```xml
   <ApplicationDisplayVersion>1.1</ApplicationDisplayVersion>
   <ApplicationVersion>2</ApplicationVersion>
   ```
   - `ApplicationDisplayVersion`: User-visible version (e.g., 1.0, 1.1, 2.0)
   - `ApplicationVersion`: Version code (integer, must increase: 1, 2, 3...)

2. **Use the SAME keystore** for updates (or users cannot update the app)

## Security Notes

- **Never commit keystore file to git/source control**
- **Never commit passwords to git/source control**
- Keep backups of your keystore in multiple safe locations
- Consider using environment variables for passwords:
  ```bash
  set KEYSTORE_PASSWORD=your_password
  set KEY_PASSWORD=your_key_password
  ```
  Then use `%KEYSTORE_PASSWORD%` and `%KEY_PASSWORD%` in commands

## Troubleshooting

**"keytool not found"**:
- Add Java/JDK bin directory to PATH:
  `C:\Program Files\Java\jdk-XX.X.X\bin`

**"Keystore was tampered with, or password was incorrect"**:
- You entered the wrong password
- Keystore file is corrupted

**"Failed to sign APK"**:
- Check that keystore path is correct
- Verify passwords are correct
- Make sure you're building in Release mode

## App Information

- **App Name**: XPG Picture Viewer
- **Package ID**: com.yxb.xpgpictureviewer
- **Current Version**: 1.0
- **Minimum Android Version**: 5.0 (API 21)
- **Target Android Version**: Latest
