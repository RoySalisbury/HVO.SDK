using HVO.Weather;

namespace HVO.Weather.Tests;

[TestClass]
public class DistanceTests
{
    [TestMethod]
    public void FromMeters_OneKilometer_ConvertsCorrectly()
    {
        var d = Distance.FromMeters(1000.0);

        Assert.AreEqual(1000.0, d.Meters, 0.001);
        Assert.AreEqual(3280.84, d.Feet, 0.01);
        Assert.AreEqual(100000.0, d.Centimeters, 0.001);
    }

    [TestMethod]
    public void FromFeet_OneFoot_ConvertsCorrectly()
    {
        var d = Distance.FromFeet(1.0);

        Assert.AreEqual(0.3048, d.Meters, 0.0001);
        Assert.AreEqual(1.0, d.Feet, 0.001);
    }

    [TestMethod]
    public void FromCentimeters_OneMeter_ConvertsCorrectly()
    {
        // BUG REGRESSION TEST: Legacy had /10 instead of /100.
        var d = Distance.FromCentimeters(100.0);

        Assert.AreEqual(1.0, d.Meters, 0.001, "100 centimeters should equal 1 meter");
        Assert.AreEqual(3.28084, d.Feet, 0.001);
    }

    [TestMethod]
    public void FromCentimeters_TenCentimeters_ConvertsCorrectly()
    {
        var d = Distance.FromCentimeters(10.0);

        Assert.AreEqual(0.1, d.Meters, 0.001, "10 centimeters should equal 0.1 meters");
    }

    [TestMethod]
    public void FromKilometers_OneKilometer_ConvertsCorrectly()
    {
        var d = Distance.FromKilometers(1.0);

        Assert.AreEqual(1000.0, d.Meters, 0.001);
        Assert.AreEqual(3280.84, d.Feet, 0.01);
    }

    [TestMethod]
    public void FromMiles_OneMile_ConvertsCorrectly()
    {
        var d = Distance.FromMiles(1.0);

        Assert.AreEqual(5280.0, d.Feet, 0.01);
        Assert.AreEqual(1609.344, d.Meters, 0.01);
    }

    [TestMethod]
    public void RoundTrip_MetersToFeetAndBack()
    {
        var original = Distance.FromMeters(42.5);
        var roundTrip = Distance.FromFeet(original.Feet);

        Assert.AreEqual(42.5, roundTrip.Meters, 0.001);
    }
}
