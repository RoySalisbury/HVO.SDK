using System.Text.Json.Serialization;

namespace HVO.Astronomy.TheSkyX
{
    /// <summary>
    /// Represents the width and height of an image or chart in pixels.
    /// Replaces <c>System.Drawing.Size</c> for cross-platform compatibility.
    /// </summary>
    public sealed class ImageSize
    {
        /// <summary>
        /// Gets or sets the width in pixels.
        /// </summary>
        [JsonPropertyName("width")]
        public int Width { get; set; }

        /// <summary>
        /// Gets or sets the height in pixels.
        /// </summary>
        [JsonPropertyName("height")]
        public int Height { get; set; }
    }
}
