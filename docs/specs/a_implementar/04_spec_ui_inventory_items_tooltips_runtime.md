# SPEC — UI Inventory Items Tooltips Runtime

> **Spec ID:** `04_spec_ui_inventory_items_tooltips_runtime`  
> **Status:** A implementar  
> **Revision:** EXPANDED_06_07  
> **Wave:** WAVE 04 — UI / UX Foundation  
> **Priority:** P0  
> **Type:** Runtime / UI / Inventory / Tooltips / Item Detail  
> **Domain:** UI / Inventory / Item Detail / Tooltip / Anti-error  
> **Parallelizable:** NO  
> **Parallel group:** WAVE_04_UI_LOCKED  
> **Can run with:** N/A  
> **Must not run with:** qualquer spec que altere inventory data model, equipment compare, shop sell, storage, item usage, hotbar or modal focus.  
> **Repo lock scope:** `Assets/_Game/Scripts/UI/**`, `Assets/_Game/Scripts/Inventory/**`, `Assets/_Game/Scripts/Items/**`, `Assets/_Game/Tests/EditMode/UI/**`, `docs/validation/04_spec_ui_inventory_items_tooltips_runtime_execution_report.md`  
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
  - `docs/design/gameplay/equipment/EQUIPMENT_WEAPONS_ARMOR_MATERIALS_DIRECTION.md`
  - `docs/specs/a_implementar/04_spec_ui_input_focus_modal_routing_runtime.md`
> **Blocks:**  
  - equipment compare;
  - shop sell flow;
  - storage/chest UI;
  - hotbar UI;
  - crafting material selection.
> **Scope:** consolidar Inventory screen, item detail drawer e tooltip padrão, sem reescrever inventory backend.  
> **Out of scope:** storage/chest full screen, equipment equip flow, shop sell transaction, crafting UI, final art/layout/prefabs.

---

# /speckit.specify

## 1. Contexto

O direction de UI/UX define inventário com ItemStack/ItemInstance, quantidade, qualidade, raridade, tag/categoria, favorito/bloqueado futuro, comparação para equipáveis, uso rápido, drop/split/move/sell quando permitido.

O menu flow detalha categorias, slot states, item detail drawer e ações possíveis.

---

## 2. Problema

Sem inventory UI contract:

```text
item quest/key pode ser vendido ou descartado;
tooltip pode esconder informação crítica;
equipável pode ser trocado sem comparação;
stack split pode ser ambíguo;
item não usável aqui pode parecer bug;
sell context pode usar lista errada.
```

---

## 3. Objetivo

Criar/endurecer Inventory UI:

```text
grid com slot states;
detail drawer;
tooltip curto;
ações permitidas conforme contexto;
anti-error para quest/key/equipped/favorite/rare;
separação entre inventory normal e sell context.
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
- docs/design/gameplay/equipment/EQUIPMENT_WEAPONS_ARMOR_MATERIALS_DIRECTION.md

### Required interpretation

```text
Esta spec é derivada dos directions/refinements canônicos.
Ela não substitui os directions.
Ela transforma parte do refinement em contrato implementável com escopo, locks, validações e quality gate.
```

---

## Direction / Refinement Coverage

### Covered from directions

- Inventory deve suportar ItemStack, ItemInstance, quantidade, qualidade, raridade, categoria, favorito/bloqueado futuro.
- Categorias iniciais: All, Tools, Weapons, Armor, Consumables, Materials, Crops/Food, Quest/Key, Magic, Pet/Companion.
- Slot states: Empty, Occupied, Selected, Equipped, Quest/Key protected, Stackable, Not usable here, Not sellable, New item.
- Item detail drawer mostra nome, ícone, categoria, quantidade, quality, rarity, valor/preço contextual, uso, tags e ações.
- Quest/Key item não pode ser vendido ou descartado por padrão.

### Deferred / future from directions

- Storage/chest full UI.
- Lock/Favorite final.
- Full drag-and-drop polished interactions.
- Gamepad final.
- Visual art final.

### Explicitly not redefined here

- Inventory backend.
- Item data schema.
- Equipment stat formulas.
- Shop transaction logic.
- Crafting recipes.

## 4. Estado atual do repo

```text
Audit report indica inventory slots/capacity/UI final como completo ou parcial.
Esta spec deve auditar e harden UI, não reescrever InventoryManager.
```

A confirmar localmente:

```text
InventoryManager;
InventoryPanel/HUD;
ItemDataSO/ItemInstance;
TooltipController;
Item actions;
Quest/key protection.
```

---

## 5. User stories

```text
Como jogador, quero entender item selecionado sem abrir tela extra.
Como jogador, não quero vender/descartar item de quest por acidente.
Como jogador, quero saber por que item não pode ser usado aqui.
Como shop sell, quero reutilizar inventory sell projection sem confundir com shop stock.
```

---

## 6. Escopo

```text
inventory screen/view model;
item detail drawer;
tooltip standard;
slot states;
action availability;
anti-error protections;
tests/validators.
```

---

## 7. Fora de escopo

```text
backend inventory rewrite;
storage/chest final;
equipment transaction;
shop transaction;
crafting UI;
prefab final.
```

---

## 8. Regras de não duplicação

```text
Não criar InventoryManager paralelo.
Não persistir UI state como inventory data.
Não vender item via inventory UI sem shop/economy transaction.
Não revelar unknown bestiary/equipment interactions.
```

---

## 9. Critérios de aceite

- Slot states claros.
- Detail drawer completo.
- Tooltip não substitui detail longo.
- Actions respeitam contexto.
- Quest/key/equipped/favorite protections.
- Report lista final PlayMode scenarios.

---

# /speckit.plan

## 10. Arquitetura alvo

```text
Assets/_Game/Scripts/UI/Inventory/InventoryScreenController.cs
Assets/_Game/Scripts/UI/Inventory/InventoryItemViewModel.cs
Assets/_Game/Scripts/UI/Inventory/ItemDetailDrawerViewModel.cs
Assets/_Game/Scripts/UI/Tooltips/StandardTooltipController.cs
Assets/_Game/Tests/EditMode/UI/InventoryItemActionTests.cs
```

---

## 11. Contratos

### Data

```text
UI consumes item IDs/instances from inventory.
UI action availability is derived, not authoritative backend.
```

### Runtime

```text
InventoryFocus blocks gameplay input.
Actions dispatch commands to inventory/economy/equipment systems.
```

### Save

```text
No save schema change.
```

### UI

```text
Detail drawer updates with focus.
Tooltip is short.
```

---

## 12. Arquivos permitidos

```text
Assets/_Game/Scripts/UI/**
Assets/_Game/Tests/EditMode/UI/**
Assets/_Game/Scripts/Editor/Validation/**
docs/validation/04_spec_ui_inventory_items_tooltips_runtime_execution_report.md
```

Leitura permitida:

```text
Assets/_Game/Scripts/Inventory/**
Assets/_Game/Scripts/Items/**
Assets/_Game/Scripts/Equipment/**
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

## 14. Estratégia

```text
1. Auditar inventory UI/backend existentes.
2. Consolidar view models/actions.
3. Add protections in UI action availability.
4. Add deterministic tests.
5. Report PlayMode scenarios.
```

---

## 15. Ordem segura

```text
Input focus -> Inventory UI -> Equipment compare / Shop sell / Storage.
```

---

## 16. Paralelização

- Parallelizable: NO
- Must not run with equipment/shop/storage UI.
- Reason: shared inventory projection/actions.

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
Adds events: SHOULD BE NO.
Changes existing events: SHOULD BE NO.
Requires unsubscribe pattern: YES if UI subscribers touched.
```

---

## 19. Impacto UI/Unity

```text
Changes UI: YES behavior/view model.
Changes scenes/prefabs/assets: NO.
Requires PlayMode/final human scenario: YES, DEFERRED.
```

---

## 20. Riscos

```text
Risco: UI bloquear item válido.
Mitigação: action availability tests.

Risco: item quest/key vendável.
Mitigação: protection rules.
```

---

## 21. Rollback

```text
Reverter UI/view model/tests/report.
```

---

# /speckit.tasks

## 22. Tasks

- [ ] T001 — Ler fontes.
- [ ] T002 — Auditar inventory UI/backend.
- [ ] T003 — Consolidar slot states/detail drawer.
- [ ] T004 — Implementar action availability.
- [ ] T005 — Criar tests.
- [ ] T006 — Rodar validações.
- [ ] T007 — Criar report.

---

## 23A. Execution Readiness Matrix

| Área | Pergunta obrigatória | Evidência esperada | Status se faltar |
|---|---|---|---|
| Fonte canônica | A execução leu os directions e refinements listados em Source Map Compliance? | Lista de fontes no execution report. | PARTIAL |
| Estado real do repo | A execução auditou classes existentes antes de criar novas? | Comandos `rg` e achados principais no report. | PARTIAL |
| Não duplicação | Existe sistema equivalente já implementado/parcial? | Decisão: reuse/harden/create. | BLOCKED se criar duplicata |
| Save/load | A spec altera ou depende de estado persistido? | Declaração explícita de schema/no schema. | PARTIAL |
| Eventos | A spec cria/usa eventos ou subscriptions? | Mapa de publishers/subscribers e unsubscribe policy. | PARTIAL |
| UI/Input | Há foco/modal/PlayMode relevante? | Cenário final deferido documentado. | BUILD_VALIDATED no máximo |
| Testes | Há lógica determinística nova? | EditMode test ou justificativa NOT RUN. | PARTIAL |
| Report | Execution report foi criado? | `docs/validation/04_spec_ui_inventory_items_tooltips_runtime_execution_report.md`. | PARTIAL |

---

## 23B. Local Audit Checklist

Antes de alterar qualquer arquivo, Claude Code/Codex deve rodar e registrar no execution report:

```bash
rg -n "Inventory|ItemStack|ItemInstance|ItemDetail|Tooltip|CanSell|CanDiscard|QuestItem|KeyItem|Favorite|Equipped|UseItem|SplitStack" Assets/_Game/Scripts docs/design docs/specs
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
Given o sistema base relacionado a inventory item/detail/tooltip UI existe ou foi criado de forma mínima
When o usuário/sistema executa o fluxo principal desta spec
Then o estado visível/resultado segue o direction canônico
And nenhum sistema paralelo é criado
And o execution report registra evidência.
```

### Scenario 2 — Missing dependency

```text
Given uma dependência runtime não existe no repo local
When a execução encontra essa ausência
Then ela não inventa uma solução massiva
And marca como MISSING_BUT_DEFER ou cria apenas adapter/validator mínimo se seguro
And registra risco residual.
```

### Scenario 3 — Save/load safety

```text
Given o fluxo envolve estado persistido direta ou indiretamente
When save/load ocorre após a ação
Then nenhum dado derivado de UI/debug substitui a fonte de verdade
And nenhum UnityEngine.Object é persistido
And schema change exige spec/migration separada.
```

### Scenario 4 — Final human validation deferred

```text
Given o fluxo exige interação visual ou PlayMode integrado
When a spec é concluída tecnicamente
Then o report registra cenário final em vez de pedir validação humana imediata
And o status máximo respeita SPEC_VALIDATION_MATRIX_MASTER.md.
```

---

## 23D. Edge Cases and Failure Modes

A execução deve cobrir ou registrar risco residual para:

- Quest/Key item vendável/descartável por UI.
- Equipped item vendido sem unequip/confirmation.
- Tooltip omitindo contexto de uso proibido.
- Stack split permitindo quantidade zero/negativa/acima do stack.
- UI calculando preço em vez de chamar economy service.
- ItemInstance mutable exibido como ItemDefinition genérico e perdendo durabilidade/qualidade.
- Inventory UI gravando estado persistido indevido.
- Tooltip revelando bestiary/quest spoiler.

---

## 23E. Minimum Execution Report Template

O execution report desta spec deve conter, no mínimo:

```md
# Execution Report — UI Inventory Items Tooltips Runtime

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

## Implementation decision
- REUSE_EXISTING / HARDEN_EXISTING / CREATE_MINIMAL / DEFER
- Justification:

## Files changed
- ...

## Functional evidence
- Happy path:
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
6. Não for possível decidir se sistema existente é canônico ou obsoleto.
```


## 23G. Inventory Slot State Matrix

| State | Visual esperado | Ações permitidas |
|---|---|---|
| Empty | Slot vazio | Nenhuma. |
| Occupied | Ícone + quantidade | Select/detail. |
| Selected | Highlight | Ações contextuais. |
| Equipped | Badge equipado | Unequip/compare, não vender sem política. |
| Quest/Key Protected | Lock/quest marker | Não vender/descartar. |
| Stackable | Quantidade/split | Split/move quando contexto permitir. |
| Not usable here | Disabled action | Explicar motivo. |
| Not sellable | Disabled sell | Explicar motivo/categoria. |
| New item | New marker | Remove marker após view. |
| Favorite/Locked future | Lock/fav marker | Proteger venda/descarte future. |

## 23H. Item Detail Minimum Fields

```text
ItemId
DisplayName
Icon
Category
Quantity
Quality
Rarity
Stack/Instance marker
Contextual value/price if applicable
CanUse/CanEquip/CanSell/CanDiscard
Reason when action disabled
Tags
Durability/charges if applicable
Quest/key/favorite/equipped protection
```

## 25. Validações obrigatórias

Docs:

```powershell
.\tools\docs\validate_docs.ps1
```

Busca local mínima:

```bash
rg -n "Dialogue|Inventory|Equipment|Tooltip|Shop|Buy|Sell|Modal|Focus|Confirm|ItemDetail|Compare|Stock|Price" Assets/_Game/Scripts docs/design docs/specs
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

- Changed deterministic logic: YES if action availability/view model logic changes.
- Requires EditMode tests: YES for action availability and slot/detail projection logic.
- Requires PlayMode automated or final human scenario: YES, DEFERRED for inventory open/focus/item action scenario.
- Requires regression test: YES if fixing known sell/drop/quest item bug; otherwise NO.
- Human validation timing: DEFERRED_TO_FINAL_VALIDATION se houver cenário integrado; caso contrário NOT REQUIRED.
- Minimum validation evidence for ACCEPTED: docs validation PASS; C# build PASS if C# changed; EditMode tests PASS or NOT RUN justified; no InventoryManager rewrite; protections enforced.

---

## 27. Definition of Done

```text
Spec executada sem alterar arquivos proibidos.
Contratos/data/runtime implementados apenas dentro do escopo.
Execution report criado em docs/validation/04_spec_ui_inventory_items_tooltips_runtime_execution_report.md.
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
Não pedir human test por spec.
Não executar runtime em massa antes da 01Q ou exceção humana explícita.
Não alterar SPEC_EXECUTION_ORDER.md.
Não alterar scenes/prefabs/assets nesta spec.
```

---

## 29. Notas para execução posterior

Esta spec deve ser executada por Claude Code/Codex com auditoria local do repo, usando `rg` e validando o estado real dos sistemas já implementados.

Se a auditoria local revelar sistema equivalente já existente, a execução deve mudar para hardening/residual, não recriar do zero.
