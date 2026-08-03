using KeePassLib.Keys;
using KeePassLib;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using YXBKeepassReader.HNoteExport;

namespace YXBKeepassReader
{
    public partial class MainForm : Form
    {
        private RecentFilesManager recentFilesManager;
        private const string DefaultTitle = "Keepass DB Reader";
        private string currentFilePath = null;

        public MainForm()
        {
            InitializeComponent();
            recentFilesManager = new RecentFilesManager();

            // Set application icon
            try
            {
                string iconPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "app.ico");
                if (File.Exists(iconPath))
                {
                    this.Icon = new Icon(iconPath);
                }
            }
            catch { }

            // Set initial title
            UpdateWindowTitle();
        }

        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Title = "Select a KeePass Database or XML Export";
            ofd.Filter = "All Supported Files|*.kdbx;*.xml|KeePass Databases|*.kdbx|XML Exports|*.xml|All Files|*.*";
            ofd.FilterIndex = 0;
            ofd.CheckFileExists = true;
            ofd.Multiselect = false;

            DialogResult result = ofd.ShowDialog();
            if (result != DialogResult.OK) return;

            OpenFile(ofd.FileName);
        }

        private void OpenFile(string filePath)
        {
            try
            {
                // First, detect the file format
                var formatInfo = FileFormatDetector.DetectFormat(filePath);

                if (!formatInfo.IsSupported && formatInfo.Format == FileFormatDetector.FileFormat.KDBX)
                {
                    // KDBX format not supported, suggest XML export
                    DialogResult result = MessageBox.Show(
                        formatInfo.Message + "\n\n" +
                        "Would you like to try loading it anyway?\n\n" +
                        "Click NO to see help on exporting as XML.",
                        "File Format Warning",
                        MessageBoxButtons.YesNo,
                        MessageBoxIcon.Warning);

                    if (result != DialogResult.Yes)
                    {
                        // Show help dialog
                        using (HelpDialog helpForm = new HelpDialog())
                        {
                            helpForm.ShowDialog(this);
                        }
                        return;
                    }
                }

                // For KDBX files, ask for password
                if (formatInfo.Format == FileFormatDetector.FileFormat.KDBX)
                {
                    using (PasswordDialog passwordDialog = new PasswordDialog(filePath))
                    {
                        DialogResult pwdResult = passwordDialog.ShowDialog(this);
                        if (pwdResult != DialogResult.OK) return;

                        string password = passwordDialog.Password;

                        try
                        {
                            LoadAsKDBX(filePath, password);
                            recentFilesManager.AddRecentFile(filePath);
                            return;
                        }
                        catch (Exception kdbxEx)
                        {
                            System.Diagnostics.Debug.WriteLine("KDBX loading failed: " + kdbxEx.Message);

                            // If KDBX fails, offer XML fallback with detailed error dialog
                            string errorMsg = "The file version is not supported by the current KeePass library.\n\n" +
                                            "Error Details:\n" + kdbxEx.Message + "\n\n" +
                                            "Solution:\n" +
                                            "Export your KeePass database as XML:\n" +
                                            "1. Open the file in KeePass application\n" +
                                            "2. Click File → Export → XML (*.xml)\n" +
                                            "3. Save the XML file\n" +
                                            "4. Select the XML file in KeePass Reader\n\n" +
                                            "Click the Help button below for detailed instructions.";

                            using (DetailedErrorDialog errorForm = new DetailedErrorDialog(
                                "Failed to Load KDBX File",
                                errorMsg,
                                showHelpButton: true))
                            {
                                errorForm.ShowDialog(this);
                            }
                            return;
                        }
                    }
                }

                // Load as XML (no password needed)
                if (formatInfo.Format == FileFormatDetector.FileFormat.XML)
                {
                    LoadAsXML(filePath);
                    recentFilesManager.AddRecentFile(filePath);
                    return;
                }

                MessageBox.Show("Unknown file format.", "Error");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error opening database:\n\n" + ex.GetType().Name + ": " + ex.Message, "Error");
            }
        }

        private void LoadAsKDBX(string dbPath, string password)
        {
            // Open the KeePass database
            var compositeKey = new CompositeKey();
            compositeKey.AddUserKey(new KcpPassword(password));

            var database = new PwDatabase();
            using (var fileStream = new FileStream(dbPath, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                var ioInfo = new KeePassLib.Serialization.IOConnectionInfo();
                ioInfo.Path = dbPath;
                database.Open(ioInfo, compositeKey, null);
            }

            treeView1.Nodes.Clear();
            TreeNode node = new TreeNode();
            TraverseGroup(database.RootGroup, 0, node);
            treeView1.Nodes.Add(node);

            // Close the database
            database.Close();

            // Store current file path and update title
            currentFilePath = dbPath;
            UpdateWindowTitle();

            // Enable Close File menu and Export
            closeFileToolStripMenuItem.Enabled = true;
            exportToHNFToolStripMenuItem.Enabled = true;

            MessageBox.Show("Database opened successfully!", "Success");
        }

        private void LoadAsXML(string xmlPath)
        {
            try
            {
                var xmlGroup = KeePassXmlReader.LoadFromXml(xmlPath);

                treeView1.Nodes.Clear();
                TreeNode rootNode = new TreeNode();
                KeePassXmlReader.PopulateTreeFromXml(xmlGroup, rootNode);
                treeView1.Nodes.Add(rootNode);

                // Store current file path and update title
                currentFilePath = xmlPath;
                UpdateWindowTitle();

                // Enable Close File menu and Export
                closeFileToolStripMenuItem.Enabled = true;
                exportToHNFToolStripMenuItem.Enabled = true;

                MessageBox.Show("XML file opened successfully!\n\nNote: XML format is read-only." +
                               "\n\nAll your entries are now visible.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                string errorMsg = "Error Details:\n" + ex.Message + "\n\n" +
                                "Make sure you exported the file from KeePass using:\n" +
                                "File → Export → XML (*.xml)";

                using (DetailedErrorDialog errorForm = new DetailedErrorDialog(
                    "Error Loading XML File",
                    errorMsg,
                    showHelpButton: false))
                {
                    errorForm.ShowDialog(this);
                }
            }
        }

        void TraverseGroup(PwGroup group, int level, TreeNode node)
        {
            NodeContent note = new NodeContent();
            note.EntryType= EntryType.Group;
            note.Title = group.Name;
            note.Notes = group.Notes;
            note.Created = group.CreationTime;
            note.Updated = group.LastModificationTime;
            node.Text = note.Title;
            node.Tag = note;

            foreach (PwEntry entry in group.Entries)
            {
                NodeContent subNote = new NodeContent();
                subNote.EntryType = EntryType.Entry;
                subNote.Title = entry.Strings.ReadSafe("Title");
                subNote.UserName = entry.Strings.ReadSafe("UserName");
                subNote.Password = entry.Strings.ReadSafe("Password");
                subNote.Url= entry.Strings.ReadSafe("URL");
                subNote.Notes = entry.Strings.ReadSafe("Notes");
                subNote.Created = entry.CreationTime;
                subNote.Updated = entry.LastModificationTime;

                TreeNode subNode = new TreeNode();
                subNode.Tag = subNote;
                subNode.Text = subNote.Title;
                node.Nodes.Add( subNode );
            }

            // Recursively traverse subgroups
            foreach (PwGroup subgroup in group.Groups)
            {
                TreeNode subGroupNode = new TreeNode();
                TraverseGroup(subgroup, level + 1, subGroupNode);
                node.Nodes.Add( subGroupNode );
            }
        }

        private void treeView1_AfterSelect(object sender, TreeViewEventArgs e)
        {
            TreeNode node = treeView1.SelectedNode;
            if(node == null) return;
            NodeContent nodeContent = node.Tag as NodeContent;
            if (nodeContent == null) return;
            tbContent.Text = nodeContent.ToString();
        }

        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (AboutDialog aboutForm = new AboutDialog())
            {
                aboutForm.ShowDialog(this);
            }
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void recentFilesToolStripMenuItem_DropDownOpening(object sender, EventArgs e)
        {
            // Clear existing items
            recentFilesToolStripMenuItem.DropDownItems.Clear();

            var recentFiles = recentFilesManager.GetRecentFiles();

            if (recentFiles.Count == 0)
            {
                var noFilesItem = new ToolStripMenuItem("(No recent files)");
                noFilesItem.Enabled = false;
                recentFilesToolStripMenuItem.DropDownItems.Add(noFilesItem);
            }
            else
            {
                int index = 1;
                foreach (var file in recentFiles)
                {
                    string displayText = $"&{index}  {Path.GetFileName(file)}";
                    var item = new ToolStripMenuItem(displayText);
                    item.Tag = file;
                    item.ToolTipText = file;
                    item.Click += RecentFileItem_Click;
                    recentFilesToolStripMenuItem.DropDownItems.Add(item);
                    index++;
                }

                // Add separator and Clear list item
                recentFilesToolStripMenuItem.DropDownItems.Add(new ToolStripSeparator());
                var clearItem = new ToolStripMenuItem("&Clear Recent Files List");
                clearItem.Click += ClearRecentFiles_Click;
                recentFilesToolStripMenuItem.DropDownItems.Add(clearItem);
            }
        }

        private void RecentFileItem_Click(object sender, EventArgs e)
        {
            var menuItem = sender as ToolStripMenuItem;
            if (menuItem?.Tag is string filePath)
            {
                if (File.Exists(filePath))
                {
                    OpenFile(filePath);
                }
                else
                {
                    MessageBox.Show($"File not found:\n{filePath}", "File Not Found", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
        }

        private void ClearRecentFiles_Click(object sender, EventArgs e)
        {
            DialogResult result = MessageBox.Show(
                "Are you sure you want to clear the recent files list?",
                "Clear Recent Files",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
            {
                recentFilesManager.ClearRecentFiles();
            }
        }

        private void closeFileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            CloseCurrentFile();
        }

        private void CloseCurrentFile()
        {
            // Clear tree view
            treeView1.Nodes.Clear();

            // Clear content display
            tbContent.Clear();

            // Clear current file path and update title
            currentFilePath = null;
            UpdateWindowTitle();

            // Disable Close File menu and Export
            closeFileToolStripMenuItem.Enabled = false;
            exportToHNFToolStripMenuItem.Enabled = false;
        }

        private void UpdateWindowTitle()
        {
            if (string.IsNullOrEmpty(currentFilePath))
            {
                this.Text = DefaultTitle;
            }
            else
            {
                this.Text = $"{DefaultTitle} - {currentFilePath}";
            }
        }

        private void exportToHNFToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // Check if tree view has any data
            if (treeView1.Nodes.Count == 0)
            {
                MessageBox.Show("No data to export. Please open a KeePass file first.", "Export Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Ask for encryption password
            using (PasswordDialog passwordDialog = new PasswordDialog("Enter password to encrypt the HNF file (minimum 8 characters):", true))
            {
                DialogResult pwdResult = passwordDialog.ShowDialog(this);
                if (pwdResult != DialogResult.OK) return;

                string password = passwordDialog.Password;

                // Validate password length
                if (string.IsNullOrEmpty(password) || password.Length < 8)
                {
                    MessageBox.Show("Password must be at least 8 characters long.", "Invalid Password", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // Show save file dialog
                SaveFileDialog sfd = new SaveFileDialog();
                sfd.Title = "Export to HierachicalNotes Format";
                sfd.Filter = "HierachicalNotes Files|*.hnf|All Files|*.*";
                sfd.FilterIndex = 0;
                sfd.DefaultExt = "hnf";

                if (sfd.ShowDialog() != DialogResult.OK) return;

                try
                {
                    // Export to HNF format
                    HNoteExporter.ExportToHNF(treeView1, sfd.FileName, password);
                    MessageBox.Show($"Successfully exported to:\n{sfd.FileName}", "Export Successful", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error exporting to HNF:\n\n{ex.Message}", "Export Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }
    }
}
