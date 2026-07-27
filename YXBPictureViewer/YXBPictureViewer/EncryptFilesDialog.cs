using System;
using System.IO;
using System.Windows.Forms;

namespace YXBPictureViewer
{
    /// <summary>
    /// Dialog for encrypting image files to YPG or XPG format
    /// </summary>
    public partial class EncryptFilesDialog : Form
    {
        public string TargetDirectory { get; private set; }
        public int FilesProcessed { get; private set; }

        private string ypgPassword;
        private string xpgPassword;

        public EncryptFilesDialog(string ypgPass, string xpgPass)
        {
            InitializeComponent();
            this.ypgPassword = ypgPass;
            this.xpgPassword = xpgPass;

            // Set default values
            cmbEncryptionType.SelectedIndex = 1; // XPG default
            cmbSourceExtension.SelectedIndex = 0; // jpg default
            chkIncludeSubfolders.Checked = true;
            chkDeleteOriginal.Checked = false;
        }

        private void btnBrowse_Click(object sender, EventArgs e)
        {
            using (FolderBrowserDialog dialog = new FolderBrowserDialog())
            {
                dialog.Description = "Select folder containing files to encrypt";

                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    txtFolder.Text = dialog.SelectedPath;
                }
            }
        }

        private void btnEncrypt_Click(object sender, EventArgs e)
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

            // Validate encryption type and password
            bool encryptToYpg = (cmbEncryptionType.SelectedIndex == 0);
            string targetExtension = encryptToYpg ? "ypg" : "xpg";
            string password = encryptToYpg ? ypgPassword : xpgPassword;

            if (string.IsNullOrWhiteSpace(password))
            {
                MessageBox.Show($"Please set the {targetExtension.ToUpper()} password in the main window first.",
                    "Password Required", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Get source extension
            string sourceExt = cmbSourceExtension.Text.Trim();
            if (sourceExt.StartsWith("."))
            {
                sourceExt = sourceExt.Substring(1);
            }

            // Confirm operation
            string message = $"Encrypt all .{sourceExt} files to .{targetExtension} in:\n{txtFolder.Text}\n\n";
            message += $"Include subfolders: {(chkIncludeSubfolders.Checked ? "Yes" : "No")}\n";
            message += $"Delete originals: {(chkDeleteOriginal.Checked ? "YES - BE CAREFUL!" : "No")}\n\n";
            message += "Proceed?";

            DialogResult result = MessageBox.Show(message, "Confirm Encryption",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (result != DialogResult.Yes)
                return;

            // Perform encryption
            try
            {
                FilesProcessed = EncryptAllFiles(
                    txtFolder.Text,
                    sourceExt,
                    targetExtension,
                    password,
                    chkIncludeSubfolders.Checked,
                    chkDeleteOriginal.Checked,
                    encryptToYpg
                );

                TargetDirectory = txtFolder.Text;

                MessageBox.Show($"Successfully encrypted {FilesProcessed} files.",
                    "Encryption Complete", MessageBoxButtons.OK, MessageBoxIcon.Information);

                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error during encryption:\n{ex.Message}",
                    "Encryption Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Encrypt all files with given extension in directory
        /// </summary>
        private int EncryptAllFiles(string directory, string sourceExt, string targetExt,
            string password, bool includeSubdirs, bool deleteOriginal, bool useOldMethod)
        {
            int count = 0;

            try
            {
                // Find all files with source extension
                string[] files = Directory.GetFiles(directory, $"*.{sourceExt}");

                foreach (string file in files)
                {
                    try
                    {
                        // Build output filename
                        string outputFile = Path.Combine(
                            Path.GetDirectoryName(file),
                            Path.GetFileNameWithoutExtension(file) + $".{targetExt}"
                        );

                        // Encrypt using appropriate method
                        if (useOldMethod)
                        {
                            YEncrypt.EncryptFile(file, outputFile, password);
                        }
                        else
                        {
                            YAESEncrypt.EncryptFile(file, outputFile, password);
                        }

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
                        System.Diagnostics.Debug.WriteLine($"Error encrypting {file}: {ex.Message}");
                    }
                }

                // Process subdirectories if requested
                if (includeSubdirs)
                {
                    string[] subdirs = Directory.GetDirectories(directory);
                    foreach (string subdir in subdirs)
                    {
                        count += EncryptAllFiles(subdir, sourceExt, targetExt, password,
                            includeSubdirs, deleteOriginal, useOldMethod);
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
