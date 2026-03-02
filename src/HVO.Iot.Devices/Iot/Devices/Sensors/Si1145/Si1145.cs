using System;
using System.Device.I2c;
using System.Threading;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using HVO.Core.Results;
using HVO.Iot.Devices.Abstractions;
using HVO.Iot.Devices.Implementation;
using HVO.Iot.Devices.Iot.Devices.Common;

namespace HVO.Iot.Devices.Iot.Devices.Sensors.Si1145;

/// <summary>
/// Driver for the Silicon Labs SI1145 UV/Visible/IR light and proximity sensor.
/// Communicates via I2C to read ambient visible light, infrared, UV index, and proximity data.
/// </summary>
/// <remarks>
/// <para>
/// The SI1145 is a low-power, reflectance-based UV index, ambient light, infrared, and proximity sensor
/// with an I2C digital interface and programmable LED driver. It can measure UV index directly
/// using a calibrated UV-response photodiode.
/// </para>
/// <para>
/// During initialization, the sensor is configured for automatic measurement of UV, visible light,
/// infrared, and proximity sensor 1 (PS1) channels. The measurement rate is set to auto mode.
/// </para>
/// <para>
/// Default I2C address: 0x60. Migrated from HVOv6 / nF.Devices legacy implementations.
/// </para>
/// </remarks>
public class Si1145 : RegisterBasedI2cDevice, ISi1145
{
    /// <summary>
    /// Default I2C hardware address for the SI1145.
    /// </summary>
    public const int DefaultI2cAddress = 0x60;

    /// <summary>
    /// Expected part ID value read from the SI1145_REG_PARTID register.
    /// </summary>
    public const byte ExpectedPartId = 0x45;

    #region Commands

    private const byte ParamSet = 0xA0;
    private const byte CommandReset = 0x01;
    private const byte CommandPsAlsAuto = 0x0F;

    #endregion

    #region Parameters

    private const byte ParamChList = 0x01;
    private const byte ParamChListEnUv = 0x80;
    private const byte ParamChListEnAlsIr = 0x20;
    private const byte ParamChListEnAlsVis = 0x10;
    private const byte ParamChListEnPs1 = 0x01;

    private const byte ParamPsLed12Sel = 0x02;
    private const byte ParamPsLed12SelPs1Led1 = 0x01;

    private const byte ParamPs1AdcMux = 0x07;
    private const byte ParamPsAdcGain = 0x0B;
    private const byte ParamPsAdcCounter = 0x0A;
    private const byte ParamPsAdcMisc = 0x0C;
    private const byte ParamPsAdcMiscRange = 0x20;
    private const byte ParamPsAdcMiscPsMode = 0x04;

    private const byte ParamAlsIrAdcMux = 0x0E;
    private const byte ParamAlsIrAdcGain = 0x1E;
    private const byte ParamAlsIrAdcCounter = 0x1D;
    private const byte ParamAlsIrAdcMisc = 0x1F;
    private const byte ParamAlsIrAdcMiscRange = 0x20;

    private const byte ParamAlsVisAdcGain = 0x11;
    private const byte ParamAlsVisAdcCounter = 0x10;
    private const byte ParamAlsVisAdcMisc = 0x12;
    private const byte ParamAlsVisAdcMiscVisRange = 0x20;

    private const byte ParamAdcCounter511Clk = 0x70;
    private const byte ParamAdcMuxSmallIr = 0x00;
    private const byte ParamAdcMuxLargeIr = 0x03;

    #endregion

    #region Registers

    private const byte RegPartId = 0x00;
    private const byte RegIntCfg = 0x03;
    private const byte RegIntCfgIntOe = 0x01;
    private const byte RegIrqEn = 0x04;
    private const byte RegIrqEnAlsEverySample = 0x01;
    private const byte RegIrqMode1 = 0x05;
    private const byte RegIrqMode2 = 0x06;
    private const byte RegHwKey = 0x07;
    private const byte RegMeasRate0 = 0x08;
    private const byte RegMeasRate1 = 0x09;
    private const byte RegPsLed21 = 0x0F;
    private const byte RegUCoeff0 = 0x13;
    private const byte RegUCoeff1 = 0x14;
    private const byte RegUCoeff2 = 0x15;
    private const byte RegUCoeff3 = 0x16;
    private const byte RegParamWr = 0x17;
    private const byte RegCommand = 0x18;
    private const byte RegIrqStat = 0x21;
    private const byte RegAlsVisData0 = 0x22;
    private const byte RegAlsIrData0 = 0x24;
    private const byte RegPs1Data0 = 0x26;
    private const byte RegUvIndex0 = 0x2C;
    private const byte RegParamRd = 0x2E;

    #endregion

    private readonly ILogger<Si1145> _logger;

    /// <summary>
    /// Initializes a new instance using a bus number and I2C address.
    /// The sensor is fully initialized with auto-measurement mode enabled.
    /// </summary>
    /// <param name="i2cBus">The I2C bus number (typically 1 on Raspberry Pi).</param>
    /// <param name="i2cAddress">The I2C address of the sensor (default 0x60).</param>
    /// <param name="logger">Optional logger for diagnostics.</param>
    /// <param name="skipInitialization">If true, skip hardware initialization (used for testing with memory clients).</param>
    public Si1145(int i2cBus = 1, int i2cAddress = DefaultI2cAddress, ILogger<Si1145>? logger = null, bool skipInitialization = false)
        : this(new I2cRegisterClient(i2cBus, i2cAddress, postTransactionDelayMs: 0), ownsClient: true, logger, skipInitialization)
    {
    }

    /// <summary>
    /// Initializes a new instance from an existing <see cref="I2cDevice"/>.
    /// </summary>
    /// <param name="device">An existing I2C device instance.</param>
    /// <param name="logger">Optional logger for diagnostics.</param>
    /// <param name="skipInitialization">If true, skip hardware initialization (used for testing with memory clients).</param>
    public Si1145(I2cDevice device, ILogger<Si1145>? logger = null, bool skipInitialization = false)
        : this(new I2cRegisterClient(device, ownsDevice: false, postTransactionDelayMs: 0), ownsClient: false, logger, skipInitialization)
    {
    }

    /// <summary>
    /// Initializes a new instance using a pre-configured <see cref="II2cRegisterClient"/>.
    /// </summary>
    /// <param name="registerClient">The register client to use for I2C communication.</param>
    /// <param name="logger">Optional logger for diagnostics.</param>
    /// <param name="skipInitialization">If true, skip hardware initialization (used for testing with memory clients).</param>
    public Si1145(II2cRegisterClient registerClient, ILogger<Si1145>? logger = null, bool skipInitialization = false)
        : this(registerClient, ownsClient: false, logger, skipInitialization)
    {
    }

    private Si1145(II2cRegisterClient registerClient, bool ownsClient, ILogger<Si1145>? logger, bool skipInitialization)
        : base(registerClient, ownsClient)
    {
        _logger = logger ?? NullLogger<Si1145>.Instance;

        if (!skipInitialization)
        {
            Initialize();
        }

        _logger.LogDebug("SI1145 initialized on bus {BusId}, address 0x{Address:X2}",
            ConnectionSettings.BusId, ConnectionSettings.DeviceAddress);
    }

    /// <inheritdoc />
    public Result<ushort> ReadIR()
    {
        try
        {
            lock (Sync)
            {
                var value = ReadUInt16(RegAlsIrData0);
                _logger.LogTrace("IR reading: {Value}", value);
                return Result<ushort>.Success(value);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to read IR from SI1145 at 0x{Address:X2}",
                ConnectionSettings.DeviceAddress);
            return Result<ushort>.Failure(ex);
        }
    }

    /// <inheritdoc />
    public Result<ushort> ReadVisible()
    {
        try
        {
            lock (Sync)
            {
                var value = ReadUInt16(RegAlsVisData0);
                _logger.LogTrace("Visible reading: {Value}", value);
                return Result<ushort>.Success(value);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to read visible light from SI1145 at 0x{Address:X2}",
                ConnectionSettings.DeviceAddress);
            return Result<ushort>.Failure(ex);
        }
    }

    /// <inheritdoc />
    public Result<float> ReadUV()
    {
        try
        {
            lock (Sync)
            {
                var raw = ReadUInt16(RegUvIndex0);
                var uvIndex = raw / 100.0f;
                _logger.LogTrace("UV index: {UvIndex:F2} (raw: {Raw})", uvIndex, raw);
                return Result<float>.Success(uvIndex);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to read UV index from SI1145 at 0x{Address:X2}",
                ConnectionSettings.DeviceAddress);
            return Result<float>.Failure(ex);
        }
    }

    /// <inheritdoc />
    public Result<ushort> ReadProximity()
    {
        try
        {
            lock (Sync)
            {
                var value = ReadUInt16(RegPs1Data0);
                _logger.LogTrace("Proximity reading: {Value}", value);
                return Result<ushort>.Success(value);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to read proximity from SI1145 at 0x{Address:X2}",
                ConnectionSettings.DeviceAddress);
            return Result<ushort>.Failure(ex);
        }
    }

    /// <summary>
    /// Performs the full initialization sequence for the SI1145.
    /// Validates the part ID, resets the device, configures UV coefficients,
    /// enables all measurement channels, and starts auto-measurement mode.
    /// </summary>
    private void Initialize()
    {
        lock (Sync)
        {
            // Verify part ID
            var partId = ReadByte(RegPartId);
            if (partId != ExpectedPartId)
            {
                throw new InvalidOperationException(
                    $"Device at 0x{ConnectionSettings.DeviceAddress:X2} is not recognized as SI1145 (part ID: 0x{partId:X2}, expected: 0x{ExpectedPartId:X2}).");
            }

            // Power-on reset sequence
            WriteByte(RegMeasRate0, 0);
            WriteByte(RegMeasRate1, 0);
            WriteByte(RegIrqEn, 0);
            WriteByte(RegIrqMode1, 0);
            WriteByte(RegIrqMode2, 0);
            WriteByte(RegIntCfg, 0);
            WriteByte(RegIrqStat, 0xFF);
            WriteByte(RegCommand, CommandReset);
            Thread.Sleep(10);

            WriteByte(RegHwKey, 0x17);
            Thread.Sleep(10);

            // UV index measurement coefficients
            WriteByte(RegUCoeff0, 0x29);
            WriteByte(RegUCoeff1, 0x89);
            WriteByte(RegUCoeff2, 0x02);
            WriteByte(RegUCoeff3, 0x00);

            // Enable UV, IR, Visible, and PS1 channels
            WriteParam(ParamChList,
                ParamChListEnUv | ParamChListEnAlsIr | ParamChListEnAlsVis | ParamChListEnPs1);

            // Enable interrupt on every sample
            WriteByte(RegIntCfg, RegIntCfgIntOe);
            WriteByte(RegIrqEn, RegIrqEnAlsEverySample);

            // Proximity Sense 1 configuration
            WriteByte(RegPsLed21, 0x03); // 20mA for LED 1 only
            WriteParam(ParamPs1AdcMux, ParamAdcMuxLargeIr);
            WriteParam(ParamPsLed12Sel, ParamPsLed12SelPs1Led1);
            WriteParam(ParamPsAdcGain, 0);
            WriteParam(ParamPsAdcCounter, ParamAdcCounter511Clk);
            WriteParam(ParamPsAdcMisc, ParamPsAdcMiscRange | ParamPsAdcMiscPsMode);

            // ALS IR configuration
            WriteParam(ParamAlsIrAdcMux, ParamAdcMuxSmallIr);
            WriteParam(ParamAlsIrAdcGain, 0);
            WriteParam(ParamAlsIrAdcCounter, ParamAdcCounter511Clk);
            WriteParam(ParamAlsIrAdcMisc, ParamAlsIrAdcMiscRange);

            // ALS Visible configuration
            WriteParam(ParamAlsVisAdcGain, 0);
            WriteParam(ParamAlsVisAdcCounter, ParamAdcCounter511Clk);
            WriteParam(ParamAlsVisAdcMisc, ParamAlsVisAdcMiscVisRange);

            // Auto measurement rate: 255 × 31.25µs ≈ 8ms
            WriteByte(RegMeasRate0, 0xFF);

            // Start auto-measurement mode
            WriteByte(RegCommand, CommandPsAlsAuto);
        }
    }

    /// <summary>
    /// Writes a value to the SI1145 parameter table using the PARAM_SET command protocol.
    /// </summary>
    /// <param name="parameter">The parameter address to write.</param>
    /// <param name="value">The value to write to the parameter.</param>
    private void WriteParam(byte parameter, byte value)
    {
        WriteByte(RegParamWr, value);
        WriteByte(RegCommand, (byte)(parameter | ParamSet));
        ReadByte(RegParamRd); // Confirm write
    }
}
