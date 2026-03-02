using Microsoft.VisualStudio.TestTools.UnitTesting;
using FluentAssertions;
using HVO.Iot.Devices.Iot.Devices.Sensors.Si1145;

namespace HVO.Iot.Devices.Tests;

[TestClass]
public class Si1145Tests
{
    private static (Si1145 sensor, Si1145MemoryClient client) CreateSensor()
    {
        var client = new Si1145MemoryClient();
        // skipInitialization=true because the memory client already has the part ID loaded
        // and the initialization sequence writes to many registers the memory client doesn't need to emulate
        var sensor = new Si1145(client, skipInitialization: true);
        return (sensor, client);
    }

    #region Visible Light

    [TestMethod]
    public void ReadVisible_DefaultValue_Returns260()
    {
        var (sensor, _) = CreateSensor();
        var result = sensor.ReadVisible();

        result.IsSuccessful.Should().BeTrue();
        result.Value.Should().Be(260);
    }

    [TestMethod]
    public void ReadVisible_SetTo1000_ReturnsCorrectValue()
    {
        var (sensor, client) = CreateSensor();
        client.SetVisible(1000);

        var result = sensor.ReadVisible();

        result.IsSuccessful.Should().BeTrue();
        result.Value.Should().Be(1000);
    }

    [TestMethod]
    public void ReadVisible_SetToZero_ReturnsZero()
    {
        var (sensor, client) = CreateSensor();
        client.SetVisible(0);

        var result = sensor.ReadVisible();

        result.IsSuccessful.Should().BeTrue();
        result.Value.Should().Be(0);
    }

    [TestMethod]
    public void ReadVisible_SetToMaxValue_ReturnsCorrectValue()
    {
        var (sensor, client) = CreateSensor();
        client.SetVisible(ushort.MaxValue);

        var result = sensor.ReadVisible();

        result.IsSuccessful.Should().BeTrue();
        result.Value.Should().Be(ushort.MaxValue);
    }

    #endregion

    #region Infrared

    [TestMethod]
    public void ReadIR_DefaultValue_Returns250()
    {
        var (sensor, _) = CreateSensor();
        var result = sensor.ReadIR();

        result.IsSuccessful.Should().BeTrue();
        result.Value.Should().Be(250);
    }

    [TestMethod]
    public void ReadIR_SetTo5000_ReturnsCorrectValue()
    {
        var (sensor, client) = CreateSensor();
        client.SetIR(5000);

        var result = sensor.ReadIR();

        result.IsSuccessful.Should().BeTrue();
        result.Value.Should().Be(5000);
    }

    #endregion

    #region UV Index

    [TestMethod]
    public void ReadUV_DefaultValue_ReturnsApproximately0Point1()
    {
        var (sensor, _) = CreateSensor();
        var result = sensor.ReadUV();

        result.IsSuccessful.Should().BeTrue();
        result.Value.Should().BeApproximately(0.10f, 0.01f);
    }

    [TestMethod]
    public void ReadUV_SetTo11Point5_ReturnsCorrectValue()
    {
        var (sensor, client) = CreateSensor();
        client.SetUVIndex(11.5f);

        var result = sensor.ReadUV();

        result.IsSuccessful.Should().BeTrue();
        result.Value.Should().BeApproximately(11.5f, 0.02f);
    }

    [TestMethod]
    public void ReadUV_SetToZero_ReturnsZero()
    {
        var (sensor, client) = CreateSensor();
        client.SetUVIndex(0.0f);

        var result = sensor.ReadUV();

        result.IsSuccessful.Should().BeTrue();
        result.Value.Should().BeApproximately(0.0f, 0.01f);
    }

    #endregion

    #region Proximity

    [TestMethod]
    public void ReadProximity_DefaultValue_Returns240()
    {
        var (sensor, _) = CreateSensor();
        var result = sensor.ReadProximity();

        result.IsSuccessful.Should().BeTrue();
        result.Value.Should().Be(240);
    }

    [TestMethod]
    public void ReadProximity_SetTo10000_ReturnsCorrectValue()
    {
        var (sensor, client) = CreateSensor();
        client.SetProximity(10000);

        var result = sensor.ReadProximity();

        result.IsSuccessful.Should().BeTrue();
        result.Value.Should().Be(10000);
    }

    #endregion

    #region Interface Compliance

    [TestMethod]
    public void Si1145_ImplementsISi1145Interface()
    {
        var (sensor, _) = CreateSensor();
        sensor.Should().BeAssignableTo<ISi1145>();
    }

    [TestMethod]
    public void Si1145_Dispose_DoesNotThrow()
    {
        var (sensor, _) = CreateSensor();
        var action = () => sensor.Dispose();
        action.Should().NotThrow();
    }

    [TestMethod]
    public void Si1145_DoubleDispose_DoesNotThrow()
    {
        var (sensor, _) = CreateSensor();
        sensor.Dispose();
        var action = () => sensor.Dispose();
        action.Should().NotThrow();
    }

    #endregion

    #region Multiple Consecutive Reads

    [TestMethod]
    public void MultipleReads_AllChannels_AllSuccessful()
    {
        var (sensor, client) = CreateSensor();
        client.SetVisible(500);
        client.SetIR(300);
        client.SetUVIndex(5.0f);
        client.SetProximity(1000);

        sensor.ReadVisible().IsSuccessful.Should().BeTrue();
        sensor.ReadIR().IsSuccessful.Should().BeTrue();
        sensor.ReadUV().IsSuccessful.Should().BeTrue();
        sensor.ReadProximity().IsSuccessful.Should().BeTrue();

        sensor.ReadVisible().Value.Should().Be(500);
        sensor.ReadIR().Value.Should().Be(300);
        sensor.ReadUV().Value.Should().BeApproximately(5.0f, 0.02f);
        sensor.ReadProximity().Value.Should().Be(1000);
    }

    #endregion
}
