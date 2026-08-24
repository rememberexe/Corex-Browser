using Microsoft.Web.WebView2.Core;

namespace CorexBrowser.Models
{
    public class DownloadItem
    {
        public string FileName { get; set; }
        public string Url { get; set; }
        public string FilePath => Operation?.ResultFilePath;
        public string State => Operation?.State.ToString();

        public long TotalBytes { get; set; }
        public long ReceivedBytes { get; set; }

        public CoreWebView2DownloadOperation Operation { get; set; }

        public double Progress =>
            TotalBytes == 0 ? 0 : (double)ReceivedBytes / TotalBytes;

        public bool IsCompleted => Operation.State == CoreWebView2DownloadState.Completed;
    }
}
