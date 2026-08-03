using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace YXBKeepassReader.HNoteExport
{
    public static class StringCipher
    {
        private const int SaltSize = 16;
        private const int NonceSize = 12;
        private const int TagSize = 16;
        private const int KeySize = 32;
        private const int Iterations = 210000;
        private const string PayloadPrefix = "v1";

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
    }
}
