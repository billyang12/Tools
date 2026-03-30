using System.Text.Json;

namespace HierarchicalNotes.Core.Models;

public class HNoteFile
{
    [System.Text.Json.Serialization.JsonIgnore]
    private string? latestNotesJson;

    public bool IsEncrypted { get; set; }
    public string? HNoteJsonStr { get; set; }
    public HNoteCollection? Notes { get; set; }

    public void SaveLatestNotesJson()
    {
        latestNotesJson = GetNotesJson();
    }

    public bool IsChangedSinceLastSave()
    {
        return GetNotesJson() != latestNotesJson;
    }

    private string? GetNotesJson()
    {
        if (IsEncrypted)
        {
            return HNoteJsonStr;
        }

        if (Notes == null)
        {
            return null;
        }

        return JsonSerializer.Serialize(Notes, new JsonSerializerOptions
        {
            WriteIndented = true
        });
    }
}
