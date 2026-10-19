# Execution Report — fable_47_spec_derived_stats_followups_closeout

> **Spec:** `.specs/a_implementar/fable/fable_47_spec_derived_stats_followups_closeout.md`
> **Data:** 2026-06-19
> **Status:** BUILD_VALIDATED_WITH_WARNINGS
> **Executor:** Claude (spec-implementer) — FABLE Batch 9 (fecha os 3 follow-ups da F18)

validated_adrs: []
validated_game_rules: [combat_rules.md]

---

## Acceptance criteria extracted

| CA | Critério | Evidência |
|----|----------|-----------|
| CA-1 | Composer sem regressão: caracterização passa; cenários de corrida (block+exhausted, status expirando durante dash) ficam CORRETOS no composer; diferença documentada | `SpeedComposerTests` + `SpeedCharacterizationTests` (produto, clear independente, pares simultâneos, floor F01). Diferença documentada na seção "Correção de corrida" abaixo. |
| CA-2 | MoveSpeed derivado vivo: equipar item com MoveSpeed altera velocidade via fator `DerivedMoveSpeed`; desequipar restaura; re-aplica no mesmo evento F18 | `PlayerVitalsApplier.ReapplyDerivedMoveSpeed()` chamado em `Reapply()` (e em `OnInvalidatingEvent`=`EquipmentSlotChangedEvent`) e no `PlayerController.OnEnable`; fórmula testada em `DerivedFollowupsTests.DerivedMoveSpeedFactor_*`. Cenário humano: ver Testing Quality Gate. |
| CA-3 | Craft/Repair mensuráveis: `CraftTimeReduction` reduz duração efetiva; `RepairEfficiencyBonus` aumenta durabilidade restaurada | `CraftingRuntime.TryStartCraft` aplica `CraftTimeMultiplier` no ponto único de criação do job; `EquipmentManager.RepairItem` aplica `EffectiveRepairAmount` no ponto único de reparo; testes `DerivedFollowupsTests.CraftTimeMultiplier_*`, `EffectiveCraftSeconds_*`, `EffectiveRepairAmount_*`. |
| CA-4 | Duração por resistência: Poison resist Toxic 10 → ×0.8; 25 → ×0.5; 50 → ×0.5 (cap); Chill no eixo Ice; clamp 1–30s preservado | `PlayerStatusReceiver.TryApplyFromEnemyAction` aplica `ApplyStatusDurationReduction`; eixo via `ResistanceAxisFor` (Chill→Ice); testes `DerivedFollowupsTests.StatusDurationMultiplier_*`, `ApplyStatusDurationReduction_PreservesClamp1To30`, `ResistanceAxis_*`. |

## Existing systems audit

Fase 0 (system-reuse) — auditoria exaustiva ANTES de qualquer mudança:

```text
REUSADOS (não recriados):
- PlayerController.SpeedMultiplier: setter público mantido por compat (vira fator Legacy + warning DEV);
  consumido no FixedUpdate junto com GetHungerMoveSpeedModifier (FOME PRESERVADA fora do composer).
- DerivedStatsCalculator (F18): fonte única; MoveSpeed/CraftTimeReduction/RepairEfficiencyBonus já
  calculados a partir de passivas — NENHUMA fórmula alterada. baseMoveSpeed continua 0f no applier.
- PlayerVitalsApplier (F18): recalcula em EquipmentSlotChangedEvent; ganhou push do fator derivado +
  fontes estáticas CraftTimeReductionSource/RepairEfficiencyBonusSource (mesmo padrão de ResistanceSource).
- PlayerDamageReceiver.ResistanceSource (F18): REUTILIZADO como mapeamento DamageType→resistência (não
  dupliquei o mapa) para a duração por resistência.
- StatusEffectSemantics.GetDamageType / GetMoveSpeedFactor (F01): reutilizados.
- CraftingStation/CraftingJob (ponto único de duração) e EquipmentManager.RepairItem (ponto único de reparo).
- PlayerStatusReceiver clamp 1–30s (F01) e MinSpeedFloor 0.5 (F01) preservados.

CALL SITES de SpeedMultiplier (Fase 0 — escritores DIRETOS confirmados, 5 sites; NENHUM 6º encontrado):
1. PlayerMovementDisplacementResolver (~50/53/73)  — snapshot→0f→restore.
2. PlayerMovementAbilityController/Dodge (~80/122-123/149) — snapshot→0f→restore.
3. PlayerBlockController (~130/132/162) — snapshot→×0.45→restore.
4. PlayerConditionService/Exhausted (~299/303) — ×0.85 / ÷0.85 IN-PLACE com floor 0.5 (o pior:
   sem snapshot, corrompe permanentemente quando composto com block/status — a corrida descrita pela spec).
5. PlayerStatusReceiver (~125-136) — reverte fator anterior / aplica novo com floor.
   PlayerDashController NÃO escreve direto (delega ao DisplacementResolver via TryDisplace).
   EnemyBrain.SpeedMultiplier = propriedade PRÓPRIA de inimigo — NÃO TOCADA.

CRIADOS (novos, mínimos):
- PlayerSpeedComposer (puro, sem Unity) + SpeedFactorKind (enum).
- DerivedFollowupFormulas (puro): 3 fórmulas + fator derivado, constantes nomeadas.
- PlayerController.ActiveInstance (auto-registro em OnEnable/OnDisable — evita FindObjectOfType).
```

## Spec Compliance Matrix

| Requisito (spec) | Implementação | Status |
|---|---|---|
| `PlayerSpeedComposer` com fatores nomeados (SetFactor/ClearFactor/Value, produto clampado, floor de status) | `PlayerSpeedComposer.cs` + `SpeedFactorKind.cs` | OK |
| `SpeedMultiplier` vira leitura do composer; setter compat → fator Legacy + warning DEV | `PlayerController.SpeedMultiplier` get=composer.Value; set→Legacy | OK |
| Migrar os 5 escritores para SetFactor/ClearFactor (sem snapshot/restore) | Displacement, Dodge, Block, Exhausted, Status migrados | OK |
| Fome FORA do composer (caminho próprio preservado) | `GetHungerMoveSpeedModifier()` intacto no FixedUpdate | OK |
| Fator `DerivedMoveSpeed` aplicado pelo applier F18 na invalidação por evento | `PlayerVitalsApplier.ReapplyDerivedMoveSpeed()` em `Reapply()` | OK |
| `CraftTimeReduction` reduz duração efetiva no ponto único do job | `CraftingRuntime.TryStartCraft`→`CraftingStation`→`CraftingJob(.. craftTimeMultiplier)` | OK |
| `RepairEfficiencyBonus` aumenta durabilidade no ponto único de reparo | `EquipmentManager.RepairItem` → `EffectiveRepairAmount` | OK |
| Duração de status = base × (1 − min(0.5, resist × 0.02)); eixo correto; clamp 1–30s | `PlayerStatusReceiver.TryApplyFromEnemyAction` + `ApplyStatusDurationReduction` + `ResistanceAxisFor` | OK |
| Não mudar fórmulas/valores do DerivedStatsCalculator (F18) | DerivedStatsCalculator NÃO editado | OK |
| EnemyBrain.SpeedMultiplier intocado | Não editado | OK |
| EditMode tests: composer, caracterização, fator derivado, craft/repair, duração | 3 arquivos novos (~40 testes) | OK |

## Validation

```text
Validation method: dotnet build (por-spec) + validate_docs.ps1 + check_spec_diff_completeness.ps1 + run_strict_validation.ps1
Assembly-CSharp:        PASS — exit 0, 0 Erros, 0 Avisos
Assembly-CSharp-Editor: PASS — exit 0, 0 Erros, 3 Avisos PRE-EXISTENTES
   (CreateEnemyActionsAndSets.cs CS0649 x2, CSharpProjectPostprocessor.cs UNT0006 — fora do escopo fable_47)
Docs validation (validate_docs.ps1):        PASS — exit 0
Diff completeness (check_spec_diff_completeness.ps1): PASS — exit 0 (valida ESTE report + testes do diff)
Strict (run_strict_validation.ps1):         exit 1 = EXPECTED_FAIL_LEGACY_ONLY
   Causa: check_spec_quality varre relatórios LEGADOS (01_spec..08_spec) sem as seções novas.
   NÃO é falha desta spec: meus builds = 0E, meu report tem todas as seções, e os testes do meu
   diff passam no diff-completeness. As únicas falhas do strict são relatórios históricos pré-existentes.
```

EditMode tests: criados (não executados via Unity Test Runner — sandbox sem Unity; DEFERIDO à validação final humana). Compilam dentro de Assembly-CSharp (exit 0). Arquivos:
`Assets/_Game/Tests/EditMode/Player/SpeedComposerTests.cs`, `SpeedCharacterizationTests.cs`, `DerivedFollowupsTests.cs`.

### Correção de corrida (CA-1 — diferença documentada)

O padrão antigo (snapshot+restore num mutável compartilhado) tinha esta corrida real:
- **Block durante Exhausted:** `PlayerConditionService` faz `SpeedMultiplier *= 0.85` in-place; ao soltar o block,
  `PlayerBlockController` restaura seu `_previousSpeedMultiplier` (capturado JÁ com o 0.85 aplicado) — e o ÷0.85
  do exhausted depois deixa resíduo. No composer cada fator é independente: `Value = 0.85 × 0.45`; soltar o
  block deixa exatamente `0.85`. (Testes `RaceScenario_*`, `BlockReleasedWhileExhausted_KeepsOnlyExhausted`.)
- **Status expirando durante dash:** `PlayerStatusReceiver` revertia seu fator enquanto o dash tinha forçado 0,
  corrompendo o valor. No composer, limpar `Status` não toca `Dash` (=0); ao fim do dash a velocidade volta a 1.0
  limpa. (Teste `StatusExpiringDuringDash_DashStillStops`.)
- **Floor de status (F01):** antes o floor era `0.5 × factor` espalhado no mutável; agora o fator `Status` é
  pisado em `MinSpeedFloor=0.5` quando não-letal (Root/Stun continuam zerando). Reprodução canônica e documentada.

## Testing Quality Gate

```text
Changed runtime code: YES
Changed deterministic logic: YES (composição de velocidade + 3 fórmulas: craft, repair, duração de status)
Changed Unity scene/prefab/asset wiring: NO
Automated tests added/updated: YES
Automated tests command: Unity Test Runner EditMode (compilados em Assembly-CSharp exit 0; execução DEFERIDA — sandbox sem Unity)
Manual Play Mode scenario: DEFERRED_TO_FINAL_HUMAN_VALIDATION
  (feel de movimento: block durante fadiga, dash com slow ativo, craft acelerado, Poison/Chill encurtados)
Justification if no automated tests: N/A (tests adicionados)
Residual risk:
  - "Feel" de movimento não verificado em Play Mode (DEFERIDO ao dono). Mitigação: caracterização cobre a
    matriz de cenários como teste de regressão puro.
  - O fator DerivedMoveSpeed é mapeado de bônus aditivo (F18, base 0) para multiplicador via base do
    PlayerController (1 + bonus/base). Valores finais de feel pendentes de playtest (balance fino é trabalho futuro).
  - EditMode não executado nesta sessão (apenas compilado). Execução no Test Runner deferida ao gate final humano.
```

## Honest status rationale

BUILD_VALIDATED_WITH_WARNINGS: núcleo completo e auditado (composer + 5 migrações + 3 follow-ups), builds
0E (runtime) / 0E+3W-legados (editor), docs PASS, diff-completeness PASS. Os 3 follow-ups da F18 estão FECHADOS
(ver abaixo). PlayMode/feel de movimento DEFERIDO à validação final do dono (autorizado). NÃO reivindico
ACCEPTED nem PLAYMODE_VALIDATED. Strict exit 1 é exclusivamente dívida de relatórios legados (EXPECTED_FAIL_LEGACY_ONLY),
não introduzida por esta spec.

### Follow-ups da F18 — FECHADOS

Referência: `docs/validation/fable_18_spec_derived_stats_vitals_application_runtime_execution_report.md` (§FOLLOW-UPs).

1. **MoveSpeed derivado no PlayerController** → FECHADO via `PlayerSpeedComposer` + fator `DerivedMoveSpeed`
   empurrado pelo applier F18 (sem refactor de composição arriscado — o composer É a composição segura).
2. **CraftTimeReduction / RepairEfficiencyBonus** → FECHADO: efeito mensurável no ponto único do job de craft
   (`CraftingRuntime`/`CraftingStation`/`CraftingJob`) e no ponto único de reparo (`EquipmentManager.RepairItem`).
3. **Duração de status por resistência** → FECHADO: `duração × (1 − min(0.5, resist × 0.02))` no
   `PlayerStatusReceiver`, eixo correto via fonte única F18, clamp 1–30s preservado.

## Remaining work

- Execução do Unity Test Runner EditMode (~40 testes novos) — no gate final humano.
- Play Mode (feel de movimento + craft real + Poison/Chill encurtados) — DEFERIDO ao dono.
- Balance fino de craft/repair/duração e do mapeamento do fator derivado — playtest futuro (declarado pela spec).

## Files changed

```text
Runtime (criados):
  Assets/_Game/Scripts/Player/Movement/PlayerSpeedComposer.cs
  Assets/_Game/Scripts/Player/Movement/SpeedFactorKind.cs
  Assets/_Game/Scripts/Player/DerivedFollowupFormulas.cs
Runtime (editados):
  Assets/_Game/Scripts/Player/PlayerController.cs                       (composer + ActiveInstance + BaseMoveSpeed)
  Assets/_Game/Scripts/Player/Movement/PlayerMovementDisplacementResolver.cs (fator Displacement)
  Assets/_Game/Scripts/Player/Movement/PlayerMovementAbilityController.cs    (fator Dash/Dodge)
  Assets/_Game/Scripts/Player/Movement/PlayerBlockController.cs              (fator Block)
  Assets/_Game/Scripts/Player/Conditions/PlayerConditionService.cs          (fator Exhausted — corrida corrigida)
  Assets/_Game/Scripts/Combat/StatusEffect/PlayerStatusReceiver.cs          (fator Status + duração por resistência)
  Assets/_Game/Scripts/Player/PlayerVitalsApplier.cs                        (push DerivedMoveSpeed + fontes craft/repair)
  Assets/_Game/Scripts/Craft/CraftingRuntime.cs                            (resolve craftTimeMultiplier)
  Assets/_Game/Scripts/Craft/CraftingStation.cs                            (param craftTimeMultiplier)
  Assets/_Game/Scripts/Craft/CraftingJob.cs                                (overload com craftTimeMultiplier)
  Assets/_Game/Scripts/Equipment/EquipmentManager.cs                       (RepairEfficiencyBonus no reparo)
Tests (criados):
  Assets/_Game/Tests/EditMode/Player/SpeedComposerTests.cs
  Assets/_Game/Tests/EditMode/Player/SpeedCharacterizationTests.cs
  Assets/_Game/Tests/EditMode/Player/DerivedFollowupsTests.cs
Build:
  Assembly-CSharp.csproj  (5 includes novos: 3 runtime + 3 testes — total 6 Compile entries)
Docs:
  docs/validation/fable_47_spec_derived_stats_followups_closeout_execution_report.md (este)
```
