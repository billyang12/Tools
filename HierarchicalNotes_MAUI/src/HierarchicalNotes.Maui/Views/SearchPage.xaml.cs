using HierarchicalNotes.Maui.Models;
using HierarchicalNotes.Maui.ViewModels;

namespace HierarchicalNotes.Maui.Views;

public partial class SearchPage : ContentPage
{
    private readonly MainViewModel _vm;

    public SearchPage(MainViewModel vm)
    {
        InitializeComponent();
        _vm = vm;
        BindingContext = _vm;
    }

    private void ResultsCollection_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (e.CurrentSelection.FirstOrDefault() is SearchResultItem item)
        {
            _vm.SelectNode(item.Node);
        }
    }
}
