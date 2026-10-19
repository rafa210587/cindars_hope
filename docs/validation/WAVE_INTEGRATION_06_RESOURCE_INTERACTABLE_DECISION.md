# WAVE_INTEGRATION_06 Resource Interactable Decision

Date: 2026-06-08
Status: BUILD_VALIDATED_CODE_READY_HUMAN_UNITY_SCENE_ACTION_REQUIRED

## Decision

Create a `FarmResourceInteractable` adapter that implements `IInteractable` for Tree, Rock, and Forage resource smoke validation. Do NOT create new resource runtime services — reuse adapter approach only.

Rationale:
- `FishingSpot` (World/FishingSpot.cs) already fully implements `IInteractable` with `CanInteract(GameObject)` and `Interact(GameObject)`. No additional adapter needed for LakeFishing.
- `TreeNode` (World/TreeNode.cs) already implements `IInteractable` for the existing 19 trees. The FarmResourceInteractable is an additional simplified node for smoke testing only.
- `RockMiningService` and `FarmForageSpawnService` exist as service-layer code but have no scene-placed interactable MonoBehaviour. FarmResourceInteractable fills this gap for smoke validation.
- The adapter is marked `TODO_INTEGRATION_NOT_FINAL` — it must NOT become the final resource interaction system.

## Integration Choice

Chosen strategy: `SMOKE_ADAPTER_REUSE_INVENTORY_PATH`.

- Tree smoke node: `FarmResourceInteractable` with type=Tree, reward=item_wood x2, placed at (7.5, 3.5) inside Zone_ResourceTrees.
- Rock smoke node: `FarmResourceInteractable` with type=Rock, reward=item_stone x2, placed at (-9.0, 4.5) inside Zone_ResourceRocks.
- Forage smoke node: `FarmResourceInteractable` with type=Forage, reward=item_herbs x1, placed at (-8.0, -2.5) inside Zone_Forage.
- LakeFishing: FishingSpot at (7.8, -2.8) handles all lake interaction — no extra component needed.

## Item IDs Used

| Resource | Item ID | Source |
|---|---|---|
| Tree | `item_wood` | TreeDataSO.WoodItemId, TreeChopService |
| Rock | `item_stone` | RockMiningService |
| Forage | `item_herbs` | CropDefinitionData ("crop_herbs") |
| Fish | `item_fish_common` | FishingSpot._fishItemId |

All IDs referenced from existing code. No new item IDs created.

## IInteractable Contract

Actual interface (Assets/_Game/Scripts/Interaction/IInteractable.cs):
```csharp
public interface IInteractable
{
    string InteractionPrompt { get; }
    bool CanInteract(GameObject interactor);
    void Interact(GameObject interactor);
}
```

FarmResourceInteractable correctly implements all three members.

## InventoryManager.AddItem

Actual signature: `public bool AddItem(string itemId, int amount)` — returns bool.
FarmResourceInteractable checks the bool return and logs a warning on inventory-full condition.

## Design/Direction Compliance Matrix

| Direction source | Rule | Implementation |
|---|---|---|
| WAVE05 decision | No new resource runtime systems. | No new services. FarmResourceInteractable is an adapter only. |
| YAML policy | No .unity/.prefab/.asset YAML edits. | CreateMvpFarmScene.cs creates objects via Unity Editor API, not YAML. |
| IInteractable rule | Use existing interaction architecture. | Correctly implements IInteractable with (GameObject interactor) signatures. |
| Inventory reward path | Same as FarmPlot harvest: InventoryManager.AddItem. | Identical reward path used. |
| FishingSpot | Already implements IInteractable. | No duplicate component — FishingSpot used as-is. |

## Gates

WAVE_INTEGRATION_07 gate: CODE_READY. Human must:
1. Run `CindarsHope/Advanced/Legacy/Scenes/Create MVP FarmScene` in Unity Editor.
2. Verify FarmResourceInteractables parent contains TreeResource_01, RockResource_01, ForageResource_01.
3. Walk player to each resource and interact (E key) — verify Debug.Log shows reward added.
4. Execute `CindarsHope/Repair and Validate/Validate Farm Resource Interactables` — verify 0 issues.
5. Walk player to FishingSpot — verify "Pescar" prompt and fishing routine triggers.

Residual gate: Play Mode remains pending human Unity validation.
Can start WAVE_INTEGRATION_07: NO, blocked until human regenerates FarmScene and Play Mode checklist passes.

## P1 Hotfix Decision (2026-06-08)

Bug: Resource depleted before confirming AddItem success.
Decision: Only deplete resource after `AddItem` returns true. Stay Available on null inventory, empty itemId, or AddItem=false.
Additional: Use `ClampedAmount` (min 1) instead of raw Amount.
Validator: Expanded to check resourceType, amount, SpriteRenderer, Collider2D, zone proximity, smoke nodes.
Status: No status change — BUILD_VALIDATED_CODE_READY_HUMAN_UNITY_SCENE_ACTION_REQUIRED.
