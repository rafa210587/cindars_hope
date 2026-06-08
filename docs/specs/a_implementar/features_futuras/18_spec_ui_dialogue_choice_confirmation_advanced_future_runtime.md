# SPEC — UI Dialogue Choice Confirmation Advanced Future Runtime

> **Spec ID:** `18_spec_ui_dialogue_choice_confirmation_advanced_future_runtime`  
> **Status:** A implementar / Future mapped  
> **Wave:** WAVE 18 — UI Shell / Accessibility / Localization Future  
> **Priority:** P1  
> **Type:** Runtime / Future / Dialogue UI / Choice / Confirmation  
> **Domain:** Dialogue Modal / Choice Metadata / Confirmation / Special Actions / Spoiler-safe Text  
> **Parallelizable:** YES  
> **Parallel group:** WAVE_18_UI_SHELL_ACCESSIBILITY_LOCALIZATION_FUTURE  
> **Can run with:** localization/text-key and notification journal specs if no same files.  
> **Must not run with:** qualquer spec que altere dialogue content, NPC roster, quest objective runtime, romance/social deep runtime, prefabs/layout, final input package or save schema.  
> **Repo lock scope:** `Assets/_Game/Scripts/UI/Dialogue/**`, `Assets/_Game/Scripts/Dialogue/**`, `Assets/_Game/Scripts/UI/Confirmations/**`, `Assets/_Game/Tests/EditMode/UI/**`, `docs/validation/18_spec_ui_dialogue_choice_confirmation_advanced_future_runtime_execution_report.md`  
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
  - quest dialogue integration;
  - gift confirmation UI;
  - final choice presentation;
  - social dialogue hooks;
  - localization text keys.
> **Scope:** definir/endurecer dialogue choice advanced contracts: compact layout, special action metadata, confirmation, disabled reasons and spoiler-safe choice projection.  
> **Out of scope:** dialogue writing, romance scenes, content authoring, prefab/layout final, quest engine rewrite.

---

# /speckit.specify

## 1. Contexto

Diálogo deve ocupar apenas o necessário: retrato/sprite, nome, texto, opções e indicadores de quest/social. Não deve ocupar tela inteira sem necessidade e não pode deixar WASD mover o personagem. Escolhas devem indicar ações especiais: aceitar quest, entregar item, comprar/vender, iniciar romance, confirmar presente, confirmar decisão final, entrar em caverna e ativar ritual/Fonte. Escolhas irreversíveis/grandes exigem confirmação.

---

## 2. Problema

Sem contrato avançado:

```text
opção de diálogo executa ação grande sem sinal;
entrega de item é ambígua;
romance/social future aparece para NPC bloqueado;
quest accept não indica consequência;
Fonte/final choice aparece cedo;
dialogue full-screen desnecessário;
disabled choices não explicam requisito;
WASD move durante diálogo.
```

---

## 3. Objetivo

Criar/endurecer:

```text
DialogueChoiceViewModel;
DialogueChoiceActionType;
DialogueChoiceAvailability;
DialogueSpecialActionMarker;
DialogueConfirmationPolicy;
DialogueCompactLayoutPolicy;
DialogueSpoilerProjectionPolicy;
DialogueChoiceCommandRequest.
```

---

## 4. Regras de design

```text
Dialogue modal bloqueia gameplay input.
Choice action metadata deve ser explícita.
Grandes/irreversíveis exigem confirmação.
Choice disabled deve explicar requisito quando seguro.
Dialogue UI é projection/command request, não fonte de Quest/Social/Fonte state.
Spoiler-safe projection para romance, Fonte, final e quest oculta.
```

---

## 5. User stories / engineering stories

```text
Como jogador, quero entender quando uma fala aceita quest, entrega item ou confirma ação.
Como UI, quero mostrar opção bloqueada com motivo seguro.
Como quest/social/Fonte, quero command request validado pelo domínio.
Como designer, quero dialogue compact e navegável.
Como accessibility, quero confirmação segura e cancel default.
```

---

## 6. Escopo

Inclui:

```text
dialogue choice view models;
action type metadata;
availability/blocked reason;
confirmation policy;
compact layout policy;
spoiler-safe projection;
tests.
```

Não inclui:

```text
dialogue text;
quest content;
romance scenes;
final choice domain;
prefab/layout final.
```

## 7. Modelo de domínio

### 7.1 DialogueChoiceActionType

```text
PlainDialogue
AcceptQuest
DeclineQuest
CompleteQuest
DeliverItem
Buy
Sell
OpenShop
StartGift
ConfirmGift
StartRomanceFuture
PolyRelationshipFuture
MarriageFuture
EnterCave
ActivateFonte
StartRitual
FinalChoiceFuture
SaveLoadSystem
DebugOnly
```

### 7.2 DialogueChoiceAvailability

```text
Available
DisabledMissingItem
DisabledMissingQuest
DisabledRelationship
DisabledStoryGate
DisabledNpcBlocked
DisabledDangerousState
DisabledAlreadyDone
HiddenSpoiler
HiddenDebugOnly
```

### 7.3 DialogueChoiceViewModel

```text
ChoiceId
TextKey
ActionType
Availability
Visible
Enabled
RequiresConfirmation
ConfirmationPolicyId optional
IconKey optional
WarningTextKey optional
BlockedReasonTextKey optional
SpoilerTier
DefaultFocusPriority
CommandRequest optional
```

### 7.4 DialogueConfirmationPolicy

```text
PolicyId
RequiresSecondStep
DefaultFocusCancel
DestructiveOrIrreversible
LargeNarrativeImpact
ConsumesItem
StartsRelationshipRoute
StartsFinalChoice
WarningTextKey
```

### 7.5 DialogueChoiceCommandRequest

```text
CommandType
TargetDomain: Quest | Inventory | Shop | Social | Cave | Fonte | Endgame | System
TargetId
PayloadIds[]
RequiresDomainValidation true
UiDoesNotMutateState true
```

---

## 8. Choice projection rules

```text
Plain dialogue:
  no icon unless needed.

Quest:
  marker accepted/complete/deliver.

Item delivery:
  show item requirement and quantity if known.

Gift:
  show confirmation if item consumed.

Romance/social future:
  only if eligibility discovered and safe; no icon for blocked/TooYoung/NarrativelyBlocked.

Fonte/final:
  hidden until correct fragment/gate.

Debug:
  never in final UI.
```

---

## 9. Confirmation rules

```text
Default focus on cancel for irreversible choice.
Confirm gift consumes item only through domain command.
Confirm final choice delegates to final choice service/presentation.
Enter cave may warn about loadout/risk.
Activate Fonte may warn if one-way or limited resource.
```

---

## 10. Criteria

```text
Dialogue choice contracts exist.
Special action metadata explicit.
Confirmation policy exists.
Hidden/disabled states are spoiler-safe.
UI command request delegates to domain validation.
Tests cover accept quest, deliver item, confirm gift, blocked romance, Fonte hidden, final hidden, cancel default and no UI state mutation.
```

# /speckit.plan

## 11. Arquitetura alvo

```text
Assets/_Game/Scripts/UI/Dialogue/DialogueChoiceActionType.cs
Assets/_Game/Scripts/UI/Dialogue/DialogueChoiceAvailability.cs
Assets/_Game/Scripts/UI/Dialogue/DialogueChoiceViewModel.cs
Assets/_Game/Scripts/UI/Dialogue/DialogueConfirmationPolicy.cs
Assets/_Game/Scripts/UI/Dialogue/DialogueCompactLayoutPolicy.cs
Assets/_Game/Scripts/UI/Dialogue/DialogueSpoilerProjectionPolicy.cs
Assets/_Game/Scripts/UI/Dialogue/DialogueChoiceCommandRequest.cs
Assets/_Game/Tests/EditMode/UI/DialogueChoiceConfirmationAdvancedTests.cs
```

Consolidar existentes se houver.

## 12. Arquivos permitidos

```text
Assets/_Game/Scripts/UI/Dialogue/**
Assets/_Game/Scripts/UI/Confirmations/**
Assets/_Game/Scripts/Dialogue/**
Assets/_Game/Scripts/Quests/**
Assets/_Game/Scripts/Social/**
Assets/_Game/Scripts/Fonte/**
Assets/_Game/Scripts/Editor/Validation/**
Assets/_Game/Tests/EditMode/UI/**
docs/validation/18_spec_ui_dialogue_choice_confirmation_advanced_future_runtime_execution_report.md
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
- [ ] T002 — Auditar dialogue/choice/confirmation systems.
- [ ] T003 — Consolidar choice metadata contracts.
- [ ] T004 — Implementar projection/validators.
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

- Diálogo deve ocupar apenas o necessário: retrato/sprite, nome, texto, opções e indicadores de quest/social.
- Diálogo não deve ocupar a tela inteira sem necessidade.
- Diálogo não deve deixar WASD mover o personagem.
- Opções devem ser navegáveis por teclado/mouse e gamepad futuro.
- Escolhas devem indicar ações especiais: aceitar quest, entregar item, comprar/vender, iniciar romance, confirmar presente, confirmar decisão final, entrar em caverna, ativar ritual/Fonte.
- Escolhas irreversíveis ou grandes devem ter confirmação.

### Deferred / future from directions

- Dialogue writing.
- Prefab layout.
- Romance scenes.
- Quest content.
- Final choice domain.
- Gamepad final implementation.

### Explicitly not redefined here

- Dialogue engine.
- Quest engine.
- Social eligibility.
- FonteAnya state.
- Final choice domain.
- Input focus base.

---

## 23A. Execution Readiness Matrix

| Área | Pergunta obrigatória | Evidência esperada | Status se faltar |
|---|---|---|---|
| Fonte canônica | Executor leu UI/UX, menu flows e directions dos domínios afetados? | Lista no execution report. | PARTIAL |
| Estado real do repo | Sistemas existentes relacionados a dialogue choice/confirmation advanced UI foram auditados antes de criar novos? | Comandos `rg` e achados. | PARTIAL |
| Não duplicação | Existe sistema equivalente já implementado/parcial? | REUSE/HARDEN/CREATE/DEFER. | BLOCKED se duplicar |
| UI não é fonte | UI não vira fonte de QuestState, SaveState, Inventory, Social, Fonte, Ending ou Economy. | Checklist/tests. | PARTIAL |
| Input/foco | Modal não permite WASD/attack/interact atrás da UI. | Checklist/tests ou dependência explícita. | PARTIAL |
| Acessibilidade | Não depender só de cor; texto legível; confirma destrutivo; reduzir flashes/volume preparados. | Checklist/tests. | PARTIAL |
| Anti-spoiler | UI/text/log não vaza Anya, Fonte, 101, final, quest oculta, eventos secretos ou NPC bloqueado. | Tests/checklist. | PARTIAL |
| Save/load | Houve alteração de schema? | Declaração NO ou STOP se migration necessária. | BLOCKED se alterar sem migration |
| Scene/assets | Não tocou scenes/prefabs/assets/fontes/ProjectSettings/Packages. | `Forbidden files touched: NO`. | BLOCKED se violar |
| Testes | Lógica determinística nova tem EditMode tests quando praticável? | Test list. | PARTIAL |
| Report | Execution report criado? | `docs/validation/18_spec_ui_dialogue_choice_confirmation_advanced_future_runtime_execution_report.md`. | PARTIAL |

---

## 23B. Local Audit Checklist

Antes de alterar qualquer arquivo, Claude Code/Codex deve rodar e registrar no execution report:

```bash
rg -n "DialogueChoice|DialogueConfirmation|AcceptQuest|DeliverItem|ConfirmGift|StartRomance|FinalChoice|ActivateFonte|EnterCave" Assets/_Game/Scripts docs/design docs/specs
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

- Choice executes special action without marker.
- Irreversible choice confirms accidentally.
- Blocked romance option visible for protected NPC.
- Fonte/final choice appears early.
- Disabled choice reveals hidden requirement.
- UI mutates quest/inventory/social directly.
- Dialogue consumes whole screen unnecessarily.
- Gameplay input passes behind dialogue.

---

## 23E. Minimum Execution Report Template

```md
# Execution Report — UI Dialogue Choice Confirmation Advanced Future Runtime

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

- Changed deterministic logic: YES, choice projection/availability/confirmation logic is deterministic.
- Requires EditMode tests: YES for quest/item/gift/blocked-romance/Fonte/final/cancel/no-mutation tests.
- Requires PlayMode automated or final human scenario: YES, DEFERRED for dialogue visual/navigation validation.
- Requires regression test: YES if fixing existing dialogue choice bug; otherwise NO.
- Human validation timing: DEFERRED_TO_FINAL_VALIDATION se houver UI/gameplay integrado; caso contrário NOT REQUIRED.
- Minimum validation evidence for ACCEPTED: docs validation PASS; C# build PASS if C# changed; EditMode tests PASS or NOT RUN justified; choices marked and safe.

---

## 27. Definition of Done

```text
Spec executada sem alterar arquivos proibidos.
Contratos/data/runtime implementados apenas dentro do escopo.
Execution report criado em docs/validation/18_spec_ui_dialogue_choice_confirmation_advanced_future_runtime_execution_report.md.
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
