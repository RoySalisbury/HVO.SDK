using System;
using HVO.Core.Results;

namespace HVO.Iot.Devices.Iot.Devices.Sensors.Mlx90614;

/// <summary>
/// Abstraction for the MLX90614 non-contact infrared temperature sensor.
/// Enables dependency injection to swap between hardware-backed and simulated implementations.
/// </summary>
public interface IMlx90614 : IDisposable
{
    /// <summary>
    /// Gets the ambient (die) temperature of the sensor in degrees Celsius.
    /// </summary>
    Result<double> GetAmbientTemperature();

    /// <summary>
    /// Gets the measured object (IR target) temperature in degrees Celsius.
    /// </summary>
    Result<double> GetObjectTemperature();

    /// <summary>
    /// Gets the raw infrared reading from channel 1.
    /// </summary>
    Result<ushort> GetRawIR1();

    /// <summary>
    /// Gets the raw infrared reading from channel 2.
    /// </summary>
    Result<ushort> GetRawIR2();
}
