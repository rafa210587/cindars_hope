# SPEC — Calendar Festivals and Calendar Events Runtime

    > **Spec ID:** `02_spec_calendar_festivals_events_runtime`  
    > **Status:** A implementar  
    > **Wave:** WAVE 02 — Time / Calendar / Weather / Lunar  
    > **Priority:** P2  
    > **Type:** Runtime / Calendar / Events / Festival Contracts / Validation  
    > **Domain:** Calendar / Festivals / City Hooks / Quest Hooks / Economy Hooks  
    > **Parallelizable:** CONDITIONAL  
    > **Parallel group:** WAVE_02_WORLD_TIME_SERIALIZED  
    > **Can run with:** docs-only specs only.  
    > **Must not run with:** calendar runtime, quest condition/trigger, NPC schedule, shop stock/pricing, UI calendar, social/festival minigames, or any spec altering festival data/assets.  
    > **Repo lock scope:** `Assets/_Game/Scripts/World/**`, `Assets/_Game/Scripts/Calendar/**`, `Assets/_Game/Scripts/Events/**`, `Assets/_Game/Scripts/Save/**`, calendar/festival data assets, `docs/validation/02_spec_calendar_festivals_events_runtime_execution_report.md`.  
    > **Depends on:** WAVE 01, 01Q, `02_spec_time_clock_day_transition_runtime.md`, `02_spec_calendar_season_year_runtime.md`, `02_spec_weather_generation_forecast_runtime.md`, `02_spec_lunar_cycle_event_runtime.md`.  
    > **Blocks:** calendar UI day detail, NPC festival schedule overrides, shop festival overrides, quest temporal hooks, festival minigames future.  
    > **Scope:** create or harden the runtime contract for calendar events/festivals: scheduling, active state, known/hidden visibility, hooks, save state and downstream ownership boundaries.  
    > **Out of scope:** minigames, full NPC schedule implementation, shop pricing implementation, social/romance boosts, festival scenes/prefabs, UI final, Play Mode human execution.

    ---

    # /speckit.specify

    ## 1. Contexto

    The world direction states that festivals are calendar events, not just decoration. Festivals can alter NPC schedules, close normal shops, open temporary stalls, alter prices/demand, generate social events, add quest hooks, and should not break essential farm routine without warning.

    Festival examples include planting, harvest, food/tavern, music/Alihana, oath/Kanthor, rural/Thandra, market/Finan, secret/semi-hidden Nyx, and chaotic Senya events.

    This spec defines the runtime contract only. Downstream implementations remain in NPC/shop/quest/UI/social specs.

    ## 2. Problema

    Without a calendar event/festival runtime contract:

    ```text
    festivals may be hardcoded in NPC/shop/quest/UI;
    event visibility may reveal secrets too early;
    shops may reset stock when opening a festival menu;
    farm routine may be blocked without warning;
    quest hooks may depend on untracked dates;
    save/load may lose active festival state;
    annual repeat rules may drift.
    ```

    ## 3. Objetivo

    Provide a calendar event/festival runtime that exposes:

    ```text
    event id;
    season/day/start/end;
    known/hidden visibility;
    active state;
    repeat yearly flag;
    schedule override hooks;
    shop override hooks;
    quest hooks;
    reward hook placeholders;
    save-safe FestivalState/CalendarEventStates.
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
    docs/design/gameplay/quests/QUEST_OBJECTIVE_EVENT_SYSTEM_DIRECTION.md
docs/design/gameplay/quests/QUESTS_MAIN_LORE_DIRECTION.md
docs/design/gameplay/quests/QUESTS_MAIN_PROGRESSION_REFINEMENT_DIRECTION.md
    docs/design/gameplay/city/CITY_LAYOUT_BUILDINGS_SCHEDULE_DIRECTION.md
    docs/design/gameplay/city/CITY_NPC_ROSTER_SERVICES_DIRECTION_v1.1.md
    docs/design/gameplay/loot_crafting_economy/ECONOMY_PRICING_STOCK_REFRESH_DIRECTION.md
    docs/design/gameplay/ui_ux/UI_UX_FULL_GAMEPLAY_DIRECTION.md
    ```

    ## 5. Estado atual do repo

    Audit report marks time/calendar/weather/lunar as PARTIAL/UNKNOWN, city/NPC/dialogue/services as complete code with final Play Mode pending, and quest engine generic as PARTIAL/UNKNOWN.

    This means festival runtime must be contract-first and must not assume full quest/social/shop integrations exist.

    ## 6. User stories / engineering stories

    ```text
    As calendar, I need to mark events active on a season/day/time window.
    As NPC schedule, I need a festival override tag without owning calendar logic.
    As shop service, I need override hooks without restocking on menu open.
    As quest system, I need temporal event hooks that do not reveal hidden events.
    As player, I need warnings if festival affects shop/farm availability.
    ```

    ## 7. Escopo

    Includes:

    ```text
    audit existing calendar/event/festival systems;
    define CalendarEvent/Festival data contract;
    define active/known/hidden state;
    define repeat yearly behavior;
    define downstream hooks;
    define save state/defaults;
    create tests/validators/report when practical.
    ```

    ## 8. Fora de escopo

    Excludes:

    ```text
    festival minigames;
    festival scene/prefab setup;
    NPC schedules implementation;
    shop pricing/stock implementation;
    quest implementation;
    social/romance boosts;
    UI final;
    Play Mode human execution.
    ```

    ## 9. Regras de não duplicação

    ```text
    Do not implement festivals separately inside shops/NPC/quests.
    Do not reveal hidden/secret festivals until known.
    Do not reset shop stock on festival menu open.
    Do not block essential farm routine without warning.
    Do not make event text the source of save state.
    ```

    ## 10. Critérios de aceite

    ### 10.1 Calendar event contract

    Must define or consolidate fields equivalent to:

    ```text
    EventId;
    DisplayName;
    Season;
    Day;
    StartTime;
    EndTime;
    LocationId;
    NpcScheduleOverrideId;
    ShopOverrideRules;
    QuestHooks;
    RewardRules;
    KnownByDefault;
    RepeatEveryYear.
    ```

    ### 10.2 Festival state

    Runtime/save state must track:

    ```text
    active/inactive;
    known/hidden;
    completed/visited if applicable;
    yearly repeat;
    one-shot quest-driven override if applicable.
    ```

    ### 10.3 Downstream boundaries

    Must document:

    ```text
    NPC consumes schedule override tags;
    shops consume override rules but own stock/pricing;
    quest system consumes event hooks but owns quest progression;
    UI consumes safe display state only.
    ```

    ### 10.4 Tests/validation

    When practical:

    ```text
    event active on exact day/time;
    repeat yearly works;
    hidden event not displayed;
    known public event displayed;
    shop/NPC hooks are data only, not executed here.
    ```

    ### 10.5 Report

    Create:

    ```text
    docs/validation/02_spec_calendar_festivals_events_runtime_execution_report.md
    ```

    ---

    # /speckit.plan

    ## 11. Arquitetura alvo

    Potential target files:

    ```text
    Assets/_Game/Scripts/World/Calendar/CalendarEventDefinition.cs
    Assets/_Game/Scripts/World/Calendar/FestivalData.cs
    Assets/_Game/Scripts/World/Calendar/CalendarEventRuntimeState.cs
    Assets/_Game/Scripts/World/Calendar/CalendarEventService.cs
    Assets/_Game/Tests/EditMode/World/Calendar/CalendarEventTests.cs
    docs/validation/02_spec_calendar_festivals_events_runtime_execution_report.md
    ```

    Consolidate existing systems if present.

    ## 12. Contratos, dados e eventos

    ### 12.1 Data contracts

    ```text
    EventId is stable.
    Display text is derived.
    Hooks are references/tags, not direct runtime object references.
    ```

    ### 12.2 Runtime contracts

    ```text
    CalendarEventService owns active/known festival state.
    Downstream systems consume hooks/tags.
    Annual repeat is deterministic by season/day/year.
    ```

    ### 12.3 Event contracts

    ```text
    FestivalStartedEvent/FestivalEndedEvent optional notifications only.
    Quest/social/shop progression remains downstream-owned.
    ```

    ### 12.4 Save contracts

    ```text
    Persist FestivalState/CalendarEventStates when needed.
    Do not persist UI state/rendered text.
    Do not persist NPC runtime objects.
    ```

    ### 12.5 UI contracts

    ```text
    UI final separate.
    This spec provides safe public/known event data.
    ```

    ## 13. Sistemas afetados

    ```text
    Calendar runtime;
    save/load;
    NPC schedule future;
    shop override future;
    quest hooks future;
    UI calendar future.
    ```

    ## 14. Arquivos permitidos

    ```text
    Assets/_Game/Scripts/World/**
    Assets/_Game/Scripts/Calendar/**
    Assets/_Game/Scripts/Save/**
    Assets/_Game/Tests/EditMode/World/**
    Assets/_Game/Scripts/Editor/Validation/**
    docs/validation/02_spec_calendar_festivals_events_runtime_execution_report.md
    ```

    ## 15. Arquivos proibidos

    ```text
    Packages/**
    ProjectSettings/**
    Assets/**/*.unity
    Assets/**/*.prefab
    Assets/**/*.asset unless data asset only and explicitly reported
    docs/specs/SPEC_EXECUTION_ORDER.md
    docs/specs/implementados/**
    docs/refinements/implementados/**
    docs/project/CURRENT_STATE.md
    PROJECT_LOG.md
    ```

    ## 16. Estratégia de implementação

    ### Fase 0 — Auditoria local obrigatória

    ```bash
    rg -n "Festival|CalendarEvent|EventId|Season|Day|RepeatEveryYear|KnownByDefault|NpcScheduleOverride|ShopOverride|QuestHooks" Assets/_Game/Scripts Assets/_Game/Data docs
    ```

    ### Fase 1 — Contract

    ```text
    1. Identify calendar service.
    2. Define/consolidate event/festival data.
    3. Define active/known/hidden runtime state.
    4. Define downstream hook contracts.
    ```

    ### Fase 2 — Validation

    ```text
    1. Add deterministic calendar event tests when practical.
    2. Run validations.
    3. Create report.
    ```

    ## 17. Ordem segura de execução

    ```text
    calendar runtime -> lunar/weather optional inputs -> festival event runtime -> NPC/shop/quest/UI adapters
    ```

    ## 18. Paralelização

    - Parallelizable: CONDITIONAL for generation only; execution serialized with calendar/quest/shop/NPC specs.

    ## 19. Impacto em save/load

    ```text
    Does this change save schema? CONDITIONAL.
    Does this add save section? CONDITIONAL: CalendarEventStates/FestivalState if absent.
    Requires migration? YES if schema changes.
    Persist Unity references? NO.
    ```

    ## 20. Impacto em eventos

    ```text
    Adds events: CONDITIONAL.
    Changes existing events: SHOULD BE NO.
    Requires unsubscribe pattern: only if subscribers are added.
    ```

    ## 21. Impacto em UI/Unity

    ```text
    Changes UI: NO
    Changes scenes: NO
    Changes prefabs: NO
    Requires PlayMode final validation: NO unless scene-bound festival flow is touched.
    Human validation timing: NOT REQUIRED unless scene flow is touched.
    ```

    ## 22. Riscos técnicos

    ```text
    Risco: festival logic leaks into shops/NPC/quests.
    Mitigação: calendar emits hooks/tags only.

    Risco: hidden event leaks.
    Mitigação: known/hidden visibility layer.

    Risco: annual repeat breaks save.
    Mitigação: deterministic season/day/year tests.
    ```

    ## 23. Rollback

    ```text
    Remove calendar event service/data additions/tests/report.
    Revert save fields if added.
    ```

    ---

    # /speckit.tasks

    ## 24. Tasks

    - [ ] T001 — Read sources and confirm branch.
    - [ ] T002 — Audit calendar/event/festival code.
    - [ ] T003 — Define/consolidate CalendarEvent/Festival data contract.
    - [ ] T004 — Define active/known/hidden runtime state.
    - [ ] T005 — Define downstream hooks/tags.
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

    ## 26. Testing Quality Gate

    - Changed deterministic logic: YES if event activation/visibility logic is created or changed.
    - Requires EditMode tests: YES for calendar event activation/visibility when practical.
    - Requires PlayMode automated or final human scenario: NO unless scene festival flow is touched.
    - Requires regression test: YES if fixing known festival/calendar bug.
    - Human validation timing: NOT REQUIRED unless scene flow touched; then DEFERRED_TO_FINAL_VALIDATION.
    - Minimum validation evidence for ACCEPTED: docs validation PASS; C# builds PASS; Unity compile PASS if C# changed; EditMode tests PASS or justified; execution report created; hidden events not exposed.

    ## 27. Definition of Done

    ```text
    Calendar event/festival contract created/consolidated.
    Active/known/hidden state defined.
    Downstream hooks documented.
    Tests/validators added or residual risk documented.
    Execution report created.
    ```

    ## 28. Anti-regressão

    ```text
    Do not implement minigames.
    Do not reset shop stock on menu open.
    Do not reveal hidden festivals.
    Do not block farm routine without warning.
    Do not mark ACCEPTED with compile only.
    ```

    ## 29. Notas para execução posterior

    Feeds NPC schedules, shop modifiers, quest hooks and calendar UI specs.
