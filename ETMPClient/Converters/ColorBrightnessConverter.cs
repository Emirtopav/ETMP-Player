using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace ETMPClient.Converters
{
    /// <summary>
    /// Converts a hex color and brightness value to a brightened SolidColorBrush
    /// </summary>
    public class ColorBrightnessConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values.Length != 2 || values[0] is not string hexColor || values[1] is not double brightness)
                return Brushes.White;

            try
            {
                // Parse hex color
                var color = (Color)ColorConverter.ConvertFromString(hexColor);
                
                // Apply brightness multiplier
                byte r = (byte)Math.Min(255, color.R * brightness);
                byte g = (byte)Math.Min(255, color.G * brightness);
                byte b = (byte)Math.Min(255, color.B * brightness);
                
                return new SolidColorBrush(Color.FromRgb(r, g, b));
            }
            catch
            {
                return Brushes.White;
            }
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
