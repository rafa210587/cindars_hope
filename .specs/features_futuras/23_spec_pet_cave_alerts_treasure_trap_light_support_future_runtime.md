# SPEC — Pet Cave Alerts Treasure Trap Light Support Future Runtime

> **Spec ID:** `23_spec_pet_cave_alerts_treasure_trap_light_support_future_runtime`  
> **Status:** A implementar / Future mapped / HOLD until explicit pet-scope approval  
> **Wave:** WAVE 23 — Pets Advanced Future  
> **Priority:** P1  
> **Type:** Runtime / Future / HOLD / Pet Cave  
> **Domain:** Pet Cave Entry / Alerts / Treasure Hints / Trap Hints / Light Interrupt  
> **Parallelizable:** NO  
> **Parallel group:** WAVE_23_PETS_ADVANCED_FUTURE  
> **Can run with:** N/A  
> **Must not run with:** qualquer spec que altere enemy AI, combat stats, boss fights, cave generator, companion active combat budget, save migration or scene/prefab assets.  
> **Repo lock scope:** `Assets/_Game/Scripts/Pets/Cave/**`, `Assets/_Game/Scripts/Cave/**`, `Assets/_Game/Scripts/Combat/**`, `Assets/_Game/Tests/EditMode/Pets/**`, `docs/validation/23_spec_pet_cave_alerts_treasure_trap_light_support_future_runtime_execution_report.md`  
> **Depends on:**  
  - `docs/design/SPEC_SOURCE_MAP.md`
  - `docs/design/SPECIFICATION_PROCESS.md`
  - `docs/design/lore/VAALARA_GAME_CANON_DIRECTION_v1.0.md`
  - `.specs/SPEC_IMPLEMENTABLE_TEMPLATE.md`
  - `.specs/SPEC_WAVE_EXECUTION_PROTOCOL.md`
  - `.specs/SPEC_VALIDATION_MATRIX_MASTER.md`
  - `.specs/SPEC_EXISTING_IMPLEMENTATION_AUDIT.md`
  - `docs/project/CURRENT_STATE.md`
  - `docs/design/gameplay/pets/PETS_DIRECTION.md`
  - `docs/design/gameplay/farm/FARM_DESIGN_DIRECTION_v1.3.md`
  - `docs/design/gameplay/farm/FARM_LAYOUT_SCALE_BUILDINGS_DIRECTION.md`
  - `docs/design/gameplay/companions/COMPANIONS_DIRECTION.md`
  - `docs/design/gameplay/combat/COMBAT_CORE_DIRECTION.md`
  - `docs/design/gameplay/combat/STATUS_EFFECTS_DIRECTION.md`
  - `docs/design/gameplay/cave/CAVE_DESIGN_DIRECTION.md`
  - `docs/design/gameplay/cave/CAVE_COMBAT_BALANCE_VULNERABILITIES_DIRECTION.md`
  - `docs/design/gameplay/loot_crafting_economy/LOOT_CRAFTING_ECONOMY_DIRECTION.md`
  - `docs/design/gameplay/loot_crafting_economy/ECONOMY_PRICING_STOCK_REFRESH_DIRECTION.md`
  - `docs/design/gameplay/player/PLAYER_CORE_SYSTEMS_DIRECTION.md`
  - `docs/design/gameplay/player/PLAYER_DERIVED_ATTRIBUTES_DIRECTION.md`
  - `docs/design/gameplay/save_load/SAVE_LOAD_FULL_STATE_DIRECTION.md`
  - `docs/design/gameplay/ui_ux/UI_UX_FULL_GAMEPLAY_DIRECTION.md`
  - `docs/design/gameplay/ui_ux/UI_UX_MENU_SCREEN_FLOWS_DIRECTION.md`
  - `docs/design/gameplay/quests/QUEST_OBJECTIVE_EVENT_SYSTEM_DIRECTION.md`
  - `docs/design/gameplay/quests/QUESTS_MAIN_PROGRESSION_REFINEMENT_DIRECTION.md`
  - `docs/design/gameplay/world/SEASONS_CALENDAR_WEATHER_LUNAR_DIRECTION.md`
  - `docs/design/gameplay/bestiary/BESTIARY_KNOWLEDGE_DISCOVERY_DIRECTION.md`
  - `docs/design/gameplay/social/SOCIAL_RELATIONSHIP_ROMANCE_DIRECTION.md`
> **Blocks:**  
  - pet core;
  - pet HUD;
  - cave combat balance adapter;
  - final cave validation.
> **Scope:** definir contratos futuros de pet na caverna: entrada opcional, alertas, hints, interrupção leve e boss safe state, mantendo HOLD até aprovação explícita.  
> **Out of scope:** enemy AI, combat tuning, boss mechanics, cave generator, visual following, scene/prefab.

---

# /speckit.specify

## 1. Contexto

Baseline futuro permite 1 pet ativo opcional na caverna, se desbloqueado e com energia/humor suficientes. Pet ativo não conta como companion, mas respeita active combat budget; não puxa packs, não sai do leash, usa safe spawn/follow position e respeita boss gates, checkpoints e restrições de run.

---

## 2. Problema

Sem contratos cave:

```text
pet vira segundo combatant;
pet puxa pack novo;
pet detecta tudo como radar perfeito;
pet desarma trap;
pet segura aggro de boss;
pet aplica stun recorrente;
pet revela loot table;
pet morre/desaparece em boss gate;
pet ignora checkpoint/run restrictions.
```

---

## 3. Objetivo

Criar, no futuro aprovado:

```text
PetCaveEligibility;
PetCavePresenceState;
PetCaveAlertType;
PetCaveAlertPolicy;
PetTreasureHintPolicy;
PetTrapHintPolicy;
PetLightInterruptPolicy;
PetBossSafeStatePolicy;
PetCaveBudgetValidator.
```

---

## 4. Regras de design

```text
1 pet ativo opcional no máximo.
Pet não conta como companion, mas respeita active combat budget.
Pet não puxa packs.
Pet fica no leash.
Alertas são condicionais, cooldown-gated e não radar perfeito.
Pet não desarma trap nem abre baú.
Pet não funciona como boss tool.
```

---

## 5. User stories / engineering stories

```text
Como jogador, quero levar pet à caverna por vínculo e legibilidade.
Como cave, quero pet seguro, leve e não dominante.
Como combat, quero interrupção leve sem stun recorrente.
Como exploration, quero hint, não solução.
Como boss, quero pet protegido por recuo/safe state.
```

---

## 6. Escopo

Inclui:

```text
cave eligibility;
presence state;
alert policies;
treasure/trap hints;
light interrupt;
boss safe state;
budget validator;
tests.
```

Não inclui:

```text
enemy AI changes;
combat tuning;
visual follow;
cave generator;
boss mechanics;
scene/prefab.
```

## 7. Modelo de domínio

### 7.1 PetCaveEligibility

```text
PetId
CanEnterCave
RequiredBondLevel
RequiredMood
RequiredEnergy
BlockedByStory
BlockedByBossGate
BlockedByRunRestriction
BlockedByInjuryFuture
```

### 7.2 PetCavePresenceState

```text
NotInCave
FollowingPlayer
CaveActive
Alerting
RestingAtCheckpoint
RetreatingToSafePosition
BossSafeState
UnavailableLowEnergy
LostFallbackToCheckpoint
```

### 7.3 PetCaveAlertType

```text
EnemyNearbyAlert
AmbushHint
TrapHint
TreasureHint
CorruptionReaction
BossFearReaction
LowPlayerStateReaction
```

### 7.4 PetCaveAlertPolicy

```text
AlertType
RequiresBondLevel
RequiresEnergy
ChanceOrCondition
CooldownScope: Room | Level | Day | Run
CanRevealExactPosition false
CanRevealExactLoot false
CanTriggerInBoss false by default
EnergyCost
```

### 7.5 PetLightInterruptPolicy

```text
AllowedEnemyClasses[]
ForbiddenEnemyClasses[]
WorksOnBoss false
WorksOnElite false by default
InterruptKind: BarkCancelSimpleWindup | DistractShortDelay | MiniStaggerNonDamage
Cooldown
EnergyCost
MaxPerEncounter
NoDamage true
NoAggroHold true
```

### 7.6 PetBossSafeStatePolicy

```text
OnBossStart
MoveToSafeAnchor
DisableInterrupts
AllowFearReaction
AllowTelegraphReaction
NoDamageNoAggro
ReturnAfterBossOrCheckpoint
```

---

## 8. Alert rules

```text
Alert is hint, not solution.
Alert has cooldown.
Alert may be probabilistic or condition-based.
Alert never always reveals exact position.
Alert never reveals loot table.
Alert does not replace player perception/attention.
```

---

## 9. Cave support rules

```text
TreasureHint:
  suspicious room/tile/prop, not exact loot table.

TrapHint:
  caution signal, pet does not disarm.

LightInterrupt:
  common enemies only, no boss, no recurring stun, cooldown+energy.

Boss:
  pet retreats/safe state; no tanking/aggro/heal/revive.
```

---

## 10. Criteria

```text
Pet cave contracts exist.
Eligibility checks mood/energy/bond/story/run restrictions.
Alert policy bounded.
Treasure/trap hints are non-solving.
Light interrupt bounded and non-damaging.
Boss safe state exists.
Tests cover eligibility, low energy block, alert cooldown, no exact reveal, trap no disarm, light interrupt common only, boss safe state and no pack pull.
```

# /speckit.plan

## 11. Arquitetura alvo

```text
Assets/_Game/Scripts/Pets/Cave/PetCaveEligibility.cs
Assets/_Game/Scripts/Pets/Cave/PetCavePresenceState.cs
Assets/_Game/Scripts/Pets/Cave/PetCaveAlertType.cs
Assets/_Game/Scripts/Pets/Cave/PetCaveAlertPolicy.cs
Assets/_Game/Scripts/Pets/Cave/PetTreasureHintPolicy.cs
Assets/_Game/Scripts/Pets/Cave/PetTrapHintPolicy.cs
Assets/_Game/Scripts/Pets/Cave/PetLightInterruptPolicy.cs
Assets/_Game/Scripts/Pets/Cave/PetBossSafeStatePolicy.cs
Assets/_Game/Scripts/Pets/Cave/PetCaveBudgetValidator.cs
Assets/_Game/Tests/EditMode/Pets/PetCaveAlertsTreasureTrapLightSupportTests.cs
```

## 12. Arquivos permitidos

```text
Assets/_Game/Scripts/Pets/**
Assets/_Game/Scripts/Cave/**
Assets/_Game/Scripts/Combat/**
Assets/_Game/Scripts/Editor/Validation/**
Assets/_Game/Tests/EditMode/Pets/**
docs/validation/23_spec_pet_cave_alerts_treasure_trap_light_support_future_runtime_execution_report.md
```

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

# /speckit.tasks

## 14. Tasks

- [ ] T001 — Confirmar aprovação explícita de escopo de pets; se ausente, parar BLOCKED_SCOPE.
- [ ] T002 — Ler fontes.
- [ ] T003 — Auditar pets/cave/combat/companion budget.
- [ ] T004 — Consolidar cave contracts.
- [ ] T005 — Implementar validators.
- [ ] T006 — Criar tests.
- [ ] T007 — Rodar validações.
- [ ] T008 — Criar report.

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

- docs/design/gameplay/pets/PETS_DIRECTION.md
- docs/design/gameplay/farm/FARM_DESIGN_DIRECTION_v1.3.md
- docs/design/gameplay/farm/FARM_LAYOUT_SCALE_BUILDINGS_DIRECTION.md
- docs/design/gameplay/companions/COMPANIONS_DIRECTION.md
- docs/design/gameplay/combat/COMBAT_CORE_DIRECTION.md
- docs/design/gameplay/combat/STATUS_EFFECTS_DIRECTION.md
- docs/design/gameplay/cave/CAVE_DESIGN_DIRECTION.md
- docs/design/gameplay/cave/CAVE_COMBAT_BALANCE_VULNERABILITIES_DIRECTION.md
- docs/design/gameplay/loot_crafting_economy/LOOT_CRAFTING_ECONOMY_DIRECTION.md
- docs/design/gameplay/loot_crafting_economy/ECONOMY_PRICING_STOCK_REFRESH_DIRECTION.md
- docs/design/gameplay/player/PLAYER_CORE_SYSTEMS_DIRECTION.md
- docs/design/gameplay/player/PLAYER_DERIVED_ATTRIBUTES_DIRECTION.md
- docs/design/gameplay/save_load/SAVE_LOAD_FULL_STATE_DIRECTION.md
- docs/design/gameplay/ui_ux/UI_UX_FULL_GAMEPLAY_DIRECTION.md
- docs/design/gameplay/ui_ux/UI_UX_MENU_SCREEN_FLOWS_DIRECTION.md
- docs/design/gameplay/quests/QUEST_OBJECTIVE_EVENT_SYSTEM_DIRECTION.md
- docs/design/gameplay/quests/QUESTS_MAIN_PROGRESSION_REFINEMENT_DIRECTION.md
- docs/design/gameplay/world/SEASONS_CALENDAR_WEATHER_LUNAR_DIRECTION.md
- docs/design/gameplay/bestiary/BESTIARY_KNOWLEDGE_DISCOVERY_DIRECTION.md
- docs/design/gameplay/social/SOCIAL_RELATIONSHIP_ROMANCE_DIRECTION.md

### Required interpretation

```text
Esta spec é future/mapped e fica em HOLD.
PETS_DIRECTION declara que pets estão deferidos e que não se deve criar specs/runtime/UI/save/data assets de pets até decisão explícita.
Este arquivo existe como preparação de futuro mapeado; não deve ser executado até aprovação humana explícita para retomar o escopo de pets.
Pet é sistema próprio.
Pet não é companion.
Pet não ocupa slot de companion.
Pet não substitui build do jogador.
Pet não joga pelo jogador.
Pet não pode ser requisito de progressão.
Pet não pode ter Breath/Fôlego.
Pet não pode gerar economia paralela, loot infinito, cura, tanking, revive, boss control ou puzzle solve automático.
Quando houver conflito, PETS_DIRECTION vence para limites de pet; COMPANIONS_DIRECTION vence para companion; CAVE/COMBAT vencem para active combat budget e bosses.
```

---

## Direction / Refinement Coverage

### Covered from directions

- Baseline futuro permite 1 pet ativo opcional na caverna se desbloqueado e com energia/humor suficientes.
- Pet ativo não conta como companion, mas respeita active combat budget.
- Pet não puxa pack novo, não sai do leash e respeita boss gates, checkpoints e restrições de run.
- Alertas incluem EnemyNearbyAlert, AmbushHint, TrapHint, TreasureHint, CorruptionReaction, BossFearReaction e LowPlayerStateReaction.
- Pet não desarma trap, não abre baú, não marca caminho ótimo, não revela loot table e não substitui atenção do jogador.
- Contra bosses, pet é suporte emocional/legibilidade; não tanka, não segura aggro, não cura, não revive, não resolve mecânica central.

### Deferred / future from directions

- Visual follow.
- Enemy AI changes.
- Combat tuning.
- Cave generator.
- Boss mechanics.
- Scene/prefab.

### Explicitly not redefined here

- Companion active budget.
- Enemy AI.
- Boss phase logic.
- Trap system.
- Loot tables.
- Cave generation.

---

## 23A. Execution Readiness Matrix

| Área | Pergunta obrigatória | Evidência esperada | Status se faltar |
|---|---|---|---|
| Escopo explícito | Existe aprovação humana explícita para retomar pets? | Link/commit/registro no execution report. | BLOCKED |
| Fonte canônica | Executor leu PETS_DIRECTION, farm, companions, cave/combat, economy, player, save e UI directions? | Lista no report. | PARTIAL |
| Estado real do repo | Sistemas existentes relacionados a pet cave alerts/treasure/trap/light support foram auditados antes de criar novos? | Comandos `rg` e achados. | PARTIAL |
| Não duplicação | Existe sistema equivalente já implementado/parcial? | REUSE/HARDEN/CREATE/DEFER. | BLOCKED se duplicar |
| Separação de companion | Pet não é companion, não ocupa companion slot e não substitui companion. | Tests/checklist. | BLOCKED se violar |
| Leveza sistêmica | Pet dá apoio leve, legibilidade e vínculo; não joga pelo jogador. | Tests/checklist. | BLOCKED se violar |
| Anti-combate | Pet não tanka boss, não segura aggro, não cura/revive, não causa dano relevante, não resolve mecânica central. | Tests/checklist. | BLOCKED se violar |
| Anti-economia | Pet não gera dinheiro relevante recorrente, loot extra padrão, auto-sell, produção ou arbitragem. | Tests/checklist. | PARTIAL |
| Anti-progressão | Pet não é obrigatório para main quest, cave, farm, boss, Fonte, Mana, final ou bestiary. | Tests/checklist. | BLOCKED se violar |
| Save/load | Houve alteração de schema? | Declaração NO ou STOP se migration necessária. | BLOCKED se alterar sem migration |
| UI/PlayMode | Há fluxo visual/gameplay integrado? | Cenário final deferido documentado. | BUILD_VALIDATED no máximo se ausente |
| Testes | Lógica determinística nova tem EditMode tests quando praticável? | Test list. | PARTIAL |
| Report | Execution report criado? | `docs/validation/23_spec_pet_cave_alerts_treasure_trap_light_support_future_runtime_execution_report.md`. | PARTIAL |

---

## 23B. Local Audit Checklist

Antes de alterar qualquer arquivo, Claude Code/Codex deve rodar e registrar no execution report:

```bash
rg -n "PetCave|PetAlert|TreasureHint|TrapHint|LightInterrupt|BossSafe|PetLeash|ActiveCombatBudget" Assets/_Game/Scripts docs/design .specs
rg -n "Pet|PetData|PetBond|PetMood|PetEnergy|PetRoutine|PetFood|PetBed|PetBowl|PetToy|PetArea|PetHUD|PetAlert|TreasureHint|TrapHint|LightInterrupt|CaveActive|FollowingPlayer|Breath|Folego|Companion|FarmAnimal|Mount" Assets/_Game/Scripts docs/design .specs
rg -n "TODO|FIXME|HACK|PARTIAL|DEFERRED|BUILD_VALIDATED|ACCEPTED" .specs docs/validation docs/IMPLEMENTATION_STATUS.md docs/project/CURRENT_STATE.md
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
Given há aprovação explícita para pets e sistemas fundacionais prontos
When o fluxo de pet cave alerts/treasure/trap/light support roda
Then ele aplica apenas apoio leve, vínculo, rotina ou feedback autorizado
And respeita energia/cooldown/humor
And não substitui companion, jogador, economia, combate ou progressão.
```

### Scenario 2 — Existing partial implementation

```text
Given já existe implementação parcial no repo
When a execução audita o sistema
Then ela escolhe HARDEN_EXISTING em vez de recriar do zero
And registra divergências do direction
And altera apenas o menor conjunto seguro de arquivos.
```

### Scenario 3 — Blocked because pets are still deferred

```text
Given não há aprovação humana explícita para retomar pets
When executor tenta implementar esta spec
Then deve parar com status BLOCKED
And não criar Assets/_Game/Scripts/Pets, UI, save, data assets ou prefabs.
```

### Scenario 4 — Anti-exploit

```text
Given pet tenta alertar, achar item, entrar na caverna, interagir com Fonte/Mana ou ajudar em farm
When energia/cooldown/contexto não permitem
Then ação é skipped/blocked
And nenhum loot/dinheiro/progresso/quest/final é gerado.
```

### Scenario 5 — Final human validation deferred

```text
Given o fluxo exige inspeção visual/gameplay de pet seguindo, HUD, alerta, rotina, cama/tigela, farm/cave behavior ou save/load
When a implementação técnica terminar
Then o report registra cenário final em docs/validation/FINAL_HUMAN_VALIDATION_BY_WAVE.md
And não pede validação humana imediata por spec.
```

---

## 23D. Edge Cases and Failure Modes

A execução deve cobrir ou registrar risco residual para:

- No explicit pet-scope approval.
- Pet becomes radar perfect.
- Pet pulls packs.
- Pet disarms traps.
- Pet opens treasure.
- Pet stuns repeatedly.
- Pet tanks boss/holds aggro.
- Pet reveals loot table or optimal route.

---

## 23E. Minimum Execution Report Template

```md
# Execution Report — Pet Cave Alerts Treasure Trap Light Support Future Runtime

## Summary
- Spec:
- Branch:
- Executor:
- Date:
- Final status:

## Scope approval
- Explicit pet-scope approval:
- Approval source:
- If missing, final status must be BLOCKED:

## Sources read
- ...

## Local audit
- Commands executed:
- Existing systems found:
- Existing partial systems found:
- Missing systems:
- Conflicts:

## Pet compliance
- Pet is not companion:
- Pet does not occupy companion slot:
- Pet does not replace player build:
- Pet does not play for player:
- No Breath/Folego:
- No boss tank/aggro/heal/revive:
- No economy/loot/progression exploit:
- Energy/cooldown used:
- Save/load safe:
- UI not source of truth:

## Implementation decision
- REUSE_EXISTING / HARDEN_EXISTING / CREATE_MINIMAL / DEFER / BLOCKED_SCOPE
- Justification:

## Files changed
- ...

## Functional evidence
- Happy path:
- Existing partial:
- Deferred-scope block:
- Anti-exploit:
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
1. Não existir aprovação humana explícita para retomar escopo de pets.
2. A implementação exigir alterar Packages/ ou ProjectSettings/.
3. A implementação exigir scene/prefab/tilemap/asset/font/icon/audio changes.
4. A implementação alterar save schema sem migration spec.
5. A implementação tratar pet como companion, farm animal ou mount.
6. A implementação dar Breath/Fôlego ao pet.
7. A implementação permitir pet tankar boss, segurar aggro, curar/reviver jogador ou causar dano relevante.
8. A implementação permitir pet resolver puzzle/quest, abrir caminho obrigatório ou completar bestiary sozinho.
9. A implementação gerar loot/dinheiro recorrente relevante, auto-sell, produção ou arbitragem.
10. A implementação tornar pet obrigatório para terminar jogo, vencer boss, farmar, entrar na caverna, acessar final ou progredir.
11. Não for possível decidir se sistema existente é canônico ou obsoleto.
```



---

## 25. Validações obrigatórias

Docs:

```powershell
.\tools\docs\validate_docs.ps1
```

Busca local mínima:

```bash
rg -n "Pet|PetBond|PetMood|PetEnergy|PetRoutine|PetFood|PetBed|PetBowl|PetHUD|PetAlert|TreasureHint|TrapHint|LightInterrupt|Breath|Folego" Assets/_Game/Scripts docs/design .specs
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
Quando houver cenário visual/gameplay de pets, HUD, rotina, cama/tigela, farm/cave hint ou save/load, registrar em execution report e vincular a docs/validation/FINAL_HUMAN_VALIDATION_BY_WAVE.md.
```

---

## 26. Testing Quality Gate

- Changed deterministic logic: YES if executed; cave eligibility/alerts/cooldowns/budget logic is deterministic.
- Requires EditMode tests: YES if executed for eligibility/energy/cooldown/no-exact/trap/interrupt/boss/no-pack tests.
- Requires PlayMode automated or final human scenario: YES, DEFERRED for cave pet visual/gameplay validation.
- Requires regression test: YES if fixing existing pet cave exploit; otherwise NO.
- Human validation timing: DEFERRED_TO_FINAL_VALIDATION se houver UI/gameplay integrado; caso contrário NOT REQUIRED.
- Minimum validation evidence for ACCEPTED: If no explicit pet-scope approval: BLOCKED_SCOPE with no file changes. If approved: docs validation PASS; C# build PASS if C# changed; EditMode tests PASS or NOT RUN justified.

---

## 27. Definition of Done

```text
Spec executada sem alterar arquivos proibidos.
Se escopo de pets ainda estiver deferido, status correto é BLOCKED_SCOPE sem alterar código.
Contratos/data/runtime implementados apenas dentro do escopo.
Execution report criado em docs/validation/23_spec_pet_cave_alerts_treasure_trap_light_support_future_runtime_execution_report.md.
Fonte/direction coverage preservado.
Validações obrigatórias PASS ou NOT RUN com motivo, impacto e mitigação.
Sem promoção indevida para ACCEPTED apenas por compile.
```

---

## 28. Anti-regressão

```text
Não executar sem aprovação explícita de escopo de pets.
Não tratar pet como companion.
Não dar Breath/Fôlego ao pet.
Não fazer pet jogar pelo jogador.
Não gerar economia/loot/progressão obrigatória.
Não tankar boss/curar/reviver/segurar aggro.
Não editar scenes/prefabs/assets.
Não pedir human test por spec.
Não executar runtime em massa antes da 01Q ou exceção humana explícita.
Não alterar SPEC_EXECUTION_ORDER.md.
```

---

## 29. Notas para execução posterior

Esta spec é future/mapped e em HOLD. Ela só deve ser executada quando houver decisão explícita de retomar pets, com atualização do roadmap/registries e validação contra estado real do repo.
