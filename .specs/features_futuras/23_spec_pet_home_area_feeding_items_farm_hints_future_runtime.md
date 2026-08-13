# SPEC — Pet Home Area Feeding Items Farm Hints Future Runtime

> **Spec ID:** `23_spec_pet_home_area_feeding_items_farm_hints_future_runtime`  
> **Status:** A implementar / Future mapped / HOLD until explicit pet-scope approval  
> **Wave:** WAVE 23 — Pets Advanced Future  
> **Priority:** P1  
> **Type:** Runtime / Future / HOLD / Pet Farm  
> **Domain:** Pet Home Area / Bed Bowl Toy / Feeding / Farm Hints / Finds  
> **Parallelizable:** YES  
> **Parallel group:** WAVE_23_PETS_ADVANCED_FUTURE  
> **Can run with:** pet core if already approved and no same files.  
> **Must not run with:** qualquer spec que altere farm automation, crop core, economy pricing final, item database final, scene/prefab assets or pet cave behavior.  
> **Repo lock scope:** `Assets/_Game/Scripts/Pets/Farm/**`, `Assets/_Game/Scripts/Pets/Items/**`, `Assets/_Game/Tests/EditMode/Pets/**`, `docs/validation/23_spec_pet_home_area_feeding_items_farm_hints_future_runtime_execution_report.md`  
> **Depends on:**  
  - `docs/design/SPEC_SOURCE_MAP.md`
  - `docs/design/SPECIFICATION_PROCESS.md`
  - `docs/design/lore/VAALARA_GAME_CANON_DIRECTION_v1.0.md`
  - `.specs/SPEC_IMPLEMENTABLE_TEMPLATE.md`
  - `.specs/SPEC_WAVE_EXECUTION_PROTOCOL.md`
  - `.specs/SPEC_VALIDATION_MATRIX_MASTER.md`
  - `.specs/SPEC_EXISTING_IMPLEMENTATION_AUDIT.md`
  - `docs/project/CURRENT_STATE.md`
  - `docs/design/gameplay/pets/PETS_DIRECTION.md`
  - `docs/design/gameplay/farm/FARM_DESIGN_DIRECTION_v1.3.md`
  - `docs/design/gameplay/farm/FARM_LAYOUT_SCALE_BUILDINGS_DIRECTION.md`
  - `docs/design/gameplay/companions/COMPANIONS_DIRECTION.md`
  - `docs/design/gameplay/combat/COMBAT_CORE_DIRECTION.md`
  - `docs/design/gameplay/combat/STATUS_EFFECTS_DIRECTION.md`
  - `docs/design/gameplay/cave/CAVE_DESIGN_DIRECTION.md`
  - `docs/design/gameplay/cave/CAVE_COMBAT_BALANCE_VULNERABILITIES_DIRECTION.md`
  - `docs/design/gameplay/loot_crafting_economy/LOOT_CRAFTING_ECONOMY_DIRECTION.md`
  - `docs/design/gameplay/loot_crafting_economy/ECONOMY_PRICING_STOCK_REFRESH_DIRECTION.md`
  - `docs/design/gameplay/player/PLAYER_CORE_SYSTEMS_DIRECTION.md`
  - `docs/design/gameplay/player/PLAYER_DERIVED_ATTRIBUTES_DIRECTION.md`
  - `docs/design/gameplay/save_load/SAVE_LOAD_FULL_STATE_DIRECTION.md`
  - `docs/design/gameplay/ui_ux/UI_UX_FULL_GAMEPLAY_DIRECTION.md`
  - `docs/design/gameplay/ui_ux/UI_UX_MENU_SCREEN_FLOWS_DIRECTION.md`
  - `docs/design/gameplay/quests/QUEST_OBJECTIVE_EVENT_SYSTEM_DIRECTION.md`
  - `docs/design/gameplay/quests/QUESTS_MAIN_PROGRESSION_REFINEMENT_DIRECTION.md`
  - `docs/design/gameplay/world/SEASONS_CALENDAR_WEATHER_LUNAR_DIRECTION.md`
  - `docs/design/gameplay/bestiary/BESTIARY_KNOWLEDGE_DISCOVERY_DIRECTION.md`
  - `docs/design/gameplay/social/SOCIAL_RELATIONSHIP_ROMANCE_DIRECTION.md`
> **Blocks:**  
  - pet core;
  - pet HUD;
  - pet cave alerts;
  - farm layout/object placement future.
> **Scope:** definir contratos futuros de área do pet, alimentação, itens e funções leves na fazenda, mantendo HOLD até aprovação explícita.  
> **Out of scope:** farm automation, crop work, pet cave support, scene/prefab placement, item DB final, economy pricing final.

---

# /speckit.specify

## 1. Contexto

Quando pets voltarem ao escopo, a fazenda deve suportar PetBed, PetBowl, PetToy e PetAreaAnchor. Pets podem alertar visitante, indicar item perdido, indicar forage próximo, reagir à Fonte, achar itens simples e participar de eventos sociais/festivais futuros. Eles não plantam, colhem, regam, mineram, cortam árvore, processam item, vendem, compram ou substituem companion job.

---

## 2. Problema

Sem contratos de farm/home:

```text
pet planta ou colhe sozinho;
pet gera item valioso todo dia;
PetFood vira arbitragem;
PetBed vira multiplicador econômico;
PetAreaAnchor hardcoda posição;
pet acha Água Viva/Mana/Pedra Negra;
feeding vira microgerenciamento pesado;
item de quest do pet pode ser vendido sem proteção.
```

---

## 3. Objetivo

Criar, no futuro aprovado:

```text
PetHomeAreaState;
PetAreaAnchor;
PetCareObjectType;
PetFeedingRequest;
PetFoodPolicy;
PetFarmHintType;
PetFarmFindPolicy;
PetFarmInteractionResult;
PetItemProtectionPolicy.
```

---

## 4. Regras de design

```text
PetBed define descanso.
PetBowl define alimentação.
PetToy gera interação simples.
PetAreaAnchor limita circulação sem hardcode.
PetFood tem BaseValue e não cria arbitragem.
Pet achado tem cooldown.
Achado valioso exige condição/evento/progresso.
Pet não produz Água Viva, Mana, purificação ou dinheiro relevante.
```

---

## 5. User stories / engineering stories

```text
Como jogador, quero cuidar do pet com cama/tigela/brinquedo sem microgerenciamento pesado.
Como farm, quero pet circulando e reagindo sem trabalhar no lugar do jogador.
Como economy, quero pet food/toys/bed sem arbitragem.
Como lore, quero reação a Fonte/Mana sem entregar solução.
Como save/load, quero objetos/anchors por IDs.
```

---

## 6. Escopo

Inclui:

```text
home area contracts;
care object types;
feeding request/policy;
farm hint types;
find policy;
item protection;
tests.
```

Não inclui:

```text
visual placement;
farm work automation;
crop interaction;
animal production;
pet cave behavior;
scene/prefab assets.
```

## 7. Modelo de domínio

### 7.1 PetCareObjectType

```text
PetBed
PetBowl
PetToy
PetAreaAnchor
PetCosmetic
PetFood
PetTreat
PetQuestItem
```

### 7.2 PetHomeAreaState

```text
PetId
HomeAnchorId
BedInstanceId optional
BowlInstanceId optional
ToyInstanceIds[]
AllowedFarmZoneIds[]
LastRestDay
LastFedDay
LastPlayedDay
ComfortTier
```

### 7.3 PetFeedingRequest

```text
PetId
ItemInstanceId
ItemId
FoodTags[]
IsFavorite
Day
PetMoodBefore
PetEnergyBefore
```

### 7.4 PetFoodPolicy

```text
AllowedFoodTags[]
FavoriteFoodIds[]
ForbiddenFoodTags[]
RestoresMood
RestoresEnergy
BondDelta
TemporaryLightBonus optional
NoStrongCombatBuff true
NoRareLoreAsCommonFood true
```

### 7.5 PetFarmHintType

```text
VisitorAlert
LostItemHint
ForageNearbyHint
FonteReaction
AnimalPastureFuture
SmallFind
SocialFestivalFuture
ManaRootReactionNoSolution
```

### 7.6 PetFarmFindPolicy

```text
FindType
AllowedItemTags[]
CooldownDays
RequiresBondLevel
RequiresMood
RequiresSeason optional
RequiresEvent optional
MaxValueTier
AutoAddToInventory false
RequiresPlayerPickup true
```

---

## 8. Farm function rules

```text
Allowed:
  visitor alert;
  lost item hint;
  forage hint;
  simple small find;
  Fonte reaction;
  Mana root reaction without solution.

Forbidden:
  planting;
  harvesting;
  watering;
  mining;
  chopping;
  processing;
  selling;
  buying;
  shipping;
  animal production collection;
  full companion job replacement.
```

---

## 9. Economy rules

```text
PetFood sellable only with BaseValue.
PetFood no arbitrage.
PetTreat limited restock/condition.
PetBed/Bowl/Toy comfort/cosmetic/bond, not strong economy multiplier.
PetQuestItem protected from sell/drop.
Pet find max value bounded and cooldown-gated.
```

---

## 10. Criteria

```text
Pet home/feed/farm contracts exist.
Care objects modeled.
Farm functions allowed/prohibited.
Pet find policy bounded.
Pet food policy anti-arbitrage.
Tests cover feeding, favorite food, forbidden food, low energy recovery, find cooldown, high value blocked, Mana/Fonte no solution, no auto inventory and forbidden farm work blocked.
```

# /speckit.plan

## 11. Arquitetura alvo

```text
Assets/_Game/Scripts/Pets/Farm/PetHomeAreaState.cs
Assets/_Game/Scripts/Pets/Farm/PetAreaAnchor.cs
Assets/_Game/Scripts/Pets/Items/PetCareObjectType.cs
Assets/_Game/Scripts/Pets/Items/PetFeedingRequest.cs
Assets/_Game/Scripts/Pets/Items/PetFoodPolicy.cs
Assets/_Game/Scripts/Pets/Farm/PetFarmHintType.cs
Assets/_Game/Scripts/Pets/Farm/PetFarmFindPolicy.cs
Assets/_Game/Scripts/Pets/Farm/PetFarmInteractionResult.cs
Assets/_Game/Scripts/Pets/Items/PetItemProtectionPolicy.cs
Assets/_Game/Tests/EditMode/Pets/PetHomeAreaFeedingItemsFarmHintsTests.cs
```

## 12. Arquivos permitidos

```text
Assets/_Game/Scripts/Pets/**
Assets/_Game/Scripts/Farm/**
Assets/_Game/Scripts/Items/**
Assets/_Game/Scripts/Economy/**
Assets/_Game/Scripts/Editor/Validation/**
Assets/_Game/Tests/EditMode/Pets/**
docs/validation/23_spec_pet_home_area_feeding_items_farm_hints_future_runtime_execution_report.md
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

- [ ] T001 — Confirmar aprovação explícita de escopo de pets; se ausente, parar BLOCKED_SCOPE.
- [ ] T002 — Ler fontes.
- [ ] T003 — Auditar pets/farm/items/economy.
- [ ] T004 — Consolidar home/feed/farm contracts.
- [ ] T005 — Implementar validators.
- [ ] T006 — Criar tests.
- [ ] T007 — Rodar validações.
- [ ] T008 — Criar report.

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

- docs/design/gameplay/pets/PETS_DIRECTION.md
- docs/design/gameplay/farm/FARM_DESIGN_DIRECTION_v1.3.md
- docs/design/gameplay/farm/FARM_LAYOUT_SCALE_BUILDINGS_DIRECTION.md
- docs/design/gameplay/companions/COMPANIONS_DIRECTION.md
- docs/design/gameplay/combat/COMBAT_CORE_DIRECTION.md
- docs/design/gameplay/combat/STATUS_EFFECTS_DIRECTION.md
- docs/design/gameplay/cave/CAVE_DESIGN_DIRECTION.md
- docs/design/gameplay/cave/CAVE_COMBAT_BALANCE_VULNERABILITIES_DIRECTION.md
- docs/design/gameplay/loot_crafting_economy/LOOT_CRAFTING_ECONOMY_DIRECTION.md
- docs/design/gameplay/loot_crafting_economy/ECONOMY_PRICING_STOCK_REFRESH_DIRECTION.md
- docs/design/gameplay/player/PLAYER_CORE_SYSTEMS_DIRECTION.md
- docs/design/gameplay/player/PLAYER_DERIVED_ATTRIBUTES_DIRECTION.md
- docs/design/gameplay/save_load/SAVE_LOAD_FULL_STATE_DIRECTION.md
- docs/design/gameplay/ui_ux/UI_UX_FULL_GAMEPLAY_DIRECTION.md
- docs/design/gameplay/ui_ux/UI_UX_MENU_SCREEN_FLOWS_DIRECTION.md
- docs/design/gameplay/quests/QUEST_OBJECTIVE_EVENT_SYSTEM_DIRECTION.md
- docs/design/gameplay/quests/QUESTS_MAIN_PROGRESSION_REFINEMENT_DIRECTION.md
- docs/design/gameplay/world/SEASONS_CALENDAR_WEATHER_LUNAR_DIRECTION.md
- docs/design/gameplay/bestiary/BESTIARY_KNOWLEDGE_DISCOVERY_DIRECTION.md
- docs/design/gameplay/social/SOCIAL_RELATIONSHIP_ROMANCE_DIRECTION.md

### Required interpretation

```text
Esta spec é future/mapped e fica em HOLD.
PETS_DIRECTION declara que pets estão deferidos e que não se deve criar specs/runtime/UI/save/data assets de pets até decisão explícita.
Este arquivo existe como preparação de futuro mapeado; não deve ser executado até aprovação humana explícita para retomar o escopo de pets.
Pet é sistema próprio.
Pet não é companion.
Pet não ocupa slot de companion.
Pet não substitui build do jogador.
Pet não joga pelo jogador.
Pet não pode ser requisito de progressão.
Pet não pode ter Breath/Fôlego.
Pet não pode gerar economia paralela, loot infinito, cura, tanking, revive, boss control ou puzzle solve automático.
Quando houver conflito, PETS_DIRECTION vence para limites de pet; COMPANIONS_DIRECTION vence para companion; CAVE/COMBAT vencem para active combat budget e bosses.
```

---

## Direction / Refinement Coverage

### Covered from directions

- PetBed, PetBowl, PetToy e PetAreaAnchor são objetos base futuros.
- PetBed define descanso/sono, PetBowl alimentação, PetToy interação diária e PetAreaAnchor circulação.
- Pets podem ajudar na fazenda de forma leve: visitante, item perdido, forage, Fonte, achado simples e eventos sociais.
- Pets não podem plantar, colher, regar, minerar, cortar, processar, vender, comprar, abrir shipping, coletar produção inteira ou substituir companion job.
- Achado de pet deve ter cooldown e achado valioso exige condição/evento/progresso.
- Pet não produz Água Viva, não cultiva Mana e não purifica Pedra Negra sozinho.

### Deferred / future from directions

- Scene/prefab placement.
- Item DB final.
- Visual pet movement.
- Farm automation.
- Pet cave behavior.
- Save migration.

### Explicitly not redefined here

- Farm automation.
- Crop core.
- Economy pricing final.
- ItemInstance backend.
- FonteAnya state.
- Mana root.

---

## 23A. Execution Readiness Matrix

| Área | Pergunta obrigatória | Evidência esperada | Status se faltar |
|---|---|---|---|
| Escopo explícito | Existe aprovação humana explícita para retomar pets? | Link/commit/registro no execution report. | BLOCKED |
| Fonte canônica | Executor leu PETS_DIRECTION, farm, companions, cave/combat, economy, player, save e UI directions? | Lista no report. | PARTIAL |
| Estado real do repo | Sistemas existentes relacionados a pet home area/feeding/items/farm hints foram auditados antes de criar novos? | Comandos `rg` e achados. | PARTIAL |
| Não duplicação | Existe sistema equivalente já implementado/parcial? | REUSE/HARDEN/CREATE/DEFER. | BLOCKED se duplicar |
| Separação de companion | Pet não é companion, não ocupa companion slot e não substitui companion. | Tests/checklist. | BLOCKED se violar |
| Leveza sistêmica | Pet dá apoio leve, legibilidade e vínculo; não joga pelo jogador. | Tests/checklist. | BLOCKED se violar |
| Anti-combate | Pet não tanka boss, não segura aggro, não cura/revive, não causa dano relevante, não resolve mecânica central. | Tests/checklist. | BLOCKED se violar |
| Anti-economia | Pet não gera dinheiro relevante recorrente, loot extra padrão, auto-sell, produção ou arbitragem. | Tests/checklist. | PARTIAL |
| Anti-progressão | Pet não é obrigatório para main quest, cave, farm, boss, Fonte, Mana, final ou bestiary. | Tests/checklist. | BLOCKED se violar |
| Save/load | Houve alteração de schema? | Declaração NO ou STOP se migration necessária. | BLOCKED se alterar sem migration |
| UI/PlayMode | Há fluxo visual/gameplay integrado? | Cenário final deferido documentado. | BUILD_VALIDATED no máximo se ausente |
| Testes | Lógica determinística nova tem EditMode tests quando praticável? | Test list. | PARTIAL |
| Report | Execution report criado? | `docs/validation/23_spec_pet_home_area_feeding_items_farm_hints_future_runtime_execution_report.md`. | PARTIAL |

---

## 23B. Local Audit Checklist

Antes de alterar qualquer arquivo, Claude Code/Codex deve rodar e registrar no execution report:

```bash
rg -n "PetBed|PetBowl|PetToy|PetFood|PetTreat|PetFarmHint|PetFind|PetArea|FonteReaction|ManaRootReaction" Assets/_Game/Scripts docs/design .specs
rg -n "Pet|PetData|PetBond|PetMood|PetEnergy|PetRoutine|PetFood|PetBed|PetBowl|PetToy|PetArea|PetHUD|PetAlert|TreasureHint|TrapHint|LightInterrupt|CaveActive|FollowingPlayer|Breath|Folego|Companion|FarmAnimal|Mount" Assets/_Game/Scripts docs/design .specs
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
Given há aprovação explícita para pets e sistemas fundacionais prontos
When o fluxo de pet home area/feeding/items/farm hints roda
Then ele aplica apenas apoio leve, vínculo, rotina ou feedback autorizado
And respeita energia/cooldown/humor
And não substitui companion, jogador, economia, combate ou progressão.
```

### Scenario 2 — Existing partial implementation

```text
Given já existe implementação parcial no repo
When a execução audita o sistema
Then ela escolhe HARDEN_EXISTING em vez de recriar do zero
And registra divergências do direction
And altera apenas o menor conjunto seguro de arquivos.
```

### Scenario 3 — Blocked because pets are still deferred

```text
Given não há aprovação humana explícita para retomar pets
When executor tenta implementar esta spec
Then deve parar com status BLOCKED
And não criar Assets/_Game/Scripts/Pets, UI, save, data assets ou prefabs.
```

### Scenario 4 — Anti-exploit

```text
Given pet tenta alertar, achar item, entrar na caverna, interagir com Fonte/Mana ou ajudar em farm
When energia/cooldown/contexto não permitem
Then ação é skipped/blocked
And nenhum loot/dinheiro/progresso/quest/final é gerado.
```

### Scenario 5 — Final human validation deferred

```text
Given o fluxo exige inspeção visual/gameplay de pet seguindo, HUD, alerta, rotina, cama/tigela, farm/cave behavior ou save/load
When a implementação técnica terminar
Then o report registra cenário final em docs/validation/FINAL_HUMAN_VALIDATION_BY_WAVE.md
And não pede validação humana imediata por spec.
```

---

## 23D. Edge Cases and Failure Modes

A execução deve cobrir ou registrar risco residual para:

- No explicit pet-scope approval.
- Pet performs farm work.
- Pet finds valuable items repeatedly.
- PetFood arbitrage.
- Pet item gives combat power.
- Pet produces Water/Mana/purification.
- Pet find auto-adds silently.
- PetQuestItem sellable accidentally.

---

## 23E. Minimum Execution Report Template

```md
# Execution Report — Pet Home Area Feeding Items Farm Hints Future Runtime

## Summary
- Spec:
- Branch:
- Executor:
- Date:
- Final status:

## Scope approval
- Explicit pet-scope approval:
- Approval source:
- If missing, final status must be BLOCKED:

## Sources read
- ...

## Local audit
- Commands executed:
- Existing systems found:
- Existing partial systems found:
- Missing systems:
- Conflicts:

## Pet compliance
- Pet is not companion:
- Pet does not occupy companion slot:
- Pet does not replace player build:
- Pet does not play for player:
- No Breath/Folego:
- No boss tank/aggro/heal/revive:
- No economy/loot/progression exploit:
- Energy/cooldown used:
- Save/load safe:
- UI not source of truth:

## Implementation decision
- REUSE_EXISTING / HARDEN_EXISTING / CREATE_MINIMAL / DEFER / BLOCKED_SCOPE
- Justification:

## Files changed
- ...

## Functional evidence
- Happy path:
- Existing partial:
- Deferred-scope block:
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
1. Não existir aprovação humana explícita para retomar escopo de pets.
2. A implementação exigir alterar Packages/ ou ProjectSettings/.
3. A implementação exigir scene/prefab/tilemap/asset/font/icon/audio changes.
4. A implementação alterar save schema sem migration spec.
5. A implementação tratar pet como companion, farm animal ou mount.
6. A implementação dar Breath/Fôlego ao pet.
7. A implementação permitir pet tankar boss, segurar aggro, curar/reviver jogador ou causar dano relevante.
8. A implementação permitir pet resolver puzzle/quest, abrir caminho obrigatório ou completar bestiary sozinho.
9. A implementação gerar loot/dinheiro recorrente relevante, auto-sell, produção ou arbitragem.
10. A implementação tornar pet obrigatório para terminar jogo, vencer boss, farmar, entrar na caverna, acessar final ou progredir.
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
rg -n "Pet|PetBond|PetMood|PetEnergy|PetRoutine|PetFood|PetBed|PetBowl|PetHUD|PetAlert|TreasureHint|TrapHint|LightInterrupt|Breath|Folego" Assets/_Game/Scripts docs/design .specs
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
Quando houver cenário visual/gameplay de pets, HUD, rotina, cama/tigela, farm/cave hint ou save/load, registrar em execution report e vincular a docs/validation/FINAL_HUMAN_VALIDATION_BY_WAVE.md.
```

---

## 26. Testing Quality Gate

- Changed deterministic logic: YES if executed; feeding/find/economy/protection validation is deterministic.
- Requires EditMode tests: YES if executed for feeding/favorite/forbidden/cooldown/value/Mana/Fonte/no-auto/blocked-work tests.
- Requires PlayMode automated or final human scenario: YES, DEFERRED for pet farm visual validation.
- Requires regression test: YES if fixing existing pet farm/economy bug; otherwise NO.
- Human validation timing: DEFERRED_TO_FINAL_VALIDATION se houver UI/gameplay integrado; caso contrário NOT REQUIRED.
- Minimum validation evidence for ACCEPTED: If no explicit pet-scope approval: BLOCKED_SCOPE with no file changes. If approved: docs validation PASS; C# build PASS if C# changed; EditMode tests PASS or NOT RUN justified.

---

## 27. Definition of Done

```text
Spec executada sem alterar arquivos proibidos.
Se escopo de pets ainda estiver deferido, status correto é BLOCKED_SCOPE sem alterar código.
Contratos/data/runtime implementados apenas dentro do escopo.
Execution report criado em docs/validation/23_spec_pet_home_area_feeding_items_farm_hints_future_runtime_execution_report.md.
Fonte/direction coverage preservado.
Validações obrigatórias PASS ou NOT RUN com motivo, impacto e mitigação.
Sem promoção indevida para ACCEPTED apenas por compile.
```

---

## 28. Anti-regressão

```text
Não executar sem aprovação explícita de escopo de pets.
Não tratar pet como companion.
Não dar Breath/Fôlego ao pet.
Não fazer pet jogar pelo jogador.
Não gerar economia/loot/progressão obrigatória.
Não tankar boss/curar/reviver/segurar aggro.
Não editar scenes/prefabs/assets.
Não pedir human test por spec.
Não executar runtime em massa antes da 01Q ou exceção humana explícita.
Não alterar SPEC_EXECUTION_ORDER.md.
```

---

## 29. Notas para execução posterior

Esta spec é future/mapped e em HOLD. Ela só deve ser executada quando houver decisão explícita de retomar pets, com atualização do roadmap/registries e validação contra estado real do repo.
