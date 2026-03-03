using System;
using System.Device.I2c;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using HVO.Core.Results;
using HVO.Iot.Devices.Abstractions;
using HVO.Iot.Devices.Implementation;
using HVO.Iot.Devices.Iot.Devices.Common;

namespace HVO.Iot.Devices.Iot.Devices.Sensors.Mcp23xxx;

/// <summary>
/// Driver for the MCP23008 8-bit I/O expander with I2C interface.
/// Provides individual pin read/write, direction control, and pull-up configuration.
/// </summary>
/// <remarks>
/// <para>
/// The MCP23008 provides 8 general-purpose I/O pins (GP0–GP7) accessible over I2C.
/// Each pin can be independently configured as input or output with optional internal
/// 100 kΩ pull-up resistors.
/// </para>
/// <para>
/// Default I2C base address: 0x20 (configurable via A0–A2 hardware pins, range 0x20–0x27).
/// All pins default to inputs on power-up (IODIR = 0xFF).
/// </para>
/// <para>
/// Migrated from HVOv6 <c>HualapaiValleyObservatory.IoT.Devices.MCP23008</c>.
/// </para>
/// </remarks>
public class Mcp23008 : RegisterBasedI2cDevice, IMcp23008
{
    /// <summary>
    /// Default I2C hardware address for the MCP23008.
    /// </summary>
    public const int DefaultI2cAddress = 0x20;

    #region Register Addresses

    /// <summary>I/O direction register. 1 = input, 0 = output.</summary>
    internal const byte RegisterIodir = 0x00;

    /// <summary>Input polarity register. 1 = inverted.</summary>
    internal const byte RegisterIpol = 0x01;

    /// <summary>Interrupt-on-change enable register.</summary>
    internal const byte RegisterGpinten = 0x02;

    /// <summary>Default compare value for interrupt-on-change.</summary>
    internal const byte RegisterDefval = 0x03;

    /// <summary>Interrupt control register. 1 = compare to DEFVAL, 0 = compare to previous.</summary>
    internal const byte RegisterIntcon = 0x04;

    /// <summary>Configuration register (IOCON).</summary>
    internal const byte RegisterIocon = 0x05;

    /// <summary>Pull-up resistor enable register. 1 = pull-up enabled.</summary>
    internal const byte RegisterGppu = 0x06;

    /// <summary>Interrupt flag register (read-only). Indicates which pin caused the interrupt.</summary>
    internal const byte RegisterIntf = 0x07;

    /// <summary>Interrupt captured value register (read-only). Captures pin state at interrupt time.</summary>
    internal const byte RegisterIntcap = 0x08;

    /// <summary>GPIO port register. Read returns pin state; write sets output latch.</summary>
    internal const byte RegisterGpio = 0x09;

    /// <summary>Output latch register.</summary>
    internal const byte RegisterOlat = 0x0A;

    #endregion

    private readonly ILogger<Mcp23008> _logger;

    /// <summary>
    /// Initializes a new instance using a bus number and I2C address.
    /// </summary>
    /// <param name="i2cBus">The I2C bus number (typically 1 on Raspberry Pi).</param>
    /// <param name="i2cAddress">The I2C address of the expander (default 0x20).</param>
    /// <param name="logger">Optional logger for diagnostics.</param>
    public Mcp23008(int i2cBus = 1, int i2cAddress = DefaultI2cAddress, ILogger<Mcp23008>? logger = null)
        : this(new I2cRegisterClient(i2cBus, i2cAddress), ownsClient: true, logger)
    {
    }

    /// <summary>
    /// Initializes a new instance from an existing <see cref="I2cDevice"/>.
    /// </summary>
    /// <param name="device">An existing I2C device instance.</param>
    /// <param name="logger">Optional logger for diagnostics.</param>
    public Mcp23008(I2cDevice device, ILogger<Mcp23008>? logger = null)
        : this(new I2cRegisterClient(device, ownsDevice: false), ownsClient: false, logger)
    {
    }

    /// <summary>
    /// Initializes a new instance using a pre-configured <see cref="II2cRegisterClient"/>.
    /// </summary>
    /// <param name="registerClient">The register client to use for I2C communication.</param>
    /// <param name="logger">Optional logger for diagnostics.</param>
    public Mcp23008(II2cRegisterClient registerClient, ILogger<Mcp23008>? logger = null)
        : this(registerClient, ownsClient: false, logger)
    {
    }

    private Mcp23008(II2cRegisterClient registerClient, bool ownsClient, ILogger<Mcp23008>? logger)
        : base(registerClient, ownsClient)
    {
        _logger = logger ?? NullLogger<Mcp23008>.Instance;
        _logger.LogDebug("MCP23008 initialized on bus {BusId}, address 0x{Address:X2}",
            ConnectionSettings.BusId, ConnectionSettings.DeviceAddress);
    }

    /// <inheritdoc />
    public byte PinCount => 8;

    /// <inheritdoc />
    public Result<bool> ReadPin(byte pin)
    {
        try
        {
            ValidatePin(pin);

            lock (Sync)
            {
                var gpio = ReadByte(RegisterGpio);
                var value = GetBit(gpio, pin);
                _logger.LogTrace("ReadPin {Pin} = {Value} (GPIO: 0x{Register:X2})", pin, value, gpio);
                return Result<bool>.Success(value);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to read pin {Pin} on MCP23008 at 0x{Address:X2}",
                pin, ConnectionSettings.DeviceAddress);
            return Result<bool>.Failure(ex);
        }
    }

    /// <inheritdoc />
    public Result<bool> WritePin(byte pin, bool value)
    {
        try
        {
            ValidatePin(pin);

            lock (Sync)
            {
                var olat = ReadByte(RegisterOlat);
                var updated = SetBit(olat, pin, value);
                WriteByte(RegisterGpio, updated);

                _logger.LogTrace("WritePin {Pin} = {Value} (OLAT: 0x{Before:X2} -> 0x{After:X2})",
                    pin, value, olat, updated);
                return Result<bool>.Success(true);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to write pin {Pin} on MCP23008 at 0x{Address:X2}",
                pin, ConnectionSettings.DeviceAddress);
            return Result<bool>.Failure(ex);
        }
    }

    /// <inheritdoc />
    public Result<bool> SetPinDirection(byte pin, bool isInput)
    {
        try
        {
            ValidatePin(pin);

            lock (Sync)
            {
                var iodir = ReadByte(RegisterIodir);
                var updated = SetBit(iodir, pin, isInput);
                WriteByte(RegisterIodir, updated);

                _logger.LogTrace("SetPinDirection {Pin} = {Direction} (IODIR: 0x{Before:X2} -> 0x{After:X2})",
                    pin, isInput ? "Input" : "Output", iodir, updated);
                return Result<bool>.Success(true);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to set pin {Pin} direction on MCP23008 at 0x{Address:X2}",
                pin, ConnectionSettings.DeviceAddress);
            return Result<bool>.Failure(ex);
        }
    }

    /// <inheritdoc />
    public Result<bool> SetPinPullup(byte pin, bool enabled)
    {
        try
        {
            ValidatePin(pin);

            lock (Sync)
            {
                var gppu = ReadByte(RegisterGppu);
                var updated = SetBit(gppu, pin, enabled);
                WriteByte(RegisterGppu, updated);

                _logger.LogTrace("SetPinPullup {Pin} = {Enabled} (GPPU: 0x{Before:X2} -> 0x{After:X2})",
                    pin, enabled, gppu, updated);
                return Result<bool>.Success(true);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to set pin {Pin} pull-up on MCP23008 at 0x{Address:X2}",
                pin, ConnectionSettings.DeviceAddress);
            return Result<bool>.Failure(ex);
        }
    }

    /// <inheritdoc />
    public Result<byte> ReadAllPins()
    {
        try
        {
            lock (Sync)
            {
                var gpio = ReadByte(RegisterGpio);
                _logger.LogTrace("ReadAllPins = 0x{Value:X2}", gpio);
                return Result<byte>.Success(gpio);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to read GPIO port on MCP23008 at 0x{Address:X2}",
                ConnectionSettings.DeviceAddress);
            return Result<byte>.Failure(ex);
        }
    }

    #region Bit Manipulation Helpers

    /// <summary>
    /// Sets or clears a single bit in a byte value.
    /// </summary>
    /// <param name="value">The original byte value.</param>
    /// <param name="bit">The bit position (0–7).</param>
    /// <param name="state">True to set, false to clear.</param>
    /// <returns>The modified byte value.</returns>
    internal static byte SetBit(byte value, byte bit, bool state)
    {
        return state
            ? (byte)(value | (1 << bit))
            : (byte)(value & ~(1 << bit));
    }

    /// <summary>
    /// Reads a single bit from a byte value.
    /// </summary>
    /// <param name="value">The byte value to read from.</param>
    /// <param name="bit">The bit position (0–7).</param>
    /// <returns>True if the bit is set, false otherwise.</returns>
    internal static bool GetBit(byte value, byte bit)
    {
        return (value & (1 << bit)) != 0;
    }

    #endregion

    /// <summary>
    /// Validates that a pin number is within the valid range for this expander.
    /// </summary>
    /// <param name="pin">The pin number to validate.</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when pin is outside 0–7.</exception>
    private void ValidatePin(byte pin)
    {
        if (pin >= PinCount)
        {
            throw new ArgumentOutOfRangeException(nameof(pin), pin,
                $"Pin number must be 0–{PinCount - 1}.");
        }
    }
}
