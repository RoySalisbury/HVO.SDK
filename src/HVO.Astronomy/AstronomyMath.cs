using System;
using System.Runtime.CompilerServices;

namespace HVO.Astronomy
{
    /// <summary>
    /// Provides core astronomical utility calculations — Julian dates, sidereal time,
    /// coordinate transforms, angle conversions, and atmospheric refraction.
    /// </summary>
    /// <remarks>
    /// Consolidated from:
    /// <list type="bullet">
    ///   <item><description>HVO.Core.Astronomy.AstronomyMath (EquatorialToHorizontal, Bennett refraction, sidereal rotation)</description></item>
    ///   <item><description>HVOv6 HVO.Astronomy.DateCalculations (J2000 day number, GMST0)</description></item>
    ///   <item><description>HVOv9-SkyMonitorV6 Imaging AstronomyMath (Julian Date, LST)</description></item>
    /// </list>
    /// Algorithm sources: Paul Schlyter (stjarnhimlen.se), Jean Meeus "Astronomical Algorithms".
    /// </remarks>
    public static class AstronomyMath
    {
        /// <summary>
        /// Length of a sidereal day in seconds (23h 56m 4.0905s).
        /// </summary>
        public const double SiderealDaySeconds = 86_164.0905;

        private const double HoursPerDay = 24.0;
        private const double DegreesPerCircle = 360.0;
        private const double SecondsPerDay = 86400.0;
        private const double TwoPi = 2.0 * Math.PI;

        // ────────────────────────────────────────────────────────────────
        //  Angle conversions
        // ────────────────────────────────────────────────────────────────

        /// <summary>
        /// Converts degrees to radians.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static double DegreesToRadians(double degrees) => degrees * Math.PI / 180.0;

        /// <summary>
        /// Converts radians to degrees.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static double RadiansToDegrees(double radians) => radians * 180.0 / Math.PI;

        /// <summary>
        /// Normalizes an angle expressed in degrees to the [0, 360) range.
        /// </summary>
        public static double NormalizeDegrees(double degrees)
        {
            var value = degrees % DegreesPerCircle;
            return value < 0.0 ? value + DegreesPerCircle : value;
        }

        /// <summary>
        /// Normalizes an angle expressed in degrees to the (-180, +180] range.
        /// </summary>
        public static double NormalizeDegrees180(double degrees)
        {
            return degrees - DegreesPerCircle * Math.Floor(degrees * (1.0 / DegreesPerCircle) + 0.5);
        }

        /// <summary>
        /// Normalizes a time expressed in hours to the [0, 24) range.
        /// </summary>
        public static double NormalizeHours(double hours)
        {
            var value = hours % HoursPerDay;
            return value < 0.0 ? value + HoursPerDay : value;
        }

        // ────────────────────────────────────────────────────────────────
        //  Julian Date / J2000
        // ────────────────────────────────────────────────────────────────

        /// <summary>
        /// Calculates the Julian Date for the provided UTC timestamp.
        /// </summary>
        /// <param name="utc">A <see cref="DateTime"/> in UTC.</param>
        /// <returns>The Julian Date as a double.</returns>
        public static double JulianDate(DateTime utc)
        {
            var normalized = utc.Kind == DateTimeKind.Utc ? utc : utc.ToUniversalTime();
            return normalized.ToOADate() + 2_415_018.5;
        }

        /// <summary>
        /// Computes the J2000 day number — days since the J2000.0 epoch (2000-01-01 12:00 UTC).
        /// </summary>
        /// <param name="dateTime">A <see cref="DateTime"/> in UTC.</param>
        /// <returns>Fractional days since J2000.0.</returns>
        public static double J2000(DateTime dateTime)
        {
            return dateTime.Subtract(new DateTime(2000, 1, 1, 12, 0, 0, DateTimeKind.Utc)).TotalSeconds / SecondsPerDay;
        }

        /// <summary>
        /// Computes the J2000 day number offset by -1.5 days, used by the Schlyter sunrise/sunset
        /// and Sun/Moon position algorithms.
        /// </summary>
        /// <param name="dateTime">A <see cref="DateTime"/> in UTC.</param>
        /// <returns>Day number for use with <see cref="SunCalculations"/> and <see cref="MoonCalculations"/>.</returns>
        public static double J2000_UT(DateTime dateTime)
        {
            return dateTime.Subtract(new DateTime(2000, 1, 1, 12, 0, 0, DateTimeKind.Utc).AddDays(-1.5)).TotalSeconds / SecondsPerDay;
        }

        /// <summary>
        /// Computes Julian centuries since J2000.0 for the given Julian Date.
        /// </summary>
        /// <param name="julianDate">A Julian Date value.</param>
        /// <returns>Julian centuries (T) since J2000.0.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static double JulianCenturies(double julianDate) => (julianDate - 2_451_545.0) / 36_525.0;

        // ────────────────────────────────────────────────────────────────
        //  Sidereal time
        // ────────────────────────────────────────────────────────────────

        /// <summary>
        /// Computes GMST0, the Greenwich Mean Sidereal Time at 0h UT, expressed in degrees [0, 360).
        /// </summary>
        /// <param name="dayNumber">J2000_UT day number.</param>
        /// <returns>GMST0 in degrees.</returns>
        internal static double GMST0(double dayNumber)
        {
            return NormalizeDegrees((180.0 + 356.0470 + 282.9404) + (0.9856002585 + 4.70935E-5) * dayNumber);
        }

        /// <summary>
        /// Calculates the local sidereal time at the provided longitude.
        /// </summary>
        /// <param name="utc">A <see cref="DateTime"/> in UTC.</param>
        /// <param name="longitudeDegrees">Observer longitude in degrees (positive east).</param>
        /// <returns>Local sidereal time in hours [0, 24).</returns>
        public static double LocalSiderealTime(DateTime utc, double longitudeDegrees)
        {
            var jd = JulianDate(utc);
            var t = JulianCenturies(jd);
            var gmst = 6.697374558 + 2400.051336 * t + 0.000025862 * t * t;
            var fractionalDay = (jd + 0.5) % 1.0;
            gmst = (gmst + fractionalDay * HoursPerDay * 1.00273790935) % HoursPerDay;
            var lst = (gmst + longitudeDegrees / 15.0) % HoursPerDay;
            if (lst < 0) lst += HoursPerDay;
            return lst;
        }

        // ────────────────────────────────────────────────────────────────
        //  Coordinate transforms
        // ────────────────────────────────────────────────────────────────

        /// <summary>
        /// Converts equatorial coordinates (RA/Dec) to horizontal coordinates (Alt/Az).
        /// </summary>
        /// <param name="rightAscensionHours">Right ascension in hours.</param>
        /// <param name="declinationDegrees">Declination in degrees.</param>
        /// <param name="localSiderealTimeHours">Local sidereal time in hours.</param>
        /// <param name="latitudeDegrees">Observer latitude in degrees.</param>
        /// <returns>A tuple of (AltitudeDeg, AzimuthDeg).</returns>
        public static (double AltitudeDeg, double AzimuthDeg) EquatorialToHorizontal(
            double rightAscensionHours,
            double declinationDegrees,
            double localSiderealTimeHours,
            double latitudeDegrees)
        {
            var hourAngle = DegreesToRadians((localSiderealTimeHours - rightAscensionHours) * 15.0);
            var declinationRad = DegreesToRadians(declinationDegrees);
            var latitudeRad = DegreesToRadians(latitudeDegrees);

            var sinAlt = Math.Sin(declinationRad) * Math.Sin(latitudeRad) +
                         Math.Cos(declinationRad) * Math.Cos(latitudeRad) * Math.Cos(hourAngle);
            var altitude = Math.Asin(Clamp(sinAlt, -1.0, 1.0));

            var cosAz = (Math.Sin(declinationRad) - Math.Sin(altitude) * Math.Sin(latitudeRad)) /
                        (Math.Cos(altitude) * Math.Cos(latitudeRad));
            cosAz = Clamp(cosAz, -1.0, 1.0);

            var azimuth = Math.Acos(cosAz);
            if (Math.Sin(hourAngle) > 0) azimuth = TwoPi - azimuth;

            return (RadiansToDegrees(altitude), RadiansToDegrees(azimuth));
        }

        // ────────────────────────────────────────────────────────────────
        //  Atmospheric refraction
        // ────────────────────────────────────────────────────────────────

        /// <summary>
        /// Calculates the apparent atmospheric refraction in degrees using the Bennett 1982 model.
        /// </summary>
        /// <param name="altitudeDegrees">Observed altitude in degrees.</param>
        /// <returns>Refraction offset in degrees.</returns>
        public static double BennettRefractionDegrees(double altitudeDegrees)
        {
            var a = Math.Max(altitudeDegrees, -0.9);
            var refractionArcMinutes = 1.02 / Math.Tan((a + 10.3 / (a + 5.11)) * Math.PI / 180.0);
            return refractionArcMinutes / 60.0;
        }

        // ────────────────────────────────────────────────────────────────
        //  Sidereal rotation (sky dome overlay)
        // ────────────────────────────────────────────────────────────────

        /// <summary>
        /// Computes the fraction of a sidereal rotation that has elapsed at the provided timestamp and longitude.
        /// </summary>
        /// <param name="timestamp">The UTC timestamp as a <see cref="DateTimeOffset"/>.</param>
        /// <param name="longitudeDegrees">Observer longitude in degrees (positive east).</param>
        /// <returns>A value in [0, 1) representing the fraction of a sidereal day elapsed.</returns>
        public static double CalculateSiderealRotationFraction(DateTimeOffset timestamp, double longitudeDegrees)
        {
            var totalSeconds = timestamp.ToUnixTimeMilliseconds() / 1_000.0;
            var secondsIntoCycle = totalSeconds % SiderealDaySeconds;
            if (secondsIntoCycle < 0)
            {
                secondsIntoCycle += SiderealDaySeconds;
            }

            var fractionOfCycle = secondsIntoCycle / SiderealDaySeconds;
            var longitudeFraction = longitudeDegrees / DegreesPerCircle;
            var localCycle = (fractionOfCycle + longitudeFraction) % 1.0;
            return localCycle < 0 ? localCycle + 1.0 : localCycle;
        }

        /// <summary>
        /// Calculates the apparent rotation of the sky dome in degrees for the provided timestamp and longitude.
        /// </summary>
        /// <param name="timestamp">The UTC timestamp as a <see cref="DateTimeOffset"/>.</param>
        /// <param name="longitudeDegrees">Observer longitude in degrees (positive east).</param>
        /// <param name="rotationOffsetDegrees">Rotation offset in degrees (default -90).</param>
        /// <returns>Sky dome rotation in degrees as a float.</returns>
        public static float CalculateSkyRotationDegrees(DateTimeOffset timestamp, double longitudeDegrees, double rotationOffsetDegrees = -90.0)
        {
            var fraction = CalculateSiderealRotationFraction(timestamp, longitudeDegrees);
            var rotation = -fraction * DegreesPerCircle;
            return (float)(rotation + rotationOffsetDegrees);
        }

        // ────────────────────────────────────────────────────────────────
        //  Internal helpers
        // ────────────────────────────────────────────────────────────────

        /// <summary>
        /// Polyfill for <c>Math.Clamp</c> which is not available in .NET Standard 2.0.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static double Clamp(double value, double min, double max) =>
            value < min ? min : value > max ? max : value;
    }
}
