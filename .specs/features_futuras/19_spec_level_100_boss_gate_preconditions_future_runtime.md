# SPEC — Level 100 Boss Gate Preconditions Future Runtime

> **Spec ID:** `19_spec_level_100_boss_gate_preconditions_future_runtime`  
> **Status:** A implementar / Future mapped  
> **Wave:** WAVE 19 — Level 100/101 Endgame Future  
> **Priority:** P1  
> **Type:** Runtime / Future / Main Quest Gate / Cave Boss Gate  
> **Domain:** Level 100 / Boss Gate / Preconditions / Fragment Checks / Unique Reward Lock  
> **Parallelizable:** NO  
> **Parallel group:** WAVE_19_LEVEL_100_101_ENDGAME_FUTURE  
> **Can run with:** N/A  
> **Must not run with:** qualquer spec que altere cave procedural generation, final boss AI, level 101 sequence, final choice, save migration, scene assets or boss reward tables.  
> **Repo lock scope:** `Assets/_Game/Scripts/MainProgression/Endgame/**`, `Assets/_Game/Scripts/Cave/Gates/**`, `Assets/_Game/Scripts/Quests/**`, `Assets/_Game/Tests/EditMode/MainProgression/**`, `docs/validation/19_spec_level_100_boss_gate_preconditions_future_runtime_execution_report.md`  
> **Depends on:**  
  - `docs/design/SPEC_SOURCE_MAP.md`
  - `docs/design/SPECIFICATION_PROCESS.md`
  - `docs/design/lore/VAALARA_GAME_CANON_DIRECTION_v1.0.md`
  - `.specs/SPEC_IMPLEMENTABLE_TEMPLATE.md`
  - `.specs/SPEC_WAVE_EXECUTION_PROTOCOL.md`
  - `.specs/SPEC_VALIDATION_MATRIX_MASTER.md`
  - `.specs/SPEC_EXISTING_IMPLEMENTATION_AUDIT.md`
  - `docs/project/CURRENT_STATE.md`
  - `docs/design/gameplay/quests/QUEST_OBJECTIVE_EVENT_SYSTEM_DIRECTION.md`
  - `docs/design/gameplay/quests/QUESTS_MAIN_LORE_DIRECTION.md`
  - `docs/design/gameplay/quests/QUESTS_MAIN_PROGRESSION_REFINEMENT_DIRECTION.md`
  - `docs/design/gameplay/cave/CAVE_DESIGN_DIRECTION.md`
  - `docs/design/gameplay/cave/CAVE_MONSTER_ROSTER_DIRECTION.md`
  - `docs/design/gameplay/cave/CAVE_COMBAT_BALANCE_VULNERABILITIES_DIRECTION.md`
  - `docs/design/gameplay/cave/CAVE_MONSTER_ROSTER_ENEMY_BEHAVIOR_ADAPTER.md`
  - `docs/design/gameplay/save_load/SAVE_LOAD_FULL_STATE_DIRECTION.md`
  - `docs/design/gameplay/ui_ux/UI_UX_FULL_GAMEPLAY_DIRECTION.md`
  - `docs/design/gameplay/ui_ux/UI_UX_MENU_SCREEN_FLOWS_DIRECTION.md`
  - `docs/design/gameplay/world/SEASONS_CALENDAR_WEATHER_LUNAR_DIRECTION.md`
  - `docs/design/gameplay/farm/FARM_DESIGN_DIRECTION_v1.3.md`
  - `docs/design/gameplay/farm/FARM_LAYOUT_SCALE_BUILDINGS_DIRECTION.md`
  - `docs/design/gameplay/city/CITY_DESIGN_DIRECTION_v1.2.md`
  - `docs/design/gameplay/city/CITY_NPC_ROSTER_SERVICES_DIRECTION_v1.1.md`
  - `docs/design/gameplay/city/CITY_LAYOUT_BUILDINGS_SCHEDULE_DIRECTION.md`
  - `docs/design/gameplay/loot_crafting_economy/LOOT_CRAFTING_ECONOMY_DIRECTION.md`
  - `docs/design/gameplay/loot_crafting_economy/ECONOMY_PRICING_STOCK_REFRESH_DIRECTION.md`
  - `docs/design/gameplay/magic/MAGIC_SPELLS_ACTIONS_DIRECTION.md`
  - `docs/design/gameplay/magic/MAGIC_LEARNING_UNLOCKS_SOURCES_DIRECTION.md`
  - `docs/design/gameplay/combat/COMBAT_CORE_DIRECTION.md`
  - `docs/design/gameplay/combat/STATUS_EFFECTS_DIRECTION.md`
  - `docs/design/gameplay/bestiary/BESTIARY_KNOWLEDGE_DISCOVERY_DIRECTION.md`
  - `docs/design/gameplay/companions/COMPANIONS_DIRECTION.md`
  - `docs/design/gameplay/pets/PETS_DIRECTION.md`
  - `docs/design/gameplay/social/SOCIAL_RELATIONSHIP_ROMANCE_DIRECTION.md`
> **Blocks:**  
  - level 101 access sequence;
  - level 101 portal return;
  - final choice handoff;
  - postgame world modifiers.
> **Scope:** definir/endurecer gate 100: pré-condições, gates anteriores, fragmentos/chaves/vestígios, boss gate final e unlock idempotente do nível 101.  
> **Out of scope:** boss AI final, encounter tuning, scene/prefab/layout, cinematic, postgame modifiers, save migration.

---

# /speckit.specify

## 1. Contexto

O Ato 4 começa quando o jogador abre o boss gate do nível 100, derrota o guardião/chefe do portão e acessa o nível 101. O gate 100 é diferente dos anteriores: fecha a escalada da caverna, exige gates anteriores completed e pode exigir fragmentos/chaves/vestígios de lore coletados. Derrotar o boss do nível 100 abre caminho ao nível 101, sem recompensa única repetida e sem farm infinito.

---

## 2. Problema

Sem contrato de gate:

```text
jogador acessa 101 cedo;
gate 100 vira só checkpoint normal;
gates anteriores não são exigidos;
fragmentos/lore/chaves são ignorados;
boss reward duplica no reload;
unlock do 101 pode regredir;
UI revela Arquivista/final antes da hora;
boss gate 100 vira farm infinito.
```

---

## 3. Objetivo

Criar/endurecer:

```text
Level100GateState;
Level100GatePrecondition;
Level100GateUnlockResult;
Level100BossGateRewardLock;
Level101AccessUnlockFlag;
Level100GateHintProjection;
Level100GateValidator.
```

---

## 4. Regras de design

```text
Gate 100 não é checkpoint comum.
Gate 100 exige gates anteriores completed.
Gate 100 pode exigir fragmentos/chaves/vestígios.
Boss do gate 100 libera acesso ao nível 101.
Reward único não repete.
Gate hint não revela tudo do 101.
```

---

## 5. User stories / engineering stories

```text
Como jogador, quero entender por que o gate 100 ainda está fechado sem spoiler.
Como main quest, quero garantir ordem canônica do Ato 4.
Como cave, quero diferenciar gate 100 de checkpoint comum.
Como save/load, quero idempotência de boss reward e unlock.
Como UI, quero mostrar hints seguros.
```

---

## 6. Escopo

Inclui:

```text
gate state;
precondition contracts;
unlock result;
reward lock;
safe hint projection;
idempotent unlock flag;
tests.
```

Não inclui:

```text
boss AI;
boss arena;
final reward tables;
level 101 sequence;
final choice;
postgame modifiers.
```

## 7. Modelo de domínio

### 7.1 Level100GateState

```text
Unknown
Locked
VisibleLocked
PreconditionsKnown
ReadyToChallenge
BossActive
BossDefeated
Level101Unlocked
ConsumedIntoEndgame
```

### 7.2 Level100GatePreconditionType

```text
PreviousBossGatesCompleted
DeepestLayerReached
RequiredFragmentsRecovered
RequiredLoreVestigesCollected
RequiredQuestFlags
RequiredFonteStage
RequiredMoonEventKnown
RequiredKeyItems
BossGateUnlockedByStory
```

### 7.3 Level100GatePrecondition

```text
PreconditionId
Type
RequiredIds[]
RequiredCount
CurrentCount
Satisfied
SafeHintTextKey
SpoilerTier
CanShowExact
```

### 7.4 Level100GateUnlockResult

```text
CanOpen
BlockedReasons[]
SatisfiedPreconditions[]
GateStateBefore
GateStateAfter
StartsBossGate
UnlocksLevel101OnBossDefeat
WarningTextKey
```

### 7.5 Level100BossGateRewardLock

```text
BossGateId
BossDefeated
UniqueRewardGranted
Level101AccessGranted
RewardGrantId
FirstDefeatedDay
CannotGrantAgain
```

---

## 8. Preconditions baseline

```text
Required:
  Previous boss gates completed.
  DeepestLayerReached >= 100 or equivalent cave gate state.
  Fragmento da Água recovered.
  Fragmento da Memória recovered.
  Fragmento da Vida recovered.
  Main quest Ato 4 visible.
  Relevant lore vestiges discovered enough to frame the gate.

Optional/future:
  key item from Vaelrion/Sethra route;
  lunar alignment known/discovered;
  Fonte stage ready for Esperança;
  corrupted Living Water purification flag.
```

---

## 9. Hint rules

```text
If previous gates missing:
  show generic "camadas anteriores ainda resistem".

If fragment missing:
  show fragment-safe hint, not final spoiler.

If lore vestige missing:
  show "a memória da caverna ainda está incompleta".

If lunar gate hidden:
  show rumor/unknown until discovered.

Never show:
  Arquivista do Silêncio;
  full Level 101 composition;
  final choice;
  true Anya explanation.
```

---

## 10. Criteria

```text
Gate100 contracts exist.
Preconditions are deterministic and testable.
Gate 100 differentiated from checkpoint.
Boss reward/unlock idempotent.
Level101 unlock flag monotonic.
Hints safe.
Tests cover missing previous gates, missing fragment, missing lore, ready state, boss defeat unlock, reload no duplicate reward and no early spoiler.
```

# /speckit.plan

## 11. Arquitetura alvo

```text
Assets/_Game/Scripts/MainProgression/Endgame/Level100GateState.cs
Assets/_Game/Scripts/MainProgression/Endgame/Level100GatePreconditionType.cs
Assets/_Game/Scripts/MainProgression/Endgame/Level100GatePrecondition.cs
Assets/_Game/Scripts/MainProgression/Endgame/Level100GateUnlockResult.cs
Assets/_Game/Scripts/MainProgression/Endgame/Level100BossGateRewardLock.cs
Assets/_Game/Scripts/MainProgression/Endgame/Level100GateValidator.cs
Assets/_Game/Scripts/UI/Endgame/Level100GateHintProjection.cs
Assets/_Game/Tests/EditMode/MainProgression/Level100BossGatePreconditionsTests.cs
```

Consolidar existentes se houver.

## 12. Arquivos permitidos

```text
Assets/_Game/Scripts/MainProgression/Endgame/**
Assets/_Game/Scripts/Cave/Gates/**
Assets/_Game/Scripts/Quests/**
Assets/_Game/Scripts/UI/Endgame/**
Assets/_Game/Scripts/Editor/Validation/**
Assets/_Game/Tests/EditMode/MainProgression/**
docs/validation/19_spec_level_100_boss_gate_preconditions_future_runtime_execution_report.md
```

## 13. Arquivos proibidos

```text
Packages/**
ProjectSettings/**
Assets/**/*.unity
Assets/**/*.prefab
Assets/**/*.asset
Assets/_Game/Scripts/Pets/**
.specs/SPEC_EXECUTION_ORDER.md
.specs/implementados/**
docs/refinements/implementados/**
docs/project/CURRENT_STATE.md
PROJECT_LOG.md
```

# /speckit.tasks

## 14. Tasks

- [ ] T001 — Ler fontes.
- [ ] T002 — Auditar cave gate/main quest state.
- [ ] T003 — Consolidar gate100 contracts.
- [ ] T004 — Implementar validator/reward lock.
- [ ] T005 — Criar tests.
- [ ] T006 — Rodar validações.
- [ ] T007 — Criar report.

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

- docs/design/gameplay/quests/QUEST_OBJECTIVE_EVENT_SYSTEM_DIRECTION.md
- docs/design/gameplay/quests/QUESTS_MAIN_LORE_DIRECTION.md
- docs/design/gameplay/quests/QUESTS_MAIN_PROGRESSION_REFINEMENT_DIRECTION.md
- docs/design/gameplay/cave/CAVE_DESIGN_DIRECTION.md
- docs/design/gameplay/cave/CAVE_MONSTER_ROSTER_DIRECTION.md
- docs/design/gameplay/cave/CAVE_COMBAT_BALANCE_VULNERABILITIES_DIRECTION.md
- docs/design/gameplay/cave/CAVE_MONSTER_ROSTER_ENEMY_BEHAVIOR_ADAPTER.md
- docs/design/gameplay/save_load/SAVE_LOAD_FULL_STATE_DIRECTION.md
- docs/design/gameplay/ui_ux/UI_UX_FULL_GAMEPLAY_DIRECTION.md
- docs/design/gameplay/ui_ux/UI_UX_MENU_SCREEN_FLOWS_DIRECTION.md
- docs/design/gameplay/world/SEASONS_CALENDAR_WEATHER_LUNAR_DIRECTION.md
- docs/design/gameplay/farm/FARM_DESIGN_DIRECTION_v1.3.md
- docs/design/gameplay/farm/FARM_LAYOUT_SCALE_BUILDINGS_DIRECTION.md
- docs/design/gameplay/city/CITY_DESIGN_DIRECTION_v1.2.md
- docs/design/gameplay/city/CITY_NPC_ROSTER_SERVICES_DIRECTION_v1.1.md
- docs/design/gameplay/city/CITY_LAYOUT_BUILDINGS_SCHEDULE_DIRECTION.md
- docs/design/gameplay/loot_crafting_economy/LOOT_CRAFTING_ECONOMY_DIRECTION.md
- docs/design/gameplay/loot_crafting_economy/ECONOMY_PRICING_STOCK_REFRESH_DIRECTION.md
- docs/design/gameplay/magic/MAGIC_SPELLS_ACTIONS_DIRECTION.md
- docs/design/gameplay/magic/MAGIC_LEARNING_UNLOCKS_SOURCES_DIRECTION.md
- docs/design/gameplay/combat/COMBAT_CORE_DIRECTION.md
- docs/design/gameplay/combat/STATUS_EFFECTS_DIRECTION.md
- docs/design/gameplay/bestiary/BESTIARY_KNOWLEDGE_DISCOVERY_DIRECTION.md
- docs/design/gameplay/companions/COMPANIONS_DIRECTION.md
- docs/design/gameplay/pets/PETS_DIRECTION.md
- docs/design/gameplay/social/SOCIAL_RELATIONSHIP_ROMANCE_DIRECTION.md

### Required interpretation

```text
Esta spec é future/mapped.
Ela deriva principalmente de QUESTS_MAIN_PROGRESSION_REFINEMENT_DIRECTION e CAVE_DESIGN_DIRECTION.
Ela não substitui as specs já geradas de final choice cinematic, postgame modifiers, cave save policy ou cave snapshot provider.
Ela não implementa cenas, prefabs, tilemaps, boss AI final, VFX, timeline, cutscene, assets finais, diálogo completo ou balance numérico definitivo.
Ela não cria runtime pet, romance/social deep ou companion AI.
Nível 101 é especial e narrativo, não nível procedural comum, não farm loop e não fonte de economia infinita.
Quando houver conflito entre main quest, cave, save/load, UI, economy, combat, magic ou world directions, o executor deve parar e registrar CONFLICT.
```

---

## Direction / Refinement Coverage

### Covered from directions

- Ato 4 estrutura: jogador abre boss gate do nível 100, derrota guardião/chefe do portão e acessa nível 101.
- Gate 100 é boss gate final, não apenas checkpoint.
- Gate 100 exige gates anteriores completed.
- Gate 100 pode exigir fragmentos/chaves/vestígios de lore.
- Derrotar boss do nível 100 abre caminho para nível 101.
- Gate 100 não deve conceder novamente recompensa única nem liberar farm infinito de boss.

### Deferred / future from directions

- Boss AI/tuning.
- Level101 fixed sequence.
- Final choice handoff.
- Cinematics.
- Scene/prefab setup.
- Save migration.

### Explicitly not redefined here

- Cave checkpoint internals.
- Boss fight combat implementation.
- Main quest fragment acquisition.
- FinalChoiceService.
- Postgame modifiers.

---

## 23A. Execution Readiness Matrix

| Área | Pergunta obrigatória | Evidência esperada | Status se faltar |
|---|---|---|---|
| Fonte canônica | Executor leu main progression, cave, save, UI, combat, economy, world, magic e lore directions? | Lista no report. | PARTIAL |
| Estado real do repo | Sistemas existentes relacionados a level 100 boss gate preconditions foram auditados antes de criar novos? | Comandos `rg` e achados. | PARTIAL |
| Não duplicação | Existe sistema equivalente já implementado/parcial? | REUSE/HARDEN/CREATE/DEFER. | BLOCKED se duplicar |
| Gate 100/101 | Nível 101 exige gate 100, gates anteriores e fragmentos/chaves/vestígios quando aplicável. | Tests/checklist. | BLOCKED se violar |
| Anti-procedural | Nível 101 não usa layout procedural comum, mineração comum, packs comuns ou nodes comuns. | Tests/checklist. | BLOCKED se violar |
| Anti-spoiler | Anya, Arquivista, Esperança, final choice, true nature do Arco/Pedra Negra/Mana não vazam cedo. | Tests/visibility. | PARTIAL |
| Anti-farm | Boss/reward/Level101 access não duplicam recompensa, loot, recurso ou progressão por reload/retry. | Tests/checklist. | PARTIAL |
| Save/load | Houve alteração de schema? | Declaração NO ou STOP se migration necessária. | BLOCKED se alterar sem migration |
| UI/PlayMode | Há fluxo visual/gameplay integrado? | Cenário final deferido documentado. | BUILD_VALIDATED no máximo se ausente |
| Pet/social/companion | Nenhum runtime pet, romance/social deep ou companion AI criado. | Checklist explícito. | BLOCKED se violar |
| Testes | Lógica determinística nova tem EditMode tests quando praticável? | Test list. | PARTIAL |
| Report | Execution report criado? | `docs/validation/19_spec_level_100_boss_gate_preconditions_future_runtime_execution_report.md`. | PARTIAL |

---

## 23B. Local Audit Checklist

Antes de alterar qualquer arquivo, Claude Code/Codex deve rodar e registrar no execution report:

```bash
rg -n "Level100Gate|Gate100|BossGate|Level101Unlocked|UniqueReward|PreviousBossGates|FragmentoEsperanca|GateHint" Assets/_Game/Scripts docs/design .specs
rg -n "Level100|Level101|Gate100|BossGate|Archivist|Arquivista|HopeFragment|FragmentoEsperanca|FinalChoice|Anya|MemoryArch|ArcoMemoria|BlackStone|PedraNegra|ManaRoot|Bromecian|Elyndor|CaveRunSeed|Checkpoint|UniqueReward|RepeatReward|PortalReturn|PostGame|Pet|Romance" Assets/_Game/Scripts docs/design .specs
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
Given o jogador cumpriu pré-condições do Ato 4
When o fluxo de level 100 boss gate preconditions roda
Then ele respeita gate 100/101
And preserva spoiler staging
And usa IDs estáveis
And não cria farm loop
And não muda domínio alheio diretamente.
```

### Scenario 2 — Existing partial implementation

```text
Given já existe implementação parcial no repo
When a execução audita o sistema
Then ela escolhe HARDEN_EXISTING em vez de recriar do zero
And registra divergências do direction
And altera apenas o menor conjunto seguro de arquivos.
```

### Scenario 3 — Blocked early access

```text
Given o jogador ainda não completou gate 100, gates anteriores, fragmentos/chaves/vestígios obrigatórios ou main quest flags
When tenta acessar Level 101 ou conteúdo associado
Then acesso é bloqueado com motivo seguro
And UI/quest hint não revela spoiler proibido.
```

### Scenario 4 — Idempotency / no exploit

```text
Given boss, reward, portal, sequence stage, Fragmento da Esperança ou final handoff já foi aplicado
When reload/retry/reentrada ocorre
Then nenhum reward/effect duplica
And estado terminal não regride
And Level 101 não vira farm comum.
```

### Scenario 5 — Final human validation deferred

```text
Given o fluxo exige inspeção visual/gameplay de gate, sequência, portal, lore room, boss handoff, UI ou save/load
When a implementação técnica terminar
Then o report registra cenário final em docs/validation/FINAL_HUMAN_VALIDATION_BY_WAVE.md
And não pede validação humana imediata por spec.
```

---

## 23D. Edge Cases and Failure Modes

A execução deve cobrir ou registrar risco residual para:

- Level101 unlock before gate 100.
- Previous gates ignored.
- Fragment/lore requirements ignored.
- Reward duplicated after reload.
- Level101 unlock regresses.
- Gate hint reveals Arquivista.
- Gate 100 treated as normal checkpoint.
- Boss gate farm loop.

---

## 23E. Minimum Execution Report Template

```md
# Execution Report — Level 100 Boss Gate Preconditions Future Runtime

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

## Level 100/101 compliance
- Gate 100 enforced:
- Gates previous complete:
- Required fragments/keys/lore checked:
- Level 101 not procedural common:
- No common mining:
- No common packs:
- No common loot/resource farm:
- Unique reward idempotent:
- Spoiler staging:
- Save/load safe:
- No pet/social/companion AI:
- UI not source of truth:

## Implementation decision
- REUSE_EXISTING / HARDEN_EXISTING / CREATE_MINIMAL / DEFER
- Justification:

## Files changed
- ...

## Functional evidence
- Happy path:
- Existing partial:
- Blocked early access:
- Idempotency/no exploit:
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
2. A implementação exigir scene/prefab/tilemap/asset/font/icon/audio/timeline changes.
3. A implementação alterar save schema sem migration spec.
4. A implementação usar Level 101 como procedural comum.
5. A implementação gerar mineração comum, packs comuns, loot comum ou node farmável comum em Level 101.
6. A implementação permitir acesso ao 101 antes de gate 100 e pré-condições.
7. A implementação duplicar reward único, boss reward, Fragmento da Esperança, portal unlock ou ending handoff.
8. A implementação revelar Anya/Arquivista/101/final/Pedra Negra/Mana completo cedo.
9. A implementação criar pet runtime, romance/social deep ou companion AI.
10. A implementação redefinir final choice/postgame já especificados no batch anterior.
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
rg -n "Level100|Level101|Gate100|BossGate|Archivist|HopeFragment|FinalChoice|Anya|MemoryArch|BlackStone|ManaRoot|CaveRunSeed|PortalReturn" Assets/_Game/Scripts docs/design .specs
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
Quando houver cenário visual/gameplay de Level 100/101, gate, boss handoff, portal, UI, final choice handoff ou save/load, registrar em execution report e vincular a docs/validation/FINAL_HUMAN_VALIDATION_BY_WAVE.md.
```

---

## 26. Testing Quality Gate

- Changed deterministic logic: YES, gate precondition/reward lock logic is deterministic.
- Requires EditMode tests: YES for missing-gates/fragments/lore/ready/unlock/reload/no-spoiler tests.
- Requires PlayMode automated or final human scenario: YES, DEFERRED for gate UI/gameplay validation.
- Requires regression test: YES if fixing existing gate100 bug; otherwise NO.
- Human validation timing: DEFERRED_TO_FINAL_VALIDATION se houver UI/gameplay integrado; caso contrário NOT REQUIRED.
- Minimum validation evidence for ACCEPTED: docs validation PASS; C# build PASS if C# changed; EditMode tests PASS or NOT RUN justified; gate100 unlock/reward safe.

---

## 27. Definition of Done

```text
Spec executada sem alterar arquivos proibidos.
Contratos/data/runtime implementados apenas dentro do escopo.
Execution report criado em docs/validation/19_spec_level_100_boss_gate_preconditions_future_runtime_execution_report.md.
Fonte/direction coverage preservado.
Validações obrigatórias PASS ou NOT RUN com motivo, impacto e mitigação.
Sem promoção indevida para ACCEPTED apenas por compile.
```

---

## 28. Anti-regressão

```text
Não transformar Level 101 em caverna procedural comum.
Não gerar mineração comum/packs comuns/loot comum em Level 101.
Não permitir gate bypass.
Não duplicar rewards/final handoff.
Não revelar spoilers cedo.
Não reimplementar final choice cinematic/postgame.
Não editar scenes/prefabs/assets.
Não criar pet runtime.
Não criar romance/social deep.
Não criar companion AI.
Não pedir human test por spec.
Não executar runtime em massa antes da 01Q ou exceção humana explícita.
Não alterar SPEC_EXECUTION_ORDER.md.
```

---

## 29. Notas para execução posterior

Esta spec é future/mapped. Deve ser executada apenas quando main progression, cave, save/load, combat, UI, Fonte, quest and final-choice handoff contracts estiverem estáveis ou quando houver decisão humana explícita de antecipar Level 100/101.
