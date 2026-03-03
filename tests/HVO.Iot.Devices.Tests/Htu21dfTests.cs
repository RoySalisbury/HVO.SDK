using Microsoft.VisualStudio.TestTools.UnitTesting;
using FluentAssertions;
using HVO.Iot.Devices.Iot.Devices.Sensors.Htu21df;

namespace HVO.Iot.Devices.Tests;

[TestClass]
public class Htu21dfTests
{
    private static (Htu21df sensor, Htu21dfMemoryClient client) CreateSensor()
    {
        var client = new Htu21dfMemoryClient();
        var sensor = new Htu21df(client);
        return (sensor, client);
    }

    #region Temperature

    [TestMethod]
    public void GetTemperature_DefaultValue_ReturnsApproximately25C()
    {
        var (sensor, _) = CreateSensor();
        var result = sensor.GetTemperature();

        result.IsSuccessful.Should().BeTrue();
        result.Value.Should().BeApproximately(25.0, 0.5);
    }

    [TestMethod]
    public void GetTemperature_SetTo0C_ReturnsApproximately0C()
    {
        var (sensor, client) = CreateSensor();
        client.SetTemperature(0.0);

        var result = sensor.GetTemperature();

        result.IsSuccessful.Should().BeTrue();
        result.Value.Should().BeApproximately(0.0, 0.5);
    }

    [TestMethod]
    public void GetTemperature_SetToNegative10C_ReturnsNegativeValue()
    {
        var (sensor, client) = CreateSensor();
        client.SetTemperature(-10.0);

        var result = sensor.GetTemperature();

        result.IsSuccessful.Should().BeTrue();
        result.Value.Should().BeApproximately(-10.0, 0.5);
    }

    [TestMethod]
    public void GetTemperature_SetTo85C_ReturnsApproximately85C()
    {
        var (sensor, client) = CreateSensor();
        client.SetTemperature(85.0);

        var result = sensor.GetTemperature();

        result.IsSuccessful.Should().BeTrue();
        result.Value.Should().BeApproximately(85.0, 0.5);
    }

    [TestMethod]
    [DataRow(-40.0)]
    [DataRow(-20.0)]
    [DataRow(0.0)]
    [DataRow(25.0)]
    [DataRow(50.0)]
    [DataRow(100.0)]
    [DataRow(125.0)]
    public void GetTemperature_VariousValues_ReturnsApproximateValue(double expected)
    {
        var (sensor, client) = CreateSensor();
        client.SetTemperature(expected);

        var result = sensor.GetTemperature();

        result.IsSuccessful.Should().BeTrue();
        result.Value.Should().BeApproximately(expected, 0.5);
    }

    #endregion

    #region Humidity

    [TestMethod]
    public void GetHumidity_DefaultValue_ReturnsApproximately50Percent()
    {
        var (sensor, _) = CreateSensor();
        var result = sensor.GetHumidity();

        result.IsSuccessful.Should().BeTrue();
        result.Value.Should().BeApproximately(50.0, 0.5);
    }

    [TestMethod]
    public void GetHumidity_SetTo0Percent_ReturnsApproximately0()
    {
        var (sensor, client) = CreateSensor();
        client.SetHumidity(0.0);

        var result = sensor.GetHumidity();

        result.IsSuccessful.Should().BeTrue();
        result.Value.Should().BeApproximately(0.0, 0.5);
    }

    [TestMethod]
    public void GetHumidity_SetTo100Percent_ReturnsApproximately100()
    {
        var (sensor, client) = CreateSensor();
        client.SetHumidity(100.0);

        var result = sensor.GetHumidity();

        result.IsSuccessful.Should().BeTrue();
        result.Value.Should().BeApproximately(100.0, 0.5);
    }

    [TestMethod]
    [DataRow(0.0)]
    [DataRow(20.0)]
    [DataRow(50.0)]
    [DataRow(75.0)]
    [DataRow(100.0)]
    public void GetHumidity_VariousValues_ReturnsApproximateValue(double expected)
    {
        var (sensor, client) = CreateSensor();
        client.SetHumidity(expected);

        var result = sensor.GetHumidity();

        result.IsSuccessful.Should().BeTrue();
        result.Value.Should().BeApproximately(expected, 0.5);
    }

    #endregion

    #region Interface Compliance

    [TestMethod]
    public void Htu21df_ImplementsIHtu21dfInterface()
    {
        var (sensor, _) = CreateSensor();
        sensor.Should().BeAssignableTo<IHtu21df>();
    }

    [TestMethod]
    public void Htu21df_Dispose_DoesNotThrow()
    {
        var (sensor, _) = CreateSensor();
        var action = () => sensor.Dispose();
        action.Should().NotThrow();
    }

    [TestMethod]
    public void Htu21df_DoubleDispose_DoesNotThrow()
    {
        var (sensor, _) = CreateSensor();
        sensor.Dispose();
        var action = () => sensor.Dispose();
        action.Should().NotThrow();
    }

    #endregion
}
