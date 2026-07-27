
using System;
using System.IO;

namespace YXBPictureViewer
{
    /// <summary>
    /// Utility class for converting encrypted files between formats
    ///
    /// This class helps migrate from old DES-encrypted .ypg files to new AES-256 encrypted .xpg files.
    /// It decrypts using the old method and re-encrypts using the new method, preserving the original image data.
    ///
    /// Typical usage:
    /// 1. Generate new AES key
    /// 2. Convert files from .ypg to .xpg
    /// 3. Verify conversions work
    /// 4. Optionally delete old .ypg files
    /// </summary>
    public class FileConverter
    {
        /// <summary>
        /// Generates a new AES-256 key for encrypting .xpg files
        /// </summary>
        /// <returns>Base64-encoded 256-bit AES key</returns>
        /// <remarks>
        /// This is a convenience wrapper around YAESEncrypt.GenerateKey()
        /// Save the generated key securely - you'll need it to decrypt your files!
        /// </remarks>
        public static string GenerateNewAESKey()
        {
            return YAESEncrypt.GenerateKey();
        }

        /// <summary>
        /// Converts a single .ypg file (DES) to .xpg file (AES-256)
        /// </summary>
        /// <param name="inputYpgFile">Path to source .ypg file</param>
        /// <param name="outputXpgFile">Path to destination .xpg file</param>
        /// <param name="desKey">DES key for decrypting .ypg file (usually "Man@QueY")</param>
        /// <param name="aesKey">AES-256 key for encrypting .xpg file (Base64-encoded)</param>
        /// <returns>True if conversion successful, false otherwise</returns>
        /// <remarks>
        /// Process:
        /// 1. Decrypt .ypg file using DES (old method)
        /// 2. Re-encrypt data using AES-256 (new method)
        /// 3. Save as .xpg file
        ///
        /// Example:
        /// bool success = ConvertYPGtoXPG("photo.ypg", "photo.xpg", "Man@QueY", generatedAesKey);
        /// </remarks>
        public static bool ConvertYPGtoXPG(string inputYpgFile, string outputXpgFile, string desKey, string aesKey)
        {
            try
            {
                // Step 1: Decrypt the old .ypg file using DES
                byte[] decryptedData = YEncrypt.DecryptFileToBuffer(inputYpgFile, desKey);

                if (decryptedData == null || decryptedData.Length == 0)
                {
                    Console.WriteLine($"Failed to decrypt {inputYpgFile}");
                    return false;
                }

                // Step 2: Re-encrypt using AES-256
                byte[] encryptedData = YAESEncrypt.EncryptBufferRecordLength(decryptedData, aesKey);

                if (encryptedData == null || encryptedData.Length == 0)
                {
                    Console.WriteLine($"Failed to encrypt data for {outputXpgFile}");
                    return false;
                }

                // Step 3: Write to .xpg file
                bool success = YAESEncrypt.WriteBufferToFile(encryptedData, outputXpgFile);

                if (success)
                {
                    Console.WriteLine($"Successfully converted: {Path.GetFileName(inputYpgFile)} -> {Path.GetFileName(outputXpgFile)}");
                }

                return success;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error converting {inputYpgFile}: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Batch converts all .ypg files in a directory to .xpg format
        /// </summary>
        /// <param name="directory">Directory containing .ypg files</param>
        /// <param name="desKey">DES key for decrypting .ypg files</param>
        /// <param name="aesKey">AES-256 key for encrypting .xpg files</param>
        /// <param name="includeSubdirectories">If true, recursively processes subdirectories</param>
        /// <param name="deleteOriginal">If true, deletes .ypg files after successful conversion (USE WITH CAUTION!)</param>
        /// <returns>Number of files successfully converted</returns>
        /// <remarks>
        /// IMPORTANT: Test with deleteOriginal=false first to verify conversions work!
        ///
        /// Safety recommendations:
        /// 1. Back up your files before batch conversion
        /// 2. Run once with deleteOriginal=false to test
        /// 3. Verify some .xpg files open correctly
        /// 4. Only then run with deleteOriginal=true if desired
        ///
        /// Example:
        /// int count = ConvertAllYPGInDirectory(
        ///     @"C:\Pictures",
        ///     "Man@QueY",
        ///     generatedAesKey,
        ///     includeSubdirectories: true,
        ///     deleteOriginal: false  // Keep originals as backup
        /// );
        /// Console.WriteLine($"Converted {count} files");
        /// </remarks>
        public static int ConvertAllYPGInDirectory(string directory, string desKey, string aesKey, bool includeSubdirectories = false, bool deleteOriginal = false)
        {
            int convertedCount = 0;

            try
            {
                // Find all .ypg files in current directory
                string[] ypgFiles = Directory.GetFiles(directory, "*.ypg");

                // Convert each file
                foreach (string ypgFile in ypgFiles)
                {
                    // Build output filename: change extension from .ypg to .xpg
                    string xpgFile = Path.Combine(
                        Path.GetDirectoryName(ypgFile),
                        Path.GetFileNameWithoutExtension(ypgFile) + ".xpg"
                    );

                    // Attempt conversion
                    if (ConvertYPGtoXPG(ypgFile, xpgFile, desKey, aesKey))
                    {
                        convertedCount++;

                        // Delete original if requested (and conversion succeeded)
                        if (deleteOriginal)
                        {
                            try
                            {
                                File.Delete(ypgFile);
                                Console.WriteLine($"Deleted original file: {Path.GetFileName(ypgFile)}");
                            }
                            catch (Exception ex)
                            {
                                // Log but don't fail - conversion still succeeded
                                Console.WriteLine($"Warning: Could not delete {ypgFile}: {ex.Message}");
                            }
                        }
                    }
                }

                // Recursively process subdirectories if requested
                if (includeSubdirectories)
                {
                    string[] subdirectories = Directory.GetDirectories(directory);
                    foreach (string subdirectory in subdirectories)
                    {
                        convertedCount += ConvertAllYPGInDirectory(subdirectory, desKey, aesKey, includeSubdirectories, deleteOriginal);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error processing directory {directory}: {ex.Message}");
            }

            return convertedCount;
        }
    }
}
