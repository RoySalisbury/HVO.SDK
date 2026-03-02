using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace HVO.Astronomy.Tests
{
    [TestClass]
    public class HemisphereTests
    {
        [TestMethod]
        public void LatitudeHemisphere_HasNorthAndSouth()
        {
            Assert.AreEqual(0, (int)LatitudeHemisphere.North);
            Assert.AreEqual(1, (int)LatitudeHemisphere.South);
        }

        [TestMethod]
        public void LongitudeHemisphere_HasEastAndWest()
        {
            Assert.AreEqual(0, (int)LongitudeHemisphere.East);
            Assert.AreEqual(1, (int)LongitudeHemisphere.West);
        }
    }
}
