using System;
using System.Collections.Generic;

namespace HVO.Astronomy
{
    /// <summary>
    /// Provides celestial mechanics helpers for computing topocentric planet positions
    /// using low-precision Keplerian ephemerides (JPL J2000 elements).
    /// </summary>
    /// <remarks>
    /// Migrated from HVOv9-SkyMonitorV6 Imaging.Rendering.Planets.PlanetMath.
    /// Adapted for .NET Standard 2.0 (no <c>Math.Tau</c> / <c>Math.Clamp</c>).
    /// </remarks>
    public static class PlanetMath
    {
        private const double TwoPi = 2.0 * Math.PI;
        private const double ObliquityDeg = 23.439291;
        private const double AuLightTimeDays = 0.0057755183;
        private const double EarthRadiusAu = 1.0 / 23455.0;

        // ────────────────────────────────────────────────────────────────
        //  Kepler orbital elements (JPL J2000 epoch, 3000 BCE – 3000 CE)
        // ────────────────────────────────────────────────────────────────

        private static readonly Dictionary<PlanetBody, double[]> KeplerJ2000 = new Dictionary<PlanetBody, double[]>
        {
            //                         a          aDot          e           eDot         i           iDot
            //                         L          LDot          w           wDot         O           ODot
            [PlanetBody.Mercury] = new[] { 0.38709927, 0.00000037, 0.20563593,  0.00001906,  7.00497902, -0.00594749,
                                           252.25032350, 149472.67411175,  77.45779628, 0.16047689,  48.33076593, -0.12534081 },

            [PlanetBody.Venus] = new[] { 0.72333566, 0.00000390, 0.00677672, -0.00004107,  3.39467605, -0.00078890,
                                           181.97909950,  58517.81538729, 131.60246718, 0.00268329,  76.67984255, -0.27769418 },

            [PlanetBody.Mars] = new[] { 1.52371034, 0.00001847, 0.09339410,  0.00007882,  1.84969142, -0.00813131,
                                           -4.55343205,  19140.30268499, -23.94362959, 0.44441088,  49.55953891, -0.29257343 },

            [PlanetBody.Jupiter] = new[] { 5.20288700,-0.00011607, 0.04838624, -0.00013253,  1.30439695, -0.00183714,
                                            34.39644051,  3034.74612775,  14.72847983, 0.21252668, 100.47390909,  0.20469106 },

            [PlanetBody.Saturn] = new[] { 9.53667594,-0.00125060, 0.05386179, -0.00050991,  2.48599187,  0.00193609,
                                            49.95424423,  1222.49362201,  92.59887831,-0.41897216, 113.66242448, -0.28867794 },

            [PlanetBody.Uranus] = new[] { 19.18916464,-0.00196176, 0.04725744, -0.00004397,  0.77263783, -0.00242939,
                                            313.23810451,   428.48202785, 170.95427630, 0.40805281,  74.01692503,  0.04240589 },

            [PlanetBody.Neptune] = new[] { 30.06992276, 0.00026291, 0.00859048,  0.00005105,  1.77004347,  0.00035372,
                                            -55.12002969,   218.45945325,  44.96476227,-0.32241464, 131.78422574, -0.00508664 },

            [PlanetBody.Sun] = new[] { 1.00000261, 0.00000562, 0.01671123, -0.00004392, -0.00001531,-0.01294668,
                                           100.46457166, 35999.37244981, 102.93768193, 0.32327364,  0.0,          0.0 }
        };

        // ────────────────────────────────────────────────────────────────
        //  Public API
        // ────────────────────────────────────────────────────────────────

        /// <summary>
        /// Computes the topocentric equatorial coordinates for one or more Solar System bodies.
        /// </summary>
        /// <param name="latitudeDeg">Observer latitude in degrees (positive north).</param>
        /// <param name="longitudeDeg">Observer longitude in degrees (positive east).</param>
        /// <param name="utc">UTC timestamp.</param>
        /// <param name="bodies">Collection of <see cref="PlanetBody"/> values to compute.</param>
        /// <returns>A list of <see cref="PlanetPosition"/> results.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="bodies"/> is <c>null</c>.</exception>
        public static IReadOnlyList<PlanetPosition> ComputeTopocentricPositions(
            double latitudeDeg,
            double longitudeDeg,
            DateTime utc,
            IReadOnlyCollection<PlanetBody> bodies)
        {
            if (bodies == null) throw new ArgumentNullException(nameof(bodies));
            if (bodies.Count == 0) return new PlanetPosition[0];

            var results = new List<PlanetPosition>(bodies.Count);
            foreach (var body in bodies)
            {
                results.Add(ComputeTopocentricPosition(body, latitudeDeg, longitudeDeg, utc));
            }

            return results;
        }

        // ────────────────────────────────────────────────────────────────
        //  Per-body dispatch
        // ────────────────────────────────────────────────────────────────

        private static PlanetPosition ComputeTopocentricPosition(
            PlanetBody body,
            double latitudeDeg,
            double longitudeDeg,
            DateTime utc)
        {
            double raHours, decDeg, distanceAu;

            if (body == PlanetBody.Moon)
            {
                MoonGeocentricEquatorial(utc, out raHours, out decDeg, out distanceAu);
            }
            else if (body == PlanetBody.Sun)
            {
                GeocentricSunEquatorial(utc, out raHours, out decDeg, out distanceAu);
            }
            else
            {
                GeocentricEquatorial(body, utc, out raHours, out decDeg, out distanceAu);
            }

            ApplyTopocentricParallax(raHours, decDeg, distanceAu, latitudeDeg, longitudeDeg, utc, out raHours, out decDeg);
            return new PlanetPosition(body, raHours, decDeg, distanceAu);
        }

        // ────────────────────────────────────────────────────────────────
        //  Geocentric equatorial (planets via Keplerian elements)
        // ────────────────────────────────────────────────────────────────

        private static void GeocentricEquatorial(PlanetBody body, DateTime utc, out double raHours, out double decDeg, out double distAu)
        {
            var jd = AstronomyMath.JulianDate(utc);
            var t = (jd - 2_451_545.0) / 36_525.0;

            var earth = KeplerJ2000[PlanetBody.Sun];
            HeliocentricEcliptic(earth, t, out var xE, out var yE, out var zE, out _);

            var planetElements = KeplerJ2000[body];
            HeliocentricEcliptic(planetElements, t, out var xP, out var yP, out var zP, out _);

            var x0 = xP - xE;
            var y0 = yP - yE;
            var z0 = zP - zE;
            var dist0 = Math.Sqrt(x0 * x0 + y0 * y0 + z0 * z0);
            var tau = dist0 * AuLightTimeDays;

            // Light-time corrected position
            var tPlanet = ((jd - tau) - 2_451_545.0) / 36_525.0;
            HeliocentricEcliptic(planetElements, tPlanet, out xP, out yP, out zP, out _);

            var x = xP - xE;
            var y = yP - yE;
            var z = zP - zE;
            distAu = Math.Sqrt(x * x + y * y + z * z);

            EclipticVectorToRaDec(x, y, z, out raHours, out decDeg);
        }

        private static void GeocentricSunEquatorial(DateTime utc, out double raHours, out double decDeg, out double distAu)
        {
            var jd = AstronomyMath.JulianDate(utc);
            var t = (jd - 2_451_545.0) / 36_525.0;

            var earth = KeplerJ2000[PlanetBody.Sun];
            HeliocentricEcliptic(earth, t, out var xE, out var yE, out var zE, out var rE);

            EclipticVectorToRaDec(-xE, -yE, -zE, out raHours, out decDeg);
            distAu = rE;
        }

        // ────────────────────────────────────────────────────────────────
        //  Heliocentric ecliptic position from Kepler elements
        // ────────────────────────────────────────────────────────────────

        private static void HeliocentricEcliptic(double[] elements, double t, out double x, out double y, out double z, out double r)
        {
            var a = elements[0] + elements[1] * t;
            var e = elements[2] + elements[3] * t;
            var i = AstronomyMath.DegreesToRadians(elements[4] + elements[5] * t);
            var l = AstronomyMath.NormalizeDegrees(elements[6] + elements[7] * t);
            var p = AstronomyMath.NormalizeDegrees(elements[8] + elements[9] * t);
            var o = AstronomyMath.NormalizeDegrees(elements[10] + elements[11] * t);

            var m = AstronomyMath.DegreesToRadians(AstronomyMath.NormalizeDegrees(l - p));
            var w = AstronomyMath.DegreesToRadians(AstronomyMath.NormalizeDegrees(p - o));

            // Solve Kepler's equation (6 iterations, Newton–Raphson)
            var eAnomaly = m;
            for (var iter = 0; iter < 6; iter++)
            {
                var f = eAnomaly - e * Math.Sin(eAnomaly) - m;
                var fp = 1.0 - e * Math.Cos(eAnomaly);
                eAnomaly -= f / fp;
            }

            var cosE = Math.Cos(eAnomaly);
            var sinE = Math.Sin(eAnomaly);
            var nu = Math.Atan2(Math.Sqrt(1 - e * e) * sinE, cosE - e);
            r = a * (1 - e * cosE);

            var xOrb = r * Math.Cos(nu);
            var yOrb = r * Math.Sin(nu);

            var cO = Math.Cos(AstronomyMath.DegreesToRadians(o));
            var sO = Math.Sin(AstronomyMath.DegreesToRadians(o));
            var cI = Math.Cos(i);
            var sI = Math.Sin(i);
            var cw = Math.Cos(w);
            var sw = Math.Sin(w);

            x = (cO * cw - sO * sw * cI) * xOrb + (-cO * sw - sO * cw * cI) * yOrb;
            y = (sO * cw + cO * sw * cI) * xOrb + (-sO * sw + cO * cw * cI) * yOrb;
            z = (sw * sI) * xOrb + (cw * sI) * yOrb;
        }

        // ────────────────────────────────────────────────────────────────
        //  Ecliptic vector → RA/Dec
        // ────────────────────────────────────────────────────────────────

        private static void EclipticVectorToRaDec(double x, double y, double z, out double raHours, out double decDeg)
        {
            var eps = AstronomyMath.DegreesToRadians(ObliquityDeg);
            var xe = x;
            var ye = y * Math.Cos(eps) - z * Math.Sin(eps);
            var ze = y * Math.Sin(eps) + z * Math.Cos(eps);

            var ra = Math.Atan2(ye, xe);
            if (ra < 0)
            {
                ra += TwoPi;
            }

            var dec = Math.Atan2(ze, Math.Sqrt(xe * xe + ye * ye));
            raHours = ra * 12.0 / Math.PI;
            decDeg = AstronomyMath.RadiansToDegrees(dec);
        }

        // ────────────────────────────────────────────────────────────────
        //  Moon geocentric (low-precision Meeus)
        // ────────────────────────────────────────────────────────────────

        private static void MoonGeocentricEquatorial(DateTime utc, out double raHours, out double decDeg, out double distAu)
        {
            var jd = AstronomyMath.JulianDate(utc);
            var t = (jd - 2_451_545.0) / 36_525.0;

            var lp = AstronomyMath.NormalizeDegrees(218.3164477 + 481267.88123421 * t - 0.0015786 * t * t + t * t * t / 538841.0 - t * t * t * t / 65194000.0);
            var d = AstronomyMath.NormalizeDegrees(297.8501921 + 445267.1114034 * t - 0.0018819 * t * t + t * t * t / 545868.0 - t * t * t * t / 113065000.0);
            var m = AstronomyMath.NormalizeDegrees(357.5291092 + 35999.0502909 * t - 0.0001536 * t * t + t * t * t / 24490000.0);
            var mp = AstronomyMath.NormalizeDegrees(134.9633964 + 477198.8675055 * t + 0.0087414 * t * t + t * t * t / 69699.0 - t * t * t * t / 14712000.0);
            var f = AstronomyMath.NormalizeDegrees(93.2720950 + 483202.0175233 * t - 0.0036539 * t * t - t * t * t / 3526000.0 + t * t * t * t / 863310000.0);

            var dr = AstronomyMath.DegreesToRadians(d);
            var mr = AstronomyMath.DegreesToRadians(m);
            var mpr = AstronomyMath.DegreesToRadians(mp);
            var fr = AstronomyMath.DegreesToRadians(f);

            var lon = lp
                + 6.289 * Math.Sin(mpr)
                + 1.274 * Math.Sin(2 * dr - mpr)
                + 0.658 * Math.Sin(2 * dr)
                + 0.214 * Math.Sin(2 * mpr)
                - 0.186 * Math.Sin(mr)
                - 0.114 * Math.Sin(2 * fr);

            var lat = 5.128 * Math.Sin(fr)
                + 0.280 * Math.Sin(mpr + fr)
                + 0.277 * Math.Sin(mpr - fr)
                + 0.173 * Math.Sin(2 * dr - fr)
                + 0.055 * Math.Sin(2 * dr + fr)
                + 0.046 * Math.Sin(2 * dr - mpr + fr)
                + 0.033 * Math.Sin(2 * dr - mpr - fr)
                + 0.017 * Math.Sin(2 * mpr + fr);

            var distanceKm = 385001.0
                - 20905.0 * Math.Cos(mpr)
                - 3699.0 * Math.Cos(2 * dr - mpr)
                - 2956.0 * Math.Cos(2 * dr)
                - 570.0 * Math.Cos(2 * mpr);

            distAu = distanceKm / 149_597_870.700;

            EclipticToEquatorial(lon, lat, out raHours, out decDeg);
        }

        private static void EclipticToEquatorial(double lonDeg, double latDeg, out double raHours, out double decDeg)
        {
            var lon = AstronomyMath.DegreesToRadians(lonDeg);
            var lat = AstronomyMath.DegreesToRadians(latDeg);
            var eps = AstronomyMath.DegreesToRadians(ObliquityDeg);

            var x = Math.Cos(lat) * Math.Cos(lon);
            var y = Math.Cos(lat) * Math.Sin(lon) * Math.Cos(eps) - Math.Sin(lat) * Math.Sin(eps);
            var z = Math.Cos(lat) * Math.Sin(lon) * Math.Sin(eps) + Math.Sin(lat) * Math.Cos(eps);

            var ra = Math.Atan2(y, x);
            if (ra < 0)
            {
                ra += TwoPi;
            }

            var dec = Math.Asin(AstronomyMath.Clamp(z, -1.0, 1.0));
            raHours = ra * 12.0 / Math.PI;
            decDeg = AstronomyMath.RadiansToDegrees(dec);
        }

        // ────────────────────────────────────────────────────────────────
        //  Topocentric parallax
        // ────────────────────────────────────────────────────────────────

        private static void ApplyTopocentricParallax(
            double raHours,
            double decDeg,
            double distanceAu,
            double latitudeDeg,
            double longitudeDeg,
            DateTime utc,
            out double raHoursTop,
            out double decDegTop)
        {
            var ra = raHours * Math.PI / 12.0;
            var dec = AstronomyMath.DegreesToRadians(decDeg);
            var phi = AstronomyMath.DegreesToRadians(latitudeDeg);

            var lstHours = AstronomyMath.LocalSiderealTime(utc, longitudeDeg);
            var lst = lstHours * Math.PI / 12.0;

            var h = lst - ra;
            var pi = Math.Asin(EarthRadiusAu / Math.Max(distanceAu, 1e-6));

            var dRa = -pi * Math.Cos(phi) * Math.Sin(h) / Math.Cos(dec);
            var dDec = -pi * (Math.Sin(phi) * Math.Cos(dec) - Math.Cos(phi) * Math.Cos(h) * Math.Sin(dec));

            var raTop = ra + dRa;
            var decTop = dec + dDec;

            raHoursTop = (raTop * 12.0 / Math.PI) % 24.0;
            if (raHoursTop < 0)
            {
                raHoursTop += 24.0;
            }

            decDegTop = AstronomyMath.RadiansToDegrees(decTop);
        }
    }

    /// <summary>
    /// Represents the topocentric equatorial position of a Solar System body.
    /// </summary>
    public sealed class PlanetPosition
    {
        /// <summary>
        /// Initializes a new <see cref="PlanetPosition"/>.
        /// </summary>
        /// <param name="body">The Solar System body.</param>
        /// <param name="rightAscensionHours">Right ascension in hours [0, 24).</param>
        /// <param name="declinationDegrees">Declination in degrees [-90, +90].</param>
        /// <param name="distanceAu">Distance from Earth in AU.</param>
        public PlanetPosition(PlanetBody body, double rightAscensionHours, double declinationDegrees, double distanceAu)
        {
            Body = body;
            RightAscensionHours = rightAscensionHours;
            DeclinationDegrees = declinationDegrees;
            DistanceAu = distanceAu;
        }

        /// <summary>The Solar System body.</summary>
        public PlanetBody Body { get; }

        /// <summary>Right ascension in hours [0, 24).</summary>
        public double RightAscensionHours { get; }

        /// <summary>Declination in degrees [-90, +90].</summary>
        public double DeclinationDegrees { get; }

        /// <summary>Distance from Earth in AU.</summary>
        public double DistanceAu { get; }
    }
}
