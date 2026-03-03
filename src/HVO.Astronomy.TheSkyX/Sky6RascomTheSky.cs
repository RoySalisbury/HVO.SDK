using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.Json;

namespace HVO.Astronomy.TheSkyX
{
    public sealed class Sky6RascomTheSky
    {
        private readonly TheSkyXClient _theSkyXClient;

        internal Sky6RascomTheSky(TheSkyXClient theSkyXClient)
        {
            this._theSkyXClient = theSkyXClient;
        }

        public void AutoMap()
        {
            this._theSkyXClient.ThrowIfNotAttached();
            this.ThrowIfNotConnected();

            this._theSkyXClient.SendToTheSkyX("sky6RASCOMTheSky.AutoMap();", out var errorMessage);
        }

        [Obsolete("Use sky6StarChart")]
        public void Connect()
        {
            this._theSkyXClient.ThrowIfNotAttached();

            this._theSkyXClient.SendToTheSkyX("sky6RASCOMTheSky.Connect();", out var errorMessage);
        }

        public void ConnectDome()
        {
            this._theSkyXClient.ThrowIfNotAttached();
            this.ThrowIfNotConnected();

            this._theSkyXClient.SendToTheSkyX("sky6RASCOMTheSky.ConnectDome();", out var errorMessage);
        }

        public void CoupleDome()
        {
            this._theSkyXClient.ThrowIfNotAttached();
            this.ThrowIfNotConnected();

            this._theSkyXClient.SendToTheSkyX("sky6RASCOMTheSky.CoupleDome();", out var errorMessage);
        }

        [Obsolete("Use sky6StarChart")]
        public void Disconnect()
        {
            this._theSkyXClient.ThrowIfNotAttached();
            this.ThrowIfNotConnected();

            this._theSkyXClient.SendToTheSkyX("sky6RASCOMTheSky.Disconnect();", out var errorMessage);
        }

        public void DisconnectDome()
        {
            this._theSkyXClient.ThrowIfNotAttached();
            this.ThrowIfNotConnected();

            this._theSkyXClient.SendToTheSkyX("sky6RASCOMTheSky.DisconnectDome();", out var errorMessage);
        }

        public void DisconnectTelescope()
        {
            this._theSkyXClient.ThrowIfNotAttached();
            this.ThrowIfNotConnected();

            this._theSkyXClient.SendToTheSkyX("sky6RASCOMTheSky.DisconnectTelescope();", out var errorMessage);
        }

        [Obsolete("Use sky6StarChart")]
        public (double Altitude, double Azimuth) GetObjectAltAz(string objectName)
        {
            this._theSkyXClient.ThrowIfNotAttached();
            this.ThrowIfNotConnected();

            var script = new StringBuilder();
            script.AppendLine($"sky6RASCOMTheSky.GetObjectAzAlt('{objectName}');");
            script.AppendLine("var objResult = { altitude: sky6RASCOMTheSky.dObjectAlt, azimuth: sky6RASCOMTheSky.dObjectAz };");
            script.AppendLine("JSON.stringify(objResult);");

            var model = this._theSkyXClient.SendToTheSkyX(script.ToString(), out var errorMessage);
            if (string.IsNullOrWhiteSpace(model))
            {
                throw new InvalidDataException("No response received.");
            }

            var result = JsonSerializer.Deserialize<AltAz>(model, new JsonSerializerOptions() { PropertyNameCaseInsensitive = true })!;
            return (result.Altitude, result.Azimuth);
        }

        [Obsolete("Use sky6StarChart")]
        public (double RightAscension, double Declination) GetObjectRaDec(string objectName)
        {
            this._theSkyXClient.ThrowIfNotAttached();
            this.ThrowIfNotConnected();

            var script = new StringBuilder();
            script.AppendLine($"sky6RASCOMTheSky.GetObjectRaDec('{objectName}');");
            script.AppendLine("var objResult = { rightAscension: sky6RASCOMTheSky.dObjectRa, declination: sky6RASCOMTheSky.dObjectDec };");
            script.AppendLine("JSON.stringify(objResult);");

            var model = this._theSkyXClient.SendToTheSkyX(script.ToString(), out var errorMessage);
            if (string.IsNullOrWhiteSpace(model))
            {
                throw new InvalidDataException("No response received.");
            }

            var result = JsonSerializer.Deserialize<RaDec>(model, new JsonSerializerOptions() { PropertyNameCaseInsensitive = true })!;
            return (result.RightAscension, result.Declination);
        }

        [Obsolete("Use sky6StarChart")]
        public (double RightAscension, double Declination) GetObjectRaDec2000(string objectName)
        {
            this._theSkyXClient.ThrowIfNotAttached();
            this.ThrowIfNotConnected();

            var script = new StringBuilder();
            script.AppendLine($"sky6RASCOMTheSky.GetObjectRaDec2000('{objectName}');");
            script.AppendLine("var objResult = { rightAscension: sky6RASCOMTheSky.dObjectRa, declination: sky6RASCOMTheSky.dObjectDec };");
            script.AppendLine("JSON.stringify(objResult);");

            var model = this._theSkyXClient.SendToTheSkyX(script.ToString(), out var errorMessage);
            if (string.IsNullOrWhiteSpace(model))
            {
                throw new InvalidDataException("No response received.");
            }

            var result = JsonSerializer.Deserialize<RaDec>(model, new JsonSerializerOptions() { PropertyNameCaseInsensitive = true })!;
            return (result.RightAscension, result.Declination);
        }

        [Obsolete("Use native ImageLink object")]
        public void ImageLink(double rightAscension2000, double declination2000, double imageScale)
        {
            this._theSkyXClient.ThrowIfNotAttached();
            this.ThrowIfNotConnected();

            this._theSkyXClient.SendToTheSkyX($"sky6RASCOMTheSky.ImageLink({rightAscension2000}, {declination2000}, {imageScale});", out var errorMessage);
        }

        [Obsolete("Use sky6StarChart")]
        public void SetTelescopeParkPosition()
        {
            this._theSkyXClient.ThrowIfNotAttached();
            this.ThrowIfNotConnected();

            this._theSkyXClient.SendToTheSkyX("sky6RASCOMTheSky.SetTelescopeParkPosition();", out var errorMessage);
        }

        public void Quit()
        {
            this._theSkyXClient.ThrowIfNotAttached();
            this.ThrowIfNotConnected();

            this._theSkyXClient.SendToTheSkyX("sky6RASCOMTheSky.Quit();", out var errorMessage);
        }

        [Obsolete("Use sky6StarChart")]
        public void SetWhenWhere(double julianDay, int dstOption, int useSystemClock, string description, double latitude, double longitude, double timeZone, double elevation)
        {
            this._theSkyXClient.ThrowIfNotAttached();
            this.ThrowIfNotConnected();

            this._theSkyXClient.SendToTheSkyX($"sky6RASCOMTheSky.SetWhenWhere({julianDay}, {dstOption}, {useSystemClock}, '{description}', {latitude}, {longitude}, {timeZone}, {elevation});", out var errorMessage);
        }

        [Obsolete("Use sky6StarChart")]
        public (double RightAscension, double Declination) GetScreenRaDec()
        {
            this._theSkyXClient.ThrowIfNotAttached();
            this.ThrowIfNotConnected();

            var script = new StringBuilder();
            script.AppendLine($"sky6RASCOMTheSky.GetScreenRaDec();");
            script.AppendLine("var objResult = { rightAscension: sky6RASCOMTheSky.dScreenRa, declination: sky6RASCOMTheSky.dScreenDec };");
            script.AppendLine("JSON.stringify(objResult);");

            var model = this._theSkyXClient.SendToTheSkyX(script.ToString(), out var errorMessage);
            if (string.IsNullOrWhiteSpace(model))
            {
                throw new InvalidDataException("No response received.");
            }

            var result = JsonSerializer.Deserialize<RaDec>(model, new JsonSerializerOptions() { PropertyNameCaseInsensitive = true })!;
            return (result.RightAscension, result.Declination);
        }

        [Obsolete("Use sky6StarChart")]
        public double ScreenRotation
        {
            get
            {
                this._theSkyXClient.ThrowIfNotAttached();
                this.ThrowIfNotConnected();

                var model = this._theSkyXClient.SendToTheSkyX("sky6RASCOMTheSky.dScreenRotation", out var errorMessage);
                if (string.IsNullOrWhiteSpace(model))
                {
                    throw new InvalidDataException("No response received.");
                }

                double.TryParse(model, out var result);
                return result;
            }
            set
            {
                this._theSkyXClient.ThrowIfNotAttached();
                this.ThrowIfNotConnected();

                this._theSkyXClient.SendToTheSkyX($"sky6RASCOMTheSky.dScreenRotation = {value}", out var errorMessage);
            }
        }

        [Obsolete("Use sky6StarChart")]
        public double ScreenFOV
        {
            get
            {
                this._theSkyXClient.ThrowIfNotAttached();
                this.ThrowIfNotConnected();

                var model = this._theSkyXClient.SendToTheSkyX("sky6RASCOMTheSky.dScreenFOV", out var errorMessage);
                if (string.IsNullOrWhiteSpace(model))
                {
                    throw new InvalidDataException("No response received.");
                }

                double.TryParse(model, out var result);
                return result;
            }
            set
            {
                this._theSkyXClient.ThrowIfNotAttached();
                this.ThrowIfNotConnected();

                this._theSkyXClient.SendToTheSkyX($"sky6RASCOMTheSky.dScreenFOV = {value}", out var errorMessage);
            }
        }

        [Obsolete("Use sky6StarChart")]
        public bool IsConnected
        {
            get
            {
                // According to the documentation, this will always return true
                this._theSkyXClient.ThrowIfNotAttached();
                var model = this._theSkyXClient.SendToTheSkyX("sky6RASCOMTheSky.IsConnected", out var errorMessage);
                if (string.IsNullOrWhiteSpace(model))
                {
                    throw new InvalidDataException("No response received.");
                }

                int.TryParse(model, out var result);
                return result != 0;
            }
        }

        [Obsolete("Use sky6StarChart")]
        public bool IsAsynchronous
        {
            get
            {
                // According to the documentation, this will always return false
                this._theSkyXClient.ThrowIfNotAttached();
                this.ThrowIfNotConnected();

                var model = this._theSkyXClient.SendToTheSkyX("sky6RASCOMTheSky.IsAsynchronous", out var errorMessage);
                if (string.IsNullOrWhiteSpace(model))
                {
                    throw new InvalidDataException("No response received.");
                }

                int.TryParse(model, out var result);
                return result != 0;
            }
            set
            {
                this._theSkyXClient.ThrowIfNotAttached();
                this.ThrowIfNotConnected();

                var model = this._theSkyXClient.SendToTheSkyX($"sky6RASCOMTheSky.IsAsynchronous = {(value ? 1 : 0)}", out var errorMessage);
            }
        }

        private void ThrowIfNotConnected()
        {
            var model = this._theSkyXClient.SendToTheSkyX("sky6RASCOMTheSky.IsConnected", out var errorMessage);
            if (string.IsNullOrWhiteSpace(model))
            {
                throw new InvalidDataException("No response received.");
            }

            if (int.TryParse(model, out var result) && (result != 0))
            {
                return;
            }

            throw new InvalidOperationException("TheSkyX telescope has not been connected.");
        }

        // This is just a private helper class to aid in deserialization.  The MS provided deserialization does not yet support dynamic objects (ExpandoObject).
        private class RaDec
        {
            public double RightAscension { get; set; }
            public double Declination { get; set; }
        }

        // This is just a private helper class to aid in deserialization.  The MS provided deserialization does not yet support dynamic objects (ExpandoObject).
        private class AltAz
        {
            public double Altitude { get; set; }
            public double Azimuth { get; set; }
        }
    }
}
