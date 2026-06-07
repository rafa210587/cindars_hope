# SPEC — UI Equipment Compare Runtime

> **Spec ID:** `04_spec_ui_equipment_compare_runtime`  
> **Status:** A implementar  
> **Revision:** EXPANDED_06_07  
> **Wave:** WAVE 04 — UI / UX Foundation  
> **Priority:** P1  
> **Type:** Runtime / UI / Equipment / Comparison / Anti-error  
> **Domain:** UI / Equipment / Compare / Slots / Known Interactions  
> **Parallelizable:** NO  
> **Parallel group:** WAVE_04_UI_LOCKED  
> **Can run with:** N/A  
> **Must not run with:** qualquer spec que altere equipment backend, inventory UI, item tooltips, bestiary knowledge, durability/repair/upgrade, player derived stats or active spell source.  
> **Repo lock scope:** `Assets/_Game/Scripts/UI/**`, `Assets/_Game/Scripts/Equipment/**`, `Assets/_Game/Scripts/Inventory/**`, `Assets/_Game/Tests/EditMode/UI/**`, `docs/validation/04_spec_ui_equipment_compare_runtime_execution_report.md`  
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
  - `docs/design/gameplay/bestiary/BESTIARY_KNOWLEDGE_DISCOVERY_DIRECTION.md`
  - `docs/specs/a_implementar/04_spec_ui_inventory_items_tooltips_runtime.md`
> **Blocks:**  
  - equipment equip transaction;
  - repair/upgrade UI;
  - known vulnerability UI;
  - player derived stats display;
  - inventory item detail.
> **Scope:** consolidar Equipment UI e comparison drawer sem equipar acidentalmente e sem revelar interações de inimigo não descobertas.  
> **Out of scope:** repair/upgrade flow completo, stat formulas, equipment backend rewrite, final bestiary UI, scene/prefab final layout.

---

# /speckit.specify

## 1. Contexto

O direction de Menu Flows define Equipment Screen com personagem/silhueta e slots equipados, lista/grid de itens equipáveis por slot, comparison drawer e ações equip/unequip/compare. Ver comparação não equipa; confirmar equip é ação separada.

O direction de Equipment define que equipamento deve alterar estilo, custo de stamina, risco/recompensa, block, resistências e interações com inimigos. A UI mostra isso sem substituir fórmulas canônicas.

---

## 2. Problema

Sem comparison contract:

```text
selecionar item pode equipar acidentalmente;
troca pode remover spell fornecida por item sem aviso;
requisito não cumprido pode falhar sem explicação;
known enemy interaction pode revelar spoiler;
item equipado pode ser vendido por acidente.
```

---

## 3. Objetivo

Criar/endurecer Equipment Compare UI:

```text
slots previstos sem criar slots inexistentes;
comparison drawer mostra antes/depois;
equip é ação separada;
requisitos bloqueiam com explicação;
known vulnerabilities só se descobertas;
durability/material/tier/rarity claros.
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
- docs/design/gameplay/bestiary/BESTIARY_KNOWLEDGE_DISCOVERY_DIRECTION.md

### Required interpretation

```text
Esta spec é derivada dos directions/refinements canônicos.
Ela não substitui os directions.
Ela transforma parte do refinement em contrato implementável com escopo, locks, validações e quality gate.
```

---

## Direction / Refinement Coverage

### Covered from directions

- Equipment Screen mostra slots equipados, itens equipáveis filtrados e comparison drawer.
- Comparison drawer mostra item atual, item selecionado, dano/armor/resistência, scaling, durabilidade, peso/ASPD/StaminaCost, material/tier/quality/rarity, efeitos e known vulnerabilities.
- Ver comparação não equipa; confirmar equip é separado.
- Se troca remover spell fornecida por item, avisar.
- Known Enemy Interactions mostram apenas conhecimento descoberto.
- Equipamento não deve ser apenas +dano; deve comunicar tradeoffs.

### Deferred / future from directions

- Repair/Upgrade screen completo.
- Source/history future.
- Full enemy interaction UI.
- Gamepad final.
- Visual equipment paperdoll final.

### Explicitly not redefined here

- Equipment stat formulas.
- Bestiary discovery runtime.
- Player derived attributes.
- Inventory backend.
- Spell source runtime.

## 4. Estado atual do repo

```text
Equipment UI/equipment durability aparecem como parciais no audit report.
Esta spec deve auditar o existente e atuar como hardening.
```

A confirmar localmente:

```text
EquipmentManager;
EquipmentUI;
Item/equipment definitions;
durability fields;
spell source from item;
known vulnerability service.
```

---

## 5. User stories

```text
Como jogador, quero comparar item sem equipar.
Como jogador, quero saber por que não posso equipar item.
Como jogador, quero ser avisado se perder spell fornecida por item.
Como jogador, só quero ver interações de inimigos que descobri.
```

---

## 6. Escopo

```text
equipment slot projection;
comparison drawer;
equip confirmation separation;
requirement failure messages;
known interaction visibility;
tests/validators.
```

---

## 7. Fora de escopo

```text
backend equipment rewrite;
repair/upgrade flow;
durability mechanics;
full bestiary UI;
stat formula changes;
scene/prefab wiring final.
```

---

## 8. Regras de não duplicação

```text
Não criar EquipmentManager paralelo.
Não calcular fórmulas finais na UI.
Não revelar unknown vulnerabilities.
Não equipar no hover/selection.
Não remover spell source sem warning.
```

---

## 9. Critérios de aceite

- Comparison does not equip.
- Equip requires explicit confirm/action.
- Requirements failure is explained.
- Known enemy interactions are gated.
- Durability/material/tier/rarity displayed when available.
- Report lists deferred PlayMode scenario.

---

# /speckit.plan

## 10. Arquitetura alvo

```text
Assets/_Game/Scripts/UI/Equipment/EquipmentScreenController.cs
Assets/_Game/Scripts/UI/Equipment/EquipmentCompareViewModel.cs
Assets/_Game/Scripts/UI/Equipment/EquipmentRequirementPresenter.cs
Assets/_Game/Tests/EditMode/UI/EquipmentCompareViewModelTests.cs
```

---

## 11. Contratos

### Data

```text
UI consumes equipment definitions/instances and derived stat snapshot.
```

### Runtime

```text
Selection updates compare.
Confirm action dispatches equip command.
```

### Save

```text
No save schema change.
```

### UI

```text
Known interactions require knowledge service visibility.
```

---

## 12. Arquivos permitidos

```text
Assets/_Game/Scripts/UI/**
Assets/_Game/Tests/EditMode/UI/**
Assets/_Game/Scripts/Editor/Validation/**
docs/validation/04_spec_ui_equipment_compare_runtime_execution_report.md
```

Leitura permitida:

```text
Assets/_Game/Scripts/Equipment/**
Assets/_Game/Scripts/Inventory/**
Assets/_Game/Scripts/Player/**
Assets/_Game/Scripts/Bestiary/**
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
1. Auditar equipment UI/backend.
2. Consolidar compare view model.
3. Integrar known interaction visibility.
4. Criar tests para no-equip-on-compare and requirement projection.
5. Criar report.
```

---

## 15. Ordem segura

```text
Inventory item projection -> Equipment compare -> Equip transaction/repair future.
```

---

## 16. Paralelização

- Parallelizable: NO
- Must not run with inventory UI or equipment backend changes.
- Reason: shared item/equipment projection.

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
Risco: revelar spoiler.
Mitigação: knowledge gate.

Risco: equip acidental.
Mitigação: confirm action separation.
```

---

## 21. Rollback

```text
Reverter equipment UI/view model/tests/report.
```

---

# /speckit.tasks

## 22. Tasks

- [ ] T001 — Ler fontes.
- [ ] T002 — Auditar equipment UI/backend.
- [ ] T003 — Consolidar compare view model.
- [ ] T004 — Implementar hardening mínimo.
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
| Report | Execution report foi criado? | `docs/validation/04_spec_ui_equipment_compare_runtime_execution_report.md`. | PARTIAL |

---

## 23B. Local Audit Checklist

Antes de alterar qualquer arquivo, Claude Code/Codex deve rodar e registrar no execution report:

```bash
rg -n "Equipment|Equip|Compare|Requirement|Durability|Material|Tier|KnownVulnerability|Bestiary|SpellSource|ActiveSkill|Accessory" Assets/_Game/Scripts docs/design docs/specs
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
Given o sistema base relacionado a equipment compare UI existe ou foi criado de forma mínima
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

- Selection/hover equipando item automaticamente.
- Comparison drawer calculando stat final com fórmula divergente.
- Known enemy interaction revelada sem descoberta.
- Troca removendo spell/focus sem warning.
- Requirement failure sem motivo visível.
- Item equipado vendável sem warning/protection.
- Durability quebrada não refletida.
- UI criando slot que backend não suporta.

---

## 23E. Minimum Execution Report Template

O execution report desta spec deve conter, no mínimo:

```md
# Execution Report — UI Equipment Compare Runtime

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


## 23G. Equipment Compare Fields

| Campo | Atual | Candidato | Observação |
|---|---:|---:|---|
| Slot | YES | YES | MainHand/OffHand/Armor/Accessory/Tool etc. |
| Damage/Armor | YES | YES | Via derived/stat snapshot, não fórmula UI. |
| StaminaCost/Weight | YES | YES | Mostrar tradeoff. |
| Block/Defense | YES | YES | Se aplicável. |
| Resistances | YES | YES | Se conhecidas. |
| Durability/Charges | YES | YES | ItemInstance-aware. |
| Material/Tier/Quality/Rarity | YES | YES | Se existir. |
| Requirements | YES | YES | Falha explicada. |
| Spell source | YES | YES | Avisar se perder magia. |
| Known enemy interactions | GATED | GATED | Só bestiary known. |

## 23H. No Accidental Equip Invariants

```text
Focus/selection updates comparison only.
Confirm/equip command is separate.
Cancel restores previous selection without equip.
Requirement failure blocks equip command.
Risky replacement may require confirmation.
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

- Changed deterministic logic: YES if compare/requirement visibility logic changes.
- Requires EditMode tests: YES for compare projection and visibility gating logic.
- Requires PlayMode automated or final human scenario: YES, DEFERRED for equipment screen compare/equip flow.
- Requires regression test: YES if fixing known accidental equip/spoiler bug; otherwise NO.
- Human validation timing: DEFERRED_TO_FINAL_VALIDATION se houver cenário integrado; caso contrário NOT REQUIRED.
- Minimum validation evidence for ACCEPTED: docs validation PASS; C# build PASS if C# changed; EditMode tests PASS or NOT RUN justified; no accidental equip; no unknown interaction spoiler.

---

## 27. Definition of Done

```text
Spec executada sem alterar arquivos proibidos.
Contratos/data/runtime implementados apenas dentro do escopo.
Execution report criado em docs/validation/04_spec_ui_equipment_compare_runtime_execution_report.md.
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
