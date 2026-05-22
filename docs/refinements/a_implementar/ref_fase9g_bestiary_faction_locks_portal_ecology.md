# REF FUTURO — FASE9G bestiary faction locks portal ecology

> Origem histórica: $Source
> Status: Refinamento futuro preservado
> Spec futura relacionada: $Spec

---

## Decisões preservadas

Conteúdo histórico preservado abaixo para evitar perda operacional de decisões, escopo e pendências.

---

# FASE 9G — Cave Bestiary, Faction Locks & Portal Ecology Spec v1.0

> **Status:** spec aprovada para orientar próximas waves de design/implementação.  
> **Feature:** `FASE9G_CAVE_BESTIARY_FACTION_LOCKS_PORTAL_ECOLOGY`  
> **Base:** FASE9F Cave/Resources/Encounters + Guia de Raças de Vaalara + GDD v2.6 + Spec Evolution Policy.  
> **Objetivo:** definir bestiário, ecologia procedural, locks de facção, variações raciais, criaturas bestiais, undeads, dracônicos, aberrações, bosses e minibosses procedurais da caverna.

---

## 1. Princípios

A cave não é uma lista aleatória de inimigos. Ela é uma rede subterrânea de Vaalara afetada por:

- fauna local;
- raças e facções de Vaalara;
- exilados de Daromir;
- ruínas e arcos de Elyndor;
- Pedras do Meteoro;
- influência das luas;
- bestas, monstros, undeads, dracônicos, aberrações e constructos antigos.

Cada nível ou subfaixa deve ter identidade coerente.

---

## 2. Regra central de procedural faction lock

A geração procedural pode variar layout, recursos, inimigos, minibosses e bosses, mas precisa travar uma ecologia coerente.

Errado:

```text
CaveLevel 35 gera, ao mesmo tempo:
- Anões da Forja Sem Sol
- Drows
- Elfos Silvestres
- Sahuagin-like
- Goblins
```

Certo:

```text
CaveLevel 35
Biome = FrostDeep
EcologyArc = DeepForgeWar
FactionLock = DeepForgeExiles

Pode gerar:
- Sunless Dwarf Miner
- Deep Hammer Guard
- Black-Anvil Smith
- Runemarked Shieldbearer
- Ice Spider
- Cave Bear
- Frozen Skeleton como ambient/rare

Não pode gerar como spawn comum:
- Drow Scout
- Elven Rootbound Hunter
- Sahuagin-like
- Goblin Trapper
```

---

## 3. Regra de raças de Vaalara

Raças de Vaalara não são inimigas por natureza.

Inimigos humanoides devem ser sempre uma destas categorias:

```text
facção hostil
clã hostil
exilado
cultista
saqueador
mercenário
explorador corrompido
guardião territorial
expedição rival
sobrevivente enlouquecido pela cave
servo de boss
```

Exemplo:

```text
Goblin não é inimigo porque é goblin.
Goblin Bloodfang Stabber é inimigo porque pertence a uma facção saqueadora hostil.
```

---

## 4. Uso de criaturas D&D-like / fantasia clássica

Criaturas clássicas de fantasia podem existir como inspiração e arquétipo.

Regras:

- Não copiar statblocks oficiais.
- Não copiar lore oficial longa.
- Não depender de nomes proprietários quando houver risco de identidade forte.
- Preferir nomes próprios de Vaalara para criaturas icônicas demais.
- Usar nomes genéricos/fantasia comuns quando forem adequados: goblin, orc, drow, kobold, wolf, spider, vampire, lich, wyvern, drake.

Renomes recomendados:

| Arquétipo | Nome de Vaalara |
|---|---|
| Beholder-like | Observador de Elyndor / Olho Partido / Tirano Ocular Partido |
| Duergar-like | Anão da Forja Sem Sol / Anão Profundo Exilado |
| Sahuagin-like | Guelra Negra / Filho Afogado de Velharra |
| Myconid-like | Micélio Errante / Povo do Micélio |
| Mephit-like | Diabrete de Brasa / Diabrete de Geada |
| Death Knight-like | Cavaleiro do Portão Morto |
| Mind Horror-like | Horror Mental do Arco |

---

## 5. Conceitos funcionais

### 5.1 Encounter Ecology

Ecologia geral do nível ou subfaixa.

Exemplos:

```text
LocalFauna
MeteorTouchedThreshold
GoblinNest
OrcWarband
DeepForgeExiles
DrowHouseExpedition
UndergroundForestBeasts
ElyndorRuinsConstructs
BlackstoneUndead
DraconicCore
AbyssalWaterIntrusion
VeyraathCult
NymirianPilgrimage
```

### 5.2 Faction Lock

Facção dominante do nível ou subfaixa.

```text
FactionLock = DeepForgeExiles
```

Enquanto ativo, só pode usar:

- enemy families permitidas;
- ambient fauna compatível;
- rare intruders permitidos;
- minibosses/bosses compatíveis;
- conflict encounters explicitamente habilitados.

### 5.3 Enemy Family

Família coerente de inimigos.

```text
EnemyFamily = DeepForge
- Sunless Dwarf Miner
- Deep Hammer Guard
- Black-Anvil Smith
- Runemarked Shieldbearer
```

### 5.4 Ecology Arc

Arco narrativo/procedural de uma subfaixa de 3–5 níveis.

Exemplo:

```text
CaveLevels 31–35 = DeepForgeExiles
CaveLevels 36–40 = FrozenDeadCompany
CaveLevels 41–44 = DeepForgeExiles
CaveLevel 45 = BossGate: DeepForge
```

### 5.5 Conflict Encounter

Exceção controlada em que duas facções incompatíveis aparecem na mesma sala.

Fora do MVP inicial.

### 5.6 Rare Intruder

Criatura rara vazada por portal, limitada por bioma.

Exemplo:

```text
Guelra Negra em Fire Cave só pode aparecer se a sala for PortalLeak_AbyssalWater.
```

---

## 6. Estrutura de dados alvo

### 6.1 CaveEncounterEcologySO

```csharp
public class CaveEncounterEcologySO : ScriptableObject, IIdentifiedData
{
    public string Id;
    public string DisplayName;
    public int MinCaveLevel;
    public int MaxCaveLevel;
    public string BiomeId;
    public string[] AllowedFactionLockIds;
    public string[] AllowedAmbientEnemyFamilyIds;
    public string[] RareIntruderFamilyIds;
    public int RareIntruderChancePercent;
    public int ConflictEncounterChancePercent;
}
```

### 6.2 CaveFactionLockSO

```csharp
public class CaveFactionLockSO : ScriptableObject, IIdentifiedData
{
    public string Id;
    public string DisplayName;
    public string LoreSummary;
    public string[] AllowedEnemyFamilyIds;
    public string[] AllowedCreatureRoleIds;
    public string[] AllowedMinibossIds;
    public string[] AllowedBossIds;
    public string[] IncompatibleFactionLockIds;
    public bool AllowsRareIntruders;
    public bool AllowsConflictEncounters;
}
```

### 6.3 EnemyFamilySO

```csharp
public class EnemyFamilySO : ScriptableObject, IIdentifiedData
{
    public string Id;
    public string DisplayName;
    public string RaceOrCreatureType;
    public string[] EnemyIds;
    public string[] CompatibleBiomeIds;
    public string[] CompatibleFactionLockIds;
}
```

### 6.4 BossCandidateSO

```csharp
public class BossCandidateSO : ScriptableObject, IIdentifiedData
{
    public string Id;
    public string DisplayName;
    public int CaveLevel;
    public string BossTier; // MiniBoss, Boss, BossGate, FinalBoss
    public string RequiredBiomeId;
    public string[] CompatibleFactionLockIds;
    public string EnemyId;
    public string LootTableId;
    public bool IsFixedLoreBoss;
}
```

### 6.5 CaveLevelEcologySaveData

```csharp
[Serializable]
public class CaveLevelEcologySaveData
{
    public int CaveLevel;
    public string BiomeId;
    public string EncounterEcologyId;
    public string FactionLockId;
    public List<string> EnemyFamilyIds = new();
    public bool HasConflictEncounter;
}
```

### 6.6 CaveBossChoiceSaveData

Bosses e minibosses procedurais precisam persistir no save para o mesmo save não trocar gate principal toda hora.

```csharp
[Serializable]
public class CaveBossChoiceSaveData
{
    public int CaveLevel;
    public string BossCandidateId;
    public string FactionLockId;
    public bool WasDefeated;
}
```

Em `CaveSaveData` futuro:

```csharp
public List<CaveLevelEcologySaveData> GeneratedLevelEcologies = new();
public List<CaveBossChoiceSaveData> GeneratedBossChoices = new();
```

Regras:

- Ecology normal pode ser por `CaveRunSeed`.
- Boss checkpoint deve ser escolhido deterministicamente por `CaveWorldSeed + CaveLevel` e salvo.
- Miniboss pode ser por `CaveRunSeed + CaveLevel`, mas deve persistir enquanto a run existir.
- Ao trocar `CaveRunSeed`, minibosses podem mudar.
- Bosses de checkpoint não devem mudar ao trocar run, salvo new game ou decisão explícita futura.

---

## 7. Ordem de geração procedural

```text
1. Determinar CaveLevel.
2. Determinar Biome pelo range.
3. Determinar EcologyArc compatível com Biome + CaveLevel.
4. Determinar FactionLock principal da subfaixa.
5. Validar incompatibilidades.
6. Selecionar EnemyFamilies compatíveis.
7. Selecionar AmbientFauna compatível.
8. Aplicar RareIntruder, se permitido.
9. Aplicar ConflictEncounter, se permitido e habilitado.
10. Selecionar miniboss/boss candidate entre 3 opções compatíveis.
11. Gerar spawn points.
12. Gerar inimigos.
13. Persistir CaveLevelEcologySaveData e CaveBossChoiceSaveData quando aplicável.
```

---

## 8. Granularidade do lock

Decisão aprovada:

```text
FactionLock padrão por subfaixa de 3–5 níveis.
Cada CaveLevel herda o FactionLock dominante da subfaixa.
Cada CaveLevel ainda pode ter variações internas de ambient fauna e rare intruder.
```

Exemplo:

```text
31–35 = DeepForgeExiles
36–40 = FrozenDeadCompany
41–44 = DeepForgeExiles
45 = BossGate DeepForge
```

---

# 9. Bestiário por faixa

## 9.1 Níveis 1–10 — Local Caves

Função: tutorial de combate, coleta, movimentação, loot e risco básico.

Ecologias:

```text
LocalFauna
SmallPredators
SimpleOozes
SpiderBrood
BatColony
```

FactionLock:

```text
None
```

Criaturas comuns:

```text
Slime
Stone Slime
Cave Bat
Giant Bat
Cave Rat
Dire Rat
Wolf
Cave Wolf
Spider
Venom Spider
Cave Beetle
Tunnel Worm
Small Ooze
Mud Ooze
Rock Crab
Mole Beast
```

Criaturas bestiais/monstruosas adicionais:

```text
Brood Spider
Alpha Cave Wolf
Armored Beetle
Burrower Larva
Crystal Tick
Blind Cave Hound
Spore Toad
Razor Mole
```

Miniboss level 10 — três opções procedurais:

| Opção | Tipo | Observação |
|---|---|---|
| Brood Spider Matriarch | beast/spider | ensina veneno e adds pequenos |
| Alpha Cave Wolf | beast/brute | ensina charge e pressão melee |
| Giant Bat Broodmother | flying/beast | ensina inimigo rápido e evasivo |

---

## 9.2 Níveis 11–15 — Meteor-Touched Threshold

Função: primeira anomalia meteórica e primeiro contato com humanoides/facções leves.

Ecologias:

```text
MeteorTouchedFauna
GoblinScavengers
KoboldScouts
RestlessMiners
SpiderInfestation
```

FactionLocks possíveis:

```text
GoblinBloodfangRaiders
GoblinThousandBroodNest
KoboldTunnelScouts
RestlessDeadMiners
MeteorTouchedFauna
```

Criaturas comuns:

```text
Meteor-Touched Slime
Redstone Bat
Giant Spider
Skeleton Miner
Restless Miner
Goblin Bloodfang Stabber
Goblin Swarm Runner
Uru'dakh Swarm Goblin
Kobold Tunnel Scout
Kobold Pebble-Slinger
Kobold Trap-Keeper
```

Criaturas bestiais/monstruosas adicionais:

```text
Meteor Ooze
Redstone Tick
Cave Centipede
Bone Spider
Stoneback Boar
Rift Rat
Shard Beetle
```

Boss level 15 — três opções procedurais:

| Opção | Tipo | Faction/Ecology |
|---|---|---|
| Meteor Ooze King | ooze/boss | MeteorTouchedFauna |
| Goblin Bloodfang Butcher | humanoid/boss | GoblinBloodfangRaiders |
| Kobold Tunnel Tyrant | humanoid/boss | KoboldTunnelScouts |

Regra: derrotar o boss 15 libera avanço para 16. Alcançar 15 pode liberar checkpoint 15.

---

## 9.3 Níveis 16–30 — Underground Forest

Função: floresta subterrânea, facções de coleta, bestas maiores, fungos e raízes.

Ecologias:

```text
UndergroundForestBeasts
GoblinNest
OrcForagers
ElvenGroveRemnants
FungalBloom
BeastDen
```

FactionLocks possíveis:

```text
GoblinTrapperCamp
OrcFuryForagers
ElvenRootboundHunters
FungalColony
BeastDen
```

Criaturas por lock:

### GoblinTrapperCamp

```text
Goblin Trapper
Bone-Scrap Bomber
Zhak'thul Free-Scream Runner
Zhak'thul Noise-Shaman
Goblin Net-Thrower
Goblin Mushroom Thief
Worg
Giant Spider
```

### OrcFuryForagers

```text
Orc Fury Grunt
Blood-Tusk Charger
Orc Hunter
Orc Bone-Axe Guard
Orc Cave Howler
Worg
Owlbear
Dire Wolf
```

### ElvenRootboundHunters

```text
Rootbound Hunter
Thornblade Scout
Lost Forest Archer
Corrupted Grove Warden
Animated Vine
Root-Tethered Wolf
Briar Sprite
```

### FungalColony

```text
Mycelium Walker
Fungal Crawler
Spore Bat
Root Horror
Poison Slime
Mushroom Brute
Spore Cloudling
```

### BeastDen

```text
Owlbear
Worg
Giant Spider
Cave Bear
Dire Wolf
Giant Centipede
Horned Cave Boar
Mossback Brute
```

Miniboss level 20 — três opções:

| Opção | Tipo | Faction/Ecology |
|---|---|---|
| Owlbear Matriarch | beast/miniboss | BeastDen |
| Goblin Trap-King | humanoid/miniboss | GoblinTrapperCamp |
| Root-Tethered Worg | beast/corrupted | UndergroundForestBeasts |

Miniboss level 25 — três opções:

| Opção | Tipo | Faction/Ecology |
|---|---|---|
| Orc Blood-Tusk Captain | humanoid/miniboss | OrcFuryForagers |
| Sporeheart Brute | fungus/miniboss | FungalColony |
| Thornblade Grove Warden | elf/fey/miniboss | ElvenRootboundHunters |

Boss level 30 — três opções:

| Opção | Tipo | Faction/Ecology |
|---|---|---|
| Rootbound Guardian | plant/beast/boss | UndergroundForestBeasts |
| Fungal Brood Sovereign | fungus/boss | FungalColony |
| Orc Root-Reaver Warchief | humanoid/boss | OrcFuryForagers |

---

## 9.4 Níveis 31–45 — Frost / Deep Subterranean

Função: gelo, forjas profundas, drows, anões profundos, mortos congelados e cristais.

Regra-chave: DeepForgeExiles e DrowFrostExpedition são incompatíveis no mesmo nível, salvo ConflictRoom futuro.

Ecologias:

```text
DeepForgeExiles
DrowHouseExpedition
FrozenUndead
IceBeastDen
CrystalCavern
```

FactionLocks possíveis:

```text
DeepForgeExiles
DrowFrostExpedition
FrozenDeadCompany
CrystalBeastNest
```

### DeepForgeExiles

```text
Sunless Dwarf Miner
Deep Hammer Guard
Black-Anvil Smith
Runemarked Shieldbearer
Forge-Bellower Brute
Coal-Eyed Axeguard
Ice Spider
Cave Bear
```

### DrowFrostExpedition

```text
Drow Scout
Drow Hand-Crossbow Hunter
Drow Spellblade
Drow Frostblade
Drow Webcaller
Drow Shadow Duelist
Ice Spider
Shadow Familiar
```

### FrozenDeadCompany

```text
Frozen Skeleton
Frost Wight
Zombie Miner
Skeleton Knight
Ice Ooze
Frostbound Ghoul
Cold Lantern Wraith
```

### CrystalBeastNest

```text
Crystal Crawler
Ice Spider
Frost Wolf
Crystal Bat
Ice Ooze
Shardback Lizard
Glasshorn Beetle
Snowblind Cave Bear
```

Miniboss level 35 — três opções:

| Opção | Tipo | Faction/Ecology |
|---|---|---|
| Black-Anvil Smith | deep dwarf/miniboss | DeepForgeExiles |
| Drow Frostblade Captain | drow/miniboss | DrowFrostExpedition |
| Ice Spider Queen | beast/miniboss | CrystalBeastNest |

Miniboss level 40 — três opções:

| Opção | Tipo | Faction/Ecology |
|---|---|---|
| Deep Hammer Overseer | deep dwarf/miniboss | DeepForgeExiles |
| Frost Wight Commander | undead/miniboss | FrozenDeadCompany |
| Crystal Crawler Prime | beast/crystal/miniboss | CrystalBeastNest |

Boss level 45 — três opções:

| Opção | Tipo | Faction/Ecology |
|---|---|---|
| Gatebreaker of the Deep Forge | deep dwarf/boss | DeepForgeExiles |
| Drow Frostblade Matriarch | drow/boss | DrowFrostExpedition |
| Frost Wight Commander | undead/boss | FrozenDeadCompany |

---

## 9.5 Níveis 46–60 — Fire / Warbands / Draconic

Função: fogo, guerra, gnolls, orcs, dracônicos, cultos de Veyraath e drakes.

Ecologias:

```text
OrcWarband
GnollPack
DraconicCult
FireBeastNest
VeyraathRuinblood
```

FactionLocks possíveis:

```text
OrcFlameDanceWarband
GnollBonechewerPack
RuinbloodDraconateCult
FireDrakeNest
VeyraathiFlameCult
```

### OrcFlameDanceWarband

```text
Flame-Dance Raider
Ember Axe Orc
Senya War-Dancer
Kaand-Marked Berserker
Ash Hound
Orc Fire-Drummer
Orc Spear Dancer
```

### GnollBonechewerPack

```text
Gnoll Bonechewer
Gnoll Pack Hunter
Gnoll Bloodhowler
Gnoll Fire-Eater
Hyena Beast
Ash Hound
Bone-Mask Gnoll
```

### RuinbloodDraconateCult

```text
Ruinblood Acolyte
Acid-Breath Marauder
Kobold Dragon-Acolyte
Kobold Scale-Priest
Drake Whelp
Fire Drake
Ashen Scale Guard
```

### FireDrakeNest

```text
Drake Whelp
Fire Drake
Magma Ooze
Ember Bat
Living Ember
Cinder Lizard
Basalt Claw Beast
Flameback Beetle
```

### VeyraathiFlameCult

```text
Veyraathi Cultist
Infernal Knife
Abyssal Flamecaller
Magma Ooze
Diabrete de Brasa
Red Horn Fanatic
Cinder Hexer
```

Miniboss level 50 — três opções:

| Opção | Tipo | Faction/Ecology |
|---|---|---|
| Gnoll Packlord | gnoll/miniboss | GnollBonechewerPack |
| Orc Flamecaller | orc/miniboss | OrcFlameDanceWarband |
| Ruinblood Scale-Priest | draconate/miniboss | RuinbloodDraconateCult |

Miniboss level 55 — três opções:

| Opção | Tipo | Faction/Ecology |
|---|---|---|
| Fire Drake | drake/miniboss | FireDrakeNest |
| Veyraathi Red Cinder | tiefling/miniboss | VeyraathiFlameCult |
| Kaand-Marked Berserker Chief | orc/miniboss | OrcFlameDanceWarband |

Boss level 60 — três opções:

| Opção | Tipo | Faction/Ecology |
|---|---|---|
| Ember Maw Wyvern | wyvern/boss | FireDrakeNest |
| Gnoll Ash-Pack Prophet | gnoll/boss | GnollBonechewerPack |
| Ruinblood Tyrant | draconate/boss | RuinbloodDraconateCult |

---

## 9.6 Níveis 61–75 — Elyndor Ruins

Função: ruínas, arcos, constructos, Ninrorin, gnomos das gemas, drows portalistas, aberrações oculares.

Ecologias:

```text
ElyndorConstructs
NinrorinPlanarScholars
GnomeCrystalSeekers
DrowPortalists
ArcaneAberrations
```

FactionLocks possíveis:

```text
ElyndorRunicDefense
NinrorinGateScholars
GemGnomePrismSeekers
DrowPortalistHouse
BrokenEyeColony
```

### ElyndorRunicDefense

```text
Runic Sentinel
Animated Armor
Relic Drone
Stone Golem-like Guardian
Elyndor Gatekeeper
Broken Rune Turret
Archive Shield-Form
```

### NinrorinGateScholars

```text
Ninrorin Gate-Touched Mage
Planar Scholar
Arcane Duelist
Broken Planewalker
Arcane Wisp
Grey Memory Keeper
Portal Scribe
```

### GemGnomePrismSeekers

```text
Gem Hermit
Crystal Channeler
Radiant Shardguard
Gem-Crazed Prospector
Clockwork Handler
Prism Lens Adept
Shard Tinkerer
```

### DrowPortalistHouse

```text
Drow Portalist
Drow House Assassin
Drow Spellblade
Shadow Familiar
Portal Spider
Drow Rift-Seer
Drow Gate Duelist
```

### BrokenEyeColony

```text
Floating Eye
Eye of the Broken Gate
Portal Maw
Tyrant Eye Fragment
Arcane Wisp
Lesser Observador de Elyndor
Mind-Glare Orb
```

Miniboss level 65 — três opções:

| Opção | Tipo | Faction/Ecology |
|---|---|---|
| Eye of the Broken Gate | aberration/miniboss | BrokenEyeColony |
| Ninrorin Broken Planewalker | elf/caster/miniboss | NinrorinGateScholars |
| Gem-Crazed Prospector | gnome/miniboss | GemGnomePrismSeekers |

Miniboss level 70 — três opções:

| Opção | Tipo | Faction/Ecology |
|---|---|---|
| Runic Golem | construct/miniboss | ElyndorRunicDefense |
| Drow Portalist Captain | drow/miniboss | DrowPortalistHouse |
| Prism Shardguard Prime | gnome/construct/miniboss | GemGnomePrismSeekers |

Boss level 75 — três opções:

| Opção | Tipo | Faction/Ecology |
|---|---|---|
| Relic Sentinel of Elyndor | construct/boss | ElyndorRunicDefense |
| Observador do Arco Partido | aberration/boss | BrokenEyeColony |
| Ninrorin Gate-Sealer | elf/caster/boss | NinrorinGateScholars |

---

## 9.7 Níveis 76–90 — Shadow Abyss

Função: Pedra Negra, undeads, vampiros, orcs de Nyx, cultos de Veyraath, sombras e horrores.

Ecologias:

```text
BlackstoneUndead
VampiricCourt
NyxboundOrcs
VeyraathCult
ShadowAberrations
```

FactionLocks possíveis:

```text
HollowDeadCompany
BloodboundCourt
NyxOrcProphets
VeyraathiRedHeralds
BlackstoneHorrors
```

### HollowDeadCompany

```text
Skeleton Knight
Zombie Miner
Ghoul
Wight
Wraith-like Shadow
Lich Fragment
Cold Lantern Wraith
Gravebound Archer
```

### BloodboundCourt

```text
Vampire Spawn
Bloodbound Noble
Blood Rapier Duelist
Bat Swarm
Wight Servant
Crimson Thrall
Veinless Courtier
```

### NyxOrcProphets

```text
Night-Tusk Scout
Shadow Shaman
Moonless Spear
Nyx-Bound Cave Prophet
Wraith-like Shadow
Orc Black-Moon Hexer
Silent-Tusk Executioner
```

### VeyraathiRedHeralds

```text
Veyraathi Cultist
Abyssal Flamecaller
Veyraath’s Red Herald
Infernal Knife
Blackstone Horror
Horned Red Invoker
Ash-Sigil Torturer
```

### BlackstoneHorrors

```text
Void Ooze
Blackstone Horror
Portal-Torn Beast
Shadow Slime
Broken Eye Fragment
No-Light Maw
Crawling Rift Flesh
```

Miniboss level 80 — três opções:

| Opção | Tipo | Faction/Ecology |
|---|---|---|
| Vampire Spawn Lord | vampire/miniboss | BloodboundCourt |
| Nyx-Bound Cave Prophet | orc/caster/miniboss | NyxOrcProphets |
| Blackstone Horror Prime | aberration/miniboss | BlackstoneHorrors |

Miniboss level 85 — três opções:

| Opção | Tipo | Faction/Ecology |
|---|---|---|
| Wight Commander | undead/miniboss | HollowDeadCompany |
| Veyraath’s Red Herald | tiefling/cult/miniboss | VeyraathiRedHeralds |
| Moonless Spear Champion | orc/miniboss | NyxOrcProphets |

Boss level 90 — três opções:

| Opção | Tipo | Faction/Ecology |
|---|---|---|
| Hollow Lich of the Black Stone | undead/boss | BlackstoneUndead |
| Bloodbound Court Patriarch/Matriarch | vampire/boss | VampiricCourt |
| Prophet of the Moonless Gate | orc/nyx/boss | NyxOrcProphets |

---

## 9.8 Níveis 91–99 — Corrupted Core

Função: endgame, dracônicos, constructos corrompidos, lich fragments, observadores, cultos do núcleo.

Ecologias:

```text
BlackstoneDraconicCore
BrokenElyndorDefense
LichRemnants
EyeTyrantColony
GateboundElites
VeyraathCoreCult
```

FactionLocks possíveis:

```text
BlackstoneDrakes
FallenKanthorJudges
LichFragments
BrokenEyeTyrants
VeyraathCoreCult
```

### BlackstoneDrakes

```text
Blackstone Drake
Crystal Drake
Drake Whelp
Young Dragon-like Boss
Meteor Elemental
Obsidian Wing
Corefire Drake
```

### FallenKanthorJudges

```text
Fallen Judge of Kanthor
Ashen Judge
Thunder-Sentence Guard
Grey Scale Inquisitor
Broken Elyndor Golem
Oath-Cracked Draconate
Lawless Arbiter
```

### LichFragments

```text
Lich Fragment
Cavaleiro do Portão Morto
Wight Commander
Spectral Scholar
Blackstone Horror
Bone Rune Magister
Skull Lantern Adept
```

### BrokenEyeTyrants

```text
Broken Eye Tyrant
Eye of the Broken Gate
Tyrant Eye Fragment
Portal Maw
Mind Horror
Observador de Elyndor
Arcane Gaze Horror
```

### VeyraathCoreCult

```text
Veyraath’s Red Herald
Ruinblood Tyrant
Veyraathi Cultist
Veyraath-Touched Scale
Core Ooze
Corrupted Scale Herald
Red Core Invoker
```

Miniboss level 94 — três opções:

| Opção | Tipo | Faction/Ecology |
|---|---|---|
| Blackstone Drake | drake/miniboss | BlackstoneDrakes |
| Fallen Judge of Kanthor | draconate/miniboss | FallenKanthorJudges |
| Lich Fragment Prime | undead/caster/miniboss | LichFragments |

Miniboss level 97 — três opções:

| Opção | Tipo | Faction/Ecology |
|---|---|---|
| Broken Eye Tyrant | aberration/miniboss | BrokenEyeTyrants |
| Veyraath’s Red Herald | tiefling/cult/miniboss | VeyraathCoreCult |
| Broken Elyndor Golem | construct/miniboss | BrokenElyndorDefense |

Boss gate level 99 — três opções:

| Opção | Tipo | Faction/Ecology |
|---|---|---|
| Three Gatebound Elites | boss trio | GateboundElites |
| Blackstone Drake Sovereign | draconic/boss | BlackstoneDrakes |
| Council of Broken Arches | mixed/boss | BrokenElyndorDefense |

Se usar Three Gatebound Elites, composição recomendada:

```text
1. Fallen Judge of Kanthor
2. Veyraath’s Red Herald
3. Broken Eye Tyrant
```

---

## 9.9 Level 100 — Central Arch of Elyndor

Função: milestone narrativo, Arco Central, conexão com Pedras do Meteoro.

Ecology:

```text
CentralElyndorArch
```

FactionLock:

```text
PortalBoundAncient
```

Final boss level 100 — três opções procedurais por save:

| Opção | Tipo | Tema |
|---|---|---|
| The Portal-Bound Ancient | entity/final boss | criatura presa ao Arco Central |
| Blackstone Dragon of the Central Arch | dragon/final boss | dragão corrompido pelo núcleo |
| Meteor Lich of Elyndor | undead/arcane/final boss | lich formado por Pedra Negra + Vermelha |

Regra:

```text
Level 100 não usa geração comum de facção.
A arena pode ser procedural controlada.
O boss final é escolhido por CaveWorldSeed e persistido no save.
```

---

# 10. Matriz de incompatibilidade inicial

| Faction Lock | Incompatível por padrão |
|---|---|
| DeepForgeExiles | DrowFrostExpedition, ElvenRootboundHunters, NymirianPilgrimage |
| DrowFrostExpedition | DeepForgeExiles, ElvenRootboundHunters, GemGnomePrismSeekers |
| ElvenRootboundHunters | DrowFrostExpedition, DeepForgeExiles, VeyraathiFlameCult |
| GoblinTrapperCamp | NymirianPilgrimage, ElyndorRunicDefense |
| OrcFlameDanceWarband | VeyraathiFlameCult, NymirianPilgrimage |
| GnollBonechewerPack | NymirianPilgrimage, GemGnomePrismSeekers |
| RuinbloodDraconateCult | FallenKanthorJudges |
| VeyraathiFlameCult | ElvenRootboundHunters, NymirianPilgrimage |
| NinrorinGateScholars | DrowPortalistHouse, BlackstoneHorrors |
| GemGnomePrismSeekers | DrowPortalistHouse, GnollBonechewerPack |
| DrowPortalistHouse | NinrorinGateScholars, GemGnomePrismSeekers |
| BloodboundCourt | NyxOrcProphets, NymirianPilgrimage |
| VeyraathiRedHeralds | BloodboundCourt, NymirianPilgrimage |
| FallenKanthorJudges | VeyraathCoreCult, RuinbloodDraconateCult |
| BrokenEyeTyrants | NymirianPilgrimage |

---

# 11. Exceções controladas

## 11.1 Ambient Fauna

Pode aparecer com várias facções se fizer sentido.

Exemplos:

```text
Bat
Spider
Cave Rat
Ooze
Frost Bat
Ember Bat
Cave Bear
Crystal Crawler
```

## 11.2 Rare Intruder

Criatura rara que vazou por portal.

Regras:

- chance baixa;
- limitada por bioma;
- nunca deve dominar o nível;
- deve aparecer no debug como `RareIntruder`.

## 11.3 Conflict Room

Fora do MVP.

Quando implementado, deve permitir duas facções incompatíveis apenas em sala marcada:

```text
EncounterType = ConflictRoom
PrimaryFaction = DeepForgeExiles
SecondaryFaction = DrowFrostExpedition
```

## 11.4 Boss Override

Boss pode quebrar lock se for boss fixo de lore.

---

# 12. Relação com luas

As luas modificam pesos, não sobrescrevem locks por padrão.

## 12.1 Senya

Aumenta peso de:

```text
OrcFlameDanceWarband
VeyraathiFlameCult
FireDrakeNest
Senya-Blooded modifiers
Chaos enemies
```

## 12.2 Nyx

Aumenta peso de:

```text
DrowFrostExpedition
BlackstoneUndead
VampiricCourt
NyxOrcProphets
Shadow modifiers
```

## 12.3 Alihana

Aumenta peso de:

```text
Veil Illusionists
NinrorinGateScholars
secret rooms
false walls
prophetic rare loot
```

---

# 13. Relação com Vaalara

A spec deve respeitar o Guia de Raças de Vaalara:

- Anya ligada aos Nymirianos;
- Thoren/Kanthor ligados aos Anões;
- Kanthor/Senya ligados aos Gnomos;
- múltiplas deusas ligadas aos Elfos;
- Veyraath ligado aos Tieflings;
- Kaand/Telisandra/Nyx/Senya ligados a Orcs e Goblins;
- Daromir como destino de drows, orcs e goblins exilados;
- Aelythar como ilha dos Nymirianos;
- Alanathus como presença dos Ninrorin e gnomos místicos/gemas.

Regras narrativas:

```text
Drow locks puxam Daromir/Khaz Baruk.
DeepForge locks puxam Khaz Baruk e exílios subterrâneos.
Ninrorin locks puxam Alanathus, memória planar e Elyndor.
Nymirian locks puxam Aelythar, Anya e Pedra Azul.
Goblin/Orc locks puxam Daromir, Kaand, Telisandra, Senya e Nyx.
```

---

# 14. MVP vs futuro

## MVP recomendável

```text
- Criar estrutura de dados de Ecology/FactionLock/EnemyFamily/BossCandidate.
- Criar dados placeholder até level 100.
- Implementar jogável só 1–15 ou 1–30.
- Validar que FactionLock impede mistura incoerente.
- Expor FactionLock no DebugHud.
- Selecionar miniboss/boss entre 3 candidatos compatíveis.
```

## Futuro

```text
- ConflictRoom.
- RareIntruder com salas especiais.
- IA específica por facção.
- Boss movesets únicos.
- Diálogos/quests de facção.
- Relação diplomática com facções.
- Salas lore de Elyndor.
- Boss final completo.
```

---

# 15. Critérios de aceite

## CA1 — Cada nível tem identidade

Cada CaveLevel gerado deve ter:

```text
BiomeId
EncounterEcologyId
FactionLockId
EnemyFamilyIds
BossCandidate quando aplicável
```

## CA2 — Lock impede mistura incoerente

Se `FactionLock = DeepForgeExiles`, não gerar `DrowFrostExpedition` como spawn comum.

## CA3 — Exceções explícitas

Misturas incompatíveis só podem ocorrer se forem:

```text
AmbientFauna
RareIntruder
BossOverride
ConflictEncounter
```

## CA4 — Lock por subfaixa

A geração deve poder travar subfaixas de 3–5 níveis com o mesmo arco/facção dominante.

## CA5 — Miniboss procedural

Cada ponto de miniboss deve ter três opções compatíveis com bioma/ecologia/faction lock.

## CA6 — Boss procedural

Cada boss checkpoint deve ter três opções compatíveis com bioma/ecologia/faction lock.

## CA7 — Boss persistente por save

Boss checkpoint escolhido deve persistir no save e não mudar apenas porque a CaveRunSeed mudou.

## CA8 — Miniboss persistente por run

Miniboss pode mudar quando a CaveRunSeed muda, mas deve ficar estável dentro da mesma run.

## CA9 — Raças de Vaalara respeitadas

Humanoides inimigos são facções/exilados/cultistas/corrompidos/guardiões, nunca raças malignas por natureza.

## CA10 — Debug

DebugHud deve poder mostrar:

```text
CaveLevel
BiomeId
EncounterEcologyId
FactionLockId
EnemyFamilyIds
BossCandidateId
```

---

# 16. Non-goals

Fora desta spec:

```text
statblocks finais
sprites finais
balance numérico definitivo
IA avançada por facção
quests completas de facção
diálogos completos
arte final de bosses
sistema diplomático com facções
captura/recrutamento de inimigos
movesets finais de todos os bosses
```

---

# 17. Decisões aprovadas nesta spec

```text
D1: Geração procedural deve lockar FactionLock por subfaixa de 3–5 níveis.
D2: Cada CaveLevel tem um FactionLock principal.
D3: Inimigos incompatíveis não aparecem no mesmo nível salvo exceções explícitas.
D4: ConflictEncounter fica fora do MVP.
D5: RareIntruder entra com chance baixa e limitado por bioma.
D6: Boss e miniboss têm 3 opções procedurais por marco.
D7: Boss checkpoint persiste por save.
D8: Miniboss persiste por run.
D9: Nymirianos e Halflings são raros como inimigos comuns.
D10: Beholder-like vira Observador/Olho de Elyndor.
D11: Duergar-like vira Anão da Forja Sem Sol / Anão Profundo Exilado.
D12: Drakes/wyverns antes do 90; dragão verdadeiro só late game/boss.
D13: Level 100 tem três possíveis final bosses por save.
D14: Luas modificam pesos, não quebram lock.
```

---

# 18. Arquivos formais relacionados

- `docs/FASE9F_CAVE_RESOURCES_ENCOUNTERS_SPEC_v1.0.md`
- `specs/FASE9F_CAVE_RESOURCES_ENCOUNTERS/spec.md`
- `docs/SPEC_EVOLUTION_POLICY_v1.0.md`
- `docs/FASE9G_CAVE_BESTIARY_FACTION_LOCKS_PORTAL_ECOLOGY_SPEC_v1.0.md`
- `specs/FASE9G_CAVE_BESTIARY_FACTION_LOCKS_PORTAL_ECOLOGY/spec.md`


## Itens que devem virar implementação

- [ ] Refinar em spec executável antes de código, quando aplicável.
- [ ] Validar dependências contra specs implementadas atuais.

## Fora de escopo / cuidado

- Não tratar este refinement como autorização automática de implementação.
- Não sobrescrever specs implementadas sem amendment/correction explícito.



