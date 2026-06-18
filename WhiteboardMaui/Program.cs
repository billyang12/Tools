using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Hosting;

namespace WhiteboardMaui;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
            });
        return builder.Build();
    }
}

public static class Program
{
    public static int Main(string[] args)
    {
        // MAUI apps don't need explicit Main logic for iOS/macOS
        // but we need this for compilation
        return 0;
    }
}
