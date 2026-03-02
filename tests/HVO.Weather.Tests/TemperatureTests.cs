using HVO.Weather;

namespace HVO.Weather.Tests;

[TestClass]
public class TemperatureTests
{
    [TestMethod]
    public void FromFahrenheit_BoilingPoint_ConvertsCorrectly()
    {
        var temp = Temperature.FromFahrenheit(212.0);

        Assert.AreEqual(212.0, temp.Fahrenheit, 0.001);
        Assert.AreEqual(100.0, temp.Celsius, 0.001);
        Assert.AreEqual(373.15, temp.Kelvin, 0.001);
        Assert.AreEqual(671.67, temp.Rankine, 0.01);
    }

    [TestMethod]
    public void FromFahrenheit_FreezingPoint_ConvertsCorrectly()
    {
        var temp = Temperature.FromFahrenheit(32.0);

        Assert.AreEqual(32.0, temp.Fahrenheit, 0.001);
        Assert.AreEqual(0.0, temp.Celsius, 0.001);
        Assert.AreEqual(273.15, temp.Kelvin, 0.001);
        Assert.AreEqual(491.67, temp.Rankine, 0.01);
    }

    [TestMethod]
    public void FromCelsius_BodyTemperature_ConvertsCorrectly()
    {
        var temp = Temperature.FromCelsius(37.0);

        Assert.AreEqual(98.6, temp.Fahrenheit, 0.001);
        Assert.AreEqual(37.0, temp.Celsius, 0.001);
        Assert.AreEqual(310.15, temp.Kelvin, 0.001);
    }

    [TestMethod]
    public void FromKelvin_AbsoluteZero_ConvertsCorrectly()
    {
        var temp = Temperature.FromKelvin(0.0);

        Assert.AreEqual(-273.15, temp.Celsius, 0.001);
        Assert.AreEqual(-459.67, temp.Fahrenheit, 0.001);
        Assert.AreEqual(0.0, temp.Rankine, 0.001);
    }

    [TestMethod]
    public void FromRankine_StandardTemperature_ConvertsCorrectly()
    {
        var temp = Temperature.FromRankine(491.67);

        Assert.AreEqual(32.0, temp.Fahrenheit, 0.01);
        Assert.AreEqual(0.0, temp.Celsius, 0.01);
        Assert.AreEqual(273.15, temp.Kelvin, 0.01);
    }

    [TestMethod]
    public void FromFahrenheitDecimal_ReturnsCorrectValues()
    {
        var temp = Temperature.FromFahrenheit(72m);

        Assert.AreEqual(72.0, temp.Fahrenheit, 0.001);
        Assert.AreEqual(22.222, temp.Celsius, 0.01);
    }

    [TestMethod]
    public void FromCelsiusDecimal_ReturnsCorrectValues()
    {
        var temp = Temperature.FromCelsius(100m);

        Assert.AreEqual(212.0, temp.Fahrenheit, 0.001);
    }

    [TestMethod]
    public void NullTemperature_IsNull()
    {
        Temperature? temp = null;
        Assert.IsNull(temp);
    }
}
