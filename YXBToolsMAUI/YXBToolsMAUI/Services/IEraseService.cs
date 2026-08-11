namespace YXBToolsMAUI.Services;

public interface IEraseService
{
    /// <summary>
    /// Securely erases a file using specified method
    /// </summary>
    Task<bool> EraseFileAsync(string filePath, EraseMethod method = EraseMethod.Quick, IProgress<(int currentPass, int totalPasses)>? passProgress = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Securely erases all files in a folder
    /// </summary>
    Task<bool> EraseFolderAsync(string folderPath, EraseMethod method = EraseMethod.Quick, bool includeSubfolders = false, IProgress<(int current, int total, string fileName)>? progress = null, IProgress<(int currentPass, int totalPasses)>? passProgress = null, CancellationToken cancellationToken = default);
}
