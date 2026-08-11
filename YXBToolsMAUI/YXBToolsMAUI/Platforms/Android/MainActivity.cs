using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;

namespace YXBToolsMAUI
{
    [Activity(Theme = "@style/Maui.SplashTheme", MainLauncher = true, LaunchMode = LaunchMode.SingleTop, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize | ConfigChanges.Density)]
    public class MainActivity : MauiAppCompatActivity
    {
        protected override void OnActivityResult(int requestCode, Result resultCode, Intent? data)
        {
            base.OnActivityResult(requestCode, resultCode, data);

            // Handle folder picker (request code 42)
            if (requestCode == 42)
            {
                Services.FolderPicker.OnActivityResult(requestCode, resultCode, data);
            }
            // Handle folder picker (request code 1001)
            else if (requestCode == 1001)
            {
                YXBToolsMAUI.Platforms.Android.FolderPickerService.OnActivityResultStatic(requestCode, resultCode, data);
            }
            // Handle file picker (request code 1002)
            else if (requestCode == 1002)
            {
                YXBToolsMAUI.Platforms.Android.AndroidFilePickerService.OnActivityResultStatic(requestCode, resultCode, data);
            }
        }
    }
}
