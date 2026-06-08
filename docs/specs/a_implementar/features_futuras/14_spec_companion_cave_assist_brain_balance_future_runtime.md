# SPEC — Companion Cave Assist Brain Balance Future Runtime

> **Spec ID:** `14_spec_companion_cave_assist_brain_balance_future_runtime`  
> **Status:** A implementar / Future mapped  
> **Wave:** WAVE 14 — Companions / Jobs / Cave Assist / Future Social Hooks  
> **Priority:** P1  
> **Type:** Runtime / Future / Cave Assist / Companion AI / Balance  
> **Domain:** Companion / Cave Entry / Brain / Assist / Combat Budget / Downed State  
> **Parallelizable:** NO  
> **Parallel group:** WAVE_14_COMPANIONS_FUTURE  
> **Can run with:** N/A  
> **Must not run with:** qualquer spec que altere enemy AI core, combat formulas, cave procedural snapshot, boss fights, pet cave runtime, save migration, companion equipment or UI visual final.  
> **Repo lock scope:** `Assets/_Game/Scripts/Companions/**`, `Assets/_Game/Scripts/Cave/**`, `Assets/_Game/Scripts/Combat/**`, `Assets/_Game/Tests/EditMode/Companions/**`, `docs/validation/14_spec_companion_cave_assist_brain_balance_future_runtime_execution_report.md`  
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
  - cave entry loadout UI;
  - companion HUD;
  - combat balance validation;
  - enemy target priority adapters;
  - knowledge/research hints.
> **Scope:** definir/endurecer companion de caverna: seleção, follow/leash, brain states, assist actions, downed/injured, balance e boss guardrails.  
> **Out of scope:** enemy AI rewrite, boss AI, combat formulas, pet cave runtime, procedural generation, visual animations.

---

# /speckit.specify

## 1. Contexto

Caverna recomenda no máximo 1 companion ativo; companion é vantagem/opção, não requisito universal. Companion deve funcionar com snapshot/replay da run, procedural level, checkpoints, boss gates, safe spawn, confinement walls, leash e fog/reveal se existir.

Esta spec cria contratos de cave assist/brain/balance.

---

## 2. Problema

Sem cave assist guardrails:

```text
companion vira party completa;
companion puxa sala nova;
companion tanka boss;
healer cura infinito;
DPS supera player;
companion morre/perde estado no reload;
companion trava em procedural cave;
enemy AI ignora active combat budget;
pet entra como combat companion junto.
```

---

## 3. Objetivo

Criar/endurecer:

```text
CompanionCaveEntryState;
CompanionBrainProfile;
CompanionBrainState;
CompanionAssistAction;
CompanionLeashPolicy;
CompanionResourceRules;
CompanionDownedInjuredState;
CompanionCombatBudgetAdapter;
CompanionCaveRunState.
```

---

## 4. Regras de design

```text
Caverna deve permitir entrar sem companion.
Caverna assume max 1 companion ativo.
Companion deve seguir jogador, evitar ficar preso e usar warp curto só como fallback técnico.
Companion não abre baú/porta sozinho por padrão.
Companion não ativa trap voluntariamente.
Companion não puxa pack novo.
Companion não mata boss sozinho.
Companion healer cura pouco/moderado com cooldown/MP.
DPS comum 15%-35%, ofensivo especializado 35%-50% com risco/cooldown/fragilidade.
```

---

## 5. User stories / engineering stories

```text
Como jogador, quero escolher companion antes de entrar na caverna ou entrar sozinho.
Como companion, quero seguir/leash/retreat sem travar.
Como combat, quero companion útil mas não dominante.
Como enemy AI, quero target priority compatível com active combat budget.
Como save/load, quero preservar companion state dentro da run se snapshot estável existir.
```

---

## 6. Escopo

Inclui:

```text
cave entry state;
brain profiles/states;
follow/leash/safe spawn;
assist actions;
healing/damage/buff guardrails;
downed/injured state;
combat budget adapter;
tests.
```

Não inclui:

```text
enemy AI rewrite;
boss mechanics;
combat formula tuning;
animations;
pet runtime;
procedural cave changes.
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

- Caverna: 1 companion ativo; fazenda pode ter múltiplos visitantes/jobs; cidade segue agenda própria.
- Caverna deve permitir entrar sem companion; companion é vantagem/opção, não requisito universal.
- Companion deve seguir jogador, evitar preso, usar warp curto só como fallback, manter distância por papel, reagir a combate, respeitar leash, não abrir baú/porta sozinho e não acionar trap voluntariamente.
- Companion deve funcionar com snapshot/replay, level procedural, checkpoints, boss gates, safe spawn, confinement walls, leash e fog/reveal.
- CompanionBrain tem PrimaryRole, SecondaryRole, FollowDistance, CombatDistance, RetreatThreshold, AssistPriority, TargetPriority, AllowedActions, ForbiddenActions, CooldownRules, ResourceRules, HazardAvoidanceRules, LeashRules e ReviveOrRetreatRules.
- Companion não deve aggrar salas novas, tankar boss, curar sem limite, matar boss sozinho ou farmar enemies sem player ativo.

### Deferred / future from directions

- Party multi-companion.
- Comandos táticos complexos.
- AI squad avançada.
- Companion equipment completo.
- Pet cave runtime.
- Boss-specific companion mechanics.

### Explicitly not redefined here

- Enemy AI core.
- Combat formulas.
- Cave generation/snapshot.
- Player build/skill tree.
- Pet system.
- Boss fights.

## 7. Modelo de domínio

### 7.1 CompanionCaveEntryState

```text
NoCompanion
SelectedAvailable
SelectedUnavailable
SelectedInjured
SelectedExhausted
SelectedStoryBlocked
Ready
```

### 7.2 CompanionBrainState

```text
Idle
FollowPlayer
ExploreFollow
CombatAssist
DefensiveAssist
HealingAssist
Retreat
Downed
InjuredUnavailable
QuestScripted
```

### 7.3 CompanionBrainProfile

```text
CompanionBrainProfileId
PrimaryRole
SecondaryRole
FollowDistance
CombatDistance
RetreatThreshold
AssistPriority
TargetPriority
AllowedActions[]
ForbiddenActions[]
CooldownRules[]
ResourceRules[]
HazardAvoidanceRules[]
LeashRules
ReviveOrRetreatRules
```

### 7.4 CompanionAssistAction

```text
ActionId
ActionType: LightAttack | Guard | Intercept | Heal | Buff | Debuff | Alert | LoreComment | ResearchHint
RequiredRole
StaminaCost
MpCost
Cooldown
TargetPolicy
AllowedEnemyTypes
ForbiddenEnemyTypes
BossPolicy
ActiveCombatBudgetCost
```

### 7.5 CompanionCaveRunState

```text
RunId
CompanionId
CurrentBrainState
Hp
Mp
Stamina
Fatigue
Downed
Injured
LastSafePosition
LeashState
CooldownStates
AssistHistory
```

---

## 8. Entry rules

```text
Player may enter cave without companion.
Only 1 active cave companion.
Companion must be available, not injured/exhausted and eligible for cave.
Show role, HP/MP/Stamina, risk and restrictions before entry.
Pet is separate and not part of this spec.
```

---

## 9. AI priority rules

```text
1. survive / leave hazard;
2. respect player leash;
3. protect self if low HP;
4. use healing/support if role allows and cooldown/resource available;
5. attack safe/priority target;
6. help against enemy threatening player;
7. avoid pulling new pack;
8. do not chase outside safe area.
```

---

## 10. Balance rules

```text
Common DPS: 15%-35% of expected player DPS.
Offensive specialist: 35%-50%, with fragility/cooldown/risk.
Healer/support: low DPS.
Healing uses MP/cooldown and cannot be infinite.
Boss: companion cannot tank, hold aggro indefinitely, kill boss alone or bypass mechanics.
Loot: companion does not create extra loot.
Farm: companion does not farm enemies without player active.
```

---

## 11. Downed/injured rules

```text
Downed companion stops assisting.
Downed companion should retreat/be protected if possible.
After run, downed may become InjuredUnavailable for cooldown days.
Injured does not imply permadeath baseline.
Player should not lose main progression due to companion downed state.
```

---

## 12. Criteria

```text
Cave entry contracts exist.
Companion brain profile/states exist.
Assist actions have resource/cooldown/budget costs.
Leash/safe spawn/fallback policy exists.
Boss guardrails enforced.
No pet runtime.
Tests cover entry eligibility, active cap, follow/leash, no new pack aggro, healer cooldown, DPS/budget cap, downed/injured, boss guardrail.
```

---

# /speckit.plan

## 13. Arquitetura alvo

```text
Assets/_Game/Scripts/Companions/Cave/CompanionCaveEntryState.cs
Assets/_Game/Scripts/Companions/Cave/CompanionBrainState.cs
Assets/_Game/Scripts/Companions/Cave/CompanionBrainProfile.cs
Assets/_Game/Scripts/Companions/Cave/CompanionAssistAction.cs
Assets/_Game/Scripts/Companions/Cave/CompanionCaveRunState.cs
Assets/_Game/Scripts/Companions/Cave/CompanionCaveAssistService.cs
Assets/_Game/Scripts/Companions/Cave/CompanionCombatBudgetValidator.cs
Assets/_Game/Tests/EditMode/Companions/CompanionCaveAssistBrainBalanceTests.cs
```

Consolidar existentes se houver.

---

## 14. Arquivos permitidos

```text
Assets/_Game/Scripts/Companions/**
Assets/_Game/Scripts/Cave/**
Assets/_Game/Scripts/Combat/**
Assets/_Game/Scripts/Enemies/**
Assets/_Game/Scripts/Save/**
Assets/_Game/Scripts/Editor/Validation/**
Assets/_Game/Tests/EditMode/Companions/**
docs/validation/14_spec_companion_cave_assist_brain_balance_future_runtime_execution_report.md
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
1. Auditar companion/cave/combat/enemy references.
2. Consolidar cave entry/brain/assist/action contracts.
3. Implementar guardrails/budget validators.
4. Integrar save state only if existing; otherwise STOP.
5. Criar tests.
6. Criar report.
```

---

## 17. Paralelização

- Parallelizable: NO
- Must not run with:
  - combat formula changes;
  - enemy AI rewrite;
  - cave procedural snapshot changes;
  - companion state foundation;
  - pet runtime.
- Reason: cave companion touches combat/cave balance.

---

## 18. Impacto save/load

```text
Does this change save schema? SHOULD BE NO if companion cave run state exists; otherwise STOP.
Does this add save section? NO unless migration approved.
Does this require migration? NO unless adding persisted cave run companion state; then STOP.
Does this persist Unity references? NO.
```

---

## 19. Impacto eventos

```text
Adds events: CONDITIONAL, e.g. CompanionDownedEvent, CompanionAssistUsedEvent.
Changes events: SHOULD BE NO.
Requires unsubscribe pattern: YES if subscribers are created.
```

---

## 20. Impacto UI/Unity

```text
Changes UI: NO final; exposes cave entry/HUD state.
Changes scenes/prefabs/assets: NO.
Requires PlayMode/final human scenario: YES, DEFERRED for cave companion flow.
```

---

## 21. Riscos

```text
Risco: companion dominates combat.
Mitigação: DPS/budget tests.

Risco: companion stuck in procedural map.
Mitigação: leash/safe spawn fallback.

Risco: companion pulls packs.
Mitigação: target/leash guardrails.
```

---

# /speckit.tasks

## 22. Tasks

- [ ] T001 — Ler fontes.
- [ ] T002 — Auditar cave/combat/enemy companion hooks.
- [ ] T003 — Consolidar entry/brain/assist contracts.
- [ ] T004 — Implementar validators.
- [ ] T005 — Criar tests.
- [ ] T006 — Rodar validações.
- [ ] T007 — Criar report.

## 23A. Execution Readiness Matrix

| Área | Pergunta obrigatória | Evidência esperada | Status se faltar |
|---|---|---|---|
| Fonte canônica | Executor leu Companions, Pets, City, Farm, Combat, Cave, Economy, UI e Save directions? | Lista no execution report. | PARTIAL |
| Estado real do repo | Sistemas existentes relacionados a companion cave assist/brain/balance foram auditados antes de criar novos? | Comandos `rg` e achados. | PARTIAL |
| Não duplicação | Existe sistema equivalente já implementado/parcial? | REUSE/HARDEN/CREATE/DEFER. | BLOCKED se duplicar |
| Companion ajuda | Companion não joga pelo jogador, não substitui build e não é obrigatório para terminar o jogo? | Tests/checklist. | PARTIAL |
| Pet separado | Pet não foi implementado nem tratado como companion? | Checklist explícito. | BLOCKED se violar |
| Romance separado | Romance/casamento profundo não foi implementado? | Checklist explícito. | BLOCKED se violar |
| Balance | Companion não tanka boss, não cura infinito, não farma loot e respeita active combat budget? | Tests/checklist. | PARTIAL |
| Save/load | Houve alteração de schema? | Declaração NO ou STOP se migration necessária. | BLOCKED se alterar sem migration |
| UI/PlayMode | Há fluxo visual/gameplay integrado? | Cenário final deferido documentado. | BUILD_VALIDATED no máximo se ausente |
| Testes | Lógica determinística nova tem EditMode tests quando praticável? | Test list. | PARTIAL |
| Report | Execution report criado? | `docs/validation/14_spec_companion_cave_assist_brain_balance_future_runtime_execution_report.md`. | PARTIAL |

---

## 23B. Local Audit Checklist

Antes de alterar qualquer arquivo, Claude Code/Codex deve rodar e registrar no execution report:

```bash
rg -n "CompanionBrain|CaveCompanion|CompanionAssist|FollowPlayer|CombatAssist|HealingAssist|Leash|Downed|ActiveCombatBudget|Boss" Assets/_Game/Scripts docs/design docs/specs
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

- Companion selected when injured/exhausted.
- More than 1 cave companion active.
- Companion pulls new pack.
- Companion tanks/soloes boss.
- Healer cures without MP/cooldown.
- DPS exceeds budget.
- Companion state lost in cave snapshot.
- Pet cave runtime added accidentally.

---

## 23E. Minimum Execution Report Template

```md
# Execution Report — Companion Cave Assist Brain Balance Future Runtime

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

- Changed deterministic logic: YES, cave assist/brain/budget logic is deterministic.
- Requires EditMode tests: YES for entry/active-cap/leash/no-pack/healer/budget/downed/boss tests.
- Requires PlayMode automated or final human scenario: YES, DEFERRED for cave companion gameplay validation.
- Requires regression test: YES if fixing existing companion cave assist bug; otherwise NO.
- Human validation timing: DEFERRED_TO_FINAL_VALIDATION se houver UI/gameplay integrado; caso contrário NOT REQUIRED.
- Minimum validation evidence for ACCEPTED: docs validation PASS; C# build PASS if C# changed; EditMode tests PASS or NOT RUN justified; companion does not dominate cave.

---

## 27. Definition of Done

```text
Spec executada sem alterar arquivos proibidos.
Contratos/data/runtime implementados apenas dentro do escopo.
Execution report criado em docs/validation/14_spec_companion_cave_assist_brain_balance_future_runtime_execution_report.md.
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
