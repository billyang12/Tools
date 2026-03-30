using HierarchicalNotes.Maui.Views;

namespace HierarchicalNotes.Maui;

public partial class App : Application
{
    public App(NotesPage notesPage, SearchPage searchPage)
    {
        InitializeComponent();

        MainPage = new TabbedPage
        {
            Children =
            {
                new NavigationPage(notesPage)
                {
                    Title = "Notes",
                    BarBackgroundColor = Color.FromArgb("#F2F2F2"),
                    BarTextColor = Color.FromArgb("#202020")
                },
                new NavigationPage(searchPage)
                {
                    Title = "Search",
                    BarBackgroundColor = Color.FromArgb("#F2F2F2"),
                    BarTextColor = Color.FromArgb("#202020")
                }
            }
        };
    }
}
