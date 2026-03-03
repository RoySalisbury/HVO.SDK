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
/// Driver for the MCP23017 16-bit I/O expander with I2C interface.
/// Provides individual pin read/write, direction control, and pull-up configuration
/// across two 8-bit ports (A and B).
/// </summary>
/// <remarks>
/// <para>
/// The MCP23017 provides 16 general-purpose I/O pins split into two 8-bit ports:
/// Port A (GPA0–GPA7, pins 0–7) and Port B (GPB0–GPB7, pins 8–15).
/// Each pin can be independently configured as input or output with optional internal
/// 100 kΩ pull-up resistors.
/// </para>
/// <para>
/// Default I2C base address: 0x20 (configurable via A0–A2 hardware pins, range 0x20–0x27).
/// The driver uses IOCON.BANK = 0 (default) register mapping where paired registers
/// are at consecutive addresses (e.g., IODIRA = 0x00, IODIRB = 0x01).
/// </para>
/// <para>
/// Migrated from HVOv6 <c>HualapaiValleyObservatory.IoT.Devices.MCP23017</c>.
/// </para>
/// </remarks>
public class Mcp23017 : RegisterBasedI2cDevice, IMcp23017
{
    /// <summary>
    /// Default I2C hardware address for the MCP23017.
    /// </summary>
    public const int DefaultI2cAddress = 0x20;

    #region Register Addresses (BANK = 0 mode — paired A/B registers at consecutive addresses)

    /// <summary>I/O direction register for port A. 1 = input, 0 = output.</summary>
    internal const byte RegisterIodirA = 0x00;

    /// <summary>I/O direction register for port B.</summary>
    internal const byte RegisterIodirB = 0x01;

    /// <summary>Input polarity register for port A. 1 = inverted.</summary>
    internal const byte RegisterIpolA = 0x02;

    /// <summary>Input polarity register for port B.</summary>
    internal const byte RegisterIpolB = 0x03;

    /// <summary>Interrupt-on-change enable register for port A.</summary>
    internal const byte RegisterGpintenA = 0x04;

    /// <summary>Interrupt-on-change enable register for port B.</summary>
    internal const byte RegisterGpintenB = 0x05;

    /// <summary>Default compare value register for port A.</summary>
    internal const byte RegisterDefvalA = 0x06;

    /// <summary>Default compare value register for port B.</summary>
    internal const byte RegisterDefvalB = 0x07;

    /// <summary>Interrupt control register for port A.</summary>
    internal const byte RegisterIntconA = 0x08;

    /// <summary>Interrupt control register for port B.</summary>
    internal const byte RegisterIntconB = 0x09;

    /// <summary>Configuration register for port A (IOCON — shared, same register).</summary>
    internal const byte RegisterIoconA = 0x0A;

    /// <summary>Configuration register for port B (IOCON — shared, same register).</summary>
    internal const byte RegisterIoconB = 0x0B;

    /// <summary>Pull-up resistor enable register for port A.</summary>
    internal const byte RegisterGppuA = 0x0C;

    /// <summary>Pull-up resistor enable register for port B.</summary>
    internal const byte RegisterGppuB = 0x0D;

    /// <summary>Interrupt flag register for port A (read-only).</summary>
    internal const byte RegisterIntfA = 0x0E;

    /// <summary>Interrupt flag register for port B (read-only).</summary>
    internal const byte RegisterIntfB = 0x0F;

    /// <summary>Interrupt captured value register for port A (read-only).</summary>
    internal const byte RegisterIntcapA = 0x10;

    /// <summary>Interrupt captured value register for port B (read-only).</summary>
    internal const byte RegisterIntcapB = 0x11;

    /// <summary>GPIO port register for port A.</summary>
    internal const byte RegisterGpioA = 0x12;

    /// <summary>GPIO port register for port B.</summary>
    internal const byte RegisterGpioB = 0x13;

    /// <summary>Output latch register for port A.</summary>
    internal const byte RegisterOlatA = 0x14;

    /// <summary>Output latch register for port B.</summary>
    internal const byte RegisterOlatB = 0x15;

    #endregion

    private readonly ILogger<Mcp23017> _logger;

    /// <summary>
    /// Initializes a new instance using a bus number and I2C address.
    /// </summary>
    /// <param name="i2cBus">The I2C bus number (typically 1 on Raspberry Pi).</param>
    /// <param name="i2cAddress">The I2C address of the expander (default 0x20).</param>
    /// <param name="logger">Optional logger for diagnostics.</param>
    public Mcp23017(int i2cBus = 1, int i2cAddress = DefaultI2cAddress, ILogger<Mcp23017>? logger = null)
        : this(new I2cRegisterClient(i2cBus, i2cAddress), ownsClient: true, logger)
    {
    }

    /// <summary>
    /// Initializes a new instance from an existing <see cref="I2cDevice"/>.
    /// </summary>
    /// <param name="device">An existing I2C device instance.</param>
    /// <param name="logger">Optional logger for diagnostics.</param>
    public Mcp23017(I2cDevice device, ILogger<Mcp23017>? logger = null)
        : this(new I2cRegisterClient(device, ownsDevice: false), ownsClient: false, logger)
    {
    }

    /// <summary>
    /// Initializes a new instance using a pre-configured <see cref="II2cRegisterClient"/>.
    /// </summary>
    /// <param name="registerClient">The register client to use for I2C communication.</param>
    /// <param name="logger">Optional logger for diagnostics.</param>
    public Mcp23017(II2cRegisterClient registerClient, ILogger<Mcp23017>? logger = null)
        : this(registerClient, ownsClient: false, logger)
    {
    }

    private Mcp23017(II2cRegisterClient registerClient, bool ownsClient, ILogger<Mcp23017>? logger)
        : base(registerClient, ownsClient)
    {
        _logger = logger ?? NullLogger<Mcp23017>.Instance;
        _logger.LogDebug("MCP23017 initialized on bus {BusId}, address 0x{Address:X2}",
            ConnectionSettings.BusId, ConnectionSettings.DeviceAddress);
    }

    /// <inheritdoc />
    public byte PinCount => 16;

    /// <inheritdoc />
    public Result<bool> ReadPin(byte pin)
    {
        try
        {
            ValidatePin(pin);
            var (gpioReg, localPin) = GetPortRegister(pin, RegisterGpioA, RegisterGpioB);

            lock (Sync)
            {
                var gpio = ReadByte(gpioReg);
                var value = Mcp23008.GetBit(gpio, localPin);
                _logger.LogTrace("ReadPin {Pin} (port {Port}, local {LocalPin}) = {Value} (GPIO: 0x{Register:X2})",
                    pin, pin < 8 ? "A" : "B", localPin, value, gpio);
                return Result<bool>.Success(value);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to read pin {Pin} on MCP23017 at 0x{Address:X2}",
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
            var (olatReg, localPin) = GetPortRegister(pin, RegisterOlatA, RegisterOlatB);
            var (gpioReg, _) = GetPortRegister(pin, RegisterGpioA, RegisterGpioB);

            lock (Sync)
            {
                var olat = ReadByte(olatReg);
                var updated = Mcp23008.SetBit(olat, localPin, value);
                WriteByte(gpioReg, updated);

                _logger.LogTrace("WritePin {Pin} (port {Port}, local {LocalPin}) = {Value} (OLAT: 0x{Before:X2} -> 0x{After:X2})",
                    pin, pin < 8 ? "A" : "B", localPin, value, olat, updated);
                return Result<bool>.Success(true);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to write pin {Pin} on MCP23017 at 0x{Address:X2}",
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
            var (iodirReg, localPin) = GetPortRegister(pin, RegisterIodirA, RegisterIodirB);

            lock (Sync)
            {
                var iodir = ReadByte(iodirReg);
                var updated = Mcp23008.SetBit(iodir, localPin, isInput);
                WriteByte(iodirReg, updated);

                _logger.LogTrace("SetPinDirection {Pin} (port {Port}, local {LocalPin}) = {Direction} (IODIR: 0x{Before:X2} -> 0x{After:X2})",
                    pin, pin < 8 ? "A" : "B", localPin, isInput ? "Input" : "Output", iodir, updated);
                return Result<bool>.Success(true);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to set pin {Pin} direction on MCP23017 at 0x{Address:X2}",
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
            var (gppuReg, localPin) = GetPortRegister(pin, RegisterGppuA, RegisterGppuB);

            lock (Sync)
            {
                var gppu = ReadByte(gppuReg);
                var updated = Mcp23008.SetBit(gppu, localPin, enabled);
                WriteByte(gppuReg, updated);

                _logger.LogTrace("SetPinPullup {Pin} (port {Port}, local {LocalPin}) = {Enabled} (GPPU: 0x{Before:X2} -> 0x{After:X2})",
                    pin, pin < 8 ? "A" : "B", localPin, enabled, gppu, updated);
                return Result<bool>.Success(true);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to set pin {Pin} pull-up on MCP23017 at 0x{Address:X2}",
                pin, ConnectionSettings.DeviceAddress);
            return Result<bool>.Failure(ex);
        }
    }

    /// <inheritdoc />
    public Result<byte> ReadPortA()
    {
        try
        {
            lock (Sync)
            {
                var gpio = ReadByte(RegisterGpioA);
                _logger.LogTrace("ReadPortA = 0x{Value:X2}", gpio);
                return Result<byte>.Success(gpio);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to read port A on MCP23017 at 0x{Address:X2}",
                ConnectionSettings.DeviceAddress);
            return Result<byte>.Failure(ex);
        }
    }

    /// <inheritdoc />
    public Result<byte> ReadPortB()
    {
        try
        {
            lock (Sync)
            {
                var gpio = ReadByte(RegisterGpioB);
                _logger.LogTrace("ReadPortB = 0x{Value:X2}", gpio);
                return Result<byte>.Success(gpio);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to read port B on MCP23017 at 0x{Address:X2}",
                ConnectionSettings.DeviceAddress);
            return Result<byte>.Failure(ex);
        }
    }

    /// <summary>
    /// Maps a global pin number (0–15) to the corresponding port register and local bit position.
    /// Pins 0–7 target port A, pins 8–15 target port B.
    /// </summary>
    /// <param name="pin">Global pin number (0–15).</param>
    /// <param name="portARegister">The register address for port A.</param>
    /// <param name="portBRegister">The register address for port B.</param>
    /// <returns>A tuple of (register address, local pin 0–7).</returns>
    private static (byte register, byte localPin) GetPortRegister(byte pin, byte portARegister, byte portBRegister)
    {
        if (pin < 8)
        {
            return (portARegister, pin);
        }

        return (portBRegister, (byte)(pin - 8));
    }

    /// <summary>
    /// Validates that a pin number is within the valid range for this expander.
    /// </summary>
    /// <param name="pin">The pin number to validate.</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when pin is outside 0–15.</exception>
    private void ValidatePin(byte pin)
    {
        if (pin >= PinCount)
        {
            throw new ArgumentOutOfRangeException(nameof(pin), pin,
                $"Pin number must be 0–{PinCount - 1}.");
        }
    }
}
