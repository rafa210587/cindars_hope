# Cindar's Hope — Farm Layout, Scale & Buildings Direction

> **Status:** documento canônico de escala visual, tamanho da fazenda, construções, props e footprint agrícola  
> **Local:** `docs/design/gameplay/farm/FARM_LAYOUT_SCALE_BUILDINGS_DIRECTION.md`  
> **Depende de:**  
> - `docs/design/lore/VAALARA_GAME_CANON_DIRECTION_v1.0.md`  
> - `docs/design/gameplay/farm/FARM_DESIGN_DIRECTION_v1.3.md`  
> - `docs/design/gameplay/city/CITY_LAYOUT_BUILDINGS_SCHEDULE_DIRECTION.md`  
> **Função:** alinhar a fazenda à mesma escala visual da cidade: player 32x48 px, tile 32x32 px, footbox collider, construções proporcionais e tamanhos em tiles/pixels.  
> **Não é spec implementável.** Specs futuras devem ser quebradas em `docs/specs/a_implementar/`.

---

## 0. Decisão

A fazenda deve seguir a mesma escala visual da cidade.

Regra canônica:

```text
Tile base: 32x32 px
Player visual: 32x48 px
NPC comum visual: 32x48 px
Collider de player/NPC: footbox inferior, não sprite inteiro
```

Isso garante que:

- player, NPCs e pets tenham proporção consistente entre fazenda e cidade;
- portas, camas, casas, currais e props tenham leitura correta;
- cidade e fazenda possam compartilhar assets, lógica de colisão e sorting;
- specs de fazenda e cidade usem a mesma régua visual.

---

# PARTE A — Escala visual

## 1. Personagens e colliders

| Tipo | Sprite visual | Collider recomendado | Observação |
|---|---:|---:|---|
| Player | 32x48 px | 20x12 a 24x16 px nos pés | referência principal |
| NPC visitante | 32x48 px | 20x12 a 24x16 px | mesma escala da cidade |
| Companion comum | 32x48 px | 20x12 a 24x16 px | igual ao NPC |
| Halfling/goblin | 28x40 px ou 32x40 px | 18x10 a 22x14 px | menor visualmente |
| Anão | 32x44 px | 22x14 px | baixo e largo |
| Orc/meio-orc/draconato | 36x52 px a 40x56 px | 24x16 px | maior visualmente |
| Pet gato | 24x24 px | 14x10 px | baixo, collider pequeno |
| Pet cachorro | 32x32 px | 18x12 px | pode seguir player e combater |
| Animal pequeno | 32x32 px | 18x12 px | galinha, pato etc. |
| Animal médio | 48x40 px | 28x16 px | ovelha, cabra etc. |
| Animal grande | 64x48 px | 36x20 px | vaca, montaria futura etc. |

Regra:

```text
O collider representa a base/pés.
O corpo visual pode sobrepor objetos por sorting Y.
```

## 2. Sorting, pivô e interação

```text
Pivot visual: bottom-center
Sorting Y: baseado na coordenada Y dos pés
Collider: base inferior
Interação: caixa/cone curto à frente do player
Grid lógico: 32x32 px
Movimento: livre suave sobre tilemap
```

## 3. Câmera

Mesma direção da cidade:

```text
Visão padrão: 20x12 a 24x14 tiles
Equivalente lógico: 640x384 px a 768x448 px antes de escala de tela
Scroll suave seguindo player
```

A fazenda deve ser maior que uma tela já no nível 1.

---

# PARTE B — Tamanho da fazenda por nível

## 4. Tamanho canônico por nível

A tabela original da fazenda já estava em tiles. Agora fica explicitamente ligada a `32x32 px`.

| Nível | Nome de trabalho | Tamanho tiles | Tamanho pixels | Observação |
|---:|---|---:|---:|---|
| 1 | Terreno Inicial | 40x32 | 1280x1024 px | maior que uma tela, fácil de ler |
| 2 | Fazenda Aberta | 56x40 | 1792x1280 px | abre lateral/sul para pasto/canteiros |
| 3 | Fazenda Produtiva | 72x52 | 2304x1664 px | permite estufa, animais e workshops |
| 4 | Fazenda Especializada | 88x64 | 2816x2048 px | layout robusto com zonas especializadas |
| 5 | Fazenda Plena | 104x72 | 3328x2304 px | espaço final para pedreira, pomar raro e área arcana |

Regra:

```text
A implementação pode usar uma única cena grande com zonas bloqueadas ou tilemaps habilitados por expansão.
O tamanho em tiles é direção de design, não obrigação de redesenhar cena por nível.
```

## 5. Comparação com cidade

```text
Cidade externa: 128x96 tiles / 4096x3072 px
Fazenda final: 104x72 tiles / 3328x2304 px
```

A fazenda final deve ser menor que a cidade, mas grande o suficiente para personalização, produção e endgame.

---

# PARTE C — Zonas fixas e livres

## 6. Zonas fixas por nível

| Zona | Nível | Flexibilidade | Observação |
|---|---:|---|---|
| Casa inicial | 1 | móvel após desbloqueio | começa fixa, pode mover depois |
| Campo inicial | 1 | livre | área inicial de plantio |
| Fonte de Anya | 1 | fixa | única representação física de Anya na fazenda |
| Lago principal | 1 | fixo | pesca e água comum |
| Bosque inicial | 1 | parcialmente livre | árvores removíveis e algumas fixas |
| SellPoint / caixa de envio | 1 | móvel após desbloqueio | preserva conteúdo ao mover |
| Entrada da caverna | 1 | fixa | marco de exploração |
| Saída para cidade | 1 | fixa | conexão com CityScene |
| Área de pet | 1–4 | livre | cama/tigela/abrigo |
| Pasto/curral | 2 | livre em terreno válido | animais grandes |
| Galinheiro | 3 | livre em terreno válido | animais pequenos |
| Estufa | 3 | livre em terreno válido | fora de estação |
| Workshops | 1–4 | livre em terreno válido | crafting/processamento |
| Companion job board | 3–4 | livre ou perto da casa | automação configurável |
| Pomar comum | 4 | livre | árvores frutíferas não-Mana |
| Área arcana da Fonte | 5 | fixa/perto da Fonte | Anya/Água Viva/Mana |
| Raiz Dormente de Mana | 5 | fixa após descoberta | evento/lore/endgame |
| Pedreira final | 5 | fixa | recurso mineral tardio e limitado |

## 7. Tiles livres

Podem ser modificados se forem válidos:

- grama comum;
- terra comum;
- solo arável;
- árvores removíveis;
- pedras pequenas;
- arbustos;
- caminhos;
- cercas;
- decoração;
- canteiros;
- construções em terreno válido.

## 8. Tiles fixos

Não podem ser movidos livremente:

- Fonte de Anya;
- lago principal;
- entrada da caverna;
- saída para cidade;
- bordas naturais;
- cliffs/rochas grandes de expansão;
- portais ou marcos de lore;
- área da pedreira final;
- área arcana diretamente ligada à Fonte;
- Raiz Dormente de Mana, se descoberta;
- áreas de expansão ainda bloqueadas.

---

# PARTE D — Construções da fazenda

## 9. Regra de footprint

Toda construção posicionável deve declarar:

```text
BuildingId
DisplayName
FootprintTiles
FootprintPx
EntranceTiles[]
RequiredClearance
CanMove
CanRotate
BlocksPath
ValidTerrainTags[]
RequiredFarmLevel
```

Regra:

```text
Footprint visual e footprint de colisão podem ser diferentes.
A área de entrada precisa ficar livre.
```

## 10. Construções principais

| ID | Construção | Footprint tiles | Footprint px | Move? | Observação |
|---|---|---:|---:|---:|---|
| farm_house | Casa do jogador | 10x8 | 320x256 | sim, após desbloqueio | dormir, salvar, cozinhar, storage inicial |
| farm_sellpoint | SellPoint / caixa de envio | 2x2 | 64x64 | sim | não pode apagar conteúdo ao mover |
| farm_pet_area_small | Área pequena de pet | 4x3 | 128x96 | sim | cama + tigela + descanso |
| farm_pet_house_dog | Casinha de cachorro | 3x3 | 96x96 | sim | pet separado de companion |
| farm_cat_bed | Cama de gato | 2x2 | 64x64 | sim | indoor/outdoor conforme spec |
| farm_storage_chest | Baú simples | 2x1 | 64x32 | sim | storage comum |
| farm_storage_shed | Depósito | 8x6 | 256x192 | sim | storage por categoria |
| farm_workshop_small | Workshop pequeno | 6x5 | 192x160 | sim | processadores iniciais |
| farm_workshop_large | Workshop grande | 10x8 | 320x256 | sim | produção avançada |
| farm_chicken_coop | Galinheiro | 8x6 | 256x192 | sim | animais pequenos |
| farm_barn | Curral/celeiro | 10x8 | 320x256 | sim | animais grandes |
| farm_greenhouse_small | Estufa inicial | 10x8 | 320x256 | sim | plantio fora de estação |
| farm_greenhouse_large | Estufa avançada | 16x12 | 512x384 | sim | endgame agrícola |
| farm_companion_board | Quadro de jobs | 2x2 | 64x64 | sim | companion automation |
| farm_well_common | Poço comum | 3x3 | 96x96 | sim/limitado | água comum, não Anya |
| farm_altar_generic | Altar permitido | 3x3 | 96x96 | sim | para deuses permitidos, exceto Anya |
| farm_fountain_anya | Fonte de Anya | 6x6 | 192x192 | não | fixa, lore central |
| farm_mana_root | Raiz Dormente de Mana | 5x5 ou evento | 160x160 | não | endgame, fixa após descoberta |
| farm_quarry_final | Pedreira final | 16x12 | 512x384 | não | último nível da fazenda |
| farm_irrigation_pump | Bomba/canal de irrigação | 3x3 | 96x96 | sim | automação de água |
| farm_sprinkler_basic | Aspersor básico | 1x1 | 32x32 | sim | raio definido em spec |
| farm_sprinkler_advanced | Aspersor avançado | 1x1 | 32x32 | sim | raio maior |

## 11. Construções fixas de lore

### Fonte de Anya

```text
Footprint: 6x6 tiles / 192x192 px
Move: não
Rotate: não
CanBuildDuplicate: não
```

Regras:

- é a única representação física de Anya na fazenda;
- não é altar genérico;
- não é estátua decorativa;
- não pode ser construída novamente;
- pode evoluir por sistema próprio;
- pode interagir com Água Viva, respec, ressurreição e lore.

### Raiz Dormente de Mana

```text
Footprint sugerido: 5x5 tiles / 160x160 px
Move: não
Rotate: não
Disponibilidade: endgame
```

Regras:

- não é crop plantável comum;
- surge por evento/condição rara;
- conecta Mana, Anya, Água Viva e solo arcano;
- deve ter risco narrativo/econômico.

### Pedreira final

```text
Footprint sugerido: 16x12 tiles / 512x384 px
Move: não
Disponibilidade: nível 5 da fazenda
```

Regras:

- recurso mineral tardio;
- não substitui mineração principal da caverna;
- fornece recurso limitado ou de suporte;
- desbloqueada apenas no último nível da fazenda.

---

# PARTE E — Props, crops, animais e elementos menores

## 12. Props e decoração

| Elemento | Tiles | Pixels | Observação |
|---|---:|---:|---|
| Crop plot | 1x1 | 32x32 | unidade básica agrícola |
| Crop pequeno | 1x1 | 32x32 | visual não deve exceder muito o tile |
| Crop alto | 1x2 visual | 32x64 | collider continua 1x1 |
| Árvore jovem | 1x2 | 32x64 | pomar/crescimento |
| Árvore adulta | 2x3 | 64x96 | madeira/frutas |
| Árvore grande | 3x4 | 96x128 | borda/bosque |
| Pedra pequena | 1x1 | 32x32 | removível |
| Pedra média | 2x2 | 64x64 | removível com ferramenta melhor |
| Rocha grande | 3x3 | 96x96 | bloqueio/expansão |
| Tronco pequeno | 1x1 | 32x32 | removível |
| Tronco grande | 2x2 | 64x64 | ferramenta melhor |
| Cerca | 1x1 por segmento | 32x32 | modular |
| Portão de cerca | 2x1 | 64x32 | abre/fecha |
| Caminho/piso | 1x1 | 32x32 | decorativo/organização |
| Lâmpada/poste | 1x2 | 32x64 | noite |
| Placa | 1x1 | 32x32 | informação |
| Banco | 2x1 | 64x32 | descanso/decoração |
| Barril | 1x1 | 32x32 | decoração/storage futuro |
| Caixote | 1x1 | 32x32 | decoração/storage futuro |
| Espantalho | 1x2 | 32x64 | proteção/visual |
| Composteira | 2x2 | 64x64 | fertilizante |
| Maker/processador pequeno | 2x2 | 64x64 | produção simples |
| Maker/processador grande | 3x2 | 96x64 | produção avançada |

## 13. Animais

| Animal | Sprite visual | Collider | Espaço recomendado |
|---|---:|---:|---:|
| Galinha/pato | 32x32 | 18x12 | 1x1 a 2x2 |
| Coelho/futuro | 32x32 | 18x12 | 1x1 |
| Gato | 24x24 | 14x10 | livre/pet area |
| Cachorro | 32x32 | 18x12 | livre/pet area |
| Ovelha/cabra | 48x40 | 28x16 | 2x2 |
| Vaca | 64x48 | 36x20 | 2x2 a 3x2 |
| Cavalo/montaria futura | 64x48 ou 64x64 | 36x20 | 3x2 |

Regra:

```text
Animais usam collider de base, não sprite inteiro.
Animais grandes precisam de área de navegação maior.
```

---

# PARTE F — Interiores da fazenda

## 14. Interiores principais

| Interior | Tamanho tiles | Tamanho px | Função |
|---|---:|---:|---|
| Casa inicial | 16x12 | 512x384 | cama, salvar, baú, calendário |
| Casa expandida | 22x16 | 704x512 | cozinha, storage, decoração |
| Galinheiro | 14x10 | 448x320 | animais pequenos, ninhos, ração |
| Curral/celeiro | 18x14 | 576x448 | animais grandes, ração, produtos |
| Estufa inicial | 16x12 | 512x384 | plantio fora de estação |
| Estufa avançada | 24x18 | 768x576 | plantio avançado |
| Workshop pequeno | 14x10 | 448x320 | makers/processadores |
| Workshop grande | 22x16 | 704x512 | produção avançada |
| Depósito | 16x12 | 512x384 | baús/storage por categoria |

## 15. Cama do jogador

```text
Cama simples inicial: 2x2 tiles / 64x64 px
Cama avançada/casal: 3x2 tiles / 96x64 px
```

Regras:

- cama do jogador permite dormir;
- dormir recupera cansaço/stamina conforme regras de PlayerCondition/Fatigue;
- casamento pode trocar ou ampliar cama;
- cônjuge pode ter rotina híbrida definida em spec de casamento.

---

# PARTE G — Construção, colisão e pathfinding

## 16. Modo construção

O modo construção deve usar grid 32x32.

Funções:

- mostrar grid;
- mostrar footprint em tiles;
- mostrar área de entrada obrigatória;
- validar terreno;
- validar colisão;
- validar distância de marcos fixos;
- mover construção;
- demolir/remover;
- rotacionar quando fizer sentido;
- preservar save.

Estados:

```text
ValidPlacement
BlockedByObject
BlockedByTerrain
BlockedByZone
BlockedByPath
BlockedByLoreAnchor
InsufficientResources
RequiresUpgrade
```

## 17. Colisão

Colisores obrigatórios:

- casa;
- Fonte de Anya;
- lago;
- entrada da caverna;
- SellPoint;
- construções;
- cercas;
- baús;
- makers;
- árvores adultas;
- rochas médias/grandes;
- pedreira final;
- Raiz Dormente de Mana;
- bordas naturais;
- cliffs;
- água.

Objetos com colisão parcial ou nenhuma:

- crops baixos;
- flores;
- grama;
- caminhos;
- crops altos com collider apenas no tile de base, se necessário;
- decoração pequena, conforme spec.

## 18. Pathfinding e acesso

Regras:

```text
Toda construção precisa de pelo menos 1 tile livre de acesso na entrada.
Caminhos principais recomendados: 2 a 3 tiles de largura.
NPC visitante deve conseguir alcançar porta/campo/evento por waypoints.
Companion de fazenda deve conseguir alcançar job area.
Pet deve poder alcançar cama/tigela/área de descanso.
```

O pathfinding da fazenda deve ser compatível com:

- player;
- pet separado de companion;
- NPC visitante;
- companion worker;
- animais em área delimitada.

---

# PARTE H — Roadmap de implementação da escala/layout

## 19. Roadmap 0 — Reconciliar base existente

- confirmar tamanho real do sprite do player no Unity;
- confirmar PPU/import settings;
- confirmar se tilemap atual usa 16x16, 32x32 ou escala visual diferente;
- mapear scripts existentes de farm plot, interação, construção, save e colisão;
- não reimplementar farm loop existente sem auditoria.

## 20. Roadmap 1 — Escala base e grid

- definir tilemap lógico 32x32;
- definir player 32x48;
- aplicar footbox collider;
- validar sorting Y;
- validar câmera;
- documentar prefab base de player/NPC/pet.

## 21. Roadmap 2 — Fazenda nível 1 em escala correta

- criar/ajustar área 40x32 tiles;
- posicionar casa, campo inicial, lago, bosque, SellPoint, Fonte, entrada da caverna e saída para cidade;
- garantir colisões e pontos de interação;
- validar navegação com player.

## 22. Roadmap 3 — Construções móveis

- footprints em grid 32x32;
- modo construção;
- preview de footprint;
- validação de terreno;
- mover casa/SellPoint/construções secundárias;
- preservar save.

## 23. Roadmap 4 — Animais, pets, workshops e visitantes

- galinheiro/curral;
- área de pet;
- workshops;
- storage;
- NPCs visitando a fazenda;
- pathfinding para visitantes/companions/pets.

## 24. Roadmap 5 — Endgame fixo

- área arcana da Fonte;
- Raiz Dormente de Mana;
- pedreira final;
- tecnologia bromeciana agrícola;
- garantir que marcos fixos não sejam movidos ou destruídos.

---

# PARTE I — Specs futuras derivadas

```text
spec_farm_scale_tilemap_player_footbox.md
spec_farm_level1_layout_fixed_anchors.md
spec_farm_building_footprints_placement_grid.md
spec_farm_move_house_sellpoint_safe_save.md
spec_farm_pet_area_animals_navigation.md
spec_farm_workshops_storage_footprints.md
spec_farm_fountain_anya_fixed_anchor.md
spec_farm_endgame_quarry_mana_root_anchors.md
```

## Fontes obrigatórias para essas specs

```text
docs/design/SPEC_SOURCE_MAP.md
docs/design/SPECIFICATION_PROCESS.md
docs/design/lore/VAALARA_GAME_CANON_DIRECTION_v1.0.md
docs/design/gameplay/farm/FARM_DESIGN_DIRECTION_v1.3.md
docs/design/gameplay/farm/FARM_LAYOUT_SCALE_BUILDINGS_DIRECTION.md
```

Se a spec tocar cidade/NPCs visitantes:

```text
docs/design/gameplay/city/CITY_DESIGN_DIRECTION_v1.2.md
docs/design/gameplay/city/CITY_NPC_ROSTER_SERVICES_DIRECTION_v1.1.md
docs/design/gameplay/city/CITY_LAYOUT_BUILDINGS_SCHEDULE_DIRECTION.md
```

---

# PARTE J — Decisões fechadas

```text
A fazenda usa a mesma escala visual da cidade.
Tile base: 32x32 px.
Player visual: 32x48 px.
NPC comum visual: 32x48 px.
Player/NPC/animals usam collider de base/footbox, não sprite inteiro.
Fazenda nível 1: 40x32 tiles / 1280x1024 px.
Fazenda nível 5: 104x72 tiles / 3328x2304 px.
Fonte de Anya é fixa e tem footprint sugerido de 6x6 tiles / 192x192 px.
SellPoint é móvel após desbloqueio e tem footprint de 2x2 tiles / 64x64 px.
Casa começa fixa, mas pode mover após desbloqueio.
Pedreira final só aparece no nível 5 e é fixa.
Raiz Dormente de Mana é endgame, fixa após descoberta e não é crop comum.
```

---

# PARTE K — Pendências

- Confirmar no Unity o tamanho real do sprite do player.
- Confirmar PPU/import settings.
- Confirmar tile size real dos tilemaps existentes.
- Confirmar se assets atuais de crops/props foram desenhados para 16x16 ou 32x32.
- Ajustar specs futuras se o Unity já estiver usando outra escala técnica.
- Definir se interiores da fazenda serão cenas separadas ou subáreas.
- Definir tamanho visual final da casa em sprites/art.
- Definir footprint final de cada construção após protótipo visual.
