---
name: runtime-bootstrap-pattern
description: Repair or migrate existing RuntimeBootstrap wiring without new global searches or auto-bootstraps. New service composition uses bootstrap-wiring and existing domain installers.
---

# Skill: Legacy runtime bootstrap wiring

**Legacy auto-installation is debt to inspect, not the template for a new system.**
The canonical route is `GameRuntimeCompositionRoot` plus domain installers or explicit
scene wiring; see [bootstrap-wiring](../bootstrap-wiring/SKILL.md) when that boundary changes.

## Bounded repair

1. Find the existing owner, consumers and initialization/disposal order. Do not assume an old
   list of bootstrap classes still describes the current checkout.
2. For a missing scene reference, repair the responsible Editor generator/serialized binding.
   Preserve IDs, fields, scene ownership and concurrent work; use Unity Editor APIs for assets.
3. When a scoped migration is authorized, move installation into the existing composition path.
   Do not add `RuntimeInitializeOnLoadMethod`, global service points, searches or Resources
   fallback to conceal missing wiring. Preserve explicitly required legacy compatibility paths.
4. Required dependencies fail with an actionable wiring diagnostic; optional dependencies
   follow their declared fallback. Never silently search the scene to make a test pass.
5. Gameplay reacts through GameEventBus; lifetime/rebinding must not duplicate handlers or
   keep destroyed scene references. No polling another owner's state to simulate initialization.

## Evidence

Select the actual risk: binding completeness for a data repair; scene load/unload and repeated
initialization for lifecycle changes; save-provider identity if persistence binding changes.
An EditMode registration test alone does not prove a live transition. Use focused existing
tests/smokes, without mandatory global validation or human scenario for every wiring edit.

Existing out-of-scope debt may be noted in the task result; it does not force a new report or
an unrelated migration. A contract expansion returns to the approved scope/spec before editing.
