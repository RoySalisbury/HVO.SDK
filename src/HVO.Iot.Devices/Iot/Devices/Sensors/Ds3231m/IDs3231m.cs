using System;
using HVO.Core.Results;

namespace HVO.Iot.Devices.Iot.Devices.Sensors.Ds3231m;

/// <summary>
/// Abstraction for the DS3231M real-time clock (RTC) module.
/// Enables dependency injection to swap between hardware-backed and simulated implementations.
/// </summary>
public interface IDs3231m : IDisposable
{
    /// <summary>
    /// Gets the current date and time from the RTC.
    /// </summary>
    Result<DateTimeOffset> GetDateTime();

    /// <summary>
    /// Sets the date and time on the RTC.
    /// </summary>
    /// <param name="value">The date/time to set (will be converted to UTC).</param>
    Result<bool> SetDateTime(DateTimeOffset value);

    /// <summary>
    /// Gets the on-chip temperature in degrees Celsius.
    /// The DS3231M includes a built-in temperature sensor with 0.25°C resolution.
    /// </summary>
    Result<double> GetTemperature();

    /// <summary>
    /// Gets a value indicating whether the oscillator stop flag (OSF) is set,
    /// meaning the clock lost power and the time may be invalid.
    /// </summary>
    Result<bool> GetOscillatorStopFlag();

    /// <summary>
    /// Clears the oscillator stop flag (OSF) after the time has been re-set.
    /// </summary>
    Result<bool> ClearOscillatorStopFlag();
}
