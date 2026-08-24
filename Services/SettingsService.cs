using CorexBrowser.Models;
using System.IO;
using System.Text.Json;

namespace CorexBrowser.Services
{
    public static class SettingsService
    {
        private static readonly string FilePath =
            Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "CorexBrowser",
                "settings.json");

        public static AppSettings Current { get; private set; } = new();

        public static void Load()
        {
            try
            {
                if (File.Exists(FilePath))
                {
                    var json = File.ReadAllText(FilePath);
                    Current = JsonSerializer.Deserialize<AppSettings>(json)!;
                }
            }
            catch
            {
                Current = new AppSettings();
            }
        }

        public static void Save()
        {
            Directory.CreateDirectory(Path.GetDirectoryName(FilePath)!);
            var json = JsonSerializer.Serialize(Current, new JsonSerializerOptions
            {
                WriteIndented = true
            });
            File.WriteAllText(FilePath, json);
        }
    }
}
