# SPEC — Weather Lunar Cave Deep Modifiers Future Runtime

> **Spec ID:** `15_spec_weather_lunar_cave_deep_modifiers_future_runtime`  
> **Status:** A implementar / Future mapped  
> **Wave:** WAVE 15 — Calendar / Festival / Weather / Lunar Future Systems  
> **Priority:** P2  
> **Type:** Runtime / Future / Cave / Weather / Lunar Modifiers  
> **Domain:** Cave / Weather Modifiers / Lunar Events / Risk Reward / Anti-softlock  
> **Parallelizable:** NO  
> **Parallel group:** WAVE_15_CALENDAR_WEATHER_LUNAR_FUTURE  
> **Can run with:** N/A  
> **Must not run with:** qualquer spec que altere cave procedural generation, enemy stats/spawns final, loot tables final, combat formulas, main progression gates, pet cave runtime, save migration or scene assets.  
> **Repo lock scope:** `Assets/_Game/Scripts/Cave/**`, `Assets/_Game/Scripts/World/Weather/**`, `Assets/_Game/Scripts/World/Lunar/**`, `Assets/_Game/Tests/EditMode/Cave/**`, `docs/validation/15_spec_weather_lunar_cave_deep_modifiers_future_runtime_execution_report.md`  
> **Depends on:**  
  - `docs/design/SPEC_SOURCE_MAP.md`
  - `docs/design/SPECIFICATION_PROCESS.md`
  - `docs/design/lore/VAALARA_GAME_CANON_DIRECTION_v1.0.md`
  - `docs/specs/SPEC_IMPLEMENTABLE_TEMPLATE.md`
  - `docs/specs/SPEC_WAVE_EXECUTION_PROTOCOL.md`
  - `docs/specs/SPEC_VALIDATION_MATRIX_MASTER.md`
  - `docs/specs/SPEC_EXISTING_IMPLEMENTATION_AUDIT.md`
  - `docs/project/CURRENT_STATE.md`
  - `docs/design/gameplay/world/SEASONS_CALENDAR_WEATHER_LUNAR_DIRECTION.md`
  - `docs/design/gameplay/ui_ux/UI_UX_FULL_GAMEPLAY_DIRECTION.md`
  - `docs/design/gameplay/ui_ux/UI_UX_MENU_SCREEN_FLOWS_DIRECTION.md`
  - `docs/design/gameplay/farm/FARM_DESIGN_DIRECTION_v1.3.md`
  - `docs/design/gameplay/farm/FARM_LAYOUT_SCALE_BUILDINGS_DIRECTION.md`
  - `docs/design/gameplay/city/CITY_DESIGN_DIRECTION_v1.2.md`
  - `docs/design/gameplay/city/CITY_LAYOUT_BUILDINGS_SCHEDULE_DIRECTION.md`
  - `docs/design/gameplay/city/CITY_NPC_ROSTER_SERVICES_DIRECTION_v1.1.md`
  - `docs/design/gameplay/quests/QUEST_OBJECTIVE_EVENT_SYSTEM_DIRECTION.md`
  - `docs/design/gameplay/quests/QUESTS_MAIN_LORE_DIRECTION.md`
  - `docs/design/gameplay/quests/QUESTS_MAIN_PROGRESSION_REFINEMENT_DIRECTION.md`
  - `docs/design/gameplay/loot_crafting_economy/LOOT_CRAFTING_ECONOMY_DIRECTION.md`
  - `docs/design/gameplay/loot_crafting_economy/ECONOMY_PRICING_STOCK_REFRESH_DIRECTION.md`
  - `docs/design/gameplay/cave/CAVE_DESIGN_DIRECTION.md`
  - `docs/design/gameplay/cave/CAVE_MONSTER_ROSTER_DIRECTION.md`
  - `docs/design/gameplay/cave/CAVE_COMBAT_BALANCE_VULNERABILITIES_DIRECTION.md`
  - `docs/design/gameplay/magic/MAGIC_SPELLS_ACTIONS_DIRECTION.md`
  - `docs/design/gameplay/magic/MAGIC_LEARNING_UNLOCKS_SOURCES_DIRECTION.md`
  - `docs/design/gameplay/save_load/SAVE_LOAD_FULL_STATE_DIRECTION.md`
  - `docs/design/gameplay/companions/COMPANIONS_DIRECTION.md`
  - `docs/design/gameplay/pets/PETS_DIRECTION.md`
  - `docs/design/gameplay/bestiary/BESTIARY_KNOWLEDGE_DISCOVERY_DIRECTION.md`
> **Blocks:**  
  - cave procedural modifiers;
  - bestary discoveries;
  - quest cave contracts;
  - lunar/fonte/main progression hooks;
  - final human validation.
> **Scope:** definir/endurecer contratos futuros para modificadores de caverna por clima/lua: risco/recompensa, visibilidade, inscrições, Pedra Negra, Água Viva, elite chance e anti-softlock.  
> **Out of scope:** procedural generation rewrite, enemy stat authoring, loot table final, boss mechanics, pet cave runtime.

---

# /speckit.specify

## 1. Contexto

Weather/lunar events podem alterar caverna: Storm pode tornar caverna mais perigosa/recompensadora; Fog pode revelar passagens/estátuas/inscrições; Alihana pode fazer lagos brilharem, inscrições nymirianas aparecerem e caminhos de lore visíveis; Senya aumenta caos/magia/elite/risk reward; Nyx aumenta sombra, Pedra Negra, itens secretos, risco de memória/corrupção e atalhos/inscrições.

---

## 2. Problema

Sem contratos:

```text
lua muda caverna de forma opaca;
Fog revela caminho crítico sem pista;
Nyx aumenta Pedra Negra cedo demais;
Senya gera loot raro infinito;
Storm pune jogador sem aviso;
Alihana revela inscrição de main quest cedo;
modifier rerolla snapshot estável;
quest cave contract depende de evento raro sem fallback.
```

---

## 3. Objetivo

Criar/endurecer:

```text
CaveWeatherModifier;
CaveLunarModifier;
CaveRiskRewardModifier;
CaveLoreRevealModifier;
CaveCorruptionModifier;
CaveModifierVisibilityPolicy;
CaveModifierApplicationResult;
CaveTemporalSoftlockValidator.
```

---

## 4. Regras de design

```text
Cave modifiers devem ser sinalizados quando impactam risco/progresso.
Não bloquear progresso essencial sem pista/previsão/alternativa.
Não rerollar snapshot estável sem política.
Não revelar main quest/lore cedo.
Não gerar loot infinito.
Pet cave runtime permanece fora.
```

---

## 5. User stories / engineering stories

```text
Como jogador, quero entender que a caverna está mais perigosa ou diferente.
Como cave, quero aplicar modifier sem quebrar run seed/snapshot.
Como quest, quero usar condições lunares/climáticas com fallback.
Como knowledge, quero registrar descobertas sem spoiler.
Como economy/loot, quero risco/recompensa sem exploit.
```

---

## 6. Escopo

Inclui:

```text
weather/lunar cave modifier contracts;
risk/reward policy;
lore/corruption reveal policy;
visibility/forecast signaling;
snapshot compatibility;
anti-softlock validator;
tests.
```

Não inclui:

```text
procedural generation rewrite;
enemy spawn/loot table final;
boss mechanics;
pet cave runtime;
visual effects final.
```

## 7. Modelo de domínio

### 7.1 CaveWeatherModifier

```text
ModifierId
WeatherType
AppliesToDepthRange
AppliesToBiomeTags[]
RiskDelta
RewardDelta
VisibilityState
ForecastRequired
AllowedEffects[]
ForbiddenEffects[]
SnapshotPolicy
```

### 7.2 CaveLunarModifier

```text
ModifierId
LunarEventType
Moon: Alihana | Senya | Nyx | Alignment
AppliesToDepthRange
LoreRevealTags[]
EnemyModifierTags[]
LootModifierTags[]
CorruptionModifierTags[]
FonteReactionTags[]
RequiresDiscovery
SpoilerTier
```

### 7.3 CaveRiskRewardModifier

```text
RiskTierDelta
EliteChanceDelta
TrapChanceDelta
RareLootChanceDelta
MemoryEventChanceDelta
BlackStoneActivityDelta
MaxRewardCap
CanAffectBoss
RequiresWarning
```

### 7.4 CaveModifierApplicationResult

```text
Applied
SkippedNoMatchingBiome
SkippedDepth
BlockedBySpoiler
BlockedBySnapshotPolicy
BlockedByQuestSoftlock
Warnings[]
EffectiveRiskTier
EffectiveRewardTier
```

---

## 8. Weather mapping

```text
Storm:
  may increase risk/reward, elite/resource chance, but no severe unannounced punishment.

Fog:
  may reduce visibility lightly, reveal passages/statues/inscriptions if gated.

Snow/Cold:
  may increase fatigue/exterior logistics, winter cave resources.

Heat:
  may affect stamina/fatigue before cave entry or surface, not arbitrary cave damage.

Rain:
  may affect cave entrance/water/lakes, not full underground irrigation.
```

---

## 9. Lunar mapping

```text
Alihana:
  memory, dreams, glowing lakes, Nymirian inscriptions, lore paths, Água Viva stability.

Senya:
  magic instability, mutation, chaos enemies, elite chance, risk/reward.

Nyx:
  night/shadow, secret items, night creatures, Pedra Negra, memory/corruption risk, hidden shortcuts.

Alignments:
  rare, not common; triple alignment for seasonal/main quest or endgame only.
```

---

## 10. Snapshot compatibility

```text
If cave run uses stable snapshot:
  modifier must be captured at run start or at level generation boundary.

Modifier cannot:
  reroll rooms mid-run;
  duplicate loot nodes;
  invalidate checkpoint;
  invalidate corpse recovery;
  change boss gate requirement without state transition.
```

---

## 11. Anti-softlock

```text
Quest requiring weather/lunar cave condition must:
  show discovered condition;
  allow waiting/forecast/retry;
  provide fallback window;
  avoid one-time missable hardlock.

Rare event quest must not require unbounded RNG.
```

---

## 12. Criteria

```text
Weather/lunar cave modifiers exist.
Risk/reward bounded.
Snapshot policy defined.
Spoiler and quest softlock validators exist.
No pet runtime.
Tests cover Storm/Fog/Alihana/Senya/Nyx, snapshot stability, reward cap, hidden lore gate, quest fallback and no reroll exploit.
```

# /speckit.plan

## 13. Arquitetura alvo

```text
Assets/_Game/Scripts/Cave/Modifiers/CaveWeatherModifier.cs
Assets/_Game/Scripts/Cave/Modifiers/CaveLunarModifier.cs
Assets/_Game/Scripts/Cave/Modifiers/CaveRiskRewardModifier.cs
Assets/_Game/Scripts/Cave/Modifiers/CaveModifierApplicationResult.cs
Assets/_Game/Scripts/Cave/Modifiers/CaveTemporalModifierService.cs
Assets/_Game/Scripts/Cave/Modifiers/CaveTemporalSoftlockValidator.cs
Assets/_Game/Tests/EditMode/Cave/WeatherLunarCaveModifiersTests.cs
```

Consolidar existentes se houver.

## 14. Arquivos permitidos

```text
Assets/_Game/Scripts/Cave/**
Assets/_Game/Scripts/World/Weather/**
Assets/_Game/Scripts/World/Lunar/**
Assets/_Game/Scripts/Quests/**
Assets/_Game/Scripts/Bestiary/**
Assets/_Game/Scripts/Editor/Validation/**
Assets/_Game/Tests/EditMode/Cave/**
docs/validation/15_spec_weather_lunar_cave_deep_modifiers_future_runtime_execution_report.md
```

## 15. Arquivos proibidos

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

## 16. Tasks

- [ ] T001 — Ler fontes.
- [ ] T002 — Auditar cave/weather/lunar modifier systems.
- [ ] T003 — Consolidar modifier contracts.
- [ ] T004 — Implementar service/validators.
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

- docs/design/gameplay/world/SEASONS_CALENDAR_WEATHER_LUNAR_DIRECTION.md
- docs/design/gameplay/ui_ux/UI_UX_FULL_GAMEPLAY_DIRECTION.md
- docs/design/gameplay/ui_ux/UI_UX_MENU_SCREEN_FLOWS_DIRECTION.md
- docs/design/gameplay/farm/FARM_DESIGN_DIRECTION_v1.3.md
- docs/design/gameplay/farm/FARM_LAYOUT_SCALE_BUILDINGS_DIRECTION.md
- docs/design/gameplay/city/CITY_DESIGN_DIRECTION_v1.2.md
- docs/design/gameplay/city/CITY_LAYOUT_BUILDINGS_SCHEDULE_DIRECTION.md
- docs/design/gameplay/city/CITY_NPC_ROSTER_SERVICES_DIRECTION_v1.1.md
- docs/design/gameplay/quests/QUEST_OBJECTIVE_EVENT_SYSTEM_DIRECTION.md
- docs/design/gameplay/quests/QUESTS_MAIN_LORE_DIRECTION.md
- docs/design/gameplay/quests/QUESTS_MAIN_PROGRESSION_REFINEMENT_DIRECTION.md
- docs/design/gameplay/loot_crafting_economy/LOOT_CRAFTING_ECONOMY_DIRECTION.md
- docs/design/gameplay/loot_crafting_economy/ECONOMY_PRICING_STOCK_REFRESH_DIRECTION.md
- docs/design/gameplay/cave/CAVE_DESIGN_DIRECTION.md
- docs/design/gameplay/cave/CAVE_MONSTER_ROSTER_DIRECTION.md
- docs/design/gameplay/cave/CAVE_COMBAT_BALANCE_VULNERABILITIES_DIRECTION.md
- docs/design/gameplay/magic/MAGIC_SPELLS_ACTIONS_DIRECTION.md
- docs/design/gameplay/magic/MAGIC_LEARNING_UNLOCKS_SOURCES_DIRECTION.md
- docs/design/gameplay/save_load/SAVE_LOAD_FULL_STATE_DIRECTION.md
- docs/design/gameplay/companions/COMPANIONS_DIRECTION.md
- docs/design/gameplay/pets/PETS_DIRECTION.md
- docs/design/gameplay/bestiary/BESTIARY_KNOWLEDGE_DISCOVERY_DIRECTION.md

### Required interpretation

```text
Esta spec deriva de SEASONS_CALENDAR_WEATHER_LUNAR_DIRECTION.
Ela não redefine rotinas individuais de NPC, layouts, crops finais, preços finais, questlines completas, cave generation ou UI visual final.
Ela implementa apenas o recorte future/mapped declarado no escopo.
Eventos secretos, luas ocultas, eventos de quest não iniciados e condições exatas de Mana antes da lore não podem ser revelados.
Pets permanecem deferidos; qualquer menção a pet é compatibilidade/guardrail, não runtime.
Quando houver conflito com City, Farm, Quest, Economy, Cave, UI ou Save directions, o executor deve parar e registrar CONFLICT.
```

---

## Direction / Refinement Coverage

### Covered from directions

- Tempo/calendário/clima/luas conectam fazenda, cidade, caverna, economia, Fonte, Mana, quests, NPCs e eventos.
- Tempo cria decisão, mas não deve impedir o jogador de jogar.
- Calendário público pode registrar dia, estação, ano, clima previsto, evento lunar conhecido, festivais, restock especial, encomendas com prazo, eventos de cidade conhecidos, eventos de quest descobertos e dias de loja fechada.
- Eventos secretos não aparecem até serem descobertos.
- Previsão pode vir de calendário/quadro público, NPC/serviço/observação/upgrade.
- Luas podem afetar mundo antes do jogador entender, mas efeitos que bloqueiam progressão precisam ser descobertos, sinalizados ou previsíveis.
- Economia pode variar por restock semanal/sazonal, itens sazonais, festival, mercador raro, loja noturna, sementes raras, itens mágicos instáveis e demanda temporária.
- Evitar arbitragem fácil, preço opaco, restock ao abrir menu, reset de loja sem day transition, economia quebrada por Mana ou evento lunar gerando dinheiro infinito.

### Deferred / future from directions

- Astronomia real completa.
- UI visual final de calendário/clima/lua.
- Eventos específicos finais de cada festival.
- Balance final de preços/crops/economia.
- Questlines completas.
- Pet runtime.
- Romance/social deep runtime.
- Scene/prefab/tilemap edits.
- Full cave procedural changes.

### Explicitly not redefined here

- NPC schedules concretos.
- Crop growth formulas finais.
- Pricing formula final.
- Enemy stats/spawns finais.
- QuestState/MainProgression/FonteAnya ownership.
- Pet systems.
- Companion systems.
- UI prefab/layout final.

---

## 23A. Execution Readiness Matrix

| Área | Pergunta obrigatória | Evidência esperada | Status se faltar |
|---|---|---|---|
| Fonte canônica | Executor leu world/calendar/weather/lunar e domains afetados? | Lista no report. | PARTIAL |
| Estado real do repo | Sistemas existentes relacionados a weather/lunar cave deep modifiers foram auditados antes de criar novos? | Comandos `rg` e achados. | PARTIAL |
| Não duplicação | Existe sistema equivalente já implementado/parcial? | REUSE/HARDEN/CREATE/DEFER. | BLOCKED se duplicar |
| Anti-spoiler | Eventos secretos, luas ocultas, Mana, Anya, Pedra Negra, 100/101 e final choice não vazam? | Tests/checklist. | PARTIAL |
| Anti-softlock | Evento temporal/lunar/climático não bloqueia progresso essencial sem pista/controle razoável? | Tests/checklist. | PARTIAL |
| Anti-economia | Variações de evento não criam arbitragem, restock indevido ou dinheiro infinito. | Tests/checklist. | PARTIAL |
| Pet deferido | Não há runtime pet. | Checklist explícito. | BLOCKED se violar |
| Save/load | Houve alteração de schema? | Declaração NO ou STOP se migration necessária. | BLOCKED se alterar sem migration |
| UI/PlayMode | Há fluxo visual/gameplay integrado? | Cenário final deferido documentado. | BUILD_VALIDATED no máximo se ausente |
| Testes | Lógica determinística nova tem EditMode tests quando praticável? | Test list. | PARTIAL |
| Report | Execution report criado? | `docs/validation/15_spec_weather_lunar_cave_deep_modifiers_future_runtime_execution_report.md`. | PARTIAL |

---

## 23B. Local Audit Checklist

Antes de alterar qualquer arquivo, Claude Code/Codex deve rodar e registrar no execution report:

```bash
rg -n "CaveWeatherModifier|CaveLunarModifier|CaveRiskReward|Alihana|Senya|Nyx|Storm|Fog|BlackStone|SnapshotPolicy" Assets/_Game/Scripts docs/design docs/specs
rg -n "Calendar|Season|Weather|Festival|Forecast|Lunar|Alihana|Senya|Nyx|Restock|Seasonal|RareMerchant|NightShop|CaveModifier|Mana|Fonte|BlackStone|PedraNegra|SecretEvent|Pet|Companion" Assets/_Game/Scripts docs/design docs/specs
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
Given o estado de calendário/clima/lua/evento é conhecido ou descoberto
When o sistema desta spec projeta ou aplica seu recorte
Then somente efeitos autorizados e sinalizados ocorrem
And nenhum evento secreto/lore sensível é revelado cedo
And economia, quest, Fonte, Mana, cave e city schedules respeitam seus domínios de origem.
```

### Scenario 2 — Existing partial implementation

```text
Given já existe implementação parcial no repo
When a execução audita o sistema
Then ela escolhe HARDEN_EXISTING em vez de recriar do zero
And registra divergências do direction
And altera apenas o menor conjunto seguro de arquivos.
```

### Scenario 3 — Anti-spoiler temporal

```text
Given evento secreto, lua oculta, condição exata de Mana, evento de quest não iniciado, Pedra Negra, Anya, Arquivista, nível 100/101 ou final choice
When calendário/UI/forecast/economy/cave consulta eventos futuros
Then informação fica oculta, genérica ou gated
And só aparece quando descoberta/autorizada.
```

### Scenario 4 — Anti-softlock

```text
Given uma quest/progressão depende de clima, festival ou lua
When a condição não está ativa
Then o jogador tem pista, previsão, janela futura, alternativa ou wait/retry claro
And progresso essencial não fica bloqueado silenciosamente.
```

### Scenario 5 — Final human validation deferred

```text
Given o fluxo exige inspeção visual/gameplay de calendário, festival, forecast, cave modifier, shop/event board ou notification
When a implementação técnica terminar
Then o report registra cenário final em docs/validation/FINAL_HUMAN_VALIDATION_BY_WAVE.md
And não pede validação humana imediata por spec.
```

---

## 23D. Edge Cases and Failure Modes

A execução deve cobrir ou registrar risco residual para:

- Modifier rerolls stable snapshot.
- Storm creates unannounced severe punishment.
- Fog reveals main path/lore too early.
- Nyx reveals Pedra Negra/full corruption too early.
- Senya creates unbounded rare loot.
- Quest requires rare event with no fallback.
- Triple alignment used as common event.
- Pet cave runtime added.

---

## 23E. Minimum Execution Report Template

```md
# Execution Report — Weather Lunar Cave Deep Modifiers Future Runtime

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

## Calendar/weather/lunar compliance
- Secret events hidden:
- Known events visible:
- Quest conditions signaled:
- Economy anti-arbitrage:
- No pet runtime:
- No romance/social deep runtime:
- No scene/prefab/tilemap:
- Save/load safe:
- UI projection not source of truth:

## Implementation decision
- REUSE_EXISTING / HARDEN_EXISTING / CREATE_MINIMAL / DEFER
- Justification:

## Files changed
- ...

## Functional evidence
- Happy path:
- Existing partial implementation:
- Anti-spoiler temporal:
- Anti-softlock:
- Anti-economy exploit:
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
2. A implementação exigir scene/prefab/tilemap/asset wiring fora do escopo.
3. A implementação exigir save migration sem spec de migration.
4. A implementação revelar evento secreto/quest/lua/Mana/Pedra Negra/100/101/final cedo.
5. A implementação bloquear progressão essencial por clima/lua/festival sem pista/previsão/alternativa.
6. A implementação criar arbitragem, restock por abrir menu, preço opaco ou dinheiro infinito.
7. A implementação criar runtime pet.
8. A implementação criar romance/social deep runtime.
9. A implementação redefinir NPC schedules concretos, crop formulas, prices, enemy spawns ou cave generation.
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
rg -n "Calendar|Season|Weather|Festival|Forecast|Lunar|Alihana|Senya|Nyx|Restock|Seasonal|RareMerchant|NightShop|CaveModifier|Mana|SecretEvent" Assets/_Game/Scripts docs/design docs/specs
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
Quando houver cenário visual/gameplay de calendário, forecast, festival, shop/event board, cave weather/lunar modifier ou notification, registrar em execution report e vincular a docs/validation/FINAL_HUMAN_VALIDATION_BY_WAVE.md.
```

---

## 26. Testing Quality Gate

- Changed deterministic logic: YES, modifier application/snapshot/softlock logic is deterministic.
- Requires EditMode tests: YES for Storm/Fog/Alihana/Senya/Nyx/snapshot/reward-cap/spoiler/fallback tests.
- Requires PlayMode automated or final human scenario: YES, DEFERRED for cave visual/gameplay validation.
- Requires regression test: YES if fixing existing cave temporal modifier bug; otherwise NO.
- Human validation timing: DEFERRED_TO_FINAL_VALIDATION se houver UI/gameplay integrado; caso contrário NOT REQUIRED.
- Minimum validation evidence for ACCEPTED: docs validation PASS; C# build PASS if C# changed; EditMode tests PASS or NOT RUN justified; cave modifiers bounded and no softlock.

---

## 27. Definition of Done

```text
Spec executada sem alterar arquivos proibidos.
Contratos/data/runtime implementados apenas dentro do escopo.
Execution report criado em docs/validation/15_spec_weather_lunar_cave_deep_modifiers_future_runtime_execution_report.md.
Fonte/direction coverage preservado.
Validações obrigatórias PASS ou NOT RUN com motivo, impacto e mitigação.
Sem promoção indevida para ACCEPTED apenas por compile.
```

---

## 28. Anti-regressão

```text
Não revelar evento secreto cedo.
Não revelar Mana/Anya/Pedra Negra/100/101/final cedo.
Não bloquear progresso sem pista.
Não criar economia infinita.
Não resetar loja ao abrir menu.
Não implementar Pet runtime.
Não implementar romance/social deep runtime.
Não editar scenes/prefabs/assets.
Não pedir human test por spec.
Não executar runtime em massa antes da 01Q ou exceção humana explícita.
Não alterar SPEC_EXECUTION_ORDER.md.
```

---

## 29. Notas para execução posterior

Esta spec é future/mapped. Deve ser executada apenas quando calendar/time, economy, city schedules, UI, quest and cave foundations estiverem estáveis ou quando houver decisão humana explícita de antecipar este bloco.
