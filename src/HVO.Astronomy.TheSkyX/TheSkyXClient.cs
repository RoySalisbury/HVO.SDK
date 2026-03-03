using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace HVO.Astronomy.TheSkyX
{
    public sealed class TheSkyXClient : IDisposable
    {
#pragma warning disable CS0414 // Field is assigned but never used
        private bool _isInitialized = false;
#pragma warning restore CS0414
        private CancellationToken _mainClientCancellationToken = default;
        private CancellationTokenSource? _mainStatusCancellationSource;
        private Task? _theSkyXMainStatusTask;

        private readonly Lazy<AutomatedImageLinkSettings> _automatedImageLinkSettings;
        private readonly Lazy<CameraDependentSetting> _cameraDependentSetting;
        private readonly Lazy<ClosedLoopSlew> _closedLoopSlew;
        private readonly Lazy<ImageLink> _imageLink;
        private readonly Lazy<Sky6RascomTele> _sky6RascomTele;
        private readonly Lazy<Sky6StarChart> _sky6StarChart;
        private readonly Lazy<Sky6Utils> _sky6Utils;
        private readonly Lazy<Sky6Web> _sky6Web;
        private readonly Lazy<TheSkyXMainCamera> _mainCamera;
        private readonly Lazy<TheSkyXAutoguiderCamera> _autoGuiderCamera;


        public TheSkyXClient(EndPoint endpoint, string description)
        {
            this.EndPoint = endpoint;
            this.Description = description;

            // These ensure that we only ever have a single instance of the class PER instance of TheSkyXClient. We dont want to use a singleton becuase 
            // there could be multile instances of the client to different hosts. We also delay creation of the instance until its requested.
            this._automatedImageLinkSettings = new Lazy<AutomatedImageLinkSettings>(() => new AutomatedImageLinkSettings(this), System.Threading.LazyThreadSafetyMode.ExecutionAndPublication);
            this._cameraDependentSetting = new Lazy<CameraDependentSetting>(() => new CameraDependentSetting(this), System.Threading.LazyThreadSafetyMode.ExecutionAndPublication);
            this._closedLoopSlew = new Lazy<ClosedLoopSlew>(() => new ClosedLoopSlew(this), System.Threading.LazyThreadSafetyMode.ExecutionAndPublication);
            this._imageLink = new Lazy<ImageLink>(() => new ImageLink(this), System.Threading.LazyThreadSafetyMode.ExecutionAndPublication);
            this._sky6RascomTele = new Lazy<Sky6RascomTele>(() => new Sky6RascomTele(this), System.Threading.LazyThreadSafetyMode.ExecutionAndPublication);
            this._sky6StarChart = new Lazy<Sky6StarChart>(() => new Sky6StarChart(this), System.Threading.LazyThreadSafetyMode.ExecutionAndPublication);
            this._sky6Utils = new Lazy<Sky6Utils>(() => new Sky6Utils(this), System.Threading.LazyThreadSafetyMode.ExecutionAndPublication);
            this._sky6Web = new Lazy<Sky6Web>(() => new Sky6Web(this), System.Threading.LazyThreadSafetyMode.ExecutionAndPublication);

            this._mainCamera = new Lazy<TheSkyXMainCamera>(() => new TheSkyXMainCamera(this), System.Threading.LazyThreadSafetyMode.ExecutionAndPublication);
            this._autoGuiderCamera = new Lazy<TheSkyXAutoguiderCamera>(() => new TheSkyXAutoguiderCamera(this), System.Threading.LazyThreadSafetyMode.ExecutionAndPublication);
        }

        public EndPoint EndPoint
        {
            get; private set;
        }


        public void Attach(CancellationToken cancellationToken = default)
        {
            if (this.IsAttached)
            {
                throw new InvalidOperationException("TheSkyXClient is already attached.");
            }


            // Attempt to make a call to the specifed instance to make sure its alive.  We will get the basic version and OS back to make sure things are working.
            var result = SendToTheSkyX(Properties.Resources.TheSkyXClientScript_Initialize, out var errorMessage);
            if (string.IsNullOrWhiteSpace(result))
            {
                // TODO: We can also get socket issues and there could be an errorMessage that we should look at. For now, this throw an exception.
                throw new InvalidOperationException("Unable to initialize.");
            }

            var model = JsonSerializer.Deserialize<Models.TheSkyXClient_InitializeResult>(result, new JsonSerializerOptions() { PropertyNameCaseInsensitive = true });
            if (model == null)
            {
                throw new InvalidOperationException("Failed to deserialize TheSkyX initialization response.");
            }

            // The 'revision' and 'build' are backwards based on the .NET class. So we need to swap them around
            var modelVersion = new Version(model.Version);

            // Try converting the build to in integer.  If it fails, it will be 0.
            int.TryParse(model.Build, out var modelBuild);

            // Create a new version object in the correct order.
            this.Version = new Version(modelVersion.Major, modelVersion.Minor, modelBuild, modelVersion.Build);

            this.OperatingSystem = model.OperatingSystem;

            // TODO: We should probably setup a handler that flips the "IsInitialized" property if the cencellationToken is signaled.
            this._mainClientCancellationToken = cancellationToken;

            // There are some static values that we want to get from TheSkyX that we can display, or use in further calculations (sunrise, sunset, moon phase, ect)
            this.ObservatoryInformation = this.GetObservatoryInformation();

            // Setup the client background task that gets system hardware status every 1000 ms.  Each device will then raise an even with its current status.
            var theSkyXHardware = this.GetSelectedHardware();

            // Join the main service cancellation token to out status token so that if the main one is cancelled, so is ours.
            this._mainStatusCancellationSource = CancellationTokenSource.CreateLinkedTokenSource(this._mainClientCancellationToken);

            // Start the background task that will constantly get the data that is possibly always updating.
#pragma warning disable CS8602 // Dereference of a possibly null reference — guarded by runtime null checks with ?. operators
            this._theSkyXMainStatusTask = Task.Run(async () =>
            {
                // Setup the script to run.  This is generated each time we attach to TheSkyX so we have an updated and unique script for the available hardware at the time.
                var script = new StringBuilder();

                #region Mount
                if ((string.IsNullOrWhiteSpace(theSkyXHardware?.Mount?.Model) == false) && string.Equals(theSkyXHardware.Mount.Model, "<No Mount Selected>", StringComparison.OrdinalIgnoreCase) == false)
                {
                    script.AppendLine("if (sky6RASCOMTele.IsConnected == 0) { var mount = { exists: true, isConnected: false }; } else {");
                    script.AppendLine("sky6RASCOMTele.GetRaDec();");
                    script.AppendLine("sky6RASCOMTele.GetAzAlt();");
                    script.AppendLine("var mount = { exists: true, isConnected: true, isSlewComplete: (sky6RASCOMTele.IsSlewComplete == 1), isTracking: sky6RASCOMTele.IsTracking == 1, isParked: sky6RASCOMTele.IsParked(), ra: sky6RASCOMTele.dRa, dec: sky6RASCOMTele.dDec, alt: sky6RASCOMTele.dAlt, az: sky6RASCOMTele.dAz }; }");
                }
                else
                {
                    script.AppendLine("var mount = { exists: false, isConnected: false, lst: dOut0 };");
                }
                #endregion

                #region Primary Camera and devices
                script.AppendLine("var mc = ccdsoftCamera;");

                if ((string.IsNullOrWhiteSpace(theSkyXHardware?.PrimaryCamera?.FilterWheel?.Model) == false) && string.Equals(theSkyXHardware.PrimaryCamera.FilterWheel.Model, "<No Filter Wheel Selected>", StringComparison.OrdinalIgnoreCase) == false)
                {
                    // This uses a special property "Real-Time" that is indicated in the scripting documentation.
                    script.AppendLine("var primaryFW = { exists: true, isConnected: (mc.filterWheelIsConnected() == 1), currentIndex : mc.PropLng('m_nFilterIndex Real-Time') };");
                }
                else
                {
                    script.AppendLine("var primaryFW = { exists: false, isConnected: false };");
                }

                if ((string.IsNullOrWhiteSpace(theSkyXHardware?.PrimaryCamera?.Focuser?.Model) == false) && string.Equals(theSkyXHardware.PrimaryCamera.Focuser.Model, "<No Focuser Selected>", StringComparison.OrdinalIgnoreCase) == false)
                {
                    script.AppendLine("if (mc.focIsConnected == 0) { var primaryFoc = { exists: true, isConnected: false }; } else {");
                    script.AppendLine("var primaryFoc = { exists: true, isConnected: true, temperature: mc.focTemperature, position: mc.focPosition }; }");
                }
                else
                {
                    script.AppendLine("var primaryFoc = { exists: false, isConnected: false };");
                }

                if ((string.IsNullOrWhiteSpace(theSkyXHardware?.PrimaryCamera?.Rotator?.Model) == false) && string.Equals(theSkyXHardware.PrimaryCamera.Rotator.Model, "<No Rotator Selected>", StringComparison.OrdinalIgnoreCase) == false)
                {
                    script.AppendLine("if (mc.rotatorIsConnected() == false) { var primaryRot = { exists: true, isConnected: false }; } else {");
                    script.AppendLine("var primaryRot = { exists: true, isConnected: true, isRotating: mc.rotatorIsRotating() == 1, positionAngle: mc.rotatorPositionAngle() }; }");
                }
                else
                {
                    script.AppendLine("var primaryRot = { exists: false, isConnected: false };");
                }

                if ((string.IsNullOrWhiteSpace(theSkyXHardware?.PrimaryCamera?.Model) == false) && string.Equals(theSkyXHardware.PrimaryCamera.Model, "<No Camera Selected>", StringComparison.OrdinalIgnoreCase) == false)
                {
                    script.AppendLine("if (mc.Status == 'Not Connected') { var primaryCamera = { exists: true, isConnected: false, fw: primaryFW, foc: primaryFoc, rotator: primaryRot }; } else {");

                    script.AppendLine("var primaryCameraTEC = { isAvailable: (mc.ThermalElectricCoolerPower != (-100)), isRegulated: mc.RegulateTemperature == 1, temperature: mc.Temperature, setPoint: mc.TemperatureSetPoint, power: mc.ThermalElectricCoolerPower };");
                    script.AppendLine("var primaryCamera = { exists: true, isConnected: true, binX: mc.BinX, binY: mc.BinY, tec: primaryCameraTEC, status: mc.Status, state: mc.State, fw: primaryFW, foc: primaryFoc, rotator: primaryRot }; }");
                }
                else
                {
                    script.AppendLine("var primaryCamera = { exists: false, isConnected: false, fw: primaryFW, foc: primaryFoc };");
                }
                #endregion

                #region Autoguider and devices
                script.AppendLine("var ac = ccdsoftAutoguider;");

                if ((string.IsNullOrWhiteSpace(theSkyXHardware?.AutoGuider?.FilterWheel?.Model) == false) && string.Equals(theSkyXHardware.AutoGuider.FilterWheel.Model, "<No Filter Wheel Selected>", StringComparison.OrdinalIgnoreCase) == false)
                {
                    // This uses a special property "Real-Time" that is indicated in the scripting documentation.
                    script.AppendLine("var autoguiderFW = { exists: true, isConnected: (ac.filterWheelIsConnected() == 1), currentIndex : ac.PropLng('m_nFilterIndex Real-Time') };");
                }
                else
                {
                    script.AppendLine("var autoguiderFW = { exists: false, isConnected: false };");
                }

                if ((string.IsNullOrWhiteSpace(theSkyXHardware?.AutoGuider?.Focuser?.Model) == false) && string.Equals(theSkyXHardware.AutoGuider.Focuser.Model, "<No Focuser Selected>", StringComparison.OrdinalIgnoreCase) == false)
                {
                    script.AppendLine("if (ac.focIsConnected == 0) { var autoguiderFoc = { exists: true, isConnected: false }; } else {");
                    script.AppendLine("var autoguiderFoc = { exists: true, isConnected: true, temperature: ac.focTemperature, position: ac.focPosition }; }");
                }
                else
                {
                    script.AppendLine("var autoguiderFoc = { exists: false, isConnected: false };");
                }

                if ((string.IsNullOrWhiteSpace(theSkyXHardware?.AutoGuider?.Rotator?.Model) == false) && string.Equals(theSkyXHardware.AutoGuider.Rotator.Model, "<No Rotator Selected>", StringComparison.OrdinalIgnoreCase) == false)
                {
                    script.AppendLine("if (ac.rotatorIsConnected() == false) { var autoguiderRot = { exists: true, isConnected: false }; } else {");
                    script.AppendLine("var autoguiderRot = { exists: true, isConnected: true, isRotating: ac.rotatorIsRotating() == 1, positionAngle: ac.rotatorPositionAngle() }; }");
                }
                else
                {
                    script.AppendLine("var autoguiderRot = { exists: false, isConnected: false };");
                }

                if ((string.IsNullOrWhiteSpace(theSkyXHardware?.AutoGuider?.Model) == false) && string.Equals(theSkyXHardware.AutoGuider.Model, "<No Camera Selected>", StringComparison.OrdinalIgnoreCase) == false)
                {
                    script.AppendLine("if (ac.Status == 'Not Connected') { var autoguider = { exists: true, isConnected: false, fw: autoguiderFW, foc: autoguiderFoc, rotator: autoguiderRot }; } else {");

                    script.AppendLine("var autoguiderTEC = {isAvailable: (ac.ThermalElectricCoolerPower != (-100)), isRegulated: ac.RegulateTemperature == 1, temperature: ac.Temperature, setPoint: ac.TemperatureSetPoint, power: ac.ThermalElectricCoolerPower };");
                    script.AppendLine("var autoguider = { exists: true, isConnected: true, binX: ac.BinX, binY: ac.BinY, tec: autoguiderTEC, status: ac.Status, state: ac.State, fw: autoguiderFW, foc: autoguiderFoc, rotator: autoguiderRot }; }");
                }
                else
                {
                    script.AppendLine("var autoguider = { exists: false, isConnected: false, fw:autoguiderFW, foc: autoguiderFoc, rotator: autoguiderRot };");
                }
                #endregion


                script.AppendLine("sky6Utils.ComputeLocalSiderealTime();");
                script.AppendLine("var dt = new Date();");

                script.AppendLine("var objResult = { lst : sky6Utils.dOut0, mount : mount, primaryCamera: primaryCamera, autoguider: autoguider };");
                script.AppendLine("JSON.stringify(objResult);");

                while (this._mainStatusCancellationSource.IsCancellationRequested == false)
                {
                    try
                    {
                        if (this.IsAttached)
                        {
                            var statusModel = this.SendToTheSkyX(script.ToString(), 2048, out errorMessage);
                            if (string.IsNullOrWhiteSpace(statusModel))
                            {
                                //throw new InvalidDataException("No response received.");
                                continue;
                            }

                            var statusResult = JsonSerializer.Deserialize<PeriodicHardwareStatus>(statusModel, new JsonSerializerOptions() { PropertyNameCaseInsensitive = true });

                            // From here, fire off the events for each device 
                            if (statusResult != null)
                            {
                                try
                                {
                                    var localSiderealTime = TimeSpan.FromHours(statusResult.LocalSiderealTime);

                                    var eventArgs = new PeriodicObservatoryStatusEventArgs(localSiderealTime);
                                    this.OnPeriodicObservatoryStatus?.Invoke(this, eventArgs);
                                }
                                catch { }


                                if ((statusResult.Mount != null) && (statusResult.Mount.Exists))
                                {
                                    try
                                    {
                                        var eventArgs = new PeriodicMountStatusEventArgs(DateTimeOffset.Now, statusResult.Mount.IsConnected);
                                        if (eventArgs.IsConnected)
                                        {
                                            eventArgs.Altitude = statusResult.Mount.Altitude;
                                            eventArgs.Azimuth = statusResult.Mount.Azimuth;
                                            eventArgs.Declination = statusResult.Mount.Declination;
                                            eventArgs.IsParked = statusResult.Mount.IsParked;
                                            eventArgs.IsTracking = statusResult.Mount.IsTracking;
                                            eventArgs.IsSlewComplete = statusResult.Mount.IsSlewComplete;
                                            eventArgs.RightAscension = HVO.Astronomy.RightAscension.FromHours(statusResult.Mount.RightAscension);
                                        }

                                        this.OnPeriodicMountStatus?.Invoke(this, eventArgs);
                                    }
                                    catch { }
                                }

                                #region Primary Camera and devices
                                if ((statusResult.PrimaryCamera != null) && (statusResult.PrimaryCamera.Exists))
                                {
                                    try
                                    {
                                        var eventArgs = new PeriodicCameraStatusEventArgs(DateTimeOffset.Now, statusResult.PrimaryCamera.IsConnected, HardwareInstanceType.Primary);
                                        if (eventArgs.IsConnected)
                                        {
                                            eventArgs.State = statusResult.PrimaryCamera.State;
                                            eventArgs.Status = statusResult.PrimaryCamera.Status;
                                            eventArgs.BinX = statusResult.PrimaryCamera.BinX;
                                            eventArgs.BinY = statusResult.PrimaryCamera.BinY;

                                            eventArgs.TEC = new PeriodicCameraStatusEventArgs._TEC(statusResult.PrimaryCamera.TEC.IsAvailable, statusResult.PrimaryCamera.TEC.IsRegulated, statusResult.PrimaryCamera.TEC.Temperature, statusResult.PrimaryCamera.TEC.Setpoint, statusResult.PrimaryCamera.TEC.Power);
                                        }

                                        this.OnPeriodicCameraStatus?.Invoke(this, eventArgs);
                                    }
                                    catch { }
                                }
                                if ((statusResult.PrimaryCamera != null) && (statusResult.PrimaryCamera.FilterWheel != null) && (statusResult.PrimaryCamera.FilterWheel.Exists))
                                {
                                    try
                                    {
                                        var eventArgs = new PeriodicFilterWheelStatusEventArgs(DateTimeOffset.Now, statusResult.PrimaryCamera.FilterWheel.IsConnected, statusResult.PrimaryCamera.FilterWheel.CurrentIndex, HardwareInstanceType.Primary);
                                        if (eventArgs.IsConnected)
                                        {
                                        }

                                        this.OnPeriodicFilterWheelStatus?.Invoke(this, eventArgs);
                                    }
                                    catch { }
                                }
                                if ((statusResult.PrimaryCamera != null) && (statusResult.PrimaryCamera.Focuser != null) && (statusResult.PrimaryCamera.Focuser.Exists))
                                {
                                    try
                                    {
                                        var eventArgs = new PeriodicFocuserStatusEventArgs(DateTimeOffset.Now, statusResult.PrimaryCamera.Focuser.IsConnected, HardwareInstanceType.Primary);
                                        if (eventArgs.IsConnected)
                                        {
                                            eventArgs.Position = statusResult.PrimaryCamera.Focuser.Position;
                                            eventArgs.TemperatureC = statusResult.PrimaryCamera.Focuser.TemperatureC;
                                        }

                                        this.OnPeriodicFocuserStatus?.Invoke(this, eventArgs);
                                    }
                                    catch { }
                                }
                                if ((statusResult.PrimaryCamera != null) && (statusResult.PrimaryCamera.Rotator != null) && (statusResult.PrimaryCamera.Rotator.Exists))
                                {
                                    try
                                    {
                                        var eventArgs = new PeriodicRotatorStatusEventArgs(DateTimeOffset.Now, statusResult.PrimaryCamera.Rotator.IsConnected, HardwareInstanceType.Primary);
                                        if (eventArgs.IsConnected)
                                        {
                                            eventArgs.IsRotating = statusResult.PrimaryCamera.Rotator.IsRotating;
                                            eventArgs.PositionAngle = statusResult.PrimaryCamera.Rotator.PositionAngle;
                                        }

                                        this.OnPeriodicRotatorStatus?.Invoke(this, eventArgs);
                                    }
                                    catch { }
                                }
                                #endregion

                                #region Autoguider and devices
                                if ((statusResult.Autoguider != null) && (statusResult.Autoguider.Exists))
                                {
                                    try
                                    {
                                        var eventArgs = new PeriodicCameraStatusEventArgs(DateTimeOffset.Now, statusResult.Autoguider.IsConnected, HardwareInstanceType.Autoguider);
                                        if (eventArgs.IsConnected)
                                        {
                                            eventArgs.State = statusResult.Autoguider.State;
                                            eventArgs.Status = statusResult.Autoguider.Status;
                                            eventArgs.BinX = statusResult.Autoguider.BinX;
                                            eventArgs.BinY = statusResult.Autoguider.BinY;

                                            eventArgs.TEC = new PeriodicCameraStatusEventArgs._TEC(statusResult.Autoguider.TEC.IsAvailable, statusResult.Autoguider.TEC.IsRegulated, statusResult.Autoguider.TEC.Temperature, statusResult.Autoguider.TEC.Setpoint, statusResult.Autoguider.TEC.Power);
                                        }

                                        this.OnPeriodicCameraStatus?.Invoke(this, eventArgs);
                                    }
                                    catch { }
                                }
                                if ((statusResult.Autoguider != null) && (statusResult.Autoguider.FilterWheel != null) && (statusResult.Autoguider.FilterWheel.Exists))
                                {
                                    try
                                    {
                                        var eventArgs = new PeriodicFilterWheelStatusEventArgs(DateTimeOffset.Now, statusResult.Autoguider.FilterWheel.IsConnected, statusResult.Autoguider.FilterWheel.CurrentIndex, HardwareInstanceType.Autoguider);
                                        if (eventArgs.IsConnected)
                                        {
                                        }

                                        this.OnPeriodicFilterWheelStatus?.Invoke(this, eventArgs);
                                    }
                                    catch { }
                                }
                                if ((statusResult.Autoguider != null) && (statusResult.Autoguider.Focuser != null) && (statusResult.Autoguider.Focuser.Exists))
                                {
                                    try
                                    {
                                        var eventArgs = new PeriodicFocuserStatusEventArgs(DateTimeOffset.Now, statusResult.Autoguider.Focuser.IsConnected, HardwareInstanceType.Autoguider);
                                        if (eventArgs.IsConnected)
                                        {
                                            eventArgs.Position = statusResult.Autoguider.Focuser.Position;
                                            eventArgs.TemperatureC = statusResult.Autoguider.Focuser.TemperatureC;
                                        }

                                        this.OnPeriodicFocuserStatus?.Invoke(this, eventArgs);
                                    }
                                    catch { }
                                }
                                if ((statusResult.Autoguider != null) && (statusResult.Autoguider.Rotator != null) && (statusResult.Autoguider.Rotator.Exists))
                                {
                                    try
                                    {
                                        var eventArgs = new PeriodicRotatorStatusEventArgs(DateTimeOffset.Now, statusResult.Autoguider.Rotator.IsConnected, HardwareInstanceType.Autoguider);
                                        if (eventArgs.IsConnected)
                                        {
                                            eventArgs.IsRotating = statusResult.Autoguider.Rotator.IsRotating;
                                            eventArgs.PositionAngle = statusResult.Autoguider.Rotator.PositionAngle;
                                        }

                                        this.OnPeriodicRotatorStatus?.Invoke(this, eventArgs);
                                    }
                                    catch { }
                                }
                                #endregion

                            }
                        }
                    }
                    catch (Exception)
                    {
                        // Polling loop — exceptions are expected during shutdown or transient connectivity issues.
                        // The loop will retry on the next iteration.
                    }

                    // Only update the status every 1000 ms.
                    await Task.Delay(1000, this._mainStatusCancellationSource.Token);
                }
            }, this._mainStatusCancellationSource.Token);
#pragma warning restore CS8602

            this.IsAttached = true;

            this.OnTheSkyXAttached?.Invoke(this, EventArgs.Empty);
        }

        public event EventHandler<EventArgs>? OnTheSkyXAttached;
#pragma warning disable CS0067 // Event is never used
        public event EventHandler<EventArgs>? OnTheSkyXDetached;
#pragma warning restore CS0067

        public event EventHandler<PeriodicObservatoryStatusEventArgs>? OnPeriodicObservatoryStatus;
        public event EventHandler<PeriodicMountStatusEventArgs>? OnPeriodicMountStatus;
        public event EventHandler<PeriodicCameraStatusEventArgs>? OnPeriodicCameraStatus;
        public event EventHandler<PeriodicFilterWheelStatusEventArgs>? OnPeriodicFilterWheelStatus;
        public event EventHandler<PeriodicFocuserStatusEventArgs>? OnPeriodicFocuserStatus;
        public event EventHandler<PeriodicRotatorStatusEventArgs>? OnPeriodicRotatorStatus;

        //public void Detach()
        //{
        //    if ((this.IsAttached) && (this._theSkyXMainStatusTask.IsCompleted == false))
        //    {
        //        this._mainStatusCancellationSource.Cancel(false);
        //        WaitHandle.WaitAll(new WaitHandle[] { this._mainStatusCancellationSource.Token.WaitHandle }, 1500);

        //        this.IsAttached = false;
        //        this.OnTheSkyXDetached?.Invoke(this, EventArgs.Empty);
        //    }

        //}

        public string Description { get; private set; } = string.Empty;

        public bool IsAttached { get; private set; }

        public bool IsTheSkyXInitialized()
        {
            ThrowIfNotAttached();

            var model = SendToTheSkyX("Application.initialized", out var errorMessage);
            if (bool.TryParse(model, out var result))
            {
                return result;
            }

            return false;
        }

        public Version? Version { get; private set; }

        public Models.TheSkyXOperatingSystem OperatingSystem { get; private set; } = Models.TheSkyXOperatingSystem.osUnknown;

        public Models.TheSkyXObservatoryInformation? ObservatoryInformation
        {
            get; private set;
        }

        public Models.TheSkyXSelectedHardware? ObservatoryHardware
        {
            get; private set;
        }

        private Models.TheSkyXObservatoryInformation? GetObservatoryInformation()
        {
            var result = SendToTheSkyX(Properties.Resources.TheSkyXScript_GetObservatoryInformation, 4096, out var errorMessage);
            if (string.IsNullOrWhiteSpace(result) == false)
            {
                var model = JsonSerializer.Deserialize<Models.TheSkyXObservatoryInformation>(result, new JsonSerializerOptions() { PropertyNameCaseInsensitive = true });
                return model;
            }

            return null;
        }

        private Models.TheSkyXSelectedHardware? GetSelectedHardware()
        {
            var result = SendToTheSkyX(Properties.Resources.TheSkyXScript_GetSelectedHardware, 4096, out var errorMessage);
            if (string.IsNullOrWhiteSpace(result) == false)
            {
                var model = JsonSerializer.Deserialize<Models.TheSkyXSelectedHardware>(result, new JsonSerializerOptions() { PropertyNameCaseInsensitive = true });
                return model;
            }

            return null;
        }

        public void DoAction(string action)
        {
            ThrowIfNotAttached();
            SendToTheSkyX($"TheSkyXAction.execute('{action}');", out var errorMessage);
        }

        public AutomatedImageLinkSettings AutomatedImageLinkSettings => this._automatedImageLinkSettings.Value;

        public CameraDependentSetting CameraDependentSetting => this._cameraDependentSetting.Value;

        public ClosedLoopSlew ClosedLoopSlew => this._closedLoopSlew.Value;

        public ImageLink ImageLink => this._imageLink.Value;

        public Sky6RascomTele Telescope => this._sky6RascomTele.Value;

        public Sky6StarChart StarChart => this._sky6StarChart.Value;

        public Sky6Utils TheSkyUtilities => this._sky6Utils.Value;

        public TheSkyXMainCamera MainCamera => this._mainCamera.Value;

        public TheSkyXAutoguiderCamera AutoguiderCamera => this._autoGuiderCamera.Value;


        // Not yet ready .. too many questions.
        //public Sky6Web TheSkyWeb => this._sky6Web.Value;

        internal void ThrowIfNotAttached(int requiredBuildNumber = 0)
        {
            if ((this.IsAttached == false) || (this.Version == null) || (this.Version.Build < requiredBuildNumber))
            {
                throw new InvalidOperationException("TheSkyX has not been attached or is the incorrect version.");
            }
        }

        // Each 'scriptBlock' is unique to what we want to accomplish.  The method will return either a singe value or a JSON formatted string for complext objects. The statusMessage will be whatever TSX appended to the 
        // result we created.  Not all command sent to TSX will have a result, so it is up the caller to determine what to do.
        //
        // TODO: for socket errors, what do we do?  Should we throw exceptions for all socket errors and TSX errors? 
        internal string SendToTheSkyX(string scriptBlock, int maxResultLength, out string errorMessage)
        {
            errorMessage = string.Empty;
            var resultText = string.Empty;

            // Our message template
            string messageText = $"/* Java Script */{Environment.NewLine}/* Socket Start Packet */{Environment.NewLine}{Environment.NewLine}{scriptBlock}{Environment.NewLine}{Environment.NewLine}/* Socket End Packet */";

            // Encode the message to send
            var messageBytes = Encoding.UTF8.GetBytes(messageText);

            // Create the socket, send the data and receive the result.
            using (var theSkyXSocket = new Socket(this.EndPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp))
            {
                try
                {
                    theSkyXSocket.Connect(this.EndPoint);
                }
                catch (SocketException)
                {
                    // This is just debug code for right now.
                    throw;
                }

                // Send the message
                var bytesSent = theSkyXSocket.Send(messageBytes, 0, (messageBytes != null) ? messageBytes.Length : 0, SocketFlags.None, out var socketError);
                if ((socketError == SocketError.Success) && (bytesSent > 0))
                {
                    var receivedBytes = new byte[maxResultLength];
                    var bytesReceived = theSkyXSocket.Receive(receivedBytes, 0, (receivedBytes != null) ? receivedBytes.Length : 0, SocketFlags.None, out socketError);
                    if ((socketError == SocketError.Success) && (bytesReceived > 0))
                    {
                        resultText = Encoding.UTF8.GetString(receivedBytes, 0, bytesReceived);
                    }
                    else
                    {
                        errorMessage = socketError.ToString();
                        return string.Empty;
                    }
                }
                else
                {
                    errorMessage = socketError.ToString();
                    return string.Empty;
                }
            }

            if (resultText.StartsWith("{\"lst\":") == false)
            {
                // Non-status responses are expected for many script calls; no action needed.
            }

            // Split out the status message from the return value.
            if (string.IsNullOrWhiteSpace(resultText) == false)
            {
                var errorMessageIndex = resultText.LastIndexOf('|');
                if (errorMessageIndex != -1)
                {
                    errorMessage = resultText.Substring(errorMessageIndex + 1); // Make sure to remove the '|' character.
                    resultText = resultText.Substring(0, errorMessageIndex);
                }


                // The errorMessage that TSX appends seems to be meaningless.  If the script errors out, it does not use that, but returns the error string and then sets the statusMessage to "No Error".  
                // Kinda stupid.  We will work around that below when we find issues.
                if (resultText.StartsWith("TypeError: ") || errorMessage.StartsWith("TypeError: "))
                {
                    if (resultText.StartsWith("TypeError: "))
                    {
                        errorMessage = resultText;
                    }

                    resultText = string.Empty;

                    Regex r = new Regex(@"^TypeError:\s*(?'errorMessage'.*)Error\s*=\s*(?'errorCode'[0-9]+)");
                    var match = r.Match(errorMessage);

                    if (match.Groups.Count > 2)
                    {
                        errorMessage = match.Groups[1]?.Value?.Trim() ?? string.Empty;
                        int.TryParse(match.Groups[2]?.Value?.Trim(), out var errorCode);

                        throw new TheSkyXException(errorMessage, errorCode);
                    }
                }
                else if (resultText.StartsWith("ParseError"))
                {
                    errorMessage = resultText;
                    resultText = string.Empty;
                }
            }

            return resultText;
        }

        internal string SendToTheSkyX(string scriptBlock, out string errorMessage)
        {
            // The default size for script results is 256 bytes .. plenty for most things. 
            return SendToTheSkyX(scriptBlock, 256, out errorMessage);
        }

        #region IDisposable Support
        private bool _disposed = false; // To detect redundant calls

        void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    // Cancel and dispose the background status polling task.
                    try
                    {
                        _mainStatusCancellationSource?.Cancel();
                    }
                    catch (ObjectDisposedException) { }

                    try
                    {
                        _mainStatusCancellationSource?.Dispose();
                    }
                    catch (ObjectDisposedException) { }

                    _mainStatusCancellationSource = null!;
                }

                _disposed = true;
            }
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        // ~TheSkyXClient()
        // {
        //   // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
        //   Dispose(false);
        // }

        // This code added to correctly implement the disposable pattern.
        void IDisposable.Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            // TODO: uncomment the following line if the finalizer is overridden above.
            // GC.SuppressFinalize(this);
        }
        #endregion

        private class PeriodicHardwareStatus
        {
            public abstract class _Base
            {
                public bool Exists
                {
                    get; set;
                }

                public bool IsConnected
                {
                    get; set;
                }
            }

            public class _FilterWheel : _Base
            {
                public int CurrentIndex
                {
                    get; set;
                }
            }

            public class _Focuser : _Base
            {
                [JsonPropertyName("temperature")]
                public double TemperatureC
                {
                    get; set;
                }

                [JsonPropertyName("position")]
                public int Position
                {
                    get; set;
                }
            }

            public class _Rotator : _Base
            {
                public bool IsRotating
                {
                    get; set;
                }

                public double PositionAngle
                {
                    get; set;
                }
            }

            public class _Mount : _Base
            {
                public bool IsSlewComplete
                {
                    get; set;
                }

                public bool IsTracking
                {
                    get; set;
                }

                public bool IsParked
                {
                    get; set;
                }

                [JsonPropertyName("ra")]
                public double RightAscension
                {
                    get; set;
                }

                [JsonPropertyName("dec")]
                public double Declination
                {
                    get; set;
                }

                [JsonPropertyName("alt")]
                public double Altitude
                {
                    get; set;
                }

                [JsonPropertyName("az")]
                public double Azimuth
                {
                    get; set;
                }
            }

            public class _Camera : _Base
            {
                public class _TEC
                {
                    public bool IsAvailable
                    {
                        get; set;
                    }

                    public bool IsRegulated
                    {
                        get; set;
                    }

                    public double Temperature
                    {
                        get; set;
                    }

                    public double Setpoint
                    {
                        get; set;
                    }

                    public int Power
                    {
                        get; set;
                    }
                }

                public _TEC? TEC
                {
                    get; set;
                }

                public string? Status
                {
                    get; set;
                }

                public int State
                {
                    get; set;
                }

                public int BinX
                {
                    get; set;
                }

                public int BinY
                {
                    get; set;
                }

                [JsonPropertyName("fw")]
                public _FilterWheel? FilterWheel
                {
                    get; set;
                }

                [JsonPropertyName("foc")]
                public _Focuser? Focuser
                {
                    get; set;
                }

                public _Rotator? Rotator
                {
                    get; set;
                }
            }

            [JsonPropertyName("lst")]
            public double LocalSiderealTime
            {
                get; set;
            }

            public _Mount? Mount
            {
                get; set;
            }

            public _Camera? PrimaryCamera
            {
                get; set;
            }

            public _Camera? Autoguider
            {
                get; set;
            }
        }
    }
}
