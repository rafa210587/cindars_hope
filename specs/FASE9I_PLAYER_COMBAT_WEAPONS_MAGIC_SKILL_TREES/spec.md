# SpecKit â€” FASE9I Player Combat, Weapons, Magic & Skill Trees

> **Feature:** `FASE9I_PLAYER_COMBAT_WEAPONS_MAGIC_SKILL_TREES`  
> **Status:** especificaÃ§Ã£o funcional aprovada para planejamento.  
> **Fonte de design:** `docs_old/FASE9I_PLAYER_COMBAT_WEAPONS_MAGIC_SKILL_TREES_SPEC_v1.0.md`

---

## 1. User story

Como jogador, quero escolher builds diferentes de combate, magia, arco, dual wield, armas pesadas, escudo, mobilidade ou sobrevivÃªncia, para enfrentar a cave com estilos variados e evoluir meu personagem de acordo com meus atributos, equipamentos e skills.

---

## 2. Objetivos funcionais

### O1 â€” Player action combat

O jogador deve ter aÃ§Ãµes de combate action: Roll, Dash, Parry, Block, Sprint, Special Attack, Spell Cast e Bow Shot.

### O2 â€” Skill-gated actions

Roll, Dash, Parry, Block, Sprint e Charge Shot devem depender de skills.

### O3 â€” Roll e Dash separados

Roll Ã© dodge defensivo. Dash Ã© avanÃ§o ofensivo.

### O4 â€” Shield Block

Block exige Shield e skill especÃ­fica.

### O5 â€” Parry limitado

Parry funciona contra Physical melee e projÃ©teis fÃ­sicos. NÃ£o funciona contra magia pura, AoE, breath, beams ou pools.

### O6 â€” Magic item required

Magia exige Staff, Wand, Focus ou Scroll.

### O7 â€” Weapon special attacks

Toda arma deve ter Special Attack.

### O8 â€” Dual wield

Dual wield leve usa ataque alternado automÃ¡tico no MVP. Dual wield pesado fica como skill avanÃ§ada Combat/Strength.

### O9 â€” Bow and elemental arrows

Bow shot padrÃ£o Ã© instantÃ¢neo, consome stamina + flecha, escala com Dexterity e suporta flechas elementais.

### O10 â€” Elemental magic

Magias existem por elemento: Arcane, Fire, Ice, Poison/Nature, Shadow, Lightning/Thunder, Acid e Corruption.

### O11 â€” Skill trees

Skill trees iniciais: Combat, Dexterity, Magic, Survival/Breath.

---

## 3. Regras de negÃ³cio

### R1 â€” Stamina

Stamina Ã© consumida por melee attack, special attack, bow shot, dash, roll/dodge, parry, block, sprint e tool usage.

### R2 â€” Mana

Mana Ã© consumida por spells, magic focus attacks, weapon enchant, barriers e summons futuros.

### R3 â€” Armor weight

Armor weight afeta movimento, stamina cost, Roll, Dash e Sprint.

### R4 â€” Crit

Crit vem principalmente de skill/equipment, nÃ£o diretamente de Dexterity pura.

### R5 â€” Dexterity

Dexterity aumenta dano ranged e velocidade levemente.

### R6 â€” Durability

Bow e armas usam durabilidade padrÃ£o definida na FASE9H.

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

## 5. CritÃ©rios de aceite

### CA1 â€” Roll

Roll Ã© dodge defensivo desbloqueado por skill de Dexterity.

### CA2 â€” Dash

Dash Ã© avanÃ§o ofensivo desbloqueado por skill de Dexterity.

### CA3 â€” Block

Block exige Shield e skill.

### CA4 â€” Parry

Parry sÃ³ funciona contra Physical melee e projÃ©teis fÃ­sicos.

### CA5 â€” Magic item

Magia exige Staff, Wand, Focus ou Scroll.

### CA6 â€” Dual wield

Dual wield leve usa ataque alternado automÃ¡tico.

### CA7 â€” Heavy dual wield

Dual wield pesado fica como skill avanÃ§ada Combat/Strength.

### CA8 â€” Special attack

Toda arma tem Special Attack.

### CA9 â€” Resource costs

Special Attack consome stamina/mana conforme perfil.

### CA10 â€” Bow

Bow shot padrÃ£o Ã© instantÃ¢neo.

### CA11 â€” Charge Shot

Charge Shot Ã© skill.

### CA12 â€” Elemental arrows

Existem flechas elementais.

### CA13 â€” Elemental spells

Magias existem por elemento.

### CA14 â€” Skill trees

Skill trees iniciais existem: Combat, Dexterity, Magic, Survival/Breath.

### CA15 â€” Armor weight

Armor weight afeta mobilidade/stamina.

---

## 6. Non-goals

Fora desta spec:

- balance final de dano/stamina/mana;
- animaÃ§Ãµes finais de combate;
- hitbox/hurtbox avanÃ§ado;
- dodge i-frames finais;
- parry perfeito avanÃ§ado;
- shield bash final;
- Ã¡rvores de skill completas com dezenas de nÃ³s;
- UI final da skill tree;
- VFX/SFX finais das magias;
- PvP/co-op.

---

## 7. MVP recomendado

1. `PlayerActionDataSO` para Roll/Dash/Sprint/Special Attack.
2. `WeaponCombatProfileSO` para Sword, Axe, Hammer, Spear, Dagger, Bow, Wand, Staff.
3. `SpellDataSO` para Arcane Bolt, Fire Spark, Frost Shard.
4. Skill unlock simples para Roll, Dash, Charge Shot, Shield Block e Arcane Bolt.
5. Bow shot instantÃ¢neo com ammo/stamina/durability.
6. Special Attack simples por arma.
7. Stamina validation comum.
8. Mana validation comum.
9. Armor weight afetando stamina/move speed.

---

## 8. DependÃªncias

- FASE9E UI/Hotbar/Inventory/Equipment.
- FASE9E Player Level Up/Progression.
- FASE9E Damage/Status/Formula.
- FASE9H Cave Loot/Crafting/Equipment Progression.
- FASE9G Enemy Combat Roles/AI/Status amendment.

---

## 9. Pronto para Plan quando

- Equipment/hotbar estiver planejado ou implementado.
- Stamina/Mana estiverem disponÃ­veis no player runtime.
- Skill tree data contracts forem aceitos para implementaÃ§Ã£o.

