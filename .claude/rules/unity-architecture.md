# Rule: Unity Architecture Invariants

Consolidates: `no-runtime-global-search`, `event-bus-only-gameplay-communication`, `save-dto-simple-types-only` (originals are stubs pointing here).

## 1. No runtime global scene search

Prohibited in runtime code (`Assets/_Game/Scripts/**`, except `Editor/`): `GameObject.Find`, `FindObjectOfType`, `FindObjectsOfType`, `FindObjectsByType`.

Allowed: editor tools, serialized references, GameBootstrap injection, explicit configuration methods, narrow same-object `GetComponent<T>()`.

If a reference is missing, log a clear wiring error (scene, GameObject, component, missing field, affected id) — never mask it with a silent scene search.

> **Known debt (decision pending):** ~14 runtime files use `FindObjectOfType`, mostly `*RuntimeBootstrap` self-wiring classes (Quests, Craft, NPC/Schedule, Farm, Player/Movement, World/Scenes, Cave). Until the human blesses or bans that idiom (see skill `runtime-bootstrap-pattern`), do **not** copy it into new code; the `runtime-code-guard` hook flags new occurrences.

## 2. Gameplay communication only via GameEventBus

`GameEventBus.Publish()` / `Subscribe()` for all gameplay communication (combat, farming, inventory, UI state, NPC, cave, day cycle, economy). Direct MonoBehaviour-to-MonoBehaviour gameplay calls are prohibited.

Allowed exceptions: editor tools; GameBootstrap wiring refs; same-object `GetComponent` for setup; Unity lifecycle internals. A spec may authorize a direct call explicitly with reason.

## 3. Save DTOs: simple types + stable IDs only

Prohibited in save DTOs: any Unity object reference (`ScriptableObject`, `GameObject`, `Transform`, `MonoBehaviour`, `Sprite`, components).

Allowed: `string`, `int`, `float`, `bool`, enums, lists/arrays of simple values, nested simple DTOs, stable IDs.

Pattern: persist IDs; resolve objects after load via registries/bootstrap; keep migrations backward-compatible; document compatibility in `docs/validation/` when schema changes.

## 4. Forbidden namespaces

`CindarsHope.Debug`, `CindarsHope.Temp` (use `CindarsHope.DebugTools`).

## Enforcement

- Hook `runtime-code-guard.ps1` (PostToolUse) flags forbidden search APIs and namespaces in newly written code.
- `/review-non-regression` audits the full diff before closeout.
