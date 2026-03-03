using System;
using HVO.Weather;

namespace HVO.Weather.Tests;

[TestClass]
public class CwopAprsFormatterTests
{
    #region FormatPacket

    [TestMethod]
    public void FormatPacket_NullObservation_ThrowsArgumentNullException()
    {
        Assert.ThrowsException<ArgumentNullException>(() =>
            CwopAprsFormatter.FormatPacket(null!));
    }

    [TestMethod]
    public void FormatPacket_ValidObservation_ContainsStationIdHeader()
    {
        var observation = CreateDefaultObservation();
        var packet = CwopAprsFormatter.FormatPacket(observation);

        Assert.IsTrue(packet.StartsWith("DW4515>APRS,TCPXX*:@"),
            $"Packet should start with station header, got: {packet}");
    }

    [TestMethod]
    public void FormatPacket_ValidObservation_ContainsTimestamp()
    {
        var observation = CreateDefaultObservation();
        observation.ObservationTimeUtc = new DateTimeOffset(2025, 6, 15, 12, 30, 0, TimeSpan.Zero);

        var packet = CwopAprsFormatter.FormatPacket(observation);

        Assert.IsTrue(packet.Contains("151230z"),
            $"Packet should contain timestamp '151230z', got: {packet}");
    }

    [TestMethod]
    public void FormatPacket_ValidObservation_ContainsFormattedLatitude()
    {
        var observation = CreateDefaultObservation();
        // 35°33'36.18" N → 3533.60N
        var packet = CwopAprsFormatter.FormatPacket(observation);

        Assert.IsTrue(packet.Contains("3533.60N"),
            $"Packet should contain formatted latitude '3533.60N', got: {packet}");
    }

    [TestMethod]
    public void FormatPacket_ValidObservation_ContainsFormattedLongitude()
    {
        var observation = CreateDefaultObservation();
        // 113°54'34.14" W → 11354.57W
        var packet = CwopAprsFormatter.FormatPacket(observation);

        Assert.IsTrue(packet.Contains("11354.57W"),
            $"Packet should contain formatted longitude '11354.57W', got: {packet}");
    }

    [TestMethod]
    public void FormatPacket_ValidObservation_ContainsWindData()
    {
        var observation = CreateDefaultObservation();
        observation.AvgWindDirectionDegrees = 180;
        observation.AvgWindSpeedMph = 5;
        observation.GustWindSpeedMph = 12;

        var packet = CwopAprsFormatter.FormatPacket(observation);

        Assert.IsTrue(packet.Contains("_180/005g012"),
            $"Packet should contain wind data '_180/005g012', got: {packet}");
    }

    [TestMethod]
    public void FormatPacket_NullWindValues_UsesEllipsis()
    {
        var observation = CreateDefaultObservation();
        observation.AvgWindDirectionDegrees = null;
        observation.AvgWindSpeedMph = null;
        observation.GustWindSpeedMph = null;

        var packet = CwopAprsFormatter.FormatPacket(observation);

        Assert.IsTrue(packet.Contains("_.../...g..."),
            $"Packet should contain wind ellipsis '_.../...g...', got: {packet}");
    }

    [TestMethod]
    public void FormatPacket_WithSolarRadiation_IncludesLPrefix()
    {
        var observation = CreateDefaultObservation();
        observation.SolarRadiationWm2 = 450;

        var packet = CwopAprsFormatter.FormatPacket(observation);

        Assert.IsTrue(packet.Contains("L450"),
            $"Packet should contain solar radiation 'L450', got: {packet}");
    }

    [TestMethod]
    public void FormatPacket_WithHighSolarRadiation_IncludeslPrefix()
    {
        var observation = CreateDefaultObservation();
        observation.SolarRadiationWm2 = 1200;

        var packet = CwopAprsFormatter.FormatPacket(observation);

        Assert.IsTrue(packet.Contains("l200"),
            $"Packet should contain high solar 'l200', got: {packet}");
    }

    [TestMethod]
    public void FormatPacket_EndsWith_SoftwareType()
    {
        var observation = CreateDefaultObservation();
        observation.SoftwareType = "HVOs";

        var packet = CwopAprsFormatter.FormatPacket(observation);

        Assert.IsTrue(packet.EndsWith("HVOs"),
            $"Packet should end with software type 'HVOs', got: {packet}");
    }

    [TestMethod]
    public void FormatPacket_DefaultSoftwareType_UsesDVs()
    {
        var observation = CreateDefaultObservation();
        // SoftwareType defaults to CwopAprsFormatter.DefaultSoftwareType ("DVs")
        observation.SoftwareType = CwopAprsFormatter.DefaultSoftwareType;

        var packet = CwopAprsFormatter.FormatPacket(observation);

        Assert.IsTrue(packet.EndsWith("DVs"),
            $"Packet should end with default type 'DVs', got: {packet}");
    }

    [TestMethod]
    public void FormatPacket_100PercentHumidity_EncodesAs00()
    {
        var observation = CreateDefaultObservation();
        observation.OutsideHumidityPercent = 100;

        var packet = CwopAprsFormatter.FormatPacket(observation);

        Assert.IsTrue(packet.Contains("h00"),
            $"Packet should contain humidity '00' for 100%, got: {packet}");
    }

    #endregion

    #region FormatLogin

    [TestMethod]
    public void FormatLogin_ValidInputs_ReturnsLoginString()
    {
        var login = CwopAprsFormatter.FormatLogin("DW4515", -1, "1.0.0.0");

        Assert.AreEqual("user DW4515 -1 vers 1.0.0.0", login);
    }

    [TestMethod]
    public void FormatLogin_NullStationId_ThrowsArgumentNullException()
    {
        Assert.ThrowsException<ArgumentNullException>(() =>
            CwopAprsFormatter.FormatLogin(null!, -1, "1.0"));
    }

    [TestMethod]
    public void FormatLogin_EmptyStationId_ThrowsArgumentNullException()
    {
        Assert.ThrowsException<ArgumentNullException>(() =>
            CwopAprsFormatter.FormatLogin("", -1, "1.0"));
    }

    [TestMethod]
    public void FormatLogin_NullVersion_DefaultsTo1000()
    {
        var login = CwopAprsFormatter.FormatLogin("DW4515", -1, null!);

        Assert.AreEqual("user DW4515 -1 vers 1.0.0.0", login);
    }

    #endregion

    #region Internal Format Helpers

    [DataTestMethod]
    [DataRow(35, 33, 36.18, "N", "3533.60N")]
    [DataRow(0, 0, 0.0, "N", "0000.00N")]
    [DataRow(90, 0, 0.0, "S", "9000.00S")]
    [DataRow(45, 30, 30.0, "N", "4530.50N")]
    public void FormatLatitude_VariousInputs_ReturnsExpected(int deg, int min, double sec, string hem, string expected)
    {
        var result = CwopAprsFormatter.FormatLatitude(deg, min, sec, hem);
        Assert.AreEqual(expected, result);
    }

    [DataTestMethod]
    [DataRow(113, 54, 34.14, "W", "11354.57W")]
    [DataRow(0, 0, 0.0, "E", "00000.00E")]
    [DataRow(180, 0, 0.0, "W", "18000.00W")]
    [DataRow(5, 15, 45.0, "E", "00515.75E")]
    public void FormatLongitude_VariousInputs_ReturnsExpected(int deg, int min, double sec, string hem, string expected)
    {
        var result = CwopAprsFormatter.FormatLongitude(deg, min, sec, hem);
        Assert.AreEqual(expected, result);
    }

    [DataTestMethod]
    [DataRow(180.0, "180")]
    [DataRow(0.0, "000")]
    [DataRow(360.0, "360")]
    [DataRow(45.0, "045")]
    [DataRow(null, "...")]
    public void FormatWindDirection_VariousInputs_ReturnsExpected(double? degrees, string expected)
    {
        var result = CwopAprsFormatter.FormatWindDirection(degrees);
        Assert.AreEqual(expected, result);
    }

    [DataTestMethod]
    [DataRow(5.0, "005")]
    [DataRow(0.0, "000")]
    [DataRow(100.0, "100")]
    [DataRow(null, "...")]
    public void FormatWindSpeed_VariousInputs_ReturnsExpected(double? speed, string expected)
    {
        var result = CwopAprsFormatter.FormatWindSpeed(speed);
        Assert.AreEqual(expected, result);
    }

    [DataTestMethod]
    [DataRow(72.0, "072")]
    [DataRow(0.0, "000")]
    [DataRow(-5.0, "-05")]
    [DataRow(-15.0, "-15")]
    [DataRow(100.0, "100")]
    [DataRow(null, "...")]
    public void FormatTemperature_VariousInputs_ReturnsExpected(double? temp, string expected)
    {
        var result = CwopAprsFormatter.FormatTemperature(temp);
        Assert.AreEqual(expected, result);
    }

    [DataTestMethod]
    [DataRow(0.05, "005")]
    [DataRow(0.0, "000")]
    [DataRow(1.0, "100")]
    [DataRow(null, "...")]
    public void FormatRainHundredths_VariousInputs_ReturnsExpected(double? rain, string expected)
    {
        var result = CwopAprsFormatter.FormatRainHundredths(rain);
        Assert.AreEqual(expected, result);
    }

    [DataTestMethod]
    [DataRow(450.0, "L450")]
    [DataRow(0.0, "L000")]
    [DataRow(999.0, "L999")]
    [DataRow(1000.0, "l000")]
    [DataRow(1200.0, "l200")]
    [DataRow(null, "")]
    public void FormatSolarRadiation_VariousInputs_ReturnsExpected(double? radiation, string expected)
    {
        var result = CwopAprsFormatter.FormatSolarRadiation(radiation);
        Assert.AreEqual(expected, result);
    }

    [DataTestMethod]
    [DataRow(50.0, "50")]
    [DataRow(100.0, "00")]
    [DataRow(0.0, "01")]
    [DataRow(99.0, "99")]
    [DataRow(null, "..")]
    public void FormatHumidity_VariousInputs_ReturnsExpected(double? humidity, string expected)
    {
        var result = CwopAprsFormatter.FormatHumidity(humidity);
        Assert.AreEqual(expected, result);
    }

    #endregion

    #region ComputeAltimeterTenthsMillibars

    [TestMethod]
    public void ComputeAltimeterTenthsMillibars_StandardPressure_ReturnsReasonableValue()
    {
        // At standard sea level: 29.92 inHg ≈ 1013.2 mb
        var result = CwopAprsFormatter.ComputeAltimeterTenthsMillibars(29.92, 0.0, 59.0, 50.0);

        // At sea level, the altimeter should be close to 10132 (tenths of mb)
        Assert.IsTrue(result > 10000 && result < 10500,
            $"Expected result near 10132 tenths mb, got {result}");
    }

    [TestMethod]
    public void ComputeAltimeterTenthsMillibars_HighElevation_ReturnsReasonableValue()
    {
        // At higher elevation with lower station pressure, the altimeter correction should still
        // produce a value in a reasonable range (roughly 8000–11000 tenths of mb)
        var elevated = CwopAprsFormatter.ComputeAltimeterTenthsMillibars(27.0, 2962.0, 60.0, 50.0);

        Assert.IsTrue(elevated > 8000 && elevated < 11000,
            $"Expected elevated altimeter in range 8000-11000, got {elevated}");
    }

    [TestMethod]
    public void ComputeAltimeterTenthsMillibars_NullTemperature_DefaultsTo59F()
    {
        var withTemp = CwopAprsFormatter.ComputeAltimeterTenthsMillibars(29.92, 0.0, 59.0, 50.0);
        var withNull = CwopAprsFormatter.ComputeAltimeterTenthsMillibars(29.92, 0.0, null, 50.0);

        Assert.AreEqual(withTemp, withNull, 1.0,
            "Null temperature should default to 59°F");
    }

    [TestMethod]
    public void ComputeAltimeterTenthsMillibars_NullHumidity_DefaultsTo50()
    {
        var withHumidity = CwopAprsFormatter.ComputeAltimeterTenthsMillibars(29.92, 0.0, 59.0, 50.0);
        var withNull = CwopAprsFormatter.ComputeAltimeterTenthsMillibars(29.92, 0.0, 59.0, null);

        Assert.AreEqual(withHumidity, withNull, 1.0,
            "Null humidity should default to 50%");
    }

    #endregion

    #region Helpers

    private static CwopObservation CreateDefaultObservation()
    {
        return new CwopObservation
        {
            StationId = "DW4515",
            ObservationTimeUtc = new DateTimeOffset(2025, 6, 15, 12, 30, 0, TimeSpan.Zero),
            LatitudeDegrees = 35,
            LatitudeMinutes = 33,
            LatitudeSeconds = 36.18,
            LatitudeHemisphere = "N",
            LongitudeDegrees = 113,
            LongitudeMinutes = 54,
            LongitudeSeconds = 34.14,
            LongitudeHemisphere = "W",
            StationElevationFeet = 2962,
            BarometerInHg = 27.89,
            OutsideTemperatureF = 72,
            OutsideHumidityPercent = 35,
            AvgWindDirectionDegrees = 225,
            AvgWindSpeedMph = 8,
            GustWindSpeedMph = 15,
            HourlyRainInches = 0.0,
            DailyRainInches = 0.0,
            SolarRadiationWm2 = 850,
            SoftwareType = "DVs"
        };
    }

    #endregion
}
