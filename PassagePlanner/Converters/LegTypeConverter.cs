using Seaware.Navigation.Enumerations;
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
    /// Conversion between PassagePlanner.LegTypeType for view (presentation layer)
    /// and Seaware.Navigation.Enumerations.LegType.
    /// This is needed beacause the latter contains 
    /// value Transit, which we do not use in application PassagePlanner. 
    /// </summary>
    public class LegTypeConverter : IValueConverter
    {
        public LegTypeConverter() : base()
        {
        }

        //  Convert from LegType to LegTypeType
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            object returnValue = null;

            if (value != null && value.GetType() == typeof(LegType))
            {
                if ((LegType)value == Seaware.Navigation.Enumerations.LegType.RhumbLine)
                {
                    returnValue = LegTypeType.RhumbLine;
                }
                else if ((LegType)value == Seaware.Navigation.Enumerations.LegType.GreatCircle)
                {
                    returnValue = LegTypeType.GreatCircle;
                }
                else
                {
                    throw new Exception("Leg type " + ((LegType)value).ToString() + " is not supported");
                }
            }

            return returnValue;

        }

        //  Convert from LegTypeType to LegType
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            object returnValue = null;

            if (value != null && value.GetType() == typeof(LegTypeType))
            {
                if ((LegTypeType)value == LegTypeType.RhumbLine)
                {
                    returnValue = LegType.RhumbLine;
                }
                else if ((LegTypeType)value == LegTypeType.GreatCircle)
                {
                    returnValue = LegType.GreatCircle;
                }
            }

            return returnValue;
        }
    }
}
