# SPEC — Cidade Coerente: Relayout, Identidade Visual e Navegação com Preservação Total

> **Spec ID:** `spec_city_preservation_first_coherent_relayout`
> **Status:** BUILD_VALIDATED_PENDING_HUMAN_PLAYMODE
> **Wave:** WAVE CITY — Preservation-First TownScene
> **Priority:** P0
> **Type:** Editor tooling + Scene composition + Runtime integration + Validation
> **Domain:** City / TownScene / NPC movement / World art
> **Parallelizable:** NO
> **Parallel group:** N/A
> **Can run with:** somente trabalho que não toque TownScene, cidade, world sprites, NPC schedules ou geradores de cena
> **Must not run with:** qualquer spec que altere TownScene, `CreateMvpTownScene`, `TownDistrictLayout`, NPC schedules, world art ou scene validators
> **Repo lock scope:**
> - `Assets/_Game/Scripts/Editor/SceneCreation/CreateMvpTownScene.cs`
> - `Assets/_Game/Scripts/Editor/SceneCreation/TownDistrictLayout.cs`
> - novos arquivos de layout da cidade em `Assets/_Game/Scripts/Editor/SceneCreation/City/`
> - `Assets/_Game/Scripts/Editor/Validation/ValidateFableCitySchedule.cs`
> - testes de layout e navegação da cidade
> - `Assets/_Game/Scenes/TownScene.unity` somente por regeneração Unity
> - sprites de mundo efetivamente usados pela TownScene
> **Depende de (Depends on):**
> - estado materializado atual da TownScene pós `spec_fable_40_town_48x42_relayout`
> - `HouseDoorInteractable`, `RoofRevealController`, `NpcScheduleAnchor`, `InteractionSystem`
> - `TownSceneRuntimeReferenceInstaller`, spawns e portais atuais
> - ADR-0011 (`32 px/tile`)
> **Não depende de:** `spec_city_real_walkin_houses_no_teleport` — absorvida por conter baseline e reduções incompatíveis com o estado atual
> **Bloqueia (Blocks):** pass final de arte da cidade e ajustes de festivais que dependam do layout definitivo
> **Scope:** reorganizar a cidade existente para obter distritos legíveis, vias conectadas, prédios semanticamente distintos, anchors coerentes e navegação confiável, preservando integralmente objetos, quantidades, IDs e lógicas já materializados.
> **Out of scope:** remover conteúdo, reduzir contagens, renomear IDs estáveis, reescrever sistemas de interação/schedule/save, alterar economia/inventário/combate, criar NPCs ou serviços não aprovados, editar YAML de cena manualmente.

required_adrs: [ADR-0011]
required_game_rules: [city_rules.md]

---

## Amendment v8 — planta visual aprovada em 2026-07-01

Este amendment é a autoridade espacial mais recente e substitui somente as cláusulas antigas que
congelavam `76×64`, mantinham a entrada a oeste ou descreviam a distribuição anterior. Todos os
contratos de preservação de conteúdo, IDs, serviços, interiores, portas, roof reveal, schedules,
portais e quantidades mínimas continuam válidos.

- footprint aprovado: `120×90` (`HalfWidth=60`, `HalfHeight=45`);
- entrada, portal da fazenda e `town_from_farm`: centro do lado sul, mantendo os mesmos IDs;
- norte cívico: templo `16×13` + cemitério cercado, mansão `16×13`, câmara de pedra escura
  `16×11` e prédio nobre roxo `17×13`;
- oeste comercial: mercado/barracas, padaria e estalagem;
- leste de ofícios: forja, alquimia, oficina e curtume;
- meio-oeste: parque/lago com deck e bancos; moinho d'água imediatamente abaixo/leste da água;
- sudoeste: `House_AnimalYard` preserva o ID, mas assume leitura de galpão/armazém com portão grande;
- sul: fileira uniforme de residências sem bloquear a avenida do portão;
- casa residencial padrão: `8×7` exterior e `6×5` interior caminhável, com recuo/parede de 1 tile por lado;
- cada residência padrão contém cama, mesa, armazenamento, cozinha compacta e fogão sem collider,
  preservando o corredor central caminhável;
- telhados placeholder usam composição tiled limitada ao footprint + `0.3` tile de beiral por lado;
- avenida principal tem 7 tiles; vias secundárias têm 4 tiles quando não limitadas pelo frontage cívico;
- bancas de NPC ficam 4 tiles deslocadas da linha central da porta, preservando um apron livre de `3×3`;
- centro: praça pavimentada quadrada `26×26`, com fonte, estátua, bancos, canteiros e iluminação;
- os 28 NPCs mantêm IDs, horários e lógicas, com trabalho/social/casa realocados por papel;
- arte final específica fica pendente; até lá cada construção usa placeholder semântico nomeado e
  substituível, além das sprites modulares já existentes;
- árvores visuais passam de 334 para 497 por causa do perímetro maior; somente as 8 árvores internas
  mantêm collider pequeno de tronco.

Conflitos resolvidos explicitamente: as cláusulas antigas “footprint continua 76×64”, “acesso oeste
permanece” e “portão sul só pode ser adicional” estão superseded pela decisão humana v8. Nenhum ID ou
fluxo de transição foi removido; apenas sua posição mudou.

---

# /speckit.specify

## 5. Contexto e decisão vinculante

A TownScene já contém uma implementação substancial que não pode ser tratada como placeholder descartável. A cena atual possui casas físicas percorríveis, portas que abrem sem teleporte, roof reveal, NPCs com anchors, serviços, barracas, árvores, portais e lógica de geração. O problema atual é de **coerência espacial, visual e de navegação**, não de ausência total do sistema.

Decisão humana vinculante para esta spec:

```text
Não executar a spec anterior como autoridade.
Não perder itens, quantidades ou decisões já implementadas.
Pode adicionar componentes.
Pode reorganizar locais.
Pode ajustar lógicas de movimento.
Não pode eliminar as lógicas existentes.
```

Portanto, esta é uma spec de migração **preservation-first**. Código e cena materializados formam o baseline mínimo. Documentos antigos ou specs anteriores não podem autorizar redução do estado real.

## 6. Problema comprovado no repo

### 6.1 Layout

- `TownDistrictLayout` usa footprint atual de `76×64` (`HalfWidth=38`, `HalfHeight=32`), mas comentários ainda citam dimensões antigas.
- Cinco prédios cívicos são fixados; os demais são distribuídos por dispersão máxima. O algoritmo reserva marcos, mas não reserva o grafo viário completo.
- Há sete prédios sobrepostos às vias principais no baseline auditado:
  - eixo norte/sul: `House_MarketHall`, `House_Residential_1`, `House_Dagna`;
  - eixo leste/oeste: `House_Fishery`, `House_Registry`, `House_GateKeeper`, `House_Residential_2`.
- A disposição parece aleatória porque lotes, fachadas, portas, serviços e vias não compartilham uma mesma definição espacial.

### 6.2 Serviços e movimento de NPCs

- Anchors de trabalho derivam de coordenadas legadas escaladas, enquanto prédios derivam de outro algoritmo.
- A distância média aproximada entre ponto de trabalho e prédio correspondente é `33,1` tiles no snapshot auditado.
- Casos críticos incluem Fishery, Tannery, Records, Blacksmith, Alchemy, Inn, Animal Yard e Workshop.
- O sistema de schedule/waypoints existe e deve permanecer. O problema é a origem incoerente dos destinos, não a existência da lógica.

### 6.3 Prédios e arte

- Os 24 prédios usam essencialmente o mesmo telhado `roof_redtile`, escalado até preencher footprints diferentes.
- `wall_plaster`, `wall_plank`, `door_wood` e `window_wood` existem, mas não estruturam as fachadas geradas.
- Props relevantes ainda usam retângulos builtin da Unity.
- Todo prédio recebe interior genérico semelhante, mesmo quando sua função é templo, cartório, oficina, mercado ou residência.
- `TownHallBuilding` e `House_Chamber` competem pela mesma leitura cívica. Nenhum deve ser removido; precisam receber funções visuais distintas.

### 6.4 Navegação, colisão e leitura

- Portas aceitam apenas `South` ou `North`; lotes leste/oeste não conseguem orientar a entrada para sua rua.
- Árvores usam collider derivado do sprite opaco completo, produzindo bloqueio de copa. O collider navegável deve corresponder ao tronco/base.
- O anel externo já fica fora do limite jogável; árvores externas não precisam criar uma segunda parede sólida invisível.
- O lago é principalmente decorativo e não possui contrato completo de borda, água, dock e bloqueio.
- Sorting usa ordens fixas e pivôs heterogêneos, sem contrato único de bottom-pivot/Y-sort.

### 6.5 Validação insuficiente

- Testes existentes verificam dimensões e distritos básicos, mas ainda contêm mensagens de `48×42`.
- O validator atual não prova preservação exata, conectividade, acesso a portas, relação serviço/prédio, colisores de árvores ou ausência de placeholders finais.
- A spec anterior previa substituir/remover prédios e reduzir inventário. Isso contradiz o baseline atual e esta decisão humana.

## 7. Objetivo

Entregar uma TownScene que:

1. preserve todo conteúdo e todos os contratos do baseline;
2. organize os prédios em lotes e distritos intencionais;
3. conecte spawns, portais, praças, serviços e portas por vias navegáveis;
4. alinhe casa, trabalho e anchors sociais de cada NPC às suas funções reais;
5. dê identidade visual aos prédios sem deformar sprites nem apagar sistemas existentes;
6. mantenha movimento livre do jogador e schedule/waypoints dos NPCs;
7. converta preservação, acessibilidade e composição em validações automatizadas.

## 8. Fontes obrigatórias lidas

```text
docs/design/SPEC_SOURCE_MAP.md
docs/design/SPECIFICATION_PROCESS.md
docs/design/lore/VAALARA_GAME_CANON_DIRECTION_v1.0.md
docs/design/gameplay/city/CITY_DESIGN_DIRECTION_v1.2.md
docs/design/gameplay/city/CITY_NPC_ROSTER_SERVICES_DIRECTION_v1.1.md
docs/design/gameplay/city/CITY_LAYOUT_BUILDINGS_SCHEDULE_DIRECTION.md
docs/decisions/ADR-0011-pixel-art-scale-32px-per-tile.md
.specs/SPEC_IMPLEMENTABLE_TEMPLATE.md
.specs/SPEC_GENERATION_ROADMAP_MASTER.md
.specs/SPEC_WAVE_EXECUTION_PROTOCOL.md
.specs/SPEC_VALIDATION_MATRIX_MASTER.md
.specs/implementados/spec_fable_40_town_48x42_relayout.md
.specs/absorvidas/city_relayout_reconciliation/spec_city_real_walkin_houses_no_teleport.md
docs/game_rules/city_rules.md
Assets/_Game/Scripts/Editor/SceneCreation/CreateMvpTownScene.cs
Assets/_Game/Scripts/Editor/SceneCreation/TownDistrictLayout.cs
Assets/_Game/Scripts/Editor/Validation/ValidateFableCitySchedule.cs
Assets/_Game/Tests/EditMode/City/Editor/TownLayoutTests.cs
Assets/_Game/Scenes/TownScene.unity (inventário mecânico; não edição manual)
```

## 9. Estado atual do repo — baseline mínimo vinculante

Contagens confirmadas no snapshot de criação desta spec:

| Contrato | Baseline mínimo | Regra |
|---|---:|---|
| `House_*` | 24 | manter todos os nomes; pode adicionar, nunca reduzir |
| `HouseDoorInteractable` | 24 | uma porta funcional por prédio atual, no mínimo |
| `RoofReveal` | 24 | preservar comportamento e cobertura |
| `Stall_npc_*` | 23 | manter objetos, vínculos e IDs; pode reorganizar |
| `MarketSquare_Stall_*` | 6 | manter; pode enriquecer visualmente |
| Total de barracas | 29 | não reduzir |
| `TownTree_*` | 334 | não reduzir quantidade visual; ajustar localização/collider permitido |
| Spawn `town_default` | 1 | ID e função preservados |
| Spawn `town_from_farm` | 1 | ID e função preservados |

Os 24 prédios baseline que não podem desaparecer nem ser renomeados:

```text
House_AlchemyLab
House_AnimalYard
House_Archive
House_Bakery
House_Blacksmith
House_CarvalhoTorto
House_Chamber
House_Dagna
House_Fishery
House_GateKeeper
House_Inn
House_Manor
House_MarketHall
House_Pip
House_Prison
House_Registry
House_Residential_1
House_Residential_2
House_Residential_3
House_Residential_4
House_Tannery
House_Temple
House_Tovin
House_Workshop
```

Antes de editar qualquer gerador, a execução deve produzir `TownSceneBaselineManifest` contendo:

```text
nome e hierarquia dos objetos preservados;
componentes relevantes por objeto;
IDs estáveis, spawn IDs, portal targets e anchor IDs;
contagens por família;
posição, bounds e orientação atuais;
NPC -> home/work/social anchors;
sprite, sorting layer/order, pivot/import scale usados;
colliders e flags trigger/solid;
referências serializadas do runtime installer.
```

O manifesto gerado na execução é a autoridade mecânica para anti-regressão. Os números acima são piso inicial, não licença para ignorar elementos adicionais encontrados.

## 10. Histórias do usuário

```text
Como jogador, quero reconhecer praça, mercado, templo, cartório, ofícios, residências e acessos pela forma da cidade.
Como jogador, quero chegar a qualquer serviço por um caminho claro sem atravessar prédio, árvore, água ou decoração.
Como jogador, quero que portas estejam voltadas para a rua e sejam alcançáveis.
Como jogador, quero ver NPCs se deslocando de maneira coerente entre casa, trabalho e áreas sociais.
Como mantenedor, quero reorganizar a cidade sem perder nenhum objeto, ID, serviço ou comportamento existente.
Como artista, quero prédios compostos por peças na escala canônica, sem telhados deformados.
Como mantenedor, quero validators que falhem antes de uma regeneração apagar ou desconectar conteúdo.
```

## 11. Escopo

Inclui:

- inventário e manifesto automatizado do baseline;
- decomposição do layout em distritos, vias, lotes, entradas e clusters de props;
- substituição do posicionamento por dispersão por layout determinístico orientado a lotes;
- preservação do footprint `76×64` nesta entrega; expansão somente se a auditoria provar impossibilidade e sem reduzir densidade útil;
- reposicionamento de prédios, árvores, barracas, anchors e props;
- orientação de portas em quatro direções;
- caminhos de acesso entre via e porta;
- reconciliação de anchors com prédios e serviços existentes;
- ajuste de movimento/path targets sem remover schedule tick, waypoints ou fallback existentes;
- identidade visual por arquétipo de prédio;
- composição modular de fachada/telhado/porta/janela/interior;
- distinção funcional entre `House_Chamber` e `TownHallBuilding`;
- tratamento navegável do lago, muros, portões e árvores;
- validators e testes de regressão;
- regeneração da TownScene exclusivamente pelo Unity/editor generator;
- relatório e cenário humano final.

## 12. Fora de escopo

Não inclui:

- remover, fundir ou substituir objetos baseline;
- reduzir casas, barracas, árvores, NPCs, serviços, anchors ou portais;
- renomear `House_*`, NPC IDs, spawn IDs ou portal contracts;
- mover interiores para cenas separadas;
- trocar `HouseDoorInteractable`, `RoofRevealController`, `InteractionSystem` ou schedule por sistemas novos;
- alterar inventário, itens iniciais, `ItemDatabase`, `WeaponDatabase`, combate, inimigos, Q/E, save ou HUD;
- mudar schema de save;
- implementar NPCs/serviços futuros ainda não materializados;
- alterar economia, stock, prices, quests, romance, festivais ou clima;
- produzir todo o catálogo de arte final do jogo;
- editar `.unity`, `.prefab` ou `.asset` manualmente como YAML;
- atualizar `SPEC_EXECUTION_ORDER.md` sem autorização humana específica.

## 13. Regras de não duplicação e preservação

```text
NÃO recriar HouseDoorInteractable, RoofRevealController, NpcScheduleAnchor ou InteractionSystem.
NÃO criar segundo gerador independente da TownScene.
NÃO manter duas fontes concorrentes para prédio, lote, porta e anchor.
NÃO substituir schedule tick/waypoints: ajustar somente destinos, grafo e fallback.
NÃO converter objetos existentes em simples documentação; eles devem continuar materializados.
NÃO usar GameObject.Find/FindObjectOfType em runtime.
NÃO reduzir contagens para resolver colisão ou composição.
NÃO deformar um sprite completo para representar footprints arbitrários.
NÃO usar a spec absorvida como autoridade de quantidade, dimensão ou remoção.
```

Quando um objeto parecer redundante, a solução é atribuir função distinta ou manter um adaptador de compatibilidade, nunca apagá-lo nesta spec.

## 14. Critérios de aceite funcionais

### 14.1 Preservação integral

- Manifesto `after` contém todos os objetos/IDs/componentes do manifesto `before`.
- Para cada família inventariada: `after.count >= before.count`.
- As contagens mínimas da seção 9 são satisfeitas.
- Nenhum vínculo serializado do `TownSceneRuntimeReferenceInstaller` fica ausente.
- Nenhum NPC, serviço, residência, barraca, portal, spawn ou schedule anchor baseline desaparece.

### 14.2 Layout e vias

- Zero overlap proibido entre footprints de prédios.
- Zero overlap entre footprint sólido de prédio e área navegável das vias.
- Todo prédio pertence a exatamente um lote e um distrito.
- Toda porta possui frontage conectado a uma via por caminho de largura mínima definida.
- Praça, mercado, templo, cartório/câmara, ofícios, lago e acessos têm silhuetas e circulação distinguíveis.
- O footprint continua `76×64`, salvo amendment explícito com evidência e autorização humana.

### 14.3 Navegação

- O grafo navegável conecta `town_default` e `town_from_farm` a todas as 24 portas, às 29 barracas, aos portais e aos anchors públicos obrigatórios.
- Nenhuma rota obrigatória depende de atravessar collider sólido.
- Corredor principal mínimo: 3 tiles; acesso local mínimo: 2 tiles; aproximação final da porta: ao menos 1 footbox livre por lado útil.
- Acesso a NPCs, lojas, portais e marcos não pode ser bloqueado por decoração.
- O portal/spawn oeste e seus IDs permanecem funcionais. Um portão sul só pode ser adicionado, não substituir silenciosamente o acesso oeste.

### 14.4 Prédios e portas

- `DoorSide` ou contrato equivalente suporta North, South, East e West.
- Porta é escolhida pela rua/frente do lote, não apenas pelo hemisfério do mapa.
- Todos os 24 prédios continuam percorríveis in-place, com porta sólida funcional e roof reveal.
- `House_Chamber` representa câmara/conselho; `TownHallBuilding` recebe função administrativa/visual complementar. Ambos permanecem.
- Interiores preservam props existentes e podem receber composição específica por arquétipo.

### 14.5 NPCs, serviços e movimento

- Cada NPC baseline mantém seus anchors obrigatórios e sua lógica de schedule.
- Anchor de trabalho de serviço fica no prédio correspondente, em sua frontage ou no cluster de barraca explicitamente associado.
- Distância navegável do work anchor ao ponto de serviço associado deve ser `<= 3` tiles, salvo exceção catalogada por função móvel.
- Rotas visíveis continuam usando movimento suave por waypoints.
- Troca offscreen por schedule tick continua permitida.
- Caminho bloqueado continua tendo fallback; fallback também precisa ser navegável.

### 14.6 Arte e sorting

- Escala segue ADR-0011: 32 px/tile, sem subpixel drift.
- Prédios usam composição modular ou sprites com footprint autoral compatível; nenhum telhado completo é esticado desproporcionalmente.
- Templo de Kanthor, mercado, cartório/câmara, estalagem, forja, oficinas e residências possuem ao menos um diferenciador estrutural e um de props/paleta.
- Sprites existentes aprovados têm prioridade sobre primitives.
- Primitives builtin só podem existir como fallback de debug explicitamente marcado; o validator deve listá-las.
- Sorting de personagens, árvores, prédios e props segue bottom-pivot/Y-sort ou contrato equivalente único e testável.

### 14.7 Árvores, lago e colisões

- Pelo menos 334 árvores visuais permanecem.
- Árvores internas usam collider restrito ao tronco/base, sem bloquear pela copa.
- Árvores fora do limite navegável podem ser visual-only; remover collider redundante não remove a árvore nem a lógica de borda.
- Lago possui máscara/edge visual, collider de água ou borda coerente e passagem/dock explícito quando aplicável.
- Nenhum collider de árvore, água ou prop fecha uma rota obrigatória.

### 14.8 Build e evidência

- docs validation passa para arquivos da entrega ou débitos legados são isolados e documentados.
- builds runtime/editor retornam exit code 0.
- Unity compile e validators retornam sucesso.
- EditMode tests de layout, manifesto e grafo passam.
- cenário humano final da TownScene está documentado; ACCEPTED só após evidência exigida pela matriz.

---

# /speckit.plan

## 15. Arquitetura alvo

### 15.1 Fonte única de layout

Introduzir uma definição determinística consumida pelo gerador, sem criar um segundo gerador:

```text
TownLayoutDefinition
  Bounds
  Districts[]
  Roads[]
  Lots[]
  Gates[]
  Landmarks[]
  PropClusters[]
  NavigationNodes[]

TownLotDefinition
  StableBuildingName
  DistrictId
  Center
  Footprint
  FrontageSide
  DoorOffset
  ServiceAnchorIds[]
  HomeAnchorIds[]
  SocialAnchorIds[]
  VisualProfileId

TownBuildingVisualProfile
  Archetype
  WallSet
  RoofSet
  DoorSet
  WindowSet
  Sign/Props
  InteriorProfile
  Sorting/Pivot contract
```

Pode ser implementada como classes C# determinísticas no editor ou assets gerados pelo editor. Não pode exigir edição YAML manual nem duplicar nomes/IDs em tabelas desconectadas.

### 15.2 Compatibilidade

- `TownHouseSpecs` pode virar projeção/adaptador da nova definição durante migração.
- Chamadores existentes continuam recebendo nomes, posições, tamanhos e cores até serem migrados.
- `TownNpcHomes` e specs de NPC passam a resolver anchors pelo `StableBuildingName`/lot, preservando seus IDs externos.
- `Reposition` permanece para consumidores legados não migrados; novos lotes não devem aplicar escala dupla.
- O acesso oeste e os spawn IDs atuais permanecem.

### 15.3 Distritos alvo

Sem excluir nenhum prédio atual:

| Distrito | Conteúdo principal | Regra espacial |
|---|---|---|
| Centro cívico | Chamber, TownHall, Registry, praça | fachadas para praça/via cívica |
| Norte institucional | Temple, Manor, Archive | silhueta dominante, circulação pública |
| Mercado | MarketHall, Bakery, stalls | 29 barracas preservadas e agrupadas por função |
| Ofícios | Blacksmith, Workshop, Tannery, AlchemyLab | pátios e rotas largas, props produtivos |
| Leste de serviços | Fishery, AnimalYard, GateKeeper | ligação clara com borda/lago/entrada |
| Residencial | casas nomeadas e Residential 1–4 | lotes menores, acessos locais conectados |
| Sul/periferia | Inn, Tovin, prisão e marcos externos | ligação com estrada sem bloquear portais |

As posições finais são responsabilidade da implementação e dos testes geométricos, não das coordenadas legadas. Todos os 24 prédios devem ser mapeados exatamente uma vez.

## 16. Contratos, dados e eventos

### Data contracts

- `TownSceneBaselineManifest`: snapshot antes/depois, serializável em relatório/editor output.
- `TownLayoutDefinition`: fonte única determinística.
- `TownLotDefinition`: footprint, frontage, porta e associações.
- `TownBuildingVisualProfile`: identidade visual sem alterar contratos de gameplay.
- `TownNavigationGraph`: nós/arestas usados para validação; pode reutilizar waypoints materializados.

### Runtime contracts preservados

```text
HouseDoorInteractable
RoofRevealController
NpcScheduleAnchor
InteractionSystem / IInteractable
TownSceneRuntimeReferenceInstaller
spawn/portal IDs existentes
schedule tick + waypoint movement + fallback
```

### Events

- Nenhum evento existente é removido ou renomeado.
- Evento novo só é permitido se necessário para atualização de navegação/visual e deve seguir `GameEventBus` atual.
- Não criar barramento paralelo.

### Save

- Schema change: NO.
- Migration: NO.
- Referências persistidas/IDs existentes: preservadas.
- Nenhum estado visual de layout deve entrar no save nesta spec.

## 17. Sistemas afetados

```text
Editor scene generation
Town layout/district definitions
Walk-in building composition
NPC schedule anchor placement
Waypoint/path validation
World sprite selection and sorting
Tree/water/prop collision composition
TownScene validators and EditMode tests
City canonical docs após evidência de implementação
```

## 18. Arquivos permitidos

```text
Assets/_Game/Scripts/Editor/SceneCreation/CreateMvpTownScene.cs
Assets/_Game/Scripts/Editor/SceneCreation/TownDistrictLayout.cs
Assets/_Game/Scripts/Editor/SceneCreation/City/** (novos componentes/dados editoriais)
Assets/_Game/Scripts/Editor/Validation/ValidateFableCitySchedule.cs
Assets/_Game/Scripts/Editor/Validation/**Town**.cs
Assets/_Game/Tests/EditMode/City/Editor/TownLayoutTests.cs
Assets/_Game/Tests/EditMode/City/**
Assets/_Game/Scripts/World/HouseDoorInteractable.cs (somente compatibilidade comprovadamente necessária)
Assets/_Game/Scripts/World/RoofRevealController.cs (somente compatibilidade comprovadamente necessária)
Assets/_Game/Scripts/NPC/** (somente resolução de anchors/waypoints; sem remover schedule)
Assets/_Game/Art/World/** (seleção/import de assets da cidade, 32 px/tile)
Assets/_Game/Art/Generated/World/_TileAssets/** (somente output gerado pela API do Unity)
Assets/_Game/Scenes/TownScene.unity (somente output regenerado pelo Unity)
docs/design/gameplay/city/** (reconciliação pós-implementação)
docs/game_rules/city_rules.md (reconciliação pós-implementação)
docs/validation/**
```

Qualquer arquivo fora dessa lista exige amendment na spec e justificativa no report antes da edição.

## 19. Arquivos proibidos

```text
Assets/_Game/Scripts/Inventory/**
Assets/_Game/Scripts/Items/**
Assets/_Game/Scripts/Combat/**
Assets/_Game/Scripts/Enemies/**
ItemDatabase e WeaponDatabase
Assets/_Game/Scripts/Save/**
HUD, hotbar e input Q/E
Packages/**
ProjectSettings/**
docs_old/**
docs/archive/**
qualquer FarmScene/CaveScene fora de referência read-only
qualquer .unity/.prefab/.asset editado manualmente como YAML
```

## 20. Estratégia de implementação

### Fase 0 — Freeze e manifesto

1. Reexecutar inventário do repo e da TownScene.
2. Gerar manifesto before versionado no execution report ou artefato de validação.
3. Confirmar todos os objetos, IDs, componentes, referências e contagens.
4. Interromper se o snapshot divergir desta spec de forma material; criar amendment, não adivinhar.

### Fase 1 — Testes antes do relayout

1. Criar regression tests do manifesto e pisos de contagem.
2. Criar testes geométricos de overlap, frontage, bounds e roads.
3. Criar teste de conectividade do grafo para portas/spawns/portais.
4. Corrigir asserts/mensagens antigas de 48×42 sem mudar o footprint real.

### Fase 2 — Modelo de lotes e vias

1. Introduzir `TownLayoutDefinition` e adaptador para contratos existentes.
2. Declarar vias antes de posicionar lotes.
3. Alocar os 24 prédios exatamente uma vez.
4. Reservar lago, praças, mercado, muros, portais e corredores.
5. Eliminar dispersão aleatória como autoridade final.

### Fase 3 — Portas, acessos e navegação

1. Suportar quatro orientações de porta.
2. Gerar frontage e caminho de acesso para cada lote.
3. Materializar/validar grafo navegável.
4. Preservar portais/spawns e adicionar entradas apenas de forma aditiva.

### Fase 4 — Anchors e movimento

1. Mapear home/work/social anchors ao lote/prédio correspondente.
2. Preservar IDs e períodos de schedule.
3. Ajustar destinos e waypoints, não remover a máquina de schedule.
4. Validar work anchors próximos ao serviço e fallback navegável.

### Fase 5 — Identidade visual

1. Criar perfis de Templo, Cívico, Mercado, Estalagem, Oficina, Rural e Residencial.
2. Compor paredes, telhados, portas, janelas, placas e props sem stretching destrutivo.
3. Diferenciar interiors sem apagar props baseline.
4. Aplicar sorting/pivot consistente.
5. Usar concept art aprovado como referência quando disponível; ausência de concept não autoriza inventar estilo incompatível com sprites existentes.

### Fase 6 — Natureza e bordas

1. Reposicionar as 334+ árvores preservando contagem visual.
2. Aplicar collider de tronco nas árvores navegáveis e visual-only fora dos bounds.
3. Integrar lago/edge/dock/colisão ao grafo.
4. Garantir que decoração não feche rotas.

### Fase 7 — Regeneração e validação

1. Regenerar a TownScene pelo menu/editor autorizado.
2. Gerar manifesto after e comparar com before.
3. Rodar validators, EditMode, build e Unity compile.
4. Produzir screenshots de pontos fixos e cenário humano final.
5. Só então reconciliar docs de cidade com o estado comprovado.

## 21. Ordem de execucao (ordem segura)

```text
manifesto before
→ regression tests
→ definição de vias/lotes
→ adaptador de compatibilidade
→ portas/frontage
→ anchors/waypoints
→ identidade visual
→ árvores/lago/props
→ regeneração Unity
→ manifesto after/diff
→ build/validators/tests
→ cenário humano final
→ docs/report
```

## 22. Paralelização

```text
Parallelizable: NO
Reason: gerador, definição espacial, anchors, arte e validação compartilham a mesma fonte de layout.
Locks: CreateMvpTownScene, TownDistrictLayout, TownScene, city validators, town anchor placement.
```

## 23. Impacto em save/load

```text
Changes save schema: NO
Adds save section: NO
Requires migration: NO
Persists Unity references: NO
Stable IDs removed/renamed: NO
```

## 24. Impacto em eventos

```text
Removes events: NO
Renames events: NO
Adds events: CONDITIONAL, somente com justificativa e testes
Requires unsubscribe pattern: YES para qualquer subscriber novo
```

## 25. Impacto em UI/Unity

```text
Changes UI/HUD: NO
Changes input Q/E: NO
Changes scene: YES, somente via generator/editor
Changes prefabs: NO por padrão
Changes ScriptableObjects/assets: CONDITIONAL, somente se a definição gerada exigir
Requires Unity compile: YES
Requires Play Mode/final human scenario: YES
Human validation timing: DEFERRED_TO_FINAL_VALIDATION
```

## 26. Riscos técnicos

| Risco | Mitigação obrigatória |
|---|---|
| Regenerador apagar conteúdo | manifesto before/after + teste de pisos antes da cena ser aceita |
| Relayout quebrar save/IDs | proibição de rename/removal + adaptadores por stable name |
| Casa acessível visualmente, mas rota bloqueada | grafo de conectividade + teste de clearance |
| Anchor correto por coordenada, mas sem caminho | medir path distance e fallback, não apenas distância euclidiana |
| 334 árvores causarem bloqueio/performance | árvores externas visual-only; internas com collider de tronco; batching/sorting auditado |
| Arte esticada ou escala divergente | ADR-0011 + profiles modulares + validator de import/scale |
| Docs canônicos divergirem do código atual | código materializado é baseline de preservação; reconciliar docs após evidência |
| Gerador monolítico ficar mais frágil | extrair dados/validators em arquivos focados sem criar segundo gerador |
| Mudança de NPC schedule virar reescrita | limitar a destinos/grafo/adaptadores; testes provam períodos e IDs preservados |
| Aumento do mapa piorar densidade | footprint 76×64 congelado; expansão exige amendment humano |

## 27. Rollback

```text
1. Manter manifesto before e commit anterior como referência.
2. Reverter somente os arquivos desta spec.
3. Regenerar TownScene pelo gerador anterior; nunca restaurar YAML parcial manualmente.
4. Confirmar que manifesto restaurado corresponde ao before.
5. Não apagar classes de compatibilidade ainda referenciadas.
6. Não tocar saves reais do usuário.
```

---

# /speckit.tasks

## 28. Tasks

```md
- [ ] T001 — Gerar `TownSceneBaselineManifest` before com todos os objetos, IDs, componentes, referências, anchors, colliders, sprites e contagens.
- [ ] T002 — Criar regression tests para 24 casas, 24 portas, 24 roof reveals, 23 NPC stalls, 6 market stalls, 334 árvores e IDs/spawns atuais.
- [ ] T003 — Corrigir testes/comentários obsoletos de 48×42 sem alterar o footprint 76×64.
- [ ] T004 — Introduzir `TownLayoutDefinition`, `TownLotDefinition`, roads/districts e adaptador de compatibilidade para `TownHouseSpecs`.
- [ ] T005 — Mapear todos os 24 prédios exatamente uma vez em lotes, preservando stable names.
- [ ] T006 — Reservar vias antes dos lotes e eliminar os sete overlaps baseline.
- [ ] T007 — Implementar frontage e porta N/S/E/W, com acesso navegável por lote.
- [ ] T008 — Criar grafo/validator de conectividade entre spawns, portais, 24 portas, 29 barracas e anchors públicos.
- [ ] T009 — Reconciliar home/work/social anchors com lotes e serviços, mantendo IDs e schedule.
- [ ] T010 — Ajustar waypoints/path targets e fallbacks sem remover movimento suave ou schedule tick.
- [ ] T011 — Criar perfis visuais por arquétipo e composição modular 32 px/tile.
- [ ] T012 — Diferenciar Chamber e TownHall sem remover nenhum dos dois.
- [ ] T013 — Reorganizar as 29 barracas preservadas em clusters coerentes com serviços/mercado.
- [ ] T014 — Reorganizar 334+ árvores; colliders internos no tronco e árvores externas visual-only.
- [ ] T015 — Integrar lago, bordas, dock/passagem e colisão ao layout/grafo.
- [ ] T016 — Aplicar contrato de bottom-pivot/Y-sort a personagens, prédios, árvores e props.
- [ ] T017 — Estender `ValidateFableCitySchedule` para manifesto, anchors, portas, paths, colliders e placeholders.
- [ ] T018 — Regenerar TownScene exclusivamente pelo Unity/editor generator.
- [ ] T019 — Gerar manifesto after e provar `after >= before` e ausência de IDs/refs perdidos.
- [ ] T020 — Rodar docs validation, builds, Unity compile, EditMode e validators.
- [ ] T021 — Criar execution report, diff do manifesto, screenshots fixos e cenário humano final.
- [ ] T022 — Reconciliar city rules/design docs somente com o estado final comprovado.
```

## 29. Validações obrigatórias

```powershell
./tools/docs/validate_docs.ps1
dotnet build ./Assembly-CSharp.csproj
dotnet build ./Assembly-CSharp-Editor.csproj
./tools/docs/run_strict_validation.ps1
```

Validações Unity obrigatórias:

```text
Unity compile sem erro.
EditMode: TownLayout, baseline manifest, overlap, frontage, graph connectivity, anchors e colliders.
Editor validator: ValidateFableCitySchedule PASS.
TownScene regeneration audit: zero LogError.
Play Mode/final human scenario: navegação e comportamento visual.
```

Se qualquer comando não puder rodar, registrar `NOT RUN`, motivo, impacto e risco residual. Não converter ausência de teste em PASS.

## 30. Testing Quality Gate

```md
- Changed deterministic logic: YES
- Requires EditMode tests: YES
- Requires PlayMode automated or final human scenario: YES
- Requires regression test: YES
- Human validation timing: DEFERRED_TO_FINAL_VALIDATION
- Minimum validation evidence for ACCEPTED: manifesto before/after sem perdas; builds e Unity compile PASS; EditMode e validators PASS; grafo conecta todos os destinos; cenário humano final confirma navegação, portas, roof reveal, movimento de NPCs e leitura visual.
```

## 31. Definition of Done

```text
Todo conteúdo baseline permanece materializado e referenciado.
Contagens after são iguais ou maiores que before.
Os 24 prédios têm lotes, frontages, portas e rotas acessíveis.
As 29 barracas e 334+ árvores permanecem.
Nenhum dos sete overlaps baseline permanece.
Anchors de serviço estão associados ao prédio/barraca correto.
Schedule, waypoints, offscreen tick e fallback continuam funcionando.
Prédios principais são visualmente distintos e seguem 32 px/tile.
Árvores internas colidem pelo tronco; bordas não criam bloqueios redundantes.
Spawns e portais atuais mantêm IDs e função.
Manifesto, tests, validators, build e cenário final produzem evidência rastreável.
Nenhum arquivo proibido foi alterado.
```

## 32. Anti-regressão

```text
Não reduzir 24 House_*, 24 portas, 24 roof reveals, 29 barracas ou 334 árvores.
Não remover NPC, serviço, anchor, portal, spawn ou referência serializada existente.
Não renomear IDs estáveis.
Não substituir schedule/waypoints por teleporte visível ou lógica nova incompatível.
Não reintroduzir interiores off-field ou portas internas por teleporte.
Não bloquear acesso a NPCs, lojas, portais, praça ou marcos.
Não usar collider de copa completa em árvores navegáveis.
Não deformar sprites completos para footprints arbitrários.
Não aceitar build como prova de comportamento da cena.
Não editar YAML de cena/prefab/asset manualmente.
Não alterar inventory, starter items, ItemDatabase, WeaponDatabase, combat, enemies, Q/E, save ou HUD.
```

## 33. Notas para execução posterior

```text
Esta spec está aprovada como contrato, não como autorização automática de execução.
A execução deve começar pela T001; qualquer tentativa de começar pelo relayout é inválida.
Se o baseline real tiver mais conteúdo que esta spec, o manifesto real vence como piso de preservação.
Se um objeto parecer duplicado, atribuir função distinta ou manter compatibilidade; remoção exige nova decisão humana.
Concept art aprovado orienta composição, mas nunca substitui os contratos mecânicos de preservação e navegação.
A spec absorvida permanece como histórico e não deve ser executada nem usada para reduzir o estado atual.
```
