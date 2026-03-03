using System;
using System.Globalization;

namespace HVO.Astronomy
{
    /// <summary>
    /// Represents a right ascension value in the equatorial coordinate system.
    /// Right ascension is measured in hours (0h to 24h), analogous to longitude on Earth.
    /// </summary>
    public readonly struct RightAscension : IEquatable<RightAscension>
    {
        private readonly TimeSpan _rightAscension;

        private RightAscension(TimeSpan timespan)
        {
            _rightAscension = timespan;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RightAscension"/> struct from hours, minutes, and seconds.
        /// </summary>
        /// <param name="hours">Hours component (0–23).</param>
        /// <param name="minutes">Minutes component (0–59).</param>
        /// <param name="seconds">Seconds component (0 to &lt;60).</param>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown when <paramref name="hours"/> is not between 0 and 23,
        /// <paramref name="minutes"/> is not between 0 and 59,
        /// or <paramref name="seconds"/> is not between 0 (inclusive) and 60 (exclusive).
        /// </exception>
        public RightAscension(int hours, int minutes, double seconds)
        {
            if (hours < 0 || hours > 23)
            {
                throw new ArgumentOutOfRangeException(nameof(hours), "Hours must be between 0 and 23.");
            }

            if (minutes < 0 || minutes > 59)
            {
                throw new ArgumentOutOfRangeException(nameof(minutes), "Minutes must be between 0 and 59.");
            }

            if (seconds < 0 || !(seconds < 60))
            {
                throw new ArgumentOutOfRangeException(nameof(seconds), "Seconds must be greater than or equal to 0 and less than 60.");
            }

            _rightAscension = TimeSpan.FromSeconds(seconds + (minutes * 60) + (hours * 3600));
        }

        /// <summary>
        /// Creates a <see cref="RightAscension"/> from a <see cref="TimeSpan"/>.
        /// </summary>
        /// <param name="timespan">The time span representing the right ascension.</param>
        /// <returns>A new <see cref="RightAscension"/> instance.</returns>
        public static RightAscension FromTimeSpan(TimeSpan timespan)
        {
            return new RightAscension(timespan);
        }

        /// <summary>
        /// Creates a <see cref="RightAscension"/> from an angle in degrees.
        /// The absolute value is used and converted at a rate of 15 degrees per hour.
        /// </summary>
        /// <param name="degrees">The angle in degrees.</param>
        /// <returns>A new <see cref="RightAscension"/> instance.</returns>
        public static RightAscension FromDegrees(double degrees)
        {
            return new RightAscension(TimeSpan.FromHours(Math.Abs(degrees) / 15.0));
        }

        /// <summary>
        /// Creates a <see cref="RightAscension"/> from total hours.
        /// The value is normalized to the range [0, 24).
        /// </summary>
        /// <param name="totalHours">The right ascension in total hours.</param>
        /// <returns>A new <see cref="RightAscension"/> instance.</returns>
        public static RightAscension FromHours(double totalHours)
        {
            // Normalize to [0, 24) range — 24h wraps to 0h in right ascension.
            totalHours = totalHours % 24.0;
            if (totalHours < 0)
            {
                totalHours += 24.0;
            }

            return new RightAscension(TimeSpan.FromHours(totalHours));
        }

        /// <summary>
        /// Gets the hours component of the right ascension.
        /// </summary>
        public int Hours => _rightAscension.Hours;

        /// <summary>
        /// Gets the minutes component of the right ascension.
        /// </summary>
        public int Minutes => _rightAscension.Minutes;

        /// <summary>
        /// Gets the seconds component of the right ascension, rounded to 4 decimal places.
        /// </summary>
        public double Seconds => Math.Round((_rightAscension.TotalMinutes - Math.Truncate(_rightAscension.TotalMinutes)) * 60, 4);

        /// <summary>
        /// Gets the total right ascension expressed in degrees (hours × 15).
        /// </summary>
        public double Degrees => _rightAscension.TotalHours * 15;

        /// <summary>
        /// Gets the total right ascension expressed in hours.
        /// </summary>
        public double TotalHours => _rightAscension.TotalHours;

        /// <summary>
        /// Returns a string representation of the right ascension in the format "Hh Mm S.SSSSs".
        /// </summary>
        /// <returns>A formatted string.</returns>
        public override string ToString()
        {
            return _rightAscension.ToString("h'h 'm'm 's'.'ffff's'", CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// Converts the right ascension to a <see cref="TimeSpan"/>.
        /// </summary>
        /// <returns>A <see cref="TimeSpan"/> representing the right ascension.</returns>
        public TimeSpan ToTimeSpan() => _rightAscension;

        /// <inheritdoc />
        public bool Equals(RightAscension other) => _rightAscension.Equals(other._rightAscension);

        /// <inheritdoc />
        public override bool Equals(object? obj) => obj is RightAscension other && Equals(other);

        /// <inheritdoc />
        public override int GetHashCode() => _rightAscension.GetHashCode();

        /// <summary>Equality operator</summary>
        public static bool operator ==(RightAscension left, RightAscension right) => left.Equals(right);

        /// <summary>Inequality operator</summary>
        public static bool operator !=(RightAscension left, RightAscension right) => !left.Equals(right);
    }
}
