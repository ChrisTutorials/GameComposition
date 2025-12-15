---
description: GameComposition production readiness roadmap (C# + GDScript)
---

# GameComposition Production Readiness Roadmap

## Scope

This roadmap covers making **GameComposition** production-ready as:

- `GameComposition.Core` (pure C#, engine-agnostic, NuGet packable)

**Note (2025-12-15)**: The GDScript backlayer is deprecated and will be removed.
GameComposition will be maintained as **C# only** going forward.

## Horizon

- Target: **v1.0.0** readiness
- Intermediate: **v0.2.x** stabilization milestones

## Milestones

### Milestone 1 — “Measurable Health” (CI + tests + packaging baseline)
- Objective: Every change is validated by automated checks; packaging artifacts are consistent.

### Milestone 2 — “API Surface Frozen” (v1 public contract)
- Objective: Public APIs and behavior contracts are stable; deprecations are explicit.

### Milestone 3 — “Production Host Integration” (reference integrations)
- Objective: Provide minimal, canonical integration samples for at least Godot (and optionally Unity).

### Milestone 4 — “Release Candidate” (v1.0.0)
- Objective: Critical lane is empty; High lane is either done or explicitly deferred.

---

## 🔴 Critical

1. [ ] **CI green for build + tests + pack**
   - [x] Add a CI workflow that runs: (2025-12-15)
     - [x] `dotnet build` for `GameComposition.sln`
     - [x] `dotnet test` for `GameComposition.Core.Tests.csproj`
     - [x] `dotnet pack` for `GameComposition.Core.csproj`
   - [ ] Done when:
     - [ ] CI runs on PR + main
     - [ ] CI is consistently green on `main`
   - Depends on: none

2. [ ] **Production packaging correctness (NuGet)**
   - [ ] Validate NuGet metadata in `GameComposition.Core.csproj`:
     - [x] `Version` strategy (pre-1.0) is documented: `docs/VERSIONING_POLICY.md` (2025-12-15)
     - [ ] `PackageId`, license, repo URL correct
     - [ ] XML docs are generated and included
     - [ ] Symbols package (`snupkg`) generated
   - [x] Add a “pack verification” script (local) to ensure: (2025-12-15)
     - [x] Package contains expected assemblies + XML
     - [x] No accidental debug artifacts
   - [ ] Done when:
     - [ ] `dotnet pack` output is reproducible
     - [ ] Package contents are validated in CI
   - Depends on: CI green

3. [ ] **Hard behavior contracts for DI primitives (C#)**
   - [ ] Audit `ServiceRegistry`, `ServiceScope`, lifetime rules, disposal semantics
   - [ ] Ensure tests explicitly cover:
     - [ ] duplicate registration behavior
     - [ ] scoped caching behavior
     - [ ] disposal behavior (registry and scopes)
     - [ ] exception messages are stable/meaningful
   - [ ] Done when:
     - [ ] tests cover all public DI behaviors
     - [ ] breaking behavior changes require a version bump + changelog
   - Depends on: CI green

4. [ ] **Framework boundaries and canonical identity**
   - [x] Define ownership boundaries (composition vs session domain): `docs/FRAMEWORK_BOUNDARIES_GAMECOMPOSITION_VS_GAMEUSERSESSIONS.md` (2025-12-15)
   - [x] Canonical `UserId` lives in GameUserSessions (`GameUserSessions.Core.UserId`) (2025-12-15)
   - [ ] Done when:
     - [ ] downstream plugins no longer reference `GameComposition.Core.Types.UserId`
     - [ ] DLL distribution bundles both `GameComposition.Core.dll` and `PlayerSessions.Core.dll`
   - Depends on: none

5. [x] **GDScript backlayer (deprecated)**
   - Decision: do not build or ship a GDScript composition framework.
   - Done when:
     - [ ] `gdscript/` folder is removed from the repo.
     - [ ] docs no longer reference `gdscript/composition`.
   - Depends on: none

---

## 🟠 High

1. [ ] **API freeze plan + deprecation policy**
   - [ ] Define what is considered public API:
     - [ ] namespaces, types, extension points
     - [ ] DI exceptions and error messages
   - [ ] Add `CHANGELOG.md` and document SemVer policy
   - [ ] Done when:
     - [ ] public API list is documented
     - [ ] deprecation workflow is documented

2. [ ] **Integration samples (canonical reference implementations)**
   - [ ] Godot sample:
     - [ ] minimal “composition root” example that creates registry + scopes
     - [ ] demonstrates `UserId` / `IUserScope` patterns
   - [ ] (Optional) Unity sample:
     - [ ] minimal composition root
   - [ ] Done when:
     - [ ] samples compile/run and are referenced in docs

3. [ ] **Observability hooks**
   - [ ] Ensure `ServiceRegistryStatistics` is tested and documented
   - [ ] Add lightweight logging hooks pattern (interfaces, not hard dependencies)
   - [ ] Done when:
     - [ ] hosts can introspect service graph/usage without reflection hacks

4. [ ] **Thread-safety policy**
   - [ ] Decide and document:
     - [ ] thread-safe or not
     - [ ] expected usage model (main-thread typical)
   - [ ] Done when:
     - [ ] docs clearly state supported concurrency expectations

---

## 🟡 Medium

1. [ ] **Docs improvements for adoption**
   - [ ] Add quickstart to `README.md`:
     - [ ] create registry
     - [ ] register services
     - [ ] create scope
     - [ ] resolve services
   - [ ] Add “Patterns” doc:
     - [ ] global scope vs user scope guidance
     - [ ] settings provider usage

2. [ ] **C# analyzers / code quality gates**
   - [ ] Add `.editorconfig` / rulesets if missing
   - [ ] Ensure nullable annotations and warnings are clean

3. [ ] **GDScript parity & maintenance policy**
   - [ ] Document what is guaranteed to match C# semantics
   - [ ] Document what is intentionally different (if any)

---

## 🟢 Low

1. [ ] **Performance micro-benchmarks**
   - [ ] Benchmark registry resolve overhead for typical usage
   - [ ] Document expected complexity and best practices

2. [ ] **Extra helper utilities**
   - [ ] optional syntactic sugar helpers (only if adoption pain shows up)

3. [ ] **Additional engine adapters**
   - [ ] expanded Unity glue patterns

---

## Notes / Constraints

- `GameComposition.Core` must remain **engine-agnostic** (no Godot/Unity references).
- No GDScript framework backlayer is maintained for GameComposition.
