using System;

namespace HVO.Weather
{
    /// <summary>
    /// Formats weather observation data into CWOP/APRS data records for transmission
    /// to the Citizen Weather Observer Program (CWOP) network.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Produces APRS-compliant packets per the APRS Protocol Reference (APRS101.PDF).
    /// Packet format: <c>CALLSIGN&gt;APRS,TCPXX*:@DDHHMMzLATITUDE/LONGITUDEwind...weather...DVs</c>
    /// </para>
    /// <para>
    /// Typical workflow:
    /// <list type="number">
    /// <item>Create a <see cref="CwopObservation"/> with the current weather data.</item>
    /// <item>Call <see cref="FormatPacket"/> to build the APRS data record.</item>
    /// <item>Open a TCP connection to <c>cwop.aprs.net:14580</c> and send the login then the packet (application concern).</item>
    /// </list>
    /// </para>
    /// </remarks>
    public static class CwopAprsFormatter
    {
        /// <summary>
        /// Default software type identifier appended to the APRS packet.
        /// </summary>
        public const string DefaultSoftwareType = "DVs";

        /// <summary>
        /// Formats a complete CWOP/APRS data record from the given observation.
        /// </summary>
        /// <param name="observation">The weather observation data.</param>
        /// <returns>A complete APRS data record string ready for transmission.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="observation"/> is <c>null</c>.</exception>
        public static string FormatPacket(CwopObservation observation)
        {
            if (observation == null) throw new ArgumentNullException(nameof(observation));

            double barometricPressureTenthsMb = ComputeAltimeterTenthsMillibars(
                observation.BarometerInHg,
                observation.StationElevationFeet,
                observation.OutsideTemperatureF,
                observation.OutsideHumidityPercent);

            return string.Format("{0}>APRS,TCPXX*:@{1}z{2}/{3}_{4}/{5}g{6}t{7}r{8}P{9}{10}h{11}b{12}{13}",
                observation.StationId,
                observation.ObservationTimeUtc.ToString("ddHHmm"),
                FormatLatitude(observation.LatitudeDegrees, observation.LatitudeMinutes, observation.LatitudeSeconds, observation.LatitudeHemisphere),
                FormatLongitude(observation.LongitudeDegrees, observation.LongitudeMinutes, observation.LongitudeSeconds, observation.LongitudeHemisphere),
                FormatWindDirection(observation.AvgWindDirectionDegrees),
                FormatWindSpeed(observation.AvgWindSpeedMph),
                FormatWindSpeed(observation.GustWindSpeedMph),
                FormatTemperature(observation.OutsideTemperatureF),
                FormatRainHundredths(observation.HourlyRainInches),
                FormatRainHundredths(observation.DailyRainInches),
                FormatSolarRadiation(observation.SolarRadiationWm2),
                FormatHumidity(observation.OutsideHumidityPercent),
                string.Format("{0:00000}", (int)Math.Truncate(barometricPressureTenthsMb)),
                observation.SoftwareType ?? DefaultSoftwareType);
        }

        /// <summary>
        /// Formats a login string for CWOP server authentication.
        /// </summary>
        /// <param name="stationId">The CWOP station identifier (e.g., "DW4515").</param>
        /// <param name="passcode">The APRS-IS passcode (use -1 for receive-only/unverified).</param>
        /// <param name="softwareVersion">The software version string.</param>
        /// <returns>The login string to send to the CWOP server.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="stationId"/> is <c>null</c> or empty.</exception>
        public static string FormatLogin(string stationId, int passcode, string softwareVersion)
        {
            if (string.IsNullOrEmpty(stationId)) throw new ArgumentNullException(nameof(stationId));
            return string.Format("user {0} {1} vers {2}", stationId, passcode, softwareVersion ?? "1.0.0.0");
        }

        /// <summary>
        /// Computes the MADIS altimeter setting in tenths of millibars from a station barometer reading.
        /// </summary>
        /// <param name="barometerInHg">Station barometer reading in inches of mercury.</param>
        /// <param name="elevationFeet">Station elevation in feet above sea level.</param>
        /// <param name="temperatureF">Outside temperature in degrees Fahrenheit.</param>
        /// <param name="humidityPercent">Outside humidity as a percentage (0–100). <c>null</c> defaults to 50.</param>
        /// <returns>The altimeter setting in tenths of millibars (e.g., 10132 for 1013.2 mb).</returns>
        public static double ComputeAltimeterTenthsMillibars(double barometerInHg, double elevationFeet, double? temperatureF, double? humidityPercent)
        {
            double elevationM = elevationFeet * 0.3048;
            double barometricPressureMb = BarometricPressure.FromInchesHg(barometerInHg).Millibars;
            Temperature temperature = Temperature.FromFahrenheit(temperatureF ?? 59.0);
            byte humidity = (byte)Math.Min(100, Math.Max(0, humidityPercent ?? 50));

            double reductionRatio = WxUtils.PressureReductionRatio(barometricPressureMb, elevationM, temperature, temperature, humidity, WxUtils.SLPAlgorithm.DavisVP);
            double madisBarometer = WxUtils.StationToAltimeter(barometricPressureMb / reductionRatio, elevationM);

            return madisBarometer * 10;
        }

        /// <summary>
        /// Formats latitude in APRS format: <c>DDMM.MMH</c> (e.g., <c>3533.60N</c>).
        /// </summary>
        /// <param name="degrees">Whole degrees (0–90).</param>
        /// <param name="minutes">Arc-minutes (0–59).</param>
        /// <param name="seconds">Arc-seconds (0–59.999).</param>
        /// <param name="hemisphere">Hemisphere character: <c>"N"</c> or <c>"S"</c>.</param>
        /// <returns>The APRS-formatted latitude string.</returns>
        public static string FormatLatitude(int degrees, int minutes, double seconds, string hemisphere)
        {
            double totalMinutes = minutes + seconds / 60.0;
            return string.Format("{0:00}{1:00.00}{2}", degrees, totalMinutes, hemisphere ?? "N");
        }

        /// <summary>
        /// Formats longitude in APRS format: <c>DDDMM.MMH</c> (e.g., <c>11354.57W</c>).
        /// </summary>
        /// <param name="degrees">Whole degrees (0–180).</param>
        /// <param name="minutes">Arc-minutes (0–59).</param>
        /// <param name="seconds">Arc-seconds (0–59.999).</param>
        /// <param name="hemisphere">Hemisphere character: <c>"E"</c> or <c>"W"</c>.</param>
        /// <returns>The APRS-formatted longitude string.</returns>
        public static string FormatLongitude(int degrees, int minutes, double seconds, string hemisphere)
        {
            double totalMinutes = minutes + seconds / 60.0;
            return string.Format("{0:000}{1:00.00}{2}", degrees, totalMinutes, hemisphere ?? "W");
        }

        /// <summary>
        /// Formats wind direction in APRS format (3 digits, 000–360).
        /// Returns <c>"..."</c> when not available.
        /// </summary>
        public static string FormatWindDirection(double? degrees)
        {
            return degrees.HasValue ? string.Format("{0:000}", (int)degrees.Value) : "...";
        }

        /// <summary>
        /// Formats wind speed in APRS format (3 digits, mph).
        /// Returns <c>"..."</c> when not available.
        /// </summary>
        public static string FormatWindSpeed(double? speedMph)
        {
            return speedMph.HasValue ? string.Format("{0:000}", (int)speedMph.Value) : "...";
        }

        /// <summary>
        /// Formats temperature in APRS format (3 digits for positive, 2 digits for negative, Fahrenheit).
        /// Returns <c>"..."</c> when not available.
        /// </summary>
        public static string FormatTemperature(double? temperatureF)
        {
            if (!temperatureF.HasValue) return "...";
            int temp = (int)temperatureF.Value;
            return temp < 0 ? string.Format("{0:00}", temp) : string.Format("{0:000}", temp);
        }

        /// <summary>
        /// Formats rain in APRS format (3 digits, in hundredths of an inch).
        /// Returns <c>"..."</c> when not available.
        /// </summary>
        public static string FormatRainHundredths(double? rainInches)
        {
            if (!rainInches.HasValue) return "...";
            return string.Format("{0:000}", (int)(rainInches.Value * 100));
        }

        /// <summary>
        /// Formats solar radiation in APRS format.
        /// Below 1000 W/m²: <c>LNNN</c>. At or above 1000 W/m²: <c>lNNN</c> (value − 1000).
        /// Returns empty string when not available.
        /// </summary>
        public static string FormatSolarRadiation(double? solarRadiationWm2)
        {
            if (!solarRadiationWm2.HasValue) return "";
            int value = (int)solarRadiationWm2.Value;
            return value < 1000
                ? string.Format("L{0:000}", value)
                : string.Format("l{0:000}", value - 1000);
        }

        /// <summary>
        /// Formats humidity in APRS format (2 digits).
        /// 100% is encoded as <c>"00"</c>. 0% is encoded as <c>"01"</c>.
        /// Returns <c>".."</c> when not available.
        /// </summary>
        public static string FormatHumidity(double? humidityPercent)
        {
            if (!humidityPercent.HasValue) return "..";
            int humidity = (int)humidityPercent.Value;
            if (humidity >= 100) return "00";
            if (humidity <= 0) return "01";
            return string.Format("{0:00}", humidity);
        }
    }

    /// <summary>
    /// Represents a complete weather observation for CWOP/APRS submission.
    /// </summary>
    public sealed class CwopObservation
    {
        /// <summary>
        /// Gets or sets the CWOP station identifier (e.g., "DW4515").
        /// </summary>
        public string StationId { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the observation time in UTC.
        /// </summary>
        public DateTimeOffset ObservationTimeUtc { get; set; }

        /// <summary>
        /// Gets or sets the station latitude degrees (0–90).
        /// </summary>
        public int LatitudeDegrees { get; set; }

        /// <summary>
        /// Gets or sets the station latitude arc-minutes (0–59).
        /// </summary>
        public int LatitudeMinutes { get; set; }

        /// <summary>
        /// Gets or sets the station latitude arc-seconds (0–59.999).
        /// </summary>
        public double LatitudeSeconds { get; set; }

        /// <summary>
        /// Gets or sets the station latitude hemisphere ("N" or "S").
        /// </summary>
        public string LatitudeHemisphere { get; set; } = "N";

        /// <summary>
        /// Gets or sets the station longitude degrees (0–180).
        /// </summary>
        public int LongitudeDegrees { get; set; }

        /// <summary>
        /// Gets or sets the station longitude arc-minutes (0–59).
        /// </summary>
        public int LongitudeMinutes { get; set; }

        /// <summary>
        /// Gets or sets the station longitude arc-seconds (0–59.999).
        /// </summary>
        public double LongitudeSeconds { get; set; }

        /// <summary>
        /// Gets or sets the station longitude hemisphere ("E" or "W").
        /// </summary>
        public string LongitudeHemisphere { get; set; } = "W";

        /// <summary>
        /// Gets or sets the station elevation in feet above sea level.
        /// </summary>
        public double StationElevationFeet { get; set; }

        /// <summary>
        /// Gets or sets the station barometer reading in inches of mercury.
        /// </summary>
        public double BarometerInHg { get; set; }

        /// <summary>
        /// Gets or sets the outside temperature in degrees Fahrenheit, or <c>null</c> if not available.
        /// </summary>
        public double? OutsideTemperatureF { get; set; }

        /// <summary>
        /// Gets or sets the outside humidity as a percentage (0–100), or <c>null</c> if not available.
        /// </summary>
        public double? OutsideHumidityPercent { get; set; }

        /// <summary>
        /// Gets or sets the average wind direction in degrees (0–360), or <c>null</c> if not available.
        /// </summary>
        public double? AvgWindDirectionDegrees { get; set; }

        /// <summary>
        /// Gets or sets the average wind speed in mph, or <c>null</c> if not available.
        /// </summary>
        public double? AvgWindSpeedMph { get; set; }

        /// <summary>
        /// Gets or sets the wind gust speed in mph, or <c>null</c> if not available.
        /// </summary>
        public double? GustWindSpeedMph { get; set; }

        /// <summary>
        /// Gets or sets the hourly rain accumulation in inches, or <c>null</c> if not available.
        /// </summary>
        public double? HourlyRainInches { get; set; }

        /// <summary>
        /// Gets or sets the daily rain accumulation in inches, or <c>null</c> if not available.
        /// </summary>
        public double? DailyRainInches { get; set; }

        /// <summary>
        /// Gets or sets the solar radiation in W/m², or <c>null</c> if not available.
        /// </summary>
        public double? SolarRadiationWm2 { get; set; }

        /// <summary>
        /// Gets or sets the software type identifier appended to the packet. Defaults to "DVs".
        /// </summary>
        public string SoftwareType { get; set; } = CwopAprsFormatter.DefaultSoftwareType;
    }
}
