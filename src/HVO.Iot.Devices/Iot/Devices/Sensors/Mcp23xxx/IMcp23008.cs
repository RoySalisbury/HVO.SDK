using System;
using HVO.Core.Results;

namespace HVO.Iot.Devices.Iot.Devices.Sensors.Mcp23xxx;

/// <summary>
/// Abstraction for the MCP23008 8-port I2C GPIO expander.
/// Enables dependency injection to swap between hardware-backed and simulated implementations.
/// </summary>
public interface IMcp23008 : IDisposable
{
    /// <summary>
    /// Gets the total number of I/O pins provided by this expander (8).
    /// </summary>
    byte PinCount { get; }

    /// <summary>
    /// Reads the current logic level of a GPIO pin.
    /// </summary>
    /// <param name="pin">Pin number (0–7).</param>
    Result<bool> ReadPin(byte pin);

    /// <summary>
    /// Writes a logic level to a GPIO pin.
    /// </summary>
    /// <param name="pin">Pin number (0–7).</param>
    /// <param name="value">True for high, false for low.</param>
    Result<bool> WritePin(byte pin, bool value);

    /// <summary>
    /// Configures a pin as input or output.
    /// </summary>
    /// <param name="pin">Pin number (0–7).</param>
    /// <param name="isInput">True for input, false for output.</param>
    Result<bool> SetPinDirection(byte pin, bool isInput);

    /// <summary>
    /// Enables or disables the internal pull-up resistor on a pin.
    /// </summary>
    /// <param name="pin">Pin number (0–7).</param>
    /// <param name="enabled">True to enable pull-up, false to disable.</param>
    Result<bool> SetPinPullup(byte pin, bool enabled);

    /// <summary>
    /// Reads the entire GPIO port register as a byte (all 8 pins).
    /// </summary>
    Result<byte> ReadAllPins();
}
