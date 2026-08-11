namespace YXBPictureViewMAUI.Services;

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
        new byte[] { 0x55 }, // Pattern 1
        new byte[] { 0xAA }, // Pattern 2
        new byte[] { 0x92, 0x49, 0x24 }, // Pattern 3
        new byte[] { 0x49, 0x24, 0x92 }, // Pattern 4
        new byte[] { 0x24, 0x92, 0x49 }, // Pattern 5
        new byte[] { 0x00 }, // Pattern 6
        new byte[] { 0x11 }, // Pattern 7
        new byte[] { 0x22 }, // Pattern 8
        new byte[] { 0x33 }, // Pattern 9
        new byte[] { 0x44 }, // Pattern 10
        new byte[] { 0x55 }, // Pattern 11
        new byte[] { 0x66 }, // Pattern 12
        new byte[] { 0x77 }, // Pattern 13
        new byte[] { 0x88 }, // Pattern 14
        new byte[] { 0x99 }, // Pattern 15
        new byte[] { 0xAA }, // Pattern 16
        new byte[] { 0xBB }, // Pattern 17
        new byte[] { 0xCC }, // Pattern 18
        new byte[] { 0xDD }, // Pattern 19
        new byte[] { 0xEE }, // Pattern 20
        new byte[] { 0xFF }, // Pattern 21
        new byte[] { 0x92, 0x49, 0x24 }, // Pattern 22
        new byte[] { 0x49, 0x24, 0x92 }, // Pattern 23
        new byte[] { 0x24, 0x92, 0x49 }, // Pattern 24
        new byte[] { 0x6D, 0xB6, 0xDB }, // Pattern 25
        new byte[] { 0xB6, 0xDB, 0x6D }, // Pattern 26
        new byte[] { 0xDB, 0x6D, 0xB6 }, // Pattern 27
    };

    /// <summary>
    /// Securely erases a file by overwriting it with patterns before deletion
    /// </summary>
    public static async Task<bool> SecureEraseFileAsync(string filePath, EraseMethod method = EraseMethod.Quick)
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
                bool success = await OverwriteFileWithPattern(filePath, pattern, fileSize);

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

    /// <summary>
    /// Securely erases multiple files (e.g., all temp files in a directory)
    /// </summary>
    public static async Task<int> SecureEraseFilesAsync(IEnumerable<string> filePaths, EraseMethod method = EraseMethod.Quick)
    {
        int erasedCount = 0;

        foreach (var filePath in filePaths)
        {
            try
            {
                if (await SecureEraseFileAsync(filePath, method))
                {
                    erasedCount++;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"SecureEraseHelper: Error erasing {filePath}: {ex.Message}");
            }
        }

        return erasedCount;
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
                3 => GetRandomPattern(fileSize), // Pass 3: random
                _ => new byte[] { 0x00 }
            },

            EraseMethod.DoD7Pass => passNumber switch
            {
                1 => new byte[] { 0x00 }, // Pass 1: zeros
                2 => new byte[] { 0xFF }, // Pass 2: ones
                3 => GetRandomPattern(fileSize), // Pass 3: random
                4 => new byte[] { 0x00 }, // Pass 4: zeros
                5 => new byte[] { 0xFF }, // Pass 5: ones
                6 => GetRandomPattern(fileSize), // Pass 6: random
                7 => GetRandomPattern(fileSize), // Pass 7: random
                _ => new byte[] { 0x00 }
            },

            EraseMethod.Gutmann => GetGutmannPattern(passNumber, fileSize),

            _ => new byte[] { 0x00 }
        };
    }

    private static byte[] GetGutmannPattern(int passNumber, long fileSize)
    {
        // First 4 passes: random
        if (passNumber <= 4)
            return GetRandomPattern(fileSize);

        // Middle 27 passes: specific patterns
        if (passNumber <= 31)
        {
            int patternIndex = (passNumber - 5) % GutmannPatterns.Length;
            return GutmannPatterns[patternIndex];
        }

        // Last 4 passes: random
        return GetRandomPattern(fileSize);
    }

    private static byte[] GetRandomPattern(long fileSize)
    {
        // Return a marker that indicates random data should be generated
        // We'll generate it in real-time during overwrite to save memory
        return new byte[] { 0xFF, 0xFF, 0xFF, 0xFF }; // Special marker for random
    }

    private static async Task<bool> OverwriteFileWithPattern(string filePath, byte[] pattern, long fileSize)
    {
        try
        {
            using (var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Write, FileShare.None))
            {
                long bytesRemaining = fileSize;

                // Check if this is random pattern (marker)
                bool isRandomPattern = pattern.Length == 4 && pattern[0] == 0xFF && pattern[1] == 0xFF && pattern[2] == 0xFF && pattern[3] == 0xFF;

                while (bytesRemaining > 0)
                {
                    int bytesToWrite = (int)Math.Min(BufferSize, bytesRemaining);
                    byte[] buffer;

                    if (isRandomPattern)
                    {
                        // Generate random data
                        buffer = new byte[bytesToWrite];
                        _random.NextBytes(buffer);
                    }
                    else
                    {
                        // Fill buffer with pattern
                        buffer = new byte[bytesToWrite];
                        for (int i = 0; i < bytesToWrite; i++)
                        {
                            buffer[i] = pattern[i % pattern.Length];
                        }
                    }

                    await fileStream.WriteAsync(buffer, 0, bytesToWrite);
                    bytesRemaining -= bytesToWrite;
                }

                // Flush to ensure data is written to disk
                await fileStream.FlushAsync();
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
