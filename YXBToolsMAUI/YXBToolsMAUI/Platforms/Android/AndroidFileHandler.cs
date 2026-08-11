using Android.Content;

namespace YXBToolsMAUI.Platforms.Android;

public static class AndroidFileHandler
{
    private static string GetTempWorkingDirectory()
    {
        var tempPath = Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.LocalApplicationData), "temp");
        if (!Directory.Exists(tempPath))
        {
            Directory.CreateDirectory(tempPath);
        }
        return tempPath;
    }

    public static async Task<string?> CopyFileToWorkingDirectoryAsync(string sourceFilePath)
    {
        try
        {
            System.Diagnostics.Debug.WriteLine($"CopyFileToWorkingDirectoryAsync called with: {sourceFilePath}");

            var tempDir = GetTempWorkingDirectory();
            string? fileName = null;
            Stream? sourceStream = null;

            // Check if it's a content URI
            if (sourceFilePath.StartsWith("content://"))
            {
                System.Diagnostics.Debug.WriteLine("Detected content URI, using ContentResolver");

                var uri = global::Android.Net.Uri.Parse(sourceFilePath);
                if (uri == null)
                {
                    System.Diagnostics.Debug.WriteLine("Failed to parse URI");
                    return null;
                }

                var context = global::Android.App.Application.Context;
                if (context == null)
                {
                    System.Diagnostics.Debug.WriteLine("Application context is null");
                    return null;
                }

                var contentResolver = context.ContentResolver;
                if (contentResolver == null)
                {
                    System.Diagnostics.Debug.WriteLine("ContentResolver is null");
                    return null;
                }

                // Get the file name from the content URI
                try
                {
                    using var cursor = contentResolver.Query(uri, null, null, null, null);
                    if (cursor != null && cursor.MoveToFirst())
                    {
                        var displayNameIndex = cursor.GetColumnIndex(global::Android.Provider.OpenableColumns.DisplayName);
                        if (displayNameIndex >= 0)
                        {
                            fileName = cursor.GetString(displayNameIndex);
                            System.Diagnostics.Debug.WriteLine($"Got filename from URI: {fileName}");
                        }
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error querying cursor: {ex.Message}");
                }

                // If we couldn't get the filename, generate one
                if (string.IsNullOrEmpty(fileName))
                {
                    fileName = $"file_{DateTime.Now:yyyyMMddHHmmss}";
                    System.Diagnostics.Debug.WriteLine($"Could not get filename from URI, using generated: {fileName}");
                }

                // Open stream from content URI
                sourceStream = contentResolver.OpenInputStream(uri);
                if (sourceStream == null)
                {
                    System.Diagnostics.Debug.WriteLine("Failed to open input stream from content URI");
                    return null;
                }

                System.Diagnostics.Debug.WriteLine($"Successfully opened input stream from content URI");
            }
            else
            {
                // Regular file path
                if (!File.Exists(sourceFilePath))
                {
                    System.Diagnostics.Debug.WriteLine($"Source file doesn't exist: {sourceFilePath}");
                    return null;
                }

                fileName = Path.GetFileName(sourceFilePath);
                sourceStream = File.OpenRead(sourceFilePath);
            }

            var tempFilePath = Path.Combine(tempDir, fileName);
            System.Diagnostics.Debug.WriteLine($"Copying file to: {tempFilePath}");

            await using (sourceStream)
            await using (var destStream = File.Create(tempFilePath))
            {
                await sourceStream.CopyToAsync(destStream);
            }

            System.Diagnostics.Debug.WriteLine($"File copied successfully to {tempFilePath}");
            return tempFilePath;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error copying file: {ex.Message}");
            System.Diagnostics.Debug.WriteLine($"Stack trace: {ex.StackTrace}");
            return null;
        }
    }

    public static async Task<string?> CopyFolderToWorkingDirectoryAsync(string sourceFolderPath)
    {
        try
        {
            System.Diagnostics.Debug.WriteLine($"CopyFolderToWorkingDirectoryAsync called with: {sourceFolderPath}");

            // Content URIs for folders are not supported yet
            if (sourceFolderPath.StartsWith("content://"))
            {
                System.Diagnostics.Debug.WriteLine("Content URI folders are not yet supported");
                return null;
            }

            if (!Directory.Exists(sourceFolderPath))
            {
                System.Diagnostics.Debug.WriteLine($"Source folder doesn't exist: {sourceFolderPath}");
                return null;
            }

            var tempDir = GetTempWorkingDirectory();
            var folderName = Path.GetFileName(sourceFolderPath);
            var tempFolderPath = Path.Combine(tempDir, folderName);

            if (Directory.Exists(tempFolderPath))
            {
                Directory.Delete(tempFolderPath, true);
            }

            Directory.CreateDirectory(tempFolderPath);

            System.Diagnostics.Debug.WriteLine($"Copying folder from {sourceFolderPath} to {tempFolderPath}");

            await CopyDirectoryRecursiveAsync(sourceFolderPath, tempFolderPath);

            System.Diagnostics.Debug.WriteLine($"Folder copied successfully to {tempFolderPath}");
            return tempFolderPath;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error copying folder: {ex.Message}");
            System.Diagnostics.Debug.WriteLine($"Stack trace: {ex.StackTrace}");
            return null;
        }
    }

    private static async Task CopyDirectoryRecursiveAsync(string sourceDir, string destDir)
    {
        Directory.CreateDirectory(destDir);

        foreach (var file in Directory.GetFiles(sourceDir))
        {
            var fileName = Path.GetFileName(file);
            var destFile = Path.Combine(destDir, fileName);

            await using var sourceStream = File.OpenRead(file);
            await using var destStream = File.Create(destFile);
            await sourceStream.CopyToAsync(destStream);
        }

        foreach (var dir in Directory.GetDirectories(sourceDir))
        {
            var dirName = Path.GetFileName(dir);
            var destSubDir = Path.Combine(destDir, dirName);
            await CopyDirectoryRecursiveAsync(dir, destSubDir);
        }
    }

    /// <summary>
    /// SECURITY CRITICAL: Securely erases all temp files before deleting directory
    /// Simple deletion leaves data recoverable - this overwrites files first
    /// </summary>
    public static async Task SecureCleanupTempDirectoryAsync()
    {
        try
        {
            var tempDir = GetTempWorkingDirectory();
            if (!Directory.Exists(tempDir))
            {
                System.Diagnostics.Debug.WriteLine("Temp directory doesn't exist, nothing to cleanup");
                return;
            }

            System.Diagnostics.Debug.WriteLine($"SECURITY: Starting secure cleanup of temp directory: {tempDir}");

            // Get all files in temp directory recursively
            var tempFiles = Directory.GetFiles(tempDir, "*", SearchOption.AllDirectories);

            System.Diagnostics.Debug.WriteLine($"SECURITY: Found {tempFiles.Length} temp files to securely erase");

            int erasedCount = 0;

            // Securely erase each temp file before deleting directory
            foreach (var tempFile in tempFiles)
            {
                try
                {
                    var fileInfo = new FileInfo(tempFile);
                    long fileSize = fileInfo.Length;

                    if (fileSize == 0)
                    {
                        File.Delete(tempFile); // Empty file, just delete
                        continue;
                    }

                    System.Diagnostics.Debug.WriteLine($"SECURITY: Securely erasing temp file: {Path.GetFileName(tempFile)} ({fileSize} bytes)");

                    // Quick overwrite with zeros (1 pass is sufficient for temp cleanup)
                    using (var fileStream = new FileStream(tempFile, FileMode.Open, FileAccess.Write, FileShare.None))
                    {
                        long bytesRemaining = fileSize;
                        byte[] buffer = new byte[Math.Min(65536, fileSize)]; // 64KB buffer
                        Array.Fill<byte>(buffer, 0x00); // Fill with zeros

                        while (bytesRemaining > 0)
                        {
                            int bytesToWrite = (int)Math.Min(buffer.Length, bytesRemaining);
                            await fileStream.WriteAsync(buffer, 0, bytesToWrite);
                            bytesRemaining -= bytesToWrite;
                        }

                        await fileStream.FlushAsync();
                    }

                    // Now delete the overwritten file
                    File.Delete(tempFile);
                    erasedCount++;

                    System.Diagnostics.Debug.WriteLine($"SECURITY: Temp file securely erased and deleted");
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"SECURITY WARNING: Failed to securely erase temp file {tempFile}: {ex.Message}");
                    // Try to at least delete it normally
                    try { File.Delete(tempFile); } catch { }
                }
            }

            // Delete the now-empty directory
            Directory.Delete(tempDir, true);

            System.Diagnostics.Debug.WriteLine($"SECURITY: Temp directory securely cleaned up - {erasedCount}/{tempFiles.Length} files securely erased");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"SECURITY ERROR: Error during secure temp cleanup: {ex.Message}");
        }
    }

    /// <summary>
    /// DEPRECATED: Use SecureCleanupTempDirectoryAsync() instead
    /// This method is kept for backward compatibility but should not be used for security-sensitive operations
    /// </summary>
    [Obsolete("Use SecureCleanupTempDirectoryAsync() for secure cleanup of sensitive data")]
    public static void CleanupTempDirectory()
    {
        try
        {
            var tempDir = GetTempWorkingDirectory();
            if (Directory.Exists(tempDir))
            {
                Directory.Delete(tempDir, true);
                System.Diagnostics.Debug.WriteLine("WARNING: Using non-secure temp cleanup - data may be recoverable!");
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error cleaning temp directory: {ex.Message}");
        }
    }

    /// <summary>
    /// Deletes a file from a content URI (for files selected via Android file picker)
    /// </summary>
    public static bool DeleteContentUriFile(string contentUri)
    {
        try
        {
            if (!contentUri.StartsWith("content://"))
            {
                System.Diagnostics.Debug.WriteLine($"Not a content URI: {contentUri}");
                return false;
            }

            System.Diagnostics.Debug.WriteLine($"Attempting to delete content URI: {contentUri}");

            var uri = global::Android.Net.Uri.Parse(contentUri);
            if (uri == null)
            {
                System.Diagnostics.Debug.WriteLine("Failed to parse URI");
                return false;
            }

            var context = global::Android.App.Application.Context;
            if (context == null)
            {
                System.Diagnostics.Debug.WriteLine("Application context is null");
                return false;
            }

            var contentResolver = context.ContentResolver;
            if (contentResolver == null)
            {
                System.Diagnostics.Debug.WriteLine("ContentResolver is null");
                return false;
            }

            // Check if this is a document URI (required for DocumentsContract.DeleteDocument)
            bool isDocumentUri = global::Android.Provider.DocumentsContract.IsDocumentUri(context, uri);
            System.Diagnostics.Debug.WriteLine($"Is document URI: {isDocumentUri}");

            if (!isDocumentUri)
            {
                System.Diagnostics.Debug.WriteLine("URI is not a document URI, cannot delete via DocumentsContract");
                System.Diagnostics.Debug.WriteLine("This file was likely selected from a media provider and can only be deleted by the system");
                return false;
            }

            // Use DocumentsContract to delete the file
            bool deleted = global::Android.Provider.DocumentsContract.DeleteDocument(contentResolver, uri);

            if (deleted)
            {
                System.Diagnostics.Debug.WriteLine($"Successfully deleted content URI file");
            }
            else
            {
                System.Diagnostics.Debug.WriteLine($"Failed to delete content URI file (DeleteDocument returned false)");
            }

            return deleted;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error deleting content URI file: {ex.Message}");
            System.Diagnostics.Debug.WriteLine($"Stack trace: {ex.StackTrace}");
            return false;
        }
    }
}
