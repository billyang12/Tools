using Android.Content;

namespace YXBToolsMAUI.Platforms.Android;

/// <summary>
/// Android file picker that returns content URIs (not cache copies)
/// Use this for operations that need to work with the original file (like secure erase)
/// </summary>
public class AndroidFilePickerService
{
    private static TaskCompletionSource<string?>? _taskCompletionSource;

    public async Task<string?> PickFileAsync()
    {
        _taskCompletionSource = new TaskCompletionSource<string?>();

        try
        {
            // Use ACTION_OPEN_DOCUMENT to get a content URI
            var intent = new Intent(Intent.ActionOpenDocument);
            intent.AddCategory(Intent.CategoryOpenable);
            intent.SetType("*/*"); // All file types
            intent.AddFlags(ActivityFlags.GrantReadUriPermission | ActivityFlags.GrantWriteUriPermission);
            intent.AddFlags(ActivityFlags.GrantPersistableUriPermission);

            var activity = Platform.CurrentActivity;
            if (activity == null)
            {
                System.Diagnostics.Debug.WriteLine("AndroidFilePickerService: CurrentActivity is null");
                _taskCompletionSource.SetResult(null);
                return null;
            }

            activity.StartActivityForResult(intent, 1002); // Different request code from folder picker

            return await _taskCompletionSource.Task;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"AndroidFilePickerService error: {ex.Message}");
            _taskCompletionSource?.SetResult(null);
            return null;
        }
    }

    public static void OnActivityResultStatic(int requestCode, global::Android.App.Result resultCode, Intent? data)
    {
        if (requestCode == 1002)
        {
            if (resultCode == global::Android.App.Result.Ok && data?.Data != null)
            {
                var uri = data.Data;
                var activity = Platform.CurrentActivity;

                if (activity != null)
                {
                    try
                    {
                        // Take persistable permission so we can use this URI later
                        activity.ContentResolver?.TakePersistableUriPermission(
                            uri,
                            ActivityFlags.GrantReadUriPermission | ActivityFlags.GrantWriteUriPermission);

                        var uriString = uri.ToString();
                        System.Diagnostics.Debug.WriteLine($"AndroidFilePickerService: File selected: {uriString}");
                        _taskCompletionSource?.SetResult(uriString);
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"AndroidFilePickerService permission error: {ex.Message}");
                        // Even if persistable permission fails, still return the URI
                        _taskCompletionSource?.SetResult(uri.ToString());
                    }
                }
                else
                {
                    _taskCompletionSource?.SetResult(null);
                }
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("AndroidFilePickerService: File picker cancelled");
                _taskCompletionSource?.SetResult(null);
            }
        }
    }
}
