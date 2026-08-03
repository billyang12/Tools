using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace YXBKeepassReader
{
    public class RecentFilesManager
    {
        private const int MaxRecentFiles = 10;
        private const string SettingsFileName = "recentfiles.txt";
        private static string SettingsFilePath => Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "YXBKeepassReader",
            SettingsFileName);

        public List<string> GetRecentFiles()
        {
            try
            {
                if (File.Exists(SettingsFilePath))
                {
                    return File.ReadAllLines(SettingsFilePath)
                        .Where(f => !string.IsNullOrWhiteSpace(f))
                        .Take(MaxRecentFiles)
                        .ToList();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error reading recent files: {ex.Message}");
            }
            return new List<string>();
        }

        public void AddRecentFile(string filePath)
        {
            try
            {
                var recentFiles = GetRecentFiles();

                // Remove if already exists
                recentFiles.RemoveAll(f => f.Equals(filePath, StringComparison.OrdinalIgnoreCase));

                // Add to top
                recentFiles.Insert(0, filePath);

                // Keep only MaxRecentFiles
                if (recentFiles.Count > MaxRecentFiles)
                {
                    recentFiles = recentFiles.Take(MaxRecentFiles).ToList();
                }

                // Save to file
                SaveRecentFiles(recentFiles);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error adding recent file: {ex.Message}");
            }
        }

        public void ClearRecentFiles()
        {
            try
            {
                SaveRecentFiles(new List<string>());
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error clearing recent files: {ex.Message}");
            }
        }

        private void SaveRecentFiles(List<string> files)
        {
            try
            {
                string directory = Path.GetDirectoryName(SettingsFilePath);
                if (!Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                File.WriteAllLines(SettingsFilePath, files);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error saving recent files: {ex.Message}");
            }
        }
    }
}
