using System;
using System.Collections.Generic;

namespace YXBKeepassReader.HNoteExport
{
    public class HNote
    {
        public string? Name { get; set; }
        public string? Password { get; set; }
        public string? Keywords { get; set; }
        public string? Content { get; set; }
        public DateTime CreateTime { get; set; }
        public DateTime UpdateTime { get; set; }
        public List<HNote>? SubNodes { get; set; }
    }
}
