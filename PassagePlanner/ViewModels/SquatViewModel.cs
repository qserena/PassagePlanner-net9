using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;


using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Media;
using System.Collections.Specialized;
using System.Collections;
using System.Threading.Tasks;
using System.Windows.Threading;
using System.ComponentModel;
using System.Threading;
using System.Xml.Serialization;
using System.IO;
using System.Xml;
//using EcdisLayer;

namespace PassagePlanner
{
    /// <summary>
    /// This class contains properties that a View can data bind to.
    /// <para>
    /// See http://www.galasoft.ch/mvvm
    /// </para>
    /// </summary>
    public class SquatViewModel : ViewModelBase
    {
        private bool _isDirty;
        private bool _showSettings;
        private const double EPSILON = 0.0001; // Small number, when double values are compared with zero.

        private DispatchingObservableCollection<Waypoint> _waypoints;
        private Waypoint _selectedWaypoint;
        private string _waypointNoAndName;
        private double _speed;
        private double _waterDepth;
        private DateTime _waypointEta;
        private double _minUkcRequired;
        private double _channelBeam;
        private double _vesselDraughtFore;
        private double _vesselDraughtAft;
        private double _vesselDraughtMid;
        private double _vesselDraughtMax;
        private double _blockCoefficient;
        private double _vesselBeam;

        private double _effectOfSwell;
        private double _tidalHeight;
        private double _divergenceFromPredictedTide;
        private double _hydrographicSurveyTolerance;
        private double _effectFromMeteorologicConditions;
        private double _increasedDraughtDueToReducedWaterDensity;
        private double _waterDensity;
        private double _listInDegrees;
        private double _effectFromList;
        private double _totalDraughtCorrection;
        private double _totalDepthCorrection;
        private double _totalDraughtAndDepthCorrection;

        private SquatUkcItem _squatUkc;
        private DispatchingObservableCollection<SquatUkcItem> _squatUkcConstSpeedVariousDepths;
        private DispatchingObservableCollection<SquatUkcItem> _squatUkcConstDepthVariousSpeeds;

        private string _ukcStatusText;

        

        /// <summary>
        /// Initializes a new instance of the ShipInfoViewModel class.
        /// </summary>
        public SquatViewModel()
        {
            ShowSettings = true;
           
            ViewModelLocator locator = new ViewModelLocator();
            RouteViewModel routeVM = locator.RouteVM;
            Waypoints = routeVM.Waypoints;
            VesselDraughtFore = routeVM.DraughtDepartureFore;
            VesselDraughtAft = routeVM.DraughtDepartureAft;

            VezzelViewModel vesselVM = locator.VesselVM;

            // Create SquatItem objects for "constant depth, various speeds", which is the right side of table in the Squat Calculation Report
            CreateSquatItemObjects();

            VesselBeam = vesselVM.VesselBeam;

            IsDirty = false;
        }

        /// <summary>
        /// Create SquatItem objects.
        /// </summary>
        private void CreateSquatItemObjects()
        {
            // SquatUkc is the SquatUkcItem shown at the bottom of "Squat Input" page and "Ukc Determination" report.
            SquatUkc = new SquatUkcItem(Speed, WaterDepth, VesselBeam);
            SquatUkc.ShowSquatEvenAtGreatDepths = true;

            SquatUkcConstSpeedVariousDepths = new DispatchingObservableCollection<SquatUkcItem>();
            SquatUkcConstDepthVariousSpeeds = new DispatchingObservableCollection<SquatUkcItem>();

            // Create SquatItem objects for "constant speed, various depths", which is the left side of table in the Squat Calculation Report
            for (int i = 0; i < 24; i++)
            {
                SquatUkcItem item = new SquatUkcItem(Speed, 0, VesselBeam);
                SquatUkcConstSpeedVariousDepths.Add(item);
            }

            // Create SquatItem objects for "constant depth, various speeds", which is the right side of table in the Squat Calculation Report

            // Speed = 2,4,6
            for (int i = 0; i < 3; i++)
            {
                SquatUkcItem item = new SquatUkcItem(i * 2 + 2, WaterDepth, VesselBeam);
                item.ShowSquatEvenAtGreatDepths = true;
                SquatUkcConstDepthVariousSpeeds.Add(item);
            }
            // Speed = 7,8,9,...,20,21,22
            for (int i = 7; i < 23; i++)
            {
                SquatUkcItem item = new SquatUkcItem(i, WaterDepth, VesselBeam);
                item.ShowSquatEvenAtGreatDepths = true;
                SquatUkcConstDepthVariousSpeeds.Add(item);
            }
            // Speed = 24,26,28,30,32
            for (int i = 12; i < 17; i++)
            {
                SquatUkcItem item = new SquatUkcItem(i * 2, WaterDepth, VesselBeam);
                item.ShowSquatEvenAtGreatDepths = true;
                SquatUkcConstDepthVariousSpeeds.Add(item);
            }
        }

        public bool IsDirty
        { 
            get
            {
                return _isDirty;
            }
            set
            {
                if (_isDirty == value)
                {
                    return;
                }
                
                _isDirty = value;
                RaisePropertyChanged("IsDirty");
            }
        }

        public bool ShowSettings
        {
            get
            {
                return _showSettings;
            }
            set
            {
                _showSettings = value;
                RaisePropertyChanged("ShowSettings");
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

                RaisePropertyChanged("Waypoints");
            }
        }

        public Waypoint SelectedWaypoint
        {
            get
            {
                return _selectedWaypoint;
            }
            set
            {
                _selectedWaypoint = value;
                RaisePropertyChanged("SelectedWaypoint");

                if (_selectedWaypoint != null)
                {
                    WaypointNoAndName = ((Waypoint)_selectedWaypoint).WaypointNoAndName;
                    double? speed = ((Waypoint)_selectedWaypoint).LegSpeedSetting;
                    if (speed != null)
                    {
                        Speed = (double)speed;
                    }
                    WaterDepth = ((Waypoint)_selectedWaypoint).MinDepth;
                    RaisePropertyChanged("WaterDepth");
                    DateTime? etd = ((Waypoint)_selectedWaypoint).ETD;
                    if (etd != null)
                    {
                        WaypointEta = (DateTime)etd;
                        RaisePropertyChanged("WaypointEta");
                    }
                }
            }
        }

        public string WaypointNoAndName 
        {
            get
            {
                return _waypointNoAndName;
            }
            set
            {
                if (_waypointNoAndName == value)
                {
                    return;
                }

                _waypointNoAndName = value;
                IsDirty = true;
                RaisePropertyChanged("WaypointNoAndName");
            }
        }

        public double Speed
        {
            get
            {
                return _speed;
            }
            set
            {
                if (_speed == value)
                {
                    return;
                }

                _speed = value;
                RaisePropertyChanged("Speed");
                IsDirty = true;

                SquatUkc.Speed = _speed;

                foreach (SquatUkcItem item in SquatUkcConstSpeedVariousDepths)
                {
                    item.Speed = _speed;
                }
                CheckUkc();
            }
        }

        public double WaterDepth
        {
            get
            {
                return _waterDepth;
            }
            set
            {
                if (_waterDepth == value)
                {
                    return;
                }

                _waterDepth = value;
                RaisePropertyChanged("WaterDepth");
                IsDirty = true;

                SquatUkc.WaterDepth = _waterDepth;

                foreach (SquatUkcItem item in SquatUkcConstDepthVariousSpeeds)
                {
                    item.WaterDepth = _waterDepth;
                }
                CheckIfDeepWater();
                CheckUkc();
            }
        }

        public DateTime WaypointEta
        {
            get
            {
                return _waypointEta;
            }
            set
            {
                if (_waypointEta == value)
                {
                    return;
                }

                _waypointEta = value;
                RaisePropertyChanged("WaypointEta");
                IsDirty = true;
                
            }
        }

        public double MinUkcRequired
        {
            get
            {
                return _minUkcRequired;
            }
            set
            {
                if (_minUkcRequired == value)
                {
                    return;
                }

                _minUkcRequired = value;
                IsDirty = true;
                RaisePropertyChanged("MinUkcRequired");

                SquatUkc.Calculate();
                CheckUkc();
            }
        }


        public double ChannelBeam
        {
            get
            {
                return _channelBeam;
            }
            set
            {
                if (_channelBeam == value)
                {
                    return;
                }

                _channelBeam = value;
                RaisePropertyChanged("ChannelBeam");
                IsDirty = true;

                SquatUkc.ChannelBeam = _channelBeam;

                foreach (SquatUkcItem item in SquatUkcConstSpeedVariousDepths)
                {
                    item.ChannelBeam = _channelBeam;
                }

                foreach (SquatUkcItem item in SquatUkcConstDepthVariousSpeeds)
                {
                    item.ChannelBeam = _channelBeam;
                }

                CheckUkc();
            }
        }


        public double VesselDraughtFore
        {
            get
            {
                return _vesselDraughtFore;
            }
            set
            {
                if (_vesselDraughtFore == value)
                {
                    return;
                }

                _vesselDraughtFore = value;
                RaisePropertyChanged("VesselDraughtFore");
                IsDirty = true;
                SetMaxAndMeanVesselDraught();
                SetBlockCoefficient();
                CheckIfDeepWater();
                CheckUkc();
 
            }
        }

        public double VesselDraughtAft
        {
            get
            {
                return _vesselDraughtAft;
            }
            set
            {
                if (_vesselDraughtAft == value)
                {
                    return;
                }

                _vesselDraughtAft = value;
                RaisePropertyChanged("VesselDraughtAft");
                IsDirty = true;
                SetMaxAndMeanVesselDraught();
                SetBlockCoefficient();
                CheckIfDeepWater();
                CheckUkc();
            }
        }

        public double VesselDraughtMid
        {
            get
            {
                return _vesselDraughtMid;
            }
            set
            {
                if (_vesselDraughtMid == value)
                {
                    return;
                }

                _vesselDraughtMid = value;
                IsDirty = true;
                RaisePropertyChanged("VesselDraughtMid");
            }
        }

        public double VesselDraughtMax
        {
            get
            {
                return _vesselDraughtMax;
            }
            set
            {
                if (_vesselDraughtMax == value)
                {
                    return;
                }

                _vesselDraughtMax = value;
                IsDirty = true;
                RaisePropertyChanged("VesselDraughtMax");
            }
        }

        //public DispatchingObservableCollection<NavigationalBridgeWatchCondition> NavigationalBridgeWatchConditions
        //{
        //    get
        //    {
        //        return _navigationalBridgeWatchConditions;
        //    }
        //    set
        //    {
        //        if (_navigationalBridgeWatchConditions == value)
        //        {
        //            return;
        //        }

        //        _navigationalBridgeWatchConditions = value;
        //        IsDirty = true;
        //        RaisePropertyChanged("NavigationalBridgeWatchConditions");
        //    }
        //}



        public double BlockCoefficient
        {
            get
            {
                return _blockCoefficient;
            }
            set
            {
                if (_blockCoefficient == value)
                {
                    return;
                }

                _blockCoefficient = value;
                RaisePropertyChanged("BlockCoefficient");
                IsDirty = true;

                SquatUkc.BlockCoefficient = _blockCoefficient;

                foreach (SquatUkcItem item in SquatUkcConstSpeedVariousDepths)
                {
                    item.BlockCoefficient = _blockCoefficient;
                }

                foreach (SquatUkcItem item in SquatUkcConstDepthVariousSpeeds)
                {
                    item.BlockCoefficient = _blockCoefficient;
                }
                
                CheckUkc();
            }
        }

        public double VesselBeam
        {
            get
            {
                return _vesselBeam;
            }
            set
            {
                if (_vesselBeam == value)
                {
                    return;
                }

                _vesselBeam = value;
                IsDirty = true;
                RaisePropertyChanged("VesselBeam");

                if (SquatUkc != null)
                {
                    SquatUkc.VesselBeam = _vesselBeam;
                
                    foreach (SquatUkcItem item in SquatUkcConstSpeedVariousDepths)
                    {
                        item.VesselBeam = _vesselBeam;
                    }

                    foreach (SquatUkcItem item in SquatUkcConstDepthVariousSpeeds)
                    {
                        item.VesselBeam = _vesselBeam;
                    }

                    CheckUkc();
                }
            }
        }

        #region Corrections

        public double EffectOfSwell
        {
            get
            {
                return _effectOfSwell;
            }
            set
            {
                if (_effectOfSwell == value)
                {
                    return;
                }

                _effectOfSwell = value;
                IsDirty = true;
                CalculateTotalDepthCorrection();
                RaisePropertyChanged("EffectOfSwell");
            }
        }

        public double TidalHeight
        {
            get
            {
                return _tidalHeight;
            }
            set
            {
                if (_tidalHeight == value)
                {
                    return;
                }

                _tidalHeight = value;
                IsDirty = true;
                CalculateTotalDepthCorrection();
                RaisePropertyChanged("TidalHeight");
            }
        }

        public double DivergenceFromPredictedTide
        {
            get
            {
                return _divergenceFromPredictedTide;
            }
            set
            {
                if (_divergenceFromPredictedTide == value)
                {
                    return;
                }

                _divergenceFromPredictedTide = value;
                IsDirty = true;
                CalculateTotalDepthCorrection();
                RaisePropertyChanged("DivergenceFromPredictedTide");
            }
        }

        public double HydrographicSurveyTolerance
        {
            get
            {
                return _hydrographicSurveyTolerance;
            }
            set
            {
                if (_hydrographicSurveyTolerance == value)
                {
                    return;
                }

                _hydrographicSurveyTolerance = value;
                IsDirty = true;
                CalculateTotalDepthCorrection();
                RaisePropertyChanged("HydrographicSurveyTolerance");
            }
        }

        public double EffectFromMeteorologicConditions
        {
            get
            {
                return _effectFromMeteorologicConditions;
            }
            set
            {
                if (_effectFromMeteorologicConditions == value)
                {
                    return;
                }

                _effectFromMeteorologicConditions = value;
                IsDirty = true;
                CalculateTotalDepthCorrection();
                RaisePropertyChanged("EffectFromMeteorologicConditions");
            }
        }

        public double IncreasedDraughtDueToReducedWaterDensity
        {
            get
            {
                return _increasedDraughtDueToReducedWaterDensity;
            }
            set
            {
                if (_increasedDraughtDueToReducedWaterDensity == value)
                {
                    return;
                }

                _increasedDraughtDueToReducedWaterDensity = value;
                IsDirty = true;
                CalculateTotalDraughtCorrection();
                RaisePropertyChanged("IncreasedDraughtDueToReducedWaterDensity");
            }
        }

        public double WaterDensity
        {
            get
            {
                return _waterDensity;
            }
            set
            {
                if (_waterDensity == value)
                {
                    return;
                }

                _waterDensity = value;
                IsDirty = true;
                RaisePropertyChanged("WaterDensity");
            }
        }

        public double ListInDegrees
        {
            get
            {
                return _listInDegrees;
            }
            set
            {
                if (_listInDegrees == value)
                {
                    return;
                }

                _listInDegrees = Math.Abs(value);
                IsDirty = true;
                RaisePropertyChanged("ListInDegrees");
                CalculateEffectFromList();
            }
        }

        public double EffectFromList
        {
            get
            {
                return _effectFromList;
            }
            set
            {
                if (_effectFromList == value)
                {
                    return;
                }

                _effectFromList = value;
                IsDirty = true;
                CalculateTotalDraughtCorrection();
                RaisePropertyChanged("EffectFromList");
            }
        }

        public double TotalDraughtCorrection
        {
            get
            {
                return _totalDraughtCorrection;
            }
            set
            {
                if (_totalDraughtCorrection == value)
                {
                    return;
                }

                _totalDraughtCorrection = value;
                IsDirty = true;
                RaisePropertyChanged("TotalDraughtCorrection");
                SquatUkc.DraughtCorrection = _totalDraughtCorrection;
                TotalDraughtAndDepthCorrection = _totalDraughtCorrection + _totalDepthCorrection;
                CheckUkc();
            }
        }

        public double TotalDepthCorrection
        {
            get
            {
                return _totalDepthCorrection;
            }
            set
            {
                if (_totalDepthCorrection == value)
                {
                    return;
                }

                _totalDepthCorrection = value;
                IsDirty = true;
                RaisePropertyChanged("TotalDepthCorrection");
                SquatUkc.DepthCorrection = _totalDepthCorrection;
                TotalDraughtAndDepthCorrection = _totalDraughtCorrection + _totalDepthCorrection;
                CheckUkc();
            }
        }

        public double TotalDraughtAndDepthCorrection
        {
            get
            {
                return _totalDraughtAndDepthCorrection;
            }
            set
            {
                if (_totalDraughtAndDepthCorrection == value)
                {
                    return;
                }

                _totalDraughtAndDepthCorrection = value;
                RaisePropertyChanged("TotalDraughtAndDepthCorrection");
                SquatUkc.DraughtAndDepthCorrection = _totalDraughtAndDepthCorrection;
            }
        }

        #endregion

        public string UkcStatusText
        {
            get
            {
                return _ukcStatusText;
            }
            set
            {
                if (_ukcStatusText == value)
                {
                    return;
                }

                _ukcStatusText = value;
                RaisePropertyChanged("UkcStatusText");
            }
        }

        public SquatUkcItem SquatUkc
        {
            get
            {
                return _squatUkc;
            }

            set
            {
                if (_squatUkc == value)
                {
                    return;
                }

                _squatUkc = value;
                RaisePropertyChanged("SquatUkc");
            }
        }

        public DispatchingObservableCollection<SquatUkcItem> SquatUkcConstSpeedVariousDepths
        {
            get
            {
                return _squatUkcConstSpeedVariousDepths;
            }

            set
            {
                if (_squatUkcConstSpeedVariousDepths == value)
                {
                    return;
                }

                _squatUkcConstSpeedVariousDepths = value;
                RaisePropertyChanged("SquatUkcConstSpeedVariousDepths");
            }
        }

        public DispatchingObservableCollection<SquatUkcItem> SquatUkcConstDepthVariousSpeeds
        {
            get
            {
                return _squatUkcConstDepthVariousSpeeds;
            }

            set
            {
                if (_squatUkcConstDepthVariousSpeeds == value)
                {
                    return;
                }

                _squatUkcConstDepthVariousSpeeds = value;
                RaisePropertyChanged("SquatUkcConstDepthVariousSpeeds");
            }
        }

      

        private void CalculateTotalDepthCorrection()
        {
            TotalDepthCorrection = EffectOfSwell - TidalHeight + DivergenceFromPredictedTide + HydrographicSurveyTolerance + EffectFromMeteorologicConditions;     
        }

        private void CalculateTotalDraughtCorrection()
        {
            TotalDraughtCorrection = IncreasedDraughtDueToReducedWaterDensity + EffectFromList; 
        }

        private void CalculateEffectFromList()
        {
            if (ListInDegrees == 0)
            {
                EffectFromList = 0;
            }
            else if (VesselDraughtMid > 0)
            {
                EffectFromList = (VesselBeam * Math.Sin(_listInDegrees * Math.PI / 180.0) / 2.0) + (VesselDraughtMid * Math.Cos(_listInDegrees * Math.PI / 180.0)) - VesselDraughtMid;
            }
        }


        private void SetMaxAndMeanVesselDraught()
        {
            VesselDraughtMax = Math.Max(VesselDraughtFore, VesselDraughtAft);

            if (VesselDraughtFore > 0.0 && VesselDraughtAft > 0.0)
            {
                VesselDraughtMid = (VesselDraughtFore + VesselDraughtAft) / 2.0;
            }
            else
            {
                VesselDraughtMid = VesselDraughtMax;
            }

            // Evaluate the greatest depth which will be present at the very left column in the Squat Calculation Report
            int maxDepthInSquatCalculations = Convert.ToInt32(Math.Truncate(2 * VesselDraughtMax));

            SquatUkc.MaxDraughtVessel = VesselDraughtMax;
            SquatUkc.MeanDraughtVessel = VesselDraughtMid;


            int i = maxDepthInSquatCalculations;

            foreach (SquatUkcItem item in SquatUkcConstSpeedVariousDepths)
            {
                item.MaxDraughtVessel = VesselDraughtMax;
                item.MeanDraughtVessel = VesselDraughtMid;
                if (i > VesselDraughtMid)
                {
                    item.WaterDepth = i--;
                }
                else
                {
                    item.WaterDepth = null;
                }
            }


            foreach (SquatUkcItem item in SquatUkcConstDepthVariousSpeeds)
            {
                item.MaxDraughtVessel = VesselDraughtMax;
                item.MeanDraughtVessel = VesselDraughtMid;
            }

            CalculateEffectFromList();
        }

        private void SetBlockCoefficient()
        {
            if (VesselDraughtFore < EPSILON && VesselDraughtAft < EPSILON)
            {
                // No idea to set block coefficient based on draught
                return;
            }
            else
            {
                double draught = 0.0;

                if (VesselDraughtAft < EPSILON)
                {
                    draught = VesselDraughtFore;
                }
                else if (VesselDraughtFore < EPSILON)
                {
                    draught = VesselDraughtAft;
                }
                else
                {
                    // Standard case
                    draught = (VesselDraughtFore + VesselDraughtAft) / 2.0;
                }

                try
                {
                    ViewModelLocator locator = new ViewModelLocator();
                    VezzelViewModel vesselVM = locator.VesselVM;
                    BlockCoefficient = vesselVM.GetBlockCoefficientAtDraught(draught);
                }
                catch (Exception ex)
                {
                    ErrorHandler.Show(ex);
                }
            }
        }

        private void CheckIfDeepWater()
        {
            string deepWaterText = "The water depth is twice the deepest draught of your vessel, or more.";

            if ((VesselDraughtFore > 0.0 || VesselDraughtAft > 0.0) && WaterDepth > 0.0)
            {
                double maxDraught = Math.Max(VesselDraughtFore, VesselDraughtAft);

                if (WaterDepth >= 2 * maxDraught)
                {
                    UkcStatusText = deepWaterText;
                }
                else
                {
                    if (UkcStatusText == deepWaterText)
                    {
                        UkcStatusText = string.Empty;
                    }
                }
            }
        }

        private void CheckUkc()
        {
            string ukcGroundingText = "GROUNDING!";
            string ukcWarningText = "WARNING! Under Keel Clearance is below minimum requirement!";

            if (SquatUkc.UkcOpenWaterCorrected < 0.0 || (ChannelBeam > 0.0 && SquatUkc.UkcChannelCorrected < 0.0))
            {
                UkcStatusText = ukcGroundingText;
            }
            else if (SquatUkc.UkcOpenWaterCorrected < MinUkcRequired || (ChannelBeam > 0.0 && SquatUkc.UkcChannelCorrected < MinUkcRequired))
            {
                UkcStatusText = ukcWarningText;
            }
            else if (UkcStatusText == ukcGroundingText || UkcStatusText == ukcWarningText)
            {
                UkcStatusText = string.Empty;
            }
        }
    }
}