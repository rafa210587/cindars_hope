# WAVE_INTEGRATION_15 — Objective Event Mapping

Date: 2026-06-10

---

## Objective Type → Event Mapping

| ObjectiveType | Trigger Event | How Progress Computed | Idempotency Rule | Status |
|---------------|---------------|----------------------|------------------|--------|
| CollectItem | InventoryChangedEvent (any itemId) | GetItemCount(targetId) clamped to RequiredAmount | Progress = min(inventory, required); completed when count >= required | ACTIVE |
| CraftItem | ItemCraftedEvent (itemId) | One-shot: mark complete when item crafted | MarkObjectiveComplete called once; objective.IsCompleted guard | ACTIVE |
| DeliverItem | TurnIn flow (manual) | Implicit: TurnIn consumes items if objective requires it | Turn-in only allowed when CanTurnIn = true | DEFERRED — no auto-track |
| TalkToNpc | NpcInteractionStartedEvent | Would mark TalkToNpc objective complete | Not wired in QuestProgressEventBridge yet | DEFERRED |
| DefeatEnemy | EnemyKilledEvent | Would check enemy family/type | KILL_OBJECTIVE_DEFERRED — combat runtime not ready | DEFERRED |
| UseItem | ItemUsedEvent | Would check itemId matches target | Not wired | DEFERRED |
| ReachLocation | (no event) | Would check location visited | Not wired | DEFERRED |

---

## Event Bridge Status

| Event | Subscribed | Action | Note |
|-------|-----------|--------|------|
| InventoryChangedEvent | YES | OnInventoryChanged(itemId) → CheckAllActiveQuestsForItem | Covers CollectItem |
| ItemCraftedEvent | YES | OnItemCrafted(itemId) → MarkObjectiveComplete for CraftItem | Covers CraftItem |
| NpcInteractionStartedEvent | NO | Would cover TalkToNpc | DEFERRED |
| EnemyKilledEvent | NO | Would cover DefeatEnemy | DEFERRED — KILL_OBJECTIVE_DEFERRED |
| ItemUsedEvent | NO | Would cover UseItem | DEFERRED |
