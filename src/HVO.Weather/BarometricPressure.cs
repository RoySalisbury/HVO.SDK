using System;

namespace HVO.Weather
{
    /// <summary>
    /// Represents a barometric pressure value with conversions between inches of mercury (inHg),
    /// millibars (hPa), and pascals.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Internal storage uses inches of mercury (inHg) as the canonical unit.
    /// Instances are immutable — use the static factory methods to create values.
    /// </para>
    /// </remarks>
    public sealed class BarometricPressure
    {
        /// <summary>Conversion factor: 1 inHg = 33.8637526 millibars.</summary>
        private const double InHgToMillibars = 33.8637526;

        private BarometricPressure() { }

        /// <summary>
        /// Creates a <see cref="BarometricPressure"/> from a value in inches of mercury.
        /// </summary>
        /// <param name="value">The pressure in inches of mercury.</param>
        /// <returns>A new <see cref="BarometricPressure"/> instance.</returns>
        public static BarometricPressure FromInchesHg(double value)
        {
            return new BarometricPressure() { InchesHg = value };
        }

        /// <summary>
        /// Creates a <see cref="BarometricPressure"/> from a value in inches of mercury.
        /// </summary>
        /// <param name="value">The pressure in inches of mercury.</param>
        /// <returns>A new <see cref="BarometricPressure"/> instance.</returns>
        public static BarometricPressure FromInchesHg(decimal value)
        {
            return new BarometricPressure() { InchesHg = (double)value };
        }

        /// <summary>
        /// Creates a <see cref="BarometricPressure"/> from a value in millibars (hPa).
        /// </summary>
        /// <param name="value">The pressure in millibars.</param>
        /// <returns>A new <see cref="BarometricPressure"/> instance.</returns>
        public static BarometricPressure FromMillibars(double value)
        {
            return new BarometricPressure() { Millibars = value };
        }

        /// <summary>
        /// Creates a <see cref="BarometricPressure"/> from a value in millibars (hPa).
        /// </summary>
        /// <param name="value">The pressure in millibars.</param>
        /// <returns>A new <see cref="BarometricPressure"/> instance.</returns>
        public static BarometricPressure FromMillibars(decimal value)
        {
            return new BarometricPressure() { Millibars = (double)value };
        }

        /// <summary>
        /// Creates a <see cref="BarometricPressure"/> from a value in pascals.
        /// </summary>
        /// <param name="value">The pressure in pascals.</param>
        /// <returns>A new <see cref="BarometricPressure"/> instance.</returns>
        public static BarometricPressure FromPascals(double value)
        {
            return new BarometricPressure() { Millibars = value * 0.01 };
        }

        /// <summary>
        /// Gets the pressure in inches of mercury (canonical internal unit).
        /// </summary>
        public double InchesHg { get; private set; }

        /// <summary>
        /// Gets the pressure in millibars (hPa).
        /// </summary>
        public double Millibars
        {
            get { return InchesHg * InHgToMillibars; }
            private set { InchesHg = value / InHgToMillibars; }
        }

        /// <summary>
        /// Gets the pressure in pascals. 1 millibar = 100 pascals.
        /// </summary>
        public double Pascals
        {
            get { return Millibars * 100.0; }
        }

        /// <summary>
        /// Computes the altimeter setting from an absolute pressure reading and station elevation
        /// using the MADIS algorithm.
        /// </summary>
        /// <param name="absoluteMillibars">The absolute (station) pressure in millibars.</param>
        /// <param name="elevationMeters">The station elevation in meters above sea level.</param>
        /// <returns>A <see cref="BarometricPressure"/> representing the altimeter setting.</returns>
        public static BarometricPressure AltimeterFromAbsoluteMb(double absoluteMillibars, double elevationMeters)
        {
            double part1 = absoluteMillibars - 0.3;
            double part2 = 1.0 + (0.0000842288 * (elevationMeters / Math.Pow(part1, 0.190284)));
            double altimeterSettingMb = part1 * Math.Pow(part2, 5.2553026);
            return FromInchesHg(altimeterSettingMb * 0.02953);
        }
    }
}
