# Rule: Testing Quality Gate

**Compile passing não é suficiente.** Mudanças de código exigem evidência de teste ou justificativa explícita.

---

## Report block obrigatório (em todo implementation report)

```text
Testing Quality Gate
────────────────────
Changed runtime code:           YES/NO
Changed deterministic logic:    YES/NO
Changed Unity scene/prefab:     YES/NO
Automated tests added/updated:  YES/NO
Automated tests command:        <cmd ou NOT RUN>
Manual Play Mode scenario:      <path ou NOT REQUIRED>
Justification if no tests:      <texto ou N/A>
Residual risk:                  <texto>
```

---

## Quando usar EditMode tests (obrigatório)

Lógica determinística que roda fora de scene interaction exige EditMode tests em `Assets/_Game/Tests/EditMode/`:

```
save/load DTO normalization + migrations + section providers;
stable IDs + registries + invalid ID fallback;
quest conditions/triggers/reward idempotency/flags/objective progress;
economy pricing + shop stock + inventory transactions/stack/split/move;
equipment durability + repair/upgrade;
combat formulas + status effect rules;
skill tree purchase/respec + active slot validation;
calendar/day transition + weather + lunar cycle;
crafting/processing job timing;
bestiary knowledge state;
event bus publish/subscribe contracts;
parsers, adapters, pure services, validators, rule engines.
```

## Quando exigir Play Mode ou human scenario

Behavior que depende de scene/lifecycle/prefab/input exige Play Mode automatizado ou human scenario em `docs/validation/playmode/<spec_id>_human_test_scenario.md`:

```
scene objects, Unity lifecycle, input, UI canvas, modal stack;
prefabs, ScriptableObject asset wiring, physics/colliders;
NPC schedules, cave procedural runtime, player movement/feel;
shop/dialogue interaction, corpse recovery flow.
```

---

## Expectativas por domínio (cobrir ou justificar ausência)

| Domínio | Pontos obrigatórios |
|---------|---------------------|
| **save/load** | DTO defaults; legacy/missing section; invalid ID fallback; round-trip; sem Unity refs; reward idempotency |
| **quest** | condition eval; trigger; reward idempotency; flag set/clear; objective progress; spoiler visibility; anti-softlock; save/load |
| **UI** | open/close; Esc/back; input blocking; focus order; confirmation modal; empty/error state; sem movimento durante modal |

---

## Bugfix

Todo bugfix exige uma das opções: regression test que falha antes e passa depois; ou validator/checklist quando automação não é prática; ou justificativa escrita de por que não é possível. Bugfix sem cobertura = residual risk explícito.

## Justificativas permitidas para pular automação

- mudança docs-only ou asset-only com manual scenario
- wiring de scene/prefab que exige inspeção no Editor
- harness ausente sendo fechado por spec futura (nomear o blocker)

---

## Closeout impact

| Situação | Status máximo |
|----------|--------------|
| Testes ausentes sem justificativa | `PARTIAL` |
| Runtime/gameplay sem Play Mode ou human scenario | `BUILD_VALIDATED` |
| Evidência completa | `ACCEPTED` (se nível de validação exigido satisfeito) |

---

## Never claim sem evidência no repo

`unit tests passed` / `EditMode tests passed` / `PlayMode tests passed` / `regression covered` / `feature accepted`
