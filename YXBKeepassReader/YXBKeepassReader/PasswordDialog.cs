using System;
using System.Windows.Forms;

namespace YXBKeepassReader
{
    public partial class PasswordDialog : Form
    {
        public string Password { get; private set; }

        public PasswordDialog(string fileName)
        {
            InitializeComponent();
            lblFileName.Text = $"Enter password for: {System.IO.Path.GetFileName(fileName)}";
        }

        public PasswordDialog(string customPrompt, bool useCustomPrompt)
        {
            InitializeComponent();
            if (useCustomPrompt)
            {
                lblFileName.Text = customPrompt;
            }
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            Password = txtPassword.Text;
            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }

        private void PasswordDialog_Load(object sender, EventArgs e)
        {
            txtPassword.Focus();
        }
    }
}
