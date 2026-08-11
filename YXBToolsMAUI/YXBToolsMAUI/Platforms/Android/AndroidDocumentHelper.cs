using AndroidX.DocumentFile.Provider;
using Android.Content;

namespace YXBToolsMAUI.Platforms.Android;

public static class AndroidDocumentHelper
{
    /// <summary>
    /// Lists all files in a folder URI (non-recursive)
    /// </summary>
    public static List<(string uri, string name)> ListFilesInFolder(string folderUri)
    {
        var fileList = new List<(string uri, string name)>();

        try
        {
            var context = global::Android.App.Application.Context;
            if (context == null)
            {
                System.Diagnostics.Debug.WriteLine("AndroidDocumentHelper.ListFiles: Context is null");
                return fileList;
            }

            var uri = global::Android.Net.Uri.Parse(folderUri);
            if (uri == null)
            {
                System.Diagnostics.Debug.WriteLine($"AndroidDocumentHelper.ListFiles: Failed to parse URI: {folderUri}");
                return fileList;
            }

            var folder = DocumentFile.FromTreeUri(context, uri);
            if (folder == null || !folder.Exists() || !folder.IsDirectory)
            {
                System.Diagnostics.Debug.WriteLine($"AndroidDocumentHelper.ListFiles: Folder not valid");
                return fileList;
            }

            var files = folder.ListFiles();
            if (files != null)
            {
                foreach (var file in files)
                {
                    if (file != null && file.IsFile)
                    {
                        var fileName = file.Name;
                        var fileUri = file.Uri?.ToString();

                        if (!string.IsNullOrEmpty(fileName) && !string.IsNullOrEmpty(fileUri))
                        {
                            fileList.Add((fileUri, fileName));
                            System.Diagnostics.Debug.WriteLine($"AndroidDocumentHelper.ListFiles: Found file: {fileName}");
                        }
                    }
                }
            }

            System.Diagnostics.Debug.WriteLine($"AndroidDocumentHelper.ListFiles: Found {fileList.Count} files");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"AndroidDocumentHelper.ListFiles: Error: {ex.Message}");
        }

        return fileList;
    }

    /// <summary>
    /// Reads a file from a content URI
    /// </summary>
    public static async Task<byte[]?> ReadFileFromUri(string fileUri)
    {
        try
        {
            var context = global::Android.App.Application.Context;
            if (context == null) return null;

            var uri = global::Android.Net.Uri.Parse(fileUri);
            if (uri == null) return null;

            var contentResolver = context.ContentResolver;
            if (contentResolver == null) return null;

            using var inputStream = contentResolver.OpenInputStream(uri);
            if (inputStream == null) return null;

            using var memoryStream = new MemoryStream();
            await inputStream.CopyToAsync(memoryStream);

            System.Diagnostics.Debug.WriteLine($"AndroidDocumentHelper.ReadFile: Read {memoryStream.Length} bytes");
            return memoryStream.ToArray();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"AndroidDocumentHelper.ReadFile: Error: {ex.Message}");
            return null;
        }
    }

    /// <summary>
    /// Creates a file in the specified folder URI using DocumentFile API
    /// </summary>
    public static async Task<bool> WriteFileToDocumentUri(string folderUri, string fileName, byte[] data)
    {
        try
        {
            var context = global::Android.App.Application.Context;
            if (context == null)
            {
                System.Diagnostics.Debug.WriteLine("AndroidDocumentHelper: Context is null");
                return false;
            }

            var uri = global::Android.Net.Uri.Parse(folderUri);
            if (uri == null)
            {
                System.Diagnostics.Debug.WriteLine($"AndroidDocumentHelper: Failed to parse URI: {folderUri}");
                return false;
            }

            // Get DocumentFile for the folder
            var folder = DocumentFile.FromTreeUri(context, uri);
            if (folder == null || !folder.Exists() || !folder.IsDirectory)
            {
                System.Diagnostics.Debug.WriteLine($"AndroidDocumentHelper: Folder not valid or doesn't exist");
                return false;
            }

            // Check if file already exists and delete it
            var existingFile = folder.FindFile(fileName);
            if (existingFile != null && existingFile.Exists())
            {
                System.Diagnostics.Debug.WriteLine($"AndroidDocumentHelper: Deleting existing file: {fileName}");
                existingFile.Delete();
            }

            // Create new file
            var newFile = folder.CreateFile("application/octet-stream", fileName);
            if (newFile == null)
            {
                System.Diagnostics.Debug.WriteLine($"AndroidDocumentHelper: Failed to create file: {fileName}");
                return false;
            }

            // Write data to file
            var contentResolver = context.ContentResolver;
            if (contentResolver == null)
            {
                System.Diagnostics.Debug.WriteLine("AndroidDocumentHelper: ContentResolver is null");
                return false;
            }

            using var outputStream = contentResolver.OpenOutputStream(newFile.Uri);
            if (outputStream == null)
            {
                System.Diagnostics.Debug.WriteLine($"AndroidDocumentHelper: Failed to open output stream for: {fileName}");
                return false;
            }

            await outputStream.WriteAsync(data, 0, data.Length);
            await outputStream.FlushAsync();

            System.Diagnostics.Debug.WriteLine($"AndroidDocumentHelper: Successfully wrote file: {fileName}");
            return true;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"AndroidDocumentHelper: Error writing file: {ex.Message}");
            System.Diagnostics.Debug.WriteLine($"Stack trace: {ex.StackTrace}");
            return false;
        }
    }

    /// <summary>
    /// Writes a file to document URI using a stream-based approach
    /// </summary>
    public static async Task<bool> WriteFileToDocumentUriFromStream(string folderUri, string fileName, Stream sourceStream)
    {
        try
        {
            var context = global::Android.App.Application.Context;
            if (context == null) return false;

            var uri = global::Android.Net.Uri.Parse(folderUri);
            if (uri == null) return false;

            var folder = DocumentFile.FromTreeUri(context, uri);
            if (folder == null || !folder.Exists() || !folder.IsDirectory) return false;

            // Check if file exists and delete
            var existingFile = folder.FindFile(fileName);
            existingFile?.Delete();

            // Create new file
            var newFile = folder.CreateFile("application/octet-stream", fileName);
            if (newFile == null) return false;

            // Write from source stream
            var contentResolver = context.ContentResolver;
            if (contentResolver == null) return false;

            using var outputStream = contentResolver.OpenOutputStream(newFile.Uri);
            if (outputStream == null) return false;

            await sourceStream.CopyToAsync(outputStream);
            await outputStream.FlushAsync();

            System.Diagnostics.Debug.WriteLine($"AndroidDocumentHelper: Successfully wrote file from stream: {fileName}");
            return true;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"AndroidDocumentHelper: Error writing file from stream: {ex.Message}");
            return false;
        }
    }
}
