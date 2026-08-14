# HVO.SDK

[![CI](https://github.com/RoySalisbury/HVO.SDK/actions/workflows/ci.yml/badge.svg)](https://github.com/RoySalisbury/HVO.SDK/actions/workflows/ci.yml)
[![NuGet](https://img.shields.io/nuget/v/HVO.Core)](https://www.nuget.org/packages/HVO.Core)
[![NuGet Downloads](https://img.shields.io/nuget/dt/HVO.Core)](https://www.nuget.org/packages/HVO.Core)
[![Tests](https://img.shields.io/endpoint?url=https://gist.githubusercontent.com/RoySalisbury/15a5e41c02e71169bd342c22576e2646/raw/hvo-sdk-tests.json)](https://github.com/RoySalisbury/HVO.SDK/actions/workflows/ci.yml)
[![Line Coverage](https://img.shields.io/endpoint?url=https://gist.githubusercontent.com/RoySalisbury/15a5e41c02e71169bd342c22576e2646/raw/hvo-sdk-coverage-line.json)](https://github.com/RoySalisbury/HVO.SDK/actions/workflows/ci.yml)
[![Branch Coverage](https://img.shields.io/endpoint?url=https://gist.githubusercontent.com/RoySalisbury/15a5e41c02e71169bd342c22576e2646/raw/hvo-sdk-coverage-branch.json)](https://github.com/RoySalisbury/HVO.SDK/actions/workflows/ci.yml)
[![.NET Standard](https://img.shields.io/badge/.NET%20Standard-2.0-blue)](https://docs.microsoft.com/en-us/dotnet/standard/net-standard)
[![License](https://img.shields.io/badge/license-MIT-green)](LICENSE)

Shared .NET libraries for observatory software, astronomy, weather, power, imaging, and hardware integration.

## Packages

| Package | Target | Description |
|---------|--------|-------------|
| **HVO.Core** | `netstandard2.0` | `Result<T>`, `Option<T>`, `OneOf<T1..T4>`, discriminated unions, guard clauses, extensions |
| **HVO.Core.SourceGenerators** | `netstandard2.0` | Roslyn source generators (e.g., `[NamedOneOf]` attribute) |
| **HVO.Astronomy** | `netstandard2.0` | Solar, lunar, planetary, twilight, and coordinate calculations |
| **HVO.Astronomy.TheSkyX** | `netstandard2.0` | Software Bisque TheSkyX equipment integration |
| **HVO.Astronomy.CFITSIO** | `net10.0` | Managed FITS image and header APIs with SkiaSharp integration |
| **HVO.Astronomy.CFITSIO.NativeAssets** | `net10.0` | CFITSIO native assets for Linux ARM64 and macOS ARM64 |
| **HVO.Iot.Devices** | `net8.0` | GPIO, I2C, HAT, RTC, and environmental sensor drivers |
| **HVO.NinaClient** | `net8.0` | N.I.N.A. REST and WebSocket client |
| **HVO.Power.OutbackMate** | `netstandard2.0` | Outback MATE serial protocol parser and monitor |
| **HVO.Weather** | `netstandard2.0` | Weather models and CWOP/Weather Underground formatting |
| **HVO.Weather.DavisVantagePro** | `netstandard2.0` | Davis Vantage Pro console protocol integration |
| **HVO.ZWOOptical.ASISDK** | `netstandard2.0` | ZWO ASI camera SDK interop |

## Installation

```bash
dotnet add package HVO.Core
```

Replace `HVO.Core` with the package needed by your application. Package-specific platform notes are included in each NuGet package readme.

## Quick Start

### Result&lt;T&gt; — Railway-oriented error handling

```csharp
using HVO.Core.Results;

public Result<Customer> GetCustomer(int id)
{
    try
    {
        var customer = _repository.Find(id);
        return customer != null
            ? Result<Customer>.Success(customer)
            : Result<Customer>.Failure(new NotFoundException($"Customer {id} not found"));
    }
    catch (Exception ex)
    {
        return ex; // Implicit conversion to failure
    }
}

// Usage
var result = GetCustomer(42);
var output = result.Match(
    success: c => $"Found: {c.Name}",
    failure: ex => $"Error: {ex.Message}");
```

### Result&lt;T, TEnum&gt; — Typed error codes

```csharp
using HVO.Core.Results;

public enum OrderError { NotFound, InvalidAmount, Unauthorized }

public Result<Order, OrderError> ValidateOrder(OrderRequest request)
{
    if (request.Amount <= 0)
        return OrderError.InvalidAmount; // Implicit conversion

    return Result<Order, OrderError>.Success(new Order(request));
}
```

### Option&lt;T&gt; — Safe optional values

```csharp
using HVO.Core.Options;

public Option<string> GetSetting(string key)
{
    return _config.TryGetValue(key, out var value)
        ? new Option<string>(value)
        : Option<string>.None();
}

// Usage
var setting = GetSetting("timeout");
var value = setting.GetValueOrDefault("30"); // Returns "30" if None
```

### OneOf&lt;T1, T2&gt; — Discriminated unions

```csharp
using HVO.Core.OneOf;

OneOf<int, string> result = 42;

var output = result.Match(
    i => $"Got integer: {i}",
    s => $"Got string: {s}");
// output: "Got integer: 42"
```

### Guard & Ensure — Input validation and state assertions

```csharp
using HVO.Core.Utilities;

// Guard: validates input parameters (throws ArgumentException)
public void Process(string name, int count)
{
    Guard.AgainstNullOrWhiteSpace(name);
    Guard.AgainstNegativeOrZero(count, 0);
}

// Ensure: asserts internal state (throws InvalidOperationException)
public void Execute()
{
    Ensure.That(_isInitialized, "Service must be initialized");
    Ensure.NotNull(_connection, "Connection not established");
}
```

### Extensions

```csharp
using HVO.Core.Extensions;

// String extensions
"hello world".ToTitleCase();           // "Hello World"
"Hello World Test".RemoveWhitespace(); // "HelloWorldTest"
"Second".ToEnum<MyEnum>();             // MyEnum.Second

// Collection extensions
new[] { 1, 2, 3 }.ForEach(x => Console.Write(x));
items.DistinctBy(x => x.Id);
items.Shuffle();

// Enum extensions
MyEnum.Value.GetDescription(); // Returns [Description] attribute value
```

## Compatibility

HVO.Core targets **.NET Standard 2.0** for maximum compatibility:

- .NET Framework 4.8.1+
- .NET 8+
- .NET 10+
- Mono, Xamarin, Unity

## Building

```bash
dotnet build
dotnet test
```

## Documentation

| Document | Description |
|----------|-------------|
| [CONTRIBUTING.md](CONTRIBUTING.md) | PR workflow, coding standards, branch naming |
| [CHANGELOG.md](CHANGELOG.md) | Release history and notable changes |
| [Testing](docs/testing.md) | Test projects, coverage settings, local commands |
| [Pipeline Integration](docs/pipeline-integration.md) | CI workflow, tag-based publishing, coordination with telemetry |

## Contributing

See [CONTRIBUTING.md](CONTRIBUTING.md) for PR workflow, coding standards, and how to run tests locally.

## License

MIT
