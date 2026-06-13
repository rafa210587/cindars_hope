# Execution Report — fable_18_spec_derived_stats_vitals_application_runtime

> **Spec:** `docs/specs/a_implementar/fable/fable_18_spec_derived_stats_vitals_application_runtime.md`
> **Data:** 2026-06-12
> **Status:** BUILD_VALIDATED
> **Executor:** Claude (FABLE master plan — Batch 2, spec 9/42)

validated_adrs: []
validated_game_rules: [player_rules.md]

---

## Acceptance criteria extracted

| CA | Critério | Evidência |
|----|----------|-----------|
| CA-1 | Max derivado preserva proporção; desequipar não mata (floor 1) | `PlayerVitalsApplier.PreserveRatio` + `SetMaxHP/SetMaxStamina/SetMaxManaPreservingRatio`; testes `PreserveRatio_*` |
| CA-2 | Resistências reduzem dano do tipo; HungerDrainReduction mensurável | `PlayerDamageReceiver.ResistanceSource` + `CalculateReducedDamage(raw, def, resist)`; `HungerManager.DrainMultiplier` (clamp 0.25, drain mínimo 1); testes `ResistanceFor_*`, `Receiver_AppliesResistance*`, `DrainMultiplier_Clamped` |
| CA-3 | Recálculo por evento (mesma invalidação F02) | applier re-aplica em `EquipmentSlotChangedEvent`; base capturada uma vez no Start |

## Existing systems audit

```text
REUSADOS: DerivedStatsCalculator (fonte única — applier consome igual ao provider F02),
PlayerManager (ganhou SetMaxHP mínimo), StaminaManager (SetMaxStamina + ExternalRegenBonus
mínimos — regen por fome intacta), ManaManager (SetMaxManaPreservingRatio + ExternalRegenBonus;
SetMaxMana legado intocado), HungerManager (DrainMultiplier no ponto único LoseHunger),
PlayerDamageReceiver (F03 — ganhou ResistanceSource e overload com DamageType),
BowArrowAttackService (BowRange/BowDamageBonus consumidos), EnemyProjectileBehaviour
(agora carrega o DamageType real até o receiver e o popup).
CRIADOS: PlayerVitalsApplier (+bootstrap no GameBootstrap GO).
```

## Spec Compliance Matrix

| Requisito | Implementação | Status |
|---|---|---|
| MaxHP/Stamina/Mana com proporção | setters mínimos + PreserveRatio puro testado | OK |
| Stamina/Mana regen derivados | ExternalRegenBonus somado às taxas existentes (fome continua mandando na stamina) | OK |
| Resistências → receiver + status | receiver OK (Toxic→Toxic, Cold→Ice, Heat→Fire); DURAÇÃO de status por resistência: FOLLOW-UP explícito (exigiria acoplar PlayerStatusReceiver↔applier; previsto pela spec como aceitável) | OK (parcial documentado) |
| HungerDrainReduction | multiplicador clampado no LoseHunger | OK |
| MoveSpeed derivado → PlayerController | FOLLOW-UP explícito: SpeedMultiplier é mutável compartilhado (block/dash/fadiga/status) — compor mais um fator permanente sem refactor de composição é risco de regressão; spec autoriza registrar follow-up | FOLLOW-UP |
| BowRange/BowDamageBonus | service consome provider.Current | OK |
| Craft/Repair hooks | FOLLOW-UP explícito (integração invasiva — autorizado pela spec) | FOLLOW-UP |
| Fonte única de stats | provider (combate) + applier (vitals) consomem o MESMO calculator; zero cálculo inline | OK |

## Validation

```text
Validation method: run_strict_validation.ps1
Exit code: 0
Docs validation: PASS | Assembly-CSharp: PASS (0E) | Assembly-CSharp-Editor: PASS (0E)
Quality check: PASS | Diff completeness: PASS (WARNs legados)
```

## Testing Quality Gate

```text
Changed runtime code: YES
Changed deterministic logic: YES
Changed Unity scene/prefab/asset wiring: NO
Automated tests added/updated: YES (6 EditMode tests em VitalsApplicationTests.cs)
Automated tests command: Unity Test Runner EditMode (compilados; execução no lote final)
Manual Play Mode scenario: equipar +HP e ver máximo subir — DEFERRED_TO_FINAL_VALIDATION
Justification if no automated tests: N/A
Residual risk: bases capturadas no primeiro Start — se outro sistema alterar máximos
diretamente depois, o applier sobrescreve no próximo evento (fonte única é intencional);
follow-ups listados (MoveSpeed/craft/repair/duração de status).
```

## Honest status rationale

BUILD_VALIDATED; 3 follow-ups explícitos e autorizados pela própria spec (não inflados como OK).

## Remaining work

- F29: passivas do catálogo canônico alimentam o mesmo applier.
- F32: registry de EquipmentDataSO liga bônus de itens reais.
- Follow-ups: MoveSpeed composto; craft/repair hooks; duração de status por resistência.
