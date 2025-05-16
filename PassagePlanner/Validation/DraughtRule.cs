using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Windows.Controls;

namespace PassagePlanner
{
    public class DraughtRule : ValidationRule
    {
        private char _systemDecimal = '#';

        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            _systemDecimal = GetSystemDecimal();

            string draughtString = value.ToString().Replace(',', _systemDecimal).Replace('.', _systemDecimal);

            double draught;

            try
            {
                draught = Double.Parse(draughtString);
            }
            catch (FormatException)
            {
                return new ValidationResult(false, "Enter a numeric value.");
            }

            if (draught < 0.0 || draught > 50.0)
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
