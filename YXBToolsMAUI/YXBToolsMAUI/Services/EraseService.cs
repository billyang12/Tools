namespace YXBToolsMAUI.Services;

public class EraseService : IEraseService
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

    public async Task<bool> EraseFileAsync(string filePath, EraseMethod method = EraseMethod.Quick, IProgress<(int currentPass, int totalPasses)>? passProgress = null, CancellationToken cancellationToken = default)
    {
        try
        {
#if ANDROID
            // For Android content URIs, use special handling to overwrite original file directly
            if (filePath.StartsWith("content://"))
            {
                return await EraseAndroidContentUriFileAsync(filePath, method, passProgress, cancellationToken);
            }
#endif

            if (!File.Exists(filePath))
            {
                System.Diagnostics.Debug.WriteLine($"EraseService: File doesn't exist: {filePath}");
                return false;
            }

            var fileInfo = new FileInfo(filePath);
            long fileSize = fileInfo.Length;

            int passCount = method.GetPassCount();
            System.Diagnostics.Debug.WriteLine($"EraseService: Erasing file: {filePath} ({fileSize} bytes) with {passCount} passes ({method})");

            // Perform multiple passes based on method
            for (int pass = 1; pass <= passCount; pass++)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    System.Diagnostics.Debug.WriteLine("EraseService: Operation cancelled");
                    return false;
                }

                passProgress?.Report((pass, passCount));

                byte[] pattern = GetPatternForPass(method, pass, fileSize);
                bool success = await OverwriteFileWithPattern(filePath, pattern, fileSize, cancellationToken);

                if (!success)
                {
                    System.Diagnostics.Debug.WriteLine($"EraseService: Failed at pass {pass}");
                    return false;
                }

                System.Diagnostics.Debug.WriteLine($"EraseService: Completed pass {pass}/{passCount}");
            }

            // Delete the file after all passes
            // NOTE: File.Delete() permanently deletes the file without sending to Recycle Bin
            // This is intentional for secure erase - file will NOT appear in Recycle Bin
            File.Delete(filePath);

            System.Diagnostics.Debug.WriteLine($"EraseService: File erased successfully (permanently deleted, not in Recycle Bin): {filePath}");
            return true;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"EraseService: Error erasing file: {ex.Message}");
            System.Diagnostics.Debug.WriteLine($"Stack trace: {ex.StackTrace}");
            return false;
        }
    }

#if ANDROID
    private async Task<bool> EraseAndroidContentUriFileAsync(string contentUri, EraseMethod method, IProgress<(int currentPass, int totalPasses)>? passProgress = null, CancellationToken cancellationToken = default)
    {
        try
        {
            System.Diagnostics.Debug.WriteLine($"EraseService: Erasing Android content URI file: {contentUri}");

            int passCount = method.GetPassCount();
            System.Diagnostics.Debug.WriteLine($"EraseService: Using {passCount} passes ({method})");

            // Perform multiple passes based on method
            for (int pass = 1; pass <= passCount; pass++)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    System.Diagnostics.Debug.WriteLine("EraseService: Operation cancelled");
                    return false;
                }

                passProgress?.Report((pass, passCount));

                byte[] pattern = GetPatternForPass(method, pass, 0); // Size not needed for pattern selection
                bool success = await YXBToolsMAUI.Platforms.Android.AndroidEraseHelper.OverwriteContentUriFileAsync(contentUri, pattern, null, cancellationToken);

                if (!success)
                {
                    System.Diagnostics.Debug.WriteLine($"EraseService: Failed at pass {pass} for content URI");
                    return false;
                }

                System.Diagnostics.Debug.WriteLine($"EraseService: Completed pass {pass}/{passCount} for content URI");
            }

            // Delete the file after all passes using DocumentsContract
            bool deleted = YXBToolsMAUI.Platforms.Android.AndroidFileHandler.DeleteContentUriFile(contentUri);

            if (!deleted)
            {
                System.Diagnostics.Debug.WriteLine("EraseService: Failed to delete content URI file after overwriting");
                return false;
            }

            System.Diagnostics.Debug.WriteLine($"EraseService: Content URI file erased successfully");
            return true;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"EraseService: Error erasing content URI file: {ex.Message}");
            System.Diagnostics.Debug.WriteLine($"Stack trace: {ex.StackTrace}");
            return false;
        }
    }
#endif

    private byte[] GetPatternForPass(EraseMethod method, int passNumber, long fileSize)
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

    private byte[] GetGutmannPattern(int passNumber, long fileSize)
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

    private byte[] GetRandomPattern(long fileSize)
    {
        // Return a marker that indicates random data should be generated
        // We'll generate it in real-time during overwrite to save memory
        return new byte[] { 0xFF, 0xFF, 0xFF, 0xFF }; // Special marker for random
    }

    private async Task<bool> OverwriteFileWithPattern(string filePath, byte[] pattern, long fileSize, CancellationToken cancellationToken)
    {
        try
        {
            using (var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Write, FileShare.None))
            {
                long bytesRemaining = fileSize;
                long position = 0;

                // Check if this is random pattern (marker)
                bool isRandomPattern = pattern.Length == 4 && pattern[0] == 0xFF && pattern[1] == 0xFF && pattern[2] == 0xFF && pattern[3] == 0xFF;

                while (bytesRemaining > 0)
                {
                    if (cancellationToken.IsCancellationRequested)
                        return false;

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

                    await fileStream.WriteAsync(buffer, 0, bytesToWrite, cancellationToken);

                    position += bytesToWrite;
                    bytesRemaining -= bytesToWrite;
                }

                // Flush to ensure data is written to disk
                await fileStream.FlushAsync(cancellationToken);
            }

            return true;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"EraseService: Error overwriting file: {ex.Message}");
            return false;
        }
    }

    public async Task<bool> EraseFolderAsync(string folderPath, EraseMethod method = EraseMethod.Quick, bool includeSubfolders = false, IProgress<(int current, int total, string fileName)>? progress = null, IProgress<(int currentPass, int totalPasses)>? passProgress = null, CancellationToken cancellationToken = default)
    {
        try
        {
#if ANDROID
            // Handle Android content URI folders (from document picker)
            if (folderPath.StartsWith("content://"))
            {
                System.Diagnostics.Debug.WriteLine($"EraseService: Erasing Android document tree: {folderPath}");

                // Get all file URIs from the document tree
                var fileUris = YXBToolsMAUI.Platforms.Android.AndroidEraseHelper.GetFilesFromDocumentTree(folderPath, includeSubfolders);

                if (fileUris.Count == 0)
                {
                    System.Diagnostics.Debug.WriteLine("EraseService: No files found in document tree");
                    return false;
                }

                string recursiveText = includeSubfolders ? " (including subfolders)" : "";
                System.Diagnostics.Debug.WriteLine($"EraseService: Erasing {fileUris.Count} files from document tree{recursiveText} using {method} ({method.GetPassCount()} passes)");

                int successCount = 0;
                int totalFiles = fileUris.Count;

                for (int i = 0; i < fileUris.Count; i++)
                {
                    if (cancellationToken.IsCancellationRequested)
                    {
                        System.Diagnostics.Debug.WriteLine("EraseService: Document tree erase cancelled");
                        break;
                    }

                    string fileUri = fileUris[i];

                    // Report file progress (just show index since we can't easily get name from URI)
                    progress?.Report((i + 1, totalFiles, $"File {i + 1}"));

                    // Erase the file via content URI
                    bool success = await EraseFileAsync(fileUri, method, passProgress, cancellationToken);
                    if (success)
                    {
                        successCount++;
                    }
                }

                System.Diagnostics.Debug.WriteLine($"EraseService: Erased {successCount} out of {totalFiles} files from document tree");
                return successCount > 0;
            }
#endif

            if (!Directory.Exists(folderPath))
            {
                System.Diagnostics.Debug.WriteLine($"EraseService: Folder doesn't exist: {folderPath}");
                return false;
            }

            // Get all files based on recursive option
            var searchOption = includeSubfolders ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;
            var files = Directory.GetFiles(folderPath, "*", searchOption);

            if (files.Length == 0)
            {
                System.Diagnostics.Debug.WriteLine("EraseService: No files found in folder");
                return false;
            }

            string recursiveInfo = includeSubfolders ? " (including subfolders)" : "";
            System.Diagnostics.Debug.WriteLine($"EraseService: Erasing {files.Length} files from folder: {folderPath}{recursiveInfo} using {method} ({method.GetPassCount()} passes)");

            int erased = 0;
            int total = files.Length;

            for (int i = 0; i < files.Length; i++)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    System.Diagnostics.Debug.WriteLine("EraseService: Folder erase cancelled");
                    break;
                }

                string file = files[i];
                string fileName = Path.GetFileName(file);
                string relativePath = includeSubfolders ? file.Replace(folderPath, "").TrimStart(Path.DirectorySeparatorChar) : fileName;

                // Report file progress
                progress?.Report((i + 1, total, relativePath));

                // Erase the file with specified method
                bool success = await EraseFileAsync(file, method, passProgress, cancellationToken);
                if (success)
                {
                    erased++;
                }
            }

            System.Diagnostics.Debug.WriteLine($"EraseService: Erased {erased} out of {total} files");
            return erased > 0;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"EraseService: Error erasing folder: {ex.Message}");
            System.Diagnostics.Debug.WriteLine($"Stack trace: {ex.StackTrace}");
            return false;
        }
    }
}
