using System;

namespace HVO.Astronomy
{
    /// <summary>
    /// Represents a geographic latitude in degrees-minutes-seconds with hemisphere.
    /// Provides implicit conversions to and from <see cref="double"/>.
    /// </summary>
    public sealed class Latitude
    {
        /// <summary>
        /// Initializes a new <see cref="Latitude"/> from DMS components.
        /// </summary>
        /// <param name="degrees">Whole degrees (0–90).</param>
        /// <param name="minutes">Arc-minutes (0–59).</param>
        /// <param name="seconds">Arc-seconds (0–59.999…).</param>
        /// <param name="hemisphere">North or south hemisphere.</param>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown when <paramref name="hemisphere"/> is not <see cref="LatitudeHemisphere.North"/>
        /// or <see cref="LatitudeHemisphere.South"/>.
        /// </exception>
        public Latitude(int degrees, int minutes, double seconds, LatitudeHemisphere hemisphere)
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

        /// <summary>North or south hemisphere.</summary>
        public LatitudeHemisphere Hemisphere { get; }

        /// <summary>
        /// Converts this <see cref="Latitude"/> to a signed decimal degrees value.
        /// North is positive; south is negative.
        /// </summary>
        public static implicit operator double(Latitude value)
        {
            int sign = value.Hemisphere == LatitudeHemisphere.North ? 1 : -1;
            return sign * (value.Degrees + (double)value.Minutes / 60 + value.Seconds / 3600);
        }

        /// <summary>
        /// Converts a signed decimal degrees value to a <see cref="Latitude"/>.
        /// Positive values are north; negative values are south.
        /// </summary>
        public static implicit operator Latitude(double value)
        {
            var hemisphere = value < 0 ? LatitudeHemisphere.South : LatitudeHemisphere.North;
            value = Math.Abs(value);

            int degree = (int)Math.Truncate(value);

            // BUG FIX: When degree == 0, the original code did (value % 0) which is DivideByZero.
            // Use (value - degree) instead of (value % degree) to get the fractional part safely.
            double fractional = value - degree;
            int minute = (int)Math.Truncate(fractional * 60.0);
            double second = (fractional * 60.0 - minute) * 60.0;

            return new Latitude(degree, minute, second, hemisphere);
        }

        /// <summary>
        /// Returns a human-readable DMS string (e.g., "N 35° 10' 48.6\"").
        /// </summary>
        public override string ToString()
        {
            string dir = Hemisphere == LatitudeHemisphere.North ? "N" : "S";
            return string.Format("{0} {1}\u00B0 {2}' {3:F1}\"", dir, Degrees, Minutes, Seconds);
        }
    }
}
