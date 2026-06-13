# SPEC — Farm Fertilizer Crop Quality Runtime

> **Spec ID:** `05_spec_farm_fertilizer_crop_quality_runtime`  
> **Status:** A implementar  
> **Wave:** WAVE 05 — Farm Construction / Tools / Conditions  
> **Priority:** P1  
> **Type:** Runtime / Farm / Fertilizer / Crop Quality  
> **Domain:** Farm / Fertilizer / Soil Modifier / Quality / Crop Growth  
> **Parallelizable:** NO  
> **Parallel group:** WAVE_05_FARM_CONSTRUCTION_TOOLS_LOCKED  
> **Can run with:** N/A  
> **Must not run with:** qualquer spec que altere crop growth core, watering/rain, item quality schema, crafting/processing, economy crop values, lunar/arcane crops ou farm save schema.  
> **Repo lock scope:** `Assets/_Game/Scripts/Farm/**`, `Assets/_Game/Scripts/Items/**`, `Assets/_Game/Scripts/Crafting/**`, `Assets/_Game/Scripts/Editor/Validation/**`, `Assets/_Game/Tests/EditMode/Farm/**`, `docs/validation/05_spec_farm_fertilizer_crop_quality_runtime_execution_report.md`  
> **Depends on:**  
  - `docs/design/SPEC_SOURCE_MAP.md`
  - `docs/design/SPECIFICATION_PROCESS.md`
  - `docs/design/lore/VAALARA_GAME_CANON_DIRECTION_v1.0.md`
  - `docs/specs/SPEC_IMPLEMENTABLE_TEMPLATE.md`
  - `docs/specs/SPEC_WAVE_EXECUTION_PROTOCOL.md`
  - `docs/specs/SPEC_VALIDATION_MATRIX_MASTER.md`
  - `docs/specs/SPEC_EXISTING_IMPLEMENTATION_AUDIT.md`
  - `docs/project/CURRENT_STATE.md`
  - `docs/design/gameplay/farm/FARM_DESIGN_DIRECTION_v1.3.md`
  - `docs/design/gameplay/farm/FARM_LAYOUT_SCALE_BUILDINGS_DIRECTION.md`
  - `docs/design/gameplay/loot_crafting_economy/LOOT_CRAFTING_ECONOMY_DIRECTION.md`
  - `docs/design/gameplay/loot_crafting_economy/ECONOMY_PRICING_STOCK_REFRESH_DIRECTION.md`
  - `docs/design/gameplay/world/SEASONS_CALENDAR_WEATHER_LUNAR_DIRECTION.md`
  - `docs/specs/a_implementar/05_spec_farm_soil_crop_growth_quality_runtime.md`
  - `docs/specs/a_implementar/05_spec_farm_harvest_processing_quality_runtime.md`
> **Blocks:**  
  - crop quality economy;
  - crafting/processing recipes;
  - farm orders quality requirements;
  - lunar/arcane fertilizer future;
  - greenhouse and irrigation systems.
> **Scope:** formalizar fertilizantes como modificadores de solo/crop quality, com aplicação segura, duração, compatibilidade e save/load.  
> **Out of scope:** fertilizante lunar/arcano completo, Mana/Água Viva fertilizer, full recipe balance, visual effects, crop list balance, UI final.

---

# /speckit.specify

## 1. Contexto

Roadmap 2 pede fertilizante simples e qualidade inicial. Roadmap 4 pede fertilizante intermediário/avançado. Roadmap 5 cita fertilizante lunar/arcano. A spec de soil/crop growth já preparou quality hooks, mas não definiu fertilizantes.

Esta spec formaliza fertilizante como modificador de solo/crop, sem balance final e sem transformar Mana/Água Viva em insumo comum.

---

## 2. Problema

Sem contrato de fertilizante:

```text
fertilizante pode ser aplicado várias vezes sem regra;
quality pode ser recalculada diferente após reload;
fertilizante pode ignorar estação/greenhouse;
fertilizante lunar/arcano pode aparecer cedo;
item pode ser consumido sem efeito por erro;
fertilizante pode virar multiplicador infinito de ouro;
soil modifier pode ficar só no visual.
```

---

## 3. Objetivo

Criar/endurecer:

```text
FertilizerDefinition;
FertilizerTier;
SoilModifierState;
Application rules;
Compatibility/override policy;
Duration;
QualityModifier;
YieldModifier optional;
GrowthModifier optional;
Save/load stable state;
Tests.
```

---

## 4. Regras de design

```text
Fertilizante simples entra na fazenda produtiva.
Fertilizante avançado reduz repetição sem remover decisão.
Fertilizante lunar/arcano é endgame/future.
Mana/Água Viva não são fertilizante comum.
Quality melhora encomendas, presentes e receitas, mas não deve quebrar economia.
```

---

## 5. User stories / engineering stories

```text
Como jogador, quero aplicar fertilizante e saber que ele afetará o crop/solo.
Como crop quality, quero usar modifier determinístico.
Como save/load, quero preservar fertilizante aplicado.
Como economy, quero impedir multiplicação infinita de valor.
Como crafting, quero usar fertilizante como sink de recursos sem balance final agora.
```

---

## 6. Escopo

Inclui:

```text
fertilizer definitions;
soil modifier state;
application validation;
duration/consumption policy;
quality/yield/growth modifier hooks;
compatibility/stacking rules;
save/load;
tests/validators.
```

Não inclui:

```text
fertilizer crafting recipes final;
lunar/arcane fertilizer runtime;
Mana/Água Viva fertilizer;
visual VFX;
full crop quality balance;
UI final.
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

- docs/design/gameplay/farm/FARM_DESIGN_DIRECTION_v1.3.md
- docs/design/gameplay/farm/FARM_LAYOUT_SCALE_BUILDINGS_DIRECTION.md
- docs/design/gameplay/loot_crafting_economy/LOOT_CRAFTING_ECONOMY_DIRECTION.md
- docs/design/gameplay/loot_crafting_economy/ECONOMY_PRICING_STOCK_REFRESH_DIRECTION.md
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

- Roadmap 2 pede fertilizante simples e qualidade inicial.
- Roadmap 4 pede fertilizante intermediário/avançado.
- Roadmap 5 reserva fertilizante lunar/arcano para endgame.
- Produtos de qualidade são melhores para encomendas, presentes e receitas.
- Farm não deve gerar explosão de ouro por crops comuns.
- Mana/Água Viva são raros/lore/endgame, não insumo comum de craft em massa.

### Deferred / future from directions

- Fertilizante lunar/arcano completo.
- Receitas finais de fertilizante.
- Balance final por crop.
- VFX/UI final.
- Mana/Água Viva como sistema agrícola comum.
- Pet/companion farming buffs.

### Explicitly not redefined here

- Crop growth core.
- Inventory backend.
- Item quality schema.
- Economy final formula.
- Crafting recipe database.

## 7. Modelo de domínio

### 7.1 FertilizerDefinition

```text
FertilizerId
DisplayName
Tier: Simple | Improved | Advanced | LunarFuture | ArcaneFuture | LivingWaterFuture
AllowedCropTags optional
AllowedSoilStates
ApplicationTiming: BeforePlanting | AfterPlanting | AnyBeforeReady
DurationPolicy: OneCrop | NDays | UntilHarvest | PermanentUntilTilled
QualityModifier
YieldModifier optional
GrowthModifier optional
WaterRetentionModifier optional
StackingPolicy
RequiredFarmLevel
RequiredRecipeId optional
IsEndgameReserved
```

### 7.2 SoilModifierState

```text
PlotId
FertilizerId
AppliedDay
ExpiresDay optional
AppliesToCropInstanceId optional
ConsumedOnHarvest
RemainingUses optional
QualityModifierSnapshot
```

### 7.3 StackingPolicy

```text
ReplaceSameTier
ReplaceLowerTier
RejectIfAnyFertilizer
AllowAdditiveCapped
RecipeDefined
```

Default recomendado: rejeitar stacking aditivo amplo.

---

## 8. Application rules

```text
Can apply only to valid tillable/planted plot by rule.
Cannot apply to dead crop unless definition allows recovery/future.
Cannot apply lunar/arcane tiers unless farm/story unlock exists.
Applying consumes item only after validation succeeds.
If modifier cannot be persisted, do not consume item.
UI visual state cannot be source of truth.
```

---

## 9. Quality integration

```text
CropQualityResolver reads SoilModifierState.
Fertilizer contributes deterministic modifier.
Quality result persists on harvest item if supported.
Fertilizer does not directly create gold.
Fertilizer should not force all crops to max quality trivially.
```

---

## 10. Save/load

Must preserve:

```text
FertilizerId
PlotId
AppliedDay
ExpiresDay/Duration
AppliesToCropInstanceId
QualityModifierSnapshot if needed
```

Must not persist:

```text
fertilizer visual effect object;
ScriptableObject reference;
UI icon selection;
temporary preview as real modifier.
```

---

## 11. Criteria

```text
Valid fertilizer application consumes item once.
Invalid application does not consume item.
Modifier affects quality through crop quality resolver.
Stacking/override rules are explicit.
Applied fertilizer survives reload.
Endgame/lunar/arcane tiers remain gated/future.
Tests cover valid, invalid, stacking, reload and harvest quality.
```

---

# /speckit.plan

## 12. Arquitetura alvo

```text
Assets/_Game/Scripts/Farm/Fertilizer/FertilizerDefinition.cs
Assets/_Game/Scripts/Farm/Fertilizer/FertilizerTier.cs
Assets/_Game/Scripts/Farm/Fertilizer/SoilModifierState.cs
Assets/_Game/Scripts/Farm/Fertilizer/FertilizerApplicationService.cs
Assets/_Game/Scripts/Farm/Fertilizer/FertilizerQualityModifierProvider.cs
Assets/_Game/Tests/EditMode/Farm/FertilizerApplicationTests.cs
```

Consolidar existentes se houver.

---

## 13. Arquivos permitidos

```text
Assets/_Game/Scripts/Farm/**
Assets/_Game/Scripts/Items/**
Assets/_Game/Scripts/Inventory/**
Assets/_Game/Scripts/Crafting/**
Assets/_Game/Scripts/Save/**
Assets/_Game/Scripts/Editor/Validation/**
Assets/_Game/Tests/EditMode/Farm/**
docs/validation/05_spec_farm_fertilizer_crop_quality_runtime_execution_report.md
```

---

## 14. Arquivos proibidos

```text
Packages/**
ProjectSettings/**
Assets/**/*.unity
Assets/**/*.prefab
Assets/**/*.asset
docs/specs/SPEC_EXECUTION_ORDER.md
docs/specs/implementados/**
docs/refinements/implementados/**
docs/project/CURRENT_STATE.md
PROJECT_LOG.md
```

---

## 15. Estratégia

```text
1. Auditar fertilizer/quality/crop systems.
2. Consolidar FertilizerDefinition/SoilModifierState.
3. Implementar application validation.
4. Integrar CropQualityResolver.
5. Garantir save/load or STOP.
6. Criar tests.
7. Criar report.
```

---

## 16. Paralelização

- Parallelizable: NO
- Must not run with:
  - crop growth quality;
  - inventory quality schema;
  - economy crop pricing;
  - crafting recipes;
  - watering/greenhouse.
- Reason: fertilizer changes crop quality and item consumption.

---

## 17. Impacto save/load

```text
Does this change save schema? SHOULD BE NO if plot modifiers exist; otherwise STOP.
Does this add save section? NO unless dedicated save spec approves.
Does this require migration? NO unless adding modifier state; then STOP.
Does this persist Unity references? NO.
```

---

## 18. Impacto eventos

```text
Adds events: CONDITIONAL, e.g. FertilizerAppliedEvent if event contracts allow.
Changes events: SHOULD BE NO.
Requires unsubscribe pattern: YES if event subscribers are created.
```

---

## 19. Impacto UI/Unity

```text
Changes UI: NO final.
Changes scenes/prefabs/assets: NO.
Requires PlayMode/final human scenario: YES, DEFERRED for apply fertilizer -> harvest quality flow.
```

---

## 20. Riscos

```text
Risco: fertilizer item consumed without modifier saved.
Mitigação: validation before consume and tests.

Risco: quality/economy exploit.
Mitigação: capped stacking and balance deferred.

Risco: endgame fertilizer leak.
Mitigação: tier gate validation.
```

---

# /speckit.tasks

## 21. Tasks

- [ ] T001 — Ler fontes.
- [ ] T002 — Auditar fertilizer/quality/crop.
- [ ] T003 — Consolidar definitions/state.
- [ ] T004 — Implementar application service.
- [ ] T005 — Integrar quality resolver.
- [ ] T006 — Criar tests.
- [ ] T007 — Rodar validações.
- [ ] T008 — Criar report.

## 23A. Execution Readiness Matrix

| Área | Pergunta obrigatória | Evidência esperada | Status se faltar |
|---|---|---|---|
| Fonte canônica | A execução leu Source Map e directions do domínio? | Lista de fontes no execution report. | PARTIAL |
| Estado real do repo | Sistemas existentes relacionados a fertilizer/crop quality foram auditados antes de criar novos? | Comandos `rg` e achados no report. | PARTIAL |
| Não duplicação | Existe sistema equivalente já implementado/parcial? | Decisão REUSE/HARDEN/CREATE/DEFER. | BLOCKED se duplicar |
| Escopo | A execução ficou dentro de fertilizer/crop quality? | Arquivos alterados e justificativa. | PARTIAL |
| Economia | A spec não cria ouro infinito, bypass de custo ou upgrade grátis? | Anti-exploit tests/checklist. | PARTIAL |
| Save/load | Houve alteração de schema? | Declaração explícita NO ou STOP se migration necessária. | BLOCKED se alterar sem migration |
| Eventos | Publishers/subscribers/lifecycle foram mapeados? | Mapa de eventos e unsubscribe policy se houver. | PARTIAL |
| UI/PlayMode | Há fluxo visual ou gameplay integrado? | Cenário final deferido documentado. | BUILD_VALIDATED no máximo se ausente |
| Testes | Há lógica determinística nova? | EditMode test ou NOT RUN justificado. | PARTIAL |
| Report | Execution report foi criado? | `docs/validation/05_spec_farm_fertilizer_crop_quality_runtime_execution_report.md`. | PARTIAL |

---

## 23B. Local Audit Checklist

Antes de alterar qualquer arquivo, Claude Code/Codex deve rodar e registrar no execution report:

```bash
rg -n "Fertilizer|SoilModifier|CropQuality|QualityResolver|FertilizerApplied|Lunar|Arcane|LivingWater" Assets/_Game/Scripts docs/design docs/specs
rg -n "Building|Construction|Workshop|Storage|Fertilizer|Tool|Upgrade|Fatigue|Sleep|Hunger|Stamina|PlayerCondition|Save|Economy|Inventory" Assets/_Game/Scripts docs/design docs/specs
rg -n "TODO|FIXME|HACK|PARTIAL|DEFERRED|BUILD_VALIDATED|ACCEPTED" docs/specs docs/validation docs/IMPLEMENTATION_STATUS.md docs/project/CURRENT_STATE.md
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
Given o sistema base relacionado a fertilizer/crop quality existe ou foi criado de forma mínima
When o jogador executa o fluxo principal desta spec
Then o comportamento segue o direction canônico
And save/load, economia, inventário e estado do jogador permanecem consistentes
And nenhum sistema paralelo é criado
And o execution report registra evidência.
```

### Scenario 2 — Existing partial implementation

```text
Given já existe implementação parcial no repo
When a execução audita o sistema
Then ela escolhe HARDEN_EXISTING em vez de recriar do zero
And registra divergências do direction
And altera apenas o menor conjunto seguro de arquivos.
```

### Scenario 3 — Missing dependency

```text
Given uma dependência runtime não existe no repo local
When a execução encontra essa ausência
Then ela não inventa uma solução massiva
And marca como MISSING_BUT_DEFER ou cria apenas adapter/validator mínimo se seguro
And registra risco residual.
```

### Scenario 4 — Economy/save invalid state

```text
Given custo, material, upgrade, construção, condition ou save payload inválido
When validator/teste é executado
Then a violação é reportada com motivo claro
And o runtime não duplica item/ouro, não aplica upgrade grátis e não corrompe save.
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

- Fertilizer consumido sem persistir modifier.
- Fertilizer aplicado em crop/solo inválido.
- Stacking infinito gerando quality máxima trivial.
- Lunar/arcane fertilizer liberado cedo.
- Quality recalculada diferente após reload.
- Mana/Água Viva tratada como fertilizante comum.
- Inventory item perdido ou duplicado.
- Save schema alterado sem migration.

---

## 23E. Minimum Execution Report Template

```md
# Execution Report — Farm Fertilizer Crop Quality Runtime

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
- Existing partial implementation:
- Missing dependency:
- Economy/save invalid state:
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
4. A implementação exigir reescrever Farm/Economy/Inventory/PlayerCondition system canônico existente.
5. A implementação criar conflito com day transition, save ownership, stable IDs ou Testing Quality Gate.
6. A implementação criar pets/companions/animals runtime fora do escopo desta wave.
7. A implementação criar invasão/defesa/inimigos/dano a crops na fazenda, proibido no roadmap atual.
8. A implementação permitir ouro infinito, item duplication, upgrade grátis, custo negativo ou recovery exploit.
9. Não for possível decidir se sistema existente é canônico ou obsoleto.
```


## 23G. Fertilizer Tier Matrix

| Tier | Runtime current? | Notes |
|---|---:|---|
| Simple | YES | early quality hook |
| Improved | CONDITIONAL | mid progression |
| Advanced | FUTURE/CONDITIONAL | Roadmap 4 |
| LunarFuture | FUTURE | gated by lunar/endgame |
| ArcaneFuture | FUTURE | gated by lore/endgame |
| LivingWaterFuture | FUTURE/SPECIAL | rare Fonte resource, not common fertilizer |

## 23H. Stacking Guardrails

```text
Default: no broad additive stacking.
Reject duplicate fertilizer unless explicit policy.
Replacing lower tier must be clear.
Quality bonus must be capped.
```


## 25. Validações obrigatórias

Docs:

```powershell
.\tools\docs\validate_docs.ps1
```

Busca local mínima:

```bash
rg -n "Building|Construction|Workshop|Storage|Fertilizer|Tool|Upgrade|Fatigue|Sleep|Hunger|Stamina|PlayerCondition|Save|Economy|Inventory" Assets/_Game/Scripts docs/design docs/specs
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
Quando houver cenário visual/gameplay de construção, upgrade, fertilizante, ferramenta, sono/cansaço/fome/stamina, registrar em execution report e vincular a docs/validation/FINAL_HUMAN_VALIDATION_BY_WAVE.md.
```

---

## 26. Testing Quality Gate

- Changed deterministic logic: YES, application/stacking/quality modifier logic is deterministic.
- Requires EditMode tests: YES for apply/invalid/stacking/reload/quality tests.
- Requires PlayMode automated or final human scenario: YES, DEFERRED for apply fertilizer and harvest quality flow.
- Requires regression test: YES if fixing existing fertilizer/quality bug; otherwise NO.
- Human validation timing: DEFERRED_TO_FINAL_VALIDATION se houver FarmScene/UI/gameplay integrado; caso contrário NOT REQUIRED.
- Minimum validation evidence for ACCEPTED: docs validation PASS; C# build PASS if C# changed; EditMode tests PASS or NOT RUN justified; no save schema change; no quality exploit.

---

## 27. Definition of Done

```text
Spec executada sem alterar arquivos proibidos.
Contratos/data/runtime implementados apenas dentro do escopo.
Execution report criado em docs/validation/05_spec_farm_fertilizer_crop_quality_runtime_execution_report.md.
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
Não criar item/ouro/upgrade infinito.
Não pedir human test por spec.
Não executar runtime em massa antes da 01Q ou exceção humana explícita.
Não alterar SPEC_EXECUTION_ORDER.md.
```

---

## 29. Notas para execução posterior

Esta spec deve ser executada por Claude Code/Codex com auditoria local do repo, usando `rg` e validando o estado real dos sistemas já implementados.

Se a auditoria local revelar sistema equivalente já existente, a execução deve mudar para hardening/residual, não recriar do zero.
