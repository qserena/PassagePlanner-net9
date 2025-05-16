using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Windows.Controls;

namespace PassagePlanner
{
    public class WaterDensityRule : ValidationRule
    {
        private char _systemDecimal = '#';

        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            _systemDecimal = GetSystemDecimal();

            string waterDensityString = value.ToString().Replace(',', _systemDecimal).Replace('.', _systemDecimal);

            double waterDensity;

            try
            {
                waterDensity = Double.Parse(waterDensityString);
            }
            catch (FormatException)
            {
                return new ValidationResult(false, "Enter a numeric value.");
            }

            if (waterDensity < 900.0 || waterDensity > 1100.0)
            {
                return new ValidationResult(false, "Enter a value between 900 and 1100 (unit is kg/m3).");
            }


            return ValidationResult.ValidResult;
        }

        public static char GetSystemDecimal()
        {
            return string.Format("{0}", 1.1f)[1];
        }
    }

}
