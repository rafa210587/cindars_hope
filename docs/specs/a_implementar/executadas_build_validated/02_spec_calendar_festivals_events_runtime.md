## Source Map Compliance

### Global sources read

- docs/design/SPEC_SOURCE_MAP.md
- docs/design/SPECIFICATION_PROCESS.md
- docs/design/lore/VAALARA_GAME_CANON_DIRECTION_v1.0.md
- docs/specs/SPEC_IMPLEMENTABLE_TEMPLATE.md
- docs/specs/SPEC_WAVE_EXECUTION_PROTOCOL.md
- docs/specs/SPEC_VALIDATION_MATRIX_MASTER.md
- docs/specs/SPEC_EXISTING_IMPLEMENTATION_AUDIT.md
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

# SPEC — Calendar Festivals and Calendar Events Runtime

    > **Spec ID:** `02_spec_calendar_festivals_events_runtime`  
    > **Status:** A implementar  
> **Revision:** EXPANDED_01_05_CORRECTED  
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
| Report | Execution report foi criado? | `docs/validation/02_spec_calendar_festivals_events_runtime_execution_report.md`. | PARTIAL |

---

## 23B. Local Audit Checklist

Antes de alterar qualquer arquivo, Claude Code/Codex deve rodar e registrar no execution report:

```bash
rg -n "Time|Clock|Calendar|Season|Weather|Rain|Irrigation|Lunar|Festival|DayTransition|Save" Assets/_Game/Scripts docs/design docs/specs
rg -n "TODO|FIXME|HACK|PARTIAL|DEFERRED|BUILD_VALIDATED|ACCEPTED" docs/specs docs/validation docs/IMPLEMENTATION_STATUS.md docs/project/CURRENT_STATE.md
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
# Execution Report — Calendar Festivals and Calendar Events Runtime

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
