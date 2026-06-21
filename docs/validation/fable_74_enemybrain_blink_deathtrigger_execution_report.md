# Execution Report — fable_74: EnemyBrain Blink & Death-Trigger Runtime

**Spec:** `fable_74_spec_enemy_brain_blink_deathtrigger_runtime`
**Status:** BUILD_VALIDATED_WITH_WARNINGS
**Date:** 2026-06-21

validated_adrs: [ADR-0005-deterministic-rng.md, ADR-0007-event-bus-gameplay-communication.md]
validated_game_rules: [combat_rules.md]

---

## Acceptance Criteria

| Critério | Evidência |
|----------|-----------|
| BlinkStrike enum value added to EnemyActionType | `EnemyActionSO.cs`: `BlinkStrike` adicionado |
| EnemyActionSO tem BlinkRange e IsDeathtrigger | Campos `public float BlinkRange` e `public bool IsDeathtrigger` adicionados |
| EnemyBrain executa blink-strike (teleporte + dano) | `EnemyBrain.ExecuteBlinkStrike()` — teleporta via `_rb.position`, aplica dano melee |
| Destino do blink é determinístico (sem UnityEngine.Random) | `EnemyBlinkExecutor.CalculateDestination()` usa `_blinkFlankSide` (set no spawn, alternante) |
| Death-trigger dispara exatamente uma vez na morte | `FireDeathTrigger()`: guarda `_deathtriggerFired`, loop pelos actions do set ativo |
| Death-trigger só dispara dentro da cave | `CaveRunManager.Instance == null → return` |
| CreateEnemyActionsAndSets atualizado (BlinkStrike + IsDeathtrigger) | 3 actions atualizadas (ember_tick_death_pop, mirror_adept_short_blink_strike, oathless_shade_shadow_step) |
| Validator SPEC 13D tem checks para BlinkStrike e IsDeathtrigger | Checks 12-14 adicionados ao `ValidateSpec13EnemyBrainRuntime.cs` |
| EditMode tests para EnemyBlinkExecutor | `EnemyBlinkExecutorTests.cs` — 6 testes |

## Arquivos criados/modificados

| Arquivo | Ação |
|---------|------|
| `Assets/_Game/Scripts/Combat/Data/EnemyActionSO.cs` | EDIT — `BlinkStrike` no enum; campos `BlinkRange`, `IsDeathtrigger` |
| `Assets/_Game/Scripts/Enemy/EnemyBlinkExecutor.cs` | NOVO — lógica de destino do blink (pura, determinística) |
| `Assets/_Game/Scripts/Enemy/EnemyBrain.cs` | EDIT — `using CindarsHope.Cave.Runtime`; `_deathtriggerFired`; `ExecuteBlinkStrike`; `FireDeathTrigger`; `ExecuteDeathTrigger`; `TakeDamage` chama `FireDeathTrigger` |
| `Assets/_Game/Scripts/Editor/EnemyTaxonomy/CreateEnemyActionsAndSets.cs` | EDIT — struct `ActionEntry` com `BlinkRange`/`IsDeathtrigger`; shorthand `BL`; `CreateAll` loop; 3 actions + 3 actionset notes |
| `Assets/_Game/Scripts/Editor/Validation/ValidateSpec13EnemyBrainRuntime.cs` | EDIT — checks 12-14 (BlinkStrike enum, SO fields, asset runtime validation) |
| `Assets/_Game/Tests/EditMode/Enemy/EnemyBlinkExecutorTests.cs` | NOVO — 6 testes EditMode |

## Actions atualizadas no gerador

| Action | Mudança |
|--------|---------|
| `action_ember_tick_death_pop` | `IsDeathtrigger=true`; display name simplificado; SPEC 13D note removida |
| `action_mirror_adept_short_blink_strike` | `ActionType=BlinkStrike`, `BlinkRange=1.0f`; SPEC 13D note removida |
| `action_oathless_shade_shadow_step` | `ActionType=BlinkStrike`, `BlinkRange=1.5f`; SPEC 13D note removida |

## Observação: assets existentes não re-gerados

O gerador `CreateEnemyActionsAndSets` é **idempotent** (skip se asset já existir). Assets existentes
no repo (`.asset` em `Data/Enemies/Actions/`) mantêm os valores antigos até regeneração no Editor Unity.
Isso é débito intencional: a regeneração requer Unity Editor aberto. Os novos campos `BlinkRange` e
`IsDeathtrigger` foram adicionados ao SO com defaults (0f, false), portanto os assets antigos
continuam funcionais (BlinkStrike cairá no fallback de range via `action.Range`).

Para aplicar os novos valores: abrir Unity Editor → CindarsHope > SPEC 13 > Create Enemy Actions and Sets
(deletar os 3 assets afetados antes se precisar sobrescrever).

## Testing Quality Gate

```text
Testing Quality Gate
────────────────────
Changed runtime code:           YES (EnemyBrain, EnemyBlinkExecutor)
Changed deterministic logic:    YES (blink destination calculation)
Changed Unity scene/prefab:     NO
Automated tests added/updated:  YES — EnemyBlinkExecutorTests.cs (6 testes EditMode)
Automated tests command:        dotnet build Assembly-CSharp.csproj (EditMode)
Manual Play Mode scenario:      1) Mirror Adept usa BlinkStrike: teleporta perto do player, aplica dano arcane
                                 2) Ember Tick morre perto do player: pop de fogo (AoE) dispara exatamente 1x
                                 3) Ember Tick morre fora da cave: pop NÃO dispara
Justification if no tests:      N/A
Residual risk:                  Assets .asset existentes mantêm ActionType=MeleeAttack/CastProjectile antigos
                                até regeneração no Unity Editor; PlayMode não executado nesta sessão
```

## Validation

```text
Validation method: dotnet build + validate_docs
Assembly-CSharp:        EXIT 0 — 0E, 1W (pre-existing: CombatTelemetrySession._blocks)
Assembly-CSharp-Editor: EXIT 0 — 0E, 3W (2 pre-existing struct CS0649; 1 pre-existing UNT0006)
validate_docs:          EXIT 0 — PASS
```

## Honest status rationale

`BUILD_VALIDATED_WITH_WARNINGS` porque:
- BlinkStrike runtime implementado no EnemyBrain ✅
- Death-trigger implementado com guard de cave + idempotência ✅
- EnemyBlinkExecutor determinístico (sem UnityEngine.Random) ✅
- Gerador e validator atualizados ✅
- EditMode tests para a lógica determinística ✅
- Assets `.asset` existentes não re-gerados (débito documentado) ⚠
- PlayMode scenario não executado nesta sessão ⚠

## Remaining work

- Regenerar os 3 assets afetados no Unity Editor (deletar + re-executar gerador)
- PlayMode validation: Mirror Adept blink; Ember Tick death pop dentro/fora de cave
