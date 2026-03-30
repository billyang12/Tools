using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HierachicalNotes.Classes
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
