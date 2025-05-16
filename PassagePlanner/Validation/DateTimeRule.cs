using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Windows.Controls;

namespace PassagePlanner
{
    public class DateTimeRule : ValidationRule
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
                    string formatExample = DateTime.Now.ToShortDateString() + " " + DateTime.Now.ToShortTimeString();
                    return new ValidationResult(false, "Enter date and time in format: " + formatExample);
                }
            }

            return ValidationResult.ValidResult;

        }
    }

}
