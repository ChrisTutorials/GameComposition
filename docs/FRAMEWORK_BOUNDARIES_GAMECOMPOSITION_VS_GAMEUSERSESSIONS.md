# Framework Boundaries: GameComposition vs GameUserSessions

## Purpose

This document defines **clear ownership boundaries** between:

- **GameComposition** (composition primitives: DI + scoping + settings)
- **GameUserSessions** (identity + session boundaries)

Goal: avoid duplicated identity types and avoid mixing domain/session logic into DI primitives.

In addition, this document defines a **hard dependency rule**:

- `GameComposition.Core` and `GameUserSessions.Core` must be **agnostic of each other**.
- Any bridging between the two is allowed only in **explicit integration suites/adapters**, and those must live **outside** both Core assemblies.

---

## What GameComposition owns

**GameComposition.Core** is an engine-agnostic composition utility library.

It owns:

- **Service wiring primitives**
  - `ServiceRegistry`
  - `ServiceScope` / `IServiceScope`
  - lifetime/disposal semantics
- **Settings provider primitives**
  - `ISettingsProvider<T>`
  - `StaticSettingsProvider<T>`
- **Scope/profile concepts** (composition-level)
  - `UserScopeProfile` (a *composition* profile: id + display name)
  - `IUserScope`, `IUserScopeRoot`, `IUserScopeProfileProvider`

It must NOT own:

- session membership rules
- session lifecycle
- authentication/profile storage
- game-specific “who is the local player” logic

---

## What GameUserSessions owns

**GameUserSessions.Core** (namespace `GameUserSessions.Core`, DLL name may remain `PlayerSessions.Core.dll` for now) is an engine-agnostic identity/session library.

It owns:

- **Canonical identity value objects**
  - `UserId`
  - `GameSessionId`
- **Session boundaries + membership**
  - `IPlayerSessionManager`
  - `ICurrentSessionContext`

It must NOT own:

- DI container / service locator
- plugin wiring/composition roots
- engine-specific node logic in core

---

## Canonical identity policy (IMPORTANT)

Core policy:

- `GameComposition.Core` and `GameUserSessions.Core` must not share identity types via direct references.
- If a single canonical identity type is desired, it must live in a **third shared package** (e.g. a small `*.Primitives` library) that both cores can depend on without depending on each other.
- If a shared primitives package is not used, each core may define its own identity representation, and **integration** is responsible for mapping between them.

Rationale:

- Avoid conversion/adapters between `UserId` types.
- Keep identity/session semantics close to the session library.
- Keep GameComposition focused on composition primitives.

---

## Dependency direction

Required direction:

- `GameComposition.Core` does **not** reference `GameUserSessions.Core`.
- `GameUserSessions.Core` does **not** reference `GameComposition.Core`.
- Any dependency between the two must exist only in:
  - dedicated `*.Integration.*` projects
  - dedicated `*.Integrations.*.Tests` suites
  - engine adapters (Godot/Unity) that stitch together the two domains at the application boundary

This keeps identity/session domain clean, prevents cyclic coupling, and ensures each Core library remains usable in isolation.

---

## Distribution guidance (DLL vs NuGet)

- If identity is shared via a third package:
  - DLL distribution: ship `GameComposition.Core.dll` + `*.Primitives.dll` (and optionally `GameUserSessions.Core.dll` if the host uses it).
  - NuGet distribution: `GameComposition.Core` should depend only on the shared primitives package.
- If identity is mapped in integrations:
  - DLL distribution: ship `GameComposition.Core.dll` and keep any `GameUserSessions` artifacts as host/integration dependencies.
  - NuGet distribution: avoid expressing a hard dependency between cores; integrations may bring both.

---

## Notes

- This document does not change any GDScript `class_name` contracts.
- Engine adapters (Godot/Unity) remain thin and should bind identity/session context into the composition layer.
