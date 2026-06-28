# Cindar's Hope — Cave Level Generation, Layout Size, Biome, Packs & Traps Direction

> **Status:** documento canônico complementar de geração procedural, tamanho dos níveis, biomas por faixa, packs e traps da caverna  
> **Local:** `docs/design/gameplay/cave/CAVE_LEVEL_GENERATION_LAYOUT_BIOME_DIRECTION.md`  
> **Depende de:**  
> - `docs/design/gameplay/cave/CAVE_DESIGN_DIRECTION.md`  
> - `docs/design/gameplay/cave/CAVE_MONSTER_ROSTER_DIRECTION.md`  
> - `docs/design/gameplay/cave/CAVE_COMBAT_BALANCE_VULNERABILITIES_DIRECTION.md`  
> - `docs/design/art/CAVE_MONSTER_VISUAL_SPRITE_DIRECTION.md`  
> - `docs/game_rules/cave_rules.md`  
> - `docs/decisions/ADR-0005-cave-stable-run-and-replay.md`  
> **Função:** definir como cada run da caverna escolhe biomas por faixa, gera níveis proceduralmente, escolhe packs/traps por bioma e preserva tudo em snapshot.  
> **Não é spec implementável.** Specs futuras devem converter estas regras em configuração/código.

---

## 0. Regra de uso

Toda spec que mexer em geração procedural da caverna deve ler este documento.

Isso inclui:

```text
CaveProceduralGenerator
CaveGenerationConfigSO
CaveBiomeRegistrySO
CaveBiomeResolver
CaveGeneratedLevel
VisitedLevelSnapshot
CaveRuntimeMaterializer
EnemySpawnResolver
ResourceNode generation
Special room generation
Trap generation
Treasure room generation
Boss gates
Level 100
Level 101
```

Regra principal:

```text
A caverna deve variar por run, mas permanecer estável dentro da mesma CaveRunSeed.
Randomização não pode quebrar snapshot/replay.
Ao revisitar nível já visitado, o jogo restaura snapshot, não rerolla bioma/layout/conteúdo.
```

---

# PARTE A — Estado atual e direção futura

## 1. Estado atual conhecido

O runtime atual já usa geração procedural, mas ainda com configuração de tamanho fixo.

Config conhecida no código atual:

```text
TargetWidth = 160
TargetHeight = 96
MinRooms = 8
MaxRooms = 14
MinRoomWidth = 12
MaxRoomWidth = 28
MinRoomHeight = 8
MaxRoomHeight = 20
CorridorMinWidth = 2
CorridorMaxWidth = 3
EnemyPointCount = 10
ResourcePointCount = 12
```

Direção futura:

```text
Manter 160x96 como tamanho médio/default, não como tamanho fixo absoluto.
Adicionar ranges mínimo/máximo por tipo de nível.
Adicionar seleção de bioma por faixa da run.
Adicionar pack pools por faixa+bioma.
Adicionar trap pools por faixa+bioma.
Registrar width/height/layout/bioma/packs/traps/special rooms no snapshot.
```

## 2. Escala visual

A caverna deve ser compatível com a escala visual geral do projeto:

```text
Tile lógico recomendado: 32x32 px
Player visual: 32x48 px
Collider: footbox inferior
```

Pendência técnica:

```text
Confirmar no Unity se a CaveScene atual usa essa escala ou se usa unidade técnica diferente.
Até validar, tratar os valores em tiles como design direction.
```

---

# PARTE B — Algoritmo procedural alvo

## 3. Princípio central

A geração procedural deve acontecer em duas camadas:

```text
Camada 1 — Run Plan
  Sorteia e fixa a identidade macro da run.
  Exemplo: qual bioma principal cada faixa terá, quais variantes de bioma entram, quais pools estão habilitados.

Camada 2 — Level Plan
  Gera cada andar individual usando o plano da run.
  Exemplo: tamanho, layout, salas, packs, traps, recursos, tesouros e hazards daquele andar.
```

Regra:

```text
Bioma principal não é sorteado livremente por andar.
Bioma principal é sorteado por faixa da run.
Todos os andares daquela faixa usam o bioma principal escolhido para a faixa.
Cada andar ainda pode ter micro-variações: salas contaminadas, sub-biomas, special rooms, hazards e packs secundários.
```

## 4. Pseudofluxo alvo

```text
GenerateCaveRunPlan(caveWorldSeed, caveRunSeed)
  Para cada CaveTier/Faixa:
    escolher PrimaryBiomeId da pool permitida da faixa
    escolher SecondaryBiomePool opcional
    escolher ContaminationTags possíveis
    escolher ResourceTableId por faixa+bioma
    escolher EnemyPackPoolId por faixa+bioma
    escolher TrapPoolId por faixa+bioma
    salvar no CaveRunPlan

GenerateCaveLevel(caveLevel, caveWorldSeed, caveRunSeed)
  se snapshot existe:
    RestoreSnapshot(caveLevel)
    return

  resolver CaveTier/Faixa do caveLevel
  carregar TierBiomePlan da run
  resolver LevelType: Normal / BossGate / Level100 / Level101

  se Level101:
    usar layout custom/roteirizado
    return

  sortear LevelSizeClass: Small/Medium/Large/Huge/BossGate/Level100
  sortear Width/Height dentro do range da size class
  sortear LayoutArchetype compatível com o bioma da faixa
  gerar rooms/corridors/main path/loops
  posicionar entrada segura e saída
  posicionar special rooms, mining clusters, treasure rooms, traps, hazards e lore points
  gerar EnemySpawnPlan usando pack pool da faixa+bioma
  aplicar active combat budget metadata
  gerar ResourceNodePlan usando resource table da faixa+bioma
  gerar TrapPlan usando trap pool da faixa+bioma
  gerar snapshot completo
  materializar runtime
```

## 5. Dados conceituais que specs futuras devem criar

```text
CaveRunPlan
CaveTierPlan
CaveLevelSizeProfileSO
CaveBiomeTierPoolSO
CaveBiomeRunPlanSaveData
CaveLayoutArchetypeProfileSO
CaveSpecialRoomProfileSO
CaveTrapProfileSO
CaveTrapPoolSO
CaveEnemyPackPoolSO
CaveResourceTierTableSO
CaveTreasureRoomProfileSO
```

---

# PARTE C — Faixas da caverna

## 6. Faixas canônicas

A caverna usa faixas macro. A faixa controla poder, loot, recursos, packs e biomas possíveis.

| Tier | Andares | Função |
|---:|---:|---|
| T1 | 1-10 | introdução real de mineração, combate e traps leves |
| T2 | 11-25 | primeira expansão de ecologia/facções e recursos intermediários |
| T3 | 26-40 | hazards ambientais claros e inimigos especializados |
| T4 | 41-55 | pressão agressiva, calor/fogo, orcs, elementais e salas perigosas |
| T5 | 56-70 | ruínas, tecnologia, constructos, tesouros e puzzles/traps mecânicas |
| T6 | 71-85 | abismo, Nyx, drow, psíquicos, medo e controle leve |
| T7 | 86-99 | núcleo corrompido, Pedra Negra, dracônicos, aberrações e endgame pré-100 |
| T8 | 100 | boss gate final |
| T9 | 101 | Câmara de Anya, custom/roteirizada |

---

# PARTE D — Tamanho dos níveis

## 7. Ranges canônicos de tamanho

Cada nível procedural deve sortear tamanho dentro de um range, de acordo com tier, bioma da faixa e tipo de andar.

| Tipo de nível | Width tiles | Height tiles | Pixels em 32x32 | Uso |
|---|---:|---:|---:|---|
| Small | 96-128 | 64-80 | 3072x2048 a 4096x2560 | andares rápidos, densos, early/atalhos |
| Medium | 128-176 | 80-112 | 4096x2560 a 5632x3584 | padrão geral, substitui 160x96 como média |
| Large | 176-224 | 104-136 | 5632x3328 a 7168x4352 | exploração, mineração e salas especiais |
| Huge | 224-288 | 128-176 | 7168x4096 a 9216x5632 | raros, profundos, ruínas/núcleo, alto custo |
| Boss Gate | 128-192 | 96-128 | 4096x3072 a 6144x4096 | antessala + arena controlada |
| Level 100 | 160-224 | 112-160 | 5120x3584 a 7168x5120 | gate final semi-especial |
| Level 101 | custom | custom | custom | cena/layout especial, não procedural comum |

Regra:

```text
Tamanho maior não significa automaticamente mais inimigos ativos.
Tamanho maior aumenta exploração, mineração, tesouros, traps e special rooms.
Active enemy budget continua limitado por faixa.
```

## 8. Distribuição de tamanho por tier

| Tier | Small | Medium | Large | Huge |
|---:|---:|---:|---:|---:|
| T1 / 1-10 | 40% | 55% | 5% | 0% |
| T2 / 11-25 | 25% | 60% | 15% | 0% |
| T3 / 26-40 | 15% | 60% | 23% | 2% |
| T4 / 41-55 | 10% | 55% | 30% | 5% |
| T5 / 56-70 | 5% | 45% | 40% | 10% |
| T6 / 71-85 | 5% | 40% | 42% | 13% |
| T7 / 86-99 | 0% | 35% | 45% | 20% |
| Boss Gates | 0% | 40% | 60% | 0% |
| Level 100 | 0% | 20% | 60% | 20% |

## 9. Room count por tamanho

| Tipo | Room count | Special rooms | Mining rooms | Treasure rooms |
|---|---:|---:|---:|---:|
| Small | 6-10 | 0-1 | 1-3 | 0-1 |
| Medium | 9-16 | 1-2 | 2-5 | 0-2 |
| Large | 14-24 | 2-4 | 3-7 | 1-3 |
| Huge | 22-36 | 3-6 | 5-10 | 2-4 |
| Boss Gate | 8-16 + arena | 1-2 | 0-2 | 1 boss reward |
| Level 100 | 12-22 + final arena | 2-4 | 0-2 simbólicas | 1-2 controladas |
| Level 101 | fixed/custom | scripted | none | scripted rewards |

---

# PARTE E — Biomas por faixa da run

## 10. Decisão revisada

Decisão canônica:

```text
A randomização de bioma principal é por faixa da run, não por nível individual.
Uma run sorteia um PrimaryBiomeId para cada tier/faixa.
Todos os andares daquela faixa usam esse PrimaryBiomeId como bioma principal.
Dentro de cada andar podem existir micro-variações, salas secundárias e contaminações.
```

Exemplo:

```text
Run Seed A
T1 / 1-10: Caverna de Pedra Úmida
T2 / 11-25: Bosque Fúngico Subterrâneo
T3 / 26-40: Galeria de Cristal Frio
T4 / 41-55: Fornalha de Basalto
T5 / 56-70: Arquivo Bromeciano Quebrado
T6 / 71-85: Abismo Sem-Lua
T7 / 86-99: Núcleo de Pedra Negra

Resultado:
Andares 11, 12, 13...25 pertencem ao Bosque Fúngico Subterrâneo.
Eles ainda podem ter salas secundárias de ruína, água escura, raízes ou Nyx, mas o bioma principal da faixa não muda.
```

## 11. Mais biomas do que faixas

Para rejogabilidade, o jogo deve ter mais biomas possíveis do que faixas disponíveis em uma run.

```text
Há 7 faixas normais até o nível 99.
Cada run usa 7 biomas principais, um por faixa.
O catálogo deve ter mais de 7 biomas possíveis.
Assim, runs diferentes podem ter identidades diferentes sem quebrar progressão.
```

## 12. Catálogo canônico inicial de biomas

| BiomeId | Nome PT-BR | Tiers permitidos | Tema |
|---|---|---:|---|
| biome_stone_dry | Caverna de Pedra Seca | T1-T2 | pedra, poeira, cobre, morcegos, goblins |
| biome_stone_wet | Caverna de Pedra Úmida | T1-T3 | poças, musgo, lodo, ratos, fungos leves |
| biome_fungal_grove | Bosque Fúngico Subterrâneo | T1-T4 | fungos, esporos, raízes, criaturas fúngicas |
| biome_blackroot_grove | Bosque de Raiz-Negra | T2-T5 | raízes hostis, seiva escura, corrupção vegetal |
| biome_crystal_cold | Galeria de Cristal Frio | T2-T5 | gelo leve, cristal, glassbone, sentinelas |
| biome_deep_ice | Abismo de Gelo Profundo | T3-T6 | frio intenso, duergar, cultistas frios, lamentos |
| biome_basalt_forge | Fornalha de Basalto | T3-T6 | calor, brasa, orcs de Kaand, lava |
| biome_ember_chasm | Fenda de Brasa | T4-T7 | fogo agressivo, elementais, hazards de calor |
| biome_bromecian_archive | Arquivo Bromeciano Quebrado | T4-T7 | ruínas técnicas, gnomorin, constructos, puzzles |
| biome_elyndor_ruins | Ruínas de Elyndor | T5-T7 | juramentos, constructos antigos, portais, memória |
| biome_moonless_abyss | Abismo Sem-Lua | T5-T7 | Nyx, drow, sombra, medo, lanternas negras |
| biome_void_warren | Covil do Vazio | T6-T7 | aberrações, devora-mentes, wisps, controle leve |
| biome_blackstone_core | Núcleo de Pedra Negra | T7-T8 | corrupção, dracônicos, observadores, Pedra Negra |
| biome_draconic_remnant | Remanescente Dracônico | T6-T8 | pseudodragões, wyverns, escamas, sopro corrompido |
| biome_anya_echo | Eco de Anya | T7-T9 especial | lore, proteção, Água Viva corrompida, não farmável |

Regra:

```text
Nem todo bioma precisa aparecer em toda run.
Alguns biomas são raros e só entram em tiers profundos.
biome_anya_echo não deve ser bioma comum; deve aparecer como sala/evento especial ou nível 101.
```

## 13. Pools de bioma por tier

| Tier | Biomas principais possíveis |
|---:|---|
| T1 / 1-10 | stone_dry, stone_wet, fungal_grove raro |
| T2 / 11-25 | stone_wet, fungal_grove, blackroot_grove, crystal_cold raro |
| T3 / 26-40 | crystal_cold, deep_ice, fungal_grove, basalt_forge raro |
| T4 / 41-55 | basalt_forge, ember_chasm, blackroot_grove, bromecian_archive raro |
| T5 / 56-70 | bromecian_archive, elyndor_ruins, deep_ice, moonless_abyss raro |
| T6 / 71-85 | moonless_abyss, void_warren, elyndor_ruins, draconic_remnant raro |
| T7 / 86-99 | blackstone_core, draconic_remnant, void_warren, moonless_abyss contaminado |
| T8 / 100 | blackstone_core ou draconic_remnant controlado |
| T9 / 101 | anya_echo/custom |

## 14. Pesos iniciais por tier

Cada tier escolhe um PrimaryBiomeId com pesos. Esses pesos são direção inicial; specs futuras podem balancear.

| Tier | Opção A | Peso | Opção B | Peso | Opção C | Peso | Opção rara | Peso |
|---:|---|---:|---|---:|---|---:|---|---:|
| T1 | stone_dry | 55% | stone_wet | 35% | fungal_grove | 10% | — | 0% |
| T2 | fungal_grove | 40% | stone_wet | 25% | blackroot_grove | 25% | crystal_cold | 10% |
| T3 | crystal_cold | 45% | deep_ice | 30% | fungal_grove | 15% | basalt_forge | 10% |
| T4 | basalt_forge | 45% | ember_chasm | 30% | blackroot_grove | 15% | bromecian_archive | 10% |
| T5 | bromecian_archive | 45% | elyndor_ruins | 30% | deep_ice | 15% | moonless_abyss | 10% |
| T6 | moonless_abyss | 40% | void_warren | 25% | elyndor_ruins | 25% | draconic_remnant | 10% |
| T7 | blackstone_core | 50% | draconic_remnant | 25% | void_warren | 15% | moonless_abyss | 10% |
| T8 | blackstone_core | 70% | draconic_remnant | 30% | — | — | — | — |
| T9 | anya_echo | 100% | — | — | — | — | — | — |

## 15. Sub-biomas e contaminações por andar

Mesmo com PrimaryBiome fixo por faixa, cada andar pode sortear detalhes internos.

```text
PrimaryBiomeId = escolhido uma vez por tier/faixa da run.
SecondaryRoomBiomeId = sorteado por andar ou sala especial.
ContaminationTags = modificadores raros por andar.
RoomBiomeOverride = bioma secundário aplicado a sala específica.
```

Contaminações possíveis:

```text
BlackstoneScar / Cicatriz de Pedra Negra
NyxShadow / Sombra de Nyx
AnyaEcho / Eco de Anya
BromecianMachinery / Maquinário Bromeciano
ElyndorSeal / Selo de Elyndor
ManaLeak / Vazamento de Mana
LivingWaterCorrupt / Água Viva Corrompida
```

Regra:

```text
Contaminação adiciona flavor, hazards, sala especial, loot ou pack raro.
Contaminação não troca o tier de progressão.
```

---

# PARTE F — Arquétipos de layout

## 16. Layout archetypes

Cada nível deve sortear um arquétipo de layout compatível com o bioma principal da faixa.

```text
CaveNetwork / Rede de Cavernas
  rede orgânica de salas e corredores.

MiningBranch / Ramificações de Mineração
  eixo principal com braços laterais de mineração.

RuinedComplex / Complexo Arruinado
  salas retangulares, corredores artificiais e câmaras quebradas.

VerticalDescent / Descida Vertical
  sensação de descida, salas em cadeia, menos loops.

LoopingLabyrinth / Labirinto em Laços
  múltiplas conexões, risco de se perder, bom para abismo/ruínas.

OpenCavern / Caverna Aberta
  grandes salas conectadas, bom para monstros Large/Huge.

ChokeCorridors / Corredores de Gargalo
  corredores estreitos, bom para packs menores, traps e treasure traps.

BossApproach / Aproximação de Boss
  antessala, preparação, arena e portal/gate.
```

## 17. Pesos por bioma

| Bioma | Layouts favorecidos |
|---|---|
| Caverna de Pedra Seca | CaveNetwork, MiningBranch, ChokeCorridors |
| Caverna de Pedra Úmida | CaveNetwork, OpenCavern, ChokeCorridors |
| Bosque Fúngico Subterrâneo | OpenCavern, CaveNetwork, LoopingLabyrinth |
| Bosque de Raiz-Negra | LoopingLabyrinth, ChokeCorridors, OpenCavern |
| Galeria de Cristal Frio | ChokeCorridors, OpenCavern, VerticalDescent |
| Abismo de Gelo Profundo | VerticalDescent, ChokeCorridors, CaveNetwork |
| Fornalha de Basalto | OpenCavern, MiningBranch, BossApproach |
| Fenda de Brasa | OpenCavern, VerticalDescent, ChokeCorridors |
| Arquivo Bromeciano Quebrado | RuinedComplex, LoopingLabyrinth, BossApproach |
| Ruínas de Elyndor | RuinedComplex, BossApproach, CaveNetwork |
| Abismo Sem-Lua | LoopingLabyrinth, VerticalDescent, OpenCavern |
| Covil do Vazio | LoopingLabyrinth, ChokeCorridors, OpenCavern |
| Núcleo de Pedra Negra | RuinedComplex, OpenCavern, BossApproach |
| Remanescente Dracônico | OpenCavern, BossApproach, MiningBranch |
| Eco de Anya | custom/roteirizado, não procedural comum |

---

# PARTE G — Packs possíveis por tier+bioma

## 18. Regra de seleção de packs

Packs devem ser escolhidos por:

```text
Tier/Faixa
PrimaryBiomeId da faixa
RoomType
Faction locks
Active combat budget
SpecialRoom tags
Trap/Treasure context
```

Regra:

```text
O bioma da faixa define a pool principal de packs.
Cada andar sorteia uma composição a partir dessa pool.
Packs secundários podem entrar por sala especial, contaminação ou treasure room.
```

## 19. Tabela de pack pools por bioma

| Bioma | Packs comuns | Packs elite/especiais | Packs raros/contaminação |
|---|---|---|---|
| Caverna de Pedra Seca | stone_swarm_low, goblin_kobold_low, rust_miner_low | sludge_treasure_low | cracked_undead_low |
| Caverna de Pedra Úmida | fungal_low, stone_swarm_low, sludge_treasure_low | rust_miner_low, chest_biter_low | blackroot_sprout_patch |
| Bosque Fúngico Subterrâneo | spore_grove, rootsnare_patch, fungal_low | fungal_elite, root_owlbear_den | nyx_stalkers raro |
| Bosque de Raiz-Negra | rootsnare_patch, blackroot_guard, fungal_elite | basilisk_grove, root_owlbear_den | anya_echo_event raro |
| Galeria de Cristal Frio | frost_beasts, crystal_hunt, frozen_guard | hook_horror_cave | mind_larva_nest raro |
| Abismo de Gelo Profundo | duergar_patrol, frozen_guard, frost_beasts | hook_horror_cave, frost_wailer_circle | cold_cult_ritual |
| Fornalha de Basalto | ember_swarm, lava_guard, kaand_warband | furnace_room | fire_basilisk_lair |
| Fenda de Brasa | ember_swarm, ash_predators, kaand_warband | stone_bulette_ambush, fire_basilisk_lair | scorched_cult_ritual |
| Arquivo Bromeciano Quebrado | rune_swarm, gnome_ruin_team, puzzle_guard | mimic_armory_room, observer_ruin_minor | memory_jelly_archive |
| Ruínas de Elyndor | sealed_hall, puzzle_guard, rune_swarm | oathless_shade_hall, mirror_adept_circle | anya_echo_event raro |
| Abismo Sem-Lua | drow_patrol, moon_cult, abyss_hunt | void_elite, blackmoon_eye_room | mind_eater_cell |
| Covil do Vazio | abyss_wisp_swarm, mind_eater_cell, void_elite | blackmoon_eye_room | brain_jelly_archive |
| Núcleo de Pedra Negra | corrupted_core, blackstone_warden, draconic_corruption | beholder_blackstone_chamber, core_devourer_treasure | anya_echo_event especial |
| Remanescente Dracônico | draconic_corruption, mana_warped_hunt, blackstone_warden | blackstone_wyvern_lair, corebreaker_mining_room | anya_echo_event especial |
| Eco de Anya | não usa pack comum | lore guardian/teste | boss_anya_bound_echo |

## 20. Packs novos nomeados para preencher pools

Alguns packs abaixo são aliases/design direction para futura conversão em `EnemySpawnPackSO`.

```text
cracked_undead_low
  cracked_bone x4-7 + cave_bat x2-4
  Motivo: sala abandonada onde mortos e morcegos ocupam restos secos.

blackroot_sprout_patch
  blackroot_sprout x4-7 + mossling x2-4
  Motivo: contaminação vegetal inicial.

blackroot_guard
  rootsnare x3-5 + blackroot_sprout x4-6 + mycobulwark x0-1
  Motivo: raiz protegendo passagem/minério.

frost_wailer_circle
  frost_wailer x1-2 + glassbone x3-5 + cold_cult_acolyte x1-2
  Motivo: mortos frios atraídos por ritual.

cold_cult_ritual
  cold_cult_acolyte x3-5 + icebound_sentinel x1 + glassbone x2-4
  Motivo: sala ritual de frio.

ash_predators
  ash_crawler x4-7 + cinder_spitter x2-4
  Motivo: predadores que vivem em cinzas e gases quentes.

scorched_cult_ritual
  scorched_cultist x3-5 + ember_tick x6-10 + lava_bulwark x0-1
  Motivo: culto alimentando brasa/fenda.

oathless_shade_hall
  oathless_shade x2-4 + sealed_knight x1-2
  Motivo: corredor de juramentos quebrados.

mirror_adept_circle
  mirror_adept x2-3 + rune_shard x4-6
  Motivo: sala de espelhos e runas quebradas.

abyss_wisp_swarm
  abyss_wisp x8-12 + black_lantern_cultist x0-1
  Motivo: enxame de sensores abissais.

mana_warped_hunt
  mana_warped_beast x2-4 + corrupted_pseudodragon x2-4
  Motivo: fauna deformada por Mana e Pedra Negra.

blackstone_wyvern_lair
  blackstone_wyvern x1 miniboss/rare + corrupted_pseudodragon x3-5
  Motivo: câmara dracônica corrompida; não usar em corredores comuns.
```

## 21. Regra de active budget aplicada a packs

```text
O nível pode planejar muitos packs, mas só parte deles ativa ao mesmo tempo.
Packs grandes devem ser ancorados em salas grandes, arenas ou special rooms.
Packs de treasure/trap devem ativar por proximidade/interação, não por aggro global.
```

---

# PARTE H — Traps / Armadilhas

## 22. Função das traps

Traps servem para:

```text
criar risco fora do combate direto
premiar atenção visual
dar valor a pets/companions/percepção futura
proteger tesouros
variar exploração
reforçar identidade do bioma
```

Não devem servir para:

```text
matar instantaneamente sem aviso
punir sem counterplay
bloquear progresso principal de forma aleatória
causar perda permanente de item sem spec própria
```

## 23. Tipos canônicos de traps

| TrapId | Nome PT-BR | Tipo | Efeito |
|---|---|---|---|
| trap_loose_rocks | Pedras Soltas | físico | queda de pedras, dano leve/médio, telegraph de tremor |
| trap_spike_floor | Espinhos de Chão | físico | dano Pierce, ativa ao pisar, pode ser desarmada |
| trap_tripwire_bones | Fio de Ossos | físico/som | chama pack próximo ou dispara projétil simples |
| trap_rust_cloud | Nuvem de Ferrugem | equipamento/status | DurabilityStress leve, sem destruir item |
| trap_spore_pod | Bolsa de Esporos | poison/slow | nuvem de Poison/Slow curta |
| trap_root_snare | Raiz Agarradora | root | prende curto, janela de reação |
| trap_ice_plate | Placa de Gelo Fino | movimento | escorregão/slow/chill leve |
| trap_frost_burst_rune | Runa de Estouro Frio | magia/frio | pulso frio com windup |
| trap_ember_vent | Respiro de Brasa | fogo | jato intermitente, Burn leve |
| trap_lava_crack | Rachadura de Lava | fogo/terreno | zona quente, dano ao permanecer |
| trap_clockwork_dart | Dardo Mecânico | mecânico/pierce | disparo em linha, comum em ruínas |
| trap_rune_lock_pulse | Pulso de Runa-Trava | arcano | Stun/Slow curto, protege baú/porta |
| trap_mirror_alarm | Alarme de Espelho | summon/alerta | chama constructos ou ativa pack |
| trap_shadow_lantern | Lanterna de Sombra | medo/Nyx | Fear leve, reduz visão temporariamente |
| trap_void_gravity_snare | Laço Gravitacional do Vazio | controle | puxa/slow curto com telegraph |
| trap_blackstone_growth | Crescimento de Pedra Negra | corrupção | dano/slow em área, mais comum no núcleo |
| trap_corrupt_mana_leak | Vazamento de Mana Corrompida | mágico | dano arcano/instabilidade, raro |
| trap_false_chest | Baú Falso | treasure trap | ativa mimic/chest_biter/mimic_armory |
| trap_collapsing_bridge | Ponte Instável | movimento | quebra trecho, força rota alternativa ou queda curta controlada |
| trap_anya_echo_seal | Selo de Eco de Anya | lore/teste | não causa dano comum; exige interação/purificação/fuga |

## 24. Quantidade de traps por tamanho de nível

| Tipo de nível | Traps comuns | Traps especiais | Treasure traps | Observação |
|---|---:|---:|---:|---|
| Small | 0-2 | 0 | 0-1 raro | early precisa ser justo |
| Medium | 1-4 | 0-1 | 0-1 | padrão da run |
| Large | 3-7 | 1-2 | 0-2 | exploração com risco/recompensa |
| Huge | 5-10 | 2-4 | 1-3 | grandes mapas precisam landmarks e avisos |
| Boss Gate | 1-4 | 1-2 | 0-1 | traps não devem atrapalhar boss injustamente |
| Level 100 | 3-6 | 2-3 | 0-1 | traps ritualísticas/controladas |
| Level 101 | scripted | scripted | none/common disabled | traps como teste/lore, não RNG comum |

## 25. Quantidade de traps por tier

| Tier | Traps por nível alvo | Máximo recomendado | Observação |
|---:|---:|---:|---|
| T1 / 1-10 | 0-2 | 3 | introduzir leitura, sem letalidade alta |
| T2 / 11-25 | 1-3 | 5 | esporos, raízes, fios simples |
| T3 / 26-40 | 2-4 | 6 | gelo, runas frias, chão escorregadio |
| T4 / 41-55 | 2-5 | 7 | fogo, lava, pressão ambiental |
| T5 / 56-70 | 3-6 | 9 | mecânicas, runas, alarmes, puzzles leves |
| T6 / 71-85 | 3-7 | 10 | sombra, medo, vazio, drow traps |
| T7 / 86-99 | 4-8 | 12 | Pedra Negra, Mana corrompida, traps híbridas |
| T8 / 100 | 3-6 | 8 | controlado e narrativo |
| T9 / 101 | scripted | scripted | sem RNG comum |

## 26. Traps por bioma

| Bioma | Traps comuns | Traps raras/especiais |
|---|---|---|
| Caverna de Pedra Seca | loose_rocks, spike_floor, tripwire_bones | rust_cloud, false_chest |
| Caverna de Pedra Úmida | loose_rocks, spore_pod, root_snare | false_chest, rust_cloud |
| Bosque Fúngico Subterrâneo | spore_pod, root_snare | false_chest, anya_echo_seal raro |
| Bosque de Raiz-Negra | root_snare, spore_pod | blackstone_growth raro, anya_echo_seal raro |
| Galeria de Cristal Frio | ice_plate, frost_burst_rune | collapsing_bridge, mirror_alarm raro |
| Abismo de Gelo Profundo | ice_plate, frost_burst_rune, loose_rocks | cold ritual trap futura |
| Fornalha de Basalto | ember_vent, lava_crack, loose_rocks | false_chest queimado |
| Fenda de Brasa | ember_vent, lava_crack | collapsing_bridge, scorched_alarm futuro |
| Arquivo Bromeciano Quebrado | clockwork_dart, rune_lock_pulse, mirror_alarm | false_chest/armory, puzzle trap futura |
| Ruínas de Elyndor | rune_lock_pulse, mirror_alarm | anya_echo_seal raro, oath seal futuro |
| Abismo Sem-Lua | shadow_lantern, tripwire_bones | void_gravity_snare |
| Covil do Vazio | void_gravity_snare, shadow_lantern | mind pulse trap futura |
| Núcleo de Pedra Negra | blackstone_growth, corrupt_mana_leak | anya_echo_seal especial |
| Remanescente Dracônico | lava_crack, blackstone_growth, collapsing_bridge | draconic breath vent futuro |
| Eco de Anya | anya_echo_seal | scripted only |

## 27. Counterplay de traps

Traps devem ter pelo menos um counterplay:

```text
telegraph visual
som/partícula antes de ativar
desarme por ferramenta/skill futura
pet detection
companion warning
rota alternativa
janelas de cooldown
ativação por peso/proximidade/interação
```

Pet/cachorro:

```text
pode farejar treasure trap, false chest, tripwire, root snare e loose rocks.
não deve detectar tudo sempre.
chance depende de vínculo/treino futuro.
```

Gato:

```text
pode ajudar a detectar false chest, secret room e anomalias mágicas leves.
```

Companions:

```text
companions técnicos/mineradores podem ajudar com clockwork/rune traps.
companions mágicos podem alertar sobre Nyx/Void/Anya/Blackstone.
```

---

# PARTE I — Conteúdo procedural por nível

## 28. Elementos gerados por andar

Cada andar pode conter:

```text
Entrance room
Exit room
Main path
Side rooms
Mining clusters
Treasure rooms
Special rooms
Trap zones
Hazard zones
Lore points
Secret rooms
Fishing spot rare
Mini arena
Boss antechamber, se gate
Boss arena, se gate
```

## 29. Ranges por tipo de andar

| Elemento | Small | Medium | Large | Huge |
|---|---:|---:|---:|---:|
| Mining clusters | 1-3 | 2-5 | 3-7 | 5-10 |
| Treasure rooms | 0-1 | 0-2 | 1-3 | 2-4 |
| Special rooms | 0-1 | 1-2 | 2-4 | 3-6 |
| Trap zones | 0-2 | 1-4 | 3-7 | 5-10 |
| Hazard zones | 0-2 | 1-3 | 2-5 | 3-7 |
| Lore points | 0-1 | 0-1 | 0-2 | 1-3 |
| Secret rooms | 0-1 raro | 0-1 | 0-2 | 1-2 |
| Mini arenas | 0 | 0-1 | 0-2 | 1-3 |

## 30. Pacing obrigatório

Todo andar precisa ter:

```text
1 entrada segura
1 rota principal clara
1 saída localizável
pelo menos 1 recompensa visível ou descoberta
pelo menos 1 decisão opcional de risco/recompensa em Medium+
```

Andares Large/Huge devem ter:

```text
atalhos internos
loops
landmarks visuais
pontos de descanso visual sem combate imediato
```

---

# PARTE J — Boss gates, nível 100 e nível 101

## 31. Boss gate layout

Boss gate level deve ter:

```text
entrada segura
antessala
1-2 packs guardiões
0-2 trap zones controladas
zona de preparação
arena de boss
portal/checkpoint reward
baú/recompensa única
saída bloqueada até boss defeated
```

Tamanho:

```text
Width: 128-192 tiles
Height: 96-128 tiles
Arena: 32x24 a 56x40 tiles
```

## 32. Nível 100

Nível 100 é semi-especial.

```text
Pode usar geração procedural limitada.
Deve ter sala final garantida.
Deve ter portal selado para nível 101.
Deve exigir gate completion anterior, se definido em spec.
```

Tamanho:

```text
Width: 160-224 tiles
Height: 112-160 tiles
Arena final: 56x40 a 80x56 tiles
```

## 33. Nível 101

Nível 101 não deve usar geração comum.

Direção:

```text
layout custom ou procedural roteirizado
sem mineração comum
sem packs comuns
sem traps comuns aleatórias
boss gauntlet
salas narrativas
traps/testes roteirizados se fizerem sentido
recuperação parcial entre bosses, se necessário
informação final parcial de Anya
portal de retorno
```

Tamanho recomendado:

```text
Hub/entrada: 32x24 a 48x32 tiles
Cada arena: 48x36 a 80x56 tiles
Corredores de memória: 16-24 tiles de largura visual, conforme arte
Total: definido por cena especial, não por CaveGenerationConfigSO comum
```

---

# PARTE K — Snapshot e save

## 34. Dados que precisam persistir

Ao randomizar tamanho, bioma, packs e traps, snapshot/save deve guardar:

```text
CaveRunPlanId
CaveTierPlan[]
CaveLevel
CaveWorldSeed
CaveRunSeed
GenerationConfigVersion
Width
Height
PrimaryBiomeId
SecondaryRoomBiomeIds[]
BiomeContaminationTags[]
LayoutArchetype
RoomLayout data
WalkableTiles
WallTiles
EntrancePosition
ExitPosition
EnemySpawnPlan
EnemyPackPlan
ActiveBudgetMetadata
ResourceNodes
SpecialRooms
TreasureRooms
TrapPlan
TrapStates
Hazards
LorePoints
SecretRooms discovered state
BossGateState, se aplicável
```

Regra:

```text
Qualquer coisa gerada proceduralmente e percebida pelo jogador precisa persistir no snapshot.
```

## 35. Compatibilidade com código atual

O código atual já possui parte do caminho:

```text
Width/Height
BiomeId
WalkableTiles
WallTiles
EnemySpawnPoints
ResourceSpawnPoints
EnemySpawnPlan
ResourceNodeStates
FishingSpotState
```

Mas specs futuras precisam expandir para:

```text
CaveRunPlan
TierBiomePlan
LayoutArchetype
SpecialRooms
TreasureRooms
TrapPlan
TrapStates
Hazards
LorePoints
SecondaryRoomBiomeIds
ContaminationTags
```

---

# PARTE L — O que specs futuras devem deixar claro

## 36. Requisitos obrigatórios para specs derivadas

Qualquer spec de procedural da caverna deve declarar:

```text
Se altera ou não CaveGenerationConfigSO.
Se cria LevelSizeProfileSO.
Se cria BiomeTierPoolSO.
Se cria CaveRunPlan save data.
Se altera VisitedLevelSnapshot.
Como migra snapshots antigos.
Como preserva stable run/replay.
Como limita active combat budget.
Como valida boss gates e level 100/101.
Como valida traps em Play Mode.
```

## 37. Specs futuras derivadas

```text
spec_cave_run_plan_tier_biome_selection.md
spec_cave_level_size_ranges_generation_config.md
spec_cave_weighted_biome_by_tier_randomization.md
spec_cave_layout_archetypes_rooms_corridors.md
spec_cave_pack_pools_by_tier_biome.md
spec_cave_trap_generation_by_biome_tier.md
spec_cave_special_rooms_treasure_hazards_generation.md
spec_cave_boss_gate_layout_generation.md
spec_cave_level_100_layout_gate_to_101.md
spec_cave_level_101_custom_gauntlet_layout.md
spec_cave_snapshot_layout_biome_packs_traps_special_elements.md
```

---

# PARTE M — Decisões fechadas

```text
O tamanho dos níveis da caverna deve variar por range, não ficar sempre fixo em 160x96.
160x96 continua sendo tamanho médio/default, não limite fixo.
Cada nível deve sortear tipo Small/Medium/Large/Huge conforme tier e tipo de nível.
Bioma principal não será sorteado livremente por nível.
Bioma principal será sorteado uma vez por faixa da run.
Todos os andares daquela faixa usam o bioma principal escolhido para a faixa.
O catálogo terá mais biomas possíveis do que faixas usadas em uma run.
Packs de monstros serão sorteados por tier+bioma, com pools principais/secundárias/raras.
Traps serão sorteadas por tier+bioma+tamanho do nível.
Traps precisam ter counterplay.
Boss gates preservam identidade narrativa e não devem ser incoerentes com boss.
Nível 100 é semi-especial e controlado.
Nível 101 é custom/roteirizado, sem procedural comum.
Toda randomização percebida pelo jogador deve persistir em snapshot/replay.
```

---

# PARTE N — Pendências

```text
Confirmar escala real da CaveScene no Unity.
Adaptar CaveGenerationConfigSO para range em vez de TargetWidth/TargetHeight fixo.
Criar CaveRunPlan e persistência por run.
Criar CaveLevelSizeProfileSO ou equivalente.
Criar BiomeTierPoolSO/BiomeWeightProfileSO ou equivalente.
Criar CaveEnemyPackPoolSO por tier+bioma.
Criar CaveTrapProfileSO e CaveTrapPoolSO.
Garantir que snapshot serializa PrimaryBiome/SecondaryBiome/LayoutArchetype/SpecialRooms/TrapPlan.
Validar performance de Large/Huge levels.
Validar navegação, câmera e confinement em mapas maiores.
Validar se active budget evita avalanche em mapas grandes.
Validar traps com telegraph/counterplay em Play Mode.
