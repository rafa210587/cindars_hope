# SPEC — UI Localization Text Keys Icon Conventions Future Runtime

> **Spec ID:** `18_spec_ui_localization_text_keys_icon_conventions_future_runtime`  
> **Status:** A implementar / Future mapped  
> **Wave:** WAVE 18 — UI Shell / Accessibility / Localization Future  
> **Priority:** P2  
> **Type:** Runtime / Future / UI Text / Localization-safe Contracts  
> **Domain:** TextKey / IconKey / Domain Copy / Translation-safe UI / Spoiler-safe Labels  
> **Parallelizable:** YES  
> **Parallel group:** WAVE_18_UI_SHELL_ACCESSIBILITY_LOCALIZATION_FUTURE  
> **Can run with:** options/accessibility or notification journal if no same files.  
> **Must not run with:** qualquer spec que adicione localization package, font files, text assets, final translations, icon sprites, prefabs/layout or scene assets.  
> **Repo lock scope:** `Assets/_Game/Scripts/UI/Text/**`, `Assets/_Game/Scripts/UI/Icons/**`, `Assets/_Game/Scripts/UI/Localization/**`, `Assets/_Game/Tests/EditMode/UI/**`, `docs/validation/18_spec_ui_localization_text_keys_icon_conventions_future_runtime_execution_report.md`  
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
  - all UI projections;
  - future localization package integration;
  - tooltip/dialogue/quest text review;
  - icon final art pass.
> **Scope:** definir/endurecer contratos future-safe de TextKey/IconKey, namespaces de texto, placeholders, pluralization hooks, spoiler-safe labels e icon conventions.  
> **Out of scope:** localization package setup, final translations, font files, icon sprites, prefab/layout changes, copywriting final.

---

# /speckit.specify

## 1. Contexto

UI/UX deixa como pendência definir regras de localização/tradução futura, convenção de ícones, fonte/tipografia final e estilos de painel. Várias specs já usam `TextKey`/`IconKey` nos ViewModels. Esta spec formaliza as convenções sem instalar package nem criar assets.

---

## 2. Problema

Sem convenção:

```text
texto hardcoded aparece em runtime;
TextKey sem namespace conflita;
placeholder quebra tradução;
ícone romântico aparece em NPC bloqueado;
quest oculta tem key reveladora;
debug id entra em texto final;
pluralização futura fica impossível;
Fonte/Anya/101/final vazam por nome de key exibida.
```

---

## 3. Objetivo

Criar/endurecer:

```text
UiTextKey;
UiIconKey;
UiTextNamespace;
UiTextPlaceholderPolicy;
UiPluralizationFutureHook;
UiSpoilerSafeLabelPolicy;
UiIconConventionPolicy;
UiDebugTextGuard.
```

---

## 4. Regras de design

```text
Runtime UI usa TextKey/IconKey, não texto final hardcoded, quando praticável.
TextKey não deve ser exibida ao jogador.
Keys não devem vazar spoiler em fallback final.
Placeholder deve ser nomeado e seguro.
Icons não devem comunicar estado crítico sozinhos.
Ícone romântico/social não aparece para NPC bloqueado.
Debug text separado do texto final.
```

---

## 5. User stories / engineering stories

```text
Como UI, quero projetar texto por keys estáveis.
Como future localization, quero namespaces claros.
Como spoiler policy, quero fallback seguro.
Como icon system, quero ícones consistentes e não ofensivos.
Como debug, quero evitar IDs/textos técnicos no final.
```

---

## 6. Escopo

Inclui:

```text
TextKey/IconKey structs;
namespace conventions;
placeholder policy;
pluralization hooks;
spoiler-safe labels;
icon conventions;
debug text guard;
tests/validators.
```

Não inclui:

```text
localization package;
translation files;
fonts;
sprites/icons;
copywriting final;
prefab layout.
```

## 7. Modelo de domínio

### 7.1 UiTextNamespace

```text
System
Hud
Notification
Dialogue
Quest
Social
Inventory
Equipment
Shop
Crafting
SkillTree
Calendar
Weather
Lunar
Fonte
Cave
Combat
SaveLoad
Endgame
Debug
```

### 7.2 UiTextKey

```text
Namespace
Key
SpoilerTier
FallbackKey optional
AllowedInFinal
DebugOnly
PlaceholderSchemaId optional
```

### 7.3 UiIconKey

```text
IconNamespace
IconName
SpoilerTier
AllowedInFinal
DebugOnly
CriticalStateRequiresText
BlockedForNpcProtectedRoute optional
```

### 7.4 UiTextPlaceholderPolicy

```text
PlaceholderSchemaId
AllowedPlaceholders[]
RequiredPlaceholders[]
TypeHints[]
SafeFallbackOnMissing
NoRawIdInFinal
```

### 7.5 UiSpoilerSafeLabelPolicy

```text
SpoilerTier
DiscoveredFlagRequired optional
QuestStateRequired optional
FallbackTextKey
CanRevealExactName
CanRevealExactLocation
CanRevealExactReward
```

---

## 8. Namespace rules

```text
Quest keys:
  do not include hidden objective names visible in fallback.

Fonte keys:
  stage-safe names only before unlock.

Endgame keys:
  hidden until gate.

Social keys:
  no romance icon/key for blocked NPC.

Debug keys:
  never AllowedInFinal.

System keys:
  safe for options/save/load/quit.
```

---

## 9. Placeholder rules

```text
Use named placeholders, not positional-only if avoidable.
Do not inject raw item/NPC/quest IDs in final text.
Do not use untrusted/generated text directly.
Missing placeholder falls back to safe generic text.
Pluralization hook records count and noun key separately.
```

---

## 10. Icon rules

```text
Critical states require text or shape/audio backup; not color-only.
Icon namespaces match domain.
Debug icons never final.
Quest/social/fonte/endgame icons respect spoiler gates.
Romantic/partner icon hidden for blocked/protected NPC.
Pet icon hidden unless pet runtime exists.
```

---

## 11. Criteria

```text
TextKey/IconKey contracts exist.
Namespaces defined.
Placeholder and pluralization hooks exist.
Spoiler-safe label policy exists.
Debug text guard exists.
Tests cover namespace validation, missing key fallback, placeholder validation, raw ID block, debug key blocked, romantic blocked icon, Fonte/endgame spoiler fallback and critical icon requires text.
```

# /speckit.plan

## 12. Arquitetura alvo

```text
Assets/_Game/Scripts/UI/Text/UiTextNamespace.cs
Assets/_Game/Scripts/UI/Text/UiTextKey.cs
Assets/_Game/Scripts/UI/Icons/UiIconKey.cs
Assets/_Game/Scripts/UI/Text/UiTextPlaceholderPolicy.cs
Assets/_Game/Scripts/UI/Text/UiPluralizationFutureHook.cs
Assets/_Game/Scripts/UI/Text/UiSpoilerSafeLabelPolicy.cs
Assets/_Game/Scripts/UI/Text/UiDebugTextGuard.cs
Assets/_Game/Scripts/UI/Icons/UiIconConventionPolicy.cs
Assets/_Game/Tests/EditMode/UI/LocalizationTextKeysIconConventionsTests.cs
```

Consolidar existentes se houver.

## 13. Arquivos permitidos

```text
Assets/_Game/Scripts/UI/Text/**
Assets/_Game/Scripts/UI/Icons/**
Assets/_Game/Scripts/UI/Localization/**
Assets/_Game/Scripts/Editor/Validation/**
Assets/_Game/Tests/EditMode/UI/**
docs/validation/18_spec_ui_localization_text_keys_icon_conventions_future_runtime_execution_report.md
```

## 14. Arquivos proibidos

```text
Packages/**
ProjectSettings/**
Assets/**/*.unity
Assets/**/*.prefab
Assets/**/*.asset
Assets/**/*.ttf
Assets/**/*.otf
Assets/**/*.png
Assets/**/*.jpg
Assets/**/*.jpeg
Assets/_Game/Scripts/Pets/**
docs/specs/SPEC_EXECUTION_ORDER.md
docs/specs/implementados/**
docs/refinements/implementados/**
docs/project/CURRENT_STATE.md
PROJECT_LOG.md
```

# /speckit.tasks

## 15. Tasks

- [ ] T001 — Ler fontes.
- [ ] T002 — Auditar existing TextKey/IconKey/localization patterns.
- [ ] T003 — Consolidar text/icon contracts.
- [ ] T004 — Implementar validators/guards.
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

- UI/UX deixa como pendência definir regras de localização/tradução futura.
- UI/UX deixa como pendência definir convenção de ícones.
- UI/UX deixa como pendência definir fonte/tipografia final, mas esta spec não entrega fonts.
- UI crítica não deve depender apenas de cor.
- HUD final não deve mostrar IDs/debug de runtime.
- Quest/social/romance/poliamor são feedbacks futuros quando runtime existir e não devem vazar cedo.

### Deferred / future from directions

- Localization package setup.
- Translation files.
- Final copywriting.
- Font files.
- Icon sprites.
- Final visual style.

### Explicitly not redefined here

- Actual text content.
- Dialogue writing.
- Quest content.
- Asset pipeline.
- UI prefab layout.
- Localization package.

---

## 23A. Execution Readiness Matrix

| Área | Pergunta obrigatória | Evidência esperada | Status se faltar |
|---|---|---|---|
| Fonte canônica | Executor leu UI/UX, menu flows e directions dos domínios afetados? | Lista no execution report. | PARTIAL |
| Estado real do repo | Sistemas existentes relacionados a localization text keys/icon conventions foram auditados antes de criar novos? | Comandos `rg` e achados. | PARTIAL |
| Não duplicação | Existe sistema equivalente já implementado/parcial? | REUSE/HARDEN/CREATE/DEFER. | BLOCKED se duplicar |
| UI não é fonte | UI não vira fonte de QuestState, SaveState, Inventory, Social, Fonte, Ending ou Economy. | Checklist/tests. | PARTIAL |
| Input/foco | Modal não permite WASD/attack/interact atrás da UI. | Checklist/tests ou dependência explícita. | PARTIAL |
| Acessibilidade | Não depender só de cor; texto legível; confirma destrutivo; reduzir flashes/volume preparados. | Checklist/tests. | PARTIAL |
| Anti-spoiler | UI/text/log não vaza Anya, Fonte, 101, final, quest oculta, eventos secretos ou NPC bloqueado. | Tests/checklist. | PARTIAL |
| Save/load | Houve alteração de schema? | Declaração NO ou STOP se migration necessária. | BLOCKED se alterar sem migration |
| Scene/assets | Não tocou scenes/prefabs/assets/fontes/ProjectSettings/Packages. | `Forbidden files touched: NO`. | BLOCKED se violar |
| Testes | Lógica determinística nova tem EditMode tests quando praticável? | Test list. | PARTIAL |
| Report | Execution report criado? | `docs/validation/18_spec_ui_localization_text_keys_icon_conventions_future_runtime_execution_report.md`. | PARTIAL |

---

## 23B. Local Audit Checklist

Antes de alterar qualquer arquivo, Claude Code/Codex deve rodar e registrar no execution report:

```bash
rg -n "TextKey|IconKey|Localization|UiText|UiIcon|Placeholder|Pluralization|SpoilerSafeLabel|DebugText" Assets/_Game/Scripts docs/design docs/specs
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

- Hardcoded text becomes final dependency.
- TextKey leaks spoiler in fallback.
- Raw IDs appear in final text.
- Missing placeholder breaks UI.
- Critical icon relies only on color.
- Romance icon shown for blocked NPC.
- Pet icon shown without pet runtime.
- Debug key allowed in final.

---

## 23E. Minimum Execution Report Template

```md
# Execution Report — UI Localization Text Keys Icon Conventions Future Runtime

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

- Changed deterministic logic: YES, key/placeholder/icon validation logic is deterministic.
- Requires EditMode tests: YES for namespace/fallback/placeholder/raw-id/debug/icon/spoiler tests.
- Requires PlayMode automated or final human scenario: YES, DEFERRED for visual text/icon validation.
- Requires regression test: YES if fixing existing hardcoded text/icon bug; otherwise NO.
- Human validation timing: DEFERRED_TO_FINAL_VALIDATION se houver UI/gameplay integrado; caso contrário NOT REQUIRED.
- Minimum validation evidence for ACCEPTED: docs validation PASS; C# build PASS if C# changed; EditMode tests PASS or NOT RUN justified; text/icon contracts localization-safe.

---

## 27. Definition of Done

```text
Spec executada sem alterar arquivos proibidos.
Contratos/data/runtime implementados apenas dentro do escopo.
Execution report criado em docs/validation/18_spec_ui_localization_text_keys_icon_conventions_future_runtime_execution_report.md.
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
