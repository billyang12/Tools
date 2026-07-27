using System;
using System.Security.Cryptography;
using System.Text;

namespace YXBPictureViewer
{
    /// <summary>
    /// Helper class for creating and validating AES encryption keys
    /// </summary>
    public static class KeyHelper
    {
        /// <summary>
        /// Converts a password/passphrase into a valid AES-256 key
        /// </summary>
        /// <param name="password">Your password or passphrase (any length)</param>
        /// <returns>Valid Base64-encoded AES-256 key</returns>
        /// <remarks>
        /// This uses PBKDF2 (Password-Based Key Derivation Function 2) to derive
        /// a secure 256-bit key from your password.
        ///
        /// Example:
        /// string myKey = KeyHelper.DeriveKeyFromPassword("MySecretPassword123!");
        /// </remarks>
        public static string DeriveKeyFromPassword(string password)
        {
            if (string.IsNullOrEmpty(password))
            {
                throw new ArgumentException("Password cannot be empty");
            }

            // Use a fixed salt for consistency (same password = same key)
            // In production, you might want to use a different salt or make it configurable
            byte[] salt = Encoding.UTF8.GetBytes("YXBPictureViewer-Salt-2026");

            // Derive a 32-byte (256-bit) key from the password
            using (var pbkdf2 = new Rfc2898DeriveBytes(password, salt, 100000, HashAlgorithmName.SHA256))
            {
                byte[] keyBytes = pbkdf2.GetBytes(32); // 32 bytes = 256 bits
                return Convert.ToBase64String(keyBytes);
            }
        }

        /// <summary>
        /// Validates if a string is a valid AES-256 key
        /// </summary>
        /// <param name="key">Key to validate</param>
        /// <returns>True if valid, false otherwise</returns>
        public static bool IsValidAESKey(string key)
        {
            if (string.IsNullOrWhiteSpace(key))
                return false;

            try
            {
                // Try to decode from Base64
                byte[] keyBytes = Convert.FromBase64String(key);

                // Check if it's exactly 32 bytes (256 bits)
                return keyBytes.Length == 32;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Gets a user-friendly description of why a key is invalid
        /// </summary>
        public static string GetKeyValidationMessage(string key)
        {
            if (string.IsNullOrWhiteSpace(key))
                return "Key is empty";

            try
            {
                byte[] keyBytes = Convert.FromBase64String(key);

                if (keyBytes.Length != 32)
                {
                    return $"Key must be 256 bits (32 bytes). Your key is {keyBytes.Length * 8} bits ({keyBytes.Length} bytes).";
                }

                return "Key is valid";
            }
            catch (FormatException)
            {
                return "Key is not valid Base64 format. Must contain only: A-Z, a-z, 0-9, +, /, =";
            }
            catch (Exception ex)
            {
                return $"Key validation error: {ex.Message}";
            }
        }

        /// <summary>
        /// Generates a random AES-256 key
        /// </summary>
        /// <returns>Base64-encoded random 256-bit key</returns>
        public static string GenerateRandomKey()
        {
            return YAESEncrypt.GenerateKey();
        }
    }
}
