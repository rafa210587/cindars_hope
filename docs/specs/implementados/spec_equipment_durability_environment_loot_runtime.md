# SPEC - Equipment durability, environment, loot runtime

> Spec ID: spec_equipment_durability_environment_loot_runtime
> Status: Implementado parcial
> Ordem de execucao: 10
> Data: 2026-05-24
> Evidencia: Assets/_Game/Scripts/Equipment/DurabilityManager.cs, EquipmentManager.cs, EquipmentDataSO.cs, EnvironmentalResistanceManager.cs

## Resumo de Implementacao

### Implementado:
- `DurabilityManager` para rastrear e decrementar durabilidade de itens
- `EquipmentManager` para equip/unequip e binding
- `EquipmentDataSO` com stats e campos de equipamento
- `EnvironmentalResistanceManager` para resistencias
- Durability repair hooks
- Save/load de durabilidade
- EquipmentSaveData para persistencia

### Nao implementado (futuro):
- Durability degradation durante combat
- Environmental damage
- Repair costs/mechanics
- Qualidade de items crafted
- Affixes/modifiers
- Complete loot table system

### Validacao pendente:
- Unity compile validation
- Play mode test de equipment durability
