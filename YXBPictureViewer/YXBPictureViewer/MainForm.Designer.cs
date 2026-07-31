namespace YXBPictureViewer
{
    partial class MainForm
    {
        private System.ComponentModel.IContainer components = null;
        private System.Windows.Forms.SplitContainer splitContainer;
        private System.Windows.Forms.TreeView tvFileExplorer;
        private System.Windows.Forms.Panel panelImageView;
        private System.Windows.Forms.PictureBox pictureBox;
        private LibVLCSharp.WinForms.VideoView mediaPlayer;
        private System.Windows.Forms.Panel panelVideoControls;
        private System.Windows.Forms.Button btnPlayPause;
        private System.Windows.Forms.TrackBar trackVideoProgress;
        private System.Windows.Forms.Label lblVideoTime;
        private System.Windows.Forms.TrackBar trackVolume;
        private System.Windows.Forms.Label lblVolume;
        private System.Windows.Forms.Panel panelViewControls;
        private System.Windows.Forms.GroupBox grpViewMode;
        private System.Windows.Forms.RadioButton rbZoom;
        private System.Windows.Forms.RadioButton rbStretchImage;
        private System.Windows.Forms.RadioButton rbNormal;
        private System.Windows.Forms.RadioButton rbAutoSize;
        private System.Windows.Forms.RadioButton rbCenterImage;
        private System.Windows.Forms.TrackBar trackZoom;
        private System.Windows.Forms.Label lblZoom;
        private System.Windows.Forms.Panel topPanel;
        private System.Windows.Forms.Label lblFolderPath;
        private System.Windows.Forms.TextBox txtFolderPath;
        private System.Windows.Forms.Button btnBrowseFolder;
        private System.Windows.Forms.Button btnLoadManualPath;
        private System.Windows.Forms.GroupBox grpPasswords;
        private System.Windows.Forms.Label lblYpgPassword;
        private System.Windows.Forms.TextBox txtYpgPassword;
        private System.Windows.Forms.Label lblXpgPassword;
        private System.Windows.Forms.TextBox txtXpgPassword;
        private System.Windows.Forms.Button btnGenerateXpgKey;
        private System.Windows.Forms.Button btnDeriveXpgKey;
        private System.Windows.Forms.Button btnCopyYpgPassword;
        private System.Windows.Forms.Button btnCopyXpgPassword;
        private System.Windows.Forms.Button btnTogglePanel;
        private System.Windows.Forms.MenuStrip menuStrip;
        private System.Windows.Forms.ToolStripMenuItem menuHelp;
        private System.Windows.Forms.ToolStripMenuItem menuAbout;
        private System.Windows.Forms.Panel bottomPanel;
        private System.Windows.Forms.StatusStrip statusStrip;
        private System.Windows.Forms.ToolStripStatusLabel lblStatus;
        private System.Windows.Forms.GroupBox grpOperations;
        private System.Windows.Forms.Button btnEncryptFiles;
        private System.Windows.Forms.Button btnConvertYpgToXpg;
        private System.Windows.Forms.Button btnDecryptToJpg;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            splitContainer = new System.Windows.Forms.SplitContainer();
            tvFileExplorer = new System.Windows.Forms.TreeView();
            panelImageView = new System.Windows.Forms.Panel();
            pictureBox = new System.Windows.Forms.PictureBox();
            mediaPlayer = new LibVLCSharp.WinForms.VideoView();
            panelVideoControls = new System.Windows.Forms.Panel();
            btnPlayPause = new System.Windows.Forms.Button();
            trackVideoProgress = new System.Windows.Forms.TrackBar();
            lblVideoTime = new System.Windows.Forms.Label();
            trackVolume = new System.Windows.Forms.TrackBar();
            lblVolume = new System.Windows.Forms.Label();
            panelViewControls = new System.Windows.Forms.Panel();
            grpViewMode = new System.Windows.Forms.GroupBox();
            rbCenterImage = new System.Windows.Forms.RadioButton();
            rbAutoSize = new System.Windows.Forms.RadioButton();
            rbNormal = new System.Windows.Forms.RadioButton();
            rbStretchImage = new System.Windows.Forms.RadioButton();
            rbZoom = new System.Windows.Forms.RadioButton();
            lblZoom = new System.Windows.Forms.Label();
            trackZoom = new System.Windows.Forms.TrackBar();
            topPanel = new System.Windows.Forms.Panel();
            btnTogglePanel = new System.Windows.Forms.Button();
            menuStrip = new System.Windows.Forms.MenuStrip();
            menuHelp = new System.Windows.Forms.ToolStripMenuItem();
            menuAbout = new System.Windows.Forms.ToolStripMenuItem();
            grpOperations = new System.Windows.Forms.GroupBox();
            btnDecryptToJpg = new System.Windows.Forms.Button();
            btnConvertYpgToXpg = new System.Windows.Forms.Button();
            btnEncryptFiles = new System.Windows.Forms.Button();
            grpPasswords = new System.Windows.Forms.GroupBox();
            btnCopyXpgPassword = new System.Windows.Forms.Button();
            btnCopyYpgPassword = new System.Windows.Forms.Button();
            btnDeriveXpgKey = new System.Windows.Forms.Button();
            btnGenerateXpgKey = new System.Windows.Forms.Button();
            lblXpgPassword = new System.Windows.Forms.Label();
            txtXpgPassword = new System.Windows.Forms.TextBox();
            lblYpgPassword = new System.Windows.Forms.Label();
            txtYpgPassword = new System.Windows.Forms.TextBox();
            btnLoadManualPath = new System.Windows.Forms.Button();
            btnBrowseFolder = new System.Windows.Forms.Button();
            txtFolderPath = new System.Windows.Forms.TextBox();
            lblFolderPath = new System.Windows.Forms.Label();
            bottomPanel = new System.Windows.Forms.Panel();
            statusStrip = new System.Windows.Forms.StatusStrip();
            lblStatus = new System.Windows.Forms.ToolStripStatusLabel();
            ((System.ComponentModel.ISupportInitialize)splitContainer).BeginInit();
            splitContainer.Panel1.SuspendLayout();
            splitContainer.Panel2.SuspendLayout();
            splitContainer.SuspendLayout();
            panelImageView.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)pictureBox).BeginInit();
            ((System.ComponentModel.ISupportInitialize)mediaPlayer).BeginInit();
            panelVideoControls.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)trackVideoProgress).BeginInit();
            panelViewControls.SuspendLayout();
            grpViewMode.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)trackZoom).BeginInit();
            topPanel.SuspendLayout();
            grpOperations.SuspendLayout();
            grpPasswords.SuspendLayout();
            bottomPanel.SuspendLayout();
            statusStrip.SuspendLayout();
            SuspendLayout();
            // 
            // splitContainer
            // 
            splitContainer.Dock = System.Windows.Forms.DockStyle.Fill;
            splitContainer.Location = new System.Drawing.Point(0, 180);
            splitContainer.Name = "splitContainer";
            //
            // splitContainer.Panel1
            //
            splitContainer.Panel1.Controls.Add(tvFileExplorer);
            splitContainer.Panel1.Padding = new System.Windows.Forms.Padding(10, 5, 5, 5);
            //
            // splitContainer.Panel2
            //
            splitContainer.Panel2.Controls.Add(panelImageView);
            splitContainer.Panel2.Controls.Add(panelViewControls);
            splitContainer.Panel2.Padding = new System.Windows.Forms.Padding(5, 5, 10, 5);
            splitContainer.Size = new System.Drawing.Size(1200, 520);
            splitContainer.SplitterDistance = 350;
            splitContainer.TabIndex = 0;
            //
            // tvFileExplorer
            //
            tvFileExplorer.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            tvFileExplorer.Dock = System.Windows.Forms.DockStyle.Fill;
            tvFileExplorer.Location = new System.Drawing.Point(10, 5);
            tvFileExplorer.Name = "tvFileExplorer";
            tvFileExplorer.Size = new System.Drawing.Size(335, 510);
            tvFileExplorer.TabIndex = 0;
            tvFileExplorer.AfterSelect += tvFileExplorer_AfterSelect;
            //
            // panelImageView
            //
            panelImageView.AutoScroll = true;
            panelImageView.AutoScrollMargin = new System.Drawing.Size(0, 0);
            panelImageView.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            panelImageView.Controls.Add(pictureBox);
            panelImageView.Controls.Add(mediaPlayer);
            panelImageView.Controls.Add(panelVideoControls);
            panelImageView.Dock = System.Windows.Forms.DockStyle.Fill;
            panelImageView.Location = new System.Drawing.Point(5, 65);
            panelImageView.Name = "panelImageView";
            panelImageView.Size = new System.Drawing.Size(831, 450);
            panelImageView.TabIndex = 1;
            //
            // pictureBox
            //
            pictureBox.BorderStyle = System.Windows.Forms.BorderStyle.None;
            pictureBox.Location = new System.Drawing.Point(0, 0);
            pictureBox.Name = "pictureBox";
            pictureBox.Size = new System.Drawing.Size(846, 460);
            pictureBox.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            pictureBox.TabIndex = 0;
            pictureBox.TabStop = false;
            //
            // mediaPlayer
            //
            mediaPlayer.Dock = System.Windows.Forms.DockStyle.Fill;
            mediaPlayer.Location = new System.Drawing.Point(0, 0);
            mediaPlayer.Name = "mediaPlayer";
            mediaPlayer.Size = new System.Drawing.Size(829, 448);
            mediaPlayer.TabIndex = 1;
            mediaPlayer.Visible = false;
            mediaPlayer.BackColor = System.Drawing.Color.Black;
            //
            // panelVideoControls
            //
            panelVideoControls.BackColor = System.Drawing.Color.FromArgb(64, 64, 64);
            panelVideoControls.Controls.Add(lblVideoTime);
            panelVideoControls.Controls.Add(trackVideoProgress);
            panelVideoControls.Controls.Add(lblVolume);
            panelVideoControls.Controls.Add(trackVolume);
            panelVideoControls.Controls.Add(btnPlayPause);
            panelVideoControls.Dock = System.Windows.Forms.DockStyle.Bottom;
            panelVideoControls.Location = new System.Drawing.Point(0, 398);
            panelVideoControls.Name = "panelVideoControls";
            panelVideoControls.Size = new System.Drawing.Size(829, 50);
            panelVideoControls.TabIndex = 2;
            panelVideoControls.Visible = false;
            //
            // btnPlayPause
            //
            btnPlayPause.BackColor = System.Drawing.Color.White;
            btnPlayPause.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Bold);
            btnPlayPause.Location = new System.Drawing.Point(10, 10);
            btnPlayPause.Name = "btnPlayPause";
            btnPlayPause.Size = new System.Drawing.Size(80, 30);
            btnPlayPause.TabIndex = 0;
            btnPlayPause.Text = "▶ Play";
            btnPlayPause.UseVisualStyleBackColor = false;
            btnPlayPause.Click += btnPlayPause_Click;
            //
            // trackVideoProgress
            //
            trackVideoProgress.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) | System.Windows.Forms.AnchorStyles.Right)));
            trackVideoProgress.Location = new System.Drawing.Point(100, 15);
            trackVideoProgress.Maximum = 1000;
            trackVideoProgress.Name = "trackVideoProgress";
            trackVideoProgress.Size = new System.Drawing.Size(500, 45);
            trackVideoProgress.TabIndex = 1;
            trackVideoProgress.TickStyle = System.Windows.Forms.TickStyle.None;
            trackVideoProgress.Scroll += trackVideoProgress_Scroll;
            trackVideoProgress.MouseDown += trackVideoProgress_MouseDown;
            trackVideoProgress.MouseUp += trackVideoProgress_MouseUp;
            //
            // lblVideoTime
            //
            lblVideoTime.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            lblVideoTime.ForeColor = System.Drawing.Color.White;
            lblVideoTime.Location = new System.Drawing.Point(610, 15);
            lblVideoTime.Name = "lblVideoTime";
            lblVideoTime.Size = new System.Drawing.Size(100, 20);
            lblVideoTime.TabIndex = 2;
            lblVideoTime.Text = "00:00 / 00:00";
            lblVideoTime.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            //
            // trackVolume
            //
            trackVolume.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            trackVolume.Location = new System.Drawing.Point(740, 15);
            trackVolume.Maximum = 100;
            trackVolume.Name = "trackVolume";
            trackVolume.Size = new System.Drawing.Size(60, 45);
            trackVolume.TabIndex = 3;
            trackVolume.TickStyle = System.Windows.Forms.TickStyle.None;
            trackVolume.Value = 50;
            trackVolume.Scroll += trackVolume_Scroll;
            //
            // lblVolume
            //
            lblVolume.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            lblVolume.ForeColor = System.Drawing.Color.White;
            lblVolume.Location = new System.Drawing.Point(715, 15);
            lblVolume.Name = "lblVolume";
            lblVolume.Size = new System.Drawing.Size(25, 20);
            lblVolume.TabIndex = 4;
            lblVolume.Text = "🔊";
            lblVolume.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            panelVideoControls.Controls.Add(trackVideoProgress);
            panelVideoControls.Controls.Add(lblVideoTime);
            panelVideoControls.Dock = System.Windows.Forms.DockStyle.Bottom;
            panelVideoControls.Location = new System.Drawing.Point(0, 408);
            panelVideoControls.Name = "panelVideoControls";
            panelVideoControls.Size = new System.Drawing.Size(829, 40);
            panelVideoControls.TabIndex = 2;
            panelVideoControls.Visible = false;
            //
            // btnPlayPause
            //
            btnPlayPause.Location = new System.Drawing.Point(5, 5);
            btnPlayPause.Name = "btnPlayPause";
            btnPlayPause.Size = new System.Drawing.Size(75, 30);
            btnPlayPause.TabIndex = 0;
            btnPlayPause.Text = "Play";
            btnPlayPause.UseVisualStyleBackColor = true;
            btnPlayPause.Click += btnPlayPause_Click;
            //
            // trackVideoProgress
            //
            trackVideoProgress.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            trackVideoProgress.LargeChange = 10;
            trackVideoProgress.Location = new System.Drawing.Point(85, 0);
            trackVideoProgress.Maximum = 1000;
            trackVideoProgress.Name = "trackVideoProgress";
            trackVideoProgress.Size = new System.Drawing.Size(650, 40);
            trackVideoProgress.TabIndex = 1;
            trackVideoProgress.TickStyle = System.Windows.Forms.TickStyle.None;
            trackVideoProgress.MouseDown += trackVideoProgress_MouseDown;
            trackVideoProgress.MouseUp += trackVideoProgress_MouseUp;
            trackVideoProgress.Scroll += trackVideoProgress_Scroll;
            //
            // lblVideoTime
            //
            lblVideoTime.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
            lblVideoTime.AutoSize = true;
            lblVideoTime.Location = new System.Drawing.Point(745, 12);
            lblVideoTime.Name = "lblVideoTime";
            lblVideoTime.Size = new System.Drawing.Size(70, 15);
            lblVideoTime.TabIndex = 2;
            lblVideoTime.Text = "00:00 / 00:00";
            //
            // panelViewControls
            //
            panelViewControls.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            panelViewControls.Controls.Add(grpViewMode);
            panelViewControls.Controls.Add(lblZoom);
            panelViewControls.Controls.Add(trackZoom);
            panelViewControls.Dock = System.Windows.Forms.DockStyle.Top;
            panelViewControls.Location = new System.Drawing.Point(0, 0);
            panelViewControls.Name = "panelViewControls";
            panelViewControls.Size = new System.Drawing.Size(846, 60);
            panelViewControls.TabIndex = 0;
            // 
            // grpViewMode
            // 
            grpViewMode.Controls.Add(rbCenterImage);
            grpViewMode.Controls.Add(rbAutoSize);
            grpViewMode.Controls.Add(rbNormal);
            grpViewMode.Controls.Add(rbStretchImage);
            grpViewMode.Controls.Add(rbZoom);
            grpViewMode.Location = new System.Drawing.Point(10, 5);
            grpViewMode.Name = "grpViewMode";
            grpViewMode.Size = new System.Drawing.Size(450, 50);
            grpViewMode.TabIndex = 0;
            grpViewMode.TabStop = false;
            grpViewMode.Text = "View Mode";
            // 
            // rbCenterImage
            // 
            rbCenterImage.AutoSize = true;
            rbCenterImage.Location = new System.Drawing.Point(340, 20);
            rbCenterImage.Name = "rbCenterImage";
            rbCenterImage.Size = new System.Drawing.Size(96, 19);
            rbCenterImage.TabIndex = 4;
            rbCenterImage.Text = "Center Image";
            rbCenterImage.UseVisualStyleBackColor = true;
            rbCenterImage.CheckedChanged += ViewMode_CheckedChanged;
            // 
            // rbAutoSize
            // 
            rbAutoSize.AutoSize = true;
            rbAutoSize.Location = new System.Drawing.Point(250, 20);
            rbAutoSize.Name = "rbAutoSize";
            rbAutoSize.Size = new System.Drawing.Size(74, 19);
            rbAutoSize.TabIndex = 3;
            rbAutoSize.Text = "Auto Size";
            rbAutoSize.UseVisualStyleBackColor = true;
            rbAutoSize.CheckedChanged += ViewMode_CheckedChanged;
            // 
            // rbNormal
            // 
            rbNormal.AutoSize = true;
            rbNormal.Location = new System.Drawing.Point(180, 20);
            rbNormal.Name = "rbNormal";
            rbNormal.Size = new System.Drawing.Size(65, 19);
            rbNormal.TabIndex = 2;
            rbNormal.Text = "Normal";
            rbNormal.UseVisualStyleBackColor = true;
            rbNormal.CheckedChanged += ViewMode_CheckedChanged;
            // 
            // rbStretchImage
            // 
            rbStretchImage.AutoSize = true;
            rbStretchImage.Location = new System.Drawing.Point(75, 20);
            rbStretchImage.Name = "rbStretchImage";
            rbStretchImage.Size = new System.Drawing.Size(98, 19);
            rbStretchImage.TabIndex = 1;
            rbStretchImage.Text = "Stretch Image";
            rbStretchImage.UseVisualStyleBackColor = true;
            rbStretchImage.CheckedChanged += ViewMode_CheckedChanged;
            // 
            // rbZoom
            // 
            rbZoom.AutoSize = true;
            rbZoom.Checked = true;
            rbZoom.Location = new System.Drawing.Point(15, 20);
            rbZoom.Name = "rbZoom";
            rbZoom.Size = new System.Drawing.Size(57, 19);
            rbZoom.TabIndex = 0;
            rbZoom.TabStop = true;
            rbZoom.Text = "Zoom";
            rbZoom.UseVisualStyleBackColor = true;
            rbZoom.CheckedChanged += ViewMode_CheckedChanged;
            // 
            // lblZoom
            // 
            lblZoom.AutoSize = true;
            lblZoom.Location = new System.Drawing.Point(470, 10);
            lblZoom.Name = "lblZoom";
            lblZoom.Size = new System.Drawing.Size(42, 15);
            lblZoom.TabIndex = 2;
            lblZoom.Text = "Zoom:";
            lblZoom.Visible = false;
            // 
            // trackZoom
            // 
            trackZoom.Location = new System.Drawing.Point(470, 28);
            trackZoom.Maximum = 100;
            trackZoom.Minimum = 10;
            trackZoom.Name = "trackZoom";
            trackZoom.Size = new System.Drawing.Size(350, 45);
            trackZoom.TabIndex = 1;
            trackZoom.TickFrequency = 10;
            trackZoom.Value = 50;
            trackZoom.Visible = false;
            trackZoom.Scroll += trackZoom_Scroll;
            // 
            // topPanel
            // 
            topPanel.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            topPanel.Controls.Add(btnTogglePanel);
            topPanel.Controls.Add(grpOperations);
            topPanel.Controls.Add(grpPasswords);
            topPanel.Controls.Add(btnLoadManualPath);
            topPanel.Controls.Add(btnBrowseFolder);
            topPanel.Controls.Add(txtFolderPath);
            topPanel.Controls.Add(lblFolderPath);
            topPanel.Dock = System.Windows.Forms.DockStyle.Top;
            topPanel.Location = new System.Drawing.Point(0, 0);
            topPanel.Name = "topPanel";
            topPanel.Padding = new System.Windows.Forms.Padding(10);
            topPanel.Size = new System.Drawing.Size(1200, 180);
            topPanel.TabIndex = 1;
            // 
            // btnTogglePanel
            // 
            btnTogglePanel.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
            btnTogglePanel.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            btnTogglePanel.Location = new System.Drawing.Point(1146, 10);
            btnTogglePanel.Name = "btnTogglePanel";
            btnTogglePanel.Size = new System.Drawing.Size(30, 25);
            btnTogglePanel.TabIndex = 6;
            btnTogglePanel.Text = "▲";
            btnTogglePanel.UseVisualStyleBackColor = true;
            btnTogglePanel.Click += btnTogglePanel_Click;
            // 
            // grpOperations
            // 
            grpOperations.Controls.Add(btnDecryptToJpg);
            grpOperations.Controls.Add(btnConvertYpgToXpg);
            grpOperations.Controls.Add(btnEncryptFiles);
            grpOperations.Location = new System.Drawing.Point(780, 10);
            grpOperations.Name = "grpOperations";
            grpOperations.Size = new System.Drawing.Size(400, 160);
            grpOperations.TabIndex = 5;
            grpOperations.TabStop = false;
            grpOperations.Text = "Operations";
            // 
            // btnDecryptToJpg
            // 
            btnDecryptToJpg.Location = new System.Drawing.Point(15, 110);
            btnDecryptToJpg.Name = "btnDecryptToJpg";
            btnDecryptToJpg.Size = new System.Drawing.Size(370, 35);
            btnDecryptToJpg.TabIndex = 2;
            btnDecryptToJpg.Text = "Decrypt Files to JPG";
            btnDecryptToJpg.UseVisualStyleBackColor = true;
            btnDecryptToJpg.Click += btnDecryptToJpg_Click;
            // 
            // btnConvertYpgToXpg
            // 
            btnConvertYpgToXpg.Location = new System.Drawing.Point(15, 65);
            btnConvertYpgToXpg.Name = "btnConvertYpgToXpg";
            btnConvertYpgToXpg.Size = new System.Drawing.Size(370, 35);
            btnConvertYpgToXpg.TabIndex = 1;
            btnConvertYpgToXpg.Text = "Convert YPG → XPG";
            btnConvertYpgToXpg.UseVisualStyleBackColor = true;
            btnConvertYpgToXpg.Click += btnConvertYpgToXpg_Click;
            // 
            // btnEncryptFiles
            // 
            btnEncryptFiles.Location = new System.Drawing.Point(15, 20);
            btnEncryptFiles.Name = "btnEncryptFiles";
            btnEncryptFiles.Size = new System.Drawing.Size(370, 35);
            btnEncryptFiles.TabIndex = 0;
            btnEncryptFiles.Text = "Encrypt Files";
            btnEncryptFiles.UseVisualStyleBackColor = true;
            btnEncryptFiles.Click += btnEncryptFiles_Click;
            // 
            // grpPasswords
            // 
            grpPasswords.Controls.Add(btnCopyXpgPassword);
            grpPasswords.Controls.Add(btnCopyYpgPassword);
            grpPasswords.Controls.Add(btnDeriveXpgKey);
            grpPasswords.Controls.Add(btnGenerateXpgKey);
            grpPasswords.Controls.Add(lblXpgPassword);
            grpPasswords.Controls.Add(txtXpgPassword);
            grpPasswords.Controls.Add(lblYpgPassword);
            grpPasswords.Controls.Add(txtYpgPassword);
            grpPasswords.Location = new System.Drawing.Point(380, 10);
            grpPasswords.Name = "grpPasswords";
            grpPasswords.Size = new System.Drawing.Size(380, 160);
            grpPasswords.TabIndex = 4;
            grpPasswords.TabStop = false;
            grpPasswords.Text = "Passwords / Keys";
            // 
            // btnCopyXpgPassword
            // 
            btnCopyXpgPassword.Location = new System.Drawing.Point(305, 105);
            btnCopyXpgPassword.Name = "btnCopyXpgPassword";
            btnCopyXpgPassword.Size = new System.Drawing.Size(60, 23);
            btnCopyXpgPassword.TabIndex = 7;
            btnCopyXpgPassword.Text = "Copy";
            btnCopyXpgPassword.UseVisualStyleBackColor = true;
            btnCopyXpgPassword.Click += btnCopyXpgPassword_Click;
            // 
            // btnCopyYpgPassword
            // 
            btnCopyYpgPassword.Location = new System.Drawing.Point(305, 50);
            btnCopyYpgPassword.Name = "btnCopyYpgPassword";
            btnCopyYpgPassword.Size = new System.Drawing.Size(60, 23);
            btnCopyYpgPassword.TabIndex = 6;
            btnCopyYpgPassword.Text = "Copy";
            btnCopyYpgPassword.UseVisualStyleBackColor = true;
            btnCopyYpgPassword.Click += btnCopyYpgPassword_Click;
            // 
            // btnDeriveXpgKey
            // 
            btnDeriveXpgKey.Location = new System.Drawing.Point(200, 135);
            btnDeriveXpgKey.Name = "btnDeriveXpgKey";
            btnDeriveXpgKey.Size = new System.Drawing.Size(165, 20);
            btnDeriveXpgKey.TabIndex = 5;
            btnDeriveXpgKey.Text = "From Password...";
            btnDeriveXpgKey.UseVisualStyleBackColor = true;
            btnDeriveXpgKey.Click += btnDeriveXpgKey_Click;
            // 
            // btnGenerateXpgKey
            // 
            btnGenerateXpgKey.Location = new System.Drawing.Point(15, 135);
            btnGenerateXpgKey.Name = "btnGenerateXpgKey";
            btnGenerateXpgKey.Size = new System.Drawing.Size(175, 20);
            btnGenerateXpgKey.TabIndex = 4;
            btnGenerateXpgKey.Text = "Generate Random";
            btnGenerateXpgKey.UseVisualStyleBackColor = true;
            btnGenerateXpgKey.Click += btnGenerateXpgKey_Click;
            // 
            // lblXpgPassword
            // 
            lblXpgPassword.AutoSize = true;
            lblXpgPassword.Location = new System.Drawing.Point(15, 85);
            lblXpgPassword.Name = "lblXpgPassword";
            lblXpgPassword.Size = new System.Drawing.Size(228, 15);
            lblXpgPassword.TabIndex = 3;
            lblXpgPassword.Text = "XPG Password (AES-256 Key or your own):";
            // 
            // txtXpgPassword
            // 
            txtXpgPassword.Location = new System.Drawing.Point(15, 105);
            txtXpgPassword.Name = "txtXpgPassword";
            txtXpgPassword.Size = new System.Drawing.Size(280, 23);
            txtXpgPassword.TabIndex = 2;
            txtXpgPassword.UseSystemPasswordChar = true;
            // 
            // lblYpgPassword
            // 
            lblYpgPassword.AutoSize = true;
            lblYpgPassword.Location = new System.Drawing.Point(15, 30);
            lblYpgPassword.Name = "lblYpgPassword";
            lblYpgPassword.Size = new System.Drawing.Size(138, 15);
            lblYpgPassword.TabIndex = 1;
            lblYpgPassword.Text = "YPG Password (DES Key):";
            // 
            // txtYpgPassword
            // 
            txtYpgPassword.Location = new System.Drawing.Point(15, 50);
            txtYpgPassword.Name = "txtYpgPassword";
            txtYpgPassword.Size = new System.Drawing.Size(280, 23);
            txtYpgPassword.TabIndex = 0;
            txtYpgPassword.UseSystemPasswordChar = true;
            // 
            // btnLoadManualPath
            // 
            btnLoadManualPath.Location = new System.Drawing.Point(250, 120);
            btnLoadManualPath.Name = "btnLoadManualPath";
            btnLoadManualPath.Size = new System.Drawing.Size(100, 35);
            btnLoadManualPath.TabIndex = 3;
            btnLoadManualPath.Text = "Load Path";
            btnLoadManualPath.UseVisualStyleBackColor = true;
            btnLoadManualPath.Click += btnLoadManualPath_Click;
            // 
            // btnBrowseFolder
            // 
            btnBrowseFolder.Location = new System.Drawing.Point(15, 120);
            btnBrowseFolder.Name = "btnBrowseFolder";
            btnBrowseFolder.Size = new System.Drawing.Size(220, 35);
            btnBrowseFolder.TabIndex = 2;
            btnBrowseFolder.Text = "Browse Folder...";
            btnBrowseFolder.UseVisualStyleBackColor = true;
            btnBrowseFolder.Click += btnBrowseFolder_Click;
            // 
            // txtFolderPath
            // 
            txtFolderPath.Location = new System.Drawing.Point(15, 80);
            txtFolderPath.Name = "txtFolderPath";
            txtFolderPath.Size = new System.Drawing.Size(335, 23);
            txtFolderPath.TabIndex = 1;
            // 
            // lblFolderPath
            // 
            lblFolderPath.AutoSize = true;
            lblFolderPath.Location = new System.Drawing.Point(15, 55);
            lblFolderPath.Name = "lblFolderPath";
            lblFolderPath.Size = new System.Drawing.Size(299, 15);
            lblFolderPath.TabIndex = 0;
            lblFolderPath.Text = "Folder Path (or browse/paste path and click Load Path):";
            // 
            // bottomPanel
            // 
            bottomPanel.Controls.Add(statusStrip);
            bottomPanel.Dock = System.Windows.Forms.DockStyle.Bottom;
            bottomPanel.Location = new System.Drawing.Point(0, 700);
            bottomPanel.Name = "bottomPanel";
            bottomPanel.Size = new System.Drawing.Size(1200, 25);
            bottomPanel.TabIndex = 2;
            // 
            // statusStrip
            // 
            statusStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] { lblStatus });
            statusStrip.Location = new System.Drawing.Point(0, 3);
            statusStrip.Name = "statusStrip";
            statusStrip.Size = new System.Drawing.Size(1200, 22);
            statusStrip.TabIndex = 0;
            statusStrip.Text = "statusStrip1";
            // 
            // lblStatus
            // 
            lblStatus.Name = "lblStatus";
            lblStatus.Size = new System.Drawing.Size(39, 17);
            lblStatus.Text = "Ready";
            // 
            //
            // menuStrip
            //
            menuStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] { menuHelp });
            menuStrip.Location = new System.Drawing.Point(0, 0);
            menuStrip.Name = "menuStrip";
            menuStrip.Size = new System.Drawing.Size(1200, 24);
            menuStrip.TabIndex = 0;
            menuStrip.Text = "menuStrip";
            //
            // menuHelp
            //
            menuHelp.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] { menuAbout });
            menuHelp.Name = "menuHelp";
            menuHelp.Size = new System.Drawing.Size(44, 20);
            menuHelp.Text = "&Help";
            //
            // menuAbout
            //
            menuAbout.Name = "menuAbout";
            menuAbout.Size = new System.Drawing.Size(107, 22);
            menuAbout.Text = "&About";
            menuAbout.Click += menuAbout_Click;
            // MainForm
            //
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            ClientSize = new System.Drawing.Size(1200, 725);
            Controls.Add(splitContainer);
            Controls.Add(topPanel);
            Controls.Add(bottomPanel);
            Controls.Add(menuStrip);
            MainMenuStrip = menuStrip;
            Name = "MainForm";
            StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            Text = "YXB Picture Viewer - Encrypted Image Manager";
            Icon = new System.Drawing.Icon("appicon.ico");
            splitContainer.Panel1.ResumeLayout(false);
            splitContainer.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)splitContainer).EndInit();
            splitContainer.ResumeLayout(false);
            panelImageView.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)pictureBox).EndInit();
            ((System.ComponentModel.ISupportInitialize)mediaPlayer).EndInit();
            panelVideoControls.ResumeLayout(false);
            panelVideoControls.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)trackVideoProgress).EndInit();
            panelViewControls.ResumeLayout(false);
            panelViewControls.PerformLayout();
            grpViewMode.ResumeLayout(false);
            grpViewMode.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)trackZoom).EndInit();
            topPanel.ResumeLayout(false);
            topPanel.PerformLayout();
            grpOperations.ResumeLayout(false);
            grpPasswords.ResumeLayout(false);
            grpPasswords.PerformLayout();
            bottomPanel.ResumeLayout(false);
            bottomPanel.PerformLayout();
            statusStrip.ResumeLayout(false);
            statusStrip.PerformLayout();
            menuStrip.ResumeLayout(false);
            menuStrip.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }
    }
}
