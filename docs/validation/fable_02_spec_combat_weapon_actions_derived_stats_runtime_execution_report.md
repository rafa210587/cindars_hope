# Execution Report — fable_02_spec_combat_weapon_actions_derived_stats_runtime

> **Spec:** `docs/specs/a_implementar/fable/fable_02_spec_combat_weapon_actions_derived_stats_runtime.md` (+ EMENDA 2026-06-12)
> **Data:** 2026-06-12
> **Status:** BUILD_VALIDATED
> **Executor:** Claude (FABLE master plan — Batch 2, spec 7/42)

validated_adrs: []
validated_game_rules: [combat_rules.md]

---

## Acceptance criteria extracted

| CA | Critério | Evidência |
|----|----------|-----------|
| CA-1 | Tap/hold/hold-longo = light/heavy/charged com multiplicadores CANÔNICOS; E continua interagindo | `AttackChargeTracker`+`AttackChargeRules` (×1.00/×1.45/×1.65/×1.90 dano; ×1.00/×1.60/×1.80/×2.20 posture); KeyDown E com candidato NÃO inicia carga (regressão protegida); testes `ResolveWeight_CanonicalThresholds`, `Multipliers_AreCanonical` |
| CA-2 | Atributos no dano real e cooldown | `PlayerCombatStatsProvider` consome DerivedStatsCalculator (órfão WAVE 05) com cache invalidado por EquipmentSlotChangedEvent; testes `Provider_AttackBonus_*`, `Provider_AttackSpeed_*` com stats sintéticos |
| CA-3 | Posture/stagger | `EnemyPostureState` (tabela por dificuldade; quebra → `EnemyBrain.ApplyStun(1.2s)` + janela CoreExposed ×1.3 + `EnemyPostureBrokenEvent` + log `CombatLog: EnemyPostureBroken`); 2 charged quebram Normal, 3 lights não (teste aritmético) |

## Existing systems audit

```text
REUSADOS: PlayerAttackController (Q/E/modal guard/unarmed preservados — input vira
hold/release), DamageCalculator (caminho único — dano final entra ANTES via request),
DerivedStatsCalculator (órfão — agora consumido), CooldownHelper, EnemyVulnerabilityState
(OpenWindow = CoreExposed), KnockbackController, Bow/Spell services (ganham StatsProvider),
SkillTreeManager.GetAllActivePassiveModifiers, PlayerProgressionManager.Strength (base de Attack),
EnemyBrainState.Stunned (existia sem entrada/saída — ApplyStun + exit no Update fecham o ciclo).
CRIADOS: AttackChargeTracker/Rules, PlayerCombatStatsProvider, EnemyPostureState,
CombatPostureEvents, telegraph de carga (tint no sprite).
INTEGRAÇÕES: materializer AddComponent EnemyPostureState.Configure(dificuldade);
MeleeStrikeSkillEffectExecutor ganha postureDamageMultiplier (quebra-guarda 3×, knockback
bônus removido conforme spec); F01 consumido: PlayerStatusReceiver.IsActionBlocked bloqueia ataque.
```

## Spec Compliance Matrix

| Requisito | Implementação | Status |
|---|---|---|
| Thresholds 0.25/0.9 (+1.5 p/ charged longo da emenda) | `AttackChargeRules` (4 pesos: emenda distingue charged curto/longo) | OK |
| Multiplicadores canônicos (emenda substitui 1.6/2.4) | ×1.45/×1.65/×1.90 dano; ×1.60/×1.80/×2.20 posture | OK |
| Stamina por arma (emenda) | INTERIM: weapon.StaminaCost × razões canônicas (×1.6/×1.9/×2.2 ≈ matriz sword 25/40/48); F03 introduz campos POR ARMA — documentado | OK (interim) |
| Crítico canônico (emenda) | chance 5% base ×1.5; janela de vulnerabilidade aberta GARANTE crit (CoreExposed = janela ×1.3 + crit) | OK |
| Provider com cache/invalidação | EquipmentSlotChangedEvent; respec invalida via recompute lazy (evento de skill formal entra com F29) | OK |
| Telegraph de carga | tint do sprite do player por peso | OK |
| Posture no materializer + Stunned no brain | Configure por baseDifficulty + ApplyStun/exit | OK |
| Quebra-guarda 3× | postureDamageMultiplier: 3f na registration | OK |
| Bow/spell com dano derivado | services usam StatsProvider.FinalDamage (peso Light) | OK |
| Eventos + unsubscribe | 2 novos; provider IDisposable (Dispose no OnDestroy) | OK |

Limitação documentada: contribuição de EQUIPMENT no Attack derivado depende de um registry
de EquipmentDataSO que não existe (descoberta Fase 0: zero callers e zero registry) — o
provider já aceita o dicionário (testado sinteticamente); F03 cria o caminho real de dados.

## Validation

```text
Validation method: run_strict_validation.ps1
Exit code: 0
Docs validation: PASS | Assembly-CSharp: PASS (0E) | Assembly-CSharp-Editor: PASS (0E)
Quality check: PASS | Diff completeness: PASS (WARNs legados)
Result artifact: docs/validation/LAST_STRICT_VALIDATION_RESULT.json
```

## Testing Quality Gate

```text
Changed runtime code: YES
Changed deterministic logic: YES (núcleo de combate)
Changed Unity scene/prefab/asset wiring: NO
Automated tests added/updated: YES (10 EditMode tests em AttackChargeAndStatsTests.cs)
Automated tests command: Unity Test Runner EditMode (compilados; execução no lote final)
Manual Play Mode scenario: 3 pesos visíveis + stagger + E interagindo — DEFERRED_TO_FINAL_VALIDATION
Justification if no automated tests: N/A
Residual risk: feel do hold/release e telegraph são validação humana; ataque agora dispara
no KeyUp (mudança de timing perceptível — intencional, canon); base Attack=Força é fórmula
interim até F18/F03 fixarem a canônica.
```

## Honest status rationale

BUILD_VALIDATED: lógica pura coberta; o LOOP real de input/hold e o stagger em cena são
exatamente o que o Play Mode final valida. Sem claims além de compile+testes compilados.

## Remaining work

- F03: custos de stamina POR ARMA + ASPD por arma em cima do provider.
- F27: perfect block consome a postura criada aqui (reflexo de posture).
- F05: bosses usam ApplyStun/posture em fases.
- HUD de posture/carga: F14.
