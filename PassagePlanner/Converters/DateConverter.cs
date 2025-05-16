using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace PassagePlanner
{
    /// <summary>
    /// The only reason for implementing this converter is to display something when DateTime value is NOT set, 
    /// for example "Select a date: ".
    /// </summary>
    public class DateConverter : IValueConverter
    {
        private const string _initialString = "Select a date: ";

        public DateConverter():base()
        {
        }

        //  Convert from DateTime value to string
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            //return (value != null && (DateTime)value != DateTime.MinValue ? ((DateTime)value).ToString() : _initialString);

            object returnValue;
            if (value != null)
            {
                returnValue = ((DateTime)value).ToShortDateString();
            }
            else
            {
                //returnValue = _initialString;
                returnValue = string.Empty;
            }
            return returnValue;

        }

        //  Convert from string to DateTime value
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {

            //return (value.GetType() == typeof(string) && value.ToString() != _initialString && value.ToString() != string.Empty ? DateTime.Parse(value.ToString()) : DateTime.MinValue);
            //return (value.GetType() == typeof(string) && value.ToString() != _initialString && value.ToString() != string.Empty ? (DateTime?)DateTime.Parse(value.ToString()) : null);

            object returnValue;
            if (value.GetType() == typeof(string) && value.ToString() != _initialString && value.ToString() != string.Empty)
            {
                try
                {
                    returnValue = (DateTime?)DateTime.Parse(value.ToString());
                }
                catch (FormatException formatEx)
                {
                    return new FormatException(value.ToString() + " could not be converted to a date.");
                }
            }
            else
            {
                returnValue = null;
            }
            return returnValue;
        }
    }
}
