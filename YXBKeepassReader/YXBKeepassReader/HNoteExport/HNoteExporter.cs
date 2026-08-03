using System;
using System.IO;
using System.Text.Json;
using System.Windows.Forms;

namespace YXBKeepassReader.HNoteExport
{
    public class HNoteExporter
    {
        public static void ExportToHNF(TreeView treeView, string filePath, string password)
        {
            // Convert TreeView to HNoteCollection
            var hNoteCollection = new HNoteCollection();

            foreach (TreeNode rootNode in treeView.Nodes)
            {
                var hNote = ConvertTreeNodeToHNote(rootNode);
                if (hNote != null)
                {
                    hNoteCollection.RootNodes.Add(hNote);
                }
            }

            // Create HNoteFile
            var hNoteFile = new HNoteFile();
            hNoteFile.IsEncrypted = !string.IsNullOrEmpty(password);

            if (hNoteFile.IsEncrypted)
            {
                // Serialize and encrypt
                string jsonContent = JsonSerializer.Serialize(hNoteCollection);
                hNoteFile.HNoteJsonStr = StringCipher.Encrypt(jsonContent, password);
                hNoteFile.Notes = null;
            }
            else
            {
                // Store unencrypted
                hNoteFile.Notes = hNoteCollection;
                hNoteFile.HNoteJsonStr = null;
            }

            // Serialize the HNoteFile to JSON
            JsonSerializerOptions options = new JsonSerializerOptions
            {
                WriteIndented = true
            };
            string finalJson = JsonSerializer.Serialize(hNoteFile, options);

            // Write to file
            File.WriteAllText(filePath, finalJson);
        }

        private static HNote? ConvertTreeNodeToHNote(TreeNode treeNode)
        {
            if (treeNode == null) return null;

            NodeContent? nodeContent = treeNode.Tag as NodeContent;
            if (nodeContent == null) return null;

            var hNote = new HNote
            {
                Name = nodeContent.Title ?? treeNode.Text,
                CreateTime = nodeContent.Created,
                UpdateTime = nodeContent.Updated
            };

            // Build content from KeePass entry
            if (nodeContent.EntryType == EntryType.Entry)
            {
                var contentLines = new System.Collections.Generic.List<string>();

                if (!string.IsNullOrEmpty(nodeContent.UserName))
                    contentLines.Add($"Username: {nodeContent.UserName}");

                if (!string.IsNullOrEmpty(nodeContent.Url))
                    contentLines.Add($"URL: {nodeContent.Url}");

                if (!string.IsNullOrEmpty(nodeContent.Notes))
                {
                    contentLines.Add("");
                    contentLines.Add("Notes:");
                    contentLines.Add(nodeContent.Notes);
                }

                hNote.Content = string.Join(Environment.NewLine, contentLines);
                hNote.Password = nodeContent.Password;
                hNote.Keywords = !string.IsNullOrEmpty(nodeContent.Url) ? nodeContent.Url : null;
            }
            else
            {
                // For groups, just use notes as content
                hNote.Content = nodeContent.Notes;
            }

            // Convert child nodes
            if (treeNode.Nodes.Count > 0)
            {
                hNote.SubNodes = new System.Collections.Generic.List<HNote>();
                foreach (TreeNode childNode in treeNode.Nodes)
                {
                    var childHNote = ConvertTreeNodeToHNote(childNode);
                    if (childHNote != null)
                    {
                        hNote.SubNodes.Add(childHNote);
                    }
                }
            }

            return hNote;
        }
    }
}
