//using GalaSoft.MvvmLight;
//using GalaSoft.MvvmLight.Command;


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
using System.Configuration;

namespace PassagePlanner
{
    /// <summary>
    /// Object representing the vessel data. Made specifically to be serialized to VesselData.xml.
    /// </summary>
    [Serializable]
    public class VesselData
    {
        public string VesselName { set; get; }
        public double VesselBeam { set; get; }
        public double MinUkcRequired { set; get; }
        public List<NavigationalBridgeWatchCondition> NavigationalBridgeWatchConditions { set; get; }
        public List<BlockCoefficientAtDraught> BlockCoefficients { set; get; }
    }
}