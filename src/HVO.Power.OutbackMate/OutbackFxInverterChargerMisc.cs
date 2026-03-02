using System;

namespace HVO.Power.OutbackMate
{
    /// <summary>
    /// Miscellaneous status flags for an Outback FX-series inverter/charger.
    /// </summary>
    [Flags]
    public enum OutbackFxInverterChargerMisc
    {
        /// <summary>No flags set.</summary>
        None = 0,

        /// <summary>Unit is configured for 240 V AC output.</summary>
        AC240 = 1,

        /// <summary>Reserved (unused).</summary>
        Reserved1 = 2,

        /// <summary>Reserved (unused).</summary>
        Reserved2 = 4,

        /// <summary>Reserved (unused).</summary>
        Reserved3 = 8,

        /// <summary>Reserved (unused).</summary>
        Reserved4 = 16,

        /// <summary>Reserved (unused).</summary>
        Reserved5 = 32,

        /// <summary>Reserved (unused).</summary>
        Reserved6 = 64,

        /// <summary>Auxiliary output relay is energized.</summary>
        AuxOutputOn = 128
    }
}
