# Changelog

All notable changes documented here. Format: [Keep a Changelog](https://keepachangelog.com/), [Semantic Versioning](https://semver.org/).

## [Unreleased]

## [1.3.0] - 2026-03-04

### Added

- **HVO.Astronomy 1.0.0** — Sun, Moon, and planet position calculations; twilight and rise/set computations; coordinate transformations (initial NuGet release)
- **HVO.Astronomy.TheSkyX 1.0.0** — TheSkyX telescope control library for mount slewing, object lookup, and equipment management (initial NuGet release)
- **HVO.Weather 1.0.0** — Weather data models, CWOP/APRS formatter, and Weather Underground uploader (initial NuGet release)
- **HVO.Weather.DavisVantagePro 1.0.0** — Davis Vantage Pro serial protocol driver for weather station communication (initial NuGet release)
- **HVO.Power.OutbackMate 1.0.0** — Outback MATE serial protocol driver for solar power system monitoring (initial NuGet release)
- **HVO.NinaClient 1.0.0** — N.I.N.A. REST API client for camera, mount, and equipment control (initial NuGet release)

### Changed

- **HVO.Iot.Devices 1.2.0** — Added MLX90614, SI1145, TSL2591, HTU21DF, DS3231M, MCP23008, and MCP23017 I2C sensor/expander drivers. Updated `System.IO.Ports` to 10.0.3.
- Comprehensive code review improvements across all packages (#37)
- Incremental CI — build and test only affected projects (#38)
- Grouped dependabot configuration for cleaner dependency updates (#39)

### Dependencies

- `System.IO.Ports` 8.0.0 → 10.0.3
- `Microsoft.Extensions.*` packages updated to 10.0.3
- `SkiaSharp.*` packages updated to 3.119.2
- `FluentAssertions` 8.7.1 → 8.8.0
- `Microsoft.NET.Test.Sdk` and `MSTest` updated to latest

## [1.2.0] - 2026-03-02

### Changed

- **HVO.ZWOOptical.ASISDK 0.0.4** — Retargeted from netstandard2.1 to **netstandard2.0** for maximum compatibility (including .NET Framework 4.8.1). Added `Math.Clamp` polyfill.
- **HVO.Astronomy.CFITSIO 1.0.4** — Retargeted from net9.0 to **net10.0** to leverage latest `[LibraryImport]` source-generated P/Invoke improvements
- **HVO.Astronomy.CFITSIO.NativeAssets 1.0.4** — Retargeted from net9.0 to **net10.0** (aligned with CFITSIO)
- **HVO.Iot.Devices 1.1.0** — Retargeted from net9.0 to **net8.0** (minimum required by `System.Device.Gpio`/`Iot.Device.Bindings` packages). Updated `System.Device.Gpio` and `Iot.Device.Bindings` from 4.0.1 to 4.1.0.

## [1.1.0] - 2026-03-02

### Added

- **HVO.Core 1.1.0** — AstronomyMath utilities, additional extensions
- **HVO.Core.SourceGenerators 1.1.0** — updated alongside HVO.Core
- **HVO.Iot.Devices 1.0.0** — GPIO, I2C device abstractions for Raspberry Pi and Sequent Microsystems HATs (initial NuGet release)
- **HVO.Astronomy.CFITSIO 1.0.3** — FITS file I/O wrapper with SkiaSharp integration (initial NuGet release)
- **HVO.Astronomy.CFITSIO.NativeAssets 1.0.3** — native CFITSIO binaries for macOS/Linux/Windows (initial NuGet release)
- **HVO.ZWOOptical.ASISDK 0.0.3** — ZWO ASI Camera SDK P/Invoke wrapper (initial NuGet release)
- Repository documentation standardization

### Changed

- All packages now published to nuget.org (eliminating LocalPackages dependency in consuming repos)

## [1.0.0] - 2025-02-01

### Added

- HVO.Core with Result<T>, Option<T>, OneOf<T1..T4>, Guard, Ensure, extensions
- HVO.Core.SourceGenerators with [NamedOneOf] source generator
- CI/CD workflows for build, test, and NuGet publishing
- .editorconfig, dependabot, copilot instructions
