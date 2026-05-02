# Contributing to SLOY OS

## Project Status
Closed development. External contributions not accepted at this stage.

## Internal Development Guidelines

### Code Style
- C# 12, .NET 8
- PascalCase for public members
- camelCase for private fields (with underscore prefix)
- Interface names start with I
- No regions
- File-scoped namespaces

### Branch Strategy
- `main` — stable, release-ready
- `dev` — active development
- `feature/*` — new features
- `fix/*` — bug fixes
- `release/*` — release preparation

### Commit Messages
type(scope): description

feat: new feature

fix: bug fix

refactor: code restructuring

test: tests

docs: documentation

chore: maintenance

text

### Testing
- Unit tests in `SLOY.Tests/`
- Integration tests in `SLOY.IntegrationTests/`
- Run `dotnet test` before pushing
- Minimum 70% coverage for new code

### Pull Requests
1. Rebase on latest dev
2. All tests pass
3. Code review by at least 1 maintainer
4. Squash merge