using System;

namespace HVO.Weather
{
    /// <summary>
    /// Represents a distance value with conversions between metric (meters, kilometers, centimeters)
    /// and imperial (feet, inches, miles) units.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Internal storage uses meters as the canonical unit. All other units are computed on access.
    /// This is an immutable value type — use the static factory methods to create values.
    /// </para>
    /// </remarks>
    public readonly struct Distance : IEquatable<Distance>, IComparable<Distance>
    {
        /// <summary>Conversion factor: 1 meter = 3.28084 feet.</summary>
        private const double MetersToFeet = 3.28084;

        private readonly double _meters;

        private Distance(double meters) => _meters = meters;

        /// <summary>
        /// Creates a <see cref="Distance"/> from a value in meters.
        /// </summary>
        /// <param name="value">The distance in meters.</param>
        /// <returns>A new <see cref="Distance"/> instance.</returns>
        public static Distance FromMeters(double value)
            => new Distance(value);

        /// <summary>
        /// Creates a <see cref="Distance"/> from a value in feet.
        /// </summary>
        /// <param name="value">The distance in feet.</param>
        /// <returns>A new <see cref="Distance"/> instance.</returns>
        public static Distance FromFeet(double value)
            => new Distance(value / MetersToFeet);

        /// <summary>
        /// Creates a <see cref="Distance"/> from a value in kilometers.
        /// </summary>
        /// <param name="value">The distance in kilometers.</param>
        /// <returns>A new <see cref="Distance"/> instance.</returns>
        public static Distance FromKilometers(double value)
            => new Distance(value * 1000);

        /// <summary>
        /// Creates a <see cref="Distance"/> from a value in miles.
        /// </summary>
        /// <param name="value">The distance in miles.</param>
        /// <returns>A new <see cref="Distance"/> instance.</returns>
        public static Distance FromMiles(double value)
            => new Distance(value * 5280 / MetersToFeet);

        /// <summary>
        /// Creates a <see cref="Distance"/> from a value in centimeters.
        /// </summary>
        /// <param name="value">The distance in centimeters.</param>
        /// <returns>A new <see cref="Distance"/> instance.</returns>
        public static Distance FromCentimeters(double value)
            => new Distance(value / 100);

        /// <summary>
        /// Gets the distance in kilometers.
        /// </summary>
        public double Kilometers => _meters / 1000;

        /// <summary>
        /// Gets the distance in centimeters.
        /// </summary>
        public double Centimeters => _meters * 100;

        /// <summary>
        /// Gets the distance in meters (canonical internal unit).
        /// </summary>
        public double Meters => _meters;

        /// <summary>
        /// Gets the distance in inches.
        /// </summary>
        public double Inches => Feet * 12;

        /// <summary>
        /// Gets the distance in feet.
        /// </summary>
        public double Feet => _meters * MetersToFeet;

        /// <summary>
        /// Gets the distance in miles.
        /// </summary>
        public double Miles => Feet / 5280;

        /// <inheritdoc />
        public bool Equals(Distance other) => _meters.Equals(other._meters);

        /// <inheritdoc />
        public override bool Equals(object? obj) => obj is Distance other && Equals(other);

        /// <inheritdoc />
        public override int GetHashCode() => _meters.GetHashCode();

        /// <inheritdoc />
        public int CompareTo(Distance other) => _meters.CompareTo(other._meters);

        /// <inheritdoc />
        public override string ToString() => $"{Meters:F2} m";

        /// <summary>Equality operator</summary>
        public static bool operator ==(Distance left, Distance right) => left.Equals(right);

        /// <summary>Inequality operator</summary>
        public static bool operator !=(Distance left, Distance right) => !left.Equals(right);

        /// <summary>Less-than operator</summary>
        public static bool operator <(Distance left, Distance right) => left._meters < right._meters;

        /// <summary>Greater-than operator</summary>
        public static bool operator >(Distance left, Distance right) => left._meters > right._meters;

        /// <summary>Less-than-or-equal operator</summary>
        public static bool operator <=(Distance left, Distance right) => left._meters <= right._meters;

        /// <summary>Greater-than-or-equal operator</summary>
        public static bool operator >=(Distance left, Distance right) => left._meters >= right._meters;
    }
}
