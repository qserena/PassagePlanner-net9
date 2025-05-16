using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Seaware.Navigation
{
    public interface IPosition
    {
        double Latitude { get; }
        double Longitude { get; }
    }
}
