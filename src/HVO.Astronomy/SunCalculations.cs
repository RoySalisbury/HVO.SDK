using System;

namespace HVO.Astronomy
{
    /// <summary>
    /// Provides solar position, sunrise/sunset, and twilight calculations.
    /// </summary>
    /// <remarks>
    /// Algorithm: Paul Schlyter — <see href="http://www.stjarnhimlen.se/comp/tutorial.htm"/>
    /// and <see href="http://www.stjarnhimlen.se/comp/riset.html"/>.
    /// <para>
    /// Horizon altitude angles used:
    /// <list type="bullet">
    ///   <item><description><c> 0°</c> — Center of disk touches a mathematical horizon</description></item>
    ///   <item><description><c>-0.25°</c> — Upper limb touches a mathematical horizon</description></item>
    ///   <item><description><c>-0.583°</c> — Center of disk, refraction-corrected</description></item>
    ///   <item><description><c>-0.833°</c> — Upper limb, refraction-corrected</description></item>
    ///   <item><description><c>-6°</c> — Civil twilight</description></item>
    ///   <item><description><c>-12°</c> — Nautical twilight</description></item>
    ///   <item><description><c>-15°</c> — Amateur astronomical twilight</description></item>
    ///   <item><description><c>-18°</c> — Astronomical twilight (sky completely dark)</description></item>
    /// </list>
    /// </para>
    /// </remarks>
    public static class SunCalculations
    {
        // ────────────────────────────────────────────────────────────────
        //  Right Ascension / Declination
        // ────────────────────────────────────────────────────────────────

        /// <summary>
        /// Computes the Sun's equatorial coordinates (RA / Dec) for the given J2000_UT day number.
        /// </summary>
        /// <param name="dayNumber">J2000_UT day number (see <see cref="AstronomyMath.J2000_UT"/>).</param>
        /// <param name="rightAscension">Right ascension in degrees.</param>
        /// <param name="declination">Declination in degrees.</param>
        /// <returns>Sun–Earth distance in AU.</returns>
        public static double SunRightAscensionDeclination(double dayNumber, out double rightAscension, out double declination)
        {
            double longitude;
            double distance = SunPosition(dayNumber, out longitude);

            // Ecliptic rectangular coordinates (z = 0)
            var x = distance * Math.Cos(AstronomyMath.DegreesToRadians(longitude));
            var y = distance * Math.Sin(AstronomyMath.DegreesToRadians(longitude));

            // Obliquity of ecliptic
            var eclipticObliquity = 23.4393 - 3.563E-7 * dayNumber;

            // Convert to equatorial rectangular coordinates (x is unchanged)
            var z = y * Math.Sin(AstronomyMath.DegreesToRadians(eclipticObliquity));
            y = y * Math.Cos(AstronomyMath.DegreesToRadians(eclipticObliquity));

            // Convert to spherical coordinates
            rightAscension = AstronomyMath.RadiansToDegrees(Math.Atan2(y, x));
            declination = AstronomyMath.RadiansToDegrees(Math.Atan2(z, Math.Sqrt(x * x + y * y)));

            return distance;
        }

        // ────────────────────────────────────────────────────────────────
        //  Altitude / Azimuth
        // ────────────────────────────────────────────────────────────────

        /// <summary>
        /// Computes the Sun's horizontal coordinates (altitude / azimuth) for the given observer.
        /// </summary>
        /// <param name="dayNumber">J2000_UT day number.</param>
        /// <param name="siteLatitude">Observer latitude in degrees (positive north).</param>
        /// <param name="siteLongitude">Observer longitude in degrees (positive east).</param>
        /// <param name="altitude">Altitude above the horizon in degrees.</param>
        /// <param name="azimuth">Azimuth in degrees measured from north (0°) through east (90°).</param>
        /// <returns>Sun–Earth distance in AU.</returns>
        public static double SunAltitudeAzimuth(double dayNumber, double siteLatitude, double siteLongitude, out double altitude, out double azimuth)
        {
            double rightAscension, declination;
            double distance = SunRightAscensionDeclination(dayNumber, out rightAscension, out declination);

            var hourAngle = (AstronomyMath.GMST0(dayNumber)
                + (dayNumber - Math.Floor(dayNumber)) * 360
                + siteLongitude) - rightAscension;

            var x = Math.Cos(AstronomyMath.DegreesToRadians(hourAngle)) * Math.Cos(AstronomyMath.DegreesToRadians(declination));
            var y = Math.Sin(AstronomyMath.DegreesToRadians(hourAngle)) * Math.Cos(AstronomyMath.DegreesToRadians(declination));
            var z = Math.Sin(AstronomyMath.DegreesToRadians(declination));

            var xhor = x * Math.Sin(AstronomyMath.DegreesToRadians(siteLatitude)) - z * Math.Cos(AstronomyMath.DegreesToRadians(siteLatitude));
            var yhor = y;
            var zhor = x * Math.Cos(AstronomyMath.DegreesToRadians(siteLatitude)) + z * Math.Sin(AstronomyMath.DegreesToRadians(siteLatitude));

            altitude = AstronomyMath.RadiansToDegrees(Math.Asin(zhor));
            azimuth = AstronomyMath.RadiansToDegrees(Math.Atan2(yhor, xhor)) + 180;

            return distance;
        }

        // ────────────────────────────────────────────────────────────────
        //  Sunrise / Sunset
        // ────────────────────────────────────────────────────────────────

        /// <summary>
        /// Computes the UTC sunrise and sunset for the given date and observer location.
        /// </summary>
        /// <param name="date">The calendar date (time of day is ignored).</param>
        /// <param name="siteLatitude">Observer latitude in degrees.</param>
        /// <param name="siteLongitude">Observer longitude in degrees (positive east).</param>
        /// <param name="sunrise">UTC time of sunrise.</param>
        /// <param name="sunset">UTC time of sunset.</param>
        /// <returns>
        /// <c>0</c> — normal day and night; <c>-1</c> — Sun always below horizon; <c>+1</c> — Sun always above horizon.
        /// </returns>
        public static int SunriseSunset(DateTime date, double siteLatitude, double siteLongitude, out DateTimeOffset sunrise, out DateTimeOffset sunset)
        {
            TimeSpan sunriseTimeSpan;
            TimeSpan sunsetTimeSpan;

            int result = SunriseSunsetCore(date, siteLatitude, siteLongitude, -35.0 / 60.0, true, out sunriseTimeSpan, out sunsetTimeSpan);

            sunrise = new DateTimeOffset(date.Year, date.Month, date.Day, 0, 0, 0, TimeSpan.Zero).Add(sunriseTimeSpan);
            sunset = new DateTimeOffset(date.Year, date.Month, date.Day, 0, 0, 0, TimeSpan.Zero).Add(sunsetTimeSpan);

            return result;
        }

        // ────────────────────────────────────────────────────────────────
        //  Twilight
        // ────────────────────────────────────────────────────────────────

        /// <summary>
        /// Computes the UTC start and end of civil twilight (-6°).
        /// </summary>
        public static int CivilTwilight(DateTime date, double siteLatitude, double siteLongitude, out DateTimeOffset twilightStart, out DateTimeOffset twilightEnd)
        {
            return TwilightCore(date, siteLatitude, siteLongitude, -6.0, out twilightStart, out twilightEnd);
        }

        /// <summary>
        /// Computes the UTC start and end of nautical twilight (-12°).
        /// </summary>
        public static int NauticalTwilight(DateTime date, double siteLatitude, double siteLongitude, out DateTimeOffset twilightStart, out DateTimeOffset twilightEnd)
        {
            return TwilightCore(date, siteLatitude, siteLongitude, -12.0, out twilightStart, out twilightEnd);
        }

        /// <summary>
        /// Computes the UTC start and end of astronomical twilight (-18°).
        /// </summary>
        public static int AstronomicalTwilight(DateTime date, double siteLatitude, double siteLongitude, out DateTimeOffset twilightStart, out DateTimeOffset twilightEnd)
        {
            return TwilightCore(date, siteLatitude, siteLongitude, -18.0, out twilightStart, out twilightEnd);
        }

        // ────────────────────────────────────────────────────────────────
        //  Day length
        // ────────────────────────────────────────────────────────────────

        /// <summary>
        /// Returns the number of hours of daylight for the given date and observer location.
        /// </summary>
        public static double DayLength(DateTime date, double siteLatitude, double siteLongitude)
        {
            return DayLengthCore(date, siteLatitude, siteLongitude, -35.0 / 60.0, true);
        }

        /// <summary>
        /// Returns the number of hours between the start and end of civil twilight.
        /// </summary>
        public static double CivilTwilightDayLength(DateTime date, double siteLatitude, double siteLongitude)
        {
            return DayLengthCore(date, siteLatitude, siteLongitude, -6.0, false);
        }

        /// <summary>
        /// Returns the number of hours between the start and end of nautical twilight.
        /// </summary>
        public static double NauticalTwilightDayLength(DateTime date, double siteLatitude, double siteLongitude)
        {
            return DayLengthCore(date, siteLatitude, siteLongitude, -12.0, false);
        }

        /// <summary>
        /// Returns the number of hours between the start and end of astronomical twilight.
        /// </summary>
        public static double AstronomicalTwilightDayLength(DateTime date, double siteLatitude, double siteLongitude)
        {
            return DayLengthCore(date, siteLatitude, siteLongitude, -18.0, false);
        }

        // ────────────────────────────────────────────────────────────────
        //  Internal: ecliptic position
        // ────────────────────────────────────────────────────────────────

        /// <summary>
        /// Computes the Sun's ecliptic longitude and distance at an instant given in J2000_UT day number.
        /// The Sun's ecliptic latitude is not computed since it is always very near 0.
        /// </summary>
        internal static double SunPosition(double dayNumber, out double longitude)
        {
            double meanAnomaly = AstronomyMath.NormalizeDegrees(356.0470 + 0.9856002585 * dayNumber);
            double perihelionLongitude = 282.9404 + 4.70935E-5 * dayNumber;
            double eccentricity = 0.016709 - 1.151E-9 * dayNumber;

            // Eccentric anomaly
            double E = meanAnomaly + eccentricity * AstronomyMath.RadiansToDegrees(1.0)
                * Math.Sin(AstronomyMath.DegreesToRadians(meanAnomaly))
                * (1.0 + eccentricity * Math.Cos(AstronomyMath.DegreesToRadians(meanAnomaly)));

            double x = Math.Cos(AstronomyMath.DegreesToRadians(E)) - eccentricity;
            double y = Math.Sqrt(1.0 - eccentricity * eccentricity) * Math.Sin(AstronomyMath.DegreesToRadians(E));

            // True anomaly
            double v = AstronomyMath.RadiansToDegrees(Math.Atan2(y, x));

            // True solar longitude
            longitude = AstronomyMath.NormalizeDegrees(v + perihelionLongitude);

            // Solar distance
            return Math.Sqrt(x * x + y * y);
        }

        // ────────────────────────────────────────────────────────────────
        //  Private helpers
        // ────────────────────────────────────────────────────────────────

        private static int TwilightCore(DateTime date, double siteLatitude, double siteLongitude, double horizonAltitude, out DateTimeOffset twilightStart, out DateTimeOffset twilightEnd)
        {
            TimeSpan startSpan;
            TimeSpan endSpan;

            int result = SunriseSunsetCore(date, siteLatitude, siteLongitude, horizonAltitude, false, out startSpan, out endSpan);

            twilightStart = new DateTimeOffset(date.Year, date.Month, date.Day, 0, 0, 0, TimeSpan.Zero).Add(startSpan);
            twilightEnd = new DateTimeOffset(date.Year, date.Month, date.Day, 0, 0, 0, TimeSpan.Zero).Add(endSpan);

            return result;
        }

        private static double DayLengthCore(DateTime date, double siteLatitude, double siteLongitude, double horizonAltitude, bool upperLimb)
        {
            double dayNumber = AstronomyMath.J2000_UT(new DateTime(date.Year, date.Month, date.Day, 12, 0, 0)) - (siteLongitude / 360.0);

            var eclipticObliquity = 23.4393 - 3.563E-7 * dayNumber;

            double sunLongitude;
            var distance = SunPosition(dayNumber, out sunLongitude);

            var sinSunDeclination = Math.Sin(AstronomyMath.DegreesToRadians(eclipticObliquity)) * Math.Sin(AstronomyMath.DegreesToRadians(sunLongitude));
            var cosSunDeclination = Math.Sqrt(1.0 - sinSunDeclination * sinSunDeclination);

            if (upperLimb)
            {
                double apparentRadius = 0.2666 / distance;
                horizonAltitude -= apparentRadius;
            }

            double diurnalArc = (Math.Sin(AstronomyMath.DegreesToRadians(horizonAltitude))
                - Math.Sin(AstronomyMath.DegreesToRadians(siteLatitude)) * Math.Sin(AstronomyMath.DegreesToRadians(sinSunDeclination)))
                / (Math.Cos(AstronomyMath.DegreesToRadians(siteLatitude)) * Math.Cos(AstronomyMath.DegreesToRadians(cosSunDeclination)));

            if (diurnalArc >= 1.0)
            {
                return 0.0; // Sun always below horizon
            }
            else if (diurnalArc <= -1.0)
            {
                return 24.0; // Sun always above horizon
            }
            else
            {
                return (2.0 / 15.0) * AstronomyMath.RadiansToDegrees(Math.Acos(diurnalArc));
            }
        }

        private static int SunriseSunsetCore(DateTime date, double siteLatitude, double siteLongitude, double horizonAltitude, bool upperLimb, out TimeSpan sunrise, out TimeSpan sunset)
        {
            double dayNumber = AstronomyMath.J2000_UT(new DateTime(date.Year, date.Month, date.Day, 12, 0, 0)) - (siteLongitude / 360.0);

            double localSiderealTime = AstronomyMath.NormalizeDegrees(AstronomyMath.GMST0(dayNumber) + 180.0 + siteLongitude);

            double rightAscension, declination;
            double distance = SunRightAscensionDeclination(dayNumber, out rightAscension, out declination);

            double sunInSouthUT = 12.0 - AstronomyMath.NormalizeDegrees180(localSiderealTime - rightAscension) / 15.0;

            if (upperLimb)
            {
                double apparentRadius = 0.2666 / distance;
                horizonAltitude -= apparentRadius;
            }

            double diurnalArc = (Math.Sin(AstronomyMath.DegreesToRadians(horizonAltitude))
                - Math.Sin(AstronomyMath.DegreesToRadians(siteLatitude)) * Math.Sin(AstronomyMath.DegreesToRadians(declination)))
                / (Math.Cos(AstronomyMath.DegreesToRadians(siteLatitude)) * Math.Cos(AstronomyMath.DegreesToRadians(declination)));

            if (diurnalArc >= 1.0)
            {
                // Sun always below horizon
                sunrise = TimeSpan.FromHours(sunInSouthUT);
                sunset = TimeSpan.FromHours(sunInSouthUT);
                return -1;
            }
            else if (diurnalArc <= -1.0)
            {
                // Sun always above horizon
                sunrise = TimeSpan.FromHours(sunInSouthUT - 12);
                sunset = TimeSpan.FromHours(sunInSouthUT + 12);
                return +1;
            }
            else
            {
                double offset = AstronomyMath.RadiansToDegrees(Math.Acos(diurnalArc)) / 15.0;
                sunrise = TimeSpan.FromHours(sunInSouthUT - offset);
                sunset = TimeSpan.FromHours(sunInSouthUT + offset);
                return 0;
            }
        }
    }
}
