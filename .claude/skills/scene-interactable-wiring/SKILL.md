---
name: scene-interactable-wiring
description: Add a new IInteractable object to a scene using CreateScene editor scripts, reward strategy, and depletion guard
version: 1.0
when_to_use: Any task adding crops, resources, fishing spots, chests, or other interactable objects to FarmScene, CaveScene, or TownScene
---

# Scene Interactable Wiring Skill

## Use When

Task requires:
- Adding a new interactable object type (tree, rock, forage, chest, spring, fishing spot)
- Wiring reward delivery for player interaction (item drop, currency, XP)
- Adding depletion state (resource depletes after harvest, respawns after time)
- Registering interactable in a CreateScene editor script

## Do NOT Use When

- Interactable is NPC dialogue/shop → use `npc-dialogue-authoring`
- Interactable is a UI modal → use `ui-modal-stack`
- Change is purely to ScriptableObject data (no runtime behavior) → use `combat-data-wiring` or `bootstrap-wiring`

## Required Reads

1. `CLAUDE.md`
2. Target spec
3. Relevant prior interactable report (e.g., `docs/validation/WAVE_INTEGRATION_06_RESOURCE_INTERACTABLE_REPORT.md`)

---

## Core Architecture

### IInteractable Contract

```csharp
public interface IInteractable
{
    bool CanInteract { get; }
    string InteractionPrompt { get; }
    void Interact(GameObject interactor);
}
```

**Always implement this interface.** Do not create parallel interaction systems.

### Existing interactables to reuse or extend

| Type | Class | File |
|------|-------|------|
| Crop | `FarmPlot` | `Assets/_Game/Scripts/Farm/FarmPlot.cs` |
| Tree | `TreeResourceInteractable` | `Assets/_Game/Scripts/Farm/Interactables/` |
| Rock | `RockResourceInteractable` | same |
| Forage | `ForageResourceInteractable` | same |
| Fishing | `FishingSpot` | `Assets/_Game/Scripts/Farm/FishingSpot.cs` |

If the new type is structurally identical to one of these, **extend** it (via subclass or config parameter). Do NOT create a parallel class.

---

## Reward Strategy

### Canonical reward path

```csharp
// Always go through InventoryManager.AddItem
var inventory = GameBootstrap.Instance?.InventoryManager;
if (inventory == null) return;

var added = inventory.AddItem(itemId, amount);
if (added)
{
    SetDepleted(true);   // only deplete if AddItem succeeded
    PublishFeedback(itemId, amount);
}
```

**Rule**: Depletion happens ONLY on `AddItem` success. If the inventory is full or item ID is invalid, the resource is NOT depleted — player can try again.

### ClampedAmount guard

```csharp
var actualAmount = Mathf.Clamp(amount, 1, maxStack);
var added = inventory.AddItem(itemId, actualAmount);
```

Always clamp amount. Never pass unclamped values.

---

## Depletion State

```csharp
public bool IsDepleted { get; private set; }

public bool CanInteract => !IsDepleted;

private void SetDepleted(bool depleted)
{
    IsDepleted = depleted;
    // Optional: visual update (hide sprite, show stump, etc.)
    UpdateVisual(depleted);
    
    if (depleted && _respawnTime > 0f)
        StartCoroutine(RespawnAfterDelay(_respawnTime));
}

private IEnumerator RespawnAfterDelay(float seconds)
{
    yield return new WaitForSeconds(seconds);
    SetDepleted(false);
}
```

Respawn time is `TODO_INTEGRATION_NOT_FINAL` unless spec defines it.

---

## Feedback

```csharp
private void PublishFeedback(string itemId, int amount)
{
    GameEventBus.Publish(new PlayerActionFeedbackEvent($"Coletado: {itemId} x{amount}"));
    // If item pickup event exists:
    // GameEventBus.Publish(new ItemPickedUpEvent(itemId, amount));
}
```

---

## CreateScene Wiring (NOT scene YAML)

**Never edit `.unity` YAML directly.** Register new interactables in the relevant `CreateScene` editor script:

```csharp
// In CreateMvpFarmScene.cs (or equivalent)
private void CreateResourceInteractables()
{
    CreateTreeResource("TreeResource_01", new Vector2(3f, -2f));
    CreateTreeResource("TreeResource_02", new Vector2(5f, -4f));
    // Add new interactable here:
    CreateRockResource("RockResource_01", new Vector2(8f, -1f));
}

private void CreateTreeResource(string name, Vector2 position)
{
    var go = new GameObject(name);
    go.transform.position = position;
    go.transform.SetParent(_resourceContainer);
    var interactable = go.AddComponent<TreeResourceInteractable>();
    interactable.Configure("item_material_wood", amount: 3, respawnTime: 120f);
}
```

After adding to CreateScene, document in wiring instructions:
```
docs/validation/WAVE_INTEGRATION_<N>_HUMAN_UNITY_<SLUG>_WIRING_INSTRUCTIONS.md
```

---

## When to Reuse FishingSpot vs. Create New

Use existing `FishingSpot.cs` if:
- Interaction is "stand near water and press interact"
- Reward is fish items
- Position is a fixed water-adjacent point

Create new class if:
- Interaction mechanic is fundamentally different (minigame, timed, etc.)
- Reward type is completely different
- Spec explicitly requires new class

---

## Interaction Trigger (Player side)

The player interaction system reads `IInteractable.CanInteract` and calls `Interact()`. Verify the player has:
```csharp
// PlayerInteractionController or equivalent
if (_nearbyInteractable != null && _nearbyInteractable.CanInteract)
{
    if (Input.GetKeyDown(KeyCode.E) || Input.GetKeyDown(KeyCode.F))
        _nearbyInteractable.Interact(gameObject);
}
```

If the player interaction controller doesn't exist yet → document:
```
PLAYER_INTERACTION_CONTROLLER_DEBT
```

---

## Debt Tags

```
TODO_INTEGRATION_NOT_FINAL
RESPAWN_TIME_BALANCE_PENDING
DEPLETION_SAVE_DEFERRED          — depleted state not persisted across sessions
PLAYER_INTERACTION_CONTROLLER_DEBT
INTERACTABLE_ANIMATION_DEFERRED  — no depletion visual yet
```

---

## Validation

```powershell
dotnet build .\Assembly-CSharp.csproj --no-restore
if ($LASTEXITCODE -ne 0) { Write-Host "BUILD FAILED"; exit 1 }
dotnet build .\Assembly-CSharp-Editor.csproj --no-restore
if ($LASTEXITCODE -ne 0) { Write-Host "EDITOR BUILD FAILED"; exit 1 }
```

Human Play Mode checklist must cover:
- Walk up to interactable → prompt appears
- Press interact key → item added to inventory
- Resource depletes after harvest
- Trying to interact with depleted resource → nothing happens
- If respawn configured → resource reappears after delay
- Full inventory → resource NOT depleted

---

## Common Regressions

- Depleting on `AddItem` call, not on `AddItem` success → item lost if inventory full
- Forgetting `ClampedAmount` guard → stack overflow on `AddItem`
- Creating new interaction controller instead of implementing `IInteractable`
- Editing scene YAML instead of CreateScene script
- Not publishing feedback event → player has no indication of what happened

## Stop Conditions

- Scene YAML must be edited and no CreateScene script exists → `CODE_READY_HUMAN_UNITY_SCENE_ACTION_REQUIRED`
- New interactable type requires new physics layer or tilemap → `BLOCKED` (requires ProjectSettings)
- Spec requires save/load of depletion state → use `save-load-pattern` skill in addition
