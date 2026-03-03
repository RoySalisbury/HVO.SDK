using Microsoft.VisualStudio.TestTools.UnitTesting;
using FluentAssertions;
using HVO.Iot.Devices.Iot.Devices.Sensors.Mcp23xxx;

namespace HVO.Iot.Devices.Tests;

[TestClass]
public class Mcp23017Tests
{
    private static (Mcp23017 expander, Mcp23017MemoryClient client) CreateExpander()
    {
        var client = new Mcp23017MemoryClient();
        var expander = new Mcp23017(client);
        return (expander, client);
    }

    #region PinCount

    [TestMethod]
    public void PinCount_Returns16()
    {
        var (expander, _) = CreateExpander();
        expander.PinCount.Should().Be(16);
    }

    #endregion

    #region ReadPin — Port A (pins 0–7)

    [TestMethod]
    public void ReadPin_PortA_DefaultState_ReturnsFalse()
    {
        var (expander, _) = CreateExpander();
        var result = expander.ReadPin(0);

        result.IsSuccessful.Should().BeTrue();
        result.Value.Should().BeFalse();
    }

    [TestMethod]
    public void ReadPin_PortA_AfterSetHigh_ReturnsTrue()
    {
        var (expander, client) = CreateExpander();
        client.SetPinValue(3, true);

        var result = expander.ReadPin(3);

        result.IsSuccessful.Should().BeTrue();
        result.Value.Should().BeTrue();
    }

    #endregion

    #region ReadPin — Port B (pins 8–15)

    [TestMethod]
    public void ReadPin_PortB_DefaultState_ReturnsFalse()
    {
        var (expander, _) = CreateExpander();
        var result = expander.ReadPin(8);

        result.IsSuccessful.Should().BeTrue();
        result.Value.Should().BeFalse();
    }

    [TestMethod]
    public void ReadPin_PortB_AfterSetHigh_ReturnsTrue()
    {
        var (expander, client) = CreateExpander();
        client.SetPinValue(10, true); // port B, local pin 2

        var result = expander.ReadPin(10);

        result.IsSuccessful.Should().BeTrue();
        result.Value.Should().BeTrue();
    }

    [TestMethod]
    public void ReadPin_PortB_Pin15_AfterSetHigh_ReturnsTrue()
    {
        var (expander, client) = CreateExpander();
        client.SetPinValue(15, true); // port B, local pin 7

        var result = expander.ReadPin(15);

        result.IsSuccessful.Should().BeTrue();
        result.Value.Should().BeTrue();
    }

    #endregion

    #region ReadPin — Boundary and Error

    [TestMethod]
    public void ReadPin_InvalidPin16_ReturnsFailure()
    {
        var (expander, _) = CreateExpander();
        var result = expander.ReadPin(16);

        result.IsSuccessful.Should().BeFalse();
        result.Error.Should().BeOfType<ArgumentOutOfRangeException>();
    }

    [TestMethod]
    public void ReadPin_PortA_DoesNotAffectPortB()
    {
        var (expander, client) = CreateExpander();
        client.SetPortA(0xFF);

        // Port B should still be 0
        for (byte pin = 8; pin < 16; pin++)
        {
            expander.ReadPin(pin).Value.Should().BeFalse($"Port B pin {pin} should be low");
        }
    }

    #endregion

    #region WritePin

    [TestMethod]
    public void WritePin_PortA_SetHigh_UpdatesOlatA()
    {
        var (expander, client) = CreateExpander();
        var result = expander.WritePin(5, true);

        result.IsSuccessful.Should().BeTrue();
        client.GetOlatA().Should().Be(0x20); // bit 5
    }

    [TestMethod]
    public void WritePin_PortB_SetHigh_UpdatesOlatB()
    {
        var (expander, client) = CreateExpander();
        var result = expander.WritePin(13, true); // port B, local pin 5

        result.IsSuccessful.Should().BeTrue();
        client.GetOlatB().Should().Be(0x20); // bit 5 of port B
    }

    [TestMethod]
    public void WritePin_PortA_DoesNotAffectPortB()
    {
        var (expander, client) = CreateExpander();
        expander.WritePin(0, true);
        expander.WritePin(7, true);

        client.GetOlatA().Should().Be(0x81);
        client.GetOlatB().Should().Be(0x00);
    }

    [TestMethod]
    public void WritePin_InvalidPin_ReturnsFailure()
    {
        var (expander, _) = CreateExpander();
        var result = expander.WritePin(16, true);

        result.IsSuccessful.Should().BeFalse();
    }

    #endregion

    #region SetPinDirection

    [TestMethod]
    public void SetPinDirection_DefaultAllInputs()
    {
        var (_, client) = CreateExpander();
        client.GetIodirA().Should().Be(0xFF);
        client.GetIodirB().Should().Be(0xFF);
    }

    [TestMethod]
    public void SetPinDirection_PortA_SetOutput_ClearsBit()
    {
        var (expander, client) = CreateExpander();
        var result = expander.SetPinDirection(3, isInput: false);

        result.IsSuccessful.Should().BeTrue();
        client.GetIodirA().Should().Be(0xF7); // bit 3 cleared
        client.GetIodirB().Should().Be(0xFF); // unchanged
    }

    [TestMethod]
    public void SetPinDirection_PortB_SetOutput_ClearsBit()
    {
        var (expander, client) = CreateExpander();
        var result = expander.SetPinDirection(10, isInput: false); // port B, local pin 2

        result.IsSuccessful.Should().BeTrue();
        client.GetIodirA().Should().Be(0xFF); // unchanged
        client.GetIodirB().Should().Be(0xFB); // bit 2 cleared
    }

    [TestMethod]
    public void SetPinDirection_InvalidPin_ReturnsFailure()
    {
        var (expander, _) = CreateExpander();
        var result = expander.SetPinDirection(16, isInput: true);

        result.IsSuccessful.Should().BeFalse();
    }

    #endregion

    #region SetPinPullup

    [TestMethod]
    public void SetPinPullup_PortA_Enable_SetsBit()
    {
        var (expander, client) = CreateExpander();
        var result = expander.SetPinPullup(4, enabled: true);

        result.IsSuccessful.Should().BeTrue();
        client.GetGppuA().Should().Be(0x10); // bit 4
        client.GetGppuB().Should().Be(0x00); // unchanged
    }

    [TestMethod]
    public void SetPinPullup_PortB_Enable_SetsBit()
    {
        var (expander, client) = CreateExpander();
        var result = expander.SetPinPullup(12, enabled: true); // port B, local pin 4

        result.IsSuccessful.Should().BeTrue();
        client.GetGppuA().Should().Be(0x00); // unchanged
        client.GetGppuB().Should().Be(0x10); // bit 4
    }

    [TestMethod]
    public void SetPinPullup_InvalidPin_ReturnsFailure()
    {
        var (expander, _) = CreateExpander();
        var result = expander.SetPinPullup(16, enabled: true);

        result.IsSuccessful.Should().BeFalse();
    }

    #endregion

    #region ReadPortA / ReadPortB

    [TestMethod]
    public void ReadPortA_DefaultState_Returns0x00()
    {
        var (expander, _) = CreateExpander();
        var result = expander.ReadPortA();

        result.IsSuccessful.Should().BeTrue();
        result.Value.Should().Be(0x00);
    }

    [TestMethod]
    public void ReadPortA_AllSet_Returns0xFF()
    {
        var (expander, client) = CreateExpander();
        client.SetPortA(0xFF);

        var result = expander.ReadPortA();

        result.IsSuccessful.Should().BeTrue();
        result.Value.Should().Be(0xFF);
    }

    [TestMethod]
    public void ReadPortB_DefaultState_Returns0x00()
    {
        var (expander, _) = CreateExpander();
        var result = expander.ReadPortB();

        result.IsSuccessful.Should().BeTrue();
        result.Value.Should().Be(0x00);
    }

    [TestMethod]
    public void ReadPortB_SpecificPattern_ReturnsCorrectByte()
    {
        var (expander, client) = CreateExpander();
        client.SetPortB(0xC3); // 11000011

        var result = expander.ReadPortB();

        result.IsSuccessful.Should().BeTrue();
        result.Value.Should().Be(0xC3);
    }

    #endregion

    #region Cross-Port Isolation

    [TestMethod]
    public void WritePin_BothPorts_IndependentState()
    {
        var (expander, client) = CreateExpander();

        // Set pins on both ports
        expander.WritePin(0, true);   // Port A, bit 0
        expander.WritePin(7, true);   // Port A, bit 7
        expander.WritePin(8, true);   // Port B, bit 0
        expander.WritePin(15, true);  // Port B, bit 7

        client.GetOlatA().Should().Be(0x81); // bits 0 and 7
        client.GetOlatB().Should().Be(0x81); // bits 0 and 7
    }

    #endregion

    #region Interface Compliance

    [TestMethod]
    public void Mcp23017_ImplementsIMcp23017Interface()
    {
        var (expander, _) = CreateExpander();
        expander.Should().BeAssignableTo<IMcp23017>();
    }

    [TestMethod]
    public void Mcp23017_Dispose_DoesNotThrow()
    {
        var (expander, _) = CreateExpander();
        var action = () => expander.Dispose();
        action.Should().NotThrow();
    }

    [TestMethod]
    public void Mcp23017_DoubleDispose_DoesNotThrow()
    {
        var (expander, _) = CreateExpander();
        expander.Dispose();
        var action = () => expander.Dispose();
        action.Should().NotThrow();
    }

    #endregion
}
