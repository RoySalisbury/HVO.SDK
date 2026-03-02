using System;
using System.Collections.Generic;
using HVO.Iot.Devices.Implementation;

namespace HVO.Iot.Devices.Iot.Devices.Sensors.Mlx90614;

/// <summary>
/// In-memory simulation of the MLX90614 IR temperature sensor for unit testing.
/// Provides settable ambient and object temperatures that are stored in the simulated register array.
/// </summary>
/// <remarks>
/// The MLX90614 uses SMBus-style reads where each register command returns an independent 3-byte
/// response (LSB, MSB, PEC). Because adjacent register addresses (0x04–0x07) each produce 3 bytes,
/// a flat memory array would cause overlapping writes. This client stores per-register data in a
/// dictionary and overrides <see cref="OnRead"/> to return the correct block for each register.
/// </remarks>
public class Mlx90614MemoryClient : MemoryI2cRegisterClient
{
    private const byte AmbientTempRegister = 0x06;
    private const byte ObjectTemp1Register = 0x07;
    private const byte RawIR1Register = 0x04;
    private const byte RawIR2Register = 0x05;

    /// <summary>
    /// Per-register data blocks. Each entry stores the 3-byte SMBus response (LSB, MSB, PEC).
    /// </summary>
    private readonly Dictionary<byte, byte[]> _registerData = new();

    /// <summary>
    /// Initializes a new memory-backed MLX90614 client.
    /// </summary>
    /// <param name="busId">Simulated bus ID.</param>
    /// <param name="address">Simulated device address (default 0x5A).</param>
    public Mlx90614MemoryClient(int busId = 1, int address = Mlx90614.DefaultI2cAddress)
        : base(busId, address, registerCount: 256)
    {
        // Set default temperatures: ~25°C ambient, ~25°C object
        SetTemperature(AmbientTempRegister, 25.0);
        SetTemperature(ObjectTemp1Register, 25.0);
    }

    /// <summary>
    /// Sets the simulated ambient temperature in degrees Celsius.
    /// </summary>
    /// <param name="celsius">Temperature in degrees Celsius.</param>
    public void SetAmbientTemperature(double celsius) => SetTemperature(AmbientTempRegister, celsius);

    /// <summary>
    /// Sets the simulated object temperature in degrees Celsius.
    /// </summary>
    /// <param name="celsius">Temperature in degrees Celsius.</param>
    public void SetObjectTemperature(double celsius) => SetTemperature(ObjectTemp1Register, celsius);

    /// <summary>
    /// Sets the simulated raw IR1 value.
    /// </summary>
    /// <param name="value">Raw 16-bit IR value.</param>
    public void SetRawIR1(ushort value)
    {
        _registerData[RawIR1Register] = new byte[]
        {
            (byte)(value & 0xFF),
            (byte)((value >> 8) & 0xFF),
            0 // PEC byte (not validated)
        };
    }

    /// <summary>
    /// Sets the simulated raw IR2 value.
    /// </summary>
    /// <param name="value">Raw 16-bit IR value.</param>
    public void SetRawIR2(ushort value)
    {
        _registerData[RawIR2Register] = new byte[]
        {
            (byte)(value & 0xFF),
            (byte)((value >> 8) & 0xFF),
            0 // PEC byte (not validated)
        };
    }

    /// <inheritdoc />
    protected override ReadOnlyMemory<byte> OnRead(byte register, int length)
    {
        if (_registerData.TryGetValue(register, out var data))
        {
            // Return up to the requested length from the per-register data
            var result = new byte[length];
            var copyLen = Math.Min(length, data.Length);
            Array.Copy(data, 0, result, 0, copyLen);
            return result;
        }

        // Fall back to base flat-memory behaviour for any unregistered addresses
        return base.OnRead(register, length);
    }

    /// <summary>
    /// Converts a Celsius temperature to MLX90614 raw register format and stores it.
    /// </summary>
    private void SetTemperature(byte register, double celsius)
    {
        // MLX90614 format: raw = (Kelvin / 0.02), stored as 16-bit LE + PEC byte
        var kelvin = celsius + 273.15;
        var raw = (ushort)(kelvin / 0.02);

        _registerData[register] = new byte[]
        {
            (byte)(raw & 0xFF),
            (byte)((raw >> 8) & 0x7F), // Bit 15 is error flag, keep clear
            0 // PEC byte (not validated in driver)
        };
    }
}
