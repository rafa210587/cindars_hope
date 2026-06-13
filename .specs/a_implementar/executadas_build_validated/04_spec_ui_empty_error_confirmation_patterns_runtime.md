# SPEC — UI Empty Error Confirmation Patterns Runtime

> **Spec ID:** `04_spec_ui_empty_error_confirmation_patterns_runtime`  
> **Status:** A implementar  
> **Wave:** WAVE 04 — UI / UX Foundation  
> **Priority:** P0  
> **Type:** Runtime / UI / Cross-screen Patterns / Empty Error Confirmation  
> **Domain:** UI / Empty States / Blocked States / Confirmation / Focus Order  
> **Parallelizable:** NO  
> **Parallel group:** WAVE_04_UI_LOCKED  
> **Can run with:** N/A  
> **Must not run with:** qualquer spec que altere input focus/modal foundation, all screen-specific confirmation behavior, shop/inventory/crafting/skill/Fonte confirmations or UI config assets.  
> **Repo lock scope:** `Assets/_Game/Scripts/UI/**`, `Assets/_Game/Tests/EditMode/UI/**`, `Assets/_Game/Scripts/Editor/Validation/**`, `docs/validation/04_spec_ui_empty_error_confirmation_patterns_runtime_execution_report.md`  
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
  - `.specs/a_implementar/04_spec_ui_input_focus_modal_routing_runtime.md`
> **Blocks:**  
  - all UI screens;
  - input focus modal routing;
  - shop/inventory/crafting/skill/Fonte flows;
  - future gamepad navigation;
> **Scope:** criar/endurecer padrões transversais de empty/error/blocked states, confirmations e focus order para todos os menus.  
> **Out of scope:** layout visual final de cada tela, localization final, gamepad final completo, accessibility settings completas, scene/prefab wiring.

---

# /speckit.specify

## 1. Contexto

O direction de menu define que toda tela precisa tratar erro como estado previsto. Estados vazios não devem parecer bug, bloqueios precisam explicar próximo passo quando possível, e feature futura não deve aparecer no runtime final como promessa quebrada.

Também define padrões de confirmação leve e forte, com ação, alvo, custo, consequência, reversibilidade e botões Confirm/Cancel.

Esta spec cria o padrão transversal usado por todas as telas da WAVE 04.

---

## 2. Problema

Sem padrão transversal:

```text
cada tela inventa empty state;
erro parece bug;
bloqueio não explica próximo passo;
feature future aparece quebrada;
ação destrutiva confirma por acidente;
ESC/back fecha camada errada;
telas divergentes não são testáveis.
```

---

## 3. Objetivo

Criar/endurecer contratos comuns:

```text
empty state taxonomy;
blocked/error state taxonomy;
feature future hiding policy;
confirmation light/strong;
confirmation content fields;
focus order definitions;
PendingConfirmationAction;
tests/validators.
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

### Required interpretation

```text
Esta spec é derivada dos directions/refinements canônicos.
Ela não substitui os directions.
Ela transforma parte do refinement em contrato implementável com escopo, locks, validações e quality gate.
```

---

## Direction / Refinement Coverage

### Covered from directions

- Estados comuns: NoItems, NoSellableItems, NoShopStock, NoRecipes, NoCraftableRecipes, NoSkillPoints, NoActiveSlots, MissingMaterials, MissingGold, MissingRequirement, ItemLocked, QuestItemProtected, InventoryFull, StorageFull, InvalidTarget, WrongContext, FeatureLockedByStory, FeatureFutureNotImplemented.
- Estado vazio nunca deve parecer bug.
- Bloqueio precisa explicar próximo passo quando possível.
- Feature futura não deve aparecer em runtime final como promessa quebrada; só em debug/dev.
- Confirmação leve: compra comum em quantidade alta, vender stack comum, craft em massa, dormir cedo.
- Confirmação forte: vender item raro, descartar item, capstone, respec, upgrade caro, escolha irreversível, decisão final.
- Confirmação deve mostrar ação, alvo, custo, consequência, reversibilidade e Confirm/Cancel.
- Focus order por tela é contrato conceitual.

### Deferred / future from directions

- Localization final.
- Gamepad final.
- Visual prefab final.
- Accessibility settings complete.
- All screen-specific layouts.

### Explicitly not redefined here

- Screen-specific runtime.
- Shop/inventory/crafting backend.
- Skill/Fonte backend.
- Quest runtime.
- Calendar runtime.

## 4. Estado atual do repo

```text
Algumas telas já têm empty states/confirmations parciais.
Esta spec consolida padrão transversal para evitar drift entre telas.
```

A confirmar localmente:

```text
ConfirmationModal;
UIEmptyState;
BlockedStatePresenter;
PendingConfirmationAction;
UIScreenConfig;
Focus order handling;
existing per-screen messages.
```

---

## 5. User stories

```text
Como jogador, quero entender por que uma ação está bloqueada.
Como jogador, quero confirmar ações custosas/destrutivas sem acidente.
Como UI, quero empty states consistentes.
Como dev, quero adicionar tela nova usando os mesmos patterns.
```

---

## 6. Escopo

```text
empty/error/blocked taxonomy;
confirmation profile;
light vs strong confirmation;
focus order contract;
future feature hiding policy;
validators/tests.
```

---

## 7. Fora de escopo

```text
localization final;
visual style final;
gamepad final;
individual screen layout;
scene/prefab wiring.
```

---

## 8. Regras de não duplicação

```text
Não criar confirmation modal paralelo se já existir.
Não duplicar empty state per screen sem profile.
Não mostrar FeatureFutureNotImplemented no runtime final.
Não permitir ação destrutiva sem confirmation profile.
Não alterar domínio/backend.
```

---

## 9. Critérios de aceite

- Taxonomia comum definida/implementada.
- Light/strong confirmation rules claras.
- Confirmation content includes action/target/cost/consequence/reversible.
- Future features hidden from runtime final.
- Validators/tests cover key patterns.
- Report lista telas impactadas.

---

# /speckit.plan

## 10. Arquitetura alvo

```text
Assets/_Game/Scripts/UI/Common/UIEmptyState.cs
Assets/_Game/Scripts/UI/Common/UIBlockedState.cs
Assets/_Game/Scripts/UI/Common/UIConfirmationProfile.cs
Assets/_Game/Scripts/UI/Common/PendingConfirmationAction.cs
Assets/_Game/Scripts/UI/Common/UIFocusOrderProfile.cs
Assets/_Game/Tests/EditMode/UI/UICommonPatternTests.cs
```

Consolidar existentes se houver.

---

## 11. Contratos

### Runtime

```text
Common UI patterns are presentation/control contracts.
Domain systems own actions and state.
```

### Save

```text
No save schema change.
```

### UI

```text
Screen-specific UI uses shared patterns or documented exception.
```

---

## 12. Arquivos permitidos

```text
Assets/_Game/Scripts/UI/**
Assets/_Game/Tests/EditMode/UI/**
Assets/_Game/Scripts/Editor/Validation/**
docs/validation/04_spec_ui_empty_error_confirmation_patterns_runtime_execution_report.md
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
1. Auditar confirmation/empty/blocked existentes.
2. Consolidar common pattern classes/view models.
3. Add validators/tests for destructive/costly actions and future-hidden state.
4. Report screen impacts.
```

---

## 15. Ordem segura

```text
Input focus foundation -> common UI patterns -> screen-specific UI.
```

---

## 16. Paralelização

- Parallelizable: NO
- Must not run with screen-specific UI specs simultaneously.
- Reason: common patterns will be shared by all UI screens.

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
Adds events: NO by default.
Changes events: NO.
Requires unsubscribe pattern: NO unless modal events are touched.
```

---

## 19. Impacto UI/Unity

```text
Changes UI: YES common behavior/view model.
Scenes/prefabs/assets: NO.
Requires PlayMode/final human scenario: YES, DEFERRED.
```

---

## 20. Riscos

```text
Risco: common pattern ficar genérico demais.
Mitigação: screen-specific exceptions allowed with report.

Risco: future feature aparece quebrada.
Mitigação: runtime final hides future not implemented.

Risco: confirmation too intrusive.
Mitigação: light/strong categories.
```

---

## 21. Rollback

```text
Reverter common UI pattern classes/tests/report.
```

---

# /speckit.tasks

## 22. Tasks

- [ ] T001 — Ler fontes.
- [ ] T002 — Auditar empty/blocked/confirmation existing.
- [ ] T003 — Consolidar taxonomy/patterns.
- [ ] T004 — Implement validators/tests.
- [ ] T005 — Rodar validações.
- [ ] T006 — Criar report.

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
| Report | Execution report criado? | `docs/validation/04_spec_ui_empty_error_confirmation_patterns_runtime_execution_report.md`. | PARTIAL |

---

## 23B. Local Audit Checklist

Antes de alterar qualquer arquivo, Claude Code/Codex deve rodar e registrar:

```bash
rg -n "EmptyState|BlockedState|Confirmation|PendingConfirmation|NoItems|NoRecipes|MissingGold|MissingRequirement|FeatureFuture|FocusOrder|Cancel|Confirm" Assets/_Game/Scripts docs/design .specs
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
Given a tela/flow de common UI empty/error/confirmation patterns é aberto com dependências válidas
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

### Scenario 3 — Empty / unavailable / blocked state

```text
Given não há dados disponíveis ou requisito está ausente
When a tela abre ou item/entry/função/dia é selecionado
Then a UI mostra empty state ou motivo bloqueado
And nenhuma ação inválida é executada
And o report registra o estado esperado.
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

- Empty state parecendo bug.
- Blocked state sem próximo passo.
- Feature future visível no runtime final.
- Ação destrutiva sem confirmation.
- Foco inicial em Confirm para ação destrutiva.
- Confirmation sem custo/consequência.
- Back/cancel executando ação.
- Tela criando padrão local divergente sem justificativa.

---

## 23E. Minimum Execution Report Template

```md
# Execution Report — UI Empty Error Confirmation Patterns Runtime

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
6. A implementação revelar spoiler/future state proibido pelo direction.
7. Não for possível decidir se sistema existente é canônico ou obsoleto.
```


## 23G. Empty / Blocked State Taxonomy

| State | Tipo | Mensagem deve incluir |
|---|---|---|
| NoItems | Empty | Fonte vazia |
| NoSellableItems | Empty | Por que nada é vendável |
| NoShopStock | Empty | Quando/restock se conhecido |
| NoRecipes | Empty | Como descobrir receitas se conhecido |
| NoCraftableRecipes | Blocked | Material/station/skill faltante |
| NoSkillPoints | Blocked | Como ganhar pontos se conhecido |
| NoActiveSlots | Blocked | Slot capacity |
| MissingMaterials | Blocked | Lista material faltante |
| MissingGold | Blocked | Gold necessário/atual |
| MissingRequirement | Blocked | Requirement conhecido |
| ItemLocked | Protected | Como destravar se permitido |
| QuestItemProtected | Protected | Proteção de quest/key |
| InventoryFull | Blocked | Onde liberar espaço |
| StorageFull | Blocked | Container sem espaço |
| InvalidTarget | Error | Contexto inválido |
| WrongContext | Error | Onde usar corretamente |
| FeatureLockedByStory | Locked | Hint sem spoiler |
| FeatureFutureNotImplemented | Debug-only | Não runtime final |

## 23H. Confirmation Strength Matrix

| Action | Strength | Initial focus |
|---|---|---|
| Comprar item comum em quantidade alta | Light | Confirm or Cancel by context |
| Vender stack comum | Light | Confirm if reversible/simple |
| Craft em massa | Light | Confirm if cost clear |
| Dormir cedo | Light | Confirm |
| Vender item raro/favorito | Strong | Cancel |
| Descartar item | Strong | Cancel |
| Capstone | Strong | Cancel |
| Respec | Strong | Cancel |
| Upgrade caro | Strong | Cancel |
| Escolha irreversível | Strong | Cancel |
| Decisão final | Strong | Cancel |
```


## 25. Validações obrigatórias

Docs:

```powershell
.\tools\docs\validate_docs.ps1
```

Busca local mínima:

```bash
rg -n "Social|NPC|Calendar|Fonte|Confirmation|EmptyState|BlockedState|Focus|Modal|Known|Hidden|Future" Assets/_Game/Scripts docs/design .specs
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

- Changed deterministic logic: YES if common UI pattern logic is created/changed.
- Requires EditMode tests: YES for confirmation/empty/blocked state tests.
- Requires PlayMode automated or final human scenario: YES, DEFERRED for destructive action confirmation and modal behavior scenario.
- Requires regression test: YES if fixing known confirmation/empty state bug; otherwise NO.
- Human validation timing: DEFERRED_TO_FINAL_VALIDATION se houver cenário integrado; caso contrário NOT REQUIRED.
- Minimum validation evidence for ACCEPTED: docs validation PASS; C# build PASS if C# changed; EditMode tests PASS or NOT RUN justified; future features hidden; destructive actions protected.

---

## 27. Definition of Done

```text
Spec executada sem alterar arquivos proibidos.
Contratos/data/runtime implementados apenas dentro do escopo.
Execution report criado em docs/validation/04_spec_ui_empty_error_confirmation_patterns_runtime_execution_report.md.
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
