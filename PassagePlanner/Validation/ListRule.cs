using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Windows.Controls;

namespace PassagePlanner
{
    public class ListRule : ValidationRule
    {
        private char _systemDecimal = '#';

        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            _systemDecimal = GetSystemDecimal();

            string listString = value.ToString().Replace(',', _systemDecimal).Replace('.', _systemDecimal);

            double list;

            try
            {
                list = Double.Parse(listString);
            }
            catch (FormatException)
            {
                return new ValidationResult(false, "Enter a numeric value.");
            }

            if (list < 0.0)
            {
                return new ValidationResult(false, "Enter a positive value.");
            }

            if (list >= 45.0)
            {
                return new ValidationResult(false, "List should be less than 45 degrees.");
            }

            return ValidationResult.ValidResult;
        }

        public static char GetSystemDecimal()
        {
            return string.Format("{0}", 1.1f)[1];
        }
    }

}
