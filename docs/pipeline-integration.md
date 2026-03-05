# Pipeline Integration

## GitHub Actions

`.github/workflows/ci.yml` restores, builds, and tests the full solution on every push/pull request.  Coverage data feeds the README badges via the GitHub Gist endpoints referenced in the badge URLs.

## Publishing Flow

SDK packages publish per-package: push a tag that matches `<PackageId>/v<SemVer>` and the `NuGet Pack & Publish` workflow publishes **only that package** to NuGet (and GitHub Packages when a PAT is configured).

> **Important:** Do _not_ push bare `v*` tags (e.g., `v1.1.1`).  Those previously triggered a wildcard publish of every `.nupkg` in the solution, causing PDB-mismatch failures for packages whose version had not actually changed.

Recommended sequence:

1. **HVO.Core** — always publish the primitives first.  Telemetry and downstream repos consume this package directly.
2. **HVO.Core.SourceGenerators** — version must stay in lockstep with `HVO.Core` because the generators emit strongly-typed helpers for the primitives.
3. **Domain libraries** — Astronomy, Weather, IoT, etc. can publish independently once Core packages are live.

Example commands:

```bash
git tag -a "HVO.Core/v1.1.1" -m "HVO.Core 1.1.1"
git tag -a "HVO.Core.SourceGenerators/v1.1.1" -m "Source generators 1.1.1"
git push origin "HVO.Core/v1.1.1" "HVO.Core.SourceGenerators/v1.1.1"
```

## Coordinating With Telemetry

`HVO.Enterprise.Telemetry` references `HVO.Core` and expects the latest version to be available on NuGet before its own release tags are pushed.  When cutting a telemetry release:

1. Publish the updated SDK packages.
2. Wait for NuGet indexing (verify via `https://api.nuget.org/v3-flatcontainer/hvo.core/index.json`).
3. Tag the telemetry packages.

Documenting this order prevents cross-repo build failures and keeps dependency badges accurate.
