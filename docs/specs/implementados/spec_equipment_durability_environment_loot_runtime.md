# SPEC 10 - Equipment Durability, Environment, Loot Runtime

## Status: Implementado parcial

Data completao: 2026-05-24  
Executado por: Claude Code  
Validacao Unity: Completa (return code 0)

## Resumo executivo

SPEC 10 implementa o sistema formal de equipment com 9 slot types, ItemInstanceId tracking para durabilidade, EquipmentDataSO com stats derivados, e loot generation com equipment instances. O sistema e persistido em SaveData v3 com List<EquipmentSlotSaveData> e List<DurabilityEntryData>.

## Escopo entregue

### 1. EquipmentSlot enum

- 9 slot types: None, LeftHand, RightHand, Head, Chest, Legs, Boots, Ring1, Ring2, Accessory
- Arquivo: `Assets/_Game/Scripts/Equipment/EquipmentSlot.cs`

### 2. EquipmentManager expandido

- Dictionary<EquipmentSlot, string> _slots vinculando slots a ItemInstanceId
- Metodos: EquipItem(slot, itemInstanceId), UnequipSlot(slot), GetEquippedItem(slot)
- RegisterEquipmentUsage() sobrecarregado: parameterless para combat generico, com itemInstanceId para durability
- CaptureSaveData() / RestoreFromSaveData() para persistencia
- Subscribe a InventoryChangedEvent para limpeza de bindings invalidos
- DurabilityTracker inicializado em Awake e exposto via propriedade publica
- Arquivo: `Assets/_Game/Scripts/Equipment/EquipmentManager.cs`

### 3. EquipmentDataSO ScriptableObject

- Properties: Id, DisplayName, Description, Icon, BaseValue
- Durability: DurabilityMax (default 100, min 1)
- Stats: StrengthBonus, BaseDefense, BreathBonus, ColdResistance, HeatResistance
- Validacao OnValidate() com min/max
- Arquivo: `Assets/_Game/Scripts/Equipment/EquipmentDataSO.cs`

### 4. DurabilityData e DurabilityTracker

- DurabilityData: gerencia CurrentDurability, MaxDurability, DurabilityPercent, IsBroken, IsLowDurability
- Metodos: TakeDamage(), Repair(), FullRepair()
- EquipmentDurabilityTracker: Dictionary<string, DurabilityData> interno gerenciando durabilities por ItemInstanceId
- Metodos: InitializeEquipment(), GetDurability(), TryRegisterUsage(), RepairEquipment(), RemoveEquipment()
- Save/Load com List<DurabilityEntryData>
- Arquivos: `Assets/_Game/Scripts/Equipment/DurabilityData.cs`, `Assets/_Game/Scripts/Equipment/EquipmentDurabilityTracker.cs`

### 5. Equipment Events

- EquipmentSlotChangedEvent: slot + itemInstanceId payload
- DurabilityChangedEvent: itemInstanceId + current/max durability
- ItemBrokenEvent: itemInstanceId + slot
- ItemRepairedEvent: itemInstanceId + restoredDurability
- Arquivo: `Assets/_Game/Scripts/Core/Events/EquipmentSlotChangedEvent.cs`

### 6. EquipmentHUD minimo

- 9 Image slots posicionais (Head, Chest, Legs, Boots, LeftHand, RightHand, Ring1, Ring2, Accessory)
- UpdateSlotDisplay() mostra cor cinza se vazio, branco se equipado
- Subscription a EquipmentSlotChangedEvent
- Arquivo: `Assets/_Game/Scripts/UI/HUD/EquipmentHUD.cs`

### 7. DerivedStatsCalculator

- Static class com DerivedStats inner contendo: MaxHP, Attack, Defense, MoveSpeed, MaxStamina, StaminaRegen, AttackSpeed, resistencias
- Calculate() method somando equipment bonuses aos stats base
- Iteracao sobre Dictionary<EquipmentSlot, EquipmentDataSO> adicionando StrengthBonus, BaseDefense, resistencias
- Arquivo: `Assets/_Game/Scripts/Player/DerivedStatsCalculator.cs`

### 8. LootTableSO expandido

- Novo array: EquipmentLootEntry[] EquipmentEntries
- Novo class: EquipmentLootData com ItemInstanceId (Guid gerado), ItemId, DurabilityCurrent, DurabilityMax, IsBroken
- TryRollEquipment() method gerando unique ItemInstanceId e inicializando durability = max
- Arquivo: `Assets/_Game/Scripts/Loot/LootTableSO.cs`

### 9. SaveData v3 compativel

- EquipmentSaveData com string EquippedToolId + List<EquipmentSlotSaveData>
- EquipmentSlotSaveData com EquipmentSlot SlotType + string ItemInstanceId
- Persistencia via JsonUtility sem Dictionary (only List)
- EquipmentDurabilitySaveData ja existente adequado via List<DurabilityEntryData>
- Arquivo: `Assets/_Game/Scripts/Save/SaveData.cs`

### 10. SaveManager integrado

- CaptureEquipmentDurabilitySaveData() acessando EquipmentManager.DurabilityTracker propriedade publica
- Restoration em ApplySaveData() chamando DurabilityTracker.LoadFromSaveData()
- Arquivo: `Assets/_Game/Scripts/Save/SaveManager.cs`

## Arquitetura de decisao

1. **EquipmentDurabilityTracker como classe, nao MonoBehaviour**: Permite instancia isolada e gerenciamento de multiplos items. Inicializado em Awake e exposto via propriedade publica.

2. **ItemInstanceId como string vinculo**: Evita serializar referencias diretas a item objects; permite rastrear items durante saves.

3. **Loot generation com Guid.NewGuid()**: Garante uniqueness de cada equipment drop; durability inicializado a max sempre.

4. **DerivedStatsCalculator static**: Util simples sem estado para calculo de stats; podem ser chamados durante gameplay ou editor.

5. **EquipmentHUD minimo Canvas-agnostic**: Usa Image components sem assumir UI Canvas final; UI completa fica para SPEC 17.

## Pendencias deliberadas

- Environmental resistance system (temperature, water exposure): deferred para refinement futuro
- Repair mechanics con RepairKit consumption: implementacao base; balanceamento para SPEC 12+
- UI final Canvas con drag/drop equipment: SPEC 17
- Equipamento visual no personagem: arte/animator, nao code scope SPEC 10
- Status effects como consequence de durability/equipment breaking: SPEC 11

## Validacoes executadas

```
Unity Compile Validation: PASS (return code 0)
- CompileScripts: 959.590ms
- No errors in Asset Pipeline Refresh
- Exiting batchmode successfully with return code 0
```

## Dependencias satisfeitas

- SPEC 00: Documentacao unica de specs ✓
- SPEC 01: Unity compile validation protocol ✓
- SPEC 02: SaveData schema migration ✓
- SPEC 03: Inventory slots e capacity ✓
- SPEC 04: Farm com tools ✓
- SPEC 05: World activities e loot tables ✓
- SPEC 09: GameTime, stamina, hunger ✓

## Bloqueadores removidos para SPEC 11

- ✓ Equipment formal system com slots
- ✓ ItemInstanceId tracking para durability
- ✓ EquipmentDataSO com stats base
- ✓ LootTableSO com equipment generation
- ✓ SaveData v3 schema com equipment persistence

## Proxima fase: SPEC 11

SPEC 11 (Damage/Status/Elements/Resistances) pode proceder com seguranca. O sistema de equipment fornece base de stats para calculo de dano.
