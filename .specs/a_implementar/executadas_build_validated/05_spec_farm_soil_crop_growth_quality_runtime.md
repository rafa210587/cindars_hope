# SPEC — Farm Soil Crop Growth Quality Runtime

> **Spec ID:** `05_spec_farm_soil_crop_growth_quality_runtime`  
> **Status:** A implementar  
> **Wave:** WAVE 05 — Farm Gameplay Core  
> **Priority:** P0  
> **Type:** Runtime / Farm / Soil / Crop Growth / Quality  
> **Domain:** Farm / Soil / Crops / Growth / Quality / Day Transition  
> **Parallelizable:** NO  
> **Parallel group:** WAVE_05_FARM_GAMEPLAY_LOCKED  
> **Can run with:** N/A  
> **Must not run with:** qualquer spec que altere FarmPlot save schema, crop definitions, watering/irrigation, rain integration, harvest yield, economy crop value, calendar/season runtime ou day transition order.  
> **Repo lock scope:** `Assets/_Game/Scripts/Farm/**`, `Assets/_Game/Scripts/Items/**`, `Assets/_Game/Scripts/World/**`, `Assets/_Game/Scripts/Editor/Validation/**`, `Assets/_Game/Tests/EditMode/Farm/**`, `docs/validation/05_spec_farm_soil_crop_growth_quality_runtime_execution_report.md`  
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
  - `docs/design/gameplay/world/SEASONS_CALENDAR_WEATHER_LUNAR_DIRECTION.md`
  - `.specs/a_implementar/02_spec_time_clock_day_transition_runtime.md`
  - `.specs/a_implementar/02_spec_calendar_season_year_runtime.md`
  - `.specs/a_implementar/05_spec_farm_scale_tilemap_player_footbox_runtime.md`
  - `.specs/a_implementar/05_spec_farm_level1_layout_fixed_anchors_runtime.md`
> **Blocks:**  
  - watering/irrigation/rain integration;
  - harvest and processing quality;
  - farm order delivery;
  - crop base value and economy;
  - save/load farm plot state.
> **Scope:** endurecer ou implementar o contrato determinístico de soil/plot/crop growth/quality sem reescrever o farm loop existente.  
> **Out of scope:** UI agrícola final, watering/irrigation implementation, greenhouse runtime, shipping/economy, animal crops/products, magical/lunar crops advanced, pets/companions.

---

# /speckit.specify

## 1. Contexto

A direção de farm diz que a base existente deve ser preservada: estados de solo/plot, arar, molhar, plantar, colher, crescimento condicionado por água, save de estado/seed/progresso/água/regrow e menu contextual agrícola já existem ou existem parcialmente.

Esta spec não deve recriar a fazenda. Ela transforma esse núcleo em contrato explícito para que as próximas specs de irrigação, colheita, processamento e shipping não contradigam o farm loop.

---

## 2. Problema

Sem contrato formal de soil/crop growth:

```text
soil state pode divergir entre visual, save e runtime;
crop pode crescer mais de uma vez por day transition;
planta pode morrer sem regra clara;
quality pode ser calculada em lugares diferentes;
season availability pode ser ignorada;
regrow pode resetar incorretamente;
crop ID inválido pode quebrar load;
rain/irrigation pode disputar ownership do campo IsWatered.
```

---

## 3. Objetivo

Ao final da execução, o repo deve ter ou documentar de forma testável:

```text
FarmPlotState canônico;
SoilState canônico;
CropGrowthState canônico;
CropDefinition mínimo;
GrowthDays/RegrowDays;
watered-today semantics;
days-without-water semantics;
dead crop semantics;
quality input hooks;
season availability hooks;
day transition growth processing;
save/load safe defaults.
```

---

## 4. Regras de design

```text
Fazenda gera decisão, não tarefa repetitiva vazia.
Crops comuns geram estabilidade, não explosão de ouro.
Produtos de qualidade são melhores para encomendas, presentes e receitas.
Agricultura cotidiana pertence a Thandra/cultura rural; Mana/Água Viva/Anya são camada sagrada rara.
Fruto Mana não é crop plantável comum.
Árvore de Mana é consciente, rara e escolhe onde crescer.
```

---

## 5. User stories / engineering stories

```text
Como jogador, quero plantar, regar, acompanhar crescimento e colher com feedback claro.
Como sistema de day transition, quero processar cada plot uma única vez por dia.
Como save/load, quero restaurar solo, crop, progresso, água, morte e regrow sem perder dados.
Como economia, quero calcular qualidade e valor depois de um crop consistente.
Como executor, quero auditar o sistema existente e endurecer o que falta.
```

---

## 6. Escopo

Inclui:

```text
FarmPlotState / SoilState / CropGrowthState contract;
CropDefinition contract mínimo;
growth processor na virada do dia;
dead crop rules;
regrow handling;
quality input hooks;
season availability hooks;
save/load defaults and invalid ID fallback interaction;
tests/validators.
```

Não inclui:

```text
watering/irrigation implementation;
rain integration internals;
greenhouse off-season logic;
harvest UI final;
shipping/economy;
crop balance final;
magical/lunar crop advanced runtime;
pets/companions/animals.
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

- Roadmap 0 já lista soil/plot, arar, molhar, plantar, colher, crescimento por água, save de seed/progresso/água/regrow e menu contextual agrícola como existentes/parciais a preservar.
- Roadmap 1 pede feedback visual de solo seco/molhado/plantado/pronto, morte de planta após 3 dias sem água, limpeza de planta morta e chuva molhando áreas externas.
- Roadmap 2 introduz qualidade inicial e fertilizante simples.
- Tempo/day transition deve calcular crescimento de crops, água/irrigação e morte de planta na virada do dia.
- Fazenda deve produzir recursos, preparar para a caverna, alimentar crafting/economia e criar rotina diária interessante.
- Fruto Mana não é crop comum plantável; Mana e Água Viva são raros/lore/endgame.

### Deferred / future from directions

- Fertilizante completo.
- Crops mágicos/lunares avançados.
- Greenhouse full runtime.
- Animal product quality.
- Visual crop sprite final.
- Full balance of every crop.

### Explicitly not redefined here

- Weather generation.
- Irrigation implementation.
- Shipping/economy formulas.
- Inventory backend.
- UI final/prefabs.
- Companion/pet/animal systems.

## 7. Modelo de domínio recomendado

### 7.1 FarmPlotState

```text
PlotId
TilePosition
SoilState
CropInstanceId optional
CropId optional
SeedItemId optional
GrowthProgressDays
GrowthRequiredDays
RegrowProgressDays optional
RegrowRequiredDays optional
IsWateredToday
WasWateredYesterday optional
DaysWithoutWater
IsDead
IsReadyToHarvest
LastProcessedDay
FertilizerId optional/future
QualityModifiersSnapshot optional/future
GreenhouseContext optional/future
```

### 7.2 SoilState

```text
None
Cleared
TilledDry
TilledWet
PlantedDry
PlantedWet
ReadyToHarvest
DeadCrop
Blocked
```

### 7.3 CropDefinition

```text
CropId
SeedItemId
HarvestItemId
DisplayName
AllowedSeasons
GrowthDays
RegrowDays optional
ExpectedYieldMin
ExpectedYieldMax
RequiresWater
DiesAfterDaysWithoutWater default 3 for normal crops
CanGrowInGreenhouse future
QualityEnabled
CanBeProcessed
CanBeShipped
Tags
```

### 7.4 CropGrowthState rules

```text
A crop grows at day transition if:
  - crop is planted;
  - crop is not dead;
  - crop is watered for that day or in a valid special context;
  - season/greenhouse context allows growth;
  - LastProcessedDay != current day being processed.

A crop does not grow if:
  - plot was not watered and no valid irrigation/rain/greenhouse override exists;
  - season invalid and no greenhouse override;
  - crop is dead;
  - ID invalid and fallback marks blocked/error.

A crop dies if:
  - DaysWithoutWater reaches crop death threshold;
  - unless crop definition says non-death/dormant behavior.
```

---

## 8. Day transition order inside this spec

Esta spec deve se encaixar na ordem global, sem tomar ownership de sistemas externos:

```text
1. Receive day transition context.
2. Read weather/rain/irrigation result as input only.
3. For each plot, skip if LastProcessedDay already equals processed day.
4. Apply water/season/growth/death/regrow rules.
5. Mark ready/dead/unchanged.
6. Reset or carry IsWateredToday according to watering spec contract.
7. Publish/update farm changed event if event system exists.
8. Persist via existing save/load provider or section ownership.
```

Regra: se a ordem real do repo for diferente, executor deve registrar CONFLICT ou HARDEN_EXISTING com justificativa.

---

## 9. Quality model

A spec deve preparar cálculo de qualidade sem exigir balance final.

Quality é resultado de inputs:

```text
CropDefinition.QualityEnabled
WateringConsistency
FertilizerTier optional/future
SeasonMatch
GreenhouseContext optional/future
Lunar/MagicModifier optional/future
RandomSeed optional if deterministic and saved/seeded
```

Quality tiers sugeridos:

```text
Q0 / Normal
Q1 / Good
Q2 / Excellent
Q3 / Rare
Q4 / Arcane/Future
```

Regras:

```text
Quality deve ser determinística ou seeded.
Quality não deve ser recalculada de forma diferente após save/load.
Quality final do item colhido deve ser carregada para ItemInstance/ItemStack se esse sistema suportar qualidade.
Se inventory ainda não suporta quality, registrar MISSING_BUT_DEFER sem inventar schema.
```

---

## 10. Save/load

A execução deve preservar ou declarar explicitamente:

```text
PlotId ou TilePosition estável;
CropId/SeedItemId por stable ID;
GrowthProgressDays;
IsWateredToday ou water state equivalente;
DaysWithoutWater;
IsDead;
Regrow state;
LastProcessedDay;
Quality-related fields only if already supported.
```

Proibido:

```text
persistir ScriptableObject/Prefab/UnityEngine.Object;
persistir crop visual as source of truth;
alterar save schema sem migration spec;
corrigir ID inválido silenciosamente para crop errado.
```

---

## 11. Critérios de aceite

```text
Crop growth is processed once per day.
Unwatered crop does not grow.
Crop death threshold is explicit and testable.
Dead crop can be cleaned/reset by existing or future tool/action.
Regrow crop preserves regrow behavior.
Invalid crop ID has safe fallback/report.
Quality hook exists without forcing final balance.
Save/load preserves plot/crop state.
No farm loop rewrite if existing system is partial/canonical.
```

---

# /speckit.plan

## 12. Arquitetura alvo

Possíveis arquivos, somente se não houver equivalentes:

```text
Assets/_Game/Scripts/Farm/Crops/FarmPlotState.cs
Assets/_Game/Scripts/Farm/Crops/SoilState.cs
Assets/_Game/Scripts/Farm/Crops/CropGrowthState.cs
Assets/_Game/Scripts/Farm/Crops/CropDefinition.cs
Assets/_Game/Scripts/Farm/Crops/CropGrowthProcessor.cs
Assets/_Game/Scripts/Farm/Crops/CropQualityResolver.cs
Assets/_Game/Scripts/Farm/Validation/FarmCropGrowthValidator.cs
Assets/_Game/Tests/EditMode/Farm/CropGrowthProcessorTests.cs
```

Se já existirem, **não criar duplicatas**. Consolidar/harden.

---

## 13. Arquivos permitidos

```text
Assets/_Game/Scripts/Farm/**
Assets/_Game/Scripts/Items/**
Assets/_Game/Scripts/Save/**
Assets/_Game/Scripts/Editor/Validation/**
Assets/_Game/Tests/EditMode/Farm/**
docs/validation/05_spec_farm_soil_crop_growth_quality_runtime_execution_report.md
```

Leitura permitida:

```text
Assets/_Game/Scripts/World/**
Assets/_Game/Scripts/Inventory/**
Assets/_Game/Scripts/Core/Events/**
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

## 15. Estratégia de implementação

```text
1. Auditar farm/crop/plot/save existentes.
2. Mapear contratos reais para o modelo canônico.
3. Se sistema existe, harden com validators/tests.
4. Se campos críticos faltarem, criar adapter/contract mínimo sem migration.
5. Integrar day transition somente pelo ponto canônico existente.
6. Criar tests de growth/no growth/death/regrow/invalid ID.
7. Criar report.
```

---

## 16. Paralelização

- Parallelizable: NO
- Must not run with:
  - watering/irrigation/rain;
  - harvest/processing;
  - shipping/sellpoint;
  - farm save schema;
  - crop economy/base value.
- Reason:
  - Soil/crop growth is the central state that these specs consume.

---

## 17. Impacto em save/load

```text
Does this change save schema? SHOULD BE NO.
Does this add save section? NO.
Does this require migration? NO unless missing crop state persistence; then STOP.
Does this persist Unity references? NO.
```

---

## 18. Impacto em eventos

```text
Adds events: CONDITIONAL, only if farm changed/crop ready/dead events are absent and event contracts allow.
Changes existing events: SHOULD BE NO.
Requires unsubscribe pattern: YES if subscribers are created.
```

---

## 19. Impacto UI/Unity

```text
Changes UI: NO final.
Changes scenes/prefabs/assets: NO.
Requires PlayMode/final human scenario: YES, DEFERRED for plant-water-sleep-grow-harvest readiness loop.
```

---

## 20. Riscos

```text
Risco: duplicar CropManager.
Mitigação: audit first and HARDEN_EXISTING.

Risco: crop grows twice.
Mitigação: LastProcessedDay and tests.

Risco: schema change required.
Mitigação: STOP and create save/migration spec.

Risco: quality invalidates economy.
Mitigação: hook only; balance deferred.
```

---

# /speckit.tasks

## 21. Tasks

- [ ] T001 — Ler fontes obrigatórias.
- [ ] T002 — Auditar FarmPlot/Crop/Save/DayTransition existentes.
- [ ] T003 — Mapear modelo real para contrato canônico.
- [ ] T004 — Implementar/harden growth processor.
- [ ] T005 — Implementar/harden death/regrow handling.
- [ ] T006 — Implementar quality hook sem balance final.
- [ ] T007 — Implementar validators.
- [ ] T008 — Criar EditMode tests.
- [ ] T009 — Rodar validações obrigatórias.
- [ ] T010 — Criar execution report.

## 23A. Execution Readiness Matrix

| Área | Pergunta obrigatória | Evidência esperada | Status se faltar |
|---|---|---|---|
| Fonte canônica | A execução leu Source Map e directions do domínio? | Lista de fontes no execution report. | PARTIAL |
| Estado real do repo | Sistemas existentes de farm/crop/economy/save foram auditados antes de criar novos? | Comandos `rg` e achados no report. | PARTIAL |
| Não duplicação | Existe sistema equivalente já implementado/parcial? | Decisão REUSE/HARDEN/CREATE/DEFER. | BLOCKED se duplicar |
| Escopo | A execução ficou dentro de soil/crop growth/quality? | Arquivos alterados e justificativa. | PARTIAL |
| Day transition | A spec respeita a ordem de virada do dia? | Evidência de integração/ordem no report. | PARTIAL |
| Save/load | Houve alteração de schema? | Declaração explícita NO ou STOP se migration necessária. | BLOCKED se alterar sem migration |
| Eventos | Publishers/subscribers/lifecycle foram mapeados? | Mapa de eventos e unsubscribe policy se houver. | PARTIAL |
| UI/PlayMode | Há fluxo visual ou gameplay integrado? | Cenário final deferido documentado. | BUILD_VALIDATED no máximo se ausente |
| Testes | Há lógica determinística nova? | EditMode test ou NOT RUN justificado. | PARTIAL |
| Report | Execution report foi criado? | `docs/validation/05_spec_farm_soil_crop_growth_quality_runtime_execution_report.md`. | PARTIAL |

---

## 23B. Local Audit Checklist

Antes de alterar qualquer arquivo, Claude Code/Codex deve rodar e registrar no execution report:

```bash
rg -n "FarmPlot|Crop|Soil|Growth|Regrow|Watered|DeadCrop|Quality|Season|DayTransition|LastProcessedDay" Assets/_Game/Scripts docs/design .specs
rg -n "Crop|FarmPlot|Soil|Water|Irrigation|Rain|Greenhouse|Harvest|Quality|Process|Shipping|SellPoint|DayTransition|Save" Assets/_Game/Scripts docs/design .specs
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
Given o sistema base relacionado a soil/crop growth/quality existe ou foi criado de forma mínima
When o jogador executa o fluxo principal desta spec durante um dia normal
Then o comportamento segue o direction canônico
And o estado é persistível/restaurável se aplicável
And nenhum sistema paralelo é criado
And o execution report registra evidência.
```

### Scenario 2 — Day transition integration

```text
Given o dia avança por dormir, colapso ou transição válida
When a ordem de virada do dia processa farm systems
Then a spec respeita a ordem declarada pelo sistema global
And crop growth, água, irrigação, shipping, restock, orders e weather não disputam responsabilidade
And qualquer dependência ausente é registrada como MISSING_BUT_DEFER.
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
Given um crop, plot, item, quality, process, shipping entry ou save payload inválido
When validator/teste é executado
Then a violação é reportada com motivo claro
And o runtime não corrompe save nem duplica recompensa/produto.
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

- Crop cresce duas vezes na mesma virada.
- Crop cresce sem água sem regra válida.
- Crop morre antes/depois do limiar por off-by-one.
- Regrow reseta seed/crop errado.
- Invalid CropId quebra load.
- Quality recalculada diferente após save/load.
- Season invalid ignored.
- Crop visual usado como fonte de verdade.

---

## 23E. Minimum Execution Report Template

```md
# Execution Report — Farm Soil Crop Growth Quality Runtime

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
- Day transition:
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
4. A implementação exigir reescrever farm/crop/economy/save system canônico existente.
5. A implementação criar conflito com day transition, save ownership, stable IDs ou Testing Quality Gate.
6. A implementação criar pet/companion/animal runtime fora do escopo desta wave.
7. A implementação permitir ouro infinito, duplicação de produto, shipping duplicado ou reward duplicado.
8. Não for possível decidir se sistema existente é canônico ou obsoleto.
```


## 23G. Crop Growth Test Matrix

| Case | Expected |
|---|---|
| Planted + watered + valid season | GrowthProgressDays +1 |
| Planted + dry + requires water | No growth; DaysWithoutWater +1 |
| DaysWithoutWater reaches threshold | IsDead = true |
| Dead crop day transition | No growth |
| Ready crop day transition | Remains ready until harvested |
| Regrow crop harvested | Enters regrow state instead of removing definition |
| Invalid CropId on load | Safe fallback/report; no crash |
| Already processed day | No duplicate growth |

## 23H. Quality Hook Guardrails

```text
Quality must be deterministic or seeded.
Quality cannot be UI-only.
Quality cannot be recalculated differently after reload.
Quality cannot require economy rebalance in this spec.
```


## 25. Validações obrigatórias

Docs:

```powershell
.\tools\docs\validate_docs.ps1
```

Busca local mínima:

```bash
rg -n "Crop|FarmPlot|Soil|Water|Irrigation|Rain|Greenhouse|Harvest|Quality|Process|Shipping|SellPoint|DayTransition|Save" Assets/_Game/Scripts docs/design .specs
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
Quando houver cenário visual/gameplay de FarmScene, plantio, irrigação, colheita, processamento ou shipping, registrar em execution report e vincular a docs/validation/FINAL_HUMAN_VALIDATION_BY_WAVE.md.
```

---

## 26. Testing Quality Gate

- Changed deterministic logic: YES, crop growth/death/regrow/quality hook logic is deterministic.
- Requires EditMode tests: YES for growth/no-growth/death/regrow/invalid ID/quality determinism tests.
- Requires PlayMode automated or final human scenario: YES, DEFERRED for plant-water-sleep-grow visual/gameplay flow.
- Requires regression test: YES if fixing existing crop growth/save/death bug; otherwise NO.
- Human validation timing: DEFERRED_TO_FINAL_VALIDATION se houver FarmScene/UI/gameplay integrado; caso contrário NOT REQUIRED.
- Minimum validation evidence for ACCEPTED: docs validation PASS; C# build PASS if C# changed; EditMode tests PASS or NOT RUN justified; no save schema change; growth processed once per day; report created.

---

## 27. Definition of Done

```text
Spec executada sem alterar arquivos proibidos.
Contratos/data/runtime implementados apenas dentro do escopo.
Execution report criado em docs/validation/05_spec_farm_soil_crop_growth_quality_runtime_execution_report.md.
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
Não criar inflação/duplicação econômica.
Não pedir human test por spec.
Não executar runtime em massa antes da 01Q ou exceção humana explícita.
Não alterar SPEC_EXECUTION_ORDER.md.
```

---

## 29. Notas para execução posterior

Esta spec deve ser executada por Claude Code/Codex com auditoria local do repo, usando `rg` e validando o estado real dos sistemas já implementados.

Se a auditoria local revelar sistema equivalente já existente, a execução deve mudar para hardening/residual, não recriar do zero.
