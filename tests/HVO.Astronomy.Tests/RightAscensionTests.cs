using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace HVO.Astronomy.Tests
{
    [TestClass]
    public class RightAscensionTests
    {
        private const double Tolerance = 1e-4;

        // ────────────────────────────────────────────────────────────────
        //  Constructor
        // ────────────────────────────────────────────────────────────────

        [TestMethod]
        public void Constructor_ValidValues_SetsProperties()
        {
            var ra = new RightAscension(12, 30, 45.1234);

            Assert.AreEqual(12, ra.Hours);
            Assert.AreEqual(30, ra.Minutes);
            Assert.AreEqual(45.1234, ra.Seconds, Tolerance);
        }

        [TestMethod]
        public void Constructor_ZeroValues_SetsProperties()
        {
            var ra = new RightAscension(0, 0, 0.0);

            Assert.AreEqual(0, ra.Hours);
            Assert.AreEqual(0, ra.Minutes);
            Assert.AreEqual(0.0, ra.Seconds, Tolerance);
        }

        [TestMethod]
        public void Constructor_MaxValidValues_SetsProperties()
        {
            var ra = new RightAscension(23, 59, 59.9999);

            Assert.AreEqual(23, ra.Hours);
            Assert.AreEqual(59, ra.Minutes);
            Assert.AreEqual(59.9999, ra.Seconds, Tolerance);
        }

        [TestMethod]
        public void Constructor_NegativeHours_ThrowsArgumentOutOfRange()
        {
            Assert.ThrowsExactly<ArgumentOutOfRangeException>(() =>
                new RightAscension(-1, 0, 0.0));
        }

        [TestMethod]
        public void Constructor_Hours24_ThrowsArgumentOutOfRange()
        {
            Assert.ThrowsExactly<ArgumentOutOfRangeException>(() =>
                new RightAscension(24, 0, 0.0));
        }

        [TestMethod]
        public void Constructor_NegativeMinutes_ThrowsArgumentOutOfRange()
        {
            Assert.ThrowsExactly<ArgumentOutOfRangeException>(() =>
                new RightAscension(0, -1, 0.0));
        }

        [TestMethod]
        public void Constructor_Minutes60_ThrowsArgumentOutOfRange()
        {
            Assert.ThrowsExactly<ArgumentOutOfRangeException>(() =>
                new RightAscension(0, 60, 0.0));
        }

        [TestMethod]
        public void Constructor_NegativeSeconds_ThrowsArgumentOutOfRange()
        {
            Assert.ThrowsExactly<ArgumentOutOfRangeException>(() =>
                new RightAscension(0, 0, -0.001));
        }

        [TestMethod]
        public void Constructor_Seconds60_ThrowsArgumentOutOfRange()
        {
            Assert.ThrowsExactly<ArgumentOutOfRangeException>(() =>
                new RightAscension(0, 0, 60.0));
        }

        // ────────────────────────────────────────────────────────────────
        //  Factory Methods
        // ────────────────────────────────────────────────────────────────

        [TestMethod]
        public void FromHours_ValidValue_CreatesRightAscension()
        {
            var ra = RightAscension.FromHours(6.5);

            Assert.AreEqual(6, ra.Hours);
            Assert.AreEqual(30, ra.Minutes);
            Assert.AreEqual(0.0, ra.Seconds, Tolerance);
        }

        [TestMethod]
        public void FromDegrees_90Degrees_Creates6Hours()
        {
            // 90 degrees / 15 = 6 hours
            var ra = RightAscension.FromDegrees(90.0);

            Assert.AreEqual(6, ra.Hours);
            Assert.AreEqual(0, ra.Minutes);
            Assert.AreEqual(0.0, ra.Seconds, Tolerance);
        }

        [TestMethod]
        public void FromDegrees_NegativeValue_UsesAbsoluteValue()
        {
            var ra = RightAscension.FromDegrees(-90.0);

            Assert.AreEqual(6, ra.Hours);
            Assert.AreEqual(0, ra.Minutes);
        }

        [TestMethod]
        public void FromDegrees_360Degrees_Creates24Hours()
        {
            var ra = RightAscension.FromDegrees(360.0);

            Assert.AreEqual(24.0, ra.TotalHours, Tolerance);
        }

        [TestMethod]
        public void FromTimeSpan_ValidTimeSpan_CreatesRightAscension()
        {
            var ts = TimeSpan.FromHours(12.5);
            var ra = RightAscension.FromTimeSpan(ts);

            Assert.AreEqual(12, ra.Hours);
            Assert.AreEqual(30, ra.Minutes);
            Assert.AreEqual(0.0, ra.Seconds, Tolerance);
        }

        // ────────────────────────────────────────────────────────────────
        //  Degrees Property
        // ────────────────────────────────────────────────────────────────

        [TestMethod]
        public void Degrees_6Hours_Returns90()
        {
            var ra = RightAscension.FromHours(6.0);

            Assert.AreEqual(90.0, ra.Degrees, Tolerance);
        }

        [TestMethod]
        public void Degrees_0Hours_Returns0()
        {
            var ra = RightAscension.FromHours(0.0);

            Assert.AreEqual(0.0, ra.Degrees, Tolerance);
        }

        [TestMethod]
        public void Degrees_24Hours_WrapsToZero()
        {
            // 24h wraps to 0h in right ascension (normalized to [0, 24) range).
            var ra = RightAscension.FromHours(24.0);

            Assert.AreEqual(0.0, ra.Degrees, Tolerance);
        }

        // ────────────────────────────────────────────────────────────────
        //  TotalHours Property
        // ────────────────────────────────────────────────────────────────

        [TestMethod]
        public void TotalHours_FractionalValue_ReturnsCorrectly()
        {
            var ra = new RightAscension(14, 15, 30.0);

            var expected = 14 + (15.0 / 60) + (30.0 / 3600);
            Assert.AreEqual(expected, ra.TotalHours, Tolerance);
        }

        // ────────────────────────────────────────────────────────────────
        //  ToTimeSpan
        // ────────────────────────────────────────────────────────────────

        [TestMethod]
        public void ToTimeSpan_ReturnsEquivalentTimeSpan()
        {
            var ra = RightAscension.FromHours(6.5);
            var ts = ra.ToTimeSpan();

            Assert.AreEqual(6.5, ts.TotalHours, Tolerance);
        }

        // ────────────────────────────────────────────────────────────────
        //  ToString
        // ────────────────────────────────────────────────────────────────

        [TestMethod]
        public void ToString_FormattedCorrectly()
        {
            var ra = new RightAscension(12, 30, 45.1234);
            var result = ra.ToString();

            Assert.IsTrue(result.Contains("12"), $"Expected hours in result: {result}");
            Assert.IsTrue(result.Contains("30"), $"Expected minutes in result: {result}");
            Assert.IsTrue(result.Contains("45"), $"Expected seconds in result: {result}");
        }

        // ────────────────────────────────────────────────────────────────
        //  Round-trip: Constructor → Properties
        // ────────────────────────────────────────────────────────────────

        [TestMethod]
        [DataRow(0, 0, 0.0)]
        [DataRow(1, 0, 0.0)]
        [DataRow(12, 30, 0.0)]
        [DataRow(23, 59, 59.9999)]
        [DataRow(6, 45, 30.5)]
        public void RoundTrip_Constructor_PropertiesMatchInput(int hours, int minutes, double seconds)
        {
            var ra = new RightAscension(hours, minutes, seconds);

            Assert.AreEqual(hours, ra.Hours);
            Assert.AreEqual(minutes, ra.Minutes);
            Assert.AreEqual(seconds, ra.Seconds, Tolerance);
        }

        // ────────────────────────────────────────────────────────────────
        //  Round-trip: FromDegrees → Degrees
        // ────────────────────────────────────────────────────────────────

        [TestMethod]
        [DataRow(0.0)]
        [DataRow(45.0)]
        [DataRow(90.0)]
        [DataRow(180.0)]
        [DataRow(270.0)]
        [DataRow(359.9999)]
        public void RoundTrip_FromDegrees_DegreesMatchInput(double degrees)
        {
            var ra = RightAscension.FromDegrees(degrees);

            Assert.AreEqual(degrees, ra.Degrees, Tolerance);
        }
    }
}
