# SpecKit — FASE9H Cave Loot, Crafting, Equipment & Gear Progression

> **Feature:** `FASE9H_CAVE_LOOT_CRAFTING_EQUIPMENT_PROGRESSION`  
> **Status:** especificação funcional aprovada para planejamento.  
> **Fonte de design:** `docs/FASE9H_CAVE_LOOT_CRAFTING_EQUIPMENT_PROGRESSION_SPEC_v1.0.md`

---

## 1. User story

Como jogador, quero explorar a cave, coletar materiais, derrotar criaturas e bosses, refinar recursos, reparar armas raras, craftar equipamentos e vencer bloqueios ambientais/ferramentais para avançar cada vez mais fundo.

---

## 2. Objetivos funcionais

### O1 — Progressão material da cave

Cada faixa da cave deve ter materiais brutos, refinados e usos claros para gear progression.

### O2 — Drops por família

Cada família de inimigos deve ter drops comuns e raros.

### O3 — Boss progression drops

Todo boss deve dropar pelo menos um `ProgressionDrop`. Boss também pode dropar material raro vendável.

### O4 — Refinamento por estação

Materiais brutos viram materiais refinados em estações adequadas.

### O5 — Crafting com tempo

Crafting exige segurar botão até completar a barra. Level e Dexterity reduzem tempo.

### O6 — Durabilidade

Todo item equipável tem `DurabilityMax = 100`. A cada 3 usos relevantes, perde 1 durabilidade.

### O7 — Equipamento RPG

Equipamentos devem permitir builds por tier: Strength, Dexterity, Intelligence, Willpower e híbridos.

### O8 — Armaduras MVP

Armor é peça única no MVP. Tipos: Cloth, Leather, Metal, Hybrid, Elemental.

### O9 — Accessory MVP

MVP possui 1 slot de accessory.

### O10 — Resistência ambiental

`HeatResistance` e `ColdResistance` existem separadas de `FireResistance` e `IceResistance`.

### O11 — Environmental gates

Ambientes quentes/frios podem causar dano e/ou bloquear progressão se resistência for insuficiente.

### O12 — Tool gates

Tool tier bloqueia nodes e pode bloquear caminho principal se ferramenta/material/receita forem acessíveis antes.

### O13 — Broken elemental weapons

Humanoides podem dropar armas elementais quebradas que exigem reparo.

---

## 3. Regras de negócio

### R1 — Equipável tem durabilidade

```text
DurabilityMax = 100
DurabilityCurrent inicia em 100
A cada 3 usos relevantes, perde 1 durability
```

### R2 — Aplicação de durabilidade

| Tipo | Uso relevante |
|---|---|
| Weapon | 3 ataques |
| Tool | 3 hits/uses em node |
| Bow | 3 disparos |
| MagicFocus | 3 casts |
| Armor | 3 hits recebidos |
| Accessory | sem durabilidade no MVP, salvo especial |

### R3 — Crafting hostil cancela

Crafting em área hostil cancela se o jogador tomar dano.

### R4 — Boss key item protegido

ProgressionDrop não deve ser vendável por padrão.

### R5 — Heavy armor tem custo

Metal/Heavy armor deve reduzir movimento e/ou aumentar custo de stamina.

### R6 — Cloth/Leather viáveis

Cloth favorece magia; Leather favorece dex/crit/mobilidade; Metal favorece defesa/resistência.

### R7 — Sem softlock silencioso

Tool/environment gate em caminho principal exige receita/material/ferramenta disponíveis antes e feedback claro.

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

## 5. Critérios de aceite

### CA1 — Materiais por faixa

Cada faixa da cave possui materiais brutos e refinados definidos.

### CA2 — Drops por família

Cada família de inimigo possui drops comuns e raros.

### CA3 — Boss progression drop

Cada boss possui pelo menos um progression drop.

### CA4 — Armaduras

Armaduras possuem tipo, peso, bônus e penalidade.

### CA5 — Resistência ambiental

`HeatResistance` e `ColdResistance` existem separados de `FireResistance` e `IceResistance`.

### CA6 — Dano ambiental

Ambientes podem exigir resistência ambiental.

### CA7 — Crafting com tempo

Crafting usa tempo segurando botão.

### CA8 — Redução por Level/Dexterity

Level e Dexterity reduzem tempo de crafting.

### CA9 — Estações

Refinamento usa estações diferentes.

### CA10 — Tool gates

Tool tier bloqueia nodes e áreas.

### CA11 — Broken elemental weapons

Armas elementais quebradas podem ser reparadas.

### CA12 — Key items

Key items de boss não são vendáveis por padrão.

### CA13 — Durability

Equipáveis possuem `DurabilityMax = 100` e perdem 1 durabilidade a cada 3 usos relevantes.

### CA14 — Armor/accessory MVP

Armor é peça única e MVP possui 1 accessory slot.

### CA15 — Crafting cancel

Crafting em área hostil cancela ao tomar dano.

### CA16 — Gate principal seguro

Tool gate pode bloquear caminho principal apenas com receita/material/ferramenta acessíveis antes.

---

## 6. Non-goals

Fora desta spec:

- balance final de todos os itens;
- UI final de crafting/inventory/equipment;
- durabilidade visual final;
- animações finais de crafting;
- sistema completo de peças separadas de armadura;
- 2+ acessórios no MVP;
- encantamento completo;
- transmog/cosmetic gear;
- sistema econômico final;
- crafting multiplayer/co-op.

---

## 7. MVP recomendado

1. Contratos de `EquipmentDataSO`, `EquipmentStats` e durability.
2. `CraftingRecipeSO vNext` com craft time.
3. `RepairBench` simples.
4. Workbench + Forge.
5. Armor única com Cloth/Leather/Metal exemplos.
6. `HeatResistance`/`ColdResistance` no equipment stats.
7. `EnvironmentalGateSO` placeholder.
8. Durability em Weapon/Tool/Armor.
9. Broken elemental weapon repair flow com 1 exemplo.

---

## 8. Dependências

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
