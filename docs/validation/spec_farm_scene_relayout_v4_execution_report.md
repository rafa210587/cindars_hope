# Execution Report — spec_farm_scene_relayout_v4

**Data:** 2026-06-26 (atualizado 2026-06-27 — v7: composicao por borda, miolo aberto, rio borda leste)
**Spec:** `.specs/a_implementar/spec_farm_scene_relayout_v4.md`
**Executor:** Claude Sonnet (automated, por instrucao humana direta)
**Status:** BUILD_VALIDATED — aguarda Play Mode humano

> **Iteracao v7 (2026-06-27 — §15.5 + §15.6):** "Miolo aberto" — composicao por borda:
>
> - **FASE 1 — Rio → borda leste:** `CreateRiverAndBridge` reescrito. Rio sai de acude (22,18)
>   e segue pela borda leste (x≈21-19), NUNCA cruzando o miolo (x<18). Segmentos:
>   Seg_N (21.5,13 / 1.8×10), Seg_C (21,6 / 1.8×4), vao ponte SEM colisor (21,3 / 1.8×2),
>   Seg_S (20,0 / 1.8×4), Seg_Lower2 (19.5,-5 / 1.8×6), Seg_Delta (19,-7 / 1.8×2 → foz lago).
>   Acude_Nascente (22,18 / 4×3). Ponte (21,3) com vao sem colisor. Lago centro (18,-13)
>   ~26×14 spans x[5,31] y[-20,-6]. FishingSpot (8,-9). Ore nodes: (-18,-9,0,9) na montanha.
>   CaveEntrance movida para (-28,18.5).
>
> - **FASE 2 — Estrutura nas bordas:** HOMESTEAD LESTE coeso: House (28,9), Greenhouse (24,10),
>   ShippingBin (28,4), SellPoint (24,5), Quadro Evoluções (30,4), Workbench (25,1), Forge (27,1),
>   CookingStation (29,1), Portal (31,-1), spawn_default (24,3). ANIMAIS BORDA SUL: Coop (-22,-19),
>   Barn (-13,-19), CheesePress (-28,-19), WineBarrel (-8,-19). BOSQUE/FONTE OESTE: Fonte (-24,4),
>   Rock (-31,6), Forage cluster borda oeste. EXPANSÕES BORDAS: Exp_North (-2,16.5) 8×4,
>   Exp_NE (16,16.5) 7×4, Exp_West (-30,-10) 5×8. Exp_South → renomeada Exp_NE.
>
> - **FASE 3 — Debris no miolo:** `CreateTrees` ampliado de 40 para **68 árvores** (40 bosque
>   oeste denso x[-32,-22] y[0,17] + 28 espalhadas no miolo x[-17,15] y[-14,15]).
>   MioloDebris_Pedras: 12 pedras (RockResource) no miolo. MioloDebris_Moitas: 10 moitas
>   (ForagePoint farm_forage_07..16) no miolo. Total: ~68 árvores + 12 pedras + 10 moitas.
>
> - **FASE 4 — Bootstrap não-arável v7:** `FarmSceneRuntimeBootstrap` atualizado com novos
>   footprints de borda. Rio v7 bloqueado em x>18 (nunca no miolo). Lago x[5,31] y[-20,-6].
>   Homestead: house (24.5,6 / 7×6), estufa (21.5,8 / 5×4). Animais borda sul.
>   Expansões: Exp_North (-6,14.5 / 8×4), Exp_NE (12.5,14.5 / 7×4), Exp_West (-32.5,-14 / 5×8).
>   Interior estufa v7 arável (21.5,8 / 5×4). MIOLO x[-18,16] y[-15,16] = totalmente arável.
>
> - **FASE 5 — Gizmo limpo:** `FarmSceneZoneMarker.OnDrawGizmos` → `OnDrawGizmosSelected`.
>   Overlay de zonas só aparece quando o objeto está selecionado — Game View limpo mesmo com Gizmos.
>
> - **FASE 6 — Validator + builds:** `ValidateFarmSceneLayoutV4` atualizado: Exp_South → Exp_NE.
>   `dotnet build Assembly-CSharp.csproj --no-restore` exit 0. `Assembly-CSharp-Editor.csproj` exit 0.
>   `validate_docs.ps1` exit 0 — sem novos erros. CindarsHopeMenu confirma RunStep ja existente.
>
> **Validação de build v7:**
> - Assembly-CSharp: PASS (exit 0)
> - Assembly-CSharp-Editor: PASS (exit 0)
> - validate_docs.ps1: PASS (exit 0)
> - Play Mode: NOT RUN (deferido ao Play Mode humano — rodar `CindarsHope/Inicializar Projeto` no Unity)

> **Iteracao v6 (2026-06-26 — §15.2 v6 + §15.4 Fases 5–8):** Fases 5–8 completadas:
>
> - **Fase 5 — Zrix perambula:** `CreateFarmZrixNpc()` cria `NPC_Zrix_Farm` com `NpcController` +
>   `NpcWanderer` + `Rigidbody2D`. `ConfigureMovement(1.0f, 4f, 2.5f, 5.5f, (-26,4), (-10,16))`.
>   Asset: `Assets/_Game/Data/NPCs/Npc_Zrix.asset`. Board_Zrix estático mantido (contrato).
>
> - **Fase 6 — 3 areas de expansao:** `CreateExpansionAreaPlaceholders()` cria `Exp_North` (-6,17)
>   8×5, `Exp_West` (-30,-4) 6×8, `Exp_South` (-16,-18) 8×5. Overlay roxo semi-transparente +
>   borda + trigger para futuro desbloqueio.
>
> - **Fase 7 — Solo aravel:** `FarmSceneRuntimeBootstrap` ja registra todas as zonas nao-araveis
>   (construcoes, agua, montanha, 3 areas de expansao) e greenhouse como aravel. Bounds 64×44
>   confirmados. Rio desagua no lago: RiverSeg_Delta (13,-10 / 1.8×4) sobrepoe borda NO do lago
>   (LakeBody_NW centro (10,-8) / 10×7) — foz confirmada.
>
> - **Fase 8 — Tool check real:** `FarmTillingInputController` agora injeta `EquipmentManager`
>   e chama `HasTool(ToolType.Hoe)` para arar e `HasTool(ToolType.WateringCan)` para regar.
>   Flag `_alwaysHasTool` removida. Fallback permissivo com aviso se EquipmentManager nao wired.
>   Enxada e regador adicionados ao starting loadout via `RepairPlayerStartingItems.EnsureStartingHoe/
>   WateringCan()` e registrados em `CindarsHopeMenu` (InicializarProjeto + RepararEReconstruir).
>
> - **ValidateFarmSceneLayoutV4:** adicionados checks `NPC_Zrix_Farm`, `Exp_North/West/South`.
>
> **Iteracao v5 (2026-06-26 — §15.2 v5 + §15.3):** Fazenda ampliada de 48×34 para **56×40**
> com origem centrada (x∈[-28,28], y∈[-20,20]). 6 melhorias visuais/UX implementadas:
>
> 1. **Zona markers gizmo-only:** `FarmSceneZoneMarker` usa `OnDrawGizmos` (`#if UNITY_EDITOR`)
>    — sem `SpriteRenderer`; visivel apenas no Editor Unity. `CreateFarmSceneZone` nao cria mais SR.
>
> 2. **Casa WALK-IN:** `CreateFarmWalkInHouse()` replica padrao de `CreateMvpTownScene`:
>    chao + paredes + `HouseDoorInteractable` (E abre) + `RoofRevealController` (some ao entrar).
>    `Bed` + `BedLetter` + `FarmHouseChest` criados **dentro** do interior percorrivel.
>    Centro (21,7), footprint 7×6, porta ao sul.
>
> 3. **Rio AZUL rerouted:** `CreateRiverAndBridge()` reescrito com cor `(0.42,0.62,0.85)`,
>    4 segmentos alinhados com rota (8,16)→(9,8)→(10,0)→(12,-8)→lago. Ponte em (10,1).
>
> 4. **Lago maior organico:** 3 corpos sobrepostos (LakeBody_Main 14×8, LakeBody_NW 6×5,
>    LakeBody_SE 6×4) centrados em (19,-13), cobrindo ~16×10 em SE.
>
> 5. **Bosque mais denso:** 28 arvores cluster NO (x∈[-26,-10], y∈[2.5,13]) + 6 espalhadas
>    = 34 total. `TreeScaleBoostV5 = 1.35f` aplicado sobre escala base.
>
> 6. **FonteAnya em clareira:** reposicionada para (-14,7) — borda sul do bosque, marcador
>    de respawn e ponto cenico narrativo.
>
> **Correcao de desvios anteriores (2026-06-26):** Uma implementacao anterior deixou dois desvios da spec
> que foram corrigidos:
>
> - **Desvio 1 (corrigido):** Os 24 canteiros fixos do campo aberto removidos. `CreateFarmPlots` agora
>   chama `registry.Configure(new FarmPlot[0])`. Os 4 canteiros da estufa (200..203) preservados.
>
> - **Desvio 2 (corrigido):** `CreateFarmExpansionLots` e helpers removidos. `FarmLotService` /
>   `FarmLotsSectionProvider` / `FarmLotCatalog` permanecem como sistemas de save/runtime.
>
> **Assembly-CSharp:** exit 0, 0 erros | **Assembly-CSharp-Editor:** exit 0, 0 erros (warnings pre-existentes).
> **validate_docs.ps1:** exit 0, PASSED.
>
> **v6 builds (Fases 5-8):** Assembly-CSharp exit 0 | Assembly-CSharp-Editor exit 0 | validate_docs.ps1 PASSED.

---

## Acceptance criteria extracted

Da spec §6 (acceptance criteria):

| ID  | Criterio | Status |
|-----|----------|--------|
| AC01 | FarmPlots fixos removidos (solo aravel por tile via Spec B) | DONE: `CreateFarmPlots` chama `registry.Configure(new FarmPlot[0])` — campo aberto sem canteiros; estufa 200..203 preservada; solo aravel via FarmTileGrid |
| AC02 | Lotes fable_41 substituidos por FarmEvolutionBoard | DONE: `CreateFarmExpansionLots` e helpers removidos do gerador; `FarmEvolutionBoardInteractable` em (14.5, 1) |
| AC03 | Todos os elementos reposicionados para coordenadas v4 (§15.2 / §32) | DONE: reposicionamento 48×34 completo em 2026-06-26 — ver matriz de compliance abaixo |
| AC04 | Montanha (N) com colisao solida, faixa y∈[13,17] | DONE: `CreateMountainBarrier()` |
| AC05 | Rio + ponte (SE, andavel) | DONE: `CreateRiverAndBridge()` |
| AC06 | 4 veios de minerio bloqueados na base da montanha | DONE: `CreateLockedOreNodes()` — IDs ore_node_farm_01..04 |
| AC07 | Casa walk-in com Bed + BedLetter + FarmHouseChest (craft fora) | DONE v5: `CreateFarmWalkInHouse()` — casa fisica, Bed/BedLetter/FarmHouseChest dentro do interior percorrivel. Craft em (16,-2),(18,-2),(20,-2) fora da casa |
| AC08 | FarmSceneRuntimeBootstrap — configura bounds 56x40 + zonas nao-araveis | DONE v5: `SetBounds(0,0,56,40,-28,-20)` com todos os footprints v5 |
| AC09 | FarmTillingInputController — input [F] fecha gap T006 da Spec B | DONE: em `CreateFarmTillingInputController()` |
| AC10 | ValidateFarmSceneLayoutV4 — presenca de todos os elementos | DONE: registrado em CindarsHopeMenu ValidarProjeto |
| AC11 | FarmScaleContract v5: bounds 56x40, origem centrada | DONE v5: `FarmLevel1LayoutContract.cs` atualizado para 56×40 |
| AC12 | EditMode tests para o contrato v5 | DONE v5: `FarmLevel1LayoutContractTests.cs` atualizado para 56×40 |

---

## Existing systems audit

Sistemas consumidos sem modificacao de behavior:

- **FarmTileGrid / FarmNonArableZones / FarmTilledSoilService** (Spec B): consumidos via `SaveManager.FarmTileGrid`. Adicionadas `NonArableZones` property e `UnregisterBlockedRect()` (API nova, sem regressao).
- **IInteractable** / **GameEventBus** / **PlayerActionFeedbackEvent**: reutilizados sem alteracao.
- **CreateMvpFarmScene.cs**: gerador existente — todos os helpers existentes foram reposicionados (nao reescritos). Novos helpers adicionados ao fim.
- **CindarsHopeMenu.cs**: apenas novo `RunStep` em `ValidarProjeto`. Nao alterou InicializarProjeto nem RepararEReconstruir (gerador da scene e acionado manualmente pelo humano).

---

## Spec Compliance Matrix

### §15.2 v5 — Elementos e coordenadas v5 (56×40)

| Elemento | Coord. v5 (56×40) | Implementado em | Verif. |
|---|---|---|---|
| spawn_farm_default | (12, 1) | `CreateFarmSpawnPoints` | OK v5 |
| spawn_farm_from_town | (25, 0) | `CreateFarmSpawnPoints` | OK v5 |
| spawn_farm_from_cave | (-21, 14) | `CreateFarmSpawnPoints` | OK v5 |
| FarmHouse (walk-in) | centro (21, 7) | `CreateFarmWalkInHouse` | OK v5 — NOVO |
| Bed | dentro FarmHouse | `CreateFarmWalkInHouse` | OK v5 |
| BedLetter | dentro FarmHouse | `CreateFarmWalkInHouse` | OK v5 |
| FarmHouseChest | dentro FarmHouse | `CreateFarmWalkInHouse` | OK v5 |
| FonteAnya | (-14, 7) clareira bosque | `CreateFonteAnya` | OK v5 |
| Workbench | (16, -2) | `CreateCraftingStations` | OK v5 |
| Forge | (18, -2) | `CreateCraftingStations` | OK v5 |
| CookingStation | (20, -2) | `CreateCraftingStations` | OK v5 |
| ShippingBin | (18, 2) | `CreateShippingBin` | OK v5 |
| SellPoint | (14, 3) | `CreateSellPoint` | OK v5 |
| Portal_Farm_To_Town | (27, 0) | `CreateFarmPortals` | OK v5 |
| CaveEntrance | (-24, 16.5) | `CreateCaveEntrance` | OK v5 |
| Board_Zrix | (-21, 15) | `CreateZrixContractBoard` | OK v5 |
| Coop_01 | (-11, -11) | `CreateAnimalHousings` | OK v5 |
| Barn_01 | (-1, -11) | `CreateAnimalHousings` | OK v5 |
| CheesePress | (-12, -16) | `CreateProcessingAndGreenhouse` | OK v5 |
| WineBarrel | (-7, -16) | `CreateProcessingAndGreenhouse` | OK v5 |
| Greenhouse | centro (14, 13) | `CreateGreenhouse` | OK v5 |
| FishingSpot | (14, -10) | `CreateFishingSpot` | OK v5 |
| RockResource_01 | (-26, 5) | `CreateFarmResourceInteractables` | OK v5 |
| DebugCarrotSeedPickup | (10, -1) | `CreateItemPickups` | OK v5 |
| Forage (6 pts) | (-23/-21/-19, -2) e (-23/-21/-19, -4) | `CreateForagePoints` | OK v5 |
| FarmEvolutionBoard | (24, 2) | `CreateFarmEvolutionBoard` | OK v5 |
| MountainBarrier | faixa y∈[16,20], 56×4 | `CreateMountainBarrier` | OK v5 |
| OreNode 01..04 | (-13/-5/5/13, 16.5) | `CreateLockedOreNodes` | OK v5 |
| RiverAndBridge | azul, 4 segs, ponte (10,1), lago organico (19,-13) | `CreateRiverAndBridge` | OK v5 — REESCRITO |

### §15.3 — Arvores v5

- Bosque NO denso: 28 arvores em x∈[-26,-10], y∈[2.5,13] com `TreeScaleBoostV5 = 1.35f`
- Arvores espalhadas: 6 arvores em posicoes variadas
- Total: 34 arvores (bosque mais denso e maior que v4)

### Bounds v5 (56×40)

- `FarmLevel1LayoutContract`: MinX=-28, MaxX=28, MinY=-20, MaxY=20 — OK
- `CreateBounds()`: Top (0,20) 56×1 / Bottom (0,-20) 56×1 / Left (-28,0) 1×40 / Right (28,0) 1×40 — OK
- `FarmSceneRuntimeBootstrap`: `SetBounds(0,0,56,40,-28,-20)` — OK v5

---

## Validation

```text
Validation method: dotnet build + validate_docs.ps1 (v5: 56x40 + walk-in + rio azul + lago organico)
Assembly-CSharp build: exit 0, 0 errors (6 pre-existing warnings MSB3245 + CS0649)
Assembly-CSharp-Editor build: exit 0, 0 errors (15 pre-existing warnings)
Docs validation (validate_docs.ps1): exit 0, PASSED

-- v6 (Fases 5-8: Zrix, Exp, tool check, starting loadout) --
Assembly-CSharp build: exit 0, 0 errors (1 pre-existing warning CS0649)
Assembly-CSharp-Editor build: exit 0, 0 errors (3 pre-existing warnings)
Docs validation (validate_docs.ps1): exit 0, PASSED

Unity batchmode: NOT RUN (Unity nao disponivel em modo script)
Play Mode: NOT RUN — ver cenario humano em docs/validation/playmode/
```

**Residual risk (Testing Quality Gate):**
- Play Mode: scene e materializada por gerador C#; sem Play Mode, refs serializadas (SaveManager,
  playerTransform, leaf, blocker da porta) nao sao verificadas em runtime.
- Validation: ValidateFarmSceneLayoutV4 so pode checar presenca de objetos apos regerar a scene via
  Unity Editor (CindarsHope/Inicializar Projeto) — nao foi executado fora do Unity.
- Casa walk-in: `RoofRevealController.Configure` e `HouseDoorInteractable.Configure` chamados em
  tempo de geracao; referencia serializadas corretas so verificaveis em Play Mode.
- Assets gerados: FarmScene.unity existe mas nao foi regerada com v5. Regerar: humano → Unity →
  CindarsHope/Inicializar Projeto.

---

## Arquivos alterados

**Novos (6 C# — criados em iteracoes anteriores):**
- `Assets/_Game/Scripts/World/FarmEvolutionBoardInteractable.cs`
- `Assets/_Game/Scripts/World/LockedOreNodeInteractable.cs`
- `Assets/_Game/Scripts/World/FarmHouseChestInteractable.cs`
- `Assets/_Game/Scripts/Farm/Runtime/FarmSceneRuntimeBootstrap.cs`
- `Assets/_Game/Scripts/Farm/Runtime/FarmTillingInputController.cs`
- `Assets/_Game/Scripts/Editor/Validation/ValidateFarmSceneLayoutV4.cs`

**Modificados na iteracao v5 (2026-06-26):**
- `Assets/_Game/Scripts/Farm/FarmLevel1LayoutContract.cs` — atualizado para 56×40, todas as ancoras v5
- `Assets/_Game/Scripts/Farm/Scene/FarmSceneZoneMarker.cs` — OnDrawGizmos (#if UNITY_EDITOR), sem SpriteRenderer
- `Assets/_Game/Scripts/Farm/Runtime/FarmSceneRuntimeBootstrap.cs` — 56×40, worldOrigin (-28,-20), footprints v5
- `Assets/_Game/Scripts/Editor/SceneCreation/CreateMvpFarmScene.cs` — todas coords v5, CreateFarmWalkInHouse, CreateRiverAndBridge v5 (azul+4segs+lago organico), bosque denso 28+6 trees com TreeScaleBoostV5
- `Assets/_Game/Scripts/Editor/Validation/ValidateFarmSceneLayoutV4.cs` — checks FarmHouse + FarmHouseChest v5
- `Assets/_Game/Tests/EditMode/Farm/FarmLevel1LayoutContractTests.cs` — testes 56×40

**Modificados em iteracoes anteriores:**
- `Assets/_Game/Scripts/Farm/FarmTileGrid.cs` — property NonArableZones
- `Assets/_Game/Scripts/Farm/FarmNonArableZones.cs` — UnregisterBlockedRect
- `Assets/_Game/Scripts/Editor/SceneCreation/CreateMvpFarmScene.cs` — reposicionamento total + helpers (spec B-v4)
- `Assets/_Game/Scripts/Editor/CindarsHopeMenu.cs` — RunStep em ValidarProjeto

---

## Decisoes tomadas durante a execucao

| Decisao | Motivo |
|---|---|
| FarmPlotRegistry retorna array vazio (0 plots de campo aberto) | Desvio corrigido em 2026-06-26: os 24 canteiros fixos foram removidos conforme a spec. O registry existe vazio para que `CreateRainIrrigation` e `FarmSectionProvider` nao quebrem por NullReferenceException. Os 4 plots da estufa (indices 200..203) sao criados diretamente dentro do GreenhouseRuntimeHost, independentes do registry de campo. |
| CreateFarmExpansionLots removido | Desvio corrigido em 2026-06-26: o metodo e seus helpers eram dead code (a chamada ja havia sido substituida por `CreateFarmEvolutionBoard()`). Removido para eliminar referencias a `FarmLotSceneBinding`, `FarmLotSignInteractable`, `FarmOrchardCatalog` e `using CindarsHope.Farm.Lots` que compilavam mas nao eram executadas. |
| FarmEvolutionBoard usa IMGUI stub | Spec autoriza stub; UI real e spec futura de evolucao/progressao. |
| LockedOreNodeInteractable sem gate de progressao real | Gate futura; stub com flag `_unlocked`. |
| FarmTillingInputController com `_alwaysHasTool = true` | Integrar StaminaManager/HotbarManager e spec futura de ferramentas. (RESOLVIDO v6: substituido por EquipmentManager.HasTool) |
| CreateRiverAndBridge usa retangulos simplificados | Rio curvilíneo exige tilemap; retangulos de colisao cobrem o contrato de bloqueio de atravessamento. |
| Zrix sem DialogueModal na fazenda | Zrix so perambula na fazenda; Board_Zrix e o canal de contratos. NpcController.OnEnable() trata _dialogueModal == null sem crash (nao registra OnClose). |
| Tool check: fallback permissivo se EquipmentManager == null | Residual: cena precisa ser regerada via CindarsHope/Inicializar Projeto para injetar EquipmentManager. Sem regerar, _equipmentManager e null e o fallback permissivo loga aviso. |
