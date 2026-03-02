using Microsoft.VisualStudio.TestTools.UnitTesting;
using FluentAssertions;
using HVO.Iot.Devices.Iot.Devices.Sensors.Tsl2591;

namespace HVO.Iot.Devices.Tests;

[TestClass]
public class Tsl2591Tests
{
    private static (Tsl2591 sensor, Tsl2591MemoryClient client) CreateSensor()
    {
        var client = new Tsl2591MemoryClient();
        // skipInitialization=true because the memory client has the device ID pre-loaded
        // and the initialization writes to command-addressed registers
        var sensor = new Tsl2591(client, skipInitialization: true);
        return (sensor, client);
    }

    #region Luminosity

    [TestMethod]
    public void GetLuminosity_DefaultValues_ReturnsSuccessfulResult()
    {
        var (sensor, _) = CreateSensor();
        var result = sensor.GetLuminosity();

        result.IsSuccessful.Should().BeTrue();
        result.Value.Gain.Should().Be(Tsl2591Gain.Low);
    }

    [TestMethod]
    public void GetLuminosity_DefaultValues_VisibleAndIRArePlausible()
    {
        var (sensor, _) = CreateSensor();
        var result = sensor.GetLuminosity();

        result.IsSuccessful.Should().BeTrue();
        // Default: fullSpectrum=500, ir=100 → visible = 500-100 = 400
        result.Value.Visible.Should().Be(400);
        result.Value.IR.Should().Be(100);
    }

    [TestMethod]
    public void GetLuminosity_DefaultValues_LuxIsPositive()
    {
        var (sensor, _) = CreateSensor();
        var result = sensor.GetLuminosity();

        result.IsSuccessful.Should().BeTrue();
        result.Value.Lux.Should().BeGreaterThan(0);
    }

    [TestMethod]
    public void GetLuminosity_WithMediumGain_ReportsCorrectGain()
    {
        var (sensor, _) = CreateSensor();
        var result = sensor.GetLuminosity(Tsl2591Gain.Medium, Tsl2591IntegrationTime.Ms200);

        result.IsSuccessful.Should().BeTrue();
        result.Value.Gain.Should().Be(Tsl2591Gain.Medium);
    }

    [TestMethod]
    public void GetLuminosity_ZeroLight_ReturnsZeroValues()
    {
        var (sensor, client) = CreateSensor();
        client.SetChannelData(fullSpectrum: 0, ir: 0);

        var result = sensor.GetLuminosity();

        result.IsSuccessful.Should().BeTrue();
        result.Value.Visible.Should().Be(0);
        result.Value.IR.Should().Be(0);
    }

    [TestMethod]
    public void GetLuminosity_HighLight_ReturnsLargeValues()
    {
        var (sensor, client) = CreateSensor();
        client.SetChannelData(fullSpectrum: 50000, ir: 10000);

        var result = sensor.GetLuminosity();

        result.IsSuccessful.Should().BeTrue();
        result.Value.Visible.Should().Be(40000);
        result.Value.IR.Should().Be(10000);
        result.Value.Lux.Should().BeGreaterThan(0);
    }

    #endregion

    #region Gain-Adjusted Luminosity

    [TestMethod]
    public void GetGainAdjustedLuminosity_DefaultValues_ReturnsSuccessfulResult()
    {
        var (sensor, _) = CreateSensor();
        var result = sensor.GetGainAdjustedLuminosity();

        result.IsSuccessful.Should().BeTrue();
    }

    [TestMethod]
    public void GetGainAdjustedLuminosity_DefaultValues_LuxIsPositive()
    {
        var (sensor, _) = CreateSensor();
        var result = sensor.GetGainAdjustedLuminosity();

        result.IsSuccessful.Should().BeTrue();
        result.Value.Lux.Should().BeGreaterThan(0);
    }

    [TestMethod]
    public void GetGainAdjustedLuminosity_ZeroLight_ReturnsMaxGain()
    {
        var (sensor, client) = CreateSensor();
        client.SetChannelData(fullSpectrum: 0, ir: 0);

        var result = sensor.GetGainAdjustedLuminosity();

        // When light is too low, gain should escalate to Max
        result.IsSuccessful.Should().BeTrue();
        result.Value.Gain.Should().Be(Tsl2591Gain.Max);
    }

    #endregion

    #region Lux Calculation Verification

    [TestMethod]
    public void GetLuminosity_KnownValues_LuxIsCalculatedCorrectly()
    {
        var (sensor, client) = CreateSensor();
        // Set known channel data: fullSpectrum=1000, ir=200 → visible=800
        client.SetChannelData(fullSpectrum: 1000, ir: 200);

        var result = sensor.GetLuminosity(Tsl2591Gain.Low, Tsl2591IntegrationTime.Ms100);

        result.IsSuccessful.Should().BeTrue();

        // Manual calculation:
        // countsPerLux = (100 * 1) / 408 = 0.245098...
        // lux1 = ((800 + 200) - (1.64 * 200)) / 0.245098 = (1000 - 328) / 0.245098 = 672 / 0.245... ≈ 2742.24
        // lux2 = ((0.59 * 1000) - (0.86 * 200)) / 0.245098 = (590 - 172) / 0.245098 = 418 / 0.245... ≈ 1705.3
        // max(2742, 1705) = 2742
        result.Value.Lux.Should().BeApproximately(2742.24, 1.0);
    }

    [TestMethod]
    public void GetLuminosity_DifferentIntegrationTimes_LuxScalesCorrectly()
    {
        var (sensor, client) = CreateSensor();
        client.SetChannelData(fullSpectrum: 1000, ir: 200);

        var result100 = sensor.GetLuminosity(Tsl2591Gain.Low, Tsl2591IntegrationTime.Ms100);
        var result200 = sensor.GetLuminosity(Tsl2591Gain.Low, Tsl2591IntegrationTime.Ms200);

        result100.IsSuccessful.Should().BeTrue();
        result200.IsSuccessful.Should().BeTrue();

        // With same raw values, longer integration time should produce LOWER lux
        // (because the sensor was integrating for longer, so the same raw count represents less light)
        result200.Value.Lux.Should().BeLessThan(result100.Value.Lux);
    }

    #endregion

    #region Interface Compliance

    [TestMethod]
    public void Tsl2591_ImplementsITsl2591Interface()
    {
        var (sensor, _) = CreateSensor();
        sensor.Should().BeAssignableTo<ITsl2591>();
    }

    [TestMethod]
    public void Tsl2591_Dispose_DoesNotThrow()
    {
        var (sensor, _) = CreateSensor();
        var action = () => sensor.Dispose();
        action.Should().NotThrow();
    }

    [TestMethod]
    public void Tsl2591_DoubleDispose_DoesNotThrow()
    {
        var (sensor, _) = CreateSensor();
        sensor.Dispose();
        var action = () => sensor.Dispose();
        action.Should().NotThrow();
    }

    #endregion

    #region Multiple Readings

    [TestMethod]
    public void MultipleReadings_DifferentChannelData_AllSuccessful()
    {
        var (sensor, client) = CreateSensor();
        var testCases = new (ushort full, ushort ir)[]
        {
            (100, 20),
            (500, 100),
            (1000, 200),
            (5000, 1000),
            (30000, 5000),
        };

        foreach (var (full, ir) in testCases)
        {
            client.SetChannelData(fullSpectrum: full, ir: ir);
            var result = sensor.GetLuminosity();
            result.IsSuccessful.Should().BeTrue(
                $"Reading with fullSpectrum={full}, ir={ir} should succeed");
            result.Value.Visible.Should().Be((ushort)(full - ir));
            result.Value.IR.Should().Be(ir);
        }
    }

    #endregion
}
