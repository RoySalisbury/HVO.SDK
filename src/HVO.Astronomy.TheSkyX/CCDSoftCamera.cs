using HVO.Astronomy.TheSkyX.Serialization;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace HVO.Astronomy.TheSkyX
{
    public abstract class ___CCDSoftCameraBase
    {
        protected readonly TheSkyXClient _theSkyXClient;
        protected readonly string _cameraScriptName;

        protected ___CCDSoftCameraBase(TheSkyXClient theSkyXClient, string cameraScriptName)
        {
            this._theSkyXClient = theSkyXClient;
            this._cameraScriptName = cameraScriptName;

            this.Focuser = new ___CCDSoftFocuser(theSkyXClient, cameraScriptName);
            this.FilterWheel = new ___CCDSoftFilterWheel(theSkyXClient, cameraScriptName);
            this.Rotator = new ___CCDSoftRotator(theSkyXClient, cameraScriptName);
        }

        public bool IsConnected
        {
            get
            {
                this._theSkyXClient.ThrowIfNotAttached();

                var model = this._theSkyXClient.SendToTheSkyX($"{this._cameraScriptName}.Status != 'Not Connected'", out var errorMessage);
                if (string.IsNullOrWhiteSpace(model))
                {
                    throw new InvalidDataException("No response received.");
                }

                bool.TryParse(model, out var result);
                return result;
            }
        }

        public (bool IsRegulated, double Temperature, double SetPoint, double TECPower) GetTemperatureInfo()
        {
            this._theSkyXClient.ThrowIfNotAttached();
            //this.ThowIfNotConnected();

            StringBuilder script = new StringBuilder();
            script.AppendLine($"var isRegulated = {this._cameraScriptName}.RegulateTemperature;");
            script.AppendLine($"var temperature = {this._cameraScriptName}.Temperature;");
            script.AppendLine($"var setPoint = {this._cameraScriptName}.TemperatureSetPoint;");
            script.AppendLine($"var tecPower = {this._cameraScriptName}.ThermalElectricCoolerPower;");

            script.AppendLine("var objResult = { isRegulated : isRegulated, temperature : temperature, setPoint : setPoint, tecPower: tecPower };");
            script.AppendLine("JSON.stringify(objResult);");

            var model = this._theSkyXClient.SendToTheSkyX(script.ToString(), out var errorMessage);
            if (string.IsNullOrWhiteSpace(model))
            {
                throw new InvalidDataException("No response received.");
            }

            var result = JsonSerializer.Deserialize<CameraTemperature>(model, new JsonSerializerOptions() { PropertyNameCaseInsensitive = true });
            return (result.IsRegulated == 1, result.Temperature, result.SetPoint, result.TECPower);
        }

        public void Connect()
        {
            this._theSkyXClient.ThrowIfNotAttached();
            this._theSkyXClient.SendToTheSkyX($"{this._cameraScriptName}.Connect();", out var errorMessage);
        }

        public void Disconnect()
        {
            this._theSkyXClient.ThrowIfNotAttached();
            this._theSkyXClient.SendToTheSkyX($"{this._cameraScriptName}.Disconnect();", out var errorMessage);
        }

        public void SetTemperatureRegulation(bool enabled, double setPoint)
        {
            this._theSkyXClient.ThrowIfNotAttached();
            //this.ThowIfNotConnected();

            StringBuilder script = new StringBuilder();
            script.AppendLine($"{this._cameraScriptName}.TemperatureSetPoint = {setPoint}");
            script.AppendLine($"{this._cameraScriptName}.RegulateTemperature = {(enabled ? 1 : 0)}");

            this._theSkyXClient.SendToTheSkyX(script.ToString(), out var errorMessage);
        }

        public void Abort()
        {
            this._theSkyXClient.ThrowIfNotAttached();
            var result = this._theSkyXClient.SendToTheSkyX($"{this._cameraScriptName}.Abort();", out var errorMessage);
            Console.WriteLine(result);
        }

        public IEnumerable<KeyValuePair<int, string>> GetBinList()
        {
            this._theSkyXClient.ThrowIfNotAttached();
            var script = new StringBuilder();
            script.AppendLine($"var camera = {this._cameraScriptName};");
            script.AppendLine(Properties.Resources.TheSkyXScript_GetBinList);

            var model = this._theSkyXClient.SendToTheSkyX(script.ToString(), 4096, out var errorMessage);
            if (string.IsNullOrWhiteSpace(model))
            {
                throw new InvalidDataException("No response received.");
            }

            var serializerOptions = new JsonSerializerOptions() { PropertyNameCaseInsensitive = true };
            serializerOptions.Converters.Add(new DictionaryKeyValueConverter());

            var result = JsonSerializer.Deserialize<Dictionary<int, string>>(model, serializerOptions);
            if (result == null)
            {
                return new Dictionary<int, string>();
            }

            return result;
        }


        public bool TakeSingleImage(double exposureTime, double? exposureDelay = null, int? binning = null, int? filterIndex = null, int? frameType = null, int? imageReduction = null, ImageSize? subframe = null, bool? simulateUsingDSS = false, CancellationToken cancellationToken = default)
        {
            this._theSkyXClient.ThrowIfNotAttached();
            this.ThrowIfNotConnected();

            // When taking an image, we need to 

            StringBuilder script = new StringBuilder();
            script.AppendLine($"if ({this._cameraScriptName}.State != 0) {{ var result = 1; }} else {{");
            script.AppendLine($"{this._cameraScriptName}.Asynchronous = true;");
            script.AppendLine($"{this._cameraScriptName}.ExposureTime = {exposureTime};");
            script.AppendLine($"{this._cameraScriptName}.AutoSaveOn = true;");

            if (exposureDelay.HasValue)
            {
                script.AppendLine($"{this._cameraScriptName}.Delay = {exposureDelay.Value};");
            }

            if (filterIndex.HasValue)
            {
                script.AppendLine($"{this._cameraScriptName}.FilterIndexZeroBased = {filterIndex.Value};");
            }

            if (frameType.HasValue)
            {
                script.AppendLine($"{this._cameraScriptName}.Frame = {frameType.Value};");
            }

            if (imageReduction.HasValue)
            {
                script.AppendLine($"{this._cameraScriptName}.ImageReduction = {imageReduction.Value};");
            }

            if (simulateUsingDSS.HasValue)
            {
                script.AppendLine($"{this._cameraScriptName}.ImageUseDigitizedSkySurvey = {(simulateUsingDSS.Value ? 1 : 0)};");
            }

            script.AppendLine($"{this._cameraScriptName}.TakeImage() == 0");
            script.AppendLine("}");

            var model = this._theSkyXClient.SendToTheSkyX(script.ToString(), out var errorMessage);
            if (string.IsNullOrWhiteSpace(model))
            {
                throw new InvalidDataException("No response received.");
            }

            bool.TryParse(model, out var result);
            if (result == true)
            {
                var takeImageStatusTask = Task.Run(async () =>
                {
                    while (cancellationToken.IsCancellationRequested == false)
                    {
                        //break;
                        try
                        {
                            // From here we start monitoring the status of the TakeImage status.  When its comleted, we will fire off an event that tells us the image status, and the path to the image (and possibly the image data).
                            // We also need to check to see if the image was aborted before completion, if the camera was disconnected, and other errors.  We will fire an event for these as well.  The idea is to get the event back to 
                            // the caller so that further images can be taken.
                            var imageStatusScript = new StringBuilder();
                            imageStatusScript.AppendLine($"var camera = {this._cameraScriptName};");
                            imageStatusScript.AppendLine(Properties.Resources.TheSkyXScript_TakeImageStatus);

                            var statusModel = this._theSkyXClient.SendToTheSkyX(imageStatusScript.ToString(), out errorMessage);
                            if (string.IsNullOrWhiteSpace(statusModel))
                            {
                                throw new InvalidDataException("No response received.");
                            }

                            var imageStatus = JsonSerializer.Deserialize<ImageStatusResult>(statusModel, new JsonSerializerOptions() { PropertyNameCaseInsensitive = true });

                            if ((imageStatus.ExposureComplete) && (imageStatus.ExposureSuccess))
                            {
                                // fire the event that indicates a successfully completed image 
                                break;
                            }
                            else if ((imageStatus.ExposureComplete) && (imageStatus.ExposureSuccess == false))
                            {
                                // Camera is no longer connected.  Fire the event that indicates the unsuccessful image due to disconnet of hardware
                                break;
                            }
                            else if (imageStatus.ExposureComplete == false)
                            {
                                // Not done yet...
                            }

                            // Only update the status every 500 ms.
                            await Task.Delay(500, cancellationToken);
                        }
                        catch (TaskCanceledException)
                        {
                            // The image was cancelled (system shutdown?).
                            break;
                        }
                        catch (InvalidDataException)
                        {
                            // Unable to parse the response
                            break;
                        }
                        catch (TheSkyXException ex) when (ex.ErrorCode == 212) // Abort
                        {
                            // The image was aborted
                            break;
                        }
                        catch (Exception)
                        {
                            break;
                        }
                        finally { }
                    }
                }, cancellationToken);

                // if '0' is received, then start the task that will monitor for the completion of the image so we can get notified when its done.
            }
            else
            {
                // Should we fire an event here (ImageComplete) with a status of "fail"?
                Console.WriteLine("Image session in progress");
            }
            return result;
        }

        private class ImageStatusResult
        {
            public bool ExposureComplete
            {
                get; set;
            }

            public bool ExposureSuccess
            {
                get; set;
            }

            public string FileName
            {
                get; set;
            } = string.Empty;
        }



        public ___CCDSoftFocuser Focuser
        {
            get;
        }

        public ___CCDSoftFilterWheel FilterWheel
        {
            get;
        }

        public ___CCDSoftRotator Rotator
        {
            get;
        }


        protected void ThrowIfNotConnected()
        {
            if (this.IsConnected == false)
            {
                throw new InvalidOperationException("TheSkyX camera not been connected.");
            }
        }

        private class CameraTemperature
        {
            public int IsRegulated
            {
                get; set;
            }

            public double Temperature
            {
                get; set;
            }
            public double SetPoint
            {
                get; set;
            }

            public double TECPower
            {
                get; set;
            }
        }
    }

    public sealed class ___CCDSoftAutoGuiderBase
    {
        private readonly TheSkyXClient _theSkyXClient;
        private readonly string _cameraScriptName;

        internal ___CCDSoftAutoGuiderBase(TheSkyXClient theSkyXClient, string cameraScriptName)
        {
            this._theSkyXClient = theSkyXClient;
            this._cameraScriptName = cameraScriptName;
        }

        public int Autoguide()
        {
            this._theSkyXClient.ThrowIfNotAttached();
            var model = this._theSkyXClient.SendToTheSkyX($"{this._cameraScriptName}.AutoGuide();", out var errorMessage);

            if (string.IsNullOrWhiteSpace(model))
            {
                throw new InvalidDataException("No response received.");
            }

            int.TryParse(model, out var result);
            return result;
        }

        public int CenterBrightestObject()
        {
            this._theSkyXClient.ThrowIfNotAttached();
            var model = this._theSkyXClient.SendToTheSkyX($"{this._cameraScriptName}.CenterBrightestObject();", out var errorMessage);

            if (string.IsNullOrWhiteSpace(model))
            {
                throw new InvalidDataException("No response received.");
            }

            int.TryParse(model, out var result);
            return result;
        }

        public int Calibrate(bool calibrateAO = false)
        {
            this._theSkyXClient.ThrowIfNotAttached();
            var model = this._theSkyXClient.SendToTheSkyX($"{this._cameraScriptName}.Calibrate({(calibrateAO ? 1 : 0)});", out var errorMessage);

            if (string.IsNullOrWhiteSpace(model))
            {
                throw new InvalidDataException("No response received.");
            }

            int.TryParse(model, out var result);
            return result;
        }

        public int MoveGuideStar()
        {
            this._theSkyXClient.ThrowIfNotAttached();
            var model = this._theSkyXClient.SendToTheSkyX($"{this._cameraScriptName}.MoveGuideStar();", out var errorMessage);

            if (string.IsNullOrWhiteSpace(model))
            {
                throw new InvalidDataException("No response received.");
            }

            int.TryParse(model, out var result);
            return result;
        }

        public int centerAO()
        {
            this._theSkyXClient.ThrowIfNotAttached(7968);
            var model = this._theSkyXClient.SendToTheSkyX($"{this._cameraScriptName}.centerAO();", out var errorMessage);

            if (string.IsNullOrWhiteSpace(model))
            {
                throw new InvalidDataException("No response received.");
            }

            int.TryParse(model, out var result);
            return result;
        }

        public int Move(double fromX, double fromY, double toX, double toY)
        {
            this._theSkyXClient.ThrowIfNotAttached();
            var model = this._theSkyXClient.SendToTheSkyX($"{this._cameraScriptName}.Move({fromX}, {fromY}, {toX}, {toY});", out var errorMessage);

            if (string.IsNullOrWhiteSpace(model))
            {
                throw new InvalidDataException("No response received.");
            }

            int.TryParse(model, out var result);
            return result;
        }
    }

    public sealed class ___CCDSoftFocuser
    {
        private readonly TheSkyXClient _theSkyXClient;
        private readonly string _cameraScriptName;

        internal ___CCDSoftFocuser(TheSkyXClient theSkyXClient, string cameraScriptName)
        {
            this._theSkyXClient = theSkyXClient;
            this._cameraScriptName = cameraScriptName;
        }

        #region Focuser

        public int focConnect()
        {
            this._theSkyXClient.ThrowIfNotAttached();
            var model = this._theSkyXClient.SendToTheSkyX($"{this._cameraScriptName}.focConnect();", out var errorMessage);

            if (string.IsNullOrWhiteSpace(model))
            {
                throw new InvalidDataException("No response received.");
            }

            int.TryParse(model, out var result);
            return result;
        }

        public int focDisconnect()
        {
            this._theSkyXClient.ThrowIfNotAttached();
            var model = this._theSkyXClient.SendToTheSkyX($"{this._cameraScriptName}.focDisconnect();", out var errorMessage);

            if (string.IsNullOrWhiteSpace(model))
            {
                throw new InvalidDataException("No response received.");
            }

            int.TryParse(model, out var result);
            return result;
        }


        public int focMoveIn(int steps)
        {
            this._theSkyXClient.ThrowIfNotAttached();
            var model = this._theSkyXClient.SendToTheSkyX($"{this._cameraScriptName}.focMoveIn({steps});", out var errorMessage);

            if (string.IsNullOrWhiteSpace(model))
            {
                throw new InvalidDataException("No response received.");
            }

            int.TryParse(model, out var result);
            return result;
        }

        public int focMoveOut(int steps)
        {
            this._theSkyXClient.ThrowIfNotAttached();
            var model = this._theSkyXClient.SendToTheSkyX($"{this._cameraScriptName}.focMoveOut({steps});", out var errorMessage);

            if (string.IsNullOrWhiteSpace(model))
            {
                throw new InvalidDataException("No response received.");
            }

            int.TryParse(model, out var result);
            return result;
        }

        public int AtFocus()
        {
            this._theSkyXClient.ThrowIfNotAttached();
            var model = this._theSkyXClient.SendToTheSkyX($"{this._cameraScriptName}.AtFocus());", out var errorMessage);

            if (string.IsNullOrWhiteSpace(model))
            {
                throw new InvalidDataException("No response received.");
            }

            int.TryParse(model, out var result);
            return result;
        }

        public int AtFocus2()
        {
            this._theSkyXClient.ThrowIfNotAttached();
            var model = this._theSkyXClient.SendToTheSkyX($"{this._cameraScriptName}.AtFocus2());", out var errorMessage);

            if (string.IsNullOrWhiteSpace(model))
            {
                throw new InvalidDataException("No response received.");
            }

            int.TryParse(model, out var result);
            return result;
        }

        public int AtFocus3(int averageImages, bool fullAuto)
        {
            this._theSkyXClient.ThrowIfNotAttached();
            var model = this._theSkyXClient.SendToTheSkyX($"{this._cameraScriptName}.AtFocus3({averageImages}, {fullAuto}));", out var errorMessage);

            if (string.IsNullOrWhiteSpace(model))
            {
                throw new InvalidDataException("No response received.");
            }

            int.TryParse(model, out var result);
            return result;
        }

        #endregion
    }

    public sealed class ___CCDSoftFilterWheel
    {
        private readonly TheSkyXClient _theSkyXClient;
        private readonly string _cameraScriptName;

        internal ___CCDSoftFilterWheel(TheSkyXClient theSkyXClient, string cameraScriptName)
        {
            this._theSkyXClient = theSkyXClient;
            this._cameraScriptName = cameraScriptName;
        }

        public int Connect()
        {
            this._theSkyXClient.ThrowIfNotAttached();
            var model = this._theSkyXClient.SendToTheSkyX($"{this._cameraScriptName}.filterWheelConnect());", out var errorMessage);

            if (string.IsNullOrWhiteSpace(model))
            {
                throw new InvalidDataException("No response received.");
            }

            int.TryParse(model, out var result);
            return result;
        }

        public int Disconnect()
        {
            this._theSkyXClient.ThrowIfNotAttached();
            var model = this._theSkyXClient.SendToTheSkyX($"{this._cameraScriptName}.filterWheelDisconnect());", out var errorMessage);

            if (string.IsNullOrWhiteSpace(model))
            {
                throw new InvalidDataException("No response received.");
            }

            int.TryParse(model, out var result);
            return result;
        }

        public bool IsConnected()
        {
            this._theSkyXClient.ThrowIfNotAttached();
            var model = this._theSkyXClient.SendToTheSkyX($"{this._cameraScriptName}.filterWheelIsConnected());", out var errorMessage);

            if (string.IsNullOrWhiteSpace(model))
            {
                throw new InvalidDataException("No response received.");
            }

            int.TryParse(model, out var result);
            return result == 0;
        }

        public int GetNumberOfFilters()
        {
            this._theSkyXClient.ThrowIfNotAttached();
            var model = this._theSkyXClient.SendToTheSkyX($"{this._cameraScriptName}.lNumberFilters);", out var errorMessage);

            if (string.IsNullOrWhiteSpace(model))
            {
                throw new InvalidDataException("No response received.");
            }

            int.TryParse(model, out var result);
            return result;
        }

        public string GetFilterName(int index)
        {
            this._theSkyXClient.ThrowIfNotAttached();
            var model = this._theSkyXClient.SendToTheSkyX($"{this._cameraScriptName}.szFilterName({index}));", out var errorMessage);

            if (string.IsNullOrWhiteSpace(model))
            {
                throw new InvalidDataException("No response received.");
            }

            return model;
        }

        public void SetFilterName(int index, string value)
        {
            this._theSkyXClient.ThrowIfNotAttached();
            var model = this._theSkyXClient.SendToTheSkyX($"{this._cameraScriptName}.setszFilterName({index}, '{value}'));", out var errorMessage);

            if (string.IsNullOrWhiteSpace(model))
            {
                throw new InvalidDataException("No response received.");
            }
        }

        public IEnumerable<KeyValuePair<int, string>> GetFilterList()
        {
            this._theSkyXClient.ThrowIfNotAttached();
            var script = new StringBuilder();
            script.AppendLine($"var camera = {this._cameraScriptName};");
            script.AppendLine(Properties.Resources.TheSkyXScript_GetFilterList);

            var model = this._theSkyXClient.SendToTheSkyX(script.ToString(), 4096, out var errorMessage);
            if (string.IsNullOrWhiteSpace(model))
            {
                throw new InvalidDataException("No response received.");
            }

            var serializerOptions = new JsonSerializerOptions() { PropertyNameCaseInsensitive = true };
            serializerOptions.Converters.Add(new DictionaryKeyValueConverter());

            var result = JsonSerializer.Deserialize<Dictionary<int, string>>(model, serializerOptions);
            if (result == null)
            {
                return new Dictionary<int, string>();
            }

            return result;
        }

        public string ReductionGroupFromIndex(int index)
        {
            this._theSkyXClient.ThrowIfNotAttached();
            var model = this._theSkyXClient.SendToTheSkyX($"{this._cameraScriptName}.ReductionGroupFromIndex({index}));", out var errorMessage);

            if (string.IsNullOrWhiteSpace(model))
            {
                throw new InvalidDataException("No response received.");
            }

            return model;
        }
    }

    public sealed class ___CCDSoftRotator
    {
        private readonly TheSkyXClient _theSkyXClient;
        private readonly string _cameraScriptName;

        internal ___CCDSoftRotator(TheSkyXClient theSkyXClient, string cameraScriptName)
        {
            this._theSkyXClient = theSkyXClient;
            this._cameraScriptName = cameraScriptName;
        }

        #region Rotator

        public int rotatorConnect()
        {
            this._theSkyXClient.ThrowIfNotAttached();
            var model = this._theSkyXClient.SendToTheSkyX($"{this._cameraScriptName}.rotatorConnect();", out var errorMessage);

            if (string.IsNullOrWhiteSpace(model))
            {
                throw new InvalidDataException("No response received.");
            }

            int.TryParse(model, out var result);
            return result;
        }

        public int rotatorDisconnect()
        {
            this._theSkyXClient.ThrowIfNotAttached();
            var model = this._theSkyXClient.SendToTheSkyX($"{this._cameraScriptName}.rotatorDisconnect();", out var errorMessage);

            if (string.IsNullOrWhiteSpace(model))
            {
                throw new InvalidDataException("No response received.");
            }

            int.TryParse(model, out var result);
            return result;
        }

        public int rotatorIsConnected()
        {
            this._theSkyXClient.ThrowIfNotAttached();
            var model = this._theSkyXClient.SendToTheSkyX($"{this._cameraScriptName}.rotatorIsConnected();", out var errorMessage);

            if (string.IsNullOrWhiteSpace(model))
            {
                throw new InvalidDataException("No response received.");
            }

            int.TryParse(model, out var result);
            return result;
        }

        public double rotatorPositionAngle()
        {
            this._theSkyXClient.ThrowIfNotAttached();
            var model = this._theSkyXClient.SendToTheSkyX($"{this._cameraScriptName}.rotatorPositionAngle();", out var errorMessage);

            if (string.IsNullOrWhiteSpace(model))
            {
                throw new InvalidDataException("No response received.");
            }

            double.TryParse(model, out var result);
            return result;
        }

        public int rotatorGotoPositionAngle(double positionAngle)
        {
            this._theSkyXClient.ThrowIfNotAttached();
            var model = this._theSkyXClient.SendToTheSkyX($"{this._cameraScriptName}.rotatorGotoPositionAngle((positionAngle);", out var errorMessage);

            if (string.IsNullOrWhiteSpace(model))
            {
                throw new InvalidDataException("No response received.");
            }

            int.TryParse(model, out var result);
            return result;
        }

        public int rotatorIsRotating()
        {
            this._theSkyXClient.ThrowIfNotAttached();
            var model = this._theSkyXClient.SendToTheSkyX($"{this._cameraScriptName}.rotatorIsRotating();", out var errorMessage);

            if (string.IsNullOrWhiteSpace(model))
            {
                throw new InvalidDataException("No response received.");
            }

            int.TryParse(model, out var result);
            return result;
        }
        #endregion
    }



    public sealed class TheSkyXMainCamera : ___CCDSoftCameraBase
    {
        private const string SCRIPTNAME = "ccdsoftCamera";

        internal TheSkyXMainCamera(TheSkyXClient theSkyXClient) : base(theSkyXClient, SCRIPTNAME)
        {
            this.Autoguider = new ___CCDSoftAutoGuiderBase(theSkyXClient, $"{SCRIPTNAME}.Autoguider");
        }

        // Exposes Autoguider as an instance 
        public ___CCDSoftAutoGuiderBase Autoguider
        {
            get;
        }
    }

    public sealed class TheSkyXAutoguiderCamera : ___CCDSoftCameraBase
    {
        private const string SCRIPTNAME = "ccdsoftAutoguider";

        // Exposes Autoguider as property/method proxy calls 
        private readonly ___CCDSoftAutoGuiderBase _autoguider;

        internal TheSkyXAutoguiderCamera(TheSkyXClient theSkyXClient) : base(theSkyXClient, SCRIPTNAME)
        {
            this._autoguider = new ___CCDSoftAutoGuiderBase(theSkyXClient, SCRIPTNAME);
        }

        public int Autoguide()
        {
            return this._autoguider.Autoguide();
        }

        public int CenterBrightestObject()
        {
            return this._autoguider.CenterBrightestObject();
        }

        public int Calibrate(bool calibrateAO = false)
        {
            return this._autoguider.Calibrate(calibrateAO);
        }

        public int MoveGuideStar()
        {
            return this._autoguider.MoveGuideStar();
        }

        public int centerAO()
        {
            return this._autoguider.centerAO();
        }

        public int Move(double fromX, double fromY, double toX, double toY)
        {
            return this._autoguider.Move(fromX, fromY, toX, toY);
        }
    }




    public enum ccdsoftImageReduction
    {
        cdNone, cdAutoDark, cdBiasDarkFlat
    }

    public enum ccdsoftImageFrame
    {
        cdLight = 1, cdBias, cdDark, cdFlat
    }

    public enum ccdsoftSBIGGuiderAntiBloom
    {
        cdAntiBloomOff, cdAntiBloomLow, cdAntiBloomMedium, cdAntiBloomHigh
    }

    public enum ccdsoftMoveVia
    {
        cdAutoguideViaRelays, cdAutoguideViaRelayAPI, cdAutoguideViaDirectGuide, cdAutoguideViaPulseGuide,
        cdAutoguideViaAO
    }

    public enum ccdsoftInterface
    {
        cdNoPort, cdLPT1, cdLPT2, cdLPT3,
        cdUSB = 0x7F00, cdEthernet
    }

    public enum ccdsoftAutoSaveAs
    {
        cdFITS, cdSBIG
    }

    public enum ccdsoftFocusGraph
    {
        cdMaximumValue, cdSharpness
    }

    public enum ccdsoftColorChannel
    {
        cdLuminance = 1, cdRed = 2, cdGreen = 3, cdBlue = 4
    }

    public enum ccdsoftfocTempCompMode
    {
        cdfocTempCompMode_None, cdfocTempCompMode_A, cdfocTempCompMode_B
    }

    public enum ccdsoftCameraState
    {
        cdStateNone, cdStateTakePicture, cdStateTakePictureSeries, cdStateFocus,
        cdStateMoveGuideStar, cdStateAutoGuide, cdStateCalibrate, cdStateTakeColor,
        cdStateAutoFocus, cdStateAutoFocus2
    }
}

