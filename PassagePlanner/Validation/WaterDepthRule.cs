using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Windows.Controls;

namespace PassagePlanner
{
    public class WaterDepthRule : ValidationRule
    {
        private char _systemDecimal = '#';

        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            _systemDecimal = GetSystemDecimal();

            string waterDepthString = value.ToString().Replace(',', _systemDecimal).Replace('.', _systemDecimal);

            double waterDepth;

            try
            {
                waterDepth = Double.Parse(waterDepthString);
            }
            catch (FormatException)
            {
                return new ValidationResult(false, "Enter a numeric value.");
            }

            if (waterDepth < 0.0)
            {
                return new ValidationResult(false, "Enter a positive value.");
            }

            if (waterDepth > 11000.0)
            {
                return new ValidationResult(false, "Enter a value less than 11000.");
            }

            return ValidationResult.ValidResult;
        }

        public static char GetSystemDecimal()
        {
            return string.Format("{0}", 1.1f)[1];
        }
    }

}
