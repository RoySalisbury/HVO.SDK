# Testing

All SDK packages use MSTest 3.7 with method-level parallelization enabled in `Directory.Build.props`.  Tests live under `tests/` and mirror the source folder names so every library keeps a dedicated suite.

## Project Map

| Test Project | Purpose |
|--------------|---------|
| `HVO.Core.Tests` | Validates Result/Option/OneOf primitives. |
| `HVO.Core.SourceGenerators.Tests` | Snapshot tests for `[NamedOneOf]` source generators. |
| Astronomy / Weather / Power suites | Exercise domain-specific math helpers and device parsers. |
| `HVO.Iot.Devices.Tests` | Hardware abstraction shims. |

## Running Tests

```bash
cd /workspaces/HVO.Workspace/repos/HVO.SDK

dotnet test HVO.SDK.sln
```

Add `--settings tests/coverage.runsettings` when collecting coverage locally to match the CI configuration.

### Tips

- Keep tests deterministic—no wall-clock dependencies or network access.
- Prefer `Assert.ThrowsException` and FluentAssertions over `try/catch` blocks.
- Use the shared `TestHelpers` inside each project to avoid duplicating builders.
