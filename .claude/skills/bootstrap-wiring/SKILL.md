---
name: bootstrap-wiring
description: Wire runtime services and scene data through existing composition/domain installers and Editor generators. Use for new service wiring, legacy bootstrap migration or missing serialized dependencies.
---

# Skill: Bootstrap and composition wiring

**Reuse the current composition owner; new wiring must not add global lookup or auto-bootstrap.**
Inspect `Assets/_Game/Scripts/Composition/GameRuntimeCompositionRoot.cs`, the relevant installer
in `DomainRuntimeInstallers.cs` and affected consumers only when service composition changes.
For a local serialized-reference repair, inspect its scene generator and consumer first.

## Choose the existing boundary

| Target | Route |
|---|---|
| Persistent runtime service | Existing domain installer/composition root; preserve install order and disposal. |
| Scene component/data asset | Explicit serialized reference or injected dependency through the responsible Editor generator/scene installer. |
| Legacy auto-bootstrap | Scoped migration or repair via `runtime-bootstrap-pattern`; no new auto-installer. |
| Save provider | Existing typed provider registry via `save-section-provider`. |
| HUD/interactable | Existing `hud-canvas-binding` or `scene-interactable-wiring` contract. |

## Implement within the contract

1. Identify state owner, lifetime, required/optional dependencies and who registers/unregisters.
   Read actual APIs; do not add a parallel manager or assume an Install signature from an old example.
2. Add the smallest binding to the current installer or generator. Installation/rebinding is
   idempotent where the contract requires it; configure before activation exposes callbacks.
3. New services do not use `RuntimeInitializeOnLoadMethod`, static service locators or the
   legacy GameBootstrap as an alternate composition root. Existing adapters may still reference
   GameBootstrap under their current contract; do not migrate unrelated consumers incidentally.
4. Missing required dependencies produce clear wiring diagnostics. Do not add `Resources.Load`
   to hide a generator omission or increase ratchet debt. Existing Resources compatibility
   fallbacks remain only where the approved contract requires them; do not delete them blindly.
5. Materialize assets/scenes with the existing Unity Editor pipeline, never manual YAML.
   Include the relevant generator when introducing a serialized dependency. Keep menu operations
   in the existing orchestration commands, with no incidental package/settings change.
6. Preserve persistent-service identity across scene transitions; scene-local views/subscribers
   must detach and bind to that owner. Publish gameplay facts after owner initialization/commit.

## Validation and limits

Use the canonical matrix: selected binding checks plus actual lifecycle/transition smoke when
that behavior changes. Test repeated installation, missing dependencies and unload/rebind as
applicable; include save provider identity when affected. Reuse equivalent Test Runner compile
evidence instead of requiring a separate build. A runtime null check does not prove wiring.

If the needed owner/asset is outside approved scope, report the concrete dependency and continue
independent work. Do not evade ownership or Editor requirements with a fallback loading path.
