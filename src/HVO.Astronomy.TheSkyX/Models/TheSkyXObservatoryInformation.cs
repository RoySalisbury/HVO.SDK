using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace HVO.Astronomy.TheSkyX.Models
{
    public class TheSkyXObservatoryInformation
    {
        [JsonPropertyName("latitude")]
        public double Latitude
        {
            get; set;
        }

        [JsonPropertyName("longitude")]
        public double Longitude
        {
            get; set;
        }

        [JsonPropertyName("timeZoneOffset")]
        public int TimeZoneOffset
        {
            get; set;
        }

        [JsonPropertyName("elevation")]
        public int ElevationInMeters
        {
            get; set;
        }

        [JsonPropertyName("locationName")]
        public string LocationName
        {
            get; set;
        } = string.Empty;
    }
}
