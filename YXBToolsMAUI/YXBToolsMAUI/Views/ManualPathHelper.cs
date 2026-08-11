namespace YXBToolsMAUI.Views;

public static class ManualPathHelper
{
    public static async Task<string?> PromptForPath(Page page, string title, string message, bool isFolder)
    {
        var result = await page.DisplayPromptAsync(
            title,
            message,
            placeholder: isFolder ? @"C:\path\to\folder" : @"C:\path\to\file.txt",
            maxLength: 500);

        if (string.IsNullOrWhiteSpace(result))
            return null;

        if (isFolder)
        {
            if (!Directory.Exists(result))
            {
                await page.DisplayAlert("Error", "The specified folder does not exist.", "OK");
                return null;
            }
        }
        else
        {
            if (!File.Exists(result))
            {
                await page.DisplayAlert("Error", "The specified file does not exist.", "OK");
                return null;
            }
        }

        return result;
    }
}
