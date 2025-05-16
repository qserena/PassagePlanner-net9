using Seaware.Navigation;
using System;
using System.Net;

namespace Seaware.Navigation.Converters
{
    /// <summary>
    /// Converts positions between radians and degrees
    /// 
    /// Breaks namin-convention to avoid name-collisions with other Position classes
    /// </summary>
    internal static class PositionConverter
    {
        public static IPosition Radians2Degrees(IPosition positionRadians)
        {
            return new Position(
                positionRadians.Latitude * 180.0 / Math.PI,
                positionRadians.Longitude * 180.0 / Math.PI);
        }
        public static IPosition Degrees2Radians(IPosition positionDegrees)
        {
            return new Position(
                positionDegrees.Latitude / 180.0 * Math.PI,
                positionDegrees.Longitude / 180.0 * Math.PI);
        }
    }
}
