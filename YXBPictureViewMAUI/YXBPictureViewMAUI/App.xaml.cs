using Microsoft.Extensions.DependencyInjection;
using YXBPictureViewMAUI.Services;

namespace YXBPictureViewMAUI
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();
        }

        protected override Window CreateWindow(IActivationState? activationState)
        {
            return new Window(new AppShell());
        }

        protected override void OnSleep()
        {
            base.OnSleep();
            // Clean up when app goes to background
            _ = CleanupTempFilesAsync(); // Fire and forget - don't block shutdown
        }

        protected override void OnResume()
        {
            base.OnResume();
        }

        private async Task CleanupTempFilesAsync()
        {
            try
            {
                // SECURITY: Securely erase all temporary decrypted video files
                var cacheDir = FileSystem.CacheDirectory;
                if (Directory.Exists(cacheDir))
                {
                    var tempFiles = Directory.GetFiles(cacheDir, "*.mp4");
                    if (tempFiles.Length > 0)
                    {
                        System.Diagnostics.Debug.WriteLine($"SECURITY: Found {tempFiles.Length} temp video files to securely erase");

                        // Use Quick method (1 pass) for fast cleanup during app shutdown
                        int erased = await SecureEraseHelper.SecureEraseFilesAsync(tempFiles, EraseMethod.Quick);

                        System.Diagnostics.Debug.WriteLine($"SECURITY: Securely erased {erased}/{tempFiles.Length} temp files");
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"SECURITY WARNING: Temp file cleanup error: {ex.Message}");
                // Don't crash the app during cleanup
            }
        }
    }
}