# SPEC — Rain Irrigation and Crop Integration

    > **Spec ID:** `02_spec_rain_irrigation_crop_integration`  
    > **Status:** A implementar  
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
    docs/specs/SPEC_IMPLEMENTABLE_TEMPLATE.md
docs/specs/SPEC_WAVE_EXECUTION_PROTOCOL.md
docs/specs/SPEC_VALIDATION_MATRIX_MASTER.md
docs/specs/SPEC_EXISTING_IMPLEMENTATION_AUDIT.md
docs/specs/SPEC_GENERATION_ROADMAP_MASTER.md
docs/specs/SPEC_REGISTRY_TO_IMPLEMENT.md
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
    docs/specs/a_implementar/02_spec_weather_generation_forecast_runtime.md
    docs/specs/a_implementar/02_spec_time_clock_day_transition_runtime.md
    docs/specs/a_implementar/02_spec_calendar_season_year_runtime.md
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
    docs/specs/SPEC_EXECUTION_ORDER.md
    docs/specs/implementados/**
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
