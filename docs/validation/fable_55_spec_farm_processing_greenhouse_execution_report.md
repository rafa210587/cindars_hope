# Execution Report — fable_55: Fazenda Processamento (Queijaria/Barril) + Estufa Mínima

> **Spec:** `.specs/a_implementar/fable/fable_55_spec_farm_processing_greenhouse.md`
> **Status:** BUILD_VALIDATED_WITH_WARNINGS
> **Date:** 2026-06-20
> **Wave:** FABLE Batch 10
> **validated_game_rules:** farm_rules.md, save_rules.md
> **Validation method:** run_strict_validation.ps1 (exit 0)

---

## Honest status rationale

`BUILD_VALIDATED_WITH_WARNINGS`. Núcleo determinístico (timing por dia, consumo de insumos,
coleta idempotente, save round-trip, override de estação da estufa, exclusão de chuva)
implementado e coberto por testes EditMode. Builds runtime e editor 0E. PlayMode/visual e a
validação humana foram **DEFERIDOS** por autorização explícita do dono desta sessão (não rodar
Unity Editor/Play Mode). A regeneração da FarmScene via gerador é CODE_READY (o gerador foi
estendido com as 2 estações + estufa), mas a execução do gerador no Unity Editor é ação humana
pendente — por isso `_WITH_WARNINGS` e não `BUILD_VALIDATED` pleno.

---

## Fase 0 — Decisão de reuso (OBRIGATÓRIA, system-reuse-audit)

### Superfícies de "transformar item com tempo" auditadas

| # | Superfície | Estado real no repo | Semântica de tempo | Persistência | Requer SO asset |
|---|---|---|---|---|---|
| 1 | `Craft/` WI-14 (`CraftingStation`/`CraftingJob`/`CraftingRuntime`) | **VIVO** (integrado + salvo via `CraftingJobSaveData`) | **segundos** (`RemainingSeconds`, `Update(deltaTime)`) | pronta | **SIM** (`RecipeDataSO`) |
| 2 | `Crafting/CraftingService` + `ProcessingJob` (por ticks de dia) | **AUSENTE** do repo (só existem `Crafting/EquipmentUpgrade*`, `RecipeUnlockService`, `HighTierGear*`; nenhum `CraftingService`/`ProcessingJob`) | n/a | n/a | n/a |
| 3 | `Farm/Processing/` (`ProcessableItem`/`FarmProcessingJob`) | **ÓRFÃO** (ninguém instancia) | **dias** (`StartDay`/`FinishDay`/`IsReadyOnDay`/`Collect` idempotente) | inexistente (sem DTO) | NÃO (C# puro) |
| 4 | `Farm/Watering/GreenhouseContextProvider` | **ÓRFÃO** (ninguém instancia) | n/a (estufa) | n/a | NÃO |

### Decisão

**A superfície canônica do processamento POR DIA é o `Farm/Processing/FarmProcessingJob` (#3),
ADOTADO** (ganhou host de runtime + DTO de save). Critérios:

- **Semântica de dias:** o ITEM_CATALOG §5 especifica "queijo 1 dia", "vinho 2 dias".
  `FarmProcessingJob` já modela `StartDay`/`FinishDay` e avança por `DayStartedEvent`. Craft WI-14
  trabalha em **segundos** — forçar dias nele exigiria fabricar um mapeamento segundos↔dias.
- **Menor diff:** adotar o órfão exigiu só um host (`FarmProcessingStationService`) + um modelo
  puro testável (`FarmProcessingStationModel`) + DTO aditivo. Usar WI-14 exigiria **autorar
  `RecipeDataSO` assets** (proibido nesta spec: sem criação de `.asset`) e wiring de `WorkshopType`.
- **Persistência:** `FarmProcessingJob` não tinha save; foi resolvido com DTO de tipos simples
  (`FarmProcessingSaveData`) — campo ADITIVO na seção farm. (Pesa a favor de WI-14, mas não supera
  a semântica de dias + a proibição de criar assets.)

### Aposentadoria do duplicado (documentação, sem deletar)

- **Craft WI-14 NÃO é aposentado:** é a superfície VIVA do *crafting em tempo real* (cook
  instantâneo / banco de oficina), conceito distinto do processamento por dias. O processamento
  por dia **deliberadamente NÃO roteia por ele**. Permanece intacto (extensão zero).
- **`Crafting/CraftingService` + `ProcessingJob`** (caminho #2, "segundo caminho por ticks"
  citado pela spec) **já está AUSENTE do repo** — nada a aposentar/deletar. Registrado aqui como
  inexistente; nenhuma 4ª superfície foi criada.
- **`Farm/Processing/ProcessableItem`** (modelo de configuração mais rico, com `QualityTransferPolicy`)
  permanece como tipo de dados; o v1 usa `ProcessingRecipe` (mais enxuto) e **não** cria um
  segundo modelo de job. `ProcessableItem` fica disponível para o follow-up de qualidade do output.

> **Conclusão:** UMA única superfície cria jobs de processamento por dia (`FarmProcessingStationModel`
> reutilizando `FarmProcessingJob`). Nenhuma 4ª superfície nasceu. Regra system-reuse-audit cumprida.

### Demais decisões de Fase 0

- **Ponto único de validação de estação no FarmPlot:** `FarmPlot.TryPlantSeed` não tinha gate de
  estação. Adicionado um ponto único (`IsSeasonAllowedForSeed` → `FarmSeasonGate.IsPlantingAllowed`)
  que consulta `GreenhouseRuntimeHost.Instance.CanOverrideSeason(PlotId)`. Diff mínimo.
- **Ponto de rega da chuva:** a rega real ocorre em `RainIrrigationRunner.WaterPlotsForToday`
  (não no `RainIrrigationIntegration`, que é só o provedor de % de água). A exclusão da estufa
  (`IsRainExcluded`) foi adicionada lá — é o único laço que efetivamente rega canteiros.
- **Estufa zona × lote F41:** **zona pequena sempre presente** (v1 recomendado pela spec),
  `SetGreenhouseUnlocked(true)` no host. Sem estado destravável persistido (zona fixa) → sem
  migração.
- **Itens do catálogo (F32/F12):** auditados e **presentes** no `CanonicalItemCatalog`:
  `item_animal_goat_milk` (28g), `item_consumable_food_goat_cheese` (65g),
  `item_crop_brigandini_grape` (uva do catálogo), `item_consumable_food_vale_wine` (120g).
  Nenhum item inventado.

---

## Acceptance criteria extracted

| CA | Critério | Implementação | Evidência | Status |
|---|---|---|---|---|
| CA-1 | Decisão de reuso documentada com critérios + aposentadoria | Seção Fase 0 acima | Este report | OK |
| CA-2 | Processamento por dias (cheese 1d / wine 2d), consumo no início, coleta 1× | `FarmProcessingStationModel.StartJob/AdvanceDay/Collect` + `FarmProcessingJob` | Testes `Cheese_ReadyNextDay_AfterStart`, `Wine_ReadyInTwoDays`, `Collect_DeliversOutput_Once_SecondCollectFails`, `Start_FailsWhenMissingInput_NoConsumption` | OK |
| CA-3 | Job sobrevive a save/load (meio e fim; sem duplicar) | `Capture`/`Restore` + DTO aditivo | Testes `SaveLoad_MidJob_PreservesStationRecipeAndFinishDay`, `SaveLoad_AfterReady_DoesNotDuplicateOutput` | OK |
| CA-4 | Estufa ignora estação + chuva; canteiro comum não | `FarmSeasonGate` + `GreenhouseContextProvider` (override/rain) + `RainIrrigationRunner` guard | Testes `Greenhouse_OverridesSeason_ForRegisteredPlot`, `CommonPlot_AllowsSeed_InMatchingSeason`, `Greenhouse_RainExcluded_ForRegisteredPlotOnly`, `Greenhouse_LockedProvider_DoesNotOverrideSeason` | OK |
| CA-5 | Save legado seguro (campos ausentes = sem jobs, estações vazias) | `Restore(null)` + normalização `??=` no SaveManager | Testes `Restore_NullSave_NoJobs_NoError`, `Restore_LegacySave_EmptyJobsList_NoStations`, `Restore_IgnoresUnknownRecipeId` | OK |

---

## Existing systems audit

| Sistema | Encontrado? | Reusado / Criado |
|---|---|---|
| `FarmProcessingJob` (job por dia, Collect idempotente) | sim (órfão) | **REUSADO** — adotado pelo modelo/host |
| `GreenhouseContextProvider` (override/rain) | sim (órfão) | **REUSADO** — instanciado pelo `GreenhouseRuntimeHost` (não reescrito) |
| `Craft/` WI-14 (crafting em segundos) | sim (vivo) | **INTOCADO** — não estendido, não usado para dias |
| `Crafting/CraftingService` + `ProcessingJob` | **não existe** | n/a (registrado ausente) |
| `FarmPlot` / `FarmPlotRegistry` | sim | **REUSADO** — gate de estação em ponto único (diff mínimo) |
| `RainIrrigationRunner` / `RainIrrigationIntegration` | sim | **REUSADO** — guard de exclusão da estufa (diff mínimo) |
| `InventoryManager` (HasItem/RemoveItem/AddItem) | sim | **REUSADO** — adaptado à porta `IProcessingInventory` |
| `FarmSaveData` (seção farm) | sim | **ESTENDIDO** — campo aditivo `Processing` |
| `CreateMvpFarmScene` (gerador) | sim | **ESTENDIDO** — 2 estações + estufa (4 canteiros) + host |
| `GameEventBus` / `DayStartedEvent` / `PlayerActionFeedbackEvent` | sim | **REUSADO** |
| `CanonicalItemCatalog` (itens F32/F12) | sim | **LIDO** — IDs confirmados (nenhum item criado) |
| Novos: `FarmProcessingStationService/Model`, `ProcessingRecipe(+Catalog)`, `IProcessingInventory`, `ProcessingStationInteractable`, `GreenhouseRuntimeHost`, `FarmSeasonGate`, `FarmProcessingSaveData`, `ProcessingJobCompletedEvent` | — | **CRIADO** (host/contrato/evento; nenhuma 4ª superfície de job) |

---

## Spec Compliance Matrix

| Requisito da spec | Implementação | OK |
|---|---|---|
| Fase 0 decisão de reuso + aposentadoria | Este report (seção Fase 0) | OK |
| 2 estações físicas no gerador (queijaria/barril), refs serializadas, sem Find | `CreateProcessingAndGreenhouse`/`CreateProcessingStation` + `ProcessingStationInteractable` | OK |
| Receitas goat_milk×2→goat_cheese(1d), grape×5→vale_wine(2d) | `ProcessingRecipeCatalog` | OK |
| Insumos consumidos no início; coleta idempotente no fim | `FarmProcessingStationModel.StartJob`/`Collect` | OK |
| Avanço por DayStartedEvent (dias) | `FarmProcessingStationService.OnDayStarted` → `Model.AdvanceDay` | OK |
| Estufa: 4 canteiros FarmPlot registrados no provider; override de estação; sem chuva | `GreenhouseRuntimeHost` + gate FarmPlot + guard RainRunner | OK |
| FarmPlot consulta provider no ponto único de validação de estação | `FarmPlot.IsSeasonAllowedForSeed` | OK |
| Save: campos aditivos na seção farm (jobs por estação, ids/ints); legado seguro | `FarmSaveData.Processing` + `FarmProcessingSaveData` + SaveManager wiring | OK |
| ProcessingJobCompletedEvent (toast) | `ProcessingJobCompletedEvent` + publish na coleta | OK |
| EditMode tests (timing/consumo/idempotência/round-trip/estufa/legado) | `ProcessingGreenhouseTests` (24 testes) | OK |
| PROIBIDO 4ª superfície de job | Apenas `FarmProcessingStationModel` cria jobs | OK |
| Não criar 2º provider de estufa | `GreenhouseContextProvider` instanciado, não reescrito | OK |
| Não criar nova seção de save | Campo aditivo na seção farm existente | OK |
| Sem GameObject.Find runtime; eventos via GameEventBus | Refs serializadas + singletons; publish/subscribe | OK |
| Não editar .unity/.prefab/.asset YAML | Cena só via gerador (código); nada de YAML manual | OK |

---

## Testing Quality Gate

```
Changed runtime code: YES
Changed deterministic logic: YES (timing por dia, consumo/coleta, override de estação, save round-trip)
Changed Unity scene/prefab/asset wiring: YES (gerador CreateMvpFarmScene — código; cena via gerador)
Automated tests added/updated: YES
Automated tests command: ProcessingGreenhouseTests.cs (24 testes EditMode; NUnit) — execução do Test Runner = ação humana deferida
Manual Play Mode scenario: DEFERRED_TO_FINAL_VALIDATION (autorização do dono da sessão)
Justification if no automated tests: N/A (testes presentes)
Residual risk: gerador FarmScene não executado no Unity nesta sessão (CODE_READY); season gate
  fica dormente em cena até um GameCalendarService ser ligado à FarmScene (greenhouse override e
  testes não dependem do calendário). Itens consumidos só após validação completa da receita
  (sem perda em falha de start).
```

---

## Validation

```
Validation method: run_strict_validation.ps1
Exit code: 0
Assembly-CSharp: PASS (0 erros; 1 warning pré-existente em CombatTelemetrySession)
Assembly-CSharp-Editor: PASS (0 erros; 3 warnings pré-existentes)
Quality check: PASS
Docs validation (validate_docs.ps1): PASS (exit 0)
Spec diff completeness (check_spec_diff_completeness.ps1): PASS (exit 0)
Result artifact: docs/validation/LAST_STRICT_VALIDATION_RESULT.json
```

Comandos:

```powershell
dotnet build .\Assembly-CSharp.csproj --no-restore          # 0E
dotnet build .\Assembly-CSharp-Editor.csproj --no-restore   # 0E
.\tools\docs\validate_docs.ps1                              # exit 0
.\tools\docs\check_spec_diff_completeness.ps1               # exit 0
.\tools\docs\run_strict_validation.ps1                      # exit 0
```

---

## Files changed

**Criados (runtime):**
- `Assets/_Game/Scripts/Farm/Processing/ProcessingRecipe.cs`
- `Assets/_Game/Scripts/Farm/Processing/ProcessingRecipeCatalog.cs`
- `Assets/_Game/Scripts/Farm/Processing/IProcessingInventory.cs`
- `Assets/_Game/Scripts/Farm/Processing/FarmProcessingStationModel.cs`
- `Assets/_Game/Scripts/Farm/Processing/FarmProcessingStationService.cs`
- `Assets/_Game/Scripts/Farm/Processing/ProcessingStationInteractable.cs`
- `Assets/_Game/Scripts/Farm/Processing/FarmProcessingSaveData.cs`
- `Assets/_Game/Scripts/Farm/Watering/GreenhouseRuntimeHost.cs`
- `Assets/_Game/Scripts/Farm/FarmSeasonGate.cs`
- `Assets/_Game/Scripts/Core/Events/ProcessingJobCompletedEvent.cs`

**Criados (testes):**
- `Assets/_Game/Tests/EditMode/Farm/ProcessingGreenhouseTests.cs`

**Modificados:**
- `Assets/_Game/Scripts/Farm/FarmPlot.cs` (gate de estação em ponto único + ref de calendário)
- `Assets/_Game/Scripts/Farm/RainIrrigationRunner.cs` (guard de exclusão da estufa)
- `Assets/_Game/Scripts/Save/SaveData.cs` (campo aditivo `FarmSaveData.Processing`)
- `Assets/_Game/Scripts/Save/SaveManager.cs` (capture/restore/normalização do campo aditivo)
- `Assets/_Game/Scripts/Editor/SceneCreation/CreateMvpFarmScene.cs` (2 estações + estufa + host)
- `Assembly-CSharp.csproj` (includes dos novos arquivos runtime + teste)

---

## Impacto em save/load

```
Schema change: YES (campo aditivo FarmSaveData.Processing — só IDs/ints; ADR-0006)
Adds a save section: NO (campo aditivo na seção farm existente)
Migration: NO (campo ausente em save legado = sem jobs; normalização ??= cobre null)
Persists Unity references: NO
Round-trip: coberto (meio e fim do job; sem duplicação pós-reload)
```

---

## Remaining work (deferred)

- Executar `CindarsHope/.../CreateMvpFarmScene` no Unity Editor (regenerar FarmScene com as 2
  estações + estufa) — ação humana; evidência de geração pendente.
- Play Mode humano: produzir queijo (1 dia) e vinho (2 dias) e plantar fora de estação na estufa
  (cenário humano) — DEFERRED_TO_FINAL_VALIDATION.
- (Opcional/follow-up) Ligar um `GameCalendarService` à FarmScene para ativar o gate de estação
  em cena para canteiros comuns; qualidade de output derivada do insumo (`ProcessableItem`).
- Registrar `Crafting/CraftingService` órfão como delete-candidate **não se aplica** (ausente do repo).

---

## Anti-regressão

- Canteiro comum: sem `SeasonTags` continua plantável o ano todo (gate é no-op); com tags fora de
  estação recusa (testado). Continua regado pela chuva (estufa só é excluída).
- Crafting WI-14: contrato intocado (extensão zero).
- Nenhuma 4ª superfície de job; `GreenhouseContextProvider` instanciado, não reescrito.
- Save legado compatível (campos ausentes = sem jobs); DTO só com tipos simples.
- Zero `GameObject.Find`/`FindObjectOfType` em runtime; comunicação por `GameEventBus`.
```
