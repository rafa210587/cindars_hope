# SPEC — UI System Menu Options Accessibility Hooks Future Runtime

> **Spec ID:** `18_spec_ui_system_menu_options_accessibility_hooks_future_runtime`  
> **Status:** A implementar / Future mapped  
> **Wave:** WAVE 18 — UI Shell / Accessibility / Localization Future  
> **Priority:** P1  
> **Type:** Runtime / Future / UI Shell / Options / Accessibility  
> **Domain:** System Menu / Options / Accessibility Hooks / Volume / Reduce Flash / Navigation  
> **Parallelizable:** YES  
> **Parallel group:** WAVE_18_UI_SHELL_ACCESSIBILITY_LOCALIZATION_FUTURE  
> **Can run with:** localization/text-key convention spec if no same files.  
> **Must not run with:** qualquer spec que altere ProjectSettings, Input System package, prefabs, scenes, fonts, save schema migration, audio mixer assets or final visual style.  
> **Repo lock scope:** `Assets/_Game/Scripts/UI/System/**`, `Assets/_Game/Scripts/UI/Accessibility/**`, `Assets/_Game/Scripts/UI/Options/**`, `Assets/_Game/Tests/EditMode/UI/**`, `docs/validation/18_spec_ui_system_menu_options_accessibility_hooks_future_runtime_execution_report.md`  
> **Depends on:**  
  - `docs/design/SPEC_SOURCE_MAP.md`
  - `docs/design/SPECIFICATION_PROCESS.md`
  - `docs/design/lore/VAALARA_GAME_CANON_DIRECTION_v1.0.md`
  - `.specs/SPEC_IMPLEMENTABLE_TEMPLATE.md`
  - `.specs/SPEC_WAVE_EXECUTION_PROTOCOL.md`
  - `.specs/SPEC_VALIDATION_MATRIX_MASTER.md`
  - `.specs/SPEC_EXISTING_IMPLEMENTATION_AUDIT.md`
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
  - pause/options UI;
  - save/load UI shell;
  - gamepad future navigation;
  - final UI visual pass;
  - final human validation.
> **Scope:** definir/endurecer contratos runtime futuros de system menu, options e accessibility hooks sem mexer em ProjectSettings/packages/prefabs.  
> **Out of scope:** ProjectSettings/Input package changes, final keybind remap, audio mixer assets, UI prefabs/layout, final fonts/visual style.

---

# /speckit.specify

## 1. Contexto

UI/UX define System Menu como camada própria para pause, options, save/load e quit. Acessibilidade básica exige texto legível, contraste, não depender só de cor, feedback visual + sonoro em eventos críticos, opção futura de reduzir flashes fortes, opção futura de volume UI/SFX, confirmação de ações destrutivas, teclado/mouse e preparo para gamepad.

---

## 2. Problema

Sem shell/options/accessibility hooks:

```text
Options vira prefab isolado sem contrato;
reduzir flashes não tem estado;
volume UI/SFX não tem estado runtime;
ações destrutivas sem confirmação;
gamepad futuro exige redesenhar UI;
save/load/quit podem executar sem confirmação;
System menu não tem prioridade de foco;
settings podem tentar alterar ProjectSettings diretamente.
```

---

## 3. Objetivo

Criar/endurecer:

```text
SystemMenuState;
OptionsMenuViewModel;
AccessibilitySettingsState;
UiAudioSettingsState;
DestructiveActionConfirmationPolicy;
UiNavigationCapability;
GamepadFutureNavigationHint;
SettingsRuntimeAdapter.
```

---

## 4. Regras de design

```text
System modal tem prioridade máxima.
Options não muda ProjectSettings diretamente.
Settings são estado runtime/config autorizado.
Ações destrutivas exigem confirmação.
Acessibilidade não depende de asset/prefab.
Gamepad futuro deve ser preparado por navegação declarativa.
```

---

## 5. User stories / engineering stories

```text
Como jogador, quero pausar/options/save/load/quit com confirmações seguras.
Como UI, quero system modal com foco e prioridade.
Como acessibilidade, quero hooks para texto/contraste/reduzir flashes/volume.
Como futuro gamepad, quero navigation graph sem redesenhar telas.
Como save/load, quero options state seguro e separado de gameplay.
```

---

## 6. Escopo

Inclui:

```text
system menu state;
options view model;
accessibility setting contracts;
UI/SFX volume contracts;
destructive confirmation policy;
navigation capability model;
tests/validators.
```

Não inclui:

```text
ProjectSettings;
Input System package settings;
audio mixer assets;
prefabs/layout;
font files;
final remapping UI.
```

## 7. Modelo de domínio

### 7.1 SystemMenuState

```text
Closed
PauseRoot
Options
SaveLoad
ConfirmQuit
ConfirmLoad
ConfirmDeleteSave
ControlsFuture
Accessibility
Audio
```

### 7.2 AccessibilitySettingsState

```text
TextScalePreset: Small | Normal | Large | ExtraLarge
HighContrastMode: Off | On | Future
ColorOnlyCriticalStatesAllowed false
ReduceStrongFlashes: Off | On
ScreenShakeIntensity: Off | Low | Normal
UiAnimationIntensity: Reduced | Normal
CriticalFeedbackMode: VisualAndAudio | VisualOnly | AudioOnly | Future
```

### 7.3 UiAudioSettingsState

```text
MasterVolume
MusicVolume
SfxVolume
UiVolume
AmbientVolume
MuteWhenUnfocusedFuture
```

### 7.4 DestructiveActionConfirmationPolicy

```text
ActionType: QuitToTitle | LoadSave | DeleteSave | AbandonCaveRun | FinalChoice | DropUnique | SellProtected
RequiresConfirmation
RequiresHoldConfirm optional
RequiresTypedToken optional
DefaultFocus: Cancel
WarningTextKey
```

### 7.5 UiNavigationCapability

```text
SupportsKeyboard
SupportsMouse
SupportsGamepadFuture
DefaultFocusElementId
CancelTarget
ConfirmTarget
NavigationGraphId
FocusWrapPolicy
```

---

## 8. System menu rules

```text
System menu blocks gameplay input.
System menu can open Options, Save/Load and Quit confirmation.
Confirm destructive actions with cancel as safe default.
Options changes apply to runtime/config adapter, not ProjectSettings.
Closed system menu returns focus to previous safe state.
```

---

## 9. Accessibility rules

```text
Critical state must not depend only on color.
Text scale is a contract, not final font implementation.
Reduce strong flashes gates intense UI feedback/VFX requests.
UI/SFX volume state must be separate from master audio backend.
Screen shake setting is a request/adapter.
```

---

## 10. Criteria

```text
System/options/accessibility contracts exist.
Destructive confirmations default to cancel.
No ProjectSettings/package/prefab/asset changes.
Critical states have not-color-only policy.
Gamepad future navigation metadata exists.
Tests cover system focus priority, confirmation defaults, accessibility state, volume clamp, no ProjectSettings adapter and no color-only critical.
```

# /speckit.plan

## 11. Arquitetura alvo

```text
Assets/_Game/Scripts/UI/System/SystemMenuState.cs
Assets/_Game/Scripts/UI/System/OptionsMenuViewModel.cs
Assets/_Game/Scripts/UI/Accessibility/AccessibilitySettingsState.cs
Assets/_Game/Scripts/UI/Accessibility/AccessibilityPolicyValidator.cs
Assets/_Game/Scripts/UI/Options/UiAudioSettingsState.cs
Assets/_Game/Scripts/UI/System/DestructiveActionConfirmationPolicy.cs
Assets/_Game/Scripts/UI/Navigation/UiNavigationCapability.cs
Assets/_Game/Scripts/UI/Options/SettingsRuntimeAdapter.cs
Assets/_Game/Tests/EditMode/UI/SystemMenuOptionsAccessibilityTests.cs
```

Consolidar existentes se houver.

## 12. Arquivos permitidos

```text
Assets/_Game/Scripts/UI/System/**
Assets/_Game/Scripts/UI/Accessibility/**
Assets/_Game/Scripts/UI/Options/**
Assets/_Game/Scripts/UI/Navigation/**
Assets/_Game/Scripts/Editor/Validation/**
Assets/_Game/Tests/EditMode/UI/**
docs/validation/18_spec_ui_system_menu_options_accessibility_hooks_future_runtime_execution_report.md
```

## 13. Arquivos proibidos

```text
Packages/**
ProjectSettings/**
Assets/**/*.unity
Assets/**/*.prefab
Assets/**/*.asset
Assets/**/*.ttf
Assets/**/*.otf
Assets/_Game/Scripts/Pets/**
.specs/SPEC_EXECUTION_ORDER.md
.specs/implementados/**
docs/refinements/implementados/**
docs/project/CURRENT_STATE.md
PROJECT_LOG.md
```

# /speckit.tasks

## 14. Tasks

- [ ] T001 — Ler fontes.
- [ ] T002 — Auditar system/options/accessibility UI.
- [ ] T003 — Consolidar state/settings/policy contracts.
- [ ] T004 — Implementar validators/adapters.
- [ ] T005 — Criar tests.
- [ ] T006 — Rodar validações.
- [ ] T007 — Criar report.

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

- System Menu é camada própria para pause, options, save/load e quit.
- Quanto mais modal a UI, maior deve ser bloqueio de input de gameplay.
- Acessibilidade básica exige texto legível, contraste suficiente, não depender só de cor para estados críticos e feedback visual + sonoro em eventos críticos.
- Opção futura de reduzir flashes fortes.
- Opção futura de ajustar volume de UI/SFX.
- Confirmar ações destrutivas.
- Permitir navegação clara por teclado/mouse e preparar gamepad futuro sem redesenhar toda UI.

### Deferred / future from directions

- Final visual layout.
- Keybind remap final.
- Input System package settings.
- Audio mixer assets.
- Final fonts/tipography.
- Gamepad implementation final.

### Explicitly not redefined here

- UI input focus base spec.
- Save/load domain.
- Audio backend.
- ProjectSettings.
- Prefabs/layout.
- Final accessibility QA.

---

## 23A. Execution Readiness Matrix

| Área | Pergunta obrigatória | Evidência esperada | Status se faltar |
|---|---|---|---|
| Fonte canônica | Executor leu UI/UX, menu flows e directions dos domínios afetados? | Lista no execution report. | PARTIAL |
| Estado real do repo | Sistemas existentes relacionados a system menu/options/accessibility hooks foram auditados antes de criar novos? | Comandos `rg` e achados. | PARTIAL |
| Não duplicação | Existe sistema equivalente já implementado/parcial? | REUSE/HARDEN/CREATE/DEFER. | BLOCKED se duplicar |
| UI não é fonte | UI não vira fonte de QuestState, SaveState, Inventory, Social, Fonte, Ending ou Economy. | Checklist/tests. | PARTIAL |
| Input/foco | Modal não permite WASD/attack/interact atrás da UI. | Checklist/tests ou dependência explícita. | PARTIAL |
| Acessibilidade | Não depender só de cor; texto legível; confirma destrutivo; reduzir flashes/volume preparados. | Checklist/tests. | PARTIAL |
| Anti-spoiler | UI/text/log não vaza Anya, Fonte, 101, final, quest oculta, eventos secretos ou NPC bloqueado. | Tests/checklist. | PARTIAL |
| Save/load | Houve alteração de schema? | Declaração NO ou STOP se migration necessária. | BLOCKED se alterar sem migration |
| Scene/assets | Não tocou scenes/prefabs/assets/fontes/ProjectSettings/Packages. | `Forbidden files touched: NO`. | BLOCKED se violar |
| Testes | Lógica determinística nova tem EditMode tests quando praticável? | Test list. | PARTIAL |
| Report | Execution report criado? | `docs/validation/18_spec_ui_system_menu_options_accessibility_hooks_future_runtime_execution_report.md`. | PARTIAL |

---

## 23B. Local Audit Checklist

Antes de alterar qualquer arquivo, Claude Code/Codex deve rodar e registrar no execution report:

```bash
rg -n "SystemMenu|OptionsMenu|Accessibility|ReduceStrongFlashes|UiVolume|DestructiveAction|GamepadFuture|NavigationCapability" Assets/_Game/Scripts docs/design .specs
rg -n "UIFocus|SystemMenu|Options|Settings|Accessibility|Localization|TextKey|IconKey|Notification|Journal|DialogueChoice|Confirmation|Gamepad|Keyboard|Mouse|DebugHUD|Breath|Folego|FinalChoice|Fonte|Level101|SecretEvent|Pet|Romance" Assets/_Game/Scripts docs/design .specs
rg -n "TODO|FIXME|HACK|PARTIAL|DEFERRED|BUILD_VALIDATED|ACCEPTED" .specs docs/validation docs/IMPLEMENTATION_STATUS.md docs/project/CURRENT_STATE.md
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

- Options touches ProjectSettings.
- System menu does not block gameplay input.
- Delete/load/quit default focus is confirm.
- Critical state depends only on color.
- Reduce flash state unused by contracts.
- Volume values out of range.
- Gamepad navigation impossible due missing graph metadata.
- Settings saved as UI transient instead of config/runtime state.

---

## 23E. Minimum Execution Report Template

```md
# Execution Report — UI System Menu Options Accessibility Hooks Future Runtime

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
rg -n "UIFocus|SystemMenu|Options|Settings|Accessibility|Localization|TextKey|IconKey|Notification|Journal|DialogueChoice|Confirmation|Gamepad|DebugHUD|Breath|Folego" Assets/_Game/Scripts docs/design .specs
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

- Changed deterministic logic: YES, settings/policy/navigation validation logic is deterministic.
- Requires EditMode tests: YES for focus/confirmation/accessibility/volume/no-projectsettings/navigation tests.
- Requires PlayMode automated or final human scenario: YES, DEFERRED for visual/options menu validation.
- Requires regression test: YES if fixing existing options/accessibility bug; otherwise NO.
- Human validation timing: DEFERRED_TO_FINAL_VALIDATION se houver UI/gameplay integrado; caso contrário NOT REQUIRED.
- Minimum validation evidence for ACCEPTED: docs validation PASS; C# build PASS if C# changed; EditMode tests PASS or NOT RUN justified; system menu/options contracts safe.

---

## 27. Definition of Done

```text
Spec executada sem alterar arquivos proibidos.
Contratos/data/runtime implementados apenas dentro do escopo.
Execution report criado em docs/validation/18_spec_ui_system_menu_options_accessibility_hooks_future_runtime_execution_report.md.
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
