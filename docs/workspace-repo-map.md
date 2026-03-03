# HVO Workspace Repository Map

> Last updated: 2026-03-02

This document catalogs all repositories in the HVO multi-repo workspace, their purpose, projects, and how domain logic maps across them. It serves as the single reference for understanding where code lives and where it should go.

## Table of Contents

- [Repository Overview](#repository-overview)
- [Active Repositories](#active-repositories)
- [Legacy / Read-Only Repositories](#legacy--read-only-repositories)
- [Domain Logic Map](#domain-logic-map)
- [Duplication Tracker](#duplication-tracker)
- [Migration Status](#migration-status)
- [Repos to Archive](#repos-to-archive)
- [Forked Repos to Delete](#forked-repos-to-delete)

---

## Repository Overview

| Repo | Status | Runtime | Purpose |
|------|--------|---------|---------|
| **HVO.SDK** | Active | .NET 10 / ns2.0 / net8.0 | Foundation NuGet packages — shared across all HVO projects |
| **HVO.SkyMonitor** | Active | .NET 10 | Next-gen distributed all-sky camera system (microservices) |
| **HVOv9** | Active | .NET 10 | Monorepo — SkyMonitorV4/V5 (production), Playground |
| **HVO.WebSite** | Active | .NET 9 | Observatory website — weather/power dashboards, all-sky viewer |
| **HVO.RoofController** | Active | .NET 9 | Roof automation — RPi server + iPad MAUI client |
| **HVO.AiCodeReview** | Active | .NET 10 | AI-powered Azure DevOps PR code review service |
| **HVO.Enterprise.Telemetry** | Active | .NET 10 / ns2.0 | Enterprise telemetry and logging library |
| **HVOv6** | Read-only | net461–net6.0 | Legacy V6 monorepo — original domain logic, Azure Functions |
| **HVOv9-SkyMonitorv6** | Read-only | .NET 10 | SkyMonitorV6 reference — imaging engine, star catalogs |

---

## Active Repositories

### HVO.SDK

> Foundation NuGet package repo. All shared libraries publish from here.

| Package | TFM | Version | Description |
|---------|-----|---------|-------------|
| `HVO.Core` | ns2.0 | 1.1.0 | Result\<T\>, Option\<T\>, OneOf, Guard, Ensure, extensions |
| `HVO.Core.SourceGenerators` | ns2.0 | 1.1.0 | Roslyn source generators (`[NamedOneOf]`) |
| `HVO.Iot.Devices` | net8.0 | 1.2.0 | GPIO/I2C abstractions, limit switches, relay hats, buttons, I2C sensors |
| `HVO.ZWOOptical.ASISDK` | ns2.0 | 0.0.4 | ZWO ASI camera SDK P/Invoke wrapper |
| `HVO.Astronomy` | ns2.0 | 1.0.0 | Sun/Moon/planet positions, sunrise/sunset, twilight, moon phase, Lat/Lon types, coordinate transforms |
| `HVO.Weather` | ns2.0 | 1.0.0 | Davis VP binary protocol, WeatherLink TCP, barometric algorithms, temperature/pressure unit conversions, CRC-16 |
| `HVO.Power` | ns2.0 | 1.0.0 | Outback Mate 2 serial parser, charge controller/FlexNet/inverter records, Digital Loggers Web Power Switch |
| `HVO.Astronomy.CFITSIO` | net10.0 | 1.0.4 | FITS file I/O via cfitsio native library |
| `HVO.Astronomy.CFITSIO.NativeAssets` | net10.0 | 1.0.4 | Native cfitsio binaries (linux-arm64) |
| `HVO.Astronomy.TheSkyX` | ns2.0 | 1.0.0 | TheSkyX telescope control TCP/socket client — mount, camera, focuser, filter wheel, rotator, plate solving |
| `HVO.NinaClient` | net8.0 | 1.0.0 | NINA astronomy software REST + WebSocket client — full equipment control, sequencer, imaging, guiding, circuit breaker, DI extensions |

**IoT Devices included:**
- `GpioLimitSwitch` — limit switch with debounce and event-driven state changes
- `GpioButtonWithLed` — push button with integrated LED feedback
- `I2cRegisterDevice` — base class for I2C sensor devices
- `FourRelayFourInputHat` — Sequent Microsystems 4-relay/4-input RPi HAT
- `WatchdogBatteryHat` — Sequent Microsystems watchdog timer HAT
- `GpioControllerClient` / `MemoryGpioControllerClient` — GPIO abstractions for testing
- `Mlx90614` — MLX90614 non-contact IR temperature sensor (ambient + object temps)
- `Si1145` — SI1145 UV/visible/IR/proximity sensor with auto-measurement
- `Tsl2591` — TSL2591 high-dynamic-range light sensor with auto-gain-ranging- `Htu21df` — HTU21DF humidity and temperature sensor
- `Ds3231m` — DS3231M real-time clock with temperature compensation
- `Mcp23008` — MCP23008 8-bit I/O expander
- `Mcp23017` — MCP23017 16-bit I/O expander
### HVO.SkyMonitor

> Next-gen distributed SkyMonitor. Microservices architecture with camera agents, logic host, and shared contracts.

| Project | Description |
|---------|-------------|
| `HVO.SkyMonitor.LogicHost` | Central coordinator service (presigned uploads, config management) |
| `HVO.SkyMonitor.CameraAgent` | Camera agent application (per-camera-type containers) |
| `HVO.SkyMonitor.CameraAgent.Common` | Shared camera agent logic (imaging, preview encoding) |
| `HVO.SkyMonitor.AgentCore` | Agent registration, heartbeat, configuration protocol |
| `HVO.SkyMonitor.Common` | Shared DTOs, models, contracts |
| `HVO.SkyMonitor.TestSupport` | Test utilities and builders |

### HVOv9

> Monorepo for production observatory applications. Targets .NET 10.

| Project | Description |
|---------|-------------|
| `HVO.SkyMonitorV5.RPi` | **Production all-sky camera** — ASP.NET Core on RPi with ZWO ASI, FITS export, star detection, S3 frame export |
| `HVO.SkyMonitorV5.Data` | EF Core data layer for SkyMonitorV5 |
| `HVO.SkyMonitorV5.RPi.Tests` | 206 unit tests |
| `HVO.SkyMonitorV5.RPi.Benchmarks` | BenchmarkDotNet performance tests |
| `HVO.SkyMonitorV5.RPi.Stress` | Stress testing harness |
| `HVO.SkyMonitorV4.RPi` | Legacy SkyMonitorV4 (ASP.NET Core + ZWO) |
| `HVO.SkyMonitorV4.CLI` | CLI tool for SkyMonitorV4 |
| ~~`HVO.NinaClient`~~ | *Migrated to HVO.SDK — use NuGet package* |
| `HVO.GpioTestApp` | GPIO testing utility |
| `HVO.Playground.CLI` | General playground CLI |

### HVO.WebSite

> Observatory dashboard website with weather/power monitoring and all-sky viewer.

| Project | Description |
|---------|-------------|
| `HVO.WebSite.v9` | Main Blazor Server dashboard — weather, power, all-sky, astronomy info |
| `HVO.DataModels` | EF Core models and DbContext for observatory SQL database |
| `HVO.WebSite.Themes` | Shared CSS themes (HVO Dark), fonts, static assets (RCL) |
| `HVO.WebSite.Playground` | Web experiments / prototyping |

### HVO.RoofController

> Observatory roof automation system.

| Project | Description |
|---------|-------------|
| `HVO.RoofControllerV4.RPi` | Raspberry Pi server — Blazor SSR + GPIO motor/relay/limit switch control |
| `HVO.RoofControllerV4.iPad` | iPad MAUI companion client |
| `HVO.RoofControllerV4.Common` | Shared models and DTOs |
| `HVO.WebSite.Themes` | Shared CSS themes (local copy of RCL) |
| `HVO.RoofControllerV4.RPi.Tests` | 67 unit tests |

### HVO.AiCodeReview

> AI-powered code review for Azure DevOps pull requests.

| Project | Description |
|---------|-------------|
| `HVO.AiCodeReview` | ASP.NET Core Web API — Azure OpenAI (GPT-4o/o4-mini/GPT-5-mini) code review |
| `HVO.AiCodeReview.Tests` | 509+ unit tests |

### HVO.Enterprise.Telemetry

> Enterprise telemetry and structured logging library. Multi-platform (.NET Framework 4.8.1 through .NET 10).

| Project | Description |
|---------|-------------|
| `HVO.Enterprise.Telemetry` | Core library — distributed tracing, metrics, structured logging |
| 12 extension packages | Platform integrations: AppInsights, Datadog, Serilog, IIS, WCF, gRPC, EF Core, ADO.NET, Redis, RabbitMQ, OpenTelemetry, Data (base) |
| `HVO.Common` | Shared utilities (Result\<T\>, Option\<T\>) — separate from HVO.Core |
| 14 test projects | Full coverage across all extensions |

---

## Legacy / Read-Only Repositories

### HVOv6 (Source/HVO/)

> Original V6 monorepo imported from Azure DevOps. 36 projects targeting net461–net6.0. **Read-only — do not modify.**

#### Core Library (`Source/HVO/`)
Target: `netstandard2.0`

| Namespace | Content | Migration Target |
|-----------|---------|-----------------|
| `HVO.Astronomy` | Sun position, sunrise/sunset, twilight (civil/nautical/astronomical), Moon RA/Dec/phase, J2000 calculations | → **HVO.SDK** new package |
| `HVO.Weather` | Temperature/pressure/distance unit conversions, 8+ barometric algorithms (SLP, altimeter, vapor pressure) | → **HVO.SDK** new package |
| `HVO.Weather.DavisVantagePro` | Binary LOOP packet parser (99-byte protocol), WeatherLink IP TCP client, CRC-16 validation | → **HVO.SDK** new package |
| `HVO.Power.OutbackPowerSystems` | Outback Mate 2 serial parser (19200 baud), charge controller/FlexNet/inverter records, 7 enum types | → **HVO.SDK** new package |
| `HVO.Power.DigitalLoggers` | Web Power Switch HTTP control (get status, toggle outlets) | → **HVO.SDK** new package |
| `HVO.Security.Cryptography` | CRC-16 CCITT hash algorithm (Davis VP packet validation) | → **HVO.SDK** (with weather) |
| `HVO.Threading.Tasks` | `Task.WithCancellation()` extension | → **HVO.Core** |

#### Data Models (`Source/HVO.Data/`)
Target: `netstandard2.0`

EF Core DbContext for Azure SQL Server with 16+ entity tables: weather records, power system records (3 subsystems), camera records, sky monitor sensors, web power switch configs, plus one-minute archive tables.

#### Azure Functions (3 generations, all archived)

**Generation 1** (`HVO.Azure.FunctionsV1/`):

| Function | Trigger | Purpose |
|----------|---------|---------|
| `ProcessDavisVantageProConsoleRecord` | ServiceBus `weatherrecords` | Parse binary Davis VP LOOP packet → SQL |
| `ProcessOutbackPowerRecord` | ServiceBus `powerrecords` | Parse Outback Mate 2 serial data → SQL |
| `ProcessCitizensWeather` | Timer `15 */5 * * * *` (every 5 min) | Format APRS packet → TCP to `cwop.aprs.net:14580` (station `DW4515`) |
| `ProcessWeatherUnderground` | Timer `*/30 * * * * *` (every 30 sec) | Format 25+ weather params → HTTP GET to WU API (station `KAZKINGM12`) |

**Generation 2** (`HVO.Azure.FunctionsV2.*/`, separate project per function — latest active version):

| Function | Project | Trigger | Purpose |
|----------|---------|---------|---------|
| `ProcessDavisVantageProConsoleRecord` | `FunctionsV2.DavisVantagePro` | ServiceBus `weatherrecords` | Parse binary Davis VP LOOP packet → SQL |
| `ProcessOutbackPowerRecord` | `FunctionsV2.OutbackPower` | ServiceBus `powerrecords` | Parse Outback Mate 2 serial data → SQL |
| `ProcessCitizensWeather` | `FunctionsV2.CitizensWeather` | Timer (every 5 min) | APRS packet → CWOP server |
| `ProcessWeatherUnderground` | `FunctionsV2.WeatherUnderground` | Timer (every 30 sec) | Weather data → WU API |
| `ProcessSecurityCameraImage` | `FunctionsV2.BlueIris` | ServiceBus `securitycamera/images` | Security camera image records → SQL |
| `ProcessSecurityCameraVideo` | `FunctionsV2.BlueIris` | ServiceBus `securitycamera/video` | Security camera video records → SQL |
| `ProcessWeatherCameraImage` | `FunctionsV2.BlueIris` | ServiceBus `weathercamera/images` | Weather camera image records → SQL |
| `ProcessWeatherCameraVideo` | `FunctionsV2.BlueIris` | ServiceBus `weathercamera/video` | Weather camera video records → SQL |
| `ProcessAllSkyCameraImage` | `FunctionsV2.BlueIris` | ServiceBus `allskycamera/images` | All-sky camera image records → SQL |
| `ProcessAllSkyCameraVideo` | `FunctionsV2.BlueIris` | ServiceBus `allskycamera/video` | All-sky camera video records → SQL |
| `ProcessSkyMonitor` | `FunctionsV2.SkyMonitor` | ServiceBus `skymonitor` | Sky sensor (luminosity/temp) data → SQL |

**Other**: `HualapaiValleyObservatory.Alexa.SmartHomeSkill` — AWS Lambda (Alexa Smart Home discovery, partially implemented)

#### Edge Agents (2 generations, deployed as Linux systemd services — no Docker)

**V1** (`HVO.ObservatoryControl.*`):

| Agent | Communication | Purpose |
|-------|---------------|---------|
| `WeatherMonitor` | Davis VP TCP (WeatherLink IP) → RabbitMQ, batch → Azure Service Bus | Reads 99-byte LOOP packets from Davis VP console |
| `PowerMonitor` | UDP listener → RabbitMQ, batch → Azure Service Bus | Receives Outback Mate 2 serial data broadcasts |
| `CameraUpload` | Filesystem watch → Azure Blob Storage + Service Bus | Watches Blue Iris directories for new image/video files |
| `QueueTransferAgent` | RabbitMQ → Azure Service Bus | Generic message bridge, configurable per queue |

**V2** (`HVO.ObservatoryMonitor.*`, netcoreapp2.2, HostedService pattern):

| Agent | Communication | Purpose |
|-------|---------------|---------|
| `WeatherStationAgent` | Same as V1 WeatherMonitor | HostedService refactor of V1 |
| `OutbackPowerAgent` | Same as V1 PowerMonitor | HostedService refactor of V1 |
| `ImageUploadAgent` | Same as V1 CameraUpload | HostedService refactor of V1 |
| `QueueTransferAgent` | Same as V1 QueueTransferAgent | HostedService refactor of V1 |

#### External Services (not in GitHub)

| Service | Location | Communication | Purpose |
|---------|----------|---------------|---------|
| JK BMS monitor | Linux box (systemd service) | Bluetooth to JK BMS | Reads battery management system data (voltage, current, SOC) — **not tracked in any GitHub repo** |

#### IoT Devices (`HualapaiValleyObservatory.IoT/`)
Target: UWP 10.0.15063.0

I2C device drivers (Windows IoT): MLX90614 (IR sky temp), TSL2591 (luminosity), HTU21DF (humidity), SI1145 (UV), DS3231M (RTC), MCP23008/23017 (GPIO expanders), HD44780 (LCD), CAT24C32 (EEPROM). These are candidates for porting to `HVO.Iot.Devices` with the modern `System.Device.I2c` API.

#### Other Notable Projects

| Project | Description |
|---------|-------------|
| `HVO.WebSite.RoofControllerV2` (net6.0) | Latest legacy roof controller with `System.Device.Gpio` |
| `HVO.ObservatoryControl.RoofControllerV3` (net5.0) | Intermediate roof controller iteration |
| `HualapaiValleyObservatory.Alexa.SmartHomeSkill` | AWS Lambda — Alexa Smart Home discovery (partially implemented) |
| `HVO.Identity` | Custom ASP.NET Core Identity with user/role/permission management |
| `HualapaiValleyObservatory.WebSite.v6` (netcoreapp3.1) | Main observatory website — weather/power dashboards |
| `HVO.Azure` | Custom Azure WebJobs extension for RabbitMQ trigger binding |

### HVOv9-SkyMonitorv6

> SkyMonitorV6 distributed architecture reference. **Read-only — do not modify.**

| Project | Description |
|---------|-------------|
| `HVO.SkyMonitorV6.Imaging` | Star field rendering — fisheye/rectilinear projectors, HYG star catalog, deep-sky catalog, constellation line figures, planet positions, star color mapping |
| `HVO.SkyMonitorV6.Contracts` | Shared DTOs and imaging models |
| `HVO.SkyMonitorV6.CameraAgent.*` | Per-camera agents (Sim, RPiCam, ZWO) with tests |
| `HVO.SkyMonitorV6.LogicService` | Central coordinator |
| `HVO.SkyMonitorV6.DataModels` | PostgreSQL EF Core models |
| `HVO.SkyMonitorV6.UI` | Blazor Server admin dashboard |
| Local copies | HVO.Core, CFITSIO, ASISDK, WebSites.Framework (duplicates of SDK packages) |

---

## Domain Logic Map

Where observatory domain knowledge currently lives and where it should go:

### Astronomy

| Capability | Legacy Source | Current Home | Canonical Target |
|-----------|-------------|-------------|-----------------|
| Sun position / rise / set / twilight | HVOv6 `HVO/Astronomy/Sun.cs` | **HVO.SDK** `HVO.Astronomy` 1.0.0 | ✅ Done |
| Moon position / rise / set / phase | HVOv6 `HVO/Astronomy/Moon.cs` | **HVO.SDK** `HVO.Astronomy` 1.0.0 | ✅ Done |
| Lat/Lon types | HVOv6 `HVO/Astronomy/Latitude.cs`, `Longitude.cs` | **HVO.SDK** `HVO.Astronomy` 1.0.0 | ✅ Done |
| J2000 / GMST / sidereal time | HVOv6 `HVO/Astronomy/DateCalculations.cs` | **HVO.SDK** `HVO.Astronomy` 1.0.0 | ✅ Done |
| Planet positions (Keplerian ephemeris) | HVOv9-SkyMonitorv6 `Imaging/Rendering/Planets/` | **HVO.SDK** `HVO.Astronomy` 1.0.0 | ✅ Done |
| Core astronomy math (deprecated in Core) | **HVO.SDK** `HVO.Core` `Astronomy/AstronomyMath.cs` | **HVO.SDK** `HVO.Astronomy` 1.0.0 | ✅ Done — Core version marked `[Obsolete]` |
| Star catalogs (HYG) | HVOv9-SkyMonitorv6 `Imaging/Catalogs/` | — | **HVO.SDK** `HVO.Astronomy` (future) |
| Constellation figures | HVOv9-SkyMonitorv6 `Imaging/Catalogs/` | — | **HVO.SDK** `HVO.Astronomy` (future) |
| Deep-sky objects | HVOv9-SkyMonitorv6 `Imaging/Catalogs/` | — | **HVO.SDK** `HVO.Astronomy` (future) |
| Star field projection | HVOv9-SkyMonitorv6 `Imaging/Rendering/` | — | **HVO.SkyMonitor** (not astronomy) |
| Fisheye/rectilinear lens projectors | HVOv9-SkyMonitorv6 `Imaging/Rendering/` | — | **HVO.SkyMonitor** (imaging, not astronomy) |
| FITS file I/O | — | **HVO.SDK** `HVO.Astronomy.CFITSIO` 1.0.4 | ✅ Done |

### Weather

| Capability | Legacy Source | Current Home | Canonical Target |
|-----------|-------------|-------------|-----------------|
| Davis VP binary protocol parser | HVOv6 `HVO/Weather/DavisVantagePro/` | **HVO.SDK** `HVO.Weather` 1.0.0 | ✅ Done |
| WeatherLink IP TCP client | HVOv6 `HVO/Weather/DavisVantagePro/` | **HVO.SDK** `HVO.Weather` 1.0.0 | ✅ Done |
| Temperature unit conversions | HVOv6 `HVO/Weather/Temperature.cs` | **HVO.SDK** `HVO.Weather` 1.0.0 | ✅ Done |
| Barometric pressure conversions | HVOv6 `HVO/Weather/BarometricPressure.cs` | **HVO.SDK** `HVO.Weather` 1.0.0 | ✅ Done |
| Meteorological algorithms (8+) | HVOv6 `HVO/Weather/WxUtils.cs` | **HVO.SDK** `HVO.Weather` 1.0.0 | ✅ Done |
| CRC-16 CCITT | HVOv6 `HVO/Security/Cryptography/Crc16.cs` | **HVO.SDK** `HVO.Weather` 1.0.0 | ✅ Done |
| CWOP/APRS packet formatting | HVOv6 `FunctionsV2.CitizensWeather/ProcessCitizensWeather.cs` *(archived)* | — | 🚧 Migrating to `HVO.Weather` |
| Weather Underground URL builder | HVOv6 `FunctionsV2.WeatherUnderground/ProcessWeatherUnderground.cs` *(archived)* | — | 🚧 Migrating to `HVO.Weather` |

### Power Systems

| Capability | Legacy Source | Current Home | Canonical Target |
|-----------|-------------|-------------|-----------------|
| Outback Mate 2 serial parser | HVOv6 `HVO/Power/OutbackPowerSystems/` | **HVO.SDK** `HVO.Power` 1.0.0 | ✅ Done |
| Charge controller records | HVOv6 `HVO/Power/OutbackPowerSystems/` | **HVO.SDK** `HVO.Power` 1.0.0 | ✅ Done |
| FlexNet DC records | HVOv6 `HVO/Power/OutbackPowerSystems/` | **HVO.SDK** `HVO.Power` 1.0.0 | ✅ Done |
| Inverter/charger records | HVOv6 `HVO/Power/OutbackPowerSystems/` | **HVO.SDK** `HVO.Power` 1.0.0 | ✅ Done |
| Web Power Switch HTTP control | HVOv6 `HVO/Power/DigitalLoggers/` | **HVO.SDK** `HVO.Power` 1.0.0 | ✅ Done |
| JK BMS battery monitor | External (Linux systemd service, Bluetooth) | — | ⬜ Not tracked in GitHub — custom service reading JK BMS via Bluetooth |

### IoT / Hardware

| Capability | Legacy Source | Current Home | Canonical Target |
|-----------|-------------|-------------|-----------------|
| GPIO abstractions / limit switches | — | **HVO.SDK** `HVO.Iot.Devices` 1.1.0 | ✅ Done |
| Relay HATs (Sequent) | — | **HVO.SDK** `HVO.Iot.Devices` 1.1.0 | ✅ Done |
| Watchdog HAT (Sequent) | — | **HVO.SDK** `HVO.Iot.Devices` 1.1.0 | ✅ Done |
| MLX90614 (IR sky temp) | HVOv6 `HualapaiValleyObservatory.IoT/` + nF.Devices | **HVO.SDK** `HVO.Iot.Devices` 1.2.0 | ✅ Done |
| TSL2591 (luminosity) | HVOv6 `HualapaiValleyObservatory.IoT/` + nF.Devices | **HVO.SDK** `HVO.Iot.Devices` 1.2.0 | ✅ Done |
| SI1145 (UV/visible/IR) | HVOv6 `HualapaiValleyObservatory.IoT/` + nF.Devices | **HVO.SDK** `HVO.Iot.Devices` 1.2.0 | ✅ Done |
| HTU21DF (humidity/temp) | HVOv6 `HualapaiValleyObservatory.IoT/` | **HVO.SDK** `HVO.Iot.Devices` 1.2.0 | ✅ Done |
| DS3231M (RTC) | HVOv6 `HualapaiValleyObservatory.IoT/` | **HVO.SDK** `HVO.Iot.Devices` 1.2.0 | ✅ Done |
| MCP23008/23017 (GPIO expander) | HVOv6 `HualapaiValleyObservatory.IoT/` | **HVO.SDK** `HVO.Iot.Devices` 1.2.0 | ✅ Done |
| ZWO ASI camera SDK | — | **HVO.SDK** `HVO.ZWOOptical.ASISDK` 0.0.4 | ✅ Done |

### Camera / Imaging

| Capability | Legacy Source | Current Home | Canonical Target |
|-----------|-------------|-------------|-----------------|
| ZWO ASI camera control | HVOv6 AllSkyCamera projects | **HVOv9** SkyMonitorV5 | ✅ In production |
| Star detection / astrometry | — | **HVOv9** SkyMonitorV5 | Stays in SkyMonitorV5 |
| Star field rendering engine | HVOv9-SkyMonitorv6 `Imaging/` | — | **HVO.SkyMonitor** |
| Fisheye/rectilinear projectors | HVOv9-SkyMonitorv6 `Imaging/Rendering/` | — | **HVO.SkyMonitor** (imaging/lens, not astronomy) |

---

## Duplication Tracker

Known duplications across repos. Canonical source is the one that should be maintained.

| Content | Canonical | Duplicates |
|---------|-----------|------------|
| `HVO.Core` (Result, Option, OneOf) | **HVO.SDK** NuGet | HVOv9-SkyMonitorv6 `src/HVO.Core/` (local copy) |
| `HVO.Core.Astronomy.AstronomyMath` | **HVO.SDK** `HVO.Astronomy` NuGet | **HVO.SDK** `HVO.Core` `Astronomy/AstronomyMath.cs` (deprecated — marked `[Obsolete]`) |
| `HVO.Astronomy.CFITSIO` | **HVO.SDK** NuGet | HVOv9-SkyMonitorv6 `src/HVO.Astronomy.CFITSIO/` (local copy) |
| `HVO.ZWOOptical.ASISDK` | **HVO.SDK** NuGet | HVOv9-SkyMonitorv6 `src/HVO.ZWOOptical.ASISDK/` (local copy) |
| `HVO.WebSite.Themes` | **HVO.WebSite** `src/` | **HVO.RoofController** `src/` (local copy) |
| Core library classes | HVOv6 `Source/HVO/` | HVOv6 `Source/HualapaiValleyObservatory/` (same classes, different namespace) |
| Data models | **HVO.WebSite** `HVO.DataModels` | HVOv6 `HVO.Data`, HVOv6 `HualapaiValleyObservatory.DataModels` |

---

## Migration Status

Track what has been migrated from legacy repos to SDK/active repos.

| Domain | Source | Target | Status | Notes |
|--------|--------|--------|--------|-------|
| Astronomy calculations | HVOv6 `HVO/Astronomy/` + HVOv9-SkyMonitorv6 `Imaging/Planets/` | HVO.SDK `HVO.Astronomy` | ✅ Done | Sun, Moon, planets, twilight, Lat/Lon, J2000. 3 bugs fixed: Crescent spelling, Longitude always-West, DivideByZero. 93 unit tests. |
| Weather protocols | HVOv6 `HVO/Weather/` | HVO.SDK `HVO.Weather` | ✅ Done | Davis VP binary parser, WeatherLink TCP, 8+ met algorithms, CRC-16, unit conversions. PR #27. |
| Power systems | HVOv6 `HVO/Power/` | HVO.SDK `HVO.Power` | ✅ Done | Outback Mate 2 serial parser, Web Power Switch HTTP control, 7 enum types. PR #28. |
| I2C sensor drivers | HVOv6 IoT + nF.Devices | HVO.SDK `HVO.Iot.Devices` | ✅ Done | MLX90614, SI1145, TSL2591 (PR #29). HTU21DF, DS3231M, MCP23008, MCP23017 (PR #32). |
| Star catalogs | HVOv9-SkyMonitorv6 `Imaging/Catalogs/` | HVO.SDK `HVO.Astronomy` | ⬜ Not started | HYG, deep-sky, constellations |
| Star field rendering | HVOv9-SkyMonitorv6 `Imaging/Rendering/` | HVO.SkyMonitor | ⬜ Not started | Projectors, star color map (imaging, not astronomy) |
| FITS file I/O | — | HVO.SDK `HVO.Astronomy.CFITSIO` | ✅ Done | v1.0.4 published |
| GPIO / relay / switch | — | HVO.SDK `HVO.Iot.Devices` | ✅ Done | v1.1.0 published |
| ZWO ASI SDK | — | HVO.SDK `HVO.ZWOOptical.ASISDK` | ✅ Done | v0.0.4 published |
| TheSkyX telescope control | `RoySalisbury/TheSkyX` | HVO.SDK `HVO.Astronomy.TheSkyX` | ✅ Done | TCP/socket client, mount/camera/focuser/filter/rotator/plate solving. PR #31. |
| NINA client | HVOv9 `HVO.NinaClient` | HVO.SDK `HVO.NinaClient` | ✅ Done | REST + WebSocket client, 120+ source files, circuit breaker, buffer management, DI extensions. PR #34. |

---

## Repos to Archive

These repos are legacy, superseded, or no longer actively developed. They should be archived on GitHub to signal read-only status and reduce account clutter.

| Repo | Reason | Action |
|------|--------|--------|
| `HVOv1` | Ancient legacy (v1) | ✅ Archived |
| `HVOv2` | Ancient legacy (v2) | ✅ Archived |
| `HVOv3` | Ancient legacy (v3) | ✅ Archived |
| `HVOv4` | Ancient legacy (v4) | ✅ Archived |
| `HVOv5` | Ancient legacy (v5) | ✅ Archived |
| `HVOv6` | Legacy monorepo — all domain logic migrated to HVO.SDK | ✅ Archived |
| `HVOv7` | Ancient legacy (v7) | ✅ Archived |
| `HVOv8` | Legacy (v8), public | ✅ Archived |
| `HVOv9-SkyMonitorv6` | Legacy reference — phase-3 branch merged for preservation (PR #2) | ✅ Archived |
| `TheSkyX` | Migrated to HVO.SDK `HVO.Astronomy.TheSkyX` (PR #31) | ✅ Archived |
| `NinaWebUIPlugIn` | NINA plugin experiment, no longer maintained | ✅ Archived |
| `nF.CoreLibrary` | nanoFramework experiment, no longer maintained | ✅ Archived |
| `nF.Devices` | nanoFramework drivers — I2C sensors migrated to HVO.SDK `HVO.Iot.Devices` | ✅ Archived |
| `DevOpsMcp` | DevOps MCP server experiment, last updated Aug 2025 | ✅ Archived |

---

## Forked Repos ~~to Delete~~ — Deleted

All forks deleted on 2026-03-02.

| Repo | Forked From | Status |
|------|-------------|--------|
| ~~`nina`~~ | christian-photo/nina | ✅ Deleted |
| ~~`lib-CoreLibrary`~~ | nanoframework/lib-CoreLibrary | ✅ Deleted |
| ~~`lib-nanoFramework.Hardware.Esp32`~~ | nanoframework/lib-nanoFramework.Hardware.Esp32 | ✅ Deleted |
| ~~`nf-Community-Contributions`~~ | nanoframework/nf-Community-Contributions | ✅ Deleted |
| ~~`nf-interpreter`~~ | nanoframework/nf-interpreter | ✅ Deleted |
| ~~`lib-Windows.Devices.Gpio`~~ | nanoframework/lib-Windows.Devices.Gpio | ✅ Deleted |
