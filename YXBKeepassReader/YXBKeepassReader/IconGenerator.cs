using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;

namespace YXBKeepassReader
{
    public class IconGenerator
    {
        public static void GenerateApplicationIcon(string outputPath)
        {
            // Create icons in multiple sizes
            int[] sizes = { 16, 32, 48, 64, 128, 256 };

            using (var icon256 = CreateIconBitmap(256))
            {
                // Save as PNG first for the largest size
                string pngPath = Path.Combine(Path.GetDirectoryName(outputPath), "app_icon.png");
                icon256.Save(pngPath, System.Drawing.Imaging.ImageFormat.Png);

                // Create .ico file with multiple sizes
                using (var ms = new MemoryStream())
                {
                    using (var writer = new BinaryWriter(ms))
                    {
                        // ICO header
                        writer.Write((short)0); // Reserved
                        writer.Write((short)1); // Type (1 = ICO)
                        writer.Write((short)sizes.Length); // Number of images

                        var imageData = new MemoryStream[sizes.Length];
                        int offset = 6 + (16 * sizes.Length); // Header + directory entries

                        // Write directory entries
                        for (int i = 0; i < sizes.Length; i++)
                        {
                            using (var bitmap = CreateIconBitmap(sizes[i]))
                            {
                                imageData[i] = new MemoryStream();
                                bitmap.Save(imageData[i], System.Drawing.Imaging.ImageFormat.Png);

                                writer.Write((byte)(sizes[i] == 256 ? 0 : sizes[i])); // Width (0 means 256)
                                writer.Write((byte)(sizes[i] == 256 ? 0 : sizes[i])); // Height
                                writer.Write((byte)0); // Color palette
                                writer.Write((byte)0); // Reserved
                                writer.Write((short)1); // Color planes
                                writer.Write((short)32); // Bits per pixel
                                writer.Write((int)imageData[i].Length); // Image size
                                writer.Write(offset); // Offset

                                offset += (int)imageData[i].Length;
                            }
                        }

                        // Write image data
                        for (int i = 0; i < sizes.Length; i++)
                        {
                            writer.Write(imageData[i].ToArray());
                            imageData[i].Dispose();
                        }
                    }

                    File.WriteAllBytes(outputPath, ms.ToArray());
                }
            }
        }

        private static Bitmap CreateIconBitmap(int size)
        {
            var bitmap = new Bitmap(size, size);
            using (var g = Graphics.FromImage(bitmap))
            {
                g.SmoothingMode = SmoothingMode.AntiAlias;
                g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                g.Clear(Color.Transparent);

                float scale = size / 256f;

                // Draw background circle with gradient
                using (var brush = new LinearGradientBrush(
                    new Rectangle(0, 0, size, size),
                    Color.FromArgb(52, 152, 219),  // Light blue
                    Color.FromArgb(41, 128, 185),  // Darker blue
                    LinearGradientMode.Vertical))
                {
                    g.FillEllipse(brush, 0, 0, size - 1, size - 1);
                }

                // Draw border
                using (var pen = new Pen(Color.FromArgb(30, 98, 145), Math.Max(1, size / 64)))
                {
                    g.DrawEllipse(pen, 0, 0, size - 1, size - 1);
                }

                // Draw key symbol
                DrawKey(g, size, scale);

                // Draw small database/folder icon overlay
                DrawDatabase(g, size, scale);
            }

            return bitmap;
        }

        private static void DrawKey(Graphics g, int size, float scale)
        {
            float keySize = size * 0.5f;
            float x = size * 0.25f;
            float y = size * 0.2f;

            using (var keyBrush = new SolidBrush(Color.White))
            using (var pen = new Pen(Color.FromArgb(200, 200, 200), Math.Max(1, size / 128)))
            {
                // Key head (circle)
                float headSize = keySize * 0.35f;
                g.FillEllipse(keyBrush, x, y, headSize, headSize);

                // Key head inner circle (hole)
                float holeSize = headSize * 0.4f;
                float holeX = x + (headSize - holeSize) / 2;
                float holeY = y + (headSize - holeSize) / 2;
                using (var holeBrush = new SolidBrush(Color.FromArgb(52, 152, 219)))
                {
                    g.FillEllipse(holeBrush, holeX, holeY, holeSize, holeSize);
                }

                // Key shaft
                float shaftWidth = keySize * 0.12f;
                float shaftHeight = keySize * 0.85f;
                float shaftX = x + headSize / 2 - shaftWidth / 2;
                float shaftY = y + headSize * 0.7f;
                g.FillRectangle(keyBrush, shaftX, shaftY, shaftWidth, shaftHeight);

                // Key teeth
                float toothWidth = shaftWidth * 1.5f;
                float toothHeight = shaftHeight * 0.15f;
                float toothY1 = shaftY + shaftHeight - toothHeight * 3;
                float toothY2 = shaftY + shaftHeight - toothHeight * 1.5f;

                g.FillRectangle(keyBrush, shaftX + shaftWidth, toothY1, toothWidth, toothHeight);
                g.FillRectangle(keyBrush, shaftX + shaftWidth, toothY2, toothWidth * 0.7f, toothHeight);
            }
        }

        private static void DrawDatabase(Graphics g, int size, float scale)
        {
            float dbSize = size * 0.25f;
            float x = size * 0.6f;
            float y = size * 0.55f;

            using (var dbBrush = new SolidBrush(Color.FromArgb(255, 255, 255)))
            using (var shadowBrush = new SolidBrush(Color.FromArgb(100, 0, 0, 0)))
            {
                // Shadow
                g.FillEllipse(shadowBrush, x + 2, y + dbSize + 2, dbSize * 0.9f, dbSize * 0.2f);

                // Database cylinder - top
                g.FillEllipse(dbBrush, x, y, dbSize, dbSize * 0.3f);

                // Database cylinder - body
                using (var bodyBrush = new LinearGradientBrush(
                    new RectangleF(x, y + dbSize * 0.15f, dbSize, dbSize * 0.7f),
                    Color.FromArgb(240, 240, 240),
                    Color.FromArgb(200, 200, 200),
                    LinearGradientMode.Horizontal))
                {
                    g.FillRectangle(bodyBrush, x, y + dbSize * 0.15f, dbSize, dbSize * 0.7f);
                }

                // Database cylinder - bottom
                g.FillEllipse(dbBrush, x, y + dbSize * 0.55f, dbSize, dbSize * 0.3f);

                // Draw horizontal lines on database
                using (var linePen = new Pen(Color.FromArgb(150, 150, 150), Math.Max(1, size / 128)))
                {
                    g.DrawLine(linePen, x, y + dbSize * 0.35f, x + dbSize, y + dbSize * 0.35f);
                    g.DrawLine(linePen, x, y + dbSize * 0.55f, x + dbSize, y + dbSize * 0.55f);
                }
            }
        }
    }
}
