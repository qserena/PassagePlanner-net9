using Seaware.Navigation.Enumerations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Seaware.Navigation
{
    /// <summary>
    /// The options for the burning of fuel that may change during a voyage
    /// </summary>
    public interface IFuelUsage
    {
        /// <summary>
        /// Determines the priority between boil off and oil
        /// </summary>
        /// 
        FuelBurningPlan FuelBurningPlan { get; }

        /// <summary>
        /// The amount of boil off avaliable at a leg [kg]
        /// </summary>
        double BoilOff { get; }

        /// <summary>
        /// Determines which fuel type that is burned at a leg
        /// </summary>
        OilType OilType { get; }
    }


}
