using System;
using System.Drawing;
using System.Reflection;
using System.Windows.Forms;

namespace YXBPictureViewer
{
    /// <summary>
    /// About dialog showing application information
    /// </summary>
    public class AboutDialog : Form
    {
        private Label lblTitle;
        private Label lblVersion;
        private Label lblDeveloper;
        private TextBox txtDescription;
        private Button btnOK;
        private PictureBox pictureIcon;

        public AboutDialog()
        {
            InitializeComponent();
            LoadVersionInfo();
        }

        private void InitializeComponent()
        {
            this.pictureIcon = new PictureBox();
            this.lblTitle = new Label();
            this.lblVersion = new Label();
            this.lblDeveloper = new Label();
            this.txtDescription = new TextBox();
            this.btnOK = new Button();

            ((System.ComponentModel.ISupportInitialize)(this.pictureIcon)).BeginInit();
            this.SuspendLayout();

            //
            // pictureIcon
            //
            this.pictureIcon.Location = new Point(20, 20);
            this.pictureIcon.Name = "pictureIcon";
            this.pictureIcon.Size = new Size(64, 64);
            this.pictureIcon.SizeMode = PictureBoxSizeMode.Zoom;
            this.pictureIcon.TabIndex = 0;
            this.pictureIcon.TabStop = false;
            try
            {
                this.pictureIcon.Image = Icon.ExtractAssociatedIcon(Application.ExecutablePath)?.ToBitmap();
            }
            catch
            {
                // If icon load fails, leave it empty
            }

            //
            // lblTitle
            //
            this.lblTitle.AutoSize = false;
            this.lblTitle.Font = new Font("Segoe UI", 16F, FontStyle.Bold);
            this.lblTitle.Location = new Point(100, 20);
            this.lblTitle.Name = "lblTitle";
            this.lblTitle.Size = new Size(350, 30);
            this.lblTitle.TabIndex = 1;
            this.lblTitle.Text = "YXB Picture Viewer";

            //
            // lblVersion
            //
            this.lblVersion.AutoSize = false;
            this.lblVersion.Font = new Font("Segoe UI", 10F);
            this.lblVersion.Location = new Point(100, 55);
            this.lblVersion.Name = "lblVersion";
            this.lblVersion.Size = new Size(350, 25);
            this.lblVersion.TabIndex = 2;
            this.lblVersion.Text = "Version: 1.0";

            //
            // lblDeveloper
            //
            this.lblDeveloper.AutoSize = false;
            this.lblDeveloper.Font = new Font("Segoe UI", 10F);
            this.lblDeveloper.Location = new Point(20, 100);
            this.lblDeveloper.Name = "lblDeveloper";
            this.lblDeveloper.Size = new Size(430, 25);
            this.lblDeveloper.TabIndex = 3;
            this.lblDeveloper.Text = "Developed by: Bill Yang";

            //
            // txtDescription
            //
            this.txtDescription.BackColor = SystemColors.Control;
            this.txtDescription.BorderStyle = BorderStyle.FixedSingle;
            this.txtDescription.Font = new Font("Segoe UI", 9F);
            this.txtDescription.Location = new Point(20, 135);
            this.txtDescription.Multiline = true;
            this.txtDescription.Name = "txtDescription";
            this.txtDescription.ReadOnly = true;
            this.txtDescription.ScrollBars = ScrollBars.Vertical;
            this.txtDescription.Size = new Size(430, 120);
            this.txtDescription.TabIndex = 4;
            this.txtDescription.TabStop = false;
            this.txtDescription.Text = "A secure image and video viewer with AES-256-GCM encryption.\r\n\r\n" +
                                       "Features:\r\n" +
                                       "• View encrypted images (.xpg) and videos (.xpv)\r\n" +
                                       "• Encrypt/decrypt files with strong AES-256 encryption\r\n" +
                                       "• Convert legacy DES (.ypg) to AES format\r\n" +
                                       "• Cross-platform compatible with mobile MAUI app\r\n" +
                                       "• Full video playback with controls (play/pause, seek, volume)\r\n" +
                                       "• Password derivation from memorable phrases\r\n" +
                                       "• Batch encryption/decryption operations";

            //
            // btnOK
            //
            this.btnOK.Location = new Point(190, 270);
            this.btnOK.Name = "btnOK";
            this.btnOK.Size = new Size(90, 30);
            this.btnOK.TabIndex = 5;
            this.btnOK.Text = "OK";
            this.btnOK.UseVisualStyleBackColor = true;
            this.btnOK.Click += BtnOK_Click;

            //
            // AboutDialog
            //
            this.AcceptButton = this.btnOK;
            this.AutoScaleDimensions = new SizeF(7F, 15F);
            this.AutoScaleMode = AutoScaleMode.Font;
            this.ClientSize = new Size(470, 320);
            this.Controls.Add(this.btnOK);
            this.Controls.Add(this.txtDescription);
            this.Controls.Add(this.lblDeveloper);
            this.Controls.Add(this.lblVersion);
            this.Controls.Add(this.lblTitle);
            this.Controls.Add(this.pictureIcon);
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "AboutDialog";
            this.StartPosition = FormStartPosition.CenterParent;
            this.Text = "About YXB Picture Viewer";
            try
            {
                this.Icon = new Icon("appicon.ico");
            }
            catch
            {
                // If icon load fails, use default
            }

            ((System.ComponentModel.ISupportInitialize)(this.pictureIcon)).EndInit();
            this.ResumeLayout(false);
        }

        private void LoadVersionInfo()
        {
            try
            {
                // Get version from assembly
                var assembly = Assembly.GetExecutingAssembly();
                var version = assembly.GetName().Version;
                lblVersion.Text = $"Version: {version.Major}.{version.Minor}";
            }
            catch
            {
                lblVersion.Text = "Version: 1.0";
            }
        }

        private void BtnOK_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.OK;
            this.Close();
        }
    }
}
