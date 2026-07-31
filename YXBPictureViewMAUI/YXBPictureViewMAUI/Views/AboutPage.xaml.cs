namespace YXBPictureViewMAUI.Views;

public partial class AboutPage : ContentPage
{
    public AboutPage()
    {
        InitializeComponent();
        LoadVersionInfo();
    }

    private void LoadVersionInfo()
    {
        try
        {
            // Get app version from AppInfo
            var version = AppInfo.Current.VersionString;
            lblVersion.Text = $"Version {version}";

            // Get current year for copyright
            var currentYear = DateTime.Now.Year;
            lblCopyright.Text = $"© {currentYear} Bill Yang. All rights reserved.";
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error loading version info: {ex.Message}");
        }
    }
}
