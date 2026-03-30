using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Windows.Controls;
using System.Windows.Navigation;

namespace HierachicalNotes.Classes
{
    public class HTMLExporter
    {
        public static string ExportToHTML(TreeView treeView)
        {
            string htmlTop = GetHTMLTop();
            string htmlBottom = GetHTMLBottom();

            StringBuilder sb = new StringBuilder();
            sb.AppendLine(htmlTop);
            sb.AppendLine(GenHTMLForItemCollection(treeView.Items));
            sb.AppendLine(htmlBottom);

            return sb.ToString();
        }
        static string GenHTMLForItemCollection(ItemCollection items, string noteContent = null, bool isNested = false)
        {
            StringBuilder sb = new StringBuilder();
            if(!isNested)
            {
                sb.AppendLine("<ul id=\"myUL\">");
            }
            else
            {
                sb.AppendLine("<ul class=\"nested\">");
            }
            if(isNested && !string.IsNullOrEmpty(noteContent))
            {
                sb.AppendLine(noteContent);
            }
            foreach (TreeViewItem item in items)
            {
                sb.AppendLine(GenHTMLForTreeNode(item));
            }
            sb.AppendLine("</ul>");
            return sb.ToString();
        }

        static string GenHTMLForTreeNode(TreeViewItem item)
        {
            if(item.Items.Count == 0)
            {
                return GenHTMLForLeafTreeNode(item);
            }
            else
            {
                return GenHTMLForNonLeafTreeNode(item);
            }
        }
        static string GenHTMLForNodeShallow(HNote note)
        {
            if (note == null) return "";

            StringBuilder sb = new StringBuilder();
            sb.AppendLine("<div>");
            sb.AppendLine(@"<span class=""note-created"">");
            sb.Append("Created: ");
            sb.Append(note.CreateTime.ToString("yyyy-MM-dd hh:mm:ss"));
            sb.AppendLine("</span>");

            sb.AppendLine(@"<span class=""note-updated"">");
            sb.Append("      Updated: ");
            sb.Append(note.UpdateTime.ToString("yyyy-MM-dd hh:mm:ss"));
            sb.AppendLine("</span>");

            string content = note.Content?.Trim().Replace('<', '(').Replace('>', ')');
            if (!string.IsNullOrEmpty(content))
            {
                sb.AppendLine(@"<div class=""comment-content"">");
                sb.AppendLine("<pre>");
                sb.AppendLine(content);
                sb.AppendLine("</pre>");
                sb.AppendLine("</div>");
            }
            sb.AppendLine("</div>");
            return sb.ToString();
        }
        static string GenHTMLForLeafTreeNode(TreeViewItem item)
        {
            if (item == null) return "";
            HNote note = item.Tag as HNote;
            if(note == null) return "";

            StringBuilder sb = new StringBuilder();
            sb.AppendLine("<li><span class=\"caret note-name\">");
            sb.AppendLine(WebUtility.HtmlEncode(note.Name));
            sb.AppendLine("</span>");

            sb.AppendLine("<ul class=\"nested\">");
            sb.AppendLine(GenHTMLForNodeShallow(note));
            sb.AppendLine("</ul>");
            sb.AppendLine("</li>");
            return sb.ToString();
        }
        static string GenHTMLForNonLeafTreeNode(TreeViewItem item)
        {
            if (item == null) return "";
            HNote note = item.Tag as HNote;
            if (note == null) return "";

            StringBuilder sb = new StringBuilder();
            sb.AppendLine("<li><span class=\"caret note-name\">");
            sb.AppendLine(WebUtility.HtmlEncode(note.Name));
            sb.AppendLine("</span>");

            sb.AppendLine(GenHTMLForItemCollection(item.Items, GenHTMLForNodeShallow(note), true));

            sb.AppendLine("</li>");
            return sb.ToString();
        }
        static string GetHTMLTop()
        {
            string htmlTop = @"
<!DOCTYPE html>
<html>
<head>
<meta name=""viewport"" content=""width=device-width, initial-scale=1"">
<style>
ul, #myUL {
  list-style-type: none;
}

#myUL {
  margin: 0;
  padding: 0;
}

.caret {
  cursor: pointer;
  -webkit-user-select: none; /* Safari 3.1+ */
  -moz-user-select: none; /* Firefox 2+ */
  -ms-user-select: none; /* IE 10+ */
  user-select: none;
}

.caret::before {
  content: ""\25B6"";
  color: black;
  display: inline-block;
  margin-right: 6px;
}

.caret-down::before {
  -ms-transform: rotate(90deg); /* IE 9 */
  -webkit-transform: rotate(90deg); /* Safari */'
  transform: rotate(90deg);  
}

.nested {
  display: none;
}

.active {
  display: block;
}

.comment-content {
   padding: 5px 5px 5px 20px;
}

</style>
</head>
<body>

                ";
            return htmlTop;
        }

        static string GetHTMLBottom()
        {
            string htmlBottom = @"

<script>
var toggler = document.getElementsByClassName(""caret"");
var i;

for (i = 0; i < toggler.length; i++) {
  toggler[i].addEventListener(""click"", function() {
    this.parentElement.querySelector("".nested"").classList.toggle(""active"");
    this.classList.toggle(""caret-down"");
  });
}
</script>

</body>
</html>

";
            return htmlBottom;
        }
    }
}
