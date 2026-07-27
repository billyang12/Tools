# AES Key Options - Three Ways to Provide Your Key

## Overview

You have **three flexible options** for providing your XPG (AES-256) encryption key:

1. ✅ **Type your own key** (if you already have one)
2. ✅ **Generate a random key** (most secure)
3. ✅ **Derive from password** (most memorable)

---

## Option 1: Type Your Own Key

### Requirements
- Must be valid Base64 format
- Must represent exactly 32 bytes (256 bits)
- Typically 44 characters long
- Characters allowed: A-Z, a-z, 0-9, +, /, =

### How to Use
1. If you already have a Base64-encoded AES-256 key, just paste it
2. In MainForm, paste into the "XPG Password" textbox
3. Start encrypting/decrypting

### Example Valid Keys
```
1234567890abcdefghijklmnopqrstuv1234567890==
rJv3xK7mP2nQ8wY5tZ9hF6cV1bN4sD0gL8kJ2mX7==
```

### How to Validate
The application will tell you if your key is invalid when you try to use it.

---

## Option 2: Generate Random Key (New Feature!)

### What It Does
- Generates a cryptographically secure random 256-bit key
- Automatically fills the XPG password textbox
- Most secure option

### How to Use
1. Click **"Generate Random"** button (below XPG password textbox)
2. Key is automatically generated and filled in
3. **IMPORTANT**: Save the generated key immediately!
4. Use this key for all future encryption/decryption

### Example
```
Click "Generate Random" button
→ Key appears: "K8m2nP5qR9tY1wZ4xC7vB0aD3fG6hJ8kL1mN4pQ7sT=="
→ Save this key in a secure location
→ Use it for encrypting files
```

### Advantages
- ✅ Maximum security (true random)
- ✅ No need to remember anything
- ✅ One click

### Disadvantages
- ❌ Must save and store the key
- ❌ If lost, cannot decrypt files
- ❌ Hard to type manually if needed

---

## Option 3: Derive from Password (New Feature!)

### What It Does
- Converts your memorable password into a valid AES-256 key
- Uses PBKDF2 (Password-Based Key Derivation Function)
- Same password always produces same key

### How to Use
1. Click **"From Password..."** button (below XPG password textbox)
2. Dialog appears asking for password
3. Enter your password (any length, any characters)
4. Click "Derive Key"
5. Key is automatically generated and filled in

### Example
```
Click "From Password..." button
→ Dialog: "Enter your password"
→ Type: "MySecretPassword123!"
→ Click "Derive Key"
→ Key appears: "sT7mK2nP5qR9tY1wZ4xC7vB0aD3fG6hJ8kL1=="
→ Every time you use "MySecretPassword123!", you get the same key!
```

### Advantages
- ✅ Easy to remember (just remember your password)
- ✅ Don't need to save/store the Base64 key
- ✅ Can regenerate key anytime from password
- ✅ Works on any computer (same password = same key)

### Disadvantages
- ❌ Password must be strong enough
- ❌ Must remember password exactly (case-sensitive)
- ❌ Slightly less secure than true random

---

## Comparison Table

| Feature | Type Own | Generate Random | From Password |
|---------|----------|-----------------|---------------|
| **Security** | Depends on key | Highest | Good (if strong password) |
| **Convenience** | Low | Medium | Highest |
| **Memorability** | None | None | High |
| **Portability** | Must copy key | Must copy key | Just remember password |
| **Recovery** | Need backup | Need backup | Regenerate from password |
| **Best For** | Advanced users | Maximum security | Ease of use |

---

## Which Option Should You Choose?

### Choose **Type Your Own** if:
- ✅ You already have a valid AES-256 key
- ✅ You're migrating from another system
- ✅ You need to share keys with others
- ✅ You have a key management system

### Choose **Generate Random** if:
- ✅ You want maximum security
- ✅ You can store keys securely (password manager, etc.)
- ✅ You're okay with managing Base64 keys
- ✅ You're encrypting very sensitive data

### Choose **From Password** if:
- ✅ You want something memorable
- ✅ You don't want to manage Base64 keys
- ✅ You use the application on multiple computers
- ✅ You're okay with slightly less security
- ✅ You can create a strong password

---

## Step-by-Step Examples

### Example 1: Using a Memorable Password

**Scenario**: You want to encrypt files but don't want to manage complex keys.

1. Open YXBPictureViewer
2. In Passwords section, click **"From Password..."** button
3. Enter your password: `MyDog&CatLive@Home2026!`
4. Click "Derive Key"
5. Key is filled in automatically
6. Encrypt your files
7. **Next time**: Just enter the same password to get the same key!

### Example 2: Maximum Security

**Scenario**: You're encrypting highly sensitive documents.

1. Open YXBPictureViewer
2. In Passwords section, click **"Generate Random"** button
3. Key appears: `K8m2nP5qR9tY1wZ4xC7vB0aD3fG6hJ8kL1mN4pQ7sT==`
4. **IMMEDIATELY** save this key:
   - Copy to password manager
   - Save to secure note
   - Write on paper and lock in safe
5. Use this key for encryption
6. **Next time**: Paste the saved key from your password manager

### Example 3: Using an Existing Key

**Scenario**: You already have an AES-256 key from another system.

1. Copy your existing key
2. Open YXBPictureViewer
3. Paste key into "XPG Password" textbox
4. Start using immediately

---

## Password Strength Guidelines (for "From Password" option)

### Weak Passwords (DON'T USE)
❌ `password`  
❌ `12345678`  
❌ `myname`  
❌ Single words from dictionary  

### Medium Passwords (OKAY)
⚠️ `MyPassword123`  
⚠️ `JohnSmith2026`  
⚠️ Two words + numbers  

### Strong Passwords (RECOMMENDED)
✅ `MyDog&3Cats!LiveAt#42MainSt`  
✅ `I<3Coffee!Drink@7AM&3PM`  
✅ `Pizza+Pasta=Italy2026!`  
✅ Use: Length (15+ chars) + Symbols + Numbers + MixedCase  

---

## How Key Derivation Works (Technical)

When you click "From Password...":

1. You enter password: `MySecretPass123`
2. Application uses **PBKDF2** with:
   - Salt: `YXBPictureViewer-Salt-2026` (fixed)
   - Iterations: 100,000 (slows down brute-force)
   - Hash: SHA-256
   - Output: 32 bytes (256 bits)
3. Result is Base64-encoded: `sT7mK2nP5qR9tY1...`
4. This becomes your AES-256 encryption key

**Important**: Same password + same salt = **same key every time**

This means:
- ✅ You can regenerate your key anytime
- ✅ Works on any computer
- ✅ No need to back up the key itself, just remember password

---

## Key Storage Best Practices

### If Using Generated Random Key

**DO**:
- ✅ Save in password manager (1Password, LastPass, Bitwarden)
- ✅ Store in encrypted file
- ✅ Write on paper, lock in safe
- ✅ Create backup in different location

**DON'T**:
- ❌ Save in plain text file on desktop
- ❌ Email to yourself
- ❌ Store in cloud without encryption
- ❌ Write in notebook that could be lost

### If Using Password-Derived Key

**DO**:
- ✅ Use a strong, memorable password
- ✅ Write password hint (not password itself)
- ✅ Use a password pattern you can remember

**DON'T**:
- ❌ Use same password as other accounts
- ❌ Use simple dictionary words
- ❌ Share password with others
- ❌ Write password in plain sight

---

## FAQs

### Q: Can I change my key later?
**A**: Yes, but files encrypted with old key can only be decrypted with that same key. You'll need to decrypt with old key and re-encrypt with new key.

### Q: What if I lose my generated key?
**A**: Cannot recover. Files encrypted with that key are permanently locked. This is why backups are crucial.

### Q: What if I forget my password (for derived key)?
**A**: Cannot recover. You must remember your password exactly to regenerate the same key.

### Q: Can I use the same password for YPG and XPG?
**A**: You can, but they use different encryption methods (DES vs AES), so they're independent.

### Q: Is password derivation secure enough?
**A**: Yes, if you use a strong password. The 100,000 iterations make brute-force attacks impractical.

### Q: Can I see my key after entering a password?
**A**: Yes! After derivation, the full Base64 key appears in the textbox. You can copy it.

### Q: Do I need to derive the key every time?
**A**: No. You can:
- Option A: Derive once, copy key, paste it in future
- Option B: Derive each time from same password (slower but more secure - key never stored)

---

## Quick Reference

### UI Buttons in MainForm

**Passwords / Keys Section:**
```
┌─────────────────────────────────┐
│ YPG Password (DES Key):         │
│ [Man@QueY________________]      │
│                                  │
│ XPG Password (AES-256 Key):     │
│ [________________________]      │
│ [Generate Random] [From Password...]│
└─────────────────────────────────┘
```

**Button Actions:**
- **Generate Random**: Creates new random AES-256 key
- **From Password...**: Opens dialog to derive key from your password

---

## Migration Scenario

**You have files encrypted with different keys and want to standardize:**

1. **Document current keys**:
   - Old key 1: (from generated random)
   - Old key 2: (from different password)

2. **Choose new standard**:
   - Option A: New generated random key
   - Option B: New password-derived key

3. **Decrypt all files** with their respective old keys to JPG

4. **Re-encrypt all files** with your new chosen key

5. **Verify** some files decrypt correctly

6. **Delete** old encrypted files (keep JPG backups temporarily)

---

## Summary

✅ **Three flexible options** - choose what works for you  
✅ **Type own** - for advanced users with existing keys  
✅ **Generate** - for maximum security  
✅ **From Password** - for memorability and convenience  
✅ **All methods** produce valid AES-256 keys  
✅ **You decide** what fits your workflow best  

**The application is flexible - use it however you want!** 🎉

---

**Last Updated**: 2026-07-27  
**Feature**: KeyHelper class with password derivation using PBKDF2
