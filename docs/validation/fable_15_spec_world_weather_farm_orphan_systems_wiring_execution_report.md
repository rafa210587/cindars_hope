# Execution Report — fable_15_spec_world_weather_farm_orphan_systems_wiring

> **Spec:** `.specs/a_implementar/fable/fable_15_spec_world_weather_farm_orphan_systems_wiring.md`
> **Data:** 2026-06-12
> **Status:** BUILD_VALIDATED
> **Executor:** Claude (FABLE master plan — Batch 0, spec 1/42)

validated_adrs: []
validated_game_rules: [farm_rules.md, time_rules.md]

---

## Acceptance criteria extracted

| CA | Critério | Evidência |
|----|----------|-----------|
| CA-1 | Clima vivo e determinístico; WeatherChangedEvent no day transition | `WorldWeatherService.ResolveWeatherForDay` (puro) + publish no `OnDayStarted`; teste `ResolveWeatherForDay_IsDeterministic_AndMatchesGenerator` |
| CA-2 | Chuva rega canteiros plantados no início do dia | `RainIrrigationRunner` (end-of-frame após handlers dos plots) + `FarmPlot.TryWaterFromRain`; testes `ShouldWaterState_*` |
| CA-3 | Qualidade e fertilizante reais na colheita | `FarmPlot.TryHarvest` consulta `CropQualityResolver` + `FarmFertilityRuntime`; bônus = unidades extras; testes `BuildQualityInput_*`, `FertilityRuntime_*` |
| CA-4 | Refresh de nós por política após N dias | `FarmResourceRefreshRuntime` hospeda `FarmResourceRefreshProcessor`; `FarmResourceInteractable` registra ao depletar; teste `RefreshProcessor_FixedDays_RespectsEligibleDay` |

## Existing systems audit

```text
REUSADOS SEM REESCRITA (os 5 módulos órfãos):
- WeatherGenerator (WAVE 02, estático, determinístico dia%4) — consumido pelo WorldWeatherService
- RainIrrigationIntegration (WAVE 02) — adicionada à FarmScene pelo gerador; runner fino a liga
  ao FarmPlotRegistry (módulo intocado; calendário ausente na cena → fallback documentado abaixo)
- FarmResourceRefreshProcessor (WAVE 05, puro) — hospedado pelo FarmResourceRefreshRuntime
- CropQualityResolver (WAVE 05, puro) — consumido no TryHarvest
- FertilizerApplicationService (WAVE 05, puro) — hospedado pelo FarmFertilityRuntime (static host)

REUSADOS DE APOIO: DayStartedEvent, GameEventBus, GameDate/Season, FarmPlotRegistry,
InventoryManager, padrão de bootstrap FarmDailyGoalRuntimeBootstrap, ItemDataInitializer.

NÃO EXISTIA (criado): WorldWeatherService, WeatherChangedEvent, RainIrrigationRunner,
FarmFertilityRuntime, FarmResourceRefreshRuntime.
```

## Spec Compliance Matrix

| Requisito da spec | Implementação | Status |
|---|---|---|
| WorldWeatherService bootstrap + WeatherChangedEvent | `World/Weather/WorldWeatherService.cs` (+ `WorldWeatherRuntimeBootstrap`) | OK |
| RainIrrigationIntegration na FarmScene via gerador | `CreateMvpFarmScene.CreateRainIrrigation` (integration + runner + refs serializadas) | OK |
| Chuva rega plantados (tempestade rega arados também) | `RainIrrigationRunner.ShouldWaterState` 50%/100% | OK |
| FarmResourceRefreshProcessor no day transition | `Farm/Resources/FarmResourceRefreshRuntime.cs` (host + bootstrap) | OK |
| Qualidade via CropQualityResolver na colheita | `FarmPlot.TryHarvest` + `BuildQualityInput` (consistência de rega + fertilizante) | OK |
| Fertilizante via item no canteiro | Ação "Aplicar Fertilizante" no menu do FarmPlot + consumo de item + `FarmFertilityRuntime` | OK |
| 2 itens de fertilizante no initializer | `ItemDataInitializer` (`fertilizer_simple` 25g, `fertilizer_improved` 60g) | OK |
| DebugHud mostra clima | `DebugHud.DrawWorldState` linha "Clima:" | OK |
| Save aditivo (fertilizerId) | `FarmPlotSaveData.FertilizerId` + `WateredDaysCount`; restore re-aplica modificador | OK |
| EditMode tests | `Assets/_Game/Tests/EditMode/Farm/OrphanSystemsWiringTests.cs` (16 testes) | OK |

Decisões documentadas (previstas pela spec):
- **Qualidade sem suporte em ItemStack** → bônus de QUANTIDADE (Good +1, Excellent +2) até F32
  introduzir itens `_silver/_gold`. Yield do fertilizante = floor(amount × YieldModifier).
- **SeasonMatch** = true fixo (SeedDataSO não declara estação; follow-up anotado, sem expandir escopo).
- **Calendário ausente na FarmScene** → RainIrrigationRunner prefere o módulo original e cai para
  WorldWeatherService (mesma função determinística) quando o módulo não tem calendário wired.
- **Crops morrendo sem água** já existia (DaysWithoutWater ≥ 3 → Dead) — sem follow-up.

## Validation

```text
Validation method: run_strict_validation.ps1
Exit code: 0
Docs validation: PASS (zero erros)
Assembly-CSharp: PASS (0 erros, 0 avisos)
Assembly-CSharp-Editor: PASS (0 erros, 0 avisos)
Quality check: PASS
Diff completeness: PASS (WARNs = reports legados pré-existentes, fora do escopo)
Result artifact: docs/validation/LAST_STRICT_VALIDATION_RESULT.json
```

## Testing Quality Gate

```text
Changed runtime code: YES
Changed deterministic logic: YES
Changed Unity scene/prefab/asset wiring: via gerador editor apenas (CreateMvpFarmScene)
Automated tests added/updated: YES (16 EditMode tests em OrphanSystemsWiringTests.cs)
Automated tests command: Unity Test Runner EditMode (compilam no Assembly-CSharp; execução no lote final)
Manual Play Mode scenario: dia de chuva + colheita fertilizada — DEFERRED_TO_FINAL_VALIDATION (lote)
Justification if no automated tests: N/A
Residual risk: cena FarmScene precisa ser REGERADA no Unity Editor para ganhar o objeto
RainIrrigation (bootstraps runtime cobrem clima/refresh mesmo sem regeneração); execução
real dos testes EditMode pendente do Unity (dotnet compila, não executa NUnit).
```

## Honest status rationale

BUILD_VALIDATED — não mais que isso: os builds passam com 0E/0W e a lógica nova é coberta
por testes EditMode compilados, mas (a) o Unity Editor não foi aberto (cena não regenerada,
testes não executados no Test Runner), (b) o cenário humano de chuva/qualidade é parte do
lote final de Play Mode. Nenhum claim de UNITY_VALIDATED/PLAYMODE.

## Remaining work

- Regenerar FarmScene no Unity (menu existente) para materializar o objeto RainIrrigation.
- F32 migra bônus de qualidade para itens `_silver/_gold`.
- F20 mostra previsão de amanhã na UI (TomorrowWeather já exposto).
