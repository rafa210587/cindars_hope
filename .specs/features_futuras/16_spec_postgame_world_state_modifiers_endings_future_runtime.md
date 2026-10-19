# SPEC — Postgame World State Modifiers Endings Future Runtime

> **Spec ID:** `16_spec_postgame_world_state_modifiers_endings_future_runtime`  
> **Status:** A implementar / Future mapped  
> **Wave:** WAVE 16 — Cave Save Policy / Endgame Presentation / Postgame Future  
> **Priority:** P2  
> **Type:** Runtime / Future / Postgame / Ending Modifiers  
> **Domain:** Postgame / World Modifiers / Ending Effects / Farm City Cave Fonte Economy  
> **Parallelizable:** NO  
> **Parallel group:** WAVE_16_CAVE_SAVE_ENDGAME_FUTURE  
> **Can run with:** N/A  
> **Must not run with:** qualquer spec que altere final choice service core, economy formula final, cave procedural generation, farm crop formula, city schedules concrete data, save migration, pet/social runtime or scene assets.  
> **Repo lock scope:** `Assets/_Game/Scripts/MainProgression/PostGame/**`, `Assets/_Game/Scripts/Fonte/**`, `Assets/_Game/Scripts/World/**`, `Assets/_Game/Tests/EditMode/MainProgression/**`, `docs/validation/16_spec_postgame_world_state_modifiers_endings_future_runtime_execution_report.md`  
> **Depends on:**  
  - `docs/design/SPEC_SOURCE_MAP.md`
  - `docs/design/SPECIFICATION_PROCESS.md`
  - `docs/design/lore/VAALARA_GAME_CANON_DIRECTION_v1.0.md`
  - `.specs/SPEC_IMPLEMENTABLE_TEMPLATE.md`
  - `.specs/SPEC_WAVE_EXECUTION_PROTOCOL.md`
  - `.specs/SPEC_VALIDATION_MATRIX_MASTER.md`
  - `.specs/SPEC_EXISTING_IMPLEMENTATION_AUDIT.md`
  - `docs/project/CURRENT_STATE.md`
  - `docs/design/gameplay/save_load/SAVE_LOAD_FULL_STATE_DIRECTION.md`
  - `docs/design/gameplay/cave/CAVE_DESIGN_DIRECTION.md`
  - `docs/design/gameplay/cave/CAVE_MONSTER_ROSTER_DIRECTION.md`
  - `docs/design/gameplay/cave/CAVE_COMBAT_BALANCE_VULNERABILITIES_DIRECTION.md`
  - `docs/design/gameplay/quests/QUEST_OBJECTIVE_EVENT_SYSTEM_DIRECTION.md`
  - `docs/design/gameplay/quests/QUESTS_MAIN_LORE_DIRECTION.md`
  - `docs/design/gameplay/quests/QUESTS_MAIN_PROGRESSION_REFINEMENT_DIRECTION.md`
  - `docs/design/gameplay/ui_ux/UI_UX_FULL_GAMEPLAY_DIRECTION.md`
  - `docs/design/gameplay/ui_ux/UI_UX_MENU_SCREEN_FLOWS_DIRECTION.md`
  - `docs/design/gameplay/world/SEASONS_CALENDAR_WEATHER_LUNAR_DIRECTION.md`
  - `docs/design/gameplay/farm/FARM_DESIGN_DIRECTION_v1.3.md`
  - `docs/design/gameplay/farm/FARM_LAYOUT_SCALE_BUILDINGS_DIRECTION.md`
  - `docs/design/gameplay/city/CITY_DESIGN_DIRECTION_v1.2.md`
  - `docs/design/gameplay/city/CITY_LAYOUT_BUILDINGS_SCHEDULE_DIRECTION.md`
  - `docs/design/gameplay/city/CITY_NPC_ROSTER_SERVICES_DIRECTION_v1.1.md`
  - `docs/design/gameplay/loot_crafting_economy/LOOT_CRAFTING_ECONOMY_DIRECTION.md`
  - `docs/design/gameplay/loot_crafting_economy/ECONOMY_PRICING_STOCK_REFRESH_DIRECTION.md`
  - `docs/design/gameplay/magic/MAGIC_SPELLS_ACTIONS_DIRECTION.md`
  - `docs/design/gameplay/magic/MAGIC_LEARNING_UNLOCKS_SOURCES_DIRECTION.md`
  - `docs/design/gameplay/bestiary/BESTIARY_KNOWLEDGE_DISCOVERY_DIRECTION.md`
  - `docs/design/gameplay/companions/COMPANIONS_DIRECTION.md`
  - `docs/design/gameplay/pets/PETS_DIRECTION.md`
> **Blocks:**  
  - ending cinematic presentation;
  - postgame UI/calendar/fonte indicators;
  - economy/cave/farm final balancing;
  - final human validation.
> **Scope:** definir/endurecer modificadores futuros de postgame para finais Proteger/Selar/Usar, sem aplicar visual/cena ou rebalanço final.  
> **Out of scope:** final choice service core, save migration, scene/prefab, final balance numbers, content writing.

---

# /speckit.specify

## 1. Contexto

Os finais têm efeitos conceituais: Proteger deixa Fonte viva/estável, Mana rara/natural, cidade preserva memória, pós-game espiritual/natural e menos tecnologia bromeciana. Selar contém corrupção, limita Fonte, estabiliza caverna, reduz recursos avançados e tem tom melancólico. Usar amplia tecnologia bromeciana, torna Mana mais previsível, abre novas áreas com risco e progride materialmente com risco de repetir Bromécia.

Esta spec transforma isso em modifiers futuros controlados.

---

## 2. Problema

Sem postgame modifier contract:

```text
final é só texto sem efeito sistêmico;
Proteger/Selar/Usar aplicam efeitos contraditórios;
Mana vira dinheiro infinito em Usar;
Selar bloqueia conteúdo demais sem clareza;
Proteger dá bônus sem limite;
cave postgame vira farm infinito;
ending modifier reaplica a cada load;
city/farm/Fonte/economy divergem.
```

---

## 3. Objetivo

Criar/endurecer:

```text
PostGameWorldState;
EndingModifierProfile;
FontePostGameModifier;
ManaPostGamePolicy;
CavePostGamePolicy;
CityMemoryPostGamePolicy;
BromecianTechPostGamePolicy;
EconomyPostGamePolicy;
PostGameModifierApplicationResult.
```

---

## 4. Regras de design

```text
Final choice aplica uma vez.
Postgame modifiers são qualitativos e bounded.
Mana nunca vira crop comum/dinheiro infinito.
Nível 101 não vira farm normal.
Anya não volta como NPC comum.
Finais são moralmente diferentes, não simplesmente bom/neutro/mau.
```

---

## 5. User stories / engineering stories

```text
Como jogador, quero sentir diferença entre finais.
Como postgame, quero alterar Fonte/Mana/cave/cidade/economia de forma segura.
Como save/load, quero aplicar modificadores uma vez.
Como economy, quero impedir exploit de final.
Como UI, quero projetar estado pós-final sem spoiler indevido.
```

---

## 6. Escopo

Inclui:

```text
postgame state contracts;
ending modifier profiles;
bounded policies for Fonte/Mana/Cave/City/Economy/Bromecian tech;
idempotent application;
validators/tests.
```

Não inclui:

```text
visual scene changes;
final balance numbers;
new postgame quests;
final content writing;
save migration.
```

## 7. Modelo de domínio

### 7.1 PostGameWorldState

```text
None
ProtectPostGame
SealPostGame
UsePostGame
DebugPostGame
```

### 7.2 EndingModifierProfile

```text
ProfileId
ChoiceType
FonteModifier
ManaPolicy
CavePolicy
CityMemoryPolicy
BromecianTechPolicy
EconomyPolicy
QuestAvailabilityPolicy
UiProjectionPolicy
AppliedOnceKey
```

### 7.3 FontePostGameModifier

```text
FonteFinalState
LivingWaterStability
RespecAvailability
PurificationAvailability
VisualStage
CanRevealAnyaFragments
CannotRestoreAnyaNpc true
```

### 7.4 ManaPostGamePolicy

```text
ManaBloomFrequency
ManaPredictability
ManaRisk
ManaSellabilityPolicy
ManaCropCommonAllowed false
ManaEconomyCapPolicy
```

### 7.5 CavePostGamePolicy

```text
Level101AccessPolicy
NewAreasPolicy
RareResourcePolicy
BlackStoneContainment
BossRepeatPolicy
CommonMiningAt101Allowed false
RewardRepeatPolicy
```

### 7.6 CityMemoryPostGamePolicy

```text
MemoryPreservationLevel
NpcMemoryRecoveryPolicy
PublicLoreDisclosurePolicy
CorvusYaelSethraVaelrionAftermathPolicy
FestivalRumorPolicy
```

### 7.7 EconomyPostGamePolicy

```text
AdvancedResourceAvailability
BromecianTechMarketPolicy
ManaCommodityPolicy
RareMerchantPolicy
AntiArbitragePolicy
```

---

## 8. Ending profiles

### Protect

```text
Fonte alive/stable.
Mana rare/natural.
Cave stable/limited.
City memory preserved.
Bromecian tech reduced.
Economy spiritual/natural, low exploit.
```

### Seal

```text
Fonte limited/sealed.
Mana rare/restricted.
Cave more stable but reduced advanced access.
City memory preserved with loss.
Bromecian tech locked/reduced.
Economy reduced advanced resources.
```

### Use

```text
Fonte used/partial.
Mana more predictable but risky.
Cave may open risky new areas.
City material progress.
Bromecian tech expanded.
Economy expanded with hard caps.
```

---

## 9. Application rules

```text
Apply once by AppliedOnceKey.
If already applied, return AlreadyApplied.
Do not reapply on load.
Do not mutate final choice type.
Do not restore Anya.
Do not create Mana common crop.
Do not unlock 101 common farming.
Do not bypass economy anti-arbitrage.
```

---

## 10. Criteria

```text
Postgame contracts exist.
Profiles for Protect/Seal/Use exist.
Application idempotent.
Fonte/Mana/Cave/City/Economy policies bounded.
No Anya restoration.
No Mana common crop.
No 101 farm loop.
Tests cover three profiles, apply once, reload no reapply, no Anya, no Mana commodity, no 101 common mining and economy caps.
```

# /speckit.plan

## 11. Arquitetura alvo

```text
Assets/_Game/Scripts/MainProgression/PostGame/PostGameWorldState.cs
Assets/_Game/Scripts/MainProgression/PostGame/EndingModifierProfile.cs
Assets/_Game/Scripts/MainProgression/PostGame/FontePostGameModifier.cs
Assets/_Game/Scripts/MainProgression/PostGame/ManaPostGamePolicy.cs
Assets/_Game/Scripts/MainProgression/PostGame/CavePostGamePolicy.cs
Assets/_Game/Scripts/MainProgression/PostGame/CityMemoryPostGamePolicy.cs
Assets/_Game/Scripts/MainProgression/PostGame/EconomyPostGamePolicy.cs
Assets/_Game/Scripts/MainProgression/PostGame/PostGameModifierService.cs
Assets/_Game/Tests/EditMode/MainProgression/PostGameWorldStateModifiersTests.cs
```

Consolidar existentes se houver.

## 12. Arquivos permitidos

```text
Assets/_Game/Scripts/MainProgression/PostGame/**
Assets/_Game/Scripts/Fonte/**
Assets/_Game/Scripts/World/**
Assets/_Game/Scripts/Cave/**
Assets/_Game/Scripts/Economy/**
Assets/_Game/Scripts/Editor/Validation/**
Assets/_Game/Tests/EditMode/MainProgression/**
docs/validation/16_spec_postgame_world_state_modifiers_endings_future_runtime_execution_report.md
```

## 13. Arquivos proibidos

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

## 14. Tasks

- [ ] T001 — Ler fontes.
- [ ] T002 — Auditar final/postgame/Fonte/Mana/cave/economy references.
- [ ] T003 — Consolidar modifier profiles.
- [ ] T004 — Implementar idempotent service/validators.
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

- docs/design/gameplay/save_load/SAVE_LOAD_FULL_STATE_DIRECTION.md
- docs/design/gameplay/cave/CAVE_DESIGN_DIRECTION.md
- docs/design/gameplay/cave/CAVE_MONSTER_ROSTER_DIRECTION.md
- docs/design/gameplay/cave/CAVE_COMBAT_BALANCE_VULNERABILITIES_DIRECTION.md
- docs/design/gameplay/quests/QUEST_OBJECTIVE_EVENT_SYSTEM_DIRECTION.md
- docs/design/gameplay/quests/QUESTS_MAIN_LORE_DIRECTION.md
- docs/design/gameplay/quests/QUESTS_MAIN_PROGRESSION_REFINEMENT_DIRECTION.md
- docs/design/gameplay/ui_ux/UI_UX_FULL_GAMEPLAY_DIRECTION.md
- docs/design/gameplay/ui_ux/UI_UX_MENU_SCREEN_FLOWS_DIRECTION.md
- docs/design/gameplay/world/SEASONS_CALENDAR_WEATHER_LUNAR_DIRECTION.md
- docs/design/gameplay/farm/FARM_DESIGN_DIRECTION_v1.3.md
- docs/design/gameplay/farm/FARM_LAYOUT_SCALE_BUILDINGS_DIRECTION.md
- docs/design/gameplay/city/CITY_DESIGN_DIRECTION_v1.2.md
- docs/design/gameplay/city/CITY_LAYOUT_BUILDINGS_SCHEDULE_DIRECTION.md
- docs/design/gameplay/city/CITY_NPC_ROSTER_SERVICES_DIRECTION_v1.1.md
- docs/design/gameplay/loot_crafting_economy/LOOT_CRAFTING_ECONOMY_DIRECTION.md
- docs/design/gameplay/loot_crafting_economy/ECONOMY_PRICING_STOCK_REFRESH_DIRECTION.md
- docs/design/gameplay/magic/MAGIC_SPELLS_ACTIONS_DIRECTION.md
- docs/design/gameplay/magic/MAGIC_LEARNING_UNLOCKS_SOURCES_DIRECTION.md
- docs/design/gameplay/bestiary/BESTIARY_KNOWLEDGE_DISCOVERY_DIRECTION.md
- docs/design/gameplay/companions/COMPANIONS_DIRECTION.md
- docs/design/gameplay/pets/PETS_DIRECTION.md

### Required interpretation

```text
Esta spec é future/mapped.
Ela deriva principalmente de SAVE_LOAD_FULL_STATE_DIRECTION, CAVE_DESIGN_DIRECTION e QUESTS_MAIN_PROGRESSION_REFINEMENT_DIRECTION.
Ela não reimplementa SaveManager do zero.
Ela não reescreve caverna procedural, boss fights, UI visual final, cenas/prefabs ou economy final.
Ela não cria runtime pet nem romance/social deep.
Quando houver conflito entre save/load, cave, main progression, UI, economy ou world directions, o executor deve parar e registrar CONFLICT.
```

---

## Direction / Refinement Coverage

### Covered from directions

- Final = Proteger / Selar / Usar.
- Proteger: Fonte viva e estável, Mana rara, cidade preserva memória, pós-game espiritual/natural, menor tecnologia bromeciana.
- Selar: corrupção contida, Fonte limitada, caverna estável, recursos avançados reduzidos, tom melancólico.
- Usar: mais tecnologia bromeciana, Mana mais previsível, novas áreas de cave com risco, progresso material e risco de repetir Bromécia.
- Não restaurar Anya completamente e não fazer Mana virar crop comum de dinheiro infinito.
- Nível 101 não deve virar farm normal.

### Deferred / future from directions

- Visual postgame world changes.
- New postgame quests.
- Final balance numbers.
- Scene/prefab changes.
- Save migration.

### Explicitly not redefined here

- FinalChoiceService.
- FonteAnya base functions.
- Cave generation.
- Economy pricing formula.
- City schedules.

---

## 23A. Execution Readiness Matrix

| Área | Pergunta obrigatória | Evidência esperada | Status se faltar |
|---|---|---|---|
| Fonte canônica | Executor leu save/load, cave, main progression, UI e world/economy directions? | Lista no execution report. | PARTIAL |
| Estado real do repo | Sistemas existentes relacionados a postgame world modifiers/endings foram auditados antes de criar novos? | Comandos `rg` e achados. | PARTIAL |
| Não duplicação | Existe sistema equivalente já implementado/parcial? | REUSE/HARDEN/CREATE/DEFER. | BLOCKED se duplicar |
| Save policy | A mudança declara seção, dono, IDs, capture, preserve, restore order, migration e validação? | Checklist no report. | BLOCKED se persistir sem contrato |
| Cave snapshot | A mudança preserva CaveRunSeed, snapshots, boss defeat states, checkpoint e corpse/death state? | Tests/checklist. | PARTIAL |
| Endgame spoiler | Anya, Arquivista, nível 101, final choice e endings não vazam cedo? | Tests/visibility. | PARTIAL |
| Anti-farm | Nível 101/final/endgame não viram farm infinito, reward repeat ou boss repeat. | Tests/checklist. | PARTIAL |
| Pet/social | Nenhum runtime pet, romance ou social deep foi criado. | Checklist explícito. | BLOCKED se violar |
| UI/PlayMode | Há fluxo visual/gameplay integrado? | Cenário final deferido documentado. | BUILD_VALIDATED no máximo se ausente |
| Testes | Lógica determinística nova tem EditMode tests quando praticável? | Test list. | PARTIAL |
| Report | Execution report criado? | `docs/validation/16_spec_postgame_world_state_modifiers_endings_future_runtime_execution_report.md`. | PARTIAL |

---

## 23B. Local Audit Checklist

Antes de alterar qualquer arquivo, Claude Code/Codex deve rodar e registrar no execution report:

```bash
rg -n "PostGameWorldState|EndingModifier|Protect|Seal|Use|ManaPostGame|FontePostGame|CavePostGame|BromecianTech|AppliedOnce" Assets/_Game/Scripts docs/design .specs
rg -n "SaveManager|GameSaveData|SchemaVersion|Migration|CaveSaveData|CaveRunSeed|VisitedLevelSnapshot|Checkpoint|Corpse|Death|Level101|FinalChoice|Ending|PostGame|Archivist|Arquivista|Anya|Fonte|Mana|BlackStone|Pet|Romance|Spouse" Assets/_Game/Scripts docs/design .specs
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
Given o sistema base existe ou foi criado de forma mínima
When o fluxo principal desta spec é executado
Then o comportamento segue save/cave/main progression directions
And usa IDs estáveis
And não persiste referências Unity
And não cria pet/social deep runtime
And não promove estado visual/UI para fonte de verdade.
```

### Scenario 2 — Existing partial implementation

```text
Given já existe implementação parcial no repo
When a execução audita o sistema
Then ela escolhe HARDEN_EXISTING em vez de recriar do zero
And registra divergências do direction
And altera apenas o menor conjunto seguro de arquivos.
```

### Scenario 3 — Anti-spoiler and staging

```text
Given o jogador ainda não desbloqueou gate 100, nível 101, Arquivista, final choice ou ending
When save/load/UI/calendar/quest/cave projection consulta o estado
Then detalhes ocultos permanecem escondidos
And apenas informação descoberta/autorizada aparece.
```

### Scenario 4 — Idempotency / no exploit

```text
Given boss, reward, final choice, ending modifier, cave checkpoint ou death/corpse state já foi aplicado
When reload, retry, scene reload ou reentrada ocorre
Then reward/effect não duplica
And estado terminal não regride sem política explícita.
```

### Scenario 5 — Final human validation deferred

```text
Given o fluxo exige inspeção visual/gameplay de save/load, cave checkpoint, nível 101, cinematic, final choice ou postgame world
When a implementação técnica terminar
Then o report registra cenário final em docs/validation/FINAL_HUMAN_VALIDATION_BY_WAVE.md
And não pede validação humana imediata por spec.
```

---

## 23D. Edge Cases and Failure Modes

A execução deve cobrir ou registrar risco residual para:

- Ending modifier applies twice after reload.
- Final choice changed by postgame service.
- Mana becomes common commodity.
- Level101 becomes common mining/farming loop.
- Anya restored as NPC.
- Use ending breaks economy.
- Seal locks too much with no clarity.
- Protect gives uncapped bonuses.

---

## 23E. Minimum Execution Report Template

```md
# Execution Report — Postgame World State Modifiers Endings Future Runtime

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

## Save/cave/endgame compliance
- Save section owner:
- Stable IDs:
- Capture policy:
- Preserve policy:
- Restore order:
- Migration needed:
- Unity references persisted:
- CaveRunSeed/snapshots preserved:
- Boss/reward idempotency:
- Endgame spoiler safe:
- No pet/social deep runtime:
- UI not source of truth:

## Implementation decision
- REUSE_EXISTING / HARDEN_EXISTING / CREATE_MINIMAL / DEFER
- Justification:

## Files changed
- ...

## Functional evidence
- Happy path:
- Existing partial:
- Anti-spoiler/staging:
- Idempotency/no exploit:
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
3. A implementação alterar save schema sem migration spec.
4. A implementação persistir ScriptableObject, GameObject, Transform, MonoBehaviour, Collider, Rigidbody, Sprite ou UI state como gameplay state.
5. A implementação reescrever SaveManager ou caverna procedural do zero.
6. A implementação rerollar snapshot estável sem política.
7. A implementação duplicar boss reward, final reward, ending modifier ou corpse/cave recovery.
8. A implementação revelar Anya/Arquivista/101/final/ending cedo.
9. A implementação criar pet runtime ou romance/social deep.
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
rg -n "SaveManager|GameSaveData|CaveSaveData|CaveRunSeed|VisitedLevelSnapshot|Level101|FinalChoice|Ending|PostGame|Archivist|Anya|Fonte|Mana" Assets/_Game/Scripts docs/design .specs
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
Quando houver cenário visual/gameplay de save/load, caverna, 101, final choice, cinematic ou postgame, registrar em execution report e vincular a docs/validation/FINAL_HUMAN_VALIDATION_BY_WAVE.md.
```

---

## 26. Testing Quality Gate

- Changed deterministic logic: YES, postgame modifier application/idempotency logic is deterministic.
- Requires EditMode tests: YES for profiles/apply-once/reload/no-Anya/no-Mana/no-101/economy cap tests.
- Requires PlayMode automated or final human scenario: YES, DEFERRED for postgame world visual validation.
- Requires regression test: YES if fixing existing postgame/final modifier bug; otherwise NO.
- Human validation timing: DEFERRED_TO_FINAL_VALIDATION se houver UI/gameplay integrado; caso contrário NOT REQUIRED.
- Minimum validation evidence for ACCEPTED: docs validation PASS; C# build PASS if C# changed; EditMode tests PASS or NOT RUN justified; postgame modifiers apply once and remain bounded.

---

## 27. Definition of Done

```text
Spec executada sem alterar arquivos proibidos.
Contratos/data/runtime implementados apenas dentro do escopo.
Execution report criado em docs/validation/16_spec_postgame_world_state_modifiers_endings_future_runtime_execution_report.md.
Fonte/direction coverage preservado.
Validações obrigatórias PASS ou NOT RUN com motivo, impacto e mitigação.
Sem promoção indevida para ACCEPTED apenas por compile.
```

---

## 28. Anti-regressão

```text
Não reimplementar SaveManager.
Não reimplementar caverna procedural.
Não salvar Unity references.
Não salvar UI state como gameplay state.
Não quebrar CaveRunSeed/snapshots/checkpoints/corpse recovery.
Não duplicar rewards/final modifiers.
Não revelar spoilers endgame cedo.
Não transformar 101/final/postgame em farm infinito.
Não criar pet runtime.
Não criar romance/social deep runtime.
Não pedir human test por spec.
Não executar runtime em massa antes da 01Q ou exceção humana explícita.
Não alterar SPEC_EXECUTION_ORDER.md.
```

---

## 29. Notas para execução posterior

Esta spec é future/mapped. Deve ser executada apenas quando save/load, cave, main progression, UI e endgame contracts estiverem estáveis ou quando houver decisão humana explícita de antecipar este bloco.
