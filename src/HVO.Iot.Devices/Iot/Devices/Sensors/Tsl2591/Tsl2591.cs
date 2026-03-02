using System;
using System.Device.I2c;
using System.Threading;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using HVO.Core.Results;
using HVO.Iot.Devices.Abstractions;
using HVO.Iot.Devices.Implementation;
using HVO.Iot.Devices.Iot.Devices.Common;

namespace HVO.Iot.Devices.Iot.Devices.Sensors.Tsl2591;

/// <summary>
/// Driver for the TSL2591 high-dynamic-range digital light sensor.
/// Communicates via I2C to read visible light, infrared, and calculated lux values
/// with automatic gain ranging capable of measuring 188 µLux to 88,000 Lux.
/// </summary>
/// <remarks>
/// <para>
/// The TSL2591 is an advanced digital light sensor with a very wide dynamic range (600M:1).
/// It contains two photodiodes — one for full spectrum (visible + IR) and one for IR only —
/// allowing the driver to compute visible light and lux values.
/// </para>
/// <para>
/// The sensor supports four gain levels (1×, 25×, 428×, 9876×) and six integration times
/// (100–600ms). The <see cref="GetGainAdjustedLuminosity"/> method automatically selects
/// the best gain for the current light level.
/// </para>
/// <para>
/// Default I2C address: 0x29. Migrated from HVOv6 / nF.Devices legacy implementations.
/// </para>
/// </remarks>
public class Tsl2591 : RegisterBasedI2cDevice, ITsl2591
{
    /// <summary>
    /// Default I2C hardware address for the TSL2591.
    /// </summary>
    public const int DefaultI2cAddress = 0x29;

    /// <summary>
    /// Expected device ID value read from the identification register.
    /// </summary>
    public const byte ExpectedDeviceId = 0x50;

    #region Register Map

    private const byte Command = 0x80;
    private const byte NormalOp = 0x20;

    private const byte EnableRegister = Command | NormalOp | 0x00;
    private const byte ControlRegister = Command | NormalOp | 0x01;
    private const byte IdRegister = Command | NormalOp | 0x12;
    private const byte Channel0DataLow = Command | NormalOp | 0x14;

    private const byte DeviceResetValue = 0x80;

    #endregion

    #region Lux Calculation Constants

    private const double DefaultLuxPerCount = 408.0;
    private const double LuxCoefB = 1.64;
    private const double LuxCoefC = 0.59;
    private const double LuxCoefD = 0.86;

    #endregion

    #region Gain Multipliers

    private const double GainMultiplierLow = 1.0;
    private const double GainMultiplierMedium = 25.0;
    private const double GainMultiplierHigh = 428.0;
    private const double GainMultiplierMax = 9876.0;

    #endregion

    private readonly ILogger<Tsl2591> _logger;

    /// <summary>
    /// Initializes a new instance using a bus number and I2C address.
    /// </summary>
    /// <param name="i2cBus">The I2C bus number (typically 1 on Raspberry Pi).</param>
    /// <param name="i2cAddress">The I2C address of the sensor (default 0x29).</param>
    /// <param name="logger">Optional logger for diagnostics.</param>
    /// <param name="skipInitialization">If true, skip hardware initialization (used for testing with memory clients).</param>
    public Tsl2591(int i2cBus = 1, int i2cAddress = DefaultI2cAddress, ILogger<Tsl2591>? logger = null, bool skipInitialization = false)
        : this(new I2cRegisterClient(i2cBus, i2cAddress, postTransactionDelayMs: 0), ownsClient: true, logger, skipInitialization)
    {
    }

    /// <summary>
    /// Initializes a new instance from an existing <see cref="I2cDevice"/>.
    /// </summary>
    /// <param name="device">An existing I2C device instance.</param>
    /// <param name="logger">Optional logger for diagnostics.</param>
    /// <param name="skipInitialization">If true, skip hardware initialization (used for testing with memory clients).</param>
    public Tsl2591(I2cDevice device, ILogger<Tsl2591>? logger = null, bool skipInitialization = false)
        : this(new I2cRegisterClient(device, ownsDevice: false, postTransactionDelayMs: 0), ownsClient: false, logger, skipInitialization)
    {
    }

    /// <summary>
    /// Initializes a new instance using a pre-configured <see cref="II2cRegisterClient"/>.
    /// </summary>
    /// <param name="registerClient">The register client to use for I2C communication.</param>
    /// <param name="logger">Optional logger for diagnostics.</param>
    /// <param name="skipInitialization">If true, skip hardware initialization (used for testing with memory clients).</param>
    public Tsl2591(II2cRegisterClient registerClient, ILogger<Tsl2591>? logger = null, bool skipInitialization = false)
        : this(registerClient, ownsClient: false, logger, skipInitialization)
    {
    }

    private Tsl2591(II2cRegisterClient registerClient, bool ownsClient, ILogger<Tsl2591>? logger, bool skipInitialization)
        : base(registerClient, ownsClient)
    {
        _logger = logger ?? NullLogger<Tsl2591>.Instance;

        if (!skipInitialization)
        {
            Initialize();
        }

        _logger.LogDebug("TSL2591 initialized on bus {BusId}, address 0x{Address:X2}",
            ConnectionSettings.BusId, ConnectionSettings.DeviceAddress);
    }

    /// <inheritdoc />
    public Result<Tsl2591Luminosity> GetLuminosity(
        Tsl2591Gain gain = Tsl2591Gain.Low,
        Tsl2591IntegrationTime integrationTime = Tsl2591IntegrationTime.Ms100)
    {
        try
        {
            var (visible, ir) = ReadRawChannels(gain, integrationTime);
            var lux = CalculateLux(visible, ir, gain, integrationTime);

            var result = new Tsl2591Luminosity
            {
                Visible = visible,
                IR = ir,
                Lux = lux,
                Gain = gain
            };

            _logger.LogTrace("Luminosity reading: Visible={Visible}, IR={IR}, Lux={Lux:F2}, Gain={Gain}",
                visible, ir, lux, gain);

            return Result<Tsl2591Luminosity>.Success(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to read luminosity from TSL2591 at 0x{Address:X2}",
                ConnectionSettings.DeviceAddress);
            return Result<Tsl2591Luminosity>.Failure(ex);
        }
    }

    /// <inheritdoc />
    public Result<Tsl2591Luminosity> GetGainAdjustedLuminosity()
    {
        try
        {
            var gain = Tsl2591Gain.Low;
            var integrationTime = Tsl2591IntegrationTime.Ms200;

            var (visible, ir) = ReadRawChannels(gain, integrationTime);

            // Auto-range: step up gain if signal is too low
            if (visible * GainMultiplierMax < ushort.MaxValue)
            {
                gain = Tsl2591Gain.Max;
                (visible, ir) = ReadRawChannels(gain, integrationTime);

                if (visible == 0)
                {
                    // Light level is too low for any meaningful reading
                    _logger.LogDebug("TSL2591 light level too low for measurement at max gain");
                    return Result<Tsl2591Luminosity>.Success(new Tsl2591Luminosity { Gain = gain });
                }
            }
            else if (visible * GainMultiplierHigh < ushort.MaxValue)
            {
                gain = Tsl2591Gain.High;
                (visible, ir) = ReadRawChannels(gain, integrationTime);
            }
            else if (visible * GainMultiplierMedium < ushort.MaxValue)
            {
                gain = Tsl2591Gain.Medium;
                (visible, ir) = ReadRawChannels(gain, integrationTime);
            }

            var lux = CalculateLux(visible, ir, gain, integrationTime);

            // Normalize values back to 1× gain equivalent
            double normalizedVisible = visible;
            double normalizedIR = ir;
            var gainMultiplier = GetGainMultiplier(gain);

            if (gainMultiplier > 1.0)
            {
                normalizedVisible = visible / gainMultiplier;
                normalizedIR = ir / gainMultiplier;
            }

            var result = new Tsl2591Luminosity
            {
                Visible = normalizedVisible,
                IR = normalizedIR,
                Lux = lux,
                Gain = gain
            };

            _logger.LogTrace(
                "Gain-adjusted luminosity: Visible={Visible:F2}, IR={IR:F2}, Lux={Lux:F2}, Gain={Gain}",
                normalizedVisible, normalizedIR, lux, gain);

            return Result<Tsl2591Luminosity>.Success(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to read gain-adjusted luminosity from TSL2591 at 0x{Address:X2}",
                ConnectionSettings.DeviceAddress);
            return Result<Tsl2591Luminosity>.Failure(ex);
        }
    }

    /// <summary>
    /// Performs the initialization sequence for the TSL2591.
    /// Resets the device and validates the device ID register.
    /// </summary>
    private void Initialize()
    {
        lock (Sync)
        {
            // Reset the device to power-on defaults (Low gain, 100ms integration)
            WriteByte(ControlRegister, DeviceResetValue);

            // Validate device identity
            var deviceId = ReadByte(IdRegister);
            if (deviceId != ExpectedDeviceId)
            {
                throw new InvalidOperationException(
                    $"Device at 0x{ConnectionSettings.DeviceAddress:X2} is not recognized as TSL2591 (ID: 0x{deviceId:X2}, expected: 0x{ExpectedDeviceId:X2}).");
            }
        }
    }

    /// <summary>
    /// Reads the raw visible and IR channel data from the sensor at the specified gain and integration time.
    /// Handles enable/disable and waits for integration to complete.
    /// </summary>
    /// <param name="gain">The gain level to use.</param>
    /// <param name="integrationTime">The integration time to use.</param>
    /// <returns>A tuple of (visible, ir) raw ADC counts. Visible is full-spectrum minus IR.</returns>
    private (ushort Visible, ushort IR) ReadRawChannels(Tsl2591Gain gain, Tsl2591IntegrationTime integrationTime)
    {
        lock (Sync)
        {
            // Configure gain and integration time
            WriteByte(ControlRegister, (byte)(((byte)gain << 4) | (byte)integrationTime));

            try
            {
                // Enable the sensor (AEN + PON)
                WriteByte(EnableRegister, 0x03);

                try
                {
                    // Wait for integration to complete (108ms per step, with margin)
                    var waitMs = 120 * ((int)integrationTime + 1);
                    Thread.Sleep(waitMs);

                    // Read all 4 bytes: C0DATAL, C0DATAH, C1DATAL, C1DATAH
                    Span<byte> buffer = stackalloc byte[4];
                    ReadBlock(Channel0DataLow, buffer);

                    var fullSpectrum = (ushort)((buffer[1] << 8) | buffer[0]);
                    var ir = (ushort)((buffer[3] << 8) | buffer[2]);
                    var visible = (ushort)(fullSpectrum - ir);

                    return (visible, ir);
                }
                finally
                {
                    // Disable the sensor
                    WriteByte(EnableRegister, 0x00);
                }
            }
            finally
            {
                // Reset control register
                WriteByte(ControlRegister, 0x00);
            }
        }
    }

    /// <summary>
    /// Calculates lux from raw visible and IR readings at the given gain and integration time.
    /// Uses the TSL2591 application note formula with dual-channel coefficients.
    /// </summary>
    private static double CalculateLux(double visible, double ir, Tsl2591Gain gain, Tsl2591IntegrationTime integrationTime)
    {
        var t = GetIntegrationTimeMs(integrationTime);
        var g = GetGainMultiplier(gain);
        var countsPerLux = (t * g) / DefaultLuxPerCount;

        var lux1 = ((visible + ir) - (LuxCoefB * ir)) / countsPerLux;
        var lux2 = ((LuxCoefC * (visible + ir)) - (LuxCoefD * ir)) / countsPerLux;

        return Math.Max(lux1, lux2);
    }

    /// <summary>
    /// Returns the integration time in milliseconds for the given setting.
    /// </summary>
    private static double GetIntegrationTimeMs(Tsl2591IntegrationTime integrationTime)
    {
        return integrationTime switch
        {
            Tsl2591IntegrationTime.Ms100 => 100.0,
            Tsl2591IntegrationTime.Ms200 => 200.0,
            Tsl2591IntegrationTime.Ms300 => 300.0,
            Tsl2591IntegrationTime.Ms400 => 400.0,
            Tsl2591IntegrationTime.Ms500 => 500.0,
            Tsl2591IntegrationTime.Ms600 => 600.0,
            _ => 100.0
        };
    }

    /// <summary>
    /// Returns the gain multiplier for the given gain setting.
    /// </summary>
    private static double GetGainMultiplier(Tsl2591Gain gain)
    {
        return gain switch
        {
            Tsl2591Gain.Low => GainMultiplierLow,
            Tsl2591Gain.Medium => GainMultiplierMedium,
            Tsl2591Gain.High => GainMultiplierHigh,
            Tsl2591Gain.Max => GainMultiplierMax,
            _ => GainMultiplierLow
        };
    }
}
