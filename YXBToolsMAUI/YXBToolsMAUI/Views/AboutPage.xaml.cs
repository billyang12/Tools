using System.Reflection;

namespace YXBToolsMAUI.Views;

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
            // Get version from assembly
            var assembly = Assembly.GetExecutingAssembly();
            var version = assembly.GetName().Version;

            if (version != null)
            {
                VersionLabel.Text = $"Version {version.Major}.{version.Minor}.{version.Build}";
            }

            // Update copyright with current year
            var currentYear = DateTime.Now.Year;
            CopyrightLabel.Text = $"© {currentYear} Bill Yang. All rights reserved.";
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error loading version info: {ex.Message}");
            VersionLabel.Text = "Version 1.0.0";
        }
    }
}
