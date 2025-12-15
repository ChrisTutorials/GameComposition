# GDScript Composition Smoke Guide (Godot 4.x)

## Purpose

This is a minimal, manual smoke guide to verify that the `gdscript/composition` backlayer:

- loads without parse errors
- supports basic registry + scope flows
- supports settings-provider usage

## Files

- `gdscript/composition/service_registry.gd`
- `gdscript/composition/service_scope.gd`
- `gdscript/composition/settings_provider.gd`
- `gdscript/composition/user_id.gd`

## Smoke steps (manual)

1. Create a clean Godot 4.x project.
2. Copy the `gdscript/composition/` folder into your project (any path is fine).
3. Create a small script (e.g. `res://smoke_test.gd`) and run it once.

## Minimal script outline

The following describes what the smoke script should do:

- Instantiate `GCServiceRegistry`.
- Register:
  - a singleton value
  - a factory service
  - a scoped factory service
- Resolve:
  - singleton directly from registry
  - factory directly from registry (ensure new instance each call)
  - scoped service through a `GCServiceScope` (ensure cached within the same scope)
- Instantiate `GCStaticSettingsProvider` and verify `get_settings()` returns the instance.

## Expected results

- No parse errors on load.
- No runtime errors during the smoke run.
- Singleton resolves as the same instance.
- Factory resolves as different instances across calls.
- Scoped resolves as the same instance within a scope and different across scopes.

## Notes

- This backlayer is intentionally small and should remain deterministic.
- Avoid relying on `uid://` references in these scripts; prefer `res://` or relative paths where possible.

