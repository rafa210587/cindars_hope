# SPEC — Companion Advanced Party Equipment Tactical AI Future Runtime

> **Spec ID:** `24_spec_companion_advanced_party_equipment_tactical_ai_future_runtime`  
> **Status:** A implementar / Future mapped  
> **Wave:** WAVE 24 — Remaining Future Closure  
> **Priority:** P2  
> **Type:** Runtime / Future / Closure  
> **Domain:** Companions Advanced / Party / Equipment / Skill Tree / Tactical Commands / AI Squad  
> **Parallelizable:** NO  
> **Parallel group:** WAVE_24_REMAINING_FUTURE_CLOSURE  
> **Can run with:** N/A  
> **Must not run with:** qualquer spec que altere companion baseline, pet runtime, social romance, combat core, enemy AI, equipment backend, save migration or UI prefabs.  
> **Repo lock scope:** `Assets/_Game/Scripts/Companions/Advanced/**`, `Assets/_Game/Scripts/UI/Companions/**`, `Assets/_Game/Tests/EditMode/Companions/**`, `docs/validation/24_spec_companion_advanced_party_equipment_tactical_ai_future_runtime_execution_report.md`  
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
  - future party system;
  - advanced companion UI;
  - companion equipment/skills;
  - tactical command validation.
> **Scope:** fechar o mapeamento futuro de companions avançados sem implementar agora: party múltipla, equipment completo, skill tree própria, comandos táticos, AI squad, hostile city events e permadeath.  
> **Out of scope:** baseline companion jobs/cave helper já especificado, romance/casamento detalhado, pet runtime, enemy AI rewrite, combat core formulas.

---

# /speckit.specify

## 1. Contexto

Companions avançados são futuro explícito. O direction lista como futuro: party com múltiplos companions, romance/casamento com rotinas profundas, companion equipment completo, companion skill tree própria grande, farm defense/invasões, companion permadeath, comandos táticos complexos, AI squad avançada, coop multiplayer e companions em cidade durante eventos hostis.

---

## 2. Problema

Sem spec de fechamento:

```text
party múltipla pode entrar no core cedo;
companion vira segunda build completa;
equipment duplica sistema do jogador;
skill tree de companion vira power creep;
tactical AI substitui jogador;
permadeath quebra narrativa sem consentimento;
farm defense vira jogo novo;
companion em cidade hostil ignora schedule/safety;
pet e companion voltam a se misturar.
```

---

## 3. Objetivo

Criar mapa futuro para:

```text
AdvancedCompanionScopeGate;
CompanionPartySizePolicy;
CompanionEquipmentEligibility;
CompanionSkillTreeScopePolicy;
TacticalCommandComplexityTier;
AdvancedCompanionAiMode;
CompanionPermadeathPolicyFuture;
HostileCityCompanionPresenceRule.
```

---

## 4. Regras de design

```text
Baseline continua 1 companion ativo na caverna.
Party múltipla é futuro explícito e não pode entrar sem rebalance de active combat budget.
Companion equipment não duplica inventário/equipment do jogador.
Companion skill tree grande não substitui skill tree do jogador.
Comandos táticos complexos não viram RTS.
Permadeath é future/opt-in/story-safe, nunca baseline invisível.
```

---

## 5. User stories / engineering stories

```text
Como designer, quero saber quando avançar companions sem quebrar core.
Como combat, quero impedir party power creep.
Como economy/equipment, quero evitar duplicar inventário do jogador.
Como UI, quero comandos táticos simples antes de complexos.
Como narrative, quero permadeath e cidade hostil com opt-in/gates claros.
```

---

## 6. Escopo

Inclui:

```text
scope gates;
party size policy;
equipment eligibility;
skill tree scope;
tactical command tiers;
advanced AI modes;
permadeath policy;
hostile city presence rules;
tests/validators.
```

Não inclui:

```text
implementação real de party múltipla;
enemy AI rewrite;
equipment backend;
skill tree real;
combat formulas;
scene/prefab.
```

## 7. Modelo de domínio

### 7.1 AdvancedCompanionScopeGate

```text
GateId
RequiredBaselineCompanionAccepted
RequiredCombatBudgetRebalance
RequiredUiCommandLayer
RequiredSaveMigrationSpec
RequiredHumanApproval
BlockedUntilExplicitRoadmapUpdate
```

### 7.2 CompanionPartySizePolicy

```text
CaveActiveCompanionsBaseline = 1
CaveActiveCompanionsFutureMax
QuestTemporaryExtraAllowed
FarmWorkersSeparateFromCaveParty
PetSeparateButBudgetAware
RequiresEncounterRebalance
```

### 7.3 CompanionEquipmentEligibility

```text
CompanionId
CanEquipWeapon
CanEquipArmor
CanEquipTrinket
AllowedSlots[]
UsesPlayerInventory false by default
ProtectedFromAutoOptimize
NoFullInventoryClone true
```

### 7.4 CompanionSkillTreeScopePolicy

```text
CompanionId
TreeSizeTier: None | SmallPerkTrack | MediumRoleTrack | LargeFutureOnly
CanUnlockRolePassives
CanUnlockCombatActions
CanExceedPlayerBuildPower false
RequiresStoryQuest
```

### 7.5 TacticalCommandComplexityTier

```text
None
BasicFollowHold
RolePreference
FocusTarget
RetreatAssist
AdvancedSquadFuture
DebugOnly
```

### 7.6 CompanionPermadeathPolicyFuture

```text
DisabledBaseline
StoryOnly
OptInHardcoreFuture
TemporaryInjuryInstead
RequiresConfirmation
RequiresRecoveryAlternative
NoSilentPermanentLoss true
```

---

## 8. Advanced rules

```text
Multiple companions:
  blocked until explicit rebalance.

Companion equipment:
  no full inventory clone.
  no auto-equip hidden optimization.
  no player-equipment duplication.

Skill trees:
  role flavor first, power second.
  no stronger-than-player build.

Tactical commands:
  simple follow/hold/retreat before focus/AI squad.

Permadeath:
  not baseline, not silent.
```

---

## 9. Criteria

```text
Advanced companion gate contracts exist.
Party size policy preserves baseline.
Equipment/skill/tactical/permadeath policies exist.
Tests cover advanced gate blocked, party baseline 1, quest temporary extra gate, equipment no inventory clone, skill no player power exceed, tactical tier gating and permadeath disabled baseline.
```

# /speckit.plan

## 10. Arquitetura alvo

```text
Assets/_Game/Scripts/Companions/Advanced/AdvancedCompanionScopeGate.cs
Assets/_Game/Scripts/Companions/Advanced/CompanionPartySizePolicy.cs
Assets/_Game/Scripts/Companions/Advanced/CompanionEquipmentEligibility.cs
Assets/_Game/Scripts/Companions/Advanced/CompanionSkillTreeScopePolicy.cs
Assets/_Game/Scripts/Companions/Advanced/TacticalCommandComplexityTier.cs
Assets/_Game/Scripts/Companions/Advanced/AdvancedCompanionAiMode.cs
Assets/_Game/Scripts/Companions/Advanced/CompanionPermadeathPolicyFuture.cs
Assets/_Game/Scripts/Companions/Advanced/HostileCityCompanionPresenceRule.cs
Assets/_Game/Tests/EditMode/Companions/CompanionAdvancedPartyEquipmentTacticalAiTests.cs
```

## 11. Arquivos permitidos

```text
Assets/_Game/Scripts/Companions/Advanced/**
Assets/_Game/Scripts/UI/Companions/**
Assets/_Game/Scripts/Editor/Validation/**
Assets/_Game/Tests/EditMode/Companions/**
docs/validation/24_spec_companion_advanced_party_equipment_tactical_ai_future_runtime_execution_report.md
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
- [ ] T002 — Auditar companion advanced/equipment/AI references.
- [ ] T003 — Consolidar policies/contracts.
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

- Companion ajuda, mas não joga pelo jogador.
- Companion não deve tankar boss infinitamente, curar sem limite, substituir build/skill tree, gerar loot infinito, resolver puzzle ou ser obrigatório.
- Futuro explícito inclui party com múltiplos companions, companion equipment completo, skill tree própria grande, farm defense/invasões, permadeath, comandos táticos complexos e AI squad avançada.
- Baseline recomendado: caverna com 1 companion ativo; fazenda pode ter múltiplos visitantes, mas jobs limitados por board/capacidade.
- Pet é sistema separado; não conta como companion e não substitui companion.
- Companion eligibility deve ser dado de NPC, não hardcode por nome.

### Deferred / future from directions

- Party real.
- Equipment backend.
- Companion skill trees.
- Enemy AI squad behavior.
- Farm defense/invasions.
- Permadeath narrative.

### Explicitly not redefined here

- Baseline companion specs.
- Pet specs.
- Social/romance specs.
- Combat formulas.
- Enemy AI.
- Equipment system.

---

## 23A. Execution Readiness Matrix

| Área | Pergunta obrigatória | Evidência esperada | Status se faltar |
|---|---|---|---|
| Fonte canônica | Executor leu todos os directions listados? | Lista no report. | PARTIAL |
| Estado real do repo | Sistemas existentes relacionados a companion advanced party/equipment/tactical AI foram auditados antes de criar novos? | Comandos `rg` e achados. | PARTIAL |
| Não duplicação | Existe sistema equivalente já implementado/parcial? | REUSE/HARDEN/CREATE/DEFER. | BLOCKED se duplicar |
| Scope futuro | A spec foi autorizada para execução agora ou ainda fica future/deferred? | Decisão registrada no report. | BLOCKED/DEFERRED |
| Guardrails | A execução preserva limites de gameplay, economia, UI, save e spoilers? | Checklist/tests. | PARTIAL |
| Anti-exploit | Não gera loot, gold, power, party size, progresso ou automação infinita. | Tests/checklist. | PARTIAL |
| Anti-spoiler | Não revela Anya, Fonte, Level 101, final, boss, secret events ou quest flags cedo. | Tests/checklist. | PARTIAL |
| Save/load | Houve alteração de schema? | Declaração NO ou STOP se migration necessária. | BLOCKED se alterar sem migration |
| UI/PlayMode | Há fluxo visual/gameplay integrado? | Cenário final deferido documentado. | BUILD_VALIDATED no máximo se ausente |
| Testes | Lógica determinística nova tem EditMode tests quando praticável? | Test list. | PARTIAL |
| Report | Execution report criado? | `docs/validation/24_spec_companion_advanced_party_equipment_tactical_ai_future_runtime_execution_report.md`. | PARTIAL |

---

## 23B. Local Audit Checklist

Antes de alterar qualquer arquivo, Claude Code/Codex deve rodar e registrar no execution report:

```bash
rg -n "AdvancedCompanion|PartySize|CompanionEquipment|CompanionSkillTree|TacticalCommand|Permadeath|AISquad|HostileCity" Assets/_Game/Scripts docs/design .specs
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
When o fluxo de companion advanced party/equipment/tactical AI roda
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

- Advanced system bypasses baseline 1 companion.
- Party power creep.
- Companion equipment clones player inventory.
- Companion skill tree stronger than player.
- Tactical AI plays game for player.
- Permadeath silently removes NPC.
- Pet mixed with companion.
- Farm defense becomes mandatory.

---

## 23E. Minimum Execution Report Template

```md
# Execution Report — Companion Advanced Party Equipment Tactical AI Future Runtime

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
rg -n "AdvancedCompanion|PartySize|CompanionEquipment|CompanionSkillTree|TacticalCommand|Permadeath|AISquad|HostileCity" Assets/_Game/Scripts docs/design .specs
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

- Changed deterministic logic: YES, advanced gating/policy validation is deterministic.
- Requires EditMode tests: YES for gate/party/equipment/skill/tactical/permadeath/pet-separation tests.
- Requires PlayMode automated or final human scenario: YES, DEFERRED for advanced companion UI/gameplay validation.
- Requires regression test: YES if fixing existing advanced companion leak; otherwise NO.
- Human validation timing: DEFERRED_TO_FINAL_VALIDATION se houver UI/gameplay integrado; caso contrário NOT REQUIRED.
- Minimum validation evidence for ACCEPTED: docs validation PASS; C# build PASS if C# changed; EditMode tests PASS or NOT RUN justified; advanced companion scope remains gated.

---

## 27. Definition of Done

```text
Spec executada sem alterar arquivos proibidos.
Contratos/data/runtime implementados apenas dentro do escopo.
Execution report criado em docs/validation/24_spec_companion_advanced_party_equipment_tactical_ai_future_runtime_execution_report.md.
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
