using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace HVO.Astronomy.Tests
{
    [TestClass]
    public class PlanetBodyTests
    {
        [TestMethod]
        public void PlanetBody_ContainsAllExpectedValues()
        {
            Assert.AreEqual(0, (int)PlanetBody.Sun);
            Assert.AreEqual(1, (int)PlanetBody.Mercury);
            Assert.AreEqual(2, (int)PlanetBody.Venus);
            Assert.AreEqual(3, (int)PlanetBody.Mars);
            Assert.AreEqual(4, (int)PlanetBody.Jupiter);
            Assert.AreEqual(5, (int)PlanetBody.Saturn);
            Assert.AreEqual(6, (int)PlanetBody.Uranus);
            Assert.AreEqual(7, (int)PlanetBody.Neptune);
            Assert.AreEqual(8, (int)PlanetBody.Moon);
        }
    }
}
