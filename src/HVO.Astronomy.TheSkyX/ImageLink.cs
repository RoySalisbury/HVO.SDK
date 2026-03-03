using System.IO;
using System.Text;
using System.Text.Json;

namespace HVO.Astronomy.TheSkyX
{
    public sealed class ImageLink
    {
        private readonly TheSkyXClient _theSkyXClient;

        internal ImageLink(TheSkyXClient theSkyXClient)
        {
            this._theSkyXClient = theSkyXClient;
        }

        public void Execute(string pathToFITS, double imageScale = 2.00, bool isUnknownScale = true)
        {
            this._theSkyXClient.ThrowIfNotAttached();

            var script = new StringBuilder();
            script.AppendLine($"ImageLink.scale = {imageScale};");
            script.AppendLine($"ImageLink.unknownScale = {(isUnknownScale ? 1 : 0)};");

            script.AppendLine($"ImageLink.pathToFITS = '{pathToFITS?.Trim()}';");
            script.AppendLine("ImageLink.execute();");

            this._theSkyXClient.SendToTheSkyX(script.ToString(), out var errorMessage);
        }

        public void Execute()
        {
            this._theSkyXClient.ThrowIfNotAttached();
            this._theSkyXClient.SendToTheSkyX("ImageLink.execute()", out var errorMessage);
        }

        public string PathToFITS
        {
            get
            {
                this._theSkyXClient.ThrowIfNotAttached();
                var model = this._theSkyXClient.SendToTheSkyX("ImageLink.pathToFITS", out var errorMessage);

                if (string.IsNullOrWhiteSpace(model))
                {
                    throw new InvalidDataException("No response received.");
                }

                return model;
            }
            set
            {
                this._theSkyXClient.ThrowIfNotAttached();
                this._theSkyXClient.SendToTheSkyX($"ImageLink.pathToFITS = '{value?.Trim()}';", out var errorMessage);
            }
        }

        public double Scale
        {
            get
            {
                this._theSkyXClient.ThrowIfNotAttached();
                var model = this._theSkyXClient.SendToTheSkyX("ImageLink.scale", out var errorMessage);

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
                this._theSkyXClient.SendToTheSkyX($"ImageLink.scale = {value};", out var errorMessage);
            }
        }

        public bool IsUnknownScale
        {
            get
            {
                this._theSkyXClient.ThrowIfNotAttached();
                var model = this._theSkyXClient.SendToTheSkyX("ImageLink.unknownScale", out var errorMessage);

                if (string.IsNullOrWhiteSpace(model))
                {
                    throw new InvalidDataException("No response received.");
                }

                int.TryParse(model, out var result);
                return result == 1;
            }
            set
            {
                this._theSkyXClient.ThrowIfNotAttached();
                this._theSkyXClient.SendToTheSkyX($"ImageLink.unknownScale = {(value ? 1 : 0)};", out var errorMessage);
            }
        }

        public bool IsImageLinkSuccess
        {
            get
            {
                this._theSkyXClient.ThrowIfNotAttached();
                var model = this._theSkyXClient.SendToTheSkyX("ImageLinkResults.succeeded", out var errorMessage);

                if (string.IsNullOrWhiteSpace(model))
                {
                    throw new InvalidDataException("No response received.");
                }

                int.TryParse(model, out var result);
                return result == 1;
            }
        }

        public int LastImageLinkErrorCode
        {
            get
            {
                this._theSkyXClient.ThrowIfNotAttached();
                var model = this._theSkyXClient.SendToTheSkyX("ImageLinkResults.errorCode", out var errorMessage);

                if (string.IsNullOrWhiteSpace(model))
                {
                    throw new InvalidDataException("No response received.");
                }

                int.TryParse(model, out var result);
                return result;
            }
        }

        public ImageLinkResults GetLastImageLinkResults()
        {
            this._theSkyXClient.ThrowIfNotAttached();
            var result = this._theSkyXClient.SendToTheSkyX(Properties.Resources.TheSkyXScript_GetImageLinkResults, 2048, out var errorMessage);

            var model = JsonSerializer.Deserialize<ImageLinkResults>(result, new JsonSerializerOptions() { PropertyNameCaseInsensitive = true });
            if (model == null)
            {
                throw new InvalidDataException("Failed to deserialize ImageLink results.");
            }

            return model;
        }

        public sealed class ImageLinkResults
        {
            public int ErrorCode { get; set; }

            public bool Succeeded { get; set; }

            public bool SearchAborted { get; set; }

            public string ErrorText { get; set; } = string.Empty;

            public double ImageScale { get; set; }

            public double ImagePositionAngle { get; set; }

            public double ImageCenterRAJ2000 { get; set; }

            public double ImageCenterDecJ2000 { get; set; }

            public ImageSize? ImageSize { get; set; }

            public bool IsImageMirrored { get; set; }

            public string ImageFilePath { get; set; } = string.Empty;

            public int ImageStarCount { get; set; }

            public double ImageFWHMInArcSeconds { get; set; }

            public double SolutionRMS { get; set; }

            public double SolutionRMSX { get; set; }

            public double SolutionRMSY { get; set; }

            public int SolutionStarCount { get; set; }

            public int CatalogStarCount { get; set; }
        }
    }
}