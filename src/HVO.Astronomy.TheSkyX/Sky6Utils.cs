using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.Json;

namespace HVO.Astronomy.TheSkyX
{
    // TODO: Most of these utility methods could be replaced with generic C# only versions.

    public sealed class Sky6Utils
    {
        private readonly TheSkyXClient _theSkyXClient;

        internal Sky6Utils(TheSkyXClient theSkyXClient)
        {
            this._theSkyXClient = theSkyXClient;
        }

        public double ConvertStringToRA(string value)
        {
            this._theSkyXClient.ThrowIfNotAttached();

            var script = new StringBuilder();
            script.AppendLine($"sky6Utils.ConvertStringToRA('{value}');");
            script.AppendLine($"sky6Utils.dOut0");

            var model = this._theSkyXClient.SendToTheSkyX(script.ToString(), out var errorMessage);
            if (string.IsNullOrWhiteSpace(model))
            {
                throw new InvalidDataException("No response received.");
            }

            double.TryParse(model, out var result);
            return result;
        }

        public double ConvertStringToDec(string value)
        {
            this._theSkyXClient.ThrowIfNotAttached();

            var script = new StringBuilder();
            script.AppendLine($"sky6Utils.ConvertStringToDec('{value}');");
            script.AppendLine($"sky6Utils.dOut0");

            var model = this._theSkyXClient.SendToTheSkyX(script.ToString(), out var errorMessage);
            if (string.IsNullOrWhiteSpace(model))
            {
                throw new InvalidDataException("No response received.");
            }

            double.TryParse(model, out var result);
            return result;
        }

        public string ConvertEquatorialToString(double rightAscension, double declination, SkXUtilsSexagesimalSigFigs significantFigures)
        {
            this._theSkyXClient.ThrowIfNotAttached();

            var script = new StringBuilder();
            script.AppendLine($"sky6Utils.ConvertEquatorialToString({rightAscension}, {declination}, {(int)significantFigures});");
            script.AppendLine($"sky6Utils.strOut");

            var model = this._theSkyXClient.SendToTheSkyX(script.ToString(), out var errorMessage);
            if (string.IsNullOrWhiteSpace(model))
            {
                throw new InvalidDataException("No response received.");
            }

            return model;
        }

        public string ConvertHorizonToString(double altitude, double azimuth, SkXUtilsSexagesimalSigFigs significantFigures)
        {
            this._theSkyXClient.ThrowIfNotAttached();

            var script = new StringBuilder();
            script.AppendLine($"sky6Utils.ConvertHorizonToString({azimuth}, {altitude}, {(int)significantFigures});");
            script.AppendLine($"sky6Utils.strOut");

            var model = this._theSkyXClient.SendToTheSkyX(script.ToString(), out var errorMessage);
            if (string.IsNullOrWhiteSpace(model))
            {
                throw new InvalidDataException("No response received.");
            }

            return model;
        }

        public (double Altitude, double Azimuth) ConvertRADecToAzAlt(double rightAscension, double declination)
        {
            this._theSkyXClient.ThrowIfNotAttached();

            var script = new StringBuilder();
            script.AppendLine($"sky6Utils.ConvertRADecToAzAlt({rightAscension}, {declination});");
            script.AppendLine("var objResult = { altitude: sky6Utils.dOut1, azimuth: sky6Utils.dOut0 };");
            script.AppendLine("JSON.stringify(objResult);");

            var model = this._theSkyXClient.SendToTheSkyX(script.ToString(), out var errorMessage);
            if (string.IsNullOrWhiteSpace(model))
            {
                throw new InvalidDataException("No response received.");
            }

            var result = JsonSerializer.Deserialize<AltAz>(model, new JsonSerializerOptions() { PropertyNameCaseInsensitive = true })!;
            return (Altitude: result.Altitude, Azimuth : result.Azimuth);
        }

        public (double RightAscension, double Declination) ConvertAzAltToRADec(double altitude, double azimuth)
        {
            this._theSkyXClient.ThrowIfNotAttached();

            var script = new StringBuilder();
            script.AppendLine($"sky6Utils.ConvertAzAltToRADec({azimuth}, {altitude});");
            script.AppendLine("var objResult = { rightAscension: sky6Utils.dOut0, declination: sky6Utils.dOut1 };");
            script.AppendLine("JSON.stringify(objResult);");

            var model = this._theSkyXClient.SendToTheSkyX(script.ToString(), out var errorMessage);
            if (string.IsNullOrWhiteSpace(model))
            {
                throw new InvalidDataException("No response received.");
            }

            var result = JsonSerializer.Deserialize<RaDec>(model, new JsonSerializerOptions() { PropertyNameCaseInsensitive = true })!;
            return (RightAscension: result.RightAscension, Declination: result.Declination);
        }

        public DateTime ConvertJulianDateToCalender(double julianDay)
        {
            this._theSkyXClient.ThrowIfNotAttached();

            var script = new StringBuilder();
            script.AppendLine($"sky6Utils.ConvertJulianDateToCalender({julianDay});");
            script.AppendLine("var date = new Date(sky6Utils.dOut0, sky6Utils.dOut1, sky6Utils.dOut2, sky6Utils.dOut3, sky6Utils.dOut4, sky6Utils.dOut5);");
            script.AppendLine("date.toJSON()");

            var model = this._theSkyXClient.SendToTheSkyX(script.ToString(), out var errorMessage);
            if (string.IsNullOrWhiteSpace(model))
            {
                throw new InvalidDataException("No response received.");
            }

            DateTime.TryParse(model, out var result);
            return result;
        }

        public double ConvertCalenderToJulianDate(int year, int month, int day, int hour, int minute, double second)
        {
            this._theSkyXClient.ThrowIfNotAttached();

            var script = new StringBuilder();
            script.AppendLine($"sky6Utils.ConvertCalenderToJulianDate({year}, {month}, {day}, {hour}, {minute}, {second});");
            script.AppendLine("sky6Utils.dOut0");

            var model = this._theSkyXClient.SendToTheSkyX(script.ToString(), out var errorMessage);
            if (string.IsNullOrWhiteSpace(model))
            {
                throw new InvalidDataException("No response received.");
            }

            double.TryParse(model, out var result);
            return result;
        }

        public double ConvertDMSToAngle(int degrees, int minutes, double seconds)
        {
            this._theSkyXClient.ThrowIfNotAttached();

            var script = new StringBuilder();
            script.AppendLine($"sky6Utils.ConvertDMSToAngle({degrees}, {minutes}, {seconds});");
            script.AppendLine("sky6Utils.dOut0");

            var model = this._theSkyXClient.SendToTheSkyX(script.ToString(), out var errorMessage);
            if (string.IsNullOrWhiteSpace(model))
            {
                throw new InvalidDataException("No response received.");
            }

            double.TryParse(model, out var result);
            return result;
        }

        public (int Degrees, int Minutes, double Seconds) ConvertAngleToDMS(double angle)
        {
            this._theSkyXClient.ThrowIfNotAttached();

            var script = new StringBuilder();
            script.AppendLine($"sky6Utils.ConvertAngleToDMS({angle});");
            script.AppendLine("var objResult = { degrees : sky6Utils.dOut0, minutes : sky6Utils.dOut1, seconds : sky6Utils.dOut2 }");
            script.AppendLine("JSON.stringify(objResult);");

            var model = this._theSkyXClient.SendToTheSkyX(script.ToString(), out var errorMessage);
            if (string.IsNullOrWhiteSpace(model))
            {
                throw new InvalidDataException("No response received.");
            }

            var result = JsonSerializer.Deserialize<DegMinSec>(model, new JsonSerializerOptions() { PropertyNameCaseInsensitive = true })!;
            return (Degrees: result.Degrees, Minutes: result.Minutes, Seconds: result.Seconds);
        }

        public string ConvertSexagesimalToString(double value, SkXUtilsSexagesimalFormat format, SkXUtilsSexagesimalSigFigs significantFigures)
        {
            this._theSkyXClient.ThrowIfNotAttached();

            var script = new StringBuilder();
            script.AppendLine($"sky6Utils.ConvertSexagesimalToString({value}, {(int)format}, {(int)significantFigures});");
            script.AppendLine("sky6Utils.strOut");

            var model = this._theSkyXClient.SendToTheSkyX(script.ToString(), out var errorMessage);
            if (string.IsNullOrWhiteSpace(model))
            {
                throw new InvalidDataException("No response received.");
            }

            return model;
        }

        public double ComputePositionAngle(double rightAscension1, double declination1, double rightAscension2, double declination2)
        {
            this._theSkyXClient.ThrowIfNotAttached();

            var script = new StringBuilder();
            script.AppendLine($"sky6Utils.ComputePositionAngle({rightAscension1}, {declination1}, {rightAscension2}, {declination2});");
            script.AppendLine("sky6Utils.dOut0");

            var model = this._theSkyXClient.SendToTheSkyX(script.ToString(), out var errorMessage);
            if (string.IsNullOrWhiteSpace(model))
            {
                throw new InvalidDataException("No response received.");
            }

            double.TryParse(model, out var result);
            return result;
        }

        public double ComputeAngularSeparation(double rightAscension1, double declination1, double rightAscension2, double declination2)
        {
            this._theSkyXClient.ThrowIfNotAttached();

            var script = new StringBuilder();
            script.AppendLine($"sky6Utils.ComputeAngularSeparation({rightAscension1}, {declination1}, {rightAscension2}, {declination2});");
            script.AppendLine("sky6Utils.dOut0");

            var model = this._theSkyXClient.SendToTheSkyX(script.ToString(), out var errorMessage);
            if (string.IsNullOrWhiteSpace(model))
            {
                throw new InvalidDataException("No response received.");
            }

            double.TryParse(model, out var result);
            return result;
        }

        public double ComputeHourAngle(double rightAscension)
        {
            this._theSkyXClient.ThrowIfNotAttached();

            var script = new StringBuilder();
            script.AppendLine($"sky6Utils.ComputeHourAngle({rightAscension});");
            script.AppendLine("sky6Utils.dOut0");

            var model = this._theSkyXClient.SendToTheSkyX(script.ToString(), out var errorMessage);
            if (string.IsNullOrWhiteSpace(model))
            {
                throw new InvalidDataException("No response received.");
            }

            double.TryParse(model, out var result);
            return result;
        }

        public double ComputeAirMass(double altitude)
        {
            this._theSkyXClient.ThrowIfNotAttached();

            var script = new StringBuilder();
            script.AppendLine($"sky6Utils.ComputeAirMass({altitude});");
            script.AppendLine("sky6Utils.dOut0");

            var model = this._theSkyXClient.SendToTheSkyX(script.ToString(), out var errorMessage);
            if (string.IsNullOrWhiteSpace(model))
            {
                throw new InvalidDataException("No response received.");
            }

            double.TryParse(model, out var result);
            return result;
        }

        public double ComputeLocalSiderealTime()
        {
            this._theSkyXClient.ThrowIfNotAttached();

            var script = new StringBuilder();
            script.AppendLine($"sky6Utils.ComputeLocalSiderealTime();");
            script.AppendLine("sky6Utils.dOut0");

            var model = this._theSkyXClient.SendToTheSkyX(script.ToString(), out var errorMessage);
            if (string.IsNullOrWhiteSpace(model))
            {
                throw new InvalidDataException("No response received.");
            }

            double.TryParse(model, out var result);
            return result;
        }

        public TimeSpan ComputeUniversalTime()
        {
            this._theSkyXClient.ThrowIfNotAttached();

            var script = new StringBuilder();
            script.AppendLine($"sky6Utils.ComputeUniversalTime();");
            script.AppendLine("sky6Utils.dOut0");

            var model = this._theSkyXClient.SendToTheSkyX(script.ToString(), out var errorMessage);
            if (string.IsNullOrWhiteSpace(model))
            {
                throw new InvalidDataException("No response received.");
            }

            double.TryParse(model, out var result);
            return TimeSpan.FromHours(result);
        }

        public double ComputeJulianDate()
        {
            this._theSkyXClient.ThrowIfNotAttached();

            var script = new StringBuilder();
            script.AppendLine($"sky6Utils.ComputeJulianDate();");
            script.AppendLine("sky6Utils.dOut0");

            var model = this._theSkyXClient.SendToTheSkyX(script.ToString(), out var errorMessage);
            if (string.IsNullOrWhiteSpace(model))
            {
                throw new InvalidDataException("No response received.");
            }

            double.TryParse(model, out var result);
            return result;
        }

        public double ComputeRefraction(double altitude)
        {
            this._theSkyXClient.ThrowIfNotAttached();

            var script = new StringBuilder();
            script.AppendLine($"sky6Utils.ComputeRefraction({altitude});");
            script.AppendLine("sky6Utils.dOut0");

            var model = this._theSkyXClient.SendToTheSkyX(script.ToString(), out var errorMessage);
            if (string.IsNullOrWhiteSpace(model))
            {
                throw new InvalidDataException("No response received.");
            }

            double.TryParse(model, out var result);
            return result;
        }

        public (TimeSpan Rise, TimeSpan Transit, TimeSpan Set) ComputeRiseTransitSetTimes(double rightAscension, double declination)
        {
            this._theSkyXClient.ThrowIfNotAttached();

            var script = new StringBuilder();
            script.AppendLine($"sky6Utils.ComputeRiseTransitSetTimes({rightAscension}, {declination});");
            script.AppendLine("var objResult = { rise: sky6Utils.dOut0, transit: sky6Utils.dOut1, set: sky6Utils.dOut2 }");
            script.AppendLine("JSON.stringify(objResult);");

            var model = this._theSkyXClient.SendToTheSkyX(script.ToString(), out var errorMessage);
            if (string.IsNullOrWhiteSpace(model))
            {
                throw new InvalidDataException("No response received.");
            }

            var result = JsonSerializer.Deserialize<RiseTransitSetTimes>(model, new JsonSerializerOptions() { PropertyNameCaseInsensitive = true })!;
            return (Rise: TimeSpan.FromHours(result.Rise), Transit: TimeSpan.FromHours(result.Transit), Set: TimeSpan.FromHours(result.Set));
        }

        public (double RightAscension, double Declination) PrecessNowTo2000(double rightAscension, double declination)
        {
            this._theSkyXClient.ThrowIfNotAttached();

            var script = new StringBuilder();
            script.AppendLine($"sky6Utils.PrecessNowTo2000({rightAscension}, {declination});");
            script.AppendLine("var objResult = { rightAscension: sky6Utils.dOut0, declination: sky6Utils.dOut1 }");
            script.AppendLine("JSON.stringify(objResult);");

            var model = this._theSkyXClient.SendToTheSkyX(script.ToString(), out var errorMessage);
            if (string.IsNullOrWhiteSpace(model))
            {
                throw new InvalidDataException("No response received.");
            }

            var result = JsonSerializer.Deserialize<RaDec>(model, new JsonSerializerOptions() { PropertyNameCaseInsensitive = true })!;
            return (RightAscension : result.RightAscension, Declination: result.Declination);
        }

        public (double RightAscension, double Declination) Precess2000ToNow(double rightAscension, double declination)
        {
            this._theSkyXClient.ThrowIfNotAttached();

            var script = new StringBuilder();
            script.AppendLine($"sky6Utils.Precess2000ToNow({rightAscension}, {declination});");
            script.AppendLine("var objResult = { rightAscension: sky6Utils.dOut0, declination: sky6Utils.dOut1 }");
            script.AppendLine("JSON.stringify(objResult);");

            var model = this._theSkyXClient.SendToTheSkyX(script.ToString(), out var errorMessage);
            if (string.IsNullOrWhiteSpace(model))
            {
                throw new InvalidDataException("No response received.");
            }

            var result = JsonSerializer.Deserialize<RaDec>(model, new JsonSerializerOptions() { PropertyNameCaseInsensitive = true })!;
            return (RightAscension: result.RightAscension, Declination: result.Declination);
        }

        // This is just a private helper class to aid in deserialization.  The MS provided deserialization does not yet support dynamic objects (ExpandoObject).
        private class RaDec
        {
            public double RightAscension
            {
                get; set;
            }
            public double Declination
            {
                get; set;
            }
        }

        // This is just a private helper class to aid in deserialization.  The MS provided deserialization does not yet support dynamic objects (ExpandoObject).
        private class AltAz
        {
            public double Altitude
            {
                get; set;
            }
            public double Azimuth
            {
                get; set;
            }
        }

        // This is just a private helper class to aid in deserialization.  The MS provided deserialization does not yet support dynamic objects (ExpandoObject).
        private class DegMinSec
        {
            public int Degrees
            {
                get; set;
            }

            public int Minutes
            {
                get; set;
            }

            public double Seconds
            {
                get; set;
            }
        }

        private class RiseTransitSetTimes
        {
            public double Rise
            {
                get; set;
            }
            public double Transit
            {
                get; set;
            }
            public double Set
            {
                get; set;
            }

        }
    }

    public enum SkXUtilsSexagesimalFormat
    {
        FMT_DMS = 0, 
        FMT_DMS_SIGN = 1, 
        FMT_DMS_NS = 2, 
        FMT_DMS_EW = 3,
        FMT_HMS = 4, 
        FMT_TIME = 5, 
        FMT_DMS_MINIM = 6, 
        FMT_COMMAS = 7,
        FMT_NOCOMMAS = 8
    }

    public enum SkXUtilsSexagesimalSigFigs
    {
        SSF_ONE = 0, 
        SSF_TWO = 1, 
        SSF_THREE = 2, 
        SSF_FOUR = 3,
        SSF_FIVE = 4, 
        SSF_SIX = 5,
        SSF_SEVEN = 6, 
        SSF_EIGHT = 7
    }
}
