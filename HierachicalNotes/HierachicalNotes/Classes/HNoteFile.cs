using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
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
        string? latestNotesHash = null;
        public bool IsEncrypted { get; set; }
        public string? HNoteJsonStr { get; set; }
        public HNoteCollection? Notes { get; set; }
        public HNoteFile()
        {
            IsEncrypted = false;
        }
        public void SaveLatestNotesJson()
        {
            latestNotesHash = ComputeContentHash();
        }
        public bool IsChangedSinceLastSave()
        {
            string? currentHash = ComputeContentHash();
            return currentHash != latestNotesHash;
        }

        private string? ComputeContentHash()
        {
            if (Notes == null && string.IsNullOrEmpty(HNoteJsonStr))
                return null;

            if (IsEncrypted)
            {
                return ComputeHash(HNoteJsonStr);
            }
            else
            {
                if (Notes == null) return null;

                JsonSerializerOptions options = new JsonSerializerOptions
                {
                    WriteIndented = false,
                    DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.Never,
                    PropertyNamingPolicy = null
                };
                string json = JsonSerializer.Serialize(Notes, options);
                return ComputeHash(json);
            }
        }

        private string? ComputeHash(string? input)
        {
            if (string.IsNullOrEmpty(input))
                return null;

            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] bytes = Encoding.UTF8.GetBytes(input);
                byte[] hash = sha256.ComputeHash(bytes);
                return Convert.ToBase64String(hash);
            }
        }
    }
}
