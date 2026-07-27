
using System;
using System.IO;
using System.Security;
using System.Security.Cryptography;
using System.Runtime.InteropServices;
using System.Text;

namespace YXBPictureViewer
{
    public class YEncrypt
    {
        //  Call this function to remove the key from memory after use for security
        [System.Runtime.InteropServices.DllImport("KERNEL32.DLL", EntryPoint = "RtlZeroMemory")]
        public static extern bool ZeroMemory(IntPtr Destination, int Length);

        // Function to Generate a 64 bits Key.
        public static string GenerateKey()
        {
            // Create an instance of Symmetric Algorithm using DES (64 bits).
            using (var desCrypto = new DESCryptoServiceProvider())
            {
                // Use the Automatically generated key for Encryption. 
                return Encoding.ASCII.GetString(desCrypto.Key);
            }
        }

        public static byte[] EncryptBufferRecordLength(byte[] buffer, string sKey)
        {
            if (buffer == null) return null;
            if (buffer.Length <= 0) return null;
            Int32 originlen = buffer.Length;
            MemoryStream msEncrypted = new MemoryStream();
            using (var DES = new DESCryptoServiceProvider())
            {
                DES.Key = Encoding.ASCII.GetBytes(sKey);
                DES.IV = Encoding.ASCII.GetBytes(sKey);
                ICryptoTransform desencrypt = DES.CreateEncryptor();
                CryptoStream cryptostream = new CryptoStream(msEncrypted,
                   desencrypt,
                   CryptoStreamMode.Write);
                cryptostream.Write(buffer, 0, buffer.Length);
                cryptostream.Close();
                byte[] b = msEncrypted.ToArray();
                byte[] temp = BitConverter.GetBytes(originlen);
                byte[] buf = new byte[b.Length + temp.Length];
                Array.Copy(temp, 0, buf, 0, temp.Length);
                Array.Copy(b, 0, buf, temp.Length, b.Length);
                return buf;
            }
        }

        public static byte[] DecryptBufferRecordedLength(byte[] buffer, string sKey)
        {
            if (buffer == null) return null;
            if (buffer.Length <= 0) return null;
            Int32 len = 0;
            int intlen;
            byte[] temp = BitConverter.GetBytes(len);
            intlen = temp.Length;
            Array.Copy(buffer, 0, temp, 0, temp.Length);
            len = BitConverter.ToInt32(temp, 0);

            System.Diagnostics.Debug.WriteLine($"DecryptBufferRecordedLength: Stored length={len}, Buffer length={buffer.Length}, Encrypted data length={buffer.Length - intlen}");

            using (var DES = new DESCryptoServiceProvider())
            {
                // DES requires exactly 8 bytes for key and IV
                DES.Key = Encoding.ASCII.GetBytes(sKey);
                DES.IV = Encoding.ASCII.GetBytes(sKey);
                MemoryStream msRead = new MemoryStream(buffer, intlen, buffer.Length - intlen);
                ICryptoTransform desdecrypt = DES.CreateDecryptor();
                msRead.Position = 0;
                CryptoStream cryptostreamDecr = new CryptoStream(msRead, desdecrypt, CryptoStreamMode.Read);

                // Read all available data from CryptoStream
                byte[] b = new byte[len];
                int totalBytesRead = 0;
                int bytesRead;

                while (totalBytesRead < len)
                {
                    bytesRead = cryptostreamDecr.Read(b, totalBytesRead, len - totalBytesRead);
                    if (bytesRead == 0) break;
                    totalBytesRead += bytesRead;
                }

                System.Diagnostics.Debug.WriteLine($"DecryptBufferRecordedLength: Requested {len} bytes, read {totalBytesRead} bytes");

                if (totalBytesRead < len)
                {
                    System.Array.Resize(ref b, totalBytesRead);
                }

                return b;
            }
        }

        public static byte[] EncryptBuffer(byte[] buffer, string sKey)
        {
            if(buffer==null) return null;
            if (buffer.Length <= 0) return null;
            MemoryStream msEncrypted = new MemoryStream();
            using (var DES = new DESCryptoServiceProvider())
            {
                DES.Key = Encoding.ASCII.GetBytes(sKey);
                DES.IV = Encoding.ASCII.GetBytes(sKey);
                ICryptoTransform desencrypt = DES.CreateEncryptor();
                CryptoStream cryptostream = new CryptoStream(msEncrypted,
                   desencrypt,
                   CryptoStreamMode.Write);
                cryptostream.Write(buffer, 0, buffer.Length);
                cryptostream.Close();
                return msEncrypted.ToArray();
            }
        }

        public static byte[] DecryptBuffer(byte[] buffer, string sKey, int len)
        {
            if (buffer == null) return null;
            if (buffer.Length <= 0) return null;
            using (var DES = new DESCryptoServiceProvider())
            {
                // DES requires exactly 8 bytes for key and IV
                DES.Key = Encoding.ASCII.GetBytes(sKey);
                DES.IV = Encoding.ASCII.GetBytes(sKey);
                MemoryStream msRead = new MemoryStream(buffer);
                ICryptoTransform desdecrypt = DES.CreateDecryptor();
                msRead.Position = 0;
                CryptoStream cryptostreamDecr = new CryptoStream(msRead, desdecrypt, CryptoStreamMode.Read);

                byte[] b = new byte[len];
                int totalBytesRead = 0;
                int bytesRead;

                while (totalBytesRead < len)
                {
                    bytesRead = cryptostreamDecr.Read(b, totalBytesRead, len - totalBytesRead);
                    if (bytesRead == 0) break;
                    totalBytesRead += bytesRead;
                }

                if (totalBytesRead < len)
                {
                    System.Array.Resize(ref b, totalBytesRead);
                }

                return b;
            }
        }

        public static byte[] EncryptFileToBuffer(string sInputFilename,string key)
        {
            using (FileStream fsInput = new FileStream(sInputFilename, FileMode.Open, FileAccess.Read))
            {
                byte[] buffer = new byte[fsInput.Length];
                fsInput.Read(buffer, 0, (int)fsInput.Length);
                byte[] buf = EncryptBufferRecordLength(buffer, key);
                return buf;
            }
        }
        public static void EncryptFile(string sInputFilename,string sOutputFilename,string sKey)
        {
            byte[] buff = EncryptFileToBuffer(sInputFilename, sKey);
            WriteBufferToFile(buff, sOutputFilename);
        }
        public static byte[] DecryptFileToBuffer(string sInputFilename, string sKey)
        {
            byte[] buff = LoadFileToBuffer(sInputFilename);
            byte[] dbuff = DecryptBufferRecordedLength(buff, sKey);
            return dbuff;
        }

        public static void DecryptFile(string sInputFilename,string sOutputFilename,string sKey)
        {
            byte[] buff = DecryptFileToBuffer(sInputFilename, sKey);
            WriteBufferToFile(buff, sOutputFilename);
        }
        public static MemoryStream DecryptFileToMemoryStream(string sInputFilename,string sKey)
        {
            byte[] buff = DecryptFileToBuffer(sInputFilename,sKey);
            MemoryStream ms = new MemoryStream(buff);
            return ms;
        }
        public static MemoryStream LoadFileToMemoryStream(string sInputFilename)
        {
            byte[] buffer = LoadFileToBuffer(sInputFilename);
            if (buffer == null || buffer.Length == 0)
            {
                return null;
            }
            System.IO.MemoryStream ms = new System.IO.MemoryStream(buffer);
            ms.Position = 0;
            return ms;
        }
        public static byte[] LoadFileToBuffer(string sInputFilename)
        {
            using (FileStream fsread = new FileStream(sInputFilename,
               FileMode.Open,
               FileAccess.Read))
            {
                byte[] buffer = new byte[fsread.Length];
                fsread.Read(buffer, 0, (int)fsread.Length);
                return buffer;
            }
        }
        public static bool WriteBufferToFile(byte[] buffer, string sOutputFilename)
        {
            try
            {
                using (FileStream fswrite = new FileStream(sOutputFilename, FileMode.OpenOrCreate, FileAccess.Write))
                {
                    fswrite.Write(buffer, 0, buffer.Length);
                    fswrite.Flush();
                }
                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error writing buffer to file: {ex.Message}");
                return false;
            }
        }
        public static byte[] RSAEncrypt(byte[] DataToEncrypt, RSAParameters RSAKeyInfo, bool DoOAEPPadding)
        {
            if (DataToEncrypt == null) return null;
            if (DataToEncrypt.Length <= 0) return null;
            try
            {
                //Create a new instance of RSACryptoServiceProvider.
                RSACryptoServiceProvider RSA = new RSACryptoServiceProvider();

                //Import the RSA Key information. This only needs
                //toinclude the public key information.
                RSA.ImportParameters(RSAKeyInfo);

                //Encrypt the passed byte array and specify OAEP padding.  
                //OAEP padding is only available on Microsoft Windows XP or
                //later.  
                return RSA.Encrypt(DataToEncrypt, DoOAEPPadding);
            }
            //Catch and display a CryptographicException  
            //to the console.
            catch (CryptographicException e)
            {
                Console.WriteLine(e.Message);

                return null;
            }

        }
        public static byte[] RSAEncrypt(byte[] DataToEncrypt, string key, bool DoOAEPPadding)
        {
            if (DataToEncrypt == null) return null;
            if (DataToEncrypt.Length <= 0) return null;
            try
            {
                //Create a new instance of RSACryptoServiceProvider.
                RSACryptoServiceProvider RSA = new RSACryptoServiceProvider();

                //Import the RSA Key information. This only needs
                //toinclude the public key information.
                //RSA.ImportParameters(RSAKeyInfo);
                RSA.FromXmlString(key);

                //Encrypt the passed byte array and specify OAEP padding.  
                //OAEP padding is only available on Microsoft Windows XP or
                //later.  
                return RSA.Encrypt(DataToEncrypt, DoOAEPPadding);
            }
            //Catch and display a CryptographicException  
            //to the console.
            catch (CryptographicException e)
            {
                Console.WriteLine(e.Message);

                return null;
            }

        }

        public static byte[] RSADecrypt(byte[] DataToDecrypt, RSAParameters RSAKeyInfo, bool DoOAEPPadding)
        {
            if (DataToDecrypt == null) return null;
            if (DataToDecrypt.Length <= 0) return null;
            try
            {
                //Create a new instance of RSACryptoServiceProvider.
                RSACryptoServiceProvider RSA = new RSACryptoServiceProvider();

                //Import the RSA Key information. This needs
                //to include the private key information.
                RSA.ImportParameters(RSAKeyInfo);

                //Decrypt the passed byte array and specify OAEP padding.  
                //OAEP padding is only available on Microsoft Windows XP or
                //later.  
                return RSA.Decrypt(DataToDecrypt, DoOAEPPadding);
            }
            //Catch and display a CryptographicException  
            //to the console.
            catch (CryptographicException e)
            {
                Console.WriteLine(e.ToString());

                return null;
            }

        }
        public static byte[] RSADecrypt(byte[] DataToDecrypt, string key, bool DoOAEPPadding)
        {
            if (DataToDecrypt == null) return null;
            if (DataToDecrypt.Length <= 0) return null;
            try
            {
                //Create a new instance of RSACryptoServiceProvider.
                RSACryptoServiceProvider RSA = new RSACryptoServiceProvider();

                //Import the RSA Key information. This needs
                //to include the private key information.
                //RSA.ImportParameters(RSAKeyInfo);
                RSA.FromXmlString(key);

                //Decrypt the passed byte array and specify OAEP padding.  
                //OAEP padding is only available on Microsoft Windows XP or
                //later.  
                return RSA.Decrypt(DataToDecrypt, DoOAEPPadding);
            }
            //Catch and display a CryptographicException  
            //to the console.
            catch (CryptographicException e)
            {
                Console.WriteLine(e.ToString());

                return null;
            }

        }

        public static byte[] RSASignData(byte[] DataToSigh, string key)
        {
            if (DataToSigh == null) return null;
            if (DataToSigh.Length <= 0) return null;
            // create an instance of the RSA implementation class
            RSACryptoServiceProvider x_rsa = new RSACryptoServiceProvider();
            x_rsa.FromXmlString(key);
            // create an instance of the SHA-256 hashing algorithm (modern replacement for SHA-1)
            using (HashAlgorithm x_sha256 = SHA256.Create())
            {
                byte[] x_rsa_signature = x_rsa.SignData(DataToSigh, x_sha256);
                return x_rsa_signature;
            }
        }
        public static bool RSAVerifySignature(byte[] DataToVeryfy, byte[] signature, string key)
        {
            if (DataToVeryfy == null) return false;
            if (DataToVeryfy.Length <= 0) return false;
            // create an instance of the RSA implementation class
            RSACryptoServiceProvider x_rsa = new RSACryptoServiceProvider();
            x_rsa.FromXmlString(key);
            // create an instance of the SHA-256 hashing algorithm (modern replacement for SHA-1)
            using (HashAlgorithm x_sha256 = SHA256.Create())
            {
                bool x_rsa_sig_valid = x_rsa.VerifyData(DataToVeryfy, x_sha256, signature);
                return x_rsa_sig_valid;
            }
        }

        //*****************DSA
        public static byte[] DSASignData(byte[] DataToSigh, string key)
        {
            if (DataToSigh == null) return null;
            if (DataToSigh.Length <= 0) return null;
            // create an instance of the RSA implementation class
            DSACryptoServiceProvider x_dsa = new DSACryptoServiceProvider();
            x_dsa.FromXmlString(key);

            byte[] x_rsa_signature = x_dsa.SignData(DataToSigh);
            // verify the signature, using the plaintext
            //bool x_rsa_sig_valid = x_rsa.VerifyData(x_plaintext, x_sha1, x_rsa_signature);
            return x_rsa_signature;

        }
        public static bool DSAVerifySignature(byte[] DataToVeryfy, byte[] signature, string key)
        {
            if (DataToVeryfy == null) return false;
            if (DataToVeryfy.Length <= 0) return false;
            // create an instance of the RSA implementation class
            DSACryptoServiceProvider x_dsa = new DSACryptoServiceProvider();
            x_dsa.FromXmlString(key);
            //byte[] x_rsa_signature = x_rsa.SignData(DataToSigh, x_sha1);
            // verify the signature, using the plaintext
            bool x_rsa_sig_valid = x_dsa.VerifyData(DataToVeryfy, signature);
            return x_rsa_sig_valid;

        }
    }
}