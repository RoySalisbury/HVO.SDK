using System;
using System.Globalization;
using System.Text;

namespace HVO.Weather
{
    /// <summary>
    /// Builds Weather Underground Personal Weather Station (PWS) upload URLs for submitting
    /// weather observations to the Weather Underground API.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Formats the query string for the WU <c>updateweatherstation.php</c> endpoint.
    /// The caller is responsible for making the HTTP GET request (application concern).
    /// </para>
    /// <para>
    /// API reference: <see href="https://support.weather.com/s/article/PWS-Upload-Protocol">WU PWS Upload Protocol</see>
    /// </para>
    /// </remarks>
    public static class WeatherUndergroundFormatter
    {
        /// <summary>
        /// The WU upload endpoint base URL.
        /// </summary>
        public const string BaseUrl = "https://weatherstation.wunderground.com/weatherstation/updateweatherstation.php";

        /// <summary>
        /// Formats a complete Weather Underground upload URL from the given observation.
        /// </summary>
        /// <param name="observation">The weather observation data.</param>
        /// <returns>A complete URL string ready for HTTP GET submission.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="observation"/> is <c>null</c>.</exception>
        public static string FormatUrl(WundergroundObservation observation)
        {
            if (observation == null) throw new ArgumentNullException(nameof(observation));
            return string.Concat(BaseUrl, "?", FormatQueryString(observation));
        }

        /// <summary>
        /// Formats only the query string portion (without base URL) for the WU upload.
        /// </summary>
        /// <param name="observation">The weather observation data.</param>
        /// <returns>A URL query string (without leading '?').</returns>
        /// <exception cref="ArgumentNullException"><paramref name="observation"/> is <c>null</c>.</exception>
        public static string FormatQueryString(WundergroundObservation observation)
        {
            if (observation == null) throw new ArgumentNullException(nameof(observation));

            var sb = new StringBuilder(512);

            // Required fields
            Append(sb, "ID", observation.StationId);
            Append(sb, "PASSWORD", observation.Password);
            Append(sb, "action", "updateraw");
            Append(sb, "dateutc", Uri.EscapeDataString(observation.ObservationTimeUtc.ToUniversalTime().ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture)));
            Append(sb, "softwaretype", observation.SoftwareType ?? "Custom");

            // Indoor values
            AppendOptional(sb, "indoortempf", observation.IndoorTemperatureF);
            AppendOptional(sb, "indoorhumidity", observation.IndoorHumidityPercent);

            // Wind
            AppendOptional(sb, "windspeedmph", observation.WindSpeedMph);
            AppendOptional(sb, "winddir", observation.WindDirectionDegrees);
            AppendOptional(sb, "windspdmph_avg2m", observation.WindSpeedAvg2MinMph);
            AppendOptional(sb, "winddir_avg2m", observation.WindDirectionAvg2MinDegrees);
            AppendOptional(sb, "windgustmph", observation.WindGustMph);
            AppendOptional(sb, "windgustdir", observation.WindGustDirectionDegrees);
            AppendOptional(sb, "windgustmph_10m", observation.WindGust10MinMph);
            AppendOptional(sb, "windgustdir_10m", observation.WindGustDirection10MinDegrees);

            // Outdoor conditions
            AppendOptional(sb, "humidity", observation.OutdoorHumidityPercent);
            AppendOptional(sb, "dewptf", observation.DewPointF);
            AppendOptional(sb, "tempf", observation.OutdoorTemperatureF);

            // Rain
            AppendOptional(sb, "rainin", observation.RainLastHourInches);
            AppendOptional(sb, "dailyrainin", observation.DailyRainInches);

            // Pressure
            AppendOptional(sb, "baromin", observation.BarometerInHg);

            // Additional
            AppendOptional(sb, "weather", observation.Weather);
            AppendOptional(sb, "clouds", observation.Clouds);
            AppendOptional(sb, "soiltempf", observation.SoilTemperatureF);
            AppendOptional(sb, "soilmoisture", observation.SoilMoisturePercent);
            AppendOptional(sb, "leafwetness", observation.LeafWetnessPercent);
            AppendOptional(sb, "solarradiation", observation.SolarRadiationWm2);
            AppendOptional(sb, "UV", observation.UvIndex);
            AppendOptional(sb, "visibility", observation.VisibilityNauticalMiles);

            return sb.ToString();
        }

        private static void Append(StringBuilder sb, string key, string value)
        {
            if (sb.Length > 0) sb.Append('&');
            sb.Append(key);
            sb.Append('=');
            sb.Append(value ?? "");
        }

        private static void AppendOptional(StringBuilder sb, string key, double? value)
        {
            if (!value.HasValue) return;
            Append(sb, key, value.Value.ToString(CultureInfo.InvariantCulture));
        }

        private static void AppendOptional(StringBuilder sb, string key, string? value)
        {
            if (string.IsNullOrEmpty(value)) return;
            Append(sb, key, Uri.EscapeDataString(value!));
        }
    }

    /// <summary>
    /// Represents a complete weather observation for Weather Underground upload.
    /// </summary>
    /// <remarks>
    /// All temperature values are in Fahrenheit; rain values are in inches; pressure
    /// is in inches of mercury (inHg). See the WU PWS Upload Protocol for field descriptions.
    /// </remarks>
    public sealed class WundergroundObservation
    {
        // --- Required fields ---

        /// <summary>
        /// Gets or sets the Weather Underground station ID (e.g., "KAZKINGM12").
        /// </summary>
        public string StationId { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the WU station password/key.
        /// </summary>
        public string Password { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the observation time in UTC.
        /// </summary>
        public DateTimeOffset ObservationTimeUtc { get; set; }

        /// <summary>
        /// Gets or sets the software type identifier. Defaults to "Custom".
        /// </summary>
        public string SoftwareType { get; set; } = "Custom";

        // --- Indoor values ---

        /// <summary>
        /// Gets or sets the indoor temperature in degrees Fahrenheit, or <c>null</c> if not available.
        /// </summary>
        public double? IndoorTemperatureF { get; set; }

        /// <summary>
        /// Gets or sets the indoor humidity as a percentage (0–100), or <c>null</c> if not available.
        /// </summary>
        public double? IndoorHumidityPercent { get; set; }

        // --- Wind ---

        /// <summary>
        /// Gets or sets the instantaneous wind speed in mph, or <c>null</c> if not available.
        /// </summary>
        public double? WindSpeedMph { get; set; }

        /// <summary>
        /// Gets or sets the instantaneous wind direction in degrees (0–360), or <c>null</c> if not available.
        /// </summary>
        public double? WindDirectionDegrees { get; set; }

        /// <summary>
        /// Gets or sets the 2-minute average wind speed in mph, or <c>null</c> if not available.
        /// </summary>
        public double? WindSpeedAvg2MinMph { get; set; }

        /// <summary>
        /// Gets or sets the 2-minute average wind direction in degrees (0–360), or <c>null</c> if not available.
        /// </summary>
        public double? WindDirectionAvg2MinDegrees { get; set; }

        /// <summary>
        /// Gets or sets the current wind gust speed in mph, or <c>null</c> if not available.
        /// </summary>
        public double? WindGustMph { get; set; }

        /// <summary>
        /// Gets or sets the current wind gust direction in degrees (0–360), or <c>null</c> if not available.
        /// </summary>
        public double? WindGustDirectionDegrees { get; set; }

        /// <summary>
        /// Gets or sets the past 10-minute wind gust speed in mph, or <c>null</c> if not available.
        /// </summary>
        public double? WindGust10MinMph { get; set; }

        /// <summary>
        /// Gets or sets the past 10-minute wind gust direction in degrees (0–360), or <c>null</c> if not available.
        /// </summary>
        public double? WindGustDirection10MinDegrees { get; set; }

        // --- Outdoor conditions ---

        /// <summary>
        /// Gets or sets the outdoor humidity as a percentage (0–100), or <c>null</c> if not available.
        /// </summary>
        public double? OutdoorHumidityPercent { get; set; }

        /// <summary>
        /// Gets or sets the outdoor dew point in degrees Fahrenheit, or <c>null</c> if not available.
        /// </summary>
        public double? DewPointF { get; set; }

        /// <summary>
        /// Gets or sets the outdoor temperature in degrees Fahrenheit, or <c>null</c> if not available.
        /// </summary>
        public double? OutdoorTemperatureF { get; set; }

        // --- Rain ---

        /// <summary>
        /// Gets or sets the rain accumulation in the last 60 minutes in inches, or <c>null</c> if not available.
        /// </summary>
        public double? RainLastHourInches { get; set; }

        /// <summary>
        /// Gets or sets the daily rain accumulation in inches, or <c>null</c> if not available.
        /// </summary>
        public double? DailyRainInches { get; set; }

        // --- Pressure ---

        /// <summary>
        /// Gets or sets the barometric pressure in inches of mercury, or <c>null</c> if not available.
        /// </summary>
        public double? BarometerInHg { get; set; }

        // --- Additional ---

        /// <summary>
        /// Gets or sets the METAR-style weather condition (e.g., "+RA"), or <c>null</c> if not available.
        /// </summary>
        public string? Weather { get; set; }

        /// <summary>
        /// Gets or sets the cloud cover (e.g., "SKC", "FEW", "SCT", "BKN", "OVC"), or <c>null</c> if not available.
        /// </summary>
        public string? Clouds { get; set; }

        /// <summary>
        /// Gets or sets the soil temperature in degrees Fahrenheit, or <c>null</c> if not available.
        /// </summary>
        public double? SoilTemperatureF { get; set; }

        /// <summary>
        /// Gets or sets the soil moisture as a percentage, or <c>null</c> if not available.
        /// </summary>
        public double? SoilMoisturePercent { get; set; }

        /// <summary>
        /// Gets or sets the leaf wetness as a percentage, or <c>null</c> if not available.
        /// </summary>
        public double? LeafWetnessPercent { get; set; }

        /// <summary>
        /// Gets or sets the solar radiation in W/m², or <c>null</c> if not available.
        /// </summary>
        public double? SolarRadiationWm2 { get; set; }

        /// <summary>
        /// Gets or sets the UV index, or <c>null</c> if not available.
        /// </summary>
        public double? UvIndex { get; set; }

        /// <summary>
        /// Gets or sets the visibility in nautical miles, or <c>null</c> if not available.
        /// </summary>
        public double? VisibilityNauticalMiles { get; set; }
    }
}
