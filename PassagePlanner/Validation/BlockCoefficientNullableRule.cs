using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Windows.Controls;

namespace PassagePlanner
{
    public class BlockCoefficientNullableRule : ValidationRule
    {
        private char _systemDecimal = '#';

        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            _systemDecimal = GetSystemDecimal();

            // In the Vessel page empty strings (corresponds to block coefficient null) are allowed
            if (value.ToString() == string.Empty)
            {
                return ValidationResult.ValidResult;
            }

            string blockCoefficientString = value.ToString().Replace(',', _systemDecimal).Replace('.', _systemDecimal);

            double blockCoefficient;

            try
            {
                blockCoefficient = Double.Parse(blockCoefficientString);
            }
            catch (FormatException)
            {
                return new ValidationResult(false, "Enter a numeric value.");
            }

            if (blockCoefficient < 0.0 || blockCoefficient > 1.0)
            {
                return new ValidationResult(false, "Enter a value between 0 and 1.");
            }


            return ValidationResult.ValidResult;
        }

        public static char GetSystemDecimal()
        {
            return string.Format("{0}", 1.1f)[1];
        }
    }

}
