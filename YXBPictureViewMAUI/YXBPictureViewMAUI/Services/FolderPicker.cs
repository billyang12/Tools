namespace YXBPictureViewMAUI.Services;

public static class FolderPicker
{
    public static async Task<FolderPickerResult> PickAsync(CancellationToken cancellationToken)
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

    private static async Task<FolderPickerResult> PickAndroidAsync(CancellationToken cancellationToken)
    {
        var status = await Permissions.RequestAsync<Permissions.StorageRead>();
        if (status != PermissionStatus.Granted)
        {
            return new FolderPickerResult { Folder = null };
        }

        try
        {
            _folderPickerTcs = new TaskCompletionSource<FolderPickerResult>();

            var intent = new Android.Content.Intent(Android.Content.Intent.ActionOpenDocumentTree);
            intent.AddFlags(Android.Content.ActivityFlags.GrantReadUriPermission
                          | Android.Content.ActivityFlags.GrantPersistableUriPermission);

            var currentActivity = Platform.CurrentActivity;
            if (currentActivity == null)
            {
                return new FolderPickerResult { Folder = null };
            }

            // Register activity result handler
            currentActivity.StartActivityForResult(intent, 42);

            using (cancellationToken.Register(() => _folderPickerTcs?.TrySetCanceled()))
            {
                return await _folderPickerTcs.Task;
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error picking folder: {ex.Message}");
            return new FolderPickerResult { Folder = null };
        }
    }

    public static void OnActivityResult(int requestCode, Android.App.Result resultCode, Android.Content.Intent? data)
    {
        if (requestCode == 42 && _folderPickerTcs != null)
        {
            if (resultCode == Android.App.Result.Ok && data?.Data != null)
            {
                try
                {
                    var uri = data.Data;
                    var currentActivity = Platform.CurrentActivity;

                    if (currentActivity != null && uri != null)
                    {
                        // Take persistable permission
                        var takeFlags = Android.Content.ActivityFlags.GrantReadUriPermission;
                        currentActivity.ContentResolver?.TakePersistableUriPermission(uri, takeFlags);

                        // Convert content:// URI to a usable path
                        var folderPath = GetPathFromUri(uri);

                        _folderPickerTcs.SetResult(new FolderPickerResult
                        {
                            Folder = new FolderInfo { Path = folderPath, Uri = uri.ToString() }
                        });
                    }
                    else
                    {
                        _folderPickerTcs.SetResult(new FolderPickerResult { Folder = null });
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error processing folder selection: {ex.Message}");
                    _folderPickerTcs.SetResult(new FolderPickerResult { Folder = null });
                }
            }
            else
            {
                _folderPickerTcs.SetResult(new FolderPickerResult { Folder = null });
            }

            _folderPickerTcs = null;
        }
    }

    private static string GetPathFromUri(Android.Net.Uri uri)
    {
        // For Android SAF, we must use the content:// URI directly
        // We cannot convert it to a file path as those are not accessible with scoped storage
        return uri.ToString();
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
        var picker = new Windows.Storage.Pickers.FolderPicker();
        picker.FileTypeFilter.Add("*");

        var hwnd = ((MauiWinUIWindow)Application.Current.Windows[0].Handler.PlatformView).WindowHandle;
        WinRT.Interop.InitializeWithWindow.Initialize(picker, hwnd);

        var folder = await picker.PickSingleFolderAsync();

        if (folder != null)
        {
            return new FolderPickerResult
            {
                Folder = new FolderInfo { Path = folder.Path }
            };
        }

        return new FolderPickerResult { Folder = null };
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
