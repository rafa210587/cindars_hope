# WAVE_INTEGRATION_23 — Modal Visibility Guard Matrix

**Date:** 2026-06-10

---

## HUD Visibility When Modal Is Active

When any modal is open (`ModalManager.HasActiveModal == true`), the HUD should hide.

| Modal | HUD Visible? | Mechanism |
|---|---|---|
| `ModalType.None` (no modal) | ✓ YES | HudVisibilityController: `!HasActiveModal` = true |
| `ModalType.Dialogue` | ✗ NO | HudVisibilityController polls → publishes `HudVisibilityChangedEvent(false)` |
| `ModalType.QuestOffer` | ✗ NO | same |
| `ModalType.ShopMenu` | ✗ NO | same |
| `ModalType.Buy` | ✗ NO | same |
| `ModalType.Sell` | ✗ NO | same |
| `ModalType.Crafting` | ✗ NO | same |
| `ModalType.Inventory` | ✗ NO | same |
| `ModalType.CorpseRecovery` | ✗ NO | same |
| `ModalType.AnyaFountain` | ✗ NO | same |
| `ModalType.SkillTree` | ✗ NO | same |
| `ModalType.CharacterEquipment` | ✗ NO | same |
| `ModalType.Pause` | ✗ NO | same |
| `ModalType.Death` | ✗ NO | same |
| `ModalType.CaveCheckpoint` | ✗ NO | same |
| `ModalType.QuestLog` | ✗ NO | same |

---

## HudVisibilityController: Polling Contract

```
Update() each frame:
  nowVisible = (ModalManager == null) || !ModalManager.HasActiveModal
  if nowVisible != _wasVisible:
    _wasVisible = nowVisible
    GameEventBus.Publish(HudVisibilityChangedEvent(nowVisible))
```

**Frequency:** Only publishes on transition (not every frame).

**Null-safe:** If `ModalManager` not yet available (scene loading), `nowVisible = true` (HUD shown by default).

---

## View Response to HudVisibilityChangedEvent

| View | Response |
|---|---|
| `StatusBarsHudView` | `gameObject.SetActive(evt.IsVisible)` |
| `QuestTrackerHudView` | `gameObject.SetActive(evt.IsVisible)` |
| `ActiveSkillSlotsHudView` | `gameObject.SetActive(evt.IsVisible)` |
| `InteractionPromptHudView` | `gameObject.SetActive(evt.IsVisible)` |
| `ModalBlockerHudView` | Sets `_hudVisible` flag; Canvas group alpha wired by human |
| `FeedbackToastHudView` | Does NOT respond — feedback persists during modals (informational) |

---

## Invariant

```
When ModalManager.HasActiveModal == true:
  - StatusBarsHudView.gameObject.activeSelf == false
  - QuestTrackerHudView.gameObject.activeSelf == false
  - ActiveSkillSlotsHudView.gameObject.activeSelf == false
  - InteractionPromptHudView.gameObject.activeSelf == false
  - ModalBlockerHudView.IsBlocking == true
When ModalManager.HasActiveModal == false:
  - All views active == true
  - ModalBlockerHudView.IsBlocking == false
```

---

## Residual Risk

**Human wiring needed:** `ModalBlockerHudView` controls `_hudVisible` but the actual CanvasGroup alpha change requires a Canvas hierarchy that only exists after human Unity Editor work. Until then, `SetActive(false)` on the individual view GameObjects is the effective gate.

---

*Created: 2026-06-10 (WAVE23)*
