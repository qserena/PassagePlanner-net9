using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Windows.Controls;

namespace PassagePlanner
{
    public class BlockCoefficientRule : ValidationRule
    {
        private char _systemDecimal = '#';

        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            _systemDecimal = GetSystemDecimal();

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
