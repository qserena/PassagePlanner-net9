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
    public class RequiredFieldColorConverter : IValueConverter
    {
        public RequiredFieldColorConverter()
        {
        }

        object IValueConverter.Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            //if (value == null)
            //{
            //    //return Brushes.Red;
            //    return Color.Red;
            //}
            //else
            //{
         
                if (Convert.ToDouble(value) == 0.0)
                {
                    //return Color.Red;
                    return Brushes.Red;
                }
                else
                {
                    //return Color.LightGray;
                    return Brushes.Blue;
                    //return Color.Blue;
                }
            //}
        }

        object IValueConverter.ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
       
    }
}
