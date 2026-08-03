using KeePassLib;
using System;
using System.Collections.Generic;
using System.Xml;

namespace YXBKeepassReader
{
    /// <summary>
    /// Reads KeePass XML export files
    /// </summary>
    public class KeePassXmlReader
    {
        public class XmlEntry
        {
            public string Title { get; set; }
            public string UserName { get; set; }
            public string Password { get; set; }
            public string Url { get; set; }
            public string Notes { get; set; }
            public DateTime CreationTime { get; set; }
            public DateTime LastModificationTime { get; set; }
        }

        public class XmlGroup
        {
            public string Name { get; set; }
            public string Notes { get; set; }
            public DateTime CreationTime { get; set; }
            public DateTime LastModificationTime { get; set; }
            public List<XmlEntry> Entries { get; set; }
            public List<XmlGroup> Groups { get; set; }

            public XmlGroup()
            {
                Entries = new List<XmlEntry>();
                Groups = new List<XmlGroup>();
                CreationTime = DateTime.Now;
                LastModificationTime = DateTime.Now;
            }
        }

        /// <summary>
        /// Loads a KeePass XML export file
        /// </summary>
        public static XmlGroup LoadFromXml(string filePath)
        {
            XmlDocument doc = new XmlDocument();
            try
            {
                doc.Load(filePath);
            }
            catch (Exception ex)
            {
                throw new Exception("Failed to load XML file: " + ex.Message, ex);
            }

            XmlNode rootNode = doc.SelectSingleNode("//Root");
            if (rootNode == null)
            {
                rootNode = doc.SelectSingleNode("//KeePassFile");
            }

            if (rootNode == null)
            {
                throw new Exception("Invalid KeePass XML format - Root or KeePassFile node not found");
            }

            XmlNode groupNode = rootNode.SelectSingleNode("Group");
            if (groupNode == null)
            {
                throw new Exception("Invalid KeePass XML format - no Group node found");
            }

            return ParseGroup(groupNode);
        }

        private static XmlGroup ParseGroup(XmlNode groupNode)
        {
            XmlGroup group = new XmlGroup();

            // Parse group properties
            XmlNode nameNode = groupNode.SelectSingleNode("Name");
            if (nameNode != null)
            {
                group.Name = nameNode.InnerText ?? "";
            }

            XmlNode notesNode = groupNode.SelectSingleNode("Notes");
            if (notesNode != null)
            {
                group.Notes = notesNode.InnerText ?? "";
            }

            // Parse times
            TryParseDateTime(groupNode, "CreationTime", out DateTime creationTime);
            group.CreationTime = creationTime;

            TryParseDateTime(groupNode, "LastModificationTime", out DateTime modTime);
            group.LastModificationTime = modTime;

            // Parse entries
            XmlNodeList entryNodes = groupNode.SelectNodes("Entry");
            foreach (XmlNode entryNode in entryNodes)
            {
                try
                {
                    XmlEntry entry = ParseEntry(entryNode);
                    group.Entries.Add(entry);
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine("Error parsing entry: " + ex.Message);
                }
            }

            // Parse subgroups
            XmlNodeList subgroupNodes = groupNode.SelectNodes("Group");
            foreach (XmlNode subgroupNode in subgroupNodes)
            {
                try
                {
                    XmlGroup subgroup = ParseGroup(subgroupNode);
                    group.Groups.Add(subgroup);
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine("Error parsing subgroup: " + ex.Message);
                }
            }

            return group;
        }

        private static XmlEntry ParseEntry(XmlNode entryNode)
        {
            XmlEntry entry = new XmlEntry();

            // Parse strings (Title, UserName, Password, URL, Notes)
            XmlNodeList stringNodes = entryNode.SelectNodes("String");
            foreach (XmlNode stringNode in stringNodes)
            {
                XmlNode keyNode = stringNode.SelectSingleNode("Key");
                XmlNode valueNode = stringNode.SelectSingleNode("Value");

                if (keyNode != null && valueNode != null)
                {
                    string key = keyNode.InnerText;
                    string value = valueNode.InnerText ?? "";

                    switch (key)
                    {
                        case "Title":
                            entry.Title = value;
                            break;
                        case "UserName":
                            entry.UserName = value;
                            break;
                        case "Password":
                            entry.Password = value;
                            break;
                        case "URL":
                            entry.Url = value;
                            break;
                        case "Notes":
                            entry.Notes = value;
                            break;
                    }
                }
            }

            // Parse times
            TryParseDateTime(entryNode, "CreationTime", out DateTime creationTime);
            entry.CreationTime = creationTime;

            TryParseDateTime(entryNode, "LastModificationTime", out DateTime modTime);
            entry.LastModificationTime = modTime;

            return entry;
        }

        private static bool TryParseDateTime(XmlNode node, string elementName, out DateTime result)
        {
            result = DateTime.Now;
            XmlNode timeNode = node.SelectSingleNode(elementName);
            if (timeNode != null && DateTime.TryParse(timeNode.InnerText, out DateTime parsed))
            {
                result = parsed;
                return true;
            }
            return false;
        }

        /// <summary>
        /// Converts parsed XML data to TreeNode format compatible with display
        /// </summary>
        public static void PopulateTreeFromXml(XmlGroup xmlGroup, System.Windows.Forms.TreeNode treeNode)
        {
            NodeContent nodeContent = new NodeContent();
            nodeContent.EntryType = EntryType.Group;
            nodeContent.Title = xmlGroup.Name;
            nodeContent.Notes = xmlGroup.Notes;
            nodeContent.Created = xmlGroup.CreationTime;
            nodeContent.Updated = xmlGroup.LastModificationTime;

            treeNode.Text = nodeContent.Title;
            treeNode.Tag = nodeContent;

            // Add entries
            foreach (var entry in xmlGroup.Entries)
            {
                NodeContent entryContent = new NodeContent();
                entryContent.EntryType = EntryType.Entry;
                entryContent.Title = entry.Title ?? "(Untitled)";
                entryContent.UserName = entry.UserName;
                entryContent.Password = entry.Password;
                entryContent.Url = entry.Url;
                entryContent.Notes = entry.Notes;
                entryContent.Created = entry.CreationTime;
                entryContent.Updated = entry.LastModificationTime;

                System.Windows.Forms.TreeNode entryNode = new System.Windows.Forms.TreeNode();
                entryNode.Text = entryContent.Title;
                entryNode.Tag = entryContent;
                treeNode.Nodes.Add(entryNode);
            }

            // Add subgroups
            foreach (var subgroup in xmlGroup.Groups)
            {
                System.Windows.Forms.TreeNode subgroupNode = new System.Windows.Forms.TreeNode();
                PopulateTreeFromXml(subgroup, subgroupNode);
                treeNode.Nodes.Add(subgroupNode);
            }
        }
    }
}
