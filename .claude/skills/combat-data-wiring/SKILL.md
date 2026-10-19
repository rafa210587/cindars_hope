---
name: combat-data-wiring
description: Faz wiring de combat data assets — WeaponDatabase, SpellDatabase, StatusEffectDatabase, projectile prefabs. Use em qualquer tarefa que toque combat ScriptableObject databases, spell/weapon/status effect IDs, ou wiring de projectile.
---

# Skill: Wiring de Combat Data

Todas as combat databases herdam de `DataRegistrySO<T>` e são wired no `GameBootstrap`; adicione lookup via database mantendo o fallback de `Resources.Load` para compatibilidade retroativa.

## Quando usar

A tarefa toca:
- `WeaponDatabaseSO`, `SpellDatabaseSO`, `StatusEffectDatabaseSO`
- `WeaponDataSO`, `SpellDataSO`, `StatusEffectSO`
- Referências de projectile prefab (`ProjectilePrefab`)
- `CombatDatabaseValidator`
- Lookup de database do `SpellCastService`
- Lookup de burn SO do `EnemyStatusRuntimeTicker`

## Leitura mínima

1. `CLAUDE.md`
2. Spec alvo
3. `Assets/_Game/Scripts/Core/Data/` — database SOs existentes
4. `Assets/_Game/Scripts/Editor/Validation/CombatDatabaseValidator.cs`

## Pattern: DataRegistrySO

Todas as combat databases herdam de `DataRegistrySO<T>`:

```csharp
[CreateAssetMenu(fileName = "WeaponDatabase", menuName = "CindarsHope/Database/Weapons")]
public class WeaponDatabaseSO : DataRegistrySO<WeaponDataSO> { }
```

Os consumers usam:
```csharp
if (_database.TryGetById(id, out var so))
    // use so
else
    // fallback or warn
```

## Pattern: fallback de Resources.Load

Ao adicionar lookup via database a um service que antes usava Resources.Load:

```csharp
// Try database first; fall back to Resources.Load for backward compat
if (_database != null && _database.TryGetById(id, out var so))
    result = so;
else
    result = Resources.Load<T>(id);
```

Isso é backward-compatible e seguro.

## Pattern: novo database SO

1. Crie `<TypeName>DatabaseSO.cs` em `Assets/_Game/Scripts/Core/Data/`
2. Herde de `DataRegistrySO<<TypeSO>>`
3. Adicione o namespace `CindarsHope.Core.Data`
4. Adicione um campo `[SerializeField]` + public property em `GameBootstrap.cs`
5. Confirmar asmdef e regeneração do projeto Unity correspondente quando necessário
6. Adicione o check do validator em `CombatDatabaseValidator`

## Validator: adicionando check de novo database

Em `CombatDatabaseValidator.ValidateX()`:

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

Use `Warning` para asset ausente (ainda não criado), `Error` para ID não encontrado num DB existente.

## Validação

Selecionar gates pela SPEC_VALIDATION_MATRIX_MASTER; asset-only não exige .NET.
Quando compile .NET for pertinente, usar `tools/unity/Invoke-UnityGeneratedProjectsBuild.ps1`.
Reusar evidência verificada; não repetir build no retorno do subagent.

Phase 2 (Unity): `CindarsHope/Validate/Combat/Validate Combat Databases`

## Regressões comuns

- Não conferir o asmdef/projeto gerado que deve incluir o novo .cs
- Usar `Resources.Load` sem fallback guard quando o database ainda pode não estar atribuído
- Adicionar severity `Error` para asset ausente quando o wiring no Inspector é o próximo passo esperado
- Não testar o null path de `TryGetById`

## Quando parar e reportar

- `DataRegistrySO<T>` não encontrado na codebase — verifique o namespace
- `CombatDatabaseValidator.cs` fora do escopo da spec — documentar como NOT updated
