---
doc_type: validation_report
spec: fable_29_spec_canonical_skill_catalog_migration
status: BUILD_VALIDATED_WITH_WARNINGS
date: 2026-06-20
validated_adrs: []
validated_game_rules:
  - player_rules.md
  - skill_tree_rules.md
---

# Execution Report — fable_29 Canonical Skill Catalog Migration

> **Spec:** `.specs/a_implementar/fable/fable_29_spec_canonical_skill_catalog_migration.md`
> (read in full, including EMENDA 2026-06-13-V3).
> **Contract consumed:** `docs/design/gameplay/combat/SKILL_NUMERIC_ADDENDUM_v1.0.md` (numbers,
> prerequisite chains, capstone triggers) + `docs/game_rules/skill_tree_rules.md` (reconciled economy).
> **Mode:** SOLO (skill tree + executors + skill save). Human/Play Mode validation DEFERRED by owner.
> **Honest status:** `BUILD_VALIDATED_WITH_WARNINGS` — core criteria met, builds 0E, EditMode tests
> authored; Play Mode/visual (F14 screen) deferred to final validation per spec.

---

## Status

| Phase | Status |
|-------|--------|
| Phase 0 (audit) | COMPLETE |
| Phase 1 (routes/aggregator/hooks) | COMPLETE |
| Phase 2 (tier gating / rank caps / capstones) | COMPLETE |
| Phase 3 (catalog tiers/routes/count) | COMPLETE |
| Phase 4 (actives mapped / dormant) | COMPLETE (reuse — executors already aligned to addendum) |
| Phase 5 (save migration + tests + report) | COMPLETE |
| Phase 2-3 (Unity batchmode / Play Mode) | NOT RUN — DEFERRED_TO_FINAL_VALIDATION (owner authorized) |

Overall: **BUILD_VALIDATED_WITH_WARNINGS**.

---

## Acceptance criteria extracted

| CA | Requirement | Evidence | Result |
|----|-------------|----------|--------|
| CA-1 / CA-COUNT | Catalog counted per tree, total = **69** | `CanonicalCatalogTests.Catalog_HasExactly69NodesAcross5Trees`; generator + validator count check; `DefaultSkillCatalog.CanonicalNodeCount = 69` | OK |
| CA-2 | Tier 2 locked at 4 pts, open at 5; 11/18/26 enforced | `TierGating_Tier2_RequiresFivePointsInTree`, `TierGating_HigherThresholds_11_18_26_AreEnforced` | OK |
| CA-2b | Unlocking Tier 2 raises a T1 node cap 2→3 | `DynamicRankCap_UnlockingTier2_RaisesT1NodeCapFrom2To3` | OK |
| CA-3 | Kanthor→Kaand blocked + inverse; Anya/Senya; confirmation required | `Capstone_ChoosingKanthor_BlocksKaand_AndRequiresConfirmation`, `Capstone_ChoosingSenya_BlocksAnya_InverseDirection`, `Catalog_MeleeAndMagicCapstones_ExposeExclusiveVariants` | OK |
| CA-4 | Passive (stamina) reduces real derived stat via aggregator→provider | `Aggregator_StaminaPassive_RaisesDerivedMaxStamina`, `Aggregator_RankScalesPassiveEffect` | OK |
| CA-5 | Old save migrates with refund total, zero exceptions, log per ID | `SaveMigration_RefundsUnknownNodeIds_NoPointLoss`, `SaveRoundTrip_PreservesRanksAndVariants`; `SkillTreeManager.MigrateUnknownNodes` logs each refund | OK |
| CA-6 | Respec re-locks tier → item non-equippable; re-unlock → equippable | `EquipGate_ItemBlockedAfterTierRelock_UnblockedAfterReunlock` | OK |
| CA-7 | Node with unmet prereq not purchasable; satisfied purchasable | `Prerequisites_BlockUntilSatisfied` | OK |
| CA-8 | Named hooks published; "efeito pendente" when consumer absent | `Aggregator_PublishesNamedHooks_WhenConsumerAbsent`, `Aggregator_NamedHook_DeliversToRegisteredConsumer` | OK |
| CA-MOD | 5 dead modifiers routed into provider (route per modifier; 2 read today, 3 reader-pending) | `DeadModifiers_AreConsumedByDerivedStats` (asserts provider fields populated); see "Dead modifier decisions" below | OK (with reader-pending disclosure) |

---

## Existing systems audit (Phase 0 — REUSE, no parallel systems)

| System | Found | Decision |
|--------|-------|----------|
| `SkillTreeManager` | yes | EXTENDED (tier gating, ranks, capstone confirm, punitive respec, migration) — not recreated |
| `SkillTreeState` | yes (boolean purchased) | EXTENDED to per-node ranks + per-tree points + variants (backward-compatible save) |
| `SkillPurchaseService` | yes | EXTENDED (tier thresholds, dynamic rank cap, prereqs, exclusivity, rank-up) |
| `SkillRespecService` | yes | REUSED (manager now recomputes equip gate after respec) |
| `SkillPassiveApplicator` | yes | SUPERSEDED by `SkillEffectAggregator` (rank-scaled); marked obsolete, zero references |
| `DefaultSkillCatalog` (69 nodes, WI-11) | yes | EXTENDED with Tier/route/variant data; **no node added/removed/renamed** |
| `ActiveSkillExecutionController` (keys 1-4, 21 executors) | yes | REUSED — base damage/cost/cooldown already match SKILL_NUMERIC_ADDENDUM §1 exactly |
| `ActiveSkillSlots` (legacy R/T/Y/G) | yes | RETIRED (behavioral: no Update() input, no executor call) — item 7 |
| `DerivedStatsCalculator` (F02 provider) | yes | EXTENDED to consume the 5 dead modifiers — publish-only, not rewritten |
| `EquipmentManager` | yes | READ-ONLY gate consult in `EquipItem` (item 6) — not rewritten |
| `CatalogExpectations` (fable_30, count=69, IDs enumerated) | yes | NOT TOUCHED — count and IDs preserved so fable_30 validator stays green |
| Skill save section (`SkillTreeSaveData` + SaveManager wiring) | yes | EXTENDED additively (NodeRanks, ChosenCapstoneVariants, Version=2); migration in manager load |

No second SkillTreeManager, executor, stats provider, or catalog/registry created (regras de não duplicação respeitadas).

---

## Spec Compliance Matrix

| Requirement (spec + emenda) | Implementation |
|------------------------------|----------------|
| SkillEffectRoute enum + typed payload | `Assets/_Game/Scripts/Skills/SkillEffectRoute.cs` (StatModifier, named hooks, ActiveSkill, EventHook) |
| SkillEffectAggregator → provider + hooks (single application point) | `Assets/_Game/Scripts/Skills/SkillEffectAggregator.cs` |
| Tier gating 0/5/11/18/26 by points-spent-in-tree | `SkillTierRules.cs` + `SkillPurchaseService.TryPurchase` |
| Dynamic rank cap by deepest unlocked tier (1.1) | `SkillTierRules.DynamicRankCapForTree` + `SkillTreeState.DynamicRankCap` + `TryRankUp` |
| 1 point/rank incl. capstone (1.2) | `SkillTreeState.Purchase`/`RankUp` (cost 1) |
| Per-node prerequisites (1.3, additive `prerequisites`) | `SkillNodeDataSO.PrerequisiteNodeIds` (preexisting) checked in `TryPurchase` |
| Numbers from ADDENDUM as executor contract (1.4) | `ActiveSkillExecutionController.RegisterCombatExecutors` values already equal §1.2–§1.6; no number invented |
| Named hooks IGold/IHarvest/ICraftCost/IToolEfficiency + "efeito pendente" (1.6) | `SkillEffectRoute` interfaces + `SkillModifierHooks` + `SkillNodeDataSO.EffectPending`/`EffectPendingTooltip` |
| Marcador de Presa canonical text (1.10) | dormant active; text "dano do jogador e de aliados invocados, quando existirem" — see note below |
| Respec punitive + equipment revalidation (1.11) | `SkillTierEquipGate` + recompute in `SkillTreeManager.TryRespec` + read-gate in `EquipmentManager.EquipItem` |
| Retire ActiveSkillSlots legacy; keep keys 1-4 (item 7) | `ActiveSkillSlots` behavioral retirement; manager auto-assign + save use keys 1-4 |
| 5 dead modifiers: consume OR retire (item 8) | ALL 5 ROUTED into `DerivedStatsCalculator` (consume route, none retired); 2 read by combat today, 3 reader-pending — see "Dead modifier decisions" |
| Fix stale "55" → 69 (item 9) | `ValidateSkillTreeRuntimeBinding.cs` (uses `CanonicalNodeCount`); `DefaultSkillCatalog` comment |
| Save additive + backward-compat, no Unity refs | `SkillTreeSaveData` (Version=2, NodeRanks, variants) — simple types/IDs only |
| EditMode tests (gating/caps/exclusivity/aggregator/migration/respec/hooks/prereq) | `Assets/_Game/Tests/EditMode/Skills/CanonicalCatalogTests.cs` (22 tests) |
| Editor generator + per-tree count validation | `Assets/_Game/Scripts/Editor/Skills/GenerateCanonicalSkillCatalog.cs` |

### Dead modifier decisions (emenda item 8 — route per modifier)

Route taken for all five: **ROUTED into the DerivedStats provider** (F02) via `DerivedStatsCalculator`
(none retired, none left dangling unaggregated on a node). Honest split between modifiers whose
downstream **reader exists today** vs. those whose **reader is pending** (the value is computed and
exposed on `DerivedStats`, same "efeito pendente" discipline as the named hooks — a future combat/
movement reader consults it; the node is not a silent no-op because the aggregator does publish it).

| Modifier | Route | Sink (DerivedStats field) | Reader status | Nodes affected |
|----------|-------|---------------------------|---------------|----------------|
| `DualWieldAttackSpeedBonus` | ROUTED → consumed | `AttackSpeed` | **READER EXISTS** — combat reads AttackSpeed (`PlayerCombatStatsProvider`/`PlayerAttackController`) | `melee_dual_wield_flow` |
| `TwoHandedDamageBonus` | ROUTED → consumed | `Attack` (flat) | **READER EXISTS** — combat reads Attack | `melee_two_handed_momentum` |
| `BowProjectileSpeedFlat` | ROUTED → aggregated | `BowProjectileSpeed` | READER PENDING — arrows currently use `bowWeapon.ProjectileSpeed`; reader to consult `DerivedStats.BowProjectileSpeed` is a future Combat slice (out of this spec's `Skills/**`+`DerivedStatsCalculator` scope) | `ranged_projectile_tuning`, `ranged_capstone_eagle_focus` |
| `DodgeCostReduction` | ROUTED → aggregated | `DodgeCostReduction` | READER PENDING — dodge stamina cost (`PlayerDodgeController`/`PlayerAttackController`) reader pending; out of scope here | `melee_dodge_training` |
| `StatusDurationReduction` | ROUTED → aggregated | `StatusDurationReduction` | READER PENDING — `PlayerStatusReceiver` uses resistance-based reduction today; reader pending; out of scope here | `survival_status_recovery` |

No modifier retired. Two reach gameplay today; three are aggregated into a derived field whose
reader is pending (wiring those readers lives in Combat/Player files outside this spec's allowed
scope, so the routing is done here and the read-side is deferred — declared, not hidden). This
fulfills "consumir OU aposentar" via the consume route (all five aggregated into the provider),
with honest reader-status disclosure for the three pending ones.

### Capstone exclusivity model (CA-3) — count-preserving rationale

The WI-11 catalog has **one** capstone node per tree, and `CatalogExpectations.cs` (fable_30,
already BUILD_VALIDATED) enumerates all 69 node IDs and asserts count = 69. To honor CA-3 (Kanthor
XOR Kaand; Anya XOR Senya) **without** adding/renaming nodes (which would break fable_30 and exceed
declared scope), exclusivity is modeled as an **intra-node variant choice**: the Melee capstone
(`melee_capstone_battle_rhythm`) carries variants `{kanthor, kaand}` and the Magic capstone
(`magic_capstone_elemental_confluence`) carries `{anya, senya}`. Choosing one variant at purchase
(with confirmation via `RequiresCapstoneConfirmation`) permanently blocks the other until a full
respec — satisfying "Kanthor comprado → Kaand bloqueado" and the inverse for both pairs.

### Final node count (CA-COUNT)

**69 nodes** = melee 14 + ranged 11 + magic 13 + survival 16 + crafting 15 (WI-11 composition,
identical to `CatalogExpectations.cs`). The emenda's alternate per-tree breakdown
(14/13/15/14/13) is the direction's intended split; the binding number is **69 total**, preserved
exactly. Stale "55" assertion corrected in `ValidateSkillTreeRuntimeBinding.cs`.

### Marcador de Presa note (item 1.10)

`ranged_marked_prey` is a dormant active (no marking system executor yet). Its canonical effect
text reads "aumenta o dano do jogador E de aliados invocados, quando existirem". The damage
modifier routes via the aggregator hook so summoned companions inherit it automatically when
WAVE 14 (companions in combat) arrives — no rework on the node. Until then it applies to the
player only; the canonical text is already final.

---

## Validation

```
Validation method: run_strict_validation.ps1
Exit code: 0
Assembly-CSharp: PASS (exit 0, 0 errors, 0 warnings)
Assembly-CSharp-Editor: PASS (exit 0, 0 errors, 3 pre-existing warnings)
Docs validation: PASS (validate_docs.ps1 exit 0)
Diff completeness: PASS (check_spec_diff_completeness.ps1)
Quality check: PASS
Result artifact: docs/validation/LAST_STRICT_VALIDATION_RESULT.json
```

- `dotnet build .\Assembly-CSharp.csproj --no-restore` → exit 0, 0E/0W.
- `dotnet build .\Assembly-CSharp-Editor.csproj --no-restore` → exit 0, 0E/3W (pre-existing).
- `.\tools\docs\validate_docs.ps1` → exit 0 (Docs validation PASSED).
- `.\tools\docs\check_spec_diff_completeness.ps1` → PASS.
- `.\tools\docs\run_strict_validation.ps1` → exit 0 (VALIDATION_PASS).

EditMode tests authored (22): not executed here (Unity Test Runner deferred with Phase 2-3). They
are pure C# and compile in `Assembly-CSharp.csproj` (0E). The build compiling the test file is the
authoritative compile signal for them; runtime execution is part of the deferred final validation.

---

## Testing Quality Gate

```
Changed runtime code: YES
Changed deterministic logic: YES (tier gating, dynamic rank cap, prerequisites, exclusivity,
  aggregation, named hooks, equip gate, save migration/refund)
Changed Unity scene/prefab/asset wiring: NO (asset generation is via editor generator; not run here)
Automated tests added/updated: YES (Assets/_Game/Tests/EditMode/Skills/CanonicalCatalogTests.cs, 22 tests)
Automated tests command: Unity Test Runner EditMode — NOT RUN (deferred to final validation; compiles 0E)
Manual Play Mode scenario: DEFERRED_TO_FINAL_VALIDATION (owner authorized; F14 screen + buy-to-tier-2
  + use one active is the human scenario)
Justification if no automated tests: N/A (tests added)
Residual risk: EditMode tests compile but were not executed in a Unity runner in this session; the
  asset generator (GenerateCanonicalSkillCatalog) was not run (no .asset evidence) — the runtime
  uses the code catalog (DefaultSkillCatalog) as the live source, so the assets are optional until
  the human runs the generator. Capstone variant runtime effects (Kanthor cura / Senya overload)
  are gated by future combat-capstone runtime; this spec locks the data + exclusivity, not the
  capstone proc runtime. Three of the five formerly-dead modifiers (BowProjectileSpeed /
  DodgeCostReduction / StatusDurationReduction) are aggregated into DerivedStats but their
  read-side is reader-pending (out-of-scope Combat/Player files). The punitive equip gate is
  correct on the read+recompute side but has no runtime data source yet (no item tier requirement
  registered) — dormant until a future registration pass.
```

---

## Save / load impact

```
Changes save schema? YES (additive: SkillTreeSaveData.Version=2, NodeRanks, ChosenCapstoneVariants)
Adds save section? NO (existing SkillTree section, version bump)
Requires migration? YES (unknown node ids → refund total of points, logged; legacy ranks default 1)
Persists Unity references? NO (string ids + ints only — save-dto-simple-types-only respected)
Owner/restore order: unchanged (SkillTreeManager owns the section)
Tier-unlock equip gate: DERIVED from points-spent-in-tree (already persisted) — no new save field
```

---

## Architecture / anti-regression

- No `GameObject.Find` / `FindObjectOfType` introduced in runtime code.
- Gameplay communication via `GameEventBus` (existing purchase/respec/derived-stats events reused).
- Save DTOs: simple types + IDs only.
- Economy intact: cap 100, 1 pt/2 levels, 50 base (F42) — REMODELAR remained CANCELLED.
- Slots: live path is `ActiveSkillExecutionController` keys 1-4; legacy R/T/Y/G retired.
- `CatalogExpectations.cs` (fable_30) untouched; 69 IDs preserved.
- Tests only under `Assets/_Game/Tests/EditMode/**`.

## Files changed

```
Assets/_Game/Scripts/Skills/SkillEffectRoute.cs            (new — routes + named-hook interfaces)
Assets/_Game/Scripts/Skills/SkillModifierHooks.cs          (new — named-hook registry/read-back)
Assets/_Game/Scripts/Skills/SkillTierRules.cs              (new — thresholds + dynamic rank cap)
Assets/_Game/Scripts/Skills/SkillTierEquipGate.cs          (new — punitive respec equip gate)
Assets/_Game/Scripts/Skills/SkillEffectAggregator.cs       (new — single recompute authority)
Assets/_Game/Scripts/Skills/SkillNodeDataSO.cs             (additive fields: Tier/route/variant/flags)
Assets/_Game/Scripts/Skills/SkillEnums.cs                  (5 dead modifiers annotated as consumed)
Assets/_Game/Scripts/Skills/SkillTreeState.cs              (ranks + per-tree points + variants + refund)
Assets/_Game/Scripts/Skills/SkillTreeSaveData.cs           (additive: Version/NodeRanks/variants)
Assets/_Game/Scripts/Skills/SkillPurchaseService.cs        (tier/cap/prereq/exclusivity/rank-up)
Assets/_Game/Scripts/Skills/SkillTreeManager.cs            (variant confirm, rank-up, punitive respec, migration)
Assets/_Game/Scripts/Skills/SkillPassiveApplicator.cs      (obsolete — superseded by aggregator)
Assets/_Game/Scripts/Skills/ActiveSkillSlots.cs            (retired legacy R/T/Y/G input)
Assets/_Game/Scripts/Skills/DefaultSkillCatalog.cs         (tiers/routes/variants/dormant + 69 comment)
Assets/_Game/Scripts/Player/DerivedStatsCalculator.cs      (consume 5 dead modifiers)
Assets/_Game/Scripts/Equipment/EquipmentManager.cs         (read-only tier equip gate consult)
Assets/_Game/Scripts/Editor/Skills/GenerateCanonicalSkillCatalog.cs (new — asset generator + count validator)
Assets/_Game/Scripts/Editor/Validation/ValidateSkillTreeRuntimeBinding.cs (55→69)
Assets/_Game/Tests/EditMode/Skills/CanonicalCatalogTests.cs (new — 22 EditMode tests)
Assembly-CSharp.csproj / Assembly-CSharp-Editor.csproj      (compile includes for new files/test)
```

## Remaining work (deferred)

- Run `CindarsHope/Skills/Generate Canonical Skill Catalog` in Unity to materialize the 69 `.asset`
  files (optional — runtime uses the code catalog; generator records asset evidence).
- Run Unity Test Runner EditMode (≈22 new tests + existing suite).
- F14 skill-tree screen: display tier/rank/cap/"efeito pendente"/capstone variant + confirm dialog
  (UI is out of scope; data is now present).
- Capstone proc runtime (Kanthor cura / Telisandra fôlego / Anya·Senya seed window) — future combat
  capstone slice; this spec locks the data and exclusivity only.
- Future consumer specs (F06/F17/F31/F48/F49) implement the named hooks (gold/harvest/craft/tool).
- **Reader wiring for 3 routed modifiers (reader-pending):** `BowProjectileSpeed`,
  `DodgeCostReduction`, `StatusDurationReduction` are aggregated into `DerivedStats` but the
  read-side (bow projectile speed, dodge stamina cost, player status duration) lives in Combat/
  Player files outside this spec's allowed scope. A future Combat slice should have those readers
  consult `DerivedStats`. Until then these three are computed-but-unread (declared, not hidden).
- **Equip gate runtime data source (dormant):** `SkillTierEquipGate.RegisterRequirement` is not
  yet called by any runtime catalog/config, so at runtime `_blocked` is always empty and
  `EquipItem` blocks nothing. The read-side consult + respec recompute are correct and CA-6 is
  proven in EditMode with a synthetic requirement; the feature is dormant until a future item
  tier-requirement registration pass populates it.

## Known latent items (from non-regression audit)

- `SkillTreeState.Purchase(nodeId, cost)` 2-arg overload passes `treeId = null` and skips per-tree
  accrual; documented as test/validator-only (runtime always uses the 3-arg form with `node.TreeId`).
  No runtime regression; warning comment added.

## validated_game_rules

- `docs/game_rules/player_rules.md` — required_game_rules of the spec; player progression/skill context.
- `docs/game_rules/skill_tree_rules.md` — canonical economy/tier/rank/respec/persistence; this spec
  implements its rules (tier dynamic cap, 1 pt/rank, prerequisites, punitive respec, keys 1-4).
