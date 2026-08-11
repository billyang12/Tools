using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace YXBToolsMAUI.Services;

public class EncryptionService : IEncryptionService
{
    private const int KeySize = 256;
    private const int NonceSize = 12;
    private const int TagSize = 16;
    private const int SaltSize = 32;
    private const int Iterations = 100000;
    private const string EncryptedFileExtension = ".yxbenc";

    private class FileMetadata
    {
        public string OriginalFileName { get; set; } = string.Empty;
        public string OriginalExtension { get; set; } = string.Empty;
        public long OriginalSize { get; set; }
        public DateTime OriginalCreationTime { get; set; }
        public DateTime OriginalModifiedTime { get; set; }
    }

    public async Task<bool> EncryptFileAsync(string sourceFilePath, string outputFilePath, string password, CancellationToken cancellationToken = default)
    {
        try
        {
            if (!File.Exists(sourceFilePath))
                return false;

            var fileInfo = new FileInfo(sourceFilePath);
            var metadata = new FileMetadata
            {
                OriginalFileName = Path.GetFileNameWithoutExtension(fileInfo.Name),
                OriginalExtension = fileInfo.Extension,
                OriginalSize = fileInfo.Length,
                OriginalCreationTime = fileInfo.CreationTime,
                OriginalModifiedTime = fileInfo.LastWriteTime
            };

            var salt = RandomNumberGenerator.GetBytes(SaltSize);
            var nonce = RandomNumberGenerator.GetBytes(NonceSize);

            using var key = new Rfc2898DeriveBytes(password, salt, Iterations, HashAlgorithmName.SHA256);
            var derivedKey = key.GetBytes(KeySize / 8);

            using var aes = new AesGcm(derivedKey, TagSize);

            var metadataJson = JsonSerializer.Serialize(metadata);
            var metadataBytes = Encoding.UTF8.GetBytes(metadataJson);
            var metadataLength = BitConverter.GetBytes(metadataBytes.Length);

            await using var sourceStream = File.OpenRead(sourceFilePath);
            await using var outputStream = File.Create(outputFilePath);

            outputStream.Write(salt, 0, salt.Length);
            outputStream.Write(nonce, 0, nonce.Length);
            outputStream.Write(metadataLength, 0, metadataLength.Length);

            var metadataCiphertext = new byte[metadataBytes.Length];
            var metadataTag = new byte[TagSize];
            aes.Encrypt(nonce, metadataBytes, metadataCiphertext, metadataTag);

            outputStream.Write(metadataTag, 0, metadataTag.Length);
            outputStream.Write(metadataCiphertext, 0, metadataCiphertext.Length);

            const int bufferSize = 64 * 1024;
            var buffer = new byte[bufferSize];
            int bytesRead;

            while ((bytesRead = await sourceStream.ReadAsync(buffer, 0, buffer.Length, cancellationToken)) > 0)
            {
                cancellationToken.ThrowIfCancellationRequested();

                var plaintext = new byte[bytesRead];
                Array.Copy(buffer, plaintext, bytesRead);

                var ciphertext = new byte[bytesRead];
                var tag = new byte[TagSize];
                var currentNonce = RandomNumberGenerator.GetBytes(NonceSize);

                aes.Encrypt(currentNonce, plaintext, ciphertext, tag);

                await outputStream.WriteAsync(currentNonce, 0, currentNonce.Length, cancellationToken);
                await outputStream.WriteAsync(tag, 0, tag.Length, cancellationToken);
                await outputStream.WriteAsync(ciphertext, 0, ciphertext.Length, cancellationToken);
            }

            return true;
        }
        catch
        {
            return false;
        }
    }

    public async Task<bool> EncryptFolderAsync(string sourceFolderPath, string outputFolderPath, string password, IProgress<(int current, int total, string fileName)>? progress = null, CancellationToken cancellationToken = default)
    {
        try
        {
            if (!Directory.Exists(sourceFolderPath))
                return false;

            Directory.CreateDirectory(outputFolderPath);

            var files = Directory.GetFiles(sourceFolderPath, "*", SearchOption.AllDirectories);
            var total = files.Length;
            var current = 0;

            foreach (var file in files)
            {
                cancellationToken.ThrowIfCancellationRequested();

                current++;
                var relativePath = Path.GetRelativePath(sourceFolderPath, file);
                var outputFile = Path.Combine(outputFolderPath, relativePath + EncryptedFileExtension);
                var outputDir = Path.GetDirectoryName(outputFile);

                if (!string.IsNullOrEmpty(outputDir))
                    Directory.CreateDirectory(outputDir);

                progress?.Report((current, total, Path.GetFileName(file)));

                var success = await EncryptFileAsync(file, outputFile, password, cancellationToken);
                if (!success)
                    return false;
            }

            return true;
        }
        catch (OperationCanceledException)
        {
            return false;
        }
        catch
        {
            return false;
        }
    }

    public async Task<(bool success, string? originalFileName, string? originalExtension)> DecryptFileAsync(string encryptedFilePath, string outputFolderPath, string password, CancellationToken cancellationToken = default)
    {
        try
        {
            if (!File.Exists(encryptedFilePath))
                return (false, null, null);

            await using var encryptedStream = File.OpenRead(encryptedFilePath);

            var salt = new byte[SaltSize];
            await encryptedStream.ReadAsync(salt, 0, salt.Length);

            var nonce = new byte[NonceSize];
            await encryptedStream.ReadAsync(nonce, 0, nonce.Length);

            var metadataLengthBytes = new byte[4];
            await encryptedStream.ReadAsync(metadataLengthBytes, 0, metadataLengthBytes.Length);
            var metadataLength = BitConverter.ToInt32(metadataLengthBytes, 0);

            using var key = new Rfc2898DeriveBytes(password, salt, Iterations, HashAlgorithmName.SHA256);
            var derivedKey = key.GetBytes(KeySize / 8);

            using var aes = new AesGcm(derivedKey, TagSize);

            var metadataTag = new byte[TagSize];
            await encryptedStream.ReadAsync(metadataTag, 0, metadataTag.Length);

            var metadataCiphertext = new byte[metadataLength];
            await encryptedStream.ReadAsync(metadataCiphertext, 0, metadataLength);

            var metadataPlaintext = new byte[metadataLength];

            try
            {
                aes.Decrypt(nonce, metadataCiphertext, metadataTag, metadataPlaintext);
            }
            catch (CryptographicException)
            {
                return (false, null, null);
            }

            var metadataJson = Encoding.UTF8.GetString(metadataPlaintext);
            var metadata = JsonSerializer.Deserialize<FileMetadata>(metadataJson);

            if (metadata == null)
                return (false, null, null);

            Directory.CreateDirectory(outputFolderPath);
            var outputFilePath = Path.Combine(outputFolderPath, metadata.OriginalFileName + metadata.OriginalExtension);

            await using var outputStream = File.Create(outputFilePath);

            const int bufferSize = 64 * 1024;

            while (encryptedStream.Position < encryptedStream.Length)
            {
                cancellationToken.ThrowIfCancellationRequested();

                var currentNonce = new byte[NonceSize];
                var bytesRead = await encryptedStream.ReadAsync(currentNonce, 0, currentNonce.Length, cancellationToken);
                if (bytesRead == 0) break;

                var tag = new byte[TagSize];
                await encryptedStream.ReadAsync(tag, 0, tag.Length, cancellationToken);

                var remaining = encryptedStream.Length - encryptedStream.Position;
                var chunkSize = (int)Math.Min(bufferSize, remaining);
                var ciphertext = new byte[chunkSize];
                await encryptedStream.ReadAsync(ciphertext, 0, chunkSize, cancellationToken);

                var plaintext = new byte[chunkSize];

                try
                {
                    aes.Decrypt(currentNonce, ciphertext, tag, plaintext);
                }
                catch (CryptographicException)
                {
                    return (false, null, null);
                }

                await outputStream.WriteAsync(plaintext, 0, plaintext.Length, cancellationToken);
            }

            var decryptedFileInfo = new FileInfo(outputFilePath);
            decryptedFileInfo.CreationTime = metadata.OriginalCreationTime;
            decryptedFileInfo.LastWriteTime = metadata.OriginalModifiedTime;

            return (true, metadata.OriginalFileName, metadata.OriginalExtension);
        }
        catch
        {
            return (false, null, null);
        }
    }

    public async Task<bool> DecryptFolderAsync(string encryptedFolderPath, string outputFolderPath, string password, IProgress<(int current, int total, string fileName)>? progress = null, CancellationToken cancellationToken = default)
    {
        try
        {
            if (!Directory.Exists(encryptedFolderPath))
                return false;

            Directory.CreateDirectory(outputFolderPath);

            var files = Directory.GetFiles(encryptedFolderPath, $"*{EncryptedFileExtension}", SearchOption.AllDirectories);
            var total = files.Length;
            var current = 0;

            foreach (var file in files)
            {
                cancellationToken.ThrowIfCancellationRequested();

                current++;
                var relativePath = Path.GetRelativePath(encryptedFolderPath, file);
                var relativeDir = Path.GetDirectoryName(relativePath) ?? string.Empty;
                var outputDir = Path.Combine(outputFolderPath, relativeDir);

                Directory.CreateDirectory(outputDir);

                progress?.Report((current, total, Path.GetFileName(file)));

                var result = await DecryptFileAsync(file, outputDir, password, cancellationToken);
                if (!result.success)
                    return false;
            }

            return true;
        }
        catch (OperationCanceledException)
        {
            return false;
        }
        catch
        {
            return false;
        }
    }
}
