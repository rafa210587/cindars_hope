# SPEC — UI HUD Main Gameplay Runtime

> **Spec ID:** `04_spec_ui_hud_main_gameplay_runtime`  
> **Status:** Implementado e ACCEPTED  
> **Evidência:** Código presente — `Assets/_Game/Scripts/UI/HUD/HUDGameplayViewModel.cs`, `HudVisibilityController.cs`, `Assets/_Game/Scripts/UI/Notification/ContextHintController.cs`, `Assets/_Game/Scripts/UI/HudSuppressionConsumer.cs`. Play Mode humano PASS 2026-08-13 — HUD (HP/stamina/hotbar/prompts) esteve visível e funcional durante os 11 fluxos do playtest sem erro reportado.  
> **Revision:** EXPANDED_06_07  
> **Wave:** WAVE 04 — UI / UX Foundation  
> **Priority:** P0  
> **Type:** Runtime / UI / HUD / Notifications / Context  
> **Domain:** UI / HUD / Hotbar / Active Slots / Notifications / Context Prompts  
> **Parallelizable:** NO  
> **Parallel group:** WAVE_04_UI_LOCKED  
> **Can run with:** N/A  
> **Must not run with:** qualquer spec que altere UI input focus/modal, hotbar, active slots, player stats, hunger/stamina/cansaço, quest notifications, cave HUD ou debug HUD.  
> **Repo lock scope:** `Assets/_Game/Scripts/UI/**`, `Assets/_Game/Scripts/Player/**`, `Assets/_Game/Scripts/Inventory/**`, `Assets/_Game/Scripts/Skills/**`, `Assets/_Game/Tests/EditMode/UI/**`, `docs/validation/04_spec_ui_hud_main_gameplay_runtime_execution_report.md`  
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
  - `.specs/SPEC_EXISTING_IMPLEMENTATION_AUDIT.md`
> **Blocks:**  
  - inventory/hotbar UI;
  - skill active slots UI;
  - combat/cave HUD;
  - notifications system;
  - debug HUD separation.
> **Scope:** auditar e consolidar HUD principal de gameplay com stats, hotbar, active slots, contextual prompts e notifications sem poluir a tela.  
> **Out of scope:** layouts finais, art final, cave combat feedback completo, social/pet/companion UI completa, debug HUD global final, accessibility settings completas.

---

# /speckit.specify

## 1. Contexto

O direction de UI/UX define HUD base: HP, MP quando relevante, Stamina, Fome, Cansaço, hotbar, 4 active slots, arma/ferramenta ativa, status negativos, buffs, companion/pet state quando relevante e quest/context prompt.

Também define que HUD não deve mostrar Breath/Fôlego/BR e não deve competir com a leitura da cena.

---

## 2. Problema

Sem contrato de HUD:

```text
HUD pode ficar poluída;
debug info pode vazar para HUD final;
hotbar e active slots podem confundir ferramentas/skills;
status e buffs podem ocupar área de combate;
notifications podem bloquear input ou empilhar demais;
quest prompt pode virar texto permanente demais.
```

---

## 3. Objetivo

Consolidar HUD principal:

```text
permanent HUD: HP, Stamina, hotbar, active slots, active weapon/tool;
contextual HUD: MP, fome/cansaço compactos, status/buffs, pet/companion state, quest/context prompt;
notifications não bloqueiam input;
debug HUD separado;
HUD não cobre área central.
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

### Required interpretation

```text
Esta spec é derivada dos directions/refinements canônicos.
Ela não substitui os directions.
Ela transforma parte do refinement em contrato implementável com escopo, locks, validações e quality gate.
```

---

## Direction / Refinement Coverage

### Covered from directions

- HUD base consolidada inclui HP, MP quando relevante, Stamina, Fome, Cansaço, hotbar, 4 active slots e arma/ferramenta ativa.
- HUD não mostra Breath/Fôlego/BR.
- Permanentes: HP, Stamina, hotbar, active slots, arma/ferramenta ativa.
- Contextuais: MP, fome/cansaço compacto, companion/pet, status/buffs, quest/context prompt.
- Notifications não devem bloquear input nem empilhar a ponto de cobrir gameplay.
- Debug HUD é separado do HUD final.

### Deferred / future from directions

- Cave combat feedback completo.
- Companion/pet UI final.
- Accessibility settings completas.
- Visual style final.
- Gamepad navigation final.

### Explicitly not redefined here

- Player stat formulas.
- Skill active slots logic.
- Inventory/hotbar data model.
- Quest runtime.
- Combat damage/status logic.

## 4. Estado atual do repo

```text
Audit report indica HUD/debug e UI 17B/17C-F parciais.
GameplayInputRouter, toasts, hints, death screen e checkpoint menu podem já existir.
Esta spec deve auditar antes de criar qualquer sistema novo.
```

A confirmar localmente:

```text
HUDController;
NotificationToastController;
ContextHintController;
HotbarUI;
ActiveSlotsUI;
DebugHUD;
PlayerStats bindings.
```

---

## 5. User stories

```text
Como jogador, quero ler vida/stamina/hotbar rapidamente.
Como jogador, quero notificação curta sem bloquear movimento.
Como combat/farm player, quero HUD que não cubra área central.
Como dev, quero debug separado da HUD final.
```

---

## 6. Escopo

```text
HUD base contract;
permanent vs contextual sections;
hotbar/active slots presentation;
context prompt behavior;
notification priority/stack limits;
debug HUD separation;
tests/validators when feasible.
```

---

## 7. Fora de escopo

```text
Cave combat HUD completo;
all status icon art;
pet/companion full UI;
quest log full UI;
accessibility settings complete;
scene/prefab final layout.
```

---

## 8. Regras de não duplicação

```text
Não criar HUDController paralelo se já existir.
Não misturar debug with final HUD.
Não colocar Breath/Fôlego/BR.
Não depender de HUD para lógica de gameplay.
Não deixar notification bloquear input salvo confirmação crítica.
```

---

## 9. Critérios de aceite

- HUD contract documentado/implementado.
- Permanent/contextual sections claras.
- Hotbar vs active slots separados.
- Notifications têm stack/priority behavior.
- Debug output marcado como debug.
- Report lista PlayMode scenarios finais.

---

# /speckit.plan

## 10. Arquitetura alvo

```text
Assets/_Game/Scripts/UI/HUD/HudController.cs
Assets/_Game/Scripts/UI/HUD/HudViewModel.cs
Assets/_Game/Scripts/UI/HUD/NotificationToastController.cs
Assets/_Game/Scripts/UI/HUD/ContextHintController.cs
Assets/_Game/Scripts/UI/HUD/DebugHudGate.cs
Assets/_Game/Tests/EditMode/UI/HudViewModelTests.cs
```

Se já existirem, consolidar.

---

## 11. Contratos

### Data

```text
HUD consumes player/UI view model, not raw gameplay internals when possible.
```

### Runtime

```text
HUD is presentation only.
Gameplay logic remains in gameplay systems.
```

### Events

```text
HUD subscribes to events with lifecycle/unsubscribe.
```

### Save

```text
No save schema change.
```

---

## 12. Arquivos permitidos

```text
Assets/_Game/Scripts/UI/**
Assets/_Game/Tests/EditMode/UI/**
Assets/_Game/Scripts/Editor/Validation/**
docs/validation/04_spec_ui_hud_main_gameplay_runtime_execution_report.md
```

Leitura permitida:

```text
Assets/_Game/Scripts/Player/**
Assets/_Game/Scripts/Inventory/**
Assets/_Game/Scripts/Skills/**
Assets/_Game/Scripts/Core/Events/**
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
1. Auditar HUD/toast/context/debug existentes.
2. Consolidar contract/view model.
3. Evitar scene/prefab changes.
4. Add tests for view model/notification stack if feasible.
5. Report PlayMode final scenarios.
```

---

## 15. Ordem segura

```text
Input focus foundation -> HUD main -> individual screens.
```

---

## 16. Paralelização

- Parallelizable: NO
- Must not run with UI input focus or hotbar/skill/inventory specs.
- Reason: HUD is shared presentation foundation.

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
Requires unsubscribe pattern: YES if HUD subscribers touched.
```

---

## 19. Impacto UI/Unity

```text
Changes UI: YES HUD behavior/presentation code.
Scenes/prefabs/assets: NO in this spec.
Requires PlayMode/final human scenario: YES, DEFERRED.
```

---

## 20. Riscos

```text
Risco: HUD ficar dependente de debug.
Mitigação: DebugHudGate.

Risco: HUD poluir tela.
Mitigação: permanent/contextual separation.

Risco: duplicate subscriptions.
Mitigação: event lifecycle tests/audit.
```

---

## 21. Rollback

```text
Reverter HUD/view model/toast/hint changes/tests/report.
```

---

# /speckit.tasks

## 22. Tasks

- [ ] T001 — Ler fontes.
- [ ] T002 — Auditar HUD/toast/context/debug existentes.
- [ ] T003 — Consolidar permanent/contextual HUD contract.
- [ ] T004 — Implementar hardening mínimo.
- [ ] T005 — Criar tests/validator.
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
| Report | Execution report foi criado? | `docs/validation/04_spec_ui_hud_main_gameplay_runtime_execution_report.md`. | PARTIAL |

---

## 23B. Local Audit Checklist

Antes de alterar qualquer arquivo, Claude Code/Codex deve rodar e registrar no execution report:

```bash
rg -n "HudController|HudViewModel|NotificationToast|ContextHint|Hotbar|ActiveSlot|PlayerStats|DebugHud|HP|Stamina|Hunger|Fatigue|Mana" Assets/_Game/Scripts docs/design .specs
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
Given o sistema base relacionado a main gameplay HUD existe ou foi criado de forma mínima
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

- Debug data visible in final HUD.
- Breath/Fôlego/BR appearing anywhere.
- Toast stack covering central gameplay.
- Quest prompt permanent enough to obscure farm/cave view.
- HUD logic mutating player stats.
- HUD subscriptions not unsubscribed.
- Active slot UI mixed with equipment slot UI.
- Context hint not clearing after target disappears.

---

## 23E. Minimum Execution Report Template

O execution report desta spec deve conter, no mínimo:

```md
# Execution Report — UI HUD Main Gameplay Runtime

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


## 23G. HUD Visibility Matrix

| Elemento | Permanente | Contextual | Proibido/Observação |
|---|---:|---:|---|
| HP | YES | NO | Sempre legível. |
| Stamina | YES | NO | Ações físicas dependem disso. |
| Hotbar | YES | NO | Consumíveis/ferramentas conforme design. |
| 4 Active Skill Slots | YES | NO | Separados de equipamento. |
| Arma/Ferramenta ativa | YES | NO | Pode ser compacto. |
| MP | NO | YES | Mostrar quando magia/MP relevante. |
| Fome/Cansaço | NO | YES | Compacto; não poluir. |
| Buffs/Debuffs | NO | YES | Ícones/tempo quando ativo. |
| Quest/context prompt | NO | YES | Deve expirar/ocultar. |
| Companion/Pet | NO | YES/FUTURE | Somente quando relevante. |
| Debug IDs/state | NO | NO | Debug HUD separado. |
| Breath/Fôlego/BR | NO | NO | Removido do design. |

## 23H. Notification Priority

```text
CRITICAL
  Confirmação/risco real; pode exigir modal separado.

IMPORTANT
  Quest updated, rare item, level up, danger.

NORMAL
  Item gained, crop ready, shop restock.

LOW
  Flavor/minor info; pode agrupar ou descartar se fila cheia.
```

Regra: notification não deve bloquear input salvo se for modal de confirmação explícita.

## 25. Validações obrigatórias

Docs:

```powershell
.\tools\docs\validate_docs.ps1
```

Busca local mínima:

```bash
rg -n "Quest|Objective|Condition|Trigger|Reward|Flag|Softlock|Debug|InputFocus|Modal|HUD|Notification" Assets/_Game/Scripts docs/design .specs
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

- Changed deterministic logic: YES if HUD view model/notification stack logic is changed.
- Requires EditMode tests: YES for view model/notification rules when harness available.
- Requires PlayMode automated or final human scenario: YES, DEFERRED for HUD readability and no-debug final presentation scenario.
- Requires regression test: YES if fixing known HUD/debug/notification bug; otherwise NO.
- Human validation timing: DEFERRED_TO_FINAL_VALIDATION se houver cenário integrado; caso contrário NOT REQUIRED.
- Minimum validation evidence for ACCEPTED: docs validation PASS; C# build PASS if C# changed; EditMode tests PASS or NOT RUN justified; final PlayMode scenario documented; no debug leakage.

---

## 27. Definition of Done

```text
Spec executada sem alterar arquivos proibidos.
Contratos/data/runtime implementados apenas dentro do escopo.
Execution report criado em docs/validation/04_spec_ui_hud_main_gameplay_runtime_execution_report.md.
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
Não revelar spoilers antes de discovery/visibility policy.
Não pedir human test por spec.
Não executar runtime em massa antes da 01Q ou exceção humana explícita.
Não alterar SPEC_EXECUTION_ORDER.md.
```

---

## 29. Notas para execução posterior

Esta spec deve ser executada por Claude Code/Codex com auditoria local do repo, usando `rg` e validando o estado real dos sistemas já implementados.

Se a auditoria local revelar sistema equivalente já existente, a execução deve mudar para hardening/residual, não recriar do zero.
