namespace HVO.Power.OutbackMate
{
    /// <summary>
    /// Operational mode of an Outback FX-series inverter/charger.
    /// </summary>
    public enum OutbackFxInverterChargerOperationalMode
    {
        /// <summary>Inverter is off.</summary>
        InverterOff = 0,

        /// <summary>Inverter is in search mode (low-power standby).</summary>
        Search = 1,

        /// <summary>Inverter is on and producing AC output.</summary>
        InverterOn = 2,

        /// <summary>Battery charging from AC input.</summary>
        Charge = 3,

        /// <summary>Silent mode (inverter off, charger off).</summary>
        Silent = 4,

        /// <summary>Float charging stage.</summary>
        Float = 5,

        /// <summary>Equalization charging stage.</summary>
        Equalize = 6,

        /// <summary>Charger is off.</summary>
        ChargerOff = 7,

        /// <summary>Support mode (AC supplement).</summary>
        Support = 8,

        /// <summary>Sell-enabled mode (grid-tie export).</summary>
        SellEnabled = 9,

        /// <summary>AC pass-through (no inversion or charging).</summary>
        PassThru = 10,

        /// <summary>FX hardware error.</summary>
        FxError = 90,

        /// <summary>AGS (Automatic Generator Start) error.</summary>
        AgsError = 91,

        /// <summary>Communications error with the Mate controller.</summary>
        CommunicationsError = 92
    }
}
