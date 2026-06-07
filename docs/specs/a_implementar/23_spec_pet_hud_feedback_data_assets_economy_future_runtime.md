# SPEC — Pet HUD Feedback Data Assets Economy Future Runtime

> **Spec ID:** `23_spec_pet_hud_feedback_data_assets_economy_future_runtime`  
> **Status:** A implementar / Future mapped / HOLD until explicit pet-scope approval  
> **Wave:** WAVE 23 — Pets Advanced Future  
> **Priority:** P2  
> **Type:** Runtime / Future / HOLD / Pet UI Data  
> **Domain:** Pet HUD / Feedback / Data Contracts / Economy Compatibility / Assets Profiles  
> **Parallelizable:** YES  
> **Parallel group:** WAVE_23_PETS_ADVANCED_FUTURE  
> **Can run with:** pet core/farm/cave specs if already approved and no same files.  
> **Must not run with:** qualquer spec que crie Unity assets, sprites, audio, prefabs, localization files, save migration or final economy prices.  
> **Repo lock scope:** `Assets/_Game/Scripts/Pets/UI/**`, `Assets/_Game/Scripts/Pets/Data/**`, `Assets/_Game/Scripts/Pets/Economy/**`, `Assets/_Game/Tests/EditMode/Pets/**`, `docs/validation/23_spec_pet_hud_feedback_data_assets_economy_future_runtime_execution_report.md`  
> **Depends on:**  
  - `docs/design/SPEC_SOURCE_MAP.md`
  - `docs/design/SPECIFICATION_PROCESS.md`
  - `docs/design/lore/VAALARA_GAME_CANON_DIRECTION_v1.0.md`
  - `docs/specs/SPEC_IMPLEMENTABLE_TEMPLATE.md`
  - `docs/specs/SPEC_WAVE_EXECUTION_PROTOCOL.md`
  - `docs/specs/SPEC_VALIDATION_MATRIX_MASTER.md`
  - `docs/specs/SPEC_EXISTING_IMPLEMENTATION_AUDIT.md`
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
  - pet home/farm;
  - pet cave;
  - final UI visual pass.
> **Scope:** definir contratos futuros de HUD leve, feedback, profiles/data contracts e compatibilidade econômica de itens de pet, mantendo HOLD até aprovação explícita.  
> **Out of scope:** Unity ScriptableObject assets, sprites/icons/audio/prefabs, localization files, final pricing, save migration.

---

# /speckit.specify

## 1. Contexto

HUD de pet é futuro e deve ser leve: ícone do pet ativo, humor, energia, fome/alimentação simples, alerta contextual, cooldown de ação especial e estado. Feedback deve ser compreensível sem texto longo. Data assets esperados incluem PetDataSO, bond/mood/food/routine/farm/cave/alert/HUD/cosmetic/area/save profiles. Itens de pet seguem economia geral.

---

## 2. Problema

Sem contratos de HUD/data/economia:

```text
HUD de pet compete com HP/MP/Stamina;
pet ganha barras complexas demais;
feedback depende só de texto longo;
profiles viram Unity assets antes da hora;
PetFood não tem BaseValue;
PetQuestItem pode ser vendido;
cosmético altera combate;
HUD mostra Breath/Fôlego;
save serializa ScriptableObject.
```

---

## 3. Objetivo

Criar, no futuro aprovado:

```text
PetHudViewModel;
PetFeedbackEvent;
PetIconState;
PetCooldownProjection;
PetDataProfileContract;
PetFoodEconomyPolicy;
PetItemCategory;
PetCosmeticPowerPolicy;
PetUiAccessibilityPolicy.
```

---

## 4. Regras de design

```text
HUD leve.
Não competir com HP/MP/Stamina.
Sem Breath/Fôlego.
Ícones e feedback curtos.
Data contracts não criam assets automaticamente.
Itens vendáveis precisam BaseValue.
PetQuestItem protegido.
Cosmético não altera poder relevante.
```

---

## 5. User stories / engineering stories

```text
Como jogador, quero entender humor/energia/alerta do pet sem poluir HUD.
Como UI, quero ícones e feedback legíveis.
Como data, quero contracts para profiles futuros sem assets ainda.
Como economy, quero PetFood/Treat/Toy/Bed/Bowl protegidos de arbitragem.
Como accessibility, quero feedback visual/sonoro sem depender só de cor.
```

---

## 6. Escopo

Inclui:

```text
HUD view model;
feedback event;
icon state;
cooldown projection;
data profile contracts;
pet item categories;
economy protection;
accessibility;
tests.
```

Não inclui:

```text
prefabs;
sprites;
audio;
ScriptableObject assets;
localization files;
final pricing;
save migration.
```

## 7. Modelo de domínio

### 7.1 PetHudViewModel

```text
ActivePetId
IconState
MoodState
EnergyState
SimpleHungerState
PresenceState
CurrentAlert
SpecialActionCooldown
CanShowDetailedPanel
DebugWarnings[]
```

### 7.2 PetIconState

```text
PetHappy
PetHungry
PetTired
PetAlert
PetTreasureHint
PetTrapHint
PetFear
PetResting
PetFollowing
PetUnavailable
```

### 7.3 PetFeedbackEvent

```text
EventId
PetId
FeedbackType: Bubble | IconAbovePet | BarkMeow | AttentionAnimation | LookDirection | ReturnToPlayer | RetreatFromDanger
TextKey optional
IconKey optional
AudioKey optional
Duration
Priority
RequiresAccessibilityFallback
```

### 7.4 PetCooldownProjection

```text
ActionType
CooldownScope
RemainingTime
RemainingRooms optional
RemainingDayLock optional
VisibleToPlayer
```

### 7.5 PetDataProfileContract

```text
PetId
Species
VisualVariantIds[]
DefaultBondProfileId
DefaultRoutineProfileId
AllowedFoodTags[]
FavoriteFoodIds[]
CanEnterCave
CanUseFarmHints
CanUseCaveHints
CanUseLightInterrupt
BaseEnergy
BaseMood
IconId
SpriteSetId
```

### 7.6 PetItemCategory

```text
PetFood
PetTreat
PetToy
PetBed
PetBowl
PetCosmetic
PetQuestItem
```

### 7.7 PetFoodEconomyPolicy

```text
ItemCategory
RequiresBaseValue
Stackable
Sellable
ProtectedFromAccidentalSell
RestockPolicy
NoArbitrage
```

---

## 8. HUD rules

```text
Pet HUD uses icon/state first.
No complex bar if icon/state is enough.
No Breath/Fôlego.
No HP-like pet bar by default.
Alert contextual only when useful.
Cooldown visible only for useful action with actual cooldown.
```

---

## 9. Feedback rules

```text
Feedback short and readable.
Visual/audio fallback for important alert.
No long text required.
Trap/treasure alert does not reveal exact solution.
Boss fear reaction does not imply mechanic answer.
```

---

## 10. Data/economy rules

```text
Contracts may exist before SO assets.
Do not create .asset files in this spec.
PetFood needs BaseValue and no arbitrage.
PetQuestItem protected.
PetCosmetic not combat power.
PetBed/Bowl/Toy comfort/bond/routine only.
```

---

## 11. Criteria

```text
HUD/feedback/data/economy contracts exist.
HUD excludes Breath/Fôlego.
HUD does not compete with HP/MP/Stamina.
Feedback accessible and short.
Pet data contract uses IDs.
Economy policies protect PetFood/PetQuestItem/cosmetics.
Tests cover HUD states, no Breath, alert feedback, accessibility fallback, data profile validation, PetFood BaseValue, PetQuestItem protected and cosmetic no combat power.
```

# /speckit.plan

## 12. Arquitetura alvo

```text
Assets/_Game/Scripts/Pets/UI/PetHudViewModel.cs
Assets/_Game/Scripts/Pets/UI/PetIconState.cs
Assets/_Game/Scripts/Pets/UI/PetFeedbackEvent.cs
Assets/_Game/Scripts/Pets/UI/PetCooldownProjection.cs
Assets/_Game/Scripts/Pets/Data/PetDataProfileContract.cs
Assets/_Game/Scripts/Pets/Economy/PetItemCategory.cs
Assets/_Game/Scripts/Pets/Economy/PetFoodEconomyPolicy.cs
Assets/_Game/Scripts/Pets/Economy/PetCosmeticPowerPolicy.cs
Assets/_Game/Scripts/Pets/UI/PetUiAccessibilityPolicy.cs
Assets/_Game/Tests/EditMode/Pets/PetHudFeedbackDataAssetsEconomyTests.cs
```

## 13. Arquivos permitidos

```text
Assets/_Game/Scripts/Pets/**
Assets/_Game/Scripts/UI/Pets/**
Assets/_Game/Scripts/Economy/**
Assets/_Game/Scripts/Editor/Validation/**
Assets/_Game/Tests/EditMode/Pets/**
docs/validation/23_spec_pet_hud_feedback_data_assets_economy_future_runtime_execution_report.md
```

## 14. Arquivos proibidos

```text
Packages/**
ProjectSettings/**
Assets/**/*.unity
Assets/**/*.prefab
Assets/**/*.asset
Assets/**/*.png
Assets/**/*.wav
Assets/**/*.mp3
Assets/Localization/**
docs/specs/SPEC_EXECUTION_ORDER.md
docs/specs/implementados/**
docs/refinements/implementados/**
docs/project/CURRENT_STATE.md
PROJECT_LOG.md
```

# /speckit.tasks

## 15. Tasks

- [ ] T001 — Confirmar aprovação explícita de escopo de pets; se ausente, parar BLOCKED_SCOPE.
- [ ] T002 — Ler fontes.
- [ ] T003 — Auditar UI/pet/economy/data patterns.
- [ ] T004 — Consolidar HUD/feedback/data/economy contracts.
- [ ] T005 — Implementar validators.
- [ ] T006 — Criar tests.
- [ ] T007 — Rodar validações.
- [ ] T008 — Criar report.

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

- HUD de pet é futuro e deve ser leve.
- Informações possíveis incluem ícone, humor, energia, fome/alimentação simples, alerta contextual, cooldown e estado.
- HUD de pet não deve competir com HP/MP/Stamina; não criar barra complexa se ícone/estado resolver.
- Ícones sugeridos incluem PetHappy, PetHungry, PetTired, PetAlert, PetTreasureHint, PetTrapHint, PetFear, PetResting e PetFollowing.
- Data assets esperados incluem PetDataSO, PetBondProfileSO, PetMoodProfileSO, PetFoodDataSO, PetRoutineProfileSO, PetFarmBehaviorProfileSO, PetCaveBehaviorProfileSO, PetAlertProfileSO e PetHUDProfileSO.
- Itens de pet seguem economia geral: PetFood, PetTreat, PetToy, PetBed, PetBowl, PetCosmetic e PetQuestItem.

### Deferred / future from directions

- Unity .asset files.
- Sprites/icons/audio.
- HUD prefab.
- Localization files.
- Final pricing.
- Save migration.

### Explicitly not redefined here

- UI core HUD.
- Player HP/MP/Stamina.
- Economy formula.
- Item database.
- ScriptableObject assets.
- Pet runtime behavior.

---

## 23A. Execution Readiness Matrix

| Área | Pergunta obrigatória | Evidência esperada | Status se faltar |
|---|---|---|---|
| Escopo explícito | Existe aprovação humana explícita para retomar pets? | Link/commit/registro no execution report. | BLOCKED |
| Fonte canônica | Executor leu PETS_DIRECTION, farm, companions, cave/combat, economy, player, save e UI directions? | Lista no report. | PARTIAL |
| Estado real do repo | Sistemas existentes relacionados a pet HUD/feedback/data/economy contracts foram auditados antes de criar novos? | Comandos `rg` e achados. | PARTIAL |
| Não duplicação | Existe sistema equivalente já implementado/parcial? | REUSE/HARDEN/CREATE/DEFER. | BLOCKED se duplicar |
| Separação de companion | Pet não é companion, não ocupa companion slot e não substitui companion. | Tests/checklist. | BLOCKED se violar |
| Leveza sistêmica | Pet dá apoio leve, legibilidade e vínculo; não joga pelo jogador. | Tests/checklist. | BLOCKED se violar |
| Anti-combate | Pet não tanka boss, não segura aggro, não cura/revive, não causa dano relevante, não resolve mecânica central. | Tests/checklist. | BLOCKED se violar |
| Anti-economia | Pet não gera dinheiro relevante recorrente, loot extra padrão, auto-sell, produção ou arbitragem. | Tests/checklist. | PARTIAL |
| Anti-progressão | Pet não é obrigatório para main quest, cave, farm, boss, Fonte, Mana, final ou bestiary. | Tests/checklist. | BLOCKED se violar |
| Save/load | Houve alteração de schema? | Declaração NO ou STOP se migration necessária. | BLOCKED se alterar sem migration |
| UI/PlayMode | Há fluxo visual/gameplay integrado? | Cenário final deferido documentado. | BUILD_VALIDATED no máximo se ausente |
| Testes | Lógica determinística nova tem EditMode tests quando praticável? | Test list. | PARTIAL |
| Report | Execution report criado? | `docs/validation/23_spec_pet_hud_feedback_data_assets_economy_future_runtime_execution_report.md`. | PARTIAL |

---

## 23B. Local Audit Checklist

Antes de alterar qualquer arquivo, Claude Code/Codex deve rodar e registrar no execution report:

```bash
rg -n "PetHUD|PetIcon|PetFeedback|PetDataSO|PetFood|PetTreat|PetCosmetic|PetQuestItem|Breath|Folego" Assets/_Game/Scripts docs/design docs/specs
rg -n "Pet|PetData|PetBond|PetMood|PetEnergy|PetRoutine|PetFood|PetBed|PetBowl|PetToy|PetArea|PetHUD|PetAlert|TreasureHint|TrapHint|LightInterrupt|CaveActive|FollowingPlayer|Breath|Folego|Companion|FarmAnimal|Mount" Assets/_Game/Scripts docs/design docs/specs
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
Given há aprovação explícita para pets e sistemas fundacionais prontos
When o fluxo de pet HUD/feedback/data/economy contracts roda
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
- Pet HUD competes with player HUD.
- Breath/Folego appears.
- Feedback is long text only.
- Data contract creates .asset files.
- PetFood no BaseValue.
- PetQuestItem sellable.
- Cosmetic gives combat power.

---

## 23E. Minimum Execution Report Template

```md
# Execution Report — Pet HUD Feedback Data Assets Economy Future Runtime

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
rg -n "Pet|PetBond|PetMood|PetEnergy|PetRoutine|PetFood|PetBed|PetBowl|PetHUD|PetAlert|TreasureHint|TrapHint|LightInterrupt|Breath|Folego" Assets/_Game/Scripts docs/design docs/specs
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

- Changed deterministic logic: YES if executed; HUD/data/economy policy validation is deterministic.
- Requires EditMode tests: YES if executed for HUD/no-Breath/feedback/data/BaseValue/protection/cosmetic tests.
- Requires PlayMode automated or final human scenario: YES, DEFERRED for pet HUD visual validation.
- Requires regression test: YES if fixing existing pet HUD/economy bug; otherwise NO.
- Human validation timing: DEFERRED_TO_FINAL_VALIDATION se houver UI/gameplay integrado; caso contrário NOT REQUIRED.
- Minimum validation evidence for ACCEPTED: If no explicit pet-scope approval: BLOCKED_SCOPE with no file changes. If approved: docs validation PASS; C# build PASS if C# changed; EditMode tests PASS or NOT RUN justified.

---

## 27. Definition of Done

```text
Spec executada sem alterar arquivos proibidos.
Se escopo de pets ainda estiver deferido, status correto é BLOCKED_SCOPE sem alterar código.
Contratos/data/runtime implementados apenas dentro do escopo.
Execution report criado em docs/validation/23_spec_pet_hud_feedback_data_assets_economy_future_runtime_execution_report.md.
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
