using System.Net;
using System.Text;
using HierarchicalNotes.Core.Models;

namespace HierarchicalNotes.Core.Services;

public static class HtmlExporter
{
    public static string ExportToHtml(IEnumerable<HNote> rootNodes)
    {
        var sb = new StringBuilder();
        sb.AppendLine(GetHtmlTop());
        sb.AppendLine(GenerateNodeList(rootNodes, "myUL"));
        sb.AppendLine(GetHtmlBottom());
        return sb.ToString();
    }

    public static string ExportSingleNote(HNote note)
    {
        return ExportToHtml(new[] { note });
    }

    private static string GenerateNodeList(IEnumerable<HNote> notes, string listId, bool nested = false)
    {
        var sb = new StringBuilder();
        sb.AppendLine(nested ? "<ul class=\"nested\">" : $"<ul id=\"{listId}\">");

        foreach (var note in notes)
        {
            sb.AppendLine(GenerateNode(note));
        }

        sb.AppendLine("</ul>");
        return sb.ToString();
    }

    private static string GenerateNode(HNote note)
    {
        var sb = new StringBuilder();
        var hasChildren = note.SubNodes.Count > 0;
        sb.AppendLine("<li>");
        sb.AppendLine($"<span class=\"caret note-name{(hasChildren ? string.Empty : " leaf")}\">{WebUtility.HtmlEncode(note.BriefName)}</span>");
        sb.AppendLine(GenerateNoteBody(note));

        if (hasChildren)
        {
            sb.AppendLine(GenerateNodeList(note.SubNodes, "nested", true));
        }

        sb.AppendLine("</li>");
        return sb.ToString();
    }

    private static string GenerateNoteBody(HNote note)
    {
        var sb = new StringBuilder();
        sb.AppendLine("<div class=\"note-meta\">");
        sb.AppendLine($"<span class=\"note-created\">Created: {note.CreateTime:yyyy-MM-dd HH:mm:ss}</span>");
        sb.AppendLine($"<span class=\"note-updated\">Updated: {note.UpdateTime:yyyy-MM-dd HH:mm:ss}</span>");
        if (!string.IsNullOrWhiteSpace(note.Password))
        {
            sb.AppendLine("<span class=\"note-password\">Password: set</span>");
        }
        sb.AppendLine("</div>");

        var content = (note.Content ?? string.Empty).Trim().Replace("<", "(").Replace(">", ")");
        if (!string.IsNullOrEmpty(content))
        {
            sb.AppendLine("<div class=\"comment-content\"><pre>");
            sb.AppendLine(WebUtility.HtmlEncode(content));
            sb.AppendLine("</pre></div>");
        }

        return sb.ToString();
    }

    private static string GetHtmlTop() => """
<!DOCTYPE html>
<html>
<head>
<meta name="viewport" content="width=device-width, initial-scale=1">
<style>
body { font-family: system-ui, sans-serif; margin: 0; padding: 12px; }
ul, #myUL { list-style-type: none; margin: 0; padding: 0; }
.caret { cursor: pointer; user-select: none; font-weight: 600; }
.caret::before { content: "\25B6"; color: black; display: inline-block; margin-right: 6px; }
.caret-down::before { transform: rotate(90deg); }
.nested { display: none; padding-left: 18px; }
.active { display: block; }
.comment-content { padding: 5px 5px 5px 20px; }
.note-meta { color: #666; font-size: 0.9em; display: flex; gap: 12px; flex-wrap: wrap; padding-left: 20px; }
.note-name.leaf::before { content: "\2022"; color: #666; margin-right: 6px; }
pre { white-space: pre-wrap; word-break: break-word; }
</style>
</head>
<body>
""";

    private static string GetHtmlBottom() => """
<script>
var toggler = document.getElementsByClassName('caret');
for (var i = 0; i < toggler.length; i++) {
  toggler[i].addEventListener('click', function() {
    var nested = this.parentElement.querySelector('.nested');
    if (nested) { nested.classList.toggle('active'); }
    this.classList.toggle('caret-down');
  });
}
</script>
</body>
</html>
""";
}
