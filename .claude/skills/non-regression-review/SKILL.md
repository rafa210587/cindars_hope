---
name: non-regression-review
description: Audit implementation diff for architectural violations and regression risks
version: 1.0
---

# Non-Regression Review Skill

Use to audit changes for violations of project rules and architectural patterns.

## When to Review

- After implementation of spec
- Before task closeout
- Whenever significant scope or rules changed
- As sanity check before user approval

## Mandatory Audit Items

### 1. File & Directory Structure

```
Check git diff --name-only for:
```

- [ ] No creation of root-level `specs/` directory
- [ ] No creation of root-level `spec/` directory
- [ ] No edits to `docs_old/**` (archive only)
- [ ] All changes within permitted scope

**Action:** If violated, undo changes and re-implement within scope.

### 2. Git Safety

```
Check git log and git status:
```

- [ ] No `git push` executed (future push awaits user approval)
- [ ] No `git reset --hard` executed
- [ ] No `git clean` executed
- [ ] No `git stash` executed
- [ ] Branch clean or only has intended commits

**Action:** Restore from backup if destructive op executed.

### 3. Runtime/Gameplay Safety (if C# changed)

```
Grep for violations:
```

- [ ] No `GameObject.Find()` calls (use GameEventBus or Bootstrap references)
- [ ] No `FindObjectOfType()` calls
- [ ] No `FindObjectsByType()` calls
- [ ] No direct MonoBehaviour-to-MonoBehaviour calls (use GameEventBus.Publish/Subscribe)

**Pattern violation example:**
```csharp
// ❌ WRONG
var enemy = FindObjectOfType<EnemyHealth>();
enemy.TakeDamage(damage);

// ✅ RIGHT
GameEventBus.Publish(new DamageAppliedEvent 
{ 
  TargetId = targetId, 
  DamageAmount = damage 
});
```

**Action:** Refactor to use GameEventBus.

### 4. Save Data Safety

- [ ] Save does NOT serialize `ScriptableObject` refs
- [ ] Save does NOT serialize `GameObject` refs
- [ ] Save does NOT serialize `Transform` refs
- [ ] Save does NOT serialize `MonoBehaviour` refs
- [ ] Save does NOT serialize `Sprite` refs
- [ ] Save does NOT serialize `Collider` refs
- [ ] Save does NOT serialize `Rigidbody` refs
- [ ] Save uses IDs and simple types (int, string, float, bool)
- [ ] Save uses `Application.persistentDataPath` (not `StreamingAssets`)

**Pattern violation example:**
```csharp
// ❌ WRONG
[System.Serializable]
class ItemSaveData
{
    public ItemDataSO itemData; // Serializing ScriptableObject!
    public Transform dropTransform; // Serializing Transform!
}

// ✅ RIGHT
[System.Serializable]
class ItemSaveData
{
    public int itemId; // ID only
    public float dropPositionX, dropPositionY;
}
```

**Action:** Refactor save structure to use IDs only.

### 5. Game Data in Code

- [ ] No hardcoded balancing numbers in MonoBehaviour
- [ ] All game data in ScriptableObject with proper prefix (ItemDataSO, WeaponDataSO, etc.)
- [ ] ScriptableObjects properly referenced from `Assets/_Game/Data/`

**Pattern violation example:**
```csharp
// ❌ WRONG
public class PlayerHealth : MonoBehaviour
{
    public float maxHealth = 100f; // Hardcoded!
}

// ✅ RIGHT
public class PlayerHealth : MonoBehaviour
{
    [SerializeField] private PlayerDataSO playerData;
    public float maxHealth => playerData.MaxHealth;
}
```

**Action:** Move data to ScriptableObject.

### 6. Event Bus Usage

- [ ] Gameplay communication uses `GameEventBus.Publish()`
- [ ] All event subscribers have `Unsubscribe()` in `OnDisable` or `OnDestroy`
- [ ] Events have proper prefix: `*Event` (DayStartedEvent, ItemCraftedEvent, etc.)
- [ ] Events carry payload, not references to systems

**Pattern violation example:**
```csharp
// ❌ WRONG
public class ItemManager : MonoBehaviour
{
    public void OnItemUsed(ItemDataSO item)
    {
        GetComponent<PlayerStats>().AddExperience(item.ExpGain);
    }
}

// ✅ RIGHT
public class ItemManager : MonoBehaviour
{
    public void OnItemUsed(int itemId)
    {
        GameEventBus.Publish(new ItemUsedEvent { ItemId = itemId });
    }
}
```

**Action:** Refactor to publish events.

### 7. Spec Execution Order

- [ ] Did not implement specs ahead of SPEC_EXECUTION_ORDER.md blockers
- [ ] Did not skip specs in dependency chain
- [ ] Consulted SPEC_EXECUTION_ORDER.md before starting

**Action:** Verify dependencies in `.specs/SPEC_EXECUTION_ORDER.md`.

### 8. Namespace Safety

- [ ] No namespace called `CindarsHope.Debug` created
- [ ] Used alternatives: `CindarsHope.Runtime`, `CindarsHope.DebugTools`, `CindarsHope.Diagnostics`, or `CindarsHope.Editor`

**Action:** Rename any forbidden namespaces.

### 9. Status & Documentation Integrity

- [ ] No spec marked as implemented without evidence in repo
- [ ] IMPLEMENTATION_STATUS.md claims match actual code/asset state
- [ ] PROJECT_LOG.md updated when task was significant
- [ ] No orphaned entries in registries

**Action:** Provide evidence or remove claim.

## Audit Output Format

```text
Non-Regression Audit Report
───────────────────────────

Status: PASS | WARNING | FAIL

File & Directory Structure:
  ✓ No root specs/ or spec/ created
  ✓ No docs_old/** edits
  ✓ All changes within scope

Git Safety:
  ✓ No destructive operations
  ✓ Branch clean

Runtime Safety (C# changes):
  ✓ No GameObject.Find() or FindObjectOfType()
  ✓ No direct system calls
  ✓ GameEventBus used for gameplay communication

Save Data (if persistence task):
  ✓ No Unity refs serialized (only IDs and simple types)
  ✓ Using Application.persistentDataPath

Game Data:
  ✓ No hardcoded balancing in MonoBehaviour
  ✓ All game data in ScriptableObjects

Events:
  ✓ Proper event prefix (* Event)
  ✓ Unsubscribe in OnDisable/OnDestroy

Spec Order:
  ✓ Respects SPEC_EXECUTION_ORDER.md

Namespaces:
  ✓ No forbidden CindarsHope.Debug namespace

Status & Docs:
  ✓ IMPLEMENTATION_STATUS.md has evidence
  ✓ PROJECT_LOG.md current
  ✓ No orphaned claims

Issues found:
  (if any)

Corrective actions required:
  (if any)

Residual risk:
  (if any)
```

## Examples

### PASS

```text
Status: PASS

Summary:
- 3 files changed: PlayerCombatManager.cs, WeaponDataSO.cs, Player.cs
- Scope: SPEC 12 (approved)
- No violations detected
- All patterns followed

Ready for task closeout.
```

### WARNING

```text
Status: WARNING

Issues found:
  - DamageAppliedEvent now carries Transform (was int TargetId before)
  - Implies event may be serialized with Transform ref

Corrective actions required:
  Change DamageAppliedEvent.TargetTransform → DamageAppliedEvent.TargetPositionId (int)
  Verify save does not serialize this event

Residual risk:
  Minor: Runtime may serialize Transform unexpectedly
```

### FAIL

```text
Status: FAIL

Issues found:
  - Found: GetComponent<EnemyHealth>().TakeDamage() in PlayerCombat.cs:42 (direct call!)
  - Found: Save serializing WeaponDataSO reference in EquipmentSaveData
  - Found: Hardcoded maxHealth = 100f in PlayerHealth.cs

Corrective actions REQUIRED:
  1. Refactor PlayerCombat.GetComponent call → Use GameEventBus.Publish
  2. Change EquipmentSaveData to store weaponId (int) instead of WeaponDataSO
  3. Move maxHealth to PlayerDataSO

Residual risk:
  CRITICAL: Task cannot proceed to closeout until these are fixed.
```

## Rules

- [ ] Do NOT claim PASS without checking all 9 items
- [ ] Do NOT ignore WARNING (early signs of larger problems)
- [ ] Do NOT accept FAIL without fixing
- [ ] Do NOT hide violations in summary
- [ ] Do NOT claim compliance without evidence

## Integration

- **Spec Execution** → Calls this before Phase 4: Closeout
- **Implementation Closeout** → Requires PASS/WARNING/FAIL result
- **Finish-Spec** → Cannot complete without this audit
