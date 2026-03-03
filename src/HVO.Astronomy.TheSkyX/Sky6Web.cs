using System.IO;
using System.Text;

namespace HVO.Astronomy.TheSkyX
{
    public sealed class Sky6Web
    {
        private readonly TheSkyXClient _theSkyXClient;

        internal Sky6Web(TheSkyXClient theSkyXClient)
        {
            this._theSkyXClient = theSkyXClient;
        }

        public string CreateStarChart(double julianDay, TSWC command, int userId, double latitude, double longitude, double timeZone, int chartWidth, int chartHeight, string outputPath)
        {
            this._theSkyXClient.ThrowIfNotAttached();

            var script = new StringBuilder();
            //script.AppendLine("");
            //script.AppendLine("");
            //script.AppendLine("");
            //script.AppendLine("");
            //script.AppendLine("");
            //script.AppendLine("");

            script.AppendLine($"sky6SWeb.CreateStarChart({julianDay}, {(int)command}, {userId}, {latitude}, {longitude}, {timeZone}, {chartWidth}, {chartHeight}, '{outputPath}');");

            var model = this._theSkyXClient.SendToTheSkyX(script.ToString(), out var errorMessage);
            if (string.IsNullOrWhiteSpace(model))
            {
                throw new InvalidDataException("No response received.");
            }

            return model;
        }

        public void MapCmdTextToTSWC(string text)
        {
        }

        public void SlewToObject(string objectName)
        {
        }

        public void GetThisDayJDAt(double hours, double timeZone)
        {
        }

        public void AbortSlew()
        {
        }

        public string ChartFileName
        {
            get;
            set;
        } = string.Empty;

        public double ChartFieldOfView
        {
            get; set;
        }

        public double ChartRotation
        {
            get; set;
        }

        public string ChartDate
        {
            get; set;
        } = string.Empty;

        public int ChartOrientation
        {
            get;
        }
    }

    public enum TSWC
    {
        TSWC_NONE, TSWC_UNKNOWN, TSWC_NORTH, TSWC_SOUTH,
        TSWC_EAST, TSWC_WEST, TSWC_ZENITH, TSWC_LEFT,
        TSWC_RIGHT, TSWC_UP, TSWC_DOWN, TSWC_ZIN,
        TSWC_ZOUT, TSWC_OBJID, TSWC_PAINT, TSWC_ROTATECCW,
        TSWC_ROTATECW, TSWC_CENTERSCOPE, TSWC_LAST
    }

}
