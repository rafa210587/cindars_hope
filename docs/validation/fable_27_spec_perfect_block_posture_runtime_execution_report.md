# Execution Report — fable_27_spec_perfect_block_posture_runtime

> **Spec:** `.specs/a_implementar/fable/fable_27_spec_perfect_block_posture_runtime.md`
> **Data:** 2026-06-12
> **Status:** BUILD_VALIDATED
> **Executor:** Claude (FABLE master plan — Batch 2, spec 10/42)

validated_adrs: []
validated_game_rules: [combat_rules.md]

---

## Acceptance criteria extracted

| CA | Critério | Evidência |
|----|----------|-----------|
| CA-1 | Hit aos 0.10s = perfeito (0 dano, postura refletida); aos 0.30s = normal | `BlockTimingRules.IsPerfect` (janela 0.15s do INPUT) + interceptação no `PlayerDamageReceiver.ApplyDamage`; testes `IsPerfect_WithinWindow` |
| CA-2 | Re-block em 0.3s não rearma (anti-spam) | `CanArmPerfectWindow` (cooldown 0.4s da liberação) + 1 perfect por block (janela consumida); teste `RearmCooldown_BlocksQuickReblock` |
| CA-3 | Quebra de postura → stagger + CoreExposed crit ×1.3 | já entregue por F02 (EnemyPostureState) — constantes verificadas por teste `CoreExposed_CanonConstants` |
| CA-4 | Drenagem 18 STA/s e mitigação sem regressão | drenagem intocada; DESCOBERTA FASE 0: block NÃO tinha mitigação de dano (era só stance) — mitigação normal 50% implementada como parte do contrato canônico (floor 1, teste) |

## Existing systems audit

```text
DESCOBERTA: PlayerBlockController era apenas stance (slow + drain) — nenhuma interceptação
de dano existia. F27 implementa o contrato completo no caminho central criado por F03.
REUSADOS: PlayerBlockController (janela/anti-spam adicionados; drain/slow intactos),
PlayerDamageReceiver (branch de block ANTES de defesa/resistência), EnemyPostureState (F02 —
reflexo de postura + CoreExposed), EnemyBrain (GuardHold → ApplyStun no perfect; melee agora
passa o GameObject atacante pelo receiver — substitui o publish de PlayerHitEvent cujo único
consumidor aplicava dano cru sem atacante), EnemyContactDamage (passa atacante).
CRIADOS: BlockTimingRules (puro), PlayerPerfectBlockEvent (hook relíquia Kanthor — F23).
```

## Spec Compliance Matrix

| Requisito | Implementação | Status |
|---|---|---|
| Janela 0.15s medida do INPUT | `_blockStartTime = Time.time` no TryStartBlock (não do frame de animação — mitigação de risco da spec) | OK |
| Perfeito: dano 0, stamina do hit 0, reflexo 40% postura, SFX/flash distinto | dano 0 + reflexo via EnemyPostureState; feedback "Perfect block!" + log dedicado (flash visual = Play Mode/arte) | OK |
| Block normal: mitigação + drenagem 18 STA/s mantidas | mitigação 50% floor 1 (nova — não existia); drenagem intocada | OK |
| CoreExposed (stagger 1.2s + 1.5s, crit garantido ×1.3) | F02 entregou; testes de constantes + crit garantido já testado em F02 | OK |
| Anti-spam 0.4s | cooldown da janela + consumo por hit | OK |
| Guard break de inimigo escudado | GuardHold + perfect → ApplyStun(1.0s) | OK |
| Dodge único i-frame (canon) | nenhum i-frame novo criado | OK |
| Rollback (janela 0 = antigo) | constantes centralizadas em BlockTimingRules | OK |

Limitação documentada: perfect block NÃO reflete postura de PROJÉTEIS (sem atacante próximo) —
dano ainda é negado; comportamento canônico razoável, anotado.

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
Changed deterministic logic: YES (timing puro)
Changed Unity scene/prefab/asset wiring: NO
Automated tests added/updated: YES (6 EditMode tests em PerfectBlockTests.cs)
Automated tests command: Unity Test Runner EditMode (compilados; execução no lote final)
Manual Play Mode scenario: perfect block + quebra de postura (timing feel) — DEFERRED_TO_FINAL_VALIDATION
Justification if no automated tests: N/A
Residual risk: block agora MITIGA dano (mudança de comportamento intencional/canônica);
PlayerHitEvent permanece definido mas sem publisher (consumidor preservado p/ compat) —
remoção formal em limpeza futura.
```

## Honest status rationale

BUILD_VALIDATED; o feel do timing (0.15s) é estritamente validação humana de Play Mode.

## Remaining work

- F23: relíquia de Kanthor consome PlayerPerfectBlockEvent (cura 2%).
- F29: skills de bloqueio do catálogo estendem a janela/mitigação.
- Limpeza futura: aposentar PlayerHitEvent formalmente.
