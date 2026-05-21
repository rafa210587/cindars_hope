# FASE 9K — Skill Trees, Nodes, Unlocks & Build Progression Spec v1.0

> **Status:** spec aprovada para orientar próximas waves.  
> **Feature:** `FASE9K_SKILL_TREES_FULL_NODE_AND_UNLOCK`  
> **Base:** FASE9E Player Level Up/Progression + FASE9I Player Combat/Weapons/Magic/Skill Trees.  
> **Objetivo:** detalhar árvores de skill, nodes, custos, requisitos, active slots, capstones, respec, unlocks e progressão de builds, com inspiração em arquétipos de feats, skills, weapon masteries e magias de fantasia medieval, sem copiar regras/textos de sistemas externos.

---

## 1. Problema

A FASE9I definiu ações e árvores conceituais:

- Combat;
- Dexterity;
- Magic;
- Survival / Breath.

Mas ainda faltava especificar:

- quantos SkillPoints o jogador ganha;
- quanto cada node custa;
- como funcionam tiers e pré-requisitos;
- como active skills são equipadas;
- capstones e limites;
- respec;
- requisitos de atributo, arma e equipamento;
- lista inicial de nodes;
- identidade de build por árvore.

---

## 2. User story

Como jogador, quero investir SkillPoints em árvores com identidade forte, escolher skills ativas e passivas, montar builds diferentes e poder fazer respec na Fonte de Anya, para que minha progressão tenha escolhas reais até o level 100.

---

## 3. Decisões fechadas

```text
D1: Jogador ganha 1 SkillPoint a cada 2 níveis.
D2: SkillPoints são concedidos nos níveis pares: 2, 4, 6... 100.
D3: Total máximo aproximado = 50 SkillPoints.
D4: Skills podem custar 1, 2 ou 3 pontos.
D5: Skills comuns custam 1 ponto.
D6: Skills fortes/modificadores avançados custam 2 pontos.
D7: Capstones ou skills transformadoras podem custar 3 pontos.
D8: Jogador pode aprender no máximo 2 capstones por árvore.
D9: Respec existe na Fonte de Anya.
D10: Respec custa 100 gold por atributo ou skill resetado.
D11: Skills avançadas podem exigir atributo mínimo.
D12: Skills específicas podem exigir arma/equipamento específico.
D13: Active skills usam 4 slots ativos.
D14: Passivas ficam sempre ativas após aprendidas.
D15: Skill tree fica desbloqueada desde o início.
D16: A árvore deve ter inspiração em arquétipos de feats, skills, weapon masteries e magias de fantasia medieval, mas com nomes/efeitos próprios de Cindar's Hope.
```

---

## 4. SkillPoint economy

### 4.1 Regra

```text
SkillPointGain = +1 SkillPoint a cada 2 níveis
```

Níveis que concedem SkillPoint:

```text
2, 4, 6, 8, 10 ... 100
```

Total máximo:

```text
50 SkillPoints
```

### 4.2 Impacto de balance

Com ~50 pontos até level 100:

```text
O jogador consegue fazer 1 árvore principal forte
+ 1 árvore secundária média
+ alguns pontos utilitários
```

Hardening:

```text
Não permitir que o jogador compre tudo.
Árvores devem ter nodes suficientes.
Skills fortes custam 2–3.
Capstones exigem investimento real.
```

---

## 5. Tiers por árvore

Cada árvore deve ter 5 tiers.

| Tier | Faixa sugerida | Função |
|---:|---:|---|
| Tier 1 | Level 1–10 | skills básicas |
| Tier 2 | Level 11–25 | especialização inicial |
| Tier 3 | Level 26–45 | build definida |
| Tier 4 | Level 46–70 | poderes fortes |
| Tier 5 | Level 71–100 | capstones |

Regras:

```text
Tier exige PlayerLevel mínimo.
Alguns nodes exigem skill anterior.
Alguns nodes exigem atributo mínimo.
Alguns nodes exigem weapon type ou equipment type.
```

---

## 6. Custo por tipo de skill

| Tipo de skill | Custo |
|---|---:|
| Skill básica | 1 |
| Passive pequena | 1 |
| Unlock básico | 1 |
| Modifier simples | 1 |
| Modifier forte | 2 |
| Mastery intermediária | 2 |
| Active skill forte | 2 |
| Capstone | 3 |
| Skill transformadora | 3 |

Exemplos:

```text
Roll = 1
Dash = 1
Parry = 1
Shield Block = 1
Charge Shot = 1
Light Dual Wield = 2
Heavy Dual Wield = 3
Perfect Parry = 3
Archmage Attunement = 3
Environmental Mastery = 3
```

---

## 7. Tipos de skill node

```text
ActiveSkill
PassiveSkill
ModifierSkill
UnlockSkill
MasterySkill
CapstoneSkill
ChoiceSkill
```

### 7.1 ActiveSkill

Desbloqueia ou equipa ação ativa.

Exemplos:

```text
Roll
Dash
Parry
Sprint
Shield Block
Charge Shot
Dagger Flurry
Arcane Bolt
Fire Spark
```

### 7.2 PassiveSkill

Bônus permanente.

Exemplos:

```text
Enduring Breath
Heavy Armor Discipline
Mana Discipline
Forager's Eye
```

### 7.3 ModifierSkill

Altera skill existente.

Exemplos:

```text
Dash Strike
Projectile Parry
Piercing Shot
Improved Sprint
Spell Shaping
```

### 7.4 UnlockSkill

Libera comportamento/equipamento.

Exemplos:

```text
Light Dual Wield
Heavy Dual Wield
Shield Block
Scroll Mastery
Field Repair Basics
```

### 7.5 MasterySkill

Especialização por arma, estilo ou item.

Exemplos:

```text
Cleaving Mastery
Crushing Mastery
Piercing Mastery
Wand Adept
Staff Adept
```

### 7.6 CapstoneSkill

Skill final de árvore ou sub-build.

Exemplos:

```text
Unbreakable Bastion
Storm of Blades
Archmage Attunement
Caveborn Survivor
```

---

# 8. Active Skill Slots

## 8.1 Regra

```text
Jogador tem 4 Active Skill Slots.
```

Passivas:

```text
sempre ativas após aprendidas
```

Ativas:

```text
precisam ser equipadas em um dos 4 slots
```

## 8.2 Exemplos de active skills

```text
Roll
Dash
Parry
Sprint
Charge Shot
Shield Block
Dagger Flurry
Arcane Bolt
Fire Spark
Frost Shard
Blackstone Pulse
Sweeping Assault
Backstep Strike
```

## 8.3 Hardening

```text
Active slots forçam escolha real.
Passivas não usam slots.
MVP pode começar com 4 slots simples OnGUI/debug.
Skills aprendidas mas não equipadas não devem ativar ação ativa.
```

Exemplos de loadout:

```text
Archer: Roll, Dash, Charge Shot, Parry
Mage: Roll, Arcane Bolt, Fire Spark, Frost Shard
Tank: Shield Block, Power Strike/Sweeping Assault, Sprint, Guard Break
Survivalist: Sprint, Roll, Return/Utility future, Tool/Crafting action future
```

---

# 9. Capstones

## 9.1 Regra

```text
Cada árvore pode ter múltiplas capstones disponíveis.
Jogador pode aprender no máximo 2 capstones por árvore.
Capstone custa 3 SkillPoints.
```

## 9.2 Hardening

```text
Capstone exige PlayerLevel alto.
Capstone exige SkillPointsSpentInTree alto.
Capstone exige skills anteriores.
Capstone pode exigir atributo mínimo.
Capstone deve mudar build, não só dar número maior.
```

Exemplo:

```text
Perfect Parry
Cost = 3
RequiredLevel = 75
RequiredDexterity = 22
RequiredSkillPointsSpentInTree = 20
RequiredSkills = Parry, Projectile Parry, Improved Roll
```

---

# 10. Respec na Fonte de Anya

## 10.1 Regra

```text
Respec acontece na Fonte de Anya.
Custa 100 gold por atributo ou skill resetado.
```

## 10.2 Tipos

| Respec | Custo |
|---|---:|
| Resetar 1 AttributePoint aplicado | 100 gold |
| Resetar 1 SkillNode aprendido | 100 gold |
| Resetar árvore inteira | 100 × número de skills daquela árvore |
| Resetar todos atributos | 100 × pontos redistribuídos |
| Resetar tudo | soma de atributos + skills |

## 10.3 Hardening

```text
Não permitir respec dentro da cave.
Não permitir respec durante combate.
Não permitir respec se quebrar pré-requisito sem remover dependentes.
Se remover uma skill pré-requisito, skills dependentes devem ser removidas ou bloqueadas.
```

---

# 11. Requisitos de atributo, arma e equipamento

Skills avançadas podem exigir atributo mínimo.

Exemplos:

```text
Heavy Dual Wield
RequiredStrength = 24
```

```text
Perfect Parry
RequiredDexterity = 22
```

```text
Meteor Channel
RequiredIntelligence = 24
RequiredWillpower = 18
```

```text
Environmental Mastery
RequiredBreath = 20
```

Skills podem exigir arma/equipamento:

```text
Crushing Mastery requires Hammer
Charge Shot requires Bow
Shield Block requires Shield
Wand Adept requires Wand
Heavy Dual Wield requires Heavy Weapon categories
```

---

# 12. Direção de design D&D-like sem cópia

A árvore deve parecer menos genérica:

```text
+5% dano
+10 stamina
ataque forte
```

E mais orientada a arquétipos:

```text
estilo de combate
maestria de arma
disciplina defensiva
técnica marcial
conjuração elemental
sobrevivência de dungeon
```

Fontes de inspiração conceitual:

```text
fighting styles
weapon mastery
feat-like unlocks
skill expertise
spell progression
metamagic-like modifiers
capstone boons
```

Regra de segurança criativa:

```text
Não copiar textos, nomes únicos proprietários, regras exatas ou progressões externas.
Usar nomes próprios e efeitos próprios de Cindar's Hope/Vaalara.
```

---

# 13. Combat Tree revisada

Foco:

```text
melee
shield
block
heavy weapons
two-handed
stagger
special attacks
defesa
dual wield pesado
```

## 13.1 Tier 1 — Fundamentos marciais

| Skill | Custo | Tipo | Efeito |
|---|---:|---|---|
| Martial Initiate | 1 | Passive | libera base de técnicas marciais e pré-requisitos Combat |
| Weapon Special Attack I | 1 | Unlock | libera special attack básico por arma |
| Dueling Form | 1 | Style | melhora uso de arma 1H sem dual wield |
| Great Weapon Form | 1 | Style | melhora arma 2H, com maior custo de stamina |
| Shield Training | 1 | Unlock | permite equipar shield corretamente, pré-req para block |

## 13.2 Tier 2 — Maestrias de arma

| Skill | Custo | Tipo | Efeito |
|---|---:|---|---|
| Cleaving Mastery | 2 | Mastery | axes/swords podem atingir arco frontal maior |
| Crushing Mastery | 2 | Mastery | hammers aumentam stagger/knockback |
| Piercing Mastery | 2 | Mastery | spears perfuram linha curta |
| Shield Block | 1 | Active | permite bloquear com shield |
| Guard Break | 1 | Active/Modifier | special attack causa pressão contra defesa |

## 13.3 Tier 3 — Controle e defesa

| Skill | Custo | Tipo | Efeito |
|---|---:|---|---|
| Sentinel Stance | 2 | Passive | melee ajuda a controlar espaço e avanço inimigo |
| Bulwark Guard | 2 | Modifier | block reduz mais dano frontal |
| Heavy Armor Discipline | 2 | Passive | reduz penalidade de stamina de armor pesada |
| Battle Endurance | 1 | Passive | reduz custo de stamina de melee básico |
| Staggering Blow | 2 | Modifier | special de hammer/2H causa stagger maior |

## 13.4 Tier 4 — Técnicas avançadas

| Skill | Custo | Tipo | Efeito |
|---|---:|---|---|
| Heavy Dual Wield I | 3 | Unlock | permite duas armas pesadas com penalidade alta |
| Titan Grip | 2 | Passive | reduz penalidade de armas pesadas |
| Counter Guard | 2 | Modifier | block perfeito abre janela curta de counter |
| Sweeping Assault | 2 | Active | ataque circular melee |
| Giantbreaker | 2 | Passive | bônus contra Brute, Elite, Miniboss e Boss |

## 13.5 Tier 5 — Capstones Combat

| Capstone | Custo | Efeito |
|---|---:|---|
| Unbreakable Bastion | 3 | block com shield fica muito eficiente, mas ainda consome stamina |
| Colossus Breaker | 3 | armas pesadas causam bônus forte contra bosses/minibosses |
| War Master | 3 | special attacks marciais custam menos stamina |
| Twin Titans | 3 | melhora Heavy Dual Wield e reduz penalidades |
| Iron Vanguard | 3 | armor pesada ganha melhor relação defesa/peso |

---

# 14. Dexterity Tree revisada

Foco:

```text
roll
dash
parry
bow
dagger
dual wield leve
charge shot
crit
mobilidade
velocidade
```

## 14.1 Tier 1 — Agilidade treinada

| Skill | Custo | Tipo | Efeito |
|---|---:|---|---|
| Acrobat's Footwork | 1 | Passive | leve bônus de move speed |
| Roll | 1 | Active | libera dodge defensivo |
| Dash | 1 | Active | libera avanço ofensivo |
| Quick Draw | 1 | Passive | bow shot mais fluido |
| Knife Practice | 1 | Mastery | dagger tem melhor stamina efficiency |

## 14.2 Tier 2 — Precisão e reação

| Skill | Custo | Tipo | Efeito |
|---|---:|---|---|
| Parry | 1 | Active | parry contra melee physical |
| Light Dual Wield | 2 | Unlock | permite duas armas leves |
| Charge Shot | 1 | Active | tiro carregado com bow |
| Skirmisher Step | 2 | Passive | após ataque leve, melhora reposicionamento |
| Dash Strike | 1 | Modifier | dash pode iniciar ataque ofensivo |

## 14.3 Tier 3 — Técnica refinada

| Skill | Custo | Tipo | Efeito |
|---|---:|---|---|
| Projectile Parry | 2 | Modifier | permite parry de projéteis físicos |
| Dagger Flurry | 2 | Active | sequência rápida com dagger |
| Bow Mastery | 2 | Mastery | bow ganha dano/controle melhor |
| Precision Training | 2 | Passive | crit chance por skill/equipment melhora |
| Evasive Footwork | 2 | Passive | roll/dash sofrem menos penalidade de armor leve |

## 14.4 Tier 4 — Especialização

| Skill | Custo | Tipo | Efeito |
|---|---:|---|---|
| Dual Wield II | 2 | Modifier | reduz custo de stamina de dual wield leve |
| Piercing Shot | 2 | Modifier | Charge Shot perfura inimigos em linha curta |
| Backstep Strike | 2 | Active | recua e ataca |
| Weakpoint Reading | 2 | Passive | crits causam mais dano |
| Shadowstep Feint | 2 | Modifier | dash reduz aggro/abre janela curta de evasão, sem invulnerabilidade total |

## 14.5 Tier 5 — Capstones Dexterity

| Capstone | Custo | Efeito |
|---|---:|---|
| Storm of Blades | 3 | dual wield leve ganha combo avançado |
| Perfect Parry | 3 | parry perfeito abre counter forte |
| Deadeye | 3 | bow ganha grande bônus de precisão/dano |
| Untouchable Footwork | 3 | roll/dash ficam muito eficientes com armor leve |
| Phantom Duelist | 3 | dagger/parry build ganha burst após evasão perfeita |

---

# 15. Magic Tree revisada

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
elemental attunement
```

## 15.1 Tier 1 — Iniciação arcana

| Skill | Custo | Tipo | Efeito |
|---|---:|---|---|
| Arcane Initiate | 1 | Passive | permite usar magia básica com wand/staff/focus |
| Arcane Bolt | 1 | Active Spell | projétil arcano básico |
| Fire Spark | 1 | Active Spell | projétil Fire com chance Burn |
| Frost Shard | 1 | Active Spell | projétil Ice com chance Chill |
| Mana Discipline I | 1 | Passive | reduz custo de mana leve |

## 15.2 Tier 2 — Círculo elemental menor

| Skill | Custo | Tipo | Efeito |
|---|---:|---|---|
| Venom Dart | 1 | Active Spell | Poison/Nature DoT |
| Shadow Spark | 1 | Active Spell | Shadow Mark/debuff |
| Spark Jolt | 1 | Active Spell | Lightning/Thunder rápido |
| Wand Adept | 1 | Mastery | wand melhora cast rápido |
| Staff Adept | 1 | Mastery | staff melhora dano/cast pesado |

## 15.3 Tier 3 — Adepto arcano

| Skill | Custo | Tipo | Efeito |
|---|---:|---|---|
| Acid Glob | 1 | Active Spell | Acid, futuro efeito em defesa |
| Blackstone Pulse | 2 | Active Spell | Corruption avançado |
| Elemental Adept | 2 | Passive | melhora dano elemental escolhido |
| Spell Shaping | 2 | Modifier | reduz penalidade de cast sob pressão |
| Focus Adept | 2 | Mastery | focus melhora status/controle |

## 15.4 Tier 4 — Metamagia / controle

| Skill | Custo | Tipo | Efeito |
|---|---:|---|---|
| Mana Discipline II | 2 | Passive | reduz mais custo de mana |
| Faster Casting | 2 | Passive | reduz cast time |
| Elemental Specialization | 2 | Choice | escolher elemento favorecido |
| Scroll Mastery | 2 | Unlock | scrolls ficam mais eficientes |
| Arcane Recovery | 2 | Passive | recupera pequena mana após condições específicas |

## 15.5 Tier 5 — Capstones Magic

| Capstone | Custo | Efeito |
|---|---:|---|
| Archmage Attunement | 3 | grande bônus para magias elementais |
| Master of Status | 3 | status mágicos duram mais/aplicam melhor |
| Twin Casting | 3 | chance/cooldown de duplicar spell futuro |
| Meteor Channel | 3 | magia avançada ligada a Elyndor/Meteoro |
| Blackstone Theurge | 3 | build de Corruption/Shadow fica muito forte, com risco/custo alto |

---

# 16. Survival / Breath Tree revisada

Foco:

```text
sprint
stamina
durabilidade
resistência ambiental
crafting
gathering
cave sustain
food/potions
tools
```

## 16.1 Tier 1 — Sobrevivente de caverna

| Skill | Custo | Tipo | Efeito |
|---|---:|---|---|
| Sprint | 1 | Active | libera sprint |
| Enduring Breath | 1 | Passive | melhora stamina máxima/regen |
| Tool Proficiency | 1 | Passive | reduz stamina no uso de tools |
| Camp Crafter | 1 | Passive | reduz craft time simples |
| Forager's Eye | 1 | Passive | chance pequena de recurso extra |

## 16.2 Tier 2 — Preparo e resistência

| Skill | Custo | Tipo | Efeito |
|---|---:|---|---|
| Durability Care | 2 | Passive | reduz desgaste de tools/weapons |
| Heat Adaptation I | 1 | Passive | bônus HeatResistance |
| Cold Adaptation I | 1 | Passive | bônus ColdResistance |
| Stamina Recovery I | 1 | Passive | melhora regen de stamina |
| Field Repair Basics | 2 | Unlock | reparo simples em safe rooms futuro |

## 16.3 Tier 3 — Dungeon expertise

| Skill | Custo | Tipo | Efeito |
|---|---:|---|---|
| Improved Sprint | 1 | Modifier | sprint custa menos stamina |
| Cave Forager | 2 | Passive | melhora drops de nodes |
| Hunger Discipline | 1 | Passive | fome cai mais devagar |
| Tool Specialist | 2 | Passive | tool gates consomem menos durability |
| Faster Crafting II | 2 | Passive | reduz mais craft time |

## 16.4 Tier 4 — Preparação avançada

| Skill | Custo | Tipo | Efeito |
|---|---:|---|---|
| Heat Adaptation II | 2 | Passive | mais HeatResistance |
| Cold Adaptation II | 2 | Passive | mais ColdResistance |
| Emergency Recovery | 2 | Passive | bônus quando HP baixo futuro |
| Pack Preparation | 2 | Passive | bônus de loadout/inventory futuro |
| Return Stone Efficiency | 2 | Passive | melhora uso/crafting de item de retorno |

## 16.5 Tier 5 — Capstones Survival

| Capstone | Custo | Efeito |
|---|---:|---|
| Caveborn Survivor | 3 | grande bônus de sustain na cave |
| Master Gatherer | 3 | bônus forte em recursos raros |
| Unbreakable Routine | 3 | reduz bastante perda de durabilidade |
| Environmental Mastery | 3 | alta resistência a heat/cold |
| Anya's Second Breath | 3 | efeito raro de sobrevivência/recuperação ligado à Fonte de Anya, com cooldown alto |

---

# 17. Hardening de balance

```text
H1: Com ~50 SkillPoints totais, jogador não pode pegar tudo.
H2: Skills podem custar 1–3 pontos.
H3: Capstones custam caro e exigem compromisso real.
H4: Máximo de 2 capstones por árvore.
H5: Active skills usam 4 slots.
H6: Passivas ficam sempre ativas.
H7: Respec existe na Fonte de Anya.
H8: Respec custa 100 gold por atributo ou skill resetado.
H9: Remover pré-requisito deve remover/bloquear dependentes.
H10: Skills avançadas podem exigir atributo mínimo.
H11: Skills de arma podem exigir weapon/equipment type.
H12: Skill trees ficam disponíveis desde o início.
H13: Feat-like nodes não devem ser só bônus numérico.
H14: Capstone precisa mudar build.
H15: Magic não pode pegar todos os elementos fácil demais.
H16: Survival precisa ser competitiva.
H17: Combat e Dexterity não devem ocupar o mesmo espaço.
```

Separação:

```text
Combat = força, arma pesada, escudo, controle físico
Dexterity = evasão, parry, bow, dagger, dual wield leve
Magic = spells, elementos, mana, status, staff/wand/focus
Survival = stamina, cave sustain, crafting, gathering, resistências ambientais
```

---

# 18. Contratos sugeridos

## 18.1 SkillTreeSO

```csharp
public class SkillTreeSO : ScriptableObject, IIdentifiedData
{
    public string Id;
    public string DisplayName;
    public string Description;
    public string[] RootSkillIds;
}
```

## 18.2 SkillNodeSO

```csharp
public class SkillNodeSO : ScriptableObject, IIdentifiedData
{
    public string Id;
    public string DisplayName;
    public string SkillTreeId;
    public string SkillNodeType;

    public int Tier;
    public int Cost;

    public int RequiredPlayerLevel;
    public int RequiredSkillPointsSpentInTree;

    public string[] RequiredSkillIds;

    public string RequiredAttributeId;
    public int RequiredAttributeValue;

    public string RequiredWeaponType;
    public string RequiredEquipmentType;

    public string[] UnlocksActionIds;
    public string[] UnlocksSpellIds;
    public string[] UnlocksWeaponBehaviorIds;

    public SkillEffect[] Effects;

    public bool IsCapstone;
}
```

## 18.3 SkillTreeRulesSO

```csharp
public class SkillTreeRulesSO : ScriptableObject, IIdentifiedData
{
    public string Id;
    public int SkillPointEveryLevels;       // 2
    public int MaxCapstonesPerTree;         // 2
    public int ActiveSkillSlots;            // 4
    public int RespecGoldCostPerPoint;      // 100
}
```

## 18.4 PlayerSkillProgressionSaveData

```csharp
[Serializable]
public class PlayerSkillProgressionSaveData
{
    public int AvailableSkillPoints;
    public List<string> LearnedSkillIds;
    public List<string> EquippedActiveSkillIds; // max 4
}
```

---

# 19. Critérios de aceite

```text
CA1: Jogador ganha 1 SkillPoint a cada 2 níveis.
CA2: Skills podem custar 1, 2 ou 3 pontos.
CA3: Jogador tem 4 Active Skill Slots.
CA4: Passivas ficam sempre ativas.
CA5: Máximo de 2 capstones por árvore.
CA6: Respec existe na Fonte de Anya.
CA7: Respec custa 100 gold por atributo ou skill resetado.
CA8: Skills avançadas podem exigir atributo mínimo.
CA9: Skills específicas podem exigir arma/equipamento específico.
CA10: Skill trees ficam disponíveis desde o início.
CA11: Combat Tree inclui estilos, maestrias, shield, heavy weapons e capstones.
CA12: Dexterity Tree inclui roll, dash, parry, bow, dagger, dual wield leve e capstones.
CA13: Magic Tree inclui spells por elemento, mana, cast, focus/staff/wand e capstones.
CA14: Survival Tree inclui sprint, stamina, durability, environmental resistance, crafting, gathering e capstones.
CA15: Skill nodes têm identidade forte e não são apenas bônus numéricos genéricos.
```

---

# 20. Non-goals

Fora desta spec:

```text
UI final da skill tree
layout visual final dos nodes
balance final de todos os valores
animações finais de active skills
VFX/SFX finais
respec UI final
conteúdo completo de 180 nodes
sistema de classes rígidas
multiplayer/co-op skill sync
```

---

# 21. MVP recomendado

MVP deve começar com dados e fluxo simples:

```text
1. SkillTreeRulesSO com SkillPointEveryLevels=2, ActiveSkillSlots=4, MaxCapstonesPerTree=2.
2. SkillTreeSO para Combat, Dexterity, Magic, Survival.
3. SkillNodeSO com subset inicial de 5–8 nodes por árvore.
4. PlayerSkillProgressionSaveData com LearnedSkillIds e EquippedActiveSkillIds.
5. Skill purchase validator.
6. Active skill equip validator.
7. Respec simples na Fonte de Anya.
8. Debug/OnGUI simples para comprar/equipar skills.
```

Não implementar árvore completa gigante no primeiro pacote.

---

# 22. Impacto em specs anteriores

Complementa:

- `docs/FASE9E_PLAYER_LEVEL_UP_PROGRESSION_SPEC_v1.0.md`
- `docs/FASE9I_PLAYER_COMBAT_WEAPONS_MAGIC_SKILL_TREES_SPEC_v1.0.md`
- `docs/FASE9J_CAVE_RUN_ENTRY_LOADOUT_HUD_AND_FAILURE_FLOW_SPEC_v1.0.md`

Não altera specs antigas destrutivamente; ajusta a economia final de SkillPoints em spec posterior.
