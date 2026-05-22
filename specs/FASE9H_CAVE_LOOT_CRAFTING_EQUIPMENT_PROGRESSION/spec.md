# SpecKit â€” FASE9H Cave Loot, Crafting, Equipment & Gear Progression

> **Feature:** `FASE9H_CAVE_LOOT_CRAFTING_EQUIPMENT_PROGRESSION`  
> **Status:** especificaÃ§Ã£o funcional aprovada para planejamento.  
> **Fonte de design:** `docs_old/FASE9H_CAVE_LOOT_CRAFTING_EQUIPMENT_PROGRESSION_SPEC_v1.0.md`

---

## 1. User story

Como jogador, quero explorar a cave, coletar materiais, derrotar criaturas e bosses, refinar recursos, reparar armas raras, craftar equipamentos e vencer bloqueios ambientais/ferramentais para avanÃ§ar cada vez mais fundo.

---

## 2. Objetivos funcionais

### O1 â€” ProgressÃ£o material da cave

Cada faixa da cave deve ter materiais brutos, refinados e usos claros para gear progression.

### O2 â€” Drops por famÃ­lia

Cada famÃ­lia de inimigos deve ter drops comuns e raros.

### O3 â€” Boss progression drops

Todo boss deve dropar pelo menos um `ProgressionDrop`. Boss tambÃ©m pode dropar material raro vendÃ¡vel.

### O4 â€” Refinamento por estaÃ§Ã£o

Materiais brutos viram materiais refinados em estaÃ§Ãµes adequadas.

### O5 â€” Crafting com tempo

Crafting exige segurar botÃ£o atÃ© completar a barra. Level e Dexterity reduzem tempo.

### O6 â€” Durabilidade

Todo item equipÃ¡vel tem `DurabilityMax = 100`. A cada 3 usos relevantes, perde 1 durabilidade.

### O7 â€” Equipamento RPG

Equipamentos devem permitir builds por tier: Strength, Dexterity, Intelligence, Willpower e hÃ­bridos.

### O8 â€” Armaduras MVP

Armor Ã© peÃ§a Ãºnica no MVP. Tipos: Cloth, Leather, Metal, Hybrid, Elemental.

### O9 â€” Accessory MVP

MVP possui 1 slot de accessory.

### O10 â€” ResistÃªncia ambiental

`HeatResistance` e `ColdResistance` existem separadas de `FireResistance` e `IceResistance`.

### O11 â€” Environmental gates

Ambientes quentes/frios podem causar dano e/ou bloquear progressÃ£o se resistÃªncia for insuficiente.

### O12 â€” Tool gates

Tool tier bloqueia nodes e pode bloquear caminho principal se ferramenta/material/receita forem acessÃ­veis antes.

### O13 â€” Broken elemental weapons

Humanoides podem dropar armas elementais quebradas que exigem reparo.

---

## 3. Regras de negÃ³cio

### R1 â€” EquipÃ¡vel tem durabilidade

```text
DurabilityMax = 100
DurabilityCurrent inicia em 100
A cada 3 usos relevantes, perde 1 durability
```

### R2 â€” AplicaÃ§Ã£o de durabilidade

| Tipo | Uso relevante |
|---|---|
| Weapon | 3 ataques |
| Tool | 3 hits/uses em node |
| Bow | 3 disparos |
| MagicFocus | 3 casts |
| Armor | 3 hits recebidos |
| Accessory | sem durabilidade no MVP, salvo especial |

### R3 â€” Crafting hostil cancela

Crafting em Ã¡rea hostil cancela se o jogador tomar dano.

### R4 â€” Boss key item protegido

ProgressionDrop nÃ£o deve ser vendÃ¡vel por padrÃ£o.

### R5 â€” Heavy armor tem custo

Metal/Heavy armor deve reduzir movimento e/ou aumentar custo de stamina.

### R6 â€” Cloth/Leather viÃ¡veis

Cloth favorece magia; Leather favorece dex/crit/mobilidade; Metal favorece defesa/resistÃªncia.

### R7 â€” Sem softlock silencioso

Tool/environment gate em caminho principal exige receita/material/ferramenta disponÃ­veis antes e feedback claro.

---

## 4. Entidades funcionais

- `EquipmentDataSO`
- `EquipmentStats`
- `EquipmentInstanceSaveData`
- `CraftingRecipeSO vNext`
- `CraftingProgressState`
- `EnvironmentalGateSO`
- `RepairBench`
- `Workbench`
- `Forge`
- `WeaponAffix`
- `DurabilityState`

---

## 5. CritÃ©rios de aceite

### CA1 â€” Materiais por faixa

Cada faixa da cave possui materiais brutos e refinados definidos.

### CA2 â€” Drops por famÃ­lia

Cada famÃ­lia de inimigo possui drops comuns e raros.

### CA3 â€” Boss progression drop

Cada boss possui pelo menos um progression drop.

### CA4 â€” Armaduras

Armaduras possuem tipo, peso, bÃ´nus e penalidade.

### CA5 â€” ResistÃªncia ambiental

`HeatResistance` e `ColdResistance` existem separados de `FireResistance` e `IceResistance`.

### CA6 â€” Dano ambiental

Ambientes podem exigir resistÃªncia ambiental.

### CA7 â€” Crafting com tempo

Crafting usa tempo segurando botÃ£o.

### CA8 â€” ReduÃ§Ã£o por Level/Dexterity

Level e Dexterity reduzem tempo de crafting.

### CA9 â€” EstaÃ§Ãµes

Refinamento usa estaÃ§Ãµes diferentes.

### CA10 â€” Tool gates

Tool tier bloqueia nodes e Ã¡reas.

### CA11 â€” Broken elemental weapons

Armas elementais quebradas podem ser reparadas.

### CA12 â€” Key items

Key items de boss nÃ£o sÃ£o vendÃ¡veis por padrÃ£o.

### CA13 â€” Durability

EquipÃ¡veis possuem `DurabilityMax = 100` e perdem 1 durabilidade a cada 3 usos relevantes.

### CA14 â€” Armor/accessory MVP

Armor Ã© peÃ§a Ãºnica e MVP possui 1 accessory slot.

### CA15 â€” Crafting cancel

Crafting em Ã¡rea hostil cancela ao tomar dano.

### CA16 â€” Gate principal seguro

Tool gate pode bloquear caminho principal apenas com receita/material/ferramenta acessÃ­veis antes.

---

## 6. Non-goals

Fora desta spec:

- balance final de todos os itens;
- UI final de crafting/inventory/equipment;
- durabilidade visual final;
- animaÃ§Ãµes finais de crafting;
- sistema completo de peÃ§as separadas de armadura;
- 2+ acessÃ³rios no MVP;
- encantamento completo;
- transmog/cosmetic gear;
- sistema econÃ´mico final;
- crafting multiplayer/co-op.

---

## 7. MVP recomendado

1. Contratos de `EquipmentDataSO`, `EquipmentStats` e durability.
2. `CraftingRecipeSO vNext` com craft time.
3. `RepairBench` simples.
4. Workbench + Forge.
5. Armor Ãºnica com Cloth/Leather/Metal exemplos.
6. `HeatResistance`/`ColdResistance` no equipment stats.
7. `EnvironmentalGateSO` placeholder.
8. Durability em Weapon/Tool/Armor.
9. Broken elemental weapon repair flow com 1 exemplo.

---

## 8. DependÃªncias

- FASE9E Item Taxonomy/IDs.
- FASE9E Save Schema/Migration.
- FASE9E Player Level Up/Progression.
- FASE9F Cave Resources/Encounters.
- FASE9G Cave Bestiary/Faction Locks.
- FASE9G Enemy Combat Roles/AI/Status amendment.

---

## 9. Pronto para Plan quando

- FASE9E equipment/hotbar estiver planejado ou implementado.
- FASE9F resource nodes/tool tier estiver planejado ou implementado.
- O time decidir iniciar contracts de equipment/crafting/durability.

