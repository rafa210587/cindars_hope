# Rule: C# Style & Craft

General C# craft conventions for runtime code. This rule is about *how a single class/method reads*; architectural invariants (no global search, GameEventBus, save DTOs, forbidden namespaces) live in [unity-architecture.md](./unity-architecture.md) and are not repeated here.

## Naming & shape

- Domain-oriented, explicit names. No `Manager`/`Helper`/`Utils` catch-alls when a domain noun exists.
- Small classes, one clear responsibility per method. If a method needs a section comment to explain a block, that block is usually a method.
- `readonly` for dependencies that never change after construction; `private set`/`init` over public mutable properties without a reason.
- New types `sealed` by default; open for inheritance only when a base/subtype relationship is intended and documented.

## Failure signaling (use the project's existing convention — do not invent a parallel one)

- **Expected gameplay failures** (cooldown active, invalid target, no stamina, missing item, save absent): return a `bool` from a `TryX(...)` method and/or surface a `FailureReason` string via a `GameEventBus` HUD event — never a silent no-op, never an exception as control flow. (See game-feel-checklist: refusals must surface a reason.)
- Before introducing a `Result<T>`/`Either`/`Outcome` wrapper, run skill `system-reuse-audit` — the project already signals failures with `bool` + `FailureReason`; a competing wrapper type is a parallel system the `runtime-code-guard` ethos forbids.
- **Broken invariants** (required dependency null, impossible state, unsupported save version): throw or assert — fail fast. See [error-handling-resilience.md](./error-handling-resilience.md) for the full taxonomy.

## Collections & immutability

- Expose `IReadOnlyList<T>` / `IReadOnlyCollection<T>` when the caller must not mutate.
- Prefer `TryGet...(out var x)` over methods that return `null` and force the caller to null-check.
- Don't hand out references to internal mutable lists; copy or wrap.

## Async & frame-cost awareness

- No `async void` except framework/Unity event handlers that require it.
- No allocation-heavy idioms (LINQ, closures capturing locals, boxing, per-frame string interpolation) in `Update`/`FixedUpdate`/`LateUpdate`/collision callbacks or per-entity loops. Hot-path detail and the authoritative checklist live with agent `performance-auditor` and skill `object-pooling-pattern`.
- No `DateTime.Now` / `UnityEngine.Random` (global state) in logic that must be deterministic or saved — use seeded `System.Random` per system (skill `rng-and-determinism`).

## Enforcement

Style is reviewed, not hard-gated: `/code-review`, `/review-non-regression`, and agent `architecture-reviewer`. The `runtime-code-guard` hook already blocks the architectural subset (search APIs, forbidden namespaces, duplicate classes).
