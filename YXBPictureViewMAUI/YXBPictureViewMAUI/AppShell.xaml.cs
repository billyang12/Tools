namespace YXBPictureViewMAUI
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();
        }

        private void OnExitClicked(object? sender, EventArgs e)
        {
            // Close the application
            Application.Current?.Quit();
        }
    }
}
