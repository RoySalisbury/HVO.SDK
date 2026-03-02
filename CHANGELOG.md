# Changelog

All notable changes documented here. Format: [Keep a Changelog](https://keepachangelog.com/), [Semantic Versioning](https://semver.org/).

## [Unreleased]

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
