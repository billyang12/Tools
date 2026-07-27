
using System;
using System.IO;
using System.Drawing;

namespace YXBPictureViewer
{
    /// <summary>
    /// Automated test suite for the encryption system
    ///
    /// This class provides comprehensive testing of both old (DES) and new (AES) encryption methods
    /// to ensure everything works correctly before using in production.
    ///
    /// Test Coverage:
    /// 1. AES key generation validation
    /// 2. AES encryption/decryption round-trip
    /// 3. DES encryption/decryption (backward compatibility)
    /// 4. File format conversion (.ypg to .xpg)
    ///
    /// Usage:
    /// // Run complete test suite
    /// EncryptionTest.RunFullTest();
    ///
    /// // Or quick validation
    /// EncryptionTest.QuickTest();
    /// </summary>
    public class EncryptionTest
    {
        /// <summary>
        /// Runs all encryption tests and reports results
        /// </summary>
        /// <remarks>
        /// This is the main test method you should run to verify the encryption system.
        /// It runs 4 different tests and reports pass/fail for each one.
        ///
        /// All tests should pass before deploying to production.
        /// If any test fails, check the error messages for details.
        /// </remarks>
        public static void RunFullTest()
        {
            Console.WriteLine("=== Encryption System Test ===\n");

            bool allPassed = true;

            // Run each test and track results
            allPassed &= TestKeyGeneration();
            allPassed &= TestAESEncryptionDecryption();
            allPassed &= TestDESEncryptionDecryption();
            allPassed &= TestFileConversion();

            // Display final results
            Console.WriteLine("\n=== Test Summary ===");
            if (allPassed)
            {
                Console.WriteLine("✓ All tests PASSED!");
            }
            else
            {
                Console.WriteLine("✗ Some tests FAILED. Check the output above.");
            }
        }

        /// <summary>
        /// Test 1: Validates AES-256 key generation
        /// </summary>
        /// <returns>True if test passes, false otherwise</returns>
        /// <remarks>
        /// Verifies that:
        /// - Key is generated without errors
        /// - Key is not empty
        /// - Key is proper Base64 format
        /// - Key decodes to exactly 32 bytes (256 bits)
        /// </remarks>
        private static bool TestKeyGeneration()
        {
            Console.WriteLine("[Test 1] AES Key Generation");
            try
            {
                // Generate a test key
                string key = YAESEncrypt.GenerateKey();

                // Validate key is not empty
                if (string.IsNullOrEmpty(key))
                {
                    Console.WriteLine("  ✗ FAIL: Generated key is empty");
                    return false;
                }

                // Validate key length (Base64 encoding of 32 bytes is ~44 characters)
                if (key.Length < 40)
                {
                    Console.WriteLine("  ✗ FAIL: Generated key is too short");
                    return false;
                }

                // Decode and validate key size
                byte[] keyBytes = Convert.FromBase64String(key);
                if (keyBytes.Length != 32) // 256 bits = 32 bytes
                {
                    Console.WriteLine("  ✗ FAIL: Key is not 256 bits (32 bytes)");
                    return false;
                }

                Console.WriteLine($"  ✓ PASS: Generated valid 256-bit key");
                Console.WriteLine($"    Key: {key}");
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"  ✗ FAIL: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Test 2: Validates AES encryption and decryption work correctly
        /// </summary>
        /// <returns>True if test passes, false otherwise</returns>
        /// <remarks>
        /// Verifies that:
        /// - Data can be encrypted without errors
        /// - Encrypted data is larger than original (overhead for nonce, tag, length)
        /// - Data can be decrypted back to original
        /// - Decrypted data matches original byte-for-byte
        /// </remarks>
        private static bool TestAESEncryptionDecryption()
        {
            Console.WriteLine("\n[Test 2] AES Encryption/Decryption");
            try
            {
                // Generate test key and data
                string aesKey = YAESEncrypt.GenerateKey();
                byte[] originalData = new byte[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16 };

                // Encrypt the data
                byte[] encrypted = YAESEncrypt.EncryptBufferRecordLength(originalData, aesKey);

                if (encrypted == null || encrypted.Length == 0)
                {
                    Console.WriteLine("  ✗ FAIL: Encryption returned null or empty");
                    return false;
                }

                if (encrypted.Length <= originalData.Length)
                {
                    Console.WriteLine("  ✗ FAIL: Encrypted data is not larger than original (missing overhead)");
                    return false;
                }

                byte[] decrypted = YAESEncrypt.DecryptBufferRecordedLength(encrypted, aesKey);

                if (decrypted == null || decrypted.Length == 0)
                {
                    Console.WriteLine("  ✗ FAIL: Decryption returned null or empty");
                    return false;
                }

                if (decrypted.Length != originalData.Length)
                {
                    Console.WriteLine($"  ✗ FAIL: Length mismatch (original: {originalData.Length}, decrypted: {decrypted.Length})");
                    return false;
                }

                for (int i = 0; i < originalData.Length; i++)
                {
                    if (originalData[i] != decrypted[i])
                    {
                        Console.WriteLine($"  ✗ FAIL: Data mismatch at position {i}");
                        return false;
                    }
                }

                Console.WriteLine("  ✓ PASS: AES encryption and decryption working correctly");
                Console.WriteLine($"    Original size: {originalData.Length} bytes");
                Console.WriteLine($"    Encrypted size: {encrypted.Length} bytes (overhead: {encrypted.Length - originalData.Length} bytes)");
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"  ✗ FAIL: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Test 3: Validates DES encryption/decryption still works (backward compatibility)
        /// </summary>
        /// <returns>True if test passes, false otherwise</returns>
        /// <remarks>
        /// This test ensures the old DES encryption (for .ypg files) still works correctly.
        /// This is important for backward compatibility - existing .ypg files must continue to work.
        ///
        /// Verifies that:
        /// - Old YEncrypt class still encrypts properly
        /// - Old YEncrypt class still decrypts properly
        /// - Round-trip (encrypt -> decrypt) returns original data
        /// </remarks>
        private static bool TestDESEncryptionDecryption()
        {
            Console.WriteLine("\n[Test 3] DES Encryption/Decryption (Legacy)");
            try
            {
                // Use the standard DES key
                string desKey = "Man@QueY";
                byte[] originalData = new byte[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16 };

                // Test old DES encryption
                byte[] encrypted = YEncrypt.EncryptBufferRecordLength(originalData, desKey);

                if (encrypted == null || encrypted.Length == 0)
                {
                    Console.WriteLine("  ✗ FAIL: DES encryption returned null or empty");
                    return false;
                }

                // Test old DES decryption
                byte[] decrypted = YEncrypt.DecryptBufferRecordedLength(encrypted, desKey);

                if (decrypted == null || decrypted.Length == 0)
                {
                    Console.WriteLine("  ✗ FAIL: DES decryption returned null or empty");
                    return false;
                }

                // Verify length matches
                if (decrypted.Length != originalData.Length)
                {
                    Console.WriteLine($"  ✗ FAIL: Length mismatch (original: {originalData.Length}, decrypted: {decrypted.Length})");
                    return false;
                }

                // Verify data matches byte-for-byte
                for (int i = 0; i < originalData.Length; i++)
                {
                    if (originalData[i] != decrypted[i])
                    {
                        Console.WriteLine($"  ✗ FAIL: Data mismatch at position {i}");
                        return false;
                    }
                }

                Console.WriteLine("  ✓ PASS: DES encryption and decryption working correctly (legacy support)");
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"  ✗ FAIL: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Test 4: Validates file conversion from .ypg to .xpg format
        /// </summary>
        /// <returns>True if test passes, false otherwise</returns>
        /// <remarks>
        /// This test validates the complete conversion workflow:
        /// 1. Create a test .ypg file with DES encryption
        /// 2. Convert it to .xpg format (DES -> AES conversion)
        /// 3. Verify the .xpg file was created
        /// 4. Decrypt the .xpg file
        /// 5. Verify data matches the original
        ///
        /// Uses a temporary directory that is cleaned up after the test.
        /// </remarks>
        private static bool TestFileConversion()
        {
            Console.WriteLine("\n[Test 4] File Format Conversion");

            // Create a unique temporary directory for testing
            string testDir = Path.Combine(Path.GetTempPath(), "YXBTest_" + Guid.NewGuid().ToString().Substring(0, 8));

            try
            {
                // Create test directory
                Directory.CreateDirectory(testDir);

                // Set up test file paths
                string testYpgFile = Path.Combine(testDir, "test.ypg");
                string testXpgFile = Path.Combine(testDir, "test.xpg");

                // Create test data (pattern: 0, 1, 2, ..., 99)
                byte[] testImageData = new byte[100];
                for (int i = 0; i < testImageData.Length; i++)
                {
                    testImageData[i] = (byte)(i % 256);
                }

                // Step 1: Create a .ypg file using old DES encryption
                string desKey = "Man@QueY";
                byte[] encryptedYpg = YEncrypt.EncryptBufferRecordLength(testImageData, desKey);
                File.WriteAllBytes(testYpgFile, encryptedYpg);

                // Step 2: Generate AES key for new encryption
                string aesKey = YAESEncrypt.GenerateKey();

                // Step 3: Convert .ypg to .xpg
                bool conversionResult = FileConverter.ConvertYPGtoXPG(testYpgFile, testXpgFile, desKey, aesKey);

                if (!conversionResult)
                {
                    Console.WriteLine("  ✗ FAIL: Conversion returned false");
                    return false;
                }

                // Step 4: Verify .xpg file was created
                if (!File.Exists(testXpgFile))
                {
                    Console.WriteLine("  ✗ FAIL: Output .xpg file was not created");
                    return false;
                }

                // Step 5: Decrypt the .xpg file
                byte[] decryptedXpg = YAESEncrypt.DecryptFileToBuffer(testXpgFile, aesKey);

                // Step 6: Verify length matches
                if (decryptedXpg == null || decryptedXpg.Length != testImageData.Length)
                {
                    Console.WriteLine($"  ✗ FAIL: Decrypted data length mismatch");
                    return false;
                }

                // Step 7: Verify data matches byte-for-byte
                for (int i = 0; i < testImageData.Length; i++)
                {
                    if (testImageData[i] != decryptedXpg[i])
                    {
                        Console.WriteLine($"  ✗ FAIL: Data mismatch after conversion at position {i}");
                        return false;
                    }
                }

                // Test passed - show file sizes for information
                Console.WriteLine("  ✓ PASS: YPG to XPG conversion working correctly");
                Console.WriteLine($"    Original data: {testImageData.Length} bytes");
                Console.WriteLine($"    YPG file size: {new FileInfo(testYpgFile).Length} bytes");
                Console.WriteLine($"    XPG file size: {new FileInfo(testXpgFile).Length} bytes");

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"  ✗ FAIL: {ex.Message}");
                Console.WriteLine($"    Stack: {ex.StackTrace}");
                return false;
            }
            finally
            {
                // Clean up temporary test directory
                try
                {
                    if (Directory.Exists(testDir))
                    {
                        Directory.Delete(testDir, true); // Delete directory and all contents
                    }
                }
                catch
                {
                    // Ignore cleanup errors
                }
            }
        }

        /// <summary>
        /// Quick validation test - encrypts and decrypts a simple text message
        /// </summary>
        /// <remarks>
        /// This is a simple, fast test to verify AES encryption is working.
        /// Good for quick validation during development.
        ///
        /// For comprehensive testing, use RunFullTest() instead.
        ///
        /// Example usage:
        /// EncryptionTest.QuickTest();
        /// </remarks>
        public static void QuickTest()
        {
            Console.WriteLine("=== Quick Encryption Test ===\n");

            // Generate a test key
            string aesKey = YAESEncrypt.GenerateKey();
            Console.WriteLine($"Generated AES Key: {aesKey}\n");

            // Create test data from a text string
            byte[] testData = System.Text.Encoding.UTF8.GetBytes("Hello, World! This is a test.");
            Console.WriteLine($"Original text: {System.Text.Encoding.UTF8.GetString(testData)}");

            // Encrypt the data
            byte[] encrypted = YAESEncrypt.EncryptBufferRecordLength(testData, aesKey);
            Console.WriteLine($"Encrypted: {encrypted.Length} bytes");

            // Decrypt it back
            byte[] decrypted = YAESEncrypt.DecryptBufferRecordedLength(encrypted, aesKey);
            Console.WriteLine($"Decrypted text: {System.Text.Encoding.UTF8.GetString(decrypted)}");

            Console.WriteLine("\n✓ Test completed successfully!");
        }
    }
}
