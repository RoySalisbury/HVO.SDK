# Contributing to HVO.SDK

Thank you for your interest in contributing! This guide covers the workflow, standards, and expectations for pull requests.

## Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download) (tests target `net10.0`)
- An IDE with EditorConfig support (Visual Studio, Rider, VS Code)

HVO.Core and HVO.Core.SourceGenerators target **.NET Standard 2.0** for broad compatibility. Tests target .NET 10.

## Getting Started

```bash
git clone https://github.com/RoySalisbury/HVO.SDK.git
cd HVO.SDK
dotnet build
dotnet test
```

Both commands must complete with **zero errors and zero warnings** — `TreatWarningsAsErrors` is enabled across all projects.

## Coding Standards

Follow the conventions in [`.github/copilot-instructions.md`](.github/copilot-instructions.md) and the repository `.editorconfig`. Key points:

- Use `Result<T>` / `Option<T>` patterns for error handling — no throwing exceptions for expected failures.
- XML documentation on all public APIs.
- Keep methods small and focused.

## Branch Naming

| Prefix | Purpose |
|--------|---------|
| `feature/` | New features |
| `bugfix/` | Bug fixes |
| `hotfix/` | Urgent production fixes |
| `chore/` | Maintenance, CI, docs |
| `refactor/` | Code refactoring |

Example: `feature/add-retry-policy`, `bugfix/option-none-equality`

## Commit Messages

Use [Conventional Commits](https://www.conventionalcommits.org/):

```
feat: add Retry<T> utility to HVO.Core
fix: correct Option<T> equality for null values
docs: update README quick-start examples
chore: bump dependabot schedule to weekly
test: add coverage for Guard.AgainstNegativeOrZero
```

## Pull Request Workflow

1. Create a branch from `main` using the naming convention above.
2. Make your changes — keep PRs focused and small.
3. Run `dotnet build` and `dotnet test` locally. Both must pass with zero warnings.
4. Push your branch and open a PR against `main`.
5. Fill out the PR template checklist.
6. PRs are **squash-merged** into `main`.

## PR Checklist

Before submitting, verify:

- [ ] `dotnet build` — zero errors, zero warnings
- [ ] `dotnet test` — all tests pass
- [ ] No new warnings introduced
- [ ] XML documentation added for public APIs
- [ ] Documentation updated (if applicable)
- [ ] Issue linked (`Resolves #N`)

## Reporting Issues

Use the [issue templates](https://github.com/RoySalisbury/HVO.SDK/issues/new/choose) for bug reports and feature requests.

## License

By contributing, you agree that your contributions will be licensed under the [MIT License](LICENSE).
