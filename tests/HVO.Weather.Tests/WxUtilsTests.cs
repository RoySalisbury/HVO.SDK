using HVO.Weather;

namespace HVO.Weather.Tests;

[TestClass]
public class WxUtilsTests
{
    #region Temperature Indices

    [TestMethod]
    public void WindChill_BelowThreshold_ReturnsComputedValue()
    {
        var temp = Temperature.FromCelsius(-10.0);
        var result = WxUtils.WindChill(temp, 20.0);

        // Wind chill at -10°C with 20 km/h wind should be well below -10
        Assert.IsTrue(result.Celsius < -10.0, $"Expected wind chill below -10°C, got {result.Celsius}");
    }

    [TestMethod]
    public void WindChill_WarmTemperature_ReturnsOriginal()
    {
        var temp = Temperature.FromCelsius(15.0);
        var result = WxUtils.WindChill(temp, 20.0);

        Assert.AreEqual(15.0, result.Celsius, 0.001, "Wind chill above 10°C should return original temp");
    }

    [TestMethod]
    public void WindChill_CalmWind_ReturnsOriginal()
    {
        var temp = Temperature.FromCelsius(-5.0);
        var result = WxUtils.WindChill(temp, 3.0);

        Assert.AreEqual(-5.0, result.Celsius, 0.001, "Wind chill with calm wind should return original temp");
    }

    [TestMethod]
    public void HeatIndex_BelowThreshold_ReturnsOriginal()
    {
        var temp = Temperature.FromFahrenheit(70.0);
        var result = WxUtils.HeatIndex(temp, 50);

        Assert.AreEqual(70.0, result.Fahrenheit, 0.001, "Heat index below 80°F should return original temp");
    }

    [TestMethod]
    public void HeatIndex_HighTempAndHumidity_ReturnsHigherThanAir()
    {
        var temp = Temperature.FromFahrenheit(95.0);
        var result = WxUtils.HeatIndex(temp, 80);

        Assert.IsTrue(result.Fahrenheit > 95.0, $"Heat index at 95°F/80% humidity should exceed air temp, got {result.Fahrenheit}");
    }

    [TestMethod]
    public void Humidex_IntegerDivisionBugFix_ReturnsNonZeroContribution()
    {
        // BUG REGRESSION TEST: Legacy code had (5/9) which was integer division = 0.
        // The humidex contribution (vapor pressure component) should be non-zero.
        var temp = Temperature.FromCelsius(30.0);
        var result = WxUtils.Humidex(temp, 70);

        // With bug: humidex would equal temp (contribution = 0).
        // Fixed: humidex should be higher than temp due to humidity contribution.
        Assert.IsTrue(result.Celsius > 30.0,
            $"Humidex at 30°C/70% should exceed air temp. Got {result.Celsius}. If equal, integer division bug may still exist.");
    }

    #endregion

    #region Dew Point

    [TestMethod]
    public void DewPoint_FullSaturation_ReturnsTempApprox()
    {
        var temp = Temperature.FromCelsius(20.0);
        var result = WxUtils.DewPoint(temp, 100);

        // At 100% humidity, dew point ≈ air temperature
        Assert.AreEqual(20.0, result.Celsius, 1.0, "Dew point at 100% humidity should approximate air temp");
    }

    [TestMethod]
    public void DewPoint_LowHumidity_ReturnsLowerValue()
    {
        var temp = Temperature.FromCelsius(20.0);
        var result = WxUtils.DewPoint(temp, 30);

        Assert.IsTrue(result.Celsius < 20.0, "Dew point at 30% humidity should be below air temp");
    }

    [TestMethod]
    public void DewPoint_ZeroHumidity_ThrowsArgumentOutOfRangeException()
    {
        var temp = Temperature.FromCelsius(20.0);

        Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => WxUtils.DewPoint(temp, 0));
    }

    #endregion

    #region Vapor Pressure

    [TestMethod]
    public void SaturationVaporPressure_Boiling_AllAlgorithmsPositive()
    {
        var temp = Temperature.FromCelsius(100.0);

        foreach (WxUtils.VapAlgorithm alg in Enum.GetValues(typeof(WxUtils.VapAlgorithm)))
        {
            double svp = WxUtils.SaturationVaporPressure(temp, alg);
            Assert.IsTrue(svp > 0, $"SVP for algorithm {alg} at 100°C should be positive, got {svp}");
        }
    }

    [TestMethod]
    public void ActualVaporPressure_HalfSaturation_ReturnsHalfSVP()
    {
        var temp = Temperature.FromCelsius(20.0);
        double svp = WxUtils.SaturationVaporPressure(temp);
        double avp = WxUtils.ActualVaporPressure(temp, 50);

        Assert.AreEqual(svp / 2.0, avp, 0.01, "AVP at 50% humidity should be half SVP");
    }

    #endregion

    #region Pressure Reduction

    [TestMethod]
    public void StationToAltimeter_SeaLevel_ReturnsApproxSameValue()
    {
        double result = WxUtils.StationToAltimeter(1013.25, 0);

        // At sea level, altimeter ≈ station pressure
        Assert.AreEqual(1013.25, result, 1.0, "Altimeter at sea level should approximately equal station pressure");
    }

    [TestMethod]
    public void StationToAltimeter_HighElevation_ReturnsHigherValue()
    {
        // At 1500m elevation, station pressure is lower, but altimeter should be near SLP
        double result = WxUtils.StationToAltimeter(850.0, 1500);

        Assert.IsTrue(result > 850.0, "Altimeter at elevation should exceed station pressure");
        Assert.IsTrue(result > 990 && result < 1040, $"Altimeter should be near SLP, got {result}");
    }

    [TestMethod]
    public void StationToAltimeter_AllAlgorithms_ReturnReasonableValues()
    {
        foreach (WxUtils.AltimeterAlgorithm alg in Enum.GetValues(typeof(WxUtils.AltimeterAlgorithm)))
        {
            double result = WxUtils.StationToAltimeter(900.0, 1000, alg);

            Assert.IsTrue(result > 900 && result < 1100,
                $"Algorithm {alg}: altimeter at 900mb/1000m should be 900–1100, got {result}");
        }
    }

    [TestMethod]
    public void PressureReductionRatio_IntegerDivisionBugFix_DavisVP()
    {
        // BUG REGRESSION TEST: Legacy code had (9/5) integer division = 1 in DavisVP algorithm.
        // The humidity correction should use 1.8, not 1.
        var temp = Temperature.FromFahrenheit(75.0);
        double ratioWithHumidity = WxUtils.PressureReductionRatio(1013.25, 1000, temp, temp, 50, WxUtils.SLPAlgorithm.DavisVP);
        double ratioNoHumidity = WxUtils.PressureReductionRatio(1013.25, 1000, temp, temp, 0, WxUtils.SLPAlgorithm.DavisVP);

        // With correct 1.8 factor, humidity correction should make a noticeable difference.
        Assert.AreNotEqual(ratioNoHumidity, ratioWithHumidity, "Humidity correction with correct (9.0/5.0) should differ from zero-humidity");
    }

    [TestMethod]
    public void PressureReductionRatio_IntegerDivisionBugFix_ManBar()
    {
        // BUG REGRESSION TEST: Same (9/5) issue in ManBar algorithm.
        var temp = Temperature.FromFahrenheit(75.0);
        double ratioWithHumidity = WxUtils.PressureReductionRatio(1013.25, 1000, temp, temp, 50, WxUtils.SLPAlgorithm.ManBar);
        double ratioNoHumidity = WxUtils.PressureReductionRatio(1013.25, 1000, temp, temp, 0, WxUtils.SLPAlgorithm.ManBar);

        Assert.AreNotEqual(ratioNoHumidity, ratioWithHumidity, "Humidity correction with correct (9.0/5.0) should differ from zero-humidity");
    }

    #endregion

    #region Geopotential Altitude

    [TestMethod]
    public void GeopotentialAltitude_SeaLevel_ReturnsZero()
    {
        double result = WxUtils.GeopotentialAltitude(0);
        Assert.AreEqual(0, result, 0.001);
    }

    [TestMethod]
    public void GeopotentialAltitude_LowAltitude_ApproximatesGeometric()
    {
        double result = WxUtils.GeopotentialAltitude(100);
        // At low altitudes, geopotential ≈ geometric
        Assert.AreEqual(100, result, 0.1);
    }

    #endregion

    #region Unit Conversions

    [TestMethod]
    public void FtToM_OneFoot_Returns0Point3048()
    {
        Assert.AreEqual(0.3048, WxUtils.FtToM(1), 0.0001);
    }

    [TestMethod]
    public void MToFt_OneMeter_Returns3Point28084()
    {
        Assert.AreEqual(3.28084, WxUtils.MToFt(1), 0.001);
    }

    [TestMethod]
    public void InToMm_OneInch_Returns25Point4()
    {
        Assert.AreEqual(25.4, WxUtils.InToMm(1), 0.001);
    }

    [TestMethod]
    public void MmToIn_25Point4mm_ReturnsOneInch()
    {
        Assert.AreEqual(1.0, WxUtils.MmToIn(25.4), 0.001);
    }

    [TestMethod]
    public void MilesToKm_OneMile_Returns1Point609344()
    {
        Assert.AreEqual(1.609344, WxUtils.MilesToKm(1), 0.0001);
    }

    [TestMethod]
    public void KmToMiles_OneKm_Returns0Point62137()
    {
        Assert.AreEqual(0.62137, WxUtils.KmToMiles(1), 0.001);
    }

    #endregion
}
