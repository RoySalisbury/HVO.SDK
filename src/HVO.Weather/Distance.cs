namespace HVO.Weather
{
    /// <summary>
    /// Represents a distance value with conversions between metric (meters, kilometers, centimeters)
    /// and imperial (feet, inches, miles) units.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Internal storage uses meters as the canonical unit. All other units are computed on access.
    /// Instances are immutable — use the static factory methods to create values.
    /// </para>
    /// </remarks>
    public sealed class Distance
    {
        /// <summary>Conversion factor: 1 meter = 3.28084 feet.</summary>
        private const double MetersToFeet = 3.28084;

        private Distance() { }

        /// <summary>
        /// Creates a <see cref="Distance"/> from a value in meters.
        /// </summary>
        /// <param name="value">The distance in meters.</param>
        /// <returns>A new <see cref="Distance"/> instance.</returns>
        public static Distance FromMeters(double value)
        {
            return new Distance() { Meters = value };
        }

        /// <summary>
        /// Creates a <see cref="Distance"/> from a value in feet.
        /// </summary>
        /// <param name="value">The distance in feet.</param>
        /// <returns>A new <see cref="Distance"/> instance.</returns>
        public static Distance FromFeet(double value)
        {
            return new Distance() { Feet = value };
        }

        /// <summary>
        /// Creates a <see cref="Distance"/> from a value in kilometers.
        /// </summary>
        /// <param name="value">The distance in kilometers.</param>
        /// <returns>A new <see cref="Distance"/> instance.</returns>
        public static Distance FromKilometers(double value)
        {
            return new Distance() { Kilometers = value };
        }

        /// <summary>
        /// Creates a <see cref="Distance"/> from a value in miles.
        /// </summary>
        /// <param name="value">The distance in miles.</param>
        /// <returns>A new <see cref="Distance"/> instance.</returns>
        public static Distance FromMiles(double value)
        {
            return new Distance() { Miles = value };
        }

        /// <summary>
        /// Creates a <see cref="Distance"/> from a value in centimeters.
        /// </summary>
        /// <param name="value">The distance in centimeters.</param>
        /// <returns>A new <see cref="Distance"/> instance.</returns>
        public static Distance FromCentimeters(double value)
        {
            return new Distance() { Centimeters = value };
        }

        /// <summary>
        /// Gets the distance in kilometers.
        /// </summary>
        public double Kilometers
        {
            get { return Meters / 1000; }
            private set { Meters = value * 1000; }
        }

        /// <summary>
        /// Gets the distance in centimeters.
        /// </summary>
        /// <remarks>
        /// BUG FIX: The legacy HVOv6 implementation used <c>value / 10</c> in the setter,
        /// which caused 100 cm to produce 10 m instead of 1 m. This has been corrected to <c>value / 100</c>.
        /// </remarks>
        public double Centimeters
        {
            get { return Meters * 100; }
            private set { Meters = value / 100; }
        }

        /// <summary>
        /// Gets the distance in meters (canonical internal unit).
        /// </summary>
        public double Meters { get; private set; }

        /// <summary>
        /// Gets the distance in inches.
        /// </summary>
        public double Inches
        {
            get { return Feet * 12; }
            private set { Feet = value / 12; }
        }

        /// <summary>
        /// Gets the distance in feet.
        /// </summary>
        public double Feet
        {
            get { return Meters * MetersToFeet; }
            private set { Meters = value / MetersToFeet; }
        }

        /// <summary>
        /// Gets the distance in miles.
        /// </summary>
        public double Miles
        {
            get { return Feet / 5280; }
            private set { Feet = value * 5280; }
        }
    }
}
