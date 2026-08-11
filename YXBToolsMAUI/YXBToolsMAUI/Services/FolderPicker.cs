namespace YXBToolsMAUI.Services;

public static class FolderPicker
{
    public static async Task<FolderPickerResult> PickAsync(CancellationToken cancellationToken = default)
    {
#if ANDROID
        return await PickAndroidAsync(cancellationToken);
#elif IOS || MACCATALYST
        return await PickiOSAsync(cancellationToken);
#elif WINDOWS
        return await PickWindowsAsync(cancellationToken);
#else
        throw new PlatformNotSupportedException();
#endif
    }

#if ANDROID
    private static TaskCompletionSource<FolderPickerResult>? _folderPickerTcs;
    private static readonly object _lockObject = new object();

    private static async Task<FolderPickerResult> PickAndroidAsync(CancellationToken cancellationToken)
    {
        // Ensure only one picker operation at a time
        lock (_lockObject)
        {
            if (_folderPickerTcs != null && !_folderPickerTcs.Task.IsCompleted)
            {
                System.Diagnostics.Debug.WriteLine("Folder picker already in progress");
                return new FolderPickerResult { Folder = null };
            }
        }

        var status = await Permissions.RequestAsync<Permissions.StorageRead>();
        if (status != PermissionStatus.Granted)
        {
            System.Diagnostics.Debug.WriteLine("Storage permission not granted");
            return new FolderPickerResult { Folder = null };
        }

        try
        {
            lock (_lockObject)
            {
                _folderPickerTcs = new TaskCompletionSource<FolderPickerResult>();
            }

            var intent = new Android.Content.Intent(Android.Content.Intent.ActionOpenDocumentTree);
            intent.AddFlags(Android.Content.ActivityFlags.GrantReadUriPermission
                          | Android.Content.ActivityFlags.GrantWriteUriPermission
                          | Android.Content.ActivityFlags.GrantPersistableUriPermission);

            var currentActivity = Platform.CurrentActivity;
            if (currentActivity == null)
            {
                System.Diagnostics.Debug.WriteLine("CurrentActivity is null");
                lock (_lockObject)
                {
                    _folderPickerTcs?.TrySetResult(new FolderPickerResult { Folder = null });
                    _folderPickerTcs = null;
                }
                return new FolderPickerResult { Folder = null };
            }

            System.Diagnostics.Debug.WriteLine("Starting folder picker activity");
            currentActivity.StartActivityForResult(intent, 42);

            using (cancellationToken.Register(() =>
            {
                System.Diagnostics.Debug.WriteLine("Folder picker cancelled via token");
                _folderPickerTcs?.TrySetCanceled();
            }))
            {
                var result = await _folderPickerTcs.Task;
                System.Diagnostics.Debug.WriteLine($"Folder picker task completed: {result?.Folder?.Path ?? "null"}");
                return result;
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error picking folder: {ex.Message}");
            System.Diagnostics.Debug.WriteLine($"Stack trace: {ex.StackTrace}");
            lock (_lockObject)
            {
                _folderPickerTcs?.TrySetResult(new FolderPickerResult { Folder = null });
                _folderPickerTcs = null;
            }
            return new FolderPickerResult { Folder = null };
        }
    }

    public static void OnActivityResult(int requestCode, Android.App.Result resultCode, Android.Content.Intent? data)
    {
        System.Diagnostics.Debug.WriteLine($"OnActivityResult called: requestCode={requestCode}, resultCode={resultCode}");

        if (requestCode != 42)
        {
            System.Diagnostics.Debug.WriteLine($"OnActivityResult: Not our request code (expected 42, got {requestCode})");
            return;
        }

        TaskCompletionSource<FolderPickerResult>? tcs;
        lock (_lockObject)
        {
            tcs = _folderPickerTcs;
            if (tcs == null)
            {
                System.Diagnostics.Debug.WriteLine("OnActivityResult: TCS is null");
                return;
            }
        }

        if (tcs != null)
        {
            if (resultCode == Android.App.Result.Ok && data?.Data != null)
            {
                try
                {
                    var uri = data.Data;
                    System.Diagnostics.Debug.WriteLine($"Folder URI received: {uri}");

                    var currentActivity = Platform.CurrentActivity;
                    if (currentActivity == null)
                    {
                        System.Diagnostics.Debug.WriteLine("CurrentActivity is null");
                        _folderPickerTcs.SetResult(new FolderPickerResult { Folder = null });
                        _folderPickerTcs = null;
                        return;
                    }

                    if (uri == null)
                    {
                        System.Diagnostics.Debug.WriteLine("URI is null");
                        _folderPickerTcs.SetResult(new FolderPickerResult { Folder = null });
                        _folderPickerTcs = null;
                        return;
                    }

                    // Try to take persistable permissions
                    try
                    {
                        var takeFlags = Android.Content.ActivityFlags.GrantReadUriPermission
                                      | Android.Content.ActivityFlags.GrantWriteUriPermission;

                        var contentResolver = currentActivity.ContentResolver;
                        if (contentResolver != null)
                        {
                            contentResolver.TakePersistableUriPermission(uri, takeFlags);
                            System.Diagnostics.Debug.WriteLine("Persistable URI permissions taken successfully");
                        }
                        else
                        {
                            System.Diagnostics.Debug.WriteLine("ContentResolver is null, couldn't take persistable permissions");
                        }
                    }
                    catch (Exception permEx)
                    {
                        System.Diagnostics.Debug.WriteLine($"Could not take persistable permissions (this may be OK): {permEx.Message}");
                    }

                    var folderPath = uri.ToString();
                    System.Diagnostics.Debug.WriteLine($"Setting folder path: {folderPath}");

                    tcs.TrySetResult(new FolderPickerResult
                    {
                        Folder = new FolderInfo { Path = folderPath, Uri = uri.ToString() }
                    });

                    System.Diagnostics.Debug.WriteLine("Folder picker result set successfully");
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error processing folder selection: {ex.Message}");
                    System.Diagnostics.Debug.WriteLine($"Stack trace: {ex.StackTrace}");
                    tcs.TrySetResult(new FolderPickerResult { Folder = null });
                }
            }
            else
            {
                System.Diagnostics.Debug.WriteLine($"Folder selection cancelled or no data");
                tcs.TrySetResult(new FolderPickerResult { Folder = null });
            }

            lock (_lockObject)
            {
                _folderPickerTcs = null;
            }
        }
    }
#endif

#if IOS || MACCATALYST
    private static async Task<FolderPickerResult> PickiOSAsync(CancellationToken cancellationToken)
    {
        var documentsPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);

        return new FolderPickerResult
        {
            Folder = new FolderInfo { Path = documentsPath }
        };
    }
#endif

#if WINDOWS
    private static async Task<FolderPickerResult> PickWindowsAsync(CancellationToken cancellationToken)
    {
        try
        {
            // Use Win32 API for unpackaged apps
            var folderPickerService = new YXBToolsMAUI.Platforms.Windows.FolderPickerService();
            var folderPath = await folderPickerService.PickFolderAsync();

            if (!string.IsNullOrEmpty(folderPath))
            {
                return new FolderPickerResult
                {
                    Folder = new FolderInfo { Path = folderPath }
                };
            }

            return new FolderPickerResult { Folder = null };
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Windows folder picker error: {ex.Message}");
            return new FolderPickerResult { Folder = null };
        }
    }
#endif
}

public class FolderPickerResult
{
    public FolderInfo? Folder { get; set; }
}

public class FolderInfo
{
    public string Path { get; set; } = string.Empty;
    public string? Uri { get; set; }
}
