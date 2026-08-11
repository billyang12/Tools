using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Forms;
using LibVLCSharp.Shared;

namespace YXBPictureViewer
{
    /// <summary>
    /// Main form for viewing and managing encrypted images
    /// Supports both old .ypg (DES) and new .xpg (AES-256) formats
    /// </summary>
    public partial class MainForm : Form
    {
        private string currentRootPath = "";
        private bool isPanelExpanded = true;
        private int expandedHeight = 180;
        private int collapsedHeight = 40;
        private string currentTempVideoFile = null;
        private LibVLC libVLC;
        private MediaPlayer vlcMediaPlayer;
        private System.Windows.Forms.Timer videoProgressTimer;
        private bool isDraggingVideoProgress = false;
        private bool isLoadingFirstFrame = false;

        public MainForm()
        {
            InitializeComponent();
            InitializeCustomComponents();
            InitializeVLC();
        }

        /// <summary>
        /// Initialize custom UI components not in designer
        /// </summary>
        private void InitializeCustomComponents()
        {
            // Set up TreeView
            tvFileExplorer.ImageList = new ImageList();
            tvFileExplorer.ImageList.Images.Add("Folder", SystemIcons.Shield.ToBitmap());
            tvFileExplorer.ImageList.Images.Add("File", SystemIcons.Application.ToBitmap());

            // Set up PictureBox for image display
            pictureBox.SizeMode = PictureBoxSizeMode.Zoom;
        }

        /// <summary>
        /// Initialize LibVLC for video playback
        /// </summary>
        private void InitializeVLC()
        {
            try
            {
                Core.Initialize();
                libVLC = new LibVLC("--no-video-title-show");
                vlcMediaPlayer = new MediaPlayer(libVLC)
                {
                    Mute = false,
                    Volume = 50
                };
                mediaPlayer.MediaPlayer = vlcMediaPlayer;

                // Set volume slider to match initial volume
                trackVolume.Value = 50;

                // Handle media ended event for cleanup
                vlcMediaPlayer.EndReached += VlcMediaPlayer_EndReached;
                vlcMediaPlayer.Playing += VlcMediaPlayer_Playing;
                vlcMediaPlayer.Paused += VlcMediaPlayer_Paused;
                vlcMediaPlayer.Stopped += VlcMediaPlayer_Paused;

                videoProgressTimer = new System.Windows.Forms.Timer();
                videoProgressTimer.Interval = 250;
                videoProgressTimer.Tick += VideoProgressTimer_Tick;
                videoProgressTimer.Start();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error initializing VLC: {ex.Message}");
            }
        }

        private void VlcMediaPlayer_EndReached(object sender, EventArgs e)
        {
            CleanupTempVideoFile();

            if (IsHandleCreated)
            {
                BeginInvoke(new Action(() =>
                {
                    btnPlayPause.Text = "Play";
                }));
            }
        }

        private void VlcMediaPlayer_Playing(object sender, EventArgs e)
        {
            if (IsHandleCreated)
            {
                BeginInvoke(new Action(() =>
                {
                    btnPlayPause.Text = "Pause";

                    // If we're loading the first frame, pause immediately to show it
                    if (isLoadingFirstFrame)
                    {
                        isLoadingFirstFrame = false;
                        // Add a small delay to ensure the first frame is rendered
                        System.Threading.Timer pauseTimer = null;
                        pauseTimer = new System.Threading.Timer(_ =>
                        {
                            if (vlcMediaPlayer != null && vlcMediaPlayer.IsPlaying)
                            {
                                vlcMediaPlayer.Pause();
                            }
                            pauseTimer?.Dispose();
                        }, null, 50, System.Threading.Timeout.Infinite);
                    }
                }));
            }
        }

        private void VlcMediaPlayer_Paused(object sender, EventArgs e)
        {
            if (IsHandleCreated)
            {
                BeginInvoke(new Action(() => btnPlayPause.Text = "Play"));
            }
        }

        /// <summary>
        /// Update the video progress bar and time label
        /// </summary>
        private void VideoProgressTimer_Tick(object sender, EventArgs e)
        {
            if (vlcMediaPlayer == null || !mediaPlayer.Visible)
                return;

            long length = vlcMediaPlayer.Length;

            if (length <= 0)
                return;

            if (!isDraggingVideoProgress)
            {
                double position = vlcMediaPlayer.Position;
                int value = (int)(position * trackVideoProgress.Maximum);
                value = Math.Max(trackVideoProgress.Minimum, Math.Min(trackVideoProgress.Maximum, value));
                trackVideoProgress.Value = value;
            }

            lblVideoTime.Text = $"{FormatTime(vlcMediaPlayer.Time)} / {FormatTime(length)}";
        }

        private static string FormatTime(long milliseconds)
        {
            TimeSpan span = TimeSpan.FromMilliseconds(Math.Max(0, milliseconds));
            return span.Hours > 0
                ? span.ToString(@"hh\:mm\:ss")
                : span.ToString(@"mm\:ss");
        }

        /// <summary>
        /// Play/Pause button click
        /// </summary>
        private void btnPlayPause_Click(object sender, EventArgs e)
        {
            if (vlcMediaPlayer == null)
                return;

            if (vlcMediaPlayer.IsPlaying)
            {
                vlcMediaPlayer.Pause();
            }
            else
            {
                vlcMediaPlayer.Play();
            }
        }

        private void trackVideoProgress_MouseDown(object sender, MouseEventArgs e)
        {
            isDraggingVideoProgress = true;
        }

        private void trackVideoProgress_MouseUp(object sender, MouseEventArgs e)
        {
            isDraggingVideoProgress = false;
            SeekVideoToTrackPosition();
        }

        private void trackVideoProgress_Scroll(object sender, EventArgs e)
        {
            if (!isDraggingVideoProgress)
            {
                SeekVideoToTrackPosition();
            }
        }

        private void SeekVideoToTrackPosition()
        {
            if (vlcMediaPlayer == null || vlcMediaPlayer.Length <= 0)
                return;

            float position = (float)trackVideoProgress.Value / trackVideoProgress.Maximum;
            vlcMediaPlayer.Position = position;
        }

        /// <summary>
        /// Volume control scroll
        /// </summary>
        private void trackVolume_Scroll(object sender, EventArgs e)
        {
            if (vlcMediaPlayer != null)
            {
                vlcMediaPlayer.Volume = trackVolume.Value;
            }
        }

        /// <summary>
        /// Browse for folder button click

        private void btnBrowseFolder_Click(object sender, EventArgs e)
        {
            try
            {
                using (FolderBrowserDialog dialog = new FolderBrowserDialog())
                {
                    dialog.Description = "Select folder containing images";
                    dialog.ShowNewFolderButton = false;

                    if (dialog.SelectedPath != "")
                    {
                        dialog.SelectedPath = currentRootPath;
                    }

                    if (dialog.ShowDialog() == DialogResult.OK)
                    {
                        LoadFolder(dialog.SelectedPath);
                    }
                }
            }
            catch (Exception ex)
            {
                // If folder browser fails, offer manual entry
                MessageBox.Show($"Folder browser error: {ex.Message}\n\nYou can enter the path manually below.",
                    "Browse Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        /// <summary>
        /// Manual folder path entry
        /// </summary>
        private void btnLoadManualPath_Click(object sender, EventArgs e)
        {
            string path = txtFolderPath.Text.Trim();

            if (string.IsNullOrWhiteSpace(path))
            {
                MessageBox.Show("Please enter a folder path.", "Empty Path",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            if (!Directory.Exists(path))
            {
                MessageBox.Show($"Folder not found:\n{path}", "Invalid Path",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            LoadFolder(path);
        }

        /// <summary>
        /// Load folder structure into TreeView
        /// </summary>
        private void LoadFolder(string path)
        {
            try
            {
                currentRootPath = path;
                txtFolderPath.Text = path;

                // Clear existing tree
                tvFileExplorer.Nodes.Clear();

                // Create root node
                TreeNode rootNode = new TreeNode(Path.GetFileName(path));
                rootNode.Tag = path;
                rootNode.Name = path;
                rootNode.ImageKey = "Folder";
                rootNode.SelectedImageKey = "Folder";

                // Load directory structure
                LoadDirectoryNodes(rootNode, path);

                // Add to tree and expand
                tvFileExplorer.Nodes.Add(rootNode);
                rootNode.Expand();

                lblStatus.Text = $"Loaded: {path}";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading folder:\n{ex.Message}", "Load Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Recursively load directory structure into TreeView nodes
        /// </summary>
        private void LoadDirectoryNodes(TreeNode parentNode, string directory)
        {
            try
            {
                // Load subdirectories
                string[] subDirs = Directory.GetDirectories(directory);
                foreach (string subDir in subDirs)
                {
                    TreeNode dirNode = new TreeNode(Path.GetFileName(subDir));
                    dirNode.Tag = subDir;
                    dirNode.Name = subDir;
                    dirNode.ImageKey = "Folder";
                    dirNode.SelectedImageKey = "Folder";

                    LoadDirectoryNodes(dirNode, subDir);
                    parentNode.Nodes.Add(dirNode);
                }

                // Load files (images and encrypted files)
                string[] files = Directory.GetFiles(directory);
                foreach (string file in files)
                {
                    string ext = Path.GetExtension(file).ToLower();

                    // Only show image files and encrypted files
                    if (IsImageOrEncryptedFile(ext))
                    {
                        TreeNode fileNode = new TreeNode(Path.GetFileName(file));
                        fileNode.Tag = file;
                        fileNode.Name = file;
                        fileNode.ImageKey = "File";
                        fileNode.SelectedImageKey = "File";

                        parentNode.Nodes.Add(fileNode);
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading directory {directory}: {ex.Message}");
            }
        }

        /// <summary>
        /// Check if file extension is an image or encrypted file
        /// </summary>
        private bool IsImageOrEncryptedFile(string extension)
        {
            extension = extension.ToLower();
            string[] validExtensions = { ".jpg", ".jpeg", ".png", ".gif", ".bmp", ".tif", ".tiff", ".ypg", ".xpg", ".xpv", ".mp4", ".avi", ".wmv", ".mov", ".mkv", ".mpeg", ".mpg" };

            foreach (string ext in validExtensions)
            {
                if (extension == ext)
                    return true;
            }

            return false;
        }

        /// <summary>
        /// Check if file extension is a video file
        /// </summary>
        private bool IsVideoFile(string extension)
        {
            extension = extension.ToLower();
            string[] videoExtensions = { ".xpv", ".mp4", ".avi", ".wmv", ".mov", ".mkv", ".mpeg", ".mpg" };

            foreach (string ext in videoExtensions)
            {
                if (extension == ext)
                    return true;
            }

            return false;
        }

        /// <summary>
        /// TreeView node selection changed - display the selected file
        /// </summary>
        private void tvFileExplorer_AfterSelect(object sender, TreeViewEventArgs e)
        {
            if (e.Node == null || e.Node.Tag == null)
                return;

            string path = e.Node.Tag.ToString();

            // Only display files, not folders
            if (File.Exists(path))
            {
                string extension = Path.GetExtension(path).ToLower();
                if (IsVideoFile(extension))
                {
                    PlayVideo(path);
                }
                else
                {
                    DisplayImage(path);
                }
            }
        }

        /// <summary>
        /// View mode changed - adjust image display
        /// </summary>
        private void ViewMode_CheckedChanged(object sender, EventArgs e)
        {
            if (pictureBox.Image == null)
                return;

            ApplyViewMode();
        }

        /// <summary>
        /// Apply current view mode to picture box
        /// </summary>
        private void ApplyViewMode()
        {
            if (pictureBox.Image == null)
                return;

            // Hide zoom track bar by default
            trackZoom.Visible = false;
            lblZoom.Visible = false;

            if (rbZoom.Checked)
            {
                // Zoom mode - show track bar and allow zoom control
                trackZoom.Visible = true;
                lblZoom.Visible = true;
                pictureBox.Dock = DockStyle.None;
                pictureBox.SizeMode = PictureBoxSizeMode.Zoom;
                AdjustZoom();
            }
            else if (rbStretchImage.Checked)
            {
                // Stretch to fill panel
                pictureBox.Dock = DockStyle.Fill;
                pictureBox.SizeMode = PictureBoxSizeMode.StretchImage;
                pictureBox.Margin = new Padding(0);
            }
            else if (rbNormal.Checked)
            {
                // Show at actual size
                pictureBox.Dock = DockStyle.None;
                pictureBox.SizeMode = PictureBoxSizeMode.Normal;
                pictureBox.Width = pictureBox.Image.Width;
                pictureBox.Height = pictureBox.Image.Height;
                pictureBox.Left = 0;
                pictureBox.Top = 0;

                // Add bottom margin to accommodate horizontal scrollbar
                pictureBox.Margin = new Padding(0, 0, 0, SystemInformation.HorizontalScrollBarHeight);
            }
            else if (rbAutoSize.Checked)
            {
                // Auto size to image
                pictureBox.Dock = DockStyle.None;
                pictureBox.SizeMode = PictureBoxSizeMode.AutoSize;
                pictureBox.Left = 0;
                pictureBox.Top = 0;

                // Add bottom margin to accommodate horizontal scrollbar
                pictureBox.Margin = new Padding(0, 0, 0, SystemInformation.HorizontalScrollBarHeight);
            }
            else if (rbCenterImage.Checked)
            {
                // Center in panel
                pictureBox.Dock = DockStyle.None;
                pictureBox.SizeMode = PictureBoxSizeMode.CenterImage;
                pictureBox.Width = pictureBox.Image.Width;
                pictureBox.Height = pictureBox.Image.Height;
                pictureBox.Left = 0;
                pictureBox.Top = 0;

                // Add bottom margin to accommodate horizontal scrollbar
                pictureBox.Margin = new Padding(0, 0, 0, SystemInformation.HorizontalScrollBarHeight);
            }
        }

        /// <summary>
        /// Zoom track bar scrolled
        /// </summary>
        private void trackZoom_Scroll(object sender, EventArgs e)
        {
            AdjustZoom();
        }

        /// <summary>
        /// Adjust picture box size based on zoom level
        /// </summary>
        private void AdjustZoom()
        {
            if (pictureBox.Image == null)
                return;

            // Calculate dimensions based on track bar value (10-100%)
            double imgWidth = pictureBox.Image.Width;
            double imgHeight = pictureBox.Image.Height;
            double maxWidth = 2 * imgWidth;  // Allow zoom up to 200%
            double maxHeight = 2 * imgHeight;

            // Track bar value represents percentage of max size
            double scale = trackZoom.Value / 100.0;
            int newWidth = (int)(maxWidth * scale);
            int newHeight = (int)(maxHeight * scale);

            pictureBox.Width = newWidth;
            pictureBox.Height = newHeight;
            pictureBox.Left = 0;
            pictureBox.Top = 0;

            // Add bottom margin to accommodate horizontal scrollbar
            pictureBox.Margin = new Padding(0, 0, 0, SystemInformation.HorizontalScrollBarHeight);
        }

        /// <summary>
        /// Calculate initial zoom level to fit image in panel
        /// </summary>
        private void CalculateInitialZoom()
        {
            if (pictureBox.Image == null || panelImageView.Width == 0 || panelImageView.Height == 0)
                return;

            double imgWidth = pictureBox.Image.Width;
            double imgHeight = pictureBox.Image.Height;
            double panelWidth = panelImageView.Width;
            double panelHeight = panelImageView.Height;

            // Calculate scale to fit in panel
            double scaleWidth = panelWidth / imgWidth;
            double scaleHeight = panelHeight / imgHeight;
            double scale = Math.Min(scaleWidth, scaleHeight);

            // Convert to track bar value (percentage of max size which is 200%)
            double maxWidth = 2 * imgWidth;
            int trackValue = (int)((scale * imgWidth / maxWidth) * 100);
            trackValue = Math.Max(10, Math.Min(100, trackValue)); // Clamp to 10-100

            trackZoom.Value = trackValue;
        }

        /// <summary>
        /// Display image from file (encrypted or unencrypted)
        /// </summary>
        private void DisplayImage(string filePath)
        {
            try
            {
                // Hide video player, show image viewer
                mediaPlayer.Visible = false;
                panelVideoControls.Visible = false;
                pictureBox.Visible = true;

                if (vlcMediaPlayer != null && vlcMediaPlayer.IsPlaying)
                {
                    vlcMediaPlayer.Stop();
                }

                // Clear previous image
                if (pictureBox.Image != null)
                {
                    pictureBox.Image.Dispose();
                    pictureBox.Image = null;
                }

                string extension = Path.GetExtension(filePath).ToLower();
                byte[] imageData = null;
                MemoryStream ms = null;

                if (extension == ".ypg")
                {
                    // Decrypt old YPG file with DES
                    string ypgPassword = txtYpgPassword.Text;

                    if (string.IsNullOrWhiteSpace(ypgPassword))
                    {
                        MessageBox.Show("Please enter YPG password (DES key) to decrypt .ypg files.",
                            "Password Required", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }

                    imageData = YEncrypt.DecryptFileToBuffer(filePath, ypgPassword);

                    if (imageData == null || imageData.Length == 0)
                    {
                        throw new Exception("Decryption returned empty data. Check your YPG password.");
                    }

                    ms = new MemoryStream(imageData);
                    pictureBox.Image = new Bitmap(ms);

                    lblStatus.Text = $"Displayed (YPG/DES): {Path.GetFileName(filePath)}";
                }
                else if (extension == ".xpg")
                {
                    // Decrypt new XPG file with AES-256
                    string xpgPassword = txtXpgPassword.Text;

                    if (string.IsNullOrWhiteSpace(xpgPassword))
                    {
                        MessageBox.Show("Please enter XPG password (AES key) to decrypt .xpg files.",
                            "Password Required", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }

                    imageData = YAESEncrypt.DecryptFileToBuffer(filePath, xpgPassword);

                    if (imageData == null || imageData.Length == 0)
                    {
                        throw new Exception("Decryption returned empty data. Check your XPG password.");
                    }

                    ms = new MemoryStream(imageData);
                    pictureBox.Image = new Bitmap(ms);

                    lblStatus.Text = $"Displayed (XPG/AES): {Path.GetFileName(filePath)}";
                }
                else
                {
                    // Display unencrypted image directly
                    pictureBox.Image = Image.FromFile(filePath);
                    lblStatus.Text = $"Displayed (unencrypted): {Path.GetFileName(filePath)}";
                }

                // Reset scroll position to top-left before applying view mode
                panelImageView.AutoScrollPosition = new Point(0, 0);

                // Apply current view mode and calculate initial zoom
                if (rbZoom.Checked)
                {
                    CalculateInitialZoom();
                }
                ApplyViewMode();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error displaying image:\n\n{ex.Message}\n\nFile: {Path.GetFileName(filePath)}",
                    "Display Error", MessageBoxButtons.OK, MessageBoxIcon.Error);

                lblStatus.Text = $"Error: {ex.Message}";
            }
        }

        /// <summary>
        /// Encrypt files in a directory
        /// </summary>
        private void btnEncryptFiles_Click(object sender, EventArgs e)
        {
            // Show encryption dialog
            using (EncryptFilesDialog dialog = new EncryptFilesDialog(txtYpgPassword.Text, txtXpgPassword.Text))
            {
                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    lblStatus.Text = $"Encrypted {dialog.FilesProcessed} files";

                    // Reload current folder if encrypting in current directory
                    if (!string.IsNullOrEmpty(currentRootPath) && dialog.TargetDirectory.StartsWith(currentRootPath))
                    {
                        LoadFolder(currentRootPath);
                    }
                }
            }
        }

        /// <summary>
        /// Convert YPG files to XPG
        /// </summary>
        private void btnConvertYpgToXpg_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtYpgPassword.Text))
            {
                MessageBox.Show("Please enter YPG password (DES key) first.", "Password Required",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (string.IsNullOrWhiteSpace(txtXpgPassword.Text))
            {
                MessageBox.Show("Please enter XPG password (AES key) first.", "Password Required",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            using (FolderBrowserDialog dialog = new FolderBrowserDialog())
            {
                dialog.Description = "Select folder containing .ypg files to convert";

                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    DialogResult confirm = MessageBox.Show(
                        $"Convert all .ypg files in:\n{dialog.SelectedPath}\n\nInclude subdirectories?",
                        "Confirm Conversion",
                        MessageBoxButtons.YesNoCancel,
                        MessageBoxIcon.Question);

                    if (confirm == DialogResult.Cancel)
                        return;

                    bool includeSubdirs = (confirm == DialogResult.Yes);

                    // Convert with progress
                    ConvertYpgToXpgWithProgress(dialog.SelectedPath, includeSubdirs);
                }
            }
        }

        /// <summary>
        /// Convert YPG to XPG with progress dialog
        /// </summary>
        private async void ConvertYpgToXpgWithProgress(string directory, bool includeSubdirs)
        {
            using (ProgressDialog progressDlg = new ProgressDialog("Converting YPG to XPG"))
            {
                progressDlg.Show(this);
                progressDlg.SetStatus("Scanning for .ypg files...");

                try
                {
                    // Collect files
                    List<string> ypgFiles = await Task.Run(() =>
                    {
                        SearchOption searchOption = includeSubdirs ?
                            SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;
                        return new List<string>(Directory.GetFiles(directory, "*.ypg", searchOption));
                    });

                    if (ypgFiles.Count == 0)
                    {
                        progressDlg.Close();
                        MessageBox.Show("No .ypg files found in the selected folder.",
                            "No Files Found", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        return;
                    }

                    // Convert files
                    int count = await Task.Run(() =>
                    {
                        int converted = 0;
                        for (int i = 0; i < ypgFiles.Count; i++)
                        {
                            if (progressDlg.CancelRequested)
                                break;

                            string ypgFile = ypgFiles[i];
                            progressDlg.UpdateProgress(i + 1, ypgFiles.Count,
                                $"Converting: {Path.GetFileName(ypgFile)}");

                            string xpgFile = Path.Combine(
                                Path.GetDirectoryName(ypgFile),
                                Path.GetFileNameWithoutExtension(ypgFile) + ".xpg"
                            );

                            try
                            {
                                if (FileConverter.ConvertYPGtoXPG(ypgFile, xpgFile,
                                    txtYpgPassword.Text, txtXpgPassword.Text))
                                {
                                    converted++;
                                }
                            }
                            catch (Exception ex)
                            {
                                System.Diagnostics.Debug.WriteLine($"Error converting {ypgFile}: {ex.Message}");
                            }
                        }
                        return converted;
                    });

                    progressDlg.Close();

                    if (progressDlg.CancelRequested)
                    {
                        MessageBox.Show($"Conversion cancelled. {count} files converted.",
                            "Conversion Cancelled", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    else
                    {
                        MessageBox.Show($"Successfully converted {count} files from .ypg to .xpg",
                            "Conversion Complete", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }

                    lblStatus.Text = $"Converted {count} files";

                    // Reload if converted current folder
                    if (!string.IsNullOrEmpty(currentRootPath) &&
                        directory.StartsWith(currentRootPath))
                    {
                        LoadFolder(currentRootPath);
                    }
                }
                catch (Exception ex)
                {
                    progressDlg.Close();
                    MessageBox.Show($"Error during conversion:\n{ex.Message}",
                        "Conversion Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        /// <summary>
        /// Decrypt encrypted files to JPG
        /// </summary>
        private void btnDecryptToJpg_Click(object sender, EventArgs e)
        {
            using (DecryptFilesDialog dialog = new DecryptFilesDialog(txtYpgPassword.Text, txtXpgPassword.Text))
            {
                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    lblStatus.Text = $"Decrypted {dialog.FilesProcessed} files";
                }
            }
        }

        /// <summary>
        /// Generate a random AES-256 key
        /// </summary>
        private void btnGenerateXpgKey_Click(object sender, EventArgs e)
        {
            try
            {
                string newKey = KeyHelper.GenerateRandomKey();
                txtXpgPassword.Text = newKey;

                MessageBox.Show("Random AES-256 key generated!\n\nIMPORTANT: Save this key securely - you'll need it to decrypt your files.",
                    "Key Generated", MessageBoxButtons.OK, MessageBoxIcon.Information);

                lblStatus.Text = "Generated new random AES-256 key";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error generating key:\n{ex.Message}",
                    "Generation Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Derive AES-256 key from user's password
        /// </summary>
        private void btnDeriveXpgKey_Click(object sender, EventArgs e)
        {
            // Show input dialog
            using (Form inputForm = new Form())
            {
                inputForm.Text = "Derive Key from Password";
                inputForm.Width = 450;
                inputForm.Height = 260;
                inputForm.StartPosition = FormStartPosition.CenterParent;
                inputForm.FormBorderStyle = FormBorderStyle.FixedDialog;
                inputForm.MaximizeBox = false;
                inputForm.MinimizeBox = false;

                Label lblInfo = new Label()
                {
                    Left = 20,
                    Top = 20,
                    Width = 400,
                    Height = 40,
                    Text = "Enter your password. It will be converted to a valid AES-256 key.\nUse the same password each time to get the same key."
                };

                Label lblPassword = new Label()
                {
                    Left = 20,
                    Top = 70,
                    Width = 100,
                    Text = "Password:"
                };

                TextBox txtPassword = new TextBox()
                {
                    Left = 20,
                    Top = 90,
                    Width = 400,
                    UseSystemPasswordChar = true // Mask password for security
                };

                Label lblConfirm = new Label()
                {
                    Left = 20,
                    Top = 125,
                    Width = 150,
                    Text = "Confirm Password:"
                };

                TextBox txtConfirm = new TextBox()
                {
                    Left = 20,
                    Top = 145,
                    Width = 400,
                    UseSystemPasswordChar = true // Mask password for security
                };

                Button btnOK = new Button()
                {
                    Text = "Derive Key",
                    Left = 220,
                    Top = 185,
                    Width = 100,
                    DialogResult = DialogResult.None // Handle manually for validation
                };

                Button btnCancel = new Button()
                {
                    Text = "Cancel",
                    Left = 330,
                    Top = 185,
                    Width = 90,
                    DialogResult = DialogResult.Cancel
                };

                // OK button click handler with password matching validation
                btnOK.Click += (s, ev) =>
                {
                    string password = txtPassword.Text;
                    string confirm = txtConfirm.Text;

                    if (string.IsNullOrEmpty(password))
                    {
                        MessageBox.Show("Password cannot be empty.", "Empty Password",
                            MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        txtPassword.Focus();
                        return;
                    }

                    if (password != confirm)
                    {
                        MessageBox.Show("Passwords do not match. Please re-enter.", "Password Mismatch",
                            MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        txtPassword.Clear();
                        txtConfirm.Clear();
                        txtPassword.Focus();
                        return;
                    }

                    // Passwords match, proceed
                    inputForm.DialogResult = DialogResult.OK;
                    inputForm.Close();
                };

                inputForm.Controls.AddRange(new Control[] { lblInfo, lblPassword, txtPassword, lblConfirm, txtConfirm, btnOK, btnCancel });
                inputForm.AcceptButton = btnOK;
                inputForm.CancelButton = btnCancel;

                if (inputForm.ShowDialog() == DialogResult.OK)
                {
                    string password = txtPassword.Text;

                    try
                    {
                        string derivedKey = KeyHelper.DeriveKeyFromPassword(password);
                        txtXpgPassword.Text = derivedKey;

                        MessageBox.Show($"Key derived from your password!\n\n" +
                            $"IMPORTANT:\n" +
                            $"- Use the SAME password each time to get the same key\n" +
                            $"- Save your password securely\n" +
                            $"- Key length: {derivedKey.Length} characters",
                            "Key Derived", MessageBoxButtons.OK, MessageBoxIcon.Information);

                        lblStatus.Text = "Derived AES-256 key from password";
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Error deriving key:\n{ex.Message}",
                            "Derivation Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        /// <summary>
        /// Copy YPG password to clipboard
        /// </summary>
        private void btnCopyYpgPassword_Click(object sender, EventArgs e)
        {
            CopyPasswordToClipboard(txtYpgPassword.Text, "YPG");
        }

        /// <summary>
        /// Copy XPG password to clipboard
        /// </summary>
        private void btnCopyXpgPassword_Click(object sender, EventArgs e)
        {
            CopyPasswordToClipboard(txtXpgPassword.Text, "XPG");
        }

        /// <summary>
        /// Copy password to clipboard with confirmation dialog that clears clipboard on close
        /// </summary>
        private void CopyPasswordToClipboard(string password, string passwordType)
        {
            if (string.IsNullOrWhiteSpace(password))
            {
                MessageBox.Show($"The {passwordType} password field is empty. Nothing to copy.",
                    "Empty Password", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            try
            {
                // Copy to clipboard
                Clipboard.SetText(password);

                // Show confirmation dialog
                using (Form confirmDialog = new Form())
                {
                    confirmDialog.Text = "Password Copied";
                    confirmDialog.Width = 350;
                    confirmDialog.Height = 150;
                    confirmDialog.StartPosition = FormStartPosition.CenterParent;
                    confirmDialog.FormBorderStyle = FormBorderStyle.FixedDialog;
                    confirmDialog.MaximizeBox = false;
                    confirmDialog.MinimizeBox = false;

                    Label lblMessage = new Label()
                    {
                        Left = 20,
                        Top = 30,
                        Width = 300,
                        Height = 40,
                        Text = $"The {passwordType} password has been copied to clipboard."
                    };

                    Button btnOK = new Button()
                    {
                        Text = "OK",
                        Left = 115,
                        Top = 75,
                        Width = 100,
                        DialogResult = DialogResult.OK
                    };

                    confirmDialog.Controls.AddRange(new Control[] { lblMessage, btnOK });
                    confirmDialog.AcceptButton = btnOK;

                    // Show dialog
                    confirmDialog.ShowDialog();

                    // Clear clipboard after user clicks OK
                    Clipboard.Clear();

                    lblStatus.Text = $"{passwordType} password copied and clipboard cleared";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error copying to clipboard:\n{ex.Message}",
                    "Copy Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Toggle control panel expand/collapse
        /// </summary>
        private void btnTogglePanel_Click(object sender, EventArgs e)
        {
            isPanelExpanded = !isPanelExpanded;

            if (isPanelExpanded)
            {
                // Expand panel
                topPanel.Height = expandedHeight;
                btnTogglePanel.Text = "▲";

                // Show all controls
                lblFolderPath.Visible = true;
                txtFolderPath.Visible = true;
                btnBrowseFolder.Visible = true;
                btnLoadManualPath.Visible = true;
                grpPasswords.Visible = true;
                grpOperations.Visible = true;
            }
            else
            {
                // Collapse panel
                topPanel.Height = collapsedHeight;
                btnTogglePanel.Text = "▼";

                // Hide all controls except toggle button
                lblFolderPath.Visible = false;
                txtFolderPath.Visible = false;
                btnBrowseFolder.Visible = false;
                btnLoadManualPath.Visible = false;
                grpPasswords.Visible = false;
                grpOperations.Visible = false;
            }

            lblStatus.Text = isPanelExpanded ? "Control panel expanded" : "Control panel collapsed";
        }

        /// <summary>
        /// Play video file (encrypted or unencrypted)
        /// </summary>
        private void PlayVideo(string filePath)
        {
            try
            {
                // Hide image viewer, show video player
                pictureBox.Visible = false;
                mediaPlayer.Visible = true;
                panelVideoControls.Visible = true;
                trackVideoProgress.Value = trackVideoProgress.Minimum;
                lblVideoTime.Text = "00:00 / 00:00";
                btnPlayPause.Text = "Play";

                string extension = Path.GetExtension(filePath).ToLower();
                string tempVideoFile = null;

                if (extension == ".xpv")
                {
                    // Decrypt encrypted video file
                    string xpgPassword = txtXpgPassword.Text;

                    if (string.IsNullOrWhiteSpace(xpgPassword))
                    {
                        MessageBox.Show("Please enter XPG password (AES key) to decrypt .xpv files.",
                            "Password Required", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        pictureBox.Visible = true;
                        mediaPlayer.Visible = false;
                        panelVideoControls.Visible = false;
                        return;
                    }

                    lblStatus.Text = "Decrypting video...";
                    Application.DoEvents();

                    byte[] videoData = YAESEncrypt.DecryptFileToBuffer(filePath, xpgPassword);

                    if (videoData == null || videoData.Length == 0)
                    {
                        throw new Exception("Decryption returned empty data. Check your XPG password.");
                    }

                    // Save to temp file
                    tempVideoFile = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString() + ".mp4");
                    File.WriteAllBytes(tempVideoFile, videoData);

                    lblStatus.Text = $"Playing (XPV/AES): {Path.GetFileName(filePath)}";
                }
                else
                {
                    // Play unencrypted video directly
                    tempVideoFile = filePath;
                    lblStatus.Text = $"Playing (unencrypted): {Path.GetFileName(filePath)}";
                }

                // Clean up previous temp file
                CleanupTempVideoFile();
                currentTempVideoFile = tempVideoFile;

                // Load and play video to show first frame, then auto-pause
                // This allows the user to see the video content before playing
                isLoadingFirstFrame = true;
                using (var media = new Media(libVLC, tempVideoFile))
                {
                    vlcMediaPlayer.Play(media);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error playing video:\n\n{ex.Message}\n\nFile: {Path.GetFileName(filePath)}",
                    "Playback Error", MessageBoxButtons.OK, MessageBoxIcon.Error);

                pictureBox.Visible = true;
                mediaPlayer.Visible = false;
                panelVideoControls.Visible = false;
                lblStatus.Text = $"Error: {ex.Message}";
            }
        }

        /// <summary>
        /// Clean up temporary video file
        /// </summary>
        private void CleanupTempVideoFile()
        {
            if (!string.IsNullOrEmpty(currentTempVideoFile) && File.Exists(currentTempVideoFile))
            {
                try
                {
                    // SECURITY: Only delete if it's in temp folder (not an unencrypted video)
                    if (currentTempVideoFile.StartsWith(Path.GetTempPath()))
                    {
                        System.Diagnostics.Debug.WriteLine($"SECURITY: Securely erasing temp video file: {Path.GetFileName(currentTempVideoFile)}");

                        // Use Quick method (1 pass) for fast cleanup
                        if (!Services.SecureEraseHelper.SecureEraseFile(currentTempVideoFile, Services.EraseMethod.Quick))
                        {
                            System.Diagnostics.Debug.WriteLine("SECURITY WARNING: Secure erase failed, attempting regular delete");
                            try { File.Delete(currentTempVideoFile); } catch { }
                        }
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"SECURITY WARNING: Error securely erasing temp video file: {ex.Message}");
                }
            }
            currentTempVideoFile = null;
        }

        /// <summary>
        /// About menu click - show About dialog
        /// </summary>
        private void menuAbout_Click(object sender, EventArgs e)
        {
            using (AboutDialog aboutDialog = new AboutDialog())
            {
                aboutDialog.ShowDialog(this);
            }
        }

        /// <summary>
        /// Form closing - cleanup
        /// </summary>
        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            if (pictureBox.Image != null)
            {
                pictureBox.Image.Dispose();
                pictureBox.Image = null;
            }

            if (videoProgressTimer != null)
            {
                videoProgressTimer.Stop();
                videoProgressTimer.Dispose();
            }

            // Stop media player and cleanup temp files
            if (vlcMediaPlayer != null)
            {
                vlcMediaPlayer.Stop();
                vlcMediaPlayer.Dispose();
            }

            if (libVLC != null)
            {
                libVLC.Dispose();
            }

            CleanupTempVideoFile();

            base.OnFormClosing(e);
        }
    }
}
