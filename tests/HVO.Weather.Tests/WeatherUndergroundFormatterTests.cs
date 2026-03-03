using System;
using HVO.Weather;

namespace HVO.Weather.Tests;

[TestClass]
public class WeatherUndergroundFormatterTests
{
    #region FormatUrl

    [TestMethod]
    public void FormatUrl_NullObservation_ThrowsArgumentNullException()
    {
        Assert.ThrowsExactly<ArgumentNullException>(() =>
            WeatherUndergroundFormatter.FormatUrl(null!));
    }

    [TestMethod]
    public void FormatUrl_ValidObservation_StartsWithBaseUrl()
    {
        var observation = CreateDefaultObservation();
        var url = WeatherUndergroundFormatter.FormatUrl(observation);

        Assert.IsTrue(url.StartsWith(WeatherUndergroundFormatter.BaseUrl + "?"),
            $"URL should start with base URL, got: {url}");
    }

    [TestMethod]
    public void FormatUrl_ValidObservation_ContainsStationId()
    {
        var observation = CreateDefaultObservation();
        var url = WeatherUndergroundFormatter.FormatUrl(observation);

        Assert.IsTrue(url.Contains("ID=KAZKINGM12"),
            $"URL should contain station ID, got: {url}");
    }

    [TestMethod]
    public void FormatUrl_ValidObservation_ContainsActionUpdateraw()
    {
        var observation = CreateDefaultObservation();
        var url = WeatherUndergroundFormatter.FormatUrl(observation);

        Assert.IsTrue(url.Contains("action=updateraw"),
            $"URL should contain action=updateraw, got: {url}");
    }

    #endregion

    #region FormatQueryString

    [TestMethod]
    public void FormatQueryString_NullObservation_ThrowsArgumentNullException()
    {
        Assert.ThrowsExactly<ArgumentNullException>(() =>
            WeatherUndergroundFormatter.FormatQueryString(null!));
    }

    [TestMethod]
    public void FormatQueryString_RequiredFieldsPresent()
    {
        var observation = CreateDefaultObservation();
        var qs = WeatherUndergroundFormatter.FormatQueryString(observation);

        Assert.IsTrue(qs.Contains("ID=KAZKINGM12"), "Missing station ID");
        Assert.IsTrue(qs.Contains("PASSWORD=testkey"), "Missing password");
        Assert.IsTrue(qs.Contains("action=updateraw"), "Missing action");
        Assert.IsTrue(qs.Contains("dateutc="), "Missing dateutc");
        Assert.IsTrue(qs.Contains("softwaretype=HVOs"), "Missing softwaretype");
    }

    [TestMethod]
    public void FormatQueryString_IncludesTemperature_WhenProvided()
    {
        var observation = CreateDefaultObservation();
        observation.OutdoorTemperatureF = 72.5;

        var qs = WeatherUndergroundFormatter.FormatQueryString(observation);

        Assert.IsTrue(qs.Contains("tempf=72.5"),
            $"Should contain tempf=72.5, got: {qs}");
    }

    [TestMethod]
    public void FormatQueryString_ExcludesTemperature_WhenNull()
    {
        var observation = CreateDefaultObservation();
        observation.OutdoorTemperatureF = null;

        var qs = WeatherUndergroundFormatter.FormatQueryString(observation);

        Assert.IsFalse(qs.Contains("tempf="),
            $"Should not contain tempf when null, got: {qs}");
    }

    [TestMethod]
    public void FormatQueryString_IncludesWindData_WhenProvided()
    {
        var observation = CreateDefaultObservation();
        observation.WindSpeedMph = 5.5;
        observation.WindDirectionDegrees = 180;
        observation.WindGustMph = 12.3;
        observation.WindGustDirectionDegrees = 190;

        var qs = WeatherUndergroundFormatter.FormatQueryString(observation);

        Assert.IsTrue(qs.Contains("windspeedmph=5.5"), $"Missing windspeedmph, got: {qs}");
        Assert.IsTrue(qs.Contains("winddir=180"), $"Missing winddir, got: {qs}");
        Assert.IsTrue(qs.Contains("windgustmph=12.3"), $"Missing windgustmph, got: {qs}");
        Assert.IsTrue(qs.Contains("windgustdir=190"), $"Missing windgustdir, got: {qs}");
    }

    [TestMethod]
    public void FormatQueryString_IncludesRainData_WhenProvided()
    {
        var observation = CreateDefaultObservation();
        observation.RainLastHourInches = 0.05;
        observation.DailyRainInches = 0.25;

        var qs = WeatherUndergroundFormatter.FormatQueryString(observation);

        Assert.IsTrue(qs.Contains("rainin=0.05"), $"Missing rainin, got: {qs}");
        Assert.IsTrue(qs.Contains("dailyrainin=0.25"), $"Missing dailyrainin, got: {qs}");
    }

    [TestMethod]
    public void FormatQueryString_IncludesBarometer_WhenProvided()
    {
        var observation = CreateDefaultObservation();
        observation.BarometerInHg = 29.92;

        var qs = WeatherUndergroundFormatter.FormatQueryString(observation);

        Assert.IsTrue(qs.Contains("baromin=29.92"), $"Missing baromin, got: {qs}");
    }

    [TestMethod]
    public void FormatQueryString_IncludesSolarRadiation_WhenProvided()
    {
        var observation = CreateDefaultObservation();
        observation.SolarRadiationWm2 = 850;

        var qs = WeatherUndergroundFormatter.FormatQueryString(observation);

        Assert.IsTrue(qs.Contains("solarradiation=850"),
            $"Missing solarradiation, got: {qs}");
    }

    [TestMethod]
    public void FormatQueryString_IncludesUvIndex_WhenProvided()
    {
        var observation = CreateDefaultObservation();
        observation.UvIndex = 6.5;

        var qs = WeatherUndergroundFormatter.FormatQueryString(observation);

        Assert.IsTrue(qs.Contains("UV=6.5"), $"Missing UV, got: {qs}");
    }

    [TestMethod]
    public void FormatQueryString_ExcludesOptionalFields_WhenNull()
    {
        var observation = CreateMinimalObservation();
        var qs = WeatherUndergroundFormatter.FormatQueryString(observation);

        Assert.IsFalse(qs.Contains("tempf="), "tempf should not appear when null");
        Assert.IsFalse(qs.Contains("windspeedmph="), "windspeedmph should not appear when null");
        Assert.IsFalse(qs.Contains("humidity="), "humidity should not appear when null");
        Assert.IsFalse(qs.Contains("baromin="), "baromin should not appear when null");
        Assert.IsFalse(qs.Contains("solarradiation="), "solarradiation should not appear when null");
        Assert.IsFalse(qs.Contains("UV="), "UV should not appear when null");
    }

    [TestMethod]
    public void FormatQueryString_IndoorValues_IncludedWhenProvided()
    {
        var observation = CreateDefaultObservation();
        observation.IndoorTemperatureF = 68.0;
        observation.IndoorHumidityPercent = 45.0;

        var qs = WeatherUndergroundFormatter.FormatQueryString(observation);

        Assert.IsTrue(qs.Contains("indoortempf=68"), $"Missing indoortempf, got: {qs}");
        Assert.IsTrue(qs.Contains("indoorhumidity=45"), $"Missing indoorhumidity, got: {qs}");
    }

    [TestMethod]
    public void FormatQueryString_DateUtc_FormattedCorrectly()
    {
        var observation = CreateDefaultObservation();
        observation.ObservationTimeUtc = new DateTimeOffset(2025, 6, 15, 14, 30, 45, TimeSpan.Zero);

        var qs = WeatherUndergroundFormatter.FormatQueryString(observation);

        Assert.IsTrue(qs.Contains("dateutc=2025-06-15%2014%3A30%3A45"),
            $"Date should be URL-encoded, got: {qs}");
    }

    [TestMethod]
    public void FormatQueryString_DefaultSoftwareType_UsesCustom()
    {
        var observation = CreateDefaultObservation();
        // SoftwareType defaults to "Custom"
        observation.SoftwareType = "Custom";

        var qs = WeatherUndergroundFormatter.FormatQueryString(observation);

        Assert.IsTrue(qs.Contains("softwaretype=Custom"),
            $"Default software type should be 'Custom', got: {qs}");
    }

    [TestMethod]
    public void FormatQueryString_2MinAverages_IncludedWhenProvided()
    {
        var observation = CreateDefaultObservation();
        observation.WindSpeedAvg2MinMph = 4.5;
        observation.WindDirectionAvg2MinDegrees = 200;

        var qs = WeatherUndergroundFormatter.FormatQueryString(observation);

        Assert.IsTrue(qs.Contains("windspdmph_avg2m=4.5"), $"Missing windspdmph_avg2m, got: {qs}");
        Assert.IsTrue(qs.Contains("winddir_avg2m=200"), $"Missing winddir_avg2m, got: {qs}");
    }

    [TestMethod]
    public void FormatQueryString_10MinGusts_IncludedWhenProvided()
    {
        var observation = CreateDefaultObservation();
        observation.WindGust10MinMph = 18.7;
        observation.WindGustDirection10MinDegrees = 210;

        var qs = WeatherUndergroundFormatter.FormatQueryString(observation);

        Assert.IsTrue(qs.Contains("windgustmph_10m=18.7"), $"Missing windgustmph_10m, got: {qs}");
        Assert.IsTrue(qs.Contains("windgustdir_10m=210"), $"Missing windgustdir_10m, got: {qs}");
    }

    [TestMethod]
    public void FormatQueryString_SoilAndLeaf_IncludedWhenProvided()
    {
        var observation = CreateDefaultObservation();
        observation.SoilTemperatureF = 65.0;
        observation.SoilMoisturePercent = 30.0;
        observation.LeafWetnessPercent = 10.0;

        var qs = WeatherUndergroundFormatter.FormatQueryString(observation);

        Assert.IsTrue(qs.Contains("soiltempf=65"), $"Missing soiltempf, got: {qs}");
        Assert.IsTrue(qs.Contains("soilmoisture=30"), $"Missing soilmoisture, got: {qs}");
        Assert.IsTrue(qs.Contains("leafwetness=10"), $"Missing leafwetness, got: {qs}");
    }

    #endregion

    #region Helpers

    private static WundergroundObservation CreateDefaultObservation()
    {
        return new WundergroundObservation
        {
            StationId = "KAZKINGM12",
            Password = "testkey",
            ObservationTimeUtc = new DateTimeOffset(2025, 6, 15, 12, 30, 0, TimeSpan.Zero),
            SoftwareType = "HVOs",
            OutdoorTemperatureF = 72.0,
            OutdoorHumidityPercent = 35,
            WindSpeedMph = 5.0,
            WindDirectionDegrees = 225,
            BarometerInHg = 27.89,
        };
    }

    private static WundergroundObservation CreateMinimalObservation()
    {
        return new WundergroundObservation
        {
            StationId = "KAZKINGM12",
            Password = "testkey",
            ObservationTimeUtc = new DateTimeOffset(2025, 6, 15, 12, 30, 0, TimeSpan.Zero),
            SoftwareType = "HVOs",
        };
    }

    #endregion
}
