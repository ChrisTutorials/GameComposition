# GameComposition Architecture

## Purpose

GameComposition provides **engine-agnostic composition primitives** for ChrisTutorials plugins.

It exists to standardize how plugins are wired together at runtime without forcing:

- a specific DI framework
- a global service locator
- any engine dependency (Godot/Unity)

## Non-Goals

GameComposition does **not** own:

- gameplay behavior (placement/targeting/manipulation/etc.)
- engine nodes/scenes/input events
- persistence, save/load, UI architecture

Those remain plugin-level or game-level responsibilities.

## Layering Model

### Core (this repo)

- `GameComposition.Core` is **pure C#**.
- It provides:
  - service registry + scopes
  - minimal composition interfaces
  - user/scope identity primitives
  - settings provider primitives

### Plugins (consumers)

A typical plugin should follow:

- `*.Core`
  - domain + services + rules
  - depends on `GameComposition.Core`
- `*.Godot` / `*.Unity`
  - thin adapters
  - may implement engine-specific composition roots that **call into** Core services

## Naming & Compatibility Policy

- Runtime-only helper types should keep **parity by concept** across languages.
- For the 5.1 GDScript backlayer, **RefCounted-based** runtime helpers are the primary target for naming parity (e.g. `GCServiceRegistry`, `GCServiceScope`, `GCSettingsProvider`, `GCUserId`).
- **Do not** enforce parity for:
  - folder names
  - Godot `Node` / `Resource` class names
  - engine-facing scene/resource paths

Rationale:

- Renaming folders, `Node`s, or `Resource`s is a breaking change for existing Godot projects.
- RefCounted runtime helpers are safe to rename/align because they are code-level utilities and do not invalidate scene/resource serialization.

## Composition Concepts

### ServiceRegistry

`ServiceRegistry` is a lightweight container for:

- singletons
- transient factories
- scoped factories

It is intentionally simple so it can be used in:

- unit tests
- headless tools
- Godot/Unity hosts

### Scopes

`ServiceScope` provides per-scope caching for scoped services.

Recommended conceptual scopes:

- **Global scope**: application/session-wide infrastructure (logging, config).
- **User scope**: per-player/per-owner state and services.
- **Scene scope** (optional): per-level/per-world state if the host wants it.

The **host** owns scope lifetime and disposal.

### User Identity

`UserId` / `UserScopeProfile` / `IUserScopeProfileProvider` exist to standardize
"who owns this scope" across plugins.

Rule of thumb:

- If a service contains per-player state, it belongs in a user scope.
- The host chooses how to map engine/game concepts to `UserId`.

### Settings

`ISettingsProvider<T>` + `StaticSettingsProvider<T>` are small utilities that:

- keep services testable
- avoid static global configuration
- allow hot-swappable implementations later (file-backed, live-edit, etc.)

## Supported Integration Approaches

### Approach A: GameComposition-first (recommended for multi-plugin projects)

Use `GameComposition.Core` as the shared foundation:

- the host constructs a `ServiceRegistry`
- plugins provide registration modules/registrars
- the host creates scopes (global/user) and wires adapters

Benefits:

- consistent patterns across plugins
- predictable scoping rules
- easier cross-plugin integration

### Approach B: Host-agnostic (manual wiring / alternate DI)

GameComposition does not require `ServiceRegistry`.

A host can:

- construct services manually
- use a different DI container
- still use some GameComposition primitives (e.g., `UserId`, `ISettingsProvider<T>`)

Rule:

- plugins should not assume a global container or a specific DI framework

## Anti-Patterns (avoid)

- **Global Service Locator**: plugin code calling into a static registry at runtime.
- **Plugin-owned container lifetime**: plugin creates/owns the composition root for the entire game.
- **Engine-dependent Core**: adding Godot/Unity references to `GameComposition.Core`.
- **Gameplay in composition**: moving gameplay rules/services into GameComposition.

## Testing Guidance

- `GameComposition.Core.Tests` should validate:
  - registry registration and retrieval
  - scope caching/disposal behavior
  - exception behavior

- Plugins should test:
  - behavior in `*.Core` with pure unit tests
  - adapters with thin wiring smoke tests
