using System.Runtime.InteropServices;
using YXBToolsMAUI.Services;

namespace YXBToolsMAUI.Platforms.Windows;

public class FolderPickerService : IFolderPickerService
{
    [DllImport("shell32.dll", SetLastError = true, CharSet = CharSet.Auto)]
    private static extern IntPtr SHBrowseForFolder(ref BrowseInfo lpbi);

    [DllImport("shell32.dll", CharSet = CharSet.Auto)]
    private static extern bool SHGetPathFromIDList(IntPtr pidl, IntPtr pszPath);

    [DllImport("user32.dll")]
    private static extern IntPtr GetActiveWindow();

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
    private struct BrowseInfo
    {
        public IntPtr hwndOwner;
        public IntPtr pidlRoot;
        public IntPtr pszDisplayName;
        public string lpszTitle;
        public uint ulFlags;
        public IntPtr lpfn;
        public IntPtr lParam;
        public int iImage;
    }

    public async Task<string?> PickFolderAsync()
    {
        var tcs = new TaskCompletionSource<string?>();

        var thread = new Thread(() =>
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("FolderPicker: Starting folder selection...");

                var bi = new BrowseInfo();
                bi.hwndOwner = GetActiveWindow();
                bi.lpszTitle = "Select a folder";
                bi.ulFlags = 0x00000001 | 0x00000010 | 0x00000040 | 0x00000200;
                bi.pszDisplayName = Marshal.AllocHGlobal(260 * Marshal.SystemDefaultCharSize);

                try
                {
                    System.Diagnostics.Debug.WriteLine("FolderPicker: Calling SHBrowseForFolder...");
                    IntPtr pidl = SHBrowseForFolder(ref bi);

                    if (pidl != IntPtr.Zero)
                    {
                        System.Diagnostics.Debug.WriteLine("FolderPicker: Folder selected, getting path...");
                        IntPtr path = Marshal.AllocHGlobal(260 * Marshal.SystemDefaultCharSize);
                        try
                        {
                            if (SHGetPathFromIDList(pidl, path))
                            {
                                var selectedPath = Marshal.PtrToStringAuto(path);
                                System.Diagnostics.Debug.WriteLine($"FolderPicker: Path retrieved: {selectedPath}");
                                tcs.SetResult(selectedPath);
                            }
                            else
                            {
                                System.Diagnostics.Debug.WriteLine("FolderPicker: Failed to get path from PIDL");
                                tcs.SetResult(null);
                            }
                        }
                        finally
                        {
                            Marshal.FreeHGlobal(path);
                            Marshal.FreeCoTaskMem(pidl);
                        }
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine("FolderPicker: Dialog cancelled");
                        tcs.SetResult(null);
                    }
                }
                finally
                {
                    Marshal.FreeHGlobal(bi.pszDisplayName);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"FolderPicker error: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"Stack trace: {ex.StackTrace}");
                tcs.SetException(ex);
            }
        });

        thread.SetApartmentState(ApartmentState.STA);
        thread.Start();

        return await tcs.Task;
    }
}
