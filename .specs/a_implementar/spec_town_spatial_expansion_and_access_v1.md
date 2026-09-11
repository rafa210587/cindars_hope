# SPEC — Town Spatial Expansion and Access v1

> **Spec ID:** `spec_town_spatial_expansion_and_access_v1`
> **Status:** DEFERRED_TO_FINAL_HUMAN_VALIDATION — Unity/PlayMode/AC12 independente PASS; cenário humano final pendente
> **Wave:** TOWN EXPANSION V1
> **Priority:** P0
> **Type:** Integration / Editor scene generation / Validation
> **Domain:** City / TownScene
> **Parallelizable:** NO
> **Parallel group:** N/A
> **Can run with:** trabalho offline de arte que não edite Assets nem contratos Town
> **Must not run with:** `spec_town_keyart_fidelity_v1`, `spec_town_components_doors_and_collision_v1` ou qualquer regeneração Farm/Town que compartilhe Unity/Assets
> **Repo lock scope:** `Assets/_Game/Scripts/Editor/SceneCreation/**Town**`, `Assets/_Game/Scripts/Editor/SceneCreation/City/**`, validators/tests Town, `TownScene.unity`, evidência Town
> **Depends on:** `ref_town_spatial_expansion_and_access_v1`; baseline materializada `revision-06-scale20-r2`
> **Blocks:** fechamento de `spec_town_keyart_fidelity_v1` e execução final de `spec_town_components_doors_and_collision_v1`
> **Scope:** expandir Town 120×90→160×112 e relayoutar conteúdo preservado com contratos verificáveis de circulação, entrada e viagem
> **Out of scope:** nova arte, novo upscale, mecânica de portas, redesign de interiores, schedule/economia/save
> **Validation level alvo:** PLAYMODE
> **Executor:** qualquer; execução serial com revisor visual independente

**Ordem de execucao:** `TOWN.P0`; antes do passe visual final e da integração definitiva de portas/componentes  
**Depende de:** `ref_town_spatial_expansion_and_access_v1` e baseline materializada `revision-06-scale20-r2`  
**Bloqueia:** fechamento de `spec_town_keyart_fidelity_v1` e execução final de `spec_town_components_doors_and_collision_v1`  

required_adrs: []
required_game_rules: []

# /speckit.specify

## 5. Contexto

A revisão06 aumentou as construções em 20% e preservou a física, mas evidenciou que o footprint atual
não reserva o envelope visual nem o encontro Player×NPC. A cena salva permite passagem individual, porém
há footprints separados por apenas 2u, rotas locais de 2u e marcos próximos da borda. Esta spec amplia
o mapa sem aumentar novamente os prédios e transforma conforto de circulação em contrato testável.

## 6. Problema

`TownKeyartPhysics 187/187` usa raio0,35 e prova apenas conectividade estática individual. Ele não impede
silhuetas sobrepostas, não garante apron de porta, passagem bidirecional ou rota interior. Reencaixar os
prédios em 120×90 já produziu conflitos Hall/Alchemy/Blacksmith e um anchor bloqueado durante a última
rodada. Sem uma expansão governada, novos ajustes continuarão trocando um bloqueio por outro.

## 7. Objetivo

Ao final, a Town materializada possui bounds exatos 160×112, os 24 prédios e todos os NPCs/IDs atuais
preservados, vias dimensionadas por classe, entradas e anchors navegáveis com margem de conforto e
distâncias de viagem limitadas, sem alterar gameplay, arte ou escala visual1,20.

## 8. Fontes obrigatórias

- `docs/refinements/a_implementar/ref_town_spatial_expansion_and_access_v1.md`
- `docs/art_catalog/_reference_keyart_GPT/town_keyart_layout_chatgpt_web_candidate_v2.png`
- `art/town-keyart-rework/evidence/revision-06-scale20-r2/inputs.json`
- `art/town-keyart-rework/evidence/revision-06-scale20-r2/town-comparable45.png`
- `docs/validation/town_keyart_deep_reassessment.md`
- `Assets/_Game/Scripts/Editor/SceneCreation/TownDistrictLayout.cs`
- `Assets/_Game/Scripts/Editor/SceneCreation/City/TownCityLayout.cs`
- `Assets/_Game/Scripts/Editor/SceneCreation/City/TownKeyartGeometry.cs`
- `Assets/_Game/Scripts/Editor/SceneCreation/City/TownKeyartBuildingArt.cs`
- `Assets/_Game/Scripts/Editor/SceneCreation/CreateMvpTownScene.cs`
- `Assets/_Game/Scripts/Editor/Validation/ValidateTownKeyartScene.cs`
- `Assets/_Game/Scripts/Editor/Validation/TownKeyartLivePhysicsProbe.cs`
- `Assets/_Game/Tests/EditMode/City/Editor/TownLayoutTests.cs`
- `Assets/_Game/Tests/EditMode/City/Editor/TownKeyartGeometryTests.cs`
- `.claude/rules/visual-iteration-budget.md`
- skills: `tilemap-world-rendering`, `sprite-scene-integration`, `editor-tooling-orchestration`, `editmode-test-authoring`, `unity-validation`, `gameplay-test-scenario`

## 9. Estado atual do repo — Phase 0 auditada em 2026-09-10

```text
Town bounds: 120x90, HalfWidth=60, HalfHeight=45.
VisualEnlargement: 1.20; não aplicado aos footprints físicos.
Conteúdo materializado: 24 prédios, 29 NPCs, 84 anchors, 23 NPC stalls,
6 market stalls, 2 spawns, 554 árvores.
Menor gap físico: Blacksmith–Tannery=2.0u.
Margem mínima à borda: AnimalYard=3.0u.
Vias: principais 3.0–3.4u; east_gate 2.2u; door links 2.0u.
PlayerData.MoveSpeed=5u/s.
EditMode City: 271/271 PASS.
Saved-scene: TownKeyartPhysics PASS:187 FAIL:0; CitySchedule PASS:21 FAIL:0.
PlayMode de encontro/entrada: NOT RUN.
Cena SHA256: 20E53DE2D91F829F3E70FE0D76C94BC538774BAA2F1218D83274292B8E8684B0.
Overview SHA256: 76339BA3F10343D01A6C443FA86C3ED88EEDDEBED0033D48F81CBCD9D6F5E9E6.
```

Comandos usados: extração dos `Lot(...)` de `TownCityLayout.cs`; leitura das larguras em
`TownKeyartGeometry.BuildRoads`; `Logs/town_keyart_revision06_scale20_r2.log`; hash SHA256 da cena e
capture. A Phase0 da execução deve repetir censo, hashes, gaps, larguras e status do lock antes de editar.
Se os números divergirem materialmente, parar e fazer amendment.

## 10. Requisitos funcionais

- **RQ01:** `TownDistrictLayout` passa a 160×112 (`±80`, `±56`).
- **RQ02:** os 24 `House_*`, TownHall, 29 NPCs, 84 anchors, stalls, spawns e portais mantêm IDs/função.
- **RQ03:** `VisualEnlargement` permanece 1,20 e os sprites não recebem novo upscale.
- **RQ04:** vias livres: avenida≥4,0u, coletora≥3,2u, local/porta≥2,4u.
- **RQ05:** cada porta pública possui apron livre4×3u, vão≥1,4u e acesso com probe raio0,50u.
- **RQ06:** cada interior walk-in tem faixa porta→área útil≥1,4u e pocket livre2×2u.
- **RQ07:** footprints não anexos mantêm gap≥4,0u; exceções possuem ID/motivo explícito.
- **RQ08:** envelopes visuais não ocultam porta, escada ou mais de5% da área opaca de outro prédio.
- **RQ09:** todos os work anchors e destinos públicos são livres e alcançáveis com probe raio0,50u.
- **RQ10:** spawn sul→praça≤75u; praça→cada porta pública≤90u pelo grafo materializado.
- **RQ11:** Player e NPC real atravessam os três clusters de maior fluxo sem contato bloqueante.
- **RQ12:** chão, muralha, água, floresta e câmera cobrem os novos bounds sem lacuna/corte.
- **RQ13:** mínimo materializado de árvores é o censo real before; atualmente 554.

## 11. Cenários observáveis

1. Do spawn sul, o Player chega à praça pela avenida4u enquanto um NPC vem no sentido oposto.
2. Na praça/mercado, Player contorna stalls e entra em Bakery/MarketHall sem colisão com fila/props.
3. No distrito de ofícios, Player cruza Blacksmith↔Alchemy enquanto Brumdar/Ozzra chegam ao trabalho.
4. Player chega a Temple, Hall, lago/dock, AnimalYard e saída leste sem passar por gramado bloqueado.
5. Em três casas e três serviços, Player atravessa porta, gira no interior e alcança estação/saída.

## 12. Fora de escopo detalhado

- gerar/editar sprites, Aseprite, paleta ou resolução;
- implementar frames/estado de porta ou alterar `HouseDoorInteractable`/`RoofRevealController`;
- mudar escala do Player/NPC, velocidade, input ou câmera gameplay ortho8;
- mudar horários, comportamento AI, conteúdo de loja, diálogo, save ou eventos;
- aumentar para 168×112 sem amendment humano;
- remover árvores/props/NPCs/prédios para satisfazer navegação.

## 13. Regras de não duplicação

- `TownDistrictLayout` continua fonte única dos bounds/distritos; não criar segundo mapa.
- `TownCityLayout` continua fonte única de lotes, vias e destinos NPC; não duplicar JSON/SO.
- `CreateMvpTownScene` continua gerador canônico; não criar gerador paralelo.
- Reusar `TownKeyartGeometry`, `ValidateTownKeyartScene` e `TownKeyartLivePhysicsProbe`.
- Reusar `HouseDoorInteractable`, `RoofRevealController`, `NpcScheduleAnchor` e spawns existentes.

# /speckit.plan

## 15. Arquitetura alvo — CREATE vs MODIFY

```text
CRIAR
Assets/_Game/Scripts/Editor/SceneCreation/City/TownAccessMetrics.cs
  Constantes e cálculos puros de apron, classe de via, gap, envelope e path budget.
Assets/_Game/Tests/EditMode/City/Editor/TownSpatialExpansionTests.cs
  Dez testes determinísticos dos requisitos RQ01–RQ10/RQ12/RQ13.

MODIFICAR
TownDistrictLayout.cs
  Bounds 160×112 e retângulos dos sete distritos.
TownCityLayout.cs
  Centros dos 24 lotes, footprints somente quando necessário para interior, anchors/stalls derivados.
TownKeyartGeometry.cs
  Rotas principais/coletoras/locais e grafo sob larguras novas.
TownKeyartGround.cs
  Rasterizar novos bounds e vias sem retângulos desconectados.
TownKeyartSceneArt.cs / TownKeyartSetDressing.cs
  Reposicionar lago, praça, props e suportes fora dos clear envelopes.
CreateMvpTownScene.cs
  Muralha, portões, chão, floresta, landmarks, wander clamps e censo160×112.
ValidateTownKeyartScene.cs
  Comfort probe0,50, aprons, interiores, envelopes, path budgets e resumo TownAccess.
TownKeyartLivePhysicsProbe.cs
  Cenários Player×NPC reais nos três clusters e seis acessos.
TownSceneCapture.cs
  Overview ortho58 e recortes de acesso nas novas coordenadas.
TownLayoutTests.cs / TownKeyartGeometryTests.cs
  Atualizar contrato dimensional e larguras sem enfraquecer preservação.
TownKeyartBuildingScaleTests.cs
  Atualizar somente as duas expectativas dimensionais 120×90→160×112; manter o gate de escala1,20.
TownScene.unity
  Somente output do gerador Unity.
```

## 16. Contratos propostos

### 16.1 Métricas editoriais

```csharp
public enum TownRoadClass { Main, District, Local }

public static class TownAccessMetrics
{
    public const float ActorBodyRadius = 0.35f;
    public const float ComfortProbeRadius = 0.50f;
    public const float MainRoadMinWidth = 4.0f;
    public const float DistrictRoadMinWidth = 3.2f;
    public const float LocalRoadMinWidth = 2.4f;
    public static readonly Vector2 DoorApronSize = new(4.0f, 3.0f);
    public const float InteriorLaneWidth = 1.4f;
    public static readonly Vector2 InteriorTurnPocket = new(2.0f, 2.0f);
    public const float BuildingGap = 4.0f;
    public const float SouthSpawnToPlazaMaxPath = 75.0f;
    public const float PlazaToPublicDoorMaxPath = 90.0f;

    public static Rect DoorApron(TownBuildingLot lot);
    public static bool ClearsApron(Rect apron, IReadOnlyList<TownBuildingLot> lots);
    public static float ShortestPathLength(Vector2 start, Vector2 target,
        IReadOnlyList<TownRoadSegment> roads, float actorRadius);
    public static bool ObscuresCriticalFacade(Rect sourceOpaque, Rect targetOpaque,
        Rect targetDoorAndSteps, float maxOpaqueFraction = 0.05f);
}
```

`Rect`/AABB é broad phase. O validator de cena confirma bounds opacos reais dos renderers e a região
porta+escada. Exceções intencionais usam uma lista local de pares com ID e motivo; lista vazia é o alvo.

### 16.2 Layout

```csharp
public const float HalfWidth = 80f;
public const float HalfHeight = 56f;
public const float WidthTiles = 160f;
public const float HeightTiles = 112f;
```

O seed inicial das posições é `x'=x*4/3`, `y'=(y-4)*(56/45)+4`. Ele não é contrato persistido nem
substitui a auditoria; posições finais permanecem explícitas no layout.

### 16.3 Runtime, eventos e save

N/A para contratos novos. Não criar runtime service, evento ou schema de save. Componentes e IDs
existentes são preservados. Toda lógica nova desta spec é Editor/test/validation.

## 17. Sistemas afetados

Editor scene generation; Tilemap/ground; Town layout/roads; physics supports; NPC anchor placement;
saved-scene validation; PlayMode probe; visual capture. Player runtime, NPC schedule e save são somente
consumidores preservados.

## 18. Arquivos permitidos

```text
Assets/_Game/Scripts/Editor/SceneCreation/TownDistrictLayout.cs
Assets/_Game/Scripts/Editor/SceneCreation/CreateMvpTownScene.cs
Assets/_Game/Scripts/Editor/SceneCreation/City/TownAccessMetrics.cs
Assets/_Game/Scripts/Editor/SceneCreation/City/TownCityLayout.cs
Assets/_Game/Scripts/Editor/SceneCreation/City/TownKeyartGeometry.cs
Assets/_Game/Scripts/Editor/SceneCreation/City/TownKeyartGround.cs
Assets/_Game/Scripts/Editor/SceneCreation/City/TownKeyartSceneArt.cs
Assets/_Game/Scripts/Editor/SceneCreation/City/TownKeyartSetDressing.cs
Assets/_Game/Scripts/Editor/Validation/ValidateTownKeyartScene.cs
Assets/_Game/Scripts/Editor/Validation/TownKeyartLivePhysicsProbe.cs
Assets/_Game/Scripts/Editor/Dev/TownSceneCapture.cs
Assets/_Game/Tests/EditMode/City/Editor/TownLayoutTests.cs
Assets/_Game/Tests/EditMode/City/Editor/TownKeyartGeometryTests.cs
Assets/_Game/Tests/EditMode/City/Editor/TownKeyartBuildingScaleTests.cs
Assets/_Game/Tests/EditMode/City/Editor/TownSpatialExpansionTests.cs
Assets/_Game/Scenes/TownScene.unity (gerado via Unity)
art/town-keyart-rework/evidence/town-expansion-v1/**
docs/validation/town_spatial_expansion_and_access_v1_execution_report.md
docs/validation/playmode/town_spatial_expansion_and_access_v1_human_test_scenario.md
```

## 19. Arquivos proibidos

```text
Assets/_Game/Scripts/World/HouseDoorInteractable.cs
Assets/_Game/Scripts/World/RoofRevealController.cs
Assets/_Game/Scripts/Player/**
Assets/_Game/Scripts/NPC/**
Assets/_Game/Scripts/Save/**
Assets/_Game/Data/**
Assets/_Game/Art/** (nenhuma nova arte/import nesta spec)
Assets/_Game/Scenes/FarmScene.unity
Assets/_Game/Scenes/CaveScene.unity
Packages/**
ProjectSettings/**
docs_old/**
qualquer .unity/.prefab/.asset editado manualmente como YAML
```

## 20. Estratégia de implementação por edição

### Fase 0 — freeze

1. Recalcular SHA da cena/capture e censo materializado; copiar snapshot para evidence.
2. Rodar o audit atual e registrar gaps, margens e widths antes.
3. Confirmar lock exclusivo Unity+lh/assets Town; divergência material bloqueia execução.

### Fase 1 — contrato antes do relayout

1. Criar `TownAccessMetrics` com os valores da §16.
2. Criar os dez testes novos inicialmente contra o estado alvo; confirmar falhas esperadas de bounds,
   widths, gap e comfort access, sem alterar produção para maquiar fixtures.
3. Em `TownLayoutTests`, trocar apenas expectativas dimensionais 120×90→160×112 e piso de árvores
   constante pelo censo before congelado.

### Fase 2 — bounds, distritos e lotes

1. Em `TownDistrictLayout`, alterar half extents e redimensionar os sete districts.
2. Em `TownCityLayout.Buildings`, aplicar o seed §16.2 e corrigir manualmente por distrito até todos os
   gaps físicos e envelopes visuais passarem. Não mudar IDs/archetypes.
3. Manter praça em `(0,4)`; posicionar Temple/Hall primeiro, depois lago/curral, serviços e residências.
4. Alterar footprint de lote somente se o interior atual não satisfizer RQ06; sprite/escala não muda.

### Fase 3 — vias, entradas e destinos

1. Em `TownKeyartGeometry.BuildRoads`, declarar cada route class e aplicar 4,0/3,2/2,4u.
2. Em `BuildDoorConnection`, usar LocalRoadMinWidth e ComfortProbeRadius; conexão precisa conservar
   disco varrido completo e ligar ao grafo principal.
3. Em `TownCityLayout.Npc`/resolvers, derivar work/stall do novo apron; social permanece em clusters
   públicos sem ocupar largura livre.
4. Pseudocódigo:

```text
place landmarks and reserved water/plaza areas
for each district in dependency order:
  seed lot position from revision06
  reject if wall margin, building gap or visual critical facade fails
  reserve DoorApron and interior exit lane
build classed road graph around reservations
for each door/work/spawn:
  require shortest path with radius0.50
  require path budget where applicable
```

### Fase 4 — materialização

1. Em `CreateMvpTownScene`, expandir ground/walls/forest/wander clamps e reposicionar spawns/portais.
2. Em `TownKeyartGround`, rasterizar roads/grass para ±80/±56.
3. Em SceneArt/SetDressing, mover água/props/árvores afetados; descartar candidato de prop que ocupe
   apron/clear lane, sem apagar a família baseline.
4. Regenerar TownScene por `GenerateValidateAndCaptureRevision`; nenhum YAML manual.

### Fase 5 — validação e revisão

1. `ValidateTownKeyartScene` mantém 187 checks legados e emite resumo agregado novo `TownAccess`.
2. `TownKeyartLivePhysicsProbe` executa oito cenários com Player/NPC reais, sem teleporte persistido.
3. `TownSceneCapture` usa overview ortho58 e details ortho8; preservar reference e revision06 lado a lado.
4. Revisor independente avalia apenas escala urbana, separação e legibilidade; fidelidade global continua
   pertencendo a `spec_town_keyart_fidelity_v1`.

### Fase 6 — closeout

1. Criar execution report e human scenario via `gameplay-test-scenario`.
2. Registrar resultados reais PASS/FAIL/NOT RUN; `/finish-spec` somente após gates aplicáveis.

## 21. Ordem segura

```text
T001 freeze → T002 métricas → T003 testes → T004 bounds/districts → T005 landmarks/lotes
→ T006 vias/aprons → T007 anchors/stalls → T008 ground/walls/water/forest
→ T009 regeneração → T010 EditMode/saved scene → T011 PlayMode → T012 visual review/report
```

## 22. Critérios de aceite binários

### AC01 — Bounds e escala
- Resultado: Town=160×112 e `VisualEnlargement=1.20`.
- DoD: geração imprime `Footprint: 160x112 (bounds +/-80/+/-56)`.
- Teste: `TownBounds_Are160By112_AndBuildingScaleRemains120Percent`.

### AC02 — Preservação
- Resultado: 24 casas,29NPCs,84anchors,23stallsNPC,6market stalls,2spawns,≥554 árvores.
- DoD: manifesto after imprime `lostStableIds=0; lostReferences=0; trees>=554`.
- Teste: `Expansion_PreservesMaterializedBaselineCountsAndIds`.

### AC03 — Espaço entre prédios e borda
- Resultado: gap físico≥4u e envelope crítico sem oclusão; margem visual à muralha≥3u.
- DoD: validator imprime `[TownAccess] buildingGap=PASS visualFacade=PASS wallMargin=PASS`.
- Teste: `Buildings_KeepFourUnitGap_AndCriticalFacadesRemainVisible`.

### AC04 — Hierarquia de vias
- Resultado: main≥4, district≥3,2, local≥2,4.
- DoD: validator imprime `[TownAccess] roads=PASS mainMin=4 districtMin=3.2 localMin=2.4`.
- Teste: `RoadClasses_RespectMinimumClearWidths`.

### AC05 — Entradas e interiores
- Resultado: 24 aprons4×3 livres; 23 interiores walk-in com lane1,4/pocket2×2; AnimalYard com acesso externo.
- DoD: validator imprime `[TownAccess] doors=24/24 interiors=23/23 yard=PASS`.
- Teste: `DoorsHaveClearAprons_AndWalkInInteriorsHaveLaneAndTurnPocket`.

### AC06 — NPCs e conectividade
- Resultado: 28 work anchors livres/alcançáveis com raio0,50; todos spawns/doors no mesmo componente.
- DoD: validator imprime `[TownAccess] comfortRadius=0.5 work=28/28 doors=24/24 spawns=2/2 failures=0`.
- Teste: `ComfortProbe_ReachesEveryDoorWorkAnchorAndSpawn`.

### AC07 — Orçamento de viagem
- Resultado: spawn sul→praça≤75u; praça→cada porta≤90u.
- DoD: validator imprime `[TownAccess] travelBudget=PASS southToPlaza<=75 maxPlazaToDoor<=90`.
- Teste: `ShortestPaths_StayWithinTownTravelBudgets`.

### AC08 — Água, borda e portais
- Resultado: água não ocupa rotas, ground cobre mapa, portão sul/acesso oeste/saída leste funcionam.
- DoD: geração imprime `LakeWater audit: lotes=0 vias=0` e `[TownAccess] portals=3/3 ground=PASS`.
- Teste: `ExpandedGroundWaterWallsAndPortalsStayCoherent`.

### AC09 — Gate EditMode
- Resultado: baseline271 + 10 testes novos, todos verdes.
- DoD: `RunUnityEditModeTests.ps1 -TestFilter CindarsHope.Tests.EditMode.City` imprime
  `UNITY_EDITMODE: PASS; total=281; passed=281; failed=0`.

### AC10 — Gate saved scene
- Resultado: checks legados e novos sem falhas.
- DoD: log contém `[TownKeyartPhysics] PASS:187 FAIL:0` e `[TownAccess] PASS:8 FAIL:0`.

### AC11 — Gate PlayMode
- Resultado: oito trajetos/encontros usam Player e NPC reais sem contato bloqueante.
- DoD: log contém `[TownAccessPlayMode] PASS:8 FAIL:0`; o `capture-metadata.json` produzido pelo
  runner canônico existente contém `sceneUnchanged=true` e `savesUnchanged=true`, com hashes before/after.

### AC12 — Evidência visual
- Resultado: overview1536×1024 ortho58 e recortes ortho8 mostram fachadas/entradas sem corte grave.
- DoD: pasta `art/town-keyart-rework/evidence/town-expansion-v1/final/` contém overview, seis details,
  collider overlays, inputs e hashes; parecer independente registra `ACCEPT_EXPANSION` ou a spec falha.

## 23. Edge cases e falhas

- Visual envelope maior que footprint: reservar bounds opacos e região porta/escada; não reduzir sprite.
- AABB acusa telhados em perspectiva: somente exceção catalogada com par/motivo e porta totalmente visível.
- Anchor euclidiano próximo mas rota longa: usar shortest path do grafo, não distância reta.
- Player passa e NPC trava: PlayMode exige corpo real dos dois em sentidos opostos.
- Expansão cria vazio: travel budgets e revisão visual falham; não preencher lane com props.
- Árvore/prop bloqueia rota após materialização: reposicionar dentro do distrito, sem remover o baseline.
- Capture corta muralha: ajustar apenas câmera de overview; gameplay ortho8 permanece.
- Geração falha após SaveScene: preservar snapshot/hash before e não aceitar cena parcial.
- 160×112 insuficiente: parar e solicitar amendment; não adotar 168×112 automaticamente.

## 24. Validação e gates

```powershell
pwsh -File tools/unity/RunUnityEditModeTests.ps1 -TestFilter CindarsHope.Tests.EditMode.City
```

Unity sequencial, com dispositivo gráfico:

```text
CindarsHope.Editor.Dev.TownSceneCapture.GenerateValidateAndCaptureRevision
TownKeyartLivePhysicsProbe / runner canônico de PlayMode
```

Também registrar `git diff --check`, scan de log e hashes before/after. PlayMode humano usa cenário em
`docs/validation/playmode/town_spatial_expansion_and_access_v1_human_test_scenario.md` e permanece
`DEFERRED_TO_FINAL_VALIDATION` até execução real.

# /speckit.tasks

## 25. Tasks rastreáveis

- [x] **T001** — Freeze de manifesto, hashes, captura, censo, gaps e lock. Cobre AC02/AC12.
- [x] **T002** — Criar `TownAccessMetrics` com contratos §16. Cobre AC03–AC08.
- [x] **T003** — Criar dez `TownSpatialExpansionTests`; registrar falhas pré-relayout. Cobre AC01–AC09.
- [x] **T004** — Alterar bounds160×112 e sete districts. Cobre AC01/AC08.
- [x] **T005** — Posicionar landmarks e 24 lotes com gap/envelope/margem. Cobre AC02/AC03.
- [x] **T006** — Recriar vias4,0/3,2/2,4 e 24 aprons conectados. Cobre AC04–AC07.
- [x] **T007** — Recalcular 84 anchors, stalls e destinos sem mudar IDs/schedule. Cobre AC02/AC06/AC07.
- [x] **T008** — Expandir ground, wall, água, floresta≥554 e reposicionar props. Cobre AC02/AC08.
- [x] **T009** — Regenerar TownScene uma vez e produzir captures/hashes. Cobre AC10/AC12.
- [x] **T010** — Corrigir somente falhas objetivas da primeira integração e rodar 281 EditMode + saved scene. Cobre AC01–AC10.
- [x] **T011** — Executar oito probes PlayMode Player×NPC/entradas; nenhuma correção runtime fora da spec. Cobre AC11.
- [x] **T012** — Revisão visual independente, human scenario e execution report; rodar `/finish-spec`. Cobre AC12.

## 26. Matriz de cobertura

| Critério | Requisitos | Tasks |
|---|---|---|
| AC01 | RQ01,RQ03 | T003,T004,T010 |
| AC02 | RQ02,RQ13 | T001,T005,T007,T008 |
| AC03 | RQ07,RQ08 | T002,T005,T010 |
| AC04 | RQ04 | T002,T006,T010 |
| AC05 | RQ05,RQ06 | T002,T006,T010 |
| AC06 | RQ09 | T006,T007,T010 |
| AC07 | RQ10 | T006,T007,T010 |
| AC08 | RQ12 | T004,T008,T010 |
| AC09 | RQ01–RQ10,RQ12,RQ13 | T003,T010 |
| AC10 | RQ02,RQ05,RQ06,RQ09,RQ12 | T009,T010 |
| AC11 | RQ11 | T011 |
| AC12 | RQ03,RQ08,RQ12 | T001,T009,T012 |

## 27. Consistency/readiness review

- Spec, Plan e Tasks: revision v1, mesma baseline e mesmo footprint160×112.
- 13 requisitos cobertos; 12 critérios cobertos; 12 tasks sem órfãos ou ciclos.
- Execução serial por compartilhar layout/scene/validators.
- Nenhuma decisão de gameplay/save/evento ficou implícita.
- Nenhum resultado futuro está marcado PASS.
- **IMPLEMENTING.** Footprint 160×112 aprovado explicitamente em 2026-09-10 e spec promovida para a
  raiz de `.specs/a_implementar/`; execução sujeita ao lock exclusivo Unity/Assets da Town.
