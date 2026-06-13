# SPEC — UI Inventory Equipment Tooltips Runtime

> **Spec ID:** `11_spec_ui_inventory_equipment_tooltips_runtime`  
> **Status:** A implementar  
> **Wave:** WAVE 11 — UI / UX / Input / Menus  
> **Priority:** P0  
> **Type:** Runtime / UI Projection / Inventory / Equipment / Tooltips  
> **Domain:** UI / Inventory Projection / Equipment Compare / Tooltip Layers / Protected Actions  
> **Parallelizable:** YES  
> **Parallel group:** WAVE_11_UI_UX_RUNTIME  
> **Can run with:** HUD projection/input focus specs if lock scopes do not overlap.  
> **Must not run with:** qualquer spec que altere inventory backend, equipment mechanics, item definitions, save schema, shop transaction runtime, prefab/layout final ou item database final.  
> **Repo lock scope:** `Assets/_Game/Scripts/UI/Inventory/**`, `Assets/_Game/Scripts/UI/Equipment/**`, `Assets/_Game/Scripts/UI/Tooltips/**`, `Assets/_Game/Scripts/Editor/Validation/**`, `Assets/_Game/Tests/EditMode/UI/**`, `docs/validation/11_spec_ui_inventory_equipment_tooltips_runtime_execution_report.md`  
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
  - `docs/design/gameplay/equipment/EQUIPMENT_WEAPONS_ARMOR_MATERIALS_DIRECTION.md`
  - `docs/design/gameplay/equipment/EQUIPMENT_MECHANICAL_BASELINES_DIRECTION.md`
  - `docs/design/gameplay/loot_crafting_economy/LOOT_CRAFTING_ECONOMY_DIRECTION.md`
  - `docs/design/gameplay/loot_crafting_economy/ECONOMY_PRICING_STOCK_REFRESH_DIRECTION.md`
  - `docs/design/gameplay/quests/QUEST_OBJECTIVE_EVENT_SYSTEM_DIRECTION.md`
  - `docs/design/gameplay/quests/QUESTS_MAIN_PROGRESSION_REFINEMENT_DIRECTION.md`
  - `docs/design/gameplay/save_load/SAVE_LOAD_FULL_STATE_DIRECTION.md`
> **Blocks:**  
  - inventory menu visual UI;
  - equipment compare UI;
  - shop sell UI;
  - crafting ingredient UI;
  - item tooltips.
> **Scope:** definir/endurecer projections de inventory/equipment/tooltips, proteção de quest/key/unique, sell/drop/split/move/use actions e comparação de equipamento sem alterar backend.  
> **Out of scope:** inventory backend rewrite, equipment stat mechanics, final UI layout, item assets/database, save schema migration.

---

# /speckit.specify

## 1. Contexto

Inventory UI deve suportar ItemStack, ItemInstance, quantidade, qualidade, raridade, tag/categoria, favorito/bloqueado contra venda futura, comparação quando equipável, uso rápido, drop/split/move/sell quando permitido. Tooltips têm camadas e equipment UI mostra comparação antes/depois.

Esta spec cria projeções e action guards.

---

## 2. Problema

Sem projections/guards:

```text
quest/key item pode parecer vendável;
Unique pode ser descartado sem confirmação;
equipamento pode trocar por acidente ao comparar;
tooltip pode omitir quality/rarity/durability;
split stack pode ser ambíguo;
drop/sell/use pode bypassar backend rule;
UI pode persistir item reference;
shop sell pode mostrar item errado.
```

---

## 3. Objetivo

Criar/endurecer:

```text
InventoryItemViewModel;
InventoryActionAvailability;
EquipmentSlotViewModel;
EquipmentComparisonViewModel;
ItemTooltipViewModel;
TooltipLayerPolicy;
ProtectedItemActionGuard;
StackSplitRequest;
UI command contracts.
```

---

## 4. Regras de design

```text
Itens vendáveis claramente distintos de não vendáveis.
Quest/key items protegidos contra venda/descarte.
Ações destrutivas exigem confirmação se irreversíveis.
Stack split explícito.
Comparação de equipamento acessível sem troca acidental.
Tooltip mostra nome, categoria, stack, quality, rarity, valor/preço contextual, uso, tags, efeitos, requisitos e avisos.
UI não é fonte de verdade.
```

---

## 5. User stories / engineering stories

```text
Como jogador, quero distinguir item vendável de bloqueado.
Como equipment UI, quero comparar antes/depois sem equipar por acidente.
Como tooltip, quero mostrar dados relevantes por contexto.
Como shop sell, quero filtrar inventário vendável corretamente.
Como protected items, quero confirmação/bloqueio claro.
```

---

## 6. Escopo

Inclui:

```text
inventory item projection;
equipment slot projection;
equipment compare projection;
tooltip view model;
action availability;
protected item action guard;
stack split request contract;
tests/validators.
```

Não inclui:

```text
inventory data model rewrite;
equipment stat calculations;
item database final;
visual prefab layout;
drag/drop implementation final;
save schema.
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
- docs/design/gameplay/equipment/EQUIPMENT_WEAPONS_ARMOR_MATERIALS_DIRECTION.md
- docs/design/gameplay/equipment/EQUIPMENT_MECHANICAL_BASELINES_DIRECTION.md
- docs/design/gameplay/loot_crafting_economy/LOOT_CRAFTING_ECONOMY_DIRECTION.md
- docs/design/gameplay/loot_crafting_economy/ECONOMY_PRICING_STOCK_REFRESH_DIRECTION.md
- docs/design/gameplay/quests/QUEST_OBJECTIVE_EVENT_SYSTEM_DIRECTION.md
- docs/design/gameplay/quests/QUESTS_MAIN_PROGRESSION_REFINEMENT_DIRECTION.md
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

- Inventário deve suportar ItemStack, ItemInstance, quantidade, qualidade, raridade, tag/categoria, favorito/bloqueado contra venda futura, comparação, uso rápido, drop/split/move/sell.
- Itens vendáveis devem ser distinguíveis de itens não vendáveis.
- Quest/key items devem ter proteção contra venda/descarte.
- Stack split precisa ser explícito.
- Comparação de equipamento acessível sem trocar item por acidente.
- Equipment UI mostra arma/ferramenta/armadura/escudo/acessórios/staff/focus/wand, status derivados, resistências, comparação, durabilidade, material/tier/quality/rarity.
- Tooltip padrão mostra nome, categoria, quantidade/stack, quality, rarity, valor/preço contextual, uso, tags, efeitos, requisitos e avisos.

### Deferred / future from directions

- Inventory visual layout.
- Drag/drop final.
- Equipment stat formula.
- Item database.
- Shop transaction runtime.
- Save schema.

### Explicitly not redefined here

- ItemDefinition contract.
- Inventory backend.
- Equipment mechanics.
- Pricing service.
- Shop service.
- Quest protection flags.

## 7. Modelo de domínio

### 7.1 InventoryItemViewModel

```text
SlotId
ItemId
DisplayName
IconId
Quantity
Quality
Rarity
Category
Tags[]
IsStack
IsInstance
IsEquippable
IsSellable
IsDroppable
IsMovable
IsUsable
IsFavorite
IsLocked
IsQuestItem
IsKeyItem
IsUnique
RequiresConfirmationForSell
RequiresConfirmationForDrop
ContextualPrice optional
WarningTextKey optional
```

### 7.2 InventoryActionAvailability

```text
CanUse
CanEquip
CanMove
CanSplit
CanDrop
CanSell
CanFavorite
CanLock
BlockedReason
RequiresConfirmation
ConfirmationTextKey optional
```

### 7.3 EquipmentSlotViewModel

```text
SlotType
EquippedItemId optional
IconId
Durability
Quality
Rarity
MaterialTier
PrimaryStats[]
Resistances[]
WarningState optional
```

### 7.4 EquipmentComparisonViewModel

```text
CurrentItem
CandidateItem
StatDiffs[]
ResistanceDiffs[]
DurabilityDiff
MaterialTierDiff
QualityDiff
RarityDiff
Warnings[]
WouldBreakRequirement
PreviewOnly
```

### 7.5 ItemTooltipViewModel

```text
Name
Category
Quantity
Quality
Rarity
ContextualValue
MainUse
Tags
PrimaryEffects
Requirements
QuestKeyWarnings
EquipmentBlock optional
SkillMagicBlock optional
AdvancedBlock optional
SpoilerSafe
```

---

## 8. Contextual tooltip rules

```text
Normal inventory:
  show base value only if useful; no full price math.

Shop buy/sell:
  show contextual price, unit price, total if quantity.

Equipment:
  add damage/armor/resist/material/tier/durability/scaling/comparison.

Skill/magic:
  add MP/Stamina cost, cooldown, cast time, shape/range, status, condition and unlock source.

Quest/key:
  show protection warning and hide destructive actions.
```

---

## 9. Protected item actions

```text
QuestItem:
  sell=false, drop=false unless authored exception.

KeyItem:
  sell=false, drop=false.

Unique:
  sell/drop false or strong confirmation + recovery policy.

Favorite/Locked:
  block sale/drop until unlocked.

Final/fragment/Fonte/BlackStone special:
  do not sell/drop as commodity.
```

---

## 10. Criteria

```text
Inventory/equipment/tooltip projections exist.
UI action availability uses backend rules, not UI guesses.
Quest/key/unique protections visible and enforced at command layer.
Equipment compare is preview-only.
Tooltip layers exist.
Tests cover sell/drop guards, tooltip fields, contextual price, equipment compare, split request and UI-not-source-of-truth.
```

---

# /speckit.plan

## 11. Arquitetura alvo

```text
Assets/_Game/Scripts/UI/Inventory/InventoryItemViewModel.cs
Assets/_Game/Scripts/UI/Inventory/InventoryActionAvailability.cs
Assets/_Game/Scripts/UI/Inventory/StackSplitRequest.cs
Assets/_Game/Scripts/UI/Equipment/EquipmentSlotViewModel.cs
Assets/_Game/Scripts/UI/Equipment/EquipmentComparisonViewModel.cs
Assets/_Game/Scripts/UI/Tooltips/ItemTooltipViewModel.cs
Assets/_Game/Scripts/UI/Tooltips/TooltipLayerPolicy.cs
Assets/_Game/Scripts/UI/Inventory/ProtectedItemActionGuard.cs
Assets/_Game/Tests/EditMode/UI/InventoryEquipmentTooltipTests.cs
```

Consolidar existentes se houver.

---

## 12. Arquivos permitidos

```text
Assets/_Game/Scripts/UI/Inventory/**
Assets/_Game/Scripts/UI/Equipment/**
Assets/_Game/Scripts/UI/Tooltips/**
Assets/_Game/Scripts/Editor/Validation/**
Assets/_Game/Tests/EditMode/UI/**
docs/validation/11_spec_ui_inventory_equipment_tooltips_runtime_execution_report.md
```

Leitura permitida:

```text
Assets/_Game/Scripts/Inventory/**
Assets/_Game/Scripts/Items/**
Assets/_Game/Scripts/Equipment/**
Assets/_Game/Scripts/Economy/**
Assets/_Game/Scripts/Quests/**
```

---

## 13. Arquivos proibidos

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

## 14. Estratégia

```text
1. Auditar inventory/equipment/tooltip UI systems.
2. Consolidar view models.
3. Implementar protected action guard.
4. Integrar price/compare projections read-only.
5. Criar tests.
6. Criar report.
```

---

## 15. Paralelização

- Parallelizable: YES.
- Can run with:
  - input focus;
  - HUD projection;
- Must not run with:
  - inventory backend rewrite;
  - equipment mechanics;
  - shop sell transaction;
  - item definition schema.
- Reason: UI projection only, but consumes item/inventory/equipment contracts.

---

## 16. Impacto save/load

```text
Does this change save schema? NO.
Does this add save section? NO.
Does this require migration? NO.
Does this persist Unity references? NO.
```

---

## 17. Impacto eventos

```text
Adds events: NO by default.
Changes events: NO.
Requires unsubscribe pattern: YES if reactive UI subscribers are created.
```

---

## 18. Impacto UI/Unity

```text
Changes UI data layer: YES.
Changes prefabs/layout: NO.
Changes scenes/prefabs/assets: NO.
Requires PlayMode/final human scenario: YES, DEFERRED for inventory/equipment visual validation.
```

---

## 19. Riscos

```text
Risco: backend rules missing.
Mitigação: read/harden existing or DEFER.

Risco: UI guard diverges from backend.
Mitigação: action availability must call domain service if exists.

Risco: tooltip leaks hidden lore item info.
Mitigação: spoiler-safe tooltip.
```

---

# /speckit.tasks

## 20. Tasks

- [ ] T001 — Ler fontes.
- [ ] T002 — Auditar inventory/equipment/tooltip UI.
- [ ] T003 — Consolidar ViewModels.
- [ ] T004 — Implementar protected action guard.
- [ ] T005 — Criar tests.
- [ ] T006 — Rodar validações.
- [ ] T007 — Criar report.

## 23A. Execution Readiness Matrix

| Área | Pergunta obrigatória | Evidência esperada | Status se faltar |
|---|---|---|---|
| Fonte canônica | A execução leu Source Map e directions de UI/UX e domínios afetados? | Lista de fontes no execution report. | PARTIAL |
| Estado real do repo | Sistemas existentes relacionados a inventory/equipment/tooltips UI projection foram auditados antes de criar novos? | Comandos `rg` e achados no report. | PARTIAL |
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
| Report | Execution report foi criado? | `docs/validation/11_spec_ui_inventory_equipment_tooltips_runtime_execution_report.md`. | PARTIAL |

---

## 23B. Local Audit Checklist

Antes de alterar qualquer arquivo, Claude Code/Codex deve rodar e registrar no execution report:

```bash
rg -n "InventoryItemViewModel|ItemTooltip|Tooltip|EquipmentComparison|ProtectedItem|QuestItem|KeyItem|Unique|StackSplit|CanSell" Assets/_Game/Scripts docs/design .specs
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
Given o sistema base relacionado a inventory/equipment/tooltips UI projection existe ou foi criado de forma mínima
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

- Quest/key item appears sellable.
- Unique dropped without confirmation/recovery.
- Equipment comparison equips accidentally.
- Tooltip omits quality/rarity/durability.
- Contextual price persisted as source.
- UI guesses backend permission incorrectly.
- Stack split ambiguous.
- Hidden lore/fragment info leaks in tooltip.

---

## 23E. Minimum Execution Report Template

```md
# Execution Report — UI Inventory Equipment Tooltips Runtime

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


## 23G. Tooltip Layer Matrix

| Context | Additional fields |
|---|---|
| Inventory | base identity, quality, rarity, tags |
| Shop | contextual price, buy/sell availability |
| Equipment | stats, durability, comparison |
| Skill/Magic | cost, cooldown, range/shape |
| Quest/Key | protection warning |
| Debug | IDs only in debug tooltip |

## 23H. Protected Action Matrix

| Item kind | Sell | Drop |
|---|---:|---:|
| Normal sellable | yes | yes |
| QuestItem | no | no |
| KeyItem | no | no |
| Unique | no/strong confirm | no/strong confirm |
| Favorite/Locked | no until unlocked | no until unlocked |
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

- Changed deterministic logic: YES, action availability/tooltip/protection logic is deterministic.
- Requires EditMode tests: YES for protection/tooltip/context/equipment comparison/split tests.
- Requires PlayMode automated or final human scenario: YES, DEFERRED for inventory/equipment visual validation.
- Requires regression test: YES if fixing existing item action/tooltip bug; otherwise NO.
- Human validation timing: DEFERRED_TO_FINAL_VALIDATION se houver UI/gameplay integrado; caso contrário NOT REQUIRED.
- Minimum validation evidence for ACCEPTED: docs validation PASS; C# build PASS if C# changed; EditMode tests PASS or NOT RUN justified; protected items not sell/drop.

---

## 27. Definition of Done

```text
Spec executada sem alterar arquivos proibidos.
Contratos/data/runtime implementados apenas dentro do escopo.
Execution report criado em docs/validation/11_spec_ui_inventory_equipment_tooltips_runtime_execution_report.md.
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
