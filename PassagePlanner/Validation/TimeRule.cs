using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Windows.Controls;

namespace PassagePlanner
{
    public class TimeRule : ValidationRule
    {
        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            DateTime? datetime;

            if (value != null)
            {
                try
                {
                    datetime = DateTime.Parse(value.ToString());
                }
                catch (FormatException)
                {
                    string formatExample = DateTime.Now.ToShortTimeString();
                    return new ValidationResult(false, "Enter time in format: " + formatExample);
                }
            }

            return ValidationResult.ValidResult;

        }
    }

}
