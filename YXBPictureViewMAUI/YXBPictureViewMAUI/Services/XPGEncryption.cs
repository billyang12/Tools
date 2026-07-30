using System;
using System.IO;
using System.Security.Cryptography;

namespace YXBPictureViewMAUI.Services
{
    /// <summary>
    /// AES-256-GCM encryption service for XPG files
    /// Compatible with desktop YXBPictureViewer app
    /// </summary>
    public class XPGEncryption
    {
        private const int KeySize = 256;
        private const int NonceSize = 12;
        private const int TagSize = 16;

        /// <summary>
        /// Generate new random AES-256 key
        /// </summary>
        public static string GenerateKey()
        {
            using (var aes = Aes.Create())
            {
                aes.KeySize = KeySize;
                aes.GenerateKey();
                return Convert.ToBase64String(aes.Key);
            }
        }

        /// <summary>
        /// Derive key from password using PBKDF2
        /// </summary>
        public static string DeriveKeyFromPassword(string password)
        {
            if (string.IsNullOrEmpty(password))
                throw new ArgumentException("Password cannot be empty");

            byte[] salt = System.Text.Encoding.UTF8.GetBytes("YXBPictureViewer-Salt-2026");

            using (var pbkdf2 = new Rfc2898DeriveBytes(password, salt, 100000, HashAlgorithmName.SHA256))
            {
                byte[] keyBytes = pbkdf2.GetBytes(32);
                return Convert.ToBase64String(keyBytes);
            }
        }

        /// <summary>
        /// Encrypt image file to XPG format
        /// </summary>
        public static async Task<bool> EncryptFileAsync(string inputPath, string outputPath, string key)
        {
            try
            {
                byte[] fileData = await File.ReadAllBytesAsync(inputPath);
                byte[] encrypted = EncryptBuffer(fileData, key);
                await File.WriteAllBytesAsync(outputPath, encrypted);
                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Encryption error: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Decrypt XPG file to image bytes
        /// </summary>
        public static async Task<byte[]?> DecryptFileAsync(string inputPath, string key)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"Reading encrypted file...");
                byte[]? encryptedData = await FileSystemHelper.ReadAllBytesAsync(inputPath);
                if (encryptedData == null)
                {
                    System.Diagnostics.Debug.WriteLine($"Failed to read file - ReadAllBytesAsync returned null");
                    return null;
                }

                System.Diagnostics.Debug.WriteLine($"Read {encryptedData.Length} bytes, attempting decryption...");
                byte[] decrypted = DecryptBuffer(encryptedData, key);
                System.Diagnostics.Debug.WriteLine($"Decryption successful, result size: {decrypted.Length} bytes");
                return decrypted;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Decryption error: {ex.GetType().Name} - {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"Stack trace: {ex.StackTrace}");
                return null;
            }
        }

        /// <summary>
        /// Encrypt buffer with AES-256-GCM
        /// Format: [4 bytes length][12 bytes nonce][16 bytes tag][encrypted data]
        /// </summary>
        private static byte[] EncryptBuffer(byte[] buffer, string sKey)
        {
            if (buffer == null || buffer.Length == 0)
                throw new ArgumentException("Buffer cannot be null or empty");

            Int32 originlen = buffer.Length;
            byte[] key = Convert.FromBase64String(sKey);

            byte[] nonce = new byte[NonceSize];
            byte[] tag = new byte[TagSize];
            byte[] ciphertext = new byte[buffer.Length];

            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(nonce);
            }

            using (var aesGcm = new AesGcm(key, TagSize))
            {
                aesGcm.Encrypt(nonce, buffer, ciphertext, tag);
            }

            byte[] lengthBytes = BitConverter.GetBytes(originlen);
            byte[] result = new byte[lengthBytes.Length + nonce.Length + tag.Length + ciphertext.Length];

            int offset = 0;
            Array.Copy(lengthBytes, 0, result, offset, lengthBytes.Length);
            offset += lengthBytes.Length;
            Array.Copy(nonce, 0, result, offset, nonce.Length);
            offset += nonce.Length;
            Array.Copy(tag, 0, result, offset, tag.Length);
            offset += tag.Length;
            Array.Copy(ciphertext, 0, result, offset, ciphertext.Length);

            return result;
        }

        /// <summary>
        /// Decrypt buffer with AES-256-GCM
        /// </summary>
        private static byte[] DecryptBuffer(byte[] buffer, string sKey)
        {
            if (buffer == null || buffer.Length == 0)
                throw new ArgumentException("Buffer cannot be null or empty");

            byte[] key = Convert.FromBase64String(sKey);
            int offset = 0;

            byte[] lengthBytes = new byte[sizeof(Int32)];
            Array.Copy(buffer, offset, lengthBytes, 0, lengthBytes.Length);
            int originalLength = BitConverter.ToInt32(lengthBytes, 0);
            offset += lengthBytes.Length;

            byte[] nonce = new byte[NonceSize];
            Array.Copy(buffer, offset, nonce, 0, nonce.Length);
            offset += nonce.Length;

            byte[] tag = new byte[TagSize];
            Array.Copy(buffer, offset, tag, 0, tag.Length);
            offset += tag.Length;

            byte[] ciphertext = new byte[buffer.Length - offset];
            Array.Copy(buffer, offset, ciphertext, 0, ciphertext.Length);

            byte[] plaintext = new byte[originalLength];

            using (var aesGcm = new AesGcm(key, TagSize))
            {
                try
                {
                    aesGcm.Decrypt(nonce, ciphertext, tag, plaintext);
                }
                catch (CryptographicException ex)
                {
                    throw new CryptographicException("Decryption failed. Check your password.", ex);
                }
            }

            return plaintext;
        }

        /// <summary>
        /// Validate if string is valid Base64 AES-256 key
        /// </summary>
        public static bool IsValidKey(string key)
        {
            if (string.IsNullOrWhiteSpace(key))
                return false;

            try
            {
                byte[] keyBytes = Convert.FromBase64String(key);
                return keyBytes.Length == 32;
            }
            catch
            {
                return false;
            }
        }
    }
}
