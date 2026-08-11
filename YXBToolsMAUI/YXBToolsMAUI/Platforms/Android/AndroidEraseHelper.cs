using Android.Content;
using System.Security.Cryptography;

namespace YXBToolsMAUI.Platforms.Android;

/// <summary>
/// Helper for secure file erasure on Android, handling content URIs properly
/// </summary>
public static class AndroidEraseHelper
{
    private const int BufferSize = 65536; // 64KB buffer
    private static readonly Random _random = new Random();

    /// <summary>
    /// Securely overwrites a file via content URI
    /// </summary>
    public static async Task<bool> OverwriteContentUriFileAsync(string contentUri, byte[] pattern, IProgress<double>? progress = null, CancellationToken cancellationToken = default)
    {
        try
        {
            if (!contentUri.StartsWith("content://"))
            {
                System.Diagnostics.Debug.WriteLine($"Not a content URI: {contentUri}");
                return false;
            }

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

            // Get file size first
            long fileSize = 0;
            try
            {
                using var cursor = contentResolver.Query(uri, null, null, null, null);
                if (cursor != null && cursor.MoveToFirst())
                {
                    var sizeIndex = cursor.GetColumnIndex(global::Android.Provider.OpenableColumns.Size);
                    if (sizeIndex >= 0)
                    {
                        fileSize = cursor.GetLong(sizeIndex);
                        System.Diagnostics.Debug.WriteLine($"File size: {fileSize} bytes");
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error getting file size: {ex.Message}");
            }

            if (fileSize == 0)
            {
                System.Diagnostics.Debug.WriteLine("Could not determine file size or file is empty");
                return false;
            }

            // Open file descriptor with write mode (NOT truncate - we need to preserve size to overwrite)
            var fileDescriptor = contentResolver.OpenFileDescriptor(uri, "w"); // w=write only
            if (fileDescriptor == null)
            {
                System.Diagnostics.Debug.WriteLine("Failed to open file descriptor for writing");
                return false;
            }

            System.Diagnostics.Debug.WriteLine($"Overwriting content URI file with pattern (size: {fileSize} bytes)");

            // Check if this is random pattern marker
            bool isRandomPattern = pattern.Length == 4 && pattern[0] == 0xFF && pattern[1] == 0xFF && pattern[2] == 0xFF && pattern[3] == 0xFF;

            using (fileDescriptor)
            {
                var fd = fileDescriptor.FileDescriptor;
                if (fd == null || !fd.Valid())
                {
                    System.Diagnostics.Debug.WriteLine("File descriptor is invalid");
                    return false;
                }

                using var outputStream = new Java.IO.FileOutputStream(fd);

                long bytesRemaining = fileSize;
                long bytesWritten = 0;

                while (bytesRemaining > 0)
                {
                    if (cancellationToken.IsCancellationRequested)
                    {
                        System.Diagnostics.Debug.WriteLine("Overwrite cancelled");
                        return false;
                    }

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

                    await outputStream.WriteAsync(buffer, 0, buffer.Length);
                    bytesWritten += bytesToWrite;
                    bytesRemaining -= bytesToWrite;

                    // Report progress
                    progress?.Report((double)bytesWritten / fileSize);
                }

                // Force write to disk
                outputStream.Flush();
                outputStream.FD?.Sync();

                System.Diagnostics.Debug.WriteLine($"Successfully overwrote {bytesWritten} bytes via content URI");
            }

            return true;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error overwriting content URI file: {ex.Message}");
            System.Diagnostics.Debug.WriteLine($"Stack trace: {ex.StackTrace}");
            return false;
        }
    }

    /// <summary>
    /// Checks if a content URI is writable
    /// </summary>
    public static bool IsContentUriWritable(string contentUri)
    {
        try
        {
            if (!contentUri.StartsWith("content://"))
                return false;

            var uri = global::Android.Net.Uri.Parse(contentUri);
            if (uri == null)
                return false;

            var context = global::Android.App.Application.Context;
            if (context == null)
                return false;

            var contentResolver = context.ContentResolver;
            if (contentResolver == null)
                return false;

            // Try to open with write mode
            var fileDescriptor = contentResolver.OpenFileDescriptor(uri, "w");
            if (fileDescriptor != null)
            {
                fileDescriptor.Dispose();
                return true;
            }

            return false;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error checking if content URI is writable: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// Gets all file URIs from a document tree URI recursively
    /// </summary>
    public static List<string> GetFilesFromDocumentTree(string treeUri, bool includeSubfolders)
    {
        var fileUris = new List<string>();

        try
        {
            if (!treeUri.StartsWith("content://"))
            {
                System.Diagnostics.Debug.WriteLine($"Not a content URI: {treeUri}");
                return fileUris;
            }

            var uri = global::Android.Net.Uri.Parse(treeUri);
            if (uri == null)
            {
                System.Diagnostics.Debug.WriteLine("Failed to parse tree URI");
                return fileUris;
            }

            var context = global::Android.App.Application.Context;
            if (context == null)
            {
                System.Diagnostics.Debug.WriteLine("Application context is null");
                return fileUris;
            }

            // Use DocumentFile to enumerate the tree
            var documentFile = AndroidX.DocumentFile.Provider.DocumentFile.FromTreeUri(context, uri);
            if (documentFile == null)
            {
                System.Diagnostics.Debug.WriteLine("Failed to create DocumentFile from tree URI");
                return fileUris;
            }

            System.Diagnostics.Debug.WriteLine($"Enumerating files in document tree, recursive: {includeSubfolders}");
            EnumerateDocumentFiles(documentFile, fileUris, includeSubfolders);

            System.Diagnostics.Debug.WriteLine($"Found {fileUris.Count} files in document tree");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error enumerating document tree: {ex.Message}");
            System.Diagnostics.Debug.WriteLine($"Stack trace: {ex.StackTrace}");
        }

        return fileUris;
    }

    private static void EnumerateDocumentFiles(AndroidX.DocumentFile.Provider.DocumentFile folder, List<string> fileUris, bool recursive)
    {
        try
        {
            var files = folder.ListFiles();
            if (files == null || files.Length == 0)
                return;

            foreach (var file in files)
            {
                if (file == null)
                    continue;

                if (file.IsDirectory)
                {
                    // Recurse into subdirectories if enabled
                    if (recursive)
                    {
                        EnumerateDocumentFiles(file, fileUris, recursive);
                    }
                }
                else if (file.IsFile)
                {
                    var fileUri = file.Uri?.ToString();
                    if (!string.IsNullOrEmpty(fileUri))
                    {
                        fileUris.Add(fileUri);
                        System.Diagnostics.Debug.WriteLine($"Found file: {file.Name} ({fileUri})");
                    }
                }
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error enumerating folder: {ex.Message}");
        }
    }
}
