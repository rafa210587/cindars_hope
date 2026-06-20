# Rule: Testing Quality Gate

Mudanças de código devem incluir o nível certo de cobertura de teste automatizada ou documentada.

Esta rule complementa a validação de compile do Unity. Sucesso de compile prova que o projeto builda; não prova behavior, persistência, imports, event wiring, UI flow ou regression safety.

---

## Core rule

Toda implementation spec que muda runtime behavior deve declarar seu impacto de teste antes do closeout.

O execution report deve dizer uma das opções:

```text
Automated tests added/updated: YES
Automated tests not added: JUSTIFIED
Manual Play Mode scenario required: YES/NO
Residual risk: <explicit risk>
```

Uma spec de runtime/gameplay não pode ser marcada `ACCEPTED` só porque o build/compile passou.

---

## Mandatory automated tests

Automated tests são obrigatórios quando uma spec muda deterministic logic que pode rodar fora de live scene interaction.

Isto inclui:

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

Tipo de teste preferido:

```text
EditMode tests for pure C# and deterministic rules.
```

---

## Mandatory Play Mode or manual scenario

Play Mode automated test ou human Play Mode scenario é obrigatório quando o behavior depende de:

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

Se Play Mode automatizado não for prático, crie um human scenario em:

```text
docs/validation/playmode/<spec_id>_human_test_scenario.md
```

O execution report deve linká-lo.

---

## Bugfix regression rule

Todo bugfix deve incluir uma das opções:

```text
a regression test that fails before the fix and passes after;
a targeted validator/checklist when automation is not practical;
a written justification explaining why automation is not possible yet.
```

Um bugfix sem regression coverage deve ser reportado como residual risk.

---

## Import and compile safety

Quando arquivos `.cs` mudam:

```text
dotnet build is a fallback compile signal;
Unity batchmode compile is the authoritative Unity compile signal;
log scan must be checked when Unity compile runs;
new missing using/import/namespace/type errors must be fixed before closeout;
```

Não esconda import ou assembly errors atrás de um closeout documentation-only.

---

## Save/load test expectations

Toda spec que muda save/load deve testar ou justificar explicitamente a ausência de teste para:

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

Toda spec que muda o behavior do quest system deve testar ou justificar explicitamente a ausência de teste para:

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

Specs UI-heavy devem, no mínimo, fornecer um human scenario cobrindo:

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

Automated UI tests são preferidos apenas quando estáveis o suficiente para manter.

---

## Allowed justification for no automated tests

Automated tests só podem ser pulados com justificativa explícita, tais como:

```text
purely documentation-only change;
asset-only change with Unity validation/manual scenario;
scene/prefab wiring that requires Unity Editor inspection;
preexisting test harness gap being closed by a future foundation spec;
```

A justificativa deve nomear o harness ausente ou o blocker.

---

## Closeout impact

Status máximo permitido se faltam testes sem justificativa:

```text
PARTIAL
```

Status máximo permitido para specs de runtime/gameplay sem evidência de Play Mode automatizado ou human scenario:

```text
BUILD_VALIDATED
```

Uma spec só pode virar `ACCEPTED` quando seu nível de validação exigido é satisfeito ou existe uma exceção explícita aprovada por humano.

---

## Required report block

Todo implementation report para uma spec que muda código deve incluir:

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

Não afirme:

```text
unit tests passed;
EditMode tests passed;
PlayMode tests passed;
regression covered;
feature accepted;
```

a menos que a evidência exista no execution report.
