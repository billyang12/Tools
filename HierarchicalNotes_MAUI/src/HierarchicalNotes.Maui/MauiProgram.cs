using CommunityToolkit.Maui;
using HierarchicalNotes.Maui.ViewModels;
using HierarchicalNotes.Maui.Views;
using Microsoft.Maui.LifecycleEvents;
#if WINDOWS
using Microsoft.UI.Windowing;
#endif

namespace HierarchicalNotes.Maui;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .UseMauiCommunityToolkit();

#if WINDOWS
        builder.ConfigureLifecycleEvents(events =>
        {
            events.AddWindows(windows =>
            {
                windows.OnWindowCreated(window =>
                {
                    var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(window);
                    var windowId = Microsoft.UI.Win32Interop.GetWindowIdFromWindow(hwnd);
                    var appWindow = AppWindow.GetFromWindowId(windowId);
                    var closeConfirmed = false;

                    appWindow.Closing += async (_, args) =>
                    {
                        if (closeConfirmed)
                        {
                            return;
                        }

                        args.Cancel = true;

                        var notesPage = TryGetNotesPage();
                        if (notesPage == null)
                        {
                            closeConfirmed = true;
                            window.Close();
                            return;
                        }

                        var canClose = await MainThread.InvokeOnMainThreadAsync(notesPage.ConfirmCloseIfNeededAsync);
                        if (!canClose)
                        {
                            return;
                        }

                        closeConfirmed = true;
                        window.Close();
                    };
                });
            });
        });
#endif

        builder.Services.AddSingleton<MainViewModel>();
        builder.Services.AddSingleton<NotesPage>();
        builder.Services.AddSingleton<SearchPage>();

        return builder.Build();
    }

    private static NotesPage? TryGetNotesPage()
    {
        if (Application.Current?.MainPage is TabbedPage tabbedPage)
        {
            return tabbedPage.Children.OfType<NotesPage>().FirstOrDefault();
        }

        return Application.Current?.MainPage as NotesPage;
    }
}
