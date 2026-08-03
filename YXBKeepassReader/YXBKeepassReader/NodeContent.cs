using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YXBKeepassReader
{
    public class NodeContent
    {
        public EntryType EntryType { get; set; }
        public string Title { get; set; }
        public string UserName {  get; set; }
        public string Password {  get; set; }
        public string Url {  get; set; }
        public string Notes { get; set; }
        public DateTime Created { get; set; }
        public DateTime Updated { get; set; }

        public override string ToString()
        {
            string txt = Notes.Replace("\r\n", "\n");
            txt = Notes.Replace("\n\r", "\n");
            txt = txt.Replace("\r", "\n");
            txt = txt.Replace("\n", "\r\n");

            StringBuilder sb = new StringBuilder();
            sb.AppendLine($"TITLE: {Title}");
            sb.AppendLine();
            sb.AppendLine($"CREATED: {Created.ToString("yyyy-MM-dd hh:mm:ss")}, UPDATED: {Updated.ToString("yyyy-MM-dd hh:mm:ss")}");
            sb.AppendLine("==============================================================================================================");
            if(EntryType == EntryType.Entry) 
            {
                sb.AppendLine($"USERNAME: {UserName}");
                sb.AppendLine($"PASSWORD: {Password}");
                sb.AppendLine($"URL: {Url}");
                sb.AppendLine("==============================================================================================================");
            }
            sb.AppendLine($"NOTES:");
            sb.AppendLine($"{txt}");

            return sb.ToString();
        }
    }
}
