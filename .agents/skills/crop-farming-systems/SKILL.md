---
name: crop-farming-systems
description: Estende o sistema de farm — soil/crop growth por dia, watering/irrigation/chuva, crop quality, fertilizer, resource node refresh, footprint/grid de plantio e harvest/processing quality — reusando os processors e services puros em Farm/* (NÃO criar paralelo). Use em specs da wave 05_* (soil_crop_growth_quality, watering_irrigation_rain_greenhouse, fertilizer_crop_quality, resource_node_refresh, harvest_processing_quality, farm_scale_tilemap, layout/placement grid).
---

# Skill: Sistemas de Crop e Farming

A farm já é um conjunto de processors/services puros e testáveis (sem Unity), separados por domínio em `Assets/_Game/Scripts/Farm/**`: growth, watering, quality, fertilizer, resource refresh, harvest, processing. O growth NÃO roda no `Update` — ele avança no day-transition. Antes de tocar qualquer regra de farm, audite o processor existente do domínio e estenda-o; não crie um `FarmManager` god paralelo.

## Quando usar

A spec mexe em:
- soil/crop growth avançando por dia (estágios, morte por falta de água, dormência sazonal);
- watering manual / irrigation / chuva regando plots;
- crop quality (Normal/Good/Excellent/...) e seus modifiers;
- fertilizer (aplicação, stacking, duração, modifiers de quality/yield);
- resource node refresh (regrow de árvores/rochas/forage por política);
- footprint/grid de plantio, layout, placement;
- harvest yield e processing quality.

## Quando NÃO usar

- A regra é "quando o dia vira / quando chove" no eixo de tempo → essa parte é `time-calendar-weather` (a farm CONSOME o evento; não republica o tempo).
- Adicionar o crop/resource como objeto clicável na scene → use `scene-interactable-wiring`.
- Balancear preço de venda / profit-per-day em moeda → use `economy-balance-tuning`.

## Leitura mínima

1. `CLAUDE.md`
2. Spec alvo
3. Report de farm anterior relevante, se a spec citar (ex.: contrato de scale/layout)

## Sistemas existentes (reusar, não duplicar)

| Domínio | Classe / arquivo | Papel |
|---|---|---|
| Crop growth | `CropGrowthProcessor` + `CropGrowthState` — `Assets/_Game/Scripts/Farm/Crops/` | avança 1 dia: idempotência por `LastProcessedDay`, dormência fora de estação, morte por dias sem água, `IsReadyToHarvest` |
| Crop quality | `CropQualityResolver` + `CropQualityInput` — `Assets/_Game/Scripts/Farm/Crops/CropQualityResolver.cs` | score → `CropQualityTier` (watering consistency + season + fertilizer) |
| Definição de crop | `CropDefinitionData` / `SeedDataSO` — `Assets/_Game/Scripts/Farm/Crops/`, `Assets/_Game/Scripts/Farm/Data/SeedDataSO.cs` | `GrowthDays`, `RequiresWater`, `DiesAfterDaysWithoutWater`, yield min/max |
| Watering | `FarmWateringService` + `FarmPlotWaterState` + `WaterSource` — `Assets/_Game/Scripts/Farm/Watering/` | manual / chuva / storm / irrigation; `ResetDayWaterState`; greenhouse/interior nunca regados por chuva |
| Greenhouse | `GreenhouseContextProvider` — `Assets/_Game/Scripts/Farm/Watering/` | contexto de plot interior/greenhouse |
| Chuva → rega | `RainIrrigationIntegration` — `Assets/_Game/Scripts/Farm/RainIrrigationIntegration.cs` | lê clima via `WeatherGenerator` e rega só crops externos |
| Fertilizer | `FertilizerApplicationService` + `FertilizerDefinition` + `SoilModifierState` — `Assets/_Game/Scripts/Farm/Fertilizer/` | aplicação, stacking policy, duração, modifiers de quality/yield |
| Resource refresh | `FarmResourceRefreshProcessor` + `ResourceNodeInstanceState` + `ResourceNodeDefinition` — `Assets/_Game/Scripts/Farm/Resources/` | políticas FixedDays/NextDayChance/SeasonStart/WeatherTriggered + forbidden zones |
| Harvest | `CropYieldResolver` + `HarvestCommand`/`HarvestResult` — `Assets/_Game/Scripts/Farm/Harvest/` | yield determinístico por seed + bônus de quality |
| Processing | `FarmProcessingJob` / `ProcessingRecipe` — `Assets/_Game/Scripts/Farm/Processing/` | jobs de processamento (timing por dia) |
| Plot/scene | `FarmPlot`, `FarmPlotState`, `FarmPlotSaveData` — `Assets/_Game/Scripts/Farm/` | estado e save do plot |

Se o conceito já está na tabela, **estenda** (parâmetro/policy/subclasse). Criar um segundo growth ou um `FarmManager` que reimplementa watering é `NEEDS_REWORK` (ver skill `system-reuse-audit`).

## Regra central: growth avança no day-transition, não no Update

A farm é movida por **um tick por dia**, não por frame. O fluxo é: a wave de tempo publica `DayStartedEvent`/`WeatherChangedEvent` → a farm reage e processa o dia. Por plot, na ordem:

1. resolver rega do dia (manual já marcada; chuva via `FarmWateringService.ApplyRainWatering`; irrigation via `ApplyIrrigationCoverage`);
2. `CropGrowthProcessor.Process(state, input)` com `IsWateredToday`, `IsValidSeason`, `CurrentDay`;
3. expirar/consumir fertilizer (`SoilModifierState`);
4. `FarmResourceRefreshProcessor.ProcessBatch(nodes, ctx)` para regrow;
5. `FarmWateringService.ResetDayWaterStates(plots)` ao fim, para o próximo dia começar seco.

Invariantes já codificadas que devem ser preservadas:
- **Idempotência por dia:** `CropGrowthProcessor` ignora reprocessar o mesmo `LastProcessedDay` — nunca dê dois avanços no mesmo dia (importante após reload).
- **Dormência sazonal:** fora da estação válida não cresce **nem morre**.
- **Morte por seca:** só conta `DaysWithoutWater` quando `Definition.RequiresWater`.
- **Chuva ≠ greenhouse:** `ApplyRainWatering` pula `IsGreenhouse`/`IsInterior`/não-`IsExternal`.

## Ligação com a wave de tempo (time-calendar-weather)

A farm é **consumidora** do eixo de tempo, nunca produtora:
- avanço de growth é disparado por `DayStartedEvent`;
- rega por chuva deriva do clima do dia (`WeatherGenerator.GenerateWeather` / `WorldWeatherService.IsWetWeather`), que é determinístico por dia;
- season válida vem de `GameDate.CurrentSeason`.

Não recalcule clima/estação dentro da farm com lógica própria — leia da wave 02. Ver skill `time-calendar-weather`. Comunicação via `GameEventBus` (skill `event-bus-pattern`); publique feedback de harvest/colheita por evento, não por chamada direta.

## Determinismo (ligação com rng-and-determinism)

Quality rolls, yield e resource refresh devem ser seeded — mesma entrada ⇒ mesmo resultado após reload:
- `CropYieldResolver.Resolve` já usa `input.Seed` (sem `UnityEngine.Random`);
- `FarmResourceRefreshProcessor` (política `NextDayChance`) usa roll determinístico `((node.RandomSeed + ctx.CurrentDay * 31) % 100)`.

Ao adicionar randomness nova (quality variance, drop extra), derive de `worldSeed`/`plotId`/`day` com o padrão de salt por sistema. Nunca `Guid.NewGuid()`/`DateTime.Now`. Ver skill `rng-and-determinism` (ADR-0005).

## Scene e depletion (ligação com scene-interactable-wiring)

Quando o crop/resource vira objeto clicável na scene, ele é um `IInteractable` registrado via CreateScene editor script — nunca editando o `.unity` YAML. A depletion (resource colhido) só ocorre no **sucesso** do `InventoryManager.AddItem`; refresh devolve o node via `FarmResourceRefreshProcessor` (não respawn aleatório). Ver skill `scene-interactable-wiring`.

## Economy (ligação com economy-balance-tuning)

Yield × quality × preço fecha o loop de profit-per-day. Quando a spec definir números de yield/quality/seed-cost/sell-price que afetam o profit por dia, valide a curva com `economy-balance-tuning` (sink/source). Quality tiers (`CropQualityTier`) escalam preço — mantenha consistente com a tabela de economy.

## Interação com save (ligação com save-load-pattern)

Persistir só estado simples por plot/node — IDs, ints, bools, enums — nunca `GameObject`/`Sprite`/`ScriptableObject` (rule `save-dto-simple-types-only`). O padrão existe em `FarmPlotSaveData`. Cubra:
- estado de growth (`DaysGrown`, `DaysWithoutWater`, `IsDead`, `IsReadyToHarvest`, `LastProcessedDay`);
- water state por dia e fertilizer/modifier ativo (`SoilModifierState`);
- estado de depletion/`NextEligibleRefreshDay` dos resource nodes.

Após reload, o `LastProcessedDay` evita re-avançar o crop. Migrations backward-compatible com default seguro. Ver skill `save-load-pattern`.

## Testes (ligação com editmode-test-authoring)

Toda essa lógica é determinística e pura ⇒ EditMode tests obrigatórios (Testing Quality Gate), em `Assets/_Game/Tests/EditMode/**` (skill `editmode-test-authoring`):

- `CropGrowthProcessor`: cresce quando regado+estação ok; não cresce/não morre fora de estação; morre após `DiesAfterDaysWithoutWater`; idempotência no mesmo `LastProcessedDay`; respeita `IsReadyToHarvest`/`IsDead`.
- `FarmWateringService`: chuva rega só externos (pula greenhouse/interior); irrigation só por `coverageId`; `ResetDayWaterState`.
- `CropQualityResolver`: thresholds Good/Excellent com season/fertilizer bonus.
- `FertilizerApplicationService`: stacking policies, duração/expiração, `RequiredFarmLevel`, endgame reserved.
- `FarmResourceRefreshProcessor`: cada política (FixedDays/NextDayChance/SeasonStart/WeatherTriggered); forbidden zones; same-day guard; zone/farm-level gates; determinismo do roll.
- `CropYieldResolver`: yield dentro do range por seed + bônus de Excellent.

A parte de feel (plantar/regar/colher na scene) precisa de human scenario em `docs/validation/playmode/<spec_id>_human_test_scenario.md`.

## Red flags (pare e reveja o escopo)

- Rodar growth no `Update`/por frame em vez de por day-transition.
- Criar `FarmManager`/`CropSystem` god paralelo em vez de estender os processors por domínio.
- Avançar o crop mais de uma vez no mesmo dia (ignorar `LastProcessedDay`) — bug clássico pós-reload.
- Chuva regando greenhouse/interior (`ApplyRainWatering` deve pular).
- Recalcular clima/estação dentro da farm com lógica própria em vez de ler da wave 02.
- `UnityEngine.Random`/`Guid.NewGuid()` para quality/yield/refresh (quebra reload determinístico).
- Depletar resource antes do sucesso do `AddItem` (item perdido com inventory cheio).
- Persistir Unity refs no save da farm.

## Onde se aplica

Wave 05_*: `soil_crop_growth_quality`, `watering_irrigation_rain_greenhouse`, `fertilizer_crop_quality`, `resource_node_refresh`, `harvest_processing_quality`, `farm_scale_tilemap`, layout/placement grid.

## Fechamento

Ao fechar uma tarefa de farm, reporte:
- qual processor/service existente foi estendido (nenhum paralelo criado);
- que growth avança por day-transition e que a idempotência por `LastProcessedDay` foi preservada;
- como chuva/irrigation/season são lidos da wave de tempo (não reimplementados);
- quality/yield/refresh determinísticos (seed/worldSeed), sem RNG não-semeado;
- o que entrou no save (simple types) e a migration;
- evidência de EditMode tests + human scenario para a parte de scene/feel.

## Relacionados

- skill `time-calendar-weather` — growth no day-transition; chuva/estação
- skill `scene-interactable-wiring` — crops/resources como `IInteractable` + depletion guard
- skill `economy-balance-tuning` — profit-per-day (yield × quality × preço)
- skill `rng-and-determinism` — quality/yield/refresh seeded
- skill `save-load-pattern` + rule `save-dto-simple-types-only` — estado de plot/node
- skill `editmode-test-authoring` — testes de deterministic logic
- skill `system-reuse-audit` — Phase 0 antes de criar qualquer service de farm
