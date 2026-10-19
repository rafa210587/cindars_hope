# SPEC — UI Repair Upgrade Screen Flow Runtime

> **Spec ID:** `04_spec_ui_repair_upgrade_screen_flow_runtime`  
> **Status:** A implementar  
> **Wave:** WAVE 04 — UI / UX Foundation  
> **Priority:** P1  
> **Type:** Runtime / UI / Repair Upgrade / Confirmation / Material Cost  
> **Domain:** UI / Equipment / Repair / Upgrade / Cost Preview / Anti-error  
> **Parallelizable:** NO  
> **Parallel group:** WAVE_04_UI_LOCKED  
> **Can run with:** N/A  
> **Must not run with:** qualquer spec que altere equipment durability backend, upgrade backend, crafting/economy material consumption, shop/service runtime, item instance schema or confirmation patterns.  
> **Repo lock scope:** `Assets/_Game/Scripts/UI/**`, `Assets/_Game/Scripts/Equipment/**`, `Assets/_Game/Scripts/Crafting/**`, `Assets/_Game/Tests/EditMode/UI/**`, `docs/validation/04_spec_ui_repair_upgrade_screen_flow_runtime_execution_report.md`  
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
  - `docs/design/gameplay/loot_crafting_economy/LOOT_CRAFTING_ECONOMY_DIRECTION.md`
  - `.specs/a_implementar/04_spec_ui_equipment_compare_runtime.md`
  - `.specs/a_implementar/04_spec_ui_empty_error_confirmation_patterns_runtime.md`
> **Blocks:**  
  - equipment durability runtime;
  - crafting/material economy;
  - blacksmith/NPC service UI;
  - inventory item detail;
  - confirmation patterns.
> **Scope:** consolidar UI de Repair/Upgrade como ações distintas, com custo/material preview, comparação antes/depois, confirmação forte para upgrade caro e sem implementar backend de upgrade.  
> **Out of scope:** repair/upgrade backend final, balance de custo/material, blacksmith service content, item stat recalculation formulas, prefab final.

---

# /speckit.specify

## 1. Contexto

O direction de UI/UX afirma: Repair e Upgrade são ações diferentes. O direction de confirmação exige confirmação forte para upgrade caro. O direction de equipment/crafting indica que materiais e tiers são parte da identidade, mas tier maior não é sempre melhor.

Esta spec cria o fluxo UI para repair/upgrade sem implementar fórmulas finais.

---

## 2. Problema

Sem repair/upgrade UI contract:

```text
repair e upgrade podem se misturar;
upgrade caro pode ocorrer sem confirmação;
material faltante pode não aparecer;
preview antes/depois pode divergir do backend;
item raro pode ser alterado por acidente;
UI pode consumir material diretamente;
upgrade pode sugerir que tier maior é sempre melhor.
```

---

## 3. Objetivo

Criar/endurecer flow UI:

```text
selecionar item elegível;
separar Repair e Upgrade;
mostrar durability atual/max;
mostrar materiais/gold necessários;
mostrar preview antes/depois;
mostrar risco/tradeoff se existir;
confirmar ações custosas;
backend owns transaction.
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

- Repair e Upgrade são ações diferentes.
- Upgrade caro exige confirmação forte.
- Confirmação deve mostrar ação, alvo, custo, consequência, reversibilidade e Confirm/Cancel.
- Material/tier maior não é sempre melhor em tudo.
- Materiais e tiers abrem identidade, eficiência, resistência ou especialização.
- Crafting/economia controla materiais, custo, produção e tradeoffs.

### Deferred / future from directions

- Repair backend final.
- Upgrade backend final.
- Blacksmith service content.
- Exact balance.
- Station/service NPC wiring.
- Visual prefab final.

### Explicitly not redefined here

- ItemInstance durability backend.
- Material inventory backend.
- Economy price service.
- Equipment formulas.
- Crafting system.

## 4. Estado atual do repo

```text
Durability/repair/upgrade podem estar parciais ou future.
Esta spec deve se adaptar: se backend não existir, criar projection/hook future only.
```

A confirmar localmente:

```text
Durability fields;
RepairService;
UpgradeService;
Blacksmith/shop service;
ItemInstance schema;
Material resolver;
Confirmation modal.
```

---

## 5. User stories

```text
Como jogador, quero saber custo de repair antes de confirmar.
Como jogador, quero ver preview de upgrade antes/depois.
Como jogador, quero evitar upgrade caro por acidente.
Como sistema, quero backend consumindo materiais, não a UI.
```

---

## 6. Escopo

```text
repair/upgrade screen projection;
eligible item list;
cost/material preview;
before/after comparison;
confirmation policy;
blocked states;
tests/validators.
```

---

## 7. Fora de escopo

```text
repair backend;
upgrade backend;
material cost balance;
service NPC content;
equipment formula changes;
prefab final.
```

---

## 8. Regras de não duplicação

```text
Não criar repair backend paralelo.
Não consumir material na UI.
Não misturar repair e upgrade.
Não permitir upgrade sem confirmação quando caro/irreversível.
Não afirmar tier maior como sempre melhor.
```

---

## 9. Critérios de aceite

- Repair and Upgrade separate.
- Cost/material preview.
- Before/after preview.
- Strong confirmation when costly/rare/irreversible.
- Missing materials/gold clear.
- Report includes backend availability decision.

---

# /speckit.plan

## 10. Arquitetura alvo

```text
Assets/_Game/Scripts/UI/Equipment/RepairUpgradeScreenController.cs
Assets/_Game/Scripts/UI/Equipment/RepairUpgradeViewModel.cs
Assets/_Game/Scripts/UI/Equipment/RepairUpgradeCostViewModel.cs
Assets/_Game/Tests/EditMode/UI/RepairUpgradeViewModelTests.cs
```

Consolidar existentes se houver.

---

## 11. Contratos

### Runtime

```text
UI previews cost and result.
Backend owns repair/upgrade transaction.
```

### Save

```text
No save schema change.
```

### UI

```text
Repair and Upgrade tabs/actions are distinct.
```

---

## 12. Arquivos permitidos

```text
Assets/_Game/Scripts/UI/**
Assets/_Game/Tests/EditMode/UI/**
Assets/_Game/Scripts/Editor/Validation/**
docs/validation/04_spec_ui_repair_upgrade_screen_flow_runtime_execution_report.md
```

Leitura permitida:

```text
Assets/_Game/Scripts/Equipment/**
Assets/_Game/Scripts/Crafting/**
Assets/_Game/Scripts/Economy/**
Assets/_Game/Scripts/Inventory/**
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
1. Auditar repair/upgrade backend/UI.
2. Se backend não existir, criar projection/future hook only.
3. Consolidar cost/preview view model.
4. Add confirmation tests.
5. Report.
```

---

## 15. Ordem segura

```text
Equipment detail -> common confirmation patterns -> repair/upgrade UI.
```

---

## 16. Paralelização

- Parallelizable: NO
- Must not run with equipment backend/durability/upgrade/crafting/economy changes.
- Reason: shared item/material transactions.

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
Risco: custo divergente.
Mitigação: backend/economy service owns.

Risco: upgrade acidental.
Mitigação: strong confirmation.

Risco: backend missing.
Mitigação: future hook only.
```

---

## 21. Rollback

```text
Reverter repair/upgrade UI/view model/tests/report.
```

---

# /speckit.tasks

## 22. Tasks

- [ ] T001 — Ler fontes.
- [ ] T002 — Auditar repair/upgrade backend/UI.
- [ ] T003 — Consolidar cost/preview projection.
- [ ] T004 — Implement confirmation hardening.
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
| Spoiler/future | A spec respeita dados ocultos e features future? | Matriz de visibility/future no report. | PARTIAL |
| Save/load | A UI altera estado persistido ou só despacha comando? | Declaração de schema/no schema. | PARTIAL |
| Eventos | Subscriptions têm lifecycle? | Mapa de publishers/subscribers se houver. | PARTIAL |
| Testes | Há view model/projection determinística? | EditMode test ou NOT RUN justificado. | PARTIAL |
| Report | Execution report criado? | `docs/validation/04_spec_ui_repair_upgrade_screen_flow_runtime_execution_report.md`. | PARTIAL |

---

## 23B. Local Audit Checklist

Antes de alterar qualquer arquivo, Claude Code/Codex deve rodar e registrar:

```bash
rg -n "Repair|Upgrade|Durability|MaterialCost|GoldCost|BeforeAfter|Blacksmith|Service|Confirmation|ItemInstance" Assets/_Game/Scripts docs/design .specs
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
Given a tela/flow de repair/upgrade UI é aberto com dados válidos
When o jogador navega, seleciona elemento e executa ação segura
Then a UI mostra foco claro, detalhe correto e ações disponíveis
And gameplay input fica bloqueado se modal estiver aberto
And o domínio recebe apenas comando/intent válido, não mutação escondida de UI.
```

### Scenario 2 — Unknown/hidden/future data

```text
Given existe informação oculta, futura, bloqueada por história ou ainda não descoberta
When a UI renderiza a tela
Then ela não revela spoiler nem promessa quebrada
And mostra estado genérico/??? apenas quando o direction permitir
And registra no report qualquer placeholder/future hook.
```

### Scenario 3 — Missing requirement / unavailable state

```text
Given o jogador não cumpre requisito, não tem recurso ou não conhece a informação
When a ação ou detalhe é selecionado
Then a UI mostra motivo bloqueado
And não executa comando inválido
And o backend continua sendo fonte de verdade.
```

### Scenario 4 — Costly or irreversible action

```text
Given uma ação é custosa, destrutiva, irreversível ou protegida
When o jogador tenta executá-la
Then confirmação é exigida quando o direction manda
And foco inicial respeita regra de ação destrutiva
And cancel/back não executa ação de mundo.
```

### Scenario 5 — Final human validation deferred

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

- Repair e Upgrade confundidos.
- Upgrade caro sem confirmação forte.
- Material/gold faltante sem mensagem.
- Preview antes/depois divergente.
- UI consumindo material diretamente.
- Backend ausente tratado como feature runtime quebrada.
- Tier maior apresentado como sempre melhor.
- Item raro/favorito alterado sem confirmação.

---

## 23E. Minimum Execution Report Template

```md
# Execution Report — UI Repair Upgrade Screen Flow Runtime

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
- Unknown/hidden/future:
- Missing requirement:
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
6. A implementação revelar spoiler/future state proibido pelo direction.
7. Não for possível decidir se sistema existente é canônico ou obsoleto.
```


## 23G. Repair / Upgrade Action Matrix

| Action | Requirements | Preview | Confirmation |
|---|---|---|---|
| Repair | Item damaged + material/gold/service | Durability current -> max/partial | Light/strong by cost/rarity |
| Upgrade | Eligible item + materials + gold + service/story/skill | Before/after stat snapshot and tradeoffs | Strong if costly/rare/irreversible |
| CannotRepair | Item not damaged/not repairable | Reason | N/A |
| CannotUpgrade | Missing material/gold/requirement | Reason | N/A |
| FutureBackendMissing | Backend unavailable | Hidden or debug-only | N/A |

## 23H. Cost Preview Requirements

```text
Gold current/required.
Materials owned/required.
Item target.
Result preview.
Risk/tradeoff if known.
Reversibility.
Confirm/Cancel.
```


## 25. Validações obrigatórias

Docs:

```powershell
.\tools\docs\validate_docs.ps1
```

Busca local mínima:

```bash
rg -n "Weapon|Armor|Repair|Upgrade|Spell|Magic|Gamepad|Navigation|Focus|Detail|Durability|Material|MP|Cooldown|Requirement" Assets/_Game/Scripts docs/design .specs
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

- Changed deterministic logic: YES if repair/upgrade projection/confirmation logic changes.
- Requires EditMode tests: YES for cost/preview/confirmation tests.
- Requires PlayMode automated or final human scenario: YES, DEFERRED for repair/upgrade screen flow.
- Requires regression test: YES if fixing known repair/upgrade confirmation bug; otherwise NO.
- Human validation timing: DEFERRED_TO_FINAL_VALIDATION se houver cenário integrado; caso contrário NOT REQUIRED.
- Minimum validation evidence for ACCEPTED: docs validation PASS; C# build PASS if C# changed; EditMode tests PASS or NOT RUN justified; no backend rewrite; no accidental costly upgrade.

---

## 27. Definition of Done

```text
Spec executada sem alterar arquivos proibidos.
Contratos/data/runtime implementados apenas dentro do escopo.
Execution report criado em docs/validation/04_spec_ui_repair_upgrade_screen_flow_runtime_execution_report.md.
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
Não revelar spoiler, future state ou função bloqueada.
```

---

## 29. Notas para execução posterior

Esta spec deve ser executada por Claude Code/Codex com auditoria local do repo, usando `rg` e validando o estado real dos sistemas já implementados.

Se a auditoria local revelar sistema equivalente já existente, a execução deve mudar para hardening/residual, não recriar do zero.
