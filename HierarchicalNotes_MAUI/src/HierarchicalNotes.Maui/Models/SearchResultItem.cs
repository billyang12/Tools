using HierarchicalNotes.Maui.ViewModels;

namespace HierarchicalNotes.Maui.Models;

public sealed class SearchResultItem
{
    public required NoteNodeViewModel Node { get; init; }
    public string Path => Node.Path;
    public string Summary => Node.DisplaySummary;
}
