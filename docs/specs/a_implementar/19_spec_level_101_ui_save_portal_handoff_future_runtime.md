# SPEC — Level 101 UI Save Portal Handoff Future Runtime

> **Spec ID:** `19_spec_level_101_ui_save_portal_handoff_future_runtime`  
> **Status:** A implementar / Future mapped  
> **Wave:** WAVE 19 — Level 100/101 Endgame Future  
> **Priority:** P2  
> **Type:** Runtime / Future / UI Projection / Save Boundary / Portal Handoff  
> **Domain:** Level 101 / UI Hint / Save Boundary / Portal Return / Final Choice Handoff  
> **Parallelizable:** YES  
> **Parallel group:** WAVE_19_LEVEL_100_101_ENDGAME_FUTURE  
> **Can run with:** docs-only reconciliation or localization/text-key spec if no same files.  
> **Must not run with:** qualquer spec que altere cave save policy base, final choice domain, postgame modifiers, scenes/prefabs/assets, save schema migration or boss AI.  
> **Repo lock scope:** `Assets/_Game/Scripts/UI/Endgame/**`, `Assets/_Game/Scripts/Cave/Level101/**`, `Assets/_Game/Scripts/Save/**`, `Assets/_Game/Tests/EditMode/UI/**`, `docs/validation/19_spec_level_101_ui_save_portal_handoff_future_runtime_execution_report.md`  
> **Depends on:**  
  - `docs/design/SPEC_SOURCE_MAP.md`
  - `docs/design/SPECIFICATION_PROCESS.md`
  - `docs/design/lore/VAALARA_GAME_CANON_DIRECTION_v1.0.md`
  - `docs/specs/SPEC_IMPLEMENTABLE_TEMPLATE.md`
  - `docs/specs/SPEC_WAVE_EXECUTION_PROTOCOL.md`
  - `docs/specs/SPEC_VALIDATION_MATRIX_MASTER.md`
  - `docs/specs/SPEC_EXISTING_IMPLEMENTATION_AUDIT.md`
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
  - level 101 sequence;
  - cave final save restriction policy;
  - final choice presentation;
  - postgame world modifiers.
> **Scope:** definir/endurecer UI projections, save boundary e portal/final-choice handoff do nível 101, sem aplicar final ou alterar policy base de save.  
> **Out of scope:** save policy base, final choice service, postgame modifiers, scene/prefab UI, portal VFX.

---

# /speckit.specify

## 1. Contexto

Nível 101 pode permitir retorno por portal próprio, mas não deve virar ponto de farm normal. O nível 101 conecta o gate 100, câmara especial, revelação parcial, Fragmento da Esperança e escolha final. Esta spec trata UI/save/portal/handoff, não a sequência em si.

---

## 2. Problema

Sem UI/save/portal handoff:

```text
UI não explica que 101 é especial;
save permitido em estado perigoso;
portal fica disponível durante boss;
portal cria farm de retorno;
handoff para final choice aplica final cedo;
reload durante portal duplica estado;
quest log revela final cedo;
Fonte UI mostra decisão final antes do Fragmento da Esperança.
```

---

## 3. Objetivo

Criar/endurecer:

```text
Level101UiProjection;
Level101ProgressHint;
Level101SaveBoundaryState;
Level101PortalHandoffState;
Level101FinalChoiceHandoffRequest;
Level101ExitResult;
Level101UiSpoilerPolicy;
Level101FinalValidationScenario.
```

---

## 4. Regras de design

```text
UI só mostra estado conhecido e seguro.
Level101 usa save policy restrita própria/delegada.
Portal não funciona durante boss.
Portal não reseta sequence/rewards.
Final choice handoff é request; domínio final aplica.
Fonte UI só mostra decisão final após gate correto.
Quest log não revela composição completa cedo.
```

---

## 5. User stories / engineering stories

```text
Como jogador, quero saber em que estágio do 101 estou sem spoilers.
Como UI, quero avisar quando save/portal não está disponível.
Como save/load, quero boundary seguro entre sequence, portal e final choice.
Como Fonte/final, quero receber handoff idempotente.
Como quest log, quero mostrar objetivo atual sem revelar final.
```

---

## 6. Escopo

Inclui:

```text
UI projection;
progress hints;
save boundary state;
portal handoff state;
final choice request;
exit result;
spoiler policy;
tests.
```

Não inclui:

```text
portal VFX;
scene/prefab;
save policy base;
final choice service;
postgame modifiers;
boss AI.
```

## 7. Modelo de domínio

### 7.1 Level101UiProjection

```text
CurrentKnownStage
DisplayTitleTextKey
DisplayHintTextKey
ShowLevel101SpecialWarning
CanShowPortalState
CanShowSaveState
CanShowFinalChoiceReady
SpoilerTier
DebugWarnings[]
```

### 7.2 Level101ProgressHint

```text
HintId
RequiredState
HintType: Safe | Warning | Blocked | Lore | Portal | Save | FinalHandoff
TextKey
CanShowExact
SpoilerTier
```

### 7.3 Level101SaveBoundaryState

```text
NoSave
CheckpointOnly
SafeRoomOnly
BeforeBoss
AfterBoss
AfterLiberation
AfterPortalOpened
BeforeFinalChoice
DuringFinalChoiceHandoff
Blocked
```

### 7.4 Level101PortalHandoffState

```text
PortalHidden
PortalLocked
PortalAvailable
PortalBlockedDuringBoss
PortalBlockedRewardPending
PortalUsedToReturn
PortalUsedToFinalChoice
PortalConsumed
```

### 7.5 Level101FinalChoiceHandoffRequest

```text
RequestId
Level101State
HopeFragmentRevealed
LiberationEventComplete
PortalState
FonteStageReady
CanOpenFinalChoice
BlockedReasons[]
UiDoesNotApplyFinalChoice true
```

### 7.6 Level101ExitResult

```text
ExitAllowed
ExitTarget: CaveEntrance | Fonte | FinalChoice | PostGameFuture
PreserveSequenceState
PreserveRewards
CanReenter
ReentryPolicy
Warnings[]
```

---

## 8. UI rules

```text
Before gate:
  do not mention Level101 exact content.

After unlock:
  show Level101 as special/deep chamber.

Inside:
  show safe progress hint, not full sequence checklist unless discovered.

Boss active:
  portal/save disabled with warning.

After liberation:
  final choice ready can be shown if Fonte/Fragmento Esperança gate allows.

Debug:
  hidden in final UI.
```

---

## 9. Save/portal rules

```text
Save boundary delegates to cave save policy if existing.
No save during boss/reward pending.
Portal disabled during boss/reward pending.
Portal return preserves sequence/reward state.
Portal does not reroll Level101.
Portal does not regrant rewards.
Portal can be exit/reentry or final choice handoff depending state.
```

---

## 10. Criteria

```text
Level101 UI projection contracts exist.
Save boundary states explicit.
Portal handoff states explicit.
Final choice request is idempotent and non-mutating.
Spoiler policy safe.
Tests cover pre-gate hidden, unlocked warning, boss portal blocked, reward pending blocked, after liberation final ready, portal preserves state, no reward regrant and UI not applying final choice.
```

# /speckit.plan

## 11. Arquitetura alvo

```text
Assets/_Game/Scripts/UI/Endgame/Level101UiProjection.cs
Assets/_Game/Scripts/UI/Endgame/Level101ProgressHint.cs
Assets/_Game/Scripts/Cave/Level101/Level101SaveBoundaryState.cs
Assets/_Game/Scripts/Cave/Level101/Level101PortalHandoffState.cs
Assets/_Game/Scripts/Cave/Level101/Level101FinalChoiceHandoffRequest.cs
Assets/_Game/Scripts/Cave/Level101/Level101ExitResult.cs
Assets/_Game/Scripts/UI/Endgame/Level101UiSpoilerPolicy.cs
Assets/_Game/Tests/EditMode/UI/Level101UiSavePortalHandoffTests.cs
```

Consolidar existentes se houver.

## 12. Arquivos permitidos

```text
Assets/_Game/Scripts/UI/Endgame/**
Assets/_Game/Scripts/Cave/Level101/**
Assets/_Game/Scripts/Save/**
Assets/_Game/Scripts/Fonte/**
Assets/_Game/Scripts/Quests/**
Assets/_Game/Scripts/Editor/Validation/**
Assets/_Game/Tests/EditMode/UI/**
docs/validation/19_spec_level_101_ui_save_portal_handoff_future_runtime_execution_report.md
```

## 13. Arquivos proibidos

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

# /speckit.tasks

## 14. Tasks

- [ ] T001 — Ler fontes.
- [ ] T002 — Auditar Level101/UI/save/portal/final handoff systems.
- [ ] T003 — Consolidar projection/boundary/handoff contracts.
- [ ] T004 — Implementar validators.
- [ ] T005 — Criar tests.
- [ ] T006 — Rodar validações.
- [ ] T007 — Criar report.

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

- Nível 101 pode permitir retorno por portal próprio, mas não deve virar ponto de farm normal.
- Fragmento da Esperança desbloqueia decisão final.
- Nível 101 e Fragmento da Esperança conectam boss final, Arco da Memória, três luas, Fonte, Mana e escolha final.
- Fonte UI só mostra funções desbloqueadas e não deve mostrar decisão final antes do Fragmento da Esperança.
- Quest log não deve revelar spoilers futuros.
- UI modal/foco/confirmations devem preservar input routing e spoiler-safe projection.

### Deferred / future from directions

- Portal VFX/assets.
- Final choice service.
- Postgame modifiers.
- Save policy base.
- Scene/prefab UI.
- Cinematic.

### Explicitly not redefined here

- Cave save policy.
- Level101 sequence.
- FinalChoiceService.
- FonteAnya core.
- QuestState.
- PostGameWorldState.

---

## 23A. Execution Readiness Matrix

| Área | Pergunta obrigatória | Evidência esperada | Status se faltar |
|---|---|---|---|
| Fonte canônica | Executor leu main progression, cave, save, UI, combat, economy, world, magic e lore directions? | Lista no report. | PARTIAL |
| Estado real do repo | Sistemas existentes relacionados a level 101 UI/save/portal/final handoff foram auditados antes de criar novos? | Comandos `rg` e achados. | PARTIAL |
| Não duplicação | Existe sistema equivalente já implementado/parcial? | REUSE/HARDEN/CREATE/DEFER. | BLOCKED se duplicar |
| Gate 100/101 | Nível 101 exige gate 100, gates anteriores e fragmentos/chaves/vestígios quando aplicável. | Tests/checklist. | BLOCKED se violar |
| Anti-procedural | Nível 101 não usa layout procedural comum, mineração comum, packs comuns ou nodes comuns. | Tests/checklist. | BLOCKED se violar |
| Anti-spoiler | Anya, Arquivista, Esperança, final choice, true nature do Arco/Pedra Negra/Mana não vazam cedo. | Tests/visibility. | PARTIAL |
| Anti-farm | Boss/reward/Level101 access não duplicam recompensa, loot, recurso ou progressão por reload/retry. | Tests/checklist. | PARTIAL |
| Save/load | Houve alteração de schema? | Declaração NO ou STOP se migration necessária. | BLOCKED se alterar sem migration |
| UI/PlayMode | Há fluxo visual/gameplay integrado? | Cenário final deferido documentado. | BUILD_VALIDATED no máximo se ausente |
| Pet/social/companion | Nenhum runtime pet, romance/social deep ou companion AI criado. | Checklist explícito. | BLOCKED se violar |
| Testes | Lógica determinística nova tem EditMode tests quando praticável? | Test list. | PARTIAL |
| Report | Execution report criado? | `docs/validation/19_spec_level_101_ui_save_portal_handoff_future_runtime_execution_report.md`. | PARTIAL |

---

## 23B. Local Audit Checklist

Antes de alterar qualquer arquivo, Claude Code/Codex deve rodar e registrar no execution report:

```bash
rg -n "Level101Ui|Level101Portal|Level101SaveBoundary|FinalChoiceHandoff|HopeFragment|FonteStage|PortalReturn" Assets/_Game/Scripts docs/design docs/specs
rg -n "Level100|Level101|Gate100|BossGate|Archivist|Arquivista|HopeFragment|FragmentoEsperanca|FinalChoice|Anya|MemoryArch|ArcoMemoria|BlackStone|PedraNegra|ManaRoot|Bromecian|Elyndor|CaveRunSeed|Checkpoint|UniqueReward|RepeatReward|PortalReturn|PostGame|Pet|Romance" Assets/_Game/Scripts docs/design docs/specs
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
Given o jogador cumpriu pré-condições do Ato 4
When o fluxo de level 101 UI/save/portal/final handoff roda
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

- UI reveals Level101 composition before unlock.
- Save allowed during boss/reward.
- Portal active during boss.
- Portal regrants reward.
- Portal rerolls sequence.
- UI applies final choice directly.
- Fonte UI shows final early.
- Quest log reveals final choice/Arquivista early.

---

## 23E. Minimum Execution Report Template

```md
# Execution Report — Level 101 UI Save Portal Handoff Future Runtime

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
rg -n "Level100|Level101|Gate100|BossGate|Archivist|HopeFragment|FinalChoice|Anya|MemoryArch|BlackStone|ManaRoot|CaveRunSeed|PortalReturn" Assets/_Game/Scripts docs/design docs/specs
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

- Changed deterministic logic: YES, UI projection/save-boundary/portal-handoff logic is deterministic.
- Requires EditMode tests: YES for pre-gate/unlocked/boss-block/reward-block/final-ready/portal-preserve/no-regrant/no-apply tests.
- Requires PlayMode automated or final human scenario: YES, DEFERRED for Level101 UI/save/portal visual validation.
- Requires regression test: YES if fixing existing Level101 UI/portal bug; otherwise NO.
- Human validation timing: DEFERRED_TO_FINAL_VALIDATION se houver UI/gameplay integrado; caso contrário NOT REQUIRED.
- Minimum validation evidence for ACCEPTED: docs validation PASS; C# build PASS if C# changed; EditMode tests PASS or NOT RUN justified; Level101 handoff safe and spoiler-proof.

---

## 27. Definition of Done

```text
Spec executada sem alterar arquivos proibidos.
Contratos/data/runtime implementados apenas dentro do escopo.
Execution report criado em docs/validation/19_spec_level_101_ui_save_portal_handoff_future_runtime_execution_report.md.
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
