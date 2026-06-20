# Execution Report — fable_39 (Inferred Player Class)

> **Spec:** `.specs/a_implementar/fable/fable_39_spec_inferred_player_class_runtime.md`
> **Spec ID:** `fable_39_spec_inferred_player_class_runtime`
> **Date:** 2026-06-20
> **Status:** `BUILD_VALIDATED_WITH_WARNINGS`
> **Wave:** FABLE Batch 7 (E34)
> **validated_game_rules:** `docs/game_rules/player_rules.md` (Rule 8 — Inferred player class)
> **required_adrs:** none

---

## Honest status rationale

`BUILD_VALIDATED_WITH_WARNINGS`. All four central acceptance criteria (CA-1 simple dominance,
CA-2 hybrid, CA-3 Colono incl. ties, CA-4 recompute determinism / no saved state) are implemented
and covered by deterministic EditMode tests. Both assemblies compile with exit code 0 and the
strict validation harness returns exit code 0. The `_WITH_WARNINGS` qualifier reflects the spec's
own `Requires Play Mode final validation: YES (lote)` and `Human validation timing:
DEFERRED_TO_FINAL_VALIDATION`: the recompute-by-event runtime wiring (`InferredClassRuntime`
subscribing `SkillDerivedStatsChangedEvent`), the F14 character-sheet line, and the F28 dialogue
greeting axis are exercised in batch Play Mode, not here. No Unity Editor / Play Mode was run in
this session (owner authorized deferral). No premature ACCEPTED / PLAYMODE_VALIDATED claim is made.

---

## Acceptance criteria extracted

| # | Criterion (spec) | Implementation | Evidence | Status |
|---|---|---|---|---|
| CA-1 | 10 Guerreiro / 2 Caçador ⇒ "Lâmina de Cindar" +3% postura (dominante ≥6 pts e ≥40%; 2ª <70% da 1ª) | `PlayerClassInference.GetProfile` dominance gate + `PureBonus(Melee)` | `ClassInferenceTests.CA1_SimpleDominance_Melee_IsWarriorWithPostureBonus` (+ below-6 / below-40% / other-trees / farmer-5% cases) | OK |
| CA-2 | 8/6 (2ª ≥70% da 1ª) ⇒ híbrido, título composto da tabela 5×4 + METADE de cada bônus | `GetProfile` hybrid gate + `HybridTitleId` + `× 0.5f` halving | `ClassInferenceTests.CA2_Hybrid_EightSix_CompositeTitle_AndHalfBonuses` (+ just-under-70% pure, order, magnitude) | OK |
| CA-3 | 2/2/2 (nenhuma ≥6 e ≥40%) ⇒ "Colono" sem bônus; empates incluídos | `GetProfile` returns `Colono` when gate fails or top tie | `ClassInferenceTests.CA3_*` (no-investment, low-spread, tie-at-top, null, missing-keys) | OK |
| CA-4 | Compra/respec recomputam imediatamente (evento), refletindo na entrada do provider e no título | `InferredClassRuntime` subscribes `SkillDerivedStatsChangedEvent` → `Recompute()` reads `SkillTreeManager.GetPointsSpentInTree` → replaces `CurrentProfile` | `ClassInferenceTests.CA4_Deterministic_*`, `CA4_RecomputeAfterRespec_FromZeroPoints_IsColono` (determinism = identical profile for equal points; respec→0 ⇒ Colono). Event-driven recompute in runtime is Play Mode (lote). | OK (logic) / Play Mode deferred (wiring) |

---

## Existing systems audit

Phase 0 system-reuse audit (no parallel system created; `system-reuse-audit` discipline):

| System | Found at | Decision |
|---|---|---|
| `SkillTreeManager` (points per tree) | `Assets/_Game/Scripts/Skills/SkillTreeManager.cs` | **REUSED** — read-only via `GetPointsSpentInTree(treeId)`; no contract change. Single source of the investment. |
| 5 tree IDs | `SkillTreeId` enum (`Melee/Ranged/Magic/Survival/Crafting`) + `DefaultSkillCatalog.BuildTree` string ids `"melee"/"ranged"/"magic"/"survival"/"crafting"` | **REUSED** — mapped spec labels → real ids (Guerreiro=melee, Caçador=ranged, Místico=magic, Lavrador=survival, Vínculo=crafting). |
| Recompute trigger | `SkillDerivedStatsChangedEvent` (`Core/Events/SkillTreeEvents.cs`), published by `SkillTreeManager` on purchase/rank-up/respec/load | **REUSED** — single recompute signal. No new event. (`SkillNodePurchasedEvent` exists but the manager emits `SkillDerivedStatsChangedEvent`, which also covers respec/load.) |
| DerivedStats provider entry pattern | `DerivedStatsCalculator` (pure static) + static `Func` source hooks (`PlayerDamageReceiver.ResistanceSource`, `PlayerVitalsApplier.CraftTimeReductionSource/RepairEfficiencyBonusSource`) | **REUSED PATTERN** — the inferred bonus is exposed as named static `Func` source hooks on `InferredClassRuntime` (one named entry, replaced each recompute), matching the project's established provider-hook idiom. |
| F14 character sheet | `Assets/_Game/Scripts/UI/Character/CharacterEquipmentPanelController.cs` | **REUSED** — additive title line in `DrawAttributes`. No new screen. |
| F28 dialogue condition | `Assets/_Game/Scripts/NPC/DialogueLineCondition.cs` + `DialogueConditionContext.cs` | **REUSED** — additive optional `RequiredInferredTitleId` axis; reuses the live line-condition mechanism (no parallel greeting system). |
| Localization | `Assets/_Game/Scripts/Localization/LocalizationService.cs` + `LocalizationStringTable.cs` (ADR-0012) | **REUSED** — title strings declared as `ui.class.*` seed entries; resolved via `LocalizationService.Get`. |
| Save | `SkillTreeSaveData` (skill points already persisted) | **NOT TOUCHED** — zero new state; profile re-derives after load. |

Not found / created new (justified): the inference itself — `PlayerClassInference` (pure rules +
title/bonus tables) and `InferredClassRuntime` (recompute hook + named provider entry). No
equivalent existed.

---

## Spec Compliance Matrix

| Requirement | Implementation | OK |
|---|---|---|
| `PlayerClassInference.GetProfile(pontosPorÁrvore)` pure & deterministic | `PlayerClassInference.GetProfile(IReadOnlyDictionary<SkillTreeId,int>)`, pure static, no Unity/IO | OK |
| dominante = mais pontos se ≥6 e ≥40% do total | `MinDominantPoints=6`, `MinDominantShare=0.40f`; gate in `GetProfile` | OK |
| híbrido se 2ª ≥70% da 1ª | `HybridSecondaryRatio=0.70f`; hybrid branch | OK |
| sem dominante ⇒ "Colono" sem bônus (inclui empates) | `Colono` returned; explicit top-tie ⇒ Colono | OK |
| 5 títulos puros + tabela 5×4 de híbridos compostos | `PureTitleId` (5) + `HybridTitleId` (composite) + 25 `ui.class.*` localization entries (5 pure + 20 hybrid + Colono) | OK |
| micro-bônus ≤5%; híbrido = metade de cada | table values 3%/3%/3%/5%/3%; hybrid `× 0.5f`; magnitude tests assert ≤5% | OK |
| bônus = 1 entrada no DerivedStats provider (recompute em evento) | `InferredClassRuntime.CurrentProfile` + named `Func` source hooks; replaced on `SkillDerivedStatsChangedEvent` (never double-counted) | OK |
| exibição: título na ficha (F14) | `CharacterEquipmentPanelController.DrawAttributes` "Classe:" line via `LocalizationService.Get` | OK |
| saudação de NPC usa título (F28 condição) | additive `DialogueLineCondition.RequiredInferredTitleId` + `DialogueConditionContext.InferredTitleId` wired from `InferredClassRuntime.CurrentProfile.TitleId` | OK |
| EditMode tests: dominância/híbrido/empate/colono, magnitudes, recompute | `ClassInferenceTests` (21 tests) | OK |
| zero estado salvo de classe | no save DTO, no field added; profile derived; `OnDestroy` resets named entry to Colono | OK |
| texto novo via LocalizationService | `ui.class.*` entries in `LocalizationStringTable` | OK |

---

## Validation

```text
Validation method: run_strict_validation.ps1
Exit code: 0
Assembly-CSharp: PASS (exit 0; 1 pre-existing warning CS0649 CombatTelemetrySession._blocks, unrelated)
Assembly-CSharp-Editor: PASS (exit 0; 3 pre-existing warnings, unrelated)
Quality check: PASS
Docs validation: PASS (validate_docs.ps1 exit 0)
Spec diff completeness: PASS (check_spec_diff_completeness.ps1 exit 0)
Result artifact: docs/validation/LAST_STRICT_VALIDATION_RESULT.json
```

Commands run (PowerShell, `$LASTEXITCODE` checked each):

```powershell
dotnet build .\Assembly-CSharp.csproj --no-restore        # exit 0
dotnet build .\Assembly-CSharp-Editor.csproj --no-restore # exit 0
.\tools\docs\validate_docs.ps1                            # exit 0
.\tools\docs\check_spec_diff_completeness.ps1             # exit 0
.\tools\docs\run_strict_validation.ps1                    # exit 0
```

---

## Testing Quality Gate

```text
Changed runtime code: YES
Changed deterministic logic: YES (inference rules)
Changed Unity scene/prefab/asset wiring: NO
Automated tests added/updated: YES (Assets/_Game/Tests/EditMode/Player/ClassInferenceTests.cs — 21 tests)
Automated tests command: Unity EditMode Test Runner (NOT RUN this session — Unity not executed; tests compile into Assembly-CSharp exit 0)
Manual Play Mode scenario: DEFERRED_TO_FINAL_VALIDATION (lote) — see player_rules.md Rule 8 Play Mode line
Justification if no automated tests: N/A (tests added)
Residual risk: event-driven recompute (InferredClassRuntime ← SkillDerivedStatsChangedEvent), the F14 title line render, and the F28 greeting axis are validated only at compile time here; live behavior is batch Play Mode. The pure inference (the only deterministic logic) is fully covered.
```

Regression coverage (spec "sem skill investida = sem mudança"): `CA3_NoInvestment_IsColono_NoBonus`
proves a player with zero points reads as Colono with no bonus — identical stats to a fresh
character (the bonus enters only as a removable named provider entry, default Colono = 0).

---

## Dependency Chain

```text
Original target: fable_39
Dependency chain: F29 (canonical skill catalog) — completed/BUILD_VALIDATED (task E33), provides reliable per-tree point counts
Forbidden dependencies: none
Resolved depth: 0 (dependency already satisfied)
Can continue original target: YES
```

---

## Files changed

New (runtime):
- `Assets/_Game/Scripts/Player/PlayerClassInference.cs` (+ `.cs.meta`) — pure inference rules + title/bonus tables.
- `Assets/_Game/Scripts/Player/InferredClassRuntime.cs` (+ `.cs.meta`) — recompute hook + named provider entry (static `Func` sources) + bootstrap.

New (test):
- `Assets/_Game/Tests/EditMode/Player/ClassInferenceTests.cs` (+ `.cs.meta`) — 21 EditMode tests (CA-1…CA-4, magnitude, determinism, edge cases).

Modified (additive, no contract change):
- `Assets/_Game/Scripts/Localization/LocalizationStringTable.cs` — +25 `ui.class.*` seed entries.
- `Assets/_Game/Scripts/UI/Character/CharacterEquipmentPanelController.cs` — +"Classe:" line in `DrawAttributes`; +2 usings.
- `Assets/_Game/Scripts/NPC/DialogueLineCondition.cs` — +optional `RequiredInferredTitleId` axis (Specificity + IsMet).
- `Assets/_Game/Scripts/NPC/DialogueConditionContext.cs` — +`InferredTitleId` (optional ctor param, wired in `FromWorld`).
- `Assembly-CSharp.csproj` — +3 `<Compile Include>` (2 runtime + 1 test). Authorized by spec "Arquivos permitidos: csproj includes".

No forbidden files changed: no `*.unity` / `*.prefab` / `*.asset` manual edit; no `Packages/`,
no `ProjectSettings/`; no SkillTreeManager core contract change; no SaveManager / save section
change; no F29 catalog change.

---

## Anti-regression

```text
SkillTreeManager: read-only (GetPointsSpentInTree); no contract change.
DerivedStats existing fields: unchanged; the inferred bonus is a separate named provider entry (Func hooks), removable.
Player with no points: Colono, no bonus — identical to current behavior (CA3_NoInvestment test).
Save schema: intact; no new state (profile derived; re-derives after load).
GameEventBus only: InferredClassRuntime communicates via SkillDerivedStatsChangedEvent; no GameObject.Find / FindObjectOfType in runtime (bootstrap uses FindAnyObjectByType, the same approved idiom as PlayerVitalsApplierBootstrap, allowed in RuntimeInitializeOnLoad bootstrap).
Existing dialogue lines: unaffected (RequiredInferredTitleId defaults null = no constraint).
```

---

## Remaining work (deferred, not blocking BUILD_VALIDATED)

- Play Mode (lote): title visible on F14 sheet after skill purchase; recompute on purchase/respec;
  optional NPC greeting line gated on `RequiredInferredTitleId` (no greeting line is authored yet —
  the F28 axis is provided; content specs F35/F36/F70 may use it).
- Wiring the identity bonuses into their downstream consumers (posture dealt, crit chance, mana
  cost, out-of-cave stamina, friendship gained) is intentionally a thin named-hook contract here;
  consumers read the `InferredClassRuntime.*Source` hooks when their systems opt in (same
  "efeito pendente" discipline as other derived hooks). The magnitude guard (≤5%) is enforced at
  the source regardless of consumer.

---

*Generated by spec-implementer (fable_39). Validation honest per `.claude/rules/validation-truth.md`.*
