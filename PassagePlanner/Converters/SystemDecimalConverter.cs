using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace PassagePlanner
{
    public class SystemDecimalConverter : IValueConverter
    {
        private char _systemDecimal = '#';

        public SystemDecimalConverter()
        {
            _systemDecimal = GetSystemDecimal();
        }

        //  Convert from decimal value to string
        object IValueConverter.Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            object returnValue = (value != null ? value.ToString().Replace('.', _systemDecimal).Replace(',', _systemDecimal) : string.Empty);
            return returnValue;
            //return (value != null ? string.Format("{0:F1}", value).Replace('.', _systemDecimal) : string.Empty);
        }

        //  Convert from string to decimal value
        object IValueConverter.ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            object returnValue = (value.GetType() == typeof(string) && value.ToString() != string.Empty ? value.ToString().Replace(',', '.').Replace(_systemDecimal, '.') : null);
            return returnValue;
        }

        public static char GetSystemDecimal()
        {
            return string.Format("{0}", 1.1f)[1];
        }
    }
}
