using System;

namespace HVO.Weather
{
    /// <summary>
    /// Represents a temperature value with conversions between Fahrenheit, Celsius, Kelvin, and Rankine scales.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Internal storage uses Kelvin as the canonical unit. All other scales are computed on access.
    /// This is an immutable value type — use the static factory methods to create values.
    /// </para>
    /// </remarks>
    public readonly struct Temperature : IEquatable<Temperature>, IComparable<Temperature>
    {
        /// <summary>Absolute zero offset in Celsius (273.15 K).</summary>
        private const double AbsoluteTemperatureC = 273.15;

        /// <summary>Absolute zero offset in Fahrenheit (459.67 R).</summary>
        private const double AbsoluteTemperatureF = 459.67;

        /// <summary>Conversion multiplier between Kelvin and Fahrenheit/Rankine scales (9/5).</summary>
        private const double KelvinFahrenheitMultiplier = 9.0d / 5.0d;

        private readonly double _kelvin;

        private Temperature(double kelvin) => _kelvin = kelvin;

        /// <summary>
        /// Creates a <see cref="Temperature"/> from a value in degrees Fahrenheit.
        /// </summary>
        /// <param name="temperature">The temperature in degrees Fahrenheit.</param>
        /// <returns>A new <see cref="Temperature"/> instance.</returns>
        public static Temperature FromFahrenheit(double temperature)
            => new Temperature((temperature + AbsoluteTemperatureF) / KelvinFahrenheitMultiplier);

        /// <summary>
        /// Creates a <see cref="Temperature"/> from a value in degrees Fahrenheit.
        /// </summary>
        /// <param name="temperature">The temperature in degrees Fahrenheit.</param>
        /// <returns>A new <see cref="Temperature"/> instance.</returns>
        public static Temperature FromFahrenheit(decimal temperature)
            => FromFahrenheit((double)temperature);

        /// <summary>
        /// Creates a <see cref="Temperature"/> from a value in degrees Celsius.
        /// </summary>
        /// <param name="temperature">The temperature in degrees Celsius.</param>
        /// <returns>A new <see cref="Temperature"/> instance.</returns>
        public static Temperature FromCelsius(double temperature)
            => new Temperature(temperature + AbsoluteTemperatureC);

        /// <summary>
        /// Creates a <see cref="Temperature"/> from a value in degrees Celsius.
        /// </summary>
        /// <param name="temperature">The temperature in degrees Celsius.</param>
        /// <returns>A new <see cref="Temperature"/> instance.</returns>
        public static Temperature FromCelsius(decimal temperature)
            => FromCelsius((double)temperature);

        /// <summary>
        /// Creates a <see cref="Temperature"/> from a value in Kelvin.
        /// </summary>
        /// <param name="temperature">The temperature in Kelvin.</param>
        /// <returns>A new <see cref="Temperature"/> instance.</returns>
        public static Temperature FromKelvin(double temperature)
            => new Temperature(temperature);

        /// <summary>
        /// Creates a <see cref="Temperature"/> from a value in Kelvin.
        /// </summary>
        /// <param name="temperature">The temperature in Kelvin.</param>
        /// <returns>A new <see cref="Temperature"/> instance.</returns>
        public static Temperature FromKelvin(decimal temperature)
            => FromKelvin((double)temperature);

        /// <summary>
        /// Creates a <see cref="Temperature"/> from a value in degrees Rankine.
        /// </summary>
        /// <param name="temperature">The temperature in degrees Rankine.</param>
        /// <returns>A new <see cref="Temperature"/> instance.</returns>
        public static Temperature FromRankine(double temperature)
            => new Temperature(temperature / KelvinFahrenheitMultiplier);

        /// <summary>
        /// Creates a <see cref="Temperature"/> from a value in degrees Rankine.
        /// </summary>
        /// <param name="temperature">The temperature in degrees Rankine.</param>
        /// <returns>A new <see cref="Temperature"/> instance.</returns>
        public static Temperature FromRankine(decimal temperature)
            => FromRankine((double)temperature);

        /// <summary>
        /// Gets the temperature in degrees Fahrenheit.
        /// </summary>
        public double Fahrenheit => (_kelvin * KelvinFahrenheitMultiplier) - AbsoluteTemperatureF;

        /// <summary>
        /// Gets the temperature in degrees Celsius.
        /// </summary>
        public double Celsius => _kelvin - AbsoluteTemperatureC;

        /// <summary>
        /// Gets the temperature in Kelvin (canonical internal unit).
        /// </summary>
        public double Kelvin => _kelvin;

        /// <summary>
        /// Gets the temperature in degrees Rankine.
        /// </summary>
        public double Rankine => _kelvin * KelvinFahrenheitMultiplier;

        /// <inheritdoc />
        public bool Equals(Temperature other) => _kelvin.Equals(other._kelvin);

        /// <inheritdoc />
        public override bool Equals(object? obj) => obj is Temperature other && Equals(other);

        /// <inheritdoc />
        public override int GetHashCode() => _kelvin.GetHashCode();

        /// <inheritdoc />
        public int CompareTo(Temperature other) => _kelvin.CompareTo(other._kelvin);

        /// <inheritdoc />
        public override string ToString() => $"{Kelvin:F2} K";

        /// <summary>Equality operator</summary>
        public static bool operator ==(Temperature left, Temperature right) => left.Equals(right);

        /// <summary>Inequality operator</summary>
        public static bool operator !=(Temperature left, Temperature right) => !left.Equals(right);

        /// <summary>Less-than operator</summary>
        public static bool operator <(Temperature left, Temperature right) => left._kelvin < right._kelvin;

        /// <summary>Greater-than operator</summary>
        public static bool operator >(Temperature left, Temperature right) => left._kelvin > right._kelvin;

        /// <summary>Less-than-or-equal operator</summary>
        public static bool operator <=(Temperature left, Temperature right) => left._kelvin <= right._kelvin;

        /// <summary>Greater-than-or-equal operator</summary>
        public static bool operator >=(Temperature left, Temperature right) => left._kelvin >= right._kelvin;
    }
}
