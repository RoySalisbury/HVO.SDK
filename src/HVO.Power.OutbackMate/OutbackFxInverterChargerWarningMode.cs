using System;

namespace HVO.Power.OutbackMate
{
    /// <summary>
    /// Warning flags for an Outback FX-series inverter/charger.
    /// Multiple flags may be set simultaneously.
    /// </summary>
    [Flags]
    public enum OutbackFxInverterChargerWarningMode
    {
        /// <summary>No warnings.</summary>
        None = 0,

        /// <summary>AC input frequency is too high.</summary>
        ACInputFreqHigh = 1,

        /// <summary>AC input frequency is too low.</summary>
        ACInputFreqLow = 2,

        /// <summary>AC input voltage is too high.</summary>
        ACInputVoltageHigh = 4,

        /// <summary>AC input voltage is too low.</summary>
        ACInputVoltageLow = 8,

        /// <summary>Buy current exceeds the configured input breaker size.</summary>
        BuyAmpsGreaterThanInputSize = 16,

        /// <summary>Temperature sensor has failed.</summary>
        TemperatureSensorFailed = 32,

        /// <summary>Communications error with the Mate controller.</summary>
        CommunicationsError = 64,

        /// <summary>Cooling fan failure detected.</summary>
        FanFailure = 128
    }
}
