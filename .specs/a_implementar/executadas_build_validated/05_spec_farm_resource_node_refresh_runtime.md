# SPEC — Farm Resource Node Refresh Runtime

> **Spec ID:** `05_spec_farm_resource_node_refresh_runtime`  
> **Status:** A implementar  
> **Wave:** WAVE 05 — Farm Resources / World Activities  
> **Priority:** P0  
> **Type:** Runtime / Farm / Resource Nodes / Refresh / Regeneration  
> **Domain:** Farm / Resource Nodes / Refresh / Save / Economy Guardrails  
> **Parallelizable:** NO  
> **Parallel group:** WAVE_05_FARM_RESOURCES_LOCKED  
> **Can run with:** N/A  
> **Must not run with:** qualquer spec que altere árvores, pedras, forage, fishing, mining, day transition, farm expansion zones, save schema, inventory drops ou economy resource values.  
> **Repo lock scope:** `Assets/_Game/Scripts/Farm/**`, `Assets/_Game/Scripts/World/**`, `Assets/_Game/Scripts/Resources/**`, `Assets/_Game/Scripts/Editor/Validation/**`, `Assets/_Game/Tests/EditMode/Farm/**`, `docs/validation/05_spec_farm_resource_node_refresh_runtime_execution_report.md`  
> **Depends on:**  
  - `docs/design/SPEC_SOURCE_MAP.md`
  - `docs/design/SPECIFICATION_PROCESS.md`
  - `docs/design/lore/VAALARA_GAME_CANON_DIRECTION_v1.0.md`
  - `.specs/SPEC_IMPLEMENTABLE_TEMPLATE.md`
  - `.specs/SPEC_WAVE_EXECUTION_PROTOCOL.md`
  - `.specs/SPEC_VALIDATION_MATRIX_MASTER.md`
  - `.specs/SPEC_EXISTING_IMPLEMENTATION_AUDIT.md`
  - `docs/project/CURRENT_STATE.md`
  - `docs/design/gameplay/farm/FARM_DESIGN_DIRECTION_v1.3.md`
  - `docs/design/gameplay/farm/FARM_LAYOUT_SCALE_BUILDINGS_DIRECTION.md`
  - `docs/design/gameplay/loot_crafting_economy/LOOT_CRAFTING_ECONOMY_DIRECTION.md`
  - `docs/design/gameplay/loot_crafting_economy/ECONOMY_PRICING_STOCK_REFRESH_DIRECTION.md`
  - `docs/design/gameplay/equipment/EQUIPMENT_WEAPONS_ARMOR_MATERIALS_DIRECTION.md`
  - `docs/design/gameplay/equipment/EQUIPMENT_MECHANICAL_BASELINES_DIRECTION.md`
  - `docs/design/gameplay/world/SEASONS_CALENDAR_WEATHER_LUNAR_DIRECTION.md`
  - `.specs/a_implementar/02_spec_time_clock_day_transition_runtime.md`
  - `.specs/a_implementar/05_spec_farm_level1_layout_fixed_anchors_runtime.md`
  - `.specs/a_implementar/05_spec_farm_layout_expansion_zones_free_build_runtime.md`
> **Blocks:**  
  - farm tree/chop spec;
  - farm rock/light mining spec;
  - farm forage/fishing spawn spec;
  - economy resource pricing;
  - farm expansion and zone unlocks.
> **Scope:** criar/endurecer contrato transversal de ResourceNode e refresh/regeneration para recursos leves da fazenda.  
> **Out of scope:** drop tables finais por item, sprites/prefabs, cave mining, animal/pet/companion automation, invasão/defesa, enemies, final balance de economia.

---

# /speckit.specify

## 1. Contexto

O Source Map lista `spec_resource_node_refresh_runtime` como spec recomendada atual de fazenda. O Farm Direction diz que árvores, pesca e world activities já existem em algum grau e devem ser preservadas/refinadas, não reimplementadas do zero.

Este contrato é a base comum para árvores, rochas, forage sazonal e pontos de pesca. Ele evita que cada sistema invente sua própria lógica de respawn, save e drops.

---

## 2. Problema

Sem contrato comum de resource node:

```text
árvore, pedra, forage e pesca podem usar saves diferentes;
nodes podem respawnar em anchors fixos/lore;
drops podem duplicar ao recarregar;
resource refresh pode gerar ouro infinito cedo;
node coletado pode reaparecer no mesmo dia sem regra;
áreas bloqueadas/endgame podem spawnar recurso cedo;
ferramentas e stamina/fatigue podem ser ignoradas;
resource node pode virar inimigo/invasão por acidente.
```

---

## 3. Objetivo

Criar/endurecer contrato transversal:

```text
ResourceNodeDefinition;
ResourceNodeInstanceState;
ResourceNodeType;
ResourceNodeRefreshPolicy;
ResourceNodeDropTableRef;
ResourceNodeHarvestResult;
Refresh scope by day/season/weather/farm level/zone;
Save/load stable node IDs;
Deterministic refresh tests;
No infinite resource loops.
```

---

## 4. Regras de design

```text
Fazenda é economia segura e previsível, não fonte infinita de ouro.
Caverna continua sendo economia de risco e fonte principal de materiais avançados.
Resource nodes leves alimentam rotina, crafting inicial, decoração e preparo.
Refresh deve criar rotina e decisão, não grind abusivo.
Fonte, Raiz de Mana, pedreira final e áreas endgame não são nodes comuns.
```

---

## 5. User stories / engineering stories

```text
Como jogador, quero cortar/coletar/minerar recursos leves e ver reposição natural com o tempo.
Como sistema de save/load, quero saber quais nodes foram coletados e quando podem voltar.
Como economy, quero evitar exploit de recurso infinito.
Como expansão da fazenda, quero que novas zonas liberem novos resource nodes.
Como executor, quero um contrato reutilizável para as specs de árvore, pedra, forage e pesca.
```

---

## 6. Escopo

Inclui:

```text
ResourceNodeType;
ResourceNodeDefinition;
ResourceNodeInstanceState;
RefreshPolicy;
DropTableRef contract;
Harvest/Collect result;
Node zone/farm level gates;
save/load safety;
editor/runtime validators;
tests.
```

Não inclui:

```text
implementação detalhada de cada árvore/pedra/forage/fish;
visual/prefab final;
cave mining;
pedreira final;
pets/companions;
enemy invasion/defense;
economy balance final.
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

- docs/design/gameplay/farm/FARM_DESIGN_DIRECTION_v1.3.md
- docs/design/gameplay/farm/FARM_LAYOUT_SCALE_BUILDINGS_DIRECTION.md
- docs/design/gameplay/loot_crafting_economy/LOOT_CRAFTING_ECONOMY_DIRECTION.md
- docs/design/gameplay/loot_crafting_economy/ECONOMY_PRICING_STOCK_REFRESH_DIRECTION.md
- docs/design/gameplay/equipment/EQUIPMENT_WEAPONS_ARMOR_MATERIALS_DIRECTION.md
- docs/design/gameplay/equipment/EQUIPMENT_MECHANICAL_BASELINES_DIRECTION.md
- docs/design/gameplay/world/SEASONS_CALENDAR_WEATHER_LUNAR_DIRECTION.md

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

- Spec Resource Node Refresh é recomendada no Source Map atual.
- Árvores/pesca/world activities já existem em algum grau e devem ser preservadas/refinadas.
- Bosque com árvores e lago funcional fazem parte da fazenda jogável essencial.
- Farm é economia segura/previsível; caverna é economia de risco.
- Resource refresh deve respeitar zonas, expansões, day transition e anti-inflação.
- Specs de fazenda não devem implementar invasões, defesa, dano a crops/estruturas ou inimigos na fazenda agora.

### Deferred / future from directions

- Pedreira final/endgame.
- Raiz Dormente de Mana como node normal.
- Pets/companions automation.
- Drop tables finais de todos os recursos.
- Sprites/prefabs/tilemaps finais.
- Cave resource nodes.

### Explicitly not redefined here

- Inventory backend.
- Economy final values.
- Tool durability/fatigue formulas.
- Farm expansion unlock costs.
- Quest/FarmOrder rewards.
- World weather generation.

## 7. Modelo de domínio

### 7.1 ResourceNodeType

```text
Tree
Stump
Rock
Boulder
Forage
FishingSpot
WaterResource
ClayPatch
FiberPatch
OreLight
SpecialLoreNode
EndgameNode
```

Regra: `SpecialLoreNode` e `EndgameNode` não são coletáveis comuns sem spec específica.

### 7.2 ResourceNodeDefinition

```text
NodeId
NodeType
DisplayName
AllowedZones
RequiredFarmLevel
RequiredToolTag optional
RequiredToolTier optional
RequiredSeason optional
RequiredWeather optional
RespawnPolicy
DropTableId
StaminaCost optional
FatigueCost optional
BlocksPath
CanBeRemoved
CanRegrow
IsLoreProtected
IsEndgameReserved
```

### 7.3 ResourceNodeInstanceState

```text
NodeInstanceId
NodeId
TilePosition
ZoneId
CurrentState: Available | Harvested | Depleted | Regrowing | Blocked | Hidden | Reserved
LastHarvestedDay optional
NextEligibleRefreshDay optional
RemainingHits optional
RandomSeed optional
SavedDropRollState optional
```

### 7.4 RefreshPolicy

```text
None
NextDayChance
FixedDays
SeasonStart
WeatherTriggered
FarmLevelUnlock
ZoneUnlock
ManualReplant
StoryUnlock
EndgameOnly
```

---

## 8. Refresh rules

```text
Refresh must run in a deterministic phase of day transition.
Refresh must not spawn into fixed anchors, buildings, blocked paths, water unless water node, Fonte, cave entrance, city exit, lore reserved zones or locked expansions.
Refresh must respect FarmPropertyLevel and ZoneId.
Refresh chance must be seeded or deterministic enough for save/load consistency.
Refresh cannot regenerate a node already Available.
Refresh cannot remove player-placed buildings or crops.
```

---

## 9. Drop and economy guardrails

```text
Drops come from DropTableRef or existing loot service.
Node system does not define final item prices.
Rare drops require gating by tool tier, season, weather, farm level or low refresh chance.
Farm resource nodes should not replace deep cave materials.
Wood/stone/fiber common resources can be stable, but not infinite within same day.
```

### Drop result model

```text
ResourceNodeHarvestResult
  Success
  FailureReason
  Drops[]
  ToolWear optional
  StaminaCostApplied
  FatigueCostApplied
  NodeStateAfter
  Events[]
```

---

## 10. Save/load

Must preserve:

```text
NodeInstanceId
NodeId
TilePosition
ZoneId
CurrentState
LastHarvestedDay
NextEligibleRefreshDay
RemainingHits if multi-hit
RandomSeed if random refresh/drop is used
```

Must not persist:

```text
GameObject references;
Prefab references;
Tilemap object references;
visual sprite as source of truth.
```

If current save lacks node state and adding it would change schema, STOP and require save/migration spec.

---

## 11. Criteria of acceptance

```text
Node state can be represented by stable ID and tile/zone.
Harvested/depleted node does not duplicate drops on reload.
Refresh does not occur in invalid/lore/locked zones.
Refresh timing is deterministic/testable.
Drops come from existing inventory/loot service or safe adapter.
Tool/stamina/fatigue hooks exist without redefining their systems.
No enemy/invasion/defense behavior is added.
```

---

# /speckit.plan

## 12. Arquitetura alvo

```text
Assets/_Game/Scripts/Farm/Resources/ResourceNodeType.cs
Assets/_Game/Scripts/Farm/Resources/ResourceNodeDefinition.cs
Assets/_Game/Scripts/Farm/Resources/ResourceNodeInstanceState.cs
Assets/_Game/Scripts/Farm/Resources/ResourceNodeRefreshPolicy.cs
Assets/_Game/Scripts/Farm/Resources/FarmResourceNodeService.cs
Assets/_Game/Scripts/Farm/Resources/FarmResourceRefreshProcessor.cs
Assets/_Game/Tests/EditMode/Farm/FarmResourceNodeRefreshTests.cs
```

Consolidar existentes se houver.

---

## 13. Arquivos permitidos

```text
Assets/_Game/Scripts/Farm/**
Assets/_Game/Scripts/Resources/**
Assets/_Game/Scripts/World/**
Assets/_Game/Scripts/Items/**
Assets/_Game/Scripts/Inventory/**
Assets/_Game/Scripts/Save/**
Assets/_Game/Scripts/Editor/Validation/**
Assets/_Game/Tests/EditMode/Farm/**
docs/validation/05_spec_farm_resource_node_refresh_runtime_execution_report.md
```

---

## 14. Arquivos proibidos

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

## 15. Estratégia

```text
1. Auditar world activities/resource nodes existentes.
2. Mapear contratos reais para ResourceNodeDefinition/Instance.
3. Criar/harden refresh processor.
4. Integrar zone/farm level gates.
5. Integrar loot/inventory adapter sem duplicar.
6. Criar tests de refresh/no-refresh/save/idempotency.
7. Criar report.
```

---

## 16. Paralelização

- Parallelizable: NO
- Must not run with:
  - tree/rock/forage/fishing specific specs;
  - farm expansion zones;
  - inventory/drop tables;
  - save schema;
  - economy resource pricing.
- Reason: this is the common foundation consumed by resource specs.

---

## 17. Impacto save/load

```text
Does this change save schema? SHOULD BE NO if node state exists; otherwise STOP.
Does this add save section? NO unless dedicated save spec approves.
Does this require migration? NO unless adding resource node section; then STOP.
Does this persist Unity references? NO.
```

---

## 18. Impacto eventos

```text
Adds events: CONDITIONAL, e.g. ResourceNodeHarvestedEvent, ResourceNodeRefreshedEvent if event contracts allow.
Changes events: SHOULD BE NO.
Requires unsubscribe pattern: YES if event subscribers are created.
```

---

## 19. Impacto UI/Unity

```text
Changes UI: NO final.
Changes scenes/prefabs/assets: NO.
Requires PlayMode/final human scenario: YES, DEFERRED for collect/sleep/refresh visual flow.
```

---

## 20. Riscos

```text
Risco: resource duplication.
Mitigação: node state/idempotency tests.

Risco: save schema needed.
Mitigação: STOP.

Risco: economy exploit.
Mitigação: refresh cooldown/gates and no same-day infinite respawn.
```

---

# /speckit.tasks

## 21. Tasks

- [ ] T001 — Ler fontes.
- [ ] T002 — Auditar ResourceNode/WorldActivity existentes.
- [ ] T003 — Consolidar resource node contracts.
- [ ] T004 — Implementar/harden refresh processor.
- [ ] T005 — Integrar zone/farm-level gates.
- [ ] T006 — Integrar loot/inventory adapter.
- [ ] T007 — Criar tests.
- [ ] T008 — Rodar validações.
- [ ] T009 — Criar report.

## 23A. Execution Readiness Matrix

| Área | Pergunta obrigatória | Evidência esperada | Status se faltar |
|---|---|---|---|
| Fonte canônica | A execução leu Source Map e directions do domínio? | Lista de fontes no execution report. | PARTIAL |
| Estado real do repo | World activities, nodes, save e inventory existentes foram auditados antes de criar novos? | Comandos `rg` e achados no report. | PARTIAL |
| Não duplicação | Existe sistema equivalente já implementado/parcial? | Decisão REUSE/HARDEN/CREATE/DEFER. | BLOCKED se duplicar |
| Escopo | A execução ficou dentro de resource node refresh? | Arquivos alterados e justificativa. | PARTIAL |
| Resource refresh | A regeneração/respawn é determinística e não cria farm infinito sem regra? | Testes/validador/report. | PARTIAL |
| Save/load | Houve alteração de schema? | Declaração explícita NO ou STOP se migration necessária. | BLOCKED se alterar sem migration |
| Eventos | Publishers/subscribers/lifecycle foram mapeados? | Mapa de eventos e unsubscribe policy se houver. | PARTIAL |
| UI/PlayMode | Há fluxo visual ou gameplay integrado? | Cenário final deferido documentado. | BUILD_VALIDATED no máximo se ausente |
| Testes | Há lógica determinística nova? | EditMode test ou NOT RUN justificado. | PARTIAL |
| Report | Execution report foi criado? | `docs/validation/05_spec_farm_resource_node_refresh_runtime_execution_report.md`. | PARTIAL |

---

## 23B. Local Audit Checklist

Antes de alterar qualquer arquivo, Claude Code/Codex deve rodar e registrar no execution report:

```bash
rg -n "ResourceNode|NodeRefresh|WorldActivity|Respawn|Regrowth|DropTable|FarmResource|ResourceRefresh" Assets/_Game/Scripts docs/design .specs
rg -n "ResourceNode|NodeRefresh|Tree|Stump|Rock|Stone|Forage|Fishing|Lake|WorldActivity|Respawn|Regrowth|Save|Inventory|Tool" Assets/_Game/Scripts docs/design .specs
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
Given o sistema base relacionado a resource node refresh existe ou foi criado de forma mínima
When o jogador executa a atividade principal da spec
Then o recurso é coletado/alterado de forma consistente
And inventário, ferramenta, stamina/fatigue e save/load não ficam inconsistentes
And nenhum sistema paralelo é criado
And o execution report registra evidência.
```

### Scenario 2 — Day transition / refresh

```text
Given o dia avança por dormir, colapso ou transição válida
When resource refresh/regrowth/spawn é processado
Then o sistema aplica apenas regras canônicas
And não cria ouro/recurso infinito cedo
And não respawna em tiles bloqueados, fixed anchors ou áreas de lore/endgame indevidas.
```

### Scenario 3 — Existing partial implementation

```text
Given já existe implementação parcial no repo
When a execução audita o sistema
Then ela escolhe HARDEN_EXISTING em vez de recriar do zero
And registra divergências do direction
And altera apenas o menor conjunto seguro de arquivos.
```

### Scenario 4 — Invalid state / invalid data

```text
Given um node, item, tool, drop table, spawn tile ou save payload inválido
When validator/teste é executado
Then a violação é reportada com motivo claro
And o runtime não duplica loot, não perde item e não corrompe save.
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

- Node coletado dropa novamente após reload.
- Refresh no mesmo dia sem regra.
- Spawn em Fonte/lago/cave/city/fixed anchor indevido.
- Spawn em locked expansion.
- Rare drop de caverna aparece em node comum de fazenda.
- Random refresh sem seed/save.
- Save schema alterado sem migration.
- Enemy/invasion behavior criado por engano.

---

## 23E. Minimum Execution Report Template

```md
# Execution Report — Farm Resource Node Refresh Runtime

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
- Day transition / refresh:
- Existing partial implementation:
- Invalid state:
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
4. A implementação exigir reescrever WorldActivity/Farm/Inventory/Save system canônico existente.
5. A implementação criar conflito com day transition, save ownership, stable IDs ou Testing Quality Gate.
6. A implementação criar pets/companions/animals runtime fora do escopo desta wave.
7. A implementação criar invasão/defesa/inimigos/dano a crops na fazenda, proibido no roadmap atual.
8. A implementação permitir recurso infinito, duplicação de drop, refresh sem cooldown ou save exploit.
9. Não for possível decidir se sistema existente é canônico ou obsoleto.
```


## 23G. Resource Refresh Matrix

| Policy | Trigger | Allowed for | Notes |
|---|---|---|---|
| None | Never | Lore/endgame fixed node | No common respawn |
| NextDayChance | Day transition | forage/light nodes | Seeded/deterministic |
| FixedDays | Day transition | tree/rock regrowth | NextEligibleRefreshDay |
| SeasonStart | Calendar season change | seasonal forage | Not per-day spam |
| WeatherTriggered | Weather phase | rare forage/fish | Known weather only |
| FarmLevelUnlock | Expansion unlock | new zones | Not retroactive exploit |
| ZoneUnlock | Zone is unlocked | zone-specific nodes | Validate tile |
| ManualReplant | Player action | orchards/future | Not automatic |
| StoryUnlock | Quest/main progression | lore nodes | Anti-spoiler |
| EndgameOnly | Endgame gates | quarry/mana root | Not current common farm |

## 23H. Anti-exploit Invariants

```text
No node may grant drops twice for the same harvest action.
No node may respawn in same day unless explicitly authored and bounded.
No common farm node may replace cave deep resources.
No refresh may bypass zone/farm-level/story gates.
```


## 25. Validações obrigatórias

Docs:

```powershell
.\tools\docs\validate_docs.ps1
```

Busca local mínima:

```bash
rg -n "ResourceNode|NodeRefresh|Tree|Stump|Rock|Stone|Forage|Fishing|Lake|WorldActivity|Respawn|Regrowth|Save|Inventory|Tool" Assets/_Game/Scripts docs/design .specs
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
Quando houver cenário visual/gameplay de FarmScene, coleta, corte, mineração, foraging ou pesca, registrar em execution report e vincular a docs/validation/FINAL_HUMAN_VALIDATION_BY_WAVE.md.
```

---

## 26. Testing Quality Gate

- Changed deterministic logic: YES, refresh/idempotency/zone gating logic is deterministic.
- Requires EditMode tests: YES for refresh/no-refresh/idempotency/zone tests.
- Requires PlayMode automated or final human scenario: YES, DEFERRED for collect -> sleep -> refresh visual flow.
- Requires regression test: YES if fixing existing resource duplicate/refresh bug; otherwise NO.
- Human validation timing: DEFERRED_TO_FINAL_VALIDATION se houver FarmScene/UI/gameplay integrado; caso contrário NOT REQUIRED.
- Minimum validation evidence for ACCEPTED: docs validation PASS; C# build PASS if C# changed; EditMode tests PASS or NOT RUN justified; no save schema change; no resource duplication.

---

## 27. Definition of Done

```text
Spec executada sem alterar arquivos proibidos.
Contratos/data/runtime implementados apenas dentro do escopo.
Execution report criado em docs/validation/05_spec_farm_resource_node_refresh_runtime_execution_report.md.
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
Não implementar pets/companions/animals runtime.
Não criar invasão/defesa/inimigos na fazenda.
Não criar recurso infinito/duplicação de loot.
Não pedir human test por spec.
Não executar runtime em massa antes da 01Q ou exceção humana explícita.
Não alterar SPEC_EXECUTION_ORDER.md.
```

---

## 29. Notas para execução posterior

Esta spec deve ser executada por Claude Code/Codex com auditoria local do repo, usando `rg` e validando o estado real dos sistemas já implementados.

Se a auditoria local revelar sistema equivalente já existente, a execução deve mudar para hardening/residual, não recriar do zero.
