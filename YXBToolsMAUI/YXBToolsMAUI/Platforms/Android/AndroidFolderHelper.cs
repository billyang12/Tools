namespace YXBToolsMAUI.Platforms.Android;

public static class AndroidFolderHelper
{
    public static string GetOutputFolderPath(string? uriString)
    {
        if (string.IsNullOrEmpty(uriString))
        {
            return GetDefaultOutputFolder();
        }

        if (uriString.StartsWith("content://"))
        {
            return GetDefaultOutputFolder();
        }

        if (Directory.Exists(uriString))
        {
            return uriString;
        }

        return GetDefaultOutputFolder();
    }

    public static string GetDefaultOutputFolder()
    {
        var documentsPath = System.Environment.GetFolderPath(System.Environment.SpecialFolder.MyDocuments);
        var yxbFolder = Path.Combine(documentsPath, "YXBTools");

        if (!Directory.Exists(yxbFolder))
        {
            Directory.CreateDirectory(yxbFolder);
        }

        return yxbFolder;
    }

    public static string GetAppStoragePath()
    {
        var externalStorageState = global::Android.OS.Environment.ExternalStorageState;

        if (externalStorageState == global::Android.OS.Environment.MediaMounted)
        {
            var externalPath = global::Android.OS.Environment.GetExternalStoragePublicDirectory(global::Android.OS.Environment.DirectoryDocuments)?.AbsolutePath;
            if (!string.IsNullOrEmpty(externalPath))
            {
                var appFolder = Path.Combine(externalPath, "YXBTools");
                if (!Directory.Exists(appFolder))
                {
                    Directory.CreateDirectory(appFolder);
                }
                return appFolder;
            }
        }

        return GetDefaultOutputFolder();
    }
}
