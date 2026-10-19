# WAVE_INTEGRATION_14 — Processing Job Lifecycle

> **Date:** 2026-06-10
> **Implementation:** `CraftingJob.cs` + `CraftingStation.cs` + `CraftingRuntime.cs`

---

## 1. State Machine

```
[IDLE] ──── TryStartCraft(recipe, inventory) ────►
     Ingredients validated + consumed
              │
              ▼
       [InProgress] ── CraftingRuntime.Update(deltaTime) ──► RemainingSeconds decrements
              │                                                │
              │  RemainingSeconds <= 0                        │
              ▼                                               │
       [Completed] ◄────────────────────────────────────────┘
         (HasCompletedOutput = true)
              │
              ├── TryCollectOutput(inventory) → AddItem to inventory → _job = null → [IDLE]
              │
              └── TryCancelJob(inventory) [not allowed from Completed — only from InProgress]

       [InProgress] ── TryCancelJob(inventory) → refund ingredients → _job = null → [IDLE]
```

### State Table

| State | Code Enum | Description |
|---|---|---|
| Queued | `CraftingJobStatus.InProgress` | Job started but not ticked yet (RemainingSeconds > 0) |
| Processing | `CraftingJobStatus.InProgress` | CraftingRuntime.Update() decrementing timer |
| ReadyToCollect | `CraftingJobStatus.Completed` | RemainingSeconds <= 0, Complete() called |
| Collected | `_job = null` | After TryCollectOutput success; job reference removed |
| Cancelled | `CraftingJobStatus.Cancelled` | TryCancelJob succeeded; ingredients refunded |
| Failed | N/A (returns false + failureReason) | Ingredient missing, inventory full, station level too low |

**Note:** `CraftingJobStatus` enum has values: InProgress, Completed, Cancelled.
The `ProcessingJobState` enum (Queued/Processing/ReadyToCollect/Collected/Cancelled/Failed) exists
in the `CindarsHope.Crafting` parallel namespace but is not used by the canonical `CraftingStation`/
`CraftingJob` runtime. The canonical runtime uses `CraftingJobStatus`.

---

## 2. Idempotency Rules

| Rule | Implementation | Evidence |
|---|---|---|
| Cannot collect twice | After TryCollectOutput success: `_job = null` | CraftingStation.TryCollectOutput() line: `_job = null;` |
| Cannot collect if not completed | `HasCompletedOutput` check (Status == Completed) | CraftingStation.TryCollectOutput() → `if (!HasCompletedOutput)` |
| Cannot cancel a completed job | TryCancelJob checks `HasActiveJob` (InProgress only) | `if (!HasActiveJob)` → "Only in-progress job can be cancelled" |
| Cannot start craft if busy | `IsBusy = HasActiveJob || HasCompletedOutput` | CraftingStation.CanStartCraft() → `if (IsBusy)` |
| Rollback on ingredient consume fail | `CaptureSaveData()` + `RestoreFromSaveData()` before consume | CraftingStation.TryStartCraft() lines 81-89 |
| Rollback on output add fail | `RestoreFromSaveData()` if AddItem returns false | CraftingStation.TryStartCraft() lines 90-98 |

---

## 3. Failure Cases

| Failure | When | Behavior |
|---|---|---|
| Recipe is null | TryStartCraft called with null recipe | Returns false, "Recipe is unavailable." |
| Station type mismatch | Recipe.RequiredStationType != station type | Returns false, "Requires {type}." |
| Station level insufficient | Recipe.RequiredWorkshopLevel > StationLevel | Returns false, "Requires station level X." |
| Recipe locked | IsUnlockedByDefault = false | Returns false, "Recipe is locked." |
| Station busy | HasActiveJob or HasCompletedOutput | Returns false, "Station already has a pending job or output." |
| Missing ingredient | inventory.HasItem fails | Returns false, "Missing ingredient {id}." |
| Inventory null | InventoryManager is null | Returns false, "Inventory system is unavailable." |
| Stamina insufficient | StaminaManager.TrySpendStamina fails | Returns false, "Not enough stamina." |
| Ingredient consume fails (atomic) | RemoveItem returns false mid-consume | Rollback via RestoreFromSaveData; returns false |
| Output add fails (inventory full) | AddItem returns false | Rollback via RestoreFromSaveData; returns false, "Inventory is full." |
| Cancel while completed | HasActiveJob = false | Returns false, "Only an in-progress job can be cancelled." |
| Collect while not complete | HasCompletedOutput = false | Returns false, "No completed output to collect." |
| Refund fails on cancel | AddItem returns false during refund loop | Rollback via RestoreFromSaveData; "Inventory has no room to return all ingredients." |

---

## 4. Events Published

All events via `GameEventBus.Publish()` — no direct references.

| Event | When Published |
|---|---|
| `CraftingJobStartedEvent` | Processing job created (non-instant recipe) |
| `CraftingJobCompletedEvent` | Processing job timer reaches 0 |
| `CraftingJobCancelledEvent` | TryCancelJob succeeds |
| `CraftingOutputCollectedEvent` | Output added to inventory (instant or collect) |
| `ItemCraftedEvent` | Output added to inventory (instant or collect) |
| `CraftingFailedEvent` | Any failure with station/recipe context |

---

## 5. Save / Load

Processing jobs survive scene reload via:

```csharp
// Capture (SaveManager calls this)
CraftingRuntimeSaveData CaptureSaveData()

// Restore (SaveManager calls this after load)
void LoadFromSaveData(CraftingRuntimeSaveData saveData)
```

`CraftingJobSaveData` persists:
- JobId, StationInstanceId, RecipeId
- Status (int cast of CraftingJobStatus)
- RemainingSeconds (float)
- OutputItemId, OutputAmount
- IngredientsConsumed (for cancel refund)

**Result: PROCESSING_JOB_SAVE_LOAD = IMPLEMENTED (no debt)**

---

*Generated: WAVE_INTEGRATION_14 (2026-06-10)*
