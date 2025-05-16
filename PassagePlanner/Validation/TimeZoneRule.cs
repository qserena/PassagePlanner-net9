using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Windows.Controls;

namespace PassagePlanner
{
    public class TimeZoneRule : ValidationRule
    {
        private char _systemDecimal = '#';

        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            double timeZone;

            _systemDecimal = GetSystemDecimal();

            string timeZoneString = value.ToString().Replace(',', _systemDecimal).Replace('.', _systemDecimal);

            try
            {
                timeZone = Double.Parse(timeZoneString);
            }
            catch (FormatException)
            {
                return new ValidationResult(false, "Enter a numeric value.");
            }

            if (timeZone < -12.0 || timeZone > 12.0)
            {
                return new ValidationResult(false, "Enter a value between -12 and 12.");
            }

            if (timeZone - Math.Truncate(timeZone) == 0.0 || timeZone - Math.Truncate(timeZone) == 0.25 || timeZone - Math.Truncate(timeZone) == 0.5 || timeZone - Math.Truncate(timeZone) == 0.75)
            {
                return ValidationResult.ValidResult;
            }
            else
            {
                return new ValidationResult(false, "Value should be a positive or negative value in quarter of hours. Examples: 8, -11.25, 6.5 etc.");
            }
        }

        public static char GetSystemDecimal()
        {
            return string.Format("{0}", 1.1f)[1];
        }
    }

}
