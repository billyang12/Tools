using System;
using System.IO;
using System.Text;
using System.Xml;

namespace YXBKeepassReader
{
    /// <summary>
    /// Detects the file format and version of KeePass database files
    /// </summary>
    public class FileFormatDetector
    {
        public enum FileFormat
        {
            Unknown,
            KDBX,
            XML
        }

        public class FormatInfo
        {
            public FileFormat Format { get; set; }
            public string Version { get; set; }
            public bool IsSupported { get; set; }
            public string Message { get; set; }
        }

        /// <summary>
        /// Detects the file format and returns format information
        /// </summary>
        public static FormatInfo DetectFormat(string filePath)
        {
            if (!File.Exists(filePath))
            {
                return new FormatInfo
                {
                    Format = FileFormat.Unknown,
                    IsSupported = false,
                    Message = "File does not exist"
                };
            }

            try
            {
                using (FileStream fs = new FileStream(filePath, FileMode.Open, FileAccess.Read))
                {
                    // Read first few bytes to identify format
                    byte[] header = new byte[4];
                    fs.Read(header, 0, 4);

                    // KDBX files start with specific magic bytes
                    if (header[0] == 0x03 || header[0] == 0x04)
                    {
                        return DetectKDBXVersion(filePath);
                    }

                    // Check for XML format
                    fs.Seek(0, SeekOrigin.Begin);
                    try
                    {
                        byte[] buffer = new byte[1024];
                        fs.Read(buffer, 0, Math.Min(buffer.Length, (int)fs.Length));
                        string content = Encoding.UTF8.GetString(buffer).TrimStart();

                        if (content.StartsWith("<?xml") || content.StartsWith("<KeePassFile"))
                        {
                            return new FormatInfo
                            {
                                Format = FileFormat.XML,
                                Version = "N/A",
                                IsSupported = true,
                                Message = "XML format detected - fully supported"
                            };
                        }
                    }
                    catch { }

                    return new FormatInfo
                    {
                        Format = FileFormat.Unknown,
                        IsSupported = false,
                        Message = "Unknown file format"
                    };
                }
            }
            catch (Exception ex)
            {
                return new FormatInfo
                {
                    Format = FileFormat.Unknown,
                    IsSupported = false,
                    Message = "Error reading file: " + ex.Message
                };
            }
        }

        private static FormatInfo DetectKDBXVersion(string filePath)
        {
            try
            {
                using (FileStream fs = new FileStream(filePath, FileMode.Open, FileAccess.Read))
                {
                    byte[] header = new byte[124];
                    fs.Read(header, 0, Math.Min(header.Length, (int)Math.Min(124, fs.Length)));

                    if (header.Length >= 4)
                    {
                        byte versionMajor = header[0];
                        byte versionMinor = header[1];

                        if (versionMajor == 0x03)
                        {
                            return new FormatInfo
                            {
                                Format = FileFormat.KDBX,
                                Version = "3",
                                IsSupported = true,
                                Message = "KDBX 3.x format detected - supported by KeePassLib 2.30.0"
                            };
                        }
                        else if (versionMajor == 0x04)
                        {
                            if (versionMinor == 0x00)
                            {
                                return new FormatInfo
                                {
                                    Format = FileFormat.KDBX,
                                    Version = "4.0",
                                    IsSupported = true,
                                    Message = "KDBX 4.0 format detected - supported"
                                };
                            }
                            else if (versionMinor == 0x01)
                            {
                                return new FormatInfo
                                {
                                    Format = FileFormat.KDBX,
                                    Version = "4.1",
                                    IsSupported = false,
                                    Message = "KDBX 4.1 format detected - may not be fully supported by KeePassLib 2.30.0. " +
                                             "Please export as XML from KeePass:\n" +
                                             "File → Export → XML (*.xml)"
                                };
                            }
                            else
                            {
                                return new FormatInfo
                                {
                                    Format = FileFormat.KDBX,
                                    Version = "4." + versionMinor,
                                    IsSupported = false,
                                    Message = $"KDBX 4.{versionMinor} format detected - may not be supported. " +
                                             "Please export as XML from KeePass."
                                };
                            }
                        }
                    }
                }
            }
            catch { }

            return new FormatInfo
            {
                Format = FileFormat.KDBX,
                Version = "Unknown",
                IsSupported = false,
                Message = "Could not determine KDBX version"
            };
        }
    }
}
