using Seaware.Navigation.Converters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Seaware.Navigation
{
    public class GreatCircle
    {

        /// <summary>
        /// Calculates the GC distance between two points
        /// </summary>
        /// <param name="position1">Position 1 [°]</param>
        /// <param name="position2">Position 2 [°]</param>
        /// <returns>The GC distance [m]</returns>
        public static double DistanceBetweenPositions(IPosition position1, IPosition position2)
        {
            double temp = 0;
            return DistanceBetweenPositions(position1, position2, out temp);
        }
        

        /// <summary>
        /// Calculates the GC distance between two points
        /// </summary>
        /// <param name="position1">Position 1 [°]</param>
        /// <param name="position2">Position 2 [°]</param>
        /// <param name="heading">The heading at start of the leg</param>
        /// <returns>The GC distance [m]</returns>
        public static double DistanceBetweenPositions(IPosition position1, IPosition position2, out double heading)
        {
            Position pos1 = new Position(position1.Latitude / 180.0 * Math.PI, position1.Longitude / 180.0 * Math.PI);
            Position pos2 = new Position(position2.Latitude / 180.0 * Math.PI, position2.Longitude / 180.0 * Math.PI);
            
            return DistanceBetweenPositionsRad(pos1, pos2, out heading) * 180 / Math.PI * 60 * 1852;
        }

        /// <summary>
        /// Calculates the GC distance between two points
        /// </summary>
        /// <param name="positionRad1">Position 1 [rad]</param>
        /// <param name="positionRad2">Position 2 [rad]</param>
        /// <returns>The calculated distance [rad]</returns>
        private static double DistanceBetweenPositionsRad(IPosition positionRad1, IPosition positionRad2)
        {
            double phi_s = positionRad1.Latitude;
            double phi_f = positionRad2.Latitude;
            double lambda_s = positionRad1.Longitude;
            double lambda_f = positionRad2.Longitude;
            double df = phi_f - phi_s;
            double dl = lambda_f - lambda_s;
            double top1 = Math.Cos(phi_f) * Math.Sin(dl);
            double top2 = (Math.Cos(phi_s) * Math.Sin(phi_f) - Math.Sin(phi_s) * Math.Cos(phi_f) * Math.Cos(dl));
            double bot = Math.Sin(phi_s) * Math.Sin(phi_f) + Math.Cos(phi_s) * Math.Cos(phi_f) * Math.Cos(dl);
            double dist = Math.Atan2(Math.Sqrt(top1 * top1 + top2 * top2), bot);
            return dist;
        }

        /// <summary>
        /// Calculates the GC distance between two points
        /// </summary>
        /// <param name="positionRad1">Position 1 [rad]</param>
        /// <param name="positionRad2">Position 2 [rad]</param>
        /// <returns>The calculated distance [rad]</returns>
        private static double DistanceBetweenPositionsRad(IPosition positionRad1, IPosition positionRad2, out double heading)
        {
            double lat1 = positionRad1.Latitude;
            double lat2 = positionRad2.Latitude;
            double lon1 = positionRad1.Longitude;
            double lon2 = positionRad2.Longitude;
            double head = 0;

            double dist = DistanceBetweenPositionsRad(positionRad1, positionRad2);
            double partial = Math.Cos(dist);

            if (partial < -1) {partial= -1;}
            else if (partial > 1) {partial= 1;}

            // Calc GC Heading
            // Check calculated distance. If zero, set course to zero!
            if (dist < 1E-15) 
            { 
                head= 0;
            }
            else
            {
                // Take care of pole starting points, course otherwise undefined
                if (Math.Cos(lat1) < 1E-15)
                    {
                        if (lat1 > 0)
                        { 
                            head= Math.PI;
                        }
                        else
                        { 
                            head=2* Math.PI;
                        }
                    }
                else
                {
                    partial = (Math.Sin(lat2) - Math.Sin(lat1) * Math.Cos(dist)) / (Math.Sin(dist) * Math.Cos(lat1));
                    if (partial < -1) 
                        { partial= -1;}
                    else if (partial > 1) 
                        { partial= 1;}
                    if (Math.Sin(lon1- lon2) < 0) 
                        { head= Math.Acos(partial);}
                    else 
                        { head = 2 * Math.PI - Math.Acos(partial); }
                }
            }
            heading = head;
            return dist;
        }

        /// <summary>
        /// Calculate an intermediate point along a great circle
        /// </summary>
        /// <param name="position1">The start position</param>
        /// <param name="position2">The end position</param>
        /// <param name="fraction">The fraction of the distance</param>
        /// <returns>The point at the fraction of the distance</returns>
        public static IPosition IntermediatePoint(IPosition position1, IPosition position2, double fraction)
        {
            if (fraction < 0 || fraction > 1.0000000000000004)
                throw new ArgumentOutOfRangeException("fraction", "fraction must be between 0 and 1");
            if (Math.Abs(position1.Latitude - position2.Latitude)<0.00001 && Math.Abs(position1.Longitude - position2.Longitude)<0.00001)
                return position1;
            if (fraction > 1) fraction = 1;

            IPosition pos1 = PositionConverter.Degrees2Radians(position1);
            IPosition pos2 = PositionConverter.Degrees2Radians(position2);
            IPosition result = IntermediatePointRad(pos1, pos2, fraction);
            result = PositionConverter.Radians2Degrees(result);
            return result;
        }

        public static IEnumerable<IPosition> IntermediatePoints(IPosition position1, IPosition position2, int n = 100)
        {
            for (int i = 0; i < n; i++)
            {
                double frac = i / (n-1.0);
                yield return IntermediatePoint(position1, position2, frac);
            }
        }

        public static double CrossTrackError(Position startPosition, Position endPosition, Position currentPosition, out double alongTrackDistance)
        {
            double ATD;
            double distance = CrossTrackErrorRad(PositionConverter.Degrees2Radians(startPosition),
                                                 PositionConverter.Degrees2Radians(endPosition), 
                                                 PositionConverter.Degrees2Radians(currentPosition), out ATD);
            alongTrackDistance = Length.Radian2Meter(ATD);
            return Length.Radian2Meter(distance);
        }

        /// <summary>
        /// Directly from http://williams.best.vwh.net/avform.htm
        /// </summary>
        /// <param name="startPosition"></param>
        /// <param name="endPosition"></param>
        /// <param name="currentPosition"></param>
        /// <param name="alongTrackDistanceRad"></param>
        /// <returns></returns>
        private static double CrossTrackErrorRad(IPosition startPosition, IPosition endPosition, IPosition currentPosition, out double alongTrackDistanceRad)
        {
            
            double crs_AB, crs_AD, dist_AB, dist_AD,XTD,ATD;
            dist_AB = DistanceBetweenPositionsRad(startPosition, endPosition, out crs_AB);
            dist_AD = DistanceBetweenPositionsRad(startPosition, currentPosition, out crs_AD);
            if (startPosition.Latitude == 90)
            {
                XTD = Math.Asin(Math.Sin(dist_AD) * Math.Sin(currentPosition.Longitude - endPosition.Longitude));
                ATD = Math.Asin(Math.Sqrt(Math.Pow(Math.Sin(dist_AD), 2) - Math.Pow(Math.Sin(XTD), 2)) / Math.Cos(XTD));
            }
            else if (startPosition.Latitude == -90)
            {
                XTD = Math.Asin(Math.Sin(dist_AD) * Math.Sin(endPosition.Longitude - currentPosition.Longitude));
                ATD = Math.Asin(Math.Sqrt(Math.Pow(Math.Sin(dist_AD), 2) - Math.Pow(Math.Sin(XTD), 2)) / Math.Cos(XTD));
            }
            else
            {
                double A = crs_AD - crs_AB;
                double b = dist_AD;
                double r = Math.Sqrt(Math.Cos(b) * Math.Cos(b) + Math.Sin(b) * Math.Sin(b) * Math.Cos(A) * Math.Cos(A));
                XTD = Math.Acos(r);
                ATD = Math.Atan2(Math.Sin(b) * Math.Cos(A), Math.Cos(b));
            }
            alongTrackDistanceRad = ATD;
            return XTD;
        }

        private static IPosition IntermediatePointRad(IPosition positionRad1, IPosition positionRad2, double fraction)
        {
            if (fraction < 0 || fraction > 1.0000000000000004)
                throw new ArgumentOutOfRangeException("fraction", "fraction must be between 0 and 1");
            else if (fraction == 0)
                return positionRad1;
            else if (fraction >=1)
                return positionRad2;
            double lat1 = positionRad1.Latitude ;
            double lat2 = positionRad2.Latitude ;
            double lon1 = positionRad1.Longitude;
            double lon2 = positionRad2.Longitude;

            //double head=0;
            double GC_Distance = DistanceBetweenPositionsRad(positionRad1,positionRad2);
            double sinGCdist = Math.Sin(GC_Distance);
            if (sinGCdist < 1e-14)
                throw new Exception("Great circle 180°. Not handled by current code");
            // Calc intermediate position
            double A = Math.Sin((1 - fraction) * GC_Distance) / sinGCdist;
            double B = Math.Sin(fraction * GC_Distance) / sinGCdist;
            double X = A* Math.Cos(lat1)* Math.Cos(lon1)+ B* Math.Cos(lat2)* Math.Cos(lon2);
            double Y = A* Math.Cos(lat1)* Math.Sin(lon1)+ B* Math.Cos(lat2)* Math.Sin(lon2);
            double Z = A* Math.Sin(lat1)+ B* Math.Sin(lat2);
            double outLat = Math.Atan2(Z, Math.Sqrt(X*X+ Y*Y));
            double outLon = Math.Atan2(Y, X);

            // This precerves the longitude when going straight north or south
            if (Math.Abs(lon1-lon2) < 1e-14)
            {
                outLon = lon1;
            }

            // Output
            Position resultRad = new Position(outLat, outLon);
            return resultRad;
        }

    }
}
