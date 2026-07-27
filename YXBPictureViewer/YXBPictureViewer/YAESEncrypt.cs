
using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace YXBPictureViewer
{
    /// <summary>
    /// Modern AES-256-GCM encryption class for .xpg files
    ///
    /// This class provides strong, authenticated encryption using AES-256-GCM algorithm.
    /// It's a replacement for the legacy DES encryption (YEncrypt class) used in .ypg files.
    ///
    /// Key Features:
    /// - AES-256 encryption (256-bit key strength)
    /// - GCM (Galois/Counter Mode) for authenticated encryption
    /// - Random nonce per encryption for security
    /// - Authentication tag to detect tampering
    /// - Compatible with .NET Core/.NET 5+ and .NET Framework 4.7.2+
    ///
    /// File Format (.xpg):
    /// [4 bytes: original length][12 bytes: nonce][16 bytes: auth tag][encrypted data]
    /// </summary>
    public class YAESEncrypt
    {
        // AES-256 uses 256-bit (32 byte) keys for strong encryption
        private const int KeySize = 256;

        // GCM nonce size: 12 bytes (96 bits) is the recommended size for AES-GCM
        private const int NonceSize = 12;

        // GCM authentication tag size: 16 bytes (128 bits) for data integrity verification
        private const int TagSize = 16;

        /// <summary>
        /// Generates a new random AES-256 key
        /// </summary>
        /// <returns>Base64-encoded 256-bit key (44 characters)</returns>
        /// <remarks>
        /// Example usage:
        /// string newKey = YAESEncrypt.GenerateKey();
        /// // Save this key securely - you'll need it to decrypt your files!
        /// </remarks>
        public static string GenerateKey()
        {
            using (var aes = Aes.Create())
            {
                aes.KeySize = KeySize; // Set to 256 bits
                aes.GenerateKey();     // Generate random key
                return Convert.ToBase64String(aes.Key); // Return as Base64 string for easy storage
            }
        }

        /// <summary>
        /// Encrypts a buffer and prepends the original length for later verification
        /// </summary>
        /// <param name="buffer">Data to encrypt (e.g., image file bytes)</param>
        /// <param name="sKey">Base64-encoded AES-256 key</param>
        /// <returns>Encrypted buffer with format: [length][nonce][tag][ciphertext]</returns>
        /// <remarks>
        /// This is the main encryption method used for creating .xpg files.
        /// The original length is stored so we can verify successful decryption.
        /// Each encryption uses a unique random nonce for security.
        /// </remarks>
        public static byte[] EncryptBufferRecordLength(byte[] buffer, string sKey)
        {
            // Validate input
            if (buffer == null) return null;
            if (buffer.Length <= 0) return null;

            // Store original length (used during decryption to verify success)
            Int32 originlen = buffer.Length;

            // Convert Base64 key to bytes
            byte[] key = Convert.FromBase64String(sKey);

            // Prepare encryption components
            byte[] nonce = new byte[NonceSize];      // Random number used once per encryption
            byte[] tag = new byte[TagSize];          // Authentication tag to detect tampering
            byte[] ciphertext = new byte[buffer.Length]; // Will hold encrypted data

            // Generate random nonce - this ensures each encryption is unique even with same data
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(nonce);
            }

            // Perform AES-GCM encryption
            using (var aesGcm = new AesGcm(key, TagSize))
            {
                // Encrypt: plaintext -> ciphertext, and generate authentication tag
                aesGcm.Encrypt(nonce, buffer, ciphertext, tag);
            }

            // Build result: [4 bytes length][12 bytes nonce][16 bytes tag][encrypted data]
            byte[] lengthBytes = BitConverter.GetBytes(originlen);
            byte[] result = new byte[lengthBytes.Length + nonce.Length + tag.Length + ciphertext.Length];

            // Copy all components into result buffer
            int offset = 0;
            Array.Copy(lengthBytes, 0, result, offset, lengthBytes.Length); // Original length (4 bytes)
            offset += lengthBytes.Length;
            Array.Copy(nonce, 0, result, offset, nonce.Length);              // Nonce (12 bytes)
            offset += nonce.Length;
            Array.Copy(tag, 0, result, offset, tag.Length);                  // Auth tag (16 bytes)
            offset += tag.Length;
            Array.Copy(ciphertext, 0, result, offset, ciphertext.Length);    // Encrypted data

            return result;
        }

        /// <summary>
        /// Decrypts a buffer that was encrypted with EncryptBufferRecordLength
        /// </summary>
        /// <param name="buffer">Encrypted buffer from .xpg file</param>
        /// <param name="sKey">Base64-encoded AES-256 key (must match the encryption key)</param>
        /// <returns>Decrypted original data</returns>
        /// <remarks>
        /// This method reads the file format: [length][nonce][tag][ciphertext]
        /// and decrypts it back to the original data.
        /// The authentication tag is verified automatically - if the data was tampered with,
        /// a CryptographicException will be thrown.
        /// </remarks>
        public static byte[] DecryptBufferRecordedLength(byte[] buffer, string sKey)
        {
            // Validate input
            if (buffer == null) return null;
            if (buffer.Length <= 0) return null;

            // Convert Base64 key to bytes
            byte[] key = Convert.FromBase64String(sKey);
            int offset = 0;

            // Extract original length (first 4 bytes)
            byte[] lengthBytes = new byte[sizeof(Int32)];
            Array.Copy(buffer, offset, lengthBytes, 0, lengthBytes.Length);
            int originalLength = BitConverter.ToInt32(lengthBytes, 0);
            offset += lengthBytes.Length;

            // Debug logging for troubleshooting
            System.Diagnostics.Debug.WriteLine($"DecryptBufferRecordedLength: Stored length={originalLength}, Buffer length={buffer.Length}");

            // Extract nonce (next 12 bytes)
            byte[] nonce = new byte[NonceSize];
            Array.Copy(buffer, offset, nonce, 0, nonce.Length);
            offset += nonce.Length;

            // Extract authentication tag (next 16 bytes)
            byte[] tag = new byte[TagSize];
            Array.Copy(buffer, offset, tag, 0, tag.Length);
            offset += tag.Length;

            // Extract encrypted data (remaining bytes)
            byte[] ciphertext = new byte[buffer.Length - offset];
            Array.Copy(buffer, offset, ciphertext, 0, ciphertext.Length);

            // Prepare buffer for decrypted data
            byte[] plaintext = new byte[originalLength];

            // Perform AES-GCM decryption with authentication
            using (var aesGcm = new AesGcm(key, TagSize))
            {
                try
                {
                    // Decrypt: ciphertext -> plaintext, and verify authentication tag
                    // If tag doesn't match, this will throw CryptographicException (data was tampered with)
                    aesGcm.Decrypt(nonce, ciphertext, tag, plaintext);
                }
                catch (CryptographicException ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Decryption failed: {ex.Message}");
                    throw new CryptographicException("Failed to decrypt data. The data may be corrupted or the key may be incorrect.", ex);
                }
            }

            System.Diagnostics.Debug.WriteLine($"DecryptBufferRecordedLength: Successfully decrypted {plaintext.Length} bytes");

            return plaintext;
        }

        /// <summary>
        /// Encrypts a buffer without recording the original length
        /// </summary>
        /// <param name="buffer">Data to encrypt</param>
        /// <param name="sKey">Base64-encoded AES-256 key</param>
        /// <returns>Encrypted buffer with format: [nonce][tag][ciphertext]</returns>
        /// <remarks>
        /// Similar to EncryptBufferRecordLength but doesn't store the original length.
        /// Use this if you already know the decrypted size or don't need to verify it.
        /// For .xpg files, use EncryptBufferRecordLength instead.
        /// </remarks>
        public static byte[] EncryptBuffer(byte[] buffer, string sKey)
        {
            if (buffer == null) return null;
            if (buffer.Length <= 0) return null;

            byte[] key = Convert.FromBase64String(sKey);
            byte[] nonce = new byte[NonceSize];
            byte[] tag = new byte[TagSize];
            byte[] ciphertext = new byte[buffer.Length];

            // Generate random nonce
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(nonce);
            }

            // Encrypt the data
            using (var aesGcm = new AesGcm(key, TagSize))
            {
                aesGcm.Encrypt(nonce, buffer, ciphertext, tag);
            }

            // Build result without length prefix
            byte[] result = new byte[nonce.Length + tag.Length + ciphertext.Length];
            int offset = 0;
            Array.Copy(nonce, 0, result, offset, nonce.Length);
            offset += nonce.Length;
            Array.Copy(tag, 0, result, offset, tag.Length);
            offset += tag.Length;
            Array.Copy(ciphertext, 0, result, offset, ciphertext.Length);

            return result;
        }

        /// <summary>
        /// Decrypts a buffer that was encrypted with EncryptBuffer
        /// </summary>
        /// <param name="buffer">Encrypted buffer</param>
        /// <param name="sKey">Base64-encoded AES-256 key</param>
        /// <param name="len">Expected length of decrypted data (must be known in advance)</param>
        /// <returns>Decrypted data</returns>
        /// <remarks>
        /// Requires you to know the original data length in advance.
        /// For .xpg files, use DecryptBufferRecordedLength which reads the stored length.
        /// </remarks>
        public static byte[] DecryptBuffer(byte[] buffer, string sKey, int len)
        {
            if (buffer == null) return null;
            if (buffer.Length <= 0) return null;

            byte[] key = Convert.FromBase64String(sKey);
            int offset = 0;

            // Extract components from buffer
            byte[] nonce = new byte[NonceSize];
            Array.Copy(buffer, offset, nonce, 0, nonce.Length);
            offset += nonce.Length;

            byte[] tag = new byte[TagSize];
            Array.Copy(buffer, offset, tag, 0, tag.Length);
            offset += tag.Length;

            byte[] ciphertext = new byte[buffer.Length - offset];
            Array.Copy(buffer, offset, ciphertext, 0, ciphertext.Length);

            byte[] plaintext = new byte[len];

            // Decrypt and verify authentication tag
            using (var aesGcm = new AesGcm(key, TagSize))
            {
                try
                {
                    aesGcm.Decrypt(nonce, ciphertext, tag, plaintext);
                }
                catch (CryptographicException ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Decryption failed: {ex.Message}");
                    throw new CryptographicException("Failed to decrypt data. The data may be corrupted or the key may be incorrect.", ex);
                }
            }

            return plaintext;
        }

        /// <summary>
        /// Reads a file and encrypts it to a buffer
        /// </summary>
        /// <param name="sInputFilename">Path to file to encrypt (e.g., image.jpg)</param>
        /// <param name="key">Base64-encoded AES-256 key</param>
        /// <returns>Encrypted buffer ready to be saved as .xpg file</returns>
        public static byte[] EncryptFileToBuffer(string sInputFilename, string key)
        {
            using (FileStream fsInput = new FileStream(sInputFilename, FileMode.Open, FileAccess.Read))
            {
                // Read entire file into memory
                byte[] buffer = new byte[fsInput.Length];
                fsInput.Read(buffer, 0, (int)fsInput.Length);

                // Encrypt it
                byte[] buf = EncryptBufferRecordLength(buffer, key);
                return buf;
            }
        }

        /// <summary>
        /// Encrypts a file and saves it (creates .xpg file)
        /// </summary>
        /// <param name="sInputFilename">Source file (e.g., image.jpg)</param>
        /// <param name="sOutputFilename">Destination file (e.g., image.xpg)</param>
        /// <param name="sKey">Base64-encoded AES-256 key</param>
        /// <remarks>
        /// Example: EncryptFile("photo.jpg", "photo.xpg", aesKey);
        /// </remarks>
        public static void EncryptFile(string sInputFilename, string sOutputFilename, string sKey)
        {
            byte[] buff = EncryptFileToBuffer(sInputFilename, sKey);
            WriteBufferToFile(buff, sOutputFilename);
        }

        /// <summary>
        /// Reads and decrypts a .xpg file to a buffer
        /// </summary>
        /// <param name="sInputFilename">Path to .xpg file</param>
        /// <param name="sKey">Base64-encoded AES-256 key (must match encryption key)</param>
        /// <returns>Decrypted original file data</returns>
        /// <remarks>
        /// This is the main method used by the viewer to open .xpg image files
        /// </remarks>
        public static byte[] DecryptFileToBuffer(string sInputFilename, string sKey)
        {
            // Load encrypted file
            byte[] buff = LoadFileToBuffer(sInputFilename);

            // Decrypt it
            byte[] dbuff = DecryptBufferRecordedLength(buff, sKey);
            return dbuff;
        }

        /// <summary>
        /// Decrypts a .xpg file and saves the original file
        /// </summary>
        /// <param name="sInputFilename">Source .xpg file</param>
        /// <param name="sOutputFilename">Destination file (e.g., image.jpg)</param>
        /// <param name="sKey">Base64-encoded AES-256 key</param>
        /// <remarks>
        /// Example: DecryptFile("photo.xpg", "photo.jpg", aesKey);
        /// </remarks>
        public static void DecryptFile(string sInputFilename, string sOutputFilename, string sKey)
        {
            byte[] buff = DecryptFileToBuffer(sInputFilename, sKey);
            WriteBufferToFile(buff, sOutputFilename);
        }

        /// <summary>
        /// Decrypts a .xpg file directly to a MemoryStream for image display
        /// </summary>
        /// <param name="sInputFilename">Path to .xpg file</param>
        /// <param name="sKey">Base64-encoded AES-256 key</param>
        /// <returns>MemoryStream containing decrypted image data</returns>
        /// <remarks>
        /// Used by the picture viewer to display .xpg images without saving to disk.
        /// The MemoryStream can be used directly with Bitmap constructor.
        /// </remarks>
        public static MemoryStream DecryptFileToMemoryStream(string sInputFilename, string sKey)
        {
            byte[] buff = DecryptFileToBuffer(sInputFilename, sKey);
            MemoryStream ms = new MemoryStream(buff);
            return ms;
        }

        /// <summary>
        /// Loads a file into a MemoryStream without decryption
        /// </summary>
        /// <param name="sInputFilename">Path to file</param>
        /// <returns>MemoryStream containing file contents</returns>
        /// <remarks>
        /// Utility method for loading unencrypted files.
        /// For encrypted .xpg files, use DecryptFileToMemoryStream instead.
        /// </remarks>
        public static MemoryStream LoadFileToMemoryStream(string sInputFilename)
        {
            byte[] buffer = LoadFileToBuffer(sInputFilename);
            if (buffer == null || buffer.Length == 0)
            {
                return null;
            }
            System.IO.MemoryStream ms = new System.IO.MemoryStream(buffer);
            ms.Position = 0; // Reset position to start
            return ms;
        }

        /// <summary>
        /// Loads an entire file into a byte array
        /// </summary>
        /// <param name="sInputFilename">Path to file</param>
        /// <returns>Byte array containing file contents</returns>
        /// <remarks>
        /// General-purpose file loading utility
        /// </remarks>
        public static byte[] LoadFileToBuffer(string sInputFilename)
        {
            using (FileStream fsread = new FileStream(sInputFilename,
               FileMode.Open,
               FileAccess.Read))
            {
                byte[] buffer = new byte[fsread.Length];
                fsread.Read(buffer, 0, (int)fsread.Length);
                return buffer;
            }
        }

        /// <summary>
        /// Writes a byte array to a file
        /// </summary>
        /// <param name="buffer">Data to write</param>
        /// <param name="sOutputFilename">Destination file path</param>
        /// <returns>True if successful, false on error</returns>
        /// <remarks>
        /// General-purpose file writing utility with error handling
        /// </remarks>
        public static bool WriteBufferToFile(byte[] buffer, string sOutputFilename)
        {
            try
            {
                using (FileStream fswrite = new FileStream(sOutputFilename, FileMode.OpenOrCreate, FileAccess.Write))
                {
                    fswrite.Write(buffer, 0, buffer.Length);
                    fswrite.Flush(); // Ensure data is written to disk
                }
                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error writing buffer to file: {ex.Message}");
                return false;
            }
        }
    }
}
