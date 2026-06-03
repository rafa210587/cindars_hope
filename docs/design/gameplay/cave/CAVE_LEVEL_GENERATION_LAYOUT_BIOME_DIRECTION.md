# Cindar's Hope — Cave Level Generation, Layout Size & Biome Direction

> **Status:** documento canônico complementar de geração procedural, tamanho dos níveis e randomização de biomas da caverna  
> **Local:** `docs/design/gameplay/cave/CAVE_LEVEL_GENERATION_LAYOUT_BIOME_DIRECTION.md`  
> **Depende de:**  
> - `docs/design/gameplay/cave/CAVE_DESIGN_DIRECTION.md`  
> - `docs/design/gameplay/cave/CAVE_MONSTER_ROSTER_DIRECTION.md`  
> - `docs/design/gameplay/cave/CAVE_COMBAT_BALANCE_VULNERABILITIES_DIRECTION.md`  
> - `docs/game_rules/cave_rules.md`  
> - `docs/decisions/ADR-0005-cave-stable-run-and-replay.md`  
> **Função:** definir como cada nível da caverna deve variar em tamanho, forma, rooms, biomas e composição procedural sem quebrar stable run/snapshot/replay.  
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

O runtime atual já usa geração procedural e configuração de tamanho fixo.

Config conhecida:

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
```

Direção futura:

```text
Manter 160x96 como tamanho médio/default.
Adicionar ranges mínimo/máximo por tipo de nível.
Permitir variação procedural de width/height, room count, room size, special rooms e biome mix.
Registrar width/height/layout/biome no snapshot.
```

## 2. Tile size

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

# PARTE B — Tamanho dos níveis

## 3. Ranges canônicos de tamanho

Cada nível procedural deve sortear tamanho dentro de um range, de acordo com profundidade, tipo de bioma e tipo de andar.

| Tipo de nível | Width tiles | Height tiles | Pixels em 32x32 | Uso |
|---|---:|---:|---:|---|
| Small | 96-128 | 64-80 | 3072x2048 a 4096x2560 | andares rápidos, densos, early/atalhos |
| Medium | 128-176 | 80-112 | 4096x2560 a 5632x3584 | padrão geral, substitui 160x96 como média |
| Large | 176-224 | 104-136 | 5632x3328 a 7168x4352 | andares com exploração, mineração e salas especiais |
| Huge | 224-288 | 128-176 | 7168x4096 a 9216x5632 | raros, profundos, ruínas/núcleo, alto custo |
| Boss Gate | 128-192 | 96-128 | 4096x3072 a 6144x4096 | antessala + arena controlada |
| Level 100 | 160-224 | 112-160 | 5120x3584 a 7168x5120 | gate final semi-especial |
| Level 101 | custom | custom | custom | cena/layout especial, não procedural comum |

Regra:

```text
Tamanho maior não significa automaticamente mais inimigos ativos.
Tamanho maior aumenta exploração, mineração, tesouros e special rooms.
Active enemy budget continua limitado por faixa.
```

## 4. Distribuição de tamanho por profundidade

| Faixa | Small | Medium | Large | Huge |
|---:|---:|---:|---:|---:|
| 1-10 | 40% | 55% | 5% | 0% |
| 11-25 | 25% | 60% | 15% | 0% |
| 26-40 | 15% | 60% | 23% | 2% |
| 41-55 | 10% | 55% | 30% | 5% |
| 56-70 | 5% | 45% | 40% | 10% |
| 71-85 | 5% | 40% | 42% | 13% |
| 86-99 | 0% | 35% | 45% | 20% |
| Boss Gates | 0% | 40% | 60% | 0% |
| Level 100 | 0% | 20% | 60% | 20% |

## 5. Room count por tamanho

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

# PARTE C — Arquétipos de layout

## 6. Layout archetypes

Cada nível deve sortear um arquétipo de layout.

```text
CaveNetwork
  rede orgânica de salas e corredores.

MiningBranch
  eixo principal com ramificações de mineração.

RuinedComplex
  mistura de salas retangulares, corredores artificiais e câmaras quebradas.

VerticalDescent
  sensação de descida, salas em cadeia, menos loops.

LoopingLabyrinth
  múltiplas conexões, risco de se perder, bom para abismo/ruínas.

OpenCavern
  grandes salas conectadas, bom para monstros Large/Huge.

ChokeCorridors
  corredores estreitos, bom para packs menores e treasure traps.

BossApproach
  antessala, preparação, arena e portal/gate.
```

## 7. Pesos por bioma

| Bioma | Layouts favorecidos |
|---|---|
| Stone Cavern | CaveNetwork, MiningBranch, ChokeCorridors |
| Underground Forest | OpenCavern, CaveNetwork, LoopingLabyrinth |
| Ice Cave | ChokeCorridors, VerticalDescent, OpenCavern |
| Fire Cave | OpenCavern, MiningBranch, BossApproach |
| Ancient Ruins | RuinedComplex, LoopingLabyrinth, BossApproach |
| Abyss | LoopingLabyrinth, VerticalDescent, OpenCavern |
| Corrupted Core | RuinedComplex, OpenCavern, BossApproach |
| Mixed/Unstable | qualquer, com restrições por nível |

---

# PARTE D — Randomização de biomas

## 8. Decisão: bioma não deve ser totalmente fixo por faixa

A ordem fixa de biomas ajuda progressão, mas reduz unicidade de cada run.

Decisão canônica:

```text
Usar randomização ponderada de biomas por faixa.
Cada faixa tem bioma dominante, mas pode sortear biomas secundários e instáveis.
Boss gates mantêm bioma narrativo dominante.
Level 100 e 101 são especiais.
```

Isso preserva:

```text
progressão temática
faction locks
resource scaling
boss gate identity
unicidade de run
variação visual
replayability
```

## 9. Bioma dominante por faixa

| Faixa | Bioma dominante | Biomas secundários possíveis |
|---:|---|---|
| 1-10 | Stone Cavern | Underground Forest baixo, Ancient Ruins raro |
| 11-25 | Underground Forest | Stone Cavern, Ice Cave baixo, Abyss raro/Nyx |
| 26-40 | Ice Cave | Stone Cavern, Ancient Ruins, Underground Forest raro |
| 41-55 | Fire Cave | Stone Cavern, Ancient Ruins, Corrupted Core raro |
| 56-70 | Ancient Ruins | Fire Cave, Ice Cave, Abyss, Corrupted Core raro |
| 71-85 | Abyss | Ancient Ruins, Underground Forest corrompida, Corrupted Core |
| 86-99 | Corrupted Core | Abyss, Ancient Ruins, Fire/Ice instável raro |
| 100 | Corrupted Core/Gate Final | controlado, semi-fixo |
| 101 | Câmara de Anya | fixo/custom |

## 10. Pesos iniciais por faixa

| Faixa | Dominante | Secundário A | Secundário B | Instável/Raro |
|---:|---:|---:|---:|---:|
| 1-10 | 80% | 15% | 5% | 0% |
| 11-25 | 70% | 20% | 8% | 2% |
| 26-40 | 70% | 18% | 10% | 2% |
| 41-55 | 68% | 20% | 10% | 2% |
| 56-70 | 62% | 22% | 12% | 4% |
| 71-85 | 60% | 24% | 12% | 4% |
| 86-99 | 65% | 22% | 10% | 3% |
| Boss Gates | 90% narrativo | 10% contaminante | 0% | 0% |
| 100 | 100% controlado | 0% | 0% | 0% |

## 11. Biome tags dentro do mesmo andar

Um andar pode ter um bioma principal e micro-zonas.

```text
PrimaryBiomeId
SecondaryBiomeId optional
BiomeContaminationTags[]
RoomBiomeOverride[]
HazardTags[]
ResourceTableId
EnemySpawnTableId
```

Exemplo:

```text
Floor 37
PrimaryBiome: Ice Cave
SecondaryBiome: Ancient Ruins
Contamination: FrozenArchive
Resultado: gelo + sala bromeciana congelada + constructos + mortos frios.
```

## 12. Regras de consistência

Randomização de bioma deve respeitar:

```text
boss gates
checkpoint identity
resource progression
enemy faction locks
visual readability
hazards compatíveis
snapshot/replay
```

Não permitir:

```text
bioma de fogo puro no andar 2
Corrupted Core completo antes de 56, salvo sala rara muito limitada
boss gate incoerente com boss
level 101 randomizado
```

---

# PARTE E — Conteúdo procedural por nível

## 13. Elementos gerados por andar

Cada andar pode conter:

```text
Entrance room
Exit room
Main path
Side rooms
Mining clusters
Treasure rooms
Special rooms
Hazard zones
Lore points
Secret rooms
Fishing spot rare
Mini arena
Boss antechamber, se gate
Boss arena, se gate
```

## 14. Ranges por tipo de andar

| Elemento | Small | Medium | Large | Huge |
|---|---:|---:|---:|---:|
| Mining clusters | 1-3 | 2-5 | 3-7 | 5-10 |
| Treasure rooms | 0-1 | 0-2 | 1-3 | 2-4 |
| Special rooms | 0-1 | 1-2 | 2-4 | 3-6 |
| Hazard zones | 0-2 | 1-3 | 2-5 | 3-7 |
| Lore points | 0-1 | 0-1 | 0-2 | 1-3 |
| Secret rooms | 0-1 raro | 0-1 | 0-2 | 1-2 |
| Mini arenas | 0 | 0-1 | 0-2 | 1-3 |

## 15. Pacing obrigatório

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

# PARTE F — Boss gates, nível 100 e nível 101

## 16. Boss gate layout

Boss gate level deve ter:

```text
entrada segura
antessala
1-2 packs guardiões
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

## 17. Nível 100

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

## 18. Nível 101

Nível 101 não deve usar geração comum.

Direção:

```text
layout custom ou procedural roteirizado
sem mineração comum
sem packs comuns
boss gauntlet
salas narrativas
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

# PARTE G — Snapshot e save

## 19. Dados que precisam persistir

Ao randomizar tamanho e bioma, snapshot deve guardar:

```text
CaveLevel
CaveWorldSeed
CaveRunSeed
GenerationConfigVersion
Width
Height
PrimaryBiomeId
SecondaryBiomeId optional
BiomeContaminationTags[]
LayoutArchetype
RoomLayout data
WalkableTiles
WallTiles
EntrancePosition
ExitPosition
EnemySpawnPlan
ResourceNodes
SpecialRooms
TreasureRooms
Hazards
LorePoints
SecretRooms discovered state
BossGateState, se aplicável
```

Regra:

```text
Qualquer coisa gerada proceduralmente e percebida pelo jogador precisa persistir no snapshot.
```

---

# PARTE H — Specs futuras derivadas

```text
spec_cave_level_size_ranges_generation_config.md
spec_cave_weighted_biome_randomization.md
spec_cave_layout_archetypes_rooms_corridors.md
spec_cave_special_rooms_treasure_hazards_generation.md
spec_cave_boss_gate_layout_generation.md
spec_cave_level_100_layout_gate_to_101.md
spec_cave_level_101_custom_gauntlet_layout.md
spec_cave_snapshot_layout_biome_special_elements.md
```

---

# PARTE I — Decisões fechadas

```text
O tamanho dos níveis da caverna deve variar por range, não ficar sempre fixo em 160x96.
160x96 continua sendo tamanho médio/default, não limite fixo.
Cada nível deve sortear tipo Small/Medium/Large/Huge conforme profundidade e tipo de nível.
Biomas não serão totalmente fixos por faixa.
Cada faixa terá bioma dominante com randomização ponderada de secundários/instáveis.
Boss gates preservam bioma narrativo dominante.
Nível 100 é semi-especial e controlado.
Nível 101 é custom/roteirizado, não procedural comum.
Toda randomização deve ser persistida em snapshot/replay.
```

---

# PARTE J — Pendências

```text
Confirmar escala real da CaveScene no Unity.
Adaptar CaveGenerationConfigSO para range em vez de TargetWidth/TargetHeight fixo.
Criar CaveLevelSizeProfileSO ou equivalente.
Criar BiomeWeightProfileSO ou equivalente.
Garantir que snapshot serializa PrimaryBiome/SecondaryBiome/LayoutArchetype/SpecialRooms.
Validar performance de Large/Huge levels.
Validar navegação, câmera e confinement em mapas maiores.
Validar se active budget evita avalanche em mapas grandes.
