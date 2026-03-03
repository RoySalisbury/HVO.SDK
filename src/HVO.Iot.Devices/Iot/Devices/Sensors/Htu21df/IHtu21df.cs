using System;
using HVO.Core.Results;

namespace HVO.Iot.Devices.Iot.Devices.Sensors.Htu21df;

/// <summary>
/// Abstraction for the HTU21D-F digital humidity and temperature sensor.
/// Enables dependency injection to swap between hardware-backed and simulated implementations.
/// </summary>
public interface IHtu21df : IDisposable
{
    /// <summary>
    /// Gets the temperature reading in degrees Celsius.
    /// </summary>
    Result<double> GetTemperature();

    /// <summary>
    /// Gets the relative humidity reading as a percentage (0–100%).
    /// </summary>
    Result<double> GetHumidity();
}
