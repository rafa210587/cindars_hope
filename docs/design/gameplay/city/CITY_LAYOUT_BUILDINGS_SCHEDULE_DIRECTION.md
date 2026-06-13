# Cindar's Hope — City Layout, Buildings & Schedule Direction

> **Status:** documento canônico de layout, construções, interiores, camas, física, movimento e agendas da cidade  
> **Local:** `docs/design/gameplay/city/CITY_LAYOUT_BUILDINGS_SCHEDULE_DIRECTION.md`  
> **Depende de:**  
> - `docs/design/lore/VAALARA_GAME_CANON_DIRECTION_v1.0.md`  
> - `docs/design/gameplay/city/CITY_DESIGN_DIRECTION_v1.2.md`  
> - `docs/design/gameplay/city/CITY_NPC_ROSTER_SERVICES_DIRECTION_v1.1.md`  
> - `docs/design/gameplay/farm/FARM_DESIGN_DIRECTION_v1.3.md`  
> **Função:** definir a cidade como espaço jogável: mapa, escala visual, zonas, prédios, interiores, moradores, camas, rotinas, colisão, pathfinding, cenas, props e roadmap.  
> **Não é spec implementável.** Specs futuras devem ser quebradas em `.specs/a_implementar/`.

---

## 0. Regra de uso deste documento

Toda spec de cidade que envolva mapa, cena, NPC, loja, rotina, pathfinding, residência, cama, interior, horário, festival ou visita à fazenda deve ler este documento antes de propor implementação.

Este documento responde:

```text
Qual é a escala visual da cidade?
Qual é o tamanho do player e dos NPCs?
Qual é o tamanho dos prédios e props?
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

# PARTE A — Escala visual e premissas técnicas

## 1. Decisão de escala

A escala visual deve ser baseada no personagem do jogador com sprite aproximado de:

```text
Player sprite: 32x48 px
```

NPCs devem ter escala semelhante ao jogador para manter leitura consistente.

Decisão canônica:

```text
Tile base: 32x32 px
Player visual: 32x48 px
NPC comum visual: 32x48 px
Collider de movimento: footbox inferior, não sprite inteiro
```

Motivo:

- player 32x48 fica proporcional em tile 32x32;
- o personagem ocupa 1 tile de largura e 1,5 tile de altura visual;
- permite casas, portas e props em escala legível;
- evita cidade visualmente minúscula por usar tile 16x16 com personagem grande.

## 2. Tamanhos de personagens

| Tipo | Sprite visual | Collider recomendado | Observação |
|---|---:|---:|---|
| Player | 32x48 px | 20x12 a 24x16 px nos pés | referência principal |
| NPC humano/elfo/tiefling médio | 32x48 px | 20x12 a 24x16 px | similar ao player |
| NPC halfling/goblin | 28x40 px ou 32x40 px | 18x10 a 22x14 px | menor, mas ainda compatível com grid |
| NPC anão | 32x44 px | 22x14 px | baixo e largo |
| NPC orc/meio-orc/draconato | 36x52 px a 40x56 px | 24x16 px | maior visualmente, sem quebrar pathfinding |
| Criança/jovem | 24x36 px a 28x40 px | 16x10 px | Pip e NPCs jovens |
| Pet gato | 24x24 px | 14x10 px | baixo, collider pequeno |
| Pet cachorro | 32x32 px | 18x12 px | pode seguir player |

Regra:

```text
O collider representa os pés/base do personagem.
A cabeça/corpo podem sobrepor visualmente objetos atrás, desde que sorting esteja correto.
```

## 3. Sorting e pivô

Direção técnica:

```text
Pivot visual do personagem: bottom-center
Sorting Y: ordenar por coordenada Y dos pés
Collider: centralizado na base inferior
Interação: cone/caixa curta à frente do player
```

Isso evita o erro de usar o sprite inteiro como colisão.

## 4. Câmera recomendada

Com tile 32x32 e player 32x48:

```text
Visão padrão: 20x12 tiles a 24x14 tiles
Equivalente em pixels: 640x384 px a 768x448 px antes de escala de tela
Scroll suave seguindo jogador
Cidade maior que uma tela
Sem teleport interno dentro do mapa externo, salvo portas/interiores
```

A resolução final da janela pode escalar isso em 2x/3x sem alterar o tamanho lógico da cidade.

---

# PARTE B — Tamanho da cidade

## 5. Tamanho externo canônico

Com tile base de 32x32 px, o tamanho recomendado da cidade muda para:

```text
Cidade externa: 128x96 tiles
Tamanho em pixels: 4096x3072 px
```

Motivo:

- mantém a cidade grande e navegável sem virar mapa gigantesco;
- comporta todos os prédios, praça, templo, jardim, guilda, caverna e residências;
- funciona melhor com personagem 32x48 px;
- permite pathfinding simples por zonas;
- mantém proporção boa para câmera de 20x12 ou 24x14 tiles.

## 6. Sistema de coordenadas

Usar coordenadas em tiles com origem no canto inferior esquerdo da cidade externa:

```text
X: 0 → 127
Y: 0 → 95
Origem: sudoeste / canto inferior esquerdo
Tile: 32x32 px
```

Coordenadas são direção de design, não contrato final de Unity.

---

# PARTE C — Macro layout

## 7. Zonas externas

| Zona | Coordenada aproximada | Tamanho em tiles | Tamanho em px | Função |
|---|---:|---:|---:|---|
| Praça Central | x48 y40 | 32x24 | 1024x768 | centro social, calendário, festivais, quadro público |
| Mercado/Rua Comercial | x12 y48 | 30x22 | 960x704 | loja geral, sementes, animais, barracas |
| Distrito de Ofícios | x14 y18 | 32x24 | 1024x768 | ferreiro, carpintaria, costura, alquimia |
| Taverna/Estalagem | x54 y18 | 24x18 | 768x576 | comida, descanso, rumores, quadro de pedidos |
| Templo de Kanthor | x86 y50 | 28x24 | 896x768 | ordem, juramentos, cerimônias, proteção civil |
| Jardim das Estátuas Antigas | x90 y28 | 26x20 | 832x640 | vestígios de Anya/Cindar, eventos de Alihana/Nyx |
| Prefeitura/Cartório | x50 y70 | 24x18 | 768x576 | licenças, impostos, contratos, reputação |
| Guilda das Estradas | x14 y72 | 28x18 | 896x576 | mapas, caverna, caravanas, contratos de exploração |
| Estrada/Caravançará | x0 y76 | 18x20 | 576x640 | entrada de viajantes, mercadores de Finan |
| Caminho da Fazenda | x48 y0 | 32x14 | 1024x448 | saída para fazenda |
| Entrada da Caverna | x96 y4 | 24x20 | 768x640 | advertências, acesso à caverna, patrulhas |
| Beco/Loja Noturna | x104 y70 | 16x16 | 512x512 | Yael, Nyx, segredos |
| Ruína Discreta/Poço | x110 y44 | 14x14 | 448x448 | subsolo, Bromécia/Elyndor, cultos |
| Residências Norte | x78 y76 | 28x18 | 896x576 | casas de NPCs não comerciantes |
| Residências Sul | x80 y8 | 20x14 | 640x448 | casas simples, passagem para caverna |

## 8. Diagrama macro

```text
NORTE
┌──────────────────────────────────────────────────────────────┐
│ Estrada/Caravançará  Guilda       Cartório   Casas Norte     │
│ Mercado/Rua Comercial     Praça Central      Templo Kanthor  │
│ Ofícios/Workshops         Taverna      Estátuas / Ruína      │
│ Caminho Fazenda                         Casas Sul  Caverna   │
└──────────────────────────────────────────────────────────────┘
SUL
```

## 9. Regras de navegação

- Mercado, praça, taverna e ofícios devem ser próximos.
- Templo de Kanthor deve ser visível e institucional.
- Jardim das Estátuas deve ser acessível, mas menos movimentado.
- Beco/Loja Noturna não deve parecer loja comum de dia.
- Entrada da caverna deve ficar na borda, não no centro.
- Caminho da fazenda deve ser claro e rápido.
- Guilda deve ficar perto da estrada e da rota para caverna.

---

# PARTE D — Tamanhos de construções e interiores

## 10. Regra de proporção

Com player 32x48 px:

```text
Porta comum: 2x2 tiles = 64x64 px
Porta alta/templo: 3x3 tiles = 96x96 px
Janela comum: 1x1 tile = 32x32 px
Balcão mínimo: 3x1 tiles = 96x32 px
Cama simples: 2x2 tiles = 64x64 px
Cama casal: 3x2 tiles = 96x64 px
Mesa comum: 2x2 tiles = 64x64 px
Mesa grande: 3x2 tiles = 96x64 px
Barraca de feira: 3x3 tiles = 96x96 px
Árvore média: 2x3 tiles = 64x96 px
Árvore grande: 3x4 tiles = 96x128 px
Estátua média: 2x3 tiles = 64x96 px
Estátua grande: 3x4 tiles = 96x128 px
```

## 11. Lista canônica de construções

| ID | Construção | Externo tiles | Externo px | Interno tiles | Interno px | Donos/residentes | Serviço |
|---|---|---:|---:|---:|---:|---|---|
| bld_kanthor_temple | Templo de Kanthor | 18x14 | 576x448 | 24x18 | 768x576 | Corvus | bênçãos, juramentos, lei |
| bld_town_hall | Prefeitura/Cartório | 16x12 | 512x384 | 22x16 | 704x512 | Mara, Tovin | licenças, contratos, reputação |
| bld_seed_shop | Loja de Sementes | 12x10 | 384x320 | 16x13 | 512x416 | Sylveth | sementes, fertilizante, calendário agrícola |
| bld_general_store | Loja Geral | 14x10 | 448x320 | 18x14 | 576x448 | Renko | itens comuns, compra/venda |
| bld_blacksmith | Forja | 14x12 | 448x384 | 18x15 | 576x480 | Brumdar | ferramentas, armas, reparo |
| bld_carpentry | Carpintaria | 18x12 | 576x384 | 22x16 | 704x512 | Nimble, Gurd, Hund | construções, mover estrutura |
| bld_alchemy | Alquimia | 12x10 | 384x320 | 17x14 | 544x448 | Ozzra | poções, fertilizantes, reagentes |
| bld_tavern_inn | Taverna Panela-Funda | 20x14 | 640x448 | 28x20 | 896x640 | Gruta, Orlan | comida, rumor, hospedagem |
| bld_roads_guild | Guilda das Estradas | 18x12 | 576x384 | 22x16 | 704x512 | Zrix, Dagna parcial | mapas, caverna, contratos |
| bld_archive | Arquivo/Biblioteca | 14x12 | 448x384 | 20x16 | 640x512 | Thalindra | pesquisa, lore, tradução |
| bld_tailor | Ateliê de Costura | 12x10 | 384x320 | 16x14 | 512x448 | Mirela | bolsas, roupas, acessórios |
| bld_ranch | Rancho/Animais | 20x14 | 640x448 | 22x16 | 704x512 | Eiran | animais, pets, ração |
| bld_herbalist | Cabana de Ervas | 12x10 | 384x320 | 16x14 | 512x448 | Savra | ervas, antídotos, pragas |
| bld_night_shop | Loja Noturna | 10x8 | 320x256 | 14x10 | 448x320 | Yael | itens raros, Nyx, segredos |
| bld_liora_house | Casa de Liora | 10x8 | 320x256 | 14x10 | 448x320 | Liora | residência/eventos de música |
| bld_maelor_hideout | Esconderijo de Maelor | oculto | oculto | 10x8 | 320x256 | Maelor | segredo, Nyx, memória |
| bld_statue_garden | Jardim das Estátuas | 26x20 | 832x640 | externo | externo | âncora de lore | Anya/Cindar, não culto ativo |
| bld_cave_gate | Entrada da Caverna | 24x20 | 768x640 | externo | externo | guarda/guilda | acesso à caverna |

## 12. Templo de Kanthor

```text
Externo: 18x14 tiles / 576x448 px
Interno: 24x18 tiles / 768x576 px
Local: nordeste da praça
Morador: Padre Corvus
Cama: cama simples no aposento lateral dos fundos
Porta principal: 3x3 tiles / 96x96 px
```

Componentes:

- nave central;
- altar de Kanthor;
- sino externo;
- sala de juramentos;
- aposento de Corvus;
- pequeno arquivo religioso;
- porta lateral trancada para subsolo futuro.

Regra:

```text
O templo é de Kanthor.
Anya não tem altar ativo aqui.
Qualquer vestígio de Anya fica fora do culto público, no Jardim das Estátuas ou em subsolo/lore.
```

## 13. Jardim das Estátuas Antigas

```text
Tipo: área externa
Tamanho: 26x20 tiles / 832x640 px
Local: leste/sudeste do templo
Morador: nenhum
Camas: nenhuma
```

Componentes:

- 3 a 5 estátuas gastas;
- uma delas associada a Anya, mas sem identificação pública clara;
- estátuas médias: 2x3 tiles / 64x96 px;
- estátua principal: 3x4 tiles / 96x128 px;
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

## 14. Taverna Panela-Funda

```text
Externo: 20x14 tiles / 640x448 px
Interno: 28x20 tiles / 896x640 px
Moradores: Gruta e Orlan
Cama: cama casal nos fundos
Camas extras: 2 camas de hóspedes
Porta: 2x2 tiles / 64x64 px
```

Componentes:

- balcão: 6x1 tiles / 192x32 px;
- cozinha: 8x5 tiles / 256x160 px;
- palco pequeno: 5x3 tiles / 160x96 px;
- 6 mesas de 2x2 tiles / 64x64 px;
- quadro de pedidos secundário: 2x2 tiles / 64x64 px;
- escada/porta para quartos;
- barris e cozinha;
- lareira: 2x2 tiles / 64x64 px;
- mesa de rumores.

## 15. Prefeitura/Cartório

```text
Externo: 16x12 tiles / 512x384 px
Interno: 22x16 tiles / 704x512 px
Moradores: Mara e Tovin em casa anexa
Cama: cama casal em cômodo residencial lateral
Porta: 2x2 tiles / 64x64 px
```

Componentes:

- balcão de registros: 5x1 tiles / 160x32 px;
- mesa de Mara;
- mesa de Tovin;
- armário de documentos;
- mural de licenças: 2x2 tiles / 64x64 px;
- sala trancada de arquivo civil;
- acesso futuro a registros antigos.

## 16. Distrito de Ofícios

Inclui:

- Forja de Brumdar;
- Carpintaria de Nimble/Gurd/Hund;
- Ateliê de Mirela;
- Laboratório de Ozzra.

Regras:

- deve ser visualmente produtivo: fumaça, madeira, caixas, ferramentas;
- corredores externos mínimos de 2 tiles / 64 px;
- caminhos principais de 3 tiles / 96 px;
- deve ter rotas largas para NPCs carregando material;
- deve conectar diretamente ao caminho da fazenda.

## 17. Tamanhos de props e elementos urbanos

| Elemento | Tiles | Pixels | Uso |
|---|---:|---:|---|
| Quadro público | 2x2 | 64x64 | praça |
| Quadro de pedidos | 2x2 | 64x64 | taverna/guilda |
| Calendário da praça | 2x2 | 64x64 | eventos |
| Placa de loja | 1x1 ou 2x1 | 32x32 / 64x32 | leitura de serviço |
| Poste/lampião | 1x2 | 32x64 | noite/Nyx |
| Banco simples | 2x1 | 64x32 | praça/jardim |
| Banco longo | 3x1 | 96x32 | praça |
| Barril | 1x1 | 32x32 | colisão/decoração |
| Caixote | 1x1 | 32x32 | mercado/ofícios |
| Caixote grande | 2x1 | 64x32 | mercado/ofícios |
| Barraca de feira | 3x3 | 96x96 | festival/mercado |
| Carruagem | 5x3 | 160x96 | caravançará |
| Fonte pública comum | 4x4 | 128x128 | praça, não Anya |
| Poço lacrado | 3x3 | 96x96 | ruína discreta |
| Anvil/Bigorna | 2x1 | 64x32 | forja |
| Forja acesa | 3x3 | 96x96 | Brumdar |
| Bancada alquímica | 3x2 | 96x64 | Ozzra |
| Mesa de pesquisa | 3x2 | 96x64 | Thalindra |
| Cama simples | 2x2 | 64x64 | NPC schedule |
| Cama casal | 3x2 | 96x64 | NPCs casados |
| Cama de hóspede | 2x2 | 64x64 | estalagem |
| Tigela de pet | 1x1 | 32x32 | rancho/fazenda |
| Cerca pequena | 1x1 por segmento | 32x32 | rancho/festival |
| Portão de cerca | 2x1 | 64x32 | rancho |
| Arbusto pequeno | 1x1 | 32x32 | decoração |
| Árvore média | 2x3 | 64x96 | cidade/jardim |
| Árvore grande | 3x4 | 96x128 | bordas |

---

# PARTE E — Residências e camas

## 18. Residências e camas

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

# PARTE F — Agenda diária canônica

## 19. Períodos do dia

```text
06:00–09:00 Morning
09:00–12:00 WorkStart
12:00–14:00 Midday
14:00–18:00 WorkAfternoon
18:00–21:00 Evening
21:00–00:00 Night
00:00–06:00 Sleep/LateNight
```

## 20. Agenda padrão por NPC

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

## 21. Modificadores de agenda

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

# PARTE G — Movimento, física e pathfinding

## 22. Movimento do jogador

Direção recomendada:

```text
Movimento livre suave sobre tilemap 32x32.
Colisão baseada no footbox, não no sprite inteiro.
Interação por proximidade + direção do jogador.
```

Não usar movimento preso rigidamente tile-a-tile para o jogador, salvo se o projeto decidir por estética/escopo.

## 23. Movimento de NPCs

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
- se caminho estiver bloqueado por evento, usa fallback waypoint;
- NPCs grandes usam o mesmo grafo, mas footbox levemente maior.

## 24. Colisão

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
- forja/bigorna;
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

## 25. Portas e cenas internas

```text
DoorTrigger
  TargetSceneId
  TargetSpawnPointId
  RequiredState
  LockedMessage
  OpenHoursRule optional
  DoorSizeTiles
  DoorSizePx
```

Regras:

- porta comum usa 2x2 tiles / 64x64 px;
- porta de templo/guilda pode usar 3x3 tiles / 96x96 px;
- porta de loja fechada mostra horário;
- porta de casa privada pode bloquear entrada até relação/quest;
- estalagem permite entrada mais ampla;
- loja noturna só ativa em condições específicas;
- interiores podem ser cenas separadas ou subáreas carregadas, a definir em spec.

## 26. Camas

```text
BedId
OwnerNpcId[]
LocationId
BedType
ScheduleOnly
CanPlayerUse
SizeTiles
SizePx
```

Regras:

- cama simples: 2x2 tiles / 64x64 px;
- cama casal: 3x2 tiles / 96x64 px;
- cama de NPC é marcador de rotina;
- cama de casal aceita dois NPCs casados;
- jogador só usa cama de hóspedes da estalagem, se hospedagem existir;
- após casamento, cônjuge pode ter cama/rotina híbrida na fazenda, definida por spec de casamento.

---

# PARTE H — Props, componentes e pontos de interação

## 27. Componentes extras da cidade

| Componente | Local | Função | Tamanho |
|---|---|---|---:|
| Quadro público | Praça | eventos, avisos, pedidos simples | 2x2 tiles |
| Quadro de pedidos | Taverna/Guilda | contratos e encomendas | 2x2 tiles |
| Calendário | Praça | festivais, aniversários, luas | 2x2 tiles |
| Sino de Kanthor | Templo | festival, quest de Corvus | 2x2 tiles |
| Estátuas antigas | Jardim | lore Anya/Cindar | 2x3 a 3x4 tiles |
| Poço lacrado | Ruína discreta | acesso/subsolo futuro | 3x3 tiles |
| Fonte pública comum | Praça | decoração, não Anya | 4x4 tiles |
| Placas de lojas | lojas | horário e nome | 1x1 ou 2x1 tiles |
| Barracas de festival | Praça/Mercado | eventos temporários | 3x3 tiles |
| Carruagem/caravançará | Estrada | Finan/mercadores | 5x3 tiles |
| Placa de perigo | Caverna | tutorial e alerta | 2x2 tiles |
| Poste/lampiões | ruas | noite/Nyx | 1x2 tiles |
| Bancos | praça/jardim | NPC idle/social | 2x1 ou 3x1 tiles |
| Caixotes/barris | mercado/ofícios | colisão/decoração | 1x1 ou 2x1 tiles |
| Bigorna/forja | forja | interação de Brumdar | 2x1 / 3x3 tiles |
| Bancada alquímica | alquimia | interação de Ozzra | 3x2 tiles |
| Mesa de pesquisa | arquivo | Thalindra/lore | 3x2 tiles |
| Tigelas/currais | rancho | Eiran/animais | 1x1 / área variável |

## 28. Pontos de spawn

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

# PARTE I — Roadmap de construção da cidade

## 29. Roadmap conceitual

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

## 30. Roadmap 0 — Reconciliar base existente

Objetivo:

- verificar cenas existentes;
- verificar sistemas de interação já implementados;
- verificar sistema de shops, inventory, save, economy, time/day;
- mapear o que pode ser reaproveitado;
- confirmar assets reais de player/NPC 32x48 px no Unity.

## 31. Roadmap 1 — Cidade navegável essencial

Entregas:

- CityScene externa;
- tilemap base 128x96 tiles, 32x32 px;
- zonas principais;
- colisores;
- portas placeholder;
- pontos de spawn;
- saída para fazenda;
- saída para caverna bloqueada/liberável;
- praça, mercado, templo, taverna, ofícios e guilda como formas externas.

## 32. Roadmap 2 — Serviços, lojas e interiores essenciais

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

## 33. Roadmap 3 — NPC schedules, casas e camas

Entregas:

- spawn de NPCs por hora;
- agenda padrão;
- camas como markers;
- casas/residências;
- NPC indo para trabalho/casa;
- NPCs fora de cena resolvidos por schedule tick;
- diálogo básico por horário.

## 34. Roadmap 4 — Reputação, visitas à fazenda e quests pessoais

Entregas:

- TownReputation;
- NpcRelationship;
- gatilhos de visita à fazenda;
- cartas/presentes simples;
- primeiras quests pessoais;
- romance flags;
- casamento ainda pode ficar para spec separada.

## 35. Roadmap 5 — Festivais, luas e religião pública

Entregas:

- calendário;
- festivais principais;
- templo de Kanthor funcional;
- altares permitidos;
- bloqueio explícito de altar/estátua de Anya;
- eventos de Alihana, Senya e Nyx;
- loja noturna de Yael.

## 36. Roadmap 6 — Segredos, subsolo, Bromécia/Elyndor e Anya

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

# PARTE J — Specs futuras derivadas

## 37. Ordem recomendada

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

## 38. Fontes obrigatórias por spec

Toda spec acima deve ler:

```text
docs/design/SPEC_SOURCE_MAP.md
docs/design/SPECIFICATION_PROCESS.md
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

# PARTE K — Decisões fechadas

```text
Player visual de referência: 32x48 px.
NPC comum visual: 32x48 px.
Tile base da cidade: 32x32 px.
Cidade externa recomendada: 128x96 tiles.
Tamanho externo em pixels: 4096x3072 px.
Cidade usa movimento livre suave sobre tilemap.
Player e NPCs usam collider de footbox, não sprite inteiro.
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

# PARTE L — Pendências

- Confirmar no Unity o tamanho real do sprite do player.
- Confirmar PPU/import settings dos sprites.
- Confirmar se tilemap atual usa 16x16, 32x32 ou escala visual diferente.
- Validar se cidade externa 128x96 tiles é aceitável para performance/arte.
- Definir se interiores são cenas separadas ou carregados no mesmo mapa.
- Definir pathfinding final: grid A*, waypoint fixo ou híbrido.
- Definir visual final das construções.
- Definir colisão por TilemapCollider2D, CompositeCollider2D ou colliders manuais.
- Definir se NPCs podem ser bloqueados fisicamente pelo jogador.
- Definir se lojas fechadas permitem entrada ou bloqueiam porta.
- Definir se festival altera a mesma cena ou carrega cena temporária.
- Definir como casamento altera residência/cama do cônjuge.
