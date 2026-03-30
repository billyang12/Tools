using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media;
using System.Xml.Linq;

namespace HierachicalNotes.Classes
{
    public class HNoteFile
    {
        string? latestNotesJson = null;
        public bool IsEncrypted { get; set; }
        public string? HNoteJsonStr { get; set; }
        public HNoteCollection? Notes { get; set; }
        public HNoteFile()
        {
            IsEncrypted = false;
        }
        public void SaveLatestNotesJson()
        {
            latestNotesJson = GetNotesJson();
        }
        public bool IsChangedSinceLastSave()
        {
            string? tmp = GetNotesJson();
            if (tmp == latestNotesJson) return false;
            return true;
        }
        string? GetNotesJson()
        {
            if (IsEncrypted)
            {
                return HNoteJsonStr;

            }
            else
            {
                if (Notes == null) return null;
                JsonSerializerOptions options = new JsonSerializerOptions();
                options.WriteIndented = true;
                string json = JsonSerializer.Serialize(Notes, options);
                return json; ;
            }
        }
    }
}
