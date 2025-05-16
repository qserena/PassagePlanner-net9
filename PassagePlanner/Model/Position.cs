using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PassagePlanner
{
    public class Position
    {
        private double _latitude;
        private double _longitude;
        private string _latitudeLetter;
        private string _longitudeLetter;
        private int _latitudeDegrees;
        private int _longitudeDegrees;
        private double _latitudeMinutes;
        private double _longitudeMinutes;

        public Position(double latitude, double longitude)
        {
            _latitude = latitude;
            _longitude = longitude;

            _latitudeLetter = (_latitude >= 0 ? "N" : "S");
            _longitudeLetter = (_longitude >= 0 ? "E" : "W");

            var unsignedLatitudeValue = Math.Abs(_latitude);
            var unsignedLongitudeValue = Math.Abs(_longitude);

            _latitudeDegrees = Convert.ToInt32(Math.Floor(unsignedLatitudeValue));
            _longitudeDegrees = Convert.ToInt32(Math.Floor(unsignedLongitudeValue));

            _latitudeMinutes = (unsignedLatitudeValue - _latitudeDegrees) * 60.0;
            _longitudeMinutes = (unsignedLongitudeValue - _longitudeDegrees) * 60.0;

        }

        public Position(string latitudeLetter, 
            int latitudeDegrees,
            double latitudeMinutes,
            string longitudeLetter,
            int longitudeDegrees,
            double longitudeMinutes)
        {
            _latitudeLetter = latitudeLetter;
            _longitudeLetter = longitudeLetter;
            _latitudeDegrees = latitudeDegrees;
            _longitudeDegrees = longitudeDegrees;
            _latitudeMinutes = latitudeMinutes;
            _longitudeMinutes = longitudeMinutes;

            CalculateLongAndLat();
        }

        private void CalculateLongAndLat()
        {
            var unsignedLatitudeValue = _latitudeDegrees + _latitudeMinutes / 60;
            _latitude = (_latitudeLetter.Equals("N") ? unsignedLatitudeValue : -unsignedLatitudeValue);

            var unsignedLongitudeValue = _longitudeDegrees + _longitudeMinutes / 60;
            _longitude = (_longitudeLetter.Equals("E") ? unsignedLongitudeValue : -unsignedLongitudeValue);
        }


        public double Latitude
        {
            get
            {
                return _latitude;
            }
        }

        public double Longitude
        {
            get
            {
                return _longitude;
            }
        }

        public string LatitudeFullString
        {
            get
            {
                return _latitudeLetter + " " + _latitudeDegrees + " " + _latitudeMinutes;
            }
        }

        public string LongitudeFullString
        {
            get
            {
                return _longitudeLetter + " " + _longitudeDegrees + " " + _longitudeMinutes;
            }
        }

        public string LatitudeLetter
        {
            get
            {
                return _latitudeLetter;
            }
            set
            {
                _latitudeLetter = value;
                CalculateLongAndLat();
            }
        }

        public string LongitudeLetter
        {
            get
            {
                return _longitudeLetter;
            }
            set
            {
                _longitudeLetter = value;
                CalculateLongAndLat();
            }
        }

        public int LatitudeDegrees
        {
            get
            {
                return _latitudeDegrees;
            }

            set
            {
                _latitudeDegrees = value;
                CalculateLongAndLat();
            }
        }

        public double LatitudeMinutes
        {
            get
            {
                return _latitudeMinutes;
            }

            set
            {
                _latitudeMinutes = value;
                CalculateLongAndLat();

            }
        }

        public int LongitudeDegrees
        {
            get
            {
                return _longitudeDegrees;
            }

            set
            {
                _longitudeDegrees = value;
                CalculateLongAndLat();
            }
        }

        public double LongitudeMinutes
        {
            get
            {
                return _longitudeMinutes;
            }

            set
            {
                _longitudeMinutes = value;
                CalculateLongAndLat();
            }
        }
    }
}
