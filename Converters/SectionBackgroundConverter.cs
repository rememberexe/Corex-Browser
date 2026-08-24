using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace CorexBrowser.Converters
{
    public class SectionBackgroundConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null || parameter == null)
                return Brushes.Transparent;

            return value.ToString() == parameter.ToString()
                ? (Brush)App.Current.Resources["AccentBrush"]
                : Brushes.Transparent;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }
}
