# Execution Report — fable_01_spec_status_effects_canonical_set_runtime

> **Spec:** `.specs/a_implementar/fable/fable_01_spec_status_effects_canonical_set_runtime.md` (+ EMENDA 2026-06-12)
> **Data:** 2026-06-12
> **Status:** BUILD_VALIDATED
> **Executor:** Claude (FABLE master plan — Batch 2, spec 6/42)

validated_adrs: []
validated_game_rules: [combat_rules.md]

---

## Acceptance criteria extracted

| CA | Critério | Evidência |
|----|----------|-----------|
| CA-1 | Enum com conjunto canônico + semântica distinta e testável por tipo | `StatusEffectType` += 9 tipos (emenda: 13 total — Slow/HeatStress/ColdStress incluídos); `StatusEffectSemantics` (puro); testes `Semantics_*`, `Enum_ContainsAllCanonicalTypes` |
| CA-2 | IDs estáveis + gerador + validator | `GenerateCanonicalStatusEffects` (13 assets idempotentes via AssetDatabase/SerializedObject) + `ValidateStatusEffectDatabase` (IDs únicos/completos, menu + API batch) |
| CA-3 | Aplicação real (skills + ações inimigas) | 4 skills wired (bleeding_arrow→status_bleed, ice_bind→status_chill, toxic_cloud→status_poison, rajada_gelida→status_chill); `EnemyBrain.ResolveAction` agora aplica `StatusApplicationIds` no player via `PlayerStatusReceiver.TryApplyFromEnemyAction` (respeita StatusApplyChance); IDs `*_minor` órfãos nas actions normalizados para canônicos |
| CA-4 | Não regressão dos 4 existentes | teste de caracterização `Characterization_ApplyTickExpire_PreservedForLegacyStatuses` + `Stacking_..._LegacyPolicyCharacterized`; ticker generaliza o DoT (burn hardcoded → semântica por tipo) preservando 1 tick/s e DurationTurns |

## Existing systems audit

```text
REUSADOS: StatusEffectSO/enum (estendido aditivamente, defaults neutros), pure
StatusEffectManager (enemy-side — intocado), Player.StatusEffectManager (id+duração —
intocado; semântica aplicada por receiver), EnemyStatusRuntimeTicker (reescrito DENTRO do
mesmo componente: generaliza o caso especial de burn para todos os tipos via database),
EnemyHealth.ApplyStatusEffect, ProjectileSkillEffectExecutor.statusEffectId (slice),
EnemyActionSO.StatusApplicationIds/StatusApplyChance (dados já autorados — agora vivos),
PlayerController.SpeedMultiplier, GameBootstrap.StatusEffectDatabase.
CRIADOS: StatusEffectSemantics (puro), PlayerStatusReceiver (+bootstrap),
StatusEffectAppliedEvent, EnemyBrain.ApplyExternalBehaviorOverride (1 método),
gerador + validator editor.
DESCOBERTA: actions inimigas referenciavam IDs status_*_minor INEXISTENTES no database
(falha silenciosa) — normalizados para os canônicos no gerador de actions.
```

## Spec Compliance Matrix

| Requisito | Implementação | Status |
|---|---|---|
| 13 tipos (emenda) com semântica | enum + semantics + ticker/receiver; Hunger/Fatigue FORA do ticker (HungerManager/F16 — documentado) | OK |
| Ticker enemy-side: Chill/Slow (speed), Root (0), Fear (Retreat), ConfusionLite (inversão), Corruption/HeatStress/ColdStress (DoT) | `EnemyStatusRuntimeTicker.Tick` genérico + `EnemyBrain.ApplyExternalBehaviorOverride` | OK |
| Player-side: Chill/Slow/Root (speed), Stun (bloqueia ação), DurabilityStress | `PlayerStatusReceiver` — `IsActionBlocked` (F02 consome), `DurabilityWearMultiplier` (F03 consome) | OK |
| Fear sem interromper windup/recover | EvaluateState guard pré-existente retorna antes do check de Fear | OK |
| Gerador + validator | `Editor/Combat/{GenerateCanonicalStatusEffects,ValidateStatusEffectDatabase}.cs` | OK |
| 4 skills + ≥2 enemy actions | 4 skills wired; actions toxic_cloud/bleeding_arrow/emerge_grab/trip_snare com IDs canônicos + aplicação real no hit melee | OK |
| StatusEffectAppliedEvent | publicado por ticker (1ª detecção) e receiver | OK |
| Sem save schema | status transientes (documentado) | OK |

Adaptações documentadas: spec pedia fear em `grito_desafio` — MeleeStrikeSkillEffectExecutor
não tem suporte a statusEffectId e estende-lo está fora dos arquivos permitidos; a 4ª skill
wired foi `rajada_gelida` (chill). Fear é aplicado por inimigos e disponível p/ skills
quando F29 mapear as ativas canônicas. Projéteis inimigos (Ranged/Cast) aplicam dano via
EnemyProjectileBehaviour — aplicação de status em projétil inimigo anotada p/ F04 (o hit
melee/área cobre o CA-3).

## Validation

```text
Validation method: run_strict_validation.ps1
Exit code: 0
Docs validation: PASS | Assembly-CSharp: PASS (0E) | Assembly-CSharp-Editor: PASS (0E)
Quality check: PASS | Diff completeness: PASS (WARNs legados)
Result artifact: docs/validation/LAST_STRICT_VALIDATION_RESULT.json
Asset generation: NOT RUN (Unity Editor indisponível nesta fase) — gerador/validator criados;
rodar CindarsHope/Combat/Generate Canonical Status Effects + Validate no Editor.
Residual: assets dos 9 status novos não existem até o gerador rodar (skills/actions fazem
fallback silencioso TryGetById=false — sem crash).
```

## Testing Quality Gate

```text
Changed runtime code: YES
Changed deterministic logic: YES
Changed Unity scene/prefab/asset wiring: geradores editor apenas (não executados — ver acima)
Automated tests added/updated: YES (8 EditMode tests em StatusEffectCanonicalTests.cs)
Automated tests command: Unity Test Runner EditMode (compilados; execução no lote final)
Manual Play Mode scenario: status visíveis em combate (lote) — DEFERRED_TO_FINAL_VALIDATION
Justification if no automated tests: N/A
Residual risk: geração de assets pendente de Unity; semântica de velocidade no player
composta com block/dash/fadiga (floor 0.5) — confirmar feel no Play Mode.
```

## Honest status rationale

BUILD_VALIDATED com ressalva explícita: o GERADOR não foi executado (regra
generated-asset-evidence — geração registrada como NOT RUN, não como feita). Código,
semântica e wiring compilam 0E com testes; os assets nascem na primeira execução do menu.

## Remaining work

- Rodar gerador + validator no Unity (lote de regeneração junto com FarmScene).
- F04: status em projéteis inimigos; F06: vulnerabilidades referenciam os IDs status_*.
- F02 consome IsActionBlocked; F03 consome DurabilityWearMultiplier.
