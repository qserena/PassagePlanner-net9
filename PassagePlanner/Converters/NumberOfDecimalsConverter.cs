using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace PassagePlanner
{
    public class NumberOfDecimalsConverter : IValueConverter
    {
        private char _systemDecimal = '#';

        public NumberOfDecimalsConverter()
        {
            _systemDecimal = GetSystemDecimal();
        }

        //  Convert from decimal value to string
        object IValueConverter.Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
            {
                return null;
            }
            else if (value.GetType() == targetType)
            {
                return value;
            }
            if (targetType == typeof(object))
            {
                return value;
            }
            else if (value.GetType() == typeof(double))
            {
                if (parameter != null)
                {
                    if (parameter.GetType() == typeof(String) &&  Convert.ToInt32(parameter) >= 0)
                    {
                        int numberOfDecimals = Convert.ToInt32(parameter);
                        string formatString = "{0:F" + numberOfDecimals + "}";
                        return string.Format(formatString, value).Replace('.', _systemDecimal).Replace(',', _systemDecimal);
                    }
                    else
                    {
                        throw new ArgumentException("Parameter must be an integer >= 0", "parameter");
                    }
                }
                else
                {
                    throw new ArgumentNullException("parameter", "Parameter must have a value");
                }
            }

            object returnValue = (value != null ? value.ToString().Replace('.', _systemDecimal).Replace(',', _systemDecimal) : string.Empty);
            return returnValue;
        }

        //  Convert from string to decimal value
        object IValueConverter.ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (value.GetType() == typeof(string) && value.ToString() != string.Empty ? value.ToString().Replace(',', '.').Replace(_systemDecimal, '.') : null);
        }

        public static char GetSystemDecimal()
        {
            return string.Format("{0}", 1.1f)[1];
        }
    }
}
