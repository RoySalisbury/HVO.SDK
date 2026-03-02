using System;

namespace HVO.Astronomy
{
    /// <summary>
    /// Represents a geographic longitude in degrees-minutes-seconds with hemisphere.
    /// Provides implicit conversions to and from <see cref="double"/>.
    /// </summary>
    public sealed class Longitude
    {
        /// <summary>
        /// Initializes a new <see cref="Longitude"/> from DMS components.
        /// </summary>
        /// <param name="degrees">Whole degrees (0–180).</param>
        /// <param name="minutes">Arc-minutes (0–59).</param>
        /// <param name="seconds">Arc-seconds (0–59.999…).</param>
        /// <param name="hemisphere">East or west hemisphere.</param>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown when <paramref name="hemisphere"/> is not <see cref="LongitudeHemisphere.East"/>
        /// or <see cref="LongitudeHemisphere.West"/>.
        /// </exception>
        public Longitude(int degrees, int minutes, double seconds, LongitudeHemisphere hemisphere)
        {
            Degrees = Math.Abs(degrees);
            Minutes = Math.Abs(minutes);
            Seconds = Math.Abs(seconds);
            Hemisphere = hemisphere;
        }

        /// <summary>Whole degrees (always ≥ 0).</summary>
        public int Degrees { get; }

        /// <summary>Arc-minutes (always ≥ 0).</summary>
        public int Minutes { get; }

        /// <summary>Arc-seconds (always ≥ 0).</summary>
        public double Seconds { get; }

        /// <summary>East or west hemisphere.</summary>
        public LongitudeHemisphere Hemisphere { get; }

        /// <summary>
        /// Converts this <see cref="Longitude"/> to a signed decimal degrees value.
        /// East is positive; west is negative.
        /// </summary>
        public static implicit operator double(Longitude value)
        {
            int sign = value.Hemisphere == LongitudeHemisphere.East ? 1 : -1;
            return sign * (value.Degrees + (double)value.Minutes / 60 + value.Seconds / 3600);
        }

        /// <summary>
        /// Converts a signed decimal degrees value to a <see cref="Longitude"/>.
        /// Positive values are east; negative values are west.
        /// </summary>
        public static implicit operator Longitude(double value)
        {
            // BUG FIX: Legacy code had CompassPoint.W : CompassPoint.W (always West).
            // Now correctly assigns East for positive, West for negative.
            var hemisphere = value < 0 ? LongitudeHemisphere.West : LongitudeHemisphere.East;
            value = Math.Abs(value);

            int degree = (int)Math.Truncate(value);

            // BUG FIX: When degree == 0, the original code did (value % 0) which is DivideByZero.
            // Use (value - degree) instead of (value % degree) to get the fractional part safely.
            double fractional = value - degree;
            int minute = (int)Math.Truncate(fractional * 60.0);
            double second = (fractional * 60.0 - minute) * 60.0;

            return new Longitude(degree, minute, second, hemisphere);
        }

        /// <summary>
        /// Returns a human-readable DMS string (e.g., "W 113° 48' 36.0\"").
        /// </summary>
        public override string ToString()
        {
            string dir = Hemisphere == LongitudeHemisphere.East ? "E" : "W";
            return string.Format("{0} {1}\u00B0 {2}' {3:F1}\"", dir, Degrees, Minutes, Seconds);
        }
    }
}
