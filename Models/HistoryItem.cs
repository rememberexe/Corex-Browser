namespace CorexBrowser.Models
{
    public class HistoryItem
    {
        public string Url { get; set; }
        public string Title { get; set; }
        public string Favicon { get; set; }
        public DateTime VisitedAt { get; set; }
    }
}
