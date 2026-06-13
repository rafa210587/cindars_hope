---
doc_type: game_rule
status: accepted
domain: quest-gameplay
source_adrs:
  - ADR-0006
  - ADR-0007
  - ADR-0010
source_documents:
  - docs/design/gameplay/quests/QUEST_OBJECTIVE_EVENT_SYSTEM_DIRECTION.md
  - docs/design/gameplay/quests/QUEST_CATALOG_DIRECTION_v1.0.md
  - docs/design/gameplay/quests/QUESTS_MAIN_LORE_DIRECTION.md
  - docs/design/gameplay/quests/QUESTS_MAIN_PROGRESSION_REFINEMENT_DIRECTION.md
  - docs/design/FABLE_QUESTIONARIO_DECISOES_v1.0.md
  - docs/design/FABLE_DECISOES_RESPOSTAS_v1.0.md
last_reviewed: 2026-06-13
---

# Quest Rules

> **This document is current operational behavior of the quest system, not desired future state.**
> **Change these rules only via a new ADR or spec.**

## Purpose

Defines how quests are sourced, evaluated, progressed, completed and persisted in Cindar's Hope. Covers the canonical quest data model (`QuestDefinition` → `QuestState` → steps → objectives → conditions → triggers → rewards → flags), the five quest sources, reward idempotency, anti-softlock guarantees for critical quests, spoiler visibility, and the save/load contract for quest state.

Scope:

```text
IN SCOPE
  generic quest model used by all quest categories;
  the five quest sources (main / side / notice board / cave contract / cave secrets);
  condition evaluation, trigger handling, objective progress;
  reward application and idempotency after reload;
  quest flag set/clear semantics;
  anti-softlock fallback for critical (main) quests;
  spoiler/visibility control in the Quest Log;
  quest state persistence and load-time normalization;
  the macro shape of the main quest (Acts 1-5) and cave gates.

OUT OF SCOPE (defined elsewhere — cross-referenced, not duplicated)
  Fonte function unlocks and Living Water charges → fonte_rules.md (planned) + FonteAnyaSection;
  NPC schedules, dialogue trees, shop service unlocks → npc_rules.md (planned);
  save DTO type contracts and migration mechanics → save_rules.md / ADR-0006;
  GameEventBus publish/subscribe contract → event_rules.md / ADR-0007;
  nominal quest catalog (the ~86 quest list) → QUEST_CATALOG_DIRECTION_v1.0.md;
  reward XP/gold balance values → BALANCE_CURVES_DIRECTION_v1.0.md §7 (not yet a game_rule).
```

Canonical source-of-truth precedence (from the directions):

```text
QUEST_OBJECTIVE_EVENT_SYSTEM_DIRECTION wins for the generic system model.
QUESTS_MAIN_LORE / QUESTS_MAIN_PROGRESSION win for main-quest lore and act shape.
QUEST_CATALOG_DIRECTION wins for WHICH quests exist (the list).
This game_rule states what the implemented runtime does today; where runtime is partial,
the rule says so and marks the gap as "proposta a calibrar".
```

---

## Definitions and Terms

| Term | Meaning | Code anchor |
|---|---|---|
| QuestDefinition | Authored contract of a quest (id, category, steps, rewards, flags, prerequisites). Data, never runtime state. | `Assets/_Game/Scripts/Quests/QuestDefinition.cs:5` |
| QuestState / QuestStateRecord | Persisted runtime state of one quest instance. | `Assets/_Game/Scripts/Quests/QuestState.cs:5`, `Assets/_Game/Scripts/Quests/Save/QuestStateRecord.cs:23` |
| QuestCategory | Enum classifying a quest (Main, Side, FarmOrder, Festival, CaveContract, Tutorial, Hidden, System, *Future). | `Assets/_Game/Scripts/Quests/QuestCategoryType.cs:5` |
| QuestStateStatus | Canonical lifecycle state (Unknown → … → Completed/Failed/Expired/HiddenCompleted). | `Assets/_Game/Scripts/Quests/QuestCategoryType.cs:20` |
| Objective | A trackable unit of progress within a step (type + target + required amount). | `QuestObjectiveType` in `QuestCategoryType.cs:46` |
| Condition | A boolean predicate that enables/blocks a step or objective; never advances a quest by itself. | `Assets/_Game/Scripts/Quests/Conditions/QuestConditionType.cs:3` |
| Trigger / QuestEvent | A gameplay event consumed by the quest system to advance objectives. | `Assets/_Game/Scripts/Quests/Triggers/QuestTriggerType.cs:3`, `QuestEventName` in `QuestCategoryType.cs:62` |
| Reward | A declared effect applied on objective/step/quest completion. | `Assets/_Game/Scripts/Quests/Rewards/QuestRewardDefinition.cs:34` |
| QuestFlag | A persistent fact other quests/systems can read (set or cleared by rewards). | `Assets/_Game/Scripts/Quests/Flags/QuestFlagType.cs:3` |
| SpoilerTier | Integer gating how much of a quest is shown; hidden quests require `SpoilerTier > 0`. | `QuestDefinition.SpoilerTier` (`QuestDefinition.cs:12`) |

---

## Canonical Rules

### Rule 1: The Quest Data Model Is Shared by All Categories

- **Rule:** Every quest — main, side, farm order, festival, cave contract, tutorial, hidden — uses the same base model: `QuestDefinition` (authored) referenced by `QuestId`, producing a `QuestState` (runtime/persisted). Steps contain objectives; objectives reference conditions and are advanced by triggers; completion applies rewards and may grant flags.
- **Constraint:** A category may add new objective/condition/reward types, but the base contract is identical across categories. Do not create a parallel quest flow per feature.
- **Applies to:** All quest authoring and runtime code.
- **Source:** QUEST_OBJECTIVE_EVENT_SYSTEM_DIRECTION §0-§4; `QuestDefinition.cs:5`, `QuestState.cs:5`.

### Rule 2: Five Quest Sources (decision Q6.1)

- **Rule:** Quests reach the player through five sources, each with its own visual language. The implemented `QuestCategory` enum maps to these sources:

| Source (design) | Marker | QuestCategory | Notes |
|---|---|---|---|
| Main | golden "!" over story NPCs | `Main = 0` | Never expires; drives the acts |
| Side | silver "!" over NPCs with an available personal chain | `Side = 1` | Chains of 3, born from the NPC's purpose; the 3rd unlocks that NPC's unique service |
| Notice Board (Hund) | scroll in the square | `FarmOrder = 2` (delivery/gather/harvest) and procedural board templates | 3 rotating contracts/day; accept and turn in AT the board |
| Cave Contracts (Zrix) | board at the cave entrance | `CaveContract = 11` | Depth milestones and run challenges |
| Cave Secrets | no marker | `Hidden = 13` | Offered by non-aggressive creatures and wandering merchants; discovered by talking/offering |

- **Constraint:** There is no limit on active quests; exactly 1 quest is "tracked" in the HUD; the Quest Log uses tabs Main / Side / Contracts / Secrets (Secrets lists only discovered quests).
- **Edge case:** Notice-board and cave-contract dailies must never request a quest item and never target non-aggressive creatures (QUEST_CATALOG §10, §13.5).
- **Source:** FABLE_QUESTIONARIO_DECISOES_v1.0 Q6.1; QUEST_CATALOG_DIRECTION §1, §14; `QuestCategoryType.cs:5`.

### Rule 3: Quest Lifecycle States

- **Rule:** A quest moves through the canonical `QuestStateStatus` values:

```text
Unknown -> Discovered -> Available -> Active -> Waiting -> ReadyToComplete -> Completed
                                              \-> Failed / Expired / Blocked
Hidden quests may resolve to HiddenCompleted instead of a visible Completed.
```

- **Constraint:** A state is terminal when it is `Completed`, `Failed`, `Expired`, or `HiddenCompleted` (`QuestState.IsTerminal()` at `QuestState.cs:22`). A terminal quest must not retain an active step (enforced on load — see Rule 12).
- **Implementation note:** `QuestState` keeps a legacy `QuestStatus` enum (`QuestState.cs:29`) for backward compatibility; the canonical field is `StateStatus`. New code reads/writes `StateStatus`.
- **Applies to:** All quest runtime transitions.
- **Source:** QUEST_OBJECTIVE_EVENT_SYSTEM §5; `QuestCategoryType.cs:20`.

### Rule 4: Conditions Gate, Triggers Advance

- **Rule:** A `Condition` is a predicate that must be true (e.g. `HasItem`, `CurrentSeasonIs`, `ActiveLunarEventIs`, `FonteStageAtLeast`, `CaveDepthReached`). A condition **never** advances a quest by itself — it only enables or blocks an objective/step/quest. Progress is advanced only by a `Trigger`/`QuestEvent` (e.g. `OnItemCollected`, `OnEnemyDefeated`, `OnNpcDialogueEnded`) once the relevant condition allows it.
- **Constraint:** Condition categories are namespaced by domain (`QuestCondition`, `InventoryCondition`, `TimeCondition`, `LunarCondition`, `CaveCondition`, `FonteCondition`, `BestiaryCondition`, …); future categories (`SocialConditionFuture`, `PetConditionFuture`, `CompanionConditionFuture`) exist as enum values only, with no runtime implementation yet.
- **Edge case:** Trigger deduplication uses an explicit `QuestTriggerDeduplicationPolicy` (`ByEventId`, `ByDayTarget`, `ByRunSeedTarget`, `ByObjectiveCompletion`, `ManualAllowRepeat`) so the same gameplay event is not counted twice (`QuestConditionType.cs:36`).
- **Source:** QUEST_OBJECTIVE_EVENT_SYSTEM §8-§9; `QuestConditionType.cs:3`, `QuestTriggerType.cs:3`, `QuestCategoryType.cs:62`.

### Rule 5: Objective Progress Tracking

- **Rule:** Objectives advance through the `QuestService` runtime in three shapes, depending on `QuestObjectiveType`:
  - **Inventory-derived (`CollectItem`):** progress is recomputed from the current inventory count, clamped to `RequiredAmount`, on every `InventoryChangedEvent` (`QuestService.cs:133`, `OnInventoryChanged` at `:379`). This is idempotent — selling the item lowers progress again.
  - **Event-marked (`CraftItem`, `TalkToNpc`, `HarvestCrop`, `SellItem`, `ReachCaveDepth`):** the matching objective is marked complete when its event fires (`MarkObjectiveComplete` at `:174`; handlers `OnItemCrafted`, `OnCropHarvested`, `OnItemSold`, `OnNpcTalkedTo`, `OnCaveLevelEntered` at `:387`-`:511`).
  - **Count-based (`DefeatEnemy`):** each matching kill increments `CurrentProgress` by 1, clamped to `RequiredAmount` (`ProgressObjectiveCount` at `:548`; `OnEnemyKilled` at `:517`).
- **Constraint:** A `TargetId` of `"any"` matches all targets of the objective type (e.g. any crop harvested, any item sold) — see `OnCropHarvested` (`:425`), `OnItemSold` (`:454`), `OnCaveLevelEntered` (`:506`).
- **Behavior:** When all known objectives are complete, the quest transitions to `ReadyToComplete` and publishes `QuestReadyToCompleteEvent` (`EvaluateReadyToComplete` at `:347`); turn-in is then a separate explicit step.
- **Source:** `QuestService.cs:133-569`; QUEST_OBJECTIVE_EVENT_SYSTEM §6-§7.

### Rule 6: Reward Idempotency (no double reward after reload)

- **Rule:** Each reward is applied at most once per quest. Idempotency is guaranteed by `QuestStateRecord.GrantedRewardIds` (and `GrantedFlagIds` for flags). On turn-in, the applicator skips any reward whose `RewardId` is already in `GrantedRewardIds` (`QuestRewardApplicator.Apply` at `Rewards/QuestRewardApplicator.cs:43`), and `QuestService.TurnIn` records each granted reward id before publishing completion (`QuestService.cs:252`).
- **Constraint:** `GrantedRewardIds` and `GrantedFlagIds` are persisted in the save DTO (`QuestStateRecord.cs:41-42`) and restored verbatim on load (`RestoreFromSaveData` at `QuestService.cs:314`). After a save/load mid-quest, re-turning-in cannot re-grant rewards.
- **Reward types (canonical):** Gold, Item, Recipe, ToolUnlock, EquipmentUnlock, SpellUnlock, SkillPoint, SkillTreeUnlock, KnowledgeUnlock, BestiaryEntryUnlock, FonteUpgrade, LivingWaterCharge, QuestFlagGrant, QuestFlagClear, AreaUnlock, CaveDepthUnlock, ShopUnlock, ShopStockUnlock, DialogueUnlock, NpcScheduleUnlock, FestivalUnlock; future-only: RelationshipFuture, CompanionUnlockFuture, PetUnlockFuture, SocialUnlockFuture (`QuestRewardDefinition.cs:5`).
- **Edge case — adapter-gated rewards:** `FonteUpgrade` and `LivingWaterCharge` cannot be applied through the generic quest reward path; they require a Fonte adapter and otherwise fail with an explicit message (`QuestRewardDefinition.RequiresFonteAdapter()` at `QuestRewardDefinition.cs:55`; guard in `QuestRewardApplicator.cs:47`). They are routed through the FonteAnya section instead — see fonte_rules.md (planned).
- **Edge case — future rewards:** future reward types defer gracefully (`FutureDeferred`) rather than fail (`QuestRewardApplicator.cs:39`).
- **Idempotency policy options (authored):** `TrackByRewardId` (default), `TrackByFlagId`, `AllowRepeat`, `AtomicOnce` (`QuestRewardDefinition.cs:18`). Repeatable board/contract rewards use `AllowRepeat`.
- **Source:** `QuestRewardApplicator.cs`, `QuestService.cs:213-274`; QUEST_OBJECTIVE_EVENT_SYSTEM §10.

### Rule 7: Quest Flags vs Quest State

- **Rule:** A `QuestFlag` records a persistent fact that other quests/systems can query (e.g. `VaelrionIntroduced`, `CaveGateMemoryOpened`). A `QuestState` records the formal progress of one quest. Flags are **not** a substitute for state.
- **Set/clear semantics:** Flags are granted via `QuestRewardType.QuestFlagGrant` and cleared via `QuestFlagClear`, both routed through `QuestFlagService` (`QuestRewardApplicator.cs:98-130`). A grant is idempotent: if the flag id is already in `AlreadyGrantedFlagIds`, the grant is skipped (`QuestRewardApplicator.cs:105`).
- **Flag metadata:** Flags carry a type (`Boolean`, `Integer`, `String`, `Enum`, `Counter`, `Timestamp`), a scope (`QuestLocal`, `GlobalStory`, `City`, `Farm`, `Cave`, `FonteReferenceOnly`, `MainProgressionReferenceOnly`, `Shop`, `Dialogue`, `Festival`, `Debug`), and a visibility (`PublicKnown`, `PlayerKnownAfterDiscovery`, `HiddenInternal`, `DebugOnly`, `SpoilerLocked`) (`QuestFlagType.cs:3-36`).
- **Anti-pattern:** Do not hide complex progression in loose flags when a `QuestState` should exist; do not use a quest flag to store true MainProgression state (validator blocker — see Rule 9).
- **Source:** QUEST_OBJECTIVE_EVENT_SYSTEM §11; `QuestFlagType.cs`, `QuestRewardApplicator.cs:98-130`.

### Rule 8: Anti-Softlock for Critical (Main) Quests

- **Rule:** Main quests must never become impossible. They cannot fail by time, cannot expire by calendar, and cannot be permanently blocked by an item being sold/discarded, a full inventory, an NPC being off-schedule, a missed weather/lunar event, player death, or a cave reset. A main quest must be recoverable without any out-of-game intervention.
- **Enforcement (authored):** `QuestDefinition.CanExpire()` returns false for `Category == Main` (`QuestDefinition.cs:34`), and `QuestDefinitionValidator` raises a blocker (`QUEST_MAIN_EXPIRABLE`) if a main quest declares any expiry rule (`QuestDefinitionValidator.cs:24`).
- **Allowed fallbacks:** re-delivery; protected quest item; item reappears in a safe location; alternate NPC/dialogue/marker; calendar fallback; repeatable condition; retroactive trigger (`QuestRetroactivePolicy` at `QuestConditionType.cs:45`); normalization on load (Rule 12).
- **Constraint:** A hidden quest must never be a hard requirement of the main quest without a reasonable in-game hint (QUEST_OBJECTIVE_EVENT_SYSTEM §3.10, §14).
- **Edge case — quest item protection:** an agricultural/quest item must not be lost to accidental shipping if it is protected (QUEST_OBJECTIVE_EVENT_SYSTEM §20).
- **Source:** QUEST_OBJECTIVE_EVENT_SYSTEM §13-§14; `QuestDefinition.cs:34`, `QuestDefinitionValidator.cs:24`.

### Rule 9: Main Progression and Fonte State Stay Out of Generic Quest State

- **Rule:** Act progression and fragment state belong to `MainProgressionSection`; Fonte function state, Living Water, respec, purification and the final decision belong to `FonteAnyaSection`. The generic `QuestStateSection` must not swallow these.
- **Enforcement:** `QuestDefinitionValidator` raises a blocker (`QUEST_FLAG_MAIN_PROGRESSION_SWALLOW`) if a main quest grants a flag whose id looks like MainProgression state (`QuestDefinitionValidator.cs:36-40`).
- **Constraint:** The three sections — `QuestStateSection`, `MainProgressionSection`, `FonteAnyaSection` — are separate save sections (QUEST_OBJECTIVE_EVENT_SYSTEM §17).
- **Source:** QUEST_OBJECTIVE_EVENT_SYSTEM §17, §24; `QuestDefinitionValidator.cs:36`.

### Rule 10: Spoiler / Visibility Control in the Quest Log

- **Rule:** The Quest Log is presentation, not the source of truth. It may show only authorized knowledge: known title/summary, current objective, numeric progress where applicable, known hint, related known NPC/location, known deadline, discovered temporal condition, and known reward.
- **Must NOT show:** raw internal `QuestStateStatus`, hidden flags, hidden future steps, secret triggers, secret rewards, an undiscovered boss/branch, or an undiscovered lunar condition.
- **Hidden quests:** do not appear in the log until discovered; a hidden quest must carry `SpoilerTier > 0` (validator warning `QUEST_HIDDEN_NO_SPOILER`, `QuestDefinitionValidator.cs:32`).
- **Never reveal early:** the Arquivista do Silêncio, the full nature of the Black Stone, the main-quest endings, cave level 101, exact Mana conditions, or undiscovered boss weaknesses (QUEST_OBJECTIVE_EVENT_SYSTEM §15; QUESTS_MAIN_LORE tone rule "Pastoral above, horror below").
- **Source:** QUEST_OBJECTIVE_EVENT_SYSTEM §15-§16; `QuestDefinition.cs:12`, `QuestDefinitionValidator.cs:32`.

### Rule 11: Expiry and Failure by Category

- **Rule:** Only non-main categories may expire/fail, and only with a clearly communicated deadline.

| Category | Can expire? | Notes |
|---|---|---|
| Main | No | Never fails by time; never expires by calendar (Rule 8) |
| FarmOrder / Notice Board | Yes | Must show the deadline; may repeat; must not block main quest; reward must not break the economy |
| Festival | Yes | May expire when the festival ends; must show date/time when known |
| Side | Conditional | May have a deadline if clearly communicated; an important side quest must warn strongly before permanent failure |
| CaveContract | Conditional | May expire by calendar if it is a contract; must not conflict with cave-run state |

- **Source:** QUEST_OBJECTIVE_EVENT_SYSTEM §3, §13; `QuestDefinition.CanExpire()` at `QuestDefinition.cs:34`.

### Rule 12: Load-Time Normalization (self-healing quest state)

- **Rule:** On load, `QuestStateNormalizer` repairs inconsistent quest states without hiding genuine errors (`Save/QuestStateNormalizer.cs:17`). Recovered issues are flagged but allow loading; unrecovered issues block. Implemented repairs:

| Code | Condition detected | Repair |
|---|---|---|
| `QUEST_NULL_ID` | QuestState with null/empty id | Not recovered (skipped, flagged) |
| `QUEST_DUPLICATE_ID` | Same QuestId appears twice | Second entry kept, flagged recovered |
| `QUEST_TERMINAL_HAS_ACTIVE_STEP` | Terminal quest still has `CurrentStepId` | Active step cleared |
| `QUEST_COMPLETED_NO_DAY` | Completed quest with null `CompletedAtDay` | Set to `StartedAtDay` |
| `QUEST_ACTIVE_NO_STEP` | Active quest with no step and no completed steps | Demoted to `Available` |

- **Constraint:** `HasUnrecoverableIssues` is true only when an issue was not recovered (`QuestStateNormalizer.cs:67`); the loader treats that as a hard failure rather than silently continuing.
- **Source:** `QuestStateNormalizer.cs`; QUEST_OBJECTIVE_EVENT_SYSTEM §14 (normalization on load).

---

## Main Quest Structure (Acts and Cave Gates)

The main quest is "the story of the Fonte relearning how to speak", recovering four fragments of Anya in canonical order **Água → Memória → Vida → Esperança**. Each act ends at a cave gate and unlocks a new Fonte function. Each act turn-in grants **+1 skill point** (4 total) — this is a binding decision (FABLE Q6.2b / FABLE_DECISOES_RESPOSTAS_v1.0 item 1.9, which also raised the skill cap to ~55; reconciled in ADR-0010 / skill_tree_rules.md).

| Act | QuestLevel | Fragment | Cave gate (boss) | Fonte function unlocked |
|---|---|---|---|---|
| Act 1 — A Fonte Adormecida | 10 | Água | Gate 10 (Guardião da Água) | Água Viva (limited) |
| Act 2 — O Arco da Memória | 35 | Memória | Gate 30 (Rimelock Colossus) | respec |
| Act 3 — A Pedra que Sussurra | 65 | Vida | Gate 70 (Draconic Guardian) | purification / advanced healing (limited) |
| Act 4 — A Esperança Enterrada | 90 | Esperança | Gate 100 (Draconic Elder) → level 101 | final decision (Proteger / Selar / Usar) |

```text
Note on "Act 5": the design canon defines FOUR acts (Acts 1-4). The task's "Ato 1-5" maps to
the four acts plus the final-decision beat at level 101 (Vel-Karaum / Cindrathel / Arquivista do
Silencio / final_choice) which closes Act 4. There is no separate fifth act.
```

- **Final choice:** `Proteger` / `Selar` / `Usar` is an irreversible `FinalChoiceBranch` requiring strong confirmation (`MakeFinalChoice` objective; `RequiresStrongConfirmation` on rewards at `QuestRewardDefinition.cs:43`). Choice consequences persist via `ChoiceHistory` (`QuestStateRecord.cs:40`).
- **Anti-softlock reminder:** none of these acts may expire or become impossible (Rule 8).
- **Source:** QUESTS_MAIN_PROGRESSION_REFINEMENT §1-§8; QUEST_CATALOG_DIRECTION §3-§7; QUESTS_MAIN_LORE §17-§18.

---

## Quest State Persistence (Save / Load)

- **Persisted in `QuestStateSection` (per quest):** `QuestId`, `State` (int), `CurrentStepId`, `CompletedStepIds`, `FailedStepIds`, `ObjectiveStates` (objective id + current/required progress + completed/failed/known), `KnownObjectiveIds`, `KnownHints`, `StartedAtDay`, `StartedAtTime`, `CompletedAtDay`, `ExpiresAtDay`, `Tracked`, `Discovered`, `FailureReason`, `ChoiceHistory`, `GrantedRewardIds`, `GrantedFlagIds`, `RepeatInstanceId` (`QuestStateRecord.cs:23-44`).
- **Save DTO discipline:** the quest save record contains only simple types and stable ids — no Unity references — per ADR-0006 / save_rules.md. The file header explicitly states this (`QuestStateRecord.cs:5`).
- **Idempotency carried by save:** `GrantedRewardIds` / `GrantedFlagIds` survive the round-trip and are restored verbatim, which is what makes Rule 6 hold after reload (`QuestService.RestoreFromSaveData` at `QuestService.cs:283-338`).
- **Section separation:** quest generic state, main progression, and Fonte/Anya state are three separate save sections (Rule 9); resolve object references from ids after load, never store live refs.
- **Source:** `QuestStateRecord.cs`, `QuestService.cs:283`; QUEST_OBJECTIVE_EVENT_SYSTEM §17; ADR-0006.

---

## Edge Cases

- **Item sold below threshold:** a `CollectItem` objective recomputes from live inventory; selling the item lowers progress again (not a softlock — by design, `QuestService.cs:153`).
- **Quest item lost / inventory full:** for critical quests, the item must be protected or recoverable (Rule 8); shipping must not consume a protected quest item.
- **Save/load mid-turn-in:** rewards already in `GrantedRewardIds` are not re-applied (Rule 6); a quest already `Completed` returns `AlreadyCompleted` from `TurnIn` (`QuestService.cs:207`).
- **Duplicate accept:** accepting a quest already `Active` is rejected (`QuestService.AcceptQuest` at `:64`); a re-accept of a non-active quest replaces the prior record (`:91`).
- **Terminal quest with stale step after a crash:** repaired on load (Rule 12, `QUEST_TERMINAL_HAS_ACTIVE_STEP`).
- **NPC off-schedule for a main objective:** must show an availability hint when known and never hard-block (Rule 8; QUEST_OBJECTIVE_EVENT_SYSTEM §19).
- **Missed lunar/weather event for a main objective:** must provide a fallback; a rare event may never be an opaque hard requirement of the main quest (QUEST_OBJECTIVE_EVENT_SYSTEM §18).

---

## What Persists in the Save and What Tests Must Cover

Aligned with `.claude/rules/testing-quality-gate.md` → "Quest test expectations". Any spec that changes quest behavior must test, or explicitly justify the lack of a test for:

```text
condition evaluation (conditions gate, never advance — Rule 4);
trigger handling and deduplication policy (Rule 4);
reward idempotency: reward not applied twice after reload (Rule 6);
quest flag set/clear, including idempotent grant (Rule 7);
objective progress (inventory-derived clamp, event-mark, count-based — Rule 5);
spoiler visibility: hidden quests absent from log until discovered (Rule 10);
anti-softlock fallback for critical (main) quests: cannot expire/fail by time (Rule 8);
save/load of quest state: round-trip of QuestStateRecord incl. GrantedRewardIds/GrantedFlagIds;
load-time normalization repairs (Rule 12);
no Unity references in the quest save DTO (Rule, save section).
```

- **Preferred test type:** EditMode tests for the pure/deterministic pieces (`QuestStateNormalizer`, `QuestRewardApplicator`, `QuestDefinitionValidator`, `QuestService` with stub adapters), per testing-quality-gate.md.
- **Play Mode / human scenario:** required for quest-giver interaction, the Quest Log/HUD tracker, board/contract acceptance, and NPC dialogue turn-in (scene + UI + modal dependence).

---

## Open Questions

- Will `QuestDefinitionSO` be a single asset per quest or composed of separate step/objective assets? (QUEST_OBJECTIVE_EVENT_SYSTEM §30 — pendência aberta.)
- Will FarmOrders be a `QuestCategory` or a separate board system with an adapter? (§30 — currently modeled as `FarmOrder` category.)
- Will hidden quests appear as `HiddenCompleted` after completion? (§30.)
- Reward XP/gold values: scaled by `QuestLevel` via `Base(tier) × (1 + 0.06 × QuestLevel)` (QUEST_CATALOG §2) — concrete numbers live in BALANCE_CURVES §7 and are **proposta a calibrar** here, not duplicated.
- Daily board limit (3 contracts/day) and tracked-quest count (1) are design defaults (Q6.1) — confirm against the runtime board implementation when the notice board ships.

---

## Sources

**Design directions (canonical):**
- `docs/design/gameplay/quests/QUEST_OBJECTIVE_EVENT_SYSTEM_DIRECTION.md` — generic system model, states, conditions, triggers, rewards, flags, anti-softlock, anti-spoiler, save sections.
- `docs/design/gameplay/quests/QUEST_CATALOG_DIRECTION_v1.0.md` — five sources, reward scaling, acts, side chains, board/contract/secret rules.
- `docs/design/gameplay/quests/QUESTS_MAIN_LORE_DIRECTION.md` — main-quest lore, four acts, fragments, endings, tone.
- `docs/design/gameplay/quests/QUESTS_MAIN_PROGRESSION_REFINEMENT_DIRECTION.md` — act/fragment/gate mapping and Fonte progression.

**Binding decisions:**
- `docs/design/FABLE_QUESTIONARIO_DECISOES_v1.0.md` Q6.1 (five sources), Q6.2 (reward tiers, +1 skill point/act), Q6.5 (quest UI).
- `docs/design/FABLE_DECISOES_RESPOSTAS_v1.0.md` item 1.9 (skill cap raised to "50 base + 1/act" — F42 amended; reconciled in ADR-0010).

**Code (runtime, file:line for key values):**
- `Assets/_Game/Scripts/Quests/QuestCategoryType.cs` — `QuestCategory` (`:5`), `QuestStateStatus` (`:20`), `CompletionMode` (`:35`), `QuestObjectiveType` (`:46`), `QuestEventName` (`:62`).
- `Assets/_Game/Scripts/Quests/QuestDefinition.cs` — authored contract; `CanExpire()` (`:34`).
- `Assets/_Game/Scripts/Quests/QuestState.cs` — runtime state; `IsTerminal()` (`:22`).
- `Assets/_Game/Scripts/Quests/QuestDefinitionValidator.cs` — main-expirable blocker (`:24`), main-progression-swallow blocker (`:36`), hidden-spoiler warning (`:32`).
- `Assets/_Game/Scripts/Quests/Conditions/QuestConditionType.cs` — condition categories, operators, dedup policy (`:36`), retroactive policy (`:45`).
- `Assets/_Game/Scripts/Quests/Triggers/QuestTriggerType.cs` — trigger domains.
- `Assets/_Game/Scripts/Quests/Rewards/QuestRewardDefinition.cs` — reward types (`:5`), idempotency policy (`:18`), future/Fonte-adapter guards (`:49`, `:55`).
- `Assets/_Game/Scripts/Quests/Rewards/QuestRewardApplicator.cs` — idempotency skip (`:43`), flag grant idempotency (`:105`), Fonte-adapter guard (`:47`).
- `Assets/_Game/Scripts/Quests/Runtime/QuestService.cs` — accept (`:55`), objective progress (`:133`), mark complete (`:174`), turn-in idempotency (`:213`), restore from save (`:283`).
- `Assets/_Game/Scripts/Quests/Save/QuestStateRecord.cs` — quest save DTO (`:23`).
- `Assets/_Game/Scripts/Quests/Save/QuestStateNormalizer.cs` — load-time repairs (`:17`).
- `Assets/_Game/Scripts/Quests/Flags/QuestFlagType.cs` — flag type/scope/visibility.

---

## Cross-References

**Sibling game rules:**
- [save_rules.md](save_rules.md) — save DTO contracts, no Unity refs, versioning/migration (quest save section conforms to these).
- [event_rules.md](event_rules.md) — GameEventBus contract used by all quest progress events.
- [skill_tree_rules.md](skill_tree_rules.md) — skill points awarded per main act (+1/act) and respec unlocked by the Memória fragment; cap reconciliation.
- [inventory_equipment_rules.md](inventory_equipment_rules.md) — item ids and inventory access used by `CollectItem`/`DeliverItem`/reward Item grants.
- [combat_rules.md](combat_rules.md) — enemy/boss ids and cave gates referenced by `DefeatEnemy`/`DefeatBoss`/`ReachCaveDepth` objectives.
- [cave_rules.md](cave_rules.md) — cave stable-run seed/snapshot contract that cave-contract and secret quests must respect.
- `fonte_rules.md` (planned) — Fonte function unlocks, Living Water charges, final-decision state (adapter-gated rewards route here, not through generic quest rewards — Rule 6).
- `npc_rules.md` (planned) — NPC identity, schedules, dialogue and unique-service unlocks that source side chains and gate `TalkToNpc` objectives.

**Related ADRs:**
- [ADR-0006: Save Data Contracts — Simple DTOs](../decisions/ADR-0006-save-data-contracts-simple-dtos.md) — quest save record discipline.
- [ADR-0007: Event Bus Gameplay Communication](../decisions/ADR-0007-event-bus-gameplay-communication.md) — quest progress/reward events.
- [ADR-0010: Fable Skill and Inventory Rules Reconciliation](../decisions/ADR-0010-fable-skill-and-inventory-rules-reconciliation.md) — +1 skill point per act and skill cap reconciliation (this rule must not contradict it).

---

*Created: 2026-06-13 (referenced by fable_10 / fable_34 / fable_35 / fable_36)*
*Source: QUEST_OBJECTIVE_EVENT_SYSTEM / QUEST_CATALOG / QUESTS_MAIN_LORE / QUESTS_MAIN_PROGRESSION directions; FABLE Q6 decisions; quest runtime code.*
