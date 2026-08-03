using System;
using System.IO;

namespace YXBKeepassReader
{
    public class GenerateIconHelper
    {
        public static void Generate()
        {
            try
            {
                string projectDir = Path.GetDirectoryName(
                    Path.GetDirectoryName(
                    Path.GetDirectoryName(
                    Path.GetDirectoryName(AppDomain.CurrentDomain.BaseDirectory))));

                string outputPath = Path.Combine(projectDir, "app.ico");

                Console.WriteLine("Generating application icon...");
                IconGenerator.GenerateApplicationIcon(outputPath);
                Console.WriteLine($"Icon generated successfully: {outputPath}");

                string pngPath = Path.Combine(projectDir, "app_icon.png");
                Console.WriteLine($"PNG version also saved: {pngPath}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error generating icon: {ex.Message}");
                Console.WriteLine(ex.StackTrace);
            }
        }
    }
}
