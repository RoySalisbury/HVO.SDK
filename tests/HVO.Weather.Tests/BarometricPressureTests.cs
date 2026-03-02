using HVO.Weather;

namespace HVO.Weather.Tests;

[TestClass]
public class BarometricPressureTests
{
    [TestMethod]
    public void FromInchesHg_StandardPressure_ConvertsCorrectly()
    {
        var bp = BarometricPressure.FromInchesHg(29.921);

        Assert.AreEqual(29.921, bp.InchesHg, 0.001);
        Assert.AreEqual(1013.25, bp.Millibars, 0.1);
        Assert.AreEqual(101325.0, bp.Pascals, 10);
    }

    [TestMethod]
    public void FromMillibars_StandardPressure_ConvertsCorrectly()
    {
        var bp = BarometricPressure.FromMillibars(1013.25);

        Assert.AreEqual(29.921, bp.InchesHg, 0.001);
        Assert.AreEqual(1013.25, bp.Millibars, 0.001);
    }

    [TestMethod]
    public void FromPascals_StandardPressure_ConvertsCorrectly()
    {
        var bp = BarometricPressure.FromPascals(101325.0);

        Assert.AreEqual(29.921, bp.InchesHg, 0.001);
        Assert.AreEqual(1013.25, bp.Millibars, 0.1);
        Assert.AreEqual(101325.0, bp.Pascals, 0.1);
    }

    [TestMethod]
    public void AltimeterFromAbsoluteMb_KingmanAZ_ReturnsReasonableValue()
    {
        // Kingman, AZ elevation ~1050m, typical absolute pressure ~900mb
        var altimeter = BarometricPressure.AltimeterFromAbsoluteMb(900.0, 1050);

        // Altimeter setting at that elevation should be close to standard SLP (~1013 mb)
        Assert.IsTrue(altimeter.Millibars > 990 && altimeter.Millibars < 1040,
            $"Expected altimeter near 1013, got {altimeter.Millibars}");
    }

    [TestMethod]
    public void FromInchesHg_Zero_ReturnsZeroes()
    {
        var bp = BarometricPressure.FromInchesHg(0.0);

        Assert.AreEqual(0, bp.InchesHg, 0.001);
        Assert.AreEqual(0, bp.Millibars, 0.001);
        Assert.AreEqual(0, bp.Pascals, 0.001);
    }
}
