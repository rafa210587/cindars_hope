# SPEC — UI HUD Hotbar Active Slots Notifications Debug Separation Runtime

> **Spec ID:** `11_spec_ui_hud_hotbar_active_slots_notifications_debug_runtime`  
> **Status:** A implementar  
> **Wave:** WAVE 11 — UI / UX / Input / Menus  
> **Priority:** P0  
> **Type:** Runtime / UI Projection / HUD / Hotbar / Notifications / Debug  
> **Domain:** UI / HUD ViewModel / Hotbar / Active Slots / Notifications / Debug HUD Separation  
> **Parallelizable:** YES  
> **Parallel group:** WAVE_11_UI_UX_RUNTIME  
> **Can run with:** UI input focus modal routing; inventory/equipment tooltip projection if lock scopes do not overlap.  
> **Must not run with:** qualquer spec que altere combat/player mechanics, skill tree runtime, companion/pet runtime, notification visual prefab, debug console, save schema ou final HUD layout assets.  
> **Repo lock scope:** `Assets/_Game/Scripts/UI/HUD/**`, `Assets/_Game/Scripts/UI/Notifications/**`, `Assets/_Game/Scripts/UI/Debug/**`, `Assets/_Game/Scripts/Editor/Validation/**`, `Assets/_Game/Tests/EditMode/UI/**`, `docs/validation/11_spec_ui_hud_hotbar_active_slots_notifications_debug_runtime_execution_report.md`  
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
  - `docs/design/gameplay/player/PLAYER_CORE_SYSTEMS_DIRECTION.md`
  - `docs/design/gameplay/combat/COMBAT_CORE_DIRECTION.md`
  - `docs/design/gameplay/combat/STATUS_EFFECTS_DIRECTION.md`
  - `docs/design/gameplay/magic/MAGIC_SPELLS_ACTIONS_DIRECTION.md`
  - `docs/design/gameplay/magic/MAGIC_LEARNING_UNLOCKS_SOURCES_DIRECTION.md`
  - `docs/design/gameplay/quests/QUEST_OBJECTIVE_EVENT_SYSTEM_DIRECTION.md`
  - `docs/design/gameplay/quests/QUESTS_MAIN_PROGRESSION_REFINEMENT_DIRECTION.md`
  - `docs/design/gameplay/cave/CAVE_DESIGN_DIRECTION.md`
  - `docs/design/gameplay/companions/COMPANIONS_DIRECTION.md`
  - `docs/design/gameplay/pets/PETS_DIRECTION.md`
> **Blocks:**  
  - combat feedback UI;
  - hotbar input;
  - skill active slot UI;
  - quest/Fonte notifications;
  - final HUD visual pass.
> **Scope:** definir/endurecer projections de HUD principal, hotbar, 4 active slots, notifications e separação Debug HUD vs HUD final.  
> **Out of scope:** HUD prefab/layout final, VFX/SFX final, combat mechanics, companion/pet runtime, debug console, accessibility options UI.

---

# /speckit.specify

## 1. Contexto

HUD base consolidada inclui HP, MP quando relevante, Stamina, Fome, Cansaço, hotbar, 4 active slots, arma/ferramenta ativa, status, buffs, companion/pet state quando relevante, quest/context prompt. HUD final não mostra Breath/Fôlego/BR nem debug interno.

Esta spec cria ViewModels/projections e regras de notifications, não layout final.

---

## 2. Problema

Sem HUD contract:

```text
HUD mostra debug ids no gameplay final;
Breath/Fôlego/BR reaparece;
active slots passam de 4;
Dash/Dodge/Block ocupam slot ativo indevidamente;
notificações cobrem gameplay;
status/buffs aparecem sem prioridade;
pet/companion state aparece sem runtime ativo;
quest text permanente polui combate;
debug HUD vira meio obrigatório de jogar.
```

---

## 3. Objetivo

Criar/endurecer:

```text
GameplayHudViewModel;
HudResourceProjection;
HotbarSlotViewModel;
ActiveSkillSlotViewModel;
StatusBuffProjection;
ContextPromptProjection;
NotificationQueuePolicy;
NotificationViewModel;
DebugHudProjection;
FinalHudGuardValidator.
```

---

## 4. Regras de design

```text
HUD é legível e compacta.
HUD não compete com cena.
HUD não cobre área central de combate/fazenda/cidade.
HUD não mostra Breath/Fôlego/BR.
HUD final não depende de debug.
Active slots = até 4 habilidades equipáveis.
Dash/Dodge/Block não ocupam active slots.
Notifications não bloqueiam input salvo confirmação importante.
```

---

## 5. User stories / engineering stories

```text
Como jogador, quero ler HP/Stamina/hotbar/skills sem poluição.
Como combat, quero feedback de insufficient MP/Stamina e status ativos.
Como quest/Fonte, quero notification sem revelar spoiler.
Como debug, quero HUD debug separado do HUD final.
Como future companion/pet, quero slot projection existir sem runtime agora.
```

---

## 6. Escopo

Inclui:

```text
HUD ViewModels;
hotbar projection;
active slot projection capped at 4;
notification queue/priority policy;
debug HUD separation contract;
no Breath/Folego validator;
context prompt projection;
tests.
```

Não inclui:

```text
visual layout/prefabs;
combat VFX/SFX final;
skill runtime;
hotbar input binding;
companion/pet runtime;
debug console.
```

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
- docs/design/gameplay/player/PLAYER_CORE_SYSTEMS_DIRECTION.md
- docs/design/gameplay/combat/COMBAT_CORE_DIRECTION.md
- docs/design/gameplay/combat/STATUS_EFFECTS_DIRECTION.md
- docs/design/gameplay/magic/MAGIC_SPELLS_ACTIONS_DIRECTION.md
- docs/design/gameplay/magic/MAGIC_LEARNING_UNLOCKS_SOURCES_DIRECTION.md
- docs/design/gameplay/quests/QUEST_OBJECTIVE_EVENT_SYSTEM_DIRECTION.md
- docs/design/gameplay/quests/QUESTS_MAIN_PROGRESSION_REFINEMENT_DIRECTION.md
- docs/design/gameplay/cave/CAVE_DESIGN_DIRECTION.md
- docs/design/gameplay/companions/COMPANIONS_DIRECTION.md
- docs/design/gameplay/pets/PETS_DIRECTION.md

### Required interpretation

```text
Esta spec é derivada dos directions/refinements canônicos.
Ela não substitui os directions.
Ela transforma parte do refinement em contrato implementável com escopo, locks, validações e quality gate.
Quando houver divergência entre esta spec e os directions, o executor deve parar e registrar CONFLICT no execution report.
```

---

## Direction / Refinement Coverage

### Covered from directions

- HUD principal deve mostrar HP, MP quando relevante, Stamina, Fome, Cansaço, hotbar, 4 active slots, arma/ferramenta ativa, status negativos, buffs, companion/pet state quando relevante e quest/context prompt.
- HUD não deve mostrar Breath, Fôlego, BR, números internos excessivos, cálculos intermediários ou debug runtime em build final.
- Permanentes: HP, Stamina, hotbar, active slots, arma/ferramenta ativa.
- Contextuais: MP, Fome/Cansaço compacto, companion/pet state se ativo/relevante, status/buffs, quest/context prompt.
- Hotbar = ferramentas, armas, consumíveis e itens rápidos; Active slots = até 4 habilidades equipáveis; Dash/Dodge/Block não ocupam active slots.
- Notifications incluem loot recebido, item insuficiente, quest atualizada/concluída, recipe/spell/skill point, relationship, shop restock, farm event, Fonte reagiu e fragmento recuperado; não devem bloquear input salvo confirmação importante.

### Deferred / future from directions

- Final HUD layout.
- Notification visual animation.
- Combat VFX/SFX.
- Companion/Pet runtime.
- Gamepad navigation.
- Accessibility options screen.

### Explicitly not redefined here

- Player stat mechanics.
- Skill tree active slot backend.
- Inventory/hotbar backend.
- Quest/Fonte/MainProgression systems.
- Debug command systems.

## 7. Modelo de domínio

### 7.1 GameplayHudViewModel

```text
Hp
MaxHp
ShowMp
Mp
MaxMp
Stamina
MaxStamina
HungerCompact
FatigueCompact
HotbarSlots[]
ActiveSkillSlots[0..4]
ActiveToolOrWeapon
StatusEffects[]
Buffs[]
ContextPrompt
QuestPrompt optional
CompanionProjection optional/future
PetProjection optional/future
DebugVisible false in final
```

### 7.2 HotbarSlotViewModel

```text
SlotIndex
ItemId
ItemIconId
Quantity
Durability optional
Cooldown optional
IsSelected
IsUsableInContext
BlockedReason optional
```

### 7.3 ActiveSkillSlotViewModel

```text
SlotIndex
SkillId
IconId
Cooldown
CostMp
CostStamina
IsUnlocked
IsEquipped
IsUsableInContext
BlockedReason: Empty | Locked | Cooldown | NoMP | NoStamina | InvalidTarget | WrongContext
```

### 7.4 NotificationPriority

```text
Low
Normal
Important
CriticalModal
DebugOnly
```

### 7.5 NotificationViewModel

```text
NotificationId
Type
Priority
TextKey
IconId optional
Duration
BlocksInput
RequiresConfirmation
SpoilerTier
StackPolicy
DebugTags[]
```

---

## 8. HUD visibility rules

```text
HP/Stamina/hotbar/active slots remain visible in gameplay.
MP appears when magic/build/equipment relevant.
Fome/Cansaço compact outside danger.
Status/buffs appear when active.
Quest/context prompt appears near relevant interaction or tracked objective.
Companion/pet state appears only if runtime active and relevant.
Debug HUD is explicitly marked debug and never final communication.
```

---

## 9. Notification queue policy

```text
Low/Normal notifications can stack but cap visible count.
Important notifications can extend duration.
CriticalModal uses confirmation modal/focus system.
DebugOnly notifications never appear in final HUD.
Quest/Fonte/Main notifications must respect spoiler projection.
Duplicate notification can merge when StackPolicy allows.
```

---

## 10. Debug HUD separation

Debug may show:

```text
IDs;
coords;
state machines;
quest flags;
enemy budget;
spawn anchors;
input focus;
shop stock state;
save/load state.
```

Final HUD must not show those.

---

## 11. Criteria

```text
HUD projection exists or is hardened.
Active slots capped at 4.
Dash/Dodge/Block excluded from active slots.
No Breath/Folego/BR in final HUD projection.
Notifications have queue/priority policy.
Debug HUD separate and marked.
Tests cover HUD projection, no forbidden stat, active slot cap, notification priority and debug separation.
```

---

# /speckit.plan

## 12. Arquitetura alvo

```text
Assets/_Game/Scripts/UI/HUD/GameplayHudViewModel.cs
Assets/_Game/Scripts/UI/HUD/HotbarSlotViewModel.cs
Assets/_Game/Scripts/UI/HUD/ActiveSkillSlotViewModel.cs
Assets/_Game/Scripts/UI/HUD/StatusBuffProjection.cs
Assets/_Game/Scripts/UI/Notifications/NotificationPriority.cs
Assets/_Game/Scripts/UI/Notifications/NotificationViewModel.cs
Assets/_Game/Scripts/UI/Notifications/NotificationQueuePolicy.cs
Assets/_Game/Scripts/UI/Debug/DebugHudProjection.cs
Assets/_Game/Scripts/UI/HUD/FinalHudGuardValidator.cs
Assets/_Game/Tests/EditMode/UI/HudNotificationDebugProjectionTests.cs
```

Consolidar existentes se houver.

---

## 13. Arquivos permitidos

```text
Assets/_Game/Scripts/UI/HUD/**
Assets/_Game/Scripts/UI/Notifications/**
Assets/_Game/Scripts/UI/Debug/**
Assets/_Game/Scripts/Editor/Validation/**
Assets/_Game/Tests/EditMode/UI/**
docs/validation/11_spec_ui_hud_hotbar_active_slots_notifications_debug_runtime_execution_report.md
```

Leitura permitida:

```text
Assets/_Game/Scripts/Player/**
Assets/_Game/Scripts/Combat/**
Assets/_Game/Scripts/Skills/**
Assets/_Game/Scripts/Inventory/**
Assets/_Game/Scripts/Quests/**
Assets/_Game/Scripts/Fonte/**
```

---

## 14. Arquivos proibidos

```text
Packages/**
ProjectSettings/**
Assets/**/*.unity
Assets/**/*.prefab
Assets/**/*.asset
Assets/_Game/Scripts/Pets/**
.specs/SPEC_EXECUTION_ORDER.md
.specs/implementados/**
docs/refinements/implementados/**
docs/project/CURRENT_STATE.md
PROJECT_LOG.md
```

---

## 15. Estratégia

```text
1. Auditar HUD/hotbar/notifications/debug systems.
2. Consolidar view models and queue policy.
3. Implementar validators for active slots/no Breath/debug separation.
4. Criar tests.
5. Criar report.
```

---

## 16. Paralelização

- Parallelizable: YES.
- Can run with:
  - input focus spec if no same files;
  - inventory tooltip spec if no same files.
- Must not run with:
  - skill tree runtime;
  - combat HUD visual implementation;
  - prefab/layout final.
- Reason: projection layer is relatively isolated.

---

## 17. Impacto save/load

```text
Does this change save schema? NO.
Does this add save section? NO.
Does this require migration? NO.
Does this persist Unity references? NO.
```

---

## 18. Impacto eventos

```text
Adds events: NO by default; consumes state projections only.
Changes events: NO.
Requires unsubscribe pattern: YES if reactive subscribers are created.
```

---

## 19. Impacto UI/Unity

```text
Changes UI data layer: YES.
Changes prefabs/layout: NO.
Changes scenes/prefabs/assets: NO.
Requires PlayMode/final human scenario: YES, DEFERRED for HUD/notification visual validation.
```

---

## 20. Riscos

```text
Risco: projection needs missing gameplay state.
Mitigação: use adapters/placeholders and report.

Risco: pet/companion field implies runtime.
Mitigação: optional/future only.

Risco: notification spam.
Mitigação: queue cap policy.
```

---

# /speckit.tasks

## 21. Tasks

- [ ] T001 — Ler fontes.
- [ ] T002 — Auditar HUD/hotbar/notification/debug systems.
- [ ] T003 — Consolidar ViewModels.
- [ ] T004 — Implementar validators.
- [ ] T005 — Criar tests.
- [ ] T006 — Rodar validações.
- [ ] T007 — Criar report.

## 23A. Execution Readiness Matrix

| Área | Pergunta obrigatória | Evidência esperada | Status se faltar |
|---|---|---|---|
| Fonte canônica | A execução leu Source Map e directions de UI/UX e domínios afetados? | Lista de fontes no execution report. | PARTIAL |
| Estado real do repo | Sistemas existentes relacionados a HUD/hotbar/active slots/notifications/debug foram auditados antes de criar novos? | Comandos `rg` e achados no report. | PARTIAL |
| Não duplicação | Existe sistema equivalente já implementado/parcial? | Decisão REUSE/HARDEN/CREATE/DEFER. | BLOCKED se duplicar |
| UI não é fonte | A UI apenas projeta/comanda, sem virar fonte de verdade? | Contrato ViewModel/Command separado de State. | PARTIAL |
| Input seguro | UI modal bloqueia gameplay input quando aplicável? | Teste/validator de foco. | PARTIAL |
| Anti-spoiler | UI não revela Quest/Main/Fonte/Anya/101/final antes de descoberta? | Visibility/filter tests. | PARTIAL |
| Anti-debug | HUD final não depende de Debug HUD e não mostra ids internos? | Checklist/test. | PARTIAL |
| Pets/social future | A execução não implementou Pets/Social/Romance runtime? | Checklist explícito. | BLOCKED se violar |
| Save/load | Houve alteração de schema? | Declaração explícita NO ou STOP se migration necessária. | BLOCKED se alterar sem migration |
| Eventos | Publishers/subscribers/lifecycle foram mapeados? | Mapa de eventos e unsubscribe policy se houver. | PARTIAL |
| UI/PlayMode | Há fluxo visual ou gameplay integrado? | Cenário final deferido documentado. | BUILD_VALIDATED no máximo se ausente |
| Testes | Há lógica determinística nova? | EditMode test ou NOT RUN justificado. | PARTIAL |
| Report | Execution report foi criado? | `docs/validation/11_spec_ui_hud_hotbar_active_slots_notifications_debug_runtime_execution_report.md`. | PARTIAL |

---

## 23B. Local Audit Checklist

Antes de alterar qualquer arquivo, Claude Code/Codex deve rodar e registrar no execution report:

```bash
rg -n "GameplayHud|HUD|Hotbar|ActiveSlot|Notification|DebugHUD|Breath|Folego|BR|ContextPrompt|SkillSlot" Assets/_Game/Scripts docs/design .specs
rg -n "UIFocus|InputFocus|Modal|HUD|Hotbar|Tooltip|Inventory|Equipment|Shop|Crafting|SkillTree|QuestLog|Fonte|Notification|DebugHUD|Breath|Folego|Pet|Social|Romance|FinalChoice" Assets/_Game/Scripts docs/design .specs
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
Given o sistema base relacionado a HUD/hotbar/active slots/notifications/debug existe ou foi criado de forma mínima
When o fluxo principal desta spec é executado
Then o comportamento segue UI_UX_FULL_GAMEPLAY_DIRECTION
And input, foco, projeções, tooltips, notifications e debug separation permanecem consistentes
And nenhum sistema paralelo é criado
And o execution report registra evidência.
```

### Scenario 2 — Existing partial implementation

```text
Given já existe implementação parcial no repo
When a execução audita o sistema
Then ela escolhe HARDEN_EXISTING em vez de recriar do zero
And registra divergências do direction
And altera apenas o menor conjunto seguro de arquivos.
```

### Scenario 3 — Modal/input safety

```text
Given uma UI modal, diálogo, loja, inventário, crafting, skill tree, quest log, Fonte menu ou system menu está aberta
When o jogador pressiona WASD/ataque/interact/hotbar
Then gameplay action não executa atrás da UI
And input vai para foco/camada correta.
```

### Scenario 4 — Spoiler/protected state

```text
Given item quest/key, Fonte não desbloqueada, final choice não disponível, quest oculta, social/pet future ou debug data
When UI/projection/tooltip/render consulta o estado
Then só informação autorizada aparece
And ações irreversíveis/destrutivas exigem confirmação adequada.
```

### Scenario 5 — Final human validation deferred

```text
Given o fluxo exige inspeção visual de layout, foco, tooltip, modal, HUD, menu, notification ou feedback
When a spec termina tecnicamente
Then o report registra cenário final em docs/validation/FINAL_HUMAN_VALIDATION_BY_WAVE.md
And não pede validação humana imediata por spec.
```

---

## 23D. Edge Cases and Failure Modes

A execução deve cobrir ou registrar risco residual para:

- Breath/Folego/BR shown in final HUD.
- Active slots > 4.
- Dash/Dodge/Block occupy active slot.
- Notifications cover gameplay or block input incorrectly.
- Debug HUD used as final communication.
- Pet/companion state shown without runtime.
- Quest/Fonte notification leaks spoiler.
- HUD projection mutates gameplay state.

---

## 23E. Minimum Execution Report Template

```md
# Execution Report — UI HUD Hotbar Active Slots Notifications Debug Separation Runtime

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
- UI projection not source of truth:
- Modal input blocking:
- Focus stack/layering:
- Anti-spoiler:
- Debug HUD separation:
- No Breath/Folego/BR final HUD:
- No Pet/Social/Romance runtime:
- Final human validation deferred:

## Implementation decision
- REUSE_EXISTING / HARDEN_EXISTING / CREATE_MINIMAL / DEFER
- Justification:

## Files changed
- ...

## Functional evidence
- Happy path:
- Existing partial implementation:
- Modal/input safety:
- Spoiler/protected state:
- Save/load safety:
- Visual/final scenario:

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
4. A implementação exigir reescrever UI/Input/Inventory/Quest/Fonte/Economy canônico existente.
5. A implementação permitir WASD/ataque/interact atrás de UI modal.
6. A implementação tornar UI fonte de verdade de inventory/equipment/shop/quest/Fonte.
7. A implementação mostrar Breath/Fôlego/BR na HUD final.
8. A implementação vazar spoiler de Anya, nível 101, final choice ou quest oculta.
9. A implementação criar Pet/Social/Romance runtime fora de specs futuras dedicadas.
10. Não for possível decidir se sistema existente é canônico ou obsoleto.
```


## 23G. Final HUD Forbidden Fields

```text
Breath
Fôlego
BR
internal formula;
spawn budget;
raw quest flags;
raw save state;
raw coordinates unless debug.
```

## 23H. Active Slot Rule

```text
ActiveSlotCount <= 4.
Dash/Dodge/Block are core movement/combat actions and do not consume active slots.
```


## 25. Validações obrigatórias

Docs:

```powershell
.\tools\docs\validate_docs.ps1
```

Busca local mínima:

```bash
rg -n "UIFocus|InputFocus|Modal|HUD|Hotbar|Tooltip|Inventory|Equipment|Shop|Crafting|SkillTree|QuestLog|Fonte|Notification|DebugHUD|Breath|Folego|Pet|Social|Romance|FinalChoice" Assets/_Game/Scripts docs/design .specs
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
Quando houver cenário visual/gameplay de UI, foco, HUD, tooltip, shop, crafting, skill tree, quest log, Fonte menu ou notification, registrar em execution report e vincular a docs/validation/FINAL_HUMAN_VALIDATION_BY_WAVE.md.
```

---

## 26. Testing Quality Gate

- Changed deterministic logic: YES, HUD projection/notification/validator logic is deterministic.
- Requires EditMode tests: YES for HUD/no forbidden stat/active slot cap/notification/debug tests.
- Requires PlayMode automated or final human scenario: YES, DEFERRED for HUD/notification visual validation.
- Requires regression test: YES if fixing existing HUD/debug/Breath bug; otherwise NO.
- Human validation timing: DEFERRED_TO_FINAL_VALIDATION se houver UI/gameplay integrado; caso contrário NOT REQUIRED.
- Minimum validation evidence for ACCEPTED: docs validation PASS; C# build PASS if C# changed; EditMode tests PASS or NOT RUN justified; no Breath/Folego/BR; active slots capped.

---

## 27. Definition of Done

```text
Spec executada sem alterar arquivos proibidos.
Contratos/data/runtime implementados apenas dentro do escopo.
Execution report criado em docs/validation/11_spec_ui_hud_hotbar_active_slots_notifications_debug_runtime_execution_report.md.
Fonte/direction coverage preservado.
Validações obrigatórias PASS ou NOT RUN com motivo, impacto e mitigação.
Sem promoção indevida para ACCEPTED apenas por compile.
```

---

## 28. Anti-regressão

```text
Não duplicar sistemas canônicos existentes.
Não quebrar save/load.
Não persistir Unity references.
Não usar UI como fonte de verdade.
Não deixar gameplay input passar por modal.
Não mostrar Breath/Fôlego/BR.
Não vazar spoiler oculto cedo.
Não implementar Pets/Social/Romance.
Não pedir human test por spec.
Não executar runtime em massa antes da 01Q ou exceção humana explícita.
Não alterar SPEC_EXECUTION_ORDER.md.
```

---

## 29. Notas para execução posterior

Esta spec deve ser executada por Claude Code/Codex com auditoria local do repo, usando `rg` e validando o estado real dos sistemas já implementados.

Se a auditoria local revelar sistema equivalente já existente, a execução deve mudar para hardening/residual, não recriar do zero.
