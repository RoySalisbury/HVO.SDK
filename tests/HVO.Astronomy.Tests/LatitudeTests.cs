using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace HVO.Astronomy.Tests
{
    [TestClass]
    public class LatitudeTests
    {
        private const double Tolerance = 1e-4;

        // ────────────────────────────────────────────────────────────────
        //  Constructor
        // ────────────────────────────────────────────────────────────────

        [TestMethod]
        public void Constructor_NorthHemisphere_SetsProperties()
        {
            var lat = new Latitude(35, 10, 48.0, LatitudeHemisphere.North);

            Assert.AreEqual(35, lat.Degrees);
            Assert.AreEqual(10, lat.Minutes);
            Assert.AreEqual(48.0, lat.Seconds, Tolerance);
            Assert.AreEqual(LatitudeHemisphere.North, lat.Hemisphere);
        }

        [TestMethod]
        public void Constructor_SouthHemisphere_SetsProperties()
        {
            var lat = new Latitude(33, 51, 54.0, LatitudeHemisphere.South);

            Assert.AreEqual(33, lat.Degrees);
            Assert.AreEqual(LatitudeHemisphere.South, lat.Hemisphere);
        }

        // ────────────────────────────────────────────────────────────────
        //  Implicit conversion to double
        // ────────────────────────────────────────────────────────────────

        [TestMethod]
        public void ImplicitToDouble_NorthLatitude_ReturnsPositive()
        {
            var lat = new Latitude(35, 10, 48.0, LatitudeHemisphere.North);
            double value = lat;

            Assert.IsTrue(value > 0, $"North latitude should be positive, was {value}");
            Assert.AreEqual(35.18, value, 0.01);
        }

        [TestMethod]
        public void ImplicitToDouble_SouthLatitude_ReturnsNegative()
        {
            var lat = new Latitude(33, 0, 0.0, LatitudeHemisphere.South);
            double value = lat;

            Assert.IsTrue(value < 0, "South latitude should be negative");
        }

        // ────────────────────────────────────────────────────────────────
        //  Implicit conversion from double — including DivideByZero BUG FIX
        // ────────────────────────────────────────────────────────────────

        [TestMethod]
        public void ImplicitFromDouble_PositiveValue_NorthHemisphere()
        {
            Latitude lat = 35.18;

            Assert.AreEqual(LatitudeHemisphere.North, lat.Hemisphere);
            Assert.AreEqual(35, lat.Degrees);
        }

        [TestMethod]
        public void ImplicitFromDouble_NegativeValue_SouthHemisphere()
        {
            Latitude lat = -33.86;

            Assert.AreEqual(LatitudeHemisphere.South, lat.Hemisphere);
            Assert.AreEqual(33, lat.Degrees);
        }

        [TestMethod]
        public void ImplicitFromDouble_ZeroDegrees_NoDivideByZero()
        {
            // BUG FIX REGRESSION TEST: Legacy code did (value % degree) which throws
            // DivideByZeroException when degree == 0 (e.g., 0.5° latitude).
            Latitude lat = 0.5;

            Assert.AreEqual(0, lat.Degrees);
            Assert.AreEqual(30, lat.Minutes);
            Assert.AreEqual(LatitudeHemisphere.North, lat.Hemisphere);
        }

        [TestMethod]
        public void ImplicitFromDouble_ExactlyZero_NoDivideByZero()
        {
            Latitude lat = 0.0;

            Assert.AreEqual(0, lat.Degrees);
            Assert.AreEqual(0, lat.Minutes);
            Assert.AreEqual(LatitudeHemisphere.North, lat.Hemisphere);
        }

        // ────────────────────────────────────────────────────────────────
        //  Round-trip
        // ────────────────────────────────────────────────────────────────

        [TestMethod]
        public void RoundTrip_DoubleToLatitudeToDouble_PreservesValue()
        {
            double original = 35.18;
            Latitude lat = original;
            double result = lat;

            Assert.AreEqual(original, result, 0.001);
        }

        [TestMethod]
        public void RoundTrip_NegativeDouble_PreservesValue()
        {
            double original = -27.4678;
            Latitude lat = original;
            double result = lat;

            Assert.AreEqual(original, result, 0.001);
        }

        // ────────────────────────────────────────────────────────────────
        //  ToString
        // ────────────────────────────────────────────────────────────────

        [TestMethod]
        public void ToString_NorthLatitude_ContainsN()
        {
            var lat = new Latitude(35, 10, 48.0, LatitudeHemisphere.North);
            var str = lat.ToString();

            Assert.IsTrue(str.Contains("N"), $"ToString should contain 'N', was '{str}'");
            Assert.IsTrue(str.Contains("35"), $"ToString should contain degrees, was '{str}'");
        }
    }
}
