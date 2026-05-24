# refinamento_init_equipment_durability_environment_loot

> **Status:** Refinamento inicial a implementar
> **Spec futura sugerida:** `spec_equipment_durability_environment_loot_runtime.md`
> **Objetivo:** completar equipment, durabilidade, resistÃªncia ambiental e loot runtime.

---

## 1. Estado atual

`EquipmentManager` Ã© parcial/debug: guarda tool equipada, weapon equipada e infere tool por ID. HÃ¡ dados/managers iniciais criados na wave overnight para equipment, durability, environmental resistance e loot, mas ainda nÃ£o formam runtime completo.

EvidÃªncia:

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

- EquipmentManager nÃ£o aplica stats de equipamento no player.
- Equipamento nÃ£o possui slots formais: weapon, armor, ring, tool, accessory etc.
- Durabilidade nÃ£o Ã© consumida por ataque/defesa/tool use.
- NÃ£o hÃ¡ repair flow.
- ResistÃªncia ambiental nÃ£o afeta cave/bioma/status.
- LootTableSO nÃ£o estÃ¡ integrado de forma unificada a inimigos/resources/Ã¡rvores/pesca.
- Save/load de equipment/durability ainda Ã© parcial.

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

- Cave bands podem exigir resistÃªncia ambiental.
- Falta de resistÃªncia aplica debuff/damage over time/slow.

### Loot

- LootTableSO deve ser usado por pelo menos enemy/resource/world activity.
- Suportar peso, min/max amount, chance e condiÃ§Ãµes.

---

## 4. Arquivos provÃ¡veis

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
- [ ] LootTableSO Ã© usado por runtime real.
- [ ] ResistÃªncia ambiental afeta pelo menos um cenÃ¡rio de cave.
- [ ] UI/debug mostra equipamento e durabilidade.

---

## 6. ValidaÃ§Ã£o

1. Equipar arma/armadura e validar stats.
2. Usar tool atÃ© reduzir durabilidade.
3. Salvar/carregar equipamento e durability.
4. Entrar em ambiente com resistÃªncia insuficiente.
5. Matar inimigo/resource e validar loot table.
