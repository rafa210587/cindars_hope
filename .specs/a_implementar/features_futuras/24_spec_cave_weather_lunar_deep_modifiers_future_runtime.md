# SPEC — Cave Weather Lunar Deep Modifiers Future Runtime

> **Spec ID:** `24_spec_cave_weather_lunar_deep_modifiers_future_runtime`  
> **Status:** A implementar / Future mapped  
> **Wave:** WAVE 24 — Remaining Future Closure  
> **Priority:** P2  
> **Type:** Runtime / Future / Closure  
> **Domain:** Cave Weather Lunar Deep Modifiers / Alihana Senya Nyx / Storm Fog Snow Cold Heat  
> **Parallelizable:** YES  
> **Parallel group:** WAVE_24_REMAINING_FUTURE_CLOSURE  
> **Can run with:** festival framework if no same world/cave files.  
> **Must not run with:** qualquer spec que altere cave generator core, enemy stats, weather/calendar core, final choice/Level101, scene assets or save migration.  
> **Repo lock scope:** `Assets/_Game/Scripts/Cave/Modifiers/**`, `Assets/_Game/Scripts/World/Weather/**`, `Assets/_Game/Scripts/World/Lunar/**`, `Assets/_Game/Tests/EditMode/Cave/**`, `docs/validation/24_spec_cave_weather_lunar_deep_modifiers_future_runtime_execution_report.md`  
> **Depends on:**  
  - `docs/design/SPEC_SOURCE_MAP.md`
  - `docs/design/SPECIFICATION_PROCESS.md`
  - `docs/design/lore/VAALARA_GAME_CANON_DIRECTION_v1.0.md`
  - `.specs/SPEC_IMPLEMENTABLE_TEMPLATE.md`
  - `.specs/SPEC_WAVE_EXECUTION_PROTOCOL.md`
  - `.specs/SPEC_VALIDATION_MATRIX_MASTER.md`
  - `.specs/SPEC_EXISTING_IMPLEMENTATION_AUDIT.md`
  - `docs/project/CURRENT_STATE.md`
  - `docs/design/gameplay/companions/COMPANIONS_DIRECTION.md`
  - `docs/design/gameplay/world/SEASONS_CALENDAR_WEATHER_LUNAR_DIRECTION.md`
  - `docs/design/gameplay/cave/CAVE_DESIGN_DIRECTION.md`
  - `docs/design/gameplay/cave/CAVE_COMBAT_BALANCE_VULNERABILITIES_DIRECTION.md`
  - `docs/design/gameplay/cave/CAVE_MONSTER_ROSTER_DIRECTION.md`
  - `docs/design/gameplay/combat/COMBAT_CORE_DIRECTION.md`
  - `docs/design/gameplay/combat/STATUS_EFFECTS_DIRECTION.md`
  - `docs/design/gameplay/farm/FARM_DESIGN_DIRECTION_v1.3.md`
  - `docs/design/gameplay/farm/FARM_LAYOUT_SCALE_BUILDINGS_DIRECTION.md`
  - `docs/design/gameplay/city/CITY_DESIGN_DIRECTION_v1.2.md`
  - `docs/design/gameplay/city/CITY_LAYOUT_BUILDINGS_SCHEDULE_DIRECTION.md`
  - `docs/design/gameplay/city/CITY_NPC_ROSTER_SERVICES_DIRECTION_v1.1.md`
  - `docs/design/gameplay/quests/QUEST_OBJECTIVE_EVENT_SYSTEM_DIRECTION.md`
  - `docs/design/gameplay/quests/QUESTS_MAIN_LORE_DIRECTION.md`
  - `docs/design/gameplay/quests/QUESTS_MAIN_PROGRESSION_REFINEMENT_DIRECTION.md`
  - `docs/design/gameplay/ui_ux/UI_UX_FULL_GAMEPLAY_DIRECTION.md`
  - `docs/design/gameplay/ui_ux/UI_UX_MENU_SCREEN_FLOWS_DIRECTION.md`
  - `docs/design/gameplay/loot_crafting_economy/LOOT_CRAFTING_ECONOMY_DIRECTION.md`
  - `docs/design/gameplay/loot_crafting_economy/ECONOMY_PRICING_STOCK_REFRESH_DIRECTION.md`
  - `docs/design/gameplay/magic/MAGIC_SPELLS_ACTIONS_DIRECTION.md`
  - `docs/design/gameplay/magic/MAGIC_LEARNING_UNLOCKS_SOURCES_DIRECTION.md`
  - `docs/design/gameplay/equipment/EQUIPMENT_WEAPONS_ARMOR_MATERIALS_DIRECTION.md`
  - `docs/design/gameplay/equipment/EQUIPMENT_MECHANICAL_BASELINES_DIRECTION.md`
  - `docs/design/gameplay/player/PLAYER_CORE_SYSTEMS_DIRECTION.md`
  - `docs/design/gameplay/player/PLAYER_DERIVED_ATTRIBUTES_DIRECTION.md`
  - `docs/design/gameplay/player/PLAYER_SKILL_TREES_DIRECTION.md`
  - `docs/design/gameplay/save_load/SAVE_LOAD_FULL_STATE_DIRECTION.md`
  - `docs/design/gameplay/pets/PETS_DIRECTION.md`
  - `docs/design/gameplay/bestiary/BESTIARY_KNOWLEDGE_DISCOVERY_DIRECTION.md`
> **Blocks:**  
  - cave biome modifier service;
  - weather forecast advanced;
  - lunar events in cave;
  - combat/environmental hazards.
> **Scope:** fechar weather/lunar deep modifiers na caverna: efeitos de Storm/Fog/Snow/Cold/Heat e eventos Alihana/Senya/Nyx na caverna com caps, forecast, anti-softlock e spoiler gates.  
> **Out of scope:** weather core, cave generator rewrite, enemy stat rebalance, Level101, final choice, scene/VFX/audio.

---

# /speckit.specify

## 1. Contexto

O roadmap lista weather/lunar deep modifiers na caverna como futuro. World/Time define que clima e luas afetam caverna; Storm pode tornar caverna mais perigosa ou recompensadora e gerar evento raro de Fonte/Água Viva com lua; Fog pode alterar entrada de caverna, revelar passagens/inscrições e tornar Pedra Negra mais perceptível sob Nyx; Cold pode alterar caverna e pets; luas afetam caverna, Fonte, Água Viva, Mana, quests, magia, crafting e eventos raros.

---

## 2. Problema

Sem policy:

```text
clima externo altera caverna sem aviso;
Storm aumenta loot sem risco;
Fog revela passagem secreta cedo;
Nyx revela Pedra Negra/final cedo;
Senya cria risco mágico opaco;
Alihana desbloqueia memória sem quest gate;
Cold/Heat pune jogador sem preparação;
reload rerolla modifier raro.
```

---

## 3. Objetivo

Criar:

```text
CaveEnvironmentalModifier;
CaveWeatherModifierPolicy;
CaveLunarModifierPolicy;
CaveDeepModifierScope;
CaveModifierForecastVisibility;
CaveModifierRewardRiskPolicy;
CaveModifierAntiSoftlockPolicy;
CaveModifierIdempotencyRecord.
```

---

## 4. Regras de design

```text
Modifiers profundos são futuros e opt-in por camada/gate.
Não devem quebrar a caverna núcleo.
Não devem reconfigurar CaveRunSeed sem regra.
Não devem revelar spoilers cedo.
Risco/recompensa deve ser explícito.
Eventos raros devem ser forecastable/discoverable ou ter fallback.
```

---

## 5. User stories / engineering stories

```text
Como jogador, quero perceber quando clima/lua alteram a caverna.
Como cave, quero aplicar modifiers sem rerollar layout.
Como combat, quero risco legível e limitado.
Como lore, quero Alihana/Senya/Nyx com identidade clara.
Como save/load, quero idempotência de eventos raros.
```

---

## 6. Escopo

Inclui:

```text
weather/lunar modifier contracts;
scope by depth/biome;
forecast visibility;
risk/reward caps;
anti-softlock;
idempotency;
tests.
```

Não inclui:

```text
weather core;
cave generator rewrite;
enemy stats;
VFX/audio;
Level101;
final choice.
```

## 7. Modelo de domínio

### 7.1 CaveEnvironmentalModifier

```text
ModifierId
SourceType: Weather | Lunar | Seasonal | Quest | Debug
WeatherType optional
LunarEventType optional
DepthRange
BiomeTags[]
RiskTier
RewardTier
SpoilerTier
CanApplyToBossGate
CanApplyToLevel101 false by default
```

### 7.2 CaveWeatherModifierPolicy

```text
WeatherType
AllowedDepthRanges[]
AllowedBiomes[]
Effects:
  VisibilityDelta
  FatigueDelta
  HazardChanceDelta
  ResourceChanceDelta
  EnemyBehaviorModifierKey optional
  TreasureRoomChanceDelta optional
RequiresForecastVisibility
MaxMagnitude
```

### 7.3 CaveLunarModifierPolicy

```text
LunarSource: Alihana | Senya | Nyx | SeasonalMajor | TripleAlignmentFuture
AllowedDepthRanges[]
AllowedLoreGates[]
EffectTheme: Memory | Instability | Shadow | Corruption | Fonte | Mana | Rumor
CanRevealInscription
CanRevealPassage
CanIncreaseCorruption
CanAlterMagicRisk
RequiresDiscovery
```

### 7.4 CaveDeepModifierScope

```text
ScopeId
AppliesToRun
AppliesToLevel
AppliesToRoom
AppliesToBossGate
AppliesToTreasureRoom
AppliesToLoreRoom
DoesNotRerollCaveSeed true
DoesNotChangeSnapshotIdentity true
```

### 7.5 CaveModifierRewardRiskPolicy

```text
RiskCaps
RewardCaps
NoGuaranteedRareLoot
NoMandatoryProgression
NoEconomyLoop
RequiresWarningForHighRisk
```

### 7.6 CaveModifierIdempotencyRecord

```text
ModifierApplicationId
CaveRunSeed
Level
Day
Weather
LunarEvent
Applied
OutcomeKey
RewardGranted
CannotRerollByReload
```

---

## 8. Weather mapping

```text
Storm:
  higher danger/resource chance, no severe crop/building style punishment, warning required.

Fog:
  visibility/mystery, rumor/passages/inscriptions if gated, no early final spoiler.

Snow/Cold:
  fatigue/logistics, winter mining/crafting value, warning/preparation.

Heat:
  stamina/fatigue/external context, fire biome interaction bounded.

Rain:
  cave entrance/wetness/minor resource/hazard hooks, not automatic magic irrigation.
```

---

## 9. Lunar mapping

```text
Alihana:
  memory/dream/inscription/Fonte hints, safe-ish, gated.

Senya:
  instability/magic/Mana risk, clear warning.

Nyx:
  shadow/secret/Pedra Negra perception, high spoiler gate.

Triple alignment:
  rare future, not normal farm/cave loop.
```

---

## 10. Criteria

```text
Cave modifier contracts exist.
Weather/lunar policies separated.
Scope preserves CaveRunSeed/snapshots.
Forecast/visibility gates exist.
Risk/reward caps exist.
Idempotency record prevents reload reroll.
Tests cover Storm risk, Fog spoiler gate, Nyx Black Stone gate, Senya risk warning, Alihana inscription gate, Cold fatigue cap, no Level101 default, no reroll by reload and no economy loop.
```

# /speckit.plan

## 11. Arquitetura alvo

```text
Assets/_Game/Scripts/Cave/Modifiers/CaveEnvironmentalModifier.cs
Assets/_Game/Scripts/Cave/Modifiers/CaveWeatherModifierPolicy.cs
Assets/_Game/Scripts/Cave/Modifiers/CaveLunarModifierPolicy.cs
Assets/_Game/Scripts/Cave/Modifiers/CaveDeepModifierScope.cs
Assets/_Game/Scripts/Cave/Modifiers/CaveModifierForecastVisibility.cs
Assets/_Game/Scripts/Cave/Modifiers/CaveModifierRewardRiskPolicy.cs
Assets/_Game/Scripts/Cave/Modifiers/CaveModifierAntiSoftlockPolicy.cs
Assets/_Game/Scripts/Cave/Modifiers/CaveModifierIdempotencyRecord.cs
Assets/_Game/Tests/EditMode/Cave/CaveWeatherLunarDeepModifiersTests.cs
```

## 12. Arquivos permitidos

```text
Assets/_Game/Scripts/Cave/Modifiers/**
Assets/_Game/Scripts/World/Weather/**
Assets/_Game/Scripts/World/Lunar/**
Assets/_Game/Scripts/UI/Cave/**
Assets/_Game/Scripts/Editor/Validation/**
Assets/_Game/Tests/EditMode/Cave/**
docs/validation/24_spec_cave_weather_lunar_deep_modifiers_future_runtime_execution_report.md
```

## 13. Arquivos proibidos

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

# /speckit.tasks

## 14. Tasks

- [ ] T001 — Ler fontes.
- [ ] T002 — Auditar cave/weather/lunar modifier systems.
- [ ] T003 — Consolidar modifier contracts.
- [ ] T004 — Implementar validators/idempotency.
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

- docs/design/gameplay/companions/COMPANIONS_DIRECTION.md
- docs/design/gameplay/world/SEASONS_CALENDAR_WEATHER_LUNAR_DIRECTION.md
- docs/design/gameplay/cave/CAVE_DESIGN_DIRECTION.md
- docs/design/gameplay/cave/CAVE_COMBAT_BALANCE_VULNERABILITIES_DIRECTION.md
- docs/design/gameplay/cave/CAVE_MONSTER_ROSTER_DIRECTION.md
- docs/design/gameplay/combat/COMBAT_CORE_DIRECTION.md
- docs/design/gameplay/combat/STATUS_EFFECTS_DIRECTION.md
- docs/design/gameplay/farm/FARM_DESIGN_DIRECTION_v1.3.md
- docs/design/gameplay/farm/FARM_LAYOUT_SCALE_BUILDINGS_DIRECTION.md
- docs/design/gameplay/city/CITY_DESIGN_DIRECTION_v1.2.md
- docs/design/gameplay/city/CITY_LAYOUT_BUILDINGS_SCHEDULE_DIRECTION.md
- docs/design/gameplay/city/CITY_NPC_ROSTER_SERVICES_DIRECTION_v1.1.md
- docs/design/gameplay/quests/QUEST_OBJECTIVE_EVENT_SYSTEM_DIRECTION.md
- docs/design/gameplay/quests/QUESTS_MAIN_LORE_DIRECTION.md
- docs/design/gameplay/quests/QUESTS_MAIN_PROGRESSION_REFINEMENT_DIRECTION.md
- docs/design/gameplay/ui_ux/UI_UX_FULL_GAMEPLAY_DIRECTION.md
- docs/design/gameplay/ui_ux/UI_UX_MENU_SCREEN_FLOWS_DIRECTION.md
- docs/design/gameplay/loot_crafting_economy/LOOT_CRAFTING_ECONOMY_DIRECTION.md
- docs/design/gameplay/loot_crafting_economy/ECONOMY_PRICING_STOCK_REFRESH_DIRECTION.md
- docs/design/gameplay/magic/MAGIC_SPELLS_ACTIONS_DIRECTION.md
- docs/design/gameplay/magic/MAGIC_LEARNING_UNLOCKS_SOURCES_DIRECTION.md
- docs/design/gameplay/equipment/EQUIPMENT_WEAPONS_ARMOR_MATERIALS_DIRECTION.md
- docs/design/gameplay/equipment/EQUIPMENT_MECHANICAL_BASELINES_DIRECTION.md
- docs/design/gameplay/player/PLAYER_CORE_SYSTEMS_DIRECTION.md
- docs/design/gameplay/player/PLAYER_DERIVED_ATTRIBUTES_DIRECTION.md
- docs/design/gameplay/player/PLAYER_SKILL_TREES_DIRECTION.md
- docs/design/gameplay/save_load/SAVE_LOAD_FULL_STATE_DIRECTION.md
- docs/design/gameplay/pets/PETS_DIRECTION.md
- docs/design/gameplay/bestiary/BESTIARY_KNOWLEDGE_DISCOVERY_DIRECTION.md

### Required interpretation

```text
Esta spec é future/mapped.
Ela fecha item explicitamente listado no roadmap macro como futuro.
Ela não altera código Unity durante geração da spec.
Ela não substitui specs núcleo já geradas.
Ela não altera Assets/, Packages/, ProjectSettings/, scenes, prefabs, tilemaps, ScriptableObject assets, audio, sprites, localization files ou save schema.
Se a execução futura exigir sistema fundacional inexistente, deve parar como BLOCKED ou DEFERRED, não improvisar arquitetura paralela.
Quando houver conflito entre directions, o domínio especializado vence para regra mecânica; UI_UX vence para apresentação/foco; Save/Load vence para persistência.
```

---

## Direction / Refinement Coverage

### Covered from directions

- World/Time define que clima e luas afetam caverna.
- Storm pode tornar caverna mais perigosa ou mais recompensadora e pode gerar evento raro de Fonte/Água Viva se combinado com lua.
- Fog pode alterar entrada de caverna, revelar passagens, estátuas ou inscrições em momentos de quest, e Pedra Negra fica mais perceptível sob Nyx.
- Cold pode alterar caverna e pets.
- As três luas afetam caverna, Fonte, Água Viva, Mana, quests, magia, crafting e eventos raros.
- Cave direction exige preservar CaveRunSeed, snapshots, boss gates, checkpoints, materializer e EnemySpawnResolver existentes.

### Deferred / future from directions

- VFX/SFX.
- Enemy stats.
- Cave generator rewrite.
- Weather core.
- Level101.
- Final choice.

### Explicitly not redefined here

- CaveRunManager.
- CaveProceduralGenerator.
- EnemySpawnResolver.
- Weather core.
- Lunar core.
- Resource tables.

---

## 23A. Execution Readiness Matrix

| Área | Pergunta obrigatória | Evidência esperada | Status se faltar |
|---|---|---|---|
| Fonte canônica | Executor leu todos os directions listados? | Lista no report. | PARTIAL |
| Estado real do repo | Sistemas existentes relacionados a cave weather/lunar deep modifiers foram auditados antes de criar novos? | Comandos `rg` e achados. | PARTIAL |
| Não duplicação | Existe sistema equivalente já implementado/parcial? | REUSE/HARDEN/CREATE/DEFER. | BLOCKED se duplicar |
| Scope futuro | A spec foi autorizada para execução agora ou ainda fica future/deferred? | Decisão registrada no report. | BLOCKED/DEFERRED |
| Guardrails | A execução preserva limites de gameplay, economia, UI, save e spoilers? | Checklist/tests. | PARTIAL |
| Anti-exploit | Não gera loot, gold, power, party size, progresso ou automação infinita. | Tests/checklist. | PARTIAL |
| Anti-spoiler | Não revela Anya, Fonte, Level 101, final, boss, secret events ou quest flags cedo. | Tests/checklist. | PARTIAL |
| Save/load | Houve alteração de schema? | Declaração NO ou STOP se migration necessária. | BLOCKED se alterar sem migration |
| UI/PlayMode | Há fluxo visual/gameplay integrado? | Cenário final deferido documentado. | BUILD_VALIDATED no máximo se ausente |
| Testes | Lógica determinística nova tem EditMode tests quando praticável? | Test list. | PARTIAL |
| Report | Execution report criado? | `docs/validation/24_spec_cave_weather_lunar_deep_modifiers_future_runtime_execution_report.md`. | PARTIAL |

---

## 23B. Local Audit Checklist

Antes de alterar qualquer arquivo, Claude Code/Codex deve rodar e registrar no execution report:

```bash
rg -n "CaveModifier|WeatherModifier|LunarModifier|Storm|Fog|Cold|Heat|Snow|Alihana|Senya|Nyx|CaveRunSeed" Assets/_Game/Scripts docs/design .specs
rg -n "Companion|Party|Tactical|AI|Squad|Festival|Minigame|Weather|Lunar|CaveModifier|Alihana|Senya|Nyx|Storm|Fog|Snow|Cold|Heat|Boss|Pet|Romance|FinalChoice|Level101" Assets/_Game/Scripts docs/design .specs
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
Given os sistemas fundacionais necessários existem
When o fluxo de cave weather/lunar deep modifiers roda
Then ele aplica somente efeitos declarados
And usa IDs estáveis e estado persistível seguro
And não duplica progresso, reward, economy, party power ou spoiler.
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
Given dependência fundacional não existe ou não está ACCEPTED
When a execução tenta criar runtime avançado
Then deve parar como BLOCKED/DEFERRED
And não criar arquitetura paralela nem fallback frágil.
```

### Scenario 4 — Anti-exploit / anti-spoiler

```text
Given o jogador tenta forçar reload, repetir evento, acumular bônus, explorar economia ou antecipar informação secreta
When o sistema processa a ação
Then ele mantém idempotência, caps e spoiler gates
And registra risco residual se algum teste não for praticável.
```

### Scenario 5 — Final human validation deferred

```text
Given o fluxo exige inspeção visual/gameplay
When a implementação técnica terminar
Then o report registra cenário final em docs/validation/FINAL_HUMAN_VALIDATION_BY_WAVE.md
And não pede validação humana imediata por spec.
```

---

## 23D. Edge Cases and Failure Modes

A execução deve cobrir ou registrar risco residual para:

- Modifier rerolls CaveRunSeed.
- Reload rerolls rare outcome.
- Fog reveals secret too early.
- Nyx reveals Black Stone/final too early.
- Storm creates economy loop.
- Cold/Heat punishes without warning.
- Modifier applies to Level101 by default.
- Risk/reward not visible to player.

---

## 23E. Minimum Execution Report Template

```md
# Execution Report — Cave Weather Lunar Deep Modifiers Future Runtime

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

## Future-scope compliance
- Execution authorized now:
- Foundation dependencies accepted:
- No duplicate architecture:
- Anti-exploit:
- Anti-spoiler:
- Save/load safe:
- UI not source of truth:
- No scene/prefab/assets:
- No pet/social/romance/companion bleed outside scope:

## Implementation decision
- REUSE_EXISTING / HARDEN_EXISTING / CREATE_MINIMAL / DEFER / BLOCKED
- Justification:

## Files changed
- ...

## Functional evidence
- Happy path:
- Existing partial:
- Missing dependency:
- Anti-exploit/spoiler:
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
2. A implementação exigir scene/prefab/tilemap/asset/font/icon/audio/localization changes.
3. A implementação alterar save schema sem migration spec.
4. A implementação reimplementar sistema já coberto em spec núcleo ou batch futuro anterior.
5. A implementação criar power creep, economia infinita, loot infinito, party size indevido, minigame obrigatório ou efeito secreto sem spoiler gate.
6. A implementação tornar companion, festival, weather/lunar, pet, romance, final choice ou Level101 obrigatório fora da regra canônica.
7. Não for possível decidir se sistema existente é canônico ou obsoleto.
```



---

## 25. Validações obrigatórias

Docs:

```powershell
.\tools\docs\validate_docs.ps1
```

Busca local mínima:

```bash
rg -n "CaveModifier|WeatherModifier|LunarModifier|Storm|Fog|Cold|Heat|Snow|Alihana|Senya|Nyx|CaveRunSeed" Assets/_Game/Scripts docs/design .specs
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
Quando houver cenário visual/gameplay, registrar em execution report e vincular a docs/validation/FINAL_HUMAN_VALIDATION_BY_WAVE.md.
```

---

## 26. Testing Quality Gate

- Changed deterministic logic: YES, modifier gating/scope/risk/reward/idempotency logic is deterministic.
- Requires EditMode tests: YES for Storm/Fog/Nyx/Senya/Alihana/Cold/Level101/reload/economy tests.
- Requires PlayMode automated or final human scenario: YES, DEFERRED for cave modifier visual/gameplay validation.
- Requires regression test: YES if fixing existing modifier bug; otherwise NO.
- Human validation timing: DEFERRED_TO_FINAL_VALIDATION se houver UI/gameplay integrado; caso contrário NOT REQUIRED.
- Minimum validation evidence for ACCEPTED: docs validation PASS; C# build PASS if C# changed; EditMode tests PASS or NOT RUN justified; cave modifiers preserve seed/snapshots and spoiler gates.

---

## 27. Definition of Done

```text
Spec executada sem alterar arquivos proibidos.
Contratos/data/runtime implementados apenas dentro do escopo.
Execution report criado em docs/validation/24_spec_cave_weather_lunar_deep_modifiers_future_runtime_execution_report.md.
Fonte/direction coverage preservado.
Validações obrigatórias PASS ou NOT RUN com motivo, impacto e mitigação.
Sem promoção indevida para ACCEPTED apenas por compile.
```

---

## 28. Anti-regressão

```text
Não reimplementar sistema já especificado.
Não alterar scenes/prefabs/assets.
Não criar economia/power/progresso infinito.
Não vazar spoilers.
Não pedir human test por spec.
Não executar runtime em massa antes da 01Q ou exceção humana explícita.
Não alterar SPEC_EXECUTION_ORDER.md.
```

---

## 29. Notas para execução posterior

Esta spec é future/mapped. Deve ser executada apenas quando as waves fundacionais relacionadas estiverem estáveis ou quando houver decisão humana explícita de antecipar este futuro.
