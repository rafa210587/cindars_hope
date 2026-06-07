# SPEC — Weather Generation and Forecast Runtime

    > **Spec ID:** `02_spec_weather_generation_forecast_runtime`  
    > **Status:** A implementar  
    > **Wave:** WAVE 02 — Time / Calendar / Weather / Lunar  
    > **Priority:** P1  
    > **Type:** Runtime / World / Weather / Forecast / Validation  
    > **Domain:** World Time / Weather / Forecast / Calendar Integration  
    > **Parallelizable:** CONDITIONAL  
    > **Parallel group:** WAVE_02_WORLD_TIME_SERIALIZED  
    > **Can run with:** docs-only specs or non-overlapping UI specs after WAVE 01 and 01Q; not with other weather/calendar/lunar runtime specs unless a single owner serializes changes.  
    > **Must not run with:** `02_spec_time_clock_day_transition_runtime.md`, `02_spec_calendar_season_year_runtime.md`, `02_spec_rain_irrigation_crop_integration.md`, `02_spec_lunar_cycle_event_runtime.md`, `02_spec_time_calendar_weather_lunar_save_state.md`, or any spec editing day transition/save/time contracts.  
    > **Repo lock scope:** `Assets/_Game/Scripts/World/**`, `Assets/_Game/Scripts/Time/**`, `Assets/_Game/Scripts/Weather/**`, `Assets/_Game/Scripts/Save/**`, weather/calendar data assets, `docs/validation/02_spec_weather_generation_forecast_runtime_execution_report.md`.  
    > **Depends on:** WAVE 01 specs, `01Q`, `02_spec_time_clock_day_transition_runtime.md`, `02_spec_calendar_season_year_runtime.md`.  
    > **Blocks:** rain irrigation integration, weather save state, calendar UI forecast display, NPC weather schedules, shop/weather modifiers, cave/weather modifiers.  
    > **Scope:** implement or harden deterministic weather generation, tomorrow forecast, weather state transitions, visibility rules, and extension hooks without implementing every weather reaction.  
    > **Out of scope:** crop watering, NPC schedules, cave modifiers, shop pricing changes, weather VFX/SFX polish, festivals, lunar events, and final Play Mode human validation.

    ---

    # /speckit.specify

    ## 1. Contexto

    The world direction defines initial weather types: `Sunny`, `Cloudy`, `Rain`, `Storm`, `Fog`, `Wind`, `Heat`, `Cold`, and `Snow`, with a recommended baseline of `Sunny`, `Cloudy`, `Rain`, `Storm`, `Snow`, and `Fog`.

    The same direction defines forecast discovery through public calendar/board, agriculture NPC, sky observation, future service, or future upgrade. Baseline is showing tomorrow's forecast after the player interacts with a public calendar/board.

    This spec turns that direction into a runtime contract for weather generation and forecast, but it must not implement all downstream reactions. It creates the source of truth that farm, NPC, shop, quest, UI and cave specs consume later.

    ## 2. Problema

    Without a canonical weather generator and forecast contract:

    ```text
    weather can change inconsistently across save/load;
    Rain/Storm may water crops before farm integration owns it;
    calendar may reveal secret events too early;
    NPC/shop/farm/cave systems may implement their own weather rules;
    forecast can become rendered text instead of stable state;
    random generation can break deterministic tests;
    weather weights can drift by season;
    save/load can restore today without tomorrow forecast or seed.
    ```

    ## 3. Objetivo

    Create or harden a weather runtime service that provides:

    ```text
    current weather;
    tomorrow forecast;
    deterministic generation by day/season/year/seed;
    allowed weather by season;
    public forecast visibility rules;
    weather transition API for day transition;
    extension tags for downstream systems;
    safe save/load inputs for weather state.
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
    ```

    Conditional downstream sources:

    ```text
    docs/design/gameplay/farm/FARM_DESIGN_DIRECTION_v1.3.md
docs/design/gameplay/loot_crafting_economy/LOOT_CRAFTING_ECONOMY_DIRECTION.md
docs/design/gameplay/loot_crafting_economy/ECONOMY_PRICING_STOCK_REFRESH_DIRECTION.md
    docs/design/gameplay/quests/QUEST_OBJECTIVE_EVENT_SYSTEM_DIRECTION.md
docs/design/gameplay/quests/QUESTS_MAIN_LORE_DIRECTION.md
docs/design/gameplay/quests/QUESTS_MAIN_PROGRESSION_REFINEMENT_DIRECTION.md
    docs/design/gameplay/city/CITY_LAYOUT_BUILDINGS_SCHEDULE_DIRECTION.md
    docs/design/gameplay/bestiary/BESTIARY_KNOWLEDGE_DISCOVERY_DIRECTION.md
    ```

    ## 5. Estado atual do repo

    Documented audit status:

    ```text
    Time/calendar/weather/lunar = PARTIAL / UNKNOWN.
    Hunger/stamina/status/time exists at MVP level.
    Calendar/weather/lunar complete runtime is not proven by registries/status.
    Claude Code must audit GameTime, calendar, weather, lunar and day transition locally before implementation.
    ```

    Execution must not assume weather systems are absent or complete. It must audit local code first.

    ## 6. User stories / engineering stories

    ```text
    As day transition, I need to generate tomorrow weather deterministically.
    As calendar UI, I need a safe forecast value without secret event leakage.
    As farm integration, I need to know whether current weather can water external tiles.
    As NPC scheduling, I need weather tags without owning weather generation.
    As save/load, I need weather state restored by stable IDs/values, not rendered text.
    ```

    ## 7. Escopo

    Includes:

    ```text
    audit existing time/weather/calendar runtime;
    define WeatherId/WeatherType baseline;
    define WeatherState current/tomorrow/seed fields;
    define weather generation rules by season;
    define forecast visibility without secrets;
    define weather day-transition API;
    define deterministic random strategy;
    create validation/test/report when practical.
    ```

    ## 8. Fora de escopo

    Excludes:

    ```text
    rain irrigation tile mutation;
    crop growth/death;
    NPC schedule implementation;
    shop price/stock modifiers;
    cave spawn modifiers;
    weather VFX/SFX final polish;
    festival event runtime;
    lunar effects;
    Play Mode human execution.
    ```

    ## 9. Regras de não duplicação

    ```text
    Do not create second time/day service if one exists.
    Do not create separate weather randomizer inside farm/NPC/shop/cave.
    Do not persist forecast rendered text.
    Do not expose hidden lunar/quest events through forecast.
    Do not let Rain integration directly define weather generation.
    Do not make weather generation depend on current scene.
    ```

    ## 10. Critérios de aceite

    ### 10.1 Weather catalog

    Must define or audit baseline weather IDs:

    ```text
    Sunny
    Cloudy
    Rain
    Storm
    Snow
    Fog
    ```

    Future/conditional weather IDs may be registered as future/extensible:

    ```text
    Wind
    Heat
    Cold
    ```

    ### 10.2 Deterministic generation

    Weather generation must be reproducible from stable inputs:

    ```text
    current day;
    season;
    year;
    WeatherSeed;
    generation version/ruleset.
    ```

    ### 10.3 Forecast visibility

    Forecast must expose only allowed data:

    ```text
    TomorrowWeather can be public after calendar/board interaction.
    Secret quest/lunar event details are not exposed by weather forecast.
    Forecast text is derived UI, not saved state.
    ```

    ### 10.4 Extension hooks

    The implementation/report must identify downstream fields/tags:

    ```text
    CanWaterExternalCrops;
    NpcScheduleTag;
    FarmModifierTags;
    CaveModifierTags;
    ShopModifierTags;
    IsSevereWeather;
    BlocksOutdoorComfort;
    ```

    ### 10.5 Execution report

    Create:

    ```text
    docs/validation/02_spec_weather_generation_forecast_runtime_execution_report.md
    ```

    Report must include audit results, weather IDs, generation algorithm, forecast visibility, save impacts, tests/validators and residual risks.

    ---

    # /speckit.plan

    ## 11. Arquitetura alvo

    Prefer existing systems if found locally. Potential target files:

    ```text
    Assets/_Game/Scripts/World/Weather/WeatherService.cs
    Assets/_Game/Scripts/World/Weather/WeatherState.cs
    Assets/_Game/Scripts/World/Weather/WeatherType.cs
    Assets/_Game/Scripts/World/Weather/WeatherGenerationRules.cs
    Assets/_Game/Scripts/World/Weather/WeatherForecastService.cs
    Assets/_Game/Tests/EditMode/World/Weather/WeatherGenerationTests.cs
    docs/validation/02_spec_weather_generation_forecast_runtime_execution_report.md
    ```

    If equivalent files already exist, consolidate them instead of creating new parallel systems.

    ## 12. Contratos, dados e eventos

    ### 12.1 Data contracts

    ```text
    WeatherId/WeatherType is stable and save-safe.
    WeatherState stores current weather, tomorrow weather and generation seed/version.
    WeatherDataSO may define display/config but save persists IDs/values only.
    ```

    ### 12.2 Runtime contracts

    ```text
    WeatherService owns generation and current/tomorrow weather state.
    Day transition requests weather roll-forward once per day.
    UI consumes forecast view model, not generator internals.
    ```

    ### 12.3 Event contracts

    ```text
    WeatherChangedEvent may exist only as notification after primary state changes.
    ForecastUpdatedEvent may exist only as UI/integration signal.
    Events are not source of save state.
    ```

    ### 12.4 Save contracts

    ```text
    Does this change save schema? CONDITIONAL.
    If WeatherState fields do not exist, add only with explicit defaults and migration/default policy.
    Persist CurrentWeather, TomorrowWeather, WeatherSeed and generation version if required.
    Do not persist VFX/current visual state or rendered forecast text.
    ```

    ### 12.5 UI contracts

    ```text
    Forecast visibility is data-level; final UI is separate.
    Calendar UI must not reveal secret events or exact hidden conditions.
    ```

    ## 13. Sistemas afetados

    ```text
    World time/calendar;
    day transition;
    save/load;
    farm integration future;
    NPC schedule future;
    shop modifiers future;
    cave modifiers future;
    UI calendar future.
    ```

    ## 14. Arquivos permitidos

    ```text
    Assets/_Game/Scripts/World/**
    Assets/_Game/Scripts/Time/**
    Assets/_Game/Scripts/Weather/**
    Assets/_Game/Scripts/Save/**
    Assets/_Game/Tests/EditMode/World/**
    Assets/_Game/Scripts/Editor/Validation/**
    docs/validation/02_spec_weather_generation_forecast_runtime_execution_report.md
    ```

    ## 15. Arquivos proibidos

    ```text
    Packages/**
    ProjectSettings/**
    Assets/**/*.unity
    Assets/**/*.prefab
    Assets/**/*.asset, unless creating explicit data asset only if safe and reported
    docs/specs/SPEC_EXECUTION_ORDER.md
    docs/specs/implementados/**
    docs/refinements/implementados/**
    docs/project/CURRENT_STATE.md
    PROJECT_LOG.md
    ```

    ## 16. Estratégia de implementação

    ### Fase 0 — Auditoria local obrigatória

    ```bash
    rg -n "Weather|Forecast|TomorrowWeather|CurrentWeather|WeatherSeed|Season|DayTransition|GameTime|Calendar" Assets/_Game/Scripts
    rg -n "WeatherDataSO|WeatherTable|WeatherGeneration|WeatherForecast" Assets/_Game/Scripts Assets/_Game/Data
    ```

    ### Fase 1 — Contract

    ```text
    1. Identify existing time/day/calendar owner.
    2. Identify whether weather state already exists.
    3. Define or consolidate WeatherState and generator.
    4. Define forecast visibility rules.
    5. Ensure future downstream tags exist or are documented.
    ```

    ### Fase 2 — Tests/validation

    ```text
    1. Add deterministic generation tests if logic changed.
    2. Add default/missing weather normalization test if save touched.
    3. Add report.
    ```

    ## 17. Ordem segura de execução

    ```text
    1. Read sources.
    2. Audit existing systems.
    3. Consolidate weather owner.
    4. Add minimal generator/forecast if missing.
    5. Add tests/validators.
    6. Run validations.
    7. Create report.
    ```

    ## 18. Paralelização

    - Parallelizable: CONDITIONAL for generation only; execution NO against other world-time specs.
    - Shared lock: weather/time/calendar/save.
    - Reason: weather state is consumed by multiple systems and save.

    ## 19. Impacto em save/load

    ```text
    Does this change save schema? CONDITIONAL.
    Does this add a save section? Prefer NO; add fields to existing time/world section if already established.
    Requires migration? YES if existing save schema changes.
    Persists Unity references? MUST BE NO.
    ```

    ## 20. Impacto em eventos

    ```text
    Adds events: CONDITIONAL.
    Changes existing events: SHOULD BE NO.
    Requires unsubscribe pattern: only if runtime subscribers are added.
    ```

    ## 21. Impacto em UI/Unity

    ```text
    Changes UI: NO
    Changes scenes: NO
    Changes prefabs: NO
    Requires Play Mode final validation: NO for pure generation; YES if scene/weather lifecycle visual/runtime is touched, then DEFERRED_TO_FINAL_VALIDATION.
    Human validation timing: NOT REQUIRED unless scene lifecycle behavior is touched.
    ```

    ## 22. Riscos técnicos

    ```text
    Risco: weather generator conflicts with existing GameTime/Calendar.
    Mitigação: audit first, consolidate owner.

    Risco: forecast reveals secret quest/lunar info.
    Mitigação: forecast exposes weather only, hidden event visibility remains elsewhere.

    Risco: non-deterministic random breaks tests/save.
    Mitigação: seed and version generation rules.
    ```

    ## 23. Rollback

    ```text
    Remove weather generator/service additions.
    Revert save fields/migration if any.
    Remove tests/validators.
    Remove execution report.
    ```

    ---

    # /speckit.tasks

    ## 24. Tasks

    - [ ] T001 — Read required sources and confirm branch.
    - [ ] T002 — Audit existing time/weather/calendar systems.
    - [ ] T003 — Define/consolidate WeatherState and weather IDs.
    - [ ] T004 — Implement/harden deterministic weather generation if needed.
    - [ ] T005 — Implement/harden tomorrow forecast visibility if needed.
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

    Run EditMode tests if generation/default logic is added.

    ## 26. Testing Quality Gate

    - Changed deterministic logic: YES if weather generation/forecast/default logic is created or changed.
    - Requires EditMode tests: YES if deterministic generation/default logic is changed and harness is available.
    - Requires PlayMode automated or final human scenario: NO unless scene/weather lifecycle integration is touched; then DEFERRED_TO_FINAL_VALIDATION.
    - Requires regression test: YES if fixing known weather/save/forecast bug.
    - Human validation timing: NOT REQUIRED unless scene lifecycle behavior is touched.
    - Minimum validation evidence for ACCEPTED: docs validation PASS; C# builds PASS; Unity compile PASS if C# changed; deterministic EditMode tests PASS or NOT RUN with justified risk; execution report created; no hidden events leaked through forecast.

    ## 27. Definition of Done

    ```text
    Weather owner audited.
    Weather IDs and state contract defined/consolidated.
    Deterministic generation/forecast implemented or residual risk documented.
    Save impact documented.
    Tests/validators added or NOT RUN justified.
    Execution report created.
    SPEC_EXECUTION_ORDER.md unchanged.
    ```

    ## 28. Anti-regressão

    ```text
    Do not create parallel weather systems.
    Do not reveal secret lunar/quest conditions through forecast.
    Do not persist rendered forecast text.
    Do not water crops in this spec.
    Do not mark ACCEPTED with compile only.
    ```

    ## 29. Notas para execução posterior

    This spec feeds:

    ```text
    02_spec_rain_irrigation_crop_integration.md
    02_spec_time_calendar_weather_lunar_save_state.md
    02_spec_calendar_ui_weather_lunar_display.md
    NPC/shop/cave weather modifiers future specs.
    ```
