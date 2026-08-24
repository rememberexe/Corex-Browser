namespace CorexBrowser.Models
{
    public class AppSettings
    {
        // SADECE yeni sekme / açılış için
        public string HomePage { get; set; } = "https://www.google.com";

        // SADECE arama motoru anahtarı
        public string SearchEngine { get; set; } = "google";
    }
}
