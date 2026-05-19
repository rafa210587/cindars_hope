# Core Contracts / Events / Save / IDs v1.1 — Delta FASE 9C

> Complementa `docs/CORE_CONTRACTS_EVENTS_SAVE_IDS_v1.0.md`.
> Fonte funcional detalhada: `docs/FASE9C_TOOLS_FARM_COMBAT_REFINEMENT_v1.0.md`.

## 1. Novos prefixos de ID

| Entidade | Prefixo | Exemplo |
|---|---|---|
| Item de ferramenta | `item_tool_` | `item_tool_axe_basic` |
| ToolDataSO | `tool_` | `tool_axe_basic` |
| Item de arma | `item_weapon_` | `item_weapon_sword_wood` |
| WeaponDataSO | `weapon_` | `weapon_sword_wood` |
| Projectile | `projectile_` | `projectile_arrow_basic` |
| Spell | `spell_` | `spell_spark_basic` |
| Element | `element_` | `element_fire` |

Regra: save sempre grava IDs simples; nunca referência Unity.

## 2. Tool contracts

### ToolType

Valores mínimos:

- `None`
- `Hoe`
- `Sickle`
- `Axe`
- `Pickaxe`
- `FishingRod`
- `WateringCan`
- `ForagingGlove`

### ToolTier

Valores mínimos:

- `Improvised = 0`
- `Basic = 1`
- `Copper = 2`
- `Iron = 3`
- `Steel = 4`
- `Arcane = 5`

### ToolDataSO

Campos persistíveis por ID indireto:

- `Id`
- `Item.Id`
- `ToolType`
- `Tier`
- `ActionPower`
- `ActionCooldownSeconds`
- `EfficiencyMultiplier`
- `YieldMultiplier`
- `FlatYieldBonus`
- `HungerCostMultiplier`

### ToolRequirement

Campos:

- `RequiredToolType`
- `MinimumTier`
- `AllowFallbackWithoutTool`
- `FallbackItemId`
- `FallbackAmount`
- `FallbackConsumesAction`
- `FallbackProgressesTarget`
- `FailureMessage`

## 3. Weapon contracts

### WeaponType

Valores obrigatórios:

- `Melee`
- `RangedPhysical`
- `RangedMagic`

### WeaponDataSO

Campos mínimos:

- `Id`
- `Item.Id`
- `WeaponType`
- `Damage`
- `Range`
- `CooldownSeconds`
- `KnockbackForce`
- `ProjectilePrefabId`
- `ProjectileSpeed`
- `ProjectileLifetimeSeconds`
- `AmmoItemId`
- `AmmoCost`
- `MpCost`
- `HungerCost`
- `ElementId`

## 4. Save contract

Adicionar em `GameSaveData`:

```csharp
public EquipmentSaveData Equipment = new EquipmentSaveData();
```

Novo tipo:

```csharp
[Serializable]
public class EquipmentSaveData
{
    public string EquippedToolId;
    public string EquippedWeaponId;
}
```

Compatibilidade:

- Se `Equipment == null`, criar estado vazio em load.
- Se `EquippedToolId` for desconhecido, limpar ferramenta equipada e registrar warning.
- Se `EquippedWeaponId` for desconhecido, limpar arma equipada e registrar warning.

## 5. Eventos novos

### ToolEquippedEvent

Payload:

- `ToolId`
- `ToolType`
- `Tier`

### ToolActionResolvedEvent

Payload:

- `ActionId`
- `ToolId`
- `UsedCorrectTool`
- `UsedFallback`
- `ResultItemId`
- `ResultAmount`
- `Message`

### WeaponEquippedEvent

Payload:

- `WeaponId`
- `WeaponType`

### PlayerAttackStartedEvent

Payload:

- `WeaponId`
- `WeaponType`
- `Position`

### PlayerAttackResolvedEvent

Payload:

- `WeaponId`
- `WeaponType`
- `Hit`
- `TargetId`
- `Damage`
- `Position`

### ProjectileSpawnedEvent

Payload:

- `ProjectileId`
- `WeaponId`
- `Position`
- `Direction`

### ProjectileHitEvent

Payload:

- `ProjectileId`
- `TargetId`
- `Damage`
- `Position`

### SpellCastEvent

Payload:

- `SpellId`
- `WeaponId`
- `ElementId`
- `MpCost`
- `Position`

### PlayerDodgeStartedEvent

Payload:

- `Direction`
- `DurationSeconds`

### PlayerDodgeEndedEvent

Payload:

- `Position`

## 6. ActionId recomendados

| Ação | ActionId |
|---|---|
| Preparar solo | `farm_prepare_plot` |
| Plantar | `farm_plant_seed` |
| Colher | `farm_harvest_crop` |
| Cortar árvore | `world_chop_tree` |
| Pescar | `world_fish` |
| Minerar | `world_mine_node` |
| Coletar foraging | `world_forage` |

## 7. Regras de compatibilidade

- Eventos continuam `readonly struct`.
- Payloads usam tipos simples ou structs Unity leves (`Vector2`, `Vector2Int`).
- Nenhum evento carrega `GameObject`, `Transform`, `MonoBehaviour` ou `ScriptableObject`.
- Save continua versionado por `SchemaVersion`.
- IDs devem ser validados por databases no load.
