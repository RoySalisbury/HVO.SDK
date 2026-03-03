using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.Json;

namespace HVO.Astronomy.TheSkyX
{
    public sealed class Sky6RascomTele
    {
        private readonly TheSkyXClient _theSkyXClient;

        internal Sky6RascomTele(TheSkyXClient theSkyXClient)
        {
            this._theSkyXClient = theSkyXClient;
        }

        public void Abort()
        {
            this._theSkyXClient.ThrowIfNotAttached();
            this.ThowIfNotConnected();

            this._theSkyXClient.SendToTheSkyX("sky6RASCOMTele.Abort();", out var errorMessage);
        }

        public void Connect(bool unpark = false)
        {
            this._theSkyXClient.ThrowIfNotAttached();
            if (unpark)
            {
                this._theSkyXClient.SendToTheSkyX("sky6RASCOMTele.Connect();", out var errorMessage);
            }
            else
            {
                this._theSkyXClient.SendToTheSkyX("sky6RASCOMTele.ConnectAndDoNotUnpark();", out var errorMessage);
            }
        }

        public void DisconnectTelescope()
        {
            this._theSkyXClient.ThrowIfNotAttached();
            this.ThowIfNotConnected();

            this._theSkyXClient.SendToTheSkyX("sky6RASCOMTheSky.DisconnectTelescope();", out var errorMessage);
        }

        // FindHome is a blocking call and can not be run asyncronously. 
        public bool TryFindHome(out Exception? exception)
        {
            exception = null;
            try
            {
                this._theSkyXClient.ThrowIfNotAttached();
                this.ThowIfNotConnected();

                this._theSkyXClient.SendToTheSkyX("sky6RASCOMTele.FindHome();", out var errorMessage);
                return true;
            }
            catch (Exception ex)
            {
                exception = ex;
                return false;
            }
        }

        public void Jog(double minutes, string direction)
        {
            this._theSkyXClient.ThrowIfNotAttached();
            this.ThowIfNotConnected();

            this._theSkyXClient.SendToTheSkyX($"sky6RASCOMTele.Jog({minutes}, {direction});", out var errorMessage);
        }

        public void Park(bool disconnect = true)
        {
            this._theSkyXClient.ThrowIfNotAttached();
            this.ThowIfNotConnected();

            var script = new StringBuilder();
            script.AppendLine("sky6RASCOMTele.Asynchronous = 1;");

            if (disconnect)
            {
                script.AppendLine("sky6RASCOMTele.Park();");
            }
            else
            {
                script.AppendLine("sky6RASCOMTele.ParkAndDoNotDisconnect();");
            }

            this._theSkyXClient.SendToTheSkyX(script.ToString(), out var errorMessage);
        }

        public void SetParkPosition()
        {
            this._theSkyXClient.ThrowIfNotAttached();
            this.ThowIfNotConnected();

            this._theSkyXClient.SendToTheSkyX("sky6RASCOMTele.SetParkPosition();", out var errorMessage);
        }

        public void SetTracking(bool enabled, double? rightAscensionRate = null, double? declinationRate = null)
        {
            this._theSkyXClient.ThrowIfNotAttached();
            this.ThowIfNotConnected();

            bool validRates = rightAscensionRate.HasValue & declinationRate.HasValue;
            this._theSkyXClient.SendToTheSkyX($"sky6RASCOMTele.SetTracking({(enabled ? 1 : 0)}, {(validRates ? 0 : 1)}, {rightAscensionRate.GetValueOrDefault()}, {declinationRate.GetValueOrDefault()})", out var errorMessage);
        }

        public void SlewToAltAz(double altitude, double azimuth, string objectName)
        {
            this._theSkyXClient.ThrowIfNotAttached();
            this.ThowIfNotConnected();

            this._theSkyXClient.SendToTheSkyX($"sky6RASCOMTele.SlewToAzAlt({azimuth}, {altitude}, {objectName?.Trim()})", out var errorMessage);
        }

        public void SlewToRaDec(double rightAscension, double declination, string objectName)
        {
            this._theSkyXClient.ThrowIfNotAttached();
            this.ThowIfNotConnected();

            this._theSkyXClient.SendToTheSkyX($"sky6RASCOMTele.SlewToRaDec({rightAscension}, {declination}, {objectName?.Trim()})", out var errorMessage);
        }

        public void Sync(double rightAscension, double declination, string objectName)
        {
            this._theSkyXClient.ThrowIfNotAttached();
            this.ThowIfNotConnected();

            this._theSkyXClient.SendToTheSkyX($"sky6RASCOMTele.Sync({rightAscension}, {declination}, {objectName?.Trim()})", out var errorMessage);
        }

        public void Unpark()
        {
            this._theSkyXClient.ThrowIfNotAttached();
            this.ThowIfNotConnected();

            this._theSkyXClient.SendToTheSkyX("sky6RASCOMTele.Unpark()", out var errorMessage);
        }

        public bool IsParked()
        {
            this._theSkyXClient.ThrowIfNotAttached();
            this.ThowIfNotConnected();

            var model = this._theSkyXClient.SendToTheSkyX("sky6RASCOMTele.IsParked()", out var errorMessage);
            if (string.IsNullOrWhiteSpace(model))
            {
                throw new InvalidDataException("No response received.");
            }

            bool.TryParse(model, out var result);
            return result;
        }

        public (double Altitude, double Azimuth) GetAltAz()
        {
            this._theSkyXClient.ThrowIfNotAttached();
            this.ThowIfNotConnected();

            StringBuilder script = new StringBuilder();
            script.AppendLine("sky6RASCOMTele.GetAzAlt();");
            script.AppendLine("var objResult = { altitude : sky6RASCOMTele.dAlt, azimuth : sky6RASCOMTele.dAz };");
            script.AppendLine("JSON.stringify(objResult);");

            var model = this._theSkyXClient.SendToTheSkyX(script.ToString(), out var errorMessage);
            if (string.IsNullOrWhiteSpace(model))
            {
                throw new InvalidDataException("No response received.");
            }

            var result = JsonSerializer.Deserialize<AltAz>(model, new JsonSerializerOptions() { PropertyNameCaseInsensitive = true });
            return (result.Altitude, result.Azimuth);
        }

        public (double RightAscension, double Declination) GetRaDec()
        {
            this._theSkyXClient.ThrowIfNotAttached();
            this.ThowIfNotConnected();

            StringBuilder script = new StringBuilder();
            script.AppendLine("sky6RASCOMTele.GetRaDec();");
            script.AppendLine("var objResult = { rightAscension : sky6RASCOMTele.dRa, declination : sky6RASCOMTele.dDec };");
            script.AppendLine("JSON.stringify(objResult);");

            var model = this._theSkyXClient.SendToTheSkyX(script.ToString(), out var errorMessage);
            if (string.IsNullOrWhiteSpace(model))
            {
                throw new InvalidDataException("No response received.");
            }

            var result = JsonSerializer.Deserialize<RaDec>(model, new JsonSerializerOptions() { PropertyNameCaseInsensitive = true });
            return (result.RightAscension, result.Declination);
        }


        public bool IsAsynchronous
        {
            get
            {
                this._theSkyXClient.ThrowIfNotAttached();
                this.ThowIfNotConnected();

                var model = this._theSkyXClient.SendToTheSkyX("sky6RASCOMTele.Asynchronous", out var errorMessage);
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
                this.ThowIfNotConnected();

                var model = this._theSkyXClient.SendToTheSkyX($"sky6RASCOMTele.Asynchronous = {(value ? 1 : 0)}", out var errorMessage);
            }
        }

        public (double rightAscension, double declination) GetRaDecTrackingRate()
        {
            this._theSkyXClient.ThrowIfNotAttached();
            this.ThowIfNotConnected();

            StringBuilder script = new StringBuilder();
            script.AppendLine("var objResult = { rightAscension : sky6RASCOMTele.dRaTrackingRate, declination : sky6RASCOMTele.dDecTrackingRate };");
            script.AppendLine($"JSON.stringify(objResult);");

            var model = this._theSkyXClient.SendToTheSkyX(script.ToString(), out var errorMessage);
            if (string.IsNullOrWhiteSpace(model))
            {
                throw new InvalidDataException("No response received.");
            }

            var result = JsonSerializer.Deserialize<RaDec>(model, new JsonSerializerOptions() { PropertyNameCaseInsensitive = true });
            return (result.RightAscension, result.Declination);
        }

        public void SetRaDecTrackingRate(double? rightAscension, double? declination)
        {
            this._theSkyXClient.ThrowIfNotAttached();
            this.ThowIfNotConnected();

            StringBuilder script = new StringBuilder();
            if (rightAscension.HasValue)
            {
                script.AppendLine($"sky6RASCOMTele.dRaTrackingRate = {rightAscension};");
            }
            if (declination.HasValue)
            {
                script.AppendLine($"sky6RASCOMTele.dDecTrackingRate = {declination};");
            }

            this._theSkyXClient.SendToTheSkyX(script.ToString(), out var errorMessage);
        }


        public double LastSlewError
        {
            get
            {
                this._theSkyXClient.ThrowIfNotAttached();
                this.ThowIfNotConnected();

                var model = this._theSkyXClient.SendToTheSkyX("sky6RASCOMTele.LastSlewError", out var errorMessage);
                if (string.IsNullOrWhiteSpace(model))
                {
                    throw new InvalidDataException("No response received.");
                }

                double.TryParse(model, out var result);
                return result;
            }
        }

        public bool IsConnected
        {
            get
            {
                this._theSkyXClient.ThrowIfNotAttached();
                var model = this._theSkyXClient.SendToTheSkyX("sky6RASCOMTele.IsConnected", out var errorMessage);
                if (string.IsNullOrWhiteSpace(model))
                {
                    throw new InvalidDataException("No response received.");
                }

                int.TryParse(model, out var result);
                return result != 0;
            }
        }

        public bool IsSlewComplete
        {
            get
            {
                this._theSkyXClient.ThrowIfNotAttached();
                //this.ThowIfNotConnected();

                var model = this._theSkyXClient.SendToTheSkyX("sky6RASCOMTele.IsSlewComplete", out var errorMessage);
                if (string.IsNullOrWhiteSpace(model))
                {
                    throw new InvalidDataException("No response received.");
                }

                int.TryParse(model, out var result);
                return result != 0;
            }
        }

        public bool IsTracking
        {
            get
            {
                this._theSkyXClient.ThrowIfNotAttached();
                this.ThowIfNotConnected();

                var model = this._theSkyXClient.SendToTheSkyX("sky6RASCOMTele.IsTracking", out var errorMessage);
                if (string.IsNullOrWhiteSpace(model))
                {
                    throw new InvalidDataException("No response received.");
                }

                int.TryParse(model, out var result);
                return result != 0;
            }
        }



        private void ThowIfNotConnected()
        {
            var model = this._theSkyXClient.SendToTheSkyX("sky6RASCOMTele.IsConnected", out var errorMessage);
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
