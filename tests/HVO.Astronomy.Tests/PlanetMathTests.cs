using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace HVO.Astronomy.Tests
{
    [TestClass]
    public class PlanetMathTests
    {
        // HVO coordinates
        private const double HvoLatitude = 35.18;
        private const double HvoLongitude = -113.81;

        // ────────────────────────────────────────────────────────────────
        //  ComputeTopocentricPositions
        // ────────────────────────────────────────────────────────────────

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ComputeTopocentricPositions_NullBodies_ThrowsArgumentNullException()
        {
            PlanetMath.ComputeTopocentricPositions(HvoLatitude, HvoLongitude,
                new DateTime(2024, 6, 15, 3, 0, 0, DateTimeKind.Utc), null!);
        }

        [TestMethod]
        public void ComputeTopocentricPositions_EmptyBodies_ReturnsEmpty()
        {
            var result = PlanetMath.ComputeTopocentricPositions(HvoLatitude, HvoLongitude,
                new DateTime(2024, 6, 15, 3, 0, 0, DateTimeKind.Utc), new PlanetBody[0]);

            Assert.AreEqual(0, result.Count);
        }

        [TestMethod]
        public void ComputeTopocentricPositions_AllPlanets_ReturnsCorrectCount()
        {
            var bodies = new[] { PlanetBody.Mercury, PlanetBody.Venus, PlanetBody.Mars, PlanetBody.Jupiter, PlanetBody.Saturn };
            var utc = new DateTime(2024, 6, 15, 3, 0, 0, DateTimeKind.Utc);

            var results = PlanetMath.ComputeTopocentricPositions(HvoLatitude, HvoLongitude, utc, bodies);

            Assert.AreEqual(5, results.Count);
        }

        // ────────────────────────────────────────────────────────────────
        //  Individual planet results
        // ────────────────────────────────────────────────────────────────

        [TestMethod]
        public void ComputeTopocentricPositions_Mars_ValidRaDec()
        {
            var utc = new DateTime(2024, 6, 15, 3, 0, 0, DateTimeKind.Utc);
            var results = PlanetMath.ComputeTopocentricPositions(HvoLatitude, HvoLongitude, utc, new[] { PlanetBody.Mars });
            var mars = results[0];

            Assert.AreEqual(PlanetBody.Mars, mars.Body);
            Assert.IsTrue(mars.RightAscensionHours >= 0.0 && mars.RightAscensionHours < 24.0,
                $"RA should be [0,24), was {mars.RightAscensionHours}");
            Assert.IsTrue(mars.DeclinationDegrees >= -90.0 && mars.DeclinationDegrees <= 90.0,
                $"Dec should be [-90,90], was {mars.DeclinationDegrees}");
            Assert.IsTrue(mars.DistanceAu > 0.3 && mars.DistanceAu < 3.0,
                $"Mars distance should be 0.3-3 AU, was {mars.DistanceAu}");
        }

        [TestMethod]
        public void ComputeTopocentricPositions_Jupiter_DistanceReasonable()
        {
            var utc = new DateTime(2024, 6, 15, 3, 0, 0, DateTimeKind.Utc);
            var results = PlanetMath.ComputeTopocentricPositions(HvoLatitude, HvoLongitude, utc, new[] { PlanetBody.Jupiter });
            var jupiter = results[0];

            Assert.IsTrue(jupiter.DistanceAu > 3.5 && jupiter.DistanceAu < 7.0,
                $"Jupiter distance should be 3.5-7 AU, was {jupiter.DistanceAu}");
        }

        [TestMethod]
        public void ComputeTopocentricPositions_Saturn_DistanceReasonable()
        {
            var utc = new DateTime(2024, 6, 15, 3, 0, 0, DateTimeKind.Utc);
            var results = PlanetMath.ComputeTopocentricPositions(HvoLatitude, HvoLongitude, utc, new[] { PlanetBody.Saturn });
            var saturn = results[0];

            Assert.IsTrue(saturn.DistanceAu > 7.0 && saturn.DistanceAu < 12.0,
                $"Saturn distance should be 7-12 AU, was {saturn.DistanceAu}");
        }

        // ────────────────────────────────────────────────────────────────
        //  Sun via PlanetMath
        // ────────────────────────────────────────────────────────────────

        [TestMethod]
        public void ComputeTopocentricPositions_Sun_DistanceNearOneAu()
        {
            var utc = new DateTime(2024, 6, 15, 3, 0, 0, DateTimeKind.Utc);
            var results = PlanetMath.ComputeTopocentricPositions(HvoLatitude, HvoLongitude, utc, new[] { PlanetBody.Sun });
            var sun = results[0];

            Assert.AreEqual(PlanetBody.Sun, sun.Body);
            Assert.IsTrue(sun.DistanceAu > 0.95 && sun.DistanceAu < 1.05,
                $"Sun distance should be ~1 AU, was {sun.DistanceAu}");
        }

        // ────────────────────────────────────────────────────────────────
        //  Moon via PlanetMath
        // ────────────────────────────────────────────────────────────────

        [TestMethod]
        public void ComputeTopocentricPositions_Moon_DistanceVerySmall()
        {
            var utc = new DateTime(2024, 6, 15, 3, 0, 0, DateTimeKind.Utc);
            var results = PlanetMath.ComputeTopocentricPositions(HvoLatitude, HvoLongitude, utc, new[] { PlanetBody.Moon });
            var moon = results[0];

            Assert.AreEqual(PlanetBody.Moon, moon.Body);
            // Moon distance is about 0.0025 AU
            Assert.IsTrue(moon.DistanceAu > 0.001 && moon.DistanceAu < 0.005,
                $"Moon distance should be ~0.0025 AU, was {moon.DistanceAu}");
        }

        // ────────────────────────────────────────────────────────────────
        //  Inner planets
        // ────────────────────────────────────────────────────────────────

        [TestMethod]
        public void ComputeTopocentricPositions_Mercury_DistanceLessThan2AU()
        {
            var utc = new DateTime(2024, 6, 15, 3, 0, 0, DateTimeKind.Utc);
            var results = PlanetMath.ComputeTopocentricPositions(HvoLatitude, HvoLongitude, utc, new[] { PlanetBody.Mercury });
            var mercury = results[0];

            Assert.IsTrue(mercury.DistanceAu > 0.4 && mercury.DistanceAu < 1.5,
                $"Mercury distance should be 0.4-1.5 AU, was {mercury.DistanceAu}");
        }

        [TestMethod]
        public void ComputeTopocentricPositions_Venus_DistanceReasonable()
        {
            var utc = new DateTime(2024, 6, 15, 3, 0, 0, DateTimeKind.Utc);
            var results = PlanetMath.ComputeTopocentricPositions(HvoLatitude, HvoLongitude, utc, new[] { PlanetBody.Venus });
            var venus = results[0];

            Assert.IsTrue(venus.DistanceAu > 0.2 && venus.DistanceAu < 1.8,
                $"Venus distance should be 0.2-1.8 AU, was {venus.DistanceAu}");
        }

        // ────────────────────────────────────────────────────────────────
        //  Outer planets
        // ────────────────────────────────────────────────────────────────

        [TestMethod]
        public void ComputeTopocentricPositions_Uranus_DistanceReasonable()
        {
            var utc = new DateTime(2024, 6, 15, 3, 0, 0, DateTimeKind.Utc);
            var results = PlanetMath.ComputeTopocentricPositions(HvoLatitude, HvoLongitude, utc, new[] { PlanetBody.Uranus });
            var uranus = results[0];

            Assert.IsTrue(uranus.DistanceAu > 17.0 && uranus.DistanceAu < 22.0,
                $"Uranus distance should be 17-22 AU, was {uranus.DistanceAu}");
        }

        [TestMethod]
        public void ComputeTopocentricPositions_Neptune_DistanceReasonable()
        {
            var utc = new DateTime(2024, 6, 15, 3, 0, 0, DateTimeKind.Utc);
            var results = PlanetMath.ComputeTopocentricPositions(HvoLatitude, HvoLongitude, utc, new[] { PlanetBody.Neptune });
            var neptune = results[0];

            Assert.IsTrue(neptune.DistanceAu > 28.0 && neptune.DistanceAu < 32.0,
                $"Neptune distance should be 28-32 AU, was {neptune.DistanceAu}");
        }

        // ────────────────────────────────────────────────────────────────
        //  PlanetPosition properties
        // ────────────────────────────────────────────────────────────────

        [TestMethod]
        public void PlanetPosition_Constructor_SetsAllProperties()
        {
            var pos = new PlanetPosition(PlanetBody.Mars, 12.5, 23.4, 1.5);

            Assert.AreEqual(PlanetBody.Mars, pos.Body);
            Assert.AreEqual(12.5, pos.RightAscensionHours);
            Assert.AreEqual(23.4, pos.DeclinationDegrees);
            Assert.AreEqual(1.5, pos.DistanceAu);
        }
    }
}
