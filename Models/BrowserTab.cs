using Microsoft.Web.WebView2.Wpf;
using System.ComponentModel;
using System.Windows.Media;

namespace CorexBrowser.Models
{
    public class BrowserTab : INotifyPropertyChanged
    {
        public WebView2 WebView { get; set; } = null!;

        private string _title = "Yeni Sekme";
        public string Title
        {
            get => _title;
            set
            {
                _title = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Title)));
            }
        }

        private ImageSource? _favicon;
        public ImageSource? Favicon
        {
            get => _favicon;
            set
            {
                _favicon = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Favicon)));
            }
        }

        private bool _isActive;
        public bool IsActive
        {
            get => _isActive;
            set
            {
                _isActive = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsActive)));
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
    }
}
