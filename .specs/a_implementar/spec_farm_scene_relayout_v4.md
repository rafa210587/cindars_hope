# SPEC — FarmScene Relayout v4: Disposição Stardew-like com Montanha, Rio/Ponte, Bosque e Base a Leste

> **Spec ID:** `spec_farm_scene_relayout_v4`
> **Status:** A implementar
> **Wave:** WAVE FARM — Coerência da FarmScene (pós WAVE 05/07 + fable_15/17)
> **Priority:** P1
> **Type:** Runtime + Tooling (editor scene generator)
> **Domain:** Farm
> **Parallelizable:** NO
> **Parallel group:** N/A
> **Can run with:** specs que não tocam a FarmScene, o gerador da fazenda, nem os contracts de layout da fazenda
> **Must not run with:** qualquer spec que edite `CreateMvpFarmScene.cs`, `FarmLevel1LayoutContract.cs`, `FarmScaleContract.cs`, a FarmScene, ou validators da fazenda
> **Repo lock scope:**
> - `Assets/_Game/Scripts/Editor/SceneCreation/CreateMvpFarmScene.cs`
> - `Assets/_Game/Scripts/Farm/FarmLevel1LayoutContract.cs`
> - `Assets/_Game/Scripts/Farm/FarmScaleContract.cs`
> - `Assets/_Game/Scripts/Editor/Validation/Validate*Farm*.cs` (validators de cena/recursos da fazenda)
> - novos scripts de `Assets/_Game/Scripts/World/` para montanha/rio/ponte/quadro de evoluções (criados aqui)
> **Depends on (Depende de):**
> - `spec_farm_till_anywhere_tilemap` (Spec B — sistema de arar qualquer terra). Esta spec **remove** os 24 canteiros fixos e passa a depender do solo arável por tile. **B deve ser implementada antes ou junto.**
> - Sistemas existentes (NÃO recriar): `CaveEntranceInteractable` (WAVE16), `SceneTransitionRouter`/`PlayerSpawnResolver` (WAVE13), `FishingSpot`, `TreeResource`/`RockResource`/`ForageResource` interactables (WAVE07), `SellPoint`/`ShippingBin` runtime (WAVE07), Fonte de Anya física + Água Viva (fable_17), `CraftingPoint`/`CraftingStation` (WAVE14), `IInteractable`/InteractionSystem, GameEventBus.
> **Blocks (Bloqueia):**
> - Qualquer regeneração futura da FarmScene que dependa da disposição v4.
> **Scope:** Regerar a FarmScene com a disposição v4 (montanha+caverna ao norte, base/casa+serviços a leste junto à saída da cidade, bosque denso a noroeste, construções de animais afastadas ao centro-sul, lago+rio+ponte a sudeste, solo aberto arável), reaproveitando 100% dos elementos da fazenda antiga **sem regressão**, via o gerador editor — sem editar YAML.
> **Out of scope:** O sistema de arar por tile (Spec B), arte final, balance de crops/economia, novos sistemas de gameplay além de montanha/rio/ponte/quadro de evoluções, save schema (exceto o que a Spec B definir).

required_adrs: []
required_game_rules: []

---

# /speckit.specify

## 5. Contexto

A FarmScene atual foi crescendo por fatias (WAVE 05/07, fable_15/17, GAMEPLAY_EXPANSION_SLICE) e ficou incoerente: campo fixo de 24 canteiros num canto, **19 árvores amontoadas a leste**, entrada da caverna no meio da cena, lotes de expansão (fable_41) sobrepondo zonas de recurso, e `ShippingBin`+`SellPoint` empilhados na mesma posição (3.5, 7.5). O `FarmLevel1LayoutContract` declara 40×32 tiles com âncoras em coordenadas de tile (0–40) que **não correspondem** à cena real (origem centrada em (0,0)).

O usuário revisou o mapa atual (reconstruído do código) e aprovou uma **nova disposição v4** inspirada na fazenda padrão de Stardew Valley:

- **Norte (fundo):** uma **montanha** intransponível fecha a cena; a **entrada da caverna** vai para o **canto noroeste** da montanha; **veios de minério** ficam na base da montanha, **bloqueados** (liberados em progressão posterior).
- **Leste (extrema direita, altura central):** a **saída para a cidade**; ao lado dela, a **base do jogador** — casa (apenas **cama + baú**), Fonte de Anya (respawn), estufa, caixa de envio, ponto de venda, **quadro de evoluções** (novo) e o **nicho de craft** (bancada/forja/fogão, realocados para fora da casa).
- **Noroeste:** um **bosque denso** (madeira) junto à caverna, com **árvores também espalhadas** pelo resto do mapa.
- **Centro-sul:** **construções de animais** (galinheiro, celeiro) e **processamento** (laticínio, vinho), afastadas da casa.
- **Sudeste:** **lago** (pesca) + um **rio** (novo) que desce da montanha até o lago, com uma **ponte de madeira** atravessável.
- **Solo aberto arável em qualquer lugar** (exceto dentro de construções; dentro da estufa é arável) — comportamento entregue pela **Spec B**.

Esta spec **regera a cena** com essa disposição via o gerador editor, preservando todos os elementos antigos.

## 6. Problema

Sem esta spec:
- A fazenda permanece visualmente incoerente e com os defeitos acima (recursos amontoados, caverna no meio, lotes sobrepondo recurso, envio/venda empilhados, contract dessincronizado da cena).
- A disposição v4 aprovada pelo usuário não se materializa.
- Risco de **regressão**: ao regerar a cena, qualquer elemento antigo não reposicionado some (a `Inicializar Projeto` é destrutiva — recria a cena). É exatamente o que o usuário pediu para evitar ("usar os elementos da antiga na nova; ver se não faltou algo").

## 7. Objetivo

Ao final desta spec, ao rodar `CindarsHope/Inicializar Projeto`, a FarmScene deve ser gerada na **disposição v4**, com **todos os elementos da fazenda antiga presentes e funcionais** (sem regressão), os **novos elementos** (montanha com colisão, rio com colisão + ponte andável, veios de minério bloqueados, quadro de evoluções), o `FarmLevel1LayoutContract`/`FarmScaleContract` **reconciliados** com as novas dimensões/posições, e os spawn anchors (`spawn_farm_default`, `spawn_farm_from_town`, `spawn_farm_from_cave`) reposicionados **mantendo seus IDs** — sem editar YAML manualmente e sem alterar save schema (exceto o que a Spec B definir).

## 8. Fontes obrigatórias lidas

```text
docs/project/CURRENT_STATE.md
.specs/SPEC_IMPLEMENTABLE_TEMPLATE.md
.specs/a_implementar/spec_farm_till_anywhere_tilemap.md   (Spec B — dependência)
.claude/rules/testing-quality-gate.md
.claude/rules/unity-architecture.md
.claude/rules/unity-assets.md
.claude/rules/editor-generation-orchestration.md
.claude/rules/id-stability.md
.claude/skills/unity-validation/SKILL.md
.claude/skills/scene-interactable-wiring/SKILL.md
.claude/skills/editor-tooling-orchestration/SKILL.md
Assets/_Game/Scripts/Editor/SceneCreation/CreateMvpFarmScene.cs (auditar — fonte de verdade do layout)
Assets/_Game/Scripts/Farm/FarmLevel1LayoutContract.cs (auditar — reconciliar)
Assets/_Game/Scripts/Farm/FarmScaleContract.cs (auditar)
Assets/_Game/Scripts/Editor/CindarsHopeMenu.cs (onde InicializarProjeto registra os RunStep)
```

## 9. Estado atual do repo

Confirmar tudo na Phase 0 (auditoria). Estado conhecido:

- `CreateMvpFarmScene.cs` — EXISTE (~2106 linhas); gera procedimentalmente a fazenda inteira (spawn, canteiros, recursos, construções, estufa, envio/venda, portais, board do Zrix, Fonte, caverna, debug pickup). É a **fonte de verdade do layout**. **Editar este arquivo** (reposicionar/criar via helpers). NÃO recriar do zero — preservar os helpers de criação de cada elemento.
- `FarmLevel1LayoutContract.cs` / `FarmScaleContract.cs` — EXISTEM; contract com âncoras em **coordenadas de tile (0–40)** dessincronizadas da cena real (origem centrada). **Reconciliar** com a v4.
- Interactables/runtime já existentes — **NÃO recriar**, apenas reposicionar/instanciar no gerador:
  - `CaveEntranceInteractable` (WAVE16, usa `SceneTransitionRouter`), `FishingSpot`, `TreeResource_01`/`RockResource_01`/`ForageResource` interactables (WAVE07), `SellPoint`/`ShippingBin` runtime (WAVE07), Fonte de Anya física + Água Viva + respawn (fable_17, wiring `_anyaFountain`), `CraftingPoint`/`CraftingStation` (WAVE14), `Board_Zrix` (contract board do Zrix), `DebugCarrotSeedPickup`.
- FarmScene `.unity` — regenerada por `CindarsHope/Inicializar Projeto`. **Nunca editar YAML manualmente.**
- 24 `FarmPlot` fixos — serão **removidos do gerador** e substituídos pelo solo arável por tile (Spec B). NÃO remover antes da Spec B existir.
- Lotes de expansão fable_41 (`Lot_North/East/West`) — serão **substituídos** pelo **Quadro de Evoluções**; ver §13 e Riscos.

```text
Estado real precisa ser auditado na Phase 0 antes de implementação. Não recriar sistema existente sem confirmar ausência no repo.
```

## 10. Engineering stories

```text
Como jogador, quero chegar da cidade e cair perto de casa (base a leste), com casa, fonte, envio, venda e craft ali por perto.
Como jogador, quero coletar madeira num bosque de verdade a noroeste, com árvores também espalhadas pelo mapa.
Como jogador, quero a entrada da caverna num canto (noroeste), embutida na montanha do fundo — não no meio da fazenda.
Como jogador, quero atravessar o rio por uma ponte a pé; a água em si me bloqueia.
Como jogador, quero ver os veios de minério na montanha, mas eles só liberam depois (progressão).
Como maintainer, quero que regenerar a fazenda do zero materialize TODOS os elementos antigos (sem regressão) na nova disposição.
Como maintainer, quero que o contract de layout reflita a cena real (origem centrada, novas dimensões), com validação.
```

## 11. Escopo

```text
Inclui:
- Reescrever o layout em CreateMvpFarmScene.cs para a disposição v4 (coordenadas na §15.2), reaproveitando os helpers de criação existentes;
- Reposicionar TODOS os elementos antigos conforme a matriz anti-regressão (§32) — nenhum elemento perdido;
- Realocar os crafting stations (Workbench/Forge/CookingStation) para um nicho de craft fora da casa (a casa fica com cama + baú);
- Adicionar o Baú (chest) dentro da casa (novo, reusando sistema de storage/baú existente se houver; senão, placeholder de storage interactable);
- Criar a montanha como backdrop com COLISÃO (parede intransponível ao norte) + entrada da caverna no canto NO;
- Criar veios de minério como nós BLOQUEADOS (gate de progressão) na base da montanha;
- Criar o rio com COLISÃO (água bloqueia) e uma ponte andável (tile sem colisão sobre o rio);
- Criar o Quadro de Evoluções (interactable) que substitui os lotes fable_41;
- Remover os 24 FarmPlot fixos do gerador (solo arável vem da Spec B); manter estufa com interior arável;
- Reposicionar spawn anchors mantendo IDs (spawn_farm_default, spawn_farm_from_town, spawn_farm_from_cave);
- Reconciliar FarmLevel1LayoutContract/FarmScaleContract com a v4 (origem centrada, novas dimensões 64×44) + validação;
- Atualizar/estender validator(es) de cena da fazenda para checar presença de cada elemento na nova disposição;
- Registrar/garantir o passo no RunStep de InicializarProjeto em CindarsHopeMenu.cs.
```

## 12. Fora de escopo

```text
Não inclui:
- O sistema de arar qualquer terra (Spec B) — esta spec apenas assume o solo arável dela;
- Arte final / sprites definitivos (placeholders OK);
- Balance de crops, economia, drop rates;
- Novos crops/animais além dos já existentes no catálogo;
- Save schema novo (a não ser o já definido pela Spec B);
- Validação humana imediata (deferida ao final do lote);
- Edição manual de QUALQUER YAML de cena/prefab/asset.
```

## 13. Regras de não duplicação

```text
Não recriar CaveEntranceInteractable, SceneTransitionRouter, FishingSpot, SellPoint/ShippingBin, Fonte de Anya, CraftingPoint/CraftingStation, TreeResource/RockResource/ForageResource — reposicionar/instanciar.
Não criar segundo gerador de FarmScene — editar CreateMvpFarmScene.cs.
Não criar [MenuItem] avulso para regenerar a fazenda — usar o RunStep dentro de InicializarProjeto (rule editor-generation-orchestration).
Não criar novo sistema de storage para o baú se já existir um (auditar na Phase 0; reusar).
Não reintroduzir os lotes fable_41 — o Quadro de Evoluções os substitui (decisão a confirmar com humano; ver Riscos).
Não recriar os 24 FarmPlot — o solo arável da Spec B os substitui.
```

## 14. Critérios de aceite

### 14.1 Disposição v4 materializada

- Ao rodar `CindarsHope/Inicializar Projeto`, a FarmScene contém todos os elementos nas zonas da §15.2 (norte=montanha/caverna/minério; leste=base/casa/saída cidade; NO=bosque; centro-sul=animais; SE=lago/rio/ponte).
- Evidência: log do RunStep + screenshot/relatório de inspeção do validator (§29).

### 14.2 Anti-regressão completa

- Todos os elementos da matriz §32 estão presentes e funcionais na nova cena (nenhum sumiu).
- Evidência: validator de cena lista cada elemento esperado e reporta PASS/FAIL por item (§29).

### 14.3 Casa = cama + baú; craft fora da casa

- A casa contém Bed + BedLetter + Baú e NÃO contém crafting stations.
- Workbench/Forge/CookingStation existem num nicho de craft a leste, fora da casa, e continuam funcionais (CraftingPoint).
- Evidência: validator confirma posições e ausência de craft dentro da casa.

### 14.4 Montanha, rio/ponte, minério

- Montanha ao norte tem colliders que bloqueiam o jogador (intransponível).
- Rio tem colisão (água bloqueia) e a ponte tem um caminho andável (sem colisão) cruzando o rio.
- Veios de minério existem como nós bloqueados (não coletáveis até o gate de progressão).
- Evidência: validator de presença + cenário humano de Play Mode (atravessar ponte, bater na montanha, minério bloqueado).

### 14.5 Saída cidade + caverna + spawns reposicionados (IDs estáveis)

- `Portal_Farm_To_Town` na extrema direita (altura central); `CaveEntrance` no canto NO da montanha.
- `spawn_farm_default`, `spawn_farm_from_town`, `spawn_farm_from_cave` reposicionados com os **mesmos IDs**.
- Evidência: validator confirma IDs presentes e coerentes; `ValidateSceneTransitions` (WAVE13) PASS.

### 14.6 Contract reconciliado

- `FarmLevel1LayoutContract` reflete origem centrada e dimensões 64×44 (ou as finais escolhidas), com âncoras coerentes com a cena gerada; métodos de validação passam.
- `FarmScaleContract.IsFarmLevel1SizeValid(64, 44)` retorna true.
- Evidência: EditMode test do contract (§30) + build PASS.

# /speckit.plan

## 15. Arquitetura alvo

### 15.1 Arquivos

```text
Assets/_Game/Scripts/Editor/SceneCreation/
  CreateMvpFarmScene.cs            (EDITAR — layout v4, helpers reusados/estendidos)

Assets/_Game/Scripts/Farm/
  FarmLevel1LayoutContract.cs      (EDITAR — reconciliar origem/dimensões/âncoras)
  FarmScaleContract.cs             (EDITAR se necessário — dimensões novas)

Assets/_Game/Scripts/World/        (NOVOS runtime — namespace CindarsHope.World)
  FarmMountainBackdrop.cs          (opcional: marcador/colisor da montanha)
  RiverBridgeMarker.cs             (opcional: marcador da ponte/água)
  LockedOreNodeInteractable.cs     (nó de minério bloqueado; IInteractable que recusa com feedback até gate)
  FarmEvolutionBoardInteractable.cs (quadro de evoluções; IInteractable)

Assets/_Game/Scripts/Farm/Runtime/ (fecha o gap T006 da Spec B — wiring sem YAML)
  FarmSceneRuntimeBootstrap.cs     (no load da FarmScene: SetBounds no FarmTileGrid; registra zonas não-aráveis
                                    [footprints de construção/água/montanha] + interior da estufa; anexa o input controller)
  FarmTillingInputController.cs    (lê o input da ferramenta — enxada/regador/semente/colher — e chama
                                    FarmTilledSoilService no tile sob/à frente do jogador; reusa o caminho de input existente)

Assets/_Game/Scripts/Editor/Validation/
  ValidateFarmSceneLayoutV4.cs     (NOVO — checa presença de cada elemento + IDs)

Assets/_Game/Scripts/Editor/
  CindarsHopeMenu.cs               (garantir RunStep de regeneração da fazenda)

Assets/_Game/Tests/EditMode/Farm/
  FarmLevel1LayoutContractTests.cs (NOVO/ATUALIZAR — bounds/âncoras)

docs/validation/
  spec_farm_scene_relayout_v4_execution_report.md
docs/validation/playmode/
  spec_farm_scene_relayout_v4_human_playmode_scenario.md
```

### 15.2 Dimensões e coordenadas da disposição v6 (world units, origem centrada, tile = 1 = 32px)

**Tamanho geral (v6):** `x ∈ [-32, 32]` (64 tiles de largura), `y ∈ [-22, 22]` (44 tiles de altura). Norte = +y. Player = 1×1.5 tiles. **Footprint** em tiles (LxA). Coordenadas/footprints são alvo de design; o gerador pode ajustar ±0.5 para encaixe/colisão, registrando no report. Espaçamento mínimo de 1 tile entre construções.

> **Composição v6:** rio corre centro-norte e **DESAGUA no lago** (foz funde na borda NO do lago); **lago ~3× maior** domina o SE; base/casa a leste; bosque+Fonte a oeste; animais centro-sul; **3 áreas de expansão RESERVADAS** (não-aráveis até desbloquear). Ponte na trilha (spawn↔centro) com **vão sem colisor** (atravessável).

```text
ELEMENTO                          CENTRO (x,y)     FOOTPRINT (LxA tiles)   NOTA
NORTE — MONTANHA / CAVERNA / MINÉRIO
  Montanha (backdrop + colisão)   faixa y∈[18,22]  64 x 4                  parede de colisão em y≈18
  CaveEntrance (canto NO, MAIOR)  (-26.0, 18.5)    5 x 4                   boca de caverna grande, embutida na montanha
  Board_Zrix (mural contratos)    (-22.0, 16.0)    1 x 2                   acesso ao contrato (estático)
  spawn_farm_from_cave            (-23.0, 15.0)    ponto
  Veios de minério (LOCKED) x4    (-15,18.5) (-5,18.5) (5,18.5) (15,18.5)  1 x 1 cada

OESTE — BOSQUE + FONTE + RECURSOS + ZRIX
  Bosque (≈30 árvores harvestable) região x∈[-30,-12], y∈[2,17]   DENSO; canopy ~2.5x2.5
  Fonte de Anya (clareira/respawn) (-16.0, 8.0)    clareira ~5x5 não-arável   marco cênico; preservar _anyaFountain + Água Viva
  Zrix (NPC, PERAMBULA devagar)   região centro (-18, 12), raio ~4   NpcWanderer lento; reusa sistema de NPC da cidade
  RockResource_01 (pedra superf.) (-30.0, 6.0)     1 x 1
  TreeResource_01                 absorvido pelo bosque
  Forage points (6) farm_forage_01..06   1 x 1 cada
    (-26,-3) (-24,-3) (-22,-3) (-26,-5) (-24,-5) (-22,-5)
  Árvores espalhadas (≈10)        pelo mapa aberto (evitar construções/água/montanha)

LESTE — BASE / CASA / SAÍDA CIDADE
  House (WALK-IN, como a cidade)  (24.0, 8.0)      7 x 6                   prédio percorrível; porta ao sul (E abre); roof-reveal
    Bed / Baú / BedLetter         DENTRO da casa   —
  Greenhouse (interior arável)    (16.0, 15.0)     6 x 5
  ShippingBin                     (20.0, 3.0)      2 x 1                   separado do SellPoint
  SellPoint                       (16.0, 4.0)      1 x 1
  Quadro de Evoluções             (27.0, 3.0)      1 x 2                   desbloqueia as áreas de expansão
  Nicho de craft (fora da casa):
    Workbench (18,-2) · Forge (20,-2) · CookingStation (22,-2)   1 x 1 cada
  Portal_Farm_To_Town (cidade)    (31.0, 0.0)      2 x 3                   extrema direita
  spawn_farm_from_town            (29.0, 0.0)      ponto
  spawn_farm_default (player)     (14.0, 2.0)      ponto                   em frente à casa, lado leste da ponte
  DebugCarrotSeedPickup           (12.0, 0.0)      prop

CENTRO-SUL — CONSTRUÇÕES
  Coop_01 (galinheiro, 4)         (-12.0, -13.0)   6 x 3
  Barn_01 (celeiro, 4)            (-2.0, -13.0)    7 x 4
  Station_CheesePress             (-13.0, -18.0)   2 x 2
  Station_WineBarrel              (-8.0, -18.0)    2 x 2

ÁGUA — RIO desagua no LAGO (3×)
  Açude (nascente)                (10.0, 18.0)     4 x 3
  Rio (AZUL, largura ~2)          (10,18) → (11,8) → (12,0) → (14,-8) → FUNDE na borda NO do lago (~14,-6)
  Ponte (andável, vão sem colisor) (11.0, 2.0)     3 x 2
  Lago (3× MAIOR, orgânico)       (18.0, -12.0)    ~28 x 17               spans x[4,32], y[-20.5,-3.5]; elipses sobrepostas; foz do rio funde no NO
  FishingSpot (margem O do lago)  (8.0, -8.0)      1 x 1

EXPANSÃO RESERVADA (3) — NÃO-ARÁVEL até desbloquear no Quadro de Evoluções
  Exp_North                       (-6.0, 17.0)     8 x 5                  cercada/hachurada
  Exp_West                        (-30.0, -4.0)    6 x 8
  Exp_South                       (-16.0, -18.0)   8 x 5                  (leste ocupado por base+lago → 3ª foi p/ sul-centro)

CÂMERA
  Orthographic size 8.5; sorting Y_Foot; background tan; bounds cobrindo 64×44.
```

### 15.4 Refinamentos da v6 (a partir da 2ª regeneração / print do usuário)

```text
1. Rio DESAGUA no lago: a foz do rio sobrepõe/funde na borda NO do lago (sem terminar no nada).
2. Arar em QUALQUER lugar EXCETO: footprint de residências/construções E as 3 áreas de expansão
   reservadas. FarmNonArableZones deve registrar as 3 áreas de expansão + construções + água + montanha.
   Greenhouse interior permanece arável.
3. Loadout inicial: garantir ENXADA + REGADOR na inventory/hotbar do jogador no início (para "sacar" e
   arar). Usar o mecanismo de starting-loadout existente (ver Phase 0); FarmTillingInputController usa a
   ferramenta equipada/selecionada.
4. Entrada da caverna MAIOR (~5×4), boca proeminente na montanha.
5. Zrix vira NPC que PERAMBULA devagar pela fazenda (NpcWanderer lento), reusando o sistema de NPC da
   cidade; mantém o acesso ao contrato (Board_Zrix estático ou via o próprio Zrix).
6. Lago ~3× maior (~28×17) no SE; fazenda ampliada para 64×44 para comportar sem espremer.
7. PONTE com vão sem colisor (bug corrigido: a ponte abre passagem real sobre o rio).
8. 3 áreas de expansão RESERVADAS (Exp_North/West/South): cercadas/hachuradas, não-aráveis, viram
   aráveis ao desbloquear no Quadro de Evoluções (estado pode ser stub/NOTE para spec futura de evoluções).
```

### 15.3 Melhorias visuais/UX da v5 (correções a partir da 1ª regeneração)

```text
1. Zone markers NÃO renderizam em jogo. CreateFarmSceneZone usa SpriteRenderer translúcido
   (linha ~899) que aparece como "blocos" coloridos. Mudar para gizmo-only (OnDrawGizmos no
   FarmSceneZoneMarker) ou desligar o SpriteRenderer — manter o marker+collider para lógica.
2. Casa WALK-IN reusando o sistema da cidade: RoofRevealController + HouseDoorInteractable +
   o padrão CreateWalkInHouse (de CreateMvpTownScene). Extrair helper compartilhado OU replicar
   o padrão no gerador da fazenda. Bed + Baú + BedLetter ficam DENTRO do interior percorrível.
3. Rio AZUL (cor de água, não vermelho/laranja) + rota divisor centro-leste (não bisecta o
   centro) + visual dos segmentos ALINHADO aos rects de colisão do FarmSceneRuntimeBootstrap
   (fonte única; sem o "L" desalinhado). Ponte na trilha, cruzando a água de verdade.
4. Lago MAIOR e orgânico (~16x10, elipses sobrepostas) no SE, alimentado pelo rio.
5. Bosque mais DENSO (~28 árvores) e sprites de árvore MAIORES; cluster no NO.
6. Fonte de Anya = marco cênico numa clareira não-arável na borda do bosque (respawn).
```

### 15.5 v7 — Composição por borda (CANÔNICA; supersede as coordenadas da §15.2)

> **Carta de design v7 (aprovada 2026-06-27):** miolo aberto e contíguo; toda estrutura nas bordas; rio é moldura de borda (nunca cruza o centro) e desagua no lago; debris espalhado pelo campo (loop de limpar). Mantém 64×44 (x∈[-32,32], y∈[-22,22]). **Estas coordenadas substituem a §15.2 onde divergirem.**

```text
PRINCÍPIOS (invariantes)
  P1 Miolo central = UM campo aberto contíguo; nada o corta.
  P2 Não-arável (água, montanha, construções, reservas) vive nas BORDAS/cantos.
  P3 Mínimo de zonas; liberdade > estrutura.
  P4 Debris (árvores/pedras/mato) espalhado pelo campo, removível → libera solo.
  P5 Identidade: montanha + caverna-dungeon (canto) + Zrix perambulando.
  P6 Arar em todo o miolo, EXCETO construções e reservas; estufa arável dentro.

NORTE (borda) — montanha y∈[18,22] (64×4, colisão y≈18)
  CaveEntrance (canto NO, GRANDE 5×4)   (-28, 18.5)
  Veios de minério (LOCKED) x4          (-18,18.5) (-9,18.5) (0,18.5) (9,18.5)
  Açude (nascente do rio)               (22, 18)

LESTE (borda) — HOMESTEAD coeso (cluster x∈[24,31])
  House (walk-in)                       (28, 9)    7×6
  Greenhouse                            (24, 10)   5×4   (interior arável)
  ShippingBin (28,4) · SellPoint (24,5) · Quadro Evoluções (30,4)
  Nicho craft: Workbench (25,1) · Forge (27,1) · CookingStation (29,1)
  Portal_Farm_To_Town (cidade)          (31, -1)   2×3
  spawn_farm_default (player)           (24, 3)    (lado homestead da ponte)

RIO (moldura da borda leste) + PONTE
  Rota: açude (22,18) → (21,8) → (20,-2) → (19,-8) → DESAGUA no lago (foz funde na borda N do lago)
  NUNCA cruza o miolo. Ponte (vão sem colisor) (21, 3) liga homestead↔campo.

SUDESTE (canto) — LAGO grande
  Lago centro (18, -13) ~26×14 (spans x[5,31], y[-20,-6]); elipses sobrepostas; foz do rio no NO
  FishingSpot (margem O)                (8, -9)

OESTE (borda) — BOSQUE + FONTE + recursos
  Bosque (≈40 árvores DENSAS)           região x∈[-32,-22], y∈[0,17]
  Fonte de Anya (clareira/respawn)      (-24, 4)   preservar _anyaFountain + Água Viva
  RockResource_01                       (-31, 6)
  Forage (6) farm_forage_01..06         cluster (-31,-4) (-29,-4) (-31,-6) (-29,-6) (-31,-2) (-29,-2)

SUL (borda) — CONSTRUÇÕES de animais (pré-colocadas)
  Coop_01 (-22,-19) · Barn_01 (-13,-19) · CheesePress (-28,-19) · WineBarrel (-8,-19)

CENTRO (sagrado) — campo aberto arável x∈[-18,16], y∈[-15,16]
  DEBRIS espalhado e limpável: ≈28 árvores + ≈12 pedras + ≈10 moitas de mato distribuídas no campo.
  (Total de árvores na cena ≈ 40 bosque + ≈28 campo ≈ 68 — MAIS que o v6.)

EXPANSÃO RESERVADA (3) — nas BORDAS, não-aráveis até desbloquear
  Exp_North (-2, 16.5) 8×4 · Exp_NE (16, 16.5) 7×4 · Exp_West (-30, -10) 5×8

ZONAS (FarmSceneZoneMarker): OnDrawGizmos → trocar para OnDrawGizmosSelected
  (só aparece quando o objeto é selecionado; mapa limpo mesmo com Gizmos ligado no Game view)

CÂMERA: ortho 8.5; Y_Foot; bg tan; bounds 64×44.
```

### 15.6 Delta v7 vs v6 (o que muda na prática)

```text
1. Rio sai do centro → vira borda leste, desaguando no lago (miolo deixa de ser picotado).
2. Construções de animais e 3 reservas → encostadas nas bordas (sul/oeste/norte), fora do miolo.
3. Debris (árvores/pedras/mato) ESPALHADO pelo campo central (loop de limpar) + bosque mais denso.
4. Mais árvores no total (~68) — pedido do usuário.
5. Homestead leste mais COESO (cluster apertado).
6. FarmSceneZoneMarker: OnDrawGizmos → OnDrawGizmosSelected (some o overlay de zonas no Game view).
```

## 16. Contratos, dados e eventos

### 16.1 Data contracts
- `FarmLevel1LayoutContract`: migrar âncoras de coordenadas de tile (0–40) para **world units centrados** (ou documentar explicitamente o mapeamento). Adicionar âncoras novas: MountainBaseY, CaveEntrance(NO), CityExit(E), BridgeCenter, EvolutionBoard. Atualizar dimensões para 64×44.
- `FarmScaleContract`: garantir `IsFarmLevel1SizeValid(64,44)` true; câmera dentro do range.

### 16.2 Runtime contracts
- `LockedOreNodeInteractable`: `IInteractable` que, ao interagir antes do gate, publica `PlayerActionFeedbackEvent` ("Minério bloqueado — requer progressão") e NÃO entrega item. Gate exposto por flag/condição (ex.: `FarmEvolutionState` ou flag de quest) — definição mínima aqui; integração de gate pode ficar como NOTE para spec futura.
- `FarmEvolutionBoardInteractable`: `IInteractable` que abre um painel mínimo (pode ser stub/IMGUI) listando evoluções; integração funcional das compras pode ser deferida (NOTE).
- Montanha/rio: colisão via colliders 2D no gerador; ponte = ausência de colisão num corredor sobre o rio.

### 16.3 Event contracts
- Reusar `PlayerActionFeedbackEvent` (recusa de minério). NÃO criar evento novo salvo necessidade — se criar, registrar em §24.

### 16.4 Save contracts
- Esta spec NÃO altera save schema. (O solo arável/tilling é da Spec B.) Estado do Quadro de Evoluções e do gate de minério: se persistirem, declarar na spec futura de evoluções — aqui ficam em memória / NOTE. **Não serializar referência Unity.**

### 16.5 UI contracts
- Quadro de Evoluções: painel mínimo (stub aceitável) sob ModalManager se abrir modal. Sem retrabalho de HUD.

## 17. Sistemas afetados

```text
Unity scene wiring (FarmScene via gerador)
Farm layout contracts
InteractionSystem (novos IInteractable: minério, quadro de evoluções)
World colliders (montanha, rio, ponte)
Scene transitions (portais + spawn anchors) — WAVE13
Validation (validator de cena da fazenda)
Editor orchestration (RunStep em InicializarProjeto)
```

## 18. Arquivos permitidos

```text
Assets/_Game/Scripts/Editor/SceneCreation/CreateMvpFarmScene.cs
Assets/_Game/Scripts/Farm/FarmLevel1LayoutContract.cs
Assets/_Game/Scripts/Farm/FarmScaleContract.cs
Assets/_Game/Scripts/World/**            (novos runtime World)
Assets/_Game/Scripts/Editor/Validation/ValidateFarmSceneLayoutV4.cs
Assets/_Game/Scripts/Editor/CindarsHopeMenu.cs   (somente registrar/garantir RunStep)
Assets/_Game/Tests/EditMode/Farm/**
docs/validation/**
docs/validation/playmode/**
```

## 19. Arquivos proibidos

```text
Assets/**/*.unity            (NUNCA editar YAML — cena vem do gerador)
Assets/**/*.prefab           (salvo autorização explícita)
Assets/**/*.asset            (salvo data asset explícito autorizado)
Assets/_Game/Scripts/Save/** (sem mudança de save schema nesta spec)
Assets/_Game/Scripts/Editor/SceneCreation/CreateMvpTownScene.cs
docs_old/**  docs/archive/**  Packages/**  ProjectSettings/**
```

## 20. Estratégia de implementação

```md
### Fase 0 — Auditoria
- Mapear no CreateMvpFarmScene.cs cada helper de criação e cada elemento atual (cross-check com a matriz §32).
- Confirmar componentes runtime existentes a reusar (cave entrance, fishing, sell/shipping, fonte, crafting, resources).
- Confirmar se existe sistema de baú/storage reusável; senão, planejar placeholder.
- Confirmar status da Spec B (tilling) — esta spec depende dela para o solo arável.

### Fase 1 — Contracts + novos runtime
- Reconciliar FarmLevel1LayoutContract/FarmScaleContract (origem centrada, 64×44, âncoras v4).
- Criar LockedOreNodeInteractable, FarmEvolutionBoardInteractable e (se necessário) marcadores de montanha/ponte.

### Fase 2 — Gerador (layout v4)
- Reposicionar todos os elementos existentes para as coordenadas §15.2 (reusando helpers).
- Realocar crafting stations p/ nicho fora da casa; adicionar baú na casa.
- Remover criação dos 24 FarmPlot (solo arável vem da Spec B); manter estufa arável.
- Distribuir bosque (≈18) + árvores espalhadas (≈13); reposicionar forage (6) e RockResource_01.
- Criar montanha (colisão), rio (colisão) + ponte (andável), veios de minério (locked), quadro de evoluções.
- Reposicionar portais + spawn anchors (IDs estáveis).

### Fase 3 — Validação
- Criar/estender ValidateFarmSceneLayoutV4 (presença + IDs de cada elemento).
- EditMode tests do contract.
- Build C# (runtime + editor); rodar validators; (regeneração de cena = ação humana no Unity).

### Fase 4 — Relatório + cenário humano
- Execution report + cenário de Play Mode humano (deferido ao final do lote).
```

## 21. Ordem de execucao (ordem segura)

```text
1. Auditar CreateMvpFarmScene.cs + componentes reusáveis + status da Spec B.
2. Reconciliar contracts + criar runtime novos (minério/quadro).
3. Reescrever o layout no gerador (coordenadas §15.2), removendo FarmPlots fixos.
4. Criar/estender o validator de cena da fazenda.
5. Adicionar/atualizar EditMode tests do contract.
6. dotnet build (runtime + editor) exit 0; rodar validators.
7. Registrar RunStep em InicializarProjeto (se ainda não estiver).
8. Gerar execution report + cenário humano de Play Mode.
9. Humano roda CindarsHope/Inicializar Projeto e executa o cenário.
```

## 22. Paralelização

```md
- Parallelizable: NO
- Parallel group: N/A
- Can run with:
  - specs que não tocam FarmScene, gerador da fazenda ou contracts de layout da fazenda
- Must not run with:
  - qualquer spec que edite CreateMvpFarmScene.cs, FarmLevel1LayoutContract.cs, FarmScaleContract.cs, FarmScene ou validators da fazenda
  - Spec B (tilling) se ambas editarem o gerador ao mesmo tempo — sequenciar (B antes de A, ou coordenar locks)
- Shared files/systems that require lock:
  - CreateMvpFarmScene.cs; FarmScene; contracts de layout; CindarsHopeMenu.cs
- Reason:
  - Mesma cena e mesmo gerador; regeneração é destrutiva.
```

## 23. Impacto em save/load

```text
Does this change save schema? NO
Does this add a save section? NO
Does this require migration? NO
Does this persist Unity references? MUST BE NO
```

NOTE: o estado do gate de minério e das compras do quadro de evoluções, se forem persistir, ficam para a spec de evoluções/progressão; aqui em memória/stub. O solo arável e sua persistência são da Spec B.

## 24. Impacto em eventos

```text
Adds events: NO (preferencialmente)
Changes existing events: NO
Requires unsubscribe pattern: YES (para qualquer subscriber novo dos interactables)
```

Reusar `PlayerActionFeedbackEvent` para recusa de minério/evoluções. Se um evento novo for inevitável, listar aqui e justificar.

## 25. Impacto em UI/Unity

```text
Changes UI: YES (painel mínimo do quadro de evoluções — pode ser stub)
Changes scenes: YES (FarmScene via gerador — NÃO via YAML)
Changes prefabs: NO (salvo autorização)
Changes ScriptableObjects/assets: NO (salvo data asset explícito)
Requires Play Mode final validation: YES
Human validation timing: DEFERRED_TO_FINAL_VALIDATION
```

## 26. Riscos técnicos

```text
Risco: regenerar a cena perde elementos antigos (regressão). 
Mitigação: matriz anti-regressão §32 + validator que falha se faltar elemento.

Risco: remover os 24 FarmPlot antes da Spec B → fazenda sem como plantar.
Mitigação: A depende de B; B implementada antes/junto; até lá manter plots ou um starter arável.

Risco: substituir lotes fable_41 pelo Quadro de Evoluções pode conflitar com a spec planejada fable_41.
Mitigação: decisão explícita do humano antes de remover (ver §33); preservar os crops de pomar (apple/cherry/pear/plum) no catálogo.

Risco: contract dessincronizado (tile 0–40 vs world centrado) gerar validação falsa.
Mitigação: reconciliar contract + EditMode test que valida bounds/âncoras reais.

Risco: colisão de montanha/rio mal calibrada bloqueia o jogador onde não deve.
Mitigação: cenário humano de Play Mode (atravessar ponte, perímetro da montanha).

Risco: Fonte de Anya / Água Viva (fable_17) quebrar ao reposicionar (_anyaFountain).
Mitigação: preservar o wiring; validator checa a Fonte e seu respawn.
```

## 27. Rollback

```text
git revert dos arquivos editados (CreateMvpFarmScene.cs, contracts, novos World scripts, validator).
A FarmScene volta à disposição anterior ao rodar Inicializar Projeto com o código revertido.
Não apagar save real do usuário. Não tocar assets de dados.
```

# /speckit.tasks

## 28. Tasks

```md
- [ ] T001 — Auditar CreateMvpFarmScene.cs: mapear helper de cada elemento atual vs matriz §32.
- [ ] T002 — Auditar componentes runtime reusáveis (cave/fishing/sell/shipping/fonte/crafting/resources) + sistema de baú.
- [ ] T003 — Confirmar dependência da Spec B (tilling) e estratégia interina de solo arável.
- [ ] T004 — Reconciliar FarmLevel1LayoutContract (origem centrada, 64×44, âncoras v4).
- [ ] T005 — Ajustar FarmScaleContract se necessário (dimensões/câmera).
- [ ] T006 — Criar LockedOreNodeInteractable (recusa com feedback até gate).
- [ ] T007 — Criar FarmEvolutionBoardInteractable (painel stub).
- [ ] T008 — Criar colisão da montanha + entrada da caverna no canto NO (reusar CaveEntranceInteractable).
- [ ] T009 — Criar rio com colisão + ponte andável.
- [ ] T010 — Reescrever layout no gerador para coordenadas §15.2 (todos os elementos antigos reposicionados).
- [ ] T011 — Realocar crafting stations p/ nicho fora da casa; adicionar baú na casa (cama+baú).
- [ ] T012 — Remover criação dos 24 FarmPlot; manter estufa com interior arável.
- [ ] T013 — Distribuir bosque (≈18) + árvores espalhadas (≈13); reposicionar forage (6) + RockResource_01.
- [ ] T014 — Reposicionar portais (cidade/caverna) + spawn anchors com IDs estáveis.
- [ ] T015 — Criar ValidateFarmSceneLayoutV4 (presença + IDs de cada elemento; FAIL se faltar).
- [ ] T016 — Criar/atualizar EditMode tests do contract.
- [ ] T017 — Garantir RunStep de regeneração da fazenda em InicializarProjeto (CindarsHopeMenu.cs).
- [ ] T018 — dotnet build runtime+editor; rodar validators; registrar evidência.
- [ ] T019 — Gerar execution report + cenário humano de Play Mode.
```

## 29. Validações obrigatórias

```powershell
dotnet build .\Assembly-CSharp.csproj --no-restore
dotnet build .\Assembly-CSharp-Editor.csproj --no-restore
.\tools\docs\validate_docs.ps1
```

Unity (quando o ambiente permitir; senão NOT RUN com motivo):

```powershell
.\tools\unity\RunUnityCompileValidation.ps1 -ProjectPath "." -LogFile ".\Logs\unity-compile-validation.log"
.\tools\unity\ScanUnityLogs.ps1 -LogFile ".\Logs\unity-compile-validation.log"
```

Editor menus (ação humana no Unity):
```text
CindarsHope/Inicializar Projeto   (regera a FarmScene na v4)
CindarsHope/Validar Projeto       (roda validators read-only, incl. ValidateFarmSceneLayoutV4)
```

EditMode:
```text
Unity Test Runner — EditMode (FarmLevel1LayoutContractTests)
```

Se algum comando não puder rodar, o report registra `NOT RUN` com motivo e risco residual.

## 30. Testing Quality Gate

```md
- Changed deterministic logic: YES (contract de layout)
- Requires EditMode tests: YES (FarmLevel1LayoutContractTests — bounds/âncoras/validações)
- Requires PlayMode automated or final human scenario: YES (cenário humano: atravessar ponte, montanha intransponível, minério bloqueado, chegar da cidade perto de casa, caverna no canto, todos os elementos presentes)
- Requires regression test: YES (validator de cena que falha se faltar elemento da matriz §32)
- Human validation timing: DEFERRED_TO_FINAL_VALIDATION
- Minimum validation evidence for ACCEPTED: dotnet build runtime+editor exit 0; EditMode contract tests PASS; ValidateFarmSceneLayoutV4 PASS após regeneração; cenário humano de Play Mode executado.
```

## 31. Definition of Done

```text
Layout v4 implementado no gerador dentro dos arquivos permitidos.
Nenhum YAML/asset proibido alterado.
Matriz anti-regressão §32 100% coberta (validator confirma).
Casa = cama+baú; craft fora da casa; montanha/rio/ponte/minério/quadro presentes.
Contracts reconciliados + EditMode tests PASS.
dotnet build runtime+editor exit 0; docs validation executada (ou NOT RUN com motivo).
RunStep de regeneração garantido em InicializarProjeto.
Execution report + cenário humano de Play Mode criados.
Sem claim de ACCEPTED sem evidência (máx. BUILD_VALIDATED até Play Mode humano).
```

## 32. Anti-regressão — matriz de elementos da fazenda antiga (NENHUM pode sumir)

> O validator (T015) deve checar a PRESENÇA de cada item abaixo na cena regerada.
> **Fonte única de coordenadas/footprints = §15.2 (64×44).** Esta matriz lista apenas presença + destino-zona; NÃO duplica coordenadas (evita drift entre seções). Todos os elementos devem usar as coordenadas da §15.2.

```text
ELEMENTO ANTIGO                         → DESTINO-ZONA (v4)              [coords em §15.2]
Player spawn / spawn_farm_default       → frente da casa (ID mantido)
spawn_farm_from_town                    → junto ao portal da cidade (ID mantido)
spawn_farm_from_cave                    → junto à caverna (ID mantido)
Bed                                     → dentro da casa
BedLetter (fable_63)                    → dentro da casa
Baú (chest) [requisito novo do usuário] → dentro da casa
FonteAnya (respawn + Água Viva, F17)    → base leste — preservar wiring _anyaFountain
Workbench / Forge / CookingStation      → nicho de craft, FORA da casa
24 FarmPlot fixos                       → REMOVIDOS → solo arável por tile (Spec B)
Greenhouse + interior (4 plots)         → leste, interior arável (Spec B)
19 árvores de recurso                   → bosque NO (≈18) + espalhadas (≈16) harvestable
RockResource_01 (pedra)                 → oeste, superfície, sempre disponível
TreeResource_01                         → absorvido pelo bosque
FishingSpot                             → margem do lago (SE)
6 forage points (farm_forage_01..06)    → cluster SW — IDs mantidos
Coop_01 (4) / Barn_01 (4)               → centro-sul
Station_CheesePress / Station_WineBarrel → centro-sul
ShippingBin                             → base leste — SEPARADO do SellPoint
SellPoint                               → base leste — SEPARADO do ShippingBin
Portal_Farm_To_Town                     → extrema direita (ID/target mantidos)
CaveEntrance (CaveEntranceInteractable) → canto NO da montanha
Board_Zrix                              → perto da caverna (NO)
DebugCarrotSeedPickup                   → perto do spawn
Lotes fable_41 (North/East/West)        → REMOVIDOS → substituídos pelo Quadro de Evoluções
Câmera (ortho 8.5 / Y_Foot / tan bg)    → mantida; bounds cobrindo 64×44
Crops: seed_carrot, seed_wheat          → preservados (catálogo)
Crops de pomar: apple/cherry/pear/plum  → preservados no catálogo (mesmo sem lotes)
Animais: Cow/Chicken/Sheep/FantasySmall → preservados (catálogo)

ELEMENTOS NOVOS (v4) — coords/footprints em §15.2
Montanha (backdrop + colisão N)         → faixa norte, profundidade 4
Veios de minério (LOCKED) x4            → base da montanha
Rio (colisão) + Ponte (andável)         → nascente (açude) → lago; ponte atravessável
Açude (nascente)                        → base da montanha
Lago + FishingSpot                      → sudeste
Quadro de Evoluções                     → base leste
```

## 33. Notas para execução posterior

```text
DECISÃO HUMANA TOMADA (2026-06-26): o Quadro de Evoluções SUBSTITUI os lotes fable_41 (Norte/Leste/Oeste). Lotes fixos removidos; crops de pomar (apple/cherry/pear/plum) preservados no catálogo.
Esta spec depende da Spec B (tilling) para o solo arável — sequenciar B antes (ou junto) de A.
Integração funcional das compras do Quadro de Evoluções e do gate de liberação do minério podem ser specs futuras (aqui ficam stub/feedback).
Esta spec não executa validação humana imediata; cria cenário para validação final do lote.
Não atualizar SPEC_EXECUTION_ORDER.md como se já estivesse implementada.
```

## 34. Checklist final da spec pronta

```text
[x] Cabeçalho completo.
[x] Parallelizable / grupo / locks declarados.
[x] Fontes obrigatórias lidas.
[x] Estado atual do repo.
[x] Escopo pequeno e focado (cena/gerador; tilling separado em B).
[x] Fora de escopo explícito.
[x] Regras de não duplicação.
[x] Arquivos permitidos e proibidos.
[x] Contratos/dados/eventos/save/UI.
[x] Critérios de aceite verificáveis.
[x] Validações obrigatórias.
[x] Testing Quality Gate.
[x] Validação humana DEFERRED_TO_FINAL_VALIDATION.
[x] Riscos e rollback.
[x] Matriz anti-regressão (§32).
[x] Não pede execução humana intermediária.
[x] Não altera SPEC_EXECUTION_ORDER.md.
```
