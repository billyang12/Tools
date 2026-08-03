using System;
using System.Drawing;
using System.Windows.Forms;

namespace YXBKeepassReader
{
    /// <summary>
    /// Help dialog showing how to export KeePass files as XML
    /// </summary>
    public class HelpDialog : Form
    {
        public HelpDialog()
        {
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.StartPosition = FormStartPosition.CenterParent;
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            this.Text = "How to Export KeePass as XML";
            this.Width = 500;
            this.Height = 450;

            SetupControls();
        }

        private void SetupControls()
        {
            this.Controls.Clear();

            // Title
            Label titleLabel = new Label();
            titleLabel.Text = "How to Export Your KeePass Database as XML";
            titleLabel.Font = new System.Drawing.Font("Segoe UI", 12, System.Drawing.FontStyle.Bold);
            titleLabel.Location = new System.Drawing.Point(15, 15);
            titleLabel.Size = new System.Drawing.Size(470, 35);
            titleLabel.AutoSize = false;
            this.Controls.Add(titleLabel);

            // Steps
            Label stepsLabel = new Label();
            stepsLabel.Text = 
                "Follow these steps to export your KeePass database:\n\n" +
                "1. Open the KeePass application\n\n" +
                "2. Open your database file (if not already open)\n\n" +
                "3. Click on the File menu\n\n" +
                "4. Select Export\n\n" +
                "5. Choose XML (*.xml) format\n\n" +
                "6. Click Save and choose a location\n\n" +
                "7. Come back to KeePass Reader and select\n" +
                "   the exported XML file\n\n" +
                "8. You don't need to enter a password for XML files\n\n" +
                "Your passwords will load successfully!";
            stepsLabel.Location = new System.Drawing.Point(15, 60);
            stepsLabel.Size = new System.Drawing.Size(470, 310);
            stepsLabel.AutoSize = false;
            stepsLabel.Font = new System.Drawing.Font("Segoe UI", 10);
            this.Controls.Add(stepsLabel);

            // OK Button
            Button okButton = new Button();
            okButton.Text = "Got It!";
            okButton.Location = new System.Drawing.Point(210, 380);
            okButton.Size = new System.Drawing.Size(80, 30);
            okButton.DialogResult = DialogResult.OK;
            this.Controls.Add(okButton);
        }
    }
}
