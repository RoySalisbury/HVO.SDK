using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace HVO.Astronomy.Tests
{
    [TestClass]
    public class AstronomyMathTests
    {
        private const double Tolerance = 1e-10;
        private const double LooseTolerance = 1e-6;

        // ────────────────────────────────────────────────────────────────
        //  Angle conversions
        // ────────────────────────────────────────────────────────────────

        [TestMethod]
        public void DegreesToRadians_90Degrees_ReturnsHalfPi()
        {
            Assert.AreEqual(Math.PI / 2.0, AstronomyMath.DegreesToRadians(90.0), Tolerance);
        }

        [TestMethod]
        public void DegreesToRadians_360Degrees_ReturnsTwoPi()
        {
            Assert.AreEqual(2.0 * Math.PI, AstronomyMath.DegreesToRadians(360.0), Tolerance);
        }

        [TestMethod]
        public void DegreesToRadians_ZeroDegrees_ReturnsZero()
        {
            Assert.AreEqual(0.0, AstronomyMath.DegreesToRadians(0.0), Tolerance);
        }

        [TestMethod]
        public void RadiansToDegrees_Pi_Returns180()
        {
            Assert.AreEqual(180.0, AstronomyMath.RadiansToDegrees(Math.PI), Tolerance);
        }

        [TestMethod]
        public void RadiansToDegrees_ZeroRadians_ReturnsZero()
        {
            Assert.AreEqual(0.0, AstronomyMath.RadiansToDegrees(0.0), Tolerance);
        }

        [TestMethod]
        public void DegreesToRadians_RoundTrip_PreservesValue()
        {
            var degrees = 123.456;
            var result = AstronomyMath.RadiansToDegrees(AstronomyMath.DegreesToRadians(degrees));
            Assert.AreEqual(degrees, result, Tolerance);
        }

        // ────────────────────────────────────────────────────────────────
        //  NormalizeDegrees
        // ────────────────────────────────────────────────────────────────

        [TestMethod]
        public void NormalizeDegrees_PositiveOverflow_Normalizes()
        {
            Assert.AreEqual(10.0, AstronomyMath.NormalizeDegrees(370.0), Tolerance);
        }

        [TestMethod]
        public void NormalizeDegrees_NegativeAngle_Normalizes()
        {
            Assert.AreEqual(350.0, AstronomyMath.NormalizeDegrees(-10.0), Tolerance);
        }

        [TestMethod]
        public void NormalizeDegrees_Exactly360_ReturnsZero()
        {
            Assert.AreEqual(0.0, AstronomyMath.NormalizeDegrees(360.0), Tolerance);
        }

        [TestMethod]
        public void NormalizeDegrees_Zero_ReturnsZero()
        {
            Assert.AreEqual(0.0, AstronomyMath.NormalizeDegrees(0.0), Tolerance);
        }

        [TestMethod]
        public void NormalizeDegrees_LargeNegative_Normalizes()
        {
            Assert.AreEqual(0.0, AstronomyMath.NormalizeDegrees(-720.0), Tolerance);
        }

        // ────────────────────────────────────────────────────────────────
        //  NormalizeDegrees180
        // ────────────────────────────────────────────────────────────────

        [TestMethod]
        public void NormalizeDegrees180_190_ReturnsMinus170()
        {
            Assert.AreEqual(-170.0, AstronomyMath.NormalizeDegrees180(190.0), Tolerance);
        }

        [TestMethod]
        public void NormalizeDegrees180_Minus190_Returns170()
        {
            Assert.AreEqual(170.0, AstronomyMath.NormalizeDegrees180(-190.0), Tolerance);
        }

        [TestMethod]
        public void NormalizeDegrees180_Zero_ReturnsZero()
        {
            Assert.AreEqual(0.0, AstronomyMath.NormalizeDegrees180(0.0), Tolerance);
        }

        // ────────────────────────────────────────────────────────────────
        //  NormalizeHours
        // ────────────────────────────────────────────────────────────────

        [TestMethod]
        public void NormalizeHours_25Hours_Returns1()
        {
            Assert.AreEqual(1.0, AstronomyMath.NormalizeHours(25.0), Tolerance);
        }

        [TestMethod]
        public void NormalizeHours_NegativeHours_Normalizes()
        {
            Assert.AreEqual(22.0, AstronomyMath.NormalizeHours(-2.0), Tolerance);
        }

        [TestMethod]
        public void NormalizeHours_Zero_ReturnsZero()
        {
            Assert.AreEqual(0.0, AstronomyMath.NormalizeHours(0.0), Tolerance);
        }

        // ────────────────────────────────────────────────────────────────
        //  Julian Date
        // ────────────────────────────────────────────────────────────────

        [TestMethod]
        public void JulianDate_J2000Epoch_ReturnsKnownValue()
        {
            // J2000.0 = 2000-01-01 12:00:00 UTC → JD 2451545.0
            var utc = new DateTime(2000, 1, 1, 12, 0, 0, DateTimeKind.Utc);
            Assert.AreEqual(2_451_545.0, AstronomyMath.JulianDate(utc), 0.01);
        }

        [TestMethod]
        public void JulianDate_UnixEpoch_ReturnsKnownValue()
        {
            // 1970-01-01 00:00:00 UTC → JD 2440587.5
            var utc = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            Assert.AreEqual(2_440_587.5, AstronomyMath.JulianDate(utc), 0.01);
        }

        // ────────────────────────────────────────────────────────────────
        //  J2000 day number
        // ────────────────────────────────────────────────────────────────

        [TestMethod]
        public void J2000_AtEpoch_ReturnsZero()
        {
            var epoch = new DateTime(2000, 1, 1, 12, 0, 0, DateTimeKind.Utc);
            Assert.AreEqual(0.0, AstronomyMath.J2000(epoch), Tolerance);
        }

        [TestMethod]
        public void J2000_OneDayLater_ReturnsOne()
        {
            var date = new DateTime(2000, 1, 2, 12, 0, 0, DateTimeKind.Utc);
            Assert.AreEqual(1.0, AstronomyMath.J2000(date), Tolerance);
        }

        // ────────────────────────────────────────────────────────────────
        //  Julian centuries
        // ────────────────────────────────────────────────────────────────

        [TestMethod]
        public void JulianCenturies_AtJ2000_ReturnsZero()
        {
            Assert.AreEqual(0.0, AstronomyMath.JulianCenturies(2_451_545.0), Tolerance);
        }

        [TestMethod]
        public void JulianCenturies_OneCenturyLater_ReturnsOne()
        {
            Assert.AreEqual(1.0, AstronomyMath.JulianCenturies(2_451_545.0 + 36_525.0), Tolerance);
        }

        // ────────────────────────────────────────────────────────────────
        //  Local Sidereal Time
        // ────────────────────────────────────────────────────────────────

        [TestMethod]
        public void LocalSiderealTime_ReturnsValueIn0To24Range()
        {
            var utc = new DateTime(2024, 6, 15, 3, 0, 0, DateTimeKind.Utc);
            var lst = AstronomyMath.LocalSiderealTime(utc, -113.81);
            Assert.IsTrue(lst >= 0.0 && lst < 24.0, $"LST should be [0,24), was {lst}");
        }

        [TestMethod]
        public void LocalSiderealTime_DifferentLongitudes_ProducesDifferentValues()
        {
            var utc = new DateTime(2024, 6, 15, 3, 0, 0, DateTimeKind.Utc);
            var lst1 = AstronomyMath.LocalSiderealTime(utc, 0.0);
            var lst2 = AstronomyMath.LocalSiderealTime(utc, -113.81);
            Assert.AreNotEqual(lst1, lst2);
        }

        // ────────────────────────────────────────────────────────────────
        //  EquatorialToHorizontal
        // ────────────────────────────────────────────────────────────────

        [TestMethod]
        public void EquatorialToHorizontal_ReturnsValidAltitudeRange()
        {
            var (alt, az) = AstronomyMath.EquatorialToHorizontal(6.0, 20.0, 6.0, 35.18);
            Assert.IsTrue(alt >= -90.0 && alt <= 90.0, $"Altitude should be [-90,90], was {alt}");
            Assert.IsTrue(az >= 0.0 && az <= 360.0, $"Azimuth should be [0,360], was {az}");
        }

        [TestMethod]
        public void EquatorialToHorizontal_ObjectAtMeridian_AzimuthNearSouth()
        {
            // When RA = LST (hour angle = 0), object is at the meridian
            // For a low-declination star in the northern hemisphere, azimuth ≈ 180° (due south)
            var (_, az) = AstronomyMath.EquatorialToHorizontal(6.0, 10.0, 6.0, 35.18);
            Assert.IsTrue(Math.Abs(az - 180.0) < 1.0, $"Azimuth at meridian should be ~180°, was {az}");
        }

        // ────────────────────────────────────────────────────────────────
        //  Bennett refraction
        // ────────────────────────────────────────────────────────────────

        [TestMethod]
        public void BennettRefractionDegrees_AtHorizon_ReturnsApproxHalfDegree()
        {
            var refraction = AstronomyMath.BennettRefractionDegrees(0.0);
            Assert.IsTrue(refraction > 0.4 && refraction < 0.7, $"Refraction at horizon should be ~0.5°, was {refraction}°");
        }

        [TestMethod]
        public void BennettRefractionDegrees_AtZenith_ReturnsNearZero()
        {
            var refraction = AstronomyMath.BennettRefractionDegrees(90.0);
            Assert.IsTrue(refraction < 0.01, $"Refraction at zenith should be ~0°, was {refraction}°");
        }

        [TestMethod]
        public void BennettRefractionDegrees_NegativeAltitude_DoesNotThrow()
        {
            var refraction = AstronomyMath.BennettRefractionDegrees(-5.0);
            Assert.IsTrue(refraction > 0);
        }

        // ────────────────────────────────────────────────────────────────
        //  Sidereal rotation
        // ────────────────────────────────────────────────────────────────

        [TestMethod]
        public void CalculateSiderealRotationFraction_ReturnsValueBetween0And1()
        {
            var timestamp = new DateTimeOffset(2024, 6, 15, 3, 0, 0, TimeSpan.Zero);
            var fraction = AstronomyMath.CalculateSiderealRotationFraction(timestamp, -113.81);
            Assert.IsTrue(fraction >= 0.0 && fraction < 1.0, $"Fraction should be [0,1), was {fraction}");
        }

        [TestMethod]
        public void CalculateSkyRotationDegrees_ReturnsFloat()
        {
            var timestamp = new DateTimeOffset(2024, 6, 15, 3, 0, 0, TimeSpan.Zero);
            var rotation = AstronomyMath.CalculateSkyRotationDegrees(timestamp, -113.81);
            Assert.IsTrue(!float.IsNaN(rotation) && !float.IsInfinity(rotation));
        }
    }
}
