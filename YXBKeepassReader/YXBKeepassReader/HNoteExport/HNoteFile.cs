namespace YXBKeepassReader.HNoteExport
{
    public class HNoteFile
    {
        public bool IsEncrypted { get; set; }
        public string? HNoteJsonStr { get; set; }
        public HNoteCollection? Notes { get; set; }

        public HNoteFile()
        {
            IsEncrypted = false;
        }
    }
}
