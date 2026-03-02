using System;
using HVO.Core.Results;

namespace HVO.Iot.Devices.Iot.Devices.Sensors.Tsl2591;

/// <summary>
/// Sensor gain levels for the TSL2591.
/// </summary>
public enum Tsl2591Gain
{
    /// <summary>1× gain.</summary>
    Low = 0,
    /// <summary>25× gain.</summary>
    Medium = 1,
    /// <summary>428× gain.</summary>
    High = 2,
    /// <summary>9876× gain.</summary>
    Max = 3
}

/// <summary>
/// Integration time settings for the TSL2591.
/// </summary>
public enum Tsl2591IntegrationTime
{
    /// <summary>100 ms integration time.</summary>
    Ms100 = 0,
    /// <summary>200 ms integration time.</summary>
    Ms200 = 1,
    /// <summary>300 ms integration time.</summary>
    Ms300 = 2,
    /// <summary>400 ms integration time.</summary>
    Ms400 = 3,
    /// <summary>500 ms integration time.</summary>
    Ms500 = 4,
    /// <summary>600 ms integration time.</summary>
    Ms600 = 5
}

/// <summary>
/// Contains the results of a TSL2591 luminosity reading.
/// </summary>
public readonly struct Tsl2591Luminosity
{
    /// <summary>
    /// Gets the visible light component (full spectrum minus IR).
    /// For readings produced by <see cref="ITsl2591.GetLuminosity(Tsl2591Gain, Tsl2591IntegrationTime)"/>,
    /// this is the raw sensor value at the configured gain. For readings produced by
    /// <see cref="ITsl2591.GetGainAdjustedLuminosity"/>, this value is gain-normalized to the
    /// equivalent of 1× gain.
    /// </summary>
    public double Visible { get; init; }

    /// <summary>
    /// Gets the infrared light component.
    /// For readings produced by <see cref="ITsl2591.GetLuminosity(Tsl2591Gain, Tsl2591IntegrationTime)"/>,
    /// this is the raw sensor value at the configured gain. For readings produced by
    /// <see cref="ITsl2591.GetGainAdjustedLuminosity"/>, this value is gain-normalized to the
    /// equivalent of 1× gain.
    /// </summary>
    public double IR { get; init; }

    /// <summary>
    /// Gets the calculated lux value.
    /// </summary>
    public double Lux { get; init; }

    /// <summary>
    /// Gets the gain level used for this reading.
    /// </summary>
    public Tsl2591Gain Gain { get; init; }
}

/// <summary>
/// Abstraction for the TSL2591 high-dynamic-range digital light sensor.
/// Enables dependency injection to swap between hardware-backed and simulated implementations.
/// </summary>
public interface ITsl2591 : IDisposable
{
    /// <summary>
    /// Reads luminosity at a specific gain and integration time.
    /// </summary>
    /// <param name="gain">The sensor gain level.</param>
    /// <param name="integrationTime">The integration time.</param>
    /// <returns>A <see cref="Tsl2591Luminosity"/> with visible, IR, and lux values.</returns>
    Result<Tsl2591Luminosity> GetLuminosity(
        Tsl2591Gain gain = Tsl2591Gain.Low,
        Tsl2591IntegrationTime integrationTime = Tsl2591IntegrationTime.Ms100);

    /// <summary>
    /// Reads luminosity with automatic gain ranging for optimal sensitivity and range.
    /// Starts at low gain and steps up (Medium → High → Max) until the signal fills
    /// the ADC range without saturating.
    /// </summary>
    /// <returns>A <see cref="Tsl2591Luminosity"/> with gain-normalized visible, IR, and lux values.</returns>
    Result<Tsl2591Luminosity> GetGainAdjustedLuminosity();
}
