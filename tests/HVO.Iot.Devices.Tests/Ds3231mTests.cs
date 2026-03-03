using Microsoft.VisualStudio.TestTools.UnitTesting;
using FluentAssertions;
using HVO.Iot.Devices.Iot.Devices.Sensors.Ds3231m;

namespace HVO.Iot.Devices.Tests;

[TestClass]
public class Ds3231mTests
{
    private static (Ds3231m rtc, Ds3231mMemoryClient client) CreateRtc()
    {
        var client = new Ds3231mMemoryClient();
        var rtc = new Ds3231m(client);
        return (rtc, client);
    }

    #region GetDateTime

    [TestMethod]
    public void GetDateTime_DefaultValue_Returns2025Jan1()
    {
        var (rtc, _) = CreateRtc();
        var result = rtc.GetDateTime();

        result.IsSuccessful.Should().BeTrue();
        result.Value.Year.Should().Be(2025);
        result.Value.Month.Should().Be(1);
        result.Value.Day.Should().Be(1);
        result.Value.Hour.Should().Be(0);
        result.Value.Minute.Should().Be(0);
        result.Value.Second.Should().Be(0);
    }

    [TestMethod]
    public void GetDateTime_SetToSpecificDate_ReturnsCorrectDate()
    {
        var (rtc, client) = CreateRtc();
        var expected = new DateTimeOffset(2025, 6, 15, 14, 30, 45, TimeSpan.Zero);
        client.SetDateTime(expected);

        var result = rtc.GetDateTime();

        result.IsSuccessful.Should().BeTrue();
        result.Value.Year.Should().Be(2025);
        result.Value.Month.Should().Be(6);
        result.Value.Day.Should().Be(15);
        result.Value.Hour.Should().Be(14);
        result.Value.Minute.Should().Be(30);
        result.Value.Second.Should().Be(45);
    }

    [TestMethod]
    public void GetDateTime_LeapYearDate_ReturnsCorrectDate()
    {
        var (rtc, client) = CreateRtc();
        var expected = new DateTimeOffset(2024, 2, 29, 12, 0, 0, TimeSpan.Zero);
        client.SetDateTime(expected);

        var result = rtc.GetDateTime();

        result.IsSuccessful.Should().BeTrue();
        result.Value.Month.Should().Be(2);
        result.Value.Day.Should().Be(29);
    }

    [TestMethod]
    public void GetDateTime_EndOfYear_ReturnsCorrectDate()
    {
        var (rtc, client) = CreateRtc();
        var expected = new DateTimeOffset(2025, 12, 31, 23, 59, 59, TimeSpan.Zero);
        client.SetDateTime(expected);

        var result = rtc.GetDateTime();

        result.IsSuccessful.Should().BeTrue();
        result.Value.Month.Should().Be(12);
        result.Value.Day.Should().Be(31);
        result.Value.Hour.Should().Be(23);
        result.Value.Minute.Should().Be(59);
        result.Value.Second.Should().Be(59);
    }

    #endregion

    #region SetDateTime

    [TestMethod]
    public void SetDateTime_ValidDate_SucceedsAndRoundTrips()
    {
        var (rtc, _) = CreateRtc();
        var dateTime = new DateTimeOffset(2025, 7, 4, 10, 20, 30, TimeSpan.Zero);

        var setResult = rtc.SetDateTime(dateTime);
        setResult.IsSuccessful.Should().BeTrue();

        var getResult = rtc.GetDateTime();
        getResult.IsSuccessful.Should().BeTrue();
        getResult.Value.Year.Should().Be(2025);
        getResult.Value.Month.Should().Be(7);
        getResult.Value.Day.Should().Be(4);
        getResult.Value.Hour.Should().Be(10);
        getResult.Value.Minute.Should().Be(20);
        getResult.Value.Second.Should().Be(30);
    }

    [TestMethod]
    public void SetDateTime_MultipleUpdates_EachRoundTripsCorrectly()
    {
        var (rtc, _) = CreateRtc();

        var dates = new[]
        {
            new DateTimeOffset(2000, 1, 1, 0, 0, 0, TimeSpan.Zero),
            new DateTimeOffset(2025, 6, 15, 12, 30, 0, TimeSpan.Zero),
            new DateTimeOffset(2099, 12, 31, 23, 59, 59, TimeSpan.Zero),
        };

        foreach (var dt in dates)
        {
            rtc.SetDateTime(dt).IsSuccessful.Should().BeTrue();
            var result = rtc.GetDateTime();
            result.IsSuccessful.Should().BeTrue();
            result.Value.Year.Should().Be(dt.Year);
            result.Value.Month.Should().Be(dt.Month);
            result.Value.Day.Should().Be(dt.Day);
        }
    }

    #endregion

    #region Temperature

    [TestMethod]
    public void GetTemperature_DefaultValue_ReturnsApproximately25C()
    {
        var (rtc, _) = CreateRtc();
        var result = rtc.GetTemperature();

        result.IsSuccessful.Should().BeTrue();
        result.Value.Should().BeApproximately(25.0, 0.25);
    }

    [TestMethod]
    public void GetTemperature_SetTo0C_ReturnsApproximately0C()
    {
        var (rtc, client) = CreateRtc();
        client.SetTemperature(0.0);

        var result = rtc.GetTemperature();

        result.IsSuccessful.Should().BeTrue();
        result.Value.Should().BeApproximately(0.0, 0.25);
    }

    [TestMethod]
    public void GetTemperature_SetToNegative10C_ReturnsNegativeValue()
    {
        var (rtc, client) = CreateRtc();
        client.SetTemperature(-10.0);

        var result = rtc.GetTemperature();

        result.IsSuccessful.Should().BeTrue();
        result.Value.Should().BeApproximately(-10.0, 0.25);
    }

    [TestMethod]
    [DataRow(-40.0)]
    [DataRow(-10.0)]
    [DataRow(0.0)]
    [DataRow(25.0)]
    [DataRow(50.0)]
    [DataRow(85.0)]
    public void GetTemperature_VariousValues_ReturnsApproximateValue(double expected)
    {
        var (rtc, client) = CreateRtc();
        client.SetTemperature(expected);

        var result = rtc.GetTemperature();

        result.IsSuccessful.Should().BeTrue();
        result.Value.Should().BeApproximately(expected, 0.25);
    }

    #endregion

    #region Oscillator Stop Flag

    [TestMethod]
    public void GetOscillatorStopFlag_Default_ReturnsFalse()
    {
        var (rtc, _) = CreateRtc();
        var result = rtc.GetOscillatorStopFlag();

        result.IsSuccessful.Should().BeTrue();
        result.Value.Should().BeFalse();
    }

    [TestMethod]
    public void GetOscillatorStopFlag_WhenSet_ReturnsTrue()
    {
        var (rtc, client) = CreateRtc();
        client.SetOscillatorStopFlag(true);

        var result = rtc.GetOscillatorStopFlag();

        result.IsSuccessful.Should().BeTrue();
        result.Value.Should().BeTrue();
    }

    [TestMethod]
    public void ClearOscillatorStopFlag_AfterSet_ClearsSuccessfully()
    {
        var (rtc, client) = CreateRtc();
        client.SetOscillatorStopFlag(true);

        rtc.GetOscillatorStopFlag().Value.Should().BeTrue();

        var clearResult = rtc.ClearOscillatorStopFlag();
        clearResult.IsSuccessful.Should().BeTrue();

        rtc.GetOscillatorStopFlag().Value.Should().BeFalse();
    }

    #endregion

    #region Interface Compliance

    [TestMethod]
    public void Ds3231m_ImplementsIDs3231mInterface()
    {
        var (rtc, _) = CreateRtc();
        rtc.Should().BeAssignableTo<IDs3231m>();
    }

    [TestMethod]
    public void Ds3231m_Dispose_DoesNotThrow()
    {
        var (rtc, _) = CreateRtc();
        var action = () => rtc.Dispose();
        action.Should().NotThrow();
    }

    [TestMethod]
    public void Ds3231m_DoubleDispose_DoesNotThrow()
    {
        var (rtc, _) = CreateRtc();
        rtc.Dispose();
        var action = () => rtc.Dispose();
        action.Should().NotThrow();
    }

    #endregion
}
