using System.Text.Json.Serialization;

namespace HierarchicalNotes.Core.Models;

public class HNote
{
    public string? Name { get; set; }
    public string? Password { get; set; }
    public string? Content { get; set; }
    public DateTime CreateTime { get; set; }
    public DateTime UpdateTime { get; set; }
    public List<HNote> SubNodes { get; set; } = new();

    [JsonIgnore]
    public string BriefContent
    {
        get
        {
            if (string.IsNullOrEmpty(Content))
            {
                return string.Empty;
            }

            var brief = GetFirstLine(Content).Trim();
            return brief.Length > 100 ? brief[..97] + "..." : brief;
        }
    }

    [JsonIgnore]
    public string BriefName
    {
        get
        {
            if (string.IsNullOrWhiteSpace(Name))
            {
                return "[No name]";
            }

            return Name.Length > 100 ? Name[..97] + "..." : Name;
        }
    }

    [JsonIgnore]
    public string BriefSummary => string.IsNullOrEmpty(BriefContent) ? BriefName : $"{BriefName} : {BriefContent}";

    public void ShallowCopyFrom(HNote note)
    {
        Name = note.Name;
        Password = note.Password;
        Content = note.Content;
        CreateTime = note.CreateTime;
        UpdateTime = note.UpdateTime;
    }

    public HNote DeepClone()
    {
        var clone = new HNote();
        clone.ShallowCopyFrom(this);
        clone.SubNodes = SubNodes?.Select(x => x.DeepClone()).ToList() ?? new List<HNote>();
        return clone;
    }

    private static string GetFirstLine(string text)
    {
        if (string.IsNullOrEmpty(text))
        {
            return string.Empty;
        }

        var i = 0;
        while (i < text.Length && text[i] != '\r' && text[i] != '\n')
        {
            i++;
        }
        return text[..i];
    }
}
