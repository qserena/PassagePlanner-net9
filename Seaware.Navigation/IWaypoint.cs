using Seaware.Navigation.Enumerations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Seaware.Navigation
{
    /// <summary>
    /// Interface for waypoint types
    /// </summary>
    public interface IWaypoint
    {
        /// <summary>
        /// Predicted time at waypoint, in UTC. Ignored for route evaluations and optimizations. This property is optional.
        /// </summary>
        DateTime? Time { get; set; }

        IPosition Position { get; set; }
        
        /// <summary>
        /// Calm water speed that corresponds to the intended engine setting, for the leg following this waypoint [knots]. This property is optional.
        /// </summary>
        double? LegSpeedSetting { get; set; }

        double? LegRpmSetting { get; set; }

        double? LegPowerSetting { get; set; }

        /// <summary>
        /// The type of the leg following this waypoint. For LegTypeEnum.Transit, the LegIsSpeedOptimizable and LegIsTrackOptimizable properties will be ignored.
        /// </summary>
        LegType LegType { get; set; }

        /// <summary>
        /// Tells if the leg following this waypoint should be optimizable with respect to speed.
        /// </summary>
        /// <value>
        /// <code>true</code> if the leg should be speed optimizable, otherwise <code>false</code>
        /// </value>
        bool LegIsSpeedOptimizable { get; set; }
        
        /// <summary>
        /// Tells if the leg following this waypoint should be optimizable with respect to geographical track.
        /// </summary>
        /// <value>
        /// <code>true</code> if the leg should be track optimizable, otherwise <code>false</code>
        /// </value>
        bool LegIsTrackOptimizable { get; set; }

        /// <summary>
        /// Type of waypoint
        /// </summary>
        WaypointType WaypointType { get; set; }
        /// <summary>
        /// Time duration of stoppage or transit. Only required for waypoints of WaypointTypeEnum.Stop type.
        /// </summary>
        TimeSpan StoppageOrTransitDuration { get; set; } // [h]

        IFuelUsage FuelUsage { get; set; }
    }
}
