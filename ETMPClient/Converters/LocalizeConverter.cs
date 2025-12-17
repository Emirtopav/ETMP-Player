using System;
using System.ComponentModel;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Markup;

namespace ETMPClient.Converters
{
    public class LocalizeConverter : MarkupExtension, IValueConverter
    {
        private static LocalizeConverter? _instance;

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (parameter is string key)
            {
                return Services.LocalizationService.Instance.GetString(key);
            }
            return parameter?.ToString() ?? string.Empty;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            return _instance ??= new LocalizeConverter();
        }
    }
}
