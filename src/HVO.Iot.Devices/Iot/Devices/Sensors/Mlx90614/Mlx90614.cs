using System;
using System.Device.I2c;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using HVO.Core.Results;
using HVO.Iot.Devices.Abstractions;
using HVO.Iot.Devices.Implementation;
using HVO.Iot.Devices.Iot.Devices.Common;

namespace HVO.Iot.Devices.Iot.Devices.Sensors.Mlx90614;

/// <summary>
/// Driver for the MLX90614 non-contact infrared temperature sensor.
/// Communicates via SMBus (I2C) to read ambient and object temperatures.
/// </summary>
/// <remarks>
/// <para>
/// The MLX90614 is a factory-calibrated IR thermometer for non-contact temperature measurement.
/// It outputs both the ambient (die) temperature and the temperature of the object it is
/// pointed at. The sensor provides 0.02°C resolution.
/// </para>
/// <para>
/// Default I2C address: 0x5A. Data is returned in Kelvin (raw), converted to Celsius by the driver.
/// </para>
/// <para>
/// Migrated from HVOv6 / nF.Devices legacy implementations.
/// </para>
/// </remarks>
public class Mlx90614 : RegisterBasedI2cDevice, IMlx90614
{
    /// <summary>
    /// Default I2C hardware address for the MLX90614.
    /// </summary>
    public const int DefaultI2cAddress = 0x5A;

    #region Register Map

    // RAM registers
    private const byte RawIR1Register = 0x04;
    private const byte RawIR2Register = 0x05;
    private const byte AmbientTempRegister = 0x06;
    private const byte ObjectTemp1Register = 0x07;

    // EEPROM registers (for reference — not currently used)
    // private const byte ToMaxRegister = 0x20;
    // private const byte ToMinRegister = 0x21;
    // private const byte EmissivityRegister = 0x24;
    // private const byte ConfigRegister = 0x25;

    #endregion

    private readonly ILogger<Mlx90614> _logger;

    /// <summary>
    /// Initializes a new instance using a bus number and I2C address.
    /// </summary>
    /// <param name="i2cBus">The I2C bus number (typically 1 on Raspberry Pi).</param>
    /// <param name="i2cAddress">The I2C address of the sensor (default 0x5A).</param>
    /// <param name="logger">Optional logger for diagnostics.</param>
    public Mlx90614(int i2cBus = 1, int i2cAddress = DefaultI2cAddress, ILogger<Mlx90614>? logger = null)
        : this(new I2cRegisterClient(i2cBus, i2cAddress, postTransactionDelayMs: 0), ownsClient: true, logger)
    {
    }

    /// <summary>
    /// Initializes a new instance from an existing <see cref="I2cDevice"/>.
    /// </summary>
    /// <param name="device">An existing I2C device instance.</param>
    /// <param name="logger">Optional logger for diagnostics.</param>
    public Mlx90614(I2cDevice device, ILogger<Mlx90614>? logger = null)
        : this(new I2cRegisterClient(device, ownsDevice: false, postTransactionDelayMs: 0), ownsClient: false, logger)
    {
    }

    /// <summary>
    /// Initializes a new instance using a pre-configured <see cref="II2cRegisterClient"/>.
    /// </summary>
    /// <param name="registerClient">The register client to use for I2C communication.</param>
    /// <param name="logger">Optional logger for diagnostics.</param>
    public Mlx90614(II2cRegisterClient registerClient, ILogger<Mlx90614>? logger = null)
        : this(registerClient, ownsClient: false, logger)
    {
    }

    private Mlx90614(II2cRegisterClient registerClient, bool ownsClient, ILogger<Mlx90614>? logger)
        : base(registerClient, ownsClient)
    {
        _logger = logger ?? NullLogger<Mlx90614>.Instance;
        _logger.LogDebug("MLX90614 initialized on bus {BusId}, address 0x{Address:X2}",
            ConnectionSettings.BusId, ConnectionSettings.DeviceAddress);
    }

    /// <inheritdoc />
    public Result<double> GetAmbientTemperature()
    {
        try
        {
            var celsius = ReadTemperature(AmbientTempRegister);
            _logger.LogTrace("Ambient temperature: {Temperature:F2}°C", celsius);
            return Result<double>.Success(celsius);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to read ambient temperature from MLX90614 at 0x{Address:X2}",
                ConnectionSettings.DeviceAddress);
            return Result<double>.Failure(ex);
        }
    }

    /// <inheritdoc />
    public Result<double> GetObjectTemperature()
    {
        try
        {
            var celsius = ReadTemperature(ObjectTemp1Register);
            _logger.LogTrace("Object temperature: {Temperature:F2}°C", celsius);
            return Result<double>.Success(celsius);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to read object temperature from MLX90614 at 0x{Address:X2}",
                ConnectionSettings.DeviceAddress);
            return Result<double>.Failure(ex);
        }
    }

    /// <inheritdoc />
    public Result<ushort> GetRawIR1()
    {
        try
        {
            lock (Sync)
            {
                var raw = ReadUInt16(RawIR1Register);
                _logger.LogTrace("Raw IR1: {RawValue}", raw);
                return Result<ushort>.Success(raw);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to read raw IR1 from MLX90614 at 0x{Address:X2}",
                ConnectionSettings.DeviceAddress);
            return Result<ushort>.Failure(ex);
        }
    }

    /// <inheritdoc />
    public Result<ushort> GetRawIR2()
    {
        try
        {
            lock (Sync)
            {
                var raw = ReadUInt16(RawIR2Register);
                _logger.LogTrace("Raw IR2: {RawValue}", raw);
                return Result<ushort>.Success(raw);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to read raw IR2 from MLX90614 at 0x{Address:X2}",
                ConnectionSettings.DeviceAddress);
            return Result<ushort>.Failure(ex);
        }
    }

    /// <summary>
    /// Reads a temperature register and converts from the MLX90614 raw format (Kelvin × 50) to degrees Celsius.
    /// </summary>
    /// <param name="register">The register address to read (ambient or object).</param>
    /// <returns>Temperature in degrees Celsius.</returns>
    private double ReadTemperature(byte register)
    {
        lock (Sync)
        {
            // MLX90614 returns 3 bytes: LSB, MSB (with error flag in bit 15), PEC
            // We read as UInt16 (LSB first) and mask off the error flag bit
            Span<byte> buffer = stackalloc byte[3];
            ReadBlock(register, buffer);

            var raw = ((buffer[1] & 0x7F) << 8) | buffer[0];
            var kelvin = raw * 0.02;  // 0.02°K resolution
            return kelvin - 273.15;    // Convert to Celsius
        }
    }
}
