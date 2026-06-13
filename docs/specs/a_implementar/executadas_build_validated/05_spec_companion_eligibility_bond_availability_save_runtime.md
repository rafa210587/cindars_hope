# SPEC — Companion Eligibility Bond Availability Save Runtime

> **Spec ID:** `05_spec_companion_eligibility_bond_availability_save_runtime`  
> **Status:** A implementar  
> **Wave:** WAVE 05 — Farm Animals / Companions  
> **Priority:** P1  
> **Type:** Runtime / Companions / Eligibility / Bond / Availability / Save  
> **Domain:** Companions / Eligibility / Recruitment / Bond / Availability / Save  
> **Parallelizable:** NO  
> **Parallel group:** WAVE_05_FARM_ANIMALS_COMPANIONS_LOCKED  
> **Can run with:** N/A  
> **Must not run with:** qualquer spec que altere city NPC roster, romance/casamento, companion cave combat, companion farm jobs, social relationship runtime, pets, save schema ou companion equipment.  
> **Repo lock scope:** `Assets/_Game/Scripts/Companions/**`, `Assets/_Game/Scripts/NPC/**`, `Assets/_Game/Scripts/Social/**`, `Assets/_Game/Scripts/Save/**`, `Assets/_Game/Scripts/Editor/Validation/**`, `Assets/_Game/Tests/EditMode/Companions/**`, `docs/validation/05_spec_companion_eligibility_bond_availability_save_runtime_execution_report.md`  
> **Depends on:**  
  - `docs/design/SPEC_SOURCE_MAP.md`
  - `docs/design/SPECIFICATION_PROCESS.md`
  - `docs/design/lore/VAALARA_GAME_CANON_DIRECTION_v1.0.md`
  - `docs/specs/SPEC_IMPLEMENTABLE_TEMPLATE.md`
  - `docs/specs/SPEC_WAVE_EXECUTION_PROTOCOL.md`
  - `docs/specs/SPEC_VALIDATION_MATRIX_MASTER.md`
  - `docs/specs/SPEC_EXISTING_IMPLEMENTATION_AUDIT.md`
  - `docs/project/CURRENT_STATE.md`
  - `docs/design/gameplay/farm/FARM_DESIGN_DIRECTION_v1.3.md`
  - `docs/design/gameplay/farm/FARM_LAYOUT_SCALE_BUILDINGS_DIRECTION.md`
  - `docs/design/gameplay/companions/COMPANIONS_DIRECTION.md`
  - `docs/design/gameplay/pets/PETS_DIRECTION.md`
  - `docs/design/gameplay/ui_ux/UI_UX_FULL_GAMEPLAY_DIRECTION.md`
  - `docs/design/gameplay/ui_ux/UI_UX_MENU_SCREEN_FLOWS_DIRECTION.md`
> **Blocks:**  
  - companion farm job board;
  - companion cave selection;
  - companion combat assist;
  - companion visits to farm;
  - social/relationship UI future.
> **Scope:** definir/endurecer elegibilidade, desbloqueio, vínculo, disponibilidade e save/load básico de companions, sem romance/casamento profundo e sem pets.  
> **Out of scope:** combat AI, cave companion behavior, farm job execution, romance/casamento, companion equipment completo, party múltipla, pets.

---

# /speckit.specify

## 1. Contexto

Companion é um NPC com vínculo funcional suficiente para acompanhar ou ajudar o jogador. O direction define que nem todo NPC é companion, nem todo companion é romance, e pet é sistema separado. O escopo atual recomendado inclui 1 companion ativo por vez na caverna, jobs simples de fazenda, visitas à fazenda, vínculo/reputação básica, assistência leve em combate, ferimento/recuo e save/load.

Esta spec cria a base de dados/estado para as specs de companion jobs e cave companion sem implementar AI/combat ou jobs.

---

## 2. Problema

Sem base de companion:

```text
companion pode ser hardcoded por nome;
romance pode ser confundido com companion;
pet pode virar companion;
companion forte pode ser recrutado só comprando serviço cedo;
availability pode ignorar agenda/quest/ferimento;
bond pode ser romance disfarçado;
save/load pode perder unlocked roles;
companion pode virar requisito obrigatório para terminar o jogo.
```

---

## 3. Objetivo

Criar/endurecer:

```text
CompanionEligibilityFlags;
CompanionUnlockState;
CompanionBondState;
CompanionAvailabilityState;
CompanionRole;
CompanionInjuryState;
Save/load state;
No pets;
No romance deep system;
No full companion equipment.
```

---

## 4. Regras de design

```text
Companion ajuda, mas não joga pelo jogador.
Eligibility deve ser dado do NPC, não hardcode por nome.
Pet é sistema separado.
Romance/casamento detalhado é separado.
Companion não é obrigatório para terminar o jogo.
O jogador deve poder entrar na caverna sem companion.
```

---

## 5. User stories / engineering stories

```text
Como jogador, quero desbloquear companion por relação/quest/progresso, não por botão genérico.
Como NPC roster, quero declarar flags de elegibilidade.
Como save/load, quero preservar vínculo, disponibilidade, ferimento e roles.
Como farm/cave systems, quero consultar disponibilidade sem duplicar lógica.
Como design, quero impedir pet/romance/equipment completo de entrar cedo.
```

---

## 6. Escopo

Inclui:

```text
eligibility flags;
unlock/recruitment state;
bond/trust state;
availability resolver;
injury/unavailable state;
role declarations;
save/load contract;
tests/validators.
```

Não inclui:

```text
farm job execution;
cave AI/combat;
romance/casamento;
spouse routines;
companion equipment complete;
party multiple companions;
pets.
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

- docs/design/gameplay/farm/FARM_DESIGN_DIRECTION_v1.3.md
- docs/design/gameplay/farm/FARM_LAYOUT_SCALE_BUILDINGS_DIRECTION.md
- docs/design/gameplay/companions/COMPANIONS_DIRECTION.md
- docs/design/gameplay/pets/PETS_DIRECTION.md
- docs/design/gameplay/ui_ux/UI_UX_FULL_GAMEPLAY_DIRECTION.md
- docs/design/gameplay/ui_ux/UI_UX_MENU_SCREEN_FLOWS_DIRECTION.md

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

- Companion é NPC com vínculo funcional suficiente para acompanhar ou ajudar.
- Companion pode acompanhar caverna, ajudar jobs de fazenda, visitar fazenda, ajudar quests e participar de combate com limites.
- Companion ajuda, mas não joga pelo jogador.
- Eligibility deve ser dado do NPC, não hardcode por nome.
- Escopo atual recomendado inclui jobs simples de fazenda, 1 companion ativo por vez, vínculo básico, assistência leve e save/load.
- Pet é sistema separado e pets estão deferidos.

### Deferred / future from directions

- Party com múltiplos companions.
- Romance/casamento profundo.
- Companion equipment completo.
- Skill tree grande de companion.
- Farm defense/invasões.
- Permadeath.
- Pets.

### Explicitly not redefined here

- NPC roster concreto.
- Quest pessoal de cada NPC.
- Social relationship runtime.
- Cave AI/combat.
- Farm job execution.
- City schedules final.

## 7. Modelo de domínio

### 7.1 CompanionEligibilityFlags

```text
NpcId
CanBeFarmCompanion
CanBeCaveCompanion
CanBeQuestCompanion
CanBeSocialCompanion
CanBeRomanceCompanion
CanBeSpouseCompanion
CompanionLockedByStory
CompanionLockedByReputation
CompanionLockedByQuest
CompanionUnavailable
```

### 7.2 CompanionUnlockState

```text
CompanionId
NpcId
UnlockState: Locked | Eligible | Temporary | Unlocked | Scheduled | StoryOnly | Unavailable
UnlockedRoles[]
UnlockedByQuestIds[]
UnlockedByReputationTier optional
UnlockedDay optional
```

### 7.3 CompanionBondState

```text
CompanionId
CompanionBondLevel
CompanionTrust
CompanionFatigue optional
CompanionInjuryState
CompanionUnlockedRoles
CompanionJobRank
CompanionCaveRank
LastInteractionDay optional
```

### 7.4 CompanionAvailabilityState

```text
AvailableNow
UnavailableReason
AllowedContexts: FarmJob | Cave | Quest | VisitFarm | Social
CurrentScheduleBlock optional
InjuryState
StoryLock
QuestConflict
```

---

## 8. Recruitment/unlock rules

```text
Farm companion common baseline: relation medium + simple quest/progress.
Cave companion baseline: relation medium/high + quest or trust proof.
Quest companion can be temporary and does not unlock all companion systems.
Strong cave companion cannot be recruited only by paying money early.
Eligibility must be data-driven from NPC data.
```

---

## 9. Bond and penalties

```text
Bond increases through personal quests, successful work, cave exploration, gifts, dialogue, events, safe return and aligned choices.
Bond can decrease/trap through repeated injury, abandonment, strong conflicting choices or failed personal quest.
Penalty should be narrative and recoverable except severe choices.
Bond is connected to relationship but not identical to romance.
```

---

## 10. Availability

```text
Companion can refuse if unavailable by schedule, quest, event, injury, exhaustion, story lock or relationship requirement.
Companion continues being a city NPC with routine.
Companion should not teleport without systemic/visual justification.
Availability resolver must be reusable by farm job board and cave entry.
```

---

## 11. Save/load

Must preserve:

```text
Companion unlock state.
Bond/trust/job/cave ranks.
Injury/unavailable state.
Temporary/story companion state if active.
Unlocked roles.
```

Must not persist:

```text
NPC GameObject reference;
dialogue UI selection as unlock;
pet state;
romance state as companion state unless explicit bridge.
```

If save state for companions is absent and cannot be added safely, STOP for save/migration spec.

---

## 12. Criteria

```text
Eligibility is data-driven, not name-hardcoded.
Companion unlock state persists.
Bond/trust state persists.
Availability resolver blocks invalid invites.
Pets are not implemented.
Romance is not required for companion.
Tests cover eligibility, unlock, availability, injury and save/load.
```

---

# /speckit.plan

## 13. Arquitetura alvo

```text
Assets/_Game/Scripts/Companions/CompanionEligibilityFlags.cs
Assets/_Game/Scripts/Companions/CompanionUnlockState.cs
Assets/_Game/Scripts/Companions/CompanionBondState.cs
Assets/_Game/Scripts/Companions/CompanionAvailabilityState.cs
Assets/_Game/Scripts/Companions/CompanionAvailabilityResolver.cs
Assets/_Game/Tests/EditMode/Companions/CompanionEligibilityAvailabilityTests.cs
```

Consolidar existentes se houver.

---

## 14. Arquivos permitidos

```text
Assets/_Game/Scripts/Companions/**
Assets/_Game/Scripts/NPC/**
Assets/_Game/Scripts/Social/**
Assets/_Game/Scripts/Save/**
Assets/_Game/Scripts/Editor/Validation/**
Assets/_Game/Tests/EditMode/Companions/**
docs/validation/05_spec_companion_eligibility_bond_availability_save_runtime_execution_report.md
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
1. Auditar NPC/social/companion state existentes.
2. Consolidar eligibility flags.
3. Consolidar unlock/bond/availability state.
4. Criar availability resolver.
5. Integrar save/load safely or STOP.
6. Criar tests.
7. Criar report.
```

---

## 17. Paralelização

- Parallelizable: NO
- Must not run with:
  - companion farm jobs;
  - cave companion combat;
  - city NPC roster;
  - social/romance;
  - pets.
- Reason: eligibility/availability is base dependency.

---

## 18. Impacto save/load

```text
Does this change save schema? SHOULD BE NO if companion save exists; otherwise STOP.
Does this add save section? NO unless dedicated save spec approves.
Does this require migration? NO unless adding companion state; then STOP.
Does this persist Unity references? NO.
```

---

## 19. Impacto eventos

```text
Adds events: CONDITIONAL, e.g. CompanionUnlockedEvent, CompanionBondChangedEvent if event contracts allow.
Changes events: SHOULD BE NO.
Requires unsubscribe pattern: YES if subscribers are created.
```

---

## 20. Impacto UI/Unity

```text
Changes UI: NO final.
Changes scenes/prefabs/assets: NO.
Requires PlayMode/final human scenario: YES, DEFERRED for invite/availability flow if wired.
```

---

## 21. Riscos

```text
Risco: romance and companion merged.
Mitigação: state separation tests.

Risco: pet implemented.
Mitigação: forbidden path.

Risco: companion hardcoded by name.
Mitigação: eligibility data-driven validator.
```

---

# /speckit.tasks

## 22. Tasks

- [ ] T001 — Ler fontes.
- [ ] T002 — Auditar companion/NPC/social state.
- [ ] T003 — Consolidar eligibility flags.
- [ ] T004 — Consolidar unlock/bond/availability.
- [ ] T005 — Implementar availability resolver.
- [ ] T006 — Integrar save/load safely.
- [ ] T007 — Criar tests.
- [ ] T008 — Rodar validações.
- [ ] T009 — Criar report.

## 23A. Execution Readiness Matrix

| Área | Pergunta obrigatória | Evidência esperada | Status se faltar |
|---|---|---|---|
| Fonte canônica | A execução leu Source Map e directions do domínio? | Lista de fontes no execution report. | PARTIAL |
| Estado real do repo | Sistemas existentes relacionados a companion eligibility/bond/availability/save foram auditados antes de criar novos? | Comandos `rg` e achados no report. | PARTIAL |
| Não duplicação | Existe sistema equivalente já implementado/parcial? | Decisão REUSE/HARDEN/CREATE/DEFER. | BLOCKED se duplicar |
| Escopo | A execução ficou dentro de companion eligibility/bond/availability/save? | Arquivos alterados e justificativa. | PARTIAL |
| Pets | A execução respeitou que pets estão deferidos e não criou runtime/UI/save de pets? | Checklist explícito no report. | BLOCKED se violar |
| Economia | A spec não cria produto/automação/loot infinito? | Anti-exploit tests/checklist. | PARTIAL |
| Save/load | Houve alteração de schema? | Declaração explícita NO ou STOP se migration necessária. | BLOCKED se alterar sem migration |
| Eventos | Publishers/subscribers/lifecycle foram mapeados? | Mapa de eventos e unsubscribe policy se houver. | PARTIAL |
| UI/PlayMode | Há fluxo visual ou gameplay integrado? | Cenário final deferido documentado. | BUILD_VALIDATED no máximo se ausente |
| Testes | Há lógica determinística nova? | EditMode test ou NOT RUN justificado. | PARTIAL |
| Report | Execution report foi criado? | `docs/validation/05_spec_companion_eligibility_bond_availability_save_runtime_execution_report.md`. | PARTIAL |

---

## 23B. Local Audit Checklist

Antes de alterar qualquer arquivo, Claude Code/Codex deve rodar e registrar no execution report:

```bash
rg -n "Companion|Eligibility|Bond|Trust|Availability|Injury|Unlock|CanBeFarmCompanion|CanBeCaveCompanion|Pet|Romance" Assets/_Game/Scripts docs/design docs/specs
rg -n "Animal|Livestock|Cow|Chicken|Sheep|Egg|Milk|Wool|Feed|Pasture|Coop|Barn|Companion|JobBoard|FarmJob|Pet|Save|Inventory|Product" Assets/_Game/Scripts docs/design docs/specs
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
Given o sistema base relacionado a companion eligibility/bond/availability/save existe ou foi criado de forma mínima
When o jogador executa o fluxo principal desta spec
Then o comportamento segue o direction canônico
And save/load, economia, inventário e rotina diária permanecem consistentes
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

### Scenario 3 — Pets remain deferred

```text
Given o repo ou directions mencionam pets
When a execução encontra referências de pet
Then ela não cria runtime, UI, save/load, data assets ou validação Unity de pets
And registra que pets permanecem deferidos até decisão explícita futura.
```

### Scenario 4 — Economy/save invalid state

```text
Given produto, job, animal, companion, feed, output ou save payload inválido
When validator/teste é executado
Then a violação é reportada com motivo claro
And o runtime não duplica item/ouro, não cria output do nada e não corrompe save.
```

### Scenario 5 — Final human validation deferred

```text
Given o fluxo exige inspeção visual de FarmScene/UI/gameplay
When a spec termina tecnicamente
Then o report registra cenário final em docs/validation/FINAL_HUMAN_VALIDATION_BY_WAVE.md
And não pede validação humana imediata por spec.
```

---

## 23D. Edge Cases and Failure Modes

A execução deve cobrir ou registrar risco residual para:

- Companion hardcoded por nome.
- Pet tratado como companion.
- Romance exigido para companion.
- Strong cave companion comprado cedo.
- Availability ignora injury/story/schedule.
- Bond vira stat de poder obrigatório.
- Companion state perdido no reload.
- Save schema alterado sem migration.

---

## 23E. Minimum Execution Report Template

```md
# Execution Report — Companion Eligibility Bond Availability Save Runtime

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

## Pets deferred compliance
- Did this spec create pet runtime/UI/save/data assets? NO
- Pet references found:
- Pet-related work deferred:

## Implementation decision
- REUSE_EXISTING / HARDEN_EXISTING / CREATE_MINIMAL / DEFER
- Justification:

## Files changed
- ...

## Functional evidence
- Happy path:
- Existing partial implementation:
- Pets deferred:
- Economy/save invalid state:
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
4. A implementação exigir reescrever Farm/Companion/Inventory/Save system canônico existente.
5. A implementação criar pets runtime, pets UI, pets save/load ou pets data assets.
6. A implementação criar romance/casamento profundo, companion equipment completo, party com múltiplos companions ou farm defense.
7. A implementação criar invasão/defesa/inimigos/dano a crops na fazenda, proibido no roadmap atual.
8. A implementação permitir item/ouro/produto/job output infinito.
9. Não for possível decidir se sistema existente é canônico ou obsoleto.
```


## 23G. Companion Separation Matrix

| Concept | Stored here? | Notes |
|---|---:|---|
| Companion eligibility | YES | data-driven from NPC |
| Companion unlock | YES | state/progression |
| Companion bond/trust | YES | not romance |
| Companion injury/unavailable | YES | base |
| Farm job assignment | NO | next spec |
| Cave AI/combat | NO | future cave/combat spec |
| Romance/marriage | NO | social spec future |
| Pet state | NO | pets deferred |

## 23H. Availability Reasons

```text
LockedByStory
LockedByReputation
LockedByQuest
UnavailableBySchedule
UnavailableByInjury
UnavailableByFatigue
UnavailableByQuestConflict
UnavailableByStoryEvent
UnavailableByContext
```


## 25. Validações obrigatórias

Docs:

```powershell
.\tools\docs\validate_docs.ps1
```

Busca local mínima:

```bash
rg -n "Animal|Livestock|Cow|Chicken|Sheep|Egg|Milk|Wool|Feed|Pasture|Coop|Barn|Companion|JobBoard|FarmJob|Pet|Save|Inventory|Product" Assets/_Game/Scripts docs/design docs/specs
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
Quando houver cenário visual/gameplay de animal, produto, job board, companion job ou rotina, registrar em execution report e vincular a docs/validation/FINAL_HUMAN_VALIDATION_BY_WAVE.md.
```

---

## 26. Testing Quality Gate

- Changed deterministic logic: YES, eligibility/unlock/bond/availability logic is deterministic.
- Requires EditMode tests: YES for eligibility/unlock/availability/injury/save tests.
- Requires PlayMode automated or final human scenario: YES, DEFERRED for invite/availability visual flow.
- Requires regression test: YES if fixing existing companion availability/save bug; otherwise NO.
- Human validation timing: DEFERRED_TO_FINAL_VALIDATION se houver FarmScene/UI/gameplay integrado; caso contrário NOT REQUIRED.
- Minimum validation evidence for ACCEPTED: docs validation PASS; C# build PASS if C# changed; EditMode tests PASS or NOT RUN justified; no pets; no romance merge; availability resolver safe.

---

## 27. Definition of Done

```text
Spec executada sem alterar arquivos proibidos.
Contratos/data/runtime implementados apenas dentro do escopo.
Execution report criado em docs/validation/05_spec_companion_eligibility_bond_availability_save_runtime_execution_report.md.
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
Não implementar pets runtime/UI/save/data assets.
Não criar companion como segundo player.
Não criar invasão/defesa/inimigos na fazenda.
Não criar item/ouro/produto/job output infinito.
Não pedir human test por spec.
Não executar runtime em massa antes da 01Q ou exceção humana explícita.
Não alterar SPEC_EXECUTION_ORDER.md.
```

---

## 29. Notas para execução posterior

Esta spec deve ser executada por Claude Code/Codex com auditoria local do repo, usando `rg` e validando o estado real dos sistemas já implementados.

Se a auditoria local revelar sistema equivalente já existente, a execução deve mudar para hardening/residual, não recriar do zero.
