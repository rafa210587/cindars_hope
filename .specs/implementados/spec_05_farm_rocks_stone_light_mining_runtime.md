# SPEC — Farm Rocks Stone Light Mining Runtime

> **Spec ID:** `05_spec_farm_rocks_stone_light_mining_runtime`  
> **Status:** A implementar  
> **Wave:** WAVE 05 — Farm Resources / World Activities  
> **Priority:** P1  
> **Type:** Runtime / Farm / Rocks / Stone / Light Mining Nodes  
> **Domain:** Farm / Rocks / Stone / Clay / Light Ore / Quarry Future  
> **Parallelizable:** NO  
> **Parallel group:** WAVE_05_FARM_RESOURCES_LOCKED  
> **Can run with:** N/A  
> **Must not run with:** qualquer spec que altere ResourceNode core, cave mining, final quarry, tool tiers, stamina/fatigue, inventory/drop tables, economy values, farm expansion zones or scene/prefab rock assets.  
> **Repo lock scope:** `Assets/_Game/Scripts/Farm/**`, `Assets/_Game/Scripts/Resources/**`, `Assets/_Game/Scripts/Tools/**`, `Assets/_Game/Scripts/Editor/Validation/**`, `Assets/_Game/Tests/EditMode/Farm/**`, `docs/validation/05_spec_farm_rocks_stone_light_mining_runtime_execution_report.md`  
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
  - `.specs/a_implementar/05_spec_farm_resource_node_refresh_runtime.md`
  - `.specs/a_implementar/05_spec_farm_layout_expansion_zones_free_build_runtime.md`
> **Blocks:**  
  - resource node refresh;
  - tool/pickaxe actions;
  - building material economy;
  - farm expansion zones;
  - final quarry future.
> **Scope:** definir/endurecer rochas/pedras/mineração leve da fazenda sem substituir mineração profunda da caverna nem implementar pedreira final.  
> **Out of scope:** cave mining, final quarry, rare ore economy, pickaxe durability balance, sprites/prefabs/tilemaps, pets/companions, enemy/invasion behavior.

---

# /speckit.specify

## 1. Contexto

Farm Direction inclui limpeza de terreno, pedras pequenas e recursos leves. Roadmap 5 cita pedreira final como endgame. Economy Direction deixa claro que materiais avançados devem vir majoritariamente da caverna e que a caverna é economia de risco.

Esta spec trata apenas de rochas/pedras comuns e mineração leve da fazenda. Não implementa pedreira final nem substitui a caverna.

---

## 2. Problema

Sem contrato de rocks/light mining:

```text
pedras comuns podem dropar minério raro cedo demais;
pedras podem respawnar em área arável/edifício sem regra;
drop pode duplicar após reload;
rock e boulder podem usar hit/depletion diferente sem contrato;
ferramenta/stamina/fatigue pode ser ignorada;
pedreira final pode ser criada cedo como node comum;
cave mining pode ser acoplado indevidamente à fazenda.
```

---

## 3. Objetivo

Criar/endurecer contrato:

```text
RockDefinition;
RockInstanceState;
RockSize/Type;
Hit/depletion rules;
Stone/clay/light ore drops;
Tool tier requirements;
Regeneration/refresh policy;
Quarry reserved future;
No cave rare replacement;
Save/load stable state.
```

---

## 4. Regras de design

```text
Farm rocks are light/common resources.
Advanced equipment materials come mostly from cave risk.
Quarry is late/endgame and limited.
Stone/common materials support buildings/crafting.
Rare ore in farm must be gated, limited, and future/endgame.
```

---

## 5. User stories / engineering stories

```text
Como jogador, quero quebrar pedra comum para limpar espaço e obter stone.
Como sistema, quero preservar depleted rock state no save/load.
Como economia, quero impedir que fazenda substitua mineração profunda.
Como expansão, quero liberar novos light resource nodes com property level.
Como endgame, quero reservar pedreira final para spec futura.
```

---

## 6. Escopo

Inclui:

```text
common rock/light mining definitions;
hit/depletion states;
stone/clay/light ore drop hooks;
pickaxe/tool requirement hooks;
refresh policy;
zone/farm level gates;
quarry reserved validation;
tests.
```

Não inclui:

```text
cave mining;
final quarry;
rare ore balance;
full pickaxe durability;
scene/prefab assets;
enemy/invasion damage;
automation.
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

- Farm tem recursos leves e limpeza de pedras/terreno.
- Roadmap 5 reserva pedreira final para endgame.
- Caverna é economia de risco e deve fornecer majoritariamente materiais avançados.
- Farm resources alimentam construção, crafting e expansão sem substituir exploração.
- Resource refresh é spec recomendada atual.
- Invasão/defesa/inimigos na fazenda são proibidos no roadmap atual.

### Deferred / future from directions

- Pedreira final.
- Cave mining nodes.
- Rare ore tables.
- Equipment material tier balance.
- Tool durability final.
- Scene/prefab/tilemap final.

### Explicitly not redefined here

- Inventory backend.
- Economy BaseValue final.
- Cave procedural generation.
- Building costs.
- Expansion unlock cost formulas.

## 7. Modelo de domínio

### 7.1 RockType

```text
SmallStone
MediumRock
LargeBoulder
ClayPatch
LightOreNode
DecorativeRock
LoreProtectedRock
FinalQuarryReserved
```

### 7.2 FarmRockDefinition

```text
RockId
RockType
DisplayName
AllowedZones
RequiredFarmLevel
RequiredPickaxeTier
MaxHits
DropTableId
CanRespawn
RespawnPolicy
BlocksPath
CanBeRemoved
IsLoreProtected
IsEndgameReserved
```

### 7.3 FarmRockInstanceState

```text
RockInstanceId
RockId
TilePosition
ZoneId
CurrentState: Available | HitPartial | Depleted | Removed | Regenerating | Reserved
RemainingHits
LastHitDay optional
LastDepletedDay optional
NextRefreshEligibleDay optional
RandomSeed optional
```

---

## 8. Hit/depletion rules

```text
Valid pickaxe/tool action reduces RemainingHits.
Final hit grants drops and transitions to Depleted/Removed.
Drops must be generated once.
State must persist before/with inventory grant or use atomic service.
Partial hit must persist if multi-hit rocks exist.
```

---

## 9. Drop policy

```text
SmallStone: common StoneResource baseline.
MediumRock/Boulder: more stone, possible clay/fiber/minor material if authored.
ClayPatch: clay/common crafting material.
LightOreNode: only low-tier/common ore if explicitly allowed.
FinalQuarryReserved: no common drops in this spec.
LoreProtectedRock: no common break unless story spec.
```

---

## 10. Refresh/regeneration

```text
Small stones may refresh with cooldown/chance in natural zones.
Boulders may not respawn or require longer cooldown.
Clay/light ore may be gated by season/weather/farm level.
No refresh under buildings, crops, fixed anchors or locked zones.
No refresh in player-cleared free-build area unless zone policy allows.
```

---

## 11. Criteria

```text
Rock can be hit/depleted with valid tool.
Drop generated once.
Partial hit/depleted state survives reload.
Farm rocks do not produce cave rare materials early.
Quarry/endgame nodes remain reserved.
Refresh respects zones and cooldown.
Tests cover hit, depletion, invalid tool, reload and reserved nodes.
```

---

# /speckit.plan

## 12. Arquitetura alvo

```text
Assets/_Game/Scripts/Farm/Rocks/FarmRockDefinition.cs
Assets/_Game/Scripts/Farm/Rocks/FarmRockInstanceState.cs
Assets/_Game/Scripts/Farm/Rocks/FarmRockBreakService.cs
Assets/_Game/Scripts/Farm/Rocks/FarmRockRefreshProcessor.cs
Assets/_Game/Scripts/Farm/Rocks/FarmRockValidator.cs
Assets/_Game/Tests/EditMode/Farm/FarmRockBreakRefreshTests.cs
```

Consolidar existentes se houver.

---

## 13. Arquivos permitidos

```text
Assets/_Game/Scripts/Farm/**
Assets/_Game/Scripts/Resources/**
Assets/_Game/Scripts/Tools/**
Assets/_Game/Scripts/Inventory/**
Assets/_Game/Scripts/Save/**
Assets/_Game/Scripts/Editor/Validation/**
Assets/_Game/Tests/EditMode/Farm/**
docs/validation/05_spec_farm_rocks_stone_light_mining_runtime_execution_report.md
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
1. Auditar rocks/mining/world activities existentes.
2. Consolidar rock definition/state.
3. Implementar/harden break/depletion.
4. Integrar resource refresh.
5. Bloquear quarry/lore protected misuse.
6. Criar tests.
7. Criar report.
```

---

## 16. Paralelização

- Parallelizable: NO
- Must not run with:
  - resource node refresh;
  - cave mining;
  - final quarry;
  - tool actions;
  - inventory/drop tables;
  - farm expansion zones.
- Reason: shared resource, tool, inventory, zone and save contracts.

---

## 17. Impacto save/load

```text
Does this change save schema? SHOULD BE NO if resource node state exists; otherwise STOP.
Does this add save section? NO unless dedicated save spec approves.
Does this require migration? NO unless adding rock state persistence; then STOP.
Does this persist Unity references? NO.
```

---

## 18. Impacto eventos

```text
Adds events: CONDITIONAL, e.g. RockBrokenEvent, FarmResourceNodeDepletedEvent if event contracts allow.
Changes events: SHOULD BE NO.
Requires unsubscribe pattern: YES if event subscribers are created.
```

---

## 19. Impacto UI/Unity

```text
Changes UI: NO final.
Changes scenes/prefabs/assets: NO.
Requires PlayMode/final human scenario: YES, DEFERRED for break/drop/refresh flow.
```

---

## 20. Riscos

```text
Risco: farm replaces cave materials.
Mitigação: drop policy/gates.

Risco: stone duplication.
Mitigação: depletion/idempotency tests.

Risco: quarry leaks early.
Mitigação: FinalQuarryReserved validation.
```

---

# /speckit.tasks

## 21. Tasks

- [ ] T001 — Ler fontes.
- [ ] T002 — Auditar rocks/mining systems.
- [ ] T003 — Consolidar rock definitions/states.
- [ ] T004 — Implementar/harden break/depletion.
- [ ] T005 — Implementar/harden refresh.
- [ ] T006 — Integrar drops/inventory.
- [ ] T007 — Criar tests.
- [ ] T008 — Rodar validações.
- [ ] T009 — Criar report.

## 23A. Execution Readiness Matrix

| Área | Pergunta obrigatória | Evidência esperada | Status se faltar |
|---|---|---|---|
| Fonte canônica | A execução leu Source Map e directions do domínio? | Lista de fontes no execution report. | PARTIAL |
| Estado real do repo | World activities, nodes, save e inventory existentes foram auditados antes de criar novos? | Comandos `rg` e achados no report. | PARTIAL |
| Não duplicação | Existe sistema equivalente já implementado/parcial? | Decisão REUSE/HARDEN/CREATE/DEFER. | BLOCKED se duplicar |
| Escopo | A execução ficou dentro de rocks/stone/light mining? | Arquivos alterados e justificativa. | PARTIAL |
| Resource refresh | A regeneração/respawn é determinística e não cria farm infinito sem regra? | Testes/validador/report. | PARTIAL |
| Save/load | Houve alteração de schema? | Declaração explícita NO ou STOP se migration necessária. | BLOCKED se alterar sem migration |
| Eventos | Publishers/subscribers/lifecycle foram mapeados? | Mapa de eventos e unsubscribe policy se houver. | PARTIAL |
| UI/PlayMode | Há fluxo visual ou gameplay integrado? | Cenário final deferido documentado. | BUILD_VALIDATED no máximo se ausente |
| Testes | Há lógica determinística nova? | EditMode test ou NOT RUN justificado. | PARTIAL |
| Report | Execution report foi criado? | `docs/validation/05_spec_farm_rocks_stone_light_mining_runtime_execution_report.md`. | PARTIAL |

---

## 23B. Local Audit Checklist

Antes de alterar qualquer arquivo, Claude Code/Codex deve rodar e registrar no execution report:

```bash
rg -n "Rock|Stone|Boulder|Clay|LightOre|Pickaxe|Mining|Quarry|FarmRock|RockBroken|Ore" Assets/_Game/Scripts docs/design .specs
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
Given o sistema base relacionado a rocks/stone/light mining existe ou foi criado de forma mínima
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

- Rock drop duplicado após reload.
- Rare cave ore aparece em farm node comum.
- Final quarry aparece antes do endgame.
- Partial hit perdido no reload.
- Tool tier ignored.
- Refresh em tile bloqueado/edifício.
- Farm rock system altera cave mining.
- Save schema alterado sem migration.

---

## 23E. Minimum Execution Report Template

```md
# Execution Report — Farm Rocks Stone Light Mining Runtime

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


## 23G. Rock Drop Guardrail Matrix

| RockType | Drop class allowed | Forbidden |
|---|---|---|
| SmallStone | StoneResource comum | Rare ore/gems |
| MediumRock | StoneResource comum, clay optional | Deep cave ore |
| LargeBoulder | StoneResource maior, limited extras | Boss/elite materials |
| ClayPatch | Clay/common reagent | Ore economy |
| LightOreNode | Low-tier ore if gated | Deep/rare ore |
| DecorativeRock | None or decor action | Resource exploit |
| LoreProtectedRock | None without story | Common break |
| FinalQuarryReserved | None now | Early quarry drops |

## 23H. Quarry Reservation

```text
Final quarry is Roadmap 5/endgame.
Do not expose as common farm node.
Do not create quarry payment/resource loop here.
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

- Changed deterministic logic: YES, hit/depletion/refresh/drop gate logic is deterministic.
- Requires EditMode tests: YES for hit/depletion/invalid tool/reload/reserved node tests.
- Requires PlayMode automated or final human scenario: YES, DEFERRED for break rock/drop/refresh visual flow.
- Requires regression test: YES if fixing existing rock duplication/mining bug; otherwise NO.
- Human validation timing: DEFERRED_TO_FINAL_VALIDATION se houver FarmScene/UI/gameplay integrado; caso contrário NOT REQUIRED.
- Minimum validation evidence for ACCEPTED: docs validation PASS; C# build PASS if C# changed; EditMode tests PASS or NOT RUN justified; no cave-economy replacement; no duplicate drops.

---

## 27. Definition of Done

```text
Spec executada sem alterar arquivos proibidos.
Contratos/data/runtime implementados apenas dentro do escopo.
Execution report criado em docs/validation/05_spec_farm_rocks_stone_light_mining_runtime_execution_report.md.
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
