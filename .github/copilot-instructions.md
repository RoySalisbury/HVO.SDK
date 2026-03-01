# Copilot Instructions for HVO.SDK

## Project Overview

**HVO.SDK** is a public .NET library providing shared utilities and functional patterns used across all HVO projects. It publishes two NuGet packages:

- **HVO.Core** — Result<T>, Option<T>, OneOf<T1..T4>, discriminated unions, guard clauses, extensions
- **HVO.Core.SourceGenerators** — Roslyn source generators (e.g., `[NamedOneOf]` attribute)

## Target Frameworks

- **HVO.Core**: `netstandard2.0` (maximum compatibility — .NET Framework 4.8.1 through .NET 10+)
- **HVO.Core.SourceGenerators**: `netstandard2.0` (required for Roslyn analyzers)
- **Tests**: `net10.0` with MSTest 3.7.0

## Critical Compatibility Guidelines

Since HVO.Core targets .NET Standard 2.0:

- **No** `ArgumentNullException.ThrowIfNull()` — use `if (x == null) throw new ArgumentNullException(nameof(x));`
- **No** `^` and `..` range operators, `init` accessors, records, default interface implementations
- **No** `ImplicitUsings` — always add explicit `using` statements (disabled project-wide)
- **Always** include `using System;` and other necessary namespaces explicitly
- Pattern matching limited to C# 7.x features
- Use nullable annotations (`?`) but ensure compatibility
- `System.Text.Json` 8.0.5 is a dependency (for `JsonElement` in `IOneOf`/`Option<T>`)

## Coding Standards

### Naming Conventions

- **PascalCase**: Classes, methods, properties, public fields, namespaces
- **camelCase**: Private fields (with underscore prefix: `_fieldName`), parameters, local variables
- **Namespace convention**: `HVO.Core.*` (Results, Options, OneOf, Extensions, Utilities)

### Error Handling Patterns

- **Result<T>**: For operations that can fail — `Result<T>.Success(value)` / `Result<T>.Failure(exception)`
- **Result<T, TEnum>**: For typed error codes with enum-based errors
- **Option<T>**: For values that may not exist — `new Option<T>(value)` / `Option<T>.None()`
- **Guard**: Input validation — `Guard.AgainstNull()`, `Guard.AgainstNullOrWhiteSpace()`, etc.
- **Ensure**: State assertions — `Ensure.That()`, `Ensure.NotNull()`, `Ensure.Unreachable()`

### Code Organization

- One class per file (unless nested/private classes)
- Namespace matches folder structure: `HVO.Core.FolderName`
- All public APIs must have complete XML documentation comments
- Strong naming enabled with `HVO.SDK.snk`

## Build & Quality

- **Zero warnings, zero errors** — `TreatWarningsAsErrors` is enabled
- **GenerateDocumentationFile** enabled for library projects (disabled for test projects via Directory.Build.targets)
- **Deterministic** builds enabled
- **Source Link** configured for GitHub debugging
- **Symbol packages** (.snupkg) published alongside NuGet packages

## Testing

- **MSTest** with method-level parallelization
- **Test naming**: `MethodName_Scenario_ExpectedBehavior`
- **Arrange-Act-Assert** pattern
- Coverage collected via `coverlet.collector`

## Publishing

- **Dual publish**: nuget.org (primary) + GitHub Packages (secondary)
- **Tag-triggered**: Push `v*` tag to publish all packages
- **Manual dispatch**: Select specific package and target via workflow_dispatch
- **Versioning**: SemVer in `.csproj` `<Version>` property

## Issue & PR Workflow

1. Create branch from `main` (`feature/<desc>` or `fix/<desc>`)
2. Implement with zero warnings/errors, all tests passing
3. Commit with conventional commits (`feat:`, `fix:`, `chore:`, `refactor:`, `test:`, `docs:`)
4. Push and create PR — stop and wait for review
5. Address review comments, rebuild, re-test
6. Squash merge into `main`, delete branch

## Dev Container Tool Policy

The dev container does **not** include Node.js, Python, or Azure CLI. Do **not** attempt to use these tools or suggest installing them.

- **Scripting & automation**: Use `bash`/`zsh` shell scripts, `gh` CLI, or `dotnet` CLI
- **JSON processing**: Use `jq` (installed) or .NET `System.Text.Json`
- **Issue/PR management**: Use `gh issue create`, `gh pr create`, etc.
- **Search**: Use `rg` (ripgrep, installed) for text search
- **Never** suggest `npm`, `npx`, `pip`, `python`, `az`, or `dotnet-script` commands

### Heredoc / Multi-Line String Warning

**Do NOT use `cat << 'EOF'` or any heredoc syntax in terminal commands.** Heredocs are unreliable in this environment — content frequently gets corrupted, garbled, or truncated. Instead:

1. Write multi-line content to a file using the file-creation tool (e.g., `create_file`).
2. Reference that file in the terminal command (e.g., `gh issue create --body-file /tmp/issue-body.md`).
