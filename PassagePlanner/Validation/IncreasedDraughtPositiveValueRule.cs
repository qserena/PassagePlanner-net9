using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Windows.Controls;

namespace PassagePlanner
{
    public class IncreasedDraughtPositiveValueRule : ValidationRule
    {
        private char _systemDecimal = '#';

        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            _systemDecimal = GetSystemDecimal();

            string increasedDraughtString = value.ToString().Replace(',', _systemDecimal).Replace('.', _systemDecimal);

            double increasedDraught;

            try
            {
                increasedDraught = Double.Parse(increasedDraughtString);
            }
            catch (FormatException)
            {
                return new ValidationResult(false, "Enter a numeric value.");
            }

            if (increasedDraught < 0.0 || increasedDraught > 10.0)
            {
                return new ValidationResult(false, "Enter a value between 0 and 10.");
            }


            return ValidationResult.ValidResult;
        }

        public static char GetSystemDecimal()
        {
            return string.Format("{0}", 1.1f)[1];
        }
    }

}
