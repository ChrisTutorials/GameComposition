# Framework Boundaries: GameComposition vs GameUserSessions

## Purpose

This document defines **clear ownership boundaries** between:

- **GameComposition** (composition primitives: DI + scoping + settings)
- **GameUserSessions** (identity + session boundaries)

Goal: avoid duplicated identity types and avoid mixing domain/session logic into DI primitives.

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

- There must be **exactly one canonical `UserId`** across the framework.
- Canonical owner: **GameUserSessions.Core (`GameUserSessions.Core.UserId`)**.
- GameComposition consumes that type.

Rationale:

- Avoid conversion/adapters between `UserId` types.
- Keep identity/session semantics close to the session library.
- Keep GameComposition focused on composition primitives.

---

## Dependency direction

Recommended direction:

- `GameComposition.Core` **references** `PlayerSessions.Core` (identity types; namespaces are `GameUserSessions.*`)
- `PlayerSessions.Core` does **not** reference GameComposition

This keeps identity/session domain clean and prevents DI concerns from bleeding upward.

---

## Distribution guidance (DLL vs NuGet)

- DLL distribution: ship both `GameComposition.Core.dll` and `PlayerSessions.Core.dll` together.
- NuGet distribution: `GameComposition.Core` package will bring `PlayerSessions.Core` as a dependency.

---

## Notes

- This document does not change any GDScript `class_name` contracts.
- Engine adapters (Godot/Unity) remain thin and should bind identity/session context into the composition layer.
