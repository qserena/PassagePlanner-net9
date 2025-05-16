using C1.WPF.Maps;
using GalaSoft.MvvmLight;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Media;
using System.Collections.Specialized;
using GalaSoft.MvvmLight.Command;
using System.Collections;
using System.Threading.Tasks;
using System.Windows.Threading;
using System.ComponentModel;
using System.Threading;
using System.Xml.Serialization;
using System.IO;
using Seaware.Navigation;
using System.Windows.Controls;
using EcdisLayer;
using System.Diagnostics;
using System.Reflection;
using System.Globalization;
using System.Xml;


namespace PassagePlanner
{
    /// <summary>
    /// This class contains properties that a View can data bind to.
    /// <para>
    /// See http://www.galasoft.ch/mvvm
    /// </para>
    /// </summary>
    public class RouteViewModel : ViewModelBase, IDisposable
    {
        //private readonly IRouteService _routeService;
        private Route _route = new Route();
        private bool _isDirty = false;

        private bool _isDateLineCrossed = false;
        private bool _isDateLineExtraPointsCalculated = false;

        public const int NUMBER_OF_WAYPOINTS_IN_REPORT_FIRST_PAGE = 20;
        public const int NUMBER_OF_WAYPOINTS_IN_REPORT_FOLLOWING_PAGES = 30;
        public const int NUMBER_OF_WAYPOINTS_IN_CHARTS_AND_PUBLICATIONS_PAGE = 18; // 18 waypoints will fit even if every waypoint occupies two rows

        public const string PassagePlannerName = "Passage Planner";

        public const string FilePathPropertyName = "FilePath";
        public const string DeparturePortPropertyName = "DeparturePort";
        public const string ArrivalPortPropertyName = "ArrivalPort";
        public const string TimeZoneDeparturePropertyName = "TimeZoneDeparture";
        public const string TimeZoneArrivalPropertyName = "TimeZoneArrival";
        public const string WaypointsPropertyName = "Waypoints";
        public const string ReportGeneralNotes2_RescueCoordinatingCentersPropertyName = "ReportGeneralNotes2_RescueCoordinatingCenters";
        public const string MapSourcePropertyName = "MapSource";
        public const string MapCenterPropertyName = "MapCenter";
        public const string MapZoomPropertyName = "MapZoom";
        public const string PolylinePropertyName = "Polyline";
        public const string NoOfWaypointsPropertyName = "NoOfWaypoints";

        private bool _isLoading;
        private string _filePath = string.Empty;
        private string _fileName = string.Empty;
        private PassagePlanManuallyAddedTexts _passagePlanTexts = null;
        private string _departurePort = string.Empty;
        private string _arrivalPort = string.Empty;
        private DateTime? _departureTimeLocal;
        private DateTime? _departureTimeDateLocal;
        private DateTime? _departureTimeHoursMinutesLocal;
        private DateTime? _departureTimeUtc;
        private DateTime? _arrivalTimeUtc;
        private DateTime? _arrivalTimeLocal;
        private TimeSpan _totalSailingTime;
        private double? _totalDistance;
        private string _totalSailingTimeHoursMinutes;
        private double _draughtDepartureFore;
        private double _draughtDepartureAft;
        private double _draughtDepartureAir;
        private double _draughtArrivalFore;
        private double _draughtArrivalAft;
        private double _draughtArrivalAir;
        private double _draughtDepartureDeepest;
        private double _draughtArrivalDeepest;
        private double _draughtDeepest;
        private double? _averageSpeed;
        private string _routeName = string.Empty;
        private double _timeZoneDeparture = 0;
        private double _timeZoneArrival = 0;
        private DispatchingObservableCollection<Waypoint> _waypoints = new DispatchingObservableCollection<Waypoint>();
        private DispatchingObservableCollection<Point> _polylinePoints = new DispatchingObservableCollection<Point>();
        private DispatchingObservableCollection<Point> _polylinePoints2 = new DispatchingObservableCollection<Point>();
        private DispatchingObservableCollection<DispatchingObservableCollection<Waypoint>> _reportPages_Waypoints = new DispatchingObservableCollection<DispatchingObservableCollection<Waypoint>>();
        private DispatchingObservableCollection<DispatchingObservableCollection<Waypoint>> _reportPages_Waypoints_ChartsAndPublications = new DispatchingObservableCollection<DispatchingObservableCollection<Waypoint>>();

        private DispatchingObservableCollection<RescueCoordinatingCenter> _reportGeneralNotes2_RescueCoordinatingCenters = 
            new DispatchingObservableCollection<RescueCoordinatingCenter>() 
            {   new RescueCoordinatingCenter(string.Empty, string.Empty, string.Empty),
                new RescueCoordinatingCenter(string.Empty, string.Empty, string.Empty),
                new RescueCoordinatingCenter(string.Empty, string.Empty, string.Empty),
                new RescueCoordinatingCenter(string.Empty, string.Empty, string.Empty),
                new RescueCoordinatingCenter(string.Empty, string.Empty, string.Empty) };

        private C1.WPF.Maps.MultiScaleTileSource _mapSource = null;
        private Point _mapCenter;
        private double _mapZoom = 1.5;
        private int _noOfWaypoints = 0;
        //private double _longMin = Double.MaxValue;
        //private double _longMax = Double.MinValue;
        //private double _latMin = Double.MaxValue;
        //private double _latMax = Double.MinValue;
        //private double _maxDiff = 0.0;
        private Waypoint _selectedItemInGrid;
        private List<Waypoint> _selectedItemsInGrid = new List<Waypoint>();
        private Waypoint _selectedSospWaypoint;
        private Waypoint _selectedEospWaypoint;
        private string _statusText = string.Empty;

        private double? _distanceSailedToSosp;
        private double? _distanceSospToEosp;
        private double? _distanceEospToArrival;
        private double? _averageSpeedToSosp;
        private double? _averageSpeedSospToEosp;
        private double? _averageSpeedEospToArrival;


        public RelayCommand OpenRouteCommand { get; private set; }
        public RelayCommand CloseRouteCommand { get; private set; }
        public RelayCommand AppendRouteCommand { get; private set; }
        public RelayCommand SaveRouteCommand { get; private set; }
        public RelayCommand SaveRouteAsCommand { get; private set; }
        public RelayCommand CopyWaypointCommand { get; private set; }
        public RelayCommand InsertCopiedWaypointBeforeCommand { get; private set; }
        public RelayCommand InsertCopiedWaypointAfterCommand { get; private set; }
        public RelayCommand InsertNewWaypointBeforeCommand { get; private set; }
        public RelayCommand InsertNewWaypointAfterCommand { get; private set; }
        public RelayCommand<Waypoint> DeleteWaypointCommand { get; private set; }
        public RelayCommand ClearPassageDataCommand { get; private set; }
        public RelayCommand ClearPassagePlanTextsCommand { get; private set; }
        public RelayCommand OpenTotalTideCommand { get; private set; }
        public RelayCommand ExportToSwxCommand { get; private set; }
        public RelayCommand ReverseRouteCommand { get; private set; }

        private static readonly XmlSerializer _activeRouteSerializer = new XmlSerializer(typeof(activeRoute));
             

        /// <summary>
        /// Initializes a new instance of the RouteEditUCViewModel class.
        /// </summary>
        public RouteViewModel()
        {
            DepartureTimeHoursMinutesLocal = new DateTime(0);

            OpenRouteCommand = new RelayCommand(OpenRoute);
            CloseRouteCommand = new RelayCommand(CloseRoute);
            AppendRouteCommand = new RelayCommand(AppendRoute);
            SaveRouteCommand = new RelayCommand(SaveRoute);
            SaveRouteAsCommand = new RelayCommand(SaveRouteAs);
            CopyWaypointCommand = new RelayCommand(CopyWaypoint);
            InsertCopiedWaypointBeforeCommand = new RelayCommand(InsertCopiedWaypointBefore);
            InsertCopiedWaypointAfterCommand = new RelayCommand(InsertCopiedWaypointAfter);
            InsertNewWaypointBeforeCommand = new RelayCommand(InsertNewWaypointBefore);
            InsertNewWaypointAfterCommand = new RelayCommand(InsertNewWaypointAfter);
            DeleteWaypointCommand = new RelayCommand<Waypoint>(DeleteWaypoint);
            ClearPassageDataCommand = new RelayCommand(ClearPassageData);
            ClearPassagePlanTextsCommand = new RelayCommand(ClearPassagePlanTexts);
            OpenTotalTideCommand = new RelayCommand(OpenTotalTide);
            ExportToSwxCommand = new RelayCommand(ExportToSwxFile);
            ReverseRouteCommand = new RelayCommand(ReverseRoute);
           

            PassagePlanTexts = new PassagePlanManuallyAddedTexts();

            if (MapSource == null)
            {
                MapSource = new LocalMapSource();
                MapZoom = 1.5;
                MapCenter = new Point(0.0, 15.0);
            }

            IsDirty = false;
        }



        public bool IsDirty
        {
            get
            {
                return _isDirty || Waypoint.IsDirty || PassagePlanTexts.IsDirty;
            }

            set
            {
                _isDirty = value;
                if (value == false)
                {
                    Waypoint.IsDirty = false;
                    PassagePlanTexts.IsDirty = false;
                }
                RaisePropertyChanged("IsDirty");
            }
        }

        public bool IsLoading
        {
            get
            {
                return _isLoading;
            }

            set
            {
                if (_isLoading == value)
                {
                    return;
                }

                _isLoading = value;
                RaisePropertyChanged("IsLoading");
            }
        }


        public Waypoint SelectedItemInGrid
        {
            get 
            { 
                return _selectedItemInGrid; 
            }
            set 
            { 
                _selectedItemInGrid = value;
                RaisePropertyChanged("SelectedItemInGrid");
            }
        }

        public List<Waypoint> SelectedItemsInGrid
        {
            get { return _selectedItemsInGrid; }
            set { _selectedItemsInGrid = value; }
        }

        /// <summary>
        /// Sosp = Start of Sea Passage
        /// </summary>
        public Waypoint SelectedSospWaypoint
        {
            get
            {
                return _selectedSospWaypoint;
            }
            set
            {
                if (value != null && _selectedEospWaypoint != null &&
                    Waypoints.IndexOf(value) > Waypoints.IndexOf(_selectedEospWaypoint))
                {
                    MessageBox mb = new MessageBox("Start of Sea passage must be \nbefore End of Sea passage!", "Validation Error", MessageBoxButton.OK);
                    mb.ShowDialog();
                }
                else
                {
                    _selectedSospWaypoint = value;
                    RaisePropertyChanged("SelectedSospWaypoint");

                    CalculateDistanceAndSpeedToSosp();
                    CalculateDistanceAndSpeedSospToEosp();
                }
            }
        }

        /// <summary>
        /// Eosp = End of Sea Passage
        /// </summary>
        public Waypoint SelectedEospWaypoint
        {
            get
            {
                return _selectedEospWaypoint;
            }
            set
            {
                if (_selectedSospWaypoint != null && value != null &&
                   Waypoints.IndexOf(_selectedSospWaypoint) > Waypoints.IndexOf(value))
                {
                    MessageBox mb = new MessageBox("Start of Sea passage must be \nbefore End of Sea passage!", "Validation Error", MessageBoxButton.OK);
                    mb.ShowDialog();
                }
                else
                {
                    _selectedEospWaypoint = value;
                    RaisePropertyChanged("SelectedEospWaypoint");

                    CalculateDistanceAndSpeedSospToEosp();
                    CalculateDistanceAndSpeedEospToArrival();
                }
            }
        }

        public double? DistanceSailedToSosp
        {
            get
            {
                return _distanceSailedToSosp;
            }
            set
            {
                if (_distanceSailedToSosp == value)
                {
                    return;
                }

                _distanceSailedToSosp = value;
                RaisePropertyChanged("DistanceSailedToSosp");
            }
        }

        public double? DistanceSospToEosp
        {
            get
            {
                return _distanceSospToEosp;
            }
            set
            {
                if (_distanceSospToEosp == value)
                {
                    return;
                }

                _distanceSospToEosp = value;
                RaisePropertyChanged("DistanceSospToEosp");
            }
        }

        public double? DistanceEospToArrival
        {
            get
            {
                return _distanceEospToArrival;
            }
            set
            {
                if (_distanceEospToArrival == value)
                {
                    return;
                }

                _distanceEospToArrival = value;
                RaisePropertyChanged("DistanceEospToArrival");
            }
        }

        public double? AverageSpeedToSosp
        {
            get
            {
                return _averageSpeedToSosp;
            }
            set
            {
                if (_averageSpeedToSosp == value)
                {
                    return;
                }

                _averageSpeedToSosp = value;
                RaisePropertyChanged("AverageSpeedToSosp");
            }
        }

        public double? AverageSpeedSospToEosp
        {
            get
            {
                return _averageSpeedSospToEosp;
            }
            set
            {
                if (_averageSpeedSospToEosp == value)
                {
                    return;
                }

                _averageSpeedSospToEosp = value;
                RaisePropertyChanged("AverageSpeedSospToEosp");
            }
        }

        public double? AverageSpeedEospToArrival
        {
            get
            {
                return _averageSpeedEospToArrival;
            }
            set
            {
                if (_averageSpeedEospToArrival == value)
                {
                    return;
                }

                _averageSpeedEospToArrival = value;
                RaisePropertyChanged("AverageSpeedEospToArrival");
            }
        }

        public bool IgnoreSelectedItemsDuringOperation { get; set; }
        

        public string FilePath
        {
            get
            {
                return _filePath;
            }

            set
            {
                if (_filePath == value)
                {
                    return;
                }

                _filePath = value;
                RaisePropertyChanged(FilePathPropertyName);
                FileName = Path.GetFileName(FilePath);
            }
        }

        public string FileName
        {
            get
            {
                return _fileName;
            }

            set
            {
                if (_fileName == value)
                {
                    return;
                }

                _fileName = value;
                RaisePropertyChanged("FileName");
            }
        }

        public int NoOfWaypoints
        {
            get
            {
                return _noOfWaypoints;
            }

            set
            {
                if (_noOfWaypoints == value)
                {
                    return;
                }

                _noOfWaypoints = value;
                RaisePropertyChanged(NoOfWaypointsPropertyName);
            }
        }

        public PassagePlanManuallyAddedTexts PassagePlanTexts
        {
            get
            {
                return _passagePlanTexts;
            }

            set
            {
                _passagePlanTexts = value;
                RaisePropertyChanged("PassagePlanTexts");

                IsDirty = true;
            }
        }

        /// <summary>
        /// Gets the DeparturePort property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public string DeparturePort
        {
            get
            {
                return _departurePort;
            }

            set
            {
                if (_departurePort == value)
                {
                    return;
                }

                _departurePort = value;
                _route.DeparturePort = _departurePort;
                RaisePropertyChanged(DeparturePortPropertyName);

                SetRouteName();

                IsDirty = true;
            }
        }

        public string ArrivalPort
        {
            get
            {
                return _arrivalPort;
            }

            set
            {
                if (_arrivalPort == value)
                {
                    return;
                }

                _arrivalPort = value;
                _route.ArrivalPort = _arrivalPort;
                
                SetRouteName();
                IsDirty = true;
                RaisePropertyChanged(ArrivalPortPropertyName);
            }
        }

        public DateTime? DepartureTimeLocal
        {
            get
            {
                return _departureTimeLocal;
            }

            set
            {
                if (_departureTimeLocal == value)
                {
                    return;
                }

                _departureTimeLocal = value;

                DepartureTimeUtc = (value != null ? (DateTime?)((DateTime)DepartureTimeLocal).AddHours(TimeZoneDeparture) : null);

                IsDirty = true;
                RaisePropertyChanged("DepartureTimeLocal");
            }
        }

        /// <summary>
        /// DepartureTimeDateLocal is the date part of DepartureTimeLocal
        /// </summary>
        public DateTime? DepartureTimeDateLocal
        {
            get
            {
                return _departureTimeDateLocal;
            }

            set
            {
                if (_departureTimeDateLocal == value)
                {
                    return;
                }

                _departureTimeDateLocal = value;

                if (_departureTimeDateLocal == null)
                {
                    DepartureTimeHoursMinutesLocal = DateTime.MinValue;
                    DepartureTimeLocal = null;
                }
                else if (_departureTimeDateLocal == DateTime.MinValue)
                {
                    DepartureTimeLocal = null;
                }
                else  if (DepartureTimeDateLocal > DateTime.MinValue && DepartureTimeHoursMinutesLocal != null)
                {
                    // Concatinate DepartureTimeDateLocal and DepartureTimeHoursMinutesLocal to calculate DepartureTimeLocal
                    int hours = ((DateTime)DepartureTimeHoursMinutesLocal).Hour;
                    int minutes = ((DateTime)DepartureTimeHoursMinutesLocal).Minute;
                    TimeSpan timeSpan = new TimeSpan(hours, minutes, 0);
                    DepartureTimeLocal = ((DateTime)DepartureTimeDateLocal).Add(timeSpan);
                }

                IsDirty = true;
                RaisePropertyChanged("DepartureTimeDateLocal");
            }
        }

        /// <summary>
        /// DepartureTimeHoursMinutesLocal is the time part of DepartureTimeLocal
        /// </summary>
        public DateTime? DepartureTimeHoursMinutesLocal
        {
            get
            {
                if (_departureTimeHoursMinutesLocal == null)
                {
                    _departureTimeHoursMinutesLocal = new DateTime(0);
                }
                return _departureTimeHoursMinutesLocal;
            }

            set
            {
                if (_departureTimeHoursMinutesLocal == value)
                {
                    return;
                }

                if (value == null)
                {
                    _departureTimeHoursMinutesLocal = new DateTime(0);
                }

                _departureTimeHoursMinutesLocal = value;

                if (DepartureTimeDateLocal > DateTime.MinValue && DepartureTimeHoursMinutesLocal != null)
                {
                    // Concatinate DepartureTimeDateLocal and DepartureTimeHoursMinutesLocal to calculate DepartureTimeLocal
                    int hours = ((DateTime)DepartureTimeHoursMinutesLocal).Hour;
                    int minutes = ((DateTime)DepartureTimeHoursMinutesLocal).Minute;
                    TimeSpan timeSpan = new TimeSpan(hours, minutes, 0);
                    DepartureTimeLocal = ((DateTime)DepartureTimeDateLocal).Add(timeSpan);
                }

                IsDirty = true;
                RaisePropertyChanged("DepartureTimeHoursMinutesLocal");
            }
        }

        public DateTime? DepartureTimeUtc
        {
            get
            {
                return _departureTimeUtc;
            }

            set
            {
                if (_departureTimeUtc == value)
                {
                    return;
                }

                _departureTimeUtc = value;
                UpdateWaypointRelatedStuff(null);
                RaisePropertyChanged("DepartureTimeUtc");
            }
        }

        public DateTime? ArrivalTimeUtc
        {
            get
            {
                return _arrivalTimeUtc;
            }

            set
            {
                if (_arrivalTimeUtc == value)
                {
                    return;
                }

                _arrivalTimeUtc = value;
                
                ArrivalTimeLocal = (value != null ? (DateTime?)((DateTime)ArrivalTimeUtc).AddHours(-TimeZoneArrival) : null);
                RaisePropertyChanged("ArrivalTimeUtc");
            }
        }

        public DateTime? ArrivalTimeLocal
        {
            get
            {
                return _arrivalTimeLocal;
            }

            set
            {
                if (_arrivalTimeLocal == value)
                {
                    return;
                }

                _arrivalTimeLocal = value;
                RaisePropertyChanged("ArrivalTimeLocal");
            }
        }

        /// <summary>
        /// Total distance in [NM]
        /// </summary>
        public double? TotalDistance
        {
            get
            {
                return _totalDistance;
            }
            set
            {
                _totalDistance = value;
                CalculateAverageSpeed();
                RaisePropertyChanged("TotalDistance");
            }
        }

        public TimeSpan TotalSailingTime 
        {
            get
            {
                return _totalSailingTime;
            }
            set
            {
                _totalSailingTime = value;
                TotalSailingTimeHoursMinutes = TotalSailingTime.Hours.ToString() + ":" + TotalSailingTime.Minutes.ToString("00");
                CalculateAverageSpeed();
                RaisePropertyChanged("TotalSailingTime");
            }
        }

        public double DraughtDepartureFore
        {
            get
            {
                return _draughtDepartureFore;
            }
            set
            {
                if (_draughtDepartureFore == value)
                {
                    return;
                }
                _draughtDepartureFore = value;
                RaisePropertyChanged("DraughtDepartureFore");
                DraughtDepartureDeepest = Math.Max(_draughtDepartureFore, _draughtDepartureAft);
                DraughtDeepest = Math.Max(_draughtDepartureDeepest, _draughtArrivalDeepest);
                IsDirty = true;

                UpdateWaypointRelatedStuff(null);
            }
        }

        public double DraughtDepartureAft
        {
            get
            {
                return _draughtDepartureAft;
            }
            set
            {
                if (_draughtDepartureAft == value)
                {
                    return;
                }
                _draughtDepartureAft = value;
                RaisePropertyChanged("DraughtDepartureAft");
                IsDirty = true;
                DraughtDepartureDeepest = Math.Max(_draughtDepartureFore, _draughtDepartureAft);
                DraughtDeepest = Math.Max(_draughtDepartureDeepest, _draughtArrivalDeepest);
                
                UpdateWaypointRelatedStuff(null);
                
            }
        }

        public double DraughtDepartureAir
        {
            get
            {
                return _draughtDepartureAir;
            }
            set
            {
                if (_draughtDepartureAir == value)
                {
                    return;
                }
                _draughtDepartureAir = value;
                IsDirty = true;
                RaisePropertyChanged("DraughtDepartureAir");
            }
        }

        public double DraughtArrivalFore
        {
            get
            {
                return _draughtArrivalFore;
            }
            set
            {
                if (_draughtArrivalFore == value)
                {
                    return;
                }
                _draughtArrivalFore = value;
                DraughtArrivalDeepest = Math.Max(_draughtArrivalFore, _draughtArrivalAft);
                DraughtDeepest = Math.Max(_draughtDepartureDeepest, _draughtArrivalDeepest);
                IsDirty = true;
                RaisePropertyChanged("DraughtArrivalFore");
                
            }
        }

        public double DraughtArrivalAft
        {
            get
            {
                return _draughtArrivalAft;
            }
            set
            {
                if (_draughtArrivalAft == value)
                {
                    return;
                }
                _draughtArrivalAft = value;
                DraughtArrivalDeepest = Math.Max(_draughtArrivalFore, _draughtArrivalAft);
                DraughtDeepest = Math.Max(_draughtDepartureDeepest, _draughtArrivalDeepest);
                IsDirty = true;
                RaisePropertyChanged("DraughtArrivalAft");
                
            }
        }

        public double DraughtArrivalAir
        {
            get
            {
                return _draughtArrivalAir;
            }
            set
            {
                if (_draughtArrivalAir == value)
                {
                    return;
                }
                _draughtArrivalAir = value;
                IsDirty = true;
                RaisePropertyChanged("DraughtArrivalAir");
            }
        }

        public double DraughtArrivalDeepest
        {
            get
            {
                return _draughtArrivalDeepest;
            }
            set
            {
                if (_draughtArrivalDeepest == value)
                {
                    return;
                }
                _draughtArrivalDeepest = value;
                RaisePropertyChanged("DraughtArrivalDeepest");
            }
        }

        public double DraughtDepartureDeepest
        {
            get
            {
                return _draughtDepartureDeepest;
            }
            set
            {
                if (_draughtDepartureDeepest == value)
                {
                    return;
                }
                _draughtDepartureDeepest = value;
                RaisePropertyChanged("DraughtDepartureDeepest");
            }
        }

        public double DraughtDeepest
        {
            get
            {
                return _draughtDeepest;
            }
            set
            {
                if (_draughtDeepest == value)
                {
                    return;
                }
                _draughtDeepest = value;
                RaisePropertyChanged("DraughtDeepest");
            }
        }

        private void CalculateAverageSpeed()
        {
            if (TotalDistance != null && TotalSailingTime != null && TotalSailingTime.TotalHours != 0.0)
            {
                AverageSpeed = (double)TotalDistance / Convert.ToDouble((TotalSailingTime).TotalHours);
            }
        }

        private void CalculateDistanceAndSpeedToSosp()
        {
            if (SelectedSospWaypoint != null)
            {
                DistanceSailedToSosp = SelectedSospWaypoint.DistanceSailed;

                if (SelectedSospWaypoint.DistanceSailed > 0.0 && SelectedSospWaypoint.SailingTimeToThisWaypoint != null && SelectedSospWaypoint.SailingTimeToThisWaypoint.TotalHours > 0.0)
                {
                    AverageSpeedToSosp = SelectedSospWaypoint.DistanceSailed / SelectedSospWaypoint.SailingTimeToThisWaypoint.TotalHours;
                }
                else
                {
                    AverageSpeedToSosp = 0.0;
                }
            }
            else
            {
                DistanceSailedToSosp = null;
                AverageSpeedToSosp = null;
            }
        }

        private void CalculateDistanceAndSpeedSospToEosp()
        {
            if (SelectedSospWaypoint != null && SelectedSospWaypoint.SailingTimeToThisWaypoint != null && 
                SelectedEospWaypoint != null && SelectedEospWaypoint.SailingTimeToThisWaypoint != null &&
                SelectedEospWaypoint.SailingTimeToThisWaypoint > SelectedSospWaypoint.SailingTimeToThisWaypoint)
            {
                DistanceSospToEosp = SelectedEospWaypoint.DistanceSailed - SelectedSospWaypoint.DistanceSailed;
                AverageSpeedSospToEosp = DistanceSospToEosp / SelectedEospWaypoint.SailingTimeToThisWaypoint.Add(-SelectedSospWaypoint.SailingTimeToThisWaypoint).TotalHours;
            }
            else
            {
                DistanceSospToEosp = null;
                AverageSpeedSospToEosp = null;
            }
        }

        private void CalculateDistanceAndSpeedEospToArrival()
        {
            if (SelectedEospWaypoint != null)
            {
                DistanceEospToArrival = TotalDistance - SelectedEospWaypoint.DistanceSailed;

                if (SelectedEospWaypoint.SailingTimeToThisWaypoint != null && TotalSailingTime.TotalHours > SelectedEospWaypoint.SailingTimeToThisWaypoint.TotalHours)
                {
                    AverageSpeedEospToArrival = DistanceEospToArrival / TotalSailingTime.Add(-SelectedEospWaypoint.SailingTimeToThisWaypoint).TotalHours;
                }
                else
                {
                    AverageSpeedEospToArrival = 0.0;
                }
            }
            else
            {
                DistanceEospToArrival = null;
                AverageSpeedEospToArrival = null;
            }
        }

        public double? AverageSpeed
        {
            get
            {
                return _averageSpeed;
            }
            set
            {
                _averageSpeed = value;
                RaisePropertyChanged("AverageSpeed");
            }
        }

        /// <summary>
        /// String only for display the hours and minutes part of TotalSailingTime in format hh:mm
        /// </summary>
        public string TotalSailingTimeHoursMinutes
        {
            get
            {
                return _totalSailingTimeHoursMinutes;
            }
            set
            {
                _totalSailingTimeHoursMinutes = value;
                RaisePropertyChanged("TotalSailingTimeHoursMinutes");
            }
        }

        public string RouteName
        {
            get
            {
                return _routeName;
            }

            set
            {
                if (_routeName == value)
                {
                    return;
                }
                _routeName = value;
                IsDirty = true;
                RaisePropertyChanged("RouteName");
            }
        }

        public double TimeZoneDeparture
        {
            get
            {
                return _timeZoneDeparture;
            }

            set
            {
                if (_timeZoneDeparture == value)
                {
                    return;
                }
                double oldTimeZoneDeparture = _timeZoneDeparture;

                _timeZoneDeparture = value;
                _route.TimeZoneDeparture = _timeZoneDeparture;

                if (DepartureTimeLocal != null && DepartureTimeUtc != null)
                {
                    // Adjust Departure time UTC
                    double timeZoneChange = _timeZoneDeparture - oldTimeZoneDeparture;
                    DepartureTimeUtc = ((DateTime)DepartureTimeUtc).Add(TimeSpan.FromHours(timeZoneChange));
                }

                IsDirty = true;
                RaisePropertyChanged(TimeZoneDeparturePropertyName);
            }
        }

        public double TimeZoneArrival
        {
            get
            {
                return _timeZoneArrival;
            }

            set
            {
                if (_timeZoneArrival == value)
                {
                    return;
                }
                double oldTimeZoneArrival = _timeZoneArrival;

                _timeZoneArrival = value;
                _route.TimeZoneArrival = _timeZoneArrival;

                if (ArrivalTimeLocal != null && ArrivalTimeUtc != null)
                {
                    // Adjust Arrival time local
                    double timeZoneChange = _timeZoneArrival - oldTimeZoneArrival;
                    ArrivalTimeLocal = ((DateTime)ArrivalTimeLocal).Add(TimeSpan.FromHours(-timeZoneChange));
                }

                IsDirty = true;
                RaisePropertyChanged(TimeZoneArrivalPropertyName);
            }
        }

        public DispatchingObservableCollection<Waypoint> Waypoints
        {
            get
            {
                return _waypoints;
            }

            set
            {
                if (_waypoints == value)
                {
                    return;
                }

                _waypoints = value;
                IsDirty = true;
                RaisePropertyChanged(WaypointsPropertyName);
                RaisePropertyChanged(NoOfWaypointsPropertyName);
            }
        }

        public DispatchingObservableCollection<Point> PolylinePoints
        {
            get
            {
                return _polylinePoints;
            }

            set
            {
                _polylinePoints = value;
                RaisePropertyChanged("PolylinePoints");
            }
        }

        public DispatchingObservableCollection<Point> PolylinePoints2
        {
            get
            {
                return _polylinePoints2;
            }

            set
            {
                _polylinePoints2 = value;
                RaisePropertyChanged("PolylinePoints2");
            }
        }

        public DispatchingObservableCollection<DispatchingObservableCollection<Waypoint>> ReportPages_Waypoints
        {
            get
            {
                return _reportPages_Waypoints;
            }

            set
            {
                if (_reportPages_Waypoints == value)
                {
                    return;
                }

                _reportPages_Waypoints = value;
                RaisePropertyChanged("ReportPages_Waypoints");
            }
        }

        public DispatchingObservableCollection<DispatchingObservableCollection<Waypoint>> ReportPages_Waypoints_ChartsAndPublications
        {
            get
            {
                return _reportPages_Waypoints_ChartsAndPublications;
            }

            set
            {
                if (_reportPages_Waypoints_ChartsAndPublications == value)
                {
                    return;
                }

                _reportPages_Waypoints_ChartsAndPublications = value;
                RaisePropertyChanged("ReportPages_Waypoints_ChartsAndPublications");
            }
        }

        public DispatchingObservableCollection<RescueCoordinatingCenter> ReportGeneralNotes2_RescueCoordinatingCenters
        {
            get
            {
                return _reportGeneralNotes2_RescueCoordinatingCenters;
            }

            set
            {
                if (_reportGeneralNotes2_RescueCoordinatingCenters == value)
                {
                    return;
                }

                _reportGeneralNotes2_RescueCoordinatingCenters = value;
                RaisePropertyChanged(ReportGeneralNotes2_RescueCoordinatingCentersPropertyName);
            }
        }


        public C1.WPF.Maps.MultiScaleTileSource MapSource
        {
            get
            {
                return _mapSource;
            }

            set
            {
                if (_mapSource == value)
                {
                    return;
                }

                _mapSource = value;
                RaisePropertyChanged(MapSourcePropertyName);
            }
        }

        public Point MapCenter
        {
            get
            {
                return _mapCenter;
            }

            set
            {
                if (_mapCenter == value)
                {
                    return;
                }

                _mapCenter = value;
                RaisePropertyChanged(MapCenterPropertyName);
            }
        }

        public double MapZoom
        {
            get
            {
                return _mapZoom;
            }

            set
            {
                if (_mapZoom == value)
                {
                    return;
                }

                // Zoom is limited to the interval 1.5 - 6.0 in order to look good!
                if (value > 1.5 && value < 6.0)
                {
                    _mapZoom = value;
                    RaisePropertyChanged(MapZoomPropertyName);
                }
            }
        }

        public string StatusBarText
        {
            get
            {
                return _statusText;
            }
            set
            {
                _statusText = value;
                RaisePropertyChanged("StatusBarText");
            }
        }

        private void SetRouteName()
        {
            if (_departurePort != null && _departurePort.Length > 0 && _arrivalPort != null && _arrivalPort.Length > 0)
            {
                RouteName = _departurePort + " - " + _arrivalPort;
            }
            else
            {
                RouteName = string.Empty;
            }
        }

        private void CopyWaypoint()
        {
            if (SelectedItemsInGrid != null && SelectedItemsInGrid.Count > 0)
            {
                Clipboard.Clear();

                string statusBarText = string.Empty;

                if (SelectedItemsInGrid.Count == 1)
                {
                    Waypoint wp = (Waypoint)SelectedItemsInGrid[0].Clone();
                    WaypointForClipboard copiedWaypoint = (WaypointForClipboard)wp;
                    Clipboard.SetData("WaypointForClipboard", copiedWaypoint);

                    statusBarText = "1 waypoint was copied to clipboard";
                }
                else
                {
                    List<WaypointForClipboard> copiedWaypoints = new List<WaypointForClipboard>();
                    foreach (Waypoint wp in SelectedItemsInGrid)
                    {
                        copiedWaypoints.Add((WaypointForClipboard)wp);
                        Thread.Sleep(10);
                    }

                    Clipboard.SetData("ListOfWaypointForClipboard", copiedWaypoints);

                    statusBarText = SelectedItemsInGrid.Count.ToString() + " waypoints were copied to clipboard";
                }

                StatusBarText = statusBarText;
            }
        }

        private void InsertCopiedWaypointBefore()
        {
            if (Clipboard.ContainsData("WaypointForClipboard") || Clipboard.ContainsData("ListOfWaypointForClipboard"))
            {
                if (SelectedItemsInGrid != null && SelectedItemsInGrid.Count > 0)
                {
                    if (SelectedItemsInGrid.Count == 1)
                    {
                        int indexOfSelectedWaypoint = Waypoints.IndexOf(SelectedItemsInGrid[0]);

                        // Lock
                        IgnoreSelectedItemsDuringOperation = true;

                        if (Clipboard.ContainsData("WaypointForClipboard"))
                        {
                            WaypointForClipboard copiedWaypoint = Clipboard.GetData("WaypointForClipboard") as WaypointForClipboard;
                            Waypoints.Insert(indexOfSelectedWaypoint, (Waypoint)copiedWaypoint);
                        }
                        else if (Clipboard.ContainsData("ListOfWaypointForClipboard"))
                        {
                            List<WaypointForClipboard> copiedWaypoints = Clipboard.GetData("ListOfWaypointForClipboard") as List<WaypointForClipboard>;
                            foreach (WaypointForClipboard wp in copiedWaypoints)
                            {
                                Waypoints.Insert(indexOfSelectedWaypoint++, (Waypoint)wp);   
                                Thread.Sleep(10);
                            }
                        }

                        // Unlock
                        IgnoreSelectedItemsDuringOperation = false;

                        UpdateWaypointRelatedStuff(null);
                    }
                    else
                    {
                        var messageBox = new MessageBox("Please select ONE waypoint only, \nto define where to insert the waypoint(s)", "Message", MessageBoxButton.OK);
                        bool? result = messageBox.ShowDialog();
                    }
                }
            }
            else
            {
                var messageBox = new MessageBox("Please copy one or more waypoints first!", "Message", MessageBoxButton.OK);
                bool? result = messageBox.ShowDialog();
            }
        }

        private void InsertCopiedWaypointAfter()
        {
            if (Clipboard.ContainsData("WaypointForClipboard") || Clipboard.ContainsData("ListOfWaypointForClipboard"))
            {
                if (SelectedItemsInGrid != null && SelectedItemsInGrid.Count > 0)
                {
                    if (SelectedItemsInGrid.Count == 1)
                    {
                        int indexOfSelectedWaypoint = Waypoints.IndexOf(SelectedItemsInGrid[0]);

                        // Lock
                        IgnoreSelectedItemsDuringOperation = true;

                        if (Clipboard.ContainsData("WaypointForClipboard"))
                        {
                            WaypointForClipboard copiedWaypoint = Clipboard.GetData("WaypointForClipboard") as WaypointForClipboard;
                            Waypoints.Insert(indexOfSelectedWaypoint + 1, (Waypoint)copiedWaypoint);
                        }
                        else if (Clipboard.ContainsData("ListOfWaypointForClipboard"))
                        {
                            List<WaypointForClipboard> copiedWaypoints = Clipboard.GetData("ListOfWaypointForClipboard") as List<WaypointForClipboard>;
                            foreach (WaypointForClipboard wp in copiedWaypoints)
                            {
                                // Check if insert shall be done before the last waypoint
                                if (indexOfSelectedWaypoint < Waypoints.Count - 1)
                                {
                                    Waypoints.Insert(indexOfSelectedWaypoint + 1, (Waypoint)wp);
                                }
                                else
                                {
                                    // Insert after last waypoint
                                    Waypoints.Add((Waypoint)wp);
                                }
                                indexOfSelectedWaypoint++;
                                Thread.Sleep(10);
                            }
                        }

                        // Unlock
                        IgnoreSelectedItemsDuringOperation = false;

                        UpdateWaypointRelatedStuff(null);
                    }
                    else
                    {
                        var messageBox = new MessageBox("Please select ONE waypoint only, \nto define where to insert the waypoint(s)", "Message", MessageBoxButton.OK);
                        bool? result = messageBox.ShowDialog();
                    }
                }
            }
            else
            {
                var messageBox = new MessageBox("Please copy one or more waypoints first!", "Message", MessageBoxButton.OK);
                bool? result = messageBox.ShowDialog();
            }
        }

        private void InsertNewWaypointBefore()
        {
            if (Waypoints.Count == 0)
            {
                Waypoint newWaypoint = new Waypoint();

                // Lock
                IgnoreSelectedItemsDuringOperation = true;

                Waypoints.Add(newWaypoint);

                // Unlock
                IgnoreSelectedItemsDuringOperation = false;

                UpdateWaypointRelatedStuff(null);
            }
            else if (SelectedItemsInGrid != null && SelectedItemsInGrid.Count > 0)
            {
                if (SelectedItemsInGrid.Count == 1)
                {
                    int indexOfSelectedWaypoint = Waypoints.IndexOf(SelectedItemsInGrid[0]);
                    Waypoint newWaypoint = new Waypoint();

                    // Lock
                    IgnoreSelectedItemsDuringOperation = true;

                    Waypoints.Insert(indexOfSelectedWaypoint, newWaypoint);

                    // Unlock
                    IgnoreSelectedItemsDuringOperation = false;

                    UpdateWaypointRelatedStuff(null);
                }
                else
                {
                    var messageBox = new MessageBox("Please select ONE waypoint only!", "Message", MessageBoxButton.OK);
                    bool? result = messageBox.ShowDialog();
                }
            }
            else
            {
                var messageBox = new MessageBox("Please select a waypoint first!", "Message", MessageBoxButton.OK);
                bool? result = messageBox.ShowDialog();
            }
        }

        private void InsertNewWaypointAfter()
        {
            if (Waypoints.Count == 0)
            {
                Waypoint newWaypoint = new Waypoint();

                // Lock
                IgnoreSelectedItemsDuringOperation = true;

                Waypoints.Add(newWaypoint);

                // Unlock
                IgnoreSelectedItemsDuringOperation = false;

                UpdateWaypointRelatedStuff(null);
            }
            else  if (SelectedItemsInGrid != null && SelectedItemsInGrid.Count > 0)
            {
                if (SelectedItemsInGrid.Count == 1)
                {
                    int indexOfSelectedWaypoint = Waypoints.IndexOf(SelectedItemsInGrid[0]);
                    Waypoint newWaypoint = new Waypoint();

                    // Lock
                    IgnoreSelectedItemsDuringOperation = true;

                    Waypoints.Insert(indexOfSelectedWaypoint + 1, newWaypoint);

                    // Unlock
                    IgnoreSelectedItemsDuringOperation = false;

                    UpdateWaypointRelatedStuff(null);
                }
                else
                {
                    var messageBox = new MessageBox("Please select ONE waypoint only!", "Message", MessageBoxButton.OK);
                    bool? result = messageBox.ShowDialog();
                }
            }
            else
            {
                var messageBox = new MessageBox("Please select a waypoint first!", "Message", MessageBoxButton.OK);
                bool? result = messageBox.ShowDialog();
            }
        }

        private void DeleteWaypoint(Waypoint obj)
        {
            if (SelectedItemsInGrid != null && SelectedItemsInGrid.Count > 0)
            {
                string waypointsString = string.Empty;
                string statusBarText = string.Empty;

                if (SelectedItemsInGrid.Count == 1)
                {
                    waypointsString = "waypoint";
                    statusBarText = "1 waypoint was deleted";
                }
                else
                {
                    waypointsString = "waypoints";
                    statusBarText = SelectedItemsInGrid.Count.ToString() + " waypoints were deleted";
                }

                var messageBox = new MessageBox("Do you want to delete selected " + waypointsString + "?", "Delete?", MessageBoxButton.OKCancel, "Delete", string.Empty, string.Empty, ButtonType.Cancel);
                
                bool? result = messageBox.ShowDialog();
                if (result == true)
                {
                    UIServices.SetBusyState();

                    // Lock
                    IgnoreSelectedItemsDuringOperation = true;
                    foreach (var selectedItem in SelectedItemsInGrid)
                    {
                        Waypoints.Remove(selectedItem);
                    }
                    // Unlock
                    IgnoreSelectedItemsDuringOperation = false;

                    UpdateWaypointRelatedStuff(null);

                    // We have to set IsDirty manually here, for some reason.
                    IsDirty = true;

                    if (Waypoints == null || Waypoints.Count < 1)
                    {
                        NoOfWaypoints = 0;
                    }

                    StatusBarText = statusBarText;
                }
            }
            else
            {
                var messageBox = new MessageBox("Please select the waypoint or waypoints to delete!", "Message", MessageBoxButton.OK);
                bool? result = messageBox.ShowDialog();
            }
        }

        public void SetValueForAllWaypoints(string headerText, PropertyPath bindingPath)
        {
            bool setValueOnlyForSelectedWaypoints = false;

            SetValueForAllWaypointsDialog messageBox = null;
            NumberStyles styles = NumberStyles.AllowDecimalPoint | NumberStyles.AllowLeadingSign;

            List<Waypoint> waypointsToBeChanged = null;

            // if 2 or more waypoints are selected => change these selected ones.
            // Otherwise => change ALL waypoints
            if (SelectedItemsInGrid != null && SelectedItemsInGrid.Count > 1)
            {
                waypointsToBeChanged = SelectedItemsInGrid;
                setValueOnlyForSelectedWaypoints = true;
            }
            else
            {
                waypointsToBeChanged = new List<Waypoint>(Waypoints);
                setValueOnlyForSelectedWaypoints = false;
            }

            switch (bindingPath.Path)
            {
                case "LegSpeedSetting":

                    messageBox = new SetValueForAllWaypointsDialog(headerText, ValidationRuleType.SpeedRule, setValueOnlyForSelectedWaypoints);
                    messageBox.ShowDialog();
                    if (messageBox.DialogResult == true)
                    {
                        double result = 0.0;
                        bool success = double.TryParse((string)messageBox.Tag, styles, System.Globalization.NumberFormatInfo.InvariantInfo, out result);
                        if (success)
                        {
                            foreach (Waypoint wp in waypointsToBeChanged)
                            {
                                wp.LegSpeedSetting = result;
                            }
                            UpdateWaypointRelatedStuff(null);
                        }
                    }
                    break;


                case "Remarks":

                    messageBox = new SetValueForAllWaypointsDialog(headerText, ValidationRuleType.NoValidation, setValueOnlyForSelectedWaypoints);
                    messageBox.ShowDialog();
                    if (messageBox.DialogResult == true)
                    {
                        foreach (Waypoint wp in waypointsToBeChanged)
                        {
                            wp.Remarks = (string)messageBox.Tag;
                        }
                        UpdateWaypointRelatedStuff(null);
                    }
                    break;


                case "MinDepth":

                    messageBox = new SetValueForAllWaypointsDialog(headerText, ValidationRuleType.WaterDepthRule, setValueOnlyForSelectedWaypoints);
                    messageBox.ShowDialog();
                    if (messageBox.DialogResult == true)
                    {
                        double result = 0.0;
                        bool success = double.TryParse((string)messageBox.Tag, styles, System.Globalization.NumberFormatInfo.InvariantInfo, out result);
                        if (success)
                        {
                            foreach (Waypoint wp in waypointsToBeChanged)
                            {
                                wp.MinDepth = result;
                            }
                            UpdateWaypointRelatedStuff(null);
                        }
                    }
                    break;


                case "Charts":

                    messageBox = new SetValueForAllWaypointsDialog(headerText, ValidationRuleType.NoValidation, setValueOnlyForSelectedWaypoints);
                    messageBox.ShowDialog();
                    if (messageBox.DialogResult == true)
                    {
                        foreach (Waypoint wp in waypointsToBeChanged)
                        {
                            wp.Charts = (string)messageBox.Tag;
                        }
                        UpdateWaypointRelatedStuff(null);
                    }
                    break;


                case "LegReferenceObject_Object":

                    messageBox = new SetValueForAllWaypointsDialog(headerText, ValidationRuleType.NoValidation, setValueOnlyForSelectedWaypoints);
                    messageBox.ShowDialog();
                    if (messageBox.DialogResult == true)
                    {
                        foreach (Waypoint wp in waypointsToBeChanged)
                        {
                            wp.LegReferenceObject_Object = (string)messageBox.Tag;
                        }
                        UpdateWaypointRelatedStuff(null);
                    }
                    break;


                case "LegReferenceObject_Bearing":

                    messageBox = new SetValueForAllWaypointsDialog(headerText, ValidationRuleType.NoValidation, setValueOnlyForSelectedWaypoints);
                    messageBox.ShowDialog();
                    if (messageBox.DialogResult == true)
                    {
                        foreach (Waypoint wp in waypointsToBeChanged)
                        {
                            wp.LegReferenceObject_Bearing = (string)messageBox.Tag;
                        }
                        UpdateWaypointRelatedStuff(null);
                    }
                    break;


                case "LegReferenceObject_Distance":

                    messageBox = new SetValueForAllWaypointsDialog(headerText, ValidationRuleType.NoValidation, setValueOnlyForSelectedWaypoints);
                    messageBox.ShowDialog();
                    if (messageBox.DialogResult == true)
                    {
                        foreach (Waypoint wp in waypointsToBeChanged)
                        {
                            wp.LegReferenceObject_Distance = (string)messageBox.Tag;
                        }
                        UpdateWaypointRelatedStuff(null);
                    }
                    break;


                case "TurnRadius":

                    messageBox = new SetValueForAllWaypointsDialog(headerText, ValidationRuleType.SpeedRule, setValueOnlyForSelectedWaypoints);
                    messageBox.ShowDialog();
                    if (messageBox.DialogResult == true)
                    {
                        double result = 0.0;
                        bool success = double.TryParse((string)messageBox.Tag, styles, System.Globalization.NumberFormatInfo.InvariantInfo, out result);
                        if (success)
                        {
                            foreach (Waypoint wp in waypointsToBeChanged)
                            {
                                wp.TurnRadius = result;
                            }
                            UpdateWaypointRelatedStuff(null);
                        }
                    }
                    break;


                case "TurnRate":

                    messageBox = new SetValueForAllWaypointsDialog(headerText, ValidationRuleType.NoValidation, setValueOnlyForSelectedWaypoints);
                    messageBox.ShowDialog();
                    if (messageBox.DialogResult == true)
                    {
                        foreach (Waypoint wp in waypointsToBeChanged)
                        {
                            wp.TurnRate = (string)messageBox.Tag;
                        }
                        UpdateWaypointRelatedStuff(null);
                    }
                    break;


                case "LandmarkAtCourseAlt_Object":

                    messageBox = new SetValueForAllWaypointsDialog(headerText, ValidationRuleType.NoValidation, setValueOnlyForSelectedWaypoints);
                    messageBox.ShowDialog();
                    if (messageBox.DialogResult == true)
                    {
                        foreach (Waypoint wp in waypointsToBeChanged)
                        {
                            wp.LandmarkAtCourseAlt_Object = (string)messageBox.Tag;
                        }
                        UpdateWaypointRelatedStuff(null);
                    }
                    break;


                case "LandmarkAtCourseAlt_Bearing":

                    messageBox = new SetValueForAllWaypointsDialog(headerText, ValidationRuleType.NoValidation, setValueOnlyForSelectedWaypoints);
                    messageBox.ShowDialog();
                    if (messageBox.DialogResult == true)
                    {
                        foreach (Waypoint wp in waypointsToBeChanged)
                        {
                            wp.LandmarkAtCourseAlt_Bearing = (string)messageBox.Tag;
                        }
                        UpdateWaypointRelatedStuff(null);
                    }
                    break;


                case "LandmarkAtCourseAlt_Distance":

                    messageBox = new SetValueForAllWaypointsDialog(headerText, ValidationRuleType.NoValidation, setValueOnlyForSelectedWaypoints);
                    messageBox.ShowDialog();
                    if (messageBox.DialogResult == true)
                    {
                        foreach (Waypoint wp in waypointsToBeChanged)
                        {
                            wp.LandmarkAtCourseAlt_Distance = (string)messageBox.Tag;
                        }
                        UpdateWaypointRelatedStuff(null);
                    }
                    break;


                case "MaxOffTrack":

                    messageBox = new SetValueForAllWaypointsDialog(headerText, ValidationRuleType.NoValidation, setValueOnlyForSelectedWaypoints);
                    messageBox.ShowDialog();
                    if (messageBox.DialogResult == true)
                    {
                        foreach (Waypoint wp in waypointsToBeChanged)
                        {
                            wp.MaxOffTrack = (string)messageBox.Tag;
                        }
                        UpdateWaypointRelatedStuff(null);
                    }
                    break;


                case "MaxIntervalsPosFix":

                    messageBox = new SetValueForAllWaypointsDialog(headerText, ValidationRuleType.NoValidation, setValueOnlyForSelectedWaypoints);
                    messageBox.ShowDialog();
                    if (messageBox.DialogResult == true)
                    {
                        foreach (Waypoint wp in waypointsToBeChanged)
                        {
                            wp.MaxIntervalsPosFix = (string)messageBox.Tag;
                        }
                        UpdateWaypointRelatedStuff(null);
                    }
                    break;


                case "EngineStatus":

                    messageBox = new SetValueForAllWaypointsDialog(headerText, ValidationRuleType.NoValidation, setValueOnlyForSelectedWaypoints);
                    messageBox.ShowDialog();
                    if (messageBox.DialogResult == true)
                    {
                        foreach (Waypoint wp in waypointsToBeChanged)
                        {
                            wp.EngineStatus = (string)messageBox.Tag;
                        }
                        UpdateWaypointRelatedStuff(null);
                    }
                    break;


                case "NavWatchLevel":

                    messageBox = new SetValueForAllWaypointsDialog(headerText, ValidationRuleType.NoValidation, setValueOnlyForSelectedWaypoints);
                    messageBox.ShowDialog();
                    if (messageBox.DialogResult == true)
                    {
                        foreach (Waypoint wp in waypointsToBeChanged)
                        {
                            wp.NavWatchLevel = (string)messageBox.Tag;
                        }
                        UpdateWaypointRelatedStuff(null);
                    }
                    break;


                case "PosFixMethod":

                    messageBox = new SetValueForAllWaypointsDialog(headerText, ValidationRuleType.NoValidation, setValueOnlyForSelectedWaypoints);
                    messageBox.ShowDialog();
                    if (messageBox.DialogResult == true)
                    {
                        foreach (Waypoint wp in waypointsToBeChanged)
                        {
                            wp.PosFixMethod = (string)messageBox.Tag;
                        }
                        UpdateWaypointRelatedStuff(null);
                    }
                    break;


                case "ListOfLights_Volume":

                    messageBox = new SetValueForAllWaypointsDialog(headerText, ValidationRuleType.NoValidation, setValueOnlyForSelectedWaypoints);
                    messageBox.ShowDialog();
                    if (messageBox.DialogResult == true)
                    {
                        foreach (Waypoint wp in waypointsToBeChanged)
                        {
                            wp.ListOfLights_Volume = (string)messageBox.Tag;
                        }
                        UpdateWaypointRelatedStuff(null);
                    }
                    break;


                case "ListOfLights_Page":

                    messageBox = new SetValueForAllWaypointsDialog(headerText, ValidationRuleType.NoValidation, setValueOnlyForSelectedWaypoints);
                    messageBox.ShowDialog();
                    if (messageBox.DialogResult == true)
                    {
                        foreach (Waypoint wp in waypointsToBeChanged)
                        {
                            wp.ListOfLights_Page = (string)messageBox.Tag;
                        }
                        UpdateWaypointRelatedStuff(null);
                    }
                    break;


                case "ListOfRadioSignals_Volume":

                    messageBox = new SetValueForAllWaypointsDialog(headerText, ValidationRuleType.NoValidation, setValueOnlyForSelectedWaypoints);
                    messageBox.ShowDialog();
                    if (messageBox.DialogResult == true)
                    {
                        foreach (Waypoint wp in waypointsToBeChanged)
                        {
                            wp.ListOfRadioSignals_Volume = (string)messageBox.Tag;
                        }
                        UpdateWaypointRelatedStuff(null);
                    }
                    break;


                case "ListOfRadioSignals_Page":

                    messageBox = new SetValueForAllWaypointsDialog(headerText, ValidationRuleType.NoValidation, setValueOnlyForSelectedWaypoints);
                    messageBox.ShowDialog();
                    if (messageBox.DialogResult == true)
                    {
                        foreach (Waypoint wp in waypointsToBeChanged)
                        {
                            wp.ListOfRadioSignals_Page = (string)messageBox.Tag;
                        }
                        UpdateWaypointRelatedStuff(null);
                    }
                    break;


                case "SailingDirections_Volume":

                    messageBox = new SetValueForAllWaypointsDialog(headerText, ValidationRuleType.NoValidation, setValueOnlyForSelectedWaypoints);
                    messageBox.ShowDialog();
                    if (messageBox.DialogResult == true)
                    {
                        foreach (Waypoint wp in waypointsToBeChanged)
                        {
                            wp.SailingDirections_Volume = (string)messageBox.Tag;
                        }
                        UpdateWaypointRelatedStuff(null);
                    }
                    break;


                case "SailingDirections_Page":

                    messageBox = new SetValueForAllWaypointsDialog(headerText, ValidationRuleType.NoValidation, setValueOnlyForSelectedWaypoints);
                    messageBox.ShowDialog();
                    if (messageBox.DialogResult == true)
                    {
                        foreach (Waypoint wp in waypointsToBeChanged)
                        {
                            wp.SailingDirections_Page = (string)messageBox.Tag;
                        }
                        UpdateWaypointRelatedStuff(null);
                    }
                    break;


                case "NavtexChannels":

                    messageBox = new SetValueForAllWaypointsDialog(headerText, ValidationRuleType.NoValidation, setValueOnlyForSelectedWaypoints);
                    messageBox.ShowDialog();
                    if (messageBox.DialogResult == true)
                    {
                        foreach (Waypoint wp in waypointsToBeChanged)
                        {
                            wp.NavtexChannels = (string)messageBox.Tag;
                        }
                        UpdateWaypointRelatedStuff(null);
                    }
                    break;


                case "ReportTo":

                    messageBox = new SetValueForAllWaypointsDialog(headerText, ValidationRuleType.NoValidation, setValueOnlyForSelectedWaypoints);
                    messageBox.ShowDialog();
                    if (messageBox.DialogResult == true)
                    {
                        foreach (Waypoint wp in waypointsToBeChanged)
                        {
                            wp.ReportTo = (string)messageBox.Tag;
                        }
                        UpdateWaypointRelatedStuff(null);
                    }
                    break;


                case "ChannelOrTelephoneNo":

                    messageBox = new SetValueForAllWaypointsDialog(headerText, ValidationRuleType.NoValidation, setValueOnlyForSelectedWaypoints);
                    messageBox.ShowDialog();
                    if (messageBox.DialogResult == true)
                    {
                        foreach (Waypoint wp in waypointsToBeChanged)
                        {
                            wp.ChannelOrTelephoneNo = (string)messageBox.Tag;
                        }
                        UpdateWaypointRelatedStuff(null);
                    }
                    break;


                case "ActualPassingTime":

                    messageBox = new SetValueForAllWaypointsDialog(headerText, ValidationRuleType.NoValidation, setValueOnlyForSelectedWaypoints);
                    messageBox.ShowDialog();
                    if (messageBox.DialogResult == true)
                    {
                        foreach (Waypoint wp in waypointsToBeChanged)
                        {
                            wp.ActualPassingTime = (string)messageBox.Tag;
                        }
                        UpdateWaypointRelatedStuff(null);
                    }
                    break;


                case "SecurityLevel":

                    messageBox = new SetValueForAllWaypointsDialog(headerText, ValidationRuleType.NoValidation, setValueOnlyForSelectedWaypoints);
                    messageBox.ShowDialog();
                    if (messageBox.DialogResult == true)
                    {
                        foreach (Waypoint wp in waypointsToBeChanged)
                        {
                            wp.SecurityLevel = (string)messageBox.Tag;
                        }
                        UpdateWaypointRelatedStuff(null);
                    }
                    break;


                default:
                    MessageBox mBox = new MessageBox("This column has no support for setting the same value for all waypoints.", "Not supported", MessageBoxButton.OK);
                    mBox.ShowDialog();
                    break;
            }
        }

        private void ClearPassageData()
        {
            var messageBox = new MessageBox("Are you sure you want to clear ALL Passage Data?", "Clear?", MessageBoxButton.OKCancel, "Clear", string.Empty, string.Empty, ButtonType.Cancel);

            bool? result = messageBox.ShowDialog();
            if (result == true)
            {
                ClearPassage();
            }
        }

        private void ClearPassage()
        {
            DepartureTimeDateLocal = null;
            DepartureTimeLocal = null;
            DepartureTimeHoursMinutesLocal = new DateTime(0);
            DepartureTimeUtc = null;
            ArrivalTimeLocal = null;
            ArrivalTimeUtc = null;

            DraughtDepartureFore = 0;
            DraughtDepartureAft = 0;
            DraughtDepartureAir = 0;
            DraughtArrivalFore = 0;
            DraughtArrivalAft = 0;
            DraughtArrivalAir = 0;

            foreach (Waypoint wp in Waypoints)
            {
                wp.ETD = null;
            }
        }

        private void ClearPassagePlanTexts()
        {
            var messageBox = new MessageBox("Are you sure you want to clear ALL manually added texts in this Passage Plan report?", "Clear?", MessageBoxButton.OKCancel, "Clear", string.Empty, string.Empty, ButtonType.Cancel);

            bool? result = messageBox.ShowDialog();
            if (result == true)
            {
                PassagePlanTexts = new PassagePlanManuallyAddedTexts();
            }
        }

        private void AppendRoute()
        {
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();
            dlg.InitialDirectory = FileManager.RoutesDirectory;

            // Show open file dialog box
            Nullable<bool> result = dlg.ShowDialog();

            // Process open file dialog box results
            if ((bool)result == true)
            {
                UIServices.SetBusyState();
                _route.AppendRoute(dlg.FileName);
            }

            UpdateViewModelFromRoute(true);
        }

        private void GetRoute(object state)
        {
            string fileName = (string)state;
            Route route = new Route(fileName);
            Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action<Route>(UpdateUI), route);
        }

      
        private void UpdateUI(Route route)
        {
            DeparturePort = route.DeparturePort;
            ArrivalPort = route.ArrivalPort;
            TimeZoneDeparture = route.TimeZoneDeparture;
            TimeZoneArrival = route.TimeZoneArrival;
            Waypoints = route.Waypoints;
        }


        /// <summary>
        /// Shows Open File dialog and opens selected route.
        /// </summary>
        private void OpenRoute()
        {
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();
            if (Directory.Exists(FileManager.RoutesDirectory))
            {
                dlg.InitialDirectory = FileManager.RoutesDirectory;
            }

            // Show open file dialog box
            Nullable<bool> result = dlg.ShowDialog();

            // Process open file dialog box results
            if ((bool)result == true)
            {
                // Clean up previous route and passage, if there was any (to be sure that no data is left in GUI if another route was open before)
                CleanRouteAndPassage();

                string filePath = dlg.FileName;
                OpenRoute(filePath, false);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="ignoreWarnings">Will ignore warnings when True, for exampel when trying to open the last opened file at startup</param>
        public void OpenRoute(string filePath, bool ignoreWarnings)
        {
            if (!File.Exists(filePath) && !ignoreWarnings)
            {
                MessageBox mb = new MessageBox("File " + Path.GetFileName(filePath) + " was not found.", "File not found", MessageBoxButton.OK);
                mb.ShowDialog();
            }
            else
            {
                UIServices.SetBusyState();

                try
                {
                    _route = new Route(filePath);

                    if (_route.Waypoints.Count > 0 || _route.PassagePlannerFileOpenOK)
                    {
                        FilePath = filePath;

                        UpdateViewModelFromRoute(false);

                        IsDirty = false;
                    }
                    else if (!ignoreWarnings)
                    {
                        MessageBox mb = new MessageBox("Could not open file \"" + Path.GetFileName(filePath) + "\".", "Failed to open file", MessageBoxButton.OK);
                        mb.ShowDialog();
                    }

                }
                catch (ReflectionTypeLoadException reflectionEx)
                {
                    ExceptionMessageBox mb = new ExceptionMessageBox(reflectionEx.LoaderExceptions[0].ToString(), "Failed to load Ecdis plugin", MessageBoxButton.OK);
                    mb.ShowDialog();
                }
                catch (Exception ex)
                {
                    MessageBox mb = new MessageBox("Could not open file \"" + Path.GetFileName(filePath) + "\".", "Failed to open file", MessageBoxButton.OK);
                    mb.ShowDialog();
                }
            }
        }

        /// <summary>
        /// Common code used in for example OpenRoute(), AppendRoute() and ImportEcdisRoute().
        /// </summary>
        private void UpdateViewModelFromRoute(bool keepExistingWaypointsInViewModel)
        {
            if (MapSource == null)
            {
                MapSource = new LocalMapSource();
            }

            double longMin = Double.MaxValue;
            double longMax = Double.MinValue;
            double latMin = Double.MaxValue;
            double latMax = Double.MinValue;

            if (!keepExistingWaypointsInViewModel)
            {
                Waypoints.Clear();
            }

            if (_route.Waypoints.Count > 0)
            {
                foreach (Waypoint waypoint in _route.Waypoints)
                {
                    Waypoints.Add(waypoint);

                    longMin = Math.Min(longMin, waypoint.Longitude);
                    longMax = Math.Max(longMax, waypoint.Longitude);
                    latMin = Math.Min(latMin, waypoint.Latitude);
                    latMax = Math.Max(latMax, waypoint.Latitude);
                }

                double maxDiff = (longMax - longMin > latMax - latMin ? longMax - longMin : latMax - latMin);

                MapCenter = new Point((longMin + longMax) / 2, (latMin + latMax) / 2);
                MapZoom = Math.Log(450 / maxDiff, 2);
            }

            if (!keepExistingWaypointsInViewModel)
            {
                if (_route.DeparturePort == null)
                {
                    DeparturePort = string.Empty;
                }
                else
                {
                    DeparturePort = _route.DeparturePort;
                }

                TimeZoneDeparture = _route.TimeZoneDeparture;
                DepartureTimeDateLocal = _route.DepartureTimeDate;
                DepartureTimeHoursMinutesLocal = _route.DepartureTimeHoursAndMinutes;
            }
            if (_route.ArrivalPort == null)
            {
                ArrivalPort = string.Empty;
            }
            else
            {
                ArrivalPort = _route.ArrivalPort;
            }
            
            TimeZoneArrival = _route.TimeZoneArrival;
            if (Waypoints != null && Waypoints.Count > 0 && _route.SelectedSospWaypointIndex > -1 && Waypoints.Count > _route.SelectedSospWaypointIndex)
            {
                SelectedSospWaypoint = Waypoints[_route.SelectedSospWaypointIndex];
            }
            if (Waypoints != null && Waypoints.Count > 0 && _route.SelectedEospWaypointIndex > -1 && Waypoints.Count > _route.SelectedEospWaypointIndex)
            {
                SelectedEospWaypoint = Waypoints[_route.SelectedEospWaypointIndex];
            }
            
            PassagePlanTexts = _route.PassagePlanTexts;
            DraughtArrivalAft = _route.DraughtArrivalAft;
            DraughtArrivalAir = _route.DraughtArrivalAir;
            DraughtArrivalFore = _route.DraughtArrivalFore;
            DraughtDepartureAft = _route.DraughtDepartureAft;
            DraughtDepartureAir = _route.DraughtDepartureAir;
            DraughtDepartureFore = _route.DraughtDepartureFore;

            if (DeparturePort.Length == 0 && ArrivalPort.Length == 0)
            {
                RouteName = _route.RouteName;
            }

            UpdateWaypointRelatedStuff(null);
        }

        private void CloseRoute()
        {
            if (IsDirty)
            {
                MessageBox mb = new MessageBox("Do you want to save changes?", "Save Changes?", MessageBoxButton.YesNo, "Save", "Don't Save", "Cancel", ButtonType.undefined);
                bool? dialogResult = mb.ShowDialog();
                mb = null;

                if (dialogResult != null)
                {
                    if (dialogResult == true)
                    {
                        // Save route data and passage data
                        SaveRoute();
                    }
                }
            }

            UIServices.SetBusyState();

            // Clean up, to be sure that no data is left in the GUI
            CleanRouteAndPassage();
          
            IsDirty = false;
        }

        private void CleanRouteAndPassage()
        {
            // Clean/Reset all kinds of data
            FilePath = string.Empty;
            Waypoints.Clear();
            ReportPages_Waypoints.Clear();
            ReportPages_Waypoints_ChartsAndPublications.Clear();
            PolylinePoints.Clear();
            NoOfWaypoints = 0;
            DeparturePort = string.Empty;
            ArrivalPort = string.Empty;
            RouteName = string.Empty;
            TimeZoneDeparture = 0;
            TimeZoneArrival = 0;
            DepartureTimeLocal = null;
            DepartureTimeDateLocal = null;
            DepartureTimeHoursMinutesLocal = new DateTime(0);
            DepartureTimeUtc = null;
            ArrivalTimeLocal = null;
            ArrivalTimeUtc = null;
            TotalDistance = null;
            AverageSpeed = null;
            TotalSailingTime = TimeSpan.Zero;
            PassagePlanTexts = new PassagePlanManuallyAddedTexts();
            if (_route != null && _route.Waypoints != null && _route.Waypoints.Count > 0)
            {
                _route.Waypoints.Clear();
            }
            //_route.PassagePlanTexts = PassagePlanTexts;
            SelectedSospWaypoint = null;
            SelectedEospWaypoint = null;
            DraughtArrivalAft = 0;
            DraughtArrivalAir = 0;
            DraughtArrivalFore = 0;
            DraughtArrivalDeepest = 0;
            DraughtDepartureAft = 0;
            DraughtDepartureAir = 0;
            DraughtDepartureFore = 0;
            DraughtDepartureDeepest = 0;


            // Squat page
            ViewModelLocator locator = new ViewModelLocator();
            SquatViewModel squatVM = locator.SquatVM;
            squatVM.VesselDraughtFore = 0;
            squatVM.VesselDraughtAft = 0;
            squatVM.Speed = 0;
            squatVM.WaterDepth = 0;
            squatVM.ChannelBeam = 0;
            squatVM.BlockCoefficient = 0;
            squatVM.WaypointEta = DateTime.MinValue;
            squatVM.IncreasedDraughtDueToReducedWaterDensity = 0;
            squatVM.WaterDensity = 0;
            squatVM.ListInDegrees = 0;
            squatVM.EffectFromList = 0;
            squatVM.EffectOfSwell = 0;
            squatVM.TidalHeight = 0;
            squatVM.DivergenceFromPredictedTide = 0;
            squatVM.HydrographicSurveyTolerance = 0;
            squatVM.EffectFromMeteorologicConditions = 0;
        }

        public void SaveRoute()
        {
            if (FilePath != string.Empty && File.Exists(FilePath) && FilePath.EndsWith(FileManager.RouteExtension))
            {
                // If Passage Planner file format is already open => Just save the changes without dialog 

                UIServices.SetBusyState();

                UpdateRouteFromViewModel();

                // Save
                FilePath = _route.Save(FilePath);
                IsDirty = false;
                StatusBarText = Path.GetFileName(FilePath) + " was successfully saved"; 
            }
            else
            {
                // Show dialog and let user choose file name to save 
                SaveRouteAs();
            }
        }

        

        private void SaveRouteAs()
        {
            Microsoft.Win32.SaveFileDialog dlg = new Microsoft.Win32.SaveFileDialog();
            dlg.InitialDirectory = FileManager.RoutesDirectory;
            dlg.DefaultExt = FileManager.RouteExtension;
            dlg.FileName = Path.GetFileNameWithoutExtension(FilePath);

            // In case opening a file with another extension than this save method supports (for example old excel file or Seaware .swx file) 
            // => keep the name but change extension
            dlg.Filter = "Passage Planner route file (*" + FileManager.RouteExtension + ")|*" + FileManager.RouteExtension;

            // Show save file dialog box
            Nullable<bool> result = dlg.ShowDialog();

            // Process open file dialog box results
            if ((bool)result == true)
            {
                UIServices.SetBusyState();

                UpdateRouteFromViewModel();

                // Save
                FilePath = _route.Save(dlg.FileName);
                IsDirty = false;
                StatusBarText = Path.GetFileName(FilePath) + " was successfully saved"; 
            }
        }

        private void UpdateRouteFromViewModel()
        {
            _route.PassagePlanTexts = PassagePlanTexts;
            _route.DepartureTimeDate = DepartureTimeDateLocal;
            _route.DepartureTimeHoursAndMinutes = DepartureTimeHoursMinutesLocal;
            _route.DraughtArrivalAft = DraughtArrivalAft;
            _route.DraughtArrivalAir = DraughtArrivalAir;
            _route.DraughtArrivalFore = DraughtArrivalFore;
            _route.DraughtDepartureAft = DraughtDepartureAft;
            _route.DraughtDepartureAir = DraughtDepartureAir;
            _route.DraughtDepartureFore = DraughtDepartureFore;
            if (_route.Waypoints != null)
            {
                _route.Waypoints.Clear();
            }
            else
            {
                _route.Waypoints = new DispatchingObservableCollection<Waypoint>();
            }

            if (Waypoints != null && Waypoints.Count > 0)
            {
                _route.SelectedSospWaypointIndex = Waypoints.IndexOf(SelectedSospWaypoint);
                _route.SelectedEospWaypointIndex = Waypoints.IndexOf(SelectedEospWaypoint);

                foreach (Waypoint wp in Waypoints)
                {
                    _route.Waypoints.Add(wp);
                }
            }
            else
            {
                _route.SelectedSospWaypointIndex = -1;
                _route.SelectedEospWaypointIndex = -1;
            }
        }


        public void UpdateWaypointRelatedStuff(DataGridCellEditEndingEventArgs e)
        {
            // Set PolylinePoints which shall be a mirrored copy of the positions in Waypoints, but in another format.
            _polylinePoints.Clear();
            _polylinePoints2.Clear();

            NoOfWaypoints = 0;

            if (_waypoints != null && _waypoints.Count > 0)
            {
                _isDateLineCrossed = false;
                _isDateLineExtraPointsCalculated = false;

                _reportPages_Waypoints = new DispatchingObservableCollection<DispatchingObservableCollection<Waypoint>>();
                _reportPages_Waypoints_ChartsAndPublications = new DispatchingObservableCollection<DispatchingObservableCollection<Waypoint>>();

                int waypointIndex = 0;
                int waypointNo = 1;
                int? editedEtaIndex = null;
                if (e != null)
                {
                    editedEtaIndex = e.Row.GetIndex();
                }

                Waypoint previousWp = null;
                Waypoint nextWp = null;
                IPosition previousPosition = null;
                IPosition thisPosition = null;
   
                _waypoints[0].ETD = DepartureTimeUtc;
                TimeSpan sailingTimeSoFar = new TimeSpan(0);

                double distanceSailedSoFarInMeters = 0.0;
                TotalDistance = 0.0;

                // Squat and UKC parameters
                ViewModelLocator locator = new ViewModelLocator();
                VezzelViewModel vesselVM = locator.VesselVM;
                double maxDraughtVessel = Math.Max(DraughtDepartureFore, DraughtDepartureAft);
                double meanDraughtVessel = (DraughtDepartureFore + DraughtDepartureAft) / 2.0;
                double blockCoefficient = vesselVM.GetBlockCoefficientAtDraught(meanDraughtVessel);

                
                
                foreach (Waypoint wp in _waypoints)
                {
                    double distanceInMetersToNextWp = 0.0;

                    wp.WaypointNo = waypointNo.ToString();

                    thisPosition = new Position(wp.Latitude, wp.Longitude);

                    ////////////////////////////////////////////////////////////////////////
                    // 1) ADD WAYPOINT TO THE MAP
                    ////////////////////////////////////////////////////////////////////////

                    if (wp.IsGreatCircle)
                    {
                        // Calculate 100 intermediate points on the great circle, for plotting
                        if (waypointIndex < _waypoints.Count - 1) // Don't do this for the last waypoint, because we need to look at the waypoint after current waypoint (=nextWp).
                        {
                            nextWp = _waypoints[waypointIndex + 1];

                            //IPosition thisPosition = new Position(wp.Latitude, wp.Longitude);
                            IPosition nextPosition = new Position(nextWp.Latitude, nextWp.Longitude);


                            IEnumerable<IPosition> greatCirclePoints = GreatCircle.IntermediatePoints(thisPosition, nextPosition, 100);
                            IPosition previousPos = null;
                            foreach (IPosition pos in greatCirclePoints)
                            {
                                // Add point to the map (and check if we may have crossed the international date line)
                                AddPolylinePoint(previousPos, pos);

                                previousPos = pos;
                            }
                        }
                        else
                        {
                            // This case may never occur, but is handled anyway. Happens if leg type of LAST waypoint is set to Great Circle.
                            // Anyway, plot the waypoint.
                            AddPolylinePoint(previousPosition, thisPosition);
                        }
                    }
                    else
                    {
                        // Leg type == Rhumbline
                        // Add point to the map (and check if we may have crossed the international date line)
                        AddPolylinePoint(previousPosition, thisPosition);
                    }

                    ////////////////////////////////////////////////////////////////////////
                    // 2) CALCULATE SQUAT AND UKC FOR THIS WAYPOINT
                    ////////////////////////////////////////////////////////////////////////

                    if (wp.LegSpeedSetting != null)
                    {
                        // Create an instance of SquatUkcItem, which will calculate Squat and UKC for this waypoint (visible in the Passage Planner printable report).
                        wp.SquatUkc = new SquatUkcItem((double)wp.LegSpeedSetting, wp.MinDepth, vesselVM.VesselBeam, blockCoefficient, maxDraughtVessel, meanDraughtVessel, true);
                    }

                    ////////////////////////////////////////////////////////////////////////
                    // 3) CALCULATE DISTANCE, HEADING AND TIME TO NEXT WAYPOINT
                    ////////////////////////////////////////////////////////////////////////

                    if (waypointIndex < _waypoints.Count - 1)
                    {
                        double headingToNextWp;

                        nextWp = _waypoints[waypointIndex + 1];

                        //IPosition thisPosition = new Position(wp.Latitude, wp.Longitude);
                        IPosition nextPosition = new Position(nextWp.Latitude, nextWp.Longitude);

                        // Calculate "distance to next waypoint" and "heading to next waypoint".
                        if (wp.IsGreatCircle)
                        {
                            double headingInRadians;
                            distanceInMetersToNextWp = GreatCircle.DistanceBetweenPositions(thisPosition, nextPosition, out headingInRadians);
                            headingToNextWp = headingInRadians * 180 / Math.PI;
                        }
                        else
                        {
                            RhumbLine.DistanceAndCourseBetweenPositions(thisPosition, nextPosition, out distanceInMetersToNextWp, out headingToNextWp);
                        }

                        wp.CourseToNextWaypoint = Convert.ToInt32(headingToNextWp);
                        wp.DistanceToNextWaypoint = distanceInMetersToNextWp / 1852.0;
                        if (wp.LegSpeedSetting != null && wp.LegSpeedSetting.Value != 0.0)
                        {
                            wp.TimeToNextWaypoint = TimeSpan.FromSeconds(distanceInMetersToNextWp / (wp.LegSpeedSetting.Value * 1852.0 / 3600.0));
                        }
                    }

                    ////////////////////////////////////////////////////////////////////////
                    // 4) CALCULATE DISTANCE SAILED AND ETD
                    ////////////////////////////////////////////////////////////////////////

                    wp.DistanceSailed = distanceSailedSoFarInMeters / 1852.0;

                    // Calculate for the NEXT waypoint (will be assigned in next loop iteration, see above)
                    distanceSailedSoFarInMeters = distanceSailedSoFarInMeters + distanceInMetersToNextWp;

                    // Calculate waypoint ETA/ETD
                    if (previousWp != null)
                    {
                        if (previousWp.ETD != null)
                        {
                            wp.ETD = previousWp.ETD.Value.Add(previousWp.TimeToNextWaypoint);
                        }
                        else
                        {
                            wp.ETD = null;
                        }

                        if (previousWp.TimeToNextWaypoint != null)
                        {
                            sailingTimeSoFar = sailingTimeSoFar.Add(previousWp.TimeToNextWaypoint);
                            wp.SailingTimeToThisWaypoint = sailingTimeSoFar;
                        }
                    }

                    //////////////////////////////////////////////////////////////////////////////////////////
                    // 5) ADD WAYPOINT INFORMATION TO DATA STRUCTURES FOR THE PASSAGE PLAN PRINTABLE REPORT
                    //////////////////////////////////////////////////////////////////////////////////////////


                    // Add the waypoint information to the proper element in _reportPartAPages_Waypoints.
                    // _reportPartAPages_Waypoints is a structure to which the the datagrids in the printable report 
                    // is data bound.
                    int pageN = 0;

                    // Calculate to which page the waypoint will be added
                    if (waypointNo <= NUMBER_OF_WAYPOINTS_IN_REPORT_FIRST_PAGE)
                    {
                        pageN = 0;
                    }
                    else
                    {
                        pageN = Convert.ToInt32(Math.Floor(Convert.ToDouble(waypointNo - NUMBER_OF_WAYPOINTS_IN_REPORT_FIRST_PAGE - 1) / Convert.ToDouble(NUMBER_OF_WAYPOINTS_IN_REPORT_FOLLOWING_PAGES))) + 1;
                    }

                    if (_reportPages_Waypoints.Count < pageN + 1)
                    {
                        // If needed, create a new collection of waypoints associated with the datagrid on page N
                        _reportPages_Waypoints.Add(new DispatchingObservableCollection<Waypoint>());
                    }

                    _reportPages_Waypoints[pageN].Add( (Waypoint)wp.Clone() );

                    // SPECIAL HANDLING OF CHARTS AND PUBLICATIONS PAGES!!! 
                    // (text wrapping and limited text lengths can make every wp occupy two rows in the report)
                    int pageN_ChartsAndPublications = 0;
                    if (waypointNo <= NUMBER_OF_WAYPOINTS_IN_CHARTS_AND_PUBLICATIONS_PAGE)
                    {
                        pageN_ChartsAndPublications = 0;
                    }
                    else
                    {
                        pageN_ChartsAndPublications = Convert.ToInt32(Math.Floor(Convert.ToDouble(waypointNo - NUMBER_OF_WAYPOINTS_IN_CHARTS_AND_PUBLICATIONS_PAGE - 1) / Convert.ToDouble(NUMBER_OF_WAYPOINTS_IN_CHARTS_AND_PUBLICATIONS_PAGE))) + 1;
                    }
                    if (_reportPages_Waypoints_ChartsAndPublications.Count < pageN_ChartsAndPublications + 1)
                    {
                        // If needed, create a new collection of waypoints associated with the datagrid on page N
                        _reportPages_Waypoints_ChartsAndPublications.Add(new DispatchingObservableCollection<Waypoint>());
                    }
                    _reportPages_Waypoints_ChartsAndPublications[pageN_ChartsAndPublications].Add((Waypoint)wp.Clone());
                    

                    previousWp = wp;
                    previousPosition = thisPosition;
                    waypointNo++;
                    waypointIndex++;
                }

                // Reset course and distance for last waypoint to zero. 
                // (necessary if waypoint below has been deleted, and the course and distance remain from previous calculation)
                _waypoints[_waypoints.Count - 1].CourseToNextWaypoint = 0;
                _waypoints[_waypoints.Count - 1].DistanceToNextWaypoint = 0.0;


                ////////////////////////////////////////////////////////////////////////
                // 6) CALCULATE ARRIVAL TIME, TOTAL DISTANCE AND TIME TO DESTINATION
                ////////////////////////////////////////////////////////////////////////

                ArrivalTimeUtc = _waypoints[_waypoints.Count - 1].ETD;

                TotalSailingTime = sailingTimeSoFar;

                TotalDistance = distanceSailedSoFarInMeters / 1852.0;


                // Loop through waypoints once again now that we know ArrivalTime and TotalSailingTime
                foreach (Waypoint wp in _waypoints)
                {
                    wp.DistanceToGoToDestination = (double)TotalDistance - wp.DistanceSailed;
                    wp.TimeToGoToDestination = TotalSailingTime.Add(-wp.SailingTimeToThisWaypoint);
                }

                CalculateDistanceAndSpeedToSosp();
                CalculateDistanceAndSpeedSospToEosp();
                CalculateDistanceAndSpeedEospToArrival();

                NoOfWaypoints = _waypoints.Count;
            }

            RaisePropertyChanged("PolylinePoints");
            RaisePropertyChanged(NoOfWaypointsPropertyName);
            RaisePropertyChanged(WaypointsPropertyName);
            RaisePropertyChanged("ReportPages_Waypoints");
        }


        /// <summary>
        /// Adds position "pos" to the collection of points which will be plotted on map.
        /// </summary>
        /// <param name="previousPos">Previous position is needed to evaluate if international date line is crossed</param>
        /// <param name="pos">Position to be plotted</param>
        private void AddPolylinePoint(IPosition previousPos, IPosition pos)
        {
            // The point to be plotted on the map
            Point p = new Point(pos.Longitude, pos.Latitude);

            // Check if we have crossed the international date line since previous position 
            // (if the difference in longitude is more than 180 degrees)
            if (!_isDateLineCrossed && previousPos != null && (Math.Abs(pos.Longitude - previousPos.Longitude) > 180.0))
            {
                _isDateLineCrossed = true;
            }

            if (!_isDateLineCrossed)
            {
                // Normal case
                _polylinePoints.Add(p);
            }
            else
            {
                // If we just crossed the date line => add waypoint to "polyline number two"

                //...but first add two points on each side of the date line, in order to draw the polyline
                // all the way to the date line, and from the date line to the following waypoint.
                if (!_isDateLineExtraPointsCalculated)
                {
                    // Calculate at which latitude the date line is crossed
                    double latitudeDateLine = CalculateLatitudeAtDateLineCrossing(previousPos, pos);

                    // Add the two points to the polyline (NOTE! No waypoints!). Just to draw the line to the edges of the map.
                    _polylinePoints.Add(new Point(179.5 * Math.Sign(previousPos.Longitude), latitudeDateLine)); // 179.5 is chosen because it gives the best visual look
                    _polylinePoints2.Add(new Point(179.5 * Math.Sign(pos.Longitude), latitudeDateLine));

                    _isDateLineExtraPointsCalculated = true;
                }

                // From now on, add points to the second polyline since vessel has crossed the date line
                _polylinePoints2.Add(p);
            }
        }

        /// <summary>
        /// Calculate the latitude value where the route crosses the date line.
        /// </summary>
        /// <param name="previousPos">Previous waypoint</param>
        /// <param name="pos">Current waypoint</param>
        /// <returns>Latitude where route crosses date line</returns>
        private static double CalculateLatitudeAtDateLineCrossing(IPosition previousPos, IPosition pos)
        {
            // y = kx + m,   k = (y2-y1)/(x2-x1)
            double k;
            double x1;
            double y1;
            double x2;
            double y2;

            // Transform longitude values as they were values on an x-axis with x=0 at the date line.
            // Find out which longitude value that correspond to x1, that is: west side of date line.
            if (previousPos.Longitude > pos.Longitude)
            {
                // Vessel heading east
                x1 = previousPos.Longitude - 180.0;
                y1 = previousPos.Latitude;

                x2 = 180.0 + pos.Longitude;
                y2 = pos.Latitude;
            }
            else
            {
                // Vessel heading west
                x1 = pos.Longitude - 180.0;
                y1 = pos.Latitude;

                x2 = 180.0 + previousPos.Longitude;
                y2 = previousPos.Latitude;
            }
            k = (y2 - y1) / (x2 - x1);

            // latitudeDateLine is the "m" in equation y = kx + m
            // m = y - kx,  m = y1 - k*x1
            double latitudeDateLine = y1 - k * x1;
            return latitudeDateLine;
        }

        private void OpenTotalTide()
        {
            try
            {
                ViewModelLocator locator = new ViewModelLocator();
                AppSettingsViewModel appSettings = locator.AppSettingsVM;
                string executableFilePath = Path.Combine(appSettings.TotalTideDirectory, "TotalTide.exe");

                if (File.Exists(executableFilePath))
                {
                    Process.Start(executableFilePath);
                }
                else
                {
                    MessageBox mb = new MessageBox("Executable file TotalTide.exe was not found", "File not found", MessageBoxButton.OK);
                    mb.ShowDialog();
                }
            }
            catch (Exception ex)
            {
                ErrorHandler.Show(ex);
            }
        }

        public void ExportToSwxFile()
        {
            try
            {
                if (this.Waypoints != null && this.Waypoints.Count > 0)
                {
                    MessageBox mb1 = new MessageBox("Route can only be exported to format *.swx, \nwhich then can be imported into \nSeaware Routing 7.2 and above.", "Export Route", MessageBoxButton.OKCancel, "Export", string.Empty, "Cancel", ButtonType.OK);
                    mb1.ShowDialog();

                    if (mb1.DialogResult != null && (bool)mb1.DialogResult)
                    {
                        Microsoft.Win32.SaveFileDialog dlg = new Microsoft.Win32.SaveFileDialog();
                        dlg.InitialDirectory = FileManager.RoutesDirectory;
                        dlg.DefaultExt = FileManager.RouteExtension;
                        if (FilePath.Length > 0)
                        {
                            dlg.FileName = Path.GetFileNameWithoutExtension(FilePath);
                        }
                        dlg.Filter = "Seaware Routing file (*.swx)|*.swx";

                        // Show save file dialog box
                        Nullable<bool> result = dlg.ShowDialog();

                        // Process open file dialog box results
                        if ((bool)result == true)
                        {
                            UIServices.SetBusyState();

                            using (FileStream fs = new FileStream(dlg.FileName, FileMode.Create))
                            {
                                XmlWriterSettings xmlWriterSettings = new XmlWriterSettings();
                                xmlWriterSettings.NewLineOnAttributes = false;
                                xmlWriterSettings.Indent = true;

                                using (XmlWriter writer = XmlTextWriter.Create(fs, xmlWriterSettings))
                                {
                                    _activeRouteSerializer.Serialize(writer, GetActiveRoute());
                                }
                            }

                            // Grant access so everybody can read and write to this file
                            FileManager.GrantAccess(dlg.FileName);

                            StatusBarText = "Route was successfully exported to file " + Path.GetFileName(dlg.FileName);
                        }
                    } 
                }
                else
                { 
                    MessageBox mb2 = new MessageBox("No waypoints were found. \nPlease create or open a route. \nThen try to export again!", "Export failed", MessageBoxButton.OK);
                    mb2.ShowDialog();
                }
            }
            catch (Exception ex)
            {
                ErrorHandler.Show(ex);
            }
        }

        public void ReverseRoute()
        {
            try
            {
                if (this.Waypoints != null && this.Waypoints.Count > 0)
                {
                    string tempPort = ArrivalPort;
                    ArrivalPort = DeparturePort;
                    DeparturePort = tempPort;

                    double tempTimeZone = TimeZoneArrival;
                    TimeZoneArrival = TimeZoneDeparture;
                    TimeZoneDeparture = tempTimeZone;

                    List<Waypoint> tempWaypoints = new List<Waypoint>(this.Waypoints);
                    tempWaypoints.Reverse();
                    this.Waypoints = new DispatchingObservableCollection<Waypoint>(tempWaypoints);

                    Waypoint tempWaypoint = SelectedEospWaypoint;
                    SelectedEospWaypoint = SelectedSospWaypoint;
                    SelectedSospWaypoint = tempWaypoint;

                    ClearPassage();

                    UpdateWaypointRelatedStuff(null);

                    ViewModelLocator locator = new ViewModelLocator();
                    SquatViewModel squatVM = locator.SquatVM;
                    squatVM.Waypoints = this.Waypoints;

                    StatusBarText = "Route was successfully reverted!";
                }
                else
                {
                    MessageBox mb2 = new MessageBox("No waypoints were found. \nPlease create or open a route. \nThen try to reverse route again!", "Reverse Route failed", MessageBoxButton.OK);
                    mb2.ShowDialog();
                }
            }
            catch (Exception ex)
            {
                ErrorHandler.Show(ex);
            }
        }

        private activeRoute GetActiveRoute()
        {
            activeRoute actRoute = new activeRoute();
            if (this.RouteName != null && this.RouteName.Length > 0)
            {
                actRoute.routeId = this.RouteName;
            }
            else
            {
                actRoute.routeId = "no_name";
            }

            List<activeRouteRouteWp> activeRouteWaypoints = new List<activeRouteRouteWp>();

            int i = 1;
            foreach(Waypoint wp in this.Waypoints)
            {
                activeRouteRouteWp activeWp = new activeRouteRouteWp();
                activeWp.wpPrimaryNumber = i;
                activeWp.wpPrimaryNumberSpecified = true;
                activeWp.wpSecondaryNumber = 0;
                activeWp.wpSecondaryNumberSpecified = true;
                activeWp.wpType = "user";
                activeWp.latAsDecimalDegree = wp.Latitude;
                activeWp.longAsDecimalDegree = wp.Longitude;
                activeWp.followingLegType = (wp.LegType == Seaware.Navigation.Enumerations.LegType.GreatCircle ? "gc" : "rl");
                if (wp.LegSpeedSetting != null)
                {
                    activeWp.resultingSpeedInKnot = (double)wp.LegSpeedSetting;
                    activeWp.resultingSpeedInKnotSpecified = true;
                }

                activeRouteWaypoints.Add(activeWp);
                i++;
            }

            actRoute.routeWp = activeRouteWaypoints.ToArray();
            return actRoute;
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }
    }
}