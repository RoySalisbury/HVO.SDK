using System;
using HVO.Core.Security.Cryptography;

namespace HVO.Weather.DavisVantagePro
{
    /// <summary>
    /// Parses a Davis Vantage Pro LOOP packet (99 bytes) into strongly-typed weather properties.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The LOOP packet format is documented in the Davis Vantage Pro serial communication reference.
    /// Each packet contains 99 bytes starting with the ASCII characters "LOO" (0x4C 0x4F 0x4F)
    /// and ending with a CRC-16 CCITT checksum.
    /// </para>
    /// <para>
    /// This class is immutable once created. Use <see cref="Create"/> to parse a raw byte array.
    /// </para>
    /// </remarks>
    public sealed class DavisVantageProConsoleRecord
    {
        private DavisVantageProConsoleRecord(byte[] rawDataRecord, DateTimeOffset recordDateTime)
        {
            RawDataRecord = (byte[])rawDataRecord.Clone();
            RecordDateTime = recordDateTime;
        }

        /// <summary>
        /// Creates a new <see cref="DavisVantageProConsoleRecord"/> by parsing a raw 99-byte LOOP packet.
        /// </summary>
        /// <param name="rawDataRecord">The raw 99-byte LOOP packet data.</param>
        /// <param name="recordDateTime">The date and time the record was received.</param>
        /// <param name="validateCrc">When <c>true</c>, validates the packet CRC before parsing.</param>
        /// <returns>A fully-parsed console record.</returns>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <paramref name="rawDataRecord"/> is <c>null</c> or has fewer than 99 bytes.
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// CRC validation failed when <paramref name="validateCrc"/> is <c>true</c>.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// The packet does not start with the expected "LOO" header bytes.
        /// </exception>
        public static DavisVantageProConsoleRecord Create(byte[] rawDataRecord, DateTimeOffset recordDateTime, bool validateCrc = true)
        {
            if (rawDataRecord == null || rawDataRecord.Length < 99)
            {
                throw new ArgumentOutOfRangeException(nameof(rawDataRecord));
            }

            if (validateCrc && !ValidatePacketCrc(rawDataRecord))
            {
                throw new InvalidOperationException("Packet CRC is invalid");
            }

            if (rawDataRecord[0] != 0x4C /* L */ || rawDataRecord[1] != 0x4F /* O */ || rawDataRecord[2] != 0x4F /* O */)
            {
                throw new ArgumentException("Packet does not start with expected LOO header.", nameof(rawDataRecord));
            }

            // Parse all raw data values from their byte offsets.
            byte barometerTrend = rawDataRecord[3];
            ushort barometer = BitConverter.ToUInt16(rawDataRecord, 7);
            short insideTemperature = BitConverter.ToInt16(rawDataRecord, 9);
            byte insideHumidity = rawDataRecord[11];
            short outsideTemperature = BitConverter.ToInt16(rawDataRecord, 12);
            byte windSpeed = rawDataRecord[14];
            byte tenMinuteWindSpeedAverage = rawDataRecord[15];
            ushort windDirection = BitConverter.ToUInt16(rawDataRecord, 16);
            byte outsideHumidity = rawDataRecord[33];
            ushort rainRate = BitConverter.ToUInt16(rawDataRecord, 41);
            byte uvIndex = rawDataRecord[43];
            ushort solarRadiation = BitConverter.ToUInt16(rawDataRecord, 44);
            ushort stormRain = BitConverter.ToUInt16(rawDataRecord, 46);
            ushort stormStartDate = BitConverter.ToUInt16(rawDataRecord, 48);
            ushort dailyRainAmount = BitConverter.ToUInt16(rawDataRecord, 50);
            ushort monthlyRainAmount = BitConverter.ToUInt16(rawDataRecord, 52);
            ushort yearlyRainAmount = BitConverter.ToUInt16(rawDataRecord, 54);
            ushort dailyETAmount = BitConverter.ToUInt16(rawDataRecord, 56);
            ushort monthlyETAmount = BitConverter.ToUInt16(rawDataRecord, 58);
            ushort yearlyETAmount = BitConverter.ToUInt16(rawDataRecord, 60);
            ushort consoleBatteryVoltage = BitConverter.ToUInt16(rawDataRecord, 87);
            byte forecastIcons = rawDataRecord[89];
            ushort sunriseTime = BitConverter.ToUInt16(rawDataRecord, 91);
            ushort sunsetTime = BitConverter.ToUInt16(rawDataRecord, 93);
            ushort nextArchiveRecord = BitConverter.ToUInt16(rawDataRecord, 5);

            // Date conversion: storm start date is encoded as a bit-packed ushort.
            DateTime? parsedStormStartDate = null;
            if (stormStartDate != ushort.MaxValue)
            {
                int year = (stormStartDate & 0x007F) + 2000;   // bits 0-6  = year offset from 2000
                int day = (stormStartDate & 0x0F80) >> 7;       // bits 7-11 = day
                int month = (stormStartDate & 0xF000) >> 12;    // bits 12-15 = month
                parsedStormStartDate = new DateTime(year, month, day);
            }

            // Time conversion: sunrise/sunset are encoded as (hour * 100) + minute.
            TimeSpan ParseTime(ushort value)
            {
                int hour = value / 100;
                int minute = value % 100;
                return new TimeSpan(hour, minute, 0);
            }

            return new DavisVantageProConsoleRecord(rawDataRecord, recordDateTime)
            {
                Barometer = barometer / 1000.0,
                BarometerTrend = (BarometerTrend)barometerTrend,
                ConsoleBatteryVoltage = (consoleBatteryVoltage * 300.0 / 512.0) / 100.0,
                DailyETAmount = dailyETAmount / 1000.0,
                DailyRainAmount = dailyRainAmount / 100.0,
                ForecastIcons = (ForecastIcon)forecastIcons,
                InsideHumidity = insideHumidity,
                InsideTemperature = Temperature.FromFahrenheit(insideTemperature / 10.0),
                MonthlyETAmount = monthlyETAmount / 100.0,
                MonthlyRainAmount = monthlyRainAmount / 100.0,
                NextArchiveRecord = nextArchiveRecord,
                OutsideHumidity = (outsideHumidity == byte.MaxValue) ? (byte?)null : outsideHumidity,
                OutsideTemperature = (outsideTemperature == short.MaxValue) ? null : Temperature.FromFahrenheit(outsideTemperature / 10.0),
                RainRate = (rainRate == ushort.MaxValue) ? (double?)null : rainRate / 100.0,
                SolarRadiation = (solarRadiation == short.MaxValue) ? (ushort?)null : solarRadiation,
                StormRain = (stormRain == short.MaxValue) ? (double?)null : stormRain / 100.0,
                StormStartDate = parsedStormStartDate,
                SunriseTime = ParseTime(sunriseTime),
                SunsetTime = ParseTime(sunsetTime),
                TenMinuteWindSpeedAverage = (tenMinuteWindSpeedAverage == byte.MaxValue) ? (byte?)null : tenMinuteWindSpeedAverage,
                UVIndex = (uvIndex == byte.MaxValue) ? (byte?)null : uvIndex,
                WindDirection = (windDirection == short.MaxValue) ? (ushort?)null : windDirection,
                WindSpeed = (windSpeed == byte.MaxValue) ? (byte?)null : windSpeed,
                YearlyETAmount = yearlyETAmount / 100.0,
                YearlyRainAmount = yearlyRainAmount / 100.0
            };
        }

        /// <summary>
        /// Validates the CRC-16 checksum of a raw LOOP packet.
        /// </summary>
        /// <param name="rawDataRecord">The raw 99-byte LOOP packet data.</param>
        /// <returns><c>true</c> if the CRC is valid; otherwise <c>false</c>.</returns>
        /// <remarks>
        /// The CRC is computed over the first 97 bytes and compared against bytes 97-98.
        /// The original method name <c>ValidatePacktCrc</c> contained a spelling error
        /// that has been corrected in this migration.
        /// </remarks>
        public static bool ValidatePacketCrc(byte[] rawDataRecord)
        {
            using (var crc16 = new Crc16())
            {
                byte[] calculatedCrc = crc16.ComputeHash(rawDataRecord, 0, 97);
                return calculatedCrc[0] == rawDataRecord[97] && calculatedCrc[1] == rawDataRecord[98];
            }
        }

        /// <summary>Gets the raw 99-byte LOOP packet data.</summary>
        public byte[] RawDataRecord { get; private set; }

        /// <summary>Gets the date and time the record was received.</summary>
        public DateTimeOffset RecordDateTime { get; private set; }

        /// <summary>Gets the barometric pressure trend.</summary>
        public BarometerTrend BarometerTrend { get; private set; }

        /// <summary>Gets the pointer to the next archive record.</summary>
        public ushort NextArchiveRecord { get; private set; }

        /// <summary>Gets the barometric pressure in inches of mercury.</summary>
        public double Barometer { get; private set; }

        /// <summary>Gets the inside temperature.</summary>
        public Temperature InsideTemperature { get; private set; } = null!;

        /// <summary>Gets the inside relative humidity (0–100).</summary>
        public byte InsideHumidity { get; private set; }

        /// <summary>Gets the outside temperature, or <c>null</c> if the sensor is unavailable.</summary>
        public Temperature? OutsideTemperature { get; private set; }

        /// <summary>Gets the instantaneous wind speed in mph, or <c>null</c> if unavailable.</summary>
        public byte? WindSpeed { get; private set; }

        /// <summary>Gets the 10-minute average wind speed in mph, or <c>null</c> if unavailable.</summary>
        public byte? TenMinuteWindSpeedAverage { get; private set; }

        /// <summary>Gets the wind direction in degrees (0–360), or <c>null</c> if unavailable.</summary>
        public ushort? WindDirection { get; private set; }

        /// <summary>Gets the outside relative humidity (0–100), or <c>null</c> if unavailable.</summary>
        public byte? OutsideHumidity { get; private set; }

        /// <summary>Gets the rain rate in inches/hour, or <c>null</c> if unavailable.</summary>
        public double? RainRate { get; private set; }

        /// <summary>Gets the UV index, or <c>null</c> if unavailable.</summary>
        public byte? UVIndex { get; private set; }

        /// <summary>Gets the solar radiation in W/m², or <c>null</c> if unavailable.</summary>
        public ushort? SolarRadiation { get; private set; }

        /// <summary>Gets the current storm rain total in inches, or <c>null</c> if unavailable.</summary>
        public double? StormRain { get; private set; }

        /// <summary>Gets the date the current storm started, or <c>null</c> if no active storm.</summary>
        public DateTime? StormStartDate { get; private set; }

        /// <summary>Gets the daily rain accumulation in inches.</summary>
        public double DailyRainAmount { get; private set; }

        /// <summary>Gets the monthly rain accumulation in inches.</summary>
        public double MonthlyRainAmount { get; private set; }

        /// <summary>Gets the yearly rain accumulation in inches.</summary>
        public double YearlyRainAmount { get; private set; }

        /// <summary>Gets the console battery voltage.</summary>
        public double ConsoleBatteryVoltage { get; private set; }

        /// <summary>Gets the forecast icon flags displayed on the console.</summary>
        public ForecastIcon ForecastIcons { get; private set; }

        /// <summary>Gets the time of sunrise as reported by the console.</summary>
        public TimeSpan SunriseTime { get; private set; }

        /// <summary>Gets the time of sunset as reported by the console.</summary>
        public TimeSpan SunsetTime { get; private set; }

        /// <summary>Gets the daily evapotranspiration amount in inches.</summary>
        public double DailyETAmount { get; private set; }

        /// <summary>Gets the monthly evapotranspiration amount in inches.</summary>
        public double MonthlyETAmount { get; private set; }

        /// <summary>Gets the yearly evapotranspiration amount in inches.</summary>
        public double YearlyETAmount { get; private set; }

        /// <summary>
        /// Gets the computed outside heat index, or the outside temperature if conditions
        /// are outside the valid heat index range.
        /// </summary>
        /// <remarks>
        /// Uses the Schoen (2005) formula when outside temperature exceeds 80°F and humidity exceeds 40%.
        /// </remarks>
        public Temperature? OutsideHeatIndex
        {
            get
            {
                if (OutsideTemperature != null && OutsideHumidity != null && OutsideTemperature.Fahrenheit > 80 && OutsideHumidity > 40)
                {
                    var dewPoint = OutsideDewpoint;
                    if (dewPoint != null)
                    {
                        double heatIndex = OutsideTemperature.Fahrenheit -
                            (0.9971 * Math.Exp(0.020867 * (OutsideTemperature.Fahrenheit * (1 - Math.Exp(0.0445 * (dewPoint.Fahrenheit - 57.2))))));
                        return Temperature.FromFahrenheit(heatIndex);
                    }
                }
                return OutsideTemperature;
            }
        }

        /// <summary>
        /// Gets the computed outside wind chill, or the outside temperature if conditions
        /// are outside the valid wind chill range.
        /// </summary>
        /// <remarks>
        /// Uses the Steadman revised (1998) formula when wind speed is greater than zero.
        /// </remarks>
        public Temperature? OutsideWindChill
        {
            get
            {
                if (OutsideTemperature != null && TenMinuteWindSpeedAverage != null && TenMinuteWindSpeedAverage > 0)
                {
                    double windChill = 3.16 - (1.20 * TenMinuteWindSpeedAverage.Value) + (0.980 * OutsideTemperature.Fahrenheit)
                        + (0.0044 * Math.Pow(TenMinuteWindSpeedAverage.Value, 2))
                        + (0.0083 * TenMinuteWindSpeedAverage.Value * OutsideTemperature.Fahrenheit);
                    return Temperature.FromFahrenheit(windChill < OutsideTemperature.Fahrenheit ? windChill : OutsideTemperature.Fahrenheit);
                }
                return OutsideTemperature;
            }
        }

        /// <summary>
        /// Gets the computed outside dew point, or <c>null</c> if outside temperature or
        /// humidity data is unavailable.
        /// </summary>
        /// <remarks>
        /// Uses the Magnus-Tetens formula (Barenbrug 1974).
        /// </remarks>
        public Temperature? OutsideDewpoint
        {
            get
            {
                // BUG FIX: Legacy code used (OutsideHumidity != null) || (OutsideHumidity > 0)
                // which is always true when OutsideHumidity is non-null (short-circuit).
                // Corrected to && so both conditions must be satisfied.
                if (OutsideTemperature != null && OutsideHumidity != null && OutsideHumidity > 0)
                {
                    double outsideHumidity = (double)OutsideHumidity.Value;
                    double z1 = ((17.27 * OutsideTemperature.Celsius) / (237.7 + OutsideTemperature.Celsius)) + Math.Log(outsideHumidity / 100);
                    return Temperature.FromCelsius((237.7 * z1) / (17.27 - z1));
                }
                return null;
            }
        }
    }
}
