using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Seaware.Navigation;
using Seaware.Navigation.Enumerations;
using EcdisLayer;
using System.Xml.Serialization;

namespace PassagePlanner
{
    [Serializable()]
    public class Waypoint : INotifyPropertyChanged, ICloneable, IWaypoint
    {
        private string _waypointNo = string.Empty;
        private string _waypointName = string.Empty;
        private string _waypointNoAndName = string.Empty;
        private double _latitude;
        private double _longitude;
        private LegType _legType;
        private double? _legSpeedSetting;
        private DateTime? _etd;
        private int _courseToNextWaypoint;
        private double _distanceToNextWaypoint;
        private Point _longLat;
        private TimeSpan _sailingTimeToThisWaypoint;
        private TimeSpan _timeToGoToDestination;
        private string _timeToGoToDestinationAsString;
        private TimeSpan _timeToNextWaypoint;
        private double _distanceToGoToDestination;
        private double _distanceSailed;
        private SquatUkcItem _squatUkc;
        private string _toolTip = string.Empty;

        private string _remarks;
        private double _minDepth;
        private string _charts;
        private string _legReferenceObject_Object;
        private string _legReferenceObject_Bearing;
        private string _legReferenceObject_Distance;
        private double _turnRadius;
        private string _turnRate;
        private string _landmarkAtCourseAlt_Object;
        private string _landmarkAtCourseAlt_Bearing;
        private string _landmarkAtCourseAlt_Distance;
        private string _maxOffTrack;
        private string _maxIntervalsPosFix;
        private string _engineStatus;
        private string _navWatchLevel;
        private string _posFixMethod;
        private string _listOfLights_Volume;
        private string _listOfLights_Page;
        private string _listOfRadioSignals_Volume;
        private string _listOfRadioSignals_Page;
        private string _sailingDirections_Volume;
        private string _sailingDirections_Page;
        private string _navtexChannels;
        private string _reportTo;
        private string _channelOrTelephoneNo;
        private string _actualPassingTime;
        private string _securityLevel;

        public event PropertyChangedEventHandler PropertyChanged;

        // Casting operator
        public static explicit operator WaypointForClipboard(Waypoint instance)
        {
            return new WaypointForClipboard(instance);
        }

        // Constructor made for casting operator above
        public Waypoint(WaypointForClipboard wpForClipboard)
        {
            WaypointName = wpForClipboard.WaypointName;
            Latitude = wpForClipboard.Latitude;
            Longitude = wpForClipboard.Longitude;
            LegType = wpForClipboard.LegType;
            LegSpeedSetting = wpForClipboard.LegSpeedSetting;
            Position = new Seaware.Navigation.Position(Latitude, Longitude);
            LongLat = new Point(Longitude, Latitude);
            Remarks = wpForClipboard.Remarks;
            MinDepth = wpForClipboard.MinDepth;
            Charts = wpForClipboard.Charts;
            LegReferenceObject_Object = wpForClipboard.LegReferenceObject_Object;
            LegReferenceObject_Bearing = wpForClipboard.LegReferenceObject_Bearing;
            LegReferenceObject_Distance = wpForClipboard.LegReferenceObject_Distance;
            TurnRadius = wpForClipboard.TurnRadius;
            TurnRate = wpForClipboard.TurnRate;
            LandmarkAtCourseAlt_Object = wpForClipboard.LandmarkAtCourseAlt_Object;
            LandmarkAtCourseAlt_Bearing = wpForClipboard.LandmarkAtCourseAlt_Bearing;
            LandmarkAtCourseAlt_Distance = wpForClipboard.LandmarkAtCourseAlt_Distance;
            MaxOffTrack = wpForClipboard.MaxOffTrack;
            MaxIntervalsPosFix = wpForClipboard.MaxIntervalsPosFix;
            EngineStatus = wpForClipboard.EngineStatus;
            NavWatchLevel = wpForClipboard.NavWatchLevel;
            PosFixMethod = wpForClipboard.PosFixMethod;
            ListOfLights_Volume = wpForClipboard.ListOfLights_Volume;
            ListOfLights_Page = wpForClipboard.ListOfLights_Page;
            ListOfRadioSignals_Volume = wpForClipboard.ListOfRadioSignals_Volume;
            ListOfRadioSignals_Page = wpForClipboard.ListOfRadioSignals_Page;
            SailingDirections_Volume = wpForClipboard.SailingDirections_Volume;
            SailingDirections_Page = wpForClipboard.SailingDirections_Page;
            NavtexChannels = wpForClipboard.NavtexChannels;
            ReportTo = wpForClipboard.ReportTo;
            ChannelOrTelephoneNo = wpForClipboard.ChannelOrTelephoneNo;
            ActualPassingTime = wpForClipboard.ActualPassingTime;
            SecurityLevel = wpForClipboard.SecurityLevel;
            ToolTip = wpForClipboard.ToolTip;

            ViewModelLocator locator = new ViewModelLocator();
            if (LegSpeedSetting == null)
            {
                LegSpeedSetting = 0.0;
            }
            SquatUkc = new SquatUkcItem((double)LegSpeedSetting, (double?)MinDepth, locator.VesselVM.VesselBeam);
        }

        // Default constructor is needed in order to add a new waypoint row in a datagrid
        public Waypoint() 
        {
            // Set default value equal to the previous waypoint
            ViewModelLocator locator = new ViewModelLocator();
            DispatchingObservableCollection<Waypoint> waypoints = locator.RouteVM.Waypoints;
            if (waypoints != null && waypoints.Count > 0)
            {
                double previousLatitude = ((Waypoint)waypoints[waypoints.Count - 1]).Latitude;
                double previousLongitude = ((Waypoint)waypoints[waypoints.Count - 1]).Longitude;

                // Special trick to set the same cardinal letter (N, S, E or W) as previous waypoint
                if (previousLatitude < 0.0)
                {
                    Latitude = -0.000000000001;
                }
                if (previousLongitude < 0.0)
                {
                    Longitude = -0.000000000001;
                }
            }
        }

        /// <summary>
        /// Constructor made for adding waypoints when reading Seaware format files (*.swx)
        /// </summary>
        /// <param name="latitude"></param>
        /// <param name="longitude"></param>
        public Waypoint(double latitude, double longitude, LegType legType, double? resultingSpeedInKnot)
        {
            _latitude = latitude;
            _longitude = longitude;
            Position = new Seaware.Navigation.Position(latitude, longitude);
            LongLat = new Point(longitude, latitude);
            LegType = legType;
            LegSpeedSetting = resultingSpeedInKnot;
        }

        /// <summary>
        /// Constructor for Ecdis import
        /// </summary>
        /// <param name="ecdisWaypoint"></param>
        public Waypoint(TSw_EcdisImportAndExportLegWaypointType ecdisWaypoint)
        {
            _latitude = ecdisWaypoint.latitude;
            _longitude = ecdisWaypoint.longitude;
            Position = new Seaware.Navigation.Position(_latitude, _longitude);
            _waypointName = ecdisWaypoint.waypointName;
            LongLat = new Point(_longitude, _latitude);
            TurnRadius = ecdisWaypoint.turnRadius;
            //TurnRate = ecdisWaypoint.turnRate.ToString();
            TurnRate = String.Format("{0:0.##}", ecdisWaypoint.turnRate);
            LegSpeedSetting = ecdisWaypoint.speed;
            if (ecdisWaypoint.followingLegType == TSw_EcdisImportAndExportLegType.gc)
            {
                LegType = LegType.GreatCircle;
            }
            else
            {
                LegType = LegType.RhumbLine;
            }
        }

        /// <summary>
        /// Contstructor for adding waypoints when opening old NaviPlan Excel file format (*.xls)
        /// </summary>
        /// <param name="waypointName"></param>
        /// <param name="latitude"></param>
        /// <param name="longitude"></param>
        /// <param name="speed"></param>
        /// <param name="remarks"></param>
        /// <param name="minDepth"></param>
        /// <param name="charts"></param>
        /// <param name="legReferenceObject_Object"></param>
        /// <param name="legReferenceObject_Bearing"></param>
        /// <param name="legReferenceObject_Distance"></param>
        /// <param name="turnRadius"></param>
        /// <param name="turnRate"></param>
        /// <param name="landmarkAtCourseAlt_Object"></param>
        /// <param name="landmarkAtCourseAlt_Bearing"></param>
        /// <param name="landmarkAtCourseAlt_Distance"></param>
        /// <param name="maxOffTrack"></param>
        /// <param name="maxIntervalsPosFix"></param>
        /// <param name="engineStatus"></param>
        /// <param name="navWatchLevel"></param>
        /// <param name="posFixMethod"></param>
        /// <param name="listOfLights_Volume"></param>
        /// <param name="listOfLights_Page"></param>
        /// <param name="listOfRadioSignals_Volume"></param>
        /// <param name="listOfRadioSignals_Page"></param>
        /// <param name="sailingDirections_Volume"></param>
        /// <param name="sailingDirections_Page"></param>
        /// <param name="navtexChannels"></param>
        /// <param name="reportTo"></param>
        /// <param name="channelOrTelephoneNo"></param>
        /// <param name="actualPassingTime"></param>
        /// <param name="securityLevel"></param>
        public Waypoint(string waypointName,
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
            WaypointName = waypointName;
            Latitude = latitude;
            Longitude = longitude;
            Position = new Seaware.Navigation.Position(latitude, longitude);
            LongLat = new Point(longitude, latitude);
            LegSpeedSetting = speed;
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

        // STATIC dirty flag takes care of the case when ANY Waypoint is dirty.
        [XmlIgnore]
        public static bool IsDirty { get; set; }

        // Seaware.Navigation.IWaypoint members
        public DateTime? ETD
        {
            get
            {
                return _etd;
            }
            set
            {
                if (value == _etd)
                {
                    return;
                }

                _etd = value;
                OnPropertyChanged("ETD");
            }
        }

        public int CourseToNextWaypoint
        {
            get
            {
                return _courseToNextWaypoint;
            }
            set
            {
                if (value == _courseToNextWaypoint)
                {
                    return;
                }

                _courseToNextWaypoint = value;
                OnPropertyChanged("CourseToNextWaypoint");
            }
        }

        public double DistanceToNextWaypoint
        {
            get
            {
                return _distanceToNextWaypoint;
            }
            set
            {
                if (value == _distanceToNextWaypoint)
                {
                    return;
                }

                _distanceToNextWaypoint = value;
                OnPropertyChanged("DistanceToNextWaypoint");
            }
        }

        [XmlIgnore]
        public bool IsAutoGenerated
        {
            get;
            set;
        }

        // Ignore because intefaces cannot be serialized.
        [XmlIgnore]
        public IPosition Position
        {
            get;
            set;
        }

        /// <summary>
        /// Seaware.Navigation.Enumerations.LegType is used 
        /// for calculations etc.
        /// </summary>
        public LegType LegType
        {
            get
            { 
                return _legType;
            }
            set
            {
                if (value == _legType)
                {
                    return;
                }

                _legType = value;

                IsDirty = true;
                OnPropertyChanged("LegType");
            }
        }


        public double? LegSpeedSetting
        {
            get
            {
                return _legSpeedSetting;
            }
            set
            {
                if (value == _legSpeedSetting)
                {
                    return;
                }

                _legSpeedSetting = value;
                IsDirty = true;
                OnPropertyChanged("LegSpeedSetting");
            }
        }

        public bool IsGreatCircle
        {
            get { return LegType == LegType.GreatCircle;  }
            set { LegType = (value ? LegType.GreatCircle : LegType.RhumbLine); }
        }

        [XmlIgnore]
        public double? LegPowerSetting { get; set; }

        [XmlIgnore]
        public double? LegRpmSetting { get; set; }


        [XmlIgnore]
        public DateTime? Time { get; set; }

        [XmlIgnore]
        public IFuelUsage FuelUsage { get; set; }

        [XmlIgnore]
        public TimeSpan StoppageOrTransitDuration { get; set; }

        [XmlIgnore]
        public WaypointType WaypointType { get; set; }

        [XmlIgnore]
        public bool LegIsTrackOptimizable { get; set; }

        [XmlIgnore]
        public bool LegIsSpeedOptimizable { get; set; }

        [XmlIgnore]
        public string Error
        {
            get
            {
                return null;
            }
        }


        public string Remarks
        {
            get
            {
                return _remarks;
            }
            set
            {
                if (value == _remarks)
                {
                    return;
                }

                _remarks = value;
                IsDirty = true;
                OnPropertyChanged("Remarks");
            }
        }

        public double MinDepth
        {
            get
            {
                return _minDepth;
            }
            set
            {
                if (value == _minDepth)
                {
                    return;
                }

                _minDepth = value;
                IsDirty = true;
                OnPropertyChanged("MinDepth");
            }
        }

        public string Charts
        {
            get
            {
                return _charts;
            }
            set
            {
                if (value == _charts)
                {
                    return;
                }

                _charts = value;
                IsDirty = true;
                OnPropertyChanged("Charts");
            }
        }

        public string LegReferenceObject_Object
        {
            get
            {
                return _legReferenceObject_Object;
            }
            set
            {
                if (value == _legReferenceObject_Object)
                {
                    return;
                }

                _legReferenceObject_Object = value;
                IsDirty = true;
                OnPropertyChanged("LegReferenceObject_Object");
            }
        }

        public string LegReferenceObject_Bearing
        {
            get
            {
                return _legReferenceObject_Bearing;
            }
            set
            {
                if (value == _legReferenceObject_Bearing)
                {
                    return;
                }

                _legReferenceObject_Bearing = value;
                IsDirty = true;
                OnPropertyChanged("LegReferenceObject_Bearing");
            }
        }

        public string LegReferenceObject_Distance
        {
            get
            {
                return _legReferenceObject_Distance;
            }
            set
            {
                if (value == _legReferenceObject_Distance)
                {
                    return;
                }

                _legReferenceObject_Distance = value;
                IsDirty = true;
                OnPropertyChanged("LegReferenceObject_Distance");
            }
        }

        public double TurnRadius
        {
            get
            {
                return _turnRadius;
            }
            set
            {
                if (value == _turnRadius)
                {
                    return;
                }

                _turnRadius = value;
                IsDirty = true;
                OnPropertyChanged("TurnRadius");
            }
        }

        public string TurnRate
        {
            get
            {
                return _turnRate;
            }
            set
            {
                if (value == _turnRate)
                {
                    return;
                }

                _turnRate = value;
                IsDirty = true;
                OnPropertyChanged("TurnRate");
            }
        }

        public string LandmarkAtCourseAlt_Object
        {
            get
            {
                return _landmarkAtCourseAlt_Object;
            }
            set
            {
                if (value == _landmarkAtCourseAlt_Object)
                {
                    return;
                } 
                _landmarkAtCourseAlt_Object = value;
                IsDirty = true;
                OnPropertyChanged("LandmarkAtCourseAlt_Object");
            }
        }

        public string LandmarkAtCourseAlt_Bearing
        {
            get
            {
                return _landmarkAtCourseAlt_Bearing;
            }
            set
            {
                if (value == _landmarkAtCourseAlt_Bearing)
                {
                    return;
                }

                _landmarkAtCourseAlt_Bearing = value;
                IsDirty = true;
                OnPropertyChanged("LandmarkAtCourseAlt_Bearing");
            }
        }

        public string LandmarkAtCourseAlt_Distance
        {
            get
            {
                return _landmarkAtCourseAlt_Distance;
            }
            set
            {
                if (value == _landmarkAtCourseAlt_Distance)
                {
                    return;
                }

                _landmarkAtCourseAlt_Distance = value;
                IsDirty = true;
                OnPropertyChanged("LandmarkAtCourseAlt_Distance");
            }
        }

        public string MaxOffTrack
        {
            get
            {
                return _maxOffTrack;
            }
            set
            {
                if (value == _maxOffTrack)
                {
                    return;
                }

                _maxOffTrack = value;
                IsDirty = true;
                OnPropertyChanged("MaxOffTrack");
            }
        }

        public string MaxIntervalsPosFix
        {
            get
            {
                return _maxIntervalsPosFix;
            }
            set
            {
                if (value == _maxIntervalsPosFix)
                {
                    return;
                }

                _maxIntervalsPosFix = value;
                IsDirty = true;
                OnPropertyChanged("MaxIntervalsPosFix");
            }
        }

        public string EngineStatus
        {
            get
            {
                return _engineStatus;
            }
            set
            {
                if (value == _engineStatus)
                {
                    return;
                }

                _engineStatus = value;
                IsDirty = true;
                OnPropertyChanged("EngineStatus");
            }
        }

        public string NavWatchLevel
        {
            get
            {
                return _navWatchLevel;
            }
            set
            {
                if (value == _navWatchLevel)
                {
                    return;
                }

                _navWatchLevel = value;
                IsDirty = true;
                OnPropertyChanged("NavWatchLevel");
            }
        }

        public string PosFixMethod
        {
            get
            {
                return _posFixMethod;
            }
            set
            {
                if (value == _posFixMethod)
                {
                    return;
                }

                _posFixMethod = value;
                IsDirty = true;
                OnPropertyChanged("PosFixMethod");
            }
        }

        public string ListOfLights_Volume
        {
            get
            {
                return _listOfLights_Volume;
            }
            set
            {
                if (value == _listOfLights_Volume)
                {
                    return;
                }

                _listOfLights_Volume = value;
                IsDirty = true;
                OnPropertyChanged("ListOfLights_Volume");
            }
        }

        public string ListOfLights_Page
        {
            get
            {
                return _listOfLights_Page;
            }
            set
            {
                if (value == _listOfLights_Page)
                {
                    return;
                }

                _listOfLights_Page = value;
                IsDirty = true;
                OnPropertyChanged("ListOfLights_Page");
            }
        }

        public string ListOfRadioSignals_Volume
        {
            get
            {
                return _listOfRadioSignals_Volume;
            }
            set
            {
                if (value == _listOfRadioSignals_Volume)
                {
                    return;
                }

                _listOfRadioSignals_Volume = value;
                IsDirty = true;
                OnPropertyChanged("ListOfRadioSignals_Volume");
            }
        }

        public string ListOfRadioSignals_Page
        {
            get
            {
                return _listOfRadioSignals_Page;
            }
            set
            {
                if (value == _listOfRadioSignals_Page)
                {
                    return;
                }

                _listOfRadioSignals_Page = value;
                IsDirty = true;
                OnPropertyChanged("ListOfRadioSignals_Page");
            }
        }

        public string SailingDirections_Volume
        {
            get
            {
                return _sailingDirections_Volume;
            }
            set
            {
                if (value == _sailingDirections_Volume)
                {
                    return;
                }

                _sailingDirections_Volume = value;
                IsDirty = true;
                OnPropertyChanged("SailingDirections_Volume");
            }
        }

        public string SailingDirections_Page
        {
            get
            {
                return _sailingDirections_Page;
            }
            set
            {
                if (value == _sailingDirections_Page)
                {
                    return;
                }

                _sailingDirections_Page = value;
                IsDirty = true;
                OnPropertyChanged("SailingDirections_Page");
            }
        }

        public string NavtexChannels
        {
            get
            {
                return _navtexChannels;
            }
            set
            {
                if (value == _navtexChannels)
                {
                    return;
                }

                _navtexChannels = value;
                IsDirty = true;
                OnPropertyChanged("NavtexChannels");
            }
        }

        public string ReportTo
        {
            get
            {
                return _reportTo;
            }
            set
            {
                if (value == _reportTo)
                {
                    return;
                }

                _reportTo = value;
                IsDirty = true;
                OnPropertyChanged("ReportTo");
            }
        }

        public string ChannelOrTelephoneNo
        {
            get
            {
                return _channelOrTelephoneNo;
            }
            set
            {
                if (value == _channelOrTelephoneNo)
                {
                    return;
                }

                _channelOrTelephoneNo = value;
                IsDirty = true;
                OnPropertyChanged("ChannelOrTelephoneNo");
            }
        }

        public string ActualPassingTime
        {
            get
            {
                return _actualPassingTime;
            }
            set
            {
                if (value == _actualPassingTime)
                {
                    return;
                }

                _actualPassingTime = value;
                IsDirty = true;
                OnPropertyChanged("ActualPassingTime");
            }
        }

        public string SecurityLevel
        {
            get
            {
                return _securityLevel;
            }
            set
            {
                if (value == _securityLevel)
                {
                    return;
                }

                _securityLevel = value;
                IsDirty = true;
                OnPropertyChanged("SecurityLevel");
            }
        }

        public string WaypointNo
        {
            get
            {
                return _waypointNo;
            }
            set
            {
                _waypointNo = value;
                SetToolTip();
                OnPropertyChanged("WaypointNo");
            }
        }

        public string WaypointName
        {
            get
            {
                return _waypointName;
            }
            set
            {
                _waypointName = value;
                SetToolTip();
                IsDirty = true;
                OnPropertyChanged("WaypointName");
            }
        }

        [XmlIgnore]
        public string WaypointNoAndName
        {
            get
            {
                _waypointNoAndName = "WP " + _waypointNo + ":  " + _waypointName;
                return _waypointNoAndName;
            }
            set
            {
                _waypointNoAndName = value;
                OnPropertyChanged("WaypointNoAndName");
            }
        }

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
                SetToolTip();
                IsDirty = true;
                OnPropertyChanged("Latitude");
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
                SetToolTip();
                IsDirty = true;
                OnPropertyChanged("Longitude");
            } 
        }

        /// <summary>
        /// The waypoints in the map on page RouteOverviewUC is bound to LongLat.
        /// They have to be of type Point.
        /// </summary>
        [XmlIgnore]
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

        [XmlIgnore]
        public string ToolTip
        {
            get
            {  
                return _toolTip;
            }
            set 
            {
                _toolTip = value;
                OnPropertyChanged("ToolTip");
            }
        }

        public TimeSpan TimeToNextWaypoint
        {
            get
            {
                return _timeToNextWaypoint;
            }
            set
            {
                _timeToNextWaypoint = value;
                OnPropertyChanged("TimeToNextWaypoint");
            }
        }

        public string TimeToNextWaypointAsString
        {
            get
            {
                // Return like "32 d 23:45";
                string hoursAndMinutes = _timeToNextWaypoint.Hours.ToString("D2") + ":" + _timeToNextWaypoint.Minutes.ToString("D2");
                if (_timeToNextWaypoint.Days == 0)
                {
                    return hoursAndMinutes;
                }
                else
                {
                    return _timeToNextWaypoint.Days.ToString() + " d " + hoursAndMinutes;
                }
            }

        }

        public TimeSpan TimeToGoToDestination
        {
            get
            {
                return _timeToGoToDestination;
            }
            set
            {
                _timeToGoToDestination = value;
                OnPropertyChanged("TimeToGoToDestination");

                string hoursAndMinutes = _timeToGoToDestination.Hours.ToString("D2") + ":" + _timeToGoToDestination.Minutes.ToString("D2");
                if (_timeToGoToDestination.Days == 0)
                {
                    TimeToGoToDestinationAsString = hoursAndMinutes;
                }
                else
                {
                    TimeToGoToDestinationAsString = _timeToGoToDestination.Days.ToString() + " d " + hoursAndMinutes;
                }
            }
        }

        public string TimeToGoToDestinationAsString
        {             
            get
            {
                // Return like "32 d 23:45";
                string hoursAndMinutes = _timeToGoToDestination.Hours.ToString("D2") + ":" + _timeToGoToDestination.Minutes.ToString("D2");
                if (_timeToGoToDestination.Days == 0)
                {
                    return hoursAndMinutes;
                }
                else
                {
                    return _timeToGoToDestination.Days.ToString() + " d " + hoursAndMinutes;
                }
            }
            set
            {
                _timeToGoToDestinationAsString = value;
                OnPropertyChanged("TimeToGoToDestinationAsString");
            }
        }

        public TimeSpan SailingTimeToThisWaypoint
        {
            get
            {
                return _sailingTimeToThisWaypoint;
            }
            set
            {
                _sailingTimeToThisWaypoint = value;
                OnPropertyChanged("SailingTimeToThisWaypoint");
            }
        }

        public double DistanceSailed
        {
            get
            {
                return _distanceSailed;
            }
            set
            {
                _distanceSailed = value;
                OnPropertyChanged("DistanceSailed");
            }
        }

        public double DistanceToGoToDestination
        {
            get
            {
                return _distanceToGoToDestination;
            }
            set
            {
                _distanceToGoToDestination = value;
                OnPropertyChanged("DistanceToGoToDestination");
            }
        }

        [XmlIgnore]
        public SquatUkcItem SquatUkc
        {
            get
            {
                if (_squatUkc == null && LegSpeedSetting != null)
                {
                    _squatUkc = new SquatUkcItem((double)LegSpeedSetting, MinDepth, 25.0);
                }
                return _squatUkc;
            }
            set
            {
                _squatUkc = value;
                OnPropertyChanged("SquatUkc");
            }
        }

        private void SetToolTip()
        {
            string positionString = PositionValueConverter.LatitudeDoubleToString(Latitude) + "\n" + PositionValueConverter.LongitudeDoubleToString(Longitude);
            ToolTip = "Waypoint " + WaypointNo + "\n" + (WaypointName != null && WaypointName.Length > 0 ? WaypointName + "\n" + positionString : positionString); 
        }


        protected virtual void OnPropertyChanged(string name)
        {
            PropertyChangedEventHandler handler = System.Threading.Interlocked.CompareExchange(ref PropertyChanged, null, null);
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(name));
            }
        }

        public object Clone()
        {
            Waypoint wp = (Waypoint)this.MemberwiseClone();
            wp.LongLat = new Point(this.LongLat.X, this.LongLat.Y);
            return (object)wp;
        }

    }
}
