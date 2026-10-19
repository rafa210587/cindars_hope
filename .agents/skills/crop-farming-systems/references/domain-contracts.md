# Crop farming domain contracts

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
