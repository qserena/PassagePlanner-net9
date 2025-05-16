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
    /// <summary>
    /// Class specially made for copy one or more Waypoints via Windows Clipboard.
    /// This class was necessary because it did not work to copy the Waypoint class via Clipboard, 
    /// because it implemented the IPropertyChanged interface. Such a class cannot be properly Serialized.
    /// </summary>
    [Serializable]
    public class WaypointForClipboard
    {
        // Casting operator
        public static explicit operator Waypoint(WaypointForClipboard instance)
        {
            return new Waypoint(instance);
        }

        // Constructor made for casting operator above
        public WaypointForClipboard(Waypoint wp)
        {
            WaypointName = wp.WaypointName;
            Latitude = wp.Latitude;
            Longitude = wp.Longitude;
            LegType = wp.LegType;
            LegSpeedSetting = wp.LegSpeedSetting;
            Remarks = wp.Remarks;
            MinDepth = wp.MinDepth;
            Charts = wp.Charts;
            LegReferenceObject_Object = wp.LegReferenceObject_Object;
            LegReferenceObject_Bearing = wp.LegReferenceObject_Bearing;
            LegReferenceObject_Distance = wp.LegReferenceObject_Distance;
            TurnRadius = wp.TurnRadius;
            TurnRate = wp.TurnRate;
            LandmarkAtCourseAlt_Object = wp.LandmarkAtCourseAlt_Object;
            LandmarkAtCourseAlt_Bearing = wp.LandmarkAtCourseAlt_Bearing;
            LandmarkAtCourseAlt_Distance = wp.LandmarkAtCourseAlt_Distance;
            MaxOffTrack = wp.MaxOffTrack;
            MaxIntervalsPosFix = wp.MaxIntervalsPosFix;
            EngineStatus = wp.EngineStatus;
            NavWatchLevel = wp.NavWatchLevel;
            PosFixMethod = wp.PosFixMethod;
            ListOfLights_Volume = wp.ListOfLights_Volume;
            ListOfLights_Page = wp.ListOfLights_Page;
            ListOfRadioSignals_Volume = wp.ListOfRadioSignals_Volume;
            ListOfRadioSignals_Page = wp.ListOfRadioSignals_Page;
            SailingDirections_Volume = wp.SailingDirections_Volume;
            SailingDirections_Page = wp.SailingDirections_Page;
            NavtexChannels = wp.NavtexChannels;
            ReportTo = wp.ReportTo;
            ChannelOrTelephoneNo = wp.ChannelOrTelephoneNo;
            ActualPassingTime = wp.ActualPassingTime;
            SecurityLevel = wp.SecurityLevel;
            ToolTip = wp.ToolTip;
        }

        public string WaypointName;
        public double Latitude;
        public double Longitude;
        public LegType LegType;
        public double? LegSpeedSetting;
        public string Remarks;
        public double MinDepth;
        public string Charts;
        public string LegReferenceObject_Object;
        public string LegReferenceObject_Bearing;
        public string LegReferenceObject_Distance;
        public double TurnRadius;
        public string TurnRate;
        public string LandmarkAtCourseAlt_Object;
        public string LandmarkAtCourseAlt_Bearing;
        public string LandmarkAtCourseAlt_Distance;
        public string MaxOffTrack;
        public string MaxIntervalsPosFix;
        public string EngineStatus;
        public string NavWatchLevel;
        public string PosFixMethod;
        public string ListOfLights_Volume;
        public string ListOfLights_Page;
        public string ListOfRadioSignals_Volume;
        public string ListOfRadioSignals_Page;
        public string SailingDirections_Volume;
        public string SailingDirections_Page;
        public string NavtexChannels;
        public string ReportTo;
        public string ChannelOrTelephoneNo;
        public string ActualPassingTime;
        public string SecurityLevel;
        public string ToolTip;
    }
}

