using System;
using System.Windows.Forms;

namespace YXBPictureViewer
{
    /// <summary>
    /// Progress dialog for long-running operations
    /// Shows progress bar and current status
    /// </summary>
    public partial class ProgressDialog : Form
    {
        public bool CancelRequested { get; private set; }

        public ProgressDialog(string title)
        {
            InitializeComponent();
            this.Text = title;
            CancelRequested = false;

            // Wire up cancel button
            btnCancel.Click += (s, e) =>
            {
                CancelRequested = true;
                btnCancel.Enabled = false;
                btnCancel.Text = "Cancelling...";
            };
        }

        /// <summary>
        /// Update progress bar and status text
        /// </summary>
        public void UpdateProgress(int current, int total, string statusText)
        {
            if (InvokeRequired)
            {
                Invoke(new Action(() => UpdateProgress(current, total, statusText)));
                return;
            }

            if (total > 0)
            {
                progressBar.Maximum = total;
                progressBar.Value = Math.Min(current, total);
                lblPercentage.Text = $"{current} / {total} ({(current * 100 / total)}%)";
            }

            lblStatus.Text = statusText;
            Application.DoEvents(); // Force UI update
        }

        /// <summary>
        /// Set status text only
        /// </summary>
        public void SetStatus(string statusText)
        {
            if (InvokeRequired)
            {
                Invoke(new Action(() => SetStatus(statusText)));
                return;
            }

            lblStatus.Text = statusText;
            Application.DoEvents();
        }
    }
}
