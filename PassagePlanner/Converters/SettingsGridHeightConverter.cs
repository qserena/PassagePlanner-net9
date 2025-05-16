using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace PassagePlanner
{
    public class SettingsGridHeightConverter : IValueConverter
    {
        public SettingsGridHeightConverter()
        {
        }

        //  Convert from boolean value to double (GridHeight)
        object IValueConverter.Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return ((bool)value == true ? 260.0 : 0.0);
        }

        //  Convert from double (GridHeight) to boolean
        object IValueConverter.ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (value.GetType() == typeof(double) && (double)value > 100.0 ? true : false);
        }
    }
}
