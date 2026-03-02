using System.ComponentModel;

namespace HVO.Weather.DavisVantagePro
{
    /// <summary>
    /// Represents the barometric pressure trend reported by a Davis Vantage Pro weather station.
    /// </summary>
    /// <remarks>
    /// Values correspond to the raw trend byte in the LOOP packet at offset 3.
    /// </remarks>
    public enum BarometerTrend : short
    {
        /// <summary>Unknown barometer trend (raw value 80).</summary>
        [Description("")]
        Unknown = 80,

        /// <summary>Barometer trend data is unavailable.</summary>
        [Description("")]
        Unavailable = short.MaxValue,

        /// <summary>Pressure is falling rapidly (≥ −0.06 inHg/3h).</summary>
        [Description("\u21D3")]
        FallingRapidly = 196,

        /// <summary>Pressure is falling slowly (≥ −0.02 inHg/3h).</summary>
        [Description("\u2193")]
        FallingSlowly = 236,

        /// <summary>Pressure is steady.</summary>
        [Description("\u2194")]
        Steady = 0,

        /// <summary>Pressure is rising slowly (≥ +0.02 inHg/3h).</summary>
        [Description("\u2191")]
        RisingSlowly = 20,

        /// <summary>Pressure is rising rapidly (≥ +0.06 inHg/3h).</summary>
        [Description("\u21D1")]
        RisingRapidly = 60
    }
}
