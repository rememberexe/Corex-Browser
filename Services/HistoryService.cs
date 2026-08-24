using CorexBrowser.Models;
using System.Collections.ObjectModel;
using System.IO;
using System.Text.Json;

namespace CorexBrowser.Services
{
    public static class HistoryService
    {
        private static readonly string FilePath =
            Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "history.json");

        public static ObservableCollection<HistoryItem> Items { get; private set; }
            = new ObservableCollection<HistoryItem>();

        public static void Load()
        {
            if (!File.Exists(FilePath))
                return;

            var json = File.ReadAllText(FilePath);
            var list = JsonSerializer.Deserialize<ObservableCollection<HistoryItem>>(json);

            if (list != null)
                Items = list;
        }

        public static void Save()
        {
            var json = JsonSerializer.Serialize(Items, new JsonSerializerOptions
            {
                WriteIndented = true
            });

            File.WriteAllText(FilePath, json);
        }

        public static void Add(string url, string title, string favicon)
        {
            Items.Add(new HistoryItem
            {
                Url = url,
                Title = title,
                Favicon = favicon,
                VisitedAt = DateTime.Now
            });

            Save();
        }

        public static void Clear()
        {
            Items.Clear();
            Save();
        }
    }
}
