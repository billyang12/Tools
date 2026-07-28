using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
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
        private bool cancelRequested = false;

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
            bool processXpv = (fileTypeIndex == 0 || fileTypeIndex == 2); // Include XPV with XPG (same password)

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
            string fileTypes = fileTypeIndex == 1 ? ".ypg" : fileTypeIndex == 2 ? ".xpg/.xpv" : ".ypg, .xpg, and .xpv";
            string message = $"Decrypt all {fileTypes} files to .{outputExt} in:\n{txtFolder.Text}\n\n";
            message += $"Include subfolders: {(chkIncludeSubfolders.Checked ? "Yes" : "No")}\n";
            message += $"Delete originals: {(chkDeleteOriginal.Checked ? "YES - BE CAREFUL!" : "No")}\n\n";
            message += "Proceed?";

            DialogResult result = MessageBox.Show(message, "Confirm Decryption",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (result != DialogResult.Yes)
                return;

            // Perform decryption with progress
            DecryptWithProgress(
                txtFolder.Text,
                outputExt,
                chkIncludeSubfolders.Checked,
                chkDeleteOriginal.Checked,
                processYpg,
                processXpg,
                processXpv
            );
        }

        /// <summary>
        /// Decrypt files with progress dialog
        /// </summary>
        private async void DecryptWithProgress(string directory, string outputExt, bool includeSubdirs,
            bool deleteOriginal, bool processYpg, bool processXpg, bool processXpv)
        {
            cancelRequested = false;

            using (ProgressDialog progressDlg = new ProgressDialog("Decrypting Files"))
            {
                progressDlg.Show(this);
                progressDlg.SetStatus("Scanning for files...");

                try
                {
                    // Collect all files to process
                    List<string> allFiles = await Task.Run(() =>
                        CollectDecryptFiles(directory, includeSubdirs, processYpg, processXpg, processXpv));

                    if (allFiles.Count == 0)
                    {
                        progressDlg.Close();
                        MessageBox.Show("No encrypted files found in the selected folder.",
                            "No Files Found", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        return;
                    }

                    // Process files with progress
                    FilesProcessed = await Task.Run(() =>
                        DecryptAllFilesWithProgress(allFiles, outputExt, deleteOriginal,
                            processYpg, processXpg, progressDlg));

                    progressDlg.Close();

                    if (cancelRequested)
                    {
                        MessageBox.Show($"Decryption cancelled. {FilesProcessed} files processed.",
                            "Decryption Cancelled", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    else
                    {
                        MessageBox.Show($"Successfully decrypted {FilesProcessed} files.",
                            "Decryption Complete", MessageBoxButtons.OK, MessageBoxIcon.Information);

                        this.DialogResult = DialogResult.OK;
                        this.Close();
                    }
                }
                catch (Exception ex)
                {
                    progressDlg.Close();
                    MessageBox.Show($"Error during decryption:\n{ex.Message}",
                        "Decryption Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        /// <summary>
        /// Collect all encrypted files to process
        /// </summary>
        private List<string> CollectDecryptFiles(string directory, bool includeSubdirs,
            bool processYpg, bool processXpg, bool processXpv)
        {
            List<string> files = new List<string>();

            try
            {
                SearchOption searchOption = includeSubdirs ?
                    SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;

                if (processYpg)
                {
                    string[] ypgFiles = Directory.GetFiles(directory, "*.ypg", searchOption);
                    files.AddRange(ypgFiles);
                }

                if (processXpg)
                {
                    string[] xpgFiles = Directory.GetFiles(directory, "*.xpg", searchOption);
                    files.AddRange(xpgFiles);
                }

                if (processXpv)
                {
                    string[] xpvFiles = Directory.GetFiles(directory, "*.xpv", searchOption);
                    files.AddRange(xpvFiles);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error collecting files: {ex.Message}");
            }

            return files;
        }

        /// <summary>
        /// Decrypt all files with progress reporting
        /// </summary>
        private int DecryptAllFilesWithProgress(List<string> files, string outputExt,
            bool deleteOriginal, bool processYpg, bool processXpg, ProgressDialog progressDlg)
        {
            int count = 0;
            int total = files.Count;

            for (int i = 0; i < total; i++)
            {
                if (progressDlg.CancelRequested)
                {
                    cancelRequested = true;
                    break;
                }

                string file = files[i];
                string extension = Path.GetExtension(file).ToLower();

                try
                {
                    // Update progress
                    progressDlg.UpdateProgress(i + 1, total,
                        $"Decrypting: {Path.GetFileName(file)}");

                    string outputFile = Path.Combine(
                        Path.GetDirectoryName(file),
                        Path.GetFileNameWithoutExtension(file) + $".{outputExt}"
                    );

                    // Decrypt based on file type
                    if (extension == ".ypg")
                    {
                        YEncrypt.DecryptFile(file, outputFile, ypgPassword);
                    }
                    else if (extension == ".xpg" || extension == ".xpv")
                    {
                        YAESEncrypt.DecryptFile(file, outputFile, xpgPassword);
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
                    System.Diagnostics.Debug.WriteLine($"Error decrypting {file}: {ex.Message}");
                }
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
