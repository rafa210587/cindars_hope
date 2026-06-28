# SPEC — Arar Qualquer Terra: Solo Arável por Tile (substitui canteiros fixos)

> **Spec ID:** `spec_farm_till_anywhere_tilemap`
> **Status:** A implementar
> **Wave:** WAVE FARM — Coerência da FarmScene (companion de `spec_farm_scene_relayout_v4`)
> **Priority:** P1
> **Type:** Runtime + Save
> **Domain:** Farm
> **Parallelizable:** CONDITIONAL
> **Parallel group:** N/A
> **Can run with:** specs que não tocam o sistema de farm/plot, save schema, nem o gerador da fazenda
> **Must not run with:** `spec_farm_scene_relayout_v4` (se editarem o gerador ao mesmo tempo); specs que alterem save schema
> **Repo lock scope:**
> - `Assets/_Game/Scripts/Farm/**` (FarmPlot/FarmPlotRegistry/Soil/Crop)
> - `Assets/_Game/Scripts/Save/**` (nova seção de save por tile)
> - `Assets/_Game/Scripts/Editor/SceneCreation/CreateMvpFarmScene.cs` (remoção dos plots fixos — coordenar com Spec A)
> **Depends on (Depende de):**
> - Sistemas existentes (NÃO recriar): FarmPlot/FarmPlotRegistry, processors/services puros em `Farm/*` (soil/crop growth, watering, quality), InventoryManager, SaveManager + padrão `ISaveSectionProvider` (precedente: HotbarSectionProvider), GameEventBus, sistema de input de ferramenta (enxada/regador).
> **Blocks (Bloqueia):**
> - `spec_farm_scene_relayout_v4` (A remove os 24 plots fixos e depende deste solo arável).
> **Scope:** Substituir os 24 `FarmPlot` fixos por um sistema em que a enxada **ara qualquer tile de solo** da fazenda (exceto footprint de construções/água/montanha; dentro da estufa é arável), persistindo o estado por **coordenada de tile** num save section, reusando os processors de crescimento/rega/qualidade existentes.
> **Out of scope:** Re-layout da cena (Spec A), arte final, novos crops, balance, irrigação avançada além do que já existe.

required_adrs: []
required_game_rules: []

---

# /speckit.specify

## 5. Contexto

Hoje a fazenda usa **24 `FarmPlot` fixos** instanciados em posições codificadas pelo `CreateMvpFarmScene.cs`. O usuário quer o modelo Stardew: **arar qualquer terra em qualquer lugar** da fazenda com a enxada — não só em canteiros pré-colocados. As **únicas** exceções (não-aráveis) são o footprint das construções, a água (lago/rio) e a montanha. **Dentro das estufas** o solo é arável (protegido/sazonal).

Esta spec é a **companion runtime/save** da `spec_farm_scene_relayout_v4` (que faz o re-layout da cena). A cena (Spec A) **remove** os plots fixos; este sistema entrega o solo arável por tile que substitui aqueles plots, reusando ao máximo os processors puros já existentes em `Farm/*` (crescimento de crop por dia, rega/chuva, qualidade).

## 6. Problema

Sem esta spec:
- A fazenda fica presa a 24 posições fixas de plantio, contrariando o pedido central do redesign ("arar qualquer terra em todo lugar").
- A Spec A não pode remover os plots fixos sem deixar a fazenda sem como plantar.
- Não há persistência de tiles arados/plantados por coordenada — o estado de cultivo se perderia entre sessões.

## 7. Objetivo

Ao final desta spec, o jogador deve poder **arar qualquer tile arável** da fazenda com a enxada, plantar/regar/colher nesse tile reusando os processors existentes, e o estado de cada tile (arado, plantado, crop, dias, regado, qualidade) deve **persistir por coordenada de tile** num save section novo — sem referência Unity em save, sem recriar o motor de crescimento/rega/qualidade, e fornecendo a API que a Spec A consome para o solo arável (incluindo o interior da estufa).

## 8. Fontes obrigatórias lidas

```text
docs/project/CURRENT_STATE.md
.specs/SPEC_IMPLEMENTABLE_TEMPLATE.md
.specs/a_implementar/spec_farm_scene_relayout_v4.md   (Spec A — companion)
.claude/rules/testing-quality-gate.md
.claude/rules/unity-architecture.md
.claude/rules/id-stability.md
.claude/skills/crop-farming-systems/SKILL.md
.claude/skills/save-section-provider/SKILL.md
.claude/skills/save-load-pattern/SKILL.md
.claude/skills/editmode-test-authoring/SKILL.md
Assets/_Game/Scripts/Farm/**          (auditar FarmPlot, FarmPlotRegistry, processors/services puros)
Assets/_Game/Scripts/Save/**          (auditar SaveManager + ISaveSectionProvider; precedente HotbarSectionProvider)
```

## 9. Estado atual do repo

Confirmar na Phase 0 (auditoria). Estado conhecido:

- `FarmPlot` / `FarmPlotRegistry` — EXISTEM; plot como objeto de cena com estado (tilled/planted/watered/cropId/days/quality). Os **processors/services puros** de soil/crop growth, watering/rain, quality (WAVE05/fable_15) — EXISTEM e devem ser **reusados** (não recriar o motor).
- `SaveManager` + padrão `ISaveSectionProvider` — EXISTEM (precedente `HotbarSectionProvider`). **Adicionar uma seção nova** de tiles de farm; não reescrever o SaveManager.
- Input de ferramenta (enxada/regador/colher) — EXISTE (WAVE05/07). Reusar; trocar o alvo de "plot fixo" para "tile sob/à frente do jogador".
- `CreateMvpFarmScene.cs` — cria os 24 plots fixos; a remoção é **coordenada com a Spec A** (lock compartilhado).

```text
Estado real precisa ser auditado na Phase 0 antes de implementação. Não recriar sistema existente sem confirmar ausência no repo.
```

## 10. Engineering stories

```text
Como jogador, quero apertar a enxada em qualquer terra livre e ela virar solo arado, em qualquer lugar da fazenda.
Como jogador, quero plantar/regar/colher no tile que arei, com o mesmo crescimento por dia de hoje.
Como jogador, NÃO quero arar dentro de construções, na água ou na montanha; mas quero arar dentro da estufa.
Como sistema de save, quero persistir o estado de cada tile por coordenada (inteiros), sem referência Unity.
Como maintainer, quero reusar os processors de crescimento/rega/qualidade — não um motor paralelo.
Como Spec A, quero uma API de "este tile é arável?" para o gerador e a estufa consumirem.
```

## 11. Escopo

```text
Inclui:
- Modelo de tile de solo arável indexado por coordenada inteira (TileX, TileY) sobre os bounds da fazenda;
- Regra de "tile arável?": livre por padrão; bloqueado por footprint de construção/água/montanha; arável dentro da estufa;
- Ação de arar: enxada converte tile livre → tilled; reusar input/feedback existentes;
- Plantar/regar/colher operando sobre o tile (delegando aos processors existentes de crop/soil/watering/quality);
- Avanço diário aplicado a todos os tiles plantados via o mesmo processor de growth atual;
- Save section novo (ISaveSectionProvider) que persiste tiles não-default por coordenada + round-trip;
- API pública consumida pela Spec A (IsTillable(tileX,tileY), marcação de zonas não-aráveis, interior de estufa);
- EditMode tests determinísticos (arar/plantar/crescer/regar/colher/round-trip de save).
```

## 12. Fora de escopo

```text
Não inclui:
- Re-layout da cena (Spec A);
- Novo motor de crescimento/rega/qualidade (reusar o existente);
- Novos crops, animais, ou balance;
- Irrigação/greenhouse sazonal além do que já existe;
- Arte/tilemap visual final (placeholder OK);
- Validação humana imediata.
```

## 13. Regras de não duplicação

```text
Não recriar os processors de soil/crop growth/watering/quality — reusar.
Não criar segundo SaveManager — adicionar ISaveSectionProvider novo.
Não criar segundo sistema de input de ferramenta — redirecionar o alvo para tile.
Manter FarmPlot apenas se ainda útil para a estufa; caso contrário, generalizar para tile (decisão na Phase 0).
```

## 14. Critérios de aceite

### 14.1 Arar em qualquer lugar
- A enxada converte qualquer tile arável livre em tilled, em qualquer região aberta da fazenda.
- Tiles sob construção/água/montanha NÃO podem ser arados; tiles no interior da estufa PODEM.
- Evidência: EditMode tests de IsTillable + ação de arar; cenário humano.

### 14.2 Ciclo de cultivo reusando processors
- Plantar → regar → crescer por dia → colher funciona no tile, usando os processors existentes (mesmos números de hoje).
- Evidência: EditMode tests de ciclo determinístico.

### 14.3 Persistência por coordenada
- Estado de cada tile arado/plantado persiste por (TileX, TileY) e sobrevive a save/load (round-trip), sem referência Unity no DTO.
- Evidência: EditMode round-trip test do save section.

### 14.4 API para a Spec A
- Existe API pública `IsTillable(tileX, tileY)` e registro de zonas não-aráveis + estufa, consumível pelo gerador da cena.
- Evidência: assinatura pública + uso documentado no report.

# /speckit.plan

## 15. Arquitetura alvo

### 15.0 Mapa de reuso (confirmado por auditoria 2026-06-26 — NÃO recriar)

```text
REUSAR (delegar, não reescrever):
  Assets/_Game/Scripts/Farm/FarmPlotLogic.cs      — lógica PURA C#: TryTill, TryWaterPlot, TryPlantSeed,
                                                     TryHarvestPure, OnHarvestCompleted, ProcessDay,
                                                     CaptureSaveData(int)->FarmPlotSaveData, RestoreFromSaveData.
                                                     ESTRATÉGIA: uma instância de FarmPlotLogic POR TILE arado.
  Assets/_Game/Scripts/Farm/FarmPlotState.cs       — enum de estados (Blocked/Raw/TilledDry/.../Dead).
  Assets/_Game/Scripts/Farm/FarmPlotSaveData.cs    — DTO por plot (PlotIndex, State, PlantedSeedId, DaysGrown,
                                                     IsWatered, DaysWithoutWater, LastProcessedDay, FertilizerId,
                                                     WateredDaysCount...). REUSAR chaveado por tile.
  Assets/_Game/Scripts/Farm/Crops/CropGrowthProcessor.cs   — crescimento por dia (idempotente).
  Assets/_Game/Scripts/Farm/Watering/FarmWateringService.cs — ApplyManualWatering/Rain/Irrigation; FarmPlotWaterState.
  Assets/_Game/Scripts/Farm/Crops/CropQualityResolver.cs    — qualidade (Normal..Arcane).
  Assets/_Game/Scripts/Save/ISaveSectionProvider.cs + Providers/HotbarSectionProvider.cs (precedente);
    SaveManager.Initialize() registra providers; FarmSectionProvider/FarmSaveData JÁ existem (auditar p/ coexistência).
  Input: GameplayInputRouter -> FarmPlotMenuController -> FarmPlot.Actions -> FarmPlotLogic (redirecionar alvo p/ tile).

NOTA: NÃO há tilemap hoje (sistema é por plot fixo via FarmPlotRegistry indexado). FarmPlotWaterState já tem
TilePosition (Vector2Int) — usar Vector2Int como chave primária do grid. Coexistir com FarmPlotRegistry/FarmSectionProvider:
o tile system gerencia tiles arados livremente; os 24 plots fixos permanecem até a Spec A removê-los.
```

### 15.1 Arquivos

```text
Assets/_Game/Scripts/Farm/
  FarmTileGrid.cs            (Dictionary<Vector2Int, FarmPlotLogic> dos tiles arados; world<->tile; IsTillable)
  FarmTileState.cs           (holder por tile: FarmPlotLogic + FarmPlotWaterState ref; ou reuso direto) — sem refs Unity no que persiste
  FarmTilledSoilService.cs   (arar/plantar/regar/colher delegando a FarmPlotLogic + FarmWateringService + CropGrowthProcessor + CropQualityResolver)
  FarmNonArableZones.cs      (registro de footprints não-aráveis: construções/água/montanha; + flag estufa)
  (reuso) FarmPlotLogic, CropGrowthProcessor, FarmWateringService, CropQualityResolver (NÃO recriar)

Assets/_Game/Scripts/Save/
  FarmTilesSaveData.cs       ([Serializable] DTO: lista de tiles não-default por coordenada)
  FarmTilesSectionProvider.cs (ISaveSectionProvider — capture/restore)

Assets/_Game/Tests/EditMode/Farm/
  FarmTileGridTests.cs
  FarmTilledSoilServiceTests.cs
  FarmTilesSaveRoundTripTests.cs

docs/validation/
  spec_farm_till_anywhere_tilemap_execution_report.md
docs/validation/playmode/
  spec_farm_till_anywhere_tilemap_human_playmode_scenario.md
```

## 16. Contratos, dados e eventos

### 16.1 Data contracts
- `FarmTileState`: apenas simple types (`bool tilled`, `string cropId`, `int daysGrown`, `bool watered`, `int quality`/enum). Tile default (não arado, vazio) NÃO é persistido.
- `FarmTileGrid`: mapeia world position ↔ (TileX,TileY) usando `FarmScaleContract.TileSizePixels` e os bounds da fazenda (Spec A).

### 16.2 Runtime contracts
- `FarmTilledSoilService.TillTile / PlantTile / WaterTile / HarvestTile` — delegam aos processors existentes; nunca duplicam fórmula.
- `IsTillable(tileX, tileY)` = não está em `FarmNonArableZones` (ou está dentro da estufa).

### 16.3 Event contracts
- Reusar eventos existentes de farm (ex.: `CropWateredEvent`, `CropHarvestedEvent`, soil/till feedback). NÃO criar evento novo salvo necessidade; se criar, listar em §24.

### 16.4 Save contracts
```text
Does this change save schema? YES (adiciona seção de tiles da farm)
Does this add a save section? YES (FarmTilesSectionProvider via ISaveSectionProvider)
Does this require migration? NO (seção nova; ausência = sem tiles arados — back-compat)
Does this persist Unity references? MUST BE NO
```
Owner: `FarmTilesSectionProvider`. Restore order: após inventário/tempo (precisa do calendário para crescimento). Round-trip obrigatório.

### 16.5 UI contracts
- N/A (reusa feedback de ação existente). Indicador visual de tile arado é placeholder.

## 17. Sistemas afetados

```text
Farm (soil/crop/watering/quality)
Save/load (nova seção por tile)
Input de ferramenta (enxada/regador/colher → alvo tile)
GameEventBus (eventos de farm existentes)
Spec A (consome IsTillable + zonas não-aráveis)
```

## 18. Arquivos permitidos

```text
Assets/_Game/Scripts/Farm/**
Assets/_Game/Scripts/Save/FarmTiles*.cs
Assets/_Game/Tests/EditMode/Farm/**
docs/validation/**
docs/validation/playmode/**
(coordenado com Spec A) Assets/_Game/Scripts/Editor/SceneCreation/CreateMvpFarmScene.cs — apenas remoção dos plots fixos
```

## 19. Arquivos proibidos

```text
Assets/**/*.unity / *.prefab / *.asset (salvo data asset explícito)
Assets/_Game/Scripts/Save/SaveManager.cs reescrito (apenas registrar a nova seção pelo padrão existente)
docs_old/**  docs/archive/**  Packages/**  ProjectSettings/**
```

## 20. Estratégia de implementação

```md
### Fase 0 — Auditoria
- Mapear FarmPlot/FarmPlotRegistry e os processors puros existentes (growth/watering/quality).
- Confirmar padrão ISaveSectionProvider (HotbarSectionProvider) e ordem de restore.
- Decidir: generalizar FarmPlot → tile, ou manter FarmPlot só na estufa.

### Fase 1 — Modelo + serviço
- FarmTileGrid + FarmTileState + FarmNonArableZones + FarmTilledSoilService (delegando aos processors).

### Fase 2 — Save section
- FarmTilesSaveData + FarmTilesSectionProvider (capture/restore) registrado pelo padrão existente.

### Fase 3 — Input
- Redirecionar a enxada/regador/colher para operar no tile sob/à frente do jogador.

### Fase 4 — Testes/validação
- EditMode: IsTillable, arar, ciclo crescer/regar/colher, round-trip de save.
- dotnet build runtime+editor exit 0.

### Fase 5 — Relatório + cenário humano
```

## 21. Ordem de execucao (ordem segura)

```text
1. Auditar farm processors + save provider pattern.
2. Criar modelo de tile + serviço (reuso dos processors).
3. Criar save section + round-trip.
4. Redirecionar input de ferramenta para tile.
5. EditMode tests + dotnet build exit 0.
6. Coordenar com Spec A a remoção dos plots fixos.
7. Execution report + cenário humano.
```

## 22. Paralelização

```md
- Parallelizable: CONDITIONAL
- Parallel group: N/A
- Can run with:
  - specs que não tocam farm/save/gerador da fazenda
- Must not run with:
  - spec_farm_scene_relayout_v4 (se ambas editarem o gerador); specs que alterem save schema
- Shared files/systems that require lock:
  - Farm/**; Save/**; CreateMvpFarmScene.cs (remoção dos plots)
- Reason:
  - Toca save schema e o motor de farm; precisa de ordem com a Spec A.
```

## 23. Impacto em save/load

```text
Does this change save schema? YES (seção nova de tiles da farm)
Does this add a save section? YES
Does this require migration? NO (ausência da seção = sem tiles arados)
Does this persist Unity references? MUST BE NO
```

## 24. Impacto em eventos

```text
Adds events: NO (preferencialmente; reusar farm events)
Changes existing events: NO
Requires unsubscribe pattern: YES (para novos subscribers)
```

## 25. Impacto em UI/Unity

```text
Changes UI: NO (feedback reusado)
Changes scenes: NO diretamente (a remoção de plots é via Spec A no gerador)
Changes prefabs: NO
Changes ScriptableObjects/assets: NO
Requires Play Mode final validation: YES
Human validation timing: DEFERRED_TO_FINAL_VALIDATION
```

## 26. Riscos técnicos

```text
Risco: duplicar a fórmula de crescimento/rega/qualidade.
Mitigação: serviço apenas delega aos processors existentes; tests garantem mesmos números.

Risco: save de tiles inflar (arar a fazenda toda).
Mitigação: persistir só tiles não-default; coordenadas inteiras compactas.

Risco: IsTillable divergir das zonas reais da cena (Spec A).
Mitigação: FarmNonArableZones alimentado pelas mesmas zonas do gerador; test de coerência.

Risco: ordem de restore (crop precisa de calendário).
Mitigação: registrar restore após tempo/inventário; round-trip test.
```

## 27. Rollback

```text
git revert dos arquivos criados (Farm tile model/service, save section, tests).
Sem a seção, saves antigos seguem válidos (back-compat).
Coordenar com Spec A: se A já removeu os plots, reverter A junto ou restaurar plots.
Não apagar save real do usuário.
```

# /speckit.tasks

## 28. Tasks

```md
- [ ] T001 — Auditar FarmPlot/processors/save provider; decidir FarmPlot→tile vs estufa-only.
- [ ] T002 — Criar FarmTileState (simple types) + FarmTileGrid (world↔tile, IsTillable).
- [ ] T003 — Criar FarmNonArableZones (footprints construção/água/montanha + flag estufa).
- [ ] T004 — Criar FarmTilledSoilService (arar/plantar/regar/colher delegando aos processors).
- [ ] T005 — Criar FarmTilesSaveData + FarmTilesSectionProvider (capture/restore) registrado.
- [ ] T006 — Redirecionar input de ferramenta para o tile sob/à frente do jogador.
- [ ] T007 — EditMode: FarmTileGridTests (IsTillable, mapeamento).
- [ ] T008 — EditMode: FarmTilledSoilServiceTests (arar/ciclo crescer-regar-colher).
- [ ] T009 — EditMode: FarmTilesSaveRoundTripTests.
- [ ] T010 — dotnet build runtime+editor; rodar validators.
- [ ] T011 — Coordenar remoção dos plots fixos com a Spec A.
- [ ] T012 — Execution report + cenário humano de Play Mode.
```

## 29. Validações obrigatórias

```powershell
dotnet build .\Assembly-CSharp.csproj --no-restore
dotnet build .\Assembly-CSharp-Editor.csproj --no-restore
.\tools\docs\validate_docs.ps1
```

EditMode:
```text
Unity Test Runner — EditMode (FarmTileGridTests, FarmTilledSoilServiceTests, FarmTilesSaveRoundTripTests)
```

Se algum comando não puder rodar, o report registra `NOT RUN` com motivo e risco residual.

## 30. Testing Quality Gate

```md
- Changed deterministic logic: YES (till/grow/save)
- Requires EditMode tests: YES (grid, serviço, round-trip de save)
- Requires PlayMode automated or final human scenario: YES (arar em vários lugares, plantar/regar/colher, save/load mantém tiles, não arar em construção/água/montanha, arar na estufa)
- Requires regression test: YES (round-trip de save + paridade dos processors)
- Human validation timing: DEFERRED_TO_FINAL_VALIDATION
- Minimum validation evidence for ACCEPTED: dotnet build runtime+editor exit 0; EditMode tests PASS; cenário humano de Play Mode executado.
```

## 31. Definition of Done

```text
Arar qualquer tile arável implementado, reusando os processors existentes.
Save section novo com round-trip; sem referência Unity no DTO; back-compat.
API IsTillable + zonas não-aráveis disponível para a Spec A.
EditMode tests PASS; dotnet build runtime+editor exit 0; docs validation executada (ou NOT RUN c/ motivo).
Remoção dos plots fixos coordenada com a Spec A.
Execution report + cenário humano criados.
Sem claim de ACCEPTED sem evidência (máx. BUILD_VALIDATED até Play Mode humano).
```

## 32. Anti-regressão

```text
Não mudar a fórmula de crescimento/rega/qualidade (mesmos números de hoje).
Não serializar referência Unity em save.
Não quebrar saves antigos (seção ausente = sem tiles arados).
Não remover os crops/animais do catálogo.
Manter o ciclo plantar→regar→crescer→colher idêntico em comportamento, só mudando o alvo (plot fixo → tile livre).
```

## 33. Notas para execução posterior

```text
Sequenciar antes (ou junto) da Spec A; A remove os plots fixos e consome a API deste sistema.
Indicador visual final do tile arado (tilemap art) pode ser spec futura.
Não atualizar SPEC_EXECUTION_ORDER.md como se já estivesse implementada.
```
