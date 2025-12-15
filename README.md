# GameComposition

Engine-agnostic composition primitives for ChrisTutorials plugins.

## Goals
- Provide a **pure C#** foundation for composition and runtime wiring.
- Be compatible with **Godot** and **Unity** (no engine dependencies).
- Standardize a **per-plugin composition glue** approach:
  - `*.Core` (pure C# domain + services)
  - `*.Godot` / `*.Unity` (thin adapters)
  - `composition/` folder (wiring + glue, minimal logic)

## What lives here (initial extraction from GridPlacement)
- Service registry + scopes
  - `ServiceRegistry`
  - `IServiceScope`, `ServiceScope`
  - DI exceptions + lifetime types
- Service context + user scope identity
  - `IServiceContext`
  - `IUserScope`, `UserId`, `UserScopeProfile`, `IUserScopeProfileProvider`
- Settings providers
  - `ISettingsProvider<T>`, `StaticSettingsProvider<T>`

## What does *not* live here
- Gameplay logic (placement, targeting, manipulation)
- Engine-specific nodes / scenes / input events

## Status
Work-in-progress extraction. Until packaging is finalized, downstream plugins reference this via `ProjectReference`.

## Architecture

See: `docs/ARCHITECTURE.md`
