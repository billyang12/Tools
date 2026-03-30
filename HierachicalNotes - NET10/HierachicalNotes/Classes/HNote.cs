using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HierachicalNotes.Classes
{
    public class HNote
    {
        public string? Name { get; set; }
        public string? Password { get; set; }
        public string? Content { get; set; }
        public DateTime CreateTime { get; set; }
        public DateTime UpdateTime { get; set; }
        public List<HNote>? SubNodes { get; set; }
        public string BriefContent
        {
            get
            {
                string brief = "";
                if (Content == null)
                {
                    brief = "";
                }
                else
                {
                    brief = GetFirstLine(Content).Trim();
                    if (brief.Length > 100)
                    {
                        brief = brief.Substring(0, 97) + "...";
                    }
                }
                return brief;
            }
        }
        string GetFirstLine(string text)
        {
            if (string.IsNullOrEmpty(text)) return "";
            int len = text.Length;
            int i = 0;
            while (i < len && text[i] != '\r' && text[i] != '\n')
            {
                i++;
            }
            return text.Substring(0, i);
        }
        public string BriefName
        {
            get
            {
                string brief = Name;
                if (string.IsNullOrWhiteSpace(Name))
                {
                    brief = "[No name]";
                }
                else
                {
                    if (Name.Length > 100)
                    {
                        brief = Name.Substring(0, 97) + "...";
                    }
                }
                return brief;
            }
        }
        public string BriefSummary
        {
            get
            {
                string tmp = BriefContent;
                if (string.IsNullOrEmpty(tmp))
                    return BriefName;
                else
                    return $"{BriefName} : {BriefContent}";
            }
        }
        public void ShallowCopyFrom(HNote note)
        {
            Name = note.Name;
            Password = note.Password;
            Content = note.Content;
            CreateTime = note.CreateTime;
            UpdateTime = note.UpdateTime;
        }
    }
}
