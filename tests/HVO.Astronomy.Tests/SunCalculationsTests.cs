using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace HVO.Astronomy.Tests
{
    [TestClass]
    public class SunCalculationsTests
    {
        // HVO coordinates (Kingman, AZ area)
        private const double HvoLatitude = 35.18;
        private const double HvoLongitude = -113.81;
        private const double Tolerance = 0.5; // degrees — low-precision algorithm

        // ────────────────────────────────────────────────────────────────
        //  RA / Dec
        // ────────────────────────────────────────────────────────────────

        [TestMethod]
        public void SunRightAscensionDeclination_SummerSolstice_DeclinationNear23()
        {
            // 2024 summer solstice ≈ June 20
            var dayNumber = AstronomyMath.J2000_UT(new DateTime(2024, 6, 20, 12, 0, 0, DateTimeKind.Utc));
            double ra, dec;
            SunCalculations.SunRightAscensionDeclination(dayNumber, out ra, out dec);

            Assert.IsTrue(Math.Abs(dec - 23.44) < 1.0, $"Summer solstice declination should be ~23.44°, was {dec}°");
        }

        [TestMethod]
        public void SunRightAscensionDeclination_WinterSolstice_DeclinationNearMinus23()
        {
            // 2024 winter solstice ≈ December 21
            var dayNumber = AstronomyMath.J2000_UT(new DateTime(2024, 12, 21, 12, 0, 0, DateTimeKind.Utc));
            double ra, dec;
            SunCalculations.SunRightAscensionDeclination(dayNumber, out ra, out dec);

            Assert.IsTrue(Math.Abs(dec + 23.44) < 1.0, $"Winter solstice declination should be ~-23.44°, was {dec}°");
        }

        [TestMethod]
        public void SunRightAscensionDeclination_DistanceReasonable()
        {
            var dayNumber = AstronomyMath.J2000_UT(new DateTime(2024, 6, 15, 12, 0, 0, DateTimeKind.Utc));
            double ra, dec;
            var distance = SunCalculations.SunRightAscensionDeclination(dayNumber, out ra, out dec);

            // Sun–Earth distance is approximately 0.98–1.02 AU
            Assert.IsTrue(distance > 0.95 && distance < 1.05, $"Distance should be ~1 AU, was {distance}");
        }

        // ────────────────────────────────────────────────────────────────
        //  Altitude / Azimuth
        // ────────────────────────────────────────────────────────────────

        [TestMethod]
        public void SunAltitudeAzimuth_Midday_PositiveAltitude()
        {
            // Noon UTC on June 15 at HVO longitude (-113.81°) is ~5:25 AM local
            // Use a time when the Sun is up at HVO: ~19:00 UTC = ~noon MST
            var dayNumber = AstronomyMath.J2000_UT(new DateTime(2024, 6, 15, 19, 0, 0, DateTimeKind.Utc));
            double alt, az;
            SunCalculations.SunAltitudeAzimuth(dayNumber, HvoLatitude, HvoLongitude, out alt, out az);

            Assert.IsTrue(alt > 0.0, $"Sun altitude at local noon should be positive, was {alt}°");
        }

        // ────────────────────────────────────────────────────────────────
        //  Sunrise / Sunset
        // ────────────────────────────────────────────────────────────────

        [TestMethod]
        public void SunriseSunset_NormalDay_ReturnsZero()
        {
            var date = new DateTime(2024, 6, 15, 0, 0, 0, DateTimeKind.Utc);
            DateTimeOffset sunrise, sunset;
            var result = SunCalculations.SunriseSunset(date, HvoLatitude, HvoLongitude, out sunrise, out sunset);

            Assert.AreEqual(0, result, "Mid-latitude summer day should return 0 (normal)");
            Assert.IsTrue(sunrise < sunset, "Sunrise should be before sunset");
        }

        [TestMethod]
        public void SunriseSunset_SunriseHourReasonable()
        {
            var date = new DateTime(2024, 6, 15, 0, 0, 0, DateTimeKind.Utc);
            DateTimeOffset sunrise, sunset;
            SunCalculations.SunriseSunset(date, HvoLatitude, HvoLongitude, out sunrise, out sunset);

            // HVO sunrise in June is around 12:20 UTC (5:20 AM MST)
            Assert.IsTrue(sunrise.Hour >= 11 && sunrise.Hour <= 14, $"Sunrise UTC hour should be ~12, was {sunrise.Hour}");
        }

        // ────────────────────────────────────────────────────────────────
        //  Day length
        // ────────────────────────────────────────────────────────────────

        [TestMethod]
        public void DayLength_SummerSolstice_LongestDay()
        {
            var summer = new DateTime(2024, 6, 21, 0, 0, 0, DateTimeKind.Utc);
            var winter = new DateTime(2024, 12, 21, 0, 0, 0, DateTimeKind.Utc);

            var summerLen = SunCalculations.DayLength(summer, HvoLatitude, HvoLongitude);
            var winterLen = SunCalculations.DayLength(winter, HvoLatitude, HvoLongitude);

            Assert.IsTrue(summerLen > winterLen, "Summer day should be longer than winter day");
            Assert.IsTrue(summerLen > 12.0 && summerLen < 16.0, $"Summer day length at 35°N should be ~14h, was {summerLen}h");
        }

        // ────────────────────────────────────────────────────────────────
        //  Twilight
        // ────────────────────────────────────────────────────────────────

        [TestMethod]
        public void CivilTwilight_NormalDay_StartBeforeSunrise()
        {
            var date = new DateTime(2024, 6, 15, 0, 0, 0, DateTimeKind.Utc);
            DateTimeOffset sunrise, sunset, twilightStart, twilightEnd;

            SunCalculations.SunriseSunset(date, HvoLatitude, HvoLongitude, out sunrise, out sunset);
            SunCalculations.CivilTwilight(date, HvoLatitude, HvoLongitude, out twilightStart, out twilightEnd);

            Assert.IsTrue(twilightStart < sunrise, "Civil twilight should start before sunrise");
            Assert.IsTrue(twilightEnd > sunset, "Civil twilight should end after sunset");
        }

        [TestMethod]
        public void AstronomicalTwilight_NormalDay_ReturnsZero()
        {
            var date = new DateTime(2024, 6, 15, 0, 0, 0, DateTimeKind.Utc);
            DateTimeOffset twilightStart, twilightEnd;
            var result = SunCalculations.AstronomicalTwilight(date, HvoLatitude, HvoLongitude, out twilightStart, out twilightEnd);

            Assert.AreEqual(0, result, "Mid-latitude location should have normal astronomical twilight");
        }

        [TestMethod]
        public void NauticalTwilight_NormalDay_ReturnsZero()
        {
            var date = new DateTime(2024, 6, 15, 0, 0, 0, DateTimeKind.Utc);
            DateTimeOffset twilightStart, twilightEnd;
            var result = SunCalculations.NauticalTwilight(date, HvoLatitude, HvoLongitude, out twilightStart, out twilightEnd);

            Assert.AreEqual(0, result);
        }

        [TestMethod]
        public void CivilTwilightDayLength_LongerThanDayLength()
        {
            var date = new DateTime(2024, 6, 15, 0, 0, 0, DateTimeKind.Utc);
            var dayLen = SunCalculations.DayLength(date, HvoLatitude, HvoLongitude);
            var civilLen = SunCalculations.CivilTwilightDayLength(date, HvoLatitude, HvoLongitude);

            Assert.IsTrue(civilLen > dayLen, "Civil twilight length should exceed day length");
        }

        [TestMethod]
        public void AstronomicalTwilightDayLength_LongerThanNautical()
        {
            var date = new DateTime(2024, 6, 15, 0, 0, 0, DateTimeKind.Utc);
            var nauticalLen = SunCalculations.NauticalTwilightDayLength(date, HvoLatitude, HvoLongitude);
            var astroLen = SunCalculations.AstronomicalTwilightDayLength(date, HvoLatitude, HvoLongitude);

            Assert.IsTrue(astroLen > nauticalLen, "Astronomical twilight length should exceed nautical");
        }
    }
}
