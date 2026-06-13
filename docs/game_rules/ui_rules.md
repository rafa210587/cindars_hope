---
doc_type: game_rule
status: accepted
domain: ui-ux
source_adrs:
  - ADR-0007
source_documents:
  - docs/design/gameplay/ui_ux/HUD_LAYOUT_SCENES_DIRECTION_v1.0.md
  - docs/design/FABLE_DECISOES_RESPOSTAS_v2.0.md
  - docs/design/FABLE_DECISOES_RESPOSTAS_v3.0.md
  - .claude/skills/ui-projection-pattern/SKILL.md
  - Assets/_Game/Scripts/UI/HUD/HUDGameplayViewModel.cs
  - Assets/_Game/Scripts/UI/HUD/HudVisibilityController.cs
  - Assets/_Game/Scripts/Core/Events/HudEvents.cs
  - Assets/_Game/Scripts/UI/HUD/GameplayFeedbackService.cs
  - Assets/_Game/Scripts/UI/HUD/GameplayHudCanvasController.cs
  - Assets/_Game/Scripts/Combat/FloatingDamageNumberDisplayer.cs
last_reviewed: 2026-06-13
---

# UI Rules (HUD and Non-Modal Overlays)

> **This document is current operational behavior, not desired future state.**
> **Change this rule only via a new ADR or spec.**

## Purpose

Defines the **non-modal** UI layer of Cindar's Hope: the gameplay HUD, its always-on
overlays (vitals, world clock/calendar, minimap, level/XP, status icons, quest tracker,
toasts) and the in-world floating combat feedback, plus the projection/ViewModel
construction pattern they all follow and the rules for suppressing the HUD.

This rule is **distinct** from [`ui_modal_rules.md`](./ui_modal_rules.md), which owns the
**modal** layer (the single 8-tab panel, the ModalManager stack, input blocking, Esc/back).
Where the two layers meet — the HUD dimming/hiding when a modal opens — is described here as
a *consequence* and cross-referenced, not duplicated. If a behavior involves a focused panel
that blocks gameplay input, it belongs to `ui_modal_rules.md`; if it is an overlay that stays
live during gameplay, it belongs here.

---

## Scope

In scope:

- Gameplay HUD layout, anchoring, and per-element rules (non-modal).
- The projection/ViewModel construction pattern shared by HUD and panels.
- Floating damage numbers and other in-world combat feedback overlays.
- HUD suppression / visibility (modal-driven and future cinematic-driven).
- The System tab's v1 option scope (volumes, fullscreen/windowed, resolution).
- Canvas responsiveness, UI scale, and safe-area anchoring.

Out of scope (owned elsewhere):

- Modal stack, input blocking, Esc behavior, the 8-tab panel internals → `ui_modal_rules.md`.
- Event bus mechanics (DTO shape, Publish/Subscribe, unsubscribe) → `event_rules.md`.
- The numeric content the HUD *displays* (HP/MP curves, slot counts, calendar values) — those
  are owned by their domain rules (combat, skill tree, time); this rule covers only how the
  HUD *presents* them.

---

## Definitions and Terms

- **HUD (heads-up display):** the always-rendered, non-modal overlay drawn over the live game
  world. Visible during normal gameplay; never blocks player movement or world input.
- **Overlay:** an individual non-modal HUD element (vitals bars, minimap, quest tracker, etc.).
- **Floating damage number:** an in-world (world-space canvas) popup spawned above a damaged
  target's head; part of combat feedback, not the screen-space HUD.
- **Toast:** a short transient HUD message (save result, loot, quest update) shown in a
  prioritized queue above the hotbar.
- **Projection / ViewModel:** a pure-C# read-only state object that derives presentation
  answers from game state; the View (a thin MonoBehaviour) binds to it. See the
  `ui-projection-pattern` skill.
- **HUD visibility:** the live, modal-driven state — HUD is hidden while a modal is on the
  stack (`HudVisibilityChangedEvent`).
- **HUD suppression:** a planned, cinematic-driven state — a system asks the HUD to hide for a
  scripted moment (e.g., a final-boss reveal) via the future `HudSuppressionChangedEvent`.
- **Reference resolution:** the canvas design resolution against which percentage anchors are
  computed (1280×720); not the shipping target resolution (1920×1080).

---

## Canonical Rules

### Rule 1: HUD is non-modal and never blocks gameplay input

- **Rule:** HUD overlays render over the live world and are **always visible during gameplay**.
  Unlike modals, the HUD **never blocks** player movement, attack, or world interaction.
- **Constraint:** The center of the screen ("the stage") stays clear for gameplay; overlays
  live in the four corners and the bottom edge (HUD direction §1).
- **Cross-reference:** When a modal opens, the modal layer takes input (see `ui_modal_rules.md`
  "Input Blocking"); the HUD's only reaction is visibility — see Rule 7.
- **Source:** HUD direction §1; `ui_modal_rules.md` (HUD elements "always active" note).

### Rule 2: Every HUD element and panel is built with the projection/ViewModel pattern

- **Rule:** UI follows the project's MVVM-lite "projection" convention:
  - **ViewModel / projection** is a **pure C# class** with zero `UnityEngine.UI` dependency
    (e.g., `GameplayHudViewModel`, alias `HUDGameplayViewModel`).
  - **View** is a thin MonoBehaviour that binds Text/Image to the ViewModel (e.g.,
    `StatusBarsHudView`, `QuestTrackerHudView`, `ActiveSkillSlotsHudView`,
    `FeedbackToastHudView`, attached by `GameplayHudCanvasController`).
  - Mutually-exclusive UI states are an **enum**, not bool soup; every disabled/locked/error
    state carries a **human-readable reason string** (no silently disabled control).
  - Views **rebuild from events, never poll**; they subscribe on enable and unsubscribe on
    disable.
- **Computed presentation** lives on the ViewModel, e.g. `HpPercent`, `MpPercent`,
  `StaminaPercent`, and the low thresholds `IsHpLow`/`IsMpLow`/`IsStaminaLow` fire at
  **< 0.25** of max (`HUDGameplayViewModel.cs:58-60`).
- **Source:** `.claude/skills/ui-projection-pattern/SKILL.md`; `HUDGameplayViewModel.cs`;
  `GameplayHudCanvasController.cs`.

### Rule 3: HUD layout, anchors, and per-element placement

- **Rule:** The HUD maps to four corners plus the bottom edge; nothing is fixed in absolute
  pixels — every element anchors in **% of the screen** via CanvasScaler (see Rule 8).
- **Layout map (HUD direction §1-2):**

| Region | Position | Elements |
|---|---|---|
| Body (top-left) | sup-esq (~2%,2%) | HP / Stamina / MP bars, hunger·fatigue, status icons |
| World (top-right) | sup-dir (~98%,2%) | clock/date/weather/lunar widget, minimap, inferred-class title, quest tracker |
| Hand (bottom-center) | (~50%,96%) | hotbar (10 slots) + active skill slots (4) |
| Stage (center) | — | empty; belongs to gameplay |

- **Per-element rules:** see the **HUD Overlay Specification** table below.
- **Source:** HUD direction §1-2.

### Rule 4: MP, status, and low-resource feedback are conditional

- **Rule:** The MP bar is shown **only when magic is known/equipped** (`ShowMp` on the
  ViewModel; HUD direction §2). Status-effect icons appear only while an effect is active and
  show a tooltip on hover.
- **Rule:** Vitals near depletion give visual feedback — hunger/fatigue icons pulse at the
  critical threshold (HUD direction §2); the ViewModel exposes `IsHpLow`/`IsMpLow`/
  `IsStaminaLow` at **< 25%** for the bars to react.
- **Constraint:** What the bars *contain* (HP/MP/stamina values and curves) is owned by combat
  and progression rules; this rule governs only their presentation.
- **Source:** `HUDGameplayViewModel.cs:16-18,55-60`; HUD direction §2.

### Rule 5: Toasts are a prioritized, capped queue

- **Rule:** Transient HUD messages ("toasts") are served by `GameplayFeedbackService` as a
  **single-active + queue** model:
  - One message is active at a time; others wait in a FIFO queue.
  - Priority is `Low` / `Normal` / `Important`; an **Important** message preempts the active
    one immediately, while Low/Normal enqueue.
  - The HUD shows at most **3** toasts at once (HUD direction §2), driven off the service via
    `HudFeedbackUpdatedEvent`.
- **Sources of toasts (current):** save/load result, quest accepted/completed/reward, enemy
  killed/loot, cave level entered/exited, generic `NotificationToastRequestedEvent`
  (`GameplayFeedbackService.cs:83-113`).
- **Source:** `GameplayFeedbackService.cs`; HUD direction §2; `HudEvents.cs`.

### Rule 6: Floating damage numbers are in-world combat feedback (already live)

- **Rule:** Damage and immunity are shown as **floating numbers** spawned above the target's
  head on a **world-space** canvas, separate from the screen-space HUD
  (`FloatingDamageNumberDisplayer`).
  - Color: **Physical = red, magical/elemental (Fire/Ice/Lightning/Arcane/Toxic) = blue,
    immune = gray**, player-taken damage = red
    (`FloatingDamageNumberDisplayer.cs:172-187`).
  - Animation: rise ~**0.35** world units and fade over ~**0.8 s**
    (`FloatingDamageNumberDisplayer.cs:16-17,206-220`).
  - The popup anchors to the target's `DamagePopupAnchor` (collider/transform fallback) so it
    always lands above the actual target head.
  - The displayer is **event-driven**: it subscribes to `DamageAppliedEvent` and
    `PlayerDamagedEvent` (`FloatingDamageNumberDisplayer.cs:69-70`), with an explicit
    `ShowAtTarget(...)` entry point for direct callers.
- **Status:** This overlay **already exists and is wired** (SPEC 14A-FIX series); the planned
  `fable_71` combat-feel pass does **not** recreate it (decision v3 4.2; fable_71 "Out of
  scope").
- **Source:** `FloatingDamageNumberDisplayer.cs`; FABLE v3.0 decision 4.2.

### Rule 7: HUD visibility (live, modal-driven) vs. HUD suppression (planned, cinematic-driven)

- **Rule (live):** When a modal is on the stack, the HUD hides; when the stack clears, it
  returns. `HudVisibilityController` polls `ModalManager.HasActiveModal` and publishes
  `HudVisibilityChangedEvent(isVisible)` on each transition
  (`HudVisibilityController.cs:15-28`; `HudEvents.cs:17-21`). This is the **consequence** of
  the modal rules described in `ui_modal_rules.md` — the modal layer owns *why* a modal is
  open; this rule owns *how the HUD reacts*.
- **Rule (planned):** A separate, **cinematic-driven** suppression channel,
  `HudSuppressionChangedEvent`, lets a scripted system hide/restore the HUD for a moment such
  as the final-boss reveal. This event is **confirmed not yet present in code** and is to be
  **provided by `fable_71`** (combat-feel pass); `fable_43` is its first consumer.
- **Constraint:** Do **not** conflate the two. `HudVisibilityChangedEvent` reflects modal stack
  state and exists today; `HudSuppressionChangedEvent` is a future cinematic contract. Both are
  ordinary `GameEventBus` events and must follow `event_rules.md` (struct/DTO, Publish/Subscribe,
  unsubscribe on destroy).
- **Source:** `HudVisibilityController.cs`; `HudEvents.cs`; FABLE v3.0 decision 4.2; fable_71;
  decision v3 4.1 (game clock pauses on any modal/panel/dialogue, related but owned by
  `time_rules.md`).

### Rule 8: Responsiveness, reference resolution, UI scale, and safe area

- **Rule (mother rule — HUD direction §0):** **No element is positioned in absolute pixels.**
  Every element anchors in **% of the screen** via a `CanvasScaler` set to
  *Scale With Screen Size*. A UI spec that copies pixel coordinates without an anchor violates
  this direction.
- **Reference resolution:** the canvas design reference is **1280×720**; percentage anchors are
  computed against it (HUD direction §0). Dialogue boxes are **60-80%** of screen width,
  anchored to the bottom, with scalable font.
- **Shipping target (decision v2 7.2):** the game targets **1920×1080 / 16:9, fullscreen with
  an optional windowed mode**. Because anchoring is percentage-based against the 1280×720
  reference, the same layout scales cleanly to 1920×1080 and other 16:9 sizes.
- **UI scale:** a single CanvasScaler match factor governs scale; the same canvas must stay
  legible at 1280×720, 1920×1080, and arbitrary editor window sizes (precedent in
  `DebugHud.cs:139`). There is **no** per-user UI-scale slider in v1 (not in the System tab's
  v1 scope — see Rule 9).
- **Safe area / anchoring:** corner overlays anchor to their nearest screen corner so they
  respect the visible area at 1920×1080; ultrawide (21:9) anchor validation is an open
  playtest item (HUD direction Parte E).
- **Source:** HUD direction §0 and Parte E; FABLE v2.0 decision 7.2.

### Rule 9: System tab v1 option scope

- **Rule (decision v3 4.4):** The **System** tab (one of the eight panel tabs) ships in v1 with
  **only** these options:
  - **Volume** sliders: Master / SFX / Music, **persisted**.
  - **Fullscreen / Windowed** toggle.
  - **Resolution** dropdown.
  - **Nothing more** in v1 (no rebind, no gamepad, no UI-scale slider, no language picker).
- **Ownership:** these options and their persistence belong to **F56** (settings/options spec).
  This `ui_rules.md` only fixes the **scope and placement**; the System tab itself is rendered
  inside the modal panel, so its *open/close/Esc* behavior is governed by `ui_modal_rules.md`.
- **Source:** FABLE v3.0 decision 4.4; HUD direction Parte B §4 (the eight-tab panel).

---

## HUD Overlay Specification (reference 720p)

Values below are from `HUD_LAYOUT_SCENES_DIRECTION_v1.0.md` §2; pixel sizes are *reference*
sizes that scale by % (Rule 8), not absolute coordinates.

| Overlay | Anchor | Reference size | Rule |
|---|---|---|---|
| Vital bars (HP/STA/MP) | top-left (2%,2%) | HP 240×20, STA 240×16, MP 240×16 | MP visible only with magic known/equipped |
| Hunger / fatigue | under bars | 24px icon + fill ring | pulse at critical threshold |
| Status icons | under hunger/fatigue | 24px icon row | tooltip on hover |
| Clock/date/weather/lunar | top-right (98%,2%) | 220×64 | the F20 widget (`time_rules.md` owns values) |
| Minimap | top-right, under clock (98%,12%) | 180×180 | fog-of-war in cave; static plan in town/farm; never shows enemies (F38) |
| Inferred-class title | under minimap | small text | updates on sleep |
| Quest tracker | right (98%,38%) | 300px wide, 2 lines | collapsible; 1 tracked quest |
| Hotbar | bottom-center (50%,96%) | 10 slots × 48px | keys 1-0 |
| Active skill slots | right of hotbar | 4 slots × 56px | key + radial cooldown; capped at 4 (`HUDGameplayViewModel.cs:33-34`) |
| Toasts | above hotbar | queue, max 3 | priority from `GameplayFeedbackService` |
| Boss bar | top-center (50%,6%) | 480×24 | name + phase; only during boss |

---

## Floating Damage Number Color Map

From `FloatingDamageNumberDisplayer.cs:172-187`:

| Condition | Color |
|---|---|
| Physical damage | red (1, 0.15, 0.15) |
| Magical / elemental (Fire/Ice/Lightning/Arcane/Toxic) | blue (0.35, 0.65, 1) |
| True damage | light red (1, 0.4, 0.4) |
| Immune | gray (0.55, 0.55, 0.55) |
| Damage taken by player | red (1, 0.15, 0.15) |

---

## Edge Cases

- **Modal opens while a toast is mid-display:** HUD hides via `HudVisibilityChangedEvent`; the
  toast queue is preserved by the service (it tracks state in C#, not in the View) and resumes
  when the HUD returns. Toasts are not lost by a modal opening.
- **Floating number for 0 damage:** suppressed unless the hit was an immunity
  (`FloatingDamageNumberDisplayer.cs:38,89` — non-immune ≤0 is skipped); immunity shows the
  "Immune" label.
- **Floating displayer not present in scene:** `ShowAtTarget` no-ops safely when no instance
  exists (`FloatingDamageNumberDisplayer.cs:36`); combat never depends on a popup spawning.
- **MP bar with no magic:** the bar is hidden, not shown empty (`ShowMp` false); re-shown the
  moment a spell/build makes mana relevant.
- **Live visibility vs. planned suppression overlap:** if both a modal is open and a future
  cinematic suppression is active, the HUD stays hidden until **both** clear. The two channels
  are independent booleans; neither overrides the other to *force-show* the HUD.
- **Ultrawide / non-16:9 resolutions:** anchors are validated at 16:9; 21:9 safe-area is an
  open playtest item (HUD direction Parte E). Until validated, corner overlays may sit closer
  to the visible edge than intended on ultrawide.
- **Reference vs. target resolution mismatch:** authoring against 1280×720 is correct; it is
  not a bug that the shipping target is 1920×1080 — the percentage anchors reconcile them.

---

## What Persists in the Save

- **Nothing HUD-specific in v1.** The HUD is a pure projection of live game state; it holds no
  authoritative data and writes no save section. HUD ViewModels are rebuilt from events and
  from the domain systems on load.
- **System-tab options (F56, future)** — volume levels and the fullscreen/resolution choice —
  persist as **simple values** (floats for volumes, enum/string for window mode, ints for
  resolution) per ADR-0006 (no Unity refs). They are **player settings**, not part of the
  gameplay `GameSaveData` snapshot, and their persistence is owned by F56.
- **Modal state** (open tabs, scroll positions) is owned by `ui_modal_rules.md`, not here.

---

## What Tests Must Cover

Aligned with `.claude/rules/testing-quality-gate.md` (UI section):

- **EditMode (pure C# projections):**
  - ViewModel state derivation: each input game-state maps to the expected projection
    (e.g., `ShowMp` toggles correctly; `IsHpLow`/`IsMpLow`/`IsStaminaLow` flip exactly at the
    < 0.25 threshold; `HpPercent`/`MpPercent`/`StaminaPercent` computed correctly, including the
    max == 0 guard).
  - Active-skill-slot projection is capped at 4 and never exposes more.
  - Toast queue logic: single-active + FIFO; `Important` preempts; the queue never exceeds the
    intended cap; empty-queue projection is the empty state.
  - Every non-actionable / disabled HUD state carries a reason string (no silent disable).
- **Play Mode / human scenario** (required for the View MonoBehaviours, scene/canvas, anchors):
  - HUD renders, anchors correctly, and stays legible at **1280×720 and 1920×1080**.
  - No gameplay movement is blocked by the HUD; the center stage stays clear.
  - HUD hides on modal open and returns on close (`HudVisibilityChangedEvent`).
  - Floating damage numbers appear above the correct target with the correct color
    (red / blue / gray) and fade out.
  - Toasts appear above the hotbar, respect the max-3 cap, and survive a modal open/close.
  - System tab shows exactly the v1 options (volumes, fullscreen/windowed, resolution) and
    nothing more.
- A human Play Mode scenario for HUD/canvas work goes under
  `docs/validation/playmode/<spec_id>_human_test_scenario.md`, linked from the execution
  report (per the testing quality gate).

---

## Open Questions

- Can the player change the HUD layout? (Current: no; fixed layout in v1 — HUD direction
  Parte E.)
- Is there a per-user UI-scale slider? (Current: no; not in System tab v1 scope — Rule 9.)
- Ultrawide (21:9) safe-area anchoring — open playtest validation (HUD direction Parte E).
- Final wiring of `HudSuppressionChangedEvent` (channel, payload, restore timing) lands with
  `fable_71`; this rule records the contract intent, not the final shape.
- Minimap art is placeholder shapes/colors in v1; real sprites are a later art phase (F38
  out-of-scope; HUD direction Parte E).

---

## Sources

- **Design directions:**
  `docs/design/gameplay/ui_ux/HUD_LAYOUT_SCENES_DIRECTION_v1.0.md` (§0 responsiveness, §1-3 HUD
  layout and minimap, Parte B §4 the eight-tab panel, Parte E pendencies).
- **Binding decisions:**
  `docs/design/FABLE_DECISOES_RESPOSTAS_v2.0.md` 7.2 (1920×1080/16:9, fullscreen + windowed);
  `docs/design/FABLE_DECISOES_RESPOSTAS_v3.0.md` 4.1 (clock pauses on modal — owned by
  `time_rules.md`), 4.2 (fable_71 combat-feel pass; floating numbers already exist;
  `HudSuppressionChangedEvent` to be provided by fable_71), 4.4 (System tab v1 scope).
- **Skill:** `.claude/skills/ui-projection-pattern/SKILL.md` (projection/ViewModel convention).
- **Code (file:line for key values):**
  `Assets/_Game/Scripts/UI/HUD/HUDGameplayViewModel.cs:16-18,33-34,55-60` (MP gating, 4-slot
  cap, low thresholds);
  `Assets/_Game/Scripts/UI/HUD/HudVisibilityController.cs:15-28` (modal-driven visibility);
  `Assets/_Game/Scripts/Core/Events/HudEvents.cs:17-21` (`HudVisibilityChangedEvent`);
  `Assets/_Game/Scripts/UI/HUD/GameplayFeedbackService.cs:54-113` (toast queue, priorities,
  sources);
  `Assets/_Game/Scripts/UI/HUD/GameplayHudCanvasController.cs:60-78` (View attach pattern);
  `Assets/_Game/Scripts/Combat/FloatingDamageNumberDisplayer.cs:16-17,69-70,172-187`
  (animation timing, event subscriptions, color map).
- **Referenced by specs:** `fable_20` (calendar/clock HUD + day detail) and `fable_38`
  (minimap v1), both of which declare `required_game_rules: [ui_rules.md]`.

---

## Cross-References

- [`ui_modal_rules.md`](./ui_modal_rules.md) — the **modal** layer (8-tab panel, ModalManager
  stack, input blocking, Esc/back). This rule covers the complementary **non-modal** HUD layer;
  the HUD's visibility reaction to modals (Rule 7) is the only intentional touch-point.
- [`event_rules.md`](./event_rules.md) — all HUD/feedback events
  (`HudVisibilityChangedEvent`, `HudFeedbackUpdatedEvent`, `DamageAppliedEvent`,
  `PlayerDamagedEvent`, the future `HudSuppressionChangedEvent`) are `GameEventBus` events and
  obey the event DTO and unsubscribe rules.
- [`combat_rules.md`](./combat_rules.md) — owns the damage/status values the floating numbers
  *display*; this rule owns their *presentation*.
- [`time_rules.md`](./time_rules.md) — owns the clock/calendar/lunar values shown by the HUD
  widget and the "clock pauses on modal" behavior (decision v3 4.1).
- [`skill_tree_rules.md`](./skill_tree_rules.md) — owns the 4 active-skill-slot definition
  (keys 1-4) the HUD renders; this rule only fixes how the slot strip is presented.

## Related ADRs

- [ADR-0007: Event Bus Gameplay Communication](../decisions/ADR-0007-event-bus-gameplay-communication.md)
  (HUD/feedback events and modal visibility events flow through the bus).
- [ADR-0006: Save Data Contracts](../decisions/ADR-0006-save-data-contracts-simple-dtos.md)
  (future System-tab option persistence: simple values, no Unity refs).
- [ADR-0010: FABLE Skill and Inventory Rules Reconciliation](../decisions/ADR-0010-fable-skill-and-inventory-rules-reconciliation.md)
  (canonical 4 active slots / keys 1-4 that the HUD slot strip reflects).

---

*Created: 2026-06-13 (referenced by fable_20, fable_38; complements ui_modal_rules.md)*
*Source: HUD_LAYOUT_SCENES_DIRECTION_v1.0, FABLE v2.0 (7.2), FABLE v3.0 (4.1/4.2/4.4), ui-projection-pattern skill, live HUD code*
