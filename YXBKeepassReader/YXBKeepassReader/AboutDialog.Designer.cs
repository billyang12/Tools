namespace YXBKeepassReader
{
    partial class AboutDialog
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
            lblTitle = new System.Windows.Forms.Label();
            lblVersion = new System.Windows.Forms.Label();
            lblDeveloper = new System.Windows.Forms.Label();
            lblDescription = new System.Windows.Forms.Label();
            btnOK = new System.Windows.Forms.Button();
            SuspendLayout();
            //
            // lblTitle
            //
            lblTitle.AutoSize = true;
            lblTitle.Font = new System.Drawing.Font("Segoe UI", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            lblTitle.Location = new System.Drawing.Point(20, 20);
            lblTitle.Name = "lblTitle";
            lblTitle.Size = new System.Drawing.Size(191, 25);
            lblTitle.TabIndex = 0;
            lblTitle.Text = "KeePass DB Reader";
            //
            // lblVersion
            //
            lblVersion.AutoSize = true;
            lblVersion.Location = new System.Drawing.Point(20, 55);
            lblVersion.Name = "lblVersion";
            lblVersion.Size = new System.Drawing.Size(69, 15);
            lblVersion.TabIndex = 1;
            lblVersion.Text = "Version 1.0.0";
            //
            // lblDeveloper
            //
            lblDeveloper.AutoSize = true;
            lblDeveloper.Location = new System.Drawing.Point(20, 80);
            lblDeveloper.Name = "lblDeveloper";
            lblDeveloper.Size = new System.Drawing.Size(125, 15);
            lblDeveloper.TabIndex = 2;
            lblDeveloper.Text = "Developed by Bill Yang";
            //
            // lblDescription
            //
            lblDescription.Location = new System.Drawing.Point(20, 115);
            lblDescription.Name = "lblDescription";
            lblDescription.Size = new System.Drawing.Size(400, 80);
            lblDescription.TabIndex = 3;
            lblDescription.Text = "A lightweight KeePass database reader that supports both KDBX (binary) and XML formats. Easily view your passwords and credentials from KeePass databases without requiring the full KeePass application.";
            //
            // btnOK
            //
            btnOK.Location = new System.Drawing.Point(345, 210);
            btnOK.Name = "btnOK";
            btnOK.Size = new System.Drawing.Size(75, 30);
            btnOK.TabIndex = 4;
            btnOK.Text = "OK";
            btnOK.UseVisualStyleBackColor = true;
            btnOK.Click += btnOK_Click;
            //
            // AboutDialog
            //
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            ClientSize = new System.Drawing.Size(440, 260);
            Controls.Add(btnOK);
            Controls.Add(lblDescription);
            Controls.Add(lblDeveloper);
            Controls.Add(lblVersion);
            Controls.Add(lblTitle);
            FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "AboutDialog";
            StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            Text = "About KeePass DB Reader";
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private System.Windows.Forms.Label lblTitle;
        private System.Windows.Forms.Label lblVersion;
        private System.Windows.Forms.Label lblDeveloper;
        private System.Windows.Forms.Label lblDescription;
        private System.Windows.Forms.Button btnOK;
    }
}
