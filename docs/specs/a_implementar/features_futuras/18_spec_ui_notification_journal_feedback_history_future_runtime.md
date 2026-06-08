# SPEC — UI Notification Journal Feedback History Future Runtime

> **Spec ID:** `18_spec_ui_notification_journal_feedback_history_future_runtime`  
> **Status:** A implementar / Future mapped  
> **Wave:** WAVE 18 — UI Shell / Accessibility / Localization Future  
> **Priority:** P1  
> **Type:** Runtime / Future / Notifications / Feedback Journal  
> **Domain:** Notifications / Journal / History / Priority / Non-blocking Feedback  
> **Parallelizable:** YES  
> **Parallel group:** WAVE_18_UI_SHELL_ACCESSIBILITY_LOCALIZATION_FUTURE  
> **Can run with:** localization/text-key convention if no same files.  
> **Must not run with:** qualquer spec que altere HUD core, quest state, social state, pet runtime, companion AI, save schema migration, UI prefab/layout or audio assets.  
> **Repo lock scope:** `Assets/_Game/Scripts/UI/Notifications/**`, `Assets/_Game/Scripts/UI/Journal/**`, `Assets/_Game/Tests/EditMode/UI/**`, `docs/validation/18_spec_ui_notification_journal_feedback_history_future_runtime_execution_report.md`  
> **Depends on:**  
  - `docs/design/SPEC_SOURCE_MAP.md`
  - `docs/design/SPECIFICATION_PROCESS.md`
  - `docs/design/lore/VAALARA_GAME_CANON_DIRECTION_v1.0.md`
  - `docs/specs/SPEC_IMPLEMENTABLE_TEMPLATE.md`
  - `docs/specs/SPEC_WAVE_EXECUTION_PROTOCOL.md`
  - `docs/specs/SPEC_VALIDATION_MATRIX_MASTER.md`
  - `docs/specs/SPEC_EXISTING_IMPLEMENTATION_AUDIT.md`
  - `docs/project/CURRENT_STATE.md`
  - `docs/design/gameplay/ui_ux/UI_UX_FULL_GAMEPLAY_DIRECTION.md`
  - `docs/design/gameplay/ui_ux/UI_UX_MENU_SCREEN_FLOWS_DIRECTION.md`
  - `docs/design/gameplay/social/SOCIAL_RELATIONSHIP_ROMANCE_DIRECTION.md`
  - `docs/design/gameplay/quests/QUEST_OBJECTIVE_EVENT_SYSTEM_DIRECTION.md`
  - `docs/design/gameplay/quests/QUESTS_MAIN_LORE_DIRECTION.md`
  - `docs/design/gameplay/quests/QUESTS_MAIN_PROGRESSION_REFINEMENT_DIRECTION.md`
  - `docs/design/gameplay/world/SEASONS_CALENDAR_WEATHER_LUNAR_DIRECTION.md`
  - `docs/design/gameplay/save_load/SAVE_LOAD_FULL_STATE_DIRECTION.md`
  - `docs/design/gameplay/cave/CAVE_DESIGN_DIRECTION.md`
  - `docs/design/gameplay/cave/CAVE_MONSTER_ROSTER_DIRECTION.md`
  - `docs/design/gameplay/combat/COMBAT_CORE_DIRECTION.md`
  - `docs/design/gameplay/combat/STATUS_EFFECTS_DIRECTION.md`
  - `docs/design/gameplay/farm/FARM_DESIGN_DIRECTION_v1.3.md`
  - `docs/design/gameplay/farm/FARM_LAYOUT_SCALE_BUILDINGS_DIRECTION.md`
  - `docs/design/gameplay/city/CITY_DESIGN_DIRECTION_v1.2.md`
  - `docs/design/gameplay/city/CITY_NPC_ROSTER_SERVICES_DIRECTION_v1.1.md`
  - `docs/design/gameplay/city/CITY_LAYOUT_BUILDINGS_SCHEDULE_DIRECTION.md`
  - `docs/design/gameplay/loot_crafting_economy/LOOT_CRAFTING_ECONOMY_DIRECTION.md`
  - `docs/design/gameplay/loot_crafting_economy/ECONOMY_PRICING_STOCK_REFRESH_DIRECTION.md`
  - `docs/design/gameplay/magic/MAGIC_SPELLS_ACTIONS_DIRECTION.md`
  - `docs/design/gameplay/magic/MAGIC_LEARNING_UNLOCKS_SOURCES_DIRECTION.md`
  - `docs/design/gameplay/companions/COMPANIONS_DIRECTION.md`
  - `docs/design/gameplay/pets/PETS_DIRECTION.md`
  - `docs/design/gameplay/bestiary/BESTIARY_KNOWLEDGE_DISCOVERY_DIRECTION.md`
> **Blocks:**  
  - HUD notification queue;
  - quest/social/farm/cave/Fonte events;
  - accessibility settings;
  - localization text keys.
> **Scope:** definir/endurecer journal/history futuro de notificações e feedback: prioridade, dedupe, non-blocking, critical modal handoff, history filters and spoiler-safe records.  
> **Out of scope:** HUD core rewrite, notification prefab animation, audio assets, quest/social/farm state mutation, save migration.

---

# /speckit.specify

## 1. Contexto

Notificações incluem loot recebido, item insuficiente, quest atualizada/concluída, recipe/spell aprendida, skill point recebido, relationship change, pet/companion state, shop restock, farm event, Fonte reagiu e fragmento recuperado. Notificação não deve bloquear input salvo confirmação importante, nem empilhar a ponto de cobrir gameplay.

Esta spec cria journal/history futuro, sem reescrever HUD queue já gerada.

---

## 2. Problema

Sem journal/history:

```text
notificação importante some para sempre;
notificação empilha e cobre gameplay;
quest/social/evento secreto fica em log antes da hora;
critical notification bloqueia input sem modal apropriado;
mensagens duplicadas poluem histórico;
debug notification aparece no journal final;
relationship/pet/companion future aparece sem runtime;
histórico vira fonte de verdade de quest.
```

---

## 3. Objetivo

Criar/endurecer:

```text
NotificationHistoryRecord;
NotificationJournalViewModel;
NotificationHistoryCategory;
NotificationDeduplicationPolicy;
NotificationRetentionPolicy;
CriticalNotificationHandoffPolicy;
NotificationSpoilerFilter;
NotificationAccessibilityAdapter.
```

---

## 4. Regras de design

```text
Notification ativa não deve bloquear input salvo confirmação importante.
Journal é histórico/projection, não fonte de truth.
Critical decision passa para modal/confirmation.
DebugOnly nunca entra no final journal.
Spoiler-safe records.
Dedupe/merge para spam.
History cap/retention para save seguro.
```

---

## 5. User stories / engineering stories

```text
Como jogador, quero revisar mensagens recentes importantes.
Como UI, quero evitar spam e dedupe.
Como quest/social/Fonte, quero log seguro sem spoiler.
Como accessibility, quero feedback persistente para quem perdeu popup.
Como save/load, quero histórico limitado e persistível por IDs/text keys.
```

---

## 6. Escopo

Inclui:

```text
history records;
journal projection;
categories/filters;
dedupe/retention;
critical modal handoff;
spoiler filter;
accessibility adapter;
tests.
```

Não inclui:

```text
HUD queue rewrite;
popup animations;
audio assets;
domain event mutation;
save migration.
```

## 7. Modelo de domínio

### 7.1 NotificationHistoryCategory

```text
Loot
InsufficientItem
QuestUpdated
QuestCompleted
RecipeLearned
SpellLearned
SkillPoint
RelationshipChangeFuture
PetCompanionStateFuture
ShopRestock
FarmEvent
FonteReaction
FragmentRecovered
System
DebugOnly
```

### 7.2 NotificationHistoryRecord

```text
RecordId
NotificationId
Category
TextKey
IconKey optional
TimestampGameDay
TimestampGameTime optional
Priority
WasCritical
WasModalHandoff
SpoilerTier
SourceDomain
SourceId optional
DeduplicationKey
CanShowInJournal
DebugOnly false
```

### 7.3 NotificationJournalViewModel

```text
VisibleRecords[]
Filters[]
UnreadImportantCount
LatestCriticalSummary
CanClearNonCritical
CanPinRecordFuture
DebugWarnings[]
```

### 7.4 NotificationDeduplicationPolicy

```text
DeduplicationKey
MergeWithinSeconds
MergeWithinGameMinutes
MaxVisibleStackCount
IncrementCounter
PreserveHighestPriority
```

### 7.5 NotificationRetentionPolicy

```text
MaxRecords
MaxDays
KeepCritical
KeepPinnedFuture
DropDebug
DropExpiredSpoilered
```

---

## 8. History rules

```text
Low/normal notifications can be logged if useful.
Spam notifications merge by key.
Critical notifications hand off to modal and record handoff.
DebugOnly never final.
Journal filters by category/source/priority.
Journal cannot unlock quest/state by itself.
```

---

## 9. Spoiler rules

```text
Hidden quest:
  do not log exact title before discovery.

Fonte/Anya:
  log stage-safe message only.

Final choice:
  no journal entry before gate.

Pet/companion future:
  log only if runtime exists; otherwise hidden/deferred.

Relationship future:
  avoid romantic icon/text for blocked NPC.
```

---

## 10. Criteria

```text
Notification history contracts exist.
Dedupe/retention policies exist.
Critical handoff policy exists.
Journal projection spoiler-safe.
DebugOnly excluded.
Tests cover duplicate merge, retention cap, critical modal handoff, hidden quest, Fonte safe text, debug excluded, pet/companion future hidden and journal not source of truth.
```

# /speckit.plan

## 11. Arquitetura alvo

```text
Assets/_Game/Scripts/UI/Notifications/NotificationHistoryCategory.cs
Assets/_Game/Scripts/UI/Notifications/NotificationHistoryRecord.cs
Assets/_Game/Scripts/UI/Journal/NotificationJournalViewModel.cs
Assets/_Game/Scripts/UI/Notifications/NotificationDeduplicationPolicy.cs
Assets/_Game/Scripts/UI/Notifications/NotificationRetentionPolicy.cs
Assets/_Game/Scripts/UI/Notifications/CriticalNotificationHandoffPolicy.cs
Assets/_Game/Scripts/UI/Notifications/NotificationSpoilerFilter.cs
Assets/_Game/Scripts/UI/Notifications/NotificationAccessibilityAdapter.cs
Assets/_Game/Tests/EditMode/UI/NotificationJournalFeedbackHistoryTests.cs
```

Consolidar existentes se houver.

## 12. Arquivos permitidos

```text
Assets/_Game/Scripts/UI/Notifications/**
Assets/_Game/Scripts/UI/Journal/**
Assets/_Game/Scripts/UI/Accessibility/**
Assets/_Game/Scripts/Quests/**
Assets/_Game/Scripts/Fonte/**
Assets/_Game/Scripts/Editor/Validation/**
Assets/_Game/Tests/EditMode/UI/**
docs/validation/18_spec_ui_notification_journal_feedback_history_future_runtime_execution_report.md
```

## 13. Arquivos proibidos

```text
Packages/**
ProjectSettings/**
Assets/**/*.unity
Assets/**/*.prefab
Assets/**/*.asset
Assets/_Game/Scripts/Pets/**
docs/specs/SPEC_EXECUTION_ORDER.md
docs/specs/implementados/**
docs/refinements/implementados/**
docs/project/CURRENT_STATE.md
PROJECT_LOG.md
```

# /speckit.tasks

## 14. Tasks

- [ ] T001 — Ler fontes.
- [ ] T002 — Auditar notification/journal systems.
- [ ] T003 — Consolidar history/journal/dedupe contracts.
- [ ] T004 — Implementar filters/validators.
- [ ] T005 — Criar tests.
- [ ] T006 — Rodar validações.
- [ ] T007 — Criar report.

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

- docs/design/gameplay/ui_ux/UI_UX_FULL_GAMEPLAY_DIRECTION.md
- docs/design/gameplay/ui_ux/UI_UX_MENU_SCREEN_FLOWS_DIRECTION.md
- docs/design/gameplay/social/SOCIAL_RELATIONSHIP_ROMANCE_DIRECTION.md
- docs/design/gameplay/quests/QUEST_OBJECTIVE_EVENT_SYSTEM_DIRECTION.md
- docs/design/gameplay/quests/QUESTS_MAIN_LORE_DIRECTION.md
- docs/design/gameplay/quests/QUESTS_MAIN_PROGRESSION_REFINEMENT_DIRECTION.md
- docs/design/gameplay/world/SEASONS_CALENDAR_WEATHER_LUNAR_DIRECTION.md
- docs/design/gameplay/save_load/SAVE_LOAD_FULL_STATE_DIRECTION.md
- docs/design/gameplay/cave/CAVE_DESIGN_DIRECTION.md
- docs/design/gameplay/cave/CAVE_MONSTER_ROSTER_DIRECTION.md
- docs/design/gameplay/combat/COMBAT_CORE_DIRECTION.md
- docs/design/gameplay/combat/STATUS_EFFECTS_DIRECTION.md
- docs/design/gameplay/farm/FARM_DESIGN_DIRECTION_v1.3.md
- docs/design/gameplay/farm/FARM_LAYOUT_SCALE_BUILDINGS_DIRECTION.md
- docs/design/gameplay/city/CITY_DESIGN_DIRECTION_v1.2.md
- docs/design/gameplay/city/CITY_NPC_ROSTER_SERVICES_DIRECTION_v1.1.md
- docs/design/gameplay/city/CITY_LAYOUT_BUILDINGS_SCHEDULE_DIRECTION.md
- docs/design/gameplay/loot_crafting_economy/LOOT_CRAFTING_ECONOMY_DIRECTION.md
- docs/design/gameplay/loot_crafting_economy/ECONOMY_PRICING_STOCK_REFRESH_DIRECTION.md
- docs/design/gameplay/magic/MAGIC_SPELLS_ACTIONS_DIRECTION.md
- docs/design/gameplay/magic/MAGIC_LEARNING_UNLOCKS_SOURCES_DIRECTION.md
- docs/design/gameplay/companions/COMPANIONS_DIRECTION.md
- docs/design/gameplay/pets/PETS_DIRECTION.md
- docs/design/gameplay/bestiary/BESTIARY_KNOWLEDGE_DISCOVERY_DIRECTION.md

### Required interpretation

```text
Esta spec é future/mapped.
Ela deriva principalmente de UI_UX_FULL_GAMEPLAY_DIRECTION.
Ela não substitui as specs já geradas de UI input focus, HUD/hotbar, inventory/equipment/tooltips ou shop/crafting/skilltree/quest/fonte projections.
Ela não implementa prefabs, cenas, assets, fontes finais, arte final, input package settings ou localization package.
Ela deve reforçar guardrails: UI modal bloqueia gameplay input, HUD final não depende de debug, UI não é fonte de verdade, e nenhum spoiler aparece cedo.
Quando houver conflito, documentos de sistema vencem para significado mecânico; UI_UX vence para apresentação, foco, prioridade visual, navegação e feedback.
```

---

## Direction / Refinement Coverage

### Covered from directions

- Notification types incluem loot recebido, item insuficiente, quest atualizada, quest concluída, recipe aprendida, spell aprendida, skill point recebido, relationship change, pet/companion state, shop restock, farm event, Fonte reagiu e fragmento recuperado.
- Notificação não deve bloquear input, salvo confirmação importante.
- Notificação não deve empilhar a ponto de cobrir gameplay.
- Notificação crítica pode pausar em modal se for decisão irreversível.
- Debug HUD é separado de HUD final e debug não deve virar comunicação final.
- Acessibilidade precisa de feedback visual + sonoro para eventos críticos.

### Deferred / future from directions

- Popup animation.
- Audio assets.
- HUD queue rewrite.
- Save migration.
- Pet/companion runtime.
- Final journal visual layout.

### Explicitly not redefined here

- Notification queue core from batch 22.
- QuestState.
- SocialRelationshipState.
- FonteAnyaSection.
- Save schema.
- Debug HUD.

---

## 23A. Execution Readiness Matrix

| Área | Pergunta obrigatória | Evidência esperada | Status se faltar |
|---|---|---|---|
| Fonte canônica | Executor leu UI/UX, menu flows e directions dos domínios afetados? | Lista no execution report. | PARTIAL |
| Estado real do repo | Sistemas existentes relacionados a notification journal/feedback history foram auditados antes de criar novos? | Comandos `rg` e achados. | PARTIAL |
| Não duplicação | Existe sistema equivalente já implementado/parcial? | REUSE/HARDEN/CREATE/DEFER. | BLOCKED se duplicar |
| UI não é fonte | UI não vira fonte de QuestState, SaveState, Inventory, Social, Fonte, Ending ou Economy. | Checklist/tests. | PARTIAL |
| Input/foco | Modal não permite WASD/attack/interact atrás da UI. | Checklist/tests ou dependência explícita. | PARTIAL |
| Acessibilidade | Não depender só de cor; texto legível; confirma destrutivo; reduzir flashes/volume preparados. | Checklist/tests. | PARTIAL |
| Anti-spoiler | UI/text/log não vaza Anya, Fonte, 101, final, quest oculta, eventos secretos ou NPC bloqueado. | Tests/checklist. | PARTIAL |
| Save/load | Houve alteração de schema? | Declaração NO ou STOP se migration necessária. | BLOCKED se alterar sem migration |
| Scene/assets | Não tocou scenes/prefabs/assets/fontes/ProjectSettings/Packages. | `Forbidden files touched: NO`. | BLOCKED se violar |
| Testes | Lógica determinística nova tem EditMode tests quando praticável? | Test list. | PARTIAL |
| Report | Execution report criado? | `docs/validation/18_spec_ui_notification_journal_feedback_history_future_runtime_execution_report.md`. | PARTIAL |

---

## 23B. Local Audit Checklist

Antes de alterar qualquer arquivo, Claude Code/Codex deve rodar e registrar no execution report:

```bash
rg -n "NotificationHistory|NotificationJournal|NotificationDeduplication|CriticalNotification|FonteReaction|FragmentRecovered|DebugOnly|Journal" Assets/_Game/Scripts docs/design docs/specs
rg -n "UIFocus|SystemMenu|Options|Settings|Accessibility|Localization|TextKey|IconKey|Notification|Journal|DialogueChoice|Confirmation|Gamepad|Keyboard|Mouse|DebugHUD|Breath|Folego|FinalChoice|Fonte|Level101|SecretEvent|Pet|Romance" Assets/_Game/Scripts docs/design docs/specs
rg -n "TODO|FIXME|HACK|PARTIAL|DEFERRED|BUILD_VALIDATED|ACCEPTED" docs/specs docs/validation docs/IMPLEMENTATION_STATUS.md docs/project/CURRENT_STATE.md
```

Classificar achados:

```text
EXISTING_CANONICAL
EXISTING_PARTIAL
MISSING_SAFE_TO_CREATE
MISSING_BUT_DEFER
CONFLICT
```

---

## 23C. Functional Acceptance Scenarios

### Scenario 1 — Happy path

```text
Given sistema UI/runtime base existe
When o fluxo desta spec projeta estado, texto, opção, notificação ou escolha
Then UI apresenta informação autorizada, clara, compacta e navegável
And não muta estado de gameplay diretamente
And não depende de debug HUD
And não vaza spoiler.
```

### Scenario 2 — Existing partial implementation

```text
Given já existe implementação parcial no repo
When a execução audita o sistema
Then ela escolhe HARDEN_EXISTING em vez de recriar do zero
And registra divergências do direction
And altera apenas o menor conjunto seguro de arquivos.
```

### Scenario 3 — Accessibility/readability

```text
Given estado crítico, decisão destrutiva, aviso de erro, feedback de combate, opção de diálogo ou notificação
When a UI apresenta esse estado
Then o jogador não depende apenas de cor
And texto/ícone/feedback sonoro ou visual alternativo existe como contrato
And confirmação é exigida quando aplicável.
```

### Scenario 4 — Anti-spoiler / protected route

```text
Given segredo de Anya/Fonte/101/final, quest oculta, evento secreto, NPC bloqueado ou relationship future
When UI/text/log/notification consulta dados
Then detalhes ficam ocultos, genéricos ou gated
And nenhum estado futuro aparece como já disponível.
```

### Scenario 5 — Final human validation deferred

```text
Given o fluxo exige inspeção visual/gameplay de UI shell, options, dialogue, notification, journal, localization ou accessibility
When a implementação técnica terminar
Then o report registra cenário final em docs/validation/FINAL_HUMAN_VALIDATION_BY_WAVE.md
And não pede validação humana imediata por spec.
```

---

## 23D. Edge Cases and Failure Modes

A execução deve cobrir ou registrar risco residual para:

- Journal becomes source of quest truth.
- Duplicate notifications flood history.
- Critical notification blocks input without modal.
- Debug notification appears in final journal.
- Hidden quest title logged early.
- Fonte/Anya spoiler in record.
- Pet/companion future notification shown without runtime.
- Retention saves too much data.

---

## 23E. Minimum Execution Report Template

```md
# Execution Report — UI Notification Journal Feedback History Future Runtime

## Summary
- Spec:
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

## UI/UX compliance
- UI is not source of truth:
- Modal/input focus safe:
- Accessibility/readability:
- Keyboard/mouse navigation:
- Gamepad future prepared:
- Debug HUD separation:
- No Breath/Folego/BR:
- Anti-spoiler:
- No scene/prefab/assets:
- Save/load safe:

## Implementation decision
- REUSE_EXISTING / HARDEN_EXISTING / CREATE_MINIMAL / DEFER
- Justification:

## Files changed
- ...

## Functional evidence
- Happy path:
- Existing partial:
- Accessibility:
- Anti-spoiler:
- Save/load:
- UI/final scenario:

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
2. A implementação exigir scene/prefab/tilemap/asset/font file changes.
3. A implementação alterar save schema sem migration spec.
4. A implementação reimplementar UI input focus, HUD, inventory/equipment/tooltips ou shop/crafting/skilltree projections já geradas.
5. A implementação tornar UI fonte de verdade de gameplay state.
6. A implementação permitir gameplay input atrás de modal.
7. A implementação depender de debug HUD para comunicar regra ao jogador.
8. A implementação mostrar Breath/Fôlego/BR em HUD final.
9. A implementação vazar Anya/Fonte/101/final/quest/evento secreto cedo.
10. A implementação criar runtime pet, romance/social deep ou companion AI fora de seus blocos.
11. Não for possível decidir se sistema existente é canônico ou obsoleto.
```



---

## 25. Validações obrigatórias

Docs:

```powershell
.\tools\docs\validate_docs.ps1
```

Busca local mínima:

```bash
rg -n "UIFocus|SystemMenu|Options|Settings|Accessibility|Localization|TextKey|IconKey|Notification|Journal|DialogueChoice|Confirmation|Gamepad|DebugHUD|Breath|Folego" Assets/_Game/Scripts docs/design docs/specs
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
Quando houver cenário visual/gameplay de options, accessibility, dialogue, notification, journal, localization text ou UI navigation, registrar em execution report e vincular a docs/validation/FINAL_HUMAN_VALIDATION_BY_WAVE.md.
```

---

## 26. Testing Quality Gate

- Changed deterministic logic: YES, dedupe/retention/filter/handoff logic is deterministic.
- Requires EditMode tests: YES for dedupe/retention/critical/hidden-quest/Fonte/debug/future-runtime tests.
- Requires PlayMode automated or final human scenario: YES, DEFERRED for journal UI/notification visual validation.
- Requires regression test: YES if fixing existing notification/journal bug; otherwise NO.
- Human validation timing: DEFERRED_TO_FINAL_VALIDATION se houver UI/gameplay integrado; caso contrário NOT REQUIRED.
- Minimum validation evidence for ACCEPTED: docs validation PASS; C# build PASS if C# changed; EditMode tests PASS or NOT RUN justified; journal safe and not source of truth.

---

## 27. Definition of Done

```text
Spec executada sem alterar arquivos proibidos.
Contratos/data/runtime implementados apenas dentro do escopo.
Execution report criado em docs/validation/18_spec_ui_notification_journal_feedback_history_future_runtime_execution_report.md.
Fonte/direction coverage preservado.
Validações obrigatórias PASS ou NOT RUN com motivo, impacto e mitigação.
Sem promoção indevida para ACCEPTED apenas por compile.
```

---

## 28. Anti-regressão

```text
Não reimplementar specs de UI já geradas.
Não alterar scenes/prefabs/assets/fonts.
Não salvar UI state como gameplay state.
Não usar debug HUD como UX final.
Não mostrar Breath/Fôlego/BR.
Não depender só de cor para estado crítico.
Não vazar spoiler cedo.
Não criar pet/social deep/companion AI.
Não pedir human test por spec.
Não executar runtime em massa antes da 01Q ou exceção humana explícita.
Não alterar SPEC_EXECUTION_ORDER.md.
```

---

## 29. Notas para execução posterior

Esta spec é future/mapped. Deve ser executada apenas depois das specs de UI fundamentais, input focus, HUD, projections, save/load e relevant domain state estarem estáveis ou com decisão humana explícita.
