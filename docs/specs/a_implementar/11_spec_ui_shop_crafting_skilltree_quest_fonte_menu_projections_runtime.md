# SPEC — UI Shop Crafting SkillTree Quest Fonte Menu Projections Runtime

> **Spec ID:** `11_spec_ui_shop_crafting_skilltree_quest_fonte_menu_projections_runtime`  
> **Status:** A implementar  
> **Wave:** WAVE 11 — UI / UX / Input / Menus  
> **Priority:** P0  
> **Type:** Runtime / UI Projection / Shop / Crafting / SkillTree / QuestLog / Fonte  
> **Domain:** UI / Menu Projections / Shop / Crafting / SkillTree / Quest / Fonte  
> **Parallelizable:** NO  
> **Parallel group:** WAVE_11_UI_UX_RUNTIME  
> **Can run with:** N/A  
> **Must not run with:** qualquer spec que altere shop/crafting/skill tree/quest/Fonte domain state, transaction runtime, reward runtime, UI prefabs/layout, save schema ou scene wiring.  
> **Repo lock scope:** `Assets/_Game/Scripts/UI/Shops/**`, `Assets/_Game/Scripts/UI/Crafting/**`, `Assets/_Game/Scripts/UI/Skills/**`, `Assets/_Game/Scripts/UI/Quests/**`, `Assets/_Game/Scripts/UI/Fonte/**`, `Assets/_Game/Tests/EditMode/UI/**`, `docs/validation/11_spec_ui_shop_crafting_skilltree_quest_fonte_menu_projections_runtime_execution_report.md`  
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
  - `docs/design/gameplay/loot_crafting_economy/LOOT_CRAFTING_ECONOMY_DIRECTION.md`
  - `docs/design/gameplay/loot_crafting_economy/ECONOMY_PRICING_STOCK_REFRESH_DIRECTION.md`
  - `docs/design/gameplay/quests/QUEST_OBJECTIVE_EVENT_SYSTEM_DIRECTION.md`
  - `docs/design/gameplay/quests/QUESTS_MAIN_PROGRESSION_REFINEMENT_DIRECTION.md`
  - `docs/design/gameplay/farm/FARM_DESIGN_DIRECTION_v1.3.md`
  - `docs/design/gameplay/farm/FARM_LAYOUT_SCALE_BUILDINGS_DIRECTION.md`
  - `docs/design/gameplay/city/CITY_DESIGN_DIRECTION_v1.2.md`
  - `docs/design/gameplay/city/CITY_NPC_ROSTER_SERVICES_DIRECTION_v1.1.md`
  - `docs/design/gameplay/cave/CAVE_DESIGN_DIRECTION.md`
  - `docs/design/gameplay/magic/MAGIC_SPELLS_ACTIONS_DIRECTION.md`
  - `docs/design/gameplay/magic/MAGIC_LEARNING_UNLOCKS_SOURCES_DIRECTION.md`
  - `docs/design/gameplay/save_load/SAVE_LOAD_FULL_STATE_DIRECTION.md`
> **Blocks:**  
  - shop buy/sell runtime;
  - crafting UI;
  - skill tree UI;
  - quest log UI;
  - Fonte menu UI;
  - final human validation checklist.
> **Scope:** definir/endurecer ViewModels/projections e command contracts para shop, crafting, skill tree, quest log e Fonte menus sem alterar domínio nem criar prefabs.  
> **Out of scope:** domain transaction execution, visual layout/prefab, final text/localization, save migration, skill/crafting/shop backend.

---

# /speckit.specify

## 1. Contexto

O direction lista fluxos prioritários: shop buy/sell, crafting recipes, skill tree active slots/respec, quest log, calendar/weather/lunar, Fonte progression. Estas UIs devem mostrar dados do domínio, controlar foco e enviar commands seguros, sem serem fonte de verdade.

Esta spec cobre projections/commands mínimos para menus complexos, sem implementar visual final.

---

## 2. Problema

Sem projections:

```text
shop buy/sell mistura inventário da loja e do jogador;
shop vazio parece bug;
crafting não indica material faltando;
processamento não mostra timer/coleta;
skill tree gasta ponto sem clareza;
respec aparece antes da Fonte/Fragmento da Memória;
quest log mostra spoilers;
Fonte UI mostra decisão final cedo;
UI aplica transação diretamente sem domínio.
```

---

## 3. Objetivo

Criar/endurecer:

```text
ShopMenuViewModel;
ShopBuySellProjection;
CraftingMenuViewModel;
RecipeProjection;
SkillTreeMenuViewModel;
QuestLogMenuViewModel reference/projection boundary;
FonteMenuViewModel;
MenuCommand contracts;
EmptyState policies;
Anti-spoiler/menu validation.
```

---

## 4. Regras de design

```text
Shop Buy mostra estoque da loja.
Shop Sell mostra inventário vendável do jogador.
Empty state explícito.
Crafting mostra receitas conhecidas/bloqueadas, inputs, station, tempo, resultado e quality prevista.
Skill tree mostra 5 árvores iniciais, pontos, nodes, prereqs, ranks, capstones, active/passive, active slots, respec pela Fonte quando desbloqueado.
Quest Log mostra ato/fragmento/Fonte sem spoilers futuros.
Fonte UI só mostra funções desbloqueadas.
```

---

## 5. User stories / engineering stories

```text
Como jogador, quero comprar/vender vendo inventários corretos.
Como crafting, quero saber por que receita está bloqueada.
Como skill tree, quero gastar ponto com feedback claro e respec gate.
Como quest log, quero ver objetivo conhecido sem spoiler.
Como Fonte, quero ver só funções desbloqueadas.
```

---

## 6. Escopo

Inclui:

```text
menu view models;
shop buy/sell projections;
crafting recipe projections;
skill tree projections;
QuestLog projection references;
Fonte menu projection;
empty states;
command contracts;
tests/validators.
```

Não inclui:

```text
domain transaction execution;
UI prefab/layout final;
final localization;
skill/crafting/shop backend logic;
save schema.
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

- docs/design/gameplay/ui_ux/UI_UX_FULL_GAMEPLAY_DIRECTION.md
- docs/design/gameplay/ui_ux/UI_UX_MENU_SCREEN_FLOWS_DIRECTION.md
- docs/design/gameplay/loot_crafting_economy/LOOT_CRAFTING_ECONOMY_DIRECTION.md
- docs/design/gameplay/loot_crafting_economy/ECONOMY_PRICING_STOCK_REFRESH_DIRECTION.md
- docs/design/gameplay/quests/QUEST_OBJECTIVE_EVENT_SYSTEM_DIRECTION.md
- docs/design/gameplay/quests/QUESTS_MAIN_PROGRESSION_REFINEMENT_DIRECTION.md
- docs/design/gameplay/farm/FARM_DESIGN_DIRECTION_v1.3.md
- docs/design/gameplay/farm/FARM_LAYOUT_SCALE_BUILDINGS_DIRECTION.md
- docs/design/gameplay/city/CITY_DESIGN_DIRECTION_v1.2.md
- docs/design/gameplay/city/CITY_NPC_ROSTER_SERVICES_DIRECTION_v1.1.md
- docs/design/gameplay/cave/CAVE_DESIGN_DIRECTION.md
- docs/design/gameplay/magic/MAGIC_SPELLS_ACTIONS_DIRECTION.md
- docs/design/gameplay/magic/MAGIC_LEARNING_UNLOCKS_SOURCES_DIRECTION.md
- docs/design/gameplay/save_load/SAVE_LOAD_FULL_STATE_DIRECTION.md

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

- Shop UI separa Buy, Sell, Shop Inventory, Player Inventory, Gold, unit price, quantity, total and stock.
- Buy mostra estoque da loja; Sell mostra inventário vendável do jogador; itens não vendáveis aparecem bloqueados ou ocultos conforme spec final.
- Empty states: loja sem itens hoje, jogador sem itens vendáveis, vendedor não compra tipo, estoque esgotado até restock.
- Crafting UI mostra receitas conhecidas, bloqueadas, inputs, quantidade disponível, station, tempo, resultado, quality prevista, energia/custo.
- Skill Tree UI mostra 5 árvores iniciais, SkillPoints, nodes, prereqs, ranks, capstones, active/passive, active slots, respec pela Fonte.
- Quest Log mostra Main/Side/Social/Farm Order/Companion/Pet-Farm Hint/Completed/Failed e main quest com ato, fragmento, Fonte sem spoiler.
- Fonte UI mostra só funções desbloqueadas e não mostra respec, purificação, decisão final, Anya completa ou nível 101 cedo.

### Deferred / future from directions

- Prefab/layout final.
- Domain transaction runtime.
- Full QuestLog UI implementation.
- Calendar full UI.
- Social/Pet runtime.
- Localization final.
- Save migration.

### Explicitly not redefined here

- Shop stock/pricing services.
- Crafting recipe service.
- Skill tree backend.
- Quest log projection service.
- FonteAnyaSection.
- Input focus modal routing.

## 7. Modelo de domínio

### 7.1 ShopMenuViewModel

```text
ShopId
ShopName
Mode: Buy | Sell
PlayerGold
ShopInventoryRows[]
PlayerSellableRows[]
SelectedItemDetails
QuantitySelector
UnitPrice
TotalPrice
StockState
EmptyStateMessage
CanConfirm
BlockedReason
```

### 7.2 CraftingMenuViewModel

```text
StationId
StationName
KnownRecipes[]
BlockedRecipes[]
SelectedRecipe
RequiredInputs[]
AvailableAmounts[]
MissingInputs[]
RequiredStation
RequiredTime
ExpectedOutput
PredictedQuality
CanCraft
CanCraftBulk
BlockedReason
ProcessingJobs[]
```

### 7.3 SkillTreeMenuViewModel

```text
AvailableSkillPoints
TreeTabs[5]
SelectedNode
NodeStates[]
PrerequisiteSummary
RankInfo
CapstoneInfo
ActiveSlotSummary
RespecAvailable
RespecCost
RespecBlockedReason
CanConfirmSpend
```

### 7.4 QuestLogMenuReference

```text
Uses QuestLogProjectionService from 09_spec_quest_log_visibility_spoiler_projection_runtime.
Must not reimplement projection rules.
Can add menu tab/selection state only.
```

### 7.5 FonteMenuViewModel

```text
FonteState
VisualStage
UnlockedFunctions[]
LivingWaterProjection
RespecProjection
PurificationProjection
FinalChoiceProjection
HiddenFunctionsPlaceholder
Warnings[]
CanUseSelectedFunction
BlockedReason
RequiresConfirmation
```

### 7.6 MenuCommand

```text
CommandId
MenuType
CommandType
TargetId
Quantity optional
RequiresConfirmation
PreviewOnly
DomainServiceTarget
ValidationResult
```

---

## 8. Shop rules

```text
Buy mode:
  display shop inventory and stock quantities.

Sell mode:
  display player sellable inventory.

Non-sellable:
  show locked or hide according to policy.

Limited/Unique stock:
  visually clear.

Empty state:
  explicit, never blank bug-like screen.

Confirm:
  creates command to domain transaction service, not UI-side item/gold mutation.
```

---

## 9. Crafting rules

```text
Craftable recipes distinguished from blocked.
Missing material identifies exact missing item/amount.
Bulk craft shows total cost.
Processing timer shows processing/ready-to-collect state.
Craft command validates recipe/station/input before domain call.
```

---

## 10. Skill tree rules

```text
No spending without visual confirmation/clear feedback.
Locked node explains requirement.
Active skill shows equipped state.
Capstone exclusivity shown.
Respec shows cost/condition/Fonte and hidden until unlocked by Memory fragment.
```

---

## 11. Quest/Fonte rules

```text
Quest menu uses existing spoiler-safe projection.
Fonte menu hides unavailable future functions.
Respec hidden until Memory fragment.
Purification hidden until Life fragment.
Final choice hidden until Hope/final route.
Anya complete explanation hidden.
Level101 hidden until discovered.
```

---

## 12. Criteria

```text
Menu ViewModels exist or are hardened.
Shop Buy/Sell projections separated.
Crafting blocked/missing reasons explicit.
Skill tree respec gate explicit.
QuestLog uses existing projection service.
Fonte menu hides locked functions.
Commands are preview/validation wrappers, not direct domain mutation.
Tests cover shop buy/sell/empty state, crafting missing inputs, skill locked node/respec gate, QuestLog projection reuse and Fonte hidden functions.
```

---

# /speckit.plan

## 13. Arquitetura alvo

```text
Assets/_Game/Scripts/UI/Shops/ShopMenuViewModel.cs
Assets/_Game/Scripts/UI/Shops/ShopBuySellProjectionService.cs
Assets/_Game/Scripts/UI/Crafting/CraftingMenuViewModel.cs
Assets/_Game/Scripts/UI/Crafting/RecipeProjectionViewModel.cs
Assets/_Game/Scripts/UI/Skills/SkillTreeMenuViewModel.cs
Assets/_Game/Scripts/UI/Quests/QuestLogMenuState.cs
Assets/_Game/Scripts/UI/Fonte/FonteMenuViewModel.cs
Assets/_Game/Scripts/UI/Menus/MenuCommand.cs
Assets/_Game/Scripts/UI/Menus/MenuProjectionValidator.cs
Assets/_Game/Tests/EditMode/UI/MenuProjectionTests.cs
```

Consolidar existentes se houver.

---

## 14. Arquivos permitidos

```text
Assets/_Game/Scripts/UI/Shops/**
Assets/_Game/Scripts/UI/Crafting/**
Assets/_Game/Scripts/UI/Skills/**
Assets/_Game/Scripts/UI/Quests/**
Assets/_Game/Scripts/UI/Fonte/**
Assets/_Game/Scripts/UI/Menus/**
Assets/_Game/Scripts/Editor/Validation/**
Assets/_Game/Tests/EditMode/UI/**
docs/validation/11_spec_ui_shop_crafting_skilltree_quest_fonte_menu_projections_runtime_execution_report.md
```

Leitura permitida:

```text
Assets/_Game/Scripts/Shops/**
Assets/_Game/Scripts/Economy/**
Assets/_Game/Scripts/Crafting/**
Assets/_Game/Scripts/Skills/**
Assets/_Game/Scripts/Quests/**
Assets/_Game/Scripts/Fonte/**
Assets/_Game/Scripts/Inventory/**
```

---

## 15. Arquivos proibidos

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

---

## 16. Estratégia

```text
1. Auditar menu projections existentes.
2. Consolidar ViewModels por domínio.
3. Implementar command contracts preview-only.
4. Reusar QuestLogProjectionService.
5. Implementar validators de spoiler/empty states.
6. Criar tests.
7. Criar report.
```

---

## 17. Paralelização

- Parallelizable: NO
- Must not run with:
  - shop/crafting/skill/Fonte/quest domain state changes;
  - UI prefab/layout implementation;
  - transaction runtime.
- Reason: crosses several menu domains.

---

## 18. Impacto save/load

```text
Does this change save schema? NO.
Does this add save section? NO.
Does this require migration? NO.
Does this persist Unity references? NO.
```

---

## 19. Impacto eventos

```text
Adds events: NO by default.
Changes events: NO.
Requires unsubscribe pattern: YES if reactive UI subscribers are created.
```

---

## 20. Impacto UI/Unity

```text
Changes UI data/projection layer: YES.
Changes prefabs/layout: NO.
Changes scenes/prefabs/assets: NO.
Requires PlayMode/final human scenario: YES, DEFERRED for menu flow.
```

---

## 21. Riscos

```text
Risco: projection duplicates domain rules.
Mitigação: call domain services/read-only adapters.

Risco: QuestLog projection reimplemented incorrectly.
Mitigação: reference 09 projection service.

Risco: Fonte/Quest spoilers.
Mitigação: validator tests.
```

---

# /speckit.tasks

## 22. Tasks

- [ ] T001 — Ler fontes.
- [ ] T002 — Auditar menu projection systems.
- [ ] T003 — Consolidar ViewModels.
- [ ] T004 — Implementar command contracts.
- [ ] T005 — Implementar validators/tests.
- [ ] T006 — Rodar validações.
- [ ] T007 — Criar report.

## 23A. Execution Readiness Matrix

| Área | Pergunta obrigatória | Evidência esperada | Status se faltar |
|---|---|---|---|
| Fonte canônica | A execução leu Source Map e directions de UI/UX e domínios afetados? | Lista de fontes no execution report. | PARTIAL |
| Estado real do repo | Sistemas existentes relacionados a shop/crafting/skilltree/quest/fonte menu projections foram auditados antes de criar novos? | Comandos `rg` e achados no report. | PARTIAL |
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
| Report | Execution report foi criado? | `docs/validation/11_spec_ui_shop_crafting_skilltree_quest_fonte_menu_projections_runtime_execution_report.md`. | PARTIAL |

---

## 23B. Local Audit Checklist

Antes de alterar qualquer arquivo, Claude Code/Codex deve rodar e registrar no execution report:

```bash
rg -n "ShopMenu|BuySell|CraftingMenu|RecipeProjection|SkillTreeMenu|QuestLogMenu|FonteMenu|MenuCommand|EmptyState" Assets/_Game/Scripts docs/design docs/specs
rg -n "UIFocus|InputFocus|Modal|HUD|Hotbar|Tooltip|Inventory|Equipment|Shop|Crafting|SkillTree|QuestLog|Fonte|Notification|DebugHUD|Breath|Folego|Pet|Social|Romance|FinalChoice" Assets/_Game/Scripts docs/design docs/specs
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
Given o sistema base relacionado a shop/crafting/skilltree/quest/fonte menu projections existe ou foi criado de forma mínima
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

- Shop Buy/Sell mixed inventories.
- Shop empty state blank.
- Crafting missing material not identified.
- Processing timer not shown.
- Skill point spent without confirmation.
- Respec shown before Memory/Fonte unlock.
- QuestLog projection duplicated and leaks spoiler.
- Fonte final choice shown early.

---

## 23E. Minimum Execution Report Template

```md
# Execution Report — UI Shop Crafting SkillTree Quest Fonte Menu Projections Runtime

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


## 23G. Menu Source-of-Truth Matrix

| Menu | Source service |
|---|---|
| Shop | shop stock + pricing + inventory sellability |
| Crafting | recipe/crafting/processing service |
| SkillTree | skill tree + Fonte respec state |
| QuestLog | QuestLogProjectionService |
| Fonte | FonteAnyaSection projection |
| Commands | domain transaction service, not UI mutation |

## 23H. Empty State Matrix

| Menu | Empty state |
|---|---|
| Shop buy | Esta loja não tem itens disponíveis hoje. |
| Shop sell | Você não possui itens vendáveis. |
| Vendor mismatch | Este vendedor não compra este tipo de item. |
| Stock exhausted | Estoque esgotado até o próximo restock. |
| Crafting | Nenhuma receita conhecida / requisitos faltando. |
| QuestLog | Nenhuma quest conhecida nesta aba. |
```


## 25. Validações obrigatórias

Docs:

```powershell
.\tools\docs\validate_docs.ps1
```

Busca local mínima:

```bash
rg -n "UIFocus|InputFocus|Modal|HUD|Hotbar|Tooltip|Inventory|Equipment|Shop|Crafting|SkillTree|QuestLog|Fonte|Notification|DebugHUD|Breath|Folego|Pet|Social|Romance|FinalChoice" Assets/_Game/Scripts docs/design docs/specs
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

- Changed deterministic logic: YES, menu projection/command/visibility logic is deterministic.
- Requires EditMode tests: YES for shop/crafting/skill/quest/fonte/empty-state/spoiler tests.
- Requires PlayMode automated or final human scenario: YES, DEFERRED for menu visual flow.
- Requires regression test: YES if fixing existing menu projection/spoiler bug; otherwise NO.
- Human validation timing: DEFERRED_TO_FINAL_VALIDATION se houver UI/gameplay integrado; caso contrário NOT REQUIRED.
- Minimum validation evidence for ACCEPTED: docs validation PASS; C# build PASS if C# changed; EditMode tests PASS or NOT RUN justified; menus projection-only and spoiler-safe.

---

## 27. Definition of Done

```text
Spec executada sem alterar arquivos proibidos.
Contratos/data/runtime implementados apenas dentro do escopo.
Execution report criado em docs/validation/11_spec_ui_shop_crafting_skilltree_quest_fonte_menu_projections_runtime_execution_report.md.
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
