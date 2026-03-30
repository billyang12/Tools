namespace HierarchicalNotes.Core.Models;

public class YListItem
{
    public string? Text { get; set; }
    public object? Tag { get; set; }

    public override string ToString() => Text ?? string.Empty;
}
