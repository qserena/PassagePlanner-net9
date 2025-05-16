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
    /// This class should probably be removed after Passage Planner version 1.0.4.
    /// See explanation in the class description of class VezzelViewModel.
    /// </summary>
    [Serializable]
    public class VesselViewModel
    {
        public string VesselName { set; get; }
        public double VesselBeam { set; get; }
        public double MinUkcRequired { set; get; }
        public DispatchingObservableCollection<NavigationalBridgeWatchCondition> NavigationalBridgeWatchConditions { set; get; }
        public DispatchingObservableCollection<BlockCoefficientAtDraught> BlockCoefficients { set; get; }
    }
}