using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows.Data;
using VTEControls.WPF;

namespace Translator.Converters
{
    internal class IndicatorConverter : IValueConverter
    {
        private Type m_enumType = typeof(ServerStatus);
        private IDictionary<IndicatorStatus, ServerStatus> m_indicatorLookup = new Dictionary<IndicatorStatus, ServerStatus>();

        public IndicatorConverter()
        {
            m_indicatorLookup[IndicatorStatus.Off] = ServerStatus.DISCONNECTED;
            m_indicatorLookup[IndicatorStatus.On] = ServerStatus.CONNECTED;
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null || !(value.GetType() == m_enumType))
                return m_indicatorLookup.First().Key;

            return m_indicatorLookup.FirstOrDefault(kvp => Equals(kvp.Value, value)).Key;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return Binding.DoNothing;
        }
    }
}
