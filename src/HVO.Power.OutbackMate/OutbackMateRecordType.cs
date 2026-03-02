namespace HVO.Power.OutbackMate
{
    /// <summary>
    /// Identifies the type of device that produced an Outback Mate record.
    /// </summary>
    public enum OutbackMateRecordType
    {
        /// <summary>Unknown or unrecognized device type.</summary>
        Unknown,

        /// <summary>Outback charge controller (e.g., FM60, FM80).</summary>
        ChargeController,

        /// <summary>Outback FlexNet DC battery monitor.</summary>
        FlexNetDC,

        /// <summary>Outback FX-series inverter/charger.</summary>
        InverterCharger
    }
}
