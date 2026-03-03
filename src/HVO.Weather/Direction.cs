using System;

namespace HVO.Weather
{
    /// <summary>
    /// Represents a compass direction as a degree value (0–359) with conversion to the nearest
    /// 16-point <see cref="CompassPoint"/>.
    /// </summary>
    public readonly struct Direction : IEquatable<Direction>, IComparable<Direction>
    {
        private static readonly CompassPoint[] CompassPoints = new[]
        {
            CompassPoint.N, CompassPoint.NNE, CompassPoint.NE, CompassPoint.ENE,
            CompassPoint.E, CompassPoint.ESE, CompassPoint.SE, CompassPoint.SSE,
            CompassPoint.S, CompassPoint.SSW, CompassPoint.SW, CompassPoint.WSW,
            CompassPoint.W, CompassPoint.WNW, CompassPoint.NW, CompassPoint.NNW
        };

        private readonly short _degree;

        /// <summary>
        /// Initializes a new <see cref="Direction"/> from a degree value.
        /// </summary>
        /// <param name="degree">The direction in degrees (0–360). A value of 360 is normalized to 0.</param>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown when <paramref name="degree"/> is less than 0 or greater than 360.
        /// </exception>
        public Direction(short degree)
        {
            if (degree < 0 || degree > 360)
            {
                throw new ArgumentOutOfRangeException(nameof(degree), degree,
                    "Degree must be between 0 and 360.");
            }

            // Normalize 360 to 0
            _degree = degree == 360 ? (short)0 : degree;
        }

        /// <summary>
        /// Initializes a new <see cref="Direction"/> from a <see cref="CompassPoint"/>.
        /// </summary>
        /// <param name="cardinalPoint">The compass point to use as the direction.</param>
        public Direction(CompassPoint cardinalPoint)
        {
            _degree = (short)cardinalPoint;
        }

        /// <summary>
        /// Gets the direction in degrees (0–359).
        /// </summary>
        public short Degree => _degree;

        /// <summary>
        /// Gets the closest 16-point <see cref="CompassPoint"/> for the current degree value.
        /// </summary>
        public CompassPoint CardinalPoint
        {
            get
            {
                for (int i = CompassPoints.Length - 1; i >= 0; i--)
                {
                    if (_degree >= (short)CompassPoints[i])
                        return CompassPoints[i];
                }

                return CompassPoint.N;
            }
        }

        /// <inheritdoc />
        public bool Equals(Direction other) => _degree == other._degree;

        /// <inheritdoc />
        public override bool Equals(object? obj) => obj is Direction other && Equals(other);

        /// <inheritdoc />
        public override int GetHashCode() => _degree.GetHashCode();

        /// <inheritdoc />
        public int CompareTo(Direction other) => _degree.CompareTo(other._degree);

        /// <inheritdoc />
        public override string ToString() => $"{_degree}°";

        /// <summary>Equality operator</summary>
        public static bool operator ==(Direction left, Direction right) => left.Equals(right);

        /// <summary>Inequality operator</summary>
        public static bool operator !=(Direction left, Direction right) => !left.Equals(right);

        /// <summary>Less-than operator</summary>
        public static bool operator <(Direction left, Direction right) => left._degree < right._degree;

        /// <summary>Greater-than operator</summary>
        public static bool operator >(Direction left, Direction right) => left._degree > right._degree;

        /// <summary>Less-than-or-equal operator</summary>
        public static bool operator <=(Direction left, Direction right) => left._degree <= right._degree;

        /// <summary>Greater-than-or-equal operator</summary>
        public static bool operator >=(Direction left, Direction right) => left._degree >= right._degree;
    }
}
