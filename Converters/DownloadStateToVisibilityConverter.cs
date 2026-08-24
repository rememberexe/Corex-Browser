using Microsoft.Web.WebView2.Core;
using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace CorexBrowser.Converters
{
    public class DownloadStateToVisibilityConverter : IValueConverter
    {
        public CoreWebView2DownloadState State { get; set; }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is CoreWebView2DownloadOperation op)
                return op.State == State ? Visibility.Visible : Visibility.Collapsed;

            return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotSupportedException();
    }
}
