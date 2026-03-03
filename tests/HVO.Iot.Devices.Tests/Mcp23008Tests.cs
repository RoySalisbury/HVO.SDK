using Microsoft.VisualStudio.TestTools.UnitTesting;
using FluentAssertions;
using HVO.Iot.Devices.Iot.Devices.Sensors.Mcp23xxx;

namespace HVO.Iot.Devices.Tests;

[TestClass]
public class Mcp23008Tests
{
    private static (Mcp23008 expander, Mcp23008MemoryClient client) CreateExpander()
    {
        var client = new Mcp23008MemoryClient();
        var expander = new Mcp23008(client);
        return (expander, client);
    }

    #region PinCount

    [TestMethod]
    public void PinCount_Returns8()
    {
        var (expander, _) = CreateExpander();
        expander.PinCount.Should().Be(8);
    }

    #endregion

    #region ReadPin

    [TestMethod]
    public void ReadPin_DefaultState_ReturnsFalse()
    {
        var (expander, _) = CreateExpander();
        var result = expander.ReadPin(0);

        result.IsSuccessful.Should().BeTrue();
        result.Value.Should().BeFalse();
    }

    [TestMethod]
    public void ReadPin_AfterSetHigh_ReturnsTrue()
    {
        var (expander, client) = CreateExpander();
        client.SetPinValue(3, true);

        var result = expander.ReadPin(3);

        result.IsSuccessful.Should().BeTrue();
        result.Value.Should().BeTrue();
    }

    [TestMethod]
    public void ReadPin_InvalidPin_ReturnsFailure()
    {
        var (expander, _) = CreateExpander();
        var result = expander.ReadPin(8);

        result.IsSuccessful.Should().BeFalse();
        result.Error.Should().BeOfType<ArgumentOutOfRangeException>();
    }

    [TestMethod]
    [DataRow((byte)0)]
    [DataRow((byte)3)]
    [DataRow((byte)7)]
    public void ReadPin_IndividualPinsSetHigh_CorrectPinRead(byte pin)
    {
        var (expander, client) = CreateExpander();
        client.SetPinValue(pin, true);

        var result = expander.ReadPin(pin);
        result.IsSuccessful.Should().BeTrue();
        result.Value.Should().BeTrue();
    }

    #endregion

    #region WritePin

    [TestMethod]
    public void WritePin_SetHigh_UpdatesGpioRegister()
    {
        var (expander, client) = CreateExpander();
        var result = expander.WritePin(5, true);

        result.IsSuccessful.Should().BeTrue();
        client.GetOlat().Should().Be(0x20); // bit 5 set
    }

    [TestMethod]
    public void WritePin_SetLow_ClearsBit()
    {
        var (expander, client) = CreateExpander();
        expander.WritePin(5, true);
        expander.WritePin(5, false);

        client.GetOlat().Should().Be(0x00);
    }

    [TestMethod]
    public void WritePin_MultiplePins_SetsCorrectBits()
    {
        var (expander, client) = CreateExpander();
        expander.WritePin(0, true);
        expander.WritePin(2, true);
        expander.WritePin(7, true);

        // Bit 0 + Bit 2 + Bit 7 = 0x01 + 0x04 + 0x80 = 0x85
        client.GetOlat().Should().Be(0x85);
    }

    [TestMethod]
    public void WritePin_InvalidPin_ReturnsFailure()
    {
        var (expander, _) = CreateExpander();
        var result = expander.WritePin(8, true);

        result.IsSuccessful.Should().BeFalse();
        result.Error.Should().BeOfType<ArgumentOutOfRangeException>();
    }

    #endregion

    #region SetPinDirection

    [TestMethod]
    public void SetPinDirection_DefaultAllInputs_IodirIs0xFF()
    {
        var (_, client) = CreateExpander();
        client.GetIodir().Should().Be(0xFF);
    }

    [TestMethod]
    public void SetPinDirection_SetOutput_ClearsBit()
    {
        var (expander, client) = CreateExpander();
        var result = expander.SetPinDirection(3, isInput: false);

        result.IsSuccessful.Should().BeTrue();
        // Bit 3 should be cleared: 0xFF & ~(1<<3) = 0xF7
        client.GetIodir().Should().Be(0xF7);
    }

    [TestMethod]
    public void SetPinDirection_SetBackToInput_RestoresBit()
    {
        var (expander, client) = CreateExpander();
        expander.SetPinDirection(3, isInput: false);
        expander.SetPinDirection(3, isInput: true);

        client.GetIodir().Should().Be(0xFF);
    }

    [TestMethod]
    public void SetPinDirection_InvalidPin_ReturnsFailure()
    {
        var (expander, _) = CreateExpander();
        var result = expander.SetPinDirection(8, isInput: true);

        result.IsSuccessful.Should().BeFalse();
    }

    #endregion

    #region SetPinPullup

    [TestMethod]
    public void SetPinPullup_Enable_SetsBit()
    {
        var (expander, client) = CreateExpander();
        var result = expander.SetPinPullup(4, enabled: true);

        result.IsSuccessful.Should().BeTrue();
        client.GetGppu().Should().Be(0x10); // bit 4 set
    }

    [TestMethod]
    public void SetPinPullup_Disable_ClearsBit()
    {
        var (expander, client) = CreateExpander();
        expander.SetPinPullup(4, enabled: true);
        expander.SetPinPullup(4, enabled: false);

        client.GetGppu().Should().Be(0x00);
    }

    [TestMethod]
    public void SetPinPullup_InvalidPin_ReturnsFailure()
    {
        var (expander, _) = CreateExpander();
        var result = expander.SetPinPullup(8, enabled: true);

        result.IsSuccessful.Should().BeFalse();
    }

    #endregion

    #region ReadAllPins

    [TestMethod]
    public void ReadAllPins_DefaultState_Returns0x00()
    {
        var (expander, _) = CreateExpander();
        var result = expander.ReadAllPins();

        result.IsSuccessful.Should().BeTrue();
        result.Value.Should().Be(0x00);
    }

    [TestMethod]
    public void ReadAllPins_AllSet_Returns0xFF()
    {
        var (expander, client) = CreateExpander();
        client.SetAllPins(0xFF);

        var result = expander.ReadAllPins();

        result.IsSuccessful.Should().BeTrue();
        result.Value.Should().Be(0xFF);
    }

    [TestMethod]
    public void ReadAllPins_SpecificPattern_ReturnsCorrectByte()
    {
        var (expander, client) = CreateExpander();
        client.SetAllPins(0xA5); // 10100101

        var result = expander.ReadAllPins();

        result.IsSuccessful.Should().BeTrue();
        result.Value.Should().Be(0xA5);
    }

    #endregion

    #region Bit Manipulation Helpers

    [TestMethod]
    public void SetBit_SetBit0_Returns0x01()
    {
        Mcp23008.SetBit(0x00, 0, true).Should().Be(0x01);
    }

    [TestMethod]
    public void SetBit_SetBit7_Returns0x80()
    {
        Mcp23008.SetBit(0x00, 7, true).Should().Be(0x80);
    }

    [TestMethod]
    public void SetBit_ClearBit0_Returns0xFE()
    {
        Mcp23008.SetBit(0xFF, 0, false).Should().Be(0xFE);
    }

    [TestMethod]
    public void SetBit_PreservesOtherBits()
    {
        Mcp23008.SetBit(0xA5, 1, true).Should().Be(0xA7);
    }

    [TestMethod]
    public void GetBit_Bit0Set_ReturnsTrue()
    {
        Mcp23008.GetBit(0x01, 0).Should().BeTrue();
    }

    [TestMethod]
    public void GetBit_Bit0NotSet_ReturnsFalse()
    {
        Mcp23008.GetBit(0xFE, 0).Should().BeFalse();
    }

    [TestMethod]
    public void GetBit_Bit7Set_ReturnsTrue()
    {
        Mcp23008.GetBit(0x80, 7).Should().BeTrue();
    }

    #endregion

    #region Interface Compliance

    [TestMethod]
    public void Mcp23008_ImplementsIMcp23008Interface()
    {
        var (expander, _) = CreateExpander();
        expander.Should().BeAssignableTo<IMcp23008>();
    }

    [TestMethod]
    public void Mcp23008_Dispose_DoesNotThrow()
    {
        var (expander, _) = CreateExpander();
        var action = () => expander.Dispose();
        action.Should().NotThrow();
    }

    [TestMethod]
    public void Mcp23008_DoubleDispose_DoesNotThrow()
    {
        var (expander, _) = CreateExpander();
        expander.Dispose();
        var action = () => expander.Dispose();
        action.Should().NotThrow();
    }

    #endregion
}
