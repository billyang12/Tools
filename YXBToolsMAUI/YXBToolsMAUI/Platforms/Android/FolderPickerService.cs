using Android.Content;
using YXBToolsMAUI.Services;

namespace YXBToolsMAUI.Platforms.Android;

public class FolderPickerService : IFolderPickerService
{
    private static TaskCompletionSource<string?>? _taskCompletionSource;

    public async Task<string?> PickFolderAsync()
    {
        _taskCompletionSource = new TaskCompletionSource<string?>();

        try
        {
            var intent = new Intent(Intent.ActionOpenDocumentTree);
            intent.AddFlags(ActivityFlags.GrantReadUriPermission | ActivityFlags.GrantWriteUriPermission);
            intent.AddFlags(ActivityFlags.GrantPersistableUriPermission);

            var activity = Platform.CurrentActivity;
            if (activity == null)
            {
                _taskCompletionSource.SetResult(null);
                return null;
            }

            activity.StartActivityForResult(intent, 1001);

            return await _taskCompletionSource.Task;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Android FolderPicker error: {ex.Message}");
            _taskCompletionSource?.SetResult(null);
            return null;
        }
    }

    public static void OnActivityResultStatic(int requestCode, global::Android.App.Result resultCode, Intent? data)
    {
        if (requestCode == 1001)
        {
            if (resultCode == global::Android.App.Result.Ok && data?.Data != null)
            {
                var uri = data.Data;
                var activity = Platform.CurrentActivity;

                if (activity != null)
                {
                    try
                    {
                        activity.ContentResolver?.TakePersistableUriPermission(
                            uri,
                            ActivityFlags.GrantReadUriPermission | ActivityFlags.GrantWriteUriPermission);

                        var uriString = uri.ToString();
                        System.Diagnostics.Debug.WriteLine($"Android folder selected: {uriString}");
                        _taskCompletionSource?.SetResult(uriString);
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"Android permission error: {ex.Message}");
                        _taskCompletionSource?.SetResult(null);
                    }
                }
                else
                {
                    _taskCompletionSource?.SetResult(null);
                }
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("Android folder picker cancelled");
                _taskCompletionSource?.SetResult(null);
            }
        }
    }
}
