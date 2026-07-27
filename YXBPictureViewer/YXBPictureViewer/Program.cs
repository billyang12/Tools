using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace YXBPictureViewer
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            // Launch new MainForm instead of old Form1
            // Form1 is kept intact for reference/backup
            Application.Run(new MainForm());
        }
    }
}
