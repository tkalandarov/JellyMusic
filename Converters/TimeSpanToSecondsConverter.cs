using System;
using System.Globalization;
using System.Windows.Data;

namespace JellyMusic.Converters
{
    class TimeSpanToSecondsConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return ((TimeSpan)value).TotalSeconds;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value == null ? TimeSpan.Zero : TimeSpan.FromSeconds((double)value);
        }
    }
}
