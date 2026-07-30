namespace YXBPictureViewMAUI.Services;

public static class FileSystemHelper
{
    public static string[] GetFiles(string folderPath, string searchPattern, SearchOption searchOption)
    {
        System.Diagnostics.Debug.WriteLine($"FileSystemHelper.GetFiles called with path: {folderPath}, pattern: {searchPattern}");
#if ANDROID
        if (folderPath.StartsWith("content://"))
        {
            System.Diagnostics.Debug.WriteLine("Using content URI method");
            return GetFilesFromContentUri(folderPath, searchPattern, searchOption);
        }
        else
        {
            System.Diagnostics.Debug.WriteLine("Path does not start with content://, using regular file path");
        }
#endif
        try
        {
            var files = Directory.GetFiles(folderPath, searchPattern, searchOption);
            System.Diagnostics.Debug.WriteLine($"Directory.GetFiles returned {files.Length} files");
            return files;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Directory.GetFiles error: {ex.Message}");
            return Array.Empty<string>();
        }
    }

#if ANDROID
    private static string[] GetFilesFromContentUri(string uriString, string searchPattern, SearchOption searchOption)
    {
        try
        {
            System.Diagnostics.Debug.WriteLine($"GetFilesFromContentUri: Parsing URI {uriString}");
            var uri = Android.Net.Uri.Parse(uriString);
            var context = Platform.CurrentActivity;

            if (context == null)
            {
                System.Diagnostics.Debug.WriteLine("GetFilesFromContentUri: context is null");
                return Array.Empty<string>();
            }

            if (uri == null)
            {
                System.Diagnostics.Debug.WriteLine("GetFilesFromContentUri: uri is null");
                return Array.Empty<string>();
            }

            System.Diagnostics.Debug.WriteLine($"GetFilesFromContentUri: Creating DocumentFile from URI");
            var docFile = AndroidX.DocumentFile.Provider.DocumentFile.FromTreeUri(context, uri);

            if (docFile == null)
            {
                System.Diagnostics.Debug.WriteLine("GetFilesFromContentUri: docFile is null");
                return Array.Empty<string>();
            }

            if (!docFile.IsDirectory)
            {
                System.Diagnostics.Debug.WriteLine("GetFilesFromContentUri: docFile is not a directory");
                return Array.Empty<string>();
            }

            System.Diagnostics.Debug.WriteLine($"GetFilesFromContentUri: Enumerating files with extension {searchPattern}");
            var files = new List<string>();
            var extension = searchPattern.Replace("*", "").Replace(".", "");

            EnumerateFiles(docFile, extension, searchOption == SearchOption.AllDirectories, files);

            System.Diagnostics.Debug.WriteLine($"GetFilesFromContentUri: Found {files.Count} files");
            return files.ToArray();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error getting files from content URI: {ex.Message}");
            System.Diagnostics.Debug.WriteLine($"Stack trace: {ex.StackTrace}");
            return Array.Empty<string>();
        }
    }

    private static void EnumerateFiles(AndroidX.DocumentFile.Provider.DocumentFile directory, string extension, bool recursive, List<string> results)
    {
        try
        {
            var files = directory.ListFiles();
            if (files == null) return;

            foreach (var file in files)
            {
                if (file.IsFile && (string.IsNullOrEmpty(extension) || file.Name?.EndsWith($".{extension}", StringComparison.OrdinalIgnoreCase) == true))
                {
                    // Store the content URI as the file path
                    if (file.Uri != null)
                    {
                        results.Add(file.Uri.ToString());
                    }
                }
                else if (file.IsDirectory && recursive)
                {
                    EnumerateFiles(file, extension, recursive, results);
                }
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error enumerating files: {ex.Message}");
        }
    }

#endif

    public static string GetFileName(string path)
    {
#if ANDROID
        if (path.StartsWith("content://"))
        {
            try
            {
                var uri = Android.Net.Uri.Parse(path);
                var context = Platform.CurrentActivity;

                if (context == null || uri == null)
                    return path;

                var docFile = AndroidX.DocumentFile.Provider.DocumentFile.FromSingleUri(context, uri);
                return docFile?.Name ?? path;
            }
            catch
            {
                return path;
            }
        }
#endif
        return Path.GetFileName(path);
    }

    public static string GetExtension(string path)
    {
#if ANDROID
        if (path.StartsWith("content://"))
        {
            var fileName = GetFileName(path);
            return Path.GetExtension(fileName);
        }
#endif
        return Path.GetExtension(path);
    }

    public static async Task<byte[]?> ReadAllBytesAsync(string path)
    {
#if ANDROID
        if (path.StartsWith("content://"))
        {
            try
            {
                var uri = Android.Net.Uri.Parse(path);
                var context = Platform.CurrentActivity;

                if (context?.ContentResolver == null || uri == null)
                    return null;

                using var inputStream = context.ContentResolver.OpenInputStream(uri);
                if (inputStream == null)
                    return null;

                // Get file size for better memory allocation
                long fileSize = inputStream.Length;
                System.Diagnostics.Debug.WriteLine($"Reading file of size: {fileSize} bytes ({fileSize / 1024.0 / 1024.0:F2} MB)");

                // Use a memory stream with initial capacity for better performance
                using var memoryStream = new MemoryStream((int)fileSize);

                // Use a buffer for copying to avoid OutOfMemory for large files
                byte[] buffer = new byte[81920]; // 80KB buffer
                int bytesRead;
                long totalRead = 0;

                while ((bytesRead = await inputStream.ReadAsync(buffer, 0, buffer.Length)) > 0)
                {
                    await memoryStream.WriteAsync(buffer, 0, bytesRead);
                    totalRead += bytesRead;
                }

                System.Diagnostics.Debug.WriteLine($"Successfully read {totalRead} bytes");
                return memoryStream.ToArray();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error reading file from content URI: {ex.GetType().Name} - {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"Stack trace: {ex.StackTrace}");
                return null;
            }
        }
#endif
        return await File.ReadAllBytesAsync(path);
    }
}
