using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace HVO.Astronomy.Tests
{
    [TestClass]
    public class LongitudeTests
    {
        private const double Tolerance = 1e-4;

        // ────────────────────────────────────────────────────────────────
        //  Constructor
        // ────────────────────────────────────────────────────────────────

        [TestMethod]
        public void Constructor_WestHemisphere_SetsProperties()
        {
            var lon = new Longitude(113, 48, 36.0, LongitudeHemisphere.West);

            Assert.AreEqual(113, lon.Degrees);
            Assert.AreEqual(48, lon.Minutes);
            Assert.AreEqual(36.0, lon.Seconds, Tolerance);
            Assert.AreEqual(LongitudeHemisphere.West, lon.Hemisphere);
        }

        [TestMethod]
        public void Constructor_EastHemisphere_SetsProperties()
        {
            var lon = new Longitude(10, 30, 0.0, LongitudeHemisphere.East);

            Assert.AreEqual(10, lon.Degrees);
            Assert.AreEqual(LongitudeHemisphere.East, lon.Hemisphere);
        }

        // ────────────────────────────────────────────────────────────────
        //  Implicit conversion to double
        // ────────────────────────────────────────────────────────────────

        [TestMethod]
        public void ImplicitToDouble_EastLongitude_ReturnsPositive()
        {
            var lon = new Longitude(10, 30, 0.0, LongitudeHemisphere.East);
            double value = lon;

            Assert.IsTrue(value > 0, $"East longitude should be positive, was {value}");
        }

        [TestMethod]
        public void ImplicitToDouble_WestLongitude_ReturnsNegative()
        {
            var lon = new Longitude(113, 48, 36.0, LongitudeHemisphere.West);
            double value = lon;

            Assert.IsTrue(value < 0, $"West longitude should be negative, was {value}");
            Assert.AreEqual(-113.81, value, 0.01);
        }

        // ────────────────────────────────────────────────────────────────
        //  Implicit conversion from double — BUG FIX regression tests
        // ────────────────────────────────────────────────────────────────

        [TestMethod]
        public void ImplicitFromDouble_PositiveValue_EastHemisphere()
        {
            // BUG FIX REGRESSION TEST: Legacy code always set West direction
            // (CompassPoint.W : CompassPoint.W). Now correctly assigns East for positive.
            Longitude lon = 10.5;

            Assert.AreEqual(LongitudeHemisphere.East, lon.Hemisphere,
                "Positive longitude must be East (legacy bug always set West)");
            Assert.AreEqual(10, lon.Degrees);
        }

        [TestMethod]
        public void ImplicitFromDouble_NegativeValue_WestHemisphere()
        {
            Longitude lon = -113.81;

            Assert.AreEqual(LongitudeHemisphere.West, lon.Hemisphere);
            Assert.AreEqual(113, lon.Degrees);
        }

        [TestMethod]
        public void ImplicitFromDouble_ZeroDegrees_NoDivideByZero()
        {
            // BUG FIX REGRESSION TEST: Legacy code did (value % degree) which throws
            // DivideByZeroException when degree == 0 (e.g., 0.5° longitude).
            Longitude lon = 0.5;

            Assert.AreEqual(0, lon.Degrees);
            Assert.AreEqual(30, lon.Minutes);
            Assert.AreEqual(LongitudeHemisphere.East, lon.Hemisphere);
        }

        [TestMethod]
        public void ImplicitFromDouble_ExactlyZero_NoDivideByZero()
        {
            Longitude lon = 0.0;

            Assert.AreEqual(0, lon.Degrees);
            Assert.AreEqual(0, lon.Minutes);
            Assert.AreEqual(LongitudeHemisphere.East, lon.Hemisphere);
        }

        [TestMethod]
        public void ImplicitFromDouble_NegativeZeroDegrees_NoDivideByZero()
        {
            Longitude lon = -0.75;

            Assert.AreEqual(0, lon.Degrees);
            Assert.AreEqual(45, lon.Minutes);
            Assert.AreEqual(LongitudeHemisphere.West, lon.Hemisphere);
        }

        // ────────────────────────────────────────────────────────────────
        //  Round-trip
        // ────────────────────────────────────────────────────────────────

        [TestMethod]
        public void RoundTrip_PositiveDouble_PreservesValue()
        {
            double original = 10.5;
            Longitude lon = original;
            double result = lon;

            Assert.AreEqual(original, result, 0.001);
        }

        [TestMethod]
        public void RoundTrip_NegativeDouble_PreservesValue()
        {
            double original = -113.81;
            Longitude lon = original;
            double result = lon;

            Assert.AreEqual(original, result, 0.001);
        }

        // ────────────────────────────────────────────────────────────────
        //  ToString
        // ────────────────────────────────────────────────────────────────

        [TestMethod]
        public void ToString_WestLongitude_ContainsW()
        {
            var lon = new Longitude(113, 48, 36.0, LongitudeHemisphere.West);
            var str = lon.ToString();

            Assert.IsTrue(str.Contains("W"), $"ToString should contain 'W', was '{str}'");
            Assert.IsTrue(str.Contains("113"), $"ToString should contain degrees, was '{str}'");
        }

        [TestMethod]
        public void ToString_EastLongitude_ContainsE()
        {
            var lon = new Longitude(10, 30, 0.0, LongitudeHemisphere.East);
            var str = lon.ToString();

            Assert.IsTrue(str.Contains("E"), $"ToString should contain 'E', was '{str}'");
        }
    }
}
