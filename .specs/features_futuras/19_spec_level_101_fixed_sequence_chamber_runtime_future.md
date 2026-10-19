# SPEC — Level 101 Fixed Sequence Chamber Runtime Future

> **Spec ID:** `19_spec_level_101_fixed_sequence_chamber_runtime_future`  
> **Status:** A implementar / Future mapped  
> **Wave:** WAVE 19 — Level 100/101 Endgame Future  
> **Priority:** P1  
> **Type:** Runtime / Future / Endgame Sequence / Special Cave Level  
> **Domain:** Level 101 / Fixed Chamber / Sequence Stages / Lore Rooms / Portal Return  
> **Parallelizable:** NO  
> **Parallel group:** WAVE_19_LEVEL_100_101_ENDGAME_FUTURE  
> **Can run with:** N/A  
> **Must not run with:** qualquer spec que altere cave procedural generation, boss AI final, scene/tilemap/prefab assets, final choice/postgame, save migration or cinematic assets.  
> **Repo lock scope:** `Assets/_Game/Scripts/MainProgression/Endgame/**`, `Assets/_Game/Scripts/Cave/Level101/**`, `Assets/_Game/Scripts/Quests/**`, `Assets/_Game/Tests/EditMode/MainProgression/**`, `docs/validation/19_spec_level_101_fixed_sequence_chamber_runtime_future_execution_report.md`  
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
  - level 101 encounters/reward policy;
  - level 101 UI/save/portal;
  - final choice handoff;
  - postgame modifiers.
> **Scope:** definir/endurecer sequência fixa do nível 101: estágios, câmaras, lore beats, handoffs e portal de retorno como contratos, sem assets/cenas.  
> **Out of scope:** procedural layout, tilemap final, boss AI, VFX/cinematics, final choice domain, postgame modifiers.

---

# /speckit.specify

## 1. Contexto

Nível 101 é Câmara de Anya: espaço especial, não um nível procedural comum. Deve conter santuário nymiriano, máquina bromeciana, Arco de Elyndor, Pedras Negras, Água Viva corrompida, raiz/estrutura de Mana, ecos de Cindar, fragmentos de Anya e boss final. Composição sugerida: entrada silenciosa/corredor de memória, boss 101-A, sala de lore, boss 101-B, sala de Água Viva corrompida, boss 101-C, câmara final, libertação parcial e portal de retorno.

---

## 2. Problema

Sem sequência fixa:

```text
Level 101 vira nível procedural normal;
salas podem sair fora de ordem;
lore final aparece antes da luta correta;
portal de retorno aparece cedo;
boss 101-C pode rodar antes do 101-A;
jogador fica preso entre câmaras;
sequence stage duplica em reload;
reentrada reinicia tudo e gera farm.
```

---

## 3. Objetivo

Criar/endurecer:

```text
Level101SequenceState;
Level101SequenceStage;
Level101ChamberDefinition;
Level101StageGate;
Level101LoreBeat;
Level101PortalReturnState;
Level101SequenceProgressionService;
Level101SequenceValidator.
```

---

## 4. Regras de design

```text
Level101 não é procedural comum.
Level101 usa sequência fixa/gated.
Stages são monotônicos salvo política explícita.
Portal de retorno só aparece em estados permitidos.
Lore beat não revela verdade absoluta de Anya.
Sequência não aplica final choice.
```

---

## 5. User stories / engineering stories

```text
Como jogador, quero progredir por uma sequência endgame clara.
Como cave, quero impedir ordem inválida ou farm.
Como lore, quero revelar parcialmente Anya/Cindar/Bromécia/Elyndor.
Como save/load, quero restaurar stage sem duplicar eventos.
Como UI, quero mostrar hint seguro do stage atual.
```

---

## 6. Escopo

Inclui:

```text
sequence state;
stage/chamber definitions;
stage gates;
lore beat contracts;
portal return state;
progression validator;
tests.
```

Não inclui:

```text
scene/tilemap/prefab;
boss AI;
final cinematic;
final choice service;
postgame effects.
```

## 7. Modelo de domínio

### 7.1 Level101SequenceState

```text
NotUnlocked
ReadyToEnter
Entered
SilentMemoryCorridor
Boss101AActive
Boss101ADefeated
BromeciaElyndorLoreRoom
Boss101BActive
Boss101BDefeated
CorruptedLivingWaterRoom
Boss101CActive
Boss101CDefeated
FinalChamberReached
PartialLiberationEvent
ReturnPortalOpened
FinalChoiceReady
Completed
```

### 7.2 Level101SequenceStage

```text
StageId
StateBefore
StateAfter
RequiredFlags[]
ForbiddenFlags[]
ChamberId
StageType: Corridor | Boss | LoreRoom | CorruptionRoom | FinalChamber | Liberation | Portal | Handoff
IsRepeatable false
SpoilerTier
```

### 7.3 Level101ChamberDefinition

```text
ChamberId
ChamberType
RequiredSequenceState
AllowedEncounterIds[]
AllowedLoreBeatIds[]
AllowsCommonMining false
AllowsCommonPacks false
AllowsCommonLoot false
AllowsSavePolicy
AllowsReturnPortal
```

### 7.4 Level101LoreBeat

```text
LoreBeatId
Topic: Cindar | Anya | Bromecia | Elyndor | BlackStone | LivingWater | ManaRoot | MemoryArch
RevealTier: Hint | Partial | Major | FinalChoiceSetup
RequiredStage
CanReplayAsMemoryFuture
CannotRevealAbsoluteTruth true
```

### 7.5 Level101PortalReturnState

```text
NotAvailable
AvailableAfterCheckpoint
AvailableAfterStage
AvailableAfterLiberation
ConsumedToPostGame
DisabledDuringBoss
```

---

## 8. Canonical stage order

```text
1. SilentMemoryCorridor
2. Boss101A — Guardião de Pedra Negra
3. BromeciaElyndorLoreRoom
4. Boss101B — Arauto Quebrado de Nyx or Constructo de Juramento
5. CorruptedLivingWaterRoom
6. Boss101C — Wyvern de Pedra Negra or Núcleo Dracônico
7. FinalChamberReached
8. PartialLiberationEvent
9. ReturnPortalOpened / FinalChoiceReady
```

---

## 9. Transition rules

```text
Stage can advance only if current state matches.
Boss stage can complete only once.
Lore room can be replayed as memory but does not regrant state.
Portal cannot be active during boss.
Return does not reset stage unless explicit retry policy.
FinalChamberReached does not automatically apply final choice.
```

---

## 10. Criteria

```text
Level101 sequence contracts exist.
Stage order deterministic.
No common mining/packs/loot allowed.
Portal return state explicit.
Lore beats partial and staged.
Tests cover stage order, blocked skip, boss completion once, lore replay no reward, portal disabled in boss, return preserves stage and no procedural flags.
```

# /speckit.plan

## 11. Arquitetura alvo

```text
Assets/_Game/Scripts/Cave/Level101/Level101SequenceState.cs
Assets/_Game/Scripts/Cave/Level101/Level101SequenceStage.cs
Assets/_Game/Scripts/Cave/Level101/Level101ChamberDefinition.cs
Assets/_Game/Scripts/Cave/Level101/Level101StageGate.cs
Assets/_Game/Scripts/Cave/Level101/Level101LoreBeat.cs
Assets/_Game/Scripts/Cave/Level101/Level101PortalReturnState.cs
Assets/_Game/Scripts/Cave/Level101/Level101SequenceProgressionService.cs
Assets/_Game/Scripts/Cave/Level101/Level101SequenceValidator.cs
Assets/_Game/Tests/EditMode/MainProgression/Level101FixedSequenceChamberTests.cs
```

Consolidar existentes se houver.

## 12. Arquivos permitidos

```text
Assets/_Game/Scripts/Cave/Level101/**
Assets/_Game/Scripts/MainProgression/Endgame/**
Assets/_Game/Scripts/Quests/**
Assets/_Game/Scripts/UI/Endgame/**
Assets/_Game/Scripts/Editor/Validation/**
Assets/_Game/Tests/EditMode/MainProgression/**
docs/validation/19_spec_level_101_fixed_sequence_chamber_runtime_future_execution_report.md
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
- [ ] T002 — Auditar level101/cave/main progression systems.
- [ ] T003 — Consolidar sequence/chamber/lore/portal contracts.
- [ ] T004 — Implementar progression validator.
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

- Nível 101 é espaço especial, não procedural comum.
- Nível 101 contém santuário nymiriano, máquina bromeciana, Arco de Elyndor, Pedras Negras, Água Viva corrompida, raiz/estrutura de Mana, ecos de Cindar, fragmentos de Anya e boss final.
- Composição sugerida: corredor de memória, boss 101-A, lore Bromécia/Elyndor, boss 101-B, Água Viva corrompida, boss 101-C, câmara final, libertação parcial e portal de retorno.
- Nível 101 não deve gerar mineração comum, packs comuns ou layout procedural normal.
- Nível 101 não deve revelar toda a verdade absoluta de Anya.
- Nível 101 não é acessível antes do gate 100.

### Deferred / future from directions

- Scene/tilemap/prefab.
- Boss AI final.
- Cinematic/VFX/audio.
- Final choice service.
- Postgame modifiers.
- Save migration.

### Explicitly not redefined here

- Cave generator.
- Enemy roster stats.
- FinalChoiceService.
- PostGameWorldState.
- Boss combat balance.

---

## 23A. Execution Readiness Matrix

| Área | Pergunta obrigatória | Evidência esperada | Status se faltar |
|---|---|---|---|
| Fonte canônica | Executor leu main progression, cave, save, UI, combat, economy, world, magic e lore directions? | Lista no report. | PARTIAL |
| Estado real do repo | Sistemas existentes relacionados a level 101 fixed sequence/chamber foram auditados antes de criar novos? | Comandos `rg` e achados. | PARTIAL |
| Não duplicação | Existe sistema equivalente já implementado/parcial? | REUSE/HARDEN/CREATE/DEFER. | BLOCKED se duplicar |
| Gate 100/101 | Nível 101 exige gate 100, gates anteriores e fragmentos/chaves/vestígios quando aplicável. | Tests/checklist. | BLOCKED se violar |
| Anti-procedural | Nível 101 não usa layout procedural comum, mineração comum, packs comuns ou nodes comuns. | Tests/checklist. | BLOCKED se violar |
| Anti-spoiler | Anya, Arquivista, Esperança, final choice, true nature do Arco/Pedra Negra/Mana não vazam cedo. | Tests/visibility. | PARTIAL |
| Anti-farm | Boss/reward/Level101 access não duplicam recompensa, loot, recurso ou progressão por reload/retry. | Tests/checklist. | PARTIAL |
| Save/load | Houve alteração de schema? | Declaração NO ou STOP se migration necessária. | BLOCKED se alterar sem migration |
| UI/PlayMode | Há fluxo visual/gameplay integrado? | Cenário final deferido documentado. | BUILD_VALIDATED no máximo se ausente |
| Pet/social/companion | Nenhum runtime pet, romance/social deep ou companion AI criado. | Checklist explícito. | BLOCKED se violar |
| Testes | Lógica determinística nova tem EditMode tests quando praticável? | Test list. | PARTIAL |
| Report | Execution report criado? | `docs/validation/19_spec_level_101_fixed_sequence_chamber_runtime_future_execution_report.md`. | PARTIAL |

---

## 23B. Local Audit Checklist

Antes de alterar qualquer arquivo, Claude Code/Codex deve rodar e registrar no execution report:

```bash
rg -n "Level101Sequence|Level101Chamber|Level101LoreBeat|PortalReturn|Boss101|SilentMemoryCorridor|CorruptedLivingWater" Assets/_Game/Scripts docs/design .specs
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
When o fluxo de level 101 fixed sequence/chamber roda
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

- Stage skip.
- Boss stage repeated for reward.
- Portal active during boss.
- Lore reveals absolute truth early.
- Common mining/packs/loot enabled.
- Return resets sequence.
- Final choice applied by sequence.
- Level101 uses normal cave generator.

---

## 23E. Minimum Execution Report Template

```md
# Execution Report — Level 101 Fixed Sequence Chamber Runtime Future

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

- Changed deterministic logic: YES, sequence/stage/portal validation logic is deterministic.
- Requires EditMode tests: YES for stage-order/blocked-skip/boss-once/lore-replay/portal/return/no-procedural tests.
- Requires PlayMode automated or final human scenario: YES, DEFERRED for Level101 gameplay/visual validation.
- Requires regression test: YES if fixing existing level101 sequence bug; otherwise NO.
- Human validation timing: DEFERRED_TO_FINAL_VALIDATION se houver UI/gameplay integrado; caso contrário NOT REQUIRED.
- Minimum validation evidence for ACCEPTED: docs validation PASS; C# build PASS if C# changed; EditMode tests PASS or NOT RUN justified; Level101 sequence fixed and non-farmable.

---

## 27. Definition of Done

```text
Spec executada sem alterar arquivos proibidos.
Contratos/data/runtime implementados apenas dentro do escopo.
Execution report criado em docs/validation/19_spec_level_101_fixed_sequence_chamber_runtime_future_execution_report.md.
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
