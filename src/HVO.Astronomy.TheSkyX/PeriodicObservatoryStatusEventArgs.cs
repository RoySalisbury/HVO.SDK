using System;
using System.Collections.Generic;
using System.Text;

namespace HVO.Astronomy.TheSkyX
{
    public class PeriodicObservatoryStatusEventArgs : EventArgs
    {
        public PeriodicObservatoryStatusEventArgs(TimeSpan localSiderealTime)
        {
            this.LocalSiderealTime = localSiderealTime;
        }

        public TimeSpan LocalSiderealTime
        {
            get; set;
        }
    }
}
