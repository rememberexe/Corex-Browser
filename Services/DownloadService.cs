using CorexBrowser.Models;
using System.Collections.ObjectModel;
using System.Text.Json;
using System.IO;

namespace CorexBrowser.Services
{
    public static class DownloadService
    {
        public static ObservableCollection<DownloadItem> Items { get; }
            = new ObservableCollection<DownloadItem>();

        public static void Add(DownloadItem item)
        {
            Items.Insert(0, item); // en üste yeni indirme
        }
        private static readonly string FilePath =
    Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "downloads.json");

        public static void Save()
        {
            var json = JsonSerializer.Serialize(Items, new JsonSerializerOptions
            {
                WriteIndented = true
            });
            File.WriteAllText(FilePath, json);
        }

        public static void Load()
        {
            if (!File.Exists(FilePath))
                return;

            var json = File.ReadAllText(FilePath);
            var list = JsonSerializer.Deserialize<ObservableCollection<DownloadItem>>(json);

            if (list != null)
                Items.Clear();

            foreach (var item in list)
                Items.Add(item);
        }
    }
}
