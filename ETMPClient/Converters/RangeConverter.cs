using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows.Data;

namespace ETMPClient.Views
{
    /// <summary>
    /// Converts an integer to a collection of that many items for ItemsControl binding
    /// </summary>
    public class RangeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is int count && count > 0)
            {
                return Enumerable.Range(0, count).ToList();
            }
            return new List<int>();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
