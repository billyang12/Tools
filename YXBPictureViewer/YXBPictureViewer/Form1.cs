using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.IO;
namespace YXBPictureViewer
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }
        public static string key = "Man@QueY";
        public static string aesKey = "YourBase64EncodedAES256KeyHere==";
        public static string m_password = "";
        public string m_RootPath = "";
        public static string m_SettingsName = "ViwerSettings";
        public static bool m_Logined = false;
        private void button1_Click(object sender, EventArgs e)
        {
            try
            {
                DialogResult r = folderBrowserDialog1.ShowDialog();
                if (r == DialogResult.OK)
                {
                    LoadFolderPath(folderBrowserDialog1.SelectedPath);
                }
                else
                {
                    // If user cancels, offer manual path entry
                    OfferManualPathInput();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"button1_Click error: {ex.Message}");
                // If dialog fails, offer manual entry
                MessageBox.Show($"The folder browser dialog encountered an error.\n\nThis sometimes happens with external drives or deeply nested paths.\n\nYou can enter the path manually instead.", 
                    "Folder Browser Error");
                OfferManualPathInput();
            }
        }

        private void OfferManualPathInput()
        {
            if (MessageBox.Show("Would you like to enter the folder path manually?\n\n" +
                "You can paste the path from File Explorer:\n" +
                "1. Open File Explorer\n" +
                "2. Navigate to your folder\n" +
                "3. Click the address bar and copy the full path\n" +
                "4. Click OK and paste it here", 
                "Manual Path Entry", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                string networkPath = PromptForNetworkPath();
                if (!string.IsNullOrEmpty(networkPath))
                {
                    LoadFolderPath(networkPath);
                }
            }
        }

        // Prompt user for network path with validation
        private string PromptForNetworkPath()
        {
            Form promptForm = new Form()
            {
                Text = "Enter Network Path",
                Width = 600,
                Height = 250,
                StartPosition = FormStartPosition.CenterParent,
                FormBorderStyle = FormBorderStyle.FixedDialog,
                MaximizeBox = false,
                MinimizeBox = false,
                ShowIcon = false
            };

            Label label = new Label() 
            { 
                Left = 20, Top = 20, 
                Text = "Network/NAS Path:", 
                Width = 550, Height = 20 
            };

            Label hintLabel = new Label() 
            { 
                Left = 20, Top = 50, 
                Text = "Examples:\n" +
                       "  • Mapped drive: Y:\\USB-Hard-Drive-2\\Xuebing\\Data\\...\n" +
                       "  • UNC path: \\\\QNAP-device\\share\\folder\n" +
                       "  • Local: C:\\Users\\Pictures",
                Width = 550, Height = 60, 
                ForeColor = System.Drawing.Color.Gray, 
                Font = new System.Drawing.Font(this.Font, System.Drawing.FontStyle.Italic) 
            };

            TextBox textBox = new TextBox() 
            { 
                Left = 20, Top = 120, 
                Width = 550, Height = 30, 
                Multiline = false,
                Text = "Y:\\" // Default to Y: drive for QNAP
            };

            Label pasteHint = new Label()
            {
                Left = 20, Top = 155,
                Text = "💡 Tip: Copy path from File Explorer address bar and paste here",
                Width = 550, Height = 20,
                ForeColor = System.Drawing.Color.Blue,
                Font = new System.Drawing.Font(this.Font, System.Drawing.FontStyle.Regular)
            };

            Button okButton = new Button() { Text = "OK", Left = 410, Width = 80, Top = 185, DialogResult = DialogResult.OK };
            Button cancelButton = new Button() { Text = "Cancel", Left = 500, Width = 80, Top = 185, DialogResult = DialogResult.Cancel };

            promptForm.Controls.Add(label);
            promptForm.Controls.Add(hintLabel);
            promptForm.Controls.Add(textBox);
            promptForm.Controls.Add(pasteHint);
            promptForm.Controls.Add(okButton);
            promptForm.Controls.Add(cancelButton);
            promptForm.AcceptButton = okButton;
            promptForm.CancelButton = cancelButton;

            if (promptForm.ShowDialog() == DialogResult.OK)
            {
                string inputPath = textBox.Text;
                System.Diagnostics.Debug.WriteLine($"PromptForNetworkPath: User entered: [{inputPath}]");
                return inputPath;
            }
            return null;
        }

        // Helper method to load a folder path (supports both local and network paths)
        private void LoadFolderPath(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
                return;

            try
            {
                // Normalize path (remove quotes if present, trim whitespace)
                path = path.Trim('"', '\'').Trim();

                System.Diagnostics.Debug.WriteLine($"=== LoadFolderPath Debug ===");
                System.Diagnostics.Debug.WriteLine($"Input path: [{path}]");
                System.Diagnostics.Debug.WriteLine($"Path length: {path.Length}");
                System.Diagnostics.Debug.WriteLine($"Path exists (Directory.Exists): {Directory.Exists(path)}");

                // Try alternative methods to check path existence
                bool existsViaDirectoryInfo = false;
                bool existsViaFileInfo = false;
                Exception dirInfoEx = null;
                Exception fileInfoEx = null;

                try
                {
                    var dirInfo = new System.IO.DirectoryInfo(path);
                    existsViaDirectoryInfo = dirInfo.Exists;
                    System.Diagnostics.Debug.WriteLine($"Path exists (DirectoryInfo.Exists): {existsViaDirectoryInfo}");
                }
                catch (Exception ex)
                {
                    dirInfoEx = ex;
                    System.Diagnostics.Debug.WriteLine($"DirectoryInfo error: {ex.Message}");
                }

                try
                {
                    var fileInfo = new System.IO.FileInfo(path);
                    existsViaFileInfo = fileInfo.Attributes != (System.IO.FileAttributes)(-1);
                    System.Diagnostics.Debug.WriteLine($"Path attributes: {fileInfo.Attributes}");
                }
                catch (Exception ex)
                {
                    fileInfoEx = ex;
                    System.Diagnostics.Debug.WriteLine($"FileInfo error: {ex.Message}");
                }

                // Try to get attributes directly
                try
                {
                    var attrs = System.IO.File.GetAttributes(path);
                    bool isDirectory = (attrs & System.IO.FileAttributes.Directory) == System.IO.FileAttributes.Directory;
                    System.Diagnostics.Debug.WriteLine($"GetAttributes success. IsDirectory: {isDirectory}");
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"GetAttributes error: {ex.Message}");
                }

                System.Diagnostics.Debug.WriteLine($"=== End Debug ===");

                // If none of the methods work, show diagnostics to user
                if (!Directory.Exists(path) && !existsViaDirectoryInfo)
                {
                    MessageBox.Show($"Path does not exist or is not accessible:\n{path}\n\n" +
                        "This path works in File Explorer but not in this application.\n\n" +
                        "Possible issues:\n" +
                        "• External drive (USB) may have disconnected\n" +
                        "• Network drive connection lost\n" +
                        "• Permission issue specific to this application\n" +
                        "• Special characters in path\n\n" +
                        "Please:\n" +
                        "1. Verify the path still works in File Explorer\n" +
                        "2. Check the Debug Output window for detailed diagnostics\n" +
                        "3. Try copying fresh from File Explorer address bar", 
                        "Invalid Path", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                // Try to list directory contents to verify access
                try
                {
                    string[] dirs = Directory.GetDirectories(path);
                    System.Diagnostics.Debug.WriteLine($"Successfully listed {dirs.Length} subdirectories");
                }
                catch (UnauthorizedAccessException)
                {
                    System.Diagnostics.Debug.WriteLine($"Access denied to: {path}");
                    MessageBox.Show($"Access denied to path:\n{path}\n\nYou may need network credentials to access this path.", 
                        "Access Denied", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                catch (Exception accessEx)
                {
                    System.Diagnostics.Debug.WriteLine($"Error accessing directory: {accessEx.GetType().Name}: {accessEx.Message}");
                    MessageBox.Show($"Cannot access path:\n{path}\n\nError: {accessEx.Message}", 
                        "Access Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                m_RootPath = path;
                tvFolder.Nodes.Clear();

                // Show status while loading
                System.Diagnostics.Debug.WriteLine($"Loading folder contents: {path}");
                this.Cursor = Cursors.WaitCursor;

                try
                {
                    LoadFolderToTree(m_RootPath, tvFolder.Nodes);
                    this.Cursor = Cursors.Default;
                    MessageBox.Show($"Successfully loaded:\n{path}", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception loadEx)
                {
                    this.Cursor = Cursors.Default;
                    System.Diagnostics.Debug.WriteLine($"Error loading folder tree: {loadEx.GetType().Name}: {loadEx.Message}");
                    MessageBox.Show($"Error loading folder contents:\n{loadEx.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                this.Cursor = Cursors.Default;
                System.Diagnostics.Debug.WriteLine($"LoadFolderPath Exception: {ex.GetType().Name}: {ex.Message}");
                MessageBox.Show($"Error accessing path:\n{ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        void calculateBar(int imgw, int imgh, ref int currw, ref int currh)
        {
            double maxw=2*imgw;
            double maxh=2*imgh;
            double width = currw;
            if (width >= maxw) width = maxw;
            double value = 0;
            double temph = 0;
            currw =(int) width;
            temph = (int)(width * (maxh / maxw));
            if (temph >= currh)
            {
                temph = currh;
                width = temph * (maxw / maxh);
                
            }
            value = (width / maxw) * 100;
            currw = (int)width;
            currh = (int)temph;
            trackScale.Value = (int)value;
        }
        void AdjustPic()
        {
            if (pBPic.Image == null) return;
            double imgw = (double)pBPic.Image.Width;
            double maxw = 2 * imgw;
            double currw = (trackScale.Value * maxw) / 100.0;
            double maxh=2*pBPic.Image.Height;
            double currh = currw * (maxh / maxw);
            pBPic.Width = (int)currw;
            pBPic.Height = (int)currh;
            pBPic.Left = 0;
            pBPic.Top = 0;
        }
        void ShowPicture(string fname)
        {
            if (!m_Logined)
            {
                MessageBox.Show("You haven't logged in, please login first!");
                return;
            }
            if (pBPic.Image != null) pBPic.Image.Dispose();
            pBPic.Image = null;
            trackScale.Visible = false;
            try
            {
                byte[] dbb =null;
                MemoryStream ms =null;
                System.Drawing.Image img = null;
                string ext = Path.GetExtension(fname).Trim().ToUpper();

                if(ext==".YPG" || ext=="YPG")
                {
                    try
                    {
                        dbb = YEncrypt.DecryptFileToBuffer(fname, key);

                        if (dbb == null || dbb.Length < 4)
                        {
                            System.Diagnostics.Debug.WriteLine($"ShowPicture: Decryption returned invalid data ({(dbb?.Length ?? 0)} bytes)");
                            return;
                        }

                        ms = new MemoryStream(dbb);
                        ms.Position = 0;
                        img = new Bitmap((System.IO.Stream)ms);
                    }
                    catch (Exception decryptEx)
                    {
                        System.Diagnostics.Debug.WriteLine($"ShowPicture: Decryption error: {decryptEx.Message}");
                        throw;
                    }
                }
                else if(ext==".XPG" || ext=="XPG")
                {
                    try
                    {
                        dbb = YAESEncrypt.DecryptFileToBuffer(fname, aesKey);

                        if (dbb == null || dbb.Length < 4)
                        {
                            System.Diagnostics.Debug.WriteLine($"ShowPicture: AES Decryption returned invalid data ({(dbb?.Length ?? 0)} bytes)");
                            return;
                        }

                        ms = new MemoryStream(dbb);
                        ms.Position = 0;
                        img = new Bitmap((System.IO.Stream)ms);
                    }
                    catch (Exception decryptEx)
                    {
                        System.Diagnostics.Debug.WriteLine($"ShowPicture: AES Decryption error: {decryptEx.Message}");
                        throw;
                    }
                }
                else if (ext == ".MOV" || ext == ".MPEG" || ext == ".AVI" || ext == ".DAT")
                {
                    return;
                }
                else
                {
                    dbb = YEncrypt.LoadFileToBuffer(fname);
                    ms = new MemoryStream(dbb);
                    img = new Bitmap((System.IO.Stream)ms);
                }

                if (rbStretchImage.Checked)
                {
                    pBPic.Dock = DockStyle.Fill;
                    pBPic.SizeMode = PictureBoxSizeMode.StretchImage;
                }
                if (rbCenterImage.Checked)
                {
                    pBPic.Dock = DockStyle.None;
                    pBPic.Left = 0;
                    pBPic.Top = 0;
                    pBPic.Width = img.Width;
                    pBPic.Height = img.Height ;
                    pBPic.SizeMode = PictureBoxSizeMode.CenterImage;
                }
                if (rbZoom.Checked)
                {
                    trackScale.Visible = true;
                    int currw = panelPic.Width ;
                    int currh = panelPic.Height;
                    calculateBar(img.Width,img.Height,ref currw,ref currh);
                    pBPic.Dock = DockStyle.None;
                    pBPic.Left = 0;
                    pBPic.Top = 0;
                    pBPic.Width = currw;
                    pBPic.Height = currh;
                    pBPic.SizeMode = PictureBoxSizeMode.Zoom;

                }
                if (rbAutoSize.Checked)
                {
                    pBPic.Dock = DockStyle.None;
                    pBPic.Left = 0;
                    pBPic.Top = 0;
                    pBPic.Width = panelPic.Width;
                    pBPic.Height = panelPic.Height;
                    pBPic.SizeMode = PictureBoxSizeMode.AutoSize;
                }

                if (rbNormal.Checked)
                {
                    pBPic.Dock = DockStyle.None;
                    pBPic.Left = 0;
                    pBPic.Top = 0;
                    pBPic.Width = img.Width;
                    pBPic.Height = img.Height;
                    pBPic.SizeMode = PictureBoxSizeMode.Normal;
                }
                pBPic.Image = img;
            }
            catch (Exception ex) 
            { 
                System.Diagnostics.Debug.WriteLine($"ShowPicture Exception: {ex.GetType().Name}: {ex.Message}\n{ex.StackTrace}");
                MessageBox.Show($"Error loading image:\n{ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

        }
        void LoadFolderToTree(string folder, TreeNodeCollection tc)
        {
            if (tc == null) return;
            string[] folders = Directory.GetDirectories(folder);
            foreach (string fd in folders)
            {
                TreeNode tv = new TreeNode();
                tv.Text = Path.GetFileName(fd);
                tv.Tag = "Folder";
                tv.Name = fd;
                tv.ImageKey = "Folder";
                LoadFolderToTree(folder + "\\" + tv.Text, tv.Nodes);
                tc.Add(tv);
            }
            string patt = "*.*";
            List<string> filesList = new List<string>();
            if (cbOnlyYPG.Checked)
            {
                filesList.AddRange(Directory.GetFiles(folder, "*.ypg"));
                filesList.AddRange(Directory.GetFiles(folder, "*.xpg"));
            }
            else
            {
                filesList.AddRange(Directory.GetFiles(folder, patt));
            }
            string[] files = filesList.ToArray();
            foreach (string ft in files)
            {
                TreeNode tv1 = new TreeNode();
                tv1.Text = Path.GetFileName(ft);
                tv1.Tag = "File";
                tv1.Name = ft;
                tv1.ImageKey = "File";
                tc.Add(tv1);
            }
        }
        public void EncryptAllFiles(string path, string extToEnc,string objext, string key, bool includeSubDir, bool deleteOriginal)
        {
            string[] files = Directory.GetFiles(path, "*." + extToEnc.Trim());
            if (files == null) return;
            foreach (string t in files)
            {
                if (Path.GetExtension(t).Trim().ToUpper() != "YPG")
                {
                    try
                    {
                        EncriptFile(t, objext, key);
                        if (deleteOriginal)
                        {
                            File.Delete(t);
                        }
                    }
                    catch
                    {
                    }
                }
            }
            if (includeSubDir)
            {
                string[] folders = Directory.GetDirectories(path);
                foreach (string fd in folders)
                {
                    EncryptAllFiles(fd, extToEnc, objext, key, includeSubDir, deleteOriginal);
                }
            }
        }
        void EncriptFile(string original, string objext, string key)
        {
            string fname = original;
            string tfname = System.IO.Path.GetFileNameWithoutExtension(fname);
            string path = System.IO.Path.GetDirectoryName(fname);
            string ofname = path + "\\" + tfname;
            ofname = ofname.Trim();
            if (objext.StartsWith(".")) ofname = ofname + objext;
            else ofname = ofname + "." + objext;

            if (objext.ToLower().Contains("xpg"))
            {
                YAESEncrypt.EncryptFile(fname, ofname, aesKey);
            }
            else
            {
                YEncrypt.EncryptFile(fname, ofname, key);
            }
        }

        private void bEncryptDirectory_Click(object sender, EventArgs e)
        {
            if (!m_Logined)
            {
                MessageBox.Show("You haven't logged in, please login first!");
                return;
            }
            DialogResult r = folderBrowserDialog1.ShowDialog();
            if (r == DialogResult.OK)
            {
                EncryptAllFiles(folderBrowserDialog1.SelectedPath,cbFileExtentions.Text,"ypg", key, cbIncludeSub.Checked,cbDeleteOriginal.Checked);
            }
        }

        private void tvFolder_AfterSelect(object sender, TreeViewEventArgs e)
        {
            if (tvFolder.SelectedNode == null) return;
            if (tvFolder.SelectedNode.Tag.ToString() == "Folder") return;
            string fname = tvFolder.SelectedNode.Name;
            ShowPicture(fname);
        }


        private void Form1_Load(object sender, EventArgs e)
        {
            LoadSettingsFromCurrentUser();
        }
        public static  bool SaveAllSettingsInCurrentUser()
        {
            YInfoManage info = new YInfoManage();

            #region private settings
            info.Add("ViewerSettings",m_password);
            #endregion
            return info.Save(m_SettingsName);
        }
        public static bool LoadSettingsFromCurrentUser()
        {
            YInfoManage info = new YInfoManage();
            bool re = info.Load(m_SettingsName);//info.LoadFromDisk(fn);

            #region private settings
            if (!re)
            {
                m_password = "mbfs1";
            }
            else
            {
                m_password = info.GetContent("ViewerSettings");
            }
            #endregion

            return re;
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void loginToolStripMenuItem_Click(object sender, EventArgs e)
        {
            FPassword f = new FPassword();
            f.ShowDialog();
        }

        private void logoutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Form1.m_Logined = false;
        }

        private void changePasswordToolStripMenuItem_Click(object sender, EventArgs e)
        {
            FChangePass f = new FChangePass();
            f.ShowDialog();
        }

        private void bPrevious_Click(object sender, EventArgs e)
        {
            TreeNode tv = tvFolder.SelectedNode;
            if (tv == null) return;
            TreeNodeCollection tc = null;
            if (tv.Parent == null) tc = tvFolder.Nodes;
            else tc = tv.Parent.Nodes;
            int index = tc.IndexOf(tv);
            int next=0;
            if (index == 0) next = tc.Count - 1;
            else next = index - 1;
            tvFolder.SelectedNode = tc[next];
        }

        private void bNext_Click(object sender, EventArgs e)
        {
            TreeNode tv = tvFolder.SelectedNode;
            if (tv == null) return;
            TreeNodeCollection tc = null;
            if (tv.Parent == null) tc = tvFolder.Nodes;
            else tc = tv.Parent.Nodes;
            int index = tc.IndexOf(tv);
            int next = 0;
            if (index == tc.Count-1) next = 0;
            else next = index + 1;
            tvFolder.SelectedNode = tc[next];
        }

        private void bClear_Click(object sender, EventArgs e)
        {
            pBPic.Image = null;
            tvFolder.Nodes.Clear();
            m_Logined = false;
           
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {

        }

        private void trackScale_Scroll(object sender, EventArgs e)
        {
            AdjustPic();
        }

        private void bDecrypt_Click(object sender, EventArgs e)
        {
            if (tvFolder.SelectedNode == null) return;
            if (tvFolder.SelectedNode.Tag.ToString() == "Folder") return;
            string fname = tvFolder.SelectedNode.Name;

            byte[] dbb = null;
            MemoryStream ms = null;
            string ext = Path.GetExtension(fname).Trim().ToUpper();
            if (ext == ".YPG" || ext == "YPG")
            {
                YEncrypt.DecryptFile(fname, "D:\\tttt\\test.zip", key);
                dbb = YEncrypt.DecryptFileToBuffer(fname, key);
                ms = new MemoryStream(dbb);

                //System.Drawing.Image img = new Bitmap((System.IO.Stream)ms);
            }

        }
    }
}
