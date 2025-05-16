using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace PassagePlanner
{
    /// <summary>
    /// This Converter is just a bug fix. Eliminates red border around DataGrid when click in grid if NoOfWaypoints is zero.
    /// </summary>
    public class WaypointsIsItemSourceConverter : IValueConverter
    {
        public WaypointsIsItemSourceConverter()
        {
        }

        object IValueConverter.Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value;
        }

        object IValueConverter.ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is Waypoint)
                return value;
            return null;

        }
    }
}
