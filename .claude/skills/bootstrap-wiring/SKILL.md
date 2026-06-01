---
name: bootstrap-wiring
description: Wire new managers, databases, and services into GameBootstrap with serialized refs and scene repair
version: 1.0
when_to_use: Any task that adds a new manager, database SO, or service requiring GameBootstrap wiring
---

# Bootstrap Wiring Skill

## Use When

Task touches:
- `GameBootstrap.cs` (adding fields or properties)
- Manager instantiation / injection
- ScriptableObject database references
- Scene creators (`FarmSceneCreator`, `TownSceneCreator`, `CaveSceneCreator`)
- Any new system that other systems access via `GameBootstrap.Instance`

## Required Reads

1. `CLAUDE.md`
2. Target spec
3. `Assets/_Game/Scripts/Core/Bootstrap/GameBootstrap.cs` (current state)

## Do Not Read By Default

```
All scenes
All prefabs
Full architecture docs
```

## Procedure

### Adding a New Database or Manager to Bootstrap

1. Add `[SerializeField] private <TypeSO> _<field>;` after related existing field
2. Add `public <TypeSO> <Property> => _<field>;` after related existing property
3. Commit: the Inspector field will be null until wired in Unity Editor
4. Add note to execution report:
   ```
   Inspector wiring required: assign <TypeSO>.asset in GameBootstrap inspector
   ```

### Consumer Wiring (accessing from a MonoBehaviour)

Preferred pattern:
```csharp
private void Start()
{
    var bootstrap = GameBootstrap.Instance;
    _database = bootstrap?.SpecificDatabase;
    if (_database == null)
        Debug.LogWarning("Missing database wiring in GameBootstrap", this);
}
```

**NEVER use:**
```csharp
// PROHIBITED
var bootstrap = FindObjectOfType<GameBootstrap>();
var bootstrap = GameObject.Find("Bootstrap").GetComponent<GameBootstrap>();
```

### Scene Creator Updates

If a scene creator needs the new manager:
1. Find the scene creator for the relevant scene
2. Add the lookup after GameBootstrap resolve
3. Use repair menu `CindarsHope/Repair and Validate Project` to apply

### Editor Validators

If a validator needs to check the new wiring:
1. Find `CombatDatabaseValidator` or similar
2. Add null check and `report.AddIssue(...)` for missing asset
3. Use `ValidationSeverity.Warning` for assets not yet created (expected until Inspector wired)

## Rules

- No `GameObject.Find()` or `FindObjectOfType()` — ever
- Serialized refs only
- Null-check at consumer with clear LogWarning including scene/component/field context
- Inspector wiring is human responsibility; document in execution report

## Validation

After wiring C#:
```powershell
dotnet build .\Assembly-CSharp.csproj --no-restore
dotnet build .\Assembly-CSharp-Editor.csproj --no-restore
```

Expected: 0 errors. New field starts as null — that is expected.

Unity validator (Phase 2, if spec requires):
- `CindarsHope/Validate/Combat/Validate Combat Databases`
- Will show warning for unwired assets

## Common Regressions

- Using `FindObjectOfType<GameBootstrap>()` in consumer
- Adding field but forgetting to add property
- Not documenting Inspector wiring requirement
- Adding to wrong scene creator

## Stop Conditions

- `GameBootstrap.cs` is outside spec scope → stop, report
- Scene YAML would need manual edit → use repair menu instead
