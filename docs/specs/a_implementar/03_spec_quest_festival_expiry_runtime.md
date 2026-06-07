# SPEC — Quest Festival Expiry Runtime

> **Spec ID:** `03_spec_quest_festival_expiry_runtime`  
> **Status:** A implementar  
> **Wave:** WAVE 03 — Quest / Objective / Event System  
> **Priority:** P1  
> **Type:** Runtime / Quest / Calendar Adapter / Expiry  
> **Domain:** Quests / Festival / Expiry / Calendar Events  
> **Parallelizable:** NO  
> **Parallel group:** WAVE_03_QUEST_LOCKED  
> **Can run with:** N/A  
> **Must not run with:** qualquer spec que altere calendar event runtime, festival state, QuestState save/load, quest visibility, rewards ou calendar UI.  
> **Repo lock scope:** `Assets/_Game/Scripts/Quests/**`, `Assets/_Game/Scripts/World/**`, `Assets/_Game/Scripts/UI/**`, `docs/validation/03_spec_quest_festival_expiry_runtime_execution_report.md`  
> **Depends on:**  
  - `docs/design/SPEC_SOURCE_MAP.md`
  - `docs/design/SPECIFICATION_PROCESS.md`
  - `docs/design/lore/VAALARA_GAME_CANON_DIRECTION_v1.0.md`
  - `docs/specs/SPEC_IMPLEMENTABLE_TEMPLATE.md`
  - `docs/specs/SPEC_WAVE_EXECUTION_PROTOCOL.md`
  - `docs/specs/SPEC_VALIDATION_MATRIX_MASTER.md`
  - `docs/specs/SPEC_EXISTING_IMPLEMENTATION_AUDIT.md`
  - `docs/project/CURRENT_STATE.md`
  - `docs/design/gameplay/quests/QUEST_OBJECTIVE_EVENT_SYSTEM_DIRECTION.md`
  - `docs/design/gameplay/world/SEASONS_CALENDAR_WEATHER_LUNAR_DIRECTION.md`
  - `docs/design/gameplay/ui_ux/UI_UX_FULL_GAMEPLAY_DIRECTION.md`
  - `docs/design/gameplay/ui_ux/UI_UX_MENU_SCREEN_FLOWS_DIRECTION.md`
  - `docs/specs/a_implementar/02_spec_calendar_festivals_events_runtime.md`
  - `docs/specs/a_implementar/03_spec_quest_condition_trigger_runtime.md`
  - `docs/specs/a_implementar/03_spec_quest_state_save_load_runtime.md`
> **Blocks:**  
  - festival quest runtime;
  - calendar/festival UI projection;
  - farm order festival integration;
  - quest log visibility for timed quests.
> **Scope:** definir e implementar a integração entre Festival quests e calendário, incluindo expiry, visibility, save/load e anti-softlock.  
> **Out of scope:** minigames de festival, conteúdo final de festivais, schedules de NPC completos, balance econômico de evento, UI final de festival.

---

# /speckit.specify

## 1. Contexto

O direction de Quest define `Festival` como categoria de quest que pode expirar no fim do festival, deve comunicar data/horário quando conhecida e não deve impedir rotina agrícola essencial sem aviso.

O direction de World/Calendar define festival como evento de calendário, com estado conhecido/oculto, repeat yearly, hooks para NPC/shop/quest/UI e sem minigames nesta fase.

Esta spec fecha o adapter entre quest e festival calendar state.

---

## 2. Problema

Sem contrato de expiry de festival:

```text
quest pode expirar sem feedback;
festival pode acabar sem atualizar QuestState;
Quest Log pode revelar evento secreto cedo;
reward pode aplicar após festival encerrado;
save/load durante festival pode duplicar ou perder estado;
quest pode bloquear main quest por data perdida.
```

---

## 3. Objetivo

Ao final:

```text
Festival quests usam QuestCategory Festival;
expiry depende de FestivalStarted/FestivalEnded/calendar state;
QuestState guarda StartedAt/ExpiresAt/State;
Quest Log mostra data/horário apenas quando conhecido;
save/load preserva active/waiting/expired/completed;
main quest não depende de evento raro sem fallback.
```

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

- docs/design/gameplay/quests/QUEST_OBJECTIVE_EVENT_SYSTEM_DIRECTION.md
- docs/design/gameplay/world/SEASONS_CALENDAR_WEATHER_LUNAR_DIRECTION.md
- docs/design/gameplay/ui_ux/UI_UX_FULL_GAMEPLAY_DIRECTION.md
- docs/design/gameplay/ui_ux/UI_UX_MENU_SCREEN_FLOWS_DIRECTION.md

### Required interpretation

```text
Esta spec é derivada dos directions/refinements canônicos.
Ela não substitui os directions.
Ela transforma parte do refinement em contrato implementável, com escopo, locks, validações e quality gate.
```

---

## Direction / Refinement Coverage

### Covered from directions

- `Festival` como QuestCategory.
- Festival quests podem expirar no fim do festival.
- Festival quest deve mostrar data/horário quando conhecida.
- AttendFestival, FestivalStarted e FestivalEnded como triggers/conditions.
- Calendar UI mostra festivais e eventos conhecidos sem revelar segredos.
- Farm routine não deve ser bloqueada sem aviso.

### Deferred / future from directions

- Minigames de festival.
- Competition ranking completo.
- NPC social boost e relationship future.
- Festival shop final.
- Festival cutscenes/timelines.

### Explicitly not redefined here

- Calendar runtime base.
- Weather/lunar generation.
- QuestState base.
- Quest Log final visual layout.

## 4. Estado atual do repo

```text
Festival runtime foi especificado na WAVE 02.
QuestState/conditions/triggers foram especificados na WAVE 03.
Festival quests ainda precisam adapter de expiry.
```

Estado local a auditar:

```text
CalendarEventState;
FestivalState;
events de FestivalStarted/FestivalEnded;
QuestState expiry fields;
Quest Log visibility/projection;
save/load de calendar/festival.
```

---

## 5. User stories

```text
Como jogador, quero saber quando uma festival quest conhecida expira.
Como sistema de calendário, quero disparar início/fim de festival.
Como quest system, quero marcar Active/Waiting/Expired/Completed conforme festival state.
Como save/load, quero preservar festival quest no meio do evento.
Como UI, quero ocultar festival secreto até discovery.
```

---

## 6. Escopo

```text
Festival quest category integration;
expiry rules;
calendar condition/trigger adapter;
QuestState fields usage;
save/load behavior;
visibility policy;
debug validation.
```

---

## 7. Fora de escopo

```text
Minigames;
festival NPC schedules finais;
festival shop balance;
festival art/UI final;
relationship/social event chains;
new calendar system.
```

---

## 8. Regras de não duplicação

```text
Não criar FestivalQuestState fora de QuestState.
Não duplicar CalendarEventState.
Não usar QuestFlag para substituir expiry formal.
Não revelar Hidden festival quest no Quest Log.
Não aplicar reward depois de Expired.
```

---

## 9. Critérios de aceite

- Festival quest usa QuestState.
- Expiry usa calendário/festival events.
- QuestState vira Expired quando festival termina sem completion, se aplicável.
- Main quest nunca fica impossível por festival perdido sem fallback.
- Save/load durante festival preserva estado.
- Quest Log mostra data/horário somente se conhecido.

---

# /speckit.plan

## 10. Arquitetura alvo

```text
Assets/_Game/Scripts/Quests/Adapters/FestivalQuestAdapter.cs
Assets/_Game/Scripts/Quests/Conditions/FestivalQuestConditions.cs
Assets/_Game/Scripts/Quests/Triggers/FestivalQuestTriggers.cs
Assets/_Game/Scripts/Quests/Validation/FestivalQuestValidator.cs
Assets/_Game/Tests/EditMode/Quests/FestivalQuestExpiryTests.cs
```

---

## 11. Contratos

### Data

```text
QuestState.State supports Waiting/Active/Expired/Completed.
StartedAtDay/Time and ExpiresAtDay are used when known.
FestivalId is stable ID.
```

### Runtime

```text
Calendar publishes festival lifecycle events.
Quest consumes them.
Quest does not own calendar state.
```

### Save

```text
Uses QuestStateSection and CalendarEventStates.
No new section.
```

### UI

```text
Calendar/Quest Log shows only known festival quest info.
```

---

## 12. Arquivos permitidos

```text
Assets/_Game/Scripts/Quests/**
Assets/_Game/Tests/EditMode/Quests/**
Assets/_Game/Scripts/Editor/Validation/**
docs/validation/03_spec_quest_festival_expiry_runtime_execution_report.md
```

Leitura permitida:

```text
Assets/_Game/Scripts/World/**
Assets/_Game/Scripts/UI/**
```

---

## 13. Arquivos proibidos

```text
Packages/**
ProjectSettings/**
Assets/**/*.unity
Assets/**/*.prefab
Assets/**/*.asset
docs/specs/SPEC_EXECUTION_ORDER.md
docs/specs/implementados/**
docs/refinements/implementados/**
docs/project/CURRENT_STATE.md
PROJECT_LOG.md
```

---

## 14. Estratégia de implementação

```text
1. Auditar calendar/festival runtime.
2. Auditar quest state expiry fields.
3. Implementar adapter/conditions/triggers mínimos.
4. Garantir save/load e visibility.
5. Criar tests para active->expired/completed/save-load.
6. Criar report.
```

---

## 15. Ordem segura

```text
Calendar events -> Quest conditions/triggers -> QuestState save/load -> Festival adapter.
```

---

## 16. Paralelização

- Parallelizable: NO
- Must not run with:
  - calendar festival runtime;
  - quest state save/load;
  - quest visibility projection;
  - rewards idempotency.
- Reason:
  - Expiry toca calendar + quest state + UI visibility.

---

## 17. Impacto em save/load

```text
Does this change save schema? NO, salvo se QuestState não tiver ExpiresAt/StartedAt.
Does this add a save section? NO.
Does this require migration? NO unless shape changes.
Does this persist Unity references? NO.
```

---

## 18. Impacto em eventos

```text
Adds events: CONDITIONAL, se FestivalStarted/FestivalEnded não existirem.
Changes existing events: SHOULD BE NO.
Requires unsubscribe pattern: YES if subscribers are created.
```

---

## 19. Impacto em UI/Unity

```text
Changes UI: NO final; projection hooks only if existing.
Changes scenes: NO.
Changes prefabs: NO.
Requires PlayMode/final human scenario: DEFERRED_TO_FINAL_VALIDATION for end-to-end festival flow.
```

---

## 20. Riscos

```text
Risco: festival expirar quest sem jogador entender.
Mitigação: known date/time in Quest Log.

Risco: hidden festival revelar spoiler.
Mitigação: VisibilityPolicy.

Risco: main quest softlock.
Mitigação: no main quest expiry by calendar.
```

---

## 21. Rollback

```text
Remover adapter/conditions/triggers/tests/report.
Não há rollback de schema se não alterado.
```

---

# /speckit.tasks

## 22. Tasks

- [ ] T001 — Ler fontes e confirmar branch.
- [ ] T002 — Auditar calendar/festival events.
- [ ] T003 — Auditar QuestState expiry.
- [ ] T004 — Implementar adapter mínimo.
- [ ] T005 — Implementar tests/validator quando praticável.
- [ ] T006 — Rodar validações.
- [ ] T007 — Criar execution report.

## 25. Validações obrigatórias

Docs:

```powershell
.\tools\docs\validate_docs.ps1
```

Busca local mínima:

```bash
rg -n "Quest|Objective|Condition|Trigger|Reward|Flag|FarmOrder|Festival|Bestiary|Fonte|MainProgression" Assets/_Game/Scripts docs/design docs/specs
```

C# runtime/editor quando houver alteração C#:

```powershell
dotnet build .\Assembly-CSharp.csproj --no-restore
dotnet build .\Assembly-CSharp-Editor.csproj --no-restore
```

Unity compile quando houver alteração Unity C#:

```powershell
.\tools\unity\RunUnityCompileValidation.ps1 -ProjectPath "." -LogFile ".\Logs\unity-compile-validation.log"
.\tools\unity\ScanUnityLogs.ps1 -LogFile ".\Logs\unity-compile-validation.log"
```

EditMode tests quando lógica determinística for criada/alterada:

```text
Unity Test Runner — EditMode, ou comando local equivalente disponível no repo.
```

PlayMode/final human validation:

```text
Não pedir validação humana por spec.
Quando houver cenário integrado, registrar em execution report e vincular a docs/validation/FINAL_HUMAN_VALIDATION_BY_WAVE.md.
```

---

## 26. Testing Quality Gate

- Changed deterministic logic: YES, se expiry adapter/conditions/triggers forem implementados.
- Requires EditMode tests: YES para expiry state transitions e save/load determinístico.
- Requires PlayMode automated or final human scenario: YES, cenário final deferido para FestivalStarted->Quest update->FestivalEnded.
- Requires regression test: YES se corrigir bug de expiry/reward duplicado; caso contrário NO.
- Human validation timing: DEFERRED_TO_FINAL_VALIDATION se houver cenário integrado; caso contrário NOT REQUIRED.
- Minimum validation evidence for ACCEPTED: docs validation PASS; C# build PASS se C# mudou; EditMode expiry tests PASS ou NOT RUN justificado; report criado; hidden visibility preservada.

---

## 27. Definition of Done

```text
Spec executada sem alterar arquivos proibidos.
Contratos/data/runtime implementados apenas dentro do escopo.
Execution report criado em docs/validation/03_spec_quest_festival_expiry_runtime_execution_report.md.
Fonte/direction coverage preservado.
Validações obrigatórias PASS ou NOT RUN com motivo, impacto e mitigação.
Sem promoção indevida para ACCEPTED apenas por compile.
```

---

## 28. Anti-regressão

```text
Não duplicar sistemas canônicos existentes.
Não quebrar save/load.
Não usar UI como fonte de verdade.
Não revelar spoilers antes de discovery/visibility policy.
Não pedir human test por spec.
Não executar runtime WAVE 03 em massa antes da 01Q ou exceção humana explícita.
Não alterar SPEC_EXECUTION_ORDER.md.
```

---

## 29. Notas para execução posterior

Esta spec deve ser executada por Claude Code/Codex com auditoria local do repo, usando `rg` e validando o estado real dos sistemas já implementados.

Se a auditoria local revelar sistema equivalente já existente, a execução deve mudar para hardening/residual, não recriar do zero.
