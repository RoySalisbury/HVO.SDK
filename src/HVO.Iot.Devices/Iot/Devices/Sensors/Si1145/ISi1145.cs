using System;
using HVO.Core.Results;

namespace HVO.Iot.Devices.Iot.Devices.Sensors.Si1145;

/// <summary>
/// Abstraction for the SI1145 UV/Visible/IR light and proximity sensor.
/// Enables dependency injection to swap between hardware-backed and simulated implementations.
/// </summary>
public interface ISi1145 : IDisposable
{
    /// <summary>
    /// Gets the current infrared light reading.
    /// </summary>
    Result<ushort> ReadIR();

    /// <summary>
    /// Gets the current visible light reading.
    /// </summary>
    Result<ushort> ReadVisible();

    /// <summary>
    /// Gets the current UV index value (scaled by 100 from the sensor, returned as a float).
    /// </summary>
    Result<float> ReadUV();

    /// <summary>
    /// Gets the current proximity sensor reading.
    /// </summary>
    Result<ushort> ReadProximity();
}
