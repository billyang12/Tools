using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HierachicalNotes.Classes
{
    public class YListItem
    {
        public string? Text { get; set; }
        public int Id { get; set; }
        public object? Tag { get; set; }
        public YListItem()
        {
            Id = 0;
            Text = "";
            Tag = null;
        }
        public YListItem(string txt)
        {
            Id = 0;
            Text = txt;
            Tag = null;
        }
        public YListItem(string txt, int id)
        {
            Id = id;
            Text = txt;
            Tag = null;
        }
        public YListItem(string txt, object tag)
        {
            Id = 0;
            Text = txt;
            Tag = tag;
        }
        public YListItem(string txt, int id, object tag)
        {
            Id = id;
            Text = txt;
            Tag = tag;
        }
        public override string ToString()
        {
            return Text==null?"":Text.Trim();
        }
    }
}
