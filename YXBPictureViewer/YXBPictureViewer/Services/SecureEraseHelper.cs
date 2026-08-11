using System;
using System.IO;

namespace YXBPictureViewer.Services
{
    /// <summary>
    /// Helper for securely erasing temporary files to prevent data recovery
    /// Uses military-grade overwrite patterns (DoD, Gutmann)
    /// </summary>
    public static class SecureEraseHelper
    {
        private const int BufferSize = 65536; // 64KB buffer for overwriting
        private static readonly Random _random = new Random();

        // Gutmann method patterns (simplified - using key patterns)
        private static readonly byte[][] GutmannPatterns = new byte[][]
        {
            new byte[] { 0x55 }, new byte[] { 0xAA }, new byte[] { 0x92, 0x49, 0x24 },
            new byte[] { 0x49, 0x24, 0x92 }, new byte[] { 0x24, 0x92, 0x49 },
            new byte[] { 0x00 }, new byte[] { 0x11 }, new byte[] { 0x22 },
            new byte[] { 0x33 }, new byte[] { 0x44 }, new byte[] { 0x55 },
            new byte[] { 0x66 }, new byte[] { 0x77 }, new byte[] { 0x88 },
            new byte[] { 0x99 }, new byte[] { 0xAA }, new byte[] { 0xBB },
            new byte[] { 0xCC }, new byte[] { 0xDD }, new byte[] { 0xEE },
            new byte[] { 0xFF }, new byte[] { 0x92, 0x49, 0x24 },
            new byte[] { 0x49, 0x24, 0x92 }, new byte[] { 0x24, 0x92, 0x49 },
            new byte[] { 0x6D, 0xB6, 0xDB }, new byte[] { 0xB6, 0xDB, 0x6D },
            new byte[] { 0xDB, 0x6D, 0xB6 }
        };

        /// <summary>
        /// Securely erases a file by overwriting it with patterns before deletion
        /// </summary>
        public static bool SecureEraseFile(string filePath, EraseMethod method = EraseMethod.Quick)
        {
            try
            {
                if (!File.Exists(filePath))
                {
                    System.Diagnostics.Debug.WriteLine($"SecureEraseHelper: File doesn't exist: {filePath}");
                    return false;
                }

                var fileInfo = new FileInfo(filePath);
                long fileSize = fileInfo.Length;

                int passCount = method.GetPassCount();
                System.Diagnostics.Debug.WriteLine($"SecureEraseHelper: Securely erasing file: {Path.GetFileName(filePath)} ({fileSize} bytes) with {passCount} passes ({method})");

                // Perform multiple passes based on method
                for (int pass = 1; pass <= passCount; pass++)
                {
                    byte[] pattern = GetPatternForPass(method, pass, fileSize);
                    bool success = OverwriteFileWithPattern(filePath, pattern, fileSize);

                    if (!success)
                    {
                        System.Diagnostics.Debug.WriteLine($"SecureEraseHelper: Failed at pass {pass}");
                        return false;
                    }

                    System.Diagnostics.Debug.WriteLine($"SecureEraseHelper: Completed pass {pass}/{passCount}");
                }

                // Delete the file after all passes
                File.Delete(filePath);

                System.Diagnostics.Debug.WriteLine($"SecureEraseHelper: File securely erased: {Path.GetFileName(filePath)}");
                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"SecureEraseHelper: Error erasing file: {ex.Message}");
                return false;
            }
        }

        private static byte[] GetPatternForPass(EraseMethod method, int passNumber, long fileSize)
        {
            return method switch
            {
                EraseMethod.Quick => new byte[] { 0x00 }, // Single pass: zeros
                EraseMethod.DoD3Pass => passNumber switch
                {
                    1 => new byte[] { 0x00 }, // Pass 1: zeros
                    2 => new byte[] { 0xFF }, // Pass 2: ones
                    3 => GetRandomPattern(), // Pass 3: random
                    _ => new byte[] { 0x00 }
                },
                EraseMethod.DoD7Pass => passNumber switch
                {
                    1 => new byte[] { 0x00 },
                    2 => new byte[] { 0xFF },
                    3 => GetRandomPattern(),
                    4 => new byte[] { 0x00 },
                    5 => new byte[] { 0xFF },
                    6 => GetRandomPattern(),
                    7 => GetRandomPattern(),
                    _ => new byte[] { 0x00 }
                },
                EraseMethod.Gutmann => GetGutmannPattern(passNumber),
                _ => new byte[] { 0x00 }
            };
        }

        private static byte[] GetGutmannPattern(int passNumber)
        {
            if (passNumber <= 4) return GetRandomPattern();
            if (passNumber <= 31)
            {
                int patternIndex = (passNumber - 5) % GutmannPatterns.Length;
                return GutmannPatterns[patternIndex];
            }
            return GetRandomPattern();
        }

        private static byte[] GetRandomPattern()
        {
            return new byte[] { 0xFF, 0xFF, 0xFF, 0xFF }; // Marker for random
        }

        private static bool OverwriteFileWithPattern(string filePath, byte[] pattern, long fileSize)
        {
            try
            {
                using (var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Write, FileShare.None))
                {
                    long bytesRemaining = fileSize;
                    bool isRandomPattern = pattern.Length == 4 && pattern[0] == 0xFF && pattern[1] == 0xFF && pattern[2] == 0xFF && pattern[3] == 0xFF;

                    while (bytesRemaining > 0)
                    {
                        int bytesToWrite = (int)Math.Min(BufferSize, bytesRemaining);
                        byte[] buffer;

                        if (isRandomPattern)
                        {
                            buffer = new byte[bytesToWrite];
                            _random.NextBytes(buffer);
                        }
                        else
                        {
                            buffer = new byte[bytesToWrite];
                            for (int i = 0; i < bytesToWrite; i++)
                            {
                                buffer[i] = pattern[i % pattern.Length];
                            }
                        }

                        fileStream.Write(buffer, 0, bytesToWrite);
                        bytesRemaining -= bytesToWrite;
                    }

                    fileStream.Flush();
                }

                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"SecureEraseHelper: Error overwriting file: {ex.Message}");
                return false;
            }
        }
    }
}
