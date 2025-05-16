using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Windows.Controls;

namespace PassagePlanner
{
    public class LongitudeRule : ValidationRule
    {
        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            var str = value as string;

            PositionValueConverter converter = new PositionValueConverter();
            object o = converter.ConvertBack(str, typeof(string), "Longitude", cultureInfo);
       
            if (o == null || o.GetType() == typeof(ArgumentException))
            {
                return new ValidationResult(false, "Enter Longitude. Examples:  E 120 15, W 002 55.23");
            }

            return new ValidationResult(true, null);
        }
    }

}
