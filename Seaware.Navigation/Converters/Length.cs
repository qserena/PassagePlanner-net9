using System;
using System.ComponentModel;

namespace Seaware.Navigation.Converters
{
    internal static class Length
    {
        public static double Degree2meter(double degree)
        {
            return degree * (60.0 * 1852.0);
        }
        public static double Meter2Degree(double meter)
        {
            return meter / (60.0 * 1852.0);
        }

        public static double Radian2Meter(double radian)
        {
            return radian * 60 * 1852 * 180 / Math.PI;
        }

    }

}
