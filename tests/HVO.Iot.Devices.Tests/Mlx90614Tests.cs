using Microsoft.VisualStudio.TestTools.UnitTesting;
using FluentAssertions;
using HVO.Iot.Devices.Iot.Devices.Sensors.Mlx90614;

namespace HVO.Iot.Devices.Tests;

[TestClass]
public class Mlx90614Tests
{
    private static (Mlx90614 sensor, Mlx90614MemoryClient client) CreateSensor()
    {
        var client = new Mlx90614MemoryClient();
        var sensor = new Mlx90614(client);
        return (sensor, client);
    }

    #region Ambient Temperature

    [TestMethod]
    public void GetAmbientTemperature_DefaultValue_ReturnsApproximately25C()
    {
        var (sensor, _) = CreateSensor();
        var result = sensor.GetAmbientTemperature();

        result.IsSuccessful.Should().BeTrue();
        result.Value.Should().BeApproximately(25.0, 0.5);
    }

    [TestMethod]
    public void GetAmbientTemperature_SetTo0C_ReturnsApproximately0C()
    {
        var (sensor, client) = CreateSensor();
        client.SetAmbientTemperature(0.0);

        var result = sensor.GetAmbientTemperature();

        result.IsSuccessful.Should().BeTrue();
        result.Value.Should().BeApproximately(0.0, 0.5);
    }

    [TestMethod]
    public void GetAmbientTemperature_SetToNegative20C_ReturnsNegativeValue()
    {
        var (sensor, client) = CreateSensor();
        client.SetAmbientTemperature(-20.0);

        var result = sensor.GetAmbientTemperature();

        result.IsSuccessful.Should().BeTrue();
        result.Value.Should().BeApproximately(-20.0, 0.5);
    }

    [TestMethod]
    public void GetAmbientTemperature_SetTo100C_ReturnsApproximately100C()
    {
        var (sensor, client) = CreateSensor();
        client.SetAmbientTemperature(100.0);

        var result = sensor.GetAmbientTemperature();

        result.IsSuccessful.Should().BeTrue();
        result.Value.Should().BeApproximately(100.0, 0.5);
    }

    #endregion

    #region Object Temperature

    [TestMethod]
    public void GetObjectTemperature_DefaultValue_ReturnsApproximately25C()
    {
        var (sensor, _) = CreateSensor();
        var result = sensor.GetObjectTemperature();

        result.IsSuccessful.Should().BeTrue();
        result.Value.Should().BeApproximately(25.0, 0.5);
    }

    [TestMethod]
    public void GetObjectTemperature_SetTo37C_ReturnsApproximately37C()
    {
        var (sensor, client) = CreateSensor();
        client.SetObjectTemperature(37.0);

        var result = sensor.GetObjectTemperature();

        result.IsSuccessful.Should().BeTrue();
        result.Value.Should().BeApproximately(37.0, 0.5);
    }

    [TestMethod]
    public void GetObjectTemperature_SetToNegative40C_ReturnsNegativeValue()
    {
        var (sensor, client) = CreateSensor();
        client.SetObjectTemperature(-40.0);

        var result = sensor.GetObjectTemperature();

        result.IsSuccessful.Should().BeTrue();
        result.Value.Should().BeApproximately(-40.0, 0.5);
    }

    #endregion

    #region Raw IR Readings

    [TestMethod]
    public void GetRawIR1_SetValue_ReturnsCorrectValue()
    {
        var (sensor, client) = CreateSensor();
        client.SetRawIR1(12345);

        var result = sensor.GetRawIR1();

        result.IsSuccessful.Should().BeTrue();
        result.Value.Should().Be(12345);
    }

    [TestMethod]
    public void GetRawIR2_SetValue_ReturnsCorrectValue()
    {
        var (sensor, client) = CreateSensor();
        client.SetRawIR2(54321);

        var result = sensor.GetRawIR2();

        result.IsSuccessful.Should().BeTrue();
        result.Value.Should().Be(54321);
    }

    #endregion

    #region Multiple Readings

    [TestMethod]
    public void MultipleReadings_DifferentTemperatures_AllSuccessful()
    {
        var (sensor, client) = CreateSensor();

        var temps = new[] { -40.0, -10.0, 0.0, 25.0, 37.0, 100.0, 200.0 };

        foreach (var temp in temps)
        {
            client.SetObjectTemperature(temp);
            var result = sensor.GetObjectTemperature();
            result.IsSuccessful.Should().BeTrue();
            result.Value.Should().BeApproximately(temp, 0.5,
                $"Expected temperature ~{temp}°C but got {result.Value}°C");
        }
    }

    #endregion

    #region Interface Compliance

    [TestMethod]
    public void Mlx90614_ImplementsIMlx90614Interface()
    {
        var (sensor, _) = CreateSensor();
        sensor.Should().BeAssignableTo<IMlx90614>();
    }

    [TestMethod]
    public void Mlx90614_Dispose_DoesNotThrow()
    {
        var (sensor, _) = CreateSensor();
        var action = () => sensor.Dispose();
        action.Should().NotThrow();
    }

    [TestMethod]
    public void Mlx90614_DoubleDispose_DoesNotThrow()
    {
        var (sensor, _) = CreateSensor();
        sensor.Dispose();
        var action = () => sensor.Dispose();
        action.Should().NotThrow();
    }

    #endregion
}
