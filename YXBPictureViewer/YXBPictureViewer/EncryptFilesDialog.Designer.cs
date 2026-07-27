namespace YXBPictureViewer
{
    partial class EncryptFilesDialog
    {
        private System.ComponentModel.IContainer components = null;
        private System.Windows.Forms.Label lblFolder;
        private System.Windows.Forms.TextBox txtFolder;
        private System.Windows.Forms.Button btnBrowse;
        private System.Windows.Forms.Label lblSourceExtension;
        private System.Windows.Forms.ComboBox cmbSourceExtension;
        private System.Windows.Forms.Label lblEncryptionType;
        private System.Windows.Forms.ComboBox cmbEncryptionType;
        private System.Windows.Forms.CheckBox chkIncludeSubfolders;
        private System.Windows.Forms.CheckBox chkDeleteOriginal;
        private System.Windows.Forms.Button btnEncrypt;
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
            this.lblSourceExtension = new System.Windows.Forms.Label();
            this.cmbSourceExtension = new System.Windows.Forms.ComboBox();
            this.lblEncryptionType = new System.Windows.Forms.Label();
            this.cmbEncryptionType = new System.Windows.Forms.ComboBox();
            this.chkIncludeSubfolders = new System.Windows.Forms.CheckBox();
            this.chkDeleteOriginal = new System.Windows.Forms.CheckBox();
            this.btnEncrypt = new System.Windows.Forms.Button();
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
            // lblSourceExtension
            //
            this.lblSourceExtension.AutoSize = true;
            this.lblSourceExtension.Location = new System.Drawing.Point(20, 80);
            this.lblSourceExtension.Name = "lblSourceExtension";
            this.lblSourceExtension.Size = new System.Drawing.Size(150, 15);
            this.lblSourceExtension.TabIndex = 3;
            this.lblSourceExtension.Text = "Source File Extension:";

            //
            // cmbSourceExtension
            //
            this.cmbSourceExtension.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbSourceExtension.FormattingEnabled = true;
            this.cmbSourceExtension.Items.AddRange(new object[] {
            "jpg",
            "jpeg",
            "png",
            "bmp",
            "gif",
            "tif",
            "tiff"});
            this.cmbSourceExtension.Location = new System.Drawing.Point(20, 100);
            this.cmbSourceExtension.Name = "cmbSourceExtension";
            this.cmbSourceExtension.Size = new System.Drawing.Size(240, 23);
            this.cmbSourceExtension.TabIndex = 4;

            //
            // lblEncryptionType
            //
            this.lblEncryptionType.AutoSize = true;
            this.lblEncryptionType.Location = new System.Drawing.Point(280, 80);
            this.lblEncryptionType.Name = "lblEncryptionType";
            this.lblEncryptionType.Size = new System.Drawing.Size(95, 15);
            this.lblEncryptionType.TabIndex = 5;
            this.lblEncryptionType.Text = "Encryption Type:";

            //
            // cmbEncryptionType
            //
            this.cmbEncryptionType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbEncryptionType.FormattingEnabled = true;
            this.cmbEncryptionType.Items.AddRange(new object[] {
            "YPG (DES - Old Method)",
            "XPG (AES-256 - New Method)"});
            this.cmbEncryptionType.Location = new System.Drawing.Point(280, 100);
            this.cmbEncryptionType.Name = "cmbEncryptionType";
            this.cmbEncryptionType.Size = new System.Drawing.Size(240, 23);
            this.cmbEncryptionType.TabIndex = 6;

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
            this.chkDeleteOriginal.Size = new System.Drawing.Size(300, 19);
            this.chkDeleteOriginal.TabIndex = 8;
            this.chkDeleteOriginal.Text = "Delete original files after encryption (USE WITH CAUTION!)";
            this.chkDeleteOriginal.UseVisualStyleBackColor = true;

            //
            // btnEncrypt
            //
            this.btnEncrypt.Location = new System.Drawing.Point(300, 220);
            this.btnEncrypt.Name = "btnEncrypt";
            this.btnEncrypt.Size = new System.Drawing.Size(110, 35);
            this.btnEncrypt.TabIndex = 9;
            this.btnEncrypt.Text = "Encrypt";
            this.btnEncrypt.UseVisualStyleBackColor = true;
            this.btnEncrypt.Click += new System.EventHandler(this.btnEncrypt_Click);

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
            // EncryptFilesDialog
            //
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(540, 275);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnEncrypt);
            this.Controls.Add(this.chkDeleteOriginal);
            this.Controls.Add(this.chkIncludeSubfolders);
            this.Controls.Add(this.cmbEncryptionType);
            this.Controls.Add(this.lblEncryptionType);
            this.Controls.Add(this.cmbSourceExtension);
            this.Controls.Add(this.lblSourceExtension);
            this.Controls.Add(this.btnBrowse);
            this.Controls.Add(this.txtFolder);
            this.Controls.Add(this.lblFolder);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "EncryptFilesDialog";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Encrypt Files";
            this.ResumeLayout(false);
            this.PerformLayout();
        }
    }
}
