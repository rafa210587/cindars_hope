# refinamento_init_equipment_durability_environment_loot

> **Status:** Refinamento inicial a implementar  
> **Spec futura sugerida:** `spec_equipment_durability_environment_loot_runtime.md`  
> **Objetivo:** completar equipment, durabilidade, resistência ambiental e loot runtime.

---

## 1. Estado atual

`EquipmentManager` é parcial/debug: guarda tool equipada, weapon equipada e infere tool por ID. Há dados/managers iniciais criados na wave overnight para equipment, durability, environmental resistance e loot, mas ainda não formam runtime completo.

Evidência:

```text
Assets/_Game/Scripts/Equipment/EquipmentManager.cs
Assets/_Game/Scripts/Equipment/EquipmentDataSO.cs
Assets/_Game/Scripts/Equipment/DurabilityManager.cs
Assets/_Game/Scripts/Equipment/EnvironmentalResistanceManager.cs
Assets/_Game/Scripts/Loot/LootTableSO.cs
docs/specs/implementados/spec_tools_001_tools_equipment_hotbar_parcial.md
docs/specs/a_implementar/spec_fase9h_loot_crafting_equipment_durability_environment.md
```

---

## 2. Gaps

- EquipmentManager não aplica stats de equipamento no player.
- Equipamento não possui slots formais: weapon, armor, ring, tool, accessory etc.
- Durabilidade não é consumida por ataque/defesa/tool use.
- Não há repair flow.
- Resistência ambiental não afeta cave/bioma/status.
- LootTableSO não está integrado de forma unificada a inimigos/resources/árvores/pesca.
- Save/load de equipment/durability ainda é parcial.

---

## 3. Escopo esperado

### Equipment slots

```text
Weapon
Tool
Head
Chest
Legs
Boots
Ring1
Ring2
Accessory
```

### Stats

Equipamentos devem poder modificar:

```text
MaxHP
Attack
Defense
MoveSpeed
Stamina
Mana
ElementalResistance
EnvironmentalResistance
```

### Durability

- Perde durabilidade em tool use/combat.
- Item quebra ou fica inativo em 0 durability.
- Repair consome material/gold.
- Durability salva/carrega por item instance ou slot.

### Environment

- Cave bands podem exigir resistência ambiental.
- Falta de resistência aplica debuff/damage over time/slow.

### Loot

- LootTableSO deve ser usado por pelo menos enemy/resource/world activity.
- Suportar peso, min/max amount, chance e condições.

---

## 4. Arquivos prováveis

```text
Assets/_Game/Scripts/Equipment/EquipmentManager.cs
Assets/_Game/Scripts/Equipment/EquipmentDataSO.cs
Assets/_Game/Scripts/Equipment/EquipmentSlot.cs
Assets/_Game/Scripts/Equipment/DurabilityManager.cs
Assets/_Game/Scripts/Equipment/EnvironmentalResistanceManager.cs
Assets/_Game/Scripts/Loot/LootTableSO.cs
Assets/_Game/Scripts/Save/SaveData.cs
Assets/_Game/Scripts/Combat/**
Assets/_Game/Scripts/Cave/**
```

---

## 5. Definition of Done

- [ ] Equipment possui slots formais.
- [ ] Equipar item altera stats derivados.
- [ ] Durabilidade reduz por uso relevante.
- [ ] Save/load preserva equipamento e durabilidade.
- [ ] LootTableSO é usado por runtime real.
- [ ] Resistência ambiental afeta pelo menos um cenário de cave.
- [ ] UI/debug mostra equipamento e durabilidade.

---

## 6. Validação

1. Equipar arma/armadura e validar stats.
2. Usar tool até reduzir durabilidade.
3. Salvar/carregar equipamento e durability.
4. Entrar em ambiente com resistência insuficiente.
5. Matar inimigo/resource e validar loot table.
