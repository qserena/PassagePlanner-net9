using System;
using System.Windows.Data;
//using Microsoft.Maps.MapControl;
using System.Text;
using System.Text.RegularExpressions;
using System.Globalization;
using System.Collections.Generic;
using System.Linq;
using System.Collections;
using Seaware.Navigation;

namespace PassagePlanner
{
    public class PositionValueConverter : IValueConverter
    {
        public PositionValueConverter():base()
        { 
        }

        #region IValueConverter
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            try
            {
                if (value == null)
                    return null;
                else if (value.GetType() == targetType)
                {
                    return value;
                }
                IPosition ipos = value as IPosition;
                if (ipos != null)
                {
                    return LocationToString(ipos);
                }
                else if (targetType == typeof(object))
                    return value;
                else if (value.GetType() == typeof(double))
                {
                    if (parameter != null)
                    {
                        if (parameter.ToString().Equals("Latitude", StringComparison.CurrentCultureIgnoreCase))
                        {
                            return LatitudeDoubleToString((double)value);
                        }
                        else if (parameter.ToString().Equals("Longitude", StringComparison.CurrentCultureIgnoreCase))
                        {
                            return LongitudeDoubleToString((double)value);
                        }
                        else
                        {
                            throw new ArgumentException("Parameter must be \"Latitude\" or \"Longitude\"", "parameter");
                        }
                    }
                    else
                        throw new ArgumentNullException("parameter", "Parameter must have a value if object is of type double");
                }

                var positions = value as IEnumerable;
                if (positions != null)
                {
                    var test = positions.OfType<IPosition>();
                    return ConvertFromIEnumerablePosition(positions.OfType<IPosition>(), targetType);
                }
                if (value != null && value.GetType() == targetType)
                    return value;
                throw new NotImplementedException();
             }
             catch (Exception ex)
             {
                 return null;
             }

        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            try
            {
                if (value.GetType() == typeof(string))
                {
                    string stringValue = value.ToString();
                    if (new Regex(locationRegExp).IsMatch(stringValue))
                        return LocationStringToLocation(stringValue);
                    else if (new Regex(latitudeRegExp).IsMatch(stringValue))
                    {
                        return LatitudeStringToDouble(stringValue);
                    }
                    else if (new Regex(longitudeRegExp).IsMatch(stringValue))
                    {
                        return LongitudeStringToDouble(stringValue);
                    }
                    else
                        return new ArgumentException("Could not identify a location, longitude or latitude in the string", "value");
                }
                else if (value.GetType() == targetType)
                {
                    return value;
                }
                else if (value != null && value.GetType() == targetType)
                    return value;
                else
                    throw new NotImplementedException();
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        #endregion

        private object ConvertFromIEnumerablePosition(IEnumerable<IPosition> value, Type targetType)
        {
            if (targetType == typeof(IEnumerable<IPosition>))
            {
                return from p in value
                       select new Position(p.Latitude, p.Longitude);
            }
            else if (targetType == typeof(List<IPosition>))
            {
                return (from p in value
                        select new Position(p.Latitude, p.Longitude)).ToList();
            }
            throw new NotImplementedException();
        }

        #region Public static converters

        public static string LatitudeDoubleToString(double latitude)
        {
            double latdeg, latmin;
            ToDegMin(latitude, out latdeg, out latmin);

            StringBuilder latitude_string = new StringBuilder();

            if (latitude >= 0)
                latitude_string.Append("N");
            else if (latitude < 0)
                latitude_string.Append("S");

            latitude_string.Append(" ");
            latitude_string.Append(latdeg.ToString("00"));
            latitude_string.Append("°");
            latitude_string.Append(" ");
            latitude_string.Append(latmin.ToString("00.000"));
            latitude_string.Append("'");

            return latitude_string.ToString();
        }

        public static string LongitudeDoubleToString(double longitude)
        {
            double londeg, lonmin;
            ToDegMin(longitude, out londeg, out lonmin);

            StringBuilder longitude_string = new StringBuilder();

            if (longitude >= 0)
                longitude_string.Append("E");
            else if (longitude < 0)
                longitude_string.Append("W");

            longitude_string.Append(" ");
            longitude_string.Append(londeg.ToString("000"));
            longitude_string.Append("°");
            longitude_string.Append(" ");
            longitude_string.Append(lonmin.ToString("00.000"));
            longitude_string.Append("'");

            return longitude_string.ToString();
        }

        public static string LocationToString(IPosition location)
        {
            return LatitudeDoubleToString(location.Latitude) + "/" + LongitudeDoubleToString(location.Longitude);
        }

        public static double LatitudeStringToDouble(string latitude)
        {
            double latDouble = 0;
            latitude = latitude.Trim();

            if (!Double.TryParse(latitude, out latDouble))
            {
                var latRegExp = new Regex(latitudeRegExp);
                var match = latRegExp.Match(latitude);

                if (match.Success) //Start with NS
                {
                    latitude = latitude.Substring(match.Index, match.Length);
                    double sign = 1;
                    if (new Regex("^[Ss]").IsMatch(latitude))
                        sign = -1;

                    // Remove letter and optional white space before degrees
                    string[] splittedString1 = Regex.Split(latitude, @"[NnSs]\s?");
                    string degreesAndMinutesString = splittedString1[1];

                    // Separate degrees and minutes (separated by °, white space, or both)
                    string[] splittedString2 = Regex.Split(degreesAndMinutesString, @"[°\s]{1,2}");
                    string degreesString = splittedString2[0];

                    // Extract degrees
                    latDouble = (double)System.Convert.ToInt16(degreesString);

                     // Check if there are any minutes
                    if (splittedString2.Length > 1 && splittedString2[1].Length > 0)
                    {
                        string minutesStringWithOptionalMinutesCharacter = splittedString2[1];

                        // Remove optional minutes character (')
                        string[] splittedString3 = Regex.Split(minutesStringWithOptionalMinutesCharacter, @"'");
                        string minutesString = splittedString3[0];

                        // Find optional decimal separator
                        Regex decimalSeparatorRegex = new Regex(@"[.,]");
                        Match decimalSeparatorMatch = decimalSeparatorRegex.Match(minutesString);
                        if (decimalSeparatorMatch.Success)
                        {
                            NumberFormatInfo formatProvider = new NumberFormatInfo();
                            formatProvider.NumberDecimalSeparator = decimalSeparatorMatch.Groups[0].Value;

                            // Add the minutes part
                            latDouble += System.Convert.ToDouble(minutesString, formatProvider) / 60;
                        }
                        else
                        {
                            // No decimal separator found. Add the minutes part without formatProvider
                            latDouble += System.Convert.ToDouble(minutesString) / 60;
                        }
                    }

                    latDouble *= sign;
                }
                else
                {
                    throw new ArgumentException("Could not find latitude in string", "latitude");
                }
            }

            if (Math.Abs(latDouble) > 90.0)
            {
                //latDouble = 90.0;
                throw new ArgumentOutOfRangeException("Latitude must not be greater than 90.0");
            }

            return latDouble;
        }

        public static double LongitudeStringToDouble(string longitude)
        {
            double lonDouble = 0;
            longitude = longitude.Trim();

            if (!Double.TryParse(longitude, out lonDouble))
            {
                var lonRegExp = new Regex(longitudeRegExp);
                var match = lonRegExp.Match(longitude);

                if (match.Success) //Start with EW
                {
                    longitude = longitude.Substring(match.Index, match.Length);
                    double sign = 1;
                    if (new Regex("^[Ww]").IsMatch(longitude))
                        sign = -1;

                    // Remove letter and optional white space before degrees
                    string[] splittedString1 = Regex.Split(longitude, @"[EeWw]\s?");
                    string degreesAndMinutesString = splittedString1[1];

                    // Separate degrees and minutes (separated by °, white space, or both)
                    string[] splittedString2 = Regex.Split(degreesAndMinutesString, @"[°\s]{1,2}");
                    string degreesString = splittedString2[0];

                    // Extract degrees
                    lonDouble = (double)System.Convert.ToInt16(degreesString);

                    // Check if there are any minutes
                    if (splittedString2.Length > 1 && splittedString2[1].Length > 0)
                    {
                        string minutesStringWithOptionalMinutesCharacter = splittedString2[1];

                        // Remove optional minutes character (')
                        string[] splittedString3 = Regex.Split(minutesStringWithOptionalMinutesCharacter, @"'");
                        string minutesString = splittedString3[0];

                        // Find optional decimal separator
                        Regex decimalSeparatorRegex = new Regex(@"[.,]");
                        Match decimalSeparatorMatch = decimalSeparatorRegex.Match(minutesString);
                        if (decimalSeparatorMatch.Success)
                        {
                            NumberFormatInfo formatProvider = new NumberFormatInfo();
                            formatProvider.NumberDecimalSeparator = decimalSeparatorMatch.Groups[0].Value;

                            // Add the minutes part
                            lonDouble += System.Convert.ToDouble(minutesString, formatProvider) / 60;
                        }
                        else
                        {
                            // No decimal separator found. Add the minutes part without formatProvider
                            lonDouble += System.Convert.ToDouble(minutesString) / 60;
                        }
                    }

                    lonDouble *= sign;
                }
                else
                {
                    throw new ArgumentException("Could not find longitude in string", "longitude");
                }
            }

            if (Math.Abs(lonDouble) > 180.0)
            {
                //lonDouble = 180.0;
                throw new ArgumentOutOfRangeException("Longitude must not be greater than 180.0");
            }

            return lonDouble;
        }

        public static IPosition LocationStringToLocation(string location)
        {
            var locationObject = new Seaware.Navigation.Position();
            
            if (new Regex(locationRegExp).IsMatch(location))
            {
                locationObject.Longitude = LongitudeStringToDouble(location);
                locationObject.Latitude = LatitudeStringToDouble(location);
            }
            else
                throw new ArgumentException("Could not find location string");
            return locationObject;
        }

        #endregion

        private static void ToDegMin(double degIn, out double deg, out double min)
        {
            degIn = Math.Abs(degIn);
            deg = Math.Floor(degIn);
            min = (degIn - deg) * 60;
        }

        const string latitudeRegExp = @"[NnSs]\s?\d?\d\D?\s?\d?\d([.,]\d+)?\D?";
        const string longitudeRegExp = @"[EeWw]\s?\d?\d?\d\D?\s?\d?\d([.,]\d+)?\D?";
        const string locationRegExp = latitudeRegExp + @"\D+" + longitudeRegExp;
    }
}
