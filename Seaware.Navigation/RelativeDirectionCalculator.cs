using System;
using System.Collections.Generic;
using System.Linq;


namespace Seaware.Navigation
{
    public class RelativeDirectionCalculator
    {
        public void Calculate(double? course, double? directionDeg, double? magnitude,
            out double? relativeDirection, out double? projectedMagnitude)
        {
            relativeDirection = null;
            projectedMagnitude = null;
            if (course.HasValue && directionDeg.HasValue)
            {
                relativeDirection = course.Value - directionDeg.Value;
                if (magnitude.HasValue)
                {
                    projectedMagnitude = magnitude.Value * Math.Cos(relativeDirection.Value * Math.PI / 180.0);
                }
            }
        }

        public void Calculate(double? course, double? directionDeg,
            out double? relativeDirection)
        {
            relativeDirection = null;
            if (course.HasValue && directionDeg.HasValue)
            {
                relativeDirection = course.Value - directionDeg.Value;
            }
        }

        
    }
}