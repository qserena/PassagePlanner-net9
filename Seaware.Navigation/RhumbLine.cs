using Seaware.Navigation.Converters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Seaware.Navigation
{
    public static class RhumbLine
    {
        /// <summary>
        /// Calculates the position starting from a point going a certain distance in a direction
        /// 
        /// WARNING! This function seems to assume that the world is flat!
        /// </summary>
        /// <param name="start">The starting point [°]</param>
        /// <param name="heading">The heading [deg]</param>
        /// <param name="distance">The distance [m]</param>
        /// <returns></returns>
        public static IPosition DeadReckoningFromPosition(IPosition start, double heading, double distance)
        {
            if (double.IsNaN(distance))
                throw new ArgumentException("Distance can't be NaN", "distance");
            // Convert input to radians!
            double lat1 = start.Latitude * Math.PI / 180.0;
            double lon1 = start.Longitude * Math.PI / 180.0;
            double headingRad = heading * Math.PI / 180.0;
            double distanceRad = distance * Math.PI / (180.0 * 60.0 * 1852);
            // Calculate
            double lat = lat1 + distanceRad * Math.Cos(headingRad);
            double diffLat = Math.Log(Math.Tan(lat / 2 + Math.PI / 4) / Math.Tan(lat1 / 2 + Math.PI / 4));
            double q = 0;
            if (Math.Abs(lat - lat1) < 1E-15)
            {
                q = Math.Cos(lat1);
            }
            else
            {
                q = (lat - lat1) / diffLat;
            }
            double theDiffLong = distanceRad * Math.Sin(headingRad) / q;
            double theSumLong = lon1 + theDiffLong + Math.PI;
            double theLong = (theSumLong - 2 * Math.PI * Math.Floor(theSumLong / (2 * Math.PI))) - Math.PI;
            if (Math.Abs(Math.Cos(theLong) - Math.Cos(lon1)) < 1e-14)
            {
                theLong = lon1;
            }

            if (Math.Abs(theLong) > 360 || Math.Abs(lat) > 90 ||
                Double.IsNaN(theLong) || Double.IsNaN(lat) ||
                Double.IsInfinity(theLong) || Double.IsInfinity(lat))
            {
                throw new Exception("Failed to to calculate dead reckoning from position. Most likely the position was to close to a pole");
            }
            // Output
            Position result = new Position(lat * 180.0 / Math.PI, theLong * 180.0 / Math.PI);
            return result;
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
            if (position1.Latitude == position2.Latitude && position1.Longitude == position2.Longitude)
                return position1;
            if (fraction > 1) fraction = 1;
            double heading, distance;
            RhumbLine.DistanceAndCourseBetweenPositions(position1, position2, out distance, out heading);
            IPosition result = RhumbLine.DeadReckoningFromPosition(position1, heading, distance * fraction);
            return result;
        }


        /// <summary>
        /// Calculates the rhumb line heading between two positions
        /// </summary>
        /// <param name="start">The start point [°]</param>
        /// <param name="end">The end point [°]</param>
        /// <returns>The heading [°]</returns>
        public static double HeadingBetweenPositions(IPosition start, IPosition end)
        {
            double courseDeg, distance;
            DistanceAndCourseBetweenPositions(start, end, out distance, out courseDeg);
            return courseDeg;
        }

        /// <summary>
        /// Calculates the rhumb line distance between two positions
        /// </summary>
        /// <param name="start">The start point [°]</param>
        /// <param name="end">The end point [°]</param>
        /// <returns>The distance [m]</returns>
        public static double DistanceBetweenPositions(IPosition start, IPosition end)
        {
            double courseDeg, distance;
            DistanceAndCourseBetweenPositions(start, end, out distance, out courseDeg);
            return distance;
        }

        private static void DistanceAndCourseBetweenPositionsRad(IPosition startRad, IPosition endRad, out double distanceRad, out double headingRad)
        {
            double theLat1 = startRad.Latitude;
            double theLong1 = startRad.Longitude;
            double theLat2 = endRad.Latitude;
            double theLong2 = endRad.Longitude;

            double theDiffLong = theLong1 - theLong2;
            const double DOUBLE_PI = Math.PI * 2;
            double theDiffLongWest = theDiffLong - DOUBLE_PI * Math.Floor(theDiffLong / DOUBLE_PI);
            theDiffLong = theLong2 - theLong1;
            double theDiffLongEast = theDiffLong - DOUBLE_PI * Math.Floor(theDiffLong / DOUBLE_PI);
            double theDiffLat = Math.Log(Math.Tan(theLat2 / 2 + Math.PI / 4) / Math.Tan(theLat1 / 2 + Math.PI / 4));
            double theQ;
            if (Math.Abs(theLat2 - theLat1) < 1E-15)
            {
                theQ = Math.Cos(theLat1);
            }
            else
            {
                theQ = (theLat2 - theLat1) / theDiffLat;
            }
            if (theDiffLongWest < theDiffLongEast)                   // Shortest way West!
            {
                distanceRad = Math.Sqrt((theQ * theQ) * (theDiffLongWest * theDiffLongWest) + Math.Pow(theLat2 - theLat1, 2));
                if (distanceRad < 1E-15)
                {
                    headingRad = 0;
                    distanceRad = 0;
                }
                else
                {
                    double thePartial = Math.Atan2(-1 * theDiffLongWest, theDiffLat);
                    headingRad = thePartial - DOUBLE_PI * Math.Floor(thePartial / DOUBLE_PI);
                }
            }
            else                                                      // Shortest way East!
            {
                distanceRad = Math.Sqrt((theQ * theQ) * (theDiffLongEast * theDiffLongEast) + Math.Pow(theLat2 - theLat1, 2));
                if (distanceRad < 1E-15)
                {
                    headingRad = 0;
                    distanceRad = 0;
                }
                else
                {
                    double thePartial = Math.Atan2(theDiffLongEast, theDiffLat);
                    headingRad = thePartial - DOUBLE_PI * Math.Floor(thePartial / DOUBLE_PI);
                }
            }
        }

        /// <summary>
        /// Calculates the rhumb line distance and heading between two positions
        /// </summary>
        /// <param name="start">The start point [°]</param>
        /// <param name="end">The end point [°]</param>
        /// <param name="distanceRad">The distance [m]</param>
        /// <param name="headingRad">The heading [°]</param>
        public static void DistanceAndCourseBetweenPositions(IPosition start, IPosition end, out double distance, out double heading)
        {
            double distanceRad, headingRad;
            DistanceAndCourseBetweenPositionsRad(PositionConverter.Degrees2Radians(start),
                                                 PositionConverter.Degrees2Radians(end),
                                                 out distanceRad, out headingRad);
            distance = Length.Radian2Meter(distanceRad);
            heading = headingRad * 180 / Math.PI;
        }
        /*
        public static void UpdateSpeedAndCourseInObservedReports(IList<IObservedReport> positionReports)
        {
            if (!positionReports.Any())
                return;
            var orderedReports = from pr in positionReports
                                 orderby pr.Time ascending
                                 select pr;
            IObservedReport lastReport = null;
            IObservedReport beforeLastReport = null;
            foreach (var pr in orderedReports)
            {
                if (lastReport != null)
                {
                    double distance, course;
                    TimeSpan dt = pr.Time - lastReport.Time;
                    IObservedReport lastPosReport = lastReport;
                    RhumbLine.DistanceAndCourseBetweenPositions(lastReport.GetPosition(), pr.GetPosition(), out distance, out course);
                    if (!lastPosReport.Course.HasValue || !lastPosReport.Speed.HasValue)
                    {
                        if (!lastPosReport.Speed.HasValue && dt.TotalSeconds > 0)
                        {
                            lastPosReport.Speed = distance / dt.TotalSeconds;
                        }
                        if (!lastPosReport.Course.HasValue)
                        {
                            lastPosReport.Course = course;
                        }
                    }
                    //if (dt.TotalSeconds > 0)
                    //    yield return lastReport;
                }
                beforeLastReport = lastReport;
                lastReport = pr;
            }

            if (beforeLastReport != null)
            {
                if (!lastReport.Course.HasValue)
                    lastReport.Course = beforeLastReport.Course;
                if (!lastReport.Speed.HasValue)
                    lastReport.Speed = beforeLastReport.Speed;
            }

            //yield return lastReport;
        }

        public static IEnumerable<PosReport> CalculateMissingValuesInPositionReports(IEnumerable<PosReport> positionReports)
        {
            if (!positionReports.Any())
                yield break;
            var orderedReports = from pr in positionReports
                                 orderby pr.Time ascending
                                 select pr;
            PosReport lastReport = null;
            PosReport beforeLastReport = null;
            foreach (var pr in orderedReports)
            {
                if (lastReport != null)
                {
                    double distance, course;
                    TimeSpan dt = pr.Time - lastReport.Time;
                    PosReport lastPosReport = lastReport;
                    RhumbLine.DistanceAndCourseBetweenPositions(lastReport.Position, pr.Position, out distance, out course);
                    if (!lastPosReport.Course.HasValue || !lastPosReport.Speed.HasValue)
                    {
                        if (!lastPosReport.Speed.HasValue && dt.TotalSeconds > 0)
                        {
                            lastPosReport.Speed = distance / dt.TotalSeconds;
                        }
                        if (!lastPosReport.Course.HasValue)
                        {
                            lastPosReport.Course = course;
                        }
                    }
                    if (dt.TotalSeconds > 0)
                        yield return lastReport;
                }
                beforeLastReport = lastReport;
                lastReport = pr;
            }

            if (beforeLastReport != null)
            {
                if (!lastReport.Course.HasValue)
                    lastReport.Course = beforeLastReport.Course;
                if (!lastReport.Speed.HasValue)
                    lastReport.Speed = beforeLastReport.Speed;
            }
            if (lastReport != null)
                yield return lastReport;
            else
                yield break;
        }
        */
        private static Position IntersectionRad(Position pos1, double course1, Position pos2, double course2)
        {
            if (pos1.Latitude == pos2.Latitude && pos1.Longitude == pos2.Longitude)
                return pos1;
            else if (course1 == course2)
                throw new Exception("Paralell rhumb lines won't intersect (except on one of the poles)");
            double lat1 = pos1.Latitude;
            double lon1 = pos1.Longitude;
            double crs13 = course1;
            double crs23 = course2;
            double dst12, crs12, crs21, tmp, lat3, lon3;
            RhumbLine.DistanceAndCourseBetweenPositions(pos1, pos2, out dst12, out crs12);
            RhumbLine.DistanceAndCourseBetweenPositions(pos1, pos2, out tmp, out crs21);

            // TODO: Check modulus
            double ang1 = (crs13 - crs12 + Math.PI) % (2.0 * Math.PI) - Math.PI;
            double ang2 = (crs21 - crs23 + Math.PI) % (2.0 * Math.PI) - Math.PI;

            if (Math.Abs(Math.Sin(ang1)) < 1e6 && Math.Abs(Math.Sin(ang2)) < 1e6)
                throw new Exception("Infinity of intersections");
            else if (Math.Sin(ang1) * Math.Sin(ang2) < 0)
                throw new Exception("Intersection ambiguous");
            else
            {
                ang1 = Math.Abs(ang1);
                ang2 = Math.Abs(ang2);
                double ang3 = Math.Acos(-Math.Cos(ang1) * Math.Cos(ang2) + Math.Sin(ang1) * Math.Sin(ang2) * Math.Cos(dst12));
                double dst13 = Math.Atan2(Math.Sin(dst12) * Math.Sin(ang1) * Math.Sin(ang2), Math.Cos(ang2) + Math.Cos(ang1) * Math.Cos(ang3));
                lat3 = Math.Asin(Math.Sin(lat1) * Math.Cos(dst13) + Math.Cos(lat1) * Math.Sin(dst13) * Math.Cos(crs13));
                double dlon = Math.Atan2(Math.Sin(crs13) * Math.Sin(dst13) * Math.Cos(lat1), Math.Cos(dst13) - Math.Sin(lat1) * Math.Sin(lat3));
                lon3 = (lon1 - dlon + Math.PI) % (2 * Math.PI) - Math.PI;
            }
            return new Position(lat3, lon3);
        }

    }

}
