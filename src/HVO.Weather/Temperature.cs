using System;

namespace HVO.Weather
{
    /// <summary>
    /// Represents a temperature value with conversions between Fahrenheit, Celsius, Kelvin, and Rankine scales.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Internal storage uses Kelvin as the canonical unit. All other scales are computed on access.
    /// Instances are immutable — use the static factory methods to create values.
    /// </para>
    /// </remarks>
    public sealed class Temperature
    {
        /// <summary>Absolute zero offset in Celsius (273.15 K).</summary>
        private const double AbsoluteTemperatureC = 273.15;

        /// <summary>Absolute zero offset in Fahrenheit (459.67 R).</summary>
        private const double AbsoluteTemperatureF = 459.67;

        /// <summary>Conversion multiplier between Kelvin and Fahrenheit/Rankine scales (9/5).</summary>
        private const double KelvinFahrenheitMultiplier = 9.0d / 5.0d;

        private Temperature() { }

        /// <summary>
        /// Creates a <see cref="Temperature"/> from a value in degrees Fahrenheit.
        /// </summary>
        /// <param name="temperature">The temperature in degrees Fahrenheit.</param>
        /// <returns>A new <see cref="Temperature"/> instance.</returns>
        public static Temperature FromFahrenheit(double temperature)
        {
            return new Temperature() { Fahrenheit = temperature };
        }

        /// <summary>
        /// Creates a <see cref="Temperature"/> from a value in degrees Fahrenheit.
        /// </summary>
        /// <param name="temperature">The temperature in degrees Fahrenheit.</param>
        /// <returns>A new <see cref="Temperature"/> instance.</returns>
        public static Temperature FromFahrenheit(decimal temperature)
        {
            return new Temperature() { Fahrenheit = (double)temperature };
        }

        /// <summary>
        /// Creates a <see cref="Temperature"/> from a value in degrees Celsius.
        /// </summary>
        /// <param name="temperature">The temperature in degrees Celsius.</param>
        /// <returns>A new <see cref="Temperature"/> instance.</returns>
        public static Temperature FromCelsius(double temperature)
        {
            return new Temperature() { Celsius = temperature };
        }

        /// <summary>
        /// Creates a <see cref="Temperature"/> from a value in degrees Celsius.
        /// </summary>
        /// <param name="temperature">The temperature in degrees Celsius.</param>
        /// <returns>A new <see cref="Temperature"/> instance.</returns>
        public static Temperature FromCelsius(decimal temperature)
        {
            return new Temperature() { Celsius = (double)temperature };
        }

        /// <summary>
        /// Creates a <see cref="Temperature"/> from a value in Kelvin.
        /// </summary>
        /// <param name="temperature">The temperature in Kelvin.</param>
        /// <returns>A new <see cref="Temperature"/> instance.</returns>
        public static Temperature FromKelvin(double temperature)
        {
            return new Temperature() { Kelvin = temperature };
        }

        /// <summary>
        /// Creates a <see cref="Temperature"/> from a value in Kelvin.
        /// </summary>
        /// <param name="temperature">The temperature in Kelvin.</param>
        /// <returns>A new <see cref="Temperature"/> instance.</returns>
        public static Temperature FromKelvin(decimal temperature)
        {
            return new Temperature() { Kelvin = (double)temperature };
        }

        /// <summary>
        /// Creates a <see cref="Temperature"/> from a value in degrees Rankine.
        /// </summary>
        /// <param name="temperature">The temperature in degrees Rankine.</param>
        /// <returns>A new <see cref="Temperature"/> instance.</returns>
        public static Temperature FromRankine(double temperature)
        {
            return new Temperature() { Rankine = temperature };
        }

        /// <summary>
        /// Creates a <see cref="Temperature"/> from a value in degrees Rankine.
        /// </summary>
        /// <param name="temperature">The temperature in degrees Rankine.</param>
        /// <returns>A new <see cref="Temperature"/> instance.</returns>
        public static Temperature FromRankine(decimal temperature)
        {
            return new Temperature() { Rankine = (double)temperature };
        }

        /// <summary>
        /// Gets the temperature in degrees Fahrenheit.
        /// </summary>
        public double Fahrenheit
        {
            get { return (Kelvin * KelvinFahrenheitMultiplier) - AbsoluteTemperatureF; }
            private set { Kelvin = (value + AbsoluteTemperatureF) / KelvinFahrenheitMultiplier; }
        }

        /// <summary>
        /// Gets the temperature in degrees Celsius.
        /// </summary>
        public double Celsius
        {
            get { return Kelvin - AbsoluteTemperatureC; }
            private set { Kelvin = value + AbsoluteTemperatureC; }
        }

        /// <summary>
        /// Gets the temperature in Kelvin (canonical internal unit).
        /// </summary>
        public double Kelvin { get; private set; }

        /// <summary>
        /// Gets the temperature in degrees Rankine.
        /// </summary>
        public double Rankine
        {
            get { return Kelvin * KelvinFahrenheitMultiplier; }
            private set { Kelvin = value / KelvinFahrenheitMultiplier; }
        }
    }
}
