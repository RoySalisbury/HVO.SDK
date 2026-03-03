using System;
using System.IO;

namespace HVO.Astronomy.TheSkyX
{
    public sealed class AutomatedImageLinkSettings
    {
        private readonly TheSkyXClient _theSkyXClient;

        internal AutomatedImageLinkSettings(TheSkyXClient theSkyXClient)
        {
            this._theSkyXClient = theSkyXClient;
        }

        public double ImageScale
        {
            get 
            {
                this._theSkyXClient.ThrowIfNotAttached(8114);
                var model = this._theSkyXClient.SendToTheSkyX("AutomatedImageLinkSettings.imageScale", out var errorMessage);

                if (string.IsNullOrWhiteSpace(model))
                {
                    throw new InvalidDataException("No response received.");
                }

                double.TryParse(model, out var result);
                return result;
            }
            set
            {
                this._theSkyXClient.ThrowIfNotAttached(8114);
                this._theSkyXClient.SendToTheSkyX($"AutomatedImageLinkSettings.imageScale = {value};", out var errorMessage);
            }
        }

        public double PositionAngle
        {
            get
            {
                this._theSkyXClient.ThrowIfNotAttached(8114);
                var model = this._theSkyXClient.SendToTheSkyX("AutomatedImageLinkSettings.positionAngle", out var errorMessage);

                if (string.IsNullOrWhiteSpace(model))
                {
                    throw new InvalidDataException("No response received.");
                }

                double.TryParse(model, out var result);
                return result;
            }
        }

        public TimeSpan ExposureTime
        {
            get
            {
                this._theSkyXClient.ThrowIfNotAttached(8114);
                var model = this._theSkyXClient.SendToTheSkyX("AutomatedImageLinkSettings.exposureTimeAILS", out var errorMessage);

                if (string.IsNullOrWhiteSpace(model))
                {
                    throw new InvalidDataException("No response received.");
                }

                double.TryParse(model, out var result);
                return TimeSpan.FromSeconds(result);
            }
            set
            {
                this._theSkyXClient.ThrowIfNotAttached(8114);
                this._theSkyXClient.SendToTheSkyX($"AutomatedImageLinkSettings.exposureTimeAILS = {value.TotalSeconds};", out var errorMessage);
            }
        }

        public int FOVsToSearch
        {
            get
            {
                this._theSkyXClient.ThrowIfNotAttached(8114);
                var model = this._theSkyXClient.SendToTheSkyX("AutomatedImageLinkSettings.fovsToSearch", out var errorMessage);

                if (string.IsNullOrWhiteSpace(model))
                {
                    throw new InvalidDataException("No response received.");
                }

                int.TryParse(model, out var result);
                return result;
            }
            set
            {
                this._theSkyXClient.ThrowIfNotAttached(8114);
                this._theSkyXClient.SendToTheSkyX($"AutomatedImageLinkSettings.fovsToSearch = {value};", out var errorMessage);
            }
        }

        public int Retries
        {
            get
            {
                this._theSkyXClient.ThrowIfNotAttached(8114);
                var model = this._theSkyXClient.SendToTheSkyX("AutomatedImageLinkSettings.retries", out var errorMessage);

                if (string.IsNullOrWhiteSpace(model))
                {
                    throw new InvalidDataException("No response received.");
                }

                int.TryParse(model, out var result);
                return result;
            }
            set
            {
                this._theSkyXClient.ThrowIfNotAttached(8114);
                this._theSkyXClient.SendToTheSkyX($"AutomatedImageLinkSettings.retries = {value};", out var errorMessage);
            }
        }

        public string FilterName
        {
            get
            {
                this._theSkyXClient.ThrowIfNotAttached(8114);
                var model = this._theSkyXClient.SendToTheSkyX("AutomatedImageLinkSettings.filterNameAILS", out var errorMessage);

                if (string.IsNullOrWhiteSpace(model))
                {
                    throw new InvalidDataException("No response received.");
                }

                return model;
            }
            set
            {
                this._theSkyXClient.ThrowIfNotAttached(8114);
                this._theSkyXClient.SendToTheSkyX($"AutomatedImageLinkSettings.filterNameAILS = '{value}';", out var errorMessage);
            }
        }
    }
}
