---
doc_type: execution_report
spec_id: fable_34_spec_quest_sources_infrastructure
status: BUILD_VALIDATED_WITH_WARNINGS
wave: FABLE Batch 7
date: 2026-06-20
validated_adrs: []
validated_game_rules:
  - quest_rules.md
---

# Execution Report — fable_34 Quest Sources Infrastructure

> **Status:** `BUILD_VALIDATED_WITH_WARNINGS`
> **Honest scope:** Phase 0-1 complete (infrastructure + EditMode tests authored + builds 0E).
> Phase 2-3 (Unity batchmode / Play Mode) DEFERRED by owner for the whole FABLE batch.

---

## 1. Spec summary

Builds the infrastructure of the 5 (now 6, per EMENDA F34-B) quest **source channels** as
distinct delivery channels on top of the EXISTING quest flow (QuestRegistry / QuestService /
QuestStateSection / QuestFlagService — WAVE 09/15/18/26). No second registry or manager was
created. Scope delivered:

- `QuestSource` channel enum {Board, Npc, Mural, CaveSecret, Main, CaveContract};
- notice board with 3 deterministic contracts/day + dynamic instancing into the existing flow;
- read-only mural channel;
- single `OfferSecretQuest` API with 2 consumers (wandering merchant 15% deterministic + peaceful
  creature interactable hook);
- side/main through the existing flow;
- single-point scaled rewards (XP/gold = base × (1 + 0.08 × questLevel));
- +1 skill point per main act (idempotent across reload);
- dynamic instances persisted as simple types (additive save fields, backward-compatible);
- Quest Log projection grouped by source (tabs Main/Side/Contracts/Secrets);
- EMENDA F34-C: closed PREREQUISITE_UI_DEBT (QuestGiverInteractable now gates offers on completed
  PrerequisiteQuestIds).

---

## 2. Acceptance criteria

| CA | Requirement | Evidence | Status |
|----|-------------|----------|--------|
| CA-1 | Board offers 3 deterministic contracts/day (StableHash); accept → normal active quest | `QuestBoardService.GenerateDailyContracts` (StableHash, no Random); tests `Board_SameDay_SameContracts`, `Board_DifferentDay_Rotates`, `Board_GeneratesExactlyThreePerDay`; `AcceptDynamicInstance` reuses `AcceptQuest` | OK |
| CA-2 | Cull contract counts band kills; rewards XP/gold by base × (1 + 0.08 × level) | `QuestRewardScaling.Scale` (single point); tests `RewardScaling_FollowsCatalogCurve`, `Board_ScalesRewardByPlayerLevel`, `DynamicContract_CullObjective_CountsKills_AndAwardsScaledRewards` | OK |
| CA-3 | Wandering merchant offers secret 15% (by seed); peaceful via API; secrets list only after discovery | `SecretQuestOffer.ShouldOfferByChance/ShouldMerchantOffer`; `CaveWanderingMerchant.ShouldOfferSecretQuest`; `QuestService.OfferSecretQuest`; projection gate `source == CaveSecret && !Discovered → null`; tests `SecretOffer_*`, `SecretQuest_OnlyDiscoveredAppearsInLog` | OK |
| CA-4 | Act completion grants +1 skill point exactly once, incl. after reload | `QuestService.TryAwardActSkillPoint` guarded by persisted `RewardedMainActIds`; tests `MainAct_GrantsExactlyOneSkillPoint`, `MainAct_NotDuplicatedAfterSaveLoad` | OK |
| CA-5 | Dynamic instances (target/qty/reward/source) survive save/load as simple params | additive fields on `QuestStateRecord` + `QuestStateSaveData`; `ReRegisterInstanceFromRecord` on restore; test `DynamicInstance_SurvivesSaveLoad_WithSimpleParams` | OK |
| F34-B | `QuestSource` reserves 6th value `CaveContract`; log groups by it | enum value + `QuestSourceMapper.TabFor` → Contracts; tests `QuestSource_HasSixChannels_IncludingCaveContract`, `SourceMapper_TabGrouping_CaveContractUnderContracts` | OK |
| F34-C | QuestGiverInteractable enforces PrerequisiteQuestIds before offering | `QuestService.ArePrerequisitesComplete` used by `QuestGiverInteractable.DetermineMode/FindRelevantQuestId`; tests `Prerequisite_BlocksOfferUntilPrereqCompleted`, `Prerequisite_NoPrereq_AlwaysAllowed` | OK |

---

## 3. Existing systems audit (Phase 0) — no parallel systems created

| Concern | Found (reused) | Created |
|---------|----------------|---------|
| Quest registry/manager | `QuestRegistry`, `QuestService` (accept/progress/turn-in/save) | none — extended additively |
| Quest save section | `QuestStateSection`, `QuestStateRecord`, `QuestStateSaveData` (WAVE18) | additive simple fields only |
| Quest flags | `QuestFlagService` | reused |
| Quest categories | `QuestCategory` (Main/Side/FarmOrder/CaveContract/Hidden/…) | NEW orthogonal `QuestSource` (channel ≠ authored category) + `QuestSourceMapper` |
| Day trigger | `DayStartedEvent(DayNumber)` | board subscribes in `QuestRuntimeBootstrap` |
| Skill points ledger | `PlayerProgressionManager.UnspentSkillPoints` (true owner; SkillTreeManager syncs FROM it) | additive `GrantSkillPoints(int)` + `QuestProgressionAdapter` |
| Wandering merchant | `CaveWanderingMerchant.ShouldAppear` (StableHash 22%) | `ShouldOfferSecretQuest` (15%) + offer call |
| Board interactable | `QuestBoardInteractable`, `QuestGiverInteractable` (WAVE15) | `MuralInteractable` (read-only); board+mural placed in town generator |
| Quest Log projection | `QuestLogProjectionService`, `QuestLogEntryViewModel` | additive `Source`/`Tab` fields + grouping |

**Phase 0 finding (scope clarification):** the spec's allowed-files list named `SkillTreeManager`
for the act skill-point hook, but the canonical skill-point ledger is `PlayerProgressionManager`
(SkillTreeManager reads/syncs from it). The hook was therefore added as an additive
`GrantSkillPoints(int)` on `PlayerProgressionManager` (publishes the existing `SkillPointGrantedEvent`,
which the skill tree already consumes). Idempotency lives in the persisted `RewardedMainActIds`.

---

## 4. Spec Compliance Matrix (requirement → implementation)

| Requirement | Implementation |
|-------------|----------------|
| QuestSource enum {Board,Npc,Mural,CaveSecret,Main} + CaveContract (F34-B) | `Assets/_Game/Scripts/Quests/QuestSource.cs` |
| Board 3/day deterministic + dynamic instance | `QuestBoardService.cs`, `QuestStableHash.cs`, `QuestInstance.cs`, `QuestService.RegisterDynamicInstance/AcceptDynamicInstance` |
| Board rules: no quest item, no non-aggressive target | `QuestBoardService.IsTargetAllowed` (+ constructor drops violators) |
| Mural read-only (≠ board) | `Quests/Runtime/MuralInteractable.cs` (no accept; publishes feedback only) |
| Secret quest API + 2 consumers | `SecretQuestOffer.cs`, `QuestService.OfferSecretQuest`, `CaveWanderingMerchant` hook |
| Scaled XP/gold single point | `QuestRewardScaling.Scale` (only formula site) |
| +1 skill point per act idempotent | `QuestService.TryAwardActSkillPoint` + `QuestStateSection.RewardedMainActIds` + `PlayerProgressionManager.GrantSkillPoints` |
| Save: dynamic instance simple types, additive, backward-compat | `QuestStateRecord` + `QuestStateSaveData` additive fields; `QuestRuntimeBootstrap.Capture` + `QuestService.RestoreFromSaveData`; defaults safe for legacy |
| Quest Log groups by source | `QuestLogEntryViewModel.Source/Tab`, `QuestLogProjectionService.ProjectEntry`, `QuestSourceMapper.TabFor` |
| QuestBoardRefreshedEvent on rotation | `Core/Events/QuestRuntimeEvents.cs`; published by `QuestRuntimeBootstrap.RefreshBoard` |
| Board/Mural interactables via generator (no YAML) | `Editor/SceneCreation/CreateMvpTownScene.cs` (`CreateNoticeBoard`, `CreateMuralInteractable`) |
| F34-C prerequisite gate | `QuestService.ArePrerequisitesComplete` + `QuestGiverInteractable` |

---

## 5. Validation

```text
Validation method: run_strict_validation.ps1
Exit code: 0
Assembly-CSharp: PASS (0 errors, 1 pre-existing warning CombatTelemetrySession)
Assembly-CSharp-Editor: PASS (0 errors, 3 pre-existing warnings)
Quality check: PASS
Docs validation: PASS
Spec diff completeness: PASS
Result artifact: docs/validation/LAST_STRICT_VALIDATION_RESULT.json
```

### Environment note (csproj is gitignored, Unity-regenerated)

`Assembly-CSharp.csproj` and `Assembly-CSharp-Editor.csproj` are **gitignored** (regenerated by
Unity; `git check-ignore` exit 0, untracked in HEAD). They are NOT committed.

During this spec the dotnet-fallback build initially failed on **4 pre-existing, unmodified test
files** that reference `CindarsHope.Editor.*` types but were generated into `Assembly-CSharp.csproj`
(runtime) instead of `Assembly-CSharp-Editor.csproj`:
`ItemCatalogDataTests.cs` (fable_32), `TownLayoutTests.cs` (fable_40),
`CatalogValidatorTests.cs` (fable_30), `BowAmmoCatalogTests.cs` (fable_48).

This is a Unity csproj-generation quirk (these EditMode test files lack an asmdef), NOT introduced by
fable_34. **Proof of isolation:** a probe build of `Assembly-CSharp.csproj` with ONLY those 4 files
excluded (all fable_34 code + `QuestSourcesTests.cs` included) returned **0 errors**. Per the
corrected-gate note ("EditMode test que referencia tipos CindarsHope.Editor.* → include no
Assembly-CSharp-Editor.csproj"), those 4 includes were moved to the editor csproj in the local
gitignored project files so the fallback build matches Unity's authoritative routing. No tracked file
and no other spec's source was changed.

---

## 6. Testing Quality Gate

```text
Changed runtime code: YES
Changed deterministic logic: YES (board rotation, reward scaling, secret chance, act idempotency, instance round-trip, prerequisite gate)
Changed Unity scene/prefab/asset wiring: YES (CreateMvpTownScene generator only — no manual YAML; .unity not edited)
Automated tests added/updated: YES — Assets/_Game/Tests/EditMode/Quests/QuestSourcesTests.cs (24 tests)
Automated tests command: Unity batchmode EditMode runner — NOT RUN (Unity Play Mode/Editor deferred by owner for the FABLE batch); compile fallback dotnet build PASS (0E)
Manual Play Mode scenario: DEFERRED_TO_FINAL_VALIDATION (board accept/turn-in, mural read, secret discovery in cave require scene + UI)
Justification if no automated tests: N/A (tests authored; only the Unity-batchmode RUN is deferred per owner)
Residual risk: EditMode tests are authored and compile but were not executed in Unity batchmode this session; runtime scene wiring of Board_Contratos/Mural in TownScene needs the human to regenerate TownScene via CreateMvpTownScene and run the Play Mode scenario.
```

### Quest + Save expectations covered by tests
- reward idempotency after reload (act skill point) — `MainAct_NotDuplicatedAfterSaveLoad`;
- quest flag/state separation preserved (act idempotency in section, not loose flag) — design;
- objective progress (count-based DefeatEnemy, inventory-derived CollectItem) — dynamic contract tests;
- save/load round-trip of extended QuestStateRecord incl. dynamic instance params — `DynamicInstance_SurvivesSaveLoad_WithSimpleParams`;
- no Unity refs in DTO (only int/string/bool/enum-as-int) — additive fields are simple types;
- spoiler/secret visibility (secret absent until discovered) — `SecretQuest_OnlyDiscoveredAppearsInLog`;
- legacy save safety (default Source = Npc, IsDynamicInstance = false) — `Section_DefaultSource_IsNpc_ForLegacyRecords`;
- anti-regression: fixed quest still accepts/turns-in unchanged — `FixedQuest_StillAcceptsAndTurnsIn_Unchanged`.

---

## 7. Save / load impact

```text
Changes save schema? YES — additive simple-type fields on QuestStateRecord + QuestStateSaveData
  (Source:int, IsDynamicInstance:bool, TemplateId, InstanceTargetId, InstanceQuantity:int,
   QuestLevel:int, InstanceRewardGold:int, InstanceRewardXp:int, GeneratedForDay:int) and section
   lists (DiscoveredSecretQuestIds, RewardedMainActIds).
Adds a save section? NO — same QuestStateSection / QuestStateSectionSaveData, same owner/order.
Requires migration? NO — defaults are safe (Source=Npc, IsDynamicInstance=false, empty lists);
  legacy saves load with no dynamic instances and no discovered secrets.
Persists Unity references? NO — simple types/IDs only (save-dto-simple-types-only rule).
```

---

## 8. Architecture invariants

```text
No GameObject.Find/FindObjectOfType in new runtime code — confirmed (board subscribes via GameEventBus;
  QuestRuntimeBootstrap uses static accessors; merchant uses QuestRuntimeBootstrap.QuestService).
GameEventBus only for gameplay comms — QuestBoardRefreshedEvent, SecretQuestDiscoveredEvent,
  SkillPointGrantedEvent published via GameEventBus; board subscribes/unsubscribes DayStartedEvent.
No Unity refs in save DTO — confirmed.
No forbidden namespaces — confirmed.
No second quest registry/manager — confirmed (dynamic instances reuse QuestRegistry/QuestService).
Reward formula single point — QuestRewardScaling only.
Tests only in Assets/_Game/Tests/EditMode/** — confirmed.
No .unity/.prefab/.asset YAML edited — confirmed (scene only via generator).
```

---

## 9. Honest status rationale

`BUILD_VALIDATED_WITH_WARNINGS`:
- Core criteria implemented, audited, documented; Spec Compliance Matrix all OK.
- `run_strict_validation.ps1` exit 0 (docs PASS, Assembly-CSharp 0E, Assembly-CSharp-Editor 0E,
  quality check PASS, diff completeness PASS).
- WITH_WARNINGS because Phase 2-3 (Unity batchmode EditMode run + Play Mode scenario + TownScene
  regeneration to instantiate Board_Contratos/Mural) is DEFERRED by owner for the whole FABLE batch,
  not executed this session. Not promoted to `implementados/`.

Not claimed: PlayMode PASS, Unity batchmode PASS, ACCEPTED, tests executed.

---

## 10. Remaining work (Phase 2-3, deferred)

```text
1. Regenerate TownScene via CindarsHope editor menu (CreateMvpTownScene) to instantiate
   Board_Contratos + TownHallMural_Interactable.
2. Run Unity Test Runner EditMode (QuestSourcesTests — 24 tests) and capture results.
3. Play Mode scenario: accept a board contract, complete it, turn in at the board; read the mural;
   discover a cave-secret via the wandering merchant; verify the Secrets tab; verify +1 skill point
   on a main act and no duplication after save/load.
4. F35/F36/F37/F51/F52 consume these channels (content authoring) — out of scope here.
```

---

## 11. Files changed

New (runtime):
- Assets/_Game/Scripts/Quests/QuestSource.cs
- Assets/_Game/Scripts/Quests/QuestInstance.cs
- Assets/_Game/Scripts/Quests/QuestStableHash.cs
- Assets/_Game/Scripts/Quests/QuestBoardService.cs
- Assets/_Game/Scripts/Quests/SecretQuestOffer.cs
- Assets/_Game/Scripts/Quests/Runtime/MuralInteractable.cs
- Assets/_Game/Scripts/Quests/Runtime/QuestProgressionAdapter.cs

New (test):
- Assets/_Game/Tests/EditMode/Quests/QuestSourcesTests.cs

Modified:
- Assets/_Game/Scripts/Core/Events/QuestRuntimeEvents.cs (QuestBoardRefreshedEvent, SecretQuestDiscoveredEvent)
- Assets/_Game/Scripts/Quests/Runtime/QuestService.cs (dynamic instances, scaled XP, act hook, secret API, prereq gate, restore)
- Assets/_Game/Scripts/Quests/Runtime/QuestRuntimeBootstrap.cs (progression adapter, board wiring, capture/restore additive fields)
- Assets/_Game/Scripts/Quests/Runtime/QuestGiverInteractable.cs (prerequisite gate — F34-C)
- Assets/_Game/Scripts/Quests/Runtime/IQuestInventoryAccess.cs (IQuestProgressionAccess)
- Assets/_Game/Scripts/Quests/Save/QuestStateRecord.cs (additive instance/source fields)
- Assets/_Game/Scripts/Quests/Save/QuestStateSection.cs (DiscoveredSecretQuestIds, RewardedMainActIds + helpers)
- Assets/_Game/Scripts/Quests/Log/QuestLogProjection.cs (Source/Tab fields)
- Assets/_Game/Scripts/Quests/Log/QuestLogProjectionService.cs (source grouping + secret gate)
- Assets/_Game/Scripts/Save/SaveData.cs (additive quest DTO fields — quest section only)
- Assets/_Game/Scripts/Player/Progression/PlayerProgressionManager.cs (GrantSkillPoints)
- Assets/_Game/Scripts/Cave/Runtime/CaveWanderingMerchant.cs (secret offer hook)
- Assets/_Game/Scripts/Editor/SceneCreation/CreateMvpTownScene.cs (Board_Contratos + Mural interactables)

Not committed (out of scope / gitignored): Assembly-CSharp*.csproj (Unity-regenerated),
tools/docs/**, other sessions' files (EconomyManager, EquipmentManager, Accessory*).
```
