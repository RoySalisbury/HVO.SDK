using System.IO;

namespace HVO.Astronomy.TheSkyX
{
    public sealed class CameraDependentSetting
    {
        private readonly TheSkyXClient _theSkyXClient;

        internal CameraDependentSetting(TheSkyXClient theSkyXClient)
        {
            this._theSkyXClient = theSkyXClient;
        }

        public string SettingName
        {
            get
            {
                this._theSkyXClient.ThrowIfNotAttached();
                var model = this._theSkyXClient.SendToTheSkyX("CameraDependentSetting.settingName", out var errorMessage);

                if (string.IsNullOrWhiteSpace(model))
                {
                    throw new InvalidDataException("No response received.");
                }

                return model;
            }
            set
            {
                this._theSkyXClient.ThrowIfNotAttached();
                this._theSkyXClient.SendToTheSkyX($"CameraDependentSetting.settingName = '{value?.Trim()}';", out var errorMessage);
            }
        }

        public bool IsAutoguider
        {
            get
            {
                this._theSkyXClient.ThrowIfNotAttached();
                var model = this._theSkyXClient.SendToTheSkyX("CameraDependentSetting.autoguiderCDS", out var errorMessage);

                if (string.IsNullOrWhiteSpace(model))
                {
                    throw new InvalidDataException("No response received.");
                }

                int.TryParse(model, out var result);
                return result == 1;
            }
        }

        public string CurrentOption
        {
            get
            {
                this._theSkyXClient.ThrowIfNotAttached();
                var model = this._theSkyXClient.SendToTheSkyX("CameraDependentSetting.currentOption", out var errorMessage);

                if (string.IsNullOrWhiteSpace(model))
                {
                    throw new InvalidDataException("No response received.");
                }

                return model;
            }
            set
            {
                this._theSkyXClient.ThrowIfNotAttached();
                this._theSkyXClient.SendToTheSkyX($"CameraDependentSetting.currentOption = '{value?.Trim()}';", out var errorMessage);
            }
        }

        public string[] AvailableOptions()
        {
            this._theSkyXClient.ThrowIfNotAttached();
            var model = this._theSkyXClient.SendToTheSkyX("CameraDependentSetting.availableOptions()", out var errorMessage);

            return model.Split(',');
        }
    }
}