namespace HVO.Power.OutbackMate
{
    /// <summary>
    /// Auxiliary relay mode for an Outback charge controller.
    /// </summary>
    public enum OutbackMateChargeControllerAuxRelayMode
    {
        /// <summary>Auxiliary relay is disabled.</summary>
        Disabled = 0,

        /// <summary>Diversion mode (excess energy diversion).</summary>
        Diversion = 1,

        /// <summary>Remote control mode.</summary>
        Remote = 2,

        /// <summary>Manual override mode.</summary>
        Manual = 3,

        /// <summary>Ventilation fan control.</summary>
        VentFan = 4,

        /// <summary>PV voltage trigger mode.</summary>
        PVTrigger = 5,

        /// <summary>Float stage trigger mode.</summary>
        Float = 6,

        /// <summary>Error output indicator.</summary>
        ErrorOutput = 7,

        /// <summary>Night-light mode (on at dark, off at dawn).</summary>
        NightLight = 8,

        /// <summary>PWM diversion mode.</summary>
        PWMDiversion = 9,

        /// <summary>Low battery disconnect mode.</summary>
        LowBattery = 10
    }
}
