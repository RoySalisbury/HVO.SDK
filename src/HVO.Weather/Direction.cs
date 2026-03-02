using System;

namespace HVO.Weather
{
    /// <summary>
    /// Represents a compass direction as a degree value (0–359) with conversion to the nearest
    /// 16-point <see cref="CompassPoint"/>.
    /// </summary>
    public sealed class Direction
    {
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
            if (degree == 360)
            {
                degree = 0;
            }

            Degree = degree;
        }

        /// <summary>
        /// Initializes a new <see cref="Direction"/> from a <see cref="CompassPoint"/>.
        /// </summary>
        /// <param name="cardinalPoint">The compass point to use as the direction.</param>
        public Direction(CompassPoint cardinalPoint)
        {
            Degree = (short)cardinalPoint;
        }

        /// <summary>
        /// Gets the direction in degrees (0–359).
        /// </summary>
        public short Degree { get; }

        /// <summary>
        /// Gets the closest 16-point <see cref="CompassPoint"/> for the current degree value.
        /// </summary>
        public CompassPoint CardinalPoint
        {
            get
            {
                if (Degree >= (short)CompassPoint.N && Degree < (short)CompassPoint.NNE)
                    return CompassPoint.N;
                else if (Degree >= (short)CompassPoint.NNE && Degree < (short)CompassPoint.NE)
                    return CompassPoint.NNE;
                else if (Degree >= (short)CompassPoint.NE && Degree < (short)CompassPoint.ENE)
                    return CompassPoint.NE;
                else if (Degree >= (short)CompassPoint.ENE && Degree < (short)CompassPoint.E)
                    return CompassPoint.ENE;
                else if (Degree >= (short)CompassPoint.E && Degree < (short)CompassPoint.ESE)
                    return CompassPoint.E;
                else if (Degree >= (short)CompassPoint.ESE && Degree < (short)CompassPoint.SE)
                    return CompassPoint.ESE;
                else if (Degree >= (short)CompassPoint.SE && Degree < (short)CompassPoint.SSE)
                    return CompassPoint.SE;
                else if (Degree >= (short)CompassPoint.SSE && Degree < (short)CompassPoint.S)
                    return CompassPoint.SSE;
                else if (Degree >= (short)CompassPoint.S && Degree < (short)CompassPoint.SSW)
                    return CompassPoint.S;
                else if (Degree >= (short)CompassPoint.SSW && Degree < (short)CompassPoint.SW)
                    return CompassPoint.SSW;
                else if (Degree >= (short)CompassPoint.SW && Degree < (short)CompassPoint.WSW)
                    return CompassPoint.SW;
                else if (Degree >= (short)CompassPoint.WSW && Degree < (short)CompassPoint.W)
                    return CompassPoint.WSW;
                else if (Degree >= (short)CompassPoint.W && Degree < (short)CompassPoint.WNW)
                    return CompassPoint.W;
                else if (Degree >= (short)CompassPoint.WNW && Degree < (short)CompassPoint.NW)
                    return CompassPoint.WNW;
                else if (Degree >= (short)CompassPoint.NW && Degree < (short)CompassPoint.NNW)
                    return CompassPoint.NW;
                else
                    return CompassPoint.NNW;
            }
        }
    }
}
