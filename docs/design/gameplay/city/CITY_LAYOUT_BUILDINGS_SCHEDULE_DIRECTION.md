# Cindar's Hope — City Layout, Buildings & Schedule Direction

> **Status:** documento canônico de layout, construções, interiores, camas, física, movimento e agendas da cidade  
> **Local:** `docs/design/gameplay/city/CITY_LAYOUT_BUILDINGS_SCHEDULE_DIRECTION.md`  
> **Depende de:**  
> - `docs/design/lore/VAALARA_GAME_CANON_DIRECTION_v1.0.md`  
> - `docs/design/gameplay/city/CITY_DESIGN_DIRECTION_v1.2.md`  
> - `docs/design/gameplay/city/CITY_NPC_ROSTER_SERVICES_DIRECTION_v1.1.md`  
> - `docs/design/gameplay/farm/FARM_DESIGN_DIRECTION_v1.3.md`  
> **Função:** definir a cidade como espaço jogável: mapa, zonas, prédios, interiores, moradores, camas, rotinas, colisão, pathfinding, cenas, props e roadmap.  
> **Não é spec implementável.** Specs futuras devem ser quebradas em `docs/specs/a_implementar/`.

---

## 0. Regra de uso deste documento

Toda spec de cidade que envolva mapa, cena, NPC, loja, rotina, pathfinding, residência, cama, interior, horário, festival ou visita à fazenda deve ler este documento antes de propor implementação.

Este documento responde:

```text
Onde fica cada coisa?
Quem mora onde?
Onde cada NPC trabalha?
Onde cada NPC dorme?
Como os NPCs se movem?
Como a cidade escala em tiles/pixels?
Quais são os prédios e seus interiores?
Quais props existem na cena?
Como a cidade muda por hora, clima, lua e festival?
Qual é o roadmap de construção da cidade?
```

---

# PARTE A — Escala e premissas técnicas

## 1. Tile, sprite e câmera

Direção recomendada:

```text
Tile base: 16x16 px
Personagem comum: 16x24 px ou 16x32 px
NPC grande: 16x32 px ou 24x32 px
Porta comum: 16x24 px
Porta larga: 32x24 px
Cama simples: 32x32 px
Cama casal: 48x32 px
Balcão: 16 px de profundidade visual, collider em grid
```

Câmera recomendada:

```text
Visão padrão: 20x12 a 24x14 tiles
Scroll suave seguindo jogador
Cidade maior que uma tela
Sem teleport interno dentro do mapa externo, salvo portas/interiores
```

## 2. Tamanho da cidade externa

Tamanho canônico recomendado:

```text
Cidade externa: 192x144 tiles
Tamanho em pixels: 3072x2304 px
```

Motivo:

- grande o bastante para conter mercado, templo, taverna, ofícios, guilda, casas, jardim e entrada da caverna;
- pequena o bastante para o jogador memorizar;
- permite festivais na praça sem trocar toda a cena;
- permite zonas noturnas e ruína discreta sem inflar escopo.

## 3. Sistema de coordenadas

Usar coordenadas em tiles com origem no canto inferior esquerdo da cidade externa:

```text
X: 0 → 191
Y: 0 → 143
Origem: sudoeste / canto inferior esquerdo
```

Coordenadas são direção de design, não contrato final de Unity.

---

# PARTE B — Macro layout

## 4. Zonas externas

| Zona | Coordenada aproximada | Tamanho | Função |
|---|---:|---:|---|
| Praça Central | x72 y62 | 48x36 | centro social, calendário, festivais, quadro público |
| Mercado/Rua Comercial | x24 y72 | 48x32 | loja geral, sementes, animais, barracas |
| Distrito de Ofícios | x24 y28 | 52x36 | ferreiro, carpintaria, costura, alquimia |
| Taverna/Estalagem | x82 y32 | 36x28 | comida, descanso, rumores, quadro de pedidos |
| Templo de Kanthor | x128 y74 | 40x34 | ordem, juramentos, cerimônias, proteção civil |
| Jardim das Estátuas Antigas | x136 y42 | 34x28 | vestígios de Anya/Cindar, eventos de Alihana/Nyx |
| Prefeitura/Cartório | x80 y104 | 36x26 | licenças, impostos, contratos, reputação |
| Guilda das Estradas | x24 y108 | 38x24 | mapas, caverna, caravanas, contratos de exploração |
| Estrada/Caravançará | x0 y112 | 26x28 | entrada de viajantes, mercadores de Finan |
| Caminho da Fazenda | x72 y0 | 36x20 | saída para fazenda |
| Entrada da Caverna | x148 y10 | 36x28 | advertências, acesso à caverna, patrulhas |
| Beco/Loja Noturna | x154 y100 | 24x24 | Yael, Nyx, segredos |
| Ruína Discreta/Poço | x160 y62 | 22x20 | subsolo, Bromécia/Elyndor, cultos |
| Residências Norte | x112 y112 | 42x24 | casas de NPCs não comerciantes |
| Residências Sul | x116 y10 | 30x24 | casas simples, passagem para caverna |

## 5. Diagrama macro

```text
NORTE
┌──────────────────────────────────────────────────────────────┐
│ Estrada/Caravançará  Guilda        Cartório      Casas Norte │
│ Mercado/Rua Comercial     Praça Central       Templo Kanthor │
│ Ofícios/Workshops         Taverna        Estátuas / Ruína    │
│ Caminho Fazenda                      Casas Sul  Caverna      │
└──────────────────────────────────────────────────────────────┘
SUL
```

## 6. Regras de navegação

- Mercado, praça, taverna e ofícios devem ser próximos.
- Templo de Kanthor deve ser visível e institucional.
- Jardim das Estátuas deve ser acessível, mas menos movimentado.
- Beco/Loja Noturna não deve parecer loja comum de dia.
- Entrada da caverna deve ficar na borda, não no centro.
- Caminho da fazenda deve ser claro e rápido.
- Guilda deve ficar perto da estrada e da rota para caverna.

---

# PARTE C — Construções externas e interiores

## 7. Lista canônica de construções

| ID | Construção | Externo | Interno | Donos/residentes | Serviço |
|---|---|---:|---:|---|---|
| bld_kanthor_temple | Templo de Kanthor | 28x22 | 34x28 | Corvus | bênçãos, juramentos, lei |
| bld_town_hall | Prefeitura/Cartório | 28x20 | 34x24 | Mara, Tovin | licenças, contratos, reputação |
| bld_seed_shop | Loja de Sementes | 22x18 | 26x20 | Sylveth | sementes, fertilizante, calendário agrícola |
| bld_general_store | Loja Geral | 24x18 | 28x22 | Renko | itens comuns, compra/venda |
| bld_blacksmith | Forja | 24x20 | 30x24 | Brumdar | ferramentas, armas, reparo |
| bld_carpentry | Carpintaria | 28x22 | 34x26 | Nimble, Gurd, Hund | construções, mover estrutura |
| bld_alchemy | Alquimia | 22x18 | 28x22 | Ozzra | poções, fertilizantes, reagentes |
| bld_tavern_inn | Taverna Panela-Funda | 32x24 | 42x30 | Gruta, Orlan | comida, rumor, hospedagem |
| bld_roads_guild | Guilda das Estradas | 30x20 | 34x24 | Zrix, Dagna parcial | mapas, caverna, contratos |
| bld_archive | Arquivo/Biblioteca | 24x20 | 32x26 | Thalindra | pesquisa, lore, tradução |
| bld_tailor | Ateliê de Costura | 22x18 | 26x22 | Mirela | bolsas, roupas, acessórios |
| bld_ranch | Rancho/Animais | 30x22 | 34x24 | Eiran | animais, pets, ração |
| bld_herbalist | Cabana de Ervas | 22x18 | 26x22 | Savra | ervas, antídotos, pragas |
| bld_night_shop | Loja Noturna | 18x16 | 22x18 | Yael | itens raros, Nyx, segredos |
| bld_liora_house | Casa de Liora | 18x16 | 22x18 | Liora | residência/eventos de música |
| bld_maelor_hideout | Esconderijo de Maelor | oculto | 18x16 | Maelor | segredo, Nyx, memória |
| bld_statue_garden | Jardim das Estátuas | 34x28 | externo | âncora de lore | Anya/Cindar, não culto ativo |
| bld_cave_gate | Entrada da Caverna | 36x28 | externo | guarda/guilda | acesso à caverna |

## 8. Templo de Kanthor

```text
Externo: 28x22 tiles
Interno: 34x28 tiles
Local: nordeste da praça
Morador: Padre Corvus
Cama: cama simples no aposento lateral dos fundos
```

Componentes:

- nave central;
- altar de Kanthor;
- sino externo;
- sala de juramentos;
- aposento de Corvus;
- pequeno arquivo religioso;
- porta lateral trancada para subsolo futuro.

Interações:

- bênção de Kanthor;
- juramento/contrato;
- diálogo com Corvus;
- eventos de ordem;
- quest de inscrição sob pedra.

Regra:

```text
O templo é de Kanthor.
Anya não tem altar ativo aqui.
Qualquer vestígio de Anya fica fora do culto público, no Jardim das Estátuas ou em subsolo/lore.
```

## 9. Jardim das Estátuas Antigas

```text
Tipo: área externa
Tamanho: 34x28 tiles
Local: leste/sudeste do templo
Morador: nenhum
Camas: nenhuma
```

Componentes:

- 3 a 5 estátuas gastas;
- uma delas associada a Anya, mas sem identificação pública clara;
- musgo, água parada, pedra clara, flores antigas;
- banco quebrado;
- pedestal apagado;
- trigger de Alihana;
- trigger de Nyx;
- entrada visualmente bloqueada para passagem antiga futura.

Regras:

- é na cidade, não na fazenda;
- não é altar ativo;
- não vende bênção;
- não substitui a Fonte da fazenda;
- reage lentamente à progressão de lore.

## 10. Taverna Panela-Funda

```text
Externo: 32x24 tiles
Interno: 42x30 tiles
Moradores: Gruta e Orlan
Cama: cama casal nos fundos
Camas extras: 2 camas de hóspedes
```

Componentes:

- balcão;
- cozinha;
- palco pequeno;
- 6 mesas;
- quadro de pedidos secundário;
- escada/porta para quartos;
- barris e cozinha;
- lareira;
- mesa de rumores.

Interações:

- comprar comida;
- ouvir rumores;
- aceitar pedidos;
- eventos de Gruta/Liora/Orlan;
- encontro social noturno.

## 11. Prefeitura/Cartório

```text
Externo: 28x20 tiles
Interno: 34x24 tiles
Moradores: Mara e Tovin em casa anexa
Cama: cama casal em cômodo residencial lateral
```

Componentes:

- balcão de registros;
- mesa de Mara;
- mesa de Tovin;
- armário de documentos;
- mural de licenças;
- sala trancada de arquivo civil;
- acesso futuro a registros antigos.

Interações:

- licenças de construção;
- contratos;
- impostos;
- reputação da cidade;
- autorização de altares permitidos;
- bloqueio formal de altar/estátua de Anya como construção livre.

## 12. Distrito de Ofícios

Inclui:

- Forja de Brumdar;
- Carpintaria de Nimble/Gurd/Hund;
- Ateliê de Mirela;
- Laboratório de Ozzra.

Regras:

- deve ser visualmente produtivo: fumaça, madeira, caixas, ferramentas;
- deve ter colisores fortes em bancadas;
- deve ter rotas largas para NPCs carregando material;
- deve conectar diretamente ao caminho da fazenda.

## 13. Residências e camas

| NPC | Residência | Cama |
|---|---|---|
| Corvus | aposento no Templo de Kanthor | cama simples `bed_corvus` |
| Mara | casa anexa ao Cartório | cama casal `bed_mara_tovin` |
| Tovin | casa anexa ao Cartório | cama casal `bed_mara_tovin` |
| Sylveth | casa atrás da Loja de Sementes | cama simples `bed_sylveth` |
| Brumdar | quarto anexo à Forja | cama simples robusta `bed_brumdar` |
| Nimble | casa-oficina da Carpintaria | cama casal `bed_nimble_mirela` |
| Mirela | casa-oficina da Carpintaria/Ateliê | cama casal `bed_nimble_mirela` |
| Gurd | alojamento da Carpintaria | cama simples `bed_gurd` |
| Hund | alojamento da Carpintaria | cama simples `bed_hund` |
| Ozzra | laboratório de Alquimia | cama improvisada `bed_ozzra` |
| Gruta | Taverna/Estalagem | cama casal `bed_gruta_orlan` |
| Orlan | Taverna/Estalagem | cama casal `bed_gruta_orlan` |
| Zrix | Guilda das Estradas ou hospedaria | cama simples `bed_zrix_guild` |
| Yael | Loja Noturna/aposento oculto | cama oculta `bed_yael` |
| Thalindra | Arquivo/Biblioteca | cama simples `bed_thalindra` |
| Dagna | alojamento da Guilda/Forja | cama simples `bed_dagna` |
| Pip | casa familiar fora de cena ou quarto de recados | cama não simulada inicialmente |
| Alaric | posto de guarda / quarto do capitão | cama simples `bed_alaric` |
| Renko | piso superior da Loja Geral | cama simples `bed_renko` |
| Eiran | Rancho | cama simples `bed_eiran` |
| Liora | Casa de Liora | cama simples `bed_liora` |
| Savra | Cabana de Ervas | cama simples `bed_savra` |
| Maelor | esconderijo oculto | cama/esteira `bed_maelor_hidden` |

Regra técnica:

```text
Camas de NPC são objetos de schedule.
Jogador não usa camas de NPC, exceto camas de hóspedes da estalagem se sistema de hospedagem for implementado.
```

---

# PARTE D — Agenda diária canônica

## 14. Períodos do dia

```text
06:00–09:00 Morning
09:00–12:00 WorkStart
12:00–14:00 Midday
14:00–18:00 WorkAfternoon
18:00–21:00 Evening
21:00–00:00 Night
00:00–06:00 Sleep/LateNight
```

## 15. Agenda padrão por NPC

| NPC | Morning | WorkStart | Midday | WorkAfternoon | Evening | Night/Sleep |
|---|---|---|---|---|---|---|
| Corvus | templo | templo | praça/templo | templo | templo/praça | aposento templo |
| Mara | casa/cartório | cartório | praça/cartório | cartório | casa/cartório | cama Mara/Tovin |
| Tovin | casa/cartório | cartório | cartório | cartório | casa | cama Mara/Tovin |
| Sylveth | casa/horta | loja sementes | horta | loja | altar Thandra/praça | casa |
| Brumdar | forja | forja | taverna rápida | forja | forja/casa | quarto forja |
| Nimble | carpintaria | carpintaria | obra externa | carpintaria | casa/Mirela | cama casal |
| Mirela | ateliê | ateliê | mercado | ateliê | casa/Nimble | cama casal |
| Gurd | alojamento | obra | taverna | obra/carpintaria | taverna | alojamento |
| Hund | alojamento | obra/guarda | praça | obra | patrulha curta | alojamento |
| Ozzra | laboratório | laboratório | mercado/ervas | laboratório | laboratório/taverna | cama improvisada |
| Gruta | cozinha | taverna | taverna | taverna | taverna cheia | cama casal |
| Orlan | estalagem | estalagem | cartório/mercado | estalagem | taverna | cama casal |
| Zrix | guilda | guilda/entrada caverna | estrada | guilda | taverna/guilda | cama guilda |
| Yael | dorme/oculta | oculta | oculta | beco | loja noturna | loja/oculta |
| Thalindra | arquivo | arquivo | jardim/arquivo | arquivo | arquivo/casa | cama arquivo |
| Dagna | guilda/forja | guilda/minério | forja | guilda/caverna | taverna/forja | cama guilda |
| Pip | praça | entregas | mercado | entregas | taverna/praça | fora de cena |
| Alaric | guarda | patrulha | templo/praça | entrada caverna | guarda | posto guarda |
| Renko | loja geral | loja geral | mercado | loja geral | loja/contas | piso superior |
| Eiran | rancho | rancho | mercado animal | rancho | animais | cama rancho |
| Liora | casa/praça | praça | taverna/jardim | praça/taverna | taverna/música | casa |
| Savra | cabana | ervas/floresta | mercado | cabana | trilha/cabana | cama cabana |
| Maelor | oculto | oculto | oculto | jardim distante | beco/jardim | esconderijo |

## 16. Modificadores de agenda

### Chuva

- Sylveth fica mais tempo na loja.
- Eiran fica no rancho.
- Pip reduz entregas externas.
- Yael pode abrir mais cedo se for noite de Nyx.
- Maelor aparece menos em praça e mais perto do Jardim.

### Alihana

- Liora vai ao Jardim das Estátuas no Evening.
- Thalindra pode ir ao Jardim à noite.
- Estátuas podem ter trigger visual sutil.
- Sylveth pode comentar sementes raras.

### Senya

- Taverna fica mais cheia.
- Gruta tem evento de comida/festa.
- Ozzra pode vender item instável.
- Gurd fica mais propenso a evento de conflito.

### Nyx

- Yael abre loja noturna.
- Maelor aparece em rotas visíveis ao jogador.
- Menos NPCs comuns na rua.
- Jardim das Estátuas pode ter evento estranho.
- Alaric patrulha mais.

### Festival

- Agenda normal é suspensa.
- NPCs vão para Praça Central ou área temática.
- Lojas normais fecham, mas barracas especiais abrem.

---

# PARTE E — Movimento, física e pathfinding

## 17. Movimento do jogador

Direção recomendada:

```text
Movimento livre suave sobre tilemap.
Colisão baseada em colliders/tile collision.
Interação por proximidade + direção do jogador.
```

Não usar movimento preso rigidamente tile-a-tile para o jogador, salvo se o projeto decidir por estética/escopo.

## 18. Movimento de NPCs

Direção recomendada:

```text
NPCs usam waypoints por agenda.
Quando visíveis, caminham entre pontos com movimento suave.
Quando fora de tela ou em outra cena, podem trocar estado por schedule tick.
```

Regras:

- NPC não precisa simulação contínua offscreen;
- schedule pode teleportar NPC fora de câmera/interior;
- quando player entra na cena, NPC aparece no waypoint correto conforme hora/estado;
- NPC em rota visível deve andar até destino;
- se caminho estiver bloqueado por evento, usa fallback waypoint.

## 19. Colisão

Colisores obrigatórios:

- paredes externas;
- paredes internas;
- balcões;
- mesas;
- camas;
- estátuas;
- fonte pública;
- poço lacrado;
- água;
- cliffs/limites;
- barris/caixotes grandes;
- forge/anvil;
- bancadas;
- portas trancadas;
- entrada da caverna enquanto bloqueada.

Objetos com colisão parcial:

- flores;
- grama;
- tapetes;
- placas pequenas;
- bancos, se interativos;
- barracas de festival.

## 20. Portas e cenas internas

```text
DoorTrigger
  TargetSceneId
  TargetSpawnPointId
  RequiredState
  LockedMessage
  OpenHoursRule optional
```

Regras:

- porta de loja fechada mostra horário;
- porta de casa privada pode bloquear entrada até relação/quest;
- estalagem permite entrada mais ampla;
- loja noturna só ativa em condições específicas;
- interiores podem ser cenas separadas ou subáreas carregadas, a definir em spec.

## 21. Camas

```text
BedId
OwnerNpcId[]
LocationId
BedType
ScheduleOnly
CanPlayerUse
```

Regras:

- cama de NPC é marcador de rotina;
- cama de casal aceita dois NPCs casados;
- jogador só usa cama de hóspedes da estalagem, se hospedagem existir;
- após casamento, cônjuge pode ter cama/rotina híbrida na fazenda, definida por spec de casamento.

---

# PARTE F — Props, componentes e pontos de interação

## 22. Componentes extras da cidade

| Componente | Local | Função |
|---|---|---|
| Quadro público | Praça | eventos, avisos, pedidos simples |
| Quadro de pedidos | Taverna/Guilda | contratos e encomendas |
| Calendário | Praça | festivais, aniversários, luas |
| Sino de Kanthor | Templo | festival, quest de Corvus |
| Estátuas antigas | Jardim | lore Anya/Cindar |
| Poço lacrado | Ruína discreta | acesso/subsolo futuro |
| Fonte pública comum | Praça | decoração, não Anya |
| Placas de lojas | lojas | horário e nome |
| Barracas de festival | Praça/Mercado | eventos temporários |
| Carruagem/caravançará | Estrada | Finan/mercadores |
| Placa de perigo | Caverna | tutorial e alerta |
| Poste/lampiões | ruas | noite/Nyx |
| Bancos | praça/jardim | NPC idle/social |
| Caixotes/barris | mercado/ofícios | colisão/decoração |
| Anvil/forja | forja | interação de Brumdar |
| Bancada alquímica | alquimia | interação de Ozzra |
| Mesa de pesquisa | arquivo | Thalindra/lore |
| Tigelas/currais | rancho | Eiran/animais |

## 23. Pontos de spawn

| SpawnId | Uso |
|---|---|
| spawn_from_farm | jogador entra vindo da fazenda |
| spawn_to_farm | saída para fazenda |
| spawn_from_road | chegada de estrada/caravana |
| spawn_to_cave | saída para caverna |
| spawn_from_cave | retorno da caverna |
| spawn_festival_square | início de festival |
| spawn_temple | eventos de Kanthor |
| spawn_night_shop | eventos de Nyx/Yael |

---

# PARTE G — Roadmap de construção da cidade

## 24. Roadmap conceitual

Seguir o mesmo modelo da fazenda: roadmap por ondas funcionais, sem limitar a MVP.

```text
Roadmap 0 — Reconciliar base existente
Roadmap 1 — Cidade navegável essencial
Roadmap 2 — Serviços, lojas e interiores essenciais
Roadmap 3 — NPC schedules, casas e camas
Roadmap 4 — Reputação, visitas à fazenda e quests pessoais
Roadmap 5 — Festivais, luas e religião pública
Roadmap 6 — Segredos, subsolo, Bromécia/Elyndor e Anya
```

## 25. Roadmap 0 — Reconciliar base existente

Objetivo:

- verificar cenas existentes;
- verificar sistemas de interação já implementados;
- verificar sistema de shops, inventory, save, economy, time/day;
- mapear o que pode ser reaproveitado.

Entregas:

- inventário de cenas;
- lista de scripts existentes;
- lacunas de sistemas;
- spec de implementação segura.

## 26. Roadmap 1 — Cidade navegável essencial

Entregas:

- CityScene externa;
- tilemap base 192x144;
- zonas principais;
- colisores;
- portas placeholder;
- pontos de spawn;
- saída para fazenda;
- saída para caverna bloqueada/liberável;
- praça, mercado, templo, taverna, ofícios e guilda como formas externas.

## 27. Roadmap 2 — Serviços, lojas e interiores essenciais

Entregas:

- interiores principais;
- loja de sementes;
- loja geral;
- ferreiro;
- carpintaria;
- taverna;
- cartório;
- templo de Kanthor;
- Guilda das Estradas;
- horários de porta/loja;
- UI mínima de compra/venda/serviço.

## 28. Roadmap 3 — NPC schedules, casas e camas

Entregas:

- spawn de NPCs por hora;
- agenda padrão;
- camas como markers;
- casas/residências;
- NPC indo para trabalho/casa;
- NPCs fora de cena resolvidos por schedule tick;
- diálogo básico por horário.

## 29. Roadmap 4 — Reputação, visitas à fazenda e quests pessoais

Entregas:

- TownReputation;
- NpcRelationship;
- gatilhos de visita à fazenda;
- cartas/presentes simples;
- primeiras quests pessoais;
- romance flags;
- casamento ainda pode ficar para spec separada.

## 30. Roadmap 5 — Festivais, luas e religião pública

Entregas:

- calendário;
- festivais principais;
- templo de Kanthor funcional;
- altares permitidos;
- bloqueio explícito de altar/estátua de Anya;
- eventos de Alihana, Senya e Nyx;
- loja noturna de Yael.

## 31. Roadmap 6 — Segredos, subsolo, Bromécia/Elyndor e Anya

Entregas:

- Jardim das Estátuas com triggers;
- poço lacrado;
- pistas de Cindar/Anya;
- registros removidos;
- símbolos de Elyndor;
- porta/ruína bromeciana;
- conexão futura com caverna profunda;
- sem revelar tudo cedo.

---

# PARTE H — Specs futuras derivadas

## 32. Ordem recomendada

```text
spec_city_scene_tilemap_collision_spawns.md
spec_city_buildings_exteriors_and_doors.md
spec_city_core_interiors_shops_services.md
spec_city_npc_residences_beds_schedule_markers.md
spec_city_npc_pathfinding_waypoints.md
spec_city_props_interactables_calendar_boards.md
spec_city_kanthor_temple_statue_garden_no_anya_altar.md
spec_city_night_shop_nyx_behaviour.md
spec_city_festivals_layout_variations.md
spec_city_farm_visits_schedule_hooks.md
spec_city_hidden_subsoil_bromecia_elyndor_hooks.md
```

## 33. Fontes obrigatórias por spec

Toda spec acima deve ler:

```text
docs/design/lore/VAALARA_GAME_CANON_DIRECTION_v1.0.md
docs/design/gameplay/city/CITY_DESIGN_DIRECTION_v1.2.md
docs/design/gameplay/city/CITY_NPC_ROSTER_SERVICES_DIRECTION_v1.1.md
docs/design/gameplay/city/CITY_LAYOUT_BUILDINGS_SCHEDULE_DIRECTION.md
```

Specs que tocam fazenda também devem ler:

```text
docs/design/gameplay/farm/FARM_DESIGN_DIRECTION_v1.3.md
```

---

# PARTE I — Decisões fechadas

```text
Cidade externa recomendada: 192x144 tiles.
Tile base recomendado: 16x16 px.
Tamanho externo em pixels: 3072x2304 px.
Cidade usa movimento livre suave sobre tilemap.
NPCs usam waypoints e schedule tick, não simulação full offscreen.
Camas de NPC são marcadores de rotina.
Jogador não usa camas de NPC, exceto hospedaria se implementada.
Templo público é de Kanthor.
Anya não tem altar/estátua construível na fazenda.
Estátuas antigas de Anya ficam na cidade como vestígio, não como culto ativo.
Fonte da fazenda é a única representação física de Anya na fazenda.
Cada NPC tem residência/cama/local de trabalho definidos em direção.
```

---

# PARTE J — Pendências

- Validar se cidade externa 192x144 é aceitável para performance/arte.
- Definir se interiores são cenas separadas ou carregados no mesmo mapa.
- Definir pathfinding final: grid A*, waypoint fixo ou híbrido.
- Definir visual final das construções.
- Definir colisão por TilemapCollider2D, CompositeCollider2D ou colliders manuais.
- Definir se NPCs podem ser bloqueados fisicamente pelo jogador.
- Definir se lojas fechadas permitem entrada ou bloqueiam porta.
- Definir se festival altera a mesma cena ou carrega cena temporária.
- Definir como casamento altera residência/cama do cônjuge.
