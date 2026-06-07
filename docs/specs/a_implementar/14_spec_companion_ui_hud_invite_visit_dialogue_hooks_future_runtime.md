# SPEC — Companion UI HUD Invite Visit Dialogue Hooks Future Runtime

> **Spec ID:** `14_spec_companion_ui_hud_invite_visit_dialogue_hooks_future_runtime`  
> **Status:** A implementar / Future mapped  
> **Wave:** WAVE 14 — Companions / Jobs / Cave Assist / Future Social Hooks  
> **Priority:** P2  
> **Type:** Runtime / Future / UI Projection / Dialogue Hooks / Farm Visits  
> **Domain:** Companion / UI / HUD / Invite / Visit / Dialogue / Knowledge Hints  
> **Parallelizable:** YES  
> **Parallel group:** WAVE_14_COMPANIONS_FUTURE  
> **Can run with:** docs-only registry updates or research service specs if lock scopes do not overlap.  
> **Must not run with:** qualquer spec que altere UI prefab/layout, dialogue writing, romance/casamento, pet HUD, relationship full runtime, save migration or quest content.  
> **Repo lock scope:** `Assets/_Game/Scripts/UI/Companions/**`, `Assets/_Game/Scripts/Companions/**`, `Assets/_Game/Scripts/Dialogue/**`, `Assets/_Game/Tests/EditMode/UI/**`, `docs/validation/14_spec_companion_ui_hud_invite_visit_dialogue_hooks_future_runtime_execution_report.md`  
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
  - companion invite UI;
  - job board UI;
  - cave loadout UI;
  - relationship/social UI future;
  - bestiary hints from companion.
> **Scope:** definir/endurecer projections e hooks de UI/diálogo/visita de companion, sem criar romance/casamento profundo nem pet HUD.  
> **Out of scope:** prefab/layout final, dialogue text, romance/spouse runtime, pet HUD, relationship full system, quest content.

---

# /speckit.specify

## 1. Contexto

Companion continua sendo NPC da cidade com casa/cama/rotina, pode recusar convite, visitar fazenda se relação/reputação permitir e participar de quests/diálogos. HUD simples deve mostrar estado necessário sem competir com o HUD principal.

Esta spec cria UI projections e hooks.

---

## 2. Problema

Sem projection/hooks:

```text
jogador não sabe por que companion recusou convite;
HUD pode competir com HP/MP/Stamina;
companion social vira romance automaticamente;
pet HUD pode entrar junto;
visitante teleporta sem justificativa;
diálogo de companion revela spoiler;
research hint completa bestiário sozinho;
job/cave state não aparece na UI.
```

---

## 3. Objetivo

Criar/endurecer:

```text
CompanionInviteViewModel;
CompanionAvailabilityReason;
CompanionHudProjection;
CompanionVisitProjection;
CompanionDialogueHook;
CompanionContextHint;
CompanionKnowledgeHintProjection;
CompanionUIValidator.
```

---

## 4. Regras de design

```text
Companion tem rotina própria.
Companion pode recusar por horário, trabalho, sono, evento, ferimento, exaustão, story lock ou relação.
HUD de companion deve ser simples.
Companion/social não implementa romance/casamento profundo.
Pet HUD não entra nesta spec.
Researcher companion pode dar hint, não resolver bestiário.
Dialogue hooks respeitam spoiler.
```

---

## 5. User stories / engineering stories

```text
Como jogador, quero saber se companion está disponível e por quê.
Como UI, quero mostrar HP/MP/Stamina/role/status de companion sem poluição.
Como diálogo, quero opções de convite/job/cave/quest quando válidas.
Como fazenda, quero mostrar visita sem teleport inconsistente.
Como knowledge, quero hints leves e gated.
```

---

## 6. Escopo

Inclui:

```text
invite availability projection;
HUD compact projection;
farm visit projection;
dialogue command hooks;
context hints;
knowledge hint projection;
UI validator;
tests.
```

Não inclui:

```text
UI prefab/layout;
dialogue final text;
relationship full system;
romance/casamento;
pet HUD/runtime;
quest content.
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

- Companion continua sendo NPC da cidade com casa/cama/rotina, pode recusar convite em certos horários/dias, pode ter trabalho/loja/serviço, visitar fazenda e não deve teleportar sem justificativa visual/sistêmica.
- Jogador pode convidar companion por diálogo, quadro de jobs, quest, evento ou agenda combinada.
- Condições de aceite incluem relação mínima, disponibilidade no horário, não estar em quest/evento conflitante, não estar ferido/exausto e não haver bloqueio narrativo.
- Antes de entrar na caverna, UI deve mostrar papel, HP/MP/Stamina, risco, restrições e se companion está ferido/indisponível.
- Companion pode reagir a eventos, oferecer diálogo, lore, bestiary hints e comentários, mas não resolver tudo.
- Pet é sistema separado/deferido e pet HUD não deve ser implementado aqui.

### Deferred / future from directions

- Romance/casamento profundo.
- Full relationship UI.
- Dialogue writing.
- UI prefab/layout final.
- Pet HUD.
- Companion equipment UI completo.

### Explicitly not redefined here

- City schedule runtime.
- Dialogue runtime base.
- Companion state service.
- Quest state/rewards.
- Bestiary knowledge state.
- Input focus/menu systems.

## 7. Modelo de domínio

### 7.1 CompanionInviteViewModel

```text
CompanionId
NpcId
DisplayName
PrimaryRole
SecondaryRole optional
UnlockState
Availability
AvailabilityReasons[]
CanInviteFarm
CanInviteCave
CanInviteQuest
CanAssignJob
CanVisitFarm
RelationshipSummary
StoryLockSummary optional
RiskWarning optional
```

### 7.2 CompanionAvailabilityReason

```text
Available
LowRelationship
WrongTime
Working
Sleeping
InQuestEvent
Injured
Exhausted
StoryLocked
ReputationLocked
AlreadyActiveCompanion
ContextNotAllowed
```

### 7.3 CompanionHudProjection

```text
CompanionId
DisplayName
RoleIcon
Hp
MaxHp
Mp optional
MaxMp optional
Stamina
MaxStamina
Fatigue optional
Status
CurrentMode: FarmJob | CaveAssist | Visiting | Quest | Unavailable
CooldownIndicators[]
Warnings[]
```

### 7.4 CompanionVisitProjection

```text
CompanionId
VisitType
FarmAreaTarget optional
ArrivalTime
DepartureTime
Reason
CanInteract
DialogueSetId optional
NoTeleportJustification
```

### 7.5 CompanionDialogueHook

```text
HookId
NpcId
HookType: InviteFarm | InviteCave | AssignJob | QuestTemporary | AskForHint | Dismiss | CheckStatus
RequiredAvailability
CommandId
RequiresConfirmation
SpoilerTier
```

### 7.6 CompanionKnowledgeHintProjection

```text
CompanionId
ExpertiseTag
HintType
KnowledgeKey optional
Confidence
CanShowExact
TextKey
Cooldown
SpoilerGate
```

---

## 8. UI rules

```text
Invite UI shows why unavailable.
Cave entry UI shows role/resources/risk/restrictions.
Job board UI shows assignment eligibility.
HUD compact; do not compete with player HP/MP/Stamina.
No Breath/Fôlego.
No pet HUD.
No romance/spouse UI beyond allowed flag display.
```

---

## 9. Dialogue hook rules

```text
Dialogue may offer invite if availability allows.
Dialogue may offer job assignment if companion can farm/job.
Dialogue may offer cave invite if companion can cave and is not injured/exhausted.
Dialogue may show hint if expertise and spoiler gate allow.
Dialogue cannot reveal main quest spoiler early.
Dialogue cannot auto-complete quest decision for player.
```

---

## 10. Farm visit rules

```text
Visit requires relation/reputation/quest/service/festival/hook.
Arrival/departure should match schedule.
No unexplained teleport.
Visit may include dialogue/context prompt.
Visit does not imply farm job unless assigned.
```

---

## 11. Criteria

```text
Invite/HUD/visit/dialogue projection contracts exist.
Unavailable reasons are explicit.
HUD compact and no Breath/Fôlego.
No pet HUD/runtime.
No romance/casamento.
Hint projection respects knowledge/spoiler gates.
Tests cover unavailable reasons, cave entry projection, HUD fields, no pet HUD, no Breath, dialogue hook gating and spoiler hint.
```

---

# /speckit.plan

## 12. Arquitetura alvo

```text
Assets/_Game/Scripts/UI/Companions/CompanionInviteViewModel.cs
Assets/_Game/Scripts/UI/Companions/CompanionAvailabilityReason.cs
Assets/_Game/Scripts/UI/Companions/CompanionHudProjection.cs
Assets/_Game/Scripts/UI/Companions/CompanionVisitProjection.cs
Assets/_Game/Scripts/Companions/Dialogue/CompanionDialogueHook.cs
Assets/_Game/Scripts/Companions/Knowledge/CompanionKnowledgeHintProjection.cs
Assets/_Game/Scripts/UI/Companions/CompanionUIValidator.cs
Assets/_Game/Tests/EditMode/UI/CompanionUIInviteHudDialogueTests.cs
```

Consolidar existentes se houver.

---

## 13. Arquivos permitidos

```text
Assets/_Game/Scripts/UI/Companions/**
Assets/_Game/Scripts/Companions/**
Assets/_Game/Scripts/Dialogue/**
Assets/_Game/Scripts/Quests/**
Assets/_Game/Scripts/Bestiary/**
Assets/_Game/Scripts/Editor/Validation/**
Assets/_Game/Tests/EditMode/UI/**
docs/validation/14_spec_companion_ui_hud_invite_visit_dialogue_hooks_future_runtime_execution_report.md
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
1. Auditar companion UI/dialogue/hud hooks.
2. Consolidar view models/projections.
3. Implementar validators for no Breath/no pet/no romance/no spoiler.
4. Integrar knowledge hint projection read-only.
5. Criar tests.
6. Criar report.
```

---

## 16. Paralelização

- Parallelizable: YES
- Can run with:
  - docs-only reconciliation;
  - knowledge UI projections if no same files.
- Must not run with:
  - companion base state;
  - dialogue runtime rewrite;
  - UI prefab/layout final;
  - romance/social system;
  - pet HUD.
- Reason: projection layer only, but consumes many states.

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
Requires unsubscribe pattern: YES if reactive UI subscribers are created.
```

---

## 19. Impacto UI/Unity

```text
Changes UI data/projection: YES.
Changes prefabs/layout: NO.
Changes scenes/prefabs/assets: NO.
Requires PlayMode/final human scenario: YES, DEFERRED for invite/HUD/dialogue flow.
```

---

## 20. Riscos

```text
Risco: UI implies unavailable companion is bug.
Mitigação: explicit reason.

Risco: hint leaks spoiler.
Mitigação: knowledge/spoiler gates.

Risco: pet HUD sneaks in.
Mitigação: Pets folder prohibited.
```

---

# /speckit.tasks

## 21. Tasks

- [ ] T001 — Ler fontes.
- [ ] T002 — Auditar companion UI/dialogue hooks.
- [ ] T003 — Consolidar view models.
- [ ] T004 — Implementar validators.
- [ ] T005 — Criar tests.
- [ ] T006 — Rodar validações.
- [ ] T007 — Criar report.

## 23A. Execution Readiness Matrix

| Área | Pergunta obrigatória | Evidência esperada | Status se faltar |
|---|---|---|---|
| Fonte canônica | Executor leu Companions, Pets, City, Farm, Combat, Cave, Economy, UI e Save directions? | Lista no execution report. | PARTIAL |
| Estado real do repo | Sistemas existentes relacionados a companion UI/HUD/invite/visit/dialogue hooks foram auditados antes de criar novos? | Comandos `rg` e achados. | PARTIAL |
| Não duplicação | Existe sistema equivalente já implementado/parcial? | REUSE/HARDEN/CREATE/DEFER. | BLOCKED se duplicar |
| Companion ajuda | Companion não joga pelo jogador, não substitui build e não é obrigatório para terminar o jogo? | Tests/checklist. | PARTIAL |
| Pet separado | Pet não foi implementado nem tratado como companion? | Checklist explícito. | BLOCKED se violar |
| Romance separado | Romance/casamento profundo não foi implementado? | Checklist explícito. | BLOCKED se violar |
| Balance | Companion não tanka boss, não cura infinito, não farma loot e respeita active combat budget? | Tests/checklist. | PARTIAL |
| Save/load | Houve alteração de schema? | Declaração NO ou STOP se migration necessária. | BLOCKED se alterar sem migration |
| UI/PlayMode | Há fluxo visual/gameplay integrado? | Cenário final deferido documentado. | BUILD_VALIDATED no máximo se ausente |
| Testes | Lógica determinística nova tem EditMode tests quando praticável? | Test list. | PARTIAL |
| Report | Execution report criado? | `docs/validation/14_spec_companion_ui_hud_invite_visit_dialogue_hooks_future_runtime_execution_report.md`. | PARTIAL |

---

## 23B. Local Audit Checklist

Antes de alterar qualquer arquivo, Claude Code/Codex deve rodar e registrar no execution report:

```bash
rg -n "CompanionInvite|CompanionHud|CompanionVisit|CompanionDialogue|AvailabilityReason|KnowledgeHint|Romance|Spouse|PetHUD|Breath" Assets/_Game/Scripts docs/design docs/specs
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

- Unavailable companion has no reason shown.
- HUD competes with player HUD.
- Pet HUD implemented accidentally.
- Breath/Fôlego shown in companion HUD.
- Romance/spouse UI implied as deep system.
- Dialogue hint reveals main quest spoiler.
- Visit teleports without schedule/justification.
- UI command mutates state directly.

---

## 23E. Minimum Execution Report Template

```md
# Execution Report — Companion UI HUD Invite Visit Dialogue Hooks Future Runtime

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

- Changed deterministic logic: YES, projection/gating/validator logic is deterministic.
- Requires EditMode tests: YES for availability/HUD/no-pet/no-Breath/dialogue/spoiler tests.
- Requires PlayMode automated or final human scenario: YES, DEFERRED for companion invite/HUD/dialogue visual validation.
- Requires regression test: YES if fixing existing companion UI/hint bug; otherwise NO.
- Human validation timing: DEFERRED_TO_FINAL_VALIDATION se houver UI/gameplay integrado; caso contrário NOT REQUIRED.
- Minimum validation evidence for ACCEPTED: docs validation PASS; C# build PASS if C# changed; EditMode tests PASS or NOT RUN justified; UI projection safe and no pet/romance runtime.

---

## 27. Definition of Done

```text
Spec executada sem alterar arquivos proibidos.
Contratos/data/runtime implementados apenas dentro do escopo.
Execution report criado em docs/validation/14_spec_companion_ui_hud_invite_visit_dialogue_hooks_future_runtime_execution_report.md.
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
