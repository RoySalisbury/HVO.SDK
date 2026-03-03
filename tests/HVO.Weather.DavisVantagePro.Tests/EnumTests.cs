using System;

namespace HVO.Weather.DavisVantagePro.Tests;

[TestClass]
public class ForecastIconTests
{
    [TestMethod]
    [DataRow(ForecastIcon.Cloud, 2)]
    [DataRow(ForecastIcon.CloudRain, 3)]
    [DataRow(ForecastIcon.PartialSunCloud, 6)]
    [DataRow(ForecastIcon.PartialSunCloudRain, 7)]
    [DataRow(ForecastIcon.Sun, 8)]
    [DataRow(ForecastIcon.CloudSnow, 18)]
    [DataRow(ForecastIcon.CloudRainSnow, 19)]
    [DataRow(ForecastIcon.PartialSunCloudSnow, 22)]
    [DataRow(ForecastIcon.PartialSunCloudRainSnow, 23)]
    public void ForecastIcon_Values_MatchDavisProtocol(ForecastIcon icon, int expectedValue)
    {
        Assert.AreEqual(expectedValue, (int)icon);
    }
}

[TestClass]
public class BarometerTrendTests
{
    [TestMethod]
    [DataRow(BarometerTrend.Unknown, (short)80)]
    [DataRow(BarometerTrend.FallingRapidly, (short)196)]
    [DataRow(BarometerTrend.FallingSlowly, (short)236)]
    [DataRow(BarometerTrend.Steady, (short)0)]
    [DataRow(BarometerTrend.RisingSlowly, (short)20)]
    [DataRow(BarometerTrend.RisingRapidly, (short)60)]
    [DataRow(BarometerTrend.Unavailable, short.MaxValue)]
    public void BarometerTrend_Values_MatchDavisProtocol(BarometerTrend trend, short expectedValue)
    {
        Assert.AreEqual(expectedValue, (short)trend);
    }
}
