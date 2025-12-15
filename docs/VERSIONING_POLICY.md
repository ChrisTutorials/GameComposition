# Versioning Policy (Pre-1.0)

## Scope

This policy applies to:

- `GameComposition.Core` (NuGet package)
- The accompanying GDScript backlayer in `gdscript/composition/`

## Goals

- Keep consumers unblocked while the library is still evolving.
- Make **breaking changes explicit** and easy to detect.
- Avoid silent behavior changes.

## SemVer interpretation before v1.0.0

We follow SemVer with the common pre-1.0 convention:

- **0.MINOR.PATCH**
  - **MINOR** may include breaking changes.
  - **PATCH** is for bug fixes and non-breaking improvements.

Practical rule:

- If you change public API or behavior in a way that can break consumers, bump **MINOR**.
- If it’s backward compatible, bump **PATCH**.

## What counts as “public API”

### C# (`GameComposition.Core`)

Public API includes:

- `public` types, methods, properties, and constructors
- Exception types and their intended semantics
- Observable behavior contracts (e.g., disposal behavior, scope caching)

Not considered public API:

- `internal` members
- test-only helpers

### GDScript backlayer

Public API includes:

- `class_name` scripts and their public methods
- documented usage patterns in `docs/`

## Required changes for breaking updates

When a breaking change is introduced:

- Bump **MINOR**
- Add a `CHANGELOG.md` entry (once `CHANGELOG.md` exists)
- Update relevant docs (usage samples / patterns)
- Add/adjust tests that lock the new behavior

## Release checklist (local)

- `dotnet build -c Release`
- `dotnet test -c Release`
- `dotnet pack -c Release`
- `bash scripts/verify-pack.sh artifacts`

