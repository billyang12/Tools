using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace YXBPictureViewer
{
    public partial class FPassword : Form
    {
        public FPassword()
        {
            InitializeComponent();
        }

        private void bOk_Click(object sender, EventArgs e)
        {
            if (tbPassword.Text == Form1.m_password)
            {
                Form1.m_Logined = true;
                DialogResult = DialogResult.OK;
            }
            else
            {
                MessageBox.Show("Sorry, your password is not correct.");
                return;
            }
        }

        private void bCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
        }
    }
}
