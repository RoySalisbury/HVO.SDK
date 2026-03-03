using System;
using System.Collections.Generic;
using HVO.Iot.Devices.Implementation;

namespace HVO.Iot.Devices.Iot.Devices.Sensors.Htu21df;

/// <summary>
/// In-memory simulation of the HTU21D-F humidity/temperature sensor for unit testing.
/// Provides settable temperature and humidity values stored in a per-register dictionary.
/// </summary>
/// <remarks>
/// The HTU21D-F uses command-based reads where each command triggers a measurement and returns
/// 3 bytes (MSB, LSB, checksum). This client stores per-command data and overrides
/// <see cref="OnRead"/> to return the correct block for each command byte.
/// </remarks>
public class Htu21dfMemoryClient : MemoryI2cRegisterClient
{
    private const byte ReadTempCommand = 0xE3;
    private const byte ReadHumidityCommand = 0xE5;

    /// <summary>
    /// Per-command data blocks. Each entry stores the 3-byte response (MSB, LSB, checksum).
    /// </summary>
    private readonly Dictionary<byte, byte[]> _commandData = new();

    /// <summary>
    /// Initializes a new memory-backed HTU21D-F client with default values (~25°C, ~50% RH).
    /// </summary>
    /// <param name="busId">Simulated bus ID.</param>
    /// <param name="address">Simulated device address (default 0x40).</param>
    public Htu21dfMemoryClient(int busId = 1, int address = Htu21df.DefaultI2cAddress)
        : base(busId, address, registerCount: 256)
    {
        SetTemperature(25.0);
        SetHumidity(50.0);
    }

    /// <summary>
    /// Sets the simulated temperature in degrees Celsius.
    /// </summary>
    /// <param name="celsius">Temperature in degrees Celsius.</param>
    public void SetTemperature(double celsius)
    {
        // Reverse the formula: celsius = (raw / 65536.0) * 175.72 - 46.85
        // raw = (celsius + 46.85) / 175.72 * 65536.0
        var raw = (int)((celsius + 46.85) / 175.72 * 65536.0);
        raw = Math.Max(0, Math.Min(raw, 0xFFFF));

        _commandData[ReadTempCommand] = new byte[]
        {
            (byte)((raw >> 8) & 0xFF),
            (byte)(raw & 0xFF),
            0 // checksum (not validated)
        };
    }

    /// <summary>
    /// Sets the simulated relative humidity as a percentage.
    /// </summary>
    /// <param name="percent">Relative humidity (0–100%).</param>
    public void SetHumidity(double percent)
    {
        // Reverse the formula: percent = (raw / 65536.0) * 125.0 - 6.0
        // raw = (percent + 6.0) / 125.0 * 65536.0
        var raw = (int)((percent + 6.0) / 125.0 * 65536.0);
        raw = Math.Max(0, Math.Min(raw, 0xFFFF));

        _commandData[ReadHumidityCommand] = new byte[]
        {
            (byte)((raw >> 8) & 0xFF),
            (byte)(raw & 0xFF),
            0 // checksum (not validated)
        };
    }

    /// <inheritdoc />
    protected override ReadOnlyMemory<byte> OnRead(byte register, int length)
    {
        if (_commandData.TryGetValue(register, out var data))
        {
            // Return the stored command response (possibly truncated to requested length)
            var result = new byte[length];
            Array.Copy(data, result, Math.Min(data.Length, length));
            return result;
        }

        return base.OnRead(register, length);
    }
}
