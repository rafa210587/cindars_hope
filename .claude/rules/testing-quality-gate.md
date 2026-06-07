# Rule: Testing Quality Gate

Code changes must include the right level of automated or documented test coverage.

This rule complements Unity compile validation. Compile success proves the project builds; it does not prove behavior, persistence, imports, event wiring, UI flow, or regression safety.

---

## Core rule

Every implementation spec that changes runtime behavior must declare its test impact before closeout.

The execution report must say one of:

```text
Automated tests added/updated: YES
Automated tests not added: JUSTIFIED
Manual Play Mode scenario required: YES/NO
Residual risk: <explicit risk>
```

A runtime/gameplay spec cannot be marked `ACCEPTED` only because build/compile passed.

---

## Mandatory automated tests

Automated tests are required when a spec changes deterministic logic that can run outside live scene interaction.

This includes:

```text
save/load DTO normalization;
save/load migrations;
save section providers;
stable IDs and registries;
invalid ID fallback rules;
quest conditions;
quest triggers;
quest reward idempotency;
quest flags;
economy pricing;
shop stock refresh rules;
inventory transactions;
stack/split/move rules;
equipment durability calculations;
repair/upgrade rules;
combat formulas;
status effect rules;
skill tree purchase/respec rules;
active slot validation;
calendar/day transition logic;
weather generation;
lunar cycle logic;
crafting/processing job timing;
bestiary knowledge state;
event bus publish/subscribe contracts;
parsers, adapters, pure services, validators and rule engines.
```

Preferred test type:

```text
EditMode tests for pure C# and deterministic rules.
```

---

## Mandatory Play Mode or manual scenario

Play Mode automated test or human Play Mode scenario is required when behavior depends on:

```text
scene objects;
Unity lifecycle;
input;
UI canvas;
modal stack;
prefabs;
ScriptableObject asset wiring;
physics/colliders;
NPC schedules in scene;
cave procedural scene runtime;
player movement/combat feel;
shop/dialogue interaction;
Fonte interaction;
corpse recovery flow.
```

If automated Play Mode is not practical, create a human scenario under:

```text
docs/validation/playmode/<spec_id>_human_test_scenario.md
```

The execution report must link it.

---

## Bugfix regression rule

Any bugfix must include one of:

```text
a regression test that fails before the fix and passes after;
a targeted validator/checklist when automation is not practical;
a written justification explaining why automation is not possible yet.
```

A bugfix without regression coverage must be reported as residual risk.

---

## Import and compile safety

When `.cs` files change:

```text
dotnet build is a fallback compile signal;
Unity batchmode compile is the authoritative Unity compile signal;
log scan must be checked when Unity compile runs;
new missing using/import/namespace/type errors must be fixed before closeout;
```

Do not hide import or assembly errors behind documentation-only closeout.

---

## Save/load test expectations

Any spec that changes save/load must test or explicitly justify lack of test for:

```text
save DTO default values;
legacy/missing section behavior;
invalid ID fallback;
round-trip capture/restore where practical;
no Unity references in DTOs;
reward/state idempotency after reload when relevant;
```

---

## Quest test expectations

Any spec that changes quest system behavior must test or explicitly justify lack of test for:

```text
condition evaluation;
trigger handling;
reward idempotency;
quest flag set/clear;
objective progress;
spoiler visibility;
anti-softlock fallback for critical quests;
save/load of quest state;
```

---

## UI test expectations

UI-heavy specs must at minimum provide a human scenario covering:

```text
open/close;
Esc/back behavior;
input blocking;
focus order;
confirmation modal;
empty state;
error state;
no gameplay movement while modal/dialogue is open;
```

Automated UI tests are preferred only when stable enough to maintain.

---

## Allowed justification for no automated tests

Automated tests may be skipped only with explicit justification, such as:

```text
purely documentation-only change;
asset-only change with Unity validation/manual scenario;
scene/prefab wiring that requires Unity Editor inspection;
preexisting test harness gap being closed by a future foundation spec;
```

The justification must name the missing harness or blocker.

---

## Closeout impact

Maximum allowed status if tests are missing without justification:

```text
PARTIAL
```

Maximum allowed status for runtime/gameplay specs without Play Mode automated or human scenario evidence:

```text
BUILD_VALIDATED
```

A spec can become `ACCEPTED` only when its required validation level is satisfied or an explicit human-approved exception exists.

---

## Required report block

Every implementation report for a code-changing spec must include:

```text
Testing Quality Gate
────────────────────
Changed runtime code: YES/NO
Changed deterministic logic: YES/NO
Changed Unity scene/prefab/asset wiring: YES/NO
Automated tests added/updated: YES/NO
Automated tests command: <command or NOT RUN>
Manual Play Mode scenario: <path or NOT REQUIRED>
Justification if no automated tests: <text or N/A>
Residual risk: <text>
```

---

## Never claim

Do not claim:

```text
unit tests passed;
EditMode tests passed;
PlayMode tests passed;
regression covered;
feature accepted;
```

unless the evidence exists in the execution report.