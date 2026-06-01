---
name: combat-data-wiring
description: Wire combat data assets — WeaponDatabase, SpellDatabase, StatusEffectDatabase, projectile prefabs
version: 1.0
when_to_use: Any task touching combat ScriptableObject databases, spell/weapon/status effect IDs, or projectile wiring
---

# Combat Data Wiring Skill

## Use When

Task touches:
- `WeaponDatabaseSO`, `SpellDatabaseSO`, `StatusEffectDatabaseSO`
- `WeaponDataSO`, `SpellDataSO`, `StatusEffectSO`
- Projectile prefab references (`ProjectilePrefab`)
- `CombatDatabaseValidator`
- `SpellCastService` database lookup
- `EnemyStatusRuntimeTicker` burn SO lookup

## Required Reads

1. `CLAUDE.md`
2. Target spec
3. `Assets/_Game/Scripts/Core/Data/` — existing database SOs
4. `Assets/_Game/Scripts/Editor/Validation/CombatDatabaseValidator.cs`

## Pattern: DataRegistrySO

All combat databases inherit from `DataRegistrySO<T>`:

```csharp
[CreateAssetMenu(fileName = "WeaponDatabase", menuName = "CindarsHope/Database/Weapons")]
public class WeaponDatabaseSO : DataRegistrySO<WeaponDataSO> { }
```

Consumers use:
```csharp
if (_database.TryGetById(id, out var so))
    // use so
else
    // fallback or warn
```

## Pattern: Resources.Load Fallback

When adding database lookup to a service that previously used Resources.Load:

```csharp
// Try database first; fall back to Resources.Load for backward compat
if (_database != null && _database.TryGetById(id, out var so))
    result = so;
else
    result = Resources.Load<T>(id);
```

This is backward-compatible and safe.

## Pattern: New Database SO

1. Create `<TypeName>DatabaseSO.cs` in `Assets/_Game/Scripts/Core/Data/`
2. Inherit from `DataRegistrySO<<TypeSO>>`
3. Add namespace `CindarsHope.Core.Data`
4. Add `[SerializeField]` field + public property to `GameBootstrap.cs`
5. Add entry to `Assembly-CSharp.csproj`
6. Add validator check in `CombatDatabaseValidator`

## Validator: Adding New Database Check

In `CombatDatabaseValidator.ValidateX()`:

```csharp
var db = AssetDatabase.LoadAssetAtPath<DatabaseSO>(DatabasePath);
if (db == null)
{
    report.AddIssue("Domain", "DB_MISSING", ValidationSeverity.Warning,
        "Database not found. Create asset and wire in GameBootstrap.",
        DatabasePath, "Database", "Create asset");
    return;
}
// then check entries...
```

Use `Warning` for missing asset (not created yet), `Error` for ID not found in existing DB.

## Validation

```powershell
dotnet build .\Assembly-CSharp.csproj --no-restore
dotnet build .\Assembly-CSharp-Editor.csproj --no-restore
```

Phase 2 (Unity): `CindarsHope/Validate/Combat/Validate Combat Databases`

## Common Regressions

- Forgetting to add new .cs to Assembly-CSharp.csproj
- Using `Resources.Load` without fallback guard when database may not be assigned yet
- Adding `Error` severity for missing asset when Inspector wiring is the expected next step
- Not testing `TryGetById` null path

## Stop Conditions

- `DataRegistrySO<T>` not found in codebase — check namespace
- `CombatDatabaseValidator.cs` outside spec scope — document as NOT updated
