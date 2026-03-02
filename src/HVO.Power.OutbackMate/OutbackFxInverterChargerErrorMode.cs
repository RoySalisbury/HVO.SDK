using System;

namespace HVO.Power.OutbackMate
{
    /// <summary>
    /// Error flags for an Outback FX-series inverter/charger.
    /// Multiple flags may be set simultaneously.
    /// </summary>
    [Flags]
    public enum OutbackFxInverterChargerErrorMode
    {
        /// <summary>No errors.</summary>
        None = 0,

        /// <summary>Low AC output voltage.</summary>
        LowVoltageACOutput = 1,

        /// <summary>Stacking error (multi-unit configuration).</summary>
        StackingError = 2,

        /// <summary>Over-temperature condition.</summary>
        OverTemperature = 4,

        /// <summary>Low battery voltage.</summary>
        LowBattery = 8,

        /// <summary>Phase loss detected (split-phase systems).</summary>
        PhaseLoss = 16,

        /// <summary>High battery voltage.</summary>
        HighBattery = 32,

        /// <summary>Shorted AC output.</summary>
        ShortedOutput = 64,

        /// <summary>Back-feed detected on AC input.</summary>
        BackFeed = 128
    }
}
