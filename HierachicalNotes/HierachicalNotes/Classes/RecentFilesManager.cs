using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace HierachicalNotes.Classes
{
    public class RecentFilesManager
    {
        private const int MaxRecentFiles = 10;
        private readonly string _recentFilesPath;
        private List<string> _recentFiles;

        public RecentFilesManager()
        {
            string appDataPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "HierachicalNotes");
            if (!Directory.Exists(appDataPath))
            {
                Directory.CreateDirectory(appDataPath);
            }
            _recentFilesPath = Path.Combine(appDataPath, "recentFiles.json");
            _recentFiles = LoadRecentFiles();
        }

        private List<string> LoadRecentFiles()
        {
            if (File.Exists(_recentFilesPath))
            {
                try
                {
                    string json = File.ReadAllText(_recentFilesPath);
                    var files = JsonSerializer.Deserialize<List<string>>(json);
                    return files?.Where(f => File.Exists(f)).ToList() ?? new List<string>();
                }
                catch
                {
                    return new List<string>();
                }
            }
            return new List<string>();
        }

        private void SaveRecentFiles()
        {
            try
            {
                JsonSerializerOptions options = new JsonSerializerOptions { WriteIndented = true };
                string json = JsonSerializer.Serialize(_recentFiles, options);
                File.WriteAllText(_recentFilesPath, json);
            }
            catch
            {
            }
        }

        public void AddRecentFile(string filePath)
        {
            if (string.IsNullOrEmpty(filePath))
                return;

            _recentFiles.Remove(filePath);
            _recentFiles.Insert(0, filePath);

            if (_recentFiles.Count > MaxRecentFiles)
            {
                _recentFiles = _recentFiles.Take(MaxRecentFiles).ToList();
            }

            SaveRecentFiles();
        }

        public List<string> GetRecentFiles()
        {
            return _recentFiles.Where(f => File.Exists(f)).ToList();
        }

        public void ClearRecentFiles()
        {
            _recentFiles.Clear();
            SaveRecentFiles();
        }
    }
}
