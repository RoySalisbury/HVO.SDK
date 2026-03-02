using System;

namespace HVO.Power.OutbackMate
{
    /// <summary>
    /// Error flags for an Outback charge controller.
    /// Multiple flags may be set simultaneously.
    /// </summary>
    [Flags]
    public enum OutbackMateChargeControllerErrorMode
    {
        /// <summary>No errors.</summary>
        None = 0,

        /// <summary>Reserved (unused).</summary>
        Reserved1 = 1,

        /// <summary>Reserved (unused).</summary>
        Reserved2 = 2,

        /// <summary>Reserved (unused).</summary>
        Reserved3 = 4,

        /// <summary>Reserved (unused).</summary>
        Reserved4 = 8,

        /// <summary>Reserved (unused).</summary>
        Reserved5 = 16,

        /// <summary>Shorted battery temperature sensor.</summary>
        ShortedBatterySensor = 32,

        /// <summary>Controller over-temperature.</summary>
        TooHot = 64,

        /// <summary>Open-circuit PV voltage is too high.</summary>
        HighVOC = 128
    }
}
