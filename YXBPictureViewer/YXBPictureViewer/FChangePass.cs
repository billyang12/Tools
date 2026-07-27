using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace YXBPictureViewer
{
    public partial class FChangePass : Form
    {
        public FChangePass()
        {
            InitializeComponent();
        }

        private void bOk_Click(object sender, EventArgs e)
        {
            if (Form1.m_password != tbPassword.Text)
            {
                MessageBox.Show("Old password is not right!");
                return;
            }
            if (tbnewpass.Text != tbretypepass.Text)
            {
                MessageBox.Show("New password in two boxes don't mach!");
                return;
            }
            Form1.m_password = tbnewpass.Text;
            Form1.SaveAllSettingsInCurrentUser();
            MessageBox.Show("Password changed successfully!");
            DialogResult = DialogResult.OK;
        }

        private void bCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
        }
    }
}
