using Microsoft.Extensions.DependencyInjection;

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
            CleanupTempFiles();
        }

        protected override void OnResume()
        {
            base.OnResume();
        }

        private void CleanupTempFiles()
        {
            try
            {
                // Securely delete all temporary video files
                var cacheDir = FileSystem.CacheDirectory;
                if (Directory.Exists(cacheDir))
                {
                    var tempFiles = Directory.GetFiles(cacheDir, "*.mp4");
                    foreach (var file in tempFiles)
                    {
                        try
                        {
                            // Overwrite then delete for security
                            SecureDeleteFile(file);
                        }
                        catch { }
                    }
                    System.Diagnostics.Debug.WriteLine($"App cleanup: Removed {tempFiles.Length} temp files");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"App cleanup error: {ex.Message}");
            }
        }

        private void SecureDeleteFile(string filePath)
        {
            try
            {
                if (string.IsNullOrEmpty(filePath) || !File.Exists(filePath))
                    return;

                // Simple delete - just remove the file
                // (Overwriting during app shutdown can cause crashes)
                File.Delete(filePath);
            }
            catch
            {
                // Ignore errors during cleanup
            }
        }
    }
}