namespace HVO.Power.OutbackMate
{
    /// <summary>
    /// AC input mode for an Outback FX-series inverter/charger.
    /// </summary>
    public enum OutbackFxInverterChargerACInputMode
    {
        /// <summary>No AC input detected.</summary>
        None = 0,

        /// <summary>AC input detected but dropped (not qualifying).</summary>
        Drop = 1,

        /// <summary>AC input detected and in use.</summary>
        Use = 2
    }
}
