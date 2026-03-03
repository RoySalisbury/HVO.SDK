using System;
using System.Device.I2c;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using HVO.Core.Results;
using HVO.Iot.Devices.Abstractions;
using HVO.Iot.Devices.Implementation;
using HVO.Iot.Devices.Iot.Devices.Common;

namespace HVO.Iot.Devices.Iot.Devices.Sensors.Ds3231m;

/// <summary>
/// Driver for the DS3231M real-time clock (RTC) with integrated temperature compensated crystal oscillator.
/// Communicates via I2C to read/write date-time and read the on-chip temperature sensor.
/// </summary>
/// <remarks>
/// <para>
/// The DS3231M is an extremely accurate I2C real-time clock with an integrated temperature-compensated
/// MEMS resonator. It maintains seconds, minutes, hours, day, date, month, and year with leap-year
/// compensation valid up to 2100. Accuracy is ±5 ppm from -45°C to +85°C.
/// </para>
/// <para>
/// Default I2C address: 0x68. The on-chip temperature sensor provides 0.25°C resolution
/// and is used internally for frequency compensation.
/// </para>
/// <para>
/// Migrated from HVOv6 <c>HualapaiValleyObservatory.IoT.Devices.DS3231M</c>.
/// GPIO alarm pin support from the legacy version has been omitted — alarm functionality
/// can be added in a future release if needed.
/// </para>
/// </remarks>
public class Ds3231m : RegisterBasedI2cDevice, IDs3231m
{
    /// <summary>
    /// Default I2C hardware address for the DS3231M.
    /// </summary>
    public const int DefaultI2cAddress = 0x68;

    #region Register Map

    /// <summary>Timekeeping registers start address (seconds through year).</summary>
    private const byte TimeCalRegister = 0x00;

    /// <summary>Control register address.</summary>
    private const byte ControlRegister = 0x0E;

    /// <summary>Status register address.</summary>
    private const byte StatusRegister = 0x0F;

    /// <summary>Temperature MSB register address.</summary>
    private const byte TemperatureRegister = 0x11;

    /// <summary>Oscillator stop flag bit mask in the status register.</summary>
    private const byte OscillatorStopFlagMask = 0x80;

    /// <summary>INTCN control bit — enables interrupt output on SQW pin.</summary>
    private const byte IntcnBit = 0x04;

    #endregion

    private readonly ILogger<Ds3231m> _logger;

    /// <summary>
    /// Initializes a new instance using a bus number and I2C address.
    /// </summary>
    /// <param name="i2cBus">The I2C bus number (typically 1 on Raspberry Pi).</param>
    /// <param name="i2cAddress">The I2C address of the RTC (default 0x68).</param>
    /// <param name="logger">Optional logger for diagnostics.</param>
    public Ds3231m(int i2cBus = 1, int i2cAddress = DefaultI2cAddress, ILogger<Ds3231m>? logger = null)
        : this(new I2cRegisterClient(i2cBus, i2cAddress, postTransactionDelayMs: 0), ownsClient: true, logger)
    {
    }

    /// <summary>
    /// Initializes a new instance from an existing <see cref="I2cDevice"/>.
    /// </summary>
    /// <param name="device">An existing I2C device instance.</param>
    /// <param name="logger">Optional logger for diagnostics.</param>
    public Ds3231m(I2cDevice device, ILogger<Ds3231m>? logger = null)
        : this(new I2cRegisterClient(device, ownsDevice: false, postTransactionDelayMs: 0), ownsClient: false, logger)
    {
    }

    /// <summary>
    /// Initializes a new instance using a pre-configured <see cref="II2cRegisterClient"/>.
    /// </summary>
    /// <param name="registerClient">The register client to use for I2C communication.</param>
    /// <param name="logger">Optional logger for diagnostics.</param>
    public Ds3231m(II2cRegisterClient registerClient, ILogger<Ds3231m>? logger = null)
        : this(registerClient, ownsClient: false, logger)
    {
    }

    private Ds3231m(II2cRegisterClient registerClient, bool ownsClient, ILogger<Ds3231m>? logger)
        : base(registerClient, ownsClient)
    {
        _logger = logger ?? NullLogger<Ds3231m>.Instance;
        _logger.LogDebug("DS3231M initialized on bus {BusId}, address 0x{Address:X2}",
            ConnectionSettings.BusId, ConnectionSettings.DeviceAddress);
    }

    /// <inheritdoc />
    public Result<DateTimeOffset> GetDateTime()
    {
        try
        {
            lock (Sync)
            {
                Span<byte> buffer = stackalloc byte[7];
                ReadBlock(TimeCalRegister, buffer);

                var year = ((buffer[5] & 0x80) >> 7) == 1
                    ? BcdToDec(buffer[6]) + 2000
                    : BcdToDec(buffer[6]) + 1900;

                var month = BcdToDec(buffer[5] & 0x1F);
                var day = BcdToDec(buffer[4]);
                var hour = BcdToDec(buffer[2]);
                var minute = BcdToDec(buffer[1]);
                var second = BcdToDec(buffer[0]);

                var result = new DateTimeOffset(year, month, day, hour, minute, second, TimeSpan.Zero);

                _logger.LogTrace("RTC time: {DateTime:O}", result);
                return Result<DateTimeOffset>.Success(result);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to read date/time from DS3231M at 0x{Address:X2}",
                ConnectionSettings.DeviceAddress);
            return Result<DateTimeOffset>.Failure(ex);
        }
    }

    /// <inheritdoc />
    public Result<bool> SetDateTime(DateTimeOffset value)
    {
        try
        {
            var utc = value.ToUniversalTime();

            lock (Sync)
            {
                // Write all 7 timekeeping registers in one block
                Span<byte> data = stackalloc byte[7];
                data[0] = DecToBcd(utc.Second);
                data[1] = DecToBcd(utc.Minute);
                data[2] = DecToBcd(utc.Hour);
                data[3] = (byte)(((int)utc.DayOfWeek + 7) % 7);
                data[4] = DecToBcd(utc.Day);
                data[5] = utc.Year >= 2000
                    ? (byte)(DecToBcd(utc.Month) | 0x80)
                    : DecToBcd(utc.Month);
                data[6] = DecToBcd(utc.Year % 100);

                WriteBlock(TimeCalRegister, data);
            }

            _logger.LogInformation("RTC time set to {DateTime:O}", utc);
            return Result<bool>.Success(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to set date/time on DS3231M at 0x{Address:X2}",
                ConnectionSettings.DeviceAddress);
            return Result<bool>.Failure(ex);
        }
    }

    /// <inheritdoc />
    public Result<double> GetTemperature()
    {
        try
        {
            lock (Sync)
            {
                Span<byte> buffer = stackalloc byte[2];
                ReadBlock(TemperatureRegister, buffer);

                // MSB is signed integer part, upper 2 bits of LSB are fractional (0.25°C steps)
                int msb = (sbyte)buffer[0]; // sign-extend
                var celsius = msb + (0.25 * (buffer[1] >> 6));

                _logger.LogTrace("RTC temperature: {Temperature:F2}°C", celsius);
                return Result<double>.Success(celsius);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to read temperature from DS3231M at 0x{Address:X2}",
                ConnectionSettings.DeviceAddress);
            return Result<double>.Failure(ex);
        }
    }

    /// <inheritdoc />
    public Result<bool> GetOscillatorStopFlag()
    {
        try
        {
            lock (Sync)
            {
                var status = ReadByte(StatusRegister);
                var osf = (status & OscillatorStopFlagMask) != 0;

                if (osf)
                {
                    _logger.LogWarning("DS3231M oscillator stop flag is set — clock may have lost power");
                }

                return Result<bool>.Success(osf);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to read OSF from DS3231M at 0x{Address:X2}",
                ConnectionSettings.DeviceAddress);
            return Result<bool>.Failure(ex);
        }
    }

    /// <inheritdoc />
    public Result<bool> ClearOscillatorStopFlag()
    {
        try
        {
            lock (Sync)
            {
                var status = ReadByte(StatusRegister);
                status = (byte)(status & ~OscillatorStopFlagMask);
                WriteByte(StatusRegister, status);
            }

            _logger.LogInformation("DS3231M oscillator stop flag cleared");
            return Result<bool>.Success(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to clear OSF on DS3231M at 0x{Address:X2}",
                ConnectionSettings.DeviceAddress);
            return Result<bool>.Failure(ex);
        }
    }

    /// <summary>
    /// Converts a BCD-encoded byte to its decimal equivalent.
    /// </summary>
    private static int BcdToDec(int value) => (value / 16 * 10) + (value % 16);

    /// <summary>
    /// Converts a decimal value to BCD encoding.
    /// </summary>
    private static byte DecToBcd(int value) => (byte)((value / 10 * 16) + (value % 10));
}
