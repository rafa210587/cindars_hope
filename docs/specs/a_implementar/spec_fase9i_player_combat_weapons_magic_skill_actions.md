# SPEC FUTURA — FASE9I Player Combat, Weapons, Magic e Skill Actions

> Origem histórica: `docs_old/FASE9I_PLAYER_COMBAT_WEAPONS_MAGIC_SKILL_TREES_SPEC_v1.0.md`
> Status: A implementar
> Observação: conteúdo refinado preservado da documentação antiga.

---

# FASE 9I — Player Combat, Weapons, Magic & Skill Trees Spec v1.0

> **Status:** spec aprovada para orientar próximas waves.  
> **Feature:** `FASE9I_PLAYER_COMBAT_WEAPONS_MAGIC_SKILL_TREES`  
> **Base:** FASE9E Progression/UI/Equipment + FASE9G Enemy Combat + FASE9H Loot/Equipment/Durability.  
> **Objetivo:** definir o lado do jogador no combate action: armas, ataques, special attacks, stamina, mana, roll, dash, block, parry, sprint, dual wield, magia elemental e skill trees.

---

## 1. Problema

As specs anteriores definem:

- cave procedural;
- bestiário e faction locks;
- IA/status/movesets de inimigos;
- loot/crafting/equipment/durability.

Ainda falta definir como o jogador luta e evolui em combate:

- ações defensivas e ofensivas;
- armas e special attacks;
- stamina e mana;
- magia elemental;
- arco/flechas;
- dual wield;
- armas de duas mãos;
- block, parry, roll, dash e sprint;
- skill trees e builds.

---

## 2. User story

Como jogador, quero escolher builds diferentes de combate, magia, arco, dual wield, armas pesadas, escudo, mobilidade ou sobrevivência, para enfrentar a cave com estilos variados e evoluir meu personagem de acordo com meus atributos, equipamentos e skills.

---

## 3. Decisões fechadas

```text
D1: Combate do jogador será action RPG.
D2: Roll é o dodge defensivo.
D3: Dash é avanço ofensivo.
D4: Roll é skill da árvore de Dexterity.
D5: Dash é skill da árvore de Dexterity.
D6: Parry é skill da árvore de Dexterity.
D7: Block exige Shield e skill específica.
D8: Sprint é skill de Survival/Breath.
D9: Roll, Dash, Block, Parry, Sprint e Special Attack consomem stamina.
D10: Parry funciona contra ataques Physical melee e projéteis físicos.
D11: Parry não funciona contra beams, AoE, pools, breath, curse, poison cloud ou magia pura.
D12: Magia exige Staff, Wand, Focus ou Pergaminho.
D13: Cada arma terá comportamento próprio.
D14: Toda arma terá Special Attack.
D15: Armas de duas mãos existem.
D16: Dual wield existe para armas leves.
D17: Dual wield leve usa ataque alternado automático no MVP.
D18: Dual wield de armas pesadas pode existir como skill avançada de Combat/Strength.
D19: Tiro de arco padrão é instantâneo.
D20: Charge Shot é skill da árvore de Dexterity.
D21: Arco consome flecha + stamina leve.
D22: Dexterity aumenta dano ranged.
D23: Dexterity aumenta velocidade levemente.
D24: Crit vem principalmente por skill/equipment, não direto por Dexterity pura.
D25: Existem flechas elementais.
D26: Arco usa durabilidade padrão: 100, perde 1 a cada 3 disparos.
D27: Magias existem por elemento.
D28: Skill trees começam simples, mas serão grandes no futuro.
```

---

## 4. Ações do jogador

| Ação | Tipo | Como libera | Custo | Função |
|---|---|---|---:|---|
| Roll/Dodge | defensiva | Dexterity tree | stamina médio/alto | evasão defensiva |
| Dash | ofensiva | Dexterity tree | stamina médio | avanço/gap closer |
| Parry | defensiva/ofensiva | Dexterity tree | stamina alto | timing contra physical/projétil físico |
| Block | defensiva | Combat/Shield skill | stamina por hit/segundo | reduzir dano com shield |
| Sprint | mobilidade | Survival/Breath tree | stamina contínuo | exploração/reposicionamento |
| Special Attack | ofensiva | arma + skill base | stamina/mana | ataque especial por arma |
| Spell Cast | ofensiva/suporte | Magic tree + item mágico | mana | magia elemental |
| Bow Shot | ranged | Bow equipado | stamina + ammo | tiro físico/ranged |

---

## 5. Roll / Dodge

```text
Roll é o dodge padrão.
É defensivo.
É skill da árvore Dexterity.
Consome stamina.
Pode ter janela curta de invulnerabilidade ou evasão.
É afetado por armor weight.
```

Skills relacionadas:

```text
Roll
Improved Roll
Lightfoot Roll
Counter Roll futuro
```

Hardening:

```text
Roll não deve virar spam infinito.
Armor pesada reduz eficiência/custo do Roll.
Roll não deve atravessar hazard permanente sem custo/risco.
```

---

## 6. Dash

```text
Dash é avanço ofensivo.
É skill da árvore Dexterity.
Consome stamina.
Serve para aproximar, reposicionar agressivamente ou iniciar combo.
Não deve ter a mesma janela defensiva do Roll.
```

Skills relacionadas:

```text
Dash
Dash Strike
Piercing Dash
Shadow Dash futuro
```

Hardening:

```text
Dash não substitui Roll defensivo.
Dash pode reduzir dano recebido durante deslocamento, mas não deve ser invulnerável no MVP.
Dash deve ter cooldown/stamina cost suficiente para não trivializar ranged enemies.
```

---

## 7. Block

```text
Block exige Shield.
Block exige skill específica.
Consome stamina ao bloquear hit.
Reduz dano, não anula tudo no MVP.
```

Slots:

```text
LeftHand = Shield
RightHand = Weapon
```

Regras:

```text
Sem Shield, não há Block.
Armas de duas mãos não podem usar Shield.
Dual wield não pode usar Block.
Block pode ter ângulo/frente no futuro.
```

Skills relacionadas:

```text
Shield Block
Improved Block
Guard Discipline
Shield Bash futuro
```

---

## 8. Parry

```text
Parry é skill de Dexterity.
Funciona contra ataques Physical melee e projéteis físicos.
Não funciona contra beams, AoE, pools, breath, curse, poison cloud ou magia pura.
Consome stamina.
Exige timing.
Tem cooldown curto.
```

Hardening:

```text
Parry não deve trivializar boss.
Boss attacks precisam de flag Parryable = true/false.
Projéteis físicos podem ser parryable.
Projéteis mágicos normalmente não.
Falha de parry deve deixar pequena janela de punição.
```

Skills relacionadas:

```text
Parry
Projectile Parry
Perfect Parry futuro
Riposte futuro
```

---

## 9. Sprint

```text
Sprint é skill de Survival/Breath.
Consome stamina continuamente.
Serve para exploração e reposicionamento.
É afetado por armor weight.
```

Skills relacionadas:

```text
Sprint
Improved Sprint
Long Breath
Stamina Recovery
```

Hardening:

```text
Sprint não substitui Dash/Roll em combate.
Pode ser interrompido por hit forte ou stamina baixa.
```

---

## 10. Tipos de arma

| Arma | Mão | Escala principal | Identidade |
|---|---|---|---|
| Sword | 1H | Strength/Dexterity | equilibrada |
| Axe | 1H/2H | Strength | dano alto, mais stamina |
| Hammer | 1H/2H | Strength | stagger, knockback, lento |
| Spear | 1H/2H | Strength/Dexterity | alcance, thrust |
| Dagger | 1H leve | Dexterity | rápido, crit por skill |
| Bow | 2H/ranged | Dexterity | tiro instantâneo, flechas |
| Wand | 1H magic | Intelligence | magia rápida |
| Staff | 2H magic | Intelligence/Willpower | magia forte, mais lenta |
| Focus | offhand/magic | Intelligence/Willpower | magia especializada/status |
| Tome/Relic | offhand/magic | Willpower | sustain, mana, status |
| Scroll | consumível | n/a | magia consumível |
| Shield | offhand | Constitution/Strength futuro | block |
| Tool-as-Weapon | 1H/2H | Strength | dano menor que arma real |

---

## 11. Light Attack e Special Attack por arma

| Arma | Light Attack | Special Attack |
|---|---|---|
| Sword | corte rápido | wide slash / guard break |
| Axe | corte pesado | cleave frontal |
| Hammer | golpe lento | slam com stagger/knockback |
| Spear | thrust | piercing dash |
| Dagger | stab rápido | flurry / backstep strike |
| Bow | tiro instantâneo | charge shot via skill |
| Wand | bolt rápido | elemental burst |
| Staff | spell cast | charged spell |
| Focus | cast/status | focused channel / mark |
| Shield | block | shield bash futuro |
| Tool | golpe improvisado | tool slam fraco |

Regras:

```text
Toda arma tem Special Attack.
Special Attack consome mais stamina ou mana.
Special Attack pode consumir durabilidade normalmente ou extra em casos futuros.
Special Attack avançado pode exigir skill.
```

---

## 12. Dual wield

### 12.1 Regra base

```text
Dual wield só permite duas armas leves no início.
Ataque alternado automático no MVP.
```

Permitido inicialmente:

```text
dagger + dagger
dagger + light sword
light sword + light sword
wand leve + dagger futuro, se build permitir
```

Não permitido inicialmente:

```text
two-handed weapon + qualquer outra arma
heavy weapon + heavy weapon
shield + dual wield
```

### 12.2 Penalidades iniciais

```text
maior consumo de stamina
menor defesa/block inexistente
menor alcance
gasto de durabilidade em ambas armas conforme uso
```

### 12.3 Skills futuras

```text
Light Dual Wield: permite duas armas leves.
Dual Wield II: reduz custo de stamina.
Dual Wield III: libera combo alternado.
Heavy Dual Wield: permite duas armas pesadas, skill avançada de Combat/Strength.
```

Hardening:

```text
Heavy Dual Wield deve ser late game/capstone.
Dual wield não deve superar todas as armas 2H sem custo.
```

---

## 13. Arco e flechas

### 13.1 Tiro padrão

```text
Instantâneo.
Consome 1 flecha.
Consome stamina baixa.
Escala com Dexterity.
Pode critar se houver skill/equipment.
Arco usa durabilidade padrão: 100, perde 1 a cada 3 disparos.
```

### 13.2 Charge Shot

```text
Não é padrão.
É skill desbloqueável.
Segurar botão carrega tiro.
Quanto mais carrega, mais dano/stagger.
Consome mais stamina.
Pode consumir durabilidade adicional em tiers futuros.
```

### 13.3 Flechas elementais

| Flecha | Dano/status |
|---|---|
| Fire Arrow | Fire + chance Burn |
| Frost Arrow | Ice + chance Chill |
| Poison Arrow | Poison |
| Shadow Arrow | Shadow + Shadow Mark |
| Arcane Arrow | Arcane + Arcane Mark |
| Lightning Arrow | Lightning/Thunder + stun futuro |
| Blackstone Arrow | Corruption |
| Acid Arrow | Acid |

Hardening:

```text
Flechas elementais devem ser consumíveis.
Flechas elementais precisam de crafting/loot compatível.
Charge Shot com flecha elemental deve custar mais stamina e/ou ter menor cadência.
```

---

## 14. Magia

```text
Magia exige Staff, Wand, Focus ou Pergaminho.
Sem item mágico equipado, jogador não lança magia ativa.
Scroll pode permitir magia consumível mesmo sem build mágica.
```

### 14.1 Tipos de item mágico

| Tipo | Uso |
|---|---|
| Wand | magia rápida, menor dano, 1 mão |
| Staff | magia forte, 2 mãos, cast mais lento |
| Focus | magia especializada/status |
| Tome/Relic | sustain, mana, status, suporte |
| Scroll | consumível, uso único ou limitado |

### 14.2 Elementos suportados

```text
Fire
Ice
Poison/Nature
Shadow
Arcane
Lightning/Thunder
Acid
Corruption
```

### 14.3 Magias base por elemento

| Elemento | Spell inicial | Função |
|---|---|---|
| Arcane | Arcane Bolt | projétil padrão, linha reta |
| Fire | Fire Spark | projétil com chance Burn |
| Ice | Frost Shard | projétil com chance Chill |
| Poison/Nature | Venom Dart | DoT Poison |
| Shadow | Shadow Spark | Shadow Mark/debuff |
| Lightning/Thunder | Spark Jolt | dano rápido, stun futuro |
| Acid | Acid Glob | reduz defesa futuro |
| Corruption | Blackstone Pulse | DoT/debuff avançado |

### 14.4 Categorias futuras

| Categoria | Exemplos |
|---|---|
| Projectile | bolt, shard, dart |
| AoE | fire pool, frost nova, poison cloud |
| Control | root, slow, fear |
| Defense | barrier, ward, cleanse |
| Summon | wisp, vine, skeleton futuro |
| Mobility | blink curto futuro |
| Weapon enchant | fire blade, frost arrow, shadow edge |

Hardening:

```text
Magias usam mana e podem ter cast time.
Staff pode ter mais dano/cast lento.
Wand pode ter cast rápido/dano menor.
Focus pode ter status/controle.
Scroll consome item.
```

---

## 15. Stamina e mana

### 15.1 Stamina

Usada por:

```text
melee attack
special attack
bow shot
dash
roll/dodge
parry
block
sprint
tool usage
```

### 15.2 Mana

Usada por:

```text
spells
magic focus attacks
weapon enchant
barriers
summons futuro
```

### 15.3 Armor weight

Peso afeta:

```text
move speed
stamina cost
dodge/roll efficiency
sprint duration
cast time em armaduras muito pesadas, se aplicável
```

Hardening:

```text
Stamina baixa deve impedir ação especial.
Mana baixa deve impedir spell.
Não permitir stamina negativa.
Feedback visual deve indicar falta de stamina/mana.
```

---

## 16. Skill trees iniciais

## 16.1 Combat Tree

Foco:

```text
melee
shield block
heavy weapons
two-handed
stagger
special attacks
heavy dual wield futuro
```

Skills iniciais:

```text
Power Strike
Weapon Special Attack I
Shield Block
Improved Block
Guard Break
Hammer Stagger
Two-Handed Mastery
Heavy Weapon Training
Heavy Dual Wield
```

## 16.2 Dexterity Tree

Foco:

```text
roll
dash
parry
bow
dagger
dual wield leve
charge shot
crit por skill
velocidade
```

Skills iniciais:

```text
Roll
Improved Roll
Dash
Dash Strike
Parry
Projectile Parry
Light Dual Wield
Dagger Flurry
Quick Draw
Charge Shot
Critical Training
Evasive Footwork
```

## 16.3 Magic Tree

Foco:

```text
staff
wand
focus
scroll
elemental spells
mana
cast time
status
```

Skills iniciais:

```text
Arcane Bolt
Fire Spark
Frost Shard
Venom Dart
Shadow Spark
Spark Jolt
Acid Glob
Blackstone Pulse
Mana Efficiency
Faster Casting
Elemental Attunement
Spell Status Chance
Scroll Mastery futuro
```

## 16.4 Survival / Breath Tree

Foco:

```text
sprint
stamina
resistência ambiental
crafting
durability
gathering
cave sustain
```

Skills iniciais:

```text
Sprint
Improved Sprint
Improved Breath
Stamina Recovery
Efficient Tools
Durability Care
Heat Adaptation
Cold Adaptation
Resourceful Gathering
Faster Crafting
```

---

## 17. Builds principais

| Build | Atributos | Equipamento | Estilo |
|---|---|---|---|
| Warrior | Strength + Constitution | metal + sword/axe/hammer + shield opcional | tanque/melee |
| Berserker | Strength + Breath | two-handed axe/hammer | dano alto/stamina |
| Duelist | Dexterity + Breath | leather + dagger/sword | rápido, parry, crit |
| Archer | Dexterity | leather + bow | ranged físico |
| Battlemage | Intelligence + Strength | staff/sword + hybrid armor | melee mágico |
| Pure Mage | Intelligence + Willpower | cloth + wand/staff | spell damage |
| Warlock/Shadow | Willpower + Intelligence | tome/focus + shadow gear | debuff/status |
| Survivalist | Breath + Dexterity | leather + tools/bow | exploração/crafting |
| Paladin-like futuro | Constitution + Willpower | metal + shield/relic | defesa/sustain |

---

# 18. Contratos sugeridos

## 18.1 PlayerCombatSkillSO

```csharp
public class PlayerCombatSkillSO : ScriptableObject, IIdentifiedData
{
    public string Id;
    public string DisplayName;
    public string SkillTreeId;
    public int RequiredPlayerLevel;
    public int RequiredSkillPoints;
    public string[] RequiredSkillIds;
    public string[] UnlocksActionIds;
    public string[] UnlocksSpellIds;
    public string[] UnlocksWeaponBehaviorIds;
}
```

## 18.2 WeaponCombatProfileSO

```csharp
public class WeaponCombatProfileSO : ScriptableObject, IIdentifiedData
{
    public string Id;
    public string WeaponType;
    public string HandMode;
    public string PrimaryScalingAttribute;
    public string SecondaryScalingAttribute;
    public float BaseStaminaCost;
    public float BaseManaCost;
    public float AttackSpeed;
    public float DamageMultiplier;
    public string[] AllowedSkillIds;
    public string[] DamageTypeIds;
    public string[] StatusIds;
}
```

## 18.3 SpellDataSO

```csharp
public class SpellDataSO : ScriptableObject, IIdentifiedData
{
    public string Id;
    public string DisplayName;
    public string ElementId;
    public string DamageTypeId;
    public string ProjectileId;
    public float ManaCost;
    public float CastTime;
    public string[] StatusIds;
    public string RequiredSkillId;
}
```

## 18.4 PlayerActionDataSO

```csharp
public class PlayerActionDataSO : ScriptableObject, IIdentifiedData
{
    public string Id;
    public string DisplayName;
    public string ActionType;
    public string RequiredSkillId;
    public float StaminaCost;
    public float ManaCost;
    public float CooldownSeconds;
    public bool RequiresShield;
    public bool RequiresMagicItem;
    public bool IsDefensive;
    public bool IsOffensive;
}
```

---

# 19. Hardening

```text
H1: Roll e Dash são ações diferentes.
H2: Roll é defensivo; Dash é ofensivo.
H3: Block exige Shield.
H4: Parry só funciona contra Physical melee e projéteis físicos.
H5: Magia exige Staff, Wand, Focus ou Scroll.
H6: Dual wield leve usa ataque alternado automático.
H7: Dual wield pesado é skill avançada de Combat/Strength.
H8: Toda arma tem Special Attack.
H9: Toda ação especial consome stamina ou mana.
H10: Armor weight afeta Roll, Dash, Sprint e stamina cost.
H11: Bow shot padrão é instantâneo.
H12: Charge Shot é skill.
H13: Crit vem principalmente de skill/equipment.
H14: Dexterity aumenta dano ranged e levemente velocidade.
H15: Bow usa durabilidade padrão até spec futura.
H16: Shield não pode ser usado com two-handed nem dual wield.
H17: Spell cast sem item mágico equipado é proibido, salvo scroll consumível.
```

---

# 20. Critérios de aceite

```text
CA1: Roll é dodge defensivo desbloqueado por skill de Dexterity.
CA2: Dash é avanço ofensivo desbloqueado por skill de Dexterity.
CA3: Block exige Shield e skill.
CA4: Parry só funciona contra physical melee e projéteis físicos.
CA5: Magia exige Staff/Wand/Focus/Scroll.
CA6: Dual wield leve usa ataque alternado automático.
CA7: Dual wield pesado fica como skill avançada Combat/Strength.
CA8: Toda arma tem Special Attack.
CA9: Special Attack consome stamina/mana.
CA10: Bow shot padrão é instantâneo.
CA11: Charge Shot é skill.
CA12: Existem flechas elementais.
CA13: Magias existem por elemento.
CA14: Skill trees iniciais existem: Combat, Dexterity, Magic, Survival/Breath.
CA15: Armor weight afeta mobilidade/stamina.
```

---

# 21. Non-goals

Fora desta spec:

```text
balance final de dano/stamina/mana
animações finais de combate
hitbox/hurtbox avançado
dodge i-frames finais
parry perfeito avançado
shield bash final
árvores de skill completas com dezenas de nós
UI final da skill tree
VFX/SFX finais das magias
PvP/co-op
```

---

# 22. MVP recomendado

MVP desta spec deve começar por contratos e poucas ações:

```text
1. PlayerActionDataSO para Roll/Dash/Sprint/Special Attack.
2. WeaponCombatProfileSO para Sword, Axe, Hammer, Spear, Dagger, Bow, Wand, Staff.
3. SpellDataSO para Arcane Bolt, Fire Spark, Frost Shard.
4. Skill unlock simples para Roll, Dash, Charge Shot, Shield Block, Arcane Bolt.
5. Bow shot instantâneo com ammo/stamina/durability.
6. Special Attack simples por arma.
7. Stamina validation comum.
8. Mana validation comum.
9. Armor weight afetando stamina/move speed.
```

Não implementar árvore gigante completa no primeiro pacote.

---

# 23. Impacto em specs anteriores

Complementa:

- `docs_old/FASE9E_UI_HOTBAR_INVENTORY_EQUIPMENT_SPEC_v1.0.md`
- `docs_old/FASE9E_PLAYER_LEVEL_UP_PROGRESSION_SPEC_v1.0.md`
- `docs_old/FASE9E_DAMAGE_STATUS_FORMULA_SPEC_v1.0.md`
- `docs_old/FASE9H_CAVE_LOOT_CRAFTING_EQUIPMENT_PROGRESSION_SPEC_v1.0.md`
- `docs/amendments/FASE9G_AMENDMENT_ENEMY_COMBAT_ROLES_AI_STATUS_v1.1.md`

Não altera specs antigas destrutivamente.





