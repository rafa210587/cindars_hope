# SPEC — Companion Eligibility Recruitment State Save Future Runtime

> **Spec ID:** `14_spec_companion_eligibility_recruitment_state_save_future_runtime`  
> **Status:** A implementar / Future mapped  
> **Wave:** WAVE 14 — Companions / Jobs / Cave Assist / Future Social Hooks  
> **Priority:** P1  
> **Type:** Runtime / Future / Companion State / Recruitment / Save  
> **Domain:** Companion / Eligibility / Recruitment / Availability / SaveLoad  
> **Parallelizable:** NO  
> **Parallel group:** WAVE_14_COMPANIONS_FUTURE  
> **Can run with:** N/A  
> **Must not run with:** qualquer spec que altere city NPC roster final, romance/casamento, pet runtime, save migration, cave/farm job execution ou UI visual final.  
> **Repo lock scope:** `Assets/_Game/Scripts/Companions/**`, `Assets/_Game/Scripts/City/**`, `Assets/_Game/Scripts/Save/**`, `Assets/_Game/Tests/EditMode/Companions/**`, `docs/validation/14_spec_companion_eligibility_recruitment_state_save_future_runtime_execution_report.md`  
> **Depends on:**  
  - `docs/design/SPEC_SOURCE_MAP.md`
  - `docs/design/SPECIFICATION_PROCESS.md`
  - `docs/design/lore/VAALARA_GAME_CANON_DIRECTION_v1.0.md`
  - `docs/specs/SPEC_IMPLEMENTABLE_TEMPLATE.md`
  - `docs/specs/SPEC_WAVE_EXECUTION_PROTOCOL.md`
  - `docs/specs/SPEC_VALIDATION_MATRIX_MASTER.md`
  - `docs/specs/SPEC_EXISTING_IMPLEMENTATION_AUDIT.md`
  - `docs/project/CURRENT_STATE.md`
  - `docs/design/gameplay/companions/COMPANIONS_DIRECTION.md`
  - `docs/design/gameplay/pets/PETS_DIRECTION.md`
  - `docs/design/gameplay/city/CITY_NPC_ROSTER_SERVICES_DIRECTION_v1.1.md`
  - `docs/design/gameplay/city/CITY_LAYOUT_BUILDINGS_SCHEDULE_DIRECTION.md`
  - `docs/design/gameplay/farm/FARM_DESIGN_DIRECTION_v1.3.md`
  - `docs/design/gameplay/farm/FARM_LAYOUT_SCALE_BUILDINGS_DIRECTION.md`
  - `docs/design/gameplay/player/PLAYER_CORE_SYSTEMS_DIRECTION.md`
  - `docs/design/gameplay/player/PLAYER_DERIVED_ATTRIBUTES_DIRECTION.md`
  - `docs/design/gameplay/player/PLAYER_SKILL_TREES_DIRECTION.md`
  - `docs/design/gameplay/combat/COMBAT_CORE_DIRECTION.md`
  - `docs/design/gameplay/combat/STATUS_EFFECTS_DIRECTION.md`
  - `docs/design/gameplay/enemies/ENEMY_BEHAVIORS_DIRECTION.md`
  - `docs/design/gameplay/cave/CAVE_DESIGN_DIRECTION.md`
  - `docs/design/gameplay/cave/CAVE_MONSTER_ROSTER_DIRECTION.md`
  - `docs/design/gameplay/cave/CAVE_MONSTER_ROSTER_ENEMY_BEHAVIOR_ADAPTER.md`
  - `docs/design/gameplay/cave/CAVE_COMBAT_BALANCE_VULNERABILITIES_DIRECTION.md`
  - `docs/design/gameplay/equipment/EQUIPMENT_WEAPONS_ARMOR_MATERIALS_DIRECTION.md`
  - `docs/design/gameplay/equipment/EQUIPMENT_MECHANICAL_BASELINES_DIRECTION.md`
  - `docs/design/gameplay/equipment/EQUIPMENT_ENEMY_VULNERABILITY_ADAPTER.md`
  - `docs/design/gameplay/magic/MAGIC_SPELLS_ACTIONS_DIRECTION.md`
  - `docs/design/gameplay/magic/MAGIC_LEARNING_UNLOCKS_SOURCES_DIRECTION.md`
  - `docs/design/gameplay/loot_crafting_economy/LOOT_CRAFTING_ECONOMY_DIRECTION.md`
  - `docs/design/gameplay/loot_crafting_economy/ECONOMY_PRICING_STOCK_REFRESH_DIRECTION.md`
  - `docs/design/gameplay/quests/QUEST_OBJECTIVE_EVENT_SYSTEM_DIRECTION.md`
  - `docs/design/gameplay/quests/QUESTS_MAIN_LORE_DIRECTION.md`
  - `docs/design/gameplay/quests/QUESTS_MAIN_PROGRESSION_REFINEMENT_DIRECTION.md`
  - `docs/design/gameplay/save_load/SAVE_LOAD_FULL_STATE_DIRECTION.md`
  - `docs/design/gameplay/ui_ux/UI_UX_FULL_GAMEPLAY_DIRECTION.md`
  - `docs/design/gameplay/ui_ux/UI_UX_MENU_SCREEN_FLOWS_DIRECTION.md`
  - `docs/design/gameplay/bestiary/BESTIARY_KNOWLEDGE_DISCOVERY_DIRECTION.md`
> **Blocks:**  
  - farm companion jobs;
  - cave companion assist;
  - companion HUD/menu;
  - companion quest hooks;
  - relationship/social future.
> **Scope:** definir/endurecer elegibilidade, recrutamento, disponibilidade, estado base e save/load de companion sem implementar jobs/combat/romance/pets.  
> **Out of scope:** farm jobs, cave AI, romance/casamento, pet system, NPC roster authoring, save migration.

---

# /speckit.specify

## 1. Contexto

Companion é NPC com vínculo funcional suficiente para acompanhar ou ajudar em atividades específicas. Nem todo NPC é companion; nem todo companion é romance; nem todo romance vira companion de combate; pet é sistema separado.

Esta spec cria a base de eligibility/recruitment/state/save.

---

## 2. Problema

Sem estado base:

```text
eligibilidade fica hardcoded por nome;
NPC romance vira companion automaticamente;
spouse vira combat companion automaticamente;
pet pode ocupar slot de companion;
companion recrutado não persiste;
agenda/indisponibilidade é ignorada;
companion entra na caverna ferido/exausto;
save pode serializar GameObject/NPC reference;
Breath/Fôlego pode reaparecer em NPC/companion.
```

---

## 3. Objetivo

Criar/endurecer:

```text
CompanionEligibilityFlags;
CompanionUnlockState;
CompanionAvailabilityState;
CompanionRecruitmentRule;
CompanionStateRecord;
CompanionSaveSection;
CompanionRole;
CompanionResourceState;
CompanionInvitationResult.
```

---

## 4. Regras de design

```text
Eligibility é dado do NPC, não hardcode por nome.
Recruitment pode vir de amizade, reputação, quest, serviço, evento, romance/casamento, resgate, guilda/templo/fazenda.
O jogador não deve recrutar companion forte só comprando serviço cedo.
TemporaryCompanion não precisa desbloquear todos os sistemas.
Companion usa HP/MP/Stamina e atributos básicos; Breath/Fôlego não existe.
Pet é sistema separado e deferido.
Romance/casamento detalhado fica fora.
```

---

## 5. User stories / engineering stories

```text
Como designer, quero declarar se NPC pode ser farm/cave/quest/social companion.
Como jogador, quero desbloquear companion por relação/quest/evento.
Como runtime, quero saber se companion está disponível no horário e estado atuais.
Como save/load, quero persistir unlocked/active/state por IDs.
Como city schedule, quero impedir convite em conflito de rotina/evento.
```

---

## 6. Escopo

Inclui:

```text
eligibility flags;
recruitment rules;
temporary/permanent/scheduled/story companion states;
availability;
base resource state;
save/load records;
invitation result;
tests/validators.
```

Não inclui:

```text
farm job execution;
cave AI/combat;
deep relationship system;
romance/casamento;
pet runtime;
NPC roster content.
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

- docs/design/gameplay/companions/COMPANIONS_DIRECTION.md
- docs/design/gameplay/pets/PETS_DIRECTION.md
- docs/design/gameplay/city/CITY_NPC_ROSTER_SERVICES_DIRECTION_v1.1.md
- docs/design/gameplay/city/CITY_LAYOUT_BUILDINGS_SCHEDULE_DIRECTION.md
- docs/design/gameplay/farm/FARM_DESIGN_DIRECTION_v1.3.md
- docs/design/gameplay/farm/FARM_LAYOUT_SCALE_BUILDINGS_DIRECTION.md
- docs/design/gameplay/player/PLAYER_CORE_SYSTEMS_DIRECTION.md
- docs/design/gameplay/player/PLAYER_DERIVED_ATTRIBUTES_DIRECTION.md
- docs/design/gameplay/player/PLAYER_SKILL_TREES_DIRECTION.md
- docs/design/gameplay/combat/COMBAT_CORE_DIRECTION.md
- docs/design/gameplay/combat/STATUS_EFFECTS_DIRECTION.md
- docs/design/gameplay/enemies/ENEMY_BEHAVIORS_DIRECTION.md
- docs/design/gameplay/cave/CAVE_DESIGN_DIRECTION.md
- docs/design/gameplay/cave/CAVE_MONSTER_ROSTER_DIRECTION.md
- docs/design/gameplay/cave/CAVE_MONSTER_ROSTER_ENEMY_BEHAVIOR_ADAPTER.md
- docs/design/gameplay/cave/CAVE_COMBAT_BALANCE_VULNERABILITIES_DIRECTION.md
- docs/design/gameplay/equipment/EQUIPMENT_WEAPONS_ARMOR_MATERIALS_DIRECTION.md
- docs/design/gameplay/equipment/EQUIPMENT_MECHANICAL_BASELINES_DIRECTION.md
- docs/design/gameplay/equipment/EQUIPMENT_ENEMY_VULNERABILITY_ADAPTER.md
- docs/design/gameplay/magic/MAGIC_SPELLS_ACTIONS_DIRECTION.md
- docs/design/gameplay/magic/MAGIC_LEARNING_UNLOCKS_SOURCES_DIRECTION.md
- docs/design/gameplay/loot_crafting_economy/LOOT_CRAFTING_ECONOMY_DIRECTION.md
- docs/design/gameplay/loot_crafting_economy/ECONOMY_PRICING_STOCK_REFRESH_DIRECTION.md
- docs/design/gameplay/quests/QUEST_OBJECTIVE_EVENT_SYSTEM_DIRECTION.md
- docs/design/gameplay/quests/QUESTS_MAIN_LORE_DIRECTION.md
- docs/design/gameplay/quests/QUESTS_MAIN_PROGRESSION_REFINEMENT_DIRECTION.md
- docs/design/gameplay/save_load/SAVE_LOAD_FULL_STATE_DIRECTION.md
- docs/design/gameplay/ui_ux/UI_UX_FULL_GAMEPLAY_DIRECTION.md
- docs/design/gameplay/ui_ux/UI_UX_MENU_SCREEN_FLOWS_DIRECTION.md
- docs/design/gameplay/bestiary/BESTIARY_KNOWLEDGE_DISCOVERY_DIRECTION.md

### Required interpretation

```text
Esta spec deriva de COMPANIONS_DIRECTION.
Ela não redefine NPC concreto, romance/casamento detalhado, pet, enemy AI, loot tables, fórmula final de dano/HP/MP/Stamina, layout de fazenda/cidade/caverna ou stats de monstros.
Ela deve respeitar que Pet é sistema separado e está explicitamente deferido.
Ela deve preservar a regra: companion ajuda, mas não joga pelo jogador.
Quando houver conflito com roster de cidade, farm, combat, enemy behavior, cave balance, pets ou economy directions, o executor deve parar e registrar CONFLICT.
```

---

## Direction / Refinement Coverage

### Covered from directions

- Companion é NPC com vínculo funcional que pode acompanhar ou ajudar o jogador.
- Companion ajuda, mas não joga pelo jogador.
- Nem todo NPC é companion; nem todo companion é romance; nem todo romance vira companion de combate; nem todo spouse deve ser combat companion.
- Eligibility deve ser dado do NPC, não hardcode por nome.
- Flags incluem CanBeFarmCompanion, CanBeCaveCompanion, CanBeQuestCompanion, CanBeSocialCompanion, CanBeRomanceCompanion, CanBeSpouseCompanion e locks.
- Companions usam HP, MP, Stamina, Força, Constituição, Destreza, Inteligência, Vontade e Carisma; Breath/Fôlego não existe.

### Deferred / future from directions

- Party com múltiplos companions.
- Romance/casamento profundo.
- Companion equipment completo.
- Skill tree própria grande.
- Farm defense/invasões.
- Perma death.
- Pet runtime.

### Explicitly not redefined here

- City NPC roster concrete data.
- Relationship/social full system.
- Farm job execution.
- Cave companion AI.
- Pet system.
- Save migration.

## 7. Modelo de domínio

### 7.1 CompanionEligibilityFlags

```text
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
Unknown
KnownNpc
EligibleLocked
AvailableToInvite
TemporaryActive
Unlocked
ScheduledAvailable
StoryLocked
Unavailable
InjuredUnavailable
ExhaustedUnavailable
```

### 7.3 CompanionType

```text
TemporaryCompanion
UnlockedCompanion
ScheduledCompanion
StoryCompanion
```

### 7.4 CompanionRole

```text
FarmWorker
Forager
Miner
Fighter
Guardian
Healer
Alchemist
Researcher
Scout
Merchant
Builder
AnimalCaretaker
Crafter
MusicianSupport
```

### 7.5 CompanionStateRecord

```text
NpcId
CompanionId
UnlockState
CompanionType
PrimaryRole
SecondaryRole optional
RelationshipGateState
QuestGateState
StoryGateState
IsActiveFarmCompanion
IsActiveCaveCompanion
CurrentAvailability
Hp
Mp
Stamina
Fatigue optional
InjuredUntilDay optional
LastInvitedDay optional
LastJobDay optional
LastCaveRunId optional
```

### 7.6 CompanionSaveSection

```text
Version
UnlockedCompanionIds[]
KnownEligibleCompanionIds[]
ActiveFarmCompanionIds[]
ActiveCaveCompanionId optional
TemporaryCompanionIds[]
CompanionStateRecords[]
LastValidatedVersion
```

---

## 8. Recruitment rule

```text
CompanionRecruitmentRule:
  CompanionId
  RequiredRelationship
  RequiredReputation
  RequiredQuestIds[]
  RequiredStoryFlags[]
  RequiredServicePayment optional
  RequiredRescueEvent optional
  RequiredGuildOrTempleProgress optional
  ForbiddenFlags[]
  TemporaryOnly
  AllowedContexts
```

---

## 9. Availability rules

```text
Accept invite when:
  relationship/reputation/quest/story gates pass;
  schedule says available;
  not injured/exhausted;
  no conflicting quest/event;
  context allowed.

Reject invite when:
  companion is unavailable by story;
  companion is working/sleeping/unreachable;
  active cave companion slot already filled;
  role not allowed for context;
  pet-only system requested.
```

---

## 10. Save/load rules

```text
Persist stable IDs and simple values.
Do not persist NPC GameObject.
Do not persist Transform.
Do not persist ScriptableObject direct reference.
Do not persist Breath/Fôlego.
Normalize active cave companion count to max 1.
Temporary companions expire by quest/event rule.
```

---

## 11. Criteria

```text
Eligibility/recruitment/state contracts exist.
Save section exists or STOP if migration required.
Availability respects schedule/injury/exhaustion/story locks.
Active cave companion max 1.
Pet not treated as companion.
Romance/spouse flags do not auto-grant combat.
Tests cover eligibility, unlock, invite accept/reject, active slot cap, save/load normalization and no Breath.
```

---

# /speckit.plan

## 12. Arquitetura alvo

```text
Assets/_Game/Scripts/Companions/CompanionEligibilityFlags.cs
Assets/_Game/Scripts/Companions/CompanionUnlockState.cs
Assets/_Game/Scripts/Companions/CompanionType.cs
Assets/_Game/Scripts/Companions/CompanionRole.cs
Assets/_Game/Scripts/Companions/CompanionStateRecord.cs
Assets/_Game/Scripts/Companions/CompanionSaveSection.cs
Assets/_Game/Scripts/Companions/CompanionRecruitmentRule.cs
Assets/_Game/Scripts/Companions/CompanionAvailabilityService.cs
Assets/_Game/Scripts/Companions/CompanionStateValidator.cs
Assets/_Game/Tests/EditMode/Companions/CompanionEligibilityStateSaveTests.cs
```

Consolidar existentes se houver.

---

## 13. Arquivos permitidos

```text
Assets/_Game/Scripts/Companions/**
Assets/_Game/Scripts/City/**
Assets/_Game/Scripts/Save/**
Assets/_Game/Scripts/Editor/Validation/**
Assets/_Game/Tests/EditMode/Companions/**
docs/validation/14_spec_companion_eligibility_recruitment_state_save_future_runtime_execution_report.md
```

---

## 14. Arquivos proibidos

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

## 15. Estratégia

```text
1. Auditar NPC/companion/save systems.
2. Consolidar eligibility/recruitment/state.
3. Implementar availability service and validator.
4. Integrar save section somente se já existir; senão STOP.
5. Criar tests.
6. Criar report.
```

---

## 16. Paralelização

- Parallelizable: NO
- Must not run with:
  - save migration;
  - farm companion jobs;
  - cave companion AI;
  - city NPC roster edits;
  - romance/social system.
- Reason: base companion state is shared.

---

## 17. Impacto save/load

```text
Does this change save schema? SHOULD BE NO if CompanionSaveSection exists; otherwise STOP.
Does this add save section? NO unless dedicated migration is approved.
Does this require migration? NO unless adding section; then STOP.
Does this persist Unity references? NO.
```

---

## 18. Impacto eventos

```text
Adds events: CONDITIONAL, e.g. CompanionUnlockedEvent, CompanionAvailabilityChangedEvent.
Changes events: SHOULD BE NO.
Requires unsubscribe pattern: YES if subscribers are created.
```

---

## 19. Impacto UI/Unity

```text
Changes UI: NO.
Changes scenes/prefabs/assets: NO.
Requires PlayMode/final human scenario: NO by default; YES DEFERRED if invite UI wired.
```

---

## 20. Riscos

```text
Risco: save section absent.
Mitigação: STOP.

Risco: romance/spouse auto-combat.
Mitigação: explicit guardrail tests.

Risco: pet included as companion.
Mitigação: Pets folder prohibited and service validation.
```

---

# /speckit.tasks

## 21. Tasks

- [ ] T001 — Ler fontes.
- [ ] T002 — Auditar companion/NPC/save systems.
- [ ] T003 — Consolidar state/recruitment contracts.
- [ ] T004 — Implementar availability/validation.
- [ ] T005 — Criar tests.
- [ ] T006 — Rodar validações.
- [ ] T007 — Criar report.

## 23A. Execution Readiness Matrix

| Área | Pergunta obrigatória | Evidência esperada | Status se faltar |
|---|---|---|---|
| Fonte canônica | Executor leu Companions, Pets, City, Farm, Combat, Cave, Economy, UI e Save directions? | Lista no execution report. | PARTIAL |
| Estado real do repo | Sistemas existentes relacionados a companion eligibility/recruitment/state/save foram auditados antes de criar novos? | Comandos `rg` e achados. | PARTIAL |
| Não duplicação | Existe sistema equivalente já implementado/parcial? | REUSE/HARDEN/CREATE/DEFER. | BLOCKED se duplicar |
| Companion ajuda | Companion não joga pelo jogador, não substitui build e não é obrigatório para terminar o jogo? | Tests/checklist. | PARTIAL |
| Pet separado | Pet não foi implementado nem tratado como companion? | Checklist explícito. | BLOCKED se violar |
| Romance separado | Romance/casamento profundo não foi implementado? | Checklist explícito. | BLOCKED se violar |
| Balance | Companion não tanka boss, não cura infinito, não farma loot e respeita active combat budget? | Tests/checklist. | PARTIAL |
| Save/load | Houve alteração de schema? | Declaração NO ou STOP se migration necessária. | BLOCKED se alterar sem migration |
| UI/PlayMode | Há fluxo visual/gameplay integrado? | Cenário final deferido documentado. | BUILD_VALIDATED no máximo se ausente |
| Testes | Lógica determinística nova tem EditMode tests quando praticável? | Test list. | PARTIAL |
| Report | Execution report criado? | `docs/validation/14_spec_companion_eligibility_recruitment_state_save_future_runtime_execution_report.md`. | PARTIAL |

---

## 23B. Local Audit Checklist

Antes de alterar qualquer arquivo, Claude Code/Codex deve rodar e registrar no execution report:

```bash
rg -n "CompanionEligibility|CompanionState|CompanionSave|CanBeFarmCompanion|CanBeCaveCompanion|CanBeRomanceCompanion|Spouse|Breath|Folego" Assets/_Game/Scripts docs/design docs/specs
rg -n "Companion|FarmCompanion|CaveCompanion|CompanionJob|CompanionBrain|CompanionState|CompanionSave|Relationship|Romance|Spouse|Pet|Breath|Folego|Boss|ActiveCombatBudget|JobBoard" Assets/_Game/Scripts docs/design docs/specs
rg -n "TODO|FIXME|HACK|PARTIAL|DEFERRED|BUILD_VALIDATED|ACCEPTED" docs/specs docs/validation docs/IMPLEMENTATION_STATUS.md docs/project/CURRENT_STATE.md
```

Classificar achados:

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
Given companion elegível/desbloqueado/disponível
When o jogador usa o fluxo desta spec
Then companion oferece ajuda limitada e explícita
And não substitui o jogador
And não ativa pet runtime
And não cria romance/casamento profundo.
```

### Scenario 2 — Existing partial implementation

```text
Given já existe implementação parcial no repo
When a execução audita o sistema
Then ela escolhe HARDEN_EXISTING em vez de recriar do zero
And registra divergências do direction
And altera apenas o menor conjunto seguro de arquivos.
```

### Scenario 3 — Balance guardrail

```text
Given companion está em job/fazenda/caverna/combate
When o sistema calcula ajuda, ação ou output
Then há limite por stamina/tempo/cooldown/área/ferramenta/vínculo/capacidade
And companion não gera recurso, cura, loot, dano ou progressão infinita.
```

### Scenario 4 — Pet/romance separation

```text
Given NPC companion, pet future ou romance candidate
When a spec cria contratos de companion
Then pet permanece sistema separado/deferido
And romance/casamento detalhado fica fora do escopo
And spouse não vira combat companion automaticamente.
```

### Scenario 5 — Final human validation deferred

```text
Given o fluxo exige inspeção visual/gameplay de convite, job board, cave entry, HUD ou companion behavior
When a implementação técnica terminar
Then o report registra cenário final em docs/validation/FINAL_HUMAN_VALIDATION_BY_WAVE.md
And não pede validação humana imediata por spec.
```

---

## 23D. Edge Cases and Failure Modes

A execução deve cobrir ou registrar risco residual para:

- Eligibility hardcoded by NPC name.
- Romance candidate becomes combat companion automatically.
- Spouse companion bypasses role rules.
- Pet occupies companion slot.
- Active cave companions exceed 1.
- Injured/exhausted companion accepted.
- Save serializes GameObject/Transform/SO.
- Breath/Fôlego stored or displayed.

---

## 23E. Minimum Execution Report Template

```md
# Execution Report — Companion Eligibility Recruitment State Save Future Runtime

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

## Companion compliance
- Companion helps but does not play for player:
- No pet runtime:
- No romance/deep marriage:
- No Breath/Folego:
- Active combat budget respected:
- Boss guardrails:
- Farm automation limits:
- Save/load safe:

## Implementation decision
- REUSE_EXISTING / HARDEN_EXISTING / CREATE_MINIMAL / DEFER
- Justification:

## Files changed
- ...

## Functional evidence
- Happy path:
- Existing partial implementation:
- Balance guardrail:
- Pet/romance separation:
- Save/load:
- UI/final scenario:

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
3. A implementação exigir save migration sem spec de migration.
4. A implementação criar pet runtime, pet save, pet HUD ou pet data assets.
5. A implementação criar romance/casamento profundo ou spouse system.
6. A implementação tornar companion obrigatório para terminar main quest.
7. A implementação permitir companion tankar boss, curar infinito, matar boss sozinho ou farmar enemies sem jogador ativo.
8. A implementação permitir companion gerar loot/recurso/economia infinita.
9. A implementação adicionar Breath/Fôlego como recurso de companion.
10. A implementação tratar classes de D&D como papéis mecânicos de companion.
11. Não for possível decidir se sistema existente é canônico ou obsoleto.
```



## 25. Validações obrigatórias

Docs:

```powershell
.\tools\docs\validate_docs.ps1
```

Busca local mínima:

```bash
rg -n "Companion|FarmCompanion|CaveCompanion|CompanionJob|CompanionBrain|CompanionState|Relationship|Romance|Spouse|Pet|Breath|Folego|Boss|ActiveCombatBudget" Assets/_Game/Scripts docs/design docs/specs
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
Quando houver cenário visual/gameplay de companion invite, job board, cave entry, HUD, combat assist ou farm jobs, registrar em execution report e vincular a docs/validation/FINAL_HUMAN_VALIDATION_BY_WAVE.md.
```

---

## 26. Testing Quality Gate

- Changed deterministic logic: YES, eligibility/availability/save normalization logic is deterministic.
- Requires EditMode tests: YES for eligibility/recruitment/availability/active-cap/save/no-Breath tests.
- Requires PlayMode automated or final human scenario: NO by default; YES DEFERRED if invite UI is wired.
- Requires regression test: YES if fixing existing companion state bug; otherwise NO.
- Human validation timing: DEFERRED_TO_FINAL_VALIDATION se houver UI/gameplay integrado; caso contrário NOT REQUIRED.
- Minimum validation evidence for ACCEPTED: docs validation PASS; C# build PASS if C# changed; EditMode tests PASS or NOT RUN justified; no pet/romance runtime; active cave companion cap enforced.

---

## 27. Definition of Done

```text
Spec executada sem alterar arquivos proibidos.
Contratos/data/runtime implementados apenas dentro do escopo.
Execution report criado em docs/validation/14_spec_companion_eligibility_recruitment_state_save_future_runtime_execution_report.md.
Fonte/direction coverage preservado.
Validações obrigatórias PASS ou NOT RUN com motivo, impacto e mitigação.
Sem promoção indevida para ACCEPTED apenas por compile.
```

---

## 28. Anti-regressão

```text
Não implementar Pet runtime.
Não implementar romance/casamento profundo.
Não adicionar Breath/Fôlego.
Não usar classes de D&D como papéis mecânicos.
Não tornar companion obrigatório.
Não deixar companion jogar pelo jogador.
Não gerar loot/economia infinita.
Não pedir human test por spec.
Não executar runtime em massa antes da 01Q ou exceção humana explícita.
Não alterar SPEC_EXECUTION_ORDER.md.
```

---

## 29. Notas para execução posterior

Esta spec é future/mapped. Deve ser executada apenas quando City/NPC, Farm, Cave, Combat, Save, UI e Quest estiverem estáveis ou quando houver decisão humana explícita de antecipar Companions.
