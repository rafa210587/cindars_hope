# SPEC — Level 101 Encounters Rewards Anti Farm Future Runtime

> **Spec ID:** `19_spec_level_101_encounters_rewards_anti_farm_future_runtime`  
> **Status:** A implementar / Future mapped  
> **Wave:** WAVE 19 — Level 100/101 Endgame Future  
> **Priority:** P1  
> **Type:** Runtime / Future / Encounter Policy / Reward Lock  
> **Domain:** Level 101 / Boss Encounter Slots / Reward Idempotency / No Farm / No Common Packs  
> **Parallelizable:** NO  
> **Parallel group:** WAVE_19_LEVEL_100_101_ENDGAME_FUTURE  
> **Can run with:** N/A  
> **Must not run with:** qualquer spec que altere enemy stats, boss AI, loot tables final, combat formulas, economy pricing, final choice/postgame or scene assets.  
> **Repo lock scope:** `Assets/_Game/Scripts/Cave/Level101/**`, `Assets/_Game/Scripts/Loot/**`, `Assets/_Game/Scripts/Combat/**`, `Assets/_Game/Tests/EditMode/Cave/**`, `docs/validation/19_spec_level_101_encounters_rewards_anti_farm_future_runtime_execution_report.md`  
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
  - level 101 fixed sequence;
  - level 101 save/portal policy;
  - postgame modifiers;
  - bestiary knowledge.
> **Scope:** definir/endurecer política de encounters/rewards do nível 101: bosses fixos, no common packs, no common mining, reward locks e no farm loop.  
> **Out of scope:** boss AI, enemy stats, loot table final values, combat formula, visual arena, postgame economy.

---

# /speckit.specify

## 1. Contexto

CAVE_DESIGN_DIRECTION define densidade: 101 tem bosses fixos, 0 packs e sem packs comuns. Também diz que Level 101 é especial, não deve gerar mineração comum, não gerar packs comuns e não usar layout procedural normal. Essa spec detalha a política de encounter/reward sem criar stats ou AI.

---

## 2. Problema

Sem política anti-farm:

```text
Level101 gera packs comuns;
bosses fixos dropam reward a cada reload;
nodes comuns aparecem por erro de bioma;
loot raro vira farm loop;
boss defeated state regride;
bestiário recebe drops que não deveriam existir;
economia ganha recurso endgame infinito;
retry da sequência duplica Fragmento da Esperança.
```

---

## 3. Objetivo

Criar/endurecer:

```text
Level101EncounterSlot;
Level101EncounterPolicy;
Level101RewardPolicy;
Level101RewardLock;
Level101NoFarmValidator;
Level101LootSuppressionPolicy;
Level101BossDefeatState;
Level101RetryPolicy.
```

---

## 4. Regras de design

```text
101 tem bosses fixos, não packs comuns.
101 não tem mineração comum.
101 não usa loot comum.
Rewards únicos aplicam uma vez.
Retry de boss não gera reward único.
Economy protected from 101 farm.
Bestiary learns encounter but not hidden drops early.
```

---

## 5. User stories / engineering stories

```text
Como cave, quero slots fixos e não spawn comum.
Como loot/economy, quero impedir recurso infinito.
Como save/load, quero boss defeat/reward monotônico.
Como jogador, quero retry justo sem perder progresso nem farmar.
Como bestiary, quero registrar conhecimento sem revelar drops secretos.
```

---

## 6. Escopo

Inclui:

```text
encounter slot contracts;
boss defeat states;
reward locks;
loot suppression;
retry policy;
anti-farm validator;
tests.
```

Não inclui:

```text
enemy stats;
boss AI;
combat tuning;
loot table final values;
arena assets;
postgame economy.
```

## 7. Modelo de domínio

### 7.1 Level101EncounterSlot

```text
SlotId
SequenceStage
EncounterKind: Boss101A | Boss101B | Boss101C | FinalBossHandoff | LoreOnly
AllowedEncounterIds[]
RequiredState
ForbiddenState
AllowsRetry
AllowsCommonAdds false
AllowsCommonLoot false
AllowsResourceNodes false
```

### 7.2 Level101EncounterPolicy

```text
PolicyId
NoCommonPacks true
NoCommonMining true
NoCommonLoot true
NoProceduralSpawn true
AllowedBossSlots[]
AllowedScriptedHazards[]
AllowedLoreEchoes[]
AllowedMemoryManifestations[]
```

### 7.3 Level101RewardPolicy

```text
RewardPolicyId
RewardType: LoreUnlock | HopeFragment | PortalUnlock | FinalChoiceUnlock | CosmeticFuture | KnowledgeUnlock
Unique
Repeatable false
GrantCondition
RewardGrantId
ProtectedFromSellEconomy
```

### 7.4 Level101RewardLock

```text
RewardGrantId
AlreadyGranted
GrantedDay
GrantedByStage
CannotGrantAgain
RecoveryIfPartialCommit
```

### 7.5 Level101RetryPolicy

```text
RetryAllowed
RetryFromCheckpoint
RetryKeepsDefeatedBosses
RetryDoesNotRegrantRewards
RetryDoesNotResetLore
RetryDoesNotRespawnCommonPacks
```

---

## 8. Reward rules

```text
HopeFragment:
  unique, once, final handoff only.

PortalUnlock:
  unique/monotonic.

LoreUnlock:
  can be replayed as memory, not regranted as reward.

KnowledgeUnlock:
  can update bestiary/knowledge once or merge idempotently.

MaterialLoot:
  normally blocked unless authored as unique non-repeatable.

Gold:
  blocked by default.
```

---

## 9. Encounter rules

```text
Boss slots are explicit.
No common packs.
No common resource nodes.
No common mining.
No random chest tables unless unique/nonrepeatable.
Scripted hazards allowed if not farmable.
Memory echoes allowed as lore/visual/event contracts.
```

---

## 10. Criteria

```text
Encounter/reward policy contracts exist.
No common packs/mining/loot.
Reward locks idempotent.
Retry policy preserves progress without farm.
Boss defeat state monotonic.
Tests cover no common packs, no mining, no common loot, unique reward once, reload no regrant, retry no regrant, boss defeat monotonic and knowledge merge idempotent.
```

# /speckit.plan

## 11. Arquitetura alvo

```text
Assets/_Game/Scripts/Cave/Level101/Level101EncounterSlot.cs
Assets/_Game/Scripts/Cave/Level101/Level101EncounterPolicy.cs
Assets/_Game/Scripts/Cave/Level101/Level101RewardPolicy.cs
Assets/_Game/Scripts/Cave/Level101/Level101RewardLock.cs
Assets/_Game/Scripts/Cave/Level101/Level101NoFarmValidator.cs
Assets/_Game/Scripts/Cave/Level101/Level101LootSuppressionPolicy.cs
Assets/_Game/Scripts/Cave/Level101/Level101BossDefeatState.cs
Assets/_Game/Scripts/Cave/Level101/Level101RetryPolicy.cs
Assets/_Game/Tests/EditMode/Cave/Level101EncountersRewardsAntiFarmTests.cs
```

Consolidar existentes se houver.

## 12. Arquivos permitidos

```text
Assets/_Game/Scripts/Cave/Level101/**
Assets/_Game/Scripts/Loot/**
Assets/_Game/Scripts/Bestiary/**
Assets/_Game/Scripts/Economy/**
Assets/_Game/Scripts/Editor/Validation/**
Assets/_Game/Tests/EditMode/Cave/**
docs/validation/19_spec_level_101_encounters_rewards_anti_farm_future_runtime_execution_report.md
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
- [ ] T002 — Auditar Level101/encounter/loot systems.
- [ ] T003 — Consolidar encounter/reward policies.
- [ ] T004 — Implementar anti-farm validator.
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

- Level 101 tem bosses fixos, 0 packs, bosses, sem packs comuns.
- Level 101 não gera mineração comum.
- Level 101 não gera packs comuns.
- Level 101 não usa layout procedural normal.
- Level 101 é acesso especial de endgame narrativo e não deve virar ponto de farm normal.
- Gate 100 e boss reward não devem conceder recompensa única novamente nem liberar farm infinito.

### Deferred / future from directions

- Boss AI.
- Enemy stats.
- Loot values.
- Arena scene/prefab.
- Postgame economy.
- Cinematic.

### Explicitly not redefined here

- Loot table final.
- Enemy roster stats.
- Combat formulas.
- Bestiary implementation.
- Economy formulas.

---

## 23A. Execution Readiness Matrix

| Área | Pergunta obrigatória | Evidência esperada | Status se faltar |
|---|---|---|---|
| Fonte canônica | Executor leu main progression, cave, save, UI, combat, economy, world, magic e lore directions? | Lista no report. | PARTIAL |
| Estado real do repo | Sistemas existentes relacionados a level 101 encounters/rewards anti-farm foram auditados antes de criar novos? | Comandos `rg` e achados. | PARTIAL |
| Não duplicação | Existe sistema equivalente já implementado/parcial? | REUSE/HARDEN/CREATE/DEFER. | BLOCKED se duplicar |
| Gate 100/101 | Nível 101 exige gate 100, gates anteriores e fragmentos/chaves/vestígios quando aplicável. | Tests/checklist. | BLOCKED se violar |
| Anti-procedural | Nível 101 não usa layout procedural comum, mineração comum, packs comuns ou nodes comuns. | Tests/checklist. | BLOCKED se violar |
| Anti-spoiler | Anya, Arquivista, Esperança, final choice, true nature do Arco/Pedra Negra/Mana não vazam cedo. | Tests/visibility. | PARTIAL |
| Anti-farm | Boss/reward/Level101 access não duplicam recompensa, loot, recurso ou progressão por reload/retry. | Tests/checklist. | PARTIAL |
| Save/load | Houve alteração de schema? | Declaração NO ou STOP se migration necessária. | BLOCKED se alterar sem migration |
| UI/PlayMode | Há fluxo visual/gameplay integrado? | Cenário final deferido documentado. | BUILD_VALIDATED no máximo se ausente |
| Pet/social/companion | Nenhum runtime pet, romance/social deep ou companion AI criado. | Checklist explícito. | BLOCKED se violar |
| Testes | Lógica determinística nova tem EditMode tests quando praticável? | Test list. | PARTIAL |
| Report | Execution report criado? | `docs/validation/19_spec_level_101_encounters_rewards_anti_farm_future_runtime_execution_report.md`. | PARTIAL |

---

## 23B. Local Audit Checklist

Antes de alterar qualquer arquivo, Claude Code/Codex deve rodar e registrar no execution report:

```bash
rg -n "Level101Encounter|Level101Reward|NoFarm|NoCommonPacks|NoCommonMining|RewardLock|BossDefeatState|RetryPolicy" Assets/_Game/Scripts docs/design .specs
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
When o fluxo de level 101 encounters/rewards anti-farm roda
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

- Common packs spawn in Level101.
- Common mining enabled.
- Unique reward repeated.
- Boss defeated state regresses.
- Retry farms lore/materials.
- Economy gains Level101 commodity.
- Bestiary records hidden drop early.
- Reward partial commit duplicates on reload.

---

## 23E. Minimum Execution Report Template

```md
# Execution Report — Level 101 Encounters Rewards Anti Farm Future Runtime

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

- Changed deterministic logic: YES, encounter/reward/idempotency policy logic is deterministic.
- Requires EditMode tests: YES for no-common/reward-lock/retry/monotonic/knowledge/economy tests.
- Requires PlayMode automated or final human scenario: YES, DEFERRED for Level101 encounter gameplay validation.
- Requires regression test: YES if fixing existing Level101 reward exploit; otherwise NO.
- Human validation timing: DEFERRED_TO_FINAL_VALIDATION se houver UI/gameplay integrado; caso contrário NOT REQUIRED.
- Minimum validation evidence for ACCEPTED: docs validation PASS; C# build PASS if C# changed; EditMode tests PASS or NOT RUN justified; Level101 no-farm policy enforced.

---

## 27. Definition of Done

```text
Spec executada sem alterar arquivos proibidos.
Contratos/data/runtime implementados apenas dentro do escopo.
Execution report criado em docs/validation/19_spec_level_101_encounters_rewards_anti_farm_future_runtime_execution_report.md.
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
