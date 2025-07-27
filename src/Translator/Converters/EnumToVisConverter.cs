using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Translator.Converters
{
    internal class EnumToVisConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null || parameter == null || !(value is Enum))
                return Visibility.Collapsed;

            string valueString = value.ToString();
            string parameterString = parameter.ToString();

            if (string.Equals(valueString, parameterString))
                return Visibility.Visible;

            return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return Binding.DoNothing;
        }
    }
}
