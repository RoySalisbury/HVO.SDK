# HVO Workspace Repository Map

> Last updated: 2026-03-04

This document catalogs all repositories in the HVO multi-repo workspace, their purpose, projects, and how domain logic maps across them. It serves as the single reference for understanding where code lives and where it should go.

## Table of Contents

- [Repository Overview](#repository-overview)
- [Active Repositories](#active-repositories)
- [Legacy / Read-Only Repositories](#legacy--read-only-repositories)
- [Domain Logic Map](#domain-logic-map)
- [Duplication Tracker](#duplication-tracker)
- [Migration Status](#migration-status)

---

## Repository Overview

| Repo | Status | Runtime | Purpose |
|------|--------|---------|---------|
| **HVO.SDK** | Active | .NET 10 / ns2.0 / net8.0 | Foundation NuGet packages — shared across all HVO projects |
| **HVO.SkyMonitor** | Active | .NET 10 | Next-gen distributed all-sky camera system (microservices) |
| **HVOv9** | Active | .NET 10 | Monorepo — SkyMonitorV4/V5 (production), Playground, NinaClient |
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
| `HVO.Iot.Devices` | net8.0 | 1.1.0 | GPIO/I2C abstractions, limit switches, relay hats, buttons |
| `HVO.ZWOOptical.ASISDK` | ns2.0 | 0.0.4 | ZWO ASI camera SDK P/Invoke wrapper |
| `HVO.Astronomy` | ns2.0 | 1.0.0 | Sun/Moon/planet positions, sunrise/sunset, twilight, moon phase, Lat/Lon types, coordinate transforms |
| `HVO.Astronomy.CFITSIO` | net10.0 | 1.0.4 | FITS file I/O via cfitsio native library |
| `HVO.Astronomy.CFITSIO.NativeAssets` | net10.0 | 1.0.4 | Native cfitsio binaries (linux-arm64) |

**IoT Devices included:**
- `GpioLimitSwitch` — limit switch with debounce and event-driven state changes
- `GpioButtonWithLed` — push button with integrated LED feedback
- `I2cRegisterDevice` — base class for I2C sensor devices
- `FourRelayFourInputHat` — Sequent Microsystems 4-relay/4-input RPi HAT
- `WatchdogBatteryHat` — Sequent Microsystems watchdog timer HAT
- `GpioControllerClient` / `MemoryGpioControllerClient` — GPIO abstractions for testing

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
| `HVO.NinaClient` | NINA astronomy software REST/WebSocket client |
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

#### Azure Functions V2 (6 functions)

| Function | Trigger | Purpose | Status |
|----------|---------|---------|--------|
| `DavisVantagePro` | ServiceBus `weatherrecords` | Parse binary weather → SQL | May be active |
| `OutbackPower` | ServiceBus `powerrecords` | Parse serial power data → SQL | May be active |
| `CitizensWeather` | Timer (5 min) | Forward weather to CWOP/APRS | May be active |
| `WeatherUnderground` | Timer (30 sec) | Forward weather to WU API | May be active |
| `BlueIris` | ServiceBus (3 queues) | Camera image/video records → SQL | Unknown |
| `SkyMonitor` | ServiceBus `skymonitor` | Luminosity/temp sensor data → SQL | Unknown |

#### Edge Agents (2 generations)

V1 (`HVO.ObservatoryControl.*`) and V2 (`HVO.ObservatoryMonitor.*`):
- **WeatherMonitor** — Davis VP TCP → RabbitMQ
- **PowerMonitor** — Outback Mate UDP → RabbitMQ
- **CameraUpload** — Filesystem watch → Azure Blob Storage → Service Bus
- **QueueTransferAgent** — RabbitMQ → Azure Service Bus bridge

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
| Davis VP binary protocol parser | HVOv6 `HVO/Weather/DavisVantagePro/` | — | **HVO.SDK** `HVO.Weather` (new) |
| WeatherLink IP TCP client | HVOv6 `HVO/Weather/DavisVantagePro/` | — | **HVO.SDK** `HVO.Weather` (new) |
| Temperature unit conversions | HVOv6 `HVO/Weather/Temperature.cs` | — | **HVO.SDK** `HVO.Weather` (new) |
| Barometric pressure conversions | HVOv6 `HVO/Weather/BarometricPressure.cs` | — | **HVO.SDK** `HVO.Weather` (new) |
| Meteorological algorithms (8+) | HVOv6 `HVO/Weather/WxUtils.cs` | — | **HVO.SDK** `HVO.Weather` (new) |
| CRC-16 CCITT | HVOv6 `HVO/Security/Cryptography/Crc16.cs` | — | **HVO.SDK** `HVO.Weather` (new) |
| CWOP/APRS formatting | HVOv6 Azure Function `CitizensWeather` | — | Stays in function / agent |
| Weather Underground posting | HVOv6 Azure Function `WeatherUnderground` | — | Stays in function / agent |

### Power Systems

| Capability | Legacy Source | Current Home | Canonical Target |
|-----------|-------------|-------------|-----------------|
| Outback Mate 2 serial parser | HVOv6 `HVO/Power/OutbackPowerSystems/` | — | **HVO.SDK** `HVO.Power` (new) |
| Charge controller records | HVOv6 `HVO/Power/OutbackPowerSystems/` | — | **HVO.SDK** `HVO.Power` (new) |
| FlexNet DC records | HVOv6 `HVO/Power/OutbackPowerSystems/` | — | **HVO.SDK** `HVO.Power` (new) |
| Inverter/charger records | HVOv6 `HVO/Power/OutbackPowerSystems/` | — | **HVO.SDK** `HVO.Power` (new) |
| Web Power Switch HTTP control | HVOv6 `HVO/Power/DigitalLoggers/` | — | **HVO.SDK** `HVO.Power` (new) |

### IoT / Hardware

| Capability | Legacy Source | Current Home | Canonical Target |
|-----------|-------------|-------------|-----------------|
| GPIO abstractions / limit switches | — | **HVO.SDK** `HVO.Iot.Devices` 1.1.0 | ✅ Done |
| Relay HATs (Sequent) | — | **HVO.SDK** `HVO.Iot.Devices` 1.1.0 | ✅ Done |
| Watchdog HAT (Sequent) | — | **HVO.SDK** `HVO.Iot.Devices` 1.1.0 | ✅ Done |
| MLX90614 (IR sky temp) | HVOv6 `HualapaiValleyObservatory.IoT/` | — | **HVO.SDK** `HVO.Iot.Devices` |
| TSL2591 (luminosity) | HVOv6 `HualapaiValleyObservatory.IoT/` | — | **HVO.SDK** `HVO.Iot.Devices` |
| HTU21DF (humidity/temp) | HVOv6 `HualapaiValleyObservatory.IoT/` | — | **HVO.SDK** `HVO.Iot.Devices` |
| SI1145 (UV/visible/IR) | HVOv6 `HualapaiValleyObservatory.IoT/` | — | **HVO.SDK** `HVO.Iot.Devices` |
| DS3231M (RTC) | HVOv6 `HualapaiValleyObservatory.IoT/` | — | **HVO.SDK** `HVO.Iot.Devices` |
| MCP23008/23017 (GPIO expander) | HVOv6 `HualapaiValleyObservatory.IoT/` | — | **HVO.SDK** `HVO.Iot.Devices` |
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
| Weather protocols | HVOv6 `HVO/Weather/` | HVO.SDK `HVO.Weather` | ⬜ Not started | Davis VP parser, unit conversions, met algorithms |
| Power systems | HVOv6 `HVO/Power/` | HVO.SDK `HVO.Power` | ⬜ Not started | Outback Mate parser, Web Power Switch |
| I2C sensor drivers | HVOv6 `HualapaiValleyObservatory.IoT/` | HVO.SDK `HVO.Iot.Devices` | ⬜ Not started | MLX90614, TSL2591, HTU21DF, SI1145 |
| Star catalogs | HVOv9-SkyMonitorv6 `Imaging/Catalogs/` | HVO.SDK `HVO.Astronomy` | ⬜ Not started | HYG, deep-sky, constellations |
| Star field rendering | HVOv9-SkyMonitorv6 `Imaging/Rendering/` | HVO.SkyMonitor | ⬜ Not started | Projectors, star color map (imaging, not astronomy) |
| FITS file I/O | — | HVO.SDK `HVO.Astronomy.CFITSIO` | ✅ Done | v1.0.4 published |
| GPIO / relay / switch | — | HVO.SDK `HVO.Iot.Devices` | ✅ Done | v1.1.0 published |
| ZWO ASI SDK | — | HVO.SDK `HVO.ZWOOptical.ASISDK` | ✅ Done | v0.0.4 published |
