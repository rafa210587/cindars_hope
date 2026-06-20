# Rule: Error Handling & Resilience

Classify every failure before handling it. The handling strategy is dictated by the category, and the categories map directly to systems that already exist in this project.

## The four categories

| # | Category | Examples | Handling in this project |
|---|----------|----------|--------------------------|
| 1 | **Expected gameplay** | invalid action, cooldown, no stamina, missing item, target out of range | `bool TryX` + `FailureReason` string surfaced via a `GameEventBus` HUD event (skill `game-feel-checklist`). Never an exception, never a silent no-op. |
| 2 | **Config / asset / content** | prefab missing, item without icon, quest points to invalid id, SO field unset | Catch at **editor/build time** with a validator (60-validator pattern, skill `editor-validator-authoring`); at runtime apply a safe, logged fallback. Mandatory config corruption fails fast in dev. |
| 3 | **Infrastructure** | save write failed, file corrupt/absent on load | Atomic save flow + post-load validation + recovery (skill `save-load-pattern`). Never overwrite a good save before the new one validates. |
| 4 | **Bug / broken invariant** | required dependency null, impossible state, unsupported save version | **Fail fast** in development (throw/assert) with a clear wiring log; do not mask with a scene search or a swallowed exception (rule `unity-architecture`). |

## Logs must carry context

A log line must let a human locate the failure without a debugger. Include: system, entity/id, scene/level, operation, relevant state, and the fallback applied (if any). Ban bare lines like `"Error loading data"`. For missing required references, log scene + GameObject + component + missing field + affected id (rule `unity-architecture`).

## Development vs. shipped build

- **Development:** fail early to surface bugs (category 4 throws/asserts).
- **Shipped:** recover where category 1–3 allows, but always log with context.
- **Never** hide an error by silently corrupting state (e.g., writing a partial save, continuing with a null that will NRE three frames later).

## What never happens

- Exception used as normal gameplay control flow (that's category 1 → `bool`/`FailureReason`).
- `catch { }` that swallows and continues (category 4 must fail fast; 2–3 must log + fallback).
- A "fallback" that produces an invalid state instead of a safe default (rule `data-driven-content`: prefer Null Object / safe default).

## Enforcement

Reviewed by skill `non-regression-review`, `/code-review`, and agent `bugfix-investigator` (which also requires a regression test per rule `testing-quality-gate`). No mechanical hook — this is a review lens.
