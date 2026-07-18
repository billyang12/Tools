using System.Reflection;
using System.Windows;

namespace HierachicalNotes
{
    public partial class AboutDialog : Window
    {
        public AboutDialog()
        {
            InitializeComponent();
            LoadVersion();
        }

        private void LoadVersion()
        {
            var version = Assembly.GetExecutingAssembly().GetName().Version;
            if (version != null)
            {
                txtVersion.Text = $"Version {version.Major}.{version.Minor}";
            }
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
