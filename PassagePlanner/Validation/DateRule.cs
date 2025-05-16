using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Windows.Controls;

namespace PassagePlanner
{
    public class DateRule : ValidationRule
    {
        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            DateTime? date;

            if (value != null && value.ToString().Trim().Length > 0)
            {
                try
                {
                    date = DateTime.Parse(value.ToString());
                }
                catch (FormatException)
                {
                    string formatExample = DateTime.Now.ToShortDateString();
                    return new ValidationResult(false, "Click on calendar icon, or enter date in format: " + formatExample);
                }

                if (date < new DateTime(2000, 01, 01))
                {
                    return new ValidationResult(false, "Year has to be later than 2000.");
                }

                if (date > new DateTime(2100, 01, 01))
                {
                    return new ValidationResult(false, "Year has to be earlier than 2100.");
                }
            }

            return ValidationResult.ValidResult;
        }
    }

}
