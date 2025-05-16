using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Windows.Controls;

namespace PassagePlanner
{
    public class EffectOfWavesRule : ValidationRule
    {
        private char _systemDecimal = '#';

        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            _systemDecimal = GetSystemDecimal();

            string effectOfWavesString = value.ToString().Replace(',', _systemDecimal).Replace('.', _systemDecimal);

            double effectOfWaves;

            try
            {
                effectOfWaves = Double.Parse(effectOfWavesString);
            }
            catch (FormatException)
            {
                return new ValidationResult(false, "Enter a numeric value.");
            }

            if (effectOfWaves < 0.0 || effectOfWaves > 30.0)
            {
                return new ValidationResult(false, "Enter a value between 0 and 30.");
            }

            return ValidationResult.ValidResult;
        }

        public static char GetSystemDecimal()
        {
            return string.Format("{0}", 1.1f)[1];
        }
    }

}
