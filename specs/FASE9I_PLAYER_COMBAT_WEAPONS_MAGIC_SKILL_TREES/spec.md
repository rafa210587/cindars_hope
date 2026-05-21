# SpecKit — FASE9I Player Combat, Weapons, Magic & Skill Trees

> **Feature:** `FASE9I_PLAYER_COMBAT_WEAPONS_MAGIC_SKILL_TREES`  
> **Status:** especificação funcional aprovada para planejamento.  
> **Fonte de design:** `docs/FASE9I_PLAYER_COMBAT_WEAPONS_MAGIC_SKILL_TREES_SPEC_v1.0.md`

---

## 1. User story

Como jogador, quero escolher builds diferentes de combate, magia, arco, dual wield, armas pesadas, escudo, mobilidade ou sobrevivência, para enfrentar a cave com estilos variados e evoluir meu personagem de acordo com meus atributos, equipamentos e skills.

---

## 2. Objetivos funcionais

### O1 — Player action combat

O jogador deve ter ações de combate action: Roll, Dash, Parry, Block, Sprint, Special Attack, Spell Cast e Bow Shot.

### O2 — Skill-gated actions

Roll, Dash, Parry, Block, Sprint e Charge Shot devem depender de skills.

### O3 — Roll e Dash separados

Roll é dodge defensivo. Dash é avanço ofensivo.

### O4 — Shield Block

Block exige Shield e skill específica.

### O5 — Parry limitado

Parry funciona contra Physical melee e projéteis físicos. Não funciona contra magia pura, AoE, breath, beams ou pools.

### O6 — Magic item required

Magia exige Staff, Wand, Focus ou Scroll.

### O7 — Weapon special attacks

Toda arma deve ter Special Attack.

### O8 — Dual wield

Dual wield leve usa ataque alternado automático no MVP. Dual wield pesado fica como skill avançada Combat/Strength.

### O9 — Bow and elemental arrows

Bow shot padrão é instantâneo, consome stamina + flecha, escala com Dexterity e suporta flechas elementais.

### O10 — Elemental magic

Magias existem por elemento: Arcane, Fire, Ice, Poison/Nature, Shadow, Lightning/Thunder, Acid e Corruption.

### O11 — Skill trees

Skill trees iniciais: Combat, Dexterity, Magic, Survival/Breath.

---

## 3. Regras de negócio

### R1 — Stamina

Stamina é consumida por melee attack, special attack, bow shot, dash, roll/dodge, parry, block, sprint e tool usage.

### R2 — Mana

Mana é consumida por spells, magic focus attacks, weapon enchant, barriers e summons futuros.

### R3 — Armor weight

Armor weight afeta movimento, stamina cost, Roll, Dash e Sprint.

### R4 — Crit

Crit vem principalmente de skill/equipment, não diretamente de Dexterity pura.

### R5 — Dexterity

Dexterity aumenta dano ranged e velocidade levemente.

### R6 — Durability

Bow e armas usam durabilidade padrão definida na FASE9H.

---

## 4. Entidades funcionais

- `PlayerCombatSkillSO`
- `WeaponCombatProfileSO`
- `SpellDataSO`
- `PlayerActionDataSO`
- `SkillTreeId`
- `ActionId`
- `WeaponBehaviorId`
- `SpellId`

---

## 5. Critérios de aceite

### CA1 — Roll

Roll é dodge defensivo desbloqueado por skill de Dexterity.

### CA2 — Dash

Dash é avanço ofensivo desbloqueado por skill de Dexterity.

### CA3 — Block

Block exige Shield e skill.

### CA4 — Parry

Parry só funciona contra Physical melee e projéteis físicos.

### CA5 — Magic item

Magia exige Staff, Wand, Focus ou Scroll.

### CA6 — Dual wield

Dual wield leve usa ataque alternado automático.

### CA7 — Heavy dual wield

Dual wield pesado fica como skill avançada Combat/Strength.

### CA8 — Special attack

Toda arma tem Special Attack.

### CA9 — Resource costs

Special Attack consome stamina/mana conforme perfil.

### CA10 — Bow

Bow shot padrão é instantâneo.

### CA11 — Charge Shot

Charge Shot é skill.

### CA12 — Elemental arrows

Existem flechas elementais.

### CA13 — Elemental spells

Magias existem por elemento.

### CA14 — Skill trees

Skill trees iniciais existem: Combat, Dexterity, Magic, Survival/Breath.

### CA15 — Armor weight

Armor weight afeta mobilidade/stamina.

---

## 6. Non-goals

Fora desta spec:

- balance final de dano/stamina/mana;
- animações finais de combate;
- hitbox/hurtbox avançado;
- dodge i-frames finais;
- parry perfeito avançado;
- shield bash final;
- árvores de skill completas com dezenas de nós;
- UI final da skill tree;
- VFX/SFX finais das magias;
- PvP/co-op.

---

## 7. MVP recomendado

1. `PlayerActionDataSO` para Roll/Dash/Sprint/Special Attack.
2. `WeaponCombatProfileSO` para Sword, Axe, Hammer, Spear, Dagger, Bow, Wand, Staff.
3. `SpellDataSO` para Arcane Bolt, Fire Spark, Frost Shard.
4. Skill unlock simples para Roll, Dash, Charge Shot, Shield Block e Arcane Bolt.
5. Bow shot instantâneo com ammo/stamina/durability.
6. Special Attack simples por arma.
7. Stamina validation comum.
8. Mana validation comum.
9. Armor weight afetando stamina/move speed.

---

## 8. Dependências

- FASE9E UI/Hotbar/Inventory/Equipment.
- FASE9E Player Level Up/Progression.
- FASE9E Damage/Status/Formula.
- FASE9H Cave Loot/Crafting/Equipment Progression.
- FASE9G Enemy Combat Roles/AI/Status amendment.

---

## 9. Pronto para Plan quando

- Equipment/hotbar estiver planejado ou implementado.
- Stamina/Mana estiverem disponíveis no player runtime.
- Skill tree data contracts forem aceitos para implementação.
