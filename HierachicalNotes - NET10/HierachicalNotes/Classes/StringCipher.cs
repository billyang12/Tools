using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace HierarchicalNotes.Core.Services;

public static class StringCipher
{
    private const int SaltSize = 16;
    private const int NonceSize = 12;
    private const int TagSize = 16;
    private const int KeySize = 32;
    private const int Iterations = 210000;
    private const string PayloadPrefix = "v1";
    private const string LegacyFixedSalt = "k!k(HJe@!H^{";

    public static string Encrypt(string plainText, string password)
    {
        ValidateString(plainText, nameof(plainText), minLength: 1);
        ValidateString(password, nameof(password), minLength: 8, maxLength: 256);

        var plainBytes = Encoding.UTF8.GetBytes(plainText);
        var salt = RandomNumberGenerator.GetBytes(SaltSize);
        var nonce = RandomNumberGenerator.GetBytes(NonceSize);
        var key = Rfc2898DeriveBytes.Pbkdf2(
            Encoding.UTF8.GetBytes(password),
            salt,
            Iterations,
            HashAlgorithmName.SHA256,
            KeySize);

        var cipherBytes = new byte[plainBytes.Length];
        var tag = new byte[TagSize];
        using (var aes = new AesGcm(key, TagSize))
        {
            aes.Encrypt(nonce, plainBytes, cipherBytes, tag);
        }

        return string.Join('.',
            PayloadPrefix,
            Convert.ToBase64String(salt),
            Convert.ToBase64String(nonce),
            Convert.ToBase64String(tag),
            Convert.ToBase64String(cipherBytes));
    }

    public static string DecryptToString(string cipherText, string password)
    {
        ValidateString(cipherText, nameof(cipherText), minLength: 1);
        ValidateString(password, nameof(password), minLength: 8, maxLength: 256);

        if (TryDecryptV1(cipherText, password, out var plainText))
        {
            return plainText;
        }

        return DecryptLegacy(cipherText, password, LegacyFixedSalt);
    }

    public static string Encrypt(string plainText, string key, string ivSalt)
    {
        return Encrypt(plainText, key);
    }

    public static string DecryptToString(string cipherText, string key, string ivSalt)
    {
        if (TryDecryptV1(cipherText, key, out var plainText))
        {
            return plainText;
        }

        return DecryptLegacy(cipherText, key, ivSalt);
    }

    private static bool TryDecryptV1(string cipherText, string password, out string plainText)
    {
        plainText = string.Empty;
        var parts = cipherText.Split('.');
        if (parts.Length != 5 || !string.Equals(parts[0], PayloadPrefix, StringComparison.Ordinal))
        {
            return false;
        }

        var salt = Convert.FromBase64String(parts[1]);
        var nonce = Convert.FromBase64String(parts[2]);
        var tag = Convert.FromBase64String(parts[3]);
        var cipherBytes = Convert.FromBase64String(parts[4]);

        if (salt.Length != SaltSize || nonce.Length != NonceSize || tag.Length != TagSize)
        {
            throw new CryptographicException("Invalid encrypted payload.");
        }

        var key = Rfc2898DeriveBytes.Pbkdf2(
            Encoding.UTF8.GetBytes(password),
            salt,
            Iterations,
            HashAlgorithmName.SHA256,
            KeySize);

        var plainBytes = new byte[cipherBytes.Length];
        using (var aes = new AesGcm(key, TagSize))
        {
            aes.Decrypt(nonce, cipherBytes, tag, plainBytes);
        }

        plainText = Encoding.UTF8.GetString(plainBytes);
        return true;
    }

    private static string DecryptLegacy(string cipherText, string key, string ivSalt)
    {
        ValidateString(cipherText, nameof(cipherText), minLength: 1);
        ValidateString(key, nameof(key), minLength: 8, maxLength: 128);
        ValidateString(ivSalt, nameof(ivSalt), minLength: 8, maxLength: 128);

        byte[] keyBytes = SHA256.HashData(Encoding.UTF8.GetBytes(key));
        byte[] ivBytes = Encoding.UTF8.GetBytes(ivSalt);
        byte[] cipherBytes = Convert.FromBase64String(cipherText);
        byte[] plain = AesHelper.Decrypt(cipherBytes, keyBytes, ivBytes);
        return Encoding.UTF8.GetString(plain);
    }

    public static void ValidateString(string text, string name, int? minLength = null, int? maxLength = null)
    {
        if (text == null)
        {
            throw new ArgumentNullException(name);
        }

        if (minLength.HasValue && text.Length < minLength.Value)
        {
            throw new Exception($"{name}'s length is less than {minLength.Value}");
        }

        if (maxLength.HasValue && text.Length > maxLength.Value)
        {
            throw new Exception($"{name}'s length is greater than {maxLength.Value}");
        }
    }

    private static class AesHelper
    {
        private const int LegacyKeySize = 256;
        private const int LegacyBlockSize = 128;
        private const int LegacyIterations = 1000;

        private static Aes CreateAesInstance(byte[] key, byte[] iv)
        {
            var aes = Aes.Create();
            aes.KeySize = LegacyKeySize;
            aes.BlockSize = LegacyBlockSize;
            using var derived = new Rfc2898DeriveBytes(key, iv, LegacyIterations, HashAlgorithmName.SHA256);
            aes.Key = derived.GetBytes(aes.KeySize / 8);
            aes.IV = derived.GetBytes(aes.BlockSize / 8);
            return aes;
        }

        public static byte[] Encrypt(byte[] bytesToBeEncrypted, byte[] key, byte[] iv)
        {
            using var aes = CreateAesInstance(key, iv);
            using var ms = new MemoryStream();
            using (var cs = new CryptoStream(ms, aes.CreateEncryptor(), CryptoStreamMode.Write))
            {
                cs.Write(bytesToBeEncrypted, 0, bytesToBeEncrypted.Length);
                cs.FlushFinalBlock();
            }
            return ms.ToArray();
        }

        public static byte[] Decrypt(byte[] bytesToBeDecrypted, byte[] key, byte[] iv)
        {
            using var aes = CreateAesInstance(key, iv);
            using var ms = new MemoryStream();
            using (var cs = new CryptoStream(ms, aes.CreateDecryptor(), CryptoStreamMode.Write))
            {
                cs.Write(bytesToBeDecrypted, 0, bytesToBeDecrypted.Length);
                cs.FlushFinalBlock();
            }

            return TrimZeroPadding(ms.ToArray());
        }

        private static byte[] TrimZeroPadding(byte[] array)
        {
            if (array.Length == 0)
            {
                return array;
            }

            var lastZeroIndex = array.Length;
            for (int i = array.Length - 1; i >= 0; i--)
            {
                if (array[i] == char.MinValue)
                {
                    lastZeroIndex = i;
                }
                else
                {
                    break;
                }
            }

            return array.Where((item, index) => index < lastZeroIndex).ToArray();
        }
    }

    public static string CreateHash(string input)
    {
        const string salt = "&dkdjk*#KK(!";
        return Convert.ToBase64String(SHA256.HashData(Encoding.UTF8.GetBytes(salt + input)));
    }
}
