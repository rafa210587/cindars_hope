# SPEC — Living Water Mana Growth Conditions Future Runtime

> **Spec ID:** `20_spec_living_water_mana_growth_conditions_future_runtime`  
> **Status:** A implementar / Future mapped  
> **Wave:** WAVE 20 — Mana / Living Water / Endgame Farm Future  
> **Priority:** P1  
> **Type:** Runtime / Future / Fonte / Living Water / Mana Conditions  
> **Domain:** Living Water / Mana Growth / Fonte Stage / Purification / Anti-spam  
> **Parallelizable:** YES  
> **Parallel group:** WAVE_20_MANA_ENDGAME_FARM_FUTURE  
> **Can run with:** Mana economy/crafting/magic policy if no same files.  
> **Must not run with:** qualquer spec que altere Fonte base functions, crop core, save migration, final choice/postgame, scenes/prefabs/assets or economy pricing final.  
> **Repo lock scope:** `Assets/_Game/Scripts/Fonte/**`, `Assets/_Game/Scripts/Farm/Mana/**`, `Assets/_Game/Scripts/World/Lunar/**`, `Assets/_Game/Tests/EditMode/Fonte/**`, `docs/validation/20_spec_living_water_mana_growth_conditions_future_runtime_execution_report.md`  
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
  - Dormant Mana Root spec;
  - Mana lunar/corruption risk;
  - Mana economy/crafting/magic spec;
  - Fonte UI.
> **Scope:** definir/endurecer Água Viva como condição rara para Mana, purificação e crescimento, sem banalizar cura/MP ou fonte de economia.  
> **Out of scope:** Fonte base rewrite, final visual effects, crop core, save migration, postgame values.

---

# /speckit.specify

## 1. Contexto

Água Viva da Fonte é recurso raro ligado a Anya. Fragmento da Água desbloqueia Água Viva limitada; Fragmento da Vida desbloqueia purificação e cura avançada limitada. Mana pode usar Água Viva, mas não deve banalizar MP/cura/purificação nem virar recurso econômico comum.

---

## 2. Problema

Sem policy:

```text
Água Viva vira água comum;
Mana cresce com spam de Água Viva;
Fonte produz Água Viva infinita;
cura/purificação substitui medicina/comida/poções;
Água Viva purifica Pedra Negra sem custo;
Mana growth ignora corrupção;
reload duplica usos diários;
UI mostra Água Viva/Mana antes da Fonte acordar.
```

---

## 3. Objetivo

Criar/endurecer:

```text
LivingWaterUseContext;
LivingWaterManaInteraction;
LivingWaterDailyLimitPolicy;
LivingWaterPurificationTier;
LivingWaterManaGrowthGate;
LivingWaterAntiSpamState;
LivingWaterUseResult;
LivingWaterSpoilerPolicy.
```

---

## 4. Regras de design

```text
Água Viva é limitada.
Uso em Mana é especial e gated.
Água Viva não é água comum.
Água Viva não substitui economia de cura/poções.
Purificação forte requer Fragmento da Vida e gates.
Uso em Mana pode aumentar risco se corrupção/lua errada.
```

---

## 5. User stories / engineering stories

```text
Como jogador, quero usar Água Viva em Mana apenas quando fizer sentido.
Como Fonte, quero limitar usos por estágio e dia/evento.
Como farm, quero crescimento especial sem spam.
Como combat/magic, quero não perder relevância de MP/cura/poções.
Como UI, quero mostrar motivo de bloqueio seguro.
```

---

## 6. Escopo

Inclui:

```text
use context;
daily/event limits;
mana growth gates;
purification tiers;
anti-spam state;
use result;
spoiler policy;
tests.
```

Não inclui:

```text
Fonte base functions rewrite;
visuals;
crop core;
postgame effects;
economy pricing.
```

## 7. Modelo de domínio

### 7.1 LivingWaterUseContext

```text
UseType: HealLight | FatigueReduce | Ritual | SmallPurification | ManaRootWatering | ManaBloomAttempt | CorruptionPurification | AdvancedHeal
FonteStage
RequiredFragmentState
TargetId
TargetType
Day
Season
LunarState
CorruptionLevel
PostGameState optional
```

### 7.2 LivingWaterDailyLimitPolicy

```text
FonteStage
MaxUsesPerDay
MaxManaUsesPerSeason
MaxPurificationUsesPerWeek
RequiresRefillEvent
RequiresLunarWindow optional
NoMenuRefresh true
NoReloadDuplication true
```

### 7.3 LivingWaterManaGrowthGate

```text
RequiresDormantManaRoot
RequiresFonteStage
RequiresFragmentWater
RequiresFragmentLife optional
RequiresLivingWaterUse
RequiresLunarState optional
RequiresSpecialSoil
MaxAttemptsPerSeason
FailureState
CorruptionRiskModifier
```

### 7.4 LivingWaterUseResult

```text
Allowed
Blocked
Consumed
PartiallyConsumed
EffectApplied
EffectBlocked
ReasonTextKey
RiskAdded
CooldownStateAfter
AntiSpamKey
```

---

## 8. Use rules

```text
Light heal/fatigue:
  early limited, non-economy.

Small purification:
  fragment/gate limited.

Mana root watering:
  consumes rare use and records anti-spam.

Mana bloom attempt:
  limited per season/event and may fail.

Corruption purification:
  requires Fragmento da Vida or deeper gate.

Advanced healing:
  late limited, not replacement for all healing.
```

---

## 9. Criteria

```text
Living Water use contracts exist.
Limits are deterministic.
Mana growth gate requires rare conditions.
No reload/menu refresh duplication.
Purification tiers gated.
Tests cover early block, light use, Mana watering, bloom attempt cap, corruption purification gate, reload no duplicate and spoiler-hidden UI.
```

# /speckit.plan

## 10. Arquitetura alvo

```text
Assets/_Game/Scripts/Fonte/LivingWaterUseContext.cs
Assets/_Game/Scripts/Fonte/LivingWaterManaInteraction.cs
Assets/_Game/Scripts/Fonte/LivingWaterDailyLimitPolicy.cs
Assets/_Game/Scripts/Fonte/LivingWaterPurificationTier.cs
Assets/_Game/Scripts/Fonte/LivingWaterManaGrowthGate.cs
Assets/_Game/Scripts/Fonte/LivingWaterAntiSpamState.cs
Assets/_Game/Scripts/Fonte/LivingWaterUseResult.cs
Assets/_Game/Scripts/Fonte/LivingWaterSpoilerPolicy.cs
Assets/_Game/Tests/EditMode/Fonte/LivingWaterManaGrowthConditionsTests.cs
```

## 11. Arquivos permitidos

```text
Assets/_Game/Scripts/Fonte/**
Assets/_Game/Scripts/Farm/Mana/**
Assets/_Game/Scripts/World/Lunar/**
Assets/_Game/Scripts/UI/Fonte/**
Assets/_Game/Scripts/Editor/Validation/**
Assets/_Game/Tests/EditMode/Fonte/**
docs/validation/20_spec_living_water_mana_growth_conditions_future_runtime_execution_report.md
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
- [ ] T002 — Auditar Fonte/LivingWater/Mana references.
- [ ] T003 — Consolidar use/gate/limit contracts.
- [ ] T004 — Implementar anti-spam validators.
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

- Água Viva da Fonte é recurso raro ligado a Anya.
- Fragmento da Água desbloqueia Água Viva limitada.
- Fragmento da Vida desbloqueia purificação e cura avançada limitada.
- Mana growth pode usar Água Viva, evento lunar, proteção contra corrupção, proximidade da Fonte e fragmentos.
- Mana não deve banalizar MP/cura.
- Água Viva não deve virar recurso de economia infinita.

### Deferred / future from directions

- Fonte visual stage final.
- Postgame final policies.
- Potion economy final.
- Crop core.
- Save migration.

### Explicitly not redefined here

- Fonte stage progression.
- Final choice.
- Postgame modifiers.
- Healing combat formula.
- Potion crafting balance.

---

## 23A. Execution Readiness Matrix

| Área | Pergunta obrigatória | Evidência esperada | Status se faltar |
|---|---|---|---|
| Fonte canônica | Executor leu farm, main progression, economy, magic, world/lunar, cave, save e UI directions? | Lista no report. | PARTIAL |
| Estado real do repo | Sistemas existentes relacionados a Living Water Mana growth conditions foram auditados antes de criar novos? | Comandos `rg` e achados. | PARTIAL |
| Não duplicação | Existe sistema equivalente já implementado/parcial? | REUSE/HARDEN/CREATE/DEFER. | BLOCKED se duplicar |
| Mana não comum | A spec impede Mana como crop comum, produção semanal, commodity ou substituto econômico? | Tests/checklist. | BLOCKED se violar |
| Endgame gating | Mana exige Fonte/Fragmentos/lore/estação/lua/Água Viva/solo ou gates declarados. | Tests/checklist. | PARTIAL |
| Anti-economia | Sellability, stack, output, processing, shop and orders não criam loop infinito. | Tests/checklist. | PARTIAL |
| Anti-magia | Mana não banaliza MP, cura, purificação, RiskCorruption, capstone ou endgame spells. | Tests/checklist. | PARTIAL |
| Anti-spoiler | Anya, Fonte, Raiz de Mana, Pedra Negra, Arco, 101, final e postgame não vazam cedo. | Tests/visibility. | PARTIAL |
| Save/load | Houve alteração de schema? | Declaração NO ou STOP se migration necessária. | BLOCKED se alterar sem migration |
| Pet/social/companion | Nenhum runtime pet, romance/social deep ou companion AI criado. | Checklist explícito. | BLOCKED se violar |
| Testes | Lógica determinística nova tem EditMode tests quando praticável? | Test list. | PARTIAL |
| Report | Execution report criado? | `docs/validation/20_spec_living_water_mana_growth_conditions_future_runtime_execution_report.md`. | PARTIAL |

---

## 23B. Local Audit Checklist

Antes de alterar qualquer arquivo, Claude Code/Codex deve rodar e registrar no execution report:

```bash
rg -n "LivingWater|AguaViva|ManaGrowth|ManaRootWatering|PurificationTier|FonteStage|FragmentoVida|FragmentoAgua" Assets/_Game/Scripts docs/design .specs
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

- Living Water becomes normal water.
- Unlimited Living Water uses.
- Reload duplicates use cap.
- Mana bloom spam.
- Purification bypasses Fragmento da Vida.
- Healing economy trivialized.
- Corruption risk ignored.
- UI reveals Mana early.

---

## 23E. Minimum Execution Report Template

```md
# Execution Report — Living Water Mana Growth Conditions Future Runtime

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

- Changed deterministic logic: YES, use limit/gate/anti-spam logic is deterministic.
- Requires EditMode tests: YES for early-block/light-use/mana-watering/cap/purification/reload/spoiler tests.
- Requires PlayMode automated or final human scenario: YES, DEFERRED for Fonte/Mana UI and gameplay validation.
- Requires regression test: YES if fixing existing Living Water exploit; otherwise NO.
- Human validation timing: DEFERRED_TO_FINAL_VALIDATION se houver UI/gameplay integrado; caso contrário NOT REQUIRED.
- Minimum validation evidence for ACCEPTED: docs validation PASS; C# build PASS if C# changed; EditMode tests PASS or NOT RUN justified; Living Water remains rare and gated.

---

## 27. Definition of Done

```text
Spec executada sem alterar arquivos proibidos.
Contratos/data/runtime implementados apenas dentro do escopo.
Execution report criado em docs/validation/20_spec_living_water_mana_growth_conditions_future_runtime_execution_report.md.
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
