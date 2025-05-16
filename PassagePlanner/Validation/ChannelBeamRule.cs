using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Windows.Controls;

namespace PassagePlanner
{
    public class ChannelBeamRule : ValidationRule
    {
        private char _systemDecimal = '#';

        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            _systemDecimal = GetSystemDecimal();

            string channelBeamString = value.ToString().Replace(',', _systemDecimal).Replace('.', _systemDecimal);

            double channelBeam;

            try
            {
                channelBeam = Double.Parse(channelBeamString);
            }
            catch (FormatException)
            {
                return new ValidationResult(false, "Enter a numeric value.");
            }

            if (channelBeam < 0.0)
            {
                return new ValidationResult(false, "Enter a positive value.");
            }

            ViewModelLocator locator = new ViewModelLocator();
            VezzelViewModel vesselVM = locator.VesselVM;
            double vesselBeam = vesselVM.VesselBeam;

            if (channelBeam > 0 && channelBeam <= vesselBeam)
            {
                return new ValidationResult(false, "Channel beam must be greater than Vessel beam.");
            }

            return ValidationResult.ValidResult;
        }

        public static char GetSystemDecimal()
        {
            return string.Format("{0}", 1.1f)[1];
        }
    }

}
