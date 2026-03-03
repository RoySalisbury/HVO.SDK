using System;
using System.Device.I2c;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using HVO.Core.Results;
using HVO.Iot.Devices.Abstractions;
using HVO.Iot.Devices.Implementation;
using HVO.Iot.Devices.Iot.Devices.Common;

namespace HVO.Iot.Devices.Iot.Devices.Sensors.Htu21df;

/// <summary>
/// Driver for the HTU21D-F digital humidity and temperature sensor.
/// Communicates via I2C to read calibrated temperature and relative humidity values.
/// </summary>
/// <remarks>
/// <para>
/// The HTU21D-F is a factory-calibrated digital humidity sensor with an integrated temperature sensor.
/// It provides ±2% RH accuracy and ±0.3°C temperature accuracy.
/// </para>
/// <para>
/// Default I2C address: 0x40. The sensor uses a hold-master measurement mode where the I2C bus
/// is held during conversion (up to 50 ms for 14-bit temperature, up to 16 ms for 12-bit humidity).
/// </para>
/// <para>
/// Migrated from HVOv6 <c>HualapaiValleyObservatory.IoT.Devices.HTU21DF</c>.
/// </para>
/// </remarks>
public class Htu21df : RegisterBasedI2cDevice, IHtu21df
{
    /// <summary>
    /// Default I2C hardware address for the HTU21D-F.
    /// </summary>
    public const int DefaultI2cAddress = 0x40;

    #region Command Bytes

    /// <summary>Trigger temperature measurement (hold master).</summary>
    private const byte ReadTempCommand = 0xE3;

    /// <summary>Trigger humidity measurement (hold master).</summary>
    private const byte ReadHumidityCommand = 0xE5;

    /// <summary>Write user register.</summary>
    private const byte WriteUserRegCommand = 0xE6;

    /// <summary>Read user register.</summary>
    private const byte ReadUserRegCommand = 0xE7;

    /// <summary>Soft reset command.</summary>
    private const byte ResetCommand = 0xFE;

    /// <summary>Expected user register default value after reset.</summary>
    private const byte ExpectedUserRegDefault = 0x02;

    #endregion

    private readonly ILogger<Htu21df> _logger;

    /// <summary>
    /// Initializes a new instance using a bus number and I2C address.
    /// </summary>
    /// <param name="i2cBus">The I2C bus number (typically 1 on Raspberry Pi).</param>
    /// <param name="i2cAddress">The I2C address of the sensor (default 0x40).</param>
    /// <param name="logger">Optional logger for diagnostics.</param>
    public Htu21df(int i2cBus = 1, int i2cAddress = DefaultI2cAddress, ILogger<Htu21df>? logger = null)
        : this(new I2cRegisterClient(i2cBus, i2cAddress, postTransactionDelayMs: 50), ownsClient: true, logger)
    {
    }

    /// <summary>
    /// Initializes a new instance from an existing <see cref="I2cDevice"/>.
    /// </summary>
    /// <param name="device">An existing I2C device instance.</param>
    /// <param name="logger">Optional logger for diagnostics.</param>
    public Htu21df(I2cDevice device, ILogger<Htu21df>? logger = null)
        : this(new I2cRegisterClient(device, ownsDevice: false, postTransactionDelayMs: 50), ownsClient: false, logger)
    {
    }

    /// <summary>
    /// Initializes a new instance using a pre-configured <see cref="II2cRegisterClient"/>.
    /// </summary>
    /// <param name="registerClient">The register client to use for I2C communication.</param>
    /// <param name="logger">Optional logger for diagnostics.</param>
    public Htu21df(II2cRegisterClient registerClient, ILogger<Htu21df>? logger = null)
        : this(registerClient, ownsClient: false, logger)
    {
    }

    private Htu21df(II2cRegisterClient registerClient, bool ownsClient, ILogger<Htu21df>? logger)
        : base(registerClient, ownsClient)
    {
        _logger = logger ?? NullLogger<Htu21df>.Instance;
        _logger.LogDebug("HTU21D-F initialized on bus {BusId}, address 0x{Address:X2}",
            ConnectionSettings.BusId, ConnectionSettings.DeviceAddress);
    }

    /// <inheritdoc />
    public Result<double> GetTemperature()
    {
        try
        {
            lock (Sync)
            {
                // HTU21D-F returns 3 bytes: MSB, LSB, checksum
                Span<byte> buffer = stackalloc byte[3];
                ReadBlock(ReadTempCommand, buffer);

                var raw = (buffer[0] << 8) | buffer[1];
                var celsius = ((raw / 65536.0) * 175.72) - 46.85;

                _logger.LogTrace("Temperature: {Temperature:F2}°C (raw: 0x{Raw:X4})", celsius, raw);
                return Result<double>.Success(celsius);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to read temperature from HTU21D-F at 0x{Address:X2}",
                ConnectionSettings.DeviceAddress);
            return Result<double>.Failure(ex);
        }
    }

    /// <inheritdoc />
    public Result<double> GetHumidity()
    {
        try
        {
            lock (Sync)
            {
                // HTU21D-F returns 3 bytes: MSB, LSB, checksum
                Span<byte> buffer = stackalloc byte[3];
                ReadBlock(ReadHumidityCommand, buffer);

                var raw = (buffer[0] << 8) | buffer[1];
                var humidity = ((raw / 65536.0) * 125.0) - 6.0;

                _logger.LogTrace("Humidity: {Humidity:F2}% (raw: 0x{Raw:X4})", humidity, raw);
                return Result<double>.Success(humidity);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to read humidity from HTU21D-F at 0x{Address:X2}",
                ConnectionSettings.DeviceAddress);
            return Result<double>.Failure(ex);
        }
    }
}
