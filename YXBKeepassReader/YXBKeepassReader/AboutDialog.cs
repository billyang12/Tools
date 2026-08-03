using System;
using System.Reflection;
using System.Windows.Forms;

namespace YXBKeepassReader
{
    public partial class AboutDialog : Form
    {
        public AboutDialog()
        {
            InitializeComponent();
            LoadVersionInfo();
        }

        private void LoadVersionInfo()
        {
            // Get version from assembly
            Assembly assembly = Assembly.GetExecutingAssembly();
            Version version = assembly.GetName().Version;

            lblVersion.Text = $"Version {version.Major}.{version.Minor}.{version.Build}";
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
