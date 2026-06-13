# SPEC — Fonte Anya Functions Living Water Respec Purification Runtime

> **Spec ID:** `10_spec_fonte_anya_functions_living_water_respec_purification_runtime`  
> **Status:** A implementar  
> **Wave:** WAVE 10 — Main Progression / Fonte / Endgame  
> **Priority:** P0  
> **Type:** Runtime / Fonte / Anya / Living Water / Respec / Purification  
> **Domain:** FonteAnya / Fonte Functions / LivingWater / Respec / Purification / Visual State  
> **Parallelizable:** NO  
> **Parallel group:** WAVE_10_MAIN_PROGRESSION_FONTE_LOCKED  
> **Can run with:** N/A  
> **Must not run with:** qualquer spec que altere MainProgressionSection, player death/respawn, skill tree respec, potion/healing balance, Mana crop, final choice, Fonte scene/prefab assets ou save schema.  
> **Repo lock scope:** `Assets/_Game/Scripts/Fonte/**`, `Assets/_Game/Scripts/MainProgression/**`, `Assets/_Game/Scripts/Player/**`, `Assets/_Game/Scripts/Skills/**`, `Assets/_Game/Scripts/Save/**`, `Assets/_Game/Scripts/Editor/Validation/**`, `Assets/_Game/Tests/EditMode/Fonte/**`, `docs/validation/10_spec_fonte_anya_functions_living_water_respec_purification_runtime_execution_report.md`  
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
  - `docs/design/gameplay/save_load/SAVE_LOAD_FULL_STATE_DIRECTION.md`
  - `docs/design/gameplay/ui_ux/UI_UX_FULL_GAMEPLAY_DIRECTION.md`
  - `docs/design/gameplay/ui_ux/UI_UX_MENU_SCREEN_FLOWS_DIRECTION.md`
  - `docs/design/gameplay/world/SEASONS_CALENDAR_WEATHER_LUNAR_DIRECTION.md`
  - `docs/design/gameplay/farm/FARM_DESIGN_DIRECTION_v1.3.md`
  - `docs/design/gameplay/farm/FARM_LAYOUT_SCALE_BUILDINGS_DIRECTION.md`
  - `docs/design/gameplay/city/CITY_DESIGN_DIRECTION_v1.2.md`
  - `docs/design/gameplay/city/CITY_NPC_ROSTER_SERVICES_DIRECTION_v1.1.md`
  - `docs/design/gameplay/city/CITY_LAYOUT_BUILDINGS_SCHEDULE_DIRECTION.md`
  - `docs/design/gameplay/cave/CAVE_DESIGN_DIRECTION.md`
  - `docs/design/gameplay/cave/CAVE_MONSTER_ROSTER_DIRECTION.md`
  - `docs/design/gameplay/loot_crafting_economy/LOOT_CRAFTING_ECONOMY_DIRECTION.md`
  - `docs/design/gameplay/loot_crafting_economy/ECONOMY_PRICING_STOCK_REFRESH_DIRECTION.md`
> **Blocks:**  
  - death/respawn flow;
  - skill tree respec;
  - LivingWater inventory/resource UI;
  - purification rituals;
  - Mana crop/fertilizer gating;
  - final choice.
> **Scope:** definir/endurecer FonteAnyaSection, estados da Fonte, unlocks por fragmentos, Água Viva limitada, respec, purificação/cura avançada limitada e guardrails anti-exploit.  
> **Out of scope:** prefab/visual final da Fonte, skill tree implementation changes, final choice, Mana crop runtime, full healing balance, save migration.

---

# /speckit.specify

## 1. Contexto

A Fonte começa adormecida e desbloqueia funções por fragmento: Estado 0 respawn/ponto de retorno; Fragmento da Água desbloqueia Água Viva limitada; Fragmento da Memória desbloqueia respec; Fragmento da Vida desbloqueia purificação/cura avançada limitada; Fragmento da Esperança habilita decisão final.

Esta spec cria o estado e serviços da Fonte, não o visual/prefab final.

---

## 2. Problema

Sem FonteAnyaSection:

```text
Água Viva pode virar item infinito;
respec pode destravar cedo;
purificação pode curar tudo sem custo;
Fonte unlocks podem virar QuestFlags soltas;
Fonte pode liberar tudo cedo;
Anya pode ser restaurada como NPC;
visual state pode não refletir fragmentos;
reload pode duplicar charges/unlocks.
```

---

## 3. Objetivo

Criar/endurecer:

```text
FonteAnyaSection;
FonteState;
FonteFunction enum;
LivingWaterState;
RespecUnlockState;
PurificationUnlockState;
FonteVisualStage;
FonteFunctionUnlockService;
FonteUseRequest/Result;
cost/cooldown/limit policies;
MainProgression adapter.
```

---

## 4. Regras de design

```text
Fonte é progressão, não vending machine.
Anya não volta inteira.
Fonte não libera tudo cedo.
Água Viva é limitada.
Respec vem com Fragmento da Memória.
Purificação/cura avançada limitada vem com Fragmento da Vida.
Decisão final vem com Fragmento da Esperança e nível 100/101.
Mana não vira economia comum por causa da Fonte.
```

---

## 5. User stories / engineering stories

```text
Como jogador, quero ver funções da Fonte desbloqueando por fragmento.
Como Fonte, quero controlar Água Viva, respec e purificação com limites.
Como MainProgression, quero informar fragment integration sem guardar estado da Fonte.
Como save/load, quero preservar charges/unlocks sem duplicação.
Como balance, quero impedir cura/respec/Água Viva/Mana infinitos.
```

---

## 6. Escopo

Inclui:

```text
FonteAnyaSection;
Fonte function unlocks;
LivingWater limited state;
respec availability state;
purification availability state;
Fonte use request/result;
cooldown/charge/limit policies;
visual stage metadata;
tests/validators.
```

Não inclui:

```text
Fonte prefab/visual effects;
player respawn implementation;
skill tree respec internals;
final choice resolution;
Mana crop runtime;
full healing balance.
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

- docs/design/gameplay/quests/QUEST_OBJECTIVE_EVENT_SYSTEM_DIRECTION.md
- docs/design/gameplay/quests/QUESTS_MAIN_LORE_DIRECTION.md
- docs/design/gameplay/quests/QUESTS_MAIN_PROGRESSION_REFINEMENT_DIRECTION.md
- docs/design/gameplay/save_load/SAVE_LOAD_FULL_STATE_DIRECTION.md
- docs/design/gameplay/ui_ux/UI_UX_FULL_GAMEPLAY_DIRECTION.md
- docs/design/gameplay/ui_ux/UI_UX_MENU_SCREEN_FLOWS_DIRECTION.md
- docs/design/gameplay/world/SEASONS_CALENDAR_WEATHER_LUNAR_DIRECTION.md
- docs/design/gameplay/farm/FARM_DESIGN_DIRECTION_v1.3.md
- docs/design/gameplay/farm/FARM_LAYOUT_SCALE_BUILDINGS_DIRECTION.md
- docs/design/gameplay/city/CITY_DESIGN_DIRECTION_v1.2.md
- docs/design/gameplay/city/CITY_NPC_ROSTER_SERVICES_DIRECTION_v1.1.md
- docs/design/gameplay/city/CITY_LAYOUT_BUILDINGS_SCHEDULE_DIRECTION.md
- docs/design/gameplay/cave/CAVE_DESIGN_DIRECTION.md
- docs/design/gameplay/cave/CAVE_MONSTER_ROSTER_DIRECTION.md
- docs/design/gameplay/loot_crafting_economy/LOOT_CRAFTING_ECONOMY_DIRECTION.md
- docs/design/gameplay/loot_crafting_economy/ECONOMY_PRICING_STOCK_REFRESH_DIRECTION.md

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

- Estado 0 da Fonte desbloqueia respawn/ponto de retorno após primeira morte/desmaio/evento crítico.
- Fragmento da Água desbloqueia Água Viva limitada para cura leve, redução controlada de cansaço, ritual simples, purificação pequena, pistas aquáticas e lagos especiais.
- Fragmento da Memória desbloqueia respec, revela inscrições, recupera diário de Cindar, resiste a esquecimento leve e desbloqueia diálogos.
- Fragmento da Vida desbloqueia purificação e cura avançada limitada para Água Viva corrompida, penalidades graves, NPC/animal/solo e proteção rara.
- Fragmento da Esperança desbloqueia decisão final.
- Não fazer: Fonte liberar tudo cedo, Anya virar NPC comum, Mana virar crop comum ou economia infinita.

### Deferred / future from directions

- Player death/respawn implementation.
- Skill tree respec mechanics.
- Fonte visual/prefab effects.
- Final choice.
- Mana crop runtime.
- Advanced corruption system.
- Full UI menu.

### Explicitly not redefined here

- MainProgressionSection.
- QuestStateSection.
- Player condition/respawn.
- Skill tree systems.
- Inventory item definitions.
- World lunar systems.

## 7. Modelo de domínio

### 7.1 FonteState

```text
Dormant
AwakenedReturnOnly
WaterFlowing
MemoryEchoing
LifeBlooming
HopeReady
FinalizedProtected
FinalizedSealed
FinalizedUsed
Corrupted
Damaged
```

### 7.2 FonteFunction

```text
ReturnPoint
LimitedLivingWater
Respec
MinorPurification
AdvancedPurification
CorruptionResistance
MemoryReveal
AquaticClueActivation
FinalChoicePreparation
PostGameState
```

### 7.3 FonteAnyaSection

```text
Version
FonteState
UnlockedFunctions[]
LivingWaterState
RespecState
PurificationState
VisualStage
FragmentIntegrationRefs[]
LastUseRecords[]
CorruptionState
FinalFonteState optional
LastValidatedVersion
```

### 7.4 LivingWaterState

```text
Unlocked
CurrentCharges
MaxCharges
RechargePolicy
LastRechargeDay optional
CanBeStoredAsItem
InventoryItemId optional
UseLimits
CorruptedWaterState optional
```

### 7.5 RespecState

```text
Unlocked
CostPolicy
CooldownPolicy optional
LastRespecDay optional
RequiresConfirmation
AllowedSkillSystems[]
```

### 7.6 PurificationState

```text
UnlockedMinor
UnlockedAdvanced
AllowedTargets[]
CostPolicy
CooldownPolicy
RequiresLivingWater
RequiresFragmentLife
CanPurifyCorruptedLivingWater
CanReduceCavePenalty
CanAffectNpcAnimalSoil
```

---

## 8. Unlock rules

```text
ReturnPoint:
  unlocked by first death/faint/critical return event.

LimitedLivingWater:
  requires Water fragment integrated.

Respec:
  requires Memory fragment integrated.

AdvancedPurification:
  requires Life fragment integrated.

FinalChoicePreparation:
  requires Hope fragment and level 100/101 route.
```

Unlock is idempotent.

---

## 9. Água Viva anti-exploit rules

```text
LivingWater is limited.
LivingWater cannot be mass sold.
LivingWater cannot replace all healing/potions.
LivingWater cannot trivialize cave penalties.
LivingWater cannot become infinite MP/cure loop.
LivingWater as item must be protected/special.
Corrupted LivingWater requires purification rule.
```

---

## 10. Respec rules

```text
Respec unavailable before Memory fragment.
Respec requires clear confirmation.
Respec may have cost/cooldown.
Respec must integrate with skill tree system through adapter.
Respec cannot duplicate skill points.
Respec cannot remove locked/story-required constraints unless skill tree allows.
```

---

## 11. Purification rules

```text
Minor purification may affect small corruption, clue activation or ritual.
Advanced purification may affect severe cave penalties, corrupted LivingWater, NPC/animal/soil events or rare corruption protection.
Purification cannot solve every consequence.
Purification cannot bypass final choice.
Purification cost/limit must be explicit.
```

---

## 12. Visual stage metadata

```text
Dormant: ruin, no water.
AwakenedReturnOnly: faint shimmer.
WaterFlowing: water circulates, weak light, Nymirian symbols.
MemoryEchoing: reflections show old scenes, Cindar echoes.
LifeBlooming: plants grow, water alive, green/gold particles.
HopeReady: final state pending choice.
Finalized*: state depends on final choice.
```

This spec stores stage metadata only; no scene/prefab editing.

---

## 13. Criteria

```text
FonteAnyaSection exists or is hardened.
Function unlocks are tied to fragment integration.
LivingWater has limits.
Respec only after Memory fragment.
Purification only after Life fragment for advanced uses.
FinalChoice function not available early.
Fonte state separated from QuestState/MainProgression.
Tests cover unlock order, LivingWater limits, respec gate, purification gate, duplicate unlock and section separation.
```

---

# /speckit.plan

## 14. Arquitetura alvo

```text
Assets/_Game/Scripts/Fonte/FonteState.cs
Assets/_Game/Scripts/Fonte/FonteFunction.cs
Assets/_Game/Scripts/Fonte/FonteAnyaSection.cs
Assets/_Game/Scripts/Fonte/LivingWaterState.cs
Assets/_Game/Scripts/Fonte/RespecState.cs
Assets/_Game/Scripts/Fonte/PurificationState.cs
Assets/_Game/Scripts/Fonte/FonteFunctionUnlockService.cs
Assets/_Game/Scripts/Fonte/FonteUseRequest.cs
Assets/_Game/Scripts/Fonte/FonteUseResult.cs
Assets/_Game/Scripts/Fonte/FonteAnyaValidator.cs
Assets/_Game/Tests/EditMode/Fonte/FonteAnyaFunctionsTests.cs
```

Consolidar existentes se houver.

---

## 15. Arquivos permitidos

```text
Assets/_Game/Scripts/Fonte/**
Assets/_Game/Scripts/MainProgression/**
Assets/_Game/Scripts/Player/**
Assets/_Game/Scripts/Skills/**
Assets/_Game/Scripts/Inventory/**
Assets/_Game/Scripts/Save/**
Assets/_Game/Scripts/Editor/Validation/**
Assets/_Game/Tests/EditMode/Fonte/**
docs/validation/10_spec_fonte_anya_functions_living_water_respec_purification_runtime_execution_report.md
```

---

## 16. Arquivos proibidos

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

---

## 17. Estratégia

```text
1. Auditar Fonte/player respawn/skill respec/healing systems.
2. Consolidar FonteAnyaSection and function states.
3. Implementar unlock service and validators.
4. Implementar use request/result contracts with limits.
5. Criar tests.
6. Criar report.
```

---

## 18. Paralelização

- Parallelizable: NO
- Must not run with:
  - MainProgression fragment state;
  - player death/respawn;
  - skill tree respec;
  - final choice;
  - Mana crop runtime.
- Reason: Fonte functions bridge many core systems.

---

## 19. Impacto save/load

```text
Does this change save schema? SHOULD BE NO if FonteAnyaSection exists; otherwise STOP.
Does this add save section? NO unless dedicated save migration is approved.
Does this require migration? NO unless adding FonteAnyaSection; then STOP.
Does this persist Unity references? NO.
```

---

## 20. Impacto eventos

```text
Adds events: CONDITIONAL, e.g. FonteFunctionUnlockedEvent, LivingWaterChangedEvent.
Changes events: SHOULD BE NO.
Requires unsubscribe pattern: YES if subscribers are created.
```

---

## 21. Impacto UI/Unity

```text
Changes UI: NO final; exposes Fonte menu state.
Changes scenes/prefabs/assets: NO.
Requires PlayMode/final human scenario: YES, DEFERRED for Fonte menu/visual flow.
```

---

## 22. Riscos

```text
Risco: LivingWater exploit.
Mitigação: charge/limit tests.

Risco: respec duplicates skill points.
Mitigação: adapter/dry-run tests if integrated.

Risco: Fonte state hidden in QuestFlags.
Mitigação: section separation tests.
```

---

# /speckit.tasks

## 23. Tasks

- [ ] T001 — Ler fontes.
- [ ] T002 — Auditar Fonte/respawn/respec/healing systems.
- [ ] T003 — Consolidar FonteAnyaSection.
- [ ] T004 — Implementar unlock/use contracts.
- [ ] T005 — Implementar validators.
- [ ] T006 — Criar tests.
- [ ] T007 — Rodar validações.
- [ ] T008 — Criar report.

## 23A. Execution Readiness Matrix

| Área | Pergunta obrigatória | Evidência esperada | Status se faltar |
|---|---|---|---|
| Fonte canônica | A execução leu Source Map e directions do domínio? | Lista de fontes no execution report. | PARTIAL |
| Estado real do repo | Sistemas existentes relacionados a Fonte Anya functions/LivingWater/respec/purification foram auditados antes de criar novos? | Comandos `rg` e achados no report. | PARTIAL |
| Não duplicação | Existe sistema equivalente já implementado/parcial? | Decisão REUSE/HARDEN/CREATE/DEFER. | BLOCKED se duplicar |
| Separação de estado | MainProgressionSection, FonteAnyaSection e QuestState permanecem separados? | Checklist explícito no report. | BLOCKED se misturar |
| Ordem canônica | Água -> Memória -> Vida -> Esperança foi preservada? | Tests/validator. | PARTIAL |
| Anti-spoiler | Arquivista, nível 101, escolha final, Anya completa e Vaelrion/Sethra não vazam cedo? | Tests/visibility policy. | PARTIAL |
| Anti-exploit | Água Viva, Mana, fragmentos, purificação, respec e final choice não viram economia/curas infinitas? | Tests/checklist. | PARTIAL |
| Anti-softlock | Main progression tem recovery para morte, item perdido, NPC indisponível, cave/state reload e eventos lunares? | Plano/validator. | PARTIAL |
| Pets/social future | A execução não implementou PetFuture/SocialFuture/romance/casamento profundo? | Checklist explícito. | BLOCKED se violar |
| Save/load | Houve alteração de schema? | Declaração explícita NO ou STOP se migration necessária. | BLOCKED se alterar sem migration |
| Eventos | Publishers/subscribers/lifecycle foram mapeados? | Mapa de eventos e unsubscribe policy se houver. | PARTIAL |
| UI/PlayMode | Há fluxo visual ou gameplay integrado? | Cenário final deferido documentado. | BUILD_VALIDATED no máximo se ausente |
| Testes | Há lógica determinística nova? | EditMode test ou NOT RUN justificado. | PARTIAL |
| Report | Execution report foi criado? | `docs/validation/10_spec_fonte_anya_functions_living_water_respec_purification_runtime_execution_report.md`. | PARTIAL |

---

## 23B. Local Audit Checklist

Antes de alterar qualquer arquivo, Claude Code/Codex deve rodar e registrar no execution report:

```bash
rg -n "FonteAnya|FonteState|LivingWater|AguaViva|Respec|Purification|FonteFunction|FragmentWater|FragmentMemory|FragmentLife|FragmentHope" Assets/_Game/Scripts docs/design .specs
rg -n "MainProgression|FonteAnya|Anya|Cindar|Fragment|WaterFragment|MemoryFragment|LifeFragment|HopeFragment|LivingWater|Respec|Purification|BlackStone|MemoryArc|Vaelrion|Sethra|Archivist|Level101|FinalChoice|Protect|Seal|Use" Assets/_Game/Scripts docs/design .specs
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
Given o sistema base relacionado a Fonte Anya functions/LivingWater/respec/purification existe ou foi criado de forma mínima
When o fluxo principal desta spec é executado
Then o comportamento segue o direction canônico
And MainProgressionSection, FonteAnyaSection, QuestState, flags e UI projection permanecem consistentes
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

### Scenario 3 — Anti-spoiler / staging

```text
Given o jogador ainda não atingiu ato/fragmento/profundidade/flag necessária
When UI, dialogue, quest, reward ou event projection consulta o estado
Then somente informação descoberta é exposta
And spoilers de Arquivista, 101, final choice, Anya completa, Vaelrion antagonista ou Sethra líder permanecem ocultos.
```

### Scenario 4 — Idempotency and replay safety

```text
Given fragmento, função da Fonte, boss gate, nível 101, final choice, purificação ou flag já foi aplicado
When reload, event repeat, scene reload ou reentrada de diálogo ocorre
Then o efeito não duplica
And o estado terminal não retorna para estado anterior sem política explícita.
```

### Scenario 5 — Final human validation deferred

```text
Given o fluxo exige inspeção visual de Fonte, efeito visual, ritual, boss gate, nível 101, final choice ou Quest Log
When a spec termina tecnicamente
Then o report registra cenário final em docs/validation/FINAL_HUMAN_VALIDATION_BY_WAVE.md
And não pede validação humana imediata por spec.
```

---

## 23D. Edge Cases and Failure Modes

A execução deve cobrir ou registrar risco residual para:

- LivingWater infinite recharge.
- LivingWater sellable as common commodity.
- Respec available before Memory fragment.
- Respec duplicates skill points.
- Purification solves all cave corruption without cost.
- FinalChoice available before Hope/101.
- Fonte state stored as QuestFlags.
- Scene/prefab edits required.

---

## 23E. Minimum Execution Report Template

```md
# Execution Report — Fonte Anya Functions Living Water Respec Purification Runtime

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

## Canon / progression compliance
- MainProgressionSection separated from QuestState:
- FonteAnyaSection separated from QuestState:
- Fragment order preserved:
- Anya not restored as NPC:
- Mana not common crop/economy:
- No PetFuture runtime:
- No SocialFuture/romance deep runtime:
- Anti-spoiler:
- Anti-softlock:
- Anti-exploit:

## Implementation decision
- REUSE_EXISTING / HARDEN_EXISTING / CREATE_MINIMAL / DEFER
- Justification:

## Files changed
- ...

## Functional evidence
- Happy path:
- Existing partial implementation:
- Anti-spoiler/staging:
- Idempotency/replay safety:
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
4. A implementação exigir reescrever Quest/Save/EventBus/Fonte/MainProgression/Cave canônico existente.
5. A implementação esconder MainProgression ou FonteAnya dentro de QuestState genérico.
6. A implementação restaurar Anya como NPC comum, templo ativo ou companion.
7. A implementação fazer Mana virar crop comum, dinheiro infinito, cura/MP infinita ou requisito de todo sistema.
8. A implementação revelar cedo Arquivista do Silêncio, escolha final, nível 101, boss oculto ou segredo de Anya.
9. A implementação tornar main quest expirável por tempo/calendário.
10. A implementação criar PetFuture, SocialFuture, romance/casamento profundo ou companion obrigatório para main quest.
11. Não for possível decidir se sistema existente é canônico ou obsoleto.
```


## 23G. Fonte Unlock Matrix

| Fragment/state | Function |
|---|---|
| Estado 0 / Return | respawn/ponto de retorno |
| Água | Água Viva limitada |
| Memória | respec |
| Vida | purificação/cura avançada limitada |
| Esperança | decisão final |

## 23H. Fonte Anti-exploit Matrix

| Function | Must not |
|---|---|
| LivingWater | infinite healing/MP/sellable commodity |
| Respec | duplicate SkillPoints |
| Purification | erase all consequences |
| FinalChoice | appear early |
| VisualStage | edit scene/prefab in this spec |
```


## 25. Validações obrigatórias

Docs:

```powershell
.\tools\docs\validate_docs.ps1
```

Busca local mínima:

```bash
rg -n "MainProgression|FonteAnya|Anya|Cindar|Fragment|WaterFragment|MemoryFragment|LifeFragment|HopeFragment|LivingWater|Respec|Purification|BlackStone|MemoryArc|Vaelrion|Sethra|Archivist|Level101|FinalChoice|Protect|Seal|Use" Assets/_Game/Scripts docs/design .specs
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
Quando houver cenário visual/gameplay de Fonte, fragmento, boss gate, nível 101, final choice ou main quest, registrar em execution report e vincular a docs/validation/FINAL_HUMAN_VALIDATION_BY_WAVE.md.
```

---

## 26. Testing Quality Gate

- Changed deterministic logic: YES, Fonte unlock/use/limit logic is deterministic.
- Requires EditMode tests: YES for unlock order/LivingWater/respec/purification/duplicate/section separation tests.
- Requires PlayMode automated or final human scenario: YES, DEFERRED for Fonte menu/visual flow.
- Requires regression test: YES if fixing existing Fonte unlock/use bug; otherwise NO.
- Human validation timing: DEFERRED_TO_FINAL_VALIDATION se houver UI/gameplay integrado; caso contrário NOT REQUIRED.
- Minimum validation evidence for ACCEPTED: docs validation PASS; C# build PASS if C# changed; EditMode tests PASS or NOT RUN justified; FonteAnya separated; no LivingWater exploit.

---

## 27. Definition of Done

```text
Spec executada sem alterar arquivos proibidos.
Contratos/data/runtime implementados apenas dentro do escopo.
Execution report criado em docs/validation/10_spec_fonte_anya_functions_living_water_respec_purification_runtime_execution_report.md.
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
Não misturar QuestState com MainProgression/FonteAnya.
Não restaurar Anya como NPC comum.
Não criar Mana como crop comum/economia infinita.
Não aplicar fragmento/Fonte/final choice duas vezes.
Não revelar spoiler oculto cedo.
Não fazer main quest expirar por tempo.
Não implementar PetFuture/SocialFuture/romance profundo.
Não pedir human test por spec.
Não executar runtime em massa antes da 01Q ou exceção humana explícita.
Não alterar SPEC_EXECUTION_ORDER.md.
```

---

## 29. Notas para execução posterior

Esta spec deve ser executada por Claude Code/Codex com auditoria local do repo, usando `rg` e validando o estado real dos sistemas já implementados.

Se a auditoria local revelar sistema equivalente já existente, a execução deve mudar para hardening/residual, não recriar do zero.
