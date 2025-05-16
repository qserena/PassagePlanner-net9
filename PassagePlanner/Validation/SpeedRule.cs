using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Windows.Controls;

namespace PassagePlanner
{
    public class SpeedRule : ValidationRule
    {
        private char _systemDecimal = '#';

        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            _systemDecimal = GetSystemDecimal();

            string speedString = value.ToString().Replace(',', _systemDecimal).Replace('.', _systemDecimal);

            double speed;

            try
            {
                speed = Double.Parse(speedString);
            }
            catch (FormatException)
            {
                return new ValidationResult(false, "Enter a numeric value.");
            }

            if (speed < 0.0 || speed > 50.0)
            {
                return new ValidationResult(false, "Enter a value between 0 and 50.");
            }

            return ValidationResult.ValidResult;
        }

        public static char GetSystemDecimal()
        {
            return string.Format("{0}", 1.1f)[1];
        }
    }

}
