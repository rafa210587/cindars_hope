# SPEC — Dormant Mana Root Endgame Farm Future Runtime

> **Spec ID:** `20_spec_dormant_mana_root_endgame_farm_future_runtime`  
> **Status:** A implementar / Future mapped  
> **Wave:** WAVE 20 — Mana / Living Water / Endgame Farm Future  
> **Priority:** P1  
> **Type:** Runtime / Future / Farm Endgame / Mana Root  
> **Domain:** Dormant Mana Root / Endgame Farm / Fonte Area / Gated Growth  
> **Parallelizable:** NO  
> **Parallel group:** WAVE_20_MANA_ENDGAME_FARM_FUTURE  
> **Can run with:** N/A  
> **Must not run with:** qualquer spec que altere crop core, farm tilemap/prefab, postgame modifiers, Level101, final choice, save migration or economy pricing final.  
> **Repo lock scope:** `Assets/_Game/Scripts/Farm/Mana/**`, `Assets/_Game/Scripts/Fonte/**`, `Assets/_Game/Scripts/World/Lunar/**`, `Assets/_Game/Tests/EditMode/Farm/**`, `docs/validation/20_spec_dormant_mana_root_endgame_farm_future_runtime_execution_report.md`  
> **Depends on:**  
  - `docs/design/SPEC_SOURCE_MAP.md`
  - `docs/design/SPECIFICATION_PROCESS.md`
  - `docs/design/lore/VAALARA_GAME_CANON_DIRECTION_v1.0.md`
  - `.specs/SPEC_IMPLEMENTABLE_TEMPLATE.md`
  - `.specs/SPEC_WAVE_EXECUTION_PROTOCOL.md`
  - `.specs/SPEC_VALIDATION_MATRIX_MASTER.md`
  - `.specs/SPEC_EXISTING_IMPLEMENTATION_AUDIT.md`
  - `docs/project/CURRENT_STATE.md`
  - `docs/design/gameplay/quests/QUESTS_MAIN_LORE_DIRECTION.md`
  - `docs/design/gameplay/quests/QUESTS_MAIN_PROGRESSION_REFINEMENT_DIRECTION.md`
  - `docs/design/gameplay/quests/QUEST_OBJECTIVE_EVENT_SYSTEM_DIRECTION.md`
  - `docs/design/gameplay/farm/FARM_DESIGN_DIRECTION_v1.3.md`
  - `docs/design/gameplay/farm/FARM_LAYOUT_SCALE_BUILDINGS_DIRECTION.md`
  - `docs/design/gameplay/world/SEASONS_CALENDAR_WEATHER_LUNAR_DIRECTION.md`
  - `docs/design/gameplay/loot_crafting_economy/LOOT_CRAFTING_ECONOMY_DIRECTION.md`
  - `docs/design/gameplay/loot_crafting_economy/ECONOMY_PRICING_STOCK_REFRESH_DIRECTION.md`
  - `docs/design/gameplay/magic/MAGIC_SPELLS_ACTIONS_DIRECTION.md`
  - `docs/design/gameplay/magic/MAGIC_LEARNING_UNLOCKS_SOURCES_DIRECTION.md`
  - `docs/design/gameplay/cave/CAVE_DESIGN_DIRECTION.md`
  - `docs/design/gameplay/cave/CAVE_COMBAT_BALANCE_VULNERABILITIES_DIRECTION.md`
  - `docs/design/gameplay/save_load/SAVE_LOAD_FULL_STATE_DIRECTION.md`
  - `docs/design/gameplay/ui_ux/UI_UX_FULL_GAMEPLAY_DIRECTION.md`
  - `docs/design/gameplay/ui_ux/UI_UX_MENU_SCREEN_FLOWS_DIRECTION.md`
  - `docs/design/gameplay/city/CITY_DESIGN_DIRECTION_v1.2.md`
  - `docs/design/gameplay/city/CITY_NPC_ROSTER_SERVICES_DIRECTION_v1.1.md`
  - `docs/design/gameplay/companions/COMPANIONS_DIRECTION.md`
  - `docs/design/gameplay/pets/PETS_DIRECTION.md`
  - `docs/design/gameplay/bestiary/BESTIARY_KNOWLEDGE_DISCOVERY_DIRECTION.md`
  - `docs/design/gameplay/social/SOCIAL_RELATIONSHIP_ROMANCE_DIRECTION.md`
> **Blocks:**  
  - living water mana growth conditions;
  - mana economy/crafting/magic policy;
  - mana lunar/corruption risk;
  - postgame modifiers.
> **Scope:** definir/endurecer contratos futuros da Raiz Dormente de Mana na fazenda endgame, sem criar crop comum nem tilemap.  
> **Out of scope:** crop core rewrite, tilemap/prefab placement, final art, postgame final values, save migration.

---

# /speckit.specify

## 1. Contexto

FARM_DESIGN_DIRECTION v1.3 remove Fruto Mana como crop comum, define Árvore de Mana como consciente, rara e seletiva, e introduz Raiz Dormente de Mana como opção canônica de endgame. Mana pode ser plantado, mas é difícil florescer, não é poder absoluto e não é crop comum.

---

## 2. Problema

Sem contrato:

```text
Mana vira semente comum;
Raiz Dormente cresce como qualquer crop;
player spamma canteiros de Mana;
Mana ignora Fonte/Anya/lua/Água Viva;
Mana cresce semanalmente e quebra economia;
postgame Use vira produção infinita;
UI revela Raiz de Mana cedo;
save/reload duplica crescimento.
```

---

## 3. Objetivo

Criar/endurecer:

```text
DormantManaRootState;
DormantManaRootGrowthStage;
DormantManaRootPlantingRequest;
ManaRootEligibilityPolicy;
ManaRootGrowthConditionSet;
ManaRootGrowthResult;
ManaRootSaveRecord;
ManaRootAntiFarmValidator.
```

---

## 4. Regras de design

```text
Raiz Dormente é endgame/lore-gated.
Mana não é crop comum.
Mana não usa rotina de crescimento comum sem policy específica.
Raiz exige Fonte/Água Viva/solo/lua/fragmentos ou postgame policy.
Raiz escolhe/aceita local limitado.
Quantidade simultânea deve ser muito limitada.
Produção deve ser rara, baixa e bounded.
```

---

## 5. User stories / engineering stories

```text
Como jogador, quero plantar ou ativar Raiz Dormente apenas após gates narrativos.
Como farm, quero validar local/solo/Fonte/lua/Água Viva.
Como economy, quero impedir produção em massa.
Como save/load, quero preservar estágio sem duplicar crescimento.
Como UI, quero mostrar motivos de bloqueio sem spoiler.
```

---

## 6. Escopo

Inclui:

```text
root state;
growth stage;
planting request;
eligibility;
growth condition set;
anti-farm cap;
save record;
tests.
```

Não inclui:

```text
tilemap/prefab;
crop core rewrite;
final visual effects;
postgame values;
economy sale rules.
```

## 7. Modelo de domínio

### 7.1 DormantManaRootState

```text
Unknown
Rumored
AcquiredDormant
SitePrepared
PlantedDormant
Awakening
Rooted
BloomingRare
Stabilized
Corrupted
Withered
Sealed
```

### 7.2 DormantManaRootGrowthStage

```text
Dormant
SiteAccepted
FirstWatering
LunarStirring
FonteResonance
ManaBloomAttempt
RareBloomSuccess
BloomFailedDormant
CorruptionDetected
PurifiedStabilized
```

### 7.3 DormantManaRootPlantingRequest

```text
RootItemId
FarmAreaId
TileOrSiteId
FonteStage
LivingWaterAvailable
SoilTag
Season
LunarState
PostGameState optional
RequiredFragments[]
PlayerKnownLoreFlags[]
```

### 7.4 ManaRootEligibilityPolicy

```text
RequiresEndgameGate
RequiresFonteStage
RequiresLivingWater
RequiresSpecialSoil
RequiresLunarEvent
MaxActiveRoots
MaxBloomsPerSeason
ForbiddenBeforeLevel101
ForbiddenIfCorruptedWater
AllowedPostGameProfiles[]
```

### 7.5 ManaRootSaveRecord

```text
RootId
State
GrowthStage
FarmAreaId
SiteId
PlantedDay
LastCheckedDay
LastWateredWithLivingWaterDay optional
LastLunarEventId optional
BloomAttemptCount
SuccessfulBloomCount
CorruptionRisk
AntiFarmLockIds[]
```

---

## 8. Growth rules

```text
Normal watering:
  insufficient for Mana bloom.

Living Water:
  required for key transitions, not spammed.

Lunar event:
  required or strongly improves specific transition.

Fonte stage:
  required for awakening/stabilization.

Special soil:
  required or blocks planting.

PostGame:
  may modify probability/availability but cannot remove anti-farm caps.

Multiple roots:
  heavily limited.
```

---

## 9. Criteria

```text
Dormant root contracts exist.
Planting eligibility blocks early access.
Growth condition set requires rare gates.
Anti-farm caps exist.
Save record idempotent.
Tests cover early blocked, valid planting, missing Living Water, missing lunar event, max active root, bloom cap, reload no duplicate and corruption/wither states.
```

# /speckit.plan

## 10. Arquitetura alvo

```text
Assets/_Game/Scripts/Farm/Mana/DormantManaRootState.cs
Assets/_Game/Scripts/Farm/Mana/DormantManaRootGrowthStage.cs
Assets/_Game/Scripts/Farm/Mana/DormantManaRootPlantingRequest.cs
Assets/_Game/Scripts/Farm/Mana/ManaRootEligibilityPolicy.cs
Assets/_Game/Scripts/Farm/Mana/ManaRootGrowthConditionSet.cs
Assets/_Game/Scripts/Farm/Mana/ManaRootGrowthResult.cs
Assets/_Game/Scripts/Farm/Mana/ManaRootSaveRecord.cs
Assets/_Game/Scripts/Farm/Mana/ManaRootAntiFarmValidator.cs
Assets/_Game/Tests/EditMode/Farm/DormantManaRootEndgameFarmTests.cs
```

## 11. Arquivos permitidos

```text
Assets/_Game/Scripts/Farm/Mana/**
Assets/_Game/Scripts/Fonte/**
Assets/_Game/Scripts/World/Lunar/**
Assets/_Game/Scripts/Save/**
Assets/_Game/Scripts/UI/Farm/**
Assets/_Game/Scripts/Editor/Validation/**
Assets/_Game/Tests/EditMode/Farm/**
docs/validation/20_spec_dormant_mana_root_endgame_farm_future_runtime_execution_report.md
```

## 12. Arquivos proibidos

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

## 13. Tasks

- [ ] T001 — Ler fontes.
- [ ] T002 — Auditar farm/Mana/Fonte/lunar systems.
- [ ] T003 — Consolidar root contracts.
- [ ] T004 — Implementar validators.
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

- docs/design/gameplay/quests/QUESTS_MAIN_LORE_DIRECTION.md
- docs/design/gameplay/quests/QUESTS_MAIN_PROGRESSION_REFINEMENT_DIRECTION.md
- docs/design/gameplay/quests/QUEST_OBJECTIVE_EVENT_SYSTEM_DIRECTION.md
- docs/design/gameplay/farm/FARM_DESIGN_DIRECTION_v1.3.md
- docs/design/gameplay/farm/FARM_LAYOUT_SCALE_BUILDINGS_DIRECTION.md
- docs/design/gameplay/world/SEASONS_CALENDAR_WEATHER_LUNAR_DIRECTION.md
- docs/design/gameplay/loot_crafting_economy/LOOT_CRAFTING_ECONOMY_DIRECTION.md
- docs/design/gameplay/loot_crafting_economy/ECONOMY_PRICING_STOCK_REFRESH_DIRECTION.md
- docs/design/gameplay/magic/MAGIC_SPELLS_ACTIONS_DIRECTION.md
- docs/design/gameplay/magic/MAGIC_LEARNING_UNLOCKS_SOURCES_DIRECTION.md
- docs/design/gameplay/cave/CAVE_DESIGN_DIRECTION.md
- docs/design/gameplay/cave/CAVE_COMBAT_BALANCE_VULNERABILITIES_DIRECTION.md
- docs/design/gameplay/save_load/SAVE_LOAD_FULL_STATE_DIRECTION.md
- docs/design/gameplay/ui_ux/UI_UX_FULL_GAMEPLAY_DIRECTION.md
- docs/design/gameplay/ui_ux/UI_UX_MENU_SCREEN_FLOWS_DIRECTION.md
- docs/design/gameplay/city/CITY_DESIGN_DIRECTION_v1.2.md
- docs/design/gameplay/city/CITY_NPC_ROSTER_SERVICES_DIRECTION_v1.1.md
- docs/design/gameplay/companions/COMPANIONS_DIRECTION.md
- docs/design/gameplay/pets/PETS_DIRECTION.md
- docs/design/gameplay/bestiary/BESTIARY_KNOWLEDGE_DISCOVERY_DIRECTION.md
- docs/design/gameplay/social/SOCIAL_RELATIONSHIP_ROMANCE_DIRECTION.md

### Required interpretation

```text
Esta spec é future/mapped.
Ela deriva principalmente de FARM_DESIGN_DIRECTION_v1.3 e QUESTS_MAIN_PROGRESSION_REFINEMENT_DIRECTION.
Ela não transforma Mana em crop comum.
Ela não permite vender Mana em massa.
Ela não substitui economia comum, MP/cura, caverna, crafting ou magia.
Ela não cria pet runtime, romance/social deep, companion AI, scene/prefab/tilemap/asset ou save migration.
Mana é raro, difícil, endgame/lore-gated e bounded.
Quando houver conflito entre Farm, Main Progression, Economy, Magic, World/Lunar, Cave, Save ou UI directions, o executor deve parar e registrar CONFLICT.
```

---

## Direction / Refinement Coverage

### Covered from directions

- Fruto Mana deixa de ser tratado como crop plantável comum.
- Árvore de Mana é consciente, rara e escolhe onde crescer.
- Raiz Dormente de Mana é opção canônica de endgame da fazenda.
- Mana pode ser plantado, é difícil fazer florescer, não é poder absoluto e não é crop comum.
- Mecânicas possíveis incluem Semente/Raiz de Mana, solo especial, Água Viva, evento lunar, proteção contra corrupção, estação específica, proximidade da Fonte, requisitos de fragmentos, tempo longo e baixa produção.
- Anti-exploit: não vender em massa, não produzir toda semana, não substituir economia comum, não banalizar MP/cura e não virar requisito para todo sistema.

### Deferred / future from directions

- Tilemap/prefab.
- Final growth probability.
- Postgame final values.
- Economy sale rules.
- VFX/SFX.
- Save migration.

### Explicitly not redefined here

- Crop core.
- Farm expansion tiles.
- Final choice/postgame.
- Economy pricing.
- Living Water base function.

---

## 23A. Execution Readiness Matrix

| Área | Pergunta obrigatória | Evidência esperada | Status se faltar |
|---|---|---|---|
| Fonte canônica | Executor leu farm, main progression, economy, magic, world/lunar, cave, save e UI directions? | Lista no report. | PARTIAL |
| Estado real do repo | Sistemas existentes relacionados a Dormant Mana Root endgame farm foram auditados antes de criar novos? | Comandos `rg` e achados. | PARTIAL |
| Não duplicação | Existe sistema equivalente já implementado/parcial? | REUSE/HARDEN/CREATE/DEFER. | BLOCKED se duplicar |
| Mana não comum | A spec impede Mana como crop comum, produção semanal, commodity ou substituto econômico? | Tests/checklist. | BLOCKED se violar |
| Endgame gating | Mana exige Fonte/Fragmentos/lore/estação/lua/Água Viva/solo ou gates declarados. | Tests/checklist. | PARTIAL |
| Anti-economia | Sellability, stack, output, processing, shop and orders não criam loop infinito. | Tests/checklist. | PARTIAL |
| Anti-magia | Mana não banaliza MP, cura, purificação, RiskCorruption, capstone ou endgame spells. | Tests/checklist. | PARTIAL |
| Anti-spoiler | Anya, Fonte, Raiz de Mana, Pedra Negra, Arco, 101, final e postgame não vazam cedo. | Tests/visibility. | PARTIAL |
| Save/load | Houve alteração de schema? | Declaração NO ou STOP se migration necessária. | BLOCKED se alterar sem migration |
| Pet/social/companion | Nenhum runtime pet, romance/social deep ou companion AI criado. | Checklist explícito. | BLOCKED se violar |
| Testes | Lógica determinística nova tem EditMode tests quando praticável? | Test list. | PARTIAL |
| Report | Execution report criado? | `docs/validation/20_spec_dormant_mana_root_endgame_farm_future_runtime_execution_report.md`. | PARTIAL |

---

## 23B. Local Audit Checklist

Antes de alterar qualquer arquivo, Claude Code/Codex deve rodar e registrar no execution report:

```bash
rg -n "DormantManaRoot|RaizDormante|ManaRoot|FrutoMana|ManaBloom|FonteResonance|LivingWater|AguaViva" Assets/_Game/Scripts docs/design .specs
rg -n "Mana|RaizDormante|DormantManaRoot|FrutoMana|ManaSeed|LivingWater|AguaViva|Fonte|Anya|Lunar|Alihana|Senya|Nyx|MagicCrop|EndgameCrop|ManaEconomy|ManaSell|RiskCorruption|BlackStone|PedraNegra|Level101|PostGame|Pet|Companion|Romance" Assets/_Game/Scripts docs/design .specs
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
Given o jogador atingiu gates de endgame/lore necessários
When o sistema desta spec avalia Mana, Água Viva, solo, lua, crafting, magia ou economia
Then a ação é permitida apenas dentro dos limites declarados
And outputs são raros, rastreáveis, idempotentes e bounded
And não viram crop comum, commodity ou loop infinito.
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
Given o jogador ainda não tem Fonte/fragmentos/story/lunar/soil/living water gates suficientes
When tenta plantar, germinar, processar, vender ou usar Mana avançada
Then a ação é bloqueada com motivo seguro
And a UI não revela detalhes proibidos de Anya, Level 101, final ou postgame.
```

### Scenario 4 — Anti-exploit

```text
Given Mana foi obtida, plantada, germinada, processada, vendida, usada em spell/crafting ou consumida
When reload/retry/day transition/shop/crafting/process ocorre
Then produção não duplica
And venda/uso não substitui economia comum
And MP/cura/purificação não ficam triviais.
```

### Scenario 5 — Final human validation deferred

```text
Given o fluxo exige inspeção visual/gameplay de farm endgame, Fonte, Mana root, lunar event, crafting, shop, spell tooltip ou UI hint
When a implementação técnica terminar
Then o report registra cenário final em docs/validation/FINAL_HUMAN_VALIDATION_BY_WAVE.md
And não pede validação humana imediata por spec.
```

---

## 23D. Edge Cases and Failure Modes

A execução deve cobrir ou registrar risco residual para:

- Mana becomes common seed.
- Multiple roots spam.
- Normal watering blooms Mana.
- Bloom duplicates on reload.
- Postgame removes caps.
- UI reveals root early.
- Corrupted water accepted.
- Mana replaces common economy.

---

## 23E. Minimum Execution Report Template

```md
# Execution Report — Dormant Mana Root Endgame Farm Future Runtime

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

## Mana compliance
- Mana is not common crop:
- Dormant root / source gated:
- Living Water gated:
- Lunar/weather/source gates:
- Economy/sell cap:
- Crafting cap:
- Magic/MP/heal cap:
- Save/load safe:
- Anti-spoiler:
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
1. A implementação exigir alterar Packages/ ou ProjectSettings/.
2. A implementação exigir scene/prefab/tilemap/asset/font/icon/audio/timeline changes.
3. A implementação alterar save schema sem migration spec.
4. A implementação transformar Mana em crop comum, weekly crop, commodity, universal reagent ou farm loop.
5. A implementação permitir venda em massa, shop arbitrage, order exploit ou processing exploit.
6. A implementação banalizar MP, cura, purificação, spells de endgame ou RiskCorruption.
7. A implementação revelar Anya/Fonte/Level101/final/postgame/Pedra Negra/Arco cedo.
8. A implementação criar pet runtime, romance/social deep ou companion AI.
9. A implementação redefinir final choice/postgame/Level101 já especificados.
10. Não for possível decidir se sistema existente é canônico ou obsoleto.
```



---

## 25. Validações obrigatórias

Docs:

```powershell
.\tools\docs\validate_docs.ps1
```

Busca local mínima:

```bash
rg -n "Mana|RaizDormante|DormantManaRoot|LivingWater|AguaViva|Fonte|Anya|Lunar|Alihana|Senya|Nyx|ManaEconomy|ManaSell|RiskCorruption|BlackStone|Level101" Assets/_Game/Scripts docs/design .specs
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
Quando houver cenário visual/gameplay de Mana, Raiz Dormente, Água Viva, Fonte, lunar event, farm/crafting/economy/magic UI, registrar em execution report e vincular a docs/validation/FINAL_HUMAN_VALIDATION_BY_WAVE.md.
```

---

## 26. Testing Quality Gate

- Changed deterministic logic: YES, eligibility/growth/save/anti-farm logic is deterministic.
- Requires EditMode tests: YES for early-block/valid/missing-water/missing-lunar/cap/reload/corruption tests.
- Requires PlayMode automated or final human scenario: YES, DEFERRED for farm/Fonte/Mana visual validation.
- Requires regression test: YES if fixing existing Mana root exploit; otherwise NO.
- Human validation timing: DEFERRED_TO_FINAL_VALIDATION se houver UI/gameplay integrado; caso contrário NOT REQUIRED.
- Minimum validation evidence for ACCEPTED: docs validation PASS; C# build PASS if C# changed; EditMode tests PASS or NOT RUN justified; Mana root remains rare/endgame/bounded.

---

## 27. Definition of Done

```text
Spec executada sem alterar arquivos proibidos.
Contratos/data/runtime implementados apenas dentro do escopo.
Execution report criado em docs/validation/20_spec_dormant_mana_root_endgame_farm_future_runtime_execution_report.md.
Fonte/direction coverage preservado.
Validações obrigatórias PASS ou NOT RUN com motivo, impacto e mitigação.
Sem promoção indevida para ACCEPTED apenas por compile.
```

---

## 28. Anti-regressão

```text
Não transformar Mana em crop comum.
Não permitir venda/processamento/farm loop.
Não banalizar MP/cura/purificação.
Não revelar spoilers cedo.
Não reimplementar final choice/postgame/Level101.
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

Esta spec é future/mapped. Deve ser executada apenas quando farm endgame, Fonte, save/load, economy, magic, lunar/weather and main progression foundations estiverem estáveis ou quando houver decisão humana explícita de antecipar Mana.
