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
    /// This is an immutable value type — use the static factory methods to create values.
    /// </para>
    /// </remarks>
    public readonly struct BarometricPressure : IEquatable<BarometricPressure>, IComparable<BarometricPressure>
    {
        /// <summary>Conversion factor: 1 inHg = 33.8637526 millibars.</summary>
        private const double InHgToMillibars = 33.8637526;

        private readonly double _inchesHg;

        private BarometricPressure(double inchesHg) => _inchesHg = inchesHg;

        /// <summary>
        /// Creates a <see cref="BarometricPressure"/> from a value in inches of mercury.
        /// </summary>
        /// <param name="value">The pressure in inches of mercury.</param>
        /// <returns>A new <see cref="BarometricPressure"/> instance.</returns>
        public static BarometricPressure FromInchesHg(double value)
            => new BarometricPressure(value);

        /// <summary>
        /// Creates a <see cref="BarometricPressure"/> from a value in inches of mercury.
        /// </summary>
        /// <param name="value">The pressure in inches of mercury.</param>
        /// <returns>A new <see cref="BarometricPressure"/> instance.</returns>
        public static BarometricPressure FromInchesHg(decimal value)
            => FromInchesHg((double)value);

        /// <summary>
        /// Creates a <see cref="BarometricPressure"/> from a value in millibars (hPa).
        /// </summary>
        /// <param name="value">The pressure in millibars.</param>
        /// <returns>A new <see cref="BarometricPressure"/> instance.</returns>
        public static BarometricPressure FromMillibars(double value)
            => new BarometricPressure(value / InHgToMillibars);

        /// <summary>
        /// Creates a <see cref="BarometricPressure"/> from a value in millibars (hPa).
        /// </summary>
        /// <param name="value">The pressure in millibars.</param>
        /// <returns>A new <see cref="BarometricPressure"/> instance.</returns>
        public static BarometricPressure FromMillibars(decimal value)
            => FromMillibars((double)value);

        /// <summary>
        /// Creates a <see cref="BarometricPressure"/> from a value in pascals.
        /// </summary>
        /// <param name="value">The pressure in pascals.</param>
        /// <returns>A new <see cref="BarometricPressure"/> instance.</returns>
        public static BarometricPressure FromPascals(double value)
            => new BarometricPressure((value * 0.01) / InHgToMillibars);

        /// <summary>
        /// Gets the pressure in inches of mercury (canonical internal unit).
        /// </summary>
        public double InchesHg => _inchesHg;

        /// <summary>
        /// Gets the pressure in millibars (hPa).
        /// </summary>
        public double Millibars => _inchesHg * InHgToMillibars;

        /// <summary>
        /// Gets the pressure in pascals. 1 millibar = 100 pascals.
        /// </summary>
        public double Pascals => Millibars * 100.0;

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

        /// <inheritdoc />
        public bool Equals(BarometricPressure other) => _inchesHg.Equals(other._inchesHg);

        /// <inheritdoc />
        public override bool Equals(object? obj) => obj is BarometricPressure other && Equals(other);

        /// <inheritdoc />
        public override int GetHashCode() => _inchesHg.GetHashCode();

        /// <inheritdoc />
        public int CompareTo(BarometricPressure other) => _inchesHg.CompareTo(other._inchesHg);

        /// <inheritdoc />
        public override string ToString() => $"{InchesHg:F2} inHg";

        /// <summary>Equality operator</summary>
        public static bool operator ==(BarometricPressure left, BarometricPressure right) => left.Equals(right);

        /// <summary>Inequality operator</summary>
        public static bool operator !=(BarometricPressure left, BarometricPressure right) => !left.Equals(right);

        /// <summary>Less-than operator</summary>
        public static bool operator <(BarometricPressure left, BarometricPressure right) => left._inchesHg < right._inchesHg;

        /// <summary>Greater-than operator</summary>
        public static bool operator >(BarometricPressure left, BarometricPressure right) => left._inchesHg > right._inchesHg;

        /// <summary>Less-than-or-equal operator</summary>
        public static bool operator <=(BarometricPressure left, BarometricPressure right) => left._inchesHg <= right._inchesHg;

        /// <summary>Greater-than-or-equal operator</summary>
        public static bool operator >=(BarometricPressure left, BarometricPressure right) => left._inchesHg >= right._inchesHg;
    }
}
