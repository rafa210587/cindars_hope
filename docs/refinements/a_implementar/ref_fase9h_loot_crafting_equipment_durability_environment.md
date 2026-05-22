# REF FUTURO — FASE9H loot crafting equipment durability environment

> Origem histórica: $Source
> Status: Refinamento futuro preservado
> Spec futura relacionada: $Spec

---

## Decisões preservadas

Conteúdo histórico preservado abaixo para evitar perda operacional de decisões, escopo e pendências.

---

# FASE 9H — Cave Loot, Crafting, Equipment & Gear Progression Spec v1.0

> **Status:** spec aprovada para orientar próximas waves.  
> **Feature:** `FASE9H_CAVE_LOOT_CRAFTING_EQUIPMENT_PROGRESSION`  
> **Base:** FASE9E Item/Save/Progression + FASE9F Cave Resources + FASE9G Bestiary/Faction Locks + FASE9G Amendment Combat/AI/Status.  
> **Objetivo:** definir progressão material da cave, drops por família, boss drops, refinamento, crafting com tempo, durabilidade, equipamentos RPG, armaduras, resistência ambiental, armas elementais quebradas e gates por ferramenta/equipamento.

---

## 1. Problema

As specs anteriores definem:

- estrutura procedural da cave;
- cave levels, checkpoints, seeds e resource nodes;
- bestiário e faction locks;
- comportamento de combate, status e bosses/minibosses.

Ainda falta definir por que o jogador desce mais fundo:

- quais recursos obtém;
- como esses recursos viram equipamento;
- como refina materiais;
- como repara armas quebradas;
- como crafta com tempo;
- como armaduras criam builds;
- como resistência ambiental bloqueia/permite avanço;
- como ferramentas bloqueiam nodes e caminhos;
- como boss drops destravam progressão.

Sem isso, a cave tem estrutura e inimigos, mas não tem economia de progressão material.

---

## 2. User story

Como jogador, quero explorar a cave, coletar materiais, derrotar criaturas e bosses, refinar recursos, reparar armas raras, craftar equipamentos e vencer bloqueios ambientais/ferramentais para avançar cada vez mais fundo.

---

## 3. Decisões fechadas

```text
D1: Equipamento será mais RPG, com múltiplas armas/builds por tier.
D2: Teremos armaduras no MVP, mas poucas.
D3: Armor será peça única no MVP.
D4: Teremos 1 slot de accessory no MVP.
D5: Armaduras dão defesa, atributos e resistências ambientais.
D6: Existem resistências ambientais separadas: HeatResistance e ColdResistance.
D7: Ambientes quentes/frios podem causar dano se o jogador não tiver resistência suficiente.
D8: Armaduras de pano favorecem Intelligence e Willpower.
D9: Armaduras de couro favorecem Dexterity, crítico e mobilidade.
D10: Armaduras metálicas favorecem defesa e resistências, mas aumentam peso.
D11: Peso reduz movimento e aumenta custo de stamina de habilidades/ataques.
D12: Drops de boss podem ser key items, itens vendáveis raros ou ambos.
D13: Armas elementais dropadas por humanoides vêm quebradas/danificadas.
D14: Crafting tem tempo de execução segurando botão.
D15: Quanto mais forte o item, maior o tempo de crafting.
D16: Level e Dexterity reduzem tempo de crafting.
D17: Refinamento/crafting usa várias estações.
D18: Tool tier bloqueia nodes e também bloqueia avanço em certas áreas da cave.
D19: Durabilidade existe no MVP.
D20: Todo item equipável tem DurabilityMax = 100.
D21: A cada 3 usos relevantes, o item perde 1 ponto de durabilidade.
D22: Resistência ambiental pode vir de armor + accessory + food/potion futuro.
D23: Crafting em área hostil cancela se o jogador tomar dano.
D24: Tool gate pode bloquear caminho principal se ferramenta/material estiver disponível antes.
```

---

## 4. Modelo de progressão

Progressão principal:

```text
CaveLevel
→ Resource Tier
→ Raw Material
→ Refined Material
→ Recipe Unlock
→ Crafted/Repaired Gear
→ Tool/Environment Gate
→ Deeper CaveLevel
```

Exemplo:

```text
CaveLevel 31–45
→ Frostwood + Ice Crystal + Deep Iron
→ Refined Frostwood + Refined Ice Crystal + Deep Iron Ingot
→ Cold-resistant armor / Frost weapon
→ acesso seguro a áreas geladas
```

---

## 5. Material tiers por faixa

| Tier | CaveLevels | Materiais principais | Uso |
|---:|---:|---|---|
| T1 | 1–15 | Stone, Copper Ore, Cave Root Wood, Slime Gel, Meteor Dust | ferramentas básicas, armas simples |
| T2 | 16–30 | Iron Ore, Underground Hardwood, Root Fiber, Fungal Spores, Worg Hide | armas/armaduras iniciais |
| T3 | 31–45 | Deep Iron, Frostwood, Ice Crystal, Drow Silk, Deepforge Coal | resistência a frio, armas frost |
| T4 | 46–60 | Gold Ore, Emberwood, Fire Crystal, Ash Core, Wyvern Scale | resistência a calor, armas fire |
| T5 | 61–75 | Ancient Timber, Relic Fragment, Arcane Dust, Elyndor Rune Plate, Prism Shard | gear arcano, foco mágico |
| T6 | 76–90 | Gloomwood, Dark Crystal, Vampire Fang, Blackstone Shard, Ectoplasm | shadow gear, curse/corruption |
| T7 | 91–99 | Diamond, Arcane Ore, Corrupted Heartwood, Dragon Scale, Core Shard | endgame gear |
| T8 | 100 | Cave Heart, Elyndor Core, Meteor Relic | lendário/lore/endgame |

---

## 6. Refinamento

Refinamento transforma material bruto em material utilizável.

### 6.1 Exemplos de bruto → refinado

| Bruto | Refinado | Estação |
|---|---|---|
| `item_ore_copper` | `item_ingot_copper` | Forge |
| `item_ore_iron` | `item_ingot_iron` | Forge |
| `item_ore_gold` | `item_ingot_gold` | Forge |
| `item_ore_arcane` | `item_ingot_arcane` | Arcane Forge |
| `item_material_cave_root_wood` | `item_material_refined_cave_wood` | Workbench |
| `item_material_underground_hardwood` | `item_material_refined_hardwood` | Workbench |
| `item_material_frostwood` | `item_material_refined_frostwood` | Workbench |
| `item_material_emberwood` | `item_material_refined_emberwood` | Workbench |
| `item_material_gloomwood` | `item_material_refined_gloomwood` | Workbench / Arcane Table |
| `item_material_corrupted_heartwood` | `item_material_refined_corrupted_heartwood` | Arcane Table |
| `item_crystal_ice` | `item_crystal_refined_ice` | Alchemy Table |
| `item_crystal_fire` | `item_crystal_refined_fire` | Alchemy Table |
| `item_crystal_dark` | `item_crystal_refined_dark` | Alchemy Table / Arcane Table |
| `item_blackstone_shard` | `item_refined_blackstone_shard` | Arcane Table |
| `item_elyndor_rune_plate` | `item_refined_elyndor_rune_plate` | Arcane Forge |

### 6.2 Hardening de refinamento

```text
Refinamento não deve gerar material acima do tier atual sem fonte apropriada.
Refinamento deve usar ItemId estável.
Receitas de refinamento devem ser data-driven.
Materiais refinados devem ter sell value maior que materiais brutos, salvo key items.
```

---

## 7. Estações de crafting/refinamento

| Estação | Função |
|---|---|
| Workbench | madeira, couro, componentes simples, ferramentas básicas |
| Forge | minérios, lingotes, armas metálicas, armaduras metálicas |
| Tannery / Leather Rack | couro, peles, armaduras leves |
| Alchemy Table | cristais, poções, venenos, materiais elementais |
| Arcane Table | magia, focos, pergaminhos, affixes arcanos |
| Arcane Forge | equipamentos late game, Meteor/Elyndor/Blackstone |
| Repair Bench | reparo de armas quebradas e equipamentos danificados |

MVP mínimo permitido:

```text
Workbench
Forge
Repair Bench
```

As demais estações devem ficar reservadas em dados/spec para waves futuras.

---

## 8. Crafting com tempo segurando botão

Crafting não é instantâneo. O jogador segura botão de craft/interação até completar a barra.

### 8.1 Fórmula alvo

```text
FinalCraftTime =
  BaseCraftTime
  Ã— ItemPowerMultiplier
  Ã— StationMultiplier
  Ã— WeightComplexityMultiplier
  Ã— max(0.35, 1 - LevelReduction - DexterityReduction)
```

### 8.2 Reduções

```text
LevelReduction = PlayerLevel Ã— 0.003
DexterityReduction = Dexterity Ã— 0.004
```

Cap inicial:

```text
redução máxima por Level + Dexterity = 65%
tempo mínimo = 35% do tempo base
```

### 8.3 Tempos base sugeridos

| Item | Tempo base |
|---|---:|
| Material simples refinado | 2–4s |
| Ferramenta T1 | 5–8s |
| Arma T1/T2 | 8–15s |
| Armadura leve | 12–20s |
| Armadura metálica | 18–35s |
| Item elemental | 20–45s |
| Reparo de arma elemental quebrada | 20–60s |
| Item boss/rare | 45–90s |
| Item lendário | 90s+ |

### 8.4 Cancelamento

```text
Se cancelar antes de concluir, não consome materiais no MVP.
Se jogador toma dano enquanto crafta em área hostil, crafting cancela.
Na Farm/Town, não cancela por dano salvo sistemas futuros.
```

Futuro:

```text
Receitas avançadas podem consumir parte dos materiais se canceladas muito tarde.
```

---

## 9. Durabilidade

Durabilidade existe no MVP.

### 9.1 Regra base

```text
Todo item equipável tem DurabilityMax = 100.
DurabilityCurrent inicia em 100.
A cada 3 usos relevantes, perde 1 ponto de durabilidade.
```

### 9.2 Aplicação por tipo

| Tipo | Quando perde durabilidade |
|---|---|
| Weapon | a cada 3 ataques executados |
| Tool | a cada 3 usos/hits em node |
| Bow | a cada 3 disparos |
| MagicFocus/Wand/Staff | a cada 3 casts |
| Armor | a cada 3 hits recebidos |
| Accessory | no MVP não perde durabilidade, salvo item especial |

### 9.3 Estados de durabilidade

| Durabilidade | Estado | Efeito |
|---:|---|---|
| 100–51 | Normal | sem penalidade |
| 50–21 | Worn | aviso visual, sem penalidade forte |
| 20–1 | Damaged | penalidade leve |
| 0 | Broken | item não funciona ou funciona muito mal |

### 9.4 Penalidades sugeridas

```text
Weapon Broken: dano reduzido drasticamente ou não pode atacar.
Tool Broken: não coleta node principal.
Armor Broken: perde defesa/resistências.
MagicFocus Broken: não conjura ou aumenta custo de mana.
```

Hardening:

```text
Item quebrado não deve ser apagado.
Item quebrado fica no inventário/equipamento com efeito reduzido ou bloqueado.
Durability não deve ser aplicada a itens consumíveis comuns.
Durability deve ser salva/carregada quando stacks reais/equipment estiverem prontos.
```

---

## 10. Reparo

Reparo usa:

```text
Repair Bench
material compatível
tempo de crafting
custo proporcional ao dano
```

Exemplo:

```text
Iron Sword com 60/100 durability
→ Repair Bench
→ Iron Ingot x1
→ tempo curto

Iron Sword com 0/100 durability
→ Repair Bench
→ Iron Ingot x2
→ tempo maior
```

Reparo de arma elemental quebrada:

```text
Broken Frost Drow Blade
+ Deep Iron Ingot
+ Refined Ice Crystal
+ Drow Silk
→ Repaired Frost Drow Blade
```

Hardening:

```text
Broken elemental weapons não devem ser usáveis como armas fortes.
Podem ser vendáveis por pouco ou usadas como material.
Reparo deve validar estação, materiais e tempo.
```

---

## 11. Equipamentos RPG

Equipamento não deve ser apenas linear. Cada tier pode ter opções de build.

### 11.1 Categorias

```text
Weapon
Tool
Armor
Accessory
MagicFocus
Shield futuro
Ammo
```

### 11.2 Slots MVP

```text
RightHand
LeftHand
Armor
Accessory1
Ammo
```

Observação:

```text
Armor é peça única no MVP.
Accessory tem 1 slot no MVP.
```

---

## 12. Armaduras

### 12.1 Tipos de armadura

| Tipo | Bônus | Penalidade |
|---|---|---|
| Cloth | Intelligence, Willpower, mana, casting | baixa defesa física |
| Leather | Dexterity, crit, movimento, stamina efficiency | defesa média-baixa |
| Metal | defesa, resistências, poise | peso, menor velocidade, maior custo de stamina |
| Hybrid | mistura de atributos | penalidades médias |
| Elemental | resistência ambiental/status | custo alto, materiais raros |

### 12.2 Stats de armadura

```csharp
public class ArmorStats
{
    public int Defense;
    public int MagicDefense;
    public int HeatResistance;
    public int ColdResistance;
    public int FireResistance;
    public int IceResistance;
    public int PoisonResistance;
    public int ShadowResistance;
    public int ArcaneResistance;
    public int StrengthBonus;
    public int DexterityBonus;
    public int IntelligenceBonus;
    public int WillpowerBonus;
    public int ConstitutionBonus;
    public int BreathBonus;
    public float CritChanceBonus;
    public float MoveSpeedMultiplier;
    public float StaminaCostMultiplier;
    public float CastTimeMultiplier;
    public int WeightClass;
}
```

### 12.3 WeightClass

| WeightClass | Tipo | Movimento | Stamina |
|---:|---|---:|---:|
| 0 | Sem armadura | 1.00x | 1.00x |
| 1 | Cloth | 0.98–1.00x | 1.00x |
| 2 | Leather | 0.95–1.00x | 0.95–1.00x |
| 3 | Hybrid | 0.90–0.96x | 1.05–1.15x |
| 4 | Metal | 0.82–0.92x | 1.15–1.35x |
| 5 | Heavy Metal / Endgame | 0.75–0.88x | 1.35–1.60x |

Hardening:

```text
Metal armor não deve ser sempre melhor.
Cloth e Leather precisam ser builds válidas.
Peso deve impactar stamina/movement de forma clara no HUD/debug futuro.
```

### 12.4 Exemplos de armaduras

#### Cloth

| Item | Bônus |
|---|---|
| Apprentice Cloth Robe | Intelligence +1, Willpower +1 |
| Frostwoven Robe | Intelligence +2, ColdResistance |
| Ash-Sealed Robe | Willpower +2, HeatResistance |
| Elyndor Thread Robe | Intelligence +3, ArcaneResistance |

#### Leather

| Item | Bônus |
|---|---|
| Cave Leather Armor | Dexterity +1 |
| Worghide Armor | Dexterity +2, CritChance |
| Frosthide Armor | Dexterity +2, ColdResistance |
| Shadowhide Armor | Dexterity +3, ShadowResistance |

#### Metal

| Item | Bônus |
|---|---|
| Copper Mail | Defense baixo |
| Iron Mail | Defense médio |
| Deepforge Plate | Defense alto, ColdResistance, peso alto |
| Emberguard Plate | Defense alto, HeatResistance, peso alto |
| Elyndor Runic Plate | Defense alto, ArcaneResistance |

---

## 13. Resistência ambiental

Separar resistência ambiental de resistência elemental:

```text
HeatResistance != FireResistance
ColdResistance != IceResistance
```

| Resistência | Protege contra |
|---|---|
| HeatResistance | calor ambiental, magma nearby, Fire Cave heat zones |
| FireResistance | dano direto Fire, Burn |
| ColdResistance | frio ambiental, Ice Cave cold zones |
| IceResistance | dano direto Ice, Chill/Freeze |

### 13.1 Regra de dano ambiental

Cada área perigosa tem requisito:

```text
RequiredHeatResistance
RequiredColdResistance
```

Se jogador não atinge:

```text
EnvironmentalDamagePerTick = BaseDamage Ã— DeficitMultiplier
```

Exemplo:

```text
RequiredColdResistance = 20
PlayerColdResistance = 12
Deficit = 8
Jogador toma dano ambiental periódico.
```

### 13.2 Hardening ambiental

```text
Ambiente deve avisar antes de causar dano pesado.
Primeiras áreas devem causar dano leve.
Ãreas críticas podem bloquear progressão se resistência for insuficiente.
Resistência ambiental pode vir de armor + accessory + food/potion futuro.
```

---

## 14. Ferramentas

Ferramentas são mais lineares em tier, mas podem ter variações especiais.

### 14.1 Tool tiers

| Tier | Pickaxe | Axe | Sickle |
|---:|---|---|---|
| T1 | Wood/Copper Pickaxe | Wood Axe | Basic Sickle |
| T2 | Iron Pickaxe | Iron Axe | Iron Sickle |
| T3 | Deepforge Pickaxe | Frostwood Axe | Frost Sickle |
| T4 | Gold Pickaxe | Ember Axe | Ember Sickle |
| T5 | Runic Pickaxe | Ancient Timber Axe | Relic Sickle |
| T6 | Blackstone Pickaxe | Gloomwood Axe | Shadow Sickle |
| T7 | Diamond Pickaxe | Corrupted Heartwood Axe | Core Sickle |
| T8 | Elyndor Pickaxe | Cave Heart Axe | Meteor Sickle |

### 14.2 Tool gating

Tools bloqueiam:

```text
nodes
atalhos
paredes quebráveis
raízes grossas
pontes improvisadas
cristais de passagem
portas de ruína
áreas opcionais
caminhos principais, se houver acesso prévio Ã  ferramenta/material
```

Exemplo:

```text
CaveLevel 31:
Frozen Crystal Wall requires Pickaxe Tier 3.
Sem isso, jogador não acessa atalho/recurso raro.
```

Hardening:

```text
Tool gate não deve travar saída principal sem aviso.
Se travar progressão principal, precisa de receita/material acessível antes.
Feedback deve informar ferramenta e tier necessários.
```

---

## 15. Armas

### 15.1 Modelo RPG

Cada tier deve ter várias opções.

| Build | Armas |
|---|---|
| Strength | sword, axe, hammer, spear |
| Dexterity | dagger, bow, light sword |
| Intelligence | wand, staff, spell focus |
| Willpower | relic, tome, holy/dark focus |
| Hybrid | elemental blade, runic bow, arcane spear |

### 15.2 Exemplos por faixa

| Faixa | Armas possíveis |
|---:|---|
| 1–15 | Copper Sword, Cave Bow, Stone Hammer, Basic Wand |
| 16–30 | Iron Sword, Root Bow, Worgfang Dagger, Spore Wand |
| 31–45 | Frostblade, Deepforge Hammer, Ice Wand, Drow Silk Bow |
| 46–60 | Ember Axe, Fire Staff, Wyvern Spear, Ash Bow |
| 61–75 | Relic Spear, Arcane Wand, Runic Hammer, Prism Bow |
| 76–90 | Shadow Dagger, Gloom Bow, Blackstone Staff, Vampire Rapier |
| 91–99 | Diamond Blade, Core Staff, Dragonbone Spear, Meteor Hammer |
| 100 | Cave Heart Weapon / Elyndor Relic |

---

## 16. Armas elementais quebradas

Humanoides podem dropar armas elementais quebradas.

Exemplos:

```text
item_broken_fire_touched_axe
item_broken_frost_drow_blade
item_broken_shadow_rapier
item_broken_arcane_runebow
```

Reparo exige:

```text
Broken weapon
matching ingot/material
elemental crystal
repair station
crafting time
```

Hardening:

```text
Arma quebrada não deve ser usável como arma forte.
Pode ser vendável por pouco ou usada como material.
Reparo deve preservar affix/identidade da arma.
```

---

## 17. Drops de criaturas

| Família | Drops comuns | Drops raros |
|---|---|---|
| Slimes/Oozes | slime gel, ooze residue | ooze core, meteor ooze core |
| Bats | bat wing, fang | echo membrane |
| Wolves/Worgs | hide, fang | alpha fang |
| Spiders | silk, venom sac | spider queen gland |
| Goblins/Kobolds | scrap, crude weapon, trap parts | broken elemental weapon |
| Orcs/Gnolls | tusk, hide, war charm | clan token, blood charm |
| Drows | drow silk, poison vial | broken shadow/frost blade |
| Deep Forge Dwarfs | forge coal, broken hammer | rune metal |
| Draconics | scale, claw, fang | breath gland, drake heart |
| Undeads | bone, ectoplasm | cursed shard, phylactery shard |
| Vampirics | fang, blood dust | blood crystal |
| Constructs | rune plate, relic gear | arcane core |
| Observers/Eyes | eye lens, gaze crystal | portal fluid |
| Veyraathi | horn shard, red sigil | corruption ember |
| Blackstone Horrors | blackstone shard | corruption core |

---

## 18. Boss drops

Boss drops podem ser:

```text
KeyItem não vendável
RareMaterial vendável
ambos
```

Regra:

```text
Todo boss deve dropar pelo menos 1 ProgressionDrop.
Boss também pode dropar material raro vendável.
ProgressionDrop não pode ser vendido por acidente.
Se item tiver valor de venda, marcar explicitamente Sellable = true.
```

### 18.1 Exemplos

| Boss | Key item | Rare vendável/material |
|---|---|---|
| Meteor Ooze King | `item_boss_core_meteor_ooze` | meteor dust, slime gel |
| Goblin Bloodfang Butcher | `item_boss_bloodfang_cleaver` | goblin war charm |
| Kobold Tunnel Tyrant | `item_boss_tunnel_crown` | trap core |
| Rootbound Guardian | `item_boss_root_heart` | refined root fiber |
| Gatebreaker of the Deep Forge | `item_boss_deepforge_anvil_core` | rune metal |
| Drow Frostblade Matriarch | `item_boss_frostblade_sigil` | drow silk, frost blade shard |
| Ember Maw Wyvern | `item_boss_wyvern_ember_gland` | wyvern scale |
| Relic Sentinel of Elyndor | `item_boss_elyndor_relic_core` | rune plate |
| Hollow Lich | `item_boss_blackstone_phylactery_shard` | cursed shard |
| Blackstone Dragon | `item_boss_blackstone_dragon_heart` | dragon scale |
| The Portal-Bound Ancient | `item_relic_cave_heart` | Elyndor core |

---

## 19. Recipe unlock

Receitas podem liberar por múltiplas fontes.

| Fonte | Uso |
|---|---|
| CaveLevel reached | receitas comuns por tier |
| Boss defeated | receitas de progressão |
| Key item acquired | receitas específicas |
| NPC | receitas narrativas |
| Skill tree | receitas por build |
| Secret room | receitas raras |
| Elyndor archive | receitas arcanas |
| Loot drop | receitas de armas especiais |

Regra recomendada:

```text
Receitas comuns = CaveLevel + estação
Receitas de tier = boss derrotado
Receitas raras = drop/secret room
Receitas lendárias = Elyndor archive / level 100
```

---

# 20. Contratos sugeridos

## 20.1 EquipmentDataSO

```csharp
public class EquipmentDataSO : ScriptableObject, IIdentifiedData
{
    public string Id;
    public string DisplayName;
    public string EquipmentType;
    public string SlotType;
    public int Tier;
    public int RequiredLevel;
    public string[] RequiredBossIds;
    public string[] RequiredRecipeIds;
    public EquipmentStats Stats;
    public bool IsBroken;
    public string RepairedItemId;
    public int DurabilityMax;
    public bool Sellable;
    public int SellValue;
}
```

## 20.2 EquipmentStats

```csharp
[Serializable]
public class EquipmentStats
{
    public int Defense;
    public int MagicDefense;
    public int HeatResistance;
    public int ColdResistance;
    public int FireResistance;
    public int IceResistance;
    public int PoisonResistance;
    public int ShadowResistance;
    public int ArcaneResistance;
    public int StrengthBonus;
    public int DexterityBonus;
    public int IntelligenceBonus;
    public int WillpowerBonus;
    public int ConstitutionBonus;
    public int BreathBonus;
    public float CritChanceBonus;
    public float MoveSpeedMultiplier;
    public float StaminaCostMultiplier;
    public float CastTimeMultiplier;
    public int WeightClass;
}
```

## 20.3 EquipmentInstanceSaveData

```csharp
[Serializable]
public class EquipmentInstanceSaveData
{
    public string InstanceId;
    public string ItemId;
    public int DurabilityCurrent;
    public int DurabilityMax;
    public bool IsBroken;
    public string SlotType;
}
```

## 20.4 CraftingRecipeSO vNext

```csharp
public class CraftingRecipeSO : ScriptableObject, IIdentifiedData
{
    public string Id;
    public string DisplayName;
    public string StationType;
    public string OutputItemId;
    public int OutputAmount;
    public CraftingIngredient[] Ingredients;
    public float BaseCraftTimeSeconds;
    public int RequiredPlayerLevel;
    public int RequiredToolTier;
    public string[] RequiredBossIds;
    public string[] RequiredKeyItemIds;
    public bool IsRepairRecipe;
}
```

## 20.5 EnvironmentalGateSO

```csharp
public class EnvironmentalGateSO : ScriptableObject, IIdentifiedData
{
    public string Id;
    public int MinCaveLevel;
    public int RequiredHeatResistance;
    public int RequiredColdResistance;
    public int RequiredToolTier;
    public string RequiredToolType;
    public int DamagePerTick;
    public float TickIntervalSeconds;
    public bool BlocksProgression;
}
```

## 20.6 CraftingProgressState

```csharp
public class CraftingProgressState
{
    public string RecipeId;
    public string StationType;
    public float RequiredTimeSeconds;
    public float CurrentProgressSeconds;
    public bool IsInHostileArea;
}
```

---

# 21. Hardening geral

## H1 — Sem hard lock injusto

Se tool/equipamento bloqueia progressão principal, o jogador precisa ter acesso prévio Ã  receita e aos materiais.

## H2 — Soft warning antes do dano ambiental

Antes de dano ambiental pesado:

```text
HUD feedback
efeito visual
mensagem curta
dano leve inicial
```

## H3 — Armadura pesada não pode ser melhor em tudo

Metal armor precisa pagar custo em:

```text
movimento
stamina
cast time ou dodge futuro
```

## H4 — Cloth/Leather precisam ser viáveis

```text
Cloth = magia/mana/status
Leather = dex/crit/mobilidade
Metal = defesa/resistência/poise
```

## H5 — Boss drop protegido

Key item não vendável por padrão.

## H6 — Arma elemental quebrada não quebra balance

Drop deve exigir reparo e materiais para virar arma usável.

## H7 — Durability sem microgerenciamento excessivo

Regra 1 durability a cada 3 usos reduz desgaste excessivo. O MVP não deve exigir reparos a cada minuto de gameplay.

---

# 22. Critérios de aceite

```text
CA1: Cada faixa da cave possui materiais brutos e refinados definidos.
CA2: Cada família de inimigo possui drops comuns e raros.
CA3: Cada boss possui progression drop.
CA4: Armaduras possuem tipo, peso, bônus e penalidade.
CA5: HeatResistance e ColdResistance existem separados de Fire/Ice.
CA6: Ambientes podem exigir resistência ambiental.
CA7: Crafting usa tempo segurando botão.
CA8: Level e Dexterity reduzem tempo de crafting.
CA9: Refinamento usa estações diferentes.
CA10: Tool tier bloqueia nodes e áreas.
CA11: Armas elementais quebradas podem ser reparadas.
CA12: Key items de boss não são vendáveis por padrão.
CA13: DurabilityMax = 100 para equipáveis.
CA14: A cada 3 usos relevantes, item perde 1 durability.
CA15: Armor é peça única no MVP.
CA16: MVP possui 1 accessory slot.
CA17: Crafting em área hostil cancela ao tomar dano.
CA18: Tool gate pode bloquear caminho principal apenas com receita/material/ferramenta acessíveis antes.
```

---

# 23. Non-goals

Fora desta spec:

```text
balance final de todos os itens
UI final de crafting/inventory/equipment
durabilidade visual final
animações finais de crafting
sistema completo de peças separadas de armadura
2+ acessórios no MVP
encantamento completo
transmog/cosmetic gear
sistema econômico final
crafting multiplayer/co-op
```

---

# 24. MVP recomendado

MVP de implementação desta spec deve começar pequeno:

```text
1. Contratos de EquipmentDataSO/EquipmentStats/Durability.
2. Contratos de CraftingRecipeSO vNext com craft time.
3. Repair Bench simples.
4. Workbench + Forge.
5. Armor única com Cloth/Leather/Metal exemplos.
6. HeatResistance/ColdResistance no equipment stats.
7. EnvironmentalGateSO placeholder.
8. Durability em Weapon/Tool/Armor.
9. Broken elemental weapon repair flow com 1 exemplo.
```

Não incluir todos os tiers de uma vez se isso atrasar a base.

---

# 25. Impacto em specs anteriores

Complementa:

- `docs/FASE9E_ITEM_TAXONOMY_IDS_SPEC_v1.0.md`
- `docs/FASE9E_SAVE_SCHEMA_MIGRATION_SPEC_v1.0.md`
- `docs/FASE9E_PLAYER_LEVEL_UP_PROGRESSION_SPEC_v1.0.md`
- `docs/FASE9F_CAVE_RESOURCES_ENCOUNTERS_SPEC_v1.0.md`
- `docs/FASE9G_CAVE_BESTIARY_FACTION_LOCKS_PORTAL_ECOLOGY_SPEC_v1.0.md`
- `docs/amendments/FASE9G_AMENDMENT_ENEMY_COMBAT_ROLES_AI_STATUS_v1.1.md`

Não altera specs antigas destrutivamente.


## Itens que devem virar implementação

- [ ] Refinar em spec executável antes de código, quando aplicável.
- [ ] Validar dependências contra specs implementadas atuais.

## Fora de escopo / cuidado

- Não tratar este refinement como autorização automática de implementação.
- Não sobrescrever specs implementadas sem amendment/correction explícito.


