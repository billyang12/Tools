namespace YXBKeepassReader
{
    partial class MainForm
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
            menuStrip1 = new System.Windows.Forms.MenuStrip();
            fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            openToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            closeFileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            exportToHNFToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            recentFilesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            exitToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            helpToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            aboutToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            tbContent = new System.Windows.Forms.TextBox();
            panel1 = new System.Windows.Forms.Panel();
            panelRight = new System.Windows.Forms.Panel();
            splitter1 = new System.Windows.Forms.Splitter();
            panelLeft = new System.Windows.Forms.Panel();
            treeView1 = new System.Windows.Forms.TreeView();
            menuStrip1.SuspendLayout();
            panel1.SuspendLayout();
            panelRight.SuspendLayout();
            panelLeft.SuspendLayout();
            SuspendLayout();
            //
            // menuStrip1
            //
            menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            fileToolStripMenuItem,
            helpToolStripMenuItem});
            menuStrip1.Location = new System.Drawing.Point(0, 0);
            menuStrip1.Name = "menuStrip1";
            menuStrip1.Size = new System.Drawing.Size(1433, 24);
            menuStrip1.TabIndex = 0;
            menuStrip1.Text = "menuStrip1";
            //
            // fileToolStripMenuItem
            //
            fileToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            openToolStripMenuItem,
            closeFileToolStripMenuItem,
            exportToHNFToolStripMenuItem,
            recentFilesToolStripMenuItem,
            toolStripSeparator1,
            exitToolStripMenuItem});
            fileToolStripMenuItem.Name = "fileToolStripMenuItem";
            fileToolStripMenuItem.Size = new System.Drawing.Size(37, 20);
            fileToolStripMenuItem.Text = "&File";
            //
            // openToolStripMenuItem
            //
            openToolStripMenuItem.Name = "openToolStripMenuItem";
            openToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.O)));
            openToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
            openToolStripMenuItem.Text = "&Open...";
            openToolStripMenuItem.Click += openToolStripMenuItem_Click;
            //
            // closeFileToolStripMenuItem
            //
            closeFileToolStripMenuItem.Enabled = false;
            closeFileToolStripMenuItem.Name = "closeFileToolStripMenuItem";
            closeFileToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
            closeFileToolStripMenuItem.Text = "&Close File";
            closeFileToolStripMenuItem.Click += closeFileToolStripMenuItem_Click;
            //
            // exportToHNFToolStripMenuItem
            //
            exportToHNFToolStripMenuItem.Enabled = false;
            exportToHNFToolStripMenuItem.Name = "exportToHNFToolStripMenuItem";
            exportToHNFToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
            exportToHNFToolStripMenuItem.Text = "&Export to HNF...";
            exportToHNFToolStripMenuItem.Click += exportToHNFToolStripMenuItem_Click;
            //
            // recentFilesToolStripMenuItem
            //
            recentFilesToolStripMenuItem.Name = "recentFilesToolStripMenuItem";
            recentFilesToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
            recentFilesToolStripMenuItem.Text = "&Recent Files";
            recentFilesToolStripMenuItem.DropDownOpening += recentFilesToolStripMenuItem_DropDownOpening;
            //
            // toolStripSeparator1
            //
            toolStripSeparator1.Name = "toolStripSeparator1";
            toolStripSeparator1.Size = new System.Drawing.Size(152, 6);
            //
            // exitToolStripMenuItem
            //
            exitToolStripMenuItem.Name = "exitToolStripMenuItem";
            exitToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
            exitToolStripMenuItem.Text = "E&xit";
            exitToolStripMenuItem.Click += exitToolStripMenuItem_Click;
            //
            // helpToolStripMenuItem
            //
            helpToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            aboutToolStripMenuItem});
            helpToolStripMenuItem.Name = "helpToolStripMenuItem";
            helpToolStripMenuItem.Size = new System.Drawing.Size(44, 20);
            helpToolStripMenuItem.Text = "&Help";
            //
            // aboutToolStripMenuItem
            //
            aboutToolStripMenuItem.Name = "aboutToolStripMenuItem";
            aboutToolStripMenuItem.Size = new System.Drawing.Size(116, 22);
            aboutToolStripMenuItem.Text = "&About...";
            aboutToolStripMenuItem.Click += aboutToolStripMenuItem_Click;
            //
            // tbContent
            //
            tbContent.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            tbContent.Dock = System.Windows.Forms.DockStyle.Fill;
            tbContent.Location = new System.Drawing.Point(8, 8);
            tbContent.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            tbContent.MaxLength = 0;
            tbContent.Multiline = true;
            tbContent.Name = "tbContent";
            tbContent.ReadOnly = true;
            tbContent.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            tbContent.Size = new System.Drawing.Size(958, 720);
            tbContent.TabIndex = 1;
            //
            // panel1
            //
            panel1.Controls.Add(panelRight);
            panel1.Controls.Add(splitter1);
            panel1.Controls.Add(panelLeft);
            panel1.Dock = System.Windows.Forms.DockStyle.Fill;
            panel1.Location = new System.Drawing.Point(0, 24);
            panel1.Margin = new System.Windows.Forms.Padding(0);
            panel1.Name = "panel1";
            panel1.Padding = new System.Windows.Forms.Padding(8);
            panel1.Size = new System.Drawing.Size(1433, 736);
            panel1.TabIndex = 2;
            //
            // panelRight
            //
            panelRight.Controls.Add(tbContent);
            panelRight.Dock = System.Windows.Forms.DockStyle.Fill;
            panelRight.Location = new System.Drawing.Point(467, 8);
            panelRight.Margin = new System.Windows.Forms.Padding(0);
            panelRight.Name = "panelRight";
            panelRight.Padding = new System.Windows.Forms.Padding(8);
            panelRight.Size = new System.Drawing.Size(958, 720);
            panelRight.TabIndex = 2;
            //
            // splitter1
            //
            splitter1.Location = new System.Drawing.Point(459, 8);
            splitter1.Margin = new System.Windows.Forms.Padding(0);
            splitter1.Name = "splitter1";
            splitter1.Size = new System.Drawing.Size(8, 720);
            splitter1.TabIndex = 1;
            splitter1.TabStop = false;
            //
            // panelLeft
            //
            panelLeft.Controls.Add(treeView1);
            panelLeft.Dock = System.Windows.Forms.DockStyle.Left;
            panelLeft.Location = new System.Drawing.Point(8, 8);
            panelLeft.Margin = new System.Windows.Forms.Padding(0);
            panelLeft.Name = "panelLeft";
            panelLeft.Padding = new System.Windows.Forms.Padding(8);
            panelLeft.Size = new System.Drawing.Size(451, 720);
            panelLeft.TabIndex = 0;
            //
            // treeView1
            //
            treeView1.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            treeView1.Dock = System.Windows.Forms.DockStyle.Fill;
            treeView1.Location = new System.Drawing.Point(8, 8);
            treeView1.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            treeView1.Name = "treeView1";
            treeView1.Size = new System.Drawing.Size(435, 704);
            treeView1.TabIndex = 0;
            treeView1.AfterSelect += treeView1_AfterSelect;
            //
            // MainForm
            //
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            ClientSize = new System.Drawing.Size(1433, 760);
            Controls.Add(panel1);
            Controls.Add(menuStrip1);
            MainMenuStrip = menuStrip1;
            Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            Name = "MainForm";
            Text = "Keepass DB Reader";
            menuStrip1.ResumeLayout(false);
            menuStrip1.PerformLayout();
            panel1.ResumeLayout(false);
            panelRight.ResumeLayout(false);
            panelRight.PerformLayout();
            panelLeft.ResumeLayout(false);
            ResumeLayout(false);
            PerformLayout();

        }

        #endregion

        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem fileToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem openToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem closeFileToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem exportToHNFToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem recentFilesToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripMenuItem exitToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem helpToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem aboutToolStripMenuItem;
        private System.Windows.Forms.TextBox tbContent;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Panel panelRight;
        private System.Windows.Forms.Splitter splitter1;
        private System.Windows.Forms.Panel panelLeft;
        private System.Windows.Forms.TreeView treeView1;
    }
}
