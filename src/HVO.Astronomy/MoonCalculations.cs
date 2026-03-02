using System;

namespace HVO.Astronomy
{
    /// <summary>
    /// Provides lunar position, age, and phase calculations.
    /// </summary>
    /// <remarks>
    /// Algorithm: Paul Schlyter — <see href="http://www.stjarnhimlen.se/comp/tutorial.htm"/>
    /// </remarks>
    public static class MoonCalculations
    {
        // ────────────────────────────────────────────────────────────────
        //  Altitude / Azimuth
        // ────────────────────────────────────────────────────────────────

        /// <summary>
        /// Computes the Moon's horizontal coordinates (altitude / azimuth) for the given observer.
        /// </summary>
        /// <param name="dayNumber">J2000_UT day number.</param>
        /// <param name="siteLatitude">Observer latitude in degrees (positive north).</param>
        /// <param name="siteLongitude">Observer longitude in degrees (positive east).</param>
        /// <param name="altitude">Altitude above the horizon in degrees (parallax-corrected).</param>
        /// <param name="azimuth">Azimuth in degrees.</param>
        /// <returns>Moon distance in Earth radii.</returns>
        public static double CalculateMoonAltitudeAzimuth(double dayNumber, double siteLatitude, double siteLongitude, out double altitude, out double azimuth)
        {
            double sunLongitude;
            SunCalculations.SunPosition(dayNumber, out sunLongitude);

            double moonRightAscension, moonDeclination;
            double moonDistance = CalculateMoonRightAscensionDeclination(dayNumber, out moonRightAscension, out moonDeclination);

            var hourAngle = (AstronomyMath.GMST0(dayNumber)
                + (dayNumber - Math.Floor(dayNumber)) * 360
                + siteLongitude) - moonRightAscension;

            var x = Math.Cos(AstronomyMath.DegreesToRadians(hourAngle)) * Math.Cos(AstronomyMath.DegreesToRadians(moonDeclination));
            var y = Math.Sin(AstronomyMath.DegreesToRadians(hourAngle)) * Math.Cos(AstronomyMath.DegreesToRadians(moonDeclination));
            var z = Math.Sin(AstronomyMath.DegreesToRadians(moonDeclination));

            var xhor = x * Math.Sin(AstronomyMath.DegreesToRadians(siteLatitude)) - z * Math.Cos(AstronomyMath.DegreesToRadians(siteLatitude));
            var yhor = y;
            var zhor = x * Math.Cos(AstronomyMath.DegreesToRadians(siteLatitude)) + z * Math.Sin(AstronomyMath.DegreesToRadians(siteLatitude));

            altitude = AstronomyMath.RadiansToDegrees(Math.Asin(zhor));

            // Topocentric parallax correction
            altitude -= AstronomyMath.RadiansToDegrees(Math.Asin(1.0 / moonDistance * Math.Cos(AstronomyMath.DegreesToRadians(altitude))));

            azimuth = AstronomyMath.RadiansToDegrees(Math.Atan2(yhor, xhor));

            if (siteLatitude < 0)
            {
                azimuth += 180;
            }
            else
            {
                azimuth -= 180;
            }

            return moonDistance;
        }

        // ────────────────────────────────────────────────────────────────
        //  Right Ascension / Declination
        // ────────────────────────────────────────────────────────────────

        /// <summary>
        /// Computes the Moon's equatorial coordinates (RA / Dec), including perturbation corrections.
        /// </summary>
        /// <param name="dayNumber">J2000_UT day number.</param>
        /// <param name="rightAscension">Right ascension in degrees.</param>
        /// <param name="declination">Declination in degrees.</param>
        /// <returns>Moon distance in Earth radii.</returns>
        public static double CalculateMoonRightAscensionDeclination(double dayNumber, out double rightAscension, out double declination)
        {
            var sunMeanAnomaly = AstronomyMath.NormalizeDegrees(356.0470 + 0.9856002585 * dayNumber);
            var sunEclipticObliquity = AstronomyMath.NormalizeDegrees(23.4393 - 3.563E-7 * dayNumber);

            double sunLongitude;
            SunCalculations.SunPosition(dayNumber, out sunLongitude);

            // Moon orbital elements
            var ascendingNode = AstronomyMath.NormalizeDegrees(125.1228 - 0.0529538083 * dayNumber);
            var inclination = 5.1454;
            var argPerigee = AstronomyMath.NormalizeDegrees(318.0634 + 0.1643573223 * dayNumber);
            var meanDistance = 60.2666;   // Earth radii
            var eccentricity = 0.054900;
            var meanAnomaly = AstronomyMath.NormalizeDegrees(115.3654 + 13.0649929509 * dayNumber);

            // Solve Kepler's equation iteratively for eccentric anomaly
            var eccentricAnomaly = meanAnomaly
                + AstronomyMath.RadiansToDegrees(1.0) * eccentricity
                * Math.Sin(AstronomyMath.DegreesToRadians(meanAnomaly))
                * (1.0 + eccentricity * Math.Cos(AstronomyMath.DegreesToRadians(meanAnomaly)));
            eccentricAnomaly = AstronomyMath.NormalizeDegrees(eccentricAnomaly);

            double eError = 9.0;
            int maxIterations = 10;
            var E0 = eccentricAnomaly;

            while (eError > 0.005 && maxIterations-- > 0)
            {
                E0 = eccentricAnomaly;
                eccentricAnomaly = E0
                    - (E0 - AstronomyMath.RadiansToDegrees(1.0) * eccentricity * Math.Sin(AstronomyMath.DegreesToRadians(E0)) - meanAnomaly)
                    / (1.0 - eccentricity * Math.Cos(AstronomyMath.DegreesToRadians(E0)));
                eccentricAnomaly = AstronomyMath.NormalizeDegrees(eccentricAnomaly);
                eError = Math.Abs(eccentricAnomaly - E0);
            }

            // Rectangular coordinates in orbital plane
            var x = meanDistance * (Math.Cos(AstronomyMath.DegreesToRadians(eccentricAnomaly)) - eccentricity);
            var y = meanDistance * Math.Sin(AstronomyMath.DegreesToRadians(AstronomyMath.NormalizeDegrees(eccentricAnomaly)))
                * Math.Sqrt(1.0 - eccentricity * eccentricity);

            // Distance and true anomaly
            var moonDist = Math.Sqrt(x * x + y * y);
            var v = AstronomyMath.NormalizeDegrees(AstronomyMath.RadiansToDegrees(Math.Atan2(y, x)));

            // Ecliptic coordinates
            var N = ascendingNode;
            var vw = v + argPerigee;
            var xeclip = moonDist * (Math.Cos(AstronomyMath.DegreesToRadians(N)) * Math.Cos(AstronomyMath.DegreesToRadians(vw))
                - Math.Sin(AstronomyMath.DegreesToRadians(N)) * Math.Sin(AstronomyMath.DegreesToRadians(vw)) * Math.Cos(AstronomyMath.DegreesToRadians(inclination)));
            var yeclip = moonDist * (Math.Sin(AstronomyMath.DegreesToRadians(N)) * Math.Cos(AstronomyMath.DegreesToRadians(vw))
                + Math.Cos(AstronomyMath.DegreesToRadians(N)) * Math.Sin(AstronomyMath.DegreesToRadians(vw)) * Math.Cos(AstronomyMath.DegreesToRadians(inclination)));
            var zeclip = moonDist * Math.Sin(AstronomyMath.DegreesToRadians(vw)) * Math.Sin(AstronomyMath.DegreesToRadians(inclination));

            var moonLongitude = AstronomyMath.NormalizeDegrees(AstronomyMath.RadiansToDegrees(Math.Atan2(yeclip, xeclip)));
            var moonLatitude = AstronomyMath.RadiansToDegrees(Math.Atan2(zeclip, Math.Sqrt(xeclip * xeclip + yeclip * yeclip)));

            // Perturbation fundamental arguments
            var Lm = AstronomyMath.NormalizeDegrees(N + argPerigee + meanAnomaly); // Moon mean longitude
            var D = AstronomyMath.NormalizeDegrees(Lm - sunLongitude);             // Moon mean elongation
            var F = AstronomyMath.NormalizeDegrees(Lm - N);                        // Moon argument of latitude

            // Longitude perturbations
            double longPert = 0;
            longPert += -1.274 * Math.Sin(AstronomyMath.DegreesToRadians(meanAnomaly - 2 * D));       // Evection
            longPert += +0.658 * Math.Sin(AstronomyMath.DegreesToRadians(2 * D));                     // Variation
            longPert += -0.186 * Math.Sin(AstronomyMath.DegreesToRadians(sunMeanAnomaly));             // Yearly equation
            longPert += -0.059 * Math.Sin(AstronomyMath.DegreesToRadians(2 * meanAnomaly - 2 * D));
            longPert += -0.057 * Math.Sin(AstronomyMath.DegreesToRadians(meanAnomaly - 2 * D + sunMeanAnomaly));
            longPert += +0.053 * Math.Sin(AstronomyMath.DegreesToRadians(meanAnomaly + 2 * D));
            longPert += +0.046 * Math.Sin(AstronomyMath.DegreesToRadians(2 * D - sunMeanAnomaly));
            longPert += +0.041 * Math.Sin(AstronomyMath.DegreesToRadians(meanAnomaly - sunMeanAnomaly));
            longPert += -0.035 * Math.Sin(AstronomyMath.DegreesToRadians(D));                         // Parallactic equation
            longPert += -0.031 * Math.Sin(AstronomyMath.DegreesToRadians(meanAnomaly + sunMeanAnomaly));
            longPert += -0.015 * Math.Sin(AstronomyMath.DegreesToRadians(2 * F - 2 * D));
            longPert += +0.011 * Math.Sin(AstronomyMath.DegreesToRadians(meanAnomaly - 4 * D));

            // Latitude perturbations
            double latPert = 0;
            latPert += -0.173 * Math.Sin(AstronomyMath.DegreesToRadians(F - 2 * D));
            latPert += -0.055 * Math.Sin(AstronomyMath.DegreesToRadians(meanAnomaly - F - 2 * D));
            latPert += -0.046 * Math.Sin(AstronomyMath.DegreesToRadians(meanAnomaly + F - 2 * D));
            latPert += +0.033 * Math.Sin(AstronomyMath.DegreesToRadians(F + 2 * D));
            latPert += +0.017 * Math.Sin(AstronomyMath.DegreesToRadians(2 * meanAnomaly + F));

            // Distance perturbation
            double distPert = -0.58 * Math.Cos(AstronomyMath.DegreesToRadians(meanAnomaly - 2 * D))
                - 0.46 * Math.Cos(AstronomyMath.DegreesToRadians(2 * D));

            moonLongitude += longPert;
            moonLatitude += latPert;
            moonDist += distPert;

            // Heliocentric → equatorial
            var xh = moonDist * Math.Cos(AstronomyMath.DegreesToRadians(moonLongitude)) * Math.Cos(AstronomyMath.DegreesToRadians(moonLatitude));
            var yh = moonDist * Math.Sin(AstronomyMath.DegreesToRadians(moonLongitude)) * Math.Cos(AstronomyMath.DegreesToRadians(moonLatitude));
            var zh = moonDist * Math.Sin(AstronomyMath.DegreesToRadians(moonLatitude));

            var xequat = xh;
            var yequat = yh * Math.Cos(AstronomyMath.DegreesToRadians(sunEclipticObliquity)) - zh * Math.Sin(AstronomyMath.DegreesToRadians(sunEclipticObliquity));
            var zequat = yh * Math.Sin(AstronomyMath.DegreesToRadians(sunEclipticObliquity)) + zh * Math.Cos(AstronomyMath.DegreesToRadians(sunEclipticObliquity));

            rightAscension = AstronomyMath.NormalizeDegrees(AstronomyMath.RadiansToDegrees(Math.Atan2(yequat, xequat)));
            declination = AstronomyMath.RadiansToDegrees(Math.Atan2(zequat, Math.Sqrt(xequat * xequat + yequat * yequat)));

            return moonDist;
        }

        // ────────────────────────────────────────────────────────────────
        //  Moon age and phase
        // ────────────────────────────────────────────────────────────────

        /// <summary>
        /// Calculates the age of the Moon in whole days (0–29) for the given date.
        /// </summary>
        /// <param name="date">The calendar date.</param>
        /// <returns>Moon age in days (0 = new moon, 14 ≈ full moon).</returns>
        public static int CalculateMoonAge(DateTime date)
        {
            var dayNumber = AstronomyMath.J2000_UT(date) + 2_451_545.0;
            dayNumber = (dayNumber - 2_451_550.1) / 29.530588853;

            dayNumber = dayNumber - (int)dayNumber;
            if (dayNumber < 0)
            {
                dayNumber += 1;
            }

            return (int)(dayNumber * 29.53);
        }

        /// <summary>
        /// Returns the name of the Moon phase for the given date.
        /// </summary>
        /// <param name="date">The calendar date.</param>
        /// <returns>
        /// One of: "New Moon", "Waxing Crescent", "First Quarter", "Waxing Gibbous",
        /// "Full Moon", "Waning Gibbous", "Last Quarter", "Waning Crescent", or empty string.
        /// </returns>
        public static string CalculateMoonPhase(DateTime date)
        {
            var moonAge = CalculateMoonAge(date);

            if (moonAge == 0 || moonAge == 29)
                return "New Moon";
            if (moonAge >= 1 && moonAge <= 6)
                return "Waxing Crescent";   // BUG FIX: was "Cresent" in legacy code
            if (moonAge == 7)
                return "First Quarter";
            if (moonAge >= 8 && moonAge <= 13)
                return "Waxing Gibbous";
            if (moonAge == 14)
                return "Full Moon";
            if (moonAge >= 15 && moonAge <= 21)
                return "Waning Gibbous";
            if (moonAge == 22)
                return "Last Quarter";
            if (moonAge >= 23 && moonAge <= 28)
                return "Waning Crescent";   // BUG FIX: was "Cresent" in legacy code

            return string.Empty;
        }
    }
}
