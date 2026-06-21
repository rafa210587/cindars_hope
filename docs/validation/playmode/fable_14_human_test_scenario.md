# Human Test Scenario — fable_14 UI Canvas Screens Integration

> Spec: `fable_14_spec_ui_canvas_screens_integration_runtime`
> Status: DEFERRED_TO_FINAL_VALIDATION (Play Mode required for UI; not run in this session).
> Scope of this scenario: validate the 4 core screens (Inventory/Equipment/Skill Tree/Quest
> Log) once the Canvas screen views are wired, plus the deterministic gates already shipped
> in this slice (focus order, modal pause gate, skill-purchase confirmation, quest spoiler).

This session shipped the deterministic, EditMode-tested core of the spec
(`UiFocusController`, `ModalPauseGate`, `GameplayScreensPanelModel`, `SkillNodePurchaseFlow`,
`QuestLogSpoilerProjection`). The Canvas screen views and the ShopCanvas/RuntimeUiBuilder
refactor remain to be built/validated in Unity. Run this scenario when those land.

## Preconditions

- Open `FarmScene` (or `TownScene`) in the Unity Editor and enter Play Mode.
- Have a debug loadout with at least a few inventory items and 1 unspent skill point.

## 1. Open / Close (CA-1)

- [ ] Press `I` → Inventory tab opens as a Canvas panel (not IMGUI debug).
- [ ] Press `K` → panel opens on the Equipment tab.
- [ ] Press `U` → panel opens on the Skills tab.
- [ ] Press `J` → panel opens on the Quest Log tab.
- [ ] Press `Tab` repeatedly → tabs cycle through the 9 tabs in order
      (Inventory → Equipment → Skills → Quests → Social → Bestiary → Calendar → Map → System)
      and wrap back to Inventory.
- [ ] Placeholder tabs (Social/Bestiary/Calendar/Map/System) show an explicit "Em breve."
      message, not a blank panel.

## 2. Esc / Back (CA-1)

- [ ] With a screen open, `Esc` closes the top of the stack (the panel).
- [ ] With a confirmation sub-modal open (e.g. drop-item confirm), `Esc` closes only the
      confirmation and returns to the underlying tab — it does NOT close the whole panel.

## 3. Input blocking — no movement with a modal open (Anti-regression)

- [ ] With any tab open, hold `W/A/S/D` → the player does NOT move.
- [ ] Attack/dash/hotbar inputs are ignored while a tab is open.
- [ ] After closing all tabs, movement and combat resume normally.

## 4. Focus order (CA-2)

- [ ] Arrow keys / `Tab` move a visible focus outline through the elements in a deterministic
      linear order; it wraps at both ends.
- [ ] `Enter`/`Space`/`E` confirms the focused element.

## 5. Confirmation modal — drop item (CA-2)

- [ ] In Inventory, focus an item, choose Drop → a confirmation appears.
- [ ] Cancelling the confirmation leaves the item in the inventory.
- [ ] Confirming drops the item. (No item is destroyed/dropped without confirmation.)

## 6. Skill node detail before purchase (CA-2)

- [ ] In Skills, selecting a node ALWAYS shows the node detail drawer first.
- [ ] A skill point is NEVER spent on selection alone.
- [ ] Spending requires an explicit confirmation step after the detail drawer.
- [ ] Cancelling at the confirmation step does not spend the point.

## 7. Equipment compare never equips on hover/focus (CA-2)

- [ ] Focusing/hovering an item in the Equipment compare view shows a stat comparison.
- [ ] Focus/hover alone never equips; equipping requires an explicit confirm action.

## 8. Empty states (CA-4)

- [ ] Empty inventory shows "Inventario vazio.".
- [ ] No active quests shows "Nenhuma quest ativa.".
- [ ] No skill points shows "Nenhum ponto de skill disponivel.".

## 9. Quest log spoiler rules (CA-2 / Anti-regression)

- [ ] A hidden/secret quest is listed but its name and objective are masked ("???").
- [ ] A normal quest shows its real name and `progress/target`.

## 10. Day clock pauses with UI open (EMENDA V3, decision 4.1)

- [ ] Note the in-game clock. Open any tab / dialogue / shop → the day clock stops advancing.
- [ ] Close everything → the clock resumes from the exact same point (no jump forward).

## 11. ShopCanvas non-regression (CA-3)

- [ ] Buy and sell flows in the town ShopCanvas behave exactly as before the
      `RuntimeUiBuilder` refactor (item list, price, buy/sell, back).

## Residual risk

- This scenario is NOT executed in the implementing session (no Unity/Play Mode available).
- Until executed, the Canvas screen views, ShopCanvas refactor, the live
  `ModalManager.HasActiveModal → GameTimeManager` pause wiring, and the IMGUI-off flag are
  build-validated only; their runtime behavior is unverified.
