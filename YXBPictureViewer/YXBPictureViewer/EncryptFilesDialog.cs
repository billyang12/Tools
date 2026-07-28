using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
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
        private bool cancelRequested = false;

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

            // Determine if source is video format
            string sourceExt = cmbSourceExtension.Text.Trim();
            if (sourceExt.StartsWith("."))
            {
                sourceExt = sourceExt.Substring(1);
            }

            bool isVideoFormat = IsVideoFormat(sourceExt);

            // Use .xpv for videos, .xpg or .ypg for images
            string targetExtension;
            if (isVideoFormat)
            {
                targetExtension = "xpv"; // Video files always use XPV (AES only)
                encryptToYpg = false; // Force AES for videos
            }
            else
            {
                targetExtension = encryptToYpg ? "ypg" : "xpg";
            }

            string password = encryptToYpg ? ypgPassword : xpgPassword;

            if (string.IsNullOrWhiteSpace(password))
            {
                MessageBox.Show($"Please set the {targetExtension.ToUpper()} password in the main window first.",
                    "Password Required", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
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

            // Perform encryption with progress dialog
            EncryptWithProgress(
                txtFolder.Text,
                sourceExt,
                targetExtension,
                password,
                chkIncludeSubfolders.Checked,
                chkDeleteOriginal.Checked,
                encryptToYpg
            );
        }

        /// <summary>
        /// Encrypt files with progress dialog running in background
        /// </summary>
        private async void EncryptWithProgress(string directory, string sourceExt, string targetExt,
            string password, bool includeSubdirs, bool deleteOriginal, bool useOldMethod)
        {
            cancelRequested = false;

            using (ProgressDialog progressDlg = new ProgressDialog("Encrypting Files"))
            {
                progressDlg.Show(this);
                progressDlg.SetStatus("Scanning for files...");

                try
                {
                    // First, collect all files to process
                    List<string> allFiles = await Task.Run(() =>
                        CollectFiles(directory, sourceExt, includeSubdirs));

                    if (allFiles.Count == 0)
                    {
                        progressDlg.Close();
                        MessageBox.Show($"No .{sourceExt} files found in the selected folder.",
                            "No Files Found", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        return;
                    }

                    // Process files with progress updates
                    FilesProcessed = await Task.Run(() =>
                        EncryptAllFilesWithProgress(allFiles, targetExt, password, deleteOriginal,
                            useOldMethod, progressDlg));

                    progressDlg.Close();

                    if (cancelRequested)
                    {
                        MessageBox.Show($"Encryption cancelled. {FilesProcessed} files processed.",
                            "Encryption Cancelled", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    else
                    {
                        TargetDirectory = directory;
                        MessageBox.Show($"Successfully encrypted {FilesProcessed} files.",
                            "Encryption Complete", MessageBoxButtons.OK, MessageBoxIcon.Information);

                        this.DialogResult = DialogResult.OK;
                        this.Close();
                    }
                }
                catch (Exception ex)
                {
                    progressDlg.Close();
                    MessageBox.Show($"Error during encryption:\n{ex.Message}",
                        "Encryption Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        /// <summary>
        /// Collect all files to be processed
        /// </summary>
        private List<string> CollectFiles(string directory, string sourceExt, bool includeSubdirs)
        {
            List<string> files = new List<string>();

            try
            {
                SearchOption searchOption = includeSubdirs ?
                    SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;

                string[] foundFiles = Directory.GetFiles(directory, $"*.{sourceExt}", searchOption);
                files.AddRange(foundFiles);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error collecting files: {ex.Message}");
            }

            return files;
        }

        /// <summary>
        /// Encrypt all files with progress reporting
        /// </summary>
        private int EncryptAllFilesWithProgress(List<string> files, string targetExt,
            string password, bool deleteOriginal, bool useOldMethod, ProgressDialog progressDlg)
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

                try
                {
                    // Update progress
                    progressDlg.UpdateProgress(i + 1, total,
                        $"Encrypting: {Path.GetFileName(file)}");

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

            return count;
        }

        /// <summary>
        /// Check if file extension is a video format
        /// </summary>
        private bool IsVideoFormat(string extension)
        {
            extension = extension.ToLower().TrimStart('.');
            string[] videoExtensions = { "mp4", "avi", "wmv", "mov", "mkv", "mpeg", "mpg" };

            foreach (string ext in videoExtensions)
            {
                if (extension == ext)
                    return true;
            }

            return false;
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }
    }
}
