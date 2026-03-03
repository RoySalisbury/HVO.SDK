namespace HVO.Astronomy.TheSkyX.Models
{
    /// <summary>
    /// Represents the hardware devices selected/configured in TheSkyX.
    /// </summary>
    public sealed class TheSkyXSelectedHarware
    {
        /// <summary>Gets or sets the primary imaging camera device.</summary>
        public CameraDevice? PrimaryCamera { get; set; }

        /// <summary>Gets or sets the autoguider camera device.</summary>
        public CameraDevice? AutoGuider { get; set; }

        /// <summary>Gets or sets the dome hardware.</summary>
        public ModelManufacturer? Dome { get; set; }

        /// <summary>Gets or sets the mount hardware.</summary>
        public ModelManufacturer? Mount { get; set; }

        /// <summary>Gets or sets the optical tube assembly.</summary>
        public ModelManufacturer? OpticalTubeAssembly { get; set; }

        /// <summary>Gets or sets the weather station hardware.</summary>
        public ModelManufacturer? WeatherStation { get; set; }
    }

    /// <summary>
    /// Represents an imaging camera device with associated peripherals (filter wheel, focuser, rotator, etc.).
    /// </summary>
    public sealed class CameraDevice : ModelManufacturer
    {
        /// <summary>Gets or sets the adaptive optics device.</summary>
        public ModelManufacturer? AdaptiveOptics { get; set; }

        /// <summary>Gets or sets the filter wheel device.</summary>
        public ModelManufacturer? FilterWheel { get; set; }

        /// <summary>Gets or sets the focuser device.</summary>
        public ModelManufacturer? Focuser { get; set; }

        /// <summary>Gets or sets the rotator device.</summary>
        public ModelManufacturer? Rotator { get; set; }

        /// <summary>Gets or sets the video device.</summary>
        public ModelManufacturer? VideoDevice { get; set; }
    }

    /// <summary>
    /// Represents a hardware device with model and manufacturer information.
    /// </summary>
    public class ModelManufacturer
    {
        /// <summary>Gets or sets the device model name.</summary>
        public string? Model { get; set; }

        /// <summary>Gets or sets the device manufacturer name.</summary>
        public string? Manufacturer { get; set; }
    }
}
