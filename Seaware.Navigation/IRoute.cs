using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Seaware.Navigation
{
    public interface IRoute
    {
        /// <summary>
        /// An identifier of the route.
        /// </summary>
        string RouteId { get; set; }
        /// <summary>
        /// Estimated time of departure from the first waypoint, in UTC.
        /// </summary>
        DateTime ETD { get; set; }
        /// <summary>
        /// Default calm water speed that corresponds to the intended engine setting [knots]. Note that this speed setting can be overridden on individual legs.
        /// </summary>
        double RouteSpeedSetting { get; set; }
        /// <summary>
        /// Waypoint list.
        /// </summary>
        IList<IWaypoint> Waypoints { get; set; }
    }
}
