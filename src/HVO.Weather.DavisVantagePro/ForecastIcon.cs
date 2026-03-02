using System.ComponentModel;

namespace HVO.Weather.DavisVantagePro
{
    /// <summary>
    /// Represents the forecast icon flags reported by a Davis Vantage Pro weather station.
    /// </summary>
    /// <remarks>
    /// Values correspond to the raw forecast icon byte in the LOOP packet at offset 89.
    /// The original type name <c>ForcastIcon</c> contained a spelling error that has been
    /// corrected in this migration.
    /// </remarks>
    public enum ForecastIcon
    {
        /// <summary>Mostly Cloudy.</summary>
        [Description("Mostly Cloudy")]
        Cloud = 2,

        /// <summary>Mostly Cloudy, Rain within 12 hours.</summary>
        [Description("Mostly Cloudy, Rain within 12 hours.")]
        CloudRain = 3,

        /// <summary>Partially Cloudy.</summary>
        [Description("Partially Cloudy")]
        PartialSunCloud = 6,

        /// <summary>Partially Cloudy, Rain within 12 hours.</summary>
        [Description("Partially Cloudy, Rain within 12 hours.")]
        PartialSunCloudRain = 7,

        /// <summary>Mostly Clear.</summary>
        [Description("Mostly Clear")]
        Sun = 8,

        /// <summary>Mostly Cloudy, Snow within 12 hours.</summary>
        [Description("Mostly Cloudy, Snow within 12 hours.")]
        CloudSnow = 18,

        /// <summary>Mostly Cloudy, Rain or Snow within 12 hours.</summary>
        [Description("Mostly Cloudy, Rain or Snow within 12 hours.")]
        CloudRainSnow = 19,

        /// <summary>Partially Cloudy, Snow within 12 hours.</summary>
        [Description("Partially Cloudy, Snow within 12 hours.")]
        PartialSunCloudSnow = 22,

        /// <summary>Partially Cloudy, Rain or Snow within 12 hours.</summary>
        [Description("Partially Cloudy, Rain or Snow within 12 hours.")]
        PartialSunCloudRainSnow = 23
    }
}
