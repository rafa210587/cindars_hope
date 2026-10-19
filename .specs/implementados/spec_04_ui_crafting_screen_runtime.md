# SPEC — UI Crafting Screen Runtime

> **Spec ID:** `04_spec_ui_crafting_screen_runtime`  
> **Status:** A implementar  
> **Wave:** WAVE 04 — UI / UX Foundation  
> **Priority:** P0  
> **Type:** Runtime / UI / Crafting / Recipes / Materials  
> **Domain:** UI / Crafting / Recipe Detail / Material Validation / Processing  
> **Parallelizable:** NO  
> **Parallel group:** WAVE_04_UI_LOCKED  
> **Can run with:** N/A  
> **Must not run with:** qualquer spec que altere crafting backend, recipe schema, inventory/material consumption, economy balance, stations/workshops, processing queue or modal focus.  
> **Repo lock scope:** `Assets/_Game/Scripts/UI/**`, `Assets/_Game/Scripts/Crafting/**`, `Assets/_Game/Scripts/Inventory/**`, `Assets/_Game/Tests/EditMode/UI/**`, `docs/validation/04_spec_ui_crafting_screen_runtime_execution_report.md`  
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
  - `docs/design/gameplay/loot_crafting_economy/LOOT_CRAFTING_ECONOMY_DIRECTION.md`
  - `.specs/a_implementar/04_spec_ui_input_focus_modal_routing_runtime.md`
> **Blocks:**  
  - crafting backend;
  - recipe unlock/runtime;
  - processing queue UI;
  - shop/economy recipe purchases;
  - station/workshop runtime.
> **Scope:** consolidar Crafting Screen com lista de receitas, detail drawer, materiais possuídos/necessários, station, tempo, craft one/many e estados de receita.  
> **Out of scope:** recipe backend rewrite, processing queue backend, balance de materiais/tempo, station placement/building, visual prefab final.

---

# /speckit.specify

## 1. Contexto

O direction de menu define Crafting Screen com lista de receitas/categorias, recipe detail drawer, inputs necessários, quantidade disponível, resultado, station, tempo e ações craft one/craft many/pin future.

O direction de loot/crafting/economia define que crafting é conversor entre fazenda, cidade e caverna, exigindo receita, estação e materiais variados sem grind excessivo.

---

## 2. Problema

Sem Crafting UI contract:

```text
material faltante pode não aparecer;
craft em massa pode consumir quantidade errada;
receita locked pode revelar spoiler;
station ausente pode parecer bug;
processing active/ready não tem estado;
UI pode calcular disponibilidade diferente do backend;
recipe detail pode omitir qualidade/tempo/resultado.
```

---

## 3. Objetivo

Criar/endurecer Crafting Screen:

```text
recipe list/category;
KnownCraftable/MissingMaterials/MissingStation/Locked/Hidden states;
detail drawer;
material owned vs required;
station and processing time;
craft one/many with recalculation;
ready/processing display;
backend owns crafting transaction.
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
- docs/design/gameplay/loot_crafting_economy/LOOT_CRAFTING_ECONOMY_DIRECTION.md

### Required interpretation

```text
Esta spec é derivada dos directions/refinements canônicos.
Ela não substitui os directions.
Ela transforma parte do refinement em contrato implementável com escopo, locks, validações e quality gate.
```

---

## Direction / Refinement Coverage

### Covered from directions

- Crafting Screen mostra receitas, requisitos, resultado e estado de craft.
- Recipe states: KnownCraftable, KnownMissingMaterials, KnownMissingStation, KnownLockedBySkill, KnownLockedByQuest, UnknownHidden, ProcessingActive, ReadyToCollect.
- Recipe detail mostra materiais possuídos vs necessários, station, tempo, qualidade prevista e uso principal.
- Material faltante deve ser explícito.
- Craft em massa deve recalcular total.
- UnknownHidden não revela spoiler.
- Crafting deve converter fazenda+caverna+cidade sem substituir esses loops.

### Deferred / future from directions

- Pin recipe future.
- Processing queue backend completo.
- Recipe balance final.
- Station placement/building.
- Crafting skill tree effects final.
- Visual prefab final.

### Explicitly not redefined here

- Recipe data schema.
- Inventory consumption backend.
- Crafting station runtime.
- Economy pricing.
- Skill unlock runtime.

## 4. Estado atual do repo

```text
Crafting queue/workstations/recipes/UI aparecem como completos ou parciais no audit report.
Esta spec deve auditar e harden, não recriar crafting backend.
```

A confirmar localmente:

```text
CraftingManager;
RecipeDefinition;
CraftingRuntime;
CraftingUI;
workstations;
processing queue;
inventory material resolver.
```

---

## 5. User stories

```text
Como jogador, quero ver quais receitas posso craftar e por quê.
Como jogador, quero saber exatamente material faltante.
Como jogador, quero craftar múltiplas unidades sem erro de total.
Como jogador, quero ver quando processing está pronto para coletar.
```

---

## 6. Escopo

```text
crafting screen/view model;
recipe state projection;
material requirement projection;
craft one/many preview;
processing/ready state projection;
locked/hidden visibility;
tests/validators.
```

---

## 7. Fora de escopo

```text
crafting backend rewrite;
recipe balance;
station placement/building;
processing backend;
item creation formulas;
scene/prefab final.
```

---

## 8. Regras de não duplicação

```text
Não criar CraftingManager paralelo.
Não consumir materiais diretamente pela UI.
Não revelar UnknownHidden recipe.
Não calcular craftability de forma divergente do backend.
Não alterar recipe schema sem spec própria.
```

---

## 9. Critérios de aceite

- Recipe states projetados corretamente.
- Material faltante explícito.
- Craft-many recalcula total.
- Locked/hidden não revela spoiler.
- Processing/ready visible.
- Report inclui PlayMode final scenario.

---

# /speckit.plan

## 10. Arquitetura alvo

```text
Assets/_Game/Scripts/UI/Crafting/CraftingScreenController.cs
Assets/_Game/Scripts/UI/Crafting/CraftingRecipeViewModel.cs
Assets/_Game/Scripts/UI/Crafting/CraftingMaterialRequirementViewModel.cs
Assets/_Game/Tests/EditMode/UI/CraftingRecipeViewModelTests.cs
```

Consolidar existentes se houver.

---

## 11. Contratos

### Runtime

```text
UI projects recipe state and dispatches craft command.
Backend owns material validation and consumption.
```

### Save

```text
No save schema change.
Processing state is backend-owned if already persisted.
```

### UI

```text
Material missing and locked reasons are explicit.
```

---

## 12. Arquivos permitidos

```text
Assets/_Game/Scripts/UI/**
Assets/_Game/Tests/EditMode/UI/**
Assets/_Game/Scripts/Editor/Validation/**
docs/validation/04_spec_ui_crafting_screen_runtime_execution_report.md
```

Leitura permitida:

```text
Assets/_Game/Scripts/Crafting/**
Assets/_Game/Scripts/Inventory/**
Assets/_Game/Scripts/Items/**
Assets/_Game/Scripts/Skills/**
```

---

## 13. Arquivos proibidos

```text
Packages/**
ProjectSettings/**
Assets/**/*.unity
Assets/**/*.prefab
Assets/**/*.asset
.specs/SPEC_EXECUTION_ORDER.md
.specs/implementados/**
docs/refinements/implementados/**
docs/project/CURRENT_STATE.md
PROJECT_LOG.md
```

---

## 14. Estratégia

```text
1. Auditar crafting UI/backend.
2. Consolidar recipe state view model.
3. Add material/quantity preview.
4. Add locked/hidden handling.
5. Add tests.
6. Report.
```

---

## 15. Ordem segura

```text
Input focus -> Inventory projection -> Crafting UI -> processing/station future.
```

---

## 16. Paralelização

- Parallelizable: NO
- Must not run with crafting backend, inventory or skill unlock changes.
- Reason: crafting UI depends on shared recipe/material contracts.

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
Changes events: SHOULD BE NO.
Requires unsubscribe pattern: YES if UI subscribers touched.
```

---

## 19. Impacto UI/Unity

```text
Changes UI: YES view model/behavior.
Scenes/prefabs/assets: NO.
Requires PlayMode/final human scenario: YES, DEFERRED.
```

---

## 20. Riscos

```text
Risco: UI e backend divergirem.
Mitigação: backend owns transaction, UI uses resolver.

Risco: spoiler recipe.
Mitigação: Hidden state.
```

---

## 21. Rollback

```text
Reverter crafting UI/view model/tests/report.
```

---

# /speckit.tasks

## 22. Tasks

- [ ] T001 — Ler fontes.
- [ ] T002 — Auditar crafting UI/backend.
- [ ] T003 — Consolidar recipe state/detail view model.
- [ ] T004 — Implementar material/quantity preview.
- [ ] T005 — Criar tests.
- [ ] T006 — Rodar validações.
- [ ] T007 — Criar report.

## 23A. Execution Readiness Matrix

| Área | Pergunta obrigatória | Evidência esperada | Status se faltar |
|---|---|---|---|
| Fonte canônica | A execução leu Source Map e directions do domínio? | Lista de fontes no execution report. | PARTIAL |
| Estado real do repo | Classes/sistemas existentes foram auditados antes de criar novos? | Comandos `rg` e achados no report. | PARTIAL |
| Não duplicação | Existe sistema equivalente já implementado/parcial? | Decisão REUSE/HARDEN/CREATE/DEFER. | BLOCKED se duplicar |
| UI focus | A tela usa focus/modal stack canônico? | Integração com input/modal foundation. | BUILD_VALIDATED no máximo |
| Save/load | A UI altera estado persistido ou só despacha comando? | Declaração de schema/no schema. | PARTIAL |
| Eventos | Subscriptions têm lifecycle? | Mapa de publishers/subscribers se houver. | PARTIAL |
| Testes | Há view model/projection determinística? | EditMode test ou NOT RUN justificado. | PARTIAL |
| Report | Execution report criado? | `docs/validation/04_spec_ui_crafting_screen_runtime_execution_report.md`. | PARTIAL |

---

## 23B. Local Audit Checklist

Antes de alterar qualquer arquivo, Claude Code/Codex deve rodar e registrar:

```bash
rg -n "Crafting|Recipe|Craftable|MissingMaterials|MissingStation|LockedBySkill|LockedByQuest|UnknownHidden|ProcessingActive|ReadyToCollect|CraftMany" Assets/_Game/Scripts docs/design .specs
rg -n "TODO|FIXME|HACK|PARTIAL|DEFERRED|BUILD_VALIDATED|ACCEPTED" .specs docs/validation docs/IMPLEMENTATION_STATUS.md docs/project/CURRENT_STATE.md
```

Classificar achados como:

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
Given a tela/flow de crafting screen/recipe projection é aberto com dependências válidas
When o jogador navega, seleciona elemento e executa ação segura
Then a UI mostra foco claro, detalhe correto e ações disponíveis
And gameplay input fica bloqueado se modal estiver aberto
And o domínio recebe apenas comando/intent válido, não mutação escondida de UI.
```

### Scenario 2 — Empty / unavailable state

```text
Given não há dados disponíveis ou requisito está ausente
When a tela abre ou item/node/recipe/quest é selecionado
Then a UI mostra empty state ou motivo bloqueado
And nenhuma ação inválida é executada
And o report registra o estado esperado.
```

### Scenario 3 — Protected / costly action

```text
Given uma ação é custosa, destrutiva, irreversível ou protegida
When o jogador tenta executá-la
Then confirmação é exigida quando o direction manda
And foco inicial respeita regra de ação destrutiva
And cancel/back não executa ação de mundo.
```

### Scenario 4 — Final human validation deferred

```text
Given o fluxo exige PlayMode ou interação visual integrada
When a spec termina tecnicamente
Then o report registra cenário final
And não pede validação humana imediata por spec
And status respeita SPEC_VALIDATION_MATRIX_MASTER.md.
```

---

## 23D. Edge Cases and Failure Modes

A execução deve cobrir ou registrar risco residual para:

- Craft-many total errado.
- Material missing não destacado.
- Receita oculta revelada.
- Station ausente sem mensagem.
- ProcessingActive sem indicação de tempo/ready.
- UI consumindo material diretamente.
- Skill/quest lock ignorado.
- Recipe detail omitindo resultado/quantidade.

---

## 23E. Minimum Execution Report Template

```md
# Execution Report — UI Crafting Screen Runtime

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
- Empty/unavailable:
- Protected/costly action:
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

```text
1. A implementação exigir Packages/ ou ProjectSettings/.
2. A implementação exigir scene/prefab/asset wiring fora do escopo.
3. A implementação exigir mudança de save schema.
4. A implementação exigir reescrever sistema canônico existente.
5. A implementação conflitar com input focus/modal foundation.
6. Não for possível decidir se sistema existente é canônico ou obsoleto.
```


## 23G. Recipe State Matrix

| State | Visible | Action | Reason display |
|---|---:|---|---|
| KnownCraftable | YES | CraftOne/CraftMany | N/A |
| KnownMissingMaterials | YES | Disabled | Missing materials list |
| KnownMissingStation | YES | Disabled | Required station |
| KnownLockedBySkill | YES | Disabled | Known skill requirement |
| KnownLockedByQuest | YES | Disabled | Known quest/progress requirement |
| UnknownHidden | NO or ??? | Disabled | No spoiler |
| ProcessingActive | YES | View status | Time/progress |
| ReadyToCollect | YES | Collect | Output ready |

## 23H. Craft-many Invariants

```text
Total materials = quantity * recipe cost.
Quantity cannot exceed available materials/station capacity.
Changing quantity updates preview immediately.
Backend validates again on command.
```


## 25. Validações obrigatórias

Docs:

```powershell
.\tools\docs\validate_docs.ps1
```

Busca local mínima:

```bash
rg -n "Storage|Chest|Crafting|Recipe|SkillTree|SkillPoint|ActiveSlot|QuestLog|QuestDetail|Modal|Focus|Confirm" Assets/_Game/Scripts docs/design .specs
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

- Changed deterministic logic: YES if recipe projection/craft-many logic changes.
- Requires EditMode tests: YES for recipe state/material/quantity projection tests.
- Requires PlayMode automated or final human scenario: YES, DEFERRED for crafting station open/craft/ready flow.
- Requires regression test: YES if fixing known material/spoiler/craft-many bug; otherwise NO.
- Human validation timing: DEFERRED_TO_FINAL_VALIDATION se houver cenário integrado; caso contrário NOT REQUIRED.
- Minimum validation evidence for ACCEPTED: docs validation PASS; C# build PASS if C# changed; EditMode tests PASS or NOT RUN justified; no recipe schema change; no hidden recipe spoiler.

---

## 27. Definition of Done

```text
Spec executada sem alterar arquivos proibidos.
Contratos/data/runtime implementados apenas dentro do escopo.
Execution report criado em docs/validation/04_spec_ui_crafting_screen_runtime_execution_report.md.
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
