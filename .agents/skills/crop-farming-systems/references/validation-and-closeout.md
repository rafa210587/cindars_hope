# Crop farming validation and closeout

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
