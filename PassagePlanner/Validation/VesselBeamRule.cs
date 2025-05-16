using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Windows.Controls;

namespace PassagePlanner
{
    public class VesselBeamRule : ValidationRule
    {
        private char _systemDecimal = '#';

        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            _systemDecimal = GetSystemDecimal();

            string vesselBeamString = value.ToString().Replace(',', _systemDecimal).Replace('.', _systemDecimal);

            double vesselBeam;

            try
            {
                vesselBeam = Double.Parse(vesselBeamString);
            }
            catch (FormatException)
            {
                return new ValidationResult(false, "Enter a numeric value.");
            }

            if (vesselBeam < 0.0 || vesselBeam > 150.0)
            {
                return new ValidationResult(false, "Enter a value between 0 and 150.");
            }

            return ValidationResult.ValidResult;
        }

        public static char GetSystemDecimal()
        {
            return string.Format("{0}", 1.1f)[1];
        }
    }

}
