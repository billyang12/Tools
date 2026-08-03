using System.Collections.Generic;

namespace YXBKeepassReader.HNoteExport
{
    public class HNoteCollection
    {
        public List<HNote> RootNodes { get; set; }

        public HNoteCollection()
        {
            RootNodes = new List<HNote>();
        }
    }
}
