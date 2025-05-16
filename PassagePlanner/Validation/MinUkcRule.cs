using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Windows.Controls;

namespace PassagePlanner
{
    public class MinUkcRule : ValidationRule
    {
        private char _systemDecimal = '#';

        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            _systemDecimal = GetSystemDecimal();

            string minUkcString = value.ToString().Replace(',', _systemDecimal).Replace('.', _systemDecimal);

            double minUkc;

            try
            {
                minUkc = Double.Parse(minUkcString);
            }
            catch (FormatException)
            {
                return new ValidationResult(false, "Enter a numeric value.");
            }

            if (minUkc < 0.0 || minUkc > 100.0)
            {
                return new ValidationResult(false, "Enter a value between 0 and 100.");
            }

            return ValidationResult.ValidResult;
        }

        public static char GetSystemDecimal()
        {
            return string.Format("{0}", 1.1f)[1];
        }
    }

}
