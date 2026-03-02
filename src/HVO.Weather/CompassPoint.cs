using System.ComponentModel;

namespace HVO.Weather
{
    /// <summary>
    /// Represents the 16 cardinal and intercardinal compass points with their approximate degree values.
    /// </summary>
    /// <remarks>
    /// Values use rounded integer degrees (e.g., NNE = 22 instead of 22.5) which matches
    /// the Davis Vantage Pro hardware protocol encoding.
    /// </remarks>
    public enum CompassPoint
    {
        /// <summary>North (0°).</summary>
        [Description("N")]
        N = 0,

        /// <summary>North-northeast (22°).</summary>
        [Description("NNE")]
        NNE = 22,

        /// <summary>Northeast (45°).</summary>
        [Description("NE")]
        NE = 45,

        /// <summary>East-northeast (68°).</summary>
        [Description("ENE")]
        ENE = 68,

        /// <summary>East (90°).</summary>
        [Description("E")]
        E = 90,

        /// <summary>East-southeast (112°).</summary>
        [Description("ESE")]
        ESE = 112,

        /// <summary>Southeast (135°).</summary>
        [Description("SE")]
        SE = 135,

        /// <summary>South-southeast (158°).</summary>
        [Description("SSE")]
        SSE = 158,

        /// <summary>South (180°).</summary>
        [Description("S")]
        S = 180,

        /// <summary>South-southwest (202°).</summary>
        [Description("SSW")]
        SSW = 202,

        /// <summary>Southwest (225°).</summary>
        [Description("SW")]
        SW = 225,

        /// <summary>West-southwest (248°).</summary>
        [Description("WSW")]
        WSW = 248,

        /// <summary>West (270°).</summary>
        [Description("W")]
        W = 270,

        /// <summary>West-northwest (292°).</summary>
        [Description("WNW")]
        WNW = 292,

        /// <summary>Northwest (315°).</summary>
        [Description("NW")]
        NW = 315,

        /// <summary>North-northwest (338°).</summary>
        [Description("NNW")]
        NNW = 338
    }
}
