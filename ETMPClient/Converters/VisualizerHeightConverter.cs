using System;
using System.Globalization;
using System.Windows.Data;

namespace ETMPClient.Converters
{
    public class VisualizerHeightConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values.Length >= 2 && values[0] is double value && values[1] is double maxHeight)
            {
                // Value is 0-100, map to 5-maxHeight
                var normalizedValue = Math.Max(5, (value / 100.0) * maxHeight);
                return normalizedValue;
            }
            return 5.0;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
