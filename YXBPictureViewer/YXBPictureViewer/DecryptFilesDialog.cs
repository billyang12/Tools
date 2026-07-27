using System;
using System.IO;
using System.Windows.Forms;

namespace YXBPictureViewer
{
    /// <summary>
    /// Dialog for decrypting YPG/XPG files to JPG format
    /// </summary>
    public partial class DecryptFilesDialog : Form
    {
        public int FilesProcessed { get; private set; }

        private string ypgPassword;
        private string xpgPassword;

        public DecryptFilesDialog(string ypgPass, string xpgPass)
        {
            InitializeComponent();
            this.ypgPassword = ypgPass;
            this.xpgPassword = xpgPass;

            // Set default values
            cmbFileType.SelectedIndex = 0; // Both
            chkIncludeSubfolders.Checked = true;
            chkDeleteOriginal.Checked = false;
        }

        private void btnBrowse_Click(object sender, EventArgs e)
        {
            using (FolderBrowserDialog dialog = new FolderBrowserDialog())
            {
                dialog.Description = "Select folder containing encrypted files";

                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    txtFolder.Text = dialog.SelectedPath;
                }
            }
        }

        private void btnDecrypt_Click(object sender, EventArgs e)
        {
            // Validate folder
            if (string.IsNullOrWhiteSpace(txtFolder.Text))
            {
                MessageBox.Show("Please select a folder.", "Folder Required",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (!Directory.Exists(txtFolder.Text))
            {
                MessageBox.Show("Selected folder does not exist.", "Invalid Folder",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // Validate passwords
            int fileTypeIndex = cmbFileType.SelectedIndex;
            bool processYpg = (fileTypeIndex == 0 || fileTypeIndex == 1); // Both or YPG only
            bool processXpg = (fileTypeIndex == 0 || fileTypeIndex == 2); // Both or XPG only

            if (processYpg && string.IsNullOrWhiteSpace(ypgPassword))
            {
                MessageBox.Show("Please set the YPG password in the main window first.",
                    "Password Required", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (processXpg && string.IsNullOrWhiteSpace(xpgPassword))
            {
                MessageBox.Show("Please set the XPG password in the main window first.",
                    "Password Required", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Get output extension
            string outputExt = txtOutputExtension.Text.Trim();
            if (outputExt.StartsWith("."))
            {
                outputExt = outputExt.Substring(1);
            }

            // Confirm operation
            string fileTypes = fileTypeIndex == 1 ? ".ypg" : fileTypeIndex == 2 ? ".xpg" : ".ypg and .xpg";
            string message = $"Decrypt all {fileTypes} files to .{outputExt} in:\n{txtFolder.Text}\n\n";
            message += $"Include subfolders: {(chkIncludeSubfolders.Checked ? "Yes" : "No")}\n";
            message += $"Delete originals: {(chkDeleteOriginal.Checked ? "YES - BE CAREFUL!" : "No")}\n\n";
            message += "Proceed?";

            DialogResult result = MessageBox.Show(message, "Confirm Decryption",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (result != DialogResult.Yes)
                return;

            // Perform decryption
            try
            {
                FilesProcessed = DecryptAllFiles(
                    txtFolder.Text,
                    outputExt,
                    chkIncludeSubfolders.Checked,
                    chkDeleteOriginal.Checked,
                    processYpg,
                    processXpg
                );

                MessageBox.Show($"Successfully decrypted {FilesProcessed} files.",
                    "Decryption Complete", MessageBoxButtons.OK, MessageBoxIcon.Information);

                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error during decryption:\n{ex.Message}",
                    "Decryption Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Decrypt all YPG/XPG files in directory
        /// </summary>
        private int DecryptAllFiles(string directory, string outputExt, bool includeSubdirs,
            bool deleteOriginal, bool processYpg, bool processXpg)
        {
            int count = 0;

            try
            {
                // Process YPG files
                if (processYpg)
                {
                    string[] ypgFiles = Directory.GetFiles(directory, "*.ypg");
                    foreach (string file in ypgFiles)
                    {
                        try
                        {
                            string outputFile = Path.Combine(
                                Path.GetDirectoryName(file),
                                Path.GetFileNameWithoutExtension(file) + $".{outputExt}"
                            );

                            // Decrypt YPG file
                            YEncrypt.DecryptFile(file, outputFile, ypgPassword);
                            count++;

                            // Delete original if requested
                            if (deleteOriginal)
                            {
                                try
                                {
                                    File.Delete(file);
                                }
                                catch (Exception ex)
                                {
                                    System.Diagnostics.Debug.WriteLine($"Could not delete {file}: {ex.Message}");
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            System.Diagnostics.Debug.WriteLine($"Error decrypting {file}: {ex.Message}");
                        }
                    }
                }

                // Process XPG files
                if (processXpg)
                {
                    string[] xpgFiles = Directory.GetFiles(directory, "*.xpg");
                    foreach (string file in xpgFiles)
                    {
                        try
                        {
                            string outputFile = Path.Combine(
                                Path.GetDirectoryName(file),
                                Path.GetFileNameWithoutExtension(file) + $".{outputExt}"
                            );

                            // Decrypt XPG file
                            YAESEncrypt.DecryptFile(file, outputFile, xpgPassword);
                            count++;

                            // Delete original if requested
                            if (deleteOriginal)
                            {
                                try
                                {
                                    File.Delete(file);
                                }
                                catch (Exception ex)
                                {
                                    System.Diagnostics.Debug.WriteLine($"Could not delete {file}: {ex.Message}");
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            System.Diagnostics.Debug.WriteLine($"Error decrypting {file}: {ex.Message}");
                        }
                    }
                }

                // Process subdirectories if requested
                if (includeSubdirs)
                {
                    string[] subdirs = Directory.GetDirectories(directory);
                    foreach (string subdir in subdirs)
                    {
                        count += DecryptAllFiles(subdir, outputExt, includeSubdirs,
                            deleteOriginal, processYpg, processXpg);
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error processing directory {directory}: {ex.Message}");
            }

            return count;
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }
    }
}
