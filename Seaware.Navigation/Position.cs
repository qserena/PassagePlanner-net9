using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Seaware.Navigation
{
    public class Position : IPosition
    {
        public Position()
        {
        }
        public Position(IPosition pos)
        {
            Latitude = pos.Latitude;
            Longitude = pos.Longitude;
        }
        public Position(double latitude, double longitude)
        {
            Latitude = latitude;
            Longitude = longitude;
        }
        public double Latitude {get;set;}
        public double Longitude {get;set;}

        public override bool Equals(object obj)
        {
            if (obj == null || GetType() != obj.GetType())
            {
                return false;
            }

            Position pos = (Position)obj;
            return pos.Latitude == Latitude && pos.Longitude == this.Longitude;
        }

        public override int GetHashCode()
        {
            return Latitude.GetHashCode() ^ Longitude.GetHashCode() * 3;
        }

        public override string ToString()
        {
            return Latitude.ToString() + " " + Longitude.ToString();
        }
    }
}
