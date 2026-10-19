# Retro-Spec 05 — TownScene/FarmScene v2 (geradores) + Conteúdo de Diálogo Expandido

> **Spec ID:** `spec_retro_05_town_farm_scene_v2_dialogue_content`
> **Status:** RETRO_DOCUMENTED (código implementado em 2026-06; spec escrita a posteriori para reconstrutibilidade)
> **Tipo:** Retro-spec
> **Domínio:** Scene generation (editor) / NPC dialogue content
> **Código que documenta:**
> - `Assets/_Game/Scripts/Editor/SceneCreation/CreateMvpTownScene.cs` (layout v2: distritos, praça, estátua, casas, mercados, árvores)
> - `Assets/_Game/Scripts/Editor/SceneCreation/CreateMvpFarmScene.cs` (24 canteiros, zonas, entrada da caverna)
> - `Assets/_Game/Scripts/NPC/TownNpcDialogueLibrary.cs` (23 NPCs × 13 nós)
> - `Assets/_Game/Scripts/Editor/NpcDialogue/RebuildTownNpcDialogues.cs` (menu de regravação)
> - `Assets/_Game/Scripts/NPC/NpcShopController.cs` (caminho "Conversar" em NPCs de loja)
> **Evidência de execução:** `docs/validation/wave_gameplay_expansion_2026_06_12_execution_report.md` (geradores exigem ação humana no Editor: menus Create Scenes + Rebuild Dialogues)
> **Supersedida/complementada por:** `fable_40` (relayout da cidade 48×42), `fable_41` (expansões de lote da fazenda) — relayouts futuros; `fable_15`/`fable_16`/`fable_17` (RainIrrigation, cama/sono, Fonte/Anya — adições à FarmScene cuja autoridade são essas specs, citadas e não re-documentadas aqui); `fable_28` (condições/pools de diálogo futuros).

---

# /speckit.specify

## Contexto

A TownScene MVP original concentrava NPCs num retângulo central sem leitura urbana, e a FarmScene tinha poucos canteiros e um portal legado para a caverna. A slice 2026-06-12 reescreveu os geradores de cena (editor-only, zero edição manual de YAML — política unity-assets) para: cidade com distritos, praça central com estátua do guerreiro, 12 casas, mercados por vendedor e 24 árvores; fazenda com 24 canteiros em 2 campos e entrada de caverna real (`CaveEntranceInteractable`). Em paralelo, todo o elenco de 23 NPCs ganhou árvore de diálogo padronizada de 13 nós com personalidade, gravada em assets por menu editor, e NPCs de loja ganharam a opção "Conversar" que percorre a árvore.

## Comportamento implementado

### 1. TownScene v2 (`CreateMvpTownScene` — menu `CindarsHope/Create Scenes/Town Scene`)

- **Playfield**: bounds 36×30 (colliders em ±15 y / ±18.5 x); câmera ortográfica size 8; fundo cor sólida `(0.621, 0.609, 0.558)` (sem ground sprite gigante).
- **Distritos** (comentário-mapa no código): templo NW, market row N, praça centro, indústria W, workshop SE, mercado noturno S, estrada da caverna E, pátio de animais NE, portão S. NPCs espalhados em ~±16 × ±12.
- **23 NPCs canônicos** (`RefinedCanonicalTownNpcSpecs`): cada spec define `npcId, objectName, NpcDataSO path, ShopDataSO path (vazio = só diálogo), posição, cor, movementProfile (string), canWander, wanderRadius`. Exemplos reais: `npc_corvus` (-12, 9) Temple, `npc_renko` (0, 7) market row, `npc_hund` patrol radius 5, `npc_yael`/`npc_maelor` NightOnly/WanderHidden, `npc_liora` no jardim da estátua (2.5, -1.5). +1 NPC legado `Npc_Vaalara_Wanderer_01` (radius 6). NPCs sem loja: alaric, maelor, liora (criados via `CreateDialogueNpc`); demais 20 via `CreateShopNpc` com `NpcShopController` totalmente referenciado (shop UI, modal manager, databases).
- **Wander bounds por distrito**: cada NPC wander tem bounds = `posição ± wanderRadius`, clampado a ±17 x / ±13.5 y — roaming local, não retângulo central compartilhado.
- **Placement markers**: todo NPC recebe `NpcScenePlacementMarker` (npcId, sceneId, placementId, movementProfile) para validação WAVE12C/25.
- **Praça central** (`CreateCentralPlaza`): piso 9×9 `(0.69, 0.66, 0.6)`; estátua composta — pedestal 2.2×0.9 com collider sólido, corpo 0.9×1.6 + cabeça 0.5×0.5 em bronze envelhecido, **espada bastarda** (lâmina 0.14×2.3 rotacionada -12° + crossguard), **escudo redondo** 0.85×0.95 com boss central, placa do fundador 1.1×0.3; 4 bancos + 4 floreiras nos cantos.
- **Mercados** (`CreateMarketStalls`): uma barraca por NPC com `ShopDataPath` — balcão 2.1×0.6 com collider + toldo 2.4×0.5 tingido com a cor do vendedor (Lerp 25% para branco), posicionada `posição do NPC + (0, 1.15)`.
- **12 casas** (`TownHouseSpecs`): Temple, Registry, MarketRow A/B/C, Inn, Blacksmith, Archive, Workshop, AnimalYard, AlchemyLab, GateKeeper — corpo 2.6×1.8 com collider, telhado 3×0.7 (Lerp 60% para vermelho-telha), porta 0.5×0.75.
- **24 árvores** (`TownTreePositions`): perímetros W/E (5+5), faixa norte (4), faixa sul (4), clusters internos (6) — escala 3×3, collider sólido.
- **Decorações**: poço, 7 postes de rua (2 tendas do mercado noturno de Yael com poste violeta), rochas da pedreira, pilha de construção, cerca do curral do Eiran.
- **Portais/spawns**: portal sul → FarmScene (`farm_from_town`) em (0, -13.5); spawns `town_default` (0,-5) e `town_from_farm` (0,-11.5).
- **Pip**: recebe `PipReceptionController` adicional (recepção do player).
- Bootstrap completo (managers, databases, save wiring, shop UI canvas com DialogueModal/ShopMenuModal/BuyPanel/SellPanel) é recriado pelo gerador — herdado das waves 02/08/12, não alterado conceitualmente pela slice.

### 2. FarmScene v2 (`CreateMvpFarmScene` — menu `CindarsHope/Create Scenes/Farm Scene`)

- **24 canteiros em 2 campos** dentro da `Zone_CropField` ampliada (centro (1, -2.25), tamanho 9.2×6.2): campo principal 4×4 (16 plots, origem (-3.4, 2)) + campo leste 2×4 (8 plots, origem (2.6, 2)); `FarmPlot_{índice:00}` escala 1.1; plot 0 mantém modo sequencial de smoke test (`seed_carrot`); todos registrados num `FarmPlotRegistry`.
- **Zonas (`FarmSceneZone`)**: PlayerSpawn (0,0), CropField, ResourceTrees (9.5, 1.0; 10×13), ResourceRocks (-9, 5), Forage (-8, -2), LakeFishing (7.8, -2.8), ShippingSellpoint (3.5, 7.5), Construction (-1, 9), HouseEntrance (-5, -7), TownExit (-8.25, -4.75), **CaveEntrance (-5.5, 0; 1.8×2.4)**.
- **Entrada da caverna real** (`CreateCaveEntrance`): boca de caverna visível na Zone_CaveEntrance com componente `CaveEntranceInteractable` (WAVE16/22 — substitui o portal legado; fecha o débito "CaveEntranceInteractable not placed" do backlog WAVE22).
- **NOTA de autoridade**: `CreateRainIrrigation` (fable_15), cama/dormir na casa (fable_16) e Fonte/Anya (fable_17) aparecem no gerador mas foram adicionados por essas specs — citar, não re-documentar.

### 3. Biblioteca de diálogo (`TownNpcDialogueLibrary`)

- Fonte única do conteúdo expandido: **23 blocos `NpcDialogueContent`** (um por NPC canônico), cada um com `Greetings[3]`, `Role1/2`, `Town1/2`, `Service1/2`, `AdviceLines[3]`, `Rumor1/2`, `Goodbyes[3]`, `HasShop`.
- `NodesPerNpc = 13`. Grafo padrão (`BuildNodes`): `node_greeting` (hub com pool aleatório de 3 saudações) → ramos `node_role→node_role_2`, `node_town→node_town_2`, `node_service→node_service_2`, `node_advice` (pool de 3), `node_rumor→node_rumor_2`, `node_hub` ("Mais alguma coisa?"), `node_goodbye` (pool de 3 + `CloseDialogue`), `node_smalltalk` (pool de 3 linhas ambiente). Todo ramo retorna ao `node_hub` via choice "Voltar.".
- Choices do hub (6): "Quem e voce?", "Como vao as coisas na cidade?", "Em que voce trabalha?", "Algum conselho?", "Ouviu algo interessante?", "Adeus.".
- NPCs com `HasShop = true` ganham no `node_service` a choice "Mostre o que voce vende." com `DialogueActionType.OpenShop`.
- Os textos têm personalidade por NPC (estrutura especificada aqui; conteúdo integral vive no código — não transcrever).

### 4. Regravação de assets (`RebuildTownNpcDialogues` — menu `CindarsHope/NPCs/Rebuild Town NPC Dialogues (Expanded)`)

- Para cada conteúdo da biblioteca: localiza o `NpcDataSO` por `NpcId` em `Assets/_Game/Data/NPCs`; cria `DialogueTreeSO` em `Assets/_Game/Data/NPCs/Dialogues/DialogueTree_{npcId}.asset` quando ausente; grava `Id = DialogueSetId`, `StartNodeId = "node_greeting"`, `Nodes = BuildNodes(content)`.
- Usa apenas APIs AssetDatabase (política unity-yaml). Loga `updated/created/missingNpcData`.
- `NpcDialogueSetRegistry` deriva a cobertura DESTA biblioteca (registry e conteúdo nunca divergem).

### 5. Caminho "Conversar" em NPCs de loja (`NpcShopController`)

- Prompt de interação: `"Conversar com {DisplayName}"`.
- Após a saudação, NPC de loja com `DialogueTree` mostra o menu raiz com 4 choices: **Conversar** (entra na árvore autorada a partir de `StartNodeId`), **Comprar**, **Vender**, **Adeus** — o conteúdo WAVE25 ficou alcançável também em vendedores.
- Navegação da árvore mapeia `DialogueChoice` por id; nó não resolvido → warning + fechamento gracioso. Thalindra mantém fluxo especial de quest (WAVE15 — ver retro-spec 09).

## Critérios de aceite (verificáveis no código atual)

1. Gerar TownScene produz: 23 NPCs nas posições do array + 1 wanderer, praça com estátua composta (espada bastarda + escudo), 12 casas, 24 árvores, 1 barraca por NPC vendedor, bounds 36×30, portal sul para FarmScene.
2. Gerar FarmScene produz: 24 `FarmPlot` (16 + 8) registrados no `FarmPlotRegistry`, 11 zonas tipadas, `CaveEntranceInteractable` na Zone_CaveEntrance (sem portal legado de caverna).
3. `TownNpcDialogueLibrary.AllContent` tem 23 entradas; `BuildNodes` retorna exatamente 13 nós com o grafo padrão; todo ramo volta ao hub; `HasShop` injeta choice `OpenShop`.
4. O menu Rebuild grava/cria os 23 `DialogueTreeSO` via AssetDatabase, nunca YAML manual.
5. NPC de loja exibe choices Conversar/Comprar/Vender/Adeus e percorre a árvore autorada.

---

# /speckit.plan

## Arquitetura real

| Arquivo | Responsabilidade |
|---|---|
| `Editor/SceneCreation/CreateMvpTownScene.cs` (~1580 linhas) | Gerador completo da TownScene v2 (bootstrap+layout+NPCs+UI) |
| `Editor/SceneCreation/CreateMvpFarmScene.cs` | Gerador completo da FarmScene v2 (plots+zonas+entrada caverna) |
| `NPC/TownNpcDialogueLibrary.cs` (~790 linhas) | Conteúdo + builder do grafo de 13 nós (fonte única) |
| `Editor/NpcDialogue/RebuildTownNpcDialogues.cs` | Sincronização biblioteca → DialogueTreeSO assets |
| `NPC/NpcShopController.cs` | Fluxo de interação do NPC de loja, incluindo ramo "Conversar" |

## Contratos

- Menus editor: `CindarsHope/Create Scenes/Town Scene` (priority 21), `CindarsHope/Create Scenes/Farm Scene`, `CindarsHope/NPCs/Rebuild Town NPC Dialogues (Expanded)` (priority 40). Todos bloqueiam em Play Mode.
- `TownNpcDialogueLibrary.AllContent : IReadOnlyList<NpcDialogueContent>`; `BuildNodes(content) : List<DialogueNode>`; `NodesPerNpc = 13`.
- `TownNpcSpec` (struct interna do gerador): npcId, objectName, dataPath, shopPath, position, color, movementProfile, canWander, wanderRadius.
- Cenas gravadas em `Assets/_Game/Scenes/TownScene.unity` / `FarmScene.unity`; diálogos em `Assets/_Game/Data/NPCs/Dialogues/`.
- Identificadores estáveis: `node_greeting`, `node_hub`, `node_goodbye` etc.; `dialogue_{npc}` como DialogueSetId.

## Decisões e invariantes

- **Cena é sempre regenerável**: nenhum estado manual nas cenas; gerar de novo reproduz o layout (idempotência por construção).
- **Zero YAML manual** (regra unity-assets): geradores usam `SerializedObject`/`AssetDatabase` exclusivamente.
- **Fonte única de diálogo**: registry e validadores leem `TownNpcDialogueLibrary` — impossibilita drift entre documentação de cobertura e conteúdo.
- **Ação humana obrigatória**: regenerar as 2 cenas + rodar o menu Rebuild no Editor antes do Play Mode (registrado no report).
- **Autoridade compartilhada**: elementos da FarmScene de fable_15/16/17 e o relayout futuro de fable_40/41 pertencem àquelas specs.

---

# /speckit.tasks

## Reconstrução (passos para refazer do zero)

1. Recriar `CreateMvpTownScene` com: bootstrap/managers (waves 02+), player, bounds 36×30, câmera size 8, spawns/portal sul, e as tabelas literais `RefinedCanonicalTownNpcSpecs` (23), `TownHouseSpecs` (12), `TownTreePositions` (24) + praça/estátua/mercados/decorações com as dimensões da seção de comportamento.
2. Recriar `CreateMvpFarmScene` com 24 plots (4×4 + 2×4, espaçamento ~1.4), as 11 zonas tipadas e `CreateCaveEntrance` com `CaveEntranceInteractable`.
3. Recriar `TownNpcDialogueLibrary` com o modelo `NpcDialogueContent`, o grafo de 13 nós e os 23 blocos de conteúdo (escrever textos com a personalidade de cada NPC; estrutura é o contrato).
4. Recriar `RebuildTownNpcDialogues` (AssetDatabase only) e `NpcDialogueSetRegistry` derivado da biblioteca.
5. Adicionar ao `NpcShopController` o menu raiz Conversar/Comprar/Vender/Adeus com navegação da árvore.
6. Rodar os 3 menus no Unity Editor e validar com `ValidateWave25TownNpcSchedulesDialogue`.

## Débitos conhecidos

- Tudo é placeholder de sprite builtin — arte final substituirá casas/estátua/árvores/NPCs.
- Diálogos não têm condições/pools contextuais (fable_28) nem variação por relacionamento.
- Relayout 48×42 da cidade (fable_40) e expansão de lotes da fazenda (fable_41) substituirão parcialmente o layout v2.
- `TownNpcDialogueLibraryTests` cobre estrutura; o conteúdo textual em si não tem teste (aceito — conteúdo autoral).
- Geradores recriam a cena inteira (sem patch incremental) — mudanças manuais na cena são perdidas ao regenerar (por design).
