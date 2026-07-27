namespace YXBPictureViewer
{
    partial class Form1
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form1));
            this.bLoad = new System.Windows.Forms.Button();
            this.pBPic = new System.Windows.Forms.PictureBox();
            this.openFileDialog1 = new System.Windows.Forms.OpenFileDialog();
            this.bEncryptDirectory = new System.Windows.Forms.Button();
            this.folderBrowserDialog1 = new System.Windows.Forms.FolderBrowserDialog();
            this.panel1 = new System.Windows.Forms.Panel();
            this.trackScale = new System.Windows.Forms.TrackBar();
            this.cbOnlyYPG = new System.Windows.Forms.CheckBox();
            this.bClear = new System.Windows.Forms.Button();
            this.bPrevious = new System.Windows.Forms.Button();
            this.bNext = new System.Windows.Forms.Button();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.rbZoom = new System.Windows.Forms.RadioButton();
            this.rbCenterImage = new System.Windows.Forms.RadioButton();
            this.rbAutoSize = new System.Windows.Forms.RadioButton();
            this.rbStretchImage = new System.Windows.Forms.RadioButton();
            this.rbNormal = new System.Windows.Forms.RadioButton();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.cbDeleteOriginal = new System.Windows.Forms.CheckBox();
            this.label1 = new System.Windows.Forms.Label();
            this.cbFileExtentions = new System.Windows.Forms.ComboBox();
            this.cbIncludeSub = new System.Windows.Forms.CheckBox();
            this.panel2 = new System.Windows.Forms.Panel();
            this.panelPic = new System.Windows.Forms.Panel();
            this.splitter1 = new System.Windows.Forms.Splitter();
            this.panel3 = new System.Windows.Forms.Panel();
            this.tvFolder = new System.Windows.Forms.TreeView();
            this.imageList1 = new System.Windows.Forms.ImageList(this.components);
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.systemToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.loginToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.logoutToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.changePasswordToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.exitToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.bDecrypt = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.pBPic)).BeginInit();
            this.panel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.trackScale)).BeginInit();
            this.groupBox2.SuspendLayout();
            this.groupBox1.SuspendLayout();
            this.panel2.SuspendLayout();
            this.panelPic.SuspendLayout();
            this.panel3.SuspendLayout();
            this.menuStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // bLoad
            // 
            this.bLoad.Location = new System.Drawing.Point(12, 32);
            this.bLoad.Name = "bLoad";
            this.bLoad.Size = new System.Drawing.Size(55, 23);
            this.bLoad.TabIndex = 0;
            this.bLoad.Text = "Load";
            this.bLoad.UseVisualStyleBackColor = true;
            this.bLoad.Click += new System.EventHandler(this.button1_Click);
            // 
            // pBPic
            // 
            this.pBPic.Location = new System.Drawing.Point(0, 3);
            this.pBPic.Name = "pBPic";
            this.pBPic.Size = new System.Drawing.Size(493, 356);
            this.pBPic.SizeMode = System.Windows.Forms.PictureBoxSizeMode.AutoSize;
            this.pBPic.TabIndex = 1;
            this.pBPic.TabStop = false;
            // 
            // openFileDialog1
            // 
            this.openFileDialog1.FileName = "openFileDialog1";
            this.openFileDialog1.Filter = "Jpeg files|*.jpg|All files|*.*";
            // 
            // bEncryptDirectory
            // 
            this.bEncryptDirectory.Location = new System.Drawing.Point(152, 53);
            this.bEncryptDirectory.Name = "bEncryptDirectory";
            this.bEncryptDirectory.Size = new System.Drawing.Size(96, 23);
            this.bEncryptDirectory.TabIndex = 2;
            this.bEncryptDirectory.Text = "Encript Directory";
            this.bEncryptDirectory.UseVisualStyleBackColor = true;
            this.bEncryptDirectory.Click += new System.EventHandler(this.bEncryptDirectory_Click);
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.bDecrypt);
            this.panel1.Controls.Add(this.trackScale);
            this.panel1.Controls.Add(this.cbOnlyYPG);
            this.panel1.Controls.Add(this.bClear);
            this.panel1.Controls.Add(this.bPrevious);
            this.panel1.Controls.Add(this.bNext);
            this.panel1.Controls.Add(this.groupBox2);
            this.panel1.Controls.Add(this.groupBox1);
            this.panel1.Controls.Add(this.bLoad);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.panel1.Location = new System.Drawing.Point(0, 365);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(871, 100);
            this.panel1.TabIndex = 3;
            // 
            // trackScale
            // 
            this.trackScale.Location = new System.Drawing.Point(263, 24);
            this.trackScale.Maximum = 100;
            this.trackScale.Name = "trackScale";
            this.trackScale.Size = new System.Drawing.Size(188, 45);
            this.trackScale.TabIndex = 9;
            this.trackScale.Visible = false;
            this.trackScale.Scroll += new System.EventHandler(this.trackScale_Scroll);
            // 
            // cbOnlyYPG
            // 
            this.cbOnlyYPG.AutoSize = true;
            this.cbOnlyYPG.Location = new System.Drawing.Point(16, 10);
            this.cbOnlyYPG.Name = "cbOnlyYPG";
            this.cbOnlyYPG.Size = new System.Drawing.Size(95, 17);
            this.cbOnlyYPG.TabIndex = 8;
            this.cbOnlyYPG.Text = "Only load YPG";
            this.cbOnlyYPG.UseVisualStyleBackColor = true;
            this.cbOnlyYPG.CheckedChanged += new System.EventHandler(this.checkBox1_CheckedChanged);
            // 
            // bClear
            // 
            this.bClear.Location = new System.Drawing.Point(73, 32);
            this.bClear.Name = "bClear";
            this.bClear.Size = new System.Drawing.Size(55, 23);
            this.bClear.TabIndex = 7;
            this.bClear.Text = "Clear";
            this.bClear.UseVisualStyleBackColor = true;
            this.bClear.Click += new System.EventHandler(this.bClear_Click);
            // 
            // bPrevious
            // 
            this.bPrevious.Location = new System.Drawing.Point(12, 61);
            this.bPrevious.Name = "bPrevious";
            this.bPrevious.Size = new System.Drawing.Size(55, 23);
            this.bPrevious.TabIndex = 6;
            this.bPrevious.Text = "<<";
            this.bPrevious.UseVisualStyleBackColor = true;
            this.bPrevious.Click += new System.EventHandler(this.bPrevious_Click);
            // 
            // bNext
            // 
            this.bNext.Location = new System.Drawing.Point(73, 61);
            this.bNext.Name = "bNext";
            this.bNext.Size = new System.Drawing.Size(55, 23);
            this.bNext.TabIndex = 5;
            this.bNext.Text = ">>";
            this.bNext.UseVisualStyleBackColor = true;
            this.bNext.Click += new System.EventHandler(this.bNext_Click);
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.rbZoom);
            this.groupBox2.Controls.Add(this.rbCenterImage);
            this.groupBox2.Controls.Add(this.rbAutoSize);
            this.groupBox2.Controls.Add(this.rbStretchImage);
            this.groupBox2.Controls.Add(this.rbNormal);
            this.groupBox2.Location = new System.Drawing.Point(153, 6);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(104, 82);
            this.groupBox2.TabIndex = 4;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Show picture";
            // 
            // rbZoom
            // 
            this.rbZoom.AutoSize = true;
            this.rbZoom.Checked = true;
            this.rbZoom.Location = new System.Drawing.Point(12, 20);
            this.rbZoom.Name = "rbZoom";
            this.rbZoom.Size = new System.Drawing.Size(52, 17);
            this.rbZoom.TabIndex = 4;
            this.rbZoom.TabStop = true;
            this.rbZoom.Text = "Zoom";
            this.rbZoom.UseVisualStyleBackColor = true;
            // 
            // rbCenterImage
            // 
            this.rbCenterImage.AutoSize = true;
            this.rbCenterImage.Location = new System.Drawing.Point(107, 20);
            this.rbCenterImage.Name = "rbCenterImage";
            this.rbCenterImage.Size = new System.Drawing.Size(85, 17);
            this.rbCenterImage.TabIndex = 3;
            this.rbCenterImage.Text = "CenterImage";
            this.rbCenterImage.UseVisualStyleBackColor = true;
            this.rbCenterImage.Visible = false;
            // 
            // rbAutoSize
            // 
            this.rbAutoSize.AutoSize = true;
            this.rbAutoSize.Location = new System.Drawing.Point(107, 43);
            this.rbAutoSize.Name = "rbAutoSize";
            this.rbAutoSize.Size = new System.Drawing.Size(67, 17);
            this.rbAutoSize.TabIndex = 2;
            this.rbAutoSize.Text = "AutoSize";
            this.rbAutoSize.UseVisualStyleBackColor = true;
            this.rbAutoSize.Visible = false;
            // 
            // rbStretchImage
            // 
            this.rbStretchImage.AutoSize = true;
            this.rbStretchImage.Location = new System.Drawing.Point(12, 39);
            this.rbStretchImage.Name = "rbStretchImage";
            this.rbStretchImage.Size = new System.Drawing.Size(88, 17);
            this.rbStretchImage.TabIndex = 1;
            this.rbStretchImage.Text = "StretchImage";
            this.rbStretchImage.UseVisualStyleBackColor = true;
            // 
            // rbNormal
            // 
            this.rbNormal.AutoSize = true;
            this.rbNormal.Location = new System.Drawing.Point(12, 59);
            this.rbNormal.Name = "rbNormal";
            this.rbNormal.Size = new System.Drawing.Size(58, 17);
            this.rbNormal.TabIndex = 0;
            this.rbNormal.Text = "Normal";
            this.rbNormal.UseVisualStyleBackColor = true;
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.cbDeleteOriginal);
            this.groupBox1.Controls.Add(this.label1);
            this.groupBox1.Controls.Add(this.cbFileExtentions);
            this.groupBox1.Controls.Add(this.cbIncludeSub);
            this.groupBox1.Controls.Add(this.bEncryptDirectory);
            this.groupBox1.Location = new System.Drawing.Point(583, 6);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(276, 82);
            this.groupBox1.TabIndex = 3;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Encript Files";
            // 
            // cbDeleteOriginal
            // 
            this.cbDeleteOriginal.AutoSize = true;
            this.cbDeleteOriginal.Location = new System.Drawing.Point(15, 63);
            this.cbDeleteOriginal.Name = "cbDeleteOriginal";
            this.cbDeleteOriginal.Size = new System.Drawing.Size(93, 17);
            this.cbDeleteOriginal.TabIndex = 6;
            this.cbDeleteOriginal.Text = "Delete original";
            this.cbDeleteOriginal.UseVisualStyleBackColor = true;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 19);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(72, 13);
            this.label1.TabIndex = 5;
            this.label1.Text = "File extention:";
            // 
            // cbFileExtentions
            // 
            this.cbFileExtentions.FormattingEnabled = true;
            this.cbFileExtentions.Items.AddRange(new object[] {
            "jpg",
            "jpeg",
            "gif",
            "bmp",
            "png",
            "*"});
            this.cbFileExtentions.Location = new System.Drawing.Point(90, 16);
            this.cbFileExtentions.Name = "cbFileExtentions";
            this.cbFileExtentions.Size = new System.Drawing.Size(121, 21);
            this.cbFileExtentions.TabIndex = 4;
            this.cbFileExtentions.Text = "jpg";
            // 
            // cbIncludeSub
            // 
            this.cbIncludeSub.AutoSize = true;
            this.cbIncludeSub.Checked = true;
            this.cbIncludeSub.CheckState = System.Windows.Forms.CheckState.Checked;
            this.cbIncludeSub.Location = new System.Drawing.Point(15, 43);
            this.cbIncludeSub.Name = "cbIncludeSub";
            this.cbIncludeSub.Size = new System.Drawing.Size(115, 17);
            this.cbIncludeSub.TabIndex = 3;
            this.cbIncludeSub.Text = "Include sub-folders";
            this.cbIncludeSub.UseVisualStyleBackColor = true;
            // 
            // panel2
            // 
            this.panel2.Controls.Add(this.panelPic);
            this.panel2.Controls.Add(this.splitter1);
            this.panel2.Controls.Add(this.panel3);
            this.panel2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel2.Location = new System.Drawing.Point(0, 24);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(871, 341);
            this.panel2.TabIndex = 4;
            // 
            // panelPic
            // 
            this.panelPic.AutoScroll = true;
            this.panelPic.Controls.Add(this.pBPic);
            this.panelPic.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelPic.Location = new System.Drawing.Point(197, 0);
            this.panelPic.Name = "panelPic";
            this.panelPic.Size = new System.Drawing.Size(674, 341);
            this.panelPic.TabIndex = 4;
            // 
            // splitter1
            // 
            this.splitter1.Location = new System.Drawing.Point(194, 0);
            this.splitter1.Name = "splitter1";
            this.splitter1.Size = new System.Drawing.Size(3, 341);
            this.splitter1.TabIndex = 3;
            this.splitter1.TabStop = false;
            // 
            // panel3
            // 
            this.panel3.Controls.Add(this.tvFolder);
            this.panel3.Dock = System.Windows.Forms.DockStyle.Left;
            this.panel3.Location = new System.Drawing.Point(0, 0);
            this.panel3.Name = "panel3";
            this.panel3.Size = new System.Drawing.Size(194, 341);
            this.panel3.TabIndex = 2;
            // 
            // tvFolder
            // 
            this.tvFolder.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tvFolder.ImageIndex = 0;
            this.tvFolder.ImageList = this.imageList1;
            this.tvFolder.Location = new System.Drawing.Point(0, 0);
            this.tvFolder.Name = "tvFolder";
            this.tvFolder.SelectedImageKey = "Selected";
            this.tvFolder.Size = new System.Drawing.Size(194, 341);
            this.tvFolder.TabIndex = 0;
            this.tvFolder.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.tvFolder_AfterSelect);
            // 
            // imageList1
            // 
            this.imageList1.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imageList1.ImageStream")));
            this.imageList1.TransparentColor = System.Drawing.Color.Transparent;
            this.imageList1.Images.SetKeyName(0, "Folder");
            this.imageList1.Images.SetKeyName(1, "File");
            this.imageList1.Images.SetKeyName(2, "Selected");
            // 
            // menuStrip1
            // 
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.systemToolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(871, 24);
            this.menuStrip1.TabIndex = 5;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // systemToolStripMenuItem
            // 
            this.systemToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.loginToolStripMenuItem,
            this.logoutToolStripMenuItem,
            this.changePasswordToolStripMenuItem,
            this.exitToolStripMenuItem});
            this.systemToolStripMenuItem.Name = "systemToolStripMenuItem";
            this.systemToolStripMenuItem.Size = new System.Drawing.Size(57, 20);
            this.systemToolStripMenuItem.Text = "System";
            // 
            // loginToolStripMenuItem
            // 
            this.loginToolStripMenuItem.Name = "loginToolStripMenuItem";
            this.loginToolStripMenuItem.Size = new System.Drawing.Size(168, 22);
            this.loginToolStripMenuItem.Text = "Login";
            this.loginToolStripMenuItem.Click += new System.EventHandler(this.loginToolStripMenuItem_Click);
            // 
            // logoutToolStripMenuItem
            // 
            this.logoutToolStripMenuItem.Name = "logoutToolStripMenuItem";
            this.logoutToolStripMenuItem.Size = new System.Drawing.Size(168, 22);
            this.logoutToolStripMenuItem.Text = "Logout";
            this.logoutToolStripMenuItem.Click += new System.EventHandler(this.logoutToolStripMenuItem_Click);
            // 
            // changePasswordToolStripMenuItem
            // 
            this.changePasswordToolStripMenuItem.Name = "changePasswordToolStripMenuItem";
            this.changePasswordToolStripMenuItem.Size = new System.Drawing.Size(168, 22);
            this.changePasswordToolStripMenuItem.Text = "Change password";
            this.changePasswordToolStripMenuItem.Click += new System.EventHandler(this.changePasswordToolStripMenuItem_Click);
            // 
            // exitToolStripMenuItem
            // 
            this.exitToolStripMenuItem.Name = "exitToolStripMenuItem";
            this.exitToolStripMenuItem.Size = new System.Drawing.Size(168, 22);
            this.exitToolStripMenuItem.Text = "Exit";
            this.exitToolStripMenuItem.Click += new System.EventHandler(this.exitToolStripMenuItem_Click);
            // 
            // bDecrypt
            // 
            this.bDecrypt.Location = new System.Drawing.Point(104, 10);
            this.bDecrypt.Name = "bDecrypt";
            this.bDecrypt.Size = new System.Drawing.Size(75, 23);
            this.bDecrypt.TabIndex = 10;
            this.bDecrypt.Text = "Decrypt";
            this.bDecrypt.UseVisualStyleBackColor = true;
            this.bDecrypt.Visible = false;
            this.bDecrypt.Click += new System.EventHandler(this.bDecrypt_Click);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(871, 465);
            this.Controls.Add(this.panel2);
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.menuStrip1);
            this.MainMenuStrip = this.menuStrip1;
            this.Name = "Form1";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Show picture";
            this.WindowState = System.Windows.Forms.FormWindowState.Maximized;
            this.Load += new System.EventHandler(this.Form1_Load);
            ((System.ComponentModel.ISupportInitialize)(this.pBPic)).EndInit();
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.trackScale)).EndInit();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.panel2.ResumeLayout(false);
            this.panelPic.ResumeLayout(false);
            this.panelPic.PerformLayout();
            this.panel3.ResumeLayout(false);
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button bLoad;
        private System.Windows.Forms.PictureBox pBPic;
        private System.Windows.Forms.OpenFileDialog openFileDialog1;
        private System.Windows.Forms.Button bEncryptDirectory;
        private System.Windows.Forms.FolderBrowserDialog folderBrowserDialog1;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.Panel panelPic;
        private System.Windows.Forms.Splitter splitter1;
        private System.Windows.Forms.Panel panel3;
        private System.Windows.Forms.TreeView tvFolder;
        private System.Windows.Forms.ImageList imageList1;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ComboBox cbFileExtentions;
        private System.Windows.Forms.CheckBox cbIncludeSub;
        private System.Windows.Forms.CheckBox cbDeleteOriginal;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.RadioButton rbAutoSize;
        private System.Windows.Forms.RadioButton rbStretchImage;
        private System.Windows.Forms.RadioButton rbNormal;
        private System.Windows.Forms.RadioButton rbCenterImage;
        private System.Windows.Forms.RadioButton rbZoom;
        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem systemToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem loginToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem logoutToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem changePasswordToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem exitToolStripMenuItem;
        private System.Windows.Forms.Button bPrevious;
        private System.Windows.Forms.Button bNext;
        private System.Windows.Forms.Button bClear;
        private System.Windows.Forms.CheckBox cbOnlyYPG;
        private System.Windows.Forms.TrackBar trackScale;
        private System.Windows.Forms.Button bDecrypt;
    }
}

