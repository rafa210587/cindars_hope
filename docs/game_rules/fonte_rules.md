---
doc_type: game_rule
status: accepted
domain: gameplay-progression
source_adrs:
  - ADR-0006
  - ADR-0007
source_documents:
  - docs/design/gameplay/quests/QUESTS_MAIN_PROGRESSION_REFINEMENT_DIRECTION.md
  - docs/design/FABLE_DECISOES_RESPOSTAS_v2.0.md
  - docs/design/FABLE_DECISOES_RESPOSTAS_v3.0.md
  - Assets/_Game/Scripts/Fonte/FonteState.cs
  - Assets/_Game/Scripts/Fonte/FonteFunctionUnlockService.cs
  - Assets/_Game/Scripts/Fonte/FonteRuntimeService.cs
  - Assets/_Game/Scripts/Fonte/FonteInteractable.cs
  - Assets/_Game/Scripts/Fonte/FonteAnyaValidator.cs
  - Assets/_Game/Scripts/MainProgression/FinalChoiceService.cs
  - Assets/_Game/Scripts/MainProgression/EndgameContracts.cs
  - Assets/_Game/Scripts/MainProgression/MainAct.cs
  - Assets/_Game/Scripts/Save/SaveData.cs
  - Assets/_Game/Scripts/Save/SaveManager.cs
last_reviewed: 2026-06-13
---

# Fonte de Anya Rules

> **This document is current operational behavior, not desired future state.**
> **Change this rule only via a new ADR or spec.**
>
> Referenced by `fable_10` (fragment integration → Fonte function unlocks) and
> `fable_17` (Fonte physical interactable and menu). The numeric progression below
> is grounded in live runtime code; values not yet present in code are marked
> **(proposta a calibrar)** and carry their design source.

## Purpose

Defines the Fonte de Anya — the farm's sacred spring that acts as the mechanical hub of
the main quest. This rule covers: the Fonte's lifecycle states (from `Dormant` to a
finalized ending state); how the four story fragments (Water, Memory, Life, Hope) unlock
Fonte functions; the limits and anti-exploit guards on each function (Living Water charges,
respec cooldown, mass-sale block); the three final choices (Protect / Seal / Use) and the
endgame world modifiers they apply; companion resurrection via the Fonte ("Ressurreição
Dolorosa"); and the inviolable lore rule that Anya has no altar — only the Fonte.

This rule states **what is true now in code** for the Fonte's deterministic logic. It does
not duplicate skill-tree respec cost rules (see `skill_tree_rules.md`), corpse/death flow
(see `death_anya_corpse_rules.md`), or save DTO contracts (see `save_rules.md`); it
cross-references them instead.

---

## Definitions and Terms

- **Fonte de Anya** — the sacred spring on the player's farm. The single physical anchor of
  Anya in the world. It is a runtime host (`FonteRuntimeService`) plus a physical
  `IInteractable` (`FonteInteractable`).
- **FonteState** — the Fonte's visible/mechanical lifecycle stage. Enum in
  `FonteState.cs:5-11`: `Dormant (0)`, `AwakenedReturnOnly (1)`, `WaterFlowing (2)`,
  `MemoryEchoing (3)`, `LifeBlooming (4)`, `HopeReady (5)`, and three finalized states
  `FinalizedProtected (6)`, `FinalizedSealed (7)`, `FinalizedUsed (8)`, plus `Corrupted (9)`
  and `Damaged (10)`.
- **FonteFunction** — a capability the Fonte can grant. Enum in `FonteState.cs:13-18`:
  `ReturnPoint`, `LimitedLivingWater`, `Respec`, `MinorPurification`, `AdvancedPurification`,
  `CorruptionResistance`, `MemoryReveal`, `AquaticClueActivation`, `FinalChoicePreparation`,
  `PostGameState`.
- **Fragment** — one of the four story fragments of Anya: `Water`, `Memory`, `Life`, `Hope`
  (`MainFragmentType` in `MainAct.cs:13-16`). Canonical recovery order is
  Water → Memory → Life → Hope (direction §2).
- **Integrated fragment** — a fragment brought to and absorbed by the Fonte
  (`FragmentAcquisitionState.Integrated`), which is the prerequisite checked when unlocking
  Fonte functions (`FonteFunctionUnlockService.cs:62-100`).
- **Água Viva (Living Water)** — a consumable resource the Fonte produces in limited charges
  (item id `item_consumable_agua_viva`, `FonteRuntimeService.cs:17`).
- **Ressurreição Dolorosa** — the Fonte's narrative companion revival with progressive cost
  (decision v3 3.7).

---

## Canonical Rules

### Rule 1: Initial State is Dormant; Awakens on First Death

- **Rule:** The Fonte begins in state `Dormant`. The default `FonteAnyaSection.FonteState`
  is `Dormant` (`FonteState.cs:45`). It awakens to `AwakenedReturnOnly` on the **first
  death / faint / critical event** of the player.
- **Mechanism:** Unlocking the `ReturnPoint` function advances the state to
  `AwakenedReturnOnly` (`FonteFunctionUnlockService.cs:110`). `ReturnPoint` requires **no
  fragment** — its prerequisite is unconditionally met (`FonteFunctionUnlockService.cs:67-68`,
  comment: "First death/faint; no fragment required").
- **Narrative effect:** The player wakes at the Fonte; the city does not understand what
  happened; Padre Corvus forgets the Litania do Primeiro Retorno (direction §3.1).
- **What Dormant does NOT grant:** Água Viva, respec, purification, advanced healing
  (direction §3.1).
- **Decision source:** v2 5.2 — option **B**: "A Fonte começa DORMENTE e desperta na 1ª
  morte/quest." The awakening beat is owned by the first-death flow (fable_63).
- **Constraint:** State only advances, never regresses
  (`FonteFunctionUnlockService.cs:114-116`: `if ((int)newState > (int)section.FonteState)`).
- **Applies to:** Fonte lifecycle, first-death flow, respawn.

### Rule 2: Functions Unlock by Fragment Integration

- **Rule:** Each Fonte function requires a specific integrated fragment (or none). Unlocks are
  evaluated by `FonteFunctionUnlockService.TryUnlock` against the live
  `MainProgressionSection` (`FonteFunctionUnlockService.cs:42-100`). The function-to-fragment
  mapping below is the **codified contract**:

| Function | Required fragment | Resulting FonteState | Code prerequisite |
|---|---|---|---|
| `ReturnPoint` | none (first death) | `AwakenedReturnOnly` | always met (`:67-68`) |
| `LimitedLivingWater` | Water | `WaterFlowing` | `IsFragmentIntegrated(Water)` (`:70-73`) |
| `MinorPurification` | Water | (no state advance) | `IsFragmentIntegrated(Water)` (`:80-83`) |
| `Respec` | Memory | `MemoryEchoing` | `IsFragmentIntegrated(Memory)` (`:75-78`) |
| `AdvancedPurification` | Life | `LifeBlooming` | `IsFragmentIntegrated(Life)` (`:85-88`) |
| `FinalChoicePreparation` | Hope **+** Level101 access | `HopeReady` | `IsFragmentIntegrated(Hope)` **and** `Level101AccessState == Unlocked` (`:90-95`) |

- **Order:** Canonical fragment order Water → Memory → Life → Hope (direction §2). Because
  state only advances, integrating fragments in order produces the visual progression
  Dormant → WaterFlowing → MemoryEchoing → LifeBlooming → HopeReady.
- **Integration entry point:** Fragments are integrated via
  `FonteRuntimeService.IntegrateFragment(MainFragmentType)`, called by quest reward logic
  (fable_10) (`FonteRuntimeService.cs:41-52`); on success it re-runs `RefreshUnlocks`.
- **Re-unlock idempotency:** Already-unlocked functions return `AlreadyUnlocked = true`
  without side effects (`FonteFunctionUnlockService.cs:50-51`).
- **Applies to:** Main-quest progression, Fonte function gating.

### Rule 3: Living Water — 3 Charges, Daily Recharge, 1 Collect/Day

- **Rule:** Once `LimitedLivingWater` is unlocked, the Fonte produces Água Viva with a hard
  cap of **3 charges** (`LivingWaterState.MaxCharges = 3`, `FonteState.cs:63`). On unlock,
  charges are set to the maximum (`FonteRuntimeService.cs:62-63`).
- **Recharge:** **+1 charge per in-game day** up to the maximum, applied on `DayStartedEvent`
  (`FonteRuntimeService.cs:181-185`). Recharge only occurs while unlocked and below max.
- **Collection:** The player may collect **1 flask per day** (idempotent per day). Collection
  is gated by `CanGrantToday(lastGrantDay, today)` — `lastGrantDay != today`
  (`FonteRuntimeService.cs:35-38, 75-110`); a second attempt the same day fails with
  `ALREADY_GRANTED_TODAY`.
- **Consumption:** Each successful collection consumes **1 charge**
  (`LivingWaterChargesConsumed = 1`, `FonteFunctionUnlockService.cs:143-147`) and adds one
  `item_consumable_agua_viva` to the inventory (`FonteRuntimeService.cs:94-100`). If the
  inventory is unavailable/full the collection fails (`INVENTORY_UNAVAILABLE_OR_FULL`) and no
  charge is consumed.
- **Possible uses (design):** light healing, controlled fatigue reduction, simple ritual,
  small purification, aquatic-clue activation, special cave lakes (direction §3.2). The exact
  per-use effects are **(proposta a calibrar)** — defined by future use-effect specs.
- **Applies to:** Resource economy, healing, cave interaction.

### Rule 4: Living Water Anti-Exploit — BlockMassSale and Charge Guards

- **Rule:** Mass sale of Água Viva is blocked. `LivingWaterState.BlockMassSale = true` by
  default (`FonteState.cs:69`), enforcing the direction's anti-exploit clause "não vender em
  massa" / "não banalizar MP/cura" (direction §9).
- **Charge integrity (validator, `FonteAnyaValidator.cs`):**
  - **Blocker** if unlocked and `MaxCharges <= 0` (`:20-21`).
  - **Blocker** if unlocked and `CurrentCharges < 0` (`:23-24`).
  - **Warning** if `CurrentCharges > MaxCharges` — "possible reload exploit" (`:26-27`).
- **Storage:** `CanBeStoredAsItem` defaults to `false` (`FonteState.cs:66`); whether Água Viva
  can be banked as a stockpiled item is **(proposta a calibrar)**.
- **Applies to:** Economy guards, save-integrity validation.

### Rule 5: Respec via Fonte — Memory-Gated, Confirmation, Cooldown

- **Rule:** The skill-tree respec is performed **at the Fonte de Anya**, gated by the Memory
  Fragment (`FonteFunction.Respec`, available from state `MemoryEchoing`). On unlock,
  `RespecState.Unlocked` is set (`FonteRuntimeService.cs:67-71`).
- **Confirmation required:** `RespecState.RequiresConfirmation = true` (`FonteState.cs:78`);
  `EvaluateUseRequest` fails with `RESPEC_REQUIRES_CONFIRMATION` if not confirmed
  (`FonteFunctionUnlockService.cs:131-132`).
- **Cooldown:** If `RespecState.CooldownDays` and `LastRespecDay` are set, a respec within the
  cooldown window fails with `RESPEC_ON_COOLDOWN`
  (`FonteFunctionUnlockService.cs:133-135`). The cooldown-day value itself is nullable in code
  (`RespecState.CooldownDays`, `FonteState.cs:76`) and is **(proposta a calibrar)** by the
  skill-tree balance spec.
- **No mid-combat respec:** respec cannot be performed in combat (consistent with
  `skill_tree_rules.md`).
- **Cost & punitive re-lock:** Respec cost (1 free, then ~250 gold) and the punitive
  re-locked-item rule are **owned by `skill_tree_rules.md` / ADR-0010** — not redefined here.
  The Fonte is the location and gate; the cost/penalty contract lives there.
- **Runtime wiring:** `FonteInteractable.TryRespec` calls `SkillTreeManager.TryRespec` via
  `GameBootstrap` (`FonteInteractable.cs:116-138`); the Fonte does not mutate skill state
  directly.
- **Applies to:** Build flexibility; cross-references skill-tree rules.

### Rule 6: Purification — Minor (Water) and Advanced (Life)

- **Rule:** Two purification functions exist:
  - `MinorPurification` requires the Water fragment (`FonteFunctionUnlockService.cs:80-83`).
  - `AdvancedPurification` requires the Life fragment (`:85-88`) and advances state to
    `LifeBlooming` (`:108`).
- **Advanced purification flags (`PurificationState`, `FonteState.cs:82-94`):**
  `RequiresFragmentLife = true` (default); the following are **disabled by default** and are
  **(proposta a calibrar)** by future purification specs: `CanPurifyCorruptedLivingWater`,
  `CanReduceCavePenalty`, `CanAffectNpcAnimalSoil`, `RequiresLivingWater`,
  `UnlockedMinor`/`UnlockedAdvanced`.
- **Use evaluation:** Advanced purification use fails with `ADVANCED_PURIFICATION_NOT_UNLOCKED`
  unless `PurificationState.UnlockedAdvanced` is set
  (`FonteFunctionUnlockService.cs:149-152`).
- **Design intent:** purify corrupted Água Viva; reduce severe cave penalties; cure specific
  NPC/animal/soil events; protect against rare corruption (direction §3.4).
- **Applies to:** Corruption mitigation, cave/world events.

### Rule 7: Final Choice — Protect / Seal / Use

- **Rule:** Once the Hope fragment is integrated and `FinalChoicePreparation` is unlocked
  (state `HopeReady`), the player makes a single, irreversible final choice resolved by
  `FinalChoiceService.EvaluateFinalChoice` (`FinalChoiceService.cs:11-79`).
- **Hard gates (all required):**
  - Hope fragment integrated (`FINAL_CHOICE_REQUIRES_HOPE_FRAGMENT`, `:29-30`).
  - `Level101AccessState == Unlocked` (`FINAL_CHOICE_REQUIRES_LEVEL101_ACCESS`, `:33-34`).
  - Strong confirmation token `"FINAL_CHOICE_CONFIRMED"`
    (`FINAL_CHOICE_REQUIRES_STRONG_CONFIRMATION`, `:8, :37-38`).
  - Choice cannot be `None` (`FINAL_CHOICE_CANNOT_BE_NONE`, `:41-42`).
- **Idempotency:** Once `FinalChoiceStatus.Resolved`, re-evaluation returns
  `AlreadyApplied = true` (`:21-22`). A preview (`PreviewOnly = true`) returns the ending id
  without changing state (`:44-54`).
- **Effect on apply:** sets `FinalChoiceState = Resolved`, `CurrentAct = PostGame`,
  `PostGameWorldState = endingId`, and the corresponding finalized `FonteState`
  (`:56-71`).
- **The three endings** map to `EndingEffectProfile` (`EndgameContracts.cs:51-79`):

| Choice | EndingId | FonteFinalState | ManaBloomPolicy | Cave post-game | City memory | Corruption containment |
|---|---|---|---|---|---|---|
| **Protect** | `ending_protect` | `FinalizedProtected` | `RareNatural` | `StableLimited` | `Preserved` | `PurifiedGuarded` |
| **Seal** | `ending_seal` | `FinalizedSealed` | `RareRestricted` | `MoreStableReduced` | `PreservedWithLoss` | `StrongSeal` |
| **Use** | `ending_use` | `FinalizedUsed` | `MorePredictable` | `NewAreas` | `MaterialProgress` | `RiskOfRepeat` |

- **All endings require strong confirmation** (`RequiresStrongConfirmation = true` on every
  profile, `EndgameContracts.cs:58, 68, 78`).
- **Anya is never restored:** no choice restores Anya (direction §3.5, §8; v2 A.4). "Use"
  amplifies only what was already unlocked and is **not** a resurrection (v2 A.4 [PROPOSTA]).
- **Applies to:** Endgame, post-game world state, Mana bloom policy.

### Rule 8: Companion Resurrection — Ressurreição Dolorosa (Progressive Cost)

- **Rule:** Companions have **no permadeath in base combat**; on defeat they go `Injured`
  for 1–2 days (decision v3 3.5). Permadeath exists **only** in scripted narrative events
  (decision v3 3.7, OVERRIDE). When a companion dies in such an event, the **Fonte de Anya
  resurrects it with a progressive cost** — "Ressurreição Dolorosa" (v3 3.7; reconciliation
  v3 §87).
- **Cost shape:** the cost grows with each resurrection ("custo progressivo"). The concrete
  cost curve and resource type are **(proposta a calibrar)** — owned by the future companion
  emenda to `COMPANIONS_DIRECTION` and the WAVE 14 companion specs; **no companion combat/
  revive code exists yet** (v3 §136). This rule records only the binding decision, not a code
  contract.
- **Inviolable:** even Dolorosa revival does not apply to Anya herself — Anya cannot be
  resurrected by any means (Rule 7; Rule 10).
- **Applies to:** Companions (WAVE 14, gated); cross-references the companion direction emenda.

### Rule 9: Function Visibility — No Spoiler ("???")

- **Rule:** A function that is **not yet unlocked is displayed as "???"** in the Fonte menu —
  the UI never reveals what is sealed (`FonteInteractable.cs:11-13` canonical comment, and
  `:97-99`, `:110-113`: "??? (a agua esta parada)" / "??? (ecos adormecidos)").
- **Rationale:** preserves the direction's spoiler-control rule "não explicar tudo por
  diálogo expositivo" and "não fazer a Fonte liberar tudo cedo" (direction §13.1).
- **Applies to:** Fonte UI/menu presentation (fable_17; canvas migration in fable_14).

### Rule 10: Anya Has No Altar — Only the Fonte (Inviolable)

- **Rule (inviolable):** Anya **never has an altar**. The Fonte is her only physical anchor in
  the world. The temple holds only Kanthor (plus a rural Thandra cult); "ANYA NUNCA TEM ALTAR
  (só a Fonte — regra inviolável)" (v2 A.5 [CANON]). "Anya não tem fiéis: tem guardiões"
  (v2 A.5).
- **Consequence for the prayer/Marks system:** the three Anya waters (Jardim das Estátuas +
  Fonte + Anya-Echo rooms) grant **no prayer bonus** — only lore plus the already-canonical
  limited Água Viva healing (v2 A.5, exception 11).
- **Applies to:** World design, prayer/Marks system (fable_68), Fonte placement.

---

## Quick Reference (canonical values)

| Field | Canonical value | Source |
|---|---|---|
| Initial state | `Dormant` | `FonteState.cs:45` / v2 5.2 |
| Awakening trigger | First death/faint (ReturnPoint, no fragment) | `FonteFunctionUnlockService.cs:67-68` |
| State advance-only | Yes (never regresses) | `FonteFunctionUnlockService.cs:114-116` |
| Living Water max charges | **3** | `LivingWaterState.MaxCharges` (`:63`) |
| Living Water recharge | +1 / in-game day, up to max | `FonteRuntimeService.cs:181-185` |
| Living Water collect rate | 1 flask / day (idempotent) | `FonteRuntimeService.cs:35-38, 75-110` |
| Living Water item id | `item_consumable_agua_viva` | `FonteRuntimeService.cs:17` |
| Mass sale | Blocked (`BlockMassSale = true`) | `FonteState.cs:69` / direction §9 |
| Respec gate | Memory fragment (`MemoryEchoing`) | `FonteFunctionUnlockService.cs:75-78` |
| Respec confirmation | Required | `FonteState.cs:78` |
| Respec cooldown | Nullable; **(proposta a calibrar)** | `FonteState.cs:76` |
| Respec cost / penalty | Owned by `skill_tree_rules.md` (1 free, ~250g, punitive) | ADR-0010 |
| Final choices | Protect / Seal / Use | `EndgameContracts.cs:51-79` |
| Final-choice token | `FINAL_CHOICE_CONFIRMED` | `FinalChoiceService.cs:8` |
| Final-choice gates | Hope fragment + Level101 Unlocked + token | `FinalChoiceService.cs:29-42` |
| Anya restoration | Never (any ending) | direction §3.5 / v2 A.4 |
| Companion revive | Fonte "Ressurreição Dolorosa", progressive cost | v3 3.7 |
| Anya altar | None — only the Fonte (inviolable) | v2 A.5 |

---

## Edge Cases

- **Function unlocked out of order:** state only advances, so unlocking a higher function
  never lowers `FonteState`; integrating a later fragment before an earlier one still raises
  state to the higher tier (`FonteFunctionUnlockService.cs:114-116`).
- **Final choice without Level101 access:** fails with `FINAL_CHOICE_REQUIRES_LEVEL101_ACCESS`
  even when the Hope fragment is integrated (`FinalChoiceService.cs:33-34`).
- **Final choice re-attempt after resolution:** returns `AlreadyApplied = true`, no second
  ending is applied (`FinalChoiceService.cs:21-22`).
- **Living Water collect when at 0 charges:** fails `LIVING_WATER_NO_CHARGES`
  (`FonteFunctionUnlockService.cs:141-142`).
- **Living Water collect when inventory full:** fails `INVENTORY_UNAVAILABLE_OR_FULL`; no
  charge consumed and `lastGrantDay` is not advanced (`FonteRuntimeService.cs:94-100`).
- **Charges exceed max after reload (save tampering):** validator emits a non-blocking
  `LIVING_WATER_OVER_MAX` warning (`FonteAnyaValidator.cs:26-27`).
- **Respec function unlocked but RespecState not flagged:** validator warns
  `RESPEC_STATE_UNLOCKED_NO_FUNCTION` (`FonteAnyaValidator.cs:30-31`).
- **Legacy save without Fonte section:** loads with defaults → Fonte `Dormant`, no functions
  (`SaveManager.cs:963-972`; `FonteSaveData` is additive, `SaveData.cs:65-75`).
- **Anya altar requested by any content:** forbidden by Rule 10 — must use the Fonte instead.

---

## What Persists in Save

- **Rule:** The Fonte persists as a simple-types-only DTO (`FonteSaveData`,
  `SaveData.cs:65-75`), per `save_rules.md` / ADR-0006. Captured fields
  (`SaveManager.cs:435-465`), restored on load (`SaveManager.cs:963-972` via
  `FonteRuntimeService.RestoreFromSave`):
  - `FonteState` — int (enum value).
  - `UnlockedFunctions` — `List<int>` (enum values).
  - `LivingWaterUnlocked` — bool.
  - `LivingWaterCharges` — int (clamped to ≥ 0 on restore, `FonteRuntimeService.cs:125`).
  - `LastGrantDay` — int (daily-collect idempotency anchor).
  - `IntegratedFragments` — `List<int>` (fragment enum values; rebuilt into
    `MainProgressionSection` as `Integrated`, `FonteRuntimeService.cs:129-140`).
- **Derived on load, not stored:** `RestoreFromSave` re-runs `RefreshUnlocks`, so
  `RespecState.Unlocked` / `LivingWaterState.Unlocked` are recomputed from fragments rather
  than persisted directly (`FonteRuntimeService.cs:142, 55-72`).
- **Known persistence gaps (residual risk / proposta a calibrar):** `FonteSaveData` does
  **not** persist `LivingWater.MaxCharges`, `RespecState.LastRespecDay` / `CooldownDays`, or
  `PurificationState` flags. After reload the respec cooldown anchor and advanced-purification
  state are not restored. A future save-section spec must decide whether these become part of
  the DTO; until then the cooldown is effectively reset on reload.
- **No Unity refs:** only ints, bools, and lists of ints — no ScriptableObject/GameObject/
  MonoBehaviour references (compliant with `save_rules.md`).

---

## What Tests Must Cover

Aligned with `.claude/rules/testing-quality-gate.md` (Fonte logic is deterministic pure C# —
EditMode tests preferred):

- **Unlock prerequisites:** each function unlocks only with its required fragment; `ReturnPoint`
  with no fragment; `FinalChoicePreparation` blocked without Level101 access
  (`FonteFunctionUnlockService.CheckUnlockPrerequisite`).
- **State advance-only:** `FonteState` never regresses across unlock sequences.
- **Living Water:** unlock sets 3 charges; daily recharge caps at max; 1 collect/day
  idempotency (`ALREADY_GRANTED_TODAY`); charge consumption; 0-charge and inventory-full
  failure paths leave state unchanged.
- **Respec:** confirmation required; cooldown blocks within window; unlocked-state derives from
  Memory fragment.
- **Final choice:** all four gates (Hope, Level101, token, non-None); idempotency after
  resolution; preview does not mutate state; each ending maps to the correct
  `EndingEffectProfile` and `FonteFinalState`.
- **Validator:** blocker/warning issues for zero/negative/over-max charges and
  function/state-flag mismatches (`FonteAnyaValidator`).
- **Save round-trip:** capture → restore preserves `FonteState`, `UnlockedFunctions`,
  Living Water charges, `LastGrantDay`, integrated fragments; legacy (null) section loads as
  Dormant; document the known cooldown/MaxCharges/purification persistence gaps as residual
  risk.
- **Play Mode / manual scenario (required):** Fonte interactable open/close, "???" masking of
  locked functions, Água Viva collection feedback, respec confirmation flow — see the fable_17
  human test scenario.

---

## Open Questions

- Concrete per-use effects of Água Viva (healing/fatigue/purification amounts) — **(proposta a
  calibrar)** by a use-effect spec.
- Respec `CooldownDays` numeric value — **(proposta a calibrar)** with the skill-tree balance
  spec (currently nullable in code).
- Whether Água Viva can be stored as a stockpiled item (`CanBeStoredAsItem` default false).
- Advanced-purification target list and the `Can*` flags (corrupted Água Viva, cave penalty,
  NPC/animal/soil) — disabled by default, pending a purification spec.
- Companion "Ressurreição Dolorosa" cost curve and resource — pending the COMPANIONS_DIRECTION
  emenda and WAVE 14 specs (no code yet).
- Persisting respec cooldown / `MaxCharges` / purification state across save reload — pending a
  save-section spec.

---

## Fontes

- **Design directions:**
  - `docs/design/gameplay/quests/QUESTS_MAIN_PROGRESSION_REFINEMENT_DIRECTION.md` — §2 fragment
    order, §3.1–§3.5 Fonte states/functions, §8 endings, §9 Mana anti-exploit, §13.1 spoiler
    control.
- **Binding decisions:**
  - `docs/design/FABLE_DECISOES_RESPOSTAS_v2.0.md` — 5.2 (initial state Dormant),
    Apêndice Lore A.4 (USAR mapping; Anya never restored), A.5 (Anya has no altar — inviolable).
  - `docs/design/FABLE_DECISOES_RESPOSTAS_v3.0.md` — 3.5 (Injured, no base permadeath),
    3.7 (narrative-only permadeath; "Ressurreição Dolorosa" progressive cost),
    reconciliation §87.
- **Code (file:line for key numbers/states):**
  - `Assets/_Game/Scripts/Fonte/FonteState.cs` — states/functions enums, default Dormant,
    `MaxCharges = 3`, `BlockMassSale = true`, respec/purification state fields.
  - `Assets/_Game/Scripts/Fonte/FonteFunctionUnlockService.cs` — unlock prerequisites,
    state-advance-only, Living Water consumption, respec/purification use evaluation.
  - `Assets/_Game/Scripts/Fonte/FonteRuntimeService.cs` — fragment integration, daily recharge,
    1-collect/day, save restore.
  - `Assets/_Game/Scripts/Fonte/FonteInteractable.cs` — "???" spoiler masking, respec wiring.
  - `Assets/_Game/Scripts/Fonte/FonteAnyaValidator.cs` — charge/state integrity guards.
  - `Assets/_Game/Scripts/MainProgression/FinalChoiceService.cs` — final-choice gates,
    idempotency, ending application.
  - `Assets/_Game/Scripts/MainProgression/EndgameContracts.cs` — `EndingEffectProfile` per
    ending (Mana bloom / cave / city / corruption policies).
  - `Assets/_Game/Scripts/MainProgression/MainAct.cs` — acts and `MainFragmentType`.
  - `Assets/_Game/Scripts/Save/SaveData.cs` / `SaveManager.cs` — `FonteSaveData` capture/restore.

> Per docs-governance, amendments are historical record only and are **not** cited here as
> canonical; the canonical sources above are the directions, binding decisions, ADRs, and code.

---

## Cross-References

- **ADRs:**
  - [ADR-0006: Save Data Contracts (Simple DTOs)](../decisions/ADR-0006-save-data-contracts-simple-dtos.md)
    — Fonte save DTO compliance.
  - [ADR-0007: Event Bus Gameplay Communication](../decisions/ADR-0007-event-bus-gameplay-communication.md)
    — `DayStartedEvent` subscription, `PlayerActionFeedbackEvent` feedback.
  - [ADR-0010: FABLE Skill and Inventory Rules Reconciliation](../decisions/ADR-0010-fable-skill-and-inventory-rules-reconciliation.md)
    — respec cost/penalty contract (location/gate only here).
- **Sibling game rules (referenced, not duplicated):**
  - [skill_tree_rules.md](skill_tree_rules.md) — respec cost, punitive re-lock, active slots
    (the Fonte is the respec **location/gate**; cost/penalty live there).
  - [save_rules.md](save_rules.md) — DTO simple-types contract and migration; Fonte section is
    additive.
  - [death_anya_corpse_rules.md](death_anya_corpse_rules.md) — death/respawn flow and corpse
    recovery (the Fonte's `ReturnPoint` is the respawn anchor; legacy Anya-NPC respec text in
    that file is superseded by Fonte respec per ADR-0010).
  - [farm_rules.md](farm_rules.md) — the Fonte sits on the farm; daily cycle drives recharge.

---

*Created: 2026-06-13 (referenced by fable_10, fable_17)*
*Source: QUESTS_MAIN_PROGRESSION direction, FABLE v2.0 (5.2, A.4, A.5), FABLE v3.0 (3.5, 3.7), live Fonte/MainProgression code*
