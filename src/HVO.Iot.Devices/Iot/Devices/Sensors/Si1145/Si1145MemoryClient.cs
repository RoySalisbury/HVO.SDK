using System;
using HVO.Iot.Devices.Implementation;

namespace HVO.Iot.Devices.Iot.Devices.Sensors.Si1145;

/// <summary>
/// In-memory simulation of the SI1145 UV/Visible/IR sensor for unit testing.
/// Pre-populates the part ID register and provides setters for all readable data registers.
/// </summary>
public class Si1145MemoryClient : MemoryI2cRegisterClient
{
    private const byte RegPartId = 0x00;
    private const byte RegAlsVisData0 = 0x22;
    private const byte RegAlsIrData0 = 0x24;
    private const byte RegPs1Data0 = 0x26;
    private const byte RegUvIndex0 = 0x2C;

    /// <summary>
    /// Initializes a new memory-backed SI1145 client with the correct part ID pre-loaded.
    /// </summary>
    /// <param name="busId">Simulated bus ID.</param>
    /// <param name="address">Simulated device address (default 0x60).</param>
    public Si1145MemoryClient(int busId = 1, int address = Si1145.DefaultI2cAddress)
        : base(busId, address, registerCount: 256)
    {
        // Set the expected part ID so initialization validation passes
        RegisterSpan[RegPartId] = Si1145.ExpectedPartId;

        // Set reasonable defaults
        SetVisible(260);
        SetIR(250);
        SetUVIndex(0.10f);
        SetProximity(240);
    }

    /// <summary>
    /// Sets the simulated visible light value.
    /// </summary>
    /// <param name="value">Raw 16-bit visible light reading.</param>
    public void SetVisible(ushort value)
    {
        RegisterSpan[RegAlsVisData0] = (byte)(value & 0xFF);
        RegisterSpan[RegAlsVisData0 + 1] = (byte)((value >> 8) & 0xFF);
    }

    /// <summary>
    /// Sets the simulated infrared light value.
    /// </summary>
    /// <param name="value">Raw 16-bit IR reading.</param>
    public void SetIR(ushort value)
    {
        RegisterSpan[RegAlsIrData0] = (byte)(value & 0xFF);
        RegisterSpan[RegAlsIrData0 + 1] = (byte)((value >> 8) & 0xFF);
    }

    /// <summary>
    /// Sets the simulated UV index value.
    /// </summary>
    /// <param name="uvIndex">The UV index value (will be stored as raw × 100).</param>
    public void SetUVIndex(float uvIndex)
    {
        var raw = (ushort)(uvIndex * 100);
        RegisterSpan[RegUvIndex0] = (byte)(raw & 0xFF);
        RegisterSpan[RegUvIndex0 + 1] = (byte)((raw >> 8) & 0xFF);
    }

    /// <summary>
    /// Sets the simulated proximity sensor value.
    /// </summary>
    /// <param name="value">Raw 16-bit proximity reading.</param>
    public void SetProximity(ushort value)
    {
        RegisterSpan[RegPs1Data0] = (byte)(value & 0xFF);
        RegisterSpan[RegPs1Data0 + 1] = (byte)((value >> 8) & 0xFF);
    }
}
