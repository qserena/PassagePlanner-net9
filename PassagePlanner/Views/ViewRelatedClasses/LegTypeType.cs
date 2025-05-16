using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PassagePlanner
{
    /// <summary>
    /// Leg type for view (presentation layer)
    /// This is needed because Seaware.Navigation.Enumerations.LegType contains 
    /// value Transit, which we do not use in application PassagePlanner.
    /// </summary>
    public enum LegTypeType
    {
        RhumbLine,

        GreatCircle
    }
}
