using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace HVO.Astronomy.Tests
{
    [TestClass]
    public class MoonCalculationsTests
    {
        private const double HvoLatitude = 35.18;
        private const double HvoLongitude = -113.81;

        // ────────────────────────────────────────────────────────────────
        //  Moon phase — BUG FIX regression tests
        // ────────────────────────────────────────────────────────────────

        [TestMethod]
        public void CalculateMoonPhase_FixedCrescentSpelling()
        {
            // Verify the "Cresent" → "Crescent" fix. Try a range of dates to hit waxing crescent.
            // Moon age 1-6 = Waxing Crescent
            for (int day = 1; day <= 30; day++)
            {
                var date = new DateTime(2024, 1, day, 12, 0, 0, DateTimeKind.Utc);
                var phase = MoonCalculations.CalculateMoonPhase(date);
                Assert.IsFalse(phase.Contains("Cresent"), $"Phase on Jan {day} contains misspelling 'Cresent': {phase}");
            }
        }

        [TestMethod]
        public void CalculateMoonPhase_WaxingCrescent_SpelledCorrectly()
        {
            // Find a date with waxing crescent and assert correct spelling
            string? found = null;
            for (int day = 1; day <= 30; day++)
            {
                var date = new DateTime(2024, 1, day, 12, 0, 0, DateTimeKind.Utc);
                var phase = MoonCalculations.CalculateMoonPhase(date);
                if (phase.StartsWith("Waxing C", StringComparison.Ordinal))
                {
                    found = phase;
                    break;
                }
            }

            Assert.IsNotNull(found, "Should find a Waxing Crescent phase in January 2024");
            Assert.AreEqual("Waxing Crescent", found);
        }

        [TestMethod]
        public void CalculateMoonPhase_WaningCrescent_SpelledCorrectly()
        {
            string? found = null;
            for (int day = 1; day <= 30; day++)
            {
                var date = new DateTime(2024, 2, day, 12, 0, 0, DateTimeKind.Utc);
                var phase = MoonCalculations.CalculateMoonPhase(date);
                if (phase.StartsWith("Waning C", StringComparison.Ordinal))
                {
                    found = phase;
                    break;
                }
            }

            Assert.IsNotNull(found, "Should find a Waning Crescent phase in February 2024");
            Assert.AreEqual("Waning Crescent", found);
        }

        // ────────────────────────────────────────────────────────────────
        //  Moon age
        // ────────────────────────────────────────────────────────────────

        [TestMethod]
        public void CalculateMoonAge_ReturnsValueBetween0And29()
        {
            var date = new DateTime(2024, 6, 15, 12, 0, 0, DateTimeKind.Utc);
            var age = MoonCalculations.CalculateMoonAge(date);
            Assert.IsTrue(age >= 0 && age <= 29, $"Moon age should be [0,29], was {age}");
        }

        [TestMethod]
        public void CalculateMoonAge_KnownNewMoon_ReturnsNearZero()
        {
            // 2024-01-11 was approximately a new moon
            var date = new DateTime(2024, 1, 11, 12, 0, 0, DateTimeKind.Utc);
            var age = MoonCalculations.CalculateMoonAge(date);
            Assert.IsTrue(age <= 2 || age >= 28, $"New moon date should have age ~0, was {age}");
        }

        // ────────────────────────────────────────────────────────────────
        //  Moon phase names
        // ────────────────────────────────────────────────────────────────

        [TestMethod]
        public void CalculateMoonPhase_FullMoon_ReturnsCorrectName()
        {
            // 2024-01-25 was approximately a full moon
            var date = new DateTime(2024, 1, 25, 12, 0, 0, DateTimeKind.Utc);
            var phase = MoonCalculations.CalculateMoonPhase(date);
            Assert.IsTrue(phase == "Full Moon" || phase == "Waxing Gibbous" || phase == "Waning Gibbous",
                $"Near full moon date should return 'Full Moon' or adjacent phase, was '{phase}'");
        }

        [TestMethod]
        public void CalculateMoonPhase_AllPhasesAreKnownNames()
        {
            var knownPhases = new[]
            {
                "New Moon", "Waxing Crescent", "First Quarter", "Waxing Gibbous",
                "Full Moon", "Waning Gibbous", "Last Quarter", "Waning Crescent", ""
            };

            for (int day = 1; day <= 28; day++)
            {
                var date = new DateTime(2024, 3, day, 12, 0, 0, DateTimeKind.Utc);
                var phase = MoonCalculations.CalculateMoonPhase(date);
                CollectionAssert.Contains(knownPhases, phase, $"Unexpected phase name on March {day}: '{phase}'");
            }
        }

        // ────────────────────────────────────────────────────────────────
        //  Moon RA/Dec
        // ────────────────────────────────────────────────────────────────

        [TestMethod]
        public void CalculateMoonRightAscensionDeclination_ReturnsValidRanges()
        {
            var dayNumber = AstronomyMath.J2000_UT(new DateTime(2024, 6, 15, 12, 0, 0, DateTimeKind.Utc));
            double ra, dec;
            var distance = MoonCalculations.CalculateMoonRightAscensionDeclination(dayNumber, out ra, out dec);

            Assert.IsTrue(ra >= 0 && ra < 360, $"RA should be [0,360), was {ra}");
            Assert.IsTrue(dec >= -30 && dec <= 30, $"Dec should be [-30,30], was {dec}");
            Assert.IsTrue(distance > 50 && distance < 70, $"Moon distance should be ~60 Earth radii, was {distance}");
        }

        // ────────────────────────────────────────────────────────────────
        //  Moon Alt/Az
        // ────────────────────────────────────────────────────────────────

        [TestMethod]
        public void CalculateMoonAltitudeAzimuth_ReturnsFiniteValues()
        {
            var dayNumber = AstronomyMath.J2000_UT(new DateTime(2024, 6, 15, 3, 0, 0, DateTimeKind.Utc));
            double alt, az;
            var distance = MoonCalculations.CalculateMoonAltitudeAzimuth(dayNumber, HvoLatitude, HvoLongitude, out alt, out az);

            Assert.IsFalse(double.IsNaN(alt), "Altitude should not be NaN");
            Assert.IsFalse(double.IsNaN(az), "Azimuth should not be NaN");
            Assert.IsTrue(alt >= -90 && alt <= 90, $"Altitude should be [-90,90], was {alt}");
        }
    }
}
