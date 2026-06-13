# Cindar's Hope — Cave Design Direction

> **Status:** documento canônico de direção ampla da caverna  
> **Local:** `docs/design/gameplay/cave/CAVE_DESIGN_DIRECTION.md`  
> **Depende de:**  
> - `docs/design/SPEC_SOURCE_MAP.md`  
> - `docs/design/SPECIFICATION_PROCESS.md`  
> - `docs/design/lore/VAALARA_GAME_CANON_DIRECTION_v1.0.md`  
> - `docs/game_rules/cave_rules.md`  
> - `docs/decisions/ADR-0005-cave-stable-run-and-replay.md`  
> - `.specs/a_implementar/spec_cave_runtime_generation_checkpoints_boss_gates.md`  
> - `.specs/a_implementar/spec_enemy_ai_roster_bestiary_faction_locks_runtime.md`  
> - `.specs/implementados/spec_cave_001_cave_scene_portal_e_runtime_basico.md` até `spec_cave_008_debug_skip_confinement_wall_distance_hardening.md`  
> - `docs/design/gameplay/cave/CAVE_MONSTER_ROSTER_DIRECTION.md`  
> **Função:** consolidar a visão de produto, regras, conteúdo e roadmap da caverna sem reimplementar o runtime já existente.  
> **Não é spec implementável.** Specs futuras devem ser quebradas em `.specs/a_implementar/`.

---

## 0. Regra de uso deste documento

Este documento deve ser lido antes de qualquer spec que toque:

```text
cave runtime
procedural generation
stable run
snapshots
boss gates
checkpoints
cave monsters
enemy packs
mining nodes
treasure rooms
special rooms
level 100
level 101
Anya final lore
Bromécia/Elyndor cave hooks
cave resources
cave UI/checkpoint portal
cave death/corpse/Anya integration
```

Regra principal:

```text
Não reconstruir a caverna do zero.
Preservar CaveRunManager, CaveProceduralGenerator, CaveRuntimeMaterializer, snapshots, boss gates, checkpoint selection, confinement e EnemySpawnResolver existentes.
Refinar em cima do estado real já implementado/parcial.
```

---

# PARTE A — Estado real que deve ser preservado

## 1. Sistemas já existentes/parciais

A caverna já possui base implementada/parcial para:

```text
CaveScene
Farm -> Cave transition
CaveRunManager
CaveWorldSeed
CaveRunSeed
CurrentCaveLevel
DeepestLayerReached
UnlockedCheckpoints
DepletedNodeIds
VisitedLevelSnapshots
BossDefeatStates
CaveProceduralGenerator
CaveGeneratedLevel
CaveGenerationConfigSO
CaveRuntimeMaterializer
ResourceNodeDataSO
ResourceNode
CaveBiomeDataSO
CaveBiomeRegistrySO
CaveBossGateDataSO
CaveBossGateRegistrySO
CaveBossSpawner
CaveBossDefeatMonitor
CaveCheckpointSelectionUI
CaveCheckpointSideMenuController
CavePlayerPathConfinement
CaveDebugLevelSkipController
VisitedLevelSnapshot
CaveSaveData
EnemySpawnResolver
CaveEnemySpawnPlan
BestiaryManager
EnemyBrain
EnemyDataSO
EnemyActionSO
EnemyActionSetSO
EnemySpawnProfileSO
EnemySpawnPackSO
EnemyFactionLockSO
```

## 2. Decisões existentes que continuam válidas

```text
A caverna é procedural por run, não por entrada.
Dentro da mesma CaveRunSeed, revisitar nível usa snapshot, não reroll.
ForwardExit e BackExit não alteram CaveRunSeed.
KO/death/defeat pode gerar nova CaveRunSeed.
Debug regeneration só pode ocorrer por comando explícito.
Boss gates existem em níveis de marco.
Checkpoints são desbloqueados por boss gate.
Boss derrotado não deve repetir recompensa única.
Resources e fishing spots entram no snapshot.
EnemySpawnPlan entra no snapshot.
EnemyIds não mudam ao revisitar o nível.
Inimigos comuns podem respawnar depois de 2 dias in-game sem trocar identidade.
Após morte, inimigos podem ser redistribuídos sem reroll de EnemyIds.
Confinement impede player/spawns fora de walkable tiles.
```

---

# PARTE B — Escopo de produto da caverna

## 3. Papel da caverna no jogo

A caverna é o eixo principal de:

```text
mineração
combate
progressão de equipamento
loot raro
recursos para construção e crafting
bestiarização
boss gates
checkpoints
segredos de Bromécia/Elyndor
aproximação gradual da verdade sobre Anya
endgame narrativo
```

A fazenda pode ter pedreira no último nível, mas a mineração principal deve vir da caverna.

## 4. Fantasia da caverna

A caverna não é apenas uma mina.

Ela deve parecer uma estrutura subterrânea antiga, instável e viva, formada por:

```text
cavernas naturais
floresta subterrânea
gelo profundo
fornalhas naturais
ruínas antigas
abismo escuro
núcleo corrompido
tecnologia perdida de Bromécia/Elyndor
Pedra Negra corrompida
traços de Mana e Água Viva
memórias ocultas de Anya
```

A sensação desejada é:

```text
exploração perigosa + mineração valiosa + combate frequente + descoberta de lore.
```

---

# PARTE C — Modelo macro de níveis

## 5. Estrutura geral

```text
Níveis 1-100: caverna procedural macro
Nível 101: Câmara final / Boss Gauntlet / Verdade parcial de Anya
```

## 6. Biomas por faixa

| Faixa | Bioma | Função |
|---:|---|---|
| 1-10 | Caverna de Pedra | tutorial real de mineração, combate e recursos básicos |
| 11-25 | Floresta Subterrânea | vida fúngica, raízes, goblins, kobolds, orcs de Nyx |
| 26-40 | Caverna de Gelo | duergar, mortos frios, cristal, hazards de frio |
| 41-55 | Caverna de Fogo | orcs de Kaand, elementais, fornalhas, calor |
| 56-70 | Ruínas Antigas | tecnologia, gnomos/gnomorin, constructos, registros quebrados |
| 71-85 | Abismo Sombrio | drow, Nyx, vazio, sombras e distorções |
| 86-99 | Núcleo Corrompido | Pedra Negra, Ninrorin, dracônicos, corrupção avançada |
| 100 | Boss Gate Final | bloqueia acesso ao nível 101 |
| 101 | Câmara de Anya | sequência de bosses + informação final para libertar parte do poder de Anya |

---

# PARTE D — Boss gates e checkpoints

## 7. Boss gates canônicos

Boss gates devem existir em:

```text
15
30
45
60
75
90
100
```

Mudança importante:

```text
O nível 100 passa a ser boss gate final.
Derrotar o boss gate do nível 100 desbloqueia o acesso ao nível 101.
```

## 8. Checkpoints canônicos

Checkpoints normais:

```text
1
15
30
45
60
75
90
100
```

O nível 101 não é checkpoint comum.

Regra:

```text
Nível 101 é acesso especial de endgame narrativo.
Ele pode permitir retorno por portal próprio, mas não deve virar ponto de farm normal.
```

## 9. Mapeamento de boss gates

| Gate | Bioma/Função | Resultado |
|---:|---|---|
| 15 | primeira prova real da Floresta Subterrânea | libera checkpoint 15 |
| 30 | marco do Gelo | libera checkpoint 30 |
| 45 | marco do Fogo | libera checkpoint 45 |
| 60 | marco das Ruínas Antigas | libera checkpoint 60 |
| 75 | marco do Abismo | libera checkpoint 75 |
| 90 | marco do Núcleo Corrompido | libera checkpoint 90 |
| 100 | boss gate final | libera acesso ao nível 101 |

## 10. Regra de boss gate final

O gate 100 deve ser diferente dos anteriores:

```text
Não é apenas mais um checkpoint.
É o fechamento da escalada da caverna.
Exige que os gates anteriores estejam completed.
Pode exigir fragmentos/chaves/vestígios de lore coletados.
Derrotar o boss do nível 100 abre caminho para o nível 101.
Não deve conceder novamente recompensa única.
Não deve liberar farm infinito de boss.
```

## 11. Nível 101 — Câmara de Anya

O nível 101 é um espaço especial, não um nível procedural comum.

Função:

```text
boss gauntlet
lore final parcial de Anya
libertar parte do poder de Anya
fechar arco profundo da caverna
abrir novo estado de mundo/fazenda/fonte
```

Regras:

```text
Não gerar mineração comum.
Não gerar packs comuns.
Não usar layout procedural normal de cavernas.
Não ser acessível antes do gate 100.
Não revelar toda a verdade absoluta de Anya; revelar parte suficiente para progressão.
```

Composição sugerida:

```text
1. Entrada silenciosa / corredor de memória
2. Boss 101-A: Guardião de Pedra Negra
3. Sala curta de lore de Bromécia/Elyndor
4. Boss 101-B: Arauto Quebrado de Nyx ou Constructo de Juramento
5. Sala de Água Viva corrompida
6. Boss 101-C: Wyvern de Pedra Negra ou Núcleo Dracônico
7. Câmara final: fragmento do poder de Anya
8. Evento de libertação parcial
9. Portal de retorno
```

---

# PARTE E — Densidade de criaturas

## 12. Problema atual

As regras antigas de 12-20 inimigos por nível deixam a caverna vazia para o tamanho real atual:

```text
CaveGenerationConfigSO atual: 160x96 tiles
MinRooms: 8
MaxRooms: 14
Corridors: 2-3 tiles
```

A nova direção aumenta a densidade de criaturas.

## 13. Nova densidade canônica de inimigos

A caverna deve usar uma densidade maior por nível, controlada por bioma, tamanho do layout e performance.

Tabela alvo:

| Faixa | Inimigos planejados por nível | Packs por nível | Elites por nível | Observação |
|---:|---:|---:|---:|---|
| 1-10 | 22-34 | 5-8 | 0-1 | início ainda legível, mas não vazio |
| 11-25 | 28-42 | 6-9 | 1-2 | floresta subterrânea mais viva |
| 26-40 | 30-44 | 6-10 | 1-3 | gelo com patrulhas e mortos |
| 41-55 | 32-48 | 7-10 | 2-3 | fogo mais agressivo |
| 56-70 | 34-50 | 7-11 | 2-4 | ruínas com constructos e grupos mistos |
| 71-85 | 36-52 | 8-12 | 3-5 | abismo mais perigoso |
| 86-99 | 38-56 | 8-13 | 4-6 | núcleo corrompido denso e hostil |
| Boss gate | 18-32 + boss | 4-7 | 1-3 | menos comuns perto da arena, foco no gate |
| 100 | 12-24 + boss final | 3-5 | 2-4 | tensão antes do acesso 101 |
| 101 | bosses fixos | 0 | bosses | sem packs comuns |

## 14. Regra de densidade adaptativa

A densidade deve respeitar:

```text
número de salas
área walkable
tamanho de sala
bioma
boss gate
safe spawn
MinDistanceFromWall
size profiles
performance budget
```

Fórmula de direção:

```text
TargetEnemies = clamp(BiomeBaseDensity + RoomCountBonus + LevelDepthBonus, MinByBiome, MaxByBiome)
```

Não precisa implementar essa fórmula literalmente agora; specs futuras podem definir o cálculo final.

## 15. Packs e distribuição

A geração não deve espalhar 50 inimigos de forma aleatória sem leitura.

A composição deve ser por grupos:

```text
packs pequenos: 2-4 inimigos
packs médios: 4-6 inimigos
packs grandes: 6-8 inimigos, raros e em salas maiores
elite packs: 1 elite + 2-5 suporte
ambient enemies: criaturas isoladas em corredores/salas secundárias
boss gate packs: guardas ou criaturas temáticas antes do boss
```

Regra:

```text
A caverna deve parecer habitada, não apenas preenchida.
```

## 16. Active budget runtime

Para evitar problemas de performance, a spec pode diferenciar:

```text
EnemiesPlannedInSnapshot: quantidade total do nível
EnemiesMaterializedNearPlayer: quantidade ativa/visível por proximidade, se necessário
EnemiesDefeatedState: estado persistente por planned id
```

Direção:

```text
A composição completa entra no snapshot.
A materialização pode ser otimizada depois sem trocar identidade dos inimigos.
```

---

# PARTE F — Elementos gerados por nível

## 17. Caverna não deve gerar só chão, parede, inimigos e nodes

Cada nível deve poder conter elementos de destaque.

Categorias:

```text
salas de mineração
veios raros
tesouros
baús
pontos de lore
salas especiais
altares quebrados
marcas de deuses
ruínas de Bromécia/Elyndor
hazards ambientais
atalhos
salas de pesca subterrânea
mini arenas
eventos raros
```

## 18. Quantidade alvo de elementos especiais

| Tipo | Quantidade por nível | Regra |
|---|---:|---|
| Mining clusters comuns | 3-7 | principal fonte de minério |
| Mining cluster raro | 0-2 | maior chance em níveis profundos |
| Treasure node/baú | 0-2 | protegido por pack, puzzle ou chave |
| Lore point | 0-1 | mais comum em ruínas/abismo/núcleo |
| Special room | 0-2 | sala temática gerada por bioma |
| Hazard zone | 0-3 | por bioma, aumenta com profundidade |
| Fishing spot | 10% e máx. 1 | regra existente preservada |
| Secret room | 0-1 raro | requer ferramenta, chave, percepção futura ou evento |
| Mini arena | 0-1 | elite pack ou miniboss futuro |

## 19. Tipos de salas especiais

| Tipo | Descrição | Recompensa/Risco |
|---|---|---|
| Mining Chamber | sala com veios concentrados | minério, chance de pack guardião |
| Collapsed Tunnel | túnel parcialmente bloqueado | pedra/madeira, acesso a baú ou atalho |
| Fungal Grove | bosque subterrâneo | fungos, inimigos fúngicos, reagentes |
| Frozen Shrine | altar congelado | gelo, resistência/frio, lore fragmentado |
| Ember Forge | fornalha antiga | metal raro, calor, orcs/constructos |
| Broken Archive | arquivo de Elyndor/Bromécia | lore, peças, puzzle futuro |
| Blackstone Scar | ferida de Pedra Negra | minério raro, inimigos corrompidos, risco |
| Moonless Pool | água escura de Nyx | pesca rara, evento noturno, perigo |
| Anya Echo | eco fraco de Anya | lore, cura limitada, não é Fonte da fazenda |
| Boss Antechamber | antessala de boss gate | preparação e guardas |

## 20. Tesouros

Tesouros devem vir de:

```text
baús comuns
baús trancados
baús de boss gate
recompensas únicas
veios raros
salas secretas
puzzles futuros
drops de elite
```

Categorias de tesouro:

```text
ouro
minério
gemas
reagentes
sementes raras
fragmentos de Pedra Negra estabilizada
partes de constructos
essências elementais
equipamentos
blueprints
itens de lore
chaves/fragmentos de progressão
```

---

# PARTE G — Mineração e recursos

## 21. Mineração principal

A mineração principal vem da caverna.

A pedreira da fazenda é:

```text
late game
nível final da fazenda
suporte econômico
não substitui a caverna
```

## 22. Recursos por faixa

| Faixa | Recursos principais | Recursos raros |
|---:|---|---|
| 1-10 | pedra, cobre, carvão, argila mineral | gema simples |
| 11-25 | cobre alto, ferro inicial, fungos minerais, madeira petrificada | raiz mineralizada |
| 26-40 | ferro, prata, cristal de gelo, sal profundo | cristal azul-frio |
| 41-55 | ferro alto, enxofre, brasa mineral, obsidiana | núcleo ígneo |
| 56-70 | prata alta, ouro inicial, peças antigas, runas quebradas | engrenagem bromeciana |
| 71-85 | ouro, cristal sombrio, tecido drow, eco lunar | fragmento de Nyx |
| 86-99 | mithril/metal raro, Pedra Negra instável, essência corrompida | Pedra Negra estabilizada |
| 100 | fragmentos de gate, boss reward | chave do nível 101 |
| 101 | sem mineração comum | fragmento de Anya / item de lore |

## 23. Estados de recurso

Todo recurso gerado deve poder entrar no snapshot:

```text
ResourceNodeInstanceId
ResourceNodeId
GridPosition
IsDepleted
RespawnPolicy
BiomeId
Rarity
```

Regra atual preservada:

```text
Resources não renovam dentro da mesma run no MVP, salvo política futura explícita.
```

---

# PARTE H — Level 100 e 101

## 24. Level 100 — Boss Gate Final

O nível 100 deve ser semi-especial:

```text
mais controlado que procedural normal
pode usar layout procedural com sala final garantida
pode exigir todos os gates anteriores completed
boss final do arco da caverna
abre acesso ao nível 101
```

Elementos obrigatórios:

```text
entrada segura
antessala
mineração mínima ou simbólica
poucos tesouros antes do boss
boss arena
portal selado para nível 101
registro incompleto de Anya
```

## 25. Level 101 — Câmara Final

Nível 101 deve ser conteúdo de endgame narrativo.

Características:

```text
não procedural comum
não farming loop
sem mineração comum
sem packs comuns
vários bosses em sequência ou salas conectadas
lore de Anya guardada em camadas
libertação parcial do poder de Anya
retorno seguro ao final
```

Bosses possíveis:

```text
Guardião de Pedra Negra
Constructo de Juramento de Elyndor
Arauto Quebrado de Nyx
Wyvern de Pedra Negra
Núcleo Corrompido Final
```

Resultado narrativo:

```text
O jogador não liberta Anya completamente.
O jogador liberta parte do poder dela ou restaura parte da Fonte.
Isso abre novos efeitos na fazenda/Fonte/skills/respec/ressurreição.
```

---

# PARTE I — Integração com Anya, Bromécia e Elyndor

## 26. Anya

Regras:

```text
Anya não tem altar construível na fazenda.
A Fonte da fazenda é a única representação física ativa de Anya na fazenda.
Na caverna, Anya aparece como eco, vestígio, registro, água viva corrompida ou memória fragmentada.
Nível 101 guarda informação final parcial para libertar parte do poder dela.
```

## 27. Bromécia/Elyndor

A caverna deve conter:

```text
ruínas técnicas
salas de arquivo quebradas
constructos antigos
portas seladas
registros incompletos
mecanismos de checkpoint/portal
vestígios de experimentos com Pedra Negra/Mana/Água Viva
```

Esses elementos devem ser mais frequentes a partir dos níveis 56-70.

---

# PARTE J — Roadmap conceitual

## 28. Roadmap 0 — Reconciliar base existente

- confirmar escala real da cave 160x96;
- confirmar tile size/PPU;
- reconciliar densidade inimigos: 12-20 vs 14-24 vs nova direção 22-56;
- confirmar EnemySpawnResolver atual;
- confirmar assets gerados para SPEC 13/14;
- confirmar boss gate registry real;
- confirmar Play Mode pendente.

## 29. Roadmap 1 — Design canônico e dados

- consolidar `CAVE_DESIGN_DIRECTION.md`;
- consolidar `CAVE_MONSTER_ROSTER_DIRECTION.md`;
- atualizar `SPEC_SOURCE_MAP.md`;
- criar/adaptar game rule futura para nova densidade;
- alinhar ADR se necessário via novo ADR/amendment.

## 30. Roadmap 2 — Densidade e elementos especiais

- aumentar EnemySpawnPlan por faixa;
- implementar packs maiores;
- adicionar mining clusters;
- adicionar treasure nodes;
- adicionar special room descriptors;
- adicionar hazards por bioma;
- preservar snapshot/replay.

## 31. Roadmap 3 — Boss gates 15-100

- configurar boss real por gate;
- adicionar gate 100;
- garantir unlock level 101;
- garantir unique rewards;
- garantir save/load;
- validar boss 15, 30, 45, 60, 75, 90, 100.

## 32. Roadmap 4 — Nível 101

- criar layout especial;
- criar boss gauntlet;
- criar lore sequence de Anya;
- criar libertação parcial;
- criar portal de retorno;
- garantir que não vire loop de farm comum.

## 33. Roadmap 5 — Polimento e validação

- Play Mode completo;
- validação de snapshot/replay;
- validação de densidade/performance;
- validação de save/load;
- validação de boss gates;
- validação de checkpoint portal;
- validação visual.

---

# PARTE K — Specs futuras sugeridas

```text
spec_cave_design_reconciliation_density_rules.md
spec_cave_special_rooms_mining_treasure_generation.md
spec_cave_enemy_density_packs_spawnplan_rebalance.md
spec_cave_boss_gate_100_level_101_unlock.md
spec_cave_level_101_boss_gauntlet_anya_lore.md
spec_cave_monster_roster_data_expansion.md
spec_cave_resources_treasures_biome_tables.md
spec_cave_snapshot_special_elements_persistence.md
```

---

# PARTE L — Decisões fechadas neste refinamento

```text
A caverna continua procedural por run.
A caverna continua com 100 níveis macro.
O nível 101 passa a ser conteúdo especial pós-gate 100.
Boss gates passam a ser 15/30/45/60/75/90/100.
Nível 100 é boss gate final para liberar o 101.
Nível 101 contém vários bosses e a informação final parcial de Anya.
A densidade de criaturas precisa aumentar consideravelmente.
A nova direção de densidade é 22-56 inimigos planejados por nível conforme faixa, sem contar bosses.
Boss gate levels têm densidade menor que níveis comuns, mas com boss e elite packs.
A caverna deve gerar mais elementos: mining rooms, tesouros, special rooms, hazards, lore points, secret rooms e mini arenas.
A mineração principal vem da caverna, não da pedreira da fazenda.
O bestiário detalhado deve ficar em documento próprio: CAVE_MONSTER_ROSTER_DIRECTION.md.
```

---

# PARTE M — Pendências e conflitos a resolver

```text
Reconciliar cave_rules.md e ADR-0005, que ainda citam 12-20 inimigos.
Reconciliar CaveGenerationConfigSO, que hoje tem EnemyPointCount=10 e ResourcePointCount=12.
Reconciliar ImplementationStatus, que cita spawn density 14-24 e packs 10-14.
Definir se nova densidade é materializada toda de uma vez ou por active budget/proximidade.
Definir bosses finais oficiais de 15/30/45/60/75/90/100.
Definir recompensa única de cada gate.
Definir se nível 101 é cena própria ou CaveScene especial.
Definir como a libertação parcial de Anya altera Fonte/fazenda/skills/respec/ressurreição.
Validar tudo em Unity antes de promover specs para completas.
