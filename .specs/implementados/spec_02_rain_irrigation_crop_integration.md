## Source Map Compliance

### Global sources read

- docs/design/SPEC_SOURCE_MAP.md
- docs/design/SPECIFICATION_PROCESS.md
- docs/design/lore/VAALARA_GAME_CANON_DIRECTION_v1.0.md
- .specs/SPEC_IMPLEMENTABLE_TEMPLATE.md
- .specs/SPEC_WAVE_EXECUTION_PROTOCOL.md
- .specs/SPEC_VALIDATION_MATRIX_MASTER.md
- .specs/SPEC_EXISTING_IMPLEMENTATION_AUDIT.md
- docs/project/CURRENT_STATE.md

### Domain directions read

- docs/design/gameplay/world/SEASONS_CALENDAR_WEATHER_LUNAR_DIRECTION.md
- docs/design/gameplay/save_load/SAVE_LOAD_FULL_STATE_DIRECTION.md
- docs/design/gameplay/ui_ux/UI_UX_FULL_GAMEPLAY_DIRECTION.md
- docs/design/gameplay/ui_ux/UI_UX_MENU_SCREEN_FLOWS_DIRECTION.md

### Required interpretation

```text
Esta spec é derivada dos directions/refinements canônicos e do roadmap macro.
Ela não substitui os directions.
Ela transforma parte do refinement em contrato implementável, com escopo, locks, validações e quality gate.
Quando houver divergência entre esta spec e os directions, o executor deve parar e registrar CONFLICT no execution report.
```

---

## Direction / Refinement Coverage

### Covered from directions

- Tempo, calendário, estação, clima, chuva, lua, festival, UI de calendário ou persistência desses estados conforme escopo da spec.
- Regra de visibilidade: eventos conhecidos aparecem; eventos secretos não são revelados cedo.
- Save/load e restore order para world state quando aplicável.

### Deferred / future from directions

- Minigames de festival.
- Clima visual final, VFX/SFX e assets.
- NPC schedules completos, aniversários/social completo e balance final de clima.

### Explicitly not redefined here

- Farm crop growth completo.
- Quest runtime completo.
- UI visual/prefab final.
- Sistema social/romance/pets/companions.

# SPEC — Rain Irrigation and Crop Integration

    > **Spec ID:** `02_spec_rain_irrigation_crop_integration`  
    > **Status:** A implementar  
> **Revision:** EXPANDED_01_05_CORRECTED  
    > **Wave:** WAVE 02 — Time / Calendar / Weather / Lunar  
    > **Priority:** P1  
    > **Type:** Runtime / Integration / Farm / Weather / Validation  
    > **Domain:** Farm / Weather / Irrigation / Crops  
    > **Parallelizable:** NO  
    > **Parallel group:** WAVE_02_WORLD_FARM_LOCKED  
    > **Can run with:** N/A during execution.  
    > **Must not run with:** weather generation, crop season rules, farm world persistence, save state, day transition, or any farm tile/crop save spec.  
    > **Repo lock scope:** `Assets/_Game/Scripts/Farm/**`, `Assets/_Game/Scripts/World/**`, `Assets/_Game/Scripts/Weather/**`, `Assets/_Game/Scripts/Save/**`, farm tile/crop data, `docs/validation/02_spec_rain_irrigation_crop_integration_execution_report.md`.  
    > **Depends on:** WAVE 01, 01Q, `02_spec_time_clock_day_transition_runtime.md`, `02_spec_calendar_season_year_runtime.md`, `02_spec_weather_generation_forecast_runtime.md`.  
    > **Blocks:** crop season/weather rules, farm world persistence, processing/day transition hardening, weather UI claims about crop watering.  
    > **Scope:** connect Rain/Storm to external crop watering in the existing farm system without redefining crop growth, irrigation devices, magical watering or greenhouse rules.  
    > **Out of scope:** weather generation, crop balance, crop quality/fertilizer, greenhouse implementation, magical irrigation, farm buildings, UI polish, Play Mode human execution.

    ---

    # /speckit.specify

    ## 1. Contexto

    The world direction states that Rain wets external areas and waters external crops, but does not wet greenhouse/interiors, does not activate magical irrigation automatically, does not replace advanced irrigation systems, and does not solve narrative drought if an event says otherwise.

    It also states that crops can depend on season, water, fertilizer, soil quality, weather, greenhouse, moon, Fonte, Água Viva, Mana and corruption, but common crops follow season and water. Storm can water crops and may create next-day debris/resources, but severe crop destruction is explicitly avoided in baseline.

    This spec must connect weather to farm watering as an integration layer only.

    ## 2. Problema

    Without a single rain/crop integration contract:

    ```text
    Rain may be generated but not affect crops;
    farm may implement its own weather state;
    greenhouse/interior tiles may be watered incorrectly;
    magical irrigation may be triggered by normal rain;
    drought/quest exceptions may be ignored;
    watering may be applied twice in day transition;
    save/load may persist derived rain effects incorrectly;
    tests may not cover external-only watering.
    ```

    ## 3. Objetivo

    Implement or harden a deterministic integration that applies weather water effects to valid external farm tiles at the correct time.

    Required outcome:

    ```text
    Rain/Storm can water external valid crop tiles;
    Snow does not water common crops;
    Sunny/Cloudy/Fog do not water crops by default;
    greenhouse/interior/covered tiles are excluded;
    magical irrigation remains separate;
    day transition order is explicit;
    EditMode coverage exists when practical.
    ```

    ## 4. Fontes obrigatórias lidas

    General sources:

    ```text
    .specs/SPEC_IMPLEMENTABLE_TEMPLATE.md
.specs/SPEC_WAVE_EXECUTION_PROTOCOL.md
.specs/SPEC_VALIDATION_MATRIX_MASTER.md
.specs/SPEC_EXISTING_IMPLEMENTATION_AUDIT.md
.specs/SPEC_GENERATION_ROADMAP_MASTER.md
.specs/SPEC_REGISTRY_TO_IMPLEMENT.md
docs/design/SPEC_SOURCE_MAP.md
docs/design/SPECIFICATION_PROCESS.md
docs/project/CURRENT_STATE.md
docs/IMPLEMENTATION_STATUS.md
CLAUDE.md
AGENTS.md
.claude/rules/testing-quality-gate.md
.claude/skills/spec-execution/SKILL.md
.claude/skills/unity-validation/SKILL.md
    ```

    Domain sources:

    ```text
    docs/design/gameplay/world/SEASONS_CALENDAR_WEATHER_LUNAR_DIRECTION.md
docs/design/gameplay/save_load/SAVE_LOAD_FULL_STATE_DIRECTION.md
    docs/design/gameplay/farm/FARM_DESIGN_DIRECTION_v1.3.md
docs/design/gameplay/loot_crafting_economy/LOOT_CRAFTING_ECONOMY_DIRECTION.md
docs/design/gameplay/loot_crafting_economy/ECONOMY_PRICING_STOCK_REFRESH_DIRECTION.md
    ```

    Conditional sources:

    ```text
    .specs/a_implementar/02_spec_weather_generation_forecast_runtime.md
    .specs/a_implementar/02_spec_time_clock_day_transition_runtime.md
    .specs/a_implementar/02_spec_calendar_season_year_runtime.md
    ```

    ## 5. Estado atual do repo

    Audit report says farm planting/irrigation exists in some complete form, while broader farm loop/world activities remain partial. Weather/calendar/lunar complete runtime is not proven.

    Execution must audit locally:

    ```text
    farm tile state;
    watered state;
    crop growth timing;
    irrigation services/tools;
    greenhouse/interior/covered tile flags;
    day transition processing;
    farm save DTOs.
    ```

    ## 6. User stories / engineering stories

    ```text
    As a farmer, I expect rain to reduce manual watering on outdoor crops.
    As crop growth, I need a clear WateredState set before growth processing.
    As greenhouse, I must not receive outdoor rain unless an explicit future rule says so.
    As quest/narrative drought, I need exception hooks so rain does not override story rules.
    As save/load, I need rain effects stored as tile/crop state, not as weather VFX.
    ```

    ## 7. Escopo

    Includes:

    ```text
    audit existing farm watering/crop systems;
    define weather-to-watering mapping;
    define external/covered/greenhouse eligibility;
    define day transition timing for rain watering;
    add tests/validators for rain watering rules when practical;
    report save/load impacts and residual risks.
    ```

    ## 8. Fora de escopo

    Excludes:

    ```text
    weather generation;
    crop season/death/growth balance;
    fertilizer/quality;
    greenhouse full rules;
    magical irrigation;
    Água Viva/Mana special crops;
    Storm debris/resource spawning;
    UI/VFX/SFX polish;
    Play Mode human execution.
    ```

    ## 9. Regras de não duplicação

    ```text
    Do not create second farm weather service.
    Do not let farm own weather generation.
    Do not create a new crop growth engine.
    Do not make Rain replace irrigation devices/magic.
    Do not persist weather VFX as crop state.
    Do not water interiors/greenhouse unless explicit flag says so.
    ```

    ## 10. Critérios de aceite

    ### 10.1 Weather-to-water mapping

    ```text
    Rain: waters external valid crop tiles.
    Storm: waters external valid crop tiles; extra Storm effects deferred.
    Snow: does not water common crops.
    Sunny/Cloudy/Fog: no automatic watering.
    Heat/Cold/Wind future modifiers: no baseline watering unless future spec.
    ```

    ### 10.2 Tile eligibility

    The implementation/report must identify or introduce safe checks for:

    ```text
    IsOutdoor;
    IsCovered;
    IsGreenhouse;
    HasCrop;
    CanReceiveRainWater;
    NarrativeWeatherOverride/DroughtBlock if such system exists or future hook is needed.
    ```

    ### 10.3 Day transition timing

    Rain watering must happen before crop growth consumes water for the day, or the report must document current order and residual risk.

    ### 10.4 Tests/validation

    When practical, test:

    ```text
    Rain waters outdoor tile;
    Rain does not water greenhouse/covered tile;
    Snow does not water common crop;
    Storm waters outdoor tile without destructive side effects;
    weather integration is idempotent for one day transition.
    ```

    ### 10.5 Report

    Create:

    ```text
    docs/validation/02_spec_rain_irrigation_crop_integration_execution_report.md
    ```

    ---

    # /speckit.plan

    ## 11. Arquitetura alvo

    Potential target, depending on existing code:

    ```text
    Assets/_Game/Scripts/Farm/Weather/FarmWeatherIrrigationService.cs
    Assets/_Game/Scripts/Farm/Crops/CropWateringRules.cs
    Assets/_Game/Tests/EditMode/Farm/FarmWeatherIrrigationTests.cs
    docs/validation/02_spec_rain_irrigation_crop_integration_execution_report.md
    ```

    Consolidate existing farm/weather adapters instead of creating parallel systems.

    ## 12. Contratos, dados e eventos

    ### 12.1 Data contracts

    ```text
    WeatherId/WeatherType comes from weather spec.
    Crop/tile WateredState remains farm-owned.
    Tile coverage/greenhouse flags remain farm/world-owned.
    ```

    ### 12.2 Runtime contracts

    ```text
    Weather service notifies/provides current weather.
    Farm integration translates weather into farm tile watering.
    Crop growth consumes farm-owned WateredState.
    ```

    ### 12.3 Event contracts

    ```text
    RainAppliedToFarmEvent optional notification only.
    WeatherChangedEvent must not directly mutate crops unless farm adapter owns subscription.
    ```

    ### 12.4 Save contracts

    ```text
    Does this change save schema? SHOULD BE NO.
    Rain effect should be reflected in tile/crop state if already persisted.
    Do not save weather VFX or forecast text.
    ```

    ### 12.5 UI contracts

    N/A.

    ## 13. Sistemas afetados

    ```text
    Weather state;
    farm tile/crop watering;
    day transition;
    save/load tile state;
    future crop growth/season rules.
    ```

    ## 14. Arquivos permitidos

    ```text
    Assets/_Game/Scripts/Farm/**
    Assets/_Game/Scripts/World/**
    Assets/_Game/Scripts/Weather/**
    Assets/_Game/Scripts/Time/**
    Assets/_Game/Scripts/Save/**
    Assets/_Game/Tests/EditMode/Farm/**
    Assets/_Game/Scripts/Editor/Validation/**
    docs/validation/02_spec_rain_irrigation_crop_integration_execution_report.md
    ```

    ## 15. Arquivos proibidos

    ```text
    Packages/**
    ProjectSettings/**
    Assets/**/*.unity
    Assets/**/*.prefab
    Assets/**/*.asset unless test data only and explicitly reported
    .specs/SPEC_EXECUTION_ORDER.md
    .specs/implementados/**
    docs/refinements/implementados/**
    docs/project/CURRENT_STATE.md
    PROJECT_LOG.md
    ```

    ## 16. Estratégia de implementação

    ### Fase 0 — Auditoria local obrigatória

    ```bash
    rg -n "Rain|Storm|Snow|Weather|Watered|Irrigation|Crop|FarmTile|Greenhouse|Covered|DayTransition" Assets/_Game/Scripts/Farm Assets/_Game/Scripts/World Assets/_Game/Scripts/Time
    ```

    ### Fase 1 — Integration contract

    ```text
    1. Identify existing crop water state.
    2. Identify timing of crop growth/day transition.
    3. Identify weather source.
    4. Implement/harden adapter only.
    ```

    ### Fase 2 — Validation

    ```text
    1. Add EditMode tests for eligibility/idempotency when practical.
    2. Run builds/Unity compile.
    3. Create report.
    ```

    ## 17. Ordem segura de execução

    ```text
    weather generation -> farm watering integration -> crop growth/season rules -> farm save persistence
    ```

    ## 18. Paralelização

    - Parallelizable: NO.
    - Reason: touches farm/weather/day transition interaction.

    ## 19. Impacto em save/load

    ```text
    Does this change save schema? SHOULD BE NO.
    Does this add save section? NO.
    Requires migration? NO unless tile state schema changes; then STOP.
    Persist Unity references? MUST BE NO.
    ```

    ## 20. Impacto em eventos

    ```text
    Adds events: SHOULD BE NO unless notification already exists.
    Changes existing events: NO.
    Requires unsubscribe pattern: YES if subscribing to weather/day events.
    ```

    ## 21. Impacto em UI/Unity

    ```text
    Changes UI: NO
    Changes scenes: NO
    Changes prefabs: NO
    Requires PlayMode final validation: YES if runtime scene farm watering is touched; DEFERRED_TO_FINAL_VALIDATION.
    Human validation timing: DEFERRED_TO_FINAL_VALIDATION if scene gameplay scenario is added.
    ```

    ## 22. Riscos técnicos

    ```text
    Risco: applying rain twice.
    Mitigação: idempotency by day/transition.

    Risco: watering greenhouse.
    Mitigação: explicit eligibility flags/tests.

    Risco: weather generation hidden inside farm.
    Mitigação: farm consumes weather, never owns generation.
    ```

    ## 23. Rollback

    ```text
    Remove adapter/tests/report.
    Revert any farm watering code touched.
    No scene/prefab/schema rollback expected.
    ```

    ---

    # /speckit.tasks

    ## 24. Tasks

    - [ ] T001 — Read sources and confirm branch.
    - [ ] T002 — Audit farm watering/crop/tile systems.
    - [ ] T003 — Audit weather source/current weather API.
    - [ ] T004 — Define rain/storm/snow mapping.
    - [ ] T005 — Implement/harden adapter if needed.
    - [ ] T006 — Add tests/validators when practical.
    - [ ] T007 — Run validations.
    - [ ] T008 — Create execution report.

## 23A. Execution Readiness Matrix

| Área | Pergunta obrigatória | Evidência esperada | Status se faltar |
|---|---|---|---|
| Fonte canônica | A execução leu Source Map e directions do domínio? | Lista de fontes no execution report. | PARTIAL |
| Estado real do repo | Sistemas existentes foram auditados antes de criar novos? | Comandos `rg` e achados no report. | PARTIAL |
| Não duplicação | Existe sistema equivalente já implementado/parcial? | Decisão REUSE/HARDEN/CREATE/DEFER. | BLOCKED se duplicar |
| Escopo | A execução ficou dentro de world/time/calendar/weather/lunar? | Arquivos alterados e justificativa. | PARTIAL |
| Save/load | Houve schema change? | Declaração explícita NO ou STOP se migration necessária. | BLOCKED se alterar sem migration |
| Eventos | Publishers/subscribers/lifecycle foram mapeados? | Mapa de eventos e unsubscribe policy se houver. | PARTIAL |
| UI/PlayMode | Há fluxo visual ou gameplay integrado? | Cenário final deferido documentado. | BUILD_VALIDATED no máximo se ausente |
| Testes | Há lógica determinística nova? | EditMode test ou NOT RUN justificado. | PARTIAL |
| Report | Execution report foi criado? | `docs/validation/02_spec_rain_irrigation_crop_integration_execution_report.md`. | PARTIAL |

---

## 23B. Local Audit Checklist

Antes de alterar qualquer arquivo, Claude Code/Codex deve rodar e registrar no execution report:

```bash
rg -n "Time|Clock|Calendar|Season|Weather|Rain|Irrigation|Lunar|Festival|DayTransition|Save" Assets/_Game/Scripts docs/design .specs
rg -n "TODO|FIXME|HACK|PARTIAL|DEFERRED|BUILD_VALIDATED|ACCEPTED" .specs docs/validation docs/IMPLEMENTATION_STATUS.md docs/project/CURRENT_STATE.md
```

A execução deve classificar cada achado como:

```text
EXISTING_CANONICAL
  Sistema já existe e deve ser reaproveitado/endurecido.

EXISTING_PARTIAL
  Sistema existe, mas precisa hardening/delta.

MISSING_SAFE_TO_CREATE
  Sistema não existe e criação é pequena, isolada e dentro do escopo.

MISSING_BUT_DEFER
  Sistema não existe, mas criação exigiria outro domínio/spec.

CONFLICT
  Há dois caminhos possíveis ou contrato divergente. Parar e reportar.
```

---

## 23C. Functional Acceptance Scenarios

### Scenario 1 — Happy path

```text
Given o sistema base relacionado a world/time/calendar/weather/lunar existe ou foi criado de forma mínima
When o fluxo principal desta spec é executado
Then o resultado segue o direction canônico
And nenhum sistema paralelo é criado
And o execution report registra evidência.
```

### Scenario 2 — Existing implementation is found

```text
Given existe implementação parcial ou completa no repo
When a execução audita o estado real
Then ela muda para REUSE_EXISTING ou HARDEN_EXISTING
And não recria arquitetura paralela
And documenta residual/future gaps.
```

### Scenario 3 — Missing dependency

```text
Given uma dependência runtime não existe no repo local
When a execução encontra essa ausência
Then ela não inventa uma solução massiva
And marca como MISSING_BUT_DEFER ou cria apenas adapter/validator mínimo se seguro
And registra risco residual.
```

### Scenario 4 — Save/load safety

```text
Given o fluxo envolve estado persistido direta ou indiretamente
When save/load ocorre após a ação
Then nenhum dado derivado de UI/debug substitui a fonte de verdade
And nenhum UnityEngine.Object é persistido
And schema change exige spec/migration separada.
```

### Scenario 5 — Final human validation deferred

```text
Given o fluxo exige interação visual, PlayMode ou gameplay integrado
When a spec é concluída tecnicamente
Then o report registra cenário final em vez de pedir validação humana imediata
And o status máximo respeita SPEC_VALIDATION_MATRIX_MASTER.md.
```

---

## 23D. Edge Cases and Failure Modes

A execução deve cobrir ou registrar risco residual para:

- Clima/lua/festival secreto revelado cedo.
- Day transition duplicado ou fora de ordem.
- Rain/Storm molhando estufa/interior indevidamente.
- World state não persistido ou restaurado em ordem errada.
- UI gerando estado em vez de consumir projection.
- Runtime depender de human test por spec.
- Mudança determinística sem EditMode test ou justificativa.
- Execução de WAVE 02 antes da 01Q sem exceção humana explícita.

---

## 23E. Minimum Execution Report Template

```md
# Execution Report — Rain Irrigation and Crop Integration

## Summary
- Spec:
- Wave: WAVE 02
- Branch:
- Executor:
- Date:
- Final status:

## Sources read
- ...

## Local audit
- Commands executed:
- Existing systems found:
- Existing partial systems found:
- Missing systems:
- Conflicts:

## Implementation decision
- REUSE_EXISTING / HARDEN_EXISTING / CREATE_MINIMAL / DEFER
- Justification:

## Files changed
- ...

## Functional evidence
- Happy path:
- Existing implementation handling:
- Missing dependency:
- Edge cases:
- Negative cases:

## Validation
- Docs validation:
- C# build:
- Unity compile:
- EditMode tests:
- PlayMode automated:
- Final human scenario:

## Testing Quality Gate
- Changed deterministic logic:
- Requires EditMode tests:
- Requires PlayMode automated or final human scenario:
- Requires regression test:
- Human validation timing:
- Minimum validation evidence for ACCEPTED:

## Residual risks
- ...

## Next specs impacted
- ...
```

---

## 23F. Stop Conditions

Parar a execução e registrar `BLOCKED` se ocorrer qualquer um destes casos:

```text
1. A implementação exigir alterar Packages/ ou ProjectSettings/.
2. A implementação exigir scene/prefab/asset wiring fora do escopo.
3. A implementação exigir mudança de save schema sem migration spec.
4. A implementação exigir reescrever sistema canônico existente.
5. A implementação criar conflito com 01Q, input focus, save ownership ou registry.
6. A implementação executar WAVE 02+ em massa antes da 01Q ou exceção humana explícita.
7. Não for possível decidir se sistema existente é canônico ou obsoleto.
```

## 25. Validações obrigatórias

    ```powershell
    .\tools\docs\validate_docs.ps1
    dotnet build .\Assembly-CSharp.csproj --no-restore
    dotnet build .\Assembly-CSharp-Editor.csproj --no-restore
    .\tools\unity\RunUnityCompileValidation.ps1 -ProjectPath "." -LogFile ".\Logs\unity-compile-validation.log"
    .\tools\unity\ScanUnityLogs.ps1 -LogFile ".\Logs\unity-compile-validation.log"
    ```

    EditMode tests required when deterministic adapter logic changes and harness is available.

    ## 26. Testing Quality Gate

    - Changed deterministic logic: YES if watering adapter/rules are created or changed.
    - Requires EditMode tests: YES for eligibility/idempotency when practical.
    - Requires PlayMode automated or final human scenario: YES for farm scene lifecycle validation; DEFERRED_TO_FINAL_VALIDATION.
    - Requires regression test: YES if fixing known rain/farm bug.
    - Human validation timing: DEFERRED_TO_FINAL_VALIDATION if scene scenario is needed.
    - Minimum validation evidence for ACCEPTED: docs validation PASS; C# builds PASS; Unity compile PASS; EditMode tests PASS or justified; final farm rain scenario documented if PlayMode not run.

    ## 27. Definition of Done

    ```text
    Rain/Storm integration audited or implemented.
    Greenhouse/covered tiles excluded.
    No weather generation duplicated.
    Idempotency documented/tested.
    Execution report created.
    SPEC_EXECUTION_ORDER unchanged.
    ```

    ## 28. Anti-regressão

    ```text
    Do not water covered/greenhouse tiles.
    Do not trigger magical irrigation.
    Do not change crop balance.
    Do not mark ACCEPTED with compile only.
    ```

    ## 29. Notas para execução posterior

    This spec feeds crop season/weather rules and farm world persistence.
