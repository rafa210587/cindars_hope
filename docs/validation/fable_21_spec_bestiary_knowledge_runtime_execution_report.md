# Execution Report — fable_21 Bestiary Knowledge Runtime

> **Spec:** `.specs/a_implementar/fable/fable_21_spec_bestiary_knowledge_runtime.md`
> **Date:** 2026-06-19
> **Branch:** dev
> **Status:** `BUILD_VALIDATED_WITH_WARNINGS`
> **Type:** Runtime / Save
> **validated_adrs:** ADR-0005 (cave stable-run determinism — knowledge derives from deterministic events; no new RNG)
> **validated_game_rules:** cave_rules.md (bestiary populates from cave combat events; no change to cave run stability contract)

---

## Honest status rationale

`BUILD_VALIDATED_WITH_WARNINGS`. The mechanical core (K0-K4 progression, C1 thresholds, external
grants, spoiler gate, boss-defeat reveal, FullyDocumented damage bonus, +1 skill point per 10
Studied milestone, save round-trip + legacy load) is implemented, audited against the existing
systems, and covered by 27 EditMode tests. Both assemblies build with exit code 0 and
`run_strict_validation.ps1` returns exit code 0.

Deferred (per owner authorization — Phase 2-3 / Play Mode is DIFERIDO):
- **Phase 2 (Unity batchmode compile):** NOT RUN — owner authorized skipping Unity Editor; the
  authoritative Unity compile signal was not executed. Residual risk: `.meta` GUIDs and import wiring
  for the 5 new scripts were authored by hand (project convention: minimal script meta) and will be
  reconciled by Unity on next import.
- **Phase 3 (human Play Mode):** NOT RUN — DEFERRED_TO_FINAL_VALIDATION per spec
  (`Human validation timing: DEFERRED_TO_FINAL_VALIDATION`). The required human scenario (unlock 1
  entry by combat + 1 by analysis) is part of the wave-end batch.
- **Skill-point milestone wiring:** the service publishes `BestiaryMilestoneReachedEvent` and exposes
  a `SkillPointGrantSink` seam, but the SkillTreeManager subscription that consumes it lives outside
  this spec's allowed files (SkillTreeManager.cs is not in scope). The grant amount/idempotency is
  proven by tests via the sink; the bus consumer is a documented follow-up wiring (see Remaining work).

This report claims neither Unity-validated, Play Mode PASS, nor ACCEPTED.

---

## Acceptance criteria extracted

| ID | Criterion | Implementation | Evidence | Status |
|----|-----------|----------------|----------|--------|
| CA-1 | Combat thresholds: 5 kills→DropsCommon; 3 effective fire hits→ElementVulnerability; 1 sighting→Identity; same action 3x OR suffered 1x→Behavior; 3 resisted→ResistanceTags | `EnemyKnowledgeService.EvaluateThresholds` with the C1 constants | Tests: `FiveKills_RevealCommonDrops`, `ThreeEffectiveFireHits_RevealVulnerability`, `EffectiveHitsOnDifferentAxes_DoNotRevealVulnerability`, `Sighting_RevealsIdentity`, `SeeingSameActionThreeTimes_RevealsBehavior`, `SufferingActionOnce_RevealsBehavior`, `ThreeResistedHits_RevealResistance` | OK |
| CA-2 | `GrantKnowledge(enemyId, category, source)` unlocks without grind; idempotent (no duplicate state, no re-publish) | `EnemyKnowledgeService.GrantKnowledge` → `Unlock` (set-guarded) | Tests: `GrantKnowledge_UnlocksWithoutGrind`, `GrantKnowledge_IsIdempotent_NoDuplicateUnlockEvent` | OK |
| CA-3 | SpoilerTier 3+ hidden until quest flag; gate covers tiers 0-4 | `EnemyKnowledgeService.IsVisible` (single decision point) + injected `QuestFlagSource`/`SpoilerTierFlagSource` | Tests: `LowTierCategories_AreVisibleWhenUnlocked`, `UnseenCategory_IsNeverVisible`, `Tier3Category_HiddenUntilQuestFlagSet`, `Tier4Category_HiddenUntilQuestFlagSet`, `Tier3Category_WithNoFlagConfigured_StaysHidden` | OK |
| CA-4 | Round-trip preserves counters + unlocked categories; legacy save (no section) → empty codex, no error | `BestiaryKnowledgeEntrySaveData` + `RestoreKnowledge` null-safe; appended to existing `BestiarySaveData` | Tests: `SaveRoundTrip_PreservesCountersAndCategories`, `LegacyLoad_NullKnowledge_YieldsEmptyCodex`, `InvalidEnemyId_IsIgnored`, `RestoreEntry_WithBlankId_IsSkipped` | OK |
| CA-5 | Each new category unlock publishes `BestiaryKnowledgeUnlockedEvent(enemyId, category, tier)` exactly once | `Unlock` publishes only on first add | Tests: `CrossingThreshold_PublishesUnlockEventOnce`, `UnlockEvent_CarriesSpoilerTier` | OK |
| EMENDA-B | +1 skill point per 10 Studied (max 5); idempotent (not re-granted after save/load) | `CheckMilestones` + persisted `KnowledgeMilestonesGranted` | Tests: `TenStudiedCreatures_GrantOneSkillPointMilestone`, `Milestone_PublishesEventOnce`, `Milestones_CapAtFive`, `Milestone_NotReGrantedAfterSaveLoad` | OK |
| EMENDA-D | FullyDocumented reward is MECHANICAL (+3% damage vs documented creature); bosses reveal ficha AFTER defeat | `GetDamageMultiplierVs` (1.03 when Studied) + `RecordKill`→`RevealFullFicha` when `IsBossSource` | Tests: `FullyDocumented_GrantsSmallDamageBonus`, `BossDefeat_RevealsFullFicha` | OK |

---

## Existing systems audit (Phase 0 — no parallel systems created)

| Concern | Found existing | Decision |
|---------|----------------|----------|
| Bestiary host | `Assets/_Game/Scripts/Enemy/BestiaryManager.cs` (MonoBehaviour, subscribes spawn/seen/damaged/damage-applied/killed/loot; has `CaptureSaveData`/`RestoreFromSaveData`) | **REUSED** — hosted `EnemyKnowledgeService` inside it; extended existing handlers (no duplicate subscriptions) |
| Bestiary save section | `BestiarySaveData` already wired in `SaveManager` (`[SerializeField] _bestiaryManager`, captured at line ~1347, restored at line ~1144) and exposed as `GameSaveData.Bestiary` | **EXTENDED** existing DTO (added `Knowledge` list + `KnowledgeMilestonesGranted`) — no second save path, no new provider; WI-18 ownership = BestiaryManager |
| Creature SpoilerTier / boss flags | `CindarsHope.Combat.Bestiary.CanonicalBestiaryCatalog.All` (F33) exposes `SpoilerTier`, `IsBoss` per ficha | **CONSUMED read-only** (built a lookup once); the bestiary REVEALS, never authors |
| Damage element + result payload | `DamageAppliedEvent.DamageResult` carries `DamageType`, `WasImmune`, `WasVulnerable`, `CombatResistanceMultiplier`, `TargetId` | **CONSUMED** — effective = WasVulnerable/mult>1; resisted = WasImmune/mult<1; axis = DamageType (no event payload change) |
| Action observed | `EnemyActionStartedEvent(enemyId, actionId)` | **CONSUMED** — new subscription drives behavior threshold |
| Quest flag gate | `CindarsHope.Quests.Flags.QuestFlagService.IsSet(flagId)` (plain C# class, no static accessor) | **DECOUPLED via seam** — `IsVisible` reads an injected `Func<string,bool>` (precedent: `PlayerDamageReceiver.ResistanceSource`); default = "not set" (fail-safe hidden) |
| Skill-point grant hook | `SkillTreeState.AddSkillPoints` driven by `PlayerLevelChangedEvent.GrantedSkillPoints` in `SkillTreeManager.OnPlayerLevelChanged`; `SkillPointGrantedEvent` published | **NOT EDITED (out of scope)** — service publishes `BestiaryMilestoneReachedEvent` + exposes `SkillPointGrantSink`; SkillTreeManager subscription is a documented follow-up |

No equivalent `EnemyKnowledgeService`, `EnemyKnowledgeState`, knowledge counters, spoiler gate, or
bestiary-knowledge save section existed before this spec (grep `EnemyKnowledge|KnowledgeLevel|FullyDocumented|Studied`
returned only unrelated spellbook matches).

---

## Spec Compliance Matrix

| Requirement (spec) | Implementation file | Notes |
|--------------------|---------------------|-------|
| `EnemyKnowledgeState` per enemyId {kills, secondsFought, actionsSeen, effectiveHits[element], resistedHits} + unlocked categories | `Assets/_Game/Scripts/Bestiary/EnemyKnowledgeState.cs` | Pure C#; `GetLevel()` derives K0-K4 |
| Thresholds C1 | `EnemyKnowledgeService.cs` constants + `EvaluateThresholds` | identity=1 sighting; behavior=3× action OR suffered 1×; vuln=3 effective on axis; drops=5 kills; resist=3 resisted |
| `GrantKnowledge(enemyId, categoria, fonte)` idempotent | `EnemyKnowledgeService.GrantKnowledge` | |
| `IsVisible(enemyId, categoria, tier)` single spoiler gate | `EnemyKnowledgeService.IsVisible` | tiers 0-2 open; 3+ gated by flag; fail-safe hidden |
| `BestiaryKnowledgeUnlockedEvent(enemyId, categoria, novoTier)` | `Assets/_Game/Scripts/Core/Events/BestiaryEvents.cs` | published once per unlock by `Unlock` |
| `BestiaryKnowledgeSaveData` (List per enemyId, simple types, WI-18, no Unity refs, backward-compat) | `Assets/_Game/Scripts/Bestiary/BestiaryKnowledgeSaveData.cs` + `BestiarySaveData` extension | parallel key/value lists (Unity-serializable, no Dictionary) |
| Host in BestiaryManager (no new manager) | `Assets/_Game/Scripts/Enemy/BestiaryManager.cs` | `Knowledge` property; sources wired in `Awake` |
| EMENDA-B milestones | `EnemyKnowledgeService.CheckMilestones` + `BestiaryMilestoneReachedEvent` | persisted `_milestonesGranted` |
| EMENDA-D mechanical reward + boss reveal | `GetDamageMultiplierVs` + `RecordKill`/`RevealFullFicha` | +3% magnitude fixed in Phase 0 per spec |
| EditMode tests | `Assets/_Game/Tests/EditMode/World/BestiaryKnowledgeTests.cs` | 27 tests |

---

## Validation

**Method:** `run_strict_validation.ps1` (exit code authoritative)

```
Validation method: run_strict_validation.ps1
Exit code: 0
Assembly-CSharp: PASS (rebuild, exit 0, 0E/0W)
Assembly-CSharp-Editor: PASS (exit 0, 0E/3W pre-existing)
Quality check: PASS
Docs validation: PASS (validate_docs.ps1 exit 0)
Spec diff completeness: PASS (check_spec_diff_completeness.ps1 exit 0)
Result artifact: run_strict_validation.ps1 stdout → "STRICT_VALIDATION_RESULT: VALIDATION_PASS" / "Exit code: 0"
```

| Level | Command | Result |
|-------|---------|--------|
| Runtime build | `dotnet build .\Assembly-CSharp.csproj --no-restore -t:Rebuild` | exit 0, 0E/0W |
| Editor build | `dotnet build .\Assembly-CSharp-Editor.csproj --no-restore` | exit 0, 0E/3W (pre-existing) |
| Docs | `.\tools\docs\validate_docs.ps1` | exit 0 |
| Diff completeness | `.\tools\docs\check_spec_diff_completeness.ps1` | exit 0 |
| Strict | `.\tools\docs\run_strict_validation.ps1` | exit 0 |
| Unity batchmode (Phase 2) | NOT RUN — owner authorized skipping Unity Editor | NOT RUN |
| Play Mode (Phase 3) | DEFERRED_TO_FINAL_VALIDATION | NOT RUN |

EditMode test files compile into Assembly-CSharp (single-assembly project; no asmdef). All new
files added as `<Compile Include>` to `Assembly-CSharp.csproj` (git-ignored build artifact —
regenerated by Unity from `.meta`). The test references only runtime types (`CindarsHope.Enemy.*`,
`CindarsHope.Core.*`), so it correctly lives in the runtime assembly, not the editor assembly.

---

## Testing Quality Gate

```
Changed runtime code: YES
Changed deterministic logic: YES (thresholds, counters, spoiler gate, milestones, save DTO)
Changed Unity scene/prefab/asset wiring: NO
Automated tests added/updated: YES
Automated tests command: EditMode (Unity Test Runner) — compiled via dotnet build; 27 tests in BestiaryKnowledgeTests
Manual Play Mode scenario: DEFERRED_TO_FINAL_VALIDATION (wave-end batch: unlock 1 by combat + 1 by analysis)
Justification if no automated tests: N/A (tests present)
Residual risk: Unity batchmode compile + Play Mode not executed (owner-authorized deferral); skill-point milestone bus consumer (SkillTreeManager subscription) is a follow-up wiring outside this spec's allowed files.
```

Regression coverage: legacy-save load (`LegacyLoad_NullKnowledge_YieldsEmptyCodex`) and
invalid-id fallback (`InvalidEnemyId_IsIgnored`, `RestoreEntry_WithBlankId_IsSkipped`) prove no
new failure path for existing saves; existing combat-event payloads are unchanged (subscribe-only).

---

## Save expectations

```
Does this change save schema? YES — additive fields on existing BestiarySaveData (Knowledge list + KnowledgeMilestonesGranted)
Does this add a save section? NO new section — reuses the existing Bestiary section (WI-18 owner: BestiaryManager)
Does this require migration? NO — additive; legacy save (fields absent) deserializes to empty list / 0 milestones
Does this persist Unity references? NO — strings, ints, floats, bool, parallel key/value lists only
Restore order: unchanged (BestiaryManager restored by SaveManager.ApplySaveData as before)
DTO default values: empty lists, zero counters/milestones
Invalid ID fallback: blank/null enemyId entries skipped on restore; never create state
Reward idempotency after reload: milestone count persisted → skill point not re-granted (tested)
```

---

## Architecture / invariants

- No `GameObject.Find` / `FindObjectOfType` in the new runtime code (service is pure C#; host uses event subscriptions only).
- Gameplay communication via `GameEventBus` only (consumes EnemyKilled/DamageApplied/EnemySpawned/EnemySeen/EnemyActionStarted; publishes BestiaryKnowledgeUnlocked/BestiaryMilestoneReached).
- Save DTOs contain only simple types + stable ids (no Unity refs).
- No forbidden namespaces; no manual `.unity/.prefab/.asset` YAML edits (only routine `.cs.meta` script imports authored by hand because Unity Editor was not run).
- Tests only under `Assets/_Game/Tests/EditMode/**`.

---

## Files changed

```
NEW  Assets/_Game/Scripts/Bestiary/EnemyKnowledgeState.cs (+ .meta, + Bestiary folder .meta)
NEW  Assets/_Game/Scripts/Bestiary/EnemyKnowledgeService.cs (+ .meta)
NEW  Assets/_Game/Scripts/Bestiary/BestiaryKnowledgeSaveData.cs (+ .meta)
NEW  Assets/_Game/Scripts/Core/Events/BestiaryEvents.cs (+ .meta)
NEW  Assets/_Game/Tests/EditMode/World/BestiaryKnowledgeTests.cs (+ .meta)
MOD  Assets/_Game/Scripts/Enemy/BestiaryManager.cs (host service, wire sources, feed handlers, route save)
MOD  Assets/_Game/Scripts/Enemy/BestiarySaveData.cs (additive Knowledge fields)
MOD  Assembly-CSharp.csproj (compile includes — git-ignored, not committed)
NEW  docs/validation/fable_21_spec_bestiary_knowledge_runtime_execution_report.md (this file)
```

---

## Remaining work (follow-ups, out of this spec's scope)

1. **Skill-point milestone bus consumer:** add a `BestiaryMilestoneReachedEvent` subscription in
   `SkillTreeManager` (or a thin bootstrap) to call `SkillTreeState.AddSkillPoints(1)` — SkillTreeManager.cs
   is outside this spec's allowed files. Until then the milestone fires the event + sink seam but no
   point lands unless a host wires `SkillPointGrantSink`.
2. **Quest-flag + SpoilerTier-flag wiring:** host should set `EnemyKnowledgeService.QuestFlagSource`
   to `QuestFlagService.IsSet` and `SpoilerTierFlagSource` to the canonical tier→flag map (F10/F36
   gate flags). Default convention strings are placeholders.
3. **F14 bestiary tab + effectiveness tooltips:** consume `IsVisible`/`IsUnlocked`/`GetDamageMultiplierVs`
   (UI is WAVE 22 future per spec out-of-scope).
4. **Unity import + Play Mode validation:** run on next Unity open; execute the wave-end human scenario.
```
