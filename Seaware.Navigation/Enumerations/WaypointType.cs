using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Seaware.Navigation.Enumerations
{
    /// <summary>
    /// This enumeration specifies possible waypoint types.
    /// </summary>
    public enum WaypointType
    {
        /// <summary>
        /// Waypoint defined by user (original waypoint).
        /// </summary>
        User,
        /// <summary>
        /// Waypoint created by optimization.
        /// </summary>
        Opt,
        /// <summary>
        /// Interpolated waypoint for which weather and performance is calculated.
        /// </summary>
        Perf,
        /// <summary>
        /// Waypoint for which a stoppage time larger than zero has been set.
        /// </summary>
        Stop,
    }
}
