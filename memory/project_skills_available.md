---
name: project_skills_available
description: Reusable skill prompts and patterns for Cindars Hope development tasks
metadata:
  type: reference
---

# Reusable Skills for Cindars Hope Development

These are proven patterns that can be invoked as skills (Claude Code `/skill` commands or as memory-based patterns).

## Skill: SPEC Validation Pattern

**When to use**: After implementing a SPEC that involves bootstrap, wiring, or integration changes

**Pattern**:
```
1. Run PowerShell validation: .\tools\unity\RunUnityCompileValidation.ps1
2. If errors: read log from offset 960+ (skip licensing noise)
3. Grep/count error patterns: categorize by error code
4. Fix by category (all CS0103 = missing using, all CS1061 = missing property)
5. Commit after validation passes
6. Document any patterns found in feedback memory
```

**Expected outputs**: 
- Zero compiler errors
- Only acceptable warnings (unused variables, deprecated methods)
- Log showing "Tundra build success" and "ExitCode: 0"

---

## Skill: Namespace Consolidation

**When to use**: When encountering type mismatch errors or "cannot convert X to X" messages

**Pattern**:
```
1. PowerShell: Search for all definitions of the conflicting class
   Get-ChildItem Assets/_Game/Scripts -Include "*.cs" -Recurse | ForEach-Object { ... grep class ... }
2. Identify authoritative namespace (usually: Save for DTOs, Core for runtime, specific namespace for domain)
3. Delete duplicate class definitions from non-authoritative namespaces
4. Add `using AuthoritativeNamespace;` to consuming code
5. Use fully qualified names in method signatures: `public void Method(CindarsHope.Save.EquipmentSaveData data)`
6. Run validation
```

**Expected outcome**: Type conversion errors resolved, cleaner inheritance chain

---

## Skill: Bootstrap Integration Pattern

**When to use**: When wiring a new manager/system into GameBootstrap

**Steps**:
```
1. Create manager field in GameBootstrap: `private ManagerType _manager;`
2. Add public property getter
3. Add to Initialize() method: `_manager = new ManagerType(); _manager.Initialize(...);`
4. Add to Shutdown() method: if needed
5. Pass to SaveManager.RebindOptionalRuntimeManagers(..., gameBootstrap._manager, ...)
6. Update scene reference installers to assign GameBootstrap._manager from scene
7. Run validation, confirm no compilation errors
8. Test in Unity Editor: assign via inspector and play a scene
```

**Critical checklist**:
- [ ] Manager has Initialize() and Shutdown() methods (or make them optional)
- [ ] Property is public for external access
- [ ] SaveManager knows how to pass it (check RebindOptionalRuntimeManagers signature)
- [ ] Scene installers updated (FarmSceneRuntimeReferenceInstaller, TownSceneRuntimeReferenceInstaller, CaveSceneRuntimeReferenceInstaller)
- [ ] Editor MVP scene creation scripts updated (CreateMvpFarmScene, CreateMvpTownScene, CreateMvpCaveScene)

---

## Skill: Event Publishing Pattern

**When to use**: When a system needs to broadcast state changes

**Pattern**:
```
1. Define event class in Core/Events/: `public class EventNameEvent { public Type Property1; ... }`
2. Add required using: `using CindarsHope.Core;` and `using CindarsHope.Core.Events;`
3. Publish: `GameEventBus.Publish(new EventNameEvent(...));`
4. Subscribe in listener: `GameEventBus.Subscribe<EventNameEvent>(HandleEvent);`
5. Unsubscribe in OnDisable: `GameEventBus.Unsubscribe<EventNameEvent>(HandleEvent);`
6. Never use GameObject.Find() or FindObjectOfType() - events are decoupled
```

**Anti-patterns**:
- ❌ Direct method calls between systems
- ❌ Storing MonoBehaviour references for late calls
- ❌ FindObjectOfType in Subscribe/Publish code
- ✅ Event-driven coupling only

---

## Skill: Using Directive Organization

**When to use**: When cleaning up imports or adding new dependencies

**Order** (top to bottom):
```csharp
// 1. System namespaces
using System;
using System.Collections.Generic;

// 2. CindarsHope core (always first in project imports)
using CindarsHope.Core;
using CindarsHope.Core.Data;
using CindarsHope.Core.Events;

// 3. CindarsHope feature namespaces
using CindarsHope.Equipment;
using CindarsHope.Inventory;

// 4. Unity
using UnityEngine;

// 5. Third-party/external
using TMPro;
```

**Rule**: Always resolve ambiguity by adding `using CindarsHope.Core;` first - it's the hub.

---

## Skill: DamageRequest Construction

**When to use**: When creating damage requests for enemy damage calculations

**Pattern**:
```csharp
var request = new DamageRequest(
    targetId: enemyHealth.EnemyId,
    baseDamage: _punchDamage,
    damageType: DamageType.Physical,
    sourceId: "", // empty if no source tracking needed
    attributeBonus: 0,
    sourceFlatBonus: 0
);
request.SourcePosition = transform.position;
request.KnockbackForce = _punchKnockbackForce;
TakeDamage(request);
```

**Properties available**:
- `Amount` (read-only, aliases `BaseDamage`)
- `KnockbackForce` (optional, for knockback)
- `SourcePosition` (for direction calculation)
- `CanTriggerVulnerability` (default true)
- `StatusApplicationRules` (for status effects)

---

## Skill: Save/Load Data Pattern

**When to use**: When adding persistence to a system

**Pattern**:
```csharp
// Capture
public SaveDataType CaptureSaveData() {
    return new SaveDataType {
        Property1 = _field1,
        Property2 = _field2 // NEVER: MonoBehaviour, Transform, GameObject
    };
}

// Restore
public void RestoreFromSaveData(SaveDataType data) {
    if (data == null) {
        ResetToDefaults();
        return;
    }
    _field1 = data.Property1;
    _field2 = data.Property2;
}
```

**Critical rule**: Save only simple types (int, string, float, List<T> where T is simple). Never serialize:
- MonoBehaviour references
- Transform/GameObject
- ScriptableObject references (save ID string instead)
- Sprite/Texture2D

---

