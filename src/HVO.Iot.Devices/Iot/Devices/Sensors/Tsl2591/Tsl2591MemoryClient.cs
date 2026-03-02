using HVO.Iot.Devices.Implementation;

namespace HVO.Iot.Devices.Iot.Devices.Sensors.Tsl2591;

/// <summary>
/// In-memory simulation of the TSL2591 light sensor for unit testing.
/// Pre-populates the device ID register and provides setters for raw channel data.
/// </summary>
/// <remarks>
/// <para>
/// The TSL2591 uses a command-based register addressing scheme where register addresses
/// are OR'd with 0xA0 (Command | NormalOp). This memory client stores data at the
/// effective register addresses so the driver's reads work correctly.
/// </para>
/// <para>
/// Use <see cref="SetChannelData"/> to configure the full-spectrum and IR raw values
/// that will be returned when the driver reads channel data.
/// </para>
/// </remarks>
public class Tsl2591MemoryClient : MemoryI2cRegisterClient
{
    // Register addresses as the driver sees them (with command bits)
    private const byte IdRegister = 0x80 | 0x20 | 0x12;        // 0xB2
    private const byte Channel0DataLow = 0x80 | 0x20 | 0x14;   // 0xB4

    /// <summary>
    /// Initializes a new memory-backed TSL2591 client with the device ID pre-loaded.
    /// </summary>
    /// <param name="busId">Simulated bus ID.</param>
    /// <param name="address">Simulated device address (default 0x29).</param>
    public Tsl2591MemoryClient(int busId = 1, int address = Tsl2591.DefaultI2cAddress)
        : base(busId, address, registerCount: 256)
    {
        // Pre-load the device ID so initialization validation passes
        RegisterSpan[IdRegister] = Tsl2591.ExpectedDeviceId;

        // Set reasonable default channel data (~100 lux)
        SetChannelData(fullSpectrum: 500, ir: 100);
    }

    /// <summary>
    /// Sets the simulated raw channel data.
    /// </summary>
    /// <param name="fullSpectrum">Full-spectrum (visible + IR) raw ADC count.</param>
    /// <param name="ir">IR-only channel raw ADC count.</param>
    public void SetChannelData(ushort fullSpectrum, ushort ir)
    {
        RegisterSpan[Channel0DataLow] = (byte)(fullSpectrum & 0xFF);
        RegisterSpan[Channel0DataLow + 1] = (byte)((fullSpectrum >> 8) & 0xFF);
        RegisterSpan[Channel0DataLow + 2] = (byte)(ir & 0xFF);
        RegisterSpan[Channel0DataLow + 3] = (byte)((ir >> 8) & 0xFF);
    }
}
