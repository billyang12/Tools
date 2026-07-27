namespace YXBPictureViewer
{
    partial class DecryptFilesDialog
    {
        private System.ComponentModel.IContainer components = null;
        private System.Windows.Forms.Label lblFolder;
        private System.Windows.Forms.TextBox txtFolder;
        private System.Windows.Forms.Button btnBrowse;
        private System.Windows.Forms.Label lblFileType;
        private System.Windows.Forms.ComboBox cmbFileType;
        private System.Windows.Forms.Label lblOutputExtension;
        private System.Windows.Forms.TextBox txtOutputExtension;
        private System.Windows.Forms.CheckBox chkIncludeSubfolders;
        private System.Windows.Forms.CheckBox chkDeleteOriginal;
        private System.Windows.Forms.Button btnDecrypt;
        private System.Windows.Forms.Button btnCancel;

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
            this.lblFolder = new System.Windows.Forms.Label();
            this.txtFolder = new System.Windows.Forms.TextBox();
            this.btnBrowse = new System.Windows.Forms.Button();
            this.lblFileType = new System.Windows.Forms.Label();
            this.cmbFileType = new System.Windows.Forms.ComboBox();
            this.lblOutputExtension = new System.Windows.Forms.Label();
            this.txtOutputExtension = new System.Windows.Forms.TextBox();
            this.chkIncludeSubfolders = new System.Windows.Forms.CheckBox();
            this.chkDeleteOriginal = new System.Windows.Forms.CheckBox();
            this.btnDecrypt = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            this.SuspendLayout();

            //
            // lblFolder
            //
            this.lblFolder.AutoSize = true;
            this.lblFolder.Location = new System.Drawing.Point(20, 20);
            this.lblFolder.Name = "lblFolder";
            this.lblFolder.Size = new System.Drawing.Size(45, 15);
            this.lblFolder.TabIndex = 0;
            this.lblFolder.Text = "Folder:";

            //
            // txtFolder
            //
            this.txtFolder.Location = new System.Drawing.Point(20, 40);
            this.txtFolder.Name = "txtFolder";
            this.txtFolder.Size = new System.Drawing.Size(400, 23);
            this.txtFolder.TabIndex = 1;

            //
            // btnBrowse
            //
            this.btnBrowse.Location = new System.Drawing.Point(430, 38);
            this.btnBrowse.Name = "btnBrowse";
            this.btnBrowse.Size = new System.Drawing.Size(90, 27);
            this.btnBrowse.TabIndex = 2;
            this.btnBrowse.Text = "Browse...";
            this.btnBrowse.UseVisualStyleBackColor = true;
            this.btnBrowse.Click += new System.EventHandler(this.btnBrowse_Click);

            //
            // lblFileType
            //
            this.lblFileType.AutoSize = true;
            this.lblFileType.Location = new System.Drawing.Point(20, 80);
            this.lblFileType.Name = "lblFileType";
            this.lblFileType.Size = new System.Drawing.Size(120, 15);
            this.lblFileType.TabIndex = 3;
            this.lblFileType.Text = "Files to Decrypt:";

            //
            // cmbFileType
            //
            this.cmbFileType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbFileType.FormattingEnabled = true;
            this.cmbFileType.Items.AddRange(new object[] {
            "Both YPG and XPG",
            "YPG only (DES)",
            "XPG only (AES-256)"});
            this.cmbFileType.Location = new System.Drawing.Point(20, 100);
            this.cmbFileType.Name = "cmbFileType";
            this.cmbFileType.Size = new System.Drawing.Size(240, 23);
            this.cmbFileType.TabIndex = 4;

            //
            // lblOutputExtension
            //
            this.lblOutputExtension.AutoSize = true;
            this.lblOutputExtension.Location = new System.Drawing.Point(280, 80);
            this.lblOutputExtension.Name = "lblOutputExtension";
            this.lblOutputExtension.Size = new System.Drawing.Size(110, 15);
            this.lblOutputExtension.TabIndex = 5;
            this.lblOutputExtension.Text = "Output Extension:";

            //
            // txtOutputExtension
            //
            this.txtOutputExtension.Location = new System.Drawing.Point(280, 100);
            this.txtOutputExtension.Name = "txtOutputExtension";
            this.txtOutputExtension.Size = new System.Drawing.Size(240, 23);
            this.txtOutputExtension.TabIndex = 6;
            this.txtOutputExtension.Text = "jpg";

            //
            // chkIncludeSubfolders
            //
            this.chkIncludeSubfolders.AutoSize = true;
            this.chkIncludeSubfolders.Checked = true;
            this.chkIncludeSubfolders.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkIncludeSubfolders.Location = new System.Drawing.Point(20, 145);
            this.chkIncludeSubfolders.Name = "chkIncludeSubfolders";
            this.chkIncludeSubfolders.Size = new System.Drawing.Size(200, 19);
            this.chkIncludeSubfolders.TabIndex = 7;
            this.chkIncludeSubfolders.Text = "Include subfolders";
            this.chkIncludeSubfolders.UseVisualStyleBackColor = true;

            //
            // chkDeleteOriginal
            //
            this.chkDeleteOriginal.AutoSize = true;
            this.chkDeleteOriginal.Location = new System.Drawing.Point(20, 175);
            this.chkDeleteOriginal.Name = "chkDeleteOriginal";
            this.chkDeleteOriginal.Size = new System.Drawing.Size(350, 19);
            this.chkDeleteOriginal.TabIndex = 8;
            this.chkDeleteOriginal.Text = "Delete encrypted files after decryption (USE WITH CAUTION!)";
            this.chkDeleteOriginal.UseVisualStyleBackColor = true;

            //
            // btnDecrypt
            //
            this.btnDecrypt.Location = new System.Drawing.Point(300, 220);
            this.btnDecrypt.Name = "btnDecrypt";
            this.btnDecrypt.Size = new System.Drawing.Size(110, 35);
            this.btnDecrypt.TabIndex = 9;
            this.btnDecrypt.Text = "Decrypt";
            this.btnDecrypt.UseVisualStyleBackColor = true;
            this.btnDecrypt.Click += new System.EventHandler(this.btnDecrypt_Click);

            //
            // btnCancel
            //
            this.btnCancel.Location = new System.Drawing.Point(420, 220);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(100, 35);
            this.btnCancel.TabIndex = 10;
            this.btnCancel.Text = "Cancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);

            //
            // DecryptFilesDialog
            //
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(540, 275);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnDecrypt);
            this.Controls.Add(this.chkDeleteOriginal);
            this.Controls.Add(this.chkIncludeSubfolders);
            this.Controls.Add(this.txtOutputExtension);
            this.Controls.Add(this.lblOutputExtension);
            this.Controls.Add(this.cmbFileType);
            this.Controls.Add(this.lblFileType);
            this.Controls.Add(this.btnBrowse);
            this.Controls.Add(this.txtFolder);
            this.Controls.Add(this.lblFolder);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "DecryptFilesDialog";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Decrypt Files to JPG";
            this.ResumeLayout(false);
            this.PerformLayout();
        }
    }
}
