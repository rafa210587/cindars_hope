---
doc_type: game_rule
status: accepted
domain: ui-ux
source_adrs: []
source_documents:
  - .specs/implementados/SPEC_28_UI_UX_FULL_GAMEPLAY_CLOSEOUT.md
  - .specs/implementados/SPEC_17_MODAL_SYSTEM.md
last_reviewed: 2026-06-01
---

# UI and Modal Rules

## Purpose

Defines modal stack behavior, input blocking, HUD updates, and UI interaction patterns.

---

## Canonical Rules

### Rule: Modal Stack Architecture

- **Rule:** Modals operate as a stack (LIFO):
  - Only top modal is interactive
  - Modals below are rendered but non-interactive (visually dimmed)
  - ESC key closes top modal; reveals modal below
  - Multiple modals can be open simultaneously (e.g., inventory over main HUD)

- **Stack depth:** Max 5 modals (to prevent UI sprawl)
- **Applies to:** All modal-based UI (inventory, shop, stats, skill tree, etc.)

### Rule: Input Blocking

- **Rule:** When modal is open:
  - Input is blocked from flowing to world/gameplay (no character movement, no attack)
  - Modal receives all input (clicks, keyboard, ESC)
  - Escape key always closes modal (if escape-closable)
  - Clicking outside modal does nothing (modal area only)

- **Applies to:** All modal interactions
- **Exception:** HUD elements always active (health bar visible, no input blocking)

### Rule: ESC Key Behavior

- **Rule:**
  - ESC closes top modal (if modal is open)
  - ESC returns to game world (if no modal open)
  - ESC behavior can be customized per modal (some modals ignore ESC)

- **Applies to:** Modal navigation
- **Constraint:** At least one modal (main HUD) is always open

### Rule: Modal Types and Behaviors

| Modal | Closeable | Stack Depth | Input | Purpose |
|---|---|---|---|---|
| HUD | No | Root | Partial | Always visible stats |
| Inventory | ESC or close button | 1+ | Full block | Item management |
| Equipment | ESC or close button | 1+ | Full block | Equip/unequip |
| Skill Tree | ESC or close button | 1+ | Full block | Skill allocation |
| Shop | ESC or close button | 1+ | Full block | Buy/sell items |
| Stats | ESC or close button | 1+ | Full block | Character sheet |
| Dialogue | Next/confirm button | 1+ | Full block (except dialogue choice) | NPC conversation |
| Combat Log | ESC or close | 1+ | Full block | Combat replay |

### Rule: HUD Elements (Always Visible)

- **Rule:** HUD elements are always visible and partially interactive:
  - Health bar: shows current/max health; no interaction
  - Mana bar: shows current/max mana; no interaction
  - Level/XP: shows current level and XP progress; no interaction
  - Time: shows day/night cycle; no interaction
  - Quick slots: show active skills; can click to use (if no modal blocking)

- **Applies to:** Background UI rendered behind modals
- **Constraint:** HUD never blocks gameplay even when modal open (except quick slots if modal active)

### Rule: Modal Transitions

- **Rule:**
  - Open modal: fade in 0.2 seconds
  - Close modal: fade out 0.2 seconds
  - Multiple modals opening: each animates in sequence
  - No stutter or freeze during transitions

- **Applies to:** Visual polish and responsiveness

### Rule: Modal Data Persistence

- **Rule:** Modal state persists in save:
  - Open modals list (if any are saved; usually modals close on save)
  - Modal content (inventory state, equipment current, skill allocation)
  - Selected tabs or scroll positions (for large modals)

- **Constraint:** Most modals close on game save/exit; some persist (inventory view preference)
- **Applies to:** UX continuity

---

## Modal Content Rules

### Inventory Modal

- **Rule:**
  - Displays 20 slots
  - Can drag/drop items to equip or reorder
  - Right-click item for context menu (use, drop, equip)
  - Search/filter bar at top

### Equipment Modal

- **Rule:**
  - Shows 4 equipped slots (main hand, off hand, armor, accessory)
  - Shows stats bonuses from equipped items
  - Can drag/drop from inventory to equip
  - Can drag equipped item back to inventory (if space)

### Skill Tree Modal

- **Rule:**
  - Shows skill tree graph (nodes linked by prereqs)
  - Shows skill points available and spent
  - Can click node to allocate point (if prereq met)
  - Can drag active skills to active slot bar (5 slots max)

### Shop Modal

- **Rule:**
  - Shows shop inventory (limited items)
  - Shows player gold at top
  - Can click item to buy (confirms purchase)
  - Can sell items from player inventory (right-click)

---

## Open Questions

- Can modals be moved/dragged? (Current: no; fixed positions)
- Can player change HUD layout? (Current: no; fixed layout in MVP)
- Are there custom modal sizes? (Current: fixed sizes; dynamic sizing Batch 2)
- What happens if player presses ESC during a cutscene? (Current: ESC ignored during cutscenes)

---

## Related ADRs

- [ADR-0007: Event Bus Gameplay Communication](../decisions/ADR-0007-event-bus-gameplay-communication.md) (modal open/close events)
- [ADR-0006: Save Data Contracts](../decisions/ADR-0006-save-data-contracts-simple-dtos.md) (modal state persistence)

---

*Last Reviewed: 2026-06-01 (SPEC_DOCS_38)*  
*Source: SPEC_17, SPEC_28*
