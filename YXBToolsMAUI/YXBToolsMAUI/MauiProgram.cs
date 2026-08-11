using Microsoft.Extensions.Logging;
using YXBToolsMAUI.Services;
using YXBToolsMAUI.Views;

namespace YXBToolsMAUI
{
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
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                });

            builder.Services.AddSingleton<IEncryptionService, EncryptionService>();
            builder.Services.AddSingleton<IEraseService, EraseService>();

            builder.Services.AddTransient<EncryptionPage>();
            builder.Services.AddTransient<DecryptionPage>();
            builder.Services.AddTransient<ErasePage>();
            builder.Services.AddTransient<AboutPage>();

#if DEBUG
    		builder.Logging.AddDebug();
#endif

            return builder.Build();
        }
    }
}
