# SPEC — UI Skill Tree Active Slots Runtime

> **Spec ID:** `04_spec_ui_skill_tree_active_slots_runtime`  
> **Status:** A implementar  
> **Wave:** WAVE 04 — UI / UX Foundation  
> **Priority:** P0  
> **Type:** Runtime / UI / Skill Tree / Active Slots / Confirmations  
> **Domain:** UI / Skill Tree / SkillPoints / Active Slots / Capstones / Respec Hooks  
> **Parallelizable:** NO  
> **Parallel group:** WAVE_04_UI_LOCKED  
> **Can run with:** N/A  
> **Must not run with:** qualquer spec que altere skill tree backend, SkillPoint economy, active slot runtime, respec/Fonte, player input skills, HUD active slots or save/load skill state.  
> **Repo lock scope:** `Assets/_Game/Scripts/UI/**`, `Assets/_Game/Scripts/Skills/**`, `Assets/_Game/Scripts/Player/**`, `Assets/_Game/Tests/EditMode/UI/**`, `docs/validation/04_spec_ui_skill_tree_active_slots_runtime_execution_report.md`  
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
  - `docs/design/gameplay/player/PLAYER_SKILL_TREES_DIRECTION.md`
  - `docs/specs/a_implementar/04_spec_ui_input_focus_modal_routing_runtime.md`
> **Blocks:**  
  - skill tree runtime;
  - active skill execution;
  - respec via Fonte;
  - HUD active slots;
  - save/load skill state;
> **Scope:** consolidar Skill Tree UI com node states, preview antes de gastar ponto, active slot assignment, capstone exclusivity e respec visibility.  
> **Out of scope:** skill tree backend rewrite, balance de nodes, implementação final de todas as skills, Fonte respec runtime completo, prefabs finais.

---

# /speckit.specify

## 1. Contexto

O direction de Skill Trees define 5 árvores iniciais, 50 SkillPoints máximos no endgame, 1 ponto a cada 2 níveis, tiers, ranks, 4 active slots e input separado: Dash, Dodge e Block não ocupam active slot.

O menu flow define Skill Tree Screen com tabs, grafo de nodes, node detail drawer, SkillPoints, active slots, preview antes de comprar, assignment submenu, capstones e respec via Fonte.

---

## 2. Problema

Sem Skill Tree UI contract:

```text
player pode gastar ponto sem preview;
active skill pode equipar automaticamente sem confirmação;
slot cheio pode sobrescrever skill;
capstone exclusivo pode não avisar consequência;
Breath/Fôlego pode reaparecer indevidamente;
respec pode aparecer antes da Fonte desbloquear Memória;
UI pode calcular requisitos diferente do backend.
```

---

## 3. Objetivo

Consolidar Skill Tree UI:

```text
5 árvores iniciais;
node states;
detail drawer;
rank/current-next preview;
SkillPoint cost;
confirm purchase;
active slots 1-4;
no auto-equip if slots full;
capstone exclusivity warning;
respec visibility gated by Fonte stage;
sem Breath/Fôlego.
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
- docs/design/gameplay/player/PLAYER_SKILL_TREES_DIRECTION.md

### Required interpretation

```text
Esta spec é derivada dos directions/refinements canônicos.
Ela não substitui os directions.
Ela transforma parte do refinement em contrato implementável com escopo, locks, validações e quality gate.
```

---

## Direction / Refinement Coverage

### Covered from directions

- Skill Tree Screen suporta 5 árvores iniciais, passivas, ativas, ranks, pré-requisitos, capstones, respec pela Fonte e 4 active slots.
- Node states: LockedUnknown, LockedVisible, Available, Purchased, MaxRank, EquippedActive, BlockedByCapstone, BlockedByRequirement, RefundableWhenRespec.
- Preview antes de gastar ponto mostra custo, efeito ganho, novo rank, active unlock, slot, stat derivado e proximidade de capstone.
- Comprar active skill não equipa automaticamente se slots cheios.
- Se slot vazio, pode sugerir equipar, mas confirmar.
- Respec só aparece quando Fonte desbloquear Fragmento da Memória.
- Breath/Fôlego não existe em atributo, HUD, skill tree ou save/load.

### Deferred / future from directions

- Balance final de todos os nodes.
- Full respec runtime.
- Fonte UI final.
- Todas as active skills finalizadas.
- Gamepad final.
- Visual graph prefab final.

### Explicitly not redefined here

- Skill backend.
- SkillPoint progression backend.
- Active skill execution.
- Save/load skill state.
- HUD active slots.

## 4. Estado atual do repo

```text
SPEC 16 skill tree/active slots/respec aparece como implementado parcial no audit report.
Esta spec deve auditar e harden UI, não recriar skill tree runtime.
```

A confirmar localmente:

```text
SkillTreeManager;
SkillPointManager;
ActiveSlot runtime;
SkillTreeUI;
Fonte/respec gates;
save skill tree state.
```

---

## 5. User stories

```text
Como jogador, quero saber exatamente o que ganho antes de gastar ponto.
Como jogador, quero equipar active skill em slot 1-4 com confirmação.
Como jogador, quero entender por que um node está bloqueado.
Como jogador, quero ver capstone exclusivo e consequência.
Como jogador, não quero ver Breath/Fôlego.
```

---

## 6. Escopo

```text
skill tree screen projection;
node detail drawer;
purchase preview/confirmation;
active slot assignment;
capstone exclusivity display;
respec visibility gate;
tests/validators.
```

---

## 7. Fora de escopo

```text
skill backend rewrite;
node balance;
all active skill implementation;
respec runtime final;
Fonte UI final;
prefab final graph layout.
```

---

## 8. Regras de não duplicação

```text
Não criar SkillTreeManager paralelo.
Não equipar active automaticamente sem regra/confirm.
Não calcular SkillPoint economy na UI.
Não mostrar Breath/Fôlego.
Não permitir capstone exclusivo sem aviso.
```

---

## 9. Critérios de aceite

- Node states projetados corretamente.
- Preview antes de compra.
- Active slot assignment seguro.
- Capstone exclusivity clara.
- Respec gated by Fonte.
- No Breath anywhere.
- Report inclui PlayMode scenario deferido.

---

# /speckit.plan

## 10. Arquitetura alvo

```text
Assets/_Game/Scripts/UI/Skills/SkillTreeScreenController.cs
Assets/_Game/Scripts/UI/Skills/SkillNodeViewModel.cs
Assets/_Game/Scripts/UI/Skills/ActiveSlotAssignmentViewModel.cs
Assets/_Game/Tests/EditMode/UI/SkillTreeViewModelTests.cs
```

Consolidar existentes se houver.

---

## 11. Contratos

### Runtime

```text
UI projects backend skill state.
Purchase/equip dispatches command.
Backend owns validation and point spending.
```

### Save

```text
No save schema change.
```

### UI

```text
Confirm before spending SkillPoint.
Confirm before replacing occupied active slot.
```

---

## 12. Arquivos permitidos

```text
Assets/_Game/Scripts/UI/**
Assets/_Game/Tests/EditMode/UI/**
Assets/_Game/Scripts/Editor/Validation/**
docs/validation/04_spec_ui_skill_tree_active_slots_runtime_execution_report.md
```

Leitura permitida:

```text
Assets/_Game/Scripts/Skills/**
Assets/_Game/Scripts/Player/**
Assets/_Game/Scripts/Fonte/**
Assets/_Game/Scripts/Save/**
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
1. Auditar skill UI/runtime.
2. Consolidar node/active slot view models.
3. Add no-Breath validation.
4. Add purchase/slot confirmation tests.
5. Report.
```

---

## 15. Ordem segura

```text
Input focus -> HUD active slots -> Skill Tree UI -> Fonte respec future.
```

---

## 16. Paralelização

- Parallelizable: NO
- Must not run with skill backend, active slot HUD or respec runtime.
- Reason: skill tree UI shares central skill state.

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
Risco: UI gasta ponto sem backend validation.
Mitigação: command/backend owns.

Risco: Breath reaparece.
Mitigação: validator/search/test.

Risco: capstone exclusivity obscura.
Mitigação: explicit confirmation.
```

---

## 21. Rollback

```text
Reverter skill tree UI/view models/tests/report.
```

---

# /speckit.tasks

## 22. Tasks

- [ ] T001 — Ler fontes.
- [ ] T002 — Auditar skill UI/runtime.
- [ ] T003 — Consolidar node/slot projection.
- [ ] T004 — Implementar preview/confirmation hardening.
- [ ] T005 — Criar tests/validator.
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
| Report | Execution report criado? | `docs/validation/04_spec_ui_skill_tree_active_slots_runtime_execution_report.md`. | PARTIAL |

---

## 23B. Local Audit Checklist

Antes de alterar qualquer arquivo, Claude Code/Codex deve rodar e registrar:

```bash
rg -n "SkillTree|SkillPoint|SkillNode|ActiveSlot|Capstone|Respec|Fonte|Breath|Fôlego|Dash|Dodge|Block" Assets/_Game/Scripts docs/design docs/specs
rg -n "TODO|FIXME|HACK|PARTIAL|DEFERRED|BUILD_VALIDATED|ACCEPTED" docs/specs docs/validation docs/IMPLEMENTATION_STATUS.md docs/project/CURRENT_STATE.md
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
Given a tela/flow de skill tree/active slots UI é aberto com dependências válidas
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

- Compra de node sem confirmação.
- Gasto de SkillPoint sem backend validation.
- Active skill sobrescreve slot ocupado sem confirmação.
- Capstone exclusivo sem aviso.
- Respec visível antes da Fonte/Memória.
- Breath/Fôlego aparecendo em texto/UI/save.
- Dash/Dodge/Block tratados como active slots.
- UI permitindo masterizar duas árvores completas por bug de preview.

---

## 23E. Minimum Execution Report Template

```md
# Execution Report — UI Skill Tree Active Slots Runtime

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


## 23G. Node State Matrix

| State | Action | Detail Drawer |
|---|---|---|
| LockedUnknown | None | Hidden or ??? without spoiler |
| LockedVisible | None | Shows known requirement |
| Available | Purchase preview | Current/next effect |
| Purchased | Upgrade if rank available | Current/next rank |
| MaxRank | None | Max rank effect |
| EquippedActive | Unequip/replace | Slot info |
| BlockedByCapstone | None | Exclusive path reason |
| BlockedByRequirement | None | Requirement reason |
| RefundableWhenRespec | Refund only in respec mode | Refund preview |

## 23H. Active Slot Assignment Invariants

```text
There are exactly 4 active skill slots.
Dash does not occupy active slot.
Dodge does not occupy active slot.
Block does not occupy active slot.
Buying active skill does not replace occupied slot without confirm.
Slot assignment is command to skill runtime, not UI-owned state.
```

## 23I. No-Breath Validation

```text
Search UI strings, skill names, view models and save DTOs for Breath/Fôlego/BR.
Any occurrence must be ERROR unless clearly in historical/archive docs outside runtime.
```


## 25. Validações obrigatórias

Docs:

```powershell
.\tools\docs\validate_docs.ps1
```

Busca local mínima:

```bash
rg -n "Storage|Chest|Crafting|Recipe|SkillTree|SkillPoint|ActiveSlot|QuestLog|QuestDetail|Modal|Focus|Confirm" Assets/_Game/Scripts docs/design docs/specs
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

- Changed deterministic logic: YES if node/slot/projection logic changes.
- Requires EditMode tests: YES for node state, active slot and no-Breath validation.
- Requires PlayMode automated or final human scenario: YES, DEFERRED for opening skill tree, buying node, assigning active slot.
- Requires regression test: YES if fixing known active slot/respec/Breath bug; otherwise NO.
- Human validation timing: DEFERRED_TO_FINAL_VALIDATION se houver cenário integrado; caso contrário NOT REQUIRED.
- Minimum validation evidence for ACCEPTED: docs validation PASS; C# build PASS if C# changed; EditMode tests PASS or NOT RUN justified; no Breath; no auto-equip; no skill backend rewrite.

---

## 27. Definition of Done

```text
Spec executada sem alterar arquivos proibidos.
Contratos/data/runtime implementados apenas dentro do escopo.
Execution report criado em docs/validation/04_spec_ui_skill_tree_active_slots_runtime_execution_report.md.
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
