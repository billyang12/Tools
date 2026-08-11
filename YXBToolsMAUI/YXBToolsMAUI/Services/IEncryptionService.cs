namespace YXBToolsMAUI.Services;

public interface IEncryptionService
{
    Task<bool> EncryptFileAsync(string sourceFilePath, string outputFilePath, string password, CancellationToken cancellationToken = default);
    Task<bool> EncryptFolderAsync(string sourceFolderPath, string outputFolderPath, string password, IProgress<(int current, int total, string fileName)>? progress = null, CancellationToken cancellationToken = default);
    Task<(bool success, string? originalFileName, string? originalExtension)> DecryptFileAsync(string encryptedFilePath, string outputFolderPath, string password, CancellationToken cancellationToken = default);
    Task<bool> DecryptFolderAsync(string encryptedFolderPath, string outputFolderPath, string password, IProgress<(int current, int total, string fileName)>? progress = null, CancellationToken cancellationToken = default);
}
