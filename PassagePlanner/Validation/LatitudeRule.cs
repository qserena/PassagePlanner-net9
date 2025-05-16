using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Windows.Controls;

namespace PassagePlanner
{
    public class LatitudeRule : ValidationRule
    {
        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            var str = value as string;

            PositionValueConverter converter = new PositionValueConverter();
            object o = converter.ConvertBack(str, typeof(string), "Latitude", cultureInfo);
       
            if (o == null || o.GetType() == typeof(ArgumentException))
            {
                return new ValidationResult(false, "Enter Latitude. Examples:  N 14 05, S 07 55.5");
            }

            return new ValidationResult(true, null);
        }
    }

}
