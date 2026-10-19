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

# SPEC — Lunar Cycle Event Runtime

    > **Spec ID:** `02_spec_lunar_cycle_event_runtime`  
    > **Status:** A implementar  
> **Revision:** EXPANDED_01_05_CORRECTED  
    > **Wave:** WAVE 02 — Time / Calendar / Weather / Lunar  
    > **Priority:** P1  
    > **Type:** Runtime / World / Lunar / Events / Validation  
    > **Domain:** World Time / Lunar Events / Vaalara Moons / Discovery  
    > **Parallelizable:** CONDITIONAL  
    > **Parallel group:** WAVE_02_WORLD_TIME_SERIALIZED  
    > **Can run with:** docs-only specs only; not runtime world/time specs.  
    > **Must not run with:** calendar/year runtime, day transition, weather generation, calendar/festival runtime, time/weather/lunar save state, quest temporal condition specs, Fonte/Mana specs.  
    > **Repo lock scope:** `Assets/_Game/Scripts/World/**`, `Assets/_Game/Scripts/Time/**`, `Assets/_Game/Scripts/Lunar/**`, `Assets/_Game/Scripts/Save/**`, lunar data assets, `docs/validation/02_spec_lunar_cycle_event_runtime_execution_report.md`.  
    > **Depends on:** WAVE 01, 01Q, `02_spec_time_clock_day_transition_runtime.md`, `02_spec_calendar_season_year_runtime.md`.  
    > **Blocks:** lunar save state, calendar UI lunar display, quest temporal conditions, Fonte lunar reactions, Mana rules, shop/night market, cave lunar modifiers.  
    > **Scope:** implement or harden the baseline lunar event model for Alihana, Senya and Nyx, including known/unknown visibility, deterministic schedule and extension hooks.  
    > **Out of scope:** full astronomy simulation, rare triple-alignment gameplay, Fonte rituals, Mana blooming, cave modifiers, shop stock changes, NPC dialogue implementation, Play Mode human execution.

    ---

    # /speckit.specify

    ## 1. Contexto

    The world direction states that the three moons of Vaalara are gameplay systems, not decoration. They can affect farm, city, cave, economy, Fonte, Água Viva, Mana, crops, quests, pets, rumors, night shop, magic, crafting and rare events.

    The recommended baseline avoids full orbital simulation and uses lunar events:

    ```text
    every 7 days: minor lunar event;
    every 14 days: medium lunar event;
    every 28 days: major seasonal/lunar event;
    day 7 Alihana minor;
    day 14 Senya minor/medium;
    day 21 Nyx minor/medium;
    day 28 seasonal/lunar major.
    ```

    This spec implements/hardens that baseline only.

    ## 2. Problema

    Without a canonical lunar event runtime:

    ```text
    different systems can compute lunar state differently;
    calendar can reveal hidden effects too early;
    quests may depend on rare events without hints;
    Fonte/Mana/cave/shop systems may hardcode moon logic;
    save/load may lose active lunar event;
    event overlap can be too complex too early;
    specs may implement astronomy simulation before needed.
    ```

    ## 3. Objetivo

    Provide a stable lunar event runtime that exposes:

    ```text
    current lunar event;
    moon identity: Alihana/Senya/Nyx/SeasonalMajor/FutureAlignment;
    event strength: Minor/Medium/Major;
    known/unknown visibility;
    deterministic event schedule by day/season/year;
    extension tags for farm/city/cave/Fonte/Mana/shop/quest;
    save-safe state fields.
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
    docs/design/gameplay/quests/QUEST_OBJECTIVE_EVENT_SYSTEM_DIRECTION.md
docs/design/gameplay/quests/QUESTS_MAIN_LORE_DIRECTION.md
docs/design/gameplay/quests/QUESTS_MAIN_PROGRESSION_REFINEMENT_DIRECTION.md
    ```

    Conditional sources:

    ```text
    docs/design/gameplay/bestiary/BESTIARY_KNOWLEDGE_DISCOVERY_DIRECTION.md
    docs/design/gameplay/loot_crafting_economy/ECONOMY_PRICING_STOCK_REFRESH_DIRECTION.md
    ```

    ## 5. Estado atual do repo

    Documented audit status:

    ```text
    Time/calendar/weather/lunar = PARTIAL / UNKNOWN.
    Complete lunar runtime is not proven.
    Claude Code must audit GameTime/calendar/lunar locally before implementation.
    ```

    ## 6. User stories / engineering stories

    ```text
    As calendar, I need to know which lunar events are known to the player.
    As quest system, I need stable conditions: LunarEvent, MoonKnown and FestivalActive.
    As Fonte/Mana future systems, I need extension tags without implementing reactions here.
    As save/load, I need to persist current/known lunar event state.
    As player, I should receive hints before mandatory lunar requirements.
    ```

    ## 7. Escopo

    Includes:

    ```text
    audit existing lunar/calendar/time code;
    define LunarEventId/MoonId/strength;
    implement deterministic baseline schedule if missing;
    define discovery/known rules;
    define extension tags;
    define save contract inputs;
    create tests/validators and report when practical.
    ```

    ## 8. Fora de escopo

    Excludes:

    ```text
    full orbital simulation;
    rare triple-alignment implementation beyond reserved IDs/hooks;
    Fonte reaction implementation;
    Mana blooming;
    night shop inventory;
    crop mutation;
    NPC dialogue;
    cave modifiers;
    UI final.
    ```

    ## 9. Regras de não duplicação

    ```text
    Do not implement moon logic independently inside quests/farm/cave/shop.
    Do not reveal hidden lunar effects automatically.
    Do not make mandatory quest depend on rare event without hint/control.
    Do not implement full astronomy.
    Do not make one moon alone cause Mana bloom.
    ```

    ## 10. Critérios de aceite

    ### 10.1 Baseline schedule

    Must support:

    ```text
    Day 7: Alihana minor;
    Day 14: Senya minor/medium;
    Day 21: Nyx minor/medium;
    Day 28: seasonal/lunar major;
    future rare alignments reserved but not common.
    ```

    ### 10.2 Visibility/discovery

    Must distinguish:

    ```text
    active lunar event;
    known lunar event;
    public display text;
    hidden effect tags.
    ```

    Calendar must not reveal hidden effects before discovery.

    ### 10.3 Extension tags

    Must define/document tags for:

    ```text
    FarmModifierTags;
    CityModifierTags;
    CaveModifierTags;
    FonteReactionTags;
    ManaModifierTags;
    ShopModifierTags;
    QuestConditionTags;
    Pet/Companion future tags.
    ```

    ### 10.4 Tests

    When practical:

    ```text
    day 7/14/21/28 map correctly;
    unknown event effects are not revealed;
    known event becomes displayable after discovery;
    save/load restores active/known events.
    ```

    ### 10.5 Report

    Create:

    ```text
    docs/validation/02_spec_lunar_cycle_event_runtime_execution_report.md
    ```

    ---

    # /speckit.plan

    ## 11. Arquitetura alvo

    Potential target files:

    ```text
    Assets/_Game/Scripts/World/Lunar/LunarCycleService.cs
    Assets/_Game/Scripts/World/Lunar/LunarEventState.cs
    Assets/_Game/Scripts/World/Lunar/LunarEventId.cs
    Assets/_Game/Scripts/World/Lunar/LunarVisibilityService.cs
    Assets/_Game/Tests/EditMode/World/Lunar/LunarCycleTests.cs
    docs/validation/02_spec_lunar_cycle_event_runtime_execution_report.md
    ```

    If equivalents exist, consolidate instead of creating parallels.

    ## 12. Contratos, dados e eventos

    ### 12.1 Data contracts

    ```text
    LunarEventId and MoonId are stable IDs.
    KnownLunarEvents is save state, not UI state.
    Lunar event tags are extension metadata, not implemented effects.
    ```

    ### 12.2 Runtime contracts

    ```text
    LunarCycleService derives active event from calendar/day.
    Effects are consumed by downstream systems later.
    Discovery/known state gates UI visibility.
    ```

    ### 12.3 Event contracts

    ```text
    LunarEventChangedEvent optional notification after state change.
    LunarEventDiscoveredEvent optional notification after discovery.
    Events are not source of save state.
    ```

    ### 12.4 Save contracts

    ```text
    Persist ActiveLunarEvent, KnownLunarEvents, cycle version if needed.
    Do not persist rendered UI text.
    Do not persist hidden effects as discovered.
    ```

    ### 12.5 UI contracts

    ```text
    UI display is future/other spec.
    This spec only provides safe known/unknown view data.
    ```

    ## 13. Sistemas afetados

    ```text
    Time/calendar;
    save/load;
    quest temporal conditions;
    future Fonte/Mana;
    future shop/cave/farm modifiers;
    future calendar UI.
    ```

    ## 14. Arquivos permitidos

    ```text
    Assets/_Game/Scripts/World/**
    Assets/_Game/Scripts/Time/**
    Assets/_Game/Scripts/Lunar/**
    Assets/_Game/Scripts/Save/**
    Assets/_Game/Tests/EditMode/World/**
    Assets/_Game/Scripts/Editor/Validation/**
    docs/validation/02_spec_lunar_cycle_event_runtime_execution_report.md
    ```

    ## 15. Arquivos proibidos

    ```text
    Packages/**
    ProjectSettings/**
    Assets/**/*.unity
    Assets/**/*.prefab
    Assets/**/*.asset unless data asset only and explicitly reported
    .specs/SPEC_EXECUTION_ORDER.md
    .specs/implementados/**
    docs/refinements/implementados/**
    docs/project/CURRENT_STATE.md
    PROJECT_LOG.md
    ```

    ## 16. Estratégia de implementação

    ### Fase 0 — Auditoria local obrigatória

    ```bash
    rg -n "Lunar|Moon|Alihana|Senya|Nyx|KnownLunar|ActiveLunar|Calendar|DayOfSeason" Assets/_Game/Scripts Assets/_Game/Data
    ```

    ### Fase 1 — Contract

    ```text
    1. Identify calendar/day owner.
    2. Define or consolidate lunar event IDs.
    3. Implement/harden baseline schedule.
    4. Implement/harden visibility/discovery state.
    ```

    ### Fase 2 — Validation

    ```text
    1. Add schedule/visibility tests when practical.
    2. Run validations.
    3. Create report.
    ```

    ## 17. Ordem segura de execução

    ```text
    calendar runtime -> lunar cycle -> lunar save state -> UI/quests/Fonte downstream
    ```

    ## 18. Paralelização

    - Parallelizable: CONDITIONAL for generation only; execution must serialize against calendar/weather/save specs.

    ## 19. Impacto em save/load

    ```text
    Does this change save schema? CONDITIONAL.
    Add fields only with defaults/migration policy.
    Persist Unity references? NO.
    ```

    ## 20. Impacto em eventos

    ```text
    Adds events: CONDITIONAL.
    Changes existing events: SHOULD BE NO.
    Requires unsubscribe pattern: if subscribers added.
    ```

    ## 21. Impacto em UI/Unity

    ```text
    Changes UI: NO
    Changes scenes: NO
    Changes prefabs: NO
    Requires PlayMode final validation: NO unless scene-bound effects are touched.
    Human validation timing: NOT REQUIRED.
    ```

    ## 22. Riscos técnicos

    ```text
    Risco: revealing lore/hidden moon effects too early.
    Mitigação: separate active vs known vs display state.

    Risco: implementing too much astronomy.
    Mitigação: use 7/14/21/28 baseline only.
    ```

    ## 23. Rollback

    ```text
    Remove lunar service/state additions/tests/report.
    Revert save additions if any.
    ```

    ---

    # /speckit.tasks

    ## 24. Tasks

    - [ ] T001 — Read sources and confirm branch.
    - [ ] T002 — Audit lunar/calendar code.
    - [ ] T003 — Define/consolidate LunarEventId/MoonId.
    - [ ] T004 — Implement/harden 7/14/21/28 schedule.
    - [ ] T005 — Implement/harden known/unknown visibility state.
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
| Report | Execution report foi criado? | `docs/validation/02_spec_lunar_cycle_event_runtime_execution_report.md`. | PARTIAL |

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
# Execution Report — Lunar Cycle Event Runtime

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

    - Changed deterministic logic: YES if lunar schedule/visibility logic is created or changed.
    - Requires EditMode tests: YES for schedule/visibility when practical.
    - Requires PlayMode automated or final human scenario: NO unless scene-bound lunar effects are touched.
    - Requires regression test: YES if fixing known lunar/calendar bug.
    - Human validation timing: NOT REQUIRED unless scene effects are touched.
    - Minimum validation evidence for ACCEPTED: docs validation PASS; C# builds PASS; Unity compile PASS; EditMode tests PASS or justified; execution report created; hidden lunar effects not revealed.

    ## 27. Definition of Done

    ```text
    Lunar baseline schedule implemented/consolidated.
    Visibility/discovery contract defined.
    Extension tags documented.
    Save impact documented.
    Execution report created.
    ```

    ## 28. Anti-regressão

    ```text
    Do not implement full astronomy.
    Do not reveal hidden effects.
    Do not make Mana bloom from one moon alone.
    Do not block main quest on rare event without hint/control.
    ```

    ## 29. Notas para execução posterior

    Feeds calendar UI, quest temporal conditions, Fonte/Mana future reactions, shop/cave/farm lunar modifiers.
