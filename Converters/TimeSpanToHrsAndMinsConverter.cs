using System;
using System.Globalization;
using System.Windows.Data;

namespace JellyMusic.Converters
{
    class TimeSpanToHrsAndMinsConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            TimeSpan timeSpan = (TimeSpan)value;
            if (timeSpan.Hours > 0)
            {
                return timeSpan.Hours + " hours " + timeSpan.Minutes + " minutes";
            }
            else
            {
                return timeSpan.Minutes + " minutes " + timeSpan.Seconds + " seconds";
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
