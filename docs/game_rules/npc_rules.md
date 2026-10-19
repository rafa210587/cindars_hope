---
doc_type: game_rule
status: accepted
domain: npc-social-gameplay
source_adrs:
  - ADR-0007
  - ADR-0006
  - ADR-0010
source_documents:
  - docs/design/gameplay/city/CITY_NPC_ROSTER_SERVICES_DIRECTION_v1.1.md
  - docs/design/gameplay/city/CITY_LAYOUT_BUILDINGS_SCHEDULE_DIRECTION.md
  - docs/design/gameplay/city/NPC_GIFT_TASTE_MATRIX_v1.0.md
  - docs/design/gameplay/social/SOCIAL_RELATIONSHIP_ROMANCE_DIRECTION.md
  - docs/design/FABLE_DECISOES_RESPOSTAS_v3.0.md
last_reviewed: 2026-06-13
---

# NPC Rules

## Purpose

Defines the canonical behavior of town NPCs: their static data contract, daily
schedule/anchors, dialogue condition and pool layers, friendship state and the
gain-per-source model, gifts with per-NPC taste (loved/liked/neutral/disliked/hated),
per-NPC unique services, and the gating boundary toward romance/marriage.

This rule is the canonical *what is true now* for the NPC/social domain. It states
which parts are live runtime, which parts are an authored data contract whose runtime
wiring is still **deferred debt**, and which parts are reserved hooks owned by sibling
directions. It does not duplicate combat, economy/shop pricing, save schema, or
romance runtime detail — those are cross-referenced (see "Cross-References").

### Scope

- In scope: NPC static definition; town roster identity and IDs; schedule/anchor
  resolution; dialogue layering; friendship value and gift reaction model; per-NPC
  services; the eligibility/gating fields that romance/marriage will later consume.
- Out of scope (cross-referenced only): shop transaction/pricing math, quest engine
  internals, the full romance/marriage/poly *runtime*, festival runtime, pet/companion
  bond. Those are deferred and live in their own directions/specs.

---

## Definitions and Terms

- **NPC definition** — the static, pure-C# data contract for one NPC
  (`Assets/_Game/Scripts/NPC/NpcDefinition.cs:16`). Static identity only; mutable
  social state (friendship points, questline progress, service-unlock flags, gift-day
  markers) is persisted separately (see comment at `NpcDefinition.cs:5-7`).
- **NpcDataSO** — the Unity ScriptableObject the runtime actually binds in scenes
  (`Assets/_Game/Scripts/NPC/NpcDataSO.cs:13`). It carries `NpcId`, display lines,
  `DialogueTree`, `ShopId`, `DefaultSceneId`, `DefaultPosition` and movement mode.
  `NpcDefinition` and `NpcDataSO` intentionally coexist.
- **Stable NpcId** — the canonical lowercase `npc_*` identifier (e.g. `npc_corvus`,
  `npc_thalindra`). It is the join key for dialogue, schedule, gift taste, services and
  save state. There are **23 roster NPCs** in the canonical city roster.
- **Anchor** — a named scene position used to resolve where an NPC stands at a given
  schedule tick (`NpcScheduleAnchor`, `Assets/_Game/Scripts/NPC/Schedule/NpcScheduleAnchor.cs:10`).
- **Time block** — a coarse period of the day used by the schedule
  (`NpcTimeBlock`: `Default/Morning/Midday/Evening/Night`,
  `Assets/_Game/Scripts/NPC/Schedule/NpcScheduleBlock.cs:5`).
- **Friendship** — the base social bond value with an NPC. It unlocks dialogue/scenes,
  improves gift response, enables visits, and opens romance eligibility for eligible
  NPCs — but is never required to finish the game.
- **Gift taste level** — one of five per-NPC reactions to a gifted item:
  `loved / liked / neutral / disliked / hated` (decision 2.4 CUSTOM).
- **Service** — the unique function an NPC provides (shop, repair, licenses, alchemy,
  guild contracts, etc.), driven by the NPC's functional gameplay class.

---

## Canonical Rules

### Rule 1: NPC Identity and Data Contract

- **Rule:** Every NPC is identified by a stable lowercase `npc_*` `NpcId`. The static
  contract is `NpcDefinition` and exposes: identity (`NpcId`, `DisplayName`, `Gender`,
  `AgeBand`, race/subrace), functional class (`GameplayClassPrimary/Secondary`), role
  and service tags, religion profile, relationship status + romance/marriage eligibility,
  farm-visit flags, stats, status resistances/weaknesses, schedule/home/work location IDs,
  questline id, gift preferences, dialogue set id and portrait id
  (`NpcDefinition.cs:16-51`).
- **Constraint — functional classes only:** NPC classes are the project's functional
  classes (`FunctionalGameplayClass`: Plantador, Colhedor, Pescador, Lenhador, Minerador,
  Artesao, Explorador, Construtor, Tratador, Comerciante, Escriba, Curandeiro, Guardiao,
  Combatente, Pesquisador, Musico, Alquimista — `FunctionalGameplayClass.cs:7-27`).
  D&D mechanical class names (Cleric, Paladin, Fighter, Wizard, Rogue, Bard, Druid,
  Ranger, Warlock, Sorcerer, Monk, Barbarian) are **forbidden** as gameplay classes and
  are rejected by the validator with code `NPC_DND_CLASS_IN_TAGS`
  (`NpcDefinitionValidator.cs:17-21,35-39`).
- **Constraint — NPC stats taxonomy:** NPC stats are HP, MP, Stamina, Forca,
  Constituicao, Destreza, Inteligencia, Vontade, Carisma (`NpcStats.cs:5-17`). `Folego`/
  `Breath`/`BR` are intentionally **absent** for NPCs (same canon as player/companions).
- **Source:** CITY_NPC_ROSTER_SERVICES_DIRECTION_v1.1 §1-2; code as cited.

### Rule 2: Town Roster and Fixed Couples

- **Rule:** The canonical city roster is 23 NPCs with fixed IDs, race/subrace, primary +
  secondary functional class, religion and romance eligibility (master table in
  CITY_NPC_ROSTER_SERVICES_DIRECTION_v1.1 §"PARTE C").
- **Fixed couples (not romanceable by the player):**

  | Couple | NpcIds |
  |---|---|
  | Nimble Galhobaixo + Mirela dos Laços | `npc_nimble` + `npc_mirela` |
  | Gruta Panela-Funda + Orlan Pouso-Curto | `npc_gruta` + `npc_orlan` |
  | Mara Vellum + Tovin Mãos-de-Selo | `npc_mara` + `npc_tovin` |

- **Constraint — married integrity:** an NPC with `RelationshipStatus.MarriedToNpc`
  must declare a `SpouseNpcId` and must **not** be romance eligible. The validator
  blocks both violations (`NPC_MARRIED_NO_SPOUSE`, `NPC_MARRIED_ROMANCE_ELIGIBLE` —
  `NpcDefinitionValidator.cs:42-48`).
- **Constraint — duplicate IDs:** roster validation blocks duplicate `NpcId`
  (`NPC_DUPLICATE_ID`, `NpcDefinitionValidator.cs:63-77`).
- **Source:** roster direction §5-6, §"PARTE C"; validator code.

### Rule 3: Schedule, Time Blocks and Scene Anchors

- **Rule:** NPC daily presence is driven by a schedule. A schedule
  (`NpcScheduleProfile`) is a list of `NpcScheduleBlock`, each binding a `NpcTimeBlock`
  to an `AnchorId`, `SceneId`, `ActivityLabel` and a `CanInteract` flag
  (`NpcScheduleBlock.cs:13-22`). Time blocks are `Morning/Midday/Evening/Night` plus
  `Default`.
- **Anchor resolution:** an NPC resolves to a named anchor by id, with convention
  `npc_<npcId>_home`; if the anchor is not registered, it falls back to
  `NpcDataSO.DefaultPosition` (`NpcScheduleService.cs:158-165`). Anchors are placed in
  the scene via `NpcScheduleAnchor` components carrying `AnchorId` + `NpcId`
  (`NpcScheduleAnchor.cs:12-18`).
- **Current runtime limitation (TIME_BLOCK_DEBT):** the live `NpcScheduleService`
  advances the schedule only on `DayStartedEvent` (once per day) and resolves every NPC
  to its **home** position; intra-day period transitions (Morning→Midday→Evening→Night)
  are **deferred** until a `TimeBlockChangedEvent` exists
  (`NpcScheduleService.cs:8-13,115-120`). The per-block agenda in the city direction
  (§19-20, periods 06:00–00:00) is the **authored target**, not yet the live behavior.
- **Constraint — no offscreen simulation:** NPCs are not simulated continuously
  offscreen; schedule may teleport an NPC to the correct anchor when the scene loads
  (city direction §23). Schedule resolution must not perform global scene searches; it
  uses registered anchors/controllers (see Rule 9 and event_rules.md).
- **Source:** CITY_LAYOUT_BUILDINGS_SCHEDULE_DIRECTION §19-26; schedule code as cited.

### Rule 4: Dialogue — Layers, Pools and Conditions

- **Rule:** NPC dialogue is a `DialogueTreeSO` of `DialogueNode`s. A node has `Text`, a
  list of `DialogueChoice`s, and an optional `RandomLinePool` from which a line is picked
  at random when the node is shown (`DialogueNode.cs:8-15`,
  `NpcShopController.cs:439-443`). The expanded town content authors a standard
  **13-node tree per NPC** with greeting hub (random pool), role, town context, service,
  advice, two rumor branches and a goodbye, all returning to the hub
  (`TownNpcDialogueLibrary.cs:5-33`; `NodesPerNpc = 13`).
- **Choice actions:** a choice may branch (`NextNodeId`) or trigger an action
  (`DialogueActionType`: `None/OpenShop/OfferQuest/CloseDialogue`, `DialogueChoice.cs:6-21`).
  `OfferQuest` carries the `questId` in `ActionPayload`.
- **Dialogue condition pools (authored target, F28):** dialogue should vary by social
  state, time of day, season, current moon (Alihana/Senya/Nyx), weather, location,
  personal quest, discovered gift taste, the NPC's religion, romance/marriage/poly
  status, active pet/companion, cave progress and local reputation
  (SOCIAL_RELATIONSHIP_ROMANCE_DIRECTION §9). The conceptual layer set is Greeting,
  DailySmallTalk, ScheduleContext, ServiceContext, GiftReaction, RelationshipMilestone,
  PersonalQuest, RomanceContext, MarriageContext, FestivalContext, FarmVisitContext,
  DeityReaction (SOCIAL §9). Live content currently realizes the schedule-independent
  layers (greeting/role/town/service/advice/rumor/goodbye); the conditional layers are
  the F28 target.
- **Source:** SOCIAL §9; dialogue code as cited.

### Rule 5: Friendship State and Gain by Source (F26)

- **Rule:** Friendship is the base social bond value per NPC. It serves to unlock
  dialogue/scenes, open personal requests, improve gift response, permit farm visits, and
  open romance eligibility for eligible NPCs (SOCIAL §4.2). Conceptual social states
  progress Unknown → Known → Acquaintance → Friendly → CloseFriend → Trusted, then
  romance states for eligible NPCs (SOCIAL §5).
- **Friendship can be gained from (sources):** daily conversation, a relevant gift,
  a gift on birthday/festival, personal quest completion, helping the NPC's work, a
  dialogue choice, a respectful religious/cultural choice, a farm or cave event,
  protection/support in an event, an accepted invitation, festival participation,
  spouse/partner events, companion events (SOCIAL §8).
- **Constraint — friendship must not:** substitute reputation, deal direct damage, grant
  strong permanent stats, be required to finish the game, or be farmable infinitely on the
  same day (SOCIAL §4.2). The daily gift cap (Rule 6) enforces the anti-spam part.
- **Constraint — floor clamp:** friendship points never drop below 0 (clamp at level 0 /
  Unknown), even when a hated gift applies a negative delta
  (NPC_GIFT_TASTE_MATRIX_v1.0 §2.1).
- **Status:** the friendship runtime is owned by fable_26. The `NpcGiftPreferences`
  struct and `lastGiftDay` marker exist as the data contract, but the friendship value,
  gain wiring and save section are **deferred debt** at the time of this rule
  (FABLE_DECISOES_RESPOSTAS_v3.0 line 141; NPC_GIFT_TASTE_MATRIX_v1.0 §6). Numeric
  thresholds per state are "proposta a calibrar" (SOCIAL §28).
- **Source:** SOCIAL §4-5,8,28; FABLE v3.0 decision 2.4 / line 141.

### Rule 6: Gifts and Per-NPC Taste (decision 2.4 CUSTOM)

- **Rule:** Decision 2.4 (A + CUSTOM) discards the original fable_26 model of a flat
  "+3 per gift". Each NPC reacts to a gift at one of five taste levels; `loved` gives more
  friendship, and `hated` **reduces** friendship — specific per NPC
  (FABLE_DECISOES_RESPOSTAS_v3.0 line 38; NPC_GIFT_TASTE_MATRIX_v1.0 §0).
- **Proposed friendship deltas per level (proposta a calibrar — NPC_GIFT_TASTE_MATRIX_v1.0 §2):**

  | Taste level | Friendship delta | Direction | Notes |
  |---|---|---|---|
  | `loved` | **+12** | high positive | rare/personal item; few per NPC (1–3) |
  | `liked` | **+6** | medium positive | by tag (the NPC's everyday tastes) |
  | `neutral` | **+2** | minimal positive | default: any `Giftable` not classified |
  | `disliked` | **-2** | small negative | tag the NPC dislikes |
  | `hated` | **-6** | high negative | tag/item the NPC repudiates (reduces friendship) |

  These deltas are an explicit **proposal to calibrate**, not validated balance.

- **Constraint — classification precedence:** most-negative-wins, from negative to
  positive: `hated` > `disliked` > `loved` (by item id) > `liked` (by tag) > `neutral`
  (default). Personal rejection beats a generic positive tag, aligning with SOCIAL §8.2
  ("a hated high-quality item is still hated") — NPC_GIFT_TASTE_MATRIX_v1.0 §2.1.
- **Constraint — Giftable gate:** an item is presentable only if it carries
  `ItemTag.Giftable` (`Assets/_Game/Scripts/Items/ItemTag.cs:14`, value `1L << 6`). An
  item without the tag is silently refused with no friendship change. (Today the tag is
  assigned to **zero items** — see Open Questions / debt.)
- **Constraint — quality does not flip sign:** item quality (Normal/Prata/Ouro) is a
  *future multiplier* on positive gifts only; it never turns a disliked/hated item
  positive. fable_26 v3 treats it as +0% for now (matrix §2.1; SOCIAL §8.2).
- **Constraint — daily limit:** default 1 relevant gift per NPC per day
  (`NpcGiftPreferences.DailyGiftLimit = 1`, `NpcDefinition.cs:13`). A gift beyond the cap
  is refused friendly, with no gain/loss, and does not consume the item; the per-NPC
  `lastGiftDay` save marker is the cap key. The 2/week refinement is future
  (matrix §2.2; SOCIAL §8.3).
- **Data contract (reuse — do not create a parallel struct):** `NpcGiftPreferences`
  already holds `LikedItemTags`, `LovedItemIds`, `DislikedItemTags`, `DailyGiftLimit`
  (`NpcDefinition.cs:8-14`). The matrix directs **extending** it with `NeutralItemTags`
  (optional; default is neutral) and `HatedItemTags`, reusing the existing fields and the
  `Giftable` tag (matrix §1). The full per-NPC loved/liked/disliked/hated table is in
  NPC_GIFT_TASTE_MATRIX_v1.0 §4-5 (23 NPCs).
- **Status:** the gift→friendship runtime (read taste, apply delta, enforce cap, persist)
  is **deferred debt** — the struct is dead code, the tag is on no item, and there is no
  read/write/generation/validation yet (matrix §6).
- **Source:** FABLE v3.0 decision 2.4; NPC_GIFT_TASTE_MATRIX_v1.0 §0-6; SOCIAL §8;
  code as cited.

### Rule 7: Per-NPC Services (F25)

- **Rule:** Each NPC provides a service derived from its functional class; services are
  unique per NPC and bound to buildings in the city layout. Canonical service→building→NPC
  bindings (city direction §11):

  | NpcId | Service | Building |
  |---|---|---|
  | `npc_corvus` | blessings, oaths, lore (Kanthor temple) | `bld_kanthor_temple` |
  | `npc_mara` / `npc_tovin` | licenses, contracts, reputation | `bld_town_hall` |
  | `npc_sylveth` | seeds, fertilizer, farm calendar | `bld_seed_shop` |
  | `npc_renko` | general goods, buy/sell | `bld_general_store` |
  | `npc_brumdar` | tools, weapons, repair | `bld_blacksmith` |
  | `npc_ozzra` | potions, fertilizers, reagents | `bld_alchemy` |
  | `npc_gruta` / `npc_orlan` | food, rumor, lodging | `bld_tavern_inn` |
  | `npc_zrix` (+ Dagna partial) | maps, cave, contracts | `bld_roads_guild` |
  | `npc_thalindra` | research, lore, translation | `bld_archive` |
  | `npc_mirela` | bags, clothes, accessories | `bld_tailor` |
  | `npc_eiran` | animals, pets, feed | `bld_ranch` |
  | `npc_savra` | herbs, antidotes, pest control | `bld_herbalist` |
  | `npc_yael` | rare items, Nyx, secrets (night shop) | `bld_night_shop` |

- **Live runtime:** a shop NPC is an `NpcShopController` (`IInteractable`) that runs an
  opening line → dialogue/menu → Buy/Sell flow against a `ShopDataSO`/`ShopManager`, then
  a closing line (`NpcShopController.cs:21-46,209-233`). Quest-giver service is folded in
  the same flow (e.g. Thalindra's quest offer/turn-in branch,
  `NpcShopController.cs:256-305`). Shop NPCs also expose their authored `DialogueTreeSO`
  "Conversar" branch alongside buy/sell (`NpcShopController.cs:357-373`).
- **Constraint — service availability follows schedule:** a service is gated by the
  NPC's `CanInteract` block (Rule 3); the city direction notes essential services must
  have a fallback and must not become permanently unavailable due to a social event
  (SOCIAL §24). Closed-shop hours behavior is an open city-spec question (city direction
  §"PARTE L").
- **Source:** roster direction (services), city direction §11; shop code as cited.
  Pricing/stock/transaction math is **not** defined here (see economy cross-ref).

### Rule 8: Romance / Marriage Gating (cross-ref, not implemented)

- **Rule:** Romance eligibility is declared per NPC, never inferred. The live enum
  `RelationshipStatus` carries `RomanceEligibleAnyPlayerGender`, `UnavailableForRomance`,
  `MarriedToNpc`, `TooYoungOrNarrativelyBlocked`, `LateRomanceEligible`
  (`NpcRelationshipStatus.cs:3-12`), plus `RomanceEligibility`/`MarriageEligibility`
  fields (`NpcRelationshipStatus.cs:14-16`, used on `NpcDefinition.cs:32-33`).
- **Constraints (gating):**
  - Same-gender romance is allowed by default for NPCs marked
    `RomanceEligibleAnyPlayerGender`; the player's gender never blocks it (SOCIAL §11.2).
  - Consensual polyamory is allowed up to **3 total partners**; each NPC may accept or
    refuse poly per profile; the cave baseline remains **1 active companion per run**
    regardless of partner count (SOCIAL §5,11.3,13).
  - `TooYoungOrNarrativelyBlocked` NPCs get no romantic route, flirt, romantic gift or
    dating event; the validator blocks them from carrying `MarriageEligibility`
    (`NPC_TOO_YOUNG_MARRIAGE`, `NpcDefinitionValidator.cs:51-53`).
  - Friendship/romance/marriage must stay optional and must not become a power
    requirement or a superior source of gold/stats (SOCIAL §11).
- **Status:** the romance/marriage/poly **runtime** (Affection value, dating/committed/
  married/polycule states, ceremonies, partner-helper/partner-companion) is explicitly
  **future, not implemented now** (SOCIAL §1,27). This rule only fixes the gating fields
  that exist today; romantic gift tags
  (`Romantic/Marriage/PolyCommitment/Forbidden`) are out of scope for the friendship gift
  matrix (matrix §"7. Fora de escopo"; SOCIAL §8.1).
- **Source:** SOCIAL §6,11,12,13; `NpcRelationshipStatus.cs`; validator code.

### Rule 9: Religion, Farm Visits and Architectural Constraints

- **Rule — religion:** each NPC declares a religion profile: `DeityWorshipped`,
  `DeitySympathy`, `DeityDisliked`, `ParticipatesInKanthorFestivals`,
  `HasPrivateAnyaSympathy` (`NpcReligionProfile.cs:7-16`). Religion affects dialogue,
  reputation, gifts (deity gift tags), festivals and altar/Fonte reactions
  (roster direction §3).
- **Constraint — Anya cult:** the public temple is Kanthor's. Anya has **no active
  public cult/altar/service**; `HasPrivateAnyaSympathy` is allowed as a narrative tag
  only. Any active Anya temple/altar service tag is blocked by the validator
  (`NPC_ANYA_ACTIVE_TEMPLE`, `NpcDefinitionValidator.cs:55-58`; canon note
  `NpcReligionProfile.cs:6,12,15`).
- **Rule — farm visits:** an NPC may visit the farm only if `CanVisitFarm` is set, and may
  relocate after marriage only if `CanMoveToFarmAfterMarriage` is set
  (`NpcDefinition.cs:34-36`). Visit *runtime* (anchors, partner helper) is reserved/
  deferred (SOCIAL §15,17). Helper actions, if implemented, use a global budget and never
  automate the whole farm (SOCIAL §14).
- **Constraint — architecture:** NPC/social systems communicate via `GameEventBus`
  (e.g. `NpcInteractionStartedEvent`/`...EndedEvent`/`QuestGiverInteractedEvent`,
  `NpcShopController.cs:219,304,731`) and resolve references via bootstrap/serialized
  refs, never via `GameObject.Find`/`FindObjectOfType` in runtime (see event_rules.md and
  ADR-0007). Schedule rebinds via registered controllers/anchors and
  GameBootstrap-adopted references (`NpcShopController.cs:128-152`).
- **Source:** roster direction §3; city direction §12-13; ADR-0007; code as cited.

---

## What Persists in Save

The static `NpcDefinition` is data, not save state. Mutable NPC/social state is persisted
separately and must follow ADR-0006 (simple types + stable IDs only; no Unity refs).
The conceptual social save (SOCIAL §22) and the contracts above imply persisting:

```text
NpcId (stable string key)
HasMet (per shop/dialogue controller — NpcShopController.RestoreState, .cs:734)
Friendship value (+ social state)  [deferred — fable_26]
lastGiftDay per NPC (daily gift cap key)  [deferred — fable_26]
GiftsGivenThisWeek (2/week refinement)  [future]
discovered loved/hated gift tags  [future]
questline progress / completed-quest ids  [quest system]
service-unlock flags
romance/marriage/poly state (booleans + PolyculeId + PartnerSlotIndex)  [future]
last talk/visit day; farm-visit state  [future]
```

Must **not** persist (ADR-0006 / SOCIAL §22): Unity object references, GameObject,
Transform, ScriptableObject directly, resolved dialogue text, computed social price, or
transient pathfinding state. On load: load registries first, validate `NpcId`s, validate
the 3-partner poly limit, then apply state and resolve scene/schedule (SOCIAL §22 load
order).

---

## What Tests Must Cover

Per `.claude/rules/testing-quality-gate.md`, the deterministic NPC/social logic below is
EditMode-testable and must be covered (or have explicit justified residual risk) by any
spec that implements it:

- **Gift taste classification** — precedence `hated > disliked > loved(id) > liked(tag) >
  neutral` resolves to the correct level for representative items (Rule 6).
- **Gift delta application** — each level applies the proposed delta; friendship clamps at
  floor 0 on a hated gift (Rules 5–6).
- **Daily gift cap** — a second gift the same day is refused without gain/loss and without
  consuming the item; `lastGiftDay` updates correctly (Rule 6).
- **Giftable gate** — an item lacking `ItemTag.Giftable` is refused silently (Rule 6).
- **Roster validation** — duplicate `NpcId`, D&D class in tags, married-without-spouse,
  married-and-romance-eligible, too-young-with-marriage-eligibility, and active-Anya-temple
  tag are all flagged (mirrors `NpcDefinitionValidator`).
- **Schedule resolution** — anchor id convention `npc_<id>_home` resolves, with fallback
  to `DefaultPosition` when the anchor is unregistered (Rule 3).
- **Save round-trip** — friendship/gift-day/social booleans survive save→load; no Unity
  refs in the DTO; invalid `NpcId` falls back safely (Save section).

Play Mode / human scenario is required for the parts that depend on scene objects: shop
open/close + Esc behavior, dialogue tree navigation, schedule teleport on day start, and
service interaction (testing-quality-gate "Mandatory Play Mode or manual scenario").

---

## Edge Cases

- **Unregistered home anchor:** NPC falls back to `NpcDataSO.DefaultPosition`
  (`NpcScheduleService.cs:161-165`); a missing anchor is not an error.
- **Intra-day transitions:** until `TimeBlockChangedEvent` exists, all NPCs resolve to
  home at day start only (TIME_BLOCK_DEBT, `NpcScheduleService.cs:8-13`); per-block agenda
  is authored but not live.
- **Gift over the daily cap:** friendly refusal, no friendship change, item not consumed
  (Rule 6).
- **Hated gift at friendship 0:** delta is negative but value clamps at 0 (Rule 5).
- **Item not Giftable:** silent refusal, no gain/loss (Rule 6).
- **Married/fixed-couple NPC targeted for romance:** blocked by data validation; no
  romantic route, gift tag or dating event is offered (Rules 2, 8).
- **Essential service vs. social event:** a social/festival event must not leave an
  essential service permanently unavailable; provide a fallback (SOCIAL §24).
- **NPC blocked physically by player / closed-shop entry:** open city-spec questions, not
  yet fixed by this rule (city direction §"PARTE L").

---

## Open Questions

- Final friendship/trust/affection numeric thresholds per social state (SOCIAL §28).
- Confirmed gift deltas — the matrix values (+12/+6/+2/-2/-6) are a proposal to calibrate
  (matrix §2).
- Which NPCs are `PolyCompatible` vs. `PolyBlocked`, `PartnerCompanionEligible`,
  `PartnerFarmHelperEligible` (SOCIAL §28).
- Whether `ItemTag.Giftable` and the `gift_*` tags get materialized in the item catalog
  (ITEM_CATALOG/A4) — today Giftable is on zero items and the gift taste runtime is dead
  code (matrix §1,6).
- Whether friendship affects shop prices via reputation/economy or via a separate social
  rule (SOCIAL §28).
- Closed-shop entry, festival scene swap, and whether NPCs physically block the player
  (city direction §"PARTE L").

---

## Sources

- **Design directions:**
  - `docs/design/gameplay/city/CITY_NPC_ROSTER_SERVICES_DIRECTION_v1.1.md` — roster IDs,
    classes, religion, eligibility, services, fixed couples.
  - `docs/design/gameplay/city/CITY_LAYOUT_BUILDINGS_SCHEDULE_DIRECTION.md` — buildings,
    residences/beds, schedule periods/anchors, spawn points.
  - `docs/design/gameplay/city/NPC_GIFT_TASTE_MATRIX_v1.0.md` — five taste levels, deltas,
    precedence, daily cap, per-NPC matrix, gift-runtime debt.
  - `docs/design/gameplay/social/SOCIAL_RELATIONSHIP_ROMANCE_DIRECTION.md` — friendship/
    trust/affection model, social states, romance/marriage/poly gating, save schema,
    anti-exploit.
- **Binding decisions:** `docs/design/FABLE_DECISOES_RESPOSTAS_v3.0.md` — decision 2.4
  (A + CUSTOM) gift taste; line 141 (NpcGiftPreferences dead-code reuse note).
- **Code (file:line):** `NpcDefinition.cs:8-51`, `NpcDataSO.cs:13`,
  `NpcRelationshipStatus.cs:3-16`, `NpcReligionProfile.cs:7-16`, `NpcStats.cs:5-17`,
  `FunctionalGameplayClass.cs:7-27`, `NpcDefinitionValidator.cs:17-58`,
  `Schedule/NpcScheduleBlock.cs:5-22`, `Schedule/NpcScheduleAnchor.cs:12-18`,
  `Schedule/NpcScheduleService.cs:8-13,115-165`, `NpcShopController.cs:21-46,209-373,734`,
  `DialogueNode.cs:8-15`, `DialogueChoice.cs:6-21`, `TownNpcDialogueLibrary.cs:5-33`,
  `Items/ItemTag.cs:14`.

> Per docs governance, `docs/amendments/` is historical record only and is intentionally
> **not** cited as canon here.

---

## Cross-References

### Related ADRs

- [ADR-0007: Event Bus Gameplay Communication](../decisions/ADR-0007-event-bus-gameplay-communication.md)
  — NPC interaction, quest-giver and schedule events flow through `GameEventBus`; no
  runtime global search (Rules 3, 9).
- [ADR-0006: Save Data Contracts (Simple DTOs)](../decisions/ADR-0006-save-data-contracts-simple-dtos.md)
  — NPC/social save state uses simple types + stable IDs, no Unity refs (Save section).
- [ADR-0010: FABLE Skill and Inventory Rules Reconciliation](../decisions/ADR-0010-fable-skill-and-inventory-rules-reconciliation.md)
  — precedent that an accepted rule reconciled to live code + v3.0 decisions; respec is
  gated by the Fonte de Anya, which is the same Fonte that NPC lore/quests reference
  (consistency anchor; this rule does not restate skill/inventory numbers).

### Sibling Game Rules

> The following sibling rules are referenced rather than duplicated. Where a sibling rule
> does not yet exist as a file, the canonical direction is the interim source.

- **city_rules** (planned) — city scene, buildings, beds, pathfinding, festivals; for
  building/anchor placement and city-wide layout. Interim source:
  `CITY_LAYOUT_BUILDINGS_SCHEDULE_DIRECTION.md`. (NPC schedule anchors live here, Rule 3.)
- **quest_rules** (planned) — quest condition/trigger/reward/idempotency engine; personal
  quests are *offered/turned in through* NPC dialogue (Rule 4, `DialogueActionType.OfferQuest`,
  `QuestGiverInteractedEvent`) but the quest logic itself is owned there.
- **economy_rules** (planned) — shop pricing, stock refresh, sell points, anti-arbitrage;
  NPC services *open* the shop UI (Rule 7) but all transaction math is owned there.
  Interim source: `ECONOMY_PRICING_STOCK_REFRESH_DIRECTION.md`.
- [combat_rules.md](combat_rules.md) — NPC `StatusResistances`/`StatusWeaknesses` and any
  combatant NPC use the combat status model defined there, not here.
- [farm_rules.md](farm_rules.md) — farm-visit anchors and partner-helper actions interact
  with farm plots/daily cycle defined there; helper must not automate the farm (Rule 9).
- [save_rules.md](save_rules.md) — DTO/versioning/migration rules the NPC social save
  section must follow.
- [event_rules.md](event_rules.md) — event bus / no-global-lookup invariants applied in
  Rule 9.

---

*Created: 2026-06-13 (NPC/social domain rule — fable_25/26/28/35 reference)*
*Sources: city/social design directions, FABLE v3.0 decision 2.4, live NPC code.*
