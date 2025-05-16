using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace PassagePlanner
{

    public class ReportPartA_Waypoint : INotifyPropertyChanged, IDataErrorInfo
    {
        private double _latitude;
        private double _longitude;
        private Point _longLat;
        private string _timeToGoToDestination;

        private string _toolTip = string.Empty;

        public event PropertyChangedEventHandler PropertyChanged;

        // Default constructor is needed in order to add a new waypoint row in a datagrid
        public ReportPartA_Waypoint() {}


        public ReportPartA_Waypoint(string waypointNo, 
            string waypointName, 
            double latitude, 
            double longitude,
            int courseToNextWaypoint,
            double distanceToNextWaypoint,
            double? legSpeedToNextWaypoint,
            double waterDepth,
            double squat,
            double ukc,
            string engineStatus,
            string navigationalWatchLevel,
            string securityLevel,
            DateTime? eta,
            string maxIntervalsPosFix)
        {
            WaypointNo = waypointNo;
            WaypointName = waypointName;
            _latitude = latitude;
            _longitude = longitude;
            CourseToNextWaypoint = courseToNextWaypoint;
            DistanceToNextWaypoint = distanceToNextWaypoint;
            Speed = legSpeedToNextWaypoint;
            MinDepth = waterDepth;
            Squat = squat;
            Ukc = ukc;
            EngineStatus = engineStatus;
            NavWatchLevel = navigationalWatchLevel;
            SecurityLevel = securityLevel;
            ETA = eta;
            MaxIntervalsPosFix = maxIntervalsPosFix;
        }

        public ReportPartA_Waypoint(string waypointNo,
            string waypointName,
               double latitude,
               double longitude,
               double speed,
               string remarks,
               double minDepth,
               string charts,
               string legReferenceObject_Object,
               string legReferenceObject_Bearing,
               string legReferenceObject_Distance,
               double turnRadius,
               string turnRate,
               string landmarkAtCourseAlt_Object,
               string landmarkAtCourseAlt_Bearing,
               string landmarkAtCourseAlt_Distance,
               string maxOffTrack,
               string maxIntervalsPosFix,
               string engineStatus,
               string navWatchLevel,
               string posFixMethod,
               string listOfLights_Volume,
               string listOfLights_Page,
               string listOfRadioSignals_Volume,
               string listOfRadioSignals_Page,
               string sailingDirections_Volume,
               string sailingDirections_Page,
               string navtexChannels,
               string reportTo,
               string channelOrTelephoneNo,
               string actualPassingTime,
               string securityLevel)
        {
            WaypointNo = waypointNo;
            WaypointName = waypointName;
            Latitude = latitude;
            Longitude = longitude;
            LongLat = new Point(longitude, latitude);
            Speed = speed;
            Remarks = remarks;
            MinDepth = minDepth;
            Charts = charts;
            LegReferenceObject_Object = legReferenceObject_Object;
            LegReferenceObject_Bearing = legReferenceObject_Bearing;
            LegReferenceObject_Distance = legReferenceObject_Distance;
            TurnRadius = turnRadius;
            TurnRate = turnRate;
            LandmarkAtCourseAlt_Object = landmarkAtCourseAlt_Object;
            LandmarkAtCourseAlt_Bearing = landmarkAtCourseAlt_Bearing;
            LandmarkAtCourseAlt_Distance = landmarkAtCourseAlt_Distance;
            MaxOffTrack = maxOffTrack;
            MaxIntervalsPosFix = maxIntervalsPosFix;
            EngineStatus = engineStatus;
            NavWatchLevel = navWatchLevel;
            PosFixMethod = posFixMethod;
            ListOfLights_Volume = listOfLights_Volume;
            ListOfLights_Page = listOfLights_Page;
            ListOfRadioSignals_Volume = listOfRadioSignals_Volume;
            ListOfRadioSignals_Page = listOfRadioSignals_Page;
            SailingDirections_Volume = sailingDirections_Volume;
            SailingDirections_Page = sailingDirections_Page;
            NavtexChannels = navtexChannels;
            ReportTo = reportTo;
            ChannelOrTelephoneNo = channelOrTelephoneNo;
            ActualPassingTime = actualPassingTime;
            SecurityLevel = securityLevel;
        }

        public string Error
        {
            get
            {
                return null;
            }
        }


        public string WaypointNo { get; set; }
        public string WaypointName { get; set; }
        public int CourseToNextWaypoint { get; set; }
        public double DistanceToNextWaypoint { get; set; }
        public double? Speed { get; set; }

        public DateTime? ETA { get; set; }


        public string Remarks { get; set; }
        public double MinDepth { get; set; }
        public double Squat { get; set; }
        public double Ukc { get; set; }


        public string Charts { get; set; }
        public string LegReferenceObject_Object { get; set; }
        public string LegReferenceObject_Bearing { get; set; }
        public string LegReferenceObject_Distance { get; set; }

        public double TurnRadius { get; set; }
        public string TurnRate { get; set; }
        public string LandmarkAtCourseAlt_Object { get; set; }
        public string LandmarkAtCourseAlt_Bearing { get; set; }
        public string LandmarkAtCourseAlt_Distance { get; set; }
        public string MaxOffTrack { get; set; }
        public string MaxIntervalsPosFix { get; set; }
        public string EngineStatus { get; set; }
        public string NavWatchLevel { get; set; }
        public string PosFixMethod { get; set; }
        public string ListOfLights_Volume { get; set; }
        public string ListOfLights_Page { get; set; }
        public string ListOfRadioSignals_Volume { get; set; }
        public string ListOfRadioSignals_Page { get; set; }
        public string SailingDirections_Volume { get; set; }
        public string SailingDirections_Page { get; set; }
        public string NavtexChannels { get; set; }
        public string ReportTo { get; set; }
        public string ChannelOrTelephoneNo { get; set; }
        public string ActualPassingTime { get; set; }
        public string SecurityLevel { get; set; }

        public double Latitude 
        { 
            get 
            { 
                return _latitude; 
            } 
            set 
            { 
                _latitude = value; 
                LongLat = new Point(Longitude, Latitude);
            } 
        }

        public double Longitude 
        { 
            get 
            { 
                return _longitude; 
            } 
            set 
            { 
                _longitude = value; 
                LongLat = new Point(Longitude, Latitude);
            } 
        }

        public Point LongLat
        {
            get
            {
                return _longLat;
            }
            set
            {
                _longLat = value;
                OnPropertyChanged("LongLat");
            }
        }

        public string TimeToGoToDestination 
        { 
            get 
            {
                _timeToGoToDestination = "32 d 23:45";
                return _timeToGoToDestination; 
            } 
            set 
            {
                _timeToGoToDestination = value;
            } 
        }
        

        public string this[string name]
        {
            get
            {
                string result = null;

                if (name == "LongitudeDegrees")
                {
                    //if (this._longitudeDegrees < -180 || this._longitudeDegrees > 180)
                    //{
                        result = "Longitude Degrees must not be less than -180 or greater than 180.";
                    //}
                }
                return result;
            }
        }



        protected virtual void OnPropertyChanged(string name)
        {
            PropertyChangedEventHandler handler = System.Threading.Interlocked.CompareExchange(ref PropertyChanged, null, null);
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(name));
            }
        }
    }
}
