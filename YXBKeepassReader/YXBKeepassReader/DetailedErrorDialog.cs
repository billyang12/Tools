using System;
using System.Drawing;
using System.Windows.Forms;

namespace YXBKeepassReader
{
    /// <summary>
    /// Enhanced error dialog with Help button and formatted error details
    /// </summary>
    public class DetailedErrorDialog : Form
    {
        private string _errorTitle;
        private string _errorMessage;
        private bool _showHelpButton;

        public DetailedErrorDialog(string title, string message, bool showHelpButton = true)
        {
            _errorTitle = title;
            _errorMessage = message;
            _showHelpButton = showHelpButton;

            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.StartPosition = FormStartPosition.CenterParent;
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            this.Text = _errorTitle;
            this.Width = 550;
            this.Height = 300;

            SetupControls();
        }

        private void SetupControls()
        {
            this.Controls.Clear();

            // Error icon
            PictureBox iconBox = new PictureBox();
            iconBox.Image = SystemIcons.Error.ToBitmap();
            iconBox.Location = new Point(15, 15);
            iconBox.Size = new Size(32, 32);
            iconBox.SizeMode = PictureBoxSizeMode.CenterImage;
            this.Controls.Add(iconBox);

            // Title
            Label titleLabel = new Label();
            titleLabel.Text = _errorTitle;
            titleLabel.Font = new Font("Segoe UI", 11, FontStyle.Bold);
            titleLabel.Location = new Point(55, 15);
            titleLabel.Size = new Size(480, 30);
            this.Controls.Add(titleLabel);

            // Error message (scrollable text)
            TextBox messageBox = new TextBox();
            messageBox.Text = _errorMessage;
            messageBox.Multiline = true;
            messageBox.ReadOnly = true;
            messageBox.ScrollBars = ScrollBars.Vertical;
            messageBox.Location = new Point(15, 55);
            messageBox.Size = new Size(520, 160);
            messageBox.Font = new Font("Segoe UI", 9);
            messageBox.BackColor = SystemColors.Window;
            messageBox.BorderStyle = BorderStyle.Fixed3D;
            this.Controls.Add(messageBox);

            // Button panel
            int buttonY = 225;

            // Help button
            if (_showHelpButton)
            {
                Button helpButton = new Button();
                helpButton.Text = "Help";
                helpButton.Location = new Point(15, buttonY);
                helpButton.Size = new Size(80, 30);
                helpButton.Click += (s, e) => ShowHelp();
                this.Controls.Add(helpButton);
            }

            // OK button (right-aligned)
            Button okButton = new Button();
            okButton.Text = "OK";
            okButton.Location = new Point(450, buttonY);
            okButton.Size = new Size(80, 30);
            okButton.DialogResult = DialogResult.OK;
            this.Controls.Add(okButton);
        }

        private void ShowHelp()
        {
            using (HelpDialog helpForm = new HelpDialog())
            {
                helpForm.ShowDialog(this);
            }
        }
    }
}
